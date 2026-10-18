using DigiTactics.Rules;
using DigiTactics.Storage;

namespace DigiTactics.Presentation;

/// <summary>Captures the last living board before elimination clears it, then records placement once.</summary>
public sealed class MatchHistoryRecorder(MatchPlanningSession session, NativeProfileStore profile, int seat = 0, bool multiplayer = false)
{
    private readonly MatchRewardTracker tracker = new(session, seat);
    public void Observe() { if (pending is null) tracker.Update(); }
    public RewardStats LiveStats { get { Observe(); return rewardStats??tracker.Stats; } }
    private HistoryTrait[] traits = [];
    private HistoryUnit[] units = [];
    private int level;
    private bool saved;
    private HistoryMatch? pending;
    private DateOnly rewardDay;
    private RewardStats? rewardStats;
    private EncounteredPlayer[] encountered = [];
    private string recordedName = "";
    public RewardBreakdown? Award { get; private set; }
    public LegendReward? LegendAward { get; private set; }
    public string? Error { get; private set; }
    public HistoryMatch? Match => pending;
    public long XpGained { get; private set; }
    public bool LeveledUp { get; private set; }
    public bool FreshAward { get; private set; }
    public void Capture()
    {
        if (pending is not null) return;
        tracker.Update();
        var player = session.State.Players[seat];
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
    /// <summary>Freezes the first placement without writing progress. Replays must verify before Complete.</summary>
    public bool PrepareCompletion(long timestamp)
    {
        if (pending is not null) return true;
        tracker.Update();
        var player = session.State.Players[seat];
        if (player.Placement == 0) return false;
        rewardDay = DateOnly.FromDateTime(DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime); rewardStats = tracker.Stats;
        recordedName = player.Name;
        if (multiplayer) encountered = session.State.Players.Where(p => p.IsHuman && p.Id != seat).Select(p => new EncounteredPlayer(p.Name)).ToArray();
        string mode = multiplayer ? "mp" : "solo";
        pending = new($"{mode}:{session.State.Seed}:{player.Id}", timestamp, mode, session.State.Duos ? "duos" : "solo",
            session.State.Rules.Id.Split('/')[0], player.Placement, session.Rounds.Position.Label, Math.Max(level, player.Economy.Level), traits, units,
            System.Text.Json.JsonSerializer.SerializeToNode(rewardStats, new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }));
        return true;
    }
    public bool Complete(long timestamp)
    {
        if (saved) return true;
        if (!PrepareCompletion(timestamp)) return false;
        var result = pending!;
        try
        {
            var rewards = MatchRewards.Calculate(profile.Storage.GetValueOrDefault("dt.shop.v1"), profile.Storage.GetValueOrDefault("dt.cosmetics.v1"),
                result.Key, session.State.Duos, rewardStats!, rewardDay, result.At);
            var changes = new Dictionary<string, string>();
            LegendRewardUpdate? legend = null;
            if (rewards.Fresh)
            {
                changes["dt.shop.v1"] = rewards.Shop; changes["dt.cosmetics.v1"] = rewards.Wallet;
                legend = LegendRewards.Calculate(profile.Storage.GetValueOrDefault("dt.legendevo.v1"), rewards.Wallet, result.Key, rewards.RewardPlace);
                if (legend.Save is not null) changes["dt.legendevo.v1"] = legend.Save;
            }
            string? previous = profile.Storage.GetValueOrDefault("dt.client.v1");
            string updated = MatchHistory.Append(previous, result with { Bits = rewards.Award?.Total });
            var levelBefore = TamerProgress.Read(previous);
            var levelAfter = TamerProgress.Read(updated);
            if (multiplayer && !MatchAlreadyRecorded(previous, result.Key))
                updated = RecentPlayers.Remember(updated, recordedName, encountered, result.At)!;
            if (updated != previous) changes["dt.client.v1"] = updated;
            saved = changes.Count == 0 || profile.UpdateStorage(changes);
            if (saved) { Award = rewards.Award; LegendAward = legend?.Award; XpGained = levelAfter.Total - levelBefore.Total; LeveledUp = levelAfter.Level > levelBefore.Level; FreshAward = rewards.Fresh; }
            Error = saved ? null : profile.Error;
        }
        catch (Exception error) when (error is System.Text.Json.JsonException or InvalidDataException or InvalidOperationException or FormatException or OverflowException)
        { Error = "Match progress could not be saved. Existing progress was preserved: " + error.Message; }
        return saved;
    }
    private static bool MatchAlreadyRecorded(string? saved, string key)
    {
        if (saved is null) return false;
        using var document = System.Text.Json.JsonDocument.Parse(saved);
        return document.RootElement.TryGetProperty("matches", out var matches) && matches.EnumerateArray().Any(m => m.GetProperty("key").GetString() == key);
    }
}
