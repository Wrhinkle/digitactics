namespace DigiTactics.Rules;

public sealed record PartnerModifier(string? Bonus, string? Trait = null);
public sealed record CombatEntry(string Line, int Stage, int Column, int Row, int Uid = 0, int? Owner = null, string[]? Items = null, Dictionary<string, int>? BonusTraits = null, PartnerModifier? Partner = null);
public readonly record struct CombatEvent(double Time, string Type, int Unit, int? Target = null, double Amount = 0);
public readonly record struct AttackSpeedBuff(double Value, double Until, bool Infiltrator = false);

/// <summary>Runtime state belongs to one battle; content definitions are shared and never modified.</summary>
public sealed class CombatUnit
{
    public int Id { get; }
    public CombatEntry Entry { get; }
    public UnitLine Line { get; }
    public int Team { get; }
    public int Stage { get; internal set; }
    public bool Summoned { get; internal set; }
    public int BlossomKills { get; internal set; }
    public double TimeRune { get; internal set; }
    internal bool FrenzyActive;
    internal bool FatesGrace;
    public int CombatGold { get; internal set; }
    internal GalaxiesUnitState Galaxies { get; } = new();
    internal FatesItemState FatesItems { get; } = new();
    public IReadOnlyList<string> Traits { get; internal set; } = Array.Empty<string>();
    internal double PerHexAmplification, BonusAgainstBurning, AttackSpeedPerAttack;
    internal int AttackSpeedStacks;
    internal int AttackSpeedStackLimit = 12;
    internal double CharmUntil;
    internal bool AbilityBurn;
    public HexCell Cell { get; internal set; }
    public bool Alive { get; internal set; } = true;
    public double MaxHealth { get; internal set; }
    public double Health { get; internal set; }
    public double AttackDamage { get; internal set; }
    public double AttackSpeed { get; internal set; }
    public double Range { get; internal set; }
    public double Armor { get; internal set; }
    public double MagicResist { get; internal set; }
    public double Mana { get; internal set; }
    public double MaxMana { get; internal set; }
    public double AbilityPower { get; internal set; } = 100;
    public double Shield { get; internal set; }
    public double AttackCooldown { get; internal set; }
    public double MoveCooldown { get; internal set; }
    public double Stun { get; internal set; }
    public double CastLock { get; internal set; }
    public double Weakness { get; internal set; }
    public double WeakUntil { get; internal set; }
    public double Dealt { get; internal set; }
    public double Taken { get; internal set; }
    public double Healed { get; internal set; }
    public int Attacks { get; internal set; }
    public bool Leap { get; internal set; }
    public bool IsPiloting => Galaxies.Piloting;
    public bool Leapt { get; internal set; }
    public CombatUnit? Target { get; internal set; }
    internal List<AttackSpeedBuff> SpeedBuffs { get; } = [];
    internal EquipmentEffects Equipment { get; } = new();
    public IReadOnlyList<string> Items { get; internal set; } = Array.Empty<string>();
    public double BaseAttackDamage { get; internal set; }
    public double CriticalChance { get; internal set; }
    public double CriticalDamage { get; internal set; } = 1.6;
    public double Dodge { get; internal set; }
    public double Omnivamp { get; internal set; }
    internal int RageStacks, TitanStacks;
    internal double Gargoyle, Veil, VeilUntil, Shred, ShredUntil, ThornCooldown, WoundUntil;
    internal CombatUnit? BurnSource;
    internal double BurnUntil, BurnAccumulated;
    internal bool CrownApplied;
    public double SilenceUntil { get; internal set; }
    public double InvulnerableUntil { get; internal set; }
    public bool Blind { get; internal set; }
    internal List<ScheduledCombatAction> Scheduled { get; } = [];
    internal List<TimedCombatBuff> TimedBuffs { get; } = [];
    internal Dictionary<string, double> Timers { get; } = [];

    internal CombatUnit(int id, CombatEntry entry, UnitLine line, int team, HexCell cell, double random)
    {
        Id = id; Entry = entry; Stage = entry.Stage; Leap = line.Combat.Leap; Line = line; Team = team; Cell = cell;
        var stats = UnitValues.Stats(line, entry.Stage);
        MaxHealth = Health = stats.MaxHealth;
        BaseAttackDamage = AttackDamage = stats.AttackDamage; AttackSpeed = stats.AttackSpeed; Range = stats.Range;
        Armor = stats.Armor; MagicResist = stats.MagicResist;
        MaxMana = stats.MaxMana; Mana = stats.StartMana;
        AttackCooldown = 0.15 + random * 0.3;
    }
}
