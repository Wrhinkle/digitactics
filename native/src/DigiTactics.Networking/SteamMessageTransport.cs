using System.Runtime.InteropServices;
using System.Text;

namespace DigiTactics.Networking;

/// <summary>Windows x64 Steam Networking Messages binding. The owning Steam client must initialize the supplied
/// API module first, keep it loaded, and dispose this transport before SteamAPI_Shutdown.</summary>
public sealed class SteamMessageTransport : IDisposable
{
    private const int MaximumMessage = 512 * 1024;
    [StructLayout(LayoutKind.Explicit, Size = 136, Pack = 1)]
    private struct Identity
    {
        [FieldOffset(0)] public int Type;
        [FieldOffset(4)] public int Size;
        [FieldOffset(8)] public ulong SteamId;
    }
    [StructLayout(LayoutKind.Explicit, Size = 216, Pack = 8)]
    private struct Message
    {
        [FieldOffset(0)] public nint Data;
        [FieldOffset(8)] public int Size;
        [FieldOffset(16)] public Identity Peer;
        [FieldOffset(184)] public nint Release;
        [FieldOffset(192)] public int Channel;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint InterfaceFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int SendFn(nint self, ref Identity peer, [In] byte[] data, uint size, int flags, int channel);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int ReceiveFn(nint self, int channel, [Out] nint[] messages, int maximum);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool SessionFn(nint self, ref Identity peer);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool ConfigureFn(nint self, int key, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ReleaseFn(nint message);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void RelayFn(nint self);
    private readonly nint messages;
    private readonly SendFn send;
    private readonly ReceiveFn receive;
    private readonly SessionFn accept, close;
    private readonly ReleaseFn release;
    private readonly Func<ulong, bool> isMember;
    private readonly HashSet<ulong> blocked = [], sessions = [];
    private readonly Dictionary<ulong, (long At, double Count, double Bytes)> traffic = [];
    private readonly nint[] box = new nint[1];
    private static readonly UTF8Encoding Utf8 = new(false, true);
    private bool disposed;
    public long Dropped { get; private set; }
    public SteamMessageTransport(nint initializedApi, Func<ulong, bool> isMember)
    {
        if (!OperatingSystem.IsWindows() || nint.Size != 8) throw new PlatformNotSupportedException("Steam transport requires Windows x64.");
        if (initializedApi == 0) throw new ArgumentException("Steam API module is not loaded.", nameof(initializedApi));
        this.isMember = isMember;
        T Bind<T>(string export) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(initializedApi, export));
        messages = Bind<InterfaceFn>("SteamAPI_SteamNetworkingMessages_SteamAPI_v002")();
        nint utils = Bind<InterfaceFn>("SteamAPI_SteamNetworkingUtils_SteamAPI_v004")();
        if (messages == 0 || utils == 0) throw new InvalidOperationException("Steam networking interfaces are unavailable.");
        send = Bind<SendFn>("SteamAPI_ISteamNetworkingMessages_SendMessageToUser");
        receive = Bind<ReceiveFn>("SteamAPI_ISteamNetworkingMessages_ReceiveMessagesOnChannel");
        accept = Bind<SessionFn>("SteamAPI_ISteamNetworkingMessages_AcceptSessionWithUser");
        close = Bind<SessionFn>("SteamAPI_ISteamNetworkingMessages_CloseSessionWithUser");
        release = Bind<ReleaseFn>("SteamAPI_SteamNetworkingMessage_t_Release");
        // Original client requires relay routes and disables direct ICE address sharing before any session.
        if (!Bind<ConfigureFn>("SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32")(utils, 104, 0))
            throw new InvalidOperationException("Steam could not enable private relay networking.");
        Bind<RelayFn>("SteamAPI_ISteamNetworkingUtils_InitRelayNetworkAccess")(utils);
    }
    private static Identity Peer(ulong id) => new() { Type = 16, Size = 8, SteamId = id };
    private bool Allowed(ulong id) => id != 0 && !blocked.Contains(id) && isMember(id);
    public bool Accept(ulong id)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Allowed(id)) return false;
        var peer = Peer(id); sessions.Add(id); return accept(messages, ref peer);
    }
    public void Block(ulong id)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        blocked.Add(id); traffic.Remove(id); sessions.Remove(id); var peer = Peer(id); close(messages, ref peer);
    }
    public void ClosePeer(ulong id)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        traffic.Remove(id); sessions.Remove(id); var peer = Peer(id); close(messages, ref peer);
    }
    public void ResetRoom()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        foreach (ulong id in sessions.ToArray()) ClosePeer(id);
        blocked.Clear(); traffic.Clear();
    }
    public bool Send(ulong recipient, string text, bool reliable)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Allowed(recipient) || text.Length == 0 || text.Length > MaximumMessage) return false;
        byte[] data;
        try { data = Utf8.GetBytes(text); } catch (EncoderFallbackException) { return false; }
        if (data.Length > MaximumMessage) return false;
        var peer = Peer(recipient); sessions.Add(recipient);
        return send(messages, ref peer, data, (uint)data.Length, (reliable ? 9 : 1) | 32, 0) == 1;
    }
    private bool Budget(ulong id, int bytes, long now)
    {
        if (traffic.Count >= 128 && !traffic.ContainsKey(id)) return false;
        var b = traffic.GetValueOrDefault(id, (At: now, Count: 300d, Bytes: 8d * 1024 * 1024));
        double elapsed = Math.Max(0, now - b.At) / 1000d;
        double count = Math.Min(300, b.Count + elapsed * 100), available = Math.Min(8 * 1024 * 1024, b.Bytes + elapsed * 1024 * 1024);
        bool allowed = count >= 1 && available >= bytes;
        traffic[id] = (now, allowed ? count - 1 : count, allowed ? available - bytes : available); return allowed;
    }
    public int Poll(long now, Action<ulong, string> deliver)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var delivered = new List<(ulong Peer, string Text)>();
        for (int i = 0; i < 64; i++)
        {
            box[0] = 0;
            int count = receive(messages, 0, box, 1);
            if (count <= 0 || box[0] == 0) break;
            try
            {
                var message = Marshal.PtrToStructure<Message>(box[0]);
                if (message.Peer.Type != 16 || message.Peer.Size != 8 || message.Channel != 0 || message.Data == 0 || message.Size <= 0 || message.Size > MaximumMessage || !Allowed(message.Peer.SteamId) || !Budget(message.Peer.SteamId, message.Size, now)) { Dropped++; continue; }
                var bytes = new byte[message.Size]; Marshal.Copy(message.Data, bytes, 0, bytes.Length);
                try { delivered.Add((message.Peer.SteamId, Utf8.GetString(bytes))); } catch (DecoderFallbackException) { Dropped++; }
            }
            finally { release(box[0]); }
        }
        // All native memory has been released before application callbacks can throw or change room membership.
        int total = 0;
        foreach (var message in delivered) if (Allowed(message.Peer)) { deliver(message.Peer, message.Text); total++; }
        foreach (var id in traffic.Keys.Where(id => !isMember(id)).ToArray()) traffic.Remove(id);
        return total;
    }
    public void Dispose()
    {
        if (disposed) return;
        foreach (ulong id in sessions) { var peer = Peer(id); close(messages, ref peer); }
        sessions.Clear(); traffic.Clear(); disposed = true;
    }
}
