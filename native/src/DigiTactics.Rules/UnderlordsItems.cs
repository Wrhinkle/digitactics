namespace DigiTactics.Rules;

internal sealed class UnderlordsItemState
{
    internal bool Reflect, Paladin, Pipe, Mekansm, Refresher, Horn, Shiva, Vladmir;
    internal double BlinkAt, EulsCooldown, BashCooldown, VyseCooldown, SatanicCooldown;
    internal int? Necromancer;
}

public sealed partial class CombatSimulation
{
    private void BindUnderlordsItems(CombatUnit u)
    {
        var s = u.Underlords.Items;
        foreach (string id in u.Items.Distinct()) {
            if (!items.TryGetValue(id, out var item) || item.Hooks.Length == 0) continue;
            var context = Hooks.Context(CombatHookSource.HeldItem, id, holder: u, count: u.Items.Count(x => x == id));
            void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action) => Hooks.Add(context, hook, action);
            void Start(Action<CombatHookContext> action) => On(CombatHook.CombatStart, (c, _) => action(c));
            CombatUnit[] RowMates() => units.Where(a => a.Alive && a.Team == u.Team && a.Cell.Row == u.Cell.Row && Math.Abs(a.Cell.Column - u.Cell.Column) <= 1).ToArray();
            switch (id) {
                case "ul_headdress": Start(c => { foreach (var a in RowMates()) a.Equipment.Regen += 20 * c.Count / Math.Max(1, a.MaxHealth); }); break;
                case "ul_hood": Start(c => { foreach (var a in RowMates()) a.MagicResist += 25 * c.Count; }); break;
                case "ul_vitality": Start(c => { foreach (var a in RowMates()) a.MaxHealth += 200 * c.Count; }); break;
                case "ul_arcane": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "ulArcane", 10) || u.Mana <= u.MaxMana / 2) return;
                    foreach (var a in UnderlordsAllies(u, 2)) if (a.MaxMana > 0) a.Mana = Math.Min(a.MaxMana, a.Mana + 20);
                }); break;
                case "ul_armlet": On(CombatHook.UnitTick, (c, e) => u.Health = Math.Max(1, u.Health - u.MaxHealth * 0.03 * c.Count * e.DeltaTime)); break;
                case "ul_blademail": On(CombatHook.DamageTaken, (c, e) => {
                    var source = e.Source; if (source is null || !source.Alive || source.Team == u.Team || e.IsBurn || s.Reflect) return;
                    source.Underlords.Items.Reflect = true; c.Damage(u, source, e.Amount * 0.2 * c.Count, DamageKind.True); source.Underlords.Items.Reflect = false;
                }); break;
                case "ul_blink": On(CombatHook.UnitTick, (_, _) => {
                    if (Time < 0.5 || !u.Alive || u.Stun > 0 || u.SilenceUntil > Time || Time < s.BlinkAt) return;
                    s.BlinkAt = Time + 15; var far = TargetByDistance(u, true);
                    if (far is null || FreeCellNear(far, u.Cell) is not { } cell || UnitAt(cell) is not null || !MoveFromHook(u, cell, 0.2)) return;
                    u.Target = far; if (u.MaxMana > 0) u.Mana = Math.Min(u.MaxMana, u.Mana + 45);
                }); break;
                case "ul_desolator": On(CombatHook.Attack, (_, e) => { if (e.Target!.Alive) UnderlordsTimedDebuff(e.Target, "ulDeso", CombatStat.Armor, 15, 5); }); break;
                case "ul_lance": Start(c => { if (u.Range > 1) u.Range += 3; else u.AttackDamage -= u.BaseAttackDamage * 0.25 * c.Count; }); break;
                case "ul_halberd": On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive && c.Every(u, "ulHalberd", 10)) UnderlordsTimedDebuff(e.Target, "ulDisarm", CombatStat.AttackSpeed, 0.6, 3); }); break;
                case "ul_paladin": Start(_ => { if (!s.Paladin) { s.Paladin = true; HealingAmplification[u.Team] += 0.2; } }); break;
                case "ul_quelling": On(CombatHook.BeforeDamage, (_, e) => { if (e.Target!.Summoned) e.Multiplier *= 2; }); break;
                case "ul_stonecloak": On(CombatHook.Kill, (c, _) => { u.MaxHealth += 100 * c.Count; c.Heal(u, 100 * c.Count); }); break;
                case "ul_stonepike": On(CombatHook.Kill, (c, _) => u.AttackDamage += u.BaseAttackDamage * 0.1 * c.Count); break;
                case "ul_vanguard": On(CombatHook.BeforeDamageTaken, (c, e) => {
                    if (e.Kind != DamageKind.Physical || e.IsBurn || e.Amount <= 0 || c.Random() >= 0.5) return;
                    e.Multiplier *= Math.Max(0, (e.Amount - 40 * c.Count) / e.Amount);
                }); break;
                case "ul_battlefury": Start(c => { if (u.Range > 1) { u.Equipment.Split = Math.Max(0, u.Equipment.Split - 0.5 * c.Count); u.AttackDamage -= u.BaseAttackDamage * 0.35 * c.Count; } }); break;
                case "ul_euls": On(CombatHook.AnyAttack, (c, e) => {
                    if (e.Target != u || !u.Alive || e.Unit!.Team == u.Team || !e.Unit.Alive || s.EulsCooldown > Time) return;
                    s.EulsCooldown = Time + 10; c.Stun(e.Unit, 2.5); e.Unit.InvulnerableUntil = Time + 2.5;
                }); break;
                case "ul_madness": Start(_ => { u.MaxMana = 0; u.Mana = 0; }); break;
                case "ul_necro": On(CombatHook.Cast, (_, _) => {
                    if (units.Count(b => b.Alive && b.Underlords.Items.Necromancer == u.Id) >= 2) return;
                    var b = UnderlordsSummon(u, "ul_s_bruiser", u.Stage, u); if (b is not null) b.Underlords.Items.Necromancer = u.Id;
                }); break;
                case "ul_octarine": Start(c => { if (u.MaxMana > 0) u.MaxMana = Math.Max(20, UnitValues.Round(u.MaxMana * Math.Pow(0.7, c.Count))); }); break;
                case "ul_pipe": On(CombatHook.UnitTick, (c, _) => {
                    if (Time < 3 || s.Pipe) return; s.Pipe = true; foreach (var a in UnderlordsAllies(u, 2)) c.Shield(a, 250);
                }); break;
                case "ul_silveredge": On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive && c.Every(u, "ulSilver", 5)) c.Weaken(e.Target, 0.25, 3); }); break;
                case "ul_basher": On(CombatHook.Attack, (c, e) => {
                    if (!e.Target!.Alive || s.BashCooldown > Time || c.Random() >= 0.25) return;
                    s.BashCooldown = Time + 2; c.Damage(u, e.Target, 100, DamageKind.Physical); if (e.Target.Alive) c.Stun(e.Target, 1.5);
                }); break;
                case "ul_shako": Start(_ => { if (u.MaxMana > 0) u.MaxMana = UnitValues.Round(u.MaxMana * 2); }); break;
                case "ul_dagon": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "ulDagon", 12)) return;
                    var pool = EnemiesOf(u).Where(x => x.Health <= x.MaxHealth * 0.5).ToArray();
                    if (pool.Length == 0) { u.Timers["ulDagon"] = Time + 1; return; }
                    c.Damage(u, pool[(int)(c.Random() * pool.Length)], 600 * c.Count);
                }); break;
                case "ul_diffusal": On(CombatHook.Attack, (c, e) => {
                    var target = e.Target!; if (!target.Alive) return;
                    if (target.MaxMana > 0) target.Mana = Math.Max(0, target.Mana - 15 * c.Count); c.Damage(u, target, 30 * c.Count, DamageKind.Physical);
                }); break;
                case "ul_skadi": On(CombatHook.Attack, (_, e) => { if (e.Target!.Alive) UnderlordsTimedDebuff(e.Target, "ulSkadi", CombatStat.AttackSpeed, 0.4, 2.5); }); break;
                case "ul_kaden":
                    On(CombatHook.BeforeDamage, (_, e) => { if (e.Kind == DamageKind.Physical && !e.IsBurn) e.Multiplier *= 0.5; });
                    On(CombatHook.Attack, (c, e) => { if (e.Target!.Alive) c.Damage(u, e.Target, e.Target.MaxHealth * 0.06 * c.Count, DamageKind.True); }); break;
                case "ul_maelstrom": On(CombatHook.Attack, (c, e) => {
                    if (c.Random() >= 0.25) return; var hit = new List<CombatUnit>(); CombatUnit? current = e.Target;
                    for (int k = 0; k < 4 && current is not null; k++) {
                        hit.Add(current); c.Damage(u, current, 110 * (current.Summoned ? 1.5 : 1)); var center = current.Cell;
                        current = EnemiesOf(u).Where(x => !hit.Contains(x) && x.Cell.DistanceTo(center) <= 3).OrderBy(x => x.Cell.DistanceTo(center)).FirstOrDefault();
                    }
                }); break;
                case "ul_mekansm": On(CombatHook.DamageTaken, (c, _) => {
                    if (u.Health >= u.MaxHealth * 0.5 || s.Mekansm) return; s.Mekansm = true;
                    foreach (var a in UnderlordsAllies(u, 3)) c.Heal(a, 500);
                }); break;
                case "ul_refresher": On(CombatHook.Cast, (_, _) => { if (u.MaxMana > 0 && !s.Refresher) { s.Refresher = true; u.Mana = u.MaxMana; } }); break;
                case "ul_vyse": On(CombatHook.DamageTaken, (c, e) => {
                    if (e.Source is not { Alive: true } source || source.Team == u.Team || s.VyseCooldown > Time) return;
                    s.VyseCooldown = Time + 15; c.Stun(source, 2.5);
                }); break;
                case "ul_bloodthorn":
                    On(CombatHook.UnitTick, (c, _) => {
                        if (!c.Every(u, "ulBloodthorn", 10)) return;
                        var target = EnemiesOf(u).Where(x => x.MaxMana > 0).OrderByDescending(x => x.Mana).ThenBy(x => x.Id).FirstOrDefault(); if (target is not null) c.Silence(target, 4);
                    });
                    On(CombatHook.BeforeDamage, (_, e) => { if (e.Target!.SilenceUntil > Time) e.Multiplier *= 1.4; }); break;
                case "ul_horn": On(CombatHook.UnitTick, (_, _) => {
                    if (Time < 2 || s.Horn) return; s.Horn = true; UnderlordsSummon(u, "ul_s_thunderhide", 1, u);
                }); break;
                case "ul_radiance": On(CombatHook.UnitTick, (c, _) => {
                    if (!c.Every(u, "ulRadiance", 1)) return;
                    foreach (var x in EnemiesNear(u, u.Cell, 2)) { c.Damage(u, x, 50 * c.Count); if (x.Alive && c.Random() < 0.17) x.Blind = true; }
                }); break;
                case "ul_satanic": On(CombatHook.DamageTaken, (c, _) => {
                    if (u.Health >= u.MaxHealth * 0.5 || s.SatanicCooldown > Time) return; s.SatanicCooldown = Time + 10; c.Buff(u, CombatStat.Omnivamp, 1.5, 3);
                }); break;
                case "ul_shiva": On(CombatHook.UnitTick, (c, _) => {
                    if (Time < 2 || s.Shiva) return; s.Shiva = true;
                    foreach (var x in EnemiesNear(u, u.Cell, 3)) { c.Damage(u, x, 250); if (x.Alive) UnderlordsTimedDebuff(x, "ulShivaSlow", CombatStat.AttackSpeed, 0.4, 4); }
                }); break;
                case "ul_vesture": Start(_ => { foreach (var a in UnderlordsAllies(u, 1)) a.Underlords.Grave = Math.Max(a.Underlords.Grave, 6); }); break;
                case "ul_vladmir": Start(_ => {
                    if (s.Vladmir) return; s.Vladmir = true;
                    foreach (var a in UnderlordsAllies(u, 2)) { a.Equipment.Amp += 0.08; a.Armor += 15; a.Omnivamp += 0.1; }
                }); break;
                default: throw new NotSupportedException($"Underlords held item {id} has not been migrated.");
            }
        }
    }
}
