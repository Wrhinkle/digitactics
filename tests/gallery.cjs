// Model gallery: places the given forms on the board in the real renderer and screenshots them, 14 per shot.
// Run: node tests/gallery.cjs <path-to-playwright> <outDir> [prefix]   (prefix filters model files, default rip_)
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const prefix = process.argv[4] || 'rip_';
const PORT = 8139;

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' || m.type() === 'warning') errors.push(m.text()); });
  await page.goto(`http://localhost:${PORT}/`);
  await page.click('#startBtn');
  await page.click('#timerBtn');
  const keys = await page.evaluate(async (pre) => {
    const idx = await (await fetch('models/index.json')).json();
    const seen = new Set();
    return Object.entries(idx).filter(([, f]) => f.includes(pre) && !seen.has(f) && seen.add(f)).map(([k]) => k);
  }, prefix);
  console.log(keys.length, 'models');
  for (let s = 0; s * 14 < keys.length; s++) {
    const batch = keys.slice(s * 14, s * 14 + 14);
    await page.evaluate((batch) => {
      const d = window.__digi;
      const g = d.game;
      g.carousel = null;
      g.roundType = 'pvp';
      const p = g.human;
      p.board = {};
      p.bench = p.bench.map(() => null);
      batch.forEach((k, i) => {
        const [line, stage] = k.split(':');
        p.board[batch.length <= 2 ? `${3 + i},6` : `${(i % 7)},${i < 7 ? 5 : 7}`] = g.newUnit(line, Number(stage));
      });
      document.getElementById('modal').classList.add('hidden');
      d.renderer.resetView();
      d.renderer.view.zoom = batch.length <= 2 ? 0.3 : 0.62;
      d.renderer.view.target.z = batch.length <= 2 ? 1.6 : 3.2;
      d.renderer.applyView();
    }, batch);
    await page.waitForTimeout(6000);
    await page.evaluate(() => document.getElementById('modal').classList.add('hidden'));
    await page.screenshot({ path: path.join(out, `gallery-${s}.png`) });
    console.log(`gallery-${s}: ${batch.join(' ')}`);
  }
  console.log('errors:', errors.length ? [...new Set(errors)].slice(0, 8).join('\n') : 'none');
  await browser.close();
  server.kill();
})();
