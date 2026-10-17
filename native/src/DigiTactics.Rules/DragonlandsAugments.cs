namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindDragonlandsAugments(int team, string[] ids, HashSet<string> activeTraits, int roundStage,
        DragonlandsState? state, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(Hooks, context, partnerTop)) continue;
            IEnumerable<CombatUnit> Real(CombatHookContext c) => c.Mine.Where(u => !u.Summoned);
            IEnumerable<CombatUnit> Neighbors(CombatUnit u) => Adjacent(u).Where(a => a.Team == u.Team && !a.Summoned);
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatUnit> action) => On(CombatHook.CombatStart, (c, _) => { foreach (var u in Real(c)) action(u); });
            switch (id)
            {
                case "drg_feather": Start(u => { if (u.Line.Tier <= 2) u.AttackSpeed *= 1.25; }); break;
                case "drg_exiles": Start(u => { if (!Neighbors(u).Any()) u.Shield += u.MaxHealth * 0.3; }); break;
                case "drg_battlemage": Start(u => { if (u.Entry.Row <= 5) { u.Armor += 20; u.AbilityPower += 20; } }); break;
                case "drg_friends": Start(u => { if (Neighbors(u).Count() == 1) { u.AttackSpeed *= 1.15; u.Armor += 20; } }); break;
                case "drg_firstaid": On(CombatHook.CombatStart, (_, _) => HealingAmplification[team] += 0.3); break;
                case "drg_training":
                    Start(u => u.AttackDamage += u.BaseAttackDamage * (0.08 + 0.005 * Math.Min(40, state?.CtKills ?? 0)));
                    On(CombatHook.Kill, (_, _) => dragonTrainingKills[team]++); break;
                case "drg_ascension": case "drg_final":
                    bool done = false;
                    if (id == "drg_final") Start(u => u.Equipment.Amp += 0.15);
                    On(CombatHook.Tick, (_, _) => {
                        if (done || Time < 15) return; done = true;
                        // Legacy survivors(team) includes both partners and summoned units.
                        foreach (var u in units.Where(u => u.Alive && u.Team == team)) u.Equipment.Amp += id == "drg_final" ? 0.45 : 0.4;
                    }); break;
                case "drg_celestial": Start(u => u.Omnivamp += 0.18); break;
                case "drg_implants": Start(u => { if (u.Items.Count > 0) { u.MaxHealth += 200; u.AttackDamage += u.BaseAttackDamage * 0.15; } }); break;
                case "drg_shell": Start(u => { if (u.Items.Count > 0) { u.MaxHealth += 200; u.Armor += 30; } }); break;
                case "drg_uplink":
                    Start(u => { if (u.Items.Count > 0) { u.MaxHealth += 200; u.Dragonlands.Uplink = true; } });
                    On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Dragonlands.Uplink && c.Every(u, "uplink", 1)) u.Mana = Math.Min(u.MaxMana, u.Mana + 3); }); break;
                case "drg_imperialist":
                    bool enabled = false;
                    On(CombatHook.CombatStart, (c, _) => enabled = Real(c).Where(u => u.Line.Slots == 2).Select(u => u.Line.Id).Distinct().Count() == 1);
                    On(CombatHook.DamageDealt, (c, e) => {
                        var u = e.Unit!; var target = e.Target!;
                        if (enabled && u.Line.Slots == 2 && target.Alive && target.Health < target.MaxHealth * 0.18)
                            { c.Text(target, "Finished"); c.Damage(u, target, target.Health + target.Shield + 1, DamageKind.True); }
                    }); break;
                case "drg_axiom": On(CombatHook.Kill, (_, e) => { var u = e.Unit!; if (u.Alive) u.Mana = Math.Min(u.MaxMana, u.Mana + 30); }); break;
                case "drg_bigfriend":
                    Start(u => { if (Neighbors(u).Any(a => a.MaxHealth >= 1800)) u.Dragonlands.BigFriend = true; });
                    On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Unit!.Dragonlands.BigFriend) e.Reduction += 0.15; }); break;
                case "drg_double": On(CombatHook.CombatStart, (c, _) => {
                    var counts = Real(c).GroupBy(u => u.Line.Id).ToDictionary(g => g.Key, g => g.Count());
                    foreach (var u in Real(c)) if (counts[u.Line.Id] == 2) { u.AttackDamage += u.BaseAttackDamage * 0.25; u.AbilityPower += 25; u.Armor += 25; u.MagicResist += 25; }
                }); break;
                case "drg_electro": On(CombatHook.DamageTaken, (c, e) => {
                    var u = e.Unit!; if (!e.Critical || u.Summoned || u.Dragonlands.ElectroCooldown > Time) return;
                    u.Dragonlands.ElectroCooldown = Time + 1;
                    foreach (var enemy in EnemiesNear(u, u.Cell, 1)) c.Damage(u, enemy, 80 + 25 * (roundStage == 0 ? 3 : roundStage));
                }); break;
                case "drg_built": Start(u => { if (!u.Traits.Any(activeTraits.Contains)) { u.MaxHealth += 250 + 60 * (roundStage == 0 ? 3 : roundStage); u.AttackSpeed *= 1.4; } }); break;
                case "drg_wings": Start(u => { if (u.Line.Slots == 2) { u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.4); u.Equipment.Amp += 0.3; } }); break;
                // No combat hooks. Pick, planning and settlement migration is tracked separately.
                case "drg_hoard": case "drg_calcloss": case "drg_duplicates": case "drg_scraps": case "drg_grabbag":
                case "drg_archives": case "drg_treasure_hoard": case "drg_crest_gold": case "drg_crest_rage":
                case "drg_crest_tamer": case "drg_crest_mage": case "drg_jackpot": case "drg_relics": case "drg_crown": case "drg_pact": break;
                default: throw new NotSupportedException($"Dragonlands augment {id} has not been migrated.");
            }
        }
    }
}
