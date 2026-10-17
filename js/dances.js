// Dances: the 'dance' cosmetics and a procedural animator that moves a model's own skeleton bones.
// Client-only and outside the lockstep rules, like the other cosmetics.
//
// The rips use several skeleton styles (Digimon Links J_center/J_ude_L, Sketchfab copies with _012 suffixes,
// Cyber Sleuth J_arm_l/J_elbow_l, 3ds Max Bip001, plain Bone001). findRoles() maps any of them onto one set of
// roles (hips, spine, chest, neck, head, arms, legs, tail, wings, ears) from bone names first, then from the
// hierarchy and bone positions. A dance is a function of the beat that returns a pose in character space
// ("left arm points up", "hips lean 10 degrees"); the Dancer turns that into bone rotations, clamped per role.
// Missing limbs are skipped: a model without arms moves its ears, body, head and tail instead.
//
// DT.Dances.list() / get(id) / bpm(id)    catalog (also DT.Cosmetics.list('dance'))
// DT.Dances.findRoles(root)               role map for a model root (a template root or a clone)
// DT.Dances.Dancer                        new Dancer(obj, templateRoot?); .set(id); .apply(seconds, weight); .clear()
// DT.Dances.attachView(view, id)          drive a LegendView's bones: { update(seconds), dispose() }
// DT.Dances.unitRig(key, id)              Promise of { group, update(dt, seconds), dispose() } for a battle unit model
// DT.Dances.objRig(obj, id, opts)         the same for an object already built (static models, placeholders)
// DT.Dances.playFor(playerId, id?)        that Tamer's Little Legend on the board dances (id defaults to their loadout)
// DT.Dances.drawInto(ctx, x, y, w, h, o)  draw a dancing legend into a 2D canvas: o { legend, dance, t, unit, size }
// DT.Dances.preview(el, id)               looping store preview; returns a cleanup function
import * as THREE from 'three';
import * as SkeletonUtils from 'three/addons/utils/SkeletonUtils.js';
import { loadTemplate } from './render3d.js';
import { dances as mysteryDances } from './mystery-content.js';

const DT = window.DT;
const C = DT.Cosmetics;
const TAU = Math.PI * 2;
const DEG = Math.PI / 180;

// ---------- bone names ----------
// word -> role. Japanese romanization from the Links / Cyber Sleuth rigs: ude arm, hiji elbow, te hand, momo thigh,
// hiza knee, ashi/asi leg, kakato heel, atama head, kubi neck, mune chest, hara belly, shippo tail, hane wing, mimi ear.
const WORDS = {
  hips: ['center', 'centre', 'pelvis', 'hip', 'hips', 'kosi', 'koshi', 'waist', 'mass', 'cog'],
  root: ['root', 'rootjoint'],
  spine: ['spine', 'hara', 'body', 'back', 'fukubu', 'upperhara', 'bd', 'torso'],
  chest: ['chest', 'mune', 'thorax', 'upperbody'],
  neck: ['neck', 'kubi'],
  head: ['head', 'haed', 'hed', 'atama', 'kao', 'face', 'hd'],
  jaw: ['ago', 'uwago', 'jaw', 'chin', 'kuchi', 'mouth'],
  clav: ['clavicle', 'collar', 'kata'],
  shoulder: ['shoulder'],
  arm: ['arm', 'ude', 'upperarm', 'la', 'ra', 'uparm'],
  fore: ['elbow', 'hiji', 'ancon', 'forearm', 'lowerarm'],
  hand: ['hand', 'te', 'wrist', 'tubi', 'lh', 'rh', 'tekubi'],
  thigh: ['leg', 'momo', 'thigh', 'upleg', 'reg', 'ashi', 'asi', 'lf', 'rf'],
  shin: ['knee', 'hiza', 'calf', 'shin', 'lowerleg', 'sune', 'horselink'],
  foot: ['foot', 'kakato', 'ankle', 'unkle', 'ashikubi', 'feet'],
  tail: ['tail', 'shippo', 'sippo', 'tale', 'teil', 'hippo'],
  wing: ['wing', 'hane', 'tsubasa', 'wingmoto'],
  ear: ['ear', 'ears', 'mimi'],
};
const WORD_ROLE = {};
for (const [role, ws] of Object.entries(WORDS)) for (const w of ws) if (!WORD_ROLE[w]) WORD_ROLE[w] = role;
const SIDE_WORDS = { la: 1, lh: 1, lf: 1, ra: -1, rh: -1, rf: -1 };
const SKIP = /target|^cp\d|^fp\d|^gp\d|^tp\d|^loc|grp|prefab|^chr\d|(^|_)end(\d|_|\s|$)|item|weapon|option|effect|hud/i;

// name -> { role, side (+1 left, -1 right, 0 none), idx, front (true/false/null) }
function parseName(name) {
  let s = String(name || '');
  s = s.replace(/_\d{2,}$/, ''); // Sketchfab suffix: J_arm_l_020
  s = s.replace(/^(mixamorig:|bip0*1\s*)/i, '');
  s = s.replace(/([a-z])([A-Z])/g, '$1 $2');
  const toks = s.toLowerCase().split(/[\s_.:\-]+/).filter(Boolean);
  let role = null, side = 0, idx = 0, front = null;
  for (let t of toks) {
    if (t === 'j' || t === 'jnt') continue;
    if (t === 'b') { front = false; continue; }
    let m = /^([lr])(\d*)$/.exec(t);
    if (m) { side = m[1] === 'l' ? 1 : -1; idx = Number(m[2] || 0) || idx; continue; }
    if (t === 'left') { side = 1; continue; }
    if (t === 'right') { side = -1; continue; }
    if (/^f\d*$/.test(t)) { front = true; continue; }
    m = /^(left|right)(.+)$/.exec(t);
    if (m) { side = m[1] === 'left' ? 1 : -1; t = m[2]; }
    const d = /^(.*?)(\d+)$/.exec(t);
    let w = d ? d[1] : t;
    if (d) idx = Number(d[2]) || idx;
    if (SIDE_WORDS[w] != null && !role) { side = SIDE_WORDS[w]; role = WORD_ROLE[w]; continue; }
    if (WORD_ROLE[w]) { if (!role || role === 'root' || role === 'jaw') role = WORD_ROLE[w]; continue; }
    // fused side prefix: lude, rhiji, lmomo, lmimi
    const f = /^([lr])([a-z]+)$/.exec(w);
    if (f && WORD_ROLE[f[2]] && !role) { side = f[1] === 'l' ? 1 : -1; role = WORD_ROLE[f[2]]; continue; }
    // trailing side letter: mimil, legl
    const g = /^([a-z]{2,})([lr])$/.exec(w);
    if (g && WORD_ROLE[g[1]] && !role) { side = g[2] === 'l' ? 1 : -1; role = WORD_ROLE[g[1]]; continue; }
  }
  return { role, side, idx, front };
}

// ---------- rig analysis (cached per template root) ----------
const rigCache = new WeakMap();
const _m = new THREE.Matrix4(), _inv = new THREE.Matrix4(), _v = new THREE.Vector3(), _s = new THREE.Vector3(), _q = new THREE.Quaternion();

function bonesOf(root) {
  const out = [];
  root.traverse((o) => { if (o.isBone) out.push(o); });
  return out;
}

// direction from each bone toward the vertices it moves, in bone space: used for leaf bones (Links legends' stubby
// arms and ears have no child bone to point at)
function vertexCentroids(root, bones) {
  const idx = new Map(bones.map((b, i) => [b, i]));
  const sum = bones.map(() => new THREE.Vector3());
  const n = bones.map(() => 0);
  const p = new THREE.Vector3();
  root.traverse((o) => {
    if (!o.isSkinnedMesh || !o.skeleton) return;
    const g = o.geometry;
    const pos = g.attributes.position, si = g.attributes.skinIndex, sw = g.attributes.skinWeight;
    if (!pos || !si || !sw) return;
    const sk = o.skeleton;
    const mats = sk.bones.map((b, j) => new THREE.Matrix4().multiplyMatrices(sk.boneInverses[j], o.bindMatrix));
    const step = Math.max(1, Math.floor(pos.count / 4000));
    for (let v = 0; v < pos.count; v += step) {
      for (let k = 0; k < 4; k++) {
        const w = sw.getComponent(v, k);
        if (w < 0.5) continue;
        const j = si.getComponent(v, k);
        const bi = idx.get(sk.bones[j]);
        if (bi == null) continue;
        p.fromBufferAttribute(pos, v).applyMatrix4(mats[j]);
        sum[bi].add(p);
        n[bi]++;
      }
    }
  });
  return sum.map((s, i) => (n[i] ? s.multiplyScalar(1 / n[i]) : null));
}

function analyze(root) {
  if (rigCache.has(root)) return rigCache.get(root);
  root.updateMatrixWorld(true);
  _inv.copy(root.matrixWorld).invert();
  const bones = bonesOf(root);
  const N = bones.length;
  const index = new Map(bones.map((b, i) => [b, i]));
  const info = bones.map((b) => {
    _m.multiplyMatrices(_inv, b.matrixWorld);
    const pos = new THREE.Vector3(), W0 = new THREE.Quaternion();
    _m.decompose(pos, W0, _s);
    const q0 = b.quaternion.clone();
    const P0 = W0.clone().multiply(q0.clone().invert());
    return { bone: b, name: b.name, pos, W0, P0, q0, mw: _m.clone(), parent: index.has(b.parent) ? index.get(b.parent) : -1, kids: [], depth: 0, ...parseName(b.name) };
  });
  for (let i = 0; i < N; i++) { const p = info[i].parent; if (p >= 0) info[p].kids.push(i); }
  for (let i = 0; i < N; i++) { let d = 0, p = info[i].parent; while (p >= 0) { d++; p = info[p].parent; } info[i].depth = d; }
  // subtree size, for picking the main child of a chain
  const size = new Array(N).fill(1);
  for (let i = N - 1; i >= 0; i--) for (const k of info[i].kids) size[i] += size[k];
  for (let i = 0; i < N; i++) info[i].size = size[i];
  for (const b of info) if (SKIP.test(b.name)) b.role = null;

  // facing: every model faces +Z (tests/upright.cjs audits that; tools/convert/upright.json fixes the odd ones)
  const yaw = 0;
  const left = { x: Math.cos(yaw), z: -Math.sin(yaw) }; // the character's left in model space
  for (const b of info) b.lat = b.pos.x * left.x + b.pos.z * left.z;
  // names that say left for bones on the right side: the rip is mirrored, swap them
  let agree = 0;
  for (const b of info) if (b.side) agree += b.side * b.lat;
  const mirrored = agree < -0.05;
  if (mirrored) for (const b of info) b.side = -b.side;
  for (const b of info) if (!b.side && Math.abs(b.lat) > 0.02 && b.role && !['hips', 'root', 'spine', 'chest', 'neck', 'head', 'jaw', 'tail'].includes(b.role)) b.side = Math.sign(b.lat);

  // bone directions (model space): toward the main child, else toward its own vertices, else away from the parent
  const cents = vertexCentroids(root, bones);
  for (let i = 0; i < N; i++) {
    const b = info[i];
    let c = -1;
    for (const k of b.kids) if (!SKIP.test(info[k].name) && (c < 0 || info[k].size > info[c].size)) c = k;
    const d = new THREE.Vector3();
    if (c >= 0) d.subVectors(info[c].pos, b.pos);
    if (d.lengthSq() < 1e-8 && cents[i]) d.copy(cents[i]).applyMatrix4(b.mw).sub(b.pos);
    if (d.lengthSq() < 1e-8 && b.parent >= 0) d.subVectors(b.pos, info[b.parent].pos);
    b.len = d.length();
    b.dir = d.lengthSq() > 1e-10 ? d.normalize() : new THREE.Vector3(0, 1, 0);
    b.child = c;
    b.leaf = c < 0;
  }
  const rig = { bones: info, N, yaw, mirrored, roles: findRolesIn(info) };
  rigCache.set(root, rig);
  return rig;
}

const isAnc = (info, a, b) => { let p = info[b].parent; while (p >= 0) { if (p === a) return true; p = info[p].parent; } return false; };
// chain down the main child that keeps the same side and is not an ear / wing / tail / weapon
function chainFrom(info, i, max = 4) {
  const out = [i];
  let cur = i;
  while (out.length < max) {
    const kids = info[cur].kids.filter((k) => !SKIP.test(info[k].name) && !['ear', 'wing', 'tail'].includes(info[k].role) || info[k].role === info[i].role);
    if (!kids.length) break;
    let best = kids[0];
    for (const k of kids) if (info[k].size > info[best].size) best = k;
    out.push(best);
    cur = best;
  }
  return out;
}

function findRolesIn(info) {
  const N = info.length;
  const R = { hips: -1, root: -1, spine: [], chest: -1, neck: -1, head: -1, arm: { 1: null, '-1': null }, leg: { 1: null, '-1': null }, tail: [], wing: { 1: [], '-1': [] }, ear: { 1: [], '-1': [] }, quadruped: false, via: {} };
  const byRole = (role, side) => info.map((b, i) => i).filter((i) => info[i].role === role && (side == null || info[i].side === side))
    .sort((a, b) => info[a].depth - info[b].depth || info[a].idx - info[b].idx || a - b);
  if (!N) return R;
  // hips: 'center' style name, else the bone with the biggest subtree that has 3+ children, else the top bone
  R.hips = byRole('hips')[0] ?? -1;
  if (R.hips < 0) {
    let best = -1;
    for (let i = 0; i < N; i++) if (info[i].kids.length >= 2 && (best < 0 || info[i].depth < info[best].depth)) best = i;
    R.hips = best >= 0 ? best : 0;
    R.via.hips = 'tree';
  }
  R.root = byRole('root')[0] ?? -1;
  // head
  R.head = byRole('head')[0] ?? -1;
  if (R.head < 0) {
    const jaws = byRole('jaw').filter((i) => info[i].side === 0);
    if (jaws.length) { R.head = jaws.sort((a, b) => info[b].pos.y - info[a].pos.y)[0]; R.via.head = 'jaw'; }
  }
  if (R.head < 0) {
    // geometry: the highest bone near the middle line under the hips that is not an ear, wing or tail
    let best = -1;
    for (let i = 0; i < N; i++) {
      const b = info[i];
      if (i === R.hips || Math.abs(b.lat) > 0.08 || ['ear', 'wing', 'tail', 'arm', 'hand', 'fore', 'thigh', 'shin', 'foot'].includes(b.role) || SKIP.test(b.name)) continue;
      if (!isAnc(info, R.hips, i)) continue;
      if (best < 0 || b.pos.y > info[best].pos.y) best = i;
    }
    if (best >= 0 && info[best].pos.y > info[R.hips].pos.y + 0.05) { R.head = best; R.via.head = 'geometry'; }
  }
  // arms, per side
  for (const side of [1, -1]) {
    const fores = byRole('fore', side);
    let up = -1;
    if (fores.length) {
      const p = info[fores[0]].parent;
      if (p >= 0 && p !== R.hips && !['spine', 'chest', 'neck', 'head', 'hips'].includes(info[p].role)) up = p;
    }
    if (up < 0) up = byRole('arm', side)[0] ?? -1;
    if (up < 0) up = byRole('shoulder', side)[0] ?? -1;
    if (up < 0) { const h = byRole('hand', side)[0]; if (h != null) up = h; }
    if (up < 0) continue;
    // a shoulder-named bone above a real arm bone is the clavicle
    const ch = chainFrom(info, up, 4);
    if (info[up].role === 'shoulder' && ch[1] != null && ['arm', 'fore'].includes(info[ch[1]].role) && info[ch[1]].role === 'arm') ch.shift();
    R.arm[side] = { up: ch[0], fore: ch[1] ?? -1, hand: ch[2] ?? -1 };
  }
  // legs, per side; a front pair (f_l_leg) becomes the arms when there are none
  const legsFor = (side, wantFront) => {
    const all = byRole('thigh', side).filter((i) => wantFront == null || (info[i].front === true) === wantFront);
    let th = all[0] ?? -1;
    if (th < 0) { const k = byRole('shin', side)[0]; if (k != null) th = info[k].parent >= 0 && info[info[k].parent].side === side ? info[k].parent : k; }
    if (th < 0) return null;
    const ch = chainFrom(info, th, 3);
    return { thigh: ch[0], shin: ch[1] ?? -1, foot: ch[2] ?? -1 };
  };
  for (const side of [1, -1]) {
    const hasFront = byRole('thigh', side).some((i) => info[i].front === true);
    R.leg[side] = legsFor(side, hasFront ? false : null);
    if (hasFront && !R.arm[side]) {
      const f = legsFor(side, true);
      if (f) { R.arm[side] = { up: f.thigh, fore: f.shin, hand: f.foot }; R.quadruped = true; }
    }
  }
  // geometry fallback for limbs: chains that branch off the middle line
  if (!R.arm[1] && !R.arm[-1] || !R.leg[1] && !R.leg[-1]) {
    const used = new Set();
    for (const s of [1, -1]) for (const g of [R.arm[s], R.leg[s]]) if (g) for (const v of Object.values(g)) used.add(v);
    const hipsY = info[R.hips].pos.y;
    for (let i = 0; i < N; i++) {
      const b = info[i];
      if (used.has(i) || SKIP.test(b.name) || ['ear', 'wing', 'tail', 'head', 'jaw', 'neck'].includes(b.role)) continue;
      const p = b.parent;
      if (p < 0 || Math.abs(b.lat) < 0.06 || Math.abs(info[p].lat) > 0.04) continue;
      const ch = chainFrom(info, i, 3);
      const end = info[ch[ch.length - 1]];
      const side = Math.sign(b.lat);
      if (end.pos.y < 0.2 && b.pos.y <= hipsY + 0.1) { if (!R.leg[side]) { R.leg[side] = { thigh: ch[0], shin: ch[1] ?? -1, foot: ch[2] ?? -1 }; R.via.leg = 'geometry'; } }
      else if (b.pos.y > hipsY - 0.05 && !R.arm[side]) { R.arm[side] = { up: ch[0], fore: ch[1] ?? -1, hand: ch[2] ?? -1 }; R.via.arm = 'geometry'; }
    }
  }
  // arms that reach the floor are front legs (quadrupeds, Patamon)
  for (const s of [1, -1]) {
    const a = R.arm[s];
    if (!a) continue;
    const endI = a.hand >= 0 ? a.hand : a.fore >= 0 ? a.fore : a.up;
    if (info[endI].pos.y < 0.12 && info[a.up].pos.y < 0.45) R.quadruped = true;
  }
  // spine: the bones between the hips and the head (or the arms)
  const top = R.head >= 0 ? R.head : (R.arm[1] && R.arm[1].up) ?? (R.arm[-1] && R.arm[-1].up) ?? -1;
  if (top >= 0) {
    const path = [];
    let p = info[top].parent;
    while (p >= 0 && p !== R.hips) { path.unshift(p); p = info[p].parent; }
    if (p === R.hips) {
      for (const i of path) {
        if (info[i].role === 'neck' && R.head >= 0) { if (R.neck < 0) R.neck = i; continue; }
        if (['clav', 'shoulder'].includes(info[i].role) || info[i].side) continue;
        R.spine.push(i);
      }
    }
    if (R.neck < 0 && R.head >= 0) { const n = byRole('neck')[0]; if (n != null && isAnc(info, n, R.head)) R.neck = n; }
  }
  if (R.spine.length) R.chest = R.spine[R.spine.length - 1];
  // the arms' common parent counts as the chest even when it is not on the head's path
  // tail, wings, ears
  const t = byRole('tail').filter((i) => !info[i].side)[0];
  if (t != null) R.tail = chainFrom(info, t, 5).filter((i) => info[i].role === 'tail' || info[i].role == null);
  for (const s of [1, -1]) {
    const w = byRole('wing', s)[0];
    if (w != null) R.wing[s] = chainFrom(info, w, 3);
    const e = byRole('ear', s)[0];
    if (e != null) R.ear[s] = chainFrom(info, e, 3);
  }
  return R;
}

// public, readable summary for tests and debugging
function findRoles(root) {
  const rig = analyze(root);
  const R = rig.roles, B = rig.bones;
  const nm = (i) => (i >= 0 && B[i] ? B[i].name : null);
  const limb = (g) => (g ? Object.fromEntries(Object.entries(g).map(([k, v]) => [k, nm(v)])) : null);
  return {
    bones: rig.N, mirrored: rig.mirrored, quadruped: R.quadruped, via: R.via,
    root: nm(R.root), hips: nm(R.hips), spine: R.spine.map(nm), chest: nm(R.chest), neck: nm(R.neck), head: nm(R.head),
    armL: limb(R.arm[1]), armR: limb(R.arm[-1]), legL: limb(R.leg[1]), legR: limb(R.leg[-1]),
    tail: R.tail.map(nm), wingL: R.wing[1].map(nm), wingR: R.wing[-1].map(nm), earL: R.ear[1].map(nm), earR: R.ear[-1].map(nm),
  };
}

// ---------- dance definitions ----------
// A dance is pose(b) for b beats since the start. Pose channels (all optional), in character space where +x is the
// character's left, +y up and +z forward:
//   aL / aR  upper arm direction        fL / fR  forearm direction (absolute)
//   lL / lR  thigh direction            kL / kR  shin direction (absolute); legs left out stay planted
//   hp [x, y, z] hips rotation, degrees     sp [bow, twist, lean] spine, degrees     hd [nod, turn, tilt] head, degrees
//   tl tail wag, degrees     w wing flap -1..1     rt { x, y, z, rx, ry, rz, piv, sq } root move in model heights / radians
const D = {
  up: [0.12, 1, 0.02], out: [1, 0.05, 0.05], upOut: [0.7, 0.7, 0.08], downOut: [0.55, -0.83, 0.05], down: [0.15, -1, 0.06],
  fwd: [0.12, 0, 1], fwdUp: [0.15, 0.7, 0.7], fwdDown: [0.18, -0.6, 0.78], cross: [-0.5, -0.15, 0.85], crossUp: [-0.4, 0.5, 0.75],
  back: [0.3, -0.6, -0.75], hip: [0.75, -0.55, -0.15], point: [0.55, 0.8, 0.25],
  // forearms
  in: [-0.85, 0.1, 0.5], toHip: [-0.55, -0.55, 0.3], toHead: [-0.75, 0.35, -0.45], toShoulder: [-0.75, 0.55, -0.2],
  // legs
  lDown: [0.06, -1, 0.02], kneeUp: [0.08, -0.15, 1], kick: [0.08, -0.45, 0.9], lBack: [0.06, -0.8, -0.6], lOut: [0.55, -0.85, 0.05], lIn: [-0.15, -0.98, 0.1],
  shinBack: [0.03, -0.75, -0.66],
};
const L = (n) => (typeof n === 'string' ? D[n] : n);
const Rt = (n) => { const d = L(n); return [-d[0], d[1], d[2]]; };
const mix = (a, b, k) => [a[0] + (b[0] - a[0]) * k, a[1] + (b[1] - a[1]) * k, a[2] + (b[2] - a[2]) * k];
const mixN = (a, b, k) => a + (b - a) * k;
const fr = (b, n = 1) => (((b / n) % 1) + 1) % 1; // phase within n beats
const osc = (b, n = 2) => Math.sin(fr(b, n) * TAU); // one full swing every n beats
const onBeat = (b, sharp = 6) => Math.exp(-fr(b) * sharp); // accent right on each beat
const hop = (b, n = 1) => Math.abs(Math.sin((Math.PI * b) / n)); // 0 on the beat, 1 between
const smooth = (k) => k * k * (3 - 2 * k);
const snap = (k) => (k < 0.22 ? smooth(k / 0.22) : 1);
const sq = (x, k = 2.5) => Math.tanh(k * x) / Math.tanh(k); // squarer wave
// key poses one per `step` beats over the loop, eased; channels must be present in every key that uses them
function keys(b, list, step = 1, ease = smooth) {
  const n = list.length;
  const pos = fr(b, n * step) * n;
  const i = Math.floor(pos), k = ease(pos - i);
  const a = list[i], c = list[(i + 1) % n];
  const o = {};
  for (const ch of Object.keys(a)) {
    const x = a[ch], y = c[ch];
    if (y == null) o[ch] = x;
    else if (Array.isArray(x)) o[ch] = mix(x, y, k);
    else if (typeof x === 'number') o[ch] = mixN(x, y, k);
    else if (x && typeof x === 'object') { o[ch] = {}; for (const kk of Object.keys(x)) o[ch][kk] = mixN(x[kk], y[kk] ?? x[kk], k); }
  }
  return o;
}

const DANCES = [
  {
    id: 'floss', name: 'The Floss', bpm: 120, rarity: 'common', free: true, icon: '🦷',
    desc: 'Straight arms swing side to side, one in front and one behind, hips going the other way.',
    pose(b) {
      const s = sq(Math.sin(Math.PI * b), 3);
      const f = sq(Math.sin((Math.PI * b) / 2 + 0.6), 3); // which arm is in front flips every other swing
      return {
        aL: [-0.55 * s, -0.83, 0.32 * f], aR: [-0.55 * s, -0.83, -0.32 * f],
        hp: [0, 0, 12 * s], sp: [4, 0, -10 * s], hd: [6 * onBeat(b), 0, 6 * s], tl: 25 * s, w: 0.3 * s,
        rt: { x: 0.05 * s, y: 0.025 * hop(b) },
      };
    },
  },
  {
    id: 'robot', name: 'The Robot', bpm: 100, rarity: 'common', icon: '🤖',
    desc: 'Stiff, snapping arm and head positions on every beat.',
    pose(b) {
      const p = keys(b, [
        { aL: L('out'), fL: L('up'), aR: Rt('down'), fR: Rt('down'), hd: [0, 30, 0], sp: [0, 10, 0], rt: { ry: 0.2 } },
        { aL: L('out'), fL: L('up'), aR: Rt('out'), fR: Rt('down'), hd: [0, 0, 0], sp: [0, 0, 0], rt: { ry: 0 } },
        { aL: L('fwd'), fL: L('fwd'), aR: Rt('fwd'), fR: Rt('fwd'), hd: [10, -30, 0], sp: [0, -12, 0], rt: { ry: -0.25 } },
        { aL: L('down'), fL: L('fwd'), aR: Rt('down'), fR: Rt('fwd'), hd: [-8, 0, 0], sp: [6, 0, 0], rt: { ry: 0 } },
        { aL: L('down'), fL: L('down'), aR: Rt('out'), fR: Rt('up'), hd: [0, -30, 0], sp: [0, -10, 0], rt: { ry: -0.2 } },
        { aL: L('out'), fL: L('down'), aR: Rt('out'), fR: Rt('up'), hd: [0, 0, 0], sp: [0, 0, 0], rt: { ry: 0 } },
        { aL: L('fwdUp'), fL: L('fwdUp'), aR: Rt('fwdDown'), fR: Rt('fwd'), hd: [0, 25, 12], sp: [0, 12, 0], rt: { ry: 0.25 } },
        { aL: L('down'), fL: L('fwd'), aR: Rt('down'), fR: Rt('fwd'), hd: [12, 0, 0], sp: [0, 0, 0], rt: { ry: 0 } },
      ], 1, snap);
      p.rt.y = -0.02 * onBeat(b, 14);
      p.tl = 20 * Math.sign(osc(b, 2));
      return p;
    },
  },
  {
    id: 'running', name: 'The Running Man', bpm: 128, rarity: 'rare', icon: '🏃',
    desc: 'Knee up, slide back, switch legs, arms pumping. Running on the spot.',
    pose(b) {
      const k = fr(b, 2), l = k < 0.5 ? smooth(k * 2) : 1 - smooth((k - 0.5) * 2); // 1 = left knee up
      const r = 1 - l;
      return {
        lL: mix(L('lBack'), L('kneeUp'), l), kL: mix(L('shinBack'), L('lDown'), l),
        lR: mix(Rt('lBack'), Rt('kneeUp'), r), kR: mix(Rt('shinBack'), Rt('lDown'), r),
        aL: mix(L('fwdDown'), L('back'), l), fL: mix(L('fwdUp'), L('fwd'), l),
        aR: mix(Rt('fwdDown'), Rt('back'), r), fR: mix(Rt('fwdUp'), Rt('fwd'), r),
        sp: [10, 8 * (l - r), 0], hd: [5 * onBeat(b), 0, 0], tl: 20 * (l - r),
        rt: { y: 0.05 * hop(b), z: 0.03 * Math.sin(Math.PI * b) },
      };
    },
  },
  {
    id: 'disco', name: 'Disco Point', bpm: 110, rarity: 'rare', icon: '🕺',
    desc: 'Point to the sky, then down across to the floor, other hand on the hip.',
    pose(b) {
      const side = Math.floor(b / 8) % 2 ? -1 : 1; // the pointing arm swaps every 8 beats
      const k = fr(b, 4);
      const upK = k < 0.5 ? snap(k * 2) : 1 - snap((k - 0.5) * 2);
      const point = mix([0.45, -0.75, 0.45], L('point'), upK);
      const hipArm = L('hip'), hipFore = L('toHip');
      const P = (v) => (side > 0 ? [-v[0], v[1], v[2]] : v); // right arm points first
      const H = (v) => (side > 0 ? v : [-v[0], v[1], v[2]]);
      const o = {
        hp: [0, 0, 10 * Math.sin(Math.PI * b)], sp: [0, -12 * side * upK, -6 * side * upK],
        hd: [mixN(18, -14, upK), -20 * side * upK + 10 * side * (1 - upK), 0], tl: 20 * Math.sin(Math.PI * b),
        rt: { x: 0.04 * Math.sin(Math.PI * b), y: 0.02 * hop(b) },
      };
      if (side > 0) { o.aR = P(point); o.aL = H(hipArm); o.fL = H(hipFore); } else { o.aL = P(point); o.aR = H(hipArm); o.fR = H(hipFore); }
      return o;
    },
  },
  {
    id: 'macarena', name: 'Macarena Moves', bpm: 103, rarity: 'epic', icon: '💃',
    desc: 'Arms out, palms, shoulders, head, hips, a hip circle and a quarter turn. 16 counts.',
    pose(b) {
      const n = Math.floor(fr(b, 16) * 16);
      const k = snap(fr(b));
      const st = [ // what each arm does after count i
        ['fwd', null, null, null], ['fwd', 'fwd', null, null], ['fwd', 'fwd', null, null], ['fwd', 'fwd', null, null],
        ['shoulder', 'fwd'], ['shoulder', 'shoulder'], ['head', 'shoulder'], ['head', 'head'],
        ['hipF', 'head'], ['hipF', 'hipF'], ['butt', 'hipF'], ['butt', 'butt'], ['butt', 'butt'], ['butt', 'butt'], ['butt', 'butt'], ['butt', 'butt'],
      ];
      // right arm first in each pair. Upper arm and forearm for the right side; the left is mirrored
      const A = { rest: [L('down'), L('down')], fwd: [L('fwd'), L('fwd')], shoulder: [[-0.45, 0.05, 0.9], [-0.7, 0.55, -0.4]],
        head: [L('upOut'), L('toHead')], hipF: [[-0.25, -0.85, 0.45], [-0.6, -0.4, 0.4]], butt: [[0.35, -0.75, -0.55], [-0.3, -0.5, -0.6]] };
      const pr = (i) => st[(i + 16) % 16];
      const get = (i, which) => A[pr(i)[which] || 'rest'];
      const right = (i) => get(i, 0).map((v) => [-v[0], v[1], v[2]]);
      const left = (i) => get(i, 1);
      const cr = n, pv = n - 1;
      const rA = mix(right(pv)[0], right(cr)[0], k), rF = mix(right(pv)[1], right(cr)[1], k);
      const lA = mix(left(pv)[0], left(cr)[0], k), lF = mix(left(pv)[1], left(cr)[1], k);
      const circle = n >= 12 && n < 15 ? fr(b, 1) * TAU : 0;
      const jump = n === 15 ? Math.sin(Math.PI * fr(b)) : 0;
      const turn = Math.floor(b / 16) * (Math.PI / 2) + (n === 15 ? smooth(fr(b)) * (Math.PI / 2) : 0);
      return {
        aR: rA, fR: rF, aL: lA, fL: lF,
        hp: circle ? [12 * Math.sin(circle), 0, 12 * Math.cos(circle)] : [0, 0, 6 * Math.sin(Math.PI * b)],
        hd: [4 * onBeat(b), 0, 0], tl: circle ? 30 * Math.sin(circle) : 10 * Math.sin(Math.PI * b),
        rt: { y: 0.15 * jump + 0.015 * hop(b), ry: turn, x: circle ? 0.04 * Math.sin(circle) : 0 },
      };
    },
  },
  {
    id: 'windmill', name: 'Windmill Breakdance', bpm: 126, rarity: 'legendary', icon: '🌀',
    desc: 'Toprock, drop to the floor, spin on the back with legs flying, then freeze.',
    pose(b) {
      const c = fr(b, 16) * 16;
      if (c < 4) { // toprock: cross step with arms crossing
        const s = Math.sin(Math.PI * c);
        return { aL: mix(L('downOut'), L('cross'), (s + 1) / 2), aR: mix(Rt('downOut'), Rt('cross'), (1 - s) / 2), fL: L('fwd'), fR: Rt('fwd'),
          lL: s > 0 ? L('kick') : L('lDown'), lR: s < 0 ? Rt('kick') : Rt('lDown'), kL: L('lDown'), kR: Rt('lDown'), sp: [10, 15 * s, 0], hd: [8 * onBeat(b), 0, 0], tl: 20 * s,
          rt: { y: 0.03 * hop(c) } };
      }
      const drop = smooth(Math.min(1, (c - 4) / 1.5));
      const rise = c > 14 ? smooth(Math.min(1, (c - 14) / 0.8)) : 0;
      const spin = c < 5.5 ? 0 : (c < 14 ? (c - 5.5) : 8.5 + smooth(Math.min(1, (c - 14) / 2)) * 0.5) * (TAU / 2);
      const tilt = drop * (1 - rise * 0.7);
      const legsUp = c > 5.5 && c < 14 ? 1 : 0;
      const sw = Math.sin(spin);
      return {
        aL: rise ? L('up') : L('out'), aR: rise ? Rt('out') : Rt('out'),
        lL: legsUp ? mix(L('lOut'), [0.7, 0.2, 0.7], 0.5 + 0.5 * sw) : L('kneeUp'), lR: legsUp ? mix(Rt('lOut'), [-0.7, 0.2, 0.7], 0.5 - 0.5 * sw) : Rt('lDown'),
        kL: legsUp ? undefined : L('lDown'), kR: legsUp ? undefined : Rt('lDown'),
        sp: [-10 * tilt, 0, 10 * sw * tilt], hd: [-15 * tilt, 0, 0], tl: 30 * sw, w: 0.6 * sw,
        rt: { rx: -1.15 * tilt, ry: spin, rz: 0.25 * sw * tilt, piv: 0.32, y: 0.03 * tilt },
      };
    },
  },
  {
    id: 'gangnam', name: 'Horse Riding', bpm: 132, rarity: 'epic', icon: '🐎',
    desc: 'Wrists crossed on the reins, a skipping gallop, then the lasso. Gangnam style.',
    pose(b) {
      const k = fr(b, 2);
      const lUp = k < 0.5 ? Math.sin(Math.PI * fr(b * 2)) : 0, rUp = k >= 0.5 ? Math.sin(Math.PI * fr(b * 2)) : 0; // L-L R-R
      const lasso = Math.floor(b / 8) % 2 === 1;
      const o = {
        lL: mix([0.3, -0.9, 0.2], [0.3, -0.45, 0.85], lUp), kL: mix([0.1, -1, 0], [0.05, -1, 0.2], lUp),
        lR: mix([-0.3, -0.9, 0.2], [-0.3, -0.45, 0.85], rUp), kR: mix([-0.1, -1, 0], [-0.05, -1, 0.2], rUp),
        aL: [-0.15, -0.35, 0.92], fL: [-0.5, 0.0, 0.85],
        aR: [0.15, -0.35, 0.92], fR: [0.5, 0.02, 0.85],
        sp: [12, 0, 0], hd: [-6 + 10 * onBeat(b * 2), 0, 0], tl: 25 * Math.sin(TAU * b), w: 0.4 * Math.sin(TAU * b),
        rt: { y: 0.05 * hop(b * 2), x: 0.02 * Math.sin(Math.PI * b) },
      };
      if (lasso) {
        const a = fr(b, 1) * TAU;
        o.aR = [-0.2, 1, 0.1];
        o.fR = [0.75 * Math.cos(a), 0.45, 0.75 * Math.sin(a)];
        o.hd = [-12, -15, 0];
      }
      return o;
    },
  },
  {
    id: 'jig', name: 'Victory Jig', bpm: 140, rarity: 'common', free: true, icon: '🎉',
    desc: 'Hop, kick, hop, kick, fists up and a spin to finish.',
    pose(b) {
      const c = fr(b, 8) * 8;
      const left = Math.floor(b) % 2 === 0;
      const kick = Math.sin(Math.PI * fr(b));
      const pump = c >= 6 ? 1 : 0;
      const spin = c >= 6 ? smooth((c - 6) / 2) * TAU : 0;
      return {
        lL: left ? mix(L('lDown'), L('kick'), kick) : L('lDown'), kL: left ? mix(L('lDown'), L('kick'), kick) : L('lDown'),
        lR: !left ? mix(Rt('lDown'), Rt('kick'), kick) : Rt('lDown'), kR: !left ? mix(Rt('lDown'), Rt('kick'), kick) : Rt('lDown'),
        aL: pump ? L('up') : mix(L('hip'), L('upOut'), left ? 0 : kick * 0.6), fL: pump ? L('up') : left ? L('toHip') : L('upOut'),
        aR: pump ? Rt('up') : mix(Rt('hip'), Rt('upOut'), left ? kick * 0.6 : 0), fR: pump ? Rt('up') : left ? Rt('upOut') : Rt('toHip'),
        sp: [-4, 0, left ? 6 : -6], hd: [-8 * kick, 0, left ? 8 : -8], tl: left ? 30 : -30, w: pump ? Math.sin(TAU * b * 2) : 0.3,
        rt: { y: 0.12 * hop(b), ry: spin },
      };
    },
  },
  {
    id: 'shuffle', name: 'Digivice Shuffle', bpm: 124, rarity: 'epic', icon: '📟',
    desc: 'Feet sliding in and out, one hand holding the Digivice up high.',
    pose(b) {
      const s = Math.sin(Math.PI * b); // feet out on one beat, in on the next
      const out = (s + 1) / 2;
      const lift = Math.floor(b) % 2 ? 0 : Math.sin(Math.PI * fr(b));
      return {
        lL: mix(L('lIn'), L('lOut'), out), lR: mix(Rt('lIn'), Rt('lOut'), 1 - out),
        kL: lift ? mix(L('lDown'), L('shinBack'), lift) : L('lDown'), kR: Rt('lDown'),
        aR: [-0.25, 0.6, 0.75], fR: [-0.05, 0.95, 0.3],
        aL: mix(L('downOut'), L('out'), onBeat(b, 4)), fL: L('fwdUp'),
        sp: [4, -10, 0], hd: [-12 + 6 * onBeat(b), -15, 0], tl: 25 * s, w: 0.4 * s,
        rt: { x: 0.06 * s, y: 0.025 * hop(b), ry: 0.25 * Math.sin((Math.PI * b) / 4) },
      };
    },
  },
  {
    id: 'butterfly', name: 'Butter-fly Sway', bpm: 96, rarity: 'rare', icon: '🦋',
    desc: 'Slow wing-arm flaps and a sway from side to side, like the theme song.',
    pose(b) {
      const flap = 0.5 + 0.5 * Math.sin(Math.PI * b);
      const sway = Math.sin((Math.PI * b) / 2);
      return {
        aL: mix(L('downOut'), L('upOut'), flap), fL: mix(L('out'), [0.6, 0.8, 0.05], flap),
        aR: mix(Rt('downOut'), Rt('upOut'), flap), fR: mix(Rt('out'), [-0.6, 0.8, 0.05], flap),
        hp: [0, 0, 12 * sway], sp: [0, 10 * sway, -8 * sway], hd: [-6 * flap, 0, -14 * sway], tl: 25 * sway, w: flap * 2 - 1,
        rt: { x: 0.07 * sway, y: 0.05 * flap, ry: 0.35 * sway },
      };
    },
  },
  {
    id: 'headbang', name: 'Headbanger', bpm: 150, rarity: 'rare', icon: '🤘',
    desc: 'Big head nods on every beat with air guitar, horns up every other bar.',
    pose(b) {
      const hit = onBeat(b, 5);
      const horns = Math.floor(b / 8) % 2 === 1 && fr(b, 8) > 0.5;
      const strum = 0.5 + 0.5 * Math.cos(TAU * b * 2);
      return {
        hd: [-10 + 42 * hit, 0, 0], sp: [4 + 16 * hit, 0, 0],
        aL: [0.75, -0.2, 0.6], fL: [0.65, 0.05, 0.75],
        aR: horns ? Rt('up') : [-0.25, -0.6, 0.75], fR: horns ? Rt('up') : mix([0.65, -0.45, 0.55], [0.6, 0.15, 0.7], strum),
        lL: L('lOut'), lR: Rt('lOut'), tl: 30 * (hit - 0.5), w: hit,
        rt: { y: -0.03 * hit + 0.01, z: 0.02 * hit },
      };
    },
  },
];

// ---------- catalog ----------
DANCES.push(...mysteryDances);
const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };
DT.Cosmetics.register('dance', DANCES.map((d) => ({
  ...(d.secret ? { secret: true, lootPool: d.lootPool, egg: false, tag: d.tag } : {}),
  id: d.id, name: d.name, rarity: d.rarity, price: d.free ? 0 : PRICE[d.rarity], icon: d.icon,
  ...(d.free ? { free: true } : {}),
  desc: `${d.desc} ${d.bpm} BPM.`,
  data: { bpm: d.bpm },
  preview: (el) => preview(el, d.id),
})));
const danceOf = (id) => DANCES.find((d) => d.id === id) || DANCES.find((d) => d.id === C.equipped('dance')) || DANCES[0];

// ---------- the animator ----------
// limits per role, radians: big enough to read, small enough that the skinning holds
const LIM = { hips: 30, spine: 28, neck: 25, head: 40, up: 155, fore: 140, hand: 35, thigh: 95, shin: 110, foot: 35, tail: 35, wing: 50, ear: 45, leafArm: 65, quad: 55 };
const Q = () => new THREE.Quaternion();
const qI = new THREE.Quaternion();
const _a = new THREE.Vector3(), _b = new THREE.Vector3(), _e = new THREE.Euler();
const _q1 = Q(), _q2 = Q(), _q3 = Q();
const ROOT_KEYS = ['x', 'y', 'z', 'rx', 'ry', 'rz'];

function clampQ(q, lim) {
  if (q.w < 0) q.set(-q.x, -q.y, -q.z, -q.w);
  const ang = 2 * Math.acos(Math.min(1, q.w));
  if (ang > lim && ang > 1e-6) {
    const s = Math.sin(lim / 2) / Math.sin(ang / 2);
    q.set(q.x * s, q.y * s, q.z * s, Math.cos(lim / 2));
  }
  return q;
}

export class Dancer {
  // obj: the model to move (a clone of templateRoot, or the template itself). templateRoot: an unposed copy in its
  // idle pose, used to measure the skeleton once per model.
  constructor(obj, templateRoot) {
    this.obj = obj;
    let rig = analyze(templateRoot || obj);
    let bones = bonesOf(obj);
    if (bones.length !== rig.N) { rig = analyze(obj); bones = bonesOf(obj); } // a different model: measure this one
    this.rig = rig;
    this.bones = bones;
    this.dance = DANCES[0];
    this.build();
  }

  build() {
    const { roles: R, bones: B, yaw } = this.rig;
    const E = [];
    const add = (bi, kind, extra = {}) => { if (bi == null || bi < 0 || E.some((e) => e.bi === bi)) return null; const e = { bi, kind, ...extra }; E.push(e); return e; };
    add(R.hips, 'hips');
    R.spine.forEach((bi) => add(bi, 'spine', { share: 1 / R.spine.length }));
    add(R.neck, 'neck');
    add(R.head, 'head', { share: R.neck >= 0 ? 0.7 : 1 });
    this.hasArms = false;
    this.stubby = true; // arms missing or only stubs (Links legends): the ears and body carry the dance
    for (const s of [1, -1]) {
      const a = R.arm[s];
      if (a) {
        this.hasArms = true;
        const leaf = B[a.up].leaf || a.fore < 0;
        if (!leaf && !R.quadruped) this.stubby = false;
        add(a.up, 'up', { side: s, lim: (R.quadruped ? LIM.quad : leaf ? LIM.leafArm : LIM.up) * DEG });
        if (a.fore >= 0) add(a.fore, 'fore', { side: s, lim: (R.quadruped ? LIM.quad : LIM.fore) * DEG });
      }
      const l = R.leg[s];
      if (l) {
        add(l.thigh, 'thigh', { side: s, lim: LIM.thigh * DEG });
        if (l.shin >= 0) add(l.shin, 'shin', { side: s, lim: LIM.shin * DEG });
        if (l.foot >= 0) add(l.foot, 'foot', { side: s, lim: LIM.foot * DEG });
      }
      R.wing[s].forEach((bi, n) => add(bi, 'wing', { side: s, share: n ? 0.35 : 1 }));
      R.ear[s].forEach((bi, n) => add(bi, 'ear', { side: s, share: n ? 0.4 : 1 }));
    }
    R.tail.forEach((bi, n) => add(bi, 'tail', { share: 1 / R.tail.length, n }));
    // parents first, and each entry knows its nearest driven ancestor
    E.sort((x, y) => B[x.bi].depth - B[y.bi].depth);
    const at = new Map(E.map((e, i) => [e.bi, i]));
    for (const e of E) {
      e.pe = -1;
      let p = B[e.bi].parent;
      while (p >= 0) { if (at.has(p)) { e.pe = at.get(p); break; } p = B[p].parent; }
      e.R = Q(); e.A = Q(); e.local = Q();
      e.d0 = B[e.bi].dir;
    }
    this.E = E;
    this.fq = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0, 1, 0), yaw || 0);
    this.fqi = this.fq.clone().invert();
    this.limbless = !this.hasArms && !R.leg[1] && !R.leg[-1];
    this.hasEars = R.ear[1].length + R.ear[-1].length > 0;
  }

  set(id) { this.dance = danceOf(id); return this; }
  get bpm() { return this.dance.bpm; }

  // character space -> model space
  dirV(v, out) {
    out.set(v[0], v[1], v[2]);
    if (out.lengthSq() < 1e-8) out.set(0, 1, 0);
    return out.normalize().applyQuaternion(this.fq);
  }
  eulerQ(deg, scale, out) {
    if (!deg) return out.identity();
    _e.set(deg[0] * DEG * scale, deg[1] * DEG * scale, -deg[2] * DEG * scale, 'YXZ');
    return out.setFromEuler(_e).premultiply(this.fq).multiply(this.fqi);
  }
  // rotation that turns this entry's rest direction toward `target` (model space), given what its parents did
  aim(e, target, out) {
    const Ap = e.pe >= 0 ? this.E[e.pe].A : qI;
    _b.copy(target).applyQuaternion(_q3.copy(Ap).invert());
    return out.setFromUnitVectors(e.d0, _b);
  }

  // Pose the bones for `t` seconds into the dance. weight 0..1 blends from the current pose.
  // Returns the root move { x, y, z, rx, ry, rz, piv, sq } in model heights / radians, already weighted.
  apply(t, weight = 1) {
    const b = (t * this.dance.bpm) / 60;
    const p = this.dance.pose(b) || {};
    const B = this.rig.bones;
    const w = Math.max(0, Math.min(1, weight));
    const body = this.limbless ? 1.5 : this.stubby ? 1.25 : 1;
    // arms raised -> a limbless body stretches and leans toward them instead
    const aL = p.aL ? this.dirV(p.aL, new THREE.Vector3()) : null, aR = p.aR ? this.dirV(p.aR, new THREE.Vector3()) : null;
    for (const e of this.E) {
      const bone = B[e.bi];
      const R = e.R;
      switch (e.kind) {
        case 'hips': this.eulerQ(p.hp, body, R); break;
        case 'spine': this.eulerQ(p.sp, e.share * body, R); break;
        case 'neck': this.eulerQ(p.hd, 0.3, R); break;
        case 'head': this.eulerQ(p.hd, e.share, R); break;
        case 'up': case 'fore': {
          const src = e.kind === 'up' ? (e.side > 0 ? p.aL : p.aR) : (e.side > 0 ? p.fL : p.fR);
          if (src) this.aim(e, this.dirV(src, _a), R); else R.identity();
          break;
        }
        case 'thigh': case 'shin': case 'foot': {
          const src = e.kind === 'thigh' ? (e.side > 0 ? p.lL : p.lR) : e.kind === 'shin' ? (e.side > 0 ? p.kL : p.kR) : null;
          if (src) this.aim(e, this.dirV(src, _a), R);
          else this.aim(e, e.d0, R); // stay planted: undo what the hips and knees did
          break;
        }
        case 'tail': _e.set(Math.sin((e.n + 1) * 0.9) * 0.1 * (p.tl ? 1 : 0), (p.tl || 0) * DEG * e.share * 1.6, 0, 'YXZ'); R.setFromEuler(_e); break;
        case 'wing': {
          const f = p.w != null ? p.w : (aL && aR ? (aL.y + aR.y) / 2 : 0);
          _a.set(0, 0, 1).applyQuaternion(this.fq);
          R.setFromAxisAngle(_a, f * 0.5 * e.side * e.share);
          break;
        }
        case 'ear': {
          const a = e.side > 0 ? aL : aR;
          // ears stand in for stubby arms: arms down = ears up, arms out = ears out, arms up = ears up and back
          if (a && this.stubby) { _b.set(a.x, Math.max(Math.abs(a.y), 0.35), a.z - (a.y > 0.5 ? 0.3 : 0)).normalize(); this.aim(e, _b.clone(), R); R.slerp(qI, 1 - 0.6 * e.share);
            if (p.w) R.premultiply(_q2.setFromAxisAngle(_a.set(0, 0, 1).applyQuaternion(this.fq), p.w * 0.35 * e.side)); }
          else this.eulerQ([0, 0, (p.hd ? p.hd[2] : 0) * 0.6 + (p.tl || 0) * 0.3], 1, R);
          break;
        }
        default: R.identity();
      }
      const lim = e.lim || (LIM[e.kind] || 40) * DEG;
      clampQ(R, lim);
      e.A.copy(e.pe >= 0 ? this.E[e.pe].A : qI).multiply(R);
      // local = P0^-1 * R * P0 * q0: R is a model-space turn applied at this bone's rest pose
      e.local.copy(bone.P0).invert().multiply(R).multiply(bone.P0).multiply(bone.q0);
      const tgt = this.bones[e.bi];
      if (w >= 1) tgt.quaternion.copy(e.local);
      else tgt.quaternion.slerp(e.local, w);
    }
    const rt = Object.assign({}, p.rt || {});
    for (const k of ROOT_KEYS) rt[k] = (rt[k] || 0) * w;
    rt.fq = this.fq;
    if (this.limbless) {
      rt.y *= 1.4;
      const up = aL && aR ? (aL.y + aR.y) / 2 : 0;
      rt.sq = 1 + 0.08 * up * w;
      if (aL && aR) rt.rz += -0.12 * ((aL.x + aR.x) / 2) * w;
    }
    return rt;
  }

  // back to the measured rest pose (for models without a mixer)
  clear() { for (const e of this.E) this.bones[e.bi].quaternion.copy(this.rig.bones[e.bi].q0); }
}

// Root move onto an object whose transform was just reset: base yaw and scale come from the object itself.
// rt is in character space; rt.fq turns it into model space for rips that stand turned.
const _rq = new THREE.Quaternion(), _pv = new THREE.Vector3(), _tv = new THREE.Vector3();
function applyRoot(obj, rt, s) {
  _e.set(rt.rx || 0, rt.ry || 0, rt.rz || 0, 'YXZ');
  _rq.setFromEuler(_e);
  if (rt.fq) _rq.premultiply(rt.fq).multiply(_q1.copy(rt.fq).invert());
  const piv = (rt.piv || 0) * s;
  // turn about a point `piv` above the feet so a body lying down stays over its spot
  _pv.set(0, piv, 0).sub(_v.set(0, piv, 0).applyQuaternion(_rq)).applyQuaternion(obj.quaternion);
  _tv.set((rt.x || 0) * s, (rt.y || 0) * s, (rt.z || 0) * s);
  if (rt.fq) _tv.applyQuaternion(rt.fq);
  _tv.applyQuaternion(obj.quaternion);
  obj.quaternion.multiply(_rq);
  obj.position.x += _tv.x + _pv.x;
  obj.position.y += _tv.y + _pv.y;
  obj.position.z += _tv.z + _pv.z;
  if (rt.sq && rt.sq !== 1) { obj.scale.y *= rt.sq; const k = 1 / Math.sqrt(rt.sq); obj.scale.x *= k; obj.scale.z *= k; }
}

// ---------- LegendView driver (kill screens, emotes, previews) ----------
// view: a LegendView from DT.Legends.createView. Call update(seconds) after view.update(dt) each frame.
export function attachView(view, id) {
  let dancer = null;
  const ensure = () => {
    if (!view.visual || !view.ready) return null;
    if (dancer && dancer.obj === view.visual) return dancer;
    const tmpl = view.t && view.t.root;
    dancer = new Dancer(view.visual, tmpl || null).set(id);
    return dancer;
  };
  return {
    get dancer() { return ensure(); },
    get bpm() { return danceOf(id).bpm; },
    update(t, weight = 1) {
      const d = ensure();
      if (!d) return null;
      const v = view.visual;
      const rt = d.apply(t, weight);
      applyRoot(v, rt, v.userData.s || v.scale.x);
      return rt;
    },
    dispose() { if (dancer && !view.mixer) dancer.clear(); dancer = null; },
  };
}

// A battle unit model dancing, for the kill screen. key: 'lineId:stage'.
export async function unitRig(key, id, opts = {}) {
  const t = await loadTemplate(key);
  if (!t) return null;
  const obj = SkeletonUtils.clone(t.root);
  const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[key];
  const yaw = src && src.rotY ? src.rotY : 0;
  const scale = Math.min(opts.height || 1, 1.5 / Math.max(t.footprint || 1, 0.01));
  const rig = objRig(obj, id, { templateRoot: t.root, clip: t.clip, yaw, scale });
  rig.key = key;
  return rig;
}

// Any object dancing: a template clone (bones move) or a static model / placeholder (no bones: procedural root
// motion only, stretched by the Dancer's limbless mode). opts: { templateRoot, clip, yaw, scale, dispose }.
export function objRig(obj, id, opts = {}) {
  const yaw = opts.yaw || 0;
  const scale = opts.scale || 1;
  let mixer = null;
  if (opts.clip) { mixer = new THREE.AnimationMixer(obj); mixer.clipAction(opts.clip).play(); }
  const dancer = new Dancer(obj, opts.templateRoot || null).set(id);
  const group = new THREE.Group();
  group.add(obj);
  let time = 0;
  return {
    group, dancer, obj, procedural: dancer.E.length === 0,
    update(dt, at) {
      time = at != null ? at : time + dt;
      if (mixer) mixer.update(dt);
      obj.position.set(0, 0, 0);
      obj.quaternion.setFromAxisAngle(_v.set(0, 1, 0), yaw);
      obj.scale.setScalar(scale);
      applyRoot(obj, dancer.apply(time, Math.min(1, time / 0.25)), scale);
    },
    dispose() { if (mixer) { mixer.stopAllAction(); mixer.uncacheRoot(obj); } if (opts.dispose) opts.dispose(); },
  };
}

// ---------- board legends: dance emotes ----------
const live = new Map(); // view -> { drv, t0, dur, playerId }
const game = () => (DT.UI && DT.UI.game ? DT.UI.game() : null);
const rend = () => (DT.UI && DT.UI.renderer ? DT.UI.renderer() : null);
function viewOf(playerId) {
  const g = game(), r = rend();
  if (!g || !r) return null;
  if (g.human && g.human.id === playerId) return r.legend ? r.legend.view : null;
  if (r.oppLegend && r.oppLegend.playerId === playerId && r.oppLegend.view.group.visible) return r.oppLegend.view;
  return null;
}
function lookDance(playerId) {
  const g = game();
  const p = g && g.players.find((q) => q.id === playerId);
  const look = p && DT.Social.lookOf(p);
  return (look && look.dance && C.get('dance', look.dance) ? look.dance : null) || C.equipped('dance');
}
function playFor(playerId, id) {
  const v = viewOf(playerId);
  if (!v) return false;
  const dance = danceOf(id || lookDance(playerId));
  for (const [view, o] of live) if (o.playerId === playerId) live.delete(view);
  // at least 8 beats, about four seconds
  const dur = Math.max(3.6, (8 * 60) / dance.bpm);
  live.set(v, { drv: attachView(v, dance.id), t0: performance.now(), dur, playerId, dance: dance.id });
  return true;
}
function stopFor(playerId) { for (const [view, o] of live) if (o.playerId === playerId) live.delete(view); }

function hookRenderer(r) {
  if (!r || r.__danceHook) return;
  r.__danceHook = true;
  const orig = r.render.bind(r);
  r.render = function () {
    if (!live.size) return orig();
    const now = performance.now();
    const undo = [];
    for (const [view, o] of [...live]) {
      const t = (now - o.t0) / 1000;
      if (t >= o.dur || view.disposed || view.moving) { live.delete(view); continue; }
      const v = view.visual;
      if (!v) continue;
      undo.push([v, v.position.clone(), v.quaternion.clone(), v.scale.clone()]);
      // fade in and out over a quarter second
      o.drv.update(t, Math.min(1, t / 0.25, (o.dur - t) / 0.3));
    }
    try { orig(); } finally {
      for (const [v, p, q, s] of undo) { v.position.copy(p); v.quaternion.copy(q); v.scale.copy(s); }
    }
  };
}
(function loop() { hookRenderer(rend()); requestAnimationFrame(loop); })();

// ---------- previews: one small offscreen renderer drawn into 2D canvases ----------
let P = null;
function pctx(size) {
  if (!P) {
    const canvas = document.createElement('canvas');
    let r;
    try { r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true, preserveDrawingBuffer: true }); } catch (e) { return null; }
    r.setPixelRatio(1);
    r.outputColorSpace = THREE.SRGBColorSpace;
    r.setClearColor(0x000000, 0);
    const scene = new THREE.Scene();
    scene.add(new THREE.HemisphereLight('#ffffff', '#3a4660', 2.2));
    const key = new THREE.DirectionalLight('#fff6e8', 2.4);
    key.position.set(1.5, 3, 4);
    scene.add(key);
    P = { canvas, r, scene, cam: new THREE.PerspectiveCamera(30, 1, 0.05, 50), size: 0, rigs: new Map() };
  }
  if (P.size !== size) { P.r.setSize(size, size, false); P.size = size; }
  return P;
}
const legendKey = () => {
  const lo = C.loadout();
  const l = DT.LegendEvo && DT.LegendEvo.look ? DT.LegendEvo.look(lo) : lo;
  return (l && l.legend) || C.equipped('legend');
};
// cached rigs per legend key (or 'unit:<key>'): { view | obj, drv, last }
function previewRig(o) {
  const key = o.unit ? `unit:${o.unit}` : o.legend || legendKey();
  let rig = P.rigs.get(key);
  if (!rig) {
    rig = { key, ready: false, last: 0 };
    P.rigs.set(key, rig);
    if (o.unit) unitRig(o.unit, o.dance).then((u) => { rig.unit = u; rig.ready = !!u; });
    else if (DT.Legends && DT.Legends.createView) {
      rig.view = DT.Legends.createView(key, { ring: false, yaw: 0, height: 1 });
      rig.drv = null;
    }
  }
  if (rig.view && !rig.ready && rig.view.ready && rig.view.t) rig.ready = true;
  return rig;
}

function drawInto(ctx, x, y, w, h, o = {}) {
  const size = o.size || 256;
  const p = pctx(size);
  if (!p) return false;
  const rig = previewRig(o);
  if (!rig.ready) return false;
  const id = danceOf(o.dance || C.equipped('dance')).id;
  const t = o.t || 0;
  const now = performance.now();
  const dt = rig.last ? Math.min(0.1, (now - rig.last) / 1000) : 0;
  rig.last = now;
  let obj;
  if (rig.unit) {
    if (rig.unit.dancer.dance.id !== id) rig.unit.dancer.set(id);
    rig.unit.update(dt, t);
    obj = rig.unit.group;
  } else {
    const v = rig.view;
    if (!rig.drv || rig.dance !== id) { rig.drv = attachView(v, id); rig.dance = id; }
    v.update(dt);
    rig.drv.update(t);
    obj = v.group;
  }
  p.scene.add(obj);
  const yaw = o.yaw != null ? o.yaw : 0.35;
  const d = o.dist || 3.3;
  p.cam.position.set(Math.sin(yaw) * d, 0.75, Math.cos(yaw) * d);
  p.cam.lookAt(0, 0.55, 0);
  p.r.render(p.scene, p.cam);
  p.scene.remove(obj);
  const s = Math.min(w, h);
  ctx.drawImage(p.canvas, x + (w - s) / 2, y + (h - s) / 2, s, s);
  return true;
}

// Store / Locker card: the dance looping on your Little Legend.
function preview(el, id) {
  const cv = document.createElement('canvas');
  cv.className = 'dancePreview';
  cv.style.cssText = 'width:100%;height:100%;display:block;border-radius:inherit';
  el.appendChild(cv);
  const c = cv.getContext('2d');
  const t0 = performance.now();
  let stop = false, last = 0, seen = false;
  const tick = (now) => {
    if (stop) return;
    if (!cv.isConnected) { if (seen || now - t0 > 5000) return; requestAnimationFrame(tick); return; }
    seen = true;
    if (now - last > 33) { // about 30 frames a second is plenty for a card
      last = now;
      const w = Math.max(64, el.clientWidth || 160), h = Math.max(64, el.clientHeight || 112);
      if (cv.width !== w || cv.height !== h) { cv.width = w; cv.height = h; }
      c.clearRect(0, 0, w, h);
      const ok = drawInto(c, 0, 0, w, h, { dance: id, t: (now - t0) / 1000, size: 192 });
      if (!ok) {
        const d = DANCES.find((x) => x.id === id);
        c.font = `${Math.round(h * 0.4)}px sans-serif`;
        c.textAlign = 'center';
        c.textBaseline = 'middle';
        c.fillText((d && d.icon) || '♪', w / 2, h / 2);
      }
      cv.dataset.drawn = ok ? '1' : '0';
    }
    requestAnimationFrame(tick);
  };
  requestAnimationFrame(tick);
  return () => { stop = true; };
}

// ---------- coverage report (tests) ----------
async function coverage(keys) {
  const out = [];
  for (const key of keys) {
    let root = null;
    if (key.startsWith('unit:')) { const t = await loadTemplate(key.slice(5)); root = t && t.root; }
    else { const t = DT.Legends && (await DT.Legends.modelFor(key)); root = t && t.root; }
    if (!root) { out.push({ key, missing: true }); continue; }
    const r = findRoles(root);
    out.push({ key, ...r, arms: !!(r.armL && r.armR), legs: !!(r.legL && r.legR), hasHead: !!r.head, core: !!r.hips && (!!r.head || r.spine.length > 0 || !!r.hips) });
  }
  return out;
}

DT.Dances = {
  list: () => DANCES.map((d) => ({ id: d.id, name: d.name, bpm: d.bpm, rarity: d.rarity, free: !!d.free })),
  get: (id) => DANCES.find((d) => d.id === id) || null,
  bpm: (id) => danceOf(id).bpm,
  findRoles,
  parseName,
  Dancer,
  attachView,
  unitRig,
  objRig,
  playFor,
  stopFor,
  drawInto,
  preview,
  coverage,
  limits: LIM,
  get playing() { return [...live.values()].map((o) => ({ playerId: o.playerId, dance: o.dance })); },
};
