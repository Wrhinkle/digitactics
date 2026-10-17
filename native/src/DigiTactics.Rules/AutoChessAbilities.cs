namespace DigiTactics.Rules;

internal sealed class AutoChessUnitState
{
    internal bool Transformed, Refreshed;
    internal int? Summoner, DruidCap;
    internal double StoneCooldown, KnightCooldown, Druid;
}

public sealed partial class CombatSimulation
{
    private readonly Dictionary<string, int>[] autoChessCounts = [[], []];
    private int AutoChessLevel(int team, string id) => rules.Traits.FirstOrDefault(t => t.Id == id) is { } trait ? TraitRules.Level(trait, autoChessCounts[team].GetValueOrDefault(id)) : 0;
    private bool CastAutoChessAbility(CombatUnit u, CombatUnit target, double value)
    {
        switch (u.Line.Ability.Kind) {
            case "summon":
                if (u.Line.Summon is not { } id || !lines.ContainsKey(id)) break;
                int[] counts = u.Line.SummonCount ?? [1, 2, 2];
                int count = u.Stage < counts.Length ? counts[u.Stage] : 1; if (count == 0) count = 1;
                double scale = value / Math.Max(1, UnitValues.AbilityValue(u.Line, u.Stage));
                int alive = units.Count(x => x.Alive && x.Summoned && x.AutoChess.Summoner == u.Id);
                for (int i = 0; i < count && alive < 4; i++) {
                    var cell = FreeCellNear(u, target.Cell); if (cell is null) break;
                    var summoned = Summon(u.Team, id, u.Stage, cell.Value, new(HealthMultiplier: scale, AttackMultiplier: scale)); if (summoned is null) break;
                    summoned.AutoChess.Summoner = u.Id; alive++;
                } break;
            case "transform":
                Heal(u, value); if (u.AutoChess.Transformed) break; u.AutoChess.Transformed = true;
                var tf = u.Line.Transformation ?? new(); u.AttackDamage += u.BaseAttackDamage * (tf.Ad ?? 0.3);
                if (tf.As != 0) Buff(u, CombatStat.AttackSpeed, tf.As, double.PositiveInfinity);
                if (tf.Armor != 0) { u.Armor += tf.Armor; u.MagicResist += tf.Armor; }
                if (tf.Range != 0) u.Range += tf.Range; Emit("text", u, text: "Transform"); Emit("shield", u); break;
            case "storm": foreach (var e in EnemiesOf(u)) { Emit("beam", u, e); AbilityDamage(u, e, value); } break;
            default: return false;
        }
        return true;
    }
    private void BindAutoChess(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++) {
            var side = team == 0 ? setup.SideA : setup.SideB; var entries = team == 0 ? sideA : sideB;
            autoChessCounts[team] = side.NoTraits ? [] : side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            void BindPart(string[] augs, string[] globals, int? owner, double partnerTop) {
                foreach (string id in augs) {
                    var context = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
                    if (!DuosCombatAugments.TryBind(Hooks, context, partnerTop)) throw new NotSupportedException($"Auto Chess augment {id} has not been migrated.");
                }
                foreach (string id in globals) {
                    var context = Hooks.Context(CombatHookSource.GlobalItem, id, team, owner: owner);
                    switch (id) {
                        case "ac_r_grace": Hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) u.Equipment.Regen += 0.012; }); break;
                        case "ac_r_reaper": Hooks.Add(context, CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) u.Equipment.Amp += 0.1; }); break;
                        case "ac_r_treasure": case "ac_r_forge": case "ac_r_vault": case "ac_r_charm": case "ac_r_recruiter": case "ac_r_eggnest": case "ac_r_card": case "ac_r_key": case "ac_r_discount": break;
                        default: throw new NotSupportedException($"Auto Chess relic {id} has not been migrated.");
                    }
                }
            }
            if (side.Parts is { } partners) foreach (var part in partners) BindPart(part.Augments ?? [], part.Globals ?? [], part.Owner, part.PartnerTop);
            else BindPart(side.Augments ?? [], side.Globals ?? [], null, side.PartnerTop);
        }
        BindAutoChessTraits();
        foreach (var u in units) BindAutoChessItems(u); BindSummonedItems = BindAutoChessItems;
        var mechanic = Hooks.Context(CombatHookSource.Mechanic, "creeps");
        Hooks.Add(mechanic, CombatHook.CombatStart, (_, _) => {
            double[] health = [1, 1.8, 3.24], ability = [1, 1.7, 3];
            foreach (var u in units) {
                if (u.Summoned || u.Stage < 1 || !u.Traits.Contains("DRUID")) continue;
                int effective = Math.Min(u.Stage, AutoChessLevel(u.Team, "DRUID")); if (effective >= u.Stage) continue;
                double scale = health[effective] / health[u.Stage]; u.MaxHealth = UnitValues.Round(u.MaxHealth * scale);
                u.AttackDamage *= scale; u.BaseAttackDamage *= scale; u.AbilityPower *= ability[effective] / ability[u.Stage]; u.AutoChess.DruidCap = effective;
            }
        });
    }
}
