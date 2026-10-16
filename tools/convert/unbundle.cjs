// Splits dt_models_bundle.bin (4-byte header length, JSON index, then the zips back to back) into rips/<name>.zip
// and writes rips/index.json with each model's source page for credits.
// Run: node unbundle.cjs <bundle> <outDir>
const fs = require('fs');
const path = require('path');
const [bundle, out] = process.argv.slice(2);
const buf = fs.readFileSync(bundle);
const n = buf.readUInt32BE(0);
const index = JSON.parse(buf.subarray(4, 4 + n).toString('utf8'));
const base = 4 + n;
fs.mkdirSync(out, { recursive: true });
for (const e of index) {
  e.file = `${e.name.replace(/ /g, '_')}.zip`;
  fs.writeFileSync(path.join(out, e.file), buf.subarray(base + e.offset, base + e.offset + e.length));
}
// merge with rips unpacked from earlier bundles
const ip = path.join(out, 'index.json');
const prev = fs.existsSync(ip) ? JSON.parse(fs.readFileSync(ip, 'utf8')).filter((e) => !index.some((x) => x.name === e.name)) : [];
fs.writeFileSync(ip, JSON.stringify(prev.concat(index), null, 1));
console.log(`wrote ${index.length} zips to ${out}`);
