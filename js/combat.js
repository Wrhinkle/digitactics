// Hex grid helpers and the auto-battle simulation. No DOM access, so it also runs headless.
// Set content plugs in through hooks (traits, items, augments, Tamer powers, the set mechanic) and ability types;
// see docs/sets/FORMAT.md. Everything here is deterministic: randomness only from the combat's seeded rng.
(function (G) {
  const DT = G.DT;
  const { COLS, ROWS } = DT;

  // odd-r offset coordinates -> axial distance
  function dist(a, b) {
    const aq = a.c - (a.r - (a.r & 1)) / 2;
    const bq = b.c - (b.r - (b.r & 1)) / 2;
    const dq = aq - bq;
    const dr = a.r - b.r;
    return (Math.abs(dq) + Math.abs(dr) + Math.abs(dq + dr)) / 2;
  }
  // Grids: the standard 7 x 8 board, and wider ones for Duos team fights (two boards side by side, 14 x 8).
  // One cached grid per size; cells go row by row, so a 7-column fight is exactly what it always was.
  const GRIDS = new Map();
  function gridFor(cols, rows) {
    const k = `${cols}x${rows}`;
    let g = GRIDS.get(k);
    if (g) return g;
    const cells = [];
    for (let r = 0; r < rows; r++) for (let c = 0; c < cols; c++) cells.push({ c, r });
    const neigh = cells.map((a) => cells.filter((b) => dist(a, b) === 1));
    g = { cols, rows, cells, neigh, idx: (c, r) => r * cols + c };
    GRIDS.set(k, g);
    return g;
  }
  const BASE = gridFor(COLS, ROWS);
  const CELLS = BASE.cells;
  const NEIGH = BASE.neigh;

  DT.hexDist = dist;
  DT.CELLS = CELLS;
  DT.neighbors = (c, r) => NEIGH[BASE.idx(c, r)];
  DT.gridFor = gridFor;

  DT.makeRng = function (seed) {
    let a = seed >>> 0;
    return function () {
      a = (a + 0x6d2b79f5) >>> 0;
      let t = a;
      t = Math.imul(t ^ (t >>> 15), t | 1);
      t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
      return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
    };
  };

  // The rules engine starts with the default set active, so the content globals exist for every module.
  if (DT.Sets && !DT.Sets.activeId) DT.Sets.activate(DT.Sets.DEFAULT);

  // Traits a unit contributes: its line's traits plus any emblems it holds.
  DT.unitTraits = function (u) {
    const L = DT.LINE[u.line];
    const extra = (u.items || []).map((id) => DT.ITEMS[id] && DT.ITEMS[id].emblem).filter((t) => t && !L.traits.includes(t));
    return extra.length ? L.traits.concat([...new Set(extra)]) : L.traits;
  };

  // Count unique shop lines per trait. augs: the side's augment ids (augments and the set mechanic may change counts
  // through onCountTraits(counts, units, augs)). A unit's bonusTraits ({ trait: n }) add n more (Chosen-style).
  DT.countTraits = function (units, augs) {
    const byLine = new Map();
    let bonus = null;
    for (const u of units) {
      const L = DT.LINE[u.line];
      if (!L.shop) continue;
      const set = byLine.get(L.id) || new Set();
      for (const t of DT.unitTraits(u)) set.add(t);
      byLine.set(L.id, set);
      if (u.bonusTraits) { bonus = bonus || {}; for (const t in u.bonusTraits) bonus[t] = Math.max(bonus[t] || 0, u.bonusTraits[t]); }
    }
    const counts = {};
    for (const set of byLine.values()) for (const t of set) counts[t] = (counts[t] || 0) + 1;
    if (bonus) for (const t in bonus) counts[t] = (counts[t] || 0) + bonus[t];
    if (augs) {
      for (const a of augs) {
        const A = DT.AUGMENT && DT.AUGMENT[a];
        if (A && A.hooks && A.hooks.onCountTraits) DT.hookFn(A.hooks.onCountTraits)(counts, units, augs);
      }
    }
    const S = DT.SET;
    if (S && S.hooks.onCountTraits) DT.hookFn(S.hooks.onCountTraits)(counts, units, augs);
    return counts;
  };
  DT.traitLevel = function (id, n) {
    let lv = 0;
    const T = DT.TRAITS[id];
    if (!T) return 0;
    T.bp.forEach((b, i) => { if (n >= b) lv = i + 1; });
    return lv;
  };

  // A hook is a function or { order, fn }. Lower order runs first; ties keep source order
  // (traits in count order, then augments, Tamer power, global items, held items, then the set mechanic).
  DT.hookFn = (h) => (typeof h === 'function' ? h : h && h.fn);
  const hookOrder = (h) => (h && typeof h === 'object' && typeof h.order === 'number' ? h.order : 0);
  const COMBAT_HOOK = new Set(DT.Sets ? [...DT.Sets.COMBAT_HOOKS, ...DT.Sets.ANY_HOOKS] : []);

  const TICK = 1 / 30;
  const MOVE_TIME = 0.4;
  const OVERTIME_START = 30;
  const TIME_LIMIT = 45;

  // ---------- combat context (what hooks receive) ----------
  // Each listener gets its own object whose prototype is this API, with fields for its source:
  //   kind ('trait' | 'augment' | 'power' | 'global' | 'item' | 'mechanic'), id, team, state (per combat),
  //   trait: T, lv, n, v, val(key), members, isMember(u); item: unit, item, count; augment: A; power: P, rank.
  const API = {
    get t() { return this.cb.t; },
    get units() { return this.cb.units; },
    // c.mine: units of this listener's team; for an augment, Tamer power or global item of one Duos partner, only that
    // partner's units (c.owner = their player id)
    get mine() { return this.cb.units.filter((u) => u.team === this.team && (this.owner == null || u.owner === this.owner)); },
    // fight size: 7 x 8, or 14 x 8 for a Duos team fight (mirror a cell with cols - 1 - c, rows - 1 - r)
    get cols() { return this.cb.cols; },
    get rows() { return this.cb.rows; },
    // Duos team fights: the other partner's units on u's side (or this listener's side); empty in solo fights
    partnerUnits(u) {
      const o = u ? u.owner : this.owner;
      const team = u ? u.team : this.team;
      return o == null ? [] : this.cb.units.filter((x) => x.team === team && x.owner != null && x.owner !== o);
    },
    rng() { return this.cb.rng(); },
    dist,
    survivors(team) { return this.cb.survivors(team); },
    enemiesOf(u) { return this.cb.units.filter((e) => e.alive && e.team !== u.team); },
    alliesOf(u) { return this.cb.units.filter((a) => a.alive && a.team === u.team && a !== u); },
    enemiesNear(team, center, radius) { return this.cb.enemiesNear(team, center, radius); },
    alliesNear(team, center, radius) { return this.cb.units.filter((a) => a.alive && a.team === team && dist(center, a) <= radius); },
    unitAt(c, r) { const cb = this.cb; return c >= 0 && c < cb.cols && r >= 0 && r < cb.rows ? cb.occ[cb.idx(c, r)] : null; },
    adjacent(u) { const cb = this.cb; const out = []; for (const n of cb.NEIGH[cb.idx(u.c, u.r)]) { const a = cb.occ[cb.idx(n.c, n.r)]; if (a) out.push(a); } return out; },
    neighbors(c, r) { return this.cb.NEIGH[this.cb.idx(c, r)]; },
    freeCellNear(u, toward) { return this.cb.freeNeighbor(u, toward || u); },
    // target selection: 'closest' | 'farthest' | 'lowestHp' | 'highestHp' | 'highestAd' | 'random' | 'backline' among enemies of u
    target(u, mode = 'closest') {
      const list = this.enemiesOf(u);
      if (!list.length) return null;
      const by = (f) => list.reduce((b, e) => (f(e) < f(b) ? e : b));
      switch (mode) {
        case 'farthest': return by((e) => -dist(u, e));
        case 'lowestHp': return by((e) => e.hp);
        case 'highestHp': return by((e) => -e.hp);
        case 'highestAd': return by((e) => -e.ad);
        case 'random': return list[Math.floor(this.rng() * list.length)];
        case 'backline': return by((e) => (u.team === 0 ? e.r : -e.r));
        default: return by((e) => dist(u, e) * 10000 + e.hp);
      }
    },
    damage(src, tg, amount, kind = 'magic', crit = false) { return this.cb.damage(src, tg, amount, kind, crit); },
    // ability damage: can crit with abilCrit, and burns when the source has abilBurn
    abilityHit(src, tg, amount) { return this.cb.abil(src, tg, amount); },
    heal(tg, amount, quiet) { this.cb.heal(tg, amount, quiet); },
    shield(tg, amount) { this.cb.addShield(tg, amount); this.cb.ev({ type: 'shield', at: tg.id }); },
    stun(tg, dur) { this.cb.stunUnit(tg, dur); },
    freeze(tg, dur) { this.cb.stunUnit(tg, dur); this.cb.ev({ type: 'text', at: tg.id, text: 'Frozen' }); },
    silence(tg, dur) { tg.silenceUntil = Math.max(tg.silenceUntil || 0, this.cb.t + dur); this.cb.ev({ type: 'text', at: tg.id, text: 'Silenced' }); },
    burn(src, tg) { this.cb.applyBurn(src, tg); },
    weaken(tg, frac, dur) { tg.weak = Math.max(tg.weakUntil > this.cb.t ? tg.weak : 0, frac); tg.weakUntil = this.cb.t + dur; },
    blind(tg) { tg.blind = true; },
    // timed stat change. stat: as (fraction), ad, ap, armor, mr, dr, amp, crit, dodge, omnivamp, regen (flat adds).
    // A negative v is a debuff. dur in seconds (Infinity for the rest of combat).
    buff(u, stat, v, dur = Infinity) {
      const until = this.cb.t + dur;
      if (stat === 'as') { u.buffs.push({ stat: 'as', v, until }); return; }
      u[stat] = (u[stat] || 0) + v;
      if (dur !== Infinity) (u.tbuffs = u.tbuffs || []).push({ stat, v, until });
    },
    // summon a unit for team at (c, r) in board coordinates (team 1 is the top half); returns it or null if the cell is taken
    summon(team, lineId, stage = 0, c, r, opts = {}) { return this.cb.summon(team, lineId, stage, c, r, opts); },
    every(u, key, period) { return this.cb.every(u, key, period); },
    // run fn after delay seconds; with a unit, it runs during that unit's update (skipped if it died)
    after(delay, fn, u) {
      if (u) (u.timers = u.timers || []).push({ at: this.cb.t + delay, fn });
      else this.cb.timers.push({ at: this.cb.t + delay, fn });
    },
    // ---- board moves (hook, charge, blink, escape) ----
    // enemies on the hex line from u through tg, up to 8 hexes out (tg first if it is off the line)
    lineTargets(u, tg) { return this.cb.lineTargets(u, tg); },
    // move a unit on the board to a free cell over dur seconds; opts.leap shows the jump. Returns false if the
    // cell is off the board or taken.
    move(u, c, r, dur = 0.25, opts = {}) {
      if (c < 0 || c >= this.cb.cols || r < 0 || r >= this.cb.rows) return false;
      const at = this.cb.occ[this.cb.idx(c, r)];
      if (at && at !== u) return false;
      if (opts.leap) this.cb.ev({ type: 'leap', at: u.id, fc: u.c, fr: u.r });
      this.cb.place(u, c, r, dur);
      return true;
    },
    // put a unit that is off the grid (revived, ejected, returning from untargetable) onto a free cell at once
    place(u, c, r) {
      const cb = this.cb;
      if (c < 0 || c >= cb.cols || r < 0 || r >= cb.rows || cb.occ[cb.idx(c, r)]) return false;
      u.c = c; u.r = r; u.fromC = c; u.fromR = r; u.moveT = 1; u.moveDur = 1;
      cb.occ[cb.idx(c, r)] = u;
      return true;
    },
    // take a unit off the grid without a knockout (it stops acting; its cell is freed)
    remove(u) {
      u.alive = false;
      if (this.cb.occ[this.cb.idx(u.c, u.r)] === u) this.cb.occ[this.cb.idx(u.c, u.r)] = null;
    },
    // knock a unit out now (fires onDeath / onKill)
    kill(u, by = null) { if (u.alive) this.cb.kill(u, by); },
    // cast u's ability again at once, at tg (Mage-style double casts). Fires onCast again, so guard against loops.
    castAgain(u, tg) { if (u.alive && tg) this.cb.cast(u, tg); },
    text(u, text) { this.cb.ev({ type: 'text', at: u.id, text }); },
    event(e) { this.cb.ev(e); },
    teamState(team) { return this.cb.teamFx[team]; },
    // the game's view for this combat: { stage, round, type, mech } (mech = the set mechanic's game state)
    get info() { return this.cb.info; },
    // a team's side: { augs, power, globals, mech (that player's mechanic state), buffs }
    // In a Duos team fight the side merges both partners (both partners' augs and globals, the first partner's power and
    // mechanic state); a listener that belongs to one partner (c.owner set) gets that partner's own side.
    side(team) {
      const s = this.cb.sides[team];
      if (this.owner != null && s && s.parts) { const p = s.parts.find((x) => x.owner === this.owner); if (p) return p; }
      return s;
    },
    counts(team) { return this.cb.traitCounts[team]; },
    traitLevel(team, id) { const n = this.cb.traitCounts[team][id]; return n ? DT.traitLevel(id, n) : 0; },
    activeTraits(team) { const counts = this.cb.traitCounts[team]; return Object.keys(counts).filter((id) => DT.traitLevel(id, counts[id]) > 0).length; },
    val(key) { return this.T && this.T[key] ? this.T[key][this.lv - 1] : undefined; },
    isMember(u) { return !!(this.id && u.traits.includes(this.id)); },
  };

  class Combat {
    // sideA / sideB: { units: [{line, stage, c, r, uid, items, bonusTraits}], augs, buffs, power: {id, rank}, globals, noTraits }
    // Both sides use their own coordinates (rows 4-7); side B is mirrored to the top half.
    constructor(sideA, sideB, opts = {}) {
      this.rng = DT.makeRng(opts.seed == null ? 1 : opts.seed);
      this.record = opts.record !== false;
      this.t = 0;
      this.overtime = 1;
      this.units = [];
      this.events = [];
      this.timers = [];
      this.done = false;
      this.winner = null;
      // Duos team fights are 14 columns wide (side.cols): each partner's board sits on its own half
      const grid = gridFor(Math.max(sideA.cols || COLS, sideB.cols || COLS), ROWS);
      this.cols = grid.cols;
      this.rows = grid.rows;
      this.idx = grid.idx;
      this.CELLS = grid.cells;
      this.NEIGH = grid.neigh;
      this.occ = new Array(this.cols * this.rows).fill(null);
      this.sides = [sideA, sideB];
      this.duo = !!(sideA.parts || sideB.parts);
      this.augs = [sideA.augs || [], sideB.augs || []];
      this.set = DT.SET;
      this.info = opts.info || {}; // from the game: { stage, round, type, mech }
      this.depth = 0;
      for (const u of sideA.units) this.add(u, 0, u.c, u.r);
      for (const u of sideB.units) this.add(u, 1, this.cols - 1 - u.c, this.rows - 1 - u.r);
      // side.counts: trait counts worked out by the game (Duos: both partners' counts summed, js/duos.js)
      const countsOf = (s, augs) => (s.noTraits ? {} : s.counts ? { ...s.counts } : DT.countTraits(s.units, augs));
      this.traitCounts = [countsOf(sideA, this.augs[0]), countsOf(sideB, this.augs[1])];
      this.teamFx = [{}, {}];
      this.buildHooks();
      for (const team of [0, 1]) {
        this.fireTeam('onCombatStart', team, {});
        const side = this.sides[team];
        if (side.parts) for (const part of side.parts) this.applyBuffs(team, part.buffs, part.owner);
        else this.applyBuffs(team, side.buffs);
      }
      this.fireGlobal('onCombatStart', {});
      for (const team of [0, 1]) this.fireTeam('onCombatReady', team, {});
      this.fireGlobal('onCombatReady', {});
      for (const u of this.units) this.finalize(u);
      this.checkEnd();
    }

    // base unit from a side entry; null if the cell is taken
    makeUnit(src, team, c, r) {
      const L = DT.LINE[src.line];
      const s = DT.unitStats(L, src.stage);
      const R = DT.ROLES[L.role || DT.roleOf(L)] || {};
      return {
        id: this.units.length, uid: src.uid, line: L, stage: src.stage, team, c, r,
        name: L.forms[src.stage], role: L.role || DT.roleOf(L), leap: !!(L.leap != null ? L.leap : R.leap),
        maxHp: s.maxHp, hp: s.maxHp, ad: s.ad, as: s.as, range: s.range, armor: s.armor, mr: s.mr,
        mana: s.mana, maxMana: s.maxMana, ap: 100, shield: 0,
        crit: 0, critDmg: 1.6, dodge: 0, omnivamp: 0, dr: 0, amp: 0, regen: 0, perHexAmp: 0, vsBurningAmp: 0, abilBurn: false,
        alive: true, target: null, atkCd: 0.15 + this.rng() * 0.3, moveCd: 0, stun: 0, lock: 0,
        buffs: [], leapt: false, fromC: c, fromR: r, moveT: 1, moveDur: 1,
        homeR: src.r, baseAd: s.ad, traits: DT.unitTraits(src), items: [], P: {}, lifelines: [],
        dealt: 0, taken: 0, healed: 0, nAtk: 0, rageN: 0, titanN: 0, tm: {}, burn: null,
        atkAs: 0, atkAsN: 0, atkAsMax: 12, weak: 0, weakUntil: 0, blind: false, invulnUntil: 0, silenceUntil: 0,
        state: {}, // per-unit scratch space for hooks
        mods: src.mods || null, // set data from the board unit (read-only)
        owner: src.owner != null ? src.owner : null, // Duos team fights: the player id whose board the unit came from
      };
    }

    add(src, team, c, r) {
      if (this.occ[this.idx(c, r)]) return; // invalid overlap: drop the duplicate
      const u = this.makeUnit(src, team, c, r);
      this.equip(u, src.items || []);
      this.units.push(u);
      this.occ[this.idx(c, r)] = u;
    }

    // mid-combat summon (hooks): base stats, items, then opts { hpMul, adMul, noHooks, owner }
    summon(team, lineId, stage, c, r, opts = {}) {
      if (!DT.LINE[lineId] || c < 0 || c >= this.cols || r < 0 || r >= this.rows || this.occ[this.idx(c, r)]) return null;
      const u = this.makeUnit({ line: lineId, stage, uid: -1, r: team === 0 ? r : this.rows - 1 - r, items: opts.items || [], owner: opts.owner }, team, c, r);
      u.summoned = true;
      this.equip(u, opts.items || []);
      if (opts.hpMul) u.maxHp = Math.round(u.maxHp * opts.hpMul);
      if (opts.adMul) u.ad *= opts.adMul;
      this.units.push(u);
      this.occ[this.idx(c, r)] = u;
      if (!opts.noHooks) this.bindItemHooks(u);
      this.finalize(u);
      this.ev({ type: 'spawn', at: u.id });
      return u;
    }

    // Item stats and passives. Thief Glove (p.thief) adds two random completed items.
    equip(u, items) {
      const list = [];
      const pool = DT.CORE_ITEMS.filter((x) => !(DT.ITEMS[x] && DT.ITEMS[x].p && DT.ITEMS[x].p.thief));
      for (const id of items) {
        list.push(id);
        if (DT.ITEMS[id] && DT.ITEMS[id].p && DT.ITEMS[id].p.thief) for (let k = 0; k < 2; k++) list.push(pool[Math.floor(this.rng() * pool.length)]);
      }
      u.items = list;
      const st = {};
      const P = u.P;
      for (const id of list) {
        const it = DT.ITEMS[id];
        if (!it) continue;
        for (const k in it.stats) st[k] = (st[k] || 0) + it.stats[k];
        for (const k in it.p) {
          const v = it.p[k];
          if (k === 'lifeline') u.lifelines.push({ ...v, used: false });
          else if (typeof v === 'number') P[k] = (P[k] || 0) + v;
          else P[k] = v;
        }
      }
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
      u.amp += P.amp || 0;
      u.omnivamp += P.vamp || 0;
      u.critDmg += P.critDmg || 0;
      u.dr += P.dr || 0;
      u.regen += P.regen || 0;
      if (P.abilCrit) u.abilCrit = true;
      if (P.bluebuff) u.maxMana = Math.max(20, u.maxMana - 10);
    }

    // ---------- hooks ----------
    buildHooks() {
      this.L = { team: [{}, {}], global: {}, any: {} };
      this.api = Object.create(API);
      this.api.cb = this;
      let seq = 0;
      const push = (bucket, name, h, ctx, rank) => {
        const fn = DT.hookFn(h);
        if (typeof fn !== 'function') return;
        const list = name.startsWith('onAny') ? (this.L.any[name] = this.L.any[name] || []) : (bucket[name] = bucket[name] || []);
        list.push({ fn, ctx, order: hookOrder(h), rank, seq: seq++ });
      };
      const addAll = (bucket, hooks, ctx, rank) => { for (const name in hooks) if (COMBAT_HOOK.has(name)) push(bucket, name, hooks[name], ctx, rank); };
      const mk = (props) => Object.assign(Object.create(this.api), { state: {} }, props);
      for (const team of [0, 1]) {
        const side = this.sides[team];
        const bucket = this.L.team[team];
        const mine = this.units.filter((u) => u.team === team);
        const counts = this.traitCounts[team];
        for (const id in counts) {
          const T = DT.TRAITS[id];
          const lv = DT.traitLevel(id, counts[id]);
          if (!lv || !T || !T.hooks) continue;
          addAll(bucket, T.hooks, mk({ kind: 'trait', id, T, lv, n: counts[id], v: T.lv[lv - 1], team, members: mine.filter((u) => u.traits.includes(id)) }), 0);
        }
        // a Duos team side: each partner's augments, power and global items act for that partner (owner)
        for (const part of side.parts || [side]) {
          const owner = side.parts ? part.owner : undefined;
          const own = owner != null ? { owner } : {};
          for (const a of side.parts ? part.augs || [] : this.augs[team]) {
            const A = DT.AUGMENT[a];
            if (A && A.hooks) addAll(bucket, A.hooks, mk({ kind: 'augment', id: a, A, team, ...own }), 1);
          }
          if (part.power && DT.POWER && DT.POWER[part.power.id]) {
            const P = DT.POWER[part.power.id];
            if (P.hooks) addAll(bucket, P.hooks, mk({ kind: 'power', id: P.id, P, rank: part.power.rank || 1, team, ...own }), 2);
          }
          for (const gid of part.globals || []) {
            const it = DT.ITEMS[gid];
            if (it && it.hooks) addAll(bucket, it.hooks, mk({ kind: 'global', id: gid, item: it, team, ...own }), 3);
          }
        }
      }
      for (const u of this.units) this.bindItemHooks(u, mk, push);
      const S = this.set;
      if (S && S.hooks) addAll(this.L.global, S.hooks, mk({ kind: 'mechanic', id: (S.mechanic && S.mechanic.id) || S.id, team: null }), 5);
      const sort = (list) => list.sort((a, b) => a.order - b.order || a.rank - b.rank || a.seq - b.seq);
      for (const b of [this.L.team[0], this.L.team[1], this.L.global, this.L.any]) for (const k in b) sort(b[k]);
      this.mk = mk;
      this.push = push;
    }

    // held items with hooks: one listener per item id per holder (copies share it; their passives stack in u.P)
    bindItemHooks(u, mk = this.mk, push = this.push) {
      const seen = new Set();
      for (const id of u.items) {
        if (seen.has(id)) continue;
        seen.add(id);
        const it = DT.ITEMS[id];
        if (!it || !it.hooks) continue;
        const ctx = mk({ kind: 'item', id, item: it, unit: u, team: u.team, count: u.items.filter((x) => x === id).length });
        for (const name in it.hooks) {
          if (!COMBAT_HOOK.has(name)) continue;
          if (name.startsWith('onAny')) push(null, name, it.hooks[name], ctx, 4);
          else {
            u.hooks = u.hooks || {};
            push(u.hooks, name, it.hooks[name], ctx, 4);
          }
        }
      }
      if (u.hooks) for (const k in u.hooks) u.hooks[k].sort((a, b) => a.order - b.order || a.seq - b.seq);
    }

    run(list, e) {
      if (this.depth > 8) return; // a hook that triggers itself
      this.depth++;
      try { for (const l of list) l.fn(l.ctx, e); } finally { this.depth--; }
    }

    // team events (no subject): the team's trait/augment/power/global listeners, then held items of the team
    fireTeam(name, team, e) {
      const list = this.L.team[team][name];
      if (list) this.run(list, e);
      for (const u of this.units) if (u.team === team && u.hooks && u.hooks[name] && (u.alive || name === 'onCombatEnd')) { e.unit = u; this.run(u.hooks[name], e); }
    }
    fireGlobal(name, e) {
      const list = this.L.global[name];
      if (list) this.run(list, e);
    }
    // subject events: subject's team listeners, items held by the subject, the set mechanic, then onAny<Event>
    fire(name, subject, e, anyName) {
      e.unit = subject;
      const tl = this.L.team[subject.team][name];
      // a Duos partner's own listeners (c.owner) only hear about that partner's units
      if (tl) this.run(this.duo ? tl.filter((l) => l.ctx.owner == null || l.ctx.owner === subject.owner) : tl, e);
      if (subject.hooks && subject.hooks[name]) this.run(subject.hooks[name], e);
      const gl = this.L.global[name];
      if (gl) this.run(gl, e);
      if (anyName) { const al = this.L.any[anyName]; if (al) this.run(al, e); }
    }
    has(name, subject, anyName) {
      return !!(this.L.team[subject.team][name] || (subject.hooks && subject.hooks[name]) || this.L.global[name] || (anyName && this.L.any[anyName]));
    }

    // One-combat buffs (Data Wisps and other next-combat effects): { as, armor (also MR), mana }
    applyBuffs(team, b, owner) {
      if (!b) return;
      for (const u of this.units) {
        if (u.team !== team || (owner != null && u.owner !== owner)) continue;
        if (b.as) u.as *= 1 + b.as;
        if (b.armor) { u.armor += b.armor; u.mr += b.armor; }
        if (b.mana) u.mana += b.mana;
      }
    }

    finalize(u) {
      if (u.P.crown) u.shield += u.maxHp * 0.25;
      u.hp = u.maxHp;
      u.mana = Math.min(u.maxMana - 1, u.mana);
    }

    ev(e) {
      if (this.record) { e.t = this.t; this.events.push(e); }
    }

    step(dt = TICK) {
      if (this.done) return;
      this.t += dt;
      const ot = this.info.overtime || OVERTIME_START; // the pace's combat timer (rules.combatTime via game.combatInfo)
      this.overtime = this.t > ot ? 1 + (this.t - ot) * 0.2 : 1;
      for (const u of this.units) if (u.alive) this.updateUnit(u, dt);
      for (const team of [0, 1]) this.fireTeam('onTick', team, { dt });
      this.fireGlobal('onTick', { dt });
      if (this.timers.length) {
        const due = this.timers.filter((x) => x.at <= this.t);
        if (due.length) { this.timers = this.timers.filter((x) => x.at > this.t); for (const x of due) x.fn(); }
      }
      this.checkEnd();
    }

    checkEnd() {
      const a = this.units.some((u) => u.alive && u.team === 0);
      const b = this.units.some((u) => u.alive && u.team === 1);
      if (!a || !b || this.t >= (this.info.timeLimit || TIME_LIMIT)) {
        this.done = true;
        this.winner = a && !b ? 0 : b && !a ? 1 : -1;
        if (this.L && !this.ended) {
          this.ended = true;
          for (const team of [0, 1]) this.fireTeam('onCombatEnd', team, { winner: this.winner });
          this.fireGlobal('onCombatEnd', { winner: this.winner });
        }
      }
    }

    survivors(team) {
      return this.units.filter((u) => u.alive && u.team === team);
    }

    updateUnit(u, dt) {
      if (u.buffs.length) u.buffs = u.buffs.filter((b) => b.until > this.t);
      if (u.tbuffs && u.tbuffs.length) {
        const gone = u.tbuffs.filter((b) => b.until <= this.t);
        if (gone.length) { u.tbuffs = u.tbuffs.filter((b) => b.until > this.t); for (const b of gone) u[b.stat] -= b.v; }
      }
      if (u.regen > 0) this.heal(u, u.maxHp * u.regen * dt, true);
      this.passives(u, dt);
      if (!u.alive) return;
      if (u.moveT < u.moveDur) u.moveT += dt;
      u.atkCd -= dt;
      u.moveCd -= dt;
      if (u.stun > 0) { u.stun -= dt; return; }
      if (u.lock > 0) { u.lock -= dt; return; }
      if (u.leap && !u.leapt) {
        u.leapt = true;
        this.leap(u);
        u.lock = 0.3;
        return;
      }
      if (u.moveCd > 0) return;

      let tg = u.target;
      if (!tg || !tg.alive || dist(u, tg) > u.range) {
        tg = this.closestInRange(u);
        u.target = tg;
      }
      if (tg) {
        if (u.maxMana > 0 && u.mana >= u.maxMana && !(u.silenceUntil > this.t)) { this.cast(u, tg); return; }
        if (u.atkCd <= 0) this.attack(u, tg);
        return;
      }
      this.moveTowardEnemy(u);
    }

    // timer-based effects; tm holds the next trigger time for each
    every(u, key, period) {
      if (u.tm[key] == null) u.tm[key] = period;
      if (this.t < u.tm[key]) return false;
      u.tm[key] += period;
      return true;
    }

    // Per-unit timed effects: burn, unit timers, onUnitTick hooks, then the item passive library.
    passives(u, dt) {
      const P = u.P;
      if (u.burn && u.burn.until > this.t) {
        u.burn.acc += u.maxHp * 0.015 * dt;
        if (u.burn.acc >= 20) {
          const a = u.burn.acc;
          u.burn.acc = 0;
          this.damage(u.burn.src, u, a, 'true', false, true);
          if (!u.alive) return;
        }
      }
      if (u.timers && u.timers.length) {
        const due = u.timers.filter((x) => this.t >= x.at);
        if (due.length) { u.timers = u.timers.filter((x) => this.t < x.at); for (const x of due) x.fn(); }
      }
      if (this.has('onUnitTick', u)) this.fire('onUnitTick', u, { dt });
      if (P.archangel && this.every(u, 'arch', 5)) u.ap += P.archangel;
      if (P.manaRegen && this.every(u, 'mregen', 3)) u.mana = Math.min(u.maxMana, u.mana + P.manaRegen);
      if (P.crown && !u.tm.crownDone && this.t >= 8) { u.tm.crownDone = true; u.ap += 30; }
      if (P.redemption && this.every(u, 'redemption', 5)) {
        for (const a of this.units) if (a.alive && a.team === u.team && dist(a, u) <= 2) this.heal(a, 80 + (a.maxHp - a.hp) * 0.1);
        this.ev({ type: 'burst', c: u.c, r: u.r, radius: 2, team: 1 - u.team });
      }
      if (P.sunfire && this.every(u, 'sunfire', 2)) {
        const near = this.enemiesNear(u.team, u, 2).sort((a, b) => dist(u, a) - dist(u, b))[0];
        if (near) this.applyBurn(u, near);
      }
      if ((P.gargoyle || P.veil) && this.every(u, 'aura', 1)) {
        const near = this.enemiesNear(u.team, u, 2);
        if (P.gargoyle) u.garg = near.length * P.gargoyle;
        if (P.veil) for (const e of near) { e.veil = Math.max(e.veilUntil > this.t ? e.veil : 0, P.veil); e.veilUntil = this.t + 1.1; }
      }
    }

    applyBurn(src, tg) {
      if (!tg.alive) return;
      tg.burn = { src, until: this.t + 5, acc: tg.burn ? tg.burn.acc : 0 };
      tg.woundUntil = this.t + 5;
    }

    stunUnit(e, dur) {
      if (e.P.ccImmune && this.t < e.P.ccImmune) {
        this.ev({ type: 'text', at: e.id, text: 'Immune' });
        return;
      }
      e.stun = Math.max(e.stun, dur);
    }

    addShield(u, amount) {
      if (!(amount > 0)) return;
      u.shield += amount;
      if (this.has('onShield', u, 'onAnyShield')) this.fire('onShield', u, { amount }, 'onAnyShield');
    }

    closestInRange(u) {
      let best = null;
      let bd = Infinity;
      for (const e of this.units) {
        if (!e.alive || e.team === u.team) continue;
        const d = dist(u, e);
        if (d <= u.range && (d < bd || (d === bd && e.hp < best.hp))) { best = e; bd = d; }
      }
      return best;
    }

    // BFS over free cells to the nearest cell that has an enemy in range; take one step.
    moveTowardEnemy(u) {
      const enemies = this.units.filter((e) => e.alive && e.team !== u.team);
      if (!enemies.length) return;
      const { idx, CELLS, NEIGH } = this;
      const start = idx(u.c, u.r);
      const prev = new Map([[start, -1]]);
      const queue = [start];
      let goal = -1;
      let goalTarget = null;
      while (queue.length) {
        const cur = queue.shift();
        const cell = CELLS[cur];
        if (cur !== start) {
          let best = null;
          let bd = Infinity;
          for (const e of enemies) {
            const d = dist(cell, e);
            if (d <= u.range && d < bd) { bd = d; best = e; }
          }
          if (best) { goal = cur; goalTarget = best; break; }
        }
        for (const n of NEIGH[cur]) {
          const ni = idx(n.c, n.r);
          if (prev.has(ni) || this.occ[ni]) continue;
          prev.set(ni, cur);
          queue.push(ni);
        }
      }
      if (goal < 0) { u.moveCd = 0.25; return; }
      let stepIdx = goal;
      while (prev.get(stepIdx) !== start) stepIdx = prev.get(stepIdx);
      const nc = CELLS[stepIdx];
      this.place(u, nc.c, nc.r, MOVE_TIME);
      u.moveCd = MOVE_TIME;
      u.target = goalTarget;
    }

    place(u, c, r, dur) {
      this.occ[this.idx(u.c, u.r)] = null;
      u.fromC = u.c; u.fromR = u.r;
      u.c = c; u.r = r;
      u.moveT = 0; u.moveDur = dur;
      this.occ[this.idx(c, r)] = u;
    }

    freeNeighbor(of, near) {
      const free = this.NEIGH[this.idx(of.c, of.r)].filter((n) => !this.occ[this.idx(n.c, n.r)]);
      if (!free.length) return null;
      free.sort((a, b) => dist(a, near) - dist(b, near));
      return free[0];
    }

    leap(u) {
      const back = this.units
        .filter((e) => e.alive && e.team !== u.team)
        .sort((a, b) => (u.team === 0 ? a.r - b.r : b.r - a.r));
      for (const e of back) {
        const cell = this.freeNeighbor(e, { c: e.c, r: u.team === 0 ? 0 : this.rows - 1 });
        if (cell) {
          this.ev({ type: 'leap', at: u.id, fc: u.c, fr: u.r });
          this.place(u, cell.c, cell.r, 0.25);
          u.target = e;
          return;
        }
      }
    }

    asMult(u) {
      let m = 1 + (u.P.rage || 0) * u.rageN + u.atkAs * u.atkAsN;
      for (const b of u.buffs) if (b.stat === 'as') m += b.v;
      return Math.min(m, 4);
    }

    attack(u, tg) {
      u.atkCd = 1 / (u.as * this.asMult(u));
      this.ev({ type: u.range > 1 ? 'shot' : 'hit', from: u.id, to: tg.id });
      if (u.blind) { u.blind = false; this.ev({ type: 'text', at: u.id, text: 'Miss' }); return; }
      u.mana = Math.min(u.maxMana, u.mana + 10 + (u.P.shojin || 0));
      u.nAtk++;
      if (u.atkAs && u.atkAsN < u.atkAsMax) u.atkAsN++;
      const P = u.P;
      if (P.rage) u.rageN++;
      if (P.kraken) u.ad += u.baseAd * P.kraken;
      if (P.titan) this.titanStack(u);
      if (tg.P.titan) this.titanStack(tg);
      if (tg.P.thorns && !(tg.thornCd > this.t)) {
        tg.thornCd = this.t + 1.5;
        for (const e of this.enemiesNear(tg.team, tg, 1)) this.damage(tg, e, tg.P.thorns, 'magic');
      }
      if (tg.dodge > 0 && this.rng() < tg.dodge) {
        this.ev({ type: 'text', at: tg.id, text: 'Dodge' });
        return;
      }
      let dmg = u.ad;
      let crit = false;
      if (u.crit > 0 && this.rng() < u.crit) { dmg *= u.critDmg; crit = true; }
      if (P.shred) { tg.shredUntil = this.t + 3; tg.shred = P.shred; }
      this.damage(u, tg, dmg, 'phys', crit);
      // a landed basic attack (not missed or dodged)
      if (this.has('onAttack', u, 'onAnyAttack')) this.fire('onAttack', u, { target: tg, dmg, crit }, 'onAnyAttack');
      if (P.split) {
        const other = this.enemiesNear(u.team, tg, 2).filter((e) => e !== tg).sort((a, b) => dist(tg, a) - dist(tg, b))[0];
        if (other) { this.ev({ type: 'shot', from: u.id, to: other.id }); this.damage(u, other, dmg * P.split, 'phys', crit); }
      }
      if (P.volt && u.nAtk % 3 === 0) {
        const pool = this.units.filter((e) => e.alive && e.team !== u.team);
        for (let k = 0; k < 3 && pool.length; k++) {
          const e = pool.splice(Math.floor(this.rng() * pool.length), 1)[0];
          this.ev({ type: 'beam', from: u.id, to: e.id });
          this.damage(u, e, P.volt, 'magic');
        }
      }
    }

    titanStack(u) {
      if (u.titanN >= 25) return;
      u.titanN++;
      u.ad += u.baseAd * 0.02;
      u.ap += 2;
    }

    // ability damage: can crit with abilCrit; burns the target when the caster has abilBurn
    abil(u, e, v) {
      const crit = !!u.abilCrit && u.crit > 0 && this.rng() < u.crit;
      const d = this.damage(u, e, crit ? v * u.critDmg : v, 'magic', crit);
      if (u.abilBurn && e.alive) this.applyBurn(u, e);
      return d;
    }

    damage(src, tg, amount, kind, crit = false, isBurn = false) {
      if (!tg.alive) return 0;
      if (tg.invulnUntil > this.t) return 0;
      let m = 1 + src.amp;
      if (src.weakUntil > this.t) m *= 1 - src.weak;
      if (src.vsBurningAmp && tg.burn && tg.burn.until > this.t) m *= 1 + src.vsBurningAmp;
      if (DT.ATTR_BEATS[src.line.attr] === tg.line.attr) m *= 1.2;
      if (src.perHexAmp) m *= 1 + src.perHexAmp * dist(src, tg);
      if (src.P.slayer && tg.maxHp >= 1600) m *= 1 + src.P.slayer;
      if (src.P.guardbreak && tg.shield > 0) m *= 1 + src.P.guardbreak;
      if (this.has('onBeforeDamage', src)) {
        const e = { target: tg, amount, kind, crit, isBurn, mult: m };
        this.fire('onBeforeDamage', src, e);
        m = e.mult;
      }
      m *= this.overtime;
      let d = amount * m;
      const veil = tg.veilUntil > this.t ? tg.veil : 0;
      const garg = tg.garg || 0;
      if (kind === 'phys') {
        let armor = tg.armor + garg - veil;
        if (tg.shredUntil > this.t) armor *= 1 - tg.shred;
        d *= 100 / (100 + Math.max(0, armor));
      } else if (kind === 'magic') d *= 100 / (100 + Math.max(0, tg.mr + garg - veil));
      let extraDr = 0;
      if (this.has('onBeforeDamageTaken', tg)) {
        const e = { source: src, target: tg, amount: d, kind, crit, isBurn, dr: 0, mult: 1 };
        this.fire('onBeforeDamageTaken', tg, e);
        extraDr = e.dr;
        d *= e.mult;
      }
      d *= 1 - Math.min(0.6, tg.dr + extraDr);
      let rest = d;
      if (tg.shield > 0) {
        const sh = Math.min(tg.shield, rest);
        tg.shield -= sh;
        rest -= sh;
      }
      tg.hp -= rest;
      src.dealt += d;
      tg.taken += d;
      if (tg.maxMana > 0 && !isBurn) tg.mana = Math.min(tg.maxMana, tg.mana + Math.min(15, d * 0.06));
      if (src.omnivamp > 0 && src.alive) this.heal(src, d * src.omnivamp, true);
      if (src.P.burn && !isBurn) this.applyBurn(src, tg);
      this.ev({ type: 'dmg', at: tg.id, amount: Math.round(d), kind, crit });
      if (tg.hp <= 0 && this.has('onLethal', tg)) this.fire('onLethal', tg, { source: src, amount: d, kind });
      if (tg.hp <= 0) this.kill(tg, src);
      else {
        this.checkLifeline(tg);
        if (this.has('onDamageTaken', tg)) this.fire('onDamageTaken', tg, { source: src, amount: d, kind, crit, isBurn });
      }
      if (this.has('onDamageDealt', src, 'onAnyDamageDealt')) this.fire('onDamageDealt', src, { target: tg, amount: d, kind, crit, isBurn, killed: !tg.alive }, 'onAnyDamageDealt');
      return d;
    }

    checkLifeline(u) {
      for (const L of u.lifelines) {
        if (L.used || u.hp / u.maxHp >= L.at) continue;
        L.used = true;
        u.shield += u.maxHp * L.shield;
        if (L.as) u.buffs.push({ stat: 'as', v: L.as, until: Infinity });
        if (L.ad) u.ad += u.baseAd * L.ad;
        if (L.armor) { u.armor += L.armor; u.mr += L.armor; }
        this.ev({ type: 'shield', at: u.id });
      }
    }

    heal(tg, amount, quiet) {
      if (!tg.alive || amount <= 0) return;
      if (tg.woundUntil > this.t) amount *= 0.67;
      amount *= 1 + (this.teamFx[tg.team].healAmp || 0);
      const before = tg.hp;
      tg.hp = Math.min(tg.maxHp, tg.hp + amount);
      tg.healed += tg.hp - before;
      if (!quiet && tg.hp > before) this.ev({ type: 'heal', at: tg.id, amount: Math.round(tg.hp - before) });
      if (tg.hp > before && this.has('onHeal', tg, 'onAnyHeal')) this.fire('onHeal', tg, { amount: tg.hp - before }, 'onAnyHeal');
    }

    kill(u, by) {
      u.alive = false;
      u.hp = 0;
      if (this.occ[this.idx(u.c, u.r)] === u) this.occ[this.idx(u.c, u.r)] = null;
      // Presentation metadata only: no RNG, no changes to combat outcomes. Keep it even in unrecorded AI fights.
      if (by && by.team !== u.team) {
        this.lastKills = this.lastKills || {};
        this.lastKills[by.team] = { id: by.id, uid: by.uid, line: by.line.id, stage: by.stage, owner: by.owner, victim: u.id, t: this.t };
      }
      this.ev({ type: 'death', at: u.id, by: by ? by.id : null });
      if (this.has('onDeath', u, 'onAnyDeath')) this.fire('onDeath', u, { killer: by || null }, 'onAnyDeath');
      if (by && by !== u && this.has('onKill', by, 'onAnyKill')) this.fire('onKill', by, { victim: u }, 'onAnyKill');
    }

    enemiesNear(team, center, radius) {
      return this.units.filter((e) => e.alive && e.team !== team && dist(center, e) <= radius);
    }

    afterCast(u) {
      if (u.P.castAS) u.buffs.push({ stat: 'as', v: u.P.castAS, until: this.t + 5 });
      if (u.P.bluebuff) u.mana = 20;
      for (const z of this.units) {
        if (z.alive && z.team !== u.team && z.P.zap && dist(z, u) <= 2) {
          this.ev({ type: 'beam', from: z.id, to: u.id });
          this.damage(z, u, z.P.zap, 'magic');
        }
      }
    }

    cast(u, tg) {
      u.mana = 0;
      u.lock = 0.35;
      const L = u.line;
      const v = (DT.abilityValue(L, u.stage) * u.ap) / 100;
      const R = L.radius || 1;
      this.ev({ type: 'cast', at: u.id, name: L.abil[u.stage] });
      this.afterCast(u);
      switch (L.type) {
        case 'nuke':
          this.ev({ type: 'beam', from: u.id, to: tg.id });
          this.abil(u, tg, v);
          break;
        case 'aoe': {
          this.ev({ type: 'burst', c: tg.c, r: tg.r, radius: R, team: u.team });
          for (const e of this.enemiesNear(u.team, tg, R)) this.abil(u, e, v);
          break;
        }
        case 'stun':
          this.ev({ type: 'beam', from: u.id, to: tg.id });
          this.stunUnit(tg, DT.stunDuration(u.stage));
          this.abil(u, tg, v);
          break;
        case 'slam': {
          this.ev({ type: 'burst', c: u.c, r: u.r, radius: R, team: u.team });
          for (const e of this.enemiesNear(u.team, u, R)) {
            this.stunUnit(e, 1);
            this.abil(u, e, v);
          }
          break;
        }
        case 'shield': {
          this.addShield(u, v);
          this.ev({ type: 'shield', at: u.id });
          for (const e of this.enemiesNear(u.team, u, 2)) e.target = u;
          break;
        }
        case 'heal': {
          const allies = this.units
            .filter((a) => a.alive && a.team === u.team)
            .sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp)
            .slice(0, DT.healTargets(u.stage));
          for (const a of allies) this.heal(a, v);
          break;
        }
        case 'barrage': {
          const shots = DT.barrageShots(L, u.stage);
          for (let i = 0; i < shots; i++) {
            const pool = this.units.filter((e) => e.alive && e.team !== u.team);
            if (!pool.length) break;
            const e = pool[Math.floor(this.rng() * pool.length)];
            this.ev({ type: 'shot', from: u.id, to: e.id, big: true });
            this.abil(u, e, v);
          }
          break;
        }
        case 'drain': {
          const hits = L.radius ? this.enemiesNear(u.team, tg, L.radius) : [tg];
          if (L.radius) this.ev({ type: 'burst', c: tg.c, r: tg.r, radius: L.radius, team: u.team });
          else this.ev({ type: 'beam', from: u.id, to: tg.id });
          let total = 0;
          for (const e of hits) total += this.abil(u, e, v);
          this.heal(u, total * 0.6);
          break;
        }
        case 'weaken':
          this.ev({ type: 'beam', from: u.id, to: tg.id });
          this.abil(u, tg, v);
          if (tg.alive) { tg.weak = Math.max(tg.weakUntil > this.t ? tg.weak : 0, 0.3); tg.weakUntil = this.t + 4; }
          break;
        case 'rally': {
          const allies = this.units
            .filter((a) => a.alive && a.team === u.team)
            .sort((a, b) => a.hp / a.maxHp - b.hp / b.maxHp || a.id - b.id)
            .slice(0, DT.rallyTargets(u.stage));
          for (const a of allies) {
            this.addShield(a, v);
            a.buffs.push({ stat: 'as', v: 0.3, until: this.t + 4 });
            this.ev({ type: 'shield', at: a.id });
          }
          break;
        }
        case 'line':
          for (const e of this.lineTargets(u, tg)) {
            this.ev({ type: 'beam', from: u.id, to: e.id });
            this.abil(u, e, v);
          }
          break;
        case 'execute': {
          const low = tg.hp < tg.maxHp * 0.5;
          this.ev({ type: dist(u, tg) <= 1 ? 'hit' : 'beam', from: u.id, to: tg.id, big: true });
          this.abil(u, tg, low ? v * 2 : v);
          break;
        }
        case 'chain': {
          const hit = new Set();
          let cur = tg;
          let val = v;
          for (let k = 0; k <= DT.chainJumps(u.stage) && cur; k++) {
            hit.add(cur);
            this.ev({ type: 'beam', from: u.id, to: cur.id });
            this.abil(u, cur, val);
            val *= 0.8;
            let next = null;
            let nd = Infinity;
            for (const e of this.units) {
              if (!e.alive || e.team === u.team || hit.has(e)) continue;
              const d = dist(cur, e);
              if (d <= 3 && d < nd) { next = e; nd = d; }
            }
            cur = next;
          }
          break;
        }
        case 'dash': {
          const pool = this.units.filter((e) => e.alive && e.team !== u.team).sort((a, b) => a.hp - b.hp);
          const e = pool[0];
          if (!e) break;
          if (dist(u, e) > 1) {
            const cell = this.freeNeighbor(e, u);
            if (cell) {
              this.ev({ type: 'leap', at: u.id, fc: u.c, fr: u.r });
              this.place(u, cell.c, cell.r, 0.2);
            }
          }
          u.target = e;
          this.ev({ type: 'hit', from: u.id, to: e.id, big: true });
          this.abil(u, e, v);
          break;
        }
        default: {
          // set-defined ability type: def.abilities[type].cast(c, u, tg, v)
          const d = DT.abilityDef(L.type);
          if (d && d.cast) d.cast(this.abilityCtx(u), u, tg, v);
        }
      }
      if (this.has('onCast', u, 'onAnyCast')) this.fire('onCast', u, { target: tg, value: v }, 'onAnyCast');
    }

    abilityCtx(u) {
      if (!this.actx) this.actx = Object.create(this.api);
      this.actx.kind = 'ability';
      this.actx.team = u.team;
      this.actx.unit = u;
      return this.actx;
    }

    // enemies on the hex line from u through tg, up to 8 hexes out (cube coordinates, exact rounding)
    lineTargets(u, tg) {
      const cube = (p) => [p.c - (p.r - (p.r & 1)) / 2, p.r];
      const [aq, ar] = cube(u);
      const [bq, br] = cube(tg);
      const n = Math.max(1, dist(u, tg));
      const out = [];
      const seen = new Set();
      for (let k = 1; k <= 8; k++) {
        const f = k / n;
        const q = aq + (bq - aq) * f + 1e-6;
        const r = ar + (br - ar) * f + 2e-6;
        const s = -q - r;
        let rq = Math.round(q), rr = Math.round(r);
        const rs = Math.round(s);
        const dq = Math.abs(rq - q), dr = Math.abs(rr - r), ds = Math.abs(rs - s);
        if (dq > dr && dq > ds) rq = -rr - rs;
        else if (dr > ds) rr = -rq - rs;
        if (rr < 0 || rr >= this.rows) break;
        const c = rq + (rr - (rr & 1)) / 2;
        if (c < 0 || c >= this.cols) break;
        const e = this.occ[this.idx(c, rr)];
        if (e && e.alive && e.team !== u.team && !seen.has(e)) { seen.add(e); out.push(e); }
      }
      if (!seen.has(tg)) out.unshift(tg);
      return out;
    }
  }

  DT.Combat = Combat;
  DT.CombatAPI = API;
  DT.TICK = TICK;

  DT.simulate = function (sideA, sideB, seed, info) {
    const cb = new Combat(sideA, sideB, { seed, record: false, info });
    let guard = 0;
    while (!cb.done && guard++ < 10000) cb.step(TICK);
    return cb;
  };
})(typeof window !== 'undefined' ? window : globalThis);
