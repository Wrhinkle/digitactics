// Digimon portraits rendered at runtime from the same 3D models the board uses (or the placeholder creature).
// One small offscreen WebGL renderer works through a queue, one form at a time, so startup is not blocked.
// Results are cached as data URLs in memory and in localStorage, keyed by model file so new models re-render.
// tools/render-portraits.cjs pre-renders every form into models/portraits/; those files are used whenever they were
// made from the model the game has now, so the shop doesn't download every model just to draw its pictures.
import * as THREE from 'three';
import * as SkeletonUtils from 'three/addons/utils/SkeletonUtils.js';
import { lib, loadTemplate, makePlaceholder, familyColor } from './render3d.js';

const DT = window.DT;
let SIZE = 160;
const PDIR = 'models/portraits/';
let pre = {};
let preLoaded = false;
const preReady = location.protocol === 'file:' ? Promise.resolve() : fetch(`${PDIR}index.json`, { cache: 'no-cache' })
  .then((r) => (r.ok ? r.json() : {}))
  .then((j) => { pre = j || {}; })
  .catch(() => {})
  .finally(() => { preLoaded = true; });
let building = false;
let libLoaded = false;
lib.ready.then(() => { libLoaded = true; });
// pre-rendered picture for this form, if it was made from the current model
function preUrl(key) {
  if (building || !preLoaded || !libLoaded) return null;
  const p = pre[key];
  return p && p.src === (lib.index[key] || 'ph') ? PDIR + p.file : null;
}
const VER = 'p3';
const mem = new Map();
const queue = [];
const queued = new Set();
const urgent = new Set(); // asked for by something on screen; prefetches wait longer between renders
const listeners = new Set();
let ctx = null;
let busy = false;

const store = {
  get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(k, v) { try { localStorage.setItem(k, v); return true; } catch (e) { return false; } },
};

function setup() {
  const canvas = document.createElement('canvas');
  canvas.width = canvas.height = SIZE;
  const r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true, preserveDrawingBuffer: true });
  r.setPixelRatio(1);
  r.setSize(SIZE, SIZE, false);
  r.outputColorSpace = THREE.SRGBColorSpace;
  r.setClearColor(0x000000, 0);
  const scene = new THREE.Scene();
  scene.add(new THREE.HemisphereLight('#ffffff', '#3a4660', 2.2));
  const key = new THREE.DirectionalLight('#fff6e8', 2.4);
  key.position.set(1.5, 3, 4);
  scene.add(key);
  const rim = new THREE.DirectionalLight('#9fd0ff', 1.2);
  rim.position.set(-3, 2, -2);
  scene.add(rim);
  const cam = new THREE.PerspectiveCamera(28, 1, 0.01, 50);
  ctx = { canvas, r, scene, cam };
}

// Upper-body framing of an object that is 1 unit tall, standing on y = 0, facing +z.
function shoot(obj, footprint) {
  const { r, scene, cam, canvas } = ctx;
  scene.add(obj);
  obj.updateMatrixWorld(true);
  const view = Math.max(0.82, Math.min(1.25, footprint * 0.75));
  const ty = 1 - view * 0.5 + 0.04;
  const d = view / 2 / Math.tan((cam.fov * Math.PI) / 360);
  const yaw = 0.38;
  cam.position.set(Math.sin(yaw) * d, ty + d * 0.18, Math.cos(yaw) * d);
  cam.lookAt(0, ty, 0);
  r.render(scene, cam);
  scene.remove(obj);
  let url = canvas.toDataURL('image/webp', 0.86);
  if (!url.startsWith('data:image/webp')) url = canvas.toDataURL('image/png');
  return url;
}

// portrait key for a line's form: its model key (DT.modelKey), or 'ph~<lineId>~<stage>' for the placeholder creature
function pkey(id, stage) {
  const line = DT.LINE[id];
  if (!line || !DT.modelKey) return `${id}:${stage}`;
  return DT.modelKey(line, stage) || `ph~${id}~${stage}`;
}

async function render(key) {
  await lib.ready;
  await preReady;
  const pu = preUrl(key);
  if (pu) return pu;
  const [id, st] = key.startsWith('ph~') ? key.slice(3).split('~') : key.split(':');
  const line = DT.LINE[id];
  const file = lib.index[key] || 'ph';
  const sk = `dtp:${VER}:${key}:${file}`;
  const cached = building ? null : store.get(sk);
  if (cached) return cached;
  const t = file === 'ph' ? null : await loadTemplate(key);
  if (!ctx) setup();
  let url;
  if (t) {
    const obj = SkeletonUtils.clone(t.root);
    const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[key];
    if (src && src.rotY) obj.rotation.y = src.rotY;
    if (t.clip) { const m = new THREE.AnimationMixer(obj); m.clipAction(t.clip).play(); m.update(0); }
    url = shoot(obj, t.footprint);
  } else {
    const ph = makePlaceholder(line);
    url = shoot(ph, 1);
    ph.traverse((o) => { if (o.isMesh) o.material.dispose(); });
  }
  if (building) return url;
  if (!store.set(sk, url)) {
    // storage full: drop older portrait entries and try once more
    try { for (let i = localStorage.length - 1; i >= 0; i--) { const k = localStorage.key(i); if (k && k.startsWith('dtp:') && k !== sk) localStorage.removeItem(k); } } catch (e) { /* blocked */ }
    store.set(sk, url);
  }
  return url;
}

async function pump() {
  if (busy) return;
  busy = true;
  while (queue.length) {
    const key = queue.shift();
    let url = null;
    try { url = await render(key); } catch (e) { console.warn('Portrait failed', key, e); }
    queued.delete(key);
    const wasUrgent = urgent.delete(key);
    const isFile = url && url.startsWith(PDIR);
    if (url) {
      if (!isFile) url = await toBlobUrl(url);
      mem.set(key, url);
      for (const el of document.querySelectorAll(`.pt[data-pk="${key}"]`)) fill(el, url);
      for (const fn of listeners) fn(key, url);
    }
    const gap = isFile ? 0 : wasUrgent || (queue.length && urgent.has(queue[0])) ? 16 : 300;
    await new Promise((res) => setTimeout(() => (window.requestIdleCallback ? requestIdleCallback(res, { timeout: 150 }) : res()), gap));
  }
  busy = false;
}

// Short blob: URLs keep the panel markup small; the data URL stays in localStorage.
function toBlobUrl(url) {
  return fetch(url).then((r) => r.blob()).then((b) => URL.createObjectURL(b)).catch(() => url);
}

function fill(el, url) {
  if (el.querySelector('img')) return;
  const img = document.createElement('img');
  img.alt = '';
  img.src = url;
  el.appendChild(img);
}

// keys with a model but no line ('extra:<Rip>' spares, digivolved Little Legend forms) work too
const known = (key) => key.startsWith('ph~') || !!DT.LINE[key.split(':')[0]] || !!(lib.index && lib.index[key]);
function want(key, front) {
  if (mem.has(key) || !known(key)) return;
  const pu = preUrl(key);
  if (pu) { mem.set(key, pu); return; }
  if (front) urgent.add(key);
  if (queued.has(key)) {
    if (front) { const i = queue.indexOf(key); if (i > 0) { queue.splice(i, 1); queue.unshift(key); } }
    return;
  }
  queued.add(key);
  if (front) queue.unshift(key); else queue.push(key);
  pump();
}

const initials = (name) => name.replace(/[^A-Za-z]/g, '').slice(0, 2);

DT.Portraits = {
  get(id, stage = 0, front = true) { const k = pkey(id, stage); want(k, front); return mem.get(k) || null; },
  // markup with a family-colored fallback until the picture is ready
  html(id, stage = 0, cls = '', front = true) {
    const line = DT.LINE[id];
    if (!line && id !== 'extra' && !(lib.index && lib.index[`${id}:${stage}`])) return '';
    const k = pkey(id, stage);
    const url = this.get(id, stage, front);
    return `<span class="pt ${cls}" data-pk="${k}" style="--fc:${line ? familyColor(line) : '#5b6b8c'}"><b>${initials(line ? line.forms[stage] : String(stage))}</b>${url ? `<img src="${url}" alt="">` : ''}</span>`;
  },
  prefetch(keys) { for (const k of keys) want(k, false); },
  // portrait key for a line's form (model key, or a placeholder key for lines with no model)
  key: (id, stage = 0) => pkey(id, stage),
  // drop queued prefetches (a game started): model loads for pictures nobody is looking at would stall frames
  cancelPrefetch() { for (let i = queue.length - 1; i >= 0; i--) if (!urgent.has(queue[i])) { queued.delete(queue[i]); queue.splice(i, 1); } },
  onReady(fn) { listeners.add(fn); },
  // for tools/render-portraits.cjs: a fresh render as a data URL, ignoring caches and pre-rendered files
  async build(key, size) {
    building = true;
    if (size && size !== SIZE) { SIZE = size; ctx = null; }
    return render(key);
  },
};

