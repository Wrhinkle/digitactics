// Move delivery primitives, impacts and basic attacks, composed from js/vfx.js building blocks.
// Each primitive is fn(fx, c, o, d): fx = MoveFx engine, c = cast context (below), o = the step's options from the
// spec, d = delay in seconds from now when the step starts. The spec format is documented in js/moves.js.
//
// Cast context c:
//   A {x,y,z}  launch point (caster mouth / chest)     P {x,z}  caster on the ground   h  caster model height
//   T [{x,y,z}] targets (enemies hit, or allies for heals; can be empty)
//   C {x,z}    area center (burst center, else first target, else caster)    R  area radius in world units
//   S          size multiplier from form stage (and spec.scale)               stage  0..2
//   ca, cb     main and highlight THREE.Color                                yaw  angle caster -> first target
//   from {x,z} where a dash started (leap start)                              spec   the move spec
import * as THREE from 'three';
import { MoveFx, CELL, PRE, rnd } from './vfx.js';
import { specFor, basicFor, PALETTE, STAGE_SCALE } from './moves.js';

const DT = window.DT;
const TAU = Math.PI * 2;
const WHITE = new THREE.Color('#ffffff');
const GREY = new THREE.Color('#8a8f99');
const ROCK = new THREE.Color('#8a7458');
const ROCK2 = new THREE.Color('#5e5040');
const SMOKE = new THREE.Color('#6a6a72');
const DRAIN_A = new THREE.Color('#ff3a6a');
const DRAIN_B = new THREE.Color('#ffb3c6');
const STUN = new THREE.Color('#ffe94a');
const colorCache = new Map();
const hexColor = (h) => { let c = colorCache.get(h); if (!c) { c = new THREE.Color(h); colorCache.set(h, c); } return c; };
// option color: 'a' / 'b' / 'w' or a hex string
const col = (c, v, dflt) => (v == null ? dflt || c.ca : v === 'a' ? c.ca : v === 'b' ? c.cb : v === 'w' ? WHITE : hexColor(v));
// the sprite system that suits a cell: colored paper and leaves need normal blending
const ALPHA_CELLS = new Set([CELL.card, CELL.leaf, CELL.smoke]);
const sysFor = (fx, cell) => (ALPHA_CELLS.has(cell) ? fx.alpha : fx.add);
const cellOf = (name, dflt = 'glow') => CELL[name] != null ? CELL[name] : CELL[dflt];
const pick = (a, b) => (Math.random() < 0.5 ? a : b);
const dist = (a, b) => Math.hypot(b.x - a.x, b.z - a.z);

// ---------- impacts ----------
export const IMPACTS = {
  blast(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 1.2 * s, 0.22);
    fx.flash(0, x, y, z, ca, 2.4 * s, 0.4);
    fx.burst(fx.add, x, y, z, 26 * s, 3.2 * s, ca, cb, 0.55, 0.5 * s, CELL.flame, PRE.flame, 1.2);
    fx.burst(fx.add, x, y, z, 14 * s, 6 * s, cb, WHITE, 0.4, 0.14, CELL.ember, PRE.spark, 2);
    fx.burst(fx.alpha, x, y, z, 6 * s, 1.2, SMOKE, SMOKE, 1.0, 0.7 * s, CELL.smoke, PRE.smoke, 0.8, 0.45);
    fx.groundRing(0, x, z, ca, 1.6 * s, 0.45);
  },
  frost(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, ca, 2 * s, 0.3);
    fx.burst(fx.add, x, y, z, 18 * s, 4.5 * s, ca, cb, 0.5, 0.3 * s, CELL.shard, PRE.spark, 1);
    fx.burst(fx.add, x, y, z, 14 * s, 2 * s, WHITE, cb, 0.9, 0.22, CELL.snow, PRE.flutter, 0.8);
    for (let i = 0; i < 3; i++) spikeAt(fx, 0.02 * i, x + rnd(-0.35, 0.35), z + rnd(-0.35, 0.35), 0.16 * s, (0.6 + Math.random() * 0.5) * s, 'ice', 0.7, rnd(-0.4, 0.4));
    fx.groundRing(0, x, z, cb, 1.4 * s, 0.45);
  },
  zap(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 2 * s, 0.2);
    fx.burst(fx.add, x, y, z, 22 * s, 6 * s, ca, cb, 0.35, 0.28, CELL.spark, PRE.spark, 1.5);
    for (let i = 0; i < 2; i++) {
      const a = Math.random() * TAU, r = 0.9 * s;
      fx.bolt(i * 0.04, x, y, z, x + Math.cos(a) * r, 0.15, z + Math.sin(a) * r, ca, 0.18, 0.05, 0.18, 5);
    }
    fx.groundRing(0, x, z, ca, 1.2 * s, 0.35, 'ring');
  },
  splash(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 1.6 * s, 0.22);
    fx.burst(fx.add, x, y, z, 22 * s, 4 * s, ca, cb, 0.6, 0.22 * s, CELL.drop, PRE.debris, 2.5);
    fx.burst(fx.add, x, y, z, 10 * s, 1.5, cb, WHITE, 0.8, 0.25 * s, CELL.bubble, PRE.float, 0.5);
    fx.groundRing(0, x, z, ca, 1.5 * s, 0.5);
  },
  holy(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, WHITE, 2.2 * s, 0.3);
    fx.pillar(0, x, z, ca, 0.45 * s, 5, 0.5);
    fx.burst(fx.add, x, 0.3, z, 20 * s, 1.6, ca, WHITE, 1.0, 0.3, CELL.star, PRE.float, 0.6);
    fx.burst(fx.add, x, y, z, 14, 4 * s, cb, WHITE, 0.4, 0.3, CELL.spark, PRE.drift);
    fx.groundRing(0, x, z, ca, 1.5 * s, 0.5, 'ring');
  },
  dark(fx, x, y, z, s, ca, cb) {
    // implode, then burst
    for (let i = 0; i < 18 * s; i++) {
      const a = Math.random() * TAU, r = rnd(0.8, 1.4) * s;
      fx.add.add(x + Math.cos(a) * r, y + rnd(-0.4, 0.5), z + Math.sin(a) * r, -Math.cos(a) * r * 5, 0, -Math.sin(a) * r * 5, pick(ca, cb), 0.2, 0.35, CELL.wisp, PRE.none);
    }
    fx.later(0.16, () => {
      fx.flash(0, x, y, z, ca, 2.4 * s, 0.35);
      fx.burst(fx.add, x, y, z, 22 * s, 3.5 * s, ca, cb, 0.6, 0.45 * s, CELL.wisp, PRE.smoke, 1);
      fx.groundRing(0, x, z, cb, 1.6 * s, 0.5);
    });
  },
  poison(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, ca, 1.5 * s, 0.25);
    fx.burst(fx.add, x, y, z, 14 * s, 1.5, ca, cb, 0.9, 0.28 * s, CELL.bubble, PRE.float, 0.8);
    fx.burst(fx.alpha, x, y, z, 6 * s, 1, ca, ca, 0.9, 0.6 * s, CELL.smoke, PRE.smoke, 0.4, 0.4);
  },
  leaf(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 1.4 * s, 0.22);
    fx.burst(fx.alpha, x, y, z, 14 * s, 3.5 * s, ca, cb, 0.9, 0.3 * s, CELL.leaf, PRE.flutter, 1.2);
    fx.burst(fx.add, x, y, z, 8, 2.5, cb, WHITE, 0.5, 0.25, CELL.petal, PRE.flutter, 1);
  },
  metal(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 1.5 * s, 0.18);
    fx.burst(fx.add, x, y, z, 16 * s, 7 * s, ca, WHITE, 0.35, 0.3, CELL.streak, PRE.spark, 2);
    fx.burst(fx.alpha, x, y, z, 4, 1, SMOKE, SMOKE, 0.7, 0.5 * s, CELL.smoke, PRE.smoke, 0.5, 0.4);
  },
  hit(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, WHITE, 1.4 * s, 0.16);
    fx.burst(fx.add, x, y, z, 12 * s, 6 * s, ca, cb, 0.3, 0.32, CELL.streak, PRE.drift, 0.5);
    fx.add.add(x, y, z, 0, 0, 0, ca, 0.22, 1.1 * s, CELL.ring, PRE.pop);
  },
  wind(fx, x, y, z, s, ca, cb) {
    fx.flash(0, x, y, z, cb, 1.3 * s, 0.2);
    for (let i = 0; i < 16 * s; i++) {
      const a = Math.random() * TAU, r = 0.4;
      fx.add.add(x + Math.cos(a) * r, y + rnd(-0.5, 0.6), z + Math.sin(a) * r, -Math.sin(a) * 5, rnd(0.5, 2), Math.cos(a) * 5, pick(ca, WHITE), 0.35, 0.4, CELL.streak, PRE.drift);
    }
    fx.groundRing(0, x, z, ca, 1.3 * s, 0.4, 'ring');
  },
  none() {},
};

function impact(fx, c, kind, x, z, d, s = 1, y = 0.8) {
  const fn = IMPACTS[kind || c.spec.impact || 'hit'] || IMPACTS.hit;
  fx.later(d, () => {
    fn(fx, x, y, z, s * c.S, c.ca, c.cb);
    if (!c.shook && c.spec.shake) { c.shook = true; fx.shake(c.spec.shake * (0.7 + c.stage * 0.3), 0.3 + c.spec.shake); }
    if (c.type === 'stun') dizzy(fx, x, z, c.stunTime || 1);
    if (c.type === 'drain' && c.P) flow(fx, x, z, c.P.x, c.P.z);
  });
}

// stars circling a stunned target's head
function dizzy(fx, x, z, dur) {
  fx.run(0, dur, () => {
    const t = fx.now * 7;
    for (let i = 0; i < 3; i++) {
      const a = t + (i / 3) * TAU;
      fx.add.add(x + Math.cos(a) * 0.4, 2.0, z + Math.sin(a) * 0.4, 0, 0, 0, STUN, 0.05, 0.22, CELL.star, PRE.none, 1, 0);
    }
  });
}

// life drained back to the caster
function flow(fx, x, z, tx, tz) {
  const dx = tx - x, dz = tz - z, d = Math.hypot(dx, dz) || 1;
  for (let i = 0; i < 16; i++) {
    const s = rnd(3, 4.5);
    fx.add.add(x + rnd(-0.2, 0.2), rnd(0.6, 1.1), z + rnd(-0.2, 0.2), (dx / d) * s, rnd(0, 0.6), (dz / d) * s, pick(DRAIN_A, DRAIN_B), Math.min(0.9, d / s), 0.3, CELL.heart, PRE.none);
  }
}

// lit spike (ice / rock / thorn / dark / metal) that juts out of the ground, holds, then sinks
function spikeAt(fx, d, x, z, r, h, kind = 'ice', hold = 0.6, tilt = 0) {
  const T = 0.14 + hold + 0.25, ry = Math.random() * TAU;
  fx.run(d, T, (k) => {
    const t = k * T;
    const g = t < 0.14 ? t / 0.14 : t < 0.14 + hold ? 1 : 1 - (t - 0.14 - hold) / 0.25;
    fx.spike(kind, x, z, r * (0.6 + 0.4 * g), h * g, tilt, ry, tilt * 0.5);
  });
}

// ---------- heads and trails for things in flight ----------
function emitTrail(fx, kind, x, y, z, dt, s, ca, cb) {
  const n = Math.max(1, Math.round(dt * 60));
  switch (kind) {
    case 'flame':
      for (let i = 0; i < n * 2; i++) fx.add.add(x + rnd(-0.08, 0.08) * s, y + rnd(-0.08, 0.08) * s, z + rnd(-0.08, 0.08) * s, rnd(-0.4, 0.4), rnd(0, 0.6), rnd(-0.4, 0.4), Math.random() < 0.75 ? ca : cb, rnd(0.25, 0.4), 0.38 * s, CELL.flame, PRE.fireTrail, 0.55);
      break;
    case 'smoke':
      for (let i = 0; i < n; i++) fx.alpha.add(x, y, z, rnd(-0.3, 0.3), rnd(0, 0.4), rnd(-0.3, 0.3), SMOKE, rnd(0.5, 0.8), 0.32 * s, CELL.smoke, PRE.smoke, 0.5);
      break;
    case 'spark':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-1.5, 1.5), rnd(-1.5, 1.5), rnd(-1.5, 1.5), pick(ca, cb), 0.22, 0.2 * s, CELL.spark, PRE.drift);
      break;
    case 'bubble':
      for (let i = 0; i < n; i++) fx.add.add(x + rnd(-0.1, 0.1), y, z + rnd(-0.1, 0.1), rnd(-0.3, 0.3), rnd(0.2, 0.8), rnd(-0.3, 0.3), pick(ca, cb), 0.5, 0.14 * s, CELL.bubble, PRE.float);
      break;
    case 'leaf':
      for (let i = 0; i < n; i++) fx.alpha.add(x, y, z, rnd(-0.8, 0.8), rnd(-0.2, 0.8), rnd(-0.8, 0.8), pick(ca, cb), 0.6, 0.22 * s, CELL.leaf, PRE.flutter);
      break;
    case 'petal':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-0.8, 0.8), rnd(-0.2, 0.8), rnd(-0.8, 0.8), pick(ca, cb), 0.6, 0.22 * s, CELL.petal, PRE.flutter);
      break;
    case 'dark':
      for (let i = 0; i < n * 2; i++) fx.add.add(x, y, z, rnd(-0.5, 0.5), rnd(-0.2, 0.7), rnd(-0.5, 0.5), pick(ca, cb), rnd(0.3, 0.5), 0.4 * s, CELL.wisp, PRE.smoke);
      break;
    case 'ice':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-0.6, 0.6), rnd(-0.6, 0.6), rnd(-0.6, 0.6), pick(ca, WHITE), 0.4, 0.18 * s, pick(CELL.snow, CELL.shard), PRE.drift);
      break;
    case 'holy':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-0.6, 0.6), rnd(-0.3, 0.6), rnd(-0.6, 0.6), pick(cb, WHITE), 0.45, 0.2 * s, CELL.star, PRE.drift);
      break;
    case 'wind':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-0.4, 0.4), rnd(-0.4, 0.4), rnd(-0.4, 0.4), pick(ca, WHITE), 0.2, 0.3 * s, CELL.glow, PRE.pop, 0.5);
      break;
    case 'data':
      for (let i = 0; i < n; i++) fx.add.add(x + rnd(-0.12, 0.12), y + rnd(-0.12, 0.12), z + rnd(-0.12, 0.12), 0, rnd(0, 0.4), 0, pick(ca, cb), 0.35, 0.16 * s, CELL.square, PRE.shrink, 1, 0);
      break;
    case 'zap':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-2, 2), rnd(-2, 2), rnd(-2, 2), pick(ca, WHITE), 0.12, 0.25 * s, CELL.bolt, PRE.drift);
      break;
    case 'note':
      for (let i = 0; i < n; i++) fx.add.add(x, y, z, rnd(-0.5, 0.5), rnd(0.2, 0.8), rnd(-0.5, 0.5), pick(ca, cb), 0.4, 0.2 * s, CELL.note, PRE.drift, 1, 0);
      break;
    default: break;
  }
}

// draws a glowing core (ball + glow quad) or a sprite head; returns { draw(x,y,z), done() }
function makeHead(fx, o, s, ca, cb, c) {
  const cell = o.head ? cellOf(o.head) : -1;
  return {
    draw(x, y, z) {
      const r = (o.size || 0.25) * s;
      if (cell >= 0) {
        const sys = sysFor(fx, cell);
        sys.add(x, y, z, 0, 0, 0, o.headColor ? col(c, o.headColor) : sys === fx.alpha ? WHITE : cb, 0.05, r * 2.2, cell, PRE.none, 1, o.spin ? fx.now * o.spin : 0);
        if (o.halo !== false) fx.quad('glow', x, y, z, r * 3, r * 3, ca, 0.6);
        return;
      }
      fx.ball(x, y, z, r * 0.55, cb, 0.9);
      fx.quad('glow', x, y, z, r * 4, r * 4, ca, 0.85, fx.now * 3);
    },
    done() {},
  };
}

// targets for a step: 'targets' (default), 'center' or 'self'
function spots(c, o) {
  const at = o.to || 'targets';
  if (at === 'center' || !c.T.length) return at === 'self' ? [{ x: c.P.x, y: 0.8, z: c.P.z }] : [{ x: c.C.x, y: 0.8, z: c.C.z }];
  if (at === 'self') return [{ x: c.P.x, y: 0.8, z: c.P.z }];
  return c.T;
}

// launch point for 'mouth' | 'chest' | 'sky' | 'hands' | 'back'
function launch(c, from, i = 0) {
  const A = c.A;
  const side = i % 2 ? 1 : -1;
  const px = Math.cos(c.yaw), pz = -Math.sin(c.yaw); // right-hand side, perpendicular to facing
  switch (from) {
    case 'chest': return { x: A.x + px * side * 0.25 * c.S, y: A.y * 0.75, z: A.z + pz * side * 0.25 * c.S };
    case 'back': return { x: A.x + px * side * 0.4 * c.S - Math.sin(c.yaw) * 0.3, y: A.y + 0.5 * c.S, z: A.z + pz * side * 0.4 * c.S - Math.cos(c.yaw) * 0.3 };
    case 'hands': return { x: A.x + px * side * 0.35 * c.S, y: A.y * 0.85, z: A.z + pz * side * 0.35 * c.S };
    case 'sky': return { x: A.x + rnd(-0.8, 0.8), y: A.y + 1.6 + rnd(0, 0.8), z: A.z + rnd(-0.8, 0.8) };
    default: return A;
  }
}

// ---------- delivery primitives ----------
export const PRIMS = {
  // projectile: fireball, bubble, iron ball, dart; o: size, dur|speed, arc, trail, head (cell), count, gap, from, side, impact
  orb(fx, c, o, d) {
    const n = o.count || 1;
    const tg = spots(c, o);
    for (let t = 0; t < tg.length; t++) {
      for (let i = 0; i < n; i++) {
        const B = tg[t], A = launch(c, o.from, i);
        const L = dist(A, B);
        const dur = o.dur || Math.min(0.6, Math.max(0.15, L / (o.speed || 14)));
        const head = makeHead(fx, o, c.S, col(c, o.color), col(c, o.core, c.cb), c);
        const side = (o.side || 0) * (i % 2 ? 1 : -1) * (n > 1 ? 1 : 0) + (o.wobble ? rnd(-o.wobble, o.wobble) : 0);
        const tx = B.x + (n > 1 ? rnd(-0.25, 0.25) : 0), tz = B.z + (n > 1 ? rnd(-0.25, 0.25) : 0);
        const delay = d + i * (o.gap || 0.07) + t * 0.03;
        fx.fly(delay, dur, A.x, A.y, A.z, tx, B.y, tz, o.arc != null ? o.arc : 0.3, (x, y, z, k, dt) => {
          head.draw(x, y, z);
          if (o.trail) emitTrail(fx, o.trail, x, y, z, dt, (o.size || 0.25) * 4 * c.S, col(c, o.color), c.cb);
          if (o.wings) for (const sgn of [-1, 1]) {
            const px = Math.cos(c.yaw) * sgn, pz = -Math.sin(c.yaw) * sgn;
            fx.add.add(x, y, z, px * 3, rnd(0.5, 1.5), pz * 3, pick(c.ca, c.cb), 0.25, 0.5 * c.S, CELL.flame, PRE.fireTrail);
          }
        }, () => head.done(), side);
        if (o.muzzle !== false) fx.flash(delay, A.x, A.y, A.z, c.cb, 0.8 * c.S * ((o.size || 0.25) / 0.25), 0.15);
        impact(fx, c, o.impact, tx, tz, delay + dur, o.impactSize || ((o.size || 0.25) / 0.3) ** 0.5, B.y);
      }
    }
  },

  // breath or stream cone from the mouth; o: dur, width, cell, size, speed, rate, color, mix (second cell)
  breath(fx, c, o, d) {
    const tg = spots(c, o);
    const B = tg[0];
    const A = launch(c, o.from || 'mouth');
    const L = Math.max(1, Math.hypot(B.x - A.x, B.z - A.z));
    const speed = (o.speed || 9) * Math.sqrt(c.S);
    const life = (L / speed) * 1.15;
    const dur = o.dur || 0.45;
    const cell = cellOf(o.cell, 'flame'), mix = o.mix ? cellOf(o.mix) : cell;
    const ca = col(c, o.color), cb = c.cb;
    const ux = (B.x - A.x) / L, uz = (B.z - A.z) / L, uy = (B.y - A.y) / L;
    const W = (o.width || 0.5) * c.S;
    const rate = (o.rate || 240) * 0.7 * Math.sqrt(c.S);
    const sz = (o.size || 0.4) * c.S;
    const sys = sysFor(fx, cell);
    fx.run(d, dur, (k, dt) => {
      const n = Math.round(rate * dt) + 1;
      for (let i = 0; i < n; i++) {
        const sx = rnd(-W, W), sy = rnd(-W, W) * 0.6;
        const v = speed * rnd(0.85, 1.15);
        sys.add(A.x, A.y, A.z, ux * v + (-uz) * sx * v / L, uy * v + sy * v / L, uz * v + ux * sx * v / L,
          Math.random() < 0.3 ? cb : ca, life * rnd(0.8, 1.1), sz * rnd(0.6, 1.1), Math.random() < 0.25 ? mix : cell, PRE.breath);
      }
      if (Math.random() < 0.5) fx.add.add(A.x, A.y, A.z, 0, 0, 0, cb, 0.06, 0.7 * c.S, CELL.glow, PRE.none, 0.9, 0);
    });
    for (const t of tg) impact(fx, c, o.impact, t.x, t.z, d + Math.min(dur * 0.6, L / speed), o.impactSize || 1, t.y);
  },

  // beam / laser; o: width, dur, charge, twin, from, glyph ('x'|'tri'), color, core, overshoot, spiral
  beam(fx, c, o, d) {
    const tg = spots(c, o);
    const dur = o.dur || 0.45;
    const charge = o.charge || 0;
    const W = (o.width || 0.16) * Math.sqrt(c.S);
    const ca = col(c, o.color), cb = col(c, o.core, WHITE);
    tg.forEach((B, ti) => {
      const nBeams = o.twin ? 2 : 1;
      for (let i = 0; i < nBeams; i++) {
        const A = o.twin ? launch(c, o.from || 'chest', i) : launch(c, o.from);
        const dx = B.x - A.x, dy = B.y - A.y, dz = B.z - A.z, L = Math.hypot(dx, dy, dz) || 1;
        const over = o.overshoot != null ? o.overshoot : 0.6;
        const E = { x: B.x + (dx / L) * over, y: B.y + (dy / L) * over, z: B.z + (dz / L) * over };
        const len = L + over;
        const st = { outer: null, inner: null, glow: null };
        const t0 = d + ti * 0.04;
        if (charge > 0) chargeGlow(fx, A, ca, cb, W * 5, t0, charge);
        fx.run(t0 + charge, dur, (k, dt) => {
          if (!st.outer) {
            st.outer = fx.mesh('beam'); st.inner = fx.mesh('beam');
            st.outer.material.color.copy(ca); st.inner.material.color.copy(cb);
            for (const m of [st.outer, st.inner]) { m.position.set((A.x + E.x) / 2, (A.y + E.y) / 2, (A.z + E.z) / 2); m.lookAt(E.x, E.y, E.z); }
          }
          const g = k < 0.12 ? k / 0.12 : k > 0.7 ? (1 - k) / 0.3 : 1;
          const wob = 1 + Math.sin(fx.now * 60) * 0.08;
          st.outer.scale.set(W * g * wob, W * g * wob, len);
          st.inner.scale.set(W * 0.3 * g, W * 0.3 * g, len);
          st.outer.material.opacity = 0.55 * g;
          st.inner.material.opacity = 0.85 * g;
          fx.quad('glow', A.x, A.y, A.z, W * 7 * g, W * 7 * g, ca, 0.7 * g);
          // sparks shed along the beam
          const n = Math.round(dt * 70 * c.S);
          for (let j = 0; j < n; j++) {
            const t = Math.random();
            fx.add.add(A.x + dx * t, A.y + dy * t, A.z + dz * t, rnd(-1.5, 1.5), rnd(-1, 1.5), rnd(-1.5, 1.5), pick(ca, cb), 0.25, W * 1.1, o.spiral ? CELL.ring : CELL.spark, PRE.drift, 0.8);
          }
          if (o.spiral) {
            const a = fx.now * 30;
            const px = -dz / L, pz = dx / L;
            const t = (fx.now * 3) % 1;
            fx.add.add(A.x + dx * t + px * Math.cos(a) * W * 2, A.y + dy * t + Math.sin(a) * W * 2, A.z + dz * t + pz * Math.cos(a) * W * 2, 0, 0, 0, cb, 0.2, W * 2.5, CELL.glow, PRE.none);
          }
        }, () => { if (st.outer) { fx.free(st.outer); fx.free(st.inner); } });
        if (o.glyph) glyph(fx, o.glyph, A, c.yaw, ca, W * 7, t0, charge + dur);
        if (i === 0) impact(fx, c, o.impact, B.x, B.z, t0 + charge + 0.05, o.impactSize || 1, B.y);
        if (i === 0 && dur > 0.4) impact(fx, c, o.impact, B.x, B.z, t0 + charge + dur * 0.6, (o.impactSize || 1) * 0.7, B.y);
      }
    });
  },

  // missiles that launch out and curl onto the targets; o: count, size, dur, from, organic, curve, color
  missiles(fx, c, o, d) {
    const n = o.count || 2;
    const tg = spots(c, o);
    for (let i = 0; i < n; i++) {
      const B = tg[i % tg.length];
      const A = launch(c, o.from || 'chest', i);
      const dur = o.dur || 0.55;
      const side = (o.curve != null ? o.curve : 1.2) * (i % 2 ? 1 : -1) * rnd(0.7, 1.1);
      const tx = B.x + rnd(-0.2, 0.2), tz = B.z + rnd(-0.2, 0.2);
      const size = (o.size || 0.14) * c.S;
      const st = { m: null, lx: A.x, ly: A.y, lz: A.z };
      const delay = d + i * (o.gap || 0.08);
      fx.fly(delay, dur, A.x, A.y, A.z, tx, B.y, tz, o.arc != null ? o.arc : 1.6, (x, y, z, k, dt) => {
        if (!st.m) {
          st.m = fx.mesh('body');
          st.m.material.color.copy(hexColor(o.organic ? '#d8c6aa' : '#a7b0bc'));
          st.m.material.emissive.copy(hexColor(o.organic ? '#3a1810' : '#101418'));
        }
        st.m.position.set(x, y, z);
        st.m.scale.set(size, size, size * 2.8);
        st.m.lookAt(x + (x - st.lx) * 10, y + (y - st.ly) * 10, z + (z - st.lz) * 10);
        // exhaust behind
        const bx = x - (x - st.lx) * 2, by = y - (y - st.ly) * 2, bz = z - (z - st.lz) * 2;
        st.lx = x; st.ly = y; st.lz = z;
        emitTrail(fx, 'flame', bx, by, bz, dt, size * 2.2, c.ca, c.cb);
        if (o.smoke !== false) emitTrail(fx, 'smoke', bx, by, bz, dt, size * 4, c.ca, c.cb);
      }, () => { if (st.m) fx.free(st.m); }, side);
      fx.flash(delay, A.x, A.y, A.z, c.cb, 0.7 * c.S, 0.15);
      impact(fx, c, o.impact || 'blast', tx, tz, delay + dur, o.impactSize || 1, B.y);
    }
  },

  // lightning; o: count, width, from ('sky'|'caster'), height, area (random strikes over the area), color
  bolt(fx, c, o, d) {
    const ca = col(c, o.color), cb = c.cb;
    const W = (o.width || 0.09) * c.S;
    const tg = spots(c, o);
    if (o.from === 'caster') {
      tg.forEach((B, i) => {
        const A = launch(c, o.launch || 'mouth');
        fx.bolt(d + i * 0.03, A.x, A.y, A.z, B.x, B.y, B.z, ca, o.dur || 0.3, W, 0.35, 14);
        fx.bolt(d + i * 0.03 + 0.04, A.x, A.y, A.z, B.x, B.y, B.z, cb, (o.dur || 0.3) * 0.8, W * 0.5, 0.25, 14);
        fx.flash(d, A.x, A.y, A.z, cb, 0.9 * c.S, 0.2);
        impact(fx, c, o.impact || 'zap', B.x, B.z, d + 0.03, o.impactSize || 1, B.y);
      });
      return;
    }
    const H = o.height || 7;
    const strikes = [];
    for (const B of tg) strikes.push(B);
    if (o.area) for (let i = 0; i < (o.area | 0); i++) { const a = Math.random() * TAU, r = Math.sqrt(Math.random()) * c.R; strikes.push({ x: c.C.x + Math.cos(a) * r, y: 0.2, z: c.C.z + Math.sin(a) * r }); }
    strikes.forEach((B, i) => {
      const n = o.count || 2;
      const t0 = d + i * 0.05;
      for (let j = 0; j < n; j++) fx.bolt(t0 + j * 0.05, B.x + rnd(-0.6, 0.6), H, B.z + rnd(-0.6, 0.6), B.x, 0.1, B.z, j % 2 ? cb : ca, 0.3, W * (j ? 0.6 : 1), 0.45, 16);
      fx.pillar(t0, B.x, B.z, ca, 0.25 * c.S, H, 0.25);
      impact(fx, c, o.impact || 'zap', B.x, B.z, t0 + 0.02, (o.impactSize || 1) * (i < tg.length ? 1 : 0.6), 0.3);
    });
  },

  // slash / claw arcs at the targets; o: count, size, claw, spread, gap, color, vertical, to
  slash(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 2;
    const ca = col(c, o.color);
    tg.forEach((B, ti) => {
      for (let i = 0; i < n; i++) {
        const base = o.vertical ? Math.PI / 2 : 0.5;
        const ang = base + (i % 2 ? -1 : 1) * (o.spread != null ? o.spread : 0.9) + rnd(-0.3, 0.3);
        slashMesh(fx, d + ti * 0.03 + i * (o.gap || 0.08), B.x + rnd(-0.15, 0.15), (B.y || 0.8) + 0.1, B.z + rnd(-0.15, 0.15), ca, (o.size || 1) * c.S, ang, o.claw ? 'claw' : 'crescent', o.dur || 0.22);
        if (i === 0) impact(fx, c, o.impact || 'hit', B.x, B.z, d + ti * 0.03 + 0.04, o.impactSize || 0.8, B.y);
      }
    });
  },

  // body dash with afterimages, then slashes; o: ghosts, slashes, claw, dur, color, size
  dash(fx, c, o, d) {
    const B = spots(c, o)[0];
    const F = c.from || c.P;
    const dur = o.dur || 0.2;
    const n = o.ghosts || 5;
    const ca = col(c, o.color), h = c.h;
    for (let i = 0; i < n; i++) {
      const k = i / Math.max(1, n - 1);
      const x = F.x + (B.x - F.x) * k * 0.85, z = F.z + (B.z - F.z) * k * 0.85;
      const gc = i % 2 ? c.cb : ca, gy = h * 0.5 + Math.sin(Math.PI * k) * 0.8;
      fx.run(d + k * dur, 0.35, (kk) => fx.quad('glow', x, gy, z, h * 0.65 * (1 + kk * 0.3), h * 1.1, gc, 0.7 * (1 - kk)));
    }
    // streak along the path
    const st = { r: null };
    fx.run(d, dur + 0.2, (k) => {
      if (!st.r) { st.r = fx.ribbon(); st.r.color.copy(ca); st.r.taper = 1; }
      const r = st.r, reach = Math.min(1, k * (dur + 0.2) / dur), tail = Math.max(0, (k - 0.5) * 2);
      r.count = 12;
      for (let i = 0; i < 12; i++) {
        const t = tail + (reach - tail) * (i / 11);
        r.pts[i].set(F.x + (B.x - F.x) * t, 0.7 + Math.sin(Math.PI * t) * 0.8, F.z + (B.z - F.z) * t);
      }
      r.width = 0.5 * c.S;
      r.build(fx.cam, 1 - k * 0.7);
    }, () => { if (st.r) fx.freeRibbon(st.r); });
    for (let i = 0; i < 24; i++) {
      const k = Math.random();
      fx.add.add(F.x + (B.x - F.x) * k, rnd(0.3, 1.3), F.z + (B.z - F.z) * k, rnd(-0.5, 0.5), rnd(0, 0.6), rnd(-0.5, 0.5), pick(ca, c.cb), rnd(0.25, 0.5), 0.3, CELL.spark, PRE.drift);
    }
    if (o.slashes !== 0) PRIMS.slash(fx, c, { count: o.slashes || 2, claw: o.claw, size: o.size || 1.2, color: o.color, impact: o.impact, spread: o.spread, to: o.to }, d + dur);
  },

  // ground eruption under the targets or across the area; o: cell, count, height, kind ('flame'|'rock'|'ice'|'thorn'|'water'|'dark'), to
  erupt(fx, c, o, d) {
    const kind = o.kind || 'flame';
    const pts = [];
    if (o.to === 'center' || o.area) {
      const n = o.count || 5;
      pts.push({ x: c.C.x, z: c.C.z });
      for (let i = 1; i < n; i++) { const a = (i / (n - 1)) * TAU + rnd(-0.3, 0.3), r = c.R * rnd(0.45, 0.85); pts.push({ x: c.C.x + Math.cos(a) * r, z: c.C.z + Math.sin(a) * r }); }
    } else for (const t of spots(c, o)) pts.push(t);
    const ca = col(c, o.color), cb = c.cb;
    const H = (o.height || 2.2) * c.S;
    pts.forEach((p, i) => {
      const t0 = d + i * 0.06;
      fx.groundRing(t0, p.x, p.z, ca, 0.9 * c.S, 0.25);
      if (kind === 'ice' || kind === 'rock' || kind === 'thorn' || kind === 'metal') {
        for (let j = 0; j < 4; j++) spikeAt(fx, t0 + 0.08 + j * 0.03, p.x + rnd(-0.35, 0.35), p.z + rnd(-0.35, 0.35), 0.2 * c.S, H * rnd(0.45, 0.8), kind, 0.45, rnd(-0.35, 0.35));
        fx.later(t0 + 0.08, () => fx.burst(kind === 'ice' ? fx.add : fx.alpha, p.x, 0.3, p.z, 12, 4, kind === 'ice' ? ca : ROCK, kind === 'ice' ? WHITE : ROCK2, 0.7, 0.22, kind === 'ice' ? CELL.shard : kind === 'thorn' ? CELL.leaf : CELL.square, PRE.debris, 3));
      } else {
        const cell = kind === 'water' ? CELL.drop : kind === 'dark' ? CELL.wisp : CELL.flame;
        fx.pillar(t0 + 0.08, p.x, p.z, ca, 0.35 * c.S, H * 1.3, 0.45);
        fx.run(t0 + 0.08, 0.3, (k, dt) => {
          const m = Math.round(dt * 200 * c.S);
          for (let j = 0; j < m; j++) fx.add.add(p.x + rnd(-0.25, 0.25), 0.1, p.z + rnd(-0.25, 0.25), rnd(-0.6, 0.6), H * rnd(2, 3.2), rnd(-0.6, 0.6), pick(ca, cb), rnd(0.35, 0.55), 0.5 * c.S, cell, kind === 'water' ? PRE.debris : PRE.flame);
        });
      }
      impact(fx, c, o.impact, p.x, p.z, t0 + 0.1, (o.impactSize || 0.8) * (i ? 0.7 : 1), 0.5);
    });
  },

  // meteor from the sky onto the area; o: size, count, height, dur, color
  meteor(fx, c, o, d) {
    const n = o.count || 1;
    for (let i = 0; i < n; i++) {
      const a = Math.random() * TAU, r = i ? c.R * rnd(0.3, 0.8) : 0;
      const B = { x: c.C.x + Math.cos(a) * r, y: 0.3, z: c.C.z + Math.sin(a) * r };
      const H = o.height || 9;
      const A = { x: B.x - 3.5, y: H, z: B.z - 2.5 };
      const dur = o.dur || 0.5;
      const head = makeHead(fx, { size: (o.size || 0.6) * (i ? 0.6 : 1) }, c.S, col(c, o.color), c.cb, c);
      const t0 = d + i * 0.12;
      fx.fly(t0, dur, A.x, A.y, A.z, B.x, B.y, B.z, 0, (x, y, z, k, dt) => {
        head.draw(x, y, z);
        emitTrail(fx, o.trail || 'flame', x, y, z, dt, (o.size || 0.6) * 3 * c.S, col(c, o.color), c.cb);
        emitTrail(fx, 'smoke', x, y + 0.3, z, dt, (o.size || 0.6) * 3 * c.S);
      }, () => head.done());
      impact(fx, c, o.impact || 'blast', B.x, B.z, t0 + dur, (o.impactSize || 1.6) * (i ? 0.6 : 1), 0.4);
      fx.later(t0 + dur, () => {
        fx.burst(fx.alpha, B.x, 0.3, B.z, 20, 5, ROCK, ROCK2, 0.9, 0.2, CELL.square, PRE.debris, 4);
        fx.groundRing(0, B.x, B.z, col(c, o.color), c.R * 1.4 + 1, 0.6);
      });
    }
  },

  // energy sphere that grows above the caster, then launches; o: size, grow, from, trail, color, to
  sphere(fx, c, o, d) {
    const tg = spots(c, o);
    const B = tg[0];
    const A0 = o.from === 'mouth' ? c.A : o.from === 'hands' ? { x: c.A.x + Math.sin(c.yaw) * 0.4, y: c.A.y, z: c.A.z + Math.cos(c.yaw) * 0.4 } : { x: c.P.x, y: c.h + 0.9 * c.S, z: c.P.z };
    const grow = o.grow != null ? o.grow : 0.35;
    const size = (o.size || 0.55) * c.S;
    const ca = col(c, o.color), cb = col(c, o.core, c.cb);
    fx.run(d, grow, (k, dt) => {
      const r = size * (0.2 + 0.8 * k);
      fx.ball(A0.x, A0.y, A0.z, r * 0.6, cb, 0.9);
      fx.quad('glow', A0.x, A0.y, A0.z, r * 3.6, r * 3.6, ca, 0.8, fx.now * 4);
      const n = Math.round(dt * 100);
      for (let i = 0; i < n; i++) {
        const a = Math.random() * TAU, b = rnd(-1, 1), R = r * 4;
        const x = Math.cos(a) * Math.sqrt(1 - b * b) * R, y = b * R, z = Math.sin(a) * Math.sqrt(1 - b * b) * R;
        fx.add.add(A0.x + x, A0.y + y, A0.z + z, -x * 5, -y * 5, -z * 5, pick(ca, cb), 0.2, 0.25 * c.S, o.cell ? cellOf(o.cell) : CELL.spark, PRE.none);
      }
    });
    const L = Math.hypot(B.x - A0.x, B.z - A0.z);
    const dur = o.dur || Math.min(0.55, Math.max(0.2, L / 12));
    fx.fly(d + grow, dur, A0.x, A0.y, A0.z, B.x, B.y, B.z, 0.3, (x, y, z, k, dt) => {
      fx.ball(x, y, z, size * 0.6, cb, 0.9);
      fx.quad('glow', x, y, z, size * 3.6, size * 3.6, ca, 0.8, fx.now * 4);
      emitTrail(fx, o.trail || 'spark', x, y, z, dt, size * 2, ca, cb);
    });
    for (const t of tg) impact(fx, c, o.impact, t.x, t.z, d + grow + dur, o.impactSize || 1.4, t.y);
  },

  // sword wave: a standing crescent that travels along the ground; o: width, dur, color, count
  wave(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 1;
    const ca = col(c, o.color);
    tg.forEach((B, ti) => {
      for (let i = 0; i < n; i++) {
        const A = c.P;
        const ang = Math.atan2(B.x - A.x, B.z - A.z);
        const L = Math.hypot(B.x - A.x, B.z - A.z) + 1.2;
        const dur = o.dur || Math.min(0.45, L / 14);
        const W = (o.width || 1.4) * Math.sqrt(c.S);
        const st = { m: null, g: null };
        const t0 = d + i * 0.1 + ti * 0.03;
        fx.run(t0, dur, (k, dt) => {
          if (!st.m) {
            st.m = fx.mesh('crescent'); st.m.material.color.copy(ca);
            st.g = fx.mesh('crescent'); st.g.material.color.copy(WHITE);
          }
          const x = A.x + Math.sin(ang) * L * k, z = A.z + Math.cos(ang) * L * k;
          for (const m of [st.m, st.g]) {
            m.position.set(x, 0.9 * c.S, z);
            m.rotation.order = 'YXZ';
            m.rotation.set(0, ang, (i % 2 ? -1 : 1) * 0.5 + Math.PI / 2 + (o.flat ? Math.PI / 2 : 0));
          }
          st.m.scale.setScalar(W * (0.8 + k * 0.4));
          st.g.scale.setScalar(W * (0.65 + k * 0.35));
          st.m.material.opacity = k > 0.8 ? (1 - k) * 5 : 1;
          st.g.material.opacity = st.m.material.opacity * 0.35;
          const m = Math.round(dt * 80);
          for (let j = 0; j < m; j++) fx.add.add(x + rnd(-0.3, 0.3) * W, rnd(0.2, 1.6) * c.S, z + rnd(-0.3, 0.3) * W, 0, rnd(0, 0.5), 0, pick(ca, WHITE), 0.3, 0.25, CELL.spark, PRE.drift);
        }, () => { if (st.m) { fx.free(st.m); fx.free(st.g); } });
        impact(fx, c, o.impact || 'hit', B.x, B.z, t0 + dur * ((L - 1.2) / L), o.impactSize || 1.1, B.y);
      }
    });
  },

  // arrows of light; o: count, rain (fall from the sky), size, color, cell
  arrows(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 8;
    const ca = col(c, o.color), cell = cellOf(o.cell, 'streak');
    for (let i = 0; i < n; i++) {
      const B = tg[i % tg.length];
      const tx = B.x + rnd(-0.5, 0.5), tz = B.z + rnd(-0.5, 0.5);
      const A = o.rain ? { x: tx - 1, y: 6, z: tz - 0.6 } : launch(c, 'sky');
      const L = Math.hypot(tx - A.x, 0.2 - A.y, tz - A.z);
      const life = L / 22;
      const t0 = d + i * (o.gap || 0.035);
      fx.later(t0, () => {
        fx.add.add(A.x, A.y, A.z, (tx - A.x) / life, (0.2 - A.y) / life, (tz - A.z) / life, ca, life, (o.size || 0.7) * c.S, cell, PRE.none);
        fx.add.add(A.x, A.y, A.z, (tx - A.x) / life, (0.2 - A.y) / life, (tz - A.z) / life, WHITE, life, (o.size || 0.7) * 0.6 * c.S, cell, PRE.none);
      });
      fx.later(t0 + life, () => {
        fx.burst(fx.add, tx, 0.25, tz, 6, 2.5, ca, WHITE, 0.3, 0.2, CELL.spark, PRE.drift, 1);
        fx.add.add(tx, 0.2, tz, 0, 0, 0, ca, 0.25, 0.8, CELL.ring, PRE.pop);
      });
    }
    for (const t of tg) impact(fx, c, o.impact || 'holy', t.x, t.z, d + 0.3, o.impactSize || 0.7, t.y);
  },

  // holy cross standing on the targets; o: size, to, orbs (ring of spheres in a cross), color
  cross(fx, c, o, d) {
    const tg = spots(c, o);
    const ca = col(c, o.color);
    const camYaw = Math.atan2(fx.cam.position.x - c.C.x, fx.cam.position.z - c.C.z);
    tg.forEach((B, ti) => {
      const S = (o.size || 1.2) * Math.sqrt(c.S);
      const t0 = d + ti * 0.05;
      const st = { v: null, f: null, g: null };
      fx.run(t0, o.dur || 0.8, (k) => {
        if (!st.v) {
          st.v = fx.mesh('cross'); st.f = fx.mesh('crossFlat'); st.g = fx.mesh('cross');
          st.v.material.color.copy(ca); st.g.material.color.copy(WHITE); st.f.material.color.copy(ca);
        }
        const rise = Math.min(1, k / 0.25);
        const fade = k > 0.65 ? (1 - k) / 0.35 : 1;
        for (const m of [st.v, st.g]) { m.position.set(B.x, S * (0.4 + 0.7 * rise), B.z); m.rotation.set(0, camYaw, 0); }
        st.v.scale.setScalar(S * (0.6 + 0.5 * rise));
        st.g.scale.setScalar(S * (0.45 + 0.35 * rise));
        st.f.position.set(B.x, 0.17, B.z);
        st.f.rotation.y = fx.now * 0.5;
        st.f.scale.setScalar(S * 1.2 * rise);
        st.v.material.opacity = fade * 0.85; st.g.material.opacity = fade * 0.5; st.f.material.opacity = fade * 0.6;
      }, () => { if (st.v) { fx.free(st.v); fx.free(st.f); fx.free(st.g); } });
      fx.pillar(t0, B.x, B.z, ca, 0.35 * S, 6, 0.7);
      if (o.orbs) {
        // spheres laid out in a cross above the target
        const P = [[0, 2], [0, 1], [0, 0], [0, -1], [-1, 1], [1, 1], [0, 3], [-2, 1], [2, 1], [0, -2]];
        P.forEach(([px, py], j) => {
          const oc = j % 2 ? c.cb : ca;
          fx.run(t0 + j * 0.03, 0.7, (k) => {
            const x = B.x + Math.cos(camYaw) * px * 0.55 * S, z = B.z - Math.sin(camYaw) * px * 0.55 * S;
            const sz = 0.9 * S * (k < 0.8 ? 1 : (1 - k) * 5);
            fx.quad('glow', x, 1.4 * S + py * 0.55 * S - k * k * 1.2 * S, z, sz, sz, oc, 0.9);
            fx.ball(x, 1.4 * S + py * 0.55 * S - k * k * 1.2 * S, z, sz * 0.12, WHITE, 0.9);
          });
        });
      }
      impact(fx, c, o.impact || 'holy', B.x, B.z, t0 + 0.2, o.impactSize || 1, B.y);
    });
  },

  // dark vortex on the ground; o: size, dur, cell, color, up (vertical funnel)
  vortex(fx, c, o, d) {
    const tg = o.to === 'targets' ? c.T : [c.C];
    const dur = o.dur || 0.9;
    const ca = col(c, o.color), cb = c.cb, cell = cellOf(o.cell, 'wisp');
    tg.forEach((B) => {
      const R = (o.size || c.R + 0.8) * (o.size ? c.S : 1);
      const st = { m: null, m2: null, t: null };
      fx.run(d, dur, (k, dt) => {
        if (!st.m) {
          st.m = fx.mesh('swirl'); st.m2 = fx.mesh('swirl'); st.t = fx.mesh('torus');
          st.m.material.color.copy(ca); st.m2.material.color.copy(cb); st.t.material.color.copy(cb);
        }
        const g = k < 0.2 ? k / 0.2 : k > 0.75 ? (1 - k) / 0.25 : 1;
        st.m.position.set(B.x, 0.18, B.z); st.m.rotation.y = -fx.now * 5; st.m.scale.setScalar(R * g);
        st.m2.position.set(B.x, 0.22, B.z); st.m2.rotation.y = -fx.now * 8; st.m2.scale.setScalar(R * 0.6 * g);
        st.m.material.opacity = g; st.m2.material.opacity = g * 0.8;
        st.t.position.set(B.x, 0.3 + k * 1.2, B.z); st.t.rotation.x = Math.PI / 2; st.t.scale.setScalar(R * (1 - k * 0.7)); st.t.material.opacity = g * 0.8;
        const n = Math.round(dt * 110 * c.S);
        for (let i = 0; i < n; i++) {
          const a = Math.random() * TAU, r = R * rnd(0.6, 1);
          // tangential + inward + up
          fx.add.add(B.x + Math.cos(a) * r, rnd(0.1, 0.4), B.z + Math.sin(a) * r, -Math.sin(a) * r * 4 - Math.cos(a) * r * 1.6, rnd(0.5, o.up ? 5 : 2), Math.cos(a) * r * 4 - Math.sin(a) * r * 1.6, pick(ca, cb), 0.45, 0.4 * c.S, cell, PRE.none);
        }
      }, () => { if (st.m) { fx.free(st.m); fx.free(st.m2); fx.free(st.t); } });
    });
    for (const t of c.T.length ? c.T : [c.C]) impact(fx, c, o.impact || 'dark', t.x, t.z, d + dur * 0.5, o.impactSize || 1, 0.8);
  },

  // a gate that opens behind the targets and pulls them in; o: size, dur, color
  portal(fx, c, o, d) {
    const ca = col(c, o.color), cb = c.cb;
    const B = c.T.length ? c.T[0] : c.C;
    const S = (o.size || 1.3) * Math.sqrt(c.S);
    const dur = o.dur || 1.0;
    // gate stands just past the target, facing the camera
    const camYaw = Math.atan2(fx.cam.position.x - B.x, fx.cam.position.z - B.z);
    const gx = B.x - Math.sin(camYaw) * 0.4, gz = B.z - Math.cos(camYaw) * 0.4, gy = 0.4 + 1.1 * S;
    const st = { ring: null, ring2: null, sw: null, glow: null };
    fx.run(d, dur, (k, dt) => {
      if (!st.ring) {
        st.ring = fx.mesh('torus'); st.ring2 = fx.mesh('torus'); st.sw = fx.mesh('swirlUp');
        st.ring.material.color.copy(ca); st.ring2.material.color.copy(WHITE); st.sw.material.color.copy(cb);
      }
      const open = k < 0.25 ? k / 0.25 : k > 0.8 ? (1 - k) / 0.2 : 1;
      for (const m of [st.ring, st.ring2, st.sw]) { m.position.set(gx, gy, gz); m.rotation.set(0, camYaw, 0); }
      st.ring.scale.set(S * open, S * 1.25 * open, S);
      st.ring2.scale.set(S * 0.92 * open, S * 1.15 * open, S * 0.5);
      st.sw.rotateZ(dt * 6);
      st.sw.scale.set(S * 0.9 * open, S * 1.1 * open, 1);
      st.ring.material.opacity = 0.9 * open; st.ring2.material.opacity = 0.5 * open; st.sw.material.opacity = 0.75 * open;
      fx.quad('glow', gx, gy, gz, S * 2.5 * open, S * 2.5 * open, ca, 0.4 * open);
      // targets' energy pulled into the gate
      const n = Math.round(dt * 140);
      for (let i = 0; i < n; i++) {
        const t = c.T.length ? c.T[i % c.T.length] : B;
        const sx = t.x + rnd(-0.4, 0.4), sy = rnd(0.2, 1.4), sz = t.z + rnd(-0.4, 0.4);
        const life = 0.35;
        fx.add.add(sx, sy, sz, (gx - sx) / life, (gy - sy) / life, (gz - sz) / life, pick(ca, cb), life, 0.3 * c.S, pick(CELL.spark, CELL.star), PRE.shrink);
      }
    }, () => { if (st.ring) { fx.free(st.ring); fx.free(st.ring2); fx.free(st.sw); } });
    for (const t of c.T.length ? c.T : [c.C]) impact(fx, c, o.impact || 'holy', t.x, t.z, d + dur * 0.75, o.impactSize || 1.1, 0.9);
    fx.later(d + dur * 0.78, () => fx.flash(0, gx, gy, gz, WHITE, S * 4, 0.3));
  },

  // line of spikes from caster to target; o: count, kind ('ice'|'rock'|'thorn'|'dark'), size, gap
  spikes(fx, c, o, d) {
    const tg = spots(c, o);
    tg.forEach((B) => {
      const n = o.count || 6;
      const A = c.P;
      for (let i = 1; i <= n; i++) {
        const k = i / n;
        const x = A.x + (B.x - A.x) * k + rnd(-0.15, 0.15), z = A.z + (B.z - A.z) * k + rnd(-0.15, 0.15);
        const big = 0.5 + 0.5 * k;
        spikeAt(fx, d + i * (o.gap || 0.04), x, z, 0.22 * c.S * (o.size || 1) * big, 1.3 * c.S * (o.size || 1) * big, o.kind || 'ice', 0.4, rnd(-0.3, 0.3));
        fx.later(d + i * (o.gap || 0.04), () => fx.burst(fx.add, x, 0.2, z, 4, 2.2, col(c, o.color), WHITE, 0.4, 0.16, o.kind === 'ice' || !o.kind ? CELL.shard : CELL.square, PRE.debris, 2));
      }
      impact(fx, c, o.impact, B.x, B.z, d + n * (o.gap || 0.04), o.impactSize || 1, B.y);
    });
  },

  // vine / tentacle / lightning whip that lashes out; o: dur, width, color, lightning, count, sway
  whip(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 1;
    const ca = col(c, o.color);
    tg.forEach((B, ti) => {
      for (let w = 0; w < n; w++) {
        const A = launch(c, o.from || 'hands', w);
        const dur = o.dur || 0.4;
        const sway = (o.sway || 1) * (w % 2 ? -1 : 1);
        const dx = B.x - A.x, dy = B.y - A.y, dz = B.z - A.z, L = Math.hypot(dx, dz) || 1;
        const px = -dz / L, pz = dx / L;
        const st = { r: null, r2: null };
        const t0 = d + w * 0.06 + ti * 0.04;
        fx.run(t0, dur, (k) => {
          if (!st.r) { st.r = fx.ribbon(); st.r.color.copy(ca); st.r.taper = 0.6; if (o.lightning) { st.r2 = fx.ribbon(); st.r2.color.copy(WHITE); st.r2.taper = 0.6; } }
          const reach = k < 0.45 ? 1 - (1 - k / 0.45) ** 2 : 1 - (k - 0.45) / 0.55 * 0.4;
          for (const r of [st.r, st.r2]) {
            if (!r) continue;
            r.count = 20;
            for (let i = 0; i < 20; i++) {
              const t = (i / 19) * reach;
              const curl = Math.sin(t * Math.PI) * sway * (1 - k * 0.8) * L * 0.25 + Math.sin(t * 9 - k * 14) * 0.12 * (1 - t);
              const j = o.lightning ? 0.12 : 0;
              r.pts[i].set(A.x + dx * t + px * curl + rnd(-j, j), A.y + dy * t + Math.sin(t * Math.PI) * 0.6 + rnd(-j, j), A.z + dz * t + pz * curl + rnd(-j, j));
            }
            r.width = (o.width || 0.12) * c.S * (r === st.r2 ? 0.4 : 1);
            r.build(fx.cam, k > 0.75 ? (1 - k) * 4 : 1);
          }
          const tip = st.r.pts[19];
          fx.add.add(tip.x, tip.y, tip.z, rnd(-1, 1), rnd(-1, 1), rnd(-1, 1), pick(ca, c.cb), 0.2, 0.3 * c.S, o.lightning ? CELL.spark : o.leafy === false ? CELL.glow : CELL.leaf, PRE.drift);
        }, () => { if (st.r) fx.freeRibbon(st.r); if (st.r2) fx.freeRibbon(st.r2); });
        if (w === 0) impact(fx, c, o.impact, B.x, B.z, t0 + dur * 0.42, o.impactSize || 0.9, B.y);
      }
    });
  },

  // flying sprites (bubbles, notes, feathers, bats, fish, cards, shards, seeds) toward the targets
  // o: cell, count, size, speed, wobble, spin, color, spread (start spread around the caster), hold (pause before flying)
  swarm(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 10;
    const cell = cellOf(o.cell, 'bubble');
    const sys = sysFor(fx, cell);
    const ca = col(c, o.color), cb = col(c, o.color2, c.cb);
    for (let i = 0; i < n; i++) {
      const B = tg[i % tg.length];
      const sp = o.spread || 0.4;
      const a0 = (i / n) * TAU;
      const A = o.fan ? { x: c.P.x + Math.cos(a0) * sp, y: c.A.y + 0.4 + Math.sin(a0 * 2) * 0.2, z: c.P.z + Math.sin(a0) * sp } : { x: c.A.x + rnd(-sp, sp), y: c.A.y + rnd(-sp, sp) * 0.5, z: c.A.z + rnd(-sp, sp) };
      const tx = B.x + rnd(-0.3, 0.3), tz = B.z + rnd(-0.3, 0.3);
      const L = Math.hypot(tx - A.x, tz - A.z);
      const dur = Math.max(0.2, L / ((o.speed || 8) * rnd(0.8, 1.2)));
      const side = rnd(-1, 1) * (o.wobble != null ? o.wobble : 0.8);
      const hold = o.hold || 0;
      const t0 = d + i * (o.gap || 0.03);
      const color = Math.random() < 0.5 ? ca : cb;
      const size = (o.size || 0.35) * c.S * rnd(0.8, 1.2);
      if (hold) fx.run(t0, hold, () => sys.add(A.x, A.y, A.z, 0, 0, 0, sys === fx.alpha && !o.tint ? WHITE : color, 0.05, size * 2, cell, PRE.none, 1, o.spin ? fx.now * o.spin : 0));
      fx.fly(t0 + hold, dur, A.x, A.y, A.z, tx, B.y, tz, o.arc != null ? o.arc : 0.5, (x, y, z) => {
        sys.add(x, y, z, 0, 0, 0, sys === fx.alpha && !o.tint ? WHITE : color, 0.05, size * 2, cell, PRE.none, 1, o.spin ? fx.now * o.spin + i : 0);
        if (o.glow !== false) fx.add.add(x, y, z, 0, 0, 0, color, 0.05, size * 2.4, CELL.glow, PRE.none, 0.35, 0);
      }, null, side);
    }
    const last = d + (n - 1) * (o.gap || 0.03) + (o.hold || 0) + 0.3;
    for (const t of tg) impact(fx, c, o.impact, t.x, t.z, Math.min(last, d + 0.6 + (o.hold || 0)), o.impactSize || 0.9, t.y);
  },

  // ground shockwave (slams, hammer blows); o: size, hammer (glow drops first), debris, color, to
  shock(fx, c, o, d) {
    const at = o.to === 'center' ? c.C : o.to === 'targets' && c.T.length ? c.T[0] : c.P;
    const R = (o.size || c.R * 0.85) * (o.size ? Math.sqrt(c.S) : 1);
    const ca = col(c, o.color), cb = c.cb;
    let t0 = d;
    if (o.hammer) {
      fx.run(d, 0.16, (k) => fx.quad('glow', at.x, 3.2 - k * 3, at.z, 1.4 * c.S, 2 * c.S, ca, 0.9));
      t0 += 0.16;
    }
    fx.groundRing(t0, at.x, at.z, ca, R * 1.15, 0.5);
    fx.groundRing(t0 + 0.06, at.x, at.z, ca, R * 1.3, 0.6, 'ring');
    const st = { t: null };
    fx.run(t0, 0.45, (k) => {
      if (!st.t) { st.t = fx.mesh('torus'); st.t.material.color.copy(cb); st.t.rotation.x = Math.PI / 2; }
      st.t.position.set(at.x, 0.25 + k * 0.3, at.z); st.t.scale.setScalar(0.3 + (1 - (1 - k) ** 2) * R); st.t.material.opacity = 0.6 * (1 - k);
    }, () => { if (st.t) fx.free(st.t); });
    fx.later(t0, () => {
      fx.flash(0, at.x, 0.4, at.z, ca, R * 0.9, 0.3);
      if (o.debris !== false) for (let i = 0; i < 26 * c.S; i++) {
        const a = Math.random() * TAU, s = rnd(2, 5) * Math.sqrt(R);
        fx.alpha.add(at.x, 0.2, at.z, Math.cos(a) * s, rnd(3, 6), Math.sin(a) * s, pick(ROCK, ROCK2), 0.9, 0.2, CELL.square, PRE.debris);
      }
      fx.ringBurst(fx.alpha, at.x, 0.2, at.z, 16, 4 * Math.sqrt(R), SMOKE, 0.7, 0.8, CELL.smoke, PRE.smoke);
      fx.burst(fx.add, at.x, 0.3, at.z, 18 * c.S, 4, ca, cb, 0.5, 0.4, o.cell ? cellOf(o.cell) : CELL.spark, PRE.drift, 2);
    });
    // hit everyone in range
    for (const t of c.T) impact(fx, c, o.impact, t.x, t.z, t0 + 0.08, o.impactSize || 0.7, t.y);
    if (!c.T.length) impact(fx, c, o.impact, at.x, at.z, t0, o.impactSize || 0.8, 0.4);
  },

  // protective dome on the caster; o: style, size, color, orbit (cell circling the dome)
  dome(fx, c, o, d) {
    const P = c.P;
    const ca = col(c, o.color), cb = c.cb;
    const R = (o.size || 1.1) * c.S;
    const st = { s: null, t: null };
    fx.run(d, 0.9, (k, dt) => {
      if (!st.s) { st.s = fx.mesh('sphere'); st.t = fx.mesh('torus'); st.s.material.color.copy(ca); st.t.material.color.copy(cb); }
      st.s.position.set(P.x, 0.8, P.z);
      st.s.scale.setScalar(R * (k < 0.15 ? k / 0.15 : 1 + 0.06 * Math.sin(k * 30) * (1 - k)));
      st.s.material.opacity = 0.28 * (1 - k * 0.9);
      st.t.rotation.x = Math.PI / 2; st.t.position.set(P.x, 0.2 + k * 1.6 * c.S, P.z); st.t.scale.setScalar(R * (0.9 + 0.3 * Math.sin(k * Math.PI))); st.t.material.opacity = 1 - k;
      if (o.orbit) {
        const cell = cellOf(o.orbit);
        for (let i = 0; i < (o.count || 4); i++) {
          const a = fx.now * 4 + (i / (o.count || 4)) * TAU;
          sysFor(fx, cell).add(P.x + Math.cos(a) * R * 1.1, 0.8 + Math.sin(a * 2) * 0.3, P.z + Math.sin(a) * R * 1.1, 0, 0, 0, i % 2 ? ca : cb, 0.05, 0.6 * c.S, cell, PRE.none, 1, fx.now * 6);
        }
      }
      if (Math.random() < dt * 30) {
        const a = Math.random() * TAU, b = rnd(-0.2, 1);
        fx.add.add(P.x + Math.cos(a) * R * Math.sqrt(1 - b * b), 0.8 + b * R, P.z + Math.sin(a) * R * Math.sqrt(1 - b * b), 0, 0, 0, cb, 0.35, 0.45 * c.S, CELL.hex, PRE.pop);
      }
    }, () => { if (st.s) { fx.free(st.s); fx.free(st.t); } });
    fx.later(d, () => fx.burst(fx.add, P.x, 0.9, P.z, 26, 2.5, ca, WHITE, 0.6, 0.25, CELL.spark, PRE.drift, 0));
  },

  // healing on each target (allies); o: cell, color, motes (light flows from caster to each ally first)
  heal(fx, c, o, d) {
    const ca = col(c, o.color, hexColor('#7dff9a')), cb = col(c, o.color2, hexColor('#fff6a0'));
    const cell = cellOf(o.cell, 'star');
    c.T.forEach((B, i) => {
      let t0 = d + i * 0.05;
      if (o.motes !== false && dist(B, c.P) > 0.3) {
        const L = dist(c.P, B), life = L / 9;
        fx.later(t0, () => { for (let j = 0; j < 8; j++) fx.add.add(c.A.x, c.A.y, c.A.z, (B.x - c.A.x) / life + rnd(-1, 1), (1.2 - c.A.y) / life + rnd(0, 2), (B.z - c.A.z) / life + rnd(-1, 1), pick(ca, cb), life, 0.3, CELL.glow, PRE.none); });
        t0 += life;
      }
      fx.later(t0, () => {
        for (let j = 0; j < 16; j++) {
          const a = Math.random() * TAU, r = rnd(0.1, 0.6);
          sysFor(fx, cell).add(B.x + Math.cos(a) * r, rnd(0.1, 0.6), B.z + Math.sin(a) * r, 0, rnd(1.2, 2.4), 0, pick(ca, cb), rnd(0.7, 1.1), 0.3 * (o.size || 1), cell, cell === CELL.petal || cell === CELL.leaf || cell === CELL.feather ? PRE.flutter : PRE.none);
        }
        fx.groundRing(0, B.x, B.z, ca, 1.2, 0.5, 'ring');
        fx.flash(0, B.x, 0.8, B.z, ca, 1.2, 0.3);
      });
    });
  },

  // rising spiral column; o: cell, height, dur, size, color, to
  tornado(fx, c, o, d) {
    const tg = o.to === 'center' ? [c.C] : spots(c, o);
    const H = (o.height || 2.8) * c.S;
    const dur = o.dur || 0.7;
    const ca = col(c, o.color), cb = c.cb, cell = cellOf(o.cell, 'streak');
    const sys = sysFor(fx, cell);
    tg.forEach((B) => {
      const R0 = (o.size || 0.6) * (o.to === 'center' ? Math.max(1, c.R * 0.6) : 1) * c.S;
      fx.run(d, dur, (k, dt) => {
        const n = Math.round(dt * 150 * c.S);
        for (let i = 0; i < n; i++) {
          const a = Math.random() * TAU, y = Math.random() * H * 0.3, r = R0 * (0.4 + y / H);
          sys.add(B.x + Math.cos(a) * r, y, B.z + Math.sin(a) * r, -Math.sin(a) * 7 * r, H * rnd(1.4, 2.2), Math.cos(a) * 7 * r, pick(ca, cb), 0.45, (o.psize || 0.35) * c.S, cell, PRE.swirl);
        }
      });
      fx.groundRing(d, B.x, B.z, ca, R0 * 2, dur, 'ring');
    });
    for (const t of tg) impact(fx, c, o.impact || 'wind', t.x, t.z, d + dur * 0.3, o.impactSize || 0.9, 0.9);
  },

  // expanding rings flying at the target (sonic rings, Blast Rings); o: count, size, gap, dur, color
  rings(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 3;
    const ca = col(c, o.color);
    tg.forEach((B, ti) => {
      for (let i = 0; i < n; i++) {
        const A = launch(c, o.from);
        const L = Math.hypot(B.x - A.x, B.z - A.z);
        const dur = o.dur || Math.min(0.5, L / 12);
        const st = { m: null };
        const t0 = d + i * (o.gap || 0.08) + ti * 0.03;
        fx.fly(t0, dur, A.x, A.y, A.z, B.x, B.y, B.z, 0, (x, y, z, k) => {
          if (!st.m) { st.m = fx.mesh('torus'); st.m.material.color.copy(i % 2 ? c.cb : ca); }
          st.m.position.set(x, y, z);
          st.m.lookAt(B.x, B.y, B.z);
          st.m.scale.setScalar((o.size || 0.35) * c.S * (0.6 + k * 1.0));
          st.m.material.opacity = 1 - k * 0.4;
        }, () => { if (st.m) fx.free(st.m); });
        if (i === 0) impact(fx, c, o.impact || 'wind', B.x, B.z, t0 + dur, o.impactSize || 0.9, B.y);
      }
    });
  },

  // rapid gunfire: one aligned tracer sprite per bullet; o: count, rate, size, guns, cell, color, spread
  volley(fx, c, o, d) {
    const tg = spots(c, o);
    const n = o.count || 10;
    const ca = col(c, o.color), cell = cellOf(o.cell, 'streak');
    for (let i = 0; i < n; i++) {
      const B = tg[i % tg.length];
      const A = launch(c, o.from || (o.guns === 1 ? 'mouth' : 'hands'), i);
      const sp = o.spread != null ? o.spread : 0.35;
      const tx = B.x + rnd(-sp, sp), ty = B.y + rnd(-sp, sp) * 0.5, tz = B.z + rnd(-sp, sp);
      const L = Math.hypot(tx - A.x, ty - A.y, tz - A.z);
      const life = L / (o.speed || 26);
      const t0 = d + i * (o.rate || 0.035);
      fx.later(t0, () => {
        fx.add.add(A.x, A.y, A.z, (tx - A.x) / life, (ty - A.y) / life, (tz - A.z) / life, ca, life, (o.size || 0.5) * c.S, cell, PRE.none);
        fx.add.add(A.x, A.y, A.z, 0, 0, 0, c.cb, 0.06, 0.5 * c.S, CELL.spark, PRE.none, 1);
      });
      fx.later(t0 + life, () => {
        fx.burst(fx.add, tx, ty, tz, 4, 3, ca, WHITE, 0.2, 0.18, CELL.spark, PRE.spark, 1);
      });
    }
    for (const t of tg) impact(fx, c, o.impact || 'metal', t.x, t.z, d + 0.1, o.impactSize || 0.8, t.y);
  },

  // the body charges at the target in a blazing streak (headbutts, Brave Metal); o: width, color, trail
  streak(fx, c, o, d) {
    const B = spots(c, o)[0];
    const A = c.P;
    const ca = col(c, o.color);
    const st = { r: null };
    const dur = o.dur || 0.3;
    fx.run(d, dur, (k, dt) => {
      if (!st.r) { st.r = fx.ribbon(); st.r.color.copy(ca); st.r.taper = 0.7; }
      const r = st.r, reach = Math.min(1, k * 2.2), tail = Math.max(0, k * 1.6 - 0.6);
      r.count = 10;
      for (let i = 0; i < 10; i++) { const t = tail + (reach - tail) * (i / 9); r.pts[i].set(A.x + (B.x - A.x) * t, 0.8, A.z + (B.z - A.z) * t); }
      r.width = (o.width || 0.9) * c.S;
      r.build(fx.cam, 1 - k * 0.6);
      const hx = A.x + (B.x - A.x) * reach, hz = A.z + (B.z - A.z) * reach;
      if (reach < 1) emitTrail(fx, o.trail || 'flame', hx, 0.8, hz, dt, c.S * 1.6, ca, c.cb);
    }, () => { if (st.r) fx.freeRibbon(st.r); });
    impact(fx, c, o.impact, B.x, B.z, d + dur * 0.45, o.impactSize || 1.2, B.y);
  },

  // a spinning wheel or saw rolling to the target (Dragon Wheel, Spiral Saw); o: size, cell, dur, color, saw
  wheel(fx, c, o, d) {
    const tg = spots(c, o);
    const ca = col(c, o.color), cb = c.cb;
    tg.forEach((B) => {
      const A = c.A;
      const L = Math.hypot(B.x - A.x, B.z - A.z);
      const dur = o.dur || Math.min(0.5, L / 10);
      const R = (o.size || 0.5) * c.S;
      const st = { t: null, t2: null };
      const ang = Math.atan2(B.x - A.x, B.z - A.z);
      fx.fly(d, dur, A.x, A.y, A.z, B.x, B.y, B.z, 0.2, (x, y, z, k, dt) => {
        if (!st.t) { st.t = fx.mesh('torus'); st.t2 = fx.mesh(o.saw ? 'tri' : 'torus'); st.t.material.color.copy(ca); st.t2.material.color.copy(cb); }
        for (const m of [st.t, st.t2]) { m.position.set(x, y, z); m.rotation.order = 'YXZ'; m.rotation.set(0, ang + Math.PI / 2, -fx.now * 25); }
        st.t.scale.set(R, R, R * 3);
        st.t2.scale.setScalar(R * 0.75);
        const n = Math.round(dt * 160);
        const cell = cellOf(o.cell, 'flame');
        for (let i = 0; i < n; i++) {
          const a = Math.random() * TAU;
          const ox = Math.cos(a) * R * Math.cos(ang), oz = -Math.cos(a) * R * Math.sin(ang);
          fx.add.add(x + ox, y + Math.sin(a) * R, z + oz, rnd(-0.5, 0.5), rnd(0, 1), rnd(-0.5, 0.5), pick(ca, cb), 0.3, 0.4 * c.S, cell, cell === CELL.flame ? PRE.fireTrail : PRE.drift);
        }
      }, () => { if (st.t) { fx.free(st.t); fx.free(st.t2); } });
      impact(fx, c, o.impact, B.x, B.z, d + dur, o.impactSize || 1, B.y);
    });
  },

  // sticky web thrown at the target; o: size, color
  net(fx, c, o, d) {
    const tg = spots(c, o);
    const ca = col(c, o.color, WHITE);
    tg.forEach((B) => {
      const A = c.A;
      const L = Math.hypot(B.x - A.x, B.z - A.z), dur = Math.min(0.4, L / 10);
      const st = { m: null };
      const S = (o.size || 0.9) * c.S;
      fx.run(d, dur + 0.7, (k) => {
        if (!st.m) { st.m = fx.mesh('web'); st.m.material.color.copy(ca); }
        const t = Math.min(1, (k * (dur + 0.7)) / dur);
        st.m.position.set(A.x + (B.x - A.x) * t, A.y + (B.y - A.y) * t + Math.sin(Math.PI * t) * 0.5, A.z + (B.z - A.z) * t);
        st.m.quaternion.copy(fx.cam.quaternion);
        st.m.scale.setScalar(S * (0.25 + 0.75 * t));
        st.m.material.opacity = k > 0.75 ? (1 - k) * 4 : 1;
      }, () => { if (st.m) fx.free(st.m); });
      impact(fx, c, o.impact || 'poison', B.x, B.z, d + dur, o.impactSize || 0.7, B.y);
    });
  },

  // beam of light down from the sky onto each target; o: color, size, height
  pillar(fx, c, o, d) {
    const tg = spots(c, o);
    const ca = col(c, o.color);
    tg.forEach((B, i) => {
      fx.pillar(d + i * 0.05, B.x, B.z, ca, (o.size || 0.55) * c.S, o.height || 8, o.dur || 0.7);
      fx.pillar(d + i * 0.05, B.x, B.z, WHITE, (o.size || 0.55) * 0.4 * c.S, o.height || 8, o.dur || 0.7);
      impact(fx, c, o.impact || 'holy', B.x, B.z, d + i * 0.05 + 0.08, o.impactSize || 0.9, B.y);
    });
  },

  // things falling over the area (petals, feathers, snow); o: cell, count, height, color
  rain(fx, c, o, d) {
    const tg = o.to === 'targets' && c.T.length ? c.T : [c.C];
    const cell = cellOf(o.cell, 'petal');
    const sys = sysFor(fx, cell);
    const ca = col(c, o.color), cb = c.cb;
    tg.forEach((B) => {
      const R = o.to === 'targets' ? 0.8 : Math.max(1.2, c.R);
      fx.run(d, o.dur || 0.8, (k, dt) => {
        const n = Math.round(dt * (o.count || 80));
        for (let i = 0; i < n; i++) {
          const a = Math.random() * TAU, r = Math.sqrt(Math.random()) * R;
          sys.add(B.x + Math.cos(a) * r, o.height || 3.2, B.z + Math.sin(a) * r, rnd(-0.5, 0.5), -rnd(2, 3.5), rnd(-0.5, 0.5), pick(ca, cb), 1.1, (o.size || 0.32) * c.S, cell, PRE.flutter);
        }
      });
    });
    for (const t of c.T) impact(fx, c, o.impact || 'leaf', t.x, t.z, d + 0.4, o.impactSize || 0.7, t.y);
  },

  // a flash or glyph at the caster without hitting anything (seals, muzzle flashes); o: glyph, size, color
  glyph(fx, c, o, d) {
    glyph(fx, o.glyph || 'tri', o.place === 'ground' ? { x: c.P.x, y: 0.2, z: c.P.z } : c.A, c.yaw, col(c, o.color), (o.size || 1) * c.S, d, o.dur || 0.6, o.place === 'ground');
  },

  // life flows from targets back to the caster
  drain(fx, c, o, d) {
    for (const t of c.T.length ? c.T : [c.C]) fx.later(d, () => flow(fx, t.x, t.z, c.P.x, c.P.z));
  },
};
PRE.breath = { grav: -0.3, drag: 0.4, grow: 2.3, spin: 1 };

// glowing glyph at a point, facing along yaw (X, triangle, cross, circle)
function glyph(fx, kind, A, yaw, color, size, d, dur, flat = false) {
  const st = { m: null };
  const meshKind = kind === 'tri' ? 'tri' : kind === 'circle' ? 'torus' : flat ? 'crossFlat' : 'cross';
  fx.run(d, dur, (k) => {
    if (!st.m) { st.m = fx.mesh(meshKind); st.m.material.color.copy(color); }
    st.m.position.set(A.x + Math.sin(yaw) * 0.35, A.y, A.z + Math.cos(yaw) * 0.35);
    if (flat && meshKind === 'crossFlat') st.m.rotation.set(0, yaw + fx.now, 0);
    else if (flat) { st.m.position.set(A.x, 0.2, A.z); st.m.rotation.set(Math.PI / 2, 0, 0); }
    else { st.m.rotation.order = 'YXZ'; st.m.rotation.set(0, yaw, kind === 'x' ? Math.PI / 4 : kind === 'tri' ? fx.now * 3 : 0); }
    const g = k < 0.2 ? k / 0.2 : k > 0.75 ? (1 - k) / 0.25 : 1;
    st.m.scale.setScalar(size * (0.4 + 0.6 * g) * 0.5);
    st.m.material.opacity = (flat ? 0.45 : 0.8) * g;
  }, () => { if (st.m) fx.free(st.m); });
}

// glow that grows at a point (charging a beam or a breath)
function chargeGlow(fx, A, ca, cb, size, d, dur) {
  fx.run(d, dur, (k, dt) => {
    fx.quad('glow', A.x, A.y, A.z, size * (0.2 + k), size * (0.2 + k), ca, 0.85, fx.now * 5);
    const n = Math.round(dt * 80);
    for (let i = 0; i < n; i++) {
      const a = Math.random() * TAU, b = rnd(-1, 1), R = size * 0.8;
      const x = Math.cos(a) * Math.sqrt(1 - b * b) * R, y = b * R, z = Math.sin(a) * Math.sqrt(1 - b * b) * R;
      fx.add.add(A.x + x, A.y + y, A.z + z, -x / 0.2, -y / 0.2, -z / 0.2, pick(ca, cb), 0.2, 0.2, CELL.spark, PRE.none);
    }
  });
}

// one slash arc facing the camera, rotated by ang in the screen plane, sweeping a little
function slashMesh(fx, d, x, y, z, color, size, ang, kind, dur) {
  fx.run(d, dur, (k) => {
    const sc = size * (0.55 + 0.6 * Math.min(1, k * 2.5));
    fx.quad(kind, x, y, z, sc, sc, color, k < 0.6 ? 1 : (1 - k) / 0.4, ang + (k - 0.5) * 0.7);
  });
}

// ---------- charge-up at the caster before the move leaves ----------
const CHARGES = {
  aura(fx, c, dur) {
    for (let i = 0; i < 20 + c.stage * 10; i++) {
      const th = Math.random() * TAU, r = rnd(0.9, 1.5);
      fx.add.add(c.P.x + Math.cos(th) * r, rnd(0.2, 1.5), c.P.z + Math.sin(th) * r, -Math.cos(th) * r / Math.max(0.2, dur), rnd(0, 1), -Math.sin(th) * r / Math.max(0.2, dur), pick(c.ca, c.cb), Math.max(0.2, dur), 0.3, CELL.spark, PRE.none);
    }
    fx.groundRing(0, c.P.x, c.P.z, c.cb, 1.3, Math.max(0.3, dur + 0.1), 'ring');
  },
  mouth(fx, c, dur) { chargeGlow(fx, c.A, c.ca, c.cb, 0.35 * c.S, 0, Math.max(0.12, dur)); },
  hands(fx, c, dur) { for (let i = 0; i < 2; i++) chargeGlow(fx, launch(c, 'hands', i), c.ca, c.cb, 0.28 * c.S, 0, Math.max(0.12, dur)); },
  sky(fx, c, dur) {
    chargeGlow(fx, { x: c.P.x, y: c.h + 0.8, z: c.P.z }, c.ca, c.cb, 0.4 * c.S, 0, Math.max(0.15, dur));
    fx.pillar(0, c.P.x, c.P.z, c.ca, 0.35, 4, Math.max(0.3, dur + 0.15));
  },
  ground(fx, c, dur) {
    fx.groundRing(0, c.P.x, c.P.z, c.ca, 1.8 * c.S, Math.max(0.3, dur + 0.1), 'shock');
    for (let i = 0; i < 16; i++) { const a = Math.random() * TAU; fx.alpha.add(c.P.x + Math.cos(a) * 0.8, 0.1, c.P.z + Math.sin(a) * 0.8, Math.cos(a), rnd(0.5, 1.5), Math.sin(a), SMOKE, 0.6, 0.5, CELL.smoke, PRE.smoke, 0.5); }
  },
  back(fx, c, dur) { for (let i = 0; i < 2; i++) chargeGlow(fx, launch(c, 'back', i), c.ca, c.cb, 0.35 * c.S, 0, Math.max(0.15, dur)); },
  none() {},
};

// ---------- basic attacks: one cheap effect per swing or shot ----------
// melee kinds draw one arc or ring at the target; 'shot' flies a sprite head from caster to target
function basicFx(fx, b, A, B, h, stage, big, ca, cb) {
  const S = 1 + stage * 0.2;
  const y = 0.85;
  const dx = B.x - A.x, dz = B.z - A.z, L = Math.hypot(dx, dz) || 1;
  switch (b.kind) {
    case 'shot': {
      const cell = cellOf(b.head, 'glow');
      const sys = sysFor(fx, cell);
      const Ay = h * 0.6;
      const dur = Math.min(0.3, Math.max(0.12, L / (b.speed || 16)));
      const size = (b.size || 0.3) * S * (big ? 1.4 : 1);
      fx.fly(0, dur, A.x, Ay, A.z, B.x, y, B.z, b.arc != null ? b.arc : 0.25, (x, yy, z, k, dt) => {
        sys.add(x, yy, z, 0, 0, 0, sys === fx.alpha ? WHITE : cb, 0.05, size * 2, cell, PRE.none, 1, b.spin ? fx.now * b.spin : 0);
        fx.add.add(x, yy, z, 0, 0, 0, ca, 0.05, size * 2.6, CELL.glow, PRE.none, 0.5, 0);
        if (b.trail && Math.random() < 0.6) emitTrail(fx, b.trail, x, yy, z, dt * 0.5, size * 2.5, ca, cb);
      }, () => {
        fx.burst(fx.add, B.x, y, B.z, 6, 2.5, ca, cb, 0.25, 0.2, CELL.spark, PRE.drift, 0.5);
        fx.add.add(B.x, y, B.z, 0, 0, 0, ca, 0.16, size * 3, CELL.glow, PRE.pop, 0.8);
      });
      return;
    }
    case 'bite':
      slashMesh(fx, 0, B.x, y + 0.15, B.z, ca, 0.7 * S, Math.PI, 'crescent', 0.16);
      slashMesh(fx, 0.02, B.x, y - 0.15, B.z, ca, 0.7 * S, 0, 'crescent', 0.16);
      break;
    case 'punch':
      fx.add.add(B.x, y, B.z, 0, 0, 0, ca, 0.18, 1.1 * S, CELL.ring, PRE.pop);
      fx.add.add(B.x, y, B.z, 0, 0, 0, cb, 0.1, 0.9 * S, CELL.glow, PRE.pop);
      break;
    case 'horn': case 'peck': case 'sting':
      for (let i = 0; i < (b.kind === 'peck' ? 2 : 1); i++) fx.add.add(B.x - (dx / L) * 0.6, y + i * 0.15, B.z - (dz / L) * 0.6, (dx / L) * 6, 0, (dz / L) * 6, ca, 0.12, 0.9 * S, CELL.streak, PRE.none);
      break;
    case 'kick':
      slashMesh(fx, 0, B.x, y, B.z, ca, 0.9 * S, Math.PI / 2 + 0.3, 'crescent', 0.18);
      break;
    case 'tail':
      slashMesh(fx, 0, B.x, y - 0.1, B.z, ca, 1.1 * S, 0.1, 'crescent', 0.2);
      break;
    case 'claw':
      slashMesh(fx, 0, B.x, y, B.z, ca, 0.8 * S, 0.6 + rnd(-0.4, 0.4), 'claw', 0.18);
      break;
    default: // slash
      slashMesh(fx, 0, B.x, y, B.z, ca, 0.9 * S, 0.5 + rnd(-0.6, 0.6), 'crescent', 0.18);
  }
  fx.burst(fx.add, B.x, y, B.z, big ? 10 : 5, 3.5, ca, cb, 0.22, 0.2, CELL.spark, PRE.drift, 0.5);
  if (big) fx.flash(0, B.x, y, B.z, WHITE, 1.1, 0.15);
}

// ---------- the renderer-facing API ----------
// begin() on the cast event, then target()/area()/ally() for the events in the same tick, then flush().
export class Moves {
  constructor(host) {
    this.fx = new MoveFx(host);
    this.pending = null;
    if (host.renderer) this.fx.warm(host.renderer);
  }

  update(dt) { this.fx.update(dt); }
  clear() { this.pending = null; this.fx.clear(); }
  elemOf(name, line) { return specFor(name, line).elem; }

  // cast event. caster: {x,z} ground position, view: the UnitView (for model height and special clip length)
  begin(name, line, stage, caster, view, curTarget) {
    this.flush();
    const spec = specFor(name, line);
    const h = (view && view.height) || [1.3, 1.65, 2.0][stage] || 1.5;
    let release = spec.release != null ? spec.release : 0.3;
    const clip = view && view.clips && view.clips.special;
    // sync to the special clip: it plays in at most 1.1 s, and most models let go a bit before the middle
    if (clip) release = Math.min(release, Math.min(clip.duration, 1.1) * 0.45);
    else release *= 0.7;
    this.pending = { name, line, stage, spec, h, caster: { x: caster.x, z: caster.z }, y0: caster.y || 0, release, T: [], area: null, from: null, curTarget };
  }
  target(w, from) { const p = this.pending; if (!p) return; p.T.push({ x: w.x, y: 0.8, z: w.z }); if (from) p.from = { x: from.x, z: from.z }; }
  area(c, radius) { const p = this.pending; if (!p) return; p.area = { x: c.x, z: c.z, r: radius }; }
  ally(w) { this.target(w); }

  flush() {
    const p = this.pending;
    if (!p) return;
    this.pending = null;
    this.play(p);
  }

  play(p) {
    const fx = this.fx, spec = p.spec, stage = p.stage;
    const S = (spec.scale ? spec.scale[stage] || spec.scale[spec.scale.length - 1] : STAGE_SCALE[stage]) || 1;
    const [ha, hb] = spec.colors || PALETTE[spec.elem] || PALETTE.beast;
    const P = p.caster;
    let T = p.T;
    // area moves with no single target: aim at the current target, or the area center
    const type = p.line ? p.line.type : null;
    if (!T.length && p.curTarget && type !== 'heal') T = [{ x: p.curTarget.x, y: 0.8, z: p.curTarget.z }];
    const C = p.area ? { x: p.area.x, z: p.area.z } : T.length ? { x: T[0].x, z: T[0].z } : { x: P.x, z: P.z };
    const R = p.area ? (p.area.r + 0.5) * 1.7 : 1.2;
    const aim = T[0] || C;
    const yaw = Math.atan2(aim.x - P.x, aim.z - P.z) || 0;
    const fwd = 0.35 * S;
    const A = { x: P.x + Math.sin(yaw) * fwd, y: p.h * 0.62 + p.y0, z: P.z + Math.cos(yaw) * fwd };
    const c = { A, P, h: p.h, T, C, R, S, stage, ca: hexColor(ha), cb: hexColor(hb), yaw, from: p.from, spec, type, shook: false,
      stunTime: DT.stunDuration ? DT.stunDuration(stage) : 1 };
    // area moves hit every enemy in the burst; we only know the center, so fake a few spots for per-target steps
    if (p.area && T.length <= 1 && c.type !== 'heal') {
      const extra = [];
      if (!T.length) extra.push({ x: C.x, y: 0.8, z: C.z });
      c.T = T.concat(extra);
    }
    (CHARGES[spec.charge || 'aura'] || CHARGES.aura)(fx, c, p.release);
    for (const step of spec.fx) {
      const [name, o = {}] = step;
      const prim = PRIMS[name];
      if (prim) prim(fx, c, o, p.release + (o.at || 0));
    }
  }

  // basic attack: from and to are world points; ranged when the attacker shoots
  basic(line, stage, from, to, h, big, ranged) {
    const b = basicFor(line, ranged);
    const [ha, hb] = b.colors || PALETTE[b.elem] || PALETTE.beast;
    basicFx(this.fx, b, from, to, h || 1.4, stage, big, hexColor(ha), hexColor(hb));
  }

  // play a move without a combat, for previews and tests: from {x,z}, targets [{x,z}], area {x,z,r} optional
  demo(name, stage, from, targets, opts = {}) {
    const line = opts.line || (DT.LINES || []).find((l) => l.abil.includes(name)) || null;
    this.begin(name, line, stage, from, opts.view || null, targets[0] || null);
    for (const t of targets) this.target(t, opts.leapFrom);
    if (opts.area) this.area(opts.area, opts.area.r);
    else if (line && (line.type === 'aoe' || line.type === 'slam' || (line.type === 'drain' && line.radius))) {
      const c = line.type === 'slam' ? from : targets[0];
      if (c) this.area(c, line.radius || 1);
      if (line.type === 'slam') this.pending.T = targets.map((t) => ({ x: t.x, y: 0.8, z: t.z }));
    }
    this.flush();
  }
}

export const PRIM_NAMES = Object.keys(PRIMS);
export const IMPACT_NAMES = Object.keys(IMPACTS);
export const CHARGE_NAMES = Object.keys(CHARGES);
if (DT) DT.MoveFxInfo = { prims: PRIM_NAMES, impacts: IMPACT_NAMES, charges: CHARGE_NAMES, cells: Object.keys(CELL) };
