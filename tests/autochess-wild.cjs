// Exercise Chaos-selected Auto Chess wild rounds through the real planning/combat UI.
// node tests/autochess-wild.cjs <playwright> <evidence-dir>
const { chromium } = require(process.argv[2] || 'playwright');
const { spawn } = require('child_process');
const fs = require('fs'), path = require('path'), assert = require('assert/strict');
const out = path.resolve(process.argv[3]); fs.mkdirSync(out, { recursive: true });
(async () => {
  const server = spawn(process.execPath, ['serve.cjs'], { cwd: path.join(__dirname, '..'), env: { ...process.env, PORT: '8205' }, windowsHide: true, stdio: 'ignore' });
  let browser;
  try {
    await new Promise(r => setTimeout(r, 500));
    browser = await chromium.launch({ args: ['--use-angle=d3d11'] });
    const page = await browser.newPage({ viewport: { width: 1440, height: 900 } }), errors = [], results = [];
    page.on('pageerror', e => errors.push(String(e)));
    await page.goto('http://localhost:8205'); await page.waitForSelector('#boot', { state: 'detached' });
    const waves = [
      [1, 1, 'ac_w_numemon', 1], [1, 2, 'ac_w_numemon', 1], [1, 3, 'ac_w_numemon', 1],
      [2, null, 'ac_w_blackgarurmon', 2], [3, null, 'ac_w_fugamon', 2],
      [4, null, 'ac_w_tyrannomon', 2], [5, null, 'ac_w_monochromon', 2],
      [6, null, 'ac_w_airdramon', 3], [7, null, 'ac_w_mammothmon', 3],
      [8, null, 'ac_w_goldguardromon', 3], [9, null, 'ac_w_kimeramon', 3],
      [10, null, 'ac_w_machinedramon', 3],
    ];
    for (const pace of ['hyper', 'normal']) for (const [stage, openingRound, first, maxStars] of waves) {
      const round = openingRound || (pace === 'hyper' ? 4 : 5);
      const resolved = await page.evaluate(({ pace, stage, round }) => {
        let seed = 1;
        while (seed < 10000 && DT.Sets.resolve('chaos', seed) !== 'autochess') seed++;
        const create = DT.Lock.create;
        DT.Lock.create = init => create.call(DT.Lock, { ...init, seed });
        try { DT.UI.newGame('chaos', pace, 'solo'); } finally { DT.Lock.create = create; }
        const g = __digi.game;
        g.stage = stage; g.round = round; g.roundType = g.R.roundType(stage, round); g.isPvE = g.roundType === 'pve';
        g.carousel = null; g.phase = 'planning';
        for (const p of g.players) { p.offer = null; p.board = {}; p.bench.fill(null); p.hp = 100; p.alive = true; p.board['3,4'] = g.newUnit('ac_agumon', 0); }
        __digi.ui.timerOn = false; __digi.ui.speed = .02; DT.UI.refresh();
        return { setId: g.setId, pace: g.pace, chaos: g.chaos, chaosPace: g.chaosPace, isPvE: g.isPvE };
      }, { pace, stage, round });
      assert.deepEqual(resolved, { setId: 'autochess', pace, chaos: true, chaosPace: false, isPvE: true });
      await page.waitForSelector('#cload', { state: 'detached' });
      await page.click('#readyBtn'); await page.waitForFunction(() => __digi.game.phase === 'combat' && !!__digi.shown);
      const enemies = await page.evaluate(() => __digi.game.humanMatch.sideB.units.map(u => ({ line: u.line, stars: u.stage + 1 })));
      assert.equal(enemies[0].line, first, `${pace} ${stage}-${round} enemy lineup`);
      assert.equal(Math.max(...enemies.map(u => u.stars)), maxStars, `${pace} ${stage}-${round} enemy stars`);
      results.push({ pace, stage, round, enemies }); console.log(`PASS ${pace} ${stage}-${round}: ${first}, max ${maxStars} stars`);
      if (pace === 'hyper' && stage === 3) await page.screenshot({ path: path.join(out, 'chaos-autochess-3-4.png') });
    }
    assert.deepEqual(errors, []); fs.writeFileSync(path.join(out, 'result.json'), JSON.stringify(results, null, 2));
  } finally { await browser?.close(); server.kill(); }
})().catch(e => { console.error(e); process.exitCode = 1; });
