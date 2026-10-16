// Dance check: the bone finder on every Little Legend (and every unit model, as a report), every dance on several
// skeletons with the rotation limits held and no NaN, frame strips saved as pictures, the kill screen playing the
// killer's equipped dance, the dance emote, and the Store / Locker Dances tab with live previews.
// Run: node tests/dances.cjs <path-to-playwright> <outDir>
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = 8158;
fs.mkdirSync(out, { recursive: true });

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
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
  page.on('console', (m) => { if (m.type() === 'error' && !/Failed to load resource/.test(m.text())) errors.push(m.text()); });
  const fail = [];
  const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fail.push(msg); };
  const g = (fn, arg) => page.evaluate(fn, arg);

  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);

    // ---------- catalog ----------
    const cat = await g(() => DT.Cosmetics.list('dance').map((x) => ({ id: x.id, name: x.name, free: !!x.free, price: x.price, rarity: x.rarity, bpm: x.data.bpm })));
    check(cat.length >= 10, `${cat.length} dances registered`);
    check(cat.filter((x) => x.free).length === 2, `two free dances (${cat.filter((x) => x.free).map((x) => x.name).join(', ')})`);
    check(cat.every((x) => x.free || x.price > 0) && cat.every((x) => x.bpm >= 60 && x.bpm <= 200), 'paid dances have a price, every dance has a BPM');
    check(await g(() => DT.Cosmetics.KINDS.includes('dance') && !!DT.Cosmetics.loadout().dance), 'dance is in KINDS and the loadout');
    for (const d of cat) console.log(`     ${d.name.padEnd(22)} ${String(d.bpm).padStart(3)} BPM  ${d.free ? 'free' : `${d.rarity} ${d.price}`}`);

    // ---------- bone finder ----------
    // static rips (Upamon, the Retro Digimon World set) have no skeleton and dance with procedural root motion
    const statics = await g(() => DT.Cosmetics.list('legend').filter((x) => x.data && x.data.static).map((x) => x.id));
    const legends = await g(() => DT.Cosmetics.list('legend').filter((x) => !(x.data && x.data.static)).map((x) => x.id));
    console.log(`     static legends (no bones): ${statics.join(', ')}`);
    const cov = await g((ids) => DT.Dances.coverage(ids), legends);
    const core = cov.filter((r) => !r.missing && r.hips && (r.head || r.spine.length || r.hips));
    for (const r of cov) console.log(`     ${r.key.padEnd(10)} bones ${String(r.bones).padStart(2)} hips ${r.hips} head ${r.head || '-'} arms ${r.arms ? `${r.armL.up}/${r.armR.up}` : '-'} legs ${r.legs ? `${r.legL.thigh}/${r.legR.thigh}` : '-'} ears ${r.earL.length ? 'y' : '-'} tail ${r.tail.length}`);
    check(core.length === legends.length, `core bones (hips + head or body) found on ${core.length}/${legends.length} legends`);
    const heads = cov.filter((r) => r.head).length;
    check(heads >= legends.length - 5, `head found on ${heads}/${legends.length} legends (round legends with no head bone, such as Punimon, Kuramon and Dodomon, dance with the body)`);
    console.log(`     legends: arms ${cov.filter((r) => r.arms).length}, legs ${cov.filter((r) => r.legs).length}, head ${heads}, ears ${cov.filter((r) => r.earL.length).length} of ${legends.length}`);
    // every unit model, as a report
    const units = JSON.parse(fs.readFileSync(path.join(__dirname, '..', 'models', 'index.json'), 'utf8'));
    const ukeys = Object.keys(units);
    const ucov = await g((keys) => DT.Dances.coverage(keys.map((k) => `unit:${k}`)), ukeys);
    const uok = ucov.filter((r) => !r.missing);
    const uskel = uok.filter((r) => r.bones > 0);
    console.log(`     units: ${uok.length} models, ${uskel.length} with skeletons: arms ${uskel.filter((r) => r.arms).length}, legs ${uskel.filter((r) => r.legs).length}, head ${uskel.filter((r) => r.head).length}, tail ${uskel.filter((r) => r.tail.length).length}, wings ${uskel.filter((r) => r.wingL.length).length}`);
    const missed = uskel.filter((r) => !r.head || !r.arms);
    console.log(`     units missing head or arms: ${missed.map((r) => `${r.key.slice(5)}(${[!r.head && 'head', !r.arms && 'arms'].filter(Boolean).join('+')})`).join(' ')}`);
    fs.writeFileSync(path.join(out, 'dance-coverage.json'), JSON.stringify({ legends: cov, units: ucov }, null, 1));
    check(uskel.filter((r) => r.head).length >= uskel.length * 0.85, `head found on ${uskel.filter((r) => r.head).length}/${uskel.length} skinned unit models`);

    // ---------- every dance on several skeletons: limits, NaN ----------
    const sample = ['tokomon', 'bukamon', 'motimon', 'tsumemon', 'kuramon', 'pabumon', 'unit:agumon:0', 'unit:renamon:0', 'unit:patamon:0', 'unit:patamon:1', 'unit:agumon:1', 'unit:betamon:0'];
    const lim = await g(async ({ sample, dances }) => {
      const res = [];
      for (const key of sample) {
        let d = null, view = null;
        if (key.startsWith('unit:')) { const r = await DT.Dances.unitRig(key.slice(5), 'floss'); d = r && r.dancer; }
        else {
          view = DT.Legends.createView(key, { ring: false });
          for (let i = 0; i < 100 && !view.ready; i++) await new Promise((r) => setTimeout(r, 50));
          d = DT.Dances.attachView(view, 'floss').dancer;
        }
        if (!d) { res.push({ key, missing: true }); continue; }
        res.push(...dances.map((id) => test(d, id, key)));
        if (view) view.dispose();
      }
      function test(d, id, key) {
        d.set(id);
        let worst = 0, nan = false, n = 0;
        for (let t = 0; t < 8; t += 0.07) {
          const rt = d.apply(t, 1);
          for (const v of Object.values(rt)) if (typeof v === 'number' && !Number.isFinite(v)) nan = true;
          for (const e of d.E) {
            const q = d.bones[e.bi].quaternion, q0 = d.rig.bones[e.bi].q0;
            if (![q.x, q.y, q.z, q.w].every(Number.isFinite)) { nan = true; continue; }
            const ang = 2 * Math.acos(Math.min(1, Math.abs(q.x * q0.x + q.y * q0.y + q.z * q0.z + q.w * q0.w)));
            const max = e.lim || DT.Dances.limits[e.kind] * Math.PI / 180;
            worst = Math.max(worst, ang - max);
            n++;
          }
        }
        return { key, id, nan, over: worst, driven: d.E.length, n };
      }
      return res;
    }, { sample, dances: cat.map((d) => d.id) });
    const bad = lim.filter((r) => r.missing || r.nan || r.over > 1e-3);
    for (const k of sample) { const rs = lim.filter((r) => r.key === k); console.log(`     ${k.padEnd(16)} driven bones ${rs[0] ? rs[0].driven : '-'}`); }
    check(!bad.length && lim.length >= sample.length * cat.length, `${cat.length} dances x ${sample.length} skeletons: no NaN, every bone inside its limit${bad.length ? ` (bad: ${JSON.stringify(bad.slice(0, 4))})` : ''}`);

    // ---------- frame strips ----------
    const strips = [
      ['floss', ['tokomon', 'bukamon', 'unit:agumon:0', 'unit:renamon:0']],
      ['robot', ['motimon', 'unit:patamon:1', 'unit:agumon:1', 'tanemon']],
      ['running', ['tsumemon', 'unit:renamon:0', 'unit:agumon:0', 'pabumon']],
      ['disco', ['bukamon', 'unit:patamon:1', 'unit:betamon:0', 'pagumon']],
      ['macarena', ['motimon', 'unit:renamon:0', 'unit:agumon:1', 'tokomon']],
      ['windmill', ['bukamon', 'unit:agumon:0', 'unit:patamon:0', 'kuramon']],
      ['gangnam', ['tokomon', 'unit:renamon:0', 'unit:patamon:1', 'wanyamon']],
      ['jig', ['tanemon', 'unit:agumon:1', 'unit:patamon:0', 'botamon']],
      ['shuffle', ['bukamon', 'unit:renamon:0', 'unit:betamon:0', 'nyaromon']],
      ['butterfly', ['motimon', 'unit:patamon:1', 'unit:agumon:0', 'punimon']],
      ['headbang', ['tsumemon', 'unit:agumon:1', 'unit:renamon:0', 'poyomon']],
    ];
    const FR = 6, CW = 200;
    for (const [dance, models] of strips) {
      const ok = await g(async ({ dance, models, FR, CW }) => {
        let cv = document.getElementById('danceStrip');
        if (!cv) { cv = document.createElement('canvas'); cv.id = 'danceStrip'; cv.style.cssText = 'position:fixed;left:0;top:0;z-index:999;background:#1d2533'; document.body.appendChild(cv); }
        cv.width = FR * CW; cv.height = models.length * CW + 24;
        const c = cv.getContext('2d');
        const bpm = DT.Dances.bpm(dance);
        const draw = () => {
          c.fillStyle = '#1d2533'; c.fillRect(0, 0, cv.width, cv.height);
          c.fillStyle = '#fff'; c.font = '16px sans-serif'; c.fillText(`${DT.Dances.get(dance).name}  ${bpm} BPM  (frames every third of a beat)`, 8, 17);
          let all = true;
          models.forEach((m, row) => {
            for (let f = 0; f < FR; f++) {
              const t = (f / 3 + (dance === 'windmill' ? 6 : dance === 'macarena' ? 4 : 0)) * 60 / bpm;
              const o = m.startsWith('unit:') ? { unit: m.slice(5), dance, t, size: CW, dist: 3.1 } : { legend: m, dance, t, size: CW, dist: 3.1 };
              if (!DT.Dances.drawInto(c, f * CW, 24 + row * CW, CW, CW, o)) all = false;
            }
            c.fillStyle = '#9ab'; c.font = '12px sans-serif'; c.fillText(m, 4, 24 + row * CW + 14);
          });
          return all;
        };
        for (let i = 0; i < 60; i++) { if (draw()) return true; await new Promise((r) => setTimeout(r, 150)); }
        return false;
      }, { dance, models, FR, CW });
      await page.screenshot({ path: path.join(out, `dance-${dance}.png`), clip: { x: 0, y: 0, width: FR * CW, height: models.length * CW + 24 } });
      check(ok, `frame strip for ${dance} on ${models.join(', ')}`);
    }
    await g(() => document.getElementById('danceStrip').remove());

    // ---------- Store / Locker ----------
    await g(() => DT.Shop.open('dance'));
    await page.waitForTimeout(1500);
    const store = await g(() => ({
      tab: !!document.querySelector('.dts-tabs [data-tab="dance"]'),
      cards: document.querySelectorAll('.dts-body section:not(.dts-feat) .dts-card[data-kind="dance"]').length,
      drawn: [...document.querySelectorAll('.dts-body .dts-card[data-kind="dance"] canvas.dancePreview')].filter((c) => c.dataset.drawn === '1').length,
    }));
    check(store.tab && store.cards >= 10, `Store Dances tab with ${store.cards} cards`);
    check(store.drawn >= 3, `${store.drawn} dance previews drawing the legend`);
    await page.screenshot({ path: path.join(out, 'dance-store.png') });
    // buy one: Bits, Buy, Confirm, Equip now
    await g(() => DT.Wallet.add(5000, 'test'));
    await page.click('.dts-body section:not(.dts-feat) .dts-card[data-id="disco"] [data-buy]');
    await page.waitForTimeout(200);
    await page.click('.dts-modal [data-confirm]');
    await page.waitForTimeout(300);
    if (await page.locator('.dts-modal [data-equipnow]').count()) await page.click('.dts-modal [data-equipnow]');
    await page.waitForTimeout(200);
    check(await g(() => DT.Cosmetics.owns('dance', 'disco') && DT.Cosmetics.equipped('dance') === 'disco'), 'bought and equipped Disco Point');
    await g(() => DT.Shop.close());
    await g(() => DT.Shop.openLocker('dance'));
    await page.waitForTimeout(800);
    const locker = await g(() => ({ cards: document.querySelectorAll('.dts-body .dts-card[data-kind="dance"]').length, eq: !!document.querySelector('.dts-card.eq[data-id="disco"]') }));
    check(locker.cards >= 3 && locker.eq, `Locker Dances tab: ${locker.cards} owned, Disco Point equipped`);
    await page.click('.dts-body .dts-card[data-id="jig"] [data-equip]');
    await page.waitForTimeout(200);
    check(await g(() => DT.Cosmetics.equipped('dance') === 'jig'), 'Locker equip switches the dance');
    await g(() => DT.Cosmetics.equip('dance', 'disco'));
    // the kill screen tab preview shows the legend dancing
    await page.click('.dts-tabs [data-tab="killscreen"]');
    await page.waitForTimeout(2500);
    const ksPrev = await g(() => [...document.querySelectorAll('.dts-body canvas.ksPreview')].filter((c) => c.dataset.dance === '1').length);
    check(ksPrev >= 1, `${ksPrev} kill screen previews show the equipped dance`);
    await page.screenshot({ path: path.join(out, 'dance-locker-killscreen.png') });
    await g(() => DT.Shop.close());

    // ---------- kill screen ----------
    await page.click('#startBtn');
    await page.waitForTimeout(500);
    await page.click('#timerBtn').catch(() => {});
    const c1 = page.locator('.choice[data-carousel]:not(.taken)');
    if (await c1.count()) { await c1.first().click(); await page.click('#carouselGo').catch(() => {}); }
    await page.waitForTimeout(800);
    await g(() => { const G = window.__digi.game; DT.KillScreens.demo({ killer: G.human, victim: G.players[1], variant: 'ks_confetti' }); });
    await page.waitForTimeout(1600);
    const ks = await g(() => DT.KillScreens.active);
    check(ks && ks.dance === 'disco', `your kill screen plays your equipped dance (${ks && ks.dance})`);
    check(ks && ks.rigs >= 1, 'kill screen has the dancing model');
    await page.screenshot({ path: path.join(out, 'dance-killscreen-1.png') });
    await page.waitForTimeout(700);
    await page.screenshot({ path: path.join(out, 'dance-killscreen-2.png') });
    await page.keyboard.press('Escape');
    await page.waitForTimeout(150);
    // another Tamer's kill screen uses their loadout's dance
    const ai = await g(() => { const G = window.__digi.game; const a = G.players.filter((p) => !p.isHuman); DT.KillScreens.demo({ killer: a[0], victim: a[1] }); return DT.Social.lookOf(a[0]).dance; });
    await page.waitForTimeout(500);
    const ks2 = await g(() => DT.KillScreens.active);
    check(ks2 && ks2.dance === ai, `another Tamer's kill screen plays their dance (${ks2 && ks2.dance}, loadout ${ai})`);
    await page.keyboard.press('Escape');
    await page.waitForTimeout(150);

    // ---------- emote ----------
    const em = await g(() => { const G = window.__digi.game; DT.Emotes.send('mydance'); return DT.Dances.playing.find((p) => p.playerId === G.human.id); });
    check(em && em.dance === 'disco', `My Dance emote plays the equipped dance on your legend (${em && em.dance})`);
    await page.waitForTimeout(900);
    await page.screenshot({ path: path.join(out, 'dance-emote.png') });
    check(!errors.length, `no page errors${errors.length ? `: ${errors.slice(0, 5).join(' | ')}` : ''}`);
  } catch (e) {
    console.log('FAIL exception', e);
    fail.push(String(e));
  } finally {
    await browser.close();
    server.kill();
  }
  console.log(fail.length ? `\n${fail.length} FAILED` : '\nall dance checks passed');
  process.exit(fail.length ? 1 : 0);
})();
