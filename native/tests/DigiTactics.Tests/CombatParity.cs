using System.Text.Json;
using DigiTactics.Rules;

internal static class CombatParity
{
    public static int Run(string root, ContentCatalog catalog)
    {
        using var fixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/combat.json")));
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        int count = 0;
        foreach (var trace in fixtures.RootElement.EnumerateArray())
        {
            string name = trace.GetProperty("name").GetString()!;
            var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
            var combat = new CombatSimulation(rules, trace.GetProperty("sideA").Deserialize<CombatEntry[]>(options)!,
                trace.GetProperty("sideB").Deserialize<CombatEntry[]>(options)!, trace.GetProperty("seed").GetUInt32(), trace.GetProperty("cols").GetInt32(),
                trace.TryGetProperty("overtime", out var ot) ? ot.GetDouble() : 30,
                trace.TryGetProperty("timeLimit", out var limit) ? limit.GetDouble() : 45);
            int tick = 0;
            foreach (var sample in trace.GetProperty("samples").EnumerateArray())
            {
                int targetTick = sample.GetProperty("tick").GetInt32();
                while (tick < targetTick) { combat.Step(); tick++; }
                foreach (var expected in sample.GetProperty("units").EnumerateArray())
                {
                    var unit = combat.Units[expected.GetProperty("id").GetInt32()];
                    void Compare(string field, double actual)
                    {
                        count++;
                        double value = expected.GetProperty(field).GetDouble();
                        if (Math.Abs(actual - value) > 1e-8 * Math.Max(1, Math.Abs(value)))
                            throw new InvalidOperationException($"Combat {name}, tick {tick}, unit {unit.Id}: {field} expected {value:R}, got {actual:R}");
                    }
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
            double completedTime = combat.Time;
            combat.Step();
            count++;
            if (combat.Time != completedTime) throw new InvalidOperationException("Completed battle advanced another tick.");
        }
        Console.WriteLine($"Passed {fixtures.RootElement.GetArrayLength()} reference battles ({count} state comparisons).");
        return count;
    }
}
