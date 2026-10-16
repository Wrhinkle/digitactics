// Into the Arcane behavior: trait hooks, item hooks, consumables, augments, set ability types and the set mechanic
// (Anomalies, opening portals, the Kowloon Syndicate black market, the Junk King workshop, Tamer item gold).
// Contexts: docs/sets/FORMAT.md. Everything is deterministic (c.rng / game.rng only).
(function (G) {
  const DT = G.DT;
  const has = (u, t) => u.traits.includes(t);
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const all = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const T = () => DT.Sets.def('arcane').traits;

  // apply one item's stats and passives to a combat unit (Junkyard: a component acting as its full item).
  // minus: the component id whose stats the unit already has.
  function applyItem(u, id, minus) {
    const it = DT.ITEMS[id];
    if (!it) return;
    const st = { ...(it.stats || {}) };
    const sub = (DT.ITEMS[minus] && DT.ITEMS[minus].stats) || {};
    for (const k in sub) st[k] = (st[k] || 0) - sub[k];
    if (st.ad) u.ad += u.baseAd * st.ad;
    if (st.as) u.as *= 1 + st.as;
    if (st.ap) u.ap += st.ap;
    if (st.mana) u.mana += st.mana;
    if (st.armor) u.armor += st.armor;
    if (st.mr) u.mr += st.mr;
    if (st.hp) u.maxHp += st.hp;
    if (st.crit) u.crit += st.crit;
    const P = u.P;
    for (const k in it.p || {}) {
      const v = it.p[k];
      if (k === 'emblem' || k === 'teamSize' || k === 'thief') continue;
      if (k === 'lifeline') { u.lifelines.push({ ...v, used: false }); continue; }
      if (typeof v === 'number') {
        P[k] = (P[k] || 0) + v;
        if (k === 'amp') u.amp += v;
        else if (k === 'vamp') u.omnivamp += v;
        else if (k === 'critDmg') u.critDmg += v;
        else if (k === 'dr') u.dr += v;
        else if (k === 'regen') u.regen += v;
      } else {
        if (k === 'bluebuff' && !P.bluebuff) u.maxMana = Math.max(20, u.maxMana - 10);
        P[k] = v;
        if (k === 'abilCrit') u.abilCrit = true;
      }
    }
  }

  // remove a unit from combat without a knockout (Devour Anomaly)
  function removeUnit(c, a) {
    c.remove(a);
    a.hp = 0;
    c.event({ type: 'death', at: a.id });
  }

  // ---------- traits ----------
  function freeBel(c) {
    const b = c.state.bel;
    if (!b || !b.state.chained || !b.alive) return;
    b.state.chained = false;
    b.stun = 0;
    c.text(b, 'Unleashed');
    if (c.lv >= 3) {
      c.heal(b, b.maxHp);
      c.event({ type: 'burst', c: b.c, r: b.r, radius: 2, team: b.team });
      for (const e of c.enemiesNear(b.team, b, 2)) c.damage(b, e, b.ad * 2.5, 'magic');
    }
  }

  function applyEnvoy(c, src) {
    switch (src.line.envoy) {
      case 'tristana': for (const u of c.mine) u.as *= 1 + 0.05 * (src.stage + 1); break;
      case 'nami': c.state.nami = true; break;
      case 'ambessa': for (const u of c.mine) { u.armor += 10; u.mr += 10; } break;
      case 'garen': for (const u of c.mine) if (u.r === src.r && Math.abs(u.c - src.c) <= 1) u.maxHp += Math.round(src.maxHp * 0.12); break;
    }
  }

  // Bagra Army war chest: gold and a component; every 3rd chest a completed item instead; with 6, 2 more gold
  function openChest(c, n, lv) {
    if (n % 3 === 0) c.give(c.randomCompleted());
    else { c.player.gold += 2; c.give(c.randomComponent()); }
    if (lv >= 3) c.player.gold += 2;
  }

  const traits = {
    ACA: {
      hooks: {
        onCombatStart(c) {
          const sp = (c.info && c.info.mech && c.info.mech.arcSponsor) || [];
          for (const u of c.mine) {
            const n = sp.length ? u.items.filter((id) => sp.includes(id)).length : 0;
            const mem = c.isMember(u);
            const f = c.v * n * (mem ? 2 : 1) + (mem ? c.val('base') : 0);
            if (f > 0) { u.maxHp = Math.round(u.maxHp * (1 + f)); u.amp += f; }
          }
        },
        onRoundEnd(c) {
          const sp = c.game.mech.arcSponsor || [];
          const m = c.player.mech;
          m.arcAcaLv = m.arcAcaLv || 0;
          while (m.arcAcaLv < c.lv && sp.length) { m.arcAcaLv++; c.give(c.pick(sp)); }
        },
      },
    },
    AUT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.armor += c.val('arm'); u.mr += c.val('arm'); u.state.aut = { n: 0, acc: 0, dmg: c.v }; } },
        onDamageDealt(c, e) {
          const u = e.unit, s = u.state.aut;
          if (!s || s.busy || e.isBurn || !(e.amount > 0)) return;
          s.n++;
          s.acc += e.amount;
          if (s.n < 12 || !e.target.alive) return;
          s.n = 0;
          const dmg = s.dmg + s.acc * 0.25;
          s.acc = 0;
          s.busy = true;
          c.event({ type: 'beam', from: u.id, to: e.target.id });
          c.text(u, 'Overload');
          c.damage(u, e.target, dmg, 'magic');
          s.busy = false;
        },
      },
    },
    BR: {
      hooks: {
        onCombatStart(c) {
          const mem = c.members.filter((u) => !u.summoned);
          const stars = mem.reduce((s, u) => s + u.stage + 1, 0);
          const mul = (0.22 + 0.045 * stars) * c.v;
          const rows = c.team === 0 ? [4, 5] : [3, 2];
          let b = null;
          for (const r of rows) {
            for (const col of [3, 2, 4, 1, 5, 0, 6]) if (!c.unitAt(col, r)) { b = c.summon(c.team, 'arc_s_belphemon', 0, col, r, { hpMul: mul, adMul: mul, noHooks: true }); break; }
            if (b) break;
          }
          if (!b) return;
          b.stun = 999;
          b.state.chained = true;
          c.state.bel = b;
          c.state.casts = 0;
          c.text(b, 'Chained');
        },
        onCast(c, e) { if (c.state.bel && c.state.bel.state.chained && c.isMember(e.unit) && ++c.state.casts >= 5) freeBel(c); },
        onDamageTaken(c, e) { const b = c.state.bel; if (b && e.unit === b && b.state.chained && b.hp < b.maxHp * 0.65) freeBel(c); },
        onBeforeDamage(c, e) { if (c.lv >= 2 && c.isMember(e.unit) && e.target.team !== c.team) e.mult *= 1.15; },
      },
    },
    SYN: {
      hooks: {
        onCombatStart(c) {
          const m = c.side(c.team).mech || {};
          for (const u of c.members) { u.omnivamp += c.v; u.maxHp = Math.round(u.maxHp * (1 + c.val('hp'))); if (m.arcSynHp) u.maxHp += m.arcSynHp; if (m.arcSynAug) u.maxHp += 150; }
        },
        onRoundEnd(c, e) {
          if (e.pve) return;
          const m = c.player.mech;
          m.arcSynLv = c.lv;
          // standard pace has more player combats per stage: TFT13's own Shimmer amounts (about 55% of the Hyper Roll ones)
          const k = c.game.pace === 'normal' ? 0.55 : 1;
          addShimmer(m, Math.round(k * (c.player.streak <= -3 ? c.val('streakGain') : c.val('gain'))));
        },
      },
      ai: { bonus: (a) => (a.stage <= 4 ? 1 : 0) },
    },
    CON: {
      hooks: {
        onCombatStart(c) {
          const ch = Math.min(10, (c.side(c.team).mech || {}).arcChests || 0);
          const f = c.v + 0.06 * ch;
          for (const u of c.members) { u.ad += u.baseAd * f; u.ap += Math.round(100 * f); }
        },
        onKill(c, e) { if (c.isMember(e.unit)) e.unit.state.arcConK = (e.unit.state.arcConK || 0) + 1; },
        onRoundEnd(c, e) {
          const cb = e.match && e.match.combat;
          if (!cb) return;
          let k = 0;
          for (const u of cb.units) if (u.team === e.team && u.state.arcConK) k += u.state.arcConK;
          const m = c.player.mech;
          m.arcConquest = (m.arcConquest || 0) + k * c.val('mult');
          while (m.arcConquest >= 10) { m.arcConquest -= 10; m.arcChests = (m.arcChests || 0) + 1; openChest(c, m.arcChests, c.lv); }
        },
      },
    },
    EMI: {
      hooks: {
        onCombatStart(c) {
          const env = c.members.filter((u) => u.line.envoy);
          const full = c.n >= 4;
          const srcs = full ? env : env.slice(0, 1);
          for (const s of srcs) applyEnvoy(c, s);
          if (full) for (const u of env) { u.maxHp += 250; u.amp += 0.2; }
        },
        onAttack(c, e) { if (c.state.nami) e.unit.mana = Math.min(e.unit.maxMana, e.unit.mana + 3); },
      },
    },
    ENF: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) { u.shield += u.maxHp * c.val('sh'); u.amp += c.v; }
          const foes = c.units.filter((e) => e.team !== c.team && e.alive).sort((a, b) => b.maxHp - a.maxHp || a.id - b.id).slice(0, c.val('n'));
          for (const f of foes) { f.state.arcWanted = (f.state.arcWanted || 0) | (1 << c.team); c.text(f, 'Wanted'); }
        },
        onAnyDeath(c, e) {
          const d = e.unit;
          if (d.team === c.team || !((d.state.arcWanted || 0) & (1 << c.team))) return;
          for (const u of c.members) if (u.alive) { u.as *= 1.3; c.text(u, 'Bounty'); }
        },
      },
    },
    EXP: {
      hooks: {
        onCombatStart(c) {
          const mem = c.members.filter((u) => !u.summoned).sort((a, b) => b.line.tier - a.line.tier || b.stage - a.stage || a.id - b.id);
          const lab = mem.slice(0, c.val('labs'));
          const bon = {};
          for (const u of lab) if (u.line.exp) bon[u.line.exp] = true;
          for (const u of lab) { u.maxHp += c.v + (bon.mundo ? 200 : 0); u.state.exp = bon; }
        },
        onDamageDealt(c, e) {
          const u = e.unit, b = u.state.exp;
          if (!b || u.state.expBusy || e.isBurn || !e.target.alive) return;
          u.state.expBusy = true;
          if (b.zyra && e.kind === 'magic') c.damage(u, e.target, e.amount * 0.2, 'true');
          if (b.nunu && e.target.alive && !(u.state.nunuCd > c.t)) { u.state.nunuCd = c.t + 1; c.damage(u, e.target, e.target.maxHp * 0.03, 'magic'); }
          u.state.expBusy = false;
        },
        onCast(c, e) { const b = e.unit.state.exp; if (b && b.urgot) { c.shield(e.unit, e.unit.maxHp * 0.15); c.buff(e.unit, 'as', 0.3, 5); } },
        onAttack(c, e) {
          const u = e.unit, b = u.state.exp;
          if (!b || !b.twitch || u.nAtk % 5 !== 0) return;
          const t = c.target(u, 'closest');
          if (t) { u.state.expBusy = true; c.damage(u, t, t.maxHp * 0.06, 'phys'); u.state.expBusy = false; }
        },
        onKill(c, e) { const b = e.unit.state.exp; if (b && b.mundo && e.unit.alive) { e.unit.maxHp += 60; c.heal(e.unit, 60); } },
      },
    },
    FAM: { hooks: each((u, c) => { u.maxMana = Math.max(10, Math.round(u.maxMana * (1 - c.v))); u.dr += 0.12; if (c.lv >= 2) u.as *= 1.2; }) },
    FIRE: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.fire = { v: c.v, last: 0, lv: c.lv }; },
        onUnitTick(c, e) {
          const u = e.unit, s = u.state.fire;
          if (!s || !c.every(u, 'arcFire', 6)) return;
          const h = (u.taken - s.last) * s.v;
          s.last = u.taken;
          if (h > 0) c.heal(u, h);
          c.text(u, 'Surge');
          if (s.lv >= 2) c.buff(u, 'as', 0.6, 2);
        },
      },
    },
    MODE: { hooks: each((u, c) => { if (u.homeR <= 5) u.dr += c.val('dr'); else u.amp += c.v; }) },
    REB: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.maxHp = Math.round(u.maxHp * (1 + c.val('hp'))); },
        onCombatReady(c) { c.state.total = c.mine.reduce((s, u) => s + u.maxHp, 0); c.state.fired = false; },
        onTick(c) {
          if (c.state.fired || !c.state.total) return;
          let cur = 0;
          for (const u of c.units) if (u.alive && u.team === c.team) cur += u.hp;
          if (cur > c.state.total * 0.75) return;
          c.state.fired = true;
          for (const u of c.members) if (u.alive) { c.buff(u, 'as', 0.6, 5); u.ad += u.baseAd * c.v; u.ap += Math.round(100 * c.v); c.text(u, 'Flare'); }
          if (c.lv >= 3) for (const e of c.units) if (e.alive && e.team !== c.team) c.stun(e, 2);
        },
      },
    },
    SCR: {
      hooks: {
        onCombatStart(c) {
          let comps = 0;
          for (const u of c.mine) {
            for (const id of u.items) {
              const it = DT.ITEMS[id];
              if (!it) continue;
              if (it.component) { if (!(DT.COMPONENTS[id] || {}).special) comps += 1; } else if (it.from && it.from.every((x) => DT.isComponent(x))) comps += 2;
            }
          }
          const sh = c.val('base') + comps * c.v;
          for (const u of c.members) {
            u.shield += sh;
            let left = c.val('n');
            for (const id of u.items.slice()) {
              if (left <= 0) break;
              if (!DT.isComponent(id) || (DT.COMPONENTS[id] || {}).special) continue;
              const full = DT.combineItems(id, id);
              if (full) { applyItem(u, full, id); left--; }
            }
          }
        },
      },
    },
    HR: {
      hooks: {
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          c.buff(u, 'dr', 0.5, 1.5);
          const r = c.rng(), k = 1 + u.stage;
          if (r < 0.3) {
            for (let i = 0; i < 8; i++) { const t = c.target(u, 'random'); if (!t) break; c.event({ type: 'shot', from: u.id, to: t.id }); c.damage(u, t, 35 * k, 'phys'); }
            c.text(u, 'Rockets');
          } else if (r < 0.6) { c.shield(u, 180 * k); c.text(u, 'Shield'); }
          else if (r < 0.85) { u.state.coins = Math.min(3, (u.state.coins || 0) + 1); c.text(u, 'Coin'); }
          else { if (e.target && e.target.alive) c.abilityHit(u, e.target, e.value); c.text(u, 'Jackpot'); }
        },
        onRoundEnd(c, e) {
          const cb = e.match && e.match.combat;
          if (!cb) return;
          let g = 0;
          for (const u of cb.units) if (u.team === e.team && u.state.coins) g += u.state.coins;
          e.gold += Math.min(3, g);
        },
      },
    },
    JUNK: {
      hooks: {
        onCombatStart(c) {
          const ups = (c.side(c.team).mech || {}).arcJunk || [];
          if (!ups.length) return;
          const u = c.members.filter((x) => x.line.id === 'arc_chaosdromon').sort((a, b) => b.stage - a.stage || a.id - b.id)[0];
          if (!u) return;
          const j = {};
          for (const id of ups) j[id] = (j[id] || 0) + 1;
          if (j.spare) { u.ap += 8 * j.spare; u.maxHp += 100 * j.spare; }
          if (j.destruct) { u.armor *= 1.25; u.mr *= 1.25; }
          if (JUNK_PARTS.every((k) => j[k])) { u.ap += 25; u.maxHp += 300; }
          u.state.junk = j;
        },
        onUnitTick(c, e) {
          const u = e.unit, j = u.state.junk;
          if (!j) return;
          const k = (1 + u.stage) * (u.ap / 100);
          if (j.flame && c.every(u, 'jFlame', 1)) for (const t of c.enemiesNear(u.team, u, 2)) c.damage(u, t, 30 * k, 'magic');
          if (j.bots && c.every(u, 'jBots', 3)) c.heal(u, u.maxHp * 0.06);
          if (j.rod && c.every(u, 'jRod', 5)) { const t = c.target(u, 'farthest'); if (t) { c.event({ type: 'beam', from: u.id, to: t.id }); c.damage(u, t, 120 * k, 'magic'); } }
          if (j.buster && c.every(u, 'jBust', 3)) {
            const list = c.enemiesOf(u);
            if (list.length) { const t = list.reduce((b, x) => (x.armor + x.mr > b.armor + b.mr ? x : b)); c.damage(u, t, 80 * k, 'true'); }
          }
        },
        onDeath(c, e) {
          const u = e.unit, j = u.state.junk;
          if (!j || !j.destruct) return;
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          for (const t of c.enemiesNear(u.team, u, 2)) c.damage(u, t, (u.armor + u.mr) * 1.6, 'magic');
        },
      },
    },
    // ---- classes ----
    AMB: { hooks: each((u, c) => { u.abilCrit = true; u.crit += c.v; u.critDmg += c.val('cd'); if (c.lv >= 4) u.dr += 0.15; }) },
    ART: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.ad += u.baseAd * c.v; u.state.art = true; } },
        onAttack(c, e) {
          const u = e.unit;
          if (!u.state.art || u.nAtk % 4 !== 0) return;
          c.event({ type: 'burst', c: e.target.c, r: e.target.r, radius: 1, team: u.team });
          for (const t of c.enemiesNear(u.team, e.target, 1)) c.damage(u, t, u.ad * 1.4, 'phys');
        },
      },
    },
    BRU: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.mine) u.maxHp += 80;
          for (const u of c.members) u.maxHp = Math.round(u.maxHp * (1 + c.v));
        },
      },
    },
    DOM: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.shield += c.val('sh'); u.state.dom = c.v; } },
        onCast(c, e) { const u = e.unit; if (u.state.dom) u.ap += Math.round(u.maxMana * u.state.dom * 0.6); },
      },
    },
    PIT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.omnivamp += 0.1; u.state.pit = { v: c.v, heal: c.val('heal') }; } },
        onDamageDealt(c, e) {
          const u = e.unit, s = u.state.pit;
          if (!s || s.busy || e.isBurn || e.kind === 'true' || !e.target.alive) return;
          s.busy = true;
          c.damage(u, e.target, e.amount * s.v, 'true');
          s.busy = false;
        },
        onDamageTaken(c, e) {
          const u = e.unit, s = u.state.pit;
          if (!s || s.healed || u.hp >= u.maxHp * 0.5) return;
          s.healed = true;
          c.text(u, 'Second Wind');
          for (let k = 1; k <= 4; k++) c.after(0.5 * k, () => c.heal(u, (u.maxHp * s.heal) / 4), u);
        },
      },
    },
    QS: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) { u.as *= 1 + c.v; u.state.qs = { cur: c.v, lo: c.v, hi: c.val('hi'), lv: c.lv }; }
        },
        onAttack(c, e) {
          const s = e.unit.state.qs;
          if (!s) return;
          const t = e.target;
          const v = s.lo + (s.hi - s.lo) * (1 - Math.max(0, t.hp) / t.maxHp);
          e.unit.as = (e.unit.as / (1 + s.cur)) * (1 + v);
          s.cur = v;
        },
        onKill(c, e) { const s = e.unit.state.qs; if (s && s.lv >= 3 && e.unit.alive) c.shield(e.unit, 200); },
      },
    },
    SEN: { hooks: all((u, c) => { const k = c.isMember(u) ? 3 : 1; u.armor += c.v * k; u.mr += c.v * k; }) },
    SNI: { hooks: each((u, c) => { u.amp += c.v; u.perHexAmp += c.val('hex'); }) },
    SOR: {
      hooks: {
        onCombatStart(c) { for (const u of c.mine) u.ap += 8 + (c.isMember(u) ? c.v : 0); },
        onCast(c, e) { if (c.lv >= 3 && c.isMember(e.unit) && e.target && e.target.alive) c.weaken(e.target, 0.2, 3); },
      },
    },
    VIS: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.vis = { v: c.v, lv: c.lv }; },
        onAttack(c, e) { const u = e.unit, s = u.state.vis; if (s) u.mana = Math.min(u.maxMana, u.mana + 10 * s.v); },
        onDamageTaken(c, e) { const u = e.unit, s = u.state.vis; if (s && !e.isBurn) u.mana = Math.min(u.maxMana, u.mana + Math.min(15, e.amount * 0.06) * s.v); },
        onDamageDealt(c, e) {
          const u = e.unit, s = u.state.vis;
          if (!s || s.lv < 3 || e.kind !== 'magic' || e.isBurn) return;
          let best = null;
          for (const a of c.units) if (a.alive && a.team === u.team && (!best || a.hp / a.maxHp < best.hp / best.maxHp)) best = a;
          if (best) c.heal(best, e.amount * 0.16, true);
        },
      },
    },
    WAT: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.state.wat = [c.v, c.val('hi')]; },
        onBeforeDamageTaken(c, e) { const u = e.unit, s = u.state.wat; if (s) e.dr += u.hp > u.maxHp * 0.5 ? s[1] : s[0]; },
      },
    },
  };

  // ---------- Shimmer and the black market ----------
  function addShimmer(m, n) {
    m.arcShimmer = (m.arcShimmer || 0) + n;
    while (m.arcShimmer >= 100) { m.arcShimmer -= 100; m.arcMarkets = (m.arcMarkets || 0) + 1; }
  }
  const CONTRABAND = (tier) => Object.values(DT.ITEMS).filter((it) => it.cat === 'contraband' && it.tier === tier).map((it) => it.id);
  function marketOffer(c) {
    const p = c.player, m = p.mech;
    const tier = (m.arcMarketsOpened || 0) >= 2 ? 2 : 1;
    const n = m.arcMarketPlus ? 4 : 3;
    const opts = c.shuffle(CONTRABAND(tier).slice()).slice(0, n);
    const lv = Math.max(1, m.arcSynLv || 1);
    opts.push(`arc_pass:${T().SYN.skipHp[lv - 1]}`);
    return opts;
  }
  const isSyn = (u) => DT.unitTraits(u).includes('SYN');

  // ---------- Junk King workshop ----------
  const JUNK_PARTS = ['flame', 'bots', 'rod', 'destruct', 'buster'];
  const JUNK = {
    flame: { name: 'Flamethrower', desc: 'Every second, burns enemies within 2 hexes for magic damage.' },
    bots: { name: 'Repairing Microbots', desc: 'Every 3 seconds, restores 6% max HP.' },
    rod: { name: 'Petricite Rod', desc: 'Every 5 seconds, blasts the farthest enemy for magic damage.' },
    destruct: { name: 'Self Destruct Button', desc: '+25% armor and MR. When knocked out, deals 160% of armor plus MR as magic damage to enemies within 2 hexes.' },
    buster: { name: 'Tankbuster', desc: 'Every 3 seconds, deals true damage to the enemy with the most armor and MR.' },
    spare: { name: 'Spare Parts', desc: 'A smaller upgrade that stacks: +8 AP and +100 HP.' },
  };
  const ownsLine = (p, id) => p.allUnits().some((u) => u.line === id);

  // ---------- Anomalies ----------
  // X-Antibody: raw mutations. DNA Digivolution: power taken from or shared with allies. Digital Hazard: dangerous code.
  // Each: name, short (combat text), theme, desc, tag (AI: ad / ap / dmg / tank / any), and handlers:
  // start/ready(c, u), attack/cast/kill/taken/dealt/lethal/death(c, u, e), tick(c, u, e), ally(c, u, dead), pick(game ctx, boardUnit)
  const ANOM = {
    x_power: { name: 'X-Antibody: Power', short: 'X Power', tag: 'ad', desc: 'This Digimon gets 55% more attack damage from every source.',
      ready(c, u) { u.ad *= 1.55; } },
    x_guard: { name: 'X-Antibody: Guard', short: 'X Guard', tag: 'tank', desc: 'Gain 15 armor and MR, then 60% more armor and MR from every source.',
      ready(c, u) { u.armor = (u.armor + 15) * 1.6; u.mr = (u.mr + 15) * 1.6; } },
    x_mind: { name: 'X-Antibody: Mind', short: 'X Mind', tag: 'ap', desc: 'Gain 10 AP, then 40% more ability power from every source.',
      ready(c, u) { u.ap = (u.ap + 10) * 1.4; } },
    x_bulwark: { name: 'X-Antibody: Bulwark', short: 'X Bulwark', tag: 'tank', desc: 'Gain 40 armor and MR, tripled while above 50% HP.',
      start(c, u) { u.armor += 120; u.mr += 120; u.state.bulwark = true; },
      taken(c, u) { if (u.state.bulwark && u.hp < u.maxHp * 0.5) { u.state.bulwark = false; u.armor -= 80; u.mr -= 80; } } },
    x_berserk: { name: 'X-Antibody: Berserk', short: 'X Berserk', tag: 'dmg', desc: 'Gain 30 AP and 30% AD. Triple it below 50% HP.',
      start(c, u) { u.ad += u.baseAd * 0.3; u.ap += 30; },
      taken(c, u) { if (!u.state.rage && u.hp < u.maxHp * 0.5) { u.state.rage = true; u.ad += u.baseAd * 0.6; u.ap += 60; c.text(u, 'Rage'); } } },
    x_giant: { name: 'X-Antibody: Giant', short: 'X Giant', tag: 'tank', desc: 'Gain 800 max HP.',
      start(c, u) { u.maxHp += 800; } },
    x_velocity: { name: 'X-Antibody: Velocity', short: 'X Velocity', tag: 'dmg', desc: 'Gain 10% attack speed. Each cast gives 15% more attack speed for the rest of combat.',
      start(c, u) { u.as *= 1.1; }, cast(c, u) { u.as *= 1.15; } },
    x_ascend: { name: 'X-Antibody: Ascension', short: 'X Ascension', tag: 'any', desc: 'After 10 seconds of combat, gain 50% max HP and 60% attack speed.',
      tick(c, u) { if (u.state.asc || c.t < 10) return; u.state.asc = true; const add = u.maxHp * 0.5; u.maxHp += add; c.heal(u, add); u.as *= 1.6; c.text(u, 'Ascend'); } },
    dna_hunger: { name: 'DNA Digivolution: Devour', short: 'Devour', tag: 'any', desc: 'At the start of combat, absorb the nearest ally: it leaves the fight and this Digimon gains 90% of its max HP and attack damage.',
      ready(c, u) {
        let best = null;
        for (const a of c.units) if (a !== u && a.alive && a.team === u.team && !a.summoned && (!best || c.dist(u, a) < c.dist(u, best) || (c.dist(u, a) === c.dist(u, best) && a.id < best.id))) best = a;
        if (!best) return;
        u.maxHp += best.maxHp * 0.9;
        u.ad += best.ad * 0.9;
        removeUnit(c, best);
        c.text(u, 'Devour');
      } },
    dna_share: { name: 'DNA Digivolution: Share', short: 'Share', tag: 'tank', desc: 'At the start of combat, adjacent allies gain 12% of this Digimon\'s attack damage, ability power, max HP, armor and MR.',
      ready(c, u) { for (const a of c.adjacent(u)) if (a.team === u.team && a.alive) { a.ad += u.ad * 0.12; a.ap += u.ap * 0.12; a.maxHp += u.maxHp * 0.12; a.armor += u.armor * 0.12; a.mr += u.mr * 0.12; } } },
    dna_power: { name: 'DNA Digivolution: Absorb Power', short: 'Absorb', tag: 'ad', desc: 'Gain 30% AD. Each time an ally is knocked out, gain 20% of its attack damage.',
      start(c, u) { u.ad += u.baseAd * 0.3; }, ally(c, u, d) { u.ad += d.ad * 0.2; } },
    dna_energy: { name: 'DNA Digivolution: Absorb Energy', short: 'Absorb', tag: 'ap', desc: 'Gain 30 AP. Each time an ally is knocked out, gain 20% of its ability power.',
      start(c, u) { u.ap += 30; }, ally(c, u, d) { u.ap += d.ap * 0.2; } },
    dna_mana: { name: 'DNA Digivolution: Nothing Wasted', short: 'Recycle', tag: 'ap', desc: 'Start with 20 more mana and gain 15% attack speed. When an ally is knocked out, gain 70% of its mana.',
      start(c, u) { u.mana += 20; u.as *= 1.15; }, ally(c, u, d) { u.mana = Math.min(u.maxMana, u.mana + d.mana * 0.7); } },
    dna_partners: { name: 'DNA Digivolution: Partners', short: 'Partners', tag: 'ad', desc: 'Two Garurumon join the fight beside this Digimon, each with 25% of its max HP and 40% of its attack damage.',
      start(c, u) {
        for (let k = 0; k < 2; k++) {
          const cell = c.freeCellNear(u);
          if (!cell) break;
          const w = c.summon(u.team, 'arc_s_garurumon', 0, cell.c, cell.r, { noHooks: true });
          if (!w) break;
          w.maxHp = Math.round(u.maxHp * 0.25); w.hp = w.maxHp; w.ad = u.ad * 0.4; w.baseAd = w.ad; w.armor = u.armor * 0.6; w.mr = u.mr * 0.6;
        }
      } },
    dna_familiar: { name: 'DNA Digivolution: Spare Body', short: 'Spare Body', tag: 'tank', desc: 'When knocked out, a ShellNumemon takes its place with this Digimon\'s max HP, armor and MR.',
      death(c, u) {
        const s = c.summon(u.team, 'arc_s_shellnumemon', 0, u.c, u.r, { noHooks: true });
        if (!s) return;
        s.maxHp = Math.round(u.maxHp); s.hp = s.maxHp; s.armor = u.armor; s.mr = u.mr; s.ad = u.ad * 0.4; s.baseAd = s.ad;
        c.text(s, 'Spare Body');
      } },
    dna_shield: { name: 'DNA Digivolution: Guardian Link', short: 'Link', tag: 'tank', desc: 'At the start of combat and every 8 seconds, shields itself and the 2 lowest-health allies for 12% of its max HP.',
      start(c, u) { u.state.linkNow = true; },
      tick(c, u) {
        if (!u.state.linkNow && !c.every(u, 'aLink', 8)) return;
        u.state.linkNow = false;
        const amt = u.maxHp * 0.12;
        const allies = c.units.filter((a) => a.alive && a.team === u.team && a !== u).sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id).slice(0, 2);
        for (const a of [u, ...allies]) c.shield(a, amt);
      } },
    dh_fireball: { name: 'Digital Hazard: Fireball', short: 'Fireball', tag: 'ap', desc: 'Each cast also throws a fireball (two with 120+ max mana): enemies in the blast take 6% of their max HP as true damage and burn.',
      cast(c, u) {
        const n = u.maxMana >= 120 ? 2 : 1;
        for (let k = 0; k < n; k++) {
          const t = c.target(u, 'random');
          if (!t) break;
          c.event({ type: 'burst', c: t.c, r: t.r, radius: 1, team: u.team });
          for (const x of c.enemiesNear(u.team, t, 1)) { c.damage(u, x, x.maxHp * 0.06, 'true'); c.burn(u, x); }
        }
      } },
    dh_laser: { name: 'Digital Hazard: Laser Eyes', short: 'Laser Eyes', tag: 'ap', desc: 'Every second, a laser hits enemies in a 3-hex line toward its target for 80% of its ability power as magic damage and burns them.',
      tick(c, u) {
        if (!c.every(u, 'aLaser', 1)) return;
        const tg = u.target && u.target.alive ? u.target : c.target(u, 'closest');
        if (!tg) return;
        for (const e of c.lineTargets(u, tg)) if (c.dist(u, e) <= 3) { c.event({ type: 'beam', from: u.id, to: e.id }); c.damage(u, e, u.ap * 0.8, 'magic'); c.burn(u, e); }
      } },
    dh_slime: { name: 'Digital Hazard: Slime Time', short: 'Slime', tag: 'tank', desc: 'Every 2 seconds, deals 110% of its ability power as magic damage to adjacent enemies and heals for the damage dealt.',
      tick(c, u) {
        if (!c.every(u, 'aSlime', 2)) return;
        let total = 0;
        for (const e of c.enemiesNear(u.team, u, 1)) total += c.damage(u, e, u.ap * 1.1, 'magic');
        if (total > 0) c.heal(u, total);
      } },
    dh_cooker: { name: 'Digital Hazard: Meltdown', short: 'Meltdown', tag: 'tank', desc: 'Every second, deals 3.5% of its max HP as magic damage to nearby enemies. The radius grows every 6 seconds.',
      tick(c, u) {
        if (!c.every(u, 'aCook', 1)) return;
        const r = 1 + Math.floor(c.t / 6);
        for (const e of c.enemiesNear(u.team, u, r)) c.damage(u, e, u.maxHp * 0.035, 'magic');
      } },
    dh_finisher: { name: 'Digital Hazard: Finisher', short: 'Finisher', tag: 'dmg', desc: 'Its attacks and abilities knock out enemies below 18% HP.',
      dealt(c, u, e) {
        const t = e.target;
        if (u.state.fin || !t.alive || t.summoned || t.hp >= t.maxHp * 0.18) return;
        u.state.fin = true;
        c.text(t, 'Finished');
        c.damage(u, t, (t.hp + t.shield) * 3 + 10, 'true');
        u.state.fin = false;
      } },
    dh_cuts: { name: 'Digital Hazard: Thousand Cuts', short: 'Cuts', tag: 'ad', desc: 'Attacks deal 25 bonus true damage, plus 10 more for each earlier cut on that target (scales with star level).',
      attack(c, u, e) {
        const t = e.target;
        if (!t.alive) return;
        const key = `cut${u.id}`;
        const n = t.state[key] || 0;
        t.state[key] = n + 1;
        c.damage(u, t, (25 + 10 * n) * (1 + 0.5 * u.stage), 'true');
      } },
    dh_dragonsoul: { name: 'Digital Hazard: Dragon Soul', short: 'Dragon Soul', tag: 'ad', desc: 'Its first attack on each new target deals 16% of the target\'s max HP as true damage.',
      attack(c, u, e) { const t = e.target; if (t === u.state.ds || !t.alive) return; u.state.ds = t; c.damage(u, t, t.maxHp * 0.16, 'true'); } },
    dh_lastchance: { name: 'Digital Hazard: Last Chance', short: 'Last Chance', tag: 'dmg', desc: 'The first time it would be knocked out, it returns at full HP with 50% attack speed and 20% omnivamp, but loses 10% max HP every second.',
      lethal(c, u) {
        if (u.state.lc) return;
        u.state.lc = true;
        u.hp = u.maxHp;
        u.as *= 1.5;
        u.omnivamp += 0.2;
        u.state.decay = true;
        c.text(u, 'Last Chance');
      },
      tick(c, u, e) {
        if (!u.state.decay) return;
        u.hp -= u.maxHp * 0.1 * e.dt;
        if (u.hp <= 0) { u.state.decay = false; c.kill(u, null); }
      } },
    dh_thorns: { name: 'Digital Hazard: Thornskin', short: 'Thornskin', tag: 'tank', desc: 'Gain 60 armor and MR. When damaged, deals 40% of the damage taken to adjacent enemies as magic damage (once a second).',
      start(c, u) { u.armor += 60; u.mr += 60; },
      taken(c, u, e) {
        if (u.state.thCd > c.t || !(e.amount > 0)) return;
        u.state.thCd = c.t + 1;
        for (const x of c.enemiesNear(u.team, u, 1)) c.damage(u, x, e.amount * 0.4, 'magic');
      } },
    u_crest: { name: 'Crest Copy', short: 'Crest Copy', tag: 'any', desc: 'Gain a Crest for one of this Digimon\'s traits (the one closest to its next breakpoint). 6 gold if it has none.',
      pick(c, bu) {
        const L = DT.LINE[bu.line];
        const counts = DT.countTraits(c.player.boardUnits());
        const cand = L.traits.filter((t) => DT.ITEMS[`arc_crest_${t.toLowerCase()}`] && DT.TRAITS[t].bp[0] > 1);
        if (!cand.length) { c.player.gold += 6; return; }
        const gap = (t) => { const n = counts[t] || 0; const next = DT.TRAITS[t].bp.find((b) => b > n); return next ? next - n : 99; };
        cand.sort((a, b) => gap(a) - gap(b));
        c.give(`arc_crest_${cand[0].toLowerCase()}`);
      } },
    u_dual: { name: 'Dual Wielding', short: 'Dual Wield', tag: 'any', desc: 'Gain 2 completed items that suit this Digimon.',
      pick(c, bu) {
        const L = DT.LINE[bu.line];
        const kind = L.ai.carry ? (L.ai.dmg === 'ap' ? 'ap' : 'ad') : 'tank';
        const list = (DT.ITEM_ROLE[kind] || []).filter((id) => DT.CORE_ITEMS.includes(id)).slice(0, 8);
        const picks = c.shuffle(list.slice()).slice(0, 2);
        for (const id of picks) c.give(id);
      } },
  };
  for (const [id, a] of Object.entries(ANOM)) { a.id = id; a.theme = id.startsWith('x_') ? 'X-Antibody' : id.startsWith('dna_') ? 'DNA Digivolution' : id.startsWith('dh_') ? 'Digital Hazard' : 'Utility'; }
  const ANOM_IDS = Object.keys(ANOM);
  const anomOf = (u) => (u && u.mods && u.mods.arcAnom && ANOM[u.mods.arcAnom]) || null;
  const dispatch = (key) => (c, e) => { const a = anomOf(e.unit); if (a && a[key]) a[key](c, e.unit, e); };

  // the Digimon that gets the Anomaly: AI -> main carry; human -> the center hex (column 4, row 5), else the strongest
  function anomalyTarget(c) {
    const p = c.player, g = c.game;
    const free = (u) => u && !(u.mods && u.mods.arcAnom);
    if (!p.isHuman) { const h = g.aiHolders(p).find((x) => free(x.u)); if (h) return h.u; }
    const center = p.board[DT.cellKey(3, 5)];
    if (free(center)) return center;
    const list = p.boardUnits().filter(free).sort((a, b) => DT.unitValue(b) - DT.unitValue(a) || b.items.length - a.items.length || a.uid - b.uid);
    return list[0] || p.bench.find(free) || null;
  }
  function applyAnomaly(c, id) {
    const p = c.player, m = p.mech;
    const a = ANOM[id];
    if (!a) return;
    const u = anomalyTarget(c);
    if (!u) { p.gold += 8; return; }
    u.mods = { ...(u.mods || {}), arcAnom: id };
    (m.arcAnoms = m.arcAnoms || []).push(id);
    m.arcAnomaly = id;
    if (a.pick) a.pick(c, u);
    if (m.arcExtra > 0) { m.arcExtra--; m.arcPendingAnom = true; }
  }
  function anomalyOptions(c, exclude = []) {
    const p = c.player, m = p.mech;
    if (exclude.length) {
      if (p.gold < 1) return exclude.slice();
      p.gold -= 1;
    }
    m.arcSeen = m.arcSeen || [];
    let pool = ANOM_IDS.filter((id) => !exclude.includes(id) && !m.arcSeen.includes(id));
    if (pool.length < 3) { m.arcSeen = []; pool = ANOM_IDS.filter((id) => !exclude.includes(id)); }
    const out = c.shuffle(pool.slice()).slice(0, 3);
    m.arcSeen.push(...out);
    return out;
  }
  function anomalyScore(c, id) {
    const u = anomalyTarget(c);
    if (!u) return 0;
    const L = DT.LINE[u.line];
    const kind = L.ai.carry ? (L.ai.dmg === 'ap' ? 'ap' : 'ad') : 'tank';
    const tag = ANOM[id].tag;
    if (tag === kind) return 5;
    if (tag === 'dmg' && kind !== 'tank') return 4;
    if (tag === 'any') return 3;
    return 0;
  }

  // ---------- opening portals (TFT13 encounters; one is rolled for the whole lobby) ----------
  const PORTALS = [
    { id: 'gold', name: 'Gold Opener', desc: 'Every Tamer starts with 6 more gold.' },
    { id: 'anvils', name: 'Component Anvils', desc: 'Every Tamer starts with 2 random components.' },
    { id: 'tag', name: 'Crest Tag', desc: 'Every Tamer starts with a Crest Tag.' },
    { id: 'device', name: 'Spare Digivice', desc: 'Every Tamer starts with a Digivice.' },
    { id: 'copier', name: 'Data Copier', desc: 'Every Tamer starts with a Data Copier.' },
    { id: 'upgraded', name: 'Upgraded Partner', desc: 'Every Tamer starts with a random 1-cost Digimon in its Champion form.' },
    { id: 'golden', name: 'Golden Prelude', desc: 'The first augment of the game is Gold.' },
    { id: 'prismatic', name: 'Prismatic Finale', desc: 'The last augment of the game is Prismatic.' },
    { id: 'rolls', name: 'Free Rolls', desc: 'Every Tamer gets 2 free rerolls at the start of each stage.' },
    { id: 'none', name: 'No Portal', desc: 'Nothing special this game.' },
  ];
  const PORTAL = {};
  for (const x of PORTALS) PORTAL[x.id] = x;
  // what the codex shows for the game in progress (display only, never read by the rules)
  const SHOWN = { portal: null, sponsor: [] };

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
        const it = DT.ITEMS[x];
        if (DT.isComponent(x)) return (DT.COMPONENTS[x] || {}).special ? x : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (it.emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (it.cat === 'artifact' || it.cat === 'contraband') return c.pick(Object.values(DT.ITEMS).filter((o) => o.cat === it.cat && o.id !== x && (it.cat !== 'contraband' || o.tier === it.tier)).map((o) => o.id)) || x;
        if (it.cat === 'special') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  function copyUnit(c, u, idx, maxTier) {
    const p = c.player, g = c.game;
    const L = DT.LINE[u.line];
    if (!L || !L.shop || L.tier > maxTier) return 'none';
    if (!(g.pool[u.line] > 0)) return 'none';
    const free = p.freeBenchSlot();
    if (free < 0) return 'benchFull';
    const id = p.items[idx];
    p.items.splice(idx, 1);
    g.takeFromPool(u.line);
    p.bench[free] = g.newUnit(u.line, 0);
    g.combine(p);
    return id;
  }

  // ---------- items ----------
  const items = {
    arc_remover: { use: (c, u, idx) => giveBack(c, u, idx, false) },
    arc_reforger: { use: (c, u, idx) => giveBack(c, u, idx, true) },
    arc_copier: { use: (c, u, idx) => copyUnit(c, u, idx, 5) },
    arc_copier_small: { use: (c, u, idx) => copyUnit(c, u, idx, 3) },
    arc_durandal: {
      hooks: {
        onDamageDealt(c, e) {
          if (e.unit !== c.unit || e.isBurn || !(e.amount > 0)) return;
          let best = null;
          for (const a of c.units) if (a.alive && a.team === c.team && a !== c.unit && (!best || a.hp / a.maxHp < best.hp / best.maxHp)) best = a;
          if (best) c.heal(best, e.amount * 0.1, true);
        },
      },
    },
    arc_shadow_mantle: {
      hooks: { onDamageTaken(c, e) { const u = c.unit; if (e.unit !== u || u.state.mantle || u.hp >= u.maxHp * 0.6) return; u.state.mantle = true; u.invulnUntil = c.t + 1; c.text(u, 'Vanish'); } },
    },
    arc_mode_helm: {
      hooks: { onCombatStart(c) { const u = c.unit; if (u.homeR <= 5) { u.armor += 35; u.mr += 35; } else { u.ap += 15; u.P.manaRegen = (u.P.manaRegen || 0) + 10; } } },
    },
    // artifacts
    arc_doom_seal: {
      hooks: {
        onUnitTick(c, e) {
          const u = c.unit;
          if (e.unit !== u) return;
          if (u.state.doom != null && c.t < u.state.doom) return;
          const t = u.target && u.target.alive ? u.target : c.target(u, 'closest');
          if (!t) return;
          u.state.doom = c.t + 13;
          c.event({ type: 'beam', from: u.id, to: t.id });
          c.damage(u, t, t.maxHp * 0.25, 'magic');
        },
      },
    },
    arc_gatling_arm: { hooks: { onCombatStart(c) { c.unit.range = Math.min(8, c.unit.range * 2); } } },
    arc_bounty_chip: {
      hooks: {
        onDamageDealt(c, e) {
          const u = c.unit, t = e.target;
          if (e.unit !== u || u.state.bounty || !t.alive || t.summoned || t.hp >= t.maxHp * 0.12) return;
          u.state.bounty = true;
          c.damage(u, t, (t.hp + t.shield) * 3 + 10, 'true');
          u.state.bounty = false;
          if (!t.alive && c.rng() < 0.33) u.state.bountyGold = Math.min(3, (u.state.bountyGold || 0) + 1);
        },
      },
    },
    arc_lone_plate: {
      hooks: {
        onCombatStart(c) {
          const u = c.unit;
          if (c.adjacent(u).some((a) => a.team === u.team)) return;
          u.maxHp += 600; u.ad += u.baseAd * 0.2; u.ap += 20;
        },
      },
    },
    arc_spell_blade: {
      hooks: {
        onCast(c, e) { if (e.unit === c.unit) c.unit.state.spellblade = true; },
        onAttack(c, e) {
          const u = c.unit;
          if (e.unit !== u || !u.state.spellblade || !e.target.alive) return;
          u.state.spellblade = false;
          c.damage(u, e.target, 60 * Math.max(2, (c.info && c.info.stage) || 3), 'magic');
        },
      },
    },
    arc_overkill_cannon: {
      hooks: {
        onDamageDealt(c, e) {
          const u = c.unit;
          if (e.unit !== u || !e.killed || u.state.overkill) return;
          u.state.overkill = true;
          const near = c.units.filter((x) => x.alive && x.team !== u.team).sort((a, b) => c.dist(e.target, a) - c.dist(e.target, b) || a.id - b.id).slice(0, 3);
          for (const x of near) c.damage(u, x, 100 + e.amount * 0.2, 'magic');
          u.state.overkill = false;
        },
      },
    },
    arc_long_barrel: {
      hooks: {
        onCombatStart(c) { c.unit.range = Math.min(8, c.unit.range + 1); },
        onKill(c, e) { if (e.unit === c.unit) c.unit.range = Math.min(8, c.unit.range + 1); },
      },
    },
    arc_scope_visor: { hooks: { onCombatStart(c) { const u = c.unit; u.range = Math.min(8, u.range + 2); u.perHexAmp += 0.08; } } },
    arc_ascension_talisman: {
      hooks: {
        onUnitTick(c, e) {
          const u = c.unit;
          if (e.unit !== u || u.state.ascT || c.t < 20) return;
          u.state.ascT = true;
          const add = u.maxHp * 0.8;
          u.maxHp += add; c.heal(u, add); u.amp += 0.8;
          c.text(u, 'Ascended');
        },
      },
    },
    arc_time_stopper: {
      hooks: {
        onDamageTaken(c, e) {
          const u = c.unit;
          if (e.unit !== u || u.state.zhonya || u.hp >= u.maxHp * 0.4) return;
          u.state.zhonya = true;
          u.invulnUntil = c.t + 2.5;
          u.lock = Math.max(u.lock, 2.5);
          c.text(u, 'Time Stop');
        },
      },
    },
  };

  // contraband: stats and effects only for Syndicate holders
  const CB_STATS = {
    arc_k_tank: { armor: 15, mr: 15, hp: 200 }, arc_k_tank2: { armor: 25, mr: 25, hp: 350 },
    arc_k_blade: { ad: 0.25, as: 0.1 }, arc_k_blade2: { ad: 0.4, as: 0.15 },
    arc_k_ripper: { ad: 0.15, crit: 0.2 }, arc_k_ripper2: { ad: 0.25, crit: 0.35 },
    arc_k_hex: { armor: 25, mr: 25, hp: 100 }, arc_k_hex2: { armor: 40, mr: 40, hp: 175 },
    arc_k_bloom: { ap: 20, hp: 150 }, arc_k_bloom2: { ap: 35, hp: 250 },
    arc_k_toxins: { ap: 15, mana: 10 }, arc_k_toxins2: { ap: 25, mana: 15 },
    arc_k_virus: { ap: 25, as: 0.15 }, arc_k_virus2: { ap: 40, as: 0.25 },
    arc_k_saber: { ad: 0.15, ap: 15, omnivamp: 0.2 }, arc_k_saber2: { ad: 0.25, ap: 25, omnivamp: 0.35 },
  };
  const CB_FX = {
    arc_k_tank: { tankAt: 0.25, tankEvery: 0.1 }, arc_k_tank2: { tankAt: 0.45, tankEvery: 0.12 },
    arc_k_blade: { exec: 0.1, rush: 0.1 }, arc_k_blade2: { exec: 0.15, rush: 0.15 },
    arc_k_ripper: { rip: [0.25, 0.1, 0.1] }, arc_k_ripper2: { rip: [0.4, 0.2, 0.15] },
    arc_k_hex: { hex: [3, 0.5] }, arc_k_hex2: { hex: [5, 0.75] },
    arc_k_bloom: { bloom: 250 }, arc_k_bloom2: { bloom: 420 },
    arc_k_toxins: { toxins: 100 }, arc_k_toxins2: { toxins: 175 },
    arc_k_virus: { virus: 0.15 }, arc_k_virus2: { virus: 0.25 },
    arc_k_saber: { saber: 150 }, arc_k_saber2: { saber: 250 },
  };
  const cbHooks = (id) => ({
    hooks: {
      onCombatStart(c) {
        const u = c.unit;
        if (!has(u, 'SYN')) return;
        const st = CB_STATS[id];
        for (let k = 0; k < c.count; k++) {
          if (st.ad) u.ad += u.baseAd * st.ad;
          if (st.as) u.as *= 1 + st.as;
          if (st.ap) u.ap += st.ap;
          if (st.mana) u.mana += st.mana;
          if (st.armor) u.armor += st.armor;
          if (st.mr) u.mr += st.mr;
          if (st.hp) u.maxHp += st.hp;
          if (st.crit) u.crit += st.crit;
          if (st.omnivamp) u.omnivamp += st.omnivamp;
        }
        u.state.cb = { ...(u.state.cb || {}), ...CB_FX[id] };
        if (CB_FX[id].toxins) u.P.shojin = (u.P.shojin || 0) + 5;
      },
    },
  });
  for (const id of Object.keys(CB_STATS)) items[id] = cbHooks(id);
  // shared contraband effects (run once per unit from the mechanic hooks)
  const CBX = {
    taken(c, u, e) {
      const s = u.state.cb;
      if (s.tankAt && !s.tankOn && u.hp < u.maxHp * 0.5) { s.tankOn = true; c.shield(u, u.maxHp * s.tankAt); c.text(u, 'Hazard Tank'); }
      if (s.rip && !s.ripped && u.hp < u.maxHp * 0.4) {
        s.ripped = true; u.invulnUntil = c.t + 0.5; c.heal(u, u.maxHp * s.rip[0]); u.ad += u.baseAd * s.rip[1]; u.omnivamp += s.rip[2]; c.text(u, 'Feral');
      }
      if (s.hex) {
        s.hexN = s.hexN || 0;
        if (s.hexN < 15) { s.hexN++; u.armor += s.hex[0]; u.mr += s.hex[0]; }
        else if (!(s.hexCd > c.t) && e.source && e.source.alive) {
          s.hexCd = c.t + 2;
          for (const x of c.enemiesNear(u.team, u, 1)) c.damage(u, x, (u.armor + u.mr) * s.hex[1], 'magic');
        }
      }
    },
    tick(c, u) {
      const s = u.state.cb;
      if (s.tankOn && c.every(u, 'cbTank', 8)) c.shield(u, u.maxHp * s.tankEvery);
      if (s.bloom && c.every(u, 'cbBloom', 8)) {
        const allies = c.units.filter((a) => a.alive && a.team === u.team && a !== u).sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id).slice(0, 2);
        for (const a of [u, ...allies]) c.shield(a, s.bloom);
      }
    },
    dealt(c, u, e) {
      const s = u.state.cb, t = e.target;
      if (s.busy || e.isBurn) return;
      s.busy = true;
      if (s.exec && t.alive && !t.summoned && t.hp < t.maxHp * s.exec) {
        c.damage(u, t, (t.hp + t.shield) * 3 + 10, 'true');
        if (!t.alive) { c.buff(u, 'amp', s.rush, 5); c.buff(u, 'dr', s.rush, 5); }
      }
      if (s.virus && e.kind === 'magic' && t.alive) c.damage(u, t, e.amount * s.virus, 'true');
      if (s.saber) {
        s.hits = (s.hits || 0) + 1;
        if (s.hits >= 6) {
          s.hits = 0;
          const low = c.units.filter((x) => x.alive && x.team !== u.team).sort((a, b) => a.hp - b.hp || a.id - b.id).slice(0, 2);
          for (const x of low) { c.event({ type: 'beam', from: u.id, to: x.id }); c.damage(u, x, s.saber, 'phys'); }
        }
      }
      s.busy = false;
    },
    cast(c, u, e) {
      const s = u.state.cb;
      if (!s.toxins) return;
      const t = e.target && e.target.alive ? e.target : c.target(u, 'closest');
      if (!t) return;
      s.busy = true;
      for (let k = 0; k < 3 && t.alive; k++) { c.event({ type: 'shot', from: u.id, to: t.id }); c.damage(u, t, s.toxins, 'magic'); }
      s.busy = false;
    },
  };

  // ---------- augments ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const giveLine = (c, id) => {
    const g = c.game, p = c.player;
    if (!(g.pool[id] > 0)) return;
    const free = p.freeBenchSlot();
    if (free < 0) return;
    g.takeFromPool(id);
    p.bench[free] = g.newUnit(id, 0);
  };
  const ARTS = () => Object.values(DT.ITEMS).filter((it) => it.cat === 'artifact').map((it) => it.id);
  const augments = {
    arc_a_coins: { hooks: { onPick(c) { c.player.gold += 8; } } },
    arc_a_cache: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    arc_a_rolls: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    arc_a_heart: { hooks: { onPick(c) { c.player.hp += 5; } } },
    arc_a_training: { hooks: team((u) => { u.ad += u.baseAd * 0.12; u.ap += 12; }) },
    arc_a_wind: {
      hooks: {
        onTick: { order: -1, fn(c) { if (c.state.done || c.t < 10) return; c.state.done = true; for (const u of c.survivors(c.team)) c.heal(u, (u.maxHp - u.hp) * 0.35); } },
      },
    },
    arc_a_feather: { hooks: team((u) => { if (u.line.tier <= 2) u.as *= 1.25; }) },
    arc_a_front: { hooks: team((u) => { if (u.homeR === 4) { u.armor += 30; u.mr += 30; u.maxHp += 200; } }) },
    arc_a_back: { hooks: team((u) => { if (u.homeR >= 6) u.as *= 1.25; }) },
    arc_a_shimmer: {
      hooks: { onPick(c) { const m = c.player.mech; m.arcSynAug = true; addShimmer(m, 60); giveLine(c, 'arc_mushroomon'); c.combine(); } },
      ai: { bonus: (c) => (c.player.allUnits().some(isSyn) ? 3 : -1) },
    },
    arc_a_bank: { hooks: { onPick(c) { c.player.gold += 18; } } },
    arc_a_trove: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomComponent()); } } },
    arc_a_tag: { hooks: { onPick(c) { c.give('arc_spat'); } } },
    arc_a_device: { hooks: { onPick(c) { c.give('arc_pan'); } } },
    arc_a_income: { hooks: { onIncome(c, e) { e.augment += 2; } } },
    arc_a_evolve: { hooks: { onPick(c) { c.giveUnit([2, 3], 1); } } },
    arc_a_blessing: { hooks: team((u) => { u.omnivamp += 0.12; }) },
    arc_a_lotus: { hooks: team((u) => { u.abilCrit = true; u.crit += 0.15; }) },
    arc_a_overcharge: { hooks: team((u) => { u.mana += 25; }) },
    arc_a_find: {
      hooks: {
        onPick(c) { c.player.freeRolls += 6; },
        onRoundEnd(c, e) {
          const cb = e.match && e.match.combat;
          if (!cb) return;
          const m = c.player.mech;
          for (const u of cb.units) if (u.team === e.team && u.mods && u.mods.arcAnom && u.state.anomK) m.arcFindK = (m.arcFindK || 0) + u.state.anomK;
          while ((m.arcFindK || 0) >= 3) { m.arcFindK -= 3; e.gold += 1; }
        },
      },
    },
    arc_a_forge: { hooks: { onPick(c) { c.give(c.pick(ARTS())); } } },
    arc_a_unity: { hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of c.mine) { u.ad += u.baseAd * 0.03 * n; u.ap += 3 * n; } } } },
    arc_a_jackpot: { hooks: { onPick(c) { c.player.gold += 35; } } },
    arc_a_armory: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    arc_a_crown: { hooks: { onPick(c) { c.give('arc_tamer_crown'); } } },
    arc_a_mega: { hooks: { onPick(c) { c.giveUnit([4], 1); } } },
    arc_a_giant: { hooks: team((u) => { u.maxHp = Math.round(u.maxHp * 1.25); }) },
    arc_a_mutant: {
      hooks: { onPick(c) { const m = c.player.mech; c.player.gold += 6; if (m.arcAnomaly) m.arcPendingAnom = true; else m.arcExtra = (m.arcExtra || 0) + 1; } },
    },
    arc_a_workshop: { hooks: { onPick(c) { const a = ARTS(); c.give(c.pick(a)); c.give(c.pick(a)); } } },
    arc_a_market: {
      hooks: { onPick(c) { const m = c.player.mech; m.arcMarketPlus = true; addShimmer(m, 120); } },
      ai: { bonus: (c) => (c.player.allUnits().filter(isSyn).length >= 2 ? 3 : -2) },
    },
  };

  // ---------- set ability types ----------
  const abilities = {
    // Mode Change (TFT Form Swapper): guards in the front two rows, blasts from the back two
    arc_swap: {
      base: 185,
      cast(c, u, tg, v) {
        if (u.homeR <= 5) {
          c.shield(u, v * 0.7);
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 1, team: u.team });
          for (const e of c.enemiesNear(u.team, u, 1)) { c.stun(e, 0.75); c.abilityHit(u, e, v * 0.45); }
        } else {
          c.event({ type: 'beam', from: u.id, to: tg.id });
          c.abilityHit(u, tg, v * 1.4);
        }
      },
      text: (line, stage, v) => `Front two rows: gains a ${Math.round(v * 0.7)} shield and hits adjacent enemies for ${Math.round(v * 0.45)} magic damage, stunning them for 0.75s. Back two rows: blasts the target for ${Math.round(v * 1.4)} magic damage.`,
    },
    // Jinx: cycles a stunning line shot, three blasts, and a board-wide rocket
    arc_rotate: {
      base: 210,
      cast(c, u, tg, v) {
        const k = (u.state.rot || 0) % 3;
        u.state.rot = k + 1;
        if (k === 0) {
          for (const e of c.lineTargets(u, tg)) { c.event({ type: 'beam', from: u.id, to: e.id }); c.abilityHit(u, e, v); if (e.alive) c.stun(e, 1.25); }
        } else if (k === 1) {
          for (let n = 0; n < 3; n++) {
            const t = c.target(u, 'random');
            if (!t) break;
            c.event({ type: 'burst', c: t.c, r: t.r, radius: 1, team: u.team });
            for (const e of c.enemiesNear(u.team, t, 1)) c.abilityHit(u, e, v * 0.6);
          }
        } else {
          c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 3, team: u.team });
          for (const e of c.enemiesOf(u)) c.abilityHit(u, e, v * 1.2 * Math.max(0.3, 1 - 0.12 * c.dist(tg, e)));
        }
      },
      text: (line, stage, v) => `Cycles three attacks: a shot through a line for ${v} magic damage that stuns for 1.25s; three blasts for ${Math.round(v * 0.6)}; a rocket that hits every enemy for up to ${Math.round(v * 1.2)}, less farther from the target.`,
    },
    // Sevika: one of three moves at random, sometimes all three
    arc_gamble: {
      base: 170,
      cast(c, u, tg, v) {
        const r = c.rng();
        const flame = () => { c.event({ type: 'burst', c: tg.c, r: tg.r, radius: 1, team: u.team }); for (const e of c.enemiesNear(u.team, tg, 1)) { c.abilityHit(u, e, v * 0.8); c.burn(u, e); } };
        const punch = () => { if (!tg.alive) return; c.abilityHit(u, tg, v * 1.5); if (tg.alive) c.stun(tg, 1); };
        const chomp = () => { if (!tg.alive) return; const d = c.abilityHit(u, tg, v * 1.1); c.heal(u, d * 0.5); };
        if (r < 0.12) { flame(); punch(); chomp(); c.text(u, 'Jackpot!'); }
        else if (r < 0.41) flame();
        else if (r < 0.7) punch();
        else chomp();
      },
      text: (line, stage, v) => `Rolls one of three moves: a flamethrower that hits the target and enemies next to it for ${Math.round(v * 0.8)} and burns; a punch for ${Math.round(v * 1.5)} that stuns for 1s; or a bite for ${Math.round(v * 1.1)} that heals half the damage. Sometimes all three.`,
    },
    // Heimerdinger: more missiles every cast
    arc_progress: {
      base: 85,
      cast(c, u, tg, v) {
        const n = 4 + u.stage + (u.state.prog || 0);
        u.state.prog = (u.state.prog || 0) + 1;
        for (let k = 0; k < n; k++) {
          const t = c.target(u, 'random');
          if (!t) break;
          c.event({ type: 'shot', from: u.id, to: t.id, big: true });
          c.abilityHit(u, t, v);
        }
      },
      text: (line, stage, v) => `Fires ${4 + stage} missiles at random enemies for ${v} magic damage each, and one more missile every cast.`,
    },
  };

  // ---------- the set mechanic ----------
  function anomalyOfferNow(c) {
    const p = c.player;
    const opts = anomalyOptions(c, []);
    if (opts.length) p.offer = { kind: 'arcAnomaly', options: opts, rerolls: 6 };
  }
  const art = (key) => (G.DT.ItemArt && G.DT.ItemArt.img ? G.DT.ItemArt.img(key, 64, 'augart') : '');

  const mechanic = {
    id: 'anomaly',
    name: 'Anomalies',
    desc: 'At 4-3, pick one of 3 Anomalies for the Digimon on your board\'s center hex (column 4, second row); if it is empty, your strongest Digimon gets it. Rerolls cost 1 gold. The mutation (X-Antibody, DNA Digivolution or Digital Hazard) lasts the whole game and stays through digivolving. Each game also opens a random portal, and the Kowloon Syndicate runs a black market.',
    hooks: {
      // ---- game ----
      onGameStart(c) {
        const g = c.game;
        g.mech.arcSponsor = g.shuffle(DT.CORE_ITEMS.slice()).slice(0, 3);
        const portal = g.pick(PORTALS).id;
        g.mech.arcPortal = portal;
        for (const p of g.players) {
          if (portal === 'gold') p.gold += 6;
          else if (portal === 'anvils') { g.giveItem(p, g.randomComponent()); g.giveItem(p, g.randomComponent()); }
          else if (portal === 'tag') g.giveItem(p, 'arc_spat');
          else if (portal === 'device') g.giveItem(p, 'arc_pan');
          else if (portal === 'copier') g.giveItem(p, 'arc_copier');
          else if (portal === 'upgraded') g.giveUnit(p, [1], 1);
        }
        SHOWN.portal = portal;
        SHOWN.sponsor = g.mech.arcSponsor.slice();
        const aca = T().ACA;
        aca.notes = `Sponsored this game: ${g.mech.arcSponsor.map((id) => DT.ITEMS[id].name).join(', ')}.`;
      },
      onRoundStart(c) {
        const g = c.game, p = c.player, m = p.mech;
        const portal = g.mech.arcPortal;
        if (portal === 'rolls' && c.round === 1 && c.stage >= 2) p.freeRolls += 2;
        if (portal === 'golden' && c.stage === 1 && c.round === 2) g.augTier = { stage: 2, tier: 2 };
        if (portal === 'prismatic' && c.stage === 4 && c.round === 1) g.augTier = { stage: 4, tier: 3 };
        if (p.offer) return;
        if (m.arcPendingAnom) { m.arcPendingAnom = false; anomalyOfferNow(c); if (p.offer) return; }
        if (m.arcMarkets > 0) {
          m.arcMarkets--;
          m.arcMarketsOpened = (m.arcMarketsOpened || 0) + 1;
          p.offer = { kind: 'arcMarket', options: marketOffer(c), rerolls: 0 };
          return;
        }
        if (ownsLine(p, 'arc_chaosdromon')) {
          m.arcJunkT = (m.arcJunkT || 0) + 1;
          if (m.arcJunkT >= (g.pace === 'normal' ? 3 : 2)) {
            m.arcJunkT = 0;
            const owned = m.arcJunk || [];
            const left = JUNK_PARTS.filter((k) => !owned.includes(k));
            const opts = c.shuffle(left.slice()).slice(0, 2);
            opts.push('spare');
            p.offer = { kind: 'arcJunk', options: opts, rerolls: 0 };
          }
        }
      },
      onRoundEnd(c, e) {
        const p = c.player;
        const cb = e.match && e.match.combat;
        for (const u of p.boardUnits()) {
          for (const id of u.items) {
            if (id === 'arc_tamer_crown' && e.won && c.rng() < 0.1) e.gold += 1;
            else if (id === 'arc_tamer_cape' && c.rng() < 0.1) e.gold += 1;
            else if (id === 'arc_tamer_shield' && cb) {
              const cu = cb.units.find((x) => x.uid === u.uid && x.team === e.team);
              if (cu && !cu.alive && c.rng() < 0.1) e.gold += 1;
            }
          }
        }
        if (cb) {
          let bounty = 0;
          for (const u of cb.units) if (u.team === e.team && u.state.bountyGold) bounty += u.state.bountyGold;
          e.gold += Math.min(3, bounty);
        }
      },
      onCombine(c, e) {
        if (e.unit.mods && e.unit.mods.arcAnom) return;
        const src = e.from.find((u) => u.mods && u.mods.arcAnom);
        if (src) e.unit.mods = { ...(e.unit.mods || {}), ...src.mods };
      },
      onCountTraits(counts) { if (counts.EMI === 2 || counts.EMI === 3) counts.EMI = 0; },
      onAiTurn(c) {
        const p = c.player, g = c.game;
        // contraband goes to Syndicate Digimon
        for (let i = p.items.length - 1; i >= 0; i--) {
          const it = DT.ITEMS[p.items[i]];
          if (!it || it.cat !== 'contraband') continue;
          const u = p.allUnits().filter((x) => isSyn(x) && x.items.length < 3).sort((a, b) => DT.unitValue(b) - DT.unitValue(a) || a.uid - b.uid)[0];
          if (!u) continue;
          u.items.push(p.items[i]);
          p.items.splice(i, 1);
        }
        // Data Copiers: copy the best unit that is closest to digivolving
        for (let i = p.items.length - 1; i >= 0; i--) {
          const id = p.items[i];
          if (id !== 'arc_copier' && id !== 'arc_copier_small') continue;
          const max = id === 'arc_copier' ? 5 : 3;
          const owned = p.allUnits();
          const cnt = (l) => owned.filter((x) => x.line === l && x.stage === 0).length;
          const u = p.boardUnits().filter((x) => x.stage < 2 && DT.LINE[x.line].tier <= max && g.pool[x.line] > 0)
            .sort((a, b) => cnt(b.line) - cnt(a.line) || DT.unitValue(b) - DT.unitValue(a) || a.uid - b.uid)[0];
          if (u && p.freeBenchSlot() >= 0) copyUnit(c, u, i, max);
        }
      },
      // ---- combat ----
      onCombatStart(c) {
        for (const u of c.units) {
          if (!u.alive) continue;
          if (u.line.swap) { if (u.homeR <= 5) u.maxHp = Math.round(u.maxHp * 1.1); else u.range = Math.max(u.range, 4); }
          const a = anomOf(u);
          if (a) { c.text(u, a.short); if (a.start) a.start(c, u); }
        }
      },
      onCombatReady(c) { for (const u of c.units.slice()) { const a = anomOf(u); if (a && a.ready && u.alive) a.ready(c, u); } },
      onAttack: dispatch('attack'),
      onCast(c, e) { const a = anomOf(e.unit); if (a && a.cast) a.cast(c, e.unit, e); if (e.unit.state.cb) CBX.cast(c, e.unit, e); },
      onKill(c, e) { const a = anomOf(e.unit); if (a) { e.unit.state.anomK = (e.unit.state.anomK || 0) + 1; if (a.kill) a.kill(c, e.unit, e); } },
      onDamageTaken(c, e) { const a = anomOf(e.unit); if (a && a.taken) a.taken(c, e.unit, e); if (e.unit.state.cb) CBX.taken(c, e.unit, e); },
      onDamageDealt(c, e) { const a = anomOf(e.unit); if (a && a.dealt) a.dealt(c, e.unit, e); if (e.unit.state.cb && e.unit.alive) CBX.dealt(c, e.unit, e); },
      onUnitTick(c, e) { const a = anomOf(e.unit); if (a && a.tick) a.tick(c, e.unit, e); if (e.unit.state.cb && e.unit.alive) CBX.tick(c, e.unit, e); },
      onLethal: dispatch('lethal'),
      onDeath: dispatch('death'),
      onAnyDeath(c, e) {
        const d = e.unit;
        for (const u of c.units) {
          if (!u.alive || u.team !== d.team || u === d) continue;
          const a = anomOf(u);
          if (a && a.ally) a.ally(c, u, d);
        }
      },
    },
    offerKinds: {
      arcAnomaly: {
        name: 'Anomaly',
        sub: 'Pick a mutation for the Digimon on your center hex (column 4, second row from the front). If that hex is empty, your strongest Digimon gets it. It lasts the whole game. Each reroll costs 1 gold.',
        options: (c, exclude) => anomalyOptions(c, exclude),
        choose: (c, id) => applyAnomaly(c, id),
        aiScore: (c, id) => anomalyScore(c, id),
        card: (id) => { const a = ANOM[id] || { name: id, desc: '' }; return { name: a.name, desc: `${a.theme}. ${a.desc}`, img: art(`arc_anom:${id}`) }; },
      },
      arcMarket: {
        name: 'Black Market',
        sub: 'Contraband only works on Kowloon Syndicate Digimon. Or pass, and your Syndicate Digimon gain permanent max HP.',
        options: (c, exclude) => marketOffer(c).filter((x) => !exclude.includes(x)),
        choose(c, id) {
          const m = c.player.mech;
          if (String(id).startsWith('arc_pass:')) { m.arcSynHp = (m.arcSynHp || 0) + (Number(String(id).split(':')[1]) || 0); m.arcSkips = (m.arcSkips || 0) + 1; return; }
          if (DT.ITEMS[id]) c.give(id);
        },
        aiScore(c, id) {
          const syn = c.player.allUnits().filter(isSyn);
          if (String(id).startsWith('arc_pass:')) return syn.length ? 2 : 8;
          const it = DT.ITEMS[id];
          let s = 4;
          for (const u of syn) {
            const L = DT.LINE[u.line];
            const kind = L.ai.carry ? (L.ai.dmg === 'ap' ? 'ap' : 'ad') : 'tank';
            if ((it.tags || []).includes(kind)) s += 1.5;
          }
          return s;
        },
        card: (id) => {
          if (String(id).startsWith('arc_pass:')) return { name: 'Pass', desc: `Your Syndicate Digimon gain ${String(id).split(':')[1]} max HP for the rest of the game.`, img: art('arc_pass') };
          const it = DT.ITEMS[id] || { name: id, desc: '' };
          return { name: it.name, desc: it.desc, img: art(id) };
        },
      },
      arcJunk: {
        name: 'Junk King Workshop',
        sub: 'Pick a permanent upgrade for your strongest Chaosdromon.',
        options: (c, exclude) => ['spare'].filter((x) => !exclude.includes(x)),
        choose(c, id) { if (JUNK[id]) (c.player.mech.arcJunk = c.player.mech.arcJunk || []).push(id); },
        aiScore: (c, id) => (id === 'spare' ? 1 : 3),
        card: (id) => { const j = JUNK[id] || { name: id, desc: '' }; return { name: j.name, desc: j.desc, img: art(`arc_junk:${id}`) }; },
      },
    },
  };

  // codex Rules tab (called from arcane.js codexRules); display only
  DT.ArcaneInfo = { ANOM, PORTALS, JUNK, SHOWN };

  // tooltip line for a Digimon with an Anomaly
  function unitNote(u) {
    const a = anomOf(u);
    return a ? `Anomaly: ${a.name}. ${a.desc}` : null;
  }

  DT.Sets.extend('arcane', { traits, items, augments, abilities, mechanic, unitNote });
})(typeof window !== 'undefined' ? window : globalThis);
