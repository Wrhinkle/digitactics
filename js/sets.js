// Set registry: every match plays one self-contained ruleset ("set"). A set lives in js/sets/<id>.js (data) plus
// optional js/sets/<id>-mech.js (hooks) and js/sets/<id>-art.js (item art and move looks). The format is documented in
// docs/sets/FORMAT.md. No DOM access, so it runs headless.
//
// DT.Sets.register(def)         add a set (js/sets/<id>.js)
// DT.Sets.extend(id, patch)     add hooks, art or overrides to a set (js/sets/<id>-mech.js, -art.js); may run before register
// DT.Sets.activate(id)          compile the set (once) and point the DT.LINES / DT.TRAITS / DT.ITEMS ... globals at it
// DT.Sets.resolve(id, seed)     'chaos' -> a set picked from the seed; unknown ids -> the default set
// DT.Sets.active()              the compiled active set
// DT.Sets.pickable()            sets the picker shows (registered, not hidden), in picker order
(function (G) {
  const DT = (G.DT = G.DT || {});

  const DEFAULT = 'faction-wars';
  // augments shared by every set (the Duos augments in js/duos.js): every set's AUGMENT table falls back to this one
  DT.SHARED_AUGMENT = DT.SHARED_AUGMENT || {};
  const defs = new Map(); // id -> raw definition
  const patches = new Map(); // id -> [patch]
  const compiled = new Map(); // id -> compiled set
  // Every line of every set, first registration wins. DT.LINE falls back to it, so cosmetics that name a line
  // (Little Legends, kill screens) still find it while another set is active.
  const LIBRARY = (DT.LINE_LIBRARY = DT.LINE_LIBRARY || {});

  // Hook names the engine fires. Combat hooks get a combat context, game hooks a game context (see FORMAT.md).
  const COMBAT_HOOKS = ['onCombatStart', 'onCombatReady', 'onTick', 'onUnitTick', 'onAttack', 'onBeforeDamage', 'onBeforeDamageTaken',
    'onDamageDealt', 'onDamageTaken', 'onLethal', 'onCast', 'onKill', 'onDeath', 'onShield', 'onHeal', 'onCombatEnd'];
  const ANY_HOOKS = ['onAnyAttack', 'onAnyCast', 'onAnyKill', 'onAnyDeath', 'onAnyDamageDealt', 'onAnyHeal', 'onAnyShield'];
  const GAME_HOOKS = ['onGameStart', 'onRoundStart', 'onRoundEnd', 'onIncome', 'onShop', 'onShopRolled', 'onBuy', 'onSell', 'onCombine', 'onPick',
    'onRankUp', 'onAiTurn', 'onEliminate', 'onTeamSize', 'canPlace', 'canEquip', 'onCountTraits', 'onUse', 'onPortal', 'onPartnerCombine'];
  const HOOK_NAMES = new Set([...COMBAT_HOOKS, ...ANY_HOOKS, ...GAME_HOOKS]);

  // Hyper Roll rules (Faction Wars). Sets override any of these in def.rules.
  const DEFAULT_RULES = {
    startHp: 20,
    startGold: 3,
    planTime: 30,
    itemCap: 10,
    rerollCost: 1,
    winGold: 2,
    shopSize: 5,
    poolSize: [0, 30, 25, 18, 10, 9],
    shopOdds: [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [75, 25, 0, 0, 0], [55, 30, 15, 0, 0], [45, 33, 20, 2, 0],
      [30, 40, 25, 5, 0], [19, 30, 40, 10, 1], [18, 25, 32, 22, 3], [15, 20, 25, 30, 10]],
    // Levels: levelByStage (Hyper Roll, no XP) or xp: { table, perRound, buyCost, buyXp, startLevel, maxLevel }.
    levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
    xp: null,
    // Income: incomeByStage (fixed per stage) or baseIncome; plus optional interest { per, max } and streak [[n, gold], ...].
    incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
    baseIncome: 0,
    interest: null,
    streak: null,
    lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
    // optional: unitDamage(unit, stage) -> extra HP lost per surviving enemy unit (TFT style); default none
    unitDamage: null,
    pveLossDamage: (stage) => (stage === 1 ? 1 : null), // null = use lossDamage
    roundsInStage: (stage) => (stage === 1 ? 3 : 4),
    roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'pve' : 'pvp'),
    armoryStages: [2, 3, 4, 5, 6],
    armoryRound: 1,
    augmentStages: [2, 3, 4],
    augmentRound: 2,
    augmentRerolls: 1,
    augmentTierOdds: [0.35, 0.82], // roll < a: tier 1, < b: tier 2, else tier 3
    // optional extra offers: [{ kind, stage, round, rerolls }] (kinds: armory, augment, power, itemTier or a mechanic offer kind)
    offers: [],
    powerRanks: [], // Tamer power rank-ups: ['5-1', '7-1'] (stage-round labels)
    copies: 3, // copies of a form that digivolve; a line can set combine: 2
    maxStage: 2,
    teamSize: null, // optional (level) => team size; default level
    carouselTiers: (stage) => (stage <= 1 ? [1, 2] : stage <= 3 ? [1, 2, 3] : [2, 3, 4]),
    carouselItems: 'component', // 'component' | 'completed' | (game, stage) -> item id
    winGoldPvE: 0,
    // optional label lists ('2-1') that replace augmentStages/augmentRound and armoryStages/armoryRound
    augmentRounds: null,
    armoryRounds: null,
    combatTime: { overtime: 30, limit: 45 }, // seconds: overtime damage ramp starts, combat ends as a draw
  };

  // ---------- pace: Hyper Roll ('hyper') or standard TFT ('normal') ----------
  // Every set plays in both paces (docs/sets/FORMAT.md, Pace). A set's flat `rules` describe its native pace;
  // rules.hyper / rules.normal override per pace; pace keys of the other pace fall back to these defaults.
  const PACES = ['hyper', 'normal'];
  // standard TFT stage damage by stage (8+ ends the game) and damage per surviving enemy unit
  const TFT_STAGE_DAMAGE = [0, 0, 1, 3, 5, 7, 9, 15, 150];
  const NORMAL_RULES = {
    ...DEFAULT_RULES,
    startHp: 100,
    startGold: 0,
    planTime: 40,
    rerollCost: 2,
    winGold: 1,
    winGoldPvE: 0,
    shopOdds: [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [75, 25, 0, 0, 0], [55, 30, 15, 0, 0], [45, 33, 20, 2, 0],
      [30, 40, 25, 5, 0], [19, 30, 40, 10, 1], [18, 25, 32, 22, 3], [15, 20, 25, 30, 10], [5, 10, 20, 40, 25]],
    levelByStage: null,
    xp: { table: [0, 2, 2, 6, 10, 20, 36, 48, 76, 84], perRound: 2, buyCost: 4, buyXp: 4, startLevel: 1, maxLevel: 10 },
    incomeByStage: null,
    // passive income: 2 at 1-2 and 1-3, 3 at 1-4, 4 at 2-1, then 5
    baseIncome: (stage, round) => (stage === 1 ? (round <= 3 ? 2 : 3) : stage === 2 && round === 1 ? 4 : 5),
    interest: { per: 10, max: 5 },
    streak: [[3, 1], [5, 2], [6, 3]],
    lossDamage: (stage) => TFT_STAGE_DAMAGE[Math.min(stage, TFT_STAGE_DAMAGE.length - 1)],
    unitDamage: (u) => [1, 1, 2][u.stage] || 1,
    // AI Tamers buy XP toward this level (TFT standard curve: 4 at 2-1, 6 at 3-2, 7 at 4-1, 8 at 5-1, 9 at 6-1)
    aiLevelTarget: (stage, round) => (stage <= 1 ? 3 : stage === 2 ? (round >= 5 ? 5 : 4) : stage === 3 ? (round >= 2 ? 6 : 5) : stage === 4 ? (round >= 5 ? 8 : 7) : stage === 5 ? 8 : 9),
    pveLossDamage: (stage) => (stage === 1 ? 0 : null),
    roundsInStage: (stage) => (stage === 1 ? 4 : 7),
    roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'carousel' : round === 7 ? 'pve' : 'pvp'),
    carouselTiers: (stage) => (stage <= 1 ? [1, 2] : stage === 2 ? [1, 2, 3] : stage <= 4 ? [2, 3, 4] : [3, 4, 5]),
    carouselItems: (game, stage) => (stage >= 5 ? game.randomCompleted() : game.randomComponent(stage >= 3)),
    armoryStages: [],
    augmentStages: [],
    augmentRounds: ['2-1', '3-2', '4-2'],
    offers: [],
    powerRanks: [],
    combatTime: { overtime: 35, limit: 50 },
  };
  const PACE_DEFAULTS = { hyper: DEFAULT_RULES, normal: NORMAL_RULES };
  // keys that describe a pace; a set's flat values for them only apply in its native pace
  const PACE_KEYS = ['startHp', 'startGold', 'planTime', 'rerollCost', 'winGold', 'winGoldPvE', 'levelByStage', 'xp', 'incomeByStage',
    'baseIncome', 'interest', 'streak', 'lossDamage', 'unitDamage', 'pveLossDamage', 'roundsInStage', 'roundType', 'carouselTiers',
    'armoryStages', 'armoryRound', 'armoryRounds', 'augmentStages', 'augmentRound', 'augmentRounds', 'offers', 'powerRanks', 'combatTime', 'aiLevelTarget'];
  const PACE_KEY_SET = new Set(PACE_KEYS);
  const hyperLoss = DEFAULT_RULES.lossDamage;
  // Mode profiles for the non-native pace (object or (nativeRules) => object), applied over the pace defaults.
  // Auto Chess and Underlords have their own economy (native 'normal'); their Hyper Roll compresses it.
  const PACE_MODES = {
    // Auto Chess Hyper Roll: 20 HP, levels 3-10 by stage, fixed income, 1-gold rerolls, creeps at 1-1..1-3 and x-4
    autochess: {
      hyper: {
        startHp: 20, startGold: 3, planTime: 30, rerollCost: 1, winGold: 2, winGoldPvE: 0,
        levelByStage: [0, 3, 4, 5, 6, 7, 8, 9, 10], xp: null,
        incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9], baseIncome: 0, interest: null, streak: null,
        lossDamage: hyperLoss, unitDamage: null, pveLossDamage: () => 1, // creeps only chip (natively they deal unit damage only)
        roundsInStage: (stage) => (stage === 1 ? 3 : 4),
        roundType: (stage, round) => (stage === 1 || round === 4 ? 'pve' : 'pvp'),
        armoryStages: [], augmentStages: [], augmentRounds: null, offers: [], powerRanks: [],
      },
    },
    // Underlords Hyper Roll keeps its 5-round stages (its offers and Tamer ranks are labelled by round number) and
    // compresses the game through HP, flat loss damage, stage levels and fixed income. Short modes (<= 20 HP) keep theirs.
    underlords: {
      hyper: (n) => (n.startHp <= 20 ? {} : {
        startHp: 20, rerollCost: 1, levelByStage: [0, 3, 4, 5, 6, 7, 8, 9, 10], xp: null,
        incomeByStage: [0, 5, 6, 7, 8, 9, 9, 10], baseIncome: 0, interest: null, streak: null,
        lossDamage: hyperLoss, unitDamage: null, pveLossDamage: (stage) => (stage === 1 ? 1 : null),
        roundsInStage: n.roundsInStage, roundType: n.roundType, offers: n.offers, powerRanks: n.powerRanks,
        armoryStages: [], augmentStages: [], augmentRounds: null, combatTime: DEFAULT_RULES.combatTime,
      }),
    },
  };
  const NATIVE_NORMAL_MODES = new Set(['autochess', 'underlords']);
  DT.PACES = PACES;
  DT.PACE_DEFAULTS = PACE_DEFAULTS;
  DT.PACE_KEYS = PACE_KEYS;
  DT.PACE_MODES = PACE_MODES;

  // AI hints by stat template. place: front | back | flank. tank: priority for the tank slot (0 = never). carry: can carry items.
  // dmg: 'ap' (builds AP), 'ad' (AD unless its ability heals or shields), 'ad!' (always AD).
  const ROLE_AI = {
    Guardian: { place: 'front', tank: 1, carry: false, dmg: 'ad!' },
    Bruiser: { place: 'front', tank: 2, carry: false, dmg: 'ad' },
    Striker: { place: 'front', tank: 3, carry: true, dmg: 'ad' },
    Duelist: { place: 'front', tank: 3, carry: true, dmg: 'ad' },
    Sniper: { place: 'back', tank: 0, carry: true, dmg: 'ad' },
    Mystic: { place: 'back', tank: 0, carry: true, dmg: 'ap' },
    Support: { place: 'back', tank: 0, carry: true, dmg: 'ap' },
    Assassin: { place: 'flank', tank: 0, carry: true, dmg: 'ad', leap: true },
  };
  const SUPPORT_TYPES = ['heal', 'shield', 'rally'];

  const warn = (msg) => { if (typeof console !== 'undefined') console.warn(`Digi-Tactics sets: ${msg}`); };

  // Sets still being built: registered (so DT.UI.newGame(id) works for testing) but left out of the picker and Chaos.
  const IN_PROGRESS = new Set([]);

  function register(def) {
    if (!def || typeof def.id !== 'string' || !def.id) throw new Error('DT.Sets.register: set needs an id');
    if (IN_PROGRESS.has(def.id)) def.hidden = true;
    if (defs.has(def.id)) warn(`set ${def.id} registered twice; the last one wins`);
    defs.set(def.id, def);
    compiled.delete(def.id);
    for (const l of def.lines || []) if (l && l.id && !LIBRARY[l.id]) LIBRARY[l.id] = l;
    return def;
  }

  function extend(id, patch) {
    if (!patches.has(id)) patches.set(id, []);
    patches.get(id).push(patch || {});
    compiled.delete(id);
  }

  // ---------- compile ----------
  const mergeInto = (target, src) => { for (const k in src) target[k] = src[k]; return target; };
  const asList = (x) => (Array.isArray(x) ? x : x ? [x] : []);

  function normalizeLine(l, S) {
    if (l.shop == null) l.shop = true;
    if (!l.role) l.role = (l.traits || []).find((t) => S.TRAITS[t] && S.TRAITS[t].kind === 'role') || null;
    if (!l.role || !S.ROLES[l.role]) {
      warn(`${S.id}: line ${l.id} has no stat template (role); using Striker`);
      l.role = l.role && S.ROLES[l.role] ? l.role : 'Striker';
    }
    for (const t of l.traits || []) if (!S.TRAITS[t]) warn(`${S.id}: line ${l.id} has unknown trait ${t}`);
    l.set = S.id;
    // AI hints: role template defaults, then the set's lineAi(line) function, then the line's own ai field
    const R = S.ROLES[l.role] || {};
    const base = { ...(ROLE_AI[l.role] || { place: 'front', tank: 0, carry: true, dmg: 'ad' }), ...(R.ai || {}) };
    if (R.leap) base.leap = true;
    if (base.dmg === 'ad' && SUPPORT_TYPES.includes(l.type)) base.dmg = 'ap';
    if (base.dmg === 'ad!') base.dmg = 'ad';
    const fromSet = S.def.lineAi ? S.def.lineAi(l, base) || {} : {};
    l.ai = { ...base, ...fromSet, ...(l.ai || {}) };
    return l;
  }

  // Rules for pace P: the pace defaults, the set's flat rules (only pace-independent keys outside the native pace),
  // the mode's profile for a non-native pace, then the set's rules[P] block.
  function buildRules(S, P) {
    const raw = S.rawRules || {};
    const out = { ...PACE_DEFAULTS[P] };
    const native = P === S.nativePace;
    for (const k of Object.keys(raw)) {
      if (PACES.includes(k)) continue;
      if (!native && PACE_KEY_SET.has(k)) continue;
      out[k] = raw[k];
    }
    if (!native) {
      const prof = PACE_MODES[S.mode] && PACE_MODES[S.mode][P];
      if (prof) Object.assign(out, typeof prof === 'function' ? prof(buildRules(S, S.nativePace)) || {} : prof);
    }
    Object.assign(out, raw[P] || {});
    out.pace = P;
    return out;
  }
  function applyRuleTables(S, rules) {
    S.POOL_SIZE = rules.poolSize;
    S.SHOP_ODDS = rules.shopOdds;
    S.XP_NEXT = rules.xp && rules.xp.table ? rules.xp.table : [0, 2, 2, 6, 10, 20, 36, 48, 72];
  }

  function compile(id) {
    const raw = defs.get(id);
    if (!raw) throw new Error(`DT.Sets: unknown set ${id}`);
    // shallow copy so patches never touch the registered definition twice
    const def = { ...raw };
    const S = { id, def, name: def.name || id, mode: def.mode || 'tft', desc: def.desc || '', colors: def.colors || {}, source: def.source || '' };
    const traitPatch = {}, itemPatch = {}, augPatch = {}, powerPatch = {}, linePatch = {};
    const art = { items: {}, moves: {}, basic: {} };
    const mech = { ...(def.mechanic || {}) };
    mech.hooks = { ...(mech.hooks || {}) };
    mech.actions = { ...(mech.actions || {}) };
    mech.offerKinds = { ...(mech.offerKinds || {}) };
    const setHooks = { ...(def.hooks || {}) };
    // raw rules: flat keys plus the per-pace blocks (copied, so patches never touch the definition)
    const rawRules = { ...(def.rules || {}) };
    for (const P of PACES) if (rawRules[P]) rawRules[P] = { ...rawRules[P] };
    const abilities = { ...(def.abilities || {}) };
    let lineAi = def.lineAi || null;
    // UI and rules helpers a set may give (register or extend): unitNote(unit) -> text lines for the unit tooltip,
    // sellValue(unit, base) -> gold for selling a unit
    let unitNote = def.unitNote || null;
    let sellValue = def.sellValue || null;
    for (const p of patches.get(id) || []) {
      for (const [k, v] of Object.entries(p.traits || {})) traitPatch[k] = { ...(traitPatch[k] || {}), ...v };
      for (const [k, v] of Object.entries(p.items || {})) itemPatch[k] = { ...(itemPatch[k] || {}), ...v };
      for (const [k, v] of Object.entries(p.augments || {})) augPatch[k] = { ...(augPatch[k] || {}), ...v };
      for (const [k, v] of Object.entries(p.powers || {})) powerPatch[k] = { ...(powerPatch[k] || {}), ...v };
      for (const [k, v] of Object.entries(p.lines || {})) linePatch[k] = { ...(linePatch[k] || {}), ...v };
      if (p.mechanic) {
        const { hooks, actions, offerKinds, ...rest } = p.mechanic;
        mergeInto(mech, rest);
        mergeInto(mech.hooks, hooks || {});
        mergeInto(mech.actions, actions || {});
        mergeInto(mech.offerKinds, offerKinds || {});
      }
      if (p.hooks) mergeInto(setHooks, p.hooks);
      if (p.rules) for (const [k, v] of Object.entries(p.rules)) rawRules[k] = PACES.includes(k) ? { ...(rawRules[k] || {}), ...(v || {}) } : v;
      if (p.abilities) mergeInto(abilities, p.abilities);
      if (p.lineAi) lineAi = p.lineAi;
      if (p.unitNote) unitNote = p.unitNote;
      if (p.sellValue) sellValue = p.sellValue;
      if (p.art) for (const k of ['items', 'moves', 'basic']) mergeInto(art[k], p.art[k] || {});
      if (p.itemArt) mergeInto(art.items, p.itemArt);
      if (p.moves) mergeInto(art.moves, p.moves);
      if (p.basic) mergeInto(art.basic, p.basic);
    }
    def.lineAi = lineAi;
    S.unitNote = typeof unitNote === 'function' ? unitNote : null;
    S.sellValue = typeof sellValue === 'function' ? sellValue : null;
    S.art = art;
    S.abilities = abilities;
    S.rawRules = rawRules;
    S.nativePace = PACES.includes(def.nativePace) ? def.nativePace : NATIVE_NORMAL_MODES.has(S.mode) ? 'normal' : 'hyper';
    S.pace = S.nativePace;
    const rules = buildRules(S, S.pace);
    S.rules = rules;
    S.mechanic = mech;
    // mechanic hooks and set-level hooks are both global listeners; the same name in both runs both (set first)
    S.hooks = { ...setHooks };
    for (const [k, h] of Object.entries(mech.hooks)) {
      const a = S.hooks[k];
      if (!a) { S.hooks[k] = h; continue; }
      const fa = typeof a === 'function' ? a : a.fn, fb = typeof h === 'function' ? h : h.fn;
      S.hooks[k] = { order: Math.min((a && a.order) || 0, (h && h.order) || 0), fn(c, e) { fa(c, e); fb(c, e); } };
    }
    S.modelsById = !!def.modelsById;

    // stat templates
    S.ROLES = { ...DT.BASE_ROLES, ...(def.roles || {}) };
    // traits
    S.TRAITS = {};
    for (const [k, T] of Object.entries(def.traits || {})) {
      const t = traitPatch[k] ? mergeInto(T, traitPatch[k]) : T;
      if (!Array.isArray(t.bp) || !Array.isArray(t.lv)) warn(`${id}: trait ${k} needs bp and lv arrays`);
      if (t.hooks) for (const h in t.hooks) if (!HOOK_NAMES.has(h)) warn(`${id}: trait ${k} has unknown hook ${h}`);
      S.TRAITS[k] = t;
    }
    // lines
    S.LINES = (def.lines || []).map((l) => normalizeLine(linePatch[l.id] ? mergeInto(l, linePatch[l.id]) : l, S));
    S.LINE = Object.create(LIBRARY);
    for (const l of S.LINES) {
      if (Object.prototype.hasOwnProperty.call(S.LINE, l.id)) warn(`${id}: duplicate line id ${l.id}`);
      S.LINE[l.id] = l;
    }
    S.SHOP_LINES = S.LINES.filter((l) => l.shop);

    // items
    const I = DT.buildItems(def.items || {}, S.TRAITS, itemPatch, id);
    Object.assign(S, I);
    // augments and powers
    const augList = (def.augments || []).map((a) => (augPatch[a.id] ? mergeInto(a, augPatch[a.id]) : a));
    S.AUGMENTS = augList.filter((a) => (a.kind || 'augment') === 'augment');
    // augments every set can look up (Duos augments, js/duos.js) sit on the prototype; the offer pool is S.AUGMENTS only
    S.AUGMENT = Object.create(DT.SHARED_AUGMENT);
    for (const a of augList) S.AUGMENT[a.id] = a;
    const powers = (def.powers || []).map((a) => (powerPatch[a.id] ? mergeInto(a, powerPatch[a.id]) : a)).concat(augList.filter((a) => a.kind === 'power'));
    S.POWERS = powers;
    S.POWER = {};
    for (const a of powers) S.POWER[a.id] = a;
    // Data Wisps (Faction Wars mechanic data; any set may define wisps for its own mechanic)
    S.WISPS = def.wisps || [];
    S.WISP = {};
    for (const w of S.WISPS) S.WISP[w.id] = w;
    // economy tables
    applyRuleTables(S, rules);
    S.STAGE_DAMAGE = def.stageDamage || [0, 0, 2, 4, 6, 9, 12, 15, 18];
    S.TAMERS = def.tamers || DT.DEFAULT_TAMERS;
    S.pveWave = typeof def.pve === 'function' ? def.pve : def.pve && def.pve.wave ? def.pve.wave : () => [];
    S.pveReward = def.pve && def.pve.reward ? def.pve.reward : null;
    return S;
  }

  // ---------- activation ----------
  // DT.HYPER stays one object (modules keep a reference to it); its keys are replaced on activation.
  DT.HYPER = DT.HYPER || {};
  let activeId = null;
  let active = null;

  // The compiled set for a pace (default: its native pace). Both paces share lines, traits, items and hooks; only
  // the rules and the economy tables differ. Each (set, pace) pair is one stable object.
  function get(id, pace) {
    if (!defs.has(id)) return null;
    if (!compiled.has(id)) compiled.set(id, compile(id));
    const base = compiled.get(id);
    const P = PACES.includes(pace) ? pace : base.pace;
    if (P === base.pace) return base;
    if (!base.paced) base.paced = {};
    if (!base.paced[P]) {
      const S = { ...base, pace: P, paced: null };
      S.rules = buildRules(base, P);
      applyRuleTables(S, S.rules);
      base.paced[P] = S;
    }
    return base.paced[P];
  }
  const nativePace = (id) => { const S = get(id); return S ? S.nativePace : 'hyper'; };
  // 'chaos' picks a pace from the seed (its own hash, independent of the Chaos set pick); unknown values -> null (native)
  function resolvePace(pace, seed) {
    if (pace === 'chaos') {
      let h = 0x9e3779b9;
      const s = `p${(seed >>> 0) || 0}`;
      for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 0x01000193) >>> 0; }
      return PACES[h % PACES.length];
    }
    return PACES.includes(pace) ? pace : null;
  }

  const GLOBALS = ['LINES', 'LINE', 'SHOP_LINES', 'TRAITS', 'ROLES', 'ITEMS', 'COMPONENTS', 'COMPONENT_IDS', 'STAT_COMPONENTS',
    'SPECIAL_COMPONENTS', 'RECIPE', 'CORE_ITEMS', 'EMBLEM_IDS', 'EMBLEM_OF', 'DROP_EMBLEMS', 'CONSUMABLE_IDS', 'ITEM_ROLE', 'AUGMENTS', 'AUGMENT',
    'POWERS', 'POWER', 'WISPS', 'WISP', 'POOL_SIZE', 'SHOP_ODDS', 'XP_NEXT', 'STAGE_DAMAGE', 'TAMERS', 'pveWave'];

  function activate(id, pace) {
    const want = defs.has(id) ? id : DEFAULT;
    const S = get(want, pace);
    if (!S) throw new Error(`DT.Sets: no set ${id} and no default set registered`);
    if (S === active && want === activeId) return active;
    for (const k of GLOBALS) DT[k] = S[k];
    DT.SHOP = S.rules.shopSize;
    for (const k of Object.keys(DT.HYPER)) delete DT.HYPER[k];
    Object.assign(DT.HYPER, S.rules);
    DT.SET = S;
    DT.SET_ID = S.id;
    DT.SET_PACE = S.pace;
    activeId = S.id;
    active = S;
    for (const fn of listeners) { try { fn(S); } catch (e) { warn(String(e)); } }
    return S;
  }

  // 'chaos' picks one of the chaos-eligible sets from the seed, the same way on every client.
  function chaosPool() {
    return [...defs.values()].filter((d) => !d.hidden && d.chaos !== false).map((d) => d.id).sort();
  }
  function resolve(id, seed) {
    if (id === 'chaos') {
      const pool = chaosPool();
      if (!pool.length) return DEFAULT;
      // integer hash of the seed (FNV-1a over its decimal digits), so neighbouring seeds spread out
      let h = 0x811c9dc5;
      const s = String((seed >>> 0) || 0);
      for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 0x01000193) >>> 0; }
      return pool[h % pool.length];
    }
    return id && defs.has(id) ? id : DEFAULT;
  }

  const listeners = new Set();

  function pickable() {
    return [...defs.values()].filter((d) => !d.hidden).sort((a, b) => (a.order || 99) - (b.order || 99) || String(a.name).localeCompare(String(b.name)))
      .map((d) => ({ id: d.id, name: d.name || d.id, mode: d.mode || 'tft', desc: d.desc || '', colors: d.colors || {},
        nativePace: PACES.includes(d.nativePace) ? d.nativePace : NATIVE_NORMAL_MODES.has(d.mode) ? 'normal' : 'hyper' }));
  }

  // Model index key for a line's form, or null for the placeholder creature.
  // Model assignments kept apart from the set files (js/sets/<id>-models.js): assignModels(setId, { lineId: model }),
  // model in the same shapes as a line's `model` field. They win over `unassigned` and `model`.
  const assigned = {};
  DT.modelKey = function (line, stage) {
    if (!line) return null;
    const a = assigned[line.set || activeId] && assigned[line.set || activeId][line.id];
    if (!a && line.unassigned) return null;
    const m = a || line.model;
    if (Array.isArray(m)) return m[stage] || null;
    if (typeof m === 'string') return m.includes(':') ? m : `${m}:${stage}`;
    if (m && typeof m === 'object') return m[stage] || null;
    const S = line.set ? compiled.get(line.set) : active;
    if (!line.set || (S && S.modelsById) || (!S && LIBRARY[line.id] === line)) return `${line.id}:${stage}`;
    return null;
  };

  DT.Sets = {
    DEFAULT,
    assignModels(setId, map) { assigned[setId] = Object.assign(assigned[setId] || {}, map); },
    assignedModels: (setId) => Object.assign({}, assigned[setId]),
    register,
    extend,
    activate,
    resolve,
    get,
    active: () => active || activate(DEFAULT),
    get activeId() { return activeId; },
    ids: () => [...defs.keys()],
    has: (id) => defs.has(id),
    def: (id) => defs.get(id) || null,
    pickable,
    chaosPool,
    onChange: (fn) => { listeners.add(fn); return () => listeners.delete(fn); },
    HOOK_NAMES,
    COMBAT_HOOKS,
    ANY_HOOKS,
    GAME_HOOKS,
    DEFAULT_RULES,
    ROLE_AI,
    // pace
    PACES,
    resolvePace,
    nativePace,
    rulesFor: (id, pace) => { const S = get(id, pace); return S ? S.rules : null; },
    get activePace() { return active ? active.pace : null; },
    paceLabel: (p) => (p === 'normal' ? 'Normal' : p === 'chaos' ? 'Chaos pace' : 'Hyper Roll'),
    // label for a set id, including 'chaos'
    label: (id) => (id === 'chaos' ? 'Chaos' : (defs.get(id) || {}).name || id),
  };

  // ---------- per-unit notes (UI) ----------
  // Text lines the active set adds to a unit's tooltip or shop card: def.unitNote(unit) gets a plain view of the unit
  // { line (id), stage, mods, bonusTraits, items } and returns a string, a list of strings, or nothing.
  DT.unitNote = function (unit) {
    const S = active;
    if (!S || !S.unitNote || !unit) return [];
    const view = { line: typeof unit.line === 'object' && unit.line ? unit.line.id : unit.line, stage: unit.stage || 0, mods: unit.mods || null,
      bonusTraits: unit.bonusTraits || null, items: (unit.items || []).slice() };
    let out = null;
    try { out = S.unitNote(view); } catch (e) { warn(`${S.id}: unitNote failed: ${e}`); out = null; }
    return (Array.isArray(out) ? out : out ? [out] : []).filter((x) => typeof x === 'string' && x);
  };
  // The set mechanic's game-start announcement for the UI: mechanic.intro(game) -> { title, name, desc, img } or null.
  DT.mechanicIntro = function (game) {
    const S = game && game.set ? game.set : active;
    const fn = S && S.mechanic && S.mechanic.intro;
    if (typeof fn !== 'function') return null;
    try { return fn(game) || null; } catch (e) { warn(`${S.id}: mechanic.intro failed: ${e}`); return null; }
  };

  // ---------- ability types ----------
  // Built-in types are handled in combat.js; sets add their own in def.abilities: { type: { base, cast(c, u, tg, v), text(line, stage, v) } }.
  DT.abilityDef = (type) => {
    const S = active;
    return (S && S.abilities && S.abilities[type]) || null;
  };
})(typeof window !== 'undefined' ? window : globalThis);
