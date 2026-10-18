using System.Text.Json;

namespace DigiTactics.Rules;

public sealed record ShopRollRequest(int Level, int Size, int Boost, double[]? Odds = null);
public sealed record ShopUnitOverride(int? Stage = null, Dictionary<string, int>? BonusTraits = null, JsonElement? Mods = null);
public sealed record ShopMetadata(int? Cost = null, string? Tag = null, ShopUnitOverride? Unit = null,
    [property: System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] string? Note = null);
public interface IShopRollHooks
{
    ShopRollRequest BeforeRoll(ShopRollRequest request);
    void Rolled(PlayerShop shop);
}

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
    private string?[] slots;
    private ShopMetadata?[] metadata;
    public PlayerShop(Ruleset rules, ShopPool pool, PlayerEconomy economy, PlanningRoster roster,
        Func<string, int, PlanningUnit> create, Action<string> giveItem)
    {
        this.rules = rules; this.pool = pool; this.economy = economy; this.roster = roster;
        this.create = create; this.giveItem = giveItem;
        lines = rules.Lines.ToDictionary(l => l.Id);
        slots = new string?[rules.ShopSize];
        metadata = new ShopMetadata?[rules.ShopSize];
    }
    public IReadOnlyList<string?> Slots => Array.AsReadOnly(slots);
    public IReadOnlyList<ShopMetadata?> Metadata => Array.AsReadOnly(metadata);
    public IShopRollHooks? RollHooks { get; set; }
    public bool Locked { get; set; }
    public int FreeRolls { get; set; }
    public int Cost(int slot) => (uint)slot < slots.Length && slots[slot] is { } line ? metadata[slot]?.Cost ?? lines[line].Tier : 0;

    public void SetMetadata(int slot, ShopMetadata? value)
    {
        if ((uint)slot >= slots.Length) throw new ArgumentOutOfRangeException(nameof(slot));
        metadata[slot] = value;
    }

    public bool ReplaceOffer(int slot, string line)
    {
        if ((uint)slot >= slots.Length || !lines.ContainsKey(line) || pool.Remaining(line) < 1 || roster.MaxedLines().Contains(line)) return false;
        if (slots[slot] is { } previous) pool.Return(previous);
        pool.Take(line); slots[slot] = line; metadata[slot] = null; return true;
    }

    public void Refresh(int levelBoost = 0)
    {
        ReleaseOffers();
        var request = new ShopRollRequest(economy.Level + levelBoost, slots.Length, levelBoost);
        if (RollHooks is { } hooks)
        {
            request = hooks.BeforeRoll(request);
            int size = Math.Clamp(request.Size, 1, 8);
            if (size != slots.Length) { slots = new string?[size]; metadata = new ShopMetadata?[size]; }
        }
        var excluded = roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++) slots[i] = pool.Roll(request.Level, excluded, request.Odds);
        RollHooks?.Rolled(this);
        ClearMaxed();
    }
    public void BeginRound()
    {
        FreeRolls = 0;
        if (!Locked) Refresh();
        ClearMaxed();
    }
    public bool RefreshMatchingTraits(IReadOnlyCollection<string> traits)
    {
        var excluded = roster.MaxedLines();
        var eligible = rules.Lines.Where(l => !excluded.Contains(l.Id) && l.Traits.Any(traits.Contains)).Select(l => l.Id).ToHashSet();
        // Existing reservations are returned only after availability is established.
        if (!eligible.Any(id => pool.Remaining(id) > 0)) return false;
        ReleaseOffers();
        for (int i = 0; i < slots.Length; i++) slots[i] = pool.RollMatching(economy.Level, eligible);
        return true;
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
    public bool Buy(int slot, bool boardLocked, Func<PlanningUnit, PlanningUnit>? onBuy = null,
        Action<PlanningUnit, IReadOnlyList<PlanningUnit>>? onCombine = null)
        => BuyWithEvolutions(slot, boardLocked, onBuy, onCombine) is not null;

    public IReadOnlyList<PlanningUnit>? BuyWithEvolutions(int slot, bool boardLocked, Func<PlanningUnit, PlanningUnit>? onBuy = null,
        Action<PlanningUnit, IReadOnlyList<PlanningUnit>>? onCombine = null)
    {
        if (CanBuy(slot, boardLocked) != PurchaseStatus.Ready) return null;
        string line = slots[slot]!;
        economy.Spend(Cost(slot));
        var special = metadata[slot]?.Unit;
        slots[slot] = null;
        metadata[slot] = null;
        var unit = create(line, 0);
        if (special is not null) unit = unit with { Stage = special.Stage ?? unit.Stage,
            BonusTraits = special.BonusTraits is { } bonus ? new(bonus) : unit.BonusTraits,
            Mods = special.Mods?.Clone() ?? unit.Mods };
        if (onBuy is not null) unit = onBuy(unit);
        var extra = roster.AddToBench(unit) ? null : unit;
        var evolved = roster.Combine(boardLocked, giveItem, extra, onCombine: onCombine, discardOverflow: true);
        ClearMaxed();
        return evolved;
    }
    public bool Sell(int uid, bool boardLocked, Action<PlanningUnit>? onSell = null)
    {
        var entry = roster.Entries.FirstOrDefault(e => e.Unit.Id == uid);
        if (entry is null || entry.Unit.NoSell || boardLocked && entry.Spot.IsBoard) return false;
        var line = rules.Lines.Concat(rules.ExtraLines).First(l => l.Id == entry.Unit.Line);
        if (line.NoSell) return false;
        roster.Remove(uid, boardLocked);
        economy.Credit(UnitValues.SellValue(line, entry.Unit.Stage));
        foreach (var item in entry.Unit.Items) giveItem(item);
        if (lines.ContainsKey(line.Id)) pool.Return(line.Id, UnitValues.Copies(line, entry.Unit.Stage));
        onSell?.Invoke(entry.Unit);
        return true;
    }
    public void ClearMaxed()
    {
        var excluded = roster.MaxedLines();
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] is { } line && excluded.Contains(line)) { pool.Return(line); slots[i] = null; metadata[i] = null; }
    }
    public void ReleaseOffers()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] is { } line) pool.Return(line);
            slots[i] = null;
            metadata[i] = null;
        }
    }
}
