// Multiplayer over the Steam transport (js/net-steam.js) against an in-memory Steam (tests/steam-mock-hub.cjs): two
// browser contexts are two Steam users. Ports the checks of tests/multiplayer.cjs: create a lobby, find it in the list
// (friends first, other Spacewar lobbies filtered out), join through an invite, play rounds with matching state, a
// forced desync that must be repaired, plus a late rejoin through +connect_lobby and a host handover.
// Run: node tests/steam-mock.cjs <path-to-playwright> <outDir> [port]
const path = require('path');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const { MockSteamHub } = require('./steam-mock-hub.cjs');
const out = process.argv[3] || __dirname;
const PORT = Number(process.argv[4] || 8180);
const URL = `http://localhost:${PORT}/`;
const ALICE = '76561190000000001';
const BOB = '76561190000000002';

const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const digest = (page) => page.evaluate(() => { const g = window.__digi.game; return g ? `${g.seq}|${window.DT.Lock.digest(g)}` : null; });
const fails = [];
const check = (ok, msg) => { console.log(`${ok ? 'ok  ' : 'FAIL'} ${msg}`); if (!ok) fails.push(msg); };

// compare only when both clients have applied the same number of log entries (digest starts with the count)
async function compare(a, b, tries = 40) {
  let da = '', db = '', sa = -1, sb = -2;
  for (let k = 0; k < tries && sa !== sb; k++) {
    await sleep(250);
    [da, db] = [await digest(a), await digest(b)];
    [sa, sb] = [Number(String(da).split('|')[0]), Number(String(db).split('|')[0])];
    if (sa === sb && Number(String(await digest(a)).split('|')[0]) !== sa) sa = -1; // moved on mid-read
  }
  return { same: sa === sb, match: sa === sb && da === db, sa, sb, da, db };
}

(async () => {
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true, stdio: 'ignore' });
  await sleep(600);
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const hub = new MockSteamHub();
  // the protocol version of this build (js/net-steam.js); lobbies of other versions stay hidden
  const PROTO = (require('fs').readFileSync(path.join(__dirname, '..', 'js', 'net-steam.js'), 'utf8').match(/PROTO_VERSION = '([^']+)'/) || [])[1];
  hub.befriend(ALICE, BOB);
  // other lobbies on App ID 480: another developer's Spacewar lobby, an older Digi-Tactics build, a stranger's public lobby
  hub.fakeLobby('76561190000000090', 'public', { name: 'spacewar dm', mode: 'deathmatch' });
  hub.fakeLobby('76561190000000091', 'public', { dt_game: 'digitactics', dt_ver: '1', name: 'old build lobby', state: 'lobby' });
  hub.fakeLobby('76561190000000092', 'public', { dt_game: 'digitactics', dt_ver: PROTO, name: 'stranger lobby', hostName: 'Stranger', state: 'lobby', rounds: '45', seats: JSON.stringify(['Stranger', '', '', '', '', '', '', '']) });
  const errors = [];
  const mk = async (name, steamId, launchLobby, deferred) => {
    const ctx = await browser.newContext({ viewport: { width: 1400, height: 880 } });
    const user = await hub.install(ctx, { steamId, name, launchLobby, deferred });
    const page = await ctx.newPage();
    page.on('pageerror', (e) => errors.push(`${name}: ${e}`));
    page.on('console', (m) => { if (m.type() === 'error') errors.push(`${name}: ${m.text()}`); if (process.env.DT_DEBUG && /Steam|sync/.test(m.text())) console.log(`  [${name}] ${m.text()}`); });
    return { ctx, page, user };
  };
  const A = await mk('Alice', ALICE);
  const B = await mk('Bob', BOB);
  const a = A.page;
  let b = B.page;
  try {
    let tl0 = Date.now();
    await a.goto(URL);
    console.log(`Alice's page loaded in ${Date.now() - tl0} ms`);
    check(await a.evaluate(() => window.DT.Net.transport) === 'steam', 'page with window.DTSteam uses the Steam transport');
    await a.click('#mpBtn');
    await a.waitForSelector('#mpPersona', { timeout: 20000 });
    check(await a.inputValue('#mpPersona') === 'Alice', 'lobby screen shows the Steam persona name');
    await a.fill('#mpLobbyName', 'steam test lobby');
    await a.selectOption('#mpVis', 'friends');
    if (process.env.DT_DUOS) await a.selectOption('#mpFormat', 'duos');
    await a.click('#mpCreate');
    await a.waitForSelector('#mpInvite', { timeout: 15000 });
    const lobbyId = [...hub.lobbies.values()].find((l) => l.owner === ALICE).id;
    const data = hub.lobbies.get(lobbyId).data;
    check(data.dt_game === 'digitactics' && data.dt_ver === PROTO && data.name === 'steam test lobby' && data.state === 'lobby' && data.rounds === '30' && !!data.setId,
      `Steam lobby data: dt_game=${data.dt_game} dt_ver=${data.dt_ver} name="${data.name}" setId=${data.setId} rounds=${data.rounds} state=${data.state}`);
    await a.click('#mpInvite');
    check(hub.invites.some((i) => i.from === ALICE && i.lobbyId === lobbyId), 'Invite button opens the Steam overlay invite dialog for the lobby');

    // Bob: the lobby list shows the friend's (friends-only) lobby first, then public Digi-Tactics lobbies; filters the rest
    await b.goto(URL);
    await b.click('#mpBtn');
    await b.waitForSelector('#mpList [data-room]', { timeout: 20000 });
    const rows = await b.evaluate(() => [...document.querySelectorAll('#mpList .mplobby')].map((r) => ({ name: r.querySelector('b').textContent, friend: r.classList.contains('friend'), room: r.querySelector('[data-room]').dataset.room })));
    console.log('Bob sees:', rows.map((r) => `${r.name}${r.friend ? ' (friend)' : ''}`).join(', '));
    check(rows.length === 2 && rows[0].room === lobbyId && rows[0].friend && rows[1].name === 'stranger lobby', 'list: friend lobby first, then public, without other Spacewar or old-version lobbies');
    await b.screenshot({ path: path.join(out, 'steam-0-list.png') });

    // accept the invite (GameLobbyJoinRequested while the game runs)
    hub.emit(hub.users.get(BOB), 'joinRequested', { lobbyId, friendId: ALICE });
    await b.waitForSelector('.mpseat.me', { timeout: 20000 });
    await a.waitForFunction(() => document.querySelectorAll('.mpseat.taken').length === 2, null, { timeout: 15000 });
    if (process.env.DT_DUOS) {
      check(hub.lobbies.get(lobbyId).data.format === 'duos', 'Steam lobby advertises Duos');
      await b.click('[data-seat="3"]');
      await b.waitForFunction(() => document.querySelector('.mpseat.me .n')?.textContent === 'T2');
      await b.click('[data-seat="1"]');
      await b.waitForFunction(() => document.querySelector('.mpseat.me .n')?.textContent === 'T1');
      check(true, 'Move seats to choose a team, then rejoin the host’s team');
    }
    await a.waitForFunction(() => document.querySelectorAll('.mpseat .mp-acct').length === 2, null, { timeout: 10000 }).catch(() => {});
    check(await a.evaluate(() => document.querySelectorAll('.mpseat .mp-acct').length) === 2, 'seats show Steam avatars');
    await a.screenshot({ path: path.join(out, 'steam-1-lobby-a.png') });
    await b.screenshot({ path: path.join(out, 'steam-1-lobby-b.png') });
    const ts = Date.now();
    await a.click('#mpStart');
    await a.waitForFunction(() => window.__digi.game && window.__digi.session, null, { timeout: 20000 });
    await b.waitForFunction(() => window.__digi.game && window.__digi.session, null, { timeout: 20000 });
    console.log(`  both in the game ${Date.now() - ts} ms after Start`);
    const bobSlot = await b.evaluate(() => window.__digi.session.slot);
    if (process.env.DT_DUOS) check(await b.evaluate(() => __digi.game.duo && __digi.game.partnerOf(__digi.game.human).id === 0), 'Duos init keeps both friends on the same team');
    const names = await a.evaluate(() => window.__digi.game.players.filter((p) => p.isHuman).map((p) => p.name).join(','));
    console.log('both in game; slots', await a.evaluate(() => window.__digi.session.slot), bobSlot, '| humans', names);
    await a.waitForFunction(() => window.__digi.game.players.filter((p) => p.isHuman).length === 2, null, { timeout: 10000 }).catch(() => {});
    const humans = await a.evaluate(() => window.__digi.game.players.filter((p) => p.isHuman).map((p) => p.name).sort().join(','));
    check(humans === 'Alice,Bob', `players carry Steam names (${humans})`);
    check(hub.lobbies.get(lobbyId).data.state === 'playing', 'lobby data state is playing');

    // carousel: whoever's turn it is picks
    for (let i = 0; i < 40; i++) {
      for (const p of [a, b]) {
        const el = p.locator('.choice[data-carousel]:not(.taken)').first();
        const mine = await p.evaluate(() => { const g = window.__digi.game; return !!(g.carousel && g.carouselTurn() === g.human); });
        if (mine && await el.count()) await el.click().catch(() => {});
      }
      if (await a.evaluate(() => window.__digi.game.roundLabel) !== '1-1') break;
      await sleep(500);
    }

    let checks = 0;
    let mismatches = 0;
    for (let round = 0; round < 7; round++) {
      for (const p of [a, b]) {
        await p.waitForFunction(() => window.__digi.game.phase === 'planning', null, { timeout: 60000 });
        await p.evaluate(() => {
          const g = window.__digi.game;
          if (g.human.offer) document.querySelector('[data-offer]') && document.querySelector('[data-offer]').click();
          document.querySelectorAll('#shop .card[data-slot]:not(.poor)').forEach((c, i) => { if (i < 2) c.click(); });
        });
      }
      const c = await compare(a, b);
      if (process.env.DT_DUOS && round === 1) {
        const sent = await a.evaluate(() => {
          const g=__digi.game,u=g.human.bench.find(Boolean);
          if(!u) return null;
          __digi.session.send('portal',{uid:u.uid});return u.uid;
        });
        if (sent != null) {
          await b.waitForFunction(uid=>__digi.game.human.incoming.some(e=>e.uid===uid),sent,{timeout:10000});
          check(true,'Portal transfer reaches the partner over the Steam transport');
        } else check(false,'A bought unit is available for a portal transfer');
      }
      if (c.same) { if (c.match) checks++; else { mismatches++; console.log(`  states differ at seq ${c.sa}`); } }
      const label = await a.evaluate(() => window.__digi.game.roundLabel);
      console.log(`round ${label}: seq ${c.sa}/${c.sb} ${c.same ? (c.match ? 'states match' : 'STATES DIFFER') : '(in flight)'}`);
      // break Bob's copy on purpose: the host's state hash on the next round should make Bob reload from the host's log
      if (round === 3) { await b.evaluate(() => { const d = window.__digi; d.game.players[d.session.slot].gold += 7; }); console.log('corrupted Bob gold'); }
      if (round === 1) { await a.screenshot({ path: path.join(out, 'steam-2-planning-a.png') }); await b.screenshot({ path: path.join(out, 'steam-2-planning-b.png') }); }
      // soundboard (fx, unreliable): Alice plays a clip, Bob should hear it
      if (round === 1 && await b.evaluate(() => !!window.DT.Soundboard)) {
        const heard = await b.evaluate(() => window.DT.Soundboard.stats.played);
        await a.evaluate(() => { window.DT.Soundboard.open(); window.DT.Soundboard.play(0); window.DT.Soundboard.close(); });
        const ok = await b.waitForFunction((n) => window.DT.Soundboard.stats.played > n, heard, { timeout: 15000 }).then(() => true, () => false);
        check(ok, 'soundboard clip reached Bob over Steam messages');
      }
      await a.click('#readyBtn').catch(() => {});
      await b.click('#readyBtn').catch(() => {});
      await a.waitForFunction(() => window.__digi.game.phase === 'combat', null, { timeout: 40000 });
      if (round === 2) { await sleep(2500); await a.screenshot({ path: path.join(out, 'steam-3-combat-a.png') }); }
      await a.click('#skipBtn').catch(() => {});
      await b.click('#skipBtn').catch(() => {});
    }
    const resyncs = await b.evaluate(() => window.DT.Net.resyncs);
    check(resyncs >= 1, `forced desync detected and repaired: Bob resynced ${resyncs} time(s)`);
    const after = await compare(a, b);
    check(after.match, `after the resync both states match (seq ${after.sa})`);
    check(checks >= 4 && mismatches === 0, `state checks matched: ${checks}, differed: ${mismatches}`);

    // late rejoin: Bob's game closes; he comes back through +connect_lobby and replays the host's full log.
    // His new window loads first (page start-up is slow with three software-rendered pages), then Steam hands it the lobby.
    const B2 = await mk('Bob (rejoined)', BOB, lobbyId, true);
    const tl = Date.now();
    await B2.page.goto(URL, { timeout: 120000 });
    console.log(`  Bob's new window loaded in ${Date.now() - tl} ms`);
    await B.ctx.close();
    console.log('Bob closed his game at', await a.evaluate(() => window.__digi.game.roundLabel));
    await sleep(2000);
    b = B2.page;
    const t0 = Date.now();
    B2.user.activate();
    if (process.env.DT_DEBUG) (async () => { let last = ''; while (Date.now() - t0 < 30000) { const st = JSON.stringify(await b.evaluate(() => { const s = window.DT.Net.debug() || {}; const r = window.DT.Net.status().room; return { room: !!r, inGame: r && r.inGame, syncing: s.syncing, log: s.log, game: !!(window.__digi && window.__digi.game), seq: window.__digi && window.__digi.game ? window.__digi.game.seq : -1, panel: ((document.querySelector('#overlay .panel') || {}).innerText || '').slice(0, 40) }; }).catch(() => null)); if (st !== last) { console.log('   ', Date.now() - t0, st && st.slice(0, 260)); last = st; } await sleep(200); } })();
    await b.waitForFunction(() => window.__digi.game && window.__digi.session, null, { timeout: 30000 }).catch(async (e) => {
      console.log('  Bob2 state:', JSON.stringify(await b.evaluate(() => ({ net: window.DT.Net.status(), steam: window.DT.Net.debug() })).catch((x) => String(x))), 'Alice:', JSON.stringify(await a.evaluate(() => window.DT.Net.debug())));
      throw e;
    });
    console.log(`  Bob back in the game ${Date.now() - t0} ms after Steam handed over the lobby`);
    const rej = await compare(a, b);
    check((await b.evaluate(() => window.__digi.session.slot)) === bobSlot && rej.match, `late rejoin: same seat ${bobSlot}, full log replayed, states match at seq ${rej.sa}`);
    await b.screenshot({ path: path.join(out, 'steam-4-rejoined.png') });

    // host leaves: Bob takes over and the game keeps advancing
    await b.waitForFunction(() => window.__digi.game.phase === 'planning', null, { timeout: 60000 }).catch(() => {});
    const before = await b.evaluate(() => window.__digi.game.roundLabel);
    const th = Date.now();
    await A.ctx.close();
    console.log('host closed at', before);
    const took = await b.waitForFunction(() => { const d = window.DT.Net.debug(); return d && d.auth; }, null, { timeout: 30000 }).then(() => true, () => false);
    check(took, `Bob became the host ${Date.now() - th} ms after the host left`);
    // play on as the new host: ready up and skip combat until the round changes
    let label = before;
    for (let k = 0; k < 60 && label === before; k++) {
      await b.click('#readyBtn', { timeout: 1000 }).catch(() => {});
      await b.click('#skipBtn', { timeout: 1000 }).catch(() => {});
      await sleep(1000);
      label = await b.evaluate(() => window.__digi.game.roundLabel);
    }
    const l = hub.lobbies.get(lobbyId);
    check(label !== before, `host handover: Bob's game moved on from ${before} to ${label}`);
    await sleep(1500);
    check(l && l.owner === BOB && l.data.host === BOB, `Steam lobby owner and host data moved to Bob (owner ${l && l.owner}, host ${l && l.data.host})`);
    await b.screenshot({ path: path.join(out, 'steam-5-after-handover.png') });
    console.log('mock Steam traffic:', JSON.stringify(hub.stats));
  } catch (e) {
    check(false, `run: ${e.stack || e}`);
  } finally {
    // missing model files are content problems, not multiplayer ones: listed, not failed (as in tests/sets.cjs)
    const missing = errors.filter((e) => /Failed to load resource/.test(e));
    const real = errors.filter((e) => !/Failed to load resource/.test(e));
    if (missing.length) console.log(`note: ${missing.length} resource(s) failed to load (missing files)`);
    check(!real.length, `no page errors${real.length ? `: ${real.slice(0, 8).join(' | ')}` : ''}`);
    await browser.close();
    server.kill();
    console.log(fails.length ? `\n${fails.length} failed` : '\nsteam mock ok');
    process.exit(fails.length ? 1 : 0);
  }
})();
