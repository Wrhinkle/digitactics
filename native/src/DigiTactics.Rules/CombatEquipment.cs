namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private bool Every(CombatUnit unit, string key, double period)
    {
        double next = unit.Timers.GetValueOrDefault(key, period);
        if (Time < next) return false;
        unit.Timers[key] = next + period;
        return true;
    }

    private void Passives(CombatUnit unit)
    {
        var p = unit.Equipment;
        if (unit.BurnSource is { } source && unit.BurnUntil > Time)
        {
            unit.BurnAccumulated += unit.MaxHealth * 0.015 * Tick;
            if (unit.BurnAccumulated >= 20)
            {
                double amount = unit.BurnAccumulated; unit.BurnAccumulated = 0;
                Damage(source, unit, amount, DamageKind.True, true);
                if (!unit.Alive) return;
            }
        }
        RunTimers(unit.Scheduled);
        if (Hooks.Has(CombatHook.UnitTick, unit)) Hooks.Fire(CombatHook.UnitTick, unit, new() { DeltaTime = Tick });
        if (p.Archangel != 0 && Every(unit, "arch", 5)) unit.AbilityPower += p.Archangel;
        if (p.ManaRegen != 0 && Every(unit, "mana", 3)) unit.Mana = Math.Min(unit.MaxMana, unit.Mana + p.ManaRegen);
        if (p.Crown && !unit.CrownApplied && Time >= 8) { unit.CrownApplied = true; unit.AbilityPower += 30; }
        if (p.Redemption && Every(unit, "redemption", 5))
        {
            foreach (var ally in units) if (ally.Alive && ally.Team == unit.Team && ally.Cell.DistanceTo(unit.Cell) <= 2)
                Heal(ally, 80 + (ally.MaxHealth - ally.Health) * 0.1);
            Burst(unit.Cell, 2, 1 - unit.Team);
        }
        if (p.Sunfire && Every(unit, "sunfire", 2))
        {
            var enemy = EnemiesNear(unit, unit.Cell, 2).OrderBy(e => unit.Cell.DistanceTo(e.Cell)).FirstOrDefault();
            if (enemy is not null) ApplyBurn(unit, enemy);
        }
        if ((p.Gargoyle != 0 || p.Veil != 0) && Every(unit, "aura", 1))
        {
            var near = EnemiesNear(unit, unit.Cell, 2);
            if (p.Gargoyle != 0) unit.Gargoyle = near.Length * p.Gargoyle;
            if (p.Veil != 0) foreach (var enemy in near)
            { enemy.Veil = Math.Max(enemy.VeilUntil > Time ? enemy.Veil : 0, p.Veil); enemy.VeilUntil = Time + 1.1; }
        }
    }

    private void ApplyBurn(CombatUnit source, CombatUnit target)
    {
        if (!target.Alive) return;
        target.BurnSource = source; target.BurnUntil = Time + 5; target.WoundUntil = Time + 5;
    }

    private void StunUnit(CombatUnit unit, double duration)
    {
        if (Time < unit.Equipment.CrowdControlImmunity) { Emit("text", unit, text: "Immune"); return; }
        unit.Stun = Math.Max(unit.Stun, duration);
    }

    private static void TitanStack(CombatUnit unit)
    {
        if (unit.TitanStacks >= 25) return;
        unit.TitanStacks++; unit.AttackDamage += unit.BaseAttackDamage * 0.02; unit.AbilityPower += 2;
    }

    private void CheckLifelines(CombatUnit unit)
    {
        foreach (var effect in unit.Equipment.Lifelines)
        {
            if (effect.Used || unit.Health / unit.MaxHealth >= effect.Threshold) continue;
            effect.Used = true; unit.Shield += unit.MaxHealth * effect.Shield;
            if (effect.Speed != 0) unit.SpeedBuffs.Add(new(effect.Speed, double.PositiveInfinity));
            unit.AttackDamage += unit.BaseAttackDamage * effect.Attack;
            unit.Armor += effect.Armor; unit.MagicResist += effect.Armor; Emit("shield", unit);
        }
    }

    private void AfterCast(CombatUnit unit)
    {
        var p = unit.Equipment;
        if (p.CastSpeed != 0) unit.SpeedBuffs.Add(new(p.CastSpeed, Time + 5));
        if (p.BlueBuff) unit.Mana = 20;
        foreach (var enemy in units)
            if (enemy.Alive && enemy.Team != unit.Team && enemy.Equipment.Zap != 0 && enemy.Cell.DistanceTo(unit.Cell) <= 2)
                { Emit("beam", enemy, unit); Damage(enemy, unit, enemy.Equipment.Zap, DamageKind.Magic); }
    }

    private double AbilityDamage(CombatUnit source, CombatUnit target, double value)
    {
        bool critical = source.Equipment.AbilityCrit && source.CriticalChance > 0 && random.Next() < source.CriticalChance;
        double result = Damage(source, target, critical ? value * source.CriticalDamage : value, DamageKind.Magic, critical: critical);
        if (source.AbilityBurn && target.Alive) ApplyBurn(source, target);
        return result;
    }
}
