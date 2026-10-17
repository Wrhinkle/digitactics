namespace DigiTactics.Rules;

internal sealed class UnderlordsUnitState
{
    internal Dictionary<string, double> EffectUntil { get; } = [];
    internal bool Fell, Sage;
    internal double VoidCooldown;
    internal UnderlordsRogue? Rogue;
    internal UnderlordsVigilant? Vigilant;
    internal bool Metamorphosed, Shifted, LivingArmor, GraveDone, Eclipse, Overload;
    internal double EmpowerUntil, MagicImmuneUntil, StoneUntil, EvadeUntil, Grave, GraveUntil, BashCooldown;
    internal int Napalm, Quill, Refract, RefractHits, Beams, CurseCount, Shift, Fervor;
    internal double RefractAmp, Kills, Gold, DuelWin;
    internal CombatUnit? CurseTarget, FervorTarget;
    internal AttackSpeedBuff? ShiftBuff, FervorBuff;
    internal int? Summoner;
    internal UnderlordsPulse? Attend, FrostAura, Fury, Exorcism;
    internal UnderlordsShield? Aphotic;
    internal UnderlordsMark? Axed;
    internal UnderlordsDuel? Duel;
    internal UnderlordsPoison? Poison;
    internal List<UnderlordsDot> Dots { get; } = [];
    internal List<(CombatUnit Unit, double Health)> Juggernaut { get; } = [];
}
internal sealed class UnderlordsPulse(double until, double next, double value, CombatUnit source, int left = 0)
{
    internal double Until = until, Next = next, Value = value;
    internal CombatUnit Source = source;
    internal int Left = left;
}
internal sealed record UnderlordsShield(CombatUnit Source, double Value, double Until);
internal sealed record UnderlordsMark(int SourceId, double Amp, double Until);
internal sealed record UnderlordsDuel(CombatUnit Opponent, double Until);
internal sealed class UnderlordsDot(CombatUnit source, double dps, double until, bool leech, bool armor)
{
    internal readonly CombatUnit Source = source;
    internal readonly double Dps = dps, Until = until;
    internal readonly bool Leech = leech, Armor = armor;
    internal double Accumulated;
}
internal sealed class UnderlordsPoison(CombatUnit source)
{
    internal CombatUnit Source = source;
    internal int Stacks;
    internal double Until, Accumulated;
    internal AttackSpeedBuff? Buff;
}
internal sealed class UnderlordsZone(CombatUnit source, HexCell cell, double until, double dps, double armor, double next)
{
    internal readonly CombatUnit Source = source;
    internal readonly HexCell Cell = cell;
    internal readonly double Until = until, Dps = dps, Armor = armor;
    internal double Next = next;
}

internal sealed record UnderlordsRogue(double Evasion, double AttackDamage);
internal sealed record UnderlordsVigilant(int Target, double Amp);
