// Digi-Farm room (rules and save: js/farm.js; training mini-games: js/farm-games.js). A small themed island where the
// Little Legends you own wander, sleep, play and need care. Opened from the title screen, the Locker and anything with
// data-farm-open (value: a legend id to select).
// DT.FarmUI.open(id?) / close() / isOpen() / select(id) / selected()
// DT.FarmUI.act(name, arg?)        the action bar: 'feed' (food id) | 'clean' | 'cure' ('bandage' | 'medicine') | 'lights'
//                                  | 'praise' | 'scold' | 'play'; returns the DT.Farm result
// DT.FarmUI.playGame(name, opts)   runs a training mini-game for the selected legend (opts: { auto, speed } for tests);
//                                  Promise of { score, res }
// DT.FarmUI.agents()               ids of the legends drawn in the room
import * as THREE from 'three';
import './farmstyle.js';
import './farm-games.js';

const DT = window.DT;
const C = DT.Cosmetics;
const W = DT.Wallet;
const E = DT.LegendEvo;
const F = DT.Farm;
const G = DT.FarmGames;

const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const bit = '<i class="bit"></i>';
const sfx = (n) => { try { if (DT.Sfx) DT.Sfx.play(n); } catch (e) { /* audio blocked */ } };
const clamp = (x, a, b) => Math.max(a, Math.min(b, x));
const PI = Math.PI;

// ---------- room layout (world units; the island top is y = 0) ----------
const R = 5.4;
const SPOT = { bed: [-3.2, -2.3], toilet: [3.3, -2.3], food: [-3.3, 1.7], train: [3.3, 1.5], lamp: [1.6, -4.0] };
const BED_SLOTS = [[0, 0.05], [-0.62, 0.05], [0.62, 0.05], [0, 0.75], [-0.62, 0.75], [0.62, 0.75]];
const WANDER = { x: 0, z: 0.3, rx: 2.7, rz: 2.3 };

// theme looks: sky gradient, ground, island side, water, decoration set, accent
const LOOK = {
  meadow: { sky: ['#7cc8f2', '#d9f2ff'], ground: '#74c255', dots: ['#8fd86a', '#5aa83f', '#a7e07e', '#ffffff'], side: '#8a5a34', water: '#3fa7e0', deco: 'meadow', accent: '#ff8fb0', fence: '#b07a4a' },
  tropical: { sky: ['#3fb6f0', '#e6f8ff'], ground: '#ead59c', dots: ['#f6e8bf', '#cdb172', '#ffffff'], side: '#c9a46a', water: '#1fc2c8', deco: 'tropical', accent: '#ff9a5a', fence: '#c49a64' },
  snowy: { sky: ['#8fa9cc', '#eef3fa'], ground: '#f2f6fb', dots: ['#ffffff', '#dfe8f3', '#cbd8e8'], side: '#8d9bb0', water: '#7fb0d8', deco: 'snowy', accent: '#6fb4ff', fence: '#7d5a3e' },
  sewer: { sky: ['#17221b', '#3a4a38'], ground: '#5f6e58', dots: ['#4e5c48', '#768870', '#3f4a3a'], side: '#3b463a', water: '#3f6a2a', deco: 'sewer', accent: '#c8e04a', fence: '#56604f' },
  toy: { sky: ['#ffc4e1', '#fff4c8'], ground: '#9fe0b0', dots: ['#ffffff', '#ffd1e8', '#bdf0c8'], side: '#ff9ec7', water: '#7fc8ff', deco: 'toy', accent: '#7f8cff', fence: '#ffffff' },
  grid: { sky: ['#050d1c', '#0f2a4c'], ground: '#0f2440', dots: ['#164070'], side: '#0a1a2e', water: '#0b3766', deco: 'grid', accent: '#38e8ff', fence: '#38e8ff' },
};

// ---------- small three helpers ----------
const std = (o) => new THREE.MeshStandardMaterial({ roughness: 0.85, ...o });
const matCache = new Map();
const mat = (color, o = {}) => {
  const k = `${color}|${JSON.stringify(o)}`;
  if (!matCache.has(k)) matCache.set(k, std({ color, ...o }));
  return matCache.get(k);
};
function add(parent, geo, m, x = 0, y = 0, z = 0, o = {}) {
  const mesh = new THREE.Mesh(geo, typeof m === 'string' ? mat(m) : m);
  mesh.position.set(x, y, z);
  if (o.ry) mesh.rotation.y = o.ry;
  if (o.rx) mesh.rotation.x = o.rx;
  if (o.rz) mesh.rotation.z = o.rz;
  if (o.s) mesh.scale.setScalar(o.s);
  mesh.castShadow = o.shadow !== false;
  mesh.receiveShadow = true;
  parent.add(mesh);
  return mesh;
}
const box = (p, w, h, d, m, x, y, z, o) => add(p, new THREE.BoxGeometry(w, h, d), m, x, y + h / 2, z, o);
const cyl = (p, r1, r2, h, m, x, y, z, o = {}) => add(p, new THREE.CylinderGeometry(r1, r2, h, o.seg || 16), m, x, y + h / 2, z, o);
const sph = (p, r, m, x, y, z, o = {}) => add(p, new THREE.SphereGeometry(r, o.seg || 16, Math.max(6, (o.seg || 16) * 0.6)), m, x, y, z, o);
function canvasTex(size, draw, repeat = 1) {
  const cv = document.createElement('canvas');
  cv.width = cv.height = size;
  draw(cv.getContext('2d'), size);
  const t = new THREE.CanvasTexture(cv);
  t.colorSpace = THREE.SRGBColorSpace;
  t.wrapS = t.wrapT = THREE.RepeatWrapping;
  t.repeat.set(repeat, repeat);
  return t;
}
function rng(seed) { let s = seed >>> 0 || 1; return () => { s = (s * 16807) % 2147483647; return s / 2147483647; }; }

// ---------- state ----------
let root = null;
let sel = null;
let menu = null; // 'feed' | 'cure' | 'train' | null
let modalKind = null; // 'bank' | 'themes' | 'game' | null
let game = null; // running mini-game handle
let hover = null;
const S = { r: null, scene: null, cam: null, raf: 0, last: 0, world: null, theme: null, agents: new Map(), piles: new Map(), ticks: [], hits: [],
  hemi: null, sun: null, lamp: null, lampBulb: null, meat: [], ring: null, ray: new THREE.Raycaster(), t: 0, slow: 0, night: false };

// ---------- DOM ----------
function build() {
  if (root) return root;
  root = document.createElement('div');
  root.id = 'dtFarm';
  root.className = 'hidden';
  root.setAttribute('role', 'dialog');
  root.setAttribute('aria-label', 'Digi-Farm');
  root.innerHTML = `<div class="df-win">
    <header class="df-head"><h2>Digi-Farm</h2><span class="df-theme"></span><span class="sp"></span><span class="df-pills"></span>
      <button data-fm="bank" title="Choose who lives in the room">Digi-Bank</button><button data-fm="themes">Themes</button><span class="df-exp"></span>
      <button class="df-close" title="Esc">Close</button></header>
    <div class="df-body">
      <div class="df-scene"><div class="df-stars"></div><canvas></canvas><div class="df-night"></div><div class="df-icons"></div><div class="df-chips"></div>
        <div class="df-toast"></div><div class="df-menu hidden"></div><div class="df-bar"></div></div>
      <aside class="df-card"></aside>
    </div>
    <div class="df-modal hidden"><div class="df-mbox"></div></div></div>`;
  document.body.appendChild(root);
  root.addEventListener('click', onClick);
  const cv = root.querySelector('canvas');
  cv.addEventListener('pointerdown', onPointer);
  cv.addEventListener('pointermove', onMove);
  cv.addEventListener('pointerleave', () => { hover = null; });
  return root;
}

const $ = (s) => root.querySelector(s);
// only touch the DOM when the markup changed (the card and bars refresh every second)
const setHTML = (el, html) => { if (el._h !== html) { el.innerHTML = html; el._h = html; } };
function toast(text, ms = 2200) {
  if (!root || !text) return;
  const t = $('.df-toast');
  t.textContent = text;
  t.classList.add('on');
  clearTimeout(toast.t);
  toast.t = setTimeout(() => t.classList.remove('on'), ms);
}

// ---------- header, chips, action bar, card ----------
const ACTIONS = [
  ['feed', '🍖', 'Feed'], ['clean', '🧹', 'Clean'], ['cure', '💊', 'Medicine'], ['lights', '💡', 'Lights'],
  ['praise', '👏', 'Praise'], ['scold', '💢', 'Scold'], ['train', '🏋️', 'Train'], ['play', '⚽', 'Play'],
];

function render() {
  if (!isOpen()) return;
  const st = F.state();
  if (!st.residents.includes(sel)) sel = st.residents[0] || null;
  const th = F.themeOf(st.theme);
  $('.df-theme').textContent = `${th.icon} ${th.name}`;
  const d = st.daily;
  setHTML($('.df-pills'), `<span class="df-pill" title="Legends in the room / room size">🏠 <b>${st.residents.length}/${st.cap}</b></span>
    <span class="df-pill" title="Free meat on the tray. One more every ${F.MEAT_EVERY / 60} hours.">🍖 <b>${st.meat.n}/${st.meat.max}</b></span>
    <span class="df-pill" title="Bandages. One more every ${F.BANDAGE_EVERY / 60} hours.">🩹 <b>${st.bandage.n}/${st.bandage.max}</b></span>
    <span class="df-pill${d.bits >= d.capBits ? ' capped' : ''}" title="Bits earned on the farm today (daily cap)">${bit} <b>${d.bits}/${d.capBits}</b> today</span>
    <span class="df-pill${d.data >= d.capData ? ' capped' : ''}" title="Digi-Data earned on the farm today (daily cap)">💾 <b>${d.data}/${d.capData}</b></span>
    <span class="df-pill" title="Your Bits">${bit} <b>${W.bits}</b></span>`);
  setHTML($('.df-exp'), st.cap < st.capMax ? `<button data-fm="expand" title="Room for ${2} more legends" ${W.bits < st.expandCost ? 'disabled' : ''}>Expand · ${bit}${st.expandCost}</button>` : '');
  // chips
  setHTML($('.df-chips'), st.residents.map((id) => {
    const c = F.card(id);
    return `<button class="df-chip${id === sel ? ' on' : ''}" data-fsel="${esc(id)}" title="${esc(c.name)}: ${esc(c.mood.text)}">${DT.Legends.html(c.key)}<i>${c.mood.icon}</i></button>`;
  }).join(''));
  // action bar
  const c = sel ? F.card(sel) : null;
  const alert = { feed: c && c.hunger < 25, clean: F.piles().length > 0, cure: c && c.sick, lights: c && !st.lights ? false : c && c.energy < 20, scold: c && c.tantrum };
  setHTML($('.df-bar'), ACTIONS.map(([k, e, n]) => {
    const label = k === 'lights' ? (st.lights ? 'Lights off' : 'Lights on') : n;
    return `<button class="df-act${menu === k ? ' on' : ''}${alert[k] ? ' alert' : ''}" data-fa="${k}" ${c ? '' : 'disabled'}><span>${k === 'lights' && !st.lights ? '🌙' : e}</span>${label}</button>`;
  }).join(''));
  renderMenu();
  renderCard(c);
  setNight(!st.lights);
  if (S.theme !== st.theme) buildWorld(st.theme);
  syncAgents();
  syncPiles();
  syncMeat(st.meat.n);
}

function renderMenu() {
  const m = $('.df-menu');
  if (!menu || !sel) { m.classList.add('hidden'); return; }
  const st = F.state();
  let h = '';
  if (menu === 'feed') {
    h = `<h4>Feed ${esc(F.nameOf(sel))}</h4>${Object.entries(F.FOODS).map(([k, f]) => {
      const cost = k === 'meat' && st.meat.n > 0 ? `Free (${st.meat.n} left)` : `${bit}${k === 'meat' ? F.MEAT_BITS : f.bits}`;
      const fx = [`+${f.hunger} hunger`, f.weight ? `+${f.weight} G` : 'no weight', f.happy ? `+${f.happy} happy` : '', f.energy ? `+${f.energy} energy` : '', f.disc ? `${f.disc} discipline` : ''].filter(Boolean).join(', ');
      return `<button class="df-mi" data-food="${k}"><span class="e">${f.icon}</span><span><b>${f.name}</b><small>${fx}. ${esc(f.desc)}</small></span><span class="c">${cost}</span></button>`;
    }).join('')}`;
  } else if (menu === 'cure') {
    h = `<h4>Medicine for ${esc(F.nameOf(sel))}</h4>
      <button class="df-mi" data-cure="bandage" ${st.bandage.n > 0 ? '' : 'disabled'}><span class="e">🩹</span><span><b>Bandage</b><small>Cures sickness. Free; one more every ${F.BANDAGE_EVERY / 60} hours.</small></span><span class="c">${st.bandage.n}/${st.bandage.max}</span></button>
      <button class="df-mi" data-cure="medicine" ${W.bits >= F.MEDICINE_BITS ? '' : 'disabled'}><span class="e">💊</span><span><b>Medicine</b><small>Cures sickness. Bitter: -5 happiness.</small></span><span class="c">${bit}${F.MEDICINE_BITS}</span></button>`;
  } else if (menu === 'train') {
    const why = F.canTrain(sel);
    h = `<h4>Training ${why ? `<span style="color:#ff9a7a;text-transform:none;letter-spacing:0">· ${esc(why)}</span>` : '<span style="text-transform:none;letter-spacing:0">· 15 energy, 8 hunger, slims 1-2 G</span>'}</h4>
      ${Object.entries(F.TRAIN).map(([k, t]) => `<button class="df-mi" data-train="${k}" ${why ? 'disabled' : ''}><span class="e">${{ strength: '💪', speed: '⚡', wisdom: '🧠', stamina: '❤️' }[k]}</span><span><b>${G.GAMES[k].title}</b><small>${esc(G.GAMES[k].help)}</small></span><span class="c">${t.name}</span></button>`).join('')}`;
  }
  setHTML(m, h);
  m.classList.remove('hidden');
}

const BARS = [['hunger', '🍖', 'Hunger', '#ff9a3c', '#ffd36a'], ['energy', '⚡', 'Energy', '#2f9bff', '#8ff0ff'], ['happy', '♥', 'Happiness', '#ff5f8f', '#ffb3c8'], ['disc', '📏', 'Discipline', '#8b6cff', '#c6b6ff']];
const CARE_ICON = { bond: '♥', battle: '⚔', food: '🍖', wild: '?' };
function renderCard(c) {
  const el = $('.df-card');
  if (!c) { setHTML(el, '<div class="muted">No legend in the room. Open the Digi-Bank.</div>'); return; }
  const i = E.info(c.id);
  const bars = BARS.map(([k, e, n, a, b]) => `<div class="df-stat" title="${n}${k === 'hunger' ? ': a full bar means well fed' : ''}"><span class="e">${e}</span><span>${n}</span>
    <div class="df-meter${c[k] < 20 ? ' low' : ''}"><i style="width:${c[k]}%;background:linear-gradient(90deg,${a},${b})"></i></div><span class="n">${Math.round(c[k])}</span></div>`).join('');
  const tags = [
    `<span class="df-tag${c.fat ? ' bad' : c.thin ? ' bad' : ' good'}" title="Ideal about ${c.ideal} G. Overfeeding adds weight; training slims.">⚖ ${c.weight} G${c.fat ? ' · chubby' : c.thin ? ' · thin' : ''}</span>`,
    `<span class="df-tag">🎂 ${c.ageDays} day${c.ageDays === 1 ? '' : 's'}</span>`,
    `<span class="df-tag${c.mistakes ? ' bad' : ' good'}" title="Counted as in the V-Pet: hungry, sad or sleepy too long, ignored tantrums, sickness left untreated">✗ ${c.mistakes} care mistake${c.mistakes === 1 ? '' : 's'}</span>`,
    c.poop ? `<span class="df-tag bad">💩 ${c.poop}</span>` : '',
    c.sick ? `<span class="df-tag bad">🤒 Sick${c.sickWhy === 'dirty' ? ' (dirty room)' : c.sickWhy === 'hungry' ? ' (hungry)' : ''}</span>` : '',
    c.tantrum ? '<span class="df-tag bad">💢 Tantrum: scold it</span>' : '',
    c.asleep ? '<span class="df-tag">💤 Asleep</span>' : '',
  ].join('');
  const tr = [['str', 'Strength'], ['spd', 'Speed'], ['wis', 'Wisdom'], ['sta', 'Stamina']].map(([k, n]) => `<div><span>${n}</span><b>${c[k]}</b><div class="df-meter"><i style="width:${(100 * c[k]) / c.trainCap}%;background:linear-gradient(90deg,#9fe07a,#e8ff9a)"></i></div></div>`).join('');
  const odds = c.odds.length ? c.odds.map((o) => {
    const f = E.formInfo(o.form);
    const known = i.dex.includes(o.form);
    return `<div class="df-odd${known ? '' : ' unk'}">${DT.Legends.html(f.legend ? f.legend : `${c.id}@${o.form}`)}<span class="nm">${known ? esc(f.name) : '???'}</span><span class="tg ${o.care}">${CARE_ICON[o.care]} ${E.CARE[o.care].name}</span><span class="pc">${o.pct}%</span></div>`;
  }).join('') : '<div class="muted" style="font-size:12px">Final form. Revert in the Digivolve panel to try other branches.</div>';
  const ev = c.evo || { bond: 0, battle: 0, food: 0, wild: 0, why: { mistakes: 0, poopNeglect: 0 } };
  const w = ev.why || {};
  const push = `<div class="df-push">
    <span class="k bond">♥ Bond</span><span>Praise, play and petting</span><span class="v">+${ev.bond}</span>
    <span class="k battle">⚔ Battle</span><span>Training stats (every 25 points)</span><span class="v">+${ev.battle}</span>
    <span class="k food">🍖 Food</span><span>Meals when hungry (carrots count more)</span><span class="v">+${ev.food}</span>
    <span class="k wild">? Wild</span><span>${w.mistakes || 0} mistakes ×3, ${w.poopNeglect || 0} poop left out ×2${w.fat ? ', chubby +6' : ''}${w.sick ? ', sick +3' : ''}</span><span class="v">+${ev.wild}</span></div>`;
  setHTML(el, `<div class="df-who">${DT.Legends.html(c.key)}<div class="t"><b>${esc(c.name)}</b><span>${c.form !== c.id ? `${esc(c.base)}'s ` : ''}${esc(c.formInfo.level)} · Lv ${c.level}</span></div>
      <span class="df-mood">${c.mood.icon ? `${c.mood.icon} ` : ''}${esc(c.mood.text)}</span></div>
    <div class="df-box">${bars}</div>
    <div class="df-tags">${tags}</div>
    <div class="df-box"><h3>Training <span>cap ${c.trainCap}</span></h3><div class="df-train">${tr}</div></div>
    <div class="df-box"><h3>Next digivolution <span>${c.canDigivolve ? '<b style="color:#ffd36a">ready now</b>' : c.next ? `at level ${c.next}` : ''}</span></h3>${odds}
      <button class="df-evobtn${c.canDigivolve ? ' primary' : ''}" data-evo-open="${esc(c.id)}">Open Digivolve</button></div>
    <div class="df-box"><h3>Farm care pushes branches</h3>${push}</div>`);
}

function renderModal() {
  const m = $('.df-modal');
  if (!modalKind) { m.classList.add('hidden'); return; }
  m.classList.remove('hidden');
  const box = m.querySelector('.df-mbox');
  if (modalKind === 'bank') {
    const st = F.state();
    const slot = (id, inRoom) => {
      const c = E.info(id);
      const btn = inRoom ? `<button data-out="${esc(id)}" ${st.residents.length <= 1 ? 'disabled' : ''}>To Digi-Bank</button>`
        : st.residents.length < st.cap ? `<button class="primary" data-in="${esc(id)}">Move in</button>`
          : sel ? `<button data-swap="${esc(id)}" title="Swaps with ${esc(F.nameOf(sel))}">Swap in for ${esc(F.nameOf(sel))}</button>` : '';
      return `<div class="df-slot">${DT.Legends.html(c.key)}<b>${esc(c.formInfo.name)}</b><span class="muted" style="font-size:11px">Lv ${c.level}</span>${btn}</div>`;
    };
    const empty = Math.max(0, st.cap - st.residents.length);
    box.innerHTML = `<button class="x" data-mclose="1">Close</button><h3>Room <span class="muted">${st.residents.length}/${st.cap}</span></h3>
      <div class="df-grid">${st.residents.map((id) => slot(id, true)).join('')}${'<div class="df-slot empty">Empty</div>'.repeat(empty)}</div>
      <h3>Digi-Bank <span class="muted">${st.bank.length} waiting. Legends in the bank are paused: no hunger, no poop.</span></h3>
      <div class="df-grid">${st.bank.map((id) => slot(id, false)).join('') || '<div class="muted">Nobody in the bank. Every legend you own lives in the room.</div>'}</div>
      ${st.cap < st.capMax ? `<div class="muted">Expand the room for ${bit}${st.expandCost} to fit 2 more (up to ${st.capMax}).</div>` : ''}`;
  } else if (modalKind === 'themes') {
    const st = F.state();
    box.innerHTML = `<button class="x" data-mclose="1">Close</button><h3>Room themes</h3><div class="df-themes">${st.themes.map((th) => {
      const L = LOOK[th.id];
      const act = th.id === st.theme ? '<button disabled>In use</button>' : th.owned ? `<button class="primary" data-theme="${th.id}">Use</button>`
        : `<button data-theme="${th.id}" ${W.bits < th.price ? 'disabled' : ''}>Buy · ${bit}${th.price}</button>`;
      return `<div class="df-th${th.id === st.theme ? ' cur' : ''}"><div class="sw" style="background:linear-gradient(${L.sky[0]},${L.sky[1]} 55%,${L.ground} 56%,${L.ground})">${th.icon}</div><b>${esc(th.name)}</b><small>${esc(th.desc)}</small>${act}</div>`;
    }).join('')}</div>`;
  }
}

// ---------- 3D ----------
function ensureScene() {
  if (S.r) return;
  const canvas = $('canvas');
  const r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true });
  r.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
  r.outputColorSpace = THREE.SRGBColorSpace;
  r.setClearColor(0x000000, 0);
  r.shadowMap.enabled = true;
  r.shadowMap.type = THREE.PCFSoftShadowMap;
  const scene = new THREE.Scene();
  S.hemi = new THREE.HemisphereLight('#ffffff', '#4a5a3a', 1.7);
  scene.add(S.hemi);
  S.sun = new THREE.DirectionalLight('#fff3dc', 2.3);
  S.sun.position.set(4, 9, 5);
  S.sun.castShadow = true;
  S.sun.shadow.mapSize.set(1024, 1024);
  Object.assign(S.sun.shadow.camera, { left: -8, right: 8, top: 8, bottom: -8, near: 1, far: 30 });
  S.sun.shadow.bias = -0.0015;
  scene.add(S.sun);
  S.lamp = new THREE.PointLight('#ffc878', 0, 9, 1.5);
  S.lamp.position.set(SPOT.lamp[0], 2.1, SPOT.lamp[1] + 0.3);
  scene.add(S.lamp);
  const cam = new THREE.PerspectiveCamera(34, 1, 0.1, 100);
  S.r = r; S.scene = scene; S.cam = cam;
  // selection ring
  S.ring = new THREE.Mesh(new THREE.RingGeometry(0.42, 0.52, 40), new THREE.MeshBasicMaterial({ color: '#ffcf4a', transparent: true, opacity: 0.9, side: THREE.DoubleSide, depthWrite: false }));
  S.ring.rotation.x = -PI / 2;
  S.ring.position.y = 0.03;
  S.ring.renderOrder = 2;
  scene.add(S.ring);
}

function placeCamera(w, h) {
  const a = w / h;
  // keep the whole island in view on narrow and wide scenes
  // far enough that the fence fits across and the front rim stays above the action bar
  const halfW = Math.tan((S.cam.fov * PI) / 360) * a;
  const dist = clamp(Math.max(15, (R + 0.2) / halfW), 15, 22);
  S.cam.position.set(0, dist * 0.74, dist * 0.84);
  S.cam.lookAt(0, -0.35, 0.55);
  S.cam.aspect = a;
  S.cam.updateProjectionMatrix();
}

function buildWorld(themeId) {
  ensureScene();
  if (S.world) {
    S.scene.remove(S.world);
    const shared = new Set(matCache.values());
    S.world.traverse((o) => { if (o.isMesh || o.isPoints || o.isSprite) { if (!o.isSprite) o.geometry.dispose(); [].concat(o.material).forEach((m) => { if (shared.has(m)) return; if (m.map) m.map.dispose(); m.dispose(); }); } });
  }
  S.ticks = [];
  S.meat = [];
  S.theme = themeId;
  const L = LOOK[themeId] || LOOK.meadow;
  const scn = $('.df-scene');
  scn.style.setProperty('--sky1', L.sky[0]);
  scn.style.setProperty('--sky2', L.sky[1]);
  const g = new THREE.Group();
  S.world = g;
  S.scene.add(g);
  const rand = rng(11);
  // water
  const water = add(g, new THREE.CircleGeometry(40, 48), std({ color: L.water, roughness: 0.25, metalness: 0.15, emissive: L.deco === 'grid' ? L.water : '#000000', emissiveIntensity: 0.4 }), 0, -0.55, 0, { rx: -PI / 2, shadow: false });
  S.ticks.push((dt, t) => { water.position.y = -0.55 + Math.sin(t * 0.9) * 0.04; });
  // island
  const topTex = L.deco === 'grid'
    ? canvasTex(256, (c, s) => { c.fillStyle = L.ground; c.fillRect(0, 0, s, s); c.strokeStyle = '#38e8ff'; c.globalAlpha = 0.55; c.lineWidth = 2; for (let i = 0; i <= s; i += 32) { c.beginPath(); c.moveTo(i, 0); c.lineTo(i, s); c.moveTo(0, i); c.lineTo(s, i); c.stroke(); } }, 3)
    : L.deco === 'toy' ? canvasTex(128, (c, s) => { c.fillStyle = L.ground; c.fillRect(0, 0, s, s); c.fillStyle = '#c4f2cf'; c.fillRect(0, 0, s / 2, s / 2); c.fillRect(s / 2, s / 2, s / 2, s / 2); }, 6)
      : L.deco === 'sewer' ? canvasTex(256, (c, s) => { c.fillStyle = L.ground; c.fillRect(0, 0, s, s); c.strokeStyle = '#3f4a3a'; c.lineWidth = 3; for (let y = 0; y < s; y += 32) { c.beginPath(); c.moveTo(0, y); c.lineTo(s, y); c.stroke(); for (let x = (y / 32) % 2 ? 0 : 32; x < s; x += 64) { c.beginPath(); c.moveTo(x, y); c.lineTo(x, y + 32); c.stroke(); } } }, 3)
        : canvasTex(256, (c, s) => { c.fillStyle = L.ground; c.fillRect(0, 0, s, s); const rr = rng(5); for (let i = 0; i < 1600; i++) { c.globalAlpha = 0.2 + rr() * 0.5; c.fillStyle = L.dots[(rr() * L.dots.length) | 0]; c.beginPath(); c.arc(rr() * s, rr() * s, 0.6 + rr() * 1.8, 0, PI * 2); c.fill(); } }, 3);
  const isl = new THREE.Mesh(new THREE.CylinderGeometry(R, R * 0.86, 1.0, 72), [std({ color: L.side, roughness: 1 }), std({ map: topTex, roughness: 0.95 }), std({ color: L.side })]);
  isl.position.y = -0.5;
  isl.receiveShadow = true;
  g.add(isl);
  // a darker rim band
  add(g, new THREE.TorusGeometry(R - 0.02, 0.07, 6, 72), L.deco === 'grid' ? std({ color: L.accent, emissive: L.accent, emissiveIntensity: 1 }) : mat(L.side), 0, -0.02, 0, { rx: PI / 2, shadow: false });
  fence(g, L);
  bed(g, L);
  toilet(g, L);
  feeder(g, L);
  gym(g, L);
  lampPost(g, L);
  DECO[L.deco](g, L, rand);
}

function fence(g, L) {
  const posts = [];
  const N = 34;
  for (let i = 0; i < N; i++) {
    const a = (i / N) * PI * 2;
    if (Math.abs(Math.atan2(Math.sin(a - PI / 2), Math.cos(a - PI / 2))) < 0.5) continue; // gap at the front
    posts.push(a);
  }
  const m = L.deco === 'grid' ? std({ color: L.fence, emissive: L.fence, emissiveIntensity: 0.8 }) : mat(L.fence);
  const rr = R - 0.28;
  for (const a of posts) cyl(g, 0.06, 0.07, 0.62, m, Math.cos(a) * rr, 0, Math.sin(a) * rr, { seg: 6 });
  for (let i = 0; i < posts.length - 1; i++) {
    const a = posts[i], b = posts[i + 1];
    if (b - a > (2.2 * PI * 2) / N) continue;
    const mx = Math.cos((a + b) / 2) * rr, mz = Math.sin((a + b) / 2) * rr;
    const len = 2 * rr * Math.sin((b - a) / 2);
    for (const y of [0.25, 0.48]) box(g, len, 0.05, 0.04, m, mx, y, mz, { ry: -(a + b) / 2 + PI / 2, shadow: false });
  }
}

function bed(g, L) {
  const [x, z] = SPOT.bed;
  const b = new THREE.Group();
  b.position.set(x, 0, z);
  g.add(b);
  box(b, 2.3, 0.18, 1.7, '#9a6a3e', 0, 0, 0.35);
  box(b, 2.15, 0.14, 1.55, L.deco === 'grid' ? '#1a3a66' : '#f4ecd8', 0, 0.18, 0.35);
  box(b, 2.15, 0.06, 0.9, L.accent, 0, 0.32, 0.68, { shadow: false });
  for (const px of [-0.62, 0, 0.62]) box(b, 0.5, 0.14, 0.32, '#ffffff', px, 0.32, -0.22);
  box(b, 2.3, 0.75, 0.12, '#8a5a34', 0, 0, -0.5); // headboard
  sph(b, 0.12, mat('#ffd36a', { emissive: '#ffb030', emissiveIntensity: 0.4 }), 0.9, 0.95, -0.5, { seg: 10 });
  sign(b, '💤', -0.95, 0.75, -0.42);
}

function toilet(g, L) {
  const [x, z] = SPOT.toilet;
  const t = new THREE.Group();
  t.position.set(x, 0, z);
  g.add(t);
  const tiles = canvasTex(64, (c, s) => { c.fillStyle = '#ffffff'; c.fillRect(0, 0, s, s); c.fillStyle = '#9fd4ff'; c.fillRect(0, 0, s / 2, s / 2); c.fillRect(s / 2, s / 2, s / 2, s / 2); }, 4);
  add(t, new THREE.BoxGeometry(1.7, 0.04, 1.5), std({ map: tiles, roughness: 0.4 }), 0, 0.02, 0.05, { shadow: false });
  box(t, 1.7, 0.95, 0.08, '#cfe6f5', 0, 0, -0.7); // walls
  box(t, 0.08, 0.95, 1.5, '#cfe6f5', 0.85, 0, 0.05);
  cyl(t, 0.27, 0.2, 0.32, '#ffffff', 0, 0.04, -0.2, { seg: 20 });
  add(t, new THREE.TorusGeometry(0.24, 0.05, 8, 20), mat('#ffffff'), 0, 0.37, -0.2, { rx: PI / 2 });
  add(t, new THREE.CircleGeometry(0.2, 20), mat('#7fc8ff', { roughness: 0.2 }), 0, 0.33, -0.2, { rx: -PI / 2, shadow: false });
  box(t, 0.5, 0.45, 0.2, '#ffffff', 0, 0.2, -0.55);
  cyl(t, 0.09, 0.09, 0.16, '#ffffff', 0.62, 0.45, -0.35, { rz: PI / 2, seg: 12 }); // paper roll
  sign(t, '🚽', -0.6, 0.85, -0.62);
}

function feeder(g, L) {
  const [x, z] = SPOT.food;
  const f = new THREE.Group();
  f.position.set(x, 0, z);
  g.add(f);
  box(f, 1.3, 0.3, 0.6, '#9a6a3e', 0, 0, 0);
  box(f, 1.15, 0.05, 0.46, '#5a3a20', 0, 0.27, 0, { shadow: false });
  // the meat tray shows the free meat left
  for (let i = 0; i < F.MEAT_MAX; i++) { const m = meatMesh(); m.position.set(-0.38 + i * 0.38, 0.4, 0); m.rotation.y = 0.4 * (i - 1); f.add(m); S.meat.push(m); }
  cyl(f, 0.04, 0.04, 0.9, '#7a5030', -0.75, 0, -0.2, { seg: 6 });
  sign(f, '🍖', -0.75, 0.95, -0.2);
}

function gym(g, L) {
  const [x, z] = SPOT.train;
  const t = new THREE.Group();
  t.position.set(x, 0, z);
  g.add(t);
  add(t, new THREE.CircleGeometry(1.15, 32), mat(L.deco === 'snowy' ? '#dfe8f3' : '#e3c98f'), 0, 0.015, 0, { rx: -PI / 2, shadow: false });
  const log = cyl(t, 0.22, 0.25, 1.1, '#8a5a34', 0.45, 0, -0.35, { seg: 14 });
  for (const y of [-0.25, 0.1, 0.35]) add(log, new THREE.TorusGeometry(0.245, 0.03, 6, 16), mat('#d9c38f'), 0, y, 0, { rx: PI / 2 });
  add(t, new THREE.TorusGeometry(0.3, 0.12, 10, 20), mat('#2a2a2e'), -0.5, 0.12, 0.35, { rx: PI / 2 });
  const db = new THREE.Group();
  db.position.set(-0.35, 0.1, -0.45);
  db.rotation.y = 0.5;
  t.add(db);
  cyl(db, 0.03, 0.03, 0.6, '#9aa3b0', 0, -0.3, 0, { rz: PI / 2, seg: 8 }).position.set(0, 0, 0);
  sph(db, 0.11, '#3a3f4a', -0.3, 0, 0, { seg: 10 });
  sph(db, 0.11, '#3a3f4a', 0.3, 0, 0, { seg: 10 });
  sign(t, '💪', 0.9, 0.85, -0.6);
}

function lampPost(g, L) {
  const [x, z] = SPOT.lamp;
  cyl(g, 0.06, 0.08, 2.0, '#3a3f4a', x, 0, z, { seg: 8 });
  S.lampBulb = sph(g, 0.2, std({ color: '#fff3c4', emissive: '#ffb84a', emissiveIntensity: 0.2 }), x, 2.1, z, { seg: 14, shadow: false });
  cyl(g, 0.24, 0.12, 0.12, '#3a3f4a', x, 2.22, z, { seg: 10 });
}

function sign(parent, emoji, x, y, z) {
  const tex = canvasTex(64, (c, s) => { c.font = `${s * 0.78}px serif`; c.textAlign = 'center'; c.textBaseline = 'middle'; c.fillText(emoji, s / 2, s / 2 + 3); });
  tex.wrapS = tex.wrapT = THREE.ClampToEdgeWrapping;
  const sp = new THREE.Sprite(new THREE.SpriteMaterial({ map: tex, depthWrite: false }));
  sp.scale.setScalar(0.42);
  sp.position.set(x, y, z);
  parent.add(sp);
  return sp;
}

function meatMesh() {
  const m = new THREE.Group();
  const bone = mat('#fff6e6');
  const c = new THREE.Mesh(new THREE.CylinderGeometry(0.035, 0.035, 0.42, 8).rotateZ(PI / 2), bone);
  m.add(c);
  for (const s of [-1, 1]) for (const o of [-1, 1]) { const k = new THREE.Mesh(new THREE.SphereGeometry(0.045, 8, 6), bone); k.position.set(s * 0.22, 0, o * 0.03); m.add(k); }
  const flesh = new THREE.Mesh(new THREE.SphereGeometry(0.13, 12, 8), mat('#b5532e', { roughness: 0.6 }));
  flesh.scale.set(1.25, 0.95, 0.95);
  m.add(flesh);
  m.traverse((o) => { if (o.isMesh) o.castShadow = true; });
  return m;
}
function foodMesh(food) {
  if (food === 'meat' || food === 'bigmeat') { const m = meatMesh(); if (food === 'bigmeat') m.scale.setScalar(1.6); return m; }
  const m = new THREE.Group();
  if (food === 'carrot') {
    const c = new THREE.Mesh(new THREE.ConeGeometry(0.08, 0.36, 10), mat('#ff8a2a')); c.rotation.z = PI / 2; m.add(c);
    const l = new THREE.Mesh(new THREE.ConeGeometry(0.06, 0.16, 6), mat('#4cbf4c')); l.rotation.z = -PI / 2; l.position.x = 0.24; m.add(l);
  } else if (food === 'sirloin') {
    const s = new THREE.Mesh(new THREE.CylinderGeometry(0.18, 0.18, 0.08, 14), mat('#a83a2a')); s.scale.set(1.3, 1, 0.9); m.add(s);
    const f = new THREE.Mesh(new THREE.TorusGeometry(0.18, 0.025, 6, 16), mat('#fff0e0')); f.rotation.x = PI / 2; f.scale.set(1.3, 0.9, 1); m.add(f);
  } else {
    const c = new THREE.Mesh(new THREE.CylinderGeometry(0.16, 0.16, 0.18, 16), mat('#ffd1e0')); m.add(c);
    const i = new THREE.Mesh(new THREE.CylinderGeometry(0.165, 0.165, 0.05, 16), mat('#ffffff')); i.position.y = 0.09; m.add(i);
    const ch = new THREE.Mesh(new THREE.SphereGeometry(0.05, 8, 6), mat('#e0203a')); ch.position.y = 0.16; m.add(ch);
  }
  return m;
}

// decoration sets by theme
const rimSpots = (rand, n, r0, r1, front = false) => {
  const out = [];
  for (let i = 0; i < n * 4 && out.length < n; i++) {
    const a = rand() * PI * 2;
    if (Math.sin(a) > (front ? 0.55 : 0.35)) continue; // keep the front open for the camera and the action bar
    const r = r0 + rand() * (r1 - r0);
    const x = Math.cos(a) * r, z = Math.sin(a) * r;
    if (r < R && Object.values(SPOT).some(([sx, sz]) => Math.hypot(x - sx, z - sz) < 1.7)) continue; // inside the fence: keep clear of the care spots
    out.push([x, z, a]);
  }
  return out;
};
function flowers(g, rand, cols, n) {
  for (let i = 0; i < n; i++) {
    const a = rand() * PI * 2, r = 3.6 + rand() * 1.4;
    const x = Math.cos(a) * r, z = Math.sin(a) * r;
    if (Math.abs(x - SPOT.bed[0]) < 1.4 && Math.abs(z - SPOT.bed[1]) < 1.3) continue;
    cyl(g, 0.012, 0.012, 0.16, '#3f8a3a', x, 0, z, { seg: 4, shadow: false });
    sph(g, 0.06, cols[i % cols.length], x, 0.18, z, { seg: 8, shadow: false });
  }
}
const DECO = {
  meadow(g, L, rand) {
    for (const [x, z] of rimSpots(rand, 9, 6.0, 7.5, true)) { cyl(g, 0.14, 0.2, 0.9, '#6b4526', x, -0.5, z, { seg: 8 }); sph(g, 0.75 + rand() * 0.3, '#3f9a45', x, 0.75, z, { seg: 12 }); }
    for (const [x, z] of rimSpots(rand, 4, 4.5, 4.9)) { cyl(g, 0.12, 0.17, 0.7, '#6b4526', x, 0, z, { seg: 8 }); sph(g, 0.55, '#4fae4a', x, 1.0, z, { seg: 12 }); sph(g, 0.4, '#5fc05a', x + 0.25, 1.3, z, { seg: 10 }); }
    add(g, new THREE.CircleGeometry(0.85, 28), mat('#5ab8f0', { roughness: 0.15, metalness: 0.2 }), -0.9, 0.02, -3.9, { rx: -PI / 2, shadow: false });
    for (const [x, z] of [[-1.6, -3.6], [-0.2, -4.4], [-1.4, -4.4]]) sph(g, 0.16, '#9aa3ad', x, 0.06, z, { seg: 6 });
    flowers(g, rand, ['#ff8fb0', '#ffe066', '#ffffff', '#b58cff'], 26);
  },
  tropical(g, L, rand) {
    const palm = (x, z, s) => {
      const p = new THREE.Group(); p.position.set(x, 0, z); p.scale.setScalar(s); g.add(p);
      for (let i = 0; i < 6; i++) cyl(p, 0.11 - i * 0.008, 0.13 - i * 0.008, 0.36, '#9a7448', Math.sin(i * 0.35) * 0.18 * i * 0.3, i * 0.34, 0, { seg: 8 });
      for (let i = 0; i < 6; i++) {
        const leaf = new THREE.Mesh(new THREE.ConeGeometry(0.22, 1.4, 4), mat('#3fae4a', { side: THREE.DoubleSide }));
        leaf.scale.set(1, 1, 0.25); leaf.position.set(0.3, 2.1, 0); leaf.rotation.set(0, (i / 6) * PI * 2, 1.25);
        const pv = new THREE.Group(); pv.position.set(0.3, 0, 0); pv.rotation.y = (i / 6) * PI * 2; leaf.position.set(0.55, 2.0, 0); leaf.rotation.set(0, 0, -1.2); pv.add(leaf); p.add(pv);
        leaf.castShadow = true;
      }
      for (let i = 0; i < 3; i++) sph(p, 0.09, '#6b4a2a', 0.3 + Math.cos(i * 2) * 0.1, 1.95, Math.sin(i * 2) * 0.1, { seg: 8 });
      S.ticks.push((dt, t) => { p.rotation.z = Math.sin(t * 0.8 + x) * 0.025; });
    };
    for (const [x, z] of rimSpots(rand, 6, 5.9, 7.2, true)) palm(x, z, 1 + rand() * 0.3);
    for (const [x, z] of rimSpots(rand, 3, 4.4, 4.9)) palm(x, z, 0.8);
    const shells = ['#f7c6c0', '#fff4e6', '#f2b48a'];
    for (let i = 0; i < 14; i++) { const a = rand() * PI * 2, r = 3.8 + rand() * 1.3; add(g, new THREE.SphereGeometry(0.08, 8, 5, 0, PI * 2, 0, PI / 2), shells[i % 3], Math.cos(a) * r, 0, Math.sin(a) * r, { shadow: false }); }
    add(g, new THREE.RingGeometry(R + 0.2, R + 1.2, 64), new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.35, depthWrite: false }), 0, -0.5, 0, { rx: -PI / 2, shadow: false });
    // beach umbrella
    cyl(g, 0.03, 0.03, 1.4, '#ffffff', -1.2, 0, -4.1, { seg: 6 });
    add(g, new THREE.ConeGeometry(0.75, 0.35, 8), mat('#ff6f6f'), -1.2, 1.5, -4.1);
  },
  snowy(g, L, rand) {
    const pine = (x, z, s, y0 = 0) => { cyl(g, 0.1 * s, 0.14 * s, 0.5 * s, '#5a3d28', x, y0, z, { seg: 6 }); for (let i = 0; i < 3; i++) { add(g, new THREE.ConeGeometry((0.75 - i * 0.18) * s, 0.8 * s, 9), mat('#2f6b4a'), x, y0 + (0.75 + i * 0.45) * s, z); add(g, new THREE.ConeGeometry((0.5 - i * 0.12) * s, 0.35 * s, 9), mat('#ffffff'), x, y0 + (1.05 + i * 0.45) * s, z, { shadow: false }); } };
    for (const [x, z] of rimSpots(rand, 9, 6.0, 7.4, true)) pine(x, z, 1.1 + rand() * 0.3, -0.5);
    for (const [x, z] of rimSpots(rand, 4, 4.4, 4.9)) pine(x, z, 0.8);
    // snowman
    sph(g, 0.34, '#ffffff', -1.0, 0.3, -4.0); sph(g, 0.24, '#ffffff', -1.0, 0.78, -4.0); sph(g, 0.17, '#ffffff', -1.0, 1.12, -4.0);
    add(g, new THREE.ConeGeometry(0.04, 0.2, 6), mat('#ff8a2a'), -1.0, 1.12, -3.8, { rx: PI / 2 });
    // snowfall
    const n = 260, pos = new Float32Array(n * 3);
    for (let i = 0; i < n; i++) { pos[i * 3] = (rand() - 0.5) * 16; pos[i * 3 + 1] = rand() * 8; pos[i * 3 + 2] = (rand() - 0.5) * 14; }
    const geo = new THREE.BufferGeometry(); geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
    const pts = new THREE.Points(geo, new THREE.PointsMaterial({ color: '#ffffff', size: 0.08, transparent: true, opacity: 0.9, depthWrite: false }));
    g.add(pts);
    S.ticks.push((dt, t) => { for (let i = 0; i < n; i++) { pos[i * 3 + 1] -= dt * (0.5 + (i % 5) * 0.12); pos[i * 3] += Math.sin(t + i) * dt * 0.1; if (pos[i * 3 + 1] < -0.4) pos[i * 3 + 1] = 8; } geo.attributes.position.needsUpdate = true; });
  },
  sewer(g, L, rand) {
    const pipeM = mat('#6f7d6a', { metalness: 0.4, roughness: 0.5 });
    for (const [x, z, a] of rimSpots(rand, 6, 5.6, 6.8, true)) {
      cyl(g, 0.32, 0.32, 1.6 + rand(), pipeM, x, -0.5, z, { seg: 14 });
      add(g, new THREE.TorusGeometry(0.34, 0.06, 6, 16), mat('#4f5a4a'), x, 0.6, z, { rx: PI / 2 });
    }
    // a big bent pipe at the back, dripping sludge
    add(g, new THREE.TorusGeometry(1.2, 0.25, 10, 24, PI), pipeM, -0.6, 0.2, -5.3, { shadow: true });
    for (const [x, z, r] of [[-1.1, -3.8, 0.7], [0.4, 3.9, 0.45], [-4.3, 0.2, 0.4]]) add(g, new THREE.CircleGeometry(r, 20), mat('#7fc23a', { roughness: 0.15, emissive: '#3a6a10', emissiveIntensity: 0.4 }), x, 0.02, z, { rx: -PI / 2, shadow: false });
    cyl(g, 0.45, 0.45, 0.04, '#3a3f3a', 0.6, 0, -3.7, { seg: 20 }); // manhole
    const drips = [];
    for (let i = 0; i < 6; i++) drips.push(sph(g, 0.05, mat('#9fe04a', { emissive: '#5a9a20', emissiveIntensity: 0.6 }), -1.6 + i * 0.4, 0.2, -5.1, { seg: 6, shadow: false }));
    S.ticks.push((dt, t) => drips.forEach((d, i) => { d.position.y = 1.2 - (((t * 0.8 + i * 0.37) % 1.4)); }));
  },
  toy(g, L, rand) {
    const cols = ['#ff6f6f', '#ffd36a', '#6fc8ff', '#9fe07a', '#c08bff'];
    for (const [x, z] of rimSpots(rand, 10, 5.8, 7.0, true)) { const s = 0.5 + rand() * 0.5; box(g, s, s, s, cols[(rand() * 5) | 0], x, -0.5, z, { ry: rand() * 3 }); box(g, s * 0.7, s * 0.7, s * 0.7, cols[(rand() * 5) | 0], x, -0.5 + s, z, { ry: rand() * 3 }); }
    for (const [x, z] of rimSpots(rand, 4, 4.4, 4.9)) { cyl(g, 0.07, 0.07, 1.1, '#ffffff', x, 0, z, { seg: 8 }); add(g, new THREE.TorusGeometry(0.18, 0.07, 8, 16, PI), mat('#ff5a7a'), x + 0.18, 1.1, z); }
    sph(g, 0.3, '#ff6f6f', -1.0, 0.3, -3.9); // ball
    add(g, new THREE.TorusGeometry(0.3, 0.04, 6, 20), mat('#ffffff'), -1.0, 0.3, -3.9);
    flowers(g, rand, ['#ff8fb0', '#7f8cff', '#ffe066'], 14);
  },
  grid(g, L, rand) {
    const cm = std({ color: '#7ff3ff', emissive: '#1ac8ff', emissiveIntensity: 0.9, roughness: 0.2 });
    const crystals = [];
    for (const [x, z] of rimSpots(rand, 10, 5.8, 7.2, true)) crystals.push(add(g, new THREE.OctahedronGeometry(0.35 + rand() * 0.25, 0), cm, x, 0.8 + rand(), z, { shadow: false }));
    for (const [x, z] of rimSpots(rand, 4, 4.4, 4.9)) box(g, 0.5, 1.2, 0.5, std({ color: '#0f2a4c', emissive: '#38e8ff', emissiveIntensity: 0.25 }), x, 0, z);
    S.ticks.push((dt, t) => crystals.forEach((c, i) => { c.rotation.y = t * 0.6 + i; c.position.y = 1.1 + Math.sin(t * 1.4 + i) * 0.2; }));
  },
};

// ---------- legends in the room ----------
function makeAgent(id) {
  const c = F.card(id);
  const a = { id, key: c.key, root: new THREE.Group(), view: null, x: 0, z: 0, tx: 0, tz: 0, mode: 'idle', t: Math.random() * 2, busy: 0, data: c, tilt: 0, then: null, partner: null };
  a.view = DT.Legends.createView(c.key, { ring: false, height: 1.2, yaw: 0 });
  a.root.add(a.view.group);
  const shadow = new THREE.Mesh(new THREE.CircleGeometry(0.36, 20), new THREE.MeshBasicMaterial({ color: '#000000', transparent: true, opacity: 0.22, depthWrite: false }));
  shadow.rotation.x = -PI / 2;
  shadow.position.y = 0.02;
  a.shadow = shadow;
  S.scene.add(shadow);
  const hit = new THREE.Mesh(new THREE.CylinderGeometry(0.5, 0.5, 1.3, 10), new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false }));
  hit.position.y = 0.65;
  hit.userData.agent = id;
  a.root.add(hit);
  a.hit = hit;
  // spread new arrivals over the open grass
  const k = S.agents.size;
  const ang = k * 2.39996;
  a.x = WANDER.x + Math.cos(ang) * WANDER.rx * 0.7 * Math.sqrt((k + 1) / 12);
  a.z = WANDER.z + Math.sin(ang) * WANDER.rz * 0.7 * Math.sqrt((k + 1) / 12);
  a.root.position.set(a.x, 0, a.z);
  S.scene.add(a.root);
  a.bub = document.createElement('div');
  a.bub.className = 'df-bub';
  a.bub.innerHTML = `<span class="ic"></span><span class="nm"></span>`;
  $('.df-icons').appendChild(a.bub);
  a.shadowSet = false;
  return a;
}
function dropAgent(a) {
  a.view.dispose();
  S.scene.remove(a.root);
  S.scene.remove(a.shadow);
  a.shadow.geometry.dispose();
  a.hit.geometry.dispose();
  a.bub.remove();
}
function syncAgents() {
  ensureScene();
  const res = F.state().residents;
  for (const [id, a] of S.agents) if (!res.includes(id)) { dropAgent(a); S.agents.delete(id); }
  for (const id of res) {
    const c = F.card(id);
    let a = S.agents.get(id);
    if (a && a.key !== c.key) { // digivolved: new model, same spot
      const { x, z } = a;
      dropAgent(a);
      S.agents.delete(id);
      a = null;
      const n = makeAgent(id);
      n.x = x; n.z = z;
      S.agents.set(id, n);
      n.view.play('happy');
      continue;
    }
    if (!a) S.agents.set(id, makeAgent(id));
    else a.data = c;
  }
  S.hits = [...S.agents.values()].map((a) => a.hit);
}

function walkTo(a, x, z, then) { a.tx = x; a.tz = z; a.mode = 'walk'; a.then = then || null; }
function randomSpot() {
  const ang = Math.random() * PI * 2, r = Math.sqrt(Math.random());
  return [WANDER.x + Math.cos(ang) * WANDER.rx * r, WANDER.z + Math.sin(ang) * WANDER.rz * r];
}
function bedSpot(a) {
  const sleepers = [...S.agents.values()].filter((b) => b.mode === 'sleep' || (b.mode === 'walk' && b.goingToBed));
  const used = new Set(sleepers.filter((b) => b !== a).map((b) => b.bedSlot));
  for (let i = 0; i < BED_SLOTS.length; i++) if (!used.has(i)) return [i, SPOT.bed[0] + BED_SLOTS[i][0], SPOT.bed[1] + BED_SLOTS[i][1]];
  return [-1, a.x, a.z]; // bed is full: sleep where it stands
}

function updateAgent(a, dt) {
  const d = a.data;
  const v = a.view;
  if (!a.shadowSet && v.ready) { a.shadowSet = true; v.group.traverse((o) => { if (o.isMesh) o.castShadow = true; }); }
  // sleep
  if (d.asleep && a.mode !== 'sleep' && !a.goingToBed) {
    const [slot, x, z] = bedSpot(a);
    a.bedSlot = slot;
    a.goingToBed = true;
    walkTo(a, x, z, () => { a.goingToBed = false; a.mode = 'sleep'; v.yaw = 0; });
  } else if (!d.asleep && (a.mode === 'sleep' || a.goingToBed)) { a.goingToBed = false; a.mode = 'idle'; a.t = 0.6; v.play('happy'); }
  let speed = d.sick ? 0.55 : d.energy < 20 ? 0.7 : 1.15;
  if (a.goingToBed) speed = 1.3;
  if (a.busy > 0) { a.busy -= dt; v.setMoving(false); }
  else if (a.mode === 'walk') {
    const dx = a.tx - a.x, dz = a.tz - a.z, dist = Math.hypot(dx, dz);
    if (dist < 0.06) { a.mode = 'idle'; a.t = 1.5 + Math.random() * 3; v.setMoving(false); const fn = a.then; a.then = null; if (fn) fn(); }
    else {
      const step = Math.min(dist, speed * dt);
      a.x += (dx / dist) * step;
      a.z += (dz / dist) * step;
      v.yaw = Math.atan2(dx, dz);
      v.setMoving(true);
    }
  } else if (a.mode === 'idle') {
    v.setMoving(false);
    a.t -= dt;
    if (a.t <= 0) {
      const roll = Math.random();
      if (d.tantrum) { v.play('taunt'); a.t = 1.4; pop(a, '💢'); }
      else if (d.sick) { if (roll < 0.4) v.play('sad'); else walkTo(a, ...randomSpot()); a.t = 2 + Math.random() * 3; }
      else if (roll < 0.55) walkTo(a, ...randomSpot());
      else if (roll < 0.7 && d.happy >= 60) { v.play('happy'); a.t = 2; }
      else if (roll < 0.78 && d.happy < 30) { v.play('sad'); a.t = 2.5; }
      else if (roll < 0.9 && d.happy >= 40 && d.energy >= 25) friendPlay(a);
      else { v.yaw = Math.atan2(-a.x, 8 - a.z) + (Math.random() - 0.5) * 0.8; a.t = 2 + Math.random() * 2; } // look toward the camera
    }
  }
  // gentle push apart so nobody stands inside another
  if (a.mode !== 'sleep' && !a.goingToBed) for (const b of S.agents.values()) {
    if (b === a || b.mode === 'sleep') continue;
    const dx = a.x - b.x, dz = a.z - b.z, dd = Math.hypot(dx, dz);
    if (dd > 0.001 && dd < 0.75) { a.x += (dx / dd) * (0.75 - dd) * dt * 2; a.z += (dz / dd) * (0.75 - dd) * dt * 2; }
  }
  const rr = Math.hypot(a.x, a.z);
  if (rr > R - 0.7) { a.x *= (R - 0.7) / rr; a.z *= (R - 0.7) / rr; }
  // pose: lie down to sleep, hop when walking without a run clip
  a.tilt += ((a.mode === 'sleep' ? 1 : 0) - a.tilt) * Math.min(1, dt * 4);
  const hop = a.mode === 'walk' && !v.hasRun && a.busy <= 0 ? Math.abs(Math.sin(S.t * 9 + a.x)) * 0.14 : 0;
  const breathe = a.mode === 'sleep' ? 1 + Math.sin(S.t * 2 + a.z) * 0.03 : 1;
  a.root.position.set(a.x, hop + a.tilt * 0.22, a.z);
  a.root.rotation.set(0, 0, a.tilt * 1.15);
  a.root.scale.set(1, breathe, 1);
  a.shadow.position.set(a.x, 0.02, a.z);
  v.update(dt);
}

function friendPlay(a) {
  const others = [...S.agents.values()].filter((b) => b !== a && b.mode === 'idle' && !b.data.asleep && !b.data.sick && b.busy <= 0);
  if (!others.length) { a.t = 1; return; }
  const b = others[(Math.random() * others.length) | 0];
  const mx = (a.x + b.x) / 2, mz = (a.z + b.z) / 2;
  const ang = Math.atan2(b.z - a.z, b.x - a.x);
  walkTo(a, mx - Math.cos(ang) * 0.4, mz - Math.sin(ang) * 0.4, () => playPair(a, b));
  walkTo(b, mx + Math.cos(ang) * 0.4, mz + Math.sin(ang) * 0.4);
}
function playPair(a, b) {
  for (const [p, q] of [[a, b], [b, a]]) {
    if (!q) { p.view.play('dance'); p.busy = 2.4; continue; }
    p.view.yaw = Math.atan2(q.x - p.x, q.z - p.z);
    p.view.play(Math.random() < 0.5 ? 'happy' : 'dance');
    p.busy = 2.4;
    p.mode = 'idle';
  }
  pop(a, '♪');
}

// ---------- poop piles ----------
function pileMesh() {
  const g = new THREE.Group();
  const m = mat('#7a4a22', { roughness: 0.55 });
  [[0.2, 0.08], [0.15, 0.2], [0.1, 0.3]].forEach(([r, y]) => { const s = new THREE.Mesh(new THREE.SphereGeometry(r, 14, 10), m); s.scale.y = 0.7; s.position.y = y; s.castShadow = true; g.add(s); });
  const tip = new THREE.Mesh(new THREE.ConeGeometry(0.06, 0.12, 8), m);
  tip.position.y = 0.39;
  g.add(tip);
  const hit = new THREE.Mesh(new THREE.CylinderGeometry(0.3, 0.3, 0.5, 8), new THREE.MeshBasicMaterial({ transparent: true, opacity: 0, depthWrite: false, colorWrite: false }));
  hit.position.y = 0.25;
  g.add(hit);
  g.userData.hit = hit;
  return g;
}
function syncPiles() {
  const piles = F.piles();
  const keep = new Set();
  for (const p of piles) {
    const k = `${p.id}|${p.k}`;
    keep.add(k);
    let m = S.piles.get(k);
    if (!m) {
      m = pileMesh();
      m.userData.hit.userData.pile = { id: p.id, k: p.k };
      m.scale.setScalar(0.01);
      m.userData.grow = 0;
      S.scene.add(m);
      S.piles.set(k, m);
    }
    m.position.set(p.x, 0, p.z);
    m.userData.flies = !!p.n;
  }
  for (const [k, m] of S.piles) if (!keep.has(k)) {
    S.piles.delete(k);
    m.userData.dying = 0;
    S.dying = S.dying || [];
    S.dying.push(m);
  }
}
function syncMeat(n) { S.meat.forEach((m, i) => { m.visible = i < n; }); }

// ---------- bubbles and pops ----------
const tmpV = new THREE.Vector3();
function project(x, y, z) {
  tmpV.set(x, y, z).project(S.cam);
  const c = S.r.domElement;
  return { x: (tmpV.x * 0.5 + 0.5) * c.clientWidth, y: (-tmpV.y * 0.5 + 0.5) * c.clientHeight, behind: tmpV.z > 1 };
}
function headY(a) { return (a.view.height || 1) * (a.mode === 'sleep' ? 0.55 : 1) + 0.25; }
function pop(a, text, cls = '') {
  if (!root || !a) return;
  const p = project(a.x, headY(a) + 0.3, a.z);
  const el = document.createElement('div');
  el.className = `df-pop ${cls}`;
  el.textContent = text;
  el.style.left = `${p.x}px`;
  el.style.top = `${p.y}px`;
  $('.df-icons').appendChild(el);
  setTimeout(() => el.remove(), 1500);
}
function rewardPop(id, rw) {
  if (!rw || (!rw.bits && !rw.data)) return;
  const a = S.agents.get(id);
  setTimeout(() => pop(a, [rw.bits ? `+${rw.bits} Bits` : '', rw.data ? `+${rw.data} Data` : ''].filter(Boolean).join('  '), 'gold'), 350);
}

function setNight(on) {
  if (!S.r || S.night === on) { if (S.r) S.night = on; return; }
  S.night = on;
  $('.df-scene').classList.toggle('night', on);
}

// ---------- loop ----------
function loop(now) {
  S.raf = requestAnimationFrame(loop);
  const dt = Math.min(0.05, (now - (S.last || now)) / 1000);
  S.last = now;
  S.t += dt;
  const c = S.r.domElement;
  const w = c.clientWidth, h = c.clientHeight;
  if (!w || !h) return;
  const pr = S.r.getPixelRatio();
  if (c.width !== Math.round(w * pr) || c.height !== Math.round(h * pr)) { S.r.setSize(w, h, false); placeCamera(w, h); }
  // lights: ease toward day or night
  const k = Math.min(1, dt * 3);
  const L = LOOK[S.theme] || LOOK.meadow;
  const night = S.night;
  S.hemi.intensity += ((night ? 0.45 : L.deco === 'grid' || L.deco === 'sewer' ? 1.3 : 1.7) - S.hemi.intensity) * k;
  S.sun.intensity += ((night ? 0.12 : 2.3) - S.sun.intensity) * k;
  S.lamp.intensity += ((night ? 7 : 0) - S.lamp.intensity) * k;
  if (S.lampBulb) S.lampBulb.material.emissiveIntensity = night ? 1.6 : 0.2;
  for (const fn of S.ticks) fn(dt, S.t);
  for (const a of S.agents.values()) updateAgent(a, dt);
  // piles: pop in, flies, fade out
  for (const m of S.piles.values()) {
    if (m.userData.grow < 1) { m.userData.grow = Math.min(1, m.userData.grow + dt * 3); m.scale.setScalar(0.01 + m.userData.grow * (1 + Math.sin(m.userData.grow * PI) * 0.3)); }
    m.rotation.y = m.userData.flies ? Math.sin(S.t * 3) * 0.05 : 0;
  }
  if (S.dying) S.dying = S.dying.filter((m) => { m.userData.dying += dt * 4; m.scale.setScalar(Math.max(0.01, 1 - m.userData.dying)); m.position.y += dt; if (m.userData.dying >= 1) { S.scene.remove(m); return false; } return true; });
  if (S.food) S.food = S.food.filter((f) => { f.t -= dt; f.m.scale.setScalar(Math.max(0.01, Math.min(1, f.t / 0.5)) * (f.s || 1)); if (f.t <= 0) { S.scene.remove(f.m); return false; } return true; });
  // selection ring and bubbles
  const sa = S.agents.get(sel);
  S.ring.visible = !!sa;
  if (sa) { S.ring.position.set(sa.x, 0.03, sa.z); S.ring.scale.setScalar(1 + Math.sin(S.t * 4) * 0.05); }
  for (const a of S.agents.values()) {
    const p = project(a.x, headY(a), a.z);
    a.bub.style.left = `${p.x}px`;
    a.bub.style.top = `${p.y}px`;
    a.bub.classList.toggle('sel', a.id === sel);
    a.bub.classList.toggle('hov', a.id === hover);
    const ic = a.data.mood.icon;
    const icEl = a.bub.firstChild;
    if (icEl.textContent !== ic) { icEl.textContent = ic; icEl.classList.toggle('note', ic === '♪'); }
    if (a.bub.lastChild.textContent !== a.data.name) a.bub.lastChild.textContent = a.data.name;
  }
  S.r.render(S.scene, S.cam);
  // once a second: stats, events, card
  S.slow += dt;
  if (S.slow >= 1) { S.slow = 0; secondTick(); }
}

function secondTick() {
  for (const a of S.agents.values()) a.data = F.card(a.id) || a.data;
  handleEvents(F.drainEvents(), false);
  if (!game) render();
}

const MISTAKE_TEXT = { hungry: 'was hungry too long', sad: 'was sad too long', lights: 'was sleepy with the lights on', tantrum: 'had a tantrum nobody handled', sick: 'was sick too long' };
function handleEvents(evs, away) {
  if (!evs.length) return;
  if (away) {
    const n = (t) => evs.filter((e) => e.type === t).length;
    const parts = [n('poop') && `${n('poop')} poop${n('poop') > 1 ? 's' : ''}`, n('toilet') && `${n('toilet')} toilet trip${n('toilet') > 1 ? 's' : ''}`, n('sick') && `${n('sick')} got sick`, n('mistake') && `${n('mistake')} care mistake${n('mistake') > 1 ? 's' : ''}`].filter(Boolean);
    if (parts.length) toast(`While you were away: ${parts.join(', ')}.`, 4200);
    syncPiles();
    return;
  }
  for (const e of evs) {
    const a = S.agents.get(e.id);
    if (e.type === 'poop' && a) {
      const p = F.pet(e.id);
      const last = p && p.poop[p.poop.length - 1];
      if (last) F.setPilePos(e.id, last.k, a.x + 0.35, a.z + 0.2);
      pop(a, '💩');
      sfx('place');
    } else if (e.type === 'toilet' && a) walkTo(a, SPOT.toilet[0], SPOT.toilet[1] + 0.1, () => { pop(a, '🚽 ✓'); a.view.play('happy'); a.busy = 1.2; });
    else if (e.type === 'sick') toast(`${F.nameOf(e.id)} got sick${e.why === 'dirty' ? ' from the dirty room' : ''}. Use a bandage or medicine.`);
    else if (e.type === 'mistake') { toast(`Care mistake: ${F.nameOf(e.id)} ${MISTAKE_TEXT[e.why] || ''}.`); if (a) pop(a, '✗'); }
    else if (e.type === 'tantrum') { toast(`${F.nameOf(e.id)} is throwing a tantrum! Scold it.`); if (a) { a.view.play('taunt'); pop(a, '💢'); } }
  }
  syncPiles();
}

// ---------- actions ----------
function act(name, arg) {
  if (!sel) return null;
  const a = S.agents.get(sel);
  let r = null;
  if (name === 'feed') {
    r = F.feed(sel, arg || 'meat');
    if (r.ok && a) {
      const m = foodMesh(arg || 'meat');
      m.position.set(a.x + Math.sin(a.view.yaw) * 0.45, 0.25, a.z + Math.cos(a.view.yaw) * 0.45);
      S.scene.add(m);
      (S.food = S.food || []).push({ m, t: 1.6, s: arg === 'bigmeat' ? 1.6 : 1 });
      a.mode = 'idle'; a.busy = 1.6; a.view.play('eat');
      pop(a, F.FOODS[arg || 'meat'].icon);
      sfx('pick');
    } else if (r.refused && a) { a.view.play('taunt'); pop(a, '🙅'); sfx('error'); }
  } else if (name === 'clean') {
    r = F.clean(sel);
    if (r.ok) { sfx('sell'); for (const ag of S.agents.values()) if (ag.data.poop) ag.view.play('happy'); }
  } else if (name === 'cure') {
    r = F.cure(sel, arg || 'bandage');
    if (r.ok && a) { a.view.play('happy'); pop(a, arg === 'medicine' ? '💊' : '🩹'); sfx('heal'); }
  } else if (name === 'lights') {
    r = F.setLights();
    sfx(r.lights ? 'equip' : 'place');
  } else if (name === 'praise') {
    r = F.praise(sel);
    if (r.ok && a) { a.view.play(r.spoiled ? 'taunt' : 'happy'); pop(a, r.spoiled ? '😜' : '♥'); sfx('pick'); }
  } else if (name === 'scold') {
    r = F.scold(sel);
    if (r.ok && a) { a.view.play(r.unfair ? 'sad' : 'hit'); pop(a, r.unfair ? '💧' : '❗'); sfx(r.unfair ? 'error' : 'place'); }
  } else if (name === 'play') {
    const mates = [...S.agents.values()].filter((b) => b.id !== sel && !b.data.asleep && !b.data.sick);
    const mate = a && mates.length ? mates.reduce((x, y) => (Math.hypot(x.x - a.x, x.z - a.z) < Math.hypot(y.x - a.x, y.z - a.z) ? x : y)) : null;
    r = F.play(sel, mate ? mate.id : null);
    if (r.ok && a) {
      if (mate) { const ang = Math.atan2(mate.z - a.z, mate.x - a.x); const mx = (a.x + mate.x) / 2, mz = (a.z + mate.z) / 2; walkTo(a, mx - Math.cos(ang) * 0.4, mz - Math.sin(ang) * 0.4, () => playPair(a, mate)); walkTo(mate, mx + Math.cos(ang) * 0.4, mz + Math.sin(ang) * 0.4); }
      else playPair(a, null);
      sfx('emote');
    }
  }
  if (r) {
    toast(r.msg);
    if (r.ok) rewardPop(sel, r.reward);
    for (const ag of S.agents.values()) ag.data = F.card(ag.id) || ag.data;
    render();
  }
  return r;
}

// ---------- training ----------
function playGame(name, opts = {}) {
  if (!sel || game) return Promise.resolve(null);
  const why = F.canTrain(sel);
  if (why) { toast(why); return Promise.resolve({ score: 0, res: { ok: false, msg: why } }); }
  const id = sel;
  const a = S.agents.get(id);
  if (a) { a.goingToBed = false; walkTo(a, SPOT.train[0] - 0.3, SPOT.train[1] + 0.3, () => { a.view.yaw = 0.6; }); }
  menu = null;
  modalKind = 'game';
  const m = $('.df-modal');
  m.classList.remove('hidden');
  const box = m.querySelector('.df-mbox');
  box.innerHTML = '<button class="x" data-mclose="1">Quit</button><div class="df-game"></div>';
  game = G.run(name, box.querySelector('.df-game'), { ...opts, ownKeys: false });
  render();
  return game.done.then((score) => {
    game = null;
    if (score < 0) { modalKind = null; renderModal(); render(); return { score: 0, res: null, cancelled: true }; }
    const res = F.train(id, name, score);
    const t = F.TRAIN[name];
    box.innerHTML = `<button class="x" data-mclose="1">Done</button><div class="dg-result">
      <div class="muted">${G.GAMES[name].title} · ${esc(F.nameOf(id))}</div>
      <b>${score >= 0.9 ? 'Excellent!' : score >= 0.7 ? 'Great!' : score >= 0.4 ? 'Good' : 'Keep practicing'}</b> <span class="muted">score ${Math.round(score * 100)}</span>
      <div class="gains">${res.ok ? Object.entries(res.gains).map(([k, v]) => `<span class="df-tag good">${{ str: 'Strength', spd: 'Speed', wis: 'Wisdom', sta: 'Stamina' }[k]} +${v}</span>`).join('') : `<span class="df-tag bad">${esc(res.msg)}</span>`}
        ${res.reward && res.reward.bits ? `<span class="df-tag">+${res.reward.bits} Bits</span>` : ''}${res.reward && res.reward.data ? `<span class="df-tag">+${res.reward.data} Digi-Data</span>` : ''}</div>
      <div class="muted" style="font-size:12px">Training used energy and hunger and trimmed a little weight.</div>
      <div style="display:flex;gap:8px;justify-content:center;margin-top:10px"><button class="primary" data-again="${name}" ${F.canTrain(id) ? 'disabled' : ''}>Again</button><button data-mclose="1">Done</button></div></div>`;
    if (a) { a.view.play(score >= 0.6 ? 'happy' : 'sad'); a.busy = 1.5; }
    sfx(score >= 0.6 ? 'win' : 'loss');
    for (const ag of S.agents.values()) ag.data = F.card(ag.id) || ag.data;
    render();
    return { score, res };
  });
}

// ---------- input ----------
function pickAt(e) {
  const c = S.r.domElement.getBoundingClientRect();
  const v = new THREE.Vector2(((e.clientX - c.left) / c.width) * 2 - 1, -((e.clientY - c.top) / c.height) * 2 + 1);
  S.ray.setFromCamera(v, S.cam);
  const pileHits = [...S.piles.values()].map((m) => m.userData.hit);
  const hit = S.ray.intersectObjects([...S.hits, ...pileHits], false)[0];
  return hit ? hit.object.userData : null;
}
function onPointer(e) {
  if (!S.r) return;
  const u = pickAt(e);
  if (menu) { menu = null; renderMenu(); render(); }
  if (!u) return;
  if (u.pile) {
    const r = F.cleanPile(u.pile.id, u.pile.k);
    if (r.ok) { sfx('sell'); rewardPop(sel || u.pile.id, r.reward); syncPiles(); render(); }
    return;
  }
  if (u.agent) {
    select(u.agent);
    const a = S.agents.get(u.agent);
    const r = F.petIt(u.agent);
    if (a && !a.data.asleep) { a.view.play(a.data.tantrum ? 'taunt' : 'happy'); a.view.yaw = Math.atan2(-a.x, 8 - a.z); if (!r.small) pop(a, '♥'); a.busy = Math.max(a.busy, 0.8); }
    else if (a) pop(a, '💤');
    sfx('emote');
  }
}
function onMove(e) { if (!S.r) return; const u = pickAt(e); hover = u && u.agent ? u.agent : null; }

function onClick(e) {
  const b = e.target.closest('button');
  if (!b) { if (e.target === root) close(); return; }
  const d = b.dataset;
  if (b.classList.contains('df-close')) return close();
  if (d.fsel) return select(d.fsel);
  if (d.fa) {
    if (['feed', 'cure', 'train'].includes(d.fa)) { menu = menu === d.fa ? null : d.fa; return render(); }
    menu = null;
    return act(d.fa);
  }
  if (d.food) { menu = null; return act('feed', d.food); }
  if (d.cure) { menu = null; return act('cure', d.cure); }
  if (d.train) return playGame(d.train);
  if (d.again) { modalKind = null; return playGame(d.again); }
  if (d.mclose) { if (game) game.cancel(); modalKind = null; renderModal(); return render(); }
  if (d.fm === 'bank' || d.fm === 'themes') { menu = null; modalKind = d.fm; renderModal(); return render(); }
  if (d.fm === 'expand') { const r = F.buyExpansion(); toast(r.msg); if (r.ok) sfx('purchase'); return render(); }
  if (d.in) { const r = F.moveIn(d.in); toast(r.msg); renderModal(); return render(); }
  if (d.out) { const r = F.moveOut(d.out); toast(r.msg); renderModal(); return render(); }
  if (d.swap) { const r = F.swap(d.swap, sel); toast(r.msg); if (r.ok) sel = d.swap; renderModal(); return render(); }
  if (d.theme) { const r = F.buyTheme(d.theme); toast(r.msg); if (r.ok) sfx('equip'); renderModal(); return render(); }
}

function select(id) {
  if (!F.state().residents.includes(id)) return;
  sel = id;
  render();
}

// ---------- open / close ----------
function open(id) {
  build();
  if (DT.Shop && DT.Shop.isOpen && DT.Shop.isOpen()) DT.Shop.close();
  const st = F.state();
  const want = id && String(id).split('@')[0];
  sel = st.residents.includes(want) ? want : st.residents.includes(C.equipped('legend')) ? C.equipped('legend') : st.residents[0] || null;
  root.classList.remove('hidden');
  ensureScene();
  render();
  handleEvents(F.drainEvents(), true);
  if (!S.raf) { S.last = 0; S.raf = requestAnimationFrame(loop); }
  sfx('pick');
}
function close() {
  if (!root || root.classList.contains('hidden')) return;
  if (game) game.cancel();
  game = null;
  menu = null;
  modalKind = null;
  renderModal();
  root.classList.add('hidden');
  cancelAnimationFrame(S.raf);
  S.raf = 0;
  for (const a of S.agents.values()) dropAgent(a);
  S.agents.clear();
  for (const m of S.piles.values()) S.scene.remove(m);
  S.piles.clear();
}
const isOpen = () => !!root && !root.classList.contains('hidden');

document.addEventListener('click', (e) => {
  const b = e.target.closest && e.target.closest('[data-farm-open]');
  if (!b) return;
  e.preventDefault();
  open(b.getAttribute('data-farm-open') || undefined);
});
// while open, keys belong to the farm: Esc closes the top layer; a running mini-game gets the rest
window.addEventListener('keydown', (e) => {
  if (!isOpen()) return;
  if (DT.LegendEvo && DT.LegendEvo.isOpen && DT.LegendEvo.isOpen()) return; // the Digivolve panel on top handles its own keys
  e.stopImmediatePropagation();
  if (e.key === 'Escape') {
    e.preventDefault();
    if (game) { game.cancel(); return; }
    if (modalKind) { modalKind = null; renderModal(); render(); return; }
    if (menu) { menu = null; render(); return; }
    close();
    return;
  }
  if (game) game.key(e);
}, true);

let queued = false;
const later = () => { if (queued || !isOpen()) return; queued = true; requestAnimationFrame(() => { queued = false; if (!game) { render(); if (modalKind && modalKind !== 'game') renderModal(); } }); };
F.onChange(later);
W.onChange(later);
E.onChange(later);
window.addEventListener('resize', later);

DT.FarmUI = { open, close, isOpen, select, selected: () => sel, act, playGame, agents: () => [...S.agents.keys()], _scene: () => S };
