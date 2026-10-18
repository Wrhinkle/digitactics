namespace DigiTactics.Rules;

/// <summary>Presentation-only final blow metadata. Never consumes RNG or changes settlement.</summary>
public sealed record CombatFinalHit(int Id, int Uid, string Line, int Stage, int? Owner, int Victim, double Time);
