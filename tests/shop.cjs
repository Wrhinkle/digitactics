// Store check: Bits award once per game, buy an item, hatch an egg, equip in the Locker. Saves screenshots.
// Run: node tests/shop.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8141;

const fails = [];
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails.push(what); };

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(m.text()); });
  page.on('response', (r) => { if (r.status() >= 400) errors.push(`${r.status()} ${r.url()}`); });
  const shot = (n) => page.screenshot({ path: path.join(out, n) });
  const bits = () => page.evaluate(() => window.DT.Wallet.bits);

  await page.goto(`http://localhost:${PORT}/?bits=5000`);
  await page.waitForTimeout(2000);
  check((await bits()) === 5000, `?bits=5000 dev helper gave ${await bits()}`);
  // kinds no agent has filled yet get stand-in items so every tab can be exercised
  const standIns = await page.evaluate(() => {
    const C = window.DT.Cosmetics, made = [];
    const rar = ['common', 'rare', 'epic', 'legendary'];
    for (const k of C.KINDS) {
      if (C.list(k).filter((x) => !x.free).length >= 3) continue;
      C.register(k, rar.map((r, i) => ({ id: `test_${k}_${r}`, name: `Test ${k} ${r}`, price: 150 + i * 150, rarity: r, icon: ['*', '+', '#', '@'][i] })));
      made.push(k);
    }
    return made;
  });
  console.log('stand-in items for:', standIns.join(', ') || 'none');
  await shot('s0-title.png');
  check(await page.locator('#storeTitleBtn').isVisible(), 'Store button on the title screen');

  // ---- a solo game, ended quickly ----
  await page.click('#startBtn');
  await page.waitForTimeout(500);
  await page.click('#timerBtn');
  await page.locator('.choice[data-carousel]:not(.taken)').first().click();
  await page.click('#carouselGo');
  await page.waitForTimeout(500);
  check(await page.locator('#storeBtn').isVisible(), 'Store button in the top bar during planning');
  check(await page.locator('#lockerBtn').isVisible(), 'Locker button in the top bar');
  // every AI Tamer is knocked out by this round's combat, so the game ends with you in 1st
  await page.evaluate(() => { for (const p of window.__digi.game.players) if (p !== window.__digi.game.human) p.hp = 0; });
  await page.keyboard.press('Space');
  await page.waitForTimeout(400);
  if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
  await page.waitForSelector('#overlay:not(.hidden) .dts-award', { timeout: 15000 });
  await page.waitForTimeout(600);
  await shot('s1-award-counting.png');
  await page.waitForTimeout(3500);
  await shot('s2-award.png');
  const award = await page.evaluate(() => ({
    lines: [...document.querySelectorAll('.dts-award .dts-aline')].map((r) => r.textContent.replace(/\s+/g, ' ').trim()),
    total: Number(document.querySelector('.dts-award .tot b').dataset.n),
    place: window.__digi.game.human.placement,
  }));
  console.log('award:', JSON.stringify(award));
  check(award.place === 1 && award.lines[0].includes('#1') && award.lines[0].includes('120'), 'placement #1 pays 120');
  check(award.lines.some((l) => l.includes('First game of the day') && l.includes('60')), 'first game of the day +60');
  check((await bits()) === 5000 + award.total, `wallet is 5000 + ${award.total} after the game (${await bits()})`);
  const again = await page.evaluate(() => { const a = window.DT.Shop.award(); return { fresh: a && a.fresh, bits: window.DT.Wallet.bits }; });
  check(again.fresh === false && again.bits === 5000 + award.total, 'a second award() for the same game pays nothing');

  // tracker: a PvP win that knocks a Tamer out counts as a win and an elimination
  const fake = await page.evaluate(() => {
    const me = { id: 0, alive: true, level: 7, placement: 0, allUnits: () => [{ stage: 2 }] };
    const opp = { id: 3, alive: false };
    const g = { seed: 424242, human: me, results: [{ match: { a: me, b: opp, pve: false }, aWon: true, bWon: false, dmgA: 0, dmgB: 4 }] };
    const t = window.DT.Shop._track(g);
    me.placement = 2;
    const before = window.DT.Wallet.bits;
    const a = window.DT.Shop.award(g, null);
    return { wins: t.pvpWins, kills: t.kills, stars: t.maxStars, lines: a.lines, got: window.DT.Wallet.bits - before, total: a.total };
  });
  console.log('tracker check:', JSON.stringify(fake));
  check(fake.wins === 1 && fake.kills === 1 && fake.stars === 3, 'tracker counts PvP wins, eliminations and 3-star units');
  check(fake.lines.some((l) => l.n === 25) && fake.lines.some((l) => l.n === 6) && fake.lines[0].n === 90 && fake.got === fake.total, '2nd place 90 + win 6 + elimination 25 paid');

  // ---- the Store ----
  await page.click('.dts-award [data-shop-open]');
  await page.waitForSelector('#dtStore:not(.hidden)');
  await page.waitForTimeout(800);
  await shot('s3-store.png');
  const featured = await page.locator('.dts-feat .dts-card').count();
  check(featured > 0, `featured row shows ${featured} items`);
  const buyBtn = page.locator('.dts-body > section:not(.dts-feat) .dts-card [data-buy]:not(.poor)').first();
  const target = await buyBtn.evaluate((b) => ({ kind: b.closest('[data-kind]').dataset.kind, id: b.dataset.buy }));
  const b0 = await bits();
  await buyBtn.click();
  await page.waitForTimeout(400);
  await shot('s4-confirm.png');
  check((await bits()) === b0, 'nothing is spent before the confirm step');
  await page.click('[data-confirm]');
  await page.waitForTimeout(400);
  const bought = await page.evaluate((t) => ({ owns: window.DT.Cosmetics.owns(t.kind, t.id), price: window.DT.Shop.priceOf(t.kind, window.DT.Cosmetics.get(t.kind, t.id)) }), target);
  check(bought.owns && (await bits()) === b0 - bought.price, `bought ${target.kind}/${target.id} for ${bought.price}`);
  await page.click('[data-equipnow]');
  await page.waitForTimeout(300);
  check(await page.evaluate((t) => window.DT.Cosmetics.equipped(t.kind) === t.id, target), 'Equip now equips it');

  // ---- a Digi-Egg ----
  await page.click('[data-tab="eggs"]');
  await page.waitForTimeout(400);
  await shot('s5-eggs.png');
  const odds = await page.evaluate(() => window.DT.Shop.eggs().map((e) => `${e.id}:${e.odds.map((o) => `${o.rarity} ${o.pct.toFixed(1)}%`).join('/')}`));
  console.log('egg odds:', odds.join(' | '));
  const ownedBefore = await page.evaluate(() => window.DT.Cosmetics.KINDS.reduce((n, k) => n + window.DT.Cosmetics.list(k).filter((x) => window.DT.Cosmetics.owns(k, x.id)).length, 0));
  const e0 = await bits();
  await page.click('.dts-eggs [data-egg="mixed"]');
  await page.click('[data-confirm]');
  await page.waitForTimeout(700);
  await shot('s6-hatch-wobble.png');
  await page.waitForSelector('.dts-egg.crack', { state: 'attached', timeout: 5000 });
  await shot('s7-hatch-crack.png');
  await page.waitForSelector('.dts-reveal:not(.hidden)', { timeout: 5000 });
  await page.waitForTimeout(500);
  await shot('s8-hatch-reveal.png');
  const egg = await page.evaluate(() => ({
    text: document.querySelector('.dts-reveal').textContent.replace(/\s+/g, ' ').trim(),
    owned: window.DT.Cosmetics.KINDS.reduce((n, k) => n + window.DT.Cosmetics.list(k).filter((x) => window.DT.Cosmetics.owns(k, x.id)).length, 0),
  }));
  const e1 = await bits();
  console.log('hatched:', egg.text);
  check(egg.owned === ownedBefore + 1 ? e1 === e0 - 350 : egg.text.includes('Duplicate') && e1 > e0 - 350, `egg cost 350 (new item, or a duplicate refund): ${e0} -> ${e1}`);
  await page.click('.dts-reveal [data-cancel]');

  // ---- the Locker: put an owned emote in slot 2 ----
  await page.click('[data-mode="locker"]');
  await page.click('[data-tab="emote"]');
  await page.waitForTimeout(300);
  await page.click('.dts-slot[data-slot="1"]');
  await page.waitForTimeout(200);
  const slotBtn = page.locator('[data-slotset]').last();
  const emote = await slotBtn.getAttribute('data-slotset');
  await slotBtn.click();
  await page.waitForTimeout(300);
  check(await page.evaluate((id) => window.DT.Cosmetics.equipped('emote')[1] === id, emote), `Locker put ${emote} in emote slot 2`);
  await page.click('.dts-slot[data-slot="1"]');
  await page.waitForTimeout(300);
  await shot('s9-locker-emote.png');
  await page.click('[data-tab="legend"]');
  await page.waitForTimeout(500);
  await shot('s10-locker-legend.png');

  // ---- sizes ----
  await page.setViewportSize({ width: 1200, height: 760 });
  await page.click('[data-mode="store"]');
  await page.waitForTimeout(500);
  await shot('s11-store-1200.png');
  const overflow = await page.evaluate(() => document.querySelector('.dts-win').scrollWidth > document.querySelector('.dts-win').clientWidth + 1);
  check(!overflow, 'no horizontal overflow at 1200x760');
  await page.setViewportSize({ width: 1920, height: 1080 });
  await page.waitForTimeout(500);
  await shot('s12-store-1920.png');
  await page.keyboard.press('Escape');
  await page.waitForTimeout(200);
  check(!(await page.evaluate(() => window.DT.Shop.isOpen())), 'Escape closes the Store');
  check(await page.evaluate(() => window.DT.Lock && typeof window.__digi.game.over === 'boolean'), 'game still present');

  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  console.log(fails.length ? `${fails.length} check(s) failed` : 'all shop checks passed');
  await browser.close();
  server.kill();
  process.exit(fails.length || errors.length ? 1 : 0);
})();
