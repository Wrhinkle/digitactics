// Item drag check: combine preview between bench items, recipe grid, drop-on-Digimon preview, swap on drop, layout at 1200x760.
// Run: node tests/itemdrag.cjs <path-to-playwright> <outDir> [path-to-esbuild]
// Passing esbuild tests current sources in memory without changing the checked-in bundle.
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8152;
const sourceBundle = process.argv[4] ? (() => {
  const root = path.join(__dirname, '..');
  return require(path.resolve(process.argv[4])).buildSync({
    absWorkingDir: root, entryPoints: [path.join(root, 'js/main.js')],
    bundle: true, format: 'iife', write: false,
    alias: { three: path.join(root, 'vendor/three/build/three.module.js'), 'three/addons': path.join(root, 'vendor/three/addons') },
    logLevel: 'warning',
  }).outputFiles[0].text;
})() : null;

let fails = 0;
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails++; };

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1200, height: 760 }, hasTouch: true });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  try {
    if (sourceBundle) await page.route('**/js/bundle.js', route => route.fulfill({ contentType: 'application/javascript', body: sourceBundle }));
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);
    await page.click('#startBtn');
    await page.waitForTimeout(400);
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(400);

    // known item bench and three bench Digimon: one holding a Chrome Plate, one full, one bare
    await page.evaluate(() => {
      const g = window.__digi.game, p = g.human;
      p.items = ['sword', 'bow', 'rod', 'egg', 'omni_blade', 'remover', 'sword'];
      const line = Object.keys(window.DT.LINE).find((id) => window.DT.LINE[id].shop && window.DT.LINE[id].tier === 1);
      for (let i = 0; i < 3; i++) { const u = g.newUnit(line, 0); p.bench[i] = u; }
      p.bench[0].items = ['vest'];
      p.bench[1].items = ['giga_belt', 'dragon_scale', 'spike_plate'];
      p.bench[2].items = [];
    });
    await page.waitForTimeout(300);

    const box = await page.locator('#board').boundingBox();
    const pt = async (expr) => { const p = await page.evaluate(expr); return { x: box.x + p.x, y: box.y + p.y }; };
    const slot = (i) => pt(`window.__digi.renderer.screenOfItemSlot(${i})`);
    const bench = (i) => pt(`window.__digi.renderer.screenOfBench(${i})`);
    const tip = () => page.evaluate(() => {
      const el = document.getElementById('tooltip');
      const r = el.getBoundingClientRect();
      return { shown: !el.classList.contains('hidden'), card: el.classList.contains('icard'), text: el.innerText.replace(/\s+/g, ' '),
        html: el.innerHTML, rect: { l: r.left, t: r.top, r: r.right, b: r.bottom }, W: innerWidth, H: innerHeight,
        cells: [...el.querySelectorAll('.ic-cell')].map((c) => ({ id: c.dataset.recipe, have: c.classList.contains('have') })) };
    });
    const onScreen = (t) => t.rect.l >= 0 && t.rect.t >= 0 && t.rect.r <= t.W && t.rect.b <= t.H;
    const items = () => page.evaluate(() => window.__digi.game.human.items.slice());

    // 1. hover a component: card with the TFT recipe row
    let a = await slot(0);
    await page.mouse.move(a.x, a.y, { steps: 3 });
    await page.waitForTimeout(150);
    let t = await tip();
    check(t.shown && t.card, 'hovering an item shows the item card');
    check(t.cells.length === 9, `recipe grid has 9 combinations (got ${t.cells.length})`);
    const have = Object.fromEntries(t.cells.map((c) => [c.id, c.have]));
    check(have.giga_slayer && have.data_gunblade && have.omni_blade && have.emblem_striker, 'recipes you can make now are marked (Speed Chip, Data Rod, second Power Chip, Digi-Egg)');
    check(!have.crit_edge && have.night_edge, 'Lucky Glove (not owned) is not marked; Chrome Plate held on a Digimon counts');
    check(/you can make now/.test(t.text), 'grid header counts makeable recipes');
    check(onScreen(t), 'item card fits on screen');
    await page.screenshot({ path: path.join(out, 'itemdrag-1-hover.png') });

    // 2. drag Power Chip over Speed Chip: combine preview
    await page.mouse.down();
    let b = await slot(1);
    await page.mouse.move(b.x, b.y, { steps: 6 });
    await page.waitForTimeout(150);
    t = await tip();
    check(t.card && /Giga Slayer/.test(t.text), 'Power Chip over Speed Chip previews Giga Slayer');
    check(/\+15% AD, \+12% AS/.test(t.text), 'preview shows both components\' stats');
    check(/1600 max HP/.test(t.text), 'preview shows the full effect text');
    check((t.html.match(/class="ic-eq"/g) || []).length === 1 && /data-item="sword"/.test(t.html) && /data-item="bow"/.test(t.html), 'preview shows A + B → result');
    check(/swap/i.test(t.text), 'preview says dropping here swaps');
    check(onScreen(t), 'combine preview fits on screen');
    await page.screenshot({ path: path.join(out, 'itemdrag-2-combine.png') });

    // live update: over the second Power Chip, the Digi-Egg, a completed item, the Remover
    b = await slot(6); await page.mouse.move(b.x, b.y, { steps: 4 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Omni Blade/.test(t.text) && /includes \+15% AD bonus/.test(t.text), 'updates live: two Power Chips preview Omni Blade with its bonus stats');
    b = await slot(3); await page.mouse.move(b.x, b.y, { steps: 4 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Striker Crest/.test(t.text) && /Adds Striker/.test(t.text), 'Power Chip over Digi-Egg previews the Striker Crest and its trait');
    await page.screenshot({ path: path.join(out, 'itemdrag-3-crest.png') });
    b = await slot(4); await page.mouse.move(b.x, b.y, { steps: 4 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Doesn't combine/.test(t.text), 'component over a completed item shows "Doesn\'t combine"');
    await page.screenshot({ path: path.join(out, 'itemdrag-4-nocombine.png') });
    b = await slot(5); await page.mouse.move(b.x, b.y, { steps: 4 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Doesn't combine/.test(t.text) && /Magnetic Remover/.test(t.text), 'component over the Remover explains what the Remover does');

    // 3. drop on the Speed Chip: pedestals swap, nothing combines
    b = await slot(1); await page.mouse.move(b.x, b.y, { steps: 4 }); await page.waitForTimeout(100);
    await page.mouse.up();
    await page.waitForTimeout(200);
    let it = await items();
    check(it[0] === 'bow' && it[1] === 'sword' && it.length === 7 && !it.includes('giga_slayer'), `drop on a bench item swaps them (now ${it.slice(0, 2).join(', ')})`);

    // 4. drag onto Digimon: combine with its component, full, crest, equip
    a = await slot(1);
    await page.mouse.move(a.x, a.y, { steps: 3 });
    await page.mouse.down();
    let u = await bench(0);
    await page.mouse.move(u.x, u.y - 10, { steps: 8 });
    await page.waitForTimeout(150);
    t = await tip();
    check(t.card && /Combines with its Chrome Plate into Night Edge/.test(t.text), 'over a Digimon holding Chrome Plate: "Combines with its Chrome Plate into Night Edge"');
    check(/60%/.test(t.text) && /Items after: 1\/3/.test(t.text), 'unit preview shows the effect and slot count');
    check(onScreen(t), 'unit preview fits on screen');
    await page.screenshot({ path: path.join(out, 'itemdrag-5-unit.png') });
    u = await bench(1); await page.mouse.move(u.x, u.y - 10, { steps: 6 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Full: 3 items/.test(t.text), 'over a full Digimon: "Full: 3 items"');
    u = await bench(2); await page.mouse.move(u.x, u.y - 10, { steps: 6 }); await page.waitForTimeout(120);
    t = await tip();
    check(/Equips Power Chip/.test(t.text) && /\+15% AD/.test(t.text), 'over a bare Digimon: equips and shows what it gains');
    u = await bench(0); await page.mouse.move(u.x, u.y - 10, { steps: 6 }); await page.waitForTimeout(120);
    await page.mouse.up();
    await page.waitForTimeout(200);
    const held = await page.evaluate(() => window.__digi.game.human.bench[0].items.slice());
    check(held.includes('night_edge'), `drop on the Digimon combines there (holds ${held.join(', ')})`);

    // crest onto a Digimon: the trait it adds
    await page.evaluate(() => { const p = window.__digi.game.human; p.items.push('emblem_guardian'); });
    await page.waitForTimeout(150);
    const n = (await items()).length;
    a = await slot(n - 1);
    await page.mouse.move(a.x, a.y, { steps: 3 });
    await page.mouse.down();
    u = await bench(2); await page.mouse.move(u.x, u.y - 10, { steps: 8 }); await page.waitForTimeout(150);
    t = await tip();
    check(/Equips Guardian Crest/.test(t.text) && /Adds Guardian/.test(t.text), 'crest over a Digimon shows the trait it adds');
    await page.screenshot({ path: path.join(out, 'itemdrag-6-crest-unit.png') });
    a = await slot(n - 1); await page.mouse.move(a.x, a.y, { steps: 6 }); await page.mouse.up();

    // 5. no horizontal overflow, and the card stays on screen near the right and bottom edges
    const ov = await page.evaluate(() => ({ sw: document.documentElement.scrollWidth, cw: document.documentElement.clientWidth, bw: document.body.scrollWidth }));
    check(ov.sw <= ov.cw && ov.bw <= ov.cw, `no horizontal overflow (${ov.sw}/${ov.cw})`);
    for (const k of [0, 9]) {
      a = await slot(Math.min(k, (await items()).length - 1));
      await page.mouse.move(a.x, a.y, { steps: 2 }); await page.waitForTimeout(100);
      t = await tip();
      check(t.shown && onScreen(t), `card on screen for pedestal ${k}`);
    }

    // 6. touch: a finger held on a pedestal shows the item card; lifting it hides the card
    await page.mouse.move(box.x + box.width - 20, box.y + 20);
    a = await slot(2);
    await page.evaluate(({ x, y }) => {
      const el = document.getElementById('board');
      const tch = new Touch({ identifier: 1, target: el, clientX: x, clientY: y });
      el.dispatchEvent(new TouchEvent('touchstart', { bubbles: true, cancelable: true, touches: [tch], changedTouches: [tch] }));
    }, a);
    await page.waitForTimeout(150);
    t = await tip();
    const want = await page.evaluate(() => window.DT.ITEMS[window.__digi.game.human.items[2]].name);
    check(t.shown && t.card && t.text.includes(want), `touch hold on a pedestal shows the item card (${want})`);
    await page.evaluate(({ x, y }) => {
      const el = document.getElementById('board');
      const tch = new Touch({ identifier: 1, target: el, clientX: x, clientY: y });
      el.dispatchEvent(new TouchEvent('touchend', { bubbles: true, cancelable: true, touches: [], changedTouches: [tch] }));
    }, a);
    await page.waitForTimeout(100);
    check(!(await page.evaluate(() => window.__digi.ui.itemDrag)), 'touch end releases the drag');

    // A glove merge may return an owned item onto an already full item bench.
    // Its overflow control must still drag through the same equip command as the pedestals.
    await page.evaluate(() => {
      const g = window.__digi.game, p = g.human;
      const line = window.DT.SHOP_LINES.find(l => !l.noItems).id;
      p.board = {}; p.bench.fill(null); p.items = Array(g.R.itemCap).fill('sword');
      for (let i = 0; i < g.copiesOf(line); i++) p.bench[i] = g.newUnit(line, 0);
      p.bench[0].items = ['thief_glove']; p.bench[1].items = ['omni_blade'];
      g.combine(p); p.bench[3] = g.newUnit(line, 0);
      window.DT.UI.refresh();
    });
    const returned = page.locator('[data-returned-item="10"]');
    await returned.waitFor({ state: 'visible', timeout: 5000 });
    check((await items()).length === 11, 'full item bench preserves returned merge item');
    const returnedBox = await returned.boundingBox();
    check(returnedBox && returnedBox.x >= 0 && returnedBox.x + returnedBox.width <= 1200 && returnedBox.y + returnedBox.height <= 760,
      'returned item control is visible within the viewport');
    await page.screenshot({ path: path.join(out, 'itemdrag-7-overflow.png') });
    await page.mouse.move(returnedBox.x + returnedBox.width / 2, returnedBox.y + returnedBox.height / 2);
    await page.mouse.down();
    u = await bench(3); await page.mouse.move(u.x, u.y - 10, { steps: 8 });
    await page.waitForTimeout(100); await page.mouse.up(); await page.waitForTimeout(150);
    check(await page.evaluate(() => window.__digi.game.human.bench[3].items.includes('omni_blade') && window.__digi.game.human.items.length === 10),
      'returned item equips from overflow without loss');
    check(await page.locator('[data-returned-item]').count() === 0, 'overflow controls disappear after equipping return');
    check(!errors.length, `no page errors${errors.length ? ': ' + errors.join(' | ') : ''}`);
  } catch (error) {
    await page.screenshot({ path: path.join(out, 'itemdrag-failure.png') });
    throw error;
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fails ? `${fails} check(s) failed` : 'all item drag checks passed');
  process.exit(fails ? 1 : 0);
})();
