// Client shell: the out-of-game screens in one place, laid out like a game launcher (League-style client).
// Top bar (PLAY button, tabs Home, Modes, Collection, Loot, Store, Digi-Farm, Codex, then Bits, store and loot icons,
// profile with level ring, settings and a button that hides the friends list), a social sidebar (your party, friends
// or recent players grouped Online / In Game / Offline), a boot splash while models load, and the pages Home, Play
// (mode select -> party -> match loading screen), Collection, Loot, Profile and Settings. The Store (js/shop.js),
// Digi-Farm (js/farm-ui.js) and Codex (js/codex.js) keep their own windows; while the shell is up they sit under the
// top bar as pages (styles in js/theme.js). #overlay is the page area: ui.js hides it when a game starts.
// Art: assets/client/*.webp (Astra paintings) and assets/client/renders/*.webp (our own 3D models).
// Client-only: reads game state for the loading screen, the end-of-game screen and match history, never changes it.
//
// DT.Client.go(page)          'home' | 'play' | 'collection' | 'loot' | 'profile' | 'settings' | 'store' | 'farm' | 'codex'
// DT.Client.back()            previous page (Esc)
// DT.Client.home()            Home (ui.js showTitle)
// DT.Client.gameOver(o)       end-of-game screen and a match-history entry; o: { game, session, again }
// DT.Client.eliminated(o)     multiplayer: out while others play; o: { game, session, watch, leave }
// DT.Client.showLobby(force)  net.js screens live on the Play page; force brings it forward
// DT.Client.toast(msg, kind)  kind: 'info' | 'good' | 'bad'
// DT.Client.history() / stats() / level()
import * as THREE from 'three';
import { lib } from './render3d.js';

const DT = window.DT;
const C = DT.Cosmetics;
const W = DT.Wallet;
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const bit = '<i class="bit"></i>';
const RARITIES = ['common', 'uncommon', 'rare', 'epic', 'legendary'];
const KINDS = [['legend', 'Little Legends', 'Little Legend'], ['board', 'Boards', 'Board'], ['killscreen', 'Kill Screens', 'Kill Screen'],
  ['dance', 'Dances', 'Dance'], ['emote', 'Emotes', 'Emote'], ['sound', 'Soundboard', 'Sound'], ['elimsound', 'Elim Sounds', 'Elim Sound']];
const KIND_ONE = Object.fromEntries(KINDS.map(([k, , one]) => [k, one]));
const MULTI = (k) => k === 'emote' || k === 'sound';
const OWN_PAGES = ['home', 'play', 'collection', 'loot', 'profile', 'settings'];
// Steam desktop build: the self-updater bridge (desktop-steam/preload.js); null on the web
const UPD = typeof window !== 'undefined' && window.DTUpdate ? window.DTUpdate : null;

// ---------- art ----------
const ART = (n) => `assets/client/${n}.webp`;
const RENDER_ART = (n) => `assets/client/renders/${n}.webp`;
const SET_ART = {
  'faction-wars': { hero: 'hero-faction-wars', mode: 'mode-faction-wars', renders: ['fw-1', 'fw-2'], tag: 'Set 1' },
  dragonlands: { hero: 'hero-dragonlands', mode: 'mode-dragonlands', renders: ['dl-1', 'dl-2'], tag: 'Set 2' },
  autochess: { hero: 'hero-autochess', mode: 'mode-autochess', renders: ['ac-1', 'ac-2'], tag: 'Auto Chess' },
  chaos: { hero: 'hero-server-continent', mode: 'mode-chaos', renders: [], tag: 'Any set' },
};
// The hero paintings carry their own Digimon (center or right, the left third stays calm for the title), so the 3D
// renders of our models go on the smaller set tiles rather than over the hero art.
const HERO_RENDERS = false;
const setArt = (id) => SET_ART[id] || { hero: 'hero-digital-world', mode: 'mode-multiplayer', renders: [], tag: 'Set' };
const COLL_ART = { legend: 'coll-legends', board: 'coll-boards', killscreen: 'store-fx', dance: 'store-fx', emote: 'coll-emotes', sound: 'coll-emotes', elimsound: 'store-fx' };
const PROFILE_BANNERS = ['profile-1', 'profile-2', 'profile-3', 'profile-4'];
const RANKS = [
  { id: 'rookie', name: 'Rookie', emblem: 'emblem-rookie' },
  { id: 'champion', name: 'Champion', emblem: 'emblem-champion' },
  { id: 'ultimate', name: 'Ultimate', emblem: 'emblem-ultimate' },
  { id: 'mega', name: 'Mega', emblem: 'emblem-mega' },
];

// ---------- save ----------
const KEY = 'dt.client.v1';
let save = { profile: { avatar: null, banner: 'profile-1' }, matches: [], recent: [], totals: { games: 0, top4: 0, wins: 0, placeSum: 0, traits: {}, units: {} }, prefs: { motion: true, uiSfx: true, social: true } };
try {
  const s = JSON.parse(localStorage.getItem(KEY));
  if (s && typeof s === 'object') {
    save = { ...save, ...s, profile: { ...save.profile, ...(s.profile || {}) }, totals: { ...save.totals, ...(s.totals || {}) }, prefs: { ...save.prefs, ...(s.prefs || {}) } };
    if (!Array.isArray(save.matches)) save.matches = [];
    if (!Array.isArray(save.recent)) save.recent = [];
  }
} catch (e) { /* blocked or corrupt: start fresh */ }
const persist = () => { try { localStorage.setItem(KEY, JSON.stringify(save)); } catch (e) { /* private mode */ } };

const store = {
  get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
};
const cleanName = (s) => String(s || '').replace(/[^\w .'\-]/g, '').trim().slice(0, 16);
// Steam builds show the Steam persona name (as net.js does); the web uses the typed name
let steamPersona = '';
const myName = () => steamPersona || cleanName(store.get('dt.name')) || '';
if (window.DTSteam && window.DTSteam.init) {
  Promise.resolve(window.DTSteam.init()).then((i) => {
    steamPersona = String((i && i.ok && i.name) || '').replace(/[\u0000-\u001f<>&"\\]/g, '').trim().slice(0, 16);
    if (steamPersona && typeof refreshName === 'function') refreshName();
  }).catch(() => {});
}

let lastSfx = 0;
function sfx(n, quiet) {
  if (quiet && !save.prefs.uiSfx) return;
  const now = performance.now();
  if (quiet && now - lastSfx < 70) return;
  lastSfx = now;
  try { if (DT.Sfx) DT.Sfx.play(n); } catch (e) { /* audio blocked */ }
}

// ---------- Tamer level and rank (from your match totals) ----------
const XP_PER = (lv) => 300 + 100 * (lv - 1);
function tamerLevel() {
  const T = save.totals;
  let xp = T.games * 100 + T.top4 * 60 + T.wins * 120;
  const total = xp;
  let lv = 1;
  while (xp >= XP_PER(lv)) { xp -= XP_PER(lv); lv++; }
  return { level: lv, xp, need: XP_PER(lv), total };
}
function tamerRank() {
  const T = save.totals;
  if (T.games < 3) return { ...RANKS[0], unranked: true, label: 'Unranked', sub: `Play ${3 - T.games} more game${3 - T.games === 1 ? '' : 's'} to get a rank` };
  const avg = T.placeSum / T.games;
  const r = avg <= 2.8 ? RANKS[3] : avg <= 3.8 ? RANKS[2] : avg <= 4.8 ? RANKS[1] : RANKS[0];
  return { ...r, label: r.name, sub: `Average place ${avg.toFixed(2)} over ${T.games} games` };
}

// ---------- state ----------
const st = { page: 'home', hist: [], module: null, playMode: 'solo', playStep: 'select', lobbyFollow: false, result: null, heroIdx: 0, mpLocked: false, htab: 'overview', lootSel: null };
const overlay = $('overlay');
const clientOn = () => !overlay.classList.contains('hidden');

const MODULES = {
  store: { isOpen: () => !!(DT.Shop && DT.Shop.isOpen()), open: () => DT.Shop && DT.Shop.open(), close: () => DT.Shop && DT.Shop.close() },
  farm: { isOpen: () => !!(DT.FarmUI && DT.FarmUI.isOpen()), open: () => DT.FarmUI && DT.FarmUI.open(), close: () => DT.FarmUI && DT.FarmUI.close() },
  codex: { isOpen: () => !!(DT.Codex && DT.Codex.fullOpen()), open: () => DT.Codex && DT.Codex.openFull(), close: () => DT.Codex && DT.Codex.closeFull() },
};
const openModule = () => Object.keys(MODULES).find((k) => MODULES[k].isOpen()) || null;
function closeModules(except) {
  for (const k of Object.keys(MODULES)) if (k !== except && MODULES[k].isOpen()) { try { MODULES[k].close(); } catch (e) { console.warn(e); } }
}

// ---------- page switching ----------
const pageEl = (p) => $(`pg${p[0].toUpperCase()}${p.slice(1)}`);
const RENDER = {};

function go(page, opts = {}) {
  if (MODULES[page]) {
    closeModules(page);
    if (!MODULES[page].isOpen()) MODULES[page].open();
    syncModules();
    return;
  }
  if (!OWN_PAGES.includes(page)) return;
  closeModules();
  st.module = null; // closed just now; don't wait for the poll
  document.body.classList.remove('client-module');
  if (page !== 'play') st.lobbyFollow = false;
  if (opts.push !== false && page !== st.page) { st.hist.push(st.page); if (st.hist.length > 20) st.hist.shift(); }
  const changed = page !== st.page;
  st.page = page;
  for (const p of OWN_PAGES) pageEl(p).classList.toggle('on', p === page);
  if (changed) { const el = pageEl(page); el.scrollTop = 0; }
  if (RENDER[page]) RENDER[page]();
  syncNav();
  syncStage();
}

function back() {
  if (st.page === 'play' && st.playStep === 'party' && st.playMode === 'solo') { setPlayStep('select'); return; }
  while (st.hist.length) {
    const p = st.hist.pop();
    if (p !== st.page) return go(p, { push: false });
  }
  if (st.page !== 'home') go('home', { push: false });
}

function syncNav() {
  const active = st.module || st.page;
  for (const b of document.querySelectorAll('#cnav [data-page]')) {
    const on = b.dataset.page === active && !(b.id === 'cnStatus' || b.id === 'cnBits' || b.id === 'cnStoreIc' || b.id === 'cnLootIc' || b.id === 'cnPlay');
    b.classList.toggle('on', on);
    if (b.classList.contains('cn-tab')) b.setAttribute('aria-current', on ? 'page' : 'false');
  }
  $('cnPlay').classList.toggle('on', active === 'play');
}

// a docked Store / Farm / Codex window opened or closed (by the nav, its own buttons or Esc)
function syncModules() {
  const m = clientOn() ? openModule() : null;
  if (m === st.module) return;
  st.module = m;
  document.body.classList.toggle('client-module', !!m);
  syncNav();
  syncStage();
  if (!m && clientOn() && RENDER[st.page]) RENDER[st.page]();
}

// body.client-on while the shell is showing; modules use it to dock under the nav
function syncClient() {
  const on = clientOn();
  if (document.body.classList.contains('client-on') === on) return;
  document.body.classList.toggle('client-on', on);
  if (on) {
    refreshChrome();
    if (RENDER[st.page]) RENDER[st.page]();
  } else {
    closeModal();
    st.module = null;
    document.body.classList.remove('client-module');
    maybeLoadingScreen();
  }
  syncStage();
}
new MutationObserver(syncClient).observe(overlay, { attributes: true, attributeFilter: ['class'] });

// ---------- nav ----------
const nav = $('cnav');
nav.addEventListener('click', (e) => {
  if (e.target.closest('#cnMin')) { toggleSocial(); return; }
  const b = e.target.closest('[data-page]');
  if (!b) return;
  const page = b.dataset.page;
  sfx(b.id === 'cnPlay' ? 'levelup' : 'pick', true);
  if (MODULES[page]) {
    // the module's own document listener (data-shop-open, data-farm-open, data-codex-open) opens it next
    closeModules(page);
    return;
  }
  if (b.dataset.playMode) { setPlayMode(b.dataset.playMode); st.lobbyFollow = true; if (st.playMode === 'mp') st.playStep = 'party'; }
  if (page === 'play' && b.classList.contains('cn-tab') && !inRoom()) st.playStep = 'select';
  go(page);
});
document.addEventListener('mouseover', (e) => {
  if (!document.body.classList.contains('client-on')) return;
  const b = e.target.closest && e.target.closest('#cnav button, #overlay .btn, #overlay .ccard, #overlay .mode-card, #overlay .ctab, #overlay .set-card, #overlay .hero-thumb, #overlay .loot-item, #csocial .cs-row');
  if (b && !b.contains(e.relatedTarget)) sfx('tick', true);
});

function refreshName() {
  const n = myName() || 'Tamer';
  for (const el of document.querySelectorAll('.cn-pn')) el.textContent = n;
}
function avatarId() {
  const a = save.profile.avatar;
  return a && C.owns('legend', a) ? a : C.equipped('legend');
}
const legendKey = (id) => (DT.LegendEvo ? DT.LegendEvo.key(id) : id);
function avatarHTML(cls = '') { return DT.Legends ? DT.Legends.html(legendKey(avatarId()), cls) : ''; }
const lookKey = (look) => (look && look.legend ? look.legend : null);

function refreshChrome() {
  const bits = $('cnBits');
  if (bits) bits.querySelector('b').textContent = W.bits.toLocaleString();
  const av = nav.querySelector('.cn-av');
  if (av) av.innerHTML = avatarHTML();
  const L = tamerLevel();
  const lv = nav.querySelector('.cn-lv');
  if (lv) lv.textContent = L.level;
  const ring = nav.querySelector('.rg-fg');
  if (ring) { const c = 2 * Math.PI * 22; ring.style.strokeDasharray = `${c}`; ring.style.strokeDashoffset = `${c * (1 - L.xp / L.need)}`; }
  refreshName();
  refreshStatus();
}
const inRoom = () => !!(DT.Net && DT.Net.status && DT.Net.status().room);
function refreshStatus() {
  const el = $('cnStatus');
  if (!el) return;
  const s = DT.Net && DT.Net.status ? DT.Net.status() : { connected: false, room: null };
  let txt = 'Solo', cls = '';
  if (s.room) { txt = s.room.inGame ? 'In a game' : `Lobby ${s.room.seated}/8`; cls = 'live'; }
  else if (s.connected) { txt = 'Online'; cls = 'on'; }
  el.className = `cn-status ${cls}`;
  const span = el.querySelector('span');
  if (span.textContent !== txt) span.textContent = txt;
  el.title = s.room ? `Lobby: ${s.room.name}` : 'Multiplayer lobbies';
  const stEl = nav.querySelector('.cn-st span');
  const stTxt = s.room ? (s.room.inGame ? 'In game' : 'In lobby') : 'Online';
  if (stEl && stEl.textContent !== stTxt) { stEl.textContent = stTxt; nav.querySelector('.cn-st').className = `cn-st ${s.room ? 'busy' : ''}`; }
  const pt = nav.querySelector('.cn-play-t');
  const ptTxt = s.room && !s.room.inGame ? 'Lobby' : 'Play';
  if (pt && pt.textContent !== ptTxt) pt.textContent = ptTxt;
  const locked = !!((DT.UI && DT.UI.session && DT.UI.session()) || s.room);
  if (locked !== st.mpLocked) {
    st.mpLocked = locked;
    for (const id of ['startBtn', 'playSoloBtn']) {
      const b = $(id);
      if (!b) continue;
      b.disabled = locked;
      b.title = locked ? 'Leave the multiplayer lobby first' : '';
    }
  }
}
W.onChange((bits, n) => {
  refreshChrome();
  const b = $('cnBits');
  if (b && n > 0 && clientOn()) { b.classList.remove('pulse'); void b.offsetWidth; b.classList.add('pulse'); }
});
C.onChange((kind) => {
  if (!clientOn()) return;
  if (kind === 'legend') { refreshChrome(); if (stage) stage.setLegend(stageKey()); }
  queueRender();
});
let renderQueued = false;
function queueRender() {
  if (renderQueued) return;
  renderQueued = true;
  requestAnimationFrame(() => {
    renderQueued = false;
    if (!clientOn() || st.module) return;
    if (st.page === 'home') renderHomeParts();
    else if (st.page === 'collection') RENDER.collection();
    else if (st.page === 'profile') RENDER.profile();
    else if (st.page === 'loot') RENDER.loot();
  });
}

// ---------- toasts ----------
function toast(msg, kind = 'info') {
  const box = $('ctoasts');
  if (!box) return;
  const t = document.createElement('div');
  t.className = `ctoast ${kind}`;
  t.textContent = msg;
  box.appendChild(t);
  while (box.children.length > 3) box.firstChild.remove();
  setTimeout(() => t.classList.add('out'), 2400);
  setTimeout(() => t.remove(), 2700);
}
// net.js and others call DT.UI.toast; while the shell is up its toasts show here (the game's toast is under it)
if (DT.UI && DT.UI.toast) {
  const gameToast = DT.UI.toast;
  DT.UI.toast = (msg, kind) => (clientOn() ? toast(msg, kind === 'info' ? 'info' : 'bad') : gameToast(msg, kind));
}

// ---------- modal ----------
const modalEl = $('cmodal');
let modalClose = null;
function modal(html, opts = {}) {
  modalEl.innerHTML = `<div class="cm-box ${opts.cls || ''}">${html}</div>`;
  modalEl.classList.remove('hidden');
  modalClose = opts.onClose || null;
  const f = modalEl.querySelector('[data-autofocus]') || modalEl.querySelector('button');
  if (f) setTimeout(() => f.focus(), 30);
  return modalEl.firstChild;
}
function closeModal() {
  if (modalEl.classList.contains('hidden')) return;
  modalEl.classList.add('hidden');
  modalEl.innerHTML = '';
  const f = modalClose;
  modalClose = null;
  if (f) f();
}
const modalOpen = () => !modalEl.classList.contains('hidden');
modalEl.addEventListener('click', (e) => {
  if (e.target === modalEl || e.target.closest('[data-cm-close]')) closeModal();
});

// ---------- keyboard: Esc goes back ----------
// Registered before the Store, Farm and Codex listeners (this module loads first), so it sees the state they close.
const popupOpen = () => (DT.LegendEvo && DT.LegendEvo.isOpen && DT.LegendEvo.isOpen()) || (DT.SoundUI && DT.SoundUI.isOpen()) || ($('picker') && !$('picker').classList.contains('hidden'))
  || ($('emoteWheel') && $('emoteWheel').classList.contains('open'));
window.addEventListener('keydown', (e) => {
  if (e.key !== 'Escape' || !clientOn()) return;
  if (modalOpen()) { e.stopImmediatePropagation(); e.preventDefault(); closeModal(); return; }
  if (openModule() || popupOpen()) return; // their own handlers close them; syncModules then shows the page again
  const t = e.target;
  if (t && /input|textarea|select/i.test(t.tagName) && t.value && t.type === 'search') { t.value = ''; t.dispatchEvent(new Event('input', { bubbles: true })); return; }
  e.preventDefault();
  back();
}, true);

// ---------- 3D stage: your Little Legend on a pedestal (Home, solo party, Collection) ----------
class Stage {
  constructor() {
    this.canvas = document.createElement('canvas');
    this.canvas.className = 'stage-canvas';
    this.ok = true;
    try {
      this.r = new THREE.WebGLRenderer({ canvas: this.canvas, alpha: true, antialias: true });
    } catch (e) { this.ok = false; return; }
    this.r.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
    this.r.outputColorSpace = THREE.SRGBColorSpace;
    this.r.setClearColor(0x000000, 0);
    this.scene = new THREE.Scene();
    this.cam = new THREE.PerspectiveCamera(30, 1, 0.05, 50);
    this.scene.add(new THREE.HemisphereLight('#dfe9ff', '#26304a', 2.0));
    const key = new THREE.DirectionalLight('#fff3e0', 2.4);
    key.position.set(1.8, 3.2, 3.5);
    this.scene.add(key);
    const rim = new THREE.DirectionalLight('#3fd6ff', 1.6);
    rim.position.set(-2.5, 1.8, -2.5);
    this.scene.add(rim);
    // pedestal: a dark metal drum with gold trim, a glowing Digivice ring and a hex grid top
    this.ped = new THREE.Group();
    const metal = new THREE.MeshStandardMaterial({ color: '#18233a', metalness: 0.6, roughness: 0.42 });
    const drum = new THREE.Mesh(new THREE.CylinderGeometry(0.92, 1.02, 0.3, 64), metal);
    drum.position.y = -0.15;
    const base = new THREE.Mesh(new THREE.CylinderGeometry(1.08, 1.16, 0.12, 64), new THREE.MeshStandardMaterial({ color: '#0e1626', metalness: 0.5, roughness: 0.6 }));
    base.position.y = -0.36;
    const gold = new THREE.Mesh(new THREE.TorusGeometry(1.03, 0.018, 8, 96), new THREE.MeshStandardMaterial({ color: '#c8a462', metalness: 0.9, roughness: 0.3 }));
    gold.rotation.x = Math.PI / 2;
    gold.position.y = -0.3;
    const ringMat = new THREE.MeshBasicMaterial({ color: '#3fd6ff' });
    const ring = new THREE.Mesh(new THREE.TorusGeometry(0.93, 0.022, 10, 96), ringMat);
    ring.rotation.x = Math.PI / 2;
    ring.position.y = 0.002;
    const ring2 = new THREE.Mesh(new THREE.TorusGeometry(1.12, 0.012, 8, 96), new THREE.MeshBasicMaterial({ color: '#ff8a3d' }));
    ring2.rotation.x = Math.PI / 2;
    ring2.position.y = -0.42;
    const top = new THREE.Mesh(new THREE.CircleGeometry(0.9, 64), new THREE.MeshBasicMaterial({ map: hexTexture(), transparent: true }));
    top.rotation.x = -Math.PI / 2;
    top.position.y = 0.003;
    this.ped.add(drum, base, gold, ring, ring2, top);
    this.scene.add(this.ped);
    this.ringMat = ringMat;
    this.spin = new THREE.Group();
    this.scene.add(this.spin);
    this.key = null;
    this.view = null;
    this.host = null;
    this.raf = 0;
    this.t = 0;
    this.drag = null;
    this.yawOff = 0;
    this.canvas.addEventListener('pointerdown', (e) => { this.drag = { x: e.clientX, y0: this.yawOff, moved: false }; this.canvas.setPointerCapture(e.pointerId); });
    this.canvas.addEventListener('pointermove', (e) => {
      if (!this.drag) return;
      const dx = e.clientX - this.drag.x;
      if (Math.abs(dx) > 3) this.drag.moved = true;
      this.yawOff = this.drag.y0 + dx * 0.012;
    });
    this.canvas.addEventListener('pointerup', () => {
      if (this.drag && !this.drag.moved && this.view) {
        const roles = ['happy', 'taunt', 'dance', 'wave'];
        this.view.play(roles[(Math.random() * roles.length) | 0]);
        sfx('emote', true);
      }
      this.drag = null;
    });
  }
  setLegend(key) {
    if (!this.ok || !key || key === this.key) return;
    this.key = key;
    if (this.view) this.view.dispose();
    this.view = DT.Legends.createView(key, { ring: false, yaw: 0, height: 1.05 });
    this.spin.add(this.view.group);
  }
  mount(host) {
    if (!this.ok) return;
    if (this.host !== host) { host.appendChild(this.canvas); this.host = host; this.w = 0; }
    if (!this.raf) { this.last = performance.now(); this.raf = requestAnimationFrame((t) => this.frame(t)); }
  }
  stop() { cancelAnimationFrame(this.raf); this.raf = 0; }
  frame(now) {
    this.raf = 0;
    if (!this.host || !this.host.isConnected || !this.host.offsetParent || !document.body.classList.contains('client-on') || st.module || st.covered) return;
    const dt = Math.min(0.1, (now - this.last) / 1000);
    this.last = now;
    this.t += dt;
    const w = this.host.clientWidth, h = this.host.clientHeight;
    if (w > 0 && h > 0 && (w !== this.w || h !== this.h)) {
      this.w = w; this.h = h;
      this.r.setSize(w, h, false);
      this.cam.aspect = w / h;
      // keep the whole pedestal (about 2.4 wide) and the legend (about 1.3 tall) in frame
      const t = Math.tan((this.cam.fov * Math.PI) / 360);
      const d = Math.max(1.05 / t, 1.32 / (t * this.cam.aspect)) + 0.6;
      this.cam.position.set(0, 0.55 + d * 0.2, d);
      this.cam.lookAt(0, 0.38, 0);
      this.cam.updateProjectionMatrix();
    }
    if (this.view) {
      this.view.update(dt);
      const sway = save.prefs.motion ? Math.sin(this.t * 0.45) * 0.35 : 0;
      this.spin.rotation.y = sway + this.yawOff;
    }
    this.ringMat.color.setHSL(0.53, 1, 0.55 + Math.sin(this.t * 2) * 0.08);
    this.r.render(this.scene, this.cam);
    this.raf = requestAnimationFrame((t) => this.frame(t));
  }
}
function hexTexture() {
  const c = document.createElement('canvas');
  c.width = c.height = 256;
  const g = c.getContext('2d');
  const grd = g.createRadialGradient(128, 128, 10, 128, 128, 128);
  grd.addColorStop(0, 'rgba(63,214,255,0.28)');
  grd.addColorStop(0.75, 'rgba(30,60,110,0.22)');
  grd.addColorStop(1, 'rgba(10,15,26,0)');
  g.fillStyle = grd;
  g.fillRect(0, 0, 256, 256);
  g.strokeStyle = 'rgba(120,220,255,0.35)';
  g.lineWidth = 1.5;
  const r = 14, hh = Math.sqrt(3) * r;
  for (let y = -hh; y < 256 + hh; y += hh) {
    for (let x = -r * 3, k = 0; x < 256 + r * 3; x += r * 1.5, k++) {
      const yy = y + (k % 2 ? hh / 2 : 0);
      g.beginPath();
      for (let i = 0; i < 6; i++) { const a = (Math.PI / 3) * i; const px = x + r * Math.cos(a), py = yy + r * Math.sin(a); if (i) g.lineTo(px, py); else g.moveTo(px, py); }
      g.closePath();
      g.stroke();
    }
  }
  const t = new THREE.CanvasTexture(c);
  t.colorSpace = THREE.SRGBColorSpace;
  return t;
}
let stage = null;
// Home and the solo party show the equipped legend; the Collection's legend tab shows the selected one (owned or not)
const stageKey = () => legendKey(st.page === 'collection' && col.tab === 'legend' && col.sel.legend ? col.sel.legend : C.equipped('legend'));
function syncStage() {
  if (!DT.Legends) return;
  const host = !clientOn() || st.module ? null
    : st.page === 'home' && !st.result && st.htab === 'overview' ? $('homeStage')
      : st.page === 'play' && st.playStep === 'party' && st.playMode === 'solo' ? document.querySelector('#partySlots .party-stage')
        : st.page === 'collection' && col.tab === 'legend' ? document.querySelector('#pgCollection .cprev-stage') : null;
  if (!host) { if (stage) stage.stop(); return; }
  if (!stage) stage = new Stage();
  if (!stage.ok) return;
  stage.setLegend(stageKey());
  stage.mount(host);
}

// ---------- previews (cosmetic item.preview(el), as the Store does) ----------
let disposers = [];
function disposePreviews(scope) {
  disposers = disposers.filter((d) => {
    if (scope && d.el.isConnected && !scope.contains(d.el)) return true;
    try { d.off(); } catch (e) { /* ignore */ }
    return false;
  });
}
function iconHTML(item) {
  const ic = item && item.icon;
  if (ic && /[/.]/.test(ic) && /^(https?:|data:|\.|\/|[\w-]+\/)/.test(ic)) return `<img src="${esc(ic)}" alt="">`;
  if (ic) return `<span class="cp-emo">${esc(ic)}</span>`;
  return `<span class="cp-art">${esc((item && item.name ? item.name : '?').split(/\s+/).map((w) => w[0]).join('').slice(0, 3))}</span>`;
}
function fillPreview(box, kind, item) {
  box.innerHTML = '';
  if (item && typeof item.preview === 'function') {
    try {
      const off = item.preview(box);
      if (typeof off === 'function') disposers.push({ el: box, off });
      if (box.childNodes.length || box.style.backgroundImage) return;
    } catch (e) { console.warn('preview failed', kind, item.id, e); }
  }
  box.innerHTML = iconHTML(item);
}
function fillPreviews(scope) {
  for (const box of scope.querySelectorAll('[data-prev]')) {
    const [kind, id] = box.dataset.prev.split('|');
    fillPreview(box, kind, C.get(kind, id));
  }
}

// =====================================================================
// Social sidebar: profile, party, friends (Steam) or players from lobbies and recent games (web)
// =====================================================================
const STEAM = typeof window !== 'undefined' && !!window.DTSteam;
const social = { friends: [], polled: 0, personas: {}, open: { party: true, online: true, ingame: true, offline: false }, html: '' };
const steamPic = (url) => (url && /^data:image\/png;base64,[\w+/=]+$/.test(url) ? url : null);
async function pollSteamFriends() {
  if (!STEAM || !document.body.classList.contains('client-on')) return;
  social.polled = Date.now();
  try {
    const rows = await window.DTSteam.friendLobbies();
    social.friends = (rows || []).map((r) => {
      const d = r.data || {};
      const ours = d.dt_game === 'digitactics';
      return { id: String(r.friendId), name: String(r.friendName || 'Friend').replace(/[\u0000-\u001f<>&"\\]/g, '').slice(0, 24), lobby: String(r.id), ours,
        state: ours ? (d.state === 'playing' ? 'ingame' : 'online') : 'ingame', note: ours ? (d.state === 'playing' ? `In game · ${d.round || '1-1'}` : `In lobby · ${String(d.name || '').slice(0, 24)}`) : 'Playing Spacewar' };
    });
    for (const f of social.friends) {
      if (social.personas[f.id] !== undefined) continue;
      social.personas[f.id] = null;
      window.DTSteam.persona(f.id).then((p) => { social.personas[f.id] = steamPic(p && p.avatar); renderSocial(); }).catch(() => {});
    }
  } catch (e) { social.friends = []; }
  renderSocial();
}
function rememberPlayers(members) {
  let changed = false;
  const me = myName();
  for (const m of members || []) {
    if (!m || !m.name || m.me || m.name === me) continue;
    const old = save.recent.find((r) => r.name === m.name);
    if (old) { if (Date.now() - old.at > 60000) { old.at = Date.now(); changed = true; } } else { save.recent.unshift({ name: m.name, at: Date.now() }); changed = true; }
  }
  if (changed) { save.recent = save.recent.sort((a, b) => b.at - a.at).slice(0, 20); persist(); }
}
const initials = (n) => esc(String(n || '?').replace(/[^\w]/g, '').slice(0, 2).toUpperCase() || '?');
function csRow(p) {
  const pic = p.pic ? `<img src="${p.pic}" alt="">` : p.legend && DT.Legends ? DT.Legends.html(p.legend, 'cs-pt') : `<b>${initials(p.name)}</b>`;
  const act = p.join ? `<button class="cs-act" data-join="${esc(p.join)}" title="Join their lobby">Join</button>` : p.invite ? `<button class="cs-act" data-invite title="Invite to your party">Invite</button>` : '';
  return `<div class="cs-row s-${p.state}${p.me ? ' me' : ''}" title="${esc(p.name)}${p.note ? ` · ${esc(p.note)}` : ''}">
    <span class="cs-av">${pic}<i class="cs-dot"></i></span>
    <span class="cs-txt"><b>${esc(p.name)}</b><small>${esc(p.note || '')}</small></span>${act}</div>`;
}
function renderSocial() {
  const body = $('csBody');
  if (!body || !document.body.classList.contains('client-on')) return;
  const s = DT.Net && DT.Net.status ? DT.Net.status() : { room: null };
  const room = s.room;
  const members = (room && room.members) || [];
  if (members.length) rememberPlayers(members);
  const listed = new Set([myName()]);
  const groups = { party: [], online: [], ingame: [], offline: [] };
  for (const m of members) {
    listed.add(m.name);
    groups.party.push({ name: m.name, state: m.online === false ? 'offline' : room.inGame ? 'ingame' : 'online', note: `${m.host ? 'Host' : 'Member'}${m.me ? ' · you' : ''}${m.online === false ? ' · offline' : ''}`, me: m.me, legend: m.legend, pic: m.pic });
  }
  if (STEAM) {
    for (const f of social.friends) {
      if (listed.has(f.name)) continue;
      listed.add(f.name);
      groups[f.state].push({ name: f.name, state: f.state, note: f.note, pic: social.personas[f.id], join: f.ours && (!room || room.id !== f.lobby) ? f.lobby : null });
    }
  } else if (DT.Net && DT.Net.lobbies) {
    for (const l of DT.Net.lobbies() || []) {
      const playing = l.status === 'playing';
      for (const seat of Object.values(l.slots || {})) {
        if (!seat || !seat.name || listed.has(seat.name)) continue;
        listed.add(seat.name);
        groups[playing ? 'ingame' : 'online'].push({ name: seat.name, state: playing ? 'ingame' : 'online', note: playing ? `In game · ${l.round || '1-1'}` : `In lobby · ${l.name || ''}`, join: !room || room.id !== l.id ? l.id : null });
      }
    }
  }
  for (const r of save.recent) {
    if (listed.has(r.name)) continue;
    listed.add(r.name);
    groups.offline.push({ name: r.name, state: 'offline', note: `Played together ${new Date(r.at).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}`, invite: !!room && !STEAM });
  }
  const sec = (k, title, list, empty) => `<section class="cs-sec${social.open[k] ? ' open' : ''}">
    <button class="cs-sh" data-cs-sec="${k}"><i class="cs-car"></i>${title} <span>(${list.length})</span></button>
    <div class="cs-list">${list.length ? list.map(csRow).join('') : `<div class="cs-empty">${empty}</div>`}</div></section>`;
  const html = (room ? sec('party', room.inGame ? 'Match' : 'Party', groups.party, 'Nobody here yet') : '')
    + sec('online', 'Online', groups.online, STEAM ? 'No Steam friends in a Digi-Tactics lobby right now.' : 'Open Multiplayer to see who is in a lobby.')
    + sec('ingame', 'In Game', groups.ingame, 'Nobody is mid-game.')
    + sec('offline', STEAM ? 'Recent players' : 'Offline', groups.offline, 'Players from your lobbies show up here.');
  if (html !== social.html) { social.html = html; body.innerHTML = html; }
  const foot = $('csFoot');
  const ftxt = room ? `<span class="cs-room"><b>${esc(room.name)}</b><small>${room.inGame ? 'Game in progress' : `${room.seated}/8 players`}</small></span><button class="cs-fbtn" data-cs-lobby>Open</button>`
    : `<span class="cs-room"><b>${STEAM ? 'Steam' : 'Not in a lobby'}</b><small>${STEAM ? 'Friends come from your Steam friends list' : 'Create a lobby to invite friends'}</small></span><button class="cs-fbtn" data-cs-lobby>${STEAM ? 'Lobbies' : 'Lobbies'}</button>`;
  if (foot.dataset.h !== ftxt) { foot.dataset.h = ftxt; foot.innerHTML = ftxt; }
}
function socialInvite() {
  sfx('pick', true);
  if (DT.Net && DT.Net.invite && inRoom()) { DT.Net.invite(); return; }
  if (DT.UI && DT.UI.inGame && DT.UI.inGame()) return toast('Invites work from the lobby');
  toast(STEAM ? 'Create a lobby first, then invite Steam friends' : 'Create a lobby first, then send the invite link', 'info');
  setPlayMode('mp');
  st.playStep = 'party';
  st.lobbyFollow = true;
  go('play');
}
$('csocial').addEventListener('click', (e) => {
  const sh = e.target.closest('[data-cs-sec]');
  if (sh) { const k = sh.dataset.csSec; social.open[k] = !social.open[k]; sh.parentElement.classList.toggle('open', social.open[k]); social.html = ''; sfx('tick', true); return; }
  if (e.target.closest('#csInvite') || e.target.closest('[data-invite]')) return socialInvite();
  const j = e.target.closest('[data-join]');
  if (j && DT.Net) {
    if (DT.UI && DT.UI.inGame && DT.UI.inGame()) return toast('Leave your current game first');
    sfx('pick');
    if (!myName() && !STEAM) { setPlayMode('mp'); st.playStep = 'party'; go('play'); return toast('Enter your name first', 'info'); }
    DT.Net.open(j.dataset.join);
    return;
  }
  if (e.target.closest('[data-cs-lobby]')) { sfx('pick', true); setPlayMode('mp'); st.playStep = 'party'; st.lobbyFollow = true; go('play'); }
});
function toggleSocial(force) {
  save.prefs.social = force != null ? force : !save.prefs.social;
  persist();
  document.body.classList.toggle('social-off', !save.prefs.social);
  const b = $('cnMin');
  b.title = save.prefs.social ? 'Hide the friends list' : 'Show the friends list';
  b.classList.toggle('on', !save.prefs.social);
  sfx('tick', true);
  if (stage) stage.w = 0;
}
document.body.classList.toggle('social-off', !save.prefs.social);

// =====================================================================
// Home
// =====================================================================
const PATCH = [
  ['New client', 'Home, Modes, Collection, Loot, Store, Digi-Farm and Codex in one launcher, with a friends list, a party screen and a match loading screen.'],
  ['Sets', () => `${setOptions().filter((o) => o.id !== 'chaos').map((o) => o.name).join(', ').replace(/, ([^,]*)$/, ' and $1')}. Chaos plays a random set every game. Browse every set in the Codex.`],
  ['Digi-Farm', 'Raise your Little Legends on a themed island. Four training mini-games raise their stats.'],
  ['Digivolution', 'Little Legends level up, then digivolve along their own trees up to Ultimate.'],
  ['Digi-Eggs', 'Rare, epic and legendary Little Legends only hatch from eggs. Open them from Loot.'],
  ['Boards and dances', '27 arena boards, some react to the fight. 11 dances for kill screens and previews.'],
  ['Item cards', 'Drag an item over a Digimon to see what it will make before you drop it.'],
];

function bestToday(m) {
  const d = new Date().toDateString();
  let best = 0;
  for (const x of save.matches) if (new Date(x.at).toDateString() === d && x.stats) best = Math.max(best, Math.min(m.goal, m.val(x.stats) || 0));
  return best;
}
const setOptions = () => (DT.SetPicker ? DT.SetPicker.options() : (DT.Sets ? DT.Sets.pickable() : []));
const selectedSet = () => (DT.SetPicker ? DT.SetPicker.selected() : (DT.Sets ? DT.Sets.DEFAULT : 'faction-wars'));
const setName = (id) => (DT.SetPicker ? DT.SetPicker.label(id) : id);
function setInfo(id) {
  if (id === 'chaos') return { name: 'Chaos', desc: 'A random set every game, picked from the game seed.', mech: null, traits: 0, lines: 0 };
  try {
    const S = DT.Sets.get(id);
    return { name: S.name, desc: S.desc, mech: S.mechanic && S.mechanic.name ? S.mechanic : null, traits: Object.keys(S.TRAITS || {}).length, lines: (S.SHOP_LINES || []).length, mode: S.def && S.def.mode };
  } catch (e) { return { name: setName(id), desc: '', mech: null, traits: 0, lines: 0 }; }
}

function renderHomeParts() {
  renderHero();
  renderMissions();
  renderLegendCard();
  renderPatch();
  renderSetNews();
  renderFeatured();
}
RENDER.home = () => {
  const res = !!st.result;
  $('homeResults').classList.toggle('hidden', !res);
  $('homeTabs').classList.toggle('hidden', res);
  for (const [k, id] of [['overview', 'homeOverview'], ['patch', 'homePatchNotes'], ['events', 'homeEvents']]) $(id).classList.toggle('hidden', res || st.htab !== k);
  for (const b of document.querySelectorAll('#homeTabs [data-htab]')) { b.classList.toggle('on', b.dataset.htab === st.htab); b.setAttribute('aria-selected', b.dataset.htab === st.htab); }
  $('homeHero').classList.toggle('hidden', res);
  if (res) return;
  if (st.htab === 'overview') renderHomeParts();
  else if (st.htab === 'patch') renderPatchNotes();
  else renderEvents();
};
$('homeTabs').addEventListener('click', (e) => {
  const b = e.target.closest('[data-htab]');
  if (!b || b.dataset.htab === st.htab) return;
  st.htab = b.dataset.htab;
  sfx('pick', true);
  RENDER.home();
  syncStage();
});

// ---------- hero carousel ----------
function heroSlides() {
  const out = [];
  const sets = setOptions().map((o) => o.id);
  for (const id of sets) {
    const a = setArt(id);
    const info = setInfo(id);
    if (id === 'chaos') {
      out.push({ id: 'set-chaos', art: a.hero, eyebrow: 'Mode · Any set', title: 'Chaos', desc: info.desc + ' Every Tamer in the lobby plays the same set.', cta: 'Play Chaos', act: 'set:chaos', thumb: 'Chaos' });
      continue;
    }
    out.push({ id: `set-${id}`, art: a.hero, eyebrow: `${a.tag} · ${info.mode === 'autochess' ? 'Auto Chess rules' : 'Hyper Roll'}`, title: info.name, desc: info.desc,
      cta: `Play ${info.name}`, act: `set:${id}`, renders: a.renders, thumb: info.name, meta: `${info.lines} Digimon · ${info.traits} traits${info.mech ? ` · ${info.mech.name}` : ''}` });
  }
  const f = DT.Shop ? DT.Shop.featured() : [];
  if (f.length) out.splice(1, 0, { id: 'store', art: 'store-hero', eyebrow: 'Store · Featured today', title: 'Featured', desc: '', cta: 'Open Store', act: 'store', feat: true, thumb: 'Store' });
  out.push({ id: 'mp', art: 'hero-arena', eyebrow: 'Multiplayer', title: 'Lobbies of 8', desc: 'Create a lobby, send the invite to your friends and fill the empty seats with AI Tamers. Take any AI seat that is still alive mid-game.', cta: 'Find a lobby', act: 'mp', thumb: 'Multiplayer' });
  if (out.length < 7) out.push({ id: 'farm', art: 'hero-file-island', eyebrow: 'Digi-Farm', title: 'Raise your partners', desc: 'Your Little Legends live on a themed island. Feed them, train them in four mini-games and watch them grow.', cta: 'Open Digi-Farm', act: 'farm', thumb: 'Digi-Farm' });
  if (out.length < 7) out.push({ id: 'evo', art: 'hero-digivice-energy', eyebrow: 'Loot · Digivolution', title: 'Digivolve', desc: 'Little Legends earn Digi-Data from every game, level up and digivolve along their own trees up to Ultimate.', cta: 'Open Loot', act: 'loot', thumb: 'Digivolve' });
  return out;
}
let heroTimer = null;
let heroSig = '';
function renderHero() {
  const el = $('homeHero');
  if (!el) return;
  const slides = heroSlides();
  if (!slides.length) { el.innerHTML = ''; return; }
  st.heroIdx = ((st.heroIdx % slides.length) + slides.length) % slides.length;
  const sig = slides.map((s) => s.id).join('|');
  if (sig !== heroSig || !el.querySelector('.hero-arts')) {
    heroSig = sig;
    el.className = 'hero';
    el.innerHTML = `<div class="hero-arts">${slides.map((s) => `<div class="hero-art" data-slide="${s.id}" style="background-image:url('${ART(s.art)}')"></div>`).join('')}</div>
      <div class="hero-shade"></div><div class="hero-renders"></div><div class="hero-prev"></div><div class="hero-txt"></div>
      <div class="hero-thumbs" role="tablist">${slides.map((s, i) => `<button class="hero-thumb" data-hero="${i}" role="tab" style="--art:url('${ART(s.art)}')"><span>${esc(s.thumb)}</span><i></i></button>`).join('')}</div>`;
  }
  const s = slides[st.heroIdx];
  for (const a of el.querySelectorAll('.hero-art')) a.classList.toggle('on', a.dataset.slide === s.id);
  for (const t of el.querySelectorAll('.hero-thumb')) { const on = Number(t.dataset.hero) === st.heroIdx; t.classList.toggle('on', on); t.setAttribute('aria-selected', on); }
  const txt = el.querySelector('.hero-txt');
  const prev = el.querySelector('.hero-prev');
  const rend = el.querySelector('.hero-renders');
  disposePreviews(el);
  prev.innerHTML = '';
  rend.innerHTML = (HERO_RENDERS ? s.renders || [] : []).map((r, i) => `<img class="hero-render r${i}" src="${RENDER_ART(r)}" alt="" onerror="this.remove()">`).join('');
  let desc = s.desc;
  let extra = s.meta ? `<div class="hero-meta">${esc(s.meta)}</div>` : '';
  let cta = `<button class="btn lc-cta" data-hero-act="${esc(s.act)}">${esc(s.cta)}</button>`;
  if (s.feat) {
    const f = DT.Shop.featured();
    const pick = f[Math.floor(Date.now() / 86400000) % f.length];
    const { kind, item } = pick;
    const rar = RARITIES.includes(item.rarity) ? item.rarity : 'common';
    const price = DT.Shop.priceOf(kind, item);
    const owned = C.owns(kind, item.id);
    desc = item.desc || `${KIND_ONE[kind] || kind}, ${rar}.`;
    extra = `<div class="hero-meta"><span class="badge r-${rar} badge-rar">${rar}</span> <span>${KIND_ONE[kind] || kind}</span> ${owned ? '<span class="badge badge-own">Owned</span>' : `<span class="price">${price < item.price ? `<s>${item.price}</s>` : ''}${bit}<b>${price}</b></span><span class="sale">-${Math.round(100 - (100 * price) / Math.max(1, item.price))}%</span>`}</div>`;
    s.title = item.name;
    cta = `<button class="btn lc-cta" data-shop-open="${kind}">Open Store</button>`;
    prev.innerHTML = `<div class="hero-prev-in${kind === 'board' || kind === 'killscreen' ? ' wide' : ''}" data-prev="${kind}|${esc(item.id)}"></div>`;
    fillPreviews(prev);
  }
  txt.innerHTML = `<div class="eyebrow">${esc(s.eyebrow)}</div><h2 class="hero-title">${esc(s.title)}</h2>${extra}${desc ? `<p class="hero-desc">${esc(desc)}</p>` : ''}<div class="hero-act">${cta}</div>`;
  txt.classList.remove('in'); void txt.offsetWidth; txt.classList.add('in');
  clearTimeout(heroTimer);
  heroTimer = setTimeout(() => {
    if (st.page === 'home' && clientOn() && !st.module && !st.result && st.htab === 'overview' && !el.matches(':hover')) { st.heroIdx++; renderHero(); }
    else renderHeroLater();
  }, 8000);
}
function renderHeroLater() { clearTimeout(heroTimer); heroTimer = setTimeout(() => { if (st.page === 'home' && clientOn()) renderHero(); }, 8000); }
$('homeHero').addEventListener('click', (e) => {
  const d = e.target.closest('[data-hero]');
  if (d) { st.heroIdx = Number(d.dataset.hero); sfx('pick', true); renderHero(); return; }
  const a = e.target.closest('[data-hero-act]');
  if (a) heroAct(a.dataset.heroAct);
});
function heroAct(act) {
  sfx('pick');
  if (act.startsWith('set:')) {
    const id = act.slice(4);
    if (DT.SetPicker) DT.SetPicker.select(id);
    setPlayMode('solo');
    st.playStep = 'select';
    return go('play');
  }
  if (act === 'mp') { setPlayMode('mp'); st.playStep = 'party'; st.lobbyFollow = true; return go('play'); }
  if (act === 'farm') return go('farm');
  if (act === 'loot') return go('loot');
}

function missionsHTML(compact) {
  if (!DT.Shop || !DT.Shop.missionInfo) return '';
  const ms = DT.Shop.missionInfo();
  const daily = DT.Shop.daily();
  return `${ms.map((m) => {
    const cur = m.done ? m.goal : bestToday(m);
    const pct = Math.round((100 * cur) / m.goal);
    return `<div class="mission${m.done ? ' done' : ''}">
      <div class="mi-top"><span class="mi-name">${esc(m.name)}</span><span class="mi-bits">${bit}${m.bits}</span></div>
      <div class="progress"><i style="width:${pct}%"></i></div>
      ${compact ? '' : `<div class="mi-sub">${m.done ? 'Complete' : m.goal > 1 ? `Best today ${cur}/${m.goal} · in one game` : 'Complete it in one game'}</div>`}
    </div>`;
  }).join('')}
  <div class="daily${daily.claimed ? ' done' : ''}"><span class="daily-ic">${daily.claimed ? '&#10003;' : '+'}</span>
    <div><b>Daily bonus</b><div class="mi-sub">${daily.claimed ? 'Claimed. Back tomorrow.' : `First game today pays ${daily.bits} extra Bits.`}</div></div>
    <span class="mi-bits">${bit}${daily.bits}</span></div>`;
}
function renderMissions() {
  const el = $('homeMissions');
  if (!el || !DT.Shop || !DT.Shop.missionInfo) return;
  el.innerHTML = `<div class="tile-h"><span>Missions</span><span class="muted">Reset at midnight</span></div><div class="tile-b">${missionsHTML()}</div>`;
}

function renderLegendCard() {
  const el = $('homeLegendInfo');
  if (!el) return;
  const id = C.equipped('legend');
  const it = C.get('legend', id);
  const evo = DT.LegendEvo && DT.LegendEvo.info(id);
  const name = evo ? evo.formInfo.name : it ? it.name : 'Little Legend';
  const rar = it && RARITIES.includes(it.rarity) ? it.rarity : 'common';
  el.innerHTML = `<div class="li-top"><div><div class="eyebrow">Your Little Legend</div><div class="li-name">${esc(name)}</div></div>
      ${evo ? `<span class="lvl">Lv ${evo.level}</span>` : ''}</div>
    ${evo ? `<div class="li-bar"><div class="progress thin"><i style="width:${evo.need ? (100 * evo.data) / evo.need : 100}%"></i></div>
      <span class="muted">${evo.canDigivolve ? '<b class="ready">Ready to digivolve</b>' : evo.need ? `${evo.data}/${evo.need} Digi-Data` : 'Max level'}</span></div>` : `<div class="muted">${esc(rar)}</div>`}
    <div class="li-btns"><button class="btn btn-ghost btn-sm" data-go="collection" data-go-tab="legend">Change</button>
      ${evo ? `<button class="btn btn-sm ${evo.canDigivolve ? 'btn-primary' : 'btn-ghost'}" data-evo-open="${esc(id)}">Digivolve</button>` : ''}
      ${DT.FarmUI ? `<button class="btn btn-ghost btn-sm" data-farm-open="${esc(id)}">Farm</button>` : ''}</div>`;
}

function renderPatch() {
  const el = $('homePatch');
  if (!el || el.dataset.done) return;
  el.dataset.done = '1';
  el.innerHTML = `<div class="tile-art" style="background-image:url('${ART('hero-datastream-sky')}')"><span class="badge badge-new">Patch notes</span></div>
    <div class="tile-h"><span>What's new</span></div>
    <ul class="patch-list">${PATCH.slice(0, 3).map(([h, t]) => `<li><b>${esc(h)}</b><span>${esc(typeof t === 'function' ? t() : t)}</span></li>`).join('')}</ul>
    <button class="tile-link" data-htab-go="patch">Read the patch notes &#8250;</button>`;
}
function renderSetNews() {
  const el = $('homeSetNews');
  if (!el) return;
  const id = selectedSet();
  const info = setInfo(id);
  const a = setArt(id);
  const sig = `${id}`;
  if (el.dataset.sig === sig) return;
  el.dataset.sig = sig;
  const bullets = id === 'chaos' ? [['Any set', 'The seed picks the set. Everyone in the lobby plays the same one.'], ['Pool', setOptions().filter((o) => o.id !== 'chaos').map((o) => o.name).join(', ')]]
    : [[info.mech ? info.mech.name : 'Set mechanic', info.mech ? info.mech.desc : info.desc], ['Roster', `${info.lines} shop Digimon and ${info.traits} traits.`]];
  el.innerHTML = `<div class="tile-art" style="background-image:url('${ART(a.hero)}')"><span class="badge">${esc(a.tag)}</span>${a.renders[0] ? `<img class="tile-render" src="${RENDER_ART(a.renders[0])}" alt="" onerror="this.remove()">` : ''}</div>
    <div class="tile-h"><span>This set: ${esc(info.name)}</span></div>
    <ul class="patch-list">${bullets.map(([h, t]) => `<li><b>${esc(h)}</b><span>${esc(t)}</span></li>`).join('')}</ul>
    <button class="tile-link" data-codex-open>Open the Codex &#8250;</button>`;
}
function renderFeatured() {
  const el = $('homeFeatured');
  if (!el || !DT.Shop) return;
  const f = DT.Shop.featured().slice(0, 3);
  const sig = f.map((x) => `${x.kind}:${x.item.id}:${C.owns(x.kind, x.item.id)}`).join('|');
  if (el.dataset.sig === sig) return;
  el.dataset.sig = sig;
  disposePreviews(el);
  el.innerHTML = `<div class="tile-h"><span>Featured</span><span class="muted">Today only</span></div>
    <div class="feat-list">${f.map(({ kind, item }) => {
      const rar = RARITIES.includes(item.rarity) ? item.rarity : 'common';
      const price = DT.Shop.priceOf(kind, item);
      const owned = C.owns(kind, item.id);
      return `<button class="feat r-${rar}" data-shop-open="${kind}"><span class="feat-prev" data-prev="${kind}|${esc(item.id)}"></span>
        <span class="feat-txt"><b>${esc(item.name)}</b><small>${KIND_ONE[kind] || kind}</small></span>
        <span class="feat-price">${owned ? '<span class="badge badge-own">Owned</span>' : `<span class="sale">-${Math.round(100 - (100 * price) / Math.max(1, item.price))}%</span>${bit}<b>${price}</b>`}</span></button>`;
    }).join('')}</div>`;
  fillPreviews(el);
}
function renderPatchNotes() {
  const el = $('homePatchNotes');
  if (el.dataset.done) return;
  el.dataset.done = '1';
  const sets = setOptions().filter((o) => o.id !== 'chaos').map((o) => ({ ...o, info: setInfo(o.id) }));
  el.innerHTML = `<article class="article">
    <div class="art-banner" style="background-image:url('${ART('hero-digivice-energy')}')"><div><div class="eyebrow">Patch notes</div><h1>The new client</h1><p>A launcher for everything outside the match.</p></div></div>
    <div class="art-body">
      <section><h3>Highlights</h3><ul class="patch-list big">${PATCH.map(([h, t]) => `<li><b>${esc(h)}</b><span>${esc(typeof t === 'function' ? t() : t)}</span></li>`).join('')}</ul></section>
      <section><h3>Sets in the rotation</h3><div class="art-sets">${sets.map((s) => `<div class="art-set" style="--art:url('${ART(setArt(s.id).mode)}')"><b>${esc(s.name)}</b>
        <span>${esc(s.info.desc)}</span><small>${s.info.lines} Digimon · ${s.info.traits} traits${s.info.mech ? ` · ${esc(s.info.mech.name)}` : ''}</small></div>`).join('')}</div></section>
    </div></article>`;
}
function msToMidnight() { const n = new Date(); const m = new Date(n); m.setHours(24, 0, 0, 0); return m - n; }
const hm = (ms) => `${Math.floor(ms / 3600000)}h ${Math.floor((ms % 3600000) / 60000)}m`;
function renderEvents() {
  const el = $('homeEvents');
  const daily = DT.Shop ? DT.Shop.daily() : { claimed: false, bits: 0 };
  el.innerHTML = `<div class="events">
    <div class="ev-banner" style="background-image:url('${ART('events-bg')}'), url('${ART('hero-datastream-sky')}')"><div><div class="eyebrow">Events</div><h1>Daily rotation</h1><p>Missions, the daily bonus and the featured Store row reset in ${hm(msToMidnight())}.</p></div></div>
    <div class="ev-grid">
      <div class="tile-card"><div class="tile-h"><span>Daily missions</span></div><div class="tile-b">${missionsHTML()}</div></div>
      <div class="tile-card"><div class="tile-art" style="background-image:url('${ART('store-hero')}')"><span class="badge badge-new">Store</span></div><div class="tile-h"><span>Featured row</span></div>
        <p class="tile-p">Four items at 25% off, new every day. ${daily.claimed ? 'Your daily bonus is claimed.' : `Your first game today pays ${daily.bits} extra Bits.`}</p><button class="tile-link" data-shop-open>Open the Store &#8250;</button></div>
      <div class="tile-card"><div class="tile-art" style="background-image:url('${ART('hero-file-island')}')"><span class="badge">Digi-Farm</span></div><div class="tile-h"><span>Farm care</span></div>
        <p class="tile-p">Hunger, energy and happiness run on real time, also while the game is closed. Check on your partners.</p><button class="tile-link" data-farm-open>Open the Digi-Farm &#8250;</button></div>
    </div></div>`;
}
overlay.addEventListener('click', (e) => {
  const h = e.target.closest('[data-htab-go]');
  if (h) { st.htab = h.dataset.htabGo; sfx('pick', true); RENDER.home(); syncStage(); }
});

// =====================================================================
// Match loading screen (League-style "loading into the match": all 8 Tamers' cards)
// =====================================================================
let loadEl = null;
let loadedFor = null;
function maybeLoadingScreen() {
  const g = DT.UI && DT.UI.game && DT.UI.game();
  if (!g || g.over || !g.players) return;
  const key = `${g.seed}:${g.human && g.human.id}`;
  if (loadedFor === key) return;
  loadedFor = key;
  showLoading(g, DT.UI.session && DT.UI.session());
}
function showLoading(g, session) {
  if (loadEl) loadEl.remove();
  const setId = g.setId || (DT.SET_ID) || 'faction-wars';
  const info = setInfo(setId);
  const a = setArt(g.chaos ? 'chaos' : setId);
  const cards = g.players.map((p) => {
    let look = null;
    try { look = DT.Social.lookOf(p); } catch (e) { look = null; }
    const k = lookKey(look);
    const pic = k && DT.Legends ? DT.Legends.html(k, 'ld-pt') : `<span class="pt ld-pt"><b>${initials(p.name)}</b></span>`;
    const me = p === g.human;
    return `<div class="ld-card${me ? ' me' : ''}${p.isHuman ? ' human' : ''}"><div class="ld-pic">${pic}</div>
      <div class="ld-name"><b>${esc(p.name)}</b><small>${me ? 'You' : p.isHuman ? 'Tamer' : 'AI Tamer'}</small></div></div>`;
  }).join('');
  loadEl = document.createElement('div');
  loadEl.id = 'cload';
  loadEl.innerHTML = `<div class="ld-bg" style="background-image:url('${ART('loading-bg')}')"></div>
    <div class="ld-head"><div class="eyebrow">${session ? 'Multiplayer' : 'Solo vs AI'} · ${esc(a.tag)} · ${g.pace === 'normal' ? 'Normal' : 'Hyper Roll'}${g.chaosPace ? ' (Chaos pace)' : ''}</div><h1>${esc(g.chaos ? `Chaos: ${info.name}` : info.name)}</h1></div>
    <div class="ld-cards">${cards}</div>
    <div class="ld-foot"><div class="ld-bar"><i></i></div><span>Loading the arena</span></div>`;
  document.body.appendChild(loadEl);
  sfx('round');
  const el = loadEl;
  setTimeout(() => el.classList.add('out'), session ? 1800 : 2100);
  setTimeout(() => { el.remove(); if (loadEl === el) loadEl = null; }, session ? 2300 : 2600);
}

// ---------- results (game over / eliminated): League-style end-of-game screen ----------
const placeLabel = (p, duo = false) => (p === 1 ? 'Champion' : p <= (duo ? 2 : 4) ? (duo ? 'Top 2 teams' : 'Top 4') : `${p}${p === 3 ? 'rd' : 'th'} place`);
const ordinal = (n) => `${n}${n % 10 === 1 && n % 100 !== 11 ? 'st' : n % 10 === 2 && n % 100 !== 12 ? 'nd' : n % 10 === 3 && n % 100 !== 13 ? 'rd' : 'th'}`;

function compHTML(rec, max = 9) {
  const traits = (rec.traits || []).slice(0, 6).map((t) => {
    const T = DT.TRAITS && DT.TRAITS[t.id];
    return T ? `<span class="tchip lv${Math.min(3, t.lv)}" style="--tc:${T.color}" title="${esc(T.name)}"><i></i>${esc(T.name)} <b>${t.n}</b></span>` : '';
  }).join('');
  const units = (rec.units || []).slice(0, max).map((u) => {
    const L = DT.LINE && DT.LINE[u.line];
    return L && DT.Portraits ? DT.Portraits.html(u.line, u.stage, `t${L.tier} up`) : '';
  }).join('');
  return { traits, units };
}

function showResults(o, mode) {
  const { game, session } = o;
  const lvBefore = tamerLevel();
  const rec = recordMatch(game, session);
  const lvAfter = tamerLevel();
  const place = game.human.placement || 1;
  const winner = game.players.find((p) => p.placement === 1);
  const box = $('homeResults');
  disposePreviews(box);
  const c = compHTML(rec || snapshotNow(game) || {});
  const tier = place === 1 ? 'gold' : place <= (game.duo ? 2 : 4) ? 'good' : 'low';
  const title = mode === 'out' ? 'Eliminated' : place === 1 ? 'Victory' : place <= (game.duo ? 2 : 4) ? (game.duo ? 'Top 2 teams' : 'Top 4') : 'Defeat';
  const emblem = place === 1 ? 'emblem-mega' : place <= 2 ? 'emblem-ultimate' : place <= 4 ? 'emblem-champion' : 'emblem-rookie';
  const rows = game.players.slice().sort((a, b) => (a.placement || 0) - (b.placement || 0)).map((p) => {
    let look = null;
    try { look = DT.Social.lookOf(p); } catch (e) { look = null; }
    const k = lookKey(look);
    const me = p === game.human;
    return `<div class="st-row${me ? ' me' : ''}${p.placement ? '' : ' alive'}"><span class="st-place">${p.placement ? `#${p.placement}` : '-'}</span>
      <span class="st-av">${k && DT.Legends ? DT.Legends.html(k) : ''}</span><b>${game.duo ? `Team ${p.team + 1} · ` : ''}${esc(p.name)}</b><span class="muted">${p.placement === 1 ? 'Winner' : p.placement ? 'Eliminated' : `${Math.max(0, p.hp)} HP`}</span></div>`;
  }).join('');
  const xpGain = lvAfter.total - lvBefore.total;
  box.innerHTML = `<div class="results p-${tier}" style="--bg:url('${ART(place === 1 && mode !== 'out' ? 'postgame-bg' : 'loading-bg')}')">
    <button class="btn btn-icon res-x" data-res-close aria-label="Dismiss results" title="Dismiss">&#10005;</button>
    <div class="res-hero">
      <div class="res-place"><img class="res-emblem" src="${ART(emblem)}" alt=""><span class="res-num">#${place}</span><span class="res-lbl">${placeLabel(place, game.duo)}</span></div>
      <div class="res-body">
        <div class="eyebrow">${session ? 'Multiplayer' : 'Solo vs AI'} · ${esc(setName(game.setId || DT.SET_ID))} · results</div>
        <h2 class="res-title">${title}</h2>
        <p class="muted">${mode === 'out' ? 'The others are still playing. Spectate their fights or leave.'
          : `Reached round ${esc(game.roundLabel)} at level ${game.human.level}.${winner && winner !== game.human ? ` Winner: ${esc(winner.name)}.` : ''}`}</p>
        ${c.traits ? `<div class="res-traits">${c.traits}</div>` : ''}
        ${c.units ? `<div class="res-units">${c.units}</div>` : ''}
      </div>
    </div>
    <div class="res-cols">
      <div class="res-col"><div class="tile-h"><span>Standings</span></div><div class="standings">${rows}</div></div>
      <div class="res-col"><div class="tile-h"><span>Progress</span></div>
        <div class="res-xp"><div class="li-top"><span>Tamer level ${lvAfter.level}${lvAfter.level > lvBefore.level ? ' <b class="ready">Level up!</b>' : ''}</span><span class="muted">${xpGain ? `+${xpGain} XP` : ''}</span></div>
          <div class="progress"><i style="width:${(100 * lvAfter.xp) / lvAfter.need}%"></i></div></div>
        <div class="res-missions">${missionsHTML(true)}</div></div>
      <div class="res-col res-award"></div>
    </div>
    <div class="res-btns">
      <button class="btn btn-ghost" data-go="profile">Match history</button>
      ${mode === 'out' ? '<button id="leaveBtn" class="btn btn-secondary">Leave</button><button id="watchBtn" class="btn lc-cta">Spectate</button>'
        : `<button class="btn btn-secondary" data-res-close>Home</button><button id="againBtn" class="btn lc-cta">${session ? 'Back to lobbies' : 'Play again'}</button>`}
    </div>
  </div>`;
  if (mode === 'out') {
    $('watchBtn').onclick = () => { st.result = null; o.watch(); };
    $('leaveBtn').onclick = () => { st.result = null; if (session) st.lobbyFollow = true; o.leave(); };
  } else {
    $('againBtn').onclick = () => { st.result = null; if (session) st.lobbyFollow = true; o.again(); };
  }
  let award = null;
  if (DT.Shop) award = DT.Shop.showAward(box.querySelector('.res-award'), game, session); // Bits breakdown (js/shop.js)
  if (rec && award && rec.bits == null) { rec.bits = award.total; persist(); }
  st.result = { mode };
  overlay.classList.remove('hidden');
  syncClient();
  closeModules();
  st.hist = [];
  go('home', { push: false });
  refreshChrome();
}

// =====================================================================
// Match history and stats (recorded at game over, from a snapshot of your board taken while you were alive)
// =====================================================================
let snap = { game: null, data: null };
function snapshotNow(g) {
  try {
    const p = g.human;
    const units = p.boardUnits();
    if (!units.length) return null;
    const counts = DT.countTraits(units, p.augs);
    const traits = Object.keys(counts).map((id) => ({ id, n: counts[id], lv: DT.traitLevel(id, counts[id]) })).filter((t) => t.lv > 0)
      .sort((a, b) => b.lv - a.lv || b.n - a.n);
    return {
      traits,
      units: units.map((u) => ({ line: u.line, stage: u.stage, items: (u.items || []).slice() }))
        .sort((a, b) => (DT.LINE[b.line] ? DT.LINE[b.line].tier : 0) + b.stage * 2 - ((DT.LINE[a.line] ? DT.LINE[a.line].tier : 0) + a.stage * 2)),
      level: p.level,
    };
  } catch (e) { return null; }
}
setInterval(() => {
  const g = DT.UI && DT.UI.game && DT.UI.game();
  if (!g || !g.human || g.over || !g.human.alive) return;
  const s = snapshotNow(g);
  if (s) snap = { game: g, data: s };
}, 700);

const matchKey = (g, session) => `${session ? 'mp' : 'solo'}:${g.seed}:${g.human.id}`;
function recordMatch(g, session) {
  if (!g || !g.human || !g.human.placement) return null;
  const key = matchKey(g, session);
  const old = save.matches.find((m) => m.key === key);
  if (old) return old;
  const s = (snap.game === g && snap.data) || snapshotNow(g) || { traits: [], units: [], level: g.human.level };
  let stats = null;
  try { const t = DT.Shop && DT.Shop._track(g); if (t) stats = { pvpWins: t.pvpWins, kills: t.kills, maxStars: t.maxStars, maxLevel: t.maxLevel, place: g.human.placement }; } catch (e) { /* no tracker */ }
  const rec = { key, at: Date.now(), mode: session ? 'mp' : 'solo', format: g.format || 'solo', set: g.setId || null, place: g.human.placement, round: g.roundLabel, level: Math.max(s.level || 0, g.human.level || 0),
    traits: s.traits.slice(0, 8), units: s.units.slice(0, 10), stats, bits: null };
  save.matches = [rec, ...save.matches].slice(0, 30);
  const T = save.totals;
  T.games++;
  if (rec.place <= (rec.format === 'duos' ? 2 : 4)) T.top4++;
  if (rec.place === 1) T.wins++;
  T.placeSum += rec.place;
  for (const t of rec.traits) T.traits[t.id] = (T.traits[t.id] || 0) + 1;
  const lines = new Set(rec.units.map((u) => u.line));
  for (const l of lines) T.units[l] = (T.units[l] || 0) + 1;
  if (session) rememberPlayers(g.players.filter((p) => p.isHuman && p !== g.human).map((p) => ({ name: p.name })));
  persist();
  return rec;
}
function stats() {
  const T = save.totals;
  const top = (o, n) => Object.entries(o).sort((a, b) => b[1] - a[1]).slice(0, n);
  return { games: T.games, wins: T.wins, top4: T.top4, avg: T.games ? T.placeSum / T.games : 0, traits: top(T.traits, 3), units: top(T.units, 5) };
}

// =====================================================================
// Play: mode select -> party (solo) or lobby (multiplayer)
// =====================================================================
function setPlayMode(mode) {
  st.playMode = mode;
  for (const b of document.querySelectorAll('#pgPlay [data-mode-pick]')) {
    const on = b.dataset.modePick === mode;
    b.classList.toggle('on', on);
    b.setAttribute('aria-selected', on ? 'true' : 'false');
  }
  $('playSolo').classList.toggle('hidden', mode !== 'solo');
  $('playMp').classList.toggle('hidden', mode !== 'mp');
  // multiplayer party: connect and list the lobbies (net.js renders into #lobbyPanel)
  if (mode === 'mp' && st.playStep === 'party' && !$('lobbyPanel').innerHTML.trim() && DT.Net) DT.Net.open();
  renderPlayPick();
}
function setPlayStep(step) {
  if (step === 'select' && st.playMode === 'mp' && inRoom()) step = 'party'; // a lobby stays up until you leave it
  st.playStep = step;
  $('playSelect').classList.toggle('hidden', step !== 'select');
  $('playParty').classList.toggle('hidden', step !== 'party');
  if (step === 'party') {
    if (st.playMode === 'mp' && !$('lobbyPanel').innerHTML.trim() && DT.Net) DT.Net.open();
    if (st.playMode === 'solo') renderParty();
  }
  syncStage();
}
function renderPlayPick() {
  const el = $('playPick');
  if (!el) return;
  const id = selectedSet();
  const pace = DT.SetPicker && DT.SetPicker.pace ? DT.SetPicker.paceLabel(DT.SetPicker.pace()) : 'Hyper Roll';
  el.innerHTML = `<span class="muted">Selected</span><b>${esc(setName(id))}</b><span class="muted">·</span><b>${esc(pace)}</b><b>${DT.SetPicker.format() === 'duos' ? 'Duos' : 'Free for all'}</b><span class="muted">·</span><b>${st.playMode === 'mp' ? 'Multiplayer' : 'Solo vs AI'}</b>`;
}
function renderParty() {
  const id = selectedSet();
  const info = setInfo(id);
  $('partyTitle').textContent = id === 'chaos' ? 'Chaos' : info.name;
  $('partySub').textContent = id === 'chaos' ? 'The game seed picks the set.' : info.desc;
  $('playSolo').style.setProperty('--party-art', `url('${ART('lobby-bg')}')`);
  const host = $('partySlots');
  const names = (DT.TAMERS || DT.DEFAULT_TAMERS || ['Tai', 'Matt', 'Sora', 'Izzy', 'Mimi', 'Joe', 'Kari']).slice(0, 7);
  const legends = C.list('legend').filter(it => !it.secret);
  const hash = (s) => { let h = 2166136261; for (let i = 0; i < s.length; i++) { h ^= s.charCodeAt(i); h = Math.imul(h, 16777619) >>> 0; } return h; };
  const ev = DT.LegendEvo ? DT.LegendEvo.info(C.equipped('legend')) : null;
  const meName = myName() || 'You';
  const mine = `<div class="party-slot me"><div class="party-stage stage-host"></div><div class="ps-plate"><b>${esc(meName)}</b><small>${ev ? `${esc(ev.formInfo.name)} · Lv ${ev.level}` : 'Little Legend'}</small></div><span class="ps-crown" title="Party leader"></span></div>`;
  const ai = names.map((n, i) => {
    const L = legends.length ? legends[hash(`party:${n}:${i}`) % legends.length] : null;
    return `<div class="party-slot ai"><div class="ps-pic">${L && DT.Legends ? DT.Legends.html(L.id) : ''}</div><div class="ps-plate"><b>${esc(n)}</b><small>AI Tamer</small></div></div>`;
  }).join('');
  const sig = `${meName}|${C.equipped('legend')}|${ev ? ev.level : ''}`;
  if (host.dataset.sig !== sig) { host.dataset.sig = sig; host.innerHTML = mine + ai; if (stage) stage.host = null; }
}
RENDER.play = () => {
  if (st.lobbyFollow && st.playMode === 'mp') st.playStep = 'party';
  setPlayStep(st.playStep);
  setPlayMode(st.playMode);
  if (DT.SetPicker) DT.SetPicker.render();
  const s = $('playStatus');
  const n = save.totals.games;
  const R = tamerRank();
  if (s) s.innerHTML = `<img class="ps-emblem${R.unranked ? ' dim' : ''}" src="${ART(R.emblem)}" alt=""><div><div class="eyebrow">${esc(R.label)}</div><div class="ps-row"><b>${n}</b><span>games</span><b>${save.totals.top4}</b><span>top finishes</span><b>${save.totals.wins}</b><span>wins</span></div></div>`;
};
$('pgPlay').addEventListener('click', (e) => {
  const m = e.target.closest('[data-mode-pick]');
  if (m) { sfx('pick', true); st.lobbyFollow = false; return setPlayMode(m.dataset.modePick); }
  if (e.target.closest('#playConfirm')) {
    sfx('levelup');
    st.lobbyFollow = st.playMode === 'mp';
    if (st.playMode === 'mp' && DT.SetPicker) { /* the lobby form starts on the picked set */ }
    return setPlayStep('party');
  }
  if (e.target.closest('[data-play-back]')) { sfx('pick', true); return setPlayStep('select'); }
  if (e.target.closest('[data-set]') || e.target.closest('[data-pace]') || e.target.closest('[data-format]')) { renderPlayPick(); }
  if (e.target.closest('#playSoloBtn') && DT.UI && DT.UI.newGame && !st.mpLocked) { sfx('fight'); DT.UI.newGame(); }
});
$('pgPlay').addEventListener('dblclick', (e) => {
  if (e.target.closest('#setPickSolo [data-set]')) { sfx('levelup'); setPlayStep('party'); }
});

// =====================================================================
// Collection
// =====================================================================
const col = { tab: 'legend', q: '', own: 'all', rar: 'all', sort: 'rarity', sel: {}, slot: 0 };
function isEquipped(kind, id) { const e = C.equipped(kind); return Array.isArray(e) ? e.includes(id) : e === id; }
const EGG_ONLY = (it) => !!(it && it.eggOnly && !it.free);

function colItems(kind) {
  const q = col.q.trim().toLowerCase();
  let items = C.visible(kind).filter((it) => {
    const owned = C.owns(kind, it.id);
    if (col.own === 'owned' && !owned) return false;
    if (col.own === 'locked' && owned) return false;
    const r = RARITIES.includes(it.rarity) ? it.rarity : 'common';
    if (col.rar !== 'all' && r !== col.rar) return false;
    if (q && !`${it.name} ${it.desc || ''} ${r} ${it.tag || ''}`.toLowerCase().includes(q)) return false;
    return true;
  });
  const rank = (it) => RARITIES.indexOf(RARITIES.includes(it.rarity) ? it.rarity : 'common');
  if (col.sort === 'name') items.sort((a, b) => a.name.localeCompare(b.name));
  else if (col.sort === 'owned') items.sort((a, b) => (C.owns(kind, b.id) ? 1 : 0) - (C.owns(kind, a.id) ? 1 : 0) || rank(b) - rank(a) || a.name.localeCompare(b.name));
  else if (col.sort === 'rarity-asc') items.sort((a, b) => rank(a) - rank(b) || a.name.localeCompare(b.name));
  else items.sort((a, b) => rank(b) - rank(a) || a.name.localeCompare(b.name));
  return items;
}

RENDER.collection = () => {
  const root = $('pgCollection');
  const kind = col.tab;
  const all = C.visible(kind);
  const items = colItems(kind);
  if (!col.sel[kind] || !C.get(kind, col.sel[kind])) col.sel[kind] = (MULTI(kind) ? C.equipped(kind).find(Boolean) : C.equipped(kind)) || (all[0] && all[0].id);
  const grid = root.querySelector('.cgrid');
  const top = grid ? grid.scrollTop : 0;
  const focusSearch = document.activeElement && document.activeElement.id === 'colSearch';
  const caret = focusSearch ? document.activeElement.selectionStart : 0;
  const ownK = all.filter((it) => C.owns(kind, it.id)).length;
  disposePreviews(root);
  root.innerHTML = `<div class="coll">
    <div class="coll-main">
      <div class="coll-banner" style="background-image:url('${ART(COLL_ART[kind] || 'coll-legends')}')">
        <div><div class="eyebrow">Collection</div><h1 class="page-title">${esc(KINDS.find((x) => x[0] === kind)[1])}</h1></div>
        <div class="coll-count"><b>${ownK}</b> / ${all.length} owned<small>${KINDS.reduce((n, [k]) => n + C.visible(k).filter((it) => C.owns(k, it.id)).length, 0)} / ${KINDS.reduce((n, [k]) => n + C.visible(k).length, 0)} in total</small>
          <div class="progress thin"><i style="width:${all.length ? (100 * ownK) / all.length : 0}%"></i></div></div></div>
      <div class="ctabs" role="tablist">${KINDS.map(([k, many]) => {
        const own = C.visible(k).filter((it) => C.owns(k, it.id)).length;
        return `<button class="ctab${k === kind ? ' on' : ''}" data-ctab="${k}" role="tab" aria-selected="${k === kind}">${many} <span>${own}/${C.visible(k).length}</span></button>`;
      }).join('')}</div>
      <div class="ctools">
        ${kind === "emote" ? '<button class="btn btn-secondary" data-retro-emotes>Digimon World 1</button>' : ""}
        <input id="colSearch" class="input search" type="search" placeholder="Search ${esc(KINDS.find((x) => x[0] === kind)[1].toLowerCase())}" value="${esc(col.q)}" aria-label="Search" spellcheck="false">
        <div class="seg" role="group" aria-label="Show">${[['all', 'All'], ['owned', 'Owned'], ['locked', 'Not owned']].map(([v, l]) => `<button class="${col.own === v ? 'on' : ''}" data-cown="${v}">${l}</button>`).join('')}</div>
        <select id="colRar" class="input" aria-label="Rarity"><option value="all">All rarities</option>${RARITIES.map((r) => `<option value="${r}" ${col.rar === r ? 'selected' : ''}>${r[0].toUpperCase()}${r.slice(1)}</option>`).join('')}</select>
        <select id="colSort" class="input" aria-label="Sort">${[['rarity', 'Rarity: high first'], ['rarity-asc', 'Rarity: low first'], ['name', 'Name'], ['owned', 'Owned first']].map(([v, l]) => `<option value="${v}" ${col.sort === v ? 'selected' : ''}>${l}</option>`).join('')}</select>
      </div>
      <div class="cgrid">${items.length ? items.map((it) => {
        const owned = C.owns(kind, it.id);
        const eq = owned && isEquipped(kind, it.id);
        const rar = RARITIES.includes(it.rarity) ? it.rarity : 'common';
        return `<button class="ccard r-${rar}${owned ? ' own' : ' locked'}${eq ? ' eq' : ''}${col.sel[kind] === it.id ? ' sel' : ''}" data-csel="${esc(it.id)}" title="${esc(it.name)}">
          <span class="ccard-prev" data-prev="${kind}|${esc(it.id)}"></span>
          <span class="ccard-name">${esc(it.name)}</span>
          <span class="ccard-meta"><i></i>${rar}${it.tag ? ` · ${esc(it.tag)}` : ''}</span>
          ${eq ? '<span class="badge badge-eq">Equipped</span>' : !owned ? `<span class="badge badge-lock">${EGG_ONLY(it) ? 'Egg only' : 'Locked'}</span>` : ''}
        </button>`;
      }).join('') : '<div class="empty">Nothing matches these filters.</div>'}</div>
    </div>
    <aside class="cprev">${previewPane(kind, C.get(kind, col.sel[kind]))}</aside>
  </div>`;
  fillPreviews(root.querySelector('.cgrid'));
  const big = root.querySelector('.cprev-big[data-prev]');
  if (big) fillPreviews(root.querySelector('.cprev'));
  root.querySelector('.cgrid').scrollTop = top;
  if (focusSearch) { const s = $('colSearch'); s.focus(); try { s.setSelectionRange(caret, caret); } catch (e) { /* ignore */ } }
  syncStage();
};

function previewPane(kind, it) {
  if (!it) return '<div class="empty">Nothing here yet.</div>';
  const owned = C.owns(kind, it.id);
  const eq = owned && isEquipped(kind, it.id);
  const rar = RARITIES.includes(it.rarity) ? it.rarity : 'common';
  const evo = kind === 'legend' && owned && DT.LegendEvo ? DT.LegendEvo.info(it.id) : null;
  let acts = '';
  if (owned && MULTI(kind)) {
    const wheel = C.equipped(kind);
    acts = `<div class="slot-h">Wheel slots <span class="muted">pick a slot for ${esc(it.name)}</span></div>
      <div class="slots">${wheel.map((id, i) => { const w = id ? C.get(kind, id) : null; return `<button class="slot${id === it.id ? ' cur' : ''}" data-cslot="${i}" title="Put it in slot ${i + 1}"><b>${i + 1}</b><span>${w ? esc(w.name) : 'Empty'}</span></button>`; }).join('')}</div>`;
  } else if (owned) {
    acts = eq ? '<button class="btn btn-secondary btn-block" disabled>Equipped</button>' : `<button class="btn lc-cta btn-block" data-cequip="${esc(it.id)}">Equip</button>`;
  } else if (it.linked) {
    const src = C.get(it.linked[0], it.linked[1]);
    const price = src ? (DT.Shop ? DT.Shop.priceOf(it.linked[0], src) : src.price) : 0;
    acts = `<button class="btn lc-cta btn-block" data-shop-open="${esc(it.linked[0])}">Get the ${esc((KIND_ONE[it.linked[0]] || '').toLowerCase())} clip · ${bit}${price}</button><p class="muted small">Owning it on the soundboard unlocks it here too.</p>`;
  } else if (EGG_ONLY(it)) {
    acts = '<button class="btn lc-cta btn-block" data-go="loot">Hatch from Digi-Eggs</button><p class="muted small">Rare and up Little Legends only come from eggs.</p>';
  } else {
    const price = DT.Shop ? DT.Shop.priceOf(kind, it) : it.price;
    acts = `<button class="btn lc-cta btn-block" data-shop-open="${kind}">Get it in the Store · ${bit}${price}</button>`;
  }
  if (evo) {
    acts += `<div class="evo-box"><div class="li-top"><span>${esc(evo.formInfo.name)} <span class="muted">${esc(evo.formInfo.level)}</span></span><span class="lvl">Lv ${evo.level}</span></div>
      <div class="progress thin"><i style="width:${evo.need ? (100 * evo.data) / evo.need : 100}%"></i></div>
      <div class="muted small">${evo.canDigivolve ? '<b class="ready">Ready to digivolve</b>' : evo.next ? `Digivolves at level ${evo.next}` : 'Final form reached'} · ${evo.need ? `${evo.data}/${evo.need} Digi-Data` : 'Max level'}</div>
      <div class="li-btns"><button class="btn btn-sm ${evo.canDigivolve ? 'btn-primary' : 'btn-secondary'}" data-evo-open="${esc(it.id)}">Digivolve</button>${DT.FarmUI ? `<button class="btn btn-sm btn-ghost" data-farm-open="${esc(it.id)}">Digi-Farm</button>` : ''}</div></div>`;
  }
  const stageBox = kind === 'legend' ? '<div class="cprev-stage stage-host"></div>' : `<div class="cprev-big" data-prev="${kind}|${esc(it.id)}"></div>`;
  return `<div class="cprev-card r-${rar}">
    ${stageBox}
    <div class="cprev-info">
      <div class="cprev-top"><span class="badge badge-rar r-${rar}">${rar}</span>${it.tag ? `<span class="badge">${esc(it.tag)}</span>` : ''}<span class="muted">${KIND_ONE[kind]}</span>${eq ? '<span class="badge badge-eq">Equipped</span>' : owned ? '<span class="badge badge-own">Owned</span>' : ''}</div>
      <h2 class="cprev-name">${esc(it.name)}</h2>
      ${it.desc ? `<p class="cprev-desc">${esc(it.desc)}</p>` : ''}
      <div class="cprev-acts">${acts}</div>
    </div>
  </div>`;
}

$('pgCollection').addEventListener('click', (e) => {
  const t = e.target.closest('button');
  if (!t) return;
  const d = t.dataset;
  const kind = col.tab;
  if ('retroEmotes' in d) { col.q = 'Digimon World 1'; col.own = 'all'; col.rar = 'all'; col.sel.emote = 'retro_hug'; return RENDER.collection(); }
  if (d.ctab) { col.tab = d.ctab; col.q = ''; sfx('pick', true); return RENDER.collection(); }
  if (d.cown) { col.own = d.cown; return RENDER.collection(); }
  if (d.csel) { col.sel[kind] = d.csel; sfx('place', true); if (stage && kind === 'legend') stage.setLegend(legendKey(d.csel)); return RENDER.collection(); }
  if (d.cequip) { if (C.equip(kind, d.cequip)) { sfx('equip'); toast(`Equipped ${C.get(kind, d.cequip).name}`, 'good'); } return; }
  if (d.cslot != null) {
    const id = col.sel[kind];
    if (id && C.equip(kind, id, Number(d.cslot))) { sfx('equip'); toast(`${C.get(kind, id).name} is in slot ${Number(d.cslot) + 1}`, 'good'); }
  }
});
$('pgCollection').addEventListener('input', (e) => {
  if (e.target.id !== 'colSearch') return;
  col.q = e.target.value;
  clearTimeout(col.qt);
  col.qt = setTimeout(RENDER.collection, 140);
});
$('pgCollection').addEventListener('change', (e) => {
  if (e.target.id === 'colRar') { col.rar = e.target.value; RENDER.collection(); }
  if (e.target.id === 'colSort') { col.sort = e.target.value; RENDER.collection(); }
});

// =====================================================================
// Loot: Digi-Eggs and Little Legend digivolution, laid out like a crafting workshop
// =====================================================================
const EGG_HUE = { mixed: 0, legend: 180, board: 95, killscreen: 320, dance: 260, emote: 40, sound: 210, elimsound: 345 };
function lootItems() {
  const eggs = DT.Shop && DT.Shop.eggs ? DT.Shop.eggs() : [];
  const legends = DT.LegendEvo ? C.list('legend').filter((x) => C.owns('legend', x.id)).map((x) => ({ it: x, evo: DT.LegendEvo.info(x.id) })) : [];
  legends.sort((a, b) => (b.evo && b.evo.canDigivolve ? 1 : 0) - (a.evo && a.evo.canDigivolve ? 1 : 0) || (b.evo ? b.evo.level : 0) - (a.evo ? a.evo.level : 0));
  return { eggs, legends };
}
RENDER.loot = () => {
  const root = $('pgLoot');
  const { eggs, legends } = lootItems();
  const boxes = DT.MysteryLoot?.inventory() || { daily: 0, vault: 0, keys: 0 };
  if (!st.lootSel || (st.lootSel.t === 'egg' && !eggs.some((e) => e.id === st.lootSel.id)) || (st.lootSel.t === 'legend' && !legends.some((l) => l.it.id === st.lootSel.id))) {
    const ready = legends.find((l) => l.evo && l.evo.canDigivolve);
    st.lootSel = ready ? { t: 'legend', id: ready.it.id } : eggs[0] ? { t: 'egg', id: eggs[0].id } : null;
  }
  const sel = st.lootSel;
  const ready = legends.filter((l) => l.evo && l.evo.canDigivolve).length;
  const dataTotal = legends.reduce((n, l) => n + (l.evo ? l.evo.data || 0 : 0), 0);
  let altar = '<div class="empty">Nothing to craft yet.</div>';
  if (sel && sel.t === 'egg') {
    const e = eggs.find((x) => x.id === sel.id);
    const odds = (e.odds || []).filter((o) => o.pct > 0 || o.n).map((o) => `<span class="odd r-${esc(o.rarity || o.r || 'common')}"><i></i>${esc(o.rarity || o.r || '')} ${o.pct != null ? `${Math.round(o.pct * 10) / 10}%` : ''}</span>`).join('');
    altar = `<div class="altar-item"><img class="altar-egg" src="${ART('loot-egg')}" style="--hue:${EGG_HUE[e.id] || 0}deg" alt=""></div>
      <div class="altar-info"><div class="eyebrow">Digi-Egg</div><h2>${esc(e.name)}</h2><p>${esc(e.desc)}</p>${odds ? `<div class="odds">${odds}</div>` : ''}
        <div class="altar-acts"><span class="price">${bit}<b>${e.price}</b></span><button class="btn lc-cta" data-loot-hatch="${esc(e.id)}" ${W.bits < e.price ? 'disabled title="Not enough Bits"' : ''}>Hatch</button></div></div>`;
  } else if (sel && sel.t === 'legend') {
    const l = legends.find((x) => x.it.id === sel.id);
    const ev = l.evo;
    altar = `<div class="altar-item legend">${DT.Legends ? DT.Legends.html(legendKey(l.it.id), 'altar-pt') : ''}</div>
      <div class="altar-info"><div class="eyebrow">Little Legend · ${ev ? esc(ev.formInfo.level) : ''}</div><h2>${esc(ev ? ev.formInfo.name : l.it.name)}</h2>
        ${ev ? `<div class="li-top"><span class="muted">${ev.need ? `${ev.data}/${ev.need} Digi-Data` : 'Max level'}</span><span class="lvl">Lv ${ev.level}</span></div>
        <div class="progress"><i style="width:${ev.need ? (100 * ev.data) / ev.need : 100}%"></i></div>
        <p>${ev.canDigivolve ? 'Ready to digivolve. Pick its next form in the Digivolve panel.' : ev.next ? `Digivolves at level ${ev.next}. Digi-Data comes from games played with it equipped, and from the Digi-Farm.` : 'Final form reached.'}</p>` : ''}
        <div class="altar-acts"><button class="btn ${ev && ev.canDigivolve ? 'lc-cta' : 'btn-secondary'}" data-evo-open="${esc(l.it.id)}">Digivolve</button>
          ${DT.FarmUI ? `<button class="btn btn-ghost" data-farm-open="${esc(l.it.id)}">Digi-Farm</button>` : ''}
          ${C.equipped('legend') === l.it.id ? '<span class="badge badge-eq">Equipped</span>' : `<button class="btn btn-ghost" data-loot-equip="${esc(l.it.id)}">Equip</button>`}</div></div>`;
  }
  root.innerHTML = `<div class="loot">
    <div class="loot-stage" style="background-image:url('${ART('loot-bg')}')">
      <div class="loot-head"><div><div class="eyebrow">Loot</div><h1 class="page-title">Incubator</h1></div>
        <div class="loot-mats"><span class="mat"><img src="${ART('loot-shard')}" alt=""><b>${dataTotal}</b><small>Digi-Data · Legend XP</small></span>
          <span class="mat"><img src="${ART('loot-chest')}" alt=""><b>${ready}</b><small>Ready to digivolve</small></span>
          <span class="mat bits">${bit}<b>${W.bits.toLocaleString()}</b><small>Bits</small></span></div></div>
      <div class="altar">${altar}</div><p class="loot-data-help">Digi-Data is XP for each Little Legend. Earn it through matches, duplicate legend hatches and Digi-Farm care to level up and unlock digivolution. Bits are your spending currency.</p>
    </div>
    <aside class="loot-inv">
      <section class="mystery-boxes"><div class="tile-h">Mystery boxes</div><p>Exclusive surprises. Rewards appear in Collection only after you discover them.</p>
        <article><span class="mystery-symbol">☀</span><div><b>Daybreak Box × ${boxes.daily}</b><small>First completed round each day.</small></div><button class="btn" data-mystery-open="daily" ${boxes.daily ? "" : "disabled"}>Open</button></article>
        <article><span class="mystery-symbol">◇</span><div><b>Cipher Box × ${boxes.vault}</b><small>Needs 1 Cipher Key. You have ${boxes.keys}.</small></div><button class="btn" data-mystery-open="vault" ${boxes.vault && boxes.keys ? "" : "disabled"}>Unlock</button></article>
        <p>Boxes and keys can drop after rounds from stage 2 onward. No purchases required. Each box reveals one new cosmetic; the two pools never overlap.</p>
      </section>
      <div class="tile-h"><span>Digi-Eggs</span><span class="muted">${eggs.length}</span></div>
      <div class="loot-grid">${eggs.map((e) => `<button class="loot-item egg${sel && sel.t === 'egg' && sel.id === e.id ? ' sel' : ''}" data-loot-egg="${esc(e.id)}" title="${esc(e.name)}">
        <img src="${ART('loot-egg')}" style="--hue:${EGG_HUE[e.id] || 0}deg" alt=""><span>${esc(e.name.replace(' Egg', '').replace('Digi-', ''))}</span><small>${bit}${e.price}</small></button>`).join('')}</div>
      <div class="tile-h"><span>Little Legends</span><span class="muted">${legends.length}</span></div>
      <div class="loot-grid legends">${legends.map((l) => `<button class="loot-item${l.evo && l.evo.canDigivolve ? ' ready' : ''}${sel && sel.t === 'legend' && sel.id === l.it.id ? ' sel' : ''}" data-loot-legend="${esc(l.it.id)}" title="${esc(l.it.name)}">
        ${DT.Legends ? DT.Legends.html(legendKey(l.it.id)) : ''}<span>${esc(l.evo ? l.evo.formInfo.name : l.it.name)}</span><small>Lv ${l.evo ? l.evo.level : 1}</small></button>`).join('')}</div>
    </aside>
  </div>`;
};
$('pgLoot').addEventListener('click', (e) => {
  const t = e.target.closest('button');
  if (!t) return;
  const d = t.dataset;
  if (d.mysteryOpen) { const reward = DT.MysteryLoot.open(d.mysteryOpen); RENDER.loot(); DT.MysteryLoot.reveal(reward); return; }
  if (d.lootEgg) { st.lootSel = { t: 'egg', id: d.lootEgg }; sfx('place', true); return RENDER.loot(); }
  if (d.lootLegend) { st.lootSel = { t: 'legend', id: d.lootLegend }; sfx('place', true); return RENDER.loot(); }
  if (d.lootEquip) { if (C.equip('legend', d.lootEquip)) { sfx('equip'); toast(`Equipped ${C.get('legend', d.lootEquip).name}`, 'good'); RENDER.loot(); } return; }
  if (d.lootHatch && DT.Shop) {
    sfx('forge');
    DT.Shop.open('eggs');
    setTimeout(() => { const b = document.querySelector(`#dtStore [data-egg="${CSS.escape(d.lootHatch)}"]`); if (b) b.click(); }, 60);
  }
});
if (DT.LegendEvo && DT.LegendEvo.onChange) DT.LegendEvo.onChange(() => queueRender());

// =====================================================================
// Profile
// =====================================================================
function placementChart() {
  const counts = Array.from({ length: 8 }, () => 0);
  for (const m of save.matches) if (m.place >= 1 && m.place <= 8) counts[m.place - 1]++;
  const max = Math.max(1, ...counts);
  const W0 = 320, H0 = 120, bw = 30, gap = (W0 - bw * 8) / 7;
  const bars = counts.map((n, i) => {
    const h = n ? Math.max(4, ((H0 - 34) * n) / max) : 0;
    const x = i * (bw + gap);
    const y = H0 - 18 - h;
    const fill = i === 0 ? 'var(--lc-gold-2)' : i < 4 ? 'var(--c-accent)' : 'var(--c-text-3)';
    return `<g class="ch-bar"><title>${ordinal(i + 1)}: ${n} game${n === 1 ? '' : 's'}</title>${h ? `<path d="M${x} ${H0 - 18}V${y + 4}q0-4 4-4h${bw - 8}q4 0 4 4V${H0 - 18}Z" fill="${fill}"/>` : ''}
      <rect x="${x - gap / 2}" y="0" width="${bw + gap}" height="${H0}" fill="transparent"/>${n ? `<text x="${x + bw / 2}" y="${y - 4}" class="ch-v">${n}</text>` : ''}<text x="${x + bw / 2}" y="${H0 - 4}" class="ch-x">${i + 1}</text></g>`;
  }).join('');
  return `<svg class="chart" viewBox="0 0 ${W0} ${H0}" role="img" aria-label="Placements in your last ${save.matches.length} games"><line x1="0" x2="${W0}" y1="${H0 - 18}" y2="${H0 - 18}" class="ch-base"/>${bars}</svg>`;
}
function trendChart() {
  const ms = save.matches.slice(0, 20).reverse();
  if (ms.length < 2) return '<div class="empty small">Play two games to see your trend.</div>';
  const W0 = 320, H0 = 120, pad = 14;
  const x = (i) => pad + ((W0 - pad * 2) * i) / Math.max(1, ms.length - 1);
  const y = (p) => 8 + ((H0 - 26) * (p - 1)) / 7;
  const pts = ms.map((m, i) => `${x(i).toFixed(1)},${y(m.place).toFixed(1)}`).join(' ');
  const dots = ms.map((m, i) => `<g><title>Game ${i + 1}: ${ordinal(m.place)} (${m.mode === 'mp' ? 'multiplayer' : 'solo'})</title><circle cx="${x(i).toFixed(1)}" cy="${y(m.place).toFixed(1)}" r="4" class="ch-dot${m.place <= (m.format === 'duos' ? 2 : 4) ? ' good' : ''}"/><circle cx="${x(i).toFixed(1)}" cy="${y(m.place).toFixed(1)}" r="10" fill="transparent"/></g>`).join('');
  return `<svg class="chart" viewBox="0 0 ${W0} ${H0}" role="img" aria-label="Placement in your last ${ms.length} games, oldest first">
    <line x1="0" x2="${W0}" y1="${y(4.5)}" y2="${y(4.5)}" class="ch-mid"/><text x="${W0 - 2}" y="${y(4.5) - 4}" class="ch-x" text-anchor="end">top 4</text>
    <text x="2" y="${y(1) + 4}" class="ch-x" text-anchor="start">1st</text><text x="2" y="${y(8)}" class="ch-x" text-anchor="start">8th</text>
    <polyline points="${pts}" class="ch-line"/>${dots}</svg>`;
}
RENDER.profile = () => {
  const root = $('pgProfile');
  const s = stats();
  const name = myName();
  const L = tamerLevel();
  const R = tamerRank();
  const banner = PROFILE_BANNERS.includes(save.profile.banner) ? save.profile.banner : PROFILE_BANNERS[0];
  const when = (t) => { const d = new Date(t); return `${d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' })} ${d.toLocaleTimeString(undefined, { hour: '2-digit', minute: '2-digit' })}`; };
  const traitFav = s.traits.map(([id, n]) => { const T = DT.TRAITS && DT.TRAITS[id]; return T ? `<div class="fav"><span class="tchip lv2" style="--tc:${T.color}"><i></i>${esc(T.name)}</span><span class="muted">${n} game${n > 1 ? 's' : ''}</span></div>` : ''; }).join('');
  const unitFav = s.units.map(([line, n]) => { const Ln = DT.LINE && DT.LINE[line]; return Ln && DT.Portraits ? `<div class="fav u">${DT.Portraits.html(line, 0, `t${Ln.tier}`)}<div><b>${esc(Ln.forms[0])}</b><span class="muted">${n} game${n > 1 ? 's' : ''}</span></div></div>` : ''; }).join('');
  const hist = W.history();
  const c = 2 * Math.PI * 58;
  root.innerHTML = `<div class="prof">
    <div class="prof-banner" style="background-image:url('${ART(banner)}')">
      <div class="prof-id-wrap">
        <button class="prof-av" id="profAv" title="Change avatar"><svg class="prof-ring" viewBox="0 0 128 128" aria-hidden="true"><circle cx="64" cy="64" r="58" class="rg-bg"/><circle cx="64" cy="64" r="58" class="rg-fg" style="stroke-dasharray:${c};stroke-dashoffset:${c * (1 - L.xp / L.need)}"/></svg>${avatarHTML('big')}<span class="pav-l">Change</span><b class="prof-lv">${L.level}</b></button>
        <div class="prof-id">
          <div class="eyebrow">Tamer · level ${L.level}</div>
          <input id="profName" class="input prof-name" maxlength="16" value="${esc(name)}" placeholder="Your Tamer name" aria-label="Tamer name" spellcheck="false"${steamPersona ? ' readonly title="Your Steam name"' : ''}>
          <div class="muted small">${L.xp} / ${L.need} XP to level ${L.level + 1}. Other players see your name in lobbies.</div>
        </div>
      </div>
      <div class="prof-rank"><img class="${R.unranked ? 'dim' : ''}" src="${ART(R.emblem)}" alt=""><div><b>${esc(R.label)}</b><small>${esc(R.sub)}</small></div></div>
      <div class="prof-banners" role="group" aria-label="Banner">${PROFILE_BANNERS.map((b) => `<button class="pb${b === banner ? ' on' : ''}" data-banner="${b}" style="background-image:url('${ART(b)}')" title="Use this banner" aria-label="Banner ${b.slice(-1)}"></button>`).join('')}</div>
    </div>
    <div class="prof-cols">
      <div class="prof-left">
        <div class="stat-tiles">
          <div class="tile"><b>${s.games}</b><span>Games</span></div>
          <div class="tile"><b>${s.wins}</b><span>Wins</span></div>
          <div class="tile"><b>${s.top4}</b><span>Top finishes</span></div>
          <div class="tile"><b>${s.games ? s.avg.toFixed(2) : '-'}</b><span>Avg place</span></div>
        </div>
        <div class="box"><div class="card-h"><span>Placements</span><span class="muted">last ${save.matches.length || 0}</span></div>${save.matches.length ? placementChart() : '<div class="empty small">No games yet.</div>'}</div>
        <div class="box"><div class="card-h"><span>Trend</span><span class="muted">oldest to newest</span></div>${trendChart()}</div>
        <div class="box"><div class="card-h"><span>Favorite traits</span></div>${traitFav || '<div class="empty small">Play a game to see your favorite traits.</div>'}</div>
        <div class="box"><div class="card-h"><span>Favorite Digimon</span></div><div class="favs-u">${unitFav || '<div class="empty small">Play a game to see the Digimon you field most.</div>'}</div></div>
      </div>
      <div class="prof-right">
        <div class="box hist-card"><div class="card-h"><span>Match history</span><span class="muted">${save.matches.length ? `last ${save.matches.length}` : ''}</span></div>
          <div class="hist-list">${save.matches.length ? save.matches.map((m) => {
            const cc = compHTML(m, 8);
            const tier = m.place === 1 ? 'gold' : m.place <= (m.format === 'duos' ? 2 : 4) ? 'good' : 'low';
            return `<div class="match p-${tier}"><div class="m-place"><b>#${m.place}</b><span>${m.mode === 'mp' ? 'Multi' : 'Solo'}</span></div>
              <div class="m-body"><div class="m-top"><span>${placeLabel(m.place, m.format === 'duos')}${m.format === 'duos' ? ' · Duos' : ''}${m.set ? ` · ${esc(setName(m.set))}` : ''} · round ${esc(m.round)} · level ${m.level}</span><span class="muted">${when(m.at)}</span></div>
                <div class="m-traits">${cc.traits || '<span class="muted small">No active traits</span>'}</div>
                <div class="m-units">${cc.units}</div></div>
              <div class="m-bits">${m.bits != null ? `${bit}<b>+${m.bits}</b>` : ''}</div></div>`;
          }).join('') : '<div class="empty">No games yet. Your last 30 games show up here with placement, traits and Digimon.</div>'}</div></div>
        <div class="box bits-card"><div class="card-h"><span>Bits history</span><span class="chip-bits static">${bit}<b>${W.bits.toLocaleString()}</b></span></div>
          <div class="bits-list">${hist.length ? hist.slice(0, 14).map((x) => `<div class="bh-row"><span>${esc(x.reason || 'Bits')}</span><b>+${x.n}</b><i>${new Date(x.at).toLocaleDateString()}</i></div>`).join('') : '<div class="empty small">No Bits earned yet. Finish a game to earn some.</div>'}</div></div>
      </div>
    </div>
  </div>`;
};
$('pgProfile').addEventListener('click', (e) => {
  if (e.target.closest('#profAv')) openAvatarPicker();
  const b = e.target.closest('[data-banner]');
  if (b) { save.profile.banner = b.dataset.banner; persist(); sfx('equip'); RENDER.profile(); }
});
$('pgProfile').addEventListener('change', (e) => {
  if (e.target.id !== 'profName') return;
  const n = cleanName(e.target.value);
  e.target.value = n;
  store.set('dt.name', n);
  refreshName();
  toast(n ? `Name saved: ${n}` : 'Name cleared', 'good');
});
function openAvatarPicker() {
  const owned = C.list('legend').filter((x) => C.owns('legend', x.id));
  const cur = avatarId();
  const box = modal(`<h2 class="cm-title">Choose your icon</h2><p class="muted">Any Little Legend you own.</p>
    <div class="av-grid">${owned.map((x) => `<button class="av-pick${x.id === cur ? ' on' : ''}" data-av="${esc(x.id)}">${DT.Legends.html(legendKey(x.id))}<span>${esc(x.name)}</span></button>`).join('')}</div>
    <div class="cm-btns"><button class="btn btn-ghost" data-cm-close>Close</button></div>`, { cls: 'wide' });
  box.addEventListener('click', (e) => {
    const b = e.target.closest('[data-av]');
    if (!b) return;
    save.profile.avatar = b.dataset.av;
    persist();
    sfx('equip');
    closeModal();
    refreshChrome();
    RENDER.profile();
  });
}

// =====================================================================
// Settings
// =====================================================================
const KEYS = [['D', 'Reroll the shop'], ['E', 'Sell the Digimon under the cursor'], ['Space', 'Start the fight'], ['T', 'Emote wheel'], ['G', 'Soundboard'],
  ['C', 'Codex'], ['1-8, Q / E', 'Scout another Tamer between rounds'], ['R', 'Reset the camera'], ['+ / -', 'Zoom'], ['Right-click', 'Move your Little Legend'],
  ['Esc', 'Back / close the top window'], ['Tab', 'Move between buttons']];
RENDER.settings = () => {
  const el = $('settingsBody');
  const S = DT.Sfx && DT.Sfx.settings;
  const pct = (v) => Math.round((v || 0) * 100);
  const sw = (id, on, label, sub) => `<label class="toggle"><input type="checkbox" id="${id}" ${on ? 'checked' : ''}><span class="tg"></span><span class="tl"><b>${label}</b>${sub ? `<small>${sub}</small>` : ''}</span></label>`;
  const slider = (id, v, label) => `<div class="srow"><span>${label}</span><input type="range" id="${id}" min="0" max="100" value="${pct(v)}"><output id="${id}V">${pct(v)}</output></div>`;
  el.innerHTML = `<div class="page-head"><div><div class="eyebrow">Settings</div><h1 class="page-title">Settings</h1></div></div>
    <div class="set-grid">
      <div class="box"><h3 class="card-h">Sound</h3>
        ${S ? `${sw('setMute', S.muted, 'Mute all')}
        ${sw('setMusicOn', !S.musicOff, 'Music')}
        ${slider('setMusic', S.music, 'Music volume')}
        ${slider('setSfx', S.sfx, 'Effects volume')}
        ${slider('setBoard', S.board, 'Soundboard volume')}
        ${sw('setOthers', S.muteOthers, "Mute other players' soundboard")}
        <div class="set-music"><span class="muted">${S.source === 'own' ? 'Playing your own music files.' : 'Built-in soundtrack.'}</span><button class="btn btn-secondary btn-sm" id="setOwn">Your music files...</button></div>` : '<p class="muted">Sound is not available here.</p>'}
      </div>
      <div class="box"><h3 class="card-h">Interface</h3>
        ${sw('setMotion', save.prefs.motion, 'Animations', 'Page transitions and the turning pedestal')}
        ${sw('setUiSfx', save.prefs.uiSfx, 'Menu sounds', 'Clicks and hovers in the menus')}
        ${sw('setSocial', save.prefs.social, 'Friends list', 'The social column on the right')}
        <h3 class="card-h sp">Data</h3>
        <p class="muted small">Your Bits, cosmetics, Little Legend levels, farm and match history are saved in this browser only.</p>
        <button class="btn btn-danger" id="setReset">Reset save...</button>
        <button class="btn btn-secondary" data-profile-backup>Export save backup</button>
        <p class="muted small">Includes progress, settings and your music files. Keep this backup private.</p>
        ${UPD ? '<h3 class="card-h sp">About</h3><p class="muted small" id="setVer">Digi-Tactics</p>' : ''}
      </div>
      <div class="box keys"><h3 class="card-h">Keys</h3>
        <table class="keytable">${KEYS.map(([k, d]) => `<tr><td>${k.split(', ').map((x) => `<kbd>${esc(x)}</kbd>`).join(' ')}</td><td>${esc(d)}</td></tr>`).join('')}</table>
      </div>
    </div>`;
  fillVersion();
};
$('pgSettings').addEventListener('input', (e) => {
  const map = { setMusic: 'music', setSfx: 'sfx', setBoard: 'board' };
  const k = map[e.target.id];
  if (!k) return;
  DT.Sfx.set({ [k]: e.target.value / 100 });
  $(`${e.target.id}V`).textContent = e.target.value;
});
$('pgSettings').addEventListener('change', (e) => {
  const id = e.target.id;
  if (id === 'setSfx') sfx('buy');
  if (id === 'setMute') DT.Sfx.set({ muted: e.target.checked });
  if (id === 'setMusicOn') DT.Sfx.set({ musicOff: !e.target.checked });
  if (id === 'setOthers') DT.Sfx.set({ muteOthers: e.target.checked });
  if (id === 'setMotion') { save.prefs.motion = e.target.checked; persist(); applyPrefs(); }
  if (id === 'setUiSfx') { save.prefs.uiSfx = e.target.checked; persist(); }
  if (id === 'setSocial') toggleSocial(e.target.checked);
});
$('pgSettings').addEventListener('click', (e) => {
  if (e.target.closest('#setReset')) return confirmReset();
  const own = e.target.closest('#setOwn');
  if (own && DT.SoundUI) {
    DT.SoundUI.open();
    const pop = $('sndPop');
    if (pop) { const r = own.getBoundingClientRect(); pop.style.top = `${Math.min(window.innerHeight - pop.offsetHeight - 8, r.bottom + 6)}px`; pop.style.left = `${Math.max(8, Math.min(window.innerWidth - pop.offsetWidth - 8, r.left))}px`; }
  }
});
if (DT.Sfx && DT.Sfx.onChange) DT.Sfx.onChange(() => { if (st.page === 'settings' && clientOn() && !document.activeElement.matches('#settingsBody input[type=range]')) RENDER.settings(); });
function applyPrefs() { document.body.classList.toggle('reduce-motion', !save.prefs.motion); }
applyPrefs();

function confirmReset() {
  const box = modal(`<h2 class="cm-title">Reset your save?</h2>
    <p>This deletes your Bits, cosmetics, Little Legend levels, Digi-Farm, match history and settings in this browser. It cannot be undone.</p>
    <label class="field"><span>Type RESET to confirm</span><input id="resetTxt" class="input" autocomplete="off" spellcheck="false" data-autofocus></label>
    <div class="cm-btns"><button class="btn btn-ghost" data-cm-close>Cancel</button><button class="btn btn-danger" id="resetGo" disabled>Delete everything</button></div>`, { cls: 'danger' });
  const inp = box.querySelector('#resetTxt');
  const go2 = box.querySelector('#resetGo');
  inp.oninput = () => { go2.disabled = inp.value.trim().toUpperCase() !== 'RESET'; };
  go2.onclick = () => {
    try { for (const k of Object.keys(localStorage)) if (k.startsWith('dt.') || k.startsWith('dt-')) localStorage.removeItem(k); } catch (e) { /* blocked */ }
    try { for (const k of Object.keys(sessionStorage)) if (k.startsWith('dt.')) sessionStorage.removeItem(k); } catch (e) { /* blocked */ }
    try { indexedDB.deleteDatabase('dt-music'); } catch (e) { /* none */ }
    location.reload();
  };
}

// ---------- credits ----------
function credits() {
  const rows = (DT.AssetCredits || []).map(s => `<div class="cr-row"><b>${esc(s.name)}</b><span>${esc(s.contributors.join(', ') || 'Contributor not yet verified')} &middot; ${esc(s.license)}${s.url ? ` &middot; <a href="${esc(s.url)}" target="_blank" rel="noopener">Source</a>` : ''}</span></div>`).join('');
  overlay.classList.remove('hidden');
  modal(`<h2 class="cm-title">Credits &amp; acknowledgements</h2>
    <p class="muted">An unofficial, non-commercial fan project by William. Thanks to the original game artists, model creators, community extractors and uploaders credited below. Source-page contributors are credited for their documented role; they are not assumed to own the original models.</p>
    <p class="muted">Illustrations were created with AI assistance; portraits use the game models. Boards, effects, music and sound cues are generated by the project. Mechanics draw on classic autobattler sets, alliances and Auto Chess. Full game, software and rights acknowledgements are in the bundled CREDITS.md and THIRD_PARTY_NOTICES.txt.</p>
    <div class="credits">${rows || 'No models configured.'}</div>
    <div class="cm-btns"><button class="btn btn-primary" data-cm-close id="backBtn">Back</button></div>`, { cls: 'wide' });
}

// ---------- shared clicks inside the shell ----------
overlay.addEventListener('click', (e) => {
  const g = e.target.closest('[data-go]');
  if (g) {
    if (g.dataset.goTab) col.tab = g.dataset.goTab;
    sfx('pick', true);
    return go(g.dataset.go);
  }
  if (e.target.closest('[data-res-close]')) { st.result = null; RENDER.home(); syncStage(); }
});

// =====================================================================
// Boot screen: splash art, models, portraits and fonts, with a progress bar
// =====================================================================
function boot() {
  const el = $('boot');
  if (!el) return;
  el.style.setProperty('--boot-art', `url('${ART('boot-splash')}')`);
  el.classList.add('art');
  const bar = el.querySelector('.boot-bar i');
  const step = el.querySelector('.boot-step');
  const t0 = performance.now();
  const portraitIdx = location.protocol === 'file:' ? Promise.resolve() : fetch('models/portraits/index.json', { cache: 'no-cache' }).catch(() => {});
  const legend = DT.Legends ? DT.Legends.modelFor(legendKey(C.equipped('legend'))).catch(() => null) : Promise.resolve();
  const shopPics = new Promise((res) => {
    const lines = (DT.SHOP_LINES || []).slice(0, 10).map((l) => l.id);
    if (!DT.Portraits || !lines.length) return res();
    const until = performance.now() + 2500;
    const tick = () => {
      const left = lines.filter((id) => !DT.Portraits.get(id, 0, false));
      if (!left.length || performance.now() > until) return res();
      setTimeout(tick, 120);
    };
    lib.ready.then(tick, tick);
  });
  // the Home hero art, so the first page shows with its painting
  const heroArt = new Promise((res) => { const i = new Image(); i.onload = i.onerror = res; i.src = ART(setArt(selectedSet()).hero); setTimeout(res, 2500); });
  const steps = [
    ['Loading fonts', document.fonts && document.fonts.ready ? document.fonts.ready : Promise.resolve()],
    ['Reading the model list', lib.ready],
    ['Loading portraits', portraitIdx],
    ['Waking your Little Legend', legend],
    ['Drawing the shop', shopPics],
    ['Painting the client', heroArt],
  ];
  let done = 0;
  const show = (label) => { bar.style.width = `${Math.round((100 * done) / steps.length)}%`; if (label) step.textContent = label; };
  show(steps[0][0]);
  const finish = () => {
    if (el.classList.contains('done')) return;
    bar.style.width = '100%';
    step.textContent = 'Ready';
    const wait = Math.max(0, 300 - (performance.now() - t0));
    setTimeout(() => {
      el.classList.add('done');
      setTimeout(() => { el.remove(); }, 260);
      syncClient();
      if (clientOn() && RENDER[st.page]) RENDER[st.page]();
      syncStage();
    }, wait);
  };
  steps.forEach(([, p], i) => {
    Promise.resolve(p).catch(() => {}).then(() => {
      done++;
      const next = steps.find((s, j) => j > i && !s.done);
      steps[i].done = true;
      show(next ? next[0] : '');
      if (done === steps.length) finish();
    });
  });
  setTimeout(finish, 6000); // never hold the menu hostage to a slow download
}

// ---------- start ----------
// while a window or popup covers the page (Store, Farm, Codex, Digivolve panel...), the page's live previews and
// the pedestal stop drawing; they come back when it is uncovered
function syncCovered() {
  const covered = clientOn() && !!(st.module || popupOpen());
  if (covered === !!st.covered) return;
  st.covered = covered;
  if (covered) { disposePreviews(); if (stage) stage.stop(); }
  else if (clientOn() && RENDER[st.page]) { RENDER[st.page](); syncStage(); }
}
setInterval(() => {
  syncModules();
  syncCovered();
  if (clientOn()) refreshStatus();
}, 150);
setInterval(() => {
  if (!document.body.classList.contains('client-on')) return;
  renderSocial();
  if (STEAM && Date.now() - social.polled > 8000) pollSteamFriends();
}, 1000);
setTimeout(() => {
  syncClient();
  refreshChrome();
  if (clientOn()) go('home', { push: false });
  boot();
  renderSocial();
  pollSteamFriends();
}, 0);

// =====================================================================
// Self-update banner (Steam desktop build only: window.DTUpdate from desktop-steam/preload.js)
// Shows download progress, then "Update downloaded - restart to apply" with Restart. Only in the client
// (never during a game), so a restart can never cut a match short.
// =====================================================================
let updState = null;
function renderUpdate() {
  let el = $('dtUpd');
  const s = updState;
  const show = s && ['downloading', 'extracting', 'ready', 'shell'].includes(s.phase) && clientOn() && !(DT.UI && DT.UI.inGame && DT.UI.inGame()) && !(el && el.dataset.later === s.latest + s.phase);
  if (!show) { if (el) el.hidden = true; return; }
  if (!el) {
    el = document.createElement('div');
    el.id = 'dtUpd';
    el.innerHTML = `<style>#dtUpd{position:fixed;left:16px;bottom:16px;z-index:94;width:300px;padding:12px 14px;background:rgba(1,10,19,.96);border:1px solid var(--lc-gold-4,#785a28);border-left:3px solid var(--lc-gold-3,#c89b3c);color:var(--lc-gold-1,#f0e6d2);font:13px/1.4 var(--font-body,system-ui);box-shadow:0 8px 24px rgba(0,0,0,.5)}
      #dtUpd b{display:block;color:var(--lc-gold-2,#c8aa6e);font-family:var(--font-display,system-ui);letter-spacing:.06em;text-transform:uppercase;font-size:12px;margin-bottom:4px}
      #dtUpd .ub{height:4px;background:#1e2328;margin-top:8px}#dtUpd .ub i{display:block;height:100%;background:var(--lc-gold-3,#c89b3c);width:0}
      #dtUpd .ubtn{display:flex;gap:8px;margin-top:10px}#dtUpd .ubtn button,#dtUpd .ubtn a{flex:1;text-align:center;padding:6px 8px;cursor:pointer;font:600 12px var(--font-body,system-ui);text-decoration:none}
      #dtUpd .go{background:linear-gradient(180deg,#c89b3c,#785a28);color:#010a13;border:1px solid #c8aa6e}#dtUpd .no{background:#1e2328;color:#cdbe91;border:1px solid #463714}</style><div class="ubody"></div>`;
    el.addEventListener('click', (e) => {
      if (e.target.closest('[data-upd-restart]')) {
        if (DT.UI && DT.UI.inGame && DT.UI.inGame()) return toast('Finish your game first');
        e.target.closest('[data-upd-restart]').disabled = true;
        UPD.restart().then((ok) => { if (!ok) toast('The update is not ready yet', 'bad'); });
      } else if (e.target.closest('[data-upd-later]')) { el.dataset.later = updState.latest + updState.phase; el.hidden = true; }
    });
    document.body.appendChild(el);
  }
  el.hidden = false;
  const body = el.querySelector('.ubody');
  const v = esc(s.latest || '');
  if (s.phase === 'ready') {
    body.innerHTML = `<b>Update ${v}</b>Update downloaded. Restart to apply.${s.notes ? `<div class="muted small">${esc(s.notes)}</div>` : ''}
      <div class="ubtn"><button class="go" data-upd-restart>Restart</button><button class="no" data-upd-later>Later</button></div>`;
  } else if (s.phase === 'shell') {
    body.innerHTML = `<b>Version ${v} is out</b>This version needs a new full download. Get the DigiTactics-Steam zip from the release page, unzip it and use it instead of this folder.
      <div class="ubtn"><a class="go" href="${esc(s.page || '')}" target="_blank" rel="noopener">Release page</a><button class="no" data-upd-later>Later</button></div>`;
  } else {
    const pct = s.total ? Math.floor((100 * (s.received || 0)) / s.total) : 0;
    body.innerHTML = `<b>Update ${v}</b>${s.phase === 'extracting' ? 'Unpacking...' : `Downloading... ${pct}%`}<div class="ub"><i style="width:${s.phase === 'extracting' ? 100 : pct}%"></i></div>`;
  }
}
if (UPD) {
  UPD.on('*', (s) => { updState = s; renderUpdate(); if (st.page === 'settings' && clientOn()) fillVersion(); });
  UPD.state().then((s) => { if (s) { updState = s; renderUpdate(); } }).catch(() => {});
  setInterval(renderUpdate, 2000); // a game starting or ending changes whether the banner may show
}
function fillVersion() {
  const el = $('setVer');
  if (!el || !UPD) return;
  UPD.state().then((s) => {
    if (!s) return;
    const note = { current: 'Up to date.', offline: 'Could not check for updates (offline).', downloading: `Downloading ${esc(s.latest)}...`, extracting: `Unpacking ${esc(s.latest)}...`,
      ready: `Version ${esc(s.latest)} is downloaded and applies on restart.`, error: 'The last update failed; it will try again next start.', shell: `Version ${esc(s.latest)} needs a new full download.`, checking: 'Checking for updates...' }[s.phase] || '';
    el.innerHTML = `Digi-Tactics ${esc(s.version)}${note ? ` &middot; ${note}` : ''}`;
  }).catch(() => {});
}

DT.Client = {
  go,
  back,
  home() { st.hist = []; overlay.classList.remove('hidden'); syncClient(); go('home', { push: false }); },
  gameOver(o) { showResults(o, 'over'); },
  eliminated(o) { showResults(o, 'out'); },
  showLobby(force) {
    st.playMode = 'mp';
    st.playStep = 'party';
    const lp = $('lobbyPanel');
    if (!lp.innerHTML.trim()) lp.innerHTML = '<div class="mp-wait"><span class="spinner"></span> Connecting...</div>'; // net.js fills it next
    if (force) st.lobbyFollow = true;
    if (st.lobbyFollow || st.page === 'play') {
      if (st.page !== 'play' || st.module) go('play'); else { setPlayStep('party'); setPlayMode('mp'); }
      st.lobbyFollow = true;
    }
    refreshStatus();
  },
  lobbyBack() { $('lobbyPanel').innerHTML = ''; st.lobbyFollow = false; st.playStep = 'select'; setPlayMode('solo'); setPlayStep('select'); refreshStatus(); },
  refreshPlay() { renderPlayPick(); if (st.playStep === 'party' && st.playMode === 'solo') renderParty(); },
  refreshName,
  credits,
  toast,
  page: () => st.module || st.page,
  history: () => save.matches.slice(),
  stats,
  level: tamerLevel,
  rank: tamerRank,
  _save: () => save,
};
