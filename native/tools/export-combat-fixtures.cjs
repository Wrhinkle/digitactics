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
  alive: u.alive, target: u.target ? u.target.id : null, atkCd: u.atkCd, moveCd: u.moveCd,
  stun: u.stun, lock: u.lock, dealt: u.dealt, taken: u.taken, healed: u.healed, nAtk: u.nAtk, maxHp: u.maxHp, ad: u.ad, as: u.as, ap: u.ap, armor: u.armor, mr: u.mr, maxMana: u.maxMana, items: u.items }));
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
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/combat.json'), JSON.stringify(fixtures) + '\n');
console.log(`Exported ${fixtures.length} shared-kernel reference battles covering ${kinds.length} ability types.`);
