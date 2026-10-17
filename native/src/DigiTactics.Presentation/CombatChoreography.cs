using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public interface ICombatModelDriver
{
    void Create(CombatUnit unit);
    void Remove(int unit);
    bool Play(int unit, string role);
    void Update(int unit, double delta);
}
public readonly record struct CombatVisual(int UnitId, WorldPosition Position, double Yaw, WorldPosition BodyOffset,
    double Scale, double Flash, double Glow, double RingScale, string? CastName, bool CastVisible);

/// <summary>Ordered combat-to-model cues and per-view motion. Simulation time controls effects;
/// frame delta controls facing and animation, matching the original renderer.</summary>
public sealed class CombatChoreography
{
    private sealed class State(string line, int stage, double yaw)
    {
        public string Line = line;
        public int Stage = stage;
        public double Yaw = yaw, FlashUntil, GlowStart, GlowUntil, CastUntil;
        public string? CastName;
        public Lunge? Lunge;
    }
    private sealed record Lunge(double Start, double Duration, double X, double Z, double Distance);
    private readonly CombatSimulation combat;
    private readonly CombatScene scene;
    private readonly ICombatModelDriver models;
    private readonly Dictionary<int, State> states = [];
    private readonly List<CombatVisual> views = [];
    private readonly HashSet<int> keep = [];
    private readonly List<int> expired = [];
    private int cursor;
    private bool celebrated;
    private (int Unit, double Time)? cast;
    public IReadOnlyList<CombatVisual> Views { get; }

    public CombatChoreography(CombatSimulation combat, ICombatModelDriver models)
    { this.combat = combat; this.models = models; scene = new(combat); Views = views.AsReadOnly(); }

    public void Update(double delta, bool mirror = false, int? hover = null)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        if (combat.Done && !celebrated)
        {
            celebrated = true;
            foreach (var unit in combat.Units) if (unit.Health > 0 && unit.Team == combat.Winner && states.ContainsKey(unit.Id)) models.Play(unit.Id, "win");
        }
        while (cursor < combat.Events.Count)
        {
            var cue = combat.Events[cursor++];
            states.TryGetValue(cue.Unit, out var state);
            if (cue.Type == "cast")
            {
                cast = (cue.Unit, cue.Time);
                if (state is not null)
                {
                    var unit = combat.Units[cue.Unit];
                    state.GlowStart = cue.Time; state.GlowUntil = cue.Time + .9; state.CastUntil = cue.Time + 1.5;
                    state.CastName = unit.Line.Ability.Names[unit.Stage]; models.Play(cue.Unit, "special");
                }
            }
            else if (cue.Type == "damage" && state is not null) state.FlashUntil = cue.Time + .12;
            else if (cue.Type is "hit" or "shot" && state is not null && cue.Target is { } target)
            {
                if (cue.Type == "shot" && cast is { } ultimate && ultimate.Unit == cue.Unit && Math.Abs(ultimate.Time - cue.Time) < 1e-6) continue;
                var a = CombatLayout.Unit(combat.Units[cue.Unit], combat.Columns, mirror);
                var b = CombatLayout.Unit(combat.Units[target], combat.Columns, mirror);
                double x = b.X - a.X, z = b.Z - a.Z, length = Math.Sqrt(x*x + z*z); if (length == 0) length = 1;
                bool swing = models.Play(cue.Unit, "attack");
                bool shot = cue.Type == "shot";
                state.Lunge = new(cue.Time, shot ? .15 : .22, x / length * (shot ? -1 : 1), z / length * (shot ? -1 : 1), shot ? .12 : swing ? .25 : .45);
            }
        }
        scene.Update(mirror, hover); views.Clear();
        keep.Clear();
        foreach (var view in scene.Views)
        {
            var unit = combat.Units[view.UnitId]; keep.Add(view.UnitId);
            if (states.TryGetValue(view.UnitId, out var state) && (state.Line != unit.Line.Id || state.Stage != unit.Stage))
            { models.Remove(view.UnitId); states.Remove(view.UnitId); state = null; }
            if (state is null)
            { state = new(unit.Line.Id, unit.Stage, view.Yaw); states.Add(view.UnitId, state); models.Create(unit); }
            models.Update(view.UnitId, delta);
            double difference = view.Yaw - state.Yaw;
            state.Yaw += Math.Atan2(Math.Sin(difference), Math.Cos(difference)) * Math.Min(1, delta * 10);
            var offset = new WorldPosition();
            if (state.Lunge is { } lunge)
            {
                double progress = (combat.Time - lunge.Start) / lunge.Duration;
                if (progress >= 1) state.Lunge = null;
                else { double distance = Math.Sin(Math.PI * progress) * lunge.Distance; offset = new(lunge.X * distance, 0, lunge.Z * distance); }
            }
            double flash = Math.Max(0, state.FlashUntil - combat.Time) * 4;
            double glow = combat.Time >= state.GlowStart && combat.Time < state.GlowUntil ? (state.GlowUntil - combat.Time) / (state.GlowUntil - state.GlowStart) : 0;
            double ring = glow > 0 ? 1 + .6 * Math.Sin(Math.PI * Math.Min(1, (1 - glow) * 2)) : 1;
            if (state.CastUntil != 0 && combat.Time > state.CastUntil) state.CastUntil = 0;
            views.Add(new(view.UnitId, view.Position with { Y = view.Position.Y - view.Dying * .5 }, state.Yaw, offset,
                view.Dying != 0 ? 1 - view.Dying : view.Hover ? 1.08 : 1, flash, glow > 0 ? .2 + .5 * glow : 0, ring, state.CastName, state.CastUntil != 0));
        }
        expired.Clear();
        foreach (int id in states.Keys) if (!keep.Contains(id)) expired.Add(id);
        foreach (int id in expired) { models.Remove(id); states.Remove(id); }
    }
}
