const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const root = path.resolve(__dirname, '../..'), read = name => fs.readFileSync(path.join(root, 'js', name), 'utf8');
const catalog = JSON.parse(fs.readFileSync(path.join(root, 'native/src/DigiTactics.Storage/cosmetics.json'), 'utf8')).legend;
const sources = ['cosmetics.js', 'legendevo.js', 'farm.js'].map(read), start = 1792260300000;
function load(wallet, legends, farm, time) {
  const storage = { 'dt.cosmetics.v1': JSON.stringify(wallet), 'dt.legendevo.v1': JSON.stringify(legends) };
  if (farm) storage['dt.farm.v1'] = JSON.stringify(farm);
  class Clock extends Date { constructor(...args) { super(...(args.length ? args : [time])); } static now() { return time; } }
  const context = { DT: {}, Date: Clock, setInterval: () => 0, addEventListener: () => {}, localStorage: { getItem: key => storage[key] || null, setItem: (key, value) => storage[key] = value } };
  context.window = context; vm.createContext(context); vm.runInContext(sources[0], context);
  context.DT.Cosmetics.register('legend', catalog);
  for (const source of sources.slice(1)) vm.runInContext('(function(){' + source + '})();', context);
  return { farm: context.DT.Farm, storage };
}
const clone = value => JSON.parse(JSON.stringify(value)), rows = [];
const legends = { v: 1, legends: {}, games: [] };
function seed(count, balance, tier, lights, distressed) {
  const ids = catalog.slice(0, count).map(item => item.id);
  const wallet = { bits: balance, owned: { legend: ids }, equipped: { legend: ids[0] }, history: [], future: 'keep-wallet' };
  const farm = clone(load(wallet, legends, null, start).farm._save());
  farm.cap = count; farm.residents = ids; farm.lights = lights; farm.future = 'keep-farm';
  for (const id of ids) if (!farm.pets[id]) farm.pets[id] = clone(farm.pets[ids[0]]);
  if (tier) farm.caretaker = { tier, since: start, until: start + 86400000, clock: start };
  if (distressed) for (const pet of Object.values(farm.pets)) {
    Object.assign(pet, { hunger: 0, energy: 3, happy: 0, disc: 37, weight: 24, sick: true, sickWhy: 'dirty', dirt: 500, sickMin: 359,
      starveMin: 179, sadMin: 119, sleepyMin: 59, tantrum: start + 60000, mistakes: 7, poopNeglect: 9,
      goodFood: 2.5, goodBond: 1.25, str: 13, spd: 14, wis: 15, sta: 16, trained: 3, overfeed: 2 });
    pet.poop = [{ t: start - 3600000, x: 1, z: 2, n: true, k: 4 }];
  }
  return { wallet, legend: clone(legends), input: farm };
}
for (const count of [6, 12]) for (const tier of [null, 'basic', 'full']) for (const lights of [false, true]) for (const minutes of [-60, 0, 1, 30, 120, 480, 1439, 1440, 1500, 1560, 2880]) {
  const input = seed(count, 10000, tier, lights, tier !== null), time = start + minutes * 60000;
  const loaded = load(input.wallet, input.legend, input.input, time);
  rows.push({ kind: 'advance', ...input, time, expected: clone(loaded.farm._save()), events: loaded.farm.drainEvents() });
}
for (const tier of [null, 'basic', 'full']) for (const balance of [0, 99, 100, 649, 650, 749, 750, 10000]) for (const plan of ['basic', 'full', 'unknown']) {
  const input = seed(6, balance, tier, true, true), loaded = load(input.wallet, input.legend, input.input, start);
  const result = loaded.farm.hireCaretaker(plan);
  rows.push({ kind: 'hire', ...input, time: start, plan, result, expected: clone(loaded.farm._save()),
    walletAfter: JSON.parse(loaded.storage['dt.cosmetics.v1']), legendAfter: JSON.parse(loaded.storage['dt.legendevo.v1']), events: loaded.farm.drainEvents() });
}
for (const tier of ['basic', 'full']) for (const action of ['MoveIn', 'Swap', 'Expand']) {
  const input = seed(6, 10000, tier, true, true), bank = catalog[6].id;
  input.wallet.owned.legend.push(bank); input.input.seen.push(bank);
  input.input.pets[bank] = clone(input.input.pets[input.input.residents[0]]);
  input.input.pets[bank].future = 'bank-progress';
  if (action === 'MoveIn') input.input.cap = 8;
  const option = action === 'Swap' ? input.input.residents[0] : null;
  const loaded = load(input.wallet, input.legend, input.input, start);
  const result = action === 'MoveIn' ? loaded.farm.moveIn(bank) : action === 'Swap' ? loaded.farm.swap(bank, option) : loaded.farm.buyExpansion();
  rows.push({ kind: 'hire', ...input, time: start, action, id: bank, option, result, expected: clone(loaded.farm._save()),
    walletAfter: JSON.parse(loaded.storage['dt.cosmetics.v1']), legendAfter: JSON.parse(loaded.storage['dt.legendevo.v1']), events: loaded.farm.drainEvents() });
}
fs.writeFileSync(path.join(root, 'native/tests/fixtures/farm-caretaker.json.gz'), zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} source caretaker/capped-offline cases.`);
