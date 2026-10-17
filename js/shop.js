// Store, Bits earnings, Digi-Eggs and the Locker.
// Client-only cosmetic economy: reads game state, never changes it, never writes to the lockstep log.
// DT.Shop.open(tab?)          tab: 'legend' | 'emote' | 'board' | 'killscreen' | 'dance' | 'sound' | 'elimsound' | 'eggs'
// DT.Shop.openLocker(kind?)   equip owned items; emote and sound wheels have DT.Cosmetics.SLOTS slots
// DT.Shop.award(game?, session?)       pays Bits for a finished game once (keyed by seed + seat), returns the breakdown
// DT.Shop.showAward(panel, game?, session?)  award() plus the counting breakdown on a game-over / eliminated panel
import './shopstyle.js';

const DT = window.DT;
const C = DT.Cosmetics;
const W = DT.Wallet;

const KEY = 'dt.shop.v1';
const KINDS = [['legend', 'Little Legends', 'Little Legend'], ['emote', 'Emotes', 'Emote'], ['board', 'Boards', 'Board'],
  ['killscreen', 'Kill Screens', 'Kill Screen'], ['dance', 'Dances', 'Dance'], ['sound', 'Soundboard', 'Sound'], ['elimsound', 'Elim Sounds', 'Elim Sound']];
const KIND_NAME = Object.fromEntries(KINDS.map(([k, , one]) => [k, one]));
const TAB_NAME = Object.fromEntries(KINDS.map(([k, many]) => [k, many]));
const RARITIES = ['common', 'uncommon', 'rare', 'epic', 'legendary'];
const ODDS = { common: 45, uncommon: 25, rare: 18, epic: 9, legendary: 3 };
// eggOnly items (rare and up Little Legends) can't be bought directly: they hatch from Digi-Eggs
const EGG_ONLY = (it) => !!(it && it.eggOnly && !it.free);
const PLACE_BITS = [0, 120, 90, 75, 60, 45, 35, 25, 20];
const PVP_WIN_BITS = 6, KILL_BITS = 25, FIRST_GAME_BITS = 60;
const EGG_KIND_PRICE = 250, EGG_MIXED_PRICE = 350, REFUND = 0.4, FEATURED_COUNT = 4, FEATURED_OFF = 0.25;
const MULTI = (k) => k === 'emote' || k === 'sound';

// Daily missions: two per day, picked by date. Progress comes from one game's stats.
const MISSIONS = [
  { id: 'win3', name: 'Win 3 PvP rounds in one game', bits: 50, goal: 3, val: (s) => s.pvpWins },
  { id: 'win6', name: 'Win 6 PvP rounds in one game', bits: 80, goal: 6, val: (s) => s.pvpWins },
  { id: 'lvl7', name: 'Reach level 7', bits: 40, goal: 7, val: (s) => s.maxLevel },
  { id: 'star3', name: 'Digivolve a Digimon to 3 stars', bits: 70, goal: 3, val: (s) => s.maxStars },
  { id: 'top4', name: 'Finish in the top half', bits: 50, goal: 1, val: (s) => (s.place && s.place <= 4 ? 1 : 0) },
  { id: 'top2', name: 'Finish top 2', bits: 70, goal: 1, val: (s) => (s.place && s.place <= 2 ? 1 : 0) },
  { id: 'kill1', name: 'Eliminate a Tamer', bits: 60, goal: 1, val: (s) => s.kills },
];

// ---------- save ----------
let save = { awarded: [], awards: {}, lastDay: '', missions: { day: '', done: [] } };
try { const s = JSON.parse(localStorage.getItem(KEY)); if (s && typeof s === 'object') save = Object.assign(save, s); } catch (e) { /* blocked or corrupt */ }
const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(save)); } catch (e) { /* private mode */ } };

const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const hash = (s) => { let h = 2166136261; for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 16777619) >>> 0; } return h; };
const today = () => { const d = new Date(); return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`; };
const bit = '<i class="bit"></i>';
const sfx = (n) => { try { if (DT.Sfx) DT.Sfx.play(n); } catch (e) { /* audio blocked */ } };
const UI = () => DT.UI || {};

// Dev helper: ?bits=5000 adds Bits once per browser session, on localhost only.
try {
  const n = Number(new URLSearchParams(location.search).get('bits'));
  if (n > 0 && /^(localhost|127\.0\.0\.1|\[::1\])$/.test(location.hostname) && !sessionStorage.getItem('dt.devbits')) {
    sessionStorage.setItem('dt.devbits', '1');
    W.add(Math.min(n, 1e6), 'Dev bits');
  }
} catch (e) { /* storage blocked */ }

function todaysMissions() {
  const d = today();
  if (save.missions.day !== d) save.missions = { day: d, done: [] };
  const order = MISSIONS.slice().sort((a, b) => hash(`${d}:${a.id}`) - hash(`${d}:${b.id}`));
  return order.slice(0, 2);
}

// ---------- tracking one game (read-only) ----------
let tr = null;
function track(g) {
  g = g || (UI().game && UI().game());
  if (!g || !g.human) return null;
  if (!tr || tr.game !== g) tr = { game: g, seen: new WeakSet(), pvpWins: 0, kills: 0, maxStars: 1, maxLevel: 0 };
  const me = g.human;
  if (me.alive) {
    tr.maxLevel = Math.max(tr.maxLevel, me.level);
    for (const u of me.allUnits()) tr.maxStars = Math.max(tr.maxStars, u.stage + 1);
  }
  const res = g.results;
  if (res && res.length && !tr.seen.has(res)) {
    tr.seen.add(res);
    for (const r of res) {
      const m = r.match;
      if (m.pve) continue;
      // the opponent who won the fight dealt the final damage; a ghost fight still counts for the real Tamer behind it
      if ((m.a === me || m.a2 === me) && r.aWon) { tr.pvpWins++; if (m.b && !m.ghost && !m.b.alive && r.dmgB > 0) tr.kills++; }
      if ((m.b === me || m.b2 === me) && !m.ghost && r.bWon) { tr.pvpWins++; if (!m.a.alive && r.dmgA > 0) tr.kills++; }
    }
  }
  return tr;
}
const statsOf = (t, place) => ({ pvpWins: t ? t.pvpWins : 0, kills: t ? t.kills : 0, maxStars: t ? t.maxStars : 1, maxLevel: t ? t.maxLevel : 0, place: place || 0 });
setInterval(() => { try { track(); topbarState(); } catch (e) { /* game mid-swap */ } }, 200);

// ---------- awarding ----------
function gameKey(g, session) { return `${session ? 'mp' : 'solo'}:${g.seed}:${g.human.id}`; }

function award(g, session) {
  g = g || (UI().game && UI().game());
  if (session === undefined) session = UI().session ? UI().session() : null;
  if (!g || !g.human || !g.human.placement) return null;
  const key = gameKey(g, session);
  if (save.awards[key]) return { ...save.awards[key], fresh: false };
  if (save.awarded.includes(key)) return null;
  const place = g.human.placement;
  const t = track(g);
  const rewardPlace = g.duo ? place * 2 - 1 : place;
  const s = statsOf(t, rewardPlace);
  const lines = [{ label: `Placement #${place}`, n: PLACE_BITS[rewardPlace] || 20 }];
  if (s.pvpWins) lines.push({ label: `${s.pvpWins} PvP round${s.pvpWins > 1 ? 's' : ''} won`, n: s.pvpWins * PVP_WIN_BITS });
  if (s.kills) lines.push({ label: `${s.kills} elimination${s.kills > 1 ? 's' : ''}`, n: s.kills * KILL_BITS });
  const d = today();
  if (save.lastDay !== d) { lines.push({ label: 'First game of the day', n: FIRST_GAME_BITS }); save.lastDay = d; }
  for (const m of todaysMissions()) {
    if (save.missions.done.includes(m.id) || m.val(s) < m.goal) continue;
    save.missions.done.push(m.id);
    lines.push({ label: `Mission: ${m.name}`, n: m.bits });
  }
  const total = lines.reduce((n, l) => n + l.n, 0);
  const rec = { lines, total, place };
  save.awarded = [key, ...save.awarded].slice(0, 200);
  save.awards[key] = rec;
  const keep = new Set(save.awarded.slice(0, 10));
  for (const k of Object.keys(save.awards)) if (!keep.has(k)) delete save.awards[k];
  persist();
  W.add(total, `Game: placed #${place}`);
  // care and Digi-Data for the equipped Little Legend (js/legendevo.js)
  if (DT.LegendEvo) { try { DT.LegendEvo.recordGame(key, rewardPlace); } catch (e) { console.warn(e); } }
  return { ...rec, fresh: true };
}

// Breakdown box on the game-over / eliminated panel. Counts up when the Bits are new.
function showAward(panel, g, session) {
  if (!panel) return null;
  const a = award(g, session);
  if (!a) return null;
  const box = document.createElement('div');
  box.className = 'dts-award';
  box.innerHTML = `<div class="dts-award-h">Bits earned${a.fresh ? '' : ' <span class="muted">(already added)</span>'}</div>
    ${a.lines.map((l) => `<div class="dts-aline"><span>${esc(l.label)}</span><b data-n="${l.n}">${a.fresh ? 0 : l.n}</b></div>`).join('')}
    <div class="dts-aline tot"><span>Total</span><b data-n="${a.total}">${bit} <span class="dts-num">${a.fresh ? 0 : a.total}</span></b></div>
    <div class="dts-award-f"><span class="muted">Wallet: ${bit} <b class="dts-wbits">${W.bits}</b></span> <button data-shop-open>Store</button></div>`;
  const btn = panel.querySelector('button.primary, button');
  if (btn && btn.parentNode === panel) panel.insertBefore(box, btn); else panel.appendChild(box);
  if (!a.fresh) return a;
  const rows = [...box.querySelectorAll('.dts-aline')];
  rows.forEach((r) => r.classList.add('pending'));
  const count = (el, n, ms) => new Promise((done) => {
    const t0 = performance.now();
    const step = (now) => {
      const k = Math.min(1, (now - t0) / ms);
      el.textContent = Math.round(n * (1 - Math.pow(1 - k, 3)));
      if (k < 1 && el.isConnected) requestAnimationFrame(step); else { el.textContent = n; done(); }
    };
    requestAnimationFrame(step);
  });
  (async () => {
    await new Promise((r) => setTimeout(r, 350));
    for (const r of rows) {
      if (!r.isConnected) return;
      r.classList.remove('pending');
      const b = r.querySelector('b');
      const el = r.classList.contains('tot') ? b.querySelector('.dts-num') : b;
      await count(el, Number(b.dataset.n), r.classList.contains('tot') ? 700 : 380);
      sfx(r.classList.contains('tot') ? 'pick' : 'tick');
    }
  })();
  return a;
}

// ---------- prices, featured, eggs ----------
const sellable = (k) => C.list(k).filter((it) => !it.secret && !it.free && it.price > 0 && !EGG_ONLY(it));
function featured() {
  const d = today();
  const all = [];
  for (const [k] of KINDS) for (const it of sellable(k)) all.push({ kind: k, item: it, h: hash(`${d}:${k}:${it.id}`) });
  return all.sort((a, b) => a.h - b.h).slice(0, FEATURED_COUNT);
}
function priceOf(kind, item) {
  if (!item || item.free) return 0;
  const f = featured().some((x) => x.kind === kind && x.item.id === item.id);
  return f ? Math.max(5, Math.round((item.price * (1 - FEATURED_OFF)) / 5) * 5) : item.price;
}

const EGGS = [{ id: 'mixed', name: 'Mixed Digi-Egg', kinds: KINDS.map(([k]) => k), price: EGG_MIXED_PRICE, desc: 'Any cosmetic from any category.' }]
  .concat(KINDS.map(([k, many]) => ({ id: k, name: `${KIND_NAME[k]} Egg`, kinds: [k], price: EGG_KIND_PRICE, desc: `One of the ${many.toLowerCase()}.` })));
function eggPool(egg) {
  const pool = [];
  for (const k of egg.kinds) for (const it of C.list(k)) if (!it.secret && it.egg !== false && !it.free) pool.push({ kind: k, item: it });
  return pool;
}
// odds per rarity among the rarities this egg can actually roll
function eggOdds(pool) {
  const present = RARITIES.filter((r) => pool.some((x) => (x.item.rarity || 'common') === r));
  const sum = present.reduce((n, r) => n + ODDS[r], 0);
  return present.map((r) => ({ rarity: r, pct: (100 * ODDS[r]) / sum, count: pool.filter((x) => (x.item.rarity || 'common') === r).length }));
}
function rollEgg(egg) {
  const pool = eggPool(egg);
  if (!pool.length) return null;
  const odds = eggOdds(pool);
  let x = Math.random() * 100, rar = odds[odds.length - 1].rarity;
  for (const o of odds) { if (x < o.pct) { rar = o.rarity; break; } x -= o.pct; }
  const of = pool.filter((p) => (p.item.rarity || 'common') === rar);
  return of[Math.floor(Math.random() * of.length)];
}

// ---------- DOM ----------
let root = null;
const view = { mode: 'store', tab: 'legend', slot: 0, open: false };
let disposers = [];
let renderQueued = false;

function el(html) { const d = document.createElement('div'); d.innerHTML = html.trim(); return d.firstChild; }

function build() {
  if (root) return root;
  root = el(`<div id="dtStore" class="hidden" role="dialog" aria-label="Store">
    <div class="dts-win">
      <header class="dts-head">
        <div class="dts-modes"><button data-mode="store">Store</button><button data-mode="locker">Locker</button></div>
        <div class="dts-missions"></div>
        <div class="dts-wallet" title="Bits: earned by playing games"><span>${bit} <b class="dts-wbits">0</b></span><button class="dts-histbtn">History</button>
          <div class="dts-hist hidden"></div></div>
        <button class="dts-close" title="Esc">Close</button>
      </header>
      <nav class="dts-tabs"></nav>
      <div class="dts-body"></div>
      <div class="dts-msg"></div>
    </div>
    <div class="dts-modal hidden"></div>
  </div>`);
  document.body.appendChild(root);
  root.addEventListener('click', onClick);
  return root;
}

function msg(text, bad) {
  const m = root.querySelector('.dts-msg');
  m.textContent = text;
  m.classList.toggle('bad', !!bad);
  m.classList.add('on');
  clearTimeout(msg.t);
  msg.t = setTimeout(() => m.classList.remove('on'), 2200);
}

function iconHTML(item) {
  const ic = item && item.icon;
  if (ic && /[/.]/.test(ic) && /^(https?:|data:|\.|\/|[\w-]+\/)/.test(ic)) return `<img src="${esc(ic)}" alt="">`;
  if (ic) return `<span class="dts-emo">${esc(ic)}</span>`;
  return `<span class="dts-art">${esc((item && item.name ? item.name : '?').split(/\s+/).map((w) => w[0]).join('').slice(0, 3))}</span>`;
}

// item.preview(el) if the item has one, else its icon. A preview may return a cleanup function.
function fillPreview(box, item) {
  box.innerHTML = '';
  if (item && typeof item.preview === 'function') {
    try {
      const off = item.preview(box);
      if (typeof off === 'function') disposers.push(off);
      if (box.childNodes.length || box.style.backgroundImage) return;
    } catch (e) { console.warn('preview failed', item.id, e); }
  }
  box.innerHTML = iconHTML(item);
}
function fillPreviews(scope) {
  for (const box of scope.querySelectorAll('[data-prev]')) {
    const [kind, id] = box.dataset.prev.split('|');
    fillPreview(box, C.get(kind, id));
  }
}
function dispose() { for (const f of disposers) { try { f(); } catch (e) { /* ignore */ } } disposers = []; }

function isEquipped(kind, id) {
  const e = C.equipped(kind);
  return Array.isArray(e) ? e.includes(id) : e === id;
}

function cardHTML(kind, it, opts = {}) {
  const owned = C.owns(kind, it.id);
  const eq = owned && isEquipped(kind, it.id);
  const price = priceOf(kind, it);
  const rar = RARITIES.includes(it.rarity) ? it.rarity : 'common';
  let act;
  if (opts.locker) act = MULTI(kind) ? `<button class="primary" data-slotset="${esc(it.id)}">Put in slot ${view.slot + 1}</button>`
    : eq ? '<button disabled>Equipped</button>' : `<button class="primary" data-equip="${esc(it.id)}">Equip</button>`;
  else if (eq) act = '<button disabled class="dts-eq">Equipped</button>';
  else if (owned) act = `<button data-equip="${esc(it.id)}">${it.free ? 'Free' : 'Owned'} · Equip</button>`;
  else if (it.linked) act = `<button data-gostore="${esc(it.linked[0])}" title="Own it on the ${esc(TAB_NAME[it.linked[0]] || it.linked[0])} to use it here">${esc(TAB_NAME[it.linked[0]] || it.linked[0])} clip · See it</button>`;
  else if (EGG_ONLY(it)) act = `<button class="dts-eggonly" data-gostore="eggs" title="Hatches from the ${KIND_NAME[kind]} Egg or the Mixed Digi-Egg">Egg only · See eggs</button>`;
  else {
    const poor = price > W.bits;
    act = `<button class="dts-buy${poor ? ' poor' : ''}" data-buy="${esc(it.id)}">${price < it.price ? `<s>${it.price}</s> ` : ''}${bit} ${price}</button>`;
  }
  return `<div class="dts-card r-${rar}${eq ? ' eq' : ''}" data-kind="${kind}" data-id="${esc(it.id)}">
    <div class="dts-prev" data-prev="${kind}|${esc(it.id)}"></div>
    <div class="dts-name">${esc(it.name)}</div>
    <div class="dts-rar">${rar}${it.tag ? ` · <b class="dts-tag">${esc(it.tag)}</b>` : ''}${opts.kindLabel ? ` · ${KIND_NAME[kind]}` : ''}</div>
    ${EGG_ONLY(it) && !owned ? '<span class="dts-eobadge">Egg only</span>' : ''}
    ${it.desc ? `<div class="dts-desc">${esc(it.desc)}</div>` : ''}
    ${kind === 'legend' && owned && DT.LegendEvo && DT.LegendEvo.cardHTML ? DT.LegendEvo.cardHTML(it.id) : ''}
    <div class="dts-act" data-kind="${kind}">${act}</div>
  </div>`;
}

function missionsHTML() {
  const t = tr && UI().game && UI().game() === tr.game ? tr : null;
  const s = statsOf(t, t && t.game.human.placement);
  return `<span class="muted">Daily missions:</span> ${todaysMissions().map((m) => {
    const done = save.missions.done.includes(m.id);
    const cur = Math.min(m.goal, m.val(s));
    const prog = done ? 'done' : t && m.goal > 1 ? `${cur}/${m.goal}` : '';
    return `<span class="dts-mis${done ? ' done' : ''}" title="Complete it in one game today">${esc(m.name)} <b>${bit}${m.bits}</b>${prog ? ` <i>${prog}</i>` : ''}</span>`;
  }).join('')}`;
}

function histHTML() {
  const h = W.history();
  return h.length ? `<div class="dts-hh">Recent Bits</div>${h.slice(0, 12).map((x) => `<div class="dts-hrow"><span>${esc(x.reason || 'Bits')}</span><b>+${x.n}</b><i>${new Date(x.at).toLocaleDateString()}</i></div>`).join('')}`
    : '<div class="muted">No Bits earned yet. Finish a game to earn some.</div>';
}

function render() {
  if (!root || !view.open) return;
  dispose();
  root.querySelectorAll('[data-mode]').forEach((b) => b.classList.toggle('on', b.dataset.mode === view.mode));
  root.querySelector('.dts-wbits').textContent = W.bits;
  root.querySelector('.dts-missions').innerHTML = missionsHTML();
  root.setAttribute('aria-label', view.mode === 'store' ? 'Store' : 'Locker');
  const tabs = view.mode === 'store' ? KINDS.map(([k, many]) => [k, many]).concat([['eggs', 'Digi-Eggs']]) : KINDS.map(([k, many]) => [k, many]);
  if (!tabs.some(([k]) => k === view.tab)) view.tab = 'legend';
  root.querySelector('.dts-tabs').innerHTML = tabs.map(([k, name]) => {
    const n = k === 'eggs' ? '' : view.mode === 'locker' ? C.list(k).filter((it) => C.owns(k, it.id)).length : C.list(k).filter((it) => !it.secret && !it.linked).length;
    return `<button data-tab="${k}" class="${k === view.tab ? 'on' : ''}">${name}${n !== '' ? ` <span>${n}</span>` : ''}</button>`;
  }).join('');
  const body = root.querySelector('.dts-body');
  const top = body.scrollTop;
  body.innerHTML = view.mode === 'locker' ? lockerHTML(view.tab) : view.tab === 'eggs' ? eggsHTML() : storeHTML(view.tab);
  body.scrollTop = top;
  fillPreviews(body);
}
function queueRender() {
  if (renderQueued || !view.open) return;
  renderQueued = true;
  requestAnimationFrame(() => { renderQueued = false; render(); });
}

function storeHTML(kind) {
  const f = featured();
  // linked items (soundboard clips as elimination sounds) are sold on their own tab, not here
  const items = C.list(kind).filter((it) => !it.secret && !it.linked);
  const linked = C.list(kind).filter(it => !it.secret).length - items.length;
  const feat = f.length ? `<section class="dts-feat"><h3>Featured today <span class="muted">${Math.round(FEATURED_OFF * 100)}% off, changes at midnight</span></h3>
    <div class="dts-grid">${f.map((x) => cardHTML(x.kind, x.item, { kindLabel: true })).join('')}</div></section>` : '';
  const sorted = items.slice().sort((a, b) => (b.free ? 1 : 0) - (a.free ? 1 : 0) || RARITIES.indexOf(a.rarity) - RARITIES.indexOf(b.rarity) || (a.price || 0) - (b.price || 0));
  const grid = sorted.length ? `<div class="dts-grid">${sorted.map((it) => cardHTML(kind, it)).join('')}</div>`
    : `<div class="dts-empty">No ${TAB_NAME[kind].toLowerCase()} in the store yet.</div>`;
  const note = linked ? `<p class="dts-note">Every soundboard clip you own also works as an elimination sound. Equip one in the Locker. <button data-gostore="sound">Soundboard clips</button></p>` : '';
  return `${feat}<section><h3>${TAB_NAME[kind]} <span class="muted">${items.filter((it) => C.owns(kind, it.id)).length}/${items.length} owned</span></h3>${note}${grid}</section>`;
}

function eggsHTML() {
  return `<p class="dts-note">A Digi-Egg hatches one random cosmetic. Odds are per rarity, then every item of that rarity is equally likely.
    A duplicate Little Legend becomes Digi-Data that levels it up toward digivolving (Bits once it is max level). Other duplicates refund ${Math.round(REFUND * 100)}% of their price as Bits.
    Rare, epic and legendary Little Legends only come from eggs.</p>
    <div class="dts-eggs">${EGGS.map((egg) => {
      const pool = eggPool(egg);
      const odds = eggOdds(pool);
      const owned = pool.filter((x) => C.owns(x.kind, x.item.id)).length;
      const poor = egg.price > W.bits;
      return `<div class="dts-egg-card">
        <div class="dts-egg-art"><div class="dts-egg e-${egg.id}"></div></div>
        <div class="dts-name">${esc(egg.name)}</div>
        <div class="dts-desc">${esc(egg.desc)} ${pool.length ? `${owned}/${pool.length} owned.` : ''}</div>
        ${pool.length ? `<table class="dts-odds">${odds.map((o) => `<tr class="r-${o.rarity}"><td>${o.rarity}</td><td>${o.pct.toFixed(o.pct % 1 ? 1 : 0)}%</td><td class="muted">${o.count} item${o.count > 1 ? 's' : ''}</td></tr>`).join('')}</table>`
          : '<div class="dts-empty small">Nothing to hatch yet.</div>'}
        ${eggOnlyHTML(pool)}
        <button class="dts-buy${poor ? ' poor' : ''}" data-egg="${egg.id}" ${pool.length ? '' : 'disabled'}>Hatch · ${bit} ${egg.price}</button>
      </div>`;
    }).join('')}</div>`;
}

// the egg-only items an egg can hatch, with each one's own chance
function eggOnlyHTML(pool) {
  const only = pool.filter((x) => EGG_ONLY(x.item));
  if (!only.length) return '';
  const odds = eggOdds(pool);
  const each = (r) => { const o = odds.find((x) => x.rarity === r); return o ? o.pct / o.count : 0; };
  return `<div class="dts-eolist"><div class="dts-eoh">Egg only</div>${only.map((x) => {
    const r = RARITIES.includes(x.item.rarity) ? x.item.rarity : 'common';
    const p = each(r);
    return `<span class="r-${r}${C.owns(x.kind, x.item.id) ? ' own' : ''}" title="${esc(r)}${C.owns(x.kind, x.item.id) ? ', owned' : ''}">${esc(x.item.name)} <i>${p.toFixed(p < 1 ? 2 : 1)}%</i></span>`;
  }).join('')}</div>`;
}

function lockerHTML(kind) {
  const owned = C.list(kind).filter((it) => C.owns(kind, it.id));
  const more = C.visible(kind).length - owned.length;
  const shopLink = more > 0 ? `<button data-gostore="${kind}">${more} more in the Store</button>` : '';
  if (!owned.length) return `<div class="dts-empty">You don't own any ${TAB_NAME[kind].toLowerCase()} yet. ${shopLink}</div>`;
  let wheel = '';
  if (MULTI(kind)) {
    const eq = C.equipped(kind);
    const n = C.SLOTS;
    wheel = `<section class="dts-wheelbox"><div class="dts-wheel">${eq.map((id, i) => {
      const a = (i / n) * Math.PI * 2 - Math.PI / 2;
      const it = id ? C.get(kind, id) : null;
      return `<button class="dts-slot${i === view.slot ? ' on' : ''}" data-slot="${i}" style="left:calc(50% + ${Math.cos(a) * 128}px);top:calc(50% + ${Math.sin(a) * 104}px)" title="Slot ${i + 1}">
        <span class="dts-slotn">${i + 1}</span>${it ? `<span class="dts-prev sm" data-prev="${kind}|${esc(it.id)}"></span><span class="dts-slotname">${esc(it.name)}</span>` : '<span class="muted">Empty</span>'}</button>`;
    }).join('')}<div class="dts-hub">${KIND_NAME[kind]} wheel</div></div>
    <div class="dts-wheelhelp"><div><b>Slot ${view.slot + 1}</b> selected. Pick a slot on the wheel, then choose what goes in it.</div>
      ${eq[view.slot] ? `<div><button data-slotclear="1">Empty slot ${view.slot + 1}</button></div>` : ''}${shopLink ? `<div>${shopLink}</div>` : ''}</div></section>`;
  }
  // Little Legends: the equipped legend's digivolution summary (js/legendevo.js opens the full panel)
  if (kind === 'legend' && DT.LegendEvo && DT.LegendEvo.bannerHTML) wheel = DT.LegendEvo.bannerHTML(C.equipped('legend'));
  return `${wheel}<section><h3>Owned ${TAB_NAME[kind].toLowerCase()} <span class="muted">${owned.length}</span>${!MULTI(kind) && shopLink ? ` ${shopLink}` : ''}</h3>
    <div class="dts-grid">${owned.map((it) => cardHTML(kind, it, { locker: true })).join('')}</div></section>`;
}

// ---------- modal: confirm, bought, hatch ----------
function modal(html) {
  const m = root.querySelector('.dts-modal');
  m.innerHTML = `<div class="dts-mbox">${html}</div>`;
  m.classList.remove('hidden');
  return m.firstChild;
}
function closeModal() {
  const m = root.querySelector('.dts-modal');
  clearTimeout(hatchTimer);
  m.classList.add('hidden');
  m.innerHTML = '';
  queueRender();
}

let pending = null;
function confirmBuy(kind, id) {
  const it = C.get(kind, id);
  if (!it || it.secret || C.owns(kind, id)) return;
  if (EGG_ONLY(it)) { sfx('error'); return msg(`${it.name} only hatches from Digi-Eggs.`, true); }
  const price = priceOf(kind, it);
  if (price > W.bits) { sfx('error'); return msg(`Not enough Bits: ${it.name} costs ${price}, you have ${W.bits}.`, true); }
  pending = { type: 'item', kind, id, price };
  const box = modal(`<h2>Buy ${esc(it.name)}?</h2>
    <div class="dts-mprev r-${it.rarity || 'common'}"><div class="dts-prev lg" data-prev="${kind}|${esc(id)}"></div></div>
    <p class="muted">${KIND_NAME[kind]} · ${esc(it.rarity || 'common')}${price < it.price ? ` · featured, was ${it.price}` : ''}</p>
    <p>${bit} <b>${price}</b> of your ${W.bits} Bits</p>
    <div class="dts-mbtns"><button class="primary" data-confirm="1">Buy</button><button data-cancel="1">Cancel</button></div>`);
  fillPreviews(box);
}
function confirmEgg(eggId) {
  const egg = EGGS.find((e) => e.id === eggId);
  if (!egg || !eggPool(egg).length) return;
  if (egg.price > W.bits) { sfx('error'); return msg(`Not enough Bits: the ${egg.name} costs ${egg.price}, you have ${W.bits}.`, true); }
  pending = { type: 'egg', id: eggId, price: egg.price };
  modal(`<h2>Hatch a ${esc(egg.name)}?</h2>
    <div class="dts-egg-art lg"><div class="dts-egg e-${egg.id}"></div></div>
    <p>${bit} <b>${egg.price}</b> of your ${W.bits} Bits</p>
    <div class="dts-mbtns"><button class="primary" data-confirm="1">Hatch</button><button data-cancel="1">Cancel</button></div>`);
}

function doBuy() {
  const p = pending;
  pending = null;
  if (!p) return closeModal();
  if (p.type === 'egg') return hatch(p.id);
  const it = C.get(p.kind, p.id);
  if (!it || it.secret || C.owns(p.kind, p.id) || EGG_ONLY(it)) return closeModal();
  if (!W.spend(p.price)) { closeModal(); sfx('error'); return msg('Not enough Bits.', true); }
  C.grant(p.kind, p.id);
  sfx('buy');
  const box = modal(`<h2>Bought ${esc(it.name)}</h2>
    <div class="dts-mprev r-${it.rarity || 'common'}"><div class="dts-prev lg" data-prev="${p.kind}|${esc(p.id)}"></div></div>
    <div class="dts-mbtns">${MULTI(p.kind) ? `<button class="primary" data-locker="${p.kind}">Put on my wheel</button>` : `<button class="primary" data-equipnow="${p.kind}|${esc(p.id)}">Equip now</button>`}<button data-cancel="1">Keep shopping</button></div>`);
  fillPreviews(box);
}

let hatchTimer = null;
function hatch(eggId) {
  const egg = EGGS.find((e) => e.id === eggId);
  const roll = egg && rollEgg(egg);
  if (!roll) return closeModal();
  if (!W.spend(egg.price)) { closeModal(); sfx('error'); return msg('Not enough Bits.', true); }
  // grant / refund at once, so closing during the animation loses nothing
  const { kind, item } = roll;
  const dup = C.owns(kind, item.id);
  // a duplicate Little Legend becomes Digi-Data for it (js/legendevo.js), unless it is already max level
  const evo = dup && kind === 'legend' && DT.LegendEvo ? DT.LegendEvo.addDuplicate(item.id) : null;
  const refund = dup && !evo ? Math.round((item.price || 0) * REFUND) : 0;
  if (dup) { if (refund) W.add(refund, `Duplicate refund: ${item.name}`); } else C.grant(kind, item.id);
  const dupText = evo ? `Duplicate: <b>+${evo.data}</b> Digi-Data for ${esc(item.name)}${evo.levels ? `. Now level ${evo.level}!` : ` (level ${evo.level}).`}`
    : `Duplicate: ${bit} <b>${refund}</b> Bits refunded.`;
  const rar = RARITIES.includes(item.rarity) ? item.rarity : 'common';
  const box = modal(`<div class="dts-hatch r-${rar}">
      <div class="dts-egg-art lg"><div class="dts-egg e-${egg.id} wobble"><i class="c1"></i><i class="c2"></i><i class="c3"></i></div></div>
      <div class="dts-reveal hidden">
        <div class="dts-mprev r-${rar}"><div class="dts-prev lg" data-prev="${kind}|${esc(item.id)}"></div></div>
        <h2>${esc(item.name)}</h2>
        <p class="dts-rar r-${rar}">${rar} ${KIND_NAME[kind]}</p>
        <p>${dup ? dupText : 'New! Added to your Locker.'}</p>
        <div class="dts-mbtns">${evo ? `<button class="primary" data-evo-open="${esc(item.id)}">Digivolve</button>` : dup ? '' : MULTI(kind) ? `<button class="primary" data-locker="${kind}">Put on my wheel</button>` : `<button class="primary" data-equipnow="${kind}|${esc(item.id)}">Equip now</button>`}
          <button data-egg="${egg.id}">Hatch another</button><button data-cancel="1">Close</button></div>
      </div>
      <p class="muted dts-skip">Click to skip</p>
    </div>`);
  const eggEl = box.querySelector('.dts-egg');
  const reveal = () => {
    clearTimeout(hatchTimer);
    if (!eggEl.isConnected || box.querySelector('.dts-hatch').classList.contains('done')) return;
    box.querySelector('.dts-hatch').classList.add('done');
    box.querySelector('.dts-egg-art').classList.add('gone');
    box.querySelector('.dts-skip').remove();
    box.querySelector('.dts-reveal').classList.remove('hidden');
    fillPreviews(box);
    sfx(rar === 'legendary' || rar === 'epic' ? 'champion' : 'digivolve');
  };
  sfx('reroll');
  hatchTimer = setTimeout(() => {
    eggEl.classList.remove('wobble'); eggEl.classList.add('crack');
    sfx('forge');
    hatchTimer = setTimeout(() => { eggEl.classList.add('burst'); hatchTimer = setTimeout(reveal, 380); }, 750);
  }, 1300);
  box.querySelector('.dts-hatch').addEventListener('click', (e) => { if (!e.target.closest('button')) reveal(); });
}

// ---------- events ----------
function onClick(e) {
  const t = e.target.closest('button, .dts-modal');
  if (!t) return;
  const d = t.dataset;
  if (t.classList.contains('dts-modal')) { if (e.target === t && !t.querySelector('.dts-hatch')) closeModal(); return; }
  if (t.classList.contains('dts-close')) return close();
  if (t.classList.contains('dts-histbtn')) { const h = root.querySelector('.dts-hist'); h.innerHTML = histHTML(); h.classList.toggle('hidden'); return; }
  root.querySelector('.dts-hist').classList.add('hidden');
  if (d.mode) { view.mode = d.mode; if (view.mode === 'locker' && view.tab === 'eggs') view.tab = 'legend'; return render(); }
  if (d.tab) { view.tab = d.tab; view.slot = 0; root.querySelector('.dts-body').scrollTop = 0; return render(); }
  if (d.gostore) { view.mode = 'store'; view.tab = d.gostore; root.querySelector('.dts-body').scrollTop = 0; return render(); }
  if (d.cancel) { pending = null; return closeModal(); }
  if (d.confirm) return doBuy();
  if (d.locker) { closeModal(); view.mode = 'locker'; view.tab = d.locker; const eq = C.equipped(d.locker); view.slot = Math.max(0, eq.indexOf(null)); return render(); }
  if (d.equipnow) { const [k, id] = d.equipnow.split('|'); C.equip(k, id); sfx('equip'); msg(`Equipped ${C.get(k, id).name}.`); return closeModal(); }
  if (d.egg) return confirmEgg(d.egg);
  const kind = t.closest('[data-kind]') && t.closest('[data-kind]').dataset.kind;
  if (d.buy) return confirmBuy(kind, d.buy);
  if (d.equip) {
    if (MULTI(kind) && view.mode === 'store') { view.mode = 'locker'; view.tab = kind; view.slot = Math.max(0, C.equipped(kind).indexOf(null)); return render(); }
    C.equip(kind, d.equip); sfx('equip'); return render();
  }
  if (d.slot != null) { view.slot = Number(d.slot); return render(); }
  if (d.slotset) { C.equip(view.tab, d.slotset, view.slot); sfx('equip'); view.slot = (view.slot + 1) % C.SLOTS; return render(); }
  if (d.slotclear) { C.equip(view.tab, null, view.slot); return render(); }
}

function show(mode, tab) {
  build();
  view.mode = mode;
  if (tab) view.tab = tab;
  if (mode === 'locker' && view.tab === 'eggs') view.tab = 'legend';
  view.slot = 0;
  view.open = true;
  root.classList.remove('hidden');
  root.querySelector('.dts-body').scrollTop = 0;
  render();
}
function open(tab) { show('store', tab); }
function openLocker(kind) { show('locker', kind); }
function close() {
  if (!root) return;
  view.open = false;
  dispose();
  closeModal();
  root.classList.add('hidden');
  root.querySelector('.dts-body').innerHTML = '';
}

// Store buttons anywhere (title, lobby browser, top bar, game-over box) use data attributes
document.addEventListener('click', (e) => {
  const b = e.target.closest && e.target.closest('[data-shop-open], [data-locker-open]');
  if (!b) return;
  e.preventDefault();
  if (b.hasAttribute('data-locker-open')) openLocker(b.getAttribute('data-locker-open') || undefined);
  else open(b.getAttribute('data-shop-open') || undefined);
});
window.addEventListener('keydown', (e) => {
  if (!view.open || e.key !== 'Escape') return;
  if (DT.LegendEvo && DT.LegendEvo.isOpen && DT.LegendEvo.isOpen()) return; // the Digivolve panel on top closes first
  const m = root.querySelector('.dts-modal');
  if (!m.classList.contains('hidden')) closeModal(); else close();
}, true);

C.onChange(queueRender);
W.onChange((bits) => {
  for (const b of document.querySelectorAll('.dts-wbits')) b.textContent = bits;
  const tb = document.getElementById('storeBtn');
  if (tb) tb.title = `Store (${bits} Bits)`;
  queueRender();
});

// Top-bar Store button only during planning (or outside a game)
function topbarState() {
  const b = document.getElementById('storeBtn');
  if (!b) return;
  const g = UI().game && UI().game();
  b.classList.toggle('hidden', !!(g && !g.over && g.human && g.human.alive && g.phase !== 'planning'));
}

// for the client shell's Home page: today's missions with their state, and the first-game-of-the-day bonus
function missionInfo() {
  return todaysMissions().map((m) => ({ id: m.id, name: m.name, bits: m.bits, goal: m.goal, val: m.val, done: save.missions.done.includes(m.id) }));
}
const daily = () => ({ claimed: save.lastDay === today(), bits: FIRST_GAME_BITS });

DT.Shop = { open, openLocker, close, award, showAward, isOpen: () => view.open, missions: todaysMissions, missionInfo, daily, priceOf, featured, eggs: () => EGGS.map((e) => ({ ...e, odds: eggOdds(eggPool(e)) })), _track: track,
  eggPool: (id) => { const e = EGGS.find((x) => x.id === id); return e ? eggPool(e).map((x) => ({ kind: x.kind, id: x.item.id, rarity: x.item.rarity })) : []; },
  _rollEgg: (id) => { const e = EGGS.find((x) => x.id === id); const r = e && rollEgg(e); return r ? { kind: r.kind, id: r.item.id } : null; } };
