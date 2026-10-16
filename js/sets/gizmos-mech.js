// Gizmos & Gadgets behavior: trait, item, augment (Digivice upgrade) and consumable hooks, the set's ability types,
// and the Hextech Gates mechanic (upgrade tiers rolled per lobby, the 'upgrade' offer kind, the Pirates treasure
// chest, Glutton feeding, Colossus stun immunity). Contexts are documented in docs/sets/FORMAT.md.
// Deterministic: randomness only from c.rng() / c.pick() (seeded).
(function (G) {
  const DT = G.DT;
  const ROWS = DT.ROWS, COLS = DT.COLS;

  // ---------- helpers ----------
  const has = (u, t) => u.traits.includes(t);
  const augsOf = (c) => (c.side(c.team) && c.side(c.team).augs) || [];
  const hasAug = (c, id) => augsOf(c).includes(id);
  const alive = (list) => list.filter((u) => u.alive);
  // best unit by score (ties keep the first, so the result never depends on sort stability)
  const best = (list, score) => { let b = null, bs = -Infinity; for (const u of list) { const s = score(u); if (s > bs) { bs = s; b = u; } } return b; };

  // Apply a completed item's stats and passives to a combat unit for this combat (Junkyard, Binary Airdrop).
  // minus: a component whose stats the unit already has (its upgrade only adds the difference).
  const DERIVED = { amp: 'amp', vamp: 'omnivamp', critDmg: 'critDmg', dr: 'dr', regen: 'regen' };
  function applyItem(u, id, minus) {
    const it = DT.ITEMS[id];
    if (!it) return;
    const st = { ...(it.stats || {}) };
    if (minus && DT.ITEMS[minus]) for (const k in DT.ITEMS[minus].stats) st[k] = (st[k] || 0) - DT.ITEMS[minus].stats[k];
    if (st.ad) u.ad += u.baseAd * st.ad;
    if (st.as) u.as *= 1 + st.as;
    if (st.ap) u.ap += st.ap;
    if (st.mana) u.mana += st.mana;
    if (st.armor) u.armor += st.armor;
    if (st.mr) u.mr += st.mr;
    if (st.hp) u.maxHp += st.hp;
    if (st.crit) u.crit += st.crit;
    if (st.dodge) u.dodge += st.dodge;
    if (st.omnivamp) u.omnivamp += st.omnivamp;
    for (const [k, v] of Object.entries(it.p || {})) {
      if (k === 'lifeline') { u.lifelines.push({ ...v, used: false }); continue; }
      if (k === 'thief' || k === 'teamSize' || k === 'emblem') continue;
      if (typeof v === 'number') {
        u.P[k] = (u.P[k] || 0) + v;
        if (DERIVED[k]) u[DERIVED[k]] += v;
      } else {
        u.P[k] = v;
        if (k === 'abilCrit') u.abilCrit = true;
        if (k === 'bluebuff') u.maxMana = Math.max(20, u.maxMana - 10);
      }
    }
  }
  const completedFrom = (comp) => Object.entries(DT.RECIPE).filter(([k, v]) => k.split('+').includes(comp) && DT.ITEMS[v] && DT.ITEMS[v].cat === 'completed').map(([, v]) => v);

  // ---------- Mutation ----------
  const MUTATIONS = ['cyber', 'voracious', 'voidborne', 'adrenal', 'synaptic', 'leech', 'meta'];
  const MUT_NAME = { cyber: 'Cybernetic', voracious: 'Voracious', voidborne: 'Voidborne', adrenal: 'Adrenal', synaptic: 'Synaptic', leech: 'Leeching', meta: 'Metamorphosis' };
  // outside a game (duel tests) each combat rolls its own mutation
  const mutOf = (c) => (c.info && c.info.mech && c.info.mech.mutation) || MUTATIONS[Math.floor(c.rng() * MUTATIONS.length)];
  const M2 = (c, a, b) => (c.lv >= 2 ? b : a);

  // ---------- traits ----------
  const each = (fn) => ({ onCombatStart(c) { for (const u of c.members) fn(u, c); } });
  const traits = {
    MASQ: {
      hooks: {
        onCombatStart(c) {
          const vip = best(c.members, (u) => u.items.length * 100 + u.line.tier * 10 + u.stage);
          for (const u of c.members) {
            const k = u === vip && u.items.length ? 1.5 : 1;
            u.maxHp += Math.round(c.v * k);
            u.ap += Math.round(c.val('ap') * k);
          }
          if (vip && vip.items.length) c.text(vip, 'VIP');
        },
      },
    },
    DB: {
      hooks: {
        onCombatReady(c) {
          const foes = c.units.filter((e) => e.alive && e.team !== c.team);
          const big = best(foes, (e) => e.maxHp);
          const stun = (e, dur) => {
            if (!e) return;
            c.stun(e, dur);
            if (e.stun > 0) { e.state.dbBreak = e.hp - e.maxHp * 0.4; c.text(e, 'Arrested'); }
          };
          stun(big, c.v);
          if (c.lv >= 2) {
            stun(best(foes.filter((e) => e !== big), (e) => e.ad), c.val('second'));
            for (const u of c.members) u.as *= 1 + c.val('as');
          }
        },
      },
    },
    CLOCK: {
      hooks: { onCombatStart(c) { const n = augsOf(c).length; for (const u of c.mine) u.as *= 1 + c.v + c.val('per') * n; } },
    },
    DM: {
      hooks: {
        onCombatStart(c) {
          const list = c.lv >= 3 ? c.mine : c.lv >= 2 ? c.members : c.members.slice().sort((a, b) => a.maxHp - b.maxHp || a.id - b.id).slice(0, 2);
          for (const u of list) { u.armor += c.v; u.mr += c.v; u.omnivamp += c.val('vamp'); }
        },
      },
    },
    JUNK: {
      hooks: {
        onCombatStart(c) {
          let n = 0;
          for (const u of c.mine) {
            const k = has(u, 'JUNK') ? 2 : 1;
            for (const id of u.items) {
              const it = DT.ITEMS[id];
              if (!it) continue;
              if (it.component && !it.special) n += k;
              else if (it.cat === 'completed' || it.cat === 'radiant') n += 2 * k;
            }
          }
          if (n) for (const u of c.mine) c.shield(u, c.v * n);
          for (const u of c.members) { u.armor += c.val('armor'); u.mr += c.val('armor'); }
          // components on Junkyard Digimon work as a completed item made from them
          let budget = c.val('conv');
          for (const u of c.members) {
            for (const id of u.items) {
              if (budget <= 0) break;
              const it = DT.ITEMS[id];
              if (!it || !it.component || it.special) continue;
              const opts = completedFrom(id);
              if (!opts.length) continue;
              applyItem(u, opts[Math.floor(c.rng() * opts.length)], id);
              budget--;
            }
          }
        },
      },
    },
    PIR: { hooks: each((u, c) => { u.amp += c.v; }), ai: { bonus: (a) => (a.stage <= 4 ? 1 : 0) } },
    CD: {
      hooks: {
        onCombatStart(c) { c.state.next = 0; c.state.mult = 1 + 0.15 * augsOf(c).length; },
        onTick(c) {
          if (c.t < c.state.next) return;
          c.state.next += 6;
          const amt = Math.round(c.v * c.state.mult);
          for (const u of alive(c.members)) {
            c.shield(u, amt);
            u.state.cdUntil = c.t + 4;
            c.after(4, () => { u.shield -= Math.min(u.shield, amt); }, u);
          }
        },
        onAttack(c, e) {
          const u = e.unit;
          if (!c.isMember(u) || !(u.state.cdUntil > c.t) || !e.target.alive) return;
          c.damage(u, e.target, c.val('dmg') * c.state.mult, 'magic');
        },
      },
    },
    MUT: {
      hooks: {
        onCombatStart(c) {
          const m = mutOf(c);
          c.state.m = m;
          for (const u of c.members) {
            c.text(u, MUT_NAME[m]);
            if (m === 'cyber') { const k = u.items.length ? 1.5 : 1; u.maxHp += Math.round(M2(c, 200, 380) * k); u.ad += u.baseAd * M2(c, 0.12, 0.22) * k; }
            if (m === 'synaptic') u.maxMana = Math.max(20, u.maxMana - M2(c, 20, 35));
          }
          if (m === 'leech') for (const u of c.mine) u.omnivamp += M2(c, 0.12, 0.22);
        },
        onDeath(c, e) {
          if (c.state.m !== 'voracious') return;
          for (const u of alive(c.members)) { c.buff(u, 'as', M2(c, 0.1, 0.18)); u.ad += u.baseAd * M2(c, 0.04, 0.08); }
        },
        onDamageDealt(c, e) {
          if (c.state.m !== 'voidborne' || c.state.busy) return;
          const u = e.unit, tg = e.target;
          if (!c.isMember(u) || !tg.alive || tg.hp > tg.maxHp * M2(c, 0.1, 0.18)) return;
          c.state.busy = true;
          c.text(tg, 'Voidborne');
          c.damage(u, tg, tg.hp + tg.shield + 10, 'true');
          c.state.busy = false;
        },
        onAttack(c, e) {
          if (c.state.m !== 'adrenal' || !c.isMember(e.unit) || !e.target.alive) return;
          if (c.rng() < M2(c, 0.2, 0.35)) c.damage(e.unit, e.target, e.dmg, 'phys', e.crit);
        },
        onTick(c) {
          if (c.state.m !== 'meta') return;
          if (c.state.next == null) c.state.next = 4;
          if (c.t < c.state.next) return;
          c.state.next += 4;
          for (const u of alive(c.members)) { u.armor += M2(c, 6, 12); u.mr += M2(c, 6, 12); u.ad += u.baseAd * M2(c, 0.05, 0.1); u.ap += M2(c, 6, 12); }
        },
      },
    },
    PET: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) { u.dodge += c.v; if (c.lv >= 2) u.maxMana = Math.max(20, Math.round(u.maxMana * 0.8)); } },
        // game hook: a free Petit Digimon after every player combat
        onRoundEnd(c, e) {
          if (e.pve) return;
          const g = c.game, p = c.player;
          const free = p.freeBenchSlot();
          if (free < 0) return;
          const tiers = c.stage <= 3 ? [1, 2] : c.stage <= 5 ? [1, 2, 3] : c.stage <= 6 ? [1, 2, 3, 4] : [1, 2, 3, 4, 5];
          const lines = DT.SHOP_LINES.filter((l) => l.traits.includes('PET') && tiers.includes(l.tier) && g.pool[l.id] > 0);
          if (!lines.length) return;
          const l = c.pick(lines);
          g.takeFromPool(l.id);
          p.bench[free] = g.newUnit(l.id, 0);
          g.combine(p);
        },
      },
      ai: { bonus: () => 1 },
    },
    XA: {
      hooks: {
        onCombatStart(c) {
          const now = hasAug(c, 'giz_a_injection');
          for (const u of c.members) {
            u.state.xa = 1;
            if (now) xaTrigger(c, u, true);
          }
        },
        onDamageTaken(c, e) {
          const u = e.unit;
          if (u.state.xa === 1 && c.isMember(u) && u.hp < u.maxHp * 0.75) xaTrigger(c, u, false);
        },
      },
    },
    SIS: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) {
            if (u.line.id === 'giz_sistermonb') u.maxMana = Math.max(20, u.maxMana - 20);
            if (u.line.id === 'giz_sistermonn') u.state.noir = true;
            u.amp += c.v;
          }
        },
        onKill(c, e) { if (e.unit.state.noir && e.unit.alive) c.buff(e.unit, 'as', 0.4, 3); },
      },
    },
    IDOL: {
      hooks: {
        onCombatReady(c) {
          const s = best(c.mine.filter((u) => !u.summoned), (u) => u.items.length * 1000 + u.line.tier * 100 + u.stage * 10 - u.id * 0.001);
          if (!s) return;
          c.state.spot = s;
          c.state.next = 1;
          s.amp += c.v;
          if (c.lv >= 3) s.omnivamp += c.val('vamp');
          c.text(s, 'Spotlight');
        },
        onTick(c) {
          const s = c.state.spot;
          if (!s || c.lv < 2 || !s.alive || c.t < c.state.next) return;
          c.state.next += 1;
          s.mana = Math.min(s.maxMana, s.mana + c.val('mana'));
        },
      },
    },
    GLUT: {
      hooks: each((u) => { const m = u.mods; if (m && m.glutHp) { u.maxHp += m.glutHp; u.ap += m.glutAp || 0; } }),
    },
    TRANS: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) {
            if (u.homeR <= 5) { u.state.mode = 'melee'; u.range = 1; u.armor += 40; u.mr += 40; }
            else { u.state.mode = 'ranged'; u.range = 4; }
          }
        },
        onCast(c, e) {
          const u = e.unit;
          if (!c.isMember(u)) return;
          if (u.state.mode === 'melee') {
            for (const x of c.enemiesNear(u.team, u, 2)) { c.buff(x, 'armor', -Math.round(x.armor * 0.3), 4); c.buff(x, 'mr', -Math.round(x.mr * 0.3), 4); }
          } else {
            for (const a of c.alliesNear(u.team, u, 6)) if (a.r === u.r) c.buff(a, 'as', 0.4, 4);
          }
        },
      },
    },
    MIND: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.members) {
            const dir = u.team === 0 ? -1 : 1;
            const others = c.mine.filter((a) => a !== u);
            let pick = others.filter((a) => a.r === u.r + dir && c.dist(a, u) <= 1);
            if (pick.length < 2) pick = pick.concat(others.filter((a) => !pick.includes(a)).sort((a, b) => c.dist(a, u) - c.dist(b, u) || a.id - b.id)).slice(0, 2);
            for (const a of pick.slice(0, 2)) a.mana = Math.min(a.maxMana - 1, a.mana + c.v);
          }
        },
      },
    },
    ARC: { hooks: { onCombatStart(c) { for (const u of c.mine) u.ap += c.v + (has(u, 'ARC') ? c.val('own') : 0); } } },
    ASN: { hooks: each((u, c) => { u.crit += c.v; u.critDmg += c.val('critDmg'); u.abilCrit = true; }) },
    BG: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.armor += c.v; },
        onTick(c) {
          if (c.state.done || c.t < 1.5) return;
          c.state.done = true;
          for (const u of alive(c.members)) {
            c.shield(u, c.val('shield'));
            for (const e of c.enemiesNear(u.team, u, 1)) e.target = u;
          }
        },
      },
    },
    BRU: { hooks: { onCombatStart(c) { for (const u of c.mine) u.maxHp += c.v * (has(u, 'BRU') ? 2 : 1); } } },
    CHAL: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.as *= 1 + c.v; },
        onKill(c, e) { if (c.isMember(e.unit) && e.unit.alive) c.buff(e.unit, 'as', c.v, 2.5); },
      },
    },
    COL: { hooks: each((u, c) => { u.dr += c.v; if (c.lv >= 2) u.maxHp = Math.round(u.maxHp * (1 + c.val('hp'))); }) },
    ENCH: {
      hooks: {
        onCombatStart(c) {
          const ts = c.teamState(c.team);
          ts.healAmp = (ts.healAmp || 0) + c.val('heal');
          for (const u of c.mine) { u.mr += c.v; if (has(u, 'ENCH')) u.ap += c.val('ap'); }
        },
      },
    },
    INNO: {
      hooks: {
        onCombatReady(c) {
          const inns = alive(c.members).filter((u) => !u.summoned);
          if (!inns.length) return;
          const stars = inns.reduce((s, u) => s + u.stage + 1, 0);
          let cell = null;
          for (const u of inns) { cell = c.freeCellNear(u, u); if (cell) break; }
          if (!cell) return;
          const m = c.summon(c.team, 'giz_inno', c.v, cell.c, cell.r, { hpMul: 0.45 + 0.04 * stars, adMul: 0.5 + 0.04 * stars, noHooks: true });
          if (m) c.text(m, 'Built');
        },
      },
    },
    SCH: {
      hooks: {
        onCombatStart(c) { c.state.next = 2; },
        onTick(c) {
          if (c.t < c.state.next) return;
          c.state.next += 2;
          for (const u of alive(c.mine)) if (u.maxMana > 0) u.mana = Math.min(u.maxMana, u.mana + c.v);
        },
      },
    },
    SNI: { hooks: each((u, c) => { u.perHexAmp = c.v; }) },
    STR: { hooks: each((u, c) => { u.ad += u.baseAd * c.v; }) },
    TWIN: {
      hooks: {
        onCombatStart(c) { for (const u of c.members) u.ad += u.baseAd * c.val('ad'); },
        onAttack(c, e) { if (c.isMember(e.unit) && e.target.alive && c.rng() < c.v) c.damage(e.unit, e.target, e.dmg, 'phys', e.crit); },
      },
    },
  };

  function xaTrigger(c, u, start) {
    u.state.xa = start ? 1 : 2;
    const T = DT.TRAITS.XA, i = c.traitLevel(u.team, 'XA') - 1;
    if (i < 0) return;
    c.buff(u, 'as', T.lv[i], 8);
    c.buff(u, 'dr', 0.15, 8);
    c.buff(u, 'regen', T.regen[i], 8);
    c.text(u, 'X-Antibody');
  }

  // ---------- items ----------
  const sc = (c) => (c.item && c.item.radiant ? 1.6 : 1);
  const rowAllies = (c, u, r) => c.units.filter((a) => a.alive && a.team === u.team && a.r === u.r && c.dist(a, u) <= r);
  const itemHooks = {
    giz_gun_blade: {
      onDamageDealt(c, e) {
        if (e.kind !== 'magic' || !(e.amount > 0)) return;
        const u = c.unit;
        const low = best(c.units.filter((a) => a.alive && a.team === u.team), (a) => -a.hp / a.maxHp);
        if (low) c.heal(low, e.amount * 0.25 * sc(c), true);
      },
    },
    giz_backup_drive: {
      onLethal(c, e) {
        const u = e.unit;
        if (u.state.backup) return;
        u.state.backup = true;
        u.hp = 1;
        u.invulnUntil = c.t + 2;
        u.stun = Math.max(u.stun, 2);
        c.text(u, 'Backup');
        const amt = 400 * sc(c);
        c.after(2, () => c.heal(u, amt), u);
      },
    },
    giz_war_banner: { onCombatStart(c) { for (const a of rowAllies(c, c.unit, 1)) a.as *= 1 + 0.3 * sc(c); } },
    giz_rail_cannon: { onCombatStart(c) { c.unit.range += 1; } },
    giz_decoy_portal: {
      onCombatReady(c) { const u = c.unit; for (const e of c.enemiesNear(u.team, u, 4)) e.target = u; },
      onDeath(c, e) {
        const u = e.unit;
        const d = c.summon(u.team, 'giz_decoy', Math.min(2, u.stage), u.c, u.r, { hpMul: sc(c), noHooks: true });
        if (d) { for (const x of c.enemiesNear(u.team, d, 4)) x.target = d; c.text(d, 'Decoy'); }
      },
    },
    giz_guard_locket: { onCombatStart(c) { const amt = [300, 350, 400][c.unit.stage] * sc(c); for (const a of rowAllies(c, c.unit, 2)) c.shield(a, amt); } },
    giz_frost_heart: {
      onUnitTick(c) {
        const u = c.unit;
        if (!c.every(u, 'frostheart', 1)) return;
        for (const e of c.enemiesNear(u.team, u, 2)) {
          if (e.state.frostUntil > c.t) continue;
          e.state.frostUntil = c.t + 1.05;
          c.buff(e, 'as', -0.35 * sc(c), 1.05);
        }
      },
    },
    giz_power_chalice: { onCombatStart(c) { for (const a of rowAllies(c, c.unit, 1)) a.ap += Math.round(30 * sc(c)); } },
    giz_static_shroud: {
      onCombatReady(c) {
        const u = c.unit;
        for (const e of c.units) {
          if (!e.alive || e.team === u.team || Math.abs(e.c - u.c) > 1 || e.state.shroud || !(e.maxMana > 0)) continue;
          e.state.shroud = e.maxMana;
          e.mana = 0;
          e.maxMana = Math.round(e.maxMana * (1 + 0.35 * sc(c)));
        }
      },
      onAnyCast(c, e) { const u = e.unit; if (u.state.shroud) { u.maxMana = u.state.shroud; u.state.shroud = 0; } },
    },
    giz_wind_turbine: {
      onCombatReady(c) {
        const u = c.unit;
        const spot = { c: c.cols - 1 - u.c, r: c.rows - 1 - u.r };
        const foe = best(c.units.filter((e) => e.alive && e.team !== u.team), (e) => -c.dist(e, spot) * 100 - e.id);
        if (!foe) return;
        const dur = 5 * (sc(c) > 1 ? 1.4 : 1);
        foe.invulnUntil = Math.max(foe.invulnUntil, c.t + dur);
        foe.stun = Math.max(foe.stun, dur);
        c.text(foe, 'Blown away');
      },
    },
    giz_spell_shield: { onCombatStart(c) { for (const a of rowAllies(c, c.unit, 1)) a.state.ward = Math.max(a.state.ward || 0, 500 * sc(c)); } },
  };
  const items = {};
  for (const [id, hooks] of Object.entries(itemHooks)) { items[id] = { hooks }; items[id.replace('giz_', 'giz_oc_')] = { hooks }; }

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
        if (DT.isComponent(x)) return it.special ? x : c.pick(DT.STAT_COMPONENTS.filter((k) => k !== x));
        if (it.emblem) return c.pick(DT.EMBLEM_IDS.filter((k) => k !== x));
        if (it.radiant) return c.pick(Object.keys(DT.ITEMS).filter((k) => DT.ITEMS[k].radiant && k !== x));
        if (it.cat === 'special') return x;
        return c.pick(DT.CORE_ITEMS.filter((k) => k !== x));
      });
    }
    u.items = [];
    for (const x of back) c.give(x);
    return id;
  }
  items.giz_remover = { use: (c, u, idx) => giveBack(c, u, idx, false) };
  items.giz_reforger = { use: (c, u, idx) => giveBack(c, u, idx, true) };
  items.giz_copy = {
    use(c, u, idx) {
      const g = c.game, p = c.player;
      const L = DT.LINE[u.line];
      if (!L.shop || L.tier > 4) return 'none';
      const free = p.freeBenchSlot();
      if (free < 0) return 'benchFull';
      if (!(g.pool[u.line] > 0)) return 'none';
      const id = p.items[idx];
      p.items.splice(idx, 1);
      g.takeFromPool(u.line);
      p.bench[free] = g.newUnit(u.line, 0);
      g.combine(p);
      return id;
    },
  };
  items.giz_ockit = {
    use(c, u, idx) {
      const p = c.player;
      const k = u.items.findIndex((x) => DT.ITEMS[x] && DT.ITEMS[x].cat === 'completed' && DT.ITEMS[x.replace('giz_', 'giz_oc_')]);
      if (k < 0) return 'noItems';
      const id = p.items[idx];
      p.items.splice(idx, 1);
      u.items[k] = u.items[k].replace('giz_', 'giz_oc_');
      return id;
    },
  };

  // ---------- Digivice upgrades (augments) ----------
  const team = (fn) => ({ onCombatStart(c) { for (const u of c.mine) fn(u, c); } });
  const gold = (n) => ({ onPick(c) { c.player.gold += n; } });
  const traitCount = (c, t) => (DT.countTraits(c.player.boardUnits(), c.player.augs)[t] || 0);
  const traitAi = (t) => ({ bonus: (c) => (traitCount(c, t) >= 2 ? 3 : traitCount(c, t) ? 0.5 : -3) });
  const feather = (x) => ({ hooks: team((u) => { if (u.line.tier <= 2) u.as *= 1 + x; }) });
  const bless = (x) => ({ hooks: team((u) => { u.omnivamp += x; }) });
  const implant = (hp, ad) => ({ hooks: team((u) => { if (u.items.length) { u.maxHp += hp; u.ad += u.baseAd * ad; } }) });
  const thrill = (hp) => ({ hooks: { onKill(c, e) { if (e.unit.alive) c.heal(e.unit, hp); } } });
  const knife = (x) => ({ hooks: team((u) => { if (u.homeR <= 5) u.ad += u.baseAd * x; }) });
  const makeshift = (x) => ({ hooks: team((u) => { if (!u.items.length) { u.armor += x; u.mr += x; } }) });
  const united = (x) => ({ hooks: { onCombatStart(c) { const n = c.activeTraits(c.team); for (const u of c.mine) { u.ad += u.baseAd * x * n; u.ap += Math.round(x * 100 * n); } } } });

  const augments = {
    giz_a_spare: { hooks: gold(10) },
    giz_a_parts: { hooks: { onPick(c) { c.give(c.randomComponent()); c.give(c.randomComponent()); } } },
    giz_a_feather1: feather(0.25),
    giz_a_bless1: bless(0.1),
    giz_a_implant1: implant(200, 0.1),
    giz_a_thrill1: thrill(300),
    giz_a_knife1: knife(0.15),
    giz_a_makeshift1: makeshift(30),
    giz_a_exiles1: {
      hooks: {
        onCombatReady(c) {
          for (const u of c.mine) if (!u.summoned && !c.adjacent(u).some((a) => a.team === u.team)) c.shield(u, u.maxHp * 0.35);
        },
      },
    },
    giz_a_united1: united(0.02),
    giz_a_ascend: { hooks: { onTick(c) { if (c.state.done || c.t < 15) return; c.state.done = true; for (const u of alive(c.mine)) u.amp += 0.4; } } },
    giz_a_calc: {
      hooks: {
        onRoundEnd(c, e) { if (!e.won) { e.gold += 3; c.player.mech.calcRoll = 1; } },
        onRoundStart(c) { if (c.player.mech.calcRoll) { c.player.freeRolls += 1; c.player.mech.calcRoll = 0; } },
      },
    },
    giz_a_hyper: { hooks: { onRoundEnd(c, e) { if (c.player.gold + e.gold < 10) e.gold += 2; } } },
    giz_a_weak: {
      hooks: {
        onCombatStart(c) { for (const u of c.mine) u.P.shred = Math.max(u.P.shred || 0, 0.2); },
        onAttack(c, e) { if (e.target.alive) e.target.woundUntil = c.t + 5; },
      },
    },
    giz_a_firstaid: {
      hooks: {
        onCombatStart(c) { const ts = c.teamState(c.team); ts.healAmp = (ts.healAmp || 0) + 0.3; },
        onShield(c, e) { if (e.amount > 0) e.unit.shield += e.amount * 0.3; },
      },
    },
    giz_a_scoped: { hooks: team((u) => { if (u.range >= 2) u.range += 1; }) },
    giz_a_underdog: {
      hooks: {
        onTick(c) {
          if (c.state.next == null) c.state.next = 1;
          if (c.t < c.state.next) return;
          c.state.next += 1;
          const mine = c.survivors(c.team), theirs = c.survivors(1 - c.team);
          if (mine.length >= theirs.length) return;
          for (const u of mine) c.heal(u, Math.min(150, u.maxHp * 0.04), true);
        },
      },
    },
    giz_a_booster: {
      hooks: {
        onCountTraits(counts) {
          let bestT = null;
          for (const t in counts) if (DT.TRAITS[t] && DT.TRAITS[t].kind === 'origin' && (!bestT || counts[t] > counts[bestT])) bestT = t;
          if (bestT) counts[bestT] += 1;
        },
      },
    },
    giz_a_runic: { hooks: { onCombatReady(c) { for (const u of c.mine) if (has(u, 'ARC')) c.shield(u, u.ap * 3); } }, ai: traitAi('ARC') },
    giz_a_overload: {
      hooks: {
        onDeath(c, e) {
          const u = e.unit;
          if (!has(u, 'XA') || u.summoned) return;
          c.event({ type: 'burst', c: u.c, r: u.r, radius: 2, team: u.team });
          for (const x of c.enemiesNear(u.team, u, 2)) c.damage(u, x, u.maxHp * 0.25, 'magic');
        },
      },
      ai: traitAi('XA'),
    },
    giz_a_sosmall: { hooks: team((u) => { if (has(u, 'PET')) { u.dodge += 0.4; u.as *= 1.1; } }), ai: traitAi('PET') },
    giz_a_cutthroat: {
      hooks: { onAttack(c, e) { const u = e.unit; if (has(u, 'ASN') && !u.state.cut) { u.state.cut = true; e.target.mana = 0; c.text(e.target, 'Mana cut'); } } },
      ai: traitAi('ASN'),
    },
    // Gold
    giz_a_bank: { hooks: gold(18) },
    giz_a_chip: { hooks: { onPick(c) { c.give('giz_chip'); c.give(c.randomComponent()); } } },
    giz_a_crestkit: { hooks: { onPick(c) { c.give(c.randomEmblem(true)); } } },
    giz_a_airdrop: {
      hooks: {
        onCombatStart(c) {
          for (const u of c.mine) {
            if (u.items.length < 2 || !DT.CORE_ITEMS.length) continue;
            applyItem(u, DT.CORE_ITEMS[Math.floor(c.rng() * DT.CORE_ITEMS.length)]);
          }
        },
      },
    },
    giz_a_bless2: bless(0.18),
    giz_a_implant2: implant(300, 0.15),
    giz_a_feather2: feather(0.4),
    giz_a_knife2: knife(0.25),
    giz_a_makeshift2: makeshift(45),
    giz_a_united2: united(0.03),
    giz_a_thrill2: thrill(500),
    giz_a_trade: { hooks: { onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    giz_a_salvage: { hooks: { onPick(c) { c.give(c.randomCompleted()); } } },
    giz_a_shrug: { hooks: team((u) => { if (has(u, 'BRU')) u.regen += 0.025; }), ai: traitAi('BRU') },
    giz_a_spellblade: {
      hooks: {
        onCast(c, e) { if (has(e.unit, 'ARC')) e.unit.state.sb = true; },
        onAttack(c, e) { const u = e.unit; if (u.state.sb && e.target.alive) { u.state.sb = false; c.damage(u, e.target, u.ap * 2, 'magic'); } },
      },
      ai: traitAi('ARC'),
    },
    giz_a_stopwatch: {
      hooks: {
        onTick(c) {
          if (c.state.done || c.t < 5) return;
          c.state.done = true;
          for (const u of c.units) if (u.alive && u.team !== c.team) c.stun(u, 2);
        },
        onCombatStart(c) { for (const u of c.mine) if (has(u, 'CLOCK')) u.as *= 1.3; },
      },
      ai: traitAi('CLOCK'),
    },
    giz_a_smoke: {
      hooks: {
        onDamageTaken(c, e) {
          const u = e.unit;
          if (!has(u, 'ASN') || u.state.smoke || u.hp >= u.maxHp * 0.6) return;
          u.state.smoke = true;
          u.invulnUntil = Math.max(u.invulnUntil, c.t + 1.5);
          u.stun = 0;
          c.text(u, 'Smoke Bomb');
        },
      },
      ai: traitAi('ASN'),
    },
    giz_a_injection: { hooks: {}, ai: traitAi('XA') },
    giz_a_reserves: {
      hooks: {
        onRoundStart(c) { c.player.mech.goldStart = c.player.gold; },
        onCombatStart(c) {
          const g = ((c.side(c.team) && c.side(c.team).mech) || {}).goldStart || 0;
          for (const u of c.mine) if (has(u, 'PIR')) u.amp += Math.min(0.45, 0.15 + 0.01 * g);
        },
      },
      ai: traitAi('PIR'),
    },
    giz_a_junkyard: {
      hooks: {
        onRoundEnd(c, e) {
          const counts = e.match && e.match.combat ? e.match.combat.traitCounts[e.team] : {};
          if (!DT.traitLevel('JUNK', (counts && counts.JUNK) || 0)) return;
          const p = c.player;
          p.mech.junk = (p.mech.junk || 0) + 1;
          if (p.mech.junk % 3 === 0) c.give(c.randomComponent());
        },
      },
      ai: traitAi('JUNK'),
    },
    giz_a_titanic: { hooks: { onCombatReady(c) { for (const u of c.mine) if (u.maxHp >= 1400) u.ad += u.maxHp * 0.03; } } },
    giz_a_censer: {
      hooks: {
        onShield(c, e) {
          const u = e.unit;
          if (!u.alive || u.state.censer > c.t) return;
          u.state.censer = c.t + 3;
          c.buff(u, 'as', 0.3, 5);
        },
      },
    },
    // Prismatic
    giz_a_jackpot: { hooks: gold(30) },
    giz_a_grabbag: { hooks: { onPick(c) { c.give(c.randomCompleted()); c.give(c.randomCompleted()); } } },
    giz_a_recruit: { hooks: { onTeamSize(c, e) { e.size += 1; } } },
    giz_a_bless3: bless(0.25),
    giz_a_implant3: implant(400, 0.2),
    giz_a_feather3: feather(0.6),
    giz_a_highend: { hooks: { onShop(c, e) { e.level += 1; }, onPick(c) { c.player.freeRolls += 1; }, onRoundStart(c) { c.player.freeRolls += 1; } } },
    giz_a_ticket: { hooks: { onPick(c) { c.player.gold += 5; }, onShopRolled(c) { if (c.rng() < 0.5) c.player.freeRolls += 1; } } },
    giz_a_woodland: {
      hooks: {
        onCombatReady(c) {
          const b = best(c.mine.filter((u) => u.alive && !u.summoned), (u) => u.maxHp * 100 - u.id);
          if (!b) return;
          const cell = c.freeCellNear(b, b);
          if (!cell) return;
          const copy = c.summon(c.team, b.line.id, b.stage, cell.c, cell.r, { noHooks: true });
          if (copy) { copy.maxHp = 1500; copy.hp = 1500; c.text(copy, 'Copy'); }
        },
      },
    },
    giz_a_windfall: { hooks: { onPick(c) { c.player.gold += 18 * c.player.augs.length; } } },
    giz_a_forge: { hooks: { onPick(c) { c.give(c.pick(Object.keys(DT.ITEMS).filter((k) => DT.ITEMS[k].radiant))); } } },
    giz_a_burst: {
      hooks: { onCountTraits(counts) { for (const t in counts) if (counts[t] >= 2) counts[t] += 1; } },
    },
    giz_a_united3: united(0.05),
    giz_a_band: { hooks: { onPick(c) { c.give('giz_thief_glove'); c.give(c.randomComponent()); } } },
  };

  // ---------- ability types ----------
  const abilities = {
    snipe: {
      base: 260,
      cast(c, u, tg, v) {
        const far = c.target(u, 'farthest') || tg;
        c.event({ type: 'beam', from: u.id, to: far.id });
        c.abilityHit(u, far, v);
      },
      text: (line, stage, v) => `Fires at the farthest enemy for ${v} magic damage.`,
    },
    grab: {
      base: 190,
      cast(c, u, tg, v) {
        const far = c.target(u, 'farthest') || tg;
        c.event({ type: 'beam', from: u.id, to: far.id });
        const cell = c.freeCellNear(u, far);
        if (cell && c.dist(u, far) > 1) c.move(far, cell.c, cell.r, 0.25);
        c.stun(far, 1.5);
        c.abilityHit(u, far, v);
        for (const a of c.alliesOf(u)) if (c.dist(a, far) <= a.range + 1) a.target = far;
      },
      text: (line, stage, v) => `Pulls the farthest enemy next to it, stunning it for 1.5s and dealing ${v} magic damage. Nearby allies switch to that target.`,
    },
    devour: {
      base: 280,
      cast(c, u, tg, v) {
        c.event({ type: 'hit', from: u.id, to: tg.id, big: true });
        c.stun(tg, 2.5);
        c.abilityHit(u, tg, v);
        c.buff(u, 'dr', 0.3, 2.5);
      },
      text: (line, stage, v) => `Swallows the target, stunning it for 2.5s and dealing ${v} magic damage. Takes 30% less damage meanwhile.`,
    },
    visions: {
      base: 85,
      cast(c, u, tg, v) {
        const foes = c.enemiesOf(u);
        const fresh = foes.filter((e) => !(e.state.visUntil > c.t));
        const e = best(fresh.length ? fresh : foes, (x) => -c.dist(u, x) * 10000 - x.hp) || tg;
        e.state.visUntil = c.t + 6;
        c.event({ type: 'beam', from: u.id, to: e.id });
        c.buff(e, 'mr', -Math.round(Math.max(0, e.mr) * 0.4), 6);
        for (let k = 1; k <= 6; k++) c.after(k, () => { if (e.alive) c.abilityHit(u, e, v); });
      },
      text: (line, stage, v) => `Infects the closest enemy that is not infected yet: ${v} magic damage every second for 6s, and 40% less magic resist.`,
    },
  };

  // ---------- Hextech Gates mechanic ----------
  const TIER = ['', 'Silver', 'Gold', 'Prismatic'];
  const gateIndex = (R, stage) => (R.offers || []).findIndex((o) => o.kind === 'upgrade' && o.stage === stage);
  const gateTier = (g, stage) => {
    const gi = gateIndex(g.R, stage);
    return (g.mech.gates && g.mech.gates[gi]) || 2;
  };
  const gateText = (g) => (g && g.mech && g.mech.gates ? `Gates: ${g.mech.gates.map((t) => TIER[t]).join(', ')}.` : '');

  function chestRoll(c, lv) {
    const p = c.player;
    const ch = p.mech.chest || (p.mech.chest = { gold: 0, items: [], shut: 0 });
    let sum = ch.shut + (lv >= 2 ? 2 : 0);
    for (let k = 0; k < 2; k++) sum += 1 + Math.floor(c.rng() * 6);
    if (sum <= 6) ch.gold += 1;
    else if (sum <= 9) ch.gold += 2;
    else if (sum <= 11) { if (ch.items.length < 4) ch.items.push(c.randomComponent()); else ch.gold += 3; }
    else if (sum <= 13) { ch.gold += 2; if (ch.items.length < 4) ch.items.push(c.randomComponent()); }
    else { if (ch.items.length < 4) ch.items.push(c.randomCompleted()); else ch.gold += 5; }
  }

  const upgradeKind = {
    name: 'Digivice Upgrade',
    get sub() {
      const g = DT.UI && DT.UI.game ? DT.UI.game() : null;
      return `Pick one upgrade. It lasts the whole game. ${gateText(g)}`;
    },
    options(c, exclude = []) {
      const p = c.player, g = c.game;
      const tier = gateTier(g, c.stage);
      let pool = DT.AUGMENTS.filter((a) => a.tier === tier && !p.augs.includes(a.id) && !exclude.includes(a.id));
      if (pool.length < 3) pool = DT.AUGMENTS.filter((a) => !p.augs.includes(a.id) && !exclude.includes(a.id));
      return g.shuffle(pool.slice()).slice(0, 3).map((a) => a.id);
    },
    choose(c, id) { if (DT.AUGMENT[id]) c.game.applyAugment(c.player, id); },
    aiScore(c, id) {
      const a = DT.AUGMENT[id];
      if (!a) return 0;
      const tags = a.tags || [];
      let s = tags.includes('econ') ? (c.stage <= 2 ? 3 : 0.5) : tags.includes('items') ? 2 : c.stage >= 3 ? 3 : 1.5;
      if (a.ai && a.ai.bonus) s += a.ai.bonus(c);
      return s + (a.tier || 1) * 0.1;
    },
    card(id) {
      const a = DT.AUGMENT[id] || { name: id, desc: '', tier: 1 };
      const img = DT.ItemArt && DT.ItemArt.img ? DT.ItemArt.img(`aug:${id}`, 64, 'augart') : '';
      return { name: a.name, desc: `${TIER[a.tier]}. ${a.desc}`, img };
    },
  };

  const mechanic = {
    id: 'gates',
    name: 'Hextech Gates',
    desc: 'Three Digivice upgrades per game (2-1, 3-2, 4-2), in Silver, Gold or Prismatic tiers. The tiers of all three rounds are rolled for the whole lobby when the game starts and shown on every upgrade pick. Silver gates also give every Tamer a component and 4 gold, Gold gates 2 gold.',
    hooks: {
      onGameStart(c) {
        const g = c.game;
        g.mech.gates = c.pick(g.R.gates || [[2, 2, 2]]).slice();
        g.mech.mutation = c.pick(MUTATIONS);
      },
      onRoundStart(c) {
        const g = c.game, p = c.player;
        // gate payout
        const gi = (g.R.offers || []).findIndex((o) => o.kind === 'upgrade' && o.stage === c.stage && o.round === c.round);
        if (gi >= 0) {
          const t = g.mech.gates ? g.mech.gates[gi] : 2;
          if (t === 1) { c.give(c.randomComponent()); p.gold += 4; } else if (t === 2) p.gold += 2;
        }
        // Pirates: dice add loot to the chest before a player combat
        if (g.roundType === 'pvp') {
          const counts = DT.countTraits(p.boardUnits(), p.augs);
          const lv = DT.traitLevel('PIR', counts.PIR || 0);
          if (lv) chestRoll(c, lv);
        }
      },
      onRoundEnd(c, e) {
        const ch = c.player.mech.chest;
        if (!ch || e.pve) return;
        if (e.won && (ch.gold || ch.items.length)) {
          e.gold += ch.gold;
          for (const x of ch.items) c.give(x);
          c.player.mech.chest = { gold: 0, items: [], shut: 0 };
        } else if (!e.won) ch.shut = Math.min(6, ch.shut + 1);
      },
      // Glutton: selling feeds Gekomon
      onSell(c, e) {
        const sold = e.unit;
        if (!sold || sold.line === 'giz_gekomon') return;
        const tahm = c.player.allUnits().find((u) => u.line === 'giz_gekomon');
        if (!tahm) return;
        const v = DT.unitValue(sold);
        const m = tahm.mods || {};
        tahm.mods = { ...m, glutHp: Math.min(1200, (m.glutHp || 0) + 20 * v), glutAp: Math.min(60, (m.glutAp || 0) + v) };
      },
      onCombine(c, e) {
        if (e.unit.line !== 'giz_gekomon') return;
        let hp = 0, ap = 0;
        for (const u of e.from) if (u.mods) { hp = Math.max(hp, u.mods.glutHp || 0); ap = Math.max(ap, u.mods.glutAp || 0); }
        if (hp || ap) e.unit.mods = { ...(e.unit.mods || {}), glutHp: hp, glutAp: ap };
      },
      // combat (every unit): Colossus stun immunity, D-Brigade stuns breaking, Spell Shield wards
      onCombatStart(c) { for (const u of c.units) if (u.line.traits.includes('COL')) u.P.ccImmune = 999; },
      onDamageTaken(c, e) {
        const u = e.unit;
        if (u.state.dbBreak != null && u.hp <= u.state.dbBreak) { u.state.dbBreak = null; if (u.stun > 0) u.stun = 0; }
      },
      onBeforeDamageTaken(c, e) {
        const tg = e.target;
        if (!(tg.state.ward > 0) || e.kind !== 'magic' || !(e.amount > 0)) return;
        e.mult *= e.amount <= tg.state.ward ? 0 : (e.amount - tg.state.ward) / e.amount;
        tg.state.ward = 0;
        c.text(tg, 'Blocked');
      },
    },
    offerKinds: { upgrade: upgradeKind },
  };

  DT.Sets.extend('gizmos', { traits, items, augments, abilities, mechanic });
})(typeof window !== 'undefined' ? window : globalThis);
