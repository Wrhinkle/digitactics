// three.js arena: hex board, bench, Digimon models (or placeholder creatures), combat effects.
// Same interface the UI used with the old 2D canvas renderer.
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import * as SkeletonUtils from 'three/addons/utils/SkeletonUtils.js';
import { MeshoptDecoder } from 'three/addons/libs/meshopt_decoder.module.js';
import { Fx, elemColor } from './fx.js';
import { Moves } from './moveprims.js';
import { itemCanvas } from './icons.js';
import { modelClips, borrowedClips } from './model-motion.js';
import { CutIn } from './cutin.js';

const DT = window.DT;
const sfx = (name, opts) => DT.Sfx && DT.Sfx.play(name, opts);
const { COLS, ROWS, BENCH } = DT;
const SQ3 = Math.sqrt(3);
const BOARD_HALF_Z = 1.5 * 3.5;
const BENCH_Z = BOARD_HALF_Z + 2.3;
const BENCH_STEP = 1.45;
const STAGE_HEIGHT = [1.3, 1.65, 2.0];
const MAX_FOOTPRINT = 1.9;
const ATTACK_MAX = 0.55;
const SPECIAL_MAX = 1.1;

// Item bench: 2 columns x 5 rows of pedestals left of the board, like TFT's item slots.
const ITEM_SLOTS = 10;
const itemWorld = (i) => ({ x: -7.5 - (i % 2) * 0.95, z: 1.2 + Math.floor(i / 2) * 1.5 });
// Loot orbs drop on your half of the board; orb.x / orb.z are 0-1 fractions.
const orbWorld = (o) => ({ x: -5.2 + o.x * 10.4, z: 0.9 + o.z * 4.2 });
const LEGEND_START = { x: -7.6, z: 8.4 };
const LEGEND_SPEED = 6;

const cellWorld = (c, r, cols = COLS) => ({ x: (c + 0.5 * (r & 1) - (cols - 0.5) / 2) * SQ3, z: (r - (ROWS - 1) / 2) * 1.5 });
const benchWorld = (i) => ({ x: (i - (BENCH - 1) / 2) * BENCH_STEP, z: BENCH_Z });
// board positions for js/boards.js (arena themes and their store previews)
export const LAYOUT = { cellWorld, benchWorld, itemWorld, BENCH_Z, BENCH_STEP, ITEM_SLOTS };
// placeholder color: the line's first non-role trait, looked up in the line's own set (cosmetics may show lines of other sets)
export const familyColor = (line) => {
  if (!line) return '#888888';
  const S = line.set && DT.Sets && DT.Sets.get(line.set);
  const T = (S && S.TRAITS) || DT.TRAITS;
  const fam = line.traits.find((t) => T[t] && T[t].kind !== 'role' && T[t].kind !== 'class') || line.traits.find((t) => T[t]);
  return line.color || (fam ? T[fam].color : '#888888');
};

// ---------- model library ----------
const loader = new GLTFLoader();
// tools/convert/optimize.mjs compresses the models with meshopt
loader.setMeshoptDecoder(MeshoptDecoder);
// three.js dropped KHR_materials_pbrSpecularGlossiness; some uploads keep their textures only there.
// Drawn unlit like the other game rips (their normals render flat grey under lights).
loader.register((parser) => ({
  name: 'KHR_materials_pbrSpecularGlossiness',
  getMaterialType(i) {
    return parser.json.materials[i].extensions?.KHR_materials_pbrSpecularGlossiness ? THREE.MeshBasicMaterial : null;
  },
  extendMaterialParams(i, params) {
    const ext = parser.json.materials[i].extensions?.KHR_materials_pbrSpecularGlossiness;
    if (!ext) return Promise.resolve();
    delete params.metalness;
    delete params.roughness;
    // Exporters often write a 0.5 diffuse factor with a full-color texture; keep the texture's own color.
    const f = ext.diffuseTexture ? [1, 1, 1, 1] : ext.diffuseFactor || [1, 1, 1, 1];
    params.color = new THREE.Color().setRGB(f[0], f[1], f[2], THREE.LinearSRGBColorSpace);
    return ext.diffuseTexture ? parser.assignTexture(params, 'map', ext.diffuseTexture, THREE.SRGBColorSpace) : Promise.resolve();
  },
}));
export const lib = { index: {}, templates: new Map(), loading: new Set() };
// Browsers refuse fetch on file://, so downloaded models only load through serve.cjs.
lib.ready = location.protocol === 'file:' ? Promise.resolve() : fetch('models/index.json', { cache: 'no-cache' })
  .then((r) => (r.ok ? r.json() : {}))
  .then((j) => { lib.index = j || {}; })
  .catch(() => {});

// Idle clip: named idle/wait/stand, or bn01 ("battle neutral") in the Cyber Sleuth rips.
const IDLE_RE = [/idle|wait|stand/i, /_bn01/i];
function pickIdle(clips) {
  for (const re of IDLE_RE) { const c = clips.find((x) => re.test(x.name)); if (c) return c; }
  return clips[0] || null;
}
// optimize.mjs keeps one clip per role and names it after the role
const ACTION_ROLES = ['attack', 'special', 'win'];

// Rips export effect cards as OPAQUE although the game blends them (MachGaogamon's
// jet glows, sword lights, Lucemon Satan Mode's circles). Effect-named textures with
// real opacity blend; opaque glow textures on a mostly black background add. Outside
// effect names only a cut-out atlas with soft edges blends. GltfMaterial.cs mirrors this.
const EFFECT_NAME = /(^|[_\s.])(eff|ef|glw|glow|lig)(?=$|[_\s.\d])/i;
// RGBA8 pixels -> shares of low alpha, fractional alpha and near-black texels.
function cardTextureStats(px) {
  const count = px.length / 4;
  let low = 0, mid = 0, dark = 0;
  for (let i = 0; i < px.length; i += 4) {
    if (px[i + 3] < 128) low++;
    if (px[i + 3] > 16 && px[i + 3] < 240) mid++;
    if (Math.max(px[i], px[i + 1], px[i + 2]) < 16) dark++;
  }
  return { low: low / count, mid: mid / count, dark: dark / count };
}
function effectCardMode(names, stats) {
  const effect = names.some((name) => EFFECT_NAME.test(name || ''));
  if (effect && stats.low >= 0.05) return 'blend';
  if (effect && stats.dark >= 0.45) return 'additive';
  if (!effect && stats.low >= 0.3 && stats.low <= 0.9 && stats.mid > 0.06 && stats.mid <= 0.25) return 'blend';
  return null;
}
const cardStats = new WeakMap();
function effectCard(m) {
  const image = m.map && m.map.image;
  if (!image || typeof document === 'undefined' || !image.width) return null;
  let stats = cardStats.get(image);
  if (!stats) {
    const canvas = document.createElement('canvas'); canvas.width = image.width; canvas.height = image.height;
    const c2d = canvas.getContext('2d', { willReadFrequently: true }); c2d.drawImage(image, 0, 0);
    stats = cardTextureStats(c2d.getImageData(0, 0, image.width, image.height).data); cardStats.set(image, stats);
  }
  return effectCardMode([m.name, m.map.name], stats);
}

export function prepareTemplate(gltf, src, borrowed = null) {
  const scene = gltf.scene;
  const clips = modelClips(gltf, src, borrowed);
  const idle = pickIdle(clips);
  // Measure in the idle pose: rest poses of game rips are often scaled or offset differently.
  if (idle) {
    const m = new THREE.AnimationMixer(scene);
    m.clipAction(idle).play();
    m.update(0);
  }
  scene.updateMatrixWorld(true);
  // posedBounds (js/models.js MODEL-OPTIONS): measure the skinned idle vertices, for an idle that reshapes the
  // bind pose (Seadramon's raised neck and coiled tail); the default box reads the bind-pose geometry bounds.
  const box = new THREE.Box3().setFromObject(scene, !!(src && src.posedBounds));
  const size = box.getSize(new THREE.Vector3());
  const h = Math.max(size.y, 1e-3);
  const wrapper = new THREE.Group();
  const inner = new THREE.Group();
  inner.add(scene);
  inner.scale.setScalar(1 / h);
  const center = box.getCenter(new THREE.Vector3());
  inner.position.set(-center.x / h, -box.min.y / h, -center.z / h);
  wrapper.add(inner);
  // GLTFLoader may share a material between RGB and RGBA geometries. Preserve an
  // independent original before the cutout pass mutates any of its other users.
  scene.traverse((o) => {
    if (!o.isMesh || o.geometry?.getAttribute('color')?.itemSize !== 4) return;
    o.material = Array.isArray(o.material) ? o.material.map(m => m.clone()) : o.material.clone();
  });
  scene.traverse((o) => {
    if (!o.isMesh) return;
    o.castShadow = true;
    o.frustumCulled = false;
    // Game rips mark everything as alpha-blended, which sorts badly and washes models out. Cut out instead.
    for (const m of [].concat(o.material)) {
      // frontOnly: models with an inverted-hull outline shell that hides the body when drawn double-sided.
      if (src && src.frontOnly) m.side = THREE.FrontSide;
      // No environment map in the scene, so metallic surfaces render near-black. Treat everything as non-metal.
      if (m.isMeshStandardMaterial) { m.metalness = 0; m.metalnessMap = null; }
      // Authored RGBA vertices contain opacity gradients that must survive the rip cutout correction.
      const card = !m.transparent && !m.alphaTest ? effectCard(m) : null;
      if (m.transparent && m.opacity >= 1 && m.map && o.geometry?.getAttribute('color')?.itemSize !== 4) { m.transparent = false; m.alphaTest = 0.5; m.depthWrite = true; }
      if (card) {
        m.transparent = true; m.depthWrite = false;
        // Glow cards add color but keep the target's alpha, so transparent portrait canvases get no box.
        if (card === 'additive') Object.assign(m, { blending: THREE.CustomBlending, blendEquation: THREE.AddEquation, blendSrc: THREE.SrcAlphaFactor,
          blendDst: THREE.OneFactor, blendSrcAlpha: THREE.ZeroFactor, blendDstAlpha: THREE.OneFactor });
      }
    }
  });
  const actions = {};
  if (idle) for (const role of ACTION_ROLES) { const c = clips.find((x) => x.name === role); if (c) actions[role] = c; }
  // These reviewed rigid-part rips now have an actual limb/wing gait. Expose it
  // to unitTemplate/CompanionView without changing other models' action maps.
  if (idle && ['rip-dw1_agumon', 'rip-dw1_airdramon', 'rip-dw1_akatorimon', 'rip-dw1_brachiomon'].includes(src?.uid)) {
    const run = clips.find(c => c.name === 'run'); if (run) actions.run = run;
  }
  return { root: wrapper, clip: idle, actions, footprint: Math.max(size.x, size.z) / h };
}

// undefined = still loading / not requested, null = no model
export function modelTemplate(key) {
  const file = lib.index[key];
  if (!file) return null;
  if (lib.templates.has(file)) return lib.templates.get(file);
  if (!lib.loading.has(file)) {
    lib.loading.add(file);
    loader.load(file,
      (g) => {
        // A clipless rip may borrow the authored clips of a rip with the same skeleton.
        const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[key], from = src && src.clipsFrom && lib.index[src.clipsFrom];
        if (!from) { lib.templates.set(file, prepareTemplate(g, src)); return; }
        loader.load(from, (s) => lib.templates.set(file, prepareTemplate(g, src, borrowedClips(s, DT.MODEL_SOURCES[src.clipsFrom]))),
          undefined, () => lib.templates.set(file, prepareTemplate(g, src)));
      },
      undefined,
      (e) => { console.warn('Model failed to load', file, e); lib.templates.set(file, null); });
  }
  return undefined;
}

// Promise of the template for "lineId:stage", or null when there is no model.
export function loadTemplate(key) {
  return lib.ready.then(() => new Promise((res) => {
    const poll = () => { const t = modelTemplate(key); if (t !== undefined) res(t); else setTimeout(poll, 120); };
    poll();
  }));
}

// ---------- item icons (canvas textures, cached per item) ----------
const iconCache = new Map();
function itemTexture(id) {
  if (iconCache.has(id)) return iconCache.get(id);
  const cv = itemCanvas(id);
  const tex = new THREE.CanvasTexture(cv);
  tex.colorSpace = THREE.SRGBColorSpace;
  if (cv.ready) cv.ready.then(() => { tex.needsUpdate = true; }); // the portrait loads after the canvas is made
  iconCache.set(id, tex);
  return tex;
}

// ---------- placeholder creature ----------
const GEO = {
  body: new THREE.SphereGeometry(0.5, 24, 18),
  eye: new THREE.SphereGeometry(0.09, 12, 10),
  pupil: new THREE.SphereGeometry(0.05, 10, 8),
  horn: new THREE.ConeGeometry(0.1, 0.32, 10),
  foot: new THREE.SphereGeometry(0.15, 12, 10),
  tail: new THREE.ConeGeometry(0.12, 0.45, 10),
  ring: new THREE.RingGeometry(0.58, 0.7, 40),
  disc: new THREE.CircleGeometry(0.58, 40),
  ball: new THREE.SphereGeometry(1, 16, 12),
  beam: new THREE.CylinderGeometry(0.06, 0.06, 1, 8, 1, true),
  burst: new THREE.RingGeometry(0.8, 1, 48),
};

export function makePlaceholder(line) {
  const g = new THREE.Group();
  const col = new THREE.Color(familyColor(line));
  const mat = (c, extra = {}) => new THREE.MeshStandardMaterial({ color: c, roughness: 0.55, metalness: 0.05, ...extra });
  const body = new THREE.Mesh(GEO.body, mat(col));
  body.scale.set(1, 0.95, 0.9);
  body.position.y = 0.5;
  const belly = new THREE.Mesh(GEO.body, mat(col.clone().lerp(new THREE.Color('#ffffff'), 0.55)));
  belly.scale.set(0.62, 0.62, 0.4);
  belly.position.set(0, 0.42, 0.25);
  g.add(body, belly);
  const white = mat('#ffffff');
  const black = mat('#111111');
  for (const s of [-1, 1]) {
    const eye = new THREE.Mesh(GEO.eye, white);
    eye.position.set(0.17 * s, 0.7, 0.38);
    const pupil = new THREE.Mesh(GEO.pupil, black);
    pupil.position.set(0.17 * s, 0.7, 0.46);
    const horn = new THREE.Mesh(GEO.horn, mat(DT.ATTR[line.attr].color));
    horn.position.set(0.24 * s, 1.0, -0.02);
    horn.rotation.z = -0.35 * s;
    const foot = new THREE.Mesh(GEO.foot, mat(col.clone().multiplyScalar(0.6)));
    foot.position.set(0.22 * s, 0.08, 0.12);
    foot.scale.set(1, 0.6, 1.3);
    g.add(eye, pupil, horn, foot);
  }
  const tail = new THREE.Mesh(GEO.tail, mat(col.clone().multiplyScalar(0.8)));
  tail.position.set(0, 0.35, -0.5);
  tail.rotation.x = -1.9;
  g.add(tail);
  g.traverse((o) => { if (o.isMesh) o.castShadow = true; });
  g.scale.setScalar(1 / 1.12); // normalize to ~1 unit tall
  return g;
}

// A cloned skinned model gets its own Skeleton, whose bone matrices live in a GPU texture until skeleton.dispose().
// Geometry and textures stay shared with the cached template, so only the skeletons are freed.
export function disposeSkeletons(obj) {
  if (!obj) return;
  const seen = new Set();
  obj.traverse((o) => { if (o.isSkinnedMesh && o.skeleton && !seen.has(o.skeleton)) { seen.add(o.skeleton); o.skeleton.dispose(); } });
}

// ---------- unit view ----------
export class UnitView {
  constructor(scene, labels, key, line, stage, opts = {}) {
    this.key = key;
    this.line = line;
    this.stage = stage;
    this.modelKey = DT.modelKey(line, stage); // null: no model assigned, the placeholder stays
    this.group = new THREE.Group();
    this.body = new THREE.Group();
    this.group.add(this.body);
    this.ringMat = new THREE.MeshBasicMaterial({ color: DT.TIER_COLORS[line.tier], transparent: true, opacity: 0.9, side: THREE.DoubleSide });
    this.ring = new THREE.Mesh(GEO.ring, this.ringMat);
    this.ring.rotation.x = -Math.PI / 2;
    this.ring.position.y = 0.16;
    this.discMat = new THREE.MeshBasicMaterial({ color: '#000000', transparent: true, opacity: 0.25 });
    const disc = new THREE.Mesh(GEO.disc, this.discMat);
    disc.rotation.x = -Math.PI / 2;
    disc.position.y = 0.155;
    this.group.add(disc, this.ring);
    this.yaw = Math.PI;
    this.alpha = 1;
    this.flash = 0;
    this.lunge = null;
    this.mixer = null;
    this.hasModel = false;
    this.height = opts.height || STAGE_HEIGHT[stage] + (line.tier === 5 ? 0.15 : 0);
    this.glow = null;
    this.setVisual(makePlaceholder(line), 1);
    this.tryModel();
    scene.add(this.group);

    this.label = document.createElement('div');
    this.label.className = 'ulabel';
    this.label.innerHTML = `<div class="ucast"></div><div class="uname">${line.forms[stage]}</div>
      <div class="bars"><div class="hp"><i class="h"></i><i class="s"></i></div><div class="mp"><i></i></div></div>
      <div class="stars s${stage}">${'★'.repeat(stage + 1)}</div><div class="items"></div>`;
    this.elItems = this.label.querySelector('.items');
    this.elCast = this.label.querySelector('.ucast');
    this.elHpBox = this.label.querySelector('.hp');
    this.castUntil = 0;
    this.tickTotal = 0;
    this.itemSig = '';
    this.elHp = this.label.querySelector('.hp .h');
    this.elSh = this.label.querySelector('.hp .s');
    this.elMp = this.label.querySelector('.mp i');
    labels.appendChild(this.label);
  }

  setVisual(obj, scale) {
    if (this.visual) this.body.remove(this.visual);
    this.visual = obj;
    obj.scale.multiplyScalar(scale);
    this.body.add(obj);
    this.materials = [];
    obj.traverse((o) => {
      if (!o.isMesh) return;
      o.material = Array.isArray(o.material) ? o.material.map((m) => m.clone()) : o.material.clone();
      for (const m of [].concat(o.material)) this.materials.push(m);
    });
    this.applyAlpha(true);
  }

  tryModel() {
    if (this.hasModel) return;
    const t = modelTemplate(this.modelKey);
    if (!t) { if (t === null) this.hasModel = 'none'; return; }
    const obj = SkeletonUtils.clone(t.root);
    const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[this.modelKey];
    if (src && src.rotY) obj.rotation.y = src.rotY;
    // maxFootprint (js/models.js MODEL-OPTIONS): lets a model with thin, wide wings spill past one tile instead of shrinking
    const scale = Math.min(this.height, ((src && src.maxFootprint) || MAX_FOOTPRINT) / Math.max(t.footprint, 0.01));
    this.setVisual(obj, scale);
    // yOffset (fraction of model height): lifts flyers whose idle clip swings below their feet
    if (src && src.yOffset) obj.position.y = src.yOffset * scale;
    if (t.clip) {
      this.mixer = new THREE.AnimationMixer(obj);
      const a = this.mixer.clipAction(t.clip);
      a.time = Math.random() * t.clip.duration;
      a.play();
      this.idleAction = a;
      this.clips = t.actions || {};
      this.mixer.addEventListener('finished', (e) => { if (e.action === this.oneShot) this.endOneShot(); });
    }
    this.hasModel = true;
  }

  // One-shot clip (attack, special, win) over the idle loop. Attacks squeeze into ATTACK_MAX seconds so fast
  // attackers don't fall behind; a special or win is never cut off by an attack.
  play(role) {
    const clip = this.clips && this.clips[role];
    if (!clip) return false;
    const cur = this.oneShotRole;
    if (cur && role === 'attack' && cur !== 'attack') return false;
    if (cur === role && role === 'attack' && this.oneShot.time < this.oneShot.getClip().duration * 0.6) return false;
    if (this.oneShot && this.oneShot.getClip() !== clip) this.oneShot.fadeOut(0.08);
    const a = this.mixer.clipAction(clip);
    a.reset();
    a.setLoop(role === 'win' ? THREE.LoopRepeat : THREE.LoopOnce, Infinity);
    a.clampWhenFinished = false;
    a.timeScale = role === 'attack' ? Math.max(1, clip.duration / ATTACK_MAX) : role === 'special' ? Math.max(1, clip.duration / SPECIAL_MAX) : 1;
    a.setEffectiveWeight(1);
    a.fadeIn(0.08).play();
    this.idleAction.fadeOut(0.08);
    this.oneShot = a;
    this.oneShotRole = role;
    return true;
  }

  endOneShot() {
    if (!this.oneShot) return;
    this.oneShot.fadeOut(0.15);
    this.idleAction.reset().fadeIn(0.15).play();
    this.oneShot = null;
    this.oneShotRole = null;
  }

  applyAlpha(force) {
    for (const m of this.materials) {
      const want = this.alpha < 1;
      if (force || m.userData.alpha !== this.alpha) {
        m.userData.alpha = this.alpha;
        if (m.userData.origTransparent == null) {
          m.userData.origTransparent = m.transparent;
          m.userData.origOpacity = m.opacity;
        }
        m.transparent = want || m.userData.origTransparent;
        m.opacity = m.userData.origOpacity * this.alpha;
        m.depthWrite = !want;
        m.needsUpdate = true;
      }
    }
    this.ringMat.opacity = 0.9 * this.alpha;
    this.discMat.opacity = 0.25 * this.alpha;
  }

  update(dt, now) {
    if (!this.hasModel) {
      this.tryModel();
      if (!this.hasModel) this.visual.scale.setScalar((this.height / 1.12) * (1 + 0.03 * Math.sin(now * 3 + this.group.id)));
    }
    if (this.mixer) this.mixer.update(dt);
    // smooth facing
    let d = this.yaw - this.body.rotation.y;
    d = Math.atan2(Math.sin(d), Math.cos(d));
    this.body.rotation.y += d * Math.min(1, dt * 10);
    // lunge toward target
    this.body.position.set(0, 0, 0);
    if (this.lunge) {
      const k = (now - this.lunge.t0) / this.lunge.dur;
      if (k >= 1) this.lunge = null;
      else {
        const s = Math.sin(Math.PI * k) * this.lunge.dist;
        this.body.position.set(this.lunge.dx * s, 0, this.lunge.dz * s);
      }
    }
    // hit flash, or the caster's element tint right after it casts
    const f = Math.max(0, this.flash - now);
    const gl = this.glow;
    const g = gl && now >= gl.t0 && now < gl.until ? (gl.until - now) / (gl.until - gl.t0) : 0;
    if (gl && now >= gl.until) this.glow = null;
    for (const m of this.materials) {
      if (!m.emissive) continue;
      if (m.userData.baseEmissive == null) m.userData.baseEmissive = m.emissive.getHex();
      if (f > 0) m.emissive.setRGB(f * 4, f * 4, f * 4);
      else if (g > 0) m.emissive.copy(gl.color).multiplyScalar(0.2 + 0.5 * g);
      else m.emissive.setHex(m.userData.baseEmissive);
    }
    this.ring.scale.setScalar(g > 0 ? 1 + 0.6 * Math.sin(Math.PI * Math.min(1, (1 - g) * 2)) : 1);
  }

  dispose(scene) {
    scene.remove(this.group);
    this.label.remove();
    disposeSkeletons(this.visual);
    for (const m of this.materials) m.dispose();
    this.ringMat.dispose();
    this.discMat.dispose();
  }
}

// ---------- renderer ----------
class Renderer3D {
  constructor(canvas) {
    this.canvas = canvas;
    this.arena = canvas.parentElement;
    this.r = new THREE.WebGLRenderer({ canvas, antialias: true });
    this.r.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
    this.r.shadowMap.enabled = true;
    this.r.shadowMap.type = THREE.PCFSoftShadowMap;
    this.r.outputColorSpace = THREE.SRGBColorSpace;
    this.scene = new THREE.Scene();
    this.scene.background = new THREE.Color('#0b1a2c');
    this.scene.fog = new THREE.Fog('#0b1a2c', 30, 60);
    this.camera = new THREE.PerspectiveCamera(38, 1.6, 0.1, 200);
    this.ray = new THREE.Raycaster();
    this.ground = new THREE.Plane(new THREE.Vector3(0, 1, 0), 0);
    this.views = new Map();
    this.fx = [];
    this.evIdx = 0;
    this.deathT = {};
    this.cbSerial = 0;
    this.ult = new Fx({ scene: this.scene, addMeshFx: (m, t0, dur, update) => this.addMeshFx(m, t0, dur, update) });
    // signature move effects (js/moves.js specs, js/moveprims.js primitives)
    this.moves = new Moves({ scene: this.scene, camera: this.camera, canvas: this.r.domElement, renderer: this.r, shake: (a, d) => this.ult.shake(a, d) });
    this.castCtx = null;
    this.clock = performance.now();
    this.time = 0;
    this.labels = document.createElement('div');
    this.labels.id = 'labels';
    this.arena.appendChild(this.labels);
    this.otLabel = document.createElement('div');
    this.otLabel.className = 'overtime hidden';
    this.labels.appendChild(this.otLabel);
    this.buildArena();
    // special cut-in and tracking shot (js/cutin.js); after the labels, so its names sit above them
    this.cutin = new CutIn(this);
  }

  buildArena() {
    const s = this.scene;
    // sky, lights, island, slabs and scenery come from the board theme (js/boards.js); these are the gameplay pieces
    // hex tiles
    const tileGeo = new THREE.CylinderGeometry(0.93, 0.97, 0.14, 6);
    this.tiles = [];
    for (const { c, r } of DT.CELLS) {
      const mine = r >= 4;
      const m = new THREE.MeshStandardMaterial({ color: mine ? '#30598c' : '#7a3442', roughness: 0.6, metalness: 0.1 });
      const t = new THREE.Mesh(tileGeo, m);
      const p = cellWorld(c, r);
      t.position.set(p.x, 0.07, p.z);
      t.receiveShadow = true;
      t.userData = { c, r, flash: 0, flashColor: new THREE.Color() };
      s.add(t);
      this.tiles.push(t);
    }
    // bench slots
    const slotGeo = new THREE.BoxGeometry(1.25, 0.1, 1.25);
    this.slots = [];
    for (let i = 0; i < BENCH; i++) {
      const m = new THREE.MeshStandardMaterial({ color: '#5a4b35', roughness: 0.7 });
      const t = new THREE.Mesh(slotGeo, m);
      const p = benchWorld(i);
      t.position.set(p.x, 0.05, p.z);
      t.receiveShadow = true;
      s.add(t);
      this.slots.push(t);
    }

    // item bench pedestals and their item tokens
    const pedGeo = new THREE.CylinderGeometry(0.42, 0.48, 0.16, 20);
    this.itemPeds = [];
    this.itemTokens = [];
    for (let i = 0; i < ITEM_SLOTS; i++) {
      const w = itemWorld(i);
      const ped = new THREE.Mesh(pedGeo, new THREE.MeshStandardMaterial({ color: '#39404f', roughness: 0.6, emissive: '#000000' }));
      ped.position.set(w.x, 0.08, w.z);
      ped.receiveShadow = true;
      s.add(ped);
      this.itemPeds.push(ped);
      const tok = new THREE.Sprite(new THREE.SpriteMaterial({ transparent: true, depthWrite: false }));
      tok.scale.set(0.78, 0.78, 1);
      tok.visible = false;
      tok.renderOrder = 5;
      s.add(tok);
      this.itemTokens.push(tok);
    }
    this.orbViews = new Map();
    this.orbGeo = new THREE.SphereGeometry(0.3, 20, 14);
    this.orbGlowGeo = new THREE.SphereGeometry(0.46, 20, 14);
    this.legend = null;
    if (DT.Boards) DT.Boards.attach(this);
  }

  // ---------- Little Legend ----------
  // the tactician is a 'legend' cosmetic (js/legends.js), a local choice that is never part of game state
  ensureLegend() {
    if (this.legend) return this.legend;
    const view = DT.Legends.createView(this.legendKey || DT.Cosmetics.equipped('legend'), { color: '#ffcf4a' });
    this.scene.add(view.group);
    const old = this.legendState;
    this.legend = { view, x: old ? old.x : LEGEND_START.x, z: old ? old.z : LEGEND_START.z, tx: null, tz: null, moving: false };
    this.legendState = this.legend;
    return this.legend;
  }

  setLegend(key) {
    if (key === this.legendKey) return;
    this.legendKey = key;
    if (this.legend) { this.legend.view.dispose(); this.legend = null; }
  }

  // The combat opponent's Little Legend at their corner, from their loadout (DT.Social.lookOf).
  updateOppLegend(game, dt) {
    const opp = game.phase !== 'planning' && game.roundType === 'pvp' && !game.isPvE ? game.opponentOf(game.human) : null;
    const q = opp && opp.player;
    const look = q && DT.Social.lookOf(q);
    const id = look && look.legend;
    let O = this.oppLegend;
    if (O && (!q || O.playerId !== q.id || O.legendId !== id)) { O.view.dispose(); O = this.oppLegend = null; }
    if (!q || !id) return;
    if (!O) {
      const view = DT.Legends.createView(id, { color: '#ff7a6a', yaw: 0 });
      view.group.position.set(-LEGEND_START.x, 0, -LEGEND_START.z + 1.8); // far corner, clear of the scenery
      this.scene.add(view.group);
      O = this.oppLegend = { view, playerId: q.id, legendId: id };
    }
    O.view.update(dt);
  }

  // Right-click destination for the Little Legend.
  moveLegend(x, y) {
    const p = this.groundAt(x, y);
    if (!p) return false;
    const L = this.ensureLegend();
    L.tx = THREE.MathUtils.clamp(p.x, -9.6, 9.6);
    L.tz = THREE.MathUtils.clamp(p.z, -0.4, 9.2);
    this.spawnClickMark(L.tx, L.tz);
    return true;
  }

  spawnClickMark(x, z) {
    const m = new THREE.Mesh(GEO.burst, new THREE.MeshBasicMaterial({ color: '#7fe0ff', transparent: true, opacity: 0.9, side: THREE.DoubleSide, depthWrite: false }));
    m.rotation.x = -Math.PI / 2;
    m.position.set(x, 0.2, z);
    m.scale.setScalar(0.5);
    this.scene.add(m);
    this.clickMarks = (this.clickMarks || []).concat([{ m, t0: this.time }]);
  }

  updateLegend(dt) {
    const L = this.ensureLegend();
    if (L.tx != null) {
      const dx = L.tx - L.x, dz = L.tz - L.z;
      const d = Math.hypot(dx, dz);
      const step = LEGEND_SPEED * dt;
      if (d <= step) { L.x = L.tx; L.z = L.tz; L.tx = null; L.moving = false; }
      else { L.x += (dx / d) * step; L.z += (dz / d) * step; L.moving = true; L.view.yaw = Math.atan2(dx, dz); }
    }
    L.view.setMoving(L.moving);
    const hop = L.moving && !L.view.hasRun ? Math.abs(Math.sin(this.time * 12)) * 0.18 : 0;
    L.view.group.position.set(L.x, hop, L.z);
    L.view.update(dt);
    for (const c of this.clickMarks || []) {
      const k = (this.time - c.t0) / 0.45;
      c.m.scale.setScalar(0.5 + k * 0.6);
      c.m.material.opacity = Math.max(0, 0.9 * (1 - k));
      if (k >= 1) { this.scene.remove(c.m); c.m.material.dispose(); c.dead = true; }
    }
    if (this.clickMarks) this.clickMarks = this.clickMarks.filter((c) => !c.dead);
  }

  legendPos() { const L = this.ensureLegend(); return { x: L.x, z: L.z }; }

  // ---------- loot orbs ----------
  syncOrbs(orbs) {
    const keep = new Set();
    for (const o of orbs) {
      keep.add(o.id);
      let v = this.orbViews.get(o.id);
      if (!v) {
        const color = o.kind === 'gold' ? '#ffcf4a' : '#4fb3ff';
        const g = new THREE.Group();
        const core = new THREE.Mesh(this.orbGeo, new THREE.MeshStandardMaterial({ color, emissive: color, emissiveIntensity: 0.8, roughness: 0.2 }));
        const glow = new THREE.Mesh(this.orbGlowGeo, new THREE.MeshBasicMaterial({ color, transparent: true, opacity: 0.25, depthWrite: false }));
        core.castShadow = true;
        g.add(core, glow);
        const w = orbWorld(o);
        g.position.set(w.x, 0.6, w.z);
        g.userData.phase = o.id * 1.7;
        this.scene.add(g);
        v = g;
        this.orbViews.set(o.id, v);
      }
      v.position.y = 0.6 + Math.sin(this.time * 2.5 + v.userData.phase) * 0.12;
      v.rotation.y += 0.02;
    }
    for (const [id, v] of this.orbViews) {
      if (keep.has(id)) continue;
      this.burstAt(v.position.x, v.position.z, v.children[0].material.color);
      this.scene.remove(v);
      v.traverse((m) => { if (m.material) m.material.dispose(); });
      this.orbViews.delete(id);
    }
  }

  orbWorld(o) { return orbWorld(o); }

  burstAt(x, z, color) {
    const m = new THREE.Mesh(GEO.burst, new THREE.MeshBasicMaterial({ color, transparent: true, opacity: 0.9, side: THREE.DoubleSide, depthWrite: false }));
    m.rotation.x = -Math.PI / 2;
    m.position.set(x, 0.3, z);
    m.scale.setScalar(0.4);
    this.scene.add(m);
    this.clickMarks = (this.clickMarks || []).concat([{ m, t0: this.time }]);
  }

  // ---------- item bench ----------
  drawItems(p, ui) {
    // The single-board shelf lies inside the combined battlefield. Hide it until planning returns.
    this.itemsVisible = !this.wide;
    for (let i = 0; i < ITEM_SLOTS; i++) {
      const tok = this.itemTokens[i];
      const id = p.items[i];
      const ped = this.itemPeds[i];
      ped.visible = this.itemsVisible;
      if (!this.itemsVisible) { tok.visible = false; continue; }
      ped.material.emissive.set((ui.hoverItem === i && !ui.itemDrag) || (ui.itemDrag && ui.itemDrag.over === i) ? '#2b4a6b' : '#000000');
      if (!id) { tok.visible = false; continue; }
      if (tok.userData.id !== id) {
        tok.material.map = itemTexture(id);
        tok.material.needsUpdate = true;
        tok.userData.id = id;
      }
      tok.visible = true;
      const dragging = ui.itemDrag && ui.itemDrag.idx === i && ui.itemDrag.x != null;
      if (dragging) {
        const g = this.groundAt(ui.itemDrag.x, ui.itemDrag.y);
        if (g) tok.position.set(g.x, 1.1, g.z);
        tok.scale.set(0.95, 0.95, 1);
      } else {
        const w = itemWorld(i);
        tok.position.set(w.x, 0.62 + Math.sin(this.time * 2 + i) * 0.05, w.z);
        const sc = ui.hoverItem === i || (ui.itemDrag && ui.itemDrag.over === i) ? 0.92 : 0.78;
        tok.scale.set(sc, sc, 1);
      }
    }
  }

  // which item token is under the cursor (screen-space, so the floating sprite is easy to grab)
  itemSlotAt(x, y, count) {
    if (this.itemsVisible === false) return -1;
    let best = -1;
    let bd = 30;
    for (let i = 0; i < Math.min(count, ITEM_SLOTS); i++) {
      const w = itemWorld(i);
      const sp = this.project(w.x, 0.62, w.z);
      const d = Math.hypot(sp.x - x, sp.y - y);
      if (d < bd) { bd = d; best = i; }
    }
    return best;
  }

  screenOfItemSlot(i) { const w = itemWorld(i); return this.project(w.x, 0.62, w.z); }
  screenOfWorld(x, z) { return this.project(x, 0.3, z); }

  // shared per-frame extras for planning and combat
  // p: whose item bench to show (the scouted player while scouting, see js/scout.js); their orbs stay hidden
  drawExtras(game, ui, dt, p = game.human) {
    this.updateLegend(dt);
    this.updateOppLegend(game, dt);
    const scouting = p !== game.human;
    for (const v of this.orbViews.values()) v.visible = !scouting;
    if (!scouting) this.syncOrbs(game.human.orbs);
    this.drawItems(p, ui);
    if (this.scoutTick) this.scoutTick(game, ui, dt);
  }

  // ---------- sizing & picking ----------
  fit(w, h) {
    w = this.arena.clientWidth;
    h = this.arena.clientHeight;
    this.r.setSize(w, h, true);
    this.camera.aspect = w / h;
    this.camera.updateProjectionMatrix();
    this.applyView();
  }

  // ---------- camera: orbit around a ground target ----------
  // dist/yaw/pitch describe the camera relative to target. Default matches the original fixed view.
  resetView() {
    this.view = { target: new THREE.Vector3(0, 0, 1.8), zoom: 1, yaw: 0, pitch: Math.atan2(16.5, 15.5) };
    this.applyView();
  }

  applyView() {
    if (!this.view) { this.resetView(); return; }
    const v = this.view;
    // pull back on narrow screens so board + bench stay in view
    const base = Math.hypot(16.5, 15.5) * Math.max(1, 1.35 / this.camera.aspect);
    const dist = base * v.zoom * (this.wide ? 1.65 : 1);
    this.camera.position.set(
      v.target.x + Math.sin(v.yaw) * Math.cos(v.pitch) * dist,
      v.target.y + Math.sin(v.pitch) * dist,
      v.target.z + Math.cos(v.yaw) * Math.cos(v.pitch) * dist,
    );
    this.camera.lookAt(v.target);
    this.camera.updateMatrixWorld();
  }

  // factor < 1 zooms in. Keeps the ground point under the cursor fixed where possible.
  zoomAt(x, y, factor) {
    const v = this.view;
    const before = this.groundAt(x, y);
    const z = THREE.MathUtils.clamp(v.zoom * factor, 0.22, 1.5);
    if (z === v.zoom) return;
    v.zoom = z;
    this.applyView();
    const after = this.groundAt(x, y);
    if (before && after) { v.target.add(before.sub(after)); this.clampTarget(); this.applyView(); }
  }

  // drag the ground: the point grabbed at (x0,y0) follows the cursor to (x1,y1)
  panBy(x0, y0, x1, y1) {
    const a = this.groundAt(x0, y0), b = this.groundAt(x1, y1);
    if (!a || !b) return;
    this.view.target.add(a.sub(b));
    this.clampTarget();
    this.applyView();
  }

  orbitBy(dx, dy) {
    const v = this.view;
    v.yaw -= dx * 0.008;
    v.pitch = THREE.MathUtils.clamp(v.pitch + dy * 0.006, 0.35, 1.45);
    this.applyView();
  }

  clampTarget() {
    const t = this.view.target;
    t.x = THREE.MathUtils.clamp(t.x, -8, 8);
    t.z = THREE.MathUtils.clamp(t.z, -8, 9);
  }

  toLocal(ev) {
    const rect = this.canvas.getBoundingClientRect();
    return { x: ev.clientX - rect.left, y: ev.clientY - rect.top };
  }

  groundAt(x, y) {
    const w = this.canvas.clientWidth;
    const h = this.canvas.clientHeight;
    this.ray.setFromCamera(new THREE.Vector2((x / w) * 2 - 1, -(y / h) * 2 + 1), this.camera);
    const p = new THREE.Vector3();
    return this.ray.ray.intersectPlane(this.ground, p) ? p : null;
  }

  cellAt(x, y) {
    const p = this.groundAt(x, y);
    if (!p) return null;
    let best = null;
    let bd = 0.95;
    for (const cell of DT.CELLS) {
      const q = cellWorld(cell.c, cell.r);
      const d = Math.hypot(q.x - p.x, q.z - p.z);
      if (d < bd) { bd = d; best = cell; }
    }
    return best;
  }

  benchAt(x, y) {
    const p = this.groundAt(x, y);
    if (!p || Math.abs(p.z - BENCH_Z) > 0.8) return -1;
    const bx = this.wide ? (p.x + 6.1) / 0.72 : p.x;
    const i = Math.round(bx / BENCH_STEP + (BENCH - 1) / 2);
    return i >= 0 && i < BENCH && Math.abs(this.benchPos(i).x - p.x) < 0.72 ? i : -1;
  }

  project(x, y, z) {
    const v = new THREE.Vector3(x, y, z).project(this.camera);
    return { x: (v.x * 0.5 + 0.5) * this.canvas.clientWidth, y: (-v.y * 0.5 + 0.5) * this.canvas.clientHeight, behind: v.z > 1 };
  }

  // screen positions (canvas px) of a tile or bench slot; used by tests
  screenOfCell(c, r) { const w = cellWorld(c, r); return this.project(w.x, 0.1, w.z); }
  screenOfBench(i) { const w = this.benchPos(i); return this.project(w.x, 0.1, w.z); }

  pickCombat(pt, cb) {
    let best = null;
    let bd = 40;
    for (const u of cb.units) {
      if (!u.alive) continue;
      const w = this.unitWorld(u);
      const s = this.project(w.x, 0.7, w.z);
      const d = Math.hypot(s.x - pt.x, s.y - pt.y);
      if (d < bd) { bd = d; best = u; }
    }
    return best;
  }

  // combat cell to world; mirrored when this viewer is team 1, so their side is always at the bottom
  combatCell(c, r) { const cols = this.combatCols || COLS; return this.mirror ? cellWorld(cols - 1 - c, ROWS - 1 - r, cols) : cellWorld(c, r, cols); }
  viewTeam(team) { return this.mirror ? 1 - team : team; }
  ownerColor(cb, u) {
    const owners = cb.sides[u.team].owners || [];
    const partner = Math.max(0, owners.indexOf(u.owner));
    return ['#5edfff', '#aa92ff', '#ff927b', '#ffc75b'][this.viewTeam(u.team) * 2 + partner % 2];
  }

  unitWorld(u) {
    const k = u.moveDur > 0 ? Math.min(1, u.moveT / u.moveDur) : 1;
    const a = this.combatCell(u.fromC, u.fromR);
    const b = this.combatCell(u.c, u.r);
    const leap = u.moveDur < 0.3 ? 1.6 : 0.25;
    return { x: a.x + (b.x - a.x) * k, z: a.z + (b.z - a.z) * k, y: k < 1 ? Math.sin(Math.PI * k) * leap : 0 };
  }

  // ---------- view sync ----------
  sync(desired) {
    const keep = new Set();
    for (const d of desired) {
      keep.add(d.key);
      let v = this.views.get(d.key);
      if (v && (v.line !== d.line || v.stage !== d.stage)) { v.dispose(this.scene); this.views.delete(d.key); v = null; }
      if (!v) {
        v = new UnitView(this.scene, this.labels, d.key, d.line, d.stage);
        v.body.rotation.y = d.yaw;
        this.views.set(d.key, v);
      }
      v.group.position.set(d.x, d.y || 0, d.z);
      v.yaw = d.yaw;
      if (v.alpha !== (d.alpha == null ? 1 : d.alpha)) { v.alpha = d.alpha == null ? 1 : d.alpha; v.applyAlpha(); }
      v.d = d;
    }
    for (const [k, v] of this.views) if (!keep.has(k)) { v.dispose(this.scene); this.views.delete(k); }
  }

  benchDesired(p, ui, out, partner = false) {
    p.bench.forEach((u, i) => {
      if (!u || (ui.drag && ui.drag.moved && ui.drag.unit === u)) return;
      const w = this.benchPos(i, partner);
      out.push({ key: `u${u.uid}`, line: DT.LINE[u.line], stage: u.stage, items: u.items, x: w.x, z: w.z, yaw: Math.PI, team: 0, hover: ui.hoverUid === u.uid });
    });
  }

  dragDesired(ui, out) {
    if (!ui.drag || !ui.drag.moved) return;
    const p = this.groundAt(ui.drag.x, ui.drag.y);
    if (!p) return;
    const u = ui.drag.unit;
    out.push({ key: `u${u.uid}`, line: DT.LINE[u.line], stage: u.stage, items: u.items, x: p.x, y: 0.6, z: p.z, yaw: Math.PI, team: 0, dragging: true });
  }

  highlight(ui, planning) {
    // tile colors come from the board theme (js/boards.js sets this.tileLook)
    const L = this.tileLook || { mine: new THREE.Color('#30598c'), enemy: new THREE.Color('#7a3442'), enemyPlan: new THREE.Color('#4a2a33'), glowMine: new THREE.Color(0), glowEnemy: new THREE.Color(0) };
    for (const t of this.tiles) {
      const { c, r } = t.userData;
      const hov = ui.drag && ui.drag.moved && ui.hoverCell && ui.hoverCell.c === c && ui.hoverCell.r === r;
      const fl = planning ? 0 : Math.max(0, t.userData.flash - this.fxNow);
      if (hov) t.material.emissive.set('#3d7fd6');
      else if (fl > 0) t.material.emissive.copy(t.userData.flashColor).multiplyScalar(fl * 2.2);
      else t.material.emissive.copy(r >= 4 ? L.glowMine : L.glowEnemy);
      t.material.color.copy(r >= 4 ? L.mine : planning ? L.enemyPlan : L.enemy);
    }
    this.slots.forEach((s, i) => s.material.emissive.set(ui.drag && ui.drag.moved && ui.hoverBench === i ? '#6b5320' : '#000000'));
  }

  benchPos(i, partner = false) {
    const p = benchWorld(i);
    if (this.wide) p.x = p.x * 0.72 + (partner ? 6.1 : -6.1);
    return p;
  }

  setCombatWidth(cols) {
    if (this.combatCols === cols) return;
    if (!this.wide && cols > COLS && this.scene.fog) this.normalFog = {near:this.scene.fog.near,far:this.scene.fog.far};
    if (cols === COLS && this.normalFog && this.scene.fog) Object.assign(this.scene.fog,this.normalFog);
    this.combatCols = cols; this.wide = cols > COLS;
    if (!this.extraTiles) {
      this.extraTiles = [];
      for (let r = 0; r < ROWS; r++) for (let c = COLS; c < COLS * 2; c++) {
        const tile = new THREE.Mesh(this.tiles[0].geometry, this.tiles[0].material.clone());
        tile.userData = { c, r, flash:0, flashColor:new THREE.Color() }; tile.receiveShadow = true;
        this.tiles.push(tile); this.extraTiles.push(tile); this.scene.add(tile);
      }
      this.partnerSlots = this.slots.map(s => { const x = s.clone(); x.material = s.material.clone(); this.scene.add(x); return x; });
    }
    for (const tile of this.tiles) {
      const { c, r } = tile.userData; tile.visible = c < cols;
      const p = cellWorld(c, r, cols); tile.position.set(p.x,.07,p.z);
    }
    this.slots.forEach((s,i) => { const p = this.benchPos(i); s.position.x = p.x; s.scale.x = this.wide ? .72 : 1; });
    this.partnerSlots.forEach((s,i) => { s.visible = this.wide; s.position.x = this.benchPos(i,true).x; s.scale.x = .72; });
    if (!this.wide && this.duoLegends) { for (const x of this.duoLegends) x.view.dispose(); this.duoLegends = null; }
    this.applyView();
  }

  drawDuoExtras(game, cb, p, ui, dt) {
    if (this.board && this.board.env) this.board.env.group.scale.x = this.wide ? 1.8 : 1;
    if (!cb.duo) return;
    if (this.scene.fog) { this.scene.fog.near = 65; this.scene.fog.far = 120; }
    const m = game.matches.find(m => [m.a,m.a2,m.b,m.b2].includes(p));
    if (!m) return;
    const members = [m.a,m.a2,m.b,m.b2].filter(Boolean);
    const sig = members.map(q => q.id).join(',') + '/' + this.mirror;
    if (this.duoLegendSig !== sig || !this.duoLegends) {
      for (const x of this.duoLegends || []) x.view.dispose();
      this.duoLegendSig = sig;
      this.duoLegends = members.map((q,i) => {
        const look = DT.Social.lookOf(q);
        const view = DT.Legends.createView(look && look.legend || DT.Cosmetics.equipped('legend'), { color:['#5edfff','#aa92ff','#ff927b','#ffc75b'][i] });
        const side = this.viewTeam(i < 2 ? 0 : 1);
        view.group.position.set((i % 2 ? 1 : -1)*12.8,0,side === 0 ? 6.3 : -6.3);
        this.scene.add(view.group); return {view};
      });
    }
    for (const x of this.duoLegends) x.view.update(dt);
    if (this.legend) this.legend.view.group.visible = false;
    if (this.oppLegend) this.oppLegend.view.group.visible = false;
  }

  // ---------- frames ----------
  tick() {
    const now = performance.now();
    const dt = Math.min(0.1, (now - this.clock) / 1000);
    this.clock = now;
    this.time += dt;
    if (this.boardTick) this.boardTick(dt, this.time);
    return dt;
  }

  drawPlanning(game, ui) {
    this.setCombatWidth(COLS);
    if (this.board && this.board.env) this.board.env.group.scale.x = 1;
    if (this.legend) this.legend.view.group.visible = true;
    const dt = this.tick();
    this.fxNow = this.time;
    // ui.viewing: the player being scouted (js/scout.js), drawn in place of your own board
    const scout = ui.viewing != null ? game.players[ui.viewing] : null;
    const p = scout || game.human;
    const out = [];
    for (const [k, u] of Object.entries(p.board)) {
      if (ui.drag && ui.drag.moved && ui.drag.unit === u) continue;
      const [c, r] = k.split(',').map(Number);
      const w = cellWorld(c, r);
      out.push({ key: `u${u.uid}`, line: DT.LINE[u.line], stage: u.stage, items: u.items, x: w.x, z: w.z, yaw: Math.PI, team: 0, hover: ui.hoverUid === u.uid });
    }
    if (ui.previewSide && !scout) {
      ui.previewSide.units.forEach((u, i) => {
        const w = cellWorld(COLS - 1 - u.c, ROWS - 1 - u.r);
        out.push({ key: `pv${i}_${u.line}_${u.stage}`, line: DT.LINE[u.line], stage: u.stage, items: u.items, x: w.x, z: w.z, yaw: 0, team: 1, alpha: 0.45, preview: true });
      });
    }
    this.benchDesired(p, ui, out);
    this.dragDesired(ui, out);
    this.sync(out);
    this.drawExtras(game, ui, dt, p);
    this.highlight(ui, true);
    this.updateViews(dt, null);
    this.otLabel.classList.add('hidden');
    this.ult.update(dt);
    this.moves.update(dt);
    this.render();
  }

  drawCombat(game, ui, cb) {
    this.setCombatWidth(cb.cols || COLS);
    const dt = this.tick();
    this.fxNow = cb.t;
    this.cineCtx = { game, ui };
    const out = [];
    for (const u of cb.units) {
      const dying = !u.alive && this.deathT[u.id] != null && cb.t - this.deathT[u.id] < 0.5;
      if (!u.alive && !dying) continue;
      const w = this.unitWorld(u);
      const team = this.viewTeam(u.team);
      let yaw = team === 0 ? Math.PI : 0;
      if (u.target && u.target.alive) {
        const t = this.unitWorld(u.target);
        yaw = Math.atan2(t.x - w.x, t.z - w.z);
      }
      out.push({ key: `c${this.cbSerial}_${u.id}`, line: u.line, stage: u.stage, items: u.items, x: w.x, y: w.y, z: w.z, yaw, team, cu: u,
        dying: dying ? (cb.t - this.deathT[u.id]) / 0.5 : 0, hover: ui.hoverCombatId === u.id });
    }
    // ui.viewing: spectating another Tamer's fight (js/scout.js); their bench and item bench are shown
    const p = (ui.viewing != null && game.players[ui.viewing]) || game.human;
    this.benchDesired(p, ui, out);
    if (cb.duo) { const q = game.partnerOf(p); if (q) this.benchDesired(q, {}, out, true); }
    if (p === game.human) this.dragDesired(ui, out);
    this.sync(out);
    this.drawExtras(game, ui, dt, p);
    this.drawDuoExtras(game, cb, p, ui, dt);
    this.highlight(ui, false);
    this.updateFx(cb.t);
    if (this.cutin) this.cutin.frame(dt);
    this.updateViews(dt, cb);
    if (cb.overtime > 1) {
      this.otLabel.textContent = `OVERTIME x${cb.overtime.toFixed(1)} damage`;
      this.otLabel.classList.remove('hidden');
    } else this.otLabel.classList.add('hidden');
    this.ult.update(dt * (this.speed || 1));
    this.moves.update(dt * (this.speed || 1));
    this.render();
  }

  updateViews(dt, cb) {
    const now = cb ? cb.t : this.time;
    for (const v of this.views.values()) {
      const d = v.d;
      v.update(dt, now);
      if (d.dying) {
        v.group.scale.setScalar(1 - d.dying);
        v.group.position.y -= d.dying * 0.5;
      } else v.group.scale.setScalar(d.hover ? 1.08 : 1);
      v.ringMat.color.set(v.glow ? v.glow.css : d.hover ? '#ffffff' : cb && cb.duo && d.cu ? this.ownerColor(cb, d.cu) : DT.TIER_COLORS[v.line.tier]);
      // label
      const s = this.project(v.group.position.x, v.group.position.y + v.height + 0.35, v.group.position.z);
      const L = v.label;
      L.style.transform = `translate(${s.x}px, ${s.y}px) translate(-50%, -100%)`;
      const showBars = !!d.cu && !d.dying;
      L.classList.toggle('combat', showBars);
      L.classList.toggle('enemy', d.team === 1);
      v.elHp.style.background = cb && cb.duo && d.cu ? this.ownerColor(cb, d.cu) : '';
      L.classList.toggle('show-name', !!d.hover || !!d.dragging);
      L.style.opacity = d.dying ? 0 : d.preview ? 0.55 : 1;
      const sig = (d.items || []).join(',');
      if (sig !== v.itemSig) {
        v.itemSig = sig;
        v.elItems.innerHTML = (d.items || []).map((id) => {
          const it = DT.ITEMS[id];
          return it ? `<img class="${it.component ? 'c' : 'f'}" src="${DT.itemIcon(id)}" alt="">` : '';
        }).join('');
      }
      if (v.castUntil && (!cb || now > v.castUntil)) { v.castUntil = 0; v.elCast.classList.remove('on'); }
      L.style.zIndex = v.castUntil ? 3 : d.hover ? 2 : '';
      if (showBars) {
        const u = d.cu;
        const total = Math.max(u.maxHp, u.hp + u.shield);
        v.elHp.style.width = `${(100 * u.hp) / total}%`;
        v.elSh.style.width = `${(100 * u.shield) / total}%`;
        // a tick every 300 HP, like TFT, so health totals compare at a glance
        if (Math.abs(total - v.tickTotal) > 1) { v.tickTotal = total; v.elHpBox.style.setProperty('--tick', `${(100 * 300) / total}%`); }
        v.elMp.style.width = u.maxMana > 0 ? `${(100 * Math.min(u.mana, u.maxMana)) / u.maxMana}%` : '0%';
        L.classList.toggle('full', u.maxMana > 0 && u.mana >= u.maxMana);
        L.classList.toggle('stunned', u.stun > 0);
      }
    }
  }

  // ---------- combat effects ----------
  // draw, with camera shake from big ultimates
  render() {
    const off = this.ult.shakeOffset();
    if (off) this.camera.position.add(off);
    this.r.render(this.scene, this.camera);
    if (off) this.camera.position.sub(off);
    if (this.cutin) this.cutin.render();
  }

  resetFx() {
    this.cutin && this.cutin.reset();
    this.ult && this.ult.clear();
    this.moves && this.moves.clear();
    this.castCtx = null;
    for (const f of this.fx) if (f.mesh) { this.scene.remove(f.mesh); f.mesh.material.dispose(); }
    this.fx = [];
    this.evIdx = 0;
    this.deathT = {};
    this.celebrated = false;
    this.cbSerial++;
    for (const t of this.tiles) t.userData.flash = 0;
    this.labels.querySelectorAll('.ftext').forEach((e) => e.remove());
  }

  viewOf(id) {
    return this.views.get(`c${this.cbSerial}_${id}`);
  }

  floatText(cb, id, text, cls) {
    const u = cb.units[id];
    if (!u) return;
    const w = this.unitWorld(u);
    const s = this.project(w.x + (Math.random() - 0.5) * 0.5, 1.6 + Math.random() * 0.3, w.z);
    const el = document.createElement('div');
    el.className = `ftext ${cls}`;
    el.textContent = text;
    el.style.left = `${s.x}px`;
    el.style.top = `${s.y}px`;
    this.labels.appendChild(el);
    setTimeout(() => el.remove(), 1000);
  }

  addMeshFx(mesh, t0, dur, update) {
    this.scene.add(mesh);
    this.fx.push({ mesh, t0, dur, update });
  }

  consume(cb) {
    this.setCombatWidth(cb.cols || COLS);
    // survivors on the winning side celebrate once the fight ends
    if (cb.done && !this.celebrated) {
      this.celebrated = true;
      for (const u of Object.values(cb.units)) if (u.hp > 0 && u.team === cb.winner) { const v = this.viewOf(u.id); if (v) v.play('win'); }
    }
    while (this.evIdx < cb.events.length) {
      const e = cb.events[this.evIdx++];
      const from = e.from != null ? cb.units[e.from] : null;
      const to = e.to != null ? cb.units[e.to] : null;
      switch (e.type) {
        case 'dmg':
          this.floatText(cb, e.at, `${e.amount}${e.crit ? '!' : ''}`, `${e.kind}${e.crit ? ' crit' : ''}`);
          { const v = this.viewOf(e.at); if (v) v.flash = e.t + 0.12; }
          break;
        case 'heal':
          this.floatText(cb, e.at, `+${e.amount}`, 'heal');
          if (this.isUlt(e) && this.castCtx.type === 'heal') { this.moves.ally(this.unitWorld(cb.units[e.at])); sfx('heal'); }
          break;
        case 'text': this.floatText(cb, e.at, e.text, 'miss'); break;
        case 'cast': {
          const u = cb.units[e.at];
          const name = u.line.abil[u.stage];
          // later events in this tick belong to the ultimate
          this.castCtx = { id: e.at, t: e.t, elem: this.moves.elemOf(name, u.line), stage: u.stage, type: u.line.type, radius: u.line.radius || 1 };
          this.moves.begin(name, u.line, u.stage, this.unitWorld(u), this.viewOf(e.at), u.target && u.target.alive ? this.unitWorld(u.target) : null);
          sfx('cast', { elem: this.castCtx.elem, stage: u.stage });
          const col = elemColor(this.castCtx.elem);
          const team = this.viewTeam(u.team);
          const v = this.viewOf(e.at);
          if (v) {
            // caster highlight: element-colored tint and ring pulse, plus the ability name pinned above its head
            v.glow = { t0: e.t, until: e.t + 0.9, color: new THREE.Color(col), css: col };
            v.elCast.textContent = name;
            v.elCast.style.setProperty('--ec', col);
            v.elCast.className = `ucast ${team === 0 ? 'ally' : 'enemy'}`;
            void v.elCast.offsetWidth; // restart the pop-in animation
            v.elCast.classList.add('on');
            v.castUntil = e.t + 1.5;
            v.play('special');
          }
          if (this.cutin) this.cutin.offer(u, v, this.cineCtx, col);
          if (this.onCast) this.onCast({ unit: u, name, team, elem: this.castCtx.elem, color: col });
          break;
        }
        case 'leap':
          if (this.isUlt(e)) this.leapFrom = { id: e.at, w: this.combatCell(e.fc, e.fr) };
          break;
        case 'death': this.deathT[e.at] = e.t; sfx('death'); break;
        case 'hit': {
          if (from && to) sfx('hit', { big: e.big });
          if (this.isUlt(e, e.from) && from && to) {
            const lf = this.leapFrom && this.leapFrom.id === e.from ? this.leapFrom.w : this.unitWorld(from);
            this.moves.target(this.unitWorld(to), lf);
            this.leapFrom = null;
          }
          const v = this.viewOf(e.from);
          if (v && from && to) {
            const a = this.unitWorld(from);
            const b = this.unitWorld(to);
            const len = Math.hypot(b.x - a.x, b.z - a.z) || 1;
            // with an attack clip the model swings itself; the lunge stays as a smaller push
            const swing = v.play('attack');
            v.lunge = { t0: e.t, dur: 0.22, dx: (b.x - a.x) / len, dz: (b.z - a.z) / len, dist: swing ? 0.25 : 0.45 };
          }
          if (to && from && !this.isUlt(e, e.from)) {
            const fv = this.viewOf(e.from);
            this.moves.basic(from.line, from.stage, this.unitWorld(from), this.unitWorld(to), fv && fv.height, e.big, false);
          }
          break;
        }
        case 'shot': {
          if (!from || !to) break;
          sfx('shot');
          const a = this.unitWorld(from);
          const b = this.unitWorld(to);
          if (this.isUlt(e, e.from)) { this.moves.target(b); break; }
          { const fv = this.viewOf(e.from); this.moves.basic(from.line, from.stage, a, b, fv && fv.height, e.big, true); }
          const v = this.viewOf(e.from);
          if (v) {
            const len = Math.hypot(b.x - a.x, b.z - a.z) || 1;
            v.play('attack');
            v.lunge = { t0: e.t, dur: 0.15, dx: -(b.x - a.x) / len, dz: -(b.z - a.z) / len, dist: 0.12 };
          }
          break;
        }
        case 'beam': {
          if (!from || !to) break;
          const a = this.unitWorld(from);
          const b = this.unitWorld(to);
          if (this.isUlt(e, e.from)) {
            this.moves.target(b);
            break;
          }
          const col = DT.ATTR[from.line.attr].color;
          const m = new THREE.Mesh(GEO.beam, new THREE.MeshBasicMaterial({ color: col, transparent: true, side: THREE.DoubleSide }));
          const len = Math.hypot(b.x - a.x, b.z - a.z);
          m.position.set((a.x + b.x) / 2, 0.9, (a.z + b.z) / 2);
          m.scale.set(1, len, 1);
          m.rotation.order = 'YXZ';
          m.rotation.set(Math.PI / 2, Math.atan2(b.x - a.x, b.z - a.z), 0);
          this.addMeshFx(m, e.t, 0.3, (k) => { m.material.opacity = 1 - k; m.scale.x = m.scale.z = 3 * (1 - k) + 0.5; });
          break;
        }
        case 'burst': {
          const col = new THREE.Color(this.viewTeam(e.team) === 0 ? '#6fd3ff' : '#ff7a6a');
          const at = this.mirror ? { c: (this.combatCols || COLS) - 1 - e.c, r: ROWS - 1 - e.r } : e;
          for (const t of this.tiles) {
            if (DT.hexDist(t.userData, at) <= e.radius) { t.userData.flash = e.t + 0.45; t.userData.flashColor.copy(col); }
          }
          const c = cellWorld(at.c, at.r);
          if (this.isUlt(e)) {
            const k = this.castCtx;
            this.moves.area(c, e.radius);
            break;
          }
          const m = new THREE.Mesh(GEO.burst, new THREE.MeshBasicMaterial({ color: col, transparent: true, side: THREE.DoubleSide }));
          m.rotation.x = -Math.PI / 2;
          m.position.set(c.x, 0.3, c.z);
          this.addMeshFx(m, e.t, 0.45, (k) => { m.scale.setScalar(0.4 + k * (1 + e.radius * 1.5)); m.material.opacity = 1 - k; });
          break;
        }
        case 'shield': {
          sfx('shield');
          const u = cb.units[e.at];
          const w = this.unitWorld(u);
          if (this.isUlt(e, e.at)) break;
          const m = new THREE.Mesh(GEO.ball, new THREE.MeshBasicMaterial({ color: '#dfe9ff', transparent: true, opacity: 0.4, depthWrite: false }));
          m.position.set(w.x, 0.8, w.z);
          this.addMeshFx(m, e.t, 0.7, (k) => { m.scale.setScalar(0.9 + k * 0.3); m.material.opacity = 0.45 * (1 - k); });
          break;
        }
      }
    }
    // the cast and its hits arrive in the same tick; play the move once all of them are known
    this.moves.flush();
  }

  // is this event part of the ultimate cast this tick (optionally by caster id)?
  isUlt(e, id) {
    const k = this.castCtx;
    return !!k && Math.abs(k.t - e.t) < 1e-6 && (id == null || id === k.id);
  }

  updateFx(now) {
    this.fx = this.fx.filter((f) => {
      const k = (now - f.t0) / f.dur;
      if (k >= 1 || k < 0) {
        this.scene.remove(f.mesh);
        f.mesh.material.dispose();
        return false;
      }
      f.update(k);
      return true;
    });
  }
}

DT.Renderer = Renderer3D;
