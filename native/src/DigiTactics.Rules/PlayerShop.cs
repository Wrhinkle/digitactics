namespace DigiTactics.Rules;

public enum PurchaseStatus { Empty, Gold, Bench, Ready }

/// <summary>Offers reserve shared pool copies; max-star exclusions remain player-specific.</summary>
public sealed class PlayerShop
{
    private readonly Ruleset rules;
    private readonly ShopPool pool;
    private readonly PlayerEconomy economy;
    private readonly PlanningRoster roster;
    private readonly Func<string, int, PlanningUnit> create;
    private readonly Action<string> giveItem;
    private readonly Dictionary<string, UnitLine> lines;
    private readonly string?[] slots;
    public PlayerShop(Ruleset rules, ShopPool pool, PlayerEconomy economy, PlanningRoster roster,
        Func<string, int, PlanningUnit> create, Action<string> giveItem)
    {
        this.rules = rules; this.pool = pool; this.economy = economy; this.roster = roster;
        this.create = create; this.giveItem = giveItem;
        lines = rules.Lines.ToDictionary(l => l.Id);
        slots = new string?[rules.ShopSize];
    }
    public IReadOnlyList<string?> Slots => Array.AsReadOnly(slots);
    public bool Locked { get; set; }
    public int FreeRolls { get; set; }
    public int Cost(int slot) => (uint)slot < slots.Length && slots[slot] is { } line ? lines[line].Tier : 0;

    public void Refresh(int levelBoost = 0)
    {
        ReleaseOffers();
        var excluded = roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++) slots[i] = pool.Roll(economy.Level + levelBoost, excluded);
    }
    public void BeginRound()
    {
        FreeRolls = 0;
        if (!Locked) Refresh();
        ClearMaxed();
    }
    public bool Reroll()
    {
        if (FreeRolls > 0) FreeRolls--;
        else if (!economy.Spend(rules.RerollCost)) return false;
        Refresh();
        return true;
    }
    public PurchaseStatus CanBuy(int slot, bool boardLocked)
    {
        if ((uint)slot >= slots.Length || slots[slot] is not { } line || roster.MaxedLines().Contains(line)) return PurchaseStatus.Empty;
        if (economy.Gold < Cost(slot)) return PurchaseStatus.Gold;
        return roster.CanAcceptPurchase(line, boardLocked) ? PurchaseStatus.Ready : PurchaseStatus.Bench;
    }
    public bool Buy(int slot, bool boardLocked)
    {
        if (CanBuy(slot, boardLocked) != PurchaseStatus.Ready) return false;
        string line = slots[slot]!;
        economy.Spend(Cost(slot));
        slots[slot] = null;
        var unit = create(line, 0);
        var extra = roster.AddToBench(unit) ? null : unit;
        roster.Combine(boardLocked, giveItem, extra);
        ClearMaxed();
        return true;
    }
    public bool Sell(int uid, bool boardLocked)
    {
        var entry = roster.Entries.FirstOrDefault(e => e.Unit.Id == uid);
        if (entry is null || entry.Unit.NoSell || boardLocked && entry.Spot.IsBoard) return false;
        var line = rules.Lines.Concat(rules.ExtraLines).First(l => l.Id == entry.Unit.Line);
        if (line.NoSell) return false;
        roster.Remove(uid, boardLocked);
        economy.Credit(UnitValues.SellValue(line, entry.Unit.Stage));
        foreach (var item in entry.Unit.Items) giveItem(item);
        if (lines.ContainsKey(line.Id)) pool.Return(line.Id, UnitValues.Copies(line, entry.Unit.Stage));
        return true;
    }
    public void ClearMaxed()
    {
        var excluded = roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] is { } line && excluded.Contains(line)) { pool.Return(line); slots[i] = null; }
    }
    public void ReleaseOffers()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] is { } line) pool.Return(line);
            slots[i] = null;
        }
    }
}
