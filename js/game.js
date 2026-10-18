// Match state: players, shared pool, shop, items, offers (Armory, augments, Tamer powers), rounds, digivolution,
// AI Tamers. No DOM access. Each match plays one set (js/sets/<id>.js); its rules come from set.rules (defaults in
// js/sets.js: Hyper Roll) and its behavior from hooks (docs/sets/FORMAT.md).
//
// Hyper Roll (Faction Wars) rules:
// - 20 Tamer HP. A loss costs a flat 2 HP in stages 1-4, 4 HP in stages 5-7, 6 HP after.
// - No XP: everyone's level is set by the stage. No interest, no streak gold. A PvP win pays 2 gold.
// - Rerolls cost 1 gold. One carousel at 1-1. Items come from the Armory and PvE drops.
// - Stage 1: 1-1 carousel, 1-2 and 1-3 PvE. Later stages: x-1 to x-3 PvP, x-4 PvE.
// - Armory at x-1 of stages 2-6, augments at 2-2, 3-2 and 4-2, a Data Wisp in the shop on even rounds.
(function (G) {
  const DT = G.DT;
  const { COLS, BENCH } = DT;

  const key = (c, r) => `${c},${r}`;
  const FRONT_COLS = [3, 2, 4, 1, 5, 0, 6];
  const GAME_HOOK = new Set(DT.Sets.GAME_HOOKS);

  // the active set's rules (stable object, updated by DT.Sets.activate)
  const H = DT.HYPER;

  const copiesOf = (line, R = DT.HYPER) => (DT.LINE[line] && DT.LINE[line].combine) || R.copies || 3;
  // copies of the base form a unit stands for (3 per star with the default rule)
  DT.unitCopies = (unit) => Math.pow(copiesOf(unit.line), unit.stage);
  // gold for selling a unit: the TFT table from its cost, or the set's sellValue(unit, base) (Dragonlands dragons)
  const baseSellValue = function (unit) {
    const cost = DT.LINE[unit.line].tier;
    const n = copiesOf(unit.line);
    if (unit.stage === 0) return cost;
    if (unit.stage === 1) return cost * n - (cost > 1 ? 1 : 0);
    return cost * n * n - 1;
  };
  DT.sellValue = function (unit) {
    const base = baseSellValue(unit);
    const S = DT.SET;
    if (!S || !S.sellValue) return base;
    const v = S.sellValue(unit, base);
    return typeof v === 'number' && isFinite(v) ? Math.max(0, Math.round(v)) : base;
  };
  // unit flags (on the unit or its line): noSell (cannot be sold), noItems (cannot hold items)
  const flag = (unit, k) => !!(unit && (unit[k] || (DT.LINE[unit.line] && DT.LINE[unit.line][k])));
  DT.canSell = (unit) => !flag(unit, 'noSell');
  DT.holdsItems = (unit) => !flag(unit, 'noItems');
  DT.unitValue = (unit) => DT.LINE[unit.line].tier * Math.pow(copiesOf(unit.line), unit.stage);
  DT.slotsOf = (unit) => (DT.LINE[unit.line] && DT.LINE[unit.line].slots) || 1;
  // team size added by held items (passive teamSize, e.g. the Golden Digi-Egg)
  const teamSizeItems = (u) => u.items.reduce((n, i) => n + ((DT.ITEMS[i] && DT.ITEMS[i].p && DT.ITEMS[i].p.teamSize) || 0), 0);

  class Player {
    constructor(id, name, isHuman, game = null) {
      const R = game ? game.R : DT.HYPER;
      this.id = id;
      this.name = name;
      this.isHuman = isHuman;
      this.hp = R.startHp;
      this.gold = R.startGold != null ? R.startGold : 3;
      this.level = R.xp ? R.xp.startLevel || 1 : R.levelByStage[1];
      this.xp = 0;
      this.board = {};
      this.bench = new Array(BENCH).fill(null);
      this.shop = new Array(R.shopSize || DT.SHOP).fill(null);
      this.shopMeta = null; // optional per-slot data from mechanics: [{ cost, unit: {...fields for the bought unit}, tag }]
      this.locked = false;
      this.streak = 0;
      this.alive = true;
      this.placement = 0;
      this.lastOpponent = null;
      this.items = [];
      this.augs = [];
      this.power = null; // Tamer power { id, rank } (Underlords mode)
      this.globals = []; // global items (affect the whole team, not equipped on a unit)
      this.offer = null; // pending Armory / augment / power choice
      this.freeRolls = 0;
      this.teamBonus = 0;
      this.nextBuffs = null;
      this.wisp = null; // { id, cost } (Faction Wars Data Wisp)
      this.orbs = []; // loot orbs waiting on the board: { id, kind, x, z, gold, items, unit }
      this.mech = {}; // per-player state for the set mechanic (plain data; it reaches combat as side.mech)
      this.team = null; // Duos: index into game.teams
      this.portal = 0; // Duos: portal sends used this round
      this.incoming = []; // Duos: what arrived through the portal this round [{ kind, from, line, stage, uid, item }] (UI effect)
      Object.defineProperty(this, 'game', { value: game, enumerable: false, writable: true });
    }
    boardUnits() { return Object.values(this.board); }
    // board slots used: units count once, lines with slots: 2 count twice
    boardCount() { let n = 0; for (const u of Object.values(this.board)) n += DT.slotsOf(u); return n; }
    benchCount() { return this.bench.filter(Boolean).length; }
    allUnits() { return this.boardUnits().concat(this.bench.filter(Boolean)); }
    freeBenchSlot() { return this.bench.indexOf(null); }
    // team size: level (or rules.teamSize(level)) plus Leap Wisps plus team-size items held on the board
    cap() {
      const g = this.game;
      const R = g ? g.R : DT.HYPER;
      const eggs = this.boardUnits().reduce((n, u) => n + teamSizeItems(u), 0);
      const base = (R.teamSize ? R.teamSize(this.level) : this.level) + this.teamBonus + eggs;
      if (g && g.hasHook('onTeamSize', this)) { const e = { size: base }; g.fireGame('onTeamSize', this, e); return e.size; }
      return base;
    }
    boardSide() {
      return Object.entries(this.board).map(([k, u]) => {
        const [c, r] = k.split(',').map(Number);
        const s = { line: u.line, stage: u.stage, uid: u.uid, items: u.items.slice(), c, r };
        if (u.bonusTraits) s.bonusTraits = u.bonusTraits;
        if (u.mods) s.mods = u.mods; // set data on a unit (Chosen, Power Ups...): combat units get it as u.mods
        return s;
      });
    }
  }

  // ---------- game hook context ----------
  // Hooks get an object whose prototype is this API with: game, player, and for traits lv, v, val(key), T.
  const GAME_API = {
    get stage() { return this.game.stage; },
    get round() { return this.game.round; },
    get R() { return this.game.R; },
    get pace() { return this.game.pace; },
    // Duos: this player's partner (a Player), or null in solo games
    get partner() { return this.player ? this.game.partnerOf(this.player) : null; },
    get format() { return this.game.format; },
    rng() { return this.game.rng(); },
    pick(arr) { return this.game.pick(arr); },
    shuffle(arr) { return this.game.shuffle(arr); },
    give(id) { this.game.giveItem(this.player, id); },
    randomComponent(withSpecial) { return this.game.randomComponent(withSpecial); },
    randomCompleted() { return this.game.randomCompleted(); },
    randomEmblem(prefer) { return this.game.randomCrest(prefer ? this.player : null); },
    giveUnit(tiers, stage = 0) { return this.game.giveUnit(this.player, tiers, stage); },
    refreshShop(levelBoost = 0) { this.game.refreshShop(this.player, levelBoost); },
    combine() { return this.game.combine(this.player); },
    val(key) { return this.T && this.T[key] ? this.T[key][this.lv - 1] : undefined; },
  };

  class Game {
    constructor(opts = {}) {
      this.seed = opts.seed == null ? (Math.random() * 1e9) | 0 : opts.seed;
      // the set: an id, 'chaos' (picked from the seed) or nothing (default set)
      this.setId = DT.Sets.resolve(opts.setId || DT.Sets.DEFAULT, this.seed);
      this.chaos = opts.setId === 'chaos';
      // the pace: 'hyper' | 'normal' | 'chaos' (picked from the seed) or nothing (the set's native pace)
      this.pace = DT.Sets.resolvePace(opts.pace, this.seed) || DT.Sets.nativePace(this.setId);
      this.chaosPace = opts.pace === 'chaos' || !!opts.chaosPace;
      this.set = DT.Sets.activate(this.setId, this.pace);
      this.R = this.set.rules;
      this.rng = DT.makeRng(this.seed);
      this.uid = 0;
      this.pool = {};
      for (const L of DT.SHOP_LINES) this.pool[L.id] = DT.POOL_SIZE[L.tier];
      this.maxTier = DT.SHOP_LINES.reduce((m, l) => Math.max(m, l.tier), 0);
      // opts.slots: 8 entries of { name, human } for multiplayer. Default: one human in slot 0.
      const slots = opts.slots || [{ name: opts.humanName || 'You', human: !opts.allAI }].concat(DT.TAMERS.slice(0, 7).map((n) => ({ name: n, human: false })));
      this.players = slots.map((s, i) => new Player(i, s.name, !!s.human, this));
      // format: 'solo' (everyone for themselves) or 'duos' (4 teams of 2, js/duos.js); old inits have none = solo
      this.format = DT.Duos && opts.format === 'duos' && this.players.length === 8 ? 'duos' : 'solo';
      this.duo = this.format === 'duos';
      if (this.duo) this.setupTeams(opts.teams);
      // the player this client controls and views; game rules never read it
      this.human = this.players.find((p) => p.isHuman) || this.players[0];
      this.stage = 1;
      this.round = 1;
      this.phase = 'planning';
      this.pairs = [];
      this.results = [];
      this.over = false;
      this.mech = {}; // game-wide state for the set mechanic (plain data; combat hooks read it as c.info.mech)
      this.fireGame('onGameStart', null, {});
      for (const p of this.players) this.refreshShop(p);
      this.roundType = this.typeOf(1, 1);
      this.isPvE = this.roundType === 'pve';
      if (this.roundType === 'carousel') this.startCarousel();
      else {
        for (const p of this.players) { this.makeOffer(p); this.fireGame('onRoundStart', p, {}); this.clearMaxedShop(p); }
        this.planAI();
      }
    }

    // point the content globals at this game's set (tests and tools may run games of several sets in one process)
    ensureSet() { if (DT.SET_ID !== this.setId || DT.SET !== this.set) DT.Sets.activate(this.setId, this.pace); }

    // ---- Duos: teams, shared HP, the portal ----
    setupTeams(teams) {
      this.D = DT.Duos.rules(this.R);
      this.teams = DT.Duos.validTeams(teams).map((ids, i) => ({ id: i, members: ids.map((s) => this.players[s]), alive: true, placement: 0, hp: this.D.teamHp }));
      for (const T of this.teams) for (const p of T.members) { p.team = T.id; p.hp = T.hp; }
    }
    // Team HP is T.hp, mirrored on both partners' p.hp. Set rules that heal or hurt one Tamer (a Heart augment, a
    // Wisp) change p.hp; syncTeams folds every such change into the team pool, so it counts for both partners.
    syncTeams() {
      if (!this.duo) return;
      for (const T of this.teams) {
        let h = T.hp;
        for (const p of T.members) h += p.hp - T.hp;
        T.hp = h;
        for (const p of T.members) p.hp = h;
      }
    }
    teamOf(p) { return this.duo && p && p.team != null ? this.teams[p.team] : null; }
    partnerOf(p) { const T = this.teamOf(p); return T ? T.members.find((q) => q !== p) || null : null; }
    aliveTeams() { return this.duo ? this.teams.filter((T) => T.alive) : []; }
    teamHp(T) { return T.hp; }
    // sends allowed per round: the pace's sends plus augments with portalSends (Wide Portal)
    portalLimit(p) {
      if (!this.duo) return 0;
      let n = this.D.sends;
      for (const a of p.augs) { const A = DT.AUGMENT[a]; if (A && A.portalSends) n += A.portalSends; }
      return n;
    }
    // can p send this bench unit / item index to their partner now? 'ok' or a reason
    canPortal(p, what) {
      if (!this.duo) return 'solo';
      const q = this.partnerOf(p);
      if (!q || !q.alive || !p.alive) return 'partner';
      if (this.phase !== 'planning' || this.roundType === 'carousel') return 'phase';
      if (p.portal >= this.portalLimit(p)) return 'limit';
      if (what.uid != null) {
        const f = this.findUnit(p, what.uid);
        if (!f || f.where !== 'bench') return 'bench';
        if (!DT.canSell(f.u)) return 'noSend';
        if (q.freeBenchSlot() >= 0) return 'ok';
        // a full bench still takes a copy that digivolves at once
        const same = q.allUnits().filter((u) => u.line === f.u.line && u.stage === f.u.stage).length;
        const maxStage = this.R.maxStage != null ? this.R.maxStage : 2;
        return f.u.stage < maxStage && same >= this.copiesOf(f.u.line) - 1 ? 'ok' : 'full';
      }
      if (what.item != null) {
        const id = p.items[what.item];
        if (!id) return 'none';
        return q.items.length < this.R.itemCap ? 'ok' : 'itemsFull';
      }
      return 'none';
    }
    // Send a bench Digimon ({ uid }) or an item ({ item: index }) to the partner. Returns { kind, evolved } or a reason.
    portal(p, what) {
      const why = this.canPortal(p, what);
      if (why !== 'ok') return why;
      const q = this.partnerOf(p);
      p.portal++;
      const e = { from: p, to: q, unit: null, item: null };
      let evolved = [];
      if (what.uid != null) {
        const f = this.findUnit(p, what.uid);
        p.bench[f.i] = null;
        e.unit = f.u;
        q.incoming.push({ kind: 'unit', from: p.id, uid: f.u.uid, line: f.u.line, stage: f.u.stage });
        evolved = this.combine(q, f.u);
      } else {
        const id = p.items.splice(what.item, 1)[0];
        e.item = id;
        q.items.push(id);
        q.incoming.push({ kind: 'item', from: p.id, item: id });
      }
      if (this.hasHook('onPortal', p)) this.fireGame('onPortal', p, e);
      if (this.hasHook('onPortal', q)) this.fireGame('onPortal', q, e);
      return { kind: e.unit ? 'unit' : 'item', evolved };
    }
    // a free unit for `to` from `from`'s augment (Care Package): through the portal, sends not counted
    portalGift(from, to, tiers) {
      const u = this.giveUnit(to, tiers, 0);
      if (!u) return null;
      if (to.bench.includes(u)) { to.incoming.push({ kind: 'unit', from: from.id, uid: u.uid, line: u.line, stage: u.stage, gift: true }); this.combine(to); }
      return u;
    }
    // team HP loss: both partners share it (Team Guard: 25% less, rounded up)
    damageTeam(T, d) {
      if (!d) return 0;
      if (T.members.some((p) => p.augs.includes('duo_guard'))) d = Math.ceil(d * 0.75);
      T.hp -= d;
      for (const p of T.members) p.hp = T.hp;
      return d;
    }
    // loss damage for a team fight: stage damage plus unitScale x per-unit damage of the whole winning side
    duoLoss(cb, winnerTeam) {
      const R = this.R;
      let d = R.lossDamage(this.stage);
      if (R.unitDamage && winnerTeam >= 0) {
        let u = 0;
        for (const x of cb.units) if (x.alive && x.team === winnerTeam && !x.summoned) u += R.unitDamage(x, this.stage);
        d += Math.round(u * this.D.unitScale);
      }
      return d;
    }
    // A team's side for a team fight: both boards side by side (lower seat on the left, cols 0-6 and 7-13), summed trait
    // counts, and each partner's augments, power, global items and mechanic state as a part acting for their own units.
    teamSide(T) {
      const units = [];
      const parts = [];
      T.members.forEach((m, k) => {
        const s = this.sideOf(m);
        for (const u of s.units) units.push({ ...u, c: u.c + k * COLS, owner: m.id });
        parts.push({ owner: m.id, augs: s.augs, power: s.power || null, globals: s.globals || [], mech: s.mech || null, buffs: s.buffs, partnerTop: s.partnerTop || 0 });
      });
      const counts = DT.Duos.teamCounts(T.members.map((m) => ({ owner: m.id, units: m.boardSide(), augs: m.augs })));
      const augs = [];
      for (const p of parts) for (const a of p.augs) if (!augs.includes(a)) augs.push(a);
      const side = { units, cols: COLS * 2, parts, counts, augs, buffs: null, owners: T.members.map((m) => m.id) };
      if (parts[0].power) side.power = parts[0].power;
      const globals = parts.flatMap((p) => p.globals);
      if (globals.length) side.globals = globals;
      const mech = DT.Duos.mergeMech(parts.map((p) => p.mech));
      if (mech) side.mech = mech;
      return side;
    }
    // the value (cost x copies) of the partner's best board Digimon, for Partner's Ace
    partnerTop(p) {
      const q = this.partnerOf(p);
      if (!q || !q.alive) return 0;
      return q.boardUnits().reduce((m, u) => Math.max(m, DT.unitValue(u)), 0);
    }

    get roundLabel() { return `${this.stage}-${this.round}`; }
    alivePlayers() { return this.players.filter((p) => p.alive); }
    newUnit(line, stage) { return { uid: ++this.uid, line, stage, items: [] }; }
    pick(arr) { return arr[Math.floor(this.rng() * arr.length)]; }
    shuffle(arr) {
      for (let i = arr.length - 1; i > 0; i--) {
        const j = Math.floor(this.rng() * (i + 1));
        [arr[i], arr[j]] = [arr[j], arr[i]];
      }
      return arr;
    }
    typeOf(stage, round) { return ((this && this.R) || DT.HYPER).roundType(stage, round); }
    // rounds played so far in this pace: 1 at 1-1 (sets that count rounds use this instead of (stage - 1) * n + round)
    roundNo(stage = this.stage, round = this.round) {
      let n = round;
      for (let s = 1; s < stage; s++) n += this.R.roundsInStage(s);
      return n;
    }
    copiesOf(line) { return copiesOf(line, this.R); }

    // ---- hooks ----
    ctx(p, extra) { return Object.assign(Object.create(GAME_API), { game: this, player: p }, extra || {}); }
    // listeners for a player: augments, Tamer power, global items, then the set mechanic / set hooks
    gameListeners(name, p) {
      const out = [];
      if (p) {
        for (const a of p.augs) { const A = DT.AUGMENT[a]; if (A && A.hooks && A.hooks[name]) out.push([A.hooks[name], { kind: 'augment', id: a, A }]); }
        if (p.power && DT.POWER[p.power.id]) { const P = DT.POWER[p.power.id]; if (P.hooks && P.hooks[name]) out.push([P.hooks[name], { kind: 'power', id: P.id, P, rank: p.power.rank }]); }
        for (const id of p.globals) { const it = DT.ITEMS[id]; if (it && it.hooks && it.hooks[name]) out.push([it.hooks[name], { kind: 'global', id, item: it }]); }
      }
      const S = this.set;
      if (S.hooks[name]) out.push([S.hooks[name], { kind: 'mechanic', id: (S.mechanic && S.mechanic.id) || S.id }]);
      return out;
    }
    hasHook(name, p) { return this.gameListeners(name, p).length > 0; }
    fireGame(name, p, e) {
      if (!GAME_HOOK.has(name)) return e;
      const list = this.gameListeners(name, p);
      if (list.length > 1) list.sort((a, b) => (typeof a[0] === 'object' && a[0].order || 0) - (typeof b[0] === 'object' && b[0].order || 0));
      for (const [h, extra] of list) DT.hookFn(h)(this.ctx(p, extra), e);
      return e;
    }
    // trait game hooks (onRoundEnd) for the traits active in counts
    fireTraits(name, p, counts, e) {
      for (const id in counts || {}) {
        const T = DT.TRAITS[id];
        if (!T || !T.hooks || !T.hooks[name]) continue;
        const lv = DT.traitLevel(id, counts[id]);
        if (!lv) continue;
        DT.hookFn(T.hooks[name])(this.ctx(p, { kind: 'trait', id, T, lv, n: counts[id], v: T.lv[lv - 1] }), e);
      }
      return e;
    }

    // ---- Pool & shop ----
    takeFromPool(lineId, n = 1) { this.pool[lineId] = Math.max(0, this.pool[lineId] - n); }
    returnToPool(unit) {
      if (DT.LINE[unit.line].shop) this.pool[unit.line] += DT.unitCopies(unit);
    }

    shopEligible(p, lineId) {
      return !p.allUnits().some((u) => u.line === lineId && u.stage >= 2);
    }

    // Release existing offers as soon as a player owns a three-star copy, including locked shops.
    clearMaxedShop(p) {
      for (let i = 0; i < p.shop.length; i++) {
        const id = p.shop[i];
        if (id && !this.shopEligible(p, id)) {
          this.pool[id] += 1;
          p.shop[i] = null;
          if (p.shopMeta) p.shopMeta[i] = null;
        }
      }
    }

    rollSlot(level, oddsOverride, p) {
      const table = DT.SHOP_ODDS;
      const odds = oddsOverride || table[Math.min(table.length - 1, level)];
      let roll = this.rng() * 100;
      let tier = 1;
      for (let i = 0; i < odds.length; i++) {
        if (roll < odds[i]) { tier = i + 1; break; }
        roll -= odds[i];
      }
      for (let t = tier; t >= 1; t--) {
        const lines = DT.SHOP_LINES.filter((l) => l.tier === t && this.pool[l.id] > 0 && (!p || this.shopEligible(p, l.id)));
        const total = lines.reduce((s, l) => s + this.pool[l.id], 0);
        if (!total) continue;
        let x = this.rng() * total;
        for (const l of lines) {
          x -= this.pool[l.id];
          if (x < 0) { this.takeFromPool(l.id); return l.id; }
        }
      }
      return null;
    }

    refreshShop(p, levelBoost = 0) {
      for (const id of p.shop) if (id) this.pool[id] += 1;
      p.shopMeta = null;
      let level = p.level + levelBoost;
      let odds = null;
      let size = p.shop.length;
      if (this.hasHook('onShop', p)) {
        const e = this.fireGame('onShop', p, { level, odds: null, size, boost: levelBoost });
        level = e.level; odds = e.odds; size = Math.max(1, Math.min(8, e.size | 0));
        if (size !== p.shop.length) p.shop = new Array(size).fill(null);
      }
      for (let i = 0; i < p.shop.length; i++) p.shop[i] = this.rollSlot(level, odds, p);
      if (this.hasHook('onShopRolled', p)) this.fireGame('onShopRolled', p, { shop: p.shop });
      this.clearMaxedShop(p);
    }

    reroll(p) {
      if (p.freeRolls > 0) p.freeRolls--;
      else if (p.gold < this.R.rerollCost) return false;
      else p.gold -= this.R.rerollCost;
      this.refreshShop(p);
      return true;
    }

    // price of a shop slot: the line's cost unless a mechanic set shopMeta[slot].cost
    shopCost(p, slot) {
      const id = p.shop[slot];
      if (!id) return 0;
      const m = p.shopMeta && p.shopMeta[slot];
      return m && m.cost != null ? m.cost : DT.LINE[id].tier;
    }

    // Board units are frozen while the human's combat is playing.
    boardLocked(p) { return this.phase === 'combat' && p.isHuman; }

    canBuy(p, slot) {
      const id = p.shop[slot];
      if (!id) return 'empty';
      if (!this.shopEligible(p, id)) return 'empty';
      if (p.gold < this.shopCost(p, slot)) return 'gold';
      if (p.freeBenchSlot() >= 0) return 'ok';
      const pool = this.boardLocked(p) ? p.bench.filter(Boolean) : p.allUnits();
      const copies = pool.filter((u) => u.line === id && u.stage === 0).length;
      return copies >= this.copiesOf(id) - 1 ? 'ok' : 'bench';
    }

    buy(p, slot) {
      if (this.canBuy(p, slot) !== 'ok') return null;
      const id = p.shop[slot];
      p.gold -= this.shopCost(p, slot);
      const meta = p.shopMeta && p.shopMeta[slot];
      p.shop[slot] = null;
      if (p.shopMeta) p.shopMeta[slot] = null;
      const unit = this.newUnit(id, 0);
      if (meta && meta.unit) Object.assign(unit, JSON.parse(JSON.stringify(meta.unit)));
      if (this.hasHook('onBuy', p)) this.fireGame('onBuy', p, { unit, slot });
      const free = p.freeBenchSlot();
      if (free >= 0) {
        p.bench[free] = unit;
        return this.combine(p);
      }
      return this.combine(p, unit);
    }

    // N copies of a form (3, or line.combine) digivolve into the next form. Items carry over (completed first, max 3).
    combine(p, extra = null) {
      const evolved = [];
      const allowBoard = !this.boardLocked(p);
      const maxStage = this.R.maxStage != null ? this.R.maxStage : 2;
      for (;;) {
        const entries = [];
        if (allowBoard) for (const [k, u] of Object.entries(p.board)) entries.push({ u, where: 'board', k });
        p.bench.forEach((u, i) => { if (u) entries.push({ u, where: 'bench', i }); });
        if (extra) entries.push({ u: extra, where: 'extra' });
        const groups = {};
        for (const e of entries) {
          if (e.u.stage >= maxStage) continue;
          const g = `${e.u.line}|${e.u.stage}`;
          (groups[g] = groups[g] || []).push(e);
        }
        const g = Object.values(groups).find((list) => list.length >= this.copiesOf(list[0].u.line));
        if (!g) break;
        const three = g.slice(0, this.copiesOf(g[0].u.line));
        const base = three[0].u;
        const up = this.newUnit(base.line, base.stage + 1);
        if (base.bonusTraits) up.bonusTraits = base.bonusTraits;
        if (base.mods) up.mods = base.mods;
        const items = three.flatMap((e) => e.u.items);
        items.sort((a, b) => DT.isComponent(a) - DT.isComponent(b));
        // These are owned items being returned, not new drops; keep overflow until equipped.
        up.items = DT.fitItems(items, (id) => p.items.push(id));
        for (const e of three) {
          if (e.where === 'board') delete p.board[e.k];
          else if (e.where === 'bench') p.bench[e.i] = null;
          else extra = null;
        }
        const anchor = three[0];
        if (anchor.where === 'board') p.board[anchor.k] = up;
        else if (anchor.where === 'bench') p.bench[anchor.i] = up;
        else p.bench[p.freeBenchSlot()] = up;
        evolved.push(up);
        if (this.hasHook('onCombine', p)) this.fireGame('onCombine', p, { unit: up, from: three.map((e) => e.u) });
        if (this.duo) {
          const q = this.partnerOf(p);
          if (q && q.alive && this.hasHook('onPartnerCombine', q)) this.fireGame('onPartnerCombine', q, { unit: up, partner: p });
        }
      }
      if (extra) {
        const free = p.freeBenchSlot();
        if (free >= 0) p.bench[free] = extra;
      }
      this.clearMaxedShop(p);
      return evolved;
    }

    findUnit(p, uid) {
      for (const [k, u] of Object.entries(p.board)) if (u.uid === uid) return { where: 'board', k, u };
      const i = p.bench.findIndex((u) => u && u.uid === uid);
      if (i >= 0) return { where: 'bench', i, u: p.bench[i] };
      return null;
    }

    sell(p, uid) {
      const f = this.findUnit(p, uid);
      if (!f || !DT.canSell(f.u)) return false;
      if (f.where === 'board' && this.boardLocked(p)) return false;
      if (f.where === 'board') delete p.board[f.k];
      else p.bench[f.i] = null;
      p.gold += DT.sellValue(f.u);
      for (const id of f.u.items) this.giveItem(p, id);
      this.returnToPool(f.u);
      if (this.hasHook('onSell', p)) this.fireGame('onSell', p, { unit: f.u });
      return true;
    }

    // set placement rules (canPlace hook): false keeps the unit where it was
    canPlace(p, unit, c, r) {
      if (!this.hasHook('canPlace', p)) return true;
      return this.fireGame('canPlace', p, { unit, c, r, ok: true }).ok !== false;
    }

    // from/to: {type:'board', c, r} | {type:'bench', i}
    move(p, from, to) {
      const get = (s) => (s.type === 'board' ? p.board[key(s.c, s.r)] : p.bench[s.i]) || null;
      const set = (s, u) => {
        if (s.type === 'board') { if (u) p.board[key(s.c, s.r)] = u; else delete p.board[key(s.c, s.r)]; }
        else p.bench[s.i] = u;
      };
      if (to.type === 'board' && (to.r < 4 || to.r > 7 || to.c < 0 || to.c >= COLS)) return false;
      if ((from.type === 'board' || to.type === 'board') && this.boardLocked(p)) return false;
      const a = get(from);
      const b = get(to);
      if (!a) return false;
      if (from.type === 'bench' && to.type === 'board') {
        // a swap with a board unit of the same size never changes the count
        const over = b ? DT.slotsOf(a) > DT.slotsOf(b) && p.boardCount() - DT.slotsOf(b) + DT.slotsOf(a) > p.cap() : p.boardCount() + DT.slotsOf(a) > p.cap();
        if (over) return 'cap';
      }
      if (to.type === 'board' && !this.canPlace(p, a, to.c, to.r)) return false;
      set(from, b);
      set(to, a);
      // moving a team-size item holder to the bench can drop the cap below the board count
      while (p.boardCount() > p.cap()) {
        const [k, u] = Object.entries(p.board).find(([, x]) => !teamSizeItems(x)) || Object.entries(p.board)[0];
        const free = p.freeBenchSlot();
        if (free < 0) break;
        delete p.board[k];
        p.bench[free] = u;
      }
      return true;
    }

    // ---- Items ----
    giveItem(p, id) {
      if (!id) return;
      if (p.items.length < this.R.itemCap) p.items.push(id);
    }

    // Swap two item bench pedestals. Items never combine on the bench (TFT rule), only on a Digimon.
    swapItems(p, a, b) {
      if (a === b || !p.items[a] || !p.items[b]) return false;
      [p.items[a], p.items[b]] = [p.items[b], p.items[a]];
      return true;
    }

    // Put inventory item idx on a unit. Components merge with a component the unit already holds.
    equipItem(p, idx, uid) {
      const id = p.items[idx];
      const f = this.findUnit(p, uid);
      if (!id || !f) return 'none';
      if (f.where === 'board' && this.boardLocked(p)) return 'locked';
      const u = f.u;
      const it = DT.ITEMS[id];
      if (!it) return 'none';
      if (it.consumable) return this.useConsumable(p, idx, u);
      if (it.cat === 'global') { p.items.splice(idx, 1); p.globals.push(id); return id; }
      if (it.emblem && DT.unitTraits(u).includes(it.emblem)) return 'hasTrait';
      if (!DT.holdsItems(u)) return 'cantHold';
      const ci = DT.isComponent(id) ? u.items.findIndex((x) => DT.isComponent(x) && DT.combineItems(x, id)) : -1;
      const veto = this.equipVeto(p, u, id, ci >= 0 ? DT.combineItems(u.items[ci], id) : null);
      if (veto) return veto;
      if (DT.isComponent(id)) {
        if (ci >= 0) {
          const made = DT.combineItems(u.items[ci], id);
          if (DT.ITEMS[made].emblem && DT.unitTraits({ ...u, items: u.items.filter((_, k) => k !== ci) }).includes(DT.ITEMS[made].emblem)) return 'hasTrait';
          u.items[ci] = made;
          p.items.splice(idx, 1);
          return made;
        }
      }
      if (u.items.length >= 3) return 'full';
      u.items.push(id);
      p.items.splice(idx, 1);
      return id;
    }

    // Set equip rules (canEquip hook): null if item id may go on unit u, else a result string ('full', 'cantHold'...).
    // combine: the item it would make with a held component, or null.
    equipVeto(p, u, id, combine = null) {
      if (this.hasHook('canEquip', p)) {
        const e = this.fireGame('canEquip', p, { unit: u, item: id, combine, ok: true, reason: null });
        if (e.ok === false) return e.reason || 'cantHold';
      }
      const ci = combine ? u.items.findIndex((held) => DT.isComponent(held) && DT.combineItems(held, id) === combine) : -1;
      return DT.itemsFit(u.items.filter((_, i) => i !== ci), combine || id) ? null : 'full';
    }
    // can u take item id from the bench as a new held item (AI)
    canHold(p, u, id) { return DT.holdsItems(u) && !this.equipVeto(p, u, id, null); }

    // Consumables (item.use(ctx, unit, idx) in the set's mech file) return a result string for the UI.
    useConsumable(p, idx, u) {
      const id = p.items[idx];
      const it = DT.ITEMS[id];
      const use = it.use || (it.hooks && DT.hookFn(it.hooks.onUse));
      if (!use) return 'none';
      return use(this.ctx(p, { kind: 'item', id, item: it }), u, idx);
    }

    randomComponent(withEgg = false) {
      const special = DT.SPECIAL_COMPONENTS || [];
      const list = withEgg && this.rng() < 0.1 && special.length ? special : DT.STAT_COMPONENTS;
      return list.length ? this.pick(list) : null;
    }
    randomCompleted() { return DT.CORE_ITEMS.length ? this.pick(DT.CORE_ITEMS) : null; }
    // any emblem; with p, prefer one for a non-role trait already on p's board
    randomCrest(p) {
      if (!DT.EMBLEM_IDS.length) return null;
      if (p) {
        const counts = DT.countTraits(p.boardUnits());
        const fit = DT.EMBLEM_IDS.filter((id) => counts[DT.ITEMS[id].emblem] && !['role', 'class'].includes(DT.TRAITS[DT.ITEMS[id].emblem].kind));
        if (fit.length) return this.pick(fit);
      }
      return this.pick(DT.EMBLEM_IDS);
    }
    // a random unit of one of these costs at this stage, onto the bench (gold if the bench is full)
    giveUnit(p, tiers, stage) {
      const lines = DT.SHOP_LINES.filter((l) => tiers.includes(l.tier) && this.pool[l.id] >= Math.pow(this.copiesOf(l.id), stage));
      if (!lines.length) return null;
      const l = this.pick(lines);
      this.takeFromPool(l.id, Math.pow(this.copiesOf(l.id), stage));
      const u = this.newUnit(l.id, stage);
      const free = p.freeBenchSlot();
      if (free >= 0) p.bench[free] = u;
      else { this.returnToPool(u); p.gold += DT.sellValue(u); }
      return u;
    }

    // ---- Carousel ----
    startCarousel() {
      const opts = [];
      const tiers = this.R.carouselTiers(this.stage);
      const low = Math.min(...tiers);
      const lines = DT.SHOP_LINES.filter((l) => tiers.includes(l.tier));
      for (let i = 0; i < 9; i++) {
        const avail = lines.filter((l) => this.pool[l.id] > 0);
        if (!avail.length) break;
        const l = this.pick(avail.filter((x) => x.tier === low).length && i < 7 ? avail.filter((x) => x.tier === low) : avail);
        this.takeFromPool(l.id);
        opts.push({ line: l.id, item: this.carouselItem(), by: null });
      }
      const order = this.shuffle(this.alivePlayers().slice());
      if (this.stage > 1) order.sort((a, b) => a.hp - b.hp); // later carousels: lowest HP picks first
      this.carousel = { opts, order, idx: 0 };
      this.carouselStep();
    }

    carouselItem() {
      const ci = this.R.carouselItems;
      if (typeof ci === 'function') return ci(this, this.stage);
      return ci === 'completed' ? this.randomCompleted() : this.randomComponent();
    }

    carouselTurn() {
      const c = this.carousel;
      return c && c.idx < c.order.length ? c.order[c.idx] : null;
    }

    // AIs pick until it's the human's turn or everyone has picked.
    carouselStep() {
      const c = this.carousel;
      while (c.idx < c.order.length && !c.order[c.idx].isHuman) {
        const p = c.order[c.idx];
        // score each option once: an rng call inside a sort comparator would differ between browser sort algorithms
        let best = -1;
        let bestScore = -Infinity;
        c.opts.forEach((o, i) => {
          if (o.by) return;
          const sc = this.carouselScore(p, o);
          if (sc > bestScore) { bestScore = sc; best = i; }
        });
        if (best < 0) { c.idx++; continue; }
        this.carouselPick(p, best);
      }
    }

    carouselScore(p, o) {
      const owned = p.allUnits().filter((u) => u.line === o.line).length;
      return DT.LINE[o.line].tier * 2 + owned * 3 + this.rng();
    }

    carouselPick(p, i) {
      const c = this.carousel;
      const o = c.opts[i];
      if (!o || o.by || this.carouselTurn() !== p) return false;
      o.by = p.name;
      const free = p.freeBenchSlot();
      if (free >= 0) p.bench[free] = this.newUnit(o.line, 0);
      else { p.gold += DT.LINE[o.line].tier; this.pool[o.line] += 1; }
      this.giveItem(p, o.item);
      c.idx++;
      this.combine(p);
      return true;
    }

    carouselDone() { return !this.carouselTurn(); }

    finishCarousel() {
      if (!this.carousel) return;
      while (this.carouselTurn()) {
        const p = this.carouselTurn();
        const free = this.carousel.opts.findIndex((o) => !o.by);
        if (free < 0) { this.carousel.idx++; continue; }
        this.carouselPick(p, free);
      }
      for (const o of this.carousel.opts) if (!o.by) this.pool[o.line] += 1;
      this.carousel = null;
    }

    // ---- Offers: Armory, augments, Tamer powers, tiered items, mechanic offers ----
    // what is offered at stage-round, or null
    offerAt(stage, round) {
      const R = this.R || DT.HYPER;
      for (const o of R.offers || []) if (o.stage === stage && o.round === round) return o;
      const label = `${stage}-${round}`;
      if (R.armoryRounds ? R.armoryRounds.includes(label) : stage >= 2 && round === (R.armoryRound || 1) && (R.armoryStages || []).includes(stage)) return { kind: 'armory' };
      if (R.augmentRounds ? R.augmentRounds.includes(label) : stage >= 2 && round === (R.augmentRound || 2) && (R.augmentStages || []).includes(stage)) return { kind: 'augment' };
      return null;
    }
    offerKindAt(stage, round) { const o = this.offerAt(stage, round); return o ? o.kind : null; }

    makeOffer(p) {
      p.offer = null;
      const o = this.offerAt(this.stage, this.round);
      if (!o) return;
      if (o.kind === 'power' && p.power) return; // one Tamer power per game
      const options = this.offerOptions(p, o);
      if (options && options.length) p.offer = { kind: o.kind, options, rerolls: o.rerolls != null ? o.rerolls : o.kind === 'augment' ? this.R.augmentRerolls : 0, tier: o.tier };
    }

    offerOptions(p, o, exclude = []) {
      switch (o.kind) {
        case 'armory': return this.armoryOptions(p);
        case 'augment': return this.augmentOptions(p, exclude);
        case 'power': return this.shuffle(DT.POWERS.filter((x) => !exclude.includes(x.id)).slice()).slice(0, o.count || 3).map((x) => x.id);
        case 'itemTier': {
          const pool = Object.values(DT.ITEMS).filter((it) => it.tier === o.tier && !it.component && !it.noDrop && !exclude.includes(it.id)).map((it) => it.id);
          return this.shuffle(pool).slice(0, o.count || 3);
        }
      }
      const K = this.set.mechanic.offerKinds[o.kind];
      return K && K.options ? K.options(this.ctx(p, { offer: o }), exclude) : null;
    }

    armoryOptions(p) {
      const custom = this.set.def.items && this.set.def.items.armory;
      if (custom) return custom(this.ctx(p));
      const out = new Set();
      for (let guard = 0; out.size < 3 && guard < 60; guard++) out.add(this.stage <= 3 ? this.randomComponent(true) : this.randomCompleted());
      out.delete(null);
      const list = [...out];
      if (this.stage >= 3 && this.rng() < 0.2 && DT.CONSUMABLE_IDS.length) list[2] = this.pick(DT.CONSUMABLE_IDS);
      if (this.stage >= 3 && this.rng() < 0.25) { const c = this.randomCrest(); if (c) list[1] = c; }
      return list.filter(Boolean);
    }

    // one tier per augment round for the whole lobby, like TFT
    augmentTier() {
      if (!this.augTier || this.augTier.stage !== this.stage) {
        const x = this.rng();
        const [a, b] = this.R.augmentTierOdds;
        this.augTier = { stage: this.stage, tier: x < a ? 1 : x < b ? 2 : 3 };
      }
      return this.augTier.tier;
    }

    augmentOptions(p, exclude = []) {
      const tier = this.augmentTier();
      const base = DT.AUGMENTS.filter((a) => !a.duo); // a set's `duo: true` augments only show up in Duos
      let pool = base.filter((a) => a.tier === tier && !p.augs.includes(a.id) && !exclude.includes(a.id));
      if (!pool.length) pool = base.filter((a) => !p.augs.includes(a.id) && !exclude.includes(a.id));
      const out = this.shuffle(pool.slice()).slice(0, 3).map((a) => a.id);
      // Duos: one of the three is a duo augment (shared ones from js/duos.js plus the set's own `duo: true` augments)
      if (this.duo) {
        const duoList = DT.Duos.AUGMENTS.concat(DT.AUGMENTS.filter((a) => a.duo && !DT.SHARED_AUGMENT[a.id]))
          .filter((a) => !p.augs.includes(a.id) && !exclude.includes(a.id));
        const fit = duoList.filter((a) => a.tier === tier);
        const pickFrom = fit.length ? fit : duoList;
        if (pickFrom.length) {
          const id = this.pick(pickFrom).id;
          if (out.length >= 3) out[2] = id; else out.push(id);
        }
      }
      return out;
    }

    rerollOffer(p) {
      if (!p.offer || p.offer.rerolls <= 0) return false;
      p.offer.rerolls--;
      const o = p.offer;
      o.options = o.kind === 'armory' ? this.armoryOptions(p) : this.offerOptions(p, { kind: o.kind, tier: o.tier }, o.options);
      return true;
    }

    chooseOffer(p, i) {
      const o = p.offer;
      if (!o || !o.options[i]) return null;
      const id = o.options[i];
      p.offer = null;
      if (o.kind === 'armory' || o.kind === 'itemTier') {
        if (DT.ITEMS[id] && DT.ITEMS[id].cat === 'global') p.globals.push(id);
        else this.giveItem(p, id);
      } else if (o.kind === 'augment') this.applyAugment(p, id);
      else if (o.kind === 'power') this.applyPower(p, id);
      else {
        const K = this.set.mechanic.offerKinds[o.kind];
        if (K && K.choose) K.choose(this.ctx(p, { offer: o }), id);
      }
      return id;
    }

    applyAugment(p, id) {
      p.augs.push(id);
      const A = DT.AUGMENT[id];
      if (A && A.hooks && A.hooks.onPick) DT.hookFn(A.hooks.onPick)(this.ctx(p, { kind: 'augment', id, A }), { id });
      if (this.set.hooks.onPick) DT.hookFn(this.set.hooks.onPick)(this.ctx(p, { kind: 'mechanic' }), { id, kind: 'augment' });
      this.combine(p);
    }

    applyPower(p, id) {
      const P = DT.POWER[id];
      if (!P) return;
      p.power = { id, rank: 1 };
      if (P.hooks && P.hooks.onPick) DT.hookFn(P.hooks.onPick)(this.ctx(p, { kind: 'power', id, P, rank: 1 }), { id });
      if (this.set.hooks.onPick) DT.hookFn(this.set.hooks.onPick)(this.ctx(p, { kind: 'mechanic' }), { id, kind: 'power' });
      this.combine(p);
    }

    // ---- XP (sets with rules.xp) ----
    addXp(p, n) {
      const X = this.R.xp;
      if (!X) return;
      const max = X.maxLevel || 9;
      p.xp += n;
      while (p.level < max && p.xp >= (DT.XP_NEXT[p.level] || Infinity)) { p.xp -= DT.XP_NEXT[p.level]; p.level++; }
      if (p.level >= max) p.xp = 0;
    }
    buyXp(p) {
      const X = this.R.xp;
      if (!X || p.level >= (X.maxLevel || 9) || p.gold < (X.buyCost || 4)) return false;
      p.gold -= X.buyCost || 4;
      this.addXp(p, X.buyXp || 4);
      return true;
    }

    // ---- Positioning helpers ----
    isFront(unit) { return DT.LINE[unit.line].ai.place === 'front'; }

    freeCellFor(p, unit) {
      const place = DT.LINE[unit.line].ai.place;
      const rows = place === 'flank' ? [7, 6] : place === 'front' ? [4, 5, 6, 7] : [7, 6, 5, 4];
      const cols = place === 'flank' ? [0, 6, 1, 5, 2, 4, 3] : FRONT_COLS;
      for (const r of rows) for (const c of cols) if (!p.board[key(c, r)] && this.canPlace(p, unit, c, r)) return { c, r };
      return null;
    }

    autoFill(p) {
      while (p.boardCount() < p.cap()) {
        let best = -1;
        p.bench.forEach((u, i) => { if (u && p.boardCount() + DT.slotsOf(u) <= p.cap() && (best < 0 || DT.unitValue(u) > DT.unitValue(p.bench[best]))) best = i; });
        if (best < 0) break;
        const cell = this.freeCellFor(p, p.bench[best]);
        if (!cell) break;
        p.board[key(cell.c, cell.r)] = p.bench[best];
        p.bench[best] = null;
      }
    }

    // ---- AI ----
    // How much a line's traits are worth to a board with these counts: progress toward breakpoints, a bonus for
    // landing exactly on one, and each trait's own ai.bonus({ active, stage, n, counts }).
    aiTraitValue(lineId, counts, stage) {
      const L = DT.LINE[lineId];
      let s = 0;
      const active = Object.keys(counts).filter((id) => DT.TRAITS[id] && DT.traitLevel(id, counts[id]) > 0).length;
      for (const t of L.traits) {
        const T = DT.TRAITS[t];
        if (!T) continue;
        const n = counts[t] || 0;
        s += n * 3;
        if (T.bp.includes(n)) s += 3;
        if (T.bp.includes(n + 1) && n > 0) s += 2;
        if (T.ai && T.ai.bonus) s += T.ai.bonus({ active, stage, n, counts, game: this });
      }
      return s + ((L.ai && L.ai.value) || 0);
    }

    aiScore(p, unit, traitCounts) {
      let s = DT.unitValue(unit) * 10 + unit.items.length * 6;
      s += this.aiTraitValue(unit.line, traitCounts, this.stage);
      return s;
    }

    aiWants(p, lineId) {
      const owned = p.allUnits();
      if (owned.some((u) => u.line === lineId && u.stage < 2)) return true;
      const counts = DT.countTraits(p.boardUnits());
      const L = DT.LINE[lineId];
      if (L.traits.some((t) => (counts[t] || 0) >= 2)) return true;
      // one short of a breakpoint
      if (L.traits.some((t) => (counts[t] || 0) >= 1 && DT.TRAITS[t] && DT.TRAITS[t].bp.includes((counts[t] || 0) + 1))) return true;
      if (L.tier === this.maxTier && p.gold >= 8) return true;
      // the line's own AI value (ai.value) stands in for trait progress, so single-trait units get bought too
      if (((L.ai && L.ai.value) || 0) >= 6) return true;
      // Duos: buy copies the partner is collecting, to send them through the portal
      if (this.duo && this.aiPartnerWants(p, lineId, 0) >= 2) return true;
      return owned.length < p.cap() + 2;
    }

    // Duos: how many copies of this form the partner holds, when p still has a portal send this round (else 0)
    aiPartnerWants(p, lineId, stage) {
      const q = this.partnerOf(p);
      if (!q || !q.alive || p.portal >= this.portalLimit(p)) return 0;
      return q.allUnits().filter((u) => u.line === lineId && u.stage === stage).length;
    }

    // trait counts the AI arranges by: its own units, plus its partner's board in Duos (team fights sum both boards,
    // so the AI leans toward traits that add up with its partner's)
    aiCounts(p, units) {
      const counts = DT.countTraits(units);
      if (!this.duo) return counts;
      const q = this.partnerOf(p);
      if (!q || !q.alive) return counts;
      const theirs = DT.countTraits(q.boardUnits());
      for (const t in theirs) counts[t] = (counts[t] || 0) + theirs[t];
      return counts;
    }

    // Duos AI: send bench Digimon the partner is collecting (first the ones that digivolve at once), and items when
    // its own item bench is crowded and the partner's is not
    aiPortal(p) {
      if (!this.duo) return;
      const q = this.partnerOf(p);
      if (!q || !q.alive) return;
      const count = (who, u) => who.allUnits().filter((x) => x.line === u.line && x.stage === u.stage).length;
      for (let guard = 0; guard < 4 && p.portal < this.portalLimit(p); guard++) {
        let best = null;
        let bestScore = 0;
        for (const u of p.bench) {
          if (!u || this.canPortal(p, { uid: u.uid }) !== 'ok') continue;
          const qc = count(q, u);
          const pc = count(p, u);
          let sc = 0;
          if (qc >= this.copiesOf(u.line) - 1) sc = 10 + u.stage * 5 + DT.LINE[u.line].tier;
          else if (qc > pc) sc = 3 + qc + DT.LINE[u.line].tier * 0.1;
          if (sc > bestScore) { bestScore = sc; best = u; }
        }
        if (best) { this.portal(p, { uid: best.uid }); continue; }
        if (p.items.length >= 7 && q.items.length <= 4) {
          const i = p.items.findIndex((id) => DT.isComponent(id));
          if (i >= 0 && this.canPortal(p, { item: i }) === 'ok') { this.portal(p, { item: i }); continue; }
        }
        break;
      }
    }

    aiBuyPass(p) {
      for (let i = 0; i < p.shop.length; i++) {
        const id = p.shop[i];
        if (!id || !this.aiWants(p, id)) continue;
        if (this.canBuy(p, i) === 'ok') this.buy(p, i);
        else if (this.canBuy(p, i) === 'bench') {
          this.aiSellWorst(p);
          if (this.canBuy(p, i) === 'ok') this.buy(p, i);
        }
      }
    }

    aiSellWorst(p) {
      const counts = {};
      for (const u of p.allUnits()) counts[`${u.line}|${u.stage}`] = (counts[`${u.line}|${u.stage}`] || 0) + 1;
      const worth = (u) => DT.unitValue(u) + (counts[`${u.line}|${u.stage}`] >= 2 ? 100 : 0) + u.items.length * 50;
      let worst = -1;
      p.bench.forEach((u, i) => { if (u && DT.canSell(u) && (worst < 0 || worth(u) < worth(p.bench[worst]))) worst = i; });
      return worst >= 0 && this.sell(p, p.bench[worst].uid);
    }

    // XP sets: buy XP toward a level target for the stage (stage + 2, +1 from stage 4)
    aiLevel(p) {
      const X = this.R.xp;
      if (!X) return;
      const want = this.R.aiLevelTarget ? this.R.aiLevelTarget(this.stage, this.round) : this.stage + 2 + (this.stage >= 4 ? 1 : 0);
      const target = Math.min(X.maxLevel || 9, want);
      let guard = 0;
      while (guard++ < 20 && p.level < target && p.gold >= (X.buyCost || 4)) this.buyXp(p);
    }
    // gold the AI keeps when rolling: Hyper Roll keeps 2 in stage 2; interest sets save up early
    aiKeep() {
      if (this.R.interest) return this.stage <= 3 ? Math.min(50, 10 * (this.stage + 1)) : this.stage <= 5 ? 20 : 0;
      return this.stage <= 2 ? 2 : 0;
    }

    aiArrange(p) {
      const units = p.allUnits();
      p.board = {};
      p.bench = new Array(BENCH).fill(null);
      const counts = this.aiCounts(p, units);
      units.sort((a, b) => this.aiScore(p, b, counts) - this.aiScore(p, a, counts));
      // team-size item holders always field, since they raise the cap
      units.sort((a, b) => (teamSizeItems(b) > 0) - (teamSizeItems(a) > 0));
      const base = (this.R.teamSize ? this.R.teamSize(p.level) : p.level) + p.teamBonus;
      const cap = base + units.slice(0, base).reduce((n, u) => n + teamSizeItems(u), 0);
      const onBoard = [];
      const rest = [];
      let used = 0;
      for (const u of units) {
        const s = DT.slotsOf(u);
        if (used + s <= cap && (s === 1 || used < cap)) { onBoard.push(u); used += s; } else rest.push(u);
      }
      onBoard.sort((a, b) => (this.isFront(b) ? 1 : 0) - (this.isFront(a) ? 1 : 0));
      for (const u of onBoard) {
        const cell = this.freeCellFor(p, u);
        if (cell) p.board[key(cell.c, cell.r)] = u;
        else rest.push(u);
      }
      // units that cannot be sold keep a bench slot first (stable sort: no change for sets without them)
      rest.sort((a, b) => !DT.canSell(b) - !DT.canSell(a));
      rest.forEach((u, i) => {
        if (i < BENCH) p.bench[i] = u;
        else { p.gold += DT.sellValue(u); for (const id of u.items) this.giveItem(p, id); this.returnToPool(u); }
      });
    }

    // ---- AI items ----
    // Main carry, second carry, tank, from each line's ai hints (place, tank, carry, dmg).
    aiHolders(p) {
      const board = p.boardUnits().filter((u) => DT.holdsItems(u)).sort((a, b) => DT.unitValue(b) - DT.unitValue(a) || b.items.length - a.items.length);
      const ai = (u) => DT.LINE[u.line].ai;
      // keep feeding units that already hold items, so builds finish instead of spreading
      const carries = board.filter((u) => ai(u).carry).sort((a, b) => b.items.filter((i) => !DT.isComponent(i)).length - a.items.filter((i) => !DT.isComponent(i)).length || DT.unitValue(b) - DT.unitValue(a));
      const tank = board.filter((u) => ai(u).tank === 1)[0] || board.filter((u) => ai(u).tank === 2)[0] ||
        board.filter((u) => ai(u).tank === 3 && !carries.slice(0, 2).includes(u))[0];
      const out = carries.slice(0, 2).map((u) => ({ u, kind: ai(u).dmg === 'ap' ? 'ap' : 'ad' }));
      if (tank) out.push({ u: tank, kind: 'tank' });
      return out;
    }

    countIn(list, id) { return list.filter((x) => x === id).length; }

    // Can this inventory build item id? Returns the two component indexes or null.
    recipeFrom(inv, id) {
      const it = DT.ITEMS[id];
      if (!it || !it.from) return null;
      const [a, b] = it.from;
      const i = inv.indexOf(a);
      if (i < 0) return null;
      const j = inv.findIndex((x, k) => x === b && k !== i);
      return j < 0 ? null : [i, j];
    }

    craft(p, id) {
      const ij = this.recipeFrom(p.items, id);
      if (!ij) return false;
      p.items = p.items.filter((_, k) => !ij.includes(k));
      p.items.push(id);
      return true;
    }

    giveFromBench(p, id, u) {
      const i = p.items.indexOf(id);
      if (i < 0 || !this.canHold(p, u, id)) return false;
      p.items.splice(i, 1);
      u.items.push(id);
      return true;
    }

    aiItems(p) {
      const holders = this.aiHolders(p);
      if (!holders.length) return;
      const counts = DT.countTraits(p.boardUnits(), p.augs);
      const nearBreak = (t) => DT.TRAITS[t].bp.some((b) => (counts[t] || 0) === b - 1);
      const board = p.boardUnits();
      const role = (kind) => DT.ITEM_ROLE[kind] || [];

      // 1. completed items already on the bench
      for (const id of p.items.slice()) {
        const it = DT.ITEMS[id];
        if (!it || it.component || it.consumable) continue;
        if (it.cat === 'global') { const i = p.items.indexOf(id); p.items.splice(i, 1); p.globals.push(id); continue; }
        if (it.emblem) {
          const t = it.emblem;
          const crowded = p.items.length >= 6 || this.stage >= 5;
          if (!(counts[t] >= 1) && !crowded) continue;
          const carriers = holders.slice(0, 2).map((h) => h.u);
          const cands = board.filter((x) => DT.itemsFit(x.items, id) && !DT.unitTraits(x).includes(t))
            .sort((a, b) => carriers.includes(a) - carriers.includes(b) || a.items.length - b.items.length);
          if (cands[0]) { this.giveFromBench(p, id, cands[0]); counts[t] = (counts[t] || 0) + 1; }
          continue;
        }
        if (it.p && it.p.teamSize) {
          const u = (holders.find((h) => h.kind === 'tank') || holders[0]).u;
          if (DT.itemsFit(u.items, id)) this.giveFromBench(p, id, u);
          continue;
        }
        // off-role items wait on the bench until it gets crowded
        const h = holders.find((x) => DT.itemsFit(x.u.items, id) && role(x.kind).includes(id)) ||
          (p.items.length >= 6 || this.stage >= 5 ? holders.find((x) => DT.itemsFit(x.u.items, id)) : null);
        if (h) this.giveFromBench(p, id, h.u);
      }

      // 2. build each holder's best items from components
      for (const h of holders) {
        for (const want of role(h.kind)) {
          if (DT.itemSlotsUsed(h.u.items) >= 3) break;
          if (h.u.items.includes(want) || !DT.itemsFit(h.u.items, want)) continue;
          if (this.craft(p, want)) this.giveFromBench(p, want, h.u);
        }
      }

      // 3. special components (Digi-Egg): two make their double recipe; one + a component makes an emblem that reaches a breakpoint
      for (const sc of DT.SPECIAL_COMPONENTS || []) {
        const dbl = DT.combineItems(sc, sc);
        while (dbl && this.countIn(p.items, sc) >= 2) { this.craft(p, dbl); this.aiItems(p); return; }
        if (p.items.includes(sc)) {
          for (const c of DT.STAT_COMPONENTS) {
            const em = DT.combineItems(sc, c);
            const t = em && DT.ITEMS[em].emblem;
            if (!t) continue;
            const u = board.find((x) => DT.itemsFit(x.items, em) && !DT.unitTraits(x).includes(t));
            if (p.items.includes(c) && nearBreak(t) && u && this.craft(p, em)) { this.giveFromBench(p, em, u); break; }
          }
        }
      }

      // 4. slam leftovers when the bench is crowded or the game is late
      const comps = () => p.items.filter((x) => DT.STAT_COMPONENTS.includes(x));
      while (comps().length >= 2 && (comps().length >= 4 || this.stage >= 4)) {
        const cs = comps();
        let made = null;
        for (const h of holders) {
          if (DT.itemSlotsUsed(h.u.items) >= 3) continue;
          made = role(h.kind).find((id) => DT.itemsFit(h.u.items, id) && this.recipeFrom(p.items, id));
          if (made) { this.craft(p, made); this.giveFromBench(p, made, h.u); break; }
        }
        if (!made) {
          const id = DT.combineItems(cs[0], cs[1]);
          if (!id || !this.craft(p, id)) break;
          const h = holders.find((x) => DT.itemsFit(x.u.items, id));
          if (!h) break;
          this.giveFromBench(p, id, h.u);
        }
      }

      // 5. sets with equip rules (canEquip, e.g. one item per unit): put finished items still on the bench on the
      // most valuable board units that can take them
      if (this.hasHook('canEquip', p)) {
        const takers = () => p.boardUnits().filter((u) => DT.holdsItems(u)).sort((a, b) => DT.unitValue(b) - DT.unitValue(a) || a.uid - b.uid);
        for (const id of p.items.slice()) {
          const it = DT.ITEMS[id];
          if (!it || it.component || it.consumable || it.cat === 'global') continue;
          const u = takers().find((x) => this.canHold(p, x, id) && !(it.emblem && DT.unitTraits(x).includes(it.emblem)));
          if (u) this.giveFromBench(p, id, u);
        }
      }
    }

    // Armory: take what the carries want. Augments: economy early, combat later, items if holding many.
    // Works from metadata only: item tags / ITEM_ROLE, augment tags ('econ' | 'items' | 'combat') and ai.bonus().
    aiOffer(p) {
      const o = p.offer;
      if (!o) return;
      let best = 0;
      let bestScore = -Infinity;
      const holders = this.aiHolders(p);
      o.options.forEach((id, i) => {
        let sc = this.rng();
        if (o.kind === 'armory' || o.kind === 'itemTier') {
          for (const h of holders) {
            const list = DT.ITEM_ROLE[h.kind] || [];
            if (list.includes(id)) sc += 10 - list.indexOf(id) * 0.5;
            if (DT.isComponent(id)) sc += list.filter((x) => DT.ITEMS[x] && (DT.ITEMS[x].from || []).includes(id) && DT.ITEMS[x].from.some((c) => c !== id && p.items.includes(c))).length * 2;
          }
          if (DT.isConsumable(id)) sc -= 3;
          const it = DT.ITEMS[id];
          if (it && it.emblem) {
            const t = it.emblem;
            const n = DT.countTraits(p.boardUnits())[t] || 0;
            sc += DT.TRAITS[t].bp.includes(n + 1) ? 9 : n ? 5 : 1;
          }
          if (it && it.ai && it.ai.bonus) sc += it.ai.bonus(this.ctx(p));
        } else if (o.kind === 'augment' || o.kind === 'power') {
          const a = o.kind === 'augment' ? DT.AUGMENT[id] : DT.POWER[id];
          const tags = (a && a.tags) || [];
          if (tags.includes('econ')) sc += this.stage <= 2 ? 3 : 0.5;
          else if (tags.includes('items')) sc += 2;
          else sc += this.stage >= 3 ? 3 : 1.5;
          if (a && a.ai && a.ai.bonus) sc += a.ai.bonus(this.ctx(p));
          sc += ((a && a.tier) || 1) * 0.1;
        } else {
          const K = this.set.mechanic.offerKinds[o.kind];
          if (K && K.aiScore) sc += K.aiScore(this.ctx(p, { offer: o }), id);
        }
        if (sc > bestScore) { bestScore = sc; best = i; }
      });
      this.chooseOffer(p, best);
    }

    aiTurn(p) {
      this.aiOffer(p);
      this.fireGame('onAiTurn', p, {});
      if (this.R.xp) this.aiLevel(p);
      this.aiBuyPass(p);
      let rolls = 0;
      const keep = this.aiKeep();
      while ((p.freeRolls > 0 || p.gold - this.R.rerollCost >= keep) && rolls < 15) {
        this.reroll(p);
        this.aiBuyPass(p);
        rolls++;
      }
      if (this.duo) this.aiPortal(p);
      while (p.benchCount() > 6) if (!this.aiSellWorst(p)) break;
      this.aiArrange(p);
      this.aiItems(p);
      this.aiArrange(p);
    }

    planAI() {
      this.ensureSet();
      for (const p of this.alivePlayers()) if (!p.isHuman) this.aiTurn(p);
      this.makePairs();
      if (this.duo) this.syncTeams();
    }

    // Duos: the alive teams pair up (a team with a human avoids its last opponent team); an odd team out fights a ghost
    // copy of a random other team. this.pairs keeps [lead A, lead B, ghost] for code that only needs the first players.
    makeTeamPairs() {
      this.duoPairs = [];
      this.pairs = [];
      if (this.roundType !== 'pvp') return;
      const alive = this.shuffle(this.aliveTeams().slice());
      const humanTeam = (T) => T.members.some((p) => p.isHuman);
      for (const T of alive.filter(humanTeam)) {
        const hi = alive.indexOf(T);
        if (alive.length <= 2) break;
        const oi = hi % 2 === 0 ? hi + 1 : hi - 1;
        if (alive[oi] && alive[oi].id === T.lastOpponent) {
          const swap = alive.findIndex((x, k) => k !== hi && k !== oi && Math.floor(k / 2) !== Math.floor(hi / 2));
          if (swap >= 0) [alive[oi], alive[swap]] = [alive[swap], alive[oi]];
        }
      }
      for (let i = 0; i + 1 < alive.length; i += 2) this.duoPairs.push([alive[i], alive[i + 1], false]);
      if (alive.length % 2 === 1) {
        const last = alive[alive.length - 1];
        this.duoPairs.push([last, this.pick(alive.filter((T) => T !== last)), true]);
      }
      this.pairs = this.duoPairs.map(([A, B, ghost]) => [A.members[0], B.members[0], ghost]);
    }

    // Duos: p's opposing team this round (or null): { team, ghost, players }
    opposingTeam(p) {
      const T = this.teamOf(p);
      if (!T || !this.duoPairs) return null;
      for (const [A, B, ghost] of this.duoPairs) {
        if (A === T) return { team: B, ghost, players: B.members };
        if (B === T && !ghost) return { team: A, ghost: false, players: A.members };
      }
      return null;
    }

    makePairs() {
      if (this.duo) return this.makeTeamPairs();
      this.pairs = [];
      if (this.roundType !== 'pvp') return;
      const alive = this.shuffle(this.alivePlayers().slice());
      // avoid giving a human the same opponent twice in a row
      for (const h of alive.filter((p) => p.isHuman)) {
        const hi = alive.indexOf(h);
        if (alive.length <= 2) break;
        const oi = hi % 2 === 0 ? hi + 1 : hi - 1;
        if (alive[oi] && alive[oi] === h.lastOpponent) {
          const swap = alive.findIndex((p, k) => k !== hi && k !== oi && Math.floor(k / 2) !== Math.floor(hi / 2));
          if (swap >= 0) [alive[oi], alive[swap]] = [alive[swap], alive[oi]];
        }
      }
      for (let i = 0; i + 1 < alive.length; i += 2) this.pairs.push([alive[i], alive[i + 1], false]);
      if (alive.length % 2 === 1) {
        const last = alive[alive.length - 1];
        const ghost = this.pick(alive.filter((p) => p !== last));
        this.pairs.push([last, ghost, true]);
      }
    }

    // p's opponent this round: { player, ghost } (Duos adds team and players: the opposing team and both its players;
    // player is the one standing across from p in the team fight)
    opponentOf(p) {
      if (this.duo) {
        const o = this.opposingTeam(p);
        if (!o) return null;
        const T = this.teamOf(p);
        const k = T.members.indexOf(p);
        const across = o.players[1 - k] || o.players[0];
        return { player: across, ghost: o.ghost, team: o.team, players: o.players };
      }
      for (const [a, b, ghost] of this.pairs) {
        if (a === p) return { player: b, ghost };
        if (b === p && !ghost) return { player: a, ghost: false };
      }
      return null;
    }

    // ---- Combat phase ----
    pveSide() {
      return { units: DT.pveWave(this.stage, this.round, this).map(([line, stage, c, r]) => ({ line, stage, c, r, uid: -1 })), noTraits: true };
    }

    sideOf(p) {
      const s = { units: p.boardSide(), augs: p.augs, buffs: p.nextBuffs };
      if (p.power) s.power = p.power;
      if (p.globals.length) s.globals = p.globals;
      if (Object.keys(p.mech).length) s.mech = p.mech;
      if (this.duo) s.partnerTop = this.partnerTop(p);
      return s;
    }
    // what combat hooks see of the game (c.info): stage, round, round type and the mechanic's game state
    combatInfo() {
      const T = this.R.combatTime;
      return { stage: this.stage, round: this.round, type: this.roundType, mech: this.mech, pace: this.pace,
        overtime: T && T.overtime, timeLimit: T && T.limit };
    }

    startCombat() {
      this.ensureSet();
      this.matches = [];
      this.humanMatch = null;
      if (this.roundType === 'carousel') { this.finishCarousel(); this.phase = 'combat'; return null; }
      for (const p of this.alivePlayers()) {
        if (p.offer) this.aiOffer(p);
        this.collectAllOrbs(p);
        this.autoFill(p);
      }
      if (this.duo) this.syncTeams();
      this.phase = 'combat';
      const seed = () => (this.rng() * 1e9) | 0;
      const info = this.combatInfo();
      if (this.isPvE) {
        for (const p of this.alivePlayers()) this.matches.push({ a: p, b: null, pve: true, sideA: this.sideOf(p), sideB: this.pveSide(), seed: seed(), info });
      } else if (this.duo) {
        // team fights: a / a2 and b / b2 are the two teams' players (lower seat first), ta / tb the team indexes
        for (const [A, B, ghost] of this.duoPairs) {
          this.matches.push({ duo: true, a: A.members[0], a2: A.members[1], b: B.members[0], b2: B.members[1], ta: A.id, tb: B.id, ghost,
            sideA: this.teamSide(A), sideB: this.teamSide(B), seed: seed(), info });
        }
      } else {
        for (const [a, b, ghost] of this.pairs) this.matches.push({ a, b, ghost, sideA: this.sideOf(a), sideB: this.sideOf(b), seed: seed(), info });
      }
      // Side A is always team 0. Matches with a human are recorded so they can be watched;
      // a viewer on side B sees the board mirrored (renderer.mirror).
      for (const m of this.matches) {
        const watched = this.matchPlayers(m).some((p) => p.isHuman);
        m.combat = watched ? new DT.Combat(m.sideA, m.sideB, { seed: m.seed, record: true, info }) : DT.simulate(m.sideA, m.sideB, m.seed, info);
      }
      this.humanMatch = this.matchOf(this.human);
      return this.humanMatch;
    }

    // the players who fight in match m (a ghost copy's owners excluded)
    matchPlayers(m) {
      const out = [m.a];
      if (m.a2) out.push(m.a2);
      if (m.b && !m.ghost) { out.push(m.b); if (m.b2) out.push(m.b2); }
      return out;
    }

    // p's match this round, with team = which combat team p is on
    matchOf(p) {
      if (!p || !p.alive || !this.matches) return null;
      for (const m of this.matches) {
        if (m.a === p || (m.a2 && m.a2 === p)) return { ...m, team: 0 };
        if ((m.b === p || (m.b2 && m.b2 === p)) && !m.ghost) return { ...m, team: 1 };
      }
      return null;
    }

    // HP a loser loses: rules.lossDamage(stage) (+ rules.unitDamage per surviving enemy unit)
    lossFor(cb, winnerTeam, pve) {
      const R = this.R;
      let d = pve && R.pveLossDamage ? R.pveLossDamage(this.stage) : null;
      if (d == null) d = R.lossDamage(this.stage);
      if (R.unitDamage && winnerTeam >= 0) for (const u of cb.units) if (u.alive && u.team === winnerTeam && !u.summoned) d += R.unitDamage(u, this.stage);
      return d;
    }

    finishCombat() {
      this.ensureSet();
      for (const m of this.matches) {
        const cb = m.combat;
        while (!cb.done) cb.step(DT.TICK);
      }
      this.results = [];
      if (this.duo) return this.finishDuoCombat();
      for (const m of this.matches) {
        const cb = m.combat;
        const w = cb.winner; // 0 = m.a won
        const res = { match: m, aWon: w === 0, bWon: w === 1, dmgA: 0, dmgB: 0, reward: null };
        if (m.pve) {
          if (w !== 0) res.dmgA = this.lossFor(cb, 1, true);
          if (w === 0 && this.R.winGoldPvE) m.a.gold += this.R.winGoldPvE;
          res.reward = this.pveReward(m.a, w === 0);
        } else {
          if (w !== 0) res.dmgA = this.lossFor(cb, 1, false);
          if (w !== 1) res.dmgB = this.lossFor(cb, 0, false);
          if (w === 0) m.a.gold += this.R.winGold;
          if (w === 1 && !m.ghost) m.b.gold += this.R.winGold;
          this.applyStreak(m.a, w === 0);
          if (!m.ghost) this.applyStreak(m.b, w === 1);
          m.a.lastOpponent = m.b;
          if (!m.ghost) m.b.lastOpponent = m.a;
        }
        res.prankA = this.roundEnd(m.a, cb.traitCounts[0], { won: w === 0, pve: !!m.pve, match: m, team: 0 });
        if (m.b && !m.ghost) res.prankB = this.roundEnd(m.b, cb.traitCounts[1], { won: w === 1, pve: false, match: m, team: 1 });
        m.a.hp -= res.dmgA;
        if (m.b && !m.ghost) m.b.hp -= res.dmgB;
        else res.dmgB = 0;
        this.results.push(res);
      }
      for (const p of this.players) { p.nextBuffs = null; this.combine(p); }
      this.eliminate();
      this.phase = 'result';
      return this.results;
    }

    // Duos round end. Team fights: the losing team loses duoLoss HP once (shared), each winner gets win gold and streaks
    // move per player. Wild rounds stay individual; a team loses the larger of its two players' wild-round damages.
    finishDuoCombat() {
      const teamDmg = new Map();
      const hit = (T, d) => { if (d > 0) teamDmg.set(T, Math.max(teamDmg.get(T) || 0, d)); };
      for (const m of this.matches) {
        const cb = m.combat;
        const w = cb.winner;
        const res = { match: m, aWon: w === 0, bWon: w === 1, dmgA: 0, dmgB: 0, reward: null };
        if (m.pve) {
          if (w !== 0) res.dmgA = this.lossFor(cb, 1, true);
          if (w === 0 && this.R.winGoldPvE) m.a.gold += this.R.winGoldPvE;
          res.reward = this.pveReward(m.a, w === 0);
          res.prankA = this.roundEnd(m.a, cb.traitCounts[0], { won: w === 0, pve: true, match: m, team: 0 });
          hit(this.teamOf(m.a), res.dmgA);
        } else {
          const TA = this.teams[m.ta];
          const TB = this.teams[m.tb];
          if (w !== 0) res.dmgA = this.duoLoss(cb, 1);
          if (w !== 1 && !m.ghost) res.dmgB = this.duoLoss(cb, 0);
          const sides = [[TA, 0, w === 0]].concat(m.ghost ? [] : [[TB, 1, w === 1]]);
          for (const [T, team, won] of sides) {
            T.lastOpponent = (team === 0 ? TB : TA).id;
            T.members.forEach((p, k) => {
              if (won) p.gold += this.R.winGold;
              this.applyStreak(p, won);
              p.lastOpponent = team === 0 ? TB.members[Math.min(1 - k, TB.members.length - 1)] : TA.members[Math.min(1 - k, TA.members.length - 1)];
              // each partner's round-end hooks see only their own units of the fight
              const own = Object.create(cb);
              own.units = cb.units.filter((u) => u.team !== team || u.owner === p.id || (u.owner == null && k === 0));
              const key = k === 0 ? (team === 0 ? 'prankA' : 'prankB') : team === 0 ? 'prankA2' : 'prankB2';
              res[key] = this.roundEnd(p, cb.traitCounts[team], { won, pve: false, match: { ...m, combat: own }, team, owner: p.id });
            });
          }
          hit(TA, res.dmgA);
          if (!m.ghost) hit(TB, res.dmgB);
        }
        this.results.push(res);
      }
      // the HP each team actually lost (Team Guard can lower it), written back on the results for the UI
      this.syncTeams();
      const lost = new Map();
      for (const [T, d] of teamDmg) lost.set(T, this.damageTeam(T, d));
      for (const res of this.results) {
        const m = res.match;
        if (m.pve) res.teamDmgA = lost.get(this.teamOf(m.a)) || 0;
        else {
          if (res.dmgA) res.dmgA = lost.get(this.teams[m.ta]) || 0;
          if (res.dmgB) res.dmgB = lost.get(this.teams[m.tb]) || 0;
        }
      }
      for (const p of this.players) { p.nextBuffs = null; this.combine(p); }
      this.eliminate();
      this.phase = 'result';
      return this.results;
    }

    // after a player's combat: trait onRoundEnd hooks (with the levels from that combat), then the player's
    // augment / power / mechanic onRoundEnd. e.gold is paid out; returns it (shown as bonus gold).
    roundEnd(p, counts, info) {
      const e = { gold: 0, ...info };
      this.fireTraits('onRoundEnd', p, counts, e);
      this.fireGame('onRoundEnd', p, e);
      if (e.gold) p.gold += e.gold;
      return e.gold;
    }

    applyStreak(p, won) {
      if (won) p.streak = p.streak > 0 ? p.streak + 1 : 1;
      else p.streak = p.streak < 0 ? p.streak - 1 : -1;
    }

    // PvE loot drops as orbs on the board. AI Tamers pick theirs up at once; the human walks a Little Legend over them.
    // Blue orbs hold gold or a component, gold orbs hold completed items or Digimon. Losing still drops something.
    // A set can replace this with pve.reward(ctx, won) returning orb specs [{ kind, gold, items, unit }].
    pveReward(p, won) {
      const orbs = [];
      const orb = (kind, c) => orbs.push({ kind, gold: 0, items: [], unit: null, ...c });
      if (this.set.pveReward) {
        for (const o of this.set.pveReward(this.ctx(p), won) || []) {
          if (o.unit) { if (this.pool[o.unit] > 0) this.takeFromPool(o.unit); else o.unit = null; }
          orb(o.kind || 'blue', { ...o, items: (o.items || []).filter(Boolean) });
        }
      } else if (this.stage === 1) {
        orb('blue', { items: [this.randomComponent()].filter(Boolean) });
        if (won) {
          const last = this.round === this.R.roundsInStage(1); // the last wild round of stage 1 pays more
          orb('blue', { gold: last ? 2 : 1 });
          if (last) {
            const lines = DT.SHOP_LINES.filter((l) => l.tier <= 2 && this.pool[l.id] > 0);
            if (lines.length) {
              const l = this.pick(lines);
              this.takeFromPool(l.id);
              orb('gold', { unit: l.id });
            }
          }
        }
      } else {
        orb('blue', { gold: won ? 3 : 2 });
        if (won && this.stage >= 4) orb('gold', { items: [this.randomCompleted()].filter(Boolean) });
        if (won && this.stage >= 4 && this.rng() < 0.2) orb('gold', { items: [this.randomCrest()].filter(Boolean) });
        else orb('blue', { items: [this.randomComponent(this.stage >= 3)].filter(Boolean) });
        if (won && this.stage >= 3 && this.rng() < 0.25 && DT.CONSUMABLE_IDS.length) orb('gold', { items: [this.pick(DT.CONSUMABLE_IDS)] });
      }
      const reward = { gold: 0, unit: null, items: [], orbs: orbs.length };
      for (const o of orbs) {
        o.id = ++this.uid;
        o.x = 0.1 + this.rng() * 0.8;
        o.z = 0.15 + this.rng() * 0.7;
        reward.gold += o.gold;
        reward.items.push(...o.items);
        if (o.unit) reward.unit = o.unit;
        p.orbs.push(o);
      }
      if (!p.isHuman) this.collectAllOrbs(p);
      return reward;
    }

    collectOrb(p, id) {
      const i = p.orbs.findIndex((o) => o.id === id);
      if (i < 0) return null;
      const o = p.orbs.splice(i, 1)[0];
      p.gold += o.gold;
      for (const it of o.items) this.giveItem(p, it);
      if (o.unit) {
        const free = p.freeBenchSlot();
        if (free >= 0) { p.bench[free] = this.newUnit(o.unit, 0); this.combine(p); }
        else { this.pool[o.unit] += 1; p.gold += DT.LINE[o.unit].tier; }
      }
      return o;
    }

    collectAllOrbs(p) {
      while (p.orbs.length) this.collectOrb(p, p.orbs[0].id);
    }

    // Duos: teams at 0 HP are knocked out together; placements count teams (1-4)
    eliminateTeams() {
      const dead = this.teams.filter((T) => T.alive && this.teamHp(T) <= 0).sort((a, b) => this.teamHp(a) - this.teamHp(b) || a.id - b.id);
      let place = this.aliveTeams().length;
      for (const T of dead) {
        T.alive = false;
        T.placement = place--;
        for (const p of T.members) this.knockOut(p, T.placement);
      }
      const alive = this.aliveTeams();
      if (alive.length <= 1) {
        if (alive[0]) { alive[0].placement = 1; for (const p of alive[0].members) p.placement = 1; }
        this.over = true;
      }
      if (!this.playOut && this.players.some((p) => p.isHuman) && !this.alivePlayers().some((p) => p.isHuman)) this.over = true;
    }

    eliminate() {
      if (this.duo) return this.eliminateTeams();
      const dead = this.players.filter((p) => p.alive && p.hp <= 0).sort((a, b) => a.hp - b.hp);
      let place = this.alivePlayers().length;
      for (const p of dead) this.knockOut(p, place--);
      const alive = this.alivePlayers();
      if (alive.length <= 1) {
        if (alive[0]) alive[0].placement = 1;
        this.over = true;
      }
      // with humans in the lobby, the match ends once they are all out (unless a solo player chose to watch the rest)
      if (!this.playOut && this.players.some((p) => p.isHuman) && !alive.some((p) => p.isHuman)) this.over = true;
    }

    // a player is out: units, shop and orbs go back to the pool
    knockOut(p, placement) {
      p.alive = false;
      p.placement = placement;
      for (const u of p.allUnits()) this.returnToPool(u);
      for (const id of p.shop) if (id) this.pool[id] += 1;
      p.board = {};
      p.bench = new Array(BENCH).fill(null);
      p.shop = new Array(p.shop.length).fill(null);
      p.shopMeta = null;
      for (const o of p.orbs) if (o.unit) this.pool[o.unit] += 1;
      p.orbs = [];
      p.items = [];
      p.offer = null;
      p.wisp = null;
      if (this.hasHook('onEliminate', p)) this.fireGame('onEliminate', p, { placement: p.placement });
    }

    // Solo only: the human is out but chose to spectate, so the AI Tamers play on to a winner.
    // Never used in multiplayer (lockstep games end for everyone the same way).
    playOn() {
      if (!this.over || (this.duo ? this.aliveTeams().length < 2 : this.alivePlayers().length < 2)) return false;
      this.over = false;
      this.playOut = true;
      return true;
    }

    // ---- Player actions ----
    // Every human input goes through here, so a multiplayer log of (slot, action, args) replays identically
    // on every client. Returns the method's result for the UI's messages. Unknown actions go to the set
    // mechanic's actions (e.g. 'wisp' in Faction Wars); args come from the network, so validate them there.
    applyAction(p, a, args = {}) {
      if (!p) return null;
      this.ensureSet();
      const n = (x) => (Number.isSafeInteger(x) && x >= 0 ? x : -1);
      const spot = (s) => (s && s.type === 'board' ? { type: 'board', c: n(s.c), r: n(s.r) } : s && s.type === 'bench' ? { type: 'bench', i: n(s.i) } : null);
      if (a === 'claim') { p.isHuman = true; p.name = String(args.name || p.name).slice(0, 16); return true; }
      if (a === 'release') {
        p.isHuman = false;
        p.name = String(args.name || p.name).slice(0, 16);
        if (this.carousel && this.carouselTurn() === p) this.carouselStep();
        return true;
      }
      if (!p.alive || this.over) return null;
      switch (a) {
        case 'buy': return this.buy(p, n(args.slot));
        case 'sell': return this.sell(p, n(args.uid));
        case 'move': { const from = spot(args.from), to = spot(args.to); return from && to ? this.move(p, from, to) : false; }
        case 'reroll': return this.reroll(p);
        case 'lock': p.locked = !p.locked; return p.locked;
        case 'equip': return this.equipItem(p, n(args.idx), n(args.uid));
        case 'swapItems': return this.swapItems(p, n(args.a), n(args.b));
        case 'buyXp': return this.phase === 'planning' || this.phase === 'result' ? this.buyXp(p) : false;
        case 'offer': return this.phase === 'planning' ? this.chooseOffer(p, n(args.i)) : null;
        case 'offerReroll': return this.phase === 'planning' ? this.rerollOffer(p) : false;
        case 'carousel': {
          if (!this.carousel) return false;
          const i = args.i == null ? this.carousel.opts.findIndex((o) => !o.by) : n(args.i);
          const ok = this.carouselPick(p, i);
          if (ok) this.carouselStep();
          return ok;
        }
        case 'orb': return this.collectOrb(p, n(args.id));
        // Duos: send a bench Digimon ({ uid }) or an item ({ item: index }) to your partner
        case 'portal': return this.duo ? this.portal(p, args.uid != null ? { uid: n(args.uid) } : { item: n(args.item) }) : null;
      }
      const fn = this.set.mechanic.actions && this.set.mechanic.actions[a];
      if (typeof fn === 'function') return fn(this.ctx(p, { kind: 'mechanic' }), args && typeof args === 'object' ? args : {});
      return null;
    }

    // ---- Planning phase ----
    nextRound() {
      if (this.over) return;
      this.ensureSet();
      const R = this.R;
      if (this.round < R.roundsInStage(this.stage)) this.round++;
      else { this.stage++; this.round = 1; }
      this.roundType = this.typeOf(this.stage, this.round);
      this.isPvE = this.roundType === 'pve';
      this.humanMatch = null;
      const label = this.roundLabel;
      if (this.duo) for (const p of this.players) { p.portal = 0; p.incoming = []; }
      for (const p of this.alivePlayers()) {
        if (R.xp) this.addXp(p, R.xp.perRound != null ? R.xp.perRound : 2);
        else if (this.round === 1) p.level = R.levelByStage[Math.min(this.stage, R.levelByStage.length - 1)];
        p.gold += this.income(p);
        p.freeRolls = 0;
        if (!p.locked) this.refreshShop(p);
        this.makeOffer(p);
        if (p.power && (R.powerRanks || []).includes(label)) {
          p.power.rank++;
          this.fireGame('onRankUp', p, { rank: p.power.rank });
        }
        this.fireGame('onRoundStart', p, {});
        this.clearMaxedShop(p);
      }
      this.phase = 'planning';
      if (this.roundType === 'carousel') this.startCarousel();
      else this.planAI();
      if (this.duo) this.syncTeams();
    }

    // income parts: base (by stage or flat), interest, streak, augment and bonus (hooks add to these)
    incomeBreakdown(p) {
      const R = this.R;
      const base = R.incomeByStage ? R.incomeByStage[Math.min(this.stage, R.incomeByStage.length - 1)]
        : typeof R.baseIncome === 'function' ? R.baseIncome(this.stage, this.round) : R.baseIncome || 0;
      const interest = R.interest ? Math.min(Math.floor(Math.max(0, p.gold) / (R.interest.per || 10)), R.interest.max || 5) : 0;
      let streak = 0;
      if (R.streak) { const s = Math.abs(p.streak); for (const [n, g] of R.streak) if (s >= n) streak = g; }
      const e = { base, interest, streak, augment: 0, bonus: 0 };
      this.fireGame('onIncome', p, e);
      e.total = e.base + e.interest + e.streak + e.augment + e.bonus;
      return e;
    }

    income(p) { return this.incomeBreakdown(p).total; }

    nextLevelAt() {
      const R = this.R;
      if (R.xp) return null;
      return this.stage + 1 < R.levelByStage.length ? { stage: this.stage + 1, level: R.levelByStage[this.stage + 1] } : null;
    }
  }

  DT.Game = Game;
  DT.Player = Player;
  DT.GameAPI = GAME_API;
  DT.cellKey = key;
  void H;
})(typeof window !== 'undefined' ? window : globalThis);
