// Actual solo and two-client Steam-transport flows through the in-game leave menu.
// Steam's external service is supplied by MockSteamHub; game/UI/host handover are real.
// node tests/game-menu.cjs <playwright-path> <output-directory> [port]
const { chromium } = require(process.argv[2] || 'playwright');
const { MockSteamHub } = require('./steam-mock-hub.cjs');
const { spawn } = require('child_process');
const fs = require('fs'), path = require('path'), assert = require('assert/strict');
const out = path.resolve(process.argv[3] || __dirname), port = Number(process.argv[4] || 8194);
const check = (ok, message) => { assert.ok(ok, message); console.log(`PASS ${message}`); };
(async () => {
  fs.mkdirSync(out, { recursive: true });
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT: String(port) }, windowsHide: true, stdio: 'ignore' });
  let browser;
  try {
    await new Promise(r => setTimeout(r, 600));
    browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
    const errors = [], watch = p => p.on('pageerror', e => errors.push(String(e)));
    const page = await browser.newPage({ viewport: { width: 960, height: 640 } }); watch(page);
    await page.goto(`http://localhost:${port}`);
    await page.waitForSelector('#boot', { state: 'detached' });
    await page.click('#startBtn');
    await page.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.click('#legendBtn');
    await page.keyboard.press('Escape');
    check(!await page.locator('#picker').isVisible() && !await page.locator('#gameMenu').isVisible(), 'Escape closes an existing picker before opening the game menu');
    await page.keyboard.press('Escape');
    check(await page.locator('#gameMenu').isVisible(), 'Escape opens the game menu');
    const gold = await page.evaluate(() => __digi.game.human.gold);
    await page.keyboard.press('d'); await page.keyboard.press('f');
    check(await page.evaluate(() => __digi.game.human.gold) === gold, 'menu blocks reroll and XP hotkeys');
    await page.keyboard.press('Tab');
    check(await page.evaluate(() => document.getElementById('gameMenu').contains(document.activeElement)), 'keyboard focus stays in the menu');
    await page.keyboard.press('Escape');
    check(!await page.locator('#gameMenu').isVisible() && await page.evaluate(() => !!__digi.game), 'Escape returns to the same match');
    await page.click('#gameMenuBtn');
    await page.click('[data-game-leave]');
    check(await page.locator('#gameMenu').evaluate(el => { const r = el.getBoundingClientRect(); return r.left >= 0 && r.right <= innerWidth && r.top >= 0 && r.bottom <= innerHeight; }), 'confirmation fits the compact desktop window');
    check((await page.locator('#gameMenuTitle').textContent()).includes('Leave this match'), 'leaving requires an explicit confirmation');
    await page.click('[data-game-return]');
    check(await page.evaluate(() => !!__digi.game), 'Keep playing cancels without ending the match');
    await page.click('#readyBtn');
    await page.waitForFunction(() => __digi.game.phase === 'combat');
    await page.click('#gameMenuBtn');
    await page.click('[data-game-leave]');
    await page.screenshot({ path: path.join(out, 'leave-match-960.png') });
    await page.click('[data-game-confirm]');
    await page.waitForFunction(() => DT.Client.page() === 'home' && !DT.UI.game());
    await page.waitForTimeout(3000);
    check(await page.evaluate(() => !__digi.game && !__digi.session && !DT.KillScreens.active && !DT.KillScreens.queued), 'leaving during combat clears the match, callbacks and killcam queue');
    check(await page.evaluate(() => DT.Client.history().length === 0), 'abandoning a solo match does not award a finished result');
    await page.click('#startBtn');
    await page.waitForFunction(() => !!DT.UI.game());
    await page.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    await page.click('#gameMenuBtn'); await page.click('[data-game-leave]'); await page.click('[data-game-confirm]');
    await page.waitForFunction(() => !DT.UI.game());
    check(true, 'a fresh solo match starts and can be left again');
    await page.close();

    const hub = new MockSteamHub(), alice = '76561190000000001', bob = '76561190000000002';
    hub.befriend(alice, bob);
    const clients = [];
    for (const [steamId, name] of [[alice, 'Alice'], [bob, 'Bob']]) {
      const ctx = await browser.newContext({ viewport: { width: 1280, height: 720 } });
      await hub.install(ctx, { steamId, name });
      const p = await ctx.newPage(); watch(p); await p.goto(`http://localhost:${port}`);
      await p.waitForSelector('#boot', { state: 'detached' }); clients.push(p);
    }
    const [a, b] = clients;
    await a.click('#mpBtn'); await a.waitForSelector('#mpCreate');
    await a.fill('#mpLobbyName', 'Quit menu QA'); await a.selectOption('#mpFormat', 'duos');
    await a.click('#mpCreate'); await a.waitForSelector('#mpInvite');
    const lobbyId = [...hub.lobbies.values()].find(l => l.owner === alice).id;
    hub.emit(hub.users.get(bob), 'joinRequested', { lobbyId, friendId: alice });
    await b.waitForSelector('.mpseat.me', { timeout: 20000 });
    await a.waitForFunction(() => document.querySelectorAll('.mpseat.taken').length === 2);
    await a.click('#mpStart');
    for (const p of clients) {
      await p.waitForFunction(() => !!__digi.session, null, { timeout: 20000 });
      await p.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    }
    check(await a.locator('#topbar button').evaluateAll(buttons => buttons.every(b => { const r = b.getBoundingClientRect(); return !r.width || (r.left >= 0 && r.right <= innerWidth); })), 'Menu and existing game controls fit at 1280px');
    await a.click('#gameMenuBtn');
    check(await a.locator('[data-game-pause]').count() === 0 && !await a.evaluate(() => DT.UI.paused()), 'multiplayer has no pause option');
    await a.click('[data-game-leave]');
    check((await a.locator('#gameMenuDescription').textContent()).includes('Remaining players'), 'multiplayer confirmation explains that others continue');
    const roundBefore = await b.evaluate(() => __digi.game.roundLabel);
    await a.click('[data-game-confirm]');
    await a.waitForFunction(() => !DT.UI.game() && !DT.Net.status().room && DT.Client.page() === 'home', null, { timeout: 20000 });
    await b.waitForFunction(() => DT.Net.debug()?.host === '76561190000000002', null, { timeout: 30000 }).catch(() => {});
    const lobby = hub.lobbies.get(lobbyId);
    check(lobby && lobby.owner === bob && lobby.data.host === bob, 'leaving through the menu transfers the lobby to the remaining player');
    check(await b.evaluate(() => !!__digi.session && !!__digi.game), 'remaining Duos player stays in the match');
    for (let i = 0; i < 60 && await b.evaluate(() => __digi.game.roundLabel) === roundBefore; i++) {
      const mine = await b.evaluate(() => { const g = __digi.game; return g.carousel && g.carouselTurn() === g.human; });
      if (mine) await b.locator('.choice[data-carousel]:not(.taken)').first().click();
      if (await b.locator('#readyBtn').isEnabled()) await b.click('#readyBtn');
      if (await b.locator('#skipBtn').isEnabled()) await b.click('#skipBtn');
      await b.waitForTimeout(500);
    }
    check(await b.evaluate(() => __digi.game.roundLabel) !== roundBefore, 'the remaining player advances to another round after the host leaves');
    await b.click('#gameMenuBtn'); await b.click('[data-game-leave]'); await b.click('[data-game-confirm]');
    await b.waitForFunction(() => !DT.UI.game() && !DT.Net.status().room && DT.Client.page() === 'home', null, { timeout: 20000 });
    await b.waitForTimeout(2500);
    check(!hub.lobbies.get(lobbyId)?.members.length, 'last player can leave without lingering lobby membership');
    check(errors.length === 0, `no browser exceptions after cleanup: ${errors.join('; ')}`);
  } finally { await browser?.close(); server.kill(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
