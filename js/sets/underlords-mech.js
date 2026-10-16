// Underlords behavior: alliance hooks, hero kits (abilities, passives and 3-star effects), item hooks, global items,
// the four Tamers (powers that put a unit on the board and rank up), the Tamer and starter-crew offers, and the
// Underlords economy rules (free reroll after a loss, streak caps, gold cap, item tier odds). Applies to both
// 'underlords' and 'underlords_ko'. Hook contexts: docs/sets/FORMAT.md. Everything random uses c.rng().
(function (G) {
  const DT = G.DT;
  const SETS = ['underlords', 'underlords_ko'];
  const DEF = DT.Sets.def('underlords');
  const TAMERS = DEF ? DEF.ulTamers : {};

  // ---------- small helpers ----------
  const S3 = (u) => u.stage === 2;
  const pick3 = (u, arr) => arr[Math.min(u.stage, arr.length - 1)];
  const alive = (list) => list.filter((x) => x.alive);
  const allies = (c, u) => c.units.filter((a) => a.alive && a.team === u.team);
  const foes = (c, u) => c.units.filter((e) => e.alive && e.team !== u.team);
  const mem = (c) => c.mine.filter((u) => u.alive && u.traits.includes(c.id));
  const isTamer = (u) => !!(u && u.line && u.line.tamer);
  const lineOf = (u) => (DT.LINE[u.line] ? DT.LINE[u.line] : null);
  const roundNo = (c) => (c.stage - 1) * (c.R.ulPerStage || 5) + c.round;
  const near = (c, team, center, r) => c.units.filter((e) => e.alive && e.team !== team && c.dist(center, e) <= r);
  const nearAllies = (c, team, center, r) => c.units.filter((a) => a.alive && a.team === team && c.dist(center, a) <= r);
  const once = (u, key) => { if (u.state[key]) return false; u.state[key] = true; return true; };
  const canAct = (c, u) => u.alive && !(u.stun > 0) && !(u.silenceUntil > c.t);
  // nearest free cell to a point (deterministic: distance, then cell order)
  function freeCell(c, center, maxR = 7) {
    let best = null, bd = Infinity;
    for (const cell of DT.CELLS) {
      if (c.unitAt(cell.c, cell.r)) continue;
      const d = c.dist(center, cell);
      if (d <= maxR && d < bd) { bd = d; best = cell; }
    }
    return best;
  }
  function moveTo(c, u, cell, dur = 0.2) {
    if (!cell || c.unitAt(cell.c, cell.r)) return false;
    return c.move(u, cell.c, cell.r, dur, { leap: true });
  }
  const hasGlobal = (c, team, id) => { const s = c.side(team); return !!(s && s.globals && s.globals.includes(id)); };
  // summon with the team's summon bonuses (Summoning Stone); passive summons never cast
  function summon(c, team, lineId, stage, center, opts = {}) {
    const cell = opts.cell || freeCell(c, center);
    if (!cell) return null;
    const u = c.summon(team, lineId, Math.max(0, Math.min(2, stage)), cell.c, cell.r, opts);
    if (!u) return null;
    if (u.line.type === 'ul_passive') { u.maxMana = 0; u.mana = 0; }
    if (hasGlobal(c, team, 'ul_g_stone')) { u.maxHp = Math.round(u.maxHp * 1.3); u.hp = u.maxHp; u.as *= 1.3; }
    if (opts.scale) { u.maxHp = Math.round(u.maxHp * opts.scale); u.hp = u.maxHp; u.ad *= opts.scale; u.baseAd = u.ad; }
    return u;
  }
  // poison: stacks (max 10) for 6 s; each stack 4 magic damage per second and -4% attack speed
  function addPoison(c, src, tg, n = 1) {
    if (!tg.alive) return;
    const s = tg.state.ulPois || (tg.state.ulPois = { n: 0, until: 0, acc: 0, src, buff: null });
    s.n = Math.min(10, s.n + n);
    s.until = c.t + 6;
    s.src = src;
    if (!s.buff || !tg.buffs.includes(s.buff)) { s.buff = { stat: 'as', v: 0, until: Infinity }; tg.buffs.push(s.buff); }
    s.buff.v = -0.04 * s.n;
  }
  const poisonOf = (u) => (u.state.ulPois && u.state.ulPois.n) || 0;
  // damage over time on a unit (magic), processed in the mechanic's onUnitTick
  function addDot(c, src, tg, dps, dur, opts = {}) {
    if (!tg.alive) return null;
    const d = { src, dps, until: c.t + dur, acc: 0, ...opts };
    (tg.state.ulDots = tg.state.ulDots || []).push(d);
    return d;
  }
  // timed attack-speed slow that does not stack with itself
  function slow(c, tg, key, v, dur) {
    if (tg.state[key] > c.t) return;
    tg.state[key] = c.t + dur;
    c.buff(tg, 'as', -v, dur);
  }
  function armorCut(c, tg, key, v, dur) {
    if (tg.state[key] > c.t) return;
    tg.state[key] = c.t + dur;
    c.buff(tg, 'armor', -v, dur);
  }
  function shieldUnit(c, tg, amt) { c.shield(tg, amt); }

  // ---------- alliances ----------
  const traits = {
    SHINOBI: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.crit += c.v; u.critDmg = Math.max(u.critDmg, c.val('critDmg')); } },
        onDamageDealt(c, e) { if (e.crit && c.isMember(e.unit) && e.target.alive) e.target.woundUntil = c.t + 3; },
      },
    },
    BEAST: {
      hooks: {
        onCombatStart(c) {
          const side = c.side(c.team);
          const kills = (side && side.mech && side.mech.ulBeastKills) || 0;
          for (const u of mem(c)) u.maxHp += c.val('base') + Math.round(c.v * kills * (u.stage + 1));
        },
      },
      ai: { bonus: (a) => (a.stage >= 4 ? 1 : 0) },
    },
    ONI: {
      hooks: {
        onAttack(c, e) {
          const tg = e.target;
          if (!c.isMember(e.unit) || !tg.alive) return;
          if (!(tg.weakUntil > c.t)) c.damage(e.unit, tg, c.val('dmg'), 'magic');
          if (tg.alive) c.weaken(tg, c.v, 4);
        },
      },
    },
    LEGEND: {
      hooks: {
        onCombatStart: {
          order: -100,
          fn(c) {
            const counts = c.counts(c.team);
            const act = Object.keys(counts).filter((t) => t !== 'LEGEND' && DT.traitLevel(t, counts[t]) > 0);
            for (const u of mem(c)) for (const t of act) if (!u.traits.includes(t)) u.traits = u.traits.concat([t]);
          },
        },
        onCombatReady(c) {
          const n = c.activeTraits(c.team);
          for (const u of mem(c)) { u.amp += c.v * n; u.maxHp = Math.round(u.maxHp * (1 + c.val('hp') * n)); }
        },
      },
      ai: { bonus: (a) => a.active * 0.5 },
    },
    DEMON: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          const demons = c.mine.filter((a) => a.alive && a.traits.includes('DEMON'));
          c.buff(u, 'amp', c.v * demons.length, 4);
          for (const d of demons) if (d !== u) c.silence(d, 2.5);
        },
      },
    },
    DRAGON: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) { u.abilBurn = true; u.ap += 15; } },
        onCast(c, e) {
          if (!c.isMember(e.unit) || !e.target || !(e.value > 0)) return;
          for (const x of near(c, e.unit.team, e.target, 1)) if (x !== e.target) c.damage(e.unit, x, e.value * c.v, 'magic');
        },
      },
    },
    UNDEAD: {
      hooks: {
        onLethal(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || u.state.ulFell) return;
          u.state.ulFell = true;
          for (const a of nearAllies(c, u.team, u, 2)) if (a !== u) { c.heal(a, c.v); a.amp += 0.06; }
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: 1 - u.team });
          if (c.lv >= 2 && u.hp <= 0) { u.hp = u.maxHp * 0.4; c.text(u, 'Rises'); }
        },
      },
    },
    HOLY: { hooks: { onCombatStart(c) { const ts = c.teamState(c.team); ts.healAmp = (ts.healAmp || 0) + c.v; } } },
    NIGHT: { hooks: { onCombatStart(c) { for (const e of c.units) if (e.team !== c.team) e.armor -= c.v; } } },
    PARTNER: {
      hooks: {
        onTick(c) {
          if (c.t < (c.state.next || 1)) return;
          c.state.next = (c.state.next || 1) + 1;
          for (const u of c.mine) if (u.alive && u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + c.v);
        },
      },
    },
    HUNTER: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || c.rng() >= c.v) return;
          for (let k = 0; k < c.lv && e.target.alive; k++) { c.event({ type: u.range > 1 ? 'shot' : 'hit', from: u.id, to: e.target.id }); c.damage(u, e.target, u.ad, 'phys'); }
        },
      },
    },
    KNIGHT: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          const ck = c.mine.some((a) => a.alive && a.line.id === 'ul_chaosknight' && a.stage === 2);
          const beside = ck || c.adjacent(u).some((a) => a !== u && a.alive && a.team === u.team && a.traits.includes('KNIGHT'));
          e.dr += c.v * (beside ? 2 : 1);
        },
      },
    },
    WIZARD: { hooks: { onBeforeDamage(c, e) { if (e.kind === 'magic' && e.unit.team === c.team) e.mult *= 1 + c.v; } } },
    SAGE: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || u.hp >= u.maxHp * 0.5 || !once(u, 'ulSage')) return;
          u.invulnUntil = c.t + 3;
          u.lock = Math.max(u.lock, 3);
          c.text(u, 'Untouchable');
          c.event({ type: 'shield', at: u.id });
        },
      },
    },
    TOXIC: { hooks: { onAttack(c, e) { if (c.isMember(e.unit)) addPoison(c, e.unit, e.target, 1); } } },
    ROGUE: {
      hooks: {
        onCombatStart(c) { for (const u of mem(c)) u.state.ulRogue = { ev: c.v, ad: c.val('ad') }; },
      },
    },
    NATURE: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (c.lv >= 3 || c.isMember(u) || (c.lv >= 2 && u.summoned)) u.ad += c.v;
        },
      },
    },
    DEEP: { hooks: { onCombatStart(c) { for (const u of c.mine) u.mr += c.v; } } },
    SHAMAN: {
      hooks: {
        onCombatStart: {
          order: 10,
          fn(c) {
            const sh = mem(c).sort((a, b) => b.stage - a.stage || b.line.tier - a.line.tier || a.id - b.id)[0];
            if (!sh) return;
            const id = ['ul_s_wolf', 'ul_s_hatchling', 'ul_s_blackdragon'][c.lv - 1];
            const s = summon(c, c.team, id, c.lv === 3 ? 0 : sh.stage, sh);
            if (s) c.text(s, 'Summoned');
          },
        },
      },
    },
    SPIRIT: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          const ch = (c.state.charged = c.state.charged || []);
          if (!ch.includes(u)) ch.push(u);
          const ready = ch.filter((s) => s.alive);
          if (ready.length < 3) return;
          const three = ready.slice(0, 3);
          c.state.charged = ready.slice(3);
          const pts = three.map((s) => (s.target && s.target.alive ? s.target : s));
          const cell = { c: Math.round(pts.reduce((a, p) => a + p.c, 0) / 3), r: Math.round(pts.reduce((a, p) => a + p.r, 0) / 3) };
          let dmg = 0, stun = 0;
          for (const s of three) { const m = [1, 1.5, 2][s.stage]; dmg += c.v * m; stun += 1.5 * m; }
          c.event({ type: 'burst', c: cell.c, r: cell.r, radius: 2, team: c.team });
          for (const x of near(c, c.team, cell, 2)) { c.damage(three[0], x, dmg, 'magic'); if (x.alive) c.stun(x, stun / 3); }
          for (const s of three) c.text(s, 'Delta Strike');
        },
      },
    },
    SUMMONER: { hooks: { onBeforeDamage(c, e) { if (e.unit.summoned && e.unit.team === c.team) e.mult *= 1 + c.v; } } },
    BLADE: {
      hooks: {
        onBeforeDamage(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || e.isBurn) return;
          const n = c.adjacent(u).filter((x) => x.alive && x.team !== u.team).length;
          if (n) e.mult *= 1 + c.v * n;
        },
      },
    },
    JUNGLE: { hooks: { onCombatStart(c) { for (const u of c.mine) u.as *= 1 + (u.traits.includes('JUNGLE') ? c.v : c.val('other')); } } },
    WIND: {
      hooks: {
        onAnyCast(c, e) {
          const caster = e.unit;
          if (caster.team === c.team || !caster.alive) return;
          for (const u of c.mine) {
            if (!u.alive || (c.lv < 2 && !u.traits.includes('WIND')) || c.dist(u, caster) > u.range) continue;
            u.target = caster;
            u.state.ulVig = { id: caster.id, v: c.v };
          }
        },
        onBeforeDamage(c, e) {
          const g = e.unit.state.ulVig;
          if (!g || e.kind !== 'phys' || e.target.id !== g.id) return;
          e.mult *= 1 + g.v;
          e.unit.state.ulVig = null;
        },
      },
    },
    DARK: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (u.summoned || !e.target.alive || (u.state.ulVoidCd || 0) > c.t || c.rng() >= 0.25) return;
          u.state.ulVoidCd = c.t + 0.5;
          c.damage(u, e.target, e.target.maxHp * c.v, 'true');
        },
      },
    },
    METAL: { hooks: { onCombatStart(c) { for (const u of mem(c)) u.armor += c.v; } } },
  };

  // ---------- hero abilities (new ability types) ----------
  const T = (s) => (line, stage, v) => s(line, stage, Math.round(v));
  const A = {
    ul_passive: { base: 0, cast() {}, text: () => 'Passive.' },
    ul_napalm: {
      base: 110,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
        for (const e of near(c, u.team, tg, 1)) { c.abilityHit(u, e, v); slow(c, e, 'ulNapalmSlow', 0.1, 4); if (S3(u)) c.burn(u, e); }
        u.state.ulNapalm = Math.min(pick3(u, [4, 7, 10]), (u.state.ulNapalm || 0) + 1);
      },
      text: T((l, s, v) => `Throws sticky oil: ${v} magic damage to the target and enemies next to it, slowing them. Each cast adds ${[20, 35, 55][s]} magic damage to his attacks (stacks ${[4, 7, 10][s]} times).${s === 2 ? ' The oil sets enemies on fire.' : ''}`),
    },
    ul_shuriken: {
      base: 240,
      cast(c, u, tg, v) { c.event({ type: 'shot', from: u.id, to: tg.id, big: true }); c.abilityHit(u, tg, v); if (tg.alive) c.stun(tg, 0.5); },
      text: T((l, s, v) => `Hurls a shuriken for ${v} magic damage and a short stun.${s === 2 ? ' Earns 1 gold for each enemy Digimon he knocks out.' : ''}`),
    },
    ul_frostbite: {
      base: 55,
      cast(c, u, tg, v) {
        const dur = [1.5, 2, 2.5][u.stage];
        c.event({ type: 'beam', from: u.id, to: tg.id });
        c.freeze(tg, dur);
        tg.woundUntil = c.t + dur + 1;
        addDot(c, u, tg, v, dur + 1);
      },
      text: T((l, s, v) => `Freezes the target for ${[1.5, 2, 2.5][s]} s, dealing ${v} magic damage per second; it heals less meanwhile.${s === 2 ? ' While she lives, allies gain 3 extra mana every second.' : ''}`),
    },
    ul_shadowwave: {
      base: 60,
      cast(c, u, tg, v) {
        const list = allies(c, u).sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id).slice(0, [3, 4, 5][u.stage]);
        if (!list.includes(u)) list.push(u);
        for (const a of list) { c.heal(a, v); for (const e of near(c, u.team, a, 1)) c.abilityHit(u, e, v * 0.6); }
      },
      text: T((l, s, v) => `Heals up to ${[3, 4, 5][s]} hurt allies and himself for ${v}; enemies next to each healed ally take ${Math.round(v * 0.6)} magic damage.${s === 2 ? ' Allies next to him start combat with a Shallow Grave: they survive 5 s after a killing blow.' : ''}`),
    },
    ul_attendants: {
      base: 22,
      cast(c, u, tg, v) { u.state.ulAttend = { until: c.t + 11, next: c.t, v }; c.event({ type: 'shield', at: u.id }); },
      text: T((l, s, v) => `Wisps heal all allies within 4 hexes for ${v} every 1.5 s for 11 s.${s === 2 ? ' Enemies attacking her have a 50% chance to attack much slower for a moment.' : ''}`),
    },
    ul_frostshield: {
      base: 32,
      cast(c, u, tg, v) {
        const list = allies(c, u).filter((a) => !a.summoned);
        const a = list[Math.floor(c.rng() * list.length)] || u;
        c.buff(a, 'dr', [0.12, 0.18, 0.25][u.stage], 7);
        a.state.ulFrostAura = { until: c.t + 7, dps: v, src: u, next: c.t + 1 };
        c.event({ type: 'shield', at: a.id });
        if (S3(u)) chainFrost(c, u, tg);
      },
      text: T((l, s, v) => `Gives a random ally a frost shield for 7 s: it takes ${[12, 18, 25][s]}% less damage and enemies within 2 hexes take ${v} magic damage per second and attack slower.${s === 2 ? ' Also casts Chain Frost: an orb bounces 7 times for 150 magic damage, slowing.' : ''}`),
    },
    ul_empower: {
      base: 0,
      cast(c, u, tg) {
        const list = allies(c, u).filter((a) => !a.summoned && !(a.state.ulEmpUntil > c.t)).sort((a, b) => b.ad - a.ad || a.id - b.id);
        const a = list[0] || u;
        const x = [0.3, 0.5, 0.8][u.stage];
        a.state.ulEmpUntil = c.t + 20;
        c.buff(a, 'amp', x * 0.5, 20);
        a.P.split = (a.P.split || 0) + x;
        c.after(20, () => { a.P.split -= x; });
        c.event({ type: 'shield', at: a.id });
        c.text(a, 'Empowered');
        if (S3(u)) {
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          for (const e of near(c, u.team, u, 2)) { c.damage(u, e, 100, 'magic'); if (e.alive) c.stun(e, 1); }
        }
      },
      text: (l, s) => `Empowers the ally with the most attack damage for 20 s: +${[15, 25, 40][s]}% damage and its attacks cleave ${[30, 50, 80][s]}%.${s === 2 ? ' Also Reverse Polarity: enemies within 2 hexes take 100 damage and are stunned for 1 s.' : ''}`,
    },
    ul_purge: {
      base: 340,
      cast(c, u, tg, v) {
        c.event({ type: 'beam', from: u.id, to: tg.id });
        c.abilityHit(u, tg, v * 0.4);
        if (!tg.alive) return;
        slow(c, tg, 'ulPurgeSlow', 0.5, 5);
        c.weaken(tg, 0.2, 5);
        let lost = 0;
        if (S3(u)) { lost = tg.hp * 0.3; c.damage(u, tg, lost, 'true'); }
        c.after(5, () => { if (tg.alive) { c.abilityHit(u, tg, v); if (lost && tg.alive) c.heal(tg, lost * 0.5); } });
      },
      text: T((l, s, v) => `Purges the target for ${Math.round(v * 0.4)} magic damage: for 5 s it attacks 50% slower and deals 20% less damage, then it takes ${v} magic damage.${s === 2 ? ' Soul Catcher: the target first loses 30% of its current HP and gets half back if it survives.' : ''}`),
    },
    ul_haze: {
      base: 130,
      cast(c, u, tg, v) {
        c.event({ type: 'beam', from: u.id, to: tg.id });
        c.abilityHit(u, tg, v);
        if (!tg.alive) return;
        c.buff(tg, 'armor', -[20, 35, 55][u.stage], 10);
        if (S3(u) && tg.alive) { c.damage(u, tg, 100, 'phys'); if (tg.alive) c.stun(tg, 1.5); }
      },
      text: T((l, s, v) => `Corrosive Haze: ${v} magic damage, and the target loses ${[20, 35, 55][s]} armor for 10 s.${s === 2 ? ' Then bashes it for 100 damage and a 1.5 s stun.' : ''}`),
    },
    ul_scatter: {
      base: 190,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
        const dur = [1, 2, 4][u.stage];
        for (const e of near(c, u.team, tg, 1)) {
          c.abilityHit(u, e, v);
          if (!e.alive) continue;
          slow(c, e, 'ulScatter', [0.1, 0.15, 0.2][u.stage], dur);
          if (S3(u)) armorCut(c, e, 'ulScatterArmor', 10, dur);
        }
      },
      text: T((l, s, v) => `Shotgun blast: ${v} magic damage to the target and enemies next to it, slowing their attacks for ${[1, 2, 4][s]} s.${s === 2 ? ' Also cuts their armor by 10.' : ''}`),
    },
    ul_punch: {
      base: 0,
      cast(c, u, tg) {
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        c.damage(u, tg, u.ad * [3.5, 4, 4.5][u.stage], 'phys', true);
        if (!tg.alive) return;
        c.stun(tg, [1, 1, 1.5][u.stage]);
        slow(c, tg, 'ulPunch', S3(u) ? 0.4 : 0.25, [2, 2, 3][u.stage]);
      },
      text: (l, s) => `Walrus Punch: a critical strike for ${[350, 400, 450][s]}% attack damage that stuns for ${[1, 1, 1.5][s]} s and slows.${s === 2 ? ' The slow lasts longer and is stronger.' : ''}`,
    },
    ul_terror: {
      base: 160,
      cast(c, u, tg, v) {
        for (const e of c.lineTargets(u, tg)) { c.event({ type: 'beam', from: u.id, to: e.id }); c.abilityHit(u, e, v); if (e.alive) armorCut(c, e, 'ulTerror', [10, 15, 20][u.stage], 6); }
      },
      text: T((l, s, v) => `Wave of Terror: ${v} magic damage to every enemy in a line, cutting their armor by ${[10, 15, 20][s]} for 6 s.${s === 2 ? ' When she falls, enemies within 2 hexes take 300 damage.' : ''}`),
    },
    ul_summon: {
      base: 0,
      cast(c, u, tg) {
        const id = u.line.summon;
        if (id === 'ul_s_bear') {
          const bear = c.units.find((b) => b.alive && b.team === u.team && b.line.id === id && b.state.ulOwner === u.id);
          if (bear) { c.heal(bear, bear.maxHp * 0.3); c.buff(bear, 'as', 0.3, 5); c.text(bear, 'Roar'); return; }
        }
        let center = u;
        if (id === 'ul_s_treant' && S3(u)) { const far = c.target(u, 'backline'); if (far) center = far; }
        const s = summon(c, u.team, id, u.stage, center);
        if (!s) return;
        s.state.ulOwner = u.id;
        if (id === 'ul_s_ward' && S3(u)) s.state.ulMagicImmuneUntil = Infinity;
        c.text(s, 'Summoned');
      },
      text: (l, s) => ({
        ul_s_ward: `Summons a Plague Ward that poisons its targets.${s === 2 ? ' Wards are immune to magic damage.' : ''}`,
        ul_s_treant: `Summons a Treant to fight.${s === 2 ? ' Treants appear behind the enemy lines.' : ''}`,
        ul_s_bear: 'Summons a Spirit Bear. If the bear is already out, it heals 30% and attacks faster.',
      })[l.summon] || 'Summons a helper.',
    },
    ul_quill: {
      base: 45,
      cast(c, u, tg, v) { quill(c, u, v); },
      text: T((l, s, v) => `Sprays quills at enemies within 2 hexes: ${v} magic damage plus ${[30, 32, 34][s]} per earlier quill hit on them this combat (up to 5).${s === 2 ? ' Sprays automatically every 1.5 s.' : ''}`),
    },
    ul_chaosbolt: {
      base: 150,
      cast(c, u, tg, v) {
        const r = c.rng();
        c.event({ type: 'beam', from: u.id, to: tg.id });
        c.abilityHit(u, tg, v * (1 + r));
        if (tg.alive) c.stun(tg, [1, 1.5, 2][u.stage] + c.rng());
      },
      text: T((l, s, v) => `Chaos Bolt: ${v}-${v * 2} magic damage and a ${[1, 1.5, 2][s]}-${[2, 2.5, 3][s]} s stun.${s === 2 ? ' Your Knights always count as standing next to another Knight.' : ''}`),
    },
    ul_grip: {
      base: 150,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
        for (const e of near(c, u.team, tg, 1)) { c.abilityHit(u, e, v); if (!e.alive) continue; c.silence(e, 3); if (S3(u)) c.stun(e, 1.5); }
      },
      text: T((l, s, v) => `Pulls a boulder through the target: ${v} magic damage and 3 s silence to it and enemies next to it.${s === 2 ? ' Also stuns them for 1.5 s.' : ''}`),
    },
    ul_bladefury: {
      base: 70,
      cast(c, u, tg, v) {
        u.state.ulFury = { until: c.t + 5, dps: v, next: c.t };
        u.state.ulMagicImmuneUntil = Math.max(u.state.ulMagicImmuneUntil || 0, c.t + 5);
        u.P.ccImmune = Math.max(u.P.ccImmune || 0, c.t + 5);
      },
      text: T((l, s, v) => `Blade Fury: spins for 5 s, immune to magic and stuns, dealing ${v} magic damage per second to adjacent enemies.${s === 2 ? ' While he lives, every ally gains 15% of his max HP.' : ''}`),
    },
    ul_ghostship: {
      base: 70,
      cast(c, u, tg, v) {
        const r = [2, 2, 3][u.stage];
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: r, team: u.team });
        for (const e of near(c, u.team, tg, r)) { c.abilityHit(u, e, v); if (e.alive) c.stun(e, [1, 1.3, 1.7][u.stage]); }
      },
      text: T((l, s, v) => `Ghost Ship: crashes on the target, ${v} magic damage and a ${[1, 1.3, 1.7][s]} s stun to enemies within ${[2, 2, 3][s]} hexes.${s === 2 ? ' Every 4th attack swings for 150 bonus damage and cleaves 75%.' : ''}`),
    },
    ul_duel: {
      base: 0,
      cast(c, u, tg) {
        const opp = near(c, u.team, u, 1).sort((a, b) => a.hp - b.hp || a.id - b.id)[0] || tg;
        if (!opp) return;
        const dur = [4, 4.75, 5.5][u.stage];
        u.target = opp; opp.target = u;
        u.state.ulDuel = { with: opp, until: c.t + dur };
        opp.state.ulDuel = { with: u, until: c.t + dur };
        for (const x of [u, opp]) { c.silence(x, dur); x.woundUntil = c.t + dur; }
        c.text(u, 'Duel!');
        if (S3(u)) for (const e of foes(c, u)) if (e !== opp && e.target === u) e.target = null;
      },
      text: (l, s) => `Duel: she and the weakest adjacent enemy must fight each other for ${[4, 4.75, 5.5][s]} s, without casting or healing. If she wins, she keeps +${[6, 10, 15][s]} attack damage for the rest of the game.${s === 2 ? ' Other enemies attacking her pick new targets.' : ''}`,
    },
    ul_poof: {
      base: 110,
      cast(c, u, tg, v) {
        const list = foes(c, u);
        if (!list.length) return;
        const e = list[Math.floor(c.rng() * list.length)];
        moveTo(c, u, c.freeCellNear(e, u));
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
        for (const x of near(c, u.team, u, 1)) c.abilityHit(u, x, v);
      },
      text: T((l, s, v) => `Poof: teleports to a random enemy and deals ${v} magic damage to enemies next to him. A clone of him joins every combat.${s === 2 ? ' The clone copies his items.' : ''}`),
    },
    ul_hook: {
      base: 240,
      cast(c, u, t0, v) {
        const tg = c.target(u, 'farthest');
        if (!tg) return;
        c.event({ type: 'beam', from: u.id, to: tg.id });
        const cell = c.freeCellNear(u, tg);
        if (cell && c.dist(u, tg) > 1) moveTo(c, tg, cell, 0.3);
        c.abilityHit(u, tg, v);
        if (!tg.alive) return;
        for (const a of nearAllies(c, u.team, u, 1)) a.target = tg;
      },
      text: T((l, s, v) => `Meat Hook: drags the farthest enemy next to him for ${v} magic damage; he and nearby allies attack it.${s === 2 ? ' Flesh Heap: each enemy that falls next to him gives 300 max HP and 10 magic resist.' : ''}`),
    },
    ul_scream: {
      base: 165,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        for (const e of near(c, u.team, u, 2)) c.abilityHit(u, e, v);
      },
      text: T((l, s, v) => `Scream of Pain: ${v} magic damage to all enemies within 2 hexes.${s === 2 ? ' Enemies within 2 hexes miss 25% of their attacks on her.' : ''}`),
    },
    ul_charge: {
      base: 110,
      cast(c, u, t0, v) {
        const tg = c.target(u, 'farthest');
        if (!tg) return;
        const hits = c.lineTargets(u, tg);
        moveTo(c, u, c.freeCellNear(tg, u), 0.3);
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        for (const e of hits) { c.abilityHit(u, e, v); if (e.alive) c.stun(e, [1, 1.3, 1.7][u.stage]); }
        u.target = tg;
      },
      text: T((l, s, v) => `Charge of Darkness: charges the farthest enemy, hitting everything on the way for ${v} magic damage and a ${[1, 1.3, 1.7][s]} s stun.${s === 2 ? ' Greater Bash: attacks have a 25% chance to deal 150 damage and stun for 2 s.' : ''}`),
    },
    ul_aphotic: {
      base: 170,
      cast(c, u, tg, v) {
        const a = allies(c, u).sort((x, y) => x.hp / x.maxHp - y.hp / y.maxHp || x.id - y.id)[0] || u;
        a.stun = 0; a.weak = 0; a.burn = null;
        shieldUnit(c, a, v);
        a.state.ulAphotic = { v, src: u, until: c.t + 10 };
      },
      text: T((l, s, v) => `Aphotic Shield: the most hurt ally loses stuns and burns and gains a ${v} shield; when the shield breaks it bursts for ${v} magic damage within 2 hexes.${s === 2 ? ' Curse of Avernus: hitting the same enemy 3 times silences it for 2 s and he attacks it faster.' : ''}`),
    },
    ul_acid: {
      base: 22,
      cast(c, u, tg, v) {
        const zs = c.teamState(u.team);
        (zs.ulZones = zs.ulZones || []).push({ c: tg.c, r: tg.r, rad: 2, until: c.t + 8, dps: v, armor: [6, 12, 18][u.stage], src: u, next: c.t });
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 2, team: u.team });
      },
      text: T((l, s, v) => `Acid Spray: for 8 s, enemies within 2 hexes of the target take ${v} magic damage per second and lose ${[6, 12, 18][s]} armor.${s === 2 ? ' At the start of combat his ogre rages: +75% attack speed and stun immunity for 5 s.' : ''}`),
    },
    ul_axes: {
      base: 140,
      cast(c, u, tg, v) {
        for (const e of c.lineTargets(u, tg)) {
          c.event({ type: 'beam', from: u.id, to: e.id });
          c.abilityHit(u, e, v);
          if (e.alive) e.state.ulAxed = { by: u.id, amp: [0.1, 0.2, 0.3][u.stage], until: c.t + 6 };
        }
      },
      text: T((l, s, v) => `Wild Axes: ${v} magic damage to enemies in a line; he deals ${[10, 20, 30][s]}% more damage to them for 6 s.${s === 2 ? ' He takes 75% less damage from summoned units.' : ''}`),
    },
    ul_sleight: {
      base: 0,
      cast(c, u) {
        const r = [2, 2, 3][u.stage];
        c.event({ type: 'burst', c: u.c, r: u.r, radius: r, team: u.team });
        for (const e of near(c, u.team, u, r)) { c.event({ type: 'hit', from: u.id, to: e.id }); c.damage(u, e, u.ad + [130, 190, 250][u.stage], 'phys'); }
      },
      text: (l, s) => `Sleight of Fist: strikes every enemy within ${[2, 2, 3][s]} hexes for his attack damage plus ${[130, 190, 250][s]}.`,
    },
    ul_rage: {
      base: 0,
      cast(c, u) {
        u.P.ccImmune = Math.max(u.P.ccImmune || 0, c.t + 5);
        u.state.ulMagicImmuneUntil = Math.max(u.state.ulMagicImmuneUntil || 0, c.t + 5);
        c.buff(u, 'as', 0.3, 5);
        c.text(u, 'Rage');
      },
      text: (l, s) => `Feast (passive): attacks deal ${[3.5, 4.5, 5.5][s]}% of the target's max HP as bonus damage and heal him for it.${s === 2 ? ' Rage: casts to become immune to magic and stuns and attack 30% faster for 5 s.' : ''}`,
    },
    ul_shapeshift: {
      base: 0,
      cast(c, u) {
        const n = [2, 2, 4][u.stage];
        for (let k = 0; k < n; k++) { const w = summon(c, u.team, 'ul_s_wolf', Math.max(0, u.stage - 1), u); if (w) w.state.ulOwner = u.id; }
        if (!u.state.ulShifted) {
          u.state.ulShifted = true;
          const add = Math.round(u.maxHp * [0.3, 0.4, 0.5][u.stage]);
          u.maxHp += add;
          c.heal(u, add);
          u.crit += 0.3;
          u.critDmg = Math.max(u.critDmg, [1.5, 1.75, 2][u.stage]);
        }
      },
      text: (l, s) => `Shapeshift: summons ${[2, 2, 4][s]} wolves; the first time, he also gains ${[30, 40, 50][s]}% max HP and a 30% chance to critically strike.`,
    },
    ul_purify: {
      base: 140,
      cast(c, u, tg, v) {
        const a = allies(c, u).sort((x, y) => x.hp / x.maxHp - y.hp / y.maxHp || x.id - y.id)[0] || u;
        c.heal(a, v);
        const r = S3(u) ? 3 : 1;
        c.event({ type: 'burst', c: a.c, r: a.r, radius: r, team: u.team });
        for (const e of near(c, u.team, a, r)) c.abilityHit(u, e, v);
      },
      text: T((l, s, v) => `Purification: heals the most hurt ally for ${v} and deals ${v} magic damage to enemies within ${s === 2 ? 3 : 1} hex${s === 2 ? 'es' : ''} of it.`),
    },
    ul_wards: {
      base: 0,
      cast(c, u, tg) {
        const n = [2, 3, 4][u.stage];
        for (let k = 0; k < n; k++) {
          const cell = c.freeCellNear(tg, u);
          if (!cell) break;
          const w = summon(c, u.team, 'ul_s_serpent', u.stage, tg, { cell });
          if (w) w.target = tg;
        }
        if (S3(u) && tg.alive) { c.stun(tg, 3); c.text(tg, 'Hexed'); }
      },
      text: (l, s) => `Mass Serpent Ward: traps the target with ${[2, 3, 4][s]} serpent wards.${s === 2 ? ' The target is also hexed (stunned) for 3 s.' : ''}`,
    },
    ul_dagger: {
      base: 260,
      cast(c, u, tg, v) {
        c.event({ type: 'shot', from: u.id, to: tg.id, big: true });
        c.abilityHit(u, tg, v);
        if (tg.alive) slow(c, tg, 'ulDagger', 0.5, 4);
        c.buff(u, 'as', 0.8, 4);
        u.target = tg;
      },
      text: T((l, s, v) => `Spectral Dagger: ${v} magic damage, the target attacks 50% slower and she attacks 80% faster for 4 s.${s === 2 ? ' While she lives, each time one of your Demon Lords casts, all your Demon Lords burn their targets for 150 damage.' : ''}`),
    },
    ul_metamorph: {
      base: 0,
      cast(c, u) {
        if (u.state.ulMeta) return;
        u.state.ulMeta = true;
        u.range = 4;
        u.ad += u.baseAd * [0.6, 1.0, 1.4][u.stage];
        c.buff(u, 'as', [0.5, 0.75, 1][u.stage]);
        const best = allies(c, u).filter((a) => a !== u && !a.summoned).sort((a, b) => b.hp / b.maxHp - a.hp / a.maxHp || a.id - b.id)[0];
        if (best && best.hp / best.maxHp > u.hp / u.maxHp) {
          const mine = u.hp / u.maxHp, theirs = best.hp / best.maxHp;
          u.hp = theirs * u.maxHp; best.hp = Math.max(1, mine * best.maxHp);
          c.event({ type: 'beam', from: u.id, to: best.id });
        }
        u.maxMana = 0;
        c.text(u, 'Metamorphosis');
      },
      text: (l, s) => `Metamorphosis (once per combat): becomes a ranged demon with +${[60, 100, 140][s]}% attack damage and +${[50, 75, 100][s]}% attack speed, and swaps HP percentage with the healthiest ally if that helps.${s === 2 ? ' Deals 60% more damage to Demon Lords.' : ''}`,
    },
    ul_leech: {
      base: 70,
      cast(c, u, tg, v) {
        c.event({ type: 'beam', from: u.id, to: tg.id });
        slow(c, tg, 'ulLeechSlow', 0.5, 5);
        addDot(c, u, tg, v, 5, { leech: true, armor: S3(u) });
      },
      text: T((l, s, v) => `Leech Seed: drains ${v} HP per second from the target for 5 s, slowing it, and heals allies next to it for the same.${s === 2 ? ' Allies it heals gain 10 armor.' : ''}`),
    },
    ul_exorcism: {
      base: 60,
      cast(c, u, tg, v) { u.state.ulExo = { left: [6, 8, 10][u.stage], next: c.t, v }; },
      text: T((l, s, v) => `Exorcism: ${[6, 8, 10][s]} spirits strike enemies within 3 hexes over 6 s for ${v} magic damage each, healing her for 25%.`),
    },
    ul_doom: {
      base: 32,
      cast(c, u, tg, v) { c.event({ type: 'beam', from: u.id, to: tg.id }); c.silence(tg, 10); addDot(c, u, tg, v, 10); },
      text: T((l, s, v) => `Doom: the target cannot cast for 10 s and takes ${v} magic damage per second.`),
    },
    ul_laguna: {
      base: 300,
      cast(c, u, tg, v) { c.event({ type: 'beam', from: u.id, to: tg.id }); c.abilityHit(u, tg, v); if (tg.alive) c.burn(u, tg); },
      text: T((l, s, v) => `Laguna Blade: ${v} magic damage to one enemy and sets it on fire.`),
    },
    ul_arrow: {
      base: 210,
      cast(c, u, tg, v) { c.event({ type: 'shot', from: u.id, to: tg.id, big: true }); c.abilityHit(u, tg, v); if (tg.alive) c.stun(tg, [2, 3, 4][u.stage]); },
      text: T((l, s, v) => `Sacred Arrow: ${v} magic damage and a ${[2, 3, 4][s]} s stun.`),
    },
    ul_shieldcrash: {
      base: 85,
      cast(c, u, tg, v) {
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        const hit = near(c, u.team, u, 2);
        for (const e of hit) { c.abilityHit(u, e, v); if (e.alive) e.target = u; }
        if (hit.length) c.buff(u, 'dr', Math.min(0.4, 0.08 * hit.length), 10);
      },
      text: T((l, s, v) => `Shield Crash: ${v} magic damage within 2 hexes; for 10 s he takes 8% less damage per enemy hit and they attack him.`),
    },
    ul_fadebolt: {
      base: 110,
      cast(c, u, tg, v) {
        const hit = new Set();
        let cur = tg;
        for (let k = 0; k < 5 && cur; k++) {
          hit.add(cur);
          c.event({ type: 'beam', from: u.id, to: cur.id });
          c.abilityHit(u, cur, v);
          if (cur.alive) c.weaken(cur, 0.2, 5);
          let next = null, nd = Infinity;
          for (const e of foes(c, u)) { if (hit.has(e)) continue; const d = c.dist(cur, e); if (d <= 3 && d < nd) { nd = d; next = e; } }
          cur = next;
        }
      },
      text: T((l, s, v) => `Fade Bolt: arcane energy hits the target and jumps 4 times for ${v} magic damage; each hit enemy deals 20% less damage for 5 s.`),
    },
    ul_godstrength: {
      base: 0,
      cast(c, u) { u.ad += u.baseAd * [0.8, 1.2, 1.6][u.stage]; u.maxMana = 0; c.text(u, "God's Strength"); },
      text: (l, s) => `God's Strength (once per combat): +${[80, 120, 160][s]}% attack damage for the rest of combat.`,
    },
    ul_refraction: {
      base: 0,
      cast(c, u) {
        const n = [4, 6, 8][u.stage];
        u.state.ulRefract = (u.state.ulRefract || 0) + n;
        u.state.ulRefractHits = (u.state.ulRefractHits || 0) + n;
        u.state.ulRefractAmp = [0.4, 0.6, 0.8][u.stage];
        c.event({ type: 'shield', at: u.id });
      },
      text: (l, s) => `Refraction: blocks the next ${[4, 6, 8][s]} hits completely, and her next ${[4, 6, 8][s]} attacks deal ${[40, 60, 80][s]}% more damage.`,
    },
    ul_ravage: {
      base: 80,
      cast(c, u, tg, v) {
        const r = [1, 2, 3][u.stage];
        c.event({ type: 'burst', c: u.c, r: u.r, radius: r, team: u.team });
        for (const e of near(c, u.team, u, r)) { c.abilityHit(u, e, v); if (e.alive) c.stun(e, [1.5, 1.75, 2.2][u.stage]); }
      },
      text: T((l, s, v) => `Ravage: tentacles hit every enemy within ${[1, 2, 3][s]} hexes for ${v} magic damage and stun them for ${[1.5, 1.75, 2.2][s]} s.`),
    },
    ul_nethertoxin: {
      base: 100,
      cast(c, u, tg, v) { c.event({ type: 'shot', from: u.id, to: tg.id, big: true }); c.silence(tg, 5); addDot(c, u, tg, v, 5); addPoison(c, u, tg, 2); },
      text: T((l, s, v) => `Nethertoxin: the target cannot cast for 5 s, takes ${v} magic damage per second and gains 2 poison stacks.`),
    },
    ul_dissimilate: {
      base: 150,
      cast(c, u, tg, v) {
        const list = foes(c, u);
        if (!list.length) return;
        let best = list[0], bn = -1;
        for (const e of list) { const n = near(c, u.team, e, 1).length; if (n > bn) { bn = n; best = e; } }
        moveTo(c, u, c.freeCellNear(best, u), 0.3);
        c.event({ type: 'burst', c: best.c, r: best.r, radius: 1, team: u.team });
        for (const e of near(c, u.team, best, 1)) { c.abilityHit(u, e, v); if (e.alive) armorCut(c, e, 'ulDissim', [20, 30, 40][u.stage], 5); }
      },
      text: T((l, s, v) => `Dissimilate: vanishes and strikes the biggest enemy group for ${v} magic damage, cutting armor by ${[20, 30, 40][s]} for 5 s.`),
    },
    ul_breathefire: {
      base: 130,
      cast(c, u, tg, v) {
        for (const e of c.lineTargets(u, tg).slice(0, 4)) { c.event({ type: 'beam', from: u.id, to: e.id }); c.abilityHit(u, e, v); if (e.alive) c.weaken(e, [0.25, 0.35, 0.45][u.stage], 4); }
      },
      text: T((l, s, v) => `Breathe Fire: ${v} magic damage to enemies in a cone; they deal ${[25, 35, 45][s]}% less damage for 4 s.`),
    },
    ul_chrono: {
      base: 0,
      cast(c, u, tg) {
        const d = [1.4, 1.8, 2.2][u.stage];
        c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 2, team: u.team });
        for (const e of near(c, u.team, tg, 2)) c.stun(e, d);
        c.buff(u, 'as', 0.75, d);
        c.text(u, 'Chronosphere');
      },
      text: (l, s) => `Chronosphere: stops time around the target; enemies within 2 hexes are stunned for ${[1.4, 1.8, 2.2][s]} s while he attacks 75% faster.`,
    },
    ul_illuminate: {
      base: 160,
      cast(c, u, tg, v) {
        const hits = c.lineTargets(u, tg);
        for (const e of near(c, u.team, tg, 1)) if (!hits.includes(e)) hits.push(e);
        for (const e of hits) { c.event({ type: 'beam', from: u.id, to: e.id }); c.abilityHit(u, e, v); }
      },
      text: T((l, s, v) => `Illuminate: a wave of light deals ${v} magic damage to enemies in a line and around the target.`),
    },
    ul_gaze: {
      base: 100,
      cast(c, u, tg, v) {
        const list = near(c, u.team, u, 3);
        for (const e of list) { c.abilityHit(u, e, v); if (e.alive) slow(c, e, 'ulGaze', 0.3, 3); }
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 3, team: u.team });
        c.after(2, () => {
          if (!u.alive) return;
          for (const e of near(c, u.team, u, 3)) { if (!list.includes(e)) continue; c.stun(e, [1.5, 2, 2.5][u.stage]); e.state.ulStone = c.t + [1.5, 2, 2.5][u.stage]; c.text(e, 'Stone'); }
        }, u);
      },
      text: T((l, s, v) => `Stone Gaze: enemies within 3 hexes take ${v} magic damage and attack 30% slower; those still near her after 2 s turn to stone for ${[1.5, 2, 2.5][s]} s and take 20% more physical damage.`),
    },
    ul_wraithfire: {
      base: 70,
      cast(c, u, tg, v) { c.event({ type: 'shot', from: u.id, to: tg.id, big: true }); c.stun(tg, 2); addDot(c, u, tg, v, 5); },
      text: T((l, s, v) => `Wraithfire Blast: stuns the target for 2 s and burns it for ${v} magic damage per second for 5 s.`),
    },
  };

  function quill(c, u, v) {
    c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
    const per = [30, 32, 34][u.stage];
    for (const e of near(c, u.team, u, 2)) {
      const n = e.state.ulQuill || 0;
      c.abilityHit(u, e, v + per * n);
      if (e.alive) e.state.ulQuill = Math.min(5, n + 1);
    }
  }
  function chainFrost(c, u, tg) {
    const hit = [];
    let cur = tg;
    for (let k = 0; k < 7 && cur; k++) {
      hit.push(cur);
      c.event({ type: 'beam', from: u.id, to: cur.id });
      c.damage(u, cur, (150 * u.ap) / 100, 'magic');
      if (cur.alive) slow(c, cur, 'ulChainFrost', 0.5, 2.5);
      let next = null, nd = Infinity;
      for (const e of foes(c, u)) { if (e === cur) continue; const d = c.dist(cur, e) + hit.filter((h) => h === e).length * 3; if (d <= 4 && d < nd) { nd = d; next = e; } }
      cur = next;
    }
  }

  // ---------- passives and 3-star effects (by line id) ----------
  const KIT = {
    ul_antimage: {
      attack(c, e) {
        const u = e.unit, tg = e.target;
        if (!tg.alive || !(tg.maxMana > 0)) return;
        const before = tg.mana;
        const b = Math.min(Math.max(0, before), [10, 20, 32][u.stage]);
        tg.mana -= b;
        if (b > 0) c.damage(u, tg, b * 0.5, 'magic');
        if (S3(u) && before > 0 && tg.mana <= 0 && tg.alive) c.damage(u, tg, 150, 'magic');
      },
      text: (s) => `Mana Break (passive): attacks burn ${[10, 20, 32][s]} mana and deal half of it as magic damage.${s === 2 ? ' Burning the last of a target\'s mana deals 150 more damage.' : ''}`,
    },
    ul_batrider: {
      attack(c, e) { const n = e.unit.state.ulNapalm || 0; if (n && e.target.alive) c.damage(e.unit, e.target, n * [20, 35, 55][e.unit.stage], 'magic'); },
    },
    ul_crystal: {
      tick(c, u) {
        if (!S3(u) || !c.every(u, 'ulCM', 1)) return;
        for (const a of allies(c, u)) if (a.maxMana > 0) a.mana = Math.min(a.maxMana, a.mana + 3);
      },
    },
    ul_dazzle: {
      start(c, u) { if (S3(u)) for (const a of nearAllies(c, u.team, u, 1)) if (a !== u) a.state.ulGrave = Math.max(a.state.ulGrave || 0, 5); },
    },
    ul_drow: {
      start(c, u) { for (const a of nearAllies(c, u.team, u, 1)) a.as *= 1 + [0.1, 0.2, 0.3][u.stage]; },
      attack(c, e) { if (S3(e.unit) && e.target.alive) slow(c, e.target, 'ulFrostArrow', 0.2, 2); },
      text: (s) => `Precision Aura (passive): allies next to her at the start of combat attack ${[10, 20, 30][s]}% faster.${s === 2 ? ' Frost Arrows: her attacks slow the target\'s attack speed by 20%.' : ''}`,
    },
    ul_enchantress: {
      struck(c, e) { if (S3(e.target) && c.rng() < 0.5) slow(c, e.unit, 'ulUntouch', 0.5, 1.5); },
      tick(c, u) {
        const s = u.state.ulAttend;
        if (!s || c.t > s.until || c.t < s.next) return;
        s.next = c.t + 1.5;
        for (const a of nearAllies(c, u.team, u, 4)) c.heal(a, s.v, true);
      },
    },
    ul_magnus: {},
    ul_phantom: {
      start(c, u) {
        u.crit += [0.15, 0.2, 0.25][u.stage];
        u.critDmg = Math.max(u.critDmg, [2.5, 3.5, 4.5][u.stage]);
        if (S3(u)) { const lv = c.traitLevel(u.team, 'ROGUE'); if (lv) u.state.ulRogue = { ev: [0.2, 0.4][lv - 1], ad: [10, 20][lv - 1] }; }
      },
      text: (s) => `Coup de Grace (passive): ${[15, 20, 25][s]}% chance to critically strike for ${[250, 350, 450][s]}% damage.${s === 2 ? ' She also gains your active Rogue bonus.' : ''}`,
    },
    ul_vengeful: {
      death(c, u) {
        if (!S3(u)) return;
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        for (const e of near(c, u.team, u, 2)) c.damage(u, e, 300, 'magic');
      },
    },
    ul_s_ward: { attack(c, e) { addPoison(c, e.unit, e.target, 1); } },
    ul_bristleback: {
      tick(c, u) {
        if (!S3(u) || !canAct(c, u) || !c.every(u, 'ulQuillAuto', 1.5)) return;
        quill(c, u, (DT.abilityValue(u.line, u.stage) * u.ap) / 100);
      },
    },
    ul_juggernaut: {
      ready(c, u) {
        if (!S3(u)) return;
        u.state.ulJugg = [];
        for (const a of allies(c, u)) if (a !== u && !a.summoned) { const add = Math.round(u.maxHp * 0.15); a.maxHp += add; a.hp += add; u.state.ulJugg.push([a, add]); }
      },
      death(c, u) {
        for (const [a, add] of u.state.ulJugg || []) { if (!a.alive) continue; a.maxHp -= add; a.hp = Math.max(1, Math.min(a.hp, a.maxHp)); }
      },
      tick(c, u) {
        const s = u.state.ulFury;
        if (!s || c.t > s.until || c.t < s.next) return;
        s.next = c.t + 0.5;
        for (const e of near(c, u.team, u, 1)) c.damage(u, e, s.dps * 0.5, 'magic');
      },
    },
    ul_kunkka: {
      attack(c, e) {
        const u = e.unit;
        if (!S3(u) || u.nAtk % 4 !== 0 || !e.target.alive) return;
        c.damage(u, e.target, 150, 'phys');
        for (const x of near(c, u.team, e.target, 1)) if (x !== e.target) c.damage(u, x, u.ad * 0.75, 'phys');
        c.text(u, 'Tidebringer');
      },
    },
    ul_luna: {
      attack(c, e) {
        const u = e.unit;
        const hit = [e.target];
        let cur = e.target, dmg = e.dmg;
        for (let k = 0; k < [3, 5, 7][u.stage]; k++) {
          dmg *= 0.7;
          let next = null, nd = Infinity;
          for (const x of foes(c, u)) { if (hit.includes(x)) continue; const d = c.dist(cur, x); if (d <= 2 && d < nd) { nd = d; next = x; } }
          if (!next) break;
          hit.push(next);
          c.event({ type: 'shot', from: u.id, to: next.id });
          c.damage(u, next, dmg, 'phys');
          cur = next;
        }
      },
      hurt(c, u) {
        if (!S3(u) || u.hp >= u.maxHp * 0.5 || !once(u, 'ulEclipse')) return;
        c.text(u, 'Eclipse');
        let k = 0;
        const beam = () => {
          if (!u.alive || k >= 15) return;
          k++;
          const list = near(c, u.team, u, 2).filter((x) => (x.state.ulBeams || 0) < 5);
          if (list.length) {
            const x = list[Math.floor(c.rng() * list.length)];
            x.state.ulBeams = (x.state.ulBeams || 0) + 1;
            c.event({ type: 'beam', from: u.id, to: x.id });
            c.damage(u, x, 150, 'magic');
            if (x.alive) c.stun(x, 0.5);
          }
          c.after(0.3, beam, u);
        };
        beam();
      },
      text: (s) => `Moon Glaives (passive): her attacks bounce to ${[3, 5, 7][s]} more enemies within 2 hexes, losing 30% each bounce.${s === 2 ? ' Eclipse: the first time she drops below 50% HP, 15 moon beams strike nearby enemies for 150 damage and short stuns.' : ''}`,
    },
    ul_meepo: {
      start(c, u) {
        if (u.summoned) return;
        const cl = summon(c, u.team, 'ul_meepo', u.stage, u, { items: S3(u) ? u.items.slice() : [], scale: 0.45 });
        if (cl) { cl.state.ulClone = true; cl.name = `${u.name} clone`; }
      },
    },
    ul_spiritbreaker: {
      attack(c, e) {
        const u = e.unit;
        if (!S3(u) || !e.target.alive || (u.state.ulBashCd || 0) > c.t || c.rng() >= 0.25) return;
        u.state.ulBashCd = c.t + 1;
        c.damage(u, e.target, 150, 'phys');
        if (e.target.alive) c.stun(e.target, 2);
      },
    },
    ul_stormspirit: {
      cast(c, e) { if (S3(e.unit)) e.unit.state.ulOverload = true; },
      attack(c, e) {
        const u = e.unit;
        if (!u.state.ulOverload || !e.target.alive) return;
        u.state.ulOverload = false;
        c.damage(u, e.target, 150, 'magic');
        if (e.target.alive) slow(c, e.target, 'ulOverload', 0.5, 1.5);
      },
    },
    ul_windranger: {
      cast(c, e) { if (S3(e.unit)) { e.unit.state.ulEvadeUntil = c.t + 3; e.unit.state.ulMagicImmuneUntil = Math.max(e.unit.state.ulMagicImmuneUntil || 0, c.t + 3); } },
    },
    ul_abaddon: {
      attack(c, e) {
        const u = e.unit;
        if (!S3(u)) return;
        const s = u.state;
        s.ulCurseN = s.ulCurseTg === e.target ? (s.ulCurseN || 0) + 1 : 1;
        s.ulCurseTg = e.target;
        if (s.ulCurseN === 3 && e.target.alive) { c.silence(e.target, 2); c.buff(u, 'as', 0.3, 2); }
      },
    },
    ul_alchemist: { start(c, u) { if (S3(u)) { c.buff(u, 'as', 0.75, 5); u.P.ccImmune = Math.max(u.P.ccImmune || 0, 5); } } },
    ul_lifestealer: {
      start(c, u) { if (!S3(u)) { u.maxMana = 0; u.mana = 0; } },
      attack(c, e) {
        const u = e.unit, tg = e.target;
        if (!tg.alive) return;
        const x = tg.maxHp * [0.035, 0.045, 0.055][u.stage];
        c.damage(u, tg, x, 'phys');
        c.heal(u, x, true);
      },
    },
    ul_puck: { start(c, u) { if (S3(u)) u.maxMana = Math.min(u.maxMana, 25); } },
    ul_slark: {
      attack(c, e) {
        const u = e.unit;
        const x = [6, 8, 10][u.stage];
        u.ad += x;
        if (e.target.alive) e.target.ad = Math.max(0, e.target.ad - x);
        u.state.ulShift = Math.min(30, (u.state.ulShift || 0) + 1);
        if (!u.state.ulShiftBuff || !u.buffs.includes(u.state.ulShiftBuff)) { u.state.ulShiftBuff = { stat: 'as', v: 0, until: Infinity }; u.buffs.push(u.state.ulShiftBuff); }
        u.state.ulShiftBuff.v = u.state.ulShift * 0.04;
      },
      tick(c, u) {
        if (!S3(u) || !c.every(u, 'ulPact', 6)) return;
        c.after(1, () => {
          u.stun = 0; u.weak = 0; u.burn = null; if (u.state.ulPois) { u.state.ulPois.n = 0; if (u.state.ulPois.buff) u.state.ulPois.buff.v = 0; }
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          for (const e of near(c, u.team, u, 2)) c.damage(u, e, 300, 'magic');
          u.hp = Math.max(1, u.hp - 150);
        }, u);
      },
      text: (s) => `Essence Shift (passive): each attack steals ${[6, 8, 10][s]} attack damage from the target and gives him attack speed, stacking.${s === 2 ? ' Dark Pact: every 6 s he purges himself and deals 300 damage within 2 hexes, losing 150 HP.' : ''}`,
    },
    ul_axe: {
      struck(c, e) {
        const u = e.target;
        if (!u.alive || c.rng() >= [0.2, 0.22, 0.25][u.stage]) return;
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
        for (const x of near(c, u.team, u, 1)) c.damage(u, x, [100, 180, 300][u.stage], 'true');
      },
      text: (s) => `Counter Helix (passive): when attacked, ${[20, 22, 25][s]}% chance to spin, dealing ${[100, 180, 300][s]} true damage to adjacent enemies.`,
    },
    ul_trollwarlord: {
      attack(c, e) {
        const u = e.unit, s = u.state;
        s.ulFerN = s.ulFerTg === e.target ? Math.min([8, 12, 16][u.stage], (s.ulFerN || 0) + 1) : 0;
        s.ulFerTg = e.target;
        if (!s.ulFerBuff || !u.buffs.includes(s.ulFerBuff)) { s.ulFerBuff = { stat: 'as', v: 0, until: Infinity }; u.buffs.push(s.ulFerBuff); }
        s.ulFerBuff.v = s.ulFerN * [0.15, 0.2, 0.25][u.stage];
      },
      text: (s) => `Fervor (passive): each attack on the same target gives ${[15, 20, 25][s]}% attack speed, up to ${[8, 12, 16][s]} times. Switching targets resets it.`,
    },
  };
  // a few passive descriptions for lines whose ability is ul_passive
  const PASSIVE_TEXT = {
    ul_stormspirit: (st, v) => `Ball Lightning: charges through a line of enemies, dealing ${v} magic damage to each.${st === 2 ? ' Overload: after casting, his next attack deals 150 magic damage and slows.' : ''}`,
    ul_windranger: (st, v) => `Powershot: a charged arrow deals ${v} magic damage to every enemy in a line.${st === 2 ? ' For 3 s after casting she evades all attacks and is immune to magic.' : ''}`,
    ul_puck: (st, v) => `Illusory Orb: an orb flies through a line of enemies for ${v} magic damage each.${st === 2 ? ' She casts it far more often.' : ''}`,
    ul_tamer_anessa: () => 'Tamer: abilities run on cooldowns (see her build).',
    ul_tamer_hob: () => 'Tamer: abilities run on cooldowns (see his build).',
    ul_tamer_jules: () => 'Tamer: abilities run on cooldowns (see his build).',
    ul_tamer_enno: () => 'Tamer: abilities run on cooldowns (see his build).',
  };

  // ---------- the set mechanic: combat-wide hooks, Underlords economy, offers ----------
  function lethalSave(c, u) {
    // Shallow Grave / Vesture: survive for a few seconds after a killing blow (once)
    const g = u.state.ulGrave;
    if (!g || u.state.ulGraveDone) return;
    if (u.state.ulGraveUntil) { if (c.t < u.state.ulGraveUntil) u.hp = 1; return; }
    u.state.ulGraveUntil = c.t + g;
    u.hp = 1;
    c.text(u, 'Grave');
    c.after(g, () => { if (u.alive) { u.state.ulGraveDone = true; c.damage(u, u, u.hp + 1e6, 'true'); } }, u);
  }

  const combatHooks = {
    onCombatStart(c) {
      for (const u of c.units) {
        if (u.line.type === 'ul_passive' && !isTamer(u)) { u.maxMana = 0; u.mana = 0; }
        const k = KIT[u.line.id];
        if (k && k.start) k.start(c, u);
      }
    },
    onCombatReady(c) {
      for (const u of c.units) { const k = KIT[u.line.id]; if (k && k.ready) k.ready(c, u); }
    },
    onTick(c) {
      for (const team of [0, 1]) {
        const zs = c.teamState(team).ulZones;
        if (!zs || !zs.length) continue;
        for (const z of zs) {
          if (c.t > z.until || c.t < z.next) continue;
          z.next = c.t + 0.5;
          for (const e of near(c, team, z, z.rad)) { c.damage(z.src, e, z.dps * 0.5, 'magic'); if (e.alive) armorCut(c, e, 'ulAcid', z.armor, 1); }
        }
        c.teamState(team).ulZones = zs.filter((z) => c.t <= z.until);
      }
    },
    onUnitTick(c, e) {
      const u = e.unit, s = u.state, dt = e.dt;
      // poison
      const p = s.ulPois;
      if (p && p.n) {
        if (c.t > p.until) { p.n = 0; if (p.buff) p.buff.v = 0; }
        else { p.acc += 4 * p.n * dt; if (p.acc >= 18) { const a = p.acc; p.acc = 0; c.damage(p.src, u, a, 'magic'); if (!u.alive) return; } }
      }
      // damage over time
      if (s.ulDots && s.ulDots.length) {
        for (const d of s.ulDots) {
          if (c.t > d.until) continue;
          d.acc += d.dps * dt;
          if (d.acc < 20) continue;
          const a = d.acc; d.acc = 0;
          const done = c.damage(d.src, u, a, 'magic');
          if (d.leech) for (const al of nearAllies(c, d.src.team, u, 1)) { c.heal(al, done, true); if (d.armor && !al.state.ulLivingArmor) { al.state.ulLivingArmor = true; al.armor += 10; } }
          if (!u.alive) return;
        }
        s.ulDots = s.ulDots.filter((d) => c.t <= d.until);
      }
      // Rejuvenating Ritual heal over time
      if (s.ulRitual && c.t <= s.ulRitual.until) c.heal(u, s.ulRitual.hps * dt, true);
      // Happy Hour: deferred damage paid over 10 s
      if (s.ulDefer > 0) {
        const pay = Math.min(s.ulDefer, (s.ulDeferRate || s.ulDefer / 10) * dt);
        s.ulDefer -= pay;
        s.ulDeferAcc = (s.ulDeferAcc || 0) + pay;
        if (s.ulDeferAcc >= 15) { const a = s.ulDeferAcc; s.ulDeferAcc = 0; s.ulPaying = true; c.damage(s.ulDeferSrc || u, u, a, 'true'); s.ulPaying = false; if (!u.alive) return; }
      }
      // Lich frost shield aura
      const fa = s.ulFrostAura;
      if (fa && c.t <= fa.until && c.t >= fa.next) {
        fa.next = c.t + 1;
        for (const x of near(c, u.team, u, 2)) { c.damage(fa.src, x, fa.dps, 'magic'); if (x.alive) slow(c, x, 'ulFrostSlow', 0.2, 1.2); }
      }
      // Death Prophet spirits
      const ex = s.ulExo;
      if (ex && ex.left > 0 && c.t >= ex.next && u.alive) {
        ex.next = c.t + 6 / 8;
        ex.left--;
        const list = near(c, u.team, u, 3);
        if (list.length) { const x = list[Math.floor(c.rng() * list.length)]; c.event({ type: 'shot', from: u.id, to: x.id }); const d = c.damage(u, x, ex.v, 'magic'); c.heal(u, d * 0.25, true); }
      }
      const k = KIT[u.line.id];
      if (k && k.tick) k.tick(c, u, dt);
    },
    onAttack(c, e) {
      const k = KIT[e.unit.line.id];
      if (k && k.attack) k.attack(c, e);
      const kt = KIT[e.target.line.id];
      if (kt && kt.struck) kt.struck(c, e);
    },
    onBeforeDamage(c, e) {
      const src = e.unit, tg = e.target;
      const ax = tg.state.ulAxed;
      if (ax && ax.by === src.id && ax.until > c.t) e.mult *= 1 + ax.amp;
      if (src.line.id === 'ul_terrorblade' && src.stage === 2 && tg.traits.includes('DEMON')) e.mult *= 1.6;
      if (e.kind === 'phys' && !e.isBurn) {
        if (src.state.ulRefractHits > 0) { e.mult *= 1 + src.state.ulRefractAmp; src.state.ulRefractHits--; }
        if (tg.state.ulStone > c.t) e.mult *= 1.2;
      }
    },
    onBeforeDamageTaken(c, e) {
      const tg = e.unit, src = e.source, s = tg.state;
      if (s.ulPaying) return;
      if (e.kind === 'magic' && s.ulMagicImmuneUntil > c.t) { e.mult = 0; return; }
      if (s.ulRefract > 0 && e.amount > 0 && !e.isBurn) { s.ulRefract--; e.mult = 0; c.text(tg, 'Blocked'); return; }
      if (e.kind === 'phys' && !e.isBurn) {
        if (s.ulEvadeUntil > c.t) { e.mult = 0; c.text(tg, 'Evade'); return; }
        if (tg.line.id === 'ul_queenofpain' && tg.stage === 2 && c.dist(tg, src) <= 2 && c.rng() < 0.25) { e.mult = 0; c.text(tg, 'Evade'); return; }
        const rg = s.ulRogue;
        if (rg && c.rng() < rg.ev) { e.mult = 0; tg.ad += rg.ad; c.text(tg, 'Evade'); return; }
      }
      if (s.ulBarrel) e.mult *= 1 + s.ulBarrel;
      if (src.summoned && tg.line.id === 'ul_beastmaster' && tg.stage === 2) e.mult *= 0.25;
      // Happy Hour: half of each hit is taken over the next 10 s
      if (s.ulHappyUntil > c.t && e.amount > 0) {
        const later = e.amount * e.mult * 0.5;
        e.mult *= 0.5;
        s.ulDefer = (s.ulDefer || 0) + later;
        s.ulDeferRate = s.ulDefer / 10;
        s.ulDeferSrc = src;
      }
    },
    onDamageTaken(c, e) {
      const u = e.unit;
      const ap = u.state.ulAphotic;
      if (ap && u.shield <= 0) {
        u.state.ulAphotic = null;
        if (c.t <= ap.until) {
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          for (const x of near(c, u.team, u, 2)) c.damage(ap.src, x, ap.v, 'magic');
        }
      }
      const k = KIT[u.line.id];
      if (k && k.hurt) k.hurt(c, u);
    },
    onLethal(c, e) { if (e.unit.hp <= 0) lethalSave(c, e.unit); },
    onDeath(c, e) {
      const u = e.unit;
      const k = KIT[u.line.id];
      if (k && k.death) k.death(c, u);
      // Pudge 3-star: Flesh Heap
      for (const p of c.units) {
        if (!p.alive || p.team === u.team || p.line.id !== 'ul_pudge' || p.stage !== 2 || c.dist(p, u) > 1) continue;
        p.maxHp += 300; p.hp += 300; p.mr += 10;
      }
      // Leech Seed jumps to a neighbor
      for (const d of u.state.ulDots || []) {
        if (!d.leech || c.t > d.until) continue;
        const nx = near(c, d.src.team, u, 1)[0];
        if (nx) addDot(c, d.src, nx, d.dps, d.until - c.t, { leech: true, armor: d.armor });
      }
    },
    onKill(c, e) {
      const k = e.unit, v = e.victim;
      k.state.ulKills = (k.state.ulKills || 0) + (v.summoned ? 0.25 : 1);
      if (!v.summoned && k.line.id === 'ul_bounty' && k.stage === 2) k.state.ulGold = (k.state.ulGold || 0) + 1;
      const d = k.state.ulDuel;
      if (d && d.with === v && c.t <= d.until && k.line.id === 'ul_legion') k.state.ulDuelWin = (k.state.ulDuelWin || 0) + [6, 10, 15][k.stage];
    },
    onCast(c, e) {
      const u = e.unit;
      const k = KIT[u.line.id];
      if (k && k.cast) k.cast(c, e);
      // Spectre 3-star: Demon Lords' casts set all your Demon Lords' targets ablaze
      if (u.traits.includes('DEMON') && c.units.some((s) => s.alive && s.team === u.team && s.line.id === 'ul_spectre' && s.stage === 2)) {
        for (const d of c.units) {
          if (!d.alive || d.team !== u.team || !d.traits.includes('DEMON') || !d.target || !d.target.alive) continue;
          c.damage(d, d.target, 150, 'magic');
          if (d.target.alive) c.burn(d, d.target);
        }
      }
    },
  };

  // Tamer unit of a player (board or bench)
  const tamerUnitOf = (p) => p.allUnits().find((u) => lineOf(u) && lineOf(u).tamer) || null;
  const tamerOnBoard = (p) => Object.values(p.board).some((u) => lineOf(u) && lineOf(u).tamer);

  // item pick: roll the tier with the round's odds (Underlords Season One table), Smuggler +1, Riches 4 choices
  function itemPool(p, tier, exclude) {
    return Object.values(DT.ITEMS).filter((it) => it.tier === tier && !it.component && !it.noDrop && !exclude.includes(it.id) && !(it.cat === 'global' && p.globals.includes(it.id))).map((it) => it.id);
  }
  function rollItemOffer(c) {
    const p = c.player, o = p.offer;
    if (!o || o.kind !== 'itemTier' || o.ulRolled) return;
    o.ulRolled = true;
    const odds = (c.R.ulItemOdds || {})[roundNo(c)];
    let tier = o.tier || 1;
    if (odds) {
      let x = c.rng() * 100;
      for (let i = 0; i < odds.length; i++) { if (x < odds[i]) { tier = i + 1; break; } x -= odds[i]; }
    }
    if (p.globals.includes('ul_g_smuggler')) tier = Math.min(5, tier + 1);
    const count = 3 + (p.globals.includes('ul_g_riches') ? 1 : 0);
    const pool = c.shuffle(itemPool(p, tier, []));
    if (pool.length) { o.tier = tier; o.options = pool.slice(0, count); }
  }

  const gameHooks = {
    onRoundStart(c) {
      const p = c.player, R = c.R;
      if (p.mech.ulFree) { p.freeRolls += 1; p.mech.ulFree = false; }
      if (R.ulStreakReset && p.streak >= R.ulStreakReset) p.streak = 0;
      if (R.ulGoldCap && p.gold > R.ulGoldCap) p.gold = R.ulGoldCap;
      rollItemOffer(c);
    },
    // one item per hero (Underlords rule); Tamers hold none (their lines have noItems)
    canEquip(c, e) { if (e.unit.items.length >= 1 && !e.combine) { e.ok = false; e.reason = 'full'; } },
    // heroes that digivolve keep one item; the rest go back to the item bench
    onCombine(c, e) { const u = e.unit; while (u.items.length > 1) c.give(u.items.pop()); },
    onIncome(c, e) {
      const p = c.player, R = c.R;
      if (p.streak < 0 && R.ulLossStreakCap != null) e.streak = Math.min(e.streak, R.ulLossStreakCap);
      if (R.ulKnockout) { const n = roundNo(c); e.base = 6 + Math.floor((n - 1) * 1.5); }
    },
    onRoundEnd(c, e) {
      const p = c.player, m = e.match;
      if (!e.won && !e.pve) p.mech.ulFree = true;
      if (!m || !m.combat) return;
      const mine = m.combat.units.filter((u) => u.team === e.team);
      let kills = 0, gold = 0, duel = 0;
      for (const u of mine) {
        if (u.traits.includes('BEAST') && !u.summoned) kills += u.state.ulKills || 0;
        gold += u.state.ulGold || 0;
        duel += u.state.ulDuelWin || 0;
      }
      if (kills) p.mech.ulBeastKills = (p.mech.ulBeastKills || 0) + kills;
      if (duel) p.mech.ulDuel = (p.mech.ulDuel || 0) + duel;
      e.gold += gold;
      // Divine Rapier: lost to a player while it was on the board -> that player takes it
      if (!e.won && !e.pve && !(m.ghost && e.team === 0)) {
        const opp = e.team === 0 ? m.b : m.a;
        const holder = Object.values(p.board).find((u) => u.items.includes('ul_rapier'));
        if (opp && holder && opp.alive !== false) {
          holder.items.splice(holder.items.indexOf('ul_rapier'), 1);
          c.game.giveItem(opp, 'ul_rapier');
        }
      }
    },
  };

  // ---------- offers: Tamer pick (round 10) and Knockout starter crew ----------
  const TAMER_IDS = ['anessa', 'hob', 'jules', 'enno'];
  const BUILDS = { anessa: ['dark', 'heal'], hob: ['fire', 'crazy'], jules: ['brawl', 'bar'], enno: ['balm', 'rabid'] };
  const offerKinds = {
    ul_tamer: {
      name: 'Pick your Tamer',
      sub: 'Your Tamer stands on the board, uses abilities on cooldowns and ranks up every 5 rounds.',
      options: (c) => TAMER_IDS.map((t) => `ul_${t}_${BUILDS[t][Math.floor(c.rng() * 2)]}`),
      choose: (c, id) => { if (DT.POWER[id] && !c.player.power) c.game.applyPower(c.player, id); },
      aiScore: (c, id) => {
        const P = DT.POWER[id];
        if (!P) return 0;
        const counts = DT.countTraits(c.player.boardUnits());
        let s = 0;
        if (P.tamer === 'anessa') s += (counts.DEMON || 0) * 0.6 + (counts.SUMMONER || 0) * 0.3;
        if (P.tamer === 'hob') s += (counts.WIZARD || 0) * 0.3 + (counts.JUNGLE || 0) * 0.4;
        if (P.tamer === 'jules') s += (counts.KNIGHT || 0) * 0.3 + (counts.METAL || 0) * 0.3;
        if (P.tamer === 'enno') s += (counts.TOXIC || 0) * 0.6 + (counts.HOLY || 0) * 0.3;
        return s;
      },
      card: (id) => {
        const P = DT.POWER[id] || { name: id, desc: '' };
        return { name: P.name, desc: P.desc, img: DT.ItemArt ? DT.ItemArt.img(`aug:${id}`, 64, 'augart') : '' };
      },
    },
    ul_crew: {
      name: 'Starter Crew',
      sub: 'Pick a crew of 5 Digimon from two alliances.',
      options: (c, exclude = []) => {
        const al = Object.keys(DT.TRAITS).filter((t) => DT.SHOP_LINES.filter((l) => l.tier <= 2 && l.traits.includes(t)).length >= 2);
        const out = [];
        for (let guard = 0; out.length < 3 && guard < 40; guard++) {
          const a = al[Math.floor(c.rng() * al.length)], b = al[Math.floor(c.rng() * al.length)];
          const id = [a, b].sort().join('+');
          if (a === b || out.includes(id) || exclude.includes(id)) continue;
          out.push(id);
        }
        return out;
      },
      choose: (c, id) => {
        const [a, b] = String(id).split('+');
        if (!DT.TRAITS[a] || !DT.TRAITS[b]) return;
        const g = c.game, p = c.player;
        const lines = c.shuffle(DT.SHOP_LINES.filter((l) => l.tier <= 2 && (l.traits.includes(a) || l.traits.includes(b)) && g.pool[l.id] > 0).map((l) => l.id));
        let n = 0;
        for (const lid of lines) {
          if (n >= 5) break;
          const free = p.freeBenchSlot();
          if (free < 0) break;
          g.takeFromPool(lid);
          p.bench[free] = g.newUnit(lid, 0);
          n++;
        }
        g.combine(p);
      },
      aiScore: () => 0,
      card: (id) => {
        const [a, b] = String(id).split('+');
        const na = DT.TRAITS[a] ? DT.TRAITS[a].name : a, nb = DT.TRAITS[b] ? DT.TRAITS[b].name : b;
        return { name: `${na} + ${nb}`, desc: `Five Digimon (cost 1-2) from the ${na} and ${nb} alliances.` };
      },
    },
  };

  // ---------- Tamers: powers ----------
  const R6 = (c) => Math.max(0, Math.min(5, (c.rank || 1) - 1));
  function cd(c, u, key, period, frac) {
    const s = c.state;
    if (s[key] == null) s[key] = period * frac;
    if (c.t < s[key] || !canAct(c, u)) return false;
    s[key] = c.t + period;
    return true;
  }
  const castFx = (c, u, name) => { c.event({ type: 'cast', at: u.id, name }); c.text(u, name); };

  const powerGame = {
    onPick(c) {
      const p = c.player, P = c.P, Tm = TAMERS[P.tamer];
      if (!Tm || tamerUnitOf(p)) return;
      const unit = c.game.newUnit(Tm.line, 0);
      const cell = c.game.freeCellFor(p, unit);
      if (cell) p.board[DT.cellKey(cell.c, cell.r)] = unit;
      else { const free = p.freeBenchSlot(); if (free >= 0) p.bench[free] = unit; }
    },
    onRankUp(c, e) {
      const u = tamerUnitOf(c.player);
      if (u) u.stage = Math.min(2, Math.floor((e.rank - 1) / 2));
    },
    onTeamSize(c, e) { if (tamerOnBoard(c.player)) e.size += 1; },
  };

  function tamerSetup(c) {
    const u = c.mine.find((x) => isTamer(x));
    c.state.tamer = u || null;
    if (!u) return null;
    const Tm = TAMERS[u.line.tamer], r = R6(c);
    u.items = []; u.P = {}; u.lifelines = []; u.hooks = null;
    u.ap = 100; u.crit = 0; u.critDmg = 1.6; u.dodge = 0; u.omnivamp = 0; u.amp = 0; u.dr = 0; u.regen = 0;
    u.maxHp = Tm.hp[r]; u.ad = Tm.ad[r]; u.baseAd = Tm.ad[r]; u.armor = Tm.armor[r]; u.mr = Tm.mr;
    u.maxMana = 0; u.mana = 0;
    return u;
  }
  const mineTamer = (c, u) => c.state.tamer && u === c.state.tamer;

  const BUILD_HOOKS = {
    // ---- Anessa ----
    anessa(build) {
      return {
        onCombatStart: {
          order: -50,
          fn(c) {
            const u = tamerSetup(c);
            if (!u) return;
            const r = R6(c);
            const a = summon(c, c.team, 'ul_s_archer', Math.floor(r / 2), u, { scale: 1 + 0.15 * (r % 2) });
            if (a) { a.state.ulOwner = u.id; c.state.archer = a; }
          },
        },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u)) return;
          const r = R6(c);
          if (build === 'dark') {
            if (cd(c, u, 'a1', 8, 0.5)) {
              castFx(c, u, 'The End of Medicine');
              const x = [0.4, 0.43, 0.46, 0.49, 0.52, 0.55][r];
              for (const a of allies(c, u)) { c.buff(a, 'ad', a.baseAd * x, 6); }
            }
            if (cd(c, u, 'a2', 8, 1)) {
              const tg = c.target(u, 'closest');
              if (tg) {
                castFx(c, u, 'Enthrall');
                c.event({ type: 'beam', from: u.id, to: tg.id });
                c.buff(tg, 'armor', -30, 6);
                tg.state.ulThrall = { team: u.team, until: c.t + 6 };
                for (const a of allies(c, u)) if (c.dist(a, tg) <= a.range + 2) a.target = tg;
              }
            }
          } else {
            if (cd(c, u, 'a1', 8, 0.5)) {
              castFx(c, u, 'Rejuvenating Ritual');
              u.hp = Math.max(1, u.hp - u.maxHp * 0.05);
              const now = [75, 110, 140, 170, 205, 230][r], hps = [10, 15, 21, 27, 33, 38][r];
              for (const a of allies(c, u)) { c.heal(a, now); a.state.ulRitual = { until: c.t + 6, hps }; }
            }
            if (cd(c, u, 'a2', 15, 1)) {
              const tg = c.target(u, 'farthest');
              if (tg) {
                castFx(c, u, 'Summon Demonic Golem');
                const cell = c.freeCellNear(tg, u);
                const gl = summon(c, u.team, 'ul_s_golem', Math.floor(r / 2), tg, { cell: cell || undefined, scale: 1 + 0.12 * (r % 2) });
                if (gl) {
                  c.after(0.25, () => {
                    if (!gl.alive) return;
                    c.event({ type: 'burst', c: gl.c, r: gl.r, radius: 2, team: u.team });
                    for (const x of near(c, u.team, gl, 2)) c.stun(x, 2);
                  });
                }
              }
            }
          }
        },
        onDeath(c, e) {
          if (!mineTamer(c, e.unit)) return;
          const a = c.state.archer;
          if (a && a.alive) c.damage(a, a, a.hp + 1e6, 'true');
        },
        onAnyDeath(c, e) {
          const v = e.unit, th = v.state.ulThrall;
          if (build !== 'dark' || !th || th.team !== c.team || c.t > th.until || v.team === c.team || v.summoned) return;
          v.state.ulThrall = null;
          const cp = c.summon(c.team, v.line.id, v.stage, v.c, v.r, {});
          if (cp) { cp.traits = cp.traits.includes('DEMON') ? cp.traits : cp.traits.concat(['DEMON']); c.text(cp, 'Enthralled'); }
        },
      };
    },
    // ---- Hob ----
    hob(build) {
      return {
        onCombatStart: { order: -50, fn(c) { tamerSetup(c); } },
        onAttack(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u) || !e.target.alive) return;
          c.burn(u, e.target);
          if (u.state.ulCrazyUntil > c.t) {
            const cool = foes(c, u).find((x) => !(x.burn && x.burn.until > c.t));
            if (cool) u.target = cool;
          }
        },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u)) return;
          const r = R6(c);
          if (u.state.ulCrazyUntil && c.t > u.state.ulCrazyUntil) { u.state.ulCrazyUntil = 0; u.range = 3; }
          if (cd(c, u, 'h1', 12, 0.5)) {
            const tg = u.target && u.target.alive ? u.target : c.target(u, 'closest');
            if (tg) {
              const fire = build === 'fire';
              castFx(c, u, fire ? 'Implosion' : 'Shockwave');
              const total = fire ? [640, 820, 1000, 1200, 1380, 1560][r] : [420, 560, 700, 840, 980, 1120][r];
              const grp = near(c, u.team, tg, 1);
              c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team });
              for (const x of grp) { c.damage(u, x, total / grp.length, 'magic'); if (!x.alive) continue; c.burn(u, x); if (!fire) c.stun(x, 1.5); }
            }
          }
          if (build === 'fire' && cd(c, u, 'h2', 13, 1)) {
            castFx(c, u, 'Friendly Fire');
            const dmg = [160, 190, 225, 270, 325, 375][r];
            for (let k = 0; k < 8; k++) {
              const list = foes(c, u);
              if (!list.length) break;
              const at = list[Math.floor(c.rng() * list.length)];
              c.event({ type: 'burst', c: at.c, r: at.r, radius: 1, team: u.team });
              for (const x of near(c, u.team, at, 1)) { c.damage(u, x, dmg, 'magic'); if (x.alive) c.burn(u, x); }
            }
          }
          if (build === 'crazy' && cd(c, u, 'h2', 10, 1)) {
            castFx(c, u, "Let's Go Crazy");
            u.state.ulCrazyUntil = c.t + 6;
            u.range = 10;
            const x = [0.3, 0.42, 0.54, 0.66, 0.78, 0.9][r];
            for (const a of allies(c, u)) c.buff(a, 'as', x, 6);
          }
        },
      };
    },
    // ---- Jules ----
    jules(build) {
      return {
        onCombatStart: { order: -50, fn(c) { const u = tamerSetup(c); if (u) u.state.ulBaseArmor = [u.armor, u.mr]; } },
        onBeforeDamageTaken(c, e) {
          const u = e.unit;
          if (mineTamer(c, u) && u.state.ulGuardUntil > c.t) e.dr += 0.5;
        },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u) || !(u.state.ulToastUntil > c.t)) return;
          for (const a of nearAllies(c, u.team, u, 1)) if (a !== u) c.heal(a, e.amount, true);
        },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u)) return;
          const r = R6(c);
          if (c.every(u, 'ulJulesArmor', 0.5)) {
            const n = c.adjacent(u).filter((x) => x.alive).length;
            const [ba, bm] = u.state.ulBaseArmor || [u.armor, u.mr];
            u.armor = ba + 10 * n; u.mr = bm + 10 * n;
          }
          if (build === 'brawl') {
            if (cd(c, u, 'j1', 8, 0.5)) {
              castFx(c, u, 'Roundhouse');
              u.state.ulGuardUntil = c.t + 5;
              c.heal(u, u.maxHp * 0.1);
              for (const x of near(c, u.team, u, 2)) x.target = u;
              c.buff(u, 'as', 1, 5);
              c.buff(u, 'omnivamp', 0.6, 5);
            }
            if (cd(c, u, 'j2', 12, 1)) {
              castFx(c, u, 'Barrels of Fun');
              const pickN = (list, n) => { const out = []; const pool = list.slice(); while (out.length < n && pool.length) out.push(pool.splice(Math.floor(c.rng() * pool.length), 1)[0]); return out; };
              for (const a of pickN(allies(c, u), 4)) c.buff(a, 'as', 0.5, 4);
              for (const x of pickN(foes(c, u), 4)) { x.state.ulBarrel = 0.25; c.event({ type: 'shot', from: u.id, to: x.id, big: true }); }
            }
          } else {
            if (cd(c, u, 'j1', 10, 0.5)) {
              castFx(c, u, 'To Your Health');
              u.state.ulGuardUntil = c.t + 5;
              u.state.ulToastUntil = c.t + 5;
            }
            if (cd(c, u, 'j2', 10, 1)) {
              castFx(c, u, 'Happy Hour');
              for (const a of allies(c, u)) { a.state.ulHappyUntil = c.t + 7; c.event({ type: 'shield', at: a.id }); }
            }
          }
        },
      };
    },
    // ---- Enno ----
    enno(build) {
      return {
        onCombatStart: {
          order: -50,
          fn(c) {
            const u = tamerSetup(c);
            if (!u || build !== 'balm') return;
            // Yoink!: the stats of the enemy's lowest-tier item move to Enno for this combat
            let best = null, bh = null;
            for (const e of c.units) {
              if (e.team === c.team) continue;
              for (const id of e.items) {
                const it = DT.ITEMS[id];
                if (!it || it.hat || it.cat === 'global' || !it.stats || !Object.keys(it.stats).length) continue;
                if (!best || it.tier < best.tier) { best = it; bh = e; }
              }
            }
            if (!best) return;
            const st = best.stats;
            const apply = (x, sign) => {
              if (st.ad) x.ad += sign * x.baseAd * st.ad;
              if (st.as) x.as *= sign > 0 ? 1 + st.as : 1 / (1 + st.as);
              for (const k of ['ap', 'armor', 'mr', 'crit', 'dodge', 'omnivamp']) if (st[k]) x[k] += sign * st[k];
              if (st.hp) x.maxHp = Math.max(1, x.maxHp + sign * st.hp);
            };
            apply(bh, -1);
            apply(u, 1);
            c.text(u, 'Yoink!');
          },
        },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u) || !u.alive) return;
          const s = u.state;
          const marks = R6(c) >= 3 ? [0.5, 0.25] : [0.5];
          s.ulEsc = s.ulEsc || 0;
          if (s.ulEsc >= marks.length || u.hp >= u.maxHp * marks[s.ulEsc]) return;
          s.ulEsc++;
          castFx(c, u, 'Escape Artist');
          u.stun = 0; u.burn = null; u.weak = 0; u.silenceUntil = 0;
          if (s.ulPois) { s.ulPois.n = 0; if (s.ulPois.buff) s.ulPois.buff.v = 0; }
          let best = null, bd = -1;
          for (const cell of DT.CELLS) {
            if (c.unitAt(cell.c, cell.r)) continue;
            const fs = foes(c, u);
            const d = fs.length ? Math.min(...fs.map((x) => c.dist(cell, x))) : 0;
            if (d > bd && d <= 3) { bd = d; best = cell; }
          }
          if (best) moveTo(c, u, best, 0.3);
          for (const x of near(c, u.team, u, 1)) c.stun(x, 1.5);
        },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!mineTamer(c, u)) return;
          const r = R6(c);
          if (cd(c, u, 'e1', 3, 0.5)) {
            const n = [3, 4, 4, 5, 5, 5][r];
            const list = near(c, u.team, u, 3).sort((a, b) => c.dist(u, a) - c.dist(u, b) || a.id - b.id).slice(0, n);
            if (list.length) {
              castFx(c, u, build === 'balm' ? 'Soothing Balm' : 'Death Spin');
              for (const x of list) {
                c.event({ type: 'shot', from: u.id, to: x.id });
                addPoison(c, u, x, 1);
                if (build === 'rabid') c.damage(u, x, [25, 25, 35, 35, 45, 45][r] * poisonOf(x), 'phys');
                else {
                  let h = [35, 35, 45, 45, 55, 55][r];
                  for (const a of allies(c, u).sort((p, q) => p.hp / p.maxHp - q.hp / q.maxHp || p.id - q.id)) { if (h < 5) break; c.heal(a, h, true); h /= 2; }
                }
              }
            }
          }
          if (build === 'rabid' && cd(c, u, 'e2', 10, 1)) {
            castFx(c, u, 'All Out Attack!');
            for (const x of foes(c, u)) { addPoison(c, u, x, 3); c.damage(u, x, [45, 65, 90, 110, 130, 155][r], 'phys'); if (x.alive) c.stun(x, 1.1); }
          }
        },
      };
    },
  };

  const powers = {};
  for (const t of TAMER_IDS) for (const b of BUILDS[t]) powers[`ul_${t}_${b}`] = { hooks: { ...powerGame, ...BUILD_HOOKS[t](b) } };

  // ---------- items ----------
  const rowMates = (c, u) => c.units.filter((a) => a.alive && a.team === u.team && a.r === u.r && Math.abs(a.c - u.c) <= 1);
  const itemHooks = {
    ul_headdress: { onCombatStart(c, e) { for (const a of rowMates(c, e.unit)) a.regen += (20 * c.count) / Math.max(1, a.maxHp); } },
    ul_hood: { onCombatStart(c, e) { for (const a of rowMates(c, e.unit)) a.mr += 25 * c.count; } },
    ul_vitality: { onCombatStart(c, e) { for (const a of rowMates(c, e.unit)) a.maxHp += 200 * c.count; } },
    ul_arcane: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (!c.every(u, 'ulArcane', 10) || !(u.mana > u.maxMana / 2)) return;
        for (const a of nearAllies(c, u.team, u, 2)) if (a.maxMana > 0) a.mana = Math.min(a.maxMana, a.mana + 20);
      },
    },
    ul_armlet: { onUnitTick(c, e) { const u = e.unit; u.hp = Math.max(1, u.hp - u.maxHp * 0.03 * c.count * e.dt); } },
    ul_blademail: {
      onDamageTaken(c, e) {
        const s = e.source;
        if (!s || !s.alive || s.team === e.unit.team || e.isBurn || e.unit.state.ulReflect) return;
        s.state.ulReflect = true;
        c.damage(e.unit, s, e.amount * 0.2 * c.count, 'true');
        s.state.ulReflect = false;
      },
    },
    ul_blink: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (c.t < 0.5 || !canAct(c, u)) return;
        if (u.state.ulBlinkAt == null) u.state.ulBlinkAt = 0.5;
        if (c.t < u.state.ulBlinkAt) return;
        u.state.ulBlinkAt = c.t + 15;
        const far = c.target(u, 'farthest');
        if (far && moveTo(c, u, c.freeCellNear(far, u), 0.2)) { u.target = far; if (u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + 45); }
      },
    },
    ul_desolator: { onAttack(c, e) { if (e.target.alive) armorCut(c, e.target, 'ulDeso', 15, 5); } },
    ul_lance: { onCombatStart(c, e) { const u = e.unit; if (u.range > 1) u.range += 3; else u.ad -= u.baseAd * 0.25 * c.count; } },
    ul_halberd: { onAttack(c, e) { if (e.target.alive && c.every(e.unit, 'ulHalberd', 10)) slow(c, e.target, 'ulDisarm', 0.6, 3); } },
    ul_paladin: { onCombatStart(c, e) { if (!e.unit.state.ulPaladin) { e.unit.state.ulPaladin = true; const ts = c.teamState(e.unit.team); ts.healAmp = (ts.healAmp || 0) + 0.2; } } },
    ul_quelling: { onBeforeDamage(c, e) { if (e.target.summoned) e.mult *= 2; } },
    ul_stonecloak: { onKill(c, e) { e.unit.maxHp += 100 * c.count; c.heal(e.unit, 100 * c.count); } },
    ul_stonepike: { onKill(c, e) { e.unit.ad += e.unit.baseAd * 0.1 * c.count; } },
    ul_vanguard: {
      onBeforeDamageTaken(c, e) {
        if (e.kind !== 'phys' || e.isBurn || e.amount <= 0 || c.rng() >= 0.5) return;
        e.mult *= Math.max(0, (e.amount - 40 * c.count) / e.amount);
      },
    },
    ul_battlefury: { onCombatStart(c, e) { const u = e.unit; if (u.range > 1) { u.P.split = Math.max(0, (u.P.split || 0) - 0.5 * c.count); u.ad -= u.baseAd * 0.35 * c.count; } } },
    ul_euls: {
      onAnyAttack(c, e) {
        const h = c.unit;
        if (e.target !== h || !h.alive || e.unit.team === h.team || !e.unit.alive) return;
        if ((h.state.ulEulCd || 0) > c.t) return;
        h.state.ulEulCd = c.t + 10;
        c.stun(e.unit, 2.5);
        e.unit.invulnUntil = c.t + 2.5;
        c.text(e.unit, 'Cyclone');
      },
    },
    ul_madness: { onCombatStart(c, e) { e.unit.maxMana = 0; e.unit.mana = 0; } },
    ul_necro: {
      onCast(c, e) {
        const u = e.unit;
        const mine = c.units.filter((b) => b.alive && b.state.ulNecroOf === u.id).length;
        if (mine >= 2) return;
        const b = summon(c, u.team, 'ul_s_bruiser', u.stage, u);
        if (b) b.state.ulNecroOf = u.id;
      },
    },
    ul_octarine: { onCombatStart(c, e) { const u = e.unit; if (u.maxMana > 0) u.maxMana = Math.max(20, Math.round(u.maxMana * Math.pow(0.7, c.count))); } },
    ul_pipe: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (c.t < 3 || !once(u, 'ulPipe')) return;
        for (const a of nearAllies(c, u.team, u, 2)) shieldUnit(c, a, 250);
      },
    },
    ul_silveredge: { onAttack(c, e) { if (e.target.alive && c.every(e.unit, 'ulSilver', 5)) c.weaken(e.target, 0.25, 3); } },
    ul_basher: {
      onAttack(c, e) {
        const u = e.unit;
        if (!e.target.alive || (u.state.ulBashItemCd || 0) > c.t || c.rng() >= 0.25) return;
        u.state.ulBashItemCd = c.t + 2;
        c.damage(u, e.target, 100, 'phys');
        if (e.target.alive) c.stun(e.target, 1.5);
      },
    },
    ul_shako: { onCombatStart(c, e) { const u = e.unit; if (u.maxMana > 0) u.maxMana = Math.round(u.maxMana * 2); } },
    ul_dagon: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (!c.every(u, 'ulDagon', 12)) return;
        const list = foes(c, u).filter((x) => x.hp <= x.maxHp * 0.5);
        if (!list.length) { u.tm.ulDagon = c.t + 1; return; }
        const x = list[Math.floor(c.rng() * list.length)];
        c.event({ type: 'beam', from: u.id, to: x.id });
        c.damage(u, x, 600 * c.count, 'magic');
      },
    },
    ul_diffusal: {
      onAttack(c, e) {
        const tg = e.target;
        if (!tg.alive) return;
        if (tg.maxMana > 0) tg.mana = Math.max(0, tg.mana - 15 * c.count);
        c.damage(e.unit, tg, 30 * c.count, 'phys');
      },
    },
    ul_skadi: { onAttack(c, e) { if (e.target.alive) slow(c, e.target, 'ulSkadi', 0.4, 2.5); } },
    ul_kaden: {
      onBeforeDamage(c, e) { if (e.kind === 'phys' && !e.isBurn) e.mult *= 0.5; },
      onAttack(c, e) { if (e.target.alive) c.damage(e.unit, e.target, e.target.maxHp * 0.06 * c.count, 'true'); },
    },
    ul_maelstrom: {
      onAttack(c, e) {
        const u = e.unit;
        if (c.rng() >= 0.25) return;
        const hit = [];
        let cur = e.target;
        for (let k = 0; k < 4 && cur; k++) {
          hit.push(cur);
          c.event({ type: 'beam', from: u.id, to: cur.id });
          c.damage(u, cur, 110 * (cur.summoned ? 1.5 : 1), 'magic');
          let next = null, nd = Infinity;
          for (const x of foes(c, u)) { if (hit.includes(x)) continue; const d = c.dist(cur, x); if (d <= 3 && d < nd) { nd = d; next = x; } }
          cur = next;
        }
      },
    },
    ul_mekansm: {
      onDamageTaken(c, e) {
        const u = e.unit;
        if (u.hp >= u.maxHp * 0.5 || !once(u, 'ulMek')) return;
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 3, team: 1 - u.team });
        for (const a of nearAllies(c, u.team, u, 3)) c.heal(a, 500);
      },
    },
    ul_refresher: { onCast(c, e) { const u = e.unit; if (u.maxMana > 0 && once(u, 'ulRefresh')) { u.mana = u.maxMana; c.text(u, 'Refresh'); } } },
    ul_vyse: {
      onDamageTaken(c, e) {
        const u = e.unit, s = e.source;
        if (!s || !s.alive || s.team === u.team || (u.state.ulVyseCd || 0) > c.t) return;
        u.state.ulVyseCd = c.t + 15;
        c.stun(s, 2.5);
        c.text(s, 'Hexed');
      },
    },
    ul_bloodthorn: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (!c.every(u, 'ulBloodthorn', 10)) return;
        const x = foes(c, u).filter((f) => f.maxMana > 0).sort((a, b) => b.mana - a.mana || a.id - b.id)[0];
        if (x) c.silence(x, 4);
      },
      onBeforeDamage(c, e) { if (e.target.silenceUntil > c.t) e.mult *= 1.4; },
    },
    ul_horn: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (c.t < 2 || !once(u, 'ulHorn')) return;
        const th = summon(c, u.team, 'ul_s_thunderhide', 1, u);
        if (th) c.text(th, 'Thunderhide');
      },
    },
    ul_radiance: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (!c.every(u, 'ulRadiance', 1)) return;
        for (const x of near(c, u.team, u, 2)) { c.damage(u, x, 50 * c.count, 'magic'); if (x.alive && c.rng() < 0.17) x.blind = true; }
      },
    },
    ul_satanic: {
      onDamageTaken(c, e) {
        const u = e.unit;
        if (u.hp >= u.maxHp * 0.5 || (u.state.ulSatanicCd || 0) > c.t) return;
        u.state.ulSatanicCd = c.t + 10;
        c.buff(u, 'omnivamp', 1.5, 3);
        c.text(u, 'Satanic');
      },
    },
    ul_shiva: {
      onUnitTick(c, e) {
        const u = e.unit;
        if (c.t < 2 || !once(u, 'ulShiva')) return;
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 3, team: u.team });
        for (const x of near(c, u.team, u, 3)) { c.damage(u, x, 250, 'magic'); if (x.alive) slow(c, x, 'ulShivaSlow', 0.4, 4); }
      },
    },
    ul_vesture: { onCombatStart(c, e) { for (const a of nearAllies(c, e.unit.team, e.unit, 1)) a.state.ulGrave = Math.max(a.state.ulGrave || 0, 6); } },
    ul_vladmir: {
      onCombatStart(c, e) {
        if (e.unit.state.ulVlad) return;
        e.unit.state.ulVlad = true;
        for (const a of nearAllies(c, e.unit.team, e.unit, 2)) { a.amp += 0.08; a.armor += 15; a.omnivamp += 0.1; }
      },
    },
    // ---- global items ----
    ul_g_silver: {
      onRoundEnd(c, e) {
        if (e.pve) return;
        const m = c.player.mech;
        if (e.won) m.ulSilver = Math.min(3, (m.ulSilver || 0) + 1);
        else if (m.ulSilver > 0) { m.ulSilver--; e.gold += 2; }
      },
    },
    ul_g_aegis: {
      onRoundEnd(c, e) {
        const p = c.player;
        if (e.won || !e.match || !e.match.combat) return;
        const dmg = c.game.lossFor(e.match.combat, 1 - e.team, e.pve);
        if (p.hp - dmg > 0) return;
        p.hp += dmg;
        p.globals.splice(p.globals.indexOf('ul_g_aegis'), 1);
      },
    },
    ul_g_vicious: {
      onRoundEnd(c, e) {
        const m = e.match;
        if (!e.won || e.pve || !m || !m.combat || (m.ghost && e.team === 0)) return;
        const opp = e.team === 0 ? m.b : m.a;
        if (!opp) return;
        const n = m.combat.units.filter((u) => u.team === e.team && u.alive && !u.summoned).length;
        opp.hp -= n;
      },
    },
    ul_g_ristul: { onCombatStart(c) { for (const t of [0, 1]) { const ts = c.teamState(t); ts.healAmp = (ts.healAmp || 0) - 0.4; } } },
    ul_g_assault: {
      onCombatStart: {
        order: 20,
        fn(c) {
          for (const j of c.mine) if (j.traits.includes('JUNGLE')) for (const a of c.adjacent(j)) if (a.team === c.team && a !== j) a.as *= 1.25;
        },
      },
    },
    ul_g_chivalry: {
      onBeforeDamageTaken(c, e) {
        const u = e.unit;
        if (u.team !== c.team || u.traits.includes('KNIGHT')) return;
        const n = c.adjacent(u).filter((a) => a.alive && a.team === u.team && a.traits.includes('KNIGHT')).length;
        if (n) e.dr += 0.1 * n;
      },
    },
    ul_g_recruiter: { onRoundStart(c) { c.player.freeRolls += 2; } },
    ul_g_roster: { onTeamSize(c, e) { e.size += 1; } },
    ul_g_criminal: { onShop(c, e) { e.level = Math.min(10, e.level + 1); } },
  };
  // AI interest in global items (they compete with the items for its carries)
  const globalAi = {
    ul_g_riches: (c) => (c.stage <= 3 ? 6 : 2),
    ul_g_silver: (c) => (c.player.streak < 0 ? 6 : 3),
    ul_g_aegis: (c) => (c.player.hp < c.R.startHp / 2 ? 8 : 3),
    ul_g_stone: (c) => DT.countTraits(c.player.boardUnits()).SUMMONER >= 2 ? 8 : DT.countTraits(c.player.boardUnits()).SHAMAN >= 2 ? 6 : 1,
    ul_g_vicious: () => 4,
    ul_g_ristul: () => 3,
    ul_g_assault: (c) => (DT.countTraits(c.player.boardUnits()).JUNGLE >= 2 ? 8 : 0),
    ul_g_chivalry: (c) => (DT.countTraits(c.player.boardUnits()).KNIGHT >= 2 ? 8 : 0),
    ul_g_recruiter: () => 5,
    ul_g_smuggler: (c) => (c.stage <= 6 ? 5 : 1),
    ul_g_roster: () => 3,
    ul_g_criminal: () => 2,
  };
  const items = {};
  for (const [id, hooks] of Object.entries(itemHooks)) items[id] = { hooks };
  for (const [id, f] of Object.entries(globalAi)) items[id] = { ...(items[id] || {}), ai: { bonus: f } };

  // ---------- line text for passives ----------
  const lines = {};
  for (const [id, k] of Object.entries(KIT)) if (k.text) lines[id] = { abilText: (stage) => k.text(stage) };
  for (const [id, f] of Object.entries(PASSIVE_TEXT)) lines[id] = { abilText: f };

  const mechanic = {
    id: 'tamers',
    name: 'Tamers',
    desc: 'At round 10 each player picks a Tamer (offered with one of two builds). The Tamer fights on the board without using a team slot, uses abilities on cooldowns and ranks up every 5 rounds. Losing to a player gives a free reroll next round.',
    hooks: { ...combatHooks, ...gameHooks, onCombatStart: { order: 5, fn: combatHooks.onCombatStart } },
    offerKinds,
  };
  // Legion Commander's permanent duel damage (stored per player in player.mech)
  const legionReady = (c) => {
    for (const team of [0, 1]) {
      const side = c.side(team);
      const bonus = (side && side.mech && side.mech.ulDuel) || 0;
      if (bonus) for (const u of c.units) if (u.team === team && u.line.id === 'ul_legion' && !u.summoned) { u.ad += bonus; u.baseAd += bonus; }
    }
  };
  mechanic.hooks.onCombatReady = { order: 0, fn(c, e) { legionReady(c); combatHooks.onCombatReady(c, e); } };

  for (const id of SETS) DT.Sets.extend(id, { traits, abilities: A, powers, items, lines, mechanic });
})(typeof window !== 'undefined' ? window : globalThis);
