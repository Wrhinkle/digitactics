using System.Text.Json;
namespace DigiTactics.Rules;

public sealed class GizmosPlanningState
{
    public int GoldStart { get; set; }
    public bool CalculatedRoll { get; set; }
    public int Junk { get; set; }
    public GizmosChest? Chest { get; set; }
}
public sealed class GizmosChest
{
    public int Gold { get; set; }
    public List<string> Items { get; } = [];
    public int Shut { get; set; }
}

/// <summary>Persistent Gates, Pirate chest, upgrade rewards and Glutton roster effects.</summary>
public sealed class GizmosPlanning(MatchState state) : IMatchShopHooks
{
    private readonly MatchLoot loot = new(state);
    private int Level(string id, IReadOnlyDictionary<string, int> counts) => TraitRules.Level(state.Rules.Traits.Single(t => t.Id == id), counts.GetValueOrDefault(id));

    public void AugmentPicked(MatchPlayer player, string id)
    {
        switch (id)
        {
            case "giz_a_spare": player.Economy.Credit(10); break;
            case "giz_a_parts": player.GiveItem(loot.RandomComponent()!); player.GiveItem(loot.RandomComponent()!); break;
            case "giz_a_bank": player.Economy.Credit(18); break;
            case "giz_a_chip": player.GiveItem("giz_chip"); player.GiveItem(loot.RandomComponent()!); break;
            case "giz_a_crestkit": player.GiveItem(loot.RandomEmblem(player)!); break;
            case "giz_a_trade": case "giz_a_highend": player.Shop.FreeRolls++; break;
            case "giz_a_salvage": player.GiveItem(loot.RandomCompleted()!); break;
            case "giz_a_jackpot": player.Economy.Credit(30); break;
            case "giz_a_grabbag": player.GiveItem(loot.RandomCompleted()!); player.GiveItem(loot.RandomCompleted()!); break;
            case "giz_a_ticket": player.Economy.Credit(5); break;
            case "giz_a_windfall": player.Economy.Credit(18 * player.Augments.Count); break;
            case "giz_a_forge": player.GiveItem(loot.Pick(state.Rules.Items.Where(i => i.Category == "radiant").Select(i => i.Id).ToArray())!); break;
            case "giz_a_band": player.GiveItem("giz_thief_glove"); player.GiveItem(loot.RandomComponent()!); break;
        }
    }

    public void RoundStart(MatchPlayer player, RoundPosition round, string roundType)
    {
        var mechanic = player.Mechanics.Gizmos;
        foreach (string id in player.Augments)
        {
            if (id == "giz_a_calc" && mechanic.CalculatedRoll) { player.Shop.FreeRolls++; mechanic.CalculatedRoll = false; }
            if (id is "giz_a_trade" or "giz_a_highend") player.Shop.FreeRolls++;
            if (id == "giz_a_reserves") mechanic.GoldStart = player.Economy.Gold;
        }
        int gate = Array.FindIndex(state.Rules.Offers!.Schedule, o => o.Kind == "upgrade" && o.Stage == round.Stage && o.Round == round.Round);
        if (gate >= 0)
        {
            int tier = state.GizmosGates is { } gates ? gate < gates.Length ? gates[gate] : 0 : 2;
            if (tier == 1) { player.GiveItem(loot.RandomComponent()!); player.Economy.Credit(4); }
            else if (tier == 2) player.Economy.Credit(2);
        }
        if (roundType != "pvp") return;
        var board = player.Roster.Board.Select(e => new CombatEntry(e.Unit.Line, e.Unit.Stage, e.Spot.Column, e.Spot.Row, e.Unit.Id, Items: e.Unit.Items, BonusTraits: e.Unit.BonusTraits));
        int level = Level("PIR", TraitRules.Count(state.Rules, board, player.Augments.ToArray()));
        if (level == 0) return;
        var chest = mechanic.Chest ??= new();
        int sum = chest.Shut + (level >= 2 ? 2 : 0);
        for (int i = 0; i < 2; i++) sum += 1 + (int)(state.Random.Next() * 6);
        if (sum <= 6) chest.Gold++;
        else if (sum <= 9) chest.Gold += 2;
        else if (sum <= 11) { if (chest.Items.Count < 4) chest.Items.Add(loot.RandomComponent()!); else chest.Gold += 3; }
        else if (sum <= 13) { chest.Gold += 2; if (chest.Items.Count < 4) chest.Items.Add(loot.RandomComponent()!); }
        else if (chest.Items.Count < 4) chest.Items.Add(loot.RandomCompleted()!);
        else chest.Gold += 5;
    }

    public int RoundEnd(RoundEndContext context, int stage, Action<MatchPlayer> combine)
    {
        ArgumentNullException.ThrowIfNull(combine);
        var player = context.Player; var mechanic = player.Mechanics.Gizmos;
        if (!context.Pve && Level("PET", context.Traits) > 0 && player.Roster.FreeBenchSlot >= 0)
        {
            int maxTier = stage <= 3 ? 2 : stage <= 5 ? 3 : stage <= 6 ? 4 : 5;
            var eligible = state.Rules.Lines.Where(l => l.Traits.Contains("PET") && l.Tier <= maxTier && state.Pool.Remaining(l.Id) > 0).Select(l => l.Id).ToArray();
            if (loot.Pick(eligible) is { } line)
            {
                state.Pool.Take(line); player.Roster.AddToBench(state.CreateUnit(line, 0)); combine(player);
            }
        }
        int gold = 0;
        foreach (string id in player.Augments)
        {
            if (id == "giz_a_calc" && !context.Won) { gold += 3; mechanic.CalculatedRoll = true; }
            if (id == "giz_a_hyper" && player.Economy.Gold + gold < 10) gold += 2;
            if (id == "giz_a_junkyard" && context.Match.Combat is not null && Level("JUNK", context.Team == 0 ? context.Match.TraitsA : context.Match.TraitsB) > 0)
            {
                mechanic.Junk++;
                if (mechanic.Junk % 3 == 0) player.GiveItem(loot.RandomComponent()!);
            }
        }
        if (mechanic.Chest is not { } chest || context.Pve) return gold;
        if (context.Won && (chest.Gold != 0 || chest.Items.Count > 0))
        {
            gold += chest.Gold;
            foreach (string item in chest.Items) player.GiveItem(item);
            mechanic.Chest = new();
        }
        else if (!context.Won) chest.Shut = Math.Min(6, chest.Shut + 1);
        return gold;
    }

    private static double Mod(PlanningUnit unit, string key) => unit.Mods is { } m && m.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number ? value.GetDouble() : 0;
    private static PlanningUnit Glutton(PlanningUnit unit, double hp, double ap)
    {
        var mods = unit.Mods?.Deserialize<Dictionary<string, JsonElement>>() ?? [];
        mods["glutHp"] = JsonSerializer.SerializeToElement(hp); mods["glutAp"] = JsonSerializer.SerializeToElement(ap);
        return unit with { Mods = JsonSerializer.SerializeToElement(mods) };
    }
    public PlanningUnit Bought(MatchPlayer player, PlanningUnit unit, int slot, MatchPhase phase) => unit;
    public void Sold(MatchPlayer player, PlanningUnit unit)
    {
        if (unit.Line == "giz_gekomon" || player.Roster.Entries.Select(e => e.Unit).FirstOrDefault(u => u.Line == "giz_gekomon") is not { } holder) return;
        double value = UnitValues.Value(state.Rules.Lines.Single(l => l.Id == unit.Line), unit.Stage);
        player.Roster.UpdateUnit(Glutton(holder, Math.Min(1200, Mod(holder, "glutHp") + 20 * value), Math.Min(60, Mod(holder, "glutAp") + value)));
    }
    public void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources)
    {
        if (unit.Line != "giz_gekomon") return;
        double hp = sources.Select(u => Mod(u, "glutHp")).Append(0).Max(), ap = sources.Select(u => Mod(u, "glutAp")).Append(0).Max();
        if (hp != 0 || ap != 0) player.Roster.UpdateUnit(Glutton(unit, hp, ap));
    }
}
