// Item portrait check: every item, augment and Data Wisp has art, the pre-rendered files load, and a contact sheet
// of all icons at 32 and 64 px is saved for a visual check.
// Run: node tests/itemart.cjs <path-to-playwright> [sheet.png] [--svg]   (--svg ignores models/items and draws the SVGs)
const path = require('path');
const os = require('os');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const args = process.argv.slice(3);
const useSvg = args.includes('--svg');
const out = args.find((a) => !a.startsWith('--')) || path.join(os.tmpdir(), 'item-sheet.png');
const PORT = 8151;

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  const browser = await chromium.launch();
  const page = await browser.newPage({ viewport: { width: 1400, height: 900 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  let fails = 0;
  const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fails++; };
  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForFunction(() => window.DT && window.DT.ItemArt);
    const r = await page.evaluate(async (useSvg) => {
      const DT = window.DT, A = DT.ItemArt;
      const index = await A.ready;
      const want = [...Object.keys(DT.ITEMS), ...DT.AUGMENTS.map((a) => `aug:${a.id}`), ...DT.WISPS.map((w) => `wisp:${w.id}`)];
      const missing = want.filter((id) => !A.has(id) || !A.svg(id).startsWith('<svg'));
      const load = (src) => new Promise((res) => { const im = new Image(); im.onload = () => res(im.naturalWidth); im.onerror = () => res(0); im.src = src; });
      const badSvg = [], badFile = [];
      for (const id of want) {
        if (!(await load(A.svgUrl(id)))) badSvg.push(id);
        if (index[id] && (await load('models/items/' + index[id])) !== 128) badFile.push(id);
      }
      const unrendered = want.filter((id) => !index[id]);
      // the DOM helpers callers use
      const iconOk = want.filter((id) => DT.ITEMS[id]).every((id) => DT.itemIcon(id).length > 0);
      const cv = document.createElement('canvas'); cv.width = cv.height = 128;
      const painted = await A.paint(cv, 'omni_blade');
      const px = cv.getContext('2d').getImageData(64, 64, 1, 1).data[3];
      return { n: want.length, missing, badSvg, badFile, unrendered, iconOk, painted, px };
    }, useSvg);
    check(r.missing.length === 0, `${r.n} ids have art${r.missing.length ? ': missing ' + r.missing.join(', ') : ''}`);
    check(r.badSvg.length === 0, `all SVGs decode${r.badSvg.length ? ': ' + r.badSvg.join(', ') : ''}`);
    check(r.badFile.length === 0, `pre-rendered files load at 128 px${r.badFile.length ? ': ' + r.badFile.join(', ') : ''}`);
    if (!useSvg) check(r.unrendered.length === 0, `every id has a pre-rendered file${r.unrendered.length ? ' (run tools/render-item-icons.cjs): ' + r.unrendered.slice(0, 8).join(', ') : ''}`);
    check(r.iconOk, 'DT.itemIcon returns an image for every item');
    check(r.painted && r.px > 0, 'ItemArt.paint draws into a canvas');

    check(errors.length === 0, `no page errors${errors.length ? ': ' + [...new Set(errors)].slice(0, 5).join(' | ') : ''}`);

    // contact sheet (replaces the page, so the game loop may throw from here on)
    await page.setViewportSize({ width: 1400, height: 2400 });
    await page.evaluate((useSvg) => {
      const DT = window.DT, A = DT.ItemArt;
      const src = (id, s) => (useSvg ? A.svgUrl(id) : A.url(id, s));
      const cell = (id, name, extra = '') => `<div class="c"><img src="${src(id, 64)}" width="64" height="64"><img src="${src(id, 32)}" width="32" height="32">${extra}<span>${name}</span></div>`;
      const comps = DT.COMPONENT_IDS.map((id) => cell(id, DT.ITEMS[id].name)).join('');
      const recipes = DT.CORE_ITEMS.map((id) => { const it = DT.ITEMS[id]; return cell(id, it.name, `<em><img src="${src(it.from[0], 20)}" width="20"><img src="${src(it.from[1], 20)}" width="20"></em>`); }).join('');
      const other = Object.keys(DT.ITEMS).filter((id) => DT.ITEMS[id].emblem || id === 'golden_egg' || DT.ITEMS[id].consumable).map((id) => cell(id, DT.ITEMS[id].name)).join('');
      const augs = DT.AUGMENTS.map((a) => cell(`aug:${a.id}`, a.name)).join('');
      const wisps = DT.WISPS.map((w) => cell(`wisp:${w.id}`, w.name)).join('');
      document.body.innerHTML = `<style>
        body { margin: 0; background: #111621; color: #cfd6e4; font: 12px system-ui, sans-serif; overflow: auto !important; }
        #sheet { padding: 14px; width: 1372px; } h2 { font-size: 14px; margin: 12px 0 6px; color: #fff; }
        .g { display: flex; flex-wrap: wrap; gap: 6px; } .c { width: 128px; height: 104px; background: #1a2130; border-radius: 6px; position: relative; padding: 4px; box-sizing: border-box; }
        .c > img:first-child { position: absolute; left: 4px; top: 4px; } .c > img:nth-child(2) { position: absolute; left: 76px; top: 4px; }
        .c em { position: absolute; left: 74px; top: 42px; display: flex; gap: 2px; } .c span { position: absolute; left: 4px; bottom: 4px; right: 4px; font-size: 11px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
      </style><div id="sheet"><h2>Components</h2><div class="g">${comps}</div><h2>Completed items (with their recipe)</h2><div class="g">${recipes}</div>
        <h2>Crests, Golden Digi-Egg, consumables</h2><div class="g">${other}</div><h2>Augments</h2><div class="g">${augs}</div><h2>Data Wisps</h2><div class="g">${wisps}</div></div>`;
    }, useSvg);
    await page.waitForFunction(() => [...document.images].every((i) => i.complete));
    await page.waitForTimeout(300);
    await page.locator('#sheet').screenshot({ path: out });
    console.log(`sheet: ${out}`);
    if (process.env.ZOOM) { // 2x close-ups of each group, for checking detail
      await page.evaluate(() => { document.body.style.zoom = 2; });
      await page.setViewportSize({ width: 2800, height: 4800 });
      const groups = page.locator('#sheet .g');
      for (let i = 0; i < await groups.count(); i++) await groups.nth(i).screenshot({ path: out.replace(/.png$/, `-zoom${i}.png`) });
    }
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fails ? `${fails} failed` : 'all passed');
  process.exit(fails ? 1 : 0);
})();
