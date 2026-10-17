namespace DigiTactics.Rules;

public sealed record ResolvedUnit(Survivor Survivor, int? Owner, int Uid, CombatUnit? State = null);
public sealed record ResolvedMatch(int A, int? B, bool Pve, bool Ghost, int Winner,
    IReadOnlyList<ResolvedUnit> Units, IReadOnlyDictionary<string, int> TraitsA, IReadOnlyDictionary<string, int> TraitsB,
    int? TeamA = null, int? TeamB = null, CombatSimulation? Combat = null)
{
    public static ResolvedMatch FromCombat(int a, int? b, bool pve, bool ghost, CombatSimulation combat,
        IReadOnlyDictionary<string, int> traitsA, IReadOnlyDictionary<string, int> traitsB, int? teamA = null, int? teamB = null)
    {
        if (!combat.Done || combat.Winner is null) throw new InvalidOperationException("Combat is not resolved.");
        var units = combat.Units.Select(u => new ResolvedUnit(new(u.Line, u.Stage, u.Team, u.Alive, u.Summoned),
            u.Entry.Owner, u.Entry.Uid, u)).ToArray();
        return new(a, b, pve, ghost, combat.Winner.Value, units, traitsA, traitsB, teamA, teamB, combat);
    }
}
public sealed record RoundEndContext(ResolvedMatch Match, MatchPlayer Player, int Team, int? Owner,
    IReadOnlyList<ResolvedUnit> Units, IReadOnlyDictionary<string, int> Traits)
{
    public bool Won => Match.Winner == Team;
    public bool Pve => Match.Pve && Team == 0;
}
public sealed record PveRewardSummary(int Gold, string? Unit, IReadOnlyList<string> Items, int Orbs);

/// <summary>The match mechanic layer must supply rewards and hooks; settlement never silently discards them.</summary>
public interface IMatchSettlementHooks
{
    PveRewardSummary Reward(MatchPlayer player, bool won);
    int RoundEnd(RoundEndContext context);
    void AfterCombat(MatchPlayer player);
    void Eliminated(MatchPlayer player);
}

public sealed class MatchRoundResult(ResolvedMatch match)
{
    public ResolvedMatch Match { get; } = match;
    public bool AWon => Match.Winner == 0;
    public bool BWon => Match.Winner == 1;
    public int DamageA { get; internal set; }
    public int DamageB { get; internal set; }
    public int TeamDamageA { get; internal set; }
    public int BonusA { get; internal set; }
    public int BonusB { get; internal set; }
    public int BonusA2 { get; internal set; }
    public int BonusB2 { get; internal set; }
    public PveRewardSummary? Reward { get; internal set; }
}

/// <summary>One-shot settlement for a round, preserving hook and health-update order.</summary>
public sealed class MatchSettlement(MatchState state, int stage, IMatchSettlementHooks hooks)
{
    private bool settled;
    public IReadOnlyList<MatchRoundResult> Settle(IReadOnlyList<ResolvedMatch> matches)
    {
        if (settled) throw new InvalidOperationException("This round has already been settled.");
        ArgumentOutOfRangeException.ThrowIfLessThan(stage, 1);
        foreach (var match in matches)
        {
            if ((uint)match.A >= state.Players.Count || match.Winner is < -1 or > 1 ||
                match.B is { } b && (uint)b >= state.Players.Count || !match.Pve && match.B is null ||
                state.Duos && !match.Pve && (match.TeamA is null || match.TeamB is null ||
                    (uint)match.TeamA.Value >= state.Teams.Count || (uint)match.TeamB.Value >= state.Teams.Count))
                throw new ArgumentException("Invalid resolved match.", nameof(matches));
        }
        settled = true;
        var results = matches.Select(m => new MatchRoundResult(m)).ToArray();
        if (state.Duos) SettleDuos(results); else SettleSolo(results);
        foreach (var player in state.Players)
        {
            hooks.AfterCombat(player);
            // The original game changes phase after combining: human boards remain frozen here.
            player.Roster.Combine(player.IsHuman, player.GiveItem);
            player.Shop.ClearMaxed();
        }
        state.Eliminate(hooks.Eliminated);
        return results;
    }

    private int Loss(ResolvedMatch match, int winnerTeam, bool duo = false) => state.Rules.Loss!.Loss(stage,
        winnerTeam, match.Pve, match.Units.Select(u => u.Survivor), duo ? state.Rules.Duo!.UnitScale : null);

    private int RoundEnd(ResolvedMatch match, MatchPlayer player, int team, int? owner = null, bool firstPartner = false)
    {
        var units = owner is null ? match.Units : match.Units.Where(u => u.Survivor.Team != team ||
            u.Owner == owner || u.Owner is null && firstPartner).ToArray();
        int gold = hooks.RoundEnd(new(match, player, team, owner, units, team == 0 ? match.TraitsA : match.TraitsB));
        player.Economy.Credit(gold);
        return gold;
    }

    private void SettleSolo(IEnumerable<MatchRoundResult> results)
    {
        foreach (var result in results)
        {
            var match = result.Match;
            var a = state.Players[match.A];
            var b = match.B is { } id ? state.Players[id] : null;
            if (match.Pve)
            {
                if (!result.AWon) result.DamageA = Loss(match, 1);
                a.Economy.RecordOutcome(result.AWon, true);
                result.Reward = hooks.Reward(a, result.AWon);
            }
            else
            {
                if (!result.AWon) result.DamageA = Loss(match, 1);
                if (!result.BWon) result.DamageB = Loss(match, 0);
                a.Economy.RecordOutcome(result.AWon, false);
                if (!match.Ghost) b!.Economy.RecordOutcome(result.BWon, false);
                a.LastOpponent = b!.Id;
                if (!match.Ghost) b.LastOpponent = a.Id;
            }
            result.BonusA = RoundEnd(match, a, 0);
            if (b is not null && !match.Ghost) result.BonusB = RoundEnd(match, b, 1);
            a.Health -= result.DamageA;
            if (b is not null && !match.Ghost) b.Health -= result.DamageB;
            else result.DamageB = 0;
        }
    }

    private void SettleDuos(IReadOnlyList<MatchRoundResult> results)
    {
        Dictionary<int, int> damage = [];
        void Hit(int team, int amount)
        {
            if (amount > 0) damage[team] = Math.Max(damage.GetValueOrDefault(team), amount);
        }
        foreach (var result in results)
        {
            var match = result.Match;
            if (match.Pve)
            {
                var player = state.Players[match.A];
                if (!result.AWon) result.DamageA = Loss(match, 1);
                player.Economy.RecordOutcome(result.AWon, true);
                result.Reward = hooks.Reward(player, result.AWon);
                result.BonusA = RoundEnd(match, player, 0);
                Hit(player.Team!.Value, result.DamageA);
                continue;
            }
            var a = state.Teams[match.TeamA!.Value];
            var b = state.Teams[match.TeamB!.Value];
            if (!result.AWon) result.DamageA = Loss(match, 1, true);
            if (!result.BWon && !match.Ghost) result.DamageB = Loss(match, 0, true);
            for (int team = 0; team < (match.Ghost ? 1 : 2); team++)
            {
                var own = team == 0 ? a : b;
                var other = team == 0 ? b : a;
                own.LastOpponent = other.Id;
                for (int k = 0; k < own.Members.Count; k++)
                {
                    var player = own.Members[k];
                    player.Economy.RecordOutcome(match.Winner == team, false);
                    player.LastOpponent = other.Members[Math.Min(1 - k, other.Members.Count - 1)].Id;
                    int gold = RoundEnd(match, player, team, player.Id, k == 0);
                    if (team == 0) { if (k == 0) result.BonusA = gold; else result.BonusA2 = gold; }
                    else { if (k == 0) result.BonusB = gold; else result.BonusB2 = gold; }
                }
            }
            Hit(a.Id, result.DamageA);
            if (!match.Ghost) Hit(b.Id, result.DamageB);
        }
        state.SyncTeams();
        var lost = damage.ToDictionary(pair => pair.Key, pair => state.DamageTeam(pair.Key, pair.Value));
        foreach (var result in results)
        {
            var match = result.Match;
            if (match.Pve) result.TeamDamageA = lost.GetValueOrDefault(state.Players[match.A].Team!.Value);
            else
            {
                if (result.DamageA != 0) result.DamageA = lost.GetValueOrDefault(match.TeamA!.Value);
                if (result.DamageB != 0) result.DamageB = lost.GetValueOrDefault(match.TeamB!.Value);
            }
        }
    }
}
