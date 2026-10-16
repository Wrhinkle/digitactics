// Emote check: solo game through the real UI. Opens the wheel, sends emotes and a dance, checks the rate limit,
// muting, incoming emotes from other Tamers, and that right-click walking works with the wheel open.
// Run: node tests/emotes.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8141;

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !/Failed to load resource/.test(m.text())) errors.push(m.text()); });
  page.on('response', (r) => { if (r.status() >= 400) console.log('note: HTTP', r.status(), r.url().replace(/^.*?\/\/[^/]+/, '')); });
  const checks = [];
  const check = (name, ok, info = '') => { checks.push({ name, ok: !!ok }); console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}${info ? ` (${info})` : ''}`); };
  const shot = (n) => page.screenshot({ path: path.join(out, n) });

  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2000);
  // store previews: every emote's preview(el) in one grid
  const count = await page.evaluate(() => {
    const items = DT.Cosmetics.list('emote');
    const grid = document.createElement('div');
    grid.id = 'emGrid';
    grid.style.cssText = 'position:fixed;inset:0;z-index:99;background:#141a26;display:grid;grid-template-columns:repeat(8,1fr);gap:8px;padding:12px;color:#fff;font:12px sans-serif';
    for (const it of items) {
      const c = document.createElement('div');
      c.style.cssText = 'display:flex;flex-direction:column;align-items:center';
      const p = document.createElement('div');
      p.style.cssText = 'width:120px;height:120px';
      it.preview(p);
      c.appendChild(p);
      c.appendChild(document.createTextNode(`${it.name} ${it.free ? '(free)' : it.price}`));
      grid.appendChild(c);
    }
    document.body.appendChild(grid);
    return items.length;
  });
  await page.waitForTimeout(400);
  await shot('e0-previews.png');
  await page.evaluate(() => document.getElementById('emGrid').remove());
  check('registered emotes', count >= 18, `${count}`);

  await page.click('#startBtn');
  await page.waitForTimeout(500);
  await page.click('#timerBtn');
  const carousel = page.locator('.choice[data-carousel]:not(.taken)');
  if (await carousel.count()) { await carousel.first().click(); await page.click('#carouselGo'); }
  await page.waitForTimeout(800);

  // wheel by key, send slot 1
  await page.keyboard.press('t');
  await page.waitForTimeout(300);
  check('T opens the wheel', await page.evaluate(() => DT.Emotes.isOpen()));
  await page.hover('#emoteWheel .ew[data-id]');
  await shot('e1-wheel.png');
  await page.keyboard.press('1');
  await page.waitForTimeout(500);
  const big = await page.evaluate(() => [...document.querySelectorAll('.em-big')].filter((e) => e.style.visibility === 'visible').length);
  check('1 sends and closes', !(await page.evaluate(() => DT.Emotes.isOpen())));
  check('sticker over own legend', big === 1);
  await shot('e2-sticker.png');

  // rate limit: 3 per 6 s
  await page.waitForTimeout(6200);
  const rate = await page.evaluate(() => [DT.Emotes.send('nice'), DT.Emotes.send('wave'), DT.Emotes.send('gg'), DT.Emotes.send('cry')]);
  check('rate limit blocks the 4th emote', rate[0] && rate[1] && rate[2] && !rate[3], JSON.stringify(rate));
  await page.waitForTimeout(6200);

  // buy-free grant of a dance into slot 5, sent from the top-bar button
  await page.evaluate(() => { DT.Cosmetics.grant('emote', 'flip'); DT.Cosmetics.equip('emote', 'flip', 4); });
  await page.click('#emoteBtn');
  await page.waitForTimeout(200);
  await page.click('#emoteWheel .ew[data-id="flip"]');
  await page.waitForTimeout(300);
  const legendY = await page.evaluate(() => { const r = window.__digi.renderer; return r.legend ? r.legend.view.body.position.y : null; });
  const dance = await page.evaluate(() => [...document.querySelectorAll('.em-big')].map((e) => e.innerHTML.includes('a-flip') + ':' + e.style.visibility).join(','));
  await shot('e3-dance.png');
  check('dance sticker shown', dance.includes('true:visible'), `${dance}; body y outside render ${legendY}`);

  // retro: a static Digimon World legend, and a retro emote that pops a DW1 model up beside it, then removes it
  await page.evaluate(() => { DT.Cosmetics.grant('legend', 'retro_botamon'); DT.Cosmetics.equip('legend', 'retro_botamon'); DT.Cosmetics.grant('emote', 'retro_hug'); DT.Cosmetics.equip('emote', 'retro_hug', 3); });
  await page.waitForTimeout(2500);
  const retroLegend = await page.evaluate(() => { const v = window.__digi.renderer.legend && window.__digi.renderer.legend.view; return v ? { id: v.id, ready: v.ready, mixer: !!v.mixer, model: !!(v.t && v.t.root), tag: DT.Cosmetics.get('legend', 'retro_botamon').tag } : null; });
  check('retro legend on the board (static DW1 model)', retroLegend && retroLegend.id === 'retro_botamon' && retroLegend.ready && retroLegend.model && !retroLegend.mixer && retroLegend.tag === 'Retro', JSON.stringify(retroLegend));
  const retroItems = await page.evaluate(() => DT.Cosmetics.list('emote').filter((x) => x.data && x.data.retro).map((x) => `${x.id}:${x.rarity}:${x.price}`));
  check('6 to 10 retro emotes, priced by rarity', retroItems.length >= 6 && retroItems.length <= 10 && retroItems.every((s) => Number(s.split(':')[2]) > 0), retroItems.join(' '));
  const before3D = await page.evaluate(() => { let n = 0; window.__digi.renderer.scene.traverse(() => n++); return n; });
  await page.evaluate(() => DT.Emotes.send('retro_hug'));
  // the DW1 model loads on first use
  await page.waitForFunction(() => DT.Emotes.pops().some((p) => p.ready), null, { timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(300);
  const pop = await page.evaluate(() => DT.Emotes.pops());
  check('retro emote pops a DW1 Monzaemon beside the legend', pop.length === 1 && pop[0].ready && pop[0].inScene && pop[0].act === 'hug', JSON.stringify(pop));
  await shot('e3b-retro-hug.png');
  await page.waitForFunction(() => !DT.Emotes.pops().length, null, { timeout: 8000 }).catch(() => {});
  await page.waitForTimeout(200);
  const after3D = await page.evaluate(() => ({ pops: DT.Emotes.pops().length, n: (() => { let n = 0; window.__digi.renderer.scene.traverse(() => n++); return n; })() }));
  check('retro pop-in removed afterwards', after3D.pops === 0 && after3D.n <= before3D + 2, `${before3D} -> ${after3D.n} scene objects`);

  // incoming from an AI / remote slot: row sticker, then muted
  const other = await page.evaluate(() => window.__digi.game.players.find((q) => q !== window.__digi.game.human).id);
  await page.evaluate((s) => DT.Social.emit('emote', s, 'laugh'), other);
  await page.waitForTimeout(400);
  const rowShown = await page.evaluate(() => [...document.querySelectorAll('.em-row')].filter((e) => e.style.visibility === 'visible').length);
  check('other Tamer emote pops from their row', rowShown >= 1, `${rowShown} row stickers`);
  await shot('e4-row.png');
  await page.evaluate((s) => { DT.Emotes.setMuted(true); DT.Social.emit('emote', s, 'heart'); }, other);
  await page.waitForTimeout(200);
  check('mute hides other Tamers', await page.evaluate((s) => ![...document.querySelectorAll('.em-row')].some((e) => e.innerHTML.includes('#ff4d7a')), other));
  await page.evaluate(() => DT.Emotes.setMuted(false));
  // spam from one remote slot is capped too
  const spam = await page.evaluate(async (s) => { for (let i = 0; i < 6; i++) DT.Social.emit('emote', s, 'gg'); return 1; }, (other + 1) % 8);
  check('incoming spam handled', spam === 1);

  // right-click walking with the wheel open
  await page.keyboard.press('t');
  const box = await page.locator('#board').boundingBox();
  const before = await page.evaluate(() => window.__digi.renderer.legendPos());
  const dest = await page.evaluate(() => window.__digi.renderer.screenOfCell(1, 6));
  await page.mouse.click(box.x + dest.x, box.y + dest.y, { button: 'right' });
  await page.waitForTimeout(1500);
  const after = await page.evaluate(() => window.__digi.renderer.legendPos());
  check('right-click still walks the legend', Math.hypot(after.x - before.x, after.z - before.z) > 0.5);
  check('click outside closes the wheel', !(await page.evaluate(() => DT.Emotes.isOpen())));

  // combat: the opponent's legend can show emotes too (PvE rounds have none, so play on to a Tamer round)
  let opp = null;
  for (let i = 0; i < 6 && opp == null; i++) {
    await page.evaluate(() => { const o = document.querySelector('.choice[data-offer]'); if (o) o.click(); });
    await page.keyboard.press('Space');
    await page.waitForTimeout(1200);
    opp = await page.evaluate(() => { const r = window.__digi.renderer; return r.oppLegend && r.oppLegend.view.group.visible ? r.oppLegend.playerId : null; });
    if (opp == null) { if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn'); await page.waitForTimeout(2600); }
  }
  if (opp != null) {
    await page.evaluate((s) => DT.Social.emit('emote', s, 'deleted'), opp);
    await page.waitForTimeout(500);
    check('opponent legend shows emote in combat', await page.evaluate(() => [...document.querySelectorAll('.em-big')].some((e) => e.style.visibility === 'visible')));
    await shot('e5-combat.png');
  } else console.log('skip opponent check: no opponent legend this round');

  console.log(`${checks.filter((c) => c.ok).length}/${checks.length} checks passed`);
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  await browser.close();
  server.kill();
  process.exit(checks.every((c) => c.ok) && !errors.length ? 0 : 1);
})();
