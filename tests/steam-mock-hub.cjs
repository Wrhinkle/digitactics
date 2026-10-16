// In-memory stand-in for Steam (matchmaking lobbies, lobby data, friends, Networking Messages, overlay invites) that
// gives Playwright pages a window.DTSteam with the same contract as desktop-steam/preload.js (see js/net-steam.js).
// The hub lives in the Node test process; each browser context is one Steam user.
//   const hub = new MockSteamHub();
//   const ctx = await browser.newContext(); await hub.install(ctx, { steamId, name, launchLobby });
//   hub.befriend(a, b); hub.emit(user, 'joinRequested', { lobbyId });
const sleep = (ms) => new Promise((r) => setTimeout(r, ms));
// 2x2 PNG, stands in for a Steam avatar
const AVATAR = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAICAYAAADED76LAAAAkklEQVR4nBXKIRVEIRQA0Q32Q9AAg0aPocFrgEGjMTR4rWYPV9/fR1pIGymkQbpID2mS/j6uhWvjCtfgurgersl9YVvYNrawDbaL7WGb7BemhWljCtNgupgepsl8ISyEjRDCIFyEhzCJF4aFYWMIw2C4GB6GyXihW+g2utANuovuoZv0F6qFaqMK1aC6qB6qSfUPEPKjIYG62OMAAAAASUVORK5CYII=';

const PAGE_SCRIPT = `(() => {
  const subs = {};
  const call = (m, a) => window.__dtSteamHub(m, a);
  window.__dtSteamEmit = (t, p) => { for (const cb of (subs[t] || []).slice()) { try { cb(p); } catch (e) { console.error(e); } } };
  const api = { mock: true };
  for (const m of ['init', 'persona', 'createLobby', 'joinLobby', 'leaveLobby', 'lobbyMembers', 'lobbyOwner', 'setLobbyOwner', 'setLobbyData',
    'getLobbyData', 'listLobbies', 'friendLobbies', 'inviteDialog', 'send', 'launchLobby']) api[m] = (...a) => call(m, a);
  api.on = (t, cb) => { (subs[t] = subs[t] || []).push(cb); return () => { subs[t] = (subs[t] || []).filter((x) => x !== cb); }; };
  window.DTSteam = Object.freeze(api);
})();`;

class MockSteamHub {
  constructor(opts = {}) {
    this.users = new Map(); // steamId -> user
    this.lobbies = new Map(); // id -> { id, type, max, owner, members[], data{} }
    this.friends = new Set(); // 'a|b'
    this.invites = [];
    this.next = 109775241000000000n;
    this.latency = opts.latency || [1, 8];
    this.dropUnreliable = opts.dropUnreliable || 0;
    this.stats = { reliable: 0, unreliable: 0, dropped: 0, bytes: 0 };
  }

  befriend(a, b) { this.friends.add(`${a}|${b}`); this.friends.add(`${b}|${a}`); }

  // a lobby owned by someone with no page (other Spacewar developers, older builds)
  fakeLobby(owner, type, data) {
    const id = String(this.next++);
    this.lobbies.set(id, { id, type, max: 8, owner, members: [owner], data: { ...data } });
    if (!this.users.has(owner)) this.users.set(owner, { steamId: owner, name: owner, page: null, queue: Promise.resolve() });
    return id;
  }

  // deferred: the user is not signed in to the hub until activate(user) (its page can load first); launchLobby waits for it
  async install(context, { steamId, name, launchLobby = null, deferred = false }) {
    const user = { steamId, name, page: null, queue: Promise.resolve(), launchLobby, gone: false };
    user.ready = new Promise((res) => { user.activate = () => { this.users.set(steamId, user); res(); }; });
    if (!deferred) user.activate();
    await context.exposeBinding('__dtSteamHub', (source, method, args) => {
      if (!user.page) { user.page = source.page; source.page.on('close', () => this.disconnect(user)); }
      return user.ready.then(() => this.call(user, method, args || []));
    });
    await context.addInitScript(PAGE_SCRIPT);
    return user;
  }

  emit(user, type, payload) {
    if (!user || !user.page) return;
    const [lo, hi] = this.latency;
    user.queue = user.queue.then(async () => {
      if (!user.page || user.page.isClosed()) return;
      await sleep(lo + Math.random() * (hi - lo));
      await user.page.evaluate(([t, p]) => window.__dtSteamEmit && window.__dtSteamEmit(t, p), [type, payload]).catch(() => {});
    });
  }

  removeMember(lobby, steamId) {
    if (!lobby.members.includes(steamId)) return;
    lobby.members = lobby.members.filter((m) => m !== steamId);
    if (!lobby.members.length) { this.lobbies.delete(lobby.id); return; }
    if (lobby.owner === steamId) lobby.owner = lobby.members[0]; // Steam moves ownership to a remaining member
    for (const m of lobby.members) this.emit(this.users.get(m), 'lobbyChat', { lobbyId: lobby.id, user: steamId, change: 'left' });
  }

  disconnect(user) {
    user.gone = true;
    for (const l of [...this.lobbies.values()]) this.removeMember(l, user.steamId);
  }

  call(user, method, a) {
    const me = user.steamId;
    const lobby = (id) => this.lobbies.get(String(id));
    switch (method) {
      case 'init': return { ok: true, steamId: me, name: user.name, appId: 480 };
      case 'persona': { const u = this.users.get(String(a[0])); return u ? { name: u.name, avatar: AVATAR } : { name: '', avatar: null }; }
      case 'createLobby': {
        const id = String(this.next++);
        this.lobbies.set(id, { id, type: a[0] === 'friends' ? 'friends' : 'public', max: a[1] || 8, owner: me, members: [me], data: {} });
        return id;
      }
      case 'joinLobby': {
        const l = lobby(a[0]);
        if (!l) return { ok: false, error: 'no such lobby' };
        if (l.members.includes(me)) return { ok: true };
        if (l.members.length >= l.max) return { ok: false, error: 'full' };
        l.members.push(me);
        for (const m of l.members) if (m !== me) this.emit(this.users.get(m), 'lobbyChat', { lobbyId: l.id, user: me, change: 'entered' });
        return { ok: true };
      }
      case 'leaveLobby': { const l = lobby(a[0]); if (l) this.removeMember(l, me); return true; }
      case 'lobbyMembers': { const l = lobby(a[0]); return l && l.members.includes(me) ? [...l.members] : []; }
      case 'lobbyOwner': { const l = lobby(a[0]); return l ? l.owner : null; }
      case 'setLobbyOwner': { const l = lobby(a[0]); if (!l || l.owner !== me || !l.members.includes(String(a[1]))) return false; l.owner = String(a[1]); return true; }
      case 'setLobbyData': {
        const l = lobby(a[0]);
        if (!l || l.owner !== me) return false;
        for (const [k, v] of Object.entries(a[1] || {})) l.data[k] = String(v);
        return true;
      }
      case 'getLobbyData': { const l = lobby(a[0]); return l ? { ...l.data } : {}; }
      case 'listLobbies': {
        const f = a[0] || {};
        return [...this.lobbies.values()].filter((l) => l.type === 'public' && Object.entries(f).every(([k, v]) => l.data[k] === String(v)))
          .map((l) => ({ id: l.id, data: { ...l.data }, members: l.members.length, max: l.max }));
      }
      case 'friendLobbies': {
        const out = [];
        for (const l of this.lobbies.values()) {
          const f = l.members.find((m) => m !== me && this.friends.has(`${me}|${m}`));
          if (f && !l.members.includes(me)) out.push({ id: l.id, friendId: f, friendName: this.users.get(f).name, data: { ...l.data }, members: l.members.length });
        }
        return out;
      }
      case 'inviteDialog': this.invites.push({ from: me, lobbyId: String(a[0]) }); return true;
      case 'send': {
        const [to, data, reliable] = a;
        const u = this.users.get(String(to));
        if (!u || u.gone) return false;
        this.stats.bytes += String(data).length;
        if (reliable) this.stats.reliable++; else this.stats.unreliable++;
        if (!reliable && Math.random() < this.dropUnreliable) { this.stats.dropped++; return true; }
        this.emit(u, 'message', { from: me, data: String(data) });
        return true;
      }
      case 'launchLobby': { const id = user.launchLobby; user.launchLobby = null; return id; }
    }
    throw new Error(`mock Steam: unknown call ${method}`);
  }
}

module.exports = { MockSteamHub, AVATAR };
