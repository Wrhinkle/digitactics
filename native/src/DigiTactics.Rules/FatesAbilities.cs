namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> FatesAbilities = ["grow", "blossom", "burst", "frenzy", "lullaby", "showstopper", "rewind"];
    private bool CastFatesAbility(CombatUnit unit, CombatUnit target, double value)
    {
        switch (unit.Line.Ability.Kind)
        {
            case "grow":
                var ally = units.Where(u => u.Alive && u.Team == unit.Team && !u.Summoned).OrderBy(u => u.Health / u.MaxHealth).FirstOrDefault();
                if (ally is null) break;
                ally.MaxHealth += value; Heal(ally, value); Emit("shield", ally);
                foreach (var enemy in EnemiesNear(unit, ally.Cell, 1)) StunUnit(enemy, 1);
                break;
            case "blossom":
                var weakest = units.Where(u => u.Alive && u.Team != unit.Team).OrderBy(u => u.Health).FirstOrDefault() ?? target;
                AbilityDamage(unit, weakest, value);
                if (!weakest.Alive) unit.BlossomKills++;
                break;
            case "burst":
                foreach (var enemy in EnemiesNear(unit, target.Cell, 1)) { StunUnit(enemy, 1.5); AbilityDamage(unit, enemy, value); }
                break;
            case "frenzy":
                if (!unit.FrenzyActive)
                { unit.FrenzyActive = true; unit.SpeedBuffs.Add(new(0.6, double.PositiveInfinity)); unit.Omnivamp += 0.25; }
                else Heal(unit, value);
                Emit("shield", unit); break;
            case "lullaby":
                foreach (var enemy in units.Where(u => u.Alive && u.Team != unit.Team).OrderByDescending(u => u.Health).ThenBy(u => u.Id).Take(unit.Stage + 2).ToArray())
                { StunUnit(enemy, 1.5 + unit.Stage * 0.5); AbilityDamage(unit, enemy, value); }
                break;
            case "showstopper":
                double amount = value + target.MaxHealth * (unit.Stage == 0 ? 0.06 : unit.Stage == 1 ? 0.1 : 0.2);
                var splash = EnemiesNear(unit, target.Cell, 1).Where(u => u != target).ToArray();
                AbilityDamage(unit, target, amount);
                foreach (var enemy in splash) AbilityDamage(unit, enemy, amount * 0.5);
                break;
            case "rewind":
                foreach (var friend in units.Where(u => u.Alive && u.Team == unit.Team && u != unit && !u.Summoned && u.TimeRune == 0)
                    .OrderBy(u => u.Health / u.MaxHealth).ThenBy(u => u.Id).Take(unit.Stage + 1).ToArray())
                { friend.TimeRune = value; Emit("shield", friend); }
                break;
            default: return false;
        }
        return true;
    }
}

internal static class FatesCombat
{
    internal static void Bind(CombatSimulation combat, Ruleset rules, CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        FatesTraits.Bind(combat, rules, sideA, sideB, setup);
        combat.BindSummonedItems = unit => FatesItems.Bind(combat, rules, unit);
        foreach (var unit in combat.Units) FatesItems.Bind(combat, rules, unit);
        var mechanic = combat.Hooks.Context(CombatHookSource.Mechanic, "partners");
        combat.Hooks.Add(mechanic, CombatHook.CombatStart, (_, _) => {
            foreach (var u in combat.Units)
            {
                if (u.Entry.Partner is { } modifier && !u.Summoned)
                {
                    var p = rules.Partner ?? throw new InvalidDataException("Missing Fates Partner tuning.");
                    u.MaxHealth += p.Hp;
                    switch (modifier.Bonus)
                    {
                        case "hp": u.MaxHealth += p.BonusHp; break;
                        case "ap": u.AbilityPower += p.BonusAp; break;
                        case "ad": u.AttackDamage += u.BaseAttackDamage * p.BonusAd; break;
                        case "mana": u.MaxMana = Math.Max(10, UnitValues.Round(u.MaxMana * (1 - p.BonusMana))); break;
                    }
                }
                if (u.Line.Id == "fat_aruraumon" && !u.Summoned)
                    u.AbilityPower += (u.Team == 0 ? setup.SideA.Fates : setup.SideB.Fates)?.Blossom ?? 0;
            }
        }, 5);
        combat.Hooks.Add(mechanic, CombatHook.Lethal, (c, e) => {
            var unit = e.Unit!;
            if (unit.TimeRune == 0 || unit.Health > 0) return;
            double value = unit.TimeRune; unit.TimeRune = 0;
            unit.Health = 1; unit.InvulnerableUntil = c.Time + 2; unit.Stun = Math.Max(unit.Stun, 2);
            c.After(2, () => { c.Heal(unit, value); c.Buff(unit, CombatStat.AttackSpeed, 0.3); }, unit);
        });
    }
}
