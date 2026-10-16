// Digi-Farm check: opens from the title screen with every owned legend up to the room cap, feeding, a fake clock for
// hunger and poop, sickness and cure, the four training mini-games, the daily reward cap, the legendevo care weights,
// the Locker entry, a reload, Esc back to a working game, and no overflow at 1200x760 and 1600x900.
// Run: node tests/farm.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8159;
const HOUR = 3600 * 1000;

const fails = [];
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails.push(what); };

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 600));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1200, height: 760 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(m.text()); });
  page.on('response', (r) => { if (r.status() >= 400) errors.push(`${r.status()} ${r.url()}`); });
  const shot = (n) => page.screenshot({ path: path.join(out, n) });
  const ev = (fn, arg) => page.evaluate(fn, arg);
  const L = 'tokomon'; // free legend with bond, food and wild branches
  const card = () => ev((id) => window.DT.Farm.card(id), L);
  // keeps the legend fit for the next step without touching anything the step checks
  const refresh = () => ev((id) => { const p = window.DT.Farm._save().pets[id]; Object.assign(p, { energy: 100, hunger: 90, sick: false, tantrum: 0, nap: false }); }, L);
  const overflow = () => ev(() => {
    const bad = [];
    const win = document.querySelector('#dtFarm .df-win').getBoundingClientRect();
    if (win.left < 0 || win.top < 0 || win.right > innerWidth || win.bottom > innerHeight) bad.push('window off screen');
    for (const sel of ['.df-head', '.df-card', '.df-scene', '.df-mbox']) {
      const el = document.querySelector(`#dtFarm ${sel}`);
      if (!el || !el.offsetParent) continue;
      if (el.scrollWidth > el.clientWidth + 1) bad.push(`${sel} x ${el.scrollWidth}>${el.clientWidth}`);
      if (sel === '.df-card' && el.scrollHeight > el.clientHeight + 1) bad.push(`${sel} y ${el.scrollHeight}>${el.clientHeight}`);
    }
    const scene = document.querySelector('#dtFarm .df-scene').getBoundingClientRect();
    for (const el of document.querySelectorAll('#dtFarm .df-bar, #dtFarm .df-act, #dtFarm .df-chip, #dtFarm .df-menu:not(.hidden)')) {
      const r = el.getBoundingClientRect();
      if (r.left < scene.left - 1 || r.right > scene.right + 1 || r.bottom > scene.bottom + 1 || r.top < scene.top - 1) bad.push(`${el.className} outside the scene`);
    }
    for (const el of document.querySelectorAll('#dtFarm .df-head button, #dtFarm .df-pill')) {
      const r = el.getBoundingClientRect();
      if (r.right > win.right + 1 || r.left < win.left - 1) bad.push(`${el.textContent.trim()} outside the header`);
    }
    if (document.documentElement.scrollWidth > innerWidth) bad.push('page scrolls sideways');
    return bad;
  });

  try {
    await page.goto(`http://localhost:${PORT}/?bits=20000`);
    await page.waitForTimeout(1800);
    // own 11 legends: more than the starting room of 6
    await ev(() => { const C = window.DT.Cosmetics; for (const x of C.list('legend').slice(3, 11)) C.grant('legend', x.id); });
    const owned = await ev(() => window.DT.Cosmetics.list('legend').filter((x) => window.DT.Cosmetics.owns('legend', x.id)).length);

    // ---- open from the title screen ----
    check(await page.locator('#farmTitleBtn').isVisible(), 'title screen has a Digi-Farm button');
    await shot('farm0-title.png');
    await page.click('#farmTitleBtn');
    await page.waitForSelector('#dtFarm:not(.hidden)');
    await page.waitForTimeout(3500);
    const room = await ev(() => ({ st: window.DT.Farm.state(), agents: window.DT.FarmUI.agents(), chips: document.querySelectorAll('#dtFarm .df-chip').length,
      views: window.DT.FarmUI.agents().filter((id) => window.DT.FarmUI._scene().agents.get(id).view.ready).length }));
    check(room.st.cap === 6 && room.agents.length === Math.min(owned, 6) && room.chips === room.agents.length, `room holds ${room.agents.length} of ${owned} owned legends (cap ${room.st.cap})`);
    check(room.st.bank.length === owned - 6, `the rest wait in the Digi-Bank (${room.st.bank.length})`);
    check(room.views === room.agents.length, `every resident has a loaded model (${room.views}/${room.agents.length})`);
    check((await overflow()).length === 0, `no overflow at 1200x760 ${(await overflow()).join('; ')}`);
    await shot('farm1-room.png');

    // ---- Digi-Bank and expansion ----
    await page.click('#dtFarm [data-fm="bank"]');
    await page.waitForTimeout(400);
    check(await page.locator('#dtFarm .df-mbox [data-swap]').count() === owned - 6, 'bank lists swap buttons for the waiting legends');
    check((await overflow()).length === 0, `bank fits ${(await overflow()).join('; ')}`);
    await shot('farm2-bank.png');
    await page.keyboard.press('Escape');
    const bits0 = await ev(() => window.DT.Wallet.bits);
    await page.click('#dtFarm [data-fm="expand"]');
    await page.waitForTimeout(800);
    const exp = await ev(() => ({ st: window.DT.Farm.state(), agents: window.DT.FarmUI.agents().length, bits: window.DT.Wallet.bits }));
    check(exp.st.cap === 8 && exp.agents === 8 && bits0 - exp.bits === 250, `expansion: room ${exp.st.cap}, ${exp.agents} legends, cost ${bits0 - exp.bits}`);
    check(await page.locator('#dtFarm:not(.hidden)').count() === 1, 'Esc closed the bank but not the farm');

    // ---- the clock: hunger drops, poop appears ----
    await ev((id) => window.DT.FarmUI.select(id), L);
    const c0 = await card();
    await ev(() => window.DT.Farm._advance(4 * 3600 * 1000));
    await page.waitForTimeout(1500);
    const c1 = await card();
    const piles1 = await ev(() => ({ n: window.DT.Farm.piles().length, meshes: window.DT.FarmUI._scene().piles.size }));
    check(c1.hunger < c0.hunger - 20, `4 hours later hunger dropped ${c0.hunger.toFixed(0)} -> ${c1.hunger.toFixed(0)}`);
    check(c1.energy < c0.energy, `energy dropped ${c0.energy.toFixed(0)} -> ${c1.energy.toFixed(0)}`);
    check(piles1.n > 0 && piles1.meshes === piles1.n, `poop appeared (${piles1.n} piles, ${piles1.meshes} in the room)`);
    await shot('farm3-poop.png');

    // ---- feed through the action bar ----
    await page.click('#dtFarm [data-fa="feed"]');
    await page.waitForTimeout(300);
    check((await overflow()).length === 0, `food menu fits ${(await overflow()).join('; ')}`);
    await shot('farm4-feed-menu.png');
    await page.click('#dtFarm [data-food="meat"]');
    await page.waitForTimeout(600);
    const c2 = await card();
    check(c2.hunger > c1.hunger + 20 && c2.weight > c1.weight, `meat: hunger ${c1.hunger.toFixed(0)} -> ${c2.hunger.toFixed(0)}, weight ${c1.weight} -> ${c2.weight} G`);
    const meat = await ev(() => window.DT.Farm.state().meat.n);
    check(meat === 2, `the free meat tray went down to ${meat}/3`);
    await ev((id) => window.DT.Farm.feed(id, 'bigmeat'), L);
    const c3 = await card();
    check(c3.weight >= c2.weight + 3, `Giant Meat adds weight (${c2.weight} -> ${c3.weight} G)`);
    await shot('farm5-fed.png');

    // ---- leave the poop: sickness, then cure ----
    await ev(() => window.DT.Farm._advance(6 * 3600 * 1000));
    await page.waitForTimeout(1200);
    const sick = await ev(() => window.DT.Farm.state().residents.map((id) => window.DT.Farm.card(id)).filter((c) => c.sick).map((c) => ({ id: c.id, why: c.sickWhy })));
    check(sick.length > 0 && sick.some((s) => s.why === 'dirty'), `leaving poop made legends sick (${sick.map((s) => `${s.id}:${s.why}`).join(', ')})`);
    const target = sick.find((s) => s.id === L) ? L : sick[0].id;
    await ev((id) => window.DT.FarmUI.select(id), target);
    await page.waitForTimeout(400);
    const neglect = await ev((id) => window.DT.Farm.card(id).poopNeglect, target);
    check(neglect > 0, `poop left out counts as neglect (${neglect})`);
    await shot('farm6-sick.png');
    await page.click('#dtFarm [data-fa="cure"]');
    await page.click('#dtFarm [data-cure="bandage"]');
    await page.waitForTimeout(500);
    const cured = await ev((id) => ({ c: window.DT.Farm.card(id), band: window.DT.Farm.state().bandage.n }), target);
    check(!cured.c.sick && cured.band === 1, `bandage cured ${target} (sick ${cured.c.sick}, bandages left ${cured.band})`);
    const medBits = await ev(() => window.DT.Wallet.bits);
    const other = sick.find((s) => s.id !== target);
    if (other) {
      const r = await ev((id) => window.DT.Farm.cure(id, 'medicine'), other.id);
      const after = await ev(() => window.DT.Wallet.bits);
      check(r.ok && medBits - after === 20 - (r.reward ? r.reward.bits : 0), `medicine cured ${other.id} for 20 Bits`);
    }
    await page.click('#dtFarm [data-fa="clean"]');
    await page.waitForTimeout(800);
    check(await ev(() => window.DT.Farm.piles().length) === 0, 'Clean removed every pile');

    // ---- the mini-games ----
    await ev((id) => window.DT.FarmUI.select(id), L);
    for (const g of ['strength', 'speed', 'wisdom', 'stamina']) {
      await refresh();
      const before = await card();
      const p = ev((name) => window.DT.FarmUI.playGame(name, { auto: true, speed: 2 }).then((r) => ({ score: r.score, ok: r.res && r.res.ok, gains: r.res && r.res.gains })), g);
      await page.waitForTimeout(2200);
      if (g === 'strength' || g === 'wisdom') {
        check((await overflow()).length === 0, `${g} game fits ${(await overflow()).join('; ')}`);
        await shot(`farm7-game-${g}.png`);
      }
      const r = await p;
      const after = await card();
      const stat = { strength: 'str', speed: 'spd', wisdom: 'wis', stamina: 'sta' }[g];
      check(r.ok && r.score >= 0.5 && after[stat] > before[stat], `${g}: score ${Math.round(r.score * 100)}, ${stat} ${before[stat]} -> ${after[stat]}`);
      check(after.energy < before.energy && after.weight <= before.weight, `${g}: training used energy (${before.energy.toFixed(0)} -> ${after.energy.toFixed(0)}) and trimmed weight (${before.weight} -> ${after.weight} G)`);
      if (g === 'stamina') await shot('farm8-result.png');
      await page.click('#dtFarm .df-mbox [data-mclose]');
      await page.waitForTimeout(200);
    }
    const tired = await ev((id) => { Object.assign(window.DT.Farm._save().pets[id], { energy: 5 }); return window.DT.Farm.canTrain(id); }, L);
    check(!!tired, `a tired legend refuses to train ("${tired}")`);

    // ---- the daily reward cap ----
    const capRun = await ev((id) => {
      const F = window.DT.Farm;
      const w0 = window.DT.Wallet.bits;
      for (let i = 0; i < 40; i++) { Object.assign(F._save().pets[id], { energy: 100, hunger: 100, tantrum: 0, sick: false }); F.train(id, 'speed', 1); }
      const d = F.state().daily;
      const w1 = window.DT.Wallet.bits;
      Object.assign(F._save().pets[id], { energy: 100, hunger: 100 });
      const r = F.train(id, 'speed', 1);
      return { d, earned: w1 - w0, last: r.reward, w2: window.DT.Wallet.bits - w1 };
    }, L);
    check(capRun.d.bits === 60 && capRun.d.data === 100, `daily farm earnings stop at the cap (${capRun.d.bits}/60 Bits, ${capRun.d.data}/100 Digi-Data)`);
    check(capRun.last.bits === 0 && capRun.last.data === 0 && capRun.w2 === 0, 'training after the cap earns nothing');

    // ---- legendevo care weights ----
    const evo = await ev((id) => {
      const E = window.DT.LegendEvo, F = window.DT.Farm;
      const w = () => Object.fromEntries(E.odds(id).map((o) => [o.care, o.weight]));
      const pct = () => Object.fromEntries(E.odds(id).map((o) => [o.care, o.pct]));
      const before = { odds: w(), pct: pct(), bonus: E.careBonus(id) };
      for (let i = 0; i < 3; i++) { F.praise(id); F._save().pets[id].lastPraise = 0; }
      for (let i = 0; i < 6; i++) { F._save().pets[id].hunger = 30; F.feed(id, 'carrot'); }
      const good = { odds: w(), pct: pct(), bonus: E.careBonus(id) };
      // neglect: mistakes, poop left out, overweight
      Object.assign(F._save().pets[id], { mistakes: 4, poopNeglect: 3, weight: 40 });
      const bad = { odds: w(), pct: pct(), bonus: E.careBonus(id) };
      return { before, good, bad, kids: E.EVO[id] };
    }, L);
    console.log('branch %', JSON.stringify(evo.before.pct), JSON.stringify(evo.good.pct), JSON.stringify(evo.bad.pct));
    check(evo.good.bonus.battle > 0 && evo.good.bonus.bond > 0 && evo.good.bonus.food > 0, `farm care adds to Bond +${evo.good.bonus.bond}, Battle +${evo.good.bonus.battle}, Food +${evo.good.bonus.food}`);
    check(evo.good.odds.bond > evo.before.odds.bond && evo.good.odds.food > evo.before.odds.food && evo.good.odds.wild < evo.before.odds.wild, `good care raises the Bond and Food branch weights and lowers Wild (${evo.before.pct.wild}% -> ${evo.good.pct.wild}%)`);
    check(evo.bad.bonus.wild >= 4 * 3 + 3 * 2 + 6 && evo.bad.odds.wild > evo.good.odds.wild, `mistakes, poop neglect and weight raise the Wild branch (+${evo.bad.bonus.wild}, ${evo.good.pct.wild}% -> ${evo.bad.pct.wild}%)`);
    await page.waitForTimeout(1300);
    const pushTxt = await page.locator('#dtFarm .df-push').innerText();
    check(/Wild/.test(pushTxt) && /\+2[0-9]/.test(pushTxt), 'the farm card shows which farm stats push which branch');
    await shot('farm9-card-evo.png');
    await page.click('#dtFarm [data-evo-open]');
    await page.waitForSelector('#dtEvo:not(.hidden)');
    await page.waitForTimeout(800);
    const note = await page.locator('#dtEvo [data-farm-note]').count();
    check(note === 1, 'the Digivolve panel shows the Digi-Farm care bonus');
    await shot('farm10-evo-panel.png');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    check(await page.locator('#dtEvo.hidden').count() === 1 && await page.locator('#dtFarm:not(.hidden)').count() === 1, 'Esc closes the Digivolve panel first, the farm stays open');
    await ev((id) => Object.assign(window.DT.Farm._save().pets[id], { mistakes: 0, poopNeglect: 0, weight: 12 }), L);

    // ---- offline catch-up is capped: nobody comes back to a wrecked pet ----
    const away = await ev((id) => {
      const F = window.DT.Farm;
      Object.assign(F._save().pets[id], { energy: 100, hunger: 100, happy: 100 });
      const m0 = F.card(id).mistakes;
      F._advance(5 * 24 * 3600 * 1000);
      const c = F.card(id);
      return { added: c.mistakes - m0, hunger: c.hunger, max: F.OFFLINE_MISTAKES };
    }, L);
    check(away.added <= away.max, `5 days away adds at most ${away.max} care mistakes per legend (${away.added})`);

    // ---- night ----
    await page.click('#dtFarm [data-fa="lights"]');
    await page.waitForTimeout(2500);
    const night = await ev(() => ({ lights: window.DT.Farm.state().lights, cls: document.querySelector('#dtFarm .df-scene').classList.contains('night') }));
    check(!night.lights && night.cls, 'Lights off darkens the room');
    await shot('farm11-night.png');
    await page.click('#dtFarm [data-fa="lights"]');

    // ---- themes ----
    await page.click('#dtFarm [data-fm="themes"]');
    await page.click('#dtFarm [data-theme="snowy"]');
    await page.waitForTimeout(1500);
    check(await ev(() => window.DT.Farm.state().theme) === 'snowy', 'bought and switched to the snowy theme');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(500);
    await shot('farm12-snowy.png');

    // ---- 1600x900 ----
    await page.setViewportSize({ width: 1600, height: 900 });
    await page.waitForTimeout(1200);
    check((await overflow()).length === 0, `no overflow at 1600x900 ${(await overflow()).join('; ')}`);
    await shot('farm13-1600.png');
    await page.setViewportSize({ width: 1200, height: 760 });
    await page.waitForTimeout(500);

    // ---- save survives a reload; older saves untouched ----
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    check(await page.locator('#dtFarm.hidden').count() === 1, 'Esc closes the farm');
    await page.reload();
    await page.waitForTimeout(1800);
    const saved = await ev(() => ({ farm: JSON.parse(localStorage.getItem('dt.farm.v1')), evo: !!JSON.parse(localStorage.getItem('dt.legendevo.v1')), cos: !!JSON.parse(localStorage.getItem('dt.cosmetics.v1')), st: window.DT.Farm.state() }));
    check(saved.farm && saved.farm.v === 1 && saved.st.cap === 8 && saved.st.theme === 'snowy' && saved.evo && saved.cos, `dt.farm.v1 reloads (room ${saved.st.cap}, ${saved.st.theme}) beside the older saves`);
    check(await ev(() => { try { localStorage.setItem('dt.farm.v1', '{bad json'); return true; } catch { return false; } }), 'wrote a corrupt farm save');
    await page.reload();
    await page.waitForTimeout(1500);
    check(await ev(() => window.DT.Farm.state().cap === 6 && window.DT.Farm.state().residents.length === 6), 'a corrupt farm save falls back to a fresh farm');

    // ---- Locker entry ----
    await page.click('#lockerBtn').catch(() => page.evaluate(() => window.DT.Shop.openLocker('legend')));
    await page.waitForTimeout(300);
    await ev(() => window.DT.Shop.openLocker('legend'));
    await page.waitForSelector('#dtStore .dte-bsec [data-farm-open]');
    await page.click('#dtStore .dte-bsec [data-farm-open]');
    await page.waitForSelector('#dtFarm:not(.hidden)');
    check(await ev(() => !window.DT.Shop.isOpen()), 'the Locker opens the farm and closes the store');
    await page.waitForTimeout(800);
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);

    // ---- the main game still works ----
    await page.click('#startBtn');
    await page.waitForTimeout(800);
    if (await page.locator('.choice[data-carousel]:not(.taken)').count()) {
      await page.locator('.choice[data-carousel]:not(.taken)').first().click();
      await page.click('#carouselGo');
    }
    await page.waitForTimeout(1500);
    const gameOk = await ev(() => { const g = window.DT.UI.game(); return g ? { players: g.players.length, phase: g.phase, gold: g.human.gold } : null; });
    check(gameOk && gameOk.players === 8, `a solo game starts after the farm (${JSON.stringify(gameOk)})`);
    const g0 = gameOk.gold;
    await page.mouse.move(600, 380);
    await page.keyboard.press('d');
    await page.waitForTimeout(300);
    const g1 = await ev(() => window.DT.UI.game().human.gold);
    check(g1 === g0 - 1 || g0 === 0, `game keys reach the game again (D reroll: gold ${g0} -> ${g1})`);
    await shot('farm14-game.png');

    check(errors.length === 0, `no page errors ${errors.slice(0, 5).join(' | ')}`);
  } catch (e) {
    console.error(e);
    fails.push(String(e));
    try { await shot('farm-error.png'); } catch { /* ignore */ }
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fails.length ? `\n${fails.length} FAILED` : '\nall passed');
  process.exit(fails.length ? 1 : 0);
})();
