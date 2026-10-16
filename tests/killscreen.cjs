// Kill screen check: plays a solo game to the first PvP round, drops every AI Tamer to 1 HP (test-only state poke),
// fights, and checks that each elimination credits the right killer and shows a kill screen. Also screenshots every
// variant through DT.KillScreens.demo, the PvE sting, a store preview, and Esc skipping.
// Run: node tests/killscreen.cjs <path-to-playwright> <outDir>
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8151;
fs.mkdirSync(out, { recursive: true });

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  // wait until the server answers
  const http = require('http');
  for (let i = 0; i < 40; i++) {
    const up = await new Promise((r) => http.get(`http://localhost:${PORT}/`, (res) => { res.resume(); r(true); }).on('error', () => r(false)));
    if (up) break;
    await new Promise((r) => setTimeout(r, 250));
  }
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error') errors.push(m.text()); });
  const fail = [];
  const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fail.push(msg); };

  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2000);
  await page.click('#startBtn');
  await page.waitForTimeout(500);
  await page.click('#timerBtn');
  const g = (fn, arg) => page.evaluate(fn, arg);

  // play rounds with an empty-handed human until a PvP planning phase
  for (let i = 0; i < 12; i++) {
    const s = await g(() => { const G = window.__digi.game; return { phase: G.phase, type: G.roundType, offer: !!G.human.offer, over: G.over }; });
    if (s.over) break;
    if (s.phase === 'planning' && s.type === 'pvp') break;
    if (s.type === 'carousel') {
      const c = page.locator('.choice[data-carousel]:not(.taken)');
      if (await c.count()) { await c.first().click(); await page.click('#carouselGo'); }
      await page.waitForTimeout(2500);
      continue;
    }
    if (s.offer && (await page.locator('.choice[data-offer]').count())) await page.locator('.choice[data-offer]').first().click();
    await page.keyboard.press('Space');
    await page.waitForTimeout(300);
    if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
    await page.waitForTimeout(2700);
  }
  const round = await g(() => window.__digi.game.roundLabel);
  check(await g(() => window.__digi.game.roundType === 'pvp'), `reached a PvP round (${round})`);

  // every AI at 1 HP, the human safe: each AI that loses or draws is eliminated this round
  await g(() => { for (const p of window.__digi.game.players) p.hp = p.isHuman ? 99 : 1; });
  if (await page.locator('.choice[data-offer]').count()) await page.locator('.choice[data-offer]').first().click();
  await page.keyboard.press('Space');
  await page.waitForTimeout(300);
  if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
  await page.waitForTimeout(450);
  const res = await g(() => {
    const G = window.__digi.game;
    const elim = DT.KillScreens.eliminations(G).map((e) => ({ victim: e.victim.name, killer: e.killer && e.killer.name, place: e.victim.placement }));
    // expected: for each match, a dead side's killer is the other side (the ghost's owner for a ghost match)
    const expect = [];
    for (const r of G.results) {
      const m = r.match;
      if (!m.a.alive) expect.push({ victim: m.a.name, killer: m.pve ? null : m.b.name });
      if (m.b && !m.ghost && !m.b.alive) expect.push({ victim: m.b.name, killer: m.a.name });
    }
    const a = DT.KillScreens.active;
    const killerP = a && a.killer ? G.players.find((p) => (p === G.human ? 'You' : p.name) === a.killer) : null;
    const teamOk = !a || !a.killer || (!!killerP && a.team === killerP.boardUnits().length);
    return { elim, expect, teamOk, team: a && a.team, active: DT.KillScreens.active, queued: DT.KillScreens.queued, pairs: G.results.map((r) => `${r.match.a.name} vs ${r.match.b ? r.match.b.name : 'PvE'}${r.match.ghost ? ' (ghost)' : ''}: ${r.aWon ? 'A' : r.bWon ? 'B' : 'draw'}`) };
  });
  console.log('matches:', res.pairs.join(' | '));
  console.log('eliminations:', JSON.stringify(res.elim));
  check(res.elim.length > 0, `${res.elim.length} eliminations this round`);
  const same = res.expect.length === res.elim.length && res.expect.every((x) => res.elim.some((e) => e.victim === x.victim && e.killer === x.killer));
  check(same, 'every killer is the opponent of the eliminated Tamer');
  check(!!res.active, `kill screen playing: ${JSON.stringify(res.active)}, ${res.queued} queued`);
  check(res.teamOk, `the kill screen lineup is the killer's board (${res.team} Digimon)`);
  await page.waitForTimeout(900);
  await page.screenshot({ path: path.join(out, 'ks-1-round.png') });
  // timers keep running under it: the solo game still moves to the next round on its own
  await page.waitForTimeout(2600);
  const after = await g(() => ({ phase: window.__digi.game.phase, label: window.__digi.game.roundLabel, active: DT.KillScreens.active }));
  check(after.phase === 'planning' && after.label !== round, `game moved on to ${after.label} while kill screens played`);
  // Esc skips the current one
  if (after.active) {
    const before = JSON.stringify(after.active);
    await page.keyboard.press('Escape');
    await page.waitForTimeout(100);
    const now = await g(() => DT.KillScreens.active);
    check(JSON.stringify(now) !== before, 'Esc skipped the current kill screen');
  }
  // drain the queue
  for (let i = 0; i < 12 && (await g(() => !!DT.KillScreens.active)); i++) await page.keyboard.press('Escape');

  // every variant, big (the human is the killer)
  const ids = await g(() => DT.Cosmetics.list('killscreen').map((x) => x.id));
  check(ids.length >= 8 && (await g(() => DT.Cosmetics.list('killscreen').filter((x) => x.free).length)) >= 1, `${ids.length} kill screen variants registered, at least one free`);
  for (const id of ids) {
    await g((v) => { const G = window.__digi.game; DT.KillScreens.demo({ killer: G.human, victim: G.players.find((p) => !p.alive) || G.players[1], variant: v }); }, id);
    await page.waitForTimeout(1500);
    await page.screenshot({ path: path.join(out, `ks-v-${id}.png`) });
    await page.keyboard.press('Escape');
    await page.waitForTimeout(120);
  }
  // small corner version (two AI Tamers) and the PvE sting
  await g(() => { const G = window.__digi.game; const ai = G.players.filter((p) => !p.isHuman); DT.KillScreens.demo({ killer: ai[0], victim: ai[1], variant: 'ks_gate' }); });
  await page.waitForTimeout(700);
  check(await g(() => DT.KillScreens.active && !DT.KillScreens.active.big), 'uninvolved viewer gets the small version');
  await page.screenshot({ path: path.join(out, 'ks-2-corner.png') });
  // a click on it skips
  await page.click('#ksLayer', { timeout: 3000 }).catch(() => {});
  await page.waitForTimeout(100);
  check(await g(() => !DT.KillScreens.active), 'click skipped it');
  await g(() => { const G = window.__digi.game; DT.KillScreens.demo({ killer: null, victim: G.human }); });
  await page.waitForTimeout(900);
  await page.screenshot({ path: path.join(out, 'ks-3-sting.png') });
  await page.keyboard.press('Escape');

  // store preview
  await g(() => {
    const el = document.createElement('div');
    el.id = 'ksPrevTest';
    el.style.cssText = 'position:fixed;left:20px;top:80px;width:240px;height:135px;z-index:50;border-radius:8px;overflow:hidden';
    document.body.appendChild(el);
    DT.Cosmetics.get('killscreen', 'ks_firewall').preview(el);
  });
  await page.waitForTimeout(900);
  await page.locator('#ksPrevTest').screenshot({ path: path.join(out, 'ks-4-preview.png') });

  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  console.log(fail.length ? `FAILED: ${fail.length}` : 'all kill screen checks passed');
  await browser.close();
  server.kill();
  process.exit(fail.length || errors.length ? 1 : 0);
})();
