using System.Text.Json;
using DigiTactics.Rules;

internal static class CombatParity
{
    public static async Task<int> Run(string root, ContentCatalog catalog)
    {
        using var input = File.OpenRead(Path.Combine(root, "tests/fixtures/combat.json.gz"));
        using var decompressed = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int count = 0, battles = 0;
        await foreach (var trace in JsonSerializer.DeserializeAsyncEnumerable<JsonElement>(decompressed))
        {
            battles++;
            string name = trace.GetProperty("name").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            bool hookProbe = trace.TryGetProperty("hookProbe", out var probe) && probe.GetBoolean();
            bool duos = trace.TryGetProperty("duos", out var duo) && duo.GetBoolean();
            bool production = trace.TryGetProperty("productionTraits", out var traits) && traits.GetBoolean();
            string[] augsA = trace.TryGetProperty("augsA", out var aa) ? aa.Deserialize<string[]>()! : [];
            string[] augsB = trace.TryGetProperty("augsB", out var ab) ? ab.Deserialize<string[]>()! : [];
            var entriesA = trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!;
            var entriesB = trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!;
            CombatSetup? setup = trace.TryGetProperty("setup", out var suppliedSetup) ? suppliedSetup.Deserialize<CombatSetup>(options)
                : production ? new(new(Augments: augsA), new(Augments: augsB)) : null;
            if (production)
                for (int team = 0; team < 2; team++)
                {
                    var side = team == 0 ? setup!.SideA : setup!.SideB;
                    var entries = team == 0 ? entriesA : entriesB;
                    var counts = side.NoTraits ? [] : side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments);
                    var expectedCounts = trace.GetProperty("traitCounts")[team].Deserialize<Dictionary<string, int>>()!;
                    count++;
                    if (!counts.SequenceEqual(expectedCounts)) throw new InvalidOperationException($"Trait counts/order mismatch {name}/{team}");
                }
            var hookLog = new List<string>();
            Action<CombatHookRegistry>? configure = hookProbe ? registry => HookProbe.Configure(registry, duos, hookLog) : null;
            if (trace.TryGetProperty("summonItemProbe", out var summonProbe) && summonProbe.GetBoolean())
            {
                bool noHooks = trace.GetProperty("summonNoHooks").GetBoolean();
                int stage = trace.GetProperty("summonStage").GetInt32();
                configure = registry => {
                    var context = registry.Context(CombatHookSource.Mechanic, "summon-item-probe");
                    registry.Add(context, CombatHook.CombatReady, (c, _) => {
                        for (int team = 0; team < 2; team++)
                        {
                            var source = c.Combat.Units.First(u => u.Team == team);
                            var cell = c.FreeCellNear(source) ?? throw new InvalidOperationException("No native summon test cell.");
                            c.Summon(team, source.Line.Id, stage, cell, new(["fat_deathblade", "fat_art_manazane", "fat_zzrot"], NoHooks: noHooks));
                        }
                    });
                };
            }
            if (trace.TryGetProperty("observerSummonProbe", out var observerProbe) && observerProbe.GetBoolean())
                configure = registry => {
                    bool added = false;
                    var context = registry.Context(CombatHookSource.Mechanic, "observer-summon");
                    registry.Add(context, CombatHook.AnyCast, (c, _) => {
                        if (added) return; added = true;
                        var source = c.Combat.Units[0];
                        var cell = c.FreeCellNear(source) ?? throw new InvalidOperationException("Missing observer probe spawn cell");
                        c.Summon(source.Team, source.Line.Id, 0, cell, new(Items: ["rec_s_mana_zap"]));
                    });
                };
            var combat = new CombatSimulation(rules, entriesA, entriesB, trace.GetProperty("seed").GetUInt32(), trace.GetProperty("cols").GetInt32(),
                trace.TryGetProperty("overtime", out var ot) ? ot.GetDouble() : 30,
                trace.TryGetProperty("timeLimit", out var limit) ? limit.GetDouble() : 45, configure, duos, setup);
            int tick = 0;
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                int targetTick = sample.GetProperty("tick").GetInt32();
                while (tick < targetTick) { combat.Step(); tick++; }
                count++;
                if (combat.Units.Count != sample.GetProperty("units").GetArrayLength()) throw new InvalidOperationException($"Unit count mismatch {name}/{tick}");
                foreach (var expected in sample.GetProperty("units").EnumerateArray())
                {
                    var unit = combat.Units[expected.GetProperty("id").GetInt32()];
                    void Compare(string field, double actual)
                    {
                        count++;
                        var encoded = expected.GetProperty(field);
                        double value = encoded.ValueKind == JsonValueKind.String ? encoded.GetString() switch {
                            "Infinity" => double.PositiveInfinity, "-Infinity" => double.NegativeInfinity, "NaN" => double.NaN,
                            _ => throw new InvalidDataException($"Invalid numeric fixture {name}/{field}")
                        } : encoded.GetDouble();
                        if (!double.IsFinite(value) || !double.IsFinite(actual)) {
                            if (!actual.Equals(value)) throw new InvalidOperationException($"Non-finite combat state {name}/{tick}/{unit.Id}/{field}: expected {value}, got {actual}");
                            return;
                        }
                        if (Math.Abs(actual - value) > 1e-8 * Math.Max(1, Math.Abs(value)))
                            throw new InvalidOperationException($"Combat {name}, tick {tick}, unit {unit.Id}: {field} expected {value:R}, got {actual:R}");
                    }
                    count++;
                    if (unit.IsPiloting != expected.GetProperty("piloting").GetBoolean()) throw new InvalidOperationException($"Pilot state mismatch {name}/{tick}");
                    Compare("stage", unit.Stage); Compare("blossomKills", unit.BlossomKills); Compare("timeRune", unit.TimeRune); Compare("combatGold", unit.CombatGold); Compare("range", unit.Range);
                    count++;
                    if (unit.Summoned != expected.GetProperty("summoned").GetBoolean()) throw new InvalidOperationException($"Summon state mismatch {name}/{tick}");
                    Compare("hp", unit.Health); Compare("mana", unit.Mana); Compare("shield", unit.Shield);
                    Compare("c", unit.Cell.Column); Compare("r", unit.Cell.Row);
                    Compare("atkCd", unit.AttackCooldown); Compare("moveCd", unit.MoveCooldown);
                    Compare("stun", unit.Stun); Compare("lock", unit.CastLock);
                    Compare("dealt", unit.Dealt); Compare("taken", unit.Taken); Compare("healed", unit.Healed); Compare("nAtk", unit.Attacks);
                    Compare("maxHp", unit.MaxHealth); Compare("ad", unit.AttackDamage); Compare("as", unit.AttackSpeed);
                    Compare("ap", unit.AbilityPower); Compare("armor", unit.Armor); Compare("mr", unit.MagicResist); Compare("maxMana", unit.MaxMana);
                    count++;
                    if (!unit.Items.SequenceEqual(expected.GetProperty("items").EnumerateArray().Select(x => x.GetString()!)))
                        throw new InvalidOperationException($"Combat {name}: equipment/RNG mismatch.");
                    count += 2;
                    if (unit.Alive != expected.GetProperty("alive").GetBoolean()) throw new InvalidOperationException($"Combat {name}: life state mismatch.");
                    int? targetId = expected.GetProperty("target").ValueKind == JsonValueKind.Null ? null : expected.GetProperty("target").GetInt32();
                    if (unit.Target?.Id != targetId) throw new InvalidOperationException($"Combat {name}, tick {tick}, unit {unit.Id}: target mismatch.");
                }
            }
            count++;
            if (!combat.Done || combat.Winner != trace.GetProperty("winner").GetInt32() || combat.Time != trace.GetProperty("time").GetDouble())
                throw new InvalidOperationException($"Combat {name}: outcome/time mismatch.");
            if (hookProbe)
            {
                var expectedLog = trace.GetProperty("hookLog").EnumerateArray().Select(x => x.GetString()!).ToArray();
                count++;
                if (!hookLog.SequenceEqual(expectedLog))
                {
                    int mismatch = Enumerable.Range(0, Math.Min(hookLog.Count, expectedLog.Length)).FirstOrDefault(i => hookLog[i] != expectedLog[i], -1);
                    throw new InvalidOperationException($"Hook order {name}: expected {expectedLog.Length}, actual {hookLog.Count}; first mismatch {mismatch}: {(mismatch < 0 ? "length" : expectedLog[mismatch] + " / " + hookLog[mismatch])}");
                }
            }
            if (trace.TryGetProperty("meeps", out var meeps))
                for (int team = 0; team < 2; team++)
                { count++; if (combat.MeepCount(team) != meeps[team].GetInt32()) throw new InvalidOperationException($"Meep count mismatch {name}"); }
            if (trace.TryGetProperty("pirateKills", out var pirateKills))
                for (int team = 0; team < 2; team++)
                { count++; if (combat.PirateKillCount(team) != pirateKills[team].GetInt32()) throw new InvalidOperationException($"Pirate count mismatch {name}"); }
            if (trace.TryGetProperty("dragonRewards", out var dragonRewards))
                for (int team = 0; team < 2; team++) {
                    var actual = combat.DragonlandsRewards(team); var expected = dragonRewards[team]; count += 3;
                    if (actual.Gold != expected.GetProperty("gold").GetInt32() || actual.Axe != expected.GetProperty("axe").GetInt32() || actual.Investor != expected.GetProperty("investor").GetInt32())
                        throw new InvalidOperationException($"Dragonlands rewards mismatch {name}/{team}");
                    if (expected.TryGetProperty("ctKills", out var trainingKills)) {
                        count++;
                        if (actual.CtKills != trainingKills.GetInt32()) throw new InvalidOperationException($"Dragonlands training counter mismatch {name}/{team}");
                    }
                    if (expected.TryGetProperty("lagoon", out var lagoon)) {
                        count += 3;
                        if (actual.Lagoon != lagoon.GetInt32() || actual.Doots != expected.GetProperty("doots").GetInt32() || actual.Wish != expected.GetProperty("wish").GetInt32())
                            throw new InvalidOperationException($"Dragonlands trait counters mismatch {name}/{team}");
                    }
                }
            if (trace.TryGetProperty("arcaneRewards", out var arcaneRewards))
                for (int team = 0; team < 2; team++) {
                    count += 2;
                    var actual = combat.ArcaneRewards(team); var expected = arcaneRewards[team];
                    if (actual.ConquestKills != expected.GetProperty("conquestKills").GetInt32() || actual.Coins != expected.GetProperty("coins").GetInt32())
                        throw new InvalidOperationException($"Arcane rewards mismatch {name}/{team}");
                    if (expected.TryGetProperty("bountyGold", out var bounty)) {
                        count++; if (actual.BountyGold != bounty.GetInt32()) throw new InvalidOperationException($"Arcane bounty mismatch {name}/{team}");
                    }
                }
            double completedTime = combat.Time;
            combat.Step();
            count++;
            if (combat.Time != completedTime) throw new InvalidOperationException("Completed battle advanced another tick.");
        }
        Console.WriteLine($"Passed {battles} reference battles ({count} state comparisons).");
        return count;
    }
}
