namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private readonly int[] pirateKills = new int[2];
    public int PirateKillCount(int team) => pirateKills[team];

    private void BindGalaxiesTraits(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
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
                void Each(Action<CombatUnit> action) => On(CombatHook.CombatStart, (_, _) => { foreach (var u in members) action(u); });
                void All(Action<CombatUnit> action) => On(CombatHook.CombatStart, (c, _) => { foreach (var u in c.Mine) action(u); });
                switch (id)
                {
                    case "DD": On(CombatHook.Cast, (c, e) => {
                        if (!Member(e.Unit!)) return;
                        var others = c.Mine.Where(a => a.Alive && a != e.Unit && a.Traits.Contains(id) && !a.Galaxies.Piloting).ToArray();
                        if (others.Length == 0) return;
                        double share = value / others.Length;
                        foreach (var a in others) a.Mana = Math.Min(a.MaxMana, a.Mana + share);
                    }); break;
                    case "DA": On(CombatHook.Death, (c, e) => {
                        if (!Member(e.Unit!)) return;
                        foreach (var a in c.Mine.Where(a => a.Alive && a != e.Unit && a.Traits.Contains(id))) { a.AttackDamage += a.BaseAttackDamage * value / 100; a.AbilityPower += value; Emit("text", a, text: "Dark power"); }
                    }); break;
                    case "XR":
                        On(CombatHook.CombatReady, (c, _) => {
                            var selected = c.Mine.Where(u => u.Alive && !u.Summoned && Member(u)).OrderByDescending(u => u.Stage)
                                .ThenByDescending(u => u.Line.Tier).ThenByDescending(u => u.Items.Count).ThenBy(u => u.Id).Take(3).ToArray();
                            if (selected.Length < 3) return;
                            var host = selected[0]; var pilots = selected.Skip(1).ToList();
                            double hp = host.MaxHealth + pilots.Sum(p => p.MaxHealth), ad = host.AttackDamage + pilots.Sum(p => p.AttackDamage);
                            host.MaxHealth = UnitValues.Round(hp * value); host.AttackDamage = ad * value * 0.8; host.Armor += 20; host.MagicResist += 20;
                            host.Galaxies.Pilots = pilots; Emit("text", host, text: "DigiXros!");
                            foreach (var pilot in pilots) { pilot.Galaxies.Piloting = true; RemoveFromHook(pilot); }
                        }, 2);
                        On(CombatHook.Death, (_, e) => {
                            var host = e.Unit!; var pilots = host.Galaxies.Pilots; if (pilots is null) return; host.Galaxies.Pilots = null; Emit("text", host, text: "Eject");
                            foreach (var pilot in pilots) {
                                HexCell? cell = UnitAt(host.Cell) is null ? host.Cell : FreeNeighbor(host, host.Cell);
                                if (cell is null) continue;
                                pilot.Galaxies.Piloting = false; pilot.Alive = true; PlaceFromHook(pilot, cell.Value);
                                pilot.Health = pilot.MaxHealth * 0.6; pilot.Mana = 0; pilot.Target = null;
                                events.Add(new(Time, "leap", pilot.Id, Cell: host.Cell));
                            }
                        }); break;
                    case "PI": On(CombatHook.Kill, (_, e) => { if (Member(e.Unit!) && !e.Target!.Summoned) pirateKills[e.Unit!.Team]++; }); break;
                    case "CE": All(u => u.Omnivamp += value); break;
                    case "TK":
                        double next = value;
                        On(CombatHook.Tick, (c, _) => { if (Time < next) return; next += value; foreach (var u in c.Mine.Where(u => u.Alive)) u.SpeedBuffs.Add(new(0.12, double.PositiveInfinity)); }); break;
                    case "CY": Each(u => { double k = u.Items.Count > 0 ? 2 : 1; u.MaxHealth += value * k; u.AttackDamage += u.BaseAttackDamage * P("ad") * k; }); break;
                    case "RE": Each(u => { int n = Adjacent(u).Count(a => a != u && a.Team == u.Team && a.Traits.Contains(id)); if (n > 0) { u.Shield += value * n; u.Equipment.Amp += P("amp") * n; } }); break;
                    case "AB": On(CombatHook.BeforeDamage, (_, e) => {
                        if (e.IsBurn || !Member(e.Unit!)) return;
                        var target = e.Target!; double resist = e.Kind == DamageKind.Physical ? Math.Max(0, target.Armor) : e.Kind == DamageKind.Magic ? Math.Max(0, target.MagicResist) : 0;
                        if (resist != 0) e.Multiplier *= (100 + resist) / (100 + resist * (1 - value));
                    }); break;
                    case "VA": Each(u => u.CriticalChance += P("crit"));
                        On(CombatHook.BeforeDamage, (_, e) => { if (!e.Critical && !e.IsBurn && Member(e.Unit!) && e.Target!.Health < e.Target.MaxHealth * value) e.Multiplier *= e.Unit!.CriticalDamage; }); break;
                    case "AS": Each(u => u.MaxMana = Math.Max(20, u.MaxMana - value)); break;
                    case "OV":
                        Each(u => { u.Galaxies.Overload = value; u.Galaxies.OverloadCount = 0; });
                        void Overload(CombatHookContext c, CombatHookEvent e) {
                            var u = e.Unit!; var state = u.Galaxies;
                            if (e.IsBurn || state.Overload == 0 || !u.Alive) return;
                            if (++state.OverloadCount < 10) return; state.OverloadCount = 0;
                            if (u.Health < u.MaxHealth * 0.5) { c.Heal(u, state.Overload); c.Text(u, "Overload"); return; }
                            var target = TargetByDistance(u); if (target is not null) { c.Effect("beam", u, target); c.Damage(u, target, state.Overload); }
                        }
                        On(CombatHook.DamageDealt, Overload); On(CombatHook.DamageTaken, Overload); break;
                    case "ARK": Each(u => { u.AttackSpeed = 0.0001; u.Equipment.CrowdControlImmunity = 999; });
                        On(CombatHook.Tick, (_, e) => { foreach (var u in members.Where(u => u.Alive)) u.Mana = Math.Min(u.MaxMana, u.Mana + value * e.DeltaTime); }); break;
                    case "SV": On(CombatHook.BeforeDamage, (_, e) => {
                        if (e.Kind != DamageKind.Physical || e.IsBurn) return;
                        double armor = 100 + Math.Max(0, e.Target!.Armor);
                        e.Multiplier *= e.Unit!.Traits.Contains("DD") ? armor / 100 : armor / (100 + Math.Max(0, e.Target.MagicResist));
                    }); break;
                    case "BH": Each(u => { double n = Math.Min(9, side.Galaxies?.Bounty?.GetValueOrDefault(u.Line.Id) ?? 0); u.AbilityPower += value * n; u.AttackDamage += u.BaseAttackDamage * 0.03 * n; }); break;
                    case "BM": On(CombatHook.Attack, (_, e) => { if (!Member(e.Unit!) || !e.Target!.Alive || random.Next() >= value) return; for (int k = 0; k < 2 && e.Target.Alive; k++) { Emit("hit", e.Unit!, e.Target); GalaxyStrike(e.Unit!, e.Target); } }); break;
                    case "GU": On(CombatHook.Attack, (_, e) => {
                        var u = e.Unit!; if (!Member(u) || ++u.Galaxies.GunnerAttacks % 4 != 0) return;
                        for (int k = 0; k < value; k++) { var foes = EnemiesOf(u); if (foes.Length == 0) break; var target = foes[(int)(random.Next() * foes.Length)]; Emit("shot", u, target); GalaxyStrike(u, target); }
                    }); break;
                    case "BR": Each(u => u.MaxHealth += value); break;
                    case "BO": On(CombatHook.DamageDealt, (_, e) => {
                        var u = e.Unit!; var target = e.Target!;
                        if (!Member(u) || e.Kind == DamageKind.Physical || e.IsBurn || !target.Alive || !GalaxySpell(u)) return;
                        double tick = UnitValues.Round(Time * 30);
                        if (target.Galaxies.BombedBy == u.Id && target.Galaxies.BombedTick == tick) return;
                        target.Galaxies.BombedBy = u.Id; target.Galaxies.BombedTick = tick; StunUnit(target, value);
                    }); break;
                    case "IN": Each(u => { u.Leap = true; u.SpeedBuffs.Add(new(value, 6, true)); });
                        On(CombatHook.Kill, (_, e) => { var u = e.Unit!; if (!Member(u)) return; u.SpeedBuffs.RemoveAll(b => b.Infiltrator); u.SpeedBuffs.Add(new(value, Time + 6, true)); }); break;
                    case "HA": On(CombatHook.Attack, (_, e) => { if (Member(e.Unit!)) GalaxyTax(e.Target!, value); }); break;
                    case "MY": All(u => u.MagicResist += value); break;
                    case "PR": On(CombatHook.Cast, (_, e) => { var u = e.Unit!; if (!Member(u) || u.Galaxies.ProtectorUntil > Time) return; u.Galaxies.ProtectorUntil = Time + 4; GalaxyShield(u, u.MaxHealth * value, 4); }); break;
                    case "SN": Each(u => u.PerHexAmplification = value); break;
                    case "SO": All(u => u.AbilityPower += value); break;
                    case "VG": Each(u => u.Armor += value); break;
                    default: throw new NotSupportedException($"Galaxies trait {id} has not been migrated.");
                }
            }
        }
    }
}
