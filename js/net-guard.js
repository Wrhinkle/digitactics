// Untrusted multiplayer data. Limits apply before allocation, queueing or game/UI callbacks.
export const MAX_LOG = 32768;
export const CHUNK = 150;
export const MAX_WIRE = 256 * 1024;
export const obj = (v) => !!v && typeof v === 'object' && !Array.isArray(v);
export const int = (v, max = Number.MAX_SAFE_INTEGER) => Number.isSafeInteger(v) && v >= 0 && v <= max;
export const seat = (v) => int(v, 7);
export const id = (v) => typeof v === 'string' && /^[1-9]\d{0,19}$/.test(v) && BigInt(v) < (1n << 64n);
const text = (v, n = 64) => typeof v === 'string' && v.length <= n && !/[\u0000-\u001f<>]/.test(v);
const token = (v) => typeof v === 'string' && /^[\w@.-]{1,80}$/.test(v);
const keys = (v, allowed) => obj(v) && Object.keys(v).every((k) => allowed.includes(k));
const spot = (v) => obj(v) && (v.type === 'board' ? keys(v, ['type', 'c', 'r']) && int(v.c, 6) && int(v.r, 7) && v.r >= 4 : v.type === 'bench' && keys(v, ['type', 'i']) && int(v.i, 8));

export function action(a, g = {}, internal = false) {
  if (!obj(g)) return false;
  const one = (k, max) => keys(g, [k]) && int(g[k], max);
  switch (a) {
    case 'claim': case 'release': return internal && keys(g, ['name']) && text(g.name, 16);
    case 'ready': case 'done': return !internal && keys(g, []);
    case 'reroll': case 'lock': case 'buyXp': case 'offerReroll': case 'wisp': return keys(g, []);
    case 'buy': return one('slot', 15);
    case 'sell': return one('uid', 10000000);
    case 'offer': return one('i', 15);
    case 'carousel': return keys(g, ['i']) && (g.i == null || int(g.i, 31));
    case 'orb': return one('id', 10000000);
    case 'move': return keys(g, ['from', 'to']) && spot(g.from) && spot(g.to);
    case 'equip': return keys(g, ['idx', 'uid']) && int(g.idx, 511) && int(g.uid, 10000000);
    case 'swapItems': return keys(g, ['a', 'b']) && int(g.a, 511) && int(g.b, 511);
    case 'portal': return one('uid', 10000000) || one('item', 511);
    default: return false;
  }
}
export function entry(e) {
  if (!obj(e) || !int(e.at) || (e.d != null && !/^[a-f0-9]{8}$/.test(e.d))) return false;
  if (e.k === 'a') return seat(e.s) && action(e.a, e.g || {}, true);
  if (e.k === 'vote') return seat(e.s);
  return ['start', 'end', 'next'].includes(e.k);
}
export function look(v) {
  if (!obj(v) || Object.keys(v).length > 9) return false;
  return Object.entries(v).every(([k, x]) => ['emote', 'sound'].includes(k) ? Array.isArray(x) && x.length <= 8 && x.every((s) => s == null || token(s)) :
    ['legendForm', 'legend', 'board', 'killscreen', 'dance', 'elimsound'].includes(k) && (x == null || token(x)));
}
export function presence(v) {
  return keys(v, ['name', 'look', 'lobbySet', 'lobbyPace', 'lobbyFormat', 'at']) &&
    (v.name == null || text(v.name, 32)) && (v.look == null || look(v.look)) &&
    (v.lobbySet == null || token(v.lobbySet)) && (v.lobbyPace == null || ['hyper', 'normal', 'chaos'].includes(v.lobbyPace)) &&
    (v.lobbyFormat == null || ['solo', 'duos'].includes(v.lobbyFormat)) && (v.at == null || int(v.at));
}
const people = (v) => obj(v) && Object.keys(v).length <= 8 && Object.entries(v).every(([k, p]) => id(k) && presence(p));
export function lobby(v) {
  if (!obj(v) || !id(v.host) || !text(v.name, 64) || !text(v.hostName, 32) || !['lobby', 'playing', 'over'].includes(v.status) || !obj(v.slots)) return false;
  const uids = new Set();
  if (!Object.entries(v.slots).every(([k, s]) => /^[0-7]$/.test(k) && obj(s) && id(s.uid) && text(s.name, 16) && !uids.has(s.uid) && !!uids.add(s.uid))) return false;
  return (v.banned == null || Array.isArray(v.banned) && v.banned.length <= 128 && v.banned.every(id) && !v.banned.includes(v.host)) &&
    (v.planTime == null || int(v.planTime, 300) && v.planTime >= 5) && (v.setId == null || token(v.setId)) &&
    (v.round == null || text(v.round, 20)) && (v.alive == null || Array.isArray(v.alive) && v.alive.length === 8 && v.alive.every((a) => typeof a === 'boolean'));
}
export function init(v) {
  if (!obj(v) || !int(v.seed, 0xffffffff) || !int(v.at) || !int(v.planTime, 300) || v.planTime < 5 || !token(v.setId)) return false;
  if (!Array.isArray(v.slots) || v.slots.length !== 8 || !v.slots.every((s) => obj(s) && text(s.name, 16) && typeof s.human === 'boolean')) return false;
  if (!['hyper', 'normal'].includes(v.pace) || !['solo', 'duos'].includes(v.format || 'solo')) return false;
  if (v.teams != null && (!Array.isArray(v.teams) || v.teams.length !== 4 || !v.teams.every((t) => Array.isArray(t) && t.length === 2 && t.every(seat)) || new Set(v.teams.flat()).size !== 8)) return false;
  return true;
}
export function wire(m) {
  if (!obj(m) || !id(m.l)) return false;
  const entries = () => int(m.start, MAX_LOG) && Array.isArray(m.entries) && m.entries.length <= CHUNK && m.start + m.entries.length <= MAX_LOG && m.entries.every(entry);
  switch (m.t) {
    case 'snapreq': return int(m.rid);
    case 'seat': return int(m.rid) && seat(m.i) && text(m.name, 16);
    case 'seatres': return int(m.rid) && typeof m.ok === 'boolean';
    case 'unseat': return seat(m.i);
    case 'pres': case 'presup': return presence(m.rec);
    case 'presdel': return true;
    case 'intent': return seat(m.s) && int(m.q) && int(m.round, 10000) && action(m.a, m.g);
    case 'logreq': return int(m.from, MAX_LOG);
    case 'ping': return int(m.t0);
    case 'pong': return int(m.t0) && int(m.now);
    case 'snap': return int(m.rid) && int(m.total, MAX_LOG) && entries() && m.start + m.entries.length <= m.total && lobby(m.lobby) && people(m.presence) && (m.init == null || init(m.init)) && int(m.now);
    case 'lobby': return m.lobby == null || lobby(m.lobby);
    case 'presence': return people(m.presence);
    case 'init': return init(m.init);
    case 'log': return int(m.n, MAX_LOG - 1) && entry(m.e);
    case 'logs': return entries();
    case 'head': return int(m.n, MAX_LOG);
    case 'fx': return obj(m.v) && seat(m.v.s) && ['emote', 'sound'].includes(m.v.kind) && token(m.v.id);
    default: return false;
  }
}
export function parse(s) {
  if (typeof s !== 'string' || s.length > MAX_WIRE) return null;
  try {
    const m = JSON.parse(s);
    // Iterative walk bounds nesting/work and rejects prototype keys, including inside ignored fields.
    const todo = [[m, 0]];
    let count = 0;
    while (todo.length) {
      const [v, depth] = todo.pop();
      if (++count > 12000 || depth > 12) return null;
      if (typeof v === 'number' && !Number.isFinite(v)) return null;
      if (typeof v === 'string' && v.length > 256) return null;
      if (v && typeof v === 'object') for (const k of Object.keys(v)) {
        if (['__proto__', 'prototype', 'constructor'].includes(k)) return null;
        todo.push([v[k], depth + 1]);
      }
    }
    return wire(m) ? m : null;
  } catch { return null; }
}
// Token buckets are scoped to room members and message classes; one noisy peer cannot consume another's allowance.
export function allow(map, key, rate, burst, cost = 1) {
  const now = performance.now();
  const b = map.get(key) || { at: now, n: burst };
  b.n = Math.min(burst, b.n + (now - b.at) * rate / 1000);
  b.at = now;
  map.set(key, b);
  if (b.n < cost) return false;
  b.n -= cost;
  return true;
}
