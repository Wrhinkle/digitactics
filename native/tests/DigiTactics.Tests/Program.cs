using System.Text.Json;
using DigiTactics.Rules;

string root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("native");
if (args.Contains("--actions-only")) { ActionParity.Run(root); return; }
if (args.Contains("--meshes-only")) { MeshParity.Run(root); return; }
var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
if (args.Contains("--match-only")) { MatchStateParity.Run(root, catalog); return; }
if (args.Contains("--planning-only")) { PlanningParity.Run(root, catalog); PlanningShopParity.Run(root, catalog); return; }
if (args.Contains("--economy-only")) { EconomyParity.Run(root, catalog); return; }
if (args.Contains("--choreography-only")) { ChoreographyParity.Run(root, catalog); return; }
using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/parity.json")));
int assertions = MatchStateParity.Run(root, catalog) + EconomyParity.Run(root, catalog) + PlanningParity.Run(root, catalog) + PlanningShopParity.Run(root, catalog);
void Check(bool condition, string message)
{
    assertions++;
    if (!condition) throw new InvalidOperationException(message);
}

foreach (var trace in fixture.RootElement.GetProperty("rng").EnumerateArray())
{
    var random = new DeterministicRandom(trace.GetProperty("seed").GetUInt32());
    foreach (var expected in trace.GetProperty("expected").EnumerateArray())
        Check(random.Next() == expected.GetDouble(), "RNG diverged from JavaScript.");
}
foreach (var trace in fixture.RootElement.GetProperty("rolls").EnumerateArray())
{
    var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
    var random = new DeterministicRandom(trace.GetProperty("seed").GetUInt32());
    var pool = new ShopPool(rules, random);
    var excluded = trace.GetProperty("maxed").EnumerateArray().Select(e => e.GetString()!).ToHashSet();
    int roll = 0;
    foreach (var expected in trace.GetProperty("expected").EnumerateArray())
    {
        string? actual = pool.Roll(1 + roll % (rules.Odds.Length - 1), excluded);
        Check(actual == expected.GetString(), $"Shop parity failed in {rules.Id}, roll {roll}.");
        roll++;
    }
}
foreach (var rules in catalog.Variants)
{
    var line = rules.Lines.First(l => l.Tier == 1);
    var roster = new Roster();
    int copies = line.Copies * line.Copies;
    for (int i = 0; i < copies; i++) Check(roster.Add(line), $"Could not add copy {i} in {rules.Id}.");
    Check(roster.Units.Count == 1 && roster.Units[0].Stage == 2, "Three-star merge failed.");
    Check(roster.MaxedLines().Contains(line.Id), "Three-star exclusion absent.");
    Check(!new Roster().MaxedLines().Contains(line.Id), "Exclusion leaked to another player.");
    roster.Remove(roster.Units[0].Id);
    Check(!roster.MaxedLines().Contains(line.Id), "Sold three-star still excluded.");

    var random = new DeterministicRandom(1);
    var pool = new ShopPool(rules, random);
    var all = rules.Lines.Select(l => l.Id).ToHashSet();
    Check(pool.Roll(1, all) is null, "Exhausted eligible pool should produce an empty slot.");
    Check(pool.Remaining(line.Id) == rules.PoolSizes[line.Tier], "Excluded roll consumed inventory.");

    var session = new ShopSession(rules, 143);
    Check(!session.Buy(-1) && !session.Buy(session.Slots.Count), "Invalid shop slot accepted.");
    int initialGold = session.Gold;
    int slot = Enumerable.Range(0, session.Slots.Count).First(i => session.Slots[i] is not null);
    var purchase = session.Line(session.Slots[slot]!);
    Check(session.Buy(slot), "Valid purchase failed.");
    Check(session.Gold == initialGold - purchase.Price && session.Slots[slot] is null, "Purchase not atomic.");
    Check(!session.Buy(slot), "Duplicate purchase accepted.");
}
foreach (var trace in fixture.RootElement.GetProperty("grids").EnumerateArray())
{
    var grid = new HexGrid(trace.GetProperty("cols").GetInt32(), trace.GetProperty("rows").GetInt32());
    int pair = 0;
    foreach (var distance in trace.GetProperty("distances").EnumerateArray())
    {
        Check(grid[pair / grid.Count].DistanceTo(grid[pair % grid.Count]) == distance.GetInt32(), "Hex distance parity failed.");
        pair++;
    }
    foreach (var path in trace.GetProperty("paths").EnumerateArray())
    {
        var enemies = path.GetProperty("enemies").EnumerateArray().Select(e => grid[e.GetInt32()]).ToArray();
        var occupied = path.GetProperty("occupied").EnumerateArray().Select(e => e.GetBoolean()).ToArray();
        var result = grid.StepTowardEnemy(grid[path.GetProperty("start").GetInt32()], path.GetProperty("range").GetDouble(), enemies, occupied);
        var expected = path.GetProperty("expected");
        Check(expected.ValueKind == JsonValueKind.Null ? result is null : result is { } step &&
            grid.Index(step.Cell) == expected.GetProperty("step").GetInt32() && step.TargetIndex == expected.GetProperty("target").GetInt32(),
            "Combat movement/target tie-breaking diverged from JavaScript.");
    }
}
using var unitFixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/unit-stats.json")));
int forms = 0;
foreach (var trace in unitFixtures.RootElement.EnumerateArray())
{
    var rules = catalog.Variants.Single(v => v.Id == trace.GetProperty("variant").GetString());
    var line = rules.Lines.Concat(rules.ExtraLines).Single(l => l.Id == trace.GetProperty("line").GetString());
    int stage = trace.GetProperty("stage").GetInt32();
    var stats = UnitValues.Stats(line, stage);
    var expected = trace.GetProperty("stats");
    Check(stats.MaxHealth == expected.GetProperty("maxHp").GetDouble(), "Unit health scaling mismatch.");
    Check(stats.AttackDamage == expected.GetProperty("ad").GetDouble(), "Unit attack scaling mismatch.");
    Check(stats.AttackSpeed == expected.GetProperty("as").GetDouble(), "Unit attack speed mismatch.");
    Check(stats.Range == expected.GetProperty("range").GetDouble(), "Unit range mismatch.");
    Check(stats.Armor == expected.GetProperty("armor").GetDouble(), "Unit armor mismatch.");
    Check(stats.MagicResist == expected.GetProperty("mr").GetDouble(), "Unit magic resistance mismatch.");
    Check(stats.MaxMana == expected.GetProperty("maxMana").GetDouble(), "Unit mana cap mismatch.");
    Check(stats.StartMana == expected.GetProperty("mana").GetDouble(), "Unit initial mana mismatch.");
    Check(UnitValues.AbilityValue(line, stage) == trace.GetProperty("ability").GetDouble(), "Ability scaling mismatch.");
    Check(UnitValues.SellValue(line, stage) == trace.GetProperty("sell").GetInt32(), $"Sell value mismatch: {rules.Id}/{line.Id}/{stage}.");
    forms++;
}
using var damageFixtures = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/damage.json")));
foreach (var trace in damageFixtures.RootElement.EnumerateArray())
{
    var request = trace.GetProperty("request").Deserialize<DamageRequest>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    var actual = DamageRules.Resolve(request);
    var expected = trace.GetProperty("expected");
    // The reference records HP/shield differences, which can lose sub-ulp precision through subtraction.
    bool Close(double a, double b) => Math.Abs(a - b) <= 1e-10 * Math.Max(1, Math.Abs(b));
    Check(Close(actual.Amount, expected.GetProperty("amount").GetDouble()), "Damage amount mismatch.");
    Check(Close(actual.ShieldAbsorbed, expected.GetProperty("shieldAbsorbed").GetDouble()), "Shield absorption mismatch.");
    Check(Close(actual.HealthDamage, expected.GetProperty("healthDamage").GetDouble()), "Health damage mismatch.");
    Check(actual.ManaAfter == expected.GetProperty("manaAfter").GetDouble(), "Damage mana gain mismatch.");
}
assertions += ModelAssetParity.Run(root);
assertions += TextureParity.Run(root);
assertions += MaterialParity.Run(root);
assertions += ActionParity.Run(root);
assertions += ChoreographyParity.Run(root, catalog);
assertions += MotionParity.Run(root);
assertions += MeshParity.Run(root);
assertions += RigParity.Run(root);
assertions += CombatSceneParity.Run(root, catalog);
assertions += CombatEventParity.Run(root, catalog);
assertions += await CombatParity.Run(root, catalog);
Console.WriteLine($"Passed {assertions} native checks: reference battles, RNG, shop, movement, {forms} unit forms, 1,200 damage traces, Solo/Duos geometry, and roster/pool/purchase rules across {catalog.Variants.Length} variants.");
