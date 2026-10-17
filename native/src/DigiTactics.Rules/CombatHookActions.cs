namespace DigiTactics.Rules;

public enum CombatStat { AttackSpeed, AttackDamage, AbilityPower, Armor, MagicResist, DamageReduction, Amplification, CriticalChance, Dodge, Omnivamp, Regeneration }
internal readonly record struct ScheduledCombatAction(double At, Action Action);
internal readonly record struct TimedCombatBuff(CombatStat Stat, double Value, double Until);

public sealed partial class CombatSimulation
{
    private readonly List<ScheduledCombatAction> timers = [];

    private void ApplyCombatBuffs(int team, CombatBuffs? buffs, int? owner)
    {
        if (buffs is null) return;
        foreach (var unit in units)
        {
            if (unit.Team != team || (owner is not null && unit.Entry.Owner != owner)) continue;
            unit.AttackSpeed *= 1 + buffs.AttackSpeed;
            unit.Armor += buffs.Armor; unit.MagicResist += buffs.Armor; unit.Mana += buffs.Mana;
        }
    }

    internal void Schedule(double delay, Action action, CombatUnit? unit)
    {
        (unit?.Scheduled ?? timers).Add(new(Time + delay, action));
    }

    private void RunTimers(List<ScheduledCombatAction> pending)
    {
        // Snapshot before executing: callbacks scheduled by callbacks wait for the next update.
        if (!pending.Any(t => t.At <= Time)) return;
        var due = pending.Where(t => t.At <= Time).ToArray();
        pending.RemoveAll(t => t.At <= Time);
        foreach (var timer in due) timer.Action();
    }

    internal void Buff(CombatUnit unit, CombatStat stat, double value, double duration)
    {
        if (stat == CombatStat.AttackSpeed) { unit.SpeedBuffs.Add(new(value, Time + duration)); return; }
        AddStat(unit, stat, value);
        if (!double.IsPositiveInfinity(duration)) unit.TimedBuffs.Add(new(stat, value, Time + duration));
    }

    private void ExpireBuffs(CombatUnit unit)
    {
        foreach (var buff in unit.TimedBuffs) if (buff.Until <= Time) AddStat(unit, buff.Stat, -buff.Value);
        unit.TimedBuffs.RemoveAll(b => b.Until <= Time);
    }

    private static void AddStat(CombatUnit unit, CombatStat stat, double value)
    {
        switch (stat)
        {
            case CombatStat.AttackDamage: unit.AttackDamage += value; break;
            case CombatStat.AbilityPower: unit.AbilityPower += value; break;
            case CombatStat.Armor: unit.Armor += value; break;
            case CombatStat.MagicResist: unit.MagicResist += value; break;
            case CombatStat.DamageReduction: unit.Equipment.Reduction += value; break;
            case CombatStat.Amplification: unit.Equipment.Amp += value; break;
            case CombatStat.CriticalChance: unit.CriticalChance += value; break;
            case CombatStat.Dodge: unit.Dodge += value; break;
            case CombatStat.Omnivamp: unit.Omnivamp += value; break;
            case CombatStat.Regeneration: unit.Equipment.Regen += value; break;
            default: throw new ArgumentOutOfRangeException(nameof(stat));
        }
    }

    internal double HookDamage(CombatUnit source, CombatUnit target, double amount, DamageKind kind, bool critical) => Damage(source, target, amount, kind, critical: critical);
    internal double HookAbility(CombatUnit source, CombatUnit target, double amount) => AbilityDamage(source, target, amount);
    internal void HookHeal(CombatUnit target, double amount, bool quiet) => Heal(target, amount, quiet);
    internal void HookStun(CombatUnit target, double duration) => StunUnit(target, duration);
    internal void HookBurn(CombatUnit source, CombatUnit target) => ApplyBurn(source, target);
    internal double NextRandom() => random.Next();
    internal bool HookEvery(CombatUnit unit, string key, double period) => Every(unit, key, period);
    internal void CastAgain(CombatUnit unit, CombatUnit target) { if (unit.Alive) Cast(unit, target); }

    internal void AddShield(CombatUnit unit, double amount)
    {
        if (amount > 0)
        {
            unit.Shield += amount;
            if (Hooks.Has(CombatHook.Shield, unit, CombatHook.AnyShield))
                Hooks.Fire(CombatHook.Shield, unit, new() { Amount = amount }, CombatHook.AnyShield);
        }
        Emit("shield", unit);
    }

    internal void Kill(CombatUnit unit, CombatUnit? source)
    {
        unit.Alive = false; unit.Health = 0;
        if (occupancy[grid.Index(unit.Cell)] == unit) occupancy[grid.Index(unit.Cell)] = null;
        Emit("death", unit, source);
        if (Hooks.Has(CombatHook.Death, unit, CombatHook.AnyDeath))
            Hooks.Fire(CombatHook.Death, unit, new() { Source = source }, CombatHook.AnyDeath);
        if (source is not null && source != unit && Hooks.Has(CombatHook.Kill, source, CombatHook.AnyKill))
            Hooks.Fire(CombatHook.Kill, source, new() { Target = unit }, CombatHook.AnyKill);
    }
}
