// Little Legend digivolution check: egg-only legends, duplicates as Digi-Data, level-ups, branch odds, deterministic
// digivolve across reloads, revert, the evolved form on the board and in the loadout, and no overflow at 1200x760.
// Run: node tests/legendevo.cjs <path-to-playwright> <outDir>
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8156;

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
  const L = 'botamon';

  try {
    await page.goto(`http://localhost:${PORT}/?bits=20000`);
    await page.waitForTimeout(2000);

    // ---- egg-only legends ----
    const shopInfo = await ev(() => {
      const C = window.DT.Cosmetics;
      const legends = C.list('legend');
      return {
        eggOnly: legends.filter((x) => x.eggOnly).map((x) => ({ id: x.id, name: x.name, rarity: x.rarity })),
        buyable: legends.filter((x) => !x.eggOnly && !x.free).map((x) => x.rarity),
        free: legends.filter((x) => x.free).length,
        featuredEggOnly: window.DT.Shop.featured().filter((f) => f.item.eggOnly).length,
        rarities: [...new Set(legends.map((x) => x.rarity))],
      };
    });
    check(shopInfo.free === 3, `3 free legends (${shopInfo.free})`);
    check(shopInfo.buyable.every((r) => r === 'common' || r === 'uncommon'), `directly buyable legends are common/uncommon only (${shopInfo.buyable.join(',')})`);
    check(shopInfo.eggOnly.length >= 5 && shopInfo.eggOnly.every((x) => ['rare', 'epic', 'legendary'].includes(x.rarity)), `egg-only legends are rare+ (${shopInfo.eggOnly.map((x) => x.id).join(',')})`);
    check(['common', 'uncommon', 'rare', 'epic', 'legendary'].every((r) => shopInfo.rarities.includes(r)), `legends span 5 rarities (${shopInfo.rarities.join(',')})`);
    check(shopInfo.featuredEggOnly === 0, 'no egg-only item in the featured row');
    await page.click('#storeTitleBtn');
    await page.waitForSelector('#dtStore:not(.hidden) .dts-card');
    await page.click('#dtStore [data-tab="legend"]');
    await page.waitForTimeout(500);
    const cardState = await ev((id) => {
      const card = document.querySelector(`#dtStore .dts-card[data-kind="legend"][data-id="${id}"]`);
      return { badge: !!(card && card.querySelector('.dts-eobadge')), buy: !!document.querySelector(`#dtStore [data-buy="${id}"]`), link: !!(card && card.querySelector('[data-gostore="eggs"]')) };
    }, L);
    check(cardState.badge && !cardState.buy && cardState.link, `egg-only card shows "Egg only", no price, links to eggs (${JSON.stringify(cardState)})`);
    await shot('le0-store-legends.png');
    // a direct buy attempt is refused even with Bits in hand
    const before = await ev(() => window.DT.Wallet.bits);
    await ev((id) => { const b = document.createElement('button'); b.dataset.buy = id; const wrap = document.querySelector('#dtStore .dts-card[data-kind="legend"] .dts-act'); wrap.appendChild(b); b.click(); b.remove(); }, L);
    await page.waitForTimeout(200);
    if (await page.locator('#dtStore [data-confirm]').count()) await page.click('#dtStore [data-confirm]');
    await page.waitForTimeout(200);
    const after = await ev((id) => ({ bits: window.DT.Wallet.bits, owns: window.DT.Cosmetics.owns('legend', id) }), L);
    check(after.bits === before && !after.owns, `buying an egg-only legend directly is impossible (bits ${before} -> ${after.bits}, owned ${after.owns})`);
    await page.click(`#dtStore .dts-card[data-id="${L}"] [data-gostore="eggs"]`);
    await page.waitForTimeout(300);
    const eggList = await ev(() => [...document.querySelectorAll('.dts-egg-card')].map((c) => c.querySelector('.dts-name').textContent + ': ' + [...c.querySelectorAll('.dts-eolist span')].map((s) => s.textContent).join(', ')));
    console.log(eggList.join('\n'));
    check(eggList.some((t) => shopInfo.eggOnly.every((x) => t.includes(x.name))), 'egg odds list every egg-only legend');
    await shot('le1-eggs.png');
    const rolled = await ev(() => {
      const seen = {};
      for (let i = 0; i < 4000; i++) { const r = window.DT.Shop._rollEgg('legend'); if (r) seen[r.id] = (seen[r.id] || 0) + 1; }
      return seen;
    });
    check(shopInfo.eggOnly.every((x) => rolled[x.id] > 0), `the Little Legend Egg rolls each egg-only legend (${shopInfo.eggOnly.map((x) => `${x.id} ${rolled[x.id] || 0}`).join(', ')})`);

    // ---- a duplicate from an egg becomes Digi-Data ----
    await ev(() => { const C = window.DT.Cosmetics; for (const x of C.list('legend')) C.grant('legend', x.id); });
    const xp0 = await ev(() => Object.fromEntries(window.DT.Cosmetics.list('legend').map((x) => [x.id, window.DT.LegendEvo.info(x.id).xp])));
    await page.click('#dtStore [data-egg="legend"]');
    await page.click('#dtStore [data-confirm]');
    await page.waitForTimeout(400);
    await page.click('#dtStore .dts-hatch');
    await page.waitForTimeout(500);
    const hatchText = await page.locator('#dtStore .dts-reveal').innerText();
    const xp1 = await ev(() => Object.fromEntries(window.DT.Cosmetics.list('legend').map((x) => [x.id, window.DT.LegendEvo.info(x.id).xp])));
    const gained = Object.keys(xp1).filter((k) => xp1[k] > xp0[k]);
    check(/Digi-Data/.test(hatchText) && gained.length === 1, `duplicate legend gives Digi-Data, not Bits (${gained.join(',')}: ${hatchText.replace(/\s+/g, ' ').slice(0, 90)})`);
    await shot('le2-dup.png');
    await page.click('#dtStore [data-cancel]');

    // ---- Locker -> Digivolve panel ----
    await page.click('#dtStore [data-mode="locker"]');
    await page.click('#dtStore [data-tab="legend"]');
    await page.waitForTimeout(400);
    check(await page.locator('#dtStore .dte-bsec').count() === 1, 'Locker shows the equipped legend banner');
    await shot('le3-locker.png');
    await page.click(`#dtStore .dts-card[data-id="${L}"] [data-evo-open]`);
    await page.waitForSelector('#dtEvo:not(.hidden)');
    await page.waitForTimeout(1200);
    await shot('le4-panel-lv1.png');
    // level up with Bits to the first threshold
    for (let n = 0; n < 6; n++) {
      const lv = await ev((id) => window.DT.LegendEvo.info(id).level, L);
      if (lv >= 3) break;
      await page.click('#dtEvo [data-evo-level]');
      await page.waitForTimeout(150);
    }
    await page.click('#dtEvo [data-evo-feed]');
    await page.click('#dtEvo [data-evo-feed]');
    await page.waitForTimeout(300);
    const st = await ev((id) => window.DT.LegendEvo.info(id), L);
    check(st.level >= 3 && st.canDigivolve, `level ${st.level} reached with Bits, can digivolve`);
    check(st.care.food === 2, `feeding raised food care to ${st.care.food}`);
    const domPct = await ev(() => [...document.querySelectorAll('#dtEvo .dte-orow .pc')].map((e) => parseInt(e.textContent, 10)));
    const sum = (a) => a.reduce((x, y) => x + y, 0);
    check(sum(st.odds.map((o) => o.pct)) === 100 && sum(domPct) === 100 && domPct.length === st.odds.length, `branch odds sum to 100 (${st.odds.map((o) => `${o.form} ${o.pct}%`).join(', ')})`);
    // odds sum to 100 for every legend at every form in its tree
    // tree shape: every legend reaches Rookie, Champion and Ultimate rows; every Rookie and Champion has 2-4 branches,
    // all one level up; every form has a model; In-Training forms come before Rookies
    const allSums = await ev(async () => {
      const E = window.DT.LegendEvo, bad = [];
      const index = await (await fetch('models/index.json')).json();
      const order = { Fresh: 0, 'In-Training': 1, Rookie: 2, Champion: 3, Ultimate: 4 };
      for (const x of window.DT.Cosmetics.list('legend')) {
        const t = E.tree(x.id);
        for (const need of ['Rookie', 'Champion', 'Ultimate']) if (!t.levels.includes(need)) bad.push(`${x.id} has no ${need} row (${t.levels})`);
        for (const [ti, row] of t.tiers.entries()) for (const f of row) {
          const fi = E.formInfo(f);
          if (!fi || !fi.name) { bad.push(`${x.id}/${f} no form info`); continue; }
          if (ti && fi.level !== t.levels[ti]) bad.push(`${x.id}/${f} in the ${t.levels[ti]} row`);
          const kids = (E.EVO[f] || '').split(' ').filter(Boolean).map((k) => k.split(':')[0]);
          if ((fi.level === 'Rookie' || fi.level === 'Champion') && (kids.length < 2 || kids.length > 4)) bad.push(`${x.id}/${f} has ${kids.length} branches`);
          for (const k of kids) { const ki = E.formInfo(k); if (!ki || (order[ki.level] !== order[fi.level] + 1 && !(fi.level === 'Fresh' && ki.level === 'Rookie'))) bad.push(`${f}->${k} skips a level`); }
          if (fi.model && !index[fi.model]) bad.push(`${f} model ${fi.model} is not in models/index.json`);
          if (fi.legend && !window.DT.Cosmetics.get('legend', fi.legend)) bad.push(`${f} legend model ${fi.legend} missing`);
        }
        const o = E.odds(x.id);
        if (o.length && o.reduce((n, w) => n + w.pct, 0) !== 100) bad.push(`${x.id} odds sum`);
      }
      return bad;
    });
    check(allSums.length === 0, `every legend reaches Rookie, Champion and Ultimate, with 2-4 branches at Rookie and Champion ${allSums.slice(0, 8).join('; ')}`);
    // forms a save may hold from the first tree version still belong to their legend
    const oldForms = { botamon: ['agumon', 'guilmon', 'veemon', 'greymon', 'tyrannomon', 'ogremon', 'growlmon', 'exveemon', 'metalgreymon', 'skullgreymon', 'megadramon', 'andromon', 'etemon', 'wargrowlmon', 'paildramon'],
      tsunomon: ['gabumon', 'elecmon', 'gizamon', 'ogremon', 'leomon', 'gaogamon', 'andromon'], punimon: ['ogremon', 'betamon', 'gesomon'], yokomon: ['kuwagamon', 'dinobeemon'],
      tokomon: ['goblimon', 'ogremon', 'etemon'], kuramon: ['candlemon', 'mistymon', 'myotismon'] };
    const lost = await ev((m) => Object.entries(m).flatMap(([l, fs]) => fs.filter((f) => !window.DT.LegendEvo.reach(l).includes(f)).map((f) => `${l}/${f}`)), oldForms);
    check(lost.length === 0, `old saved forms are still on their trees ${lost.join(',')}`);
    const newLegends = ['koromon', 'chibomon', 'conomon', 'dodomon', 'jyarimon', 'pichimon', 'pusumon', 'yukimibotamon', 'yuramon', 'zerimon', 'demiveemon', 'dorimon', 'gigimon', 'gummymon', 'kokomon', 'pusurimon', 'upamon'];
    const reg = await ev((ids) => ids.filter((id) => !window.DT.Cosmetics.get('legend', id)), newLegends);
    check(reg.length === 0, `all 17 October legends are registered (${32 - reg.length} legends) ${reg.join(',')}`);
    const canon = await ev(() => { const E = window.DT.LegendEvo; const k = (f) => (E.EVO[f] || '').split(' ').map((x) => x.split(':')[0]);
      return { botamon: k('botamon').includes('koromon') && k('koromon').includes('agumon'), numemon: k('agumon').includes('numemon') && E.formInfo('numemon').model === 'numemon:0',
        chibomon: k('chibomon').includes('demiveemon') && k('demiveemon').includes('veemon'), gigimon: k('gigimon').includes('guilmon'), gummymon: k('gummymon').includes('terriermon'),
        kokomon: k('kokomon').includes('lopmon'), dorimon: k('dorimon').includes('dorumon'), upamon: k('upamon').includes('armadillomon'), sukamon: E.formInfo('sukamon').model === 'extra:Sukamon' && E.reach('bukamon').includes('sukamon') }; });
    check(Object.values(canon).every(Boolean), `canon lines: ${JSON.stringify(canon)}`);
    await shot('le5-panel-ready.png');

    // ---- deterministic roll across reloads ----
    const predicted = await ev((id) => window.DT.LegendEvo._roll(id), L);
    await page.reload();
    await page.waitForTimeout(1500);
    const predicted2 = await ev((id) => window.DT.LegendEvo._roll(id), L);
    check(predicted && predicted === predicted2, `the roll is the same after a reload (${predicted} / ${predicted2})`);
    await ev((id) => window.DT.LegendEvo.open(id), L);
    await page.waitForTimeout(1200);
    await page.click('#dtEvo [data-evo-go]');
    await page.waitForTimeout(700);
    await shot('le6-evolving-white.png');
    await page.waitForTimeout(1300);
    await shot('le7-evolving-morph.png');
    await page.waitForTimeout(2200);
    await shot('le8-evolved.png');
    const form0 = await ev((id) => window.DT.LegendEvo.info(id).form, L);
    check(form0 === predicted, `digivolved into the predicted form ${predicted} (${form0})`);
    // Botamon goes through an In-Training form (drawn with that legend's model) before its Rookie
    const itForm = await ev((id) => { const i = window.DT.LegendEvo.info(id); return i.formInfo.legend ? i.formInfo.legend : null; }, L);
    if (itForm) {
      const tpl = await ev(async (k) => { const t = await window.DT.Legends.modelFor(k); return t && t.item && t.item.id; }, `${L}@${form0}`);
      check(tpl === itForm, `the In-Training form ${form0} uses the ${itForm} legend model (${tpl})`);
      const nx = await ev((id) => window.DT.LegendEvo.info(id), L);
      check(nx.canDigivolve && nx.next === 3, `${form0} can digivolve again at level 3 (next ${nx.next})`);
      await page.click('#dtEvo [data-evo-go]');
      await page.waitForTimeout(4300);
    }
    const form1 = await ev((id) => window.DT.LegendEvo.info(id).form, L);
    check(await ev((f) => window.DT.LegendEvo.formInfo(f).level === 'Rookie', form1), `now a Rookie (${form1})`);
    await shot('le8b-rookie.png');
    await page.reload();
    await page.waitForTimeout(1500);
    const form2 = await ev((id) => ({ form: window.DT.LegendEvo.info(id).form, dex: window.DT.LegendEvo.info(id).dex }), L);
    check(form2.form === form1 && form2.dex.includes(form1), `form survives a reload (${form2.form}), dex ${form2.dex.join(',')}`);

    // ---- equipped evolved legend in game, loadout and lookOf ----
    await ev((id) => window.DT.Cosmetics.equip('legend', id), L);
    const look = await ev(() => ({ lo: window.DT.Cosmetics.loadout(), key: window.DT.LegendEvo.key(window.DT.Cosmetics.equipped('legend')) }));
    check(look.lo.legend === L && look.lo.legendForm === form1, `loadout carries the form (${look.lo.legend} / ${look.lo.legendForm})`);
    await page.waitForTimeout(800);
    await shot('le9-title.png');
    await page.click('#startBtn');
    await page.waitForTimeout(600);
    if (await page.locator('#timerBtn').count()) await page.click('#timerBtn');
    if (await page.locator('.choice[data-carousel]:not(.taken)').count()) {
      await page.locator('.choice[data-carousel]:not(.taken)').first().click();
      await page.click('#carouselGo');
    }
    await page.waitForTimeout(2500);
    const inGame = await ev(() => {
      const r = window.DT.UI.renderer(), g = window.DT.UI.game();
      const v = r.legend && r.legend.view;
      if (!v) return null;
      const look = window.DT.Social.lookOf(g.human);
      const ai = g.players.filter((p) => p !== g.human).map((p) => window.DT.Social.lookOf(p).legend);
      // templates are normalized to 1 unit tall, so the visual scale is its height on the board
      return { s: v.visual.userData.s, key: v.key, form: v.form && v.form.id, tForm: v.t && v.t.form && v.t.form.id, ready: v.ready, look: look.legend, ai,
        sad: window.DT.Legends.play(g.human.id, 'sad'), human: g.human.id };
    });
    console.log('in game:', JSON.stringify(inGame));
    check(inGame && inGame.key === `${L}@${form1}` && inGame.tForm === form1, `board legend uses the evolved model (${inGame && inGame.key}, template ${inGame && inGame.tForm})`);
    check(inGame && inGame.s > 0.3 && inGame.s <= 1.2 + 1e-6, `evolved legend height is capped at 1.2 (${inGame && inGame.s.toFixed(2)} board units)`);
    check(inGame && inGame.look === `${L}@${form1}`, `DT.Social.lookOf gives the form key (${inGame && inGame.look})`);
    check(inGame && inGame.ai.filter((k) => String(k).includes('@')).length >= 5, `AI Tamers wear evolved forms (${inGame && inGame.ai.join(', ')})`);
    check(inGame && inGame.sad === true, 'sad reaction plays on the evolved legend');
    await page.waitForTimeout(500);
    await shot('le10-board-sad.png');
    const emo = await ev((id) => [window.DT.Legends.play(id, 'happy'), window.DT.Legends.play(id, 'dance'), window.DT.Legends.play(id, 'taunt')], inGame.human);
    check(emo.every(Boolean), `happy / dance / taunt play (${emo})`);
    // walk: the evolved form has no run clip, so the renderer hops it
    await ev(() => { const r = window.DT.UI.renderer(); r.legend.tx = r.legend.x + 4; r.legend.tz = r.legend.z - 2; });
    await page.waitForTimeout(250);
    const walk = await ev(() => { const v = window.DT.UI.renderer().legend.view; return { hasRun: v.hasRun, moving: v.moving }; });
    check(walk.hasRun === false && walk.moving === true, `walking uses the procedural hop (hasRun ${walk.hasRun}, moving ${walk.moving})`);
    await page.waitForTimeout(400);
    await shot('le11-board-walk.png');

    // ---- revert ----
    await ev((id) => window.DT.LegendEvo.open(id), L);
    await page.waitForTimeout(900);
    const nonce0 = await ev((id) => JSON.parse(localStorage.getItem('dt.legendevo.v1')).legends[id].nonce, L);
    await page.click('#dtEvo [data-evo-revert]');
    await page.waitForTimeout(800);
    const rv = await ev((id) => ({ i: window.DT.LegendEvo.info(id), nonce: JSON.parse(localStorage.getItem('dt.legendevo.v1')).legends[id].nonce }), L);
    check(rv.i.form === (itForm ? form0 : L) && rv.i.level === st.level && rv.i.dex.includes(form1) && rv.nonce === nonce0 + 1, `revert goes back to ${rv.i.form}, keeps level ${rv.i.level} and the dex, new nonce ${rv.nonce}`);
    const treeOk = await ev((f) => { const n = document.querySelector(`#dtEvo [data-node="${f}"]`); return n && !n.classList.contains('unk'); }, form1);
    check(treeOk, 'discovered form stays lit in the tree after revert');
    await shot('le12-reverted.png');

    // ---- overflow at 1200x760 ----
    const over = await ev(() => {
      const bad = [];
      const win = document.querySelector('#dtEvo .dte-win');
      const wr = win.getBoundingClientRect();
      if (wr.right > innerWidth || wr.bottom > innerHeight || wr.left < 0 || wr.top < 0) bad.push('window off screen');
      for (const sel of ['.dte-left', '.dte-right', '.dte-tree', '.dte-odds', '.dte-head']) {
        const el = document.querySelector(`#dtEvo ${sel}`);
        if (el.scrollWidth > el.clientWidth + 1) bad.push(`${sel} x ${el.scrollWidth}>${el.clientWidth}`);
        if (sel !== '.dte-head' && el.scrollHeight > el.clientHeight + 1) bad.push(`${sel} y ${el.scrollHeight}>${el.clientHeight}`);
      }
      for (const n of document.querySelectorAll('#dtEvo .dte-node, #dtEvo .dte-orow, #dtEvo button')) {
        const r = n.getBoundingClientRect();
        if (n.closest('.dte-pick')) continue; // the legend picker scrolls sideways
        if (r.width && (r.right > wr.right + 1 || r.left < wr.left - 1)) bad.push(`${n.className || n.tagName} outside`);
      }
      if (document.documentElement.scrollWidth > innerWidth) bad.push('page scrolls sideways');
      return bad;
    });
    check(over.length === 0, `no overflow at 1200x760 ${over.slice(0, 6).join('; ')}`);
    // the largest tree too
    await ev(() => { const E = window.DT.LegendEvo; let best = null, n = 0; for (const x of window.DT.Cosmetics.list('legend')) { const c = E.reach(x.id).length; if (c > n) { n = c; best = x.id; } } E.close(); E.open(best); });
    await page.waitForTimeout(1200);
    const over2 = await ev(() => {
      const t = document.querySelector('#dtEvo .dte-tree');
      const tr = t.getBoundingClientRect();
      const bad = [];
      for (const n of t.querySelectorAll('.dte-node')) { const r = n.getBoundingClientRect(); if (r.right > tr.right + 1 || r.left < tr.left - 1 || r.bottom > tr.bottom + 1 || r.top < tr.top - 1) bad.push(n.dataset.node); }
      return bad;
    });
    check(over2.length === 0, `largest tree fits (${over2.join(',')})`);
    await shot('le13-largest-tree.png');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(200);
    check(await page.locator('#dtEvo.hidden').count() === 1, 'Esc closes the panel');

    check(errors.length === 0, `no page errors ${errors.slice(0, 5).join(' | ')}`);
  } catch (e) {
    console.error(e);
    fails.push(String(e));
    try { await shot('le-error.png'); } catch { /* ignore */ }
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fails.length ? `\n${fails.length} FAILED` : '\nall passed');
  process.exit(fails.length ? 1 : 0);
})();
