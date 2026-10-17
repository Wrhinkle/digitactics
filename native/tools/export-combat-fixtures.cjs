// Isolate the shared combat kernel; set hooks and equipment are tested separately as they are migrated.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
for (const name of ['buildHooks', 'fireTeam', 'fireGlobal']) DT.Combat.prototype[name] = function () {};
DT.Combat.prototype.has = () => false;
const kinds = ['nuke', 'aoe', 'stun', 'slam', 'shield', 'heal', 'barrage', 'drain', 'weaken', 'rally', 'line', 'execute', 'chain', 'dash'];
const snapshots = cb => cb.units.map(u => ({ id: u.id, hp: u.hp, mana: u.mana, shield: u.shield, c: u.c, r: u.r,
  alive: u.alive, target: u.target ? u.target.id : null, atkCd: u.atkCd, moveCd: u.moveCd,
  stun: u.stun, lock: u.lock, dealt: u.dealt, taken: u.taken, healed: u.healed, nAtk: u.nAtk }));
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
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/combat.json'), JSON.stringify(fixtures) + '\n');
console.log(`Exported ${fixtures.length} shared-kernel reference battles covering ${kinds.length} ability types.`);
