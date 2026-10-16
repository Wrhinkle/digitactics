// Signature move check: every ability in DT.LINES has a valid move spec and every line a basic attack spec,
// mid-cast screenshots of a set of moves, and frame time during a 16-unit fight where everyone keeps casting.
// Run: node tests/moves.cjs <path-to-playwright> <outDir> [soft] [--set <id>]
// With --set <id> (not faction-wars): checks that set's own move and basic attack specs (js/sets/<id>-art.js), lists
// the abilities that use a generated look, and plays every one of its abilities once on the board.
const path = require('path');
const fs = require('fs');
const { spawn } = require('child_process');
const { argSet, stripSet } = require('../tools/headless.cjs');
const SET = argSet(process.argv);
process.argv = stripSet(process.argv);
const { chromium } = require(process.argv[2] || 'playwright');
const out = process.argv[3] || path.join(__dirname, 'moves-out');
const PORT = 8154;

// [ability, line, stage, ms into the move for the screenshot]
const SHOTS = [
  ['Nova Blast', 'agumon', 1, 520],
  ['Giga Destroyer', 'agumon', 2, 560],
  ['Howling Blaster', 'gabumon', 1, 450],
  ['Gate of Destiny', 'patamon', 2, 800],
  ["Heaven's Charm", 'salamon', 2, 520],
  ['Crimson Lightning', 'myotismon', 0, 420],
  ['Fire Rocket', 'imperialdramon', 0, 600],
  ["Vulcan's Hammer", 'gomamon', 2, 750],
  ['Electro Shocker', 'tentomon', 1, 640],
  ['Thunder Javelin', 'betamon', 2, 400],
  ['Talisman of Light', 'renamon', 2, 450],
  ['Hyper Mugen Cannon', 'machinedramon', 2, 680],
  ['Corona Destroyer', 'beelzemon', 2, 330],
  ['Grand Cross', 'lucemon', 0, 620],
  ['Pandemonium Lost', 'myotismon', 2, 800],
  ['Omni Sword', 'imperialdramon', 2, 520],
  ['Marching Fishes', 'gomamon', 0, 420],
  ['Flower Cannon', 'palmon', 2, 520],
  ['Ice Fist', 'elecmon', 2, 650],
  ['Giga Crusher', 'imperialdramon', 1, 720],
  ['Dragon Wheel', 'renamon', 1, 380],
  ['Needle Spray', 'palmon', 1, 420],
  ['Wing Blade', 'biyomon', 2, 480],
  ['Evil Wing', 'gizamon', 2, 520],
  ['Atomic Blaster', 'guilmon', 2, 520],
];

(async () => {
  fs.mkdirSync(out, { recursive: true });
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true });
  await new Promise((r) => setTimeout(r, 500));
  let browser;
  // the real GPU when there is one, so the frame times mean something; software GL otherwise (pass 'soft' to force it)
  const soft = process.argv[4] === 'soft';
  try { browser = await chromium.launch({ args: soft ? ['--use-gl=angle', '--use-angle=swiftshader'] : ['--use-angle=d3d11', '--enable-gpu', '--ignore-gpu-blocklist'] }); }
  catch { browser = await chromium.launch({ channel: 'msedge' }); }
  const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
  const errors = [];
  page.on('pageerror', (e) => errors.push(String(e)));
  page.on('console', (m) => { if (m.type() === 'error') errors.push(m.text()); });
  let fail = 0;
  try {
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForTimeout(2000);
    console.log('gpu:', await page.evaluate(() => { const g = document.createElement('canvas').getContext('webgl2'); const e = g && g.getExtension('WEBGL_debug_renderer_info'); return e ? g.getParameter(e.UNMASKED_RENDERER_WEBGL) : 'unknown'; }));

    if (SET && SET !== 'faction-wars') {
      const r = await page.evaluate(async (set) => {
        const DT = window.DT, info = DT.MoveFxInfo, M = DT.Moves;
        DT.UI.newGame(set);
        const S = DT.SET;
        const errs = [], generated = [];
        for (const [n, sp] of Object.entries(S.art.moves || {})) errs.push(...M.validateSpec(n, sp, info));
        for (const [id, b] of Object.entries(S.art.basic || {})) errs.push(...M.validateBasic(id, b, info));
        for (const l of DT.LINES) for (const a of l.abil) { const sp = M.specFor(a, l); if (sp.fallback) generated.push(a); errs.push(...M.validateSpec(a, sp, info)); }
        // every ability and basic attack once in a paused timeline
        const rr = window.__digi.renderer;
        const fx = rr.moves.fx;
        fx.paused = true;
        for (const l of DT.LINES) for (let st = 0; st < 3; st++) {
          try {
            rr.moves.demo(l.abil[st], st, rr.combatCell(3, 6), [rr.combatCell(3, 1), rr.combatCell(2, 2)]);
            fx.advance(1.6, 1 / 30);
            rr.moves.basic(l, st, rr.combatCell(3, 6), rr.combatCell(3, 2), 1.4, true, true);
            fx.advance(0.5, 1 / 30);
          } catch (e) { errs.push(`${l.abil[st]}: ${e.message}`); }
        }
        fx.clear();
        fx.paused = false;
        return { set: S.id, errs, generated: [...new Set(generated)], lines: DT.LINES.length };
      }, SET);
      console.log(`set ${r.set}: ${r.lines} lines; generated looks for: ${r.generated.join(', ') || 'none'}`);
      if (r.errs.length) { fail++; console.log('SPEC ERRORS:\n  ' + r.errs.join('\n  ')); } else console.log('spec check: ok');
      if (errors.length) { fail++; console.log('page errors:', errors.slice(0, 5).join(' | ')); }
      await browser.close();
      server.kill();
      console.log(fail ? 'FAIL' : 'PASS');
      process.exit(fail ? 1 : 0);
    }

    // 1. specs
    const check = await page.evaluate(() => {
      const DT = window.DT, info = DT.MoveFxInfo, M = DT.Moves;
      const errs = [], names = new Set();
      for (const l of DT.LINES) {
        for (const a of l.abil) {
          names.add(a);
          const s = DT.MOVES[a];
          if (!s) errs.push(`${l.id}: no spec for ${a}`);
          else errs.push(...M.validateSpec(a, s, info));
        }
        const b = DT.MOVE_BASIC[l.id];
        if (!b) errs.push(`${l.id}: no basic attack spec`);
        else errs.push(...M.validateBasic(l.id, b, info));
      }
      // fallbacks for lines that do not exist yet
      for (const type of ['nuke', 'aoe', 'stun', 'slam', 'shield', 'heal', 'barrage', 'drain', 'dash']) {
        for (const nm of ['Mega Flame', 'Frozen Claw', 'Thunder Laser', 'Holy Arrow', 'Shadow Wing', 'Rock Smash']) {
          errs.push(...M.validateSpec(`fallback ${nm}/${type}`, M.fallbackSpec(nm, type), info));
        }
      }
      errs.push(...M.validateBasic('fallback', M.basicFor({ id: 'newline', abil: ['Mega Flame'], type: 'nuke' }, true), info));
      const unused = Object.keys(DT.MOVES).filter((n) => !names.has(n));
      return { errs, moves: names.size, specs: Object.keys(DT.MOVES).length, unused, lines: DT.LINES.length };
    });
    console.log(`specs: ${check.moves} abilities across ${check.lines} lines, ${check.specs} specs, unused: ${check.unused.join(', ') || 'none'}`);
    if (check.errs.length) { fail++; console.log('SPEC ERRORS:\n  ' + check.errs.join('\n  ')); } else console.log('spec check: ok');

    // 2. game: carousel, then planning for 1-2
    await page.click('#startBtn');
    await page.waitForTimeout(500);
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(800);

    // every move once in a paused timeline, with the caster on the board (no exceptions allowed)
    const allErr = await page.evaluate(() => {
      const r = window.__digi.renderer, DT = window.DT;
      const fx = r.moves.fx;
      const errs = [];
      fx.paused = true;
      for (const l of DT.LINES) for (let st = 0; st < 3; st++) {
        try {
          r.moves.demo(l.abil[st], st, r.combatCell(3, 6), [r.combatCell(3, 1), r.combatCell(2, 2)]);
          fx.advance(1.6, 1 / 30);
          r.moves.basic(l, st, r.combatCell(3, 6), r.combatCell(3, 5), 1.4, false, false);
          r.moves.basic(l, st, r.combatCell(3, 6), r.combatCell(3, 2), 1.4, true, true);
          fx.advance(0.5, 1 / 30);
        } catch (e) { errs.push(`${l.abil[st]}: ${e.message}`); }
      }
      fx.clear();
      fx.paused = false;
      return errs;
    });
    if (allErr.length) { fail++; console.log('DEMO ERRORS:\n  ' + allErr.join('\n  ')); } else console.log('all moves played without errors');

    // 3. mid-cast screenshots
    const box = await page.locator('#board').boundingBox();
    for (const [name, line, stage, ms] of SHOTS) {
      const clip = await page.evaluate(({ name, line, stage, ms }) => {
        const g = window.__digi.game, r = window.__digi.renderer, p = g.human;
        // caster on the board so its model is in the picture
        for (const k of Object.keys(p.board)) delete p.board[k];
        p.board['3,6'] = g.newUnit(line, stage);
        return new Promise((res) => setTimeout(() => {
          const fx = r.moves.fx;
          fx.clear();
          fx.paused = true;
          const from = r.combatCell(3, 6);
          const targets = [r.combatCell(3, 1), r.combatCell(1, 2), r.combatCell(5, 2)];
          const view = [...r.views.values()].find((v) => Math.hypot(v.group.position.x - from.x, v.group.position.z - from.z) < 0.3);
          if (view) view.play('special');
          r.moves.demo(name, stage, from, targets, { view });
          fx.advance(ms / 1000);
          const a = r.screenOfCell(1, 0), b = r.screenOfCell(5, 7);
          res({ x: Math.min(a.x, b.x) - 160, y: a.y - 200, w: Math.abs(b.x - a.x) + 320, h: b.y - a.y + 300 });
        }, 700));
      }, { name, line, stage, ms });
      await page.waitForTimeout(250);
      const file = path.join(out, `${name.replace(/[^a-z0-9]+/gi, '_').toLowerCase()}.png`);
      await page.screenshot({ path: file, clip: { x: box.x + Math.max(0, clip.x), y: box.y + Math.max(0, clip.y), width: Math.min(clip.w, box.width), height: Math.min(clip.h, box.height) } });
      console.log('shot', path.basename(file));
    }
    await page.evaluate(() => { const r = window.__digi.renderer; r.moves.fx.clear(); r.moves.fx.paused = false; });

    // 4. fixed-step benchmark: 16 Mega moves fired together every half second plus basic attacks, stepped at 60 fps,
    // each frame timed through render and gl.finish. Compared with the same frames and no effects.
    const bench = await page.evaluate(() => {
      const r = window.__digi.renderer, DT = window.DT, fx = r.moves.fx, gl = r.r.getContext();
      fx.clear(); fx.paused = true;
      const lines = DT.LINES.filter((l) => l.shop).slice(-16);
      const ours = [[1, 4], [3, 4], [5, 4], [0, 5], [2, 5], [4, 5], [6, 5], [3, 6]], theirs = [[1, 3], [3, 3], [5, 3], [0, 2], [2, 2], [4, 2], [6, 2], [3, 1]];
      const run = (casts) => {
        const times = [];
        let peak = 0;
        for (let i = 0; i < 240; i++) {
          const t = performance.now();
          if (casts && i % 30 === 0) lines.forEach((l, k) => {
            const a = k < 8 ? ours[k] : theirs[k - 8], b = k < 8 ? theirs[k] : ours[k - 8];
            r.moves.demo(l.abil[2], 2, r.combatCell(a[0], a[1]), [r.combatCell(b[0], b[1])]);
          });
          if (casts && i % 4 === 0) lines.forEach((l, k) => {
            const a = k < 8 ? ours[k] : theirs[k - 8], b = k < 8 ? theirs[k] : ours[k - 8];
            r.moves.basic(l, 2, r.combatCell(a[0], a[1]), r.combatCell(b[0], b[1]), 1.8, false, l.abil && DT.ROLES && (l.traits.includes('Sniper') || l.traits.includes('Mystic')));
          });
          fx.step(1 / 60);
          r.r.render(r.scene, r.camera);
          gl.finish();
          times.push(performance.now() - t);
          peak = Math.max(peak, fx.add.n + fx.alpha.n);
        }
        times.sort((x, y) => x - y);
        return { avg: times.reduce((x, y) => x + y, 0) / times.length, p95: times[Math.floor(times.length * 0.95)], peak };
      };
      run(false); // warm up
      const off = run(false), on = run(true), off2 = run(false), on2 = run(true);
      fx.clear(); fx.paused = false;
      return { off: (off.avg + off2.avg) / 2, on: (on.avg + on2.avg) / 2, offP95: Math.max(off.p95, off2.p95), onP95: Math.max(on.p95, on2.p95), peak: Math.max(on.peak, on2.peak) };
    });
    console.log(`fixed-step benchmark (16 Mega casts every 0.5 s + 16 basic attacks every 4 frames, render + gl.finish per frame):`);
    console.log(`  with effects ${bench.on.toFixed(2)} ms avg (p95 ${bench.onP95.toFixed(2)}), without ${bench.off.toFixed(2)} ms avg (p95 ${bench.offP95.toFixed(2)}), peak particles ${bench.peak}`);

    // 5. big fight: 8 v 8 Mega forms, everyone at full mana all the time
    const ours = [['imperialdramon', 2], ['machinedramon', 2], ['lucemon', 2], ['myotismon', 2], ['beelzemon', 2], ['agumon', 2], ['patamon', 2], ['gomamon', 2]];
    const theirs = [['tentomon', 2], ['gabumon', 2], ['guilmon', 2], ['biyomon', 2], ['renamon', 2], ['palmon', 2], ['betamon', 2], ['candlemon', 2]];
    await page.evaluate(({ ours, theirs }) => {
      const g = window.__digi.game, p = g.human;
      p.level = 9;
      for (const k of Object.keys(p.board)) delete p.board[k];
      const cells = [[1, 4], [3, 4], [5, 4], [0, 5], [2, 5], [4, 5], [6, 5], [3, 6]];
      ours.forEach(([l, s], i) => { p.board[cells[i].join(',')] = g.newUnit(l, s); });
      g.pveSide = () => ({ units: theirs.map(([line, stage], i) => ({ line, stage, c: cells[i][0], r: cells[i][1], uid: -1 - i })), noTraits: true });
    }, { ours, theirs });
    for (let i = 0; i < 40; i++) { // models load
      await page.waitForTimeout(500);
      if (i > 3 && await page.evaluate(() => [...window.__digi.renderer.views.values()].every((v) => v.hasModel))) break;
    }
    await page.keyboard.press('Space');
    await page.waitForTimeout(300);
    const perfP = page.evaluate(() => new Promise((res) => {
      const g = window.__digi.game, r = window.__digi.renderer;
      const m = r.moves;
      const orig = m.update.bind(m), realFlush = m.flush.bind(m), realBasic = m.basic.bind(m);
      // move effects on or off under the same load; segments alternate so other load on the machine averages out
      const setOn = (on) => { m.flush = on ? realFlush : () => { m.pending = null; }; m.basic = on ? realBasic : () => {}; };
      let fxTime = 0, fxN = 0, fxMax = 0, maxParts = 0, maxTasks = 0;
      m.update = (dt) => { const t = performance.now(); orig(dt); const d = performance.now() - t; fxTime += d; fxN++; fxMax = Math.max(fxMax, d); maxParts = Math.max(maxParts, m.fx.add.n + m.fx.alpha.n); maxTasks = Math.max(maxTasks, m.fx.tasks.length); };
      const pump = setInterval(() => {
        const cb = g.humanMatch && g.humanMatch.combat;
        if (!cb) return;
        // near-immune and full mana, so all 16 keep casting for the whole measurement (test only)
        for (const u of cb.units) if (u.alive) { u.armor = u.mr = 1e5; u.hp = u.maxHp; if (u.maxMana > 0) u.mana = u.maxMana; }
      }, 50);
      const SEG = 2500, order = [true, false, true, false];
      const fr = { true: [], false: [] };
      const cb0 = g.humanMatch && g.humanMatch.combat;
      const ev0 = cb0 ? cb0.events.length : 0;
      let seg = 0, segT = performance.now(), last = segT;
      setOn(order[0]);
      const tick = (now) => {
        if (now - segT > 150) fr[order[seg]].push(now - last); // skip the switch-over
        last = now;
        if (now - segT > SEG) { seg++; segT = now; if (seg < order.length) setOn(order[seg]); }
        if (seg < order.length) { requestAnimationFrame(tick); return; }
        clearInterval(pump);
        m.update = orig; setOn(true);
        const stat = (a) => { a.sort((x, y) => x - y); return { n: a.length, avg: a.reduce((x, y) => x + y, 0) / Math.max(1, a.length), p95: a[Math.floor(a.length * 0.95)] || 0, max: a[a.length - 1] || 0 }; };
        const cbx = g.humanMatch && g.humanMatch.combat;
        res({ on: stat(fr.true), off: stat(fr.false), castEvents: cbx ? cbx.events.slice(ev0).filter((e) => e.type === 'cast').length : -1, done: !!(cbx && cbx.done),
          fxAvg: fxTime / Math.max(1, fxN), fxMax, maxParts, maxTasks, pooledMeshes: Object.values(m.fx.pools).reduce((n, q) => n + q.length, 0), ribbons: m.fx.ribbons.length });
      };
      requestAnimationFrame(tick);
    }));
    await page.waitForTimeout(700);
    await page.screenshot({ path: path.join(out, 'fight-16-casts.png') }); // everyone's first cast
    const perf = await perfP;
    await page.screenshot({ path: path.join(out, 'big-fight.png') });
    const f = (x) => `avg ${x.avg.toFixed(1)} ms, p95 ${x.p95.toFixed(1)} ms, max ${x.max.toFixed(1)} ms (${x.n} frames)`;
    console.log(`big fight, 16 units casting nonstop (${perf.castEvents} casts in 10 s${perf.done ? ', ENDED EARLY' : ''}):`);
    console.log(`  frame time with move effects:    ${f(perf.on)}`);
    console.log(`  frame time without move effects: ${f(perf.off)}`);
    console.log(`  move fx CPU per frame: avg ${perf.fxAvg.toFixed(2)} ms, max ${perf.fxMax.toFixed(2)} ms | peak particles ${perf.maxParts}, peak tasks ${perf.maxTasks} | pooled meshes ${perf.pooledMeshes}, ribbons ${perf.ribbons}`);
  } catch (e) {
    fail++;
    console.log('TEST ERROR', e);
  }
  console.log('errors:', errors.length ? errors.slice(0, 10) : 'none');
  if (errors.length) fail++;
  await browser.close();
  server.kill();
  console.log(fail ? 'FAIL' : 'PASS');
  process.exit(fail ? 1 : 0);
})();
