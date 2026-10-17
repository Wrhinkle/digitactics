// Boards ('board' cosmetics): arena themes. A theme sets the sky, fog, lights, island, slabs, tile colors and the
// animated scenery around the rim. renderer.setBoard(id) swaps themes at runtime and disposes the old one.
// Tiles, bench slots and item pedestals stay the same objects (render3d.js); only their materials change
// (color, finish, and an optional small texture from spec.surface).
// More themes live in js/boards-extra.js. Themes can react to the game (combat start, wins, stage, digivolves)
// through ctx.on / ctx.state; that is visual only and never touches game state.
// Which board shows: yours in planning and PvE; in PvP combat the home board (yours when you defend,
// your opponent's when you visit, i.e. renderer.mirror).
import * as THREE from 'three';
import { mergeGeometries } from 'three/addons/utils/BufferGeometryUtils.js';
import { LAYOUT } from './render3d.js';
import { extraThemes } from './boards-extra.js';
import { mysteryBoards } from './mystery-boards.js';

const DT = window.DT;
const { cellWorld, benchWorld, itemWorld, BENCH_Z, BENCH_STEP, ITEM_SLOTS } = LAYOUT;
const DEFAULT = 'file-island';
const FADE = 0.18; // seconds each way

// ---------- helpers ----------
const C = (c) => new THREE.Color(c);
const std = (o) => new THREE.MeshStandardMaterial({ roughness: 0.85, ...o });
const dummy = new THREE.Object3D();

function canvasTex(w, h, draw, repeat = 1) {
  const cv = document.createElement('canvas');
  cv.width = w;
  cv.height = h;
  draw(cv.getContext('2d'), w, h);
  const t = new THREE.CanvasTexture(cv);
  t.colorSpace = THREE.SRGBColorSpace;
  t.wrapS = t.wrapT = THREE.RepeatWrapping;
  t.repeat.set(repeat, repeat);
  t.anisotropy = 4;
  return t;
}

// vertical gradient, top color first
const gradient = (stops) => canvasTex(2, 256, (g, w, h) => {
  const gr = g.createLinearGradient(0, 0, 0, h);
  stops.forEach((c, i) => gr.addColorStop(i / (stops.length - 1), c));
  g.fillStyle = gr;
  g.fillRect(0, 0, w, h);
});

// base color with random dots: sand, grass, snow, stone
function speckle(base, dots, { n = 1400, size = 256, r = [0.6, 2.2], seed = 1, repeat = 3, alpha = [0.15, 0.55] } = {}) {
  const rng = DT.makeRng(seed);
  return canvasTex(size, size, (g, w, h) => {
    g.fillStyle = base;
    g.fillRect(0, 0, w, h);
    for (let i = 0; i < n; i++) {
      g.globalAlpha = alpha[0] + rng() * (alpha[1] - alpha[0]);
      g.fillStyle = dots[(rng() * dots.length) | 0];
      g.beginPath();
      g.arc(rng() * w, rng() * h, r[0] + rng() * (r[1] - r[0]), 0, Math.PI * 2);
      g.fill();
    }
  }, repeat);
}

const softDot = () => canvasTex(32, 32, (g) => {
  const gr = g.createRadialGradient(16, 16, 0, 16, 16, 16);
  gr.addColorStop(0, 'rgba(255,255,255,1)');
  gr.addColorStop(0.4, 'rgba(255,255,255,0.6)');
  gr.addColorStop(1, 'rgba(255,255,255,0)');
  g.fillStyle = gr;
  g.fillRect(0, 0, 32, 32);
});

// a geometry moved into place and painted one color (vertexColors), ready to merge
function part(geo, color, o = {}) {
  const g = geo.index ? geo.toNonIndexed() : geo;
  if (g !== geo) geo.dispose();
  dummy.position.set(o.x || 0, o.y || 0, o.z || 0);
  dummy.rotation.set(o.rx || 0, o.ry || 0, o.rz || 0);
  dummy.scale.set(o.sx ?? o.s ?? 1, o.sy ?? o.s ?? 1, o.sz ?? o.s ?? 1);
  dummy.updateMatrix();
  g.applyMatrix4(dummy.matrix);
  const c = C(color);
  const n = g.attributes.position.count;
  const arr = new Float32Array(n * 3);
  for (let i = 0; i < n; i++) { arr[i * 3] = c.r; arr[i * 3 + 1] = c.g; arr[i * 3 + 2] = c.b; }
  g.setAttribute('color', new THREE.BufferAttribute(arr, 3));
  return g;
}
function merge(parts) {
  const g = mergeGeometries(parts);
  for (const p of parts) p.dispose();
  return g;
}

function setInst(m, i, it) {
  dummy.position.set(it.x || 0, it.y || 0, it.z || 0);
  dummy.rotation.set(it.rx || 0, it.ry || 0, it.rz || 0);
  dummy.scale.set(it.sx ?? it.s ?? 1, it.sy ?? it.s ?? 1, it.sz ?? it.s ?? 1);
  dummy.updateMatrix();
  m.setMatrixAt(i, dummy.matrix);
}
// one InstancedMesh for many copies; items: {x,y,z,rx,ry,rz,s|sx,sy,sz,color?}
function inst(ctx, geo, mat, items, { shadow = true, moving = false } = {}) {
  const m = new THREE.InstancedMesh(geo, mat, Math.max(1, items.length));
  m.count = items.length;
  items.forEach((it, i) => { setInst(m, i, it); if (it.color) m.setColorAt(i, C(it.color)); });
  m.castShadow = shadow;
  m.receiveShadow = shadow;
  if (moving) m.frustumCulled = false;
  ctx.g.add(m);
  return m;
}

function mesh(ctx, geo, mat, o = {}) {
  const m = new THREE.Mesh(geo, mat);
  m.position.set(o.x || 0, o.y || 0, o.z || 0);
  m.rotation.set(o.rx || 0, o.ry || 0, o.rz || 0);
  if (o.s) m.scale.setScalar(o.s);
  m.castShadow = !!o.shadow;
  m.receiveShadow = !!o.receive;
  ctx.g.add(m);
  return m;
}

// spots around the rim, keeping the board, bench and item bench clear (same rule as the original arena).
// back: also skip the front of the island, where tall or floating props would cover the bench
function rimSpots(rng, n, rMin = 9.6, rMax = 12.2, back = false) {
  const out = [];
  for (let i = 0; i < n; i++) {
    const a = rng() * Math.PI * 2;
    const rad = rMin + rng() * (rMax - rMin);
    const x = Math.cos(a) * rad, z = Math.sin(a) * rad;
    if (Math.abs(x) < 9.6 && z > -7 && z < BENCH_Z + 1.8) continue;
    if (back && z > 2.5 && Math.abs(x) < 11.5) continue;
    out.push({ x, z, a });
  }
  return out;
}
// spots on a ring farther out (sea, plains); the front is off screen so it is skipped
function farSpots(rng, n, rMin, rMax) {
  const out = [];
  for (let i = 0; i < n; i++) {
    const a = rng() * Math.PI * 2;
    const rad = rMin + rng() * (rMax - rMin);
    const x = Math.cos(a) * rad, z = Math.sin(a) * rad;
    if (z > 12) continue;
    out.push({ x, z, a });
  }
  return out;
}

// low-poly water: a grid whose heights move every frame (flat shading, so no normals to recompute)
function sea(ctx, { color, amp = 0.12, y = -1.1, rough = 0.3, metal = 0.25, size = 150, seg = 50, speed = 1, opacity = 1 }) {
  const geo = new THREE.PlaneGeometry(size, size, seg, seg);
  geo.rotateX(-Math.PI / 2);
  const mat = std({ color, roughness: rough, metalness: metal, flatShading: true, transparent: opacity < 1, opacity });
  const m = mesh(ctx, geo, mat, { y, receive: true });
  const pos = geo.attributes.position;
  const base = Float32Array.from(pos.array);
  ctx.tick((dt, t) => {
    const s = t * speed;
    for (let i = 0; i < pos.count; i++) {
      const x = base[i * 3], z = base[i * 3 + 2];
      pos.array[i * 3 + 1] = amp * (Math.sin(x * 0.33 + s * 1.1) + 0.8 * Math.sin(z * 0.27 - s * 0.9) + 0.35 * Math.sin((x + z) * 0.7 + s * 1.9));
    }
    pos.needsUpdate = true;
  });
  return m;
}

// flat ground plane beyond the island
const plain = (ctx, mat, y = -1.1) => mesh(ctx, new THREE.CircleGeometry(80, 48).rotateX(-Math.PI / 2), mat, { y, receive: true });

// drifting points inside a box, wrapping around; v = [vx, vy, vz] in units per second
function particles(ctx, n, { box = [-16, 16, 0, 12, -15, 12], color = '#ffffff', size = 0.15, v = [0, -1, 0], wobble = 0.3, opacity = 0.9, additive = false, fog = true }) {
  const rng = DT.makeRng(n * 13 + 5);
  const [x0, x1, y0, y1, z0, z1] = box;
  const pos = new Float32Array(n * 3);
  const spd = new Float32Array(n);
  for (let i = 0; i < n; i++) {
    pos[i * 3] = x0 + rng() * (x1 - x0);
    pos[i * 3 + 1] = y0 + rng() * (y1 - y0);
    pos[i * 3 + 2] = z0 + rng() * (z1 - z0);
    spd[i] = 0.6 + rng() * 0.8;
  }
  const geo = new THREE.BufferGeometry();
  geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
  const tex = softDot();
  const mat = new THREE.PointsMaterial({ color, size, map: tex, transparent: true, opacity, depthWrite: false, fog,
    blending: additive ? THREE.AdditiveBlending : THREE.NormalBlending });
  const pts = new THREE.Points(geo, mat);
  pts.frustumCulled = false;
  ctx.g.add(pts);
  const wrap = (val, a, b) => (val < a ? val + (b - a) : val > b ? val - (b - a) : val);
  ctx.tick((dt, t) => {
    for (let i = 0; i < n; i++) {
      const k = spd[i];
      pos[i * 3] = wrap(pos[i * 3] + (v[0] * k + Math.sin(t * 1.3 + i) * wobble) * dt, x0, x1);
      pos[i * 3 + 1] = wrap(pos[i * 3 + 1] + v[1] * k * dt, y0, y1);
      pos[i * 3 + 2] = wrap(pos[i * 3 + 2] + (v[2] * k + Math.cos(t * 1.1 + i * 0.7) * wobble) * dt, z0, z1);
    }
    geo.attributes.position.needsUpdate = true;
  });
  return pts;
}

// ---------- reusable props ----------
function palmGeo() {
  const curve = new THREE.CatmullRomCurve3([new THREE.Vector3(0, 0, 0), new THREE.Vector3(0.25, 1.2, 0), new THREE.Vector3(0.7, 2.4, 0), new THREE.Vector3(1.2, 3.3, 0)]);
  const parts = [part(new THREE.TubeGeometry(curve, 10, 0.15, 7), '#8a6440')];
  for (let i = 0; i < 4; i++) {
    const p = curve.getPoint(0.15 + i * 0.22);
    parts.push(part(new THREE.TorusGeometry(0.16, 0.035, 4, 10), '#6e4d30', { x: p.x, y: p.y, z: p.z, rx: Math.PI / 2 }));
  }
  const top = curve.getPoint(1);
  for (let i = 0; i < 7; i++) {
    const a = (i / 7) * Math.PI * 2;
    const leaf = new THREE.ConeGeometry(0.32, 2.1, 4);
    leaf.translate(0, 1.05, 0);
    leaf.scale(1, 1, 0.25);
    const g = part(leaf, i % 2 ? '#3f9a3a' : '#2f7d33', { x: top.x, y: top.y, z: top.z, ry: a, rz: -1.95 });
    parts.push(g);
  }
  for (let i = 0; i < 3; i++) parts.push(part(new THREE.SphereGeometry(0.15, 8, 6), '#5a3d22', { x: top.x + Math.cos(i * 2.1) * 0.18, y: top.y - 0.15, z: top.z + Math.sin(i * 2.1) * 0.18 }));
  return merge(parts);
}

function pineGeo(snow) {
  const parts = [part(new THREE.CylinderGeometry(0.1, 0.15, 0.7, 6), '#5b3d24', { y: 0.35 })];
  [[0.95, 1.1, 0.9], [0.75, 1.0, 1.5], [0.5, 0.9, 2.05]].forEach(([r, h, y]) => {
    parts.push(part(new THREE.ConeGeometry(r, h, 8), '#2c5a3e', { y }));
    if (snow) parts.push(part(new THREE.ConeGeometry(r * 0.62, h * 0.45, 8), '#f2f6ff', { y: y + h * 0.3 }));
  });
  return merge(parts);
}

function gearGeo(r, teeth, depth, hole = 0.35) {
  const s = new THREE.Shape();
  const ri = r * 0.82;
  for (let i = 0; i < teeth; i++) {
    const a0 = (i / teeth) * Math.PI * 2, step = (Math.PI * 2) / teeth;
    const pts = [[ri, a0], [r, a0 + step * 0.15], [r, a0 + step * 0.45], [ri, a0 + step * 0.6]];
    pts.forEach(([rr, a], k) => { const x = Math.cos(a) * rr, y = Math.sin(a) * rr; if (i === 0 && k === 0) s.moveTo(x, y); else s.lineTo(x, y); });
  }
  s.closePath();
  const h = new THREE.Path();
  h.absarc(0, 0, r * hole, 0, Math.PI * 2, true);
  s.holes.push(h);
  for (let i = 0; i < 5; i++) {
    const a = (i / 5) * Math.PI * 2;
    const w = new THREE.Path();
    w.absarc(Math.cos(a) * r * 0.6, Math.sin(a) * r * 0.6, r * 0.12, 0, Math.PI * 2, true);
    s.holes.push(w);
  }
  const g = new THREE.ExtrudeGeometry(s, { depth, bevelEnabled: false, curveSegments: 6 });
  g.translate(0, 0, -depth / 2);
  return g;
}

// ---------- themes ----------
// sky: gradient stops (top first) or a color; fog: [color, near, far]; hemi: [sky, ground, intensity]; sun: [color, intensity]
// tiles: mine / enemy (combat) / enemyPlan (planning), optional glowMine / glowEnemy emissive
// props(ctx): ground, island and scenery; ctx = { g, rng, tick(fn), spec }
const island = (ctx, top, side = '#6b5a44', cliff = side) => {
  const topMat = top.isTexture ? std({ map: top, roughness: 0.95 }) : std({ color: top, roughness: 0.9 });
  const m = mesh(ctx, new THREE.CylinderGeometry(12.5, 13.6, 1.4, 56), [std({ color: side, roughness: 1 }), topMat, topMat], { y: -0.72, receive: true });
  mesh(ctx, new THREE.CylinderGeometry(13.62, 14.2, 0.9, 56), std({ color: cliff, roughness: 1 }), { y: -1.4 });
  return m;
};

const THEMES = [
  {
    id: 'file-island', name: 'File Island', rarity: 'common', price: 0, free: true, icon: '🌲',
    desc: 'Grass island in a dark sea, with pines, rocks and data crystals. The default board.',
    sky: '#0b1a2c', fog: ['#0b1a2c', 30, 60], hemi: ['#bfe3ff', '#2b3a22', 1.4], sun: ['#fff4e0', 2.6],
    slab: '#2a2f3a', benchSlab: '#3a3226', slot: '#5a4b35', ped: '#39404f',
    tiles: { mine: '#30598c', enemy: '#7a3442', enemyPlan: '#4a2a33' },
    props(ctx) {
      sea(ctx, { color: '#1d5a8a', amp: 0.07, rough: 0.25, metal: 0.3, speed: 0.7 });
      island(ctx, '#4f8a4a');
      // same layout as the original arena (seed 7)
      const rng = DT.makeRng(7);
      const trees = [], rocks = [], crystals = [];
      for (let i = 0; i < 46; i++) {
        const a = rng() * Math.PI * 2;
        const rad = 9.6 + rng() * 2.6;
        const x = Math.cos(a) * rad, z = Math.sin(a) * rad;
        if (Math.abs(x) < 9.6 && z > -7 && z < BENCH_Z + 1.8) continue;
        const kind = rng();
        if (kind < 0.5) trees.push({ x, z, s: 0.8 + rng() * 0.7 });
        else if (kind < 0.82) rocks.push({ x, y: 0.1, z, sx: 0.6 + rng(), sy: 0.5 + rng() * 0.6, sz: 0.6 + rng(), ry: rng() * 3 });
        else crystals.push({ x, y: 1.1, z, sx: 0.7, sy: 1.6 + rng(), sz: 0.7, phase: rng() * 6 });
      }
      const tree = merge([part(new THREE.CylinderGeometry(0.12, 0.18, 0.8, 8), '#5b3d24', { y: 0.4 }), part(new THREE.ConeGeometry(0.75, 1.8, 9), '#2f6b3a', { y: 1.6 })]);
      inst(ctx, tree, std({ vertexColors: true, roughness: 0.9 }), trees);
      inst(ctx, new THREE.DodecahedronGeometry(0.6, 0), std({ color: '#7d8590', roughness: 1, flatShading: true }), rocks);
      const cm = inst(ctx, new THREE.OctahedronGeometry(0.45, 0), std({ color: '#7ff3ff', emissive: '#1ac8ff', emissiveIntensity: 0.9, roughness: 0.2 }), crystals, { shadow: false, moving: true });
      ctx.tick((dt, t) => {
        crystals.forEach((c, i) => { c.ry = t * 0.6 + c.phase; c.y = 1.1 + Math.sin(t * 1.5 + c.phase) * 0.15; setInst(cm, i, c); });
        cm.instanceMatrix.needsUpdate = true;
      });
    },
  },
  {
    id: 'file-beach', name: 'File Island Beach', rarity: 'common', price: 150, icon: '🏝️',
    desc: 'Sandy island in a turquoise sea. Palms sway, the surf rolls in, shells on the sand.',
    sky: ['#55b8f0', '#bfe9ff', '#eafaff'], fog: ['#a8e0f5', 34, 72], hemi: ['#e6f6ff', '#c9b27a', 1.5], sun: ['#fff1d6', 2.8],
    slab: '#7a5a3c', benchSlab: '#6a4c32', slot: '#a07c52', ped: '#efe6d4',
    tiles: { mine: '#2a7fb8', enemy: '#c4503c', enemyPlan: '#7c3a2e' },
    props(ctx) {
      sea(ctx, { color: '#22aebc', amp: 0.16, rough: 0.2, metal: 0.15 });
      island(ctx, speckle('#e6cf96', ['#f5e6bd', '#c9ad70', '#ffffff'], { seed: 3 }), '#cfb47a', '#b89a62');
      // surf ring
      const foam = mesh(ctx, new THREE.RingGeometry(13.4, 15.2, 64).rotateX(-Math.PI / 2), new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.45, depthWrite: false }), { y: -0.98 });
      ctx.tick((dt, t) => { const k = 0.5 + 0.5 * Math.sin(t * 0.9); foam.scale.setScalar(0.97 + k * 0.05); foam.material.opacity = 0.2 + 0.35 * (1 - k); });
      const palms = rimSpots(ctx.rng, 60, 10, 12, true).filter((_, i) => i % 2 === 0).slice(0, 11)
        .map((p) => ({ x: p.x, z: p.z, ry: -p.a + (ctx.rng() - 0.5) * 0.8, s: 0.85 + ctx.rng() * 0.4, phase: ctx.rng() * 6 }));
      const pm = inst(ctx, palmGeo(), std({ vertexColors: true, roughness: 0.85, side: THREE.DoubleSide }), palms, { moving: true });
      ctx.tick((dt, t) => {
        palms.forEach((p, i) => { p.rz = Math.sin(t * 0.9 + p.phase) * 0.035; p.rx = Math.cos(t * 0.7 + p.phase) * 0.025; setInst(pm, i, p); });
        pm.instanceMatrix.needsUpdate = true;
      });
      const shellCols = ['#f7c6c0', '#fff4e6', '#f2b48a', '#e8a0b4'];
      const shells = rimSpots(ctx.rng, 120, 9.7, 12.3).map((p, i) => ({ x: p.x, y: 0.02, z: p.z, ry: ctx.rng() * 6, s: 0.6 + ctx.rng() * 0.6, color: shellCols[i % 4] }));
      const shell = new THREE.SphereGeometry(0.22, 10, 6, 0, Math.PI * 2, 0, Math.PI / 2);
      shell.scale(1, 0.55, 1.25);
      inst(ctx, shell, std({ color: '#ffffff', roughness: 0.5 }), shells, { shadow: false });
      const rocks = farSpots(ctx.rng, 14, 14.5, 22).map((p) => ({ x: p.x, y: -1.1, z: p.z, s: 0.8 + ctx.rng() * 1.4, ry: ctx.rng() * 3 }));
      inst(ctx, new THREE.DodecahedronGeometry(0.8, 0), std({ color: '#8d8a80', flatShading: true }), rocks);
    },
  },
  {
    id: 'server-desert', name: 'Server Desert', rarity: 'common', price: 150, icon: '🏜️',
    desc: 'Dunes under a hot sky. Cacti, sandstone and drifting sand, with heat haze on the horizon.',
    sky: ['#6fa8dc', '#f6d7a0', '#f3b878'], fog: ['#efcf9c', 30, 66], hemi: ['#fff0d0', '#a8743a', 1.4], sun: ['#ffe2b0', 3.0],
    slab: '#7e5230', benchSlab: '#6c4528', slot: '#a8723f', ped: '#c79a5e',
    tiles: { mine: '#3a74a6', enemy: '#b3442c', enemyPlan: '#6e3424' },
    props(ctx) {
      const sand = speckle('#d9a55e', ['#e9bf7c', '#b8843e', '#f3d39a'], { seed: 11, repeat: 10 });
      plain(ctx, std({ map: sand, roughness: 1 }));
      island(ctx, speckle('#e2b56c', ['#f0c98a', '#bf8a44'], { seed: 12 }), '#b98146', '#a06c38');
      const duneMat = std({ color: '#d79f58', roughness: 1 });
      const dunes = farSpots(ctx.rng, 16, 17, 34).map((p) => ({ x: p.x, y: -1.2, z: p.z, sx: 5 + ctx.rng() * 5, sy: 1.2 + ctx.rng() * 1.4, sz: 3 + ctx.rng() * 3, ry: p.a }));
      inst(ctx, new THREE.SphereGeometry(1, 20, 10, 0, Math.PI * 2, 0, Math.PI / 2), duneMat, dunes, { shadow: false });
      const cactus = merge([
        part(new THREE.CylinderGeometry(0.22, 0.26, 2.2, 8), '#4f8a3c', { y: 1.1 }),
        part(new THREE.SphereGeometry(0.22, 8, 6), '#4f8a3c', { y: 2.2 }),
        part(new THREE.CylinderGeometry(0.14, 0.14, 0.6, 7), '#4f8a3c', { x: 0.4, y: 1.0, rz: Math.PI / 2 }),
        part(new THREE.CylinderGeometry(0.14, 0.14, 0.7, 7), '#4f8a3c', { x: 0.62, y: 1.33 }),
        part(new THREE.CylinderGeometry(0.12, 0.12, 0.45, 7), '#4f8a3c', { x: -0.33, y: 1.4, rz: Math.PI / 2 }),
        part(new THREE.CylinderGeometry(0.12, 0.12, 0.55, 7), '#4f8a3c', { x: -0.52, y: 1.66 }),
        part(new THREE.SphereGeometry(0.1, 6, 4), '#f07aa0', { y: 2.4 }),
      ]);
      const spots = rimSpots(ctx.rng, 60, 9.8, 12.2);
      inst(ctx, cactus, std({ vertexColors: true, roughness: 0.8 }), spots.slice(0, 12).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.7 + ctx.rng() * 0.5 })));
      inst(ctx, new THREE.DodecahedronGeometry(0.6, 0), std({ color: '#c47a46', flatShading: true }),
        spots.slice(12, 30).map((p) => ({ x: p.x, y: 0.15, z: p.z, sx: 0.7 + ctx.rng() * 1.2, sy: 0.4 + ctx.rng() * 0.9, sz: 0.7 + ctx.rng(), ry: ctx.rng() * 3 })));
      // heat haze: a transparent band of wavy stripes around the horizon, scrolling
      const haze = canvasTex(256, 64, (g, w, h) => {
        g.clearRect(0, 0, w, h);
        for (let y = 0; y < h; y += 6) {
          g.strokeStyle = `rgba(255,240,210,${0.25 + 0.25 * Math.sin(y)})`;
          g.lineWidth = 2;
          g.beginPath();
          for (let x = 0; x <= w; x += 8) g.lineTo(x, y + Math.sin(x * 0.05 + y) * 2);
          g.stroke();
        }
      });
      haze.repeat.set(8, 1);
      const band = mesh(ctx, new THREE.CylinderGeometry(26, 26, 3.5, 48, 1, true), new THREE.MeshBasicMaterial({ map: haze, transparent: true, opacity: 0.35, depthWrite: false, side: THREE.DoubleSide, fog: false }), { y: 0.4 });
      ctx.tick((dt, t) => { haze.offset.x = t * 0.01; haze.offset.y = Math.sin(t * 2.3) * 0.04; band.material.opacity = 0.28 + 0.08 * Math.sin(t * 3.1); });
      particles(ctx, 160, { color: '#f4d9a6', size: 0.12, v: [1.6, 0, 0.3], wobble: 0.2, opacity: 0.6, box: [-18, 18, 0, 3, -15, 11] });
    },
  },
  {
    id: 'primary-village', name: 'Primary Village', rarity: 'rare', price: 400, icon: '🍼',
    desc: 'Soft pastel meadow with cribs full of Digi-Eggs and toy blocks floating in the air.',
    sky: ['#ffc9e6', '#e8dcff', '#cdeaff'], fog: ['#efe2f6', 34, 72], hemi: ['#fff4fb', '#b9d8a8', 1.7], sun: ['#fff6ea', 2.3],
    slab: '#e9d6bd', benchSlab: '#dcbf9c', slot: '#f3d2ad', ped: '#efe6ff',
    tiles: { mine: '#5f9fe6', enemy: '#e8728b', enemyPlan: '#b06476' },
    props(ctx) {
      sea(ctx, { color: '#a4d6f0', amp: 0.05, rough: 0.4, metal: 0.05, speed: 0.6 });
      island(ctx, speckle('#b9e3a8', ['#ffb3cf', '#fff3a6', '#ffffff', '#a4d48f'], { seed: 21, n: 1100, r: [1, 2.6] }), '#f2c6a0', '#e8b48a');
      // cribs: a merged frame of rails and bars, each with an egg that rocks gently
      const wood = '#fff2de';
      const crib = [part(new THREE.BoxGeometry(1.5, 0.12, 1.0), wood, { y: 0.35 }), part(new THREE.BoxGeometry(1.4, 0.08, 0.9), '#cfe6ff', { y: 0.44 })];
      for (const sx of [-0.72, 0.72]) for (const sz of [-0.47, 0.47]) crib.push(part(new THREE.CylinderGeometry(0.05, 0.05, 1.0, 6), wood, { x: sx, y: 0.5, z: sz }));
      for (const sz of [-0.47, 0.47]) {
        crib.push(part(new THREE.BoxGeometry(1.5, 0.06, 0.06), wood, { y: 0.92, z: sz }));
        for (let i = 0; i < 7; i++) crib.push(part(new THREE.CylinderGeometry(0.025, 0.025, 0.48, 4), wood, { x: -0.54 + i * 0.18, y: 0.68, z: sz }));
      }
      for (const sx of [-0.72, 0.72]) crib.push(part(new THREE.BoxGeometry(0.06, 0.06, 1.0), wood, { x: sx, y: 0.92 }));
      const spots = rimSpots(ctx.rng, 70, 9.9, 12);
      const cribs = spots.slice(0, 7).map((p) => ({ x: p.x, z: p.z, ry: -p.a + Math.PI / 2, s: 1.1 }));
      inst(ctx, merge(crib), std({ vertexColors: true, roughness: 0.6 }), cribs);
      const eggGeo = new THREE.SphereGeometry(0.28, 16, 12);
      eggGeo.scale(0.85, 1.15, 0.85);
      const eggCols = ['#ffd1e8', '#d2f0ff', '#fff1b8', '#d9ffd0', '#e8d6ff', '#ffe0c2', '#c9f1ee'];
      const eggs = cribs.map((c, i) => ({ x: c.x, y: 0.8, z: c.z, color: eggCols[i % eggCols.length], phase: i * 1.3 }));
      const em = inst(ctx, eggGeo, std({ color: '#ffffff', roughness: 0.45 }), eggs, { moving: true });
      ctx.tick((dt, t) => { eggs.forEach((e, i) => { e.rz = Math.sin(t * 2 + e.phase) * 0.18; setInst(em, i, e); }); em.instanceMatrix.needsUpdate = true; });
      // floating letter blocks
      const blockTex = canvasTex(64, 64, (g) => {
        g.fillStyle = '#ffffff'; g.fillRect(0, 0, 64, 64);
        g.strokeStyle = 'rgba(0,0,0,0.18)'; g.lineWidth = 6; g.strokeRect(5, 5, 54, 54);
        g.fillStyle = 'rgba(0,0,0,0.22)'; g.beginPath(); g.arc(32, 32, 11, 0, Math.PI * 2); g.fill();
      });
      const bCols = ['#ff9ec4', '#9fd4ff', '#ffe28a', '#b8f0a0', '#d0b4ff', '#ffc49a'];
      const blocks = rimSpots(ctx.rng, 90, 9.8, 12.5, true).slice(0, 16).map((p, i) => ({ x: p.x, y: 1.3 + ctx.rng() * 1.6, z: p.z, s: 0.45 + ctx.rng() * 0.35, color: bCols[i % bCols.length], phase: ctx.rng() * 6, base: 0 }));
      blocks.forEach((b) => { b.base = b.y; });
      const bm = inst(ctx, new THREE.BoxGeometry(1, 1, 1), std({ map: blockTex, roughness: 0.6 }), blocks, { moving: true });
      ctx.tick((dt, t) => {
        blocks.forEach((b, i) => { b.y = b.base + Math.sin(t * 1.2 + b.phase) * 0.3; b.ry = t * 0.4 + b.phase; b.rx = Math.sin(t * 0.5 + b.phase) * 0.3; setInst(bm, i, b); });
        bm.instanceMatrix.needsUpdate = true;
      });
      // puffy clouds far out
      const clouds = [];
      farSpots(ctx.rng, 10, 22, 34).forEach((p) => { for (let k = 0; k < 4; k++) clouds.push({ x: p.x + (k - 1.5) * 1.6, y: 3 + ctx.rng() * 1.5, z: p.z + ctx.rng(), s: 1.4 + ctx.rng() * 1.2 }); });
      inst(ctx, new THREE.SphereGeometry(1, 12, 8), std({ color: '#ffffff', roughness: 1, emissive: '#ffffff', emissiveIntensity: 0.25 }), clouds, { shadow: false });
    },
  },
  {
    id: 'toy-town', name: 'Toy Town', rarity: 'rare', price: 400, icon: '🧸',
    desc: 'Block houses, candy poles, spinning tops and a toy train circling the island.',
    sky: ['#7fd2ff', '#d8f2ff', '#fff4cf'], fog: ['#cfeeff', 34, 72], hemi: ['#ffffff', '#c8a07a', 1.6], sun: ['#fff8ec', 2.6],
    slab: '#2f4f7f', benchSlab: '#e76f51', slot: '#f9c74f', ped: '#90be6d',
    tiles: { mine: '#3a86ff', enemy: '#ff4d6d', enemyPlan: '#9b3550' },
    props(ctx) {
      const mat = canvasTex(64, 64, (g) => { g.fillStyle = '#f6d365'; g.fillRect(0, 0, 64, 64); g.fillStyle = '#f29e4c'; g.fillRect(0, 0, 32, 32); g.fillRect(32, 32, 32, 32); }, 40);
      plain(ctx, std({ map: mat, roughness: 0.9 }));
      island(ctx, speckle('#79c267', ['#8fd27a', '#66a856'], { seed: 31, n: 700 }), '#e85d75', '#c94860');
      const spots = rimSpots(ctx.rng, 80, 9.8, 11.6);
      const backSpots = rimSpots(ctx.rng, 60, 9.8, 11.6, true);
      const cols = ['#ff595e', '#ffca3a', '#8ac926', '#1982c4', '#6a4c93', '#ff924c'];
      // houses: body + roof; instance color tints the body, the roof stays red
      const house = merge([part(new THREE.BoxGeometry(1, 1, 1), '#ffffff', { y: 0.5 }), part(new THREE.ConeGeometry(0.85, 0.7, 4), '#ffd0d0', { y: 1.35, ry: Math.PI / 4 }), part(new THREE.BoxGeometry(0.3, 0.45, 0.02), '#7a5230', { y: 0.23, z: 0.51 })]);
      inst(ctx, house, std({ vertexColors: true, roughness: 0.5 }), backSpots.slice(0, 9).map((p, i) => ({ x: p.x, z: p.z, ry: -p.a - Math.PI / 2, s: 0.9 + ctx.rng() * 0.5, color: cols[i % cols.length] })));
      // stacked block towers
      const blocks = [];
      spots.slice(9, 16).forEach((p, i) => { const n = 2 + (i % 3); for (let k = 0; k < n; k++) blocks.push({ x: p.x + (ctx.rng() - 0.5) * 0.1, y: 0.3 + k * 0.6, z: p.z, s: 0.6, ry: ctx.rng() * 0.6, color: cols[(i + k) % cols.length] }); });
      inst(ctx, new THREE.BoxGeometry(1, 1, 1), std({ color: '#ffffff', roughness: 0.45 }), blocks);
      // candy poles
      const stripe = canvasTex(32, 64, (g) => { g.fillStyle = '#ffffff'; g.fillRect(0, 0, 32, 64); g.fillStyle = '#ff4d6d'; for (let y = -32; y < 64; y += 16) { g.beginPath(); g.moveTo(0, y); g.lineTo(32, y + 16); g.lineTo(32, y + 24); g.lineTo(0, y + 8); g.fill(); } });
      stripe.repeat.set(1, 3);
      const poleGeo = merge([part(new THREE.CylinderGeometry(0.1, 0.1, 2.4, 10), '#ffffff', { y: 1.2 }), part(new THREE.SphereGeometry(0.2, 10, 8), '#ffffff', { y: 2.45 })]);
      inst(ctx, poleGeo, std({ map: stripe, vertexColors: true, roughness: 0.4 }), spots.slice(16, 22).map((p) => ({ x: p.x, z: p.z })));
      // spinning tops
      const top = merge([part(new THREE.ConeGeometry(0.4, 0.5, 12), '#ffffff', { y: 0.25, rx: Math.PI }), part(new THREE.CylinderGeometry(0.4, 0.4, 0.12, 12), '#ffffff', { y: 0.56 }), part(new THREE.CylinderGeometry(0.05, 0.05, 0.35, 6), '#333333', { y: 0.78 })]);
      const tops = spots.slice(22, 27).map((p, i) => ({ x: p.x, z: p.z, cx: p.x, cz: p.z, color: cols[(i + 2) % cols.length], phase: i * 1.7 }));
      const tm = inst(ctx, top, std({ vertexColors: true, roughness: 0.35 }), tops, { moving: true });
      ctx.tick((dt, t) => {
        tops.forEach((o, i) => { o.ry = t * 9 + o.phase; o.rx = Math.sin(t * 1.5 + o.phase) * 0.12; o.x = o.cx + Math.sin(t * 0.6 + o.phase) * 0.25; o.z = o.cz + Math.cos(t * 0.6 + o.phase) * 0.25; setInst(tm, i, o); });
        tm.instanceMatrix.needsUpdate = true;
      });
      // toy train on a ring track at the island's edge
      const R = 12.05;
      mesh(ctx, new THREE.RingGeometry(R - 0.35, R + 0.35, 96).rotateX(-Math.PI / 2), std({ color: '#8d6e63', roughness: 0.9 }), { y: 0.005, receive: true });
      const car = merge([part(new THREE.BoxGeometry(0.9, 0.5, 0.55), '#ffffff', { y: 0.35 }), part(new THREE.CylinderGeometry(0.13, 0.13, 0.62, 10), '#333333', { x: -0.28, y: 0.13, rx: Math.PI / 2 }), part(new THREE.CylinderGeometry(0.13, 0.13, 0.62, 10), '#333333', { x: 0.28, y: 0.13, rx: Math.PI / 2 })]);
      const cars = [0, 1, 2, 3, 4].map((i) => ({ color: i === 0 ? '#d62828' : cols[i % cols.length], k: i }));
      const trm = inst(ctx, car, std({ vertexColors: true, roughness: 0.4 }), cars, { moving: true });
      ctx.tick((dt, t) => {
        cars.forEach((c, i) => { const a = t * 0.12 - c.k * 0.085; c.x = Math.cos(a) * R; c.z = Math.sin(a) * R; c.ry = -a - Math.PI / 2; setInst(trm, i, c); });
        trm.instanceMatrix.needsUpdate = true;
      });
    },
  },
  {
    id: 'ancient-coliseum', name: 'Ancient Coliseum', rarity: 'rare', price: 400, icon: '🏛️',
    desc: 'Sand arena inside stepped stone seating at sunset, columns on the rim and banners on the wall.',
    sky: ['#f08a5d', '#ffc89a', '#ffe8c8'], fog: ['#e9bf94', 32, 68], hemi: ['#ffe6cc', '#8a6a4a', 1.4], sun: ['#ffd2a0', 2.8],
    slab: '#6e5a44', benchSlab: '#7a6248', slot: '#a88a64', ped: '#bfae94',
    tiles: { mine: '#33689e', enemy: '#a63c32', enemyPlan: '#5e2a24' },
    props(ctx) {
      plain(ctx, std({ color: '#bfa070' }));
      island(ctx, speckle('#d6b886', ['#e6cc9e', '#b89664', '#c4a476'], { seed: 41 }), '#a88a64', '#8a6e4e');
      const stone = speckle('#cdbb9c', ['#b4a282', '#e0d2b8'], { seed: 42, repeat: 6, n: 900 });
      const pts = [[13.9, -1.3], [13.9, 0.7], [15.1, 0.7], [15.1, 1.7], [16.3, 1.7], [16.3, 2.7], [17.5, 2.7], [17.5, 3.7], [18.7, 3.7], [18.7, 5.6], [19.3, 5.6], [19.3, -1.3]].map(([r, y]) => new THREE.Vector2(r, y));
      mesh(ctx, new THREE.LatheGeometry(pts, 72), std({ map: stone, roughness: 1, side: THREE.DoubleSide }), { receive: true, shadow: false });
      const n = 30;
      const col = merge([part(new THREE.CylinderGeometry(0.32, 0.36, 3, 10), '#e8dcc4', { y: 1.5 }), part(new THREE.BoxGeometry(0.9, 0.25, 0.9), '#d8ccb2', { y: 3.1 }), part(new THREE.BoxGeometry(0.9, 0.2, 0.9), '#d8ccb2', { y: 0.1 })]);
      const cols = [];
      for (let i = 0; i < n; i++) { const a = (i / n) * Math.PI * 2; if (i % 7 === 3) continue; cols.push({ x: Math.cos(a) * 19, y: 5.6, z: Math.sin(a) * 19, ry: -a }); }
      inst(ctx, col, std({ vertexColors: true, roughness: 0.9 }), cols);
      mesh(ctx, new THREE.CylinderGeometry(19.5, 19.5, 0.5, 72, 1, true), std({ color: '#d8ccb2', side: THREE.DoubleSide }), { y: 9.0 });
      // broken columns and rubble on the arena floor rim
      const spots = rimSpots(ctx.rng, 50, 10, 12.2);
      inst(ctx, new THREE.CylinderGeometry(0.32, 0.36, 1, 10), std({ color: '#e0d4bc' }), spots.slice(0, 7).map((p) => ({ x: p.x, y: 0.5, z: p.z, sy: 0.6 + ctx.rng() * 1.8, ry: ctx.rng() * 3 })));
      inst(ctx, new THREE.DodecahedronGeometry(0.35, 0), std({ color: '#cbbd9f', flatShading: true }), spots.slice(7, 22).map((p) => ({ x: p.x, y: 0.1, z: p.z, s: 0.6 + ctx.rng() * 0.8, ry: ctx.rng() * 3 })));
      // banners on the inner wall, swaying
      const banners = [];
      for (let i = 0; i < 12; i++) {
        const a = Math.PI + (i / 11) * Math.PI; // back half
        banners.push({ x: Math.cos(a) * 13.85, y: -0.3, z: Math.sin(a) * 13.85, ry: -a - Math.PI / 2, color: i % 2 ? '#2f5f9e' : '#a8322a', phase: i });
      }
      const bGeo = new THREE.PlaneGeometry(0.9, 1.6);
      bGeo.translate(0, -0.8, 0.02);
      const bm = inst(ctx, bGeo, std({ color: '#ffffff', side: THREE.DoubleSide, roughness: 0.9 }), banners.map((b) => ({ ...b, y: 0.65 })), { moving: true, shadow: false });
      ctx.tick((dt, t) => { banners.forEach((b, i) => { b.y = 0.65; b.rx = -0.08 + Math.sin(t * 1.6 + b.phase) * 0.08; setInst(bm, i, b); }); bm.instanceMatrix.needsUpdate = true; });
      // torch flames on the rim
      const torches = spots.slice(22, 28).map((p) => ({ x: p.x, z: p.z }));
      inst(ctx, new THREE.CylinderGeometry(0.08, 0.1, 1.4, 6), std({ color: '#5a4030' }), torches.map((p) => ({ x: p.x, y: 0.7, z: p.z })));
      const flames = torches.map((p, i) => ({ x: p.x, y: 1.6, z: p.z, phase: i * 1.9 }));
      const fm = inst(ctx, new THREE.ConeGeometry(0.16, 0.45, 8), new THREE.MeshBasicMaterial({ color: '#ffb347' }), flames, { moving: true, shadow: false });
      ctx.tick((dt, t) => { flames.forEach((f, i) => { const k = 1 + 0.18 * Math.sin(t * 13 + f.phase) + 0.1 * Math.sin(t * 7.3 + f.phase * 2); f.sx = f.sz = 1 / Math.sqrt(k); f.sy = k; setInst(fm, i, f); }); fm.instanceMatrix.needsUpdate = true; });
    },
  },
  {
    id: 'infinity-mountain', name: 'Infinity Mountain', rarity: 'epic', price: 900, icon: '🏔️',
    desc: 'Snowfield at night under the stars, snowy pines, falling snow and a tall peak in the distance.',
    sky: ['#050a22', '#14204a', '#2c3c6e'], fog: ['#1c2748', 28, 62], hemi: ['#8fb0ff', '#262c44', 1.15], sun: ['#c4d6ff', 2.0],
    slab: '#343c52', benchSlab: '#444658', slot: '#666c86', ped: '#56668a',
    tiles: { mine: '#3a70bf', enemy: '#a63a58', enemyPlan: '#5a2a3c' },
    props(ctx) {
      plain(ctx, std({ map: speckle('#c9d3e8', ['#ffffff', '#a8b4d0'], { seed: 51, repeat: 12, n: 600 }), roughness: 1 }));
      island(ctx, speckle('#e4ecf8', ['#ffffff', '#c4d0e6', '#b6c4dc'], { seed: 52 }), '#7e8aa2', '#6a7590');
      const spots = rimSpots(ctx.rng, 70, 9.7, 12.3);
      inst(ctx, pineGeo(true), std({ vertexColors: true, roughness: 0.9 }), rimSpots(ctx.rng, 60, 9.7, 12.3, true).slice(0, 20).map((p) => ({ x: p.x, z: p.z, s: 0.8 + ctx.rng() * 0.8, ry: ctx.rng() * 6 })));
      inst(ctx, new THREE.IcosahedronGeometry(0.55, 0), std({ color: '#9fc4e8', roughness: 0.3, flatShading: true, emissive: '#1a3a66', emissiveIntensity: 0.4 }),
        spots.slice(22, 34).map((p) => ({ x: p.x, y: 0.2, z: p.z, sx: 0.6 + ctx.rng(), sy: 0.6 + ctx.rng() * 1.5, sz: 0.6 + ctx.rng(), ry: ctx.rng() * 3 })));
      // distant peaks with snow caps
      const peak = merge([part(new THREE.ConeGeometry(1, 1, 7), '#4a5470'), part(new THREE.ConeGeometry(0.42, 0.42, 7), '#f4f8ff', { y: 0.29 })]);
      inst(ctx, peak, std({ vertexColors: true, flatShading: true, roughness: 1, fog: false }), [
        { x: 2, y: 12, z: -46, sx: 20, sy: 30, sz: 20 }, { x: -24, y: 6, z: -40, sx: 14, sy: 17, sz: 14, ry: 0.5 }, { x: 26, y: 5, z: -38, sx: 13, sy: 15, sz: 13, ry: 1 },
      ], { shadow: false });
      mesh(ctx, new THREE.SphereGeometry(3, 20, 14), new THREE.MeshBasicMaterial({ color: '#f4f1dc', fog: false }), { x: -30, y: 30, z: -60 });
      // stars on a dome
      const rng = DT.makeRng(53);
      const sp = new Float32Array(260 * 3);
      for (let i = 0; i < 260; i++) {
        const a = rng() * Math.PI * 2, e = 0.12 + rng() * 1.3;
        sp[i * 3] = Math.cos(a) * Math.cos(e) * 90; sp[i * 3 + 1] = Math.sin(e) * 90; sp[i * 3 + 2] = Math.sin(a) * Math.cos(e) * 90;
      }
      const sg = new THREE.BufferGeometry();
      sg.setAttribute('position', new THREE.BufferAttribute(sp, 3));
      const stars = new THREE.Points(sg, new THREE.PointsMaterial({ color: '#ffffff', size: 1.3, map: softDot(), transparent: true, depthWrite: false, fog: false }));
      ctx.g.add(stars);
      ctx.tick((dt, t) => { stars.material.opacity = 0.75 + 0.25 * Math.sin(t * 1.7); });
      particles(ctx, 300, { color: '#ffffff', size: 0.14, v: [0.15, -0.9, 0.1], wobble: 0.35, opacity: 0.9, box: [-17, 17, 0, 13, -15, 12] });
    },
  },
  {
    id: 'dark-ocean', name: 'Dark Ocean', rarity: 'epic', price: 900, icon: '🌊',
    desc: 'Black rock in a grey sea, thick fog, a broken pier and a lighthouse sweeping its beam.',
    sky: ['#24272c', '#3e4248', '#5a5f66'], fog: ['#474b52', 16, 46], hemi: ['#a0aab6', '#1a1c20', 1.15], sun: ['#d6dae2', 1.7],
    slab: '#1c1e24', benchSlab: '#2a2a30', slot: '#3e3e48', ped: '#383844',
    tiles: { mine: '#3a6e96', enemy: '#9c3e4c', enemyPlan: '#552c34' },
    props(ctx) {
      sea(ctx, { color: '#2a3036', amp: 0.24, rough: 0.35, metal: 0.25, speed: 0.8 });
      island(ctx, speckle('#3b3b40', ['#2a2a2e', '#4e4e55', '#55524a'], { seed: 61 }), '#2a2a2e', '#202024');
      const spots = rimSpots(ctx.rng, 70, 9.6, 12.4);
      inst(ctx, new THREE.TetrahedronGeometry(0.8, 0), std({ color: '#44464c', flatShading: true }),
        spots.slice(0, 22).map((p) => ({ x: p.x, y: 0.25, z: p.z, sx: 0.6 + ctx.rng() * 0.8, sy: 0.8 + ctx.rng() * 1.6, sz: 0.6 + ctx.rng() * 0.8, rx: ctx.rng(), ry: ctx.rng() * 6 })));
      const dead = merge([part(new THREE.CylinderGeometry(0.06, 0.12, 2, 5), '#3a332c', { y: 1 }), part(new THREE.CylinderGeometry(0.03, 0.05, 0.9, 4), '#3a332c', { x: 0.25, y: 1.5, rz: -0.8 }), part(new THREE.CylinderGeometry(0.03, 0.05, 0.7, 4), '#3a332c', { x: -0.2, y: 1.2, rz: 0.9 })]);
      inst(ctx, dead, std({ vertexColors: true }), spots.slice(22, 30).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.9 + ctx.rng() * 0.6 })));
      // broken pier posts in the water
      const posts = [];
      for (let i = 0; i < 7; i++) if (i !== 4) posts.push({ x: -13.5 - i * 1.3, y: -0.6, z: -3 + (i % 2) * 0.9, sy: 0.7 + ((i * 37) % 5) * 0.12, rz: (i % 3 - 1) * 0.08 });
      inst(ctx, new THREE.CylinderGeometry(0.15, 0.17, 2, 6), std({ color: '#2e2620' }), posts);
      // lighthouse
      const LX = 11.4, LZ = -6.2;
      const stripes = canvasTex(32, 128, (g) => { for (let i = 0; i < 4; i++) { g.fillStyle = i % 2 ? '#e8e4dc' : '#9e2b2b'; g.fillRect(0, i * 32, 32, 32); } });
      mesh(ctx, new THREE.CylinderGeometry(0.55, 0.85, 5, 16), std({ map: stripes, roughness: 0.7 }), { x: LX, y: 2.5, z: LZ, shadow: true });
      mesh(ctx, new THREE.CylinderGeometry(0.85, 0.85, 0.15, 16), std({ color: '#25272c' }), { x: LX, y: 5.05, z: LZ });
      mesh(ctx, new THREE.CylinderGeometry(0.42, 0.42, 0.6, 12), std({ color: '#fff3b0', emissive: '#ffd860', emissiveIntensity: 1.4 }), { x: LX, y: 5.4, z: LZ });
      mesh(ctx, new THREE.ConeGeometry(0.6, 0.6, 12), std({ color: '#25272c' }), { x: LX, y: 6.0, z: LZ });
      const beamGeo = new THREE.ConeGeometry(1.6, 14, 20, 1, true);
      beamGeo.translate(0, -7, 0);
      beamGeo.rotateZ(Math.PI / 2);
      const beam = new THREE.Group();
      beam.position.set(LX, 5.4, LZ);
      const beamMat = new THREE.MeshBasicMaterial({ color: '#fff2b8', transparent: true, opacity: 0.16, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide, fog: false });
      const b1 = new THREE.Mesh(beamGeo, beamMat);
      const b2 = new THREE.Mesh(beamGeo, beamMat);
      b2.rotation.y = Math.PI;
      beam.add(b1, b2);
      ctx.g.add(beam);
      ctx.tick((dt, t) => { beam.rotation.y = t * 0.7; });
      particles(ctx, 90, { color: '#9aa2ac', size: 3.2, v: [0.5, 0, 0.15], wobble: 0.1, opacity: 0.09, box: [-20, 20, -0.5, 2.5, -16, 11] });
    },
  },
  {
    id: 'gear-savanna', name: 'Gear Savanna', rarity: 'epic', price: 900, icon: '⚙️',
    desc: 'Golden grassland with flat-topped trees and huge brass gears turning half-buried in the ground.',
    sky: ['#86c4f0', '#d6ecff', '#ffe2a8'], fog: ['#ecd8a8', 32, 70], hemi: ['#fff3d6', '#8a7444', 1.5], sun: ['#ffe6b8', 2.8],
    slab: '#4a3d2a', benchSlab: '#5a4630', slot: '#7a6040', ped: '#8a7a5a',
    tiles: { mine: '#2e6a9c', enemy: '#a8482a', enemyPlan: '#5e2e1e' },
    props(ctx) {
      plain(ctx, std({ map: speckle('#b89e48', ['#cdb45a', '#9a8238', '#d8c070'], { seed: 71, repeat: 12 }), roughness: 1 }));
      island(ctx, speckle('#c4ae58', ['#d4bf6a', '#a8923e', '#8aa04a'], { seed: 72 }), '#8a6a3a', '#745830');
      const spots = rimSpots(ctx.rng, 90, 9.7, 12.3);
      const acacia = merge([
        part(new THREE.CylinderGeometry(0.1, 0.16, 1.8, 6), '#6a4a2a', { y: 0.9, rz: 0.12 }),
        part(new THREE.CylinderGeometry(0.06, 0.08, 0.9, 5), '#6a4a2a', { x: 0.35, y: 1.8, rz: -0.6 }),
        part(new THREE.CylinderGeometry(1.5, 1.2, 0.35, 10), '#5e8a32', { x: 0.15, y: 2.2 }),
        part(new THREE.CylinderGeometry(0.9, 0.8, 0.3, 9), '#6e9a3a', { x: 0.6, y: 2.0, z: 0.4 }),
      ]);
      inst(ctx, acacia, std({ vertexColors: true, roughness: 0.9, flatShading: true }), rimSpots(ctx.rng, 40, 9.9, 12.3, true).slice(0, 8).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.8 + ctx.rng() * 0.4 })));
      const tufts = spots.slice(8, 90).map((p) => ({ x: p.x, y: 0.2, z: p.z, s: 0.5 + ctx.rng() * 0.5, ry: ctx.rng() * 3 }));
      inst(ctx, new THREE.ConeGeometry(0.18, 0.5, 4), std({ color: '#d8c068', flatShading: true }), tufts, { shadow: false });
      // gears: standing half-buried on the rim and far out, turning; some lying flat
      const brass = std({ color: '#d0a050', metalness: 0.35, roughness: 0.45 });
      const steel = std({ color: '#9aa4ae', metalness: 0.3, roughness: 0.5 });
      const gears = [];
      const add = (geo, m, list) => { const g = inst(ctx, geo, m, list, { moving: true }); gears.push({ g, list }); };
      add(gearGeo(1.8, 14, 0.4), brass, [{ x: -11.2, y: 0.4, z: -4.5, ry: 1.2, spin: 0.35 }, { x: 11.6, y: 0.2, z: -2.5, ry: -1.3, spin: -0.3 }, { x: 4, y: 0.6, z: -11.6, ry: 0.1, spin: 0.25 }]);
      add(gearGeo(1.1, 10, 0.3), steel, [{ x: -9.9, y: 0.7, z: -6.6, ry: 1.0, spin: -0.57 }, { x: 2.3, y: 0.5, z: -11.2, ry: 0.15, spin: -0.4 }, { x: 10.8, y: 0.9, z: -5.2, ry: -1.1, spin: 0.5 }]);
      add(gearGeo(5, 22, 0.8), brass, [{ x: -18, y: 0, z: -20, ry: 0.7, spin: 0.08 }, { x: 22, y: -1, z: -16, ry: -0.8, spin: -0.07 }, { x: -3, y: -0.5, z: -28, ry: 0, spin: 0.06 }]);
      add(gearGeo(0.9, 9, 0.2), steel, ([{ x: -10.6, y: 0.1, z: 0.0, rx: -Math.PI / 2, spin: 0.6 }, { x: 10.2, y: 0.1, z: 2.0, rx: -Math.PI / 2, spin: -0.5 }, { x: -6.5, y: 0.1, z: -9.5, rx: -Math.PI / 2, spin: 0.7 }]));
      ctx.tick((dt, t) => {
        for (const { g, list } of gears) {
          list.forEach((it, i) => {
            const a = t * it.spin;
            dummy.position.set(it.x, it.y, it.z);
            if (it.rx) dummy.rotation.set(it.rx, 0, a);
            else { dummy.rotation.set(0, it.ry, 0); dummy.rotateZ(a); }
            dummy.scale.set(1, 1, 1);
            dummy.updateMatrix();
            g.setMatrixAt(i, dummy.matrix);
          });
          g.instanceMatrix.needsUpdate = true;
        }
      });
      particles(ctx, 90, { color: '#fff1b0', size: 0.1, v: [0.4, 0.15, 0], wobble: 0.5, opacity: 0.8, additive: true, box: [-16, 16, 0.3, 4, -14, 10] });
    },
  },
  {
    id: 'digital-grid', name: 'Digital Grid', rarity: 'legendary', price: 1800, icon: '💠',
    desc: 'Cyberspace: neon wireframe island on a glowing grid, floating polyhedra and falling data rain.',
    sky: ['#000000', '#020616', '#0a1438'], fog: ['#030714', 24, 60], hemi: ['#5a7cff', '#050510', 0.9], sun: ['#a8c4ff', 1.4],
    slab: '#060a16', benchSlab: '#0c1428', slot: '#1c3060', ped: '#1a2c52',
    tiles: { mine: '#0e2c52', enemy: '#3a0c2a', enemyPlan: '#24081c', glowMine: '#0a4a90', glowEnemy: '#8a0e50' },
    props(ctx) {
      const grid = canvasTex(64, 64, (g) => { g.fillStyle = '#02040c'; g.fillRect(0, 0, 64, 64); g.strokeStyle = '#1aa0ff'; g.lineWidth = 2; g.strokeRect(0, 0, 64, 64); }, 60);
      const floor = plain(ctx, new THREE.MeshBasicMaterial({ map: grid }));
      ctx.tick((dt, t) => { grid.offset.y = (t * 0.05) % 1; });
      const top = canvasTex(64, 64, (g) => { g.fillStyle = '#060c1c'; g.fillRect(0, 0, 64, 64); g.strokeStyle = 'rgba(80,220,255,0.35)'; g.lineWidth = 1.5; g.strokeRect(0.5, 0.5, 63, 63); }, 14);
      island(ctx, top, '#050914', '#03060e');
      floor.position.y = -1.6;
      // neon edges on the island and the board
      const edge = (geo, color, y) => { const l = new THREE.LineSegments(new THREE.EdgesGeometry(geo), new THREE.LineBasicMaterial({ color })); l.position.y = y; geo.dispose(); ctx.g.add(l); return l; };
      edge(new THREE.CylinderGeometry(12.5, 13.6, 1.4, 28), '#2ad4ff', -0.72);
      edge(new THREE.BoxGeometry(14.4, 0.3, 12.8), '#7a5cff', -0.14);
      // hex outlines on every tile: cyan for your half, magenta for the enemy half
      const ring = new THREE.RingGeometry(0.84, 0.94, 6);
      ring.rotateZ(Math.PI / 2);
      ring.rotateX(-Math.PI / 2);
      const rings = DT.CELLS.map(({ c, r }) => { const p = cellWorld(c, r); return { x: p.x, y: 0.147, z: p.z, color: r >= 4 ? '#36d6ff' : '#ff3ab0' }; });
      const rm = inst(ctx, ring, new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.85, depthWrite: false }), rings, { shadow: false });
      ctx.tick((dt, t) => { rm.material.opacity = 0.7 + 0.2 * Math.sin(t * 2.2); });
      // floating wireframe polyhedra
      const spots = rimSpots(ctx.rng, 60, 10, 12.4);
      const polys = rimSpots(ctx.rng, 60, 10, 12.4, true).slice(0, 10).map((p, i) => ({ x: p.x, y: 1.6 + ctx.rng() * 1.6, z: p.z, s: 0.5 + ctx.rng() * 0.5, phase: ctx.rng() * 6, color: i % 2 ? '#36d6ff' : '#b45cff' }));
      polys.forEach((p) => { p.base = p.y; });
      const pm = inst(ctx, new THREE.IcosahedronGeometry(1, 0), new THREE.MeshBasicMaterial({ color: '#ffffff', wireframe: true }), polys, { moving: true, shadow: false });
      ctx.tick((dt, t) => { polys.forEach((p, i) => { p.y = p.base + Math.sin(t + p.phase) * 0.25; p.ry = t * 0.5 + p.phase; p.rx = t * 0.3; setInst(pm, i, p); }); pm.instanceMatrix.needsUpdate = true; });
      // light pillars far out
      const pillars = farSpots(ctx.rng, 14, 16, 30).map((p, i) => ({ x: p.x, y: 6, z: p.z, sx: 0.4, sy: 14 + (i % 4) * 4, sz: 0.4, color: i % 3 ? '#1a6cff' : '#ff2a9a' }));
      inst(ctx, new THREE.BoxGeometry(1, 1, 1), new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.35, depthWrite: false, blending: THREE.AdditiveBlending }), pillars, { shadow: false });
      particles(ctx, 320, { color: '#4dffb0', size: 0.16, v: [0, -4, 0], wobble: 0, opacity: 0.85, additive: true, box: [-18, 18, -1, 14, -16, 11] });
    },
  },
];

THEMES.push(...extraThemes({ THREE, DT, C, std, dummy, canvasTex, gradient, speckle, softDot, part, merge, setInst, inst, mesh, rimSpots, farSpots, sea, plain, particles, island, pineGeo, gearGeo, LAYOUT }));

THEMES.push(...mysteryBoards({ THREE, std, mesh, island, sea, inst, setInst }));
const THEME = Object.fromEntries(THEMES.map((t) => [t.id, t]));
const themeOf = (id) => THEME[id] || THEME[DEFAULT];

// ---------- building and disposing ----------
// What a theme can see of the game: phase, stage, whether the viewer's fight was won, and the total digivolve
// level of the viewer's Digimon (it goes up when three copies combine). Store previews get a fixed mid-game state.
const NO_GAME = { phase: 'none', stage: 1, won: false, evo: 0 };
const PREVIEW_STATE = { phase: 'planning', stage: 4, won: false, evo: 0 };
function gameSignals() {
  if (DT.Boards && DT.Boards.testSignals) return DT.Boards.testSignals; // tests/boards.cjs drives reactions with this
  const g = DT.UI && DT.UI.game ? DT.UI.game() : null;
  if (!g || !g.human) return NO_GAME;
  const p = g.human;
  let evo = 0;
  if (p.allUnits) for (const u of p.allUnits()) evo += u.stage || 0;
  let won = false;
  if (g.phase !== 'planning' && g.matchOf) {
    const m = g.matchOf(p);
    const cb = m && m.combat;
    won = !!(cb && cb.done && cb.winner === m.team);
  }
  return { phase: g.phase, stage: g.stage || 1, won, evo };
}

// opts.L / opts.scene: the lights and scene this env drives (live lighting and weather); opts.preview for store images
function buildEnv(spec, { L = null, scene = null, preview = false } = {}) {
  const g = new THREE.Group();
  g.name = `board:${spec.id}`;
  const ticks = [];
  const handlers = {};
  let lit = null, fogFx = null;
  const ctx = {
    g, spec, preview, rng: DT.makeRng(spec.id.length * 101 + spec.id.charCodeAt(0)), tick: (fn) => ticks.push(fn),
    state: { ...(preview ? PREVIEW_STATE : NO_GAME) },
    // on('combat' | 'planning' | 'result' | 'stage' | 'win' | 'evolve', fn(t))
    on: (name, fn) => { (handlers[name] = handlers[name] || []).push(fn); },
    // live multipliers on the theme's lights; flash adds to the sky light and fades by itself
    light: () => lit || (lit = { hemi: 1, sun: 1, tint: C(spec.sun[0]), tintK: 0, flash: 0, flashColor: C('#ffffff') }),
    // live fog values, applied every frame
    fog: () => fogFx || (fogFx = { near: spec.fog[1], far: spec.fog[2], color: C(spec.fog[0]) }),
  };
  // board slab under the tiles and the bench slab: same sizes as before
  mesh(ctx, new THREE.BoxGeometry(14.4, 0.3, 12.8), std({ color: spec.slab, roughness: 0.8 }), { y: -0.14, receive: true });
  mesh(ctx, new THREE.BoxGeometry(DT.BENCH * BENCH_STEP + 0.5, 0.25, 1.9), std({ color: spec.benchSlab }), { y: -0.12, z: BENCH_Z, receive: true });
  spec.props(ctx);
  const surface = spec.surface ? spec.surface() : {};
  const background = Array.isArray(spec.sky) ? gradient(spec.sky) : C(spec.sky);
  const base = { hemi: C(spec.hemi[0]), sun: C(spec.sun[0]) };
  let prev = null, now = 0;
  const emit = (name) => { for (const fn of handlers[name] || []) fn(now); };
  return {
    group: g, ticks, background, surface,
    // called every frame with gameSignals(); fires handlers on changes (the first call only records the state)
    signal(s) {
      Object.assign(ctx.state, s);
      if (!prev) { prev = { ...s }; return; }
      if (s.phase !== prev.phase) emit(s.phase);
      if (s.stage !== prev.stage) emit('stage');
      if (s.won && !prev.won) emit('win');
      if (s.evo > prev.evo && s.phase !== 'none' && prev.phase !== 'none') emit('evolve');
      prev = { ...s };
    },
    tick(dt, t) {
      now = t;
      for (const f of ticks) f(dt, t);
      if (lit && L) {
        lit.flash = Math.max(0, lit.flash - dt * 5);
        L.hemi.intensity = spec.hemi[2] * lit.hemi + lit.flash;
        L.hemi.color.copy(base.hemi).lerp(lit.flashColor, Math.min(1, lit.flash));
        L.sun.intensity = spec.sun[1] * lit.sun;
        L.sun.color.copy(base.sun).lerp(lit.tint, lit.tintK);
      }
      if (fogFx && scene && scene.fog) { scene.fog.near = fogFx.near; scene.fog.far = fogFx.far; scene.fog.color.copy(fogFx.color); }
    },
  };
}

function disposeEnv(env) {
  disposeTree(env.group);
  if (env.background.isTexture) env.background.dispose();
  for (const tex of Object.values(env.surface)) if (tex && tex.isTexture) tex.dispose();
}

// a material's texture; switching between textured and plain needs a shader rebuild
function setMap(m, tex) {
  tex = tex || null;
  if (m.map === tex) return;
  if (!!m.map !== !!tex) m.needsUpdate = true;
  m.map = tex;
}
// tiles, bench slots and item pedestals: color, finish and texture from the theme
function applySurface(spec, env, { tiles = [], slots = [], peds = [] }) {
  const S = env.surface, lk = spec.look || {};
  for (const m of tiles) { m.roughness = spec.tiles.rough ?? 0.6; m.metalness = spec.tiles.metal ?? 0.1; setMap(m, S.tile); }
  for (const m of slots) { m.color.set(spec.slot); m.roughness = lk.slotRough ?? 0.7; m.metalness = lk.slotMetal ?? 0; setMap(m, S.slot); }
  for (const m of peds) { m.color.set(spec.ped); m.roughness = lk.pedRough ?? 0.6; m.metalness = lk.pedMetal ?? 0; setMap(m, S.ped); }
}

function disposeTree(obj) {
  const seen = new Set();
  obj.traverse((o) => {
    if (o.geometry && !seen.has(o.geometry)) { seen.add(o.geometry); o.geometry.dispose(); }
    for (const m of [].concat(o.material || [])) {
      if (seen.has(m)) continue;
      seen.add(m);
      for (const v of Object.values(m)) if (v && v.isTexture && !seen.has(v)) { seen.add(v); v.dispose(); }
      m.dispose();
    }
    if (o.isInstancedMesh) o.dispose();
  });
}

function tileLook(spec) {
  const t = spec.tiles;
  return { mine: C(t.mine), enemy: C(t.enemy), enemyPlan: C(t.enemyPlan), glowMine: C(t.glowMine || '#000000'), glowEnemy: C(t.glowEnemy || '#000000') };
}

function lights(scene, shadowSize) {
  const hemi = new THREE.HemisphereLight('#ffffff', '#000000', 1);
  const sun = new THREE.DirectionalLight('#ffffff', 2);
  sun.position.set(-8, 18, 10);
  sun.castShadow = true;
  sun.shadow.mapSize.set(shadowSize, shadowSize);
  Object.assign(sun.shadow.camera, { left: -14, right: 14, top: 14, bottom: -14, near: 1, far: 50 });
  sun.shadow.bias = -0.0005;
  scene.add(hemi, sun);
  return { hemi, sun };
}
function setLights(L, spec) {
  L.hemi.color.set(spec.hemi[0]);
  L.hemi.groundColor.set(spec.hemi[1]);
  L.hemi.intensity = spec.hemi[2];
  L.sun.color.set(spec.sun[0]);
  L.sun.intensity = spec.sun[1];
}
function setFog(scene, spec) {
  if (!scene.fog) scene.fog = new THREE.Fog(spec.fog[0], spec.fog[1], spec.fog[2]);
  scene.fog.color.set(spec.fog[0]);
  scene.fog.near = spec.fog[1];
  scene.fog.far = spec.fog[2];
}

// ---------- which board this viewer should see ----------
function wanted(r) {
  const mine = themeOf(DT.Cosmetics.equipped('board')).id;
  const g = DT.UI && DT.UI.game ? DT.UI.game() : null;
  if (!g || !g.human || g.phase === 'planning' || !r.mirror) return mine;
  const m = g.matchOf(g.human);
  if (!m || m.pve) return mine;
  const opp = m.team === 1 ? m.a : m.b;
  const look = opp && DT.Social.lookOf(opp);
  return themeOf(look && look.board).id;
}

// ---------- renderer integration ----------
function attach(r) {
  const L = lights(r.scene, 2048);
  r.boardLights = L;
  const overlay = document.createElement('div');
  overlay.className = 'board-fade';
  Object.assign(overlay.style, { position: 'absolute', inset: '0', pointerEvents: 'none', opacity: '0', zIndex: '1', background: '#000' });
  r.canvas.insertAdjacentElement('afterend', overlay);
  let fade = null;
  let lastWant = null;
  // overlay opacity 0 -> 1, swap at full cover, then 1 -> 0
  const fadeStep = () => {
    if (!fade) return;
    const now = performance.now();
    fade.k += Math.min(0.08, (now - fade.last) / 1000) / FADE;
    fade.last = now;
    const k = fade.k;
    if (k >= 1 && !fade.swapped) { fade.swapped = true; r.setBoard(fade.to, { keepFade: true }); }
    overlay.style.opacity = String(k < 1 ? k : Math.max(0, 2 - k));
    if (k < 2) requestAnimationFrame(fadeStep);
    else { overlay.style.opacity = '0'; fade = null; }
  };

  // Swap the arena to theme id. Safe to call any time and repeatedly; the same id is a no-op.
  // opts.fade: dip to the new theme's fog color instead of popping.
  r.setBoard = function (id, opts = {}) {
    const spec = themeOf(id);
    if (opts.fade && this.board) {
      if (this.boardId() === spec.id) return;
      overlay.style.background = spec.fog[0];
      const cur = Number(overlay.style.opacity) || 0;
      const running = !!fade;
      fade = { to: spec.id, k: cur, last: performance.now(), swapped: false };
      if (!running) requestAnimationFrame(fadeStep);
      return;
    }
    // an instant swap cancels a fade in flight, so the fade cannot swap back afterwards
    if (fade && !opts.keepFade) { fade = null; overlay.style.opacity = '0'; }
    if (this.board && this.board.id === spec.id && !opts.force) return;
    const env = buildEnv(spec, { L, scene: this.scene });
    if (this.board) {
      this.scene.remove(this.board.env.group);
      disposeEnv(this.board.env);
    }
    this.scene.add(env.group);
    this.scene.background = env.background;
    setFog(this.scene, spec);
    setLights(L, spec);
    this.tileLook = tileLook(spec);
    for (const t of this.tiles) {
      t.material.color.copy(t.userData.r >= 4 ? this.tileLook.mine : this.tileLook.enemy);
      t.material.emissive.copy(t.userData.r >= 4 ? this.tileLook.glowMine : this.tileLook.glowEnemy);
    }
    applySurface(spec, env, { tiles: this.tiles.map((t) => t.material), slots: this.slots.map((s) => s.material), peds: this.itemPeds.map((p) => p.material) });
    // keep the tick clock so animations continue smoothly
    env.signal(gameSignals());
    env.tick(0, this.time || 0);
    this.board = { id: spec.id, env };
    if (this.onBoard) this.onBoard(spec.id);
  };

  r.boardId = () => (fade ? fade.to : r.board ? r.board.id : null);

  // per frame: theme animation, and a fade when the board this viewer should see changes. Edge-triggered, so a
  // direct setBoard(id) call (scouting another Tamer) sticks until the next round transition or equip.
  // r.boardOverride = id pins a board (fading to it) and pauses the automatic choice; null hands it back.
  r.boardOverride = null;
  const target = () => (r.boardOverride ? themeOf(r.boardOverride).id : wanted(r));
  r.boardTick = function (dt, t) {
    const w = target();
    if (w !== lastWant) { lastWant = w; if (w !== this.boardId()) this.setBoard(w, { fade: true }); }
    if (this.board) { this.board.env.signal(gameSignals()); this.board.env.tick(dt, t); }
  };

  DT.Cosmetics.onChange((kind) => {
    if (kind !== 'board' || r.boardOverride) return;
    const w = wanted(r);
    lastWant = w;
    if (w !== r.boardId()) r.setBoard(w, { fade: !!(DT.UI && DT.UI.game && DT.UI.game()) });
  });

  lastWant = wanted(r);
  r.setBoard(lastWant);
}

// ---------- store previews: one shared offscreen renderer, results cached as images ----------
const PW = 320, PH = 200;
let pv = null;
const previewCache = new Map();
const previewWait = new Map();
let pvQueue = Promise.resolve();

function previewSetup() {
  const canvas = document.createElement('canvas');
  canvas.width = PW;
  canvas.height = PH;
  const r = new THREE.WebGLRenderer({ canvas, antialias: true, preserveDrawingBuffer: true });
  r.setPixelRatio(1);
  r.setSize(PW, PH, false);
  r.outputColorSpace = THREE.SRGBColorSpace;
  r.shadowMap.enabled = true;
  r.shadowMap.type = THREE.PCFSoftShadowMap;
  const scene = new THREE.Scene();
  const L = lights(scene, 1024);
  const cam = new THREE.PerspectiveCamera(38, PW / PH, 0.1, 200);
  const d = Math.hypot(16.5, 15.5) * 1.02, pitch = Math.atan2(16.5, 15.5);
  cam.position.set(0, Math.sin(pitch) * d, 1.8 + Math.cos(pitch) * d);
  cam.lookAt(0, 0, 1.8);
  // stand-ins for the gameplay pieces
  const tileGeo = new THREE.CylinderGeometry(0.93, 0.97, 0.14, 6);
  const mineMat = std({ roughness: 0.6, metalness: 0.1 }), enemyMat = std({ roughness: 0.6, metalness: 0.1 });
  const cellsOf = (mine) => DT.CELLS.filter(({ r: row }) => (row >= 4) === mine).map(({ c, r: row }) => { const p = cellWorld(c, row); return { x: p.x, y: 0.07, z: p.z }; });
  const fake = { g: scene };
  const tm = inst(fake, tileGeo, mineMat, cellsOf(true));
  const te = inst(fake, tileGeo, enemyMat, cellsOf(false));
  const slotMat = std({ roughness: 0.7 }), pedMat = std({ roughness: 0.6 });
  inst(fake, new THREE.BoxGeometry(1.25, 0.1, 1.25), slotMat, Array.from({ length: DT.BENCH }, (_, i) => ({ ...benchWorld(i), y: 0.05 })));
  inst(fake, new THREE.CylinderGeometry(0.42, 0.48, 0.16, 20), pedMat, Array.from({ length: ITEM_SLOTS }, (_, i) => ({ ...itemWorld(i), y: 0.08 })));
  tm.castShadow = te.castShadow = false;
  pv = { canvas, r, scene, cam, L, mineMat, enemyMat, slotMat, pedMat };
}

function renderPreview(id) {
  if (!pv) previewSetup();
  const spec = themeOf(id);
  const env = buildEnv(spec, { L: pv.L, scene: pv.scene, preview: true });
  pv.scene.add(env.group);
  pv.scene.background = env.background;
  setFog(pv.scene, spec);
  setLights(pv.L, spec);
  env.tick(1 / 60, 2.0);
  const look = tileLook(spec);
  pv.mineMat.color.copy(look.mine); pv.mineMat.emissive.copy(look.glowMine);
  pv.enemyMat.color.copy(look.enemy); pv.enemyMat.emissive.copy(look.glowEnemy);
  applySurface(spec, env, { tiles: [pv.mineMat, pv.enemyMat], slots: [pv.slotMat], peds: [pv.pedMat] });
  pv.r.render(pv.scene, pv.cam);
  let url = pv.canvas.toDataURL('image/webp', 0.86);
  if (!url.startsWith('data:image/webp')) url = pv.canvas.toDataURL('image/png');
  pv.scene.remove(env.group);
  disposeEnv(env);
  return url;
}

// Promise of a data URL picture of the theme; one render at a time, each cached for the session
function previewUrl(id) {
  id = themeOf(id).id;
  if (previewCache.has(id)) return Promise.resolve(previewCache.get(id));
  if (previewWait.has(id)) return previewWait.get(id);
  const p = (pvQueue = pvQueue.then(() => new Promise((res) => {
    const run = () => {
      let url = null;
      try { url = renderPreview(id); } catch (e) { console.warn('Board preview failed', id, e); }
      if (url) previewCache.set(id, url);
      previewWait.delete(id);
      res(url);
    };
    if (window.requestIdleCallback) requestIdleCallback(run, { timeout: 200 }); else setTimeout(run, 16);
  })));
  previewWait.set(id, p);
  return p;
}

function previewInto(id, el) {
  if (!el) return;
  const show = (url) => {
    if (!url) return;
    let img = el.querySelector('img.board-preview');
    if (!img) {
      img = document.createElement('img');
      img.className = 'board-preview';
      img.alt = '';
      Object.assign(img.style, { width: '100%', height: '100%', objectFit: 'cover', display: 'block', borderRadius: 'inherit' });
      el.appendChild(img);
    }
    img.src = url;
  };
  if (previewCache.has(id)) show(previewCache.get(id));
  else previewUrl(id).then(show);
}

// ---------- catalog ----------
DT.Cosmetics.register('board', THEMES.map((t) => ({
  ...(t.secret ? { secret: true, lootPool: t.lootPool, egg: false, tag: t.tag } : {}),
  id: t.id, name: t.name, price: t.price, rarity: t.rarity, desc: t.desc, icon: t.icon,
  ...(t.free ? { free: true } : {}),
  data: { tiles: t.tiles, fog: t.fog[0] },
  preview: (el) => previewInto(t.id, el),
})));

DT.Boards = { attach, ids: THEMES.map((t) => t.id), DEFAULT, wanted, previewUrl, preview: previewInto, testSignals: null };
