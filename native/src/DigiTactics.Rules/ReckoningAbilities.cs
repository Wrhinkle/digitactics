namespace DigiTactics.Rules;

internal sealed class ReckoningUnitState
{
    internal double Trickster, Night, NightDamage, Day, Phantom, Cavalier, CavalierUntil, Ranger, RangerNext, Renewer, Skirmisher;
    internal bool NightUsed, DayUsed, PhantomUsed, PhantomRisen, Legion, LegionHeal;
    internal int Spellweaver, Cannoneer;
    internal CombatUnit? Pet, Possessor, CurseSource;
    internal double PossessUntil, Decay, DecayNext, ShroudMax, CurseNext;
    internal double Lunar { get; set; }
    internal double CurseUntil { get; set; }
    internal double CurseRate { get; set; }
    internal bool SpellShield, TrapStun;
    internal bool Unstable { get; set; }
}

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> ReckoningAbilities = ["hook", "summon", "possess", "respite"];
    private readonly (CombatUnit? Source, double Until)[] respites = new (CombatUnit?, double)[2];

    private void ResurrectPossessed(CombatUnit caster, CombatUnit victim)
    {
        if (!caster.Alive) return;
        HexCell? cell = UnitAt(victim.Cell) is null ? victim.Cell : FreeNeighbor(victim, victim.Cell);
        if (cell is null) return;
        var risen = Summon(caster.Team, victim.Line.Id, victim.Stage, cell.Value, null);
        if (risen is null) return;
        risen.Leap = false; risen.Reckoning.Decay = caster.Stage == 0 ? 0.15 : caster.Stage == 1 ? 0.07 : 0;
        risen.Reckoning.DecayNext = Time + 1;
    }

    private bool CastReckoningAbility(CombatUnit unit, CombatUnit target, double value)
    {
        switch (unit.Line.Ability.Kind)
        {
            case "hook":
                var far = units.Where(u => u.Alive && u.Team != unit.Team).OrderByDescending(u => u.Cell.DistanceTo(unit.Cell)).FirstOrDefault() ?? target;
                Emit("beam", unit, far); AbilityDamage(unit, far, value);
                if (!far.Alive) break;
                if (unit.Cell.DistanceTo(far.Cell) > 1 && FreeNeighbor(unit, far.Cell) is { } cell) MoveFromHook(far, cell, 0.25);
                StunUnit(far, 1.5 + unit.Stage * 0.5); break;
            case "summon":
                double power = 40 + 15 * unit.Line.Tier + 30 * unit.Stage;
                if (unit.Reckoning.Pet is { Alive: true } pet) { Heal(pet, pet.MaxHealth * 0.3); pet.Mana = pet.MaxMana; pet.AbilityPower = Math.Max(pet.AbilityPower, power); break; }
                var spawn = FreeNeighbor(unit, target.Cell);
                if (spawn is null || unit.Line.Summon is null) break;
                pet = Summon(unit.Team, unit.Line.Summon, unit.Stage, spawn.Value, new(HealthMultiplier: 0.5 + 0.05 * unit.Line.Tier));
                if (pet is null) break;
                pet.AbilityPower = power; pet.Mana = pet.MaxMana; pet.Leap = false; unit.Reckoning.Pet = pet; break;
            case "possess":
                if (!target.Alive) break;
                Emit("beam", unit, target); StunUnit(target, 2); AbilityDamage(unit, target, value);
                if (!target.Alive) ResurrectPossessed(unit, target);
                else { target.Reckoning.Possessor = unit; target.Reckoning.PossessUntil = Time + 3; }
                break;
            case "respite":
                respites[unit.Team] = (unit, Time + (unit.Stage == 0 ? 2.5 : unit.Stage == 1 ? 3 : 4));
                foreach (var ally in units.Where(u => u.Alive && u.Team == unit.Team && u.Cell.DistanceTo(unit.Cell) <= 2)) { Heal(ally, value); Emit("shield", ally); }
                break;
            default: return false;
        }
        return true;
    }

    private void BindReckoning(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        foreach (var side in new[] { setup.SideA, setup.SideB })
            if ((side.Augments?.Length ?? 0) > 0 || (side.Parts?.Any(p => (p.Augments?.Length ?? 0) > 0) ?? false))
                throw new NotSupportedException("Reckoning augments have not been migrated yet.");
        BindReckoningTraits(sideA, sideB, setup);
        var context = Hooks.Context(CombatHookSource.Mechanic, "shadow");
        Hooks.Add(context, CombatHook.CombatStart, (_, _) => {
            if (setup.RoundType != "pve" || rules.PveMultipliers is not { Length: > 0 } multipliers) return;
            double mult = multipliers[Math.Min(setup.RoundStage, multipliers.Length - 1)]; if (mult == 0 || mult == 1) return;
            foreach (var u in units.Where(u => u.Team == 1)) { u.MaxHealth = UnitValues.Round(u.MaxHealth * mult); u.AttackDamage *= mult; u.BaseAttackDamage *= mult; }
        });
        Hooks.Add(context, CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.Reckoning.Lunar != 0) u.Mana = Math.Max(0, u.Mana - u.Reckoning.Lunar); });
        Hooks.Add(context, CombatHook.Cast, (_, e) => { var u = e.Unit!; if (u.Reckoning.ShroudMax != 0) { u.MaxMana = u.Reckoning.ShroudMax; u.Reckoning.ShroudMax = 0; } });
        Hooks.Add(context, CombatHook.BeforeDamageTaken, (c, e) => {
            var target = e.Target!; var state = target.Reckoning;
            if (!state.SpellShield || e.Kind != DamageKind.Magic || e.IsBurn || e.Source is null || e.Source.Team == target.Team) return;
            state.SpellShield = false; e.Multiplier = 0;
            if (state.TrapStun && e.Source.Alive) { state.TrapStun = false; c.Stun(e.Source, 3); }
        });
        Hooks.Add(context, CombatHook.UnitTick, (c, e) => {
            var u = e.Unit!; var state = u.Reckoning;
            if (state.CurseSource is { } source) {
                if (Time >= state.CurseUntil) state.CurseSource = null;
                else if (Time >= state.CurseNext) { state.CurseNext++; c.Damage(source, u, u.MaxHealth * (state.CurseRate != 0 ? state.CurseRate : 0.04), DamageKind.True); if (!u.Alive) return; }
            }
            if (state.Decay != 0 && Time >= state.DecayNext) { state.DecayNext++; c.Damage(u, u, u.MaxHealth * state.Decay, DamageKind.True); }
        });
        Hooks.Add(context, CombatHook.Lethal, (_, e) => {
            var u = e.Unit!; if (u.Health > 0) return;
            var respite = respites[u.Team];
            if (Time < respite.Until && respite.Source is { Alive: true } source && source.Cell.DistanceTo(u.Cell) <= 2) u.Health = 1;
        });
        Hooks.Add(context, CombatHook.Death, (c, e) => {
            var u = e.Unit!;
            if (u.Reckoning.Unstable) foreach (var target in EnemiesNear(u, u.Cell, 1)) c.Damage(u, target, 250, DamageKind.True);
            if (u.Reckoning.Possessor is { Alive: true } caster && Time <= u.Reckoning.PossessUntil && !u.Summoned) ResurrectPossessed(caster, u);
        });
    }
}
