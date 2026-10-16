// Checks that every form of every line (shop, wild and summons) in the given sets resolves through DT.modelKey to a
// models/index.json key whose file exists in models/files. Exit code 1 if anything is missing.
//   node tests/models.cjs [setId ...]   (default: faction-wars dragonlands autochess)
const fs = require('fs');
const path = require('path');
const { load, ROOT } = require('../tools/headless.cjs');

const DT = load();
const index = JSON.parse(fs.readFileSync(path.join(ROOT, 'models', 'index.json'), 'utf8'));
const sets = process.argv.slice(2).length ? process.argv.slice(2) : ['faction-wars', 'dragonlands', 'autochess'];
const gaps = [];
let forms = 0;
for (const id of sets) {
  const S = DT.Sets.activate(id);
  if (S.id !== id) { gaps.push(`${id}: set not registered`); continue; }
  const lines = [...S.LINES];
  // summons named by a line but not in the set's own list
  for (const l of S.LINES) if (l.summon && !S.LINES.some((x) => x.id === l.summon)) {
    if (S.LINE[l.summon]) lines.push(S.LINE[l.summon]); else gaps.push(`${id}: ${l.id} summons unknown line ${l.summon}`);
  }
  for (const line of lines) {
    const n = (line.forms || []).length || 3;
    for (let st = 0; st < n; st++) {
      forms++;
      const what = `${id}: ${line.id}[${st}] ${(line.forms || [])[st] || ''}`;
      const key = DT.modelKey(line, st);
      if (!key) { gaps.push(`${what}: no model key`); continue; }
      const file = index[key];
      if (!file) { gaps.push(`${what}: key ${key} not in models/index.json`); continue; }
      if (!fs.existsSync(path.join(ROOT, file))) gaps.push(`${what}: ${key} -> ${file} missing`);
    }
  }
}
for (const g of gaps) console.log(g);
console.log(`${forms} forms checked in ${sets.join(', ')}: ${gaps.length} gaps`);
process.exit(gaps.length ? 1 : 0);
