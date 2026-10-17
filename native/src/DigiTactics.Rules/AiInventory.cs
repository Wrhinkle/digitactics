namespace DigiTactics.Rules;

/// <summary>Ordered crafting and equipment decisions; live unit lookups retain mutations across holder passes.</summary>
public sealed class AiInventory(Ruleset rules, PlanningItems equipment)
{
    private readonly PlanningAi ai = new(rules);
    private readonly Dictionary<string, ItemDefinition> items = rules.Items.ToDictionary(i => i.Id);
    private readonly Dictionary<string, UnitLine> lines = rules.Lines.Concat(rules.ExtraLines).ToDictionary(l => l.Id);

    public void Assign(MatchPlayer player, int stage, bool hasEquipRules)
    {
        var holders = ai.Holders(player);
        if (holders.Count == 0) return;
        var board = player.Roster.Board.Select(e => e.Unit.Id).ToArray();
        PlanningUnit Unit(int id) => player.Roster.Entries.First(e => e.Unit.Id == id).Unit;
        string[] Traits(int id) { var u = Unit(id); return TraitRules.UnitTraits(new(u.Line, u.Stage, 0, 4, Items: u.Items), rules); }
        var counts = TraitRules.Count(rules, board.Select(id => { var u = Unit(id); return new CombatEntry(u.Line, u.Stage, 0, 4, Items: u.Items, BonusTraits: u.BonusTraits); }), player.Augments.ToArray());
        string[] Role(string kind) => rules.ItemRoles!.GetValueOrDefault(kind) ?? [];
        bool Room(AiItemHolder holder) => Unit(holder.Unit.Id).Items.Length < 3;
        bool Give(string item, int uid) => equipment.GiveFromBench(player, item, uid);

        foreach (string id in player.Items.ToArray())
        {
            if (!items.TryGetValue(id, out var item) || item.Component || item.Consumable) continue;
            if (item.Category == "global") { player.Items.Remove(id); player.Globals.Add(id); continue; }
            if (item.Emblem is { } trait)
            {
                if (counts.GetValueOrDefault(trait) < 1 && player.Items.Count < 6 && stage < 5) continue;
                var carriers = holders.Take(2).Select(h => h.Unit.Id).ToHashSet();
                var target = board.Where(uid => Unit(uid).Items.Length < 3 && !Traits(uid).Contains(trait))
                    .OrderBy(carriers.Contains).ThenBy(uid => Unit(uid).Items.Length).Select(uid => (int?)uid).FirstOrDefault();
                // The reference advances its planning count even when an equip veto rejects the emblem.
                if (target is { } uid) { Give(id, uid); counts[trait] = counts.GetValueOrDefault(trait) + 1; }
                continue;
            }
            if (item.Passives.TryGetValue("teamSize", out var size) && size.GetDouble() != 0)
            {
                var holder = holders.FirstOrDefault(h => h.Kind == "tank") ?? holders[0];
                if (Room(holder)) Give(id, holder.Unit.Id);
                continue;
            }
            var chosen = holders.FirstOrDefault(h => Room(h) && Role(h.Kind).Contains(id)) ??
                (player.Items.Count >= 6 || stage >= 5 ? holders.FirstOrDefault(Room) : null);
            if (chosen is not null) Give(id, chosen.Unit.Id);
        }

        foreach (var holder in holders)
            foreach (string wanted in Role(holder.Kind))
            {
                if (!Room(holder)) break;
                if (Unit(holder.Unit.Id).Items.Contains(wanted)) continue;
                if (equipment.Craft(player, wanted)) Give(wanted, holder.Unit.Id);
            }

        foreach (string special in rules.Loot!.SpecialComponents)
        {
            string? doubled = equipment.Combine(special, special);
            if (doubled is not null && player.Items.Count(id => id == special) >= 2)
            {
                equipment.Craft(player, doubled);
                Assign(player, stage, hasEquipRules);
                return;
            }
            if (!player.Items.Contains(special)) continue;
            foreach (string component in rules.Loot.StatComponents)
            {
                string? made = equipment.Combine(special, component);
                if (made is null || items[made].Emblem is not { } trait) continue;
                var target = board.Where(uid => Unit(uid).Items.Length < 3 && !Traits(uid).Contains(trait)).Select(uid => (int?)uid).FirstOrDefault();
                bool near = rules.Traits.Single(t => t.Id == trait).Breakpoints.Any(b => counts.GetValueOrDefault(trait) == b - 1);
                if (player.Items.Contains(component) && near && target is { } uid && equipment.Craft(player, made)) { Give(made, uid); break; }
            }
        }

        string[] Components() => player.Items.Where(rules.Loot.StatComponents.Contains).ToArray();
        while (true)
        {
            var components = Components();
            if (components.Length < 2 || components.Length < 4 && stage < 4) break;
            string? made = null;
            foreach (var holder in holders)
            {
                if (!Room(holder)) continue;
                made = Role(holder.Kind).FirstOrDefault(id => equipment.RecipeFrom(player.Items, id) is not null);
                if (made is not null) { equipment.Craft(player, made); Give(made, holder.Unit.Id); break; }
            }
            if (made is not null) continue;
            string? fallback = equipment.Combine(components[0], components[1]);
            if (fallback is null || !equipment.Craft(player, fallback)) break;
            var target = holders.FirstOrDefault(Room);
            if (target is null) break;
            Give(fallback, target.Unit.Id);
        }

        if (!hasEquipRules) return;
        foreach (string id in player.Items.ToArray())
        {
            if (!items.TryGetValue(id, out var item) || item.Component || item.Consumable || item.Category == "global") continue;
            var target = player.Roster.Board.Select(e => e.Unit).Where(equipment.HoldsItems)
                .OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage)).ThenBy(u => u.Id)
                .FirstOrDefault(u => equipment.CanHold(player, u, id) && !(item.Emblem is { } trait && Traits(u.Id).Contains(trait)));
            if (target is not null) Give(id, target.Id);
        }
    }
}
