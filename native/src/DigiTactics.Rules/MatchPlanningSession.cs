namespace DigiTactics.Rules;

/// <summary>Owns the match loop, from planning through native combat and settlement.</summary>
public sealed class MatchPlanningSession
{
    public MatchPlanningSession(Ruleset rules, uint seed, IReadOnlyList<MatchSeat> seats, bool duos = false, int[][]? teams = null,bool chaos=false,bool chaosPace=false)
    {
        MatchRounds? rounds = null;
        MatchPhase Phase() => rounds?.Phase ?? MatchPhase.Planning;
        State = MatchStart.Create(rules, seed, seats, () => rounds?.Position ?? RoundPosition.First, duos, teams);
        State.Chaos=chaos;State.ChaosPace=chaosPace;
        Units = State.Units!;
        CombatProjection = new(State);
        Offers = new(State, Units, Phase);
        Ai = new(State, Offers, Units);
        var effects = new MatchRoundEffects(State, Offers, Units, Ai, Phase);
        Rounds = rounds = new(State, Offers, effects);
        Actions = new(this);
        Rounds.Start();
    }
    public MatchState State { get; }
    public MatchUnitLifecycle Units { get; }
    public MatchCombatProjection CombatProjection { get; }
    public MatchOffers Offers { get; }
    public MatchAi Ai { get; }
    public MatchRounds Rounds { get; }
    public MatchActions Actions { get; }
    public IReadOnlyList<MatchBattle> Battles { get; private set; } = [];
    public IReadOnlyList<MatchBattle> StartCombat()
    {
        if (State.Over || Rounds.Phase != MatchPhase.Planning)
            throw new InvalidOperationException("The round is not ready for combat.");
        List<MatchBattle> battles = [];
        if (Rounds.RoundType == "carousel") Units.Carousel.Finish(Rounds.Phase);
        else
        {
            var choices = new AiOffers(State);
            var loot = new MatchLoot(State);
            var placement = new MatchPlacement(State);
            foreach (var player in State.Players.Where(p => p.Alive))
            {
                if (player.Offer is { } offer) Offers.Choose(player, choices.Select(player, offer, Rounds.Position.Stage), false);
                loot.CollectAll(player);
                placement.AutoFill(player, Rounds.Position.Stage);
            }
            if (State.Duos) State.SyncTeams();
        }
        Rounds.BeginCombat();
        void Add(int a, int? b, bool pve, bool ghost, ProjectedCombatSide sideA, ProjectedCombatSide sideB, int? ta = null, int? tb = null)
        {
            uint seed = (uint)(State.Random.Next() * 1e9);
            battles.Add(new(State, a, b, pve, ghost, sideA, sideB, seed,
                CombatProjection.Setup(sideA, sideB, Rounds.Position), ta, tb));
        }
        if (Rounds.RoundType == "pve")
        {
            foreach (var player in State.Players.Where(p => p.Alive))
                Add(player.Id, null, true, false, CombatProjection.Player(player),
                    new(State.Rules.MatchCombat!.Wave(Rounds.Position, State.TreasureVariant), new(NoTraits: true), 7));
        }
        else if (Rounds.RoundType == "pvp")
        {
            foreach (var pair in Rounds.Pairs)
                if (State.Duos)
                {
                    var a = State.Teams[pair.A]; var b = State.Teams[pair.B];
                    Add(a.Members[0].Id, b.Members[0].Id, false, pair.Ghost, CombatProjection.Team(a), CombatProjection.Team(b), a.Id, b.Id);
                }
                else Add(pair.A, pair.B, false, pair.Ghost, CombatProjection.Player(State.Players[pair.A]), CombatProjection.Player(State.Players[pair.B]));
        }
        Battles = battles.AsReadOnly();
        return Battles;
    }
    public IReadOnlyList<MatchRoundResult> FinishCombat()
    {
        if (Rounds.Phase != MatchPhase.Combat) throw new InvalidOperationException("There is no combat to finish.");
        foreach (var battle in Battles) while (!battle.Combat.Done) battle.Combat.Step();
        return Resolve(Battles.Select(b => b.Result()).ToArray());
    }
    public IReadOnlyList<MatchRoundResult> Resolve(IReadOnlyList<ResolvedMatch> matches) =>
        Rounds.Resolve(matches, new MatchSettlementEffects(State, Units, Rounds.Position));
    public bool PickCarousel(MatchPlayer player, int index)
    {
        if (!Units.Carousel.Pick(player, index, Rounds.Phase)) return false;
        Units.Carousel.Step(Rounds.Phase);
        return true;
    }
    public bool Advance()
    {
        if (Units.Carousel.Active) Units.Carousel.Finish(Rounds.Phase);
        return Rounds.Advance();
    }
}
