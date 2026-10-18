// Export the original move resolver, including each set's overrides, for native presentation.
const fs = require('fs'), path = require('path');
const DT = require('../../tools/headless.cjs').load({ strict: true });
new Function('window', fs.readFileSync(path.join(__dirname, '../../js/moves.js'), 'utf8').replace(/^export /gm, ''))({ DT });
const result = {};
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  result[`${set.id}/${pace}`] = Object.fromEntries(Object.values(DT.LINE).map(line => [line.id,
    line.abil.map(name => {
      const element = DT.Moves.specFor(name, line).elem;
      if (!['fire','water','ice','thunder','plant','wind','holy','dark','metal','beast'].includes(element)) throw Error(`Unknown element: ${element}`);
      return element;
    })]));
}
fs.writeFileSync(path.join(__dirname, '../content/ability-elements.json'), JSON.stringify(result, null, 2) + '\n');
console.log('Exported ability elements for all 22 variants.');
