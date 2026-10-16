// Full spectate check: plays a solo game through the real UI and watches other Tamers' fights live.
// - switching to another Tamer mid-combat shows their fight, in sync with the round's clock, with units moving
// - the watched fight's outcome matches the game's resolved result (and every fight of the round replays the same)
// - cycling through every live fight, the bar's fight list, and Return
// - own-board inputs (buying, dragging) are blocked while spectating
// - no growth in scene objects, unit labels or GPU resources after 20 switches
// - after elimination: Spectate keeps the game going for the AI Tamers and follows a fight; Leave ends it
// Run: node tests/spectate.cjs <path-to-playwright> <outDir> [port]
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = Number(process.argv[4] || process.env.PORT || 8192);

let failed = 0;
const check = (name, ok, detail = '') => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${name}${detail ? ` (${detail})` : ''}`); if (!ok) failed++; };

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => { server.stdout.once('data', r); setTimeout(r, 5000); });
  server.stderr.on('data', (d) => console.log('server:', String(d).trim()));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader', '--enable-precise-memory-info', '--js-flags=--expose-gc'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error') errors.push(m.text()); if (m.type() === 'warning' && m.text().startsWith('spectate')) errors.push(m.text()); });
  page.on('requestfailed', (r) => errors.push(`request failed: ${r.url()}`));
  const phase = () => page.evaluate(() => window.__digi.game.phase);
  const viewing = () => page.evaluate(() => window.__digi.ui.viewing);
  // a board switch has finished (fade done) and a few frames have drawn the new view
  const settle = async () => { await page.waitForFunction(() => !window.DT.Scout.busy(), null, { timeout: 5000 }).catch(() => {}); await frames(3); };
  const frames = (n = 2) => page.evaluate((n) => new Promise((res) => { let k = 0; const f = () => (++k >= n ? res() : requestAnimationFrame(f)); requestAnimationFrame(f); }), n);

  // what is on screen: the fight shown, its clock, its unit views
  const shown = () => page.evaluate(() => {
    const { game: g, renderer: r, ui } = window.__digi;
    const cb = window.__digi.shown;
    const own = window.__digi.own;
    const views = [...r.views.keys()].filter((k) => k.startsWith(`c${r.cbSerial}_`));
    let match = null;
    if (cb && window.DT.Scout.replays()) for (const [m, rp] of window.DT.Scout.replays()) if (rp === cb) match = m;
    return {
      viewing: ui.viewing, phase: g.phase, isOwn: !!cb && cb === own, has: !!cb, t: cb ? cb.t : null, clock: window.__digi.clock,
      alive: cb ? cb.units.filter((u) => u.alive).length : 0, views: views.length, done: cb ? cb.done : null, winner: cb ? cb.winner : null,
      moved: cb ? cb.units.some((u) => u.c0 != null && (u.c !== u.c0 || u.r !== u.r0)) : false,
      pos: views.map((k) => { const p = r.views.get(k).group.position; return `${k}:${p.x.toFixed(2)},${p.z.toFixed(2)}`; }),
      inMatch: match ? [match.a.id, match.b ? match.b.id : null] : null, ghost: match ? !!match.ghost : null,
      recorded: match && match.combat && match.combat.done ? { winner: match.combat.winner, t: match.combat.t } : null,
      evIdx: r.evIdx, mirror: r.mirror,
      bar: (document.querySelector('#scoutBanner:not(.hidden)') || {}).textContent || '',
      fights: document.querySelectorAll('#scoutBanner .spf [data-pid]').length,
    };
  });
  // starting cells, to tell that units have walked
  const markStart = () => page.evaluate(() => { const cb = window.__digi.shown; if (cb) for (const u of cb.units) { if (u.fromC0 == null) { u.fromC0 = u.c; u.fromR0 = u.r; } } });

  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);
    await page.click('#startBtn');
    await page.waitForTimeout(500);
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(300);

    // buy what we can; combat start auto-fills the board from the bench
    async function shopRound() {
      if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
      for (let i = 0; i < 5; i++) {
        const card = page.locator(`.card[data-slot="${i}"]:not(.poor)`);
        if (await card.count()) await card.click().catch(() => {});
      }
      if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
    }
    async function skipRound() {
      await page.keyboard.press('Space');
      await page.waitForTimeout(300);
      if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
      await page.waitForFunction(() => window.__digi.game.phase === 'planning' || window.__digi.game.over, null, { timeout: 15000 });
      await page.waitForTimeout(300);
    }
    // play to the first PvP round with at least 3 fights
    for (let k = 0; k < 8; k++) {
      const g = await page.evaluate(() => ({ t: window.__digi.game.roundType, label: window.__digi.game.roundLabel }));
      if (g.t === 'pvp') break;
      await shopRound();
      await skipRound();
    }
    await shopRound();
    check('reached a PvP round', (await page.evaluate(() => window.__digi.game.roundType)) === 'pvp', await page.evaluate(() => window.__digi.game.roundLabel));

    // ---------- inputs blocked while spectating (planning) ----------
    const other = await page.evaluate(() => { const g = window.__digi.game; return g.players.find((q) => q !== g.human && q.alive).id; });
    await page.click(`#players .pl[data-pid="${other}"]`);
    await settle();
    const before = await page.evaluate(() => { const p = window.__digi.game.human; return JSON.stringify([p.gold, p.bench.map((u) => u && u.uid), Object.keys(p.board)]); });
    await page.evaluate(() => { const p = window.__digi.game.human; p.gold += 10; }); // so a buy would succeed
    const gold0 = await page.evaluate(() => window.__digi.game.human.gold);
    const card = page.locator('.card[data-slot]:not(.poor)').first();
    if (await card.count()) await card.click();
    await page.waitForTimeout(250);
    const after = await page.evaluate(() => { const p = window.__digi.game.human; return [p.gold, JSON.stringify(p.bench.map((u) => u && u.uid))]; });
    check('buying is blocked while spectating', after[0] === gold0 && JSON.parse(before)[1].join() === JSON.parse(after[1]).join(), `gold ${gold0} -> ${after[0]}`);
    const box = await page.locator('#board').boundingBox();
    const pt = async (expr) => { const p = await page.evaluate(expr); return { x: box.x + p.x, y: box.y + p.y }; };
    const a = await pt('window.__digi.renderer.screenOfBench(0)');
    const b = await pt('window.__digi.renderer.screenOfCell(3, 5)');
    const boardBefore = await page.evaluate(() => JSON.stringify(Object.keys(window.__digi.game.human.board).sort()));
    await page.mouse.move(a.x, a.y); await page.mouse.down(); await page.mouse.move(b.x, b.y, { steps: 8 }); await page.mouse.up();
    await page.waitForTimeout(200);
    check('dragging is blocked while spectating', boardBefore === (await page.evaluate(() => JSON.stringify(Object.keys(window.__digi.game.human.board).sort()))));
    const prompt = await page.evaluate(() => (document.querySelector('#scoutBanner .spp') || {}).textContent || '');
    check('planning prompt offers the way back', /planning phase|pick is waiting/i.test(prompt), prompt);
    await page.keyboard.press('Space');
    await settle();
    check('Space returns without starting combat', (await viewing()) == null && (await phase()) === 'planning');

    // ---------- mid-combat switch ----------
    await page.click('#readyBtn');
    await page.waitForTimeout(1500);
    const pick = await page.evaluate(() => {
      const g = window.__digi.game;
      const mine = g.matches.find((m) => m.a === g.human || m.b === g.human);
      // a Tamer in another fight, preferring a long one so it is still going
      const others = g.matches.filter((m) => m !== mine).sort((x, y) => y.combat.t - x.combat.t);
      const m = others[0];
      return m ? { id: m.a.id, name: m.a.name, len: m.combat.t } : null;
    });
    check('there is another fight to watch', !!pick, JSON.stringify(pick));
    await page.click(`#players .pl[data-pid="${pick.id}"]`);
    await settle();
    await markStart();
    let s = await shown();
    await page.mouse.move(box.x + 10, box.y + 10);
    await page.screenshot({ path: path.join(out, 'spectate-1-midfight.png') });
    check('switching mid-combat shows their fight', s.viewing === pick.id && s.has && !s.isOwn && s.inMatch && s.inMatch.includes(pick.id), JSON.stringify({ v: s.viewing, own: s.isOwn, m: s.inMatch }));
    check('their fight is at the round clock', s.done || Math.abs(s.t - s.clock) <= 2 / 30 + 1e-6, `fight ${s.t && s.t.toFixed(3)} s, clock ${s.clock.toFixed(3)} s`);
    check('its units are on screen', s.views >= s.alive && s.alive > 0, `${s.views} views, ${s.alive} alive`);
    check('the bar names both sides', /Watching .+ vs .+/.test(s.bar), s.bar.slice(0, 80));
    check('the bar lists every live fight', s.fights === (await page.evaluate(() => window.__digi.game.matches.length)), `${s.fights} rows`);
    const t0 = s.t;
    const ev0 = s.evIdx;
    const pos0 = s.pos;
    await page.waitForTimeout(1200);
    s = await shown();
    await page.screenshot({ path: path.join(out, 'spectate-2-midfight-later.png') });
    check('the fight plays live (time and events advance)', s.done || (s.t > t0 + 0.5 && s.evIdx > ev0), `t ${t0.toFixed(2)} -> ${s.t.toFixed(2)}, events ${ev0} -> ${s.evIdx}`);
    const shifted = s.pos.filter((p) => !pos0.includes(p)).length;
    check('units move (cells changed or models shifted)', s.moved || shifted > 0 || s.done, `cells ${s.moved}, ${shifted} views shifted`);

    // the round's fights replayed from the copied inputs match the game's resolved results
    const ver = await page.evaluate(() => window.DT.Scout.verify());
    check('every fight this round replays to the same result', ver.length > 0 && ver.every((v) => v.same), JSON.stringify(ver.map((v) => [v.winner, v.recorded, +v.t.toFixed(2)])));

    // faster, so the watched fight finishes in this round
    await page.click('#speedBtn'); await page.click('#speedBtn'); // 4x
    // read the result inside the page the moment it ends (the next round may start right after)
    const fin = await page.waitForFunction(() => {
      const cb = window.__digi.shown;
      if (window.__digi.game.phase === 'planning') return { cut: true };
      if (!cb || !cb.done) return null;
      let match = null;
      for (const [m, rp] of window.DT.Scout.replays()) if (rp === cb) match = m;
      return { winner: cb.winner, t: cb.t, recorded: match && match.combat && match.combat.done ? { winner: match.combat.winner, t: match.combat.t } : null };
    }, null, { timeout: 120000, polling: 100 }).then((h) => h.jsonValue()).catch(() => ({ timeout: true }));
    check('watched fight outcome matches the recorded result', !!fin.recorded && fin.recorded.winner === fin.winner && Math.abs(fin.recorded.t - fin.t) < 1e-6,
      JSON.stringify(fin));
    const st1 = await page.evaluate(() => window.DT.Scout.stats);
    check('no replay differed from the game', st1.mismatches === 0 && st1.checked > 0, JSON.stringify(st1));

    // ---------- next PvP round: cycle through every fight, memory, Return ----------
    await page.waitForFunction(() => window.__digi.game.phase === 'planning', null, { timeout: 90000 });
    await page.click('#speedBtn'); // back to 1x
    check('still watching them in planning (no snap back)', (await viewing()) === pick.id || !(await page.evaluate((id) => window.__digi.game.players[id].alive, pick.id)));
    await page.keyboard.press('Escape');
    await page.waitForTimeout(400);
    for (let k = 0; k < 6; k++) {
      if ((await page.evaluate(() => window.__digi.game.roundType)) === 'pvp') break;
      await shopRound();
      await skipRound();
    }
    await shopRound();
    await page.click('#readyBtn');
    await page.waitForTimeout(800);
    const alive = await page.evaluate(() => window.__digi.game.alivePlayers().length);
    const seen = [];
    let allOk = true;
    for (let k = 0; k < alive; k++) {
      await page.keyboard.press('e');
      await settle();
      const x = await shown();
      seen.push(x.viewing);
      if (x.viewing == null) { if (!x.isOwn) allOk = false; }
      else if (!(x.has && x.inMatch && x.inMatch.includes(x.viewing) && x.views >= x.alive)) allOk = false;
    }
    const uniq = new Set(seen);
    check('E cycles through every Tamer and back to you', uniq.size === alive && seen[seen.length - 1] == null, JSON.stringify(seen));
    check('each cycled Tamer shows their own fight', allOk);
    // fight list click
    const rowPid = await page.evaluate(() => { const g = window.__digi.game; const v = window.__digi.ui.viewing; const m = g.matches.find((m) => m.a !== g.human && m.b !== g.human && m.a.id !== v); return m ? m.a.id : null; });
    await page.keyboard.press('e');
    await settle();
    if (rowPid != null && (await page.locator(`#scoutBanner .spf [data-pid="${rowPid}"]`).count())) {
      await page.locator(`#scoutBanner .spf [data-pid="${rowPid}"]`).click();
      await settle();
      const v = await viewing();
      check('clicking a fight in the bar watches it', v === rowPid, `viewing ${v}, row ${rowPid}`);
    } else console.log('note: no other fight row to click');
    await page.screenshot({ path: path.join(out, 'spectate-3-cycle.png') });

    const mem = () => page.evaluate(async () => {
      await new Promise((res) => requestAnimationFrame(() => requestAnimationFrame(res)));
      if (window.gc) window.gc();
      const r = window.__digi.renderer;
      let objs = 0;
      r.scene.traverse(() => objs++);
      const stale = [...r.views.keys()].filter((k) => k.startsWith('c') && !k.startsWith(`c${r.cbSerial}_`)).length;
      return { geo: r.r.info.memory.geometries, tex: r.r.info.memory.textures, objs, labels: r.labels.children.length, views: r.views.size, stale, fx: r.fx.length,
        heap: performance.memory ? performance.memory.usedJSHeapSize : 0, replays: window.DT.Scout.replays() ? window.DT.Scout.replays().size : 0, matches: window.__digi.game.matches.length };
    });
    // 20 switches during combat: old fights' units and effects are released
    const switch20 = async () => { for (let k = 0; k < 20; k++) { await page.keyboard.press(k % 3 === 2 ? 'q' : 'e'); await settle(); } };
    const c0 = await mem();
    await switch20();
    const c1 = await mem();
    console.log('combat memory before', JSON.stringify(c0), 'after', JSON.stringify(c1));
    check('combat: no stale fight views after 20 switches', c1.stale === 0, `${c1.stale} stale`);
    check('combat: GPU textures / geometries stay flat', c1.tex <= c0.tex + 10 && c1.geo <= c0.geo + 30, `tex ${c0.tex} -> ${c1.tex}, geo ${c0.geo} -> ${c1.geo}`);
    check('one replay per fight at most', c1.replays <= c1.matches, `${c1.replays} replays, ${c1.matches} fights`);
    const phaseBefore = await phase();
    await page.keyboard.press('Escape');
    await settle();
    s = await shown();
    check('Return shows your own fight again', s.viewing == null && (s.isOwn || !s.has), JSON.stringify({ v: s.viewing, own: s.isOwn, phase: s.phase, phaseBefore }));

    // 20 switches in planning, same state before and after: nothing grows
    await page.waitForFunction(() => window.__digi.game.phase === 'planning', null, { timeout: 60000 });
    if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
    const n = await page.evaluate(() => window.__digi.game.alivePlayers().length);
    for (let pass = 0; pass < 2; pass++) for (let k = 0; k < n; k++) { await page.keyboard.press('e'); await settle(); await page.waitForTimeout(500); } // every board seen (models cached)
    await page.keyboard.press('Escape'); // Esc, not Space: Space on your own board would start the fight
    await settle();
    await page.waitForTimeout(1500);
    const m0 = await mem();
    const round20 = async () => { await switch20(); await page.keyboard.press('Escape'); await settle(); await page.waitForTimeout(1500); return mem(); };
    const m1 = await round20();
    const m2 = await round20();
    console.log('planning memory', JSON.stringify(m0), JSON.stringify(m1), JSON.stringify(m2));
    check('no growth in scene objects after 20 switches', m1.objs <= m0.objs && m2.objs <= m0.objs, `${m0.objs} -> ${m1.objs} -> ${m2.objs}`);
    check('no leaked unit labels or views', m2.labels <= m0.labels && m2.views <= m0.views, `labels ${m0.labels} -> ${m2.labels}, views ${m0.views} -> ${m2.views}`);
    // the first batch may still finish loading a model or two; a second batch must add nothing
    check('no growth in GPU geometries / textures', m2.geo <= m1.geo && m2.tex <= m1.tex && m1.tex <= m0.tex + 8, `geo ${m0.geo} -> ${m1.geo} -> ${m2.geo}, tex ${m0.tex} -> ${m1.tex} -> ${m2.tex}`);
    check('JS heap stays flat', !m0.heap || m2.heap < m0.heap + 15e6, `${(m0.heap / 1e6).toFixed(1)} MB -> ${(m2.heap / 1e6).toFixed(1)} MB`);

    // ---------- elimination: Spectate, then Leave ----------
    await page.evaluate(() => {
      const g = window.__digi.game;
      const p = g.human;
      p.hp = 1;
      p.board = {};
      p.bench = p.bench.map(() => null);
    });
    if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
    console.log('before elimination round', JSON.stringify(await page.evaluate(() => { const g = window.__digi.game; return { phase: g.phase, over: g.over, type: g.roundType, alive: g.human.alive, viewing: window.__digi.ui.viewing, ready: document.getElementById('readyBtn').disabled }; })));
    await page.click('#readyBtn');
    await page.waitForFunction(() => !!document.querySelector('#watchBtn'), null, { timeout: 20000 });
    await page.screenshot({ path: path.join(out, 'spectate-4-eliminated.png') });
    const labels = await page.evaluate(() => [document.querySelector('#watchBtn').textContent, document.querySelector('#leaveBtn').textContent]);
    check('elimination screen offers Spectate and Leave', labels[0] === 'Spectate' && labels[1] === 'Leave', labels.join(' / '));
    await page.evaluate(() => document.querySelector('#watchBtn').click());
    await page.waitForTimeout(300);
    await settle();
    const out1 = await page.evaluate(() => ({ over: window.__digi.game.over, playOut: !!window.__digi.game.playOut, viewing: window.__digi.ui.viewing, alive: window.__digi.game.human.alive }));
    check('Spectate keeps the game going for the AI Tamers', !out1.over && out1.playOut && !out1.alive, JSON.stringify(out1));
    check('you follow another Tamer', out1.viewing != null, JSON.stringify(out1));
    await page.waitForFunction(() => window.__digi.game.phase === 'combat', null, { timeout: 15000 });
    await page.waitForTimeout(1500);
    await markStart();
    s = await shown();
    await page.mouse.move(box.x + 10, box.y + 10);
    await page.screenshot({ path: path.join(out, 'spectate-5-after-elim.png') });
    check('after elimination you watch a live fight', s.has && !s.isOwn && s.viewing != null && s.views >= s.alive && s.alive > 0, JSON.stringify({ v: s.viewing, has: s.has, views: s.views, alive: s.alive }));
    const tA = s.t;
    await page.waitForTimeout(800);
    s = await shown();
    check('it plays live', s.done || s.t > tA + 0.3, `${tA} -> ${s.t}`);
    check('the bar has a Leave button', !!(await page.$('#scoutLeave')));
    // the round ends on its own once the longest fight has played out (4x so it is quick)
    await page.click('#speedBtn'); await page.click('#speedBtn');
    await page.waitForFunction(() => window.__digi.game.phase !== 'combat', null, { timeout: 150000 }).catch(() => {});
    check('the round ends without you', (await phase()) !== 'combat');
    await page.evaluate(() => document.querySelector('#scoutLeave') && document.querySelector('#scoutLeave').click());
    await page.waitForTimeout(800);
    const end = await page.evaluate(() => ({ over: window.__digi.game.over, overlay: !document.getElementById('overlay').classList.contains('hidden') }));
    check('Leave ends the game for you', end.over && end.overlay, JSON.stringify(end));
  } catch (e) {
    check('test ran', false, String(e && e.stack || e));
  }
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  if (errors.length) failed++;
  console.log(failed ? `${failed} failed` : 'all passed');
  await browser.close();
  server.kill();
  process.exitCode = failed ? 1 : 0;
})();
