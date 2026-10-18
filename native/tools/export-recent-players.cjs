const fs = require('fs'), path = require('path'), vm = require('vm');
const source = fs.readFileSync(path.join(__dirname, '../../js/client.js'), 'utf8');
const initial = { recent: [{ name: 'Old', at: 1000, future: true }], profile: { name: 'Me' }, future: { keep: true } };
let now = 1000, writes = 0;
const context = { save: structuredClone(initial), myName: () => 'Me', Date: { now: () => now }, persist: () => writes++ };
vm.createContext(context);
vm.runInContext(source.slice(source.indexOf('function rememberPlayers('), source.indexOf('\nconst initials')), context);
const cases = [];
function test(members, at) {
  now = at; const before = writes;
  context.rememberPlayers(members);
  cases.push({ members, at, changed: writes !== before, expected: structuredClone(context.save) });
}
test([{ name: 'Me' }, { name: 'Alias', me: true }, { name: '' }, {}], 1000);
test([{ name: 'Old' }, { name: 'New' }, { name: 'New' }], 1000);
test([{ name: 'Old' }], 61000);
test([{ name: 'Old' }], 61001);
test([{ name: 'New' }], 500);
for (let i = 0; i < 30; i++) test([{ name: 'Tamer ' + i }, { name: 'Me' }], 62000 + i);
test([{ name: 'Tamer 29' }, { name: 'Tamer 28' }], 200000);
test([{ name: 'tamer 29' }], 200000);
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/recent-players.json.gz'), require('zlib').gzipSync(JSON.stringify({ initial, cases })));
console.log(`Exported ${cases.length} original recent-player updates.`);
