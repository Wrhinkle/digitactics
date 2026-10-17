namespace DigiTactics.Rules;

public sealed record UnderlordsPlanningRules(int PerStage, Dictionary<int, double[]> ItemOdds,
    int? GoldCap, int? LossStreakCap, int? StreakReset, bool Knockout);

/// <summary>Underlords persistent Tamer, economy, item-offer and combination hooks.</summary>
public sealed class UnderlordsPlanning(MatchState state)
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly PlanningAi placement = new(state.Rules);
    private PlanningUnit? Tamer(MatchPlayer player) => player.Roster.Entries.Select(e => e.Unit).FirstOrDefault(u => !string.IsNullOrEmpty(lines[u.Line].Tamer));

    public int RoundEnd(RoundEndContext context, int stage)
    {
        var player = context.Player;
        var match = context.Match;
        int? opponentId = context.Team == 0 ? match.B : match.A;
        var opponent = opponentId is { } id ? state.Players[id] : null;
        int gold = 0;
        // The original dispatcher snapshots global listeners before any can remove themselves.
        foreach (string global in player.Globals.ToArray())
        {
            switch (global)
            {
                case "ul_g_silver" when !context.Pve:
                    if (context.Won) player.Mechanics.UnderlordsSilver = Math.Min(3, player.Mechanics.UnderlordsSilver + 1);
                    else if (player.Mechanics.UnderlordsSilver > 0) { player.Mechanics.UnderlordsSilver--; gold += 2; }
                    break;
                case "ul_g_aegis" when !context.Won && match.Combat is not null:
                    int damage = state.Rules.Loss!.Loss(stage, 1 - context.Team, context.Pve, context.Units.Select(u => u.Survivor));
                    if (player.Health - damage <= 0)
                    {
                        player.Health += damage;
                        player.Globals.Remove("ul_g_aegis");
                    }
                    break;
                case "ul_g_vicious" when context.Won && !context.Pve && match.Combat is not null && !(match.Ghost && context.Team == 0) && opponent is not null:
                    opponent.Health -= context.Units.Count(u => u.Survivor.Team == context.Team && u.Survivor.Alive && !u.Survivor.Summoned);
                    break;
            }
        }
        if (!context.Won && !context.Pve) player.Mechanics.UnderlordsFreeRoll = true;
        if (match.Combat is null) return gold;
        foreach (var result in context.Units.Where(u => u.Survivor.Team == context.Team))
        {
            var unit = result.State ?? throw new InvalidOperationException("An Underlords result is missing unit state.");
            if (!unit.Summoned && unit.Traits.Contains("BEAST")) player.Mechanics.UnderlordsBeastKills += unit.Underlords.Kills;
            player.Mechanics.UnderlordsDuel += unit.Underlords.DuelWin;
            gold += checked((int)unit.Underlords.Gold);
        }
        if (!context.Won && !context.Pve && !(match.Ghost && context.Team == 0) && opponent is { Alive: true } &&
            player.Roster.Board.Select(e => e.Unit).FirstOrDefault(u => u.Items.Contains("ul_rapier")) is { } holder)
        {
            var items = holder.Items.ToList();
            items.Remove("ul_rapier");
            player.Roster.UpdateUnit(holder with { Items = items.ToArray() });
            opponent.GiveItem("ul_rapier");
        }
        return gold;
    }

    public IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown income)
    {
        var rules = state.Rules.Underlords!;
        if (player.Economy.Streak < 0 && rules.LossStreakCap is { } cap)
            income = income with { Streak = Math.Min(income.Streak, cap) };
        if (rules.Knockout)
            income = income with { Base = 6 + (int)Math.Floor(((round.Stage - 1) * rules.PerStage + round.Round - 1) * 1.5) };
        return income;
    }

    public void RoundStart(MatchPlayer player, RoundPosition round)
    {
        var rules = state.Rules.Underlords!;
        player.Shop.FreeRolls += 2 * player.Globals.Count(id => id == "ul_g_recruiter");
        if (player.Mechanics.UnderlordsFreeRoll)
        {
            player.Shop.FreeRolls++;
            player.Mechanics.UnderlordsFreeRoll = false;
        }
        if (rules.StreakReset is > 0 && player.Economy.Streak >= rules.StreakReset) player.Economy.ResetStreak();
        if (rules.GoldCap is > 0 && player.Economy.Gold > rules.GoldCap) player.Economy.Spend(player.Economy.Gold - rules.GoldCap.Value);
        if (player.Offer is not { Kind: "itemTier", UnderlordsRolled: false } offer) return;
        player.Offer = offer with { UnderlordsRolled = true };
        int tier = offer.Tier is null or 0 ? 1 : offer.Tier.Value;
        if (rules.ItemOdds.TryGetValue((round.Stage - 1) * rules.PerStage + round.Round, out var odds))
        {
            double roll = state.Random.Next() * 100;
            for (int i = 0; i < odds.Length; i++)
            {
                if (roll < odds[i]) { tier = i + 1; break; }
                roll -= odds[i];
            }
        }
        if (player.Globals.Contains("ul_g_smuggler")) tier = Math.Min(5, tier + 1);
        var pool = state.Rules.Items.Where(i => i.Tier == tier && !i.Component && !i.NoDrop &&
            !(i.Category == "global" && player.Globals.Contains(i.Id))).Select(i => i.Id).ToArray();
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = (int)(state.Random.Next() * (i + 1));
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        if (pool.Length > 0) player.Offer = player.Offer with { Tier = tier,
            Options = pool.Take(player.Globals.Contains("ul_g_riches") ? 4 : 3).ToArray() };
    }

    public void Combined(MatchPlayer player, PlanningUnit unit)
    {
        for (int i = unit.Items.Length - 1; i >= 1; i--) player.GiveItem(unit.Items[i]);
        if (unit.Items.Length > 1) player.Roster.UpdateUnit(unit with { Items = unit.Items.Take(1).ToArray() });
    }

    public void PowerPicked(MatchPlayer player, string id)
    {
        if (!state.Rules.OfferAi!.TryGetValue("power:" + id, out var power) || power.Tamer is null ||
            state.Rules.Tamers is null || !state.Rules.Tamers.TryGetValue(power.Tamer, out var tamer) || Tamer(player) is not null) return;
        var unit = state.CreateUnit(tamer.Line, 0);
        var cell = placement.FreeCell(player, unit, (_, _) => true);
        if (cell is { } spot) player.Roster.PlaceDetachedOnBoard(unit, spot);
        else player.Roster.AddToBench(unit);
    }

    public void RankUp(MatchPlayer player, int rank)
    {
        if (Tamer(player) is { } unit) player.Roster.UpdateUnit(unit with { Stage = Math.Min(2, (int)Math.Floor((rank - 1) / 2.0)) });
    }
}
