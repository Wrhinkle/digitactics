namespace DigiTactics.Presentation;

public interface IFarmCompanionView
{
    bool HasRun { get; }
    double Yaw { get; set; }
    void SetMoving(bool moving);
    void Play(string role);
    void Update(double delta);
}
public sealed record FarmResidentNeeds(bool Asleep, bool Sick, double Energy, double Happy, bool Tantrum);
public sealed class FarmResident(string id, IFarmCompanionView view, double x, double z, double wait)
{
    public string Id { get; } = id;
    public IFarmCompanionView View { get; } = view;
    public FarmResidentNeeds Needs { get; set; } = new(false, false, 100, 100, false);
    public double X { get; internal set; } = x;
    public double Z { get; internal set; } = z;
    public double TargetX { get; internal set; }
    public double TargetZ { get; internal set; }
    public string Mode { get; internal set; } = "idle";
    public double Wait { get; internal set; } = wait;
    public double Busy { get; set; }
    public bool GoingToBed { get; internal set; }
    public int BedSlot { get; internal set; } = -1;
    public double Tilt { get; internal set; }
    public double Height { get; internal set; }
    public double Breathe { get; internal set; } = 1;
    internal Action? Arrived;
}

/// <summary>Original farm wandering, sleep routing and social movement. Never mutates saved pet needs.</summary>
public sealed class FarmChoreography(Func<double>? random = null)
{
    private readonly Func<double> random = random ?? Random.Shared.NextDouble;
    private readonly Dictionary<string, FarmResident> residents = [];
    private static readonly (double X, double Z)[] Beds = [(0, .05), (-.62, .05), (.62, .05), (0, .75), (-.62, .75), (.62, .75)];
    public IReadOnlyDictionary<string, FarmResident> Residents => residents;
    public double Time { get; private set; }
    public FarmResident Add(string id, IFarmCompanionView view, double? x = null, double? z = null)
    {
        int k = residents.Count; double angle = k * 2.39996, radius = .7 * Math.Sqrt((k + 1) / 12.0);
        var actor = new FarmResident(id, view, x ?? Math.Cos(angle) * 2.7 * radius, z ?? .3 + Math.Sin(angle) * 2.3 * radius, random() * 2);
        residents.Add(id, actor); return actor;
    }
    public bool Remove(string id) => residents.Remove(id);
    public string? NearestPlaymate(string id)
    {
        if (!residents.TryGetValue(id, out var a)) return null;
        FarmResident? nearest = null; double distance = double.PositiveInfinity;
        foreach (var b in residents.Values)
        {
            if (b == a || b.Needs.Asleep || b.Needs.Sick) continue;
            double dx = b.X - a.X, dz = b.Z - a.Z, next = Math.Sqrt(dx * dx + dz * dz);
            // Original reduce chooses the later resident when distances tie.
            if (next <= distance) { distance = next; nearest = b; }
        }
        return nearest?.Id;
    }
    public void Feed(string id)
    {
        if (!residents.TryGetValue(id, out var a)) return;
        a.Mode = "idle"; a.Busy = 1.6; a.View.Play("eat");
    }
    public void Pet(string id)
    {
        if (!residents.TryGetValue(id, out var a) || a.Needs.Asleep) return;
        a.View.Play(a.Needs.Tantrum ? "taunt" : "happy");
        a.View.Yaw = Math.Atan2(-a.X, 8 - a.Z); a.Busy = Math.Max(a.Busy, .8);
    }
    public void Play(string id, string? mate)
    {
        if (!residents.TryGetValue(id, out var a)) return;
        if (mate is null || !residents.TryGetValue(mate, out var b)) { PlayPair(a, null); return; }
        ApproachPair(a, b);
    }
    public void StartTraining(string id)
    {
        if (!residents.TryGetValue(id, out var a)) return;
        a.GoingToBed = false; WalkTo(a, 3, 1.8, () => a.View.Yaw = .6);
    }
    public void Toilet(string id)
    {
        if (!residents.TryGetValue(id, out var a)) return;
        WalkTo(a, 3.3, -2.2, () => { a.View.Play("happy"); a.Busy = 1.2; });
    }
    public void FinishTraining(string id, double score)
    {
        if (!residents.TryGetValue(id, out var a)) return;
        a.View.Play(score >= .6 ? "happy" : "sad"); a.Busy = 1.5;
    }
    public static void WalkTo(FarmResident actor, double x, double z, Action? arrived = null)
    { actor.TargetX = x; actor.TargetZ = z; actor.Mode = "walk"; actor.Arrived = arrived; }
    private void Wander(FarmResident actor)
    {
        double angle = random() * Math.PI * 2, radius = Math.Sqrt(random());
        WalkTo(actor, Math.Cos(angle) * 2.7 * radius, .3 + Math.Sin(angle) * 2.3 * radius);
    }
    private void Bed(FarmResident actor)
    {
        var used = residents.Values.Where(x => x != actor && (x.Mode == "sleep" || x.Mode == "walk" && x.GoingToBed)).Select(x => x.BedSlot).ToHashSet();
        int slot = Enumerable.Range(0, Beds.Length).FirstOrDefault(x => !used.Contains(x), -1);
        actor.BedSlot = slot; actor.GoingToBed = true;
        WalkTo(actor, slot < 0 ? actor.X : -3.2 + Beds[slot].X, slot < 0 ? actor.Z : -2.3 + Beds[slot].Z,
            () => { actor.GoingToBed = false; actor.Mode = "sleep"; actor.View.Yaw = 0; });
    }
    private void FriendPlay(FarmResident actor)
    {
        var others = residents.Values.Where(x => x != actor && x.Mode == "idle" && !x.Needs.Asleep && !x.Needs.Sick && x.Busy <= 0).ToArray();
        if (others.Length == 0) { actor.Wait = 1; return; }
        var other = others[(int)(random() * others.Length)];
        ApproachPair(actor, other);
    }
    private void ApproachPair(FarmResident actor, FarmResident other)
    {
        double mx = (actor.X + other.X) / 2, mz = (actor.Z + other.Z) / 2, angle = Math.Atan2(other.Z - actor.Z, other.X - actor.X);
        WalkTo(actor, mx - Math.Cos(angle) * .4, mz - Math.Sin(angle) * .4, () => PlayPair(actor, other));
        WalkTo(other, mx + Math.Cos(angle) * .4, mz + Math.Sin(angle) * .4);
    }
    public void PlayPair(FarmResident actor, FarmResident? other)
    {
        if (other is null) { actor.View.Play("dance"); actor.Busy = 2.4; return; }
        foreach (var (p, q) in new[] { (actor, other), (other, actor) })
        {
            p.View.Yaw = Math.Atan2(q.X - p.X, q.Z - p.Z); p.View.Play(random() < .5 ? "happy" : "dance"); p.Busy = 2.4; p.Mode = "idle";
        }
    }
    public void Update(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        Time += delta;
        foreach (var actor in residents.Values) Update(actor, delta);
    }
    private void Update(FarmResident a, double delta)
    {
        var data = a.Needs; var view = a.View;
        if (data.Asleep && a.Mode != "sleep" && !a.GoingToBed) Bed(a);
        else if (!data.Asleep && (a.Mode == "sleep" || a.GoingToBed)) { a.GoingToBed = false; a.Mode = "idle"; a.Wait = .6; view.Play("happy"); }
        double speed = a.GoingToBed ? 1.3 : data.Sick ? .55 : data.Energy < 20 ? .7 : 1.15;
        if (a.Busy > 0) { a.Busy -= delta; view.SetMoving(false); }
        else if (a.Mode == "walk")
        {
            double dx = a.TargetX - a.X, dz = a.TargetZ - a.Z, distance = Math.Sqrt(dx * dx + dz * dz);
            if (distance < .06) { a.Mode = "idle"; a.Wait = 1.5 + random() * 3; view.SetMoving(false); var arrived = a.Arrived; a.Arrived = null; arrived?.Invoke(); }
            else { double step = Math.Min(distance, speed * delta); a.X += dx / distance * step; a.Z += dz / distance * step; view.Yaw = Math.Atan2(dx, dz); view.SetMoving(true); }
        }
        else if (a.Mode == "idle")
        {
            view.SetMoving(false); a.Wait -= delta;
            if (a.Wait <= 0)
            {
                double roll = random();
                if (data.Tantrum) { view.Play("taunt"); a.Wait = 1.4; }
                else if (data.Sick) { if (roll < .4) view.Play("sad"); else Wander(a); a.Wait = 2 + random() * 3; }
                else if (roll < .55) Wander(a);
                else if (roll < .7 && data.Happy >= 60) { view.Play("happy"); a.Wait = 2; }
                else if (roll < .78 && data.Happy < 30) { view.Play("sad"); a.Wait = 2.5; }
                else if (roll < .9 && data.Happy >= 40 && data.Energy >= 25) FriendPlay(a);
                else { view.Yaw = Math.Atan2(-a.X, 8 - a.Z) + (random() - .5) * .8; a.Wait = 2 + random() * 2; }
            }
        }
        if (a.Mode != "sleep" && !a.GoingToBed) foreach (var b in residents.Values)
        {
            if (b == a || b.Mode == "sleep") continue;
            double dx = a.X - b.X, dz = a.Z - b.Z, distance = Math.Sqrt(dx * dx + dz * dz);
            if (distance > .001 && distance < .75) { a.X += dx / distance * (.75 - distance) * delta * 2; a.Z += dz / distance * (.75 - distance) * delta * 2; }
        }
        double radius = Math.Sqrt(a.X * a.X + a.Z * a.Z);
        if (radius > 4.7) { a.X *= 4.7 / radius; a.Z *= 4.7 / radius; }
        a.Tilt += ((a.Mode == "sleep" ? 1 : 0) - a.Tilt) * Math.Min(1, delta * 4);
        double hop = a.Mode == "walk" && !view.HasRun && a.Busy <= 0 ? Math.Abs(Math.Sin(Time * 9 + a.X)) * .14 : 0;
        a.Breathe = a.Mode == "sleep" ? 1 + Math.Sin(Time * 2 + a.Z) * .03 : 1;
        a.Height = hop + a.Tilt * .22; view.Update(delta);
    }
}
