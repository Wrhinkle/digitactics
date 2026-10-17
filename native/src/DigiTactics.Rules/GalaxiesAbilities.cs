namespace DigiTactics.Rules;

internal sealed class GalaxiesUnitState
{
    internal bool Frenzy, Mega, Excited, Casting, Lanterned;
    internal bool Piloting { get; set; }
    internal double DisarmUntil { get; set; }
    internal double FrenzyUntil, FrenzyPercent, OriginalRange, DreadUntil, DreadDps, ChosenUntil, ChosenBonus, ChosenNext,
        PoisonUntil, PoisonNext, PoisonDps, CycloneUntil, CycloneNext, CycloneDamage, TaxMana, HexUntil, HexAmplification;
    internal int Dunks, Slashes, Lanterns;
    internal CombatUnit? PoisonSource;
    internal HashSet<int> CycloneHit { get; } = [];
}

public sealed partial class CombatSimulation
{
    private static readonly HashSet<string> GalaxiesAbilities = [
        "bomb", "snipe", "riposte", "smoke", "sap", "isolate", "toss", "plumage", "bubble", "orb", "fireshield", "grab", "dunk", "frenzy",
        "dashshot", "dreadshield", "crater", "entrance", "triple", "lastbreath", "contempt", "arrow", "meep", "poison", "emp", "chosen", "flame",
        "backstab", "orbs", "tumble", "charge", "shark", "transform", "surge", "whisper", "excited", "energyslash", "wish", "traps", "deathray",
        "ray", "cyclone", "fleet", "rupture", "chronobreak", "orbital", "gale", "polymorph", "bullettime", "lantern", "reel", "meteors"];
    private readonly int[] meeps = new int[2];
    public int MeepCount(int team) => meeps[team];
    private CombatUnit[] GalaxyEnemies(CombatUnit u) => units.Where(e => e.Alive && e.Team != u.Team).ToArray();
    private CombatUnit? GalaxyTarget(CombatUnit u, bool farthest = false) => farthest
        ? GalaxyEnemies(u).OrderByDescending(e => e.Cell.DistanceTo(u.Cell)).FirstOrDefault()
        : GalaxyEnemies(u).OrderBy(e => e.Cell.DistanceTo(u.Cell) * 10000 + e.Health).FirstOrDefault();
    private double GalaxyHit(CombatUnit u, CombatUnit e, double value) => e.Alive ? AbilityDamage(u, e, value) : 0;
    private double GalaxyStrike(CombatUnit u, CombatUnit e, double multiplier = 1)
    {
        if (!e.Alive) return 0;
        double amount = u.AttackDamage * multiplier;
        bool critical = u.CriticalChance > 0 && random.Next() < u.CriticalChance;
        return Damage(u, e, critical ? amount * u.CriticalDamage : amount, DamageKind.Physical, critical: critical);
    }
    private void GalaxyBlink(CombatUnit u, CombatUnit e, HexCell? toward = null)
    {
        if (u.Cell.DistanceTo(e.Cell) <= 1 || FreeNeighbor(e, toward ?? u.Cell) is not { } cell) return;
        Emit("leap", u); MoveFromHook(u, cell, 0.2);
    }
    private void GalaxyShield(CombatUnit u, double amount, double duration)
    {
        if (amount <= 0) return;
        AddShield(u, amount); Schedule(duration, () => u.Shield = Math.Max(0, u.Shield - Math.Min(u.Shield, amount)), u);
    }
    private void GalaxyCasting(CombatUnit u, Action action)
    { u.Galaxies.Casting = true; try { action(); } finally { u.Galaxies.Casting = false; } }
    private static void GalaxyTax(CombatUnit u, double fraction)
    {
        if (!u.Alive || u.Galaxies.TaxMana != 0 || u.MaxMana <= 0 || u.MaxMana > 500) return;
        u.Galaxies.TaxMana = u.MaxMana; u.MaxMana = UnitValues.Round(u.MaxMana * (1 + fraction));
    }
    private bool CastGalaxiesAbility(CombatUnit u, CombatUnit target, double v)
    {
        var s = u.Galaxies;
        double Stage(double a, double b, double c) => u.Stage == 0 ? a : u.Stage == 1 ? b : c;
        CombatUnit Farthest() => GalaxyTarget(u, true) ?? target;
        CombatUnit NearFarthest() => GalaxyEnemies(u).Where(e => e.Cell.DistanceTo(u.Cell) <= 3).OrderByDescending(e => e.Cell.DistanceTo(u.Cell)).FirstOrDefault() ?? target;
        switch (u.Line.Ability.Kind)
        {
            case "bomb": case "excited": foreach (var e in EnemiesNear(u, target.Cell, 1)) GalaxyHit(u, e, v); break;
            case "snipe": GalaxyHit(u, Farthest(), v); break;
            case "riposte":
                u.InvulnerableUntil = Math.Max(u.InvulnerableUntil, Time + 1);
                Schedule(1, () => { var e = GalaxyTarget(u); if (e is null) return; StunUnit(e, Stage(1.5, 1.5, 2.5)); GalaxyHit(u, e, v); }, u); break;
            case "smoke": foreach (var e in EnemiesNear(u, target.Cell, 1)) { GalaxyHit(u, e, v); if (e.Alive) { e.Weakness = Math.Max(e.WeakUntil > Time ? e.Weakness : 0, 0.5); e.WeakUntil = Time + 3 + u.Stage; } } break;
            case "sap": foreach (var e in LineTargets(u, target)) {
                GalaxyHit(u, e, v); if (!e.Alive) continue;
                double armor = Math.Max(0, e.Armor) * 0.3, mr = Math.Max(0, e.MagicResist) * 0.3;
                Buff(e, CombatStat.Armor, -armor, 6); Buff(e, CombatStat.MagicResist, -mr, 6); Buff(u, CombatStat.Armor, armor, 6); Buff(u, CombatStat.MagicResist, mr, 6);
            } break;
            case "isolate": { var e = GalaxyTarget(u) ?? target; bool lone = !Adjacent(e).Any(a => a != e && a.Alive && a.Team == e.Team); GalaxyHit(u, e, lone ? v * 2.5 : v); break; }
            case "toss": GalaxyHit(u, Farthest(), v); AddShield(u, v * 1.6); break;
            case "plumage": Buff(u, CombatStat.AttackSpeed, v / 100, 8); break;
            case "bubble": case "orbs": {
                var list = GalaxyEnemies(u); if (list.Length == 0) break; var best = list[0];
                foreach (var e in list.Skip(1)) {
                    double delta = e.Health / e.MaxHealth - best.Health / best.MaxHealth;
                    if (delta > 1e-9 || (u.Line.Ability.Kind == "bubble" && Math.Abs(delta) < 1e-9 && e.MaxHealth > best.MaxHealth)) best = e;
                }
                if (u.Line.Ability.Kind == "bubble") { StunUnit(best, Stage(2, 2.25, 3)); GalaxyHit(u, best, v); }
                else for (int i = 0; i < 3 + u.Stage && best.Alive; i++) GalaxyHit(u, best, v);
                break;
            }
            case "orb": case "deathray": {
                var hits = LineTargets(u, target); foreach (var e in hits) GalaxyHit(u, e, v);
                if (u.Line.Ability.Kind == "orb") Schedule(0.5, () => { foreach (var e in hits.Where(e => e.Alive)) Damage(u, e, v * 0.75, DamageKind.True); }, u);
                else Schedule(1, () => GalaxyCasting(u, () => { foreach (var e in hits.Where(e => e.Alive)) GalaxyHit(u, e, v * 0.8); }), u);
                break;
            }
            case "fireshield": foreach (var e in EnemiesNear(u, target.Cell, 1)) GalaxyHit(u, e, v); GalaxyShield(u, v * 1.8, 4); break;
            case "grab": case "reel": {
                var e = Farthest(); if (u.Cell.DistanceTo(e.Cell) > 1 && FreeNeighbor(u, e.Cell) is { } cell) MoveFromHook(e, cell, 0.25);
                if (u.Line.Ability.Kind == "grab") {
                    StunUnit(e, 2); GalaxyHit(u, e, v);
                    if (e.Alive) foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && a != u && a.Cell.DistanceTo(e.Cell) <= a.Range)) a.Target = e;
                } else {
                    Damage(u, e, v, DamageKind.True); if (e.Alive && e.Health < e.MaxHealth * 0.25) Damage(u, e, e.Health + 1, DamageKind.True);
                    if (!e.Alive) u.MaxMana = Math.Max(30, u.MaxMana - 20);
                } break;
            }
            case "dunk": GalaxyHit(u, target, target.Health < target.MaxHealth * 0.5 ? v * 2 : v); if (!target.Alive && ++s.Dunks <= 4) u.Mana = u.MaxMana; break;
            case "frenzy": if (!s.Frenzy) s.OriginalRange = u.Range; s.Frenzy = true; s.FrenzyUntil = Time + 4; s.FrenzyPercent = v / 100; u.Range = s.OriginalRange + 6; Buff(u, CombatStat.AttackSpeed, 0.6, 4); break;
            case "dashshot": GalaxyStrike(u, target); GalaxyHit(u, target, v); break;
            case "dreadshield": GalaxyShield(u, v * 2, 8); s.DreadUntil = Time + 8; s.DreadDps = v * 0.25; break;
            case "crater": foreach (var e in EnemiesNear(u, target.Cell, 1)) { StunUnit(e, e == target ? Stage(2, 2.5, 3) : 1); GalaxyHit(u, e, e == target ? v : v / 2); } break;
            case "entrance": { var e = NearFarthest(); GalaxyBlink(u, e); foreach (var x in EnemiesNear(u, e.Cell, 1)) { StunUnit(x, 1.25); GalaxyHit(u, x, v); } break; }
            case "triple": for (int i = 0; i < 3 && target.Alive; i++) GalaxyStrike(u, target); if (target.Alive) { StunUnit(target, 1.5); GalaxyHit(u, target, v); } break;
            case "lastbreath": { var e = NearFarthest(); GalaxyBlink(u, e); u.Target = e; StunUnit(e, 1); for (int i = 0; i < 3 + u.Stage && e.Alive; i++) GalaxyStrike(u, e); GalaxyHit(u, e, v); break; }
            case "contempt": GalaxyHit(u, target, v); if (target.Alive) { double steal = target.AttackDamage * 0.25; Buff(target, CombatStat.AttackDamage, -steal, double.PositiveInfinity); Buff(u, CombatStat.AttackDamage, steal, double.PositiveInfinity); } break;
            case "arrow": foreach (var e in EnemiesNear(u, Farthest().Cell, 1)) { StunUnit(e, 1.5); GalaxyHit(u, e, v); } break;
            case "meep": StunUnit(target, 1); GalaxyHit(u, target, v); meeps[u.Team]++; break;
            case "poison": {
                var list = GalaxyEnemies(u); if (list.Length == 0) break; var fresh = list.Where(e => e.Galaxies.PoisonUntil <= Time).ToArray();
                var e = (fresh.Length > 0 ? fresh : list).OrderBy(e => e.Cell.DistanceTo(u.Cell)).First();
                e.Galaxies.PoisonSource = u; e.Galaxies.PoisonDps = v / 5; e.Galaxies.PoisonUntil = Time + 10; e.Galaxies.PoisonNext = Time + 1; e.WoundUntil = Math.Max(e.WoundUntil, Time + 10); break;
            }
            case "emp": { var list = GalaxyEnemies(u); var e = list.Length > 0 ? list[(int)(random.Next() * list.Length)] : target; foreach (var x in EnemiesNear(u, e.Cell, 1)) { GalaxyHit(u, x, v); GalaxyTax(x, 0.4); } break; }
            case "chosen": s.ChosenUntil = Time + 5; s.ChosenBonus = v; s.ChosenNext = Time + 1; Buff(u, CombatStat.AttackSpeed, 0.3, 5); break;
            case "flame": foreach (var e in EnemiesNear(u, target.Cell, 1)) { GalaxyHit(u, e, v); if (e.Alive) ApplyBurn(u, e); } break;
            case "backstab": GalaxyBlink(u, target, new(target.Cell.Column, u.Team == 0 ? 0 : 7)); u.Target = target; Damage(u, target, u.AttackDamage * 2 * u.CriticalDamage, DamageKind.Physical, critical: true); GalaxyHit(u, target, v); break;
            case "tumble": Buff(u, CombatStat.Dodge, 0.5, 2); GalaxyStrike(u, target, 1.6); if (target.Alive) Damage(u, target, v, DamageKind.True); break;
            case "charge": { var e = Farthest(); foreach (var x in LineTargets(u, e)) if (x != e) GalaxyHit(u, x, v * 0.4); GalaxyBlink(u, e); u.Target = e; StunUnit(e, Stage(2, 2.5, 3)); GalaxyHit(u, e, v); break; }
            case "shark": case "orbital": {
                var at = u.Line.Ability.Kind == "shark" ? Farthest().Cell : target.Cell; bool shark = u.Line.Ability.Kind == "shark";
                Schedule(shark ? 0.6 : 1.5, () => GalaxyCasting(u, () => { foreach (var e in EnemiesNear(u, at, shark ? 1 : 2)) { if (shark) StunUnit(e, 1.5); GalaxyHit(u, e, v); } }), u); break;
            }
            case "transform": if (!s.Mega) { s.Mega = true; u.MaxHealth += v * 2; u.Health += v * 2; u.AttackDamage += v * 0.25; u.Range = 1; } GalaxyBlink(u, target); u.Target = target; foreach (var e in EnemiesNear(u, target.Cell, 1)) { StunUnit(e, 1.5); GalaxyHit(u, e, v); } break;
            case "surge": { CombatUnit? e = target; for (int k = 0; k < 4 && e is not null; k++) { GalaxyBlink(u, e); u.Target = e; GalaxyStrike(u, e, 1.5); GalaxyHit(u, e, v); if (e.Alive) break; e = GalaxyEnemies(u).OrderByDescending(x => x.Mana).FirstOrDefault(); } break; }
            case "whisper": GalaxyStrike(u, target, Stage(3, 3.4, 4.5)); GalaxyHit(u, target, v); break;
            case "energyslash": s.Slashes++; GalaxyShield(u, v * 1.2, 3); foreach (var e in s.Slashes % 3 == 0 ? LineTargets(u, target).ToArray() : EnemiesNear(u, u.Cell, 1)) GalaxyHit(u, e, s.Slashes % 3 == 0 ? v * 2 : v); break;
            case "wish": foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && !a.Galaxies.Piloting)) Heal(a, v); break;
            case "traps": {
                var list = GalaxyEnemies(u); if (list.Length == 0) break; var spots = new HexCell[3]; for (int i = 0; i < 3; i++) spots[i] = list[(int)(random.Next() * list.Length)].Cell;
                Schedule(1.2, () => GalaxyCasting(u, () => { foreach (var at in spots) foreach (var e in EnemiesNear(u, at, 1)) { GalaxyHit(u, e, v); if (e.Alive) Buff(e, CombatStat.AttackSpeed, -0.3, 3); } }), u); break;
            }
            case "ray": { var aim = target.Cell; void Pulse() => GalaxyCasting(u, () => { var e = UnitAt(aim); if (e is null || !e.Alive) e = GalaxyTarget(u); if (e is not null) foreach (var x in LineTargets(u, e)) GalaxyHit(u, x, v * 0.5); }); Pulse(); Schedule(0.4, Pulse, u); Schedule(0.8, Pulse, u); break; }
            case "cyclone": s.CycloneUntil = Time + 3; s.CycloneDamage = v / 6; s.CycloneNext = Time; s.CycloneHit.Clear(); break;
            case "fleet": case "meteors": for (int k = 0; k < (u.Line.Ability.Kind == "fleet" ? 3 + 2 * u.Stage : 4 + u.Stage); k++) {
                var list = GalaxyEnemies(u); if (list.Length == 0) break; var e = list[(int)(random.Next() * list.Length)]; GalaxyHit(u, e, v);
                if (u.Line.Ability.Kind == "meteors" && !e.Alive) foreach (var x in EnemiesNear(u, e.Cell, 1)) { StunUnit(x, 1); GalaxyHit(u, x, v * 0.25); }
            } break;
            case "rupture": foreach (var e in EnemiesNear(u, target.Cell, 2)) { StunUnit(e, Stage(1.5, 1.75, 2.5)); GalaxyHit(u, e, v); } break;
            case "chronobreak": u.InvulnerableUntil = Math.Max(u.InvulnerableUntil, Time + 0.6); foreach (var e in GalaxyEnemies(u)) { StunUnit(e, 1.25); GalaxyStrike(u, e, 0.5); GalaxyHit(u, e, v); } break;
            case "gale": foreach (var e in LineTargets(u, target)) { StunUnit(e, 1.5); GalaxyHit(u, e, v * 2.5); } foreach (var a in units.Where(a => a.Alive && a.Team == u.Team && a != u && !a.Galaxies.Piloting).OrderBy(a => a.Cell.DistanceTo(u.Cell)).ThenBy(a => a.Id).Take(3)) Buff(a, CombatStat.AttackSpeed, v / 100, 5); break;
            case "polymorph": foreach (var e in GalaxyEnemies(u).OrderBy(e => e.Cell.DistanceTo(u.Cell)).ThenBy(e => e.Id).Take((int)Stage(3, 5, 8))) { double duration = Stage(3, 3, 5); StunUnit(e, duration); e.Galaxies.HexUntil = Time + duration; e.Galaxies.HexAmplification = v / 1000; } break;
            case "bullettime": { var at = target.Cell; void Wave() => GalaxyCasting(u, () => { foreach (var e in EnemiesNear(u, at, 2)) GalaxyHit(u, e, e.MaxHealth * v / 300); }); Wave(); Schedule(0.5, Wave, u); Schedule(1, Wave, u); break; }
            case "lantern": {
                var fallen = units.FirstOrDefault(a => a.Team == u.Team && !a.Alive && !a.Summoned && !a.Galaxies.Piloting && !a.Galaxies.Lanterned);
                if (fallen is not null && s.Lanterns < 2 && FreeNeighbor(u, u.Cell) is { } cell) {
                    s.Lanterns++; fallen.Galaxies.Lanterned = true; fallen.Alive = true; PlaceFromHook(fallen, cell);
                    fallen.Health = fallen.MaxHealth * 0.4; fallen.Mana = Math.Min(fallen.MaxMana - 1, 40); fallen.Target = null; fallen.Stun = 0; fallen.BurnSource = null; fallen.BurnUntil = 0; fallen.BurnAccumulated = 0;
                } else { var ally = units.Where(a => a.Alive && a.Team == u.Team && !a.Galaxies.Piloting).OrderBy(a => a.Health / a.MaxHealth).FirstOrDefault(); if (ally is not null) AddShield(ally, v); }
                break;
            }
            default: return false;
        }
        return true;
    }
}
