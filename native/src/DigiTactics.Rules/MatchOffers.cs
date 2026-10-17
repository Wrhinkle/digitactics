namespace DigiTactics.Rules;

public sealed record OfferDefinition(string Kind, int Stage = 0, int Round = 0, int? Rerolls = null, int? Tier = null, int? Count = null, int? Gate = null);
public sealed record AugmentDefinition(string Id, int Tier);
public sealed record OfferRules(OfferDefinition[] Schedule, int[] ArmoryStages, int ArmoryRound, string[]? ArmoryRounds,
    int[] AugmentStages, int AugmentRound, string[]? AugmentRounds, int AugmentRerolls, double[] AugmentTierOdds,
    AugmentDefinition[] Augments, AugmentDefinition[] DuoAugments, string[] Powers, bool CustomArmory, string[] PowerRanks);
public sealed record PlayerOffer(string Kind, string[] Options, int Rerolls, int? Tier, bool UnderlordsRolled = false);

public interface IMatchOfferHooks
{
    string[] Armory(MatchPlayer player, RoundPosition round);
    string[] CustomOptions(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude);
    void AugmentPicked(MatchPlayer player, string id);
    void PowerPicked(MatchPlayer player, string id);
    void CustomChosen(MatchPlayer player, PlayerOffer offer, string id);
}

/// <summary>Offer scheduling and shared augment tier rolls; mode effects are dispatched through the match hook layer.</summary>
public sealed class MatchOffers(MatchState state, IMatchOfferHooks hooks)
{
    private readonly MatchLoot loot = new(state);
    private readonly ModeOfferRewards modeRewards = new(state);
    private readonly ArcanePlanning arcane = new(state);
    private readonly ColiseumPlanning coliseum = new(state);
    private OfferRules Rules => state.Rules.Offers!;
    private int tierStage = -1, tier;

    public OfferDefinition? At(RoundPosition round)
    {
        var scheduled = Rules.Schedule.FirstOrDefault(o => o.Stage == round.Stage && o.Round == round.Round);
        if (scheduled is not null) return scheduled;
        if (Rules.ArmoryRounds is { } armory ? armory.Contains(round.Label) :
            round.Stage >= 2 && round.Round == Rules.ArmoryRound && Rules.ArmoryStages.Contains(round.Stage)) return new("armory");
        if (Rules.AugmentRounds is { } augments ? augments.Contains(round.Label) :
            round.Stage >= 2 && round.Round == Rules.AugmentRound && Rules.AugmentStages.Contains(round.Stage)) return new("augment");
        return null;
    }
    public void Make(MatchPlayer player, RoundPosition round)
    {
        player.Offer = null;
        var definition = At(round);
        if (definition is null || definition.Kind == "power" && player.Power is not null) return;
        var options = Options(player, round, definition, []);
        if (options.Length > 0) player.Offer = new(definition.Kind, options,
            definition.Rerolls ?? (definition.Kind == "augment" ? Rules.AugmentRerolls : 0), definition.Tier);
    }
    public bool Reroll(MatchPlayer player, RoundPosition round)
    {
        if (player.Offer is not { Rerolls: > 0 } offer) return false;
        player.Offer = offer with { Rerolls = offer.Rerolls - 1 };
        var options = offer.Kind == "armory" ? Armory(player, round)
            : Options(player, round, new(offer.Kind, Tier: offer.Tier), offer.Options);
        player.Offer = player.Offer with { Options = options };
        return true;
    }
    public string? Choose(MatchPlayer player, int index, bool boardLocked)
    {
        if (player.Offer is not { } offer || (uint)index >= offer.Options.Length) return null;
        string id = offer.Options[index];
        player.Offer = null;
        switch (offer.Kind)
        {
            case "armory": case "itemTier":
                if (state.Rules.Items.FirstOrDefault(i => i.Id == id)?.Category == "global") player.Globals.Add(id);
                else player.GiveItem(id);
                break;
            case "augment":
                player.Augments.Add(id);
                hooks.AugmentPicked(player, id);
                Combine(player, boardLocked);
                break;
            case "power":
                if (!Rules.Powers.Contains(id)) break;
                player.Power = new(id);
                hooks.PowerPicked(player, id);
                Combine(player, boardLocked);
                break;
            case "rec_bless": case "rec_radiant":
                modeRewards.GrantRadiant(player, id);
                break;
            case "col_core":
                coliseum.ChooseCore(player, id);
                break;
            case "col_form":
                coliseum.ChooseForm(player, id);
                break;
            case "col_gambit":
                coliseum.ChooseGambit(player, id);
                break;
            case "rec_tome":
                player.GiveItem(id);
                break;
            case "drg_treasure":
                modeRewards.ChooseTreasure(player, id, boardLocked);
                break;
            case "arcAnomaly": arcane.ChooseAnomaly(player, id); break;
            case "arcMarket": arcane.ChooseMarket(player, id); break;
            case "arcJunk": arcane.ChooseJunk(player, id); break;
            case "ul_tamer":
                if (player.Power is not null || !Rules.Powers.Contains(id)) break;
                player.Power = new(id);
                hooks.PowerPicked(player, id);
                Combine(player, boardLocked);
                break;
            case "ul_crew":
                var alliances = id.Split('+');
                if (alliances.Length < 2 || !state.Rules.Traits.Any(t => t.Id == alliances[0]) || !state.Rules.Traits.Any(t => t.Id == alliances[1])) break;
                var crew = Shuffle(state.Rules.Lines.Where(l => l.Tier <= 2 && (l.Traits.Contains(alliances[0]) || l.Traits.Contains(alliances[1])) && state.Pool.Remaining(l.Id) > 0).Select(l => l.Id));
                int added = 0;
                foreach (string line in crew)
                {
                    if (added >= 5 || player.Roster.FreeBenchSlot < 0) break;
                    state.Pool.Take(line);
                    player.Roster.AddToBench(state.CreateUnit(line, 0));
                    added++;
                }
                Combine(player, boardLocked);
                break;
            case "upgrade":
                if (!state.Rules.OfferAi!.ContainsKey("augment:" + id)) break;
                player.Augments.Add(id);
                hooks.AugmentPicked(player, id);
                Combine(player, boardLocked);
                break;
            case "ac_relic":
                var relics = state.Rules.Items.Where(i => i.Relic).Select(i => i.Id).ToHashSet();
                player.Globals.RemoveAll(relics.Contains);
                player.Globals.Add(id);
                switch (id)
                {
                    case "ac_r_forge": player.GiveItem(loot.RandomCompleted()!); break;
                    case "ac_r_eggnest": player.GiveItem("ac_digiegg"); player.GiveItem("ac_digiegg"); break;
                    case "ac_r_card": player.Shop.FreeRolls++; break;
                }
                Combine(player, boardLocked);
                break;
            case "ac_loot":
                player.GiveItem(id);
                player.Mechanics.AutoChessLoot = null;
                break;
            default: hooks.CustomChosen(player, offer, id); break;
        }
        return id;
    }
    private void Combine(MatchPlayer player, bool boardLocked)
    {
        player.Roster.Combine(boardLocked, player.GiveItem);
        player.Shop.ClearMaxed();
    }
    public string[] Options(MatchPlayer player, RoundPosition round, OfferDefinition definition, IReadOnlyList<string> exclude)
    {
        int count = definition.Count is null or 0 ? 3 : definition.Count.Value;
        return definition.Kind switch
        {
            "armory" => Armory(player, round),
            "augment" => Augments(player, round.Stage, exclude),
            "power" => Shuffle(Rules.Powers.Where(id => !exclude.Contains(id))).Take(count).ToArray(),
            "itemTier" => Shuffle(state.Rules.Items.Where(i => i.Tier == definition.Tier && !i.Component && !i.NoDrop && !exclude.Contains(i.Id))
                .Select(i => i.Id)).Take(count).ToArray(),
            "rec_bless" => modeRewards.RadiantOptions(player, 3, exclude),
            "rec_radiant" => modeRewards.RadiantOptions(player, 4, exclude),
            "rec_tome" => Shuffle(state.Rules.Loot!.Emblems).Take(4).ToArray(),
            "col_core" => coliseum.CoreOptions(player, exclude),
            "col_form" => coliseum.FormOptions(player),
            "col_gambit" => coliseum.GambitOptions(player),
            "drg_treasure" => modeRewards.TreasureOptions(player, exclude),
            "arcAnomaly" => arcane.AnomalyOptions(player, exclude),
            "arcMarket" => arcane.MarketOptions(player, exclude),
            "arcJunk" => new[] { "spare" }.Where(id => !exclude.Contains(id)).ToArray(),
            "ul_tamer" => new[] { new[] { "ul_anessa_dark", "ul_anessa_heal" }, new[] { "ul_hob_fire", "ul_hob_crazy" },
                new[] { "ul_jules_brawl", "ul_jules_bar" }, new[] { "ul_enno_balm", "ul_enno_rabid" } }.Select(builds => builds[(int)(state.Random.Next() * 2)]).ToArray(),
            "ul_crew" => CrewOptions(exclude),
            "upgrade" => UpgradeOptions(player, round.Stage, exclude),
            "ac_relic" => Shuffle(state.Rules.Items.Where(i => i.Relic && !player.Globals.Contains(i.Id) && !exclude.Contains(i.Id)).Select(i => i.Id)).Take(3).ToArray(),
            "ac_loot" => player.Mechanics.AutoChessLoot?.ToArray() ?? [],
            _ => hooks.CustomOptions(player, round, definition, exclude),
        };
    }
    private string[] CrewOptions(IReadOnlyList<string> exclude)
    {
        var alliances = state.Rules.Traits.Where(t => state.Rules.Lines.Count(l => l.Tier <= 2 && l.Traits.Contains(t.Id)) >= 2).Select(t => t.Id).ToArray();
        var result = new List<string>();
        for (int guard = 0; result.Count < 3 && guard < 40; guard++)
        {
            string first = alliances[(int)(state.Random.Next() * alliances.Length)], second = alliances[(int)(state.Random.Next() * alliances.Length)];
            string id = string.CompareOrdinal(first, second) <= 0 ? first + "+" + second : second + "+" + first;
            if (first != second && !result.Contains(id) && !exclude.Contains(id)) result.Add(id);
        }
        return result.ToArray();
    }

    private string[] UpgradeOptions(MatchPlayer player, int stage, IReadOnlyList<string> exclude)
    {
        int gate = Array.FindIndex(Rules.Schedule, o => o.Kind == "upgrade" && o.Stage == stage);
        int tier = state.GizmosGates is { } gates && gate >= 0 && gate < gates.Length && gates[gate] != 0 ? gates[gate] : 2;
        var eligible = Rules.Augments.Where(a => !player.Augments.Contains(a.Id) && !exclude.Contains(a.Id)).ToArray();
        var pool = eligible.Where(a => a.Tier == tier).ToArray();
        return Shuffle(pool.Length < 3 ? eligible : pool).Take(3).Select(a => a.Id).ToArray();
    }

    private string[] Augments(MatchPlayer player, int stage, IReadOnlyList<string> exclude)
    {
        if (tierStage != stage)
        {
            double value = state.Random.Next();
            tier = value < Rules.AugmentTierOdds[0] ? 1 : value < Rules.AugmentTierOdds[1] ? 2 : 3;
            tierStage = stage;
        }
        var eligible = Rules.Augments.Where(a => !player.Augments.Contains(a.Id) && !exclude.Contains(a.Id)).ToArray();
        var pool = eligible.Where(a => a.Tier == tier).ToArray();
        var result = Shuffle(pool.Length > 0 ? pool : eligible).Take(3).Select(a => a.Id).ToList();
        if (state.Duos)
        {
            var duo = Rules.DuoAugments.Where(a => !player.Augments.Contains(a.Id) && !exclude.Contains(a.Id)).ToArray();
            var fit = duo.Where(a => a.Tier == tier).ToArray();
            var from = fit.Length > 0 ? fit : duo;
            if (from.Length > 0)
            {
                string id = from[(int)(state.Random.Next() * from.Length)].Id;
                if (result.Count >= 3) result[2] = id; else result.Add(id);
            }
        }
        return result.ToArray();
    }
    private string[] Armory(MatchPlayer player, RoundPosition round)
    {
        if (Rules.CustomArmory) return modeRewards.Armory(player, round) ?? hooks.Armory(player, round);
        List<string?> items = [];
        for (int guard = 0; items.Count < 3 && guard < 60; guard++)
        {
            string? id = round.Stage <= 3 ? loot.RandomComponent(true) : loot.RandomCompleted();
            if (!items.Contains(id)) items.Add(id);
        }
        items.RemoveAll(id => id is null);
        void Set(int index, string value) { while (items.Count <= index) items.Add(null); items[index] = value; }
        if (round.Stage >= 3 && state.Random.Next() < .2 && state.Rules.Loot!.Consumables.Length > 0)
            Set(2, loot.Pick(state.Rules.Loot.Consumables)!);
        if (round.Stage >= 3 && state.Random.Next() < .25 && loot.RandomEmblem() is { } emblem) Set(1, emblem);
        return items.OfType<string>().ToArray();
    }
    private T[] Shuffle<T>(IEnumerable<T> source)
    {
        var items = source.ToArray();
        for (int i = items.Length - 1; i > 0; i--)
        {
            int j = (int)(state.Random.Next() * (i + 1));
            (items[i], items[j]) = (items[j], items[i]);
        }
        return items;
    }
}
