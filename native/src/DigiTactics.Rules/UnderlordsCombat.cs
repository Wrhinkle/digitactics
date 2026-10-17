namespace DigiTactics.Rules;

public sealed record UnderlordsState(double UlDuel = 0, double UlBeastKills = 0);
public sealed record UnderlordsCombatRewards(double Kills, double Gold, double Duel);

public sealed partial class CombatSimulation
{
    public UnderlordsCombatRewards UnderlordsRewards(int team) => new(units.Where(u => u.Team == team && !u.Summoned && u.Traits.Contains("BEAST")).Sum(u => u.Underlords.Kills), units.Where(u => u.Team == team).Sum(u => u.Underlords.Gold), units.Where(u => u.Team == team).Sum(u => u.Underlords.DuelWin));
    private void BindUnderlords(CombatEntry[] sideA, CombatEntry[] sideB, CombatSetup setup)
    {
        BindUnderlordsTraits(sideA, sideB, setup);
        for (int team = 0; team < 2; team++) {
            var side = team == 0 ? setup.SideA : setup.SideB;
            void BindAugments(string[] ids, int? owner, double partnerTop) {
                foreach (string id in ids) {
                    var augment = Hooks.Context(CombatHookSource.Augment, id, team, owner: owner);
                    if (!DuosCombatAugments.TryBind(Hooks, augment, partnerTop)) throw new NotSupportedException($"Underlords augment {id} has not been migrated.");
                }
            }
            if (side.Parts is { } parts) foreach (var part in parts) BindAugments(part.Augments ?? [], part.Owner, part.PartnerTop);
            else BindAugments(side.Augments ?? [], null, side.PartnerTop);
        }
        foreach (var u in units) foreach (var id in u.Items)
            if (items.TryGetValue(id, out var item) && item.Hooks.Length > 0) throw new NotSupportedException($"Underlords item {id} is still being migrated.");
        var context = Hooks.Context(CombatHookSource.Mechanic, "tamers");
        void On(CombatHook hook, Action<CombatHookContext, CombatHookEvent> action, int order = 0) => Hooks.Add(context, hook, action, order);
        On(CombatHook.CombatStart, (c, _) => {
            for (int i = 0; i < units.Count; i++) {
                var u = units[i];
                if (u.Line.Ability.Kind == "ul_passive" && !u.Line.Id.StartsWith("ul_tamer_", StringComparison.Ordinal)) { u.MaxMana = 0; u.Mana = 0; }
                StartUnderlordsHero(c, u);
            }
        }, 5);
        On(CombatHook.CombatReady, (_, _) => {
            foreach (var u in units) {
                double bonus = (u.Team == 0 ? setup.SideA : setup.SideB).Underlords?.UlDuel ?? 0;
                if (bonus != 0 && u.Line.Id == "ul_legion" && !u.Summoned) { u.AttackDamage += bonus; u.BaseAttackDamage += bonus; }
            }
            foreach (var u in units) if (u.Line.Id == "ul_juggernaut" && u.Stage == 2) foreach (var a in UnderlordsAllies(u)) {
                if (a == u || a.Summoned) continue;
                double add = UnitValues.Round(u.MaxHealth * 0.15); a.MaxHealth += add; a.Health += add; u.Underlords.Juggernaut.Add((a, add));
            }
        });
        On(CombatHook.Tick, (c, _) => {
            for (int team = 0; team < 2; team++) {
                foreach (var z in underlordsZones[team]) {
                    if (Time > z.Until || Time < z.Next) continue; z.Next = Time + 0.5;
                    foreach (var e in units.Where(e => e.Alive && e.Team != team && e.Cell.DistanceTo(z.Cell) <= 2).ToArray()) {
                        c.Damage(z.Source, e, z.Dps * 0.5); if (e.Alive) UnderlordsTimedDebuff(e, "ulAcid", CombatStat.Armor, z.Armor, 1);
                    }
                }
                underlordsZones[team].RemoveAll(z => Time > z.Until);
            }
        });
        On(CombatHook.UnitTick, (c, e) => TickUnderlords(c, e.Unit!, e.DeltaTime));
        On(CombatHook.Attack, (c, e) => { AttackUnderlordsHero(c, e); StruckUnderlordsHero(c, e); });
        On(CombatHook.BeforeDamage, (_, e) => {
            var src = e.Unit!; var target = e.Target!;
            if (target.Underlords.Axed is { } ax && ax.SourceId == src.Id && ax.Until > Time) e.Multiplier *= 1 + ax.Amp;
            if (src.Line.Id == "ul_terrorblade" && src.Stage == 2 && target.Traits.Contains("DEMON")) e.Multiplier *= 1.6;
            if (e.Kind == DamageKind.Physical && !e.IsBurn) {
                if (src.Underlords.RefractHits > 0) { e.Multiplier *= 1 + src.Underlords.RefractAmp; src.Underlords.RefractHits--; }
                if (target.Underlords.StoneUntil > Time) e.Multiplier *= 1.2;
            }
        });
        On(CombatHook.BeforeDamageTaken, (c, e) => {
            var u = e.Unit!; var s = u.Underlords;
            if (e.Kind == DamageKind.Magic && s.MagicImmuneUntil > Time) { e.Multiplier = 0; return; }
            if (s.Refract > 0 && e.Amount > 0 && !e.IsBurn) { s.Refract--; e.Multiplier = 0; return; }
            if (e.Kind == DamageKind.Physical && !e.IsBurn) {
                if (s.EvadeUntil > Time) { e.Multiplier = 0; return; }
                if (u.Line.Id == "ul_queenofpain" && u.Stage == 2 && u.Cell.DistanceTo(e.Source!.Cell) <= 2 && c.Random() < 0.25) { e.Multiplier = 0; return; }
                if (s.Rogue is { } rogue && c.Random() < rogue.Evasion) { e.Multiplier = 0; u.AttackDamage += rogue.AttackDamage; return; }
            }
            if (e.Source!.Summoned && u.Line.Id == "ul_beastmaster" && u.Stage == 2) e.Multiplier *= 0.25;
        });
        On(CombatHook.DamageTaken, (c, e) => {
            var u = e.Unit!;
            if (u.Underlords.Aphotic is { } ap && u.Shield <= 0) {
                u.Underlords.Aphotic = null;
                if (Time <= ap.Until) foreach (var x in EnemiesNear(u, u.Cell, 2)) c.Damage(ap.Source, x, ap.Value);
            }
            HurtUnderlordsHero(c, u);
        });
        On(CombatHook.Lethal, (c, e) => {
            var u = e.Unit!; var s = u.Underlords;
            if (u.Health > 0 || s.Grave == 0 || s.GraveDone) return;
            if (s.GraveUntil != 0) { if (Time < s.GraveUntil) u.Health = 1; return; }
            s.GraveUntil = Time + s.Grave; u.Health = 1;
            c.After(s.Grave, () => { if (u.Alive) { s.GraveDone = true; c.Damage(u, u, u.Health + 1e6, DamageKind.True); } }, u);
        });
        On(CombatHook.Death, (c, e) => {
            var u = e.Unit!;
            if (u.Line.Id == "ul_vengeful" && u.Stage == 2) foreach (var x in EnemiesNear(u, u.Cell, 2)) c.Damage(u, x, 300);
            if (u.Line.Id == "ul_juggernaut") foreach (var (a, add) in u.Underlords.Juggernaut) if (a.Alive) { a.MaxHealth -= add; a.Health = Math.Max(1, Math.Min(a.Health, a.MaxHealth)); }
            foreach (var p in units) if (p.Alive && p.Team != u.Team && p.Line.Id == "ul_pudge" && p.Stage == 2 && p.Cell.DistanceTo(u.Cell) <= 1) { p.MaxHealth += 300; p.Health += 300; p.MagicResist += 10; }
            foreach (var d in u.Underlords.Dots.ToArray()) {
                if (!d.Leech || Time > d.Until) continue;
                var next = units.FirstOrDefault(x => x.Alive && x.Team != d.Source.Team && x.Cell.DistanceTo(u.Cell) <= 1);
                if (next is not null) UnderlordsDot(d.Source, next, d.Dps, d.Until - Time, true, d.Armor);
            }
        });
        On(CombatHook.Kill, (_, e) => {
            var u = e.Unit!; var victim = e.Target!; u.Underlords.Kills += victim.Summoned ? 0.25 : 1;
            if (!victim.Summoned && u.Line.Id == "ul_bounty" && u.Stage == 2) u.Underlords.Gold++;
            if (u.Underlords.Duel is { } duel && duel.Opponent == victim && Time <= duel.Until && u.Line.Id == "ul_legion") u.Underlords.DuelWin += UnderlordsValue(u, 6, 10, 15);
        });
        On(CombatHook.Cast, (c, e) => {
            var u = e.Unit!;
            if (u.Stage == 2) {
                if (u.Line.Id == "ul_stormspirit") u.Underlords.Overload = true;
                if (u.Line.Id == "ul_windranger") { u.Underlords.EvadeUntil = Time + 3; u.Underlords.MagicImmuneUntil = Math.Max(u.Underlords.MagicImmuneUntil, Time + 3); }
            }
            if (u.Traits.Contains("DEMON") && units.Any(s => s.Alive && s.Team == u.Team && s.Line.Id == "ul_spectre" && s.Stage == 2))
                foreach (var d in units.ToArray()) if (d.Alive && d.Team == u.Team && d.Traits.Contains("DEMON") && d.Target is { Alive: true } target) {
                    c.Damage(d, target, 150); if (target.Alive) c.Burn(d, target);
                }
        });
    }
    private void TickUnderlords(CombatHookContext c, CombatUnit u, double dt)
    {
        var s = u.Underlords;
        if (s.Poison is { Stacks: > 0 } p) {
            if (Time > p.Until) { p.Stacks = 0; if (p.Buff is not null) UnderlordsSpeed(u, ref p.Buff, 0); }
            else { p.Accumulated += 4 * p.Stacks * dt; if (p.Accumulated >= 18) { double amount = p.Accumulated; p.Accumulated = 0; c.Damage(p.Source, u, amount); if (!u.Alive) return; } }
        }
        // Death can transfer a leech to another unit; preserve insertion order and early exit.
        for (int i = 0; i < s.Dots.Count; i++) {
            var d = s.Dots[i]; if (Time > d.Until) continue;
            d.Accumulated += d.Dps * dt; if (d.Accumulated < 20) continue;
            double amount = d.Accumulated; d.Accumulated = 0; double done = c.Damage(d.Source, u, amount);
            if (d.Leech) foreach (var a in units.Where(a => a.Alive && a.Team == d.Source.Team && a.Cell.DistanceTo(u.Cell) <= 1).ToArray()) {
                c.Heal(a, done, true); if (d.Armor && !a.Underlords.LivingArmor) { a.Underlords.LivingArmor = true; a.Armor += 10; }
            }
            if (!u.Alive) return;
        }
        s.Dots.RemoveAll(d => Time > d.Until);
        if (s.FrostAura is { } frost && Time <= frost.Until && Time >= frost.Next) {
            frost.Next = Time + 1;
            foreach (var x in EnemiesNear(u, u.Cell, 2)) { c.Damage(frost.Source, x, frost.Value); if (x.Alive) UnderlordsTimedDebuff(x, "ulFrostSlow", CombatStat.AttackSpeed, 0.2, 1.2); }
        }
        if (s.Exorcism is { Left: > 0 } ex && Time >= ex.Next && u.Alive) {
            ex.Next = Time + 6.0 / 8; ex.Left--;
            var pool = EnemiesNear(u, u.Cell, 3);
            if (pool.Length > 0) { var x = pool[(int)(c.Random() * pool.Length)]; double done = c.Damage(u, x, ex.Value); c.Heal(u, done * 0.25, true); }
        }
        TickUnderlordsHero(c, u);
    }
}
