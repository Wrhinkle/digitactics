// Codex check: the in-game trait panel (key and trait-list click), counts and breakpoints against game state,
// the title-screen Codex (every trait and shop line listed, search, filters, the set switcher with each set's traits
// and rules), overflow at 1200x760, Escape. Saves screenshots.
// Run: node tests/codex.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8153;

const fails = [];
const check = (ok, what) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${what}`); if (!ok) fails.push(what); };
const same = (a, b) => a.length === b.length && [...a].sort().join() === [...b].sort().join();

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  try { browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1200, height: 760 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(m.text()); });
  const shot = (n) => page.screenshot({ path: path.join(out, n) });
  const noHScroll = () => page.evaluate(() => {
    const bad = [];
    if (document.documentElement.scrollWidth > innerWidth) bad.push(`page ${document.documentElement.scrollWidth}>${innerWidth}`);
    for (const sel of ['#cxPanel .cx-pbody', '#cxFull .cx-body', '#cxFull .cx-head', '#cxFull .cx-filters', '#cxFull .cx-mbox']) {
      const el = document.querySelector(sel);
      if (el && el.offsetParent !== null && el.scrollWidth > el.clientWidth + 1) bad.push(`${sel} ${el.scrollWidth}>${el.clientWidth}`);
    }
    return bad;
  });

  await page.goto(`http://localhost:${PORT}/`);
  await page.waitForTimeout(2000);

  // ---- title-screen Codex ----
  check(await page.locator('#codexTitleBtn').isVisible(), 'Codex button on the title screen');
  // a trait from the future with the optional fields, plus an unknown kind
  await page.evaluate(() => {
    window.DT.TRAITS.ZZTest = { name: 'Test Circuit', kind: 'origin', color: '#33ddaa', bp: [2, 4, 6], lv: [1, 2, 3], desc: 'A test trait.',
      notes: ['First note.', 'Second note.'], bpDesc: ['Custom two', 'Custom four'], fmt: (v) => `fmt ${v}` };
  });
  await page.click('#codexTitleBtn');
  await page.waitForTimeout(500);
  check(await page.locator('#cxFull').isVisible(), 'Codex opens from the title button');
  await shot('cx0-traits.png');
  const traitIds = await page.evaluate(() => ({ want: Object.keys(window.DT.TRAITS), got: [...document.querySelectorAll('#cxFull .cx-trait')].map((e) => e.dataset.cxTid) }));
  check(same(traitIds.want, traitIds.got), `Traits tab lists every DT.TRAITS key (${traitIds.got.length}/${traitIds.want.length})`);
  const ext = await page.evaluate(() => {
    const el = document.querySelector('#cxFull [data-cx-tid="ZZTest"]');
    return el && { bps: [...el.querySelectorAll('.cx-bp span')].map((s) => s.textContent), notes: el.querySelectorAll('.cx-note').length,
      kinds: [...document.querySelectorAll('#cxFull [data-kind]')].map((b) => b.dataset.kind) };
  });
  check(ext && ext.bps.join('|') === 'Custom two|Custom four|fmt 3', `bpDesc used per breakpoint, fmt() as fallback (${ext && ext.bps.join('|')})`);
  check(ext && ext.notes === 2, 'notes shown');
  check(ext && ext.kinds.includes('origin') && ext.kinds.includes('family') && ext.kinds.includes('role'), `kind filters come from the data (${ext && ext.kinds.join(',')})`);
  await page.click('#cxFull [data-kind="role"]');
  const roles = await page.evaluate(() => [...document.querySelectorAll('#cxFull .cx-trait')].map((e) => window.DT.TRAITS[e.dataset.cxTid].kind));
  check(roles.length > 0 && roles.every((k) => k === 'role'), `Roles filter shows only roles (${roles.length})`);
  await page.click('#cxFull [data-kind="all"]');
  await page.evaluate(() => { delete window.DT.TRAITS.ZZTest; });

  // search
  await page.fill('#cxFull .cx-search', 'greymon');
  await page.waitForTimeout(150);
  const hits = await page.evaluate(() => {
    const ids = [...document.querySelectorAll('#cxFull .cx-trait')].map((e) => e.dataset.cxTid);
    const want = Object.keys(window.DT.TRAITS).filter((id) => window.DT.LINES.some((l) => l.traits.includes(id) && l.forms.join(' ').toLowerCase().includes('greymon')));
    return { ids, want };
  });
  check(hits.ids.length > 0 && same(hits.ids, hits.want), `search "greymon" in Traits: ${hits.ids.join(', ')}`);
  await shot('cx1-search.png');
  await page.fill('#cxFull .cx-search', 'zzqqxx');
  await page.waitForTimeout(150);
  check((await page.locator('#cxFull .cx-trait').count()) === 0 && (await page.locator('#cxFull .cx-empty').isVisible()), 'search with no match shows the empty message');
  await page.fill('#cxFull .cx-search', '');

  // Digimon tab: every shop line and every wild Digimon
  await page.click('#cxFull [data-tab="digimon"]');
  await page.waitForTimeout(800);
  const lines = await page.evaluate(() => ({ want: window.DT.LINES.map((l) => l.id), shop: window.DT.SHOP_LINES.map((l) => l.id), got: [...document.querySelectorAll('#cxFull .cx-line')].map((e) => e.dataset.cxLine) }));
  check(lines.shop.every((id) => lines.got.includes(id)), `Digimon tab lists every shop line (${lines.shop.filter((id) => lines.got.includes(id)).length}/${lines.shop.length})`);
  check(same(lines.got, lines.want), `Digimon tab lists all ${lines.want.length} lines including wild ones`);
  await shot('cx2-digimon.png');
  check((await noHScroll()).length === 0, `no horizontal overflow on Digimon tab at 1200x760 ${(await noHScroll()).join(' ')}`);
  await page.fill('#cxFull .cx-search', 'Pepper Breath');
  await page.waitForTimeout(150);
  const ab = await page.evaluate(() => [...document.querySelectorAll('#cxFull .cx-line')].map((e) => e.dataset.cxLine));
  check(ab.length === 1 && ab[0] === 'agumon', `search by ability finds Agumon (${ab.join(',')})`);
  await page.click('#cxFull .cx-line[data-cx-line="agumon"]');
  await page.waitForTimeout(300);
  const card = await page.evaluate(() => {
    const m = document.querySelector('#cxFull .cx-modal:not(.hidden)');
    return m && { forms: [...m.querySelectorAll('.cx-fname b')].map((b) => b.textContent), abil: m.querySelectorAll('.cx-abil').length, text: m.textContent };
  });
  check(card && card.forms.join() === 'Agumon,Greymon,MetalGreymon' && card.abil === 3, `unit card shows 3 forms with abilities (${card && card.forms.join()})`);
  check(card && card.text.includes('Vaccine') && card.text.includes("Dragon's Roar") && card.text.includes('HP'), 'unit card shows attribute, traits and stats');
  await shot('cx3-unit.png');
  await page.keyboard.press('Escape');
  check(await page.locator('#cxFull .cx-modal').isHidden() && await page.locator('#cxFull').isVisible(), 'Escape closes the unit card first');
  await page.fill('#cxFull .cx-search', '');

  // Items, augments, wisps, rules
  await page.click('#cxFull [data-tab="items"]');
  await page.waitForTimeout(300);
  const items = await page.evaluate(() => ({ want: Object.keys(window.DT.ITEMS), got: [...document.querySelectorAll('#cxFull .cx-card.item')].map((e) => e.dataset.cxItem),
    grid: document.querySelectorAll('#cxFull .cx-rgrid td button').length, recipes: Object.keys(window.DT.RECIPE).length }));
  check(same(items.got, items.want), `Items tab lists every item (${items.got.length}/${items.want.length})`);
  const n = await page.evaluate(() => window.DT.COMPONENT_IDS.length);
  check(items.grid === n * n, `recipe grid has ${items.grid} filled cells for ${n} components`);
  await shot('cx4-items.png');
  check((await noHScroll()).length === 0, `no horizontal overflow on Items tab ${(await noHScroll()).join(' ')}`);
  await page.click('#cxFull [data-tab="augments"]');
  check((await page.locator('#cxFull [data-cx-aug]').count()) === (await page.evaluate(() => window.DT.AUGMENTS.length)), 'Augments tab lists every augment');
  await shot('cx5-augments.png');
  await page.click('#cxFull [data-tab="wisps"]');
  check((await page.locator('#cxFull [data-cx-wisp]').count()) === (await page.evaluate(() => window.DT.WISPS.length)), 'Wisps tab lists every Data Wisp');
  await page.click('#cxFull [data-tab="rules"]');
  await page.waitForTimeout(200);
  const rules = await page.evaluate(() => document.querySelector('#cxFull .cx-body').textContent);
  check(/no interest/i.test(rules) && rules.includes('Vaccine') && rules.includes('Carousel'), 'Rules tab covers economy, attributes and rounds');
  await shot('cx6-rules.png');
  check((await noHScroll()).length === 0, `no horizontal overflow on Rules tab ${(await noHScroll()).join(' ')}`);
  // ---- set switcher: every playable set with its own traits and rules; the active set is left alone ----
  await page.fill('#cxFull .cx-search', '');
  const sw = await page.evaluate(async () => {
    const wait = (ms) => new Promise((r) => setTimeout(r, ms));
    const want = window.DT.Sets.pickable().map((o) => o.id);
    const tabs = [...document.querySelectorAll('#cxFull .cx-sets [data-cx-set]')].map((b) => b.dataset.cxSet);
    const active = window.DT.SET_ID;
    const res = [];
    for (const id of want) {
      document.querySelector(`#cxFull [data-cx-set="${id}"]`).click();
      document.querySelector('#cxFull [data-tab="traits"]').click();
      await wait(120);
      const shown = [...document.querySelectorAll('#cxFull .cx-tgrid [data-cx-tid]')].map((el) => el.dataset.cxTid);
      const data = Object.keys(window.DT.Sets.get(id).TRAITS);
      const title = document.querySelector('#cxFull .cx-head h2').textContent;
      const tab = document.querySelector(`#cxFull [data-cx-set="${id}"]`);
      document.querySelector('#cxFull [data-tab="rules"]').click();
      await wait(120);
      const rules = document.querySelector('#cxFull .cx-body').textContent;
      res.push({ id, shown: shown.length, same: shown.length === data.length && data.every((t) => shown.includes(t)), data: data.length, title, on: tab.classList.contains('on'), rules, activeAfter: window.DT.SET_ID });
    }
    document.querySelector(`#cxFull [data-cx-set="${active}"]`).click();
    const names = Object.fromEntries(window.DT.Sets.pickable().map((o) => [o.id, o.name]));
    return { want, tabs, active, res, names };
  });
  check(['faction-wars', 'dragonlands', 'autochess'].every((id) => sw.tabs.includes(id)) && sw.want.every((id) => sw.tabs.includes(id)), `set switcher lists every playable set (${sw.tabs.join(', ')})`);
  for (const r of sw.res) {
    check(r.same && r.on && r.title.includes(sw.names[r.id]), `${r.id}: ${r.shown} traits shown, ${r.data} in its data, title "${r.title}"`);
    check(r.activeAfter === sw.active, `${r.id}: browsing leaves the active set on ${sw.active} (${r.activeAfter})`);
  }
  const rulesOf = (id) => (sw.res.find((r) => r.id === id) || {}).rules || '';
  check(rulesOf('dragonlands').includes('Treasure Dragon') && rulesOf('dragonlands').includes('Golden Armor') && !rulesOf('dragonlands').includes('Data Wisp'), 'Dragonlands rules: its own text (Dragons, Treasure Dragon, Golden Armor), no Data Wisps');
  check(rulesOf('autochess').includes('Creep rounds') && rulesOf('autochess').includes('Druids') && rulesOf('autochess') !== rulesOf('dragonlands'), 'Auto Chess rules: its own text (creep rounds, 2-copy Druids)');
  check(rulesOf('faction-wars').includes('Data Wisps') && /no interest/i.test(rulesOf('faction-wars')), 'Faction Wars rules: Hyper Roll and Data Wisps');
  await page.click('#cxFull [data-cx-set="dragonlands"]');
  await page.click('#cxFull [data-tab="traits"]');
  await page.waitForTimeout(200);
  await shot('cx6b-switcher.png');
  await page.click(`#cxFull [data-cx-set="${sw.active}"]`);
  await page.keyboard.press('Escape');
  await page.waitForTimeout(150);
  check(await page.locator('#cxFull').isHidden(), 'Escape closes the Codex');
  await page.keyboard.press('c');
  await page.waitForTimeout(200);
  check(await page.locator('#cxFull').isVisible(), 'C opens the full Codex on the title screen');
  await page.keyboard.press('c');
  await page.waitForTimeout(150);
  check(await page.locator('#cxFull').isHidden(), 'C closes it again');

  // ---- in-game side panel ----
  await page.click('#startBtn');
  await page.waitForTimeout(500);
  await page.click('#timerBtn');
  await page.locator('.choice[data-carousel]:not(.taken)').first().click();
  await page.click('#carouselGo');
  await page.waitForTimeout(600);
  // a known board: three lines sharing traits, plus one bench unit
  await page.evaluate(() => {
    const g = window.__digi.game, p = g.human;
    for (const u of p.allUnits()) g.returnToPool(u);
    p.board = {}; p.bench = p.bench.map(() => null);
    p.level = 5;
    const put = (line, stage, key) => { g.takeFromPool(line, Math.pow(3, stage)); p.board[key] = g.newUnit(line, stage); };
    put('agumon', 1, '3,4'); put('guilmon', 0, '2,4'); put('gabumon', 0, '4,4'); put('tentomon', 0, '3,7');
    g.takeFromPool('palmon'); p.bench[0] = g.newUnit('palmon', 0);
    window.DT.UI.refresh();
  });
  await page.waitForTimeout(400);
  check(await page.locator('#codexBtn').isVisible(), 'Codex button in the top bar');
  await page.keyboard.press('c');
  await page.waitForTimeout(500);
  check(await page.locator('#cxPanel').isVisible(), 'C opens the side panel in a game');
  const st = await page.evaluate(() => {
    const p = window.__digi.game.human;
    const counts = window.DT.countTraits(p.boardUnits(), p.augs);
    const rows = [...document.querySelectorAll('#cxPanel .cx-trait')].map((e) => ({
      id: e.dataset.cxTid, n: Number(e.querySelector('.cx-count').firstChild.textContent),
      on: e.querySelectorAll('.cx-bp.on').length, bps: [...e.querySelectorAll('.cx-bp b')].map((b) => Number(b.textContent)),
      units: [...e.querySelectorAll('.cx-u')].map((u) => u.dataset.cxLine), marks: [...e.querySelectorAll('.cx-u .cx-mark')].map((m) => m.className),
    }));
    const want = Object.keys(counts).map((id) => ({ id, n: counts[id], lv: window.DT.traitLevel(id, counts[id]), bp: window.DT.TRAITS[id].bp,
      lines: window.DT.SHOP_LINES.filter((l) => l.traits.includes(id)).map((l) => l.id) }));
    return { rows, want };
  });
  check(same(st.rows.map((r) => r.id), st.want.map((w) => w.id)), `panel lists the board's traits (${st.rows.map((r) => r.id).join(', ')})`);
  let okCounts = true, okBp = true, okUnits = true;
  for (const w of st.want) {
    const r = st.rows.find((x) => x.id === w.id);
    if (!r || r.n !== w.n) okCounts = false;
    if (!r || r.on !== w.lv || r.bps.join() !== w.bp.join()) okBp = false;
    if (!r || !same(r.units, w.lines)) okUnits = false;
  }
  check(okCounts, 'trait counts match DT.countTraits');
  check(okBp, 'breakpoints and active breakpoints match the trait data');
  check(okUnits, 'each entry shows every shop Digimon with the trait');
  const dr = st.rows.find((r) => r.id === 'DR');
  check(dr && dr.n === 2 && dr.on === 1, `Dragon's Roar shows 2 and its first breakpoint active (${dr && dr.n}, ${dr && dr.on})`);
  const jt = await page.evaluate(() => {
    const e = document.querySelector('#cxPanel [data-cx-tid="JT"]');
    return e && { board: !!e.querySelector('[data-cx-line="tentomon"] .cx-mark.board'), bench: !!e.querySelector('[data-cx-line="palmon"] .cx-mark.own') };
  });
  check(jt && jt.board && jt.bench, 'on-board and bench marks on portraits');
  await shot('cx7-panel.png');
  check((await noHScroll()).length === 0, `no horizontal overflow with the panel at 1200x760 ${(await noHScroll()).join(' ')}`);
  const fit = await page.evaluate(() => {
    const r = (s) => document.querySelector(s).getBoundingClientRect();
    const p = r('#cxPanel'), top = r('#topbar'), shop = r('#shopbar'), right = r('#right');
    return { p: [p.left, p.top, p.right, p.bottom], ok: p.top >= top.bottom - 1 && p.bottom <= shop.top + 1 && p.right <= right.left && p.left >= 0 && p.width <= 300 };
  });
  check(fit.ok, `panel fits between the top bar, shop and Tamers list (${fit.p.map(Math.round).join(',')})`);
  // all traits in this game's pool
  await page.click('#cxPanel [data-scope="pool"]');
  await page.waitForTimeout(200);
  const pool = await page.evaluate(() => {
    const g = window.__digi.game;
    const want = new Set();
    for (const id of Object.keys(g.pool)) for (const t of window.DT.LINE[id].traits) want.add(t);
    return { want: [...want], got: [...document.querySelectorAll('#cxPanel .cx-trait')].map((e) => e.dataset.cxTid) };
  });
  check(same(pool.got, pool.want), `"All in game" lists every trait in the shop pool (${pool.got.length})`);
  // unit card from a portrait
  await page.click('#cxPanel [data-cx-tid="DR"] .cx-u[data-cx-line="agumon"]');
  await page.waitForTimeout(300);
  const uc = await page.evaluate(() => document.querySelector('#cxPanel .cx-pbody').textContent);
  check(uc.includes('Greymon') && uc.includes('You have') && uc.includes('left in the pool'), 'clicking a portrait shows the unit card with ownership and pool');
  await shot('cx8-panel-unit.png');
  check((await noHScroll()).length === 0, `no horizontal overflow on the panel unit card ${(await noHScroll()).join(' ')}`);
  await page.keyboard.press('Escape');
  await page.waitForTimeout(150);
  check(await page.locator('#cxPanel .cx-trait').count() > 0, 'Escape goes back from the unit card');
  await page.click('#cxPanel [data-ptab="rules"]');
  await page.waitForTimeout(200);
  await page.evaluate(() => document.querySelectorAll('#cxPanel details').forEach((d) => { d.open = true; }));
  await page.evaluate(() => { const b = document.querySelector('#cxPanel .cx-pbody'); b.scrollTop = 520; });
  await shot('cx9-panel-rules.png');
  check((await noHScroll()).length === 0, `no horizontal overflow on the panel rules ${(await noHScroll()).join(' ')}`);
  await page.keyboard.press('Escape');
  await page.waitForTimeout(150);
  check(await page.locator('#cxPanel').isHidden(), 'Escape closes the side panel');

  // clicking a trait in the HUD trait list opens the panel at that trait
  await page.click('#traitList [data-trait="NSp"]');
  await page.waitForTimeout(400);
  const focus = await page.evaluate(() => {
    const el = document.querySelector('#cxPanel [data-cx-tid="NSp"]');
    const body = document.querySelector('#cxPanel .cx-pbody');
    if (!el) return null;
    const a = el.getBoundingClientRect(), b = body.getBoundingClientRect();
    return { visible: a.top >= b.top - 2 && a.top < b.bottom, flash: el.classList.contains('flash') };
  });
  check(await page.locator('#cxPanel').isVisible() && focus && focus.visible, 'clicking a HUD trait opens the panel at that trait');
  await shot('cx10-panel-click.png');
  await page.keyboard.press('c');
  await page.waitForTimeout(150);
  check(await page.locator('#cxPanel').isHidden(), 'C closes the side panel');
  // the game still plays with the codex around
  await page.keyboard.press('c');
  await page.keyboard.press('Space');
  await page.waitForTimeout(600);
  check(await page.evaluate(() => window.__digi.game.phase !== 'planning'), 'Space still starts combat with the panel open');

  check(errors.length === 0, `no page errors${errors.length ? ': ' + errors.slice(0, 3).join(' | ') : ''}`);
  await browser.close();
  server.kill();
  console.log(fails.length ? `\n${fails.length} FAILED` : '\nall passed');
  process.exit(fails.length ? 1 : 0);
})().catch((e) => { console.error(e); process.exit(1); });
