using System.Globalization;
using System.Text.Json;

namespace DigiTactics.Rules;

public sealed class ArcanePlanningState
{
    public int AcademyLevel { get; set; }
    public double Conquest { get; set; }
    public int Chests { get; set; }
    public int FindKills { get; set; }
    public int Shimmer { get; set; }
    private bool syndicateAugment;
    public bool HasSyndicateAugment { get; private set; }
    public bool SyndicateAugment { get => syndicateAugment; set { syndicateAugment = value; HasSyndicateAugment = true; } }
    public int MarketsQueued { get; set; }
    public int JunkRounds { get; set; }
    public int MarketsOpened { get; set; }
    public bool MarketPlus { get; set; }
    public int SyndicateLevel { get; set; }
    public double SyndicateHealth { get; set; }
    public int Skips { get; set; }
    public List<string> Junk { get; } = [];
    public List<string> Seen { get; } = [];
    public List<string> Anomalies { get; } = [];
    public string? Anomaly { get; set; }
    public int ExtraAnomalies { get; set; }
    public bool PendingAnomaly { get; set; }
}

/// <summary>Arcane choices mutate persistent planning records; combat consumes their modifiers.</summary>
public sealed class ArcanePlanning(MatchState state) : IMatchShopHooks
{
    private readonly MatchLoot loot = new(state);
    private readonly PlanningAi ai = new(state.Rules);
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    private T[] Shuffle<T>(IEnumerable<T> values)
    {
        var list = values.ToArray();
        for (int i = list.Length - 1; i > 0; i--) { int j = (int)(state.Random.Next() * (i + 1)); (list[i], list[j]) = (list[j], list[i]); }
        return list;
    }
    public int RoundEnd(RoundEndContext context)
    {
        var player = context.Player; var m = player.Mechanics.Arcane;
        bool hasCombat = context.Match.Combat is not null;
        var mine = hasCombat ? context.Units.Where(u => u.Survivor.Team == context.Team).Select(u =>
            u.State ?? throw new InvalidOperationException("An Arcane result is missing unit state.")).ToArray() : [];
        int gold = 0;
        foreach (var count in context.Traits)
        {
            var trait = state.Rules.Traits.FirstOrDefault(t => t.Id == count.Key);
            if (trait is null) continue;
            int level = TraitRules.Level(trait, count.Value);
            if (level == 0) continue;
            switch (count.Key)
            {
                case "ACA":
                    while (m.AcademyLevel < level && state.ArcaneSponsor.Length > 0)
                    {
                        m.AcademyLevel++;
                        player.GiveItem(loot.Pick(state.ArcaneSponsor)!);
                    }
                    break;
                case "SYN" when !context.Pve:
                    m.SyndicateLevel = level;
                    double gain = trait.Parameters[player.Economy.Streak <= -3 ? "streakGain" : "gain"][level - 1];
                    AddShimmer(m, (int)UnitValues.Round((state.Rules.Pace == "normal" ? .55 : 1) * gain));
                    break;
                case "CON" when hasCombat:
                    m.Conquest += mine.Sum(u => u.Arcane.ConquestKills) * trait.Parameters["mult"][level - 1];
                    while (m.Conquest >= 10)
                    {
                        m.Conquest -= 10; m.Chests++;
                        if (m.Chests % 3 == 0) player.GiveItem(loot.RandomCompleted()!);
                        else { player.Economy.Credit(2); player.GiveItem(loot.RandomComponent()!); }
                        if (level >= 3) player.Economy.Credit(2);
                    }
                    break;
                case "HR" when hasCombat:
                    gold += Math.Min(3, mine.Sum(u => u.Arcane.Coins));
                    break;
            }
        }
        if (hasCombat && player.Augments.Contains("arc_a_find"))
        {
            m.FindKills += mine.Where(u => !string.IsNullOrEmpty(u.Entry.ArcaneAnomaly)).Sum(u => u.Arcane.Anomaly.Kills);
            while (m.FindKills >= 3) { m.FindKills -= 3; gold++; }
        }
        foreach (var entry in player.Roster.Board)
            foreach (string id in entry.Unit.Items)
            {
                if (id == "arc_tamer_crown" && context.Won && state.Random.Next() < .1) gold++;
                else if (id == "arc_tamer_cape" && state.Random.Next() < .1) gold++;
                else if (id == "arc_tamer_shield" && hasCombat)
                {
                    var unit = context.Units.FirstOrDefault(u => u.Uid == entry.Unit.Id && u.Survivor.Team == context.Team);
                    if (unit is not null && !unit.Survivor.Alive && state.Random.Next() < .1) gold++;
                }
            }
        return gold + Math.Min(3, mine.Sum(u => u.Arcane.Items.BountyGold));
    }

    public void AiTurn(MatchPlayer player, PlanningItems inventory, MatchPhase phase)
    {
        for (int i = player.Items.Count - 1; i >= 0; i--)
        {
            if (!items.TryGetValue(player.Items[i], out var item) || item.Category != "contraband") continue;
            var holder = player.Roster.Entries.Select(e => e.Unit).Where(u => u.Items.Length < 3 &&
                (lines[u.Line].Traits.Contains("SYN") || u.Items.Any(id => items.TryGetValue(id, out var equipped) && equipped.Emblem == "SYN")))
                .OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage)).ThenBy(u => u.Id).FirstOrDefault();
            if (holder is null) continue;
            player.Roster.UpdateUnit(holder with { Items = holder.Items.Append(player.Items[i]).ToArray() });
            player.Items.RemoveAt(i);
        }
        for (int i = player.Items.Count - 1; i >= 0; i--)
        {
            string id = player.Items[i];
            if (id is not ("arc_copier" or "arc_copier_small")) continue;
            int maxTier = id == "arc_copier" ? 5 : 3;
            var counts = player.Roster.Entries.Select(e => e.Unit).Where(u => u.Stage == 0).GroupBy(u => u.Line).ToDictionary(g => g.Key, g => g.Count());
            var target = player.Roster.Board.Select(e => e.Unit).Where(u => u.Stage < 2 && lines[u.Line].Tier <= maxTier && state.Pool.Remaining(u.Line) > 0)
                .OrderByDescending(u => counts.GetValueOrDefault(u.Line)).ThenByDescending(u => UnitValues.Value(lines[u.Line], u.Stage)).ThenBy(u => u.Id).FirstOrDefault();
            if (target is not null && player.Roster.FreeBenchSlot >= 0) inventory.UseConsumable(player, i, target.Id, phase);
        }
    }

    public static void AddShimmer(ArcanePlanningState mechanic, int amount)
    {
        mechanic.Shimmer += amount;
        while (mechanic.Shimmer >= 100) { mechanic.Shimmer -= 100; mechanic.MarketsQueued++; }
    }

    public IncomeBreakdown Income(MatchPlayer player, IncomeBreakdown income) =>
        player.Augments.Contains("arc_a_income") ? income with { Augment = income.Augment + 2 } : income;

    public void AugmentPicked(MatchPlayer player, string id, Action<MatchPlayer> combine)
    {
        ArgumentNullException.ThrowIfNull(combine);
        var m = player.Mechanics.Arcane;
        string? Artifact() => loot.Pick(state.Rules.Items.Where(i => i.Category == "artifact").Select(i => i.Id).ToArray());
        switch (id)
        {
            case "arc_a_coins": player.Economy.Credit(8); break;
            case "arc_a_cache": player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!); break;
            case "arc_a_rolls": player.Shop.FreeRolls++; break;
            case "arc_a_heart": player.Health += 5; break;
            case "arc_a_shimmer":
                m.SyndicateAugment = true;
                AddShimmer(m, 60);
                if (state.Pool.Remaining("arc_mushroomon") > 0 && player.Roster.FreeBenchSlot >= 0)
                {
                    state.Pool.Take("arc_mushroomon");
                    player.Roster.AddToBench(state.CreateUnit("arc_mushroomon", 0));
                }
                combine(player);
                break;
            case "arc_a_bank": player.Economy.Credit(18); break;
            case "arc_a_trove": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomComponent()!); break;
            case "arc_a_tag": player.GiveItem("arc_spat"); break;
            case "arc_a_device": player.GiveItem("arc_pan"); break;
            case "arc_a_evolve": loot.GiveUnit(player, [2, 3], 1); break;
            case "arc_a_find": player.Shop.FreeRolls += 6; break;
            case "arc_a_forge": player.GiveItem(Artifact()!); break;
            case "arc_a_jackpot": player.Economy.Credit(35); break;
            case "arc_a_armory": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomCompleted()!); break;
            case "arc_a_crown": player.GiveItem("arc_tamer_crown"); break;
            case "arc_a_mega": loot.GiveUnit(player, [4], 1); break;
            case "arc_a_mutant":
                player.Economy.Credit(6);
                if (!string.IsNullOrEmpty(m.Anomaly)) m.PendingAnomaly = true;
                else m.ExtraAnomalies++;
                break;
            case "arc_a_workshop": player.GiveItem(Artifact()!); player.GiveItem(Artifact()!); break;
            case "arc_a_market": m.MarketPlus = true; AddShimmer(m, 120); break;
        }
    }

    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase) => unit;
    public void Sold(MatchPlayer player, PlanningUnit unit) { }
    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        if (!Free(unit) || sources.FirstOrDefault(u => !Free(u)) is not { } source) return;
        var mods = unit.Mods is { ValueKind: JsonValueKind.Object } existing ? existing.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone()) : [];
        foreach (var property in source.Mods!.Value.EnumerateObject()) mods[property.Name] = property.Value.Clone();
        player.Roster.UpdateUnit(unit with { Mods = JsonSerializer.SerializeToElement(mods) });
    }

    public void RoundStart(MatchPlayer player, RoundPosition round, MatchOffers offers)
    {
        var m = player.Mechanics.Arcane;
        if (player.Augments.Contains("arc_a_rolls")) player.Shop.FreeRolls++;
        if (state.ArcanePortal == "rolls" && round.Round == 1 && round.Stage >= 2) player.Shop.FreeRolls += 2;
        if (state.ArcanePortal == "golden" && round == new RoundPosition(1, 2)) offers.SetAugmentTier(2, 2);
        if (state.ArcanePortal == "prismatic" && round == new RoundPosition(4, 1)) offers.SetAugmentTier(4, 3);
        if (player.Offer is not null) return;
        if (m.PendingAnomaly)
        {
            m.PendingAnomaly = false;
            var options = AnomalyOptions(player, []);
            if (options.Length > 0) { player.Offer = new("arcAnomaly", options, 6, null); return; }
        }
        if (m.MarketsQueued > 0)
        {
            m.MarketsQueued--;
            m.MarketsOpened++;
            player.Offer = new("arcMarket", MarketOptions(player, []), 0, null);
            return;
        }
        if (!player.Roster.Entries.Any(e => e.Unit.Line == "arc_chaosdromon")) return;
        m.JunkRounds++;
        if (m.JunkRounds < (state.Rules.Pace == "normal" ? 3 : 2)) return;
        m.JunkRounds = 0;
        var parts = new[] { "flame", "bots", "rod", "destruct", "buster" };
        player.Offer = new("arcJunk", Shuffle(parts.Where(id => !m.Junk.Contains(id))).Take(2).Append("spare").ToArray(), 0, null);
    }

    public string[] MarketOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        var m = player.Mechanics.Arcane; int tier = m.MarketsOpened >= 2 ? 2 : 1;
        var options = Shuffle(state.Rules.Items.Where(i => i.Category == "contraband" && i.Tier == tier).Select(i => i.Id)).Take(m.MarketPlus ? 4 : 3).ToList();
        var hp = state.Rules.Traits.Single(t => t.Id == "SYN").Parameters["skipHp"];
        int index = Math.Max(1, m.SyndicateLevel) - 1;
        options.Add("arc_pass:" + (index < hp.Length ? hp[index].ToString(CultureInfo.InvariantCulture) : "undefined"));
        return options.Where(id => !exclude.Contains(id)).ToArray();
    }
    public void ChooseMarket(MatchPlayer player, string id)
    {
        if (id.StartsWith("arc_pass:", StringComparison.Ordinal))
        {
            string value = id.Split(':')[1];
            player.Mechanics.Arcane.SyndicateHealth += double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var hp) ? hp : 0;
            player.Mechanics.Arcane.Skips++;
        }
        else if (items.ContainsKey(id)) player.GiveItem(id);
    }
    public void ChooseJunk(MatchPlayer player, string id)
    {
        if (id is "flame" or "bots" or "rod" or "destruct" or "buster" or "spare") player.Mechanics.Arcane.Junk.Add(id);
    }
    public string[] AnomalyOptions(MatchPlayer player, IReadOnlyList<string> exclude)
    {
        if (exclude.Count > 0 && !player.Economy.Spend(1)) return exclude.ToArray();
        var m = player.Mechanics.Arcane;
        var pool = state.Rules.AnomalyTags!.Keys.Where(id => !exclude.Contains(id) && !m.Seen.Contains(id)).ToArray();
        if (pool.Length < 3) { m.Seen.Clear(); pool = state.Rules.AnomalyTags.Keys.Where(id => !exclude.Contains(id)).ToArray(); }
        var options = Shuffle(pool).Take(3).ToArray(); m.Seen.AddRange(options); return options;
    }
    private static bool Free(PlanningUnit? unit) => unit is not null && !(unit.Mods is { } mods && mods.TryGetProperty("arcAnom", out var value) &&
        value.ValueKind is not (JsonValueKind.Null or JsonValueKind.False or JsonValueKind.Undefined) &&
        (value.ValueKind != JsonValueKind.String || value.GetString() != "") && (value.ValueKind != JsonValueKind.Number || value.GetDouble() != 0));
    public PlanningUnit? AnomalyTarget(MatchPlayer player)
    {
        if (!player.IsHuman && ai.Holders(player).FirstOrDefault(h => Free(h.Unit)) is { } holder) return holder.Unit;
        var center = player.Roster.At(UnitSpot.Board(3, 5)); if (Free(center)) return center;
        return player.Roster.Board.Select(e => e.Unit).Where(Free).OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage))
            .ThenByDescending(u => u.Items.Length).ThenBy(u => u.Id).FirstOrDefault() ?? player.Roster.Bench.FirstOrDefault(Free);
    }
    public void ChooseAnomaly(MatchPlayer player, string id)
    {
        if (!state.Rules.AnomalyTags!.ContainsKey(id)) return;
        var unit = AnomalyTarget(player); if (unit is null) { player.Economy.Credit(8); return; }
        var mods = unit.Mods is { ValueKind: JsonValueKind.Object } existing ? existing.EnumerateObject().ToDictionary(p => p.Name, p => p.Value.Clone()) : [];
        mods["arcAnom"] = JsonSerializer.SerializeToElement(id);
        player.Roster.UpdateUnit(unit with { Mods = JsonSerializer.SerializeToElement(mods) });
        var m = player.Mechanics.Arcane; m.Anomalies.Add(id); m.Anomaly = id;
        if (id == "u_crest")
        {
            var counts = ai.Counts(player.Roster.Board.Select(e => e.Unit));
            var traits = state.Rules.Traits.ToDictionary(t => t.Id);
            var candidates = lines[unit.Line].Traits.Where(t => items.ContainsKey("arc_crest_" + t.ToLowerInvariant()) && traits[t].Breakpoints[0] > 1).ToArray();
            int Gap(string trait) { int n = counts.GetValueOrDefault(trait); int next = traits[trait].Breakpoints.FirstOrDefault(b => b > n); return next != 0 ? next - n : 99; }
            if (candidates.Length == 0) player.Economy.Credit(6);
            else player.GiveItem("arc_crest_" + candidates.OrderBy(Gap).First().ToLowerInvariant());
        }
        if (id == "u_dual")
        {
            var hints = lines[unit.Line].Ai!; string role = hints.Carry ? hints.Damage == "ap" ? "ap" : "ad" : "tank";
            var preferred = (state.Rules.ItemRoles!.GetValueOrDefault(role) ?? []).Where(state.Rules.CoreItems.Contains).Take(8);
            foreach (string item in Shuffle(preferred).Take(2)) player.GiveItem(item);
        }
        if (m.ExtraAnomalies > 0) { m.ExtraAnomalies--; m.PendingAnomaly = true; }
    }
}
