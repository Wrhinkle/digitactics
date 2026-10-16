// Multiplayer check against the live Firebase backend: two browsers (two anonymous users) create and join a lobby,
// play several rounds, and must hold identical game state. Also checks desync recovery and host handover.
// Run: node tests/multiplayer.cjs <path-to-playwright> <outDir> [url]
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8138;
const URL = process.argv[4] || `http://localhost:${PORT}/`;

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const digest = (page) => page.evaluate(() => { const g = window.__digi.game; return g ? `${g.seq}|${window.DT.Lock.digest(g)}` : null; });
const seq = (page) => page.evaluate(() => (window.__digi.game ? window.__digi.game.seq : -1));

(async () => {
  const server = URL.includes('localhost') ? spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true }) : null;
  await sleep(500);
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const errors = [];
  const mk = async (name) => {
    const ctx = await browser.newContext({ viewport: { width: 1400, height: 880 } });
    const page = await ctx.newPage();
    page.on('pageerror', (e) => errors.push(`${name}: ${e}`));
    page.on('console', (m) => { if (m.type() === 'error') errors.push(`${name}: ${m.text()}`); });
    return page;
  };
  const a = await mk('A');
  const b = await mk('B');
  try {
    await a.goto(URL);
    await a.click('#mpBtn');
    await a.waitForSelector('#mpName', { timeout: 20000 });
    await a.fill('#mpName', 'Alice');
    await a.fill('#mpLobbyName', `test lobby ${Date.now() % 10000}`);
    await a.click('#mpCreate');
    await a.waitForSelector('#mpLink', { timeout: 15000 });
    const link = await a.inputValue('#mpLink');
    console.log('invite link:', link.replace(/^http:\/\/localhost:\d+/, ''));

    await b.goto(URL);
    await b.evaluate(() => localStorage.setItem('dt.name', 'Bob'));
    await b.goto(link);
    await b.waitForSelector('.mpseat.me', { timeout: 20000 });
    await a.waitForFunction(() => document.querySelectorAll('.mpseat.taken').length === 2, null, { timeout: 15000 });
    await a.screenshot({ path: path.join(out, 'mp-0-lobby.png') });
    await b.screenshot({ path: path.join(out, 'mp-0-lobby-b.png') });
    await a.click('#mpStart');
    await a.waitForFunction(() => window.__digi.game && window.__digi.session, null, { timeout: 20000 });
    await b.waitForFunction(() => window.__digi.game && window.__digi.session, null, { timeout: 20000 });
    console.log('both in game; slots', await a.evaluate(() => window.__digi.session.slot), await b.evaluate(() => window.__digi.session.slot));

    // carousel: whoever's turn it is picks
    for (let i = 0; i < 40; i++) {
      for (const p of [a, b]) {
        const el = p.locator('.choice[data-carousel]:not(.taken)').first();
        const mine = await p.evaluate(() => { const g = window.__digi.game; return !!(g.carousel && g.carouselTurn() === g.human); });
        if (mine && await el.count()) await el.click().catch(() => {});
      }
      const r = await a.evaluate(() => window.__digi.game.roundLabel);
      if (r !== '1-1') break;
      await sleep(500);
    }
    await a.screenshot({ path: path.join(out, 'mp-1-after-carousel.png') });

    // a few rounds: buy what we can, ready up, check both clients agree
    let checks = 0;
    for (let round = 0; round < 7; round++) {
      for (const p of [a, b]) {
        await p.waitForFunction(() => window.__digi.game.phase === 'planning', null, { timeout: 60000 });
        await p.evaluate(() => {
          const d = window.__digi;
          const g = d.game;
          if (g.human.offer) document.querySelector('[data-offer]') && document.querySelector('[data-offer]').click();
          document.querySelectorAll('#shop .card[data-slot]:not(.poor)').forEach((c, i) => { if (i < 2) c.click(); });
        });
      }
      // compare only when both clients have applied the same number of log entries (digest starts with the count)
      let da = '', db = '', sa = -1, sb = -2;
      for (let k = 0; k < 40 && sa !== sb; k++) {
        await sleep(250);
        [da, db] = [await digest(a), await digest(b)];
        [sa, sb] = [Number(da.split('|')[0]), Number(db.split('|')[0])];
        if (sa === sb && Number((await digest(a)).split('|')[0]) !== sa) sa = -1; // moved on mid-read
      }
      if (sa === sb) { if (da !== db) throw new Error(`desync at ${da.slice(0, 40)}`); checks++; }
      const label = await a.evaluate(() => window.__digi.game.roundLabel);
      console.log(`round ${label}: seq ${sa}/${sb} ${sa === sb ? 'states match' : '(in flight)'}`);
      // break Bob's copy on purpose: the host's state hash on the next round should make Bob reload from the log
      if (round === 3) { await b.evaluate(() => { const d = window.__digi; d.game.players[d.session.slot].gold += 7; }); console.log('corrupted Bob gold'); }
      if (round === 1) { await a.screenshot({ path: path.join(out, 'mp-2-planning-a.png') }); await b.screenshot({ path: path.join(out, 'mp-2-planning-b.png') }); }
      // soundboard: Alice plays her first clip, Bob should hear it through Firebase and see a label on her row
      if (round === 1 && await b.evaluate(() => !!window.DT.Soundboard)) {
        const heard = await b.evaluate(() => window.DT.Soundboard.stats.played);
        await a.evaluate(() => { window.DT.Soundboard.open(); window.DT.Soundboard.play(0); window.DT.Soundboard.close(); });
        const ok = await b.waitForFunction((n) => window.DT.Soundboard.stats.played > n, heard, { timeout: 15000 }).then(() => true, () => false);
        console.log('soundboard clip reached Bob:', ok, '| labels on Bob:', await b.evaluate(() => window.DT.Soundboard.stats.labels));
      }
      await a.click('#readyBtn').catch(() => {});
      await b.click('#readyBtn').catch(() => {});
      await a.waitForFunction(() => window.__digi.game.phase === 'combat', null, { timeout: 40000 });
      if (round === 2) {
        await sleep(2500);
        await a.screenshot({ path: path.join(out, 'mp-3-combat-a.png') });
        await b.screenshot({ path: path.join(out, 'mp-3-combat-b.png') });
        console.log('mirror flags', await a.evaluate(() => window.__digi.renderer.mirror), await b.evaluate(() => window.__digi.renderer.mirror));
      }
      await a.click('#skipBtn').catch(() => {});
      await b.click('#skipBtn').catch(() => {});
    }
    const resyncs = await b.evaluate(() => window.DT.Net.resyncs);
    console.log('Bob resynced', resyncs, 'time(s)');
    if (!resyncs) throw new Error('desync was not detected');
    // host leaves: Bob should take over and the game should keep advancing
    const before = await b.evaluate(() => window.__digi.game.roundLabel);
    await a.close();
    console.log('host closed at', before);
    await b.waitForFunction((r) => window.__digi.game.roundLabel !== r, before, { timeout: 90000 });
    console.log('after handover, round', await b.evaluate(() => window.__digi.game.roundLabel), 'players', await b.evaluate(() => window.__digi.game.players.filter((p) => p.isHuman).map((p) => p.name).join(',')));
    await b.screenshot({ path: path.join(out, 'mp-4-after-handover.png') });
    console.log(`state checks matched: ${checks}`);
  } finally {
    console.log('errors:', errors.length ? errors.slice(0, 10).join('\n') : 'none');
    await browser.close();
    if (server) server.kill();
  }
})().catch((e) => { console.error('FAILED', e); process.exit(1); });
