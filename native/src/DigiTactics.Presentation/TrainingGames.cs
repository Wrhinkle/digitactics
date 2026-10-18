namespace DigiTactics.Presentation;

/// <summary>Frame-driven training rules; the host supplies elapsed game time and routes player input.</summary>
public abstract class TrainingGame
{
    private readonly Queue<string> sounds = new();
    public double? Score { get; private set; }
    public double TimeLeft { get; protected set; } = 1;
    public string Feedback { get; protected set; } = "";
    public abstract string Left { get; }
    public abstract string Right { get; }
    public static TrainingGame Create(string name) => name switch
    {
        "strength" => new SwingTraining(), "speed" => new BeatTraining(),
        "wisdom" => new MemoryTraining(), "stamina" => new EnduranceTraining(),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };
    public void Update(double delta, double time, double? wallDelta = null)
    {
        if (!double.IsFinite(delta) || delta < 0 || !double.IsFinite(time) || time < 0) throw new ArgumentOutOfRangeException(nameof(delta));
        if (wallDelta is { } wall && (!double.IsFinite(wall) || wall < 0)) throw new ArgumentOutOfRangeException(nameof(wallDelta));
        if (Score is not null) return;
        AdvanceTimers(wallDelta ?? delta);
        var result = Step(delta, time);
        TimeLeft = Math.Clamp(TimeLeft, 0, 1);
        if (result is not null) Score = Math.Clamp(result.Value, 0, 1);
    }
    protected abstract double? Step(double delta, double time);
    protected virtual void AdvanceTimers(double delta) { }
    public abstract void Input(int value = 0);
    public abstract int? Key(string key);
    protected void Sound(string name) => sounds.Enqueue(name);
    public string[] DrainSounds() { var result = sounds.ToArray(); sounds.Clear(); return result; }
    protected static int Round(double value) => (int)Math.Floor(value + .5);
}

public sealed class SwingTraining : TrainingGame
{
    private static readonly double[] Zones = [.2, .17, .14, .12, .1], Speeds = [.9, 1.05, 1.2, 1.35, 1.55];
    private double direction = 1, swingTime, pause, total;
    public int Swings { get; private set; }
    public double Position { get; private set; }
    public double Center => .3 + ((Math.Min(Swings, 4) * 37) % 5) * .1;
    public double Zone => Zones[Math.Min(Swings, 4)];
    public bool Ready => Score is null && Swings < 5 && pause <= 0;
    public override string Left => $"Swing {Math.Min(Swings + 1, 5)} / 5";
    public override string Right => $"Power {Round(total * 20)}";
    private void Judge(bool miss)
    {
        double half = Zone / 2, distance = Math.Abs(Position - Center);
        double value = miss ? 0 : distance <= half ? 1 - .3 * distance / half : Math.Max(0, .5 - (distance - half) * 3);
        total += value;
        Feedback = value >= .9 ? "POW!" : value >= .7 ? "Great" : value > .3 ? "OK" : "Miss";
        Sound(value > .3 ? value >= .9 ? "hit" : "place" : "error");
        Swings++; pause = .55;
    }
    protected override double? Step(double delta, double time)
    {
        TimeLeft = 1 - (Swings + Math.Min(1, swingTime / 3)) / 5;
        if (Swings >= 5) { if (pause > 0) { pause -= delta; return null; } return total / 5; }
        if (pause > 0) { pause -= delta; return null; }
        swingTime += delta;
        Position += direction * Speeds[Swings] * delta;
        if (Position > 1) { Position = 2 - Position; direction = -1; }
        else if (Position < 0) { Position = -Position; direction = 1; }
        if (swingTime >= 3) { swingTime = 0; Judge(true); }
        return null;
    }
    public override void Input(int value = 0) { if (!Ready) return; swingTime = 0; Judge(false); }
    public override int? Key(string key) => key is " " or "Enter" ? 0 : null;
}

public sealed class BeatTraining : TrainingGame
{
    public sealed class Note(double beat, int lane)
    {
        public double At { get; } = beat * (60.0 / 128);
        public int Lane { get; } = lane;
        public bool Done { get; internal set; }
    }
    private readonly Note[] notes = [new(2, 0), new(3, 0), new(4, 1), new(5, 1), new(6, 0), new(6.5, 0), new(7, 1), new(8, 0), new(9, 1), new(10, 1), new(10.5, 0), new(11, 0), new(12, 1), new(13, 0), new(13.5, 1), new(14, 0), new(15, 1), new(16, 0)];
    public IReadOnlyList<Note> Notes => notes;
    public double Time { get; private set; }
    private double total;
    public int Hits { get; private set; }
    public int Combo { get; private set; }
    public override string Left => $"Hits {Hits} / {notes.Length}";
    public override string Right => Combo > 2 ? $"Combo {Combo}" : "";
    protected override double? Step(double delta, double time)
    {
        Time = time; TimeLeft = 1 - time / 8.3;
        foreach (var note in notes)
            if (!note.Done && note.At - time < -.2) { note.Done = true; Combo = 0; Feedback = "Miss"; }
        return time >= 8.3 ? total / notes.Length : null;
    }
    public override void Input(int value = 0)
    {
        if (Score is not null || value is < 0 or > 1) return;
        Note? best = null;
        foreach (var note in notes)
            if (!note.Done && note.Lane == value && Math.Abs(note.At - Time) < .25 && (best is null || Math.Abs(note.At - Time) < Math.Abs(best.At - Time))) best = note;
        if (best is null) return;
        double distance = Math.Abs(best.At - Time), score = distance < .07 ? 1 : distance < .14 ? .6 : .25;
        best.Done = true; total += score; Hits++; Combo++;
        Feedback = score == 1 ? "Perfect" : score > .5 ? "Good" : "Late"; Sound("tick");
    }
    public override int? Key(string key) => key switch { "f" or "F" or "ArrowUp" or "w" => 0, "j" or "J" or "ArrowDown" or "s" => 1, _ => null };
}

public sealed class MemoryTraining : TrainingGame
{
    private long seed = 7;
    private int[] sequence = [];
    private int index;
    private double phaseTime, lightTime, elapsed, clock;
    private double? nextAt;
    public int RoundIndex { get; private set; }
    public int Correct { get; private set; }
    public int LitPad { get; private set; } = -1;
    public string Phase { get; private set; } = "show";
    public override string Left => $"Round {Math.Min(RoundIndex + 1, 3)} / 3";
    public override string Right => $"Correct {Correct} / 12";
    public MemoryTraining() => NewRound();
    private void NewRound()
    {
        sequence = new int[RoundIndex + 3];
        for (int i = 0; i < sequence.Length; i++) { seed = seed * 16807 % 2147483647; sequence[i] = (int)Math.Floor(seed / 2147483647.0 * 4); }
        Phase = "show"; phaseTime = -.4; index = 0; Feedback = $"Round {RoundIndex + 1}";
    }
    private void Light(int pad, double duration) { LitPad = pad; lightTime = duration; }
    protected override void AdvanceTimers(double delta)
    {
        // Original next-round callbacks run before the next animation frame.
        clock += delta;
        if (nextAt is { } due && clock >= due) { nextAt = null; RoundIndex++; if (RoundIndex < 3) NewRound(); else Phase = "end"; }
    }
    protected override double? Step(double delta, double time)
    {
        elapsed += delta; TimeLeft = 1 - elapsed / 24;
        if (lightTime > 0) { lightTime -= delta; if (lightTime <= 0) LitPad = -1; }
        if (Phase == "end") return Correct / 12.0;
        if (elapsed >= 24) { Feedback = "Time!"; return Correct / 12.0; }
        if (Phase == "show")
        {
            phaseTime += delta; const double step = .55 + .18;
            int k = (int)Math.Floor(phaseTime / step);
            if (phaseTime >= 0 && k < sequence.Length && phaseTime - k * step < .55 && LitPad != sequence[k]) { Light(sequence[k], .55 - (phaseTime - k * step)); Sound("tick"); }
            if (phaseTime >= sequence.Length * step) { Phase = "input"; index = 0; Feedback = "Your turn"; }
        }
        return null;
    }
    public override void Input(int value = 0)
    {
        if (Score is not null || Phase != "input" || value is < 0 or > 3) return;
        Light(value, .2);
        if (sequence[index] == value)
        {
            Correct++; index++; Sound("pick");
            if (index >= sequence.Length) { Feedback = "Correct!"; Phase = "wait"; nextAt = clock + .35; }
        }
        else { Sound("error"); Feedback = "Oops"; Phase = "wait"; nextAt = clock + .45; }
    }
    public override int? Key(string key) => key is "1" or "2" or "3" or "4" ? key[0] - '1' : null;
}

public sealed class EnduranceTraining : TrainingGame
{
    public double Meter { get; private set; } = .6;
    public double InZone { get; private set; }
    public double Time { get; private set; }
    public override string Left => $"In the band {Round(100 * InZone / Math.Max(Time, .01))}%";
    public override string Right
    {
        get
        {
            // JS toFixed rounds the exact binary value, with ties away from zero.
            double seconds = Math.Max(0, 12 - Time);
            ulong tenths = 0;
            if (seconds >= .05)
            {
                ulong bits = BitConverter.DoubleToUInt64Bits(seconds);
                int shift = 1075 - (int)((bits >> 52) & 2047);
                ulong scaled = ((bits & 0xfffffffffffffUL) | 0x10000000000000UL) * 10;
                tenths = (scaled + (1UL << (shift - 1))) >> shift;
            }
            return $"{tenths / 10}.{tenths % 10} s";
        }
    }
    protected override double? Step(double delta, double time)
    {
        Time = time; TimeLeft = 1 - time / 12;
        Meter = Math.Clamp(Meter - delta * (.28 + .03 * time), 0, 1);
        if (Meter is >= .38 and <= .82) InZone += delta;
        return time >= 12 ? InZone / 12 : null;
    }
    public override void Input(int value = 0) { if (Score is null) Meter = Math.Clamp(Meter + .075, 0, 1); }
    public override int? Key(string key) => key is " " or "Enter" ? 0 : null;
}
