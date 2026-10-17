namespace DigiTactics.Rules;

/// <summary>Opening stage, repeating stages, and explicit mode-specific exceptions.</summary>
public sealed record RoundSchedule(string[] Opening, string[] Repeating, Dictionary<string, string> Overrides)
{
    public int RoundsInStage(int stage)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stage, 1);
        return stage == 1 ? Opening.Length : Repeating.Length;
    }

    public string TypeOf(RoundPosition position)
    {
        int count = RoundsInStage(position.Stage);
        if (position.Round < 1 || position.Round > count) throw new ArgumentOutOfRangeException(nameof(position));
        return Overrides.TryGetValue(position.Label, out var type) ? type
            : (position.Stage == 1 ? Opening : Repeating)[position.Round - 1];
    }

    public RoundPosition Next(RoundPosition position)
    {
        _ = TypeOf(position);
        return position.Round < RoundsInStage(position.Stage) ? position with { Round = position.Round + 1 }
            : new(checked(position.Stage + 1), 1);
    }
}

public readonly record struct RoundPosition(int Stage, int Round)
{
    public static RoundPosition First => new(1, 1);
    public string Label => $"{Stage}-{Round}";
}
