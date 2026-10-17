namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private void BindUnderlordsPower(int team, CombatPower? power, int? owner = null)
    {
        if (power is null) return;
        string[] identity = power.Id.Split('_');
        if (identity.Length != 3 || identity[0] != "ul" || !(power.Id is "ul_anessa_dark" or "ul_anessa_heal" or "ul_hob_fire" or "ul_hob_crazy" or "ul_jules_brawl" or "ul_jules_bar" or "ul_enno_balm" or "ul_enno_rabid"))
            throw new NotSupportedException($"Underlords power {power.Id} has not been migrated.");
        string kind = identity[1], build = identity[2]; int rank = Math.Clamp((power.Rank == 0 ? 1 : power.Rank) - 1, 0, 5);
        var context = Hooks.Context(CombatHookSource.Power, power.Id, team, owner: owner);
        CombatUnit? tamer = null, archer = null; var cooldowns = new Dictionary<string, double>();
        double V(params double[] values) => values[rank];
        bool Cooldown(CombatUnit u, string key, double period, double fraction) {
            if (!cooldowns.TryGetValue(key, out double next)) cooldowns[key] = next = period * fraction;
            if (Time < next || !u.Alive || u.Stun > 0 || u.SilenceUntil > Time) return false;
            cooldowns[key] = Time + period; return true;
        }
        void CastFx(CombatUnit u) => Emit("cast", u); // Presentation cue, not a mana cast or onCast dispatch.
        void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => Hooks.Add(context, hook, action, order);
        On(CombatHook.CombatStart, (c, _) => {
            tamer = c.Mine.FirstOrDefault(u => u.Line.Tamer is not null); if (tamer is not { } u) return;
            var stats = rules.Tamers![u.Line.Tamer!];
            u.Items = []; u.Equipment = new(); Hooks.ClearHeldItems(u);
            u.AbilityPower = 100; u.CriticalChance = 0; u.CriticalDamage = 1.6; u.Dodge = 0; u.Omnivamp = 0;
            u.MaxHealth = stats.Hp[rank]; u.AttackDamage = u.BaseAttackDamage = stats.Ad[rank]; u.Armor = stats.Armor[rank]; u.MagicResist = stats.Mr;
            u.MaxMana = 0; u.Mana = 0;
            if (kind == "anessa") {
                archer = UnderlordsSummon(u, "ul_s_archer", rank / 2, u, scale: 1 + 0.15 * (rank % 2)); if (archer is not null) archer.Underlords.Summoner = u.Id;
            }
            if (kind == "jules") { u.Underlords.Tamer.BaseArmor = u.Armor; u.Underlords.Tamer.BaseMr = u.MagicResist; }
            if (kind == "enno" && build == "balm") UnderlordsYoink(u);
        }, -50);
        On(CombatHook.UnitTick, (c, e) => {
            var u = e.Unit!; if (u != tamer) return; var s = u.Underlords.Tamer;
            switch (kind) {
                case "anessa":
                    if (build == "dark") {
                        if (Cooldown(u, "a1", 8, 0.5)) { CastFx(u); foreach (var a in UnderlordsAllies(u)) c.Buff(a, CombatStat.AttackDamage, a.BaseAttackDamage * V(0.4, 0.43, 0.46, 0.49, 0.52, 0.55), 6); }
                        if (Cooldown(u, "a2", 8, 1) && TargetByDistance(u) is { } target) {
                            CastFx(u); c.Buff(target, CombatStat.Armor, -30, 6); target.Underlords.Tamer.ThrallTeam = u.Team; target.Underlords.Tamer.ThrallUntil = Time + 6;
                            foreach (var a in UnderlordsAllies(u)) if (a.Cell.DistanceTo(target.Cell) <= a.Range + 2) a.Target = target;
                        }
                    } else {
                        if (Cooldown(u, "a1", 8, 0.5)) {
                            CastFx(u); u.Health = Math.Max(1, u.Health - u.MaxHealth * 0.05);
                            foreach (var a in UnderlordsAllies(u)) { c.Heal(a, V(75, 110, 140, 170, 205, 230)); a.Underlords.Tamer.RitualUntil = Time + 6; a.Underlords.Tamer.RitualHps = V(10, 15, 21, 27, 33, 38); }
                        }
                        if (Cooldown(u, "a2", 15, 1) && TargetByDistance(u, true) is { } target) {
                            CastFx(u); var golem = UnderlordsSummon(u, "ul_s_golem", rank / 2, target, FreeCellNear(target, u.Cell), scale: 1 + 0.12 * (rank % 2));
                            if (golem is not null) c.After(0.25, () => { if (golem.Alive) foreach (var x in EnemiesNear(u, golem.Cell, 2)) c.Stun(x, 2); });
                        }
                    } break;
                case "hob":
                    if (s.CrazyUntil != 0 && Time > s.CrazyUntil) { s.CrazyUntil = 0; u.Range = 3; }
                    if (Cooldown(u, "h1", 12, 0.5)) {
                        var target = u.Target is { Alive: true } current ? current : TargetByDistance(u);
                        if (target is not null) {
                            CastFx(u); bool fire = build == "fire"; double total = fire ? V(640, 820, 1000, 1200, 1380, 1560) : V(420, 560, 700, 840, 980, 1120);
                            var group = EnemiesNear(u, target.Cell, 1);
                            foreach (var x in group) { c.Damage(u, x, total / group.Length); if (!x.Alive) continue; c.Burn(u, x); if (!fire) c.Stun(x, 1.5); }
                        }
                    }
                    if (build == "fire" && Cooldown(u, "h2", 13, 1)) {
                        CastFx(u); for (int k = 0; k < 8; k++) {
                            var target = RandomEnemyTarget(u); if (target is null) break;
                            foreach (var x in EnemiesNear(u, target.Cell, 1)) { c.Damage(u, x, V(160, 190, 225, 270, 325, 375)); if (x.Alive) c.Burn(u, x); }
                        }
                    }
                    if (build == "crazy" && Cooldown(u, "h2", 10, 1)) {
                        CastFx(u); s.CrazyUntil = Time + 6; u.Range = 10;
                        foreach (var a in UnderlordsAllies(u)) c.Buff(a, CombatStat.AttackSpeed, V(0.3, 0.42, 0.54, 0.66, 0.78, 0.9), 6);
                    } break;
                case "jules":
                    if (c.Every(u, "ulJulesArmor", 0.5)) { int adjacent = c.Adjacent(u).Count(x => x.Alive); u.Armor = s.BaseArmor + 10 * adjacent; u.MagicResist = s.BaseMr + 10 * adjacent; }
                    if (build == "brawl") {
                        if (Cooldown(u, "j1", 8, 0.5)) {
                            CastFx(u); s.GuardUntil = Time + 5; c.Heal(u, u.MaxHealth * 0.1);
                            foreach (var x in EnemiesNear(u, u.Cell, 2)) x.Target = u;
                            c.Buff(u, CombatStat.AttackSpeed, 1, 5); c.Buff(u, CombatStat.Omnivamp, 0.6, 5);
                        }
                        if (Cooldown(u, "j2", 12, 1)) {
                            CastFx(u);
                            foreach (var a in UnderlordsPick(UnderlordsAllies(u), 4)) c.Buff(a, CombatStat.AttackSpeed, 0.5, 4);
                            foreach (var x in UnderlordsPick(EnemiesOf(u), 4)) x.Underlords.Tamer.Barrel = 0.25;
                        }
                    } else {
                        if (Cooldown(u, "j1", 10, 0.5)) { CastFx(u); s.GuardUntil = Time + 5; s.ToastUntil = Time + 5; }
                        if (Cooldown(u, "j2", 10, 1)) { CastFx(u); foreach (var a in UnderlordsAllies(u)) a.Underlords.Tamer.HappyUntil = Time + 7; }
                    } break;
                case "enno":
                    if (Cooldown(u, "e1", 3, 0.5)) {
                        var targets = EnemiesNear(u, u.Cell, 3).OrderBy(x => x.Cell.DistanceTo(u.Cell)).ThenBy(x => x.Id).Take((int)V(3, 4, 4, 5, 5, 5)).ToArray();
                        if (targets.Length > 0) {
                            CastFx(u); foreach (var x in targets) {
                                UnderlordsPoison(u, x);
                                if (build == "rabid") c.Damage(u, x, V(25, 25, 35, 35, 45, 45) * (x.Underlords.Poison?.Stacks ?? 0), DamageKind.Physical);
                                else {
                                    double heal = V(35, 35, 45, 45, 55, 55);
                                    foreach (var a in UnderlordsAllies(u).OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id)) { if (heal < 5) break; c.Heal(a, heal, true); heal /= 2; }
                                }
                            }
                        }
                    }
                    if (build == "rabid" && Cooldown(u, "e2", 10, 1)) {
                        CastFx(u); foreach (var x in EnemiesOf(u)) { UnderlordsPoison(u, x, 3); c.Damage(u, x, V(45, 65, 90, 110, 130, 155), DamageKind.Physical); if (x.Alive) c.Stun(x, 1.1); }
                    } break;
            }
        });
        if (kind == "anessa") {
            On(CombatHook.Death, (c, e) => { if (e.Unit == tamer && archer is { Alive: true }) c.Damage(archer, archer, archer.Health + 1e6, DamageKind.True); });
            On(CombatHook.AnyDeath, (c, e) => {
                var victim = e.Unit!; var s = victim.Underlords.Tamer;
                if (build != "dark" || s.ThrallTeam != team || Time > s.ThrallUntil || victim.Team == team || victim.Summoned) return;
                s.ThrallTeam = null;
                var copy = c.Summon(team, victim.Line.Id, victim.Stage, victim.Cell, new());
                if (copy is not null && !copy.Traits.Contains("DEMON")) copy.Traits = copy.Traits.Append("DEMON").ToArray();
            });
        }
        if (kind == "hob") On(CombatHook.Attack, (c, e) => {
            var u = e.Unit!; if (u != tamer || !e.Target!.Alive) return; c.Burn(u, e.Target);
            if (u.Underlords.Tamer.CrazyUntil > Time) { var cool = EnemiesOf(u).FirstOrDefault(x => x.BurnSource is null || x.BurnUntil <= Time); if (cool is not null) u.Target = cool; }
        });
        if (kind == "jules") {
            On(CombatHook.BeforeDamageTaken, (_, e) => { if (e.Unit == tamer && e.Unit!.Underlords.Tamer.GuardUntil > Time) e.Reduction += 0.5; });
            On(CombatHook.DamageTaken, (c, e) => { var u = e.Unit!; if (u == tamer && u.Underlords.Tamer.ToastUntil > Time) foreach (var a in UnderlordsAllies(u, 1)) if (a != u) c.Heal(a, e.Amount, true); });
        }
        if (kind == "enno") On(CombatHook.DamageTaken, (c, e) => {
            var u = e.Unit!; if (u != tamer || !u.Alive) return; var s = u.Underlords.Tamer;
            double[] marks = rank >= 3 ? [0.5, 0.25] : [0.5]; if (s.Escapes >= marks.Length || u.Health >= u.MaxHealth * marks[s.Escapes]) return;
            s.Escapes++; CastFx(u); u.Stun = 0; u.BurnSource = null; u.BurnUntil = 0; u.BurnAccumulated = 0; u.Weakness = 0; u.SilenceUntil = 0;
            if (u.Underlords.Poison is { } poison) { poison.Stacks = 0; if (poison.Buff is not null) UnderlordsSpeed(u, ref poison.Buff, 0); }
            HexCell? best = null; int distance = -1;
            for (int i = 0; i < 56; i++) {
                var cell = new HexCell(i % 7, i / 7); if (UnitAt(cell) is not null) continue;
                var foes = EnemiesOf(u); int d = foes.Length == 0 ? 0 : foes.Min(x => x.Cell.DistanceTo(cell));
                if (d > distance && d <= 3) { distance = d; best = cell; }
            }
            if (best is { } destination) MoveFromHook(u, destination, 0.3);
            foreach (var x in EnemiesNear(u, u.Cell, 1)) c.Stun(x, 1.5);
        });
    }
    private CombatUnit[] UnderlordsPick(CombatUnit[] units, int count)
    {
        var pool = units.ToList(); var result = new List<CombatUnit>();
        while (result.Count < count && pool.Count > 0) { int index = (int)(random.Next() * pool.Count); result.Add(pool[index]); pool.RemoveAt(index); }
        return result.ToArray();
    }
    private void UnderlordsYoink(CombatUnit tamer)
    {
        ItemDefinition? best = null; CombatUnit? holder = null;
        foreach (var u in units) {
            if (u.Team == tamer.Team) continue;
            foreach (string id in u.Items) {
                if (!items.TryGetValue(id, out var item) || item.Hat || item.Category == "global" || item.Stats.Count == 0) continue;
                if (best is null || item.Tier < best.Tier) { best = item; holder = u; }
            }
        }
        if (best is null) return;
        void Apply(CombatUnit u, int sign) {
            double S(string key) => best.Stats.GetValueOrDefault(key);
            u.AttackDamage += sign * u.BaseAttackDamage * S("ad");
            if (S("as") != 0) u.AttackSpeed *= sign > 0 ? 1 + S("as") : 1 / (1 + S("as"));
            u.AbilityPower += sign * S("ap"); u.Armor += sign * S("armor"); u.MagicResist += sign * S("mr");
            u.CriticalChance += sign * S("crit"); u.Dodge += sign * S("dodge"); u.Omnivamp += sign * S("omnivamp");
            if (S("hp") != 0) u.MaxHealth = Math.Max(1, u.MaxHealth + sign * S("hp"));
        }
        Apply(holder!, -1); Apply(tamer, 1);
    }
}
