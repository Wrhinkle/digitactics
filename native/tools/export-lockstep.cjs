const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib');
const DT = require('../../tools/headless.cjs').load({ strict: true });
const traces = [];
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) for (const duo of [false, true]) {
  const init = { seed: 143, setId: set.id, pace, format: duo ? 'duos' : 'solo', planTime: 30, at: 10000,
    slots: Array.from({ length: 8 }, (_, i) => ({ name: `Tamer ${i}`, human: i < 2 })) };
  const game = DT.Lock.create(init), steps = []; let now = init.at;
  const snapshot = () => {
    const contexts = [
      { now: game.phaseAt + 29999, online: [0,1], done: [] },
      { now: game.phaseAt + 30000, online: [0,1], done: [] },
      { now: game.turnAt + 11999, online: [0,1], done: [] },
      { now: game.turnAt + 12000, online: [0,1], done: [] },
      { now: game.phaseAt + 3199, online: [], done: [] },
      { now: game.phaseAt + 3200, online: [0,1], done: [0,1] },
      { now: game.phaseAt + 100000, online: [0], done: [] }
    ];
    return { digest: DT.Lock.digest(game), check: DT.Lock.check(game), seq: game.seq, phaseAt: game.phaseAt, turnAt: game.turnAt, votes: game.votes,
      maxCombat: game.phase === 'combat' ? DT.Lock.maxCombatMs(game) : null,
      decisions: contexts.map(c => ({ ...c, result: DT.Lock.decide(game, { now: c.now, online: new Set(c.online), done: new Set(c.done) }) })) };
  };
  const opening = structuredClone(snapshot());
  const apply = entry => { const result = DT.Lock.apply(game, entry); steps.push({ entry, applied: !!result, after: structuredClone(snapshot()) }); };
  apply({ k: 'end', at: ++now }); // Wrong-phase entries still consume sequence numbers.
  for (let n = 0; n < 24 && !game.over; n++) {
    if (game.phase === 'planning' && game.roundType !== 'carousel') {
      apply({ k: 'a', s: 0, a: 'buy', g: { slot: n % 5 }, at: ++now });
      apply({ k: 'a', s: 0, a: 'move', g: { from: { type: 'bench', i: 0 }, to: { type: 'board', c: n % 7, r: 7 } }, at: ++now });
      apply({ k: 'vote', s: 0, at: ++now }); apply({ k: 'vote', s: 0, at: ++now });
    }
    now += 100000;
    const entry = DT.Lock.decide(game, { now, online: new Set([0,1]), done: new Set([0,1]) });
    if (entry) {
      apply({ ...entry, at: now });
      if (entry.k === 'start' || entry.k === 'end') apply({ ...entry, at: now + 1 });
    }
  }
  traces.push({ variant: `${set.id}/${pace}`, init, opening, steps });
}
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/lockstep.json.gz'), zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} original lockstep traces.`);
