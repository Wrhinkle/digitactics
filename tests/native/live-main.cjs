// Electron main for tests/steam-live.cjs: the real Steam client, one account. Runs desktop-steam/steam.js against the real
// steam_api64.dll: init, create a friends-only lobby, lobby data, persona and avatar for yourself and a friend, an empty
// receive, a message to yourself, friends' lobbies and the lobby list, then leaves. JSON report to DT_LIVE_OUT. No window.
const { app } = require('electron');
const fs = require('fs');
const path = require('path');

const OUT = process.env.DT_LIVE_OUT;
const results = [];
const logLines = [];
const check = (ok, msg) => results.push({ ok: !!ok, msg });
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
const steam = require(path.join(__dirname, '..', '..', 'desktop-steam', 'steam.js'));
const events = [];
const info = steam.init({ log: (m) => logLines.push(`${new Date().toISOString()} ${m}`), emit: (type, payload) => events.push({ type, payload }) });

app.whenReady().then(async () => {
  let lobby = null;
  try {
    check(info.ok, `Steam init: ${JSON.stringify(info)}`);
    if (!info.ok) throw new Error('Steam is not running');
    await sleep(500);
    steam._test.poll();
    check(!events.some((e) => e.type === 'message'), 'empty receive queue');
    lobby = await steam.createLobby('friends', 8);
    check(steam.isId(lobby), `lobby created: ${lobby}`);
    check(steam.setLobbyData(lobby, { dt_game: 'digitactics', dt_ver: 'live-test', name: 'native live test' }), 'SetLobbyData');
    const data = steam.getLobbyData(lobby);
    check(data.name === 'native live test', `GetLobbyData: ${data.name}`);
    const members = steam.lobbyMembers(lobby);
    check(members.length === 1 && members[0] === info.steamId, `members: ${members.join(',')}`);
    check(steam.lobbyOwner(lobby) === info.steamId, 'we own the lobby');
    const me = steam.persona(info.steamId);
    check(!!me.name, `own persona: ${me.name}, avatar ${me.avatar ? 'yes' : 'no'}`);
    const api = steam._test.api();
    const n = api.friendCount(api.FR, 4);
    if (n > 0) {
      const fid = String(BigInt(api.friendByIndex(api.FR, 0, 4)));
      let p = steam.persona(fid);
      for (let i = 0; i < 10 && !p.avatar; i++) { await sleep(300); p = steam.persona(fid); }
      check(!!p.name, `friend ${fid} persona: ${p.name}, avatar ${p.avatar ? 'yes' : 'not loaded (ok if they have none)'}`);
    } else check(true, 'no friends to look up');
    // a message to ourselves (Steam loops it back locally when it allows that)
    const text = JSON.stringify({ t: 'ping', l: lobby, at: Date.now() });
    const buf = Buffer.from(text);
    const r = api.sendMessage(api.SNM, steam._test.identity(info.steamId), buf, buf.length, 9, 0);
    let got = null;
    for (let i = 0; i < 40 && !got; i++) { await sleep(100); steam._test.poll(); got = events.find((e) => e.type === 'message'); }
    check(true, `send to self: EResult ${r}, ${got ? (got.payload.data === text ? 'came back intact' : 'came back CHANGED') : 'no loopback (Steam may not allow it)'}`);
    if (got) check(got.payload.data === text, 'loopback message intact');
    const fl = await steam.friendLobbies();
    check(Array.isArray(fl), `friends' lobbies: ${fl.length}`);
    const ll = await steam.listLobbies({ dt_game: 'digitactics' });
    check(Array.isArray(ll), `public lobby list: ${ll.length}`);
  } catch (e) {
    check(false, `error: ${e.stack || e}`);
  }
  if (lobby) steam.leaveLobby(lobby);
  fs.writeFileSync(OUT, JSON.stringify({ results, log: logLines }, null, 2));
  setTimeout(() => app.quit(), 300);
});
