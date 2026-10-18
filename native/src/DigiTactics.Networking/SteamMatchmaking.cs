using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace DigiTactics.Networking;

public enum SteamLobbyVisibility { Private = 0, Friends = 1, Public = 2 }
public sealed record SteamLobby(ulong Id, IReadOnlyDictionary<string, string> Data, int Members, int Capacity);
public sealed record SteamMemberChange(ulong Lobby, ulong User, ulong Actor, uint Flags);

/// <summary>Steam matchmaking calls completed by SteamClient.PumpCallbacks on the owning thread.
/// The game loop polls returned tasks; subsequent API calls must remain on that thread.</summary>
public sealed class SteamMatchmaking : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint InterfaceFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong CreateFn(nint self, int type, int maximum);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong LobbyCallFn(nint self, ulong lobby);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong ListFn(nint self);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong IndexFn(nint self, int index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong MemberFn(nint self, ulong lobby, int index);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn(nint self, ulong lobby);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void LeaveFn(nint self, ulong lobby);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void FilterFn(nint self, [MarshalAs(UnmanagedType.LPUTF8Str)] string key, [MarshalAs(UnmanagedType.LPUTF8Str)] string value, int comparison);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void FilterIntFn(nint self, int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint DataFn(nint self, ulong lobby, [MarshalAs(UnmanagedType.LPUTF8Str)] string key);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool SetDataFn(nint self, ulong lobby, [MarshalAs(UnmanagedType.LPUTF8Str)] string key, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool OwnerFn(nint self, ulong lobby, ulong owner);
    private readonly SteamClient client;
    private readonly nint api;
    private readonly CreateFn create;
    private readonly LobbyCallFn join, owner;
    private readonly ListFn list;
    private readonly IndexFn byIndex;
    private readonly MemberFn member;
    private readonly CountFn count, capacity;
    private readonly LeaveFn leave;
    private readonly FilterFn filter;
    private readonly FilterIntFn distance, limit;
    private readonly DataFn data;
    private readonly SetDataFn setData;
    private readonly OwnerFn setOwner;
    private sealed record Pending(int Callback, int Size, long Started, Action<SteamCallResult> Complete, Action<Exception> Fail);
    private readonly Dictionary<ulong, Pending> pending = [];
    private readonly HashSet<ulong> joined = [];
    private bool disposed, listing;
    private static readonly string[] Keys = ["dt_game", "dt_ver", "name", "host", "hostName", "setId", "format", "pace", "rounds", "state", "round", "seats"];
    public event Action<SteamMemberChange>? MemberChanged;
    public event Action<ulong, ulong, bool>? DataChanged;
    public event Action<ulong, ulong>? JoinRequested;
    internal SteamMatchmaking(SteamClient client)
    {
        this.client = client;
        api = client.Bind<InterfaceFn>("SteamAPI_SteamMatchmaking_v009")();
        if (api == 0) throw new InvalidOperationException("Steam matchmaking interface is unavailable.");
        create = client.Bind<CreateFn>("SteamAPI_ISteamMatchmaking_CreateLobby"); join = client.Bind<LobbyCallFn>("SteamAPI_ISteamMatchmaking_JoinLobby");
        owner = client.Bind<LobbyCallFn>("SteamAPI_ISteamMatchmaking_GetLobbyOwner"); list = client.Bind<ListFn>("SteamAPI_ISteamMatchmaking_RequestLobbyList");
        byIndex = client.Bind<IndexFn>("SteamAPI_ISteamMatchmaking_GetLobbyByIndex"); member = client.Bind<MemberFn>("SteamAPI_ISteamMatchmaking_GetLobbyMemberByIndex");
        count = client.Bind<CountFn>("SteamAPI_ISteamMatchmaking_GetNumLobbyMembers"); capacity = client.Bind<CountFn>("SteamAPI_ISteamMatchmaking_GetLobbyMemberLimit");
        leave = client.Bind<LeaveFn>("SteamAPI_ISteamMatchmaking_LeaveLobby"); filter = client.Bind<FilterFn>("SteamAPI_ISteamMatchmaking_AddRequestLobbyListStringFilter");
        distance = client.Bind<FilterIntFn>("SteamAPI_ISteamMatchmaking_AddRequestLobbyListDistanceFilter"); limit = client.Bind<FilterIntFn>("SteamAPI_ISteamMatchmaking_AddRequestLobbyListResultCountFilter");
        data = client.Bind<DataFn>("SteamAPI_ISteamMatchmaking_GetLobbyData"); setData = client.Bind<SetDataFn>("SteamAPI_ISteamMatchmaking_SetLobbyData");
        setOwner = client.Bind<OwnerFn>("SteamAPI_ISteamMatchmaking_SetLobbyOwner");
        client.CallResult += Complete; client.Callback += Dispatch;
    }
    private void Verify() { client.VerifyThread(); ObjectDisposedException.ThrowIf(disposed, this); }
    private Task<T> Track<T>(ulong call, int callback, int size, Func<byte[], T> read)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (call == 0) { completion.SetException(new IOException("Steam did not start the matchmaking request.")); return completion.Task; }
        pending.Add(call, new(callback, size, Environment.TickCount64,
            result => { try { completion.TrySetResult(read(result.Data)); } catch (Exception e) { completion.TrySetException(e); } },
            e => completion.TrySetException(e)));
        return completion.Task;
    }
    public Task<ulong> Create(SteamLobbyVisibility visibility = SteamLobbyVisibility.Public, int maximum = 8)
    {
        Verify(); if (!Enum.IsDefined(visibility)) throw new ArgumentOutOfRangeException(nameof(visibility));
        return Track(create(api, (int)visibility, Math.Clamp(maximum, 2, 8)), 513, 16, bytes =>
        {
            int result = BinaryPrimitives.ReadInt32LittleEndian(bytes); ulong id = BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8));
            if (result != 1 || id == 0) throw new IOException($"Steam could not create the lobby ({result}).");
            joined.Add(id); return id;
        });
    }
    public Task<ulong> Join(ulong lobby)
    {
        Verify(); if (lobby == 0) throw new ArgumentOutOfRangeException(nameof(lobby));
        return Track(join(api, lobby), 504, 24, bytes =>
        {
            ulong id = BinaryPrimitives.ReadUInt64LittleEndian(bytes); uint response = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(16));
            if (response != 1 || id != lobby) throw new IOException($"Steam could not join the lobby ({response}).");
            joined.Add(id); return id;
        });
    }
    public Task<IReadOnlyList<SteamLobby>> ListCompatible()
    {
        Verify(); if (listing) throw new InvalidOperationException("A lobby search is already pending.");
        filter(api, "dt_game", "digitactics", 0); filter(api, "dt_ver", NetworkProtocol.Version, 0); distance(api, 3); limit(api, 50);
        listing = true;
        var task = Track<IReadOnlyList<SteamLobby>>(list(api), 510, 4, bytes =>
        {
            uint size = Math.Min(50, BinaryPrimitives.ReadUInt32LittleEndian(bytes)); var rooms = new List<SteamLobby>();
            for (int i = 0; i < size; i++)
            {
                ulong id = byIndex(api, i); if (id == 0) continue; var record = Read(id);
                if (record.Data["dt_game"] == "digitactics" && record.Data["dt_ver"] == NetworkProtocol.Version) rooms.Add(record);
            }
            return rooms;
        });
        // Completion is observed on the game thread, including failures/timeouts.
        listTask = task; if (task.IsCompleted) listing = false; return task;
    }
    private Task? listTask;
    public SteamLobby Read(ulong lobby)
    {
        Verify(); var values = new Dictionary<string, string>();
        foreach (string key in Keys) values[key] = Marshal.PtrToStringUTF8(data(api, lobby, key)) ?? "";
        return new(lobby, values, Math.Clamp(count(api, lobby), 0, 250), Math.Clamp(capacity(api, lobby), 0, 250));
    }
    public ulong[] Members(ulong lobby)
    {
        Verify(); int size = Math.Clamp(count(api, lobby), 0, 250);
        return Enumerable.Range(0, size).Select(i => member(api, lobby, i)).Where(id => id != 0).Distinct().ToArray();
    }
    public ulong Owner(ulong lobby) { Verify(); return owner(api, lobby); }
    public bool SetOwner(ulong lobby, ulong target)
    {
        Verify(); return joined.Contains(lobby) && Owner(lobby) == client.SteamId && Members(lobby).Contains(target) && setOwner(api, lobby, target);
    }
    public bool SetData(ulong lobby, IReadOnlyDictionary<string, string> values)
    {
        Verify(); if (!joined.Contains(lobby) || Owner(lobby) != client.SteamId) return false;
        if (values.Any(p => p.Key.Length is < 1 or > 32 || !p.Key.All(c => char.IsAsciiLetterOrDigit(c) || c == '_') || p.Value.Contains('\0') || Encoding.UTF8.GetByteCount(p.Value) > 4000)) return false;
        bool ok = true; foreach (var field in values) ok &= setData(api, lobby, field.Key, field.Value); return ok;
    }
    public void Leave(ulong lobby) { Verify(); if (joined.Remove(lobby)) leave(api, lobby); }
    private void Complete(SteamCallResult result)
    {
        if (!pending.Remove(result.Call, out var request)) return;
        if (result.Failed) request.Fail(new IOException("Steam matchmaking request failed."));
        else if (result.Id != request.Callback || result.Data.Length != request.Size) request.Fail(new InvalidDataException("Unexpected Steam matchmaking callback layout."));
        else request.Complete(result);
        if (listTask?.IsCompleted == true) listing = false;
    }
    internal void ExpireRequests()
    {
        foreach (var call in pending.Where(p => Environment.TickCount64 - p.Value.Started >= 30000).Select(p => p.Key).ToArray())
        { var request = pending[call]; pending.Remove(call); request.Fail(new TimeoutException("Steam matchmaking request timed out.")); }
        if (listTask?.IsCompleted == true) listing = false;
    }
    private void Dispatch(SteamCallback callback)
    {
        var bytes = callback.Data;
        if (callback.Id == 506 && bytes.Length == 32) MemberChanged?.Invoke(new(BinaryPrimitives.ReadUInt64LittleEndian(bytes), BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8)), BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(16)), BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(24))));
        else if (callback.Id == 505 && bytes.Length == 24) DataChanged?.Invoke(BinaryPrimitives.ReadUInt64LittleEndian(bytes), BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8)), bytes[16] != 0);
        else if (callback.Id == 333 && bytes.Length == 16) JoinRequested?.Invoke(BinaryPrimitives.ReadUInt64LittleEndian(bytes), BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(8)));
    }
    public void Dispose()
    {
        if (disposed) return; client.VerifyThread();
        client.CallResult -= Complete; client.Callback -= Dispatch;
        foreach (var request in pending.Values) request.Fail(new ObjectDisposedException(nameof(SteamMatchmaking)));
        pending.Clear(); foreach (ulong lobby in joined) leave(api, lobby); joined.Clear(); disposed = true;
    }
}
