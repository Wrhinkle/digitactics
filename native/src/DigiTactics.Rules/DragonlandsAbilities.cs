namespace DigiTactics.Rules;

internal sealed class DragonlandsUnitState
{
    internal bool Veil;
    internal double Shroud, XabMana, Jade, MirageZap, RagingUntil, Whisper, ShrunkUntil, Cannon, CannonFlat, Guard, Mode, Swift, Warrior;
    internal bool Statue, MirageExecute, Rage, Slayer, Lagoon, GuardUsed, Mage, MageAgain, ModeDone, Empowered;
    internal int WhisperStacks, CannonAttacks;
}

public sealed record DragonlandsCombatRewards(int Gold, int Axe, int Investor, int Lagoon = 0, int Doots = 0, int Wish = 0);

public sealed partial class CombatSimulation
{
    private readonly int[] dragonGold = new int[2], dragonAxe = new int[2], dragonInvestor = new int[2];
    private readonly int[] dragonLagoon = new int[2], dragonDoots = new int[2], dragonWish = new int[2];
    public DragonlandsCombatRewards DragonlandsRewards(int team) => new(dragonGold[team], dragonAxe[team], dragonInvestor[team], dragonLagoon[team], dragonDoots[team], dragonWish[team]);

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
        foreach (var side in new[] { setup.SideA, setup.SideB })
            if ((side.Augments?.Length ?? 0) > 0 || (side.Parts?.Any(p => (p.Augments?.Length ?? 0) > 0) ?? false))
                throw new NotSupportedException("Dragonlands augments have not been migrated yet.");
        BindDragonlandsTraits(sideA, sideB, setup);
        double Bank(CombatUnit u) => Math.Clamp((u.Team == 0 ? setup.SideA : setup.SideB).Dragonlands?.Bank ?? 0, 0, 15);
        foreach (var unit in units) BindDragonlandsItems(unit, Bank(unit));
        BindSummonedItems = unit => BindDragonlandsItems(unit, Bank(unit));
    }
}
