using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Captures the last living board before elimination clears it, then records placement once.</summary>
public sealed class MatchHistoryRecorder(MatchPlanningSession session, NativeProfileStore profile)
{
    private readonly MatchRewardTracker tracker = new(session);
    public void Observe() => tracker.Update();
    private HistoryTrait[] traits = [];
    private HistoryUnit[] units = [];
    private int level;
    private bool saved;
    private HistoryMatch? pending;
    public string? Error { get; private set; }
    public void Capture()
    {
        tracker.Update();
        var player = session.State.Players[0];
        if (!player.Alive || player.Roster.BoardCount == 0) return;
        var rules = session.State.Rules;
        var counts = TraitRules.Count(rules, session.CombatProjection.Board(player), player.Augments.ToArray());
        traits = counts.Select(pair => new HistoryTrait(pair.Key, pair.Value, TraitRules.Level(rules.Traits.Single(t => t.Id == pair.Key), pair.Value)))
            .Where(t => t.Lv > 0).OrderByDescending(t => t.Lv).ThenByDescending(t => t.N).ToArray();
        var lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);
        units = player.Roster.Entries.Where(e => e.Spot.IsBoard).Select(e => e.Unit)
            .OrderByDescending(u => lines[u.Line].Tier + u.Stage * 2).Select(u => new HistoryUnit(u.Line, u.Stage, u.Items.ToArray())).ToArray();
        level = player.Economy.Level;
    }
    public bool Complete(long timestamp)
    {
        tracker.Update();
        var player = session.State.Players[0];
        if (saved) return true;
        if (player.Placement == 0) return false;
        pending ??= new($"solo:{session.State.Seed}:{player.Id}", timestamp, "solo", session.State.Duos ? "duos" : "solo",
            session.State.Rules.Id.Split('/')[0], player.Placement, session.Rounds.Position.Label, Math.Max(level, player.Economy.Level), traits, units,
            System.Text.Json.JsonSerializer.SerializeToNode(tracker.Stats, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
        try
        {
            string? previous = profile.Storage.GetValueOrDefault("dt.client.v1");
            string updated = MatchHistory.Append(previous, pending);
            saved = updated == previous || profile.UpdateStorage(new Dictionary<string, string> { ["dt.client.v1"] = updated });
            Error = saved ? null : profile.Error;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { Error = "Match history could not be saved. Existing progress was preserved: " + error.Message; }
        return saved;
    }
}
