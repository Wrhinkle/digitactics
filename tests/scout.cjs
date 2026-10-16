// Scouting check: plays two solo rounds through the real UI, visits an AI Tamer's board, then returns.
// Run: node tests/scout.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = Number(process.env.PORT) || 8141;

let failed = 0;
const check = (name, ok, detail = '') => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}${detail ? ` (${detail})` : ''}`); if (!ok) failed++; };

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => { server.stdout.once('data', r); setTimeout(r, 5000); });
  server.stderr.on('data', (d) => console.log('server:', String(d).trim()));
  server.on('exit', (c) => console.log('server exited', c));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error') errors.push(m.text()); });
  page.on('requestfailed', (r) => errors.push(`request failed: ${r.url()}`));
  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);
    await page.click('#startBtn');
    await page.waitForTimeout(500);
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(300);

    const box = await page.locator('#board').boundingBox();
    const pt = async (expr) => { const p = await page.evaluate(expr); return { x: box.x + p.x, y: box.y + p.y }; };
    async function drag(a, b) {
      await page.mouse.move(a.x, a.y);
      await page.mouse.down();
      await page.mouse.move((a.x + b.x) / 2, (a.y + b.y) / 2, { steps: 5 });
      await page.mouse.move(b.x, b.y, { steps: 5 });
      await page.mouse.up();
    }
    const phase = () => page.evaluate(() => window.__digi.game.phase);

    // two rounds so the AIs have boards
    const spots = [[3, 4], [2, 7], [4, 4], [4, 7]];
    for (let round = 0; round < 2; round++) {
      if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
      for (let i = 0; i < 5; i++) {
        const card = page.locator(`.card[data-slot="${i}"]:not(.poor)`);
        if (await card.count()) await card.click();
      }
      for (let k = 0; k < 2; k++) {
        const sp = spots[(round * 2 + k) % spots.length];
        await drag(await pt('window.__digi.renderer.screenOfBench(0)'), await pt(`window.__digi.renderer.screenOfCell(${sp[0]}, ${sp[1]})`));
      }
      await page.keyboard.press('Space');
      await page.waitForTimeout(400);
      if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
      await page.waitForTimeout(2800);
    }
    if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
    check('back in planning after two rounds', (await phase()) === 'planning', await page.evaluate(() => window.__digi.game.roundLabel));

    // the AI with the most units
    const target = await page.evaluate(() => {
      const g = window.__digi.game;
      const q = g.players.filter((x) => x !== g.human && x.alive).sort((a, b) => b.allUnits().length - a.allUnits().length)[0];
      return { id: q.id, name: q.name };
    });
    const humanBefore = await page.evaluate(() => JSON.stringify(Object.keys(window.__digi.game.human.board).sort()));

    // what the 3D view shows vs. what that player has
    // read after a fresh frame so the views reflect the current state
    const scene = (id) => page.evaluate(async (id) => {
      await new Promise((res) => requestAnimationFrame(() => requestAnimationFrame(res)));
      const { game: g, renderer: r, ui } = window.__digi;
      const p = id == null ? g.human : g.players[id];
      const units = [...r.views.keys()].filter((k) => k.startsWith('u'));
      const want = [...Object.values(p.board), ...p.bench.filter(Boolean)].map((u) => `u${u.uid}`);
      return {
        viewing: ui.viewing,
        shown: units.length,
        expected: want.length,
        board: Object.keys(p.board).length,
        allMatch: want.every((k) => r.views.has(k)) && units.every((k) => want.includes(k)),
        items: r.itemTokens.filter((t) => t.visible).length,
        expectedItems: p.items.length,
        banner: (document.querySelector('#scoutBanner:not(.hidden)') || {}).textContent || '',
        traitsHead: document.querySelector('#traitList .ph').textContent,
        legend: !!(r.legend && r.legend.view.group.visible),
        phase: g.phase,
      };
    }, id);

    await page.click(`#players .pl[data-pid="${target.id}"]`);
    await page.waitForTimeout(2000); // models and the Little Legend load
    await page.mouse.move(box.x + 10, box.y + 10);
    await page.screenshot({ path: path.join(out, 'scout-1-visit.png') });
    let s = await scene(target.id);
    check('clicking a Tamer row scouts them', s.viewing === target.id, `viewing=${s.viewing}`);
    check('rendered unit count matches their board + bench', s.allMatch && s.shown === s.expected, `shown ${s.shown}, board ${s.board} + bench ${s.expected - s.board}`);
    check('their item bench is shown', s.items === s.expectedItems, `${s.items}/${s.expectedItems}`);
    check('banner names them', s.banner.includes(`Viewing ${target.name}'s board`), s.banner.slice(0, 80));
    check('traits panel is labelled with their name', s.traitsHead.includes(`${target.name}'s traits`), s.traitsHead);
    check('your Little Legend is hidden', !s.legend);

    // read-only: dragging on their board does nothing to yours
    await drag(await pt('window.__digi.renderer.screenOfCell(3, 4)'), await pt('window.__digi.renderer.screenOfBench(5)'));
    await page.waitForTimeout(200);
    check('dragging while scouting does not move units', humanBefore === (await page.evaluate(() => JSON.stringify(Object.keys(window.__digi.game.human.board).sort()))));

    // Esc returns
    await page.keyboard.press('Escape');
    await page.waitForTimeout(700);
    await page.screenshot({ path: path.join(out, 'scout-2-home.png') });
    s = await scene(null);
    const preview = await page.evaluate(() => [...window.__digi.renderer.views.keys()].filter((k) => k.startsWith('pv')).length);
    check('Esc returns to your board', s.viewing == null && s.allMatch && s.shown === s.expected, `viewing=${s.viewing}, shown ${s.shown}/${s.expected}, preview ${preview}`);
    check('banner hidden and traits panel back', !s.banner && s.traitsHead.startsWith('Traits'), s.traitsHead);
    check('your Little Legend is back', s.legend);

    // Q / E cycle, Space returns without starting combat
    await page.keyboard.press('e');
    await page.waitForTimeout(500);
    const afterE = await page.evaluate(() => window.__digi.ui.viewing);
    await page.keyboard.press('e');
    await page.waitForTimeout(500);
    const afterE2 = await page.evaluate(() => window.__digi.ui.viewing);
    check('E cycles to the next Tamer', afterE != null && afterE2 != null && afterE !== afterE2, `${afterE} -> ${afterE2}`);
    await page.keyboard.press('q');
    await page.waitForTimeout(500);
    check('Q cycles back', (await page.evaluate(() => window.__digi.ui.viewing)) === afterE);
    await page.keyboard.press('2');
    await page.waitForTimeout(500);
    const second = await page.evaluate(() => {
      const row = document.querySelectorAll('#players .pl[data-pid]')[1];
      return { pid: Number(row.dataset.pid), viewing: window.__digi.ui.viewing, me: window.__digi.game.human.id };
    });
    check('number key picks that row of the Tamers list', second.viewing === (second.pid === second.me ? null : second.pid), JSON.stringify(second));
    if (second.viewing == null) { await page.keyboard.press('e'); await page.waitForTimeout(500); }
    await page.keyboard.press('Space');
    await page.waitForTimeout(500);
    await page.waitForFunction(() => !window.DT.Scout.busy(), null, { timeout: 5000 }).catch(() => {});
    check('Space returns and does not start combat', (await page.evaluate(() => window.__digi.ui.viewing)) == null && (await phase()) === 'planning');

    // combat start keeps you on the Tamer you are watching (full spectate, tests/spectate.cjs)
    await page.click(`#players .pl[data-pid="${target.id}"]`);
    await page.waitForTimeout(500);
    await page.click('#readyBtn');
    await page.waitForTimeout(300);
    check('combat start keeps the view', (await page.evaluate(() => window.__digi.ui.viewing)) === target.id && (await phase()) === 'combat');
    await page.keyboard.press('Space');
    await page.waitForTimeout(500);
    await page.click('#skipBtn').catch(() => {});
    await page.waitForFunction(() => window.__digi.game.phase === 'result', null, { timeout: 5000 }).catch(() => {});
    // result phase: scouting is allowed
    if ((await phase()) === 'result') {
      await page.click(`#players .pl[data-pid="${target.id}"]`);
      await page.waitForTimeout(500);
      await page.waitForFunction(() => !window.DT.Scout.busy(), null, { timeout: 5000 }).catch(() => {});
      // the result phase shows their fight (tests/spectate.cjs) plus their bench
      const r = await page.evaluate(async (id) => {
        await new Promise((res) => requestAnimationFrame(() => requestAnimationFrame(res)));
        const { game: g, renderer: rr, ui } = window.__digi;
        const p = g.players[id];
        const bench = [...rr.views.keys()].filter((k) => k.startsWith('u')).length;
        return { viewing: ui.viewing, fight: !!window.__digi.shown && window.__digi.shown !== window.__digi.own, bench, expBench: p.bench.filter(Boolean).length };
      }, target.id);
      await page.screenshot({ path: path.join(out, 'scout-3-result.png') });
      check('scouting in the result phase shows their fight and bench', r.viewing === target.id && r.fight && r.bench === r.expBench, JSON.stringify(r));
      await page.click('#scoutHome');
      await page.waitForTimeout(600);
      check('banner button returns', (await page.evaluate(() => window.__digi.ui.viewing)) == null);
    } else console.log('note: result phase already over, skipped the result-phase check');
  } catch (e) {
    check('test ran', false, String(e));
  }
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  if (errors.length) failed++;
  console.log(failed ? `${failed} failed` : 'all passed');
  await browser.close();
  server.kill();
  process.exitCode = failed ? 1 : 0;
})();
