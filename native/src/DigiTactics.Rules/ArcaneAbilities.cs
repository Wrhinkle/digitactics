namespace DigiTactics.Rules;

internal sealed class ArcaneUnitState
{
    internal ArcaneAnomalyState Anomaly { get; } = new();
    internal ArcaneItemState Items { get; } = new();
    internal int Rotation, Progress, AutomatonHits, Wanted, ConquestKills, Coins;
    internal double Automaton, AutomatonDamage, Fire, FireLast, Dominator, Pit, PitHeal, Quick, QuickLow, QuickHigh, Vision, Watcher, WatcherHigh, NunuCooldown;
    internal int FireLevel, QuickLevel, VisionLevel;
    internal bool Chained, AutomatonBusy, ExperimentBusy, Artillery, PitBusy, PitHealed;
    internal HashSet<string>? Experiments;
    internal Dictionary<string, int>? Junk;
}

public sealed record ArcaneCombatRewards(int ConquestKills, int Coins, int BountyGold = 0, int AnomalyKills = 0);

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> ArcaneAbilities = ["arc_swap", "arc_rotate", "arc_gamble", "arc_progress"];
    public ArcaneCombatRewards ArcaneRewards(int team) => new(units.Where(u => u.Team == team).Sum(u => u.Arcane.ConquestKills), units.Where(u => u.Team == team).Sum(u => u.Arcane.Coins), units.Where(u => u.Team == team).Sum(u => u.Arcane.Items.BountyGold), units.Where(u => u.Team == team).Sum(u => u.Arcane.Anomaly.Kills));
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
        for (int team = 0; team < 2; team++) {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            int active = counts.Count(p => rules.Traits.Any(t => t.Id == p.Key && TraitRules.Level(t, p.Value) > 0));
            if (side.Parts is { } partners) foreach (var part in partners) BindArcaneAugments(team, part.Augments ?? [], active, part.Owner, part.PartnerTop);
            else BindArcaneAugments(team, side.Augments ?? [], active, partnerTop: side.PartnerTop);
        }
        BindArcaneTraits(sideA, sideB, setup);
        foreach (var u in units) BindArcaneItems(u, setup.RoundStage);
        BindSummonedItems = u => BindArcaneItems(u, setup.RoundStage);
        var context = Hooks.Context(CombatHookSource.Mechanic, "anomaly");
        BindArcaneAnomalies(context);
        BindArcaneContraband(context);
    }
}
