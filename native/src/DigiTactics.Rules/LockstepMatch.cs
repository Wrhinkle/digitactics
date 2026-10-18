using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace DigiTactics.Rules;

public sealed record LockstepResult(bool Applied, MatchActionResult? Action = null);

/// <summary>Deterministic shared-log application and host phase decisions, matching js/lockstep.js.
/// Transport authorization and entry ordering belong to NetworkRoom, outside the rules engine.</summary>
public sealed class LockstepMatch
{
    public const int CarouselTurnMs = 12000, CarouselEndMs = 1500, ResultMs = 3200, CombatGraceMs = 4000, OfflineReleaseMs = 20000;
    public MatchPlanningSession Session { get; }
    public long PhaseAt { get; private set; }
    public long TurnAt { get; private set; }
    public double PlanMs { get; }
    public int Sequence { get; private set; }
    private readonly HashSet<int> votes = [];
    public IReadOnlySet<int> Votes => votes;
    private double? maxCombatMs;
    private static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
    public LockstepMatch(Ruleset rules, uint seed, IReadOnlyList<MatchSeat> seats, double planSeconds, long at = 0, bool duos = false, int[][]? teams = null,bool chaos=false,bool chaosPace=false)
    {
        if (!double.IsFinite(planSeconds) || planSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(planSeconds));
        Session = new(rules, seed, seats, duos, teams,chaos,chaosPace); PlanMs = planSeconds * 1000; PhaseAt = TurnAt = at;
    }
    public LockstepResult Apply(JsonElement entry)
    {
        Sequence++;
        long at = entry.TryGetProperty("at", out var timestamp) ? (long)timestamp.GetDouble() : 0;
        var rounds = Session.Rounds;
        switch (entry.GetProperty("k").GetString())
        {
            case "a":
                MatchActionResult? result;
                string action = entry.GetProperty("a").GetString()!;
                try { result = Session.Actions.Apply((int)entry.GetProperty("s").GetDouble(), action, entry.TryGetProperty("g", out var args) ? args : default); }
                catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException) { result = null; }
                if (Session.State.Duos) Session.State.SyncTeams();
                bool ok = result is not null and not MatchActionResult.Flag { Value: false } and not MatchActionResult.Code { Value: "" };
                if (action == "carousel" && ok) TurnAt = at;
                return new(ok, result);
            case "vote":
                if (rounds.Phase == MatchPhase.Planning) votes.Add((int)entry.GetProperty("s").GetDouble());
                return new(true);
            case "start":
                if (Session.State.Over || rounds.Phase != MatchPhase.Planning || rounds.RoundType == "carousel") return new(false);
                Session.StartCombat(); PhaseAt = at; votes.Clear(); maxCombatMs = null; return new(true);
            case "end":
                if (rounds.Phase != MatchPhase.Combat) return new(false);
                Session.FinishCombat(); PhaseAt = at; return new(true);
            case "next":
                bool carousel = rounds.RoundType == "carousel" && rounds.Phase == MatchPhase.Planning;
                if (Session.State.Over || !(carousel || rounds.Phase == MatchPhase.Result)) return new(false);
                Session.Advance(); PhaseAt = TurnAt = at; votes.Clear(); return new(true);
            default: return new(false);
        }
    }
    private IEnumerable<MatchPlayer> Players(MatchBattle battle)
    {
        if (battle.TeamA is { } a) foreach (var player in Session.State.Teams[a].Members) yield return player;
        else yield return Session.State.Players[battle.A];
        if (battle.Ghost) yield break;
        if (battle.TeamB is { } b) foreach (var player in Session.State.Teams[b].Members) yield return player;
        else if (battle.B is { } seat) yield return Session.State.Players[seat];
    }
    public double MaximumCombatMs()
    {
        if (maxCombatMs is { } cached) return cached;
        double maximum = 0; var rules = Session.State.Rules; var timing = rules.MatchCombat!;
        foreach (var battle in Session.Battles.Where(b => Players(b).Any(p => p.IsHuman)))
        {
            var copy = new CombatSimulation(rules, battle.SideA.Units, battle.SideB.Units, battle.Seed, battle.SideA.Columns, timing.Overtime, timing.Limit, setup: battle.Setup);
            while (!copy.Done) copy.Step(); maximum = Math.Max(maximum, copy.Time * 1000);
        }
        maxCombatMs = maximum; return maximum;
    }
    public JsonObject? Decide(long now, IReadOnlySet<int> online, IReadOnlySet<int> done)
    {
        if (Session.State.Over) return null;
        var humans = Session.State.Players.Where(p => p.Alive && p.IsHuman && online.Contains(p.Id)).ToArray();
        JsonObject Entry(string kind) => new() { ["k"] = kind };
        var rounds = Session.Rounds;
        if (rounds.RoundType == "carousel" && rounds.Phase == MatchPhase.Planning)
        {
            var carousel = Session.Units.Carousel;
            if (!carousel.Active || carousel.Done) return now - TurnAt >= CarouselEndMs ? Entry("next") : null;
            int seat = carousel.Turn!.Id;
            return !online.Contains(seat) || now - TurnAt >= CarouselTurnMs ? new JsonObject { ["k"] = "a", ["s"] = seat, ["a"] = "carousel", ["g"] = new JsonObject() } : null;
        }
        if (rounds.Phase == MatchPhase.Planning) return now >= PhaseAt + PlanMs || humans.Length > 0 && humans.All(p => votes.Contains(p.Id)) ? Entry("start") : null;
        if (rounds.Phase == MatchPhase.Combat)
        {
            bool allDone = humans.Where(p => Session.Battles.Any(b => Players(b).Contains(p))).All(p => done.Contains(p.Id));
            return allDone || now >= PhaseAt + MaximumCombatMs() + CombatGraceMs ? Entry("end") : null;
        }
        return rounds.Phase == MatchPhase.Result && now - PhaseAt >= ResultMs ? Entry("next") : null;
    }
    public string Digest()
    {
        var state = Session.State;
        string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
        var players = state.Players.Select(p =>
        {
            var fields = new List<string> { Number(p.Health), Number(p.Economy.Gold), Number(p.Economy.Level), p.Alive ? "true" : "false", p.IsHuman ? "true" : "false",
                string.Join(',', p.Shop.Slots), string.Join(',', p.Items),
                string.Join(';', p.Roster.Board.Select(e => $"{e.Spot.Column},{e.Spot.Row}:{e.Unit.Line}{Number(e.Unit.Stage)}{string.Join('+',e.Unit.Items)}")),
                string.Join(',', p.Roster.Bench.Select(u => u is null ? "-" : u.Line + Number(u.Stage))), string.Join(',', p.Augments), Number(p.Orbs.Count) };
            if (state.Rules.Economy!.Xp is not null) fields.Add(Number(p.Economy.Xp));
            if (state.Rules.Economy.Streak.Length > 0) fields.Add("s" + Number(p.Economy.Streak));
            if (p.Power is { } power) fields.Add(power.Id + Number(power.Rank));
            if (p.Globals.Count > 0) fields.Add(string.Join(',', p.Globals));
            if (state.Duos) fields.Add($"t{p.Team}p{p.PortalSends}");
            return string.Join('|', fields);
        });
        string pool = JsonSerializer.Serialize(state.Rules.Lines.ToDictionary(l => l.Id, l => state.Pool.Remaining(l.Id)), Json);
        return $"{(state.Duos ? "duos|" : "")}{state.Rules.Pace}|{Session.Rounds.Position.Label}|{Session.Rounds.Phase.ToString().ToLowerInvariant()}|{pool}|{string.Join('#', players)}";
    }
    public string Check()
    {
        uint hash = 0x811c9dc5;
        foreach (char c in Digest()) hash = unchecked((hash ^ c) * 0x01000193);
        return hash.ToString("x8", CultureInfo.InvariantCulture);
    }
}
