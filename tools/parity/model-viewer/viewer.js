// Model sweep review page (served by tools/parity/model-viewer.cjs).
// Each model is a real UnitView from js/render3d.js: the game's model library (models/index.json, the game's
// GLTFLoader setup, clipsFrom borrowing), prepareTemplate (js/model-motion.js clip processing, normalisation,
// material fixes), the stage/tier height and footprint scale, and UnitView.play for one-shot clips.
// Clips the game never plays on board units are listed separately; they come from a second modelClips run.
import * as THREE from 'three';
import { GLTFLoader } from 'three/addons/loaders/GLTFLoader.js';
import { MeshoptDecoder } from 'three/addons/libs/meshopt_decoder.module.js';
import { UnitView, lib, modelTemplate } from '/js/render3d.js';
import { modelClips, borrowedClips } from '/js/model-motion.js';
import { hasRigidPartMotion } from '/js/model-rigid-parts.js';

const DT = window.DT;
const $ = (id) => document.getElementById(id);
const STAGE_HEIGHT = [1.3, 1.65, 2.0]; // render3d.js
const SQ3 = Math.sqrt(3);

// ---------------------------------------------------------------- renderer, same settings and default board lights as the game
const viewEl = $('view');
const renderer = new THREE.WebGLRenderer({ antialias: true, preserveDrawingBuffer: true });
renderer.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
renderer.shadowMap.enabled = true;
renderer.shadowMap.type = THREE.PCFSoftShadowMap;
renderer.outputColorSpace = THREE.SRGBColorSpace;
viewEl.prepend(renderer.domElement);
const scene = new THREE.Scene();
scene.background = new THREE.Color('#0b1a2c');
const camera = new THREE.PerspectiveCamera(38, 1.6, 0.05, 200);
// js/boards.js lights() with the default file-island theme values
const hemi = new THREE.HemisphereLight('#bfe3ff', '#2b3a22', 1.4);
const sun = new THREE.DirectionalLight('#fff4e0', 2.6);
sun.position.set(-8, 18, 10);
sun.castShadow = true;
sun.shadow.mapSize.set(2048, 2048);
Object.assign(sun.shadow.camera, { left: -6, right: 6, top: 6, bottom: -6, near: 1, far: 50 });
sun.shadow.bias = -0.0005;
scene.add(hemi, sun);

// hex tiles as the board draws them (radius 1, pointy along z), one ring around the unit's tile
const tileGeo = new THREE.CylinderGeometry(0.93, 0.97, 0.14, 6);
const ground = new THREE.Group();
for (let r = -3; r <= 3; r++) for (let c = -3; c <= 3; c++) {
  const x = (c + 0.5 * (r & 1)) * SQ3, z = r * 1.5;
  if (Math.hypot(x, z) > 4.6) continue;
  const centre = r === 0 && c === 0;
  const t = new THREE.Mesh(tileGeo, new THREE.MeshStandardMaterial({ color: centre ? '#3a6aa3' : r < 0 ? '#7a3442' : '#30598c', roughness: 0.6, metalness: 0.1 }));
  t.position.set(x, -0.07, z); // tile top at y = 0, where the unit stands
  t.receiveShadow = true;
  ground.add(t);
}
scene.add(ground);
// height reference: a pole at the tile edge with marks at the three stage heights and the current unit height
const ref = new THREE.Group();
scene.add(ref);
const refMat = new THREE.LineBasicMaterial({ color: '#9fb4d6' });
const refHi = new THREE.LineBasicMaterial({ color: '#ffd166' });
const refMeasured = new THREE.LineBasicMaterial({ color: '#3fcf7a' });
function lineSeg(a, b, mat) { return new THREE.Line(new THREE.BufferGeometry().setFromPoints([a, b]), mat); }
function buildRef(target, measured) {
  ref.clear();
  const x = 1.25;
  ref.add(lineSeg(new THREE.Vector3(x, 0, 0), new THREE.Vector3(x, 2.3, 0), refMat));
  for (const h of STAGE_HEIGHT) ref.add(lineSeg(new THREE.Vector3(x - 0.08, h, 0), new THREE.Vector3(x + 0.08, h, 0), refMat));
  ref.add(lineSeg(new THREE.Vector3(x - 0.25, target, 0), new THREE.Vector3(x + 0.12, target, 0), refHi));
  if (measured) ref.add(lineSeg(new THREE.Vector3(-0.9, measured, 0), new THREE.Vector3(x, measured, 0), refMeasured));
}

// ---------------------------------------------------------------- orbit camera (left drag orbit, right/shift drag pan, wheel zoom)
const cam = { target: new THREE.Vector3(0, 0.9, 0), dist: 5.2, yaw: 0.35, pitch: 0.28 };
function resetCam(h = 1.8) { Object.assign(cam, { dist: Math.max(3.2, h * 2.9), yaw: 0.35, pitch: 0.28 }); cam.target.set(0, h * 0.5, 0); applyCam(); }
function applyCam() {
  camera.position.set(cam.target.x + Math.sin(cam.yaw) * Math.cos(cam.pitch) * cam.dist, cam.target.y + Math.sin(cam.pitch) * cam.dist,
    cam.target.z + Math.cos(cam.yaw) * Math.cos(cam.pitch) * cam.dist);
  camera.lookAt(cam.target);
}
let drag = null;
renderer.domElement.addEventListener('contextmenu', (e) => e.preventDefault());
renderer.domElement.addEventListener('pointerdown', (e) => { drag = { x: e.clientX, y: e.clientY, pan: e.button === 2 || e.button === 1 || e.shiftKey }; renderer.domElement.setPointerCapture(e.pointerId); });
renderer.domElement.addEventListener('pointerup', () => { drag = null; });
renderer.domElement.addEventListener('pointermove', (e) => {
  if (!drag) return;
  const dx = e.clientX - drag.x, dy = e.clientY - drag.y;
  drag.x = e.clientX; drag.y = e.clientY;
  if (drag.pan) {
    const s = cam.dist * 0.0016;
    const right = new THREE.Vector3().setFromMatrixColumn(camera.matrix, 0), up = new THREE.Vector3().setFromMatrixColumn(camera.matrix, 1);
    cam.target.addScaledVector(right, -dx * s).addScaledVector(up, dy * s);
  } else {
    cam.yaw -= dx * 0.008;
    cam.pitch = THREE.MathUtils.clamp(cam.pitch + dy * 0.006, -1.2, 1.5);
  }
  applyCam();
});
renderer.domElement.addEventListener('wheel', (e) => { e.preventDefault(); cam.dist = THREE.MathUtils.clamp(cam.dist * Math.exp(e.deltaY * 0.001), 0.6, 40); applyCam(); }, { passive: false });
function fit() {
  const w = viewEl.clientWidth, h = viewEl.clientHeight;
  renderer.setSize(w, h, false);
  camera.aspect = w / Math.max(1, h);
  camera.updateProjectionMatrix();
}
new ResizeObserver(fit).observe(viewEl);

// ---------------------------------------------------------------- the game's processed clip list (second, independent load)
const loader = new GLTFLoader();
loader.setMeshoptDecoder(MeshoptDecoder);
// same plugin as render3d.js, so files that require KHR_materials_pbrSpecularGlossiness parse
loader.register((parser) => ({
  name: 'KHR_materials_pbrSpecularGlossiness',
  getMaterialType(i) { return parser.json.materials[i].extensions?.KHR_materials_pbrSpecularGlossiness ? THREE.MeshBasicMaterial : null; },
  extendMaterialParams() { return Promise.resolve(); },
}));
async function processedClips(key) {
  const file = lib.index[key], src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[key];
  const g = await loader.loadAsync(file);
  const raw = g.animations.map((a) => `${a.name} (${a.duration.toFixed(2)} s)`);
  let borrowed = null;
  const from = src && src.clipsFrom && lib.index[src.clipsFrom];
  if (from) { try { borrowed = borrowedClips(await loader.loadAsync(from), DT.MODEL_SOURCES[src.clipsFrom]); } catch { borrowed = null; } }
  let skinned = false;
  g.scene.traverse((o) => { if (o.isSkinnedMesh || o.isBone) skinned = true; });
  const clips = modelClips(g, src, borrowed);
  const rawNames = new Set(g.animations.map((a) => a.name));
  let path;
  if (borrowed && !g.animations.length) path = `borrowed clips from ${src.clipsFrom} (retargeted)`;
  else if (hasRigidPartMotion(src)) path = 'reviewed rigid-part rig (js/model-rigid-parts.js)';
  else if (!g.animations.length && !skinned) path = 'automatic rigid-part rig (unskinned, clipless)';
  else if (!g.animations.length) path = 'generated skeletal fallback clips (clipless skinned rig)';
  else if (src && src.anim === false) path = 'authored reel disabled (anim:false): generated skeletal fallback clips';
  else {
    const generated = clips.map((c) => c.name).filter((n) => !rawNames.has(n));
    path = generated.length ? `authored clips; generated: ${generated.join(', ')}` : 'authored clips';
    if (src && src.fallbackRoles) path += ` (fallbackRoles ${src.fallbackRoles.join(', ')})`;
  }
  return { clips, raw, path };
}

// ---------------------------------------------------------------- state
const CATS = [
  { id: 0, test: (f) => f === 't-pose', name: 'idle t-pose' },
  { id: 1, test: (f) => f === 'bind-pose-idle', name: 'bind-pose idle' },
  { id: 2, test: (f) => f === 'motionless-idle', name: 'motionless idle' },
  { id: 3, test: (f) => /^(attack|special|win)-t-pose$/.test(f), name: 'action t-pose' },
  { id: 4, test: (f) => /-height-x/.test(f), name: 'height jump' },
];
const catOf = (f) => { const c = CATS.find((k) => k.test(f)); return c ? c.id : -1; };
let sweep = { Results: [] }, flagged = [], shown = [], bySource = new Map(), verdicts = new Map();
let cur = null; // { entry, key, view, extras, ... }
let paused = false, speed = 1, loop = true, bindMode = false, skelHelper = null;
let active = null; // { kind: 'idle'|'role'|'extra', name, action }
let replayRole = null;
const labelsEl = $('labels');
const status = (t) => { $('status').textContent = t || ''; };

// ---------------------------------------------------------------- list
function verdictOf(src) { return verdicts.get(src) || null; }
function renderList() {
  const q = $('filter').value.trim().toLowerCase();
  const thumbs = $('thumbs').checked;
  shown = flagged.filter((e) => {
    if (!q) return true;
    const v = verdictOf(e.Source);
    if (q === 'unreviewed') return !v;
    const hay = `${e.Index} ${e.Source} ${e.Asset} ${e.Line || ''} ${e.Variant || ''} ${e.Flags.join(' ')} ${v ? v.verdict : 'unreviewed'}`.toLowerCase();
    return q.split(/\s+/).every((t) => hay.includes(t));
  });
  const list = $('list');
  list.innerHTML = '';
  const frag = document.createDocumentFragment();
  for (const e of shown) {
    const v = verdictOf(e.Source);
    const d = document.createElement('div');
    d.className = 'item' + (cur && cur.key === e.Source ? ' sel' : '');
    d.dataset.src = e.Source;
    d.innerHTML = `<div class="top"><span class="idx">#${e.Index}</span><span class="key"></span>${v ? `<span class="vd ${v.verdict}">${v.verdict}</span>` : ''}</div>
      <div class="chips">${e.Flags.filter((f) => f !== 'no-portrait').map((f) => `<span class="chip c${catOf(f)}">${f}</span>`).join('')}</div>
      ${thumbs && e.Image ? `<img loading="lazy" src="/sweep/${e.Image}" alt="">` : ''}`;
    d.querySelector('.key').textContent = e.Source;
    d.addEventListener('click', () => select(e.Source));
    frag.appendChild(d);
  }
  list.appendChild(frag);
  const done = flagged.filter((e) => verdictOf(e.Source)).length;
  $('count').textContent = `${shown.length} shown, ${flagged.length} flagged, ${done} reviewed`;
}
function markSelected() {
  for (const d of document.querySelectorAll('.item')) d.classList.toggle('sel', !!cur && d.dataset.src === cur.key);
  const el = document.querySelector('.item.sel');
  if (el) el.scrollIntoView({ block: 'nearest' });
}
function step(dir) {
  if (!shown.length) return;
  const i = cur ? shown.findIndex((e) => e.Source === cur.key) : -1;
  const n = i < 0 ? 0 : THREE.MathUtils.clamp(i + dir, 0, shown.length - 1);
  select(shown[n].Source);
}

// ---------------------------------------------------------------- loading one model as a game UnitView
let loadSerial = 0;
async function select(key) {
  const serial = ++loadSerial;
  const entry = bySource.get(key) || { Source: key, Index: null, Flags: [], Stage: 2, Tier: 1, Asset: lib.index[key] };
  disposeCurrent();
  history.replaceState(null, '', `#${encodeURIComponent(key)}`);
  cur = { key, entry, view: null, extras: [], info: null };
  markSelected();
  renderVerdict();
  renderSweepInfo(entry);
  $('hudKey').textContent = key;
  $('hudSub').textContent = entry.Flags.join(', ');
  const strip = $('strip');
  if (entry.Image) { strip.src = `/sweep/${entry.Image}`; strip.classList.toggle('hide', !$('stripBtn').classList.contains('on')); } else strip.classList.add('hide');
  if (!lib.index[key]) { status(`${key} is not in models/index.json`); renderInfo(); renderClips(); return; }
  status('Loading model');
  // a stand-in line with the sweep's stage and tier: UnitView takes its height (STAGE_HEIGHT + tier 5 bonus) from them
  const stage = Number.isInteger(entry.Stage) ? THREE.MathUtils.clamp(entry.Stage, 0, 2) : 2;
  const name = key.split(':').slice(1).join(':') || key;
  const line = { id: `viewer:${key}`, tier: entry.Tier || 1, forms: [name, name, name], attr: Object.keys(DT.ATTR)[0], traits: [], color: '#888888' };
  const realKey = DT.modelKey;
  DT.modelKey = () => key;
  let view;
  try { view = new UnitView(scene, labelsEl, `viewer-${serial}`, line, stage); } finally { DT.modelKey = realKey; }
  view.yaw = 0; view.body.rotation.y = 0; // face the camera, as in the sweep strip
  cur.view = view;
  const extrasP = processedClips(key).catch((e) => ({ clips: [], raw: [], path: `clip list failed: ${e.message}` }));
  // UnitView.tryModel polls the model library like the board does
  while (!view.hasModel && serial === loadSerial) { view.update(0, 0); await new Promise((r) => setTimeout(r, 60)); }
  if (serial !== loadSerial) return;
  if (view.hasModel === 'none') { status('Model failed to load (placeholder shown)'); renderInfo(); renderClips(); return; }
  view.group.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(view.visual);
  cur.measured = box.max.y;
  buildRef(view.height, cur.measured);
  resetCam(Math.max(view.height, cur.measured));
  if (view.mixer) view.mixer.addEventListener('finished', (e) => { if (loop && active && active.kind === 'role' && e.action === active.action) replayRole = active.name; });
  active = view.idleAction ? { kind: 'idle', name: 'idle', action: view.idleAction } : null;
  if (active) { view.idleAction.time = 0; }
  status(view.mixer ? '' : 'No idle clip: the game shows this model unanimated');
  const ex = await extrasP;
  if (serial !== loadSerial) return;
  cur.info = ex;
  const gameClips = new Set([view.idleAction && view.idleAction.getClip(), ...Object.values(view.clips || {})]);
  cur.extras = ex.clips.filter((c) => ![...gameClips].some((g) => g && g.name === c.name));
  renderInfo();
  renderClips();
  if (skelHelper) { setSkeleton(false); setSkeleton(true); }
  resetMotion();
}
function disposeCurrent() {
  setSkeleton(false, true);
  if (bindMode) setBind(false);
  if (cur && cur.view) { cur.view.dispose(scene); }
  active = null; replayRole = null;
  cur = null;
}

// ---------------------------------------------------------------- clips
const ROLE_ORDER = ['idle', 'run', 'attack', 'special', 'win'];
function renderClips() {
  const row = $('clipRow');
  row.innerHTML = '';
  const v = cur && cur.view;
  const add = (label, fn, on, enabled, title) => {
    const b = document.createElement('button');
    b.textContent = label; b.disabled = !enabled; if (on) b.classList.add('on'); if (title) b.title = title;
    b.addEventListener('click', fn);
    row.appendChild(b);
    return b;
  };
  const span = document.createElement('span'); span.className = 'lbl'; span.textContent = 'game clips:'; row.appendChild(span);
  for (const role of ROLE_ORDER) {
    const has = role === 'idle' ? !!(v && v.idleAction) : !!(v && v.clips && v.clips[role]);
    const clip = role === 'idle' ? v && v.idleAction && v.idleAction.getClip() : v && v.clips && v.clips[role];
    const label = role === 'run' ? 'move (run)' : role;
    add(has ? label : `${label} (none)`, () => playRole(role), active && active.kind !== 'extra' && active.name === role, has,
      has ? `${clip.name}, ${clip.duration.toFixed(2)} s` : role === 'run' ? 'board units have no walk clip: the game slides/leaps them between tiles' : 'no clip for this role: the game skips it');
  }
  if (cur && cur.extras.length) {
    const s2 = document.createElement('span'); s2.className = 'lbl'; s2.textContent = ' other clips (not played on the board):'; row.appendChild(s2);
    for (const c of cur.extras) add(c.name, () => playExtra(c), active && active.kind === 'extra' && active.name === c.name, true, `${c.duration.toFixed(2)} s`);
  }
}
function playRole(role) {
  const v = cur && cur.view;
  if (!v || !v.mixer) return;
  if (bindMode) setBind(false);
  stopExtra();
  replayRole = null;
  if (role === 'idle') {
    if (v.oneShot) v.endOneShot();
    active = { kind: 'idle', name: 'idle', action: v.idleAction };
  } else {
    // UnitView.play refuses an attack over a running special; clear any one-shot first so every button works
    if (v.oneShot) { v.oneShot.stop(); v.oneShot = null; v.oneShotRole = null; }
    if (!v.play(role)) return;
    active = { kind: 'role', name: role, action: v.oneShot };
  }
  resetMotion();
  renderClips();
}
function stopExtra() {
  if (active && active.kind === 'extra') { active.action.fadeOut(0.08); }
}
function playExtra(clip) {
  const v = cur && cur.view;
  if (!v || !v.mixer) return;
  if (bindMode) setBind(false);
  stopExtra();
  if (v.oneShot) { v.oneShot.stop(); v.oneShot = null; v.oneShotRole = null; }
  const a = v.mixer.clipAction(clip);
  a.reset(); a.setLoop(THREE.LoopRepeat, Infinity); a.setEffectiveWeight(1); a.fadeIn(0.08).play();
  v.idleAction.fadeOut(0.08);
  active = { kind: 'extra', name: clip.name, action: a };
  resetMotion();
  renderClips();
}

// ---------------------------------------------------------------- pause, scrub, speed, skeleton, bind pose
function setPaused(p) { paused = p; $('pause').textContent = p ? 'Play' : 'Pause'; $('pause').classList.toggle('on', p); }
$('pause').addEventListener('click', () => setPaused(!paused));
for (const b of document.querySelectorAll('.spd')) b.addEventListener('click', () => {
  speed = Number(b.dataset.s);
  for (const o of document.querySelectorAll('.spd')) o.classList.toggle('on', o === b);
});
$('loop').addEventListener('click', () => { loop = !loop; $('loop').classList.toggle('on', loop); });
$('scrub').addEventListener('input', () => {
  const v = cur && cur.view;
  if (!v || !v.mixer || !active) return;
  if (bindMode) setBind(false);
  setPaused(true);
  const a = active.action, d = a.getClip().duration;
  // show exactly this clip at this time: no cross-fade weights from the clip before
  v.mixer.stopAllAction();
  a.reset().setEffectiveWeight(1).play();
  a.time = (Number($('scrub').value) / 1000) * d;
  v.mixer.update(0);
  if (active.kind === 'role') { v.oneShot = a; v.oneShotRole = active.name; }
});
function setSkeleton(on, keepButton) {
  if (skelHelper) { scene.remove(skelHelper); skelHelper.dispose(); skelHelper = null; }
  if (on && cur && cur.view && cur.view.visual) {
    skelHelper = new THREE.SkeletonHelper(cur.view.visual);
    skelHelper.material.depthTest = false; skelHelper.material.transparent = true; skelHelper.renderOrder = 10;
    scene.add(skelHelper);
  }
  if (!keepButton) $('skel').classList.toggle('on', !!on);
}
$('skel').addEventListener('click', () => setSkeleton(!$('skel').classList.contains('on')));
// Skeleton.pose() writes the bind matrices as root-bone LOCAL transforms, dropping the wrapper scale the game adds
// around the model. Place each bone so boneWorld * boneInverse = meshWorld * bindMatrix^-1, which draws the
// mesh exactly as authored at its current place and size.
function bindPose(visual) {
  visual.updateMatrixWorld(true);
  const done = new Set(), base = new THREE.Matrix4(), world = new THREE.Matrix4(), local = new THREE.Matrix4();
  const depth = (o) => { let d = 0; for (let p = o.parent; p; p = p.parent) d++; return d; };
  let skinned = 0;
  visual.traverse((o) => {
    if (!o.isSkinnedMesh || !o.skeleton) return;
    skinned++;
    base.copy(o.matrixWorld).multiply(world.copy(o.bindMatrix).invert());
    const order = o.skeleton.bones.map((b, i) => ({ b, i })).sort((x, y) => depth(x.b) - depth(y.b));
    for (const { b, i } of order) {
      if (done.has(b)) continue;
      done.add(b);
      world.copy(base).multiply(local.copy(o.skeleton.boneInverses[i]).invert());
      if (b.parent) { b.parent.updateWorldMatrix(true, false); local.copy(b.parent.matrixWorld).invert().multiply(world); } else local.copy(world);
      local.decompose(b.position, b.quaternion, b.scale);
      b.updateMatrixWorld(true);
    }
  });
  return skinned;
}
// skinned vertex bounds in the unit group's frame
function skinnedBox(v) {
  v.group.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(v.visual, true);
  return box.applyMatrix4(new THREE.Matrix4().copy(v.group.matrixWorld).invert());
}
function setBind(on) {
  const v = cur && cur.view;
  if ((on && !!v) === bindMode) return;
  bindMode = on && !!v;
  $('bind').classList.toggle('on', bindMode);
  if (!v || !v.visual) return;
  if (bindMode) {
    const skinned = bindPose(v.visual);
    if (!skinned) { status('This model has no skinned mesh, so it has no skin bind pose'); return; }
    // Rips often author the skin in other units or far from the animated pose. Fit the bind pose to the
    // animated height on the same tile so the two can be compared; the factor is shown.
    v.bindRestore = { p: v.visual.position.clone(), s: v.visual.scale.clone() };
    const box = skinnedBox(v);
    const h = box.max.y - box.min.y, want = cur.measured || v.height;
    let note = '';
    if (h > 1e-6 && Number.isFinite(h)) {
      const k = want / h;
      v.visual.scale.multiplyScalar(k);
      const b2 = skinnedBox(v), c = b2.getCenter(new THREE.Vector3());
      v.visual.position.x -= c.x; v.visual.position.z -= c.z; v.visual.position.y -= b2.min.y;
      if (Math.abs(k - 1) > 0.02) note = `, refit x${k.toPrecision(3)} to the animated height`;
    }
    status(`Bind pose (skin rest pose${note}); press B or pick a clip to animate again`);
  } else {
    status('');
    if (v.bindRestore) { v.visual.position.copy(v.bindRestore.p); v.visual.scale.copy(v.bindRestore.s); v.bindRestore = null; }
    if (v.mixer) v.mixer.update(0);
  }
}
$('bind').addEventListener('click', () => setBind(!bindMode));
$('stripBtn').addEventListener('click', () => {
  const on = !$('stripBtn').classList.contains('on');
  $('stripBtn').classList.toggle('on', on);
  $('strip').classList.toggle('hide', !on || !(cur && cur.entry.Image));
});
$('strip').addEventListener('click', () => $('strip').classList.toggle('big'));
$('resetCam').addEventListener('click', () => resetCam(cur && cur.view ? Math.max(cur.view.height, cur.measured || 0) : 1.8));

// ---------------------------------------------------------------- motion coverage: which nodes move during the current clip
let motion = null;
function resetMotion() {
  const v = cur && cur.view;
  motion = null;
  if (!v || !v.visual) { $('motion').textContent = '-'; return; }
  const nodes = [];
  const seen = new Set();
  v.visual.traverse((o) => {
    if (o.isSkinnedMesh && o.skeleton) for (const b of o.skeleton.bones) if (!seen.has(b)) { seen.add(b); nodes.push(b); }
  });
  if (!nodes.length) v.visual.traverse((o) => { if (o.isMesh) nodes.push(o); }); // rigid-part models move whole meshes
  motion = { nodes, min: nodes.map(() => new THREE.Vector3(Infinity, Infinity, Infinity)), max: nodes.map(() => new THREE.Vector3(-Infinity, -Infinity, -Infinity)),
    t: 0, kind: nodes.length && nodes[0].isBone ? 'bones' : 'meshes' };
}
const tmpV = new THREE.Vector3(), inv = new THREE.Matrix4();
function trackMotion(dt) {
  const v = cur && cur.view;
  if (!motion || !v || paused || bindMode) return;
  motion.t += dt;
  v.visual.updateMatrixWorld(true);
  inv.copy(v.group.matrixWorld).invert();
  motion.nodes.forEach((n, i) => { tmpV.setFromMatrixPosition(n.matrixWorld).applyMatrix4(inv); motion.min[i].min(tmpV); motion.max[i].max(tmpV); });
  if ((motion.frame = (motion.frame || 0) + 1) % 10) return;
  const h = Math.max(v.height, 0.5), still = [];
  let moving = 0;
  motion.nodes.forEach((n, i) => { const d = motion.max[i].distanceTo(motion.min[i]); if (d > 0.01 * h) moving++; else still.push(n.name || '(unnamed)'); });
  const dur = active ? active.action.getClip().duration : 0;
  $('motion').innerHTML = `${moving} / ${motion.nodes.length} ${motion.kind} travel more than 1% of unit height over ${motion.t.toFixed(1)} s` +
    `${dur ? ` (clip ${dur.toFixed(2)} s)` : ''}.` + (dur && motion.t < dur / Math.max(1e-3, Math.abs(active.action.timeScale)) ? ' Still counting: let the clip play once.' : '') + (still.length ? `<br>Still: <span style="color:var(--text)">${still.slice(0, 40).map(escapeHtml).join(', ')}${still.length > 40 ? ', ...' : ''}</span>` : '');
}
const escapeHtml = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

// ---------------------------------------------------------------- info panels
function dl(el, rows) {
  el.innerHTML = rows.filter(([, v]) => v != null && v !== '').map(([k, v]) => `<dt>${escapeHtml(k)}</dt><dd>${escapeHtml(v)}</dd>`).join('');
}
function renderInfo() {
  const key = cur.key, src = (DT.MODEL_SOURCES && DT.MODEL_SOURCES[key]) || {}, v = cur.view;
  const opts = ['clipsFrom', 'fallbackRoles', 'anim', 'rotY', 'yOffset', 'maxFootprint', 'frontOnly'].filter((k) => src[k] != null).map((k) => `${k}=${JSON.stringify(src[k])}`).join(' ');
  dl($('info'), [
    ['file', lib.index[key] || '(not in models/index.json)'],
    ['uid', src.uid], ['name', src.name], ['options', opts],
    ['motion path', cur.info && cur.info.path],
    ['idle clip', v && v.idleAction ? `${v.idleAction.getClip().name} (${v.idleAction.getClip().duration.toFixed(2)} s)` : 'none'],
    ['roles', v && v.clips ? Object.keys(v.clips).join(', ') || 'none' : ''],
    ['clips in file', cur.info ? (cur.info.raw.length ? cur.info.raw.join(', ') : 'none') : 'loading'],
    ['processed', cur.info ? cur.info.clips.map((c) => c.name).join(', ') : ''],
    ['unit height', v ? `${v.height.toFixed(2)} target (yellow), ${cur.measured ? cur.measured.toFixed(2) : '?'} measured (green)` : ''],
  ]);
}
function renderSweepInfo(e) {
  const r = (x) => (x == null ? null : String(x));
  dl($('sweepInfo'), [
    ['index', r(e.Index)], ['line', e.Line ? `${e.Line} (${e.Variant})` : 'not on a board line'], ['stage/tier', e.Index != null ? `${e.Stage} / ${e.Tier}` : null],
    ['asset', e.Asset !== lib.index[e.Source] ? `${e.Asset} (JS index: ${lib.index[e.Source]})` : e.Asset],
    ['idle motion', r(e.IdleMotion)], ['idle vs bind', r(e.IdleBindDeviation)], ['arms found', r(e.ArmsFound)], ['arms horizontal', r(e.ArmsHorizontal)],
    ['height ratios', e.Index != null ? `attack ${e.AttackHeightRatio}, special ${e.SpecialHeightRatio}, win ${e.WinHeightRatio}` : null],
    ['error', e.Error],
  ]);
}

// ---------------------------------------------------------------- verdicts
function renderVerdict() {
  const v = cur && verdictOf(cur.key);
  for (const b of document.querySelectorAll('.vb')) b.classList.toggle('on', !!v && b.dataset.v === v.verdict);
  $('notes').value = v ? v.notes || '' : '';
  $('saved').textContent = v ? `Saved ${v.verdict} at ${new Date(v.at).toLocaleString()}` : 'No verdict yet';
}
async function saveVerdict(verdict) {
  if (!cur) return false;
  const body = { source: cur.key, index: cur.entry.Index, verdict, notes: $('notes').value, flags: cur.entry.Flags };
  try {
    const r = await fetch('/api/verdict', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
    const j = await r.json();
    if (!r.ok) throw new Error(j.error || r.status);
    if (verdict === 'clear') verdicts.delete(cur.key); else verdicts.set(cur.key, j.entry);
    renderVerdict();
    const el = document.querySelector(`.item[data-src="${CSS.escape(cur.key)}"] .top`);
    if (el) { el.querySelector('.vd')?.remove(); if (verdict !== 'clear') el.insertAdjacentHTML('beforeend', `<span class="vd ${verdict}">${verdict}</span>`); }
    $('count').textContent = `${shown.length} shown, ${flagged.length} flagged, ${flagged.filter((e) => verdictOf(e.Source)).length} reviewed`;
    return true;
  } catch (e) { $('saved').textContent = `Save failed: ${e.message}`; return false; }
}
for (const b of document.querySelectorAll('.vb')) b.addEventListener('click', () => saveVerdict(b.dataset.v));
$('saveNotes').addEventListener('click', () => { const v = cur && verdictOf(cur.key); if (v) saveVerdict(v.verdict); else $('saved').textContent = 'Pick a verdict first; notes are saved with it'; });
$('clearV').addEventListener('click', () => saveVerdict('clear'));
$('notes').addEventListener('keydown', (e) => { if (e.key === 'Enter' && e.ctrlKey) { e.preventDefault(); $('saveNotes').click(); } });

// ---------------------------------------------------------------- keyboard
document.addEventListener('keydown', async (e) => {
  if (e.target.matches('input[type=text], textarea')) return;
  const k = e.key;
  if (k === 'ArrowDown' || k === 'ArrowUp') { e.preventDefault(); step(k === 'ArrowDown' ? 1 : -1); return; }
  const vmap = { 1: 'broken', 2: 'fine', 3: 'partial' };
  if (vmap[k]) { e.preventDefault(); if (await saveVerdict(vmap[k])) step(1); return; }
  if (k === ' ') { e.preventDefault(); setPaused(!paused); return; }
  const lk = k.toLowerCase();
  if (lk === 's') setSkeleton(!$('skel').classList.contains('on'));
  else if (lk === 'b') setBind(!bindMode);
  else if (lk === 'r') $('resetCam').click();
  else if (lk === 'i') playRole('idle');
  else if (lk === 'm') playRole('run');
  else if (lk === 'a') playRole('attack');
  else if (lk === 'p') playRole('special');
  else if (lk === 'w') playRole('win');
});
$('filter').addEventListener('input', renderList);
$('thumbs').addEventListener('change', renderList);
const loadFromBox = () => { const k = $('loadKey').value.trim(); if (k) select(k); };
$('loadBtn').addEventListener('click', loadFromBox);
$('loadKey').addEventListener('keydown', (e) => { if (e.key === 'Enter') loadFromBox(); });

// ---------------------------------------------------------------- frame loop
let last = performance.now(), now = 0;
function frame() {
  requestAnimationFrame(frame);
  const t = performance.now();
  const dt = Math.min(0.1, (t - last) / 1000) * speed;
  last = t;
  const v = cur && cur.view;
  if (v && v.hasModel === true) {
    if (replayRole && !paused && !bindMode) { const r = replayRole; replayRole = null; playRole(r); }
    else if (active && active.kind === 'role' && !v.oneShot) { active = { kind: 'idle', name: 'idle', action: v.idleAction }; renderClips(); }
    if (!paused && !bindMode) now += dt;
    // UnitView.update advances the mixer and keeps the game's facing/emissive handling; in bind pose the mixer stays off
    if (!bindMode) v.update(paused ? 0 : dt, now);
    trackMotion(dt);
    if (active && !(paused && document.activeElement === $('scrub'))) {
      const d = active.action.getClip().duration;
      const time = active.action.isRunning() || paused ? active.action.time : 0;
      $('scrub').value = d ? String(Math.round((time / d) * 1000)) : '0';
      $('time').textContent = `${time.toFixed(2)} / ${d.toFixed(2)} s${active.action.timeScale !== 1 ? ` (x${active.action.timeScale.toFixed(2)} game speed)` : ''}`;
    }
  }
  renderer.render(scene, camera);
}

// ---------------------------------------------------------------- boot
(async () => {
  fit();
  resetCam();
  buildRef(STAGE_HEIGHT[2], null);
  requestAnimationFrame(frame);
  status('Loading sweep and model index');
  const [sw, vd] = await Promise.all([fetch('/api/sweep').then((r) => (r.ok ? r.json() : { Results: [] })), fetch('/api/verdicts').then((r) => r.json()), lib.ready]);
  sweep = sw;
  for (const e of vd.entries || []) { if (e.verdict === 'clear') verdicts.delete(e.source); else verdicts.set(e.source, e); }
  for (const e of sweep.Results) bySource.set(e.Source, e);
  const cat = (e) => Math.min(...e.Flags.map(catOf).filter((c) => c >= 0));
  flagged = sweep.Results.filter((e) => e.Flags.some((f) => catOf(f) >= 0)).sort((a, b) => cat(a) - cat(b) || a.Index - b.Index);
  const keys = new Set([...sweep.Results.map((e) => e.Source), ...Object.keys(lib.index)]);
  $('allKeys').innerHTML = [...keys].sort().map((k) => `<option value="${escapeHtml(k)}">`).join('');
  renderList();
  status(sweep.Results.length ? 'Pick a model' : 'sweep.json not found (start the server with --sweep <dir>)');
  window.__viewer = { select, get cur() { return cur; }, get flagged() { return flagged; }, playRole, setBind, setSkeleton, setPaused };
  const hash = decodeURIComponent(location.hash.slice(1));
  if (hash) select(hash); else if (flagged.length) select(flagged[0].Source);
})();
