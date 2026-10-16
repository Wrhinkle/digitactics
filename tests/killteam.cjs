// Kill screen team lineup and elimination sounds: the 'elimsound' catalog and loadout, a kill screen with a full
// 9-Digimon board (lineup count, every member dancing, static models on procedural motion), the elimination sound
// through the DT.Sfx counters (yours, other players' muted by "mute others", the default for older clients),
// GPU memory flat over 3 kill screens, the Collection's Elim Sounds tab (preview, equip) and the kill screen
// preview with the lineup and its play button. Screenshots of the big and small versions.
// Run: node tests/killteam.cjs <path-to-playwright> <outDir>
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || __dirname;
const PORT = Number(process.env.PORT) || 8191;
fs.mkdirSync(out, { recursive: true });

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  const http = require('http');
  for (let i = 0; i < 40; i++) {
    const up = await new Promise((r) => http.get(`http://localhost:${PORT}/`, (res) => { res.resume(); r(true); }).on('error', () => r(false)));
    if (up) break;
    await new Promise((r) => setTimeout(r, 250));
  }
  const args = ['--use-gl=angle', '--use-angle=swiftshader', '--autoplay-policy=no-user-gesture-required'];
  let browser;
  try { browser = await chromium.launch({ args }); }
  catch { browser = await chromium.launch({ channel: 'msedge', args: args.slice(2) }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error' && !/Failed to load resource/.test(m.text())) errors.push(m.text()); });
  const fail = [];
  const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fail.push(msg); };
  const g = (fn, arg) => page.evaluate(fn, arg);
  const drain = async () => { for (let i = 0; i < 20 && (await g(() => !!DT.KillScreens.active || DT.KillScreens.queued > 0)); i++) { await g(() => DT.KillScreens.skip()); await page.waitForTimeout(60); } };
  const until = async (fn, arg, ms = 8000) => { const t0 = Date.now(); for (;;) { const v = await g(fn, arg); if (v || Date.now() - t0 > ms) return v; await page.waitForTimeout(100); } };

  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);
    await page.mouse.click(5, 5); // a user gesture, so the audio context starts

    // ---------- catalog and loadout ----------
    const cat = await g(() => DT.Cosmetics.list('elimsound').map((x) => ({ id: x.id, name: x.name, free: !!x.free, price: x.price, rarity: x.rarity, linked: x.linked || null, egg: x.egg })));
    const stingers = cat.filter((x) => !x.linked);
    const linked = cat.filter((x) => x.linked);
    check(await g(() => DT.Cosmetics.KINDS.includes('elimsound') && !!DT.Cosmetics.loadout().elimsound), `elimsound is a cosmetic kind and in the loadout (${await g(() => DT.Cosmetics.loadout().elimsound)})`);
    check(stingers.length >= 8 && stingers.length <= 12, `${stingers.length} new elimination stingers`);
    check(stingers.filter((x) => x.free).length >= 2 && stingers.filter((x) => x.free).length <= 3, `${stingers.filter((x) => x.free).length} free stingers`);
    const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };
    check(stingers.every((x) => x.free || x.price === PRICE[x.rarity]), 'paid stingers are priced by rarity');
    for (const x of stingers) console.log(`     ${x.name.padEnd(20)} ${x.free ? 'free' : `${x.rarity} ${x.price}`}`);
    const sounds = await g(() => DT.Cosmetics.list('sound').length);
    check(linked.length === sounds && linked.every((x) => x.price === 0 && x.egg === false), `every soundboard clip (${linked.length}/${sounds}) is an elimination sound too, not sold or rolled on its own`);
    check(await g(() => DT.Cosmetics.list('elimsound').filter((x) => x.linked).every((x) => DT.Cosmetics.owns('elimsound', x.id) === DT.Cosmetics.owns('sound', x.linked[1]))), 'a soundboard clip is owned as an elimination sound exactly when it is owned on the soundboard');
    check(await g(() => DT.ElimSounds.resolve(undefined) === DT.ElimSounds.DEFAULT && DT.ElimSounds.resolve('es_no_such') === DT.ElimSounds.DEFAULT), `older clients without one fall back to ${await g(() => DT.ElimSounds.DEFAULT)}`);

    // every sound builds and plays without an error
    let played = 0;
    for (const x of cat) {
      if (x.linked) { if (await g((id) => DT.Sfx.hasClip(DT.Cosmetics.get('elimsound', id).data.clip), x.id)) played++; continue; }
      if (await g((id) => DT.ElimSounds.play(id, { mine: true, preview: true }), x.id)) played++;
      await page.waitForTimeout(250);
    }
    check(played === cat.length, `all ${played}/${cat.length} elimination sounds play (soundboard clips: the clip exists)`);

    // ---------- Collection: Elim Sounds tab ----------
    await page.click('#cnav .cn-tab[data-page="collection"]');
    await page.waitForTimeout(500);
    await page.click('#pgCollection [data-ctab="elimsound"]');
    await page.waitForTimeout(400);
    const cards = await page.locator('#pgCollection .ccard').count();
    check(cards === cat.length, `Collection Elim Sounds tab lists ${cards}/${cat.length}`);
    const free2 = stingers.filter((x) => x.free)[1];
    await page.click(`#pgCollection .ccard[data-csel="${free2.id}"] .ccard-name`); // the middle of the card is its play button
    await page.waitForTimeout(300);
    const pv0 = await g(() => DT.ElimSounds.stats.preview);
    await page.click('#pgCollection .cprev [data-elim-play]');
    await page.waitForTimeout(200);
    check(await g((n) => DT.ElimSounds.stats.preview > n, pv0), 'the preview play button plays it');
    await page.click(`#pgCollection [data-cequip="${free2.id}"]`);
    await page.waitForTimeout(300);
    check(await g((id) => DT.Cosmetics.equipped('elimsound') === id && DT.Cosmetics.loadout().elimsound === id, free2.id), `equipped ${free2.name} from the Collection (and it is in the loadout)`);
    await page.screenshot({ path: path.join(out, 'kt-collection-elimsound.png') });
    // a locked soundboard clip points at the soundboard store
    const lockedSb = await g(() => { const it = DT.Cosmetics.list('elimsound').find((x) => x.linked && !DT.Cosmetics.owns('elimsound', x.id)); return it && it.id; });
    if (lockedSb) {
      await page.click(`#pgCollection .ccard[data-csel="${lockedSb}"] .ccard-name`);
      await page.waitForTimeout(300);
      check(await page.locator('#pgCollection .cprev [data-shop-open="sound"]').count() === 1, 'a locked soundboard clip links to the Soundboard store');
    }
    // the Store tab sells only the stingers
    await g(() => DT.Shop.open('elimsound'));
    await page.waitForTimeout(500);
    const storeCards = await page.locator('#dtStore .dts-body > section:not(.dts-feat) .dts-card').count();
    check(storeCards === stingers.length, `Store Elim Sounds tab sells the ${storeCards} stingers`);
    await page.screenshot({ path: path.join(out, 'kt-store-elimsound.png') });
    await g(() => DT.Shop.close());

    // kill screen preview in the Collection: the lineup with a sample team (no game running) and a play button
    await page.click('#pgCollection [data-ctab="killscreen"]');
    await page.waitForTimeout(400);
    await page.click('#pgCollection .ccard[data-csel="ks_confetti"]');
    const lined = await until(() => { const c = document.querySelector('#pgCollection .cprev-big canvas.ksPreview'); return c && c.dataset.lineup === '1'; }, null, 15000);
    check(!!lined, 'the Collection kill screen preview shows the team lineup');
    await page.waitForTimeout(2500);
    const pinfo = await g(() => DT.KillScreens.lineupInfo(true));
    check(!!pinfo && pinfo.members.filter((m) => !m.legend).length === (await g(() => DT.KillScreens.sampleTeam().length)), `preview lineup: ${pinfo ? pinfo.members.length : 0} members (sample team ${await g(() => DT.KillScreens.sampleTeam().length)} + legend)`);
    const el0 = await g(() => DT.Sfx.stats.elim || 0);
    await page.click('#pgCollection .cprev-big .ksPrevPlay');
    await page.waitForTimeout(400);
    check(await g((n) => (DT.Sfx.stats.elim || 0) > n, el0), 'the preview play button plays your elimination sound');
    await page.locator('#pgCollection .cprev-big').screenshot({ path: path.join(out, 'kt-preview-lineup.png') });

    // ---------- a solo game with a full 9-Digimon board ----------
    await page.click('#cnav .cn-tab[data-page="home"]').catch(() => {});
    await page.waitForTimeout(300);
    await g(() => document.getElementById('startBtn').click());
    await page.waitForTimeout(800);
    await g(() => document.getElementById('timerBtn').click());
    // a team with the static rips (no skeleton) in it when the set has any
    const team = await g(async () => {
      const cand = [];
      const seen = new Set();
      const add = (l, s) => { const k = DT.modelKey(l, s); if (k && !seen.has(k)) { seen.add(k); cand.push({ line: l.id, stage: s, key: k }); } };
      for (const l of DT.LINES.filter((x) => x.forms && x.shop !== false)) for (let s = 0; s < Math.min(3, l.forms.length); s++) add(l, s);
      // static rips (DW1, Time Stranger, Masters) from any set, as long as the line id resolves to that line here
      const extra = [];
      for (const id of DT.Sets.ids()) for (const l of (DT.Sets.get(id) || {}).LINES || []) if (DT.LINE[l.id] === l || (DT.LINE[l.id] && DT.LINE[l.id].id === l.id)) extra.push(DT.LINE[l.id]);
      const before = cand.length;
      for (const l of extra) for (let s = 0; s < Math.min(3, (l.forms || []).length); s++) add(l, s);
      const cov = await DT.Dances.coverage(cand.map((c) => `unit:${c.key}`));
      cand.forEach((c, i) => { c.bones = cov[i].missing ? -1 : cov[i].bones; c.extra = i >= before; });
      const ok = cand.filter((c) => c.bones >= 0);
      const stat = ok.filter((c) => c.bones === 0);
      const rig = ok.filter((c) => c.bones > 0 && !c.extra);
      const pick = [];
      const used = new Set();
      for (const c of stat.slice(0, 2).concat(rig.filter((_, i) => i % 7 === 0))) { if (pick.length >= 9 || used.has(c.line)) continue; used.add(c.line); pick.push(c); }
      return pick;
    });
    console.log(`     team: ${team.map((u) => `${u.key}${u.bones === 0 ? ' (static)' : ''}`).join(', ')}`);
    check(team.length === 9, `built a 9-Digimon team (${team.filter((u) => u.bones === 0).length} static)`);
    await g((tm) => {
      const G = window.__digi.game;
      for (const k of Object.keys(G.human.board)) delete G.human.board[k];
      tm.forEach((u, i) => { G.human.board[`${i % 7},${3 + Math.floor(i / 7)}`] = G.newUnit(u.line, u.stage); });
    }, team);
    const boardN = await g(() => window.__digi.game.human.boardUnits().length);
    check(boardN === 9, `the human's board has ${boardN} Digimon`);

    const base = await g(() => new Promise((res) => { let n = 0; const t0 = performance.now(); const f = () => { n++; if (performance.now() - t0 < 1000) requestAnimationFrame(f); else res(Math.round((n * 1000) / (performance.now() - t0))); }; requestAnimationFrame(f); }));
    console.log(`     the game alone: ${base} frames a second (software GL in this test)`);
    const playOne = async (variant, shotName) => {
      const before = await g(() => ({ elim: DT.Sfx.stats.elim || 0, played: DT.ElimSounds.stats.played }));
      await g((v) => { const G = window.__digi.game; DT.KillScreens.demo({ killer: G.human, victim: G.players.find((p) => p !== G.human), variant: v }); }, variant);
      const loaded = await until(() => { const a = DT.KillScreens.active; return a && a.rigs >= a.expected; }, null, 3800);
      const act = await g(() => DT.KillScreens.active);
      let moving = null;
      if (act) {
        // two samples a few frames apart (wait on kill screen time: software GL under load can be very slow)
        const a = await g(() => ({ ...DT.KillScreens.lineupInfo(), t: DT.KillScreens.active && DT.KillScreens.active.t }));
        await until((t0) => !DT.KillScreens.active || DT.KillScreens.active.t > t0 + 0.12, a.t, 8000);
        const b = await g(() => DT.KillScreens.lineupInfo());
        moving = a && b ? b.members.map((m, i) => ({ key: m.key, procedural: m.procedural, bones: m.bones, moved: a.members[i] && Math.abs(m.sig - a.members[i].sig) > 1e-5 })) : null;
      }
      if (shotName) {
        await until(() => DT.KillScreens.active && DT.KillScreens.active.t > 1.5, null, 3000);
        await page.screenshot({ path: path.join(out, shotName) });
      }
      const fps = await g(() => new Promise((res) => { let n = 0; const t0 = performance.now(); const f = () => { n++; if (performance.now() - t0 < 1000 && DT.KillScreens.active) requestAnimationFrame(f); else res(Math.round((n * 1000) / (performance.now() - t0))); }; requestAnimationFrame(f); }));
      console.log(`     ${variant}: ${fps} frames a second with ${act ? act.rigs : 0} models (software GL in this test)`);
      const after = await g(() => ({ elim: DT.Sfx.stats.elim || 0, played: DT.ElimSounds.stats.played }));
      // software GL runs slower than real time (frame steps are clamped), so allow plenty
      await until(() => !DT.KillScreens.active, null, 30000);
      return { act, loaded, moving, sound: after.elim > before.elim && after.played > before.played };
    };

    // first kill screen: loads the templates
    const r1 = await playOne('ks_confetti', 'kt-team-9.png');
    check(!!r1.act && r1.act.team === boardN, `kill screen team = the killer's board (${r1.act && r1.act.team}/${boardN})`);
    check(!!r1.loaded, `every lineup member loaded during the kill screen (${r1.act && r1.act.rigs}/${r1.act && r1.act.expected})`);
    const info = r1.moving || [];
    const units = info.filter((m) => m.key !== 'legend');
    check(units.length === boardN, `lineup shows ${units.length} Digimon for ${boardN} on the board, plus ${info.length - units.length} legend`);
    check(info.length > 0 && info.every((m) => m.moved), `every member is dancing (${info.filter((m) => m.moved).length}/${info.length} moved; ${info.filter((m) => m.procedural).length} on procedural motion)`);
    check(info.filter((m) => m.procedural).length >= team.filter((u) => u.bones === 0).length, 'static models dance with procedural motion');
    check(r1.sound && r1.act.elimPlayed && r1.act.elimsound === free2.id, `your elimination sound played (${r1.act && r1.act.elimsound})`);
    const mem1 = await g(() => DT.KillScreens.memory());
    // two more, then the GPU memory of the kill screen renderer must be where it was after the first
    for (const v of ['ks_gate', 'ks_mega']) await playOne(v, v === 'ks_mega' ? 'kt-team-9-mega.png' : null);
    const mem3 = await g(() => DT.KillScreens.memory());
    console.log(`     memory after 1: ${JSON.stringify(mem1.live)} after 3: ${JSON.stringify(mem3.live)} scene children ${mem1.sceneChildren} -> ${mem3.sceneChildren}`);
    check(mem3.live.geometries <= mem1.live.geometries && mem3.live.textures <= mem1.live.textures, 'no geometry or texture growth after 3 kill screens');
    check(mem3.sceneChildren === mem1.sceneChildren, 'everything is removed from the scene after each kill screen');

    // an uninvolved viewer: the same lineup, small (an AI Tamer with six of the team)
    await drain();
    await g((tm) => { const G = window.__digi.game; const ai = G.players.filter((p) => !p.isHuman)[0]; for (const k of Object.keys(ai.board)) delete ai.board[k]; tm.slice(3).forEach((u, i) => { ai.board[`${i},3`] = G.newUnit(u.line, u.stage); }); }, team);
    await g(() => { const G = window.__digi.game; const ai = G.players.filter((p) => !p.isHuman); DT.KillScreens.demo({ killer: ai[0], victim: ai[1], variant: 'ks_stream' }); });
    await until(() => { const a = DT.KillScreens.active; return a && a.rigs >= a.expected && a.t > 1.4; }, null, 4000);
    const small = await g(() => DT.KillScreens.active);
    const aiBoard = await g(() => window.__digi.game.players.filter((p) => !p.isHuman)[0].boardUnits().length);
    check(!!small && !small.big && small.team === aiBoard, `uninvolved viewer: small version with the killer's ${small && small.team}/${aiBoard} Digimon`);
    await page.screenshot({ path: path.join(out, 'kt-team-small.png') });
    await drain();

    // other players' elimination sounds follow "mute other players' soundboard"
    await drain();
    await g(() => DT.Sfx.set({ muteOthers: true }));
    const m0 = await g(() => ({ muted: DT.ElimSounds.stats.muted, elim: DT.Sfx.stats.elim || 0 }));
    await g(() => { const G = window.__digi.game; const ai = G.players.filter((p) => !p.isHuman); DT.KillScreens.demo({ killer: ai[1], victim: ai[2] }); });
    await page.waitForTimeout(300);
    const m1 = await g(() => ({ muted: DT.ElimSounds.stats.muted, elim: DT.Sfx.stats.elim || 0 }));
    check(m1.muted === m0.muted + 1 && m1.elim === m0.elim, 'another player\'s elimination sound is muted by "mute other players\' soundboard"');
    await drain();
    const own = await g(() => { const s = DT.Sfx.stats.elim || 0; const G = window.__digi.game; DT.KillScreens.demo({ killer: G.human, victim: G.players[1] }); return s; });
    await page.waitForTimeout(300);
    check(await g((s) => (DT.Sfx.stats.elim || 0) > s, own), 'your own elimination sound still plays with others muted');
    await drain();
    await g(() => DT.Sfx.set({ muteOthers: false }));
    // Mute all silences it
    await g(() => DT.Sfx.set({ muted: true }));
    const mu = await g(() => { const s = DT.Sfx.stats.elim || 0; const G = window.__digi.game; DT.KillScreens.demo({ killer: G.human, victim: G.players[1] }); return s; });
    await page.waitForTimeout(300);
    check(await g((s) => (DT.Sfx.stats.elim || 0) === s, mu), 'Mute all silences elimination sounds');
    await drain();
    await g(() => DT.Sfx.set({ muted: false }));

    // an older client's loadout (no elimsound) gets the default
    const old = await g(() => {
      const G = window.__digi.game;
      const ai = G.players.filter((p) => !p.isHuman)[3];
      const prevNet = DT.Social.net;
      ai.isHuman = true;
      DT.Social.net = { lookOf: () => ({ legend: DT.Cosmetics.list('legend')[0].id, killscreen: 'ks_gate', dance: 'floss', emote: [], sound: [] }), send() {} };
      DT.KillScreens.demo({ killer: ai, victim: G.players.find((p) => p !== ai && p !== G.human) });
      const a = DT.KillScreens.active;
      ai.isHuman = false;
      DT.Social.net = prevNet;
      DT.KillScreens.skip();
      return a && a.elimsound;
    });
    check(old === (await g(() => DT.ElimSounds.DEFAULT)), `an older client's loadout without an elimination sound plays the default (${old})`);
  } catch (e) {
    check(false, `exception: ${e.stack || e}`);
  }

  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  console.log(fail.length ? `FAILED: ${fail.length}` : 'all kill screen team checks passed');
  await browser.close();
  server.kill();
  process.exit(fail.length || errors.length ? 1 : 0);
})();
