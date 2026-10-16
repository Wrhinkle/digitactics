// Steam for the main process: steamworks.js (init, callbacks, lobbies, lobby data, overlay) plus koffi calls into the same
// steam_api64.dll for what steamworks.js 0.4 does not wrap: ISteamNetworkingMessages (relayed P2P messages; steamworks.js
// only has the old ISteamNetworking P2P API), lobby list filters, lobby owner changes, friends' lobbies, persona names and
// avatars of other players. App ID 480 (Spacewar), the public Steamworks test app.
//
// Native safety rules (a wrong pointer or buffer here kills the whole process with no JS exception):
//   - struct layouts follow the SDK headers (Windows packing: pack(8), SteamNetworkingIdentity pack(1)); tests/steam-structs.cjs
//     checks every size and offset koffi computes against the values MSVC gives for the SDK headers
//   - never koffi.view(): Electron forbids external ArrayBuffers and koffi aborts the process (napi_fatal_error) when asked
//     for one. Message bytes are copied into a Node Buffer with RtlMoveMemory instead.
//   - every received message is released exactly once; null pointers, bad sizes and unknown identity types are skipped
//   - avatar handles 0 (none) and -1 (still downloading) are never passed on; RGBA buffers are sized from GetImageSize
// DEBUG_NATIVE logs a line before and after each native call on the join path (first calls of each kind, then a sample).
const APP_ID = 480;
const CHANNEL = 0;
const SEND_UNRELIABLE = 1; // k_nSteamNetworkingSend_UnreliableNoNagle
const SEND_RELIABLE = 9; // k_nSteamNetworkingSend_ReliableNoNagle
const AUTO_RESTART = 32; // k_nSteamNetworkingSend_AutoRestartBrokenSession
const MAX_MESSAGE = 512 * 1024; // k_cbMaxSteamNetworkingSocketsMessageSizeSend
const IDENTITY_STEAMID = 16; // k_ESteamNetworkingIdentityType_SteamID
const FRIEND_IMMEDIATE = 4; // k_EFriendFlagImmediate
const LOBBY_EQUAL = 0; // k_ELobbyComparisonEqual
const DISTANCE_WORLDWIDE = 3; // k_ELobbyDistanceFilterWorldwide
const LIST_KEYS = ['dt_game', 'dt_ver', 'name', 'host', 'hostName', 'setId', 'format', 'pace', 'rounds', 'state', 'round', 'seats'];
const NOT_RUNNING = 'Steam is not running. Start Steam, then reopen Digi-Tactics.';
const DEBUG_NATIVE = process.env.DT_NATIVE_DEBUG === '1'; // opt in; public traffic must not flood disk logs
// steamworks.js and koffi live in the shell's node_modules (boot.js), not next to this payload file
const shellRequire = (global.DTBoot && global.DTBoot.require) || require;

let sw = null; // steamworks.js module
let client = null; // steamworks.js client
let api = null; // koffi functions and interface pointers
let info = { ok: false, error: NOT_RUNNING };
const lobbies = new Map(); // id -> steamworks.js Lobby we created or joined
const dataWaits = new Map(); // lobby id -> [resolve] waiting for LobbyDataUpdate
const requested = new Set(); // steam ids we asked Steam to load persona data for
const blocked = new Set();
const traffic = new Map();
function receiveBudget(id, bytes) {
  const now = performance.now();
  const b = traffic.get(id) || { at: now, count: 300, bytes: 8 * 1024 * 1024 };
  const dt = Math.max(0, now - b.at) / 1000;
  b.count = Math.min(300, b.count + dt * 100);
  b.bytes = Math.min(8 * 1024 * 1024, b.bytes + dt * 1024 * 1024);
  b.at = now;
  if (traffic.size >= 128 && !traffic.has(id)) return false;
  traffic.set(id, b);
  if (b.count < 1 || b.bytes < bytes) return false;
  b.count--; b.bytes -= bytes;
  return true;
}
let emit = () => {};
let log = () => {};
let isMember = (steamId) => memberOfOurs(steamId);

const isId = (s) => typeof s === 'string' && /^\d{1,20}$/.test(s) && BigInt(s) < 1n << 64n;
const big = (s) => BigInt(s);

// ---- native call tracing ----
const traceCounts = new Map();
const fmt = (v) => (typeof v === 'bigint' ? `${v}n` : typeof v === 'object' && v !== null ? JSON.stringify(v, (k, x) => (typeof x === 'bigint' ? String(x) : x)) : String(v));
function shouldTrace(kind) {
  const n = (traceCounts.get(kind) || 0) + 1;
  traceCounts.set(kind, n);
  return n <= 40 || n % 500 === 0 ? n : 0;
}
// run fn (one native call) with a log line before and after it
function nat(kind, detail, fn) {
  const n = DEBUG_NATIVE ? shouldTrace(kind) : 0;
  if (n) log(`native> ${kind}#${n} ${detail}`);
  const r = fn();
  if (n) log(`native< ${kind}#${n} ${fmt(r)}`);
  return r;
}
function note(kind, msg) { const n = DEBUG_NATIVE ? shouldTrace(kind) : 0; if (n) log(`native  ${kind}#${n} ${msg}`); }

// ---- types (layouts from the Steamworks SDK headers, Windows x64) ----
const typeCache = new Map();
function defineTypes(koffi) {
  if (typeCache.has(koffi)) return typeCache.get(koffi);
  // steamnetworkingtypes.h, #pragma pack(push,1): m_eType, m_cbSize, then a 128-byte union starting with m_steamID64
  const Identity = koffi.pack('DT_SteamNetworkingIdentity', { eType: 'int32', cbSize: 'int32', steamID64: 'uint64', rest: koffi.array('uint8', 120) });
  // steamnetworkingtypes.h, pack(8) on Windows; 216 bytes
  const Message = koffi.struct('DT_SteamNetworkingMessage_t', {
    data: 'void *', size: 'int32', conn: 'uint32', peer: Identity,
    connUserData: 'int64', timeReceived: 'int64', messageNumber: 'int64', freeData: 'void *', release: 'void *',
    channel: 'int32', flags: 'int32', userData: 'int64', lane: 'uint16', pad: 'uint16',
  });
  // isteamfriends.h, pack(8) on Windows; 24 bytes
  const FriendGameInfo = koffi.struct('DT_FriendGameInfo_t', { gameID: 'uint64', ip: 'uint32', port: 'uint16', queryPort: 'uint16', lobby: 'uint64' });
  const t = { Identity, Message, FriendGameInfo };
  typeCache.set(koffi, t);
  return t;
}

// ---- flat API bindings ----
let raw = null;
function bindFlatRaw(libPath) {
  if (raw && !libPath) return { ...raw };
  const koffi = shellRequire('koffi');
  let lib;
  if (libPath) lib = koffi.load(libPath);
  else {
    // by base name: Windows hands back the steam_api64.dll steamworks.js already loaded (and initialised)
    try { lib = koffi.load('steam_api64.dll'); } catch (e) { lib = koffi.load(require.resolve('steamworks.js/dist/win64/steam_api64.dll').replace('app.asar', 'app.asar.unpacked')); }
  }
  const k32 = koffi.load('kernel32.dll');
  const { Identity, Message, FriendGameInfo } = defineTypes(koffi);
  const f = (sig) => lib.func(sig);
  const a = {
    koffi, Identity, Message, FriendGameInfo,
    copy: k32.func('void RtlMoveMemory(void *dst, void *src, size_t n)'),
    SNM: f('void *SteamAPI_SteamNetworkingMessages_SteamAPI_v002()'),
    MM: f('void *SteamAPI_SteamMatchmaking_v009()'),
    FR: f('void *SteamAPI_SteamFriends_v017()'),
    UT: f('void *SteamAPI_SteamUtils_v010()'),
    NU: f('void *SteamAPI_SteamNetworkingUtils_SteamAPI_v004()'),
    sendMessage: f('int SteamAPI_ISteamNetworkingMessages_SendMessageToUser(void *self, void *identity, void *data, uint32 size, int flags, int channel)'),
    receive: f('int SteamAPI_ISteamNetworkingMessages_ReceiveMessagesOnChannel(void *self, int channel, _Out_ void **out, int max)'),
    accept: f('bool SteamAPI_ISteamNetworkingMessages_AcceptSessionWithUser(void *self, void *identity)'),
    closeSession: f('bool SteamAPI_ISteamNetworkingMessages_CloseSessionWithUser(void *self, void *identity)'),
    setGlobalInt: f('bool SteamAPI_ISteamNetworkingUtils_SetGlobalConfigValueInt32(void *self, int key, int32 value)'),
    release: f('void SteamAPI_SteamNetworkingMessage_t_Release(void *msg)'),
    initRelay: f('void SteamAPI_ISteamNetworkingUtils_InitRelayNetworkAccess(void *self)'),
    filterString: f('void SteamAPI_ISteamMatchmaking_AddRequestLobbyListStringFilter(void *self, const char *key, const char *value, int cmp)'),
    filterDistance: f('void SteamAPI_ISteamMatchmaking_AddRequestLobbyListDistanceFilter(void *self, int distance)'),
    filterCount: f('void SteamAPI_ISteamMatchmaking_AddRequestLobbyListResultCountFilter(void *self, int max)'),
    getLobbyData: f('const char *SteamAPI_ISteamMatchmaking_GetLobbyData(void *self, uint64 lobby, const char *key)'),
    requestLobbyData: f('bool SteamAPI_ISteamMatchmaking_RequestLobbyData(void *self, uint64 lobby)'),
    numMembers: f('int SteamAPI_ISteamMatchmaking_GetNumLobbyMembers(void *self, uint64 lobby)'),
    setOwner: f('bool SteamAPI_ISteamMatchmaking_SetLobbyOwner(void *self, uint64 lobby, uint64 owner)'),
    friendCount: f('int SteamAPI_ISteamFriends_GetFriendCount(void *self, int flags)'),
    friendByIndex: f('uint64 SteamAPI_ISteamFriends_GetFriendByIndex(void *self, int i, int flags)'),
    gamePlayed: f('bool SteamAPI_ISteamFriends_GetFriendGamePlayed(void *self, uint64 id, _Out_ DT_FriendGameInfo_t *info)'),
    personaName: f('const char *SteamAPI_ISteamFriends_GetFriendPersonaName(void *self, uint64 id)'),
    requestUser: f('bool SteamAPI_ISteamFriends_RequestUserInformation(void *self, uint64 id, bool nameOnly)'),
    mediumAvatar: f('int SteamAPI_ISteamFriends_GetMediumFriendAvatar(void *self, uint64 id)'),
    imageSize: f('bool SteamAPI_ISteamUtils_GetImageSize(void *self, int image, _Out_ uint32 *w, _Out_ uint32 *h)'),
    imageRGBA: f('bool SteamAPI_ISteamUtils_GetImageRGBA(void *self, int image, void *dest, int size)'),
  };
  if (!libPath) raw = a;
  return { ...a };
}

function bindFlat(libPath) {
  const a = bindFlatRaw(libPath);
  for (const k of ['SNM', 'MM', 'FR', 'UT', 'NU']) a[k] = a[k]();
  if (!a.SNM || !a.MM || !a.FR || !a.UT) throw new Error('Steam interfaces are not available');
  return a;
}

// SteamNetworkingIdentity for a Steam user, built in place (same bytes as SteamNetworkingIdentity::SetSteamID64)
function identity(steamId) {
  const buf = Buffer.alloc(136);
  buf.writeInt32LE(IDENTITY_STEAMID, 0);
  buf.writeInt32LE(8, 4);
  buf.writeBigUInt64LE(BigInt(steamId), 8);
  return buf;
}

// ---- init ----
function init(opts = {}) {
  emit = opts.emit || emit;
  log = opts.log || log;
  process.env.SteamAppId = String(APP_ID); // so Steam never relaunches the exe through the Steam client
  process.env.SteamGameId = String(APP_ID);
  try {
    sw = shellRequire('steamworks.js');
    client = sw.init(APP_ID);
  } catch (e) {
    log(`steam init failed: ${e.message}`);
    info = { ok: false, error: /not running|IPC|pipe/i.test(e.message) ? NOT_RUNNING : `Steam could not start: ${e.message}` };
    return info;
  }
  try { api = bindFlat(); } catch (e) {
    log(`steam flat API failed: ${e.stack || e}`);
    info = { ok: false, error: `Steam networking is not available: ${e.message}` };
    return info;
  }
  // 104 = P2P_Transport_ICE_Enable. Disable direct ICE routes/address sharing before creating any peer sessions.
  // Fail closed instead of silently exposing player IPs when this Steam runtime cannot apply the setting.
  if (!api.NU || !api.setGlobalInt(api.NU, 104, 0)) {
    api = null;
    info = { ok: false, error: 'Steam could not enable private relay networking. Solo play is still available.' };
    return info;
  }
  const me = client.localplayer.getSteamId();
  info = { ok: true, steamId: String(me.steamId64), name: client.localplayer.getName(), appId: client.utils.getAppId() };
  log(`steam ready: app ${info.appId}, user ${info.steamId}, native debug ${DEBUG_NATIVE ? 'on' : 'off'}`);
  try { if (api.NU) api.initRelay(api.NU); } catch (e) { log(`relay init: ${e.message}`); }
  const CB = sw.SteamCallback;
  client.callback.register(CB.LobbyChatUpdate, (v) => {
    try {
      // steamworks.js documents a numeric enum (Entered = 0); accept the serde variant name too
      const s = v.member_state_change;
      const entered = s === 0 || s === 'Entered';
      const user = String(v.user_changed);
      log(`lobby ${v.lobby}: ${user} ${entered ? 'entered' : `left (${fmt(s)})`}`);
      if (entered && isId(user) && user !== info.steamId) acceptUser(user);
      emit('lobbyChat', { lobbyId: String(v.lobby), user, change: entered ? 'entered' : 'left' });
    } catch (e) { log(`lobbyChat: ${e.stack || e}`); }
  });
  client.callback.register(CB.GameLobbyJoinRequested, (v) => {
    try {
      log(`join requested: lobby ${v.lobby_steam_id} via ${v.friend_steam_id}`);
      emit('joinRequested', { lobbyId: String(v.lobby_steam_id), friendId: String(v.friend_steam_id) });
    } catch (e) { log(`joinRequested: ${e.stack || e}`); }
  });
  client.callback.register(CB.LobbyDataUpdate, (v) => {
    try {
      const id = String(v.lobby);
      const w = dataWaits.get(id);
      if (w) { dataWaits.delete(id); w.forEach((r) => r()); }
    } catch (e) { log(`lobbyData: ${e.stack || e}`); }
  });
  setInterval(poll, 15);
  setInterval(acceptMembers, 1000);
  return info;
}

// ---- messages ----
// one received message: copy what we need out of Steam's memory, then release it (always, exactly once)
function readMessage(ptr) {
  let msg = null;
  try {
    const m = nat('decode', `msg ${ptr}`, () => api.koffi.decode(ptr, api.Message));
    const size = m.size | 0;
    if (m.peer.eType !== IDENTITY_STEAMID) { note('skip', `identity type ${m.peer.eType}`); return null; }
    const from = String(BigInt(m.peer.steamID64));
    if (!m.data || size <= 0 || size > MAX_MESSAGE) { note('skip', `from ${from}: size ${size}, data ${m.data ? 'set' : 'null'}`); return null; }
    if (blocked.has(from) || !isMember(from) || !receiveBudget(from, size)) return null;
    const bytes = Buffer.alloc(size);
    nat('copy', `${size} bytes from ${from}`, () => api.copy(bytes, m.data, size));
    msg = { from, data: bytes.toString('utf8') };
  } catch (e) {
    log(`receive: ${e.stack || e}`);
  } finally {
    nat('release', `msg ${ptr}`, () => api.release(ptr));
  }
  return msg;
}
let pollCount = 0;
function poll() {
  if (!api) return;
  const out = [];
  for (let n = 0; n < 64; n++) {
    const box = [null];
    let got = 0;
    try { got = api.receive(api.SNM, CHANNEL, box, 1); } catch (e) { log(`receive call: ${e.stack || e}`); return; }
    if (++pollCount % 4000 === 0 && DEBUG_NATIVE) log(`native  poll#${pollCount} queue ${got > 0 ? 'has messages' : 'empty'}`);
    if (got < 1) break;
    const ptr = box[0];
    note('receive', `got ${got}, msg ${ptr}`);
    if (!ptr) { log('receive: Steam returned a message count without a message'); break; }
    const m = readMessage(ptr);
    if (!m) continue;
    if (isMember(m.from)) out.push(m);
    else note('drop', `message from ${m.from}, not in our lobby`);
  }
  for (const m of out) emit('message', m);
}
// only players in a lobby we are in may talk to us
function memberOfOurs(steamId) {
  for (const l of lobbies.values()) { try { if (l.getMembers().some((p) => String(p.steamId64) === steamId)) return true; } catch (e) { /* lobby gone */ } }
  return false;
}
function acceptUser(steamId) {
  if (!api || !isId(steamId) || blocked.has(steamId) || !isMember(steamId)) return false;
  try { return nat('accept', steamId, () => api.accept(api.SNM, identity(steamId))); } catch (e) { log(`accept ${steamId}: ${e.stack || e}`); return false; }
}
// Steam holds messages from a new peer until we accept the session (SteamNetworkingMessagesSessionRequest_t, which
// steamworks.js does not expose): accept every member of our lobbies, right away on LobbyChatUpdate and then each second.
// Sending to a peer accepts its session as well.
function acceptMembers() {
  for (const id of traffic.keys()) if (!isMember(id)) traffic.delete(id);
  for (const l of lobbies.values()) {
    let members = [];
    try { members = l.getMembers(); } catch (e) { continue; }
    for (const p of members) if (String(p.steamId64) !== info.steamId) acceptUser(String(p.steamId64));
  }
}
function send(to, text, reliable) {
  if (!api || !isId(to) || to === info.steamId || blocked.has(to) || !isMember(to) || typeof text !== 'string' || text.length > MAX_MESSAGE) return false;
  const data = Buffer.from(text, 'utf8');
  if (!data.length || data.length > MAX_MESSAGE) { log(`send to ${to}: ${data.length} bytes is out of range`); return false; }
  let r;
  try {
    r = nat('send', `${data.length} bytes to ${to}${reliable ? ' reliable' : ''}`, () => api.sendMessage(api.SNM, identity(to), data, data.length, (reliable ? SEND_RELIABLE : SEND_UNRELIABLE) | AUTO_RESTART, CHANNEL));
  } catch (e) { log(`send to ${to}: ${e.stack || e}`); return false; }
  if (r !== 1) note(`send-fail-${r}`, `to ${to}: EResult ${r}`); // 1 = k_EResultOK
  return r === 1;
}

// ---- lobbies ----
async function createLobby(type, max) {
  const lobby = await client.matchmaking.createLobby(type === 'friends' ? 1 : 2, Math.max(2, Math.min(8, max | 0 || 8)));
  lobbies.set(String(lobby.id), lobby);
  log(`lobby ${lobby.id} created (${type})`);
  return String(lobby.id);
}
async function joinLobby(id) {
  if (!isId(id)) return { ok: false, error: 'bad lobby id' };
  try {
    log(`joining lobby ${id}`);
    const lobby = await client.matchmaking.joinLobby(big(id));
    lobbies.set(String(lobby.id), lobby);
    let members = [];
    try { members = lobby.getMembers().map((p) => String(p.steamId64)); } catch (e) { /* ignore */ }
    log(`joined lobby ${id}: members ${members.join(',')}`);
    for (const m of members) if (m !== info.steamId) acceptUser(m);
    return { ok: true };
  } catch (e) { log(`join lobby ${id} failed: ${e.message}`); return { ok: false, error: e.message }; }
}
function leaveLobby(id) {
  const l = lobbies.get(id);
  const former = lobbyMembers(id);
  if (l) { try { l.leave(); } catch (e) { /* already gone */ } lobbies.delete(id); log(`left lobby ${id}`); }
  for (const peer of former) if (peer !== info.steamId && !memberOfOurs(peer)) {
    try { api.closeSession(api.SNM, identity(peer)); } catch { /* already closed */ }
  }
  if (!lobbies.size) { blocked.clear(); traffic.clear(); requested.clear(); }
  return true;
}
function blockPeer(id) {
  if (!api || !isId(id) || id === info.steamId || blocked.size >= 128) return false;
  blocked.add(id);
  traffic.delete(id);
  setTimeout(() => { if (blocked.has(id) && api) api.closeSession(api.SNM, identity(id)); }, 500);
  return true;
}
function lobbyMembers(id) { const l = lobbies.get(id); try { return l ? l.getMembers().map((p) => String(p.steamId64)) : []; } catch (e) { return []; } }
function lobbyOwner(id) { const l = lobbies.get(id); try { return l ? String(l.getOwner().steamId64) : null; } catch (e) { return null; } }
function setLobbyOwner(id, owner) {
  if (!isId(id) || !isId(owner) || lobbyOwner(id) !== info.steamId || !lobbyMembers(id).includes(owner) || blocked.has(owner)) return false;
  return nat('setOwner', `${id} -> ${owner}`, () => api.setOwner(api.MM, big(id), big(owner)));
}
function setLobbyData(id, data) {
  const l = lobbies.get(id);
  if (!l || !data || typeof data !== 'object') return false;
  const clean = {};
  for (const [k, v] of Object.entries(data)) if (/^[\w]{1,32}$/.test(k)) clean[k] = String(v).slice(0, 4000);
  return l.mergeFullData(clean);
}
// lobby data through steamworks.js when we hold the Lobby object, else through the flat API
function readData(id, lobby) {
  const d = {};
  for (const k of LIST_KEYS) {
    let v = null;
    try { v = lobby ? lobby.getData(k) : api.getLobbyData(api.MM, big(id), k); } catch (e) { v = null; }
    d[k] = typeof v === 'string' ? v : '';
  }
  return d;
}
function getLobbyData(id) { return isId(id) ? readData(id, lobbies.get(id)) : {}; }
async function listLobbies(filters) {
  // the filters apply to the next RequestLobbyList, which steamworks.js getLobbies() makes
  for (const [k, v] of Object.entries(filters || {})) if (/^\w{1,32}$/.test(k)) api.filterString(api.MM, k, String(v).slice(0, 64), LOBBY_EQUAL);
  api.filterDistance(api.MM, DISTANCE_WORLDWIDE);
  api.filterCount(api.MM, 50);
  const list = await client.matchmaking.getLobbies();
  return list.map((l) => ({ id: String(l.id), data: readData(String(l.id), l), members: Number(l.getMemberCount()), max: Number(l.getMemberLimit() || 8) }));
}
async function friendLobbies() {
  const found = [];
  const n = api.friendCount(api.FR, FRIEND_IMMEDIATE);
  for (let i = 0; i < Math.min(n, 2000); i++) {
    const fid = BigInt(api.friendByIndex(api.FR, i, FRIEND_IMMEDIATE));
    if (!fid) continue;
    const g = {};
    if (!api.gamePlayed(api.FR, fid, g)) continue;
    if ((BigInt(g.gameID) & 0xffffffn) !== BigInt(APP_ID) || !g.lobby || BigInt(g.lobby) === 0n) continue;
    found.push({ id: String(BigInt(g.lobby)), friendId: String(fid), friendName: api.personaName(api.FR, fid) || '' });
  }
  // lobby data for lobbies we are not in arrives with LobbyDataUpdate
  await Promise.all(found.map((f) => new Promise((res) => {
    const t = setTimeout(res, 1500);
    const list = dataWaits.get(f.id) || [];
    list.push(() => { clearTimeout(t); res(); });
    dataWaits.set(f.id, list);
    if (!api.requestLobbyData(api.MM, big(f.id))) { clearTimeout(t); res(); }
  })));
  return found.map((f) => ({ ...f, data: readData(f.id), members: api.numMembers(api.MM, big(f.id)) }));
}
function inviteDialog(id) {
  if (!isId(id)) return false;
  client.overlay.activateInviteDialog(big(id));
  return true;
}

// ---- people ----
// name and 64x64 avatar of any Steam user. Steam may not have the data yet (RequestUserInformation starts the
// download): then the name is empty or "[unknown]" and the avatar handle is -1; the page asks again a few seconds later.
function persona(steamId) {
  if (!api || !isId(steamId)) return { name: '', avatar: null };
  const id = big(steamId);
  const self = steamId === info.steamId;
  if (!self && !requested.has(steamId)) {
    requested.add(steamId);
    nat('requestUser', steamId, () => api.requestUser(api.FR, id, false));
  }
  const name = nat('personaName', steamId, () => api.personaName(api.FR, id)) || '';
  let avatar = null;
  const img = nat('avatar', steamId, () => api.mediumAvatar(api.FR, id));
  if (Number.isInteger(img) && img > 0) {
    const w = [0];
    const h = [0];
    const okSize = nat('imageSize', `image ${img}`, () => api.imageSize(api.UT, img, w, h));
    const W = Number(w[0]);
    const H = Number(h[0]);
    if (okSize && W > 0 && W <= 256 && H > 0 && H <= 256) {
      const rgba = Buffer.alloc(W * H * 4);
      if (nat('imageRGBA', `image ${img} ${W}x${H} into ${rgba.length} bytes`, () => api.imageRGBA(api.UT, img, rgba, rgba.length))) {
        for (let i = 0; i < rgba.length; i += 4) { const r = rgba[i]; rgba[i] = rgba[i + 2]; rgba[i + 2] = r; } // RGBA -> BGRA
        const { nativeImage } = require('electron');
        avatar = nat('toPng', `${W}x${H}`, () => nativeImage.createFromBitmap(rgba, { width: W, height: H }).toDataURL()) || null;
      }
    } else note('avatar-size', `${steamId}: image ${img} size ${W}x${H} ok=${okSize}`);
  }
  return { name, avatar };
}

// for the smoke test: the native pieces load and every flat-API prototype binds (works without Steam running)
function selfTest() {
  try {
    shellRequire('steamworks.js');
    const a = bindFlatRaw();
    return { ok: true, functions: Object.keys(a).filter((k) => typeof a[k] === 'function').length };
  } catch (e) { return { ok: false, error: e.message }; }
}

function shutdown() {
  for (const id of [...lobbies.keys()]) leaveLobby(id);
}

// for tests/steam-native.cjs: run the native paths against a stand-in steam_api64.dll without steamworks.js
const _test = {
  defineTypes,
  identity,
  bind(libPath, opts = {}) {
    api = bindFlat(libPath);
    info = { ok: true, steamId: String(opts.steamId || '76561190000000001'), name: 'test', appId: APP_ID };
    if (opts.log) log = opts.log;
    if (opts.emit) emit = opts.emit;
    if (opts.isMember) isMember = opts.isMember;
    return api;
  },
  poll,
  persona,
  send,
  blockPeer,
  acceptUser,
  traceCounts,
  api: () => api,
};

module.exports = {
  APP_ID, NOT_RUNNING, init, selfTest, info: () => info, send, createLobby, joinLobby, leaveLobby, lobbyMembers, lobbyOwner, setLobbyOwner,
  setLobbyData, getLobbyData, listLobbies, friendLobbies, inviteDialog, persona, shutdown, isId, blockPeer, _test,
};
