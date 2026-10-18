// Verify exported card selections against the source API, independently of the browser exporter.
const assert = require('assert/strict'), fs = require('fs'), path = require('path'), crypto = require('crypto'), vm = require('vm');
const { load } = require('../../tools/headless.cjs');
const root = path.resolve(__dirname, '../..'), folder = path.join(root, 'native/content/choice-art');
const manifest = JSON.parse(fs.readFileSync(path.join(folder, 'manifest.json'), 'utf8'));
const DT = load({ strict: true });
// ItemArt's resolver and SVG renderer are pure; only paint needs browser Canvas/Image APIs.
vm.runInNewContext(fs.readFileSync(path.join(root, 'js/itemart.js'), 'utf8').replace('export const ItemArt = DT.ItemArt;', ''),
  { window: { DT }, location: { protocol: 'file:' }, console });
let checks = 0, mappings = 0; const images = new Set();
function check(condition, message) { checks++; assert.ok(condition, message); }
const sha = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
for (const [file, expected] of Object.entries(manifest.source)) check(sha(fs.readFileSync(path.join(root, file))) === expected, 'Stale source: ' + file);
const catalog = JSON.parse(fs.readFileSync(path.join(root, 'native/content/catalog.json'), 'utf8'));
check(catalog.variants.every(variant => manifest.variants[variant.id]), 'Every native variant has choice metadata');
for (const [variant, kinds] of Object.entries(manifest.variants)) {
  DT.Sets.activate(...variant.split('/'));
  for (const [kind, choices] of Object.entries(kinds)) for (const [id, expected] of Object.entries(choices)) {
    let token = null; const original = DT.ItemArt.img;
    DT.ItemArt.img = (...args) => { check(token === null, 'Only one source image per card'); token = args[0]; return original(...args); };
    let card, tier = null;
    try {
      if (kind === 'augment' || kind === 'power') {
        const definition = (kind === 'power' ? DT.POWER[id] : DT.AUGMENT[id]);
        check(!!definition, variant + '/' + kind + '/' + id + ' definition');
        tier = definition.tier || 1;
        card = { img: DT.ItemArt.img('aug:' + id, 64, 'augart') };
      } else card = DT.SET.mechanic.offerKinds[kind].card(id);
    } finally { DT.ItemArt.img = original; }
    const label = variant + '/' + kind + '/' + id;
    check(expected.token === token, label + ' source token');
    check(expected.tier === tier, label + ' source tier');
    check(!!expected.file === !!card.img, label + ' intentionally absent or present image');
    if (expected.file) images.add(expected.file);
    mappings++;
  }
  for (const [kind, definitions] of [['augment', [...DT.AUGMENTS, ...(DT.Duos?.AUGMENTS || [])]], ['power', DT.POWERS]])
    for (const definition of definitions) check(!!kinds[kind]?.[definition.id], variant + ' complete ' + kind + ' metadata');
}
for (const hash of images) {
  const file = path.join(folder, hash + '.png'); check(fs.existsSync(file), 'Available native image: ' + hash);
  const bytes = fs.readFileSync(file);
  check(sha(bytes) === hash, 'Content hash: ' + hash);
  check(bytes.subarray(0, 8).equals(Buffer.from([137, 80, 78, 71, 13, 10, 26, 10])), 'PNG signature');
  check(bytes.readUInt32BE(16) === manifest.size && bytes.readUInt32BE(20) === manifest.size, 'Native texture dimensions');
}
for (const sample of manifest.samples) {
  DT.Sets.activate(...sample.variant.split('/'));
  const item = DT.ITEMS[sample.id];
  if (['rec_bless', 'rec_radiant'].includes(sample.kind)) check(item?.cat === 'radiant', 'Radiant sample is a Radiant offer');
  if (sample.kind === 'rec_tome') check(!!item?.emblem, 'Tome sample is a Crest');
  if (sample.kind === 'ac_relic') check(!!item?.relic, 'Relic sample is a Relic');
  if (sample.kind === 'arcMarket') check(item?.cat === 'contraband', 'Market sample is Contraband');
  const tokens = []; const original = DT.ItemArt.img;
  DT.ItemArt.img = (id, ...args) => { tokens.push(id); return original(id, ...args); };
  try {
    if (sample.kind === 'augment' || sample.kind === 'power') DT.ItemArt.img('aug:' + sample.id);
    else DT.SET.mechanic.offerKinds[sample.kind].card(sample.id);
  } finally { DT.ItemArt.img = original; }
  check(tokens.length === 1 && tokens[0] === sample.art.token && images.has(sample.art.file), 'Reachable UI sample uses exported original artwork');
}
console.log(`Choice artwork: ${checks} checks passed for ${mappings} source cards and ${images.size} native PNGs.`);
