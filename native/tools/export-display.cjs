const fs = require('fs'), path = require('path');
const DT = require('../../tools/headless.cjs').load({ strict: true });
const breakpointSource = fs.readFileSync(path.join(__dirname, '../../js/ui.js'), 'utf8').match(/const bpText = (\(T, i\) => \{[\s\S]*?\n  \});/);
if (!breakpointSource) throw new Error('Original trait breakpoint formatter could not be located.');
const breakpointText = require('vm').runInNewContext(breakpointSource[1]);
const variants = {};
const consumableCases = [];
const abilityCases = [];
const sharedAbilities = new Set(['nuke','aoe','stun','slam','shield','heal','barrage','drain','dash','weaken','rally','line','execute','chain']);
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const entries = {};
  const add = (id, v) => {
    if (id) entries[id] = { name: v.name || id, description: v.desc || v.description || '', ...(typeof v.preview === 'string' ? { preview: v.preview } : {}) };
  };
  for (const [id, v] of Object.entries(DT.ITEMS)) add(id, v);
  for (const [id, v] of Object.entries(DT.ITEMS)) if (v.consumable) {
    if (typeof v.preview === 'function') throw new Error(`Native consumable preview needs an implementation: ${set.id}/${id}`);
    add(`consumable:${id}`, { name: v.name, desc: DT.consumablePreview(id, { items: [] }, null, null).text });
    const cap = DT.HYPER.itemCap;
    for (let held = 0; held <= 3; held++) for (const bench of [1, cap - 2, cap, cap + 1]) {
      const unit = { items: Array(held).fill(Object.keys(DT.ITEMS)[0]) };
      const player = { items: Array(bench).fill(id), game: { R: DT.HYPER } };
      consumableCases.push({ variant: `${set.id}/${pace}`, id, held, bench, text: DT.consumablePreview(id, unit, player, player.game).text });
    }
  }
  for (const [id, v] of Object.entries(DT.TRAITS)) add(id, v);
  for (const [id, trait] of Object.entries(DT.TRAITS)) trait.bp.forEach((count, index) => {
    add(`trait:${id}:${index}`, { name: String(count), desc: breakpointText(trait, index) });
  });
  for (const line of Object.values(DT.LINE)) for (let stage = 0; stage < line.forms.length; stage++) {
    add(`ability:${line.id}:${stage}`, { name: line.abil[stage], desc: DT.abilityText(line, stage) });
    if (!line.abilText && sharedAbilities.has(line.type)) {
      const key = `ability:${line.id}:${stage}`, base = DT.abilityValue(line, stage);
      const original = DT.abilityValue;
      try {
        DT.abilityValue = () => 1000000007;
        entries[key].scaledDescription = DT.abilityText(line, stage).replaceAll('1000000007', '{value}');
      } finally { DT.abilityValue = original; }
      if (!entries[key].scaledDescription.includes('{value}')) throw Error(`Shared ability format missing value: ${key}`);
      entries[key].abilityValue = base;
      for (const ap of [0, 33.3, 99.5, 100, 127.25, 200, 999.9])
        abilityCases.push({ variant: `${set.id}/${pace}`, line: line.id, stage, ap, text: DT.abilityText(line, stage, ap) });
    }
  }
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
fs.writeFileSync(path.join(__dirname, '../content/consumable-preview-cases.json'), JSON.stringify(consumableCases) + '\n');
fs.writeFileSync(path.join(__dirname, '../content/ability-display-cases.json'), JSON.stringify(abilityCases) + '\n');
console.log(`Exported native display text and ${abilityCases.length} shared ability AP cases.`);
