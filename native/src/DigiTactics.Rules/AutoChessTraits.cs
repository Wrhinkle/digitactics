namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindAutoChessTraits()
    {
        for (int team = 0; team < 2; team++) foreach (var (id, count) in autoChessCounts[team]) {
            var trait = rules.Traits.FirstOrDefault(t => t.Id == id); if (trait is null) continue;
            int level = TraitRules.Level(trait, count); if (level == 0) continue;
            double value = trait.Values[level - 1]; double P(string key) => trait.Parameters.TryGetValue(key, out var values) ? values[level - 1] : 0;
            var context = Hooks.Context(CombatHookSource.Trait, id, team);
            bool Member(CombatUnit u) => u.Traits.Contains(id);
            var members = units.Where(u => u.Team == team && Member(u)).ToArray();
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            void Each(Action<CombatUnit> action) => Start(_ => { foreach (var u in members) action(u); });
            void All(Action<CombatUnit> action) => Start(c => { foreach (var u in c.Mine) action(u); });
            switch (id) {
                case "BEAST": All(u => u.AttackDamage += u.BaseAttackDamage * value); break;
                case "DRAGON": Each(u => u.Mana = Math.Max(u.Mana, UnitValues.Round(u.MaxMana * value))); break;
                case "UNDEAD": Start(c => { foreach (var u in units) if (u.Team != c.Team) u.Armor -= value; }); break;
                case "DEMON": On(CombatHook.Attack, (c, e) => {
                    if (!Member(e.Unit!) || !e.Target!.Alive || AutoChessLevel(1 - c.Team!.Value, "DH") >= 1) return;
                    c.Damage(e.Unit!, e.Target, e.Amount * value, DamageKind.True);
                }); break;
                case "FAIRY": Each(u => u.Dodge += value); break;
                case "HUMAN": On(CombatHook.Attack, (c, e) => {
                    if (Member(e.Unit!) && e.Target!.Alive && e.Target.SilenceUntil <= Time && c.Random() < value) c.Silence(e.Target, 3);
                }); break;
                case "MACHINE": Start(c => {
                    var mine = c.Mine.ToArray(); var selected = level >= 2 ? mine : mine.Length > 0 ? new[] { mine[(int)(c.Random() * mine.Length)] } : [];
                    foreach (var u in selected) { u.Armor += value; u.Equipment.Regen += P("regen"); }
                }); break;
                case "DINO": Each(u => u.MaxHealth += value); break;
                case "INSECT": All(u => { double mult = 1 + P("all"); if (Member(u)) mult += value; u.AttackSpeed *= mult; }); break;
                case "ELEMENTAL":
                    Each(u => { u.Armor += P("armor"); u.MagicResist += P("armor"); });
                    On(CombatHook.DamageTaken, (c, e) => {
                        var u = e.Unit!; var source = e.Source; if (!(level >= 2 || Member(u))) return;
                        if (source is null || !source.Alive || source.Team == u.Team || source.Range > 1 || e.Kind != DamageKind.Physical || e.IsBurn || source.AutoChess.StoneCooldown > Time) return;
                        if (c.Random() < value) { source.AutoChess.StoneCooldown = Time + 2.5; c.Stun(source, 1.5); c.Text(source, "Stone"); }
                    }); break;
                case "AQUAN": All(u => u.MagicResist += value); break;
                case "CYBORG": Each(u => { u.Range++; u.AttackDamage += u.BaseAttackDamage * value; }); break;
                case "OLYMPOS": All(u => { u.Equipment.Shojin += value; u.Mana += P("mana"); }); break;
                case "WARRIOR": Each(u => u.Armor += value); break;
                case "MAGE": On(CombatHook.BeforeDamage, (_, e) => { if (e.Kind == DamageKind.Magic) e.Multiplier *= 1 + value; }); break;
                case "HUNTER": Each(u => u.AttackDamage += u.BaseAttackDamage * value); break;
                case "ASSASSIN": Each(u => { u.CriticalChance += value; u.CriticalDamage = Math.Max(u.CriticalDamage, P("cd")); }); break;
                case "KNIGHT": On(CombatHook.DamageTaken, (c, e) => {
                    var u = e.Unit!; if (!Member(u) || u.AutoChess.KnightCooldown > Time) return;
                    int successes = 0; for (int k = 0; k < value; k++) if (c.Random() < 0.3) successes++;
                    if (successes == 0) { u.AutoChess.KnightCooldown = Time + 0.5; return; }
                    u.AutoChess.KnightCooldown = Time + 4; c.Buff(u, CombatStat.DamageReduction, 0.18 * successes, 3);
                    c.Text(u, successes > 1 ? $"Divine Shield x{successes}" : "Divine Shield"); c.Effect("shield", u);
                }); break;
                case "DRUID":
                    Each(u => u.AutoChess.Druid = value);
                    On(CombatHook.UnitTick, (c, e) => {
                        var u = e.Unit!; if (u.AutoChess.Druid == 0 || !c.Every(u, "acDruid", 4)) return;
                        var best = units.Where(a => a.Alive && a.Team == u.Team).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault(); if (best is not null) c.Heal(best, best.MaxHealth * u.AutoChess.Druid);
                    }); break;
                case "MECH": Each(u => u.Equipment.Regen += value); break;
                case "SHAMAN": Start(c => {
                    var foes = units.Where(e => e.Team != c.Team && e.Alive).ToArray(); if (foes.Length > 0) { var target = foes[(int)(c.Random() * foes.Length)]; c.Stun(target, value); c.Text(target, "Hexed"); }
                }); break;
                case "WARLOCK": All(u => u.Omnivamp += value); break;
                case "DH":
                    Each(u => u.AttackSpeed *= 1 + P("as"));
                    On(CombatHook.BeforeDamage, (_, e) => { if (Member(e.Unit!) && e.Target!.Traits.Contains("DEMON")) e.Multiplier *= 1 + value; }); break;
                case "PRIEST": Start(c => HealingAmplification[c.Team!.Value] += P("heal")); break;
                case "WIZARD": Each(u => u.AbilityPower += value); break;
                default: throw new NotSupportedException($"Auto Chess trait {id} has not been migrated.");
            }
        }
    }
}
