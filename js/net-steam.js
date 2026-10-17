// Steam transport for js/net.js, used when the page runs inside the Steam desktop build (window.DTSteam, from
// desktop-steam/preload.js) or under the test mock (tests/steam-mock.cjs).
//
// One Steam matchmaking lobby = one room. The game host (lobby.host) is the authority: it holds the lobby record, the
// presence map, the game init and the ordered log, the same shape as the Firebase data. Other members send it requests
// (take a seat, presence, intents) over Steam Networking Messages; it applies them and broadcasts the results. Log entries
// carry their index, go out reliable and ordered, and every member keeps a full copy, so any member can take over.
// Emotes and soundboard clips go straight to every member, unreliable.
//
// Steam lobby data (written by the Steam lobby owner, who is moved to the game host) is only for the lobby list:
//   dt_game=digitactics, dt_ver=<PROTO_VERSION>, name, host, hostName, setId, pace, rounds, state, round, seats (JSON names)
// App ID 480 (Spacewar) is shared by every developer testing Steamworks, so the list filters on dt_game and dt_ver.
//
// Host handover: when the host leaves the Steam lobby, everyone drops its presence. net.js (in a game) or this file
// (in the lobby screen) then lets the first seated player still in the lobby claim the host after 6 s; the new host
// broadcasts its log length and members whose log is longer or different reload from the new host's log.
//
// DTSteam contract (desktop-steam/preload.js and the test mock implement it; Steam ids and lobby ids are decimal strings):
//   init() -> { ok, steamId, name, appId, error }        persona(steamId) -> { name, avatar: 'data:image/png;base64,...'|null }
//   createLobby(type 'public'|'friends', max) -> lobbyId|null    joinLobby(id) -> { ok, error }    leaveLobby(id)
//   lobbyMembers(id) -> [steamId] (lobby order)   lobbyOwner(id) -> steamId   setLobbyOwner(id, steamId)
//   setLobbyData(id, {k: v})   getLobbyData(id) -> {k: v}
//   listLobbies({k: v} string filters) -> [{ id, data, members, max }]
//   friendLobbies() -> [{ id, friendId, friendName, data, members }]     friends in this app who are in a lobby
//   inviteDialog(id)   send(steamId, text, reliable) -> bool   launchLobby() -> lobbyId|null (+connect_lobby)
//   on(type, cb) -> off   types: 'message' {from, data}, 'lobbyChat' {lobbyId, user, change: 'entered'|'left'},
//                                 'joinRequested' {lobbyId, friendId}
import { logKey } from './net-firebase.js';
import * as Guard from './net-guard.js';

const GAME_KEY = 'digitactics';
export const PROTO_VERSION = '8'; // 8: expanded roster forms and model assignments across sets
const HOST_GONE_MS = 6000;
const CHUNK = 150; // log entries per snapshot message (Steam messages max out at 512 KB)
const keyIndex = (k) => (k ? parseInt(String(k).slice(1), 10) : -1);
const clone = (v) => (v == null ? null : JSON.parse(JSON.stringify(v)));
const later = (fn) => Promise.resolve().then(fn);
const cleanRemote = (s, max = 16) => String(s == null ? '' : s).replace(/[\u0000-\u001f<>&"\\]/g, '').trim().slice(0, max);
const PRES_KEYS = ['name', 'look', 'lobbySet', 'lobbyPace', 'lobbyFormat'];

export function steamTransport(S) {
  let offset = 0;
  let bestRtt = Infinity;
  let R = null; // the room we are in
  const avatars = new Map(); // steamId -> data URL, or null while loading

  const newRoom = (id) => ({
    id, members: [], owner: null, lobby: null, presence: {}, init: null, log: [], pending: new Map(), syncing: false,
    cbs: { lobby: new Set(), pres: new Set(), log: new Set(), intent: new Set(), fx: new Set() },
    seatReq: new Map(), snapReq: new Map(), snapIn: null, rid: 0, intentN: 0, hostGoneAt: null, lastPing: 0, lastFill: 0,
    lastLobbyData: '', inbox: Promise.resolve(), timer: null, myPres: null,
    limits: new Map(), lastIntent: new Map(), outgoingIntent: Date.now() * 1000, queued: 0, dropped: 0,
  });
  const isAuth = () => !!(R && R.lobby && R.lobby.host === T.uid);
  const hostId = () => (R && R.lobby ? R.lobby.host : null);
  const banned = (uid) => !!R?.lobby?.banned?.includes(uid);
  const expectedHost = (uid) => uid === (hostId() || R?.owner) ||
    (hostId() && !R.members.includes(hostId()) && uid === successor(hostId()));
  function applyBans() {
    if (!R) return;
    for (const uid of R.lobby?.banned || []) if (uid !== T.uid) S.blockPeer?.(uid);
    if (banned(T.uid)) { for (const cb of R.cbs.lobby) later(() => cb(null)); }
  }

  // ---- sending ----
  const send = (to, msg, reliable = true) => { if (R && to && to !== T.uid && !banned(to)) S.send(to, JSON.stringify({ ...msg, l: R.id }), reliable); };
  const broadcast = (msg, reliable = true) => { if (!R) return; for (const m of R.members) if (m !== T.uid) send(m, msg, reliable); };

  // ---- local listeners ----
  const notifyLobby = () => { const v = clone(R.lobby); for (const cb of R.cbs.lobby) later(() => cb(clone(v))); };
  const notifyPres = () => { const v = clone(R.presence); for (const cb of R.cbs.pres) later(() => cb(clone(v))); };
  const deliver = (w) => {
    while (!w.off && R && w.after + 1 < R.log.length) { w.after++; w.cb(logKey(w.after), clone(R.log[w.after])); }
  };
  const deliverAll = () => { if (R) for (const w of [...R.cbs.log]) deliver(w); };
  const resetLog = () => { if (R) for (const w of [...R.cbs.log]) if (w.onReset) later(() => !w.off && w.onReset()); };
  const emitIntent = (it) => { const key = `i${++R.intentN}`; for (const cb of R.cbs.intent) later(() => cb(key, clone(it))); };
  const watch = (set, item, first) => { set.add(item); if (first) later(first); return () => { item.off = true; set.delete(item); }; };

  // ---- authority: apply and broadcast ----
  function commitLobby() {
    notifyLobby();
    broadcast({ t: 'lobby', lobby: R.lobby });
    writeLobbyData();
  }
  function commitPresence() {
    notifyPres();
    broadcast({ t: 'presence', presence: R.presence });
  }
  function writeLobbyData() {
    if (!R || !R.lobby || R.owner !== T.uid) return;
    const l = R.lobby;
    const seats = Array.from({ length: 8 }, (_, i) => (l.slots && l.slots[i] ? l.slots[i].name : ''));
    const data = {
      dt_game: GAME_KEY, dt_ver: PROTO_VERSION, name: String(l.name || ''), host: String(l.host || ''), hostName: String(l.hostName || ''),
      format: l.format === 'duos' ? 'duos' : 'solo', setId: String(l.setId || ''), pace: String(l.pace || 'hyper'), rounds: String(l.planTime || 30), state: String(l.status || 'lobby'), round: String(l.round || '1-1'), seats: JSON.stringify(seats),
    };
    const j = JSON.stringify(data);
    if (j === R.lastLobbyData) return;
    R.lastLobbyData = j;
    S.setLobbyData(R.id, data);
  }
  function applySeat(uid, i, name) {
    i = Number(i);
    if (!Number.isInteger(i) || i < 0 || i > 7 || !R.lobby) return false;
    const slots = { ...(R.lobby.slots || {}) };
    if (slots[i] || banned(uid)) return false;
    // Moving seats is atomic. A client can never occupy two seats, even if it omits the unseat request.
    for (const [k, s] of Object.entries(slots)) if (s.uid === uid) delete slots[k];
    if (R.lobby.status === 'playing' && R.lobby.alive?.[i] === false) return false;
    slots[i] = { uid, name: cleanRemote(name) || 'Tamer' };
    R.lobby = { ...R.lobby, slots };
    commitLobby();
    return true;
  }
  function freeSeatLocal(i) {
    if (!R.lobby || !R.lobby.slots || !R.lobby.slots[i]) return;
    const slots = { ...R.lobby.slots };
    delete slots[i];
    R.lobby = { ...R.lobby, slots };
    commitLobby();
  }
  function sendSnapshot(to, rid) {
    const base = { t: 'snap', rid, lobby: R.lobby, presence: R.presence, init: R.init, now: T.now(), total: R.log.length };
    if (!R.log.length) return send(to, { ...base, start: 0, entries: [] });
    for (let k = 0; k < R.log.length; k += CHUNK) send(to, { ...base, start: k, entries: R.log.slice(k, k + CHUNK) });
  }
  // who takes over when the host is gone: the first seated player still in the Steam lobby, else the first member
  function successor(exclude) {
    const slots = (R.lobby && R.lobby.slots) || {};
    for (let i = 0; i < 8; i++) { const s = slots[i]; if (s && s.uid !== exclude && !banned(s.uid) && R.members.includes(s.uid)) return s.uid; }
    return R.members.find((m) => m !== exclude && !banned(m)) || null;
  }
  function becomeHost(name) {
    if (!R || !R.lobby || isAuth()) return;
    R.lobby = { ...R.lobby, host: T.uid, hostName: name || R.lobby.hostName };
    if (R.myPres) R.presence[T.uid] = { ...R.myPres, at: T.now() };
    commitLobby();
    commitPresence();
    broadcast({ t: 'head', n: R.log.length });
  }
  function mySeatName() {
    const slots = (R.lobby && R.lobby.slots) || {};
    for (let i = 0; i < 8; i++) if (slots[i] && slots[i].uid === T.uid) return slots[i].name;
    return cleanRemote(T.personaName) || 'Tamer';
  }

  // ---- log replication ----
  function drain() {
    for (const n of [...R.pending.keys()]) if (n < R.log.length) R.pending.delete(n);
    while (R.pending.has(R.log.length)) { const e = R.pending.get(R.log.length); R.pending.delete(R.log.length); R.log.push(e); }
    deliverAll();
  }
  function onLogEntry(n, e) {
    if (n > R.log.length + 512 || R.pending.size >= 512) return;
    if (R.syncing) { R.pending.set(n, e); return; }
    if (n === R.log.length) { R.log.push(e); drain(); return; }
    if (n > R.log.length) {
      R.pending.set(n, e);
      if (Date.now() - R.lastFill > 1000) { R.lastFill = Date.now(); send(hostId(), { t: 'logreq', from: R.log.length }); }
      return;
    }
    if (JSON.stringify(R.log[n]) !== JSON.stringify(e)) { // the host's log differs from ours from here: reload from the host
      R.log.length = n;
      R.log.push(e);
      R.pending.clear();
      resetLog();
    }
  }
  function requestSnapshot() {
    const r = R;
    const rid = ++r.rid;
    r.syncing = true;
    r.snapIn = null;
    return new Promise((resolve) => {
      let t1, t2;
      const finish = (ok) => {
        if (!r.snapReq.has(rid)) return;
        r.snapReq.delete(rid);
        clearTimeout(t1);
        clearTimeout(t2);
        if (!ok && R === r) { r.syncing = false; drain(); console.warn('Digi-Tactics Steam: no snapshot from the host, using the local copy'); }
        resolve(ok);
      };
      r.snapReq.set(rid, finish);
      const ask = (all) => {
        if (R !== r) return finish(false);
        const h = hostId();
        if (!all && h && h !== T.uid && r.members.includes(h)) send(h, { t: 'snapreq', rid });
        else broadcast({ t: 'snapreq', rid });
      };
      ask(false);
      t1 = setTimeout(() => ask(true), 2500);
      t2 = setTimeout(() => finish(false), 9000);
    });
  }
  function onSnapshot(from, m) {
    if (!R.snapReq.has(m.rid) || !expectedHost(from) || m.lobby.host !== from) return;
    let s = R.snapIn;
    if (!s || s.rid !== m.rid || s.from !== from) s = R.snapIn = { rid: m.rid, from, entries: new Array(m.total), got: 0 };
    if (s.entries.length !== m.total) return;
    (m.entries || []).forEach((e, i) => { if (s.entries[m.start + i] === undefined) { s.entries[m.start + i] = e; s.got++; } });
    if (s.got < m.total) return;
    R.snapIn = null;
    R.lobby = m.lobby;
    applyBans();
    R.presence = filterPresence(m.presence || {});
    R.init = m.init || null;
    R.log = s.entries;
    R.syncing = false;
    if (!Number.isFinite(bestRtt) && typeof m.now === 'number') offset = m.now - Date.now();
    drain();
    notifyLobby();
    notifyPres();
    const fin = R.snapReq.get(m.rid);
    if (fin) fin(true);
  }
  const filterPresence = (p) => { const o = {}; for (const [k, v] of Object.entries(p || {})) if (R.members.includes(k)) o[k] = v; return o; };

  // ---- members ----
  async function refreshMembers() {
    const r = R;
    if (!r) return;
    const [m, o] = await Promise.all([S.lobbyMembers(r.id), S.lobbyOwner(r.id)]);
    if (R !== r || !m || !m.length) return;
    r.members = m.map(String);
    r.owner = o ? String(o) : null;
    let changed = false;
    for (const uid of Object.keys(r.presence)) if (!r.members.includes(uid) || banned(uid)) { delete r.presence[uid]; changed = true; }
    for (const k of r.limits.keys()) if (!r.members.includes(k.split(':')[0])) r.limits.delete(k);
    for (const uid of r.lastIntent.keys()) if (!r.members.includes(uid)) r.lastIntent.delete(uid);
    if (changed) { if (isAuth()) commitPresence(); else notifyPres(); }
  }
  const enqueue = (fn) => {
    const r = R;
    if (!r || r.queued >= 256) return;
    r.queued++;
    r.inbox = r.inbox.then(() => (R === r ? fn() : null)).catch((e) => console.warn('Digi-Tactics Steam:', e)).finally(() => r.queued--);
  };

  async function tick() {
    if (!R) return;
    await refreshMembers();
    if (!R || !R.lobby) return;
    // the Steam lobby owner writes the list data; keep the owner on the game host
    if (R.owner === T.uid && R.lobby.host !== T.uid && R.members.includes(R.lobby.host)) S.setLobbyOwner(R.id, R.lobby.host);
    if (isAuth()) writeLobbyData();
    // host gone (also on the lobby screen, where net.js does not watch): the successor takes over
    if (!R.members.includes(R.lobby.host)) {
      R.hostGoneAt = R.hostGoneAt || Date.now();
      if (Date.now() - R.hostGoneAt > HOST_GONE_MS && successor(R.lobby.host) === T.uid) becomeHost(mySeatName());
    } else R.hostGoneAt = null;
    // clock: follow the host's clock (Lock timers count from the host's timestamps)
    if (!isAuth() && Date.now() - R.lastPing > 5000) { R.lastPing = Date.now(); send(R.lobby.host, { t: 'ping', t0: Date.now() }); }
  }

  // ---- incoming ----
  function onMessage(ev) {
    const from = String(ev.from);
    if (!R || from === T.uid || banned(from) || !R.members.includes(from)) return;
    if (!Guard.allow(R.limits, from + ':wire', 100, 300)) { R.dropped++; return; }
    const m = Guard.parse(ev.data);
    if (!m || m.l !== R.id) { R.dropped++; return; }
    const group = ['snapreq', 'logreq'].includes(m.t) ? 'sync' : m.t === 'fx' ? 'fx' : m.t === 'intent' ? 'intent' : ['seat', 'unseat', 'pres', 'presup', 'presdel', 'ping'].includes(m.t) ? 'request' : null;
    const limits = { sync: [0.25, 2], fx: [0.5, 2], intent: [8, 16], request: [3, 6] };
    if (group && !Guard.allow(R.limits, from + ':' + group, ...limits[group])) { R.dropped++; return; }
    enqueue(async () => {
      if (!R.members.includes(from) || banned(from)) return;
      handle(from, m);
    });
  }
  function handle(from, m) {
    const auth = isAuth();
    switch (m.t) {
      // requests to the host
      case 'snapreq':
        if (auth) return sendSnapshot(from, m.rid);
        // the host itself asks (it restarted and lost its copy): its successor takes over and answers
        if (R.lobby && from === R.lobby.host && successor(from) === T.uid) { becomeHost(mySeatName()); sendSnapshot(from, m.rid); }
        return;
      case 'seat': if (auth) send(from, { t: 'seatres', rid: m.rid, ok: applySeat(from, m.i, m.name) }); return;
      case 'unseat': if (auth && R.lobby.slots && R.lobby.slots[m.i] && R.lobby.slots[m.i].uid === from) freeSeatLocal(m.i); return;
      case 'pres':
      case 'presup': {
        if (!auth) return;
        const rec = m.t === 'pres' ? { at: T.now() } : { ...(R.presence[from] || { at: T.now() }) };
        if (m.t === 'presup' && !R.presence[from]) return;
        for (const k of PRES_KEYS) if (m.rec && k in m.rec) rec[k] = k === 'name' ? cleanRemote(m.rec[k]) : clone(m.rec[k]);
        R.presence[from] = rec;
        return commitPresence();
      }
      case 'presdel': if (auth && R.presence[from]) { delete R.presence[from]; commitPresence(); } return;
      case 'intent': {
        if (!auth || R.lobby.slots?.[m.s]?.uid !== from || m.q <= (R.lastIntent.get(from) || -1)) return;
        R.lastIntent.set(from, m.q);
        emitIntent({ uid: from, s: m.s, a: m.a, g: m.g, round: m.round });
        return;
      }
      case 'logreq':
        if (!auth) return;
        for (let k = Math.max(0, m.from | 0); k < R.log.length; k += CHUNK) send(from, { t: 'logs', start: k, entries: R.log.slice(k, k + CHUNK) });
        return;
      case 'ping': send(from, { t: 'pong', t0: m.t0, now: T.now() }); return;
      // answers and broadcasts from the host
      case 'pong': {
        const rtt = Date.now() - m.t0;
        if (from === hostId() && rtt >= 0 && rtt <= bestRtt * 1.5 + 20) { bestRtt = Math.min(bestRtt, rtt); offset = m.now + rtt / 2 - Date.now(); }
        return;
      }
      case 'seatres': if (from === hostId()) { const f = R.seatReq.get(m.rid); if (f) f(!!m.ok); } return;
      case 'snap': return onSnapshot(from, m);
      case 'lobby': {
        const cur = hostId();
        const claim = m.lobby && m.lobby.host === from;
        // two players claimed the host at once: the one earlier in seat order keeps it
        const ok = expectedHost(from) && (from === cur || claim);
        if (!ok) return;
        if (!m.lobby) { R.lobby = null; for (const cb of R.cbs.lobby) later(() => cb(null)); return; }
        R.lobby = m.lobby;
        applyBans();
        return notifyLobby();
      }
      case 'presence': if (from === hostId()) { R.presence = filterPresence(m.presence); notifyPres(); } return;
      case 'init': if (from === hostId() && !R.init) R.init = m.init; return;
      case 'log': if (from === hostId()) onLogEntry(m.n | 0, m.e); return;
      case 'logs': if (from === hostId()) (m.entries || []).forEach((e, i) => onLogEntry((m.start | 0) + i, e)); return;
      case 'head':
        if (from !== hostId()) return;
        if (R.log.length > m.n) { R.log.length = m.n; R.pending.clear(); resetLog(); } else if (R.log.length < m.n) send(from, { t: 'logreq', from: R.log.length });
        return;
      case 'fx': if (R.lobby?.slots?.[m.v.s]?.uid === from) { const v = { ...m.v, uid: from }; for (const cb of R.cbs.fx) later(() => cb(v)); } return;
    }
  }

  function enterSteamRoom(id) {
    R = newRoom(id);
    R.timer = setInterval(() => enqueue(tick), 1000);
  }
  function needAuth() { if (!isAuth()) throw new Error('only the host can change the lobby'); }

  const T = {
    kind: 'steam',
    uid: null,
    personaName: null,
    now: () => Math.round(Date.now() + offset), // RTT/2 can be fractional; wire timestamps are integer milliseconds
    onChange: null,

    async connect() {
      if (T.uid) return;
      const info = await S.init();
      if (!info || !info.ok) throw new Error((info && info.error) || 'Steam is not running. Start Steam, then restart the game.');
      T.uid = String(info.steamId);
      T.personaName = info.name || null;
      S.on('message', onMessage);
      S.on('lobbyChat', (e) => { if (R && String(e.lobbyId) === R.id) enqueue(refreshMembers); });
    },

    // ---- lobbies ----
    watchLobbies(cb) {
      let stop = false;
      let timer = null;
      const ours = (d) => d && d.dt_game === GAME_KEY && d.dt_ver === PROTO_VERSION;
      const row = (l, friend) => {
        const d = l.data || {};
        let names = [];
        try { names = JSON.parse(d.seats || '[]'); } catch (e) { names = []; }
        const slots = {};
        (Array.isArray(names) ? names : []).slice(0, 8).forEach((n, i) => { if (n) slots[i] = { uid: '', name: cleanRemote(n) }; });
        return { id: String(l.id), name: cleanRemote(d.name, 32) || 'Lobby', hostName: cleanRemote(d.hostName), status: d.state || 'lobby', planTime: Number(d.rounds) || 30, format: d.format === 'duos' ? 'duos' : 'solo', setId: d.setId || null, pace: ['hyper', 'normal', 'chaos'].includes(d.pace) ? d.pace : 'hyper', round: d.round || '1-1', slots, friend: friend ? cleanRemote(friend) : null };
      };
      const run = async () => {
        if (stop) return;
        try {
          const [fr, pub] = await Promise.all([S.friendLobbies().catch(() => []), S.listLobbies({ dt_game: GAME_KEY, dt_ver: PROTO_VERSION }).catch(() => [])]);
          const rows = [];
          const seen = new Set();
          for (const l of fr || []) if (ours(l.data) && !seen.has(String(l.id))) { seen.add(String(l.id)); rows.push(row(l, l.friendName || 'Friend')); }
          for (const l of pub || []) if (ours(l.data) && !seen.has(String(l.id))) { seen.add(String(l.id)); rows.push(row(l)); }
          if (!stop) cb(rows.filter((r) => r.status !== 'over'));
        } catch (e) { if (!stop) cb([]); }
        if (!stop) timer = setTimeout(run, 4000);
      };
      run();
      return () => { stop = true; clearTimeout(timer); };
    },
    async createLobby(rec, setId, opts = {}) {
      if (R) await T.leave(R.id);
      const id = await S.createLobby(opts.visibility === 'friends' ? 'friends' : 'public', 8);
      if (!id) throw new Error('Steam could not create a lobby');
      enterSteamRoom(String(id));
      R.lobby = { ...clone(rec), setId };
      R.members = [T.uid];
      R.owner = T.uid;
      writeLobbyData();
      return { id: R.id, setStored: true };
    },
    async getLobby(id) {
      id = String(id);
      if (R && R.id === id && R.lobby) return clone(R.lobby);
      if (R) await T.leave(R.id);
      const res = await S.joinLobby(id);
      if (!res || !res.ok) return null;
      const data = await S.getLobbyData(id);
      if (data.dt_game !== GAME_KEY || data.dt_ver !== PROTO_VERSION) { await S.leaveLobby(id); return null; }
      enterSteamRoom(id);
      await refreshMembers();
      if (!R || R.members.filter((m) => m !== T.uid).length === 0) { await T.leave(id); return null; }
      const ok = await requestSnapshot();
      if (!ok || !R || !R.lobby || banned(T.uid)) { await T.leave(id); return null; }
      return clone(R.lobby);
    },
    watchLobby(id, cb) { return watch(R.cbs.lobby, cb, () => R && R.cbs.lobby.has(cb) && cb(clone(R.lobby))); },
    async updateLobby(id, patch) {
      needAuth();
      const l = { ...R.lobby };
      for (const [k, v] of Object.entries(patch || {})) { if (v == null) delete l[k]; else l[k] = clone(v); }
      R.lobby = l;
      commitLobby();
    },
    async removeLobby() {
      needAuth();
      broadcast({ t: 'lobby', lobby: null });
      R.lobby = null;
      for (const cb of R.cbs.lobby) later(() => cb(null));
      R.lastLobbyData = '';
      if (R.owner === T.uid) S.setLobbyData(R.id, { state: 'over' });
    },
    async claimSeat(id, i, seat) {
      if (!R || !R.lobby) return false;
      if (isAuth()) return applySeat(T.uid, i, seat.name);
      const rid = ++R.rid;
      const r = R;
      return new Promise((resolve) => {
        const t = setTimeout(() => { r.seatReq.delete(rid); resolve(false); }, 6000);
        r.seatReq.set(rid, (ok) => { clearTimeout(t); r.seatReq.delete(rid); resolve(ok); });
        send(hostId(), { t: 'seat', rid, i, name: seat.name });
      });
    },
    async freeSeat(id, i) {
      if (!R || !R.lobby) return;
      if (isAuth()) freeSeatLocal(i); else send(hostId(), { t: 'unseat', i });
    },
    async claimHost(id, name) {
      if (!R || !R.lobby || isAuth()) return;
      if (R.members.includes(R.lobby.host)) throw new Error('the host is still in the lobby');
      if (successor(R.lobby.host) !== T.uid) return;
      becomeHost(name);
    },
    async ban(uid) {
      needAuth();
      if (!Guard.id(uid) || uid === T.uid || banned(uid) || (R.lobby.banned || []).length >= 128) return false;
      const slots = { ...R.lobby.slots };
      for (const [i, s] of Object.entries(slots)) if (s.uid === uid) delete slots[i];
      // Broadcast before blocking the native session so an unmodified client can leave promptly.
      R.lobby = { ...R.lobby, slots, banned: [...(R.lobby.banned || []), uid] };
      S.send(uid, JSON.stringify({ t: 'lobby', l: R.id, lobby: R.lobby }), true);
      delete R.presence[uid];
      applyBans(); commitLobby(); commitPresence();
      return true;
    },

    // ---- presence and loadouts ----
    async joinPresence(id, rec) {
      R.myPres = clone(rec);
      if (isAuth()) { R.presence[T.uid] = { ...clone(rec), at: T.now() }; commitPresence(); } else send(hostId(), { t: 'pres', rec });
    },
    async updatePresence(id, patch) {
      if (!R) return;
      R.myPres = { ...(R.myPres || {}), ...clone(patch) };
      if (isAuth()) { if (R.presence[T.uid]) { Object.assign(R.presence[T.uid], clone(patch)); commitPresence(); } } else send(hostId(), { t: 'presup', rec: patch });
    },
    async leavePresence() {
      if (!R) return;
      R.myPres = null;
      if (isAuth()) { delete R.presence[T.uid]; commitPresence(); } else if (R.lobby) send(hostId(), { t: 'presdel' });
    },
    watchPresence(id, cb) { return watch(R.cbs.pres, cb, () => R && R.cbs.pres.has(cb) && cb(clone(R.presence))); },

    // ---- game init and the host-ordered log ----
    async writeInit(id, init) {
      needAuth();
      R.init = clone(init);
      broadcast({ t: 'init', init });
    },
    async readGame() {
      if (!R) return { init: null, log: [] };
      if (!isAuth()) await requestSnapshot();
      if (!R) return { init: null, log: [] };
      return { init: clone(R.init), log: R.log.map((e, i) => ({ key: logKey(i), e: clone(e) })) };
    },
    watchLog(id, afterKey, cb, onReset) {
      const w = { after: keyIndex(afterKey), cb, onReset, off: false };
      return watch(R.cbs.log, w, () => deliver(w));
    },
    async appendLog(id, seq, e) {
      needAuth();
      if (seq >= Guard.MAX_LOG) { await T.removeLobby(); throw new Error('lobby reached its safety limit'); }
      if (!Guard.entry(e)) throw new Error('invalid game log entry');
      if (seq !== R.log.length) throw new Error('the log moved on');
      R.log.push(clone(e));
      broadcast({ t: 'log', n: seq, e });
      later(deliverAll);
    },
    async sendIntent(id, it) {
      if (!R || !R.lobby) return;
      if (isAuth()) emitIntent({ ...clone(it), uid: T.uid });
      else send(hostId(), { t: 'intent', s: it.s, a: it.a, g: it.g, round: it.round, q: ++R.outgoingIntent });
    },
    watchIntents(id, cb) { return watch(R.cbs.intent, cb); },
    dropIntent() {},

    // ---- fx: straight to every member, unreliable ----
    sendFx(id, v) { broadcast({ t: 'fx', v }, false); },
    watchFx(id, cb) { return watch(R.cbs.fx, cb); },
    sweepFx() {},

    // ---- invites, names, pictures ----
    inviteLink: () => null,
    invite(id) { S.inviteDialog(String(id)); },
    onJoinRequest(cb) { return S.on('joinRequested', (e) => { if (e && e.lobbyId) cb(String(e.lobbyId)); }); },
    async launchLobby() { const id = await S.launchLobby(); return id ? String(id) : null; },
    avatar(uid) {
      if (avatars.has(uid)) return avatars.get(uid);
      avatars.set(uid, null);
      Promise.resolve(S.persona(uid)).then((p) => {
        if (p && p.avatar) { avatars.set(uid, p.avatar); if (T.onChange) T.onChange(); } else setTimeout(() => avatars.delete(uid), 5000);
      }, () => setTimeout(() => avatars.delete(uid), 5000));
      return null;
    },
    // for tests: what this client holds
    debug: () => (R ? { id: R.id, members: [...R.members], owner: R.owner, host: hostId(), auth: isAuth(), init: !!R.init, log: R.log.length, pending: R.pending.size, queued: R.queued, dropped: R.dropped, syncing: R.syncing, presence: Object.keys(R.presence), offset } : null),
    async leave(id) {
      if (!R || R.id !== String(id)) return;
      const r = R;
      clearInterval(r.timer);
      R = null;
      await new Promise((res) => setTimeout(res, 250)); // let the last messages go out
      await S.leaveLobby(r.id);
    },
  };
  return T;
}
