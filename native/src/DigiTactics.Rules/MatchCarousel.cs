namespace DigiTactics.Rules;

public sealed record CarouselRules(int[][] TiersByStage, string ItemPolicy, double ShadowChance);
public sealed record CarouselOption(string Line, string? Item, string? PickedBy = null);
public interface IMatchCarouselHooks
{
    void Combined(MatchPlayer player, PlanningUnit unit, IReadOnlyList<PlanningUnit> sources);
    void PartnerCombined(MatchPlayer partner, MatchPlayer player, PlanningUnit unit);
}

/// <summary>Carousel reservations, ordered picks and remaining-copy release share match ownership.</summary>
public sealed class MatchCarousel(MatchState state, IMatchCarouselHooks hooks)
{
    private readonly MatchLoot loot = new(state);
    private readonly List<CarouselOption> options = [];
    private MatchPlayer[] order = [];
    private int index;
    public bool Active { get; private set; }
    public IReadOnlyList<CarouselOption> Options => options.AsReadOnly();
    public IReadOnlyList<MatchPlayer> Order => Array.AsReadOnly(order);
    public int Index => index;
    public MatchPlayer? Turn => Active && index < order.Length ? order[index] : null;
    public bool Done => Turn is null;
    public void Start(int stage, MatchPhase phase)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stage, 1);
        var rules = state.Rules.Carousel!; var tiers = rules.TiersByStage[Math.Min(stage - 1, rules.TiersByStage.Length - 1)];
        int low = tiers.Min(); var lines = state.Rules.Lines.Where(l => tiers.Contains(l.Tier)).ToArray();
        options.Clear();
        for (int i = 0; i < 9; i++)
        {
            var available = lines.Where(l => state.Pool.Remaining(l.Id) > 0).ToArray(); if (available.Length == 0) break;
            var cheapest = available.Where(l => l.Tier == low).ToArray();
            var pool = i < 7 && cheapest.Length > 0 ? cheapest : available;
            var line = pool[(int)(state.Random.Next() * pool.Length)]; state.Pool.Take(line.Id);
            options.Add(new(line.Id, Item(stage)));
        }
        order = state.Players.Where(p => p.Alive).ToArray();
        for (int i = order.Length - 1; i > 0; i--) { int j = (int)(state.Random.Next() * (i + 1)); (order[i], order[j]) = (order[j], order[i]); }
        if (stage > 1) order = order.OrderBy(p => p.Health).ToArray();
        index = 0; Active = true; Step(phase);
    }
    private string? Item(int stage)
    {
        var rules = state.Rules.Carousel!;
        switch (rules.ItemPolicy)
        {
            case "completed": return loot.RandomCompleted();
            case "component": return loot.RandomComponent();
            case "normal": return stage >= 5 ? loot.RandomCompleted() : loot.RandomComponent(stage >= 3);
            case "reckoning":
                if (stage <= 1) return loot.RandomComponent();
                if (stage >= 5)
                {
                    string? id = loot.RandomCompleted();
                    return state.Random.Next() < rules.ShadowChance * .5 && id is not null ? state.Rules.Loot!.Mode!.ShadowOf.GetValueOrDefault(id, id) : id;
                }
                return state.Random.Next() < rules.ShadowChance ? loot.Pick(state.Rules.Loot!.Mode!.ShadowComponents) : loot.RandomComponent();
            default: throw new InvalidDataException("Unknown carousel item policy: " + rules.ItemPolicy);
        }
    }
    public void Step(MatchPhase phase)
    {
        while (Turn is { IsHuman: false } player)
        {
            int best = -1; double high = double.NegativeInfinity;
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i]; if (!string.IsNullOrEmpty(option.PickedBy)) continue;
                int tier = state.Rules.Lines.Single(l => l.Id == option.Line).Tier;
                double score = tier * 2 + player.Roster.Entries.Count(e => e.Unit.Line == option.Line) * 3 + state.Random.Next();
                if (score > high) { high = score; best = i; }
            }
            if (best < 0) { index++; continue; }
            Pick(player, best, phase);
        }
    }
    public bool Pick(MatchPlayer player, int slot, MatchPhase phase)
    {
        if ((uint)slot >= options.Count || !string.IsNullOrEmpty(options[slot].PickedBy) || Turn != player) return false;
        var option = options[slot]; options[slot] = option with { PickedBy = player.Name };
        if (player.Roster.FreeBenchSlot >= 0) player.Roster.AddToBench(state.CreateUnit(option.Line, 0));
        else { player.Economy.Credit(state.Rules.Lines.Single(l => l.Id == option.Line).Tier); state.Pool.Return(option.Line); }
        player.GiveItem(option.Item!); index++;
        player.Roster.Combine(phase == MatchPhase.Combat && player.IsHuman, player.GiveItem, onCombine: (unit, sources) =>
        {
            hooks.Combined(player, unit, sources);
            if (state.Duos && player.Team is { } team && state.Teams[team].Members.FirstOrDefault(p => p != player && p.Alive) is { } partner)
                hooks.PartnerCombined(partner, player, player.Roster.Entries.FirstOrDefault(e => e.Unit.Id == unit.Id)?.Unit ?? unit);
        });
        player.Shop.ClearMaxed(); return true;
    }
    public void Finish(MatchPhase phase)
    {
        if (!Active) return;
        while (Turn is { } player)
        {
            int free = options.FindIndex(o => string.IsNullOrEmpty(o.PickedBy));
            if (free < 0) index++; else Pick(player, free, phase);
        }
        foreach (var option in options.Where(o => string.IsNullOrEmpty(o.PickedBy))) state.Pool.Return(option.Line);
        Active = false; options.Clear(); order = []; index = 0;
    }
}
