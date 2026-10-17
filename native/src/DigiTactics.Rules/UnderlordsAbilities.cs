namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private bool IsUnderlords => rules.Id.StartsWith("underlords/", StringComparison.Ordinal) || rules.Id.StartsWith("underlords_ko/", StringComparison.Ordinal);
    private static readonly HashSet<string> UnderlordsAbilities = [
        "ul_napalm", "ul_frostbite", "ul_attendants", "ul_frostshield", "ul_summon", "ul_quill", "ul_bladefury", "ul_duel", "ul_aphotic", "ul_acid", "ul_axes", "ul_rage", "ul_shapeshift", "ul_wards", "ul_leech", "ul_exorcism", "ul_doom", "ul_refraction", "ul_nethertoxin", "ul_gaze", "ul_wraithfire",
        "ul_passive", "ul_shuriken", "ul_shadowwave", "ul_empower", "ul_purge", "ul_haze", "ul_scatter", "ul_punch", "ul_terror",
        "ul_chaosbolt", "ul_grip", "ul_ghostship", "ul_poof", "ul_hook", "ul_scream", "ul_charge", "ul_sleight", "ul_purify",
        "ul_dagger", "ul_metamorph", "ul_laguna", "ul_arrow", "ul_shieldcrash", "ul_fadebolt", "ul_godstrength", "ul_ravage",
        "ul_dissimilate", "ul_breathefire", "ul_chrono", "ul_illuminate"
    ];

    private void UnderlordsTimedDebuff(CombatUnit target, string key, CombatStat stat, double amount, double duration)
    {
        if (target.Underlords.EffectUntil.GetValueOrDefault(key) > Time) return;
        target.Underlords.EffectUntil[key] = Time + duration;
        Buff(target, stat, -amount, duration);
    }

    private void UnderlordsWeaken(CombatUnit target, double fraction, double duration)
    {
        target.Weakness = Math.Max(target.WeakUntil > Time ? target.Weakness : 0, fraction);
        target.WeakUntil = Time + duration;
    }

    private void UnderlordsMoveNear(CombatUnit unit, CombatUnit center, double duration = 0.2)
    {
        if (FreeCellNear(center, unit.Cell) is { } cell && UnitAt(cell) is null) MoveFromHook(unit, cell, duration, leap: true);
    }

    private bool CastUnderlordsAbility(CombatUnit u, CombatUnit target, double value)
    {
        int stage = u.Stage;
        double V(double a, double b, double c) => stage == 0 ? a : stage == 1 ? b : c;
        CombatUnit[] Allies() => units.Where(a => a.Alive && a.Team == u.Team).ToArray();
        void Slow(CombatUnit t, string key, double amount, double duration) => UnderlordsTimedDebuff(t, key, CombatStat.AttackSpeed, amount, duration);
        void Cut(CombatUnit t, string key, double amount, double duration) => UnderlordsTimedDebuff(t, key, CombatStat.Armor, amount, duration);
        switch (u.Line.Ability.Kind)
        {
            case "ul_passive": break;
            case "ul_shuriken": Emit("shot", u, target, big: true); AbilityDamage(u, target, value); if (target.Alive) StunUnit(target, 0.5); break;
            case "ul_shadowwave":
                var healed = Allies().OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).Take(3 + stage).ToList();
                if (!healed.Contains(u)) healed.Add(u);
                foreach (var a in healed) { Heal(a, value); foreach (var e in EnemiesNear(u, a.Cell, 1)) AbilityDamage(u, e, value * 0.6); }
                break;
            case "ul_empower":
                var empowered = Allies().Where(a => !a.Summoned && a.Underlords.EmpowerUntil <= Time).OrderByDescending(a => a.AttackDamage).ThenBy(a => a.Id).FirstOrDefault() ?? u;
                double cleave = V(0.3, 0.5, 0.8); empowered.Underlords.EmpowerUntil = Time + 20;
                Buff(empowered, CombatStat.Amplification, cleave * 0.5, 20); empowered.Equipment.Split += cleave;
                Schedule(20, () => empowered.Equipment.Split -= cleave, null);
                Emit("shield", empowered); Emit("text", empowered, text: "Empowered");
                if (stage == 2) Burst(u.Cell, 2, u.Team);
                if (stage == 2) foreach (var e in EnemiesNear(u, u.Cell, 2)) { Damage(u, e, 100, DamageKind.Magic); if (e.Alive) StunUnit(e, 1); }
                break;
            case "ul_purge": Emit("beam", u, target);
                AbilityDamage(u, target, value * 0.4); if (!target.Alive) break;
                Slow(target, "ulPurgeSlow", 0.5, 5); UnderlordsWeaken(target, 0.2, 5);
                double lost = 0;
                if (stage == 2) { lost = target.Health * 0.3; Damage(u, target, lost, DamageKind.True); }
                Schedule(5, () => { if (target.Alive) { AbilityDamage(u, target, value); if (lost != 0 && target.Alive) Heal(target, lost * 0.5); } }, null);
                break;
            case "ul_haze": Emit("beam", u, target);
                AbilityDamage(u, target, value); if (!target.Alive) break;
                Buff(target, CombatStat.Armor, -V(20, 35, 55), 10);
                if (stage == 2) { Damage(u, target, 100, DamageKind.Physical); if (target.Alive) StunUnit(target, 1.5); }
                break;
            case "ul_scatter": Burst(target.Cell, 1, u.Team);
                foreach (var e in EnemiesNear(u, target.Cell, 1)) {
                    AbilityDamage(u, e, value); if (!e.Alive) continue;
                    Slow(e, "ulScatter", V(0.1, 0.15, 0.2), V(1, 2, 4)); if (stage == 2) Cut(e, "ulScatterArmor", 10, 4);
                } break;
            case "ul_punch": Emit("hit", u, target, big: true);
                Damage(u, target, u.AttackDamage * V(3.5, 4, 4.5), DamageKind.Physical, critical: true);
                if (target.Alive) { StunUnit(target, V(1, 1, 1.5)); Slow(target, "ulPunch", V(0.25, 0.25, 0.4), V(2, 2, 3)); } break;
            case "ul_terror":
                foreach (var e in LineTargets(u, target)) { Emit("beam", u, e); AbilityDamage(u, e, value); if (e.Alive) Cut(e, "ulTerror", V(10, 15, 20), 6); } break;
            case "ul_chaosbolt": Emit("beam", u, target);
                AbilityDamage(u, target, value * (1 + random.Next())); if (target.Alive) StunUnit(target, V(1, 1.5, 2) + random.Next()); break;
            case "ul_grip": Burst(target.Cell, 1, u.Team);
                foreach (var e in EnemiesNear(u, target.Cell, 1)) {
                    AbilityDamage(u, e, value); if (!e.Alive) continue;
                    e.SilenceUntil = Math.Max(e.SilenceUntil, Time + 3); Emit("text", e, text: "Silenced"); if (stage == 2) StunUnit(e, 1.5);
                } break;
            case "ul_ghostship": Burst(target.Cell, stage == 2 ? 3 : 2, u.Team);
                foreach (var e in EnemiesNear(u, target.Cell, stage == 2 ? 3 : 2)) { AbilityDamage(u, e, value); if (e.Alive) StunUnit(e, V(1, 1.3, 1.7)); } break;
            case "ul_poof":
                if (RandomEnemyTarget(u) is not { } destination) break;
                UnderlordsMoveNear(u, destination); Burst(u.Cell, 1, u.Team);
                foreach (var e in EnemiesNear(u, u.Cell, 1)) AbilityDamage(u, e, value); break;
            case "ul_hook":
                if (TargetByDistance(u, true) is not { } hooked) break;
                Emit("beam", u, hooked);
                if (u.Cell.DistanceTo(hooked.Cell) > 1) UnderlordsMoveNear(hooked, u, 0.3);
                AbilityDamage(u, hooked, value);
                if (hooked.Alive) foreach (var a in Allies().Where(a => a.Cell.DistanceTo(u.Cell) <= 1)) a.Target = hooked;
                break;
            case "ul_scream": Burst(u.Cell, 2, u.Team); foreach (var e in EnemiesNear(u, u.Cell, 2)) AbilityDamage(u, e, value); break;
            case "ul_charge":
                if (TargetByDistance(u, true) is not { } far) break;
                var crossed = LineTargets(u, far); UnderlordsMoveNear(u, far, 0.3); Emit("hit", u, far, big: true);
                foreach (var e in crossed) { AbilityDamage(u, e, value); if (e.Alive) StunUnit(e, V(1, 1.3, 1.7)); }
                u.Target = far; break;
            case "ul_sleight": Burst(u.Cell, stage == 2 ? 3 : 2, u.Team); foreach (var e in EnemiesNear(u, u.Cell, stage == 2 ? 3 : 2)) { Emit("hit", u, e); Damage(u, e, u.AttackDamage + V(130, 190, 250), DamageKind.Physical); } break;
            case "ul_purify":
                var weakest = Allies().OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).FirstOrDefault() ?? u;
                Heal(weakest, value); Burst(weakest.Cell, stage == 2 ? 3 : 1, u.Team); foreach (var e in EnemiesNear(u, weakest.Cell, stage == 2 ? 3 : 1)) AbilityDamage(u, e, value); break;
            case "ul_dagger": Emit("shot", u, target, big: true);
                AbilityDamage(u, target, value); if (target.Alive) Slow(target, "ulDagger", 0.5, 4);
                Buff(u, CombatStat.AttackSpeed, 0.8, 4); u.Target = target; break;
            case "ul_metamorph":
                if (u.Underlords.Metamorphosed) break;
                u.Underlords.Metamorphosed = true; u.Range = 4; u.AttackDamage += u.BaseAttackDamage * V(0.6, 1, 1.4);
                Buff(u, CombatStat.AttackSpeed, V(0.5, 0.75, 1), double.PositiveInfinity);
                var best = Allies().Where(a => a != u && !a.Summoned).OrderByDescending(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).FirstOrDefault();
                if (best is not null && best.Health / best.MaxHealth > u.Health / u.MaxHealth) {
                    double mine = u.Health / u.MaxHealth, theirs = best.Health / best.MaxHealth;
                    u.Health = theirs * u.MaxHealth; best.Health = Math.Max(1, mine * best.MaxHealth); Emit("beam", u, best);
                }
                u.MaxMana = 0; Emit("text", u, text: "Metamorphosis"); break;
            case "ul_laguna": Emit("beam", u, target); AbilityDamage(u, target, value); if (target.Alive) ApplyBurn(u, target); break;
            case "ul_arrow": Emit("shot", u, target, big: true); AbilityDamage(u, target, value); if (target.Alive) StunUnit(target, 2 + stage); break;
            case "ul_shieldcrash": Burst(u.Cell, 2, u.Team);
                var crash = EnemiesNear(u, u.Cell, 2);
                foreach (var e in crash) { AbilityDamage(u, e, value); if (e.Alive) e.Target = u; }
                if (crash.Length > 0) Buff(u, CombatStat.DamageReduction, Math.Min(0.4, 0.08 * crash.Length), 10); break;
            case "ul_fadebolt":
                var hit = new HashSet<CombatUnit>(); CombatUnit? current = target;
                for (int k = 0; k < 5 && current is not null; k++) {
                    hit.Add(current); Emit("beam", u, current); AbilityDamage(u, current, value); if (current.Alive) UnderlordsWeaken(current, 0.2, 5);
                    var center = current.Cell;
                    current = EnemiesOf(u).Where(e => !hit.Contains(e) && e.Cell.DistanceTo(center) <= 3).OrderBy(e => e.Cell.DistanceTo(center)).FirstOrDefault();
                } break;
            case "ul_godstrength": u.AttackDamage += u.BaseAttackDamage * V(0.8, 1.2, 1.6); u.MaxMana = 0; Emit("text", u, text: "God's Strength"); break;
            case "ul_ravage": Burst(u.Cell, 1 + stage, u.Team);
                foreach (var e in EnemiesNear(u, u.Cell, 1 + stage)) { AbilityDamage(u, e, value); if (e.Alive) StunUnit(e, V(1.5, 1.75, 2.2)); } break;
            case "ul_dissimilate":
                var cluster = EnemiesOf(u).OrderByDescending(e => EnemiesNear(u, e.Cell, 1).Length).FirstOrDefault(); if (cluster is null) break;
                UnderlordsMoveNear(u, cluster, 0.3); Burst(cluster.Cell, 1, u.Team);
                foreach (var e in EnemiesNear(u, cluster.Cell, 1)) { AbilityDamage(u, e, value); if (e.Alive) Cut(e, "ulDissim", V(20, 30, 40), 5); } break;
            case "ul_breathefire":
                foreach (var e in LineTargets(u, target).Take(4)) { Emit("beam", u, e); AbilityDamage(u, e, value); if (e.Alive) UnderlordsWeaken(e, V(0.25, 0.35, 0.45), 4); } break;
            case "ul_chrono":
                double duration = V(1.4, 1.8, 2.2); Burst(target.Cell, 2, u.Team);
                foreach (var e in EnemiesNear(u, target.Cell, 2)) StunUnit(e, duration);
                Buff(u, CombatStat.AttackSpeed, 0.75, duration); Emit("text", u, text: "Chronosphere"); break;
            case "ul_illuminate":
                var illuminated = LineTargets(u, target).ToList();
                foreach (var e in EnemiesNear(u, target.Cell, 1)) if (!illuminated.Contains(e)) illuminated.Add(e);
                foreach (var e in illuminated) { Emit("beam", u, e); AbilityDamage(u, e, value); } break;
            default: return CastUnderlordsTimedAbility(u, target, value);
        }
        return true;
    }
}
