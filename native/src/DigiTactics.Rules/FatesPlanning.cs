using System.Text.Json;

namespace DigiTactics.Rules;

/// <summary>Partner offers reserve one shop copy; purchase reserves the two remaining copies.</summary>
public sealed class FatesPlanning(MatchState state) : IMatchShopHooks
{
    private static readonly string[] Bonuses = ["hp", "ap", "ad", "mana"];
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.ToDictionary(l => l.Id);
    private readonly Dictionary<string, TraitDefinition> traits = state.Rules.Traits.ToDictionary(t => t.Id);
    private readonly Dictionary<string, UnitLine> allLines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);

    public int RoundEnd(RoundEndContext context)
    {
        if (context.Match.Combat is null) return 0;
        int gold = 0;
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
        var marks = line.Traits.Where(t => traits.TryGetValue(t, out var definition) && definition.Kind != "unique").ToArray();
        if (marks.Length == 0) marks = line.Traits;
        string trait = marks[(int)(state.Random.Next() * marks.Length)];
        string bonus = Bonuses[(int)(state.Random.Next() * Bonuses.Length)];
        player.Shop.SetMetadata(slot, new(line.Tier * 3, "Partner", new(1, new() { [trait] = 1 },
            JsonSerializer.SerializeToElement(new { partner = true, trait, bonus }))));
        player.Mechanics.FatesPartnerSeen = true; return true;
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
