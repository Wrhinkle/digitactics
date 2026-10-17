namespace DigiTactics.Rules;

public enum DamageKind { Physical, Magic, True }

public readonly record struct DamageSource(string Attribute, double Amplification = 0, bool Weakened = false,
    double Weakness = 0, double BonusAgainstBurning = 0, double PerHexAmplification = 0,
    double GiantSlayer = 0, double GuardBreaker = 0);

public readonly record struct DamageTarget(string Attribute, double MaxHealth, double Armor, double MagicResist,
    double Shield = 0, double Mana = 0, double MaxMana = 0, double Reduction = 0,
    double Gargoyle = 0, double Veil = 0, double Shred = 0, bool Burning = false,
    bool Invulnerable = false, bool Alive = true);

public readonly record struct DamageRequest(DamageSource Source, DamageTarget Target, DamageKind Kind, double Amount,
    int Distance = 0, double Overtime = 1, double ExtraReduction = 0, double IncomingMultiplier = 1,
    double? OutgoingMultiplierOverride = null, bool IsBurn = false);

public readonly record struct DamageResolution(double Amount, double ShieldAbsorbed, double HealthDamage, double ManaAfter);

/// <summary>
/// Shared damage pipeline. Effects provide their active values; hooks can override outgoing amplification
/// and supply incoming reduction/multipliers. Combat owns health changes, lethal hooks, healing and events.
/// </summary>
public static class DamageRules
{
    public static DamageResolution Resolve(in DamageRequest request)
    {
        var target = request.Target;
        if (!target.Alive || target.Invulnerable) return new(0, 0, 0, target.Mana);
        return ResolveMitigated(request, Mitigate(request, OutgoingMultiplier(request)));
    }

    /// <summary>Finish a damage operation after incoming hooks. Resistance has already been applied.</summary>
    public static DamageResolution ResolveMitigated(in DamageRequest request, double amount)
    {
        var target = request.Target;
        amount *= request.IncomingMultiplier;
        amount *= 1 - Math.Min(0.6, target.Reduction + request.ExtraReduction);
        double shield = target.Shield > 0 ? Math.Min(target.Shield, amount) : 0;
        double mana = target.Mana;
        if (target.MaxMana > 0 && !request.IsBurn)
            mana = Math.Min(target.MaxMana, mana + Math.Min(15, amount * 0.06));
        return new(amount, shield, amount - shield, mana);
    }

    public static double OutgoingMultiplier(in DamageRequest request)
    {
        var source = request.Source; var target = request.Target;
        double multiplier = 1 + source.Amplification;
        if (source.Weakened) multiplier *= 1 - source.Weakness;
        if (target.Burning && source.BonusAgainstBurning != 0) multiplier *= 1 + source.BonusAgainstBurning;
        if (HasAttributeAdvantage(source.Attribute, target.Attribute)) multiplier *= 1.2;
        if (source.PerHexAmplification != 0) multiplier *= 1 + source.PerHexAmplification * request.Distance;
        if (source.GiantSlayer != 0 && target.MaxHealth >= 1600) multiplier *= 1 + source.GiantSlayer;
        if (source.GuardBreaker != 0 && target.Shield > 0) multiplier *= 1 + source.GuardBreaker;
        return request.OutgoingMultiplierOverride ?? multiplier;
    }

    /// <summary>Damage after outgoing modifiers, overtime and resistance, before incoming hooks/reduction.</summary>
    public static double Mitigate(in DamageRequest request, double multiplier)
    {
        var target = request.Target;
        double amount = request.Amount * (multiplier * request.Overtime);
        if (request.Kind == DamageKind.Physical)
        {
            double armor = target.Armor + target.Gargoyle - target.Veil;
            armor *= 1 - target.Shred;
            amount *= 100 / (100 + Math.Max(0, armor));
        }
        else if (request.Kind == DamageKind.Magic)
            amount *= 100 / (100 + Math.Max(0, target.MagicResist + target.Gargoyle - target.Veil));
        return amount;
    }

    public static bool HasAttributeAdvantage(string attacker, string defender) => (attacker, defender) is
        ("Vaccine", "Virus") or ("Virus", "Data") or ("Data", "Vaccine");
}
