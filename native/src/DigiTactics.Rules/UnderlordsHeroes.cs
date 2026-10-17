namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void StartUnderlordsHero(CombatHookContext c, CombatUnit u)
    {
        bool three = u.Stage == 2;
        switch (u.Line.Id) {
            case "ul_dazzle": if (three) foreach (var a in UnderlordsAllies(u, 1)) if (a != u) a.Underlords.Grave = Math.Max(a.Underlords.Grave, 5); break;
            case "ul_drow": foreach (var a in UnderlordsAllies(u, 1)) a.AttackSpeed *= 1 + UnderlordsValue(u, 0.1, 0.2, 0.3); break;
            case "ul_phantom":
                u.CriticalChance += UnderlordsValue(u, 0.15, 0.2, 0.25); u.CriticalDamage = Math.Max(u.CriticalDamage, UnderlordsValue(u, 2.5, 3.5, 4.5));
                // Rogue inheritance is bound with alliance counts, once that layer is enabled.
                break;
            case "ul_meepo": if (!u.Summoned) UnderlordsSummon(u, "ul_meepo", u.Stage, u, options: new(Items: three ? u.Items.ToArray() : []), scale: 0.45); break;
            case "ul_alchemist": if (three) { c.Buff(u, CombatStat.AttackSpeed, 0.75, 5); u.Equipment.CrowdControlImmunity = Math.Max(u.Equipment.CrowdControlImmunity, 5); } break;
            case "ul_lifestealer": if (!three) { u.MaxMana = 0; u.Mana = 0; } break;
            case "ul_puck": if (three) u.MaxMana = Math.Min(u.MaxMana, 25); break;
        }
    }
    private void TickUnderlordsHero(CombatHookContext c, CombatUnit u)
    {
        var s = u.Underlords; bool three = u.Stage == 2;
        switch (u.Line.Id) {
            case "ul_crystal":
                if (three && c.Every(u, "ulCM", 1)) foreach (var a in UnderlordsAllies(u)) if (a.MaxMana > 0) a.Mana = Math.Min(a.MaxMana, a.Mana + 3); break;
            case "ul_enchantress":
                if (s.Attend is { } attend && Time <= attend.Until && Time >= attend.Next) {
                    attend.Next = Time + 1.5; foreach (var a in UnderlordsAllies(u, 4)) c.Heal(a, attend.Value, true);
                } break;
            case "ul_bristleback":
                if (three && u.Alive && u.Stun <= 0 && u.SilenceUntil <= Time && c.Every(u, "ulQuillAuto", 1.5)) UnderlordsQuill(u, UnitValues.AbilityValue(u.Line, u.Stage) * u.AbilityPower / 100); break;
            case "ul_juggernaut":
                if (s.Fury is { } fury && Time <= fury.Until && Time >= fury.Next) {
                    fury.Next = Time + 0.5; foreach (var e in EnemiesNear(u, u.Cell, 1)) c.Damage(u, e, fury.Value * 0.5);
                } break;
            case "ul_slark":
                if (!three || !c.Every(u, "ulPact", 6)) break;
                c.After(1, () => {
                    u.Stun = 0; u.Weakness = 0; u.BurnSource = null; u.BurnUntil = 0; u.BurnAccumulated = 0;
                    if (s.Poison is { } poison) { poison.Stacks = 0; if (poison.Buff is not null) UnderlordsSpeed(u, ref poison.Buff, 0); }
                    foreach (var e in EnemiesNear(u, u.Cell, 2)) c.Damage(u, e, 300);
                    u.Health = Math.Max(1, u.Health - 150);
                }, u); break;
        }
    }
    private void AttackUnderlordsHero(CombatHookContext c, CombatHookEvent e)
    {
        var u = e.Unit!; var target = e.Target!; var s = u.Underlords; bool three = u.Stage == 2;
        switch (u.Line.Id) {
            case "ul_antimage":
                if (!target.Alive || target.MaxMana <= 0) break;
                double before = target.Mana, burned = Math.Min(Math.Max(0, before), UnderlordsValue(u, 10, 20, 32)); target.Mana -= burned;
                if (burned > 0) c.Damage(u, target, burned * 0.5);
                if (three && before > 0 && target.Mana <= 0 && target.Alive) c.Damage(u, target, 150); break;
            case "ul_batrider": if (s.Napalm != 0 && target.Alive) c.Damage(u, target, s.Napalm * UnderlordsValue(u, 20, 35, 55)); break;
            case "ul_drow": if (three && target.Alive) UnderlordsTimedDebuff(target, "ulFrostArrow", CombatStat.AttackSpeed, 0.2, 2); break;
            case "ul_s_ward": UnderlordsPoison(u, target); break;
            case "ul_kunkka":
                if (!three || u.Attacks % 4 != 0 || !target.Alive) break;
                c.Damage(u, target, 150, DamageKind.Physical);
                foreach (var x in EnemiesNear(u, target.Cell, 1)) if (x != target) c.Damage(u, x, u.AttackDamage * 0.75, DamageKind.Physical); break;
            case "ul_luna":
                var hit = new List<CombatUnit> { target }; var current = target; double damage = e.Amount;
                for (int k = 0; k < UnderlordsValue(u, 3, 5, 7); k++) {
                    damage *= 0.7; var cell = current.Cell;
                    var next = EnemiesOf(u).Where(x => !hit.Contains(x) && x.Cell.DistanceTo(cell) <= 2).OrderBy(x => x.Cell.DistanceTo(cell)).FirstOrDefault();
                    if (next is null) break;
                    hit.Add(next); c.Damage(u, next, damage, DamageKind.Physical); current = next;
                } break;
            case "ul_spiritbreaker":
                if (!three || !target.Alive || s.BashCooldown > Time || c.Random() >= 0.25) break;
                s.BashCooldown = Time + 1; c.Damage(u, target, 150, DamageKind.Physical); if (target.Alive) c.Stun(target, 2); break;
            case "ul_stormspirit":
                if (!s.Overload || !target.Alive) break;
                s.Overload = false; c.Damage(u, target, 150); if (target.Alive) UnderlordsTimedDebuff(target, "ulOverload", CombatStat.AttackSpeed, 0.5, 1.5); break;
            case "ul_abaddon":
                if (!three) break;
                s.CurseCount = s.CurseTarget == target ? s.CurseCount + 1 : 1; s.CurseTarget = target;
                if (s.CurseCount == 3 && target.Alive) { c.Silence(target, 2); c.Buff(u, CombatStat.AttackSpeed, 0.3, 2); } break;
            case "ul_lifestealer":
                if (!target.Alive) break;
                double feast = target.MaxHealth * UnderlordsValue(u, 0.035, 0.045, 0.055); c.Damage(u, target, feast, DamageKind.Physical); c.Heal(u, feast, true); break;
            case "ul_slark":
                double stolen = UnderlordsValue(u, 6, 8, 10); u.AttackDamage += stolen; if (target.Alive) target.AttackDamage = Math.Max(0, target.AttackDamage - stolen);
                s.Shift = Math.Min(30, s.Shift + 1); UnderlordsSpeed(u, ref s.ShiftBuff, s.Shift * 0.04); break;
            case "ul_trollwarlord":
                s.Fervor = s.FervorTarget == target ? Math.Min((int)UnderlordsValue(u, 8, 12, 16), s.Fervor + 1) : 0;
                s.FervorTarget = target; UnderlordsSpeed(u, ref s.FervorBuff, s.Fervor * UnderlordsValue(u, 0.15, 0.2, 0.25)); break;
        }
    }
    private void StruckUnderlordsHero(CombatHookContext c, CombatHookEvent e)
    {
        var u = e.Target!;
        if (u.Line.Id == "ul_enchantress" && u.Stage == 2 && c.Random() < 0.5) UnderlordsTimedDebuff(e.Unit!, "ulUntouch", CombatStat.AttackSpeed, 0.5, 1.5);
        if (u.Line.Id == "ul_axe" && u.Alive && c.Random() < UnderlordsValue(u, 0.2, 0.22, 0.25))
            foreach (var x in EnemiesNear(u, u.Cell, 1)) c.Damage(u, x, UnderlordsValue(u, 100, 180, 300), DamageKind.True);
    }
    private void HurtUnderlordsHero(CombatHookContext c, CombatUnit u)
    {
        if (u.Line.Id != "ul_luna" || u.Stage != 2 || u.Health >= u.MaxHealth * 0.5 || u.Underlords.Eclipse) return;
        u.Underlords.Eclipse = true; int count = 0;
        void Beam() {
            if (!u.Alive || count >= 15) return; count++;
            var pool = EnemiesNear(u, u.Cell, 2).Where(x => x.Underlords.Beams < 5).ToArray();
            if (pool.Length > 0) {
                var x = pool[(int)(c.Random() * pool.Length)]; x.Underlords.Beams++;
                c.Damage(u, x, 150); if (x.Alive) c.Stun(x, 0.5);
            }
            c.After(0.3, Beam, u);
        }
        Beam();
    }
}
