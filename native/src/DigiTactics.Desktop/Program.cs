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
    var catalog = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content", "catalog.json"));
    int captureIndex = Array.IndexOf(args, "--capture");
    string? capture = captureIndex >= 0 && captureIndex + 1 < args.Length ? args[captureIndex + 1] : null;
    if (args.Contains("--companion-model-smoke")) { CompanionModelSmoke.Run(capture); return 0; }
    if (args.Contains("--farm-room-smoke")) { FarmRoomSmoke.Run(capture); return 0; }
    if (args.Contains("--smoke") || args.Contains("--workbench")) NativeApplication.Run(catalog, args.Contains("--smoke"), capture);
    else
    {
        bool evolutionSmoke = args.Contains("--evolution-smoke");
        bool collectionSmoke = args.Contains("--collection-smoke") || evolutionSmoke;
        bool trainingSmoke = args.Contains("--training-smoke");
        if (trainingSmoke && profileDirectory is null) throw new ArgumentException("Training smoke requires an isolated --profile-directory.");
        if (collectionSmoke && profileDirectory is null) throw new ArgumentException("Collection smoke requires an isolated --profile-directory.");
        MatchApplication.Run(catalog, args.Contains("--match-smoke") || collectionSmoke || trainingSmoke, capture, args.Contains("--duos"), profileDirectory, collectionSmoke, evolutionSmoke, trainingSmoke);
    }
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
