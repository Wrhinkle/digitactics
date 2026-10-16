// Rasterizes the item portraits from js/itemart.js (items, augments, Data Wisps) into models/items/*.webp and writes
// models/items/index.json ({ id: file }). The game uses these files and falls back to the SVGs when one is missing.
// Re-run after changing js/itemart.js (and rebuild js/bundle.js first, since the page loads the bundle).
// Run: node tools/render-item-icons.cjs <path-to-playwright> [quality 0-1] [--set <id>]
// With --set, only that set's own art (DT.Sets.extend(id, { art: { items } }) and its auto crests) is rendered, into
// models/items/sets/<id>/ with its own index.json; set art.prerendered: true in js/sets/<id>-art.js to use the files.
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { spawn } = require('child_process');
const { argSet, stripSet } = require('./headless.cjs');
const SET = argSet(process.argv);
const argv = stripSet(process.argv);
const { chromium } = require(argv[2] || 'playwright');
const QUALITY = Number(argv[3]) || 0.9;
const PORT = Number(process.env.PORT || 8151);
const root = path.join(__dirname, '..');
const outDir = SET ? path.join(root, 'models', 'items', 'sets', SET) : path.join(root, 'models', 'items');

(async () => {
  const server = spawn(process.execPath, [path.join(root, 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  const browser = await chromium.launch();
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForFunction(() => window.DT && window.DT.ItemArt);
    // [id, key]: key is what the index stores ('<id>' for base art, '<set>/<id>' for a set's own)
    const ids = await page.evaluate((set) => {
      const D = window.DT;
      if (set) D.Sets.activate(set);
      return D.ItemArt.ids().map((id) => [id, D.ItemArt.resolve(id).key]).filter(([, k]) => (set ? k.startsWith(`${set}/`) : !k.includes('/')));
    }, SET);
    fs.mkdirSync(outDir, { recursive: true });
    // clear the old files of this run only (the base run keeps models/items/sets/)
    for (const f of fs.readdirSync(outDir)) if (f.endsWith('.webp') || f === 'index.json') fs.rmSync(path.join(outDir, f));
    const index = {};
    for (const [id, key] of ids) {
      const url = await page.evaluate(([id, q]) => new Promise((res) => {
        const A = window.DT.ItemArt;
        const im = new Image();
        im.onload = () => {
          const cv = document.createElement('canvas');
          cv.width = cv.height = A.size;
          cv.getContext('2d').drawImage(im, 0, 0, A.size, A.size);
          res(cv.toDataURL('image/webp', q));
        };
        im.onerror = () => res('');
        im.src = A.svgUrl(id);
      }), [id, QUALITY]);
      const m = /^data:image\/webp;base64,(.*)$/.exec(url);
      if (!m) { console.log(`fail ${id}`); continue; }
      const buf = Buffer.from(m[1], 'base64');
      const hash = crypto.createHash('sha1').update(buf).digest('hex').slice(0, 8);
      const file = `${id.replace(/[^a-z0-9_]/gi, '_')}.${hash}.webp`;
      fs.writeFileSync(path.join(outDir, file), buf);
      index[key] = file;
      process.stdout.write('.');
    }
    fs.writeFileSync(path.join(outDir, 'index.json'), JSON.stringify(index, null, 1));
    const bytes = fs.readdirSync(outDir).reduce((s, f) => s + fs.statSync(path.join(outDir, f)).size, 0);
    console.log(`\n${Object.keys(index).length} / ${ids.length} icons, ${(bytes / 1024).toFixed(0)} KB in ${path.relative(root, outDir)}`);
  } finally {
    if (errors.length) console.log('errors:', [...new Set(errors)].slice(0, 8).join('\n'));
    await browser.close();
    server.kill();
  }
})();
