// Parity captures of the browser/Electron build (index.html + js/) for side-by-side comparison with native captures.
//   node tools/parity/capture-electron.cjs [--states a,b,c] [--sizes 1280x720,...] [--out parity] [--port 8190]
//        [--model <unitId> --model-times attack=0.35,special=0.5,win=0.5]
// Writes <out>/<state>/<size>/electron.png, <out>/model-<id>/<size>/electron-<pose>.png and <out>/electron-report.json.
//
// Determinism: every run uses a fresh browser context (empty localStorage, seeded from
// native/tests/fixtures/collection-profile.json), Math.random replaced by a fixed-seed PRNG, Playwright's fake clock
// (Date, timers, requestAnimationFrame and performance.now only move when this script advances them), headless
// SwiftShader WebGL and --mute-audio. Network loads (models, art, fonts) happen in real time while the fake clock is
// stopped, so the amount of game time before each capture is fixed. Firebase is replaced by tests/fake-firebase.cjs;
// every other off-machine request except Google Fonts is aborted.
const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require('C:/Users/William/Documents/Repos/dungeon-delver/node_modules/playwright');
const { installFakeFirebase } = require('../../tests/fake-firebase.cjs');
const { MockSteamHub } = require('../../tests/steam-mock-hub.cjs');

const ROOT = path.join(__dirname, '..', '..');
const STATES = JSON.parse(fs.readFileSync(path.join(__dirname, 'states.json'), 'utf8'));
const FIXTURE = JSON.parse(fs.readFileSync(path.join(ROOT, 'native/tests/fixtures/collection-profile.json'), 'utf8'));
const SEED = 20261021;
const START_TIME = Date.parse('2026-10-01T12:00:00Z');

function arg(name, dflt) {
  const a = process.argv;
  for (let i = 2; i < a.length; i++) {
    if (a[i] === `--${name}`) return a[i + 1];
    if (a[i].startsWith(`--${name}=`)) return a[i].slice(name.length + 3);
  }
  return dflt;
}
// first line of an error plus Playwright's note on what intercepted a click
const errText = (e) => { const lines = String(e).split('\n'); return [lines[0], ...lines.filter((l) => /intercepts pointer events|waiting for locator/.test(l)).slice(0, 2).map((l) => l.trim())].join(' | '); };
const FORCE = { force: true, timeout: 5000 };
const list = (s) => String(s).split(',').map((x) => x.trim()).filter(Boolean);
const OUT = path.resolve(ROOT, arg('out', 'parity'));
const PORT = Number(arg('port', 8190));
const SIZES = list(arg('sizes', STATES.sizes.join(','))).map((s) => { const [w, h] = s.split('x').map(Number); return { id: s, w, h }; });
const ALL_IDS = STATES.states.map((s) => s.id);
const WANT = arg('states') ? list(arg('states')) : ALL_IDS;
for (const id of WANT) if (!ALL_IDS.includes(id)) throw new Error(`unknown state ${id} (states.json has ${ALL_IDS.join(', ')})`);
const MODEL = arg('model', null);
const MODEL_TIMES = Object.fromEntries(list(arg('model-times', 'attack=0.35,special=0.5,win=0.5')).map((kv) => { const [k, v] = kv.split('='); return [k, Number(v)]; }));
const BASE = `http://localhost:${PORT}`;

const COLLECTION_TABS = { 'collection-legends': 'legend', 'collection-emotes': 'emote', 'collection-boards': 'board', 'collection-killscreens': 'killscreen',
  'collection-dances': 'dance', 'collection-soundboard': 'sound', 'collection-elimsounds': 'elimsound' };
const NAV_PAGES = ['store', 'loot', 'farm', 'codex', 'profile', 'settings'];

// ---------------------------------------------------------------- server
function startServer() {
  return new Promise((resolve, reject) => {
    const child = spawn(process.execPath, [path.join(ROOT, 'serve.cjs')], { cwd: ROOT, env: { ...process.env, PORT: String(PORT) }, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'] });
    let log = '';
    const t = setTimeout(() => reject(new Error(`serve.cjs did not start: ${log}`)), 10000);
    child.stdout.on('data', (d) => { log += d; if (/Digi-Tactics at/.test(log)) { clearTimeout(t); resolve(child); } });
    child.stderr.on('data', (d) => { log += d; });
    child.on('exit', (code) => { clearTimeout(t); reject(new Error(`serve.cjs exited ${code}: ${log}`)); });
  });
}

// ---------------------------------------------------------------- one isolated browser run
async function openRun(browser, size, opts = {}) {
  const context = await browser.newContext({ viewport: { width: size.w, height: size.h }, deviceScaleFactor: 1, locale: 'en-US', timezoneId: 'UTC', colorScheme: 'dark' });
  // the Electron/Steam build talks to Steam through window.DTSteam; the lobby state uses the in-memory MockSteamHub
  // from tests/steam-mock-hub.cjs with no lobbies, so it shows the empty Steam lobby browser
  if (opts.steam) await new MockSteamHub().install(context, { steamId: '76561190000000001', name: 'Tamer' });
  const page = await context.newPage();
  const errors = [];
  let inflight = 0, lastNet = Date.now();
  page.on('pageerror', (e) => errors.push(`pageerror: ${String(e).split('\n')[0]}`));
  page.on('console', (m) => { if (m.type() === 'error' && !m.text().startsWith('Failed to load resource')) errors.push(`console: ${m.text().slice(0, 300)}`); });
  page.on('response', (r) => { if (r.status() >= 400) errors.push(`${r.status()} ${r.url()}`); });
  page.on('request', () => { inflight++; lastNet = Date.now(); });
  const done = () => { inflight = Math.max(0, inflight - 1); lastNet = Date.now(); };
  page.on('requestfinished', done);
  page.on('requestfailed', done);
  await installFakeFirebase(page);
  await page.route(/^https?:\/\/(?!localhost[:/]|127\.0\.0\.1[:/]|fonts\.googleapis\.com\/|fonts\.gstatic\.com\/|www\.gstatic\.com\/firebasejs\/)/, (route) => {
    errors.push(`blocked ${route.request().url()}`);
    route.abort();
  });
  await page.addInitScript(({ seed, storage }) => {
    let s = seed >>> 0; // mulberry32
    Math.random = () => { s = (s + 0x6d2b79f5) >>> 0; let t = s; t = Math.imul(t ^ (t >>> 15), t | 1); t ^= t + Math.imul(t ^ (t >>> 7), t | 61); return ((t ^ (t >>> 14)) >>> 0) / 4294967296; };
    try { if (!localStorage.getItem('__parity_seeded')) { for (const [k, v] of Object.entries(storage)) localStorage.setItem(k, v); localStorage.setItem('__parity_seeded', '1'); } } catch { /* ignore */ }
  }, { seed: SEED, storage: FIXTURE.storage || {} });
  await page.clock.install({ time: START_TIME });

  // real-time wait until the network is quiet (the fake clock does not move meanwhile)
  const quiet = async (maxMs = 15000, idleMs = 250) => {
    const t0 = Date.now();
    while (Date.now() - t0 < maxMs) {
      if (inflight === 0 && Date.now() - lastNet >= idleMs) return true;
      await new Promise((r) => setTimeout(r, 50));
    }
    return false;
  };
  // move game time forward in fixed slices, letting loads settle between slices
  const advance = async (ms, slice = 250) => {
    for (let left = ms; left > 0; left -= slice) { await page.clock.runFor(Math.min(slice, left)); await quiet(); }
  };
  const ev = (fn, a) => page.evaluate(fn, a);
  // advance until fn() is true (checked between slices); throws after maxFake ms of game time
  const until = async (fn, a, what, maxFake = 20000, slice = 100) => {
    for (let t = 0; t <= maxFake; t += slice) {
      if (await ev(fn, a)) return t;
      await page.clock.runFor(slice);
      await quiet();
    }
    throw new Error(`timed out waiting for ${what}`);
  };
  // force: the fake clock also drives requestAnimationFrame, which Playwright's stability check waits on
  const click = async (sel) => { await page.locator(sel).first().click(FORCE); await advance(100); };
  // fonts, <img> elements, CSS backgrounds in flight and 3D models: wait until all are loaded
  const settle = async () => {
    await quiet();
    for (let i = 0; i < 40; i++) {
      const pending = await ev(() => {
        const imgs = [...document.images].filter((im) => im.src && !im.complete && im.offsetParent);
        const r = window.__digi && window.__digi.renderer;
        const views = r && r.views ? [...r.views.values()].filter((v) => v.modelKey && v.hasModel === false).length : 0;
        return { fonts: document.fonts.status !== 'loaded', imgs: imgs.length, views };
      });
      if (!pending.fonts && !pending.imgs && !pending.views) break;
      await page.clock.runFor(50);
      await quiet();
    }
    await advance(100);
    await quiet();
  };
  const boot = async () => {
    await page.goto(`${BASE}/`, { waitUntil: 'load' });
    await quiet();
    await until(() => !document.getElementById('boot'), null, 'boot screen to finish', 30000, 250);
    await advance(800);
    await settle();
  };
  return { context, page, errors, ev, advance, until, click, quiet, settle, boot };
}

// move the pointer to an empty spot of the left column so no hover tooltip or highlight shows
async function parkMouse(run) {
  await run.page.mouse.move(60, Math.round(run.size.h * 0.78));
  await run.advance(50);
}
async function shoot(run, file) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  await parkMouse(run);
  await run.settle();
  await run.page.screenshot({ path: file, animations: 'disabled', caret: 'hide' });
}

// ---------------------------------------------------------------- client pages
const nav = async (run, p) => {
  const sel = { profile: '#cnProfile', settings: '#cnSettings' }[p] || `#cnav .cn-tab[data-page="${p}"]`;
  await run.click(sel);
  await run.advance(p === 'farm' ? 1500 : 500);
};

async function clientState(run, id) {
  await run.boot();
  if (id === 'home') return;
  if (id === 'play') return nav(run, 'play');
  if (id === 'party') { await nav(run, 'play'); await run.click('[data-mode-pick="solo"]'); await run.click('#playConfirm'); await run.advance(500); return; }
  if (id === 'lobby') {
    await nav(run, 'play'); await run.click('[data-mode-pick="mp"]'); await run.click('#playConfirm');
    await run.until(() => { const l = document.getElementById('lobbyPanel'); return l && l.innerHTML.trim() && !l.querySelector('.mp-wait'); }, null, 'lobby list', 15000, 250);
    await run.advance(500);
    return;
  }
  if (id === 'match-loading') {
    // #cload stays up for 2.1 s; the screenshot's animations: 'disabled' shows its cards and bar at their end state
    await nav(run, 'play'); await run.click('[data-mode-pick="solo"]'); await run.click('#playConfirm'); await run.advance(300);
    await run.click('#playSoloBtn');
    await run.until(() => !!document.getElementById('cload'), null, 'match loading screen', 15000, 50);
    // keep the screen up while settle() waits for arena models: its timers only fade and remove this element
    await run.ev(() => { const el = document.getElementById('cload'); el.classList.add = () => {}; el.remove = () => {}; });
    await run.advance(900, 100);
    return;
  }
  if (COLLECTION_TABS[id]) { await nav(run, 'collection'); await run.click(`#pgCollection [data-ctab="${COLLECTION_TABS[id]}"]`); await run.advance(900); return; }
  if (NAV_PAGES.includes(id)) return nav(run, id);
  throw new Error(`no client route for ${id}`);
}

// ---------------------------------------------------------------- match
async function startSolo(run) {
  await run.boot();
  await nav(run, 'play');
  await run.click('[data-mode-pick="solo"]');
  await run.click('#playConfirm');
  await run.advance(300);
  await run.click('#playSoloBtn');
  await run.until(() => !!(window.__digi && window.__digi.game) && !document.getElementById('cload'), null, 'match loading screen to finish', 30000, 250);
}
const carouselUp = () => !!document.querySelector('.choice[data-carousel]');
async function finishCarousel(run) {
  await run.until(() => !!document.querySelector('.choice[data-carousel]:not(.taken)'), null, 'opening carousel choice', 15000);
  await run.advance(1000); // the carousel slides in; at 1920x1080 an early click can land before the cards are hit-testable
  // the carousel can move on by itself (auto pick, auto start) while a slow frame is rendering, so a vanished
  // target is not an error; the phase check below decides
  const tryClick = async (sel) => { try { await run.page.locator(sel).first().click({ force: true, timeout: 3000 }); } catch { /* moved on */ } await run.advance(100); };
  await tryClick('.choice[data-carousel]:not(.taken)');
  await run.until(() => !!document.getElementById('carouselGo') || (window.__digi.game.phase === 'planning' && window.__digi.game.roundType !== 'carousel'), null, 'carousel completion', 30000, 250);
  if (await run.ev(() => !!document.getElementById('carouselGo'))) await tryClick('#carouselGo');
  await run.until(() => { const g = window.__digi.game; return g.phase === 'planning' && g.roundType !== 'carousel'; }, null, 'first planning phase', 15000);
}
// dismiss whatever modal blocks the next round in a solo match (offers are handled by the caller)
async function nextRound(run) {
  const g0 = await run.ev(() => `${window.__digi.game.stage}-${window.__digi.game.round}`);
  if (await run.page.locator('#readyBtn:not([disabled])').count()) await run.page.locator('#readyBtn').click(FORCE);
  await run.until(() => window.__digi.game.phase === 'combat' || window.__digi.game.over, null, 'combat to start', 15000);
  await run.advance(300);
  if (await run.page.locator('#skipBtn:not([disabled])').count()) await run.page.locator('#skipBtn').click(FORCE);
  await run.until((g0) => { const g = window.__digi.game; return g.over || (g.phase === 'planning' && `${g.stage}-${g.round}` !== g0); }, g0, 'next planning phase', 60000, 250);
}

async function matchStates(run, want, shotPath, results) {
  const t0 = Date.now();
  let last = t0;
  const record = async (id, fn) => {
    if (!want.includes(id)) { await fn(false); return; }
    const r = { state: id, size: run.size.id, ok: false };
    try { await fn(true); await shoot(run, shotPath(id)); r.ok = true; r.file = path.relative(ROOT, shotPath(id)); }
    catch (e) { r.error = errText(e); throw Object.assign(e, { record: r }); }
    finally { const now = Date.now(); r.ms = now - last; last = now; r.errors = run.errors.splice(0); results.push(r); }
  };
  const order = ['match-carousel', 'match-planning', 'match-combat', 'match-augment'];
  const lastNeeded = Math.max(...want.map((w) => order.indexOf(w)));
  try {
    await record('match-carousel', async () => {
      await startSolo(run);
      await run.until(carouselUp, null, 'opening carousel', 15000);
      await run.advance(1000);
    });
    if (lastNeeded < 1) return;
    await record('match-planning', async () => { await finishCarousel(run); await run.advance(1000); });
    if (lastNeeded < 2) return;
    await record('match-combat', async () => {
      await run.page.locator('#readyBtn').click(FORCE);
      await run.until(() => window.__digi.game.phase === 'combat', null, 'first combat', 15000);
      await run.advance(2000, 100);
    });
    if (lastNeeded < 3) return;
    await record('match-augment', async () => {
      if (await run.page.locator('#skipBtn:not([disabled])').count()) await run.page.locator('#skipBtn').click(FORCE);
      await run.until(() => window.__digi.game.phase === 'planning' || window.__digi.game.over, null, 'planning after the first combat', 60000, 250);
      for (let i = 0; i < 25; i++) {
        if (await run.ev(() => !!document.querySelector('[data-offer]'))) break;
        if (await run.ev(() => window.__digi.game.over)) throw new Error('match ended before any offer');
        // keep the Tamer alive so the match reaches its first offer
        await run.ev(() => { const h = window.__digi.game.human; if (h.hp < 6) h.hp = 6; });
        await nextRound(run);
        await run.advance(500);
      }
      await run.until(() => !!document.querySelector('[data-offer]'), null, 'first offer choice', 2000);
      await run.advance(800);
    });
  } catch (e) {
    // remaining wanted match states could not be reached
    const doneIds = results.map((r) => `${r.state}@${r.size}`);
    for (const id of order) if (want.includes(id) && !doneIds.includes(`${id}@${run.size.id}`)) results.push({ state: id, size: run.size.id, ok: false, ms: 0, error: `not reached: ${errText(e)}`, errors: [] });
    try { await run.page.screenshot({ path: path.join(OUT, 'match-error', run.size.id, 'electron-error.png') }); } catch { /* ignore */ }
  }
}

async function postgame(run) {
  await startSolo(run);
  await finishCarousel(run);
  for (let i = 0; i < 3; i++) { const c = run.page.locator(`.card[data-slot="${i}"]:not(.poor)`); if (await c.count()) await c.first().click(FORCE); }
  await run.advance(500);
  await run.ev(() => { const g = window.__digi.game; for (const p of g.players) if (p !== g.human) p.hp = 0; });
  await run.page.locator('#readyBtn').click(FORCE);
  await run.until(() => window.__digi.game.phase === 'combat' || window.__digi.game.over, null, 'final combat', 15000);
  await run.advance(300);
  if (await run.page.locator('#skipBtn:not([disabled])').count()) await run.page.locator('#skipBtn').click(FORCE);
  await run.until(() => !!document.querySelector('#overlay:not(.hidden) #homeResults .results'), null, 'result screen', 60000, 250);
  // the seven queued elimination notices would cover the result screen for about a minute: clear them
  await run.ev(() => DT.KillScreens && DT.KillScreens.clear());
  await run.until(() => !(DT.KillScreens && DT.KillScreens.active), null, 'elimination notices to clear', 10000, 100);
  await run.advance(3000);
}

// ---------------------------------------------------------------- model poses
// Each pose resets the planning timer to 25.5 s, so it reads the same and never starts combat mid-capture.
// Runs in the page: pose the single board unit at clip time t of a role (idle: idle clip at t) through the renderer's
// own UnitView.play (fades and attack/special time scaling included), then freeze its mixer so later frames keep the pose.
const POSE = ({ pose, t }) => {
  window.__digi.ui.planLeft = 25.5;
  const v = [...window.__digi.renderer.views.values()].find((x) => x.key.startsWith('u'));
  if (!v) return 'unit view missing (planning ended?)';
  if (!v.mixer) return 'no animated model';
  if (v.__realUpdate) v.mixer.update = v.__realUpdate;
  v.mixer.stopAllAction(); v.oneShot = null; v.oneShotRole = null;
  v.idleAction.reset().setEffectiveWeight(1).play();
  if (pose !== 'idle' && !v.play(pose)) return 'no ' + pose + ' clip';
  for (let e = 0; e < t - 1e-6;) { const d = Math.min(1 / 60, t - e); v.mixer.update(d); e += d; }
  if (t <= 0) v.mixer.update(0);
  v.__realUpdate = v.__realUpdate || v.mixer.update.bind(v.mixer);
  v.mixer.update = () => v.mixer;
  return 'ok';
};
async function modelPoses(run, unit, size, results) {
  const dir = path.join(OUT, `model-${unit.replace(/[^a-z0-9_-]/gi, '_')}`, size.id);
  const t0 = Date.now();
  let last = t0;
  const rec = async (pose, fn) => {
    const file = path.join(dir, `electron-${pose}.png`);
    const r = { state: `model-${unit}`, pose, size: size.id, ok: false };
    try { await fn(); await parkMouse(run); await run.settle(); fs.mkdirSync(dir, { recursive: true }); await run.page.screenshot({ path: file, animations: 'disabled', caret: 'hide' }); r.ok = true; r.file = path.relative(ROOT, file); }
    catch (e) { r.error = errText(e); }
    finally { const now = Date.now(); r.ms = now - last; last = now; r.errors = run.errors.splice(0); results.push(r); }
  };
  try {
    await startSolo(run);
    await finishCarousel(run);
  } catch (e) {
    try { fs.mkdirSync(dir, { recursive: true }); await run.page.screenshot({ path: path.join(dir, 'electron-error.png') }); } catch { /* ignore */ }
    for (const pose of STATES.modelStates) results.push({ state: `model-${unit}`, pose, size: size.id, ok: false, ms: 0, error: `not reached: ${errText(e)}`, errors: [] });
    return;
  }
  const ok = await run.ev((id) => !!DT.LINE[id], unit);
  if (!ok) { for (const pose of STATES.modelStates) results.push({ state: `model-${unit}`, pose, size: size.id, ok: false, ms: 0, error: `unknown unit id ${unit} (not in DT.LINE)`, errors: [] }); return; }
  // shop portrait: every shop slot shows the unit, board empty
  await rec('portrait', async () => {
    await run.ev((id) => {
      const g = window.__digi.game, h = g.human;
      h.board = {}; h.bench.fill(null);
      // no PvE/opponent preview units on the board
      Object.defineProperty(window.__digi.ui, 'previewSide', { configurable: true, get: () => null, set() {} });
      h.shop = h.shop.map(() => id);
      window.__digi.ui.planLeft = 25.5;
      DT.UI.refresh();
    }, unit);
    await run.advance(500);
  });
  // board idle: a single 1-star copy in front-center
  await rec('idle', async () => {
    await run.ev((id) => {
      const g = window.__digi.game, h = g.human;
      const u = g.newUnit(id, 0);
      h.board = { '3,5': u }; h.bench.fill(null);
      window.__digi.ui.planLeft = 25.5;
      DT.UI.refresh();
    }, unit);
    await run.until(() => { const v = [...window.__digi.renderer.views.values()].find((x) => x.key.startsWith('u')); return v && v.hasModel && (v.mixer || v.hasModel === 'none'); }, null, 'unit model to load', 10000);
    const res = await run.ev(POSE, { pose: 'idle', t: 0 });
    if (res !== 'ok') throw new Error(res);
    await run.advance(32, 16);
  });
  for (const pose of ['attack', 'special', 'win']) {
    await rec(pose, async () => {
      const t = MODEL_TIMES[pose] != null ? MODEL_TIMES[pose] : 0.5;
      const res = await run.ev(POSE, { pose, t });
      if (res !== 'ok') throw new Error(res);
      await run.advance(32, 16);
    });
  }
}

// ---------------------------------------------------------------- main
(async () => {
  const tStart = Date.now();
  fs.mkdirSync(OUT, { recursive: true });
  const results = [];
  const server = await startServer();
  let browser;
  try {
    browser = await chromium.launch({ headless: true, args: ['--use-gl=angle', '--use-angle=swiftshader', '--mute-audio', '--autoplay-policy=no-user-gesture-required'] });
    const shotPath = (size) => (id) => path.join(OUT, id, size.id, 'electron.png');
    for (const size of SIZES) {
      const isolated = async (fn, opts) => {
        const run = await openRun(browser, size, opts);
        run.size = size;
        try { await fn(run); } finally { await run.context.close().catch(() => {}); }
      };
      // a local 4xx means a game file was missing while loading (for example during a checkout): retry once
      const broken = (rs) => rs.some((r) => !r.ok || (r.errors || []).some((e) => /^4\d\d http:\/\/localhost/.test(e)));
      const batch = async (fn, opts) => {
        fn.opts = opts;
        let rs = [];
        for (let attempt = 1; attempt <= 2; attempt++) {
          rs = [];
          await isolated((run) => fn(run, rs), fn.opts).catch((e) => { if (!rs.length) rs.push({ ok: false, error: String(e).split('\n')[0], errors: [] }); });
          for (const r of rs) r.attempt = attempt;
          if (!broken(rs)) break;
          if (attempt === 1) {
            const why = rs.map((r) => r.error || (r.errors || []).find((e) => /^4\d\d http:\/\/localhost/.test(e))).filter(Boolean)[0];
            console.log(`  retrying ${rs.map((r) => r.state)[0] || ''} ${size.id}: ${why}`);
            for (const r of rs) r.firstAttemptError = why;
          }
        }
        results.push(...rs);
        for (const r of rs) console.log(`${r.ok ? 'ok  ' : 'FAIL'} ${r.state}${r.pose ? ` ${r.pose}` : ''} ${size.id} ${r.ms} ms${r.attempt > 1 ? ' (retried)' : ''}${r.error ? ` ${r.error}` : ''}${r.errors && r.errors.length ? ` (${r.errors.length} page errors)` : ''}`);
      };
      for (const id of WANT.filter((s) => STATES.states.find((x) => x.id === s).group === 'client')) {
        await batch(async (run, rs) => {
          const t0 = Date.now();
          const r = { state: id, size: size.id, ok: false };
          rs.push(r);
          try {
            if (id === 'postgame') await postgame(run); else await clientState(run, id);
            await shoot(run, shotPath(size)(id));
            r.ok = true; r.file = path.relative(ROOT, shotPath(size)(id));
          } catch (e) {
            r.error = errText(e);
            try { await shoot(run, path.join(OUT, id, size.id, 'electron-error.png')); } catch { /* ignore */ }
          } finally { r.errors = run.errors.splice(0); r.ms = Date.now() - t0; }
        }, { steam: id === 'lobby' });
      }
      const matchWant = WANT.filter((s) => STATES.states.find((x) => x.id === s).group === 'match');
      if (matchWant.length) await batch((run, rs) => matchStates(run, matchWant, shotPath(size), rs));
      if (MODEL) await batch((run, rs) => modelPoses(run, MODEL, size, rs));
    }
  } finally {
    await browser?.close().catch(() => {});
    server.kill();
  }
  const report = {
    tool: 'tools/parity/capture-electron.cjs', createdAt: new Date().toISOString(), seed: SEED, fakeClockStart: new Date(START_TIME).toISOString(),
    profile: 'native/tests/fixtures/collection-profile.json (storage keys copied into localStorage)', renderer: 'chromium headless, ANGLE SwiftShader',
    sizes: SIZES.map((s) => s.id), states: WANT, model: MODEL, modelTimes: MODEL ? MODEL_TIMES : undefined,
    totalMs: Date.now() - tStart, ok: results.filter((r) => r.ok).length, failed: results.filter((r) => !r.ok).length, captures: results,
  };
  fs.writeFileSync(path.join(OUT, 'electron-report.json'), JSON.stringify(report, null, 2));
  console.log(`\n${report.ok} ok, ${report.failed} failed, ${(report.totalMs / 1000).toFixed(1)} s -> ${path.join(OUT, 'electron-report.json')}`);
  process.exitCode = report.failed ? 1 : 0;
})().catch((e) => { console.error(e); process.exitCode = 2; });
