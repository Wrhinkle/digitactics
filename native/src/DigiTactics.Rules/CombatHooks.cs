namespace DigiTactics.Rules;

public enum CombatHook
{
    CombatStart, CombatReady, Tick, UnitTick, Attack, BeforeDamage, BeforeDamageTaken,
    DamageDealt, DamageTaken, Lethal, Cast, Kill, Death, Shield, Heal, CombatEnd,
    AnyAttack, AnyCast, AnyKill, AnyDeath, AnyDamageDealt, AnyHeal, AnyShield
}
public enum CombatHookSource { Trait, Augment, Power, GlobalItem, HeldItem, Mechanic }

/// <summary>One event instance travels through each ordered listener, allowing damage modifiers to compose.</summary>
public sealed class CombatHookEvent
{
    public CombatUnit? Unit { get; internal set; }
    public CombatUnit? Target { get; init; }
    public CombatUnit? Source { get; init; }
    public double Amount { get; init; }
    public double Value { get; init; }
    public double DeltaTime { get; init; }
    public DamageKind Kind { get; init; }
    public bool Critical { get; init; }
    public bool IsBurn { get; init; }
    public bool Killed { get; init; }
    public int? Winner { get; init; }
    public double Multiplier { get; set; } = 1;
    public double Reduction { get; set; }
}

/// <summary>Shared by the hooks of one trait, item holder or other source; never shared between battles.</summary>
public sealed class CombatHookContext
{
    internal CombatHookContext(CombatSimulation combat, CombatHookSource kind, string id, int? team, CombatUnit? holder, int? owner, int count)
    { Combat = combat; Kind = kind; Id = id; Team = team; Holder = holder; Owner = owner; Count = count; }
    public CombatSimulation Combat { get; }
    public CombatHookSource Kind { get; }
    public string Id { get; }
    public int? Team { get; }
    public CombatUnit? Holder { get; }
    public int? Owner { get; }
    public int Count { get; }
    public Dictionary<string, object> State { get; } = [];
    public double Time => Combat.Time;
    public double Random() => Combat.NextRandom();
    public void After(double delay, Action action, CombatUnit? unit = null) => Combat.Schedule(delay, action, unit);
    public bool Every(CombatUnit unit, string key, double period) => Combat.HookEvery(unit, key, period);
    public void Buff(CombatUnit unit, CombatStat stat, double value, double duration = double.PositiveInfinity) => Combat.Buff(unit, stat, value, duration);
    public double Damage(CombatUnit source, CombatUnit target, double amount, DamageKind kind = DamageKind.Magic, bool critical = false) => Combat.HookDamage(source, target, amount, kind, critical);
    public double AbilityHit(CombatUnit source, CombatUnit target, double amount) => Combat.HookAbility(source, target, amount);
    public void Heal(CombatUnit target, double amount, bool quiet = false) => Combat.HookHeal(target, amount, quiet);
    public void Shield(CombatUnit target, double amount) => Combat.AddShield(target, amount);
    public void Stun(CombatUnit target, double duration) => Combat.HookStun(target, duration);
    public void Burn(CombatUnit source, CombatUnit target) => Combat.HookBurn(source, target);
    public void Blind(CombatUnit target) => target.Blind = true;
    public void Silence(CombatUnit target, double duration) => target.SilenceUntil = Math.Max(target.SilenceUntil, Time + duration);
    public void Weaken(CombatUnit target, double fraction, double duration)
    { target.Weakness = Math.Max(target.WeakUntil > Time ? target.Weakness : 0, fraction); target.WeakUntil = Time + duration; }
    public void Kill(CombatUnit unit, CombatUnit? source = null) { if (unit.Alive) Combat.Kill(unit, source); }
    public void CastAgain(CombatUnit unit, CombatUnit target) => Combat.CastAgain(unit, target);
    public IEnumerable<CombatUnit> Mine => Combat.Units.Where(u => u.Team == Team && (Owner is null || u.Entry.Owner == Owner));
}

/// <summary>Legacy dispatch order: team, subject items, mechanic, observers. Sorting is local to each bucket.</summary>
public sealed class CombatHookRegistry
{
    private sealed record Listener(CombatHookContext Context, int Order, int Sequence, Action<CombatHookContext, CombatHookEvent> Invoke);
    private readonly Dictionary<(int, CombatHook), List<Listener>> teams = [];
    private readonly Dictionary<(int, CombatHook), List<Listener>> holders = [];
    private readonly Dictionary<CombatHook, List<Listener>> global = [], observers = [];
    private readonly CombatSimulation combat;
    private readonly bool duos;
    private int sequence, depth;
    internal CombatHookRegistry(CombatSimulation combat, bool duos) { this.combat = combat; this.duos = duos; }

    public CombatHookContext Context(CombatHookSource kind, string id, int? team = null, CombatUnit? holder = null, int? owner = null, int count = 1)
    {
        if (team is < 0 or > 1) throw new ArgumentOutOfRangeException(nameof(team));
        if (kind == CombatHookSource.HeldItem && holder is null) throw new ArgumentException("Held item hooks require a holder.");
        if (kind != CombatHookSource.Mechanic && team is null && holder is null) throw new ArgumentException("Team hooks require a team.");
        return new(combat, kind, id, holder?.Team ?? team, holder, owner, count);
    }

    public void Add(CombatHookContext context, CombatHook hook, Action<CombatHookContext, CombatHookEvent> callback, int order = 0)
    {
        if (context.Combat != combat) throw new ArgumentException("Hook context belongs to another battle.");
        List<Listener> list;
        if (hook >= CombatHook.AnyAttack) list = Bucket(observers, hook);
        else if (context.Kind == CombatHookSource.HeldItem) list = Bucket(holders, (context.Holder!.Id, hook));
        else if (context.Kind == CombatHookSource.Mechanic) list = Bucket(global, hook);
        else list = Bucket(teams, (context.Team!.Value, hook));
        list.Add(new(context, order, sequence++, callback));
        list.Sort((a, b) => a.Order != b.Order ? a.Order.CompareTo(b.Order) : a.Context.Kind != b.Context.Kind
            ? a.Context.Kind.CompareTo(b.Context.Kind) : a.Sequence.CompareTo(b.Sequence));
    }

    private static List<Listener> Bucket<TKey>(Dictionary<TKey, List<Listener>> map, TKey key) where TKey : notnull
    { if (!map.TryGetValue(key, out var list)) map[key] = list = []; return list; }

    private void Run(List<Listener>? list, CombatHookEvent e, CombatUnit? subject = null)
    {
        if (list is null || depth > 8) return;
        depth++;
        try
        {
            foreach (var listener in list)
                if (subject is null || !duos || listener.Context.Owner is null || listener.Context.Owner == subject.Entry.Owner)
                    listener.Invoke(listener.Context, e);
        }
        finally { depth--; }
    }

    internal bool Has(CombatHook hook, CombatUnit subject, CombatHook? any = null) =>
        teams.ContainsKey((subject.Team, hook)) || holders.ContainsKey((subject.Id, hook)) || global.ContainsKey(hook) ||
        (any is { } observer && observers.ContainsKey(observer));

    internal void Fire(CombatHook hook, CombatUnit subject, CombatHookEvent e, CombatHook? any = null)
    {
        e.Unit = subject;
        Run(teams.GetValueOrDefault((subject.Team, hook)), e, subject);
        Run(holders.GetValueOrDefault((subject.Id, hook)), e);
        Run(global.GetValueOrDefault(hook), e);
        if (any is { } observer) Run(observers.GetValueOrDefault(observer), e);
    }

    internal void FireTeam(CombatHook hook, int team, CombatHookEvent e)
    {
        Run(teams.GetValueOrDefault((team, hook)), e);
        foreach (var unit in combat.Units)
            if (unit.Team == team && (unit.Alive || hook == CombatHook.CombatEnd) && holders.TryGetValue((unit.Id, hook), out var list))
            { e.Unit = unit; Run(list, e); }
    }
    internal void FireGlobal(CombatHook hook, CombatHookEvent e) => Run(global.GetValueOrDefault(hook), e);
}
