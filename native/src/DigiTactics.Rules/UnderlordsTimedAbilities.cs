namespace DigiTactics.Rules;

public sealed partial class CombatSimulation
{
    private readonly List<UnderlordsZone>[] underlordsZones = [[], []];
    private CombatUnit[] UnderlordsAllies(CombatUnit u, int radius = int.MaxValue) => units.Where(a => a.Alive && a.Team == u.Team && a.Cell.DistanceTo(u.Cell) <= radius).ToArray();
    private static double UnderlordsValue(CombatUnit u, double a, double b, double c) => u.Stage == 0 ? a : u.Stage == 1 ? b : c;
    private CombatUnit? UnderlordsSummon(CombatUnit source, string id, int stage, CombatUnit center, HexCell? cell = null, SummonOptions? options = null, double scale = 0)
    {
        if (cell is null) {
            int distance = int.MaxValue;
            // The authored helper uses DT.CELLS (the seven-column base board), even in Duos.
            for (int i = 0; i < 56; i++) {
                var candidate = new HexCell(i % 7, i / 7); if (UnitAt(candidate) is not null) continue;
                int d = candidate.DistanceTo(center.Cell); if (d <= 7 && d < distance) { cell = candidate; distance = d; }
            }
        }
        if (cell is null) return null;
        var u = Summon(source.Team, id, Math.Clamp(stage, 0, 2), cell.Value, options);
        if (u is null) return null;
        if (u.Line.Ability.Kind == "ul_passive") { u.MaxMana = 0; u.Mana = 0; }
        if (underlordsSummoningStone[u.Team]) { u.MaxHealth = UnitValues.Round(u.MaxHealth * 1.3); u.Health = u.MaxHealth; u.AttackSpeed *= 1.3; }
        if (scale != 0) { u.MaxHealth = UnitValues.Round(u.MaxHealth * scale); u.Health = u.MaxHealth; u.AttackDamage *= scale; u.BaseAttackDamage = u.AttackDamage; }
        return u;
    }
    private void UnderlordsQuill(CombatUnit u, double value)
    {
        Burst(u.Cell, 2, u.Team);
        foreach (var e in EnemiesNear(u, u.Cell, 2)) {
            int count = e.Underlords.Quill;
            AbilityDamage(u, e, value + UnderlordsValue(u, 30, 32, 34) * count);
            if (e.Alive) e.Underlords.Quill = Math.Min(5, count + 1);
        }
    }
    private void UnderlordsChainFrost(CombatUnit u, CombatUnit target)
    {
        var hit = new List<CombatUnit>(); CombatUnit? current = target;
        for (int k = 0; k < 7 && current is not null; k++) {
            hit.Add(current); Emit("beam", u, current); Damage(u, current, 150 * u.AbilityPower / 100, DamageKind.Magic);
            if (current.Alive) UnderlordsTimedDebuff(current, "ulChainFrost", CombatStat.AttackSpeed, 0.5, 2.5);
            var previous = current;
            current = EnemiesOf(u).Where(e => e != previous).Select(e => (Unit: e, Score: e.Cell.DistanceTo(previous.Cell) + hit.Count(x => x == e) * 3))
                .Where(x => x.Score <= 4).OrderBy(x => x.Score).Select(x => x.Unit).FirstOrDefault();
        }
    }
    private void UnderlordsDot(CombatUnit source, CombatUnit target, double dps, double duration, bool leech = false, bool armor = false)
    {
        if (target.Alive) target.Underlords.Dots.Add(new(source, dps, Time + duration, leech, armor));
    }
    private static void UnderlordsSpeed(CombatUnit unit, ref AttackSpeedBuff? buff, double value)
    {
        var previous = buff;
        int index = previous is null ? -1 : unit.SpeedBuffs.FindIndex(b => ReferenceEquals(b, previous));
        buff = new(value, double.PositiveInfinity);
        if (index < 0) unit.SpeedBuffs.Add(buff); else unit.SpeedBuffs[index] = buff;
    }
    private void UnderlordsPoison(CombatUnit source, CombatUnit target, int stacks = 1)
    {
        if (!target.Alive) return;
        var p = target.Underlords.Poison ??= new(source);
        p.Stacks = Math.Min(10, p.Stacks + stacks); p.Until = Time + 6; p.Source = source;
        UnderlordsSpeed(target, ref p.Buff, -0.04 * p.Stacks);
    }
    private bool CastUnderlordsTimedAbility(CombatUnit u, CombatUnit target, double value)
    {
        double V(double a, double b, double c) => UnderlordsValue(u, a, b, c);
        var s = u.Underlords; bool three = u.Stage == 2;
        switch (u.Line.Ability.Kind)
        {
            case "ul_napalm": Burst(target.Cell, 1, u.Team);
                foreach (var e in EnemiesNear(u, target.Cell, 1)) {
                    AbilityDamage(u, e, value); UnderlordsTimedDebuff(e, "ulNapalmSlow", CombatStat.AttackSpeed, 0.1, 4); if (three) ApplyBurn(u, e);
                } s.Napalm = Math.Min((int)V(4, 7, 10), s.Napalm + 1); break;
            case "ul_frostbite":
                double freeze = V(1.5, 2, 2.5); Emit("beam", u, target); StunUnit(target, freeze); Emit("text", target, text: "Frozen"); target.WoundUntil = Time + freeze + 1; UnderlordsDot(u, target, value, freeze + 1); break;
            case "ul_attendants": s.Attend = new(Time + 11, Time, value, u); Emit("shield", u); break;
            case "ul_frostshield":
                var pool = UnderlordsAllies(u).Where(a => !a.Summoned).ToArray(); int index = (int)(random.Next() * pool.Length);
                var shielded = pool.Length > 0 ? pool[index] : u;
                Buff(shielded, CombatStat.DamageReduction, V(0.12, 0.18, 0.25), 7); shielded.Underlords.FrostAura = new(Time + 7, Time + 1, value, u); Emit("shield", shielded);
                if (three) UnderlordsChainFrost(u, target); break;
            case "ul_summon":
                string id = u.Line.Summon!;
                var bear = id == "ul_s_bear" ? units.FirstOrDefault(b => b.Alive && b.Team == u.Team && b.Line.Id == id && b.Underlords.Summoner == u.Id) : null;
                if (bear is not null) { Heal(bear, bear.MaxHealth * 0.3); Buff(bear, CombatStat.AttackSpeed, 0.3, 5); Emit("text", bear, text: "Roar"); break; }
                var center = id == "ul_s_treant" && three ? EnemiesOf(u).OrderBy(e => u.Team == 0 ? e.Cell.Row : -e.Cell.Row).FirstOrDefault() ?? u : u;
                var summoned = UnderlordsSummon(u, id, u.Stage, center);
                if (summoned is not null) { summoned.Underlords.Summoner = u.Id; if (id == "ul_s_ward" && three) summoned.Underlords.MagicImmuneUntil = double.PositiveInfinity; Emit("text", summoned, text: "Summoned"); } break;
            case "ul_quill": UnderlordsQuill(u, value); break;
            case "ul_bladefury":
                s.Fury = new(Time + 5, Time, value, u); s.MagicImmuneUntil = Math.Max(s.MagicImmuneUntil, Time + 5);
                u.Equipment.CrowdControlImmunity = Math.Max(u.Equipment.CrowdControlImmunity, Time + 5); break;
            case "ul_duel":
                var opponent = EnemiesNear(u, u.Cell, 1).OrderBy(e => e.Health).ThenBy(e => e.Id).FirstOrDefault() ?? target;
                double duel = V(4, 4.75, 5.5); u.Target = opponent; opponent.Target = u;
                s.Duel = new(opponent, Time + duel); opponent.Underlords.Duel = new(u, Time + duel);
                foreach (var x in new[] { u, opponent }) { x.SilenceUntil = Math.Max(x.SilenceUntil, Time + duel); Emit("text", x, text: "Silenced"); x.WoundUntil = Time + duel; }
                Emit("text", u, text: "Duel!");
                if (three) foreach (var e in EnemiesOf(u)) if (e != opponent && e.Target == u) e.Target = null; break;
            case "ul_aphotic":
                var ally = UnderlordsAllies(u).OrderBy(a => a.Health / a.MaxHealth).ThenBy(a => a.Id).FirstOrDefault() ?? u;
                ally.Stun = 0; ally.Weakness = 0; ally.BurnSource = null; ally.BurnUntil = 0; ally.BurnAccumulated = 0;
                AddShield(ally, value); ally.Underlords.Aphotic = new(u, value, Time + 10); break;
            case "ul_acid": underlordsZones[u.Team].Add(new(u, target.Cell, Time + 8, value, V(6, 12, 18), Time)); Burst(target.Cell, 2, u.Team); break;
            case "ul_axes":
                foreach (var e in LineTargets(u, target)) { Emit("beam", u, e); AbilityDamage(u, e, value); if (e.Alive) e.Underlords.Axed = new(u.Id, V(0.1, 0.2, 0.3), Time + 6); } break;
            case "ul_rage":
                u.Equipment.CrowdControlImmunity = Math.Max(u.Equipment.CrowdControlImmunity, Time + 5);
                s.MagicImmuneUntil = Math.Max(s.MagicImmuneUntil, Time + 5); Buff(u, CombatStat.AttackSpeed, 0.3, 5); Emit("text", u, text: "Rage"); break;
            case "ul_shapeshift":
                for (int k = 0; k < V(2, 2, 4); k++) { var w = UnderlordsSummon(u, "ul_s_wolf", Math.Max(0, u.Stage - 1), u); if (w is not null) w.Underlords.Summoner = u.Id; }
                if (!s.Shifted) {
                    s.Shifted = true; double add = UnitValues.Round(u.MaxHealth * V(0.3, 0.4, 0.5)); u.MaxHealth += add; Heal(u, add);
                    u.CriticalChance += 0.3; u.CriticalDamage = Math.Max(u.CriticalDamage, V(1.5, 1.75, 2));
                } break;
            case "ul_wards":
                for (int k = 0; k < V(2, 3, 4); k++) {
                    var cell = FreeCellNear(target, u.Cell); if (cell is null) break;
                    var w = UnderlordsSummon(u, "ul_s_serpent", u.Stage, target, cell); if (w is not null) w.Target = target;
                } if (three && target.Alive) { StunUnit(target, 3); Emit("text", target, text: "Hexed"); } break;
            case "ul_leech": Emit("beam", u, target); UnderlordsTimedDebuff(target, "ulLeechSlow", CombatStat.AttackSpeed, 0.5, 5); UnderlordsDot(u, target, value, 5, true, three); break;
            case "ul_exorcism": s.Exorcism = new(0, Time, value, u, (int)V(6, 8, 10)); break;
            case "ul_doom": Emit("beam", u, target); target.SilenceUntil = Math.Max(target.SilenceUntil, Time + 10); Emit("text", target, text: "Silenced"); UnderlordsDot(u, target, value, 10); break;
            case "ul_refraction": s.Refract += (int)V(4, 6, 8); s.RefractHits += (int)V(4, 6, 8); s.RefractAmp = V(0.4, 0.6, 0.8); Emit("shield", u); break;
            case "ul_nethertoxin": Emit("shot", u, target, big: true); target.SilenceUntil = Math.Max(target.SilenceUntil, Time + 5); Emit("text", target, text: "Silenced"); UnderlordsDot(u, target, value, 5); UnderlordsPoison(u, target, 2); break;
            case "ul_gaze":
                var gazed = EnemiesNear(u, u.Cell, 3);
                foreach (var e in gazed) { AbilityDamage(u, e, value); if (e.Alive) UnderlordsTimedDebuff(e, "ulGaze", CombatStat.AttackSpeed, 0.3, 3); }
                Burst(u.Cell, 3, u.Team);
                Schedule(2, () => {
                    if (!u.Alive) return;
                    foreach (var e in EnemiesNear(u, u.Cell, 3)) if (gazed.Contains(e)) { double duration = UnderlordsValue(u, 1.5, 2, 2.5); StunUnit(e, duration); e.Underlords.StoneUntil = Time + duration; Emit("text", e, text: "Stone"); }
                }, u); break;
            case "ul_wraithfire": Emit("shot", u, target, big: true); StunUnit(target, 2); UnderlordsDot(u, target, value, 5); break;
            default: return false;
        }
        return true;
    }
}
