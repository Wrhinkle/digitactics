using System.Text.Json.Nodes;

namespace DigiTactics.Presentation;

/// <summary>Care symbols and delayed saved rewards, with the original fixed-position CSS rise.</summary>
public sealed class FarmPops
{
    public sealed class Pop(string id, string text)
    {
        public string Id { get; } = id;
        public string Text { get; } = text;
        public FarmSymbol? Symbol { get; internal set; }
        public double Age { get; internal set; } = -.35;
        public (float X, float Y)? Position { get; internal set; }
        public double Progress => Ease(Math.Clamp(Age / 1.4, 0, 1));
        public float Offset => (float)(4 - 38 * Progress);
        public float Opacity => (float)(Progress < .15 ? Progress / .15 : Progress > .8 ? (1 - Progress) / .2 : 1);
    }
    private readonly List<Pop> pops = [];
    public IReadOnlyList<Pop> Items => pops;
    public void AddSymbol(string id, FarmSymbol symbol, (float X, float Y)? position)
    {
        if (position is not null) pops.Add(new(id, "") { Symbol = symbol, Position = position, Age = 0 });
    }
    public void Add(string? id, JsonObject receipt)
    {
        if (id is null || receipt["ok"]?.GetValue<bool>() != true || receipt["reward"] is not JsonObject reward) return;
        var parts = new List<string>();
        foreach (var (key, label) in new[] { ("bits", "Bits"), ("data", "Data") })
            if (reward[key] is { } amount && amount.GetValue<int>() > 0) parts.Add($"+{amount} {label}");
        if (parts.Count > 0) pops.Add(new(id, string.Join("  ", parts)));
    }
    public void Update(double delta, Func<string, (float X, float Y)?> project)
    {
        foreach (var pop in pops)
        {
            pop.Age += delta;
            if (pop.Age >= 0 && pop.Position is null) pop.Position = project(pop.Id);
        }
        pops.RemoveAll(p => p.Age >= 1.5 || p.Age >= 0 && p.Position is null);
    }
    public void Clear() => pops.Clear();
    private static double Ease(double x)
    {
        double lo = 0, hi = 1;
        for (int i = 0; i < 24; i++)
        {
            double t = (lo + hi) / 2;
            if (3 * (1 - t) * t * t * .58 + t * t * t < x) lo = t; else hi = t;
        }
        double u = (lo + hi) / 2;
        return 3 * (1 - u) * u * u + u * u * u;
    }
}
