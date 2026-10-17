namespace DigiTactics.Rules;

public sealed record ReckoningPlanningRules(int BlessAt, int BlessGold);
public sealed record ReckoningEgg(int Size, int Left, bool Golden);
public sealed class ReckoningPlanningState
{
    public bool Blessed { get; set; }
    public bool EarlyBlessing { get; set; }
    public List<ReckoningEgg> Eggs { get; } = [];
}

/// <summary>Reckoning's persistent Dragon Kin eggs, Radiant Blessing and augment rewards.</summary>
public sealed class ReckoningPlanning(MatchState state)
{
    private readonly MatchLoot loot = new(state);
    private readonly ModeOfferRewards offers = new(state);

    public void AiTurn(MatchPlayer player, PlanningItems equipment, int stage)
    {
        var rules = state.Rules;
        var items = rules.Items.ToDictionary(i => i.Id);
        var traits = rules.Traits.ToDictionary(t => t.Id);
        var counts = TraitRules.Count(rules, player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line,
            e.Unit.Stage, 0, 4, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits)), player.Augments.ToArray());
        var shadows = rules.Loot!.Mode!.ShadowComponents;
        for (int guard = 0; guard < 4 && player.Items.Contains("rec_egg"); guard++)
        {
            bool crafted = false;
            foreach (string component in shadows)
            {
                if (!player.Items.Contains(component) || equipment.Combine("rec_egg", component) is not { } made ||
                    items[made].Emblem is not { } trait || counts.GetValueOrDefault(trait) < 1 ||
                    !traits[trait].Breakpoints.Contains(counts[trait] + 1)) continue;
                if (equipment.Craft(player, made)) { counts[trait]++; crafted = true; break; }
            }
            if (!crafted) break;
        }
        var holders = new PlanningAi(rules).Holders(player);
        for (int guard = 0; guard < 10; guard++)
        {
            int first = player.Items.FindIndex(shadows.Contains);
            if (first < 0) break;
            int best = -1; double bestScore = double.NegativeInfinity;
            for (int j = 0; j < player.Items.Count; j++)
            {
                string id = player.Items[j];
                if (j == first || !items[id].Component || id == "rec_egg" ||
                    equipment.Combine(player.Items[first], id) is not { } made || items[made].Emblem is not null) continue;
                double score = 0;
                foreach (var holder in holders)
                {
                    int rank = Array.IndexOf(rules.ItemRoles!.GetValueOrDefault(holder.Kind) ?? [], made);
                    if (holder.Unit.Items.Length < 3 && rank >= 0) score = Math.Max(score, 10 - rank * .1);
                }
                if (score > bestScore) { bestScore = score; best = j; }
            }
            if (best < 0 || bestScore <= 0 && player.Items.Count < 6 && stage < 4) break;
            string result = equipment.Combine(player.Items[first], player.Items[best])!;
            player.Items.RemoveAt(Math.Max(first, best)); player.Items.RemoveAt(Math.Min(first, best));
            player.Items.Add(result);
        }
        for (int guard = 0; guard < 3 && player.Items.Remove("rec_tome"); guard++)
        {
            var options = rules.Loot.Emblems.ToArray();
            for (int i = options.Length - 1; i > 0; i--)
            { int j = (int)(state.Random.Next() * (i + 1)); (options[i], options[j]) = (options[j], options[i]); }
            string best = options[0]; double bestScore = double.NegativeInfinity;
            foreach (string id in options.Take(4))
            {
                string trait = items[id].Emblem!; int count = counts.GetValueOrDefault(trait);
                double score = (traits[trait].Breakpoints.Contains(count + 1) ? 9 : count > 0 ? 5 : 1) + state.Random.Next();
                if (score > bestScore) { best = id; bestScore = score; }
            }
            player.GiveItem(best);
        }
        for (int guard = 0; guard < 3 && player.Items.Remove("rec_remover"); guard++) player.Economy.Credit(1);
    }

    public void RoundStart(MatchPlayer player, string roundType)
    {
        if (player.Augments.Contains("rec_a_scan")) player.Shop.FreeRolls++;
        var m = player.Mechanics.Reckoning;
        var keep = new List<ReckoningEgg>();
        foreach (var egg in m.Eggs)
        {
            var next = egg with { Left = egg.Left - 1 };
            if (next.Left > 0) { keep.Add(next); continue; }
            LootSpec drop = egg.Size switch
            {
                1 => new(Gold: egg.Golden ? 4 : 2),
                2 => new(Items: egg.Golden ? [loot.RandomComponent()!, loot.RandomComponent()!] : [loot.RandomComponent()!]),
                _ => new("gold", Gold: egg.Golden ? 3 : 0, Items: [loot.RandomCompleted()!])
            };
            loot.Drop(player, [drop]);
        }
        m.Eggs.Clear(); m.Eggs.AddRange(keep);
        var rules = state.Rules.Reckoning!;
        if (m.Blessed || !player.Alive || player.Health > rules.BlessAt && !m.EarlyBlessing || player.Offer is not null || roundType == "carousel") return;
        m.Blessed = true; m.EarlyBlessing = false;
        player.Economy.Credit(rules.BlessGold);
        player.Offer = new("rec_bless", offers.RadiantOptions(player, 3, []), 1, null);
    }

    public void RoundEnd(RoundEndContext context, int stage)
    {
        var player = context.Player;
        var dragon = state.Rules.Traits.Single(t => t.Id == "DRK");
        int level = TraitRules.Level(dragon, context.Traits.GetValueOrDefault("DRK"));
        if (!context.Pve && level > 0)
        {
            double roll = state.Random.Next(); int size = roll < .55 ? 1 : roll < .9 ? 2 : 3;
            player.Mechanics.Reckoning.Eggs.Add(new(size, size, dragon.Parameters["egg"][level - 1] >= 2));
        }
        if (!context.Won && player.Augments.Contains("rec_a_darkness"))
            player.Health -= state.Rules.Loss!.Loss(stage, -1, context.Pve, Array.Empty<Survivor>());
    }

    public IncomeBreakdown Income(MatchPlayer player, IncomeBreakdown income) =>
        player.Augments.Contains("rec_a_income") ? income with { Augment = income.Augment + 2 } : income;

    public void AugmentPicked(MatchPlayer player, string id)
    {
        switch (id)
        {
            case "rec_a_coins": player.Economy.Credit(8); break;
            case "rec_a_cache":
                player.GiveItem(loot.Pick(state.Rules.Loot!.Mode!.ShadowComponents)!);
                player.GiveItem(loot.Pick(state.Rules.Loot.Mode.ShadowComponents)!); break;
            case "rec_a_scan": player.Shop.FreeRolls++; break;
            case "rec_a_heart": player.Health += 4; break;
            case "rec_a_bank": player.Economy.Credit(16); break;
            case "rec_a_trove": player.GiveItem(loot.Pick(state.Rules.Items.Where(i => i.Category == "shadow").Select(i => i.Id).ToArray())!); player.GiveItem(loot.RandomComponent()!); break;
            case "rec_a_egg": player.GiveItem("rec_egg"); break;
            case "rec_a_jackpot": player.Economy.Credit(30); break;
            case "rec_a_blessing": if (!player.Mechanics.Reckoning.Blessed) player.Mechanics.Reckoning.EarlyBlessing = true; break;
            case "rec_a_armory": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomCompleted()!); break;
            case "rec_a_nature": player.GiveItem("rec_golden_egg"); break;
            case "rec_a_darkness": player.TeamBonus += 2; break;
            case "rec_a_tome": player.GiveItem("rec_tome"); player.GiveItem("rec_tome"); player.GiveItem("rec_egg"); break;
        }
    }
}
