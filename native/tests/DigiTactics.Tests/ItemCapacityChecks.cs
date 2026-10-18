using DigiTactics.Rules;

internal static class ItemCapacityChecks
{
    internal static int Run(ContentCatalog catalog)
    {
        int checks = 0;
        void Check(bool condition, string message) { checks++; if (!condition) throw new InvalidOperationException("Item capacity: " + message); }
        foreach (var rules in catalog.Variants)
        foreach (var glove in rules.Items.Where(i => i.Passives.TryGetValue("thief", out var thief) && thief.GetBoolean()))
        {
            var state = new MatchState(rules, 143, [new("Human", true)]);
            var player = state.Players[0];
            var equipment = new PlanningItems(rules, new Hooks());
            var line = rules.Lines.First(l => !l.NoItems && l.Ai!.Carry);
            string ordinary = rules.CoreItems.First(id => !rules.Items.Single(i => i.Id == id).Passives.ContainsKey("thief"));
            int nextId = 0;
            PlanningUnit Reset(string[] held, string[] inventory)
            {
                foreach (var entry in player.Roster.Entries.ToArray()) player.Roster.Remove(entry.Unit.Id, false);
                player.Items.Clear(); player.Items.AddRange(inventory);
                var unit = new PlanningUnit(++nextId, line.Id, 0, held);
                player.Roster.AddToBench(unit);
                return unit;
            }
            string context = rules.Id + "/" + glove.Id;
            var unit = Reset([glove.Id], [ordinary, glove.Id]);
            Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full", context + " rejects extra item");
            Check(equipment.Equip(player, 1, unit.Id, MatchPhase.Planning) == "full", context + " rejects second glove");
            Check(player.Items.Count == 2 && player.Roster.Entries.Single().Unit.Items.SequenceEqual([glove.Id]), context + " rejected equip preserves items");
            Check(!equipment.GiveFromBench(player, ordinary, unit.Id), context + " AI cannot add item");
            unit = Reset([ordinary], [glove.Id]);
            Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full", context + " glove needs empty unit");
            unit = Reset([], [glove.Id]);
            Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == glove.Id, context + " glove equips alone");
            unit = Reset([glove.Id], [ordinary]);
            player.Roster.UpdateUnit(unit with { Stage = 2 });
            player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0, 4), false, () => 10);
            var free = new PlanningUnit(++nextId, line.Id, 0, []);
            player.Roster.AddToBench(free);
            player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(1, 4), false, () => 10);
            new AiInventory(rules, equipment).Assign(player, 5, false);
            Check(player.Roster.At(UnitSpot.Board(1, 4))!.Items.Contains(ordinary) && player.Roster.At(UnitSpot.Board(0, 4))!.Items.Length == 1,
                context + " AI selects free holder");
            if (rules.Id.StartsWith("arcane/", StringComparison.Ordinal))
            {
                string contraband = rules.Items.First(i => i.Category == "contraband").Id;
                unit = Reset([glove.Id], [contraband]);
                player.Roster.UpdateUnit(unit with { Line = rules.Lines.First(l => l.Traits.Contains("SYN")).Id });
                new ArcanePlanning(state).AiTurn(player, equipment, MatchPhase.Planning);
                Check(player.Roster.Entries.Single().Unit.Items.Length == 1 && player.Items.Contains(contraband), context + " contraband AI respects glove");
            }
            if (glove.Ingredients is { Length: 2 } recipe)
            {
                unit = Reset([recipe[0], ordinary], [recipe[1]]);
                Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full", context + " cannot craft beside item");
                unit = Reset([recipe[0]], [recipe[1]]);
                Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == glove.Id, context + " crafts alone");
            }
            unit = Reset([ordinary, ordinary], [ordinary, ordinary]);
            Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == ordinary && player.Roster.Entries.Single().Unit.Items.Length == 3, context + " normal third item fits");
            Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full" && player.Items.Count == 1, context + " normal fourth rejected");
            if (rules.Items.FirstOrDefault(i => i.Consumable && i.Id.EndsWith("remover", StringComparison.Ordinal)) is { } remover)
            {
                unit = Reset([glove.Id], [remover.Id]);
                Check(new PlanningItems(state, new Hooks()).Equip(player, 0, unit.Id, MatchPhase.Planning) == remover.Id &&
                    player.Roster.Entries.Single().Unit.Items.Length == 0 && player.Items.Contains(glove.Id), context + " remover still returns glove");
            }
            if (rules.Items.FirstOrDefault(i => i.Consumable && i.Id.EndsWith("reforger", StringComparison.Ordinal)) is { } reforger)
            {
                unit = Reset([ordinary, ordinary, ordinary], [reforger.Id]);
                Check(new PlanningItems(state, new Hooks()).Equip(player, 0, unit.Id, MatchPhase.Planning) == reforger.Id &&
                    player.Roster.Entries.Single().Unit.Items.Length == 0 && player.Items.Count == 3, context + " reforger returns rolls to bench");
            }
            if (rules.Id.StartsWith("gizmos/", StringComparison.Ordinal) && glove.Category == "completed")
            {
                unit = Reset([glove.Id], ["giz_ockit", ordinary]);
                Check(new PlanningItems(state, new Hooks()).Equip(player, 0, unit.Id, MatchPhase.Planning) == "giz_ockit" &&
                    player.Roster.Entries.Single().Unit.Items.SequenceEqual(["giz_oc_thief_glove"]), context + " overclock upgrades glove");
                Check(equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full" && player.Items.Count == 1, context + " overclock retains slot cost");
            }
            if (rules.Loot!.Mode!.RadiantOf?.GetValueOrDefault(glove.Id) is { } radiant)
            {
                unit = Reset([glove.Id], [ordinary]);
                new ModeOfferRewards(state).GrantRadiant(player, radiant);
                Check(player.Roster.Entries.Single().Unit.Items.SequenceEqual([radiant]) && equipment.Equip(player, 0, unit.Id, MatchPhase.Planning) == "full",
                    context + " radiant retains slot cost");
            }
            foreach (bool gloveFirst in new[] { true, false })
            foreach (bool fullInventory in new[] { false, true })
            {
                int existing = fullInventory ? rules.Loot.ItemCap : 0;
                Reset([], Enumerable.Repeat(ordinary, existing).ToArray());
                foreach (var entry in player.Roster.Entries.ToArray()) player.Roster.Remove(entry.Unit.Id, false);
                for (int i = 0; i < line.Copies; i++) player.Roster.AddToBench(new(++nextId, line.Id, 0,
                    i == 0 ? [gloveFirst ? glove.Id : ordinary] : i == 1 ? [gloveFirst ? ordinary : glove.Id] : []));
                var evolved = player.Roster.Combine(false, player.GiveItem).Single();
                Check(evolved.Items.SequenceEqual([gloveFirst ? glove.Id : ordinary]), context + " merge retains first eligible item");
                Check(player.Items.Count == existing + 1 && player.Items[existing] == (gloveFirst ? ordinary : glove.Id), context + " merge refunds even with full bench");
                if (fullInventory)
                {
                    player.GiveItem(ordinary);
                    Check(player.Items.Count == existing + 1, context + " new drops stay capped");
                }
            }
            if (rules.Id.StartsWith("reckoning/", StringComparison.Ordinal))
            {
                unit = Reset(["rec_thief_gloves"], ["rec_sglove", "rec_blade"]);
                player.Roster.UpdateUnit(unit with { Line = "rec_zubamon" });
                player.Roster.Move(UnitSpot.Bench(0), UnitSpot.Board(0, 4), false, () => 10);
                new ReckoningPlanning(state).AiTurn(player, equipment, 2);
                Check(player.Items.SequenceEqual(["rec_sglove", "rec_blade"]), context + " Reckoning AI does not craft early for full glove carrier");
            }
            if (rules.Items.FirstOrDefault(i => i.Emblem is { } trait && !line.Traits.Contains(trait)) is { } emblem)
            {
                var combat = new CombatSimulation(rules, [new(line.Id, 0, 3, 4, Items: [glove.Id, emblem.Id])], [new(line.Id, 0, 3, 4)], 10);
                Check(!combat.Units.First(u => u.Team == 0).Traits.Contains(emblem.Emblem!), context + " rejected legacy emblem has no trait effect");
            }
            foreach (string[] held in new string[][] { [glove.Id], [glove.Id, ordinary, glove.Id], [ordinary, glove.Id, ordinary] })
            {
                var original = held.ToArray();
                var combat = new CombatSimulation(rules, [new(line.Id, 0, 3, 4, Items: held)], [new(line.Id, 0, 3, 4)], 10);
                var equipped = combat.Units.First(u => u.Team == 0).Items;
                Check(equipped.Count <= 3, context + " combat cap");
                if (held[0] == glove.Id) Check(equipped.Count == 3 && equipped.Count(id => id == glove.Id) == 1, context + " grants two random items");
                Check(held.SequenceEqual(original), context + " combat projection preserves source inventory");
            }
        }
        Console.WriteLine($"Item capacity: {checks:N0} checks across all catalog variants.");
        return checks;
    }
    private sealed class Hooks : IMatchItemHooks
    {
        public string UseConsumable(MatchPlayer player, int index, PlanningUnit unit) => "none";
        public string? EquipVeto(MatchPlayer player, PlanningUnit unit, string item, string? combined) => null;
        public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources) { }
    }
}
