using DigiTactics.Rules;

internal static class HookProbe
{
    internal static void Configure(CombatHookRegistry registry, bool duos, List<string> log)
    {
        var mechanic = registry.Context(CombatHookSource.Mechanic, "probe_mechanic");
        void Bind(CombatHookContext context)
        {
            foreach (var hook in Enum.GetValues<CombatHook>())
                registry.Add(context, hook, (c, e) =>
                {
                    string id = c.Id;
                    log.Add($"{id}:{c.Team?.ToString() ?? "-"}:on{hook}:{e.Unit?.Id.ToString() ?? "-"}");
                    if (hook == CombatHook.CombatStart)
                        foreach (var unit in c.Mine)
                        {
                            c.Buff(unit, CombatStat.Armor, 2, 0.4);
                            c.After(0.1, () => c.Buff(unit, CombatStat.AttackDamage, 3, 0.4));
                            c.After(0.2, () => c.Heal(unit, 5), unit);
                        }
                    if (hook == CombatHook.CombatReady && id == "probe_item") c.Shield(c.Holder!, 10);
                    if (hook == CombatHook.Shield && id == "probe_trait") c.Shield(e.Unit!, 1);
                    if (hook == CombatHook.BeforeDamage) { e.Multiplier *= 1.02; c.Buff(e.Target!, CombatStat.Armor, 0.5, 0.1); }
                    if (hook == CombatHook.BeforeDamageTaken) { e.Multiplier *= 0.99; e.Reduction += 0.01; c.Buff(e.Unit!, CombatStat.Armor, 0.25, 0.1); c.Buff(e.Unit!, CombatStat.DamageReduction, 0.001, 0.1); }
                    if (hook == CombatHook.Attack && id == "probe_power") c.Buff(e.Unit!, CombatStat.AttackSpeed, 0.1, 0.2);
                    if (hook == CombatHook.Cast && id == "probe_augment" && !c.State.ContainsKey("recast"))
                    { c.State["recast"] = true; c.CastAgain(e.Unit!, e.Target!); }
                    if (hook == CombatHook.DamageTaken && id == "probe_global") c.Heal(e.Unit!, 3, true);
                    if (hook == CombatHook.Lethal && id == "probe_item" && !c.State.ContainsKey("saved"))
                    { c.State["saved"] = true; c.Heal(e.Unit!, e.Unit!.MaxHealth * 2); }
                }, context.Id == "probe_early" ? -2 : 0);
        }
        for (int team = 0; team < 2; team++)
        {
            Bind(registry.Context(CombatHookSource.Trait, "probe_trait", team));
            for (int partner = 0; partner < (duos ? 2 : 1); partner++)
            {
                int? owner = duos ? partner : null;
                Bind(registry.Context(CombatHookSource.Augment, "probe_augment", team, owner: owner));
                Bind(registry.Context(CombatHookSource.Augment, "probe_early", team, owner: owner));
                Bind(registry.Context(CombatHookSource.Power, "probe_power", team, owner: owner));
                Bind(registry.Context(CombatHookSource.GlobalItem, "probe_global", team, owner: owner));
            }
        }
        foreach (var unit in mechanic.Combat.Units)
            Bind(registry.Context(CombatHookSource.HeldItem, "probe_item", holder: unit, count: 2));
        Bind(mechanic);
    }
}
