namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindGizmosAugments(int team, string[] ids, int activeTraits, GizmosState? state, int? owner = null, double partnerTop = 0)
    {
        foreach (string id in ids)
        {
            var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
            if (DuosCombatAugments.TryBind(Hooks, context, partnerTop)) continue;
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatUnit> action) => On(CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
            bool Has(CombatUnit u, string trait) => u.Traits.Contains(trait);
            switch (id)
            {
                case "giz_a_feather1": case "giz_a_feather2": case "giz_a_feather3":
                    double feather = id.EndsWith('1') ? 0.25 : id.EndsWith('2') ? 0.4 : 0.6;
                    Start(u => { if (u.Line.Tier <= 2) u.AttackSpeed *= 1 + feather; }); break;
                case "giz_a_bless1": case "giz_a_bless2": case "giz_a_bless3":
                    double bless = id.EndsWith('1') ? 0.1 : id.EndsWith('2') ? 0.18 : 0.25;
                    Start(u => u.Omnivamp += bless); break;
                case "giz_a_implant1": case "giz_a_implant2": case "giz_a_implant3":
                    int tier = id[^1] - '0';
                    Start(u => { if (u.Items.Count > 0) { u.MaxHealth += (tier + 1) * 100; u.AttackDamage += u.BaseAttackDamage * (tier == 1 ? 0.1 : tier == 2 ? 0.15 : 0.2); } }); break;
                case "giz_a_thrill1": case "giz_a_thrill2": On(CombatHook.Kill, (c, e) => { if (e.Unit!.Alive) c.Heal(e.Unit, id.EndsWith('1') ? 300 : 500); }); break;
                case "giz_a_knife1": case "giz_a_knife2": Start(u => { if (u.Entry.Row <= 5) u.AttackDamage += u.BaseAttackDamage * (id.EndsWith('1') ? 0.15 : 0.25); }); break;
                case "giz_a_makeshift1": case "giz_a_makeshift2": Start(u => { if (u.Items.Count == 0) { u.Armor += id.EndsWith('1') ? 30 : 45; u.MagicResist += id.EndsWith('1') ? 30 : 45; } }); break;
                case "giz_a_united1": case "giz_a_united2": case "giz_a_united3":
                    double united = id.EndsWith('1') ? 0.02 : id.EndsWith('2') ? 0.03 : 0.05;
                    Start(u => { u.AttackDamage += u.BaseAttackDamage * united * activeTraits; u.AbilityPower += UnitValues.Round(united * 100 * activeTraits); }); break;
                case "giz_a_exiles1": On(CombatHook.CombatReady, (c, _) => { foreach (var u in c.Mine) if (!u.Summoned && !Adjacent(u).Any(a => a.Team == u.Team)) c.Shield(u, u.MaxHealth * 0.35); }); break;
                case "giz_a_ascend":
                    bool ascended = false;
                    On(CombatHook.Tick, (c, _) => { if (ascended || Time < 15) return; ascended = true; foreach (var u in c.Mine.Where(u => u.Alive)) u.Equipment.Amp += 0.4; }); break;
                case "giz_a_weak": Start(u => u.Equipment.Shred = Math.Max(u.Equipment.Shred, 0.2));
                    On(CombatHook.Attack, (_, e) => { if (e.Target!.Alive) e.Target.WoundUntil = Time + 5; }); break;
                case "giz_a_firstaid": On(CombatHook.CombatStart, (_, _) => HealingAmplification[team] += 0.3);
                    On(CombatHook.Shield, (_, e) => { if (e.Amount > 0) e.Unit!.Shield += e.Amount * 0.3; }); break;
                case "giz_a_scoped": Start(u => { if (u.Range >= 2) u.Range++; }); break;
                case "giz_a_underdog":
                    double next = 1;
                    On(CombatHook.Tick, (c, _) => {
                        if (Time < next) return; next++;
                        var mine = units.Where(u => u.Alive && u.Team == team).ToArray();
                        if (mine.Length >= units.Count(u => u.Alive && u.Team != team)) return;
                        foreach (var u in mine) c.Heal(u, Math.Min(150, u.MaxHealth * 0.04), true);
                    }); break;
                case "giz_a_runic": On(CombatHook.CombatReady, (c, _) => { foreach (var u in c.Mine) if (Has(u, "ARC")) c.Shield(u, u.AbilityPower * 3); }); break;
                case "giz_a_overload": On(CombatHook.Death, (c, e) => { var u = e.Unit!; if (Has(u, "XA") && !u.Summoned) foreach (var x in EnemiesNear(u, u.Cell, 2)) c.Damage(u, x, u.MaxHealth * 0.25); }); break;
                case "giz_a_sosmall": Start(u => { if (Has(u, "PET")) { u.Dodge += 0.4; u.AttackSpeed *= 1.1; } }); break;
                case "giz_a_cutthroat": On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (Has(u, "ASN") && !u.Gizmos.Cut) { u.Gizmos.Cut = true; e.Target!.Mana = 0; } }); break;
                case "giz_a_airdrop": Start(u => { if (u.Items.Count >= 2 && rules.CoreItems.Length > 0) ApplyTemporaryItemUpgrade(u, items[rules.CoreItems[(int)(random.Next() * rules.CoreItems.Length)]]); }); break;
                case "giz_a_shrug": Start(u => { if (Has(u, "BRU")) u.Equipment.Regen += 0.025; }); break;
                case "giz_a_spellblade": On(CombatHook.Cast, (_, e) => { if (Has(e.Unit!, "ARC")) e.Unit!.Gizmos.Spellblade = true; });
                    On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (u.Gizmos.Spellblade && e.Target!.Alive) { u.Gizmos.Spellblade = false; c.Damage(u, e.Target, u.AbilityPower * 2); } }); break;
                case "giz_a_stopwatch":
                    bool stopped = false;
                    Start(u => { if (Has(u, "CLOCK")) u.AttackSpeed *= 1.3; });
                    On(CombatHook.Tick, (c, _) => { if (stopped || Time < 5) return; stopped = true; foreach (var u in units.Where(u => u.Alive && u.Team != team)) c.Stun(u, 2); }); break;
                case "giz_a_smoke": On(CombatHook.DamageTaken, (_, e) => { var u = e.Unit!; if (!Has(u, "ASN") || u.Gizmos.Smoke || u.Health >= u.MaxHealth * 0.6) return; u.Gizmos.Smoke = true; u.InvulnerableUntil = Math.Max(u.InvulnerableUntil, Time + 1.5); u.Stun = 0; }); break;
                case "giz_a_reserves": Start(u => { if (Has(u, "PIR")) u.Equipment.Amp += Math.Min(0.45, 0.15 + 0.01 * (state?.GoldStart ?? 0)); }); break;
                case "giz_a_titanic": On(CombatHook.CombatReady, (c, _) => { foreach (var u in c.Mine) if (u.MaxHealth >= 1400) u.AttackDamage += u.MaxHealth * 0.03; }); break;
                case "giz_a_censer": On(CombatHook.Shield, (c, e) => { var u = e.Unit!; if (!u.Alive || u.Gizmos.CenserUntil > Time) return; u.Gizmos.CenserUntil = Time + 3; c.Buff(u, CombatStat.AttackSpeed, 0.3, 5); }); break;
                case "giz_a_woodland": On(CombatHook.CombatReady, (c, _) => {
                    var best = c.Mine.Where(u => u.Alive && !u.Summoned).OrderByDescending(u => u.MaxHealth * 100 - u.Id).FirstOrDefault();
                    if (best is null || c.FreeCellNear(best, best.Cell) is not { } cell) return;
                    var copy = c.Summon(team, best.Line.Id, best.Stage, cell, new(NoHooks: true));
                    if (copy is not null) copy.MaxHealth = copy.Health = 1500;
                }); break;
                // Trait inputs, count modifiers and non-combat effects are handled outside this binder.
                case "giz_a_injection": case "giz_a_booster": case "giz_a_burst": case "giz_a_spare": case "giz_a_parts":
                case "giz_a_calc": case "giz_a_hyper": case "giz_a_bank": case "giz_a_chip": case "giz_a_crestkit":
                case "giz_a_trade": case "giz_a_salvage": case "giz_a_junkyard": case "giz_a_jackpot": case "giz_a_grabbag":
                case "giz_a_recruit": case "giz_a_highend": case "giz_a_ticket": case "giz_a_windfall": case "giz_a_forge": case "giz_a_band": break;
                default: throw new NotSupportedException($"Gizmos augment {id} has not been migrated.");
            }
        }
    }
}
