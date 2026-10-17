using System.Text.Json;

namespace DigiTactics.Rules;

/// <summary>Partner offers reserve one shop copy; purchase reserves the two remaining copies.</summary>
public sealed class FatesPlanning(MatchState state) : IMatchShopHooks
{
    private static readonly string[] Bonuses = ["hp", "ap", "ad", "mana"];
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.ToDictionary(l => l.Id);
    private readonly Dictionary<string, TraitDefinition> traits = state.Rules.Traits.ToDictionary(t => t.Id);
    private readonly Dictionary<string, UnitLine> allLines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);

    private readonly MatchLoot loot = new(state);

    public int RoundEnd(RoundEndContext context)
    {
        int gold = 0;
        var player = context.Player;
        var mechanic = player.Mechanics;
        foreach (var count in context.Traits)
        {
            if (!traits.TryGetValue(count.Key, out var trait)) continue;
            int level = TraitRules.Level(trait, count.Value);
            if (level == 0) continue;
            switch (count.Key)
            {
                case "TREASURE" when !context.Pve:
                    if (!context.Won) mechanic.FatesLuck = Math.Min(8, mechanic.FatesLuck + 1);
                    else
                    {
                        gold += 1 + mechanic.FatesLuck + (level >= 2 ? 3 : 0);
                        if (mechanic.FatesLuck >= 3) player.GiveItem(loot.RandomComponent()!);
                        if (mechanic.FatesLuck >= 5) player.GiveItem(loot.RandomCompleted()!);
                        mechanic.FatesLuck = 0;
                    }
                    break;
                case "DEVA" when !context.Pve && context.Won:
                    mechanic.FatesDeva = Math.Min(5, mechanic.FatesDeva + 1);
                    break;
                case "FALLEN":
                    mechanic.FatesFallenFights++;
                    if (mechanic.FatesFallenFights >= 3 && string.IsNullOrEmpty(mechanic.FatesPath) &&
                        !mechanic.FatesFallenGiven && player.Items.Count <= state.Rules.Loot!.ItemCap - 2)
                    {
                        mechanic.FatesFallenGiven = true;
                        player.GiveItem("fat_holy");
                        player.GiveItem("fat_dark");
                    }
                    break;
                case "FORGE":
                    int smith = player.Roster.Board.Where(e => e.Unit.Line == "fat_volcamon").Select(e => e.Unit.Stage).DefaultIfEmpty(-1).Max();
                    if (smith < 0) break;
                    mechanic.FatesForge += smith + 1;
                    var artifacts = state.Rules.Loot!.Mode!.Artifacts;
                    while (mechanic.FatesForge >= trait.Values[level - 1] && mechanic.FatesForged < 4 && player.Items.Count < state.Rules.Loot.ItemCap)
                    {
                        mechanic.FatesForge -= (int)trait.Values[level - 1];
                        mechanic.FatesForged++;
                        var eligible = artifacts.Where(id => !player.Items.Contains(id)).ToArray();
                        player.GiveItem(loot.Pick(eligible.Length > 0 ? eligible : artifacts)!);
                    }
                    break;
            }
        }
        if (context.Match.Combat is null) return gold;
        foreach (var result in context.Units.Where(u => u.Survivor.Team == context.Team && !u.Survivor.Summoned))
        {
            var unit = result.State ?? throw new InvalidOperationException("A Fates result is missing unit state.");
            if (unit.BlossomKills != 0) context.Player.Mechanics.FatesBlossom = Math.Min(60, context.Player.Mechanics.FatesBlossom + 2 * unit.BlossomKills);
            gold += unit.CombatGold;
        }
        return gold;
    }

    public void AiTurn(MatchPlayer player, PlanningItems inventory, MatchPhase phase)
    {
        string? path = player.Items.Contains("fat_holy") ? state.Random.Next() < .5 ? "fat_holy" : "fat_dark" : player.Items.Contains("fat_dark") ? "fat_dark" : null;
        if (path is not null && string.IsNullOrEmpty(player.Mechanics.FatesPath))
        {
            var fallen = player.Roster.Entries.FirstOrDefault(e => e.Unit.Line == "fat_lopmon")?.Unit;
            if (fallen is not null) inventory.UseConsumable(player, player.Items.IndexOf(path), fallen.Id, phase);
        }
        var units = player.Roster.Entries.Select(e => e.Unit).Where(u => u.Stage < 2).ToArray();
        int index = player.Items.IndexOf("fat_copy");
        if (index >= 0)
        {
            var counts = units.GroupBy(u => u.Line).ToDictionary(g => g.Key, g => g.Count());
            var best = units.OrderByDescending(u => counts[u.Line]).ThenByDescending(u => allLines[u.Line].Tier).FirstOrDefault();
            if (best is not null) inventory.UseConsumable(player, index, best.Id, phase);
        }
        index = player.Items.IndexOf("fat_dice");
        if (index >= 0)
        {
            var best = player.Roster.Board.Select(e => e.Unit).OrderByDescending(u => UnitValues.Value(allLines[u.Line], u.Stage)).FirstOrDefault();
            if (best is not null) inventory.UseConsumable(player, index, best.Id, phase);
        }
    }
    public IShopRollHooks ShopHooks(MatchPlayer player, Func<RoundPosition> position) => new RollHooks(this, player, position);

    private sealed class RollHooks(FatesPlanning mode, MatchPlayer player, Func<RoundPosition> position) : IShopRollHooks
    {
        public ShopRollRequest BeforeRoll(ShopRollRequest request) => request;
        public void Rolled(PlayerShop shop) => mode.OfferPartner(player, position());
    }

    public static bool IsPartner(PlanningUnit unit) => unit.Mods is { } mods && mods.TryGetProperty("partner", out var value) &&
        value.ValueKind switch { JsonValueKind.False or JsonValueKind.Null or JsonValueKind.Undefined => false,
            JsonValueKind.Number => value.GetDouble() != 0, JsonValueKind.String => value.GetString()!.Length > 0, _ => true };

    public bool OfferPartner(MatchPlayer player, RoundPosition round)
    {
        var config = state.Rules.Partner!;
        if (player.Economy.Level < config.FromLevel || player.Roster.Entries.Any(e => IsPartner(e.Unit)) || player.Shop.Slots.Count == 0) return false;
        bool force = !player.Mechanics.FatesPartnerSeen && (round.Stage > 2 || round.Stage == 2 && round.Round >= 2);
        double chance = player.Augments.Contains("fat_seeker") ? config.SeekerChance : config.Chance;
        if (!force && state.Random.Next() >= chance) return false;
        var odds = config.Odds![Math.Min(config.Odds.Length - 1, player.Economy.Level)]!;
        double roll = state.Random.Next() * 100; int tier = 1;
        for (int i = 0; i < odds.Length; i++) { if (roll < odds[i]) { tier = i + 1; break; } roll -= odds[i]; }
        var excluded = player.Roster.MaxedLines(); UnitLine[] candidates = [];
        for (int cost = tier; cost >= 1 && candidates.Length == 0; cost--)
            candidates = state.Rules.Lines.Where(l => l.Tier == cost && state.Pool.Remaining(l.Id) >= 3 && !excluded.Contains(l.Id)).ToArray();
        if (candidates.Length == 0) return false;
        var line = candidates[(int)(state.Random.Next() * candidates.Length)];
        int slot = (int)(state.Random.Next() * player.Shop.Slots.Count);
        if (!player.Shop.ReplaceOffer(slot, line.Id)) throw new InvalidOperationException("Partner reservation changed during shop roll.");
        for (int i = 0; i < player.Shop.Slots.Count; i++) player.Shop.SetMetadata(i, null);
        player.Shop.SetMetadata(slot, new(line.Tier * 3, "Partner", PartnerSpec(line)));
        player.Mechanics.FatesPartnerSeen = true; return true;
    }

    private ShopUnitOverride PartnerSpec(UnitLine line)
    {
        var marks = line.Traits.Where(t => traits.TryGetValue(t, out var definition) && definition.Kind != "unique").ToArray();
        if (marks.Length == 0) marks = line.Traits;
        string trait = marks[(int)(state.Random.Next() * marks.Length)];
        string bonus = Bonuses[(int)(state.Random.Next() * Bonuses.Length)];
        return new(1, new() { [trait] = 1 }, JsonSerializer.SerializeToElement(new { partner = true, trait, bonus }));
    }

    public IncomeBreakdown Income(MatchPlayer player, IncomeBreakdown income) =>
        player.Augments.Contains("fat_offerings") ? income with { Augment = income.Augment + 2 } : income;

    public void RoundStart(MatchPlayer player)
    {
        if (player.Augments.Contains("fat_dice_aug")) player.Shop.FreeRolls++;
    }

    public void AugmentPicked(MatchPlayer player, string id, Action<MatchPlayer> combine)
    {
        ArgumentNullException.ThrowIfNull(combine);
        string? Artifact() => loot.Pick(state.Rules.Loot!.Mode!.Artifacts);
        switch (id)
        {
            case "fat_envelope": player.Economy.Credit(8); break;
            case "fat_stall": player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!); break;
            case "fat_dice_aug": player.Shop.FreeRolls++; break;
            case "fat_charm": player.Health += 5; break;
            case "fat_seeker": player.Economy.Credit(3); break;
            case "fat_pouch": player.Economy.Credit(18); break;
            case "fat_chest": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomComponent()!); break;
            case "fat_egghunt": player.GiveItem("fat_egg"); break;
            case "fat_tome": player.GiveItem(loot.RandomEmblem(player)!); break;
            case "fat_apprentice": player.GiveItem(Artifact()!); break;
            case "fat_digivolve": loot.GiveUnit(player, [2, 3], 1); break;
            case "fat_fortune": player.Economy.Credit(35); break;
            case "fat_grandarmory": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomCompleted()!); break;
            case "fat_golden": player.GiveItem("fat_golden_egg"); break;
            case "fat_master": player.GiveItem(Artifact()!); player.GiveItem(Artifact()!); break;
            case "fat_mega": loot.GiveUnit(player, [4], 1); break;
            case "fat_festival":
                player.Economy.Credit(4);
                var candidates = state.Rules.Lines.Where(l => l.Tier == 4 && state.Pool.Remaining(l.Id) >= 3).ToArray();
                if (player.Roster.Entries.Any(e => IsPartner(e.Unit)) || candidates.Length == 0 || player.Roster.FreeBenchSlot < 0)
                {
                    player.Economy.Credit(4);
                    break;
                }
                var line = candidates[(int)(state.Random.Next() * candidates.Length)];
                state.Pool.Take(line.Id, 3);
                var unit = state.CreateUnit(line.Id, 0);
                var spec = PartnerSpec(line);
                player.Roster.AddToBench(unit with { Stage = spec.Stage!.Value, BonusTraits = spec.BonusTraits, Mods = spec.Mods });
                combine(player);
                break;
        }
    }

    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase)
    {
        if (!IsPartner(unit)) return unit;
        if (state.Pool.Remaining(unit.Line) >= 2 && player.Roster.FreeBenchSlot >= 0) state.Pool.Take(unit.Line, 2);
        else { player.Economy.Credit(2 * lines[unit.Line].Tier); unit = unit with { Stage = 0 }; }
        return unit;
    }
    public void Sold(MatchPlayer player, PlanningUnit unit) { }
    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        if (sources.FirstOrDefault(IsPartner) is { } source && !IsPartner(unit))
            player.Roster.UpdateUnit(unit with { Mods = source.Mods, BonusTraits = source.BonusTraits });
    }
}
