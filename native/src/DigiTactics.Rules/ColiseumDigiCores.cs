namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void StartDigiCore(CombatHookContext c, CombatUnit u)
    {
        switch (u.Entry.Coliseum?.PowerUp) {
            case "burst": case "overdrive": u.Equipment.Amp += 0.12; break;
            case "dark": u.AttackDamage += u.BaseAttackDamage * 0.22; u.Omnivamp += 0.1; break;
            case "warp": u.AttackDamage += u.BaseAttackDamage * 0.08; u.AbilityPower += 8; u.Armor += 10; u.MagicResist += 10; u.Equipment.Amp += 0.05; break;
            case "armor": u.Shield += u.MaxHealth * 0.25; break;
            case "mode":
                double amount = u.MaxHealth * 0.18; u.Shield += amount;
                var ally = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Cell.DistanceTo(u.Cell)).FirstOrDefault();
                if (ally is not null) ally.Shield += amount; break;
            case "deathx": if (!u.Leapt) u.Leap = true; u.CriticalChance += 0.3; u.CriticalDamage += 0.1; break;
            case "mega": u.AttackDamage += u.BaseAttackDamage * 0.4; break;
            case "sage": u.AbilityPower += 25; break;
            case "golem": u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.3); break;
            case "speed": u.AttackSpeed *= 1.3; break;
            case "overwrite": u.CriticalChance += 0.3; u.CriticalDamage += 0.15; u.Equipment.AbilityCrit = true; break;
            case "matrix": if (u.MaxMana > 0) { u.MaxMana = Math.Max(10, UnitValues.Round(u.MaxMana * 0.75)); u.Mana += 10; } break;
            case "atomic": u.Coliseum.Atomic = 0.1; break;
            case "glacier": u.AttackSpeed *= 1.15; break;
            case "inferno": u.AbilityBurn = true; u.BonusAgainstBurning += 0.15; break;
            case "hunter": u.Equipment.Amp += 0.08; break;
            case "guardian":
                u.Armor += 30; u.MagicResist += 30;
                foreach (var a in c.Adjacent(u)) if (a.Team == u.Team) { a.Armor += 10; a.MagicResist += 10; }
                break;
        }
    }

    private void TickDigiCore(CombatHookContext c, CombatUnit u)
    {
        var s = u.Coliseum;
        switch (u.Entry.Coliseum?.PowerUp) {
            case "dark": if (c.Every(u, "pu_dark", 1)) u.Health = Math.Max(1, u.Health - u.MaxHealth * 0.01); break;
            case "armor": if (u.Shield > 0 && c.Every(u, "pu_armor", 0.5)) c.Buff(u, CombatStat.AttackSpeed, 0.3, 0.5); break;
            case "biomerge": if (s.Biomerge < 30 && c.Every(u, "pu_bio", 1)) { s.Biomerge++; u.AttackDamage += u.BaseAttackDamage * 0.03; u.AbilityPower += 3; } break;
            case "overdrive": if (!s.Overdrive && Time >= 10) KillDigiCore(c, u); break;
            case "atomic": if (c.Every(u, "pu_atom", 1)) s.Atomic += 0.02; break;
            case "ascend": if (!s.Ascended && Time >= 6) { s.Ascended = true; u.Equipment.Amp += 0.4; c.Text(u, "Ascension"); } break;
        }
    }

    private void KillDigiCore(CombatHookContext c, CombatUnit u)
    {
        var s = u.Coliseum;
        if (u.Entry.Coliseum?.PowerUp == "burst" && !s.Burst) { s.Burst = true; u.Equipment.Amp += 0.13; }
        if (u.Entry.Coliseum?.PowerUp != "overdrive" || s.Overdrive) return;
        s.Overdrive = true; u.Equipment.Amp += 0.2;
        foreach (var e in EnemiesNear(u, u.Cell, 2)) c.Stun(e, 1.5);
        c.Burst(u.Cell, 2, u.Team); c.Text(u, "Overdrive");
    }

    private void TakenDigiCore(CombatHookContext c, CombatHookEvent e)
    {
        var u = e.Unit!; var s = u.Coliseum;
        switch (u.Entry.Coliseum?.PowerUp) {
            case "xanti":
                if (s.Adaptive >= 90) return;
                if (e.Kind == DamageKind.Physical) { u.Armor += 4; s.Adaptive += 4; }
                else if (e.Kind == DamageKind.Magic) { u.MagicResist += 8; s.Adaptive += 8; }
                break;
            case "spirit":
                if (s.Spirit || u.Health >= u.MaxHealth * 0.4) return;
                s.Spirit = true; c.Heal(u, u.MaxHealth * 0.25); c.Buff(u, CombatStat.AttackSpeed, 0.25); c.Text(u, "Spirit Evolution"); break;
            case "thorn":
                if (e.Kind != DamageKind.Physical || e.Source is null || u.Cell.DistanceTo(e.Source.Cell) > 1 || s.ThornCooldown > Time) return;
                s.ThornCooldown = Time + 1; foreach (var a in EnemiesNear(u, u.Cell, 1)) c.Damage(u, a, 50 + u.MaxHealth * 0.05); break;
        }
    }
}
