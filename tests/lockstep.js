// Lockstep test: one host and two clients apply the same log while three random "humans" play.
// Checks that every replica (and a late joiner replaying the whole log) ends with identical state.
// Run: node tests/lockstep.js [games] [--set <id>] [--pace hyper|normal|chaos]
const { load, argSet, argPace, stripSet } = require('../tools/headless.cjs');

const DT = load({ strict: true });
const Lock = DT.Lock;
const SET = argSet(process.argv) || DT.Sets.DEFAULT;
const PACE = argPace(process.argv);
const argv = stripSet(process.argv);

function play(gameNo) {
  const rnd = DT.makeRng(1000 + gameNo);
  const humans = [0, 3, 5];
  const slots = Array.from({ length: 8 }, (_, i) => ({ name: humans.includes(i) ? `P${i}` : DT.TAMERS[i % 7], human: humans.includes(i) }));
  const init = { seed: 77 + gameNo, slots, planTime: 30, at: 0, setId: DT.Sets.resolve(SET, 77 + gameNo), pace: DT.Sets.resolvePace(PACE, 77 + gameNo) || undefined };
  const host = Lock.create(init);
  const clients = [Lock.create(init), Lock.create(init)];
  const log = [];
  let now = 0;
  const online = new Set(humans);
  const done = new Set();
  const push = (e) => {
    e.at = now;
    log.push(e);
    Lock.apply(host, e);
    for (const c of clients) Lock.apply(c, e);
    if (e.k === 'start' || e.k === 'next') done.clear();
  };
  const pick = (n) => Math.floor(rnd() * n);

  let steps = 0;
  while (!host.over && steps++ < 20000) {
    now += 250;
    // humans act on their own replica's view (client 0 stands in for everyone)
    const view = clients[0];
    for (const s of humans) {
      const p = view.players[s];
      if (!p.alive || !p.isHuman || rnd() > 0.35) continue;
      const r = rnd();
      let act = null;
      if (view.carousel && view.carouselTurn() === p) act = { a: 'carousel', g: { i: pick(9) } };
      else if (view.R.xp && r > 0.97) act = { a: 'buyXp' };
      else if (p.offer && view.phase === 'planning') act = r < 0.2 ? { a: 'offerReroll' } : { a: 'offer', g: { i: pick(3) } };
      else if (r < 0.25) act = { a: 'buy', g: { slot: pick(5) } };
      else if (r < 0.32) act = { a: 'reroll' };
      else if (r < 0.45 && p.bench.some(Boolean)) {
        const i = p.bench.findIndex(Boolean);
        act = { a: 'move', g: { from: { type: 'bench', i }, to: { type: 'board', c: pick(7), r: 4 + pick(4) } } };
      } else if (r < 0.52 && p.items.length) {
        const us = p.allUnits();
        if (us.length) act = { a: 'equip', g: { idx: pick(p.items.length), uid: us[pick(us.length)].uid } };
      } else if (r < 0.56 && p.orbs.length) act = { a: 'orb', g: { id: p.orbs[0].id } };
      else if (r < 0.58 && p.wisp) act = { a: 'wisp' };
      else if (r < 0.6) {
        const us = p.bench.filter(Boolean);
        if (us.length) act = { a: 'sell', g: { uid: us[0].uid } };
      } else if (r < 0.61) act = { a: 'lock' };
      else if (r < 0.75 && view.phase === 'planning') { push({ k: 'vote', s }); continue; }
      if (act) push({ k: 'a', s, ...act });
    }
    // a player drops and comes back mid-game
    if (host.stage === 3 && host.round === 1 && online.has(5) && host.players[5].isHuman) push({ k: 'a', s: 5, a: 'release', g: { name: 'Kari' } });
    if (host.stage === 4 && !host.players[5].isHuman && host.players[5].alive) push({ k: 'a', s: 5, a: 'claim', g: { name: 'P5' } });
    // clients report finished combat replays after a while
    if (host.phase === 'combat' && now - host.phaseAt > 3000) for (const s of humans) done.add(s);
    const e = Lock.decide(host, { now, online, done });
    if (e) push(e);
  }
  const d = Lock.digest(host);
  clients.forEach((c, i) => { if (Lock.digest(c) !== d) throw new Error(`client ${i} desynced in game ${gameNo}`); });
  const late = Lock.create(init);
  for (const e of log) Lock.apply(late, e);
  if (Lock.digest(late) !== d) throw new Error(`late joiner desynced in game ${gameNo}`);
  if (!host.over) throw new Error(`game ${gameNo} did not finish`);
  const winner = host.players.find((p) => p.placement === 1);
  return { entries: log.length, round: host.roundLabel, winner: winner ? winner.name : '(humans out)', places: humans.map((s) => host.players[s].placement) };
}

const games = Number(argv[2] || 5);
for (let g = 1; g <= games; g++) {
  const r = play(g);
  console.log(`game ${g}${SET !== DT.Sets.DEFAULT || PACE ? ` [${SET}${PACE ? ` ${PACE}` : ''}]` : ''}: ${r.entries} log entries, ended ${r.round}, winner ${r.winner}, human places ${r.places.join('/')}`);
}
console.log('lockstep ok');
