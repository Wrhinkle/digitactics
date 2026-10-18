const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const root = path.resolve(__dirname, '../..');
const read = name => fs.readFileSync(path.join(root, 'js', name), 'utf8');
function load(wallet, legend) {
  const storage = { 'dt.cosmetics.v1': JSON.stringify(wallet) };
  if (legend) storage['dt.legendevo.v1'] = JSON.stringify(legend);
  const ctx = { DT: {}, console, localStorage: { getItem: k => storage[k] || null, setItem: (k, v) => storage[k] = v } };
  ctx.window = ctx; vm.createContext(ctx);
  vm.runInContext(read('cosmetics.js'), ctx);
  const source = read('legends.js');
  vm.runInContext('(function(){' + source.slice(source.indexOf('const DT ='), source.indexOf('// Fallbacks')) + '})();', ctx);
  vm.runInContext('(function(){' + read('legendevo.js') + '})();', ctx);
  return { ctx, storage };
}
const base = load({});
const legends = base.ctx.DT.Cosmetics.list('legend').map(x => ({ id: x.id, free: !!x.free, reach: base.ctx.DT.LegendEvo.reach(x.id) }));
const data = { legends, dataFor: base.ctx.DT.LegendEvo.DATA_FOR, gameData: base.ctx.DT.LegendEvo.GAME_DATA, top4Data: base.ctx.DT.LegendEvo.TOP4_DATA, dupData: base.ctx.DT.LegendEvo.DUP_DATA };
fs.writeFileSync(path.join(root, 'native/src/DigiTactics.Storage/legend-rewards.json'), JSON.stringify(data));
const cases = [];
for (const item of legends) for (const mode of ['new', 'capped', 'invalid-form', 'unowned', 'unknown']) {
  const wallet = { owned: { legend: mode === 'unowned' ? [] : [item.id] }, equipped: { legend: mode === 'unknown' ? 'future' : item.id } };
  const initial = mode === 'new' ? null : { v: 1, legends: { [item.id]: { xp: mode === 'capped' ? 1990 : 80, form: mode === 'invalid-form' ? 'future' : item.reach.at(-1), path: [], nonce: 3, dex: [item.id], care: { bond: 2, battle: 4, food: 8 }, future: 'keep' } }, games: [], future: { keep: true } };
  const { ctx, storage } = load(wallet, initial);
  let previous = initial;
  for (let i = 0; i < 65; i++) {
    const key = 'game:' + (i === 4 ? 3 : i === 64 ? 0 : i), place = i % 8 + 1;
    const award = ctx.DT.LegendEvo.recordGame(key, place);
    const expected = JSON.parse(storage['dt.legendevo.v1']);
    cases.push({ wallet, previous, key, place, expected, award }); previous = expected;
  }
}
fs.writeFileSync(path.join(root, 'native/tests/fixtures/legend-rewards.json.gz'), zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${legends.length} legends and ${cases.length} original legend rewards.`);
