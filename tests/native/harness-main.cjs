// Electron main for tests/steam-native.cjs: runs desktop-steam/steam.js's native paths (receive, release, send, accept,
// persona and avatar, friends' lobbies) against fake_steam_api.dll inside the real Electron runtime, where external
// buffers are forbidden. Writes a JSON report to DT_NATIVE_OUT and quits. No window.
const { app } = require('electron');
const fs = require('fs');
const path = require('path');

const OUT = process.env.DT_NATIVE_OUT;
const DLL = process.env.DT_NATIVE_DLL;
const ROOT = path.join(__dirname, '..', '..');
const results = [];
const logLines = [];
const check = (ok, msg) => results.push({ ok: !!ok, msg });
const write = (extra) => fs.writeFileSync(OUT, JSON.stringify({ results, log: logLines.slice(-80), ...extra }, null, 2));

app.whenReady().then(async () => {
  try {
    const koffi = require(path.join(ROOT, 'desktop-steam', 'node_modules', 'koffi'));
    const steam = require(path.join(ROOT, 'desktop-steam', 'steam.js'));
    const SELF = '76561190000000001';
    const FRIEND = '76561190000000002';
    const STRANGER = '76561190000000999';
    const members = new Set([SELF, FRIEND]);
    const events = [];
    steam._test.bind(DLL, { steamId: SELF, log: (m) => logLines.push(m), emit: (type, payload) => events.push({ type, payload }), isMember: (id) => members.has(id) });
    const lib = koffi.load(DLL);
    const inject = lib.func('void FakeSteam_Inject(uint64 from, const char *data, int size)');
    const injectRaw = lib.func('void FakeSteam_InjectRaw(int type, uint64 from, const char *data, int size)');
    const statsFn = lib.func('int FakeSteam_Stats(_Out_ char *out, int cap)');
    const lastSent = lib.func('const char *FakeSteam_LastSent()');
    const stats = () => { const b = Buffer.alloc(1024); statsFn(b, b.length); return JSON.parse(b.toString('utf8').replace(/\0.*$/s, '')); };
    const sendBytes = (from, s) => { const b = Buffer.from(s, 'utf8'); inject(BigInt(from), b, b.length); };

    // empty queue
    steam._test.poll();
    check(events.length === 0, 'poll on an empty queue emits nothing');

    // a message from a lobby member
    const text = JSON.stringify({ t: 'snapreq', rid: 1, l: '109775240000000001', note: 'Grüße ✓' });
    sendBytes(FRIEND, text);
    steam._test.poll();
    check(events.length === 1 && events[0].type === 'message' && events[0].payload.from === FRIEND && events[0].payload.data === text, 'a member message arrives intact (UTF-8, sender id)');

    // a large message (a log snapshot) near the 512 KB limit
    const bigText = JSON.stringify({ t: 'snap', entries: 'x'.repeat(400000) });
    sendBytes(FRIEND, bigText);
    steam._test.poll();
    check(events.length === 2 && events[1].payload.data === bigText, `a ${bigText.length}-byte message arrives intact`);

    // not ours, bad identity type, empty: all skipped but released
    sendBytes(STRANGER, '{"t":"ping"}');
    injectRaw(1, BigInt(FRIEND), Buffer.from('{"t":"x"}'), 9);
    inject(BigInt(FRIEND), Buffer.alloc(0), 0);
    steam._test.poll();
    check(events.length === 2, 'messages from non-members, non-Steam identities and empty messages are dropped');

    // several at once
    for (let i = 0; i < 20; i++) sendBytes(FRIEND, `{"n":${i}}`);
    steam._test.poll();
    check(events.length === 22 && events[21].payload.data === '{"n":19}', '20 queued messages arrive in order in one poll');

    let s = stats();
    check(s.allocated === s.released && s.queued === 0, `every received message is released once (allocated ${s.allocated}, released ${s.released})`);
    check(s.badRelease === 0, 'no double or foreign releases');

    // sessions: a new peer's first message leaves a pending session that accept takes
    members.add('76561190000000003');
    sendBytes('76561190000000003', '{"t":"pres"}');
    check(steam._test.acceptUser('76561190000000003') === true, 'AcceptSessionWithUser accepts a pending session');
    check(steam._test.acceptUser('76561190000000003') === false, 'a second accept returns false (nothing pending)');
    steam._test.poll();

    // sending
    check(steam._test.send(FRIEND, text, true) === true, 'send to a member returns k_EResultOK');
    check(lastSent() === text, 'the bytes Steam got are the bytes we sent');
    check(steam._test.send(FRIEND, bigText, true) === true, 'a 400 KB reliable send works');
    check(steam._test.send(FRIEND, 'y'.repeat(600 * 1024), true) === false, 'a send over 512 KB is refused before reaching Steam');
    check(steam._test.send('not-an-id', 'x', true) === false && steam._test.send(SELF, 'x', true) === false, 'bad ids and ourselves are refused');
    check(steam._test.send(STRANGER, 'x', true) === false, 'outbound messages cannot implicitly accept a non-member session');
    check(steam._test.acceptUser(STRANGER) === false, 'non-member session requests are rejected');
    s = stats();
    check(s.badIdentity === 0 && s.badBuffer === 0, 'Steam never saw a bad identity or buffer');

    // persona and avatars: another player's avatar is still downloading (-1) the first time
    const p1 = steam._test.persona(FRIEND);
    check(p1.name === 'Tamer 2' && p1.avatar === null, 'persona of a friend whose avatar is still downloading: name, no avatar');
    const p2 = steam._test.persona(FRIEND);
    check(p2.name === 'Tamer 2' && typeof p2.avatar === 'string' && p2.avatar.startsWith('data:image/png;base64,'), 'second ask returns the avatar as PNG');
    const p3 = steam._test.persona(SELF);
    check(typeof p3.avatar === 'string', 'own avatar');
    const p0 = steam._test.persona('0');
    check(p0.avatar === null, 'no avatar for an id without one');
    check(stats().badBuffer === 0, 'avatar buffers sized from GetImageSize');

    // friends' lobbies (flat API only)
    const fl = await require(path.join(ROOT, 'desktop-steam', 'steam.js')).friendLobbies();
    check(fl.length === 1 && fl[0].friendId === FRIEND && fl[0].id === '109775240000000001' && fl[0].data.dt_game === 'digitactics', `friends' lobbies via GetFriendGamePlayed: ${JSON.stringify(fl.map((f) => f.id))}`);

    check(logLines.some((l) => l.startsWith('native> copy')) && logLines.some((l) => l.startsWith('native< release')), 'native calls are traced in the log');
    const before = events.length;
    for (let i = 0; i < 1000; i++) sendBytes(FRIEND, '{"t":"ping"}');
    for (let i = 0; i < 16; i++) steam._test.poll();
    check(events.length - before <= 400, 'native peer flood is bounded before forwarding to the renderer');
    steam._test.blockPeer(FRIEND);
    const blockedAt = events.length;
    sendBytes(FRIEND, '{"t":"ping"}'); steam._test.poll();
    check(events.length === blockedAt && !steam._test.acceptUser(FRIEND) && !steam._test.send(FRIEND, 'x', true), 'blocked peer is denied receive, accept and send');
    s = stats();
    check(s.allocated === s.released && s.badRelease === 0, 'flooded and blocked messages still release native memory exactly once');
    const api = steam._test.api();
    check(api.setGlobalInt(api.NU, 104, 0), 'IP-sharing disable option binds and accepts the expected enum/value');
    write({ stats: stats() });
  } catch (e) {
    check(false, `harness error: ${e.stack || e}`);
    write({});
  }
  app.quit();
});
