// Real browser coverage for set art, carousel input, saved choices and compact Duos play.
// node tests/carousel.cjs <playwright-path> <output-directory> [port]
const { chromium } = require(process.argv[2] || 'playwright');
const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');
const assert = require('assert/strict');
const out = path.resolve(process.argv[3] || __dirname);
const port = Number(process.argv[4] || 8195);
const check = (ok, message) => { assert.ok(ok, message); console.log(`ok ${message}`); };
(async () => {
  fs.mkdirSync(out, { recursive: true });
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], {
    env: { ...process.env, PORT: String(port) }, windowsHide: true, stdio: 'ignore',
  });
  let browser;
  try {
    await new Promise(r => setTimeout(r, 700));
    browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
    const page = await browser.newPage({ viewport: { width: 1600, height: 900 }, reducedMotion: 'reduce' });
    const errors = [];
    page.on('pageerror', e => errors.push(String(e)));
    page.on('response', r => { if (r.status() >= 400 && !r.url().includes('fonts.g')) errors.push(`${r.status()} ${r.url()}`); });
    await page.goto(`http://localhost:${port}`);
    await page.waitForSelector('#boot', { state: 'detached', timeout: 30000 });
    await page.click('#cnPlay');
    const selected = () => page.locator('.set-card[aria-checked="true"]').getAttribute('data-set');
    const ids = await page.locator('.set-card').evaluateAll(cards => cards.map(c => c.dataset.set));
    check(ids.length === 12, 'all eleven sets and Chaos have cards');
    await page.waitForFunction(() => [...document.querySelectorAll('.sc-art')].every(img => img.complete && img.naturalWidth >= 720));
    check(await page.locator('.sc-art').evaluateAll(imgs => new Set(imgs.map(i => i.src)).size === imgs.length), 'every set loads its own full-size portrait');
    await page.locator('.set-card.on').focus();
    await page.keyboard.press('Home');
    const first = await page.locator('.set-card').first().elementHandle();
    for (let i = 1; i <= ids.length; i++) {
      await page.getByRole('button', { name: 'Next set', exact: true }).click();
      check(await selected() === ids[i % ids.length], `Next selects ${ids[i % ids.length]} and wraps`);
    }
    check(await first.evaluate(el => el.isConnected), 'carousel selection preserves the cards instead of rebuilding them');
    await page.getByRole('button', { name: 'Previous set', exact: true }).click();
    check(await selected() === ids.at(-1), 'Previous wraps to the last set');
    await page.locator('.set-card.on').focus();
    await page.keyboard.press('ArrowRight');
    check(await selected() === ids[0], 'Right arrow wraps with keyboard focus');
    await page.keyboard.press('End');
    check(await selected() === ids.at(-1), 'End selects last card');
    await page.keyboard.press('Home');
    check(await selected() === ids[0], 'Home selects first card');
    await page.getByRole('button', { name: 'Next set', exact: true }).click();
    await page.locator('#setPickSolo [data-pace="normal"]').click();
    await page.keyboard.press('ArrowRight');
    check(await selected() === ids[1], 'pace keyboard input does not move the set carousel');
    await page.locator('#setPickSolo [data-format="duos"]').click();
    check((await page.locator('#playPick').innerText()).includes('Duos'), 'selection summary follows format changes');
    await page.reload();
    await page.waitForSelector('#boot', { state: 'detached', timeout: 30000 });
    await page.click('#cnPlay');
    check(await selected() === ids[1], 'selected set persists after reload');
    check(await page.locator('#setPickSolo [data-format="duos"]').getAttribute('aria-checked') === 'true', 'Duos persists after reload');
    check(await page.locator('#setPickSolo [data-pace="normal"]').getAttribute('aria-checked') === 'true', 'pace persists after reload');
    for (const [width, height] of [[2560, 1440], [1600, 900], [1280, 720], [960, 640], [768, 1024], [390, 844]]) {
      await page.setViewportSize({ width, height });
      await page.locator('.set-card.on').focus();
      await page.keyboard.press('Home');
      await page.waitForTimeout(250);
      const fits = await page.evaluate(() => {
        const nav = document.getElementById('cnav').getBoundingClientRect();
        const track = document.querySelector('.setpick-row').getBoundingClientRect();
        const card = document.querySelector('.set-card.on');
        const box = card.getBoundingClientRect(), title = card.querySelector('b');
        return { card: box.width >= 235 && box.left >= track.left - 3 && box.right <= track.right + 3,
          title: title.scrollWidth <= title.clientWidth + 1,
          nav: [...document.querySelectorAll('.cn-tabs, .cn-right, .cn-profile')].every(el => el.getBoundingClientRect().bottom <= nav.bottom + 1),
          page: document.documentElement.scrollWidth <= innerWidth };
      });
      check(Object.values(fits).every(Boolean), `readable card, full name, contained navigation at ${width}x${height}: ${JSON.stringify(fits)}`);
      await page.screenshot({ path: path.join(out, `carousel-${width}.png`) });
    }
    // A real touch gesture scrolls the strip without unexpectedly changing the chosen rules.
    const touch = await page.context().newCDPSession(page);
    const rowBox = await page.locator('.setpick-row').boundingBox();
    const y = Math.min(rowBox.y + 100, 650);
    await touch.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x: 320, y }] });
    for (const x of [280, 220, 160, 100, 55]) {
      await touch.send('Input.dispatchTouchEvent', { type: 'touchMove', touchPoints: [{ x, y }] });
      await page.waitForTimeout(25);
    }
    await touch.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
    await page.waitForTimeout(600);
    check(await page.locator('.setpick-row').evaluate(el => el.scrollLeft > 50), 'touch swipe browses the carousel');
    check(await selected() === ids[0], 'browsing leaves selection unchanged until a card is chosen');
    await page.setViewportSize({ width: 960, height: 640 });
    await page.locator('[data-mode-pick="solo"]').click();
    await page.click('#playConfirm');
    check((await page.locator('#partyTitle').textContent()).trim() === 'Faction Wars', 'Confirm opens the selected set party');
    await page.click('#playSoloBtn');
    await page.waitForSelector('#cload', { state: 'detached', timeout: 15000 });
    await page.waitForFunction(() => window.__digi?.game && !document.body.classList.contains('client-on'));
    await page.waitForTimeout(500);
    check(!await page.locator('#duoPanel').isVisible(), 'partner panel does not cover opening carousel choices');
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(500);
    check(await page.evaluate(() => DT.SetPicker.format() === 'duos') && await page.locator('#duoPanel').isVisible(), 'compact window reaches Duos planning with partner controls');
    const gameFits = await page.evaluate(() => {
      const ids = ['app', 'topbar', 'shopbar', 'arena'];
      return ids.every(id => { const r = document.getElementById(id).getBoundingClientRect(); return r.left >= 0 && r.right <= innerWidth + 1 && r.bottom <= innerHeight + 1 && r.height > 0; });
    });
    check(gameFits, 'board, top bar and shop fit the 960x640 game window');
    await page.screenshot({ path: path.join(out, 'compact-duos.png') });
    check(!errors.length, `no missing art or browser errors: ${errors.join('; ')}`);
  } finally { await browser?.close(); server.kill(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
