namespace DigiTactics.Rules;

/// <summary>Persistent Auto Chess relic hooks and Digi-Egg decisions.</summary>
public sealed class AutoChessPlanning(MatchState state)
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.ToDictionary(l => l.Id);

    public void RoundEnd(RoundEndContext context, int stage)
    {
        if (context.Won || context.Match.Combat is null) return;
        var priest = state.Rules.Traits.Single(t => t.Id == "PRIEST");
        int level = TraitRules.Level(priest, context.Traits.GetValueOrDefault("PRIEST"));
        if (level == 0) return;
        int damage = state.Rules.Loss!.Loss(stage, 1 - context.Team, context.Pve, context.Units.Select(u => u.Survivor));
        context.Player.Health += (int)Math.Floor(damage * priest.Values[level - 1]);
    }

    public IncomeBreakdown Income(MatchPlayer player, RoundPosition round, IncomeBreakdown income)
    {
        foreach (string relic in player.Globals)
            income = relic switch
            {
                "ac_r_treasure" => income with { Augment = income.Augment + 2 },
                "ac_r_vault" => income with { Interest = Math.Min(Math.Max(0, player.Economy.Gold) / 10, 8) },
                "ac_r_charm" => income with { Streak = income.Streak * 2 },
                _ => income
            };
        if (state.Rules.Economy!.IncomeByStage is null)
        {
            var schedule = state.Rules.Schedule!;
            int number = round.Round + (round.Stage == 1 ? 0 : schedule.Opening.Length + (round.Stage - 2) * schedule.Repeating.Length);
            income = income with { Base = Math.Min(number, 5) };
        }
        return income;
    }

    public int ShopLevel(MatchPlayer player, int level)
    {
        foreach (string relic in player.Globals) if (relic == "ac_r_recruiter") level = Math.Min(10, level + 1);
        return level;
    }
    public int TeamSize(MatchPlayer player, int size) => size + player.Globals.Count(id => id == "ac_r_key");
    public void RoundStart(MatchPlayer player) => player.Shop.FreeRolls += player.Globals.Count(id => id == "ac_r_card");
    public void Sold(MatchPlayer player) => player.Economy.Credit(player.Globals.Count(id => id == "ac_r_discount"));

    public PlanningUnit? EggTarget(MatchPlayer player)
    {
        PlanningUnit? best = null; int high = -1;
        foreach (var group in player.Roster.Entries.Select(e => e.Unit).Where(u => u.Stage < 2 && lines.ContainsKey(u.Line)).GroupBy(u => (u.Line, u.Stage)))
        {
            var line = lines[group.Key.Line];
            if (group.Count() != line.Copies - 1 || state.Pool.Remaining(line.Id) < UnitValues.Copies(line, group.Key.Stage)) continue;
            int score = line.Tier * 10 + group.Key.Stage * 25;
            if (score > high) { best = group.First(); high = score; }
        }
        return best;
    }

    public void AiTurn(MatchPlayer player, PlanningItems items, MatchPhase phase)
    {
        for (int guard = 0; guard < 4; guard++)
        {
            int index = player.Items.IndexOf("ac_digiegg");
            if (index < 0 || EggTarget(player) is not { } unit || items.Equip(player, index, unit.Id, phase) != "ac_digiegg") break;
        }
    }
}
