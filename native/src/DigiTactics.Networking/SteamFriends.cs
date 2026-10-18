using System.Runtime.InteropServices;

namespace DigiTactics.Networking;

public sealed record SteamFriend(ulong Id, string Name);
public sealed record SteamFriendRoom(SteamLobby Lobby, IReadOnlyList<SteamFriend> Friends);
public sealed record SteamAvatar(int Handle, int Width, int Height, byte[] Rgba);

/// <summary>Read-only friend room discovery. Results complete on the Steam client's game-thread pump.</summary>
public sealed partial class SteamFriends : IDisposable
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
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int AvatarFn(nint self, ulong user);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool UserFn(nint self, ulong user, [MarshalAs(UnmanagedType.I1)] bool nameOnly);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool SizeFn(nint self, int image, out uint width, out uint height);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool PixelsFn(nint self, int image, [Out] byte[] bytes, int size);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void InviteOverlayFn(nint self, ulong lobby);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] [return: MarshalAs(UnmanagedType.I1)] private delegate bool OverlayEnabledFn(nint self);
    private readonly SteamClient client;
    private readonly nint friends, matchmaking;
    private readonly CountFn count;
    private readonly FriendFn byIndex;
    private readonly GameFn game;
    private readonly NameFn name;
    private readonly DataFn request;
    private readonly AvatarFn avatar;
    private readonly UserFn user;
    private readonly SizeFn imageSize;
    private readonly PixelsFn pixels;
    private readonly InviteOverlayFn inviteOverlay;
    private readonly OverlayEnabledFn overlayEnabled;
    private readonly nint utils;
    private readonly HashSet<ulong> requestedPeople = [];
    private readonly Dictionary<int, SteamAvatar> avatars = [];
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
        utils = client.Bind<InterfaceFn>("SteamAPI_SteamUtils_v010")();
        avatar = client.Bind<AvatarFn>("SteamAPI_ISteamFriends_GetMediumFriendAvatar");
        user = client.Bind<UserFn>("SteamAPI_ISteamFriends_RequestUserInformation");
        imageSize = client.Bind<SizeFn>("SteamAPI_ISteamUtils_GetImageSize");
        pixels = client.Bind<PixelsFn>("SteamAPI_ISteamUtils_GetImageRGBA");
        inviteOverlay = client.Bind<InviteOverlayFn>("SteamAPI_ISteamFriends_ActivateGameOverlayInviteDialog");
        overlayEnabled = client.Bind<OverlayEnabledFn>("SteamAPI_ISteamUtils_IsOverlayEnabled");
        client.Matchmaking.DataChanged += DataChanged;
    }
    private void Verify() { client.VerifyThread(); ObjectDisposedException.ThrowIf(disposed, this); }
    /// <summary>Opens Steam's friend-selection dialog. This method sends no invitations itself.</summary>
    public bool OpenInviteOverlay(ulong lobby)
    {
        Verify();if(lobby==0)throw new ArgumentOutOfRangeException(nameof(lobby));
        if(!overlayEnabled(utils))return false;
        inviteOverlay(friends,lobby);return true;
    }
    public IReadOnlyList<SteamFriend> ListFriends()
    {
        Verify();var result=new List<SteamFriend>();var seen=new HashSet<ulong>();
        for(int i=0,size=Math.Clamp(count(friends,4),0,2000);i<size;i++)
        {
            ulong id=byIndex(friends,i,4);
            if(id!=0 && seen.Add(id))result.Add(new(id,Name(id)));
        }
        return result.AsReadOnly();
    }
    public string Name(ulong id) { Verify(); return id == 0 ? "" : Marshal.PtrToStringUTF8(name(friends, id)) ?? ""; }
    public SteamAvatar? Avatar(ulong id)
    {
        Verify(); if (id == 0) return null;
        if (id != client.SteamId && !requestedPeople.Contains(id))
        {
            if (requestedPeople.Count >= 256) requestedPeople.Clear();
            requestedPeople.Add(id); user(friends, id, false);
        }
        int handle = avatar(friends, id);
        if (handle <= 0) return null; // -1 is still downloading; callers retry on a later frame.
        if (avatars.TryGetValue(handle, out var cached)) return cached;
        if (!imageSize(utils, handle, out uint width, out uint height) || width is 0 or > 256 || height is 0 or > 256) return null;
        byte[] rgba = new byte[checked((int)(width * height * 4))];
        if (!pixels(utils, handle, rgba, rgba.Length)) return null;
        if (avatars.Count >= 64) avatars.Remove(avatars.Keys.First());
        var result = new SteamAvatar(handle, (int)width, (int)height, rgba); avatars.Add(handle, result); return result;
    }
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
        pending?.TrySetException(new ObjectDisposedException(nameof(SteamFriends))); pending = null; found.Clear(); finished.Clear(); avatars.Clear(); requestedPeople.Clear(); disposed = true;
    }
}
