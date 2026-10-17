namespace DigiTactics.Rules;

public sealed record ModeLootRules(string Policy, string[] Artifacts, string[] Components,
    string[] ShadowComponents, Dictionary<string, string> ShadowOf, double ShadowChance, Dictionary<string, string>? RadiantOf = null, Dictionary<string, string>? BaseOf = null);
public sealed class PlayerMechanics
{
    public double UnderlordsBeastKills { get; set; }
    public double UnderlordsDuel { get; set; }
    public int UnderlordsSilver { get; set; }
    public bool UnderlordsFreeRoll { get; set; }
    public bool FatesPartnerSeen { get; set; }
    public double DragonDoots { get; set; }
    public bool DragonStarShop { get; set; }
    public Dictionary<string, int> GalaxiesBounties { get; } = [];
    public ArcanePlanningState Arcane { get; } = new();
    public string[]? AutoChessLoot { get; set; }
    public bool? DragonTreasureWon { get; set; }
    public string? FatesPath { get; set; }
    public int FatesBlossom { get; set; }
    public int ColiseumCores { get; set; }
    public int? ColiseumCoreUnitId { get; set; }
    public int? ColiseumFormUnitId { get; set; }
    public ColiseumGambitState ColiseumGambit { get; } = new();
    public ColiseumPirateState ColiseumPirate { get; } = new();
    public string? ColiseumSupreme { get; set; }
    public int ColiseumFans { get; set; }
}

public sealed class ColiseumPirateState
{
    public int Count { get; set; }
    public string? LastReward { get; set; }
    public double AbilityPower { get; set; }
}

public sealed class ColiseumGambitState
{
    public double Gems { get; set; }
    public int Level { get; set; }
    public int Count { get; set; }
    public bool DoubleDown { get; set; }
    public bool Ready { get; set; }
    public int Kills { get; set; }
    public int KillTarget { get; set; }
}

internal sealed class ModePveRewards(MatchState state, MatchLoot loot)
{
    private ModeLootRules Mode => state.Rules.Loot!.Mode!;
    private double Random() => state.Random.Next();
    private string? Pick(IReadOnlyList<string> ids) => loot.Pick(ids);
    private static LootSpec Item(string? id, string kind = "blue") => new(kind, Items: id is null ? [] : [id]);
    private string? LowTierUnit() => Pick(state.Rules.Lines.Where(l => l.Tier <= 2 && state.Pool.Remaining(l.Id) > 0).Select(l => l.Id).ToArray());

    public IReadOnlyList<LootSpec> Generate(MatchPlayer player, RoundPosition round, bool won)
    {
        if (Mode.Policy == "autochess") return AutoChess(player, round, won);
        if (Mode.Policy is "underlords" or "underlords_ko")
        {
            int n = (round.Stage - 1) * 5 + round.Round;
            if (!won) return [new(Gold: n <= 3 ? 0 : 1)];
            List<LootSpec> rewards = [new(Gold: n <= 3 ? 1 : 2 + n / 15)];
            if (n == 3 && LowTierUnit() is { } unit) rewards.Add(new("gold", Unit: unit));
            return rewards;
        }
        if (Mode.Policy is not ("fates" or "arcane" or "dragonlands" or "reckoning" or "gizmos"))
            throw new InvalidDataException($"Unknown custom PvE reward policy: {Mode.Policy}.");
        List<LootSpec> result = [];
        int stage = round.Stage;
        if (stage == 1)
        {
            result.Add(Item(Mode.Policy == "reckoning" ? Pick(state.Rules.Loot!.StatComponents) : loot.RandomComponent()));
            if (won)
            {
                bool last = round.Round == (Mode.Policy == "gizmos" ? state.Rules.Schedule!.RoundsInStage(1) : 3);
                result.Add(new(Gold: last ? 2 : 1));
                if (last && LowTierUnit() is { } unit) result.Add(new("gold", Unit: unit));
            }
            return result;
        }
        if (Mode.Policy == "dragonlands") return Dragonlands(player, stage, won);
        result.Add(new(Gold: won ? 3 : 2));
        if (Mode.Policy == "reckoning")
        {
            result.Add(Item(Pick(Random() < Mode.ShadowChance ? Mode.ShadowComponents : state.Rules.Loot!.StatComponents)));
            if (won && stage >= 4)
            {
                string? item = loot.RandomCompleted();
                if (Random() < .25 && item is not null) item = Mode.ShadowOf.GetValueOrDefault(item, item);
                result.Add(Item(item, "gold"));
            }
            if (won && stage >= 3 && Random() < .25) result.Add(Item(Random() < .5 ? "rec_tome" : "rec_remover", "gold"));
            return result;
        }
        if (won && stage >= 4)
            result.Add(Item(Mode.Policy == "fates" && Random() < .12 ? Pick(Mode.Artifacts) : loot.RandomCompleted(), "gold"));
        if (Mode.Policy == "arcane" && won && stage >= 5 && Random() < .3) result.Add(Item(Pick(Mode.Artifacts), "gold"));
        else if (won && stage >= 4 && Random() < .2) result.Add(Item(loot.RandomEmblem(), "gold"));
        else result.Add(Item(loot.RandomComponent(stage >= 3)));
        if (won && stage >= 3 && Random() < .25)
        {
            var consumables = Mode.Policy == "arcane" ? new[] { "arc_remover", "arc_reforger", "arc_copier_small", "arc_copier" }
                : Mode.Policy == "gizmos" ? new[] { "giz_remover", "giz_reforger", "giz_copy" } : state.Rules.Loot!.Consumables;
            if (consumables.Length > 0) result.Add(Item(Pick(consumables), "gold"));
        }
        if (Mode.Policy == "gizmos" && won && stage >= 6 && Random() < .4) result.Add(Item("giz_ockit", "gold"));
        return result;
    }

    private IReadOnlyList<LootSpec> Dragonlands(MatchPlayer player, int stage, bool won)
    {
        if (stage == 4) { player.Mechanics.DragonTreasureWon = won; return [new(Gold: 3)]; }
        List<LootSpec> result = [];
        if (stage <= 3)
        {
            if (won) result.Add(new(Gold: 2));
            result.Add(Item(loot.RandomComponent(stage >= 3)));
            if (won && stage == 3) result.Add(Item(loot.RandomComponent()));
            return result;
        }
        result.Add(new(Gold: won ? 3 : 2));
        result.Add(Item(won ? loot.RandomCompleted() : loot.RandomComponent(), won ? "gold" : "blue"));
        if (won && Random() < .25) result.Add(Item(loot.RandomEmblem(player), "gold"));
        if (won && Random() < .2 && state.Rules.Loot!.Consumables.Length > 0)
            result.Add(Item(Pick(state.Rules.Loot.Consumables), "gold"));
        return result;
    }

    private IReadOnlyList<LootSpec> AutoChess(MatchPlayer player, RoundPosition round, bool won)
    {
        var schedule = state.Rules.Schedule!;
        int n = round.Round + (round.Stage == 1 ? 0 : schedule.Opening.Length + (round.Stage - 2) * schedule.Repeating.Length);
        if (state.Rules.Pace == "hyper") n = (int)Math.Floor(n * 1.25 + .5);
        if (!won)
        {
            player.Mechanics.AutoChessLoot = null;
            return RollAutoChess(n) is { } item ? [Item(item)] : [];
        }
        List<string> options = [];
        for (int guard = 0; options.Count < 3 && guard < 40; guard++)
            if (RollAutoChess(n) is { } item && !options.Contains(item)) options.Add(item);
        player.Mechanics.AutoChessLoot = options.ToArray();
        return [];
    }

    private string? RollAutoChess(int round)
    {
        if (round >= 15 && Random() < .05) return "ac_digiegg";
        int[] weights = round <= 3 ? [100, 0, 0, 0] : round <= 10 ? [60, 40, 0, 0]
            : round <= 15 ? [30, 50, 20, 0] : round <= 20 ? [20, 40, 35, 5]
            : round <= 25 ? [5, 35, 40, 20] : round <= 30 ? [0, 25, 45, 30] : [0, 10, 45, 45];
        double value = Random() * 100;
        int tier = 1;
        for (int i = 0; i < 4; i++) { if (value < weights[i]) { tier = i + 1; break; } value -= weights[i]; }
        if (tier == 4) return loot.RandomCompleted();
        var parts = Mode.Components.Where(id => state.Rules.Items.Single(i => i.Id == id).Tier == tier).ToArray();
        return Pick(parts.Length > 0 ? parts : Mode.Components);
    }
}
