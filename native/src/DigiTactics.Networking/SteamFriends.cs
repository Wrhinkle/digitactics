using System.Runtime.InteropServices;

namespace DigiTactics.Networking;

public sealed record SteamFriend(ulong Id, string Name);
public sealed record SteamFriendRoom(SteamLobby Lobby, IReadOnlyList<SteamFriend> Friends);

/// <summary>Read-only friend room discovery. Results complete on the Steam client's game-thread pump.</summary>
public sealed class SteamFriends : IDisposable
{
    [StructLayout(LayoutKind.Explicit, Size = 24)] private struct GameInfo
    {
        [FieldOffset(0)] public ulong Game;
        [FieldOffset(16)] public ulong Lobby;
    }
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint InterfaceFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int CountFn(nint self, int flags);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate ulong FriendFn(nint self, int index, int flags);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool GameFn(nint self, ulong id, out GameInfo info);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint NameFn(nint self, ulong id);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool DataFn(nint self, ulong lobby);
    private readonly SteamClient client;
    private readonly nint friends, matchmaking;
    private readonly CountFn count;
    private readonly FriendFn byIndex;
    private readonly GameFn game;
    private readonly NameFn name;
    private readonly DataFn request;
    private readonly Dictionary<ulong, List<SteamFriend>> found = [];
    private readonly Dictionary<ulong, bool> finished = [];
    private TaskCompletionSource<IReadOnlyList<SteamFriendRoom>>? pending;
    private long started;
    private bool disposed;
    internal SteamFriends(SteamClient client)
    {
        this.client = client;
        friends = client.Bind<InterfaceFn>("SteamAPI_SteamFriends_v017")();
        matchmaking = client.Bind<InterfaceFn>("SteamAPI_SteamMatchmaking_v009")();
        count = client.Bind<CountFn>("SteamAPI_ISteamFriends_GetFriendCount");
        byIndex = client.Bind<FriendFn>("SteamAPI_ISteamFriends_GetFriendByIndex");
        game = client.Bind<GameFn>("SteamAPI_ISteamFriends_GetFriendGamePlayed");
        name = client.Bind<NameFn>("SteamAPI_ISteamFriends_GetFriendPersonaName");
        request = client.Bind<DataFn>("SteamAPI_ISteamMatchmaking_RequestLobbyData");
        client.Matchmaking.DataChanged += DataChanged;
    }
    private void Verify() { client.VerifyThread(); ObjectDisposedException.ThrowIf(disposed, this); }
    public string Name(ulong id) { Verify(); return id == 0 ? "" : Marshal.PtrToStringUTF8(name(friends, id)) ?? ""; }
    public Task<IReadOnlyList<SteamFriendRoom>> ListRooms()
    {
        Verify(); if (pending is not null) return pending.Task;
        found.Clear(); finished.Clear();
        var seen = new HashSet<ulong>();
        for (int i = 0, size = Math.Clamp(count(friends, 4), 0, 2000); i < size; i++)
        {
            ulong id = byIndex(friends, i, 4);
            if (id == 0 || !seen.Add(id) || !game(friends, id, out var info) || (info.Game & 0xffffff) != client.AppId || info.Lobby == 0) continue;
            if (!found.TryGetValue(info.Lobby, out var people)) found[info.Lobby] = people = [];
            people.Add(new(id, Name(id)));
        }
        pending = new(TaskCreationOptions.RunContinuationsAsynchronously); var task = pending.Task; started = Environment.TickCount64;
        foreach (ulong lobby in found.Keys) if (!request(matchmaking, lobby)) finished[lobby] = false;
        Pump(); return task;
    }
    private void DataChanged(ulong lobby, ulong member, bool success)
    {
        if (pending is not null && member == lobby && found.ContainsKey(lobby)) finished[lobby] = success;
    }
    internal void Pump()
    {
        if (disposed || pending is null || finished.Count < found.Count && Environment.TickCount64 - started < 1500) return;
        var result = new List<SteamFriendRoom>();
        foreach (var pair in found)
        {
            if (!finished.GetValueOrDefault(pair.Key)) continue;
            var lobby = client.Matchmaking.Read(pair.Key);
            if (lobby.Data["dt_game"] == "digitactics" && lobby.Data["dt_ver"] == NetworkProtocol.Version)
                result.Add(new(lobby, pair.Value.AsReadOnly()));
        }
        var completion = pending; pending = null; found.Clear(); finished.Clear(); completion.TrySetResult(result);
    }
    public void Dispose()
    {
        if (disposed) return; client.VerifyThread(); client.Matchmaking.DataChanged -= DataChanged;
        pending?.TrySetException(new ObjectDisposedException(nameof(SteamFriends))); pending = null; found.Clear(); finished.Clear(); disposed = true;
    }
}
