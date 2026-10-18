namespace DigiTactics.Rules;

/// <summary>A round's battle, retaining participants and snapshots for viewing and settlement.</summary>
public sealed class MatchBattle
{
    public MatchBattle(MatchState state, int a, int? b, bool pve, bool ghost,
        ProjectedCombatSide sideA, ProjectedCombatSide sideB, uint seed, CombatSetup setup, int? teamA = null, int? teamB = null)
    {
        A = a; B = b; Pve = pve; Ghost = ghost; SideA = sideA; SideB = sideB; Seed = seed; Setup = setup;
        TeamA = teamA; TeamB = teamB;
        var timing = state.Rules.MatchCombat!;
        TraitsA = Counts(state.Rules, sideA); TraitsB = Counts(state.Rules, sideB);
        Combat = new(state.Rules, sideA.Units, sideB.Units, seed, sideA.Columns, timing.Overtime, timing.Limit, setup: setup);
    }
    private static Dictionary<string, int> Counts(Ruleset rules, ProjectedCombatSide side) => side.Effects.NoTraits ? [] :
        side.Effects.Counts is { } counts ? new(counts) : TraitRules.Count(rules, side.Units, side.Effects.Augments);
    public int A { get; }
    public int? B { get; }
    public int? TeamA { get; }
    public int? TeamB { get; }
    public bool Pve { get; }
    public bool Ghost { get; }
    public uint Seed { get; }
    public ProjectedCombatSide SideA { get; }
    public ProjectedCombatSide SideB { get; }
    public CombatSetup Setup { get; }
    public IReadOnlyDictionary<string, int> TraitsA { get; }
    public IReadOnlyDictionary<string, int> TraitsB { get; }
    public CombatSimulation Combat { get; }
    public ResolvedMatch Result() => ResolvedMatch.FromCombat(A, B, Pve, Ghost, Combat, TraitsA, TraitsB, TeamA, TeamB);
}
