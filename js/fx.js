// Ability effects: a particle system plus per-element ultimate visuals.
// Purely visual: reads combat events, never touches combat state, so multiplayer sync is unaffected.
import * as THREE from 'three';

// Element from the ability's name. Anything unmatched is a physical hit.
const ELEMENTS = [
  ['dark', /death|dark|demi|neo inferno|evil|venom|pandemonium|crimson|hell|nightmare|paradise lost|masquerade|bada boom|corona destroyer/i],
  ['holy', /heaven|talisman|grand cross|divine|gate of destiny|hand of fate|deva|angel|celestial|light|charm|omni sword/i],
  ['thunder', /shock|thunder|lightning|electric|static|spark|electro|javelin|plasma/i],
  ['ice', /blue blaster|howling|ice|frost|snow|freez/i],
  ['fire', /pepper|nova|pyro|atomic|flame|fire|blaze|inferno|wax|heat|burn|meteor|giga destroyer|mugen|ground zero/i],
  ['water', /fish|torpedo|harpoon|bubble|coral|aqua|tidal|wave|hammer/i],
  ['plant', /ivy|needle|flower|seed|sunshine|shower|leaf|net|spiking|poison/i],
  ['wind', /twister|wing|rings|storm|wheel|tornado|cyclone/i],
  ['metal', /gear|gatling|metal|cannon|laser|missile|m16|data crusher|barrage|positron|side attack|shooter|beam/i],
];
const PALETTE = {
  fire: ['#ff5a1a', '#ffd36a'], ice: ['#7fdcff', '#ffffff'], thunder: ['#ffe94a', '#a8dcff'], water: ['#2f9bff', '#bff0ff'],
  plant: ['#5cff6a', '#ffb3e6'], wind: ['#b8fff0', '#ffffff'], holy: ['#fff0a0', '#ffffff'], dark: ['#a04aff', '#ff3a7a'],
  metal: ['#ffb04a', '#d8e4f4'], beast: ['#ffffff', '#ff9a5a'],
};
// UI color for an element (cast banner, caster highlight)
export const elemColor = (elem) => ({ fire: '#ff7a3a', ice: '#8fe3ff', thunder: '#ffe94a', water: '#4fb0ff', plant: '#6cff7a', wind: '#b8fff0',
  holy: '#fff0a0', dark: '#b46bff', metal: '#ffb04a', beast: '#ffd6a8' })[elem] || '#ffd6a8';
export function elementOf(name) {
  const hit = ELEMENTS.find(([, re]) => re.test(name || ''));
  return hit ? hit[0] : 'beast';
}

function softTexture() {
  const c = document.createElement('canvas');
  c.width = c.height = 64;
  const g = c.getContext('2d');
  const grd = g.createRadialGradient(32, 32, 0, 32, 32, 32);
  grd.addColorStop(0, 'rgba(255,255,255,1)');
  grd.addColorStop(0.35, 'rgba(255,255,255,0.7)');
  grd.addColorStop(1, 'rgba(255,255,255,0)');
  g.fillStyle = grd;
  g.fillRect(0, 0, 64, 64);
  const t = new THREE.CanvasTexture(c);
  t.colorSpace = THREE.SRGBColorSpace;
  return t;
}

// Additive point sprites in three size classes. Fading = color toward black, which adds nothing.
class Particles {
  constructor(scene, size, max) {
    this.max = max;
    this.n = 0;
    this.pos = new Float32Array(max * 3);
    this.col = new Float32Array(max * 3);
    this.vel = new Float32Array(max * 3);
    this.base = new Float32Array(max * 3);
    this.life = new Float32Array(max);
    this.maxLife = new Float32Array(max);
    this.grav = new Float32Array(max);
    this.drag = new Float32Array(max);
    const geo = new THREE.BufferGeometry();
    geo.setAttribute('position', new THREE.BufferAttribute(this.pos, 3).setUsage(THREE.DynamicDrawUsage));
    geo.setAttribute('color', new THREE.BufferAttribute(this.col, 3).setUsage(THREE.DynamicDrawUsage));
    geo.setDrawRange(0, 0);
    this.mat = new THREE.PointsMaterial({ size, map: softTexture(), vertexColors: true, transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, sizeAttenuation: true });
    this.points = new THREE.Points(geo, this.mat);
    this.points.frustumCulled = false;
    this.points.renderOrder = 6;
    scene.add(this.points);
  }

  add(x, y, z, vx, vy, vz, color, life, grav = 0, drag = 0) {
    if (this.n >= this.max) return;
    const i = this.n++;
    this.pos.set([x, y, z], i * 3);
    this.vel.set([vx, vy, vz], i * 3);
    this.base.set([color.r, color.g, color.b], i * 3);
    this.life[i] = this.maxLife[i] = life;
    this.grav[i] = grav;
    this.drag[i] = drag;
  }

  update(dt) {
    let w = 0;
    for (let i = 0; i < this.n; i++) {
      const l = this.life[i] - dt;
      if (l <= 0) continue;
      const k = Math.max(0, 1 - this.drag[i] * dt);
      const vx = this.vel[i * 3] * k, vy = this.vel[i * 3 + 1] * k - this.grav[i] * dt, vz = this.vel[i * 3 + 2] * k;
      const f = l / this.maxLife[i];
      const fade = f > 0.8 ? (1 - f) * 5 : f / 0.8; // quick fade in, slow fade out
      this.pos[w * 3] = this.pos[i * 3] + vx * dt;
      this.pos[w * 3 + 1] = Math.max(0.03, this.pos[i * 3 + 1] + vy * dt);
      this.pos[w * 3 + 2] = this.pos[i * 3 + 2] + vz * dt;
      this.vel[w * 3] = vx; this.vel[w * 3 + 1] = vy; this.vel[w * 3 + 2] = vz;
      for (let c = 0; c < 3; c++) { this.base[w * 3 + c] = this.base[i * 3 + c]; this.col[w * 3 + c] = this.base[i * 3 + c] * fade; }
      this.life[w] = l; this.maxLife[w] = this.maxLife[i]; this.grav[w] = this.grav[i]; this.drag[w] = this.drag[i];
      w++;
    }
    this.n = w;
    const g = this.points.geometry;
    g.setDrawRange(0, w);
    g.attributes.position.needsUpdate = true;
    g.attributes.color.needsUpdate = true;
  }

  clear() { this.n = 0; this.points.geometry.setDrawRange(0, 0); }
}

const rnd = (a, b) => a + Math.random() * (b - a);
const C = (hex) => new THREE.Color(hex);

const GEO = {
  sphere: new THREE.SphereGeometry(1, 16, 12),
  ring: new THREE.RingGeometry(0.7, 1, 48),
  thinRing: new THREE.RingGeometry(0.92, 1, 64),
  pillar: new THREE.CylinderGeometry(1, 1, 1, 24, 1, true),
  cone: new THREE.ConeGeometry(0.14, 1, 6),
  slash: new THREE.RingGeometry(0.85, 1, 32, 1, -0.9, 1.8),
  torus: new THREE.TorusGeometry(1, 0.08, 8, 48),
  box: new THREE.BoxGeometry(1, 1, 1),
};
const addMat = (color, opacity = 1, side = THREE.FrontSide) =>
  new THREE.MeshBasicMaterial({ color, transparent: true, opacity, depthWrite: false, blending: THREE.AdditiveBlending, side });

export class Fx {
  // host: { scene, addMeshFx(mesh, t0, dur, update), camera }
  constructor(host) {
    this.host = host;
    this.small = new Particles(host.scene, 0.14, 4000);
    this.mid = new Particles(host.scene, 0.32, 2500);
    this.big = new Particles(host.scene, 0.8, 600);
    this.shakeT = 0;
    this.shakeAmp = 0;
  }

  update(dt) {
    this.small.update(dt);
    this.mid.update(dt);
    this.big.update(dt);
    this.shakeT = Math.max(0, this.shakeT - dt);
  }

  clear() { this.small.clear(); this.mid.clear(); this.big.clear(); this.shakeT = 0; }

  shake(amp, dur) { this.shakeAmp = Math.max(this.shakeT > 0 ? this.shakeAmp : 0, amp); this.shakeT = Math.max(this.shakeT, dur); }

  // camera offset for this frame
  shakeOffset() {
    if (this.shakeT <= 0) return null;
    const a = this.shakeAmp * Math.min(1, this.shakeT * 4);
    return new THREE.Vector3(rnd(-a, a), rnd(-a, a) * 0.6, rnd(-a, a));
  }

  colors(elem) { const [a, b] = PALETTE[elem] || PALETTE.beast; return [C(a), C(b)]; }

  burst(p, x, y, z, n, speed, colA, colB, life, grav = 0, drag = 1.5, up = 0) {
    for (let i = 0; i < n; i++) {
      const th = Math.random() * Math.PI * 2, ph = Math.acos(rnd(-1, 1));
      const s = speed * rnd(0.35, 1);
      p.add(x, y, z, Math.sin(ph) * Math.cos(th) * s, Math.abs(Math.cos(ph)) * s * 0.7 + up, Math.sin(ph) * Math.sin(th) * s,
        Math.random() < 0.5 ? colA : colB, life * rnd(0.6, 1.2), grav, drag);
    }
  }

  mesh(geo, color, t0, dur, place, update, opacity = 1, side) {
    const m = new THREE.Mesh(geo, addMat(color, opacity, side));
    place(m);
    this.host.addMeshFx(m, t0, dur, (k) => update(m, k));
    return m;
  }

  flash(t, x, y, z, color, size, dur = 0.25) {
    this.mesh(GEO.sphere, color, t, dur, (m) => m.position.set(x, y, z), (m, k) => { m.scale.setScalar(size * (0.4 + k)); m.material.opacity = 1 - k; });
  }

  groundRing(t, x, z, color, r, dur = 0.5, thin = false) {
    this.mesh(thin ? GEO.thinRing : GEO.ring, color, t, dur, (m) => { m.rotation.x = -Math.PI / 2; m.position.set(x, 0.18, z); },
      (m, k) => { m.scale.setScalar(0.2 + k * r); m.material.opacity = 1 - k; }, 1, THREE.DoubleSide);
  }

  pillar(t, x, z, color, r, h, dur = 0.6) {
    this.mesh(GEO.pillar, color, t, dur, (m) => m.position.set(x, h / 2, z),
      (m, k) => { const w = r * (k < 0.2 ? k / 0.2 : 1 - (k - 0.2) * 0.6); m.scale.set(w, h, w); m.material.opacity = (1 - k) * 0.7; }, 0.7, THREE.DoubleSide);
  }

  // jagged lightning from (x0,y0,z0) to (x1,y1,z1)
  bolt(t, a, b, color, dur = 0.25, width = 0.06) {
    const pts = [];
    const n = 8;
    for (let i = 0; i <= n; i++) {
      const k = i / n;
      const j = i === 0 || i === n ? 0 : 0.35;
      pts.push(new THREE.Vector3(a.x + (b.x - a.x) * k + rnd(-j, j), a.y + (b.y - a.y) * k + rnd(-j, j) * 0.5, a.z + (b.z - a.z) * k + rnd(-j, j)));
    }
    const geo = new THREE.TubeGeometry(new THREE.CatmullRomCurve3(pts, false, 'catmullrom', 0.1), 24, width, 5, false);
    const m = new THREE.Mesh(geo, addMat(color));
    this.host.addMeshFx(m, t, dur, (k) => { m.material.opacity = k < 0.15 ? 1 : 1 - k; if (k >= 0.98) geo.dispose(); });
  }

  slashAt(t, x, y, z, color, size, angle) {
    for (const d of [0, 1]) {
      this.mesh(GEO.slash, color, t + d * 0.08, 0.25, (m) => { m.position.set(x, y, z); m.rotation.set(-0.6, angle, (d ? -1 : 1) * 0.9 + Math.PI / 2); },
        (m, k) => { m.scale.setScalar(size * (0.6 + k * 0.6)); m.material.opacity = 1 - k; }, 1, THREE.DoubleSide);
    }
  }

  projectile(t, a, b, color, size, dur, arc = 0.4, trail = null) {
    const m = this.mesh(GEO.sphere, color, t, dur, () => {}, (mm, k) => {
      mm.position.set(a.x + (b.x - a.x) * k, a.y + (b.y - a.y) * k + Math.sin(Math.PI * k) * arc, a.z + (b.z - a.z) * k);
      mm.scale.setScalar(size);
      if (trail) trail(mm.position);
    });
    return m;
  }

  // ---------- effects called from the renderer ----------

  // caster wind-up: particles drawn in, a column of light
  cast(t, w, elem, stage) {
    const [a, b] = this.colors(elem);
    for (let i = 0; i < 24 + stage * 12; i++) {
      const th = Math.random() * Math.PI * 2, r = rnd(0.9, 1.6);
      this.mid.add(w.x + Math.cos(th) * r, rnd(0.2, 1.6), w.z + Math.sin(th) * r, -Math.cos(th) * r * 3, rnd(0, 1), -Math.sin(th) * r * 3, Math.random() < 0.5 ? a : b, 0.35, 0, 0);
    }
    this.pillar(t, w.x, w.z, a, 0.5 + stage * 0.15, 3 + stage, 0.45);
    this.groundRing(t, w.x, w.z, b, 1.4, 0.4, true);
    if (stage >= 2) this.shake(0.08, 0.25);
  }

  // single-target ult (nuke / stun / single drain), from caster world point a to target b
  strike(t, a, b, elem, stage, kind) {
    const [ca, cb] = this.colors(elem);
    const big = 1 + stage * 0.35;
    const A = { x: a.x, y: 0.9, z: a.z }, B = { x: b.x, y: 0.8, z: b.z };
    const impact = (delay) => {
      const T = t + delay;
      this.flash(T, B.x, B.y, B.z, ca, 0.9 * big);
      this.groundRing(T, B.x, B.z, cb, 1.6 * big);
      setTimeoutFx(delay, () => this.burst(this.mid, B.x, B.y, B.z, 30 * big, 4 * big, ca, cb, 0.6, 3));
    };
    switch (elem) {
      case 'thunder':
        for (let i = 0; i < 2 + stage; i++) this.bolt(t + i * 0.05, { x: B.x + rnd(-0.3, 0.3), y: 7, z: B.z + rnd(-0.3, 0.3) }, B, i ? cb : ca, 0.3, 0.05 + stage * 0.02);
        this.bolt(t, A, B, cb, 0.2, 0.03);
        impact(0.02);
        break;
      case 'holy':
        this.pillar(t + 0.05, B.x, B.z, ca, 0.7 * big, 8, 0.7);
        this.burst(this.small, B.x, 0.2, B.z, 40 * big, 2, ca, cb, 1, -2.5, 0.5);
        impact(0.1);
        break;
      case 'ice': {
        for (let i = 0; i < 4 + stage * 2; i++) {
          const off = { x: rnd(-0.3, 0.3), z: rnd(-0.3, 0.3) };
          const dir = Math.atan2(B.x - A.x, B.z - A.z);
          this.mesh(GEO.cone, i % 2 ? ca : cb, t + i * 0.04, 0.3, (m) => { m.rotation.set(Math.PI / 2, 0, -dir); },
            (m, k) => { m.position.set(A.x + (B.x + off.x - A.x) * k, 0.9, A.z + (B.z + off.z - A.z) * k); m.scale.set(1.2, 2.5, 1.2); m.material.opacity = 1 - k * 0.3; });
        }
        impact(0.3);
        break;
      }
      case 'dark': {
        this.projectile(t, A, B, ca, 0.35 * big, 0.4, 0.8, (p) => this.small.add(p.x, p.y, p.z, rnd(-0.5, 0.5), rnd(-0.5, 0.5), rnd(-0.5, 0.5), Math.random() < 0.5 ? ca : cb, 0.5));
        this.mesh(GEO.torus, cb, t + 0.4, 0.4, (m) => { m.position.set(B.x, 0.8, B.z); m.rotation.x = Math.PI / 2; },
          (m, k) => { m.scale.setScalar(1.6 * big * (1 - k)); m.material.opacity = 1 - k * 0.5; });
        impact(0.4);
        break;
      }
      case 'beast': {
        const ang = Math.atan2(B.x - A.x, B.z - A.z);
        this.slashAt(t, B.x, 0.9, B.z, ca, 1.1 * big, ang);
        impact(0.05);
        break;
      }
      default: { // fire, water, plant, wind, metal: element ball with a trail, or a laser for metal
        if (elem === 'metal') {
          const len = Math.hypot(B.x - A.x, B.z - A.z);
          this.mesh(GEO.pillar, ca, t, 0.35, (m) => { m.position.set((A.x + B.x) / 2, 0.9, (A.z + B.z) / 2); m.rotation.order = 'YXZ'; m.rotation.set(Math.PI / 2, Math.atan2(B.x - A.x, B.z - A.z), 0); },
            (m, k) => { const r = (0.12 + stage * 0.06) * (1 - k); m.scale.set(r, len, r); m.material.opacity = 1 - k; }, 1, THREE.DoubleSide);
          this.flash(t, A.x, 0.9, A.z, cb, 0.5);
          impact(0.02);
        } else {
          this.projectile(t, A, B, ca, 0.28 * big, 0.3, 0.6, (p) => {
            this.mid.add(p.x, p.y, p.z, rnd(-0.4, 0.4), rnd(-0.2, 0.6), rnd(-0.4, 0.4), Math.random() < 0.5 ? ca : cb, 0.45, elem === 'fire' ? -1.5 : 1.5);
          });
          impact(0.3);
        }
      }
    }
    if (kind === 'stun') {
      for (let i = 0; i < 5; i++) {
        const th = (i / 5) * Math.PI * 2;
        this.mesh(GEO.sphere, '#ffe94a', t + 0.3, 0.9, () => {}, (m, k) => {
          const a2 = th + k * 8;
          m.position.set(B.x + Math.cos(a2) * 0.45, 2.1, B.z + Math.sin(a2) * 0.45);
          m.scale.setScalar(0.08);
          m.material.opacity = 1 - k;
        });
      }
    }
    if (stage >= 2) this.shake(0.12, 0.3);
  }

  // area ult centered on a board point
  nova(t, c, elem, stage, radius) {
    const [ca, cb] = this.colors(elem);
    const R = (radius + 0.5) * 1.7;
    const big = 1 + stage * 0.35;
    const meteor = /fire|dark/.test(elem) && stage >= 1;
    const land = meteor ? 0.45 : 0;
    if (meteor) {
      const from = { x: c.x - 4, y: 9, z: c.z - 3 };
      this.projectile(t, from, { x: c.x, y: 0.3, z: c.z }, ca, 0.6 * big, land, 0, (p) => {
        this.big.add(p.x, p.y, p.z, rnd(-0.5, 0.5), rnd(0, 1), rnd(-0.5, 0.5), Math.random() < 0.5 ? ca : cb, 0.5);
      });
    }
    const T = t + land;
    after(land, () => {
      this.burst(this.mid, c.x, 0.3, c.z, 60 * big, 6 * big, ca, cb, 0.8, 4, 1.2, 2);
      this.burst(this.big, c.x, 0.5, c.z, 12 * big, 3, ca, cb, 0.6, -1, 1.5);
    });
    this.flash(T, c.x, 0.4, c.z, ca, R * 0.7, 0.35);
    this.groundRing(T, c.x, c.z, ca, R, 0.55);
    this.groundRing(T + 0.08, c.x, c.z, cb, R * 1.2, 0.6, true);
    if (elem === 'thunder') for (let i = 0; i < 4 + stage * 2; i++) {
      const p = { x: c.x + rnd(-R, R) * 0.6, y: 0.2, z: c.z + rnd(-R, R) * 0.6 };
      this.bolt(T + i * 0.04, { x: p.x, y: 7, z: p.z }, p, i % 2 ? ca : cb, 0.3, 0.06);
    }
    if (elem === 'ice' || elem === 'plant') for (let i = 0; i < 8 + stage * 4; i++) {
      const th = Math.random() * Math.PI * 2, r = rnd(0.2, R * 0.6);
      const x = c.x + Math.cos(th) * r, z = c.z + Math.sin(th) * r;
      this.mesh(GEO.cone, i % 2 ? ca : cb, T + rnd(0, 0.15), 0.7, (m) => m.position.set(x, 0, z),
        (m, k) => { const h = k < 0.3 ? k / 0.3 : 1; m.scale.set(2, 1.4 * h * big, 2); m.position.y = 0.7 * h * big; m.material.opacity = k > 0.7 ? (1 - k) / 0.3 : 1; });
    }
    if (elem === 'holy') for (let i = 0; i < 4; i++) {
      const th = (i / 4) * Math.PI * 2 + Math.PI / 4;
      this.pillar(T + i * 0.06, c.x + Math.cos(th) * R * 0.4, c.z + Math.sin(th) * R * 0.4, ca, 0.35, 7, 0.6);
    }
    if (elem === 'wind' || elem === 'water' || elem === 'dark') {
      for (let i = 0; i < 50 * big; i++) {
        const th = Math.random() * Math.PI * 2, r = rnd(0.3, R * 0.6);
        this.mid.add(c.x + Math.cos(th) * r, rnd(0.1, 0.5), c.z + Math.sin(th) * r, -Math.sin(th) * 5, rnd(1.5, 4), Math.cos(th) * 5, Math.random() < 0.5 ? ca : cb, 0.7, 0, 1);
      }
    }
    this.shake(0.1 + stage * 0.06, 0.35);
  }

  // slam around the caster: shockwave and flying debris
  slam(t, c, elem, stage, radius) {
    const [ca, cb] = this.colors(elem);
    const R = (radius + 0.5) * 1.7;
    this.mesh(GEO.torus, ca, t, 0.45, (m) => { m.rotation.x = Math.PI / 2; m.position.set(c.x, 0.25, c.z); },
      (m, k) => { m.scale.setScalar(0.3 + k * R); m.material.opacity = 1 - k; });
    this.groundRing(t, c.x, c.z, cb, R * 1.1, 0.5);
    const rock = C('#9a8a70');
    for (let i = 0; i < 30 + stage * 15; i++) {
      const th = Math.random() * Math.PI * 2, s = rnd(2, 5);
      this.mid.add(c.x, 0.2, c.z, Math.cos(th) * s, rnd(3, 6), Math.sin(th) * s, i % 3 ? rock : ca, 0.9, 12, 0.5);
    }
    this.burst(this.big, c.x, 0.3, c.z, 10, 2, ca, cb, 0.5, -0.5);
    this.shake(0.18 + stage * 0.06, 0.4);
  }

  // one barrage shot
  missile(t, a, b, elem) {
    const [ca, cb] = this.colors(elem);
    const A = { x: a.x, y: 1, z: a.z }, B = { x: b.x, y: 0.8, z: b.z };
    this.projectile(t, A, B, ca, 0.15, 0.32, 1.4, (p) => this.small.add(p.x, p.y, p.z, rnd(-0.3, 0.3), rnd(0, 0.5), rnd(-0.3, 0.3), cb, 0.4));
    after(0.32, () => this.burst(this.mid, B.x, B.y, B.z, 14, 3, ca, cb, 0.4, 2));
    this.flash(t + 0.32, B.x, B.y, B.z, ca, 0.45, 0.2);
  }

  // assassin dash: streak from the old spot and a cross slash
  dash(t, from, to, elem, stage) {
    const [ca, cb] = this.colors(elem);
    for (let i = 0; i < 30; i++) {
      const k = Math.random();
      this.mid.add(from.x + (to.x - from.x) * k, rnd(0.4, 1.2), from.z + (to.z - from.z) * k, 0, rnd(0, 0.5), 0, Math.random() < 0.5 ? ca : cb, rnd(0.2, 0.45));
    }
    this.slashAt(t + 0.15, to.x, 0.9, to.z, ca, 1.2 + stage * 0.3, Math.atan2(to.x - from.x, to.z - from.z));
    after(0.15, () => this.burst(this.mid, to.x, 0.9, to.z, 25, 4, ca, cb, 0.5, 3));
    if (stage >= 2) this.shake(0.1, 0.25);
  }

  heal(t, w) {
    const a = C('#7dff9a'), b = C('#fff6a0');
    for (let i = 0; i < 18; i++) {
      const th = Math.random() * Math.PI * 2, r = rnd(0.1, 0.6);
      this.mid.add(w.x + Math.cos(th) * r, rnd(0.1, 0.6), w.z + Math.sin(th) * r, 0, rnd(1.2, 2.4), 0, Math.random() < 0.6 ? a : b, rnd(0.6, 1));
    }
    this.groundRing(t, w.x, w.z, '#7dff9a', 1.2, 0.5, true);
  }

  shield(t, w, stage) {
    this.mesh(GEO.sphere, '#bfe4ff', t, 0.9, (m) => m.position.set(w.x, 0.9, w.z), (m, k) => {
      m.scale.setScalar(0.95 + 0.15 * Math.sin(k * 9) * (1 - k) + stage * 0.1);
      m.material.opacity = 0.35 * (1 - k);
    }, 0.35);
    this.mesh(GEO.torus, '#ffffff', t, 0.6, (m) => { m.rotation.x = Math.PI / 2; m.position.set(w.x, 0.2, w.z); },
      (m, k) => { m.scale.setScalar(0.6 + k * 0.8); m.position.y = 0.2 + k * 1.4; m.material.opacity = 1 - k; });
    this.burst(this.small, w.x, 0.9, w.z, 30, 2.5, C('#bfe4ff'), C('#ffffff'), 0.6, 0, 2);
  }

  // life drained from each target flows back to the caster
  drainFlow(from, to) {
    const a = C('#ff3a6a'), b = C('#ffb3c6');
    for (let i = 0; i < 16; i++) {
      const s = rnd(2.5, 4);
      const dx = to.x - from.x, dz = to.z - from.z, d = Math.hypot(dx, dz) || 1;
      this.mid.add(from.x + rnd(-0.2, 0.2), rnd(0.6, 1.1), from.z + rnd(-0.2, 0.2), (dx / d) * s, rnd(0, 0.6), (dz / d) * s, Math.random() < 0.5 ? a : b, Math.min(0.8, d / s), 0, 0);
    }
  }
}

// small helper: run a particle spawn after a delay in real time
function after(sec, fn) { if (sec <= 0) fn(); else setTimeout(fn, sec * 1000); }
const setTimeoutFx = after;
