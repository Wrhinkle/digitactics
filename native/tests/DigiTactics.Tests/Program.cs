using System.Text.Json;
using DigiTactics.Rules;

string root = args.Length > 0 ? Path.GetFullPath(args[0]) : Path.GetFullPath("native");
var catalog = ContentCatalog.Load(Path.Combine(root, "content/catalog.json"));
using var fixture = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "tests/fixtures/parity.json")));
int assertions = 0;
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
        var result = grid.StepTowardEnemy(grid[path.GetProperty("start").GetInt32()], path.GetProperty("range").GetInt32(), enemies, occupied);
        var expected = path.GetProperty("expected");
        Check(expected.ValueKind == JsonValueKind.Null ? result is null : result is { } step &&
            grid.Index(step.Cell) == expected.GetProperty("step").GetInt32() && step.TargetIndex == expected.GetProperty("target").GetInt32(),
            "Combat movement/target tie-breaking diverged from JavaScript.");
    }
}
Console.WriteLine($"Passed {assertions} native checks: RNG, 88 shop traces, 200 movement traces, Solo/Duos hex geometry, and roster/pool/purchase rules across {catalog.Variants.Length} variants.");
