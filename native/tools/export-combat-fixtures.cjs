// Isolate shared combat and equipment; executable item/set hooks remain disabled.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const originalHooks = Object.fromEntries(['buildHooks', 'fireTeam', 'fireGlobal', 'has'].map(name => [name, DT.Combat.prototype[name]]));
for (const name of ['buildHooks', 'fireTeam', 'fireGlobal']) DT.Combat.prototype[name] = function () {};
DT.Combat.prototype.has = () => false;
const kinds = ['nuke', 'aoe', 'stun', 'slam', 'shield', 'heal', 'barrage', 'drain', 'weaken', 'rally', 'line', 'execute', 'chain', 'dash'];
const snapshots = cb => cb.units.map(u => ({ id: u.id, hp: u.hp, mana: u.mana, shield: u.shield, c: u.c, r: u.r,
  fromC: u.fromC, fromR: u.fromR, moveT: u.moveT, moveDur: u.moveDur,
  alive: u.alive, target: u.target ? u.target.id : null, atkCd: u.atkCd, moveCd: u.moveCd,
  stun: u.stun, lock: u.lock, dealt: u.dealt, taken: u.taken, healed: u.healed, nAtk: u.nAtk, maxHp: u.maxHp, ad: u.ad, as: u.as, ap: u.ap, armor: u.armor, mr: u.mr, maxMana: u.maxMana, items: u.items, stage: u.stage, summoned: !!u.summoned, blossomKills: u.state.blossom || 0, timeRune: u.state.rune || 0, combatGold: u.state.gold || 0, range: u.range, piloting: !!u.state.piloting }));
const fixtures = [];
const exercised = new Set();
for (const kind of kinds) {
  let chosen;
  for (const set of DT.Sets.pickable()) {
    DT.Sets.activate(set.id, 'hyper');
    const line = DT.SHOP_LINES.find(l => l.type === kind);
    if (line) { chosen = { set: set.id, line }; break; }
  }
  if (!chosen) throw new Error(`No reference unit for ${kind}`);
  const pool = DT.SHOP_LINES.filter(l => kinds.includes(l.type));
  for (const stage of [0, 1, 2]) for (const cols of [7, 14]) {
    const seed = 431 + fixtures.length;
    const pick = DT.makeRng(seed);
    const entries = (primary) => Array.from({ length: 4 }, (_, i) => ({
      line: i === 0 && primary ? chosen.line.id : pool[Math.floor(pick() * pool.length)].id,
      stage, c: (i * 2 + 1) % cols, r: 4 + i % 2, uid: i + 1, items: [], owner: i % 2,
    }));
    const sideA = entries(true), sideB = entries(false);
    const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed });
    const samples = [{ tick: 0, units: snapshots(cb) }];
    let tick = 0;
    while (!cb.done && tick < 2000) {
      cb.step(); tick++;
      if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) });
    }
    for (const event of cb.events) if (event.type === 'cast') exercised.add(cb.units[event.at].line.type);
    fixtures.push({ name: `${kind}/${stage}/${cols}`, variant: `${chosen.set}/hyper`, seed, cols,
      sideA: sideA.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, uid: u.uid, owner: u.owner })),
      sideB: sideB.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, uid: u.uid, owner: u.owner })),
      samples, tick, winner: cb.winner, time: cb.t });
  }
}
for (const kind of kinds) if (!exercised.has(kind)) throw new Error(`Reference cases never cast ${kind}`);
DT.Sets.activate('faction-wars', 'hyper');
const entry = { line: DT.SHOP_LINES.find(l => l.type === 'nuke').id, stage: 0, c: 3, r: 4, items: [] };
for (const test of [
  { name: 'empty-both', a: [], b: [] },
  { name: 'empty-team', a: [entry], b: [] },
  { name: 'overlap', a: [entry, entry], b: [entry] },
  { name: 'time-limit', a: [entry], b: [entry], overtime: 0.05, timeLimit: 0.2 },
]) {
  const cb = new DT.Combat({ units: test.a, noTraits: true }, { units: test.b, noTraits: true },
    { seed: 11, info: { overtime: test.overtime, timeLimit: test.timeLimit } });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r }));
  fixtures.push({ name: test.name, variant: 'faction-wars/hyper', seed: 11, cols: 7,
    sideA: convert(test.a), sideB: convert(test.b), samples, tick, winner: cb.winner, time: cb.t,
    overtime: test.overtime || 30, timeLimit: test.timeLimit || 45 });
}
// Exercise every shared completed item alone, then mixed three-item loadouts in every ruleset/pace.
function equippedBattle(set, pace, loadouts, label, stage, cols) {
  DT.Sets.activate(set, pace);
  const seed = 9021 + fixtures.length;
  const pick = DT.makeRng(seed);
  const pool = DT.SHOP_LINES.filter(l => kinds.includes(l.type));
  const entries = () => Array.from({ length: 4 }, (_, i) => ({
    line: pool[Math.floor(pick() * pool.length)].id, stage, c: (i * 2 + 1) % cols,
    r: 4 + i % 2, items: loadouts[i % loadouts.length], uid: i + 1,
  }));
  const sideA = entries(), sideB = entries();
  const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, uid: u.uid, items: u.items }));
  fixtures.push({ name: label, variant: `${set}/${pace}`, seed, cols, sideA: convert(sideA), sideB: convert(sideB),
    samples, tick, winner: cb.winner, time: cb.t });
}
DT.Sets.activate('faction-wars', 'hyper');
const core = [...DT.CORE_ITEMS];
for (const item of core) for (const stage of [0, 2])
  equippedBattle('faction-wars', 'hyper', [[item]], `item/${item}/${stage}`, stage, 7);
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) for (const stage of [0, 2]) {
  DT.Sets.activate(set.id, pace);
  const pick = DT.makeRng(413 + fixtures.length);
  const available = DT.CORE_ITEMS.length ? DT.CORE_ITEMS : Object.keys(DT.ITEMS);
  const loadouts = Array.from({ length: 4 }, () => Array.from({ length: 3 }, () => available[Math.floor(pick() * available.length)]));
  equippedBattle(set.id, pace, loadouts, `mixed/${set.id}/${pace}/${stage}`, stage, stage === 2 ? 14 : 7);
}
// Real legacy dispatch with controlled hook definitions: source ordering, owner filtering,
// recursion, timed callbacks and mutable damage events are compared alongside fight state.
Object.assign(DT.Combat.prototype, originalHooks);
// Fates custom casts use the actual ability registry and mechanic (including lethal time runes).
for (const kind of ['grow', 'blossom', 'burst', 'frenzy', 'lullaby', 'showstopper', 'rewind']) for (const stage of [0, 1, 2]) for (const cols of [7, 14]) {
  const pace = cols === 7 ? 'normal' : 'hyper';
  DT.Sets.activate('fates', pace);
  const primary = DT.SHOP_LINES.find(l => l.type === kind);
  if (!primary) throw new Error(`No Fates ability ${kind}`);
  const pool = DT.SHOP_LINES.filter(l => kinds.includes(l.type));
  const seed = 12000 + fixtures.length, pick = DT.makeRng(seed);
  const entries = main => Array.from({ length: 5 }, (_, i) => ({ line: main && i === 0 ? primary.id : pool[Math.floor(pick() * pool.length)].id,
    stage, c: i, r: 4 + i % 2, items: [] }));
  const sideA = entries(true), sideB = entries(false);
  const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  if (!cb.events.some(e => e.type === 'cast' && e.at === 0)) throw new Error(`Unexercised Fates cast ${kind}/${stage}/${cols}`);
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `fates-ability/${kind}/${stage}/${cols}`, variant: `fates/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t,
    setup: { sideA: { noTraits: true }, sideB: { noTraits: true } } });
}

DT.Sets.activate('fates', 'hyper');
const fatesHookItems = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].hooks);
const itemCases = fatesHookItems.flatMap(item => [0, 2].map(stage => ({ item, stage }))).concat(Array.from({ length: 8 }, (_, n) => ({ item: null, stage: n % 3 })));
for (const { item, stage } of itemCases) {
  const pace = stage === 0 ? 'normal' : 'hyper';
  DT.Sets.activate('fates', pace);
  const cols = stage === 2 ? 14 : 7, seed = 18000 + fixtures.length, pick = DT.makeRng(seed);
  const candidates = DT.SHOP_LINES;
  const entries = () => Array.from({ length: 6 }, (_, i) => ({ line: candidates[Math.floor(pick() * candidates.length)].id,
    stage, c: i, r: 4 + i % 2,
    items: item ? (i === 0 ? [item, item] : [item]) : Array.from({ length: 3 }, () => fatesHookItems[Math.floor(pick() * fatesHookItems.length)]) }));
  const sideA = entries(), sideB = entries();
  const cb = new DT.Combat({ units: sideA, cols }, { units: sideB, cols }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `fates-item/${item || 'mixed'}/${stage}/${seed}`, variant: `fates/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts });
}
for (const noHooks of [false, true]) for (const stage of [0, 1, 2]) {
  DT.Sets.activate('fates', 'hyper');
  const previous = DT.SET.hooks.onCombatReady;
  DT.SET.hooks.onCombatReady = c => {
    for (const team of [0, 1]) {
      const source = c.units.find(u => u.team === team);
      const cell = c.freeCellNear(source);
      if (!cell) throw new Error('No summon test cell');
      c.summon(team, source.line.id, stage, cell.c, cell.r, { items: ['fat_deathblade', 'fat_art_manazane', 'fat_zzrot'], noHooks });
    }
  };
  const entries = () => ['fat_agumon', 'fat_gabumon'].map((line, i) => ({ line, stage, c: i + 2, r: 4, items: [] }));
  // Use existing exported lines rather than relying on names of Digimon mappings.
  const names = DT.SHOP_LINES.filter(l => ['nuke', 'barrage'].includes(l.type)).slice(0, 2).map(l => l.id);
  const sideA = entries().map((u, i) => ({ ...u, line: names[i] })), sideB = sideA.map(u => ({ ...u }));
  const seed = 2011;
  const cb = new DT.Combat({ units: sideA, noTraits: true }, { units: sideB, noTraits: true }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `fates-summon-items/${stage}/${noHooks}`, variant: 'fates/hyper', seed, cols: 7,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t,
    summonItemProbe: true, summonNoHooks: noHooks, summonStage: stage,
    setup: { sideA: { noTraits: true }, sideB: { noTraits: true } } });
  if (previous) DT.SET.hooks.onCombatReady = previous; else delete DT.SET.hooks.onCombatReady;
}
DT.Sets.activate('fates', 'hyper');
const fatesTraits = Object.entries(DT.TRAITS).map(([id, t]) => ({ id, bp: [...t.bp] }));
for (const { id, bp } of fatesTraits) for (let level = 0; level < bp.length; level++) for (const fallen of id === 'FALLEN' ? ['dark', 'holy'] : ['dark']) {
  const pace = level % 2 ? 'normal' : 'hyper';
  DT.Sets.activate('fates', pace);
  const cols = level % 2 ? 14 : 7, seed = 14000 + fixtures.length, pick = DT.makeRng(seed);
  const own = DT.SHOP_LINES.filter(l => l.traits.includes(id)).slice(0, bp[level]);
  const selected = [];
  while (selected.length < Math.max(4, own.length)) { const l = DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)]; if (!selected.includes(l)) selected.push(l); }
  const components = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].component && !DT.ITEMS[id].hooks);
  const entries = lines => lines.map((l, i) => ({ line: l.id, stage: level % 3, c: i % cols, r: 4 + Math.floor(i / cols),
    items: i % 3 ? [] : [components[0]], owner: i % 2 }));
  const sideA = entries(own), sideB = entries(selected);
  if (own.length < bp[level]) sideA[0].bonusTraits = { [id]: bp[level] - own.length };
  sideA[0].mods = { partner: true, bonus: ['hp', 'ap', 'ad', 'mana'][fixtures.length % 4] };
  const mech = { deva: level * 3, fallen, blossom: 12 };
  const cb = new DT.Combat({ units: sideA, cols, mech }, { units: sideB, cols, mech }, { seed });
  if (DT.traitLevel(id, cb.traitCounts[0][id]) !== level + 1) throw new Error(`Missing Fates breakpoint ${id}/${level}`);
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, owner: u.owner, items: u.items, bonusTraits: u.bonusTraits, partner: u.mods?.partner ? { bonus: u.mods.bonus } : null }));
  fixtures.push({ name: `fates-trait/${id}/${level}/${fallen}`, variant: `fates/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts, setup: { sideA: { fates: mech }, sideB: { fates: mech } } });
}
DT.Sets.activate('faction-wars', 'hyper');
const productionTraits = Object.entries(DT.TRAITS).map(([id, t]) => ({ id, bp: [...t.bp] }));
for (const { id, bp } of productionTraits) for (let level = 0; level < bp.length; level++) {
  const pace = level % 2 ? 'normal' : 'hyper';
  DT.Sets.activate('faction-wars', pace);
  const cols = level % 2 ? 14 : 7;
  const seed = 3200 + fixtures.length, pick = DT.makeRng(seed);
  const own = DT.SHOP_LINES.filter(l => l.traits.includes(id)).slice(0, bp[level]);
  if (own.length < bp[level]) throw new Error(`Not enough distinct lines for ${id}/${level}`);
  const rival = DT.SHOP_LINES;
  const selected = [];
  while (selected.length < own.length) { const l = rival[Math.floor(pick() * rival.length)]; if (!selected.includes(l)) selected.push(l); }
  const entries = lines => lines.map((l, i) => ({ line: l.id, stage: level % 3, c: i % cols, r: 4 + Math.floor(i / cols),
    items: [DT.CORE_ITEMS[Math.floor(pick() * DT.CORE_ITEMS.length)]], owner: i % 2 }));
  const sideA = entries(own), sideB = entries(selected);
  const cb = new DT.Combat({ units: sideA, cols }, { units: sideB, cols }, { seed });
  if (DT.traitLevel(id, cb.traitCounts[0][id]) !== level + 1) throw new Error(`Missing breakpoint ${id}/${level}`);
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, owner: u.owner, items: u.items }));
  fixtures.push({ name: `faction-trait/${id}/${level}`, variant: `faction-wars/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts });
}

// Augments and counting edge cases use the production definitions, including Crest's counting hook.
DT.Sets.activate('faction-wars', 'hyper');
const combatAugments = Object.entries(DT.AUGMENT).filter(([id, a]) => Object.keys(a.hooks || {}).some(k => DT.Sets.COMBAT_HOOKS.includes(k))).map(([id]) => id);
for (const augment of [...combatAugments, 'crest', 'counting-edge']) {
  const seed = 7700 + fixtures.length;
  const pick = DT.makeRng(seed);
  const lines = ['agumon', 'guilmon', 'salamon', 'woodmon', 'hackmon', 'dracmon'];
  const entries = () => lines.map((line, i) => ({ line, stage: 1, c: i, r: 4 + i % 3, items: i % 2 ? [] : [DT.CORE_ITEMS[Math.floor(pick() * DT.CORE_ITEMS.length)]] }));
  const sideA = entries(), sideB = entries();
  if (augment === 'counting-edge') {
    sideA[1].line = sideA[0].line;
    const emblem = trait => Object.keys(DT.ITEMS).find(id => DT.ITEMS[id].emblem === trait);
    sideA[0].items = [emblem('INS'), emblem('INS')]; sideA[1].items = [emblem('HB')];
    sideA[0].bonusTraits = { DR: 1, JT: 2 }; sideA[1].bonusTraits = { DR: 2, JT: 1 };
    for (const id of [...sideA[0].items, ...sideA[1].items]) if (!DT.ITEMS[id]) throw new Error(`Missing emblem ${id}`);
  }
  const augs = augment === 'counting-edge' ? ['crest'] : [augment];
  const cb = new DT.Combat({ units: sideA, augs }, { units: sideB, augs: [] }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items, bonusTraits: u.bonusTraits, partner: u.mods?.partner ? { bonus: u.mods.bonus } : null }));
  fixtures.push({ name: `faction-augment/${augment}`, variant: 'faction-wars/hyper', seed, cols: 7,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    augsA: augs, augsB: [], traitCounts: cb.traitCounts });
}
// Production Duos sides: owner-scoped augments, per-partner buffs and independently counted boards.
for (let variant = 0; variant < 18; variant++) {
  const pace = variant % 2 ? 'normal' : 'hyper';
  DT.Sets.activate('faction-wars', pace);
  const seed = 10001 + variant, pick = DT.makeRng(seed);
  const choices = ['training', 'lotus', 'thrill', 'secondWind', 'unity', 'frontline', 'backline', 'implants', 'duo_ace', 'duo_tag', 'duo_sync', 'duo_crest_link', 'crest'];
  const names = ['agumon', 'guilmon', 'hackmon', 'salamon'];
  function side(team) {
    const parts = [0, 1].map(part => {
      const owner = team * 2 + part + 2;
      const augs = [choices[(variant + part * 3 + team) % choices.length], 'duo_tag'];
      const units = names.map((line, i) => ({ line, stage: variant % 3, c: part * 7 + i, r: 4 + i % 3, owner,
        items: i === 0 ? ['emblem_ins', 'emblem_ins'] : i === 1 ? ['thief_glove'] : [] }));
      return { owner, augs, units, partnerTop: variant + part * 5,
        buffs: { as: 0.05 * (part + 1), armor: 3 + team, mana: 11 + part } };
    });
    return { units: parts.flatMap(p => p.units), parts, cols: 14, counts: DT.Duos.teamCounts(parts) };
  }
  const a = side(0), b = side(1);
  const cb = new DT.Combat(a, b, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const entries = a => a.units.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, owner: u.owner, items: u.items }));
  const effects = a => ({ parts: a.parts.map(p => ({ owner: p.owner, augments: p.augs, partnerTop: p.partnerTop,
    buffs: { attackSpeed: p.buffs.as, armor: p.buffs.armor, mana: p.buffs.mana } })) });
  fixtures.push({ name: `faction-duos/${variant}`, variant: `faction-wars/${pace}`, seed, cols: 14,
    sideA: entries(a), sideB: entries(b), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    setup: { sideA: effects(a), sideB: effects(b) }, traitCounts: cb.traitCounts });
}
DT.Sets.activate('fates', 'hyper');
const fatesAugments = Object.entries(DT.AUGMENT).filter(([id, a]) => id.startsWith('fat_') && Object.keys(a.hooks || {}).some(k => DT.Sets.COMBAT_HOOKS.includes(k) || k === 'onCountTraits')).map(([id]) => id);
for (const augment of fatesAugments) for (const duos of [false, true]) for (const stage of [0, 1]) {
  const pace = duos ? 'normal' : 'hyper'; DT.Sets.activate('fates', pace);
  const cols = duos ? 14 : 7, seed = 23000 + fixtures.length, pick = DT.makeRng(seed);
  const lunar = DT.SHOP_LINES.filter(l => l.traits.includes('LUNAR')).slice(0, 3);
  const lineup = lunar.concat(DT.SHOP_LINES.filter(l => !lunar.includes(l)).slice(0, 3));
  function side(team) {
    const parts = Array.from({ length: duos ? 2 : 1 }, (_, partner) => {
      const owner = team * 2 + partner + 3;
      const augs = partner === 0 ? [augment] : ['fat_crowns', 'fat_soulmates', 'duo_tag'];
      const units = lineup.map((line, i) => ({ line: line.id, stage, c: (duos ? partner * 7 : 0) + i, r: 4 + i % 3, owner,
        items: i % 2 ? [] : [DT.CORE_ITEMS[Math.floor(pick() * DT.CORE_ITEMS.length)]] }));
      units[0].mods = { partner: true, trait: lineup[0].traits[0], bonus: ['hp', 'ap', 'ad', 'mana'][fixtures.length % 4] };
      units[0].bonusTraits = { [units[0].mods.trait]: 1 };
      return { owner, augs, units };
    });
    return { units: parts.flatMap(p => p.units), cols, ...(duos ? { parts, counts: DT.Duos.teamCounts(parts) } : { augs: parts[0].augs }) };
  }
  const a = side(0), b = side(1);
  const cb = new DT.Combat(a, b, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.units.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items, owner: u.owner,
    bonusTraits: u.bonusTraits, partner: u.mods?.partner ? { bonus: u.mods.bonus, trait: u.mods.trait } : null }));
  const effects = a => duos ? { parts: a.parts.map(p => ({ owner: p.owner, augments: p.augs })) } : { augments: a.augs };
  fixtures.push({ name: `fates-augment/${augment}/${duos}/${stage}`, variant: `fates/${pace}`, seed, cols,
    sideA: convert(a), sideB: convert(b), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts, setup: { sideA: effects(a), sideB: effects(b) } });
}
// Every Galaxies custom ability with its production mechanic callbacks and no traits/items hooks.
DT.Sets.activate('galaxies', 'hyper');
const galaxiesKinds = [...new Set(DT.SHOP_LINES.map(l => l.type))].filter(kind => !kinds.includes(kind));
for (const kind of galaxiesKinds) for (const stage of [0, 1, 2]) for (const cols of [7, 14]) {
  const pace = cols === 7 ? 'normal' : 'hyper'; DT.Sets.activate('galaxies', pace);
  const primary = DT.SHOP_LINES.find(l => l.type === kind);
  const galaxy = stage === 0 ? null : stage === 1 ? 'datastream' : 'darkocean';
  const critItem = Object.keys(DT.ITEMS).find(id => DT.ITEMS[id].component && DT.ITEMS[id].stats.crit);
  let accepted = false;
  for (let attempt = 0; attempt < 12 && !accepted; attempt++) {
    const seed = 30000 + galaxiesKinds.indexOf(kind) * 6 + stage * 2 + (cols === 14 ? 1 : 0) + attempt * 100000, pick = DT.makeRng(seed);
    const entries = main => Array.from({ length: 5 }, (_, i) => ({ line: main && i === 0 ? primary.id : DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)].id,
      stage, c: i + 1, r: i === 0 ? 6 : 4 + i % 2, items: i === 0 && critItem ? [critItem] : [] }));
    const sideA = entries(true), sideB = entries(false);
    const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed, info: { mech: { galaxy } } });
    const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
    while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
    if (!cb.events.some(e => e.type === 'cast' && e.at === 0)) continue;
    accepted = true;
    const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
    fixtures.push({ name: `galaxies-ability/${kind}/${stage}/${cols}`, variant: `galaxies/${pace}`, seed, cols,
      sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, meeps: cb.teamFx.map(fx => fx.meep || 0),
      setup: { sideA: { noTraits: true }, sideB: { noTraits: true }, galaxy } });
  }
  if (!accepted) throw new Error(`Unable to exercise Galaxies ability ${kind}/${stage}/${cols}`);
}

// Every Galaxies trait breakpoint with mixed opponents and production ability hooks.
DT.Sets.activate('galaxies', 'hyper');
const galaxyTraits = Object.entries(DT.TRAITS).map(([id,t]) => [id, [...t.bp]]);
for (const [id, bp] of galaxyTraits) for (let level = 0; level < bp.length; level++) {
  const pace = level % 2 ? 'normal' : 'hyper', cols = level % 2 ? 14 : 7;
  DT.Sets.activate('galaxies', pace);
  const seed = 73000 + fixtures.length, pick = DT.makeRng(seed);
  const components = Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component);
  const own = DT.SHOP_LINES.filter(l => l.traits.includes(id)).slice(0, bp[level]);
  const selected = [];
  while (selected.length < Math.max(4, own.length)) {
    const l = DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)];
    if (!selected.includes(l)) selected.push(l);
  }
  const entries = lines => lines.map((l,i) => ({ line:l.id, stage:level % 3, c:i % cols, r:4 + Math.floor(i/cols),
    items:i % 2 ? [] : [components[Math.floor(pick()*components.length)]] }));
  const sideA=entries(own), sideB=entries(selected);
  if (own.length < bp[level]) sideA[0].bonusTraits = {[id]:bp[level]-own.length};
  const bounty = Object.fromEntries(own.map((l,i)=>[l.id,i*4]));
  const cb = new DT.Combat({units:sideA,cols,mech:{bounty}}, {units:sideB,cols}, {seed});
  if (DT.traitLevel(id,cb.traitCounts[0][id]) !== level+1) throw new Error(`Missing Galaxies breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}]; let tick=0;
  while (!cb.done && tick<2000) {cb.step();tick++;if(tick%15===0 || cb.done) samples.push({tick,units:snapshots(cb)});}
  if (id === 'XR') {
    const pilots = samples[0].units.filter(u=>u.piloting);
    if (pilots.length !== 2 || pilots.some(p=>!samples.some(s=>s.units[p.id].alive && !s.units[p.id].piloting)))
      throw new Error('DigiXros reference must exercise fusion and both pilot ejections');
  }
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits}));
  fixtures.push({name:`galaxies-trait/${id}/${level}`,variant:`galaxies/${pace}`,seed,cols,
    sideA:convert(sideA),sideB:convert(sideB),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,
    traitCounts:cb.traitCounts,meeps:cb.teamFx.map(f=>f.meep||0),pirateKills:cb.teamFx.map(f=>f.pirate||0),
    setup:{sideA:{galaxies:{bounty}},sideB:{}}});
}

DT.Sets.activate('galaxies', 'hyper');
const galaxiesHookItems = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].hooks);
const galaxyItemCases = galaxiesHookItems.flatMap(item => [0, 2].map(stage => ({ item, stage }))).concat(Array.from({ length: 8 }, (_, n) => ({ item: null, stage: n % 3 })));
for (const { item, stage } of galaxyItemCases) {
  const pace = stage === 0 ? 'normal' : 'hyper';
  DT.Sets.activate('galaxies', pace);
  const cols = stage === 2 ? 14 : 7, seed = 91000 + fixtures.length, pick = DT.makeRng(seed);
  const candidates = DT.SHOP_LINES;
  const entries = () => Array.from({ length: 6 }, (_, i) => ({ line: candidates[Math.floor(pick() * candidates.length)].id,
    stage, c: i, r: 4 + i % 2,
    items: item ? (i === 0 ? [item, item] : [item]) : Array.from({ length: 3 }, () => galaxiesHookItems[Math.floor(pick() * galaxiesHookItems.length)]) }));
  const sideA = entries(), sideB = entries();
  const cb = new DT.Combat({ units: sideA, cols }, { units: sideB, cols }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `galaxies-item/${item || 'mixed'}/${stage}/${seed}`, variant: `galaxies/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts });
}

DT.Sets.activate('galaxies', 'hyper');
const galaxyAugments = Object.entries(DT.AUGMENT).filter(([id,a]) => id.startsWith('gal_') && Object.keys(a.hooks || {}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k) || k === 'onCountTraits')).map(([id])=>id);
for (const augment of galaxyAugments) for (const duos of [false,true]) for (const stage of [0,2]) {
  const pace = duos ? 'normal' : 'hyper', cols = duos ? 14 : 7;
  DT.Sets.activate('galaxies',pace);
  const seed=97000+fixtures.length,pick=DT.makeRng(seed);
  function side(team) {
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes('TK')).slice(0,3);
      chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      return {owner,augs:part===0?[augment,'duo_tag']:[augment,'duo_sync'],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%3,owner,
        items:[galaxiesHookItems[Math.floor(pick()*galaxiesHookItems.length)]]}))};
    });
    return duos ? {parts,units:parts.flatMap(p=>p.units),cols,counts:DT.Duos.teamCounts(parts)} : {units:parts[0].units,augs:[augment],cols};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
  const effects=a=>duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))}:{augments:a.augs};
  fixtures.push({name:`galaxies-augment/${augment}/${duos}/${stage}`,variant:`galaxies/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:effects(a),sideB:effects(b)}});
}

// Every Gizmos custom ability with production mechanic callbacks and no traits/items hooks.
DT.Sets.activate('gizmos', 'hyper');
const gizmosKinds = [...new Set(DT.SHOP_LINES.map(l => l.type))].filter(kind => !kinds.includes(kind));
for (const kind of gizmosKinds) for (const stage of [0, 1, 2]) for (const cols of [7, 14]) {
  const pace = cols === 7 ? 'normal' : 'hyper'; DT.Sets.activate('gizmos', pace);
  const primary = DT.SHOP_LINES.find(l => l.type === kind);
  const galaxy = null;
  const critItem = Object.keys(DT.ITEMS).find(id => DT.ITEMS[id].component && DT.ITEMS[id].stats.crit);
  let accepted = false;
  for (let attempt = 0; attempt < 12 && !accepted; attempt++) {
    const seed = 130000 + gizmosKinds.indexOf(kind) * 6 + stage * 2 + (cols === 14 ? 1 : 0) + attempt * 100000, pick = DT.makeRng(seed);
    const entries = main => Array.from({ length: 5 }, (_, i) => ({ line: main && i === 0 ? primary.id : DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)].id,
      stage, c: i + 1, r: i === 0 ? 6 : 4 + i % 2, items: i === 0 && critItem ? [critItem] : [] }));
    const sideA = entries(true), sideB = entries(false);
    const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed, info: { mech: { galaxy } } });
    const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
    while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
    if (!cb.events.some(e => e.type === 'cast' && e.at === 0)) continue;
    accepted = true;
    const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
    fixtures.push({ name: `gizmos-ability/${kind}/${stage}/${cols}`, variant: `gizmos/${pace}`, seed, cols,
      sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, meeps: cb.teamFx.map(fx => fx.meep || 0),
      setup: { sideA: { noTraits: true }, sideB: { noTraits: true }, galaxy } });
  }
  if (!accepted) throw new Error(`Unable to exercise Gizmos ability ${kind}/${stage}/${cols}`);
}

DT.Sets.activate('gizmos', 'hyper');
const gizmosHookItems = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].hooks);
const gizmoItemCases = gizmosHookItems.flatMap(item => [0, 2].map(stage => ({ item, stage }))).concat(Array.from({ length: 8 }, (_, n) => ({ item: null, stage: n % 3 })));
for (const { item, stage } of gizmoItemCases) {
  const pace = stage === 0 ? 'normal' : 'hyper';
  DT.Sets.activate('gizmos', pace);
  const cols = stage === 2 ? 14 : 7, seed = 140000 + fixtures.length, pick = DT.makeRng(seed);
  const candidates = DT.SHOP_LINES;
  const entries = () => Array.from({ length: 6 }, (_, i) => ({ line: candidates[Math.floor(pick() * candidates.length)].id,
    stage, c: i, r: 4 + i % 2,
    items: item ? (i === 0 ? [item, item] : [item]) : Array.from({ length: 3 }, () => gizmosHookItems[Math.floor(pick() * gizmosHookItems.length)]) }));
  const sideA = entries(), sideB = entries();
  const cb = new DT.Combat({ units: sideA, cols, noTraits: true }, { units: sideB, cols, noTraits: true }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `gizmos-item/${item || 'mixed'}/${stage}/${seed}`, variant: `gizmos/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, setup: {sideA: {noTraits:true},sideB:{noTraits:true}},
    traitCounts: cb.traitCounts });
}


// Every Gizmos trait breakpoint with mixed opponents and production ability hooks.
DT.Sets.activate('gizmos', 'hyper');
const gizmoTraits = Object.entries(DT.TRAITS).map(([id,t]) => [id, [...t.bp]]);
for (const [id, bp] of gizmoTraits) for (let level = 0; level < bp.length; level++) for (const mutation of (id === 'MUT' ? ['cyber','voracious','voidborne','adrenal','synaptic','leech','meta',null] : [null])) for (const formation of [0,1]) {
  const pace = level % 2 ? 'normal' : 'hyper', cols = level % 2 ? 14 : 7;
  DT.Sets.activate('gizmos', pace);
  const seed = 180000 + fixtures.length, pick = DT.makeRng(seed);
  const components = Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component);
  const own = DT.SHOP_LINES.filter(l => l.traits.includes(id)).slice(0, bp[level]);
  const selected = [];
  while (selected.length < Math.max(4, own.length)) {
    const l = DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)];
    if (!selected.includes(l)) selected.push(l);
  }
  const entries = lines => lines.map((l,i) => ({ line:l.id, stage:level % 3, c:i % cols, r:formation ? 4+i%3 : 4+Math.floor(i/cols),
    items:i % 2 ? [] : [formation && id !== 'JUNK' ? gizmosHookItems[Math.floor(pick()*gizmosHookItems.length)] : components[Math.floor(pick()*components.length)]] }));
  const sideA=entries(own), sideB=entries(selected);
  if (own.length < bp[level]) sideA[0].bonusTraits = {[id]:bp[level]-own.length};
  if (id === 'GLUT') sideA[0].mods = {glutHp:700,glutAp:35};
  const cb = new DT.Combat({units:sideA,cols}, {units:sideB,cols}, {seed, info:{mech:{mutation}}});
  if (DT.traitLevel(id,cb.traitCounts[0][id]) !== level+1) throw new Error(`Missing Gizmos breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}]; let tick=0;
  while (!cb.done && tick<2000) {cb.step();tick++;if(tick%15===0 || cb.done) samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits,gluttonHealth:u.mods?.glutHp||0,gluttonAbilityPower:u.mods?.glutAp||0}));
  fixtures.push({name:`gizmos-trait/${id}/${level}/${mutation}/${formation}`,variant:`gizmos/${pace}`,seed,cols,
    sideA:convert(sideA),sideB:convert(sideB),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,
    traitCounts:cb.traitCounts,meeps:cb.teamFx.map(f=>f.meep||0),pirateKills:cb.teamFx.map(f=>f.pirate||0),
    setup:{sideA:{},sideB:{},mutation}});
}


DT.Sets.activate('gizmos', 'hyper');
const gizmoAugments = Object.entries(DT.AUGMENT).filter(([id,a]) => id.startsWith('giz_') && Object.keys(a.hooks || {}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k) || k === 'onCountTraits') || id === 'giz_a_injection').map(([id])=>id);
for (const augment of gizmoAugments) for (const duos of [false,true]) for (const stage of [0,2]) {
  const pace = duos ? 'normal' : 'hyper', cols = duos ? 14 : 7;
  DT.Sets.activate('gizmos',pace);
  const seed=210000+fixtures.length,pick=DT.makeRng(seed);
  function side(team) {
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const required = {giz_a_runic:'ARC',giz_a_overload:'XA',giz_a_sosmall:'PET',giz_a_cutthroat:'ASN',giz_a_shrug:'BRU',giz_a_spellblade:'ARC',giz_a_stopwatch:'CLOCK',giz_a_smoke:'ASN',giz_a_injection:'XA',giz_a_reserves:'PIR'}[augment] || 'JUNK';
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes(required)).slice(0,4);
      while(chosen.length<6) chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      return {owner,mech:{goldStart:part===0?12:80},augs:part===0?[augment,'duo_tag']:[augment,'duo_sync'],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%3,owner,
        items:i%3===0?[]:Array.from({length:i%3},()=>gizmosHookItems[Math.floor(pick()*gizmosHookItems.length)])}))};
    });
    return duos ? {parts,augs:parts.flatMap(p=>p.augs),units:parts.flatMap(p=>p.units),cols,counts:DT.Duos.teamCounts(parts)} : {units:parts[0].units,augs:[augment],cols};
  }
  const a=side(0),b=side(1);
  a.mech={goldStart:stage===0?12:80};b.mech={goldStart:20};
  const cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
  const effects=a=>duos?{augments:a.augs,parts:a.parts.map(p=>({owner:p.owner,augments:p.augs,gizmos:p.mech}))}:{augments:a.augs};
  fixtures.push({name:`gizmos-augment/${augment}/${duos}/${stage}`,variant:`gizmos/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:{...effects(a),gizmos:a.mech},sideB:{...effects(b),gizmos:b.mech}}});
}


// Every Reckoning custom ability with production mechanic callbacks and no traits/items hooks.
DT.Sets.activate('reckoning', 'hyper');
const reckoningCasters = DT.SHOP_LINES.filter(l => !kinds.includes(l.type)).map(l=>l.id);
for (const caster of reckoningCasters) for (const stage of [0, 1, 2]) for (const cols of [7, 14]) {
  const pace = cols === 7 ? 'normal' : 'hyper'; DT.Sets.activate('reckoning', pace);
  const primary = DT.SHOP_LINES.find(l => l.id === caster), kind = primary.type;
  const roundType = stage === 1 ? 'pve' : 'pvp', roundStage = stage + 3;
  const critItem = Object.keys(DT.ITEMS).find(id => DT.ITEMS[id].component && DT.ITEMS[id].stats.crit);
  let accepted = false;
  for (let attempt = 0; attempt < 12 && !accepted; attempt++) {
    const seed = 250000 + reckoningCasters.indexOf(caster) * 6 + stage * 2 + (cols === 14 ? 1 : 0) + attempt * 100000, pick = DT.makeRng(seed);
    const entries = main => Array.from({ length: 5 }, (_, i) => ({ line: main && i === 0 ? primary.id : DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)].id,
      stage, c: i + 1, r: i === 0 ? 6 : 4 + i % 2, items: i === 0 && critItem ? [critItem] : [] }));
    const sideA = entries(true), sideB = entries(false);
    const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed, info: { type:roundType, stage:roundStage } });
    const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
    while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
    if (!cb.events.some(e => e.type === 'cast' && e.at === 0)) continue;
    accepted = true;
    const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
    fixtures.push({ name: `reckoning-ability/${caster}/${stage}/${cols}`, variant: `reckoning/${pace}`, seed, cols,
      sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, meeps: cb.teamFx.map(fx => fx.meep || 0),
      setup: { sideA: { noTraits: true }, sideB: { noTraits: true }, roundType, roundStage } });
  }
  if (!accepted) throw new Error(`Unable to exercise Reckoning ability ${kind}/${stage}/${cols}`);
}


// Every Reckoning trait breakpoint with mixed opponents and production ability hooks.
DT.Sets.activate('reckoning', 'hyper');
const reckoningTraits = Object.entries(DT.TRAITS).map(([id,t]) => [id, [...t.bp]]);
for (const [id, bp] of reckoningTraits) for (let level = 0; level < bp.length; level++) for (const formation of [0,1]) {
  const pace = level % 2 ? 'normal' : 'hyper', cols = level % 2 ? 14 : 7;
  DT.Sets.activate('reckoning', pace);
  const seed = 290000 + fixtures.length, pick = DT.makeRng(seed);
  const components = Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component);
  const shadow = Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].cat === 'shadow' && !DT.ITEMS[id].hooks);
  const own = DT.SHOP_LINES.filter(l => l.traits.includes(id)).slice(0, bp[level]);
  const selected = [];
  while (selected.length < Math.max(4, own.length)) {
    const l = DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)];
    if (!selected.includes(l)) selected.push(l);
  }
  const entries = lines => lines.map((l,i) => ({ line:l.id, stage:level % 3, c:i % cols, r:formation ? 4+i%3 : 4+Math.floor(i/cols),
    items:i % 2 ? [] : [formation && shadow.length ? shadow[Math.floor(pick()*shadow.length)] : components[Math.floor(pick()*components.length)]] }));
  const sideA=entries(own), sideB=entries(selected);
  if (own.length < bp[level]) sideA[0].bonusTraits = {[id]:bp[level]-own.length};
  const cb = new DT.Combat({units:sideA,cols}, {units:sideB,cols}, {seed});
  if (DT.traitLevel(id,cb.traitCounts[0][id]) !== level+1) throw new Error(`Missing Reckoning breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}]; let tick=0;
  while (!cb.done && tick<2000) {cb.step();tick++;if(tick%15===0 || cb.done) samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits,gluttonHealth:u.mods?.glutHp||0,gluttonAbilityPower:u.mods?.glutAp||0}));
  fixtures.push({name:`reckoning-trait/${id}/${level}/${formation}`,variant:`reckoning/${pace}`,seed,cols,
    sideA:convert(sideA),sideB:convert(sideB),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,
    traitCounts:cb.traitCounts,meeps:cb.teamFx.map(f=>f.meep||0),pirateKills:cb.teamFx.map(f=>f.pirate||0),
    setup:{sideA:{},sideB:{}}});
}



DT.Sets.activate('reckoning','hyper');
const royalRivals=DT.SHOP_LINES.filter(l=>l.traits.includes('RIV'));
for(const rival of royalRivals) for(const both of [false,true]) for(const stage of [0,2]) {
  const own=both?royalRivals:[rival];
  const foes=DT.SHOP_LINES.filter(l=>l.traits.some(t=>rival.rivals.includes(t))).slice(0,4);
  const entries=lines=>lines.map((l,i)=>({line:l.id,stage,c:i+1,r:4,items:[]}));
  const a=entries(own),b=entries(foes),seed=310000+fixtures.length;
  const cb=new DT.Combat({units:a},{units:b},{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
  fixtures.push({name:`reckoning-rival/${rival.id}/${both}/${stage}`,variant:'reckoning/hyper',seed,cols:7,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts});
}

DT.Sets.activate('reckoning', 'hyper');
const reckoningHookItems = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].hooks);
const reckoningItemCases = reckoningHookItems.flatMap(item => [0, 1, 2].map(stage => ({ item, stage }))).concat(Array.from({ length: 12 }, (_, n) => ({ item: null, stage: n % 3 })));
for (const { item, stage } of reckoningItemCases) {
  const pace = stage === 0 ? 'normal' : 'hyper';
  DT.Sets.activate('reckoning', pace);
  const cols = stage === 2 ? 14 : 7, seed = 340000 + fixtures.length, pick = DT.makeRng(seed);
  const candidates = DT.SHOP_LINES;
  const entries = () => Array.from({ length: 6 }, (_, i) => ({ line: candidates[Math.floor(pick() * candidates.length)].id,
    stage, c: i, r: 4 + i % 2,
    items: item ? (i === 0 ? [item, item] : [item]) : Array.from({ length: 3 }, () => reckoningHookItems[Math.floor(pick() * reckoningHookItems.length)]) }));
  const sideA = entries(), sideB = entries();
  const augs = stage === 1 ? ['rec_a_xanti'] : [];
  const cb = new DT.Combat({ units: sideA, cols, augs }, { units: sideB, cols, augs }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `reckoning-item/${item || 'mixed'}/${stage}/${seed}`, variant: `reckoning/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, productionTraits: true,
    traitCounts: cb.traitCounts, setup:{sideA:{augments:augs},sideB:{augments:augs}} });
}


DT.Sets.activate('reckoning', 'hyper');
const reckoningAugments = Object.entries(DT.AUGMENT).filter(([id,a]) => id.startsWith('rec_') && Object.keys(a.hooks || {}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k) || k === 'onCountTraits')).map(([id])=>id);
for (const augment of reckoningAugments) for (const duos of [false,true]) for (const stage of [0,2]) {
  const pace = duos ? 'normal' : 'hyper', cols = duos ? 14 : 7;
  DT.Sets.activate('reckoning',pace);
  const seed=380000+fixtures.length,pick=DT.makeRng(seed);
  function side(team) {
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes('CHI')).slice(0,3);
      chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      return {owner,augs:part===0?[augment,'duo_tag']:[augment,'duo_sync'],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%3,owner,
        items:Array.from({length:2},()=>reckoningHookItems[Math.floor(pick()*reckoningHookItems.length)])}))};
    });
    return duos ? {parts,augs:parts.flatMap(p=>p.augs),units:parts.flatMap(p=>p.units),cols,counts:DT.Duos.teamCounts(parts)} : {units:parts[0].units,augs:[augment],cols};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
  const effects=a=>duos?{augments:a.augs,parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))}:{augments:a.augs};
  fixtures.push({name:`reckoning-augment/${augment}/${duos}/${stage}`,variant:`reckoning/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:effects(a),sideB:effects(b)}});
}


// A summoned item can append an observer while a cast is being dispatched.
DT.Sets.activate('reckoning','hyper');
{
  const previous=DT.SET.hooks.onAnyCast;
  let added=false;
  DT.SET.hooks.onAnyCast=c=>{
    if(added)return;added=true;
    const source=c.units[0],cell=c.freeCellNear(source);
    if(!cell)throw new Error('Missing observer probe spawn cell');
    c.summon(source.team,source.line.id,0,cell.c,cell.r,{items:['rec_s_mana_zap']});
  };
  const line=DT.SHOP_LINES.find(l=>l.type==='nuke').id;
  const entries=()=>[0,1,2].map(i=>({line,stage:0,c:i+1,r:4,items:[]}));
  const a=entries(),b=entries(),seed=412343;
  const cb=new DT.Combat({units:a,noTraits:true},{units:b,noTraits:true},{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  if(!added||cb.units.length!==7)throw new Error('Observer probe did not summon');
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
  fixtures.push({name:'reckoning-observer-summon',variant:'reckoning/hyper',seed,cols:7,sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,
    observerSummonProbe:true,setup:{sideA:{noTraits:true},sideB:{noTraits:true}}});
  if(previous)DT.SET.hooks.onAnyCast=previous;else delete DT.SET.hooks.onAnyCast;
}

// Every Dragonlands custom ability with production mechanic callbacks and no traits/items hooks.
DT.Sets.activate('dragonlands', 'hyper');
const copyOutcomes = new Set();
const dragonlandsKinds = [...new Set(DT.SHOP_LINES.map(l => l.type))].filter(kind => !kinds.includes(kind));
for (const kind of dragonlandsKinds) for (const stage of [0, 1, 2]) for (const cols of [7, 14]) for (const variation of [0,1,2]) {
  const pace = cols === 7 ? 'normal' : 'hyper'; DT.Sets.activate('dragonlands', pace);
  const primary = DT.SHOP_LINES.find(l => l.type === kind);
  const galaxy = null;
  const critItem = Object.keys(DT.ITEMS).find(id => DT.ITEMS[id].component && DT.ITEMS[id].stats.crit);
  let accepted = false;
  for (let attempt = 0; attempt < 12 && !accepted; attempt++) {
    const seed = 450000 + dragonlandsKinds.indexOf(kind) * 6 + stage * 2 + (cols === 14 ? 1 : 0) + variation * 50000 + attempt * 100000, pick = DT.makeRng(seed);
    const entries = main => Array.from({ length: 5 }, (_, i) => ({ line: main && i === 0 ? primary.id : DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)].id,
      stage, c: i + 1, r: i === 0 ? 6 : 4 + i % 2, items: i === 0 && critItem ? [critItem] : [] }));
    const sideA = entries(true), sideB = entries(false);
    const cb = new DT.Combat({ units: sideA, noTraits: true, cols }, { units: sideB, noTraits: true, cols }, { seed, info: { mech: { galaxy } } });
    const samples = [{ tick: 0, units: snapshots(cb) }]; let tick = 0;
    while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
    if (!cb.events.some(e => e.type === 'cast' && e.at === 0)) continue;
    for (const event of cb.events) if (event.type === 'text' && ['Blast','Explosion','Time Stop','Barrage','Restore'].includes(event.text)) copyOutcomes.add(event.text);
    accepted = true;
    const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
    fixtures.push({ name: `dragonlands-ability/${kind}/${stage}/${cols}/${variation}`, variant: `dragonlands/${pace}`, seed, cols,
      sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, meeps: cb.teamFx.map(fx => fx.meep || 0),
      setup: { sideA: { noTraits: true }, sideB: { noTraits: true }, galaxy } });
  }
  if (!accepted) throw new Error(`Unable to exercise Dragonlands ability ${kind}/${stage}/${cols}/${variation}`);
}

if(copyOutcomes.size!==5)throw new Error('Copycat fixtures must exercise all five random abilities');
DT.Sets.activate('dragonlands', 'hyper');
const dragonlandsHookItems = Object.keys(DT.ITEMS).filter(id => DT.ITEMS[id].hooks);
const dragonItemCases = dragonlandsHookItems.flatMap(item => [0, 1, 2].map(stage => ({ item, stage }))).concat(Array.from({ length: 8 }, (_, n) => ({ item: null, stage: n % 3 })));
for (const { item, stage } of dragonItemCases) {
  const pace = stage === 0 ? 'normal' : 'hyper';
  DT.Sets.activate('dragonlands', pace);
  const cols = stage === 2 ? 14 : 7, seed = 490000 + fixtures.length, pick = DT.makeRng(seed);
  const candidates = DT.SHOP_LINES;
  const entries = () => Array.from({ length: 6 }, (_, i) => ({ line: candidates[Math.floor(pick() * candidates.length)].id,
    stage, c: i, r: 4 + i % 2,
    items: item ? (i === 0 ? [item, item] : [item]) : Array.from({ length: 3 }, () => dragonlandsHookItems[Math.floor(pick() * dragonlandsHookItems.length)]) }));
  const sideA = entries(), sideB = entries();
  const bank = stage === 0 ? 0 : stage === 1 ? 7 : 30;
  const cb = new DT.Combat({ units: sideA, cols, noTraits:true, mech:{bank} }, { units: sideB, cols, noTraits:true, mech:{bank} }, { seed });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items }));
  fixtures.push({ name: `dragonlands-item/${item || 'mixed'}/${stage}/${seed}`, variant: `dragonlands/${pace}`, seed, cols,
    sideA: convert(sideA), sideB: convert(sideB), samples, tick, winner: cb.winner, time: cb.t, dragonRewards:cb.teamFx.map(f=>({gold:f.gold||0,axe:f.axe||0,investor:f.investor||0})),
    setup:{sideA:{noTraits:true,dragonlands:{bank}},sideB:{noTraits:true,dragonlands:{bank}}},
    traitCounts: cb.traitCounts });
}


// Every Dragonlands trait breakpoint with mixed opponents and production ability hooks.
DT.Sets.activate('dragonlands', 'hyper');
const dragonTraits = Object.entries(DT.TRAITS).map(([id,t]) => [id, [...t.bp]]);
for (const [id, bp] of dragonTraits) for (let level = 0; level < bp.length; level++) for (const mirage of (id === 'MIRAGE' ? ['dancer','warlord','overload','executioner',null] : [null])) for (const formation of [0,1]) {
  const pace = formation ? 'normal' : 'hyper', cols = formation ? 14 : 7;
  DT.Sets.activate('dragonlands', pace);
  const seed = 530000 + fixtures.length, pick = DT.makeRng(seed);
  const components = Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component);
  const own=[]; let contributed=0;
  for(const line of DT.SHOP_LINES.filter(l=>l.traits.includes(id))) {
    const weight=line.mark===id?3:1;
    if(contributed+weight<=bp[level]){own.push(line);contributed+=weight;}
  }
  if(!own.length){const line=DT.SHOP_LINES.find(l=>l.traits.includes(id));if(!line)throw new Error(`No board for ${id}/${level}`);own.push(line);contributed=line.mark===id?3:1;}
  const selected = [];
  while (selected.length < Math.max(4, own.length)) {
    const l = DT.SHOP_LINES[Math.floor(pick() * DT.SHOP_LINES.length)];
    if (!selected.includes(l)) selected.push(l);
  }
  const entries = lines => lines.map((l,i) => ({ line:l.id, stage:level % 3, c:i % cols, r:formation ? 4+i%3 : 4+Math.floor(i/cols),
    items:i % 2 ? [] : [formation ? dragonlandsHookItems[Math.floor(pick()*dragonlandsHookItems.length)] : components[Math.floor(pick()*components.length)]] }));
  const sideA=entries(own), sideB=entries(selected);
  if(id==='MONO'){
    sideA[0].c=2; sideA[0].r=4;
    const ally=DT.SHOP_LINES.find(l=>!l.mark).id;
    sideA.push({line:ally,stage:1,c:4,r:4,items:[]},{line:ally,stage:1,c:3,r:6,items:[]},
      {line:sideA[0].line,stage:0,c:0,r:5,items:[]});
  }
  if(contributed<bp[level])sideA[0].bonusTraits={[id]:bp[level]-contributed};
  const mech={hpFull:!!formation,bank:formation?25:7};
  const cb = new DT.Combat({units:sideA,cols,mech}, {units:sideB,cols,mech}, {seed, info:{mech:{mirage},type:'pvp',stage:4,pace}});
  if (DT.traitLevel(id,cb.traitCounts[0][id]) !== level+1) throw new Error(`Missing Dragonlands breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}]; let tick=0;
  while (!cb.done && tick<2000) {cb.step();tick++;if(tick%15===0 || cb.done) samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits,gluttonHealth:u.mods?.glutHp||0,gluttonAbilityPower:u.mods?.glutAp||0}));
  fixtures.push({name:`dragonlands-trait/${id}/${level}/${mirage}/${formation}`,variant:`dragonlands/${pace}`,seed,cols,
    sideA:convert(sideA),sideB:convert(sideB),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,
    traitCounts:cb.traitCounts,meeps:cb.teamFx.map(f=>f.meep||0),pirateKills:cb.teamFx.map(f=>f.pirate||0),
    dragonRewards:cb.teamFx.map(f=>({gold:f.gold||0,axe:f.axe||0,investor:f.investor||0,lagoon:f.lagoon||0,doots:f.doots||0,wish:f.wish||0})),
    setup:{sideA:{dragonlands:mech},sideB:{dragonlands:mech},mirage,roundType:'pvp',roundStage:4,pace}});
}




// Dragonlands augment effects, retained training kills and owner-scoped Duos state.
DT.Sets.activate('dragonlands', 'hyper');
const dragonAugments=Object.entries(DT.AUGMENT).filter(([id,a])=>id.startsWith('drg_')&&Object.keys(a.hooks||{}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k))).map(([id])=>id);
for(const augment of [...dragonAugments,'mixed']) for(const duos of [false,true]) for(const stage of [0,1,2]) {
  const pace=duos?'normal':'hyper',cols=duos?14:7;
  DT.Sets.activate('dragonlands',pace);
  const seed=570000+fixtures.length,pick=DT.makeRng(seed),noTraits=augment==='drg_built'&&stage===0;
  function side(team) {
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes('STONE')).slice(0,3);
      const dragon=DT.SHOP_LINES.find(l=>l.slots===2);
      chosen.push(dragon,stage===2?DT.SHOP_LINES.filter(l=>l.slots===2)[1]:dragon);
      chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      const augs=augment==='mixed'?['drg_training','drg_firstaid','drg_bigfriend','drg_final','drg_electro']:[augment];
      return {owner,mech:{ctKills:part?60:stage*12,bank:part?15:7,hpFull:stage===2},augs:[...augs,...(duos?[part?'duo_sync':'duo_tag']:[])],
        units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+[0,1,2,4,5,6][i],r:[4,4,5,4,4,7][i],owner,
          items:i%3===0?[]:[dragonlandsHookItems[Math.floor(pick()*dragonlandsHookItems.length)]]}))};
    });
    return duos?{parts,augs:parts.flatMap(p=>p.augs),units:parts.flatMap(p=>p.units),cols,counts:DT.Duos.teamCounts(parts),mech:parts[0].mech}
      :{units:parts[0].units,augs:parts[0].augs,cols,mech:parts[0].mech};
  }
  const a=side(0),b=side(1);if(noTraits){a.noTraits=true;b.noTraits=true;}
  const cb=new DT.Combat(a,b,{seed,info:{type:'pvp',stage:stage?4:0,pace,mech:{mirage:'executioner'}}});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
  const effects=a=>({augments:a.augs,dragonlands:a.mech,noTraits,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs,dragonlands:p.mech}))}:{})});
  fixtures.push({name:`dragonlands-augment/${augment}/${duos}/${stage}`,variant:`dragonlands/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    dragonRewards:cb.teamFx.map(f=>({gold:f.gold||0,axe:f.axe||0,investor:f.investor||0,lagoon:f.lagoon||0,doots:f.doots||0,wish:f.wish||0,ctKills:f.ctKills||0})),
    setup:{sideA:effects(a),sideB:effects(b),roundType:'pvp',roundStage:stage?4:0,pace,mirage:'executioner'}});
}



// Observe branch coverage without replacing the reference ability behavior.
DT.Sets.activate('arcane','hyper');
const arcaneBranches={rotate:new Set(),gamble:new Set()};
const arcaneOriginalCasts={};
for(const kind of ['arc_rotate','arc_gamble']) {
  const ability=DT.SET.abilities[kind];arcaneOriginalCasts[kind]=ability.cast;
  ability.cast=function(c,u,t,v) {
    if(kind==='arc_rotate')arcaneBranches.rotate.add((u.state.rot||0)%3);
    const observed=Object.create(c);
    observed.rng=()=>{const r=c.rng();if(kind==='arc_gamble')arcaneBranches.gamble.add(r<0.12?'jackpot':r<0.41?'flame':r<0.7?'punch':'chomp');return r;};
    return arcaneOriginalCasts[kind](observed,u,t,v);
  };
}
DT.Sets.activate('arcane','hyper');
const arcaneCasters=DT.SHOP_LINES.filter(l=>!kinds.includes(l.type)).map(l=>l.id);
for(const line of arcaneCasters) for(const stage of [0,1,2]) for(const cols of [7,14]) for(const row of [4,7]) {
  const pace=cols===7?'hyper':'normal';DT.Sets.activate('arcane',pace);
  let accepted=false;
  for(let attempt=0;attempt<20&&!accepted;attempt++) {
    const seed=640000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
    const entries=main=>Array.from({length:6},(_,i)=>({line:main&&i===0?line:DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id,
      stage,c:i,r:i===0?row:4+i%3,items:[]}));
    const a=entries(true),b=entries(false);
    const cb=new DT.Combat({units:a,cols,noTraits:true},{units:b,cols,noTraits:true},{seed});
    const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
    while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
    if(!cb.events.some(e=>e.type==='cast'&&e.at===0))continue;accepted=true;
    const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
    fixtures.push({name:`arcane-ability/${line}/${stage}/${cols}/${row}`,variant:`arcane/${pace}`,seed,cols,
      sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,setup:{sideA:{noTraits:true},sideB:{noTraits:true}}});
  }
  if(!accepted)throw new Error(`Arcane caster never cast ${line}/${stage}/${cols}/${row}`);
}

DT.Sets.activate('arcane','hyper');
const arcaneTraits=Object.entries(DT.TRAITS).map(([id,t])=>[id,[...t.bp]]);
for(const [id,bp] of arcaneTraits) for(let level=0;level<bp.length;level++) for(const formation of [0,1]) {
  const pace=formation?'normal':'hyper',cols=formation?14:7;DT.Sets.activate('arcane',pace);
  const seed=680000+fixtures.length,pick=DT.makeRng(seed);
  const components=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component&&!DT.COMPONENTS[id]?.special);
  const own=DT.SHOP_LINES.filter(l=>l.traits.includes(id)).slice(0,bp[level]);
  const foes=[];while(foes.length<Math.max(5,own.length)){const l=DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)];if(!foes.includes(l))foes.push(l);}
  const entries=lines=>lines.map((l,i)=>({line:l.id,stage:(level+formation)%3,c:i%cols,r:formation?4+i%4:4+Math.floor(i/cols),
    items:Array.from({length:i%3},()=>components[Math.floor(pick()*components.length)])}));
  const a=entries(own),b=entries(foes);
  if(own.length<bp[level])a[0].bonusTraits={[id]:bp[level]-own.length};
  const mech={arcSynHp:formation?350:0,arcSynAug:!!formation,arcChests:formation?15:0,arcJunk:formation?['flame','bots','rod','destruct','buster','spare','spare']:[]};
  const arcSponsor=formation?components.slice(0,3):[];
  const cb=new DT.Combat({units:a,cols,mech},{units:b,cols,mech},{seed,info:{mech:{arcSponsor}}});
  if(DT.traitLevel(id,cb.traitCounts[0][id])!==level+1)throw new Error(`Arcane missing breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits}));
  fixtures.push({name:`arcane-trait/${id}/${level}/${formation}`,variant:`arcane/${pace}`,seed,cols,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    arcaneRewards:[0,1].map(team=>({conquestKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.arcConK||0),0),coins:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.coins||0),0)})),
    setup:{sideA:{arcane:mech},sideB:{arcane:mech},arcSponsor}});
}



// Targeted trait interactions: every lab combination, Envoy activation gap,
// individual workshop parts, and repeated component self-upgrades.
DT.Sets.activate('arcane','hyper');
const arcaneSpecial=[];
const labs=DT.SHOP_LINES.filter(l=>l.exp);
for(let mask=1;mask<(1<<labs.length);mask++) {
  const chosen=labs.filter((_,i)=>mask&(1<<i));
  if(chosen.length===2||chosen.length===3)arcaneSpecial.push({name:`lab-${mask}`,lines:chosen.map(l=>l.id),bonus:{EXP:(chosen.length===2?3:5)-chosen.length}});
}
const envoys=DT.SHOP_LINES.filter(l=>l.envoy);
for(let mask=1;mask<(1<<envoys.length);mask++)arcaneSpecial.push({name:`envoy-${mask}`,lines:envoys.filter((_,i)=>mask&(1<<i)).map(l=>l.id)});
for(const part of ['flame','bots','rod','destruct','buster','spare','all'])arcaneSpecial.push({name:`workshop-${part}`,lines:['arc_chaosdromon'],mech:{arcJunk:part==='all'?['flame','bots','rod','destruct','buster','spare','spare']:[part]}});
for(const item of Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component&&!DT.COMPONENTS[id]?.special))arcaneSpecial.push({name:`scrap-${item}`,lines:DT.SHOP_LINES.filter(l=>l.traits.includes('SCR')).slice(0,4).map(l=>l.id),items:[item,item,item]});
for(const test of arcaneSpecial)for(const duos of [false,true]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate('arcane',pace);
  const seed=720000+fixtures.length,pick=DT.makeRng(seed),mech=test.mech||{};
  const a=test.lines.map((line,i)=>({line,stage:i%3,c:i,r:4+i%3,items:test.items||[],owner:0}));
  if(test.bonus)a[0].bonusTraits=test.bonus;
  const b=Array.from({length:Math.max(5,a.length)},(_,i)=>({line:DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id,stage:i%3,c:i,r:4+i%3,items:[],owner:2}));
  if(duos)for(const [team,list] of [[0,a],[1,b]])for(let i=0;i<3;i++)list.push({line:DT.SHOP_LINES.filter(l=>!l.envoy&&!l.exp)[i].id,stage:1,c:i+7,r:4+i,owner:team*2+1,items:[]});
  const side=(units,team)=>duos?{units,cols,mech,parts:[0,1].map(part=>({owner:team*2+part,units:units.filter(u=>u.owner===team*2+part),augs:[]}))}:{units,cols,mech};
  const sideA=side(a,0),sideB=side(b,1);
  if(duos){sideA.counts=DT.Duos.teamCounts(sideA.parts);sideB.counts=DT.Duos.teamCounts(sideB.parts);}
  const cb=new DT.Combat(sideA,sideB,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner,bonusTraits:u.bonusTraits}));
  const effects=side=>({arcane:mech,...(duos?{parts:side.parts.map(p=>({owner:p.owner,augments:[]}))}:{})});
  fixtures.push({name:`arcane-special/${test.name}/${duos}`,variant:`arcane/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    arcaneRewards:[0,1].map(team=>({conquestKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.arcConK||0),0),coins:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.coins||0),0)})),
    setup:{sideA:effects(sideA),sideB:effects(sideB)}});
}
for(const [kind,cast] of Object.entries(arcaneOriginalCasts))DT.SET.abilities[kind].cast=cast;
if(arcaneBranches.rotate.size!==3||arcaneBranches.gamble.size!==4)throw new Error('Arcane ability branches were not all exercised');



DT.Sets.activate('arcane','hyper');
const arcaneItemCoverage=new Set();
const arcaneHookItems=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].hooks);
const arcaneItemCases=arcaneHookItems.flatMap(item=>[0,1,2].map(stage=>({name:item,items:[item],stage})));
for(const item of arcaneHookItems.filter(id=>id.startsWith('arc_k_')&&!id.endsWith('2')))for(const reversed of [false,true])arcaneItemCases.push({name:`order-${item}-${reversed}`,items:reversed?[item+'2',item]:[item,item+'2'],stage:1});
for(let i=0;i<12;i++)arcaneItemCases.push({name:`mixed-${i}`,items:null,stage:i%3});
for(const test of arcaneItemCases){
  const stage=test.stage,cols=stage===2?14:7,pace=cols===14?'normal':'hyper';DT.Sets.activate('arcane',pace);
  const seed=790000+fixtures.length,pick=DT.makeRng(seed);
  const syn=DT.SHOP_LINES.filter(l=>l.traits.includes('SYN')),other=DT.SHOP_LINES.filter(l=>!l.traits.includes('SYN'));
  const entries=()=>Array.from({length:6},(_,i)=>{
    const pool=i%2===0?syn:other,line=pool[Math.floor(pick()*pool.length)].id;
    const gear=test.items?[...test.items]:Array.from({length:2},()=>arcaneHookItems[Math.floor(pick()*arcaneHookItems.length)]);
    if(test.items?.length===1&&i%2===0)gear.push(test.items[0]);
    return {line,stage,c:i,r:4+i%4,items:gear};
  });
  const a=entries(),b=entries(),mech={arcSynHp:100,arcChests:4,arcJunk:['bots','destruct']},roundStage=stage===0?0:stage===1?1:6;
  const cb=new DT.Combat({units:a,cols,mech},{units:b,cols,mech},{seed,info:{stage:roundStage}});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
  for(const u of cb.units) {
    for(const flag of ['ascT','zhonya','mantle'])if(u.state[flag])arcaneItemCoverage.add(flag);
    if(u.state.doom>13)arcaneItemCoverage.add('doom-repeat');
    if(u.state.bountyGold>0)arcaneItemCoverage.add('bounty');
    if(u.state.cb?.tankOn)arcaneItemCoverage.add('tank');
    if(u.state.cb?.ripped)arcaneItemCoverage.add('ripper');
    if(u.state.cb?.hexCd>0)arcaneItemCoverage.add('hex-burst');
  }
  fixtures.push({name:`arcane-item/${test.name}/${stage}`,variant:`arcane/${pace}`,seed,cols,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    arcaneRewards:[0,1].map(team=>({conquestKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.arcConK||0),0),coins:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.coins||0),0),bountyGold:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.bountyGold||0),0)})),
    setup:{sideA:{arcane:mech},sideB:{arcane:mech},roundStage}});
}



DT.Sets.activate('arcane','hyper');
const arcaneAugments=Object.entries(DT.AUGMENT).filter(([id,a])=>id.startsWith('arc_')&&Object.keys(a.hooks||{}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k))).map(([id])=>id);
for(const augment of [...arcaneAugments,'mixed'])for(const duos of [false,true])for(const stage of [0,2]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate('arcane',pace);
  const seed=840000+fixtures.length,pick=DT.makeRng(seed);
  function side(team){
    const parts=[0,1].map(part=>{
      const owner=team*2+part,chosen=DT.SHOP_LINES.filter(l=>l.traits.includes(part?'SYN':'BR')).slice(0,3);
      while(chosen.length<6)chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      const augs=augment==='mixed'?['arc_a_wind','arc_a_lotus','arc_a_unity','arc_a_giant']:[augment];
      return {owner,augs:[...augs,...(duos?[part?'duo_sync':'duo_tag']:[])],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%4,owner,
        items:i%3===0?[]:Array.from({length:i%3},()=>arcaneHookItems[Math.floor(pick()*arcaneHookItems.length)])}))};
    });
    const mech={arcSynHp:120,arcSynAug:true,arcChests:5,arcJunk:['flame','bots','destruct']};
    return duos?{parts,units:parts.flatMap(p=>p.units),augs:parts.flatMap(p=>p.augs),counts:DT.Duos.teamCounts(parts),cols,mech}:{units:parts[0].units,augs:parts[0].augs,cols,mech};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner}));
  const effects=a=>({arcane:a.mech,augments:a.augs,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))}:{})});
  for(const u of cb.units) {
    for(const flag of ['ascT','zhonya','mantle'])if(u.state[flag])arcaneItemCoverage.add(flag);
    if(u.state.doom>13)arcaneItemCoverage.add('doom-repeat');
    if(u.state.bountyGold>0)arcaneItemCoverage.add('bounty');
    if(u.state.cb?.tankOn)arcaneItemCoverage.add('tank');
    if(u.state.cb?.ripped)arcaneItemCoverage.add('ripper');
    if(u.state.cb?.hexCd>0)arcaneItemCoverage.add('hex-burst');
  }
  fixtures.push({name:`arcane-augment/${augment}/${duos}/${stage}`,variant:`arcane/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    arcaneRewards:[0,1].map(team=>({conquestKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.arcConK||0),0),coins:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.coins||0),0),bountyGold:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.bountyGold||0),0)})),
    setup:{sideA:effects(a),sideB:effects(b)}});
}


for(const flag of ['ascT','zhonya','mantle','doom-repeat','bounty','tank','ripper','hex-burst'])if(!arcaneItemCoverage.has(flag))throw new Error(`Arcane item effect never exercised: ${flag}`);


// Discover every anomaly through the production offer generator, including utility picks.
DT.Sets.activate('arcane','hyper');
const anomalyIds=new Set(),anomalyOfferContext={player:{mech:{}},shuffle:a=>a};
for(let i=0;i<100;i++){
  const before=anomalyIds.size;
  for(const id of DT.SET.mechanic.offerKinds.arcAnomaly.options(anomalyOfferContext,[]))anomalyIds.add(id);
  if(anomalyIds.size===before)break;
}
if(anomalyIds.size!==27)throw new Error('Arcane anomaly inventory changed; update native coverage');
const anomalyCoverage=new Set();
for(const anomaly of [...anomalyIds,'mixed'])for(const duos of [false,true])for(const stage of [0,1,2]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate('arcane',pace);
  const seed=910000+fixtures.length,pick=DT.makeRng(seed),all=[...anomalyIds];
  function side(team){
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes(part?'SYN':'BR')).slice(0,3);
      while(chosen.length<6)chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      return {owner,augs:['arc_a_wind','arc_a_lotus',...(duos?[part?'duo_sync':'duo_tag']:[])],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%4,owner,
        items:i%3===0?[]:Array.from({length:i%3},()=>arcaneHookItems[Math.floor(pick()*arcaneHookItems.length)]),
        mods:i%2===0||anomaly==='mixed'?{arcAnom:anomaly==='mixed'?all[Math.floor(pick()*all.length)]:anomaly}:{}}))};
    });
    const mech={arcSynHp:140,arcChests:2,arcJunk:['flame','bots','destruct']};
    return duos?{units:parts.flatMap(p=>p.units),parts,augs:parts.flatMap(p=>p.augs),counts:DT.Duos.teamCounts(parts),cols,mech}:{units:parts[0].units,augs:parts[0].augs,cols,mech};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  for(const u of cb.units){
    if(u.state.lc)anomalyCoverage.add('lastchance');
    if(u.state.asc)anomalyCoverage.add('ascend');
    if(u.state.rage)anomalyCoverage.add('berserk');
    if(u.state.bulwark===false)anomalyCoverage.add('bulwark');
    if(u.line.id==='arc_s_garurumon')anomalyCoverage.add('partners');
    if(u.line.id==='arc_s_shellnumemon')anomalyCoverage.add('familiar');
    if(u.state.anomK>0)anomalyCoverage.add('kills');
  }
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner,arcaneAnomaly:u.mods?.arcAnom||null}));
  const effects=a=>({arcane:a.mech,augments:a.augs,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))}:{})});
  fixtures.push({name:`arcane-anomaly/${anomaly}/${duos}/${stage}`,variant:`arcane/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    arcaneRewards:[0,1].map(team=>({conquestKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.arcConK||0),0),coins:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.coins||0),0),bountyGold:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.bountyGold||0),0),anomalyKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.anomK||0),0)})),
    setup:{sideA:effects(a),sideB:effects(b)}});
}
for(const flag of ['lastchance','ascend','berserk','bulwark','partners','familiar','kills'])if(!anomalyCoverage.has(flag))throw new Error(`Arcane anomaly effect never exercised: ${flag}`);



DT.Sets.activate('arcane','hyper');
const center={c:3,r:5},adjacent=Array.from({length:56},(_,i)=>({c:i%7,r:Math.floor(i/7)})).filter(x=>DT.hexDist(x,center)===1);
for(const edge of ['devour-alone','devour-chain','partners-full','partners-one','share-partners','link-contraband','fireball-119','fireball-120'])for(const stage of [0,1,2]){
  const seed=970000+fixtures.length,line='arc_psychemon';
  const entry=(anomaly,cell,items=[])=>({line,stage,...cell,items,mods:anomaly?{arcAnom:anomaly}:{}});
  let a;
  if(edge==='devour-alone')a=[entry('dna_hunger',center)];
  else if(edge==='devour-chain')a=[entry('dna_hunger',center),entry('dna_hunger',adjacent[0]),entry('dna_power',adjacent[1])];
  else if(edge.startsWith('partners-'))a=[entry('dna_partners',center),...adjacent.slice(0,edge==='partners-full'?6:5).map(cell=>entry(null,cell))];
  else if(edge==='share-partners')a=[entry('dna_partners',center),entry('dna_share',adjacent[0]),entry('dna_share',adjacent[1])];
  else if(edge==='link-contraband')a=[entry('dna_shield',center,['arc_k_bloom2']),entry('dh_lastchance',adjacent[0],['arc_time_stopper']),entry('dna_energy',adjacent[1])];
  else a=[entry('dh_fireball',{c:3,r:7}),...adjacent.map(cell=>entry(null,cell))];
  if(edge==='link-contraband')a[0].line='arc_mushroomon';
  const b=a.map(u=>({...u,items:[...u.items],mods:{...u.mods}}));
  // Synthetic catalog boundary: current roster caps below 120 mana, but Fireball explicitly supports both branches.
  const mana=edge.startsWith('fireball-')?Number(edge.split('-')[1]):null,oldMana=DT.LINE[line].mana;
  if(mana!==null)DT.LINE[line].mana=mana;
  const cb=new DT.Combat({units:a,noTraits:true},{units:b,noTraits:true},{seed});
  if(edge==='partners-full'&&cb.units.some(u=>u.summoned))throw new Error('Blocked partner summon unexpectedly succeeded');
  if(edge==='partners-one'&&cb.units.filter(u=>u.summoned).length!==2)throw new Error('Expected one partner summon per side');
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  if(mana!==null&&!cb.events.some(e=>e.type==='cast'&&e.at===0))throw new Error('Fireball mana boundary never cast');
  if(oldMana===undefined)delete DT.LINE[line].mana;else DT.LINE[line].mana=oldMana;
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,arcaneAnomaly:u.mods?.arcAnom||null}));
  fixtures.push({name:`arcane-anomaly-edge/${edge}/${stage}`,variant:'arcane/hyper',seed,cols:7,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,
    ...(mana!==null?{lineManaOverride:{line,maxMana:mana}}:{}),
    arcaneRewards:[0,1].map(team=>({conquestKills:0,coins:0,bountyGold:0,anomalyKills:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.anomK||0),0)})),
    setup:{sideA:{noTraits:true},sideB:{noTraits:true}}});
}



DT.Sets.activate('coliseum','hyper');
const coliseumCasters=DT.SHOP_LINES.filter(l=>l.type.startsWith('col_')).flatMap(l=>[null,...(DT.Sets.def('coliseum').forms[l.id]||[]).map(f=>f.id)].map(form=>({line:l.id,form})));
const coliseumCoverage=new Set();
function observeColiseum(cb,coverage=coliseumCoverage){
  for(const u of cb.units){
    if(u.state.allOutUsed)coverage.add('allout');
    const inf=u.state.inf;
    if(inf&&inf.victim!==u)coverage.add('infection-spread');
    if(inf&&inf.left===0)coverage.add('infection-expire');
    if(inf&&inf.left>0&&!inf.src.alive)coverage.add('infection-dead-source');
  }
}
for(const test of coliseumCasters)for(const stage of [0,1,2])for(const cols of [7,14]){
  const pace=cols===7?'hyper':'normal';DT.Sets.activate('coliseum',pace);
  let accepted=false;
  for(let attempt=0;attempt<20&&!accepted;attempt++){
    const coverage=new Set();
    const seed=1030000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
    const entries=main=>Array.from({length:6},(_,i)=>({line:main&&i===0?test.line:DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id,
      stage,c:i,r:4+i%3,items:[],mods:main&&i===0&&test.form?{form:test.form}:{}}));
    const a=entries(true),b=entries(false),mech={rcAP:stage*35};
    const cb=new DT.Combat({units:a,cols,noTraits:true,mech},{units:b,cols,noTraits:true,mech},{seed});
    const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
    while(!cb.done&&tick<2000){cb.step();tick++;observeColiseum(cb,coverage);if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
    if(!cb.events.some(e=>e.type==='cast'&&e.at===0))continue;accepted=true;
    for(const flag of coverage)coliseumCoverage.add(flag);
    const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,coliseum:{form:u.mods.form||null}}));
    fixtures.push({name:`coliseum-ability/${test.line}/${test.form}/${stage}/${cols}`,variant:`coliseum/${pace}`,seed,cols,
      sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,setup:{sideA:{noTraits:true,coliseum:mech},sideB:{noTraits:true,coliseum:mech}}});
  }
  if(!accepted)throw new Error(`Coliseum caster never cast ${test.line}/${test.form}/${stage}/${cols}`);
}

const coliseumCores=DT.Sets.def('coliseum').powerUps.map(p=>p.id);
for(const core of [...coliseumCores,'mixed'])for(const stage of [0,1,2])for(const duos of [false,true]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate('coliseum',pace);
  const seed=1090000+fixtures.length,pick=DT.makeRng(seed),components=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component);
  function side(team){
    const parts=[0,1].map(part=>({owner:team*2+part,augs:[],units:Array.from({length:6},(_,i)=>{
      const line=DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)],forms=DT.Sets.def('coliseum').forms[line.id]||[];
      return {line:line.id,stage,c:part*7+i,r:4+i%4,owner:team*2+part,
        items:i%3===0?[]:[components[Math.floor(pick()*components.length)]],mods:{pu:core==='mixed'?coliseumCores[Math.floor(pick()*coliseumCores.length)]:core,
          ...(forms.length?{form:forms[Math.floor(pick()*forms.length)].id}:{})}};
    })}));
    const mech={rcAP:stage*30};
    return duos?{units:parts.flatMap(p=>p.units),parts,cols,noTraits:true,mech}:{units:parts[0].units,cols,noTraits:true,mech};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;observeColiseum(cb);if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner,coliseum:{powerUp:u.mods.pu,form:u.mods.form||null}}));
  const effects=a=>({noTraits:true,coliseum:a.mech,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:[]}))}:{})});
  fixtures.push({name:`coliseum-core/${core}/${stage}/${duos}`,variant:`coliseum/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,setup:{sideA:effects(a),sideB:effects(b)}});
}
for(const flag of ['allout','infection-spread','infection-expire','infection-dead-source'])if(!coliseumCoverage.has(flag))throw new Error(`Coliseum lifecycle never exercised: ${flag}`);



DT.Sets.activate('coliseum','hyper');
const coliseumTraits=Object.entries(DT.TRAITS).map(([id,t])=>[id,[...t.bp]]);
for(const [id,bp] of coliseumTraits)for(let level=0;level<bp.length;level++)for(const formation of [0,1]){
  const pace=formation?'normal':'hyper',cols=formation?14:7;DT.Sets.activate('coliseum',pace);
  const seed=1170000+fixtures.length,pick=DT.makeRng(seed),components=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].component&&!DT.COMPONENTS[id]?.special);
  const own=DT.SHOP_LINES.filter(l=>l.traits.includes(id)).slice(0,bp[level]),foes=[];
  while(foes.length<Math.max(5,own.length)){const line=DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)];if(!foes.includes(line))foes.push(line);}
  const entries=lines=>lines.map((l,i)=>{
    const forms=DT.Sets.def('coliseum').forms[l.id]||[],form=forms[(level+formation)%forms.length];
    return {line:l.id,stage:(level+formation)%3,c:i%cols,r:formation?4+i%4:4+Math.floor(i/cols),
      items:i%2?[components[Math.floor(pick()*components.length)]]:[],
      mods:{...(form?{form:form.id}:{}),...(formation?{pu:coliseumCores[Math.floor(pick()*coliseumCores.length)]}:{})},
      ...(form?.trait?{bonusTraits:{[form.trait]:1}}:{})};
  });
  const a=entries(own),b=entries(foes);
  if(own.length<bp[level])a[0].bonusTraits={...(a[0].bonusTraits||{}),[id]:bp[level]-own.length};
  const mech={rcAP:formation?65:0,supreme:formation?own[0].id:null},roundStage=formation?6:0;
  const cb=new DT.Combat({units:a,cols,mech},{units:b,cols,mech},{seed,info:{stage:roundStage}});
  if(DT.traitLevel(id,cb.traitCounts[0][id])!==level+1)throw new Error(`Coliseum missing breakpoint ${id}/${level}`);
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits,coliseum:{form:u.mods.form||null,powerUp:u.mods.pu||null}}));
  fixtures.push({name:`coliseum-trait/${id}/${level}/${formation}`,variant:`coliseum/${pace}`,seed,cols,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:{coliseum:mech},sideB:{coliseum:mech},roundStage}});
}



DT.Sets.activate('coliseum','hyper');
const coliseumSpecial=[],sensei=DT.SHOP_LINES.filter(l=>l.traits.includes('MEN'));
for(let mask=1;mask<16;mask++)coliseumSpecial.push({name:`sensei-${mask}`,lines:sensei.filter((_,i)=>mask&(1<<i)).map(l=>l.id)});
for(const form of DT.Sets.def('coliseum').forms.col_justimon)for(const stars of [0,1,2]){
  const ally=DT.SHOP_LINES.find(l=>l.traits.includes(form.trait));
  coliseumSpecial.push({name:`stance-${form.id}-${stars}`,lines:[ally.id,'col_justimon'],stars,form});
}
for(const stars of [0,1,2])coliseumSpecial.push({name:`rose-${stars}`,lines:['col_lotosmon','col_gotsumon','col_bearmon','col_geogreymon','col_lalamon'],stars,rose:true});
for(const preferred of [false,true])coliseumSpecial.push({name:`supreme-${preferred}`,lines:DT.SHOP_LINES.filter(l=>l.traits.includes('SC')).map(l=>l.id),preferred});
let sawRegrowth=false;
for(const test of coliseumSpecial)for(const duos of [false,true]){
  const cols=duos?14:7,pace=duos?'normal':'hyper';DT.Sets.activate('coliseum',pace);
  const seed=1230000+fixtures.length,pick=DT.makeRng(seed),stars=test.stars??1;
  const a=test.lines.map((line,i)=>({line,stage:stars,c:i,r:test.rose&&i===0?7:4+i%2,owner:0,items:[],mods:test.rose?{pu:'matrix'}:{},
    ...(line==='col_justimon'&&test.form?{mods:{form:test.form.id},bonusTraits:{[test.form.trait]:1}}:{})}));
  const b=test.rose?a.map(u=>({...u,owner:2,mods:{...u.mods}})):Array.from({length:Math.max(5,a.length)},(_,i)=>({line:DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id,stage:stars,c:i,r:4+i%3,owner:2,items:[],mods:{}}));
  if(duos)for(const [team,units] of [[0,a],[1,b]])for(let i=0;i<3;i++)units.push({line:DT.SHOP_LINES.filter(l=>!l.traits.includes('MEN'))[i].id,stage:1,c:7+i,r:4+i,owner:team*2+1,items:[],mods:{pu:'guardian'}});
  const mech={supreme:test.preferred?test.lines[0]:null,rcAP:40};
  const side=(units,team)=>({units,cols,mech,...(duos?{parts:[0,1].map(part=>({owner:team*2+part,augs:[],units:units.filter(u=>u.owner===team*2+part)}))}:{})});
  const sideA=side(a,0),sideB=side(b,1);
  if(duos){sideA.counts=DT.Duos.teamCounts(sideA.parts);sideB.counts=DT.Duos.teamCounts(sideB.parts);}
  const cb=new DT.Combat(sideA,sideB,{seed,info:{stage:5}}),initialPlants=cb.units.filter(u=>u.line.id==='col_root'||u.line.id==='col_spine').length;
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  if(cb.units.filter(u=>u.line.id==='col_root'||u.line.id==='col_spine').length>initialPlants)sawRegrowth=true;
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner,bonusTraits:u.bonusTraits,coliseum:{form:u.mods.form||null,powerUp:u.mods.pu||null}}));
  const effects=a=>({coliseum:mech,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:[]}))}:{})});
  fixtures.push({name:`coliseum-special/${test.name}/${duos}`,variant:`coliseum/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:effects(sideA),sideB:effects(sideB),roundStage:5}});
}
if(!sawRegrowth)throw new Error('Coliseum plant regrowth never exercised');
// Full front rows prevent the Mech summon; opening one cell permits exactly one.
for(const gap of [false,true]){
  DT.Sets.activate('coliseum','hyper');const seed=1290000+fixtures.length,lines=DT.SHOP_LINES.filter(l=>l.traits.includes('MECH')).slice(0,3);
  const a=Array.from({length:gap?27:28},(_,i)=>({line:lines[i%3].id,stage:0,c:i%7,r:4+Math.floor(i/7),items:[]}));
  const b=a.map(u=>({...u})),cb=new DT.Combat({units:a},{units:b},{seed});
  if(cb.units.filter(u=>u.line.id==='col_mech').length!==(gap?2:0))throw new Error('Coliseum Mech placement boundary mismatch');
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
  fixtures.push({name:`coliseum-special/mech-space/${gap}`,variant:'coliseum/hyper',seed,cols:7,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts});
}



DT.Sets.activate('coliseum','hyper');
const colHookItems=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].hooks);
const colCombatAugments=Object.entries(DT.AUGMENT).filter(([id,a])=>id.startsWith('col_')&&Object.keys(a.hooks||{}).some(k=>DT.Sets.COMBAT_HOOKS.includes(k)||k==='onCountTraits')).map(([id])=>id);
const colEffectCases=[...colHookItems.map(item=>({name:item,item})),...colCombatAugments.map(augment=>({name:augment,augment})),...Array.from({length:12},(_,i)=>({name:`mixed-${i}`,mixed:true}))];
const colEffectCoverage=new Set();
for(const test of colEffectCases)for(const stage of [0,1,2])for(const duos of [false,true]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate('coliseum',pace);
  const seed=1100000+fixtures.length,pick=DT.makeRng(seed),roundStage=stage===0?0:stage===1?1:6;
  function side(team){
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const chosen=DT.SHOP_LINES.filter(l=>l.traits.includes(part?'MEN':'MECH')).slice(0,4);
      while(chosen.length<6)chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      const augs=test.augment?[test.augment]:test.mixed?['col_a_second','col_a_ascension','col_a_arena','col_a_unity','col_a_lotus']:[];
      return {owner,augs:[...augs,...(duos?[part?'duo_sync':'duo_tag']:[])],units:chosen.map((l,i)=>({line:l.id,stage,c:part*7+i,r:4+i%4,owner,
        items:test.item?(i%2?[test.item]:[test.item,test.item]):Array.from({length:2},()=>colHookItems[Math.floor(pick()*colHookItems.length)]),
        mods:{pu:['atomic','matrix','burst','inferno','spirit','xanti'][i]}}))};
    });
    return duos?{parts,units:parts.flatMap(p=>p.units),augs:parts.flatMap(p=>p.augs),counts:DT.Duos.teamCounts(parts),cols}:{units:parts[0].units,augs:parts[0].augs,cols};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed,info:{stage:roundStage}});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  for(const u of cb.units)for(const flag of ['nc','vow','zh','doom0','voidUntil','flail'])if(u.state[flag])colEffectCoverage.add(flag);
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,owner:u.owner,coliseum:{powerUp:u.mods.pu}}));
  const effects=a=>({augments:a.augs,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))}:{})});
  fixtures.push({name:`coliseum-effects/${test.name}/${stage}/${duos}`,variant:`coliseum/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    setup:{sideA:effects(a),sideB:effects(b),roundStage}});
}
for(const flag of ['nc','vow','zh','doom0','voidUntil','flail'])if(!colEffectCoverage.has(flag))throw new Error(`Coliseum effect not exercised: ${flag}`);



// Underlords cast kernel, independently of the still-pending hero/alliance/item hooks.
// Retain the real ability registry and engine timing; only hook dispatch is disabled.
for (const name of ['fireTeam', 'fireGlobal']) DT.Combat.prototype[name] = function () {};
DT.Combat.prototype.has = () => false;
const ulCastKinds=['ul_shuriken','ul_shadowwave','ul_empower','ul_purge','ul_haze','ul_scatter','ul_punch','ul_terror',
  'ul_chaosbolt','ul_grip','ul_ghostship','ul_poof','ul_hook','ul_scream','ul_charge','ul_sleight','ul_purify',
  'ul_dagger','ul_metamorph','ul_laguna','ul_arrow','ul_shieldcrash','ul_fadebolt','ul_godstrength','ul_ravage',
  'ul_dissimilate','ul_breathefire','ul_chrono','ul_illuminate'];
for(const set of ['underlords','underlords_ko'])for(const kind of ulCastKinds)for(const stage of [0,1,2])for(const cols of [7,14]){
  const pace=cols===7?'hyper':'normal';DT.Sets.activate(set,pace);
  const primary=DT.SHOP_LINES.find(l=>l.type===kind);if(!primary)throw new Error(`Missing Underlords cast ${set}/${kind}`);
  const pool=DT.SHOP_LINES.filter(l=>kinds.includes(l.type)||ulCastKinds.includes(l.type));
  let accepted=false;
  for(let attempt=0;attempt<30&&!accepted;attempt++){
    const seed=1400000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
    const entries=main=>Array.from({length:6},(_,i)=>({line:main&&i===0?primary.id:pool[Math.floor(pick()*pool.length)].id,
      stage,c:i,r:4+i%3,items:[]}));
    const a=entries(true),b=entries(false),cb=new DT.Combat({units:a,cols,noTraits:true},{units:b,cols,noTraits:true},{seed});
    const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
    while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
    if(!cb.events.some(e=>e.type==='cast'&&e.at===0))continue;
    const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
    fixtures.push({name:`underlords-cast/${set}/${kind}/${stage}/${cols}`,variant:`${set}/${pace}`,seed,cols,
      sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t});accepted=true;
  }
  if(!accepted)throw new Error(`Underlords cast not exercised: ${set}/${kind}/${stage}/${cols}`);
}
Object.assign(DT.Combat.prototype,originalHooks);



const underlordsRewardsSnapshot=cb=>[0,1].map(team=>({
  kills:cb.units.filter(u=>u.team===team&&!u.summoned&&u.traits.includes('BEAST')).reduce((n,u)=>n+(u.state.ulKills||0),0),
  gold:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.ulGold||0),0),
  duel:cb.units.filter(u=>u.team===team).reduce((n,u)=>n+(u.state.ulDuelWin||0),0)}));
const ulMechanicCoverage=new Set();
for(const set of ['underlords','underlords_ko']){
  DT.Sets.activate(set,'hyper');const heroes=DT.SHOP_LINES.map(l=>l.id);
  for(const hero of heroes)for(const stage of [0,1,2])for(const cols of [7,14]){
    const pace=cols===7?'hyper':'normal';DT.Sets.activate(set,pace);
    let accepted=false;
    for(let attempt=0;attempt<30&&!accepted;attempt++){
      const seed=1700000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
      const entries=main=>Array.from({length:6},(_,i)=>({line:main&&i===0?hero:heroes[Math.floor(pick()*heroes.length)],stage,c:i,r:4+i%3,items:[]}));
      const a=entries(true),b=entries(false),mech={ulDuel:stage*15};
      const cb=new DT.Combat({units:a,cols,noTraits:true,mech},{units:b,cols,noTraits:true,mech},{seed});
      const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
      while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
      const mustCast=DT.LINE[hero].type!=='ul_passive'&&!(hero==='ul_lifestealer'&&stage<2);
      if(mustCast&&!cb.events.some(e=>e.type==='cast'&&e.at===0))continue;
      for(const u of cb.units){
        for(const key of ['ulGraveDone','ulEclipse','ulLivingArmor','ulStone','ulMeta','ulShifted','ulPois'])if(u.state[key])ulMechanicCoverage.add(key);
        if(u.summoned)ulMechanicCoverage.add(u.line.id);
      }
      const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items}));
      fixtures.push({name:`underlords-mechanic/${set}/${hero}/${stage}/${cols}`,variant:`${set}/${pace}`,seed,cols,
        sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
        underlordsRewards:underlordsRewardsSnapshot(cb),
        setup:{sideA:{noTraits:true,underlords:mech},sideB:{noTraits:true,underlords:mech}}});accepted=true;
    }
    if(!accepted)throw new Error(`Underlords hero not exercised: ${set}/${hero}/${stage}/${cols}`);
  }
}
for(const key of ['ulGraveDone','ulEclipse','ulLivingArmor','ulStone','ulMeta','ulShifted','ulPois','ul_s_ward','ul_s_bear','ul_s_treant','ul_s_serpent','ul_s_wolf','ul_meepo'])
  if(!ulMechanicCoverage.has(key))throw new Error(`Underlords mechanic not exercised: ${key}`);



const ulAllianceCoverage=new Set();
DT.Sets.activate('underlords','hyper');
const ulAllianceCases=Object.entries(DT.TRAITS).flatMap(([id,t])=>t.bp.map((bp,level)=>({name:`${id}-${level}`,id,bp,level})));
for(const set of ['underlords','underlords_ko'])for(const test of ulAllianceCases)for(const formation of [0,1]){
  const pace=formation?'normal':'hyper',cols=formation?14:7;DT.Sets.activate(set,pace);
  const seed=2100000+fixtures.length,pick=DT.makeRng(seed);
  const own=DT.SHOP_LINES.filter(l=>l.traits.includes(test.id)).slice(0,test.bp),foes=[];
  while(foes.length<Math.max(5,own.length)){const line=DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)];if(!foes.includes(line))foes.push(line);}
  const entries=lines=>lines.map((l,i)=>({line:l.id,stage:(test.level+formation+i)%3,c:i%cols,r:formation?4+i%4:4+Math.floor(i/cols),items:[]}));
  const a=entries(own),b=entries(foes);if(own.length<test.bp)a[0].bonusTraits={[test.id]:test.bp-own.length};
  const mech={ulDuel:formation?30:0,ulBeastKills:formation?12.25:0};
  const cb=new DT.Combat({units:a,cols,mech},{units:b,cols,mech},{seed});
  if(DT.traitLevel(test.id,cb.traitCounts[0][test.id])!==test.level+1)throw new Error(`Underlords missing breakpoint ${set}/${test.name}`);
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  for(const u of cb.units){for(const key of ['ulRogue','ulFell','ulSage','ulVig','ulVoidCd'])if(u.state[key])ulAllianceCoverage.add(key);if(u.summoned)ulAllianceCoverage.add(u.line.id);}
  if(cb.events.some(e=>e.text==='Delta Strike'))ulAllianceCoverage.add('spirit');
  const convert=a=>a.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,items:u.items,bonusTraits:u.bonusTraits}));
  fixtures.push({name:`underlords-alliance/${set}/${test.name}/${formation}`,variant:`${set}/${pace}`,seed,cols,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    underlordsRewards:underlordsRewardsSnapshot(cb),
    setup:{sideA:{underlords:mech},sideB:{underlords:mech}}});
}
for(const set of ['underlords','underlords_ko'])for(const trait of ['LEGEND','SHAMAN','ROGUE','SPIRIT','KNIGHT','UNDEAD'])for(const stars of [0,1,2]){
  DT.Sets.activate(set,'normal');const cols=14,seed=2200000+fixtures.length,pick=DT.makeRng(seed);
  function side(team){
    const parts=[0,1].map(part=>{
      const owner=team*2+part,chosen=DT.SHOP_LINES.filter(l=>l.traits.includes(trait));
      if(trait==='ROGUE'&&!chosen.some(l=>l.id==='ul_phantom'))chosen.unshift(DT.LINE.ul_phantom);
      while(chosen.length<6)chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)]);
      return {owner,augs:[part?'duo_sync':'duo_tag'],units:chosen.slice(0,7).map((l,i)=>({line:l.id,stage:stars,c:part*7+i,r:4+i%3,owner,items:[],
        ...(trait==='LEGEND'&&i===0?{bonusTraits:{ROGUE:4,SHAMAN:6,UNDEAD:6,DEMON:1,KNIGHT:6,HOLY:4,SPIRIT:3}}:{})}))};
    });
    return {parts,units:parts.flatMap(p=>p.units),augs:parts.flatMap(p=>p.augs),counts:DT.Duos.teamCounts(parts),cols,mech:{ulDuel:25,ulBeastKills:14.5}};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  if(cb.events.some(e=>e.text==='Delta Strike'))ulAllianceCoverage.add('spirit');
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items,bonusTraits:u.bonusTraits}));
  const effects=a=>({underlords:a.mech,augments:a.augs,parts:a.parts.map(p=>({owner:p.owner,augments:p.augs}))});
  fixtures.push({name:`underlords-alliance-duos/${set}/${trait}/${stars}`,variant:`${set}/normal`,seed,cols,duos:true,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    underlordsRewards:underlordsRewardsSnapshot(cb),
    setup:{sideA:effects(a),sideB:effects(b)}});
}
for(const key of ['ulRogue','ulFell','ulSage','ulVoidCd','ul_s_hatchling','ul_s_blackdragon','spirit'])if(!ulAllianceCoverage.has(key))throw new Error(`Underlords alliance not exercised: ${key}`);



DT.Sets.activate('underlords','hyper');
const ulHeldItems=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].hooks&&DT.ITEMS[id].cat!=='global');
const ulGlobalItems=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].cat==='global');
const ulItemCases=[...ulHeldItems.map(item=>({name:item,item})),...ulGlobalItems.map(global=>({name:global,global})),...Array.from({length:12},(_,i)=>({name:`mixed-${i}`,mixed:true}))];
const ulItemCoverage=new Set();
for(const set of ['underlords','underlords_ko'])for(const test of ulItemCases)for(const stage of [0,1,2]){
  const duos=stage===2,cols=duos?14:7,pace=duos?'normal':'hyper';DT.Sets.activate(set,pace);
  const seed=2400000+fixtures.length,pick=DT.makeRng(seed);
  function side(team){
    const parts=[0,1].map(part=>{
      const owner=team*2+part;
      const lines=['ul_meepo','ul_lycan','ul_luna','ul_bristleback','ul_treant','ul_lich'];
      const globals=test.global?[test.global]:test.mixed?['ul_g_stone','ul_g_ristul','ul_g_assault','ul_g_chivalry']:[];
      return {owner,globals,augs:duos?[part?'duo_sync':'duo_tag']:[],units:lines.map((line,i)=>({line:team===0?line:DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id,
        stage,c:part*7+i,r:4+i%4,owner,items:test.item?(i%2?[test.item]:[test.item,test.item]):Array.from({length:2},()=>ulHeldItems[Math.floor(pick()*ulHeldItems.length)])}))};
    });
    const mech={ulDuel:20,ulBeastKills:10};
    return duos?{parts,units:parts.flatMap(p=>p.units),augs:parts.flatMap(p=>p.augs),globals:parts.flatMap(p=>p.globals),counts:DT.Duos.teamCounts(parts),cols,mech}
      :{units:parts[0].units,globals:parts[0].globals,augs:parts[0].augs,cols,mech};
  }
  const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
  const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
  while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
  for(const u of cb.units){
    for(const key of ['ulBlinkAt','ulEulCd','ulPipe','ulMek','ulRefresh','ulVyseCd','ulHorn','ulSatanicCd','ulShiva','ulGraveDone'])if(u.state[key])ulItemCoverage.add(key);
    if(u.summoned)ulItemCoverage.add(u.line.id);
    if(u.summoned&&u.items.includes('ul_euls'))ulItemCoverage.add('summoned-euls');
  }
  const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
  const effects=a=>({underlords:a.mech,globals:a.globals,augments:a.augs,...(duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs,globals:p.globals}))}:{})});
  fixtures.push({name:`underlords-item/${set}/${test.name}/${stage}`,variant:`${set}/${pace}`,seed,cols,duos,
    sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
    underlordsRewards:underlordsRewardsSnapshot(cb),setup:{sideA:effects(a),sideB:effects(b)}});
}
for(const key of ['ulBlinkAt','ulEulCd','ulPipe','ulMek','ulRefresh','ulVyseCd','ulHorn','ulSatanicCd','ulShiva','ulGraveDone','ul_s_bruiser','ul_s_thunderhide','summoned-euls'])
  if(!ulItemCoverage.has(key))throw new Error(`Underlords item effect not exercised: ${key}`);



const ulTamerCoverage=new Set();
const ulPowers=['ul_anessa_dark','ul_anessa_heal','ul_hob_fire','ul_hob_crazy','ul_jules_brawl','ul_jules_bar','ul_enno_balm','ul_enno_rabid'];
for(const set of ['underlords','underlords_ko'])for(const power of ulPowers)for(const rank of [1,2,3,4,5,6])for(const duos of [false,true]){
  const pace=duos?'normal':'hyper',cols=duos?14:7;DT.Sets.activate(set,pace);let accepted=false;
  for(let attempt=0;attempt<30&&!accepted;attempt++){
    const seed=2800000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
    function side(team){
      const parts=[0,1].map(part=>{
        const id=part?ulPowers[(ulPowers.indexOf(power)+3)%ulPowers.length]:power,tamer=DT.POWER[id].tamer,owner=team*2+part;
        const chosen=['ul_omniknight','ul_treant','ul_bristleback','ul_lich','ul_dazzle'];
        const units=[{line:`ul_tamer_${tamer}`,stage:Math.floor((rank-1)/2),c:part*7+3,r:tamer==='jules'?4:7,owner,items:['ul_euls','ul_octarine']},
          ...chosen.map((line,i)=>({line,stage:Math.floor((rank-1)/2),c:part*7+i,r:5+i%2,owner,items:rank%2?['ul_vitality']:['ul_pipe','ul_vesture']}))];
        return {owner,power:{id,rank},globals:rank%2?['ul_g_stone']:['ul_g_ristul','ul_g_chivalry'],augs:[],units};
      });
      return duos?{parts,units:parts.flatMap(p=>p.units),power:parts[0].power,globals:parts.flatMap(p=>p.globals),counts:DT.Duos.teamCounts(parts),cols}
        :{units:parts[0].units,power:parts[0].power,globals:parts[0].globals,cols};
    }
    const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
    const samples=[{tick:0,units:snapshots(cb)}];let tick=0;
    while(!cb.done&&tick<2000){cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});}
    if(!cb.events.some(e=>e.type==='cast'&&e.at===0&&e.name))continue;
    for(const event of cb.events)if(event.type==='cast'&&event.name)ulTamerCoverage.add(event.name);
    for(const u of cb.units){if(u.summoned)ulTamerCoverage.add(u.line.id);if(u.state.ulEsc>=2)ulTamerCoverage.add('escape-twice');if(u.state.ulDeferAcc>0)ulTamerCoverage.add('deferred');}
    if(cb.events.some(e=>e.text==='Enthralled'))ulTamerCoverage.add('enthralled');
    const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items}));
    const effects=a=>({power:a.power,globals:a.globals,...(duos?{parts:a.parts.map(p=>({owner:p.owner,power:p.power,globals:p.globals}))}:{})});
    fixtures.push({name:`underlords-tamer/${set}/${power}/${rank}/${duos}`,variant:`${set}/${pace}`,seed,cols,duos,
      sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
      underlordsRewards:underlordsRewardsSnapshot(cb),setup:{sideA:effects(a),sideB:effects(b)}});accepted=true;
  }
  if(!accepted)throw new Error(`Underlords Tamer never acted: ${set}/${power}/${rank}/${duos}`);
}
for(const key of ['The End of Medicine','Enthrall','Rejuvenating Ritual','Summon Demonic Golem','Implosion','Shockwave','Friendly Fire',"Let's Go Crazy",'Roundhouse','Barrels of Fun','To Your Health','Happy Hour','Soothing Balm','Death Spin','All Out Attack!','Escape Artist','ul_s_archer','ul_s_golem','escape-twice','deferred','enthralled'])
  if(!ulTamerCoverage.has(key))throw new Error(`Tamer effect not exercised: ${key}`);



DT.Sets.activate('autochess','hyper');
const acCoverage=new Set(),acCases=[];
const acCasters=DT.SHOP_LINES.filter(l=>['summon','transform','storm'].includes(l.type)).map(l=>l.id);
for(const line of acCasters)for(const stage of [0,1,2])for(const cols of [7,14])acCases.push({name:`ability-${line}-${stage}-${cols}`,lines:[line],stage,cols,cast:true,noTraits:true});
for(const [id,t] of Object.entries(DT.TRAITS))for(let level=0;level<t.bp.length;level++)for(const cols of [7,14]){
  const lines=DT.SHOP_LINES.filter(l=>l.traits.includes(id)).slice(0,t.bp[level]).map(l=>l.id);
  acCases.push({name:`trait-${id}-${level}-${cols}`,lines,stage:(level+(cols===14?1:0))%3,cols,trait:id,level,
    bonus:lines.length<t.bp[level]?{[id]:t.bp[level]-lines.length}:null});
}
const acHeldItems=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].hooks&&!DT.ITEMS[id].relic);
for(const item of acHeldItems)for(const stage of [0,1,2])for(const cols of [7,14])acCases.push({name:`item-${item}-${stage}-${cols}`,lines:acCasters.slice(0,5),item,stage,cols});
for(const relic of Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].relic))for(const cols of [7,14])acCases.push({name:`relic-${relic}-${cols}`,lines:acCasters.slice(0,5),globals:[relic],stage:2,cols,duos:cols===14});
for(let demons=1;demons<=3;demons++)for(let slayers=0;slayers<=2;slayers++)for(const wizard of [false,true]){
  const lines=['ac_goblimon','ac_floramon','ac_gazimon'].slice(0,demons).concat(['ac_lopmon','ac_guilmon'].slice(0,slayers),wizard?['ac_candlemon','ac_labramon']:[]);
  for(const cols of [7,14])acCases.push({name:`counts-${demons}-${slayers}-${wizard}-${cols}`,lines,stage:1,cols,duos:cols===14});
}
for(const line of DT.SHOP_LINES.filter(l=>l.traits.includes('DRUID')).map(l=>l.id))for(const stage of [0,1,2])for(const level of [0,1,2])
  acCases.push({name:`druid-cap-${line}-${stage}-${level}`,lines:[line],stage,cols:7,bonus:level?{DRUID:level===1?1:3}:null});
for(const count of [0,1,2,3,4,5])for(const slayers of [false,true])acCases.push({name:`olympos-wizard-${count}-${slayers}`,
  lines:['ac_coronamon','ac_labramon','ac_candlemon','ac_psychemon',...(slayers?['ac_lopmon','ac_guilmon']:[])],bonus:{BEAST:count},stage:2,cols:7});
for(let i=0;i<12;i++)acCases.push({name:`mixed-${i}`,lines:acCasters,stage:i%3,cols:14,duos:true,globals:['ac_r_grace','ac_r_reaper']});
for(const test of acCases){
  const cols=test.cols,pace=cols===14?'normal':'hyper';DT.Sets.activate('autochess',pace);let accepted=false;
  for(let attempt=0;attempt<30&&!accepted;attempt++){
    const seed=3100000+fixtures.length+attempt*10000,pick=DT.makeRng(seed);
    function side(team){
      const parts=[0,1].map(part=>{
        const owner=team*2+part,chosen=team===0?[...test.lines]:Array.from({length:Math.max(5,test.lines.length)},()=>DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id);
        if(test.cast)while(chosen.length<5)chosen.push(DT.SHOP_LINES[Math.floor(pick()*DT.SHOP_LINES.length)].id);
        const width=test.duos?7:cols;
        const units=chosen.map((line,i)=>({line,stage:test.stage,c:part*7+i%width,r:4+Math.floor(i/width),owner,
          items:test.item?(i%2?[test.item]:[test.item,test.item]):test.name.startsWith('mixed')?Array.from({length:2},()=>acHeldItems[Math.floor(pick()*acHeldItems.length)]):[],
          ...(team===0&&i===0&&test.bonus?{bonusTraits:test.bonus}:{})}));
        return {owner,units,globals:test.globals||[],augs:test.duos?[part?'duo_sync':'duo_tag']:[]};
      });
      return test.duos?{parts,units:parts.flatMap(p=>p.units),augs:parts.flatMap(p=>p.augs),globals:parts.flatMap(p=>p.globals),counts:DT.Duos.teamCounts(parts),cols}
        :{units:parts[0].units,globals:parts[0].globals,cols,noTraits:!!test.noTraits};
    }
    const a=side(0),b=side(1),cb=new DT.Combat(a,b,{seed});
    if(test.trait&&DT.traitLevel(test.trait,cb.traitCounts[0][test.trait])!==test.level+1)throw new Error(`Auto Chess missing breakpoint ${test.name}`);
    const samples=[{tick:0,units:snapshots(cb)}];let tick=0,summonCap=false;
    while(!cb.done&&tick<2000){
      cb.step();tick++;if(tick%15===0||cb.done)samples.push({tick,units:snapshots(cb)});
      const owners=new Map();for(const u of cb.units)if(u.alive&&u.summoned&&u.state.acOwner!=null)owners.set(u.state.acOwner,(owners.get(u.state.acOwner)||0)+1);
      for(const n of owners.values()){if(n>4)throw new Error('Auto Chess reference exceeded summon cap');if(n===4)summonCap=true;}
    }
    if(test.cast&&!cb.events.some(e=>e.type==='cast'&&e.at===0))continue;
    if(summonCap)acCoverage.add('summon-cap');
    for(const u of cb.units){for(const key of ['acTf','acRefresh','acStoneCd','acKnCd'])if(u.state[key])acCoverage.add(key);if(u.state.acDruidCap!=null)acCoverage.add(`druid-cap-${u.state.acDruidCap}`);}
    const convert=a=>a.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,owner:u.owner,items:u.items,bonusTraits:u.bonusTraits}));
    const effects=a=>({noTraits:!!a.noTraits,globals:a.globals,augments:a.augs,...(test.duos?{parts:a.parts.map(p=>({owner:p.owner,augments:p.augs,globals:p.globals}))}:{})});
    fixtures.push({name:`autochess/${test.name}`,variant:`autochess/${pace}`,seed,cols,duos:!!test.duos,
      sideA:convert(a),sideB:convert(b),samples,tick,winner:cb.winner,time:cb.t,productionTraits:true,traitCounts:cb.traitCounts,
      setup:{sideA:effects(a),sideB:effects(b)}});accepted=true;
  }
  if(!accepted)throw new Error(`Auto Chess cast never exercised: ${test.name}`);
}
for(const key of ['summon-cap','acTf','acRefresh','acStoneCd','acKnCd','druid-cap-0','druid-cap-1'])if(!acCoverage.has(key))throw new Error(`Auto Chess effect not exercised: ${key}`);


for (const duos of [false, true]) {
  DT.Sets.activate('faction-wars', 'hyper');
  const log = [];
  const names = [...DT.Sets.COMBAT_HOOKS, ...DT.Sets.ANY_HOOKS];
  function hooks(id) {
    return Object.fromEntries(names.map(name => [name, { order: id === 'probe_early' ? -2 : 0, fn(c, e) {
      log.push(`${id}:${c.team ?? '-'}:${name}:${e.unit ? e.unit.id : '-'}`);
      if (name === 'onCombatStart') {
        for (const u of c.mine) {
          c.buff(u, 'armor', 2, 0.4);
          c.after(0.1, () => c.buff(u, 'ad', 3, 0.4));
          c.after(0.2, () => c.heal(u, 5), u);
        }
      }
      if (name === 'onCombatReady' && id === 'probe_item') c.shield(c.unit, 10);
      if (name === 'onCombatReady' && id === 'probe_augment') {
        const u = c.mine[0];
        if (u) { u.stage++; u.maxHp = Math.round(u.maxHp * 1.5); u.ad *= 1.5; u.baseAd *= 1.5; }
        c.after(0.1, () => {
          if (!u) return;
          const cell = c.freeCellNear(u);
          if (!cell) return;
          const summoned = c.summon(c.team, u.line.id, 0, cell.c, cell.r, { owner: u.owner, items: ['thief_glove'], hpMul: 0.8, adMul: 1.1 });
          if (!summoned) throw new Error('Reference summon failed');
          // Occupied/out-of-board/unknown summons must not consume RNG.
          if (c.summon(c.team, u.line.id, 0, cell.c, cell.r) || c.summon(c.team, u.line.id, 0, -1, 0) || c.summon(c.team, 'missing', 0, 0, 0)) throw new Error('Invalid summon accepted');
          const move = c.freeCellNear(summoned);
          if (move) c.move(summoned, move.c, move.r, 0.2);
        }, u);
      }
      if (name === 'onShield' && id === 'probe_trait') c.shield(e.unit, 1);
      if (name === 'onBeforeDamage') { e.mult *= 1.02; c.buff(e.target, 'armor', 0.5, 0.1); }
      if (name === 'onBeforeDamageTaken') { e.mult *= 0.99; e.dr += 0.01; c.buff(e.unit, 'armor', 0.25, 0.1); c.buff(e.unit, 'dr', 0.001, 0.1); }
      if (name === 'onAttack' && id === 'probe_power') c.buff(e.unit, 'as', 0.1, 0.2);
      if (name === 'onCast' && id === 'probe_augment' && !c.state.recast) { c.state.recast = true; c.castAgain(e.unit, e.target); }
      if (name === 'onDamageTaken' && id === 'probe_global') c.heal(e.unit, 3, true);
      if (name === 'onLethal' && id === 'probe_item' && !c.state.saved) { c.state.saved = true; c.heal(e.unit, e.unit.maxHp * 2); }
    } }]));
  }
  DT.TRAITS.probe_trait = { bp: [1], lv: [1], hooks: hooks('probe_trait') };
  DT.AUGMENT.probe_augment = { hooks: hooks('probe_augment') };
  DT.AUGMENT.probe_early = { hooks: hooks('probe_early') };
  DT.POWER.probe_power = { id: 'probe_power', hooks: hooks('probe_power') };
  DT.ITEMS.probe_global = { stats: {}, p: {}, hooks: hooks('probe_global') };
  DT.ITEMS.probe_item = { stats: {}, p: {}, hooks: hooks('probe_item') };
  const oldSetHooks = DT.SET.hooks;
  DT.SET.hooks = hooks('probe_mechanic');
  const cols = duos ? 14 : 7;
  const entries = () => [0, 1].map(i => ({ line: DT.SHOP_LINES.find(l => l.type === (i ? 'heal' : 'nuke')).id,
    stage: 0, c: 2 + i, r: 4, owner: i, items: ['probe_item', 'probe_item'] }));
  function side(units) {
    const part = owner => ({ owner, augs: ['probe_augment', 'probe_early'], power: { id: 'probe_power' }, globals: ['probe_global'] });
    return { units, cols, counts: { probe_trait: 1 }, ...part(0), ...(duos ? { parts: [part(0), part(1)] } : {}) };
  }
  const sideA = entries(), sideB = entries();
  const cb = new DT.Combat(side(sideA), side(sideB), { seed: 432 });
  const samples = [{ tick: 0, units: snapshots(cb) }];
  let tick = 0;
  while (!cb.done && tick < 2000) { cb.step(); tick++; if (tick % 15 === 0 || cb.done) samples.push({ tick, units: snapshots(cb) }); }
  const convert = a => a.map(u => ({ line: u.line, stage: u.stage, column: u.c, row: u.r, owner: u.owner, items: u.items }));
  fixtures.push({ name: `hooks/${duos}`, variant: 'faction-wars/hyper', seed: 432, cols, sideA: convert(sideA), sideB: convert(sideB),
    samples, tick, winner: cb.winner, time: cb.t, hookProbe: true, duos, hookLog: log });
  const covered = new Set(log.map(entry => entry.split(':')[2]));
  for (const name of names) if (!covered.has(name)) throw new Error(`Hook reference never exercised ${name}`);
  DT.SET.hooks = oldSetHooks;
}
// Content-addressed shards keep each file small and preserve the old manifest on failure.
async function writeFixtures() {
  const directory=path.join(__dirname,'../tests/fixtures/combat');fs.mkdirSync(directory,{recursive:true});
  const encode=(_key,value)=>typeof value==='number'&&!Number.isFinite(value)?String(value):value;
  const groups=new Map();
  for(const fixture of fixtures){const key=fixture.variant.split('/')[0];if(!groups.has(key))groups.set(key,[]);groups.get(key).push(fixture);}
  const files=[];
  for(const [key,battles] of groups){
    const temporary=path.join(directory,key+'.tmp');const hash=require('node:crypto').createHash('sha256');
    function* chunks(){yield '[';for(let i=0;i<battles.length;i++){if(i)yield ',';yield JSON.stringify(battles[i],encode);}yield ']\n';}
    const hashing=new (require('node:stream').Transform)({transform(chunk,_encoding,callback){hash.update(chunk);callback(null,chunk);}});
    try {
      await require('node:stream/promises').pipeline(require('node:stream').Readable.from(chunks()),require('node:zlib').createGzip(),hashing,fs.createWriteStream(temporary));
      const sha256=hash.digest('hex'),file=`${key}-${sha256.slice(0,16)}.json.gz`;
      fs.renameSync(temporary,path.join(directory,file));files.push({file,battles:battles.length,sha256});
    } catch(error){fs.rmSync(temporary,{force:true});throw error;}
  }
  const manifest=path.join(directory,'manifest.json');
  fs.writeFileSync(manifest+'.tmp',JSON.stringify({schemaVersion:1,battles:fixtures.length,files},null,2)+'\n');
  fs.renameSync(manifest+'.tmp',manifest);
  const keep=new Set(files.map(f=>f.file));
  for(const file of fs.readdirSync(directory))if(file.endsWith('.json.gz')&&!keep.has(file))fs.unlinkSync(path.join(directory,file));
  fs.rmSync(path.join(directory,'../combat.json.gz'),{force:true});
  console.log(`Exported ${fixtures.length} reference battles in ${files.length} verified fixture shards.`);
}
writeFixtures().catch(error=>{console.error(error);process.exitCode=1;});
