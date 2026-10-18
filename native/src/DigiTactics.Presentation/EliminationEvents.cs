using DigiTactics.Rules;

namespace DigiTactics.Presentation;

public sealed record EliminationMember(string Line, int Stage, int? Uid = null, int? Owner = null, bool FinalHit = false);
public sealed record EliminationPlayer(int Id, string Name, bool Alive, int Placement, int? Team, EliminationMember[] Board);
public sealed record EliminationRound(EliminationPlayer A, EliminationPlayer? B, EliminationPlayer? A2, EliminationPlayer? B2,
    bool Pve, bool Ghost, int DamageA, int DamageB, int TeamDamageA, CombatFinalHit? LastKillA, CombatFinalHit? LastKillB);
public sealed record EliminationEvent(EliminationPlayer Victim, EliminationPlayer? Killer, bool Ghost,
    EliminationPlayer[] Victims, EliminationPlayer[] Killers, EliminationMember? FinalHit, EliminationMember[] Team);

/// <summary>Source kill-screen attribution and immutable round-end lineup snapshots, independent of UI or RNG.</summary>
public static class EliminationEvents
{
    public static EliminationEvent[] Resolve(IEnumerable<EliminationRound> rounds)
    {
        var events = new List<EliminationEvent>();
        foreach (var round in rounds)
        {
            void Add(EliminationPlayer victim, EliminationPlayer? fallbackKiller, int damage, bool a)
            {
                if (victim.Alive) return;
                var owners = (a ? new[] { round.B, round.B2 } : new[] { round.A, round.A2 }).OfType<EliminationPlayer>().ToArray();
                var hit = !round.Pve && damage != 0 ? a ? round.LastKillB : round.LastKillA : null;
                var killer = round.Pve || damage == 0 ? null : owners.FirstOrDefault(p => p.Id == hit?.Owner) ?? fallbackKiller;
                var victims = (a ? new[] { round.A, round.A2 } : new[] { round.B, round.B2 }).OfType<EliminationPlayer>().ToArray();
                var killers = !round.Pve && damage != 0 ? owners : [];
                var team = killer is null ? [] : (killers.Length > 0 ? killers : [killer]).SelectMany(p => p.Board).ToList();
                var finalHit = hit is null ? team.FirstOrDefault() : new EliminationMember(hit.Line, hit.Stage, hit.Uid, hit.Owner);
                if (finalHit is not null)
                {
                    int index = team.FindIndex(u => finalHit.Uid is { } uid && u.Uid == uid || u.Line == finalHit.Line && u.Stage == finalHit.Stage && (finalHit.Owner is null || u.Owner == finalHit.Owner));
                    var honor = index < 0 ? finalHit : team[index];
                    if (index >= 0) team.RemoveAt(index);
                    team.Insert(0, honor with { FinalHit = true });
                }
                events.Add(new(victim, killer, round.Ghost, victims, killers, finalHit, team.ToArray()));
            }
            Add(round.A, round.B, round.TeamDamageA != 0 ? round.TeamDamageA : round.DamageA, true);
            if (round.B is not null && !round.Ghost) Add(round.B, round.A, round.DamageB, false);
        }
        return events.OrderByDescending(e => e.Victim.Placement).ToArray();
    }
    public static EliminationEvent[] From(MatchPlanningSession session)
    {
        var state = session.State;
        var lines = state.Rules.Lines.Concat(state.Rules.ExtraLines).ToDictionary(l => l.Id);
        EliminationPlayer Snapshot(int id)
        {
            var p = state.Players[id];
            var board = p.Roster.Entries.Where(e => e.Spot.IsBoard).Select(e => e.Unit)
                .OrderByDescending(u => UnitValues.Value(lines[u.Line], u.Stage) + u.Items.Length * .5)
                .Select(u => new EliminationMember(u.Line, u.Stage, u.Id, p.Id)).ToArray();
            return new(id, p.Name, p.Alive, p.Placement, p.Team, board);
        }
        EliminationPlayer? Partner(int? team, int seat) => team is { } id ? Snapshot(state.Teams[id].Members.First(p => p.Id != seat).Id) : null;
        return Resolve(session.Rounds.Results.Select(r => new EliminationRound(Snapshot(r.Match.A), r.Match.B is { } b ? Snapshot(b) : null,
            Partner(r.Match.TeamA, r.Match.A), r.Match.B is { } b2 ? Partner(r.Match.TeamB, b2) : null,
            r.Match.Pve, r.Match.Ghost, r.DamageA, r.DamageB, r.TeamDamageA, r.Match.Combat?.FinalHits[0], r.Match.Combat?.FinalHits[1])));
    }
}

/// <summary>Client-only sequential playback; it never holds up simulation or settlement.</summary>
public sealed class EliminationQueue(int localSeat)
{
    private readonly HashSet<int> shown = [];
    private readonly Queue<EliminationEvent> pending = [];
    public EliminationPlayback? Current { get; private set; }
    public int PendingCount => pending.Count;
    public void Enqueue(IEnumerable<EliminationEvent> events)
    {
        foreach (var entry in events) if (shown.Add(entry.Victim.Id)) pending.Enqueue(entry);
        NextIfIdle();
    }
    private void NextIfIdle() { if (Current is null && pending.TryDequeue(out var entry)) Current = new(entry, localSeat); }
    public void Advance(double delta, bool paused = false)
    {
        if (paused || Current is null) return;
        Current.Advance(delta); if (Current.Complete) Skip();
    }
    public void Skip() { Current = null; NextIfIdle(); }
    public void Clear() { pending.Clear(); shown.Clear(); Current = null; }
}

public sealed class EliminationPlayback
{
    public const double AttackDuration = 3.5;
    public EliminationEvent Event { get; }
    public bool Big { get; }
    public bool Cinematic { get; }
    public double Duration { get; }
    public double Time { get; private set; }
    public bool Attack => Cinematic && Time < AttackDuration;
    public bool Complete => Time >= Duration;
    public double DanceTime => Math.Max(0, Time - (Cinematic ? AttackDuration : 0));
    public double Opacity => Math.Min(CssEase(Time / .3), 1 - CssEase((Time - Duration + .3) / .3));
    public static double CssEase(double fraction)
    {
        double x=Math.Clamp(fraction,0,1);if(x is 0 or 1)return x;
        double lo=0,hi=1,t=0;
        for(int i=0;i<24;i++){t=(lo+hi)/2;double q=1-t,p=3*q*q*t*.25+3*q*t*t*.25+t*t*t;if(p<x)lo=t;else hi=t;}
        return 3*(1-t)*(1-t)*t*.1+3*(1-t)*t*t+t*t*t;
    }
    public double Flash => Attack ? Math.Min(1, Math.Max(0, 1 - Math.Abs(Time - 1.35) / .12) * .75 + Math.Max(0, (Time - 3.1) / .4)) : 0;
    public EliminationPlayback(EliminationEvent entry, int localSeat)
    {
        Event = entry;
        Big = entry.Victim.Id == localSeat || entry.Killer?.Id == localSeat || entry.Victims.Any(p => p.Id == localSeat) || entry.Killers.Any(p => p.Id == localSeat);
        Cinematic = entry.Killer is not null && entry.FinalHit is not null && Big;
        Duration = (entry.Killer is null ? 2 : Big ? 4 : 2.8) + (Cinematic ? AttackDuration : 0);
    }
    public void Advance(double delta)
    {
        if (!double.IsFinite(delta) || delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        Time += Math.Min(.1, delta);
    }
}
