namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindReckoningTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        for (int team = 0; team < 2; team++)
        {
            var side = team == 0 ? setup.SideA : setup.SideB;
            if (side.NoTraits) continue;
            var entries = team == 0 ? sideA : sideB;
            var counts = side.Counts ?? (side.Parts is { } parts ? TraitRules.TeamCount(rules, entries, parts) : TraitRules.Count(rules, entries, side.Augments));
            foreach (var (id, count) in counts)
            {
                var trait = rules.Traits.FirstOrDefault(t => t.Id == id);
                if (trait is null) continue;
                int level = TraitRules.Level(trait, count); if (level == 0) continue;
                double value = trait.Values[level - 1]; double P(string key) => trait.Parameters[key][level - 1];
                var context = Hooks.Context(CombatHookSource.Trait, id, team);
                var members = units.Where(u => u.Team == team && u.Traits.Contains(id)).ToArray();
                bool Member(CombatUnit u) => u.Traits.Contains(id);
                void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => Hooks.Add(context, hook, action, order);
                void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
                void Each(Action<CombatUnit> action) => Start(_ => { foreach (var u in members) action(u); });
                HexCell? Grave(CombatUnit u) => UnitAt(u.Cell) is null ? u.Cell : FreeNeighbor(u, u.Cell);
                switch (id)
                {
                    case "DSP": Start(_ => {
                        int n = members.Sum(u => u.Items.Count(item => items.TryGetValue(item, out var definition) && definition.Category == "shadow"));
                        double factor = value * (1 + 0.2 * Math.Min(4, n));
                        foreach (var u in members) { u.AttackDamage += u.BaseAttackDamage * factor; u.AbilityPower += UnitValues.Round(factor * 100); }
                    }); break;
                    case "TRK": Each(u => { u.AttackSpeed *= 1 + value; u.Reckoning.Trickster = value; });
                        On(CombatHook.Death, (c, e) => {
                            var u = e.Unit!; if (u.Reckoning.Trickster == 0 || u.Summoned || Grave(u) is not { } cell) return;
                            var copy = c.Summon(u.Team, u.Line.Id, Math.Max(0, u.Stage - 1), cell);
                            if (copy is null) return;
                            copy.MaxHealth = UnitValues.Round(copy.MaxHealth * 0.55); copy.Health = copy.MaxHealth;
                            copy.AttackSpeed *= 1 + u.Reckoning.Trickster; copy.Leap = false; c.Text(copy, "Copy");
                        }); break;
                    case "SER": Each(u => { u.Armor += value; u.MagicResist += value; u.AbilityPower += value; });
                        On(CombatHook.Death, (_, e) => {
                            if (!Member(e.Unit!) || e.Unit!.Summoned) return;
                            var left = members.Where(u => u.Alive).ToArray(); if (left.Length == 0) return;
                            double share = value / left.Length;
                            foreach (var u in left) { u.Armor += share; u.MagicResist += share; u.AbilityPower += share; }
                        }); break;
                    case "NGT": Each(u => { u.Reckoning.Night = value; u.Reckoning.NightDamage = P("dmg"); });
                        On(CombatHook.DamageTaken, (c, e) => {
                            var u = e.Unit!; var s = u.Reckoning; if (s.Night == 0 || s.NightUsed || u.Health >= u.MaxHealth * 0.5) return;
                            s.NightUsed = true; double amount = u.MaxHealth * s.Night; c.Shield(u, amount); u.Equipment.Amp += s.NightDamage; double at = u.Shield;
                            c.After(8, () => u.Shield = Math.Max(0, u.Shield - Math.Max(0, amount - (at - u.Shield))), u); c.Text(u, "Nightfall");
                        }); break;
                    case "DAY": Each(u => u.Reckoning.Day = value);
                        On(CombatHook.DamageTaken, (c, e) => {
                            var u = e.Unit!; var s = u.Reckoning; if (s.Day == 0 || s.DayUsed || u.Health >= u.MaxHealth * 0.5) return;
                            s.DayUsed = true; c.Heal(u, u.MaxHealth * s.Day * 0.5); c.After(1, () => c.Heal(u, u.MaxHealth * s.Day * 0.5), u);
                            foreach (var m in members.Where(m => m.Alive)) m.Equipment.Amp += 0.1; c.Text(u, "Daybreak");
                        }); break;
                    case "CHI":
                        int deaths = 0; bool risen = false;
                        On(CombatHook.Death, (c, e) => {
                            var u = e.Unit!; if (u.Summoned || risen || ++deaths < 3) return; risen = true;
                            var inherited = new List<string>();
                            foreach (var m in members.OrderBy(m => m.Cell.DistanceTo(u.Cell)).ThenBy(m => m.Id).Take(3)) {
                                var own = m.Items.Where(item => items.TryGetValue(item, out var def) && def.Emblem is null).ToArray();
                                if (own.Length > 0) inherited.Add(own[(int)(c.Random() * own.Length)]);
                            }
                            double factor = 1 + 0.4 * members.Sum(m => m.Stage) / Math.Max(1.0, members.Length);
                            if (Grave(u) is not { } cell) return;
                            var summon = c.Summon(c.Team!.Value, "rec_kimeramon", 0, cell, new(Items: inherited.ToArray()));
                            if (summon is null) return;
                            summon.MaxHealth = UnitValues.Round(summon.MaxHealth + value * factor - UnitValues.Stats(summon.Line, 0).MaxHealth); summon.Health = summon.MaxHealth;
                            double ad = P("ad") * factor; summon.AttackDamage = summon.AttackDamage - summon.BaseAttackDamage + ad; summon.BaseAttackDamage = ad; c.Text(summon, "Kimeramon rises");
                        }); break;
                    case "WIT":
                        CombatUnit? leader = null;
                        Start(c => {
                            if (members.Length == 0) return;
                            double x = members.Average(m => m.Cell.Column), y = members.Average(m => m.Cell.Row), best = double.PositiveInfinity;
                            foreach (var u in c.Mine) { double d = (u.Cell.Column - x) * (u.Cell.Column - x) + (u.Cell.Row - y) * (u.Cell.Row - y); if (d < best - 1e-9) { best = d; leader = u; } }
                            if (leader is not null) leader.AbilityPower += value;
                        });
                        On(CombatHook.Cast, (_, e) => { if (leader is { Alive: true } && Member(e.Unit!)) leader.Mana = Math.Min(leader.MaxMana, leader.Mana + e.Unit!.MaxMana * 0.3); }); break;
                    case "DRK": Each(u => u.MaxHealth += value); break;
                    case "HNT": Each(u => u.AbilityPower += value); bool slain = false;
                        On(CombatHook.Kill, (c, e) => { if (slain || !Member(e.Unit!) || e.Target!.MaxHealth < 1500) return; slain = true; foreach (var u in c.Mine.Where(u => u.Alive)) u.AbilityPower += P("team"); c.Text(e.Unit!, "Dragon slain"); }); break;
                    case "IRN": Start(c => { foreach (var u in c.Mine) u.Armor += value; }); break;
                    case "PHA": Each(u => u.Reckoning.Phantom = value);
                        On(CombatHook.Lethal, (_, e) => {
                            var u = e.Unit!; var s = u.Reckoning; if (s.Phantom == 0 || s.PhantomUsed || u.Summoned || u.Health > 0) return;
                            s.PhantomUsed = s.PhantomRisen = true; u.Health = Math.Max(1, u.MaxHealth * s.Phantom); u.InvulnerableUntil = Time + 1; u.Stun = Math.Max(u.Stun, 1); u.BurnSource = null; u.BurnUntil = 0; u.BurnAccumulated = 0; Emit("text", u, text: "Rises"); Emit("shield", u);
                        });
                        On(CombatHook.BeforeDamage, (_, e) => { if (e.Unit!.Reckoning.PhantomRisen) e.Multiplier *= 1.2; });
                        On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Target!.Reckoning.PhantomRisen) e.Multiplier *= 1.3; }); break;
                    case "SEN":
                        CombatUnit? holder = null; AttackSpeedBuff? speed = null; int jumps = 0; double given = 0, shieldAt = 0, until = 0, minimum = 0;
                        void Give(CombatHookContext c, CombatUnit u) {
                            double amount = value * (1 + 0.5 * u.Stage); c.Shield(u, amount);
                            if (holder is not null && speed is not null) holder.SpeedBuffs.RemoveAll(b => ReferenceEquals(b, speed));
                            jumps = Math.Min(5, jumps + 1); speed = new(P("as") * jumps, double.PositiveInfinity); u.SpeedBuffs.Add(speed);
                            holder = u; given = amount; shieldAt = u.Shield; until = Time + P("dur"); minimum = Time + P("dur") * 0.75; c.Text(u, "Sentinel");
                        }
                        On(CombatHook.CombatReady, (c, _) => { var u = members.Where(u => u.Alive).OrderByDescending(u => u.Items.Count).ThenByDescending(u => u.AttackSpeed).ThenBy(u => u.Id).FirstOrDefault(); if (u is not null) Give(c, u); }, 3);
                        On(CombatHook.Tick, (c, _) => {
                            if (holder is null || Time < minimum) return;
                            bool broken = holder.Shield <= Math.Max(0, shieldAt - given) + 1;
                            if (holder.Alive && !broken && Time < until) return;
                            if (holder.Alive) holder.Shield = Math.Max(0, holder.Shield - Math.Max(0, holder.Shield - (shieldAt - given)));
                            var next = c.Mine.Where(u => u.Alive).OrderBy(u => u.Health / u.MaxHealth).FirstOrDefault();
                            if (next is not null) Give(c, next); else holder = null;
                        }); break;
                    case "RIV": On(CombatHook.BeforeDamage, (_, e) => { if (count == 1 && Member(e.Unit!) && e.Target!.Traits.Any(t => (e.Unit!.Line.Rivals ?? []).Contains(t))) e.Multiplier *= 1 + value; }); break;
                    case "ETR": Start(c => { foreach (var u in members) if (c.FreeCellNear(u) is { } cell) { var wolf = c.Summon(u.Team, "rec_wolf", u.Stage, cell, new(HealthMultiplier: 0.5 * value, AttackMultiplier: 0.5 * value)); if (wolf is not null) c.Text(wolf, "Garurumon"); } }); break;
                    case "INA": Start(c => { foreach (var u in members) foreach (var a in c.Mine) if (a.Cell.DistanceTo(u.Cell) <= 1) c.Buff(a, CombatStat.DamageReduction, value, 8); }); break;
                    case "ASN": Each(u => { u.CriticalChance += value; u.CriticalDamage += P("cd"); u.Equipment.AbilityCrit = true; }); break;
                    case "BRW": Each(u => u.MaxHealth += value); break;
                    case "CAV": Each(u => u.Reckoning.Cavalier = value);
                        On(CombatHook.BeforeDamageTaken, (_, e) => { var s = e.Target!.Reckoning; if (s.Cavalier != 0) e.Reduction += s.Cavalier * (Time < 4 || Time < s.CavalierUntil ? 2 : 1); });
                        On(CombatHook.Cast, (_, e) => { if (e.Unit!.Reckoning.Cavalier != 0) e.Unit.Reckoning.CavalierUntil = Time + 3; }); break;
                    case "INV": On(CombatHook.Attack, (_, e) => { var u = e.Unit!; if (u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + value); }); break;
                    case "KNT": On(CombatHook.BeforeDamageTaken, (_, e) => { if (!e.IsBurn && e.Amount > 0) e.Multiplier *= Math.Max(0, (e.Amount - value) / e.Amount); }); break;
                    case "LEG": Each(u => { u.AttackSpeed *= 1 + value; u.Reckoning.Legion = true; });
                        On(CombatHook.Cast, (_, e) => { if (e.Unit!.Reckoning.Legion) e.Unit.Reckoning.LegionHeal = true; });
                        On(CombatHook.DamageDealt, (c, e) => { var u = e.Unit!; if (!u.Reckoning.LegionHeal || e.Kind != DamageKind.Physical || !u.Alive) return; u.Reckoning.LegionHeal = false; c.Heal(u, e.Amount * 0.5); }); break;
                    case "MYS": Start(c => { foreach (var u in c.Mine) u.MagicResist += value; }); break;
                    case "RNG": Each(u => { u.Reckoning.Ranger = value; u.Reckoning.RangerNext = 4; });
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Reckoning.Ranger == 0 || Time < u.Reckoning.RangerNext) return; c.Buff(u, CombatStat.AttackSpeed, u.Reckoning.Ranger, 4); u.Reckoning.RangerNext += 8; }); break;
                    case "REN": Each(u => u.Reckoning.Renewer = value);
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; double v = u.Reckoning.Renewer; if (v == 0) return; if (u.Health < u.MaxHealth) c.Heal(u, u.MaxHealth * v * e.DeltaTime, true); else if (u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + u.MaxMana * v * e.DeltaTime); }); break;
                    case "SKR": Each(u => u.Reckoning.Skirmisher = P("ad"));
                        On(CombatHook.CombatReady, (_, _) => { foreach (var u in members) u.Shield += u.MaxHealth * value; }, 2);
                        On(CombatHook.UnitTick, (c, e) => { var u = e.Unit!; if (u.Reckoning.Skirmisher != 0 && c.Every(u, "skr", 1)) u.AttackDamage += u.Reckoning.Skirmisher * (1 + 0.5 * u.Stage); }); break;
                    case "SPW": Each(u => u.AbilityPower += value);
                        On(CombatHook.AnyCast, (_, _) => { foreach (var m in members) if (m.Alive && m.Reckoning.Spellweaver < 10) { m.Reckoning.Spellweaver++; m.AbilityPower += P("per"); } }); break;
                    case "CAN": On(CombatHook.Attack, (c, e) => { var u = e.Unit!; if (!Member(u) || ++u.Reckoning.Cannoneer % 5 != 0 || !e.Target!.Alive) return; c.Burst(e.Target.Cell, 1, u.Team); foreach (var target in EnemiesNear(u, e.Target.Cell, 1)) c.Damage(u, target, e.Amount * value, DamageKind.Physical); }); break;
                    default: throw new NotSupportedException($"Reckoning trait {id} has not been migrated.");
                }
            }
        }
    }
}
