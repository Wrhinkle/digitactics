using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiTactics.Networking;

public sealed record SteamCallback(int Id, byte[] Data);
public sealed record SteamCallResult(ulong Call, int Id, byte[] Data, bool Failed);

/// <summary>Owns Steam initialization and manual callback dispatch on the game thread. See Valve's
/// Steamworks API overview, Manual Callback Dispatch and Initialization/Shutdown sections.</summary>
public sealed class SteamClient : IDisposable
{
    [StructLayout(LayoutKind.Explicit, Size = 24, Pack = 8)]
    private struct CallbackMessage
    {
        [FieldOffset(0)] public int User;
        [FieldOffset(4)] public int Id;
        [FieldOffset(8)] public nint Data;
        [FieldOffset(16)] public int Size;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int InitFn([Out] byte[] error);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void VoidFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int PipeFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint InterfaceFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong IdFn(nint self);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate uint AppFn(nint self);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint NameFn(nint self);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void FrameFn(int pipe);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool NextFn(int pipe, out CallbackMessage callback);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool ResultFn(int pipe, ulong call, [Out] byte[] data, int size, int callback, [MarshalAs(UnmanagedType.I1)] out bool failed);
    private static int active;
    private nint module;
    private readonly int thread = Environment.CurrentManagedThreadId;
    private int pipe;
    private VoidFn? shutdown;
    private FrameFn run = null!, free = null!;
    private NextFn next = null!;
    private ResultFn result = null!;
    private bool initialized, disposed, changedAppEnvironment;
    private string? priorApp, priorGame;
    public SteamMessageTransport Messages { get; private set; } = null!;
    public SteamMatchmaking Matchmaking { get; private set; } = null!;
    public ulong SteamId { get; private set; }
    public uint AppId { get; private set; }
    public string Name { get; private set; } = "";
    public int DroppedCallbacks { get; private set; }
    public event Action<SteamCallback>? Callback;
    public event Action<SteamCallResult>? CallResult;
    internal T Bind<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(module, name));
    public SteamClient(string libraryPath, Func<ulong, bool> isMember, uint? developmentAppId = null)
    {
        ArgumentNullException.ThrowIfNull(isMember);
        if (!OperatingSystem.IsWindows() || nint.Size != 8) throw new PlatformNotSupportedException("Steam client requires Windows x64.");
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0) throw new InvalidOperationException("A Steam client already owns this process.");
        try
        {
            if (developmentAppId is { } id)
            {
                if (id == 0) throw new ArgumentOutOfRangeException(nameof(developmentAppId));
                priorApp = Environment.GetEnvironmentVariable("SteamAppId"); priorGame = Environment.GetEnvironmentVariable("SteamGameId");
                changedAppEnvironment = true;
                Environment.SetEnvironmentVariable("SteamAppId", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Environment.SetEnvironmentVariable("SteamGameId", id.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }
            module = NativeLibrary.Load(Path.GetFullPath(libraryPath)); shutdown = Bind<VoidFn>("SteamAPI_Shutdown");
            var error = new byte[1024]; int status = Bind<InitFn>("SteamAPI_InitFlat")(error);
            if (status != 0)
            {
                int end = System.Array.IndexOf(error, (byte)0); string message = Encoding.UTF8.GetString(error, 0, end < 0 ? error.Length : end);
                throw new InvalidOperationException(message.Length == 0 ? $"Steam initialization failed ({status}). Start Steam and reopen Digi-Tactics." : message);
            }
            initialized = true; Bind<VoidFn>("SteamAPI_ManualDispatch_Init")();
            pipe = Bind<PipeFn>("SteamAPI_GetHSteamPipe")(); if (pipe == 0) throw new InvalidOperationException("Steam callback pipe is unavailable.");
            run = Bind<FrameFn>("SteamAPI_ManualDispatch_RunFrame"); free = Bind<FrameFn>("SteamAPI_ManualDispatch_FreeLastCallback");
            next = Bind<NextFn>("SteamAPI_ManualDispatch_GetNextCallback"); result = Bind<ResultFn>("SteamAPI_ManualDispatch_GetAPICallResult");
            nint user = Bind<InterfaceFn>("SteamAPI_SteamUser_v023")(), friends = Bind<InterfaceFn>("SteamAPI_SteamFriends_v017")(), utils = Bind<InterfaceFn>("SteamAPI_SteamUtils_v010")();
            if (user == 0 || friends == 0 || utils == 0) throw new InvalidOperationException("Steam identity interfaces are unavailable.");
            SteamId = Bind<IdFn>("SteamAPI_ISteamUser_GetSteamID")(user); AppId = Bind<AppFn>("SteamAPI_ISteamUtils_GetAppID")(utils);
            Name = Marshal.PtrToStringUTF8(Bind<NameFn>("SteamAPI_ISteamFriends_GetPersonaName")(friends)) ?? "Tamer";
            if (SteamId == 0 || AppId == 0) throw new InvalidOperationException("Steam returned an invalid player or application identity.");
            Messages = new(module, isMember);
            Matchmaking = new(this);
        }
        catch { Cleanup(); throw; }
    }
    internal void VerifyThread()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (Environment.CurrentManagedThreadId != thread) throw new InvalidOperationException("Steam client must be used on its owning game thread.");
    }
    public int PumpCallbacks()
    {
        VerifyThread(); Matchmaking.ExpireRequests(); run(pipe); int count = 0;
        while (count < 256 && next(pipe, out var callback))
        {
            count++; SteamCallback? copied = null; SteamCallResult? completed = null;
            try
            {
                if (callback.Size < 0 || callback.Size > 1024 * 1024 || callback.Size > 0 && callback.Data == 0) { DroppedCallbacks++; continue; }
                var bytes = new byte[callback.Size]; if (bytes.Length > 0) Marshal.Copy(callback.Data, bytes, 0, bytes.Length);
                if (callback.Id == 703)
                {
                    if (bytes.Length != 16) { DroppedCallbacks++; continue; }
                    ulong call = BinaryPrimitives.ReadUInt64LittleEndian(bytes); int id = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(8)); uint length = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(12));
                    if (length > 1024 * 1024) { DroppedCallbacks++; continue; }
                    var payload = new byte[length]; bool ok = result(pipe, call, payload, payload.Length, id, out bool failed);
                    completed = new(call, id, ok ? payload : [], !ok || failed);
                }
                else copied = new(callback.Id, bytes);
            }
            finally { free(pipe); }
            if (completed is not null) CallResult?.Invoke(completed);
            else if (copied is not null) Callback?.Invoke(copied);
            if (disposed) break;
        }
        return count;
    }
    private void Cleanup()
    {
        if (disposed) return;
        try { try { try { Matchmaking?.Dispose(); } finally { Messages?.Dispose(); } } finally { if (initialized) shutdown?.Invoke(); } }
        finally
        {
            initialized = false; disposed = true;
            if (module != 0) { NativeLibrary.Free(module); module = 0; }
            if (changedAppEnvironment) { Environment.SetEnvironmentVariable("SteamAppId", priorApp); Environment.SetEnvironmentVariable("SteamGameId", priorGame); }
            Interlocked.Exchange(ref active, 0);
        }
    }
    public void Dispose() { if (disposed) return; VerifyThread(); Cleanup(); }
}
