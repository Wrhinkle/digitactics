// In-memory stand-in for the Firebase app, auth and Realtime Database modules js/net.js imports, for local lobby
// tests. Serve it with Playwright: await installFakeFirebase(page, { rejectLobbySetId }). One page = one database
// (enough to test a host's lobby, its set and the game init). rejectLobbySetId: refuse lobby writes that carry a
// setId field, like database rules that do not list it, to test the presence fallback.
const APP = `export function initializeApp(cfg, name) { return { cfg, name }; }`;
const AUTH = `export function getAuth(app) { return { app }; }
export async function signInAnonymously() { return { user: { uid: 'u' + Math.random().toString(36).slice(2, 10) } }; }`;

const DB = String.raw`
const root = {};
const listeners = new Set();
const opts = (window.__fakeFirebase = window.__fakeFirebase || { rejectLobbySetId: false, writes: [] });
const parts = (p) => String(p).split('/').filter(Boolean);
function getAt(path) { let n = root; for (const k of parts(path)) { if (n == null || typeof n !== 'object') return null; n = n[k]; } return n === undefined ? null : n; }
function clone(v) { return v == null ? null : JSON.parse(JSON.stringify(v)); }
const TS = { '.sv': 'timestamp' };
function fill(v) { if (v && typeof v === 'object') { if (v['.sv'] === 'timestamp') return Date.now(); const o = Array.isArray(v) ? [] : {}; for (const k in v) o[k] = fill(v[k]); return o; } return v; }
function setAt(path, val) {
  const ks = parts(path);
  if (!ks.length) return;
  let n = root;
  for (let i = 0; i < ks.length - 1; i++) { if (n[ks[i]] == null || typeof n[ks[i]] !== 'object') n[ks[i]] = {}; n = n[ks[i]]; }
  const last = ks[ks.length - 1];
  if (val == null) delete n[last]; else n[last] = fill(clone(val));
  prune(root);
}
function prune(n) { for (const k of Object.keys(n)) if (n[k] && typeof n[k] === 'object') { prune(n[k]); if (!Object.keys(n[k]).length) delete n[k]; } }
function check(path, val) {
  const ks = parts(path);
  if (opts.rejectLobbyFormat && ks[0] === 'tft' && ks[1] === 'lobbies' && ks.length >= 3 && ((ks.length === 3 && val && typeof val === 'object' && 'format' in val) || ks[3] === 'format')) {
    throw new Error('PERMISSION_DENIED: format field not in deployed rules');
  }
  if (opts.rejectLobbySetId && ks[0] === 'tft' && ks[1] === 'lobbies' && ks.length >= 3) {
    if ((ks.length === 3 && val && typeof val === 'object' && 'setId' in val) || ks[3] === 'setId') {
      const e = new Error('PERMISSION_DENIED: Permission denied'); e.code = 'PERMISSION_DENIED'; throw e;
    }
  }
}
function notify() { for (const l of [...listeners]) l.fire(); }
let pushN = 0;
function pushKey() { pushN++; return '-N' + String(Date.now()).padStart(13, '0') + String(pushN).padStart(6, '0'); }

class Ref {
  constructor(path) { this.path = parts(path).join('/'); this.key = parts(path).slice(-1)[0] || null; }
  get ref() { return this; }
  toString() { return this.path; }
}
class Query { constructor(ref, cons) { this.ref = ref; this.path = ref.path; this.cons = cons; } }
class Snap {
  constructor(path, val, key) { this._v = val; this.key = key !== undefined ? key : parts(path).slice(-1)[0] || null; this.ref = new Ref(path); }
  val() { return clone(this._v); }
  exists() { return this._v != null; }
  forEach(fn) {
    const v = this._v;
    if (!v || typeof v !== 'object') return false;
    for (const [k, x] of this._entries || Object.entries(v)) if (fn(new Snap(this.ref.path + '/' + k, x, k)) === true) return true;
    return false;
  }
}
function entriesOf(target) {
  const v = getAt(target.path);
  if (!v || typeof v !== 'object') return [];
  let list = Object.entries(v);
  const cons = target.cons || [];
  const by = cons.find((c) => c.t === 'orderByChild');
  const keyOrder = cons.find((c) => c.t === 'orderByKey');
  const val = (e) => (by ? (e[1] && typeof e[1] === 'object' ? e[1][by.v] : null) : e[0]);
  list.sort((a, b) => { const x = val(a), y = val(b); return x < y ? -1 : x > y ? 1 : a[0] < b[0] ? -1 : 1; });
  for (const c of cons) {
    if (c.t === 'startAfter') list = list.filter((e) => val(e) > c.v);
    if (c.t === 'startAt') list = list.filter((e) => val(e) >= c.v);
    if (c.t === 'endAt') list = list.filter((e) => val(e) <= c.v);
  }
  const lim = cons.find((c) => c.t === 'limitToLast');
  if (lim) list = list.slice(-lim.v);
  void keyOrder;
  return list;
}
function snapOf(target) {
  if (target instanceof Query) {
    const list = entriesOf(target);
    const s = new Snap(target.path, list.length ? Object.fromEntries(list) : null);
    s._entries = list;
    return s;
  }
  return new Snap(target.path, getAt(target.path));
}

export function getDatabase() { return {}; }
export function ref(db, path = '') { return new Ref(path); }
export function query(r, ...cons) { return new Query(r, cons); }
export const orderByChild = (v) => ({ t: 'orderByChild', v });
export const orderByKey = () => ({ t: 'orderByKey' });
export const limitToLast = (v) => ({ t: 'limitToLast', v });
export const startAfter = (v) => ({ t: 'startAfter', v });
export const startAt = (v) => ({ t: 'startAt', v });
export const endAt = (v) => ({ t: 'endAt', v });
export const serverTimestamp = () => TS;
export async function set(r, val) { check(r.path, val); opts.writes.push(['set', r.path, clone(val)]); setAt(r.path, val); notify(); }
export async function update(r, val) {
  for (const k of Object.keys(val || {})) check(r.path + '/' + k, val[k]);
  opts.writes.push(['update', r.path, clone(val)]);
  for (const [k, v] of Object.entries(val || {})) setAt(r.path + '/' + k, v);
  notify();
}
export async function remove(r) { setAt(r.path, null); notify(); }
export async function get(target) { return snapOf(target); }
export function push(r, val) {
  const child = new Ref(r.path + '/' + pushKey());
  if (val === undefined) return child;
  const p = set(child, val).then(() => child);
  child.then = p.then.bind(p);
  child.catch = p.catch.bind(p);
  return child;
}
export async function runTransaction(r, fn) {
  const cur = getAt(r.path);
  const next = fn(clone(cur));
  if (next === undefined) return { committed: false, snapshot: new Snap(r.path, cur) };
  check(r.path, next);
  setAt(r.path, next);
  notify();
  return { committed: true, snapshot: new Snap(r.path, getAt(r.path)) };
}
export function onDisconnect() { return { remove: async () => {}, set: async () => {} }; }
export function onValue(target, cb) {
  if (target.path === '.info/serverTimeOffset') { setTimeout(() => cb(new Snap('.info/serverTimeOffset', 0)), 0); return () => {}; }
  let last;
  const l = { fire() { const s = snapOf(target); const j = JSON.stringify(s._entries || s._v); if (j === last) return; last = j; cb(s); } };
  listeners.add(l);
  setTimeout(() => l.fire(), 0);
  return () => listeners.delete(l);
}
export function onChildAdded(target, cb) {
  const seen = new Set();
  const l = { fire() { for (const [k, v] of entriesOf(target)) if (!seen.has(k)) { seen.add(k); cb(new Snap(target.path + '/' + k, v, k)); } } };
  listeners.add(l);
  setTimeout(() => l.fire(), 0);
  return () => listeners.delete(l);
}
window.__fakeDb = { root, getAt: (p) => clone(getAt(p)) };
`;

async function installFakeFirebase(page, o = {}) {
  await page.addInitScript((opts) => { window.__fakeFirebase = { rejectLobbySetId: !!opts.rejectLobbySetId, rejectLobbyFormat: !!opts.rejectLobbyFormat, writes: [] }; }, o);
  await page.route('https://www.gstatic.com/firebasejs/**', (route) => {
    const url = route.request().url();
    const body = url.endsWith('firebase-app.js') ? APP : url.endsWith('firebase-auth.js') ? AUTH : DB;
    route.fulfill({ status: 200, contentType: 'text/javascript', headers: { 'Access-Control-Allow-Origin': '*' }, body });
  });
}

module.exports = { installFakeFirebase };
