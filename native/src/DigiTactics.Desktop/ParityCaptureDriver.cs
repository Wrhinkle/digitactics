using System.Reflection;
using Brutal.ImGuiApi;
using Brutal.Numerics;
using DigiTactics.Presentation;
using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Desktop;

/// <summary>
/// Diagnostic driver for UI parity captures: reaches one named state through the client's own
/// navigation and visible controls, waits for it to settle, saves a PNG of the framebuffer and exits.
/// It never asserts behavior. Launch: --parity-capture &lt;state&gt; --capture out.png --capture-size WxH.
/// </summary>
internal sealed class ParityCaptureDriver
{
    public static readonly string[] States =
    [
        "home", "play", "party", "lobby",
        "collection-legends", "collection-emotes", "collection-boards", "collection-killscreens", "collection-dances", "collection-soundboard", "collection-elimsounds",
        "store", "loot", "farm", "codex", "profile", "settings", "postgame",
        "match-carousel", "match-planning", "match-combat", "match-augment",
    ];
    private static readonly MethodInfo Navigate = typeof(MatchScreen).GetMethod("NavigateClient", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private readonly string state;
    private readonly ContentCatalog catalog;
    private readonly NativeProfileStore? profile;
    private readonly int settleFrames;
    private readonly double combatSeconds;
    private int frame, cooldown = 30, settled = -1, carouselStep;
    private bool mouseDown, keyDown, ready, postgameReady;
    private double combatElapsed;
    private string phase = "start";
    // combat-sequence:<line>:<stage>:<frames>[:<start seconds>] puts the line on the first combat board and saves
    // one PNG per frame (capture_000.png ...) for animation review.
    private readonly string? sequenceLine;
    private readonly int sequenceStage, sequenceFrames;
    private readonly double sequenceStart;
    private bool sequencePlaced;
    private int sequenceSaved;
    public string Pending => $"parity {state}: {phase}";

    public ParityCaptureDriver(string state, ContentCatalog catalog, NativeProfileStore? profile, int settleFrames, double combatSeconds)
    {
        if (state.StartsWith("combat-sequence:", StringComparison.Ordinal))
        {
            var parts = state.Split(':');
            if (parts.Length is < 4 or > 5) throw new ArgumentException("Use combat-sequence:<line>:<stage>:<frames>[:<start seconds>].");
            sequenceLine = parts[1]; sequenceStage = int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
            sequenceFrames = int.Parse(parts[3], System.Globalization.CultureInfo.InvariantCulture);
            sequenceStart = parts.Length == 5 ? double.Parse(parts[4], System.Globalization.CultureInfo.InvariantCulture) : 0;
            state = "match-combat"; settleFrames = 0; combatSeconds = sequenceStart;
        }
        if (!States.Contains(state) && !state.StartsWith("combat-sequence:", StringComparison.Ordinal)) throw new ArgumentException($"Unknown parity state '{state}'. Choose: {string.Join(", ", States)}");
        if (profile is null && state is "collection-legends" or "collection-emotes" or "collection-boards" or "collection-killscreens" or "collection-dances" or "collection-soundboard" or "collection-elimsounds" or "store" or "loot" or "farm" or "profile" or "postgame")
            throw new ArgumentException($"Parity state '{state}' requires an isolated --profile-directory.");
        this.state = state; this.catalog = catalog; this.profile = profile; this.settleFrames = settleFrames; this.combatSeconds = combatSeconds;
    }

    private void Go(MatchScreen screen, string route) => Navigate.Invoke(screen, [route, true]);
    private static void Park() => ImGui.GetIO().AddMousePosEvent(-float.MaxValue, -float.MaxValue);
    private bool Click(MatchScreen screen, string key)
    {
        if (!screen.Buttons.TryGetValue(key, out var point)) return false;
        var io = ImGui.GetIO(); io.AddMousePosEvent(point.X, point.Y); io.AddMouseButtonEvent(0, true); mouseDown = true; return true;
    }
    private void Press(ImGuiKey key) { ImGui.GetIO().AddKeyEvent(key, true); keyDown = true; }

    public void Input(MatchScreen screen)
    {
        frame++;
        if (mouseDown) { ImGui.GetIO().AddMouseButtonEvent(0, false); mouseDown = false; cooldown = 4; return; }
        if (keyDown) { ImGui.GetIO().AddKeyEvent(ImGuiKey.Space, false); ImGui.GetIO().AddKeyEvent(ImGuiKey.Escape, false); keyDown = false; cooldown = 4; return; }
        if (ready) { Park(); return; }
        if (cooldown-- > 0) return;
        if (screen.MatchLoadingVisible) { phase = "match loading"; return; }
        if (screen.EliminationCinematicActive) { Press(ImGuiKey.Escape); return; }
        switch (state)
        {
            case "home": ready = true; break;
            case "play": Go(screen, "play"); ready = true; break;
            case "party":
                if (!screen.HomePlayVisible) { Go(screen, "play"); cooldown = 6; break; }
                if (!screen.HomePlayPartyVisible) { phase = "confirm mode"; Click(screen, "homePlayConfirm"); break; }
                ready = true; break;
            case "lobby": Go(screen, "multiplayer"); ready = true; break;
            case "store": Go(screen, "store"); ready = true; break;
            case "loot": Go(screen, "loot"); ready = true; break;
            case "farm": Go(screen, "farm"); ready = true; break;
            case "codex": Go(screen, "codex"); ready = true; break;
            case "profile": Go(screen, "profile"); ready = true; break;
            case "settings": Go(screen, "settings"); ready = true; break;
            case "postgame": Postgame(screen); break;
            default:
                if (state.StartsWith("collection-", StringComparison.Ordinal)) { Collection(screen); break; }
                Match(screen); break;
        }
        if (ready) Park();
    }

    private void Collection(MatchScreen screen)
    {
        int tab = Array.IndexOf(["collection-legends", "collection-emotes", "collection-boards", "collection-killscreens", "collection-dances", "collection-soundboard", "collection-elimsounds"], state);
        if (!screen.CollectionVisible || screen.CollectionStoreVisible || screen.CollectionLootVisible) { phase = "open collection"; Go(screen, "collection"); cooldown = 6; return; }
        if (phase != "tab clicked") { phase = "tab clicked"; if (tab == 0 || Click(screen, "collectionTab/" + tab)) cooldown = 6; else phase = "waiting for tab"; return; }
        ready = true;
    }

    private void Postgame(MatchScreen screen)
    {
        // Same deterministic victory fixture as PostgameSmoke scenario 0: a three-unit solo board,
        // every opponent eliminated in one combat, then the normal result route.
        if (!postgameReady)
        {
            var session = new MatchPlanningSession(catalog.Variants[0], 900, Enumerable.Range(0, 8).Select(i => new MatchSeat(i == 0 ? "Result Tamer" : "Opponent " + i, i == 0)).ToArray(), false);
            var player = session.State.Players[0];
            foreach (var old in player.Roster.Entries.ToArray()) player.Roster.Remove(old.Unit.Id, false);
            for (int i = 0; i < 3; i++)
            {
                var unit = session.State.CreateUnit(session.State.Rules.Lines[i].Id, i % 3);
                player.Roster.AddToBench(unit); player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(i % 7, 4), false, () => 9);
            }
            var recorder = new MatchHistoryRecorder(session, profile!); recorder.Capture();
            session.StartCombat();
            foreach (var target in session.State.Players.Where(p => p.Id != 0)) target.Health = -10000;
            session.FinishCombat();
            if (!session.State.Over || !recorder.Complete(DateTimeOffset.Now.ToUnixTimeMilliseconds())) throw new InvalidOperationException("Parity postgame fixture failed to complete.");
            typeof(MatchScreen).GetProperty(nameof(MatchScreen.Session))!.SetValue(screen, session);
            typeof(MatchScreen).GetField("history", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(screen, recorder);
            postgameReady = true; phase = "result shown"; cooldown = 30; return;
        }
        if (screen.PostgameVisible) ready = true;
    }

    private void Match(MatchScreen screen)
    {
        var session = screen.Session;
        if (session is null)
        {
            phase = "launch";
            string launch = screen.Buttons.ContainsKey("homePlayStart") ? "homePlayStart" : screen.Buttons.ContainsKey("homePlayConfirm") ? "homePlayConfirm" : "clientPlay";
            Click(screen, launch); return;
        }
        var player = session.State.Players[0];
        var rounds = session.Rounds;
        bool carousel = rounds.RoundType == "carousel" || session.Units.Carousel.Active;
        phase = $"{rounds.Position.Label} {rounds.RoundType}/{rounds.Phase}";
        if (state == "match-carousel" && carousel) { ready = true; return; }
        if (state == "match-augment" && player.Offer is { Kind: "augment" } && screen.Buttons.ContainsKey("offer/0")) { ready = true; return; }
        if (carousel)
        {
            var options = session.Units.Carousel.Options;
            if (screen.Buttons.ContainsKey("carouselGo")) { Click(screen, "carouselGo"); return; }
            if (session.Units.Carousel.Turn == player && carouselStep++ % 2 == 0)
            {
                int pick = options.Select((option, index) => (option, index)).First(pair => pair.option.PickedBy is null).index;
                Click(screen, "carousel/" + pick);
            }
            return;
        }
        if (player.Offer is { } offer)
        {
            // Non-augment offers (and augments when they are not the target) take the first visible card.
            if (screen.Buttons.ContainsKey("offer/0")) Click(screen, "offer/0");
            return;
        }
        switch (rounds.Phase)
        {
            case MatchPhase.Planning:
                if (state == "match-planning") { ready = true; return; }
                if (sequenceLine is not null && !sequencePlaced) { PlaceSequence(session); sequencePlaced = true; cooldown = 10; return; }
                Press(ImGuiKey.Space); combatElapsed = 0; return;
            case MatchPhase.Combat:
                if (state == "match-combat")
                {
                    combatElapsed += CombatSimulation.Tick;
                    if (combatElapsed >= combatSeconds) ready = true;
                    return;
                }
                Click(screen, "finish"); return;
            case MatchPhase.Result:
                Click(screen, "next"); return;
        }
    }

    private void PlaceSequence(MatchPlanningSession session)
    {
        var state = session.State; var player = state.Players[0];
        var line = state.Rules.Lines.Concat(state.Rules.ExtraLines).FirstOrDefault(l => l.Id == sequenceLine)
            ?? state.Rules.Lines.Concat(state.Rules.ExtraLines).FirstOrDefault(l => l.Id.Contains(sequenceLine!, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"No line '{sequenceLine}' in {state.Rules.Id}.");
        foreach (var old in player.Roster.Entries.ToArray()) player.Roster.Remove(old.Unit.Id, false);
        foreach (int column in new[] { 2, 4 })
        {
            var unit = state.CreateUnit(line.Id, sequenceStage);
            player.Roster.AddToBench(unit); player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(column, 4), false, () => 9);
        }
        phase = $"placed {line.Id} stage {sequenceStage}";
    }

    public bool Verify(MatchScreen screen, int2 size, string? capture)
    {
        if (!ready) return false;
        if (settled < 0) settled = frame;
        if (frame - settled < settleFrames) return false;
        string path = capture ?? throw new ArgumentException("--parity-capture requires --capture <file.png>.");
        if (sequenceLine is not null)
        {
            string numbered = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $"{Path.GetFileNameWithoutExtension(path)}_{sequenceSaved:D3}.png");
            FrameCapture.SavePng(numbered, size.X, size.Y);
            if (++sequenceSaved < sequenceFrames) return false;
            Console.WriteLine($"Parity sequence {sequenceLine}: {sequenceSaved} frames at {size.X}x{size.Y} ({phase})");
            return true;
        }
        if (string.Equals(Path.GetExtension(path), ".bmp", StringComparison.OrdinalIgnoreCase)) FrameCapture.SaveBitmap(path, size.X, size.Y);
        else FrameCapture.SavePng(path, size.X, size.Y);
        Console.WriteLine($"Parity capture {state}: {size.X}x{size.Y} after {frame} frames ({phase}) -> {Path.GetFullPath(path)}");
        return true;
    }

    public string? Timeout(int maxFrames) => frame > maxFrames ? $"Parity capture timed out after {frame} frames: {Pending}" : null;
}
