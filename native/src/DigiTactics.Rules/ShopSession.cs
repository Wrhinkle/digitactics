namespace DigiTactics.Rules;

/// <summary>Interactive migration sandbox for the ported shop and roster rules.</summary>
public sealed class ShopSession
{
    public Ruleset Rules { get; }
    public Roster Roster { get; } = new();
    public int Gold { get; private set; } = 50;
    public int Level { get; private set; } = 5;
    private readonly ShopPool pool;
    private readonly string?[] slots;
    private readonly Dictionary<string, UnitLine> lines;
    public IReadOnlyList<string?> Slots => Array.AsReadOnly(slots);
    public UnitLine Line(string id) => lines[id];

    public ShopSession(Ruleset rules, uint seed)
    {
        Rules = rules;
        lines = rules.Lines.ToDictionary(l => l.Id);
        pool = new(rules, new(seed));
        slots = new string?[rules.ShopSize];
        Refresh();
    }

    public bool Reroll()
    {
        if (Gold < Rules.RerollCost) return false;
        Gold -= Rules.RerollCost;
        Refresh();
        return true;
    }

    private void Refresh()
    {
        foreach (var id in slots) if (id is not null) pool.Return(id);
        var excluded = Roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++) slots[i] = pool.Roll(Level, excluded);
    }

    public bool Buy(int slot)
    {
        if ((uint)slot >= (uint)slots.Length || slots[slot] is not { } id) return false;
        var line = lines[id];
        if (Gold < line.Price || Roster.MaxedLines().Contains(id) || !Roster.Add(line)) return false;
        Gold -= line.Price;
        slots[slot] = null;
        var excluded = Roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] is { } offer && excluded.Contains(offer))
            { pool.Return(offer); slots[i] = null; }
        return true;
    }

    public bool Sell(int unitId)
    {
        var unit = Roster.Remove(unitId);
        if (unit is null) return false;
        var line = lines[unit.Line];
        int copies = (int)Math.Pow(line.Copies, unit.Stage);
        // Sandbox liquidation returns purchase cost; set-specific sell hooks are not migrated yet.
        Gold += line.Price * copies;
        pool.Return(unit.Line, copies);
        return true;
    }
}
