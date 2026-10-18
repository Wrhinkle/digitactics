using DigiTactics.Desktop;
using DigiTactics.Rules;

try
{
    if (args.Contains("--audio-smoke")) { GameAudio.Smoke(); return 0; }
    var catalog = ContentCatalog.Load(Path.Combine(AppContext.BaseDirectory, "content", "catalog.json"));
    int captureIndex = Array.IndexOf(args, "--capture");
    string? capture = captureIndex >= 0 && captureIndex + 1 < args.Length ? args[captureIndex + 1] : null;
    if (args.Contains("--smoke") || args.Contains("--workbench")) NativeApplication.Run(catalog, args.Contains("--smoke"), capture);
    else MatchApplication.Run(catalog, args.Contains("--match-smoke"), capture, args.Contains("--duos"));
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
