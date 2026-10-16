// Upright audit: renders every model in models/index.json from the front and the side in its idle pose
// (tests/upright.html), checks the numbers, and writes contact sheets plus upright.json to <outDir>.
// Run: node tests/upright.cjs <path-to-playwright> <outDir> [filter] [port]
//   filter: substring of the key or file, or a /regex/ (default: all). Exit code 1 when any model is flagged.
// Checks, in board units after the renderer's auto-scaling:
//   flat      height / longest horizontal side < 0.4 (lying down or tipped over; long dragons can trip it)
//   tiny      shown height < 55% of the stage height (footprint clamp shrank it, usually a lying model)
//   float     the lowest point rises above 0.12 at some time in the idle clip
//   sink      the lowest point drops below -0.12 in the idle clip
//   idleTilt  the idle clip changes the height by more than 40% (clip root rotation differs from the bind pose)
//   back      the head (bone, else top-of-mesh centroid) sits behind the body (-z); the board's front is +z
//   side      the head sits off to the side more than forward
// Flags a person reviewed and accepted are listed per file in tools/convert/upright.json ("ok"); they print as "ok*".
const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || path.join(__dirname, 'upright-out');
const filter = process.argv[4] && process.argv[4] !== 'all' ? process.argv[4] : '';
const PORT = Number(process.argv[5]) || 8155;
const filterRe = /^\/.*\/$/.test(filter) ? new RegExp(filter.slice(1, -1)) : null; // filter /regex/ on the key or file
const PER_SHEET = 16;

function flags(m) {
  const f = [];
  if (m.aspect < 0.4) f.push('flat');
  if (m.shown < m.wantHeight * 0.55) f.push('tiny');
  if (m.idleMinY[1] > 0.12) f.push('float');
  if (m.idleMinY[0] < -0.12) f.push('sink');
  if (m.idleH[1] > m.idleH[0] * 1.4) f.push('idleTilt');
  const dz = m.head ? m.head.z : m.topDz;
  const dx = m.head ? m.head.x : m.topDx;
  if (dz < -0.06 && Math.abs(dz) > Math.abs(dx) * 0.7) f.push('back');
  else if (Math.abs(dx) > 0.12 && Math.abs(dx) > Math.abs(dz) * 1.5) f.push('side');
  return f;
}

(async () => {
  fs.mkdirSync(out, { recursive: true });
  const root = path.join(__dirname, '..');
  const server = spawn(process.execPath, [path.join(root, 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 600));
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const page = await browser.newPage({ viewport: { width: 700, height: 300 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  try {
    await page.goto(`http://localhost:${PORT}/tests/upright.html`);
    await page.waitForFunction(() => window.auditReady, null, { timeout: 20000 });
    const idx = JSON.parse(fs.readFileSync(path.join(root, 'models', 'index.json'), 'utf8'));
    const reviewed = JSON.parse(fs.readFileSync(path.join(root, 'tools', 'convert', 'upright.json'), 'utf8'));
    const keys = Object.keys(idx).filter((k) => !filter || (filterRe ? filterRe.test(k) || filterRe.test(idx[k]) : k.includes(filter) || idx[k].includes(filter)));
    const results = [];
    const tiles = [];
    for (const k of keys) {
      try {
        const r = await page.evaluate(([k, f]) => window.audit(k, f), [k, idx[k]]);
        const all = flags(r.metrics);
        const ok = (reviewed[path.basename(idx[k])] || {}).ok || [];
        r.metrics.flags = all.filter((f) => !ok.includes(f));
        r.metrics.accepted = all.filter((f) => ok.includes(f));
        results.push(r.metrics);
        tiles.push(r.img);
        console.log(`${r.metrics.flags.length ? 'FLAG' : r.metrics.accepted.length ? 'ok* ' : 'ok  '} ${k.padEnd(20)} aspect ${r.metrics.aspect} shown ${r.metrics.shown}/${r.metrics.wantHeight} idleMinY ${r.metrics.idleMinY} idleH ${r.metrics.idleH} head ${r.metrics.head ? `${r.metrics.head.x},${r.metrics.head.z}` : `~${r.metrics.topDx},${r.metrics.topDz}`} ${r.metrics.flags.join(' ')}${r.metrics.accepted.length ? ` (accepted: ${r.metrics.accepted.join(' ')})` : ''}`);
      } catch (e) {
        console.log(`FAIL ${k}: ${String(e).slice(0, 200)}`);
        results.push({ key: k, file: idx[k], error: String(e).slice(0, 300), flags: ['error'] });
      }
    }
    // contact sheets, 2 tiles per row
    for (let s = 0; s * PER_SHEET < tiles.length; s++) {
      const png = await page.evaluate(async (list) => {
        const imgs = await Promise.all(list.map((u) => new Promise((res) => { const i = new Image(); i.onload = () => res(i); i.src = u; })));
        const w = imgs[0].width; const h = imgs[0].height;
        const c = document.createElement('canvas'); c.width = w * 2 + 6; c.height = Math.ceil(imgs.length / 2) * (h + 6);
        const g = c.getContext('2d'); g.fillStyle = '#000'; g.fillRect(0, 0, c.width, c.height);
        imgs.forEach((im, i) => g.drawImage(im, (i % 2) * (w + 6), Math.floor(i / 2) * (h + 6)));
        return c.toDataURL('image/png');
      }, tiles.slice(s * PER_SHEET, s * PER_SHEET + PER_SHEET));
      fs.writeFileSync(path.join(out, `upright-${String(s).padStart(2, '0')}.png`), Buffer.from(png.split(',')[1], 'base64'));
    }
    fs.writeFileSync(path.join(out, 'upright.json'), JSON.stringify(results, null, 1));
    const bad = results.filter((m) => m.flags.length);
    console.log(`\n${results.length} models, ${bad.length} flagged: ${bad.map((m) => `${m.key}[${m.flags.join(',')}]`).join(' ')}`);
    if (errors.length) console.log('page errors:', [...new Set(errors)].slice(0, 5).join('\n'));
    process.exitCode = bad.length ? 1 : 0;
  } finally {
    await browser.close();
    server.kill();
  }
})();
