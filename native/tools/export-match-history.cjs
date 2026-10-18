const fs = require('fs'), path = require('path'), vm = require('vm');
const source = fs.readFileSync(path.join(__dirname, '../../js/client.js'), 'utf8');
const initial = { profile: { avatar: 'keep' }, matches: [], recent: [], totals: { games: 2, wins: 1, top4: 1, placeSum: 7, traits: {}, units: {} }, prefs: { motion: false }, future: { keep: true } };
let now = 1700000000000;
const ctx = { save: structuredClone(initial), snap: {}, snapshotNow: () => null, DT: {}, Date: { now: () => now }, persist() {}, rememberPlayers() {} };
vm.createContext(ctx);
vm.runInContext(source.slice(source.indexOf('const matchKey ='), source.indexOf('\nfunction stats()', source.indexOf('const matchKey ='))) + '\nthis.record = recordMatch;', ctx);
const cases = [];
for (let i = 0; i < 42; i++) {
  const duo = i % 2 === 0, place = 1 + i % (duo ? 4 : 8);
  const snapshot = { level: 5, traits: Array.from({ length: 10 }, (_, n) => ({ id: 't' + n, n: n + 2, lv: 1 })), units: Array.from({ length: 12 }, (_, n) => ({ line: 'u' + (n % 4), stage: n % 3, items: ['rod'] })) };
  const game = { seed: i === 41 ? 40 : i, human: { id: 0, placement: place, level: 6 }, format: duo ? 'duos' : 'solo', setId: 'faction-wars', roundLabel: '5-2' };
  ctx.snap = { game, data: snapshot }; now++;
  const input = { key: `solo:${game.seed}:0`, at: now, mode: 'solo', format: game.format, set: game.setId, place, round: game.roundLabel, level: 6, traits: snapshot.traits, units: snapshot.units, stats: null, bits: null };
  ctx.record(game, null); cases.push({ input, expected: structuredClone(ctx.save) });
}
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/match-history.json.gz'), require('zlib').gzipSync(JSON.stringify({ initial, cases })));
console.log('Exported 42 original match-history updates.');
