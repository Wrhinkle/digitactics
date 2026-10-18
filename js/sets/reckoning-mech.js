// Reckoning behavior: trait, item, augment and consumable hooks, the custom abilities (hook, summon, possess,
// respite), and the set mechanic: Shadow items (Spore components make stronger items with a drawback), Dragon Kin
// eggs, the Radiant Armory (4-1) and the Radiant Blessing (once, when a Tamer drops to rules.blessAt HP).
// Hook contexts are documented in docs/sets/FORMAT.md. Everything random uses c.rng() / c.pick().
(function (G) {
  const DT = G.DT;
  const SID = 'reckoning';
  const defItems = () => DT.Sets.def(SID).items;
  const R = () => DT.HYPER;

  // ---------- shared helpers ----------
  // Shadow drawbacks are halved by the X-Antibody augment.
  const dbk = (c) => { const s = c.side && c.side(c.team); return s && s.augs && s.augs.includes('rec_a_xanti') ? 0.5 : 1; };
  // HP cost from a drawback: never knocks the unit out (stops at 1 HP)
  function hurt(c, u, amt) {
    if (!u.alive || !(amt > 0)) return;
    u.hp = Math.max(1, u.hp - amt);
    u.taken += amt;
    c.event({ type: 'dmg', at: u.id, amount: Math.round(amt), kind: 'true' });
  }
  const frail = (c, f) => { const u = c.unit; u.maxHp = Math.round(u.maxHp * (1 - f * dbk(c))); };
  const isShadow = (id) => !!(DT.ITEMS[id] && DT.ITEMS[id].cat === 'shadow');
  const rowMates = (c, u, range, self = true) => c.mine.filter((a) => a.alive && a.r === u.r && c.dist(a, u) <= range && (self || a !== u));
  const lowestAlly = (c, u, notSelf) => {
    let best = null;
    for (const a of c.units) if (a.alive && a.team === u.team && (!notSelf || a !== u) && (!best || a.hp / a.maxHp < best.hp / best.maxHp)) best = a;
    return best;
  };
  const pickN = (c, list, n) => { const pool = list.slice(); const out = []; while (pool.length && out.length < n) out.push(pool.splice(Math.floor(c.rng() * pool.length), 1)[0]); return out; };
  // remove one specific buff object
  const unbuff = (u, b) => { u.buffs = u.buffs.filter((x) => x !== b); };

  // ---------- traits ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });

  // Sentinel: the shield that jumps
  // the shield's attack speed sits on its current holder and grows with every jump (up to 5)
  function sentinelGive(c, u) {
    const s = c.state;
    const amt = c.v * (1 + 0.5 * u.stage);
    c.shield(u, amt);
    if (s.holder && s.buff) unbuff(s.holder, s.buff);
    s.jumps = Math.min(5, (s.jumps || 0) + 1);
    s.buff = { stat: 'as', v: c.val('as') * s.jumps, until: Infinity };
    u.buffs.push(s.buff);
    s.holder = u; s.given = amt; s.shAt = u.shield; s.until = c.t + c.val('dur'); s.min = c.t + c.val('dur') * 0.75;
    c.text(u, 'Sentinel');
  }

  const traits = {
    DSP: {
      hooks: {
        onCombatStart(c) {
          let k = 0;
          for (const m of c.members) for (const id of m.items) if (isShadow(id)) k++;
          const f = c.v * (1 + 0.2 * Math.min(4, k));
          for (const u of c.members) { u.ad += u.baseAd * f; u.ap += Math.round(f * 100); }
        },
      },
    },
    TRK: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.as *= 1 + c.v; u.state.trk = c.v; } },
        onDeath(c, e) {
          const u = e.unit;
          if (!u.state.trk || u.summoned) return;
          const cell = c.unitAt(u.c, u.r) ? c.freeCellNear(u) : { c: u.c, r: u.r };
          if (!cell) return;
          const s = c.summon(u.team, u.line.id, Math.max(0, u.stage - 1), cell.c, cell.r);
          if (!s) return;
          s.maxHp = Math.round(s.maxHp * 0.55); s.hp = s.maxHp;
          s.as *= 1 + u.state.trk;
          s.leap = false;
          c.text(s, 'Copy');
        },
      },
    },
    SER: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.armor += c.v; u.mr += c.v; u.ap += c.v; } },
        onDeath(c, e) {
          if (!c.isMember(e.unit) || e.unit.summoned) return;
          const left = c.members.filter((m) => m.alive);
          if (!left.length) return;
          const share = c.v / left.length;
          for (const m of left) { m.armor += share; m.mr += share; m.ap += share; }
        },
      },
    },
    NGT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.ngt = c.v; u.state.ngtDmg = c.val('dmg'); } },
        onDamageTaken(c, e) {
          const u = e.unit, s = u.state;
          if (!s.ngt || s.ngtUsed || u.hp >= u.maxHp * 0.5) return;
          s.ngtUsed = true;
          const amt = u.maxHp * s.ngt;
          c.shield(u, amt);
          u.amp += s.ngtDmg;
          const at = u.shield;
          c.after(8, () => { u.shield = Math.max(0, u.shield - Math.max(0, amt - (at - u.shield))); }, u);
          c.text(u, 'Nightfall');
        },
      },
    },
    DAY: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.day = c.v; },
        onDamageTaken(c, e) {
          const u = e.unit, s = u.state;
          if (!s.day || s.dayUsed || u.hp >= u.maxHp * 0.5) return;
          s.dayUsed = true;
          c.heal(u, u.maxHp * s.day * 0.5);
          c.after(1, () => c.heal(u, u.maxHp * s.day * 0.5), u);
          for (const m of c.members) if (m.alive) m.amp += 0.1;
          c.text(u, 'Daybreak');
        },
      },
    },
    CHI: {
      hooks: {
        onCombatStart(c) { c.state.deaths = 0; c.state.risen = false; },
        onDeath(c, e) {
          const u = e.unit;
          if (u.summoned || c.state.risen) return;
          c.state.deaths++;
          if (c.state.deaths < 3) return;
          c.state.risen = true;
          // one random item from each of the (up to) 3 Chimera Digimon nearest the grave
          const near = c.members.slice().sort((a, b) => c.dist(a, u) - c.dist(b, u) || a.id - b.id).slice(0, 3);
          const items = [];
          for (const m of near) { const own = m.items.filter((id) => DT.ITEMS[id] && !DT.ITEMS[id].emblem); if (own.length) items.push(own[Math.floor(c.rng() * own.length)]); }
          const avg = c.members.reduce((s, m) => s + m.stage, 0) / Math.max(1, c.members.length);
          const k = 1 + 0.4 * avg;
          const cell = c.unitAt(u.c, u.r) ? c.freeCellNear(u) : { c: u.c, r: u.r };
          if (!cell) return;
          const s = c.summon(c.team, 'rec_kimeramon', 0, cell.c, cell.r, { items });
          if (!s) return;
          const base = DT.unitStats(s.line, 0);
          s.maxHp = Math.round(s.maxHp + c.v * k - base.maxHp);
          s.hp = s.maxHp;
          const ad = c.val('ad') * k;
          s.ad = s.ad - s.baseAd + ad;
          s.baseAd = ad;
          c.text(s, 'Kimeramon rises');
        },
      },
    },
    WIT: {
      hooks: {
        onCombatStart(c) {
          const mem = c.members;
          if (!mem.length) return;
          const cx = mem.reduce((s, m) => s + m.c, 0) / mem.length, cy = mem.reduce((s, m) => s + m.r, 0) / mem.length;
          let best = null, bd = Infinity;
          for (const a of c.mine) { const d = (a.c - cx) * (a.c - cx) + (a.r - cy) * (a.r - cy); if (d < bd - 1e-9) { bd = d; best = a; } }
          if (!best) return;
          best.ap += c.v;
          c.state.leader = best;
          best.state.covenLeader = true;
        },
        onCast(c, e) {
          const L = c.state.leader;
          if (!L || !L.alive || !c.isMember(e.unit)) return;
          L.mana = Math.min(L.maxMana, L.mana + e.unit.maxMana * 0.3);
        },
      },
    },
    DRK: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.maxHp += c.v; },
        // game hook: a Digi-Egg after each player combat
        onRoundEnd(c, e) {
          if (e.pve) return;
          const p = c.player;
          const x = c.rng();
          const size = x < 0.55 ? 1 : x < 0.9 ? 2 : 3;
          (p.mech.eggs = p.mech.eggs || []).push({ size, left: size, golden: c.val('egg') >= 2 });
        },
      },
      ai: { bonus: (a) => (a.stage <= 4 ? 2 : 0) },
    },
    HNT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.ap += c.v; c.state.done = false; },
        onKill(c, e) {
          if (c.state.done || !c.isMember(e.unit) || e.victim.maxHp < 1500) return;
          c.state.done = true;
          const t = c.val('team');
          for (const a of c.mine) if (a.alive) a.ap += t;
          c.text(e.unit, 'Dragon slain');
        },
      },
    },
    IRN: { hooks: all((u, c) => { u.armor += c.v; }) },
    PHA: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.pha = c.v; },
        onLethal(c, e) {
          const u = e.unit, s = u.state;
          if (!s.pha || s.phaUsed || u.summoned || u.hp > 0) return;
          s.phaUsed = true;
          s.phaRisen = true;
          u.hp = Math.max(1, u.maxHp * s.pha);
          u.invulnUntil = c.t + 1;
          u.stun = Math.max(u.stun, 1);
          u.burn = null;
          c.text(u, 'Rises');
          c.event({ type: 'shield', at: u.id });
        },
        onBeforeDamage(c, e) { if (e.unit.state.phaRisen) e.mult *= 1.2; },
        onBeforeDamageTaken(c, e) { if (e.target.state.phaRisen) e.mult *= 1.3; },
      },
    },
    SEN: {
      hooks: {
        onCombatReady: {
          order: 3,
          fn(c) {
            const mem = c.members.filter((m) => m.alive);
            if (!mem.length) return;
            mem.sort((a, b) => b.items.length - a.items.length || b.as - a.as || a.id - b.id);
            sentinelGive(c, mem[0]);
          },
        },
        onTick(c) {
          const s = c.state;
          if (!s.holder || c.t < s.min) return;
          const u = s.holder;
          const broken = u.shield <= Math.max(0, s.shAt - s.given) + 1;
          if (u.alive && !broken && c.t < s.until) return;
          if (u.alive) u.shield = Math.max(0, u.shield - Math.max(0, u.shield - (s.shAt - s.given)));
          const next = lowestAlly(c, u, false);
          if (next) sentinelGive(c, next);
          else s.holder = null;
        },
      },
    },
    RIV: {
      hooks: {
        onBeforeDamage(c, e) {
          if (c.n !== 1 || !c.isMember(e.unit)) return;
          const rivals = e.unit.line.rivals || [];
          if (e.target.traits.some((t) => rivals.includes(t))) e.mult *= 1 + c.v;
        },
      },
      ai: { bonus: (a) => (a.n >= 1 ? -12 : 0) }, // a second Royal Rival turns the trait off
    },
    ETR: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) {
            const cell = c.freeCellNear(u);
            if (!cell) continue;
            const w = c.summon(u.team, 'rec_wolf', u.stage, cell.c, cell.r, { hpMul: 0.5 * c.v, adMul: 0.5 * c.v });
            if (w) c.text(w, 'Garurumon');
          }
        },
      },
    },
    INA: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) for (const a of c.mine) if (c.dist(a, u) <= 1) c.buff(a, 'dr', c.v, 8);
        },
      },
    },
    // ---- classes ----
    ASN: { hooks: each((u, c) => { u.crit += c.v; u.critDmg += c.val('cd'); u.abilCrit = true; }) },
    BRW: { hooks: each((u, c) => { u.maxHp += c.v; }) },
    CAV: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.cav = c.v; },
        onBeforeDamageTaken(c, e) {
          const s = e.target.state;
          if (s.cav) e.dr += s.cav * (c.t < 4 || c.t < (s.cavUntil || 0) ? 2 : 1);
        },
        onCast(c, e) { if (e.unit.state.cav) e.unit.state.cavUntil = c.t + 3; },
      },
    },
    INV: { hooks: { onAttack(c, e) { const u = e.unit; if (u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + c.v); } } },
    KNT: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          if (e.isBurn || !(e.amount > 0)) return;
          e.mult *= Math.max(0, (e.amount - c.v) / e.amount);
        },
      },
    },
    LEG: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.as *= 1 + c.v; u.state.leg = true; } },
        onCast(c, e) { if (e.unit.state.leg) e.unit.state.legHeal = true; },
        onDamageDealt(c, e) {
          const u = e.unit;
          if (!u.state.legHeal || e.kind !== 'phys' || !u.alive) return;
          u.state.legHeal = false;
          c.heal(u, e.amount * 0.5);
        },
      },
    },
    MYS: { hooks: all((u, c) => { u.mr += c.v; }) },
    RNG: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.state.rng = c.v; u.state.rngNext = 4; } },
        onUnitTick(c, e) {
          const u = e.unit;
          if (!u.state.rng || c.t < u.state.rngNext) return;
          u.buffs.push({ stat: 'as', v: u.state.rng, until: c.t + 4 });
          u.state.rngNext += 8;
        },
      },
    },
    REN: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.ren = c.v; },
        onUnitTick(c, e) {
          const u = e.unit, v = u.state.ren;
          if (!v) return;
          if (u.hp < u.maxHp) c.heal(u, u.maxHp * v * e.dt, true);
          else if (u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + u.maxMana * v * e.dt);
        },
      },
    },
    SKR: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.skr = c.val('ad'); },
        onCombatReady: { order: 2, fn(c) { for (const u of c.members) u.shield += u.maxHp * c.v; } },
        onUnitTick(c, e) {
          const u = e.unit;
          if (u.state.skr && c.every(u, 'skr', 1)) u.ad += u.state.skr * (1 + 0.5 * u.stage);
        },
      },
    },
    SPW: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.ap += c.v; },
        onAnyCast(c) {
          const per = c.val('per');
          for (const m of c.members) if (m.alive && (m.state.spw || 0) < 10) { m.state.spw = (m.state.spw || 0) + 1; m.ap += per; }
        },
      },
    },
    CAN: {
      hooks: {
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          u.state.can = (u.state.can || 0) + 1;
          if (u.state.can % 5 !== 0 || !e.target.alive) return;
          c.event({ type: 'burst', c: e.target.c, r: e.target.r, radius: 1, team: u.team });
          for (const t of c.enemiesNear(u.team, e.target, 1)) c.damage(u, t, e.dmg * c.v, 'phys');
        },
      },
    },
  };

  // ---------- item hooks ----------
  const killAd = (f) => function (c, e) { if (e.unit === c.unit) e.unit.ad += e.unit.baseAd * f; };
  function feather(pct, slow) {
    return {
      onLethal(c, e) {
        const u = e.unit;
        if (u !== c.unit || u.state.feather || u.hp > 0) return;
        u.state.feather = true;
        u.hp = 1;
        u.invulnUntil = c.t + 2;
        u.stun = Math.max(u.stun, 2);
        u.burn = null; u.weakUntil = 0;
        c.text(u, 'Held safe');
        c.after(2, () => {
          u.hp = Math.min(u.maxHp, Math.max(u.hp, u.maxHp * pct));
          c.event({ type: 'heal', at: u.id, amount: Math.round(u.maxHp * pct) });
          if (slow) u.buffs.push({ stat: 'as', v: -slow * dbk(c), until: Infinity });
        }, u);
      },
    };
  }
  function banner(asUp, vamp) {
    return { onCombatStart(c) { for (const a of rowMates(c, c.unit, 1)) { a.as *= 1 + asUp; a.omnivamp += vamp || 0; } } };
  }
  const range = (n) => ({ onCombatStart(c) { c.unit.range += n; } });
  function voidSpawn(c, at, stage, unstable) {
    const cell = c.unitAt(at.c, at.r) ? c.freeCellNear(at) : { c: at.c, r: at.r };
    if (!cell) return null;
    const s = c.summon(c.team, 'rec_voidspawn', Math.min(2, Math.max(0, stage)), cell.c, cell.r);
    if (!s) return null;
    s.leap = false;
    if (unstable) s.state.unstable = true;
    for (const e of c.enemiesNear(c.team, s, 2)) e.target = s;
    c.text(s, 'Voidlarva');
    return s;
  }
  const lockets = (shield, range2, hp) => ({
    onCombatStart(c) {
      const u = c.unit;
      for (const a of rowMates(c, u, range2)) { if (hp) a.maxHp += hp; c.shield(a, shield[u.stage]); }
    },
  });
  function frost(slowE, slowA, reach = 2) {
    return {
      onUnitTick(c, e) {
        const u = e.unit;
        if (u !== c.unit || !c.every(u, 'frost', 0.5)) return;
        for (const t of c.enemiesNear(u.team, u, reach)) if ((t.state.frostUntil || 0) <= c.t + 0.1) { t.buffs.push({ stat: 'as', v: -slowE, until: c.t + 0.6 }); t.state.frostUntil = c.t + 0.6; }
        if (slowA) for (const a of c.alliesNear(u.team, u, 1)) if (a !== u && (a.state.dfrostUntil || 0) <= c.t + 0.1) { a.buffs.push({ stat: 'as', v: -slowA * dbk(c), until: c.t + 0.6 }); a.state.dfrostUntil = c.t + 0.6; }
      },
    };
  }
  function shroud(fe, fa, cols, allyMana) {
    return {
      onCombatStart(c) {
        const u = c.unit;
        for (const t of c.units) {
          if (!t.alive || t === u || Math.abs(t.c - u.c) > cols) continue;
          const f = t.team !== u.team ? fe : t.c === u.c ? fa * dbk(c) : 0;
          if (t.team === u.team && allyMana) t.mana += allyMana;
          if (!f || t.state.shroudMax) continue;
          t.state.shroudMax = t.maxMana;
          t.maxMana = Math.round(t.maxMana * (1 + f));
        }
      },
    };
  }
  function wind(dur, after, teamAs) {
    return {
      onCombatStart(c) {
        const u = c.unit;
        const mc = c.cols - 1 - u.c, mr = c.rows - 1 - u.r;
        let best = null, bd = Infinity;
        for (const t of c.units) if (t.alive && t.team !== u.team) { const d = c.dist(t, { c: mc, r: mr }); if (d < bd) { bd = d; best = t; } }
        if (teamAs) for (const a of c.mine) a.as *= 1 + teamAs;
        if (!best) return;
        best.stun = Math.max(best.stun, dur);
        c.text(best, 'Blown away');
        if (after) c.after(dur, () => { best.ad += best.baseAd * 0.3 * dbk(c); best.ap += 30 * dbk(c); }, best);
      },
    };
  }
  const spellShield = (who, stun) => ({
    onCombatStart(c) {
      const u = c.unit;
      const list = who === 'self' ? [u] : who === 'row' ? rowMates(c, u, 1) : c.mine;
      for (const a of list) a.state.spellShield = true;
      if (stun) u.state.trapStun = true;
    },
  });

  const items = {
    rec_victory_sword: { hooks: { onKill: killAd(0.12) } },
    rec_s_victory_sword: { hooks: { onKill: killAd(0.2), onCombatStart(c) { frail(c, 0.15); } } },
    rec_r_victory_sword: { hooks: { onKill: killAd(0.2) } },
    rec_s_titan_slayer: { hooks: { onBeforeDamage(c, e) { if (e.unit === c.unit && e.target.maxHp >= 1100) e.mult *= 1.5; } } },
    rec_s_gunblade: {
      hooks: {
        onDamageDealt(c, e) {
          if (e.unit !== c.unit || e.kind !== 'magic' || e.isBurn || !(e.amount > 0)) return;
          const a = lowestAlly(c, c.unit, true);
          if (a) c.heal(a, e.amount * 0.5, true);
        },
      },
    },
    rec_guardian_feather: { hooks: feather(0.3) },
    rec_s_guardian_feather: { hooks: feather(0.7, 0.5) },
    rec_r_guardian_feather: { hooks: feather(1) },
    rec_s_blood_fang: {
      hooks: {
        onUnitTick(c, e) { const u = e.unit; if (u === c.unit && !u.state.risk) { u.state.risk = true; u.hp = u.maxHp * (1 - 0.2 * dbk(c)); } },
        onHeal(c, e) {
          const u = e.unit;
          if (u !== c.unit || u.hp < u.maxHp * 0.9 || (u.state.riskAt || 0) > c.t) return;
          u.state.riskAt = c.t + 4;
          c.buff(u, 'as', 0.4, 4);
        },
      },
    },
    rec_war_banner: { hooks: banner(0.25) },
    rec_s_war_banner: {
      hooks: {
        onCombatStart(c) {
          const u = c.unit;
          const mates = rowMates(c, u, 1, false);
          for (const a of mates) a.as *= 1 - 0.2 * dbk(c);
          u.as *= 1 + 0.35 * mates.length;
        },
      },
    },
    rec_r_war_banner: { hooks: banner(0.45, 0.1) },
    rec_s_crit_edge: { hooks: { onAttack(c, e) { if (e.unit === c.unit) hurt(c, c.unit, c.unit.maxHp * 0.02 * dbk(c)); } } },
    rec_rapid_cannon: { hooks: range(1) },
    rec_s_rapid_cannon: {
      hooks: {
        onCombatStart(c) { c.unit.range += 1; },
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'rdc', 0.5)) return;
          const near = c.enemiesNear(u.team, u, 2).length > 0;
          if (!near && !u.state.rdc) { u.state.rdc = { stat: 'as', v: 0.75, until: Infinity }; u.buffs.push(u.state.rdc); }
          else if (near && u.state.rdc) { unbuff(u, u.state.rdc); u.state.rdc = null; }
        },
      },
    },
    rec_r_rapid_cannon: { hooks: range(2) },
    rec_s_rage_wing: { hooks: { onAttack(c, e) { if (e.unit === c.unit) hurt(c, c.unit, c.unit.maxHp * 0.015 * dbk(c)); } } },
    rec_s_volt_disk: {
      hooks: {
        onCombatStart(c) { c.unit.ad -= c.unit.baseAd * 0.3 * dbk(c); },
        onAttack(c, e) {
          const u = e.unit;
          if (u !== c.unit || u.nAtk % 2 !== 0) return;
          for (const t of pickN(c, c.enemiesOf(u), 4)) { c.event({ type: 'beam', from: u.id, to: t.id }); c.damage(u, t, 80, 'magic'); }
        },
      },
    },
    rec_s_titan_will: {
      hooks: {
        onDamageTaken(c, e) { if (e.unit === c.unit) e.unit.state.store = (e.unit.state.store || 0) + e.amount * 0.6; },
        onAttack(c, e) {
          const u = e.unit, s = u.state;
          if (u !== c.unit || !(s.store > 0) || c.t < (s.revAt || 2) || !e.target.alive) return;
          const d = s.store;
          s.store = 0;
          s.revAt = c.t + 2;
          c.damage(u, e.target, d, 'true');
          c.text(u, 'Revenge');
        },
      },
    },
    rec_s_twin_shot: {
      hooks: {
        onCombatStart(c) { c.unit.ad -= c.unit.baseAd * 0.1 * dbk(c); },
        onAttack(c, e) {
          const u = e.unit;
          if (u !== c.unit) return;
          const others = c.enemiesNear(u.team, e.target, 2).filter((t) => t !== e.target).sort((a, b) => c.dist(e.target, a) - c.dist(e.target, b) || a.id - b.id).slice(0, 2);
          for (const t of others) { c.event({ type: 'shot', from: u.id, to: t.id }); c.damage(u, t, e.dmg * 0.7, 'phys', e.crit); }
        },
      },
    },
    rec_void_portal: {
      hooks: {
        onCombatStart(c) { for (const t of c.enemiesNear(c.team, c.unit, 2)) t.target = c.unit; },
        onDeath(c, e) { if (e.unit === c.unit) voidSpawn(c, e.unit, e.unit.stage, false); },
      },
    },
    rec_s_void_portal: {
      hooks: {
        onCombatStart(c) { frail(c, 0.1); for (const t of c.enemiesNear(c.team, c.unit, 2)) t.target = c.unit; },
        onDeath(c, e) { if (e.unit === c.unit) voidSpawn(c, e.unit, e.unit.stage, true); },
      },
    },
    rec_r_void_portal: {
      hooks: {
        onCombatStart(c) {
          const s = voidSpawn(c, c.freeCellNear(c.unit) || c.unit, c.unit.stage, false);
          if (!s) return;
          for (const t of c.enemiesNear(c.team, s, 3)) t.target = s;
        },
        onDeath(c, e) { if (e.unit === c.unit) voidSpawn(c, e.unit, e.unit.stage, false); },
      },
    },
    rec_s_armor_breaker: {
      hooks: {
        onCombatStart(c) { const u = c.unit, k = 1 - 0.2 * dbk(c); u.armor *= k; u.mr *= k; },
        onAttack(c, e) {
          const t = e.target;
          if (e.unit !== c.unit || !e.crit || !t.alive) return;
          t.shred = Math.max(t.shredUntil > c.t ? t.shred : 0, 0.7);
          t.shredUntil = c.t + 5;
          if ((t.state.fwUntil || 0) <= c.t && t.mr > 0) { c.buff(t, 'mr', -t.mr * 0.7, 5); t.state.fwUntil = c.t + 5; }
        },
      },
    },
    rec_s_ultima_wand: { hooks: { onCombatStart(c) { frail(c, 0.15); } } },
    rec_seraph_staff: { hooks: { onCast(c, e) { if (e.unit === c.unit) e.unit.ap += e.unit.maxMana * 0.3; } } },
    rec_s_seraph_staff: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (u !== c.unit) return;
          const add = u.maxMana * 3.5;
          u.maxHp += add;
          c.heal(u, add);
        },
      },
    },
    rec_r_seraph_staff: { hooks: { onCast(c, e) { if (e.unit === c.unit) e.unit.ap += e.unit.maxMana * 0.6; } } },
    rec_iron_locket: { hooks: lockets([250, 350, 450], 2) },
    rec_s_iron_locket: {
      hooks: {
        onCombatStart(c) {
          const lose = 5 * dbk(c);
          for (const a of rowMates(c, c.unit, 2)) { a.armor += 60; a.mr += 60; a.state.lunar = Math.max(a.state.lunar || 0, lose); }
        },
      },
    },
    rec_r_iron_locket: { hooks: lockets([450, 600, 750], 3, 200) },
    rec_s_mana_zap: {
      hooks: {
        onAnyCast(c, e) {
          const z = c.unit, t = e.unit;
          if (!z.alive || t === z || !t.alive || c.dist(z, t) > 2) return;
          c.event({ type: 'beam', from: z.id, to: t.id });
          c.damage(z, t, t.maxMana * (t.team !== z.team ? 2 : 0.5 * dbk(c)), 'magic');
          z.ap += 10;
        },
      },
    },
    rec_s_flame_tome: {
      hooks: {
        onDamageDealt(c, e) {
          const t = e.target;
          if (e.unit !== c.unit || e.kind !== 'magic' || e.isBurn || !t.alive) return;
          if (!t.state.curse) t.state.curseNext = c.t + 1;
          t.state.curse = { until: c.t + 8, src: c.unit, rate: 0.06 };
          t.woundUntil = Math.max(t.woundUntil || 0, c.t + 8);
        },
        onBeforeDamage(c, e) { if (e.unit === c.unit && e.kind === 'magic' && !e.isBurn) e.mult *= 1 - 0.2 * dbk(c); },
      },
    },
    rec_s_jewel_gauntlet: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (u !== c.unit) return;
          if (!u.state.sg) { u.state.sg = true; u.crit += 0.25; }
          hurt(c, u, u.maxMana * 0.5 * (u.stage + 1) * dbk(c));
        },
      },
    },
    rec_s_mana_overflow: { hooks: { onCast(c, e) { const u = e.unit; if (u === c.unit) u.mana = u.hp < u.maxHp * 0.6 ? 50 : 20; } } },
    rec_r_mana_overflow: { hooks: { onCast(c, e) { if (e.unit === c.unit) e.unit.mana = 30; } } },
    rec_frost_heart: { hooks: frost(0.25) },
    rec_s_frost_heart: { hooks: frost(0.35, 0.25, 2) },
    rec_r_frost_heart: { hooks: frost(0.5) },
    rec_power_chalice: { hooks: { onCombatStart(c) { for (const a of rowMates(c, c.unit, 1)) a.ap += 25; } } },
    rec_s_power_chalice: {
      hooks: { onCombatStart(c) { for (const a of rowMates(c, c.unit, 2, false)) a.ap += 45; c.unit.ap -= 20 * dbk(c); } },
    },
    rec_r_power_chalice: { hooks: { onCombatStart(c) { for (const a of rowMates(c, c.unit, 1)) { a.ap += 50; a.omnivamp += 0.1; } } } },
    rec_s_holy_ring: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'sring', 5)) return;
          for (const a of c.units) if (a.alive && a.team === u.team && a !== u) c.heal(a, (a.maxHp - a.hp) * 0.25);
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 3, team: 1 - u.team });
          hurt(c, u, u.hp * 0.05 * dbk(c));
        },
      },
    },
    rec_s_justice_hand: {
      hooks: {
        onCombatStart(c) { const u = c.unit; u.state.vh = true; u.ad += u.baseAd * 0.3; u.ap += 30; u.omnivamp += 0.2; },
        onKill(c, e) {
          const u = e.unit;
          if (u !== c.unit) return;
          const k = u.state.vh ? -1 : 1;
          u.state.vh = !u.state.vh;
          u.ad += k * u.baseAd * 0.3; u.ap += k * 30; u.omnivamp += k * 0.2;
        },
      },
    },
    rec_s_stone_hide: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'ustone', 2)) return;
          const n = c.enemiesNear(u.team, u, 2).length;
          if (n) c.heal(u, u.maxHp * 0.02 * n, true);
        },
      },
    },
    rec_s_flame_core: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit) return;
          if (!u.state.ecl) { u.state.ecl = true; if (dbk(c) === 1) c.burn(u, u); }
          if (!c.every(u, 'eclipse', 1.5)) return;
          const pool = c.enemiesNear(u.team, u, 3);
          if (pool.length) c.burn(u, pool[Math.floor(c.rng() * pool.length)]);
        },
      },
    },
    rec_r_flame_core: {
      hooks: {
        onUnitTick(c, e) {
          const u = e.unit;
          if (u !== c.unit || !c.every(u, 'sunlight', 1.5)) return;
          const pool = c.enemiesNear(u.team, u, 4);
          if (pool.length) c.burn(u, pool[Math.floor(c.rng() * pool.length)]);
        },
      },
    },
    rec_still_shroud: { hooks: shroud(0.35, 0, 0) },
    rec_s_still_shroud: { hooks: shroud(0.8, 0.4, 0) },
    rec_r_still_shroud: { hooks: shroud(0.5, 0, 1, 15) },
    rec_s_dragon_scale: {
      hooks: {
        onBeforeDamageTaken(c, e) {
          if (e.target !== c.unit) return;
          if (e.kind === 'magic') e.mult *= 0.8;
          if (e.crit && e.source && e.source.critDmg > 1) e.mult /= e.source.critDmg;
        },
      },
    },
    rec_wind_ward: { hooks: wind(4) },
    rec_s_wind_ward: { hooks: wind(7, true) },
    rec_r_wind_ward: { hooks: wind(6, false, 0.1) },
    rec_s_quick_silver: { hooks: { onCombatStart(c) { frail(c, 0.12); } } },
    rec_s_giga_belt: { hooks: { onUnitTick(c, e) { const u = e.unit; if (u === c.unit && c.every(u, 'sbelt', 1)) hurt(c, u, u.maxHp * 0.015 * dbk(c)); } } },
    rec_trap_claw: { hooks: spellShield('self', true) },
    rec_s_trap_claw: { hooks: spellShield('row', false) },
    rec_r_trap_claw: { hooks: spellShield('team', true) },
    rec_s_thief_gloves: { hooks: { onCombatStart(c) { frail(c, 0.1); } } },
  };

  // ---------- consumables ----------
  function remove(c, u, idx) {
    const p = c.player;
    if (!u.items.length) return 'noItems';
    if (p.items.length - 1 + u.items.length > c.game.R.itemCap) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    const back = u.items;
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  function tomeOptions(c) { return c.shuffle(DT.EMBLEM_IDS.slice()).slice(0, 4); }
  items.rec_remover = { use: (c, u, idx) => remove(c, u, idx) };
  items.rec_tome = {
    use(c, u, idx) {
      const p = c.player;
      if (c.game.phase !== 'planning') return 'phase';
      if (p.offer) return 'busy';
      const id = p.items[idx];
      p.items.splice(idx, 1);
      p.offer = { kind: 'rec_tome', options: tomeOptions(c), rerolls: 0 };
      return id;
    },
  };

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const shadowComps = () => DT.SPECIAL_COMPONENTS.filter((k) => DT.COMPONENTS[k].shadow);
  const shadowItems = () => Object.keys(DT.ITEMS).filter((k) => DT.ITEMS[k].cat === 'shadow');
  const augments = {
    rec_a_coins: { hooks: { onPick(c) { c.player.gold += 8; } } },
    rec_a_cache: { hooks: { onPick(c) { c.give(c.pick(shadowComps())); c.give(c.pick(shadowComps())); } } },
    rec_a_scan: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    rec_a_heart: { hooks: { onPick(c) { c.player.hp += 4; } } },
    rec_a_training: { hooks: team((u) => { u.ad += u.baseAd * 0.1; u.ap += 10; }) },
    rec_a_mist: {
      hooks: {
        onDeath(c, e) {
          const u = e.unit;
          if (u.summoned) return;
          for (const t of c.enemiesNear(u.team, u, 2)) c.damage(u, t, 150, 'magic');
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
        },
      },
    },
    rec_a_front: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 25; u.mr += 25; u.maxHp += 150; } }) },
    rec_a_nest: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.2; }) },
    rec_a_bank: { hooks: { onPick(c) { c.player.gold += 16; } } },
    rec_a_trove: { hooks: { onPick(c) { c.give(c.pick(shadowItems())); c.give(c.randomComponent()); } } },
    rec_a_egg: { hooks: { onPick(c) { c.give('rec_egg'); } } },
    rec_a_income: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    rec_a_xanti: { hooks: team((u) => { if (u.items.some(isShadow)) u.maxHp += 150; }) },
    rec_a_vamp: { hooks: team((u) => { u.omnivamp += 0.12; }) },
    rec_a_mana: { hooks: team((u) => { u.mana += 25; }) },
    rec_a_unity: { hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of c.mine) { u.ad += u.baseAd * 0.03 * n; u.ap += 3 * n; } } } },
    rec_a_ascend: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 12) return;
          c.state.done = true;
          for (const u of c.survivors(c.team)) { u.amp += 0.35; c.text(u, 'Ascended'); }
        },
      },
    },
    rec_a_jackpot: { hooks: { onPick(c) { c.player.gold += 30; } } },
    rec_a_blessing: { hooks: { onPick(c) { if (!c.player.mech.blessed) c.player.mech.blessEarly = true; } } },
    rec_a_armory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    rec_a_nature: { hooks: { onPick(c) { c.give('rec_golden_egg'); } } },
    rec_a_darkness: {
      hooks: {
        onPick(c) { c.player.teamBonus += 2; },
        onRoundEnd(c, e) {
          if (e.won) return;
          const Rr = c.R;
          let d = e.pve && Rr.pveLossDamage ? Rr.pveLossDamage(c.stage) : null;
          if (d == null) d = Rr.lossDamage(c.stage);
          c.player.hp -= d;
        },
      },
    },
    rec_a_giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    rec_a_tome: { hooks: { onPick(c) { c.give('rec_tome'); c.give('rec_tome'); c.give('rec_egg'); } } },
  };

  // ---------- custom ability types ----------
  const STAGE3 = (a) => (stage) => a[stage];
  const hookStun = STAGE3([1.5, 2, 2.5]);
  const decay = STAGE3([0.15, 0.07, 0]);
  const respiteDur = STAGE3([2.5, 3, 4]);
  function resurrect(c, viego, v) {
    if (!viego.alive) return;
    const cell = c.unitAt(v.c, v.r) ? c.freeCellNear(v) : { c: v.c, r: v.r };
    if (!cell) return;
    const s = c.summon(viego.team, v.line.id, v.stage, cell.c, cell.r);
    if (!s) return;
    s.leap = false;
    s.state.decay = decay(viego.stage);
    s.state.decayNext = c.t + 1;
    c.text(s, 'Possessed');
  }
  // summoned partners grow with the caster's cost and form
  const petAp = (u) => 40 + 15 * u.line.tier + 30 * u.stage;
  const petHp = (line) => 0.5 + 0.05 * line.tier;
  const abilities = {
    hook: {
      base: 170,
      cast(c, u, tg, v) {
        const far = c.target(u, 'farthest') || tg;
        if (!far) return;
        c.event({ type: 'beam', from: u.id, to: far.id });
        c.abilityHit(u, far, v);
        if (!far.alive) return;
        if (c.dist(u, far) > 1) {
          const cell = c.freeCellNear(u, far);
          if (cell) c.move(far, cell.c, cell.r, 0.25, { leap: true });
        }
        c.stun(far, hookStun(u.stage));
      },
      text: (line, stage, v) => `Hooks the farthest enemy for ${v} magic damage, pulls it close and stuns it for ${hookStun(stage)}s.`,
    },
    summon: {
      base: 100,
      cast(c, u, tg, v) {
        const s = u.state.pet;
        if (s && s.alive) {
          c.heal(s, s.maxHp * 0.3);
          s.mana = s.maxMana;
          s.ap = Math.max(s.ap, petAp(u));
          c.text(s, 'Go!');
          return;
        }
        const cell = c.freeCellNear(u, tg || u);
        if (!cell) return;
        const pet = c.summon(u.team, u.line.summon, u.stage, cell.c, cell.r, { hpMul: petHp(u.line) });
        if (!pet) return;
        pet.ap = petAp(u);
        pet.mana = pet.maxMana; // casts on arrival
        pet.leap = false;
        u.state.pet = pet;
      },
      text: (line, stage) => `Calls ${DT.LINE[line.summon] ? DT.LINE[line.summon].forms[0] : 'a partner'} (${petAp({ line, stage })} ability power, ${Math.round(petHp(line) * 100)}% HP) at its own form, which casts on arrival. If it is already out, it heals 30% and casts again.`,
    },
    possess: {
      base: 220,
      cast(c, u, tg, v) {
        if (!tg || !tg.alive) return;
        c.event({ type: 'beam', from: u.id, to: tg.id });
        c.stun(tg, 2);
        c.abilityHit(u, tg, v);
        if (!tg.alive) resurrect(c, u, tg);
        else tg.state.possess = { by: u, until: c.t + 3 };
      },
      text: (line, stage, v) => `Stuns the target for 2s and deals ${v} magic damage. If it falls within 3 seconds, it rises to fight for this team${decay(stage) ? `, losing ${Math.round(decay(stage) * 100)}% max HP per second` : ''}.`,
    },
    respite: {
      base: 150,
      cast(c, u, tg, v) {
        const dur = respiteDur(u.stage);
        c.teamState(u.team).respite = { until: c.t + dur, src: u };
        c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: 1 - u.team });
        for (const a of c.alliesNear(u.team, u, 2)) { c.heal(a, v); c.event({ type: 'shield', at: a.id }); }
      },
      text: (line, stage, v) => `For ${respiteDur(stage)}s, allies within 2 hexes of it cannot drop below 1 HP. They also heal ${v}.`,
    },
  };

  // ---------- the Radiant Blessing and Radiant Armory ----------
  const itemsDef = () => defItems();
  // base item ids the player holds: board units first, then bench units, then the item bench
  function heldBases(p) {
    const RO = itemsDef().radiantOf;
    const out = [];
    const add = (id) => { if (RO[id] && !out.includes(id)) out.push(id); };
    for (const u of p.boardUnits()) for (const id of u.items) add(id);
    for (const u of p.bench) if (u) for (const id of u.items) add(id);
    for (const id of p.items) add(id);
    return out;
  }
  function radiantOptions(c, n, exclude = []) {
    const D = itemsDef();
    const held = heldBases(c.player).map((id) => D.radiantOf[id]).filter((id) => !exclude.includes(id));
    const out = c.shuffle(held).slice(0, n);
    const rest = Object.values(D.radiantOf).filter((id) => !out.includes(id) && !exclude.includes(id));
    while (out.length < n && rest.length) out.push(rest.splice(Math.floor(c.rng() * rest.length), 1)[0]);
    return out;
  }
  // upgrade the first held copy of the base item, else put the Radiant item on the item bench
  function grantRadiant(c, rid) {
    const p = c.player;
    const base = itemsDef().baseOf[rid];
    for (const u of p.boardUnits().concat(p.bench.filter(Boolean))) {
      const i = u.items.indexOf(base);
      if (i >= 0) { u.items[i] = rid; return; }
    }
    const j = p.items.indexOf(base);
    if (j >= 0) { p.items[j] = rid; return; }
    c.give(rid);
  }
  function radiantScore(c, rid) {
    const g = c.game, p = c.player;
    const base = itemsDef().baseOf[rid];
    let sc = 0;
    if (heldBases(p).includes(base)) sc += 6;
    for (const h of g.aiHolders(p)) {
      if (h.u.items.includes(base)) sc += 8;
      const list = DT.ITEM_ROLE[h.kind] || [];
      if (list.includes(rid)) sc += 4 - list.indexOf(rid) * 0.1;
    }
    return sc;
  }
  const itemCard = (id) => ({
    name: DT.ITEMS[id] ? DT.ITEMS[id].name : id,
    desc: `${DT.itemText(id)} Upgrades your ${DT.ITEMS[itemsDef().baseOf[id]] ? DT.ITEMS[itemsDef().baseOf[id]].name : 'item'} if you hold one, otherwise goes to your item bench.`,
    img: DT.ItemArt ? DT.ItemArt.img(id, 64) : '',
  });
  function blessDue(c) {
    const p = c.player;
    return !p.mech.blessed && p.alive && (p.hp <= c.R.blessAt || p.mech.blessEarly);
  }
  function deliverBlessing(c) {
    const p = c.player;
    p.mech.blessed = true;
    p.mech.blessEarly = false;
    p.gold += c.R.blessGold || 0;
    p.offer = { kind: 'rec_bless', options: radiantOptions(c, 3), rerolls: 1 };
  }

  // ---------- Dragon Kin eggs ----------
  function dropOrb(c, kind, o) {
    const g = c.game, p = c.player;
    p.orbs.push({ id: ++g.uid, kind, gold: 0, items: [], unit: null, x: 0.1 + c.rng() * 0.8, z: 0.15 + c.rng() * 0.7, ...o });
    if (!p.isHuman) g.collectAllOrbs(p);
  }
  function hatchEggs(c) {
    const p = c.player;
    const eggs = p.mech.eggs;
    if (!eggs || !eggs.length) return;
    const keep = [];
    for (const egg of eggs) {
      egg.left--;
      if (egg.left > 0) { keep.push(egg); continue; }
      const gold = egg.golden ? 1 : 0;
      if (egg.size === 1) dropOrb(c, 'blue', { gold: 2 + gold * 2 });
      else if (egg.size === 2) dropOrb(c, 'blue', { items: egg.golden ? [c.randomComponent(), c.randomComponent()] : [c.randomComponent()] });
      else dropOrb(c, 'gold', { items: [c.randomCompleted()], gold: gold * 3 });
    }
    p.mech.eggs = keep;
  }

  // ---------- loot, Armory, carousel ----------
  const normalComp = (c) => c.pick(DT.STAT_COMPONENTS);
  const sporeComp = (c) => c.pick(shadowComps());
  const comp = (c, chance) => (c.rng() < chance ? sporeComp(c) : normalComp(c));
  const completed = (c, chance) => { const id = c.randomCompleted(); return c.rng() < chance ? itemsDef().shadowOf[id] || id : id; };

  const Reckoning = {
    pveReward(c, won) {
      const out = [];
      const st = c.stage, R0 = c.R;
      if (st === 1) {
        out.push({ kind: 'blue', items: [normalComp(c)] });
        if (won) {
          out.push({ kind: 'blue', gold: c.round === 3 ? 2 : 1 });
          if (c.round === 3) {
            const lines = DT.SHOP_LINES.filter((l) => l.tier <= 2 && c.game.pool[l.id] > 0);
            if (lines.length) out.push({ kind: 'gold', unit: c.pick(lines).id });
          }
        }
        return out;
      }
      out.push({ kind: 'blue', gold: won ? 3 : 2 });
      out.push({ kind: 'blue', items: [comp(c, R0.shadowDropChance)] });
      if (won && st >= 4) out.push({ kind: 'gold', items: [completed(c, 0.25)] });
      if (won && st >= 3 && c.rng() < 0.25) out.push({ kind: 'gold', items: [c.rng() < 0.5 ? 'rec_tome' : 'rec_remover'] });
      return out;
    },
    armory(c) {
      const st = c.stage, ch = c.R.shadowDropChance;
      const out = [];
      const add = (fn) => { for (let k = 0; k < 20; k++) { const id = fn(); if (id && !out.includes(id)) { out.push(id); return; } } };
      if (st <= 2) { add(() => sporeComp(c)); add(() => normalComp(c)); add(() => normalComp(c)); }
      else if (st <= 3) { add(() => comp(c, 0.4)); add(() => comp(c, 0.4)); add(() => (c.rng() < 0.4 ? 'rec_tome' : c.rng() < 0.5 ? c.randomEmblem(true) : normalComp(c))); }
      else { add(() => completed(c, ch)); add(() => completed(c, ch)); add(() => (c.rng() < 0.2 ? 'rec_tome' : completed(c, ch))); }
      return out;
    },
    carouselItem(game, stage) {
      const pick = (arr) => arr[Math.floor(game.rng() * arr.length)];
      if (stage <= 1) return pick(DT.STAT_COMPONENTS);
      if (stage >= 5) { const id = game.randomCompleted(); return game.rng() < game.R.shadowCarouselChance * 0.5 ? itemsDef().shadowOf[id] || id : id; }
      return game.rng() < game.R.shadowCarouselChance ? pick(shadowComps()) : pick(DT.STAT_COMPONENTS);
    },
    // codex Rules tab, built from the rules of the pace being shown
    codexRules(H, pace) {
      const normal = (pace || H.pace) === 'normal';
      const NAME = { carousel: 'carousel', pve: 'wild Digimon', pvp: 'PvP' };
      const row = (st) => { const out = []; for (let r = 1; r <= H.roundsInStage(st); r++) out.push(`${st}-${r} ${NAME[H.roundType(st, r)] || H.roundType(st, r)}`); return out.join(', '); };
      const NAMES = { armory: 'Armory', rec_radiant: 'Radiant Armory', augment: 'augment' };
      const offers = (H.offers || []).map((o) => `${NAMES[o.kind] || o.kind} at ${o.stage}-${o.round}`);
      const augs = H.augmentRounds || (H.augmentStages || []).map((st) => `${st}-${H.augmentRound || 2}`);
      const odds = [3, 4, 5, 6, 7, 8, 9, 10].filter((l) => H.shopOdds[l]).map((l) => `<tr><td>${l}</td>${H.shopOdds[l].map((x) => `<td>${x}%</td>`).join('')}</tr>`).join('');
      const stages = [1, 2, 3, 4, 5, 6, 7];
      const econ = H.xp
        ? `<p>Standard TFT rules: buy ${H.xp.buyXp} XP for ${H.xp.buyCost} gold and gain ${H.xp.perRound} XP a round. Income is 5 gold a round (less in stage 1), plus 1 interest per 10 gold you hold (up to 5) and win or loss streak gold. A loss costs stage damage plus damage for each surviving enemy Digimon. Rerolls cost ${H.rerollCost}.</p>`
        : `<p>Hyper Roll rules: no XP and no interest. Your level (team size) is set by the stage, income is fixed per round, a PvP win pays ${H.winGold} gold and rerolls cost ${H.rerollCost}.</p><table class="cx-tbl"><tr><th>Stage</th>${stages.map((st) => `<th>${st}</th>`).join('')}</tr><tr><td>Level</td>${stages.map((st) => `<td>${H.levelByStage[Math.min(st, H.levelByStage.length - 1)]}</td>`).join('')}</tr><tr><td>Income</td>${stages.map((st) => `<td>${H.incomeByStage[Math.min(st, H.incomeByStage.length - 1)]}</td>`).join('')}</tr><tr><td>Loss</td>${stages.map((st) => `<td>${H.lossDamage(st)}</td>`).join('')}</tr></table>`;
      return [
        { id: 'rounds', title: 'Rounds', html: `<p>8 Tamers start with ${H.startHp} HP. Last one standing wins.</p><p>${row(1)}.</p><p>${row(2)}.</p>${normal ? '' : `<p>${row(3)}. The 3-1 carousel is the Shadow carousel: lowest HP picks first, and half its items are Spore components.</p>`}<p>${offers.join(', ')}. Augments at ${augs.join(', ')}.</p>` },
        { id: 'econ', title: 'Economy', html: econ },
        { id: 'shop', title: 'Shop odds', html: `<p>All Tamers share one pool: 30, 25, 18, 10 and 9 copies of each 1 to 5-cost Digimon.</p><table class="cx-tbl odds"><tr><th>Level</th><th>1g</th><th>2g</th><th>3g</th><th>4g</th><th>5g</th></tr>${odds}</table>` },
        { id: 'shadow', title: 'Shadow items', html: `<p>Spore components (from the Armory, wild Digimon loot and ${normal ? 'the carousels after the first' : 'the 3-1 carousel'}) have the same stats as normal components. A Spore component combined with any component makes the Shadow version of the item: stronger, with a drawback written on the item. A drawback never knocks out its holder. A Digi-Egg plus a Spore component makes one of 8 rarer Crests. The X-Antibody augment halves drawbacks.</p>` },
        { id: 'radiant', title: 'Radiant items', html: `<p>Every completed item has a Radiant upgrade: the same item, much stronger. The Radiant Armory lets everyone pick one. The Radiant Blessing comes once per game, at the first free round after you drop to ${H.blessAt} HP or lower: you get ${H.blessGold} gold and a pick of 3 Radiant upgrades (one reroll). Picks upgrade an item you hold, or go to your item bench.</p>` },
        { id: 'evo', title: 'Digivolution', html: '<p>Three copies of a form digivolve into the next form. Items carry over. Tricksters copies, Kimeramon, summoned partners and Possessed Digimon never use a team slot.</p>' },
      ];
    },
  };
  DT.Reckoning = Reckoning;

  // ---------- AI helpers ----------
  function aiCraftShadow(c) {
    const p = c.player, g = c.game;
    const counts = DT.countTraits(p.boardUnits(), p.augs);
    // Digi-Egg + Spore component when it reaches a breakpoint
    for (let guard = 0; guard < 4 && p.items.includes('rec_egg'); guard++) {
      let done = false;
      for (const sc of shadowComps()) {
        if (!p.items.includes(sc)) continue;
        const em = DT.combineItems('rec_egg', sc);
        const t = em && DT.ITEMS[em].emblem;
        if (!t || !(counts[t] >= 1) || !DT.TRAITS[t].bp.includes(counts[t] + 1)) continue;
        if (g.craft(p, em)) { counts[t]++; done = true; break; }
      }
      if (!done) break;
    }
    const holders = g.aiHolders(p);
    for (let guard = 0; guard < 10; guard++) {
      const i = p.items.findIndex((x) => DT.COMPONENTS[x] && DT.COMPONENTS[x].shadow);
      if (i < 0) break;
      let best = -1, bestSc = -Infinity;
      p.items.forEach((x, j) => {
        if (j === i || !DT.isComponent(x) || x === 'rec_egg') return;
        const made = DT.combineItems(p.items[i], x);
        if (!made || DT.ITEMS[made].emblem) return;
        let sc = 0;
        for (const h of holders) { const list = DT.ITEM_ROLE[h.kind] || []; if (DT.itemsFit(h.u.items, made) && list.includes(made)) sc = Math.max(sc, 10 - list.indexOf(made) * 0.1); }
        if (sc > bestSc) { bestSc = sc; best = j; }
      });
      if (best < 0 || (bestSc <= 0 && p.items.length < 6 && g.stage < 4)) break;
      const made = DT.combineItems(p.items[i], p.items[best]);
      const a = p.items[i], b = p.items[best];
      p.items = p.items.filter((_, k) => k !== i && k !== best);
      p.items.push(made);
      void a; void b;
    }
    // Tome of Traits: the best of its 4 Crests; removers become 1 gold (the AI does not shuffle items)
    for (let guard = 0; guard < 3; guard++) {
      const t = p.items.indexOf('rec_tome');
      if (t < 0) break;
      p.items.splice(t, 1);
      const opts = tomeOptions(c);
      let best = opts[0], bs = -Infinity;
      for (const id of opts) {
        const tr = DT.ITEMS[id].emblem;
        const n = counts[tr] || 0;
        const sc = (DT.TRAITS[tr].bp.includes(n + 1) ? 9 : n ? 5 : 1) + c.rng();
        if (sc > bs) { bs = sc; best = id; }
      }
      c.give(best);
    }
    for (let guard = 0; guard < 3; guard++) {
      const r = p.items.indexOf('rec_remover');
      if (r < 0) break;
      p.items.splice(r, 1);
      p.gold += 1;
    }
  }

  // ---------- the set mechanic ----------
  const mechanic = {
    id: 'shadow',
    name: 'Shadow and Radiant',
    desc: 'Spore components make Shadow items: stronger, with a drawback. Every item has a Radiant upgrade, offered at the 4-1 Radiant Armory and by the Radiant Blessing, which comes once when you drop low.',
    hooks: {
      // game hooks
      onRoundStart(c) {
        hatchEggs(c);
        const p = c.player;
        if (blessDue(c) && !p.offer && c.game.roundType !== 'carousel') deliverBlessing(c);
      },
      onAiTurn(c) { aiCraftShadow(c); },
      // combat hooks (every unit)
      onCombatStart(c) {
        if (!c.info || c.info.type !== 'pve') return;
        const muls = R().pveMul || [];
        const m = muls[Math.min(c.info.stage, muls.length - 1)] || 1;
        if (m === 1) return;
        for (const u of c.units) if (u.team === 1) { u.maxHp = Math.round(u.maxHp * m); u.ad *= m; u.baseAd *= m; }
      },
      onAttack(c, e) { const u = e.unit; if (u.state.lunar) u.mana = Math.max(0, u.mana - u.state.lunar); },
      onCast(c, e) { const u = e.unit; if (u.state.shroudMax) { u.maxMana = u.state.shroudMax; u.state.shroudMax = 0; } },
      onBeforeDamageTaken(c, e) {
        const t = e.target, s = t.state;
        if (!s.spellShield || e.kind !== 'magic' || e.isBurn || !e.source || e.source.team === t.team) return;
        s.spellShield = false;
        e.mult = 0;
        c.text(t, 'Blocked');
        if (s.trapStun && e.source.alive) { s.trapStun = false; c.stun(e.source, 3); }
      },
      onUnitTick(c, e) {
        const u = e.unit, s = u.state;
        if (s.curse) {
          if (c.t >= s.curse.until) s.curse = null;
          else if (c.t >= s.curseNext) { s.curseNext += 1; c.damage(s.curse.src, u, u.maxHp * (s.curse.rate || 0.04), 'true'); if (!u.alive) return; }
        }
        if (s.decay && c.t >= s.decayNext) { s.decayNext += 1; c.damage(u, u, u.maxHp * s.decay, 'true'); }
      },
      onLethal(c, e) {
        const u = e.unit;
        if (u.hp > 0) return;
        const r = c.teamState(u.team).respite;
        if (r && c.t < r.until && r.src.alive && c.dist(r.src, u) <= 2) { u.hp = 1; c.text(u, 'Respite'); }
      },
      onDeath(c, e) {
        const u = e.unit;
        if (u.state.unstable) {
          for (const t of c.enemiesNear(u.team, u, 1)) c.damage(u, t, 250, 'true');
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
        }
        const ps = u.state.possess;
        if (ps && c.t <= ps.until && ps.by.alive && !u.summoned) resurrect(c, ps.by, u);
      },
    },
    offerKinds: {
      rec_bless: {
        name: 'Radiant Blessing',
        sub: 'You dropped low. Pick one Radiant upgrade: it upgrades an item you hold, or goes to your item bench.',
        options: (c, exclude) => radiantOptions(c, 3, exclude || []),
        choose: (c, id) => grantRadiant(c, id),
        aiScore: (c, id) => radiantScore(c, id),
        card: itemCard,
      },
      rec_radiant: {
        name: 'Radiant Armory',
        sub: 'Pick one Radiant upgrade: it upgrades an item you hold, or goes to your item bench.',
        options: (c, exclude) => radiantOptions(c, 4, exclude || []),
        choose: (c, id) => grantRadiant(c, id),
        aiScore: (c, id) => radiantScore(c, id),
        card: itemCard,
      },
      rec_tome: {
        name: 'Tome of Traits',
        sub: 'Pick one Crest. It goes to your item bench.',
        options: (c) => tomeOptions(c),
        choose: (c, id) => c.give(id),
        aiScore: (c, id) => { const t = DT.ITEMS[id].emblem; const n = DT.countTraits(c.player.boardUnits())[t] || 0; return DT.TRAITS[t].bp.includes(n + 1) ? 9 : n ? 5 : 1; },
        card: (id) => ({ name: DT.ITEMS[id] ? DT.ITEMS[id].name : id, desc: DT.ITEMS[id] ? DT.ITEMS[id].desc : '', img: DT.ItemArt ? DT.ItemArt.img(id, 64) : '' }),
      },
    },
  };

  DT.Sets.extend(SID, { traits, items, augments, abilities, mechanic });
})(typeof window !== 'undefined' ? window : globalThis);
