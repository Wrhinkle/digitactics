// Browser check: serves the game, plays Hyper Roll rounds through the real UI headless, saves screenshots.
// Run: node tests/browser.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = Number(process.env.PORT || 8137);

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' || m.type() === 'warning') errors.push(m.text()); });
  const reqs = { glb: 0, portraits: 0 };
  page.on('request', (r) => { if (r.url().endsWith('.glb')) reqs.glb++; else if (r.url().includes('models/portraits/')) reqs.portraits++; });
  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2500);
  // title screen: shop pictures come from pre-rendered files, no model downloads yet
  const titleLoads = { ...reqs };
  await page.screenshot({ path: path.join(out, '0-title.png') });
  await page.click('#startBtn');
  await page.waitForTimeout(500);
  // the planning timer would fire during slow headless steps
  await page.click('#timerBtn');

  // opening carousel
  await page.screenshot({ path: path.join(out, '1-carousel.png') });
  await page.locator('.choice[data-carousel]:not(.taken)').first().click();
  await page.click('#carouselGo');
  await page.waitForTimeout(300);

  const box = await page.locator('#board').boundingBox();
  const pt = async (expr) => { const p = await page.evaluate(expr); return { x: box.x + p.x, y: box.y + p.y }; };
  const bench = (i) => pt(`window.__digi.renderer.screenOfBench(${i})`);
  const cell = (c, r) => pt(`window.__digi.renderer.screenOfCell(${c}, ${r})`);
  async function drag(a, b) {
    await page.mouse.move(a.x, a.y);
    await page.mouse.down();
    await page.mouse.move((a.x + b.x) / 2, (a.y + b.y) / 2, { steps: 5 });
    await page.mouse.move(b.x, b.y, { steps: 5 });
    await page.mouse.up();
  }
  const state = () => page.evaluate(() => {
    const g = window.__digi.game, p = g.human;
    return { round: g.roundLabel, phase: g.phase, gold: p.gold, hp: p.hp, items: p.items.length, offer: p.offer && p.offer.kind, wisp: !!p.wisp,
      board: p.boardCount(), cap: p.cap(), equipped: p.allUnits().reduce((n, u) => n + u.items.length, 0), augs: p.augs.length, over: g.over };
  });

  const spots = [[3, 4], [2, 7], [4, 4], [4, 7], [2, 4], [3, 7], [5, 4], [1, 7], [1, 4]];
  let clipsSeen = 0;
  let equippedOnce = false, wispBought = false, offersSeen = new Set(), orbsWalked = 0;
  for (let round = 0; round < 14; round++) {
    let s = await state();
    if (s.over) break;
    if (s.offer) {
      offersSeen.add(s.offer);
      if (s.offer === 'augment' && !offersSeen.has('shot')) { offersSeen.add('shot'); await page.screenshot({ path: path.join(out, '2-augment.png') }); }
      await page.locator('.choice[data-offer]').first().click();
    }
    for (let i = 0; i < 5; i++) {
      const card = page.locator(`.card[data-slot="${i}"]:not(.poor)`);
      if (await card.count()) await card.click();
    }
    s = await state();
    if (s.wisp && !wispBought && (await page.locator('.card.wisp:not(.poor)').count())) { await page.click('.card.wisp'); wispBought = true; }
    for (let k = 0; k < 2; k++) {
      const sp = spots[(round * 2 + k) % spots.length];
      await drag(await bench(0), await cell(sp[0], sp[1]));
    }
    // walk the Little Legend into any loot orbs (right-click, no drag)
    const orbs = await page.evaluate(() => window.__digi.game.human.orbs.map((o) => { const w = window.__digi.renderer.orbWorld(o); return window.__digi.renderer.screenOfWorld(w.x, w.z); }));
    for (const o of orbs) {
      await page.mouse.click(box.x + o.x, box.y + o.y, { button: 'right' });
      await page.waitForTimeout(1600);
    }
    if (orbs.length) orbsWalked += orbs.length - (await page.evaluate(() => window.__digi.game.human.orbs.length));
    // drag items from the item bench pedestals onto the first board Digimon
    s = await state();
    if (s.items > 0 && s.board > 0) {
      const target = await page.evaluate(() => {
        const [k] = Object.keys(window.__digi.game.human.board);
        const [c, r] = k.split(',').map(Number);
        return window.__digi.renderer.screenOfCell(c, r);
      });
      for (let n = 0; n < 2; n++) {
        if (!(await state()).items) break;
        const slot = await pt('window.__digi.renderer.screenOfItemSlot(0)');
        await drag(slot, { x: box.x + target.x, y: box.y + target.y - 10 });
      }
      if ((await state()).equipped > 0) equippedOnce = true;
    }
    if (round === 3) await page.screenshot({ path: path.join(out, '2b-items.png') });
    await page.mouse.move((await cell(3, 4)).x, (await cell(3, 4)).y);
    if (round === 5) await page.screenshot({ path: path.join(out, '3-planning.png') });
    await page.keyboard.press('Space');
    await page.waitForTimeout(round === 5 ? 3500 : 300);
    if (round === 5) await page.screenshot({ path: path.join(out, '4-combat.png') });
    // count units that played an attack/special clip this fight
    if (round >= 3) clipsSeen += await page.evaluate(() => [...window.__digi.renderer.views.values()].filter((v) => v.oneShot || v.playedRoles).length);
    if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
    await page.waitForTimeout(2600);
    if (await page.locator('#overlay:not(.hidden)').count()) break;
  }
  const s = await state();
  await page.screenshot({ path: path.join(out, '5-later.png') });
  console.log('state:', JSON.stringify(s));
  console.log('orbs picked up by walking:', orbsWalked, '| equipped an item:', equippedOnce, '| bought a wisp:', wispBought, '| offers seen:', [...offersSeen].filter((x) => x !== 'shot').join(','));
  console.log('sounds:', JSON.stringify(await page.evaluate(() => ({ running: window.DT.Sfx.running, music: window.DT.Sfx.musicOn, plays: window.DT.Sfx.stats }))));
  console.log('title screen loads:', JSON.stringify(titleLoads), '| units mid-clip during combat checks:', clipsSeen);
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  await browser.close();
  server.kill();
})();
