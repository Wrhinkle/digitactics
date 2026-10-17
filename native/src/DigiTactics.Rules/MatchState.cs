namespace DigiTactics.Rules;

public sealed record MatchSeat(string Name, bool Human);
public sealed record DuoRules(int TeamHp, int Sends, double UnitScale);
public sealed record MatchPair(int A, int B, bool Ghost);

public sealed class MatchPlayer
{
    internal MatchPlayer(int id, MatchSeat seat, Ruleset rules, ShopPool pool, Func<string, int, PlanningUnit> create)
    {
        itemCap = rules.Loot!.ItemCap;
        Id = id; Name = seat.Name; IsHuman = seat.Human; Health = rules.Economy!.StartHp;
        Economy = new(rules.Economy); Roster = new(rules, create);
        Shop = new(rules, pool, Economy, Roster, create, GiveItem);
    }
    private readonly int itemCap;
    public void GiveItem(string id) { if (!string.IsNullOrEmpty(id) && Items.Count < itemCap) Items.Add(id); }
    public int Id { get; }
    public string Name { get; }
    public bool IsHuman { get; }
    public int Health { get; set; }
    public bool Alive { get; internal set; } = true;
    public int Placement { get; internal set; }
    public int? LastOpponent { get; set; }
    public int? Team { get; internal set; }
    public PlayerEconomy Economy { get; }
    public int TeamBonus { get; set; }
    public PlanningRoster Roster { get; }
    public PlayerShop Shop { get; }
    public List<string> Items { get; } = [];
    public HashSet<string> Augments { get; } = [];
    // Reserved unit drops return to the pool when the owner is eliminated.
    public List<LootOrb> Orbs { get; } = [];
    public PlayerMechanics Mechanics { get; } = new();
    public PlayerOffer? Offer { get; internal set; }
    public CombatPower? Power { get; set; }
    public List<string> Globals { get; } = [];
    public CombatBuffs? NextBuffs { get; set; }
    public WispOffer? Wisp { get; set; }
    public int PortalSends { get; internal set; }
    public int? ProudPartnerRound { get; internal set; }
    public int ProudPartnerCount { get; internal set; }
    public List<PortalArrival> Incoming { get; } = [];
}

public sealed class MatchTeam(int id, MatchPlayer[] members, int health)
{
    public int Id { get; } = id;
    public IReadOnlyList<MatchPlayer> Members { get; } = Array.AsReadOnly(members);
    public int Health { get; internal set; } = health;
    public bool Alive { get; internal set; } = true;
    public int Placement { get; internal set; }
    public int? LastOpponent { get; set; }
}

/// <summary>Shared match ownership, pairing and elimination; set hooks and combat orchestration are separate.</summary>
public sealed class MatchState
{
    private int uid;
    private readonly Dictionary<string, UnitLine> shopLines;
    public MatchState(Ruleset rules, uint seed, IReadOnlyList<MatchSeat> seats, bool duos = false, int[][]? teams = null,
        Action<MatchState>? initialize = null)
    {
        if (seats.Count == 0) throw new ArgumentException("A match needs players.", nameof(seats));
        Rules = rules; Seed = seed; Random = new(seed); Pool = new(rules, Random);
        shopLines = rules.Lines.ToDictionary(l => l.Id);
        Players = Array.AsReadOnly(seats.Select((s, i) => new MatchPlayer(i, s, rules, Pool, CreateUnit)).ToArray());
        Duos = duos && seats.Count == 8;
        Teams = Duos ? Array.AsReadOnly(ValidTeams(teams).Select((ids, i) =>
            new MatchTeam(i, ids.Select(id => Players[id]).ToArray(), rules.Duo!.TeamHp)).ToArray()) : Array.Empty<MatchTeam>();
        foreach (var team in Teams) foreach (var player in team.Members) { player.Team = team.Id; player.Health = team.Health; }
        initialize?.Invoke(this);
        foreach (var player in Players) player.Shop.Refresh();
    }
    public Ruleset Rules { get; }
    public uint Seed { get; }
    public DeterministicRandom Random { get; }
    public ShopPool Pool { get; }
    public IReadOnlyList<MatchPlayer> Players { get; }
    public IReadOnlyList<MatchTeam> Teams { get; }
    public bool Duos { get; }
    public bool Over { get; private set; }
    public bool PlayOut { get; set; }
    public int[]? GizmosGates { get; set; }
    public string? TreasureVariant { get; set; }
    public string? Galaxy { get; set; }
    public string? GizmosMutation { get; set; }
    public string? DragonMirage { get; set; }
    public string? ArcanePortal { get; set; }
    public string[] ArcaneSponsor { get; set; } = [];
    public MatchUnitLifecycle? Units { get; internal set; }
    public PlanningUnit CreateUnit(string line, int stage) => new(AllocateId(), line, stage, []);
    internal int AllocateId() => ++uid;

    public IReadOnlyList<MatchPair> MakePairs(string roundType)
    {
        if (roundType != "pvp") return Array.Empty<MatchPair>();
        var entrants = Duos
            ? Teams.Where(t => t.Alive).Select(t => new Entrant(t.Id, t.Members.Any(p => p.IsHuman), t.LastOpponent)).ToList()
            : Players.Where(p => p.Alive).Select(p => new Entrant(p.Id, p.IsHuman, p.LastOpponent)).ToList();
        if (entrants.Count < 2) return Array.Empty<MatchPair>();
        for (int i = entrants.Count - 1; i > 0; i--)
        {
            int j = (int)(Random.Next() * (i + 1));
            (entrants[i], entrants[j]) = (entrants[j], entrants[i]);
        }
        foreach (var human in entrants.Where(e => e.Human).ToArray())
        {
            if (entrants.Count <= 2) break;
            int index = entrants.IndexOf(human), opponent = index % 2 == 0 ? index + 1 : index - 1;
            if (opponent < entrants.Count && entrants[opponent].Id == human.LastOpponent)
            {
                int swap = Enumerable.Range(0, entrants.Count).FirstOrDefault(k => k != index && k != opponent && k / 2 != index / 2, -1);
                if (swap >= 0) (entrants[opponent], entrants[swap]) = (entrants[swap], entrants[opponent]);
            }
        }
        List<MatchPair> pairs = [];
        for (int i = 0; i + 1 < entrants.Count; i += 2) pairs.Add(new(entrants[i].Id, entrants[i + 1].Id, false));
        if (entrants.Count % 2 == 1)
            pairs.Add(new(entrants[^1].Id, entrants[(int)(Random.Next() * (entrants.Count - 1))].Id, true));
        return pairs;
    }

    public void SyncTeams()
    {
        foreach (var team in Teams)
        {
            team.Health += team.Members.Sum(p => p.Health - team.Health);
            foreach (var player in team.Members) player.Health = team.Health;
        }
    }
    public int DamageTeam(int id, int damage)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(damage);
        var team = Teams[id];
        if (team.Members.Any(p => p.Augments.Contains("duo_guard"))) damage = (int)Math.Ceiling(damage * .75);
        team.Health -= damage;
        foreach (var player in team.Members) player.Health = team.Health;
        return damage;
    }

    public void Eliminate(Action<MatchPlayer>? onEliminate = null)
    {
        if (Duos)
        {
            int place = Teams.Count(t => t.Alive);
            foreach (var team in Teams.Where(t => t.Alive && t.Health <= 0).OrderBy(t => t.Health).ThenBy(t => t.Id))
            {
                team.Alive = false; team.Placement = place--;
                foreach (var player in team.Members) KnockOut(player, team.Placement, onEliminate);
            }
            var alive = Teams.Where(t => t.Alive).ToArray();
            if (alive.Length <= 1)
            {
                if (alive.Length == 1) { alive[0].Placement = 1; foreach (var player in alive[0].Members) player.Placement = 1; }
                Over = true;
            }
        }
        else
        {
            int place = Players.Count(p => p.Alive);
            foreach (var player in Players.Where(p => p.Alive && p.Health <= 0).OrderBy(p => p.Health)) KnockOut(player, place--, onEliminate);
            var alive = Players.Where(p => p.Alive).ToArray();
            if (alive.Length <= 1) { if (alive.Length == 1) alive[0].Placement = 1; Over = true; }
        }
        if (!PlayOut && Players.Any(p => p.IsHuman) && !Players.Any(p => p.Alive && p.IsHuman)) Over = true;
    }

    private void KnockOut(MatchPlayer player, int placement, Action<MatchPlayer>? onEliminate)
    {
        player.Alive = false; player.Placement = placement;
        foreach (var entry in player.Roster.Entries.ToArray())
        {
            if (shopLines.TryGetValue(entry.Unit.Line, out var line)) Pool.Return(line.Id, UnitValues.Copies(line, entry.Unit.Stage));
            player.Roster.Remove(entry.Unit.Id, false);
        }
        player.Shop.ReleaseOffers();
        foreach (var orb in player.Orbs) if (orb.Unit is { } line) Pool.Return(line);
        player.Orbs.Clear(); player.Items.Clear(); player.Offer = null;
        onEliminate?.Invoke(player);
    }
    private static int[][] ValidTeams(int[][]? teams)
    {
        if (teams is not { Length: 4 } || teams.Any(t => t is not { Length: 2 }) ||
            !teams.SelectMany(t => t).Order().SequenceEqual(Enumerable.Range(0, 8)))
            return [[0, 1], [2, 3], [4, 5], [6, 7]];
        return teams.Select(t => t.Order().ToArray()).ToArray();
    }
    private sealed record Entrant(int Id, bool Human, int? LastOpponent);
}
