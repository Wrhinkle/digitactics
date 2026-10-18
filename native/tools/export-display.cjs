const fs = require('fs'), path = require('path');
const DT = require('../../tools/headless.cjs').load({ strict: true });
const variants = {};
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const entries = {};
  const add = (id, v) => {
    if (id) entries[id] = { name: v.name || id, description: v.desc || v.description || '' };
  };
  for (const [id, v] of Object.entries(DT.ITEMS)) add(id, v);
  for (const [id, v] of Object.entries(DT.TRAITS)) add(id, v);
  for (const v of [...DT.Duos.AUGMENTS, ...DT.AUGMENTS, ...DT.POWERS, ...DT.WISPS]) add(v.id, v);
  const def = DT.Sets.def(set.id);
  const kinds = DT.SET.mechanic.offerKinds || {};
  for (const [kind, spec] of Object.entries(kinds)) {
    add(`offer:${kind}`, { name: spec.name, desc: spec.sub || 'Pick one.' });
    let ids = [];
    if (kind === 'col_core') ids = (def.powerUps || []).map(v => v.id);
    else if (kind === 'col_form') ids = Object.values(def.forms || {}).flat().map(v => v.id);
    else if (kind === 'arcAnomaly') ids = Object.keys(DT.ArcaneInfo.ANOM);
    else if (kind === 'arcJunk') ids = Object.keys(DT.ArcaneInfo.JUNK);
    else if (kind === 'ul_crew') {
      const traits = Object.keys(DT.TRAITS);
      ids = traits.flatMap((a, i) => traits.slice(i + 1).map(b => [a, b].sort().join('+')));
    } else if (kind === 'upgrade') ids = DT.AUGMENTS.map(v => v.id);
    else if (kind === 'ul_tamer') ids = DT.POWERS.map(v => v.id);
    else if (['rec_bless', 'rec_radiant', 'rec_tome', 'arcMarket'].includes(kind)) ids = Object.keys(DT.ITEMS);
    for (const id of ids) add(`choice:${kind}:${id}`, spec.card(id));
  }
  variants[`${set.id}/${pace}`] = entries;
}
fs.writeFileSync(path.join(__dirname, '../content/display.json'), JSON.stringify(variants, null, 2) + '\n');
console.log('Exported native display text.');
