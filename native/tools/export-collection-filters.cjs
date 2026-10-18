// Reference results from the original collection UI and ownership registry.
// No browser, rendering, or replacement implementation of colItems is involved.
const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), zlib = require('node:zlib'), assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const catalog = JSON.parse(fs.readFileSync(path.join(root, 'native/src/DigiTactics.Storage/cosmetics.json'), 'utf8'));
const client = fs.readFileSync(path.join(root, 'js/client.js'), 'utf8');
const rarities = client.match(/^const RARITIES = .*;$/m)?.[0];
const state = client.match(/^const col = .*;$/m)?.[0];
const start = client.indexOf('function colItems(kind) {'), end = client.indexOf('\nRENDER.collection =', start);
assert(rarities && state && start >= 0 && end > start, 'Original collection filter declarations moved');
const originalFilter = client.slice(start, end);
const kinds = Object.keys(catalog);
const ownership = ['all', 'owned', 'locked'], sorts = ['rarity', 'rarity-asc', 'name', 'owned'];
const saves = [];
function snapshot(name, choose) {
  const owned = Object.fromEntries(kinds.map(kind => [kind, catalog[kind].filter((item, index) => choose(item, index, kind)).map(item => item.id)]));
  saves.push({ name, save: { bits: 12345, owned, equipped: {}, history: [{ n: 12, reason: 'preserve me', at: 123 }], future: { preserved: true } } });
}
snapshot('fresh', () => false);
snapshot('partial', (item, index, kind) => !item.linked && (index % 4 === 0 || item.id === catalog[kind].find(item => item.secret)?.id));
snapshot('complete', () => true);
// Granting a linked ID alone does not confer ownership of its source item.
snapshot('linked-only', item => !!item.linked);

const metadata = Object.fromEntries(kinds.map(kind => [kind, catalog[kind].map(item => ({ id: item.id, name: item.name, description: item.desc ?? null,
  rarity: item.rarity ?? null, tag: item.tag ?? null, secret: !!item.secret, free: !!item.free, linked: item.linked ?? null }))]));
const snapshots = []; let count = 0; let sourceRarities;
for (const input of saves) {
  const storage = { 'dt.cosmetics.v1': JSON.stringify(input.save) };
  const context = { DT: {}, console, localStorage: { getItem: key => storage[key] || null, setItem: (key, value) => { storage[key] = value; } } };
  context.window = context; vm.createContext(context);
  vm.runInContext(fs.readFileSync(path.join(root, 'js/cosmetics.js'), 'utf8'), context, { filename: 'js/cosmetics.js' });
  const C = context.DT.Cosmetics;
  for (const kind of kinds) C.register(kind, structuredClone(catalog[kind]));
  vm.runInContext(`const C = DT.Cosmetics;\n${rarities}\n${state}\n${originalFilter}\nthis.rarities = RARITIES; this.select = (kind,q,own,rar,sort) => {Object.assign(col,{q,own,rar,sort}); return colItems(kind).map(item => item.id);};`, context, { filename: 'js/client.js:collection' });
  sourceRarities = Array.from(context.rarities);
  const before = JSON.stringify(kinds.map(kind => C.list(kind))), savedBefore = storage['dt.cosmetics.v1'];
  const cases = [];
  for (const kind of kinds) {
    const items = catalog[kind], named = items.find(item => !item.secret && item.name.includes(' ')) || items[0];
    const description = items.find(item => !item.secret && item.desc)?.desc || '';
    const tag = items.find(item => item.tag)?.tag || 'Retro';
    const queries = [...new Set(['', '   ', 'COMMON', ' rArE ', 'legendary', named.name,
      '\t  ' + named.name.toUpperCase() + '  \n', named.name.replace(/ /g, '  '),
      description.split(/\s+/).slice(0, 4).join(' '), description.split(/\s+/).slice(1, 3).join(' ').toUpperCase(),
      tag, '  ' + tag.toUpperCase() + '  ', 'rare retro', "jijimon's", 'no-such-cosmetic-filter-xyz'])];
    for (const query of queries) for (const own of ownership) for (const rarity of ['all', ...sourceRarities]) for (const sort of sorts) {
      cases.push({ kind, query, ownership: own, rarity, sort, expected: context.select(kind, query, own, rarity, sort) }); count++;
    }
  }
  assert.equal(JSON.stringify(kinds.map(kind => C.list(kind))), before, input.name + ': source filter mutated registry');
  assert.equal(storage['dt.cosmetics.v1'], savedBefore, input.name + ': source filter mutated save');
  snapshots.push({ ...input, visible: Object.fromEntries(kinds.map(kind => [kind, C.visible(kind).map(item => item.id)])),
    owned: Object.fromEntries(kinds.map(kind => [kind, C.list(kind).filter(item => C.owns(kind, item.id)).map(item => item.id)])), cases });
}
const output = path.join(root, 'native/tests/fixtures/collection-filters.json.gz');
fs.mkdirSync(path.dirname(output), { recursive: true });
fs.writeFileSync(output, zlib.gzipSync(JSON.stringify({ schema: 1, locale: Intl.DateTimeFormat().resolvedOptions().locale, rarities: sourceRarities, kinds, metadata, snapshots }), { level: 9 }));
console.log(`Exported ${count} original ordered collection queries across ${snapshots.length} ownership snapshots and ${kinds.length} kinds.`);
