// K.O. Coliseum behavior: trait, item, artifact and augment hooks, the ability types the set adds (col_frenzy,
// col_bulk, col_aegis, col_infect, col_stance, col_monster), and the set mechanic: Digi-Cores (Power Ups as
// digivolution paths), Stance Master / Monster Tamer form picks, Jewel Gambit cash-outs, Pirate Captain bounties and
// The Champ's fans. Contexts are documented in docs/sets/FORMAT.md. Randomness only from c.rng() / game rng.
(function (G) {
  const DT = G.DT;
  const SET = 'coliseum';
  const def = () => DT.Sets.def(SET);
  const POWER = () => def().powerUps;
  const PU = (id) => POWER().find((x) => x.id === id) || null;
  const FORMS = () => def().forms;
  const formOf = (lineId, id) => (FORMS()[lineId] || []).find((f) => f.id === id) || null;
  const anyForm = (id) => { for (const k in FORMS()) { const f = formOf(k, id); if (f) return f; } return null; };
  const ARTIFACTS = ['col_ar_doom', 'col_ar_collector', 'col_ar_lichbane', 'col_ar_hull', 'col_ar_static', 'col_ar_hydra', 'col_ar_zhonya', 'col_ar_infinity'];

  // ---------- combat helpers ----------
  const mem = (c) => c.mine.filter((u) => u.alive && c.isMember(u));
  const stageOf = (c) => (c.info && c.info.stage) || 1;
  // finish an enemy off: true damage large enough to go through damage reduction and shields
  const execute = (c, src, tg) => { if (tg.alive) c.damage(src, tg, (tg.hp + tg.shield) * 3 + 10, 'true'); };
  const lowestAlly = (c, u) => { let b = null; for (const a of c.units) if (a.alive && a.team === u.team && (!b || a.hp / a.maxHp < b.hp / b.maxHp)) b = a; return b; };
  const nearestAlly = (c, u) => { let b = null, bd = Infinity; for (const a of c.units) { if (!a.alive || a.team !== u.team || a === u) continue; const d = c.dist(u, a); if (d < bd) { bd = d; b = a; } } return b; };
  const nearestEnemies = (c, from, team, n) => c.units.filter((e) => e.alive && e.team !== team).map((e) => [c.dist(from, e) * 10000 + e.id, e]).sort((a, b) => a[0] - b[0]).slice(0, n).map((x) => x[1]);
  // first free cell in a team's front rows (board coordinates; team 1 owns the top half)
  function frontCell(c, team) {
    const rows = team === 0 ? [4, 5, 6, 7] : [3, 2, 1, 0];
    for (const r of rows) for (const col of [3, 2, 4, 1, 5, 0, 6]) if (!c.unitAt(col, r)) return { c: col, r };
    return null;
  }

  // ---------- traits ----------
  const traits = {
    BA: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.maxHp += 50 * c.v; u.state.ba = 0.09 * c.v; } },
        onBeforeDamage(c, e) { if (e.kind === 'magic' && e.unit.state.ba) e.mult *= 1 + e.unit.state.ba; },
      },
    },
    CG: {
      hooks: { onCombatStart(c) { for (const u of mem(c)) { u.maxHp += c.val('hp'); u.amp += c.val('amp'); } } },
      ai: { bonus: (a) => (a.stage <= 4 ? 2 : 0) },
    },
    MEN: {
      hooks: {
        onCombatStart(c) {
          const ms = mem(c).filter((u) => LESSON[u.line.id]);
          if (!ms.length) return;
          if (c.n === 1) { const f = LESSON[ms[0].line.id]; for (const u of c.mine) f(u); return; }
          for (const m of ms) { for (const k in LESSON) LESSON[k](m); m.ap += 10; }
        },
      },
    },
    MECH: {
      hooks: {
        onCombatStart(c) {
          const ms = mem(c);
          const stars = ms.reduce((s, u) => s + u.stage + 1, 0);
          const cell = frontCell(c, c.team);
          if (!cell) return;
          const m = c.summon(c.team, 'col_mech', 0, cell.c, cell.r);
          if (!m) return;
          const late = 1 + 0.1 * Math.max(0, stageOf(c) - 2);
          m.maxHp = Math.round((400 + 145 * stars) * late);
          m.hp = m.maxHp;
          m.ad = m.baseAd = (35 + 10 * stars) * late;
          if (c.lv >= 2) { m.ad *= 1.15; m.P.split = 0.3; }
          if (c.lv >= 3) { m.omnivamp += 0.25; m.dr += 0.2; }
          c.state.mech = m;
          c.text(m, 'Mega Mech');
        },
        onDamageDealt(c, e) {
          const m = c.state.mech;
          if (m && m.alive && e.unit !== m && c.isMember(e.unit)) c.heal(m, e.amount * 0.12, true);
        },
      },
    },
    RC: {
      hooks: {
        onDamageDealt(c, e) {
          if (e.kind === 'true' || e.isBurn || !e.unit.traits.includes('CREW') || !e.target.alive) return;
          c.damage(e.unit, e.target, e.amount * 0.12, 'true');
        },
      },
    },
    RM: {
      hooks: {
        onCombatStart(c) {
          const rose = c.mine.find((u) => u.line.id === 'col_lotosmon');
          if (!rose) return;
          c.state.rose = rose;
          c.state.plants = [];
          const n = rose.stage >= 2 ? 4 : 2;
          const fwd = c.team === 0 ? -3 : 3;
          for (let i = 0; i < n; i++) {
            const kind = i % 2 === 0 ? 'col_root' : 'col_spine';
            const cell = c.freeCellNear(rose, { c: rose.c + (i >= 2 ? (i === 2 ? -2 : 2) : 0), r: rose.r + (kind === 'col_root' ? fwd : -fwd) });
            if (!cell) break;
            const p = growPlant(c, rose, kind, cell);
            if (p) c.state.plants.push({ u: p, kind, cell });
          }
        },
        onCast(c, e) {
          if (e.unit !== c.state.rose || !c.state.plants) return;
          for (const pl of c.state.plants) {
            if (pl.u.alive) { c.heal(pl.u, pl.u.maxHp * 0.35); pl.u.buffs.push({ stat: 'as', v: 0.35, until: Infinity }); continue; }
            const cell = c.unitAt(pl.cell.c, pl.cell.r) ? c.freeCellNear(e.unit, pl.cell) : pl.cell;
            if (!cell) continue;
            const p = growPlant(c, e.unit, pl.kind, cell);
            if (p) pl.u = p;
          }
        },
      },
    },
    SF: {
      hooks: {
        onCombatStart(c) {
          for (const u of mem(c)) u.maxHp += c.v;
          Object.assign(c.state, { stacks: 0, next: 1, sps: c.val('sps'), td: c.val('td') });
        },
        onTick(c) {
          const s = c.state;
          if (s.stacks >= 8 || c.t < s.next) return;
          s.next += 1;
          s.stacks++;
          for (const u of mem(c)) { u.ad += u.baseAd * s.sps; u.ap += s.sps * 100; if (s.stacks === 8) c.text(u, 'Max Spirit'); }
        },
        onDamageDealt(c, e) {
          if (c.state.stacks < 8 || e.kind === 'true' || e.isBurn || !c.isMember(e.unit) || !e.target.alive) return;
          c.damage(e.unit, e.target, e.amount * c.state.td, 'true');
        },
      },
    },
    SM: {
      // runs before every other trait so the stance's trait counts Justimon as a member
      hooks: {
        onCombatStart: {
          order: -100,
          fn(c) {
            for (const u of c.mine) {
              if (u.line.id !== 'col_justimon') continue;
              const f = formOf('col_justimon', (u.mods && u.mods.form) || 'blitz');
              if (f && f.trait && !u.traits.includes(f.trait)) u.traits = u.traits.concat([f.trait]);
            }
          },
        },
      },
    },
    SG: {
      hooks: {
        onCombatStart(c) {
          const m = c.v;
          const lines = new Set(c.mine.filter((u) => u.line.traits.includes('SG')).map((u) => u.line.id));
          const st = stageOf(c);
          if (lines.has('col_lalamon')) { const T = c.teamState(c.team); T.healAmp = (T.healAmp || 0) + 0.1 * m; }
          for (const u of mem(c)) {
            if (lines.has('col_gotsumon')) u.shield += 150 * m;
            if (lines.has('col_syakomon')) u.state.sgAp = 4 * m;
            if (lines.has('col_hawkmon')) u.state.sgHit = (30 + 15 * st) * m;
            if (lines.has('col_kyubimon')) u.state.sgMana = 15 * m;
            if (lines.has('col_bearmon')) u.state.sgHeal = 0.25 * m;
            if (lines.has('col_gargomon')) u.as *= 1 + 0.12 * m;
            if (lines.has('col_ophanimon')) { u.armor += 25 * m; u.mr += 25 * m; u.maxHp += 100 * m; u.ad += u.baseAd * 0.08 * m; u.as *= 1 + 0.08 * m; u.crit += 0.08 * m; }
          }
        },
        onUnitTick(c, e) { const u = e.unit; if (u.state.sgAp && c.every(u, 'sgAp', 3)) u.ap += u.state.sgAp; },
        onAttack(c, e) { const u = e.unit; if (u.state.sgHit && u.nAtk % 3 === 0 && e.target.alive) c.damage(u, e.target, u.state.sgHit, 'magic'); },
        onCast(c, e) { const u = e.unit; if (u.state.sgMana) u.mana = Math.min(u.maxMana, u.mana + u.state.sgMana); },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!u.state.sgHeal || u.state.sgHealed || u.hp >= u.maxHp * 0.4) return;
          u.state.sgHealed = true;
          c.heal(u, u.maxHp * u.state.sgHeal);
        },
      },
    },
    SC: {
      hooks: {
        onCombatStart(c) {
          const ms = mem(c);
          for (const u of ms) u.amp += c.v;
          const want = (c.side(c.team).mech || {}).supreme;
          const order = ms.slice().sort((a, b) => (b.line.id === want) - (a.line.id === want) || b.line.tier - a.line.tier || b.stage - a.stage || a.id - b.id);
          c.state.sup = c.val('sup') - c.v;
          for (const u of order.slice(0, c.lv >= 3 ? 2 : 1)) supreme(c, u);
        },
        onDamageDealt(c, e) {
          const t = e.target;
          if (e.unit.state.supreme && t.alive && e.kind !== 'true' && t.hp < t.maxHp * 0.1) execute(c, e.unit, t);
        },
        onDeath(c, e) {
          if (!e.unit.state.supreme) return;
          e.unit.state.supreme = false;
          const next = mem(c).filter((u) => !u.state.supreme).sort((a, b) => b.dealt - a.dealt || a.id - b.id)[0];
          if (next) supreme(c, next);
        },
      },
    },
    CREW: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.maxHp = Math.round(u.maxHp * (1 + c.v)); u.as *= 1 + c.v; } c.state.acc = 0; },
        onDamageDealt(c, e) {
          if (c.state.firing || e.kind === 'true' || e.isBurn || !c.isMember(e.unit)) return;
          c.state.acc += e.amount;
          while (c.state.acc >= 400) {
            c.state.acc -= 400;
            const pool = c.survivors(1 - c.team);
            if (!pool.length) break;
            const tg = pool[Math.floor(c.rng() * pool.length)];
            c.state.firing = true;
            c.event({ type: 'shot', from: e.unit.id, to: tg.id, big: true });
            c.damage(e.unit, tg, 50 + 25 * stageOf(c), 'magic');
            c.state.firing = false;
          }
        },
      },
    },
    WR: {
      hooks: {
        onCombatStart(c) { c.state.pool = 0; c.state.next = 4; },
        onDamageDealt(c, e) { if (!c.state.realm && c.isMember(e.unit)) c.state.pool += e.amount; },
        onTick(c) {
          const s = c.state;
          if (c.t < s.next) return;
          s.next += 4;
          const ms = mem(c);
          if (!ms.length || s.pool <= 0) return;
          const amount = s.pool * c.v;
          const heal = s.pool * 0.18;
          s.pool = 0;
          s.realm = true;
          for (const e of nearestEnemies(c, ms[0], c.team, 3)) { c.event({ type: 'beam', from: ms[0].id, to: e.id }); c.damage(ms[0], e, amount * 0.6, 'magic'); }
          s.realm = false;
          const low = ms.filter((u) => u.alive).sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id)[0];
          if (low) c.heal(low, heal);
        },
      },
    },
    LU: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.ad += u.baseAd * c.v; u.state.lu = c.val('heal'); } },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!u.state.lu || u.state.luUsed || u.hp >= u.maxHp * 0.5) return;
          u.state.luUsed = true;
          u.stun = 0;
          c.heal(u, u.maxHp * u.state.lu);
          for (const a of c.adjacent(u)) if (a.alive && a.team !== u.team) c.stun(a, 1);
          c.text(u, 'Luchador!');
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
        },
      },
    },
    BAS: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.mine) {
            const k = c.isMember(u) ? c.v : 6 + (c.lv >= 3 ? 25 : 0);
            u.armor += k; u.mr += k;
            if (c.isMember(u)) { c.buff(u, 'armor', c.v, 10); c.buff(u, 'mr', c.v, 10); }
          }
        },
      },
    },
    DUE: { hooks: { onCombatStart(c) { for (const u of mem(c)) { u.atkAs = Math.max(u.atkAs, c.v); if (c.lv >= 3) u.dr += 0.12; } } } },
    EDG: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.ad += u.baseAd * c.v; u.omnivamp += c.val('vamp'); u.state.edge = true; } },
        onAttack(c, e) {
          const u = e.unit;
          if (!u.state.edge || !e.target.alive || e.target.hp >= e.target.maxHp * 0.5 || u.state.edgeUntil > c.t) return;
          u.state.edgeUntil = c.t + 1.5;
          u.buffs.push({ stat: 'as', v: 0.4, until: c.t + 1.5 });
        },
      },
    },
    EXE: { hooks: { onCombatStart(c) { for (const u of mem(c)) { u.crit += c.v; u.critDmg += c.val('cdmg'); u.abilCrit = true; } } } },
    HEV: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.mine) u.maxHp += 60;
          for (const u of mem(c)) { u.maxHp = Math.round(u.maxHp * (1 + c.v)); u.ad += u.maxHp * c.val('k'); }
        },
      },
    },
    JUG: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.dr += c.v; u.state.jug = true; } },
        onBeforeDamageTaken(c, e) { if (e.unit.state.jug && e.unit.hp > e.unit.maxHp * 0.5) e.dr += 0.08; },
        onDeath(c, e) { if (!e.unit.state.jug) return; for (const u of mem(c)) if (u !== e.unit) c.heal(u, u.maxHp * 0.1); },
      },
    },
    PRO: {
      hooks: {
        onCombatStart(c) { for (const u of c.mine) u.state.pro = c.isMember(u) ? c.v : 1; },
        onUnitTick(c, e) { const u = e.unit; if (u.state.pro && u.maxMana > 0 && c.every(u, 'pro', 1)) u.mana = Math.min(u.maxMana, u.mana + u.state.pro); },
        onDamageDealt(c, e) {
          if (c.lv < 4 || e.kind !== 'magic' || !c.isMember(e.unit)) return;
          const a = lowestAlly(c, e.unit);
          if (a) c.heal(a, e.amount * 0.12, true);
        },
      },
    },
    PRT: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) u.state.prt = c.v; },
        onBeforeDamageTaken(c, e) { if (e.unit.state.prt && e.unit.shield > 0) e.dr += 0.05; },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!u.state.prt || u.state.prtUsed || u.hp >= u.maxHp * 0.5) return;
          u.state.prtUsed = true;
          const amt = u.maxHp * u.state.prt;
          c.shield(u, amt);
          const a = nearestAlly(c, u);
          if (a) c.shield(a, amt);
        },
      },
    },
    SNI: { hooks: { onCombatStart(c) { for (const u of mem(c)) { u.amp += c.v; u.perHexAmp = c.val('hex'); } } } },
    SOR: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) u.ap += c.v; },
        onDamageDealt(c, e) { if (c.isMember(e.unit)) e.target.state.sorc = c.team + 1; },
        onAnyDeath(c, e) {
          const v = e.unit;
          if (v.team === c.team || v.state.sorc !== c.team + 1 || v.summoned) return;
          const src = mem(c)[0];
          if (!src) return;
          const pool = c.survivors(1 - c.team);
          for (let k = 0; k < (c.lv >= 3 ? 2 : 1) && pool.length; k++) {
            const tg = pool.splice(Math.floor(c.rng() * pool.length), 1)[0];
            c.event({ type: 'beam', from: src.id, to: tg.id });
            c.damage(src, tg, v.maxHp * c.val('burst'), 'magic');
          }
        },
      },
    },
    STR: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.mine) {
            const k = c.isMember(u) ? 3 : 1;
            if (u.homeR <= 5) u.shield += c.v * k; else u.amp += c.val('amp') * k;
          }
        },
      },
    },
  };

  // Sensei lessons (one per Sensei line)
  const LESSON = {
    col_kamemon: (u) => { u.dr += 0.06; },
    col_reppamon: (u) => { u.ad += u.baseAd * 0.08; u.ap += 8; },
    col_kotemon: (u) => { u.as *= 1.1; },
    col_wizardmon: (u) => { u.P.shojin = (u.P.shojin || 0) + 2; },
  };

  function supreme(c, u) {
    u.state.supreme = true;
    u.amp += c.state.sup || 0;
    c.text(u, 'Supreme');
  }

  function growPlant(c, rose, kind, cell) {
    const p = c.summon(rose.team, kind, Math.max(0, rose.stage - 1), cell.c, cell.r, { hpMul: 0.5 * (rose.ap / 100) });
    if (!p) return null;
    p.hp = p.maxHp;
    if (kind === 'col_spine') { p.ad *= 0.6 * (rose.ap / 100); p.as = rose.as; }
    return p;
  }

  // ---------- ability types ----------
  const abilities = {
    // Gnar, Volibear: a burst of attack damage and attack speed, shared with the 2 nearest allies
    col_frenzy: {
      base: 55,
      cast(c, u, tg, v) {
        const f = v / 200;
        c.buff(u, 'ad', u.baseAd * f, 6);
        u.buffs.push({ stat: 'as', v: 0.4, until: c.t + 6 });
        if (u.line.tier >= 5) c.buff(u, 'omnivamp', 0.15, 6);
        const allies = c.units.filter((a) => a.alive && a.team === u.team && a !== u).map((a) => [c.dist(u, a) * 1000 + a.id, a]).sort((a, b) => a[0] - b[0]).slice(0, 2);
        for (const [, a] of allies) c.buff(a, 'ad', a.baseAd * f / 3, 6);
        if (tg && tg.alive) { c.event({ type: 'hit', from: u.id, to: tg.id, big: true }); c.abilityHit(u, tg, v * 1.5); }
        c.event({ type: 'shield', at: u.id });
      },
      text: (line, stage, v) => `Gains ${Math.round(v / 2)}% attack damage and 40% attack speed for 6 seconds${line.tier >= 5 ? ', plus 15% omnivamp' : ''}. The 2 nearest allies gain ${Math.round(v / 6)}% attack damage. Then strikes its target for ${Math.round(v * 1.5)} magic damage.`,
    },
    // Dr. Mundo: grow, heal, and hit hard with the next attack
    col_bulk: {
      base: 260,
      cast(c, u, tg, v) {
        u.maxHp += v;
        c.heal(u, v);
        u.state.bulk = u.maxHp * 0.08;
        c.text(u, 'Bulk Up');
      },
      text: (line, stage, v) => `Gains ${v} max HP and heals that much. Its next attack deals 8% of its max HP as bonus physical damage.`,
    },
    // Rell, Leona, Poppy: shield, then damage everything within 2 hexes
    col_aegis: {
      base: 190,
      cast(c, u, tg, v) {
        c.shield(u, v);
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        for (const e of c.enemiesNear(u.team, u, 2)) c.abilityHit(u, e, v * 0.35);
      },
      text: (line, stage, v) => `Gains a ${v} shield, then deals ${Math.round(v * 0.35)} magic damage to enemies within 2 hexes.`,
    },
    // Malzahar: damage over 6 seconds; spreads to another enemy when the victim falls
    col_infect: {
      base: 320,
      cast(c, u, tg, v) {
        const free = c.units.filter((e) => e.alive && e.team !== u.team && !e.state.inf).map((e) => [c.dist(u, e) * 10000 + e.id, e]).sort((a, b) => a[0] - b[0]);
        const victim = free.length ? free[0][1] : tg;
        infect(c, u, victim, { per: v / 6, left: 6 });
      },
      text: (line, stage, v) => `Infects the nearest uninfected enemy for ${v} magic damage over 6 seconds. If it falls, the infection moves to the next enemy.`,
    },
    // Justimon: the ability depends on the arm
    col_stance: {
      base: 270,
      cast(c, u, tg, v) {
        const s = (u.mods && u.mods.form) || 'blitz';
        if (s === 'blitz') {
          for (const e of nearestEnemies(c, u, u.team, 2)) { c.event({ type: 'hit', from: u.id, to: e.id, big: true }); c.abilityHit(u, e, v * 0.8); c.buff(e, 'armor', -15, 5); }
        } else if (s === 'critical') {
          const near = c.units.filter((e) => e.alive && e.team !== u.team && c.dist(u, e) <= 3).sort((a, b) => a.hp - b.hp || a.id - b.id);
          const t = near[0] || tg;
          c.event({ type: 'beam', from: u.id, to: t.id });
          c.abilityHit(u, t, v * 1.6);
          if (t.alive) c.stun(t, 1.5);
          for (const e of c.enemiesNear(u.team, t, 1)) if (e !== t) c.abilityHit(u, e, v * 0.5);
        } else {
          c.buff(u, 'dr', 0.5, 2);
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          c.abilityHit(u, tg, v * 1.3);
          for (const e of c.enemiesNear(u.team, u, 2)) if (e !== tg) c.abilityHit(u, e, v * 0.5);
        }
      },
      text: (line, stage, v) => `Blitz Arm: hits the 2 nearest enemies for ${Math.round(v * 0.8)} and cuts their armor by 15. Critical Arm: hits the lowest-health enemy within 3 hexes for ${Math.round(v * 1.6)} and stuns it for 1.5s. Trinity Arm: takes 50% less damage for 2s and smashes for ${Math.round(v * 1.3)}, ${Math.round(v * 0.5)} to enemies within 2 hexes.`,
    },
    // Labramon (Monster Tamer): the ability depends on the partner
    col_monster: {
      base: 200,
      cast(c, u, tg, v) {
        const f = (u.mods && u.mods.form) || 'ballista';
        if (f === 'ballista') {
          c.shield(u, v);
          for (const e of c.enemiesNear(u.team, u, 2)) { e.target = u; c.stun(e, 0.75); c.abilityHit(u, e, v * 0.3); }
        } else if (f === 'dorulu') {
          const t = c.target(u, 'lowestHp') || tg;
          c.event({ type: 'hit', from: u.id, to: t.id, big: true });
          c.abilityHit(u, t, v * 1.2 * (t.hp < t.maxHp * 0.5 ? 2 : 1));
        } else {
          u.buffs.push({ stat: 'as', v: 0.5, until: c.t + 5 });
          const pool = c.units.filter((e) => e.alive && e.team !== u.team);
          for (let k = 0; k < 3 && pool.length; k++) {
            const e = pool.splice(Math.floor(c.rng() * pool.length), 1)[0];
            c.event({ type: 'shot', from: u.id, to: e.id, big: true });
            c.abilityHit(u, e, v * 0.5);
          }
        }
      },
      text: (line, stage, v) => `Ballistamon: gains a ${v} shield, taunts and stuns enemies within 2 hexes for 0.75s and deals ${Math.round(v * 0.3)} to them. Dorulumon: hits the lowest-health enemy for ${Math.round(v * 1.2)}, double below 50% HP. Sparrowmon: gains 50% attack speed for 5s and fires at 3 enemies for ${Math.round(v * 0.5)}.`,
    },
  };

  function infect(c, src, victim, inf) {
    inf.src = src;
    inf.victim = victim;
    victim.state.inf = inf;
    const tick = () => {
      if (inf.left <= 0) return;
      inf.left--;
      const v = inf.victim;
      if (v && v.alive) c.abilityHit(inf.src, v, inf.per);
      if (inf.left > 0) c.after(1, tick);
    };
    c.after(1, tick);
  }

  // ---------- Digi-Core paths (Power Ups) ----------
  // start(c, u) runs once at combat start; the event handlers run from the set's global hooks below.
  const FX = {
    burst: { start(c, u) { u.amp += 0.12; }, kill(c, u) { if (!u.state.burstUp) { u.state.burstUp = true; u.amp += 0.13; } } },
    xanti: {
      taken(c, u, e) {
        const got = u.state.xanti || 0;
        if (got >= 90) return;
        if (e.kind === 'phys') { u.armor += 4; u.state.xanti = got + 4; } else if (e.kind === 'magic') { u.mr += 8; u.state.xanti = got + 8; }
      },
    },
    dark: {
      start(c, u) { u.ad += u.baseAd * 0.22; u.omnivamp += 0.1; },
      tick(c, u) { if (c.every(u, 'pu_dark', 1)) u.hp = Math.max(1, u.hp - u.maxHp * 0.01); },
    },
    warp: { start(c, u) { u.ad += u.baseAd * 0.08; u.ap += 8; u.armor += 10; u.mr += 10; u.amp += 0.05; } },
    armor: {
      start(c, u) { u.shield += u.maxHp * 0.25; },
      tick(c, u) { if (u.shield > 0 && c.every(u, 'pu_armor', 0.5)) u.buffs.push({ stat: 'as', v: 0.3, until: c.t + 0.5 }); },
    },
    mode: {
      start(c, u) {
        const amt = u.maxHp * 0.18;
        u.shield += amt;
        const a = nearestAlly(c, u);
        if (a) a.shield += amt;
      },
    },
    slide: {
      attack(c, u, e) {
        if (u.state.lastTg && u.state.lastTg !== e.target && e.target.alive) c.damage(u, e.target, u.ad * 1.2, 'magic');
        u.state.lastTg = e.target;
      },
    },
    spirit: {
      taken(c, u) {
        if (u.state.spirit || u.hp >= u.maxHp * 0.4) return;
        u.state.spirit = true;
        c.heal(u, u.maxHp * 0.25);
        u.buffs.push({ stat: 'as', v: 0.25, until: Infinity });
        c.text(u, 'Spirit Evolution');
      },
    },
    deathx: { start(c, u) { if (!u.leapt) u.leap = true; u.crit += 0.3; u.critDmg += 0.1; } },
    biomerge: { tick(c, u) { if ((u.state.bio || 0) < 30 && c.every(u, 'pu_bio', 1)) { u.state.bio = (u.state.bio || 0) + 1; u.ad += u.baseAd * 0.03; u.ap += 3; } } },
    mega: { start(c, u) { u.ad += u.baseAd * 0.4; } },
    sage: { start(c, u) { u.ap += 25; } },
    golem: { start(c, u) { u.maxHp = Math.round(u.maxHp * 1.3); } },
    speed: { start(c, u) { u.as *= 1.3; } },
    overwrite: { start(c, u) { u.crit += 0.3; u.critDmg += 0.15; u.abilCrit = true; } },
    matrix: { start(c, u) { if (u.maxMana > 0) { u.maxMana = Math.max(10, Math.round(u.maxMana * 0.75)); u.mana += 10; } } },
    overdrive: {
      start(c, u) { u.amp += 0.12; },
      tick(c, u) { if (!u.state.odDone && c.t >= 10) FX.overdrive.kill(c, u); },
      kill(c, u) {
        if (u.state.odDone) return;
        u.state.odDone = true;
        u.amp += 0.2;
        for (const e of c.enemiesNear(u.team, u, 2)) c.stun(e, 1.5);
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        c.text(u, 'Overdrive');
      },
    },
    atomic: {
      start(c, u) { u.state.atom = 0.1; },
      tick(c, u) { if (c.every(u, 'pu_atom', 1)) u.state.atom += 0.02; },
      death(c, u) {
        const dmg = u.maxHp * (u.state.atom || 0.1);
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        for (const e of c.enemiesNear(u.team, u, 2)) c.damage(u, e, dmg, 'magic');
      },
    },
    thorn: {
      taken(c, u, e) {
        if (e.kind !== 'phys' || !e.source || c.dist(u, e.source) > 1 || u.state.thornCd > c.t) return;
        u.state.thornCd = c.t + 1;
        for (const a of c.enemiesNear(u.team, u, 1)) c.damage(u, a, 50 + u.maxHp * 0.05, 'magic');
      },
    },
    glacier: {
      start(c, u) { u.as *= 1.15; },
      attack(c, u, e) {
        const t = e.target;
        if (!t.alive || t.state.chillUntil > c.t) return;
        t.state.chillUntil = c.t + 2;
        t.buffs.push({ stat: 'as', v: -0.3, until: c.t + 2 });
      },
    },
    inferno: { start(c, u) { u.abilBurn = true; u.vsBurningAmp += 0.15; } },
    hunter: { start(c, u) { u.amp += 0.08; }, before(c, u, e) { if (e.target.hp < e.target.maxHp * 0.5) e.mult *= 1.3; } },
    guardian: {
      start(c, u) {
        u.armor += 30; u.mr += 30;
        for (const a of c.adjacent(u)) if (a.team === u.team) { a.armor += 10; a.mr += 10; }
      },
    },
    ascend: { tick(c, u) { if (!u.state.asc && c.t >= 6) { u.state.asc = true; u.amp += 0.4; c.text(u, 'Ascension'); } } },
  };
  const puOf = (u) => (u && u.mods && u.mods.pu && FX[u.mods.pu] ? FX[u.mods.pu] : null);

  // which paths a Digimon is offered: attack carries, casters or front liners
  function kindOf(lineId) {
    const L = DT.LINE[lineId];
    const ai = L.ai || {};
    if (!ai.carry && ai.tank > 0) return 'tank';
    if (ai.place === 'front' && ai.tank > 0 && ai.tank <= 2) return 'tank';
    return ai.dmg === 'ap' ? 'ap' : 'ad';
  }
  function puOptions(c, unit, exclude = []) {
    const kind = kindOf(unit.line);
    const cur = unit.mods && unit.mods.pu;
    const pool = POWER().filter((x) => (x.tags.includes(kind) || x.tags.includes('any')) && x.id !== cur && !exclude.includes(x.id)).map((x) => x.id);
    return c.shuffle(pool.slice()).slice(0, 3);
  }
  function setPu(unit, id) { unit.mods = { ...(unit.mods || {}), pu: id }; }

  // ---------- game helpers ----------
  const unitByUid = (g, p, uid) => { const f = g.findUnit(p, uid); return f ? f.u : null; };
  function giveCore(c) {
    const p = c.player;
    if (p.items.length < c.game.R.itemCap) c.give('col_core');
    else p.mech.cores = (p.mech.cores || 0) + 1;
  }
  // AI form pick: Justimon picks the arm whose trait lands closest to a breakpoint; Labramon takes the tank partner when the board needs one
  function aiForm(c, unit) {
    const p = c.player;
    const opts = FORMS()[unit.line] || [];
    const counts = DT.countTraits(p.boardUnits(), p.augs);
    let best = null, bs = -Infinity;
    for (const f of opts) {
      let s = c.rng();
      if (f.trait) { const n = counts[f.trait] || 0; s += n + (DT.TRAITS[f.trait].bp.includes(n + 1) ? 3 : 0); }
      if (f.id === 'ballista') s += p.boardUnits().filter((u) => DT.LINE[u.line].ai.tank > 0 && DT.LINE[u.line].ai.tank <= 2).length < 2 ? 2 : 0;
      if (s > bs) { bs = s; best = f; }
    }
    return best;
  }
  function applyForm(unit, f) {
    unit.mods = { ...(unit.mods || {}), form: f.id };
    if (f.trait) unit.bonusTraits = { [f.trait]: 1 };
  }
  function needsForm(u) { return FORMS()[u.line] && !(u.mods && u.mods.form); }

  // Jewel Gambit cash-out: 50% of Gem Power as gold, plus items at 25 / 45 / 80
  function cashOut(c) {
    const p = c.player;
    const m = p.mech;
    const g = (m.gem || 0) * (m.gLv >= 3 ? 1.3 : 1);
    p.gold += Math.round(g * 0.5);
    if (g >= 80) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } else if (g >= 45) c.give(c.randomCompleted());
    else if (g >= 25) c.give(c.randomComponent());
    m.gem = 0; m.gCount = 0; m.dd = false; m.gReady = false;
  }
  const gambitOptions = (p) => [`cash:${Math.round(p.mech.gem || 0)}`, `double:${Math.round(p.mech.gem || 0)}`];

  // Pirate Captain bounty: better rewards the longer Piedmon stays fielded
  function bounty(c) {
    const p = c.player;
    const m = p.mech;
    m.rcN = (m.rcN || 0) + 1;
    const n = m.rcN;
    const pool = ['gold', 'rolls', 'ap', 'shield'];
    if (n >= 3) pool.push('comp', 'comp');
    if (n >= 6) pool.push('big');
    if (n >= 8) pool.push('item');
    const b = c.pick(pool);
    m.rcLast = b;
    if (b === 'gold') p.gold += 2 + Math.floor(n / 3);
    else if (b === 'rolls') p.freeRolls += 2;
    else if (b === 'ap') m.rcAP = (m.rcAP || 0) + 8;
    else if (b === 'shield') { p.nextBuffs = p.nextBuffs || {}; p.nextBuffs.armor = (p.nextBuffs.armor || 0) + 25; }
    else if (b === 'comp') c.give(c.randomComponent());
    else if (b === 'big') p.gold += 8;
    else if (b === 'item') c.give(c.randomCompleted());
  }

  // ---------- offers ----------
  const coreImg = (id) => (DT.ItemArt && DT.ItemArt.img ? DT.ItemArt.img(`pu:${id}`, 64) : '');
  const offerKinds = {
    col_core: {
      name: 'Digi-Core', sub: 'Pick a digivolution path. It stays when the Digimon digivolves.',
      options(c, exclude) { const u = unitByUid(c.game, c.player, c.player.mech.coreUid); return u ? puOptions(c, u, exclude || []) : []; },
      choose(c, id) {
        const u = unitByUid(c.game, c.player, c.player.mech.coreUid);
        c.player.mech.coreUid = null;
        if (!u || !PU(id)) { giveCore(c); return; }
        setPu(u, id);
      },
      aiScore: (c) => c.rng(),
      card: (id) => { const x = PU(id) || { name: id, desc: '' }; return { name: x.name, desc: x.desc, img: coreImg(id) }; },
    },
    col_form: {
      name: 'Choose a form', sub: 'Pick how this Digimon fights. The choice stays when it digivolves.',
      options(c) { const u = unitByUid(c.game, c.player, c.player.mech.formUid); return u ? (FORMS()[u.line] || []).map((f) => f.id) : []; },
      choose(c, id) {
        const u = unitByUid(c.game, c.player, c.player.mech.formUid);
        c.player.mech.formUid = null;
        const f = u && formOf(u.line, id);
        if (f) applyForm(u, f);
      },
      aiScore: (c) => c.rng(),
      card: (id) => { const f = anyForm(id) || { name: id, desc: '' }; return { name: f.name, desc: f.desc, img: coreImg(`form_${id}`) }; },
    },
    col_gambit: {
      name: 'Jewel Gambit', sub: 'Cash out your Gem Power now, or Double Down for the next 3 player combats.',
      options(c) { return gambitOptions(c.player); },
      choose(c, id) {
        const m = c.player.mech;
        if (String(id).startsWith('double')) { m.dd = true; m.gCount = 0; m.gReady = false; } else cashOut(c);
      },
      aiScore(c, id) {
        const p = c.player;
        const dbl = String(id).startsWith('double');
        return (dbl ? (p.hp > 10 && p.streak < 0 ? 2 : -1) : 1) + c.rng();
      },
      card(id) {
        const [k, n] = String(id).split(':');
        const g = Number(n) || 0;
        return k === 'double'
          ? { name: 'Double Down', desc: `Keep your ${g} Gem Power. For the next 3 player combats, losses earn twice as much, but a win halves it and cashes out.`, img: coreImg('gambit_double') }
          : { name: 'Cash Out', desc: `Turn ${g} Gem Power into about ${Math.round(g * 0.5)} gold${g >= 80 ? ' and 2 completed items' : g >= 45 ? ' and a completed item' : g >= 25 ? ' and a component' : ''}.`, img: coreImg('gambit_cash') };
      },
    },
  };

  // ---------- the set mechanic: game hooks ----------
  const label = (c) => `${c.stage}-${c.round}`;
  const mechanic = {
    id: 'digicores',
    name: 'Digi-Cores',
    desc: 'At set rounds (1-3 and 3-3 in Hyper Roll, 1-3 and 3-6 in Normal) every Tamer gets a Digi-Core. Drag it onto a Digimon to pick 1 of 3 digivolution paths for it; selling that Digimon gives the Digi-Core back. Justimon and Labramon pick a form when you get them.',
    offerKinds,
    hooks: {
      onRoundStart(c) {
        const p = c.player;
        const m = p.mech;
        const g = c.game;
        if ((c.R.coreRounds || []).includes(label(c))) m.cores = (m.cores || 0) + 1;
        while (m.cores > 0 && p.items.length < c.R.itemCap) { c.give('col_core'); m.cores--; }
        // Street Crew: a free reroll per three-star Crew Digimon
        p.freeRolls += p.boardUnits().filter((u) => u.stage === 2 && DT.LINE[u.line].traits.includes('CREW')).length;
        // Pirate Captain
        if (c.stage >= 2 && p.boardUnits().some((u) => u.line === 'col_piedmon')) bounty(c);
        // offers (one at a time; AI Tamers decide at once)
        if (m.gReady && (m.gem || 0) > 0) {
          if (!p.isHuman) { const o = gambitOptions(p); let best = o[0], bs = -Infinity; for (const id of o) { const s = offerKinds.col_gambit.aiScore(c, id); if (s > bs) { bs = s; best = id; } } offerKinds.col_gambit.choose(c, best); }
          else if (!p.offer) { p.offer = { kind: 'col_gambit', options: gambitOptions(p), rerolls: 0 }; return; }
        }
        for (const u of p.allUnits()) {
          if (!needsForm(u)) continue;
          if (!p.isHuman) { applyForm(u, aiForm(c, u)); continue; }
          if (!p.offer) { m.formUid = u.uid; p.offer = { kind: 'col_form', options: FORMS()[u.line].map((f) => f.id), rerolls: 0 }; }
          break;
        }
        void g;
      },
      onBuy(c, e) {
        const p = c.player;
        if (!needsForm(e.unit)) return;
        if (!p.isHuman) applyForm(e.unit, aiForm(c, e.unit));
        else if (!p.offer && c.game.phase === 'planning') { p.mech.formUid = e.unit.uid; p.offer = { kind: 'col_form', options: FORMS()[e.unit.line].map((f) => f.id), rerolls: 0 }; }
      },
      onSell(c, e) { if (e.unit.mods && e.unit.mods.pu) giveCore(c); },
      // the path, the form and the stance trait carry over when copies digivolve; a second path's Digi-Core comes back
      onCombine(c, e) {
        const up = e.unit;
        const mods = {};
        let pus = 0;
        for (const f of e.from) {
          if (!f.mods) continue;
          if (f.mods.pu) { pus++; if (!mods.pu) mods.pu = f.mods.pu; }
          if (f.mods.form && !mods.form) mods.form = f.mods.form;
        }
        if (mods.pu || mods.form) up.mods = mods; else delete up.mods;
        const f = mods.form && formOf(up.line, mods.form);
        if (f && f.trait) up.bonusTraits = { [f.trait]: 1 }; else if (FORMS()[up.line]) delete up.bonusTraits;
        for (let k = 1; k < pus; k++) giveCore(c);
      },
      onAiTurn(c) {
        const p = c.player;
        const g = c.game;
        for (let guard = 0; guard < 4 && p.items.includes('col_core'); guard++) {
          const free = (u) => u && DT.LINE[u.line].shop && !(u.mods && u.mods.pu);
          const holders = g.aiHolders(p).map((h) => h.u).filter(free);
          const target = holders[0] || p.boardUnits().filter(free).sort((a, b) => DT.unitValue(b) - DT.unitValue(a))[0];
          if (!target) break;
          const opts = puOptions(c, target);
          if (!opts.length) break;
          p.items.splice(p.items.indexOf('col_core'), 1);
          setPu(target, opts[Math.floor(c.rng() * opts.length)]);
        }
      },
      // Sensei: only exactly 1 or 4 count
      onCountTraits(counts) { if (counts.MEN === 2 || counts.MEN === 3) counts.MEN = 0; },
      onRoundEnd(c, e) {
        const p = c.player;
        const m = p.mech;
        const cb = e.match && e.match.combat;
        if (!cb) return;
        const team = e.team;
        // Supreme Virus: remember who dealt the most damage
        let top = null;
        for (const u of cb.units) if (u.team === team && !u.summoned && u.traits.includes('SC') && (!top || u.dealt > top.dealt)) top = u;
        if (top) m.supreme = top.line.id;
        if (e.pve) return;
        const counts = cb.traitCounts[team] || {};
        // Jewel Gambit
        const lv = counts.CG ? DT.traitLevel('CG', counts.CG) : 0;
        if (lv) {
          const kills = cb.units.filter((u) => u.team !== team && !u.alive && !u.summoned).length;
          const mul = c.stage >= 4 ? 2 : c.stage >= 3 ? 1.5 : 1;
          let gp = kills * mul + (e.won ? 0 : 4 * mul);
          m.gLv = lv;
          if (m.dd && !e.won) gp *= 2;
          m.gem = (m.gem || 0) + gp;
          if (lv >= 2) {
            m.gKills = (m.gKills || 0) + kills;
            const need = m.gTarget || 8;
            if (m.gKills >= need) { m.gKills -= need; c.give(c.randomComponent()); m.gTarget = 6 + Math.floor(c.rng() * 5); }
          }
          if (m.dd && e.won) { m.gem *= 0.5; cashOut(c); } else { m.gCount = (m.gCount || 0) + 1; if (m.gCount >= 3) m.gReady = true; }
        }
        // The Champ
        if (counts.CH) {
          const champ = cb.units.find((u) => u.team === team && u.line.id === 'col_craniamon');
          if (e.won) m.fans = (m.fans || 0) + (champ ? champ.stage + 1 : 1);
          else if (m.fans) { p.hp += Math.min(m.fans, c.R.lossDamage(c.stage)); m.fans = 0; }
        }
      },

      // ---- combat (global listeners: every unit, both teams) ----
      onCombatStart(c) {
        for (const u of c.units) {
          if (u.summoned) continue;
          const id = u.line.id;
          if (id === 'col_justimon') {
            const s = (u.mods && u.mods.form) || 'blitz';
            if (s === 'blitz') u.as *= 1.3; else if (s === 'critical') u.crit += 0.2; else u.maxHp += 400;
          } else if (id === 'col_labramon') {
            const f = (u.mods && u.mods.form) || 'ballista';
            if (f === 'ballista') { u.maxHp = Math.round(u.maxHp * 1.2); u.armor += 15; u.mr += 15; }
            else if (f === 'dorulu') { u.ad += u.baseAd * 0.3; u.omnivamp += 0.2; }
            else { u.range = 4; u.as *= 1.25; u.maxHp = Math.round(u.maxHp * 0.85); }
          } else if (id === 'col_piedmon') {
            const side = c.side(u.team);
            if (side && side.mech && side.mech.rcAP) u.ap += side.mech.rcAP;
          }
          const fx = puOf(u);
          if (fx && fx.start) fx.start(c, u);
        }
      },
      onUnitTick(c, e) {
        const u = e.unit;
        const fx = puOf(u);
        if (fx && fx.tick) fx.tick(c, u);
        if (u.state.allOut && c.every(u, 'allout', 1)) c.damage(u, u, u.maxHp * 0.05, 'true');
      },
      onAttack(c, e) {
        const u = e.unit;
        if (u.state.bulk && e.target.alive) { const b = u.state.bulk; u.state.bulk = 0; c.damage(u, e.target, b, 'phys'); }
        const fx = puOf(u);
        if (fx && fx.attack) fx.attack(c, u, e);
      },
      onBeforeDamage(c, e) { const fx = puOf(e.unit); if (fx && fx.before) fx.before(c, e.unit, e); },
      onDamageTaken(c, e) { const fx = puOf(e.unit); if (fx && fx.taken) fx.taken(c, e.unit, e); },
      onKill(c, e) { const fx = puOf(e.unit); if (fx && fx.kill) fx.kill(c, e.unit, e); },
      onDeath(c, e) {
        const u = e.unit;
        const fx = puOf(u);
        if (fx && fx.death) fx.death(c, u, e);
        // Malzahar's infection moves on
        const inf = u.state.inf;
        if (inf && inf.left > 0 && inf.victim === u) {
          const next = c.units.filter((x) => x.alive && x.team === u.team && !x.state.inf).map((x) => [c.dist(u, x) * 10000 + x.id, x]).sort((a, b) => a[0] - b[0])[0];
          if (next) { inf.victim = next[1]; next[1].state.inf = inf; }
        }
      },
      // K'Sante line (Bakemon): goes All Out instead of falling once
      onLethal(c, e) {
        const u = e.unit;
        if (u.line.id !== 'col_bakemon' || u.state.allOutUsed || u.summoned) return;
        u.state.allOutUsed = true;
        u.state.allOut = true;
        u.hp = u.maxHp * 0.25;
        u.buffs.push({ stat: 'as', v: 0.4, until: Infinity });
        u.ad += (u.armor + u.mr) * 0.3;
        u.invulnUntil = c.t + 0.5;
        c.text(u, 'All Out');
      },
    },
  };

  // ---------- items ----------
  const holder = (c, e) => e.unit === c.unit;
  const items = {
    col_data_gunblade: { hooks: { onDamageDealt(c, e) { if (!holder(c, e) || e.kind === 'true') return; const a = lowestAlly(c, e.unit); if (a) c.heal(a, e.amount * 0.15, true); } } },
    col_night_cloak: { hooks: { onDamageTaken(c, e) { const u = e.unit; if (u.state.nc || u.hp >= u.maxHp * 0.6) return; u.state.nc = true; u.invulnUntil = c.t + 1; u.stun = 0; c.text(u, 'Vanish'); } } },
    col_void_staff: {
      hooks: {
        onDamageDealt(c, e) {
          const t = e.target;
          if (!holder(c, e) || !t.alive || t.state.voidUntil > c.t) return;
          t.state.voidUntil = c.t + 5;
          c.buff(t, 'mr', -Math.max(0, t.mr) * 0.3, 5);
        },
      },
    },
    col_guardian_vow: { hooks: { onDamageTaken(c, e) { const u = e.unit; if (u.state.vow || u.hp >= u.maxHp * 0.4) return; u.state.vow = true; u.mana = Math.min(u.maxMana, u.mana + 15); } } },
    col_adaptive_helm: { hooks: { onCombatStart(c, e) { const u = e.unit; if (u.homeR <= 5) { u.armor += 25; u.mr += 25; } else { u.ad += u.baseAd * 0.15; u.ap += 15; } } } },
    col_steadfast_heart: { hooks: { onBeforeDamageTaken(c, e) { if (e.unit.hp > e.unit.maxHp * 0.5) e.dr += 0.08; } } },
    col_even_shroud: { hooks: { onCombatStart(c, e) { c.buff(e.unit, 'armor', 25, 15); c.buff(e.unit, 'mr', 25, 15); } } },
    col_warmog_disk: { hooks: { onCombatStart(c, e) { e.unit.maxHp = Math.round(e.unit.maxHp * 1.1); } } },
    col_striker_flail: {
      hooks: {
        onDamageDealt(c, e) {
          if (!holder(c, e) || !e.crit) return;
          const u = e.unit;
          u.state.flail = (u.state.flail || []).filter((x) => x > c.t);
          if (u.state.flail.length >= 4) return;
          u.state.flail.push(c.t + 5);
          c.buff(u, 'amp', 0.05, 5);
        },
      },
    },
    // artifacts
    col_ar_doom: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u.state.doom0 && !c.every(u, 'doom', 13)) return;
          u.state.doom0 = true;
          const t = c.target(u, 'closest');
          if (!t) return;
          c.event({ type: 'beam', from: u.id, to: t.id });
          c.damage(u, t, t.maxHp * 0.25, 'magic');
        },
      },
    },
    col_ar_collector: { hooks: { onDamageDealt(c, e) { const t = e.target; if (holder(c, e) && t.alive && e.kind !== 'true' && t.hp < t.maxHp * 0.12) execute(c, e.unit, t); } } },
    col_ar_lichbane: {
      hooks: {
        onCast(c, e) { e.unit.state.lich = true; },
        onAttack(c, e) { const u = e.unit; if (!u.state.lich || !e.target.alive) return; u.state.lich = false; c.damage(u, e.target, 100 + 60 * stageOf(c), 'magic'); },
      },
    },
    col_ar_hull: {
      hooks: {
        onCombatStart(c, e) {
          const u = e.unit;
          if (c.adjacent(u).some((a) => a.team === u.team)) return;
          u.maxHp += 600; u.ad += u.baseAd * 0.2; u.ap += 20;
        },
      },
    },
    col_ar_zhonya: { hooks: { onDamageTaken(c, e) { const u = e.unit; if (u.state.zh || u.hp >= u.maxHp * 0.4) return; u.state.zh = true; u.invulnUntil = c.t + 2.5; c.text(u, 'Stasis'); } } },
    // consumables
    col_core: {
      use(c, unit, idx) {
        const p = c.player;
        if (c.game.phase !== 'planning') return 'locked';
        if (p.offer) return 'none';
        if (!DT.LINE[unit.line] || !DT.LINE[unit.line].shop) return 'none';
        p.items.splice(idx, 1);
        p.mech.coreUid = unit.uid;
        p.offer = { kind: 'col_core', options: puOptions(c, unit), rerolls: 1 };
        return 'core';
      },
    },
    col_extractor: {
      use(c, unit, idx) {
        if (!unit.mods || !unit.mods.pu) return 'none';
        c.player.items.splice(idx, 1);
        const mods = { ...unit.mods };
        delete mods.pu;
        if (Object.keys(mods).length) unit.mods = mods; else delete unit.mods;
        giveCore(c);
        return 'extractor';
      },
    },
    col_remover: { use: (c, u, idx) => giveBack(c, u, idx, false) },
    col_reforger: { use: (c, u, idx) => giveBack(c, u, idx, true) },
  };

  function giveBack(c, u, idx, reforge) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.game.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    let back = u.items;
    if (reforge) {
      back = back.map((x) => {
        const it = DT.ITEMS[x];
        if (DT.isComponent(x)) return it.special ? x : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (it.emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (it.cat === 'artifact') return c.pick(ARTIFACTS.filter((k) => k !== x));
        if (it.cat === 'special') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const ORIGIN_SKIP = ['MEN'];
  const augments = {
    col_a_coins: { hooks: { onPick(c) { c.player.gold += 8; } } },
    col_a_band: { hooks: { onPick(c) { c.give('col_thief_glove'); } } },
    col_a_cache: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    col_a_rolls: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    col_a_heart: { hooks: { onPick(c) { c.player.hp += 5; } } },
    col_a_warmup: { hooks: team((u) => { u.ad += u.baseAd * 0.12; u.ap += 12; }) },
    col_a_front: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 30; u.mr += 30; u.maxHp += 200; } }) },
    col_a_ringside: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.25; }) },
    col_a_snack: { hooks: { onPick(c) { giveCore(c); c.give(c.randomComponent()); } } },
    col_a_second: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 10) return;
          c.state.done = true;
          for (const u of c.survivors(c.team)) c.heal(u, (u.maxHp - u.hp) * 0.35);
        },
      },
    },
    col_a_prize: { hooks: { onPick(c) { c.player.gold += 18; } } },
    col_a_trove: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomComponent()); } } },
    col_a_ascension: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 15) return;
          c.state.done = true;
          for (const u of c.survivors(c.team)) u.amp += 0.4;
        },
      },
    },
    col_a_sponsor: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    col_a_crest: { hooks: { onPick(c) { c.give(c.randomEmblem(true)); } } },
    col_a_tags: { hooks: { onPick(c) { c.give('col_tag'); c.give('col_egg'); } } },
    col_a_snacktime: { hooks: { onPick(c) { giveCore(c); c.player.gold += 6; } } },
    col_a_lotus: { hooks: team((u) => { u.abilCrit = true; u.crit += 0.15; }) },
    col_a_unity: { hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of c.mine) { u.ad += u.baseAd * 0.03 * n; u.ap += 3 * n; } } } },
    col_a_titan: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.15); }) },
    col_a_evolve: { hooks: { onPick(c) { c.giveUnit([2, 3], 1); } } },
    col_a_artifact: { hooks: { onPick(c) { c.give(c.pick(ARTIFACTS)); } } },
    col_a_jackpot: { hooks: { onPick(c) { c.player.gold += 35; } } },
    col_a_armory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    col_a_golden: { hooks: { onPick(c) { c.give('col_golden_egg'); } } },
    col_a_mega: { hooks: { onPick(c) { c.giveUnit([4], 1); } } },
    col_a_giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    col_a_feast: { hooks: { onPick(c) { giveCore(c); giveCore(c); c.give(c.randomCompleted()); } } },
    col_a_arena: {
      hooks: {
        onCountTraits(counts) {
          for (const t in counts) {
            const T = DT.TRAITS[t];
            if (T && T.kind === 'origin' && !ORIGIN_SKIP.includes(t) && T.bp.length > 1 && counts[t] >= T.bp[0]) counts[t] += 1;
          }
        },
      },
      ai: { bonus: (c) => (Object.entries(DT.countTraits(c.player.boardUnits())).some(([t, n]) => DT.TRAITS[t] && DT.TRAITS[t].kind === 'origin' && DT.TRAITS[t].bp.length > 1 && n >= DT.TRAITS[t].bp[0]) ? 3 : 0) },
    },
    col_a_finals: { hooks: team((u) => { u.amp += 0.12; u.dr += 0.1; }) },
  };

  // K'Sante line ability text mentions its All Out passive
  const lines = {
    col_bakemon: { abilText: (stage, v) => `Deals ${v} magic damage and stuns the target for ${DT.stunDuration(stage)}s. Passive: the first time it would fall, it goes All Out instead: back to 25% HP, 40% attack speed and attack damage equal to 30% of its armor and MR, but it loses 5% HP every second.` },
  };

  // tooltip lines: the Digi-Core path (Power Up) and the Stance Master / Monster Tamer form
  function unitNote(u) {
    const m = u.mods;
    if (!m) return null;
    const out = [];
    const pu = m.pu && PU(m.pu);
    if (pu) out.push(`Digi-Core path: ${pu.name}. ${pu.desc}`);
    const f = m.form && formOf(u.line, m.form);
    if (f) out.push(`Form: ${f.name}. ${f.desc}`);
    return out;
  }

  DT.Sets.extend(SET, { traits, abilities, mechanic, items, augments, lines, unitNote });
})(typeof window !== 'undefined' ? window : globalThis);
