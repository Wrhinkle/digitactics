namespace DigiTactics.Rules;

internal sealed class GizmosUnitState
{
    internal double VisionsUntil, Ward, FrostUntil, Shroud;
    internal double? ArrestBreak;
    internal bool Backup, Noir, Melee, Cut, Spellblade, Smoke;
    internal double CenserUntil;
    internal int Antibody;
    internal double CyberUntil;
}

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> GizmosAbilities = ["snipe", "grab", "devour", "visions"];
    private bool CastGizmosAbility(CombatUnit unit, CombatUnit target, double value)
    {
        switch (unit.Line.Ability.Kind)
        {
            case "snipe":
                var far = units.Where(u => u.Alive && u.Team != unit.Team).OrderByDescending(u => u.Cell.DistanceTo(unit.Cell)).FirstOrDefault() ?? target;
                Emit("beam", unit, far); AbilityDamage(unit, far, value); break;
            case "grab":
                far = units.Where(u => u.Alive && u.Team != unit.Team).OrderByDescending(u => u.Cell.DistanceTo(unit.Cell)).FirstOrDefault() ?? target;
                Emit("beam", unit, far);
                var cell = FreeNeighbor(unit, far.Cell);
                if (cell is not null && unit.Cell.DistanceTo(far.Cell) > 1) MoveFromHook(far, cell.Value, 0.25);
                StunUnit(far, 1.5); AbilityDamage(unit, far, value);
                foreach (var ally in units.Where(a => a.Alive && a.Team == unit.Team && a != unit))
                    if (ally.Cell.DistanceTo(far.Cell) <= ally.Range + 1) ally.Target = far;
                break;
            case "devour":
                Emit("hit", unit, target, big: true); StunUnit(target, 2.5); AbilityDamage(unit, target, value);
                Buff(unit, CombatStat.DamageReduction, 0.3, 2.5); break;
            case "visions":
                var enemies = units.Where(u => u.Alive && u.Team != unit.Team).ToArray();
                var fresh = enemies.Where(e => !(e.Gizmos.VisionsUntil > Time)).ToArray();
                var victim = (fresh.Length > 0 ? fresh : enemies).OrderBy(e => unit.Cell.DistanceTo(e.Cell) * 10000 + e.Health).FirstOrDefault() ?? target;
                victim.Gizmos.VisionsUntil = Time + 6; Emit("beam", unit, victim);
                Buff(victim, CombatStat.MagicResist, -UnitValues.Round(Math.Max(0, victim.MagicResist) * 0.4), 6);
                for (int k = 1; k <= 6; k++) Schedule(k, () => { if (victim.Alive) AbilityDamage(unit, victim, value); }, null);
                break;
            default: return false;
        }
        return true;
    }

    private void BindGizmos(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            int active = counts.Count(p => rules.Traits.Any(t => t.Id == p.Key && TraitRules.Level(t, p.Value) > 0));
            if (side.Parts is { } partners) foreach (var part in partners) BindGizmosAugments(team, part.Augments ?? [], active, part.Gizmos, part.Owner, part.PartnerTop);
            else BindGizmosAugments(team, side.Augments ?? [], active, side.Gizmos, partnerTop: side.PartnerTop);
        }
        BindGizmosTraits(sideA, sideB, setup);
        foreach (var unit in units) BindGizmosItems(unit);
        BindSummonedItems = BindGizmosItems;
        var context = Hooks.Context(CombatHookSource.Mechanic, "gates");
        Hooks.Add(context, CombatHook.CombatStart, (_, _) => {
            foreach (var u in units) if (u.Line.Traits.Contains("COL")) u.Equipment.CrowdControlImmunity = 999;
        });
        Hooks.Add(context, CombatHook.DamageTaken, (_, e) => {
            var u = e.Unit!;
            if (u.Gizmos.ArrestBreak is { } threshold && u.Health <= threshold) { u.Gizmos.ArrestBreak = null; if (u.Stun > 0) u.Stun = 0; }
        });
        Hooks.Add(context, CombatHook.BeforeDamageTaken, (_, e) => {
            var target = e.Target!;
            if (!(target.Gizmos.Ward > 0) || e.Kind != DamageKind.Magic || !(e.Amount > 0)) return;
            e.Multiplier *= e.Amount <= target.Gizmos.Ward ? 0 : (e.Amount - target.Gizmos.Ward) / e.Amount;
            target.Gizmos.Ward = 0; Emit("text", target, text: "Blocked");
        });
    }
}
