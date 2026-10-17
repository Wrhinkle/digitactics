namespace DigiTactics.Rules;

/// <summary>Galaxy opening rewards, size changes, purchase history, salvage and AI copier use.</summary>
public sealed class GalaxiesPlanning(MatchState state) : IMatchShopHooks
{
    private readonly Dictionary<string, UnitLine> lines = state.Rules.Lines.ToDictionary(l => l.Id);
    private readonly Dictionary<string, ItemDefinition> items = state.Rules.Items.ToDictionary(i => i.Id);
    public int TeamSize(int size, int stage) => size + (state.Galaxy == "bigger" && stage >= 2 ? 1 : 0);

    public void RoundStart(MatchPlayer player, RoundPosition round)
    {
        if (state.Galaxy is "trade" or "bigbang") player.Shop.FreeRolls++;
        if (state.Galaxy == "cluster" && round == new RoundPosition(1, 2))
        {
            var unit = player.Roster.Entries.Select(e => e.Unit).FirstOrDefault(u => u.Stage == 0 && lines.ContainsKey(u.Line));
            if (unit is not null)
            {
                int need = lines[unit.Line].Copies, stage = state.Pool.Remaining(unit.Line) >= need * need - 1 ? 2 : state.Pool.Remaining(unit.Line) >= need - 1 ? 1 : 0;
                if (stage != 0) { state.Pool.Take(unit.Line, UnitValues.Copies(lines[unit.Line], stage) - 1); player.Roster.UpdateUnit(unit with { Stage = stage }); }
            }
        }
        if (state.Galaxy == "mirror" && round.Round == 1 && round.Stage is 2 or 3) player.GiveItem("gal_copier");
        if (state.Galaxy is "superdense" or "bigbang" && round == new RoundPosition(3, 1)) player.GiveItem("gal_golden");
    }
    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase)
    {
        if (lines.TryGetValue(unit.Line, out var line) && line.Traits.Contains("BH"))
            player.Mechanics.GalaxiesBounties[line.Id] = Math.Min(9, player.Mechanics.GalaxiesBounties.GetValueOrDefault(line.Id) + 1);
        return unit;
    }
    public void Sold(MatchPlayer player, PlanningUnit unit)
    {
        if (state.Galaxy != "salvage") return;
        foreach (string id in unit.Items)
        {
            if (!items.TryGetValue(id, out var item) || item.Ingredients is null || item.Component || item.Emblem is not null || item.Category != "completed") continue;
            int index = player.Items.LastIndexOf(id);
            if (index < 0 || player.Items.Count + 1 > state.Rules.Loot!.ItemCap) continue;
            player.Items.RemoveAt(index);
            foreach (var component in item.Ingredients) player.GiveItem(component);
        }
    }
    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) { }
    public void AiTurn(MatchPlayer player, PlanningItems inventory, MatchPhase phase)
    {
        for (int guard = 0; guard < 3; guard++)
        {
            int index = player.Items.IndexOf("gal_copier"); if (index < 0) return;
            var units = player.Roster.Entries.Select(e => e.Unit).ToArray();
            var candidates = units.Where(u => lines.ContainsKey(u.Line) && state.Pool.Remaining(u.Line) > 0 && u.Stage < 2);
            PlanningUnit? best = null; int high = int.MinValue;
            foreach (var unit in candidates)
            {
                int score = lines[unit.Line].Tier * 3 + units.Count(u => u.Line == unit.Line && u.Stage == 0) * 4 + (unit.Stage == 0 ? 2 : 0);
                if (score > high) { best = unit; high = score; }
            }
            if (best is null || inventory.Equip(player, index, best.Id, phase) == "none") return;
        }
    }
}
