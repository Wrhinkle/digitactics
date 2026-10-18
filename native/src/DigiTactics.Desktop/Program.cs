using DigiTactics.Desktop;
using DigiTactics.Rules;
using DigiTactics.Storage;

try
{
    string? Option(string name)
    {
        int index = Array.IndexOf(args, name);
        if (index < 0) return null;
        if (index + 1 >= args.Length || args[index + 1].StartsWith("--", StringComparison.Ordinal)) throw new ArgumentException($"Provide a path after {name}.");
        return Path.GetFullPath(args[index + 1]);
    }
    string? profileDirectory = Option("--profile-directory");
    if (args.Contains("--custom-music-smoke")) { GameAudio.CustomMusicSmoke(Option("--ffmpeg") ?? throw new ArgumentException("Provide --ffmpeg.")); return 0; }
    if (args.Contains("--steam-room-smoke"))
    {
        string library = Option("--steam-api") ?? throw new ArgumentException("Steam smoke requires --steam-api with the Steam API DLL path.");
        using var session = new DigiTactics.Networking.SteamRoomSession(library, 480);
        var created = session.Create(new System.Text.Json.Nodes.JsonObject { ["name"] = "Native private room smoke", ["hostName"] = "Host", ["status"] = "lobby", ["slots"] = new System.Text.Json.Nodes.JsonObject() }, DigiTactics.Networking.SteamLobbyVisibility.Private);
        var timer = System.Diagnostics.Stopwatch.StartNew();
        while (!created.IsCompleted && timer.Elapsed.TotalSeconds < 35) { session.Pump(Environment.TickCount64); Thread.Sleep(10); }
        if (!created.IsCompleted) throw new TimeoutException("Private room smoke timed out.");
        var room = created.GetAwaiter().GetResult();
        room.ClaimSeat(0, session.Name, Environment.TickCount64); session.Pump(Environment.TickCount64);
        ulong id = ulong.Parse(room.Authority.RoomId);
        if (!room.Authority.IsHost || room.Authority.Lobby!["slots"]!["0"] is null || session.Matchmaking.Read(id).Data["name"] != "Native private room smoke") throw new InvalidDataException("Native room membership, seat or metadata did not match.");
        room.Close(); session.Pump(Environment.TickCount64);
        if (session.Room is not null) throw new InvalidOperationException("Closed room is still attached.");
        Console.WriteLine("Steam room smoke passed: private room created, native host/seat and metadata verified, room closed and left.");
        return 0;
    }
    if (args.Contains("--steam-client-smoke") || args.Contains("--steam-lobby-smoke"))
    {
        string library = Option("--steam-api") ?? throw new ArgumentException("Steam smoke requires --steam-api with the Steam API DLL path.");
        using var client = new DigiTactics.Networking.SteamClient(library, _ => false, 480);
        if (args.Contains("--steam-lobby-smoke"))
        {
            var created = client.Matchmaking.Create(DigiTactics.Networking.SteamLobbyVisibility.Private);
            var deadline = System.Diagnostics.Stopwatch.StartNew();
            while (!created.IsCompleted && deadline.Elapsed.TotalSeconds < 35) { client.PumpCallbacks(); Thread.Sleep(10); }
            if (!created.IsCompleted) throw new TimeoutException("Private lobby smoke timed out.");
            ulong lobby = created.GetAwaiter().GetResult();
            if (client.Matchmaking.Owner(lobby) != client.SteamId || !client.Matchmaking.Members(lobby).Contains(client.SteamId)) throw new InvalidDataException("Private lobby membership did not match its creator.");
            if (!client.Matchmaking.SetData(lobby, new Dictionary<string,string> { ["dt_game"] = "digitactics", ["dt_ver"] = DigiTactics.Networking.NetworkProtocol.Version, ["name"] = "Native private smoke" })) throw new IOException("Private lobby metadata write failed.");
            if (client.Matchmaking.Read(lobby).Data["name"] != "Native private smoke") throw new InvalidDataException("Private lobby metadata read failed.");
            client.Matchmaking.Leave(lobby);
            Console.WriteLine("Steam lobby smoke passed: private lobby created, ownership/membership and metadata verified, lobby left.");
            return 0;
        }
        var timer = System.Diagnostics.Stopwatch.StartNew(); int callbacks = 0;
        while (timer.Elapsed.TotalSeconds < 2) { callbacks += client.PumpCallbacks(); Thread.Sleep(10); }
        Console.WriteLine($"Steam client smoke passed: app {client.AppId}, player identity available, relay-only messages initialized, {callbacks} callbacks dispatched.");
        return 0;
    }
    if (Option("--import-profile") is { } importFile)
    {
        var profile = new NativeProfileStore(profileDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native"));
        profile.Import(new ProfileImport(importFile));
        Console.WriteLine("Profile imported; original backup preserved. Identity and saved totals are available; other profile features remain in development.");
        return 0;
    }
    int profileIndex = Array.IndexOf(args, "--stage-profile");
    if (profileIndex >= 0)
    {
        if (profileIndex + 1 >= args.Length) throw new ArgumentException("Provide the exported profile backup file.");
        var staged = LegacyProfileArchive.Stage(args[profileIndex + 1], Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DigiTactics", "native", "imports"));
        Console.WriteLine($"Preserved backup: {staged.Path}\n{staged.Summary.StorageEntries} saved entries, {staged.Summary.MusicTracks} music files. Player data has not been applied.");
        return 0;
    }
    if (args.Contains("--audio-smoke")) { GameAudio.Smoke(); return 0; }
    if (args.Contains("--audio-settings-smoke")) { AudioSettingsSmoke.Run(); return 0; }
    var catalog = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content", "catalog.json"));
    int captureIndex = Array.IndexOf(args, "--capture");
    string? capture = captureIndex >= 0 && captureIndex + 1 < args.Length ? args[captureIndex + 1] : null;
    if (args.Contains("--board-mesh-smoke")) { BoardMeshSmoke.Run(capture); return 0; }
    if (args.Contains("--companion-model-smoke")) { CompanionModelSmoke.Run(capture); return 0; }
    if (args.Contains("--farm-room-smoke")) { FarmRoomSmoke.Run(capture); return 0; }
    if (args.Contains("--smoke") || args.Contains("--workbench")) NativeApplication.Run(catalog, args.Contains("--smoke"), capture);
    else
    {
        bool evolutionSmoke = args.Contains("--evolution-smoke");
        bool collectionSmoke = args.Contains("--collection-smoke") || evolutionSmoke;
        bool trainingSmoke = args.Contains("--training-smoke");
        bool farmPointerSmoke = args.Contains("--farm-pointer-smoke");
        if (farmPointerSmoke && profileDirectory is null) throw new ArgumentException("Farm pointer smoke requires an isolated --profile-directory.");
        if (trainingSmoke && profileDirectory is null) throw new ArgumentException("Training smoke requires an isolated --profile-directory.");
        if (collectionSmoke && profileDirectory is null) throw new ArgumentException("Collection smoke requires an isolated --profile-directory.");
        bool spectatorSmoke = args.Contains("--spectator-smoke");
        bool networkMatchSmoke = args.Contains("--network-match-smoke") || spectatorSmoke;
        bool audioVisibilitySmoke = args.Contains("--audio-visibility-smoke");
        bool modeBonusSmoke = args.Contains("--mode-bonus-smoke");
        bool steamLobbyUiSmoke = args.Contains("--steam-lobby-ui-smoke");
        MatchApplication.Run(catalog, args.Contains("--match-smoke") || collectionSmoke || trainingSmoke || farmPointerSmoke || networkMatchSmoke || steamLobbyUiSmoke || modeBonusSmoke || audioVisibilitySmoke, capture, args.Contains("--duos"), profileDirectory, collectionSmoke, evolutionSmoke, trainingSmoke, farmPointerSmoke, networkMatchSmoke, Option("--steam-api"), steamLobbyUiSmoke, spectatorSmoke, modeBonusSmoke, audioVisibilitySmoke);
    }
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
