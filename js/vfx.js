// Effect primitives for signature moves (js/moves.js holds the per-move specs).
// Everything here is pooled: sprites live in two big typed-array point systems (one additive, one alpha-blended),
// meshes come from per-kind pools that toggle visibility, and lightning / whips use fixed-size ribbons.
// After warm-up a move allocates only its small closures; nothing is allocated per frame.
// Purely visual: reads world positions handed in by the renderer, never touches combat state.
import * as THREE from 'three';

// ---------- sprite atlas: 8 x 4 cells of 64 px, white shapes tinted per particle ----------
const CELL_NAMES = ['glow', 'spark', 'flame', 'smoke', 'bubble', 'feather', 'note', 'card',
  'leaf', 'petal', 'shard', 'streak', 'cross', 'gear', 'bat', 'fish',
  'square', 'needle', 'star', 'ring', 'seed', 'blade', 'bolt', 'paw',
  'drop', 'snow', 'skull', 'heart', 'hex', 'dot', 'ember', 'wisp'];
export const CELL = {};
CELL_NAMES.forEach((n, i) => { CELL[n] = i; });
// cells drawn along their x axis; they turn to face their direction of travel on screen
const ALIGNED = new Set(['streak', 'needle', 'blade', 'feather', 'bolt', 'fish', 'seed']);

function drawCell(g, name) {
  const W = 64, c = 32;
  g.save();
  g.translate(c, c);
  g.fillStyle = '#fff';
  g.strokeStyle = '#fff';
  const radial = (r, stops) => {
    const grd = g.createRadialGradient(0, 0, 0, 0, 0, r);
    for (const [k, a] of stops) grd.addColorStop(k, `rgba(255,255,255,${a})`);
    g.fillStyle = grd;
    g.beginPath(); g.arc(0, 0, r, 0, Math.PI * 2); g.fill();
  };
  const poly = (pts) => { g.beginPath(); g.moveTo(pts[0], pts[1]); for (let i = 2; i < pts.length; i += 2) g.lineTo(pts[i], pts[i + 1]); g.closePath(); g.fill(); };
  switch (name) {
    case 'glow': radial(30, [[0, 1], [0.3, 0.6], [1, 0]]); break;
    case 'spark':
      radial(12, [[0, 1], [1, 0]]);
      g.globalAlpha = 0.9;
      for (const r of [0, Math.PI / 2]) { g.save(); g.rotate(r); g.beginPath(); g.ellipse(0, 0, 29, 3, 0, 0, Math.PI * 2); g.fill(); g.restore(); }
      break;
    case 'flame': {
      const grd = g.createRadialGradient(0, 8, 2, 0, 4, 26);
      grd.addColorStop(0, 'rgba(255,255,255,1)'); grd.addColorStop(0.5, 'rgba(255,255,255,0.65)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
      g.fillStyle = grd;
      g.beginPath(); g.moveTo(0, -28); g.bezierCurveTo(14, -10, 22, 6, 18, 16); g.bezierCurveTo(14, 28, -14, 28, -18, 16); g.bezierCurveTo(-22, 6, -14, -10, 0, -28); g.fill();
      break;
    }
    case 'smoke':
      for (const [x, y, r] of [[-8, 4, 18], [9, 2, 17], [0, -8, 18], [2, 10, 15]]) {
        const grd = g.createRadialGradient(x, y, 0, x, y, r);
        grd.addColorStop(0, 'rgba(255,255,255,0.55)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
        g.fillStyle = grd; g.beginPath(); g.arc(x, y, r, 0, Math.PI * 2); g.fill();
      }
      break;
    case 'bubble':
      radial(26, [[0, 0.08], [0.75, 0.15], [0.92, 0.9], [1, 0]]);
      g.fillStyle = 'rgba(255,255,255,0.95)'; g.beginPath(); g.ellipse(-9, -10, 6, 4, -0.6, 0, Math.PI * 2); g.fill();
      break;
    case 'feather':
      g.beginPath(); g.ellipse(0, 0, 28, 9, 0, 0, Math.PI * 2); g.globalAlpha = 0.85; g.fill();
      g.globalAlpha = 1; g.lineWidth = 2; g.strokeStyle = 'rgba(255,255,255,1)'; g.beginPath(); g.moveTo(-30, 0); g.lineTo(26, 0); g.stroke();
      break;
    case 'note':
      g.beginPath(); g.ellipse(-8, 14, 11, 8, -0.4, 0, Math.PI * 2); g.fill();
      g.fillRect(1, -24, 5, 38);
      poly([1, -26, 20, -14, 20, -4, 6, -14]);
      break;
    case 'card':
      g.fillStyle = 'rgba(255,248,225,1)'; g.fillRect(-14, -26, 28, 52);
      g.strokeStyle = 'rgba(220,40,40,1)'; g.lineWidth = 3; g.strokeRect(-10, -22, 20, 44);
      g.fillStyle = 'rgba(220,30,30,1)'; g.fillRect(-2, -16, 4, 30); g.fillRect(-7, -8, 14, 3); g.fillRect(-7, 2, 14, 3);
      break;
    case 'leaf':
      g.beginPath(); g.moveTo(-28, 0); g.quadraticCurveTo(0, -20, 28, 0); g.quadraticCurveTo(0, 20, -28, 0); g.globalAlpha = 0.9; g.fill();
      g.globalAlpha = 0.5; g.fillStyle = '#000'; g.fillRect(-24, -1, 48, 2);
      break;
    case 'petal':
      g.beginPath(); g.moveTo(0, 26); g.bezierCurveTo(-22, 6, -14, -24, 0, -14); g.bezierCurveTo(14, -24, 22, 6, 0, 26); g.globalAlpha = 0.95; g.fill();
      break;
    case 'shard': poly([0, -29, 11, 0, 0, 29, -11, 0]); g.globalAlpha = 0.6; radial(14, [[0, 1], [1, 0]]); break;
    case 'streak': {
      const grd = g.createLinearGradient(-31, 0, 31, 0);
      grd.addColorStop(0, 'rgba(255,255,255,0)'); grd.addColorStop(0.7, 'rgba(255,255,255,0.9)'); grd.addColorStop(1, 'rgba(255,255,255,1)');
      g.fillStyle = grd; g.beginPath(); g.ellipse(0, 0, 31, 4, 0, 0, Math.PI * 2); g.fill();
      break;
    }
    case 'cross': radial(16, [[0, 0.8], [1, 0]]); g.fillRect(-4, -28, 8, 56); g.fillRect(-20, -14, 40, 8); break;
    case 'gear': {
      g.beginPath();
      for (let i = 0; i < 16; i++) { const a = (i / 16) * Math.PI * 2, r = i % 2 ? 22 : 29; g.lineTo(Math.cos(a) * r, Math.sin(a) * r); }
      g.closePath(); g.fill();
      g.globalCompositeOperation = 'destination-out'; g.beginPath(); g.arc(0, 0, 8, 0, Math.PI * 2); g.fill();
      break;
    }
    case 'bat': poly([0, -4, 8, -10, 14, -18, 30, -12, 22, -4, 26, 6, 14, 2, 6, 10, 0, 4, -6, 10, -14, 2, -26, 6, -22, -4, -30, -12, -14, -18, -8, -10]); break;
    case 'fish':
      g.beginPath(); g.ellipse(4, 0, 20, 10, 0, 0, Math.PI * 2); g.fill(); poly([-14, 0, -29, -12, -27, 12]);
      g.globalCompositeOperation = 'destination-out'; g.beginPath(); g.arc(15, -3, 3, 0, Math.PI * 2); g.fill();
      break;
    case 'square': g.globalAlpha = 0.5; g.fillRect(-18, -18, 36, 36); g.globalAlpha = 1; g.lineWidth = 4; g.strokeRect(-20, -20, 40, 40); break;
    case 'needle': { const grd = g.createLinearGradient(-31, 0, 31, 0); grd.addColorStop(0, 'rgba(255,255,255,0.1)'); grd.addColorStop(1, 'rgba(255,255,255,1)'); g.fillStyle = grd; poly([-30, -2, 22, -2, 31, 0, 22, 2, -30, 2]); break; }
    case 'star':
      g.beginPath();
      for (let i = 0; i < 10; i++) { const a = (i / 10) * Math.PI * 2 - Math.PI / 2, r = i % 2 ? 11 : 28; g.lineTo(Math.cos(a) * r, Math.sin(a) * r); }
      g.closePath(); g.fill();
      break;
    case 'ring': g.lineWidth = 5; g.beginPath(); g.arc(0, 0, 24, 0, Math.PI * 2); g.stroke(); g.globalAlpha = 0.4; g.lineWidth = 11; g.stroke(); break;
    case 'seed': g.beginPath(); g.ellipse(0, 0, 18, 9, 0, 0, Math.PI * 2); g.fill(); break;
    case 'blade': poly([-28, -3, -14, -5, 18, -6, 30, 0, 18, 5, -14, 4, -28, 3]); g.fillRect(-16, -10, 3, 20); break;
    case 'bolt': g.lineWidth = 5; g.lineJoin = 'miter'; g.beginPath(); g.moveTo(-30, -4); g.lineTo(-10, 6); g.lineTo(-2, -8); g.lineTo(12, 6); g.lineTo(30, -2); g.stroke(); break;
    case 'paw':
      g.beginPath(); g.ellipse(0, 9, 13, 11, 0, 0, Math.PI * 2); g.fill();
      for (const [x, y] of [[-15, -6], [-6, -16], [6, -16], [15, -6]]) { g.beginPath(); g.arc(x, y, 6, 0, Math.PI * 2); g.fill(); }
      break;
    case 'drop': g.beginPath(); g.moveTo(0, -26); g.bezierCurveTo(14, -6, 18, 6, 0, 22); g.bezierCurveTo(-18, 6, -14, -6, 0, -26); g.fill(); break;
    case 'snow':
      g.lineWidth = 4;
      for (let i = 0; i < 3; i++) { g.save(); g.rotate((i * Math.PI) / 3); g.beginPath(); g.moveTo(-27, 0); g.lineTo(27, 0); g.moveTo(16, 0); g.lineTo(22, -7); g.moveTo(16, 0); g.lineTo(22, 7); g.stroke(); g.restore(); }
      break;
    case 'skull':
      g.beginPath(); g.arc(0, -4, 20, 0, Math.PI * 2); g.fill(); g.fillRect(-11, 10, 22, 12);
      g.globalCompositeOperation = 'destination-out';
      g.beginPath(); g.arc(-8, -4, 6, 0, Math.PI * 2); g.arc(8, -4, 6, 0, Math.PI * 2); g.fill(); g.fillRect(-2, 14, 4, 8);
      break;
    case 'heart': g.beginPath(); g.moveTo(0, 24); g.bezierCurveTo(-30, 2, -18, -26, 0, -10); g.bezierCurveTo(18, -26, 30, 2, 0, 24); g.fill(); break;
    case 'hex':
      g.lineWidth = 4; g.beginPath();
      for (let i = 0; i < 6; i++) { const a = (i / 6) * Math.PI * 2; g.lineTo(Math.cos(a) * 24, Math.sin(a) * 24); }
      g.closePath(); g.stroke(); g.globalAlpha = 0.25; g.fill();
      break;
    case 'dot': radial(10, [[0, 1], [0.6, 1], [1, 0]]); break;
    case 'ember': radial(18, [[0, 1], [0.2, 0.9], [1, 0]]); break;
    case 'wisp': {
      const grd = g.createRadialGradient(0, 10, 0, 0, 6, 24);
      grd.addColorStop(0, 'rgba(255,255,255,0.9)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
      g.fillStyle = grd;
      g.beginPath(); g.moveTo(4, -30); g.bezierCurveTo(18, -12, 22, 10, 10, 22); g.bezierCurveTo(-2, 30, -22, 18, -16, 2); g.bezierCurveTo(-10, -10, 4, -12, 4, -30); g.fill();
      break;
    }
  }
  g.restore();
}

function canvasTexture(w, h, draw, srgb = true) {
  const c = document.createElement('canvas');
  c.width = w; c.height = h;
  draw(c.getContext('2d'), w, h);
  const t = new THREE.CanvasTexture(c);
  if (srgb) t.colorSpace = THREE.SRGBColorSpace;
  return t;
}

function atlasTexture() {
  const t = canvasTexture(512, 256, (g) => {
    CELL_NAMES.forEach((n, i) => {
      g.save();
      g.translate((i % 8) * 64, Math.floor(i / 8) * 64);
      g.beginPath(); g.rect(1, 1, 62, 62); g.clip();
      drawCell(g, n);
      g.restore();
    });
  });
  t.minFilter = THREE.LinearFilter;
  t.generateMipmaps = false;
  return t;
}

// big textures for meshes
const TEX = {};
function makeTextures() {
  if (TEX.glow) return;
  TEX.glow = canvasTexture(128, 128, (g) => {
    const grd = g.createRadialGradient(64, 64, 0, 64, 64, 64);
    grd.addColorStop(0, 'rgba(255,255,255,1)'); grd.addColorStop(0.25, 'rgba(255,255,255,0.75)'); grd.addColorStop(0.6, 'rgba(255,255,255,0.18)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 128, 128);
  });
  // slash crescent: bright outer edge fading inward, tapered ends
  TEX.crescent = canvasTexture(256, 256, (g) => {
    for (let i = 0; i < 40; i++) {
      const k = i / 39;
      g.strokeStyle = `rgba(255,255,255,${0.06 + 0.94 * k * k})`;
      g.lineWidth = 3;
      g.beginPath();
      g.ellipse(128, 128 + (1 - k) * 18, 118 - (1 - k) * 6, 100 - (1 - k) * 30, 0, Math.PI * 1.08, Math.PI * 1.92);
      g.stroke();
    }
    g.globalCompositeOperation = 'destination-in';
    const grd = g.createLinearGradient(0, 0, 256, 0);
    grd.addColorStop(0, 'rgba(255,255,255,0)'); grd.addColorStop(0.3, 'rgba(255,255,255,1)'); grd.addColorStop(0.7, 'rgba(255,255,255,1)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 256, 256);
  });
  // three claw marks
  TEX.claw = canvasTexture(256, 256, (g) => {
    for (const off of [-34, 0, 34]) {
      g.strokeStyle = 'rgba(255,255,255,1)';
      for (let w = 14; w > 1; w -= 3) {
        g.globalAlpha = 0.25 + (14 - w) / 18; g.lineWidth = w;
        g.beginPath(); g.moveTo(70 + off, 30); g.quadraticCurveTo(128 + off * 1.2, 128, 186 + off, 226); g.stroke();
      }
    }
    g.globalAlpha = 1;
    g.globalCompositeOperation = 'destination-in';
    const grd = g.createLinearGradient(0, 0, 0, 256);
    grd.addColorStop(0, 'rgba(255,255,255,0)'); grd.addColorStop(0.25, 'rgba(255,255,255,1)'); grd.addColorStop(0.8, 'rgba(255,255,255,1)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 256, 256);
  });
  TEX.swirl = canvasTexture(256, 256, (g) => {
    g.translate(128, 128);
    const grd = g.createRadialGradient(0, 0, 0, 0, 0, 128);
    grd.addColorStop(0, 'rgba(255,255,255,0.9)'); grd.addColorStop(0.2, 'rgba(255,255,255,0.15)'); grd.addColorStop(0.9, 'rgba(255,255,255,0.05)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(-128, -128, 256, 256);
    for (let arm = 0; arm < 5; arm++) {
      for (let i = 0; i < 70; i++) {
        const k = i / 70, a = arm * (Math.PI * 2 / 5) + k * 5.2, r = 10 + k * 112;
        g.fillStyle = `rgba(255,255,255,${0.9 * (1 - k)})`;
        g.beginPath(); g.arc(Math.cos(a) * r, Math.sin(a) * r, 2 + 9 * (1 - k) * k * 2, 0, Math.PI * 2); g.fill();
      }
    }
  });
  TEX.cross = canvasTexture(256, 256, (g) => {
    g.translate(128, 128);
    const glow = g.createRadialGradient(0, -20, 0, 0, -20, 120);
    glow.addColorStop(0, 'rgba(255,255,255,0.6)'); glow.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = glow; g.fillRect(-128, -128, 256, 256);
    for (let w = 22; w > 2; w -= 4) {
      g.fillStyle = `rgba(255,255,255,${0.25 + (22 - w) / 26})`;
      g.fillRect(-w / 2, -118, w, 236); g.fillRect(-80, -50 - w / 2, 160, w);
    }
  });
  TEX.shock = canvasTexture(256, 256, (g) => {
    const grd = g.createRadialGradient(128, 128, 0, 128, 128, 128);
    grd.addColorStop(0, 'rgba(255,255,255,0)'); grd.addColorStop(0.62, 'rgba(255,255,255,0.08)'); grd.addColorStop(0.86, 'rgba(255,255,255,1)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 256, 256);
  });
  TEX.ring = canvasTexture(256, 256, (g) => {
    const grd = g.createRadialGradient(128, 128, 0, 128, 128, 128);
    grd.addColorStop(0, 'rgba(255,255,255,0)'); grd.addColorStop(0.8, 'rgba(255,255,255,0)'); grd.addColorStop(0.9, 'rgba(255,255,255,1)'); grd.addColorStop(0.97, 'rgba(255,255,255,0.4)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 256, 256);
  });
  TEX.web = canvasTexture(256, 256, (g) => {
    g.translate(128, 128); g.strokeStyle = 'rgba(255,255,255,0.95)'; g.lineWidth = 3;
    for (let i = 0; i < 10; i++) { const a = (i / 10) * Math.PI * 2; g.beginPath(); g.moveTo(0, 0); g.lineTo(Math.cos(a) * 124, Math.sin(a) * 124); g.stroke(); }
    for (let r = 20; r < 124; r += 20) {
      g.beginPath();
      for (let i = 0; i <= 10; i++) { const a = (i / 10) * Math.PI * 2; const rr = r * (i % 2 ? 0.92 : 1); g.lineTo(Math.cos(a) * rr, Math.sin(a) * rr); }
      g.stroke();
    }
  });
  // vertical fade for light pillars (v = 0 at the bottom)
  TEX.pillar = canvasTexture(8, 128, (g) => {
    const grd = g.createLinearGradient(0, 128, 0, 0);
    grd.addColorStop(0, 'rgba(255,255,255,1)'); grd.addColorStop(0.5, 'rgba(255,255,255,0.5)'); grd.addColorStop(1, 'rgba(255,255,255,0)');
    g.fillStyle = grd; g.fillRect(0, 0, 8, 128);
  });
  // soft edges across a beam's circumference
  TEX.beam = canvasTexture(64, 8, (g) => {
    const grd = g.createLinearGradient(0, 0, 64, 0);
    for (let i = 0; i <= 8; i++) grd.addColorStop(i / 8, `rgba(255,255,255,${i % 2 ? 1 : 0.45})`);
    g.fillStyle = grd; g.fillRect(0, 0, 64, 8);
  });
  TEX.beam.wrapS = THREE.RepeatWrapping;
}

// ---------- point sprites ----------
const VERT = `
attribute vec4 acolor; attribute float asize; attribute float arot; attribute float acell;
uniform float uScale;
varying vec4 vColor; varying float vRot; varying float vCell;
void main() {
  vec4 mv = modelViewMatrix * vec4(position, 1.0);
  gl_Position = projectionMatrix * mv;
  gl_PointSize = min(asize * uScale / max(0.1, -mv.z), 256.0);
  vColor = acolor; vRot = arot; vCell = acell;
}`;
const FRAG = `
uniform sampler2D map;
varying vec4 vColor; varying float vRot; varying float vCell;
void main() {
  vec2 p = vec2(gl_PointCoord.x - 0.5, 0.5 - gl_PointCoord.y);
  float c = cos(vRot), s = sin(vRot);
  vec2 q = vec2(c * p.x + s * p.y, -s * p.x + c * p.y) + 0.5;
  if (q.x < 0.0 || q.y < 0.0 || q.x > 1.0 || q.y > 1.0) discard;
  float cx = mod(vCell, 8.0), cy = floor(vCell / 8.0);
  vec4 t = texture2D(map, vec2((cx + q.x) / 8.0, 1.0 - (cy + 1.0 - q.y) / 4.0));
  float a = t.a * vColor.a;
  if (a < 0.004) discard;
  gl_FragColor = vec4(vColor.rgb * t.rgb, a);
}`;

// particle behaviour presets: gravity (negative rises), drag, size growth over life, spin
export const PRE = {
  none: { grav: 0, drag: 0, grow: 1, spin: 0 },
  drift: { grav: 0, drag: 1.5, grow: 1.3, spin: 0 },
  flame: { grav: -2.2, drag: 1.8, grow: 0.35, spin: 0.5 },
  fireTrail: { grav: -1.2, drag: 2.5, grow: 0.3, spin: 1 },
  smoke: { grav: -0.6, drag: 1.5, grow: 2.4, spin: 0.8 },
  spark: { grav: 6, drag: 1.2, grow: 0.6, spin: 0 },
  debris: { grav: 12, drag: 0.4, grow: 1, spin: 4 },
  float: { grav: -1.4, drag: 1, grow: 1, spin: 1.5 },
  flutter: { grav: 1.2, drag: 2.5, grow: 1, spin: 3 },
  pop: { grav: 0, drag: 4, grow: 2.2, spin: 0 },
  shrink: { grav: 0, drag: 0, grow: 0.2, spin: 0 },
  swirl: { grav: -1, drag: 0.5, grow: 0.8, spin: 6 },
};

class Sprites {
  constructor(scene, max, additive, map, order) {
    this.max = max;
    this.n = 0;
    this.pos = new Float32Array(max * 3);
    this.col = new Float32Array(max * 4);
    this.size = new Float32Array(max);
    this.rot = new Float32Array(max);
    this.cell = new Float32Array(max);
    this.vel = new Float32Array(max * 3);
    this.life = new Float32Array(max);
    this.maxLife = new Float32Array(max);
    this.alpha0 = new Float32Array(max);
    this.size0 = new Float32Array(max);
    this.grow = new Float32Array(max);
    this.spin = new Float32Array(max);
    this.grav = new Float32Array(max);
    this.drag = new Float32Array(max);
    this.align = new Uint8Array(max);
    const geo = new THREE.BufferGeometry();
    const attr = (name, arr, size) => geo.setAttribute(name, new THREE.BufferAttribute(arr, size).setUsage(THREE.DynamicDrawUsage));
    attr('position', this.pos, 3); attr('acolor', this.col, 4); attr('asize', this.size, 1); attr('arot', this.rot, 1); attr('acell', this.cell, 1);
    geo.setDrawRange(0, 0);
    this.mat = new THREE.ShaderMaterial({
      uniforms: { map: { value: map }, uScale: { value: 600 } },
      vertexShader: VERT, fragmentShader: FRAG,
      transparent: true, depthWrite: false, blending: additive ? THREE.AdditiveBlending : THREE.NormalBlending,
    });
    this.points = new THREE.Points(geo, this.mat);
    this.points.frustumCulled = false;
    this.points.renderOrder = order;
    scene.add(this.points);
  }

  // color is a THREE.Color; cell an index from CELL; pre one of PRE
  add(x, y, z, vx, vy, vz, color, life, size, cell, pre = PRE.none, alpha = 1, rot = Math.random() * 6.283) {
    if (this.n >= this.max) return;
    const i = this.n++, i3 = i * 3, i4 = i * 4;
    this.pos[i3] = x; this.pos[i3 + 1] = y; this.pos[i3 + 2] = z;
    this.vel[i3] = vx; this.vel[i3 + 1] = vy; this.vel[i3 + 2] = vz;
    this.col[i4] = color.r; this.col[i4 + 1] = color.g; this.col[i4 + 2] = color.b; this.col[i4 + 3] = 0;
    this.life[i] = this.maxLife[i] = life;
    this.alpha0[i] = alpha;
    this.size[i] = this.size0[i] = size;
    this.cell[i] = cell;
    this.grow[i] = pre.grow;
    this.spin[i] = pre.spin * (Math.random() < 0.5 ? -1 : 1);
    this.grav[i] = pre.grav;
    this.drag[i] = pre.drag;
    this.align[i] = ALIGNED_IDX[cell];
    this.rot[i] = rot;
  }

  update(dt, vp, aspect) {
    const e = vp.elements;
    const P = this.pos, V = this.vel, Cc = this.col;
    let w = 0;
    for (let i = 0; i < this.n; i++) {
      const l = this.life[i] - dt;
      if (l <= 0) continue;
      const i3 = i * 3, w3 = w * 3, w4 = w * 4;
      const k = Math.max(0, 1 - this.drag[i] * dt);
      const vx = V[i3] * k, vy = V[i3 + 1] * k - this.grav[i] * dt, vz = V[i3 + 2] * k;
      const x = P[i3] + vx * dt, y = Math.max(0.02, P[i3 + 1] + vy * dt), z = P[i3 + 2] + vz * dt;
      P[w3] = x; P[w3 + 1] = y; P[w3 + 2] = z;
      V[w3] = vx; V[w3 + 1] = vy; V[w3 + 2] = vz;
      const ml = this.maxLife[i];
      const f = l / ml; // 1 at birth -> 0 at death
      const fade = f > 0.85 ? (1 - f) / 0.15 : f < 0.5 ? f / 0.5 : 1;
      Cc[w4] = Cc[i * 4]; Cc[w4 + 1] = Cc[i * 4 + 1]; Cc[w4 + 2] = Cc[i * 4 + 2];
      Cc[w4 + 3] = this.alpha0[i] * fade;
      const g = this.grow[i];
      this.size0[w] = this.size0[i];
      this.size[w] = this.size0[i] * (1 + (g - 1) * (1 - f));
      this.cell[w] = this.cell[i];
      const al = this.align[i];
      if (al) {
        // face the direction of travel on screen
        const cw0 = e[3] * x + e[7] * y + e[11] * z + e[15];
        const qx = x + vx * 0.05, qy = y + vy * 0.05, qz = z + vz * 0.05;
        const cw1 = e[3] * qx + e[7] * qy + e[11] * qz + e[15];
        const sx = (e[0] * qx + e[4] * qy + e[8] * qz + e[12]) / cw1 - (e[0] * x + e[4] * y + e[8] * z + e[12]) / cw0;
        const sy = (e[1] * qx + e[5] * qy + e[9] * qz + e[13]) / cw1 - (e[1] * x + e[5] * y + e[9] * z + e[13]) / cw0;
        if (sx * sx + sy * sy > 1e-10) this.rot[w] = Math.atan2(sy, sx * aspect);
        else this.rot[w] = this.rot[i];
      } else this.rot[w] = this.rot[i] + this.spin[i] * dt;
      this.align[w] = al;
      this.life[w] = l; this.maxLife[w] = ml; this.alpha0[w] = this.alpha0[i];
      this.grow[w] = g; this.spin[w] = this.spin[i]; this.grav[w] = this.grav[i]; this.drag[w] = this.drag[i];
      w++;
    }
    this.n = w;
    const geo = this.points.geometry;
    geo.setDrawRange(0, w);
    if (w > 0) {
      for (const [name, size] of ATTRS) {
        const a = geo.attributes[name];
        a.clearUpdateRanges();
        a.addUpdateRange(0, w * size);
        a.needsUpdate = true;
      }
    }
  }

  clear() { this.n = 0; this.points.geometry.setDrawRange(0, 0); }
}
const ATTRS = [['position', 3], ['acolor', 4], ['asize', 1], ['arot', 1]];
const ALIGNED_IDX = new Uint8Array(CELL_NAMES.length);
CELL_NAMES.forEach((n, i) => { ALIGNED_IDX[i] = ALIGNED.has(n) ? 1 : 0; });
// acell changes only with the particle order, but compaction moves it, so it is uploaded too
ATTRS.push(['acell', 1]);

// ---------- camera-facing ribbon for lightning, whips and streaks ----------
const RIB_N = 24;
class Ribbon {
  constructor(scene) {
    this.pts = [];
    for (let i = 0; i < RIB_N; i++) this.pts.push(new THREE.Vector3());
    this.count = 0;
    this.width = 0.1;
    this.taper = 1;
    const geo = new THREE.BufferGeometry();
    this.posArr = new Float32Array(RIB_N * 2 * 3);
    this.colArr = new Float32Array(RIB_N * 2 * 3);
    geo.setAttribute('position', new THREE.BufferAttribute(this.posArr, 3).setUsage(THREE.DynamicDrawUsage));
    geo.setAttribute('color', new THREE.BufferAttribute(this.colArr, 3).setUsage(THREE.DynamicDrawUsage));
    const idx = [];
    for (let i = 0; i < RIB_N - 1; i++) { const a = i * 2; idx.push(a, a + 1, a + 2, a + 1, a + 3, a + 2); }
    geo.setIndex(idx);
    this.mat = new THREE.MeshBasicMaterial({ vertexColors: true, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide });
    this.mesh = new THREE.Mesh(geo, this.mat);
    this.mesh.frustumCulled = false;
    this.mesh.renderOrder = 7;
    this.color = new THREE.Color();
    scene.add(this.mesh);
  }

  // fill this.pts[0..count) first, then call build
  build(cam, bright = 1) {
    const n = this.count, P = this.posArr, C = this.colArr, pts = this.pts;
    const cx = cam.position.x, cy = cam.position.y, cz = cam.position.z;
    for (let i = 0; i < RIB_N; i++) {
      const j = Math.min(i, n - 1);
      const p = pts[j];
      const a = pts[Math.max(0, j - 1)], b = pts[Math.min(n - 1, j + 1)];
      let tx = b.x - a.x, ty = b.y - a.y, tz = b.z - a.z;
      const vx = cx - p.x, vy = cy - p.y, vz = cz - p.z;
      // side = tangent x view
      let sx = ty * vz - tz * vy, sy = tz * vx - tx * vz, sz = tx * vy - ty * vx;
      const sl = Math.hypot(sx, sy, sz) || 1;
      const k = n > 1 ? j / (n - 1) : 0;
      const w = this.width * (1 - this.taper * 0.85 * Math.abs(k * 2 - 1) ** 3) * 0.5;
      sx = (sx / sl) * w; sy = (sy / sl) * w; sz = (sz / sl) * w;
      const o = i * 6;
      P[o] = p.x + sx; P[o + 1] = p.y + sy; P[o + 2] = p.z + sz;
      P[o + 3] = p.x - sx; P[o + 4] = p.y - sy; P[o + 5] = p.z - sz;
      const f = bright * (i < n ? 1 : 0);
      C[o] = C[o + 3] = this.color.r * f; C[o + 1] = C[o + 4] = this.color.g * f; C[o + 2] = C[o + 5] = this.color.b * f;
    }
    const g = this.mesh.geometry;
    g.attributes.position.needsUpdate = true;
    g.attributes.color.needsUpdate = true;
    g.setDrawRange(0, Math.max(0, n - 1) * 6);
  }
}

// ---------- mesh pools ----------
const addMat = (map) => new THREE.MeshBasicMaterial({ map: map || null, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide });
const GEO = {};
function makeGeos() {
  if (GEO.sphere) return;
  GEO.sphere = new THREE.SphereGeometry(1, 18, 12);
  GEO.beam = new THREE.CylinderGeometry(1, 1, 1, 18, 1, true).rotateX(Math.PI / 2); // axis +z, centered
  GEO.cone = new THREE.ConeGeometry(1, 1, 8).rotateX(Math.PI / 2); // tip toward +z
  GEO.spike = new THREE.ConeGeometry(1, 1, 6).translate(0, 0.5, 0); // base on the ground, tip up
  GEO.ring = new THREE.RingGeometry(0.72, 1, 48).rotateX(-Math.PI / 2);
  GEO.flat = new THREE.PlaneGeometry(2, 2).rotateX(-Math.PI / 2); // lies on the ground
  GEO.plane = new THREE.PlaneGeometry(2, 2); // stands up, faces +z
  GEO.torus = new THREE.TorusGeometry(1, 0.08, 8, 40);
  GEO.tri = new THREE.RingGeometry(0.78, 1, 3);
  GEO.pillar = new THREE.CylinderGeometry(1, 1, 1, 20, 1, true).translate(0, 0.5, 0);
  GEO.body = new THREE.SphereGeometry(1, 12, 8);
}

// ---------- instanced quads and balls, drawn fresh every frame (one draw call per kind) ----------
// Short-lived glows, ground rings, slash arcs and cores are pushed each frame by the tasks that own them.
// Additive blending, so fading is done by darkening the instance color.
const _m4 = new THREE.Matrix4(), _p = new THREE.Vector3(), _q = new THREE.Quaternion(), _q2 = new THREE.Quaternion(), _s = new THREE.Vector3();
const _zAxis = new THREE.Vector3(0, 0, 1);
const Q_FLAT = new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(1, 0, 0), -Math.PI / 2);
export const FACE_CAM = 0, FACE_FLAT = 1;
const _e = new THREE.Euler();
class Batch {
  constructor(scene, geo, map, max, material) {
    this.max = max;
    this.n = 0;
    this.mat = material || new THREE.MeshBasicMaterial({ map: map || null, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide });
    this.mesh = new THREE.InstancedMesh(geo, this.mat, max);
    this.mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage);
    this.mesh.instanceColor = new THREE.InstancedBufferAttribute(new Float32Array(max * 3), 3).setUsage(THREE.DynamicDrawUsage);
    this.mesh.count = 0;
    this.mesh.frustumCulled = false;
    this.mesh.renderOrder = material ? 0 : 7;
    scene.add(this.mesh);
  }
  pushEuler(x, y, z, sx, sy, sz, rx, ry, rz, color, alpha) {
    if (this.n >= this.max) return;
    _q.setFromEuler(_e.set(rx, ry, rz));
    _m4.compose(_p.set(x, y, z), _q, _s.set(sx, sy, sz));
    _m4.toArray(this.mesh.instanceMatrix.array, this.n * 16);
    const c = this.mesh.instanceColor.array, o = this.n * 3;
    c[o] = color.r * alpha; c[o + 1] = color.g * alpha; c[o + 2] = color.b * alpha;
    this.n++;
  }
  // face: FACE_CAM (billboard, rot spins it on screen), FACE_FLAT (on the ground, rot spins it), or a yaw (standing, facing that way)
  push(x, y, z, sx, sy, sz, color, alpha, rot, face, cam) {
    if (this.n >= this.max || alpha <= 0.002) return;
    if (face === FACE_CAM) _q.copy(cam.quaternion);
    else if (face === FACE_FLAT) _q.copy(Q_FLAT);
    else if (face == null) _q.identity();
    else _q.setFromAxisAngle(_YAXIS, face);
    if (rot) _q.multiply(_q2.setFromAxisAngle(_zAxis, rot));
    _m4.compose(_p.set(x, y, z), _q, _s.set(sx, sy, sz));
    _m4.toArray(this.mesh.instanceMatrix.array, this.n * 16);
    const c = this.mesh.instanceColor.array, o = this.n * 3;
    c[o] = color.r * alpha; c[o + 1] = color.g * alpha; c[o + 2] = color.b * alpha;
    this.n++;
  }
  begin() { this.n = 0; }
  end() {
    this.mesh.count = this.n;
    if (this.n) {
      this.mesh.instanceMatrix.clearUpdateRanges(); this.mesh.instanceMatrix.addUpdateRange(0, this.n * 16); this.mesh.instanceMatrix.needsUpdate = true;
      this.mesh.instanceColor.clearUpdateRanges(); this.mesh.instanceColor.addUpdateRange(0, this.n * 3); this.mesh.instanceColor.needsUpdate = true;
    }
  }
}
const _YAXIS = new THREE.Vector3(0, 1, 0);
const SPIKE_LOOK = { ice: ['#cdf3ff', '#2a7fb0'], rock: ['#8a7458', '#1a1208'], thorn: ['#4f9a3a', '#0d2a08'], dark: ['#4a2a6a', '#3a0a5a'], metal: ['#b8c0cc', '#202833'] };
const WHITE_C = new THREE.Color(1, 1, 1);

const KINDS = {
  sphere: () => new THREE.Mesh(GEO.sphere, addMat()),
  beam: () => new THREE.Mesh(GEO.beam, addMat(TEX.beam)),
  cone: () => new THREE.Mesh(GEO.cone, addMat()),
  ring: () => new THREE.Mesh(GEO.ring, addMat()),
  shock: () => new THREE.Mesh(GEO.flat, addMat(TEX.shock)),
  swirl: () => new THREE.Mesh(GEO.flat, addMat(TEX.swirl)),
  swirlUp: () => new THREE.Mesh(GEO.plane, addMat(TEX.swirl)),
  crescent: () => new THREE.Mesh(GEO.plane, addMat(TEX.crescent)),
  claw: () => new THREE.Mesh(GEO.plane, addMat(TEX.claw)),
  cross: () => new THREE.Mesh(GEO.plane, addMat(TEX.cross)),
  crossFlat: () => new THREE.Mesh(GEO.flat, addMat(TEX.cross)),
  web: () => new THREE.Mesh(GEO.plane, addMat(TEX.web)),
  torus: () => new THREE.Mesh(GEO.torus, addMat()),
  tri: () => new THREE.Mesh(GEO.tri, addMat()),
  pillar: () => new THREE.Mesh(GEO.pillar, addMat(TEX.pillar)),
  glow: () => new THREE.Sprite(new THREE.SpriteMaterial({ map: TEX.glow, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending })),
  // lit, solid pieces: ice and rock spikes, organic missiles, iron balls
  spike: () => new THREE.Mesh(GEO.spike, new THREE.MeshStandardMaterial({ roughness: 0.25, metalness: 0.1, transparent: true, flatShading: true })),
  body: () => new THREE.Mesh(GEO.body, new THREE.MeshStandardMaterial({ roughness: 0.5, metalness: 0.3, transparent: true })),
};

const rnd = (a, b) => a + Math.random() * (b - a);
const BLACK = new THREE.Color(0, 0, 0);

// ---------- the effect engine ----------
export class MoveFx {
  // host: { scene, camera, canvas, shake(amp, dur) }
  constructor(host) {
    makeTextures();
    makeGeos();
    this.host = host;
    this.scene = host.scene;
    this.cam = host.camera;
    const atlas = atlasTexture();
    // glow quads are 1 unit wide (scale = width); the others 2 units (scale = radius)
    this.batches = {
      glow: new Batch(this.scene, new THREE.PlaneGeometry(1, 1), TEX.glow, 768),
      shock: new Batch(this.scene, GEO.plane, TEX.shock, 256),
      ring: new Batch(this.scene, GEO.plane, TEX.ring, 256),
      crescent: new Batch(this.scene, GEO.plane, TEX.crescent, 256),
      claw: new Batch(this.scene, GEO.plane, TEX.claw, 128),
      ball: new Batch(this.scene, GEO.sphere, null, 256),
      pillar: new Batch(this.scene, GEO.pillar, TEX.pillar, 128),
    };
    // lit spikes (ice, rock, thorn, dark, metal), one batch per look
    for (const [k, [c, e]] of Object.entries(SPIKE_LOOK)) {
      this.batches[`spike_${k}`] = new Batch(this.scene, GEO.spike, null, 256,
        new THREE.MeshStandardMaterial({ color: c, emissive: e, roughness: 0.25, metalness: 0.1, flatShading: true, transparent: k === 'ice', opacity: k === 'ice' ? 0.88 : 1 }));
    }
    this.batchList = Object.values(this.batches);
    this.alpha = new Sprites(this.scene, 2500, false, atlas, 6);
    this.add = new Sprites(this.scene, 9000, true, atlas, 8);
    this.pools = {};
    this.ribbons = [];
    this.tasks = [];
    this.taskFree = [];
    this.now = 0;
    this.vp = new THREE.Matrix4();
    this.tmpV = new THREE.Vector3();
    this.paused = false;
  }

  // compile every effect material once up front, so the first cast of a kind doesn't stall on a shader compile
  warm(renderer) {
    const ms = Object.keys(KINDS).map((k) => this.mesh(k));
    const r = this.ribbon();
    try { renderer.compile(this.scene, this.cam); } catch (e) { /* older three.js: compiles on first use instead */ }
    for (const m of ms) this.free(m);
    this.freeRibbon(r);
  }

  // ---- pools ----
  mesh(kind) {
    const pool = this.pools[kind] || (this.pools[kind] = []);
    let m = pool.pop();
    if (!m) {
      m = KINDS[kind]();
      m.userData.kind = kind;
      m.frustumCulled = false;
      m.renderOrder = kind === 'spike' || kind === 'body' ? 0 : 7;
      this.scene.add(m);
    }
    m.visible = true;
    m.position.set(0, 0, 0);
    m.rotation.set(0, 0, 0);
    m.rotation.order = 'XYZ';
    m.scale.set(1, 1, 1);
    m.material.opacity = 1;
    m.material.color.setRGB(1, 1, 1);
    if (m.material.emissive) m.material.emissive.copy(BLACK);
    if (m.material.rotation != null) m.material.rotation = 0;
    return m;
  }
  free(m) { m.visible = false; this.pools[m.userData.kind].push(m); }
  ribbon() {
    let r = this.ribbons.find((x) => !x.busy);
    if (!r) { r = new Ribbon(this.scene); this.ribbons.push(r); }
    r.busy = true; r.mesh.visible = true; r.count = 0; r.taper = 1;
    return r;
  }
  freeRibbon(r) { r.busy = false; r.mesh.visible = false; }

  // ---- timeline ----
  // fn(k, dt) runs every frame for dur seconds after delay (k goes 0 -> 1, the last call has k = 1); end() after
  run(delay, dur, fn, end) {
    const t = this.taskFree.pop() || {};
    t.t0 = this.now + Math.max(0, delay); t.dur = Math.max(0, dur); t.fn = fn; t.end = end || null; t.started = false;
    this.tasks.push(t);
    return t;
  }
  later(delay, fn) { return this.run(delay, 0, null, fn); }

  update(dt) {
    if (this.paused) return;
    this.step(dt);
  }

  step(dt) {
    this.now += dt;
    for (const b of this.batchList) b.begin();
    const tasks = this.tasks;
    let w = 0;
    for (let i = 0; i < tasks.length; i++) {
      const t = tasks[i];
      if (this.now < t.t0) { tasks[w++] = t; continue; }
      const k = t.dur > 0 ? Math.min(1, (this.now - t.t0) / t.dur) : 1;
      if (t.fn) t.fn(k, dt);
      if (k >= 1) { if (t.end) t.end(); t.fn = t.end = null; this.taskFree.push(t); }
      else tasks[w++] = t;
    }
    tasks.length = w;
    for (const b of this.batchList) b.end();
    const cam = this.cam;
    cam.updateMatrixWorld();
    this.vp.multiplyMatrices(cam.projectionMatrix, cam.matrixWorldInverse);
    const h = (this.host.canvas && this.host.canvas.height) || 900;
    const scale = (h * cam.projectionMatrix.elements[5]) / 2;
    this.add.mat.uniforms.uScale.value = scale;
    this.alpha.mat.uniforms.uScale.value = scale;
    this.add.update(dt, this.vp, cam.aspect);
    this.alpha.update(dt, this.vp, cam.aspect);
  }

  // advance a paused timeline (tests take screenshots of exact moments this way)
  advance(sec, frame = 1 / 60) { for (let t = 0; t < sec - 1e-6; t += frame) this.step(frame); }

  clear() {
    for (const t of this.tasks) { t.fn = null; if (t.end) { try { t.end(); } catch (e) { /* ignore */ } } t.end = null; this.taskFree.push(t); }
    this.tasks.length = 0;
    this.add.clear(); this.alpha.clear();
    for (const b of this.batchList) { b.begin(); b.end(); }
  }

  // immediate-mode pieces, call every frame from a task: billboard / flat / standing quad, and a glowing ball
  // (glows and cores are toned down a little: additive light piles up to white fast when many moves overlap)
  quad(kind, x, y, z, sx, sy, color, alpha, rot = 0, face = FACE_CAM) { this.batches[kind].push(x, y, z, sx, sy, 1, color, kind === 'glow' ? alpha * 0.75 : alpha, rot, face, this.cam); }
  ball(x, y, z, r, color, alpha = 1) { this.batches.ball.push(x, y, z, r, r, r, color, alpha * 0.8, 0, null, this.cam); }
  spike(kind, x, z, r, h, rx, ry, rz) { if (h > 0.01) (this.batches[`spike_${kind}`] || this.batches.spike_ice).pushEuler(x, 0, z, r, h, r, rx, ry, rz, WHITE_C, 1); }

  shake(amp, dur) { if (amp > 0 && this.host.shake) this.host.shake(amp, dur); }

  // ---- small building blocks used by the primitives ----
  burst(sys, x, y, z, n, speed, ca, cb, life, size, cell, pre = PRE.drift, up = 0, alpha = 1) {
    for (let i = 0; i < n; i++) {
      const th = Math.random() * 6.283, ph = Math.acos(rnd(-1, 1));
      const s = speed * rnd(0.35, 1);
      sys.add(x, y, z, Math.sin(ph) * Math.cos(th) * s, Math.abs(Math.cos(ph)) * s * 0.7 + up, Math.sin(ph) * Math.sin(th) * s,
        Math.random() < 0.5 ? ca : cb, life * rnd(0.6, 1.2), size * rnd(0.7, 1.3), cell, pre, alpha);
    }
  }

  // flat ring burst on the ground
  ringBurst(sys, x, y, z, n, speed, color, life, size, cell, pre = PRE.drift) {
    for (let i = 0; i < n; i++) {
      const th = (i / n) * 6.283 + rnd(-0.1, 0.1);
      sys.add(x, y, z, Math.cos(th) * speed, rnd(0, 0.4), Math.sin(th) * speed, color, life, size, cell, pre);
    }
  }

  flash(delay, x, y, z, color, size, dur = 0.25) {
    this.run(delay, dur, (k) => { const s = size * (0.5 + k * 0.8); this.quad('glow', x, y, z, s, s, color, 0.8 * (1 - k)); });
  }

  groundRing(delay, x, z, color, r, dur = 0.5, kind = 'shock', y = 0.16) {
    const b = kind === 'ring' ? 'ring' : 'shock';
    this.run(delay, dur, (k) => {
      const e = 1 - (1 - k) * (1 - k), s = 0.15 + e * r;
      this.quad(b, x, y, z, s, s, color, (b === 'ring' ? 0.6 : 0.55) * (1 - k), 0, FACE_FLAT);
    });
  }

  pillar(delay, x, z, color, r, h, dur = 0.6) {
    this.run(delay, dur, (k) => {
      const w = r * (k < 0.15 ? k / 0.15 : 1 - (k - 0.15) * 0.7);
      this.batches.pillar.pushEuler(x, 0, z, w, h * (k < 0.15 ? 0.5 + k / 0.3 : 1), w, 0, 0, 0, color, (1 - k) * 0.5);
    });
  }

  // jagged line between two points into a ribbon; jitter regenerates a few times for flicker
  bolt(delay, ax, ay, az, bx, by, bz, color, dur = 0.25, width = 0.08, jag = 0.35, segs = 10) {
    const st = { r: null, next: 0 };
    this.run(delay, dur, (k) => {
      if (!st.r) { st.r = this.ribbon(); st.r.color.copy(color); st.r.taper = 0.4; }
      const r = st.r;
      if (k >= st.next) {
        st.next = k + 0.3;
        r.count = Math.min(RIB_N, segs + 1);
        for (let i = 0; i < r.count; i++) {
          const t = i / (r.count - 1);
          const j = i === 0 || i === r.count - 1 ? 0 : jag;
          r.pts[i].set(ax + (bx - ax) * t + rnd(-j, j), ay + (by - ay) * t + rnd(-j, j) * 0.6, az + (bz - az) * t + rnd(-j, j));
        }
      }
      r.width = width * (k < 0.2 ? 1.6 : 1);
      r.build(this.cam, k < 0.15 ? 1.6 : 1 - k * 0.8);
    }, () => { if (st.r) this.freeRibbon(st.r); });
  }

  // generic projectile from A to B; head is drawn by drawHead(x, y, z, k, dt), onHit at arrival
  fly(delay, dur, ax, ay, az, bx, by, bz, arc, drawHead, onHit, side = 0) {
    // side bends the path sideways (missile curls); perpendicular to the flight direction
    const dx = bx - ax, dz = bz - az, len = Math.hypot(dx, dz) || 1;
    const px = -dz / len, pz = dx / len;
    this.run(delay, dur, (k, dt) => {
      const s = Math.sin(Math.PI * k);
      drawHead(ax + dx * k + px * side * s, ay + (by - ay) * k + arc * s, az + dz * k + pz * side * s, k, dt);
    }, onHit);
  }
}

export { rnd };
