namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindGalaxies(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        foreach (var side in new[] { setup.SideA, setup.SideB })
            if ((side.Augments?.Length ?? 0) > 0 || (side.Parts?.Any(p => (p.Augments?.Length ?? 0) > 0) ?? false))
                throw new NotSupportedException("Galaxies augments have not been migrated yet.");
        BindGalaxiesTraits(sideA, sideB, setup);
        var context = Hooks.Context(CombatHookSource.Mechanic, "galaxy");
        Hooks.Add(context, CombatHook.CombatStart, (_, _) => {
            if (setup.Galaxy == "darkocean") {
                foreach (var u in units) u.Equipment.Amp += 0.15;
                for (int team = 0; team < 2; team++) HealingAmplification[team] -= 0.3;
            }
            if (setup.Galaxy == "datastream") foreach (var u in units) u.Mana += 25;
        });
        Hooks.Add(context, CombatHook.Attack, (_, e) => {
            var u = e.Unit!; var s = u.Galaxies; var target = e.Target!;
            if (s.Frenzy && s.FrenzyUntil > Time && target.Alive) Damage(u, target, target.MaxHealth * s.FrenzyPercent, DamageKind.Magic);
            if (s.ChosenUntil > Time && target.Alive) Damage(u, target, s.ChosenBonus, DamageKind.True);
        });
        Hooks.Add(context, CombatHook.Kill, (_, e) => {
            var u = e.Unit!;
            if (u.Line.Ability.Kind == "excited" && !u.Galaxies.Excited && u.Alive) { u.Galaxies.Excited = true; u.SpeedBuffs.Add(new(0.5, double.PositiveInfinity)); }
        });
        Hooks.Add(context, CombatHook.UnitTick, (_, e) => {
            var u = e.Unit!; var s = u.Galaxies;
            if (s.Frenzy && s.FrenzyUntil <= Time) { u.Range = s.OriginalRange; s.Frenzy = false; }
            if (s.DreadUntil > Time && u.Shield > 0 && Every(u, "galDread", 1))
                foreach (var enemy in EnemiesNear(u, u.Cell, 1)) Damage(u, enemy, s.DreadDps, DamageKind.Magic);
            if (s.ChosenUntil > Time && Time >= s.ChosenNext) { s.ChosenNext += 1; Heal(u, u.MaxHealth * 0.06, true); }
            if (s.PoisonSource is { } source && s.PoisonUntil > Time - 0.01 && Time >= s.PoisonNext)
            {
                // Legacy context.damage ignores a sixth burn argument; poison grants damage mana.
                s.PoisonNext += 1; Damage(source, u, s.PoisonDps, DamageKind.Magic);
                if (!u.Alive) return;
            }
            if (s.CycloneUntil > Time && Time >= s.CycloneNext) {
                s.CycloneNext += 0.5;
                GalaxyCasting(u, () => { foreach (var enemy in EnemiesNear(u, u.Cell, 1)) {
                    if (s.CycloneHit.Add(enemy.Id)) StunUnit(enemy, 1.5);
                    AbilityDamage(u, enemy, s.CycloneDamage);
                } });
            }
        });
        Hooks.Add(context, CombatHook.BeforeDamage, (_, e) => {
            if (e.Unit!.Galaxies.DisarmUntil > Time && e.Kind == DamageKind.Physical && !e.IsBurn) e.Multiplier = 0;
        });
        Hooks.Add(context, CombatHook.BeforeDamageTaken, (_, e) => {
            if (e.Target!.Galaxies.HexUntil > Time) e.Multiplier *= 1 + e.Target.Galaxies.HexAmplification;
        });
        Hooks.Add(context, CombatHook.Cast, (_, e) => {
            var u = e.Unit!; if (u.Galaxies.TaxMana != 0) { u.MaxMana = u.Galaxies.TaxMana; u.Galaxies.TaxMana = 0; }
        });
    }
}
