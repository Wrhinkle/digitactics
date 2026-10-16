// Client shell check (js/client.js, js/theme.js): every nav page (Home with its Overview / Patch notes / Events tabs,
// Modes, Collection, Loot, Store, Digi-Farm, Codex, Profile, Settings), the social column, no sideways overflow or
// clipped text from 390x844 through 1920x1080, including 960x640 desktop and 768x1024 portrait. Esc goes back,
// buy in the Store and see it in the Collection, equip it, the Digi-Farm and Codex as pages, a solo game from the mode
// select (Confirm, party, Start) through the match loading screen to round 1-2, a quick finish, the end-of-game screen
// on Home and the game in the Profile's match history. Saves screenshots.
// Run: node tests/client.cjs <path-to-playwright> <outDir> [port]
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = Number(process.argv[4] || process.env.PORT || 8162);

const fails = [];
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails.push(what); };
const SIZES = [[1280, 720], [1200, 760], [1366, 768], [1600, 900], [1920, 1080], [1024, 700], [960, 640], [768, 1024], [390, 844]];
const PAGES = ['home', 'play', 'collection', 'loot', 'store', 'farm', 'codex', 'profile', 'settings'];

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true, stdio: 'ignore' });
  await new Promise((r) => setTimeout(r, 600));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1600, height: 900 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(m.text()); });
  page.on('response', (r) => { if (r.status() >= 400 && !r.url().includes('fonts.g')) errors.push(`${r.status()} ${r.url()}`); });
  const shot = (n) => page.screenshot({ path: path.join(out, n) });
  const ev = (fn, arg) => page.evaluate(fn, arg);
  const cur = () => ev(() => window.DT.Client.page());
  const nav = async (p) => {
    const sel = { profile: '#cnProfile', settings: '#cnSettings' }[p] || `#cnav .cn-tab[data-page="${p}"]`;
    await page.click(sel);
    await page.waitForTimeout(p === 'farm' ? 1500 : 450);
  };
  // sideways overflow, things past the right edge, and text cut off without an ellipsis
  const layout = () => ev(() => {
    const bad = [];
    if (document.documentElement.scrollWidth > innerWidth + 1) bad.push(`page scrolls sideways (${document.documentElement.scrollWidth} > ${innerWidth})`);
    const scope = document.querySelectorAll('#cnav, #overlay:not(.hidden), #dtStore:not(.hidden), #cxFull:not(.hidden), #dtFarm:not(.hidden)');
    for (const root of scope) {
      for (const el of root.querySelectorAll('*')) {
        if (!el.offsetParent || el.closest('svg, canvas, .hero-art')) continue;
        if (el.parentElement && el.parentElement.closest('.ctabs, .dts-tabs, .cx-tabs, .cx-sets, .dte-pick, .setpick-row, .cn-tabs, .hero-thumbs')) continue; // tab strips scroll sideways on purpose
        const r = el.getBoundingClientRect();
        if (!r.width || !r.height) continue;
        // inside a scrolling list, only the list itself has to fit
        const scroller = el.parentElement && el.parentElement.closest('.cgrid, .hist-list, .bits-list, .ctabs, .dte-pick, .dts-body, .cx-body, .df-card, .credits, .dts-tabs, .cx-tabs, .cx-sets, .df-chips, .df-scene, .mplist, .av-grid, .cpage, #lobbyPanel, .play-solo-body, .loot-inv, .res-col, .article, .events, .prof-left');
        const box = scroller ? scroller.getBoundingClientRect() : { left: 0, right: innerWidth };
        if (r.right > Math.min(innerWidth, box.right) + 1.5 && getComputedStyle(el).position !== 'fixed') bad.push(`${el.tagName.toLowerCase()}.${String(el.className).split(' ')[0]} past the edge (${Math.round(r.right)})`);
        const cs = getComputedStyle(el);
        const ownText = [...el.childNodes].some((n) => n.nodeType === 3 && n.textContent.trim());
        if (ownText && el.scrollWidth > el.clientWidth + 1 && cs.textOverflow !== 'ellipsis' && /hidden|clip/.test(cs.overflowX) && !/INPUT|TEXTAREA|SELECT/.test(el.tagName))
          bad.push(`${el.tagName.toLowerCase()}.${String(el.className).split(' ')[0]} text clipped "${el.textContent.trim().slice(0, 30)}"`);
      }
    }
    return [...new Set(bad)].slice(0, 8);
  });

  try {
    // record what the boot screen shows from the first paint on
    await page.addInitScript(() => {
      window.__boot = { steps: [], bar: [] };
      document.addEventListener('DOMContentLoaded', () => {
        const el = document.getElementById('boot');
        if (!el) return;
        new MutationObserver(() => {
          const s = el.querySelector('.boot-step'), b = el.querySelector('.boot-bar i');
          if (s && window.__boot.steps[window.__boot.steps.length - 1] !== s.textContent) window.__boot.steps.push(s.textContent);
          if (b) window.__boot.bar.push(b.style.width);
        }).observe(el, { subtree: true, childList: true, characterData: true, attributes: true });
      });
    });
    await page.goto(`http://localhost:${PORT}/?bits=5000`);
    await page.waitForSelector('#boot', { state: 'detached', timeout: 15000 });
    const boot = await ev(() => window.__boot);
    check(boot.steps.length >= 1 && boot.bar.includes('100%'), `boot screen shows loading steps and fills its bar (${boot.steps.join(' > ')})`);
    await page.waitForTimeout(800);
    check(await ev(() => !!window.DT.Client && document.body.classList.contains('client-on')), 'client shell is up after boot');
    check(await page.locator('#startBtn').isVisible() && await page.locator('#mpBtn').isVisible(), 'Play solo and Multiplayer on Home');
    // League-style chrome: PLAY button, social column with its groups, Home hero with art slides
    const chrome = await ev(() => ({
      play: !!document.querySelector('#cnPlay .cn-mark'),
      social: getComputedStyle(document.getElementById('csocial')).display !== 'none',
      groups: [...document.querySelectorAll('#csBody .cs-sh')].map((b) => b.textContent.replace(/\s*\(\d+\)/, '').trim()),
      slides: document.querySelectorAll('#homeHero .hero-art').length,
      art: getComputedStyle(document.querySelector('#homeHero .hero-art.on')).backgroundImage,
      ring: !!document.querySelector('#cnProfile .cn-ring .rg-fg') && document.querySelector('#cnProfile .cn-lv').textContent,
    }));
    check(chrome.play && chrome.social && ['Online', 'In Game'].every((g) => chrome.groups.includes(g)), `PLAY button and social column with ${chrome.groups.join(' / ')}`);
    check(chrome.slides >= 4 && /assets\/client\/.+\.webp/.test(chrome.art) && chrome.ring, `hero carousel with ${chrome.slides} art slides, profile level ring (level ${chrome.ring})`);
    const artOk = await ev(() => new Promise((res) => { const i = new Image(); i.onload = () => res(i.naturalWidth > 0); i.onerror = () => res(false); i.src = 'assets/client/hero-arena.webp'; }));
    check(artOk, 'client art loads from assets/client/');
    for (const t of ['patch', 'events', 'overview']) {
      await page.click(`#homeTabs [data-htab="${t}"]`);
      await page.waitForTimeout(250);
      const id = { patch: 'homePatchNotes', events: 'homeEvents', overview: 'homeOverview' }[t];
      check(await page.locator(`#${id}`).isVisible(), `Home tab ${t} shows #${id}`);
    }
    await page.click('#cnMin');
    await page.waitForTimeout(200);
    const off = await ev(() => getComputedStyle(document.getElementById('csocial')).display === 'none' && getComputedStyle(document.getElementById('overlay')).paddingRight === '0px');
    await page.click('#cnMin');
    await page.waitForTimeout(200);
    check(off && await ev(() => getComputedStyle(document.getElementById('csocial')).display !== 'none'), 'the minimize button hides and shows the friends list');
    check(await ev(() => document.querySelector('link[rel=icon]') && document.querySelector('.cn-mark').naturalWidth > 0), 'favicon linked and the logo mark loads');
    // kinds nobody has filled yet get stand-in items so the Store has something to buy
    await ev(() => {
      const C = window.DT.Cosmetics;
      for (const k of C.KINDS) if (C.list(k).filter((x) => !x.free).length < 3) C.register(k, ['common', 'rare'].map((r, i) => ({ id: `test_${k}_${r}`, name: `Test ${k} ${r}`, price: 150 + i * 150, rarity: r, icon: '*' })));
    });
    await shot('c0-home.png');

    // ---- every nav page ----
    for (const p of PAGES) {
      await nav(p);
      const st = await ev((x) => ({
        page: window.DT.Client.page(),
        open: { store: window.DT.Shop.isOpen(), farm: window.DT.FarmUI.isOpen(), codex: window.DT.Codex.fullOpen() }[x],
        visible: !!document.querySelector(`#pg${x[0].toUpperCase()}${x.slice(1)}.on`),
        tab: (document.querySelector('#cnav .on[data-page]') || {}).dataset ? document.querySelector('#cnav .on[data-page]').dataset.page : null,
      }), p);
      const ok = st.page === p && st.tab === p && (['store', 'farm', 'codex'].includes(p) ? st.open : st.visible);
      check(ok, `nav ${p}: page ${st.page}, highlighted ${st.tab}`);
      await shot(`c1-${p}.png`);
    }

    // ---- layout at every size ----
    for (const [w, h] of SIZES) {
      await page.setViewportSize({ width: w, height: h });
      const bad = [];
      for (const p of PAGES) {
        await nav(p);
        await page.waitForTimeout(250);
        for (const b of await layout()) bad.push(`${p}: ${b}`);
      }
      check(bad.length === 0, `no overflow or clipped text at ${w}x${h} ${bad.slice(0, 6).join(' | ')}`);
    }
    await page.setViewportSize({ width: 1600, height: 900 });
    await nav('home');

    // ---- Esc goes back ----
    await nav('collection');
    await nav('profile');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    const b1 = await cur();
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    const b2 = await cur();
    check(b1 === 'collection' && b2 === 'home', `Esc: profile -> ${b1} -> ${b2}`);
    await nav('play');
    await nav('store');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(400);
    check(!(await ev(() => window.DT.Shop.isOpen())) && (await cur()) === 'play', `Esc closes the Store and returns to Play (${await cur()})`);
    await page.keyboard.press('Tab');
    check(await ev(() => document.activeElement && document.activeElement !== document.body), 'Tab moves focus to a control');

    // ---- buy in the Store, see it in the Collection, equip it ----
    await nav('store');
    await page.waitForTimeout(500);
    const kinds = ['board', 'killscreen', 'dance', 'legend'];
    let target = null;
    for (const k of kinds) {
      await page.click(`#dtStore [data-tab="${k}"]`);
      await page.waitForTimeout(300);
      const btn = page.locator('.dts-body > section:not(.dts-feat) .dts-card [data-buy]:not(.poor)').first();
      if (await btn.count()) { target = await btn.evaluate((b) => ({ kind: b.closest('[data-kind]').dataset.kind, id: b.dataset.buy })); await btn.click(); break; }
    }
    check(!!target, `found something to buy (${target && `${target.kind}/${target.id}`})`);
    await page.waitForTimeout(300);
    await shot('c2-store-confirm.png');
    const b0 = await ev(() => window.DT.Wallet.bits);
    await page.click('#dtStore [data-confirm]');
    await page.waitForTimeout(400);
    const bought = await ev((t) => ({ owns: window.DT.Cosmetics.owns(t.kind, t.id), bits: window.DT.Wallet.bits, chip: document.querySelector('#cnBits b').textContent }), target);
    check(bought.owns && bought.bits < b0, `bought ${target.id} (${b0} -> ${bought.bits} Bits)`);
    check(bought.chip.replace(/\D/g, '') === String(bought.bits), `nav Bits chip shows the new balance (${bought.chip})`);
    await shot('c3-store-bought.png');
    await page.click('#dtStore [data-cancel]');
    await nav('collection');
    await page.click(`#pgCollection [data-ctab="${target.kind}"]`);
    await page.waitForTimeout(400);
    const card = page.locator(`#pgCollection .ccard[data-csel="${target.id}"]`);
    check(await card.count() === 1 && (await card.getAttribute('class')).includes('own'), 'the bought item is in the Collection as owned');
    await card.click();
    await page.waitForTimeout(400);
    await page.click(`#pgCollection [data-cequip="${target.id}"]`);
    await page.waitForTimeout(400);
    check(await ev((t) => window.DT.Cosmetics.equipped(t.kind) === t.id, target), `equipped ${target.id} from the Collection`);
    check(await page.locator(`#pgCollection .ccard[data-csel="${target.id}"].eq`).count() === 1, 'its card shows Equipped');
    // search and filters
    await page.fill('#colSearch', 'zzzz-nothing');
    await page.waitForTimeout(400);
    check(await page.locator('#pgCollection .ccard').count() === 0 && await page.locator('#pgCollection .cgrid .empty').count() === 1, 'search with no match shows the empty state');
    await page.fill('#colSearch', '');
    await page.click('#pgCollection [data-cown="owned"]');
    await page.waitForTimeout(300);
    check(await page.locator('#pgCollection .ccard.locked').count() === 0, 'Owned filter hides items you do not own');
    await page.click('#pgCollection [data-cown="all"]');
    // emote wheel slot
    await page.click('#pgCollection [data-ctab="emote"]');
    await page.waitForTimeout(300);
    const em = await ev(() => { const C = window.DT.Cosmetics; const it = C.list('emote').find((x) => C.owns('emote', x.id)); return it && it.id; });
    await page.click(`#pgCollection .ccard[data-csel="${em}"]`);
    await page.waitForTimeout(300);
    await page.click('#pgCollection [data-cslot="4"]');
    await page.waitForTimeout(300);
    check(await ev((id) => window.DT.Cosmetics.equipped('emote')[4] === id, em), `emote ${em} put in wheel slot 5`);
    await page.click('#pgCollection [data-ctab="legend"]');
    await page.waitForTimeout(900);
    await shot('c4-collection-legend.png');

    // ---- Digi-Farm and back ----
    await nav('farm');
    await page.waitForTimeout(1500);
    check(await page.locator('#dtFarm:not(.hidden)').count() === 1 && await page.locator('#cnav').isVisible(), 'Digi-Farm opens as a page under the nav');
    await shot('c5-farm.png');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(400);
    check(await page.locator('#dtFarm.hidden').count() === 1 && (await cur()) === 'collection', `Esc leaves the farm and returns to the Collection (${await cur()})`);

    // ---- Codex ----
    await nav('codex');
    check(await page.locator('#cxFull').isVisible() && await page.locator('#cxFull .cx-trait').count() > 5, 'Codex opens as a page with its traits');
    await page.click('#cnav .cn-tab[data-page="home"]');
    await page.waitForTimeout(400);
    check(!(await ev(() => window.DT.Codex.fullOpen())) && (await cur()) === 'home', 'Home from the nav closes the Codex');

    // ---- a solo game from Play ----
    await nav('play');
    check(await page.locator('#setPickSolo .set-card').count() >= 2 && await page.locator('#playConfirm').isVisible(), 'mode select shows set cards and Confirm');
    await page.click('[data-mode-pick="solo"]');
    await page.click('#playConfirm');
    await page.waitForTimeout(500);
    check(await page.locator('#playSolo').isVisible() && await page.locator('#partySlots .party-slot').count() === 8, 'Confirm opens the solo party with 8 slots');
    await page.click('#playSoloBtn');
    await page.waitForTimeout(700);
    check(await ev(() => !!window.__digi.game && document.getElementById('overlay').classList.contains('hidden') && !document.body.classList.contains('client-on')), 'Start begins a solo game and hides the shell');
    check(await ev(() => document.querySelectorAll('#cload .ld-card').length === 8 && !!document.querySelector('#cload .ld-card.me')), 'match loading screen shows all 8 Tamers');
    await page.waitForSelector('#cload', { state: 'detached', timeout: 6000 });
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(900);
    const r = await ev(() => ({ round: window.__digi.game.roundLabel, phase: window.__digi.game.phase }));
    check(r.round === '1-2' && r.phase === 'planning', `reached the first round (${r.round}, ${r.phase})`);
    await shot('c6-game.png');
    // buy something so the board has a comp to record
    for (let i = 0; i < 3; i++) { const c = page.locator(`.card[data-slot="${i}"]:not(.poor)`); if (await c.count()) await c.click(); }
    await page.waitForTimeout(1200);

    // ---- quick finish: every AI Tamer falls this round ----
    await ev(() => { const g = window.__digi.game; for (const p of g.players) if (p !== g.human) p.hp = 0; });
    await page.keyboard.press('Space');
    await page.waitForTimeout(400);
    if (await page.locator('#skipBtn:not([disabled])').count()) await page.click('#skipBtn');
    await page.waitForSelector('#overlay:not(.hidden) #homeResults .results', { timeout: 20000 });
    await page.waitForTimeout(3000);
    const res = await ev(() => ({ page: window.DT.Client.page(), place: document.querySelector('.res-num').textContent, award: !!document.querySelector('#homeResults .dts-award'), again: !!document.getElementById('againBtn') }));
    check(res.page === 'home' && res.place === '#1' && res.award && res.again, `game over returns to Home with results (${JSON.stringify(res)})`);
    await shot('c7-results.png');
    await nav('profile');
    const hist = await ev(() => ({ rows: document.querySelectorAll('#pgProfile .match').length, first: (document.querySelector('#pgProfile .match .m-place b') || {}).textContent, games: window.DT.Client.stats().games, rec: window.DT.Client.history()[0] }));
    check(hist.rows >= 1 && hist.first === '#1' && hist.games >= 1, `Profile match history records the game (${hist.rows} row, ${hist.first}, ${hist.rec && hist.rec.units.length} units)`);
    await shot('c8-profile.png');
    await nav('home');
    await page.click('#againBtn');
    await page.waitForTimeout(600);
    check(await ev(() => !!window.__digi.game && !window.__digi.game.over && document.getElementById('overlay').classList.contains('hidden')), 'Play again starts a new game');

    // ---- the save survives a reload ----
    await page.reload();
    await page.waitForSelector('#boot', { state: 'detached', timeout: 15000 });
    check(await ev(() => window.DT.Client.history().length >= 1), 'match history is saved (dt.client.v1)');
    check(errors.length === 0, `no page errors ${errors.slice(0, 5).join(' | ')}`);
  } catch (e) {
    console.error(e);
    fails.push(String(e).split('\n')[0]);
    try { await shot('c-error.png'); } catch { /* ignore */ }
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fails.length ? `\n${fails.length} FAILED` : '\nall client checks passed');
  process.exit(fails.length ? 1 : 0);
})();
