// Pre-renders a portrait for every Digimon form into models/portraits/ with the game's own portrait renderer, and
// writes models/portraits/index.json ({ "line:stage": { file, src } }). The game uses a file only while src still
// matches the model in models/index.json, so re-run this after changing models.
// Keys: every form in DT.LINES plus every other models/index.json key (extra:<Name> models the roster has not placed yet).
// Run: node tools/render-portraits.cjs <path-to-playwright> [size] [filter]
//   filter: only re-render keys containing this text (or a /regex/); the other entries in models/portraits are kept.
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const SIZE = Number(process.argv[3]) || 256;
const FILTER = process.argv[4] || '';
const match = (k) => !FILTER || (/^\/.*\/$/.test(FILTER) ? new RegExp(FILTER.slice(1, -1)).test(k) : k.includes(FILTER));
const PORT = Number(process.env.PORT) || 8141;
const root = path.join(__dirname, '..');
const outDir = path.join(root, 'models', 'portraits');

(async () => {
  const server = spawn(process.execPath, [path.join(root, 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const page = await browser.newPage();
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForFunction(() => window.DT && window.DT.Portraits);
    const lineKeys = await page.evaluate(() => window.DT.LINES.flatMap((l) => l.forms.map((f, s) => `${l.id}:${s}`)));
    const modelIndex = JSON.parse(fs.readFileSync(path.join(root, 'models', 'index.json'), 'utf8'));
    const keys = [...new Set([...lineKeys, ...Object.keys(modelIndex)])].filter(match);
    let index = {};
    if (FILTER) {
      // keep the other portraits; drop the old files of the keys being re-rendered
      try { index = JSON.parse(fs.readFileSync(path.join(outDir, 'index.json'), 'utf8')); } catch (e) { index = {}; }
      for (const k of keys) if (index[k]) { fs.rmSync(path.join(outDir, index[k].file), { force: true }); delete index[k]; }
    } else fs.rmSync(outDir, { recursive: true, force: true });
    fs.mkdirSync(outDir, { recursive: true });
    for (const key of keys) {
      const r = await page.evaluate(async ([k, size]) => {
        const url = await window.DT.Portraits.build(k, size);
        const idx = await (await fetch('models/index.json')).json();
        return { url, src: idx[k] || 'ph' };
      }, [key, SIZE]);
      const m = /^data:image\/(webp|png);base64,(.*)$/.exec(r.url || '');
      if (!m) { console.log(`fail ${key}`); continue; }
      const buf = Buffer.from(m[2], 'base64');
      const hash = crypto.createHash('sha1').update(buf).digest('hex').slice(0, 8);
      const file = `${key.replace(':', '_').replace(/[^\w.-]/g, '_')}.${hash}.${m[1]}`;
      fs.writeFileSync(path.join(outDir, file), buf);
      index[key] = { file, src: r.src };
      process.stdout.write('.');
    }
    fs.writeFileSync(path.join(outDir, 'index.json'), JSON.stringify(index, null, 1));
    const bytes = fs.readdirSync(outDir).reduce((s, f) => s + fs.statSync(path.join(outDir, f)).size, 0);
    console.log(`\n${Object.keys(index).length} / ${keys.length} portraits, ${(bytes / 1e6).toFixed(2)} MB in models/portraits`);
  } finally {
    if (errors.length) console.log('errors:', [...new Set(errors)].slice(0, 8).join('\n'));
    await browser.close();
    server.kill();
  }
})();
