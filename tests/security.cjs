// Two real browser clients running the full game, with hostile traffic injected at the mocked Steam boundary.
// Native sender/memory/rate-limit coverage is in steam-native.cjs; this test makes no live Steam or Firebase writes.
const path = require('path');
const fs = require('fs');
const assert = require('assert/strict');
const { spawn } = require('child_process');
const { chromium } = require(process.argv[2] || 'playwright');
const { MockSteamHub } = require('./steam-mock-hub.cjs');
const ROOT = path.join(__dirname, '..');
const PORT = Number(process.argv[4] || 8192);
const OUT = process.argv[3] || __dirname;
const A = '76561190000000001', B = '76561190000000002', X = '76561190000000999';
const pause = (n) => new Promise((r) => setTimeout(r, n));
const check = (ok, label) => { assert.ok(ok, label); console.log('ok  ' + label); };
(async () => {
  const server = spawn(process.execPath, [path.join(ROOT, 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true, stdio: 'ignore' });
  const browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
  const hub = new MockSteamHub();
  const pages = [];
  const errors = [];
  try {
    await pause(500);
    for (const [steamId, name] of [[A, 'Alice'], [B, 'Bob']]) {
      const ctx = await browser.newContext({ viewport: { width: 1400, height: 900 } });
      await ctx.route('https://fonts.googleapis.com/**', (r) => r.abort());
      await ctx.route('https://fonts.gstatic.com/**', (r) => r.abort());
      await hub.install(ctx, { steamId, name });
      const p = await ctx.newPage(); pages.push(p);
      p.on('pageerror', (e) => errors.push(String(e)));
      await p.goto(`http://localhost:${PORT}/`, { waitUntil: 'domcontentloaded', timeout: 60000 });
    }
    const [a, b] = pages;
    await a.click('#mpBtn'); await a.waitForSelector('#mpCreate');
    await a.selectOption('#mpFormat', 'duos'); await a.click('#mpCreate');
    await a.waitForSelector('#mpInvite');
    const l = [...hub.lobbies.values()].find((v) => v.owner === A);
    hub.emit(hub.users.get(B), 'joinRequested', { lobbyId: l.id, friendId: A });
    await b.waitForSelector('.mpseat.me');
    await a.waitForFunction(() => document.querySelectorAll('.mpseat.taken').length === 2);
    const inject = async (target, from, msg) => {
      await target.evaluate(([from, data]) => window.__dtSteamEmit('message', { from, data }), [from, typeof msg === 'string' ? msg : JSON.stringify({ l: l.id, ...msg })]);
    };
    const seats = () => a.evaluate(() => DT.Net.status().room.members);
    await inject(a, B, { t: 'seat', rid: 900, i: 3, name: 'Bob' }); await pause(300);
    check((await seats()).filter((v) => v.uid === B).length === 1, 'one Steam identity cannot occupy multiple seats');
    await b.click('[data-seat="1"]'); await b.waitForFunction(() => document.querySelector('.mpseat.me .n')?.textContent === 'T1');
    await a.click('[data-mute="1"]');
    check(await a.evaluate((id) => JSON.parse(localStorage.getItem('dt.muted')).includes(id), B), 'mute preference persists by SteamID');
    await a.click('#mpStart');
    for (const p of pages) await p.waitForFunction(() => !!window.__digi?.session, null, { timeout: 30000 });
    const before = await a.evaluate(() => ({ host: DT.Net.debug().host, gold: __digi.game.players.map((p) => p.gold), human: __digi.game.players.map((p) => p.isHuman), slots: DT.Net.status().room.members }));
    const round = await a.evaluate(() => __digi.game.stage * 100 + __digi.game.round);
    const q = Date.now() * 1000 + 100;
    for (const msg of [
      { t: 'intent', s: 0, a: 'buy', g: { slot: 0 }, q, round },
      { t: 'intent', s: 1, a: 'claim', g: { name: 'Hacked' }, q: q + 1, round },
      { t: 'intent', s: 1, a: 'release', g: { name: 'Hacked' }, q: q + 2, round },
      { t: 'intent', s: 1, a: 'move', g: { from: { type: 'bench', i: 0 }, to: { type: 'bench', i: 999999999 } }, q: q + 3, round },
      { t: 'head', n: -1 }, { t: 'log', n: 2147483648, e: { k: 'next', at: Date.now() } },
      { t: 'lobby', lobby: { host: B, hostName: 'Bob', name: 'Hijack', status: 'lobby', slots: {} } },
      { t: 'snap', rid: 1, total: 999999999, start: 0, entries: [], lobby: { host: B, hostName: 'Bob', name: 'Huge', status: 'lobby', slots: {} }, presence: {}, init: null, now: Date.now() },
      { t: 'presence', presence: { [B]: { look: { legend: '<img src=x onerror=alert(1)>' } } } },
    ]) await inject(a, B, msg);
    await inject(a, B, '{"l":"' + l.id + '","t":"presup","rec":{"__proto__":{"polluted":true}}}');
    await inject(a, B, 'x'.repeat(300000));
    await inject(a, X, { t: 'seat', rid: 901, i: 2, name: 'Outsider' });
    await pause(800);
    const after = await a.evaluate(() => ({ host: DT.Net.debug().host, gold: __digi.game.players.map((p) => p.gold), human: __digi.game.players.map((p) => p.isHuman), slots: DT.Net.status().room.members }));
    check(JSON.stringify(before) === JSON.stringify(after), 'forged seats/actions/host claims and malformed snapshots leave gameplay unchanged');
    check(await a.evaluate(() => !({}).polluted && DT.Net.debug().pending <= 512), 'prototype and pending-log attacks are rejected');
    await a.evaluate(() => { window.seenFx = 0; DT.Social.on('emote', () => seenFx++); });
    await inject(a, B, { t: 'fx', v: { s: 1, kind: 'emote', id: 'gg' } }); await pause(200);
    check(await a.evaluate(() => seenFx === 0), 'muted peer effects do not reach the game');
    await a.click('#mpPlayerControls'); await a.waitForSelector('#mpSafety');
    await a.click('#mpSafety [data-mute="1"]'); await a.click('#mpSafety > button');
    check(await a.evaluate((id) => !JSON.parse(localStorage.getItem('dt.muted')).includes(id) && DT.Net.status().room.members.some((s) => s.uid === id), B), 'unmute keeps the other player seated');
    await pause(2200);
    await inject(a, B, { t: 'fx', v: { s: 0, kind: 'emote', id: 'gg' } }); await pause(100);
    check(await a.evaluate(() => seenFx === 0), 'peer cannot spoof another player’s cosmetic effects');
    await inject(a, B, { t: 'fx', v: { s: 1, kind: 'emote', id: 'gg' } }); await pause(100);
    check(await a.evaluate(() => seenFx === 1), 'unmuted valid effects still arrive');
    const locked = await a.evaluate(() => __digi.game.players[1].locked);
    const replay = { t: 'intent', s: 1, a: 'lock', g: {}, q: q + 10, round };
    await inject(a, B, replay); await inject(a, B, replay); await pause(650);
    check(await a.evaluate((v) => __digi.game.players[1].locked !== v, locked), 'replayed action sequence is applied exactly once');
    await inject(a, B, { ...replay, q: q + 11, round: round - 1 }); await pause(350);
    check(await a.evaluate((v) => __digi.game.players[1].locked !== v, locked), 'stale-round action is rejected');
    const bytes = hub.stats.bytes;
    await a.evaluate(([from, lobby]) => {
      for (let i = 0; i < 1000; i++) __dtSteamEmit('message', { from, data: JSON.stringify({ t: 'snapreq', l: lobby, rid: 5000 + i }) });
    }, [B, l.id]);
    await pause(400);
    check(hub.stats.bytes - bytes < 200000 && await a.evaluate(() => DT.Net.debug().queued < 256 && DT.Net.debug().dropped > 500), 'snapshot spam cannot amplify into unbounded response traffic or work');
    await a.click('#mpPlayerControls'); await a.click('#mpSafety [data-remove="1"]');
    await b.waitForFunction(() => !DT.Net.status().room, null, { timeout: 10000 });
    check((await seats()).every((v) => v.uid !== B), 'host removal frees the seat and disconnects the normal client');
    await b.evaluate((id) => DT.Net.open(id), l.id); await pause(3000);
    check(await b.evaluate(() => !DT.Net.status().room), 'removed account cannot rejoin the same lobby');
    await a.screenshot({ path: path.join(OUT, 'security-player-controls.png') });
    check(!errors.length, `no uncaught browser errors (${errors.join('; ')})`);
    fs.writeFileSync(path.join(OUT, 'security-result.json'), JSON.stringify({ ok: true, traffic: hub.stats }, null, 2));
  } finally { await browser.close(); server.kill(); }
})().catch((e) => { console.error(e); process.exitCode = 1; });
