// Boards check: starts a solo game, switches through every board theme three times and screenshots each, checks GPU
// memory does not grow across swaps, runs every board's game reactions, equips a board through DT.Cosmetics (fade),
// checks the combat home-board rule, and saves each store preview image.
// Run: node tests/boards.cjs <path-to-playwright> <outDir>
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8151;

(async () => {
  fs.mkdirSync(out, { recursive: true });
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' || m.type() === 'warning') errors.push(m.text()); });
  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2500);
  await page.click('#startBtn');
  await page.waitForTimeout(500);
  await page.click('#timerBtn');
  await page.locator('.choice[data-carousel]:not(.taken)').first().click();
  await page.click('#carouselGo');
  await page.waitForTimeout(800);

  const ids = await page.evaluate(() => window.DT.Boards.ids);
  const mem = () => page.evaluate(() => ({ ...window.__digi.renderer.r.info.memory }));
  const stats = [];
  // frame time of the live render loop over ~1 s (swiftshader is slow; compare boards against each other)
  const frameMs = () => page.evaluate(() => new Promise((res) => { const t0 = performance.now(); let n = 0; const f = () => { n++; if (performance.now() - t0 < 1000) requestAnimationFrame(f); else res((performance.now() - t0) / n); }; requestAnimationFrame(f); }));
  let first = null;
  const PASSES = 3;
  for (let pass = 0; pass < PASSES; pass++) {
    for (const id of ids) {
      await page.evaluate((id) => window.__digi.renderer.setBoard(id), id);
      await page.waitForTimeout(pass ? 150 : 700);
      if (!pass) {
        const s = await page.evaluate(() => { const r = window.__digi.renderer; return { id: r.board.id, calls: r.r.info.render.calls, tris: r.r.info.render.triangles }; });
        s.ms = await frameMs();
        stats.push(s);
        await page.locator('#arena').screenshot({ path: path.join(out, `board-${id}.png`) });
      }
    }
    if (!pass) first = await mem();
  }
  const second = await mem();
  console.log('boards:', ids.length);
  for (const s of stats) console.log(`  ${s.id.padEnd(22)} draw calls ${String(s.calls).padStart(4)}  triangles ${String(s.tris).padStart(6)}  frame ${s.ms.toFixed(1)} ms`);
  console.log(`gpu memory after pass 1: ${JSON.stringify(first)} | after pass ${PASSES}: ${JSON.stringify(second)}`,
    first.geometries === second.geometries && first.textures === second.textures ? '(no growth)' : '(GROWTH)');

  // reactions: drive every board through combat, a win with a digivolve and a stage change; no errors, and the
  // lights and fog return to the theme's own values on the next swap
  const react = await page.evaluate(async (ids) => {
    const DT = window.DT, r = window.__digi.renderer;
    const wait = (ms) => new Promise((res) => setTimeout(res, ms));
    for (const id of ids) {
      r.setBoard(id);
      DT.Boards.testSignals = { phase: 'planning', stage: 2, won: false, evo: 0 };
      await wait(150);
      DT.Boards.testSignals = { phase: 'combat', stage: 2, won: false, evo: 0 };
      await wait(200);
      DT.Boards.testSignals = { phase: 'combat', stage: 3, won: true, evo: 2 };
      await wait(200);
    }
    DT.Boards.testSignals = null;
    r.setBoard('file-island');
    await wait(50);
    const L = r.boardLights;
    return { hemi: +L.hemi.intensity.toFixed(2), sun: +L.sun.intensity.toFixed(2), fogNear: r.scene.fog.near, fogFar: r.scene.fog.far };
  }, ids);
  console.log('reactions on every board, then back to file-island ->', JSON.stringify(react), react.hemi === 1.4 && react.sun === 2.6 && react.fogNear === 30 ? '(restored)' : '(NOT restored)');

  // equip through the cosmetics API during planning: the board fades over to it
  const equip = await page.evaluate(async () => {
    const DT = window.DT, r = window.__digi.renderer;
    r.setBoard(DT.Cosmetics.equipped('board'));
    DT.Cosmetics.grant('board', 'dark-ocean');
    DT.Cosmetics.equip('board', 'dark-ocean');
    const ov = document.querySelector('.board-fade');
    let peak = 0;
    const t0 = performance.now();
    while (performance.now() - t0 < 700) { peak = Math.max(peak, Number(ov.style.opacity) || 0); await new Promise((res) => requestAnimationFrame(res)); }
    return { board: r.board.id, peak, after: Number(ov.style.opacity) || 0 };
  });
  console.log('equip dark-ocean in planning ->', JSON.stringify(equip));
  await page.locator('#arena').screenshot({ path: path.join(out, 'equip-dark-ocean.png') });

  // boardOverride (used by scouting) pins a board and hands control back when cleared
  const ovr = await page.evaluate(async () => {
    const r = window.__digi.renderer;
    const wait = (ms) => new Promise((res) => setTimeout(res, ms));
    r.boardOverride = 'toy-town';
    await wait(1200);
    const pinned = r.board.id;
    r.boardOverride = null;
    await wait(1200);
    return { pinned, after: r.board.id, mine: window.DT.Cosmetics.equipped('board') };
  });
  console.log('boardOverride toy-town ->', JSON.stringify(ovr));

  // combat: home board is yours when defending, the opponent's when visiting (mirror)
  const checks = [];
  for (let round = 0; round < 6; round++) {
    const st = await page.evaluate(() => ({ over: window.__digi.game.over, phase: window.__digi.game.phase, offer: !!window.__digi.game.human.offer }));
    if (st.over) break;
    if (st.offer) await page.locator('.choice[data-offer]').first().click();
    for (let i = 0; i < 3; i++) { const c = page.locator(`.card[data-slot="${i}"]:not(.poor)`); if (await c.count()) await c.click(); }
    await page.keyboard.press('Space');
    await page.waitForTimeout(1500);
    const c = await page.evaluate(() => {
      const DT = window.DT, g = window.__digi.game, r = window.__digi.renderer;
      const m = g.matchOf(g.human);
      const opp = m && !m.pve ? (m.team === 1 ? m.a : m.b) : null;
      const oppBoard = opp ? DT.Social.lookOf(opp).board : null;
      const expect = !m || m.pve || !r.mirror ? DT.Cosmetics.equipped('board') : oppBoard;
      return { round: g.roundLabel, phase: g.phase, pve: !!(m && m.pve), mirror: !!r.mirror, oppBoard, expect, shown: r.boardId(), ok: r.boardId() === expect };
    });
    checks.push(c);
    if (c.mirror && !checks.some((x) => x.shot)) { c.shot = true; await page.locator('#arena').screenshot({ path: path.join(out, 'combat-visiting.png') }); }
    if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
    await page.waitForTimeout(3000);
    const back = await page.evaluate(() => ({ phase: window.__digi.game.phase, shown: window.__digi.renderer.boardId(), mine: window.DT.Cosmetics.equipped('board') }));
    c.backInPlanning = back.phase === 'planning' ? back.shown === back.mine : 'n/a';
  }
  for (const c of checks) console.log('  combat', JSON.stringify(c));

  // store previews from the shared offscreen renderer
  const previews = await page.evaluate(async () => {
    const o = {};
    for (const id of window.DT.Boards.ids) o[id] = await window.DT.Boards.previewUrl(id);
    return o;
  });
  let saved = 0;
  for (const [id, url] of Object.entries(previews)) {
    if (!url) continue;
    const ext = url.startsWith('data:image/webp') ? 'webp' : 'png';
    fs.writeFileSync(path.join(out, `preview-${id}.${ext}`), Buffer.from(url.split(',')[1], 'base64'));
    saved++;
  }
  // preview(el) from the catalog fills an element
  const filled = await page.evaluate(async () => {
    const el = document.createElement('div');
    document.body.appendChild(el);
    window.DT.Cosmetics.get('board', 'digital-grid').preview(el);
    await new Promise((r) => setTimeout(r, 300));
    const ok = !!el.querySelector('img.board-preview');
    el.remove();
    return ok;
  });
  console.log('previews saved:', saved, '| preview(el) filled an <img>:', filled);
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  await browser.close();
  server.kill();
})();
