// Headless smoke test: plays full 8-AI matches and checks invariants.
// Run: node tests/sim.js [games] [--set <id>] [--pace hyper|normal|chaos]   (default set: faction-wars; 'chaos' picks
// per game from the seed; no --pace plays the set's native pace)
const { load, argSet, argPace, stripSet } = require('../tools/headless.cjs');

const DT = load({ strict: true });
const SET = argSet(process.argv) || DT.Sets.DEFAULT;
const PACE = argPace(process.argv);
const argv = stripSet(process.argv);
if (SET !== 'chaos' && !DT.Sets.has(SET)) { console.error(`unknown set ${SET}; registered: ${DT.Sets.ids().join(', ')}`); process.exit(1); }

function checkPool(g) {
  const counts = {};
  for (const L of DT.SHOP_LINES) counts[L.id] = g.pool[L.id];
  for (const p of g.players) {
    for (const u of p.allUnits()) if (DT.LINE[u.line].shop) counts[u.line] += DT.unitCopies(u);
    for (const id of p.shop) if (id) counts[id] += 1;
    for (const o of p.orbs) if (o.unit) counts[o.unit] += 1;
  }
  for (const L of DT.SHOP_LINES) {
    if (counts[L.id] !== DT.POOL_SIZE[L.tier]) throw new Error(`pool leak ${L.id}: ${counts[L.id]} != ${DT.POOL_SIZE[L.tier]} at ${g.roundLabel}`);
  }
}

const games = Number(argv[2] || 20);
const winsByTrait = {};
let rounds = 0;
let draws = 0;
let fights = 0;
let combats = 0;
const t0 = Date.now();
for (let s = 1; s <= games; s++) {
  const g = new DT.Game({ seed: s * 7919, allAI: true, setId: SET, pace: PACE });
  let guard = 0;
  while (!g.over && guard++ < 200) {
    if (g.human.alive && !g.human.isHuman) {} // human slot is AI in this mode
    g.startCombat();
    const res = g.finishCombat();
    if (g.roundType !== 'carousel') combats++;
    for (const r of res) { fights++; if (!r.aWon && !r.bWon) draws++; }
    for (const p of g.players) {
      if (p.alive && p.boardCount() > p.cap()) throw new Error(`${p.name} over cap`);
      if (p.gold < 0) throw new Error(`${p.name} negative gold`);
    }
    checkPool(g);
    g.nextRound();
    rounds++;
  }
  if (!g.over) throw new Error(`game ${s} did not end`);
  const winner = g.players.find((p) => p.placement === 1);
  const counts = DT.countTraits(winner.boardUnits());
  for (const t in counts) if (DT.traitLevel(t, counts[t])) winsByTrait[t] = (winsByTrait[t] || 0) + 1;
  console.log(`game ${s} [${g.setId} ${g.pace}]: ${g.roundLabel}, winner ${winner.name} L${winner.level} ` +
    winner.boardUnits().map((u) => DT.LINE[u.line].forms[u.stage]).join(', '));
}
console.log(`\n${games} games, ${rounds} rounds, ${(combats / games).toFixed(1)} combat rounds a game, ${fights} fights, ${draws} timeouts, ${Date.now() - t0}ms`);
console.log('active traits on winning boards:', winsByTrait);
