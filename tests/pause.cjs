// Real solo UI flows: planning, combat, results, repeated resume, and abandoning a paused game.
// node tests/pause.cjs <playwright-path> <output-directory> [port]
const { chromium } = require(process.argv[2] || 'playwright');
const { spawn } = require('child_process');
const fs = require('fs'), path = require('path'), assert = require('assert/strict');
const out = path.resolve(process.argv[3] || __dirname), port = Number(process.argv[4] || 8195);
const check = (ok, message) => { assert.ok(ok, message); console.log(`PASS ${message}`); };
(async () => {
  fs.mkdirSync(out, { recursive: true });
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT: String(port) }, windowsHide: true, stdio: 'ignore' });
  let browser;
  try {
    await new Promise(r => setTimeout(r, 600));
    browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
    const page = await browser.newPage({ viewport: { width: 960, height: 640 } });
    const errors = []; page.on('pageerror', e => errors.push(String(e)));
    await page.goto(`http://localhost:${port}`);
    await page.waitForSelector('#boot', { state: 'detached' });
    await page.click('#startBtn');
    await page.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForFunction(() => __digi.game.phase === 'planning' && __digi.game.roundType !== 'carousel');
    const pause = async () => {
      await page.click('#gameMenuBtn'); await page.click('[data-game-pause]');
      await page.waitForFunction(() => DT.UI.paused());
    };
    const snapshot = () => page.evaluate(() => ({ phase: __digi.game.phase, round: __digi.game.roundLabel,
      plan: __digi.ui.planLeft, clock: __digi.clock, combat: __digi.own?.t,
      hp: __digi.own?.units.map(u => u.hp), gold: __digi.game.human.gold, render: __digi.renderer.time }));
    await pause();
    const planning = await snapshot();
    await page.keyboard.press('d'); await page.keyboard.press('f');
    await page.waitForTimeout(2600);
    assert.deepEqual(await snapshot(), planning);
    check(true, 'solo pause freezes planning, rendering and game hotkeys');
    check(await page.locator('#gameMenu').evaluate(el => { const r = el.getBoundingClientRect(); return r.left >= 0 && r.right <= innerWidth && r.top >= 0 && r.bottom <= innerHeight; }), 'pause menu fits 960 × 640');
    await page.screenshot({ path: path.join(out, 'solo-paused-960.png') });
    await page.click('[data-game-return]');
    await page.waitForFunction(() => !DT.UI.paused());
    await page.waitForTimeout(350);
    const resumed = await snapshot();
    check(resumed.plan < planning.plan && resumed.plan > planning.plan - 1, 'Resume game continues the timer without charging paused time');
    await page.click('#readyBtn');
    await page.waitForFunction(() => __digi.game.phase === 'combat' && __digi.own?.t > 0);
    await pause();
    const combat = await snapshot();
    await page.waitForTimeout(2600);
    assert.deepEqual(await snapshot(), combat);
    check(true, 'combat time, unit health and animation remain frozen');
    await page.keyboard.press('Escape');
    await page.waitForFunction(t => !DT.UI.paused() && __digi.own.t > t, combat.combat);
    check(true, 'Escape resumes combat');
    await page.click('#skipBtn');
    await page.waitForFunction(() => __digi.game.phase === 'result');
    await pause();
    const result = await snapshot();
    await page.waitForTimeout(3000);
    assert.deepEqual(await snapshot(), result);
    check(true, 'pause suspends the result delay beyond its original deadline');
    await page.click('[data-game-return]');
    await page.waitForTimeout(150);
    check(await page.evaluate(() => __digi.game.phase === 'result'), 'resuming preserves the remaining result delay');
    await page.waitForFunction(round => __digi.game.roundLabel !== round && __digi.game.phase === 'planning', result.round);
    check(true, 'the next round starts normally after resuming results');
    await pause();
    await page.click('[data-game-leave]');
    check(await page.evaluate(() => DT.UI.paused()), 'leave confirmation keeps a paused match frozen');
    await page.click('[data-game-return]');
    await page.waitForFunction(() => !DT.UI.paused());
    check(true, 'cancelling leave resumes the same match');
    await pause();
    await page.click('[data-game-leave]'); await page.click('[data-game-confirm]');
    await page.waitForFunction(() => !DT.UI.game() && !DT.UI.paused() && DT.Client.page() === 'home');
    await page.waitForTimeout(2600);
    check(await page.evaluate(() => !DT.UI.game() && DT.Client.history().length === 0), 'leaving a paused match clears its callbacks without saving a result');
    await page.click('#startBtn');
    await page.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    check(await page.evaluate(() => !!DT.UI.game() && !DT.UI.paused()), 'a new match does not inherit pause');
    check(errors.length === 0, `no browser exceptions: ${errors.join('; ')}`);
  } finally { if (browser) await browser.close(); server.kill(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
