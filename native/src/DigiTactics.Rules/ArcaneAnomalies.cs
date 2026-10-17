namespace DigiTactics.Rules;

internal sealed class ArcaneAnomalyState
{
    internal bool Bulwark, Berserk, Ascended, LinkNow, Finishing, LastChance, Decay;
    internal double ThornCooldown;
    internal CombatUnit? DragonTarget;
    internal Dictionary<int, int> Cuts { get; } = [];
    internal int Kills;
}

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> ArcaneAnomalies = [
        "x_power", "x_guard", "x_mind", "x_bulwark", "x_berserk", "x_giant", "x_velocity", "x_ascend",
        "dna_hunger", "dna_share", "dna_power", "dna_energy", "dna_mana", "dna_partners", "dna_familiar", "dna_shield",
        "dh_fireball", "dh_laser", "dh_slime", "dh_cooker", "dh_finisher", "dh_cuts", "dh_dragonsoul", "dh_lastchance", "dh_thorns",
        "u_crest", "u_dual"];

    private void BindArcaneAnomalies(CombatHookContext context)
    {
        Hooks.Add(context, CombatHook.CombatStart, (c, _) => {
            // Partner summons extend this list during iteration, just as in the reference engine.
            for (int i = 0; i < units.Count; i++) {
                var u = units[i]; if (!u.Alive) continue;
                if (u.Line.Swap) {
                    if (u.Entry.Row <= 5) u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.1);
                    else u.Range = Math.Max(u.Range, 4);
                }
                switch (u.Entry.ArcaneAnomaly) {
                    case "x_bulwark": u.Armor += 120; u.MagicResist += 120; u.Arcane.Anomaly.Bulwark = true; break;
                    case "x_berserk": case "dna_power": u.AttackDamage += u.BaseAttackDamage * 0.3; if (u.Entry.ArcaneAnomaly == "x_berserk") u.AbilityPower += 30; break;
                    case "x_giant": u.MaxHealth += 800; break;
                    case "x_velocity": u.AttackSpeed *= 1.1; break;
                    case "dna_energy": u.AbilityPower += 30; break;
                    case "dna_mana": u.Mana += 20; u.AttackSpeed *= 1.15; break;
                    case "dna_partners":
                        for (int k = 0; k < 2; k++) {
                            var cell = c.FreeCellNear(u); if (cell is null) break;
                            var w = c.Summon(u.Team, "arc_s_garurumon", 0, cell.Value, new(NoHooks: true)); if (w is null) break;
                            w.MaxHealth = w.Health = UnitValues.Round(u.MaxHealth * 0.25); w.AttackDamage = w.BaseAttackDamage = u.AttackDamage * 0.4;
                            w.Armor = u.Armor * 0.6; w.MagicResist = u.MagicResist * 0.6;
                        }
                        break;
                    case "dna_shield": u.Arcane.Anomaly.LinkNow = true; break;
                    case "dh_thorns": u.Armor += 60; u.MagicResist += 60; break;
                }
            }
        });
        Hooks.Add(context, CombatHook.CombatReady, (c, _) => {
            foreach (var u in units.ToArray()) {
                if (!u.Alive) continue;
                switch (u.Entry.ArcaneAnomaly) {
                    case "x_power": u.AttackDamage *= 1.55; break;
                    case "x_guard": u.Armor = (u.Armor + 15) * 1.6; u.MagicResist = (u.MagicResist + 15) * 1.6; break;
                    case "x_mind": u.AbilityPower = (u.AbilityPower + 10) * 1.4; break;
                    case "dna_hunger":
                        var best = units.Where(a => a != u && a.Alive && a.Team == u.Team && !a.Summoned).OrderBy(a => a.Cell.DistanceTo(u.Cell)).ThenBy(a => a.Id).FirstOrDefault();
                        if (best is not null) { u.MaxHealth += best.MaxHealth * 0.9; u.AttackDamage += best.AttackDamage * 0.9; c.Remove(best); best.Health = 0; Emit("death", best); }
                        break;
                    case "dna_share":
                        foreach (var a in c.Adjacent(u)) if (a.Team == u.Team && a.Alive) {
                            a.AttackDamage += u.AttackDamage * 0.12; a.AbilityPower += u.AbilityPower * 0.12; a.MaxHealth += u.MaxHealth * 0.12;
                            a.Armor += u.Armor * 0.12; a.MagicResist += u.MagicResist * 0.12;
                        }
                        break;
                }
            }
        });
        Hooks.Add(context, CombatHook.Cast, (c, e) => {
            var u = e.Unit!;
            if (u.Entry.ArcaneAnomaly == "x_velocity") u.AttackSpeed *= 1.15;
            else if (u.Entry.ArcaneAnomaly == "dh_fireball") for (int k = 0; k < (u.MaxMana >= 120 ? 2 : 1); k++) {
                var t = RandomEnemyTarget(u); if (t is null) break;
                foreach (var x in EnemiesNear(u, t.Cell, 1)) { c.Damage(u, x, x.MaxHealth * 0.06, DamageKind.True); c.Burn(u, x); }
            }
        });
        Hooks.Add(context, CombatHook.Attack, (c, e) => {
            var u = e.Unit!; var t = e.Target!; if (!t.Alive) return;
            if (u.Entry.ArcaneAnomaly == "dh_cuts") {
                int n = t.Arcane.Anomaly.Cuts.GetValueOrDefault(u.Id); t.Arcane.Anomaly.Cuts[u.Id] = n + 1;
                c.Damage(u, t, (25 + 10 * n) * (1 + 0.5 * u.Stage), DamageKind.True);
            } else if (u.Entry.ArcaneAnomaly == "dh_dragonsoul" && t != u.Arcane.Anomaly.DragonTarget) {
                u.Arcane.Anomaly.DragonTarget = t; c.Damage(u, t, t.MaxHealth * 0.16, DamageKind.True);
            }
        });
        Hooks.Add(context, CombatHook.Kill, (_, e) => { if (e.Unit!.Entry.ArcaneAnomaly is { } id && ArcaneAnomalies.Contains(id)) e.Unit.Arcane.Anomaly.Kills++; });
        Hooks.Add(context, CombatHook.DamageTaken, (c, e) => {
            var u = e.Unit!; var s = u.Arcane.Anomaly;
            switch (u.Entry.ArcaneAnomaly) {
                case "x_bulwark": if (s.Bulwark && u.Health < u.MaxHealth * 0.5) { s.Bulwark = false; u.Armor -= 80; u.MagicResist -= 80; } break;
                case "x_berserk": if (!s.Berserk && u.Health < u.MaxHealth * 0.5) { s.Berserk = true; u.AttackDamage += u.BaseAttackDamage * 0.6; u.AbilityPower += 60; } break;
                case "dh_thorns":
                    if (s.ThornCooldown > Time || !(e.Amount > 0)) return;
                    s.ThornCooldown = Time + 1; foreach (var x in EnemiesNear(u, u.Cell, 1)) c.Damage(u, x, e.Amount * 0.4); break;
            }
        });
        Hooks.Add(context, CombatHook.DamageDealt, (c, e) => {
            var u = e.Unit!; var t = e.Target!; var s = u.Arcane.Anomaly;
            if (u.Entry.ArcaneAnomaly != "dh_finisher" || s.Finishing || !t.Alive || t.Summoned || t.Health >= t.MaxHealth * 0.18) return;
            s.Finishing = true; c.Damage(u, t, (t.Health + t.Shield) * 3 + 10, DamageKind.True); s.Finishing = false;
        });
        Hooks.Add(context, CombatHook.Lethal, (_, e) => {
            var u = e.Unit!; var s = u.Arcane.Anomaly; if (u.Entry.ArcaneAnomaly != "dh_lastchance" || s.LastChance) return;
            s.LastChance = true; u.Health = u.MaxHealth; u.AttackSpeed *= 1.5; u.Omnivamp += 0.2; s.Decay = true;
        });
        Hooks.Add(context, CombatHook.Death, (c, e) => {
            var u = e.Unit!; if (u.Entry.ArcaneAnomaly != "dna_familiar") return;
            var s = c.Summon(u.Team, "arc_s_shellnumemon", 0, u.Cell, new(NoHooks: true)); if (s is null) return;
            s.MaxHealth = s.Health = UnitValues.Round(u.MaxHealth); s.Armor = u.Armor; s.MagicResist = u.MagicResist;
            s.AttackDamage = s.BaseAttackDamage = u.AttackDamage * 0.4;
        });
        Hooks.Add(context, CombatHook.AnyDeath, (_, e) => {
            var d = e.Unit!;
            for (int i = 0; i < units.Count; i++) {
                var u = units[i]; if (!u.Alive || u.Team != d.Team || u == d) continue;
                switch (u.Entry.ArcaneAnomaly) {
                    case "dna_power": u.AttackDamage += d.AttackDamage * 0.2; break;
                    case "dna_energy": u.AbilityPower += d.AbilityPower * 0.2; break;
                    case "dna_mana": u.Mana = Math.Min(u.MaxMana, u.Mana + d.Mana * 0.7); break;
                }
            }
        });
        Hooks.Add(context, CombatHook.UnitTick, ArcaneAnomalyTick);
    }

    private void ArcaneAnomalyTick(CombatHookContext c, CombatHookEvent e)
    {
        var u = e.Unit!; var s = u.Arcane.Anomaly;
        switch (u.Entry.ArcaneAnomaly) {
            case "x_ascend":
                if (s.Ascended || Time < 10) return;
                s.Ascended = true; double add = u.MaxHealth * 0.5; u.MaxHealth += add; c.Heal(u, add); u.AttackSpeed *= 1.6; break;
            case "dna_shield":
                if (!s.LinkNow && !c.Every(u, "aLink", 8)) return;
                s.LinkNow = false; double amount = u.MaxHealth * 0.12;
                var allies = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).Take(2).ToArray();
                c.Shield(u, amount); foreach (var a in allies) c.Shield(a, amount); break;
            case "dh_laser":
                if (!c.Every(u, "aLaser", 1)) return;
                var target = u.Target is { Alive: true } ? u.Target : TargetByDistance(u); if (target is null) return;
                foreach (var t in LineTargets(u, target)) if (u.Cell.DistanceTo(t.Cell) <= 3) { c.Damage(u, t, u.AbilityPower * 0.8); c.Burn(u, t); } break;
            case "dh_slime":
                if (!c.Every(u, "aSlime", 2)) return;
                double total = 0; foreach (var t in EnemiesNear(u, u.Cell, 1)) total += c.Damage(u, t, u.AbilityPower * 1.1);
                if (total > 0) c.Heal(u, total); break;
            case "dh_cooker":
                if (!c.Every(u, "aCook", 1)) return;
                foreach (var t in EnemiesNear(u, u.Cell, 1 + (int)Math.Floor(Time / 6))) c.Damage(u, t, u.MaxHealth * 0.035); break;
            case "dh_lastchance":
                if (!s.Decay) return;
                u.Health -= u.MaxHealth * 0.1 * e.DeltaTime;
                if (u.Health <= 0) { s.Decay = false; c.Kill(u); }
                break;
        }
    }
}
