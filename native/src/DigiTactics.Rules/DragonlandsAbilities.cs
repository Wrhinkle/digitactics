namespace DigiTactics.Rules;

internal sealed class DragonlandsUnitState
{
    internal bool Veil, Uplink, BigFriend;
    internal double ElectroCooldown;
    internal double Shroud, XabMana, Jade, MirageZap, RagingUntil, Whisper, ShrunkUntil, Cannon, CannonFlat, Guard, Mode, Swift, Warrior;
    internal bool Statue, MirageExecute, Rage, Slayer, Lagoon, GuardUsed, Mage, MageAgain, ModeDone, Empowered;
    internal int WhisperStacks, CannonAttacks;
}

public sealed record DragonlandsCombatRewards(int Gold, int Axe, int Investor, int Lagoon = 0, int Doots = 0, int Wish = 0, int CtKills = 0);

public sealed partial class CombatSimulation
{
    private readonly int[] dragonTrainingKills = new int[2];
    private readonly int[] dragonGold = new int[2], dragonAxe = new int[2], dragonInvestor = new int[2];
    private readonly int[] dragonLagoon = new int[2], dragonDoots = new int[2], dragonWish = new int[2];
    public DragonlandsCombatRewards DragonlandsRewards(int team) => new(dragonGold[team], dragonAxe[team], dragonInvestor[team], dragonLagoon[team], dragonDoots[team], dragonWish[team], dragonTrainingKills[team]);

    private bool CastDragonlandsAbility(CombatUnit unit, CombatUnit target, double value)
    {
        if (unit.Line.Ability.Kind != "copy") return false;
        switch ((int)(random.Next() * 5))
        {
            case 0: AbilityDamage(unit, target, value * 1.4); break;
            case 1: foreach (var enemy in EnemiesNear(unit, target.Cell, 1)) AbilityDamage(unit, enemy, value); break;
            case 2: StunUnit(target, 1.5); AbilityDamage(unit, target, value); break;
            case 3:
                for (int i = 0; i < 5; i++) {
                    var pool = units.Where(e => e.Alive && e.Team != unit.Team).ToArray();
                    if (pool.Length == 0) break;
                    AbilityDamage(unit, pool[(int)(random.Next() * pool.Length)], value * 0.4);
                }
                break;
            case 4:
                foreach (var ally in units.Where(u => u.Alive && u.Team == unit.Team && !u.Summoned).OrderBy(u => u.Health / u.MaxHealth).ThenBy(u => u.Id).Take(2).ToArray()) Heal(ally, value * 0.9);
                break;
        }
        return true;
    }

    private void BindDragonlands(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            var active = rules.Traits.Where(t => TraitRules.Level(t, counts.GetValueOrDefault(t.Id)) > 0).Select(t => t.Id).ToHashSet();
            if (side.Parts is { } partners) foreach (var part in partners)
                BindDragonlandsAugments(team, part.Augments ?? [], active, setup.RoundStage, part.Dragonlands, part.Owner, part.PartnerTop);
            else BindDragonlandsAugments(team, side.Augments ?? [], active, setup.RoundStage, side.Dragonlands, partnerTop: side.PartnerTop);
        }
        BindDragonlandsTraits(sideA, sideB, setup);
        double Bank(CombatUnit u) => Math.Clamp((u.Team == 0 ? setup.SideA : setup.SideB).Dragonlands?.Bank ?? 0, 0, 15);
        foreach (var unit in units) BindDragonlandsItems(unit, Bank(unit));
        BindSummonedItems = unit => BindDragonlandsItems(unit, Bank(unit));
    }
}
