// Optional developer-configured Firebase transport for js/net.js; no shared backend is bundled.
// This is the code net.js used directly before the transport split, moved behind the interface described in net.js.
//
// Data:
//   tft/lobbies/{id}   { name, host, hostName, status: lobby|playing|over, planTime, setId, pace, created, updated, round, alive[], slots{0-7: {uid, name}} }
//   tft/games/{id}     { init: {seed, slots, planTime, at, setId, chaos, pace, chaosPace}, log: {eNNNNNN: entry}, intents: {push: {uid, s, a, g}}, fx: {push: {uid, s, kind, id, at}} }
//   tft/presence/{id}/{uid}  { name, at, look, lobbySet }, removed on disconnect
const SDK = 'https://www.gstatic.com/firebasejs/12.4.0/';
const CONFIG = globalThis.DT_FIREBASE_CONFIG || null;
const STALE_MS = 3 * 3600 * 1000;
export const logKey = (seq) => `e${String(seq).padStart(6, '0')}`;

export function firebaseTransport() {
  let fb = null; // { api, db }
  const r = (path) => fb.api.ref(fb.db, path);

  const T = {
    kind: 'firebase',
    uid: null,
    personaName: null, // Firebase has no account names: players type one
    now: () => Date.now(),
    connected: () => !!fb,

    async connect() {
      if (fb) return;
      if (!CONFIG) throw new Error('Browser online play is not configured. Download the Windows build to play online, or play solo here.');
      const load = (m) => import(SDK + m);
      const [appMod, authMod, api] = await Promise.all([load('firebase-app.js'), load('firebase-auth.js'), load('firebase-database.js')]);
      const app = appMod.initializeApp(CONFIG, 'digitactics');
      const auth = authMod.getAuth(app);
      const cred = await authMod.signInAnonymously(auth);
      const db = api.getDatabase(app);
      let offset = 0;
      api.onValue(api.ref(db, '.info/serverTimeOffset'), (s) => { offset = s.val() || 0; });
      fb = { api, db };
      T.uid = cred.user.uid;
      T.now = () => Date.now() + offset;
    },

    // ---- lobbies ----
    // cb(rows): open lobbies, most recently updated first
    watchLobbies(cb) {
      const { api } = fb;
      const q = api.query(r('tft/lobbies'), api.orderByChild('updated'), api.limitToLast(40));
      return api.onValue(q, (snap) => {
        const rows = [];
        snap.forEach((c) => { rows.push({ id: c.key, ...c.val() }); });
        cb(rows.filter((l) => l.status !== 'over' && T.now() - (l.updated || 0) < STALE_MS).reverse());
      });
    },
    async createLobby(rec, setId) {
      const { api } = fb;
      const ref = api.push(r('tft/lobbies'));
      try { await api.set(ref, { ...rec, setId }); return { id: ref.key, setStored: true }; } catch (e) {
        // older database rules: no setId field on lobbies; the host's presence carries it instead
        await api.set(ref, rec);
        return { id: ref.key, setStored: false };
      }
    },
    async getLobby(id) {
      const snap = await fb.api.get(r(`tft/lobbies/${id}`));
      return snap.exists() ? snap.val() : null;
    },
    watchLobby(id, cb) { return fb.api.onValue(r(`tft/lobbies/${id}`), (s) => cb(s.val())); },
    updateLobby(id, patch) { return fb.api.update(r(`tft/lobbies/${id}`), patch); },
    removeLobby(id) { return fb.api.remove(r(`tft/lobbies/${id}`)); },
    async claimSeat(id, i, seat) {
      const res = await fb.api.runTransaction(r(`tft/lobbies/${id}/slots/${i}`), (cur) => (cur ? undefined : seat));
      return res.committed;
    },
    freeSeat(id, i) { return fb.api.remove(r(`tft/lobbies/${id}/slots/${i}`)); },
    claimHost(id, name) { return fb.api.update(r(`tft/lobbies/${id}`), { host: T.uid, hostName: name }); },

    // ---- presence and loadouts ----
    async joinPresence(id, rec) {
      const pres = r(`tft/presence/${id}/${T.uid}`);
      await fb.api.set(pres, { ...rec, at: fb.api.serverTimestamp() });
      fb.api.onDisconnect(pres).remove();
    },
    updatePresence(id, patch) { return fb.api.update(r(`tft/presence/${id}/${T.uid}`), patch); },
    leavePresence(id) { return fb.api.remove(r(`tft/presence/${id}/${T.uid}`)); },
    watchPresence(id, cb) { return fb.api.onValue(r(`tft/presence/${id}`), (s) => cb(s.val() || {})); },

    // ---- game init and the host-ordered log ----
    writeInit(id, init) { return fb.api.set(r(`tft/games/${id}/init`), init); },
    async readGame(id) {
      const { api } = fb;
      const init = (await api.get(r(`tft/games/${id}/init`))).val();
      if (!init) return { init: null, log: [] };
      const logSnap = await api.get(r(`tft/games/${id}/log`));
      const log = [];
      logSnap.forEach((c) => { log.push({ key: c.key, e: c.val() }); });
      return { init, log };
    },
    // cb(key, entry) for every entry after afterKey, in log order. onReset is never called here: the database log is append-only.
    watchLog(id, afterKey, cb) {
      const { api } = fb;
      const q = afterKey ? api.query(r(`tft/games/${id}/log`), api.orderByKey(), api.startAfter(afterKey)) : r(`tft/games/${id}/log`);
      return api.onChildAdded(q, (c) => cb(c.key, c.val()));
    },
    appendLog(id, seq, e) { return fb.api.set(r(`tft/games/${id}/log/${logKey(seq)}`), e); },
    sendIntent(id, it) { return fb.api.push(r(`tft/games/${id}/intents`), it); },
    watchIntents(id, cb) { return fb.api.onChildAdded(r(`tft/games/${id}/intents`), (c) => cb(c.key, c.val())); },
    dropIntent(id, key) { fb.api.remove(r(`tft/games/${id}/intents/${key}`)); },

    // ---- fx (emotes, soundboard): outside the log, only recent ones ----
    sendFx(id, v) { fb.api.push(r(`tft/games/${id}/fx`), v).catch(() => {}); },
    watchFx(id, cb) {
      const { api } = fb;
      return api.onChildAdded(api.query(r(`tft/games/${id}/fx`), api.orderByChild('at'), api.startAt(T.now() - 3000)), (c) => cb(c.val()));
    },
    sweepFx(id, now) {
      const { api } = fb;
      api.get(api.query(r(`tft/games/${id}/fx`), api.orderByChild('at'), api.endAt(now - 30000))).then((s) => s.forEach((c) => { api.remove(c.ref); })).catch(() => {});
    },

    // ---- invites ----
    inviteLink(id) {
      const u = new URL(location.href);
      u.search = `?room=${id}`;
      u.hash = '';
      return u.toString();
    },
    invite: null,
    avatar: () => null,
    leave: async () => {},
  };
  return T;
}
