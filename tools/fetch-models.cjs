// Downloads the Sketchfab models listed in js/models.js into models/src and writes models/index.json.
// Then run tools/convert/optimize.mjs (writes the web-size copies to models/files) and tools/render-portraits.cjs.
// Needs a Sketchfab API token (free account): https://sketchfab.com/settings/password -> API token.
// Run: SKETCHFAB_TOKEN=xxxx node tools/fetch-models.cjs        (PowerShell: $env:SKETCHFAB_TOKEN='xxxx'; node tools/fetch-models.cjs)
const fs = require('fs');
const path = require('path');
const vm = require('vm');
const { execFileSync } = require('child_process');

const token = process.env.SKETCHFAB_TOKEN;
if (!token) {
  console.error('Set SKETCHFAB_TOKEN first (Sketchfab > Settings > Password & API > API token).');
  process.exit(1);
}

const root = path.join(__dirname, '..');
const ctx = {};
ctx.globalThis = ctx;
vm.createContext(ctx);
vm.runInContext(fs.readFileSync(path.join(root, 'js', 'models.js'), 'utf8'), ctx);
const sources = ctx.DT.MODEL_SOURCES;
const filesDir = path.join(root, 'models', 'src');
fs.mkdirSync(filesDir, { recursive: true });
const indexPath = path.join(root, 'models', 'index.json');
const index = fs.existsSync(indexPath) ? JSON.parse(fs.readFileSync(indexPath, 'utf8')) : {};

const findGltf = (dir) => {
  for (const f of fs.readdirSync(dir, { withFileTypes: true })) {
    const p = path.join(dir, f.name);
    if (f.isDirectory()) { const r = findGltf(p); if (r) return r; }
    else if (/\.(gltf|glb)$/i.test(f.name)) return p;
  }
  return null;
};

(async () => {
  const done = {};
  for (const [key, src] of Object.entries(sources)) {
    if (src.rip) continue; // game rips come from tools/convert, not Sketchfab
    if (done[src.uid]) { index[key] = done[src.uid]; continue; }
    const existing = Object.values(index).find((p) => p.includes(src.uid));
    if (existing && fs.existsSync(path.join(filesDir, path.basename(existing)))) { index[key] = done[src.uid] = existing; continue; }
    try {
      const r = await fetch(`https://api.sketchfab.com/v3/models/${src.uid}/download`, { headers: { Authorization: `Token ${token}` } });
      if (!r.ok) throw new Error(`HTTP ${r.status}`);
      const links = await r.json();
      let rel;
      if (links.glb) {
        const buf = Buffer.from(await (await fetch(links.glb.url)).arrayBuffer());
        fs.writeFileSync(path.join(filesDir, `${src.uid}.glb`), buf);
        rel = `models/files/${src.uid}.glb`;
      } else if (links.gltf) {
        const zip = path.join(filesDir, `${src.uid}.zip`);
        fs.writeFileSync(zip, Buffer.from(await (await fetch(links.gltf.url)).arrayBuffer()));
        const dir = path.join(filesDir, src.uid);
        fs.mkdirSync(dir, { recursive: true });
        execFileSync('tar', ['-xf', zip, '-C', dir]);
        fs.unlinkSync(zip);
        const f = findGltf(dir);
        if (!f) throw new Error('no .gltf in archive');
        rel = path.relative(root, f).replace(/\\/g, '/');
      } else throw new Error('no glTF download offered');
      index[key] = done[src.uid] = rel;
      console.log(`ok   ${src.form.padEnd(20)} ${rel}`);
    } catch (e) {
      console.log(`fail ${src.form.padEnd(20)} ${e.message}`);
    }
    fs.writeFileSync(indexPath, JSON.stringify(index, null, 1));
    await new Promise((res) => setTimeout(res, 600));
  }
  fs.writeFileSync(indexPath, JSON.stringify(index, null, 1));
  console.log(`\n${Object.keys(index).length} forms have models. Index: models/index.json`);
})();
