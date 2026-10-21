const fs = require('node:fs'), path = require('node:path'), cp = require('node:child_process'), crypto = require('node:crypto');
const root = path.resolve(__dirname, '..');
const git = (...args) => cp.execFileSync('git', args, { cwd: root, maxBuffer: 128 * 1024 * 1024 }).toString();
if (git('diff', '--cached', '--name-only').trim()) throw new Error('Index has another change; coordinate before staging.');
const provenance = JSON.parse(fs.readFileSync(path.join(root,'native/artifacts/item-capacity-fixture-provenance.json'),'utf8'));
const hashFile = file => fs.existsSync(path.join(root,file)) ? crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex') : null;
for (const fixture of provenance.fixtures) if (hashFile(fixture.file) !== fixture.after) throw new Error('Fixture changed after verification: ' + fixture.file);
for (const [file, hash] of Object.entries(provenance.sourceFiles)) if (hashFile(file) !== hash) throw new Error('Gameplay source changed after export: ' + file);
const files = [
  'js/items.js','js/game.js','js/combat.js','js/duos.js','js/itemcards.js','js/ui.js',
  'js/sets/arcane-mech.js','js/sets/reckoning-mech.js',
  ...['ItemCapacity','PlanningItems','PlanningRoster','EquipmentEffects','AiInventory','ArcanePlanning','ReckoningPlanning','MatchState','TraitRules'].map(x=>'native/src/DigiTactics.Rules/'+x+'.cs'),
  'native/src/DigiTactics.Desktop/ItemDragSmoke.cs','native/tests/DigiTactics.Tests/ItemCapacityChecks.cs',
  'tests/item-capacity.cjs','tests/itemdrag.cjs','native/tools/export-planning.cjs','native/content/display.json',
  ...provenance.fixtures.map(f=>f.file),
];
git('add', '-A', '--', ...files);
function stagePartial(file, update) {
  const old = git('show', 'HEAD:' + file), next = update(old);
  if (next === old) throw new Error('Expected a scoped change in ' + file);
  const hash = cp.execFileSync('git', ['hash-object','-w','--stdin'], { cwd: root, input: next }).toString().trim();
  git('update-index', '--cacheinfo', '100644', hash, file);
}
stagePartial('native/tests/DigiTactics.Tests/Program.cs', old => {
  if (old.includes('--item-capacity-only') || old.includes(' + ItemCapacityChecks.Run(catalog)'))
    throw new Error('Capacity hooks already in HEAD; inspect shared commit before staging.');
  return old.replace('if (args.Contains("--items-only"))', 'if (args.Contains("--item-capacity-only")) { ItemCapacityChecks.Run(catalog); return; }\nif (args.Contains("--items-only"))')
    .replace(' + PlanningItemParity.Run(root, catalog)', ' + ItemCapacityChecks.Run(catalog) + PlanningItemParity.Run(root, catalog)');
});
stagePartial('native/src/DigiTactics.Desktop/MatchScreen.ItemDrag.cs', old => {
  const current = fs.readFileSync(path.join(root,'native/src/DigiTactics.Desktop/MatchScreen.ItemDrag.cs'),'utf8').replace(/\r\n/g,'\n');
  const pattern = /    internal string ItemDropPreview[\s\S]*?(?=    private void ItemDragSource)/;
  const method = current.match(pattern)?.[0];
  if (!method) throw new Error('Missing ItemDropPreview method');
  return old.replace(pattern, method);
});
console.log(git('diff', '--cached', '--stat'));
