namespace DigiTactics.Rules;

internal sealed class ArcaneUnitState
{
    internal int Rotation, Progress, AutomatonHits, Wanted, ConquestKills, Coins;
    internal double Automaton, AutomatonDamage, Fire, FireLast, Dominator, Pit, PitHeal, Quick, QuickLow, QuickHigh, Vision, Watcher, WatcherHigh, NunuCooldown;
    internal int FireLevel, QuickLevel, VisionLevel;
    internal bool Chained, AutomatonBusy, ExperimentBusy, Artillery, PitBusy, PitHealed;
    internal HashSet<string>? Experiments;
    internal Dictionary<string, int>? Junk;
}

public sealed record ArcaneCombatRewards(int ConquestKills, int Coins);

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> ArcaneAbilities = ["arc_swap", "arc_rotate", "arc_gamble", "arc_progress"];
    public ArcaneCombatRewards ArcaneRewards(int team) => new(units.Where(u => u.Team == team).Sum(u => u.Arcane.ConquestKills), units.Where(u => u.Team == team).Sum(u => u.Arcane.Coins));
    private bool CastArcaneAbility(CombatUnit u, CombatUnit target, double value)
    {
        switch (u.Line.Ability.Kind)
        {
            case "arc_swap":
                if (u.Entry.Row <= 5) {
                    AddShield(u, value * 0.7);
                    foreach (var e in EnemiesNear(u, u.Cell, 1)) { StunUnit(e, 0.75); AbilityDamage(u, e, value * 0.45); }
                } else AbilityDamage(u, target, value * 1.4);
                break;
            case "arc_rotate":
                int rotation = u.Arcane.Rotation % 3; u.Arcane.Rotation = rotation + 1;
                if (rotation == 0) foreach (var e in LineTargets(u, target)) { AbilityDamage(u, e, value); if (e.Alive) StunUnit(e, 1.25); }
                else if (rotation == 1) for (int k = 0; k < 3; k++) {
                    var t = RandomEnemyTarget(u); if (t is null) break;
                    foreach (var e in EnemiesNear(u, t.Cell, 1)) AbilityDamage(u, e, value * 0.6);
                }
                else foreach (var e in units.Where(e => e.Alive && e.Team != u.Team).ToArray())
                    AbilityDamage(u, e, value * 1.2 * Math.Max(0.3, 1 - 0.12 * target.Cell.DistanceTo(e.Cell)));
                break;
            case "arc_gamble":
                double roll = random.Next();
                void Flame() { foreach (var e in EnemiesNear(u, target.Cell, 1)) { AbilityDamage(u, e, value * 0.8); ApplyBurn(u, e); } }
                void Punch() { if (!target.Alive) return; AbilityDamage(u, target, value * 1.5); if (target.Alive) StunUnit(target, 1); }
                void Chomp() { if (target.Alive) Heal(u, AbilityDamage(u, target, value * 1.1) * 0.5); }
                if (roll < 0.12) { Flame(); Punch(); Chomp(); }
                else if (roll < 0.41) Flame(); else if (roll < 0.7) Punch(); else Chomp();
                break;
            case "arc_progress":
                int shots = 4 + u.Stage + u.Arcane.Progress++;
                for (int k = 0; k < shots; k++) { var t = RandomEnemyTarget(u); if (t is null) break; AbilityDamage(u, t, value); }
                break;
            default: return false;
        }
        return true;
    }

    private void BindArcane(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        foreach (var side in new[] { setup.SideA, setup.SideB })
            if ((side.Augments?.Length ?? 0) > 0 || (side.Parts?.Any(p => (p.Augments?.Length ?? 0) > 0) ?? false))
                throw new NotSupportedException("Arcane augments have not been migrated yet.");
        foreach (var u in units) foreach (var id in u.Items)
            if (items.TryGetValue(id, out var item) && item.Hooks.Length > 0) throw new NotSupportedException($"Arcane item hooks {id} have not been migrated yet.");
        BindArcaneTraits(sideA, sideB, setup);
        var context = Hooks.Context(CombatHookSource.Mechanic, "anomaly");
        Hooks.Add(context, CombatHook.CombatStart, (_, _) => {
            foreach (var u in units.Where(u => u.Alive && u.Line.Swap)) {
                if (u.Entry.Row <= 5) u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.1);
                else u.Range = Math.Max(u.Range, 4);
            }
        });
    }
}
