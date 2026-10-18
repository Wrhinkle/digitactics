const fs = require('fs'), path = require('path');
const DT = require('../../tools/headless.cjs').load({ strict: true });
const variants = {}, cases = [];
const sentinel = '__native_trait_name__';
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const variant = `${set.id}/${pace}`, def = DT.Sets.def(set.id), first = Object.keys(DT.LINE)[0];
  const rules = { traits: Object.fromEntries(Object.entries(DT.TRAITS).map(([id, t]) => [id, t.name])), partner: {}, anomalies: {}, powerUps: {}, forms: {} };
  const note = (line, mods, stage = 0) => DT.unitNote({ line, stage, mods, items: [], bonusTraits: null });
  function sample(line, mods, stage = 0) {
    // CombatEntry keeps the mechanic-bearing fields from the planning snapshot.
    // Compare that path against the original formatter as a separate projection.
    const string = v => typeof v === 'string' ? v : undefined;
    const combat = mods ? {
      ...(mods.partner ? { partner: true, trait: string(mods.trait), bonus: string(mods.bonus) } : {}),
      arcAnom: string(mods.arcAnom), pu: string(mods.pu), form: string(mods.form)
    } : null;
    const notes = note(line, mods, stage);
    // Malformed non-string modifier values cannot survive the existing typed
    // CombatEntry projection. Do not rewrite the source expectation to hide it.
    const combatNotes = JSON.stringify(notes) === JSON.stringify(note(line, combat, stage)) ? notes : null;
    cases.push({ variant, line, stage, mods, notes, combatNotes });
  }
  sample(first, null); sample(first, {});
  sample(first, { partner: false, trait: 'unknown', bonus: 'hp', arcAnom: 'unknown', pu: 'unknown', form: 'unknown' });
  if (set.id === 'fates') {
    for (const bonus of ['', 'hp', 'ap', 'ad', 'mana']) {
      const text = note(first, { partner: true, trait: sentinel, bonus })[0];
      if (!text || !text.includes(sentinel)) throw Error(`Partner note template unavailable: ${variant}/${bonus}`);
      rules.partner[bonus] = text.replace(sentinel, '{trait}');
    }
    for (const trait of [...Object.keys(DT.TRAITS), 'unknown %s ## trait', null, undefined])
      for (const bonus of ['hp', 'ap', 'ad', 'mana', '', 'unknown', null, undefined])
        for (const partner of [true, false, 0, 1, '', 'partner'])
          for (const stage of [0, 1, 2]) sample(first, { partner, trait, bonus }, stage);
  } else if (set.id === 'arcane') {
    for (const id of Object.keys(DT.ArcaneInfo.ANOM)) {
      rules.anomalies[id] = note(first, { arcAnom: id })[0];
      for (const stage of [0, 1, 2]) sample(first, { arcAnom: id }, stage);
    }
    for (const id of ['', 'unknown', null, 0, false]) sample(first, { arcAnom: id });
  } else if (set.id === 'coliseum') {
    for (const power of def.powerUps) rules.powerUps[power.id] = note(first, { pu: power.id })[0];
    for (const [line, forms] of Object.entries(def.forms)) {
      rules.forms[line] = {};
      for (const form of forms) rules.forms[line][form.id] = note(line, { form: form.id })[0];
    }
    for (const line of [first, ...Object.keys(def.forms)])
      for (const pu of [...def.powerUps.map(p => p.id), 'unknown', '', null])
        for (const form of [...new Set(Object.values(def.forms).flat().map(f => f.id)), 'unknown', '', null])
          for (const stage of [0, 1, 2]) sample(line, { pu, form }, stage);
  } else if (DT.SET.unitNote) throw Error(`Unit note provider requires native mapping: ${variant}`);
  variants[variant] = rules;
}
fs.writeFileSync(path.join(__dirname, '../content/unit-notes.json'), JSON.stringify(variants, null, 2) + '\n');
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/unit-notes.json.gz'), require('zlib').gzipSync(JSON.stringify(cases)));
const partner = cases.find(s => s.variant === 'fates/normal' && s.notes.some(n => n.includes('more HP')));
const anomaly = cases.find(s => s.variant === 'arcane/normal' && s.notes.length === 1);
const coreForm = cases.find(s => s.variant === 'coliseum/normal' && s.notes.length === 2);
const unknown = cases.find(s => s.variant === 'coliseum/normal' && s.line === coreForm.line && s.mods?.pu === 'unknown' && s.mods?.form === 'unknown');
if (![partner, anomaly, coreForm, unknown].every(Boolean)) throw Error('Missing actual-tooltip source fixtures.');
fs.writeFileSync(path.join(__dirname, '../content/unit-note-ui-cases.json'), JSON.stringify([partner, anomaly, coreForm, unknown]) + '\n');
console.log(`Exported unit notes for ${Object.keys(variants).length} variants and ${cases.length} original-source cases.`);
