// Galaxies behavior: trait hooks, the set's ability types, item and augment hooks, consumables and the Galaxy
// mechanic (one rule per game, picked from the seed). Hook contexts are documented in docs/sets/FORMAT.md.
// Rules code only: deterministic (c.rng() / game rng), no DOM, exact math.
(function (G) {
  const DT = G.DT;
  const COLS = DT.COLS;

  // ---------- shared helpers (combat) ----------
  const hit = (c, u, e, v) => (e && e.alive ? c.abilityHit(u, e, v) : 0);
  const beam = (c, u, e) => c.event({ type: 'beam', from: u.id, to: e.id });
  const shot = (c, u, e, big) => c.event({ type: 'shot', from: u.id, to: e.id, big: !!big });
  const burst = (c, at, radius, team) => c.event({ type: 'burst', c: at.c, r: at.r, radius, team });
  const enemiesNear = (c, u, at, radius) => c.enemiesNear(u.team, at, radius);
  const alive = (list) => list.filter((x) => x && x.alive);
  // a shield that fades after dur seconds (what is left of it)
  function tempShield(c, u, amt, dur) {
    if (!(amt > 0)) return;
    c.shield(u, amt);
    c.after(dur, () => { u.shield = Math.max(0, u.shield - Math.min(u.shield, amt)); }, u);
  }
  // a basic-attack-like physical strike (crit roll, no on-attack effects)
  function strike(c, u, e, mult = 1) {
    if (!e || !e.alive) return 0;
    let d = u.ad * mult;
    let crit = false;
    if (u.crit > 0 && c.rng() < u.crit) { d *= u.critDmg; crit = true; }
    return c.damage(u, e, d, 'phys', crit);
  }
  // move u next to e (toward `toward`); returns true if it moved
  function blinkTo(c, u, e, toward) {
    if (!e || c.dist(u, e) <= 1) return false;
    const cell = c.freeCellNear(e, toward || u);
    if (!cell) return false;
    return c.move(u, cell.c, cell.r, 0.2, { leap: true });
  }
  // raise the mana cost of e's next ability (Hackers, Still Shroud, EMP); restored when it casts
  function tax(e, f) {
    if (!e || !e.alive || e.state.galTax || !(e.maxMana > 0) || e.maxMana > 500) return;
    e.state.galTax = e.maxMana;
    e.maxMana = Math.round(e.maxMana * (1 + f));
  }
  // ability damage dealt later (delayed blasts) still counts as ability damage for Bombers and Trap Claw
  function casting(u, fn) { u.state.casting = true; try { fn(); } finally { u.state.casting = false; } }
  const isSpell = (src) => !!src && ((src.lock >= 0.34 && src.mana < 1) || src.state.casting);
  const S = (stage, a) => a[Math.min(stage, a.length - 1)];

  // ---------- ability types ----------
  // base: value at tier 1, first form, 100 AP (scaled x1.7 / x3 by form and +25% per cost tier). v = scaled value.
  const abilities = {
    bomb: { base: 210, text: (l, s, v) => `Throws a bomb at its target: ${v} magic damage to it and enemies next to it.`,
      cast(c, u, tg, v) { shot(c, u, tg, true); burst(c, tg, 1, u.team); for (const e of enemiesNear(c, u, tg, 1)) hit(c, u, e, v); } },
    snipe: { base: 370, text: (l, s, v) => `Shoots the farthest enemy for ${v} magic damage.`,
      cast(c, u, tg, v) { const e = c.target(u, 'farthest') || tg; if (!e) return; beam(c, u, e); hit(c, u, e, v); } },
    riposte: { base: 190, text: (l, s, v) => `Blocks all damage for 1 second, then strikes the nearest enemy for ${v} magic damage and stuns it for ${S(s, [1.5, 1.5, 2.5])}s.`,
      cast(c, u, tg, v) {
        u.invulnUntil = Math.max(u.invulnUntil, c.t + 1);
        c.text(u, 'Riposte');
        c.event({ type: 'shield', at: u.id });
        c.after(1, () => { const e = c.target(u, 'closest'); if (!e) return; c.event({ type: 'hit', from: u.id, to: e.id, big: true }); c.stun(e, S(u.stage, [1.5, 1.5, 2.5])); hit(c, u, e, v); }, u);
      } },
    smoke: { base: 150, text: (l, s, v) => `Throws a smoke bomb at its target: ${v} magic damage to it and enemies next to it, and their damage drops 50% for ${3 + s}s.`,
      cast(c, u, tg, v) {
        burst(c, tg, 1, u.team);
        for (const e of enemiesNear(c, u, tg, 1)) { hit(c, u, e, v); if (e.alive) c.weaken(e, 0.5, 3 + u.stage); }
      } },
    sap: { base: 160, text: (l, s, v) => `Slams a tentacle in a line: ${v} magic damage to each enemy hit, stealing 30% of their armor and magic resist for 6s.`,
      cast(c, u, tg, v) {
        for (const e of c.lineTargets(u, tg)) {
          beam(c, u, e);
          hit(c, u, e, v);
          if (!e.alive) continue;
          const a = Math.max(0, e.armor) * 0.3, m = Math.max(0, e.mr) * 0.3;
          c.buff(e, 'armor', -a, 6); c.buff(e, 'mr', -m, 6); c.buff(u, 'armor', a, 6); c.buff(u, 'mr', m, 6);
        }
      } },
    isolate: { base: 200, text: (l, s, v) => `Slashes the nearest enemy for ${v} magic damage, or ${Math.round(v * 2.5)} if no ally stands next to it.`,
      cast(c, u, tg, v) {
        const e = c.target(u, 'closest') || tg;
        if (!e) return;
        const lone = !c.adjacent(e).some((a) => a !== e && a.alive && a.team === e.team);
        c.event({ type: 'hit', from: u.id, to: e.id, big: true });
        if (lone) c.text(e, 'Isolated');
        hit(c, u, e, lone ? v * 2.5 : v);
      } },
    toss: { base: 120, text: (l, s, v) => `Throws a buckler at the farthest enemy for ${v} magic damage. It bounces back as a ${Math.round(v * 1.6)} shield.`,
      cast(c, u, tg, v) { const e = c.target(u, 'farthest') || tg; if (!e) return; beam(c, u, e); hit(c, u, e, v); c.shield(u, v * 1.6); } },
    plumage: { base: 125, text: (l, s, v) => `Gains ${v}% attack speed for 8s.`,
      cast(c, u, tg, v) { c.buff(u, 'as', v / 100, 8); c.text(u, 'Plumage'); } },
    bubble: { base: 185, text: (l, s, v) => `Kicks a bubble at the healthiest enemy: ${v} magic damage and a ${S(s, [2, 2.25, 3])}s stun.`,
      cast(c, u, tg, v) {
        const list = c.enemiesOf(u);
        if (!list.length) return;
        const e = list.reduce((b, x) => (x.hp / x.maxHp > b.hp / b.maxHp + 1e-9 || (Math.abs(x.hp / x.maxHp - b.hp / b.maxHp) < 1e-9 && x.maxHp > b.maxHp) ? x : b));
        beam(c, u, e); c.stun(e, S(u.stage, [2, 2.25, 3])); hit(c, u, e, v);
      } },
    orb: { base: 170, text: (l, s, v) => `Fires an orb through its target: ${v} magic damage on the way out and ${Math.round(v * 0.75)} true damage on the way back.`,
      cast(c, u, tg, v) {
        const hits = c.lineTargets(u, tg);
        for (const e of hits) { beam(c, u, e); hit(c, u, e, v); }
        c.after(0.5, () => { for (const e of alive(hits)) c.damage(u, e, v * 0.75, 'true'); }, u);
      } },
    fireshield: { base: 160, text: (l, s, v) => `Blasts its target and enemies next to it for ${v} magic damage, then gains a ${Math.round(v * 1.8)} shield for 4s.`,
      cast(c, u, tg, v) { burst(c, tg, 1, u.team); for (const e of enemiesNear(c, u, tg, 1)) hit(c, u, e, v); tempShield(c, u, v * 1.8, 4); } },
    grab: { base: 220, text: (l, s, v) => `Pulls the farthest enemy next to it, dealing ${v} magic damage and stunning it for 2s. Nearby allies attack that enemy.`,
      cast(c, u, tg, v) {
        const e = c.target(u, 'farthest') || tg;
        if (!e) return;
        if (c.dist(u, e) > 1) { const cell = c.freeCellNear(u, e); if (cell) c.move(e, cell.c, cell.r, 0.25, { leap: true }); }
        beam(c, u, e); c.stun(e, 2); hit(c, u, e, v);
        if (e.alive) for (const a of c.alliesOf(u)) if (c.dist(a, e) <= a.range) a.target = e;
      } },
    dunk: { base: 220, text: (l, s, v) => `Dunks its target for ${v} magic damage, doubled below 50% HP. A takedown lets it cast again at once.`,
      cast(c, u, tg, v) {
        const low = tg.hp < tg.maxHp * 0.5;
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        hit(c, u, tg, low ? v * 2 : v);
        if (!tg.alive && (u.state.dunks = (u.state.dunks || 0) + 1) <= 4) u.mana = u.maxMana;
      } },
    frenzy: { base: 4, text: (l, s, v) => `For 4s: +6 range, 60% attack speed, and basic attacks deal ${v}% of the target's max HP as magic damage.`,
      cast(c, u, tg, v) {
        if (!u.state.frenzy) u.state.frRange = u.range;
        u.state.frenzy = { until: c.t + 4, pct: v / 100 };
        u.range = u.state.frRange + 6;
        c.buff(u, 'as', 0.6, 4);
        c.text(u, 'Frenzy');
      } },
    dashshot: { base: 170, text: (l, s, v) => `Fires twice at its target: a basic attack and a shot for ${v} magic damage.`,
      cast(c, u, tg, v) { shot(c, u, tg); strike(c, u, tg); shot(c, u, tg, true); hit(c, u, tg, v); } },
    dreadshield: { base: 135, text: (l, s, v) => `Gains a ${v * 2} shield for 8s. While it holds, deals ${Math.round(v * 0.25)} magic damage per second to adjacent enemies.`,
      cast(c, u, tg, v) { tempShield(c, u, v * 2, 8); u.state.dread = { until: c.t + 8, dps: v * 0.25 }; } },
    crater: { base: 200, text: (l, s, v) => `Erupts the ground under its target: ${v} magic damage and a ${S(s, [2, 2.5, 3])}s stun. Adjacent enemies take half and a 1s stun.`,
      cast(c, u, tg, v) {
        burst(c, tg, 1, u.team);
        for (const e of enemiesNear(c, u, tg, 1)) { const main = e === tg; c.stun(e, main ? S(u.stage, [2, 2.5, 3]) : 1); hit(c, u, e, main ? v : v / 2); }
      } },
    entrance: { base: 180, text: (l, s, v) => `Dashes to the farthest enemy within 3 hexes, dealing ${v} magic damage to it and enemies next to it and stunning them for 1.25s.`,
      cast(c, u, tg, v) {
        const near = c.enemiesOf(u).filter((e) => c.dist(u, e) <= 3);
        const e = near.length ? near.reduce((b, x) => (c.dist(u, x) > c.dist(u, b) ? x : b)) : tg;
        blinkTo(c, u, e);
        burst(c, e, 1, u.team);
        for (const x of enemiesNear(c, u, e, 1)) { c.stun(x, 1.25); hit(c, u, x, v); }
      } },
    triple: { base: 160, text: (l, s, v) => `Strikes its target three times. The third strike deals ${v} more magic damage and stuns for 1.5s.`,
      cast(c, u, tg, v) {
        for (let k = 0; k < 3 && tg.alive; k++) { c.event({ type: 'hit', from: u.id, to: tg.id, big: k === 2 }); strike(c, u, tg); }
        if (tg.alive) { c.stun(tg, 1.5); hit(c, u, tg, v); }
      } },
    lastbreath: { base: 165, text: (l, s, v) => `Blinks to the farthest enemy within 3 hexes, stuns it for 1s and strikes it ${3 + s} times, plus ${v} magic damage.`,
      cast(c, u, tg, v) {
        const near = c.enemiesOf(u).filter((e) => c.dist(u, e) <= 3);
        const e = near.length ? near.reduce((b, x) => (c.dist(u, x) > c.dist(u, b) ? x : b)) : tg;
        blinkTo(c, u, e);
        u.target = e;
        c.stun(e, 1);
        for (let k = 0; k < 3 + u.stage && e.alive; k++) { c.event({ type: 'hit', from: u.id, to: e.id, big: true }); strike(c, u, e); }
        hit(c, u, e, v);
      } },
    contempt: { base: 230, text: (l, s, v) => `Strikes its target for ${v} magic damage and steals 25% of its attack damage for the rest of combat.`,
      cast(c, u, tg, v) {
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        hit(c, u, tg, v);
        if (tg.alive) { const s = tg.ad * 0.25; c.buff(tg, 'ad', -s); c.buff(u, 'ad', s); }
      } },
    arrow: { base: 160, text: (l, s, v) => `Fires an arrow at the farthest enemy that explodes for ${v} magic damage, stunning enemies in the blast for 1.5s.`,
      cast(c, u, tg, v) { const e = c.target(u, 'farthest') || tg; if (!e) return; beam(c, u, e); burst(c, e, 1, u.team); for (const x of enemiesNear(c, u, e, 1)) { c.stun(x, 1.5); hit(c, u, x, v); } } },
    meep: { base: 265, text: (l, s, v) => `Hits its target for ${v} magic damage and stuns it for 1s. Each cast leaves a meep that pays 1 gold after combat (up to 2).`,
      cast(c, u, tg, v) { beam(c, u, tg); c.stun(tg, 1); hit(c, u, tg, v); const fx = c.teamState(u.team); fx.meep = (fx.meep || 0) + 1; } },
    poison: { base: 270, text: (l, s, v) => `Poisons the nearest enemy that is not poisoned yet: ${v * 2} magic damage over 10s, and its healing is cut by a third.`,
      cast(c, u, tg, v) {
        const list = c.enemiesOf(u);
        if (!list.length) return;
        const fresh = list.filter((e) => !(e.state.galPoison && e.state.galPoison.until > c.t));
        const pool = fresh.length ? fresh : list;
        const e = pool.reduce((b, x) => (c.dist(u, x) < c.dist(u, b) ? x : b));
        beam(c, u, e);
        e.state.galPoison = { src: u, dps: v / 5, until: c.t + 10, next: c.t + 1 };
        e.woundUntil = Math.max(e.woundUntil || 0, c.t + 10);
      } },
    emp: { base: 150, text: (l, s, v) => `Fires a pulse at a random enemy: ${v} magic damage to it and enemies next to it, and their next ability costs 40% more mana.`,
      cast(c, u, tg, v) { const e = c.target(u, 'random') || tg; if (!e) return; beam(c, u, e); burst(c, e, 1, u.team); for (const x of enemiesNear(c, u, e, 1)) { hit(c, u, x, v); tax(x, 0.4); } } },
    chosen: { base: 70, text: (l, s, v) => `For 5s: +30% attack speed, heals 6% of max HP per second, and basic attacks deal ${v} more true damage.`,
      cast(c, u, tg, v) { u.state.chosen = { until: c.t + 5, bonus: v, next: c.t + 1 }; c.buff(u, 'as', 0.3, 5); c.text(u, 'Chosen'); } },
    flame: { base: 150, text: (l, s, v) => `Torches its target and enemies next to it for ${v} magic damage and burns them (healing cut by a third).`,
      cast(c, u, tg, v) { burst(c, tg, 1, u.team); for (const e of enemiesNear(c, u, tg, 1)) { hit(c, u, e, v); if (e.alive) c.burn(u, e); } } },
    backstab: { base: 58, text: (l, s, v) => `Teleports behind its target and backstabs it: a critical strike for 200% attack damage plus ${v} magic damage.`,
      cast(c, u, tg, v) {
        blinkTo(c, u, tg, { c: tg.c, r: u.team === 0 ? 0 : c.rows - 1 });
        u.target = tg;
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        c.damage(u, tg, u.ad * 2 * u.critDmg, 'phys', true);
        hit(c, u, tg, v);
      } },
    orbs: { base: 100, text: (l, s, v) => `Fires ${3 + s} orbs at the healthiest enemy, each dealing ${v} magic damage.`,
      cast(c, u, tg, v) {
        const list = c.enemiesOf(u);
        if (!list.length) return;
        const e = list.reduce((b, x) => (x.hp / x.maxHp > b.hp / b.maxHp + 1e-9 ? x : b));
        for (let k = 0; k < 3 + u.stage && e.alive; k++) { shot(c, u, e, true); hit(c, u, e, v); }
      } },
    tumble: { base: 70, text: (l, s, v) => `Tumbles (50% dodge for 2s) and fires a bolt for 160% attack damage plus ${v} true damage.`,
      cast(c, u, tg, v) { c.buff(u, 'dodge', 0.5, 2); shot(c, u, tg, true); strike(c, u, tg, 1.6); if (tg.alive) c.damage(u, tg, v, 'true'); } },
    charge: { base: 170, text: (l, s, v) => `Charges the farthest enemy: ${Math.round(v * 0.4)} magic damage to enemies in the way, then ${v} magic damage and a ${S(s, [2, 2.5, 3])}s stun to the target.`,
      cast(c, u, tg, v) {
        const e = c.target(u, 'farthest') || tg;
        if (!e) return;
        for (const x of c.lineTargets(u, e)) if (x !== e) hit(c, u, x, v * 0.4);
        blinkTo(c, u, e);
        u.target = e;
        c.event({ type: 'hit', from: u.id, to: e.id, big: true });
        c.stun(e, S(u.stage, [2, 2.5, 3]));
        hit(c, u, e, v);
      } },
    shark: { base: 150, text: (l, s, v) => `Throws a lure at the farthest enemy. After 0.6s a shark bursts up: ${v} magic damage to enemies there and a 1.5s stun.`,
      cast(c, u, tg, v) {
        const e = c.target(u, 'farthest') || tg;
        if (!e) return;
        beam(c, u, e);
        const at = { c: e.c, r: e.r };
        c.after(0.6, () => casting(u, () => { burst(c, at, 1, u.team); for (const x of enemiesNear(c, u, at, 1)) { c.stun(x, 1.5); hit(c, u, x, v); } }), u);
      } },
    transform: { base: 92, text: (l, s, v) => `The first cast digivolves it for the rest of combat: +${v * 2} HP, +${Math.round(v * 0.25)} AD, melee range. Every cast leaps at its target: ${v} magic damage and a 1.5s stun to enemies next to it.`,
      cast(c, u, tg, v) {
        if (!u.state.mega) {
          u.state.mega = true;
          u.maxHp += v * 2; u.hp += v * 2; u.ad += v * 0.25; u.range = 1;
          c.text(u, 'Digivolve!');
        }
        blinkTo(c, u, tg);
        u.target = tg;
        burst(c, tg, 1, u.team);
        for (const x of enemiesNear(c, u, tg, 1)) { c.stun(x, 1.5); hit(c, u, x, v); }
      } },
    surge: { base: 120, text: (l, s, v) => `Dashes through its target for 150% attack damage plus ${v} magic damage. A takedown lets it dash again at the enemy with the most mana.`,
      cast(c, u, tg, v) {
        let e = tg;
        for (let k = 0; k < 4 && e; k++) {
          blinkTo(c, u, e);
          u.target = e;
          c.event({ type: 'hit', from: u.id, to: e.id, big: true });
          strike(c, u, e, 1.5);
          hit(c, u, e, v);
          if (e.alive) break;
          const list = c.enemiesOf(u);
          e = list.length ? list.reduce((b, x) => (x.mana > b.mana ? x : b)) : null;
        }
      } },
    whisper: { base: 80, text: (l, s, v) => `Every fourth shot is a whisper: ${S(s, [300, 340, 450])}% attack damage plus ${v} magic damage.`,
      cast(c, u, tg, v) { shot(c, u, tg, true); strike(c, u, tg, S(u.stage, [3, 3.4, 4.5])); hit(c, u, tg, v); } },
    excited: { base: 250, text: (l, s, v) => `Fires a rocket: ${v} magic damage to its target and enemies next to it. After its first takedown in a combat it gains 50% attack speed.`,
      cast(c, u, tg, v) { shot(c, u, tg, true); burst(c, tg, 1, u.team); for (const e of enemiesNear(c, u, tg, 1)) hit(c, u, e, v); } },
    energyslash: { base: 110, text: (l, s, v) => `Gains a ${Math.round(v * 1.2)} shield for 3s and slashes enemies next to it for ${v} magic damage. Every third cast sends a wave through its target for ${v * 2}.`,
      cast(c, u, tg, v) {
        u.state.rs = (u.state.rs || 0) + 1;
        tempShield(c, u, v * 1.2, 3);
        if (u.state.rs % 3 === 0) for (const e of c.lineTargets(u, tg)) { beam(c, u, e); hit(c, u, e, v * 2); }
        else { burst(c, u, 1, u.team); for (const e of enemiesNear(c, u, u, 1)) hit(c, u, e, v); }
      } },
    wish: { base: 150, text: (l, s, v) => `Heals every ally for ${v}.`,
      cast(c, u, tg, v) { for (const a of c.survivors(u.team)) if (!a.state.piloting) c.heal(a, v); } },
    traps: { base: 110, text: (l, s, v) => `Drops 3 traps near random enemies. After 1.2s each explodes for ${v} magic damage to enemies within 1 hex and slows their attacks 30% for 3s.`,
      cast(c, u, tg, v) {
        const list = c.enemiesOf(u);
        if (!list.length) return;
        const spots = [];
        for (let k = 0; k < 3; k++) { const e = list[Math.floor(c.rng() * list.length)]; spots.push({ c: e.c, r: e.r }); shot(c, u, e); }
        c.after(1.2, () => casting(u, () => {
          for (const at of spots) { burst(c, at, 1, u.team); for (const x of enemiesNear(c, u, at, 1)) { hit(c, u, x, v); if (x.alive) c.buff(x, 'as', -0.3, 3); } }
        }), u);
      } },
    deathray: { base: 230, text: (l, s, v) => `Burns a line through its target for ${v} magic damage. One second later the line explodes for ${Math.round(v * 0.8)} more.`,
      cast(c, u, tg, v) {
        const hits = c.lineTargets(u, tg);
        for (const e of hits) { beam(c, u, e); hit(c, u, e, v); }
        c.after(1, () => casting(u, () => { for (const e of alive(hits)) hit(c, u, e, v * 0.8); }), u);
      } },
    ray: { base: 250, text: (l, s, v) => `Channels a ray through its target: three pulses of ${Math.round(v * 0.5)} magic damage to every enemy in the line.`,
      cast(c, u, tg, v) {
        const aim = { c: tg.c, r: tg.r };
        const pulse = () => casting(u, () => {
          const t = alive([c.unitAt(aim.c, aim.r)])[0] || c.target(u, 'closest');
          if (!t) return;
          for (const e of c.lineTargets(u, t)) { beam(c, u, e); hit(c, u, e, v * 0.5); }
        });
        pulse(); c.after(0.4, pulse, u); c.after(0.8, pulse, u);
      } },
    cyclone: { base: 240, text: (l, s, v) => `Spins for 3s, dealing ${v} magic damage over the spin to adjacent enemies. Each enemy is stunned for 1.5s the first time it is hit.`,
      cast(c, u, tg, v) { u.state.cyc = { until: c.t + 3, dmg: v / 6, next: c.t, hit: {} }; c.text(u, 'Cyclone'); } },
    fleet: { base: 78, text: (l, s, v) => `Launches ${3 + 2 * s} fighters at random enemies, each dealing ${v} magic damage.`,
      cast(c, u, tg, v) {
        for (let k = 0; k < 3 + 2 * u.stage; k++) {
          const list = c.enemiesOf(u);
          if (!list.length) break;
          const e = list[Math.floor(c.rng() * list.length)];
          shot(c, u, e, true);
          hit(c, u, e, v);
        }
      } },
    rupture: { base: 120, text: (l, s, v) => `Ruptures a large area around its target: ${v} magic damage and a ${S(s, [1.5, 1.75, 2.5])}s stun to enemies within 2 hexes.`,
      cast(c, u, tg, v) { burst(c, tg, 2, u.team); for (const e of enemiesNear(c, u, tg, 2)) { c.stun(e, S(u.stage, [1.5, 1.75, 2.5])); hit(c, u, e, v); } } },
    chronobreak: { base: 70, text: (l, s, v) => `Stops time: every enemy is stunned for 1.25s and takes 50% attack damage plus ${v} magic damage. It cannot be hurt for 0.6s.`,
      cast(c, u, tg, v) {
        u.invulnUntil = Math.max(u.invulnUntil, c.t + 0.6);
        c.text(u, 'Time stop');
        for (const e of c.enemiesOf(u)) { c.event({ type: 'beam', from: u.id, to: e.id }); c.stun(e, 1.25); strike(c, u, e, 0.5); hit(c, u, e, v); }
      } },
    orbital: { base: 260, text: (l, s, v) => `Calls an orbital strike on its target's area. After 1.5s, enemies within 2 hexes take ${v} magic damage.`,
      cast(c, u, tg, v) {
        const at = { c: tg.c, r: tg.r };
        c.text(tg, 'Incoming');
        c.after(1.5, () => casting(u, () => { burst(c, at, 2, u.team); for (const e of enemiesNear(c, u, at, 2)) hit(c, u, e, v); }), u);
      } },
    gale: { base: 40, text: (l, s, v) => `Sends a gale through its target: enemies in the line take ${Math.round(v * 2.5)} magic damage and are stunned for 1.5s. The 3 nearest allies gain ${v}% attack speed for 5s.`,
      cast(c, u, tg, v) {
        for (const e of c.lineTargets(u, tg)) { beam(c, u, e); c.stun(e, 1.5); hit(c, u, e, v * 2.5); }
        const allies = c.alliesOf(u).filter((a) => !a.state.piloting).sort((a, b) => c.dist(u, a) - c.dist(u, b) || a.id - b.id).slice(0, 3);
        for (const a of allies) { c.buff(a, 'as', v / 100, 5); c.event({ type: 'shield', at: a.id }); }
      } },
    polymorph: { base: 50, text: (l, s, v) => `Turns the ${S(s, [3, 5, 8])} nearest enemies into harmless critters for ${S(s, [3, 3, 5])}s: they cannot act and take ${Math.round(v / 10)}% more damage.`,
      cast(c, u, tg, v) {
        const list = c.enemiesOf(u).sort((a, b) => c.dist(u, a) - c.dist(u, b) || a.id - b.id).slice(0, S(u.stage, [3, 5, 8]));
        const dur = S(u.stage, [3, 3, 5]);
        for (const e of list) { beam(c, u, e); c.stun(e, dur); e.state.hex = { until: c.t + dur, amp: v / 1000 }; c.text(e, 'Critter'); }
      } },
    bullettime: { base: 20, text: (l, s, v) => `Fires waves of arrows at its target's area: enemies within 2 hexes take ${v}% of their max HP as magic damage over 1.5s.`,
      cast(c, u, tg, v) {
        const at = { c: tg.c, r: tg.r };
        const wave = () => casting(u, () => { for (const e of enemiesNear(c, u, at, 2)) { shot(c, u, e); hit(c, u, e, (e.maxHp * v) / 300); } });
        wave(); c.after(0.5, wave, u); c.after(1, wave, u);
      } },
    lantern: { base: 260, text: (l, s, v) => `Pulls the first fallen ally back into the fight with 40% HP and 40 mana (twice per combat). With no one to save, shields the weakest ally for ${v}.`,
      cast(c, u, tg, v) {
        const fallen = c.units.filter((a) => a.team === u.team && !a.alive && !a.summoned && !a.state.piloting && !a.state.lanterned);
        if (fallen.length && (u.state.lanterns || 0) < 2) {
          const a = fallen[0];
          const cell = c.freeCellNear(u, u);
          if (cell) {
            u.state.lanterns = (u.state.lanterns || 0) + 1;
            a.state.lanterned = true;
            a.alive = true;
            c.place(a, cell.c, cell.r);
            a.hp = a.maxHp * 0.4; a.mana = Math.min(a.maxMana - 1, 40); a.target = null; a.stun = 0; a.burn = null;
            c.event({ type: 'heal', at: a.id, amount: Math.round(a.hp) });
            c.text(a, 'Rescued');
            return;
          }
        }
        const allies = c.survivors(u.team).filter((a) => !a.state.piloting);
        if (!allies.length) return;
        const w = allies.reduce((b, x) => (x.hp / x.maxHp < b.hp / b.maxHp ? x : b));
        c.shield(w, v);
      } },
    reel: { base: 190, text: (l, s, v) => `Hooks the farthest enemy and reels it in, dealing ${v} true damage. An enemy left below 25% HP is finished off; a takedown makes this ability cost 20 less mana.`,
      cast(c, u, tg, v) {
        const e = c.target(u, 'farthest') || tg;
        if (!e) return;
        if (c.dist(u, e) > 1) { const cell = c.freeCellNear(u, e); if (cell) c.move(e, cell.c, cell.r, 0.3, { leap: true }); }
        beam(c, u, e);
        c.damage(u, e, v, 'true');
        if (e.alive && e.hp < e.maxHp * 0.25) { c.text(e, 'Finished'); c.damage(u, e, e.hp + 1, 'true'); }
        if (!e.alive) u.maxMana = Math.max(30, u.maxMana - 20);
      } },
    meteors: { base: 100, text: (l, s, v) => `Calls ${4 + s} meteors on random enemies, each dealing ${v} magic damage. A meteor that knocks out its target stuns the enemies next to it for 1s.`,
      cast(c, u, tg, v) {
        for (let k = 0; k < 4 + u.stage; k++) {
          const list = c.enemiesOf(u);
          if (!list.length) break;
          const e = list[Math.floor(c.rng() * list.length)];
          shot(c, u, e, true);
          hit(c, u, e, v);
          if (!e.alive) for (const x of enemiesNear(c, u, e, 1)) { c.stun(x, 1); hit(c, u, x, v * 0.25); }
        }
      } },
  };

  // ---------- traits ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });

  function eject(c, host) {
    const pilots = host.state.xros || [];
    host.state.xros = null;
    c.text(host, 'Eject');
    for (const p of pilots) {
      const cell = c.unitAt(host.c, host.r) ? c.freeCellNear(host, host) : { c: host.c, r: host.r };
      if (!cell) continue;
      p.state.piloting = null;
      p.alive = true;
      c.place(p, cell.c, cell.r);
      p.hp = p.maxHp * 0.6; p.mana = 0; p.target = null;
      c.event({ type: 'leap', at: p.id, fc: host.c, fr: host.r });
    }
  }

  function overload(c, u) {
    const s = u.state;
    if (!s.ov) return;
    s.ovN = (s.ovN || 0) + 1;
    if (s.ovN < 10) return;
    s.ovN = 0;
    if (u.hp < u.maxHp * 0.5) { c.heal(u, s.ov); c.text(u, 'Overload'); return; }
    const e = c.target(u, 'closest');
    if (!e) return;
    beam(c, u, e);
    c.damage(u, e, s.ov, 'magic');
  }

  const traits = {
    DD: {
      hooks: {
        onCast(c, e) {
          if (!c.isMember(e.unit)) return;
          const others = c.survivors(c.team).filter((a) => a !== e.unit && a.traits.includes('DD') && !a.state.piloting);
          if (!others.length) return;
          const share = c.v / others.length;
          for (const a of others) a.mana = Math.min(a.maxMana, a.mana + share);
        },
      },
    },
    DA: {
      hooks: {
        onDeath(c, e) {
          if (!c.isMember(e.unit)) return;
          for (const a of c.survivors(c.team)) {
            if (a === e.unit || !a.traits.includes('DA')) continue;
            a.ad += (a.baseAd * c.v) / 100;
            a.ap += c.v;
            c.text(a, 'Dark power');
          }
        },
      },
    },
    XR: {
      hooks: {
        onCombatReady: {
          order: 2,
          fn(c) {
            const mem = c.mine.filter((u) => u.alive && !u.summoned && u.traits.includes('XR'))
              .sort((a, b) => b.stage - a.stage || b.line.tier - a.line.tier || b.items.length - a.items.length || a.id - b.id);
            if (mem.length < 3) return;
            const [host, ...pilots] = mem.slice(0, 3);
            const hp = host.maxHp + pilots.reduce((s, p) => s + p.maxHp, 0);
            const ad = host.ad + pilots.reduce((s, p) => s + p.ad, 0);
            host.maxHp = Math.round(hp * c.v);
            host.ad = ad * c.v * 0.8;
            host.armor += 20; host.mr += 20;
            host.state.xros = pilots;
            for (const p of pilots) {
              p.state.piloting = host;
              c.remove(p);
            }
            c.text(host, 'DigiXros!');
          },
        },
        onDeath(c, e) { if (e.unit.state.xros) eject(c, e.unit); },
      },
    },
    PI: {
      hooks: {
        onKill(c, e) { if (c.isMember(e.unit) && !e.victim.summoned) { const fx = c.teamState(c.team); fx.pirate = (fx.pirate || 0) + 1; } },
        // game hook: loot after a PvP combat
        onRoundEnd(c, e) {
          if (e.pve || !e.match || !e.match.combat) return;
          const n = Math.min(6, e.match.combat.teamFx[e.team].pirate || 0);
          for (let k = 0; k < n; k++) {
            if (c.rng() < c.v) e.gold += 1;
            const comp = c.val('comp');
            if (comp && c.rng() < comp) c.give(c.randomComponent());
          }
        },
      },
      ai: { bonus: (a) => (a.stage <= 4 ? 1 : 0) },
    },
    CE: { hooks: all((u, c) => { u.omnivamp += c.v; }) },
    TK: {
      hooks: {
        onCombatStart(c) { c.state.next = c.v; },
        onTick(c) {
          if (c.t < c.state.next) return;
          c.state.next += c.v;
          for (const u of c.survivors(c.team)) u.buffs.push({ stat: 'as', v: 0.12, until: Infinity });
        },
      },
    },
    CY: { hooks: each((u, c) => { const k = u.items.length ? 2 : 1; u.maxHp += c.v * k; u.ad += u.baseAd * c.val('ad') * k; }) },
    RE: {
      hooks: each((u, c) => {
        const n = c.adjacent(u).filter((a) => a !== u && a.team === u.team && a.traits.includes('RE')).length;
        if (!n) return;
        u.shield += c.v * n;
        u.amp += c.val('amp') * n;
      }),
    },
    AB: {
      hooks: {
        onBeforeDamage(c, e) {
          if (e.isBurn || !c.isMember(e.unit)) return;
          const tg = e.target;
          const res = e.kind === 'phys' ? Math.max(0, tg.armor) : e.kind === 'magic' ? Math.max(0, tg.mr) : 0;
          if (res) e.mult *= (100 + res) / (100 + res * (1 - c.v));
        },
      },
    },
    VA: {
      hooks: {
        onBeforeDamage(c, e) {
          if (e.crit || e.isBurn || !c.isMember(e.unit)) return;
          if (e.target.hp < e.target.maxHp * c.v) e.mult *= e.unit.critDmg;
        },
        onCombatStart(c) { for (const u of c.members) u.crit += c.val('crit'); },
      },
    },
    AS: { hooks: each((u, c) => { u.maxMana = Math.max(20, u.maxMana - c.v); }) },
    OV: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.ov = c.v; u.state.ovN = 0; } },
        onDamageDealt(c, e) { if (!e.isBurn && e.unit.state.ov && e.unit.alive) overload(c, e.unit); },
        onDamageTaken(c, e) { if (!e.isBurn && e.unit.state.ov && e.unit.alive) overload(c, e.unit); },
      },
    },
    ARK: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.ark = c.v; u.as = 0.0001; u.P.ccImmune = 999; } },
        onTick(c, e) { for (const u of c.members) if (u.alive && u.state.ark) u.mana = Math.min(u.maxMana, u.mana + u.state.ark * e.dt); },
      },
    },
    SV: {
      hooks: {
        onBeforeDamage(c, e) {
          if (e.kind !== 'phys' || e.isBurn) return;
          const tg = e.target;
          const arm = 100 + Math.max(0, tg.armor);
          e.mult *= e.unit.traits.includes('DD') ? arm / 100 : arm / (100 + Math.max(0, tg.mr));
        },
      },
    },
    BH: {
      hooks: {
        onCombatStart(c) {
          const side = c.side(c.team);
          const b = (side && side.mech && side.mech.bounty) || {};
          for (const u of c.members) { const n = Math.min(9, b[u.line.id] || 0); u.ap += c.v * n; u.ad += u.baseAd * 0.03 * n; }
        },
      },
    },
    BM: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit, tg = e.target;
          if (!c.isMember(u) || !tg.alive || c.rng() >= c.v) return;
          for (let k = 0; k < 2 && tg.alive; k++) { c.event({ type: 'hit', from: u.id, to: tg.id }); strike(c, u, tg); }
        },
      },
    },
    GU: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          u.state.guN = (u.state.guN || 0) + 1;
          if (u.state.guN % 4 !== 0) return;
          for (let k = 0; k < c.v; k++) {
            const list = c.enemiesOf(u);
            if (!list.length) break;
            const x = list[Math.floor(c.rng() * list.length)];
            shot(c, u, x);
            strike(c, u, x);
          }
        },
      },
    },
    BR: { hooks: each((u, c) => { u.maxHp += c.v; }) },
    BO: {
      hooks: {
        onDamageDealt(c, e) {
          const u = e.unit, tg = e.target;
          if (!c.isMember(u) || e.kind === 'phys' || e.isBurn || !tg.alive || !isSpell(u)) return;
          const tag = `${u.id}@${Math.round(c.t * 30)}`;
          if (tg.state.bombed === tag) return;
          tg.state.bombed = tag;
          c.stun(tg, c.v);
        },
      },
    },
    IN: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.leap = true; u.buffs.push({ stat: 'as', v: c.v, until: 6, inf: true }); } },
        onKill(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          u.buffs = u.buffs.filter((b) => !b.inf);
          u.buffs.push({ stat: 'as', v: c.v, until: c.t + 6, inf: true });
        },
      },
    },
    HA: { hooks: { onAttack(c, e) { if (c.isMember(e.unit)) tax(e.target, c.v); } } },
    MY: { hooks: all((u, c) => { u.mr += c.v; }) },
    PR: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || u.state.prUntil > c.t) return;
          u.state.prUntil = c.t + 4;
          tempShield(c, u, u.maxHp * c.v, 4);
        },
      },
    },
    SN: { hooks: each((u, c) => { u.perHexAmp = c.v; }) },
    SO: { hooks: all((u, c) => { u.ap += c.v; }) },
    VG: { hooks: each((u, c) => { u.armor += c.v; }) },
  };

  // ---------- items ----------
  const sameRow = (c, u, radius) => c.mine.filter((a) => a.r === u.r && c.dist(a, u) <= radius);
  const items = {
    gal_reaper: { hooks: { onKill(c, e) { if (e.unit === c.unit) c.unit.ad += c.unit.baseAd * 0.2 * c.count; } } },
    gal_rebirth: {
      hooks: {
        onLethal(c, e) {
          const u = e.unit;
          if (u !== c.unit || u.state.rebirth) return;
          u.state.rebirth = true;
          u.hp = 1;
          u.invulnUntil = c.t + 1.5;
          c.text(u, 'Rebirth');
          c.after(1.5, () => c.heal(u, u.maxHp * 0.4), u);
        },
      },
    },
    gal_banner: { hooks: { onCombatStart(c, e) { for (const a of sameRow(c, e.unit, 1)) a.as *= 1.3; } } },
    gal_longshot: { hooks: { onCombatStart(c, e) { e.unit.range += 2; } } },
    gal_portal: {
      hooks: {
        onDeath(c, e) {
          const u = e.unit;
          if (u !== c.unit || u.summoned) return;
          const x = c.summon(u.team, 'gal_construct', u.stage, u.c, u.r, { hpMul: 1.25 });
          if (x) c.text(x, 'Construct');
        },
      },
    },
    gal_ultima: { hooks: { onCombatReady: { order: 3, fn(c, e) { e.unit.ap *= 1.3; } } } },
    gal_echo: {
      hooks: {
        onCast(c, e) {
          const u = e.unit, tg = e.target;
          if (u !== c.unit || !tg || !tg.alive) return;
          const near = c.enemiesNear(u.team, tg, 1).filter((x) => x !== tg).sort((a, b) => a.id - b.id).slice(0, 3);
          for (const x of [tg, ...near]) { c.event({ type: 'beam', from: tg.id, to: x.id }); c.damage(u, x, 160, 'magic'); }
        },
      },
    },
    gal_locket: { hooks: { onCombatStart(c, e) { const amt = S(e.unit.stage, [260, 320, 400]); for (const a of sameRow(c, e.unit, 2)) a.shield += amt; } } },
    gal_tome: { hooks: { onCombatStart(c, e) { e.unit.abilBurn = true; } } },
    gal_frostheart: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'galFrost', 1)) return;
          for (const x of c.enemiesNear(u.team, u, 2)) c.buff(x, 'as', -0.3, 1.05);
        },
      },
    },
    gal_chalice: { hooks: { onCombatStart(c, e) { for (const a of sameRow(c, e.unit, 1)) a.ap += 30; } } },
    gal_spikeplate: { hooks: { onBeforeDamageTaken(c, e) { if (e.crit && e.target === c.unit && e.source) e.mult /= e.source.critDmg || 1.6; } } },
    gal_swordbreaker: {
      hooks: {
        onAttack(c, e) {
          if (e.unit !== c.unit || !e.target.alive || c.rng() >= 0.3) return;
          e.target.state.disarmUntil = c.t + 3;
          c.text(e.target, 'Disarmed');
        },
      },
    },
    gal_shroud: { hooks: { onCombatStart(c, e) { for (const x of c.units) if (x.team !== e.unit.team && Math.abs(x.c - e.unit.c) <= 1) tax(x, 0.4); } } },
    gal_dragonscale: { hooks: { onBeforeDamageTaken(c, e) { if (e.kind === 'magic' && e.target === c.unit) e.mult *= 0.6; } } },
    gal_zephyr: {
      hooks: {
        onCombatStart(c, e) {
          const u = e.unit;
          const at = { c: c.cols - 1 - u.c, r: c.rows - 1 - u.r };
          const foes = c.units.filter((x) => x.team !== u.team && x.alive);
          if (!foes.length) return;
          const x = foes.reduce((b, y) => (c.dist(at, y) < c.dist(at, b) || (c.dist(at, y) === c.dist(at, b) && y.id < b.id) ? y : b));
          if (x.P.ccImmune) return;
          x.stun = Math.max(x.stun, 4);
          x.invulnUntil = Math.max(x.invulnUntil, 4);
          c.text(x, 'Blown away');
        },
      },
    },
    gal_trapclaw: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          const u = c.unit, src = e.source;
          if (e.target !== u || u.state.trapped || e.kind !== 'magic' || e.isBurn || !src || src.team === u.team || !isSpell(src)) return;
          u.state.trapped = true;
          e.mult = 0;
          c.text(u, 'Blocked');
          c.stun(src, 3);
        },
      },
    },
  };

  // ---------- consumables ----------
  function giveBack(c, u, idx, reforge) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.game.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    let back = u.items;
    if (reforge) {
      back = back.map((x) => {
        if (DT.isComponent(x)) return DT.ITEMS[x].special ? x : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (DT.ITEMS[x].emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (DT.ITEMS[x].cat === 'special') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  function copier(c, u, idx) {
    const g = c.game, p = c.player;
    const L = DT.LINE[u.line];
    if (!L || !L.shop || !(g.pool[u.line] > 0)) return 'none';
    const id = p.items[idx];
    const unit = g.newUnit(u.line, 0);
    const free = p.freeBenchSlot();
    if (free < 0) {
      // a full bench still works when the copy completes a digivolution
      const same = p.allUnits().filter((x) => x.line === u.line && x.stage === 0).length;
      if (same < g.copiesOf(u.line) - 1) return 'none';
    }
    p.items.splice(idx, 1);
    g.takeFromPool(u.line);
    if (free >= 0) { p.bench[free] = unit; g.combine(p); } else g.combine(p, unit);
    return id;
  }
  items.gal_copier = { use: copier };
  items.gal_remover = { use: (c, u, idx) => giveBack(c, u, idx, false) };
  items.gal_reforger = { use: (c, u, idx) => giveBack(c, u, idx, true) };

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const augments = {
    gal_a_cash: { hooks: { onPick(c) { c.player.gold += 8; } } },
    gal_a_cache: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    gal_a_rolls: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    gal_a_training: { hooks: team((u) => { u.ad += u.baseAd * 0.12; u.ap += 12; }) },
    gal_a_front: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 30; u.mr += 30; u.maxHp += 200; } }) },
    gal_a_back: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.25; }) },
    gal_a_starlight: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 10) return;
          c.state.done = true;
          for (const u of c.survivors(c.team)) c.heal(u, (u.maxHp - u.hp) * 0.3);
        },
      },
    },
    gal_a_bank: { hooks: { onPick(c) { c.player.gold += 18; } } },
    gal_a_trove: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomComponent()); } } },
    gal_a_mental: { hooks: { onPick(c) { c.give('gal_mental'); } } },
    gal_a_income: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    gal_a_blessing: { hooks: team((u) => { u.omnivamp += 0.12; }) },
    gal_a_lotus: { hooks: team((u) => { u.abilCrit = true; u.crit += 0.15; }) },
    gal_a_implants: { hooks: team((u) => { if (u.items.length) { u.maxHp += 250; u.ad += u.baseAd * 0.15; } }) },
    gal_a_jackpot: { hooks: { onPick(c) { c.player.gold += 35; } } },
    gal_a_armory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    gal_a_golden: { hooks: { onPick(c) { c.give('gal_golden'); } } },
    gal_a_giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    gal_a_stars: {
      hooks: {
        onCountTraits(counts) { for (const t in counts) { const T = DT.TRAITS[t]; if (T && T.kind === 'origin' && T.bp.length > 1 && counts[t] >= T.bp[0]) counts[t] += 1; } },
      },
      ai: { bonus: (c) => (Object.entries(DT.countTraits(c.player.boardUnits())).some(([t, n]) => DT.TRAITS[t] && DT.TRAITS[t].kind === 'origin' && n >= DT.TRAITS[t].bp[0]) ? 3 : 0) },
    },
    gal_a_megaEvolve: { hooks: { onPick(c) { c.giveUnit([4], 1); } } },
    gal_a_overcharge: { hooks: team((u) => { u.mana += 30; u.ap += 20; }) },
  };

  // ---------- the Galaxy mechanic ----------
  // One Galaxy per game, picked from the seed (FNV-1a hash, independent of the game rng). Tools can force one with
  // DT.GAL_FORCE = '<id>' (headless balance runs only).
  const galaxyList = () => (DT.Sets.def('galaxies') || {}).galaxies || [];
  const galaxyOf = (id) => galaxyList().find((g) => g.id === id) || null;
  function rollGalaxy(seed) {
    const list = galaxyList();
    if (DT.GAL_FORCE && list.some((g) => g.id === DT.GAL_FORCE)) return DT.GAL_FORCE;
    let h = 0x9e3779b9;
    const s = `galaxy:${(seed >>> 0) || 0}`;
    for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 0x01000193) >>> 0; }
    const total = list.reduce((n, g) => n + (g.weight || 1), 0);
    let x = h % total;
    for (const g of list) { x -= g.weight || 1; if (x < 0) return g.id; }
    return list[0].id;
  }
  const gal = (c) => (c.game ? c.game.mech.galaxy : c.info && c.info.mech ? c.info.mech.galaxy : null);
  const is = (c, ...ids) => ids.includes(gal(c));

  function salvage(c, unit) {
    const p = c.player;
    for (const id of unit.items) {
      const it = DT.ITEMS[id];
      if (!it || !it.from || it.component || it.emblem || it.cat !== 'completed') continue;
      const i = p.items.lastIndexOf(id);
      if (i < 0 || p.items.length + 1 > c.game.R.itemCap) continue;
      p.items.splice(i, 1);
      for (const x of it.from) c.game.giveItem(p, x);
    }
  }

  function aiUseCopier(c) {
    const p = c.player, g = c.game;
    for (let guard = 0; guard < 3; guard++) {
      const idx = p.items.indexOf('gal_copier');
      if (idx < 0) return;
      // best target: a unit with copies left in the pool, favoring the most valuable Rookie-form unit on the board
      const units = p.boardUnits().concat(p.bench.filter(Boolean)).filter((u) => DT.LINE[u.line].shop && g.pool[u.line] > 0 && u.stage < 2);
      if (!units.length) return;
      const score = (u) => DT.LINE[u.line].tier * 3 + p.allUnits().filter((x) => x.line === u.line && x.stage === 0).length * 4 + (u.stage === 0 ? 2 : 0);
      const best = units.reduce((b, u) => (score(u) > score(b) ? u : b));
      if (g.equipItem(p, idx, best.uid) === 'none') return;
    }
  }

  const mechanic = {
    id: 'galaxy',
    name: 'Galaxies',
    desc: 'Every game rolls one Galaxy from the seed, the same for every Tamer, and shows it at the start of the game. A Galaxy changes the rules for the whole game: extra items, free rerolls, bigger teams, copies of your Digimon and more. File Island and Big Bang are twice as likely as the others.',
    hooks: {
      onGameStart(c) {
        const g = c.game;
        g.mech.galaxy = rollGalaxy(g.seed);
        if (g.mech.galaxy === 'armory') {
          const a = g.randomComponent(), b = g.randomComponent();
          for (const p of g.players) { g.giveItem(p, a); g.giveItem(p, b); }
        }
        if (g.mech.galaxy === 'shrine') for (const p of g.players) g.giveItem(p, 'gal_mental');
      },
      onRoundStart(c) {
        const p = c.player, g = c.game;
        if (is(c, 'trade', 'bigbang')) p.freeRolls += 1;
        if (is(c, 'cluster') && c.stage === 1 && c.round === 2) {
          // the carousel pick is the only unit a Tamer owns at 1-2
          const u = p.allUnits().find((x) => x.stage === 0 && DT.LINE[x.line].shop);
          if (u) {
            const need = g.copiesOf(u.line);
            if (g.pool[u.line] >= need * need - 1) { g.takeFromPool(u.line, need * need - 1); u.stage = 2; }
            else if (g.pool[u.line] >= need - 1) { g.takeFromPool(u.line, need - 1); u.stage = 1; }
          }
        }
        if (is(c, 'mirror') && c.round === 1 && (c.stage === 2 || c.stage === 3)) c.give('gal_copier');
        if (is(c, 'superdense', 'bigbang') && c.stage === 3 && c.round === 1) c.give('gal_golden');
      },
      onRoundEnd(c, e) {
        if (e.pve && is(c, 'treasure')) c.give(c.stage >= 4 ? c.randomCompleted() : c.randomComponent());
        if (e.pve && is(c, 'bigbang')) c.give(c.randomComponent());
        const cb = e.match && e.match.combat;
        if (!cb) return;
        if (!e.pve && is(c, 'plunder')) {
          const foes = cb.units.filter((u) => u.team !== e.team && !u.alive && !u.summoned && !u.state.piloting).length;
          for (let k = 0; k < foes; k++) if (c.rng() < 0.3) e.gold += 1;
        }
        const meep = cb.teamFx[e.team] && cb.teamFx[e.team].meep;
        if (meep) e.gold += Math.min(2, meep);
      },
      onShop(c, e) { if (is(c, 'server')) e.size = 6; },
      onTeamSize(c, e) { if (is(c, 'bigger') && c.game.stage >= 2) e.size += 1; },
      onSell(c, e) { if (is(c, 'salvage')) salvage(c, e.unit); },
      onBuy(c, e) {
        const L = DT.LINE[e.unit.line];
        if (!L || !L.traits.includes('BH')) return;
        const m = c.player.mech;
        m.bounty = m.bounty || {};
        m.bounty[L.id] = Math.min(9, (m.bounty[L.id] || 0) + 1);
      },
      onAiTurn(c) { aiUseCopier(c); },
      // ---- combat (every unit, both teams) ----
      onCombatStart(c) {
        if (is(c, 'darkocean')) {
          for (const u of c.units) u.amp += 0.15;
          for (const t of [0, 1]) { const fx = c.teamState(t); fx.healAmp = (fx.healAmp || 0) - 0.3; }
        }
        if (is(c, 'datastream')) for (const u of c.units) u.mana += 25;
      },
      onAttack(c, e) {
        const u = e.unit, tg = e.target;
        const s = u.state;
        if (s.frenzy && s.frenzy.until > c.t && tg.alive) c.damage(u, tg, tg.maxHp * s.frenzy.pct, 'magic');
        if (s.chosen && s.chosen.until > c.t && tg.alive) c.damage(u, tg, s.chosen.bonus, 'true');
      },
      onKill(c, e) {
        const u = e.unit;
        if (u.line.type === 'excited' && !u.state.excited && u.alive) { u.state.excited = true; u.buffs.push({ stat: 'as', v: 0.5, until: Infinity }); c.text(u, 'Excited'); }
      },
      onUnitTick(c, e) {
        const u = e.unit;
        const s = u.state;
        if (s.frenzy && s.frenzy.until <= c.t) { u.range = s.frRange; s.frenzy = null; }
        if (s.dread && s.dread.until > c.t && u.shield > 0 && c.every(u, 'galDread', 1)) {
          for (const x of c.enemiesNear(u.team, u, 1)) c.damage(u, x, s.dread.dps, 'magic');
        }
        if (s.chosen && s.chosen.until > c.t && c.t >= s.chosen.next) { s.chosen.next += 1; c.heal(u, u.maxHp * 0.06, true); }
        const po = s.galPoison;
        if (po && po.until > c.t - 0.01 && c.t >= po.next) {
          po.next += 1;
          if (po.src) c.damage(po.src, u, po.dps, 'magic', false, true);
          if (!u.alive) return;
        }
        const cy = s.cyc;
        if (cy && cy.until > c.t && c.t >= cy.next) {
          cy.next += 0.5;
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
          casting(u, () => {
            for (const x of c.enemiesNear(u.team, u, 1)) {
              if (!cy.hit[x.id]) { cy.hit[x.id] = true; c.stun(x, 1.5); }
              c.abilityHit(u, x, cy.dmg);
            }
          });
        }
      },
      onBeforeDamage(c, e) {
        const u = e.unit;
        if (u.state.disarmUntil > c.t && e.kind === 'phys' && !e.isBurn) e.mult = 0;
      },
      onBeforeDamageTaken(c, e) {
        const h = e.target.state.hex;
        if (h && h.until > c.t) e.mult *= 1 + h.amp;
      },
      onCast(c, e) {
        const u = e.unit;
        if (u.state.galTax) { u.maxMana = u.state.galTax; u.state.galTax = 0; }
      },
    },
    actions: {},
    // shown by the UI when the game starts (1-1): this game's Galaxy
    intro(game) {
      const id = game && game.mech && game.mech.galaxy;
      const g = galaxyOf(id);
      if (!g) return null;
      const img = DT.ItemArt && DT.ItemArt.img ? DT.ItemArt.img(`galaxy:${id}`, 96) : '';
      return { title: "This game's Galaxy", name: g.name, desc: `${g.short} It changes the rules for every Tamer until the game ends.`, img, color: g.color };
    },
  };

  // codex: economy, rounds and the Galaxy list
  function codexRules(H, pace) {
    const normal = pace === 'normal' || (!!H.xp && !H.levelByStage);
    const esc = (s) => String(s).replace(/[&<>"]/g, (ch) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[ch]);
    const list = galaxyList();
    const total = list.reduce((n, g) => n + (g.weight || 1), 0);
    const rows = list.map((g) => `<tr><td><b style="color:${g.color}">${esc(g.name)}</b></td><td>${esc(g.short)}</td><td>${Math.round((100 * (g.weight || 1)) / total)}%</td></tr>`).join('');
    const tiers = [1, 2, 3, 4, 5];
    const odds = (normal ? [1, 2, 3, 4, 5, 6, 7, 8, 9, 10] : [3, 4, 5, 6, 7, 8, 9]).map((lv) => `<tr><td>${lv}</td>${tiers.map((t) => `<td>${(H.shopOdds[lv] || [])[t - 1] || 0}%</td>`).join('')}</tr>`).join('');
    return [
      { id: 'set', title: 'Galaxies', html: `<p>${esc(DT.SET ? DT.SET.desc : '')}</p><p><b>${esc(mechanic.name)}.</b> ${esc(mechanic.desc)}</p>` },
      { id: 'galaxies', title: 'Galaxy list', html: `<table class="cx-tbl"><tr><th>Galaxy</th><th>Rule</th><th>Odds</th></tr>${rows}</table>` },
      normal ? { id: 'econ', title: 'Economy and damage', html: `<p>Standard pace. 8 Tamers start with ${H.startHp} HP. Buy 4 XP for 4 gold and gain 2 XP a round, up to level 10. Income is 5 gold a round (less in stage 1), plus 1 interest per 10 gold saved (up to 5) and streak gold (2 in a row: 1, 4: 2, 5 or more: 3). A PvP win pays ${H.winGold}. Rerolls cost ${H.rerollCost}.</p>
        <p>A loss costs the Set 3 stage damage (0, 3, 4, 5, 10, 15, 20 in stages 1-7) plus 1 HP per surviving enemy Digimon.</p>` } :
      { id: 'econ', title: 'Economy and damage', html: `<p>8 Tamers start with ${H.startHp} HP and ${H.startGold} gold. Everyone levels up at the start of each stage (level 3 in stage 1, then 4 to 9). Income is set by stage (${H.incomeByStage.slice(1).join(', ')} gold). Win or loss streaks of 3 pay 1 extra gold, streaks of 5 pay 2. A PvP win pays ${H.winGold}. Rerolls cost ${H.rerollCost}.</p>
        <p>A PvP loss costs 1 HP in stages 1-4, 2 in stages 5-6, 3 in stage 7 and 4 after that. From stage 3, every surviving enemy Digimon at Champion form or above (or of cost 4-5) adds 1 more. A wild round loss costs 1 HP plus the same per surviving wild Digimon.</p>` },
      { id: 'shop', title: 'Shop odds', html: `<p>All 8 Tamers share one pool: ${tiers.map((t) => `${H.poolSize[t]} of each ${t}-cost`).join(', ')}.</p><table class="cx-tbl odds"><tr><th>Level</th>${tiers.map((t) => `<th>${t}g</th>`).join('')}</tr>${odds}</table>` },
      normal ? { id: 'rounds', title: 'Rounds and picks', html: '<p>1-1 opening carousel, 1-2 to 1-4 wild Digimon. From stage 2: x-1 to x-3 PvP, x-4 carousel, x-5 and x-6 PvP, x-7 wild Digimon. Augments at 2-1, 3-2 and 4-2. The Galaxy is shown when the game starts. Each x-7 wild round sends the wild Digimon of the next stage.</p>' } :
      { id: 'rounds', title: 'Rounds and picks', html: '<p>1-1 opening carousel, 1-2 and 1-3 wild Digimon. From stage 2: x-1 to x-3 PvP, x-4 wild Digimon. Armory picks at x-1 of stages 2-6, augments at 2-2, 3-2 and 4-2. The Galaxy is shown when the game starts. Wild rounds: Goblimon on File Island, then Kuwagamon and Ogremon, dinosaurs, MetalSeadramon of the Dark Ocean (5-4), Diaboromon and a Kunemon swarm (6-4), Armageddemon (7-4) and Azulongmon (8-4 on).</p>' },
      { id: 'evo', title: 'Digivolution and items', html: '<p>Three copies of a form digivolve into the next form. Items: 8 components, 36 completed items, and the Digimental, which makes a Crest with a component or a Golden Digimental (+1 team size) with another Digimental. 10 more Crests have no recipe and come from the Armory and loot.</p>' },
    ];
  }

  DT.Sets.extend('galaxies', { traits, abilities, items, augments, mechanic, codexRules });
  // codexRules is read from the definition, so put it there too
  const def = DT.Sets.def('galaxies');
  if (def) def.codexRules = codexRules;
  DT.Galaxies = { roll: rollGalaxy, list: galaxyList, get: galaxyOf };
})(typeof window !== 'undefined' ? window : globalThis);
