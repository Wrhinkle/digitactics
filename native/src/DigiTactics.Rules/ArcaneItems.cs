namespace DigiTactics.Rules;

internal sealed class ArcaneItemState
{
    internal bool Mantle, Bounty, Spellblade, Overkill, Ascended, TimeStop;
    internal double Doom;
    internal int BountyGold;
    internal ArcaneContrabandState? Contraband;
}

internal sealed class ArcaneContrabandState
{
    internal double TankAt, TankEvery, Execute, Rush, RipHeal, RipAd, RipVamp, HexArmor, HexDamage, Bloom, Toxins, Virus, Saber, HexCooldown;
    internal bool TankOn, Ripped, Busy;
    internal int HexHits, SaberHits;
}

public sealed partial class CombatSimulation
{
    private void BindArcaneItems(CombatUnit u, int roundStage)
    {
        var state = u.Arcane.Items;
        foreach (string id in u.Items.Distinct())
        {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            if (id.StartsWith("arc_k_", StringComparison.Ordinal)) {
                Start(c => ApplyArcaneContraband(u, id, c.Count)); continue;
            }
            switch (id)
            {
                case "arc_durandal": On(CombatHook.DamageDealt, (c, e) => {
                    if (e.IsBurn || !(e.Amount > 0)) return;
                    var best = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault();
                    if (best is not null) c.Heal(best, e.Amount * 0.1, true);
                }); break;
                case "arc_shadow_mantle": On(CombatHook.DamageTaken, (_, _) => {
                    if (state.Mantle || u.Health >= u.MaxHealth * 0.6) return;
                    state.Mantle = true; u.InvulnerableUntil = Time + 1;
                }); break;
                case "arc_mode_helm": Start(_ => {
                    if (u.Entry.Row <= 5) { u.Armor += 35; u.MagicResist += 35; }
                    else { u.AbilityPower += 15; u.Equipment.ManaRegen += 10; }
                }); break;
                case "arc_doom_seal": On(CombatHook.UnitTick, (c, _) => {
                    if (Time < state.Doom) return;
                    var target = u.Target is { Alive: true } ? u.Target : TargetByDistance(u); if (target is null) return;
                    state.Doom = Time + 13; c.Damage(u, target, target.MaxHealth * 0.25);
                }); break;
                case "arc_gatling_arm": Start(_ => u.Range = Math.Min(8, u.Range * 2)); break;
                case "arc_bounty_chip": On(CombatHook.DamageDealt, (c, e) => {
                    var t = e.Target!; if (state.Bounty || !t.Alive || t.Summoned || t.Health >= t.MaxHealth * 0.12) return;
                    state.Bounty = true; c.Damage(u, t, (t.Health + t.Shield) * 3 + 10, DamageKind.True); state.Bounty = false;
                    if (!t.Alive && c.Random() < 0.33) state.BountyGold = Math.Min(3, state.BountyGold + 1);
                }); break;
                case "arc_lone_plate": Start(c => {
                    if (c.Adjacent(u).Any(a => a.Team == u.Team)) return;
                    u.MaxHealth += 600; u.AttackDamage += u.BaseAttackDamage * 0.2; u.AbilityPower += 20;
                }); break;
                case "arc_spell_blade":
                    On(CombatHook.Cast, (_, _) => state.Spellblade = true);
                    On(CombatHook.Attack, (c, e) => {
                        if (!state.Spellblade || !e.Target!.Alive) return;
                        state.Spellblade = false; c.Damage(u, e.Target, 60 * Math.Max(2, roundStage == 0 ? 3 : roundStage));
                    }); break;
                case "arc_overkill_cannon": On(CombatHook.DamageDealt, (c, e) => {
                    if (!e.Killed || state.Overkill) return;
                    state.Overkill = true;
                    var near = EnemiesOf(u).OrderBy(x => x.Cell.DistanceTo(e.Target!.Cell)).ThenBy(x => x.Id).Take(3).ToArray();
                    foreach (var x in near) c.Damage(u, x, 100 + e.Amount * 0.2);
                    state.Overkill = false;
                }); break;
                case "arc_long_barrel":
                    Start(_ => u.Range = Math.Min(8, u.Range + 1));
                    On(CombatHook.Kill, (_, _) => u.Range = Math.Min(8, u.Range + 1)); break;
                case "arc_scope_visor": Start(_ => { u.Range = Math.Min(8, u.Range + 2); u.PerHexAmplification += 0.08; }); break;
                case "arc_ascension_talisman": On(CombatHook.UnitTick, (c, _) => {
                    if (state.Ascended || Time < 20) return;
                    state.Ascended = true; double add = u.MaxHealth * 0.8; u.MaxHealth += add; c.Heal(u, add); u.Equipment.Amp += 0.8;
                }); break;
                case "arc_time_stopper": On(CombatHook.DamageTaken, (_, _) => {
                    if (state.TimeStop || u.Health >= u.MaxHealth * 0.4) return;
                    state.TimeStop = true; u.InvulnerableUntil = Time + 2.5; u.CastLock = Math.Max(u.CastLock, 2.5);
                }); break;
                default: throw new NotSupportedException($"Arcane item {id} has not been migrated.");
            }
        }
    }

    private static void ApplyArcaneContraband(CombatUnit u, string id, int count)
    {
        if (!u.Traits.Contains("SYN")) return;
        var s = u.Arcane.Items.Contraband ??= new();
        bool upgraded = id.EndsWith('2'); string kind = upgraded ? id[..^1] : id;
        double V(double low, double high) => upgraded ? high : low;
        for (int k = 0; k < count; k++) switch (kind) {
            case "arc_k_tank": u.Armor += V(15, 25); u.MagicResist += V(15, 25); u.MaxHealth += V(200, 350); break;
            case "arc_k_blade": u.AttackDamage += u.BaseAttackDamage * V(0.25, 0.4); u.AttackSpeed *= 1 + V(0.1, 0.15); break;
            case "arc_k_ripper": u.AttackDamage += u.BaseAttackDamage * V(0.15, 0.25); u.CriticalChance += V(0.2, 0.35); break;
            case "arc_k_hex": u.Armor += V(25, 40); u.MagicResist += V(25, 40); u.MaxHealth += V(100, 175); break;
            case "arc_k_bloom": u.AbilityPower += V(20, 35); u.MaxHealth += V(150, 250); break;
            case "arc_k_toxins": u.AbilityPower += V(15, 25); u.Mana += V(10, 15); break;
            case "arc_k_virus": u.AbilityPower += V(25, 40); u.AttackSpeed *= 1 + V(0.15, 0.25); break;
            case "arc_k_saber": u.AttackDamage += u.BaseAttackDamage * V(0.15, 0.25); u.AbilityPower += V(15, 25); u.Omnivamp += V(0.2, 0.35); break;
            default: throw new NotSupportedException($"Unknown contraband {id}.");
        }
        // Each distinct item's effects overwrite that family's previous values, in equipment order.
        switch (kind) {
            case "arc_k_tank": s.TankAt = V(0.25, 0.45); s.TankEvery = V(0.1, 0.12); break;
            case "arc_k_blade": s.Execute = s.Rush = V(0.1, 0.15); break;
            case "arc_k_ripper": s.RipHeal = V(0.25, 0.4); s.RipAd = V(0.1, 0.2); s.RipVamp = V(0.1, 0.15); break;
            case "arc_k_hex": s.HexArmor = V(3, 5); s.HexDamage = V(0.5, 0.75); break;
            case "arc_k_bloom": s.Bloom = V(250, 420); break;
            case "arc_k_toxins": s.Toxins = V(100, 175); u.Equipment.Shojin += 5; break;
            case "arc_k_virus": s.Virus = V(0.15, 0.25); break;
            case "arc_k_saber": s.Saber = V(150, 250); break;
        }
    }

    private void BindArcaneContraband(CombatHookContext context)
    {
        Hooks.Add(context, CombatHook.DamageTaken, (c, e) => {
            var u = e.Unit!; var s = u.Arcane.Items.Contraband; if (s is null) return;
            if (s.TankAt != 0 && !s.TankOn && u.Health < u.MaxHealth * 0.5) { s.TankOn = true; c.Shield(u, u.MaxHealth * s.TankAt); }
            if (s.RipHeal != 0 && !s.Ripped && u.Health < u.MaxHealth * 0.4) {
                s.Ripped = true; u.InvulnerableUntil = Time + 0.5; c.Heal(u, u.MaxHealth * s.RipHeal); u.AttackDamage += u.BaseAttackDamage * s.RipAd; u.Omnivamp += s.RipVamp;
            }
            if (s.HexArmor != 0) {
                if (s.HexHits < 15) { s.HexHits++; u.Armor += s.HexArmor; u.MagicResist += s.HexArmor; }
                else if (s.HexCooldown <= Time && e.Source is { Alive: true }) {
                    s.HexCooldown = Time + 2; foreach (var x in EnemiesNear(u, u.Cell, 1)) c.Damage(u, x, (u.Armor + u.MagicResist) * s.HexDamage);
                }
            }
        });
        Hooks.Add(context, CombatHook.UnitTick, (c, e) => {
            var u = e.Unit!; var s = u.Arcane.Items.Contraband; if (s is null || !u.Alive) return;
            if (s.TankOn && c.Every(u, "cbTank", 8)) c.Shield(u, u.MaxHealth * s.TankEvery);
            if (s.Bloom != 0 && c.Every(u, "cbBloom", 8)) {
                var allies = units.Where(a => a.Alive && a.Team == u.Team && a != u).OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).Take(2).ToArray();
                c.Shield(u, s.Bloom); foreach (var a in allies) c.Shield(a, s.Bloom);
            }
        });
        Hooks.Add(context, CombatHook.DamageDealt, (c, e) => {
            var u = e.Unit!; var s = u.Arcane.Items.Contraband; if (s is null || !u.Alive || s.Busy || e.IsBurn) return;
            var t = e.Target!; s.Busy = true;
            if (s.Execute != 0 && t.Alive && !t.Summoned && t.Health < t.MaxHealth * s.Execute) {
                c.Damage(u, t, (t.Health + t.Shield) * 3 + 10, DamageKind.True);
                if (!t.Alive) { c.Buff(u, CombatStat.Amplification, s.Rush, 5); c.Buff(u, CombatStat.DamageReduction, s.Rush, 5); }
            }
            if (s.Virus != 0 && e.Kind == DamageKind.Magic && t.Alive) c.Damage(u, t, e.Amount * s.Virus, DamageKind.True);
            if (s.Saber != 0 && ++s.SaberHits >= 6) {
                s.SaberHits = 0;
                foreach (var x in EnemiesOf(u).OrderBy(x => x.Health).ThenBy(x => x.Id).Take(2).ToArray()) c.Damage(u, x, s.Saber, DamageKind.Physical);
            }
            s.Busy = false;
        });
        Hooks.Add(context, CombatHook.Cast, (c, e) => {
            var u = e.Unit!; var s = u.Arcane.Items.Contraband; if (s is null || s.Toxins == 0) return;
            var t = e.Target is { Alive: true } ? e.Target : TargetByDistance(u); if (t is null) return;
            s.Busy = true; for (int k = 0; k < 3 && t.Alive; k++) c.Damage(u, t, s.Toxins); s.Busy = false;
        });
    }
}
