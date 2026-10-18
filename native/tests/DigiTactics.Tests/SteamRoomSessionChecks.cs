using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using DigiTactics.Networking;

internal static class SteamRoomSessionChecks
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void IntFn(int value);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate nint TextFn();
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void InjectFn(ulong peer, byte[] bytes, int size);
    public static int Run(string root)
    {
        int checks = 0;
        void Check(bool ok, string why) { checks++; if (!ok) throw new InvalidOperationException("Steam room: " + why); }
        string path = Path.GetFullPath(Path.Combine(root, "artifacts/steam-native/fake_steam_api.dll"));
        nint module = NativeLibrary.Load(path);
        try
        {
            T Bind<T>(string name) where T : Delegate => Marshal.GetDelegateForFunctionPointer<T>(NativeLibrary.GetExport(module,name));
            var capture = Bind<IntFn>("FakeSteam_CaptureSent"); var pop = Bind<TextFn>("FakeSteam_PopSent");
            var inject = Bind<InjectFn>("FakeSteam_Inject"); var membership = Bind<IntFn>("FakeSteam_Members");
            void Receive(ulong peer, string text) { byte[] bytes = Encoding.UTF8.GetBytes(text); inject(peer,bytes,bytes.Length); }
            capture(1); membership(3);
            using var session = new SteamRoomSession(path,480);
            const ulong peer = 76561190000000002;
            long now = 10000;
            JsonObject Lobby(string host) => new() { ["host"] = host, ["hostName"] = "Host", ["name"] = "Room", ["status"] = "lobby", ["slots"] = new JsonObject { ["0"] = new JsonObject { ["uid"] = host, ["name"] = "Host" } } };
            var created = session.Create(Lobby(session.SteamId.ToString()), SteamLobbyVisibility.Private);
            session.Pump(now); var room = created.GetAwaiter().GetResult(); ulong id = ulong.Parse(room.Authority.RoomId);
            Check(room.Authority.IsHost && room.Authority.Members.Count == 2, "create binds verified members and host");
            Check(session.Matchmaking.Read(id).Data["name"] == "Room", "created room publishes discovery metadata");
            Receive(peer, JsonSerializer.Serialize(new { l = id.ToString(), t = "seat", rid = 1, i = 1, name = "Guest" })); session.Pump(++now);
            Check(room.Authority.Lobby!["slots"]!["1"]!["uid"]!.GetValue<string>() == peer.ToString(), "native incoming packet reaches authoritative seat controller");
            bool reply = false; string wire;
            while ((wire = Marshal.PtrToStringUTF8(pop())!) != "") { using var doc = JsonDocument.Parse(wire); reply |= doc.RootElement.GetProperty("t").GetString() == "seatres"; }
            Check(reply, "room reply travels through native send ABI");
            Receive(76561190000000003, JsonSerializer.Serialize(new { l = id.ToString(), t = "seat", rid = 2, i = 2, name = "Intruder" })); session.Pump(++now);
            Check(room.Authority.Lobby!["slots"]!["2"] is null, "nonmember cannot claim seat through transport");
            Check(room.Ban(peer.ToString()), "ban member"); session.Pump(now += 600);
            Check(room.Authority.Banned(peer.ToString()), "ban remains attached to game room");
            session.Leave();
            created = session.Create(Lobby(session.SteamId.ToString()), SteamLobbyVisibility.Private); session.Pump(++now); room = created.Result;
            Receive(peer, JsonSerializer.Serialize(new { l = id.ToString(), t = "seat", rid = 3, i = 1, name = "Guest" })); session.Pump(++now);
            Check(room.Authority.Lobby!["slots"]!["1"] is not null, "room switch clears prior transport ban");
            membership(1); session.Pump(now += 1001);
            Check(room.Authority.Members.Count == 1, "departed member removed from room authority");
            Receive(peer, JsonSerializer.Serialize(new { l = id.ToString(), t = "pres", v = new { name = "Departed" } })); session.Pump(++now);
            Check(room.Authority.Presence[peer.ToString()] is null, "departed member packets discarded");
            membership(3); session.Pump(now += 1001);
            session.Matchmaking.SetOwner(id,peer); session.Leave();
            var joining = session.Join(id); session.Pump(++now);
            Check(!joining.IsCompleted && session.Room!.Log.Syncing, "join waits for peer snapshot rather than just Steam membership");
            var remote = new NetworkRoom(id.ToString(),peer.ToString(),Lobby(peer.ToString()),now);
            remote.Members(new[] { session.SteamId.ToString(),peer.ToString() },peer.ToString(),now);
            remote.Send += packet => Receive(peer,packet.Text);
            while ((wire = Marshal.PtrToStringUTF8(pop())!) != "") remote.Receive(session.SteamId.ToString(),wire,now);
            session.Pump(++now);
            Check(joining.IsCompletedSuccessfully && joining.Result.Authority.Host == peer.ToString(), "native snapshot joins guest to authenticated host");
            session.Leave();
            joining = session.Join(id); session.Pump(++now); session.Pump(now += 9001);
            Check(joining.IsFaulted && joining.Exception!.InnerException is IOException && session.Room is null, "snapshot timeout faults join and leaves Steam room");
            var canceled = session.Create(Lobby(session.SteamId.ToString()), SteamLobbyVisibility.Private); session.Leave(); session.Pump(++now);
            Check(canceled.IsCanceled && session.Room is null, "late create completion after leave never reopens room");
            created = session.Create(Lobby(session.SteamId.ToString()), SteamLobbyVisibility.Private); session.Pump(++now);
            created.Result.Close(); session.Pump(++now);
            Check(session.Room is null && session.Matchmaking.Read(id).Data["state"] == "over", "closed room is marked over and left");
            created = session.Create(Lobby(session.SteamId.ToString()), SteamLobbyVisibility.Private); session.Pump(++now);
            int lost = 0; session.ConnectionLost += _ => lost++;
            membership(2); session.Pump(now += 1001);
            Check(session.Room is null && lost == 1, "lost local membership tears down connection");
            membership(3); capture(0);
        }
        finally { NativeLibrary.Free(module); }
        Console.WriteLine($"Steam room session: {checks} native transport/authority integration checks passed.");
        return checks;
    }
}
