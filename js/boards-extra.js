// More arena boards (js/boards.js registers them with the first ten). Each theme uses the same spec as boards.js,
// plus these optional parts:
//   surface(K)  -> { tile, slot, ped } small canvas textures for the hex tiles, bench slots and item pedestals
//   look        -> { slotRough, slotMetal, pedRough, pedMetal }
//   props(ctx)  ctx also has: on(name, fn) for 'combat' | 'planning' | 'win' | 'stage' | 'evolve',
//               state = { phase, stage, won, evo }, light() / fog() for live lighting and weather, preview (store image).
// Everything here is visual only: boards read the game state and never change it.
export function extraThemes(K) {
  const { THREE, DT, C, std, canvasTex, speckle, softDot, part, merge, setInst, inst, mesh, rimSpots, farSpots, sea, plain, particles, pineGeo, LAYOUT } = K;
  const { cellWorld } = LAYOUT;
  const TAU = Math.PI * 2;
  const basic = (o) => new THREE.MeshBasicMaterial(o);
  const glow = (color, opacity = 0.5, o = {}) => basic({ color, transparent: true, opacity, depthWrite: false, blending: THREE.AdditiveBlending, ...o });
  const lerp = (a, b, k) => a + (b - a) * k;
  const approach = (cur, target, rate, dt) => cur + (target - cur) * Math.min(1, rate * dt);

  // ---------- shapes ----------
  // The ground the board stands on. sides: 4 square, 6 hex, 8 octagon, 40+ round. Top surface at y = -0.02.
  function platform(ctx, { sides = 48, r = 12.6, r2 = r + 0.9, h = 1.4, top = '#777', side = '#555', rot = 0, y = 0, flat = sides < 14 } = {}) {
    const topMat = top.isMaterial ? top : top.isTexture ? std({ map: top, roughness: 0.95 }) : std({ color: top, roughness: 0.9 });
    const sideMat = side.isMaterial ? side : std({ color: side, roughness: 1, flatShading: flat });
    const geo = new THREE.CylinderGeometry(r, r2, h, sides);
    geo.rotateY(rot);
    return mesh(ctx, geo, [sideMat, topMat, topMat], { y: y - 0.02 - h / 2, receive: true });
  }
  // jagged rock hanging under a floating platform
  function rockUnder(ctx, { r = 13.5, h = 10, seg = 12, color = '#3a3040', y = -1.42, seed = 3, rot = 0 }) {
    const geo = new THREE.ConeGeometry(r, h, seg, 5, true);
    geo.rotateX(Math.PI);
    geo.rotateY(rot);
    const pos = geo.attributes.position;
    const rng = DT.makeRng(seed);
    const off = new Map();
    for (let i = 0; i < pos.count; i++) {
      const x = pos.getX(i), yy = pos.getY(i), z = pos.getZ(i);
      if (yy > h / 2 - 0.01) continue;
      const key = `${x.toFixed(2)},${yy.toFixed(2)},${z.toFixed(2)}`;
      if (!off.has(key)) off.set(key, [0.75 + rng() * 0.5, (rng() - 0.5) * 0.8]);
      const [k, dy] = off.get(key);
      pos.setXYZ(i, x * k, yy + dy, z * k);
    }
    geo.computeVertexNormals();
    return mesh(ctx, geo, std({ color, roughness: 1, flatShading: true }), { y: y - h / 2 });
  }
  // outlines on every hex tile, so the grid reads on any surface
  function hexEdges(ctx, { mine = '#8fd0ff', enemy = '#ff8fa8', opacity = 0.5, inner = 0.85, outer = 0.94, additive = false } = {}) {
    const ring = new THREE.RingGeometry(inner, outer, 6);
    ring.rotateZ(Math.PI / 2);
    ring.rotateX(-Math.PI / 2);
    const items = DT.CELLS.map(({ c, r }) => { const p = cellWorld(c, r); return { x: p.x, y: 0.147, z: p.z, color: r >= 4 ? mine : enemy }; });
    const mat = basic({ color: '#ffffff', transparent: true, opacity, depthWrite: false, blending: additive ? THREE.AdditiveBlending : THREE.NormalBlending });
    return inst(ctx, ring, mat, items, { shadow: false });
  }
  // evenly spaced spots on an arc; a0..a1 in radians (back of the island is PI..2PI)
  const arc = (n, r, a0, a1, jitter = 0, rng = null) => Array.from({ length: n }, (_, i) => {
    const a = a0 + (a1 - a0) * (n === 1 ? 0.5 : i / (n - 1)) + (rng ? (rng() - 0.5) * jitter : 0);
    return { x: Math.cos(a) * r, z: Math.sin(a) * r, a };
  });
  // instanced mesh whose items fn(item, i, t, dt) moves every frame
  function anim(ctx, geo, mat, items, fn, opts = {}) {
    const m = inst(ctx, geo, mat, items, { moving: true, shadow: false, ...opts });
    ctx.tick((dt, t) => {
      for (let i = 0; i < items.length; i++) { fn(items[i], i, t, dt); setInst(m, i, items[i]); }
      m.instanceMatrix.needsUpdate = true;
    });
    return m;
  }

  // ---------- textures (all tiny, drawn once) ----------
  function hexPath(g, cx, cy, r) {
    g.beginPath();
    for (let i = 0; i < 6; i++) { const a = (i / 6) * TAU; const x = cx + Math.cos(a) * r, y = cy + Math.sin(a) * r; if (i) g.lineTo(x, y); else g.moveTo(x, y); }
    g.closePath();
  }
  // a light hex tile face: base fill, a darker bevel line near the rim, then extra drawing
  const tileTex = (base, line, draw, lw = 6) => canvasTex(128, 128, (g, w, h) => {
    g.fillStyle = base; g.fillRect(0, 0, w, h);
    if (draw) draw(g, w, h);
    g.strokeStyle = line; g.lineWidth = lw; hexPath(g, 64, 64, 58); g.stroke();
  });
  const bricks = (base, mortar, { w = 128, h = 128, bw = 32, bh = 16, seed = 5, vary = 0.12, repeat = 4 } = {}) => {
    const rng = DT.makeRng(seed);
    return canvasTex(w, h, (g) => {
      g.fillStyle = mortar; g.fillRect(0, 0, w, h);
      const c = C(base);
      for (let y = 0; y < h; y += bh) for (let x = -((y / bh) % 2) * bw / 2; x < w; x += bw) {
        const k = 1 + (rng() - 0.5) * vary * 2;
        g.fillStyle = `rgb(${Math.min(255, c.r * 255 * k) | 0},${Math.min(255, c.g * 255 * k) | 0},${Math.min(255, c.b * 255 * k) | 0})`;
        g.fillRect(x + 1.5, y + 1.5, bw - 3, bh - 3);
      }
    }, repeat);
  };
  const stripes = (a, b, n = 8, repeat = 1, vertical = true) => canvasTex(64, 64, (g) => {
    for (let i = 0; i < n; i++) { g.fillStyle = i % 2 ? b : a; if (vertical) g.fillRect((i * 64) / n, 0, 64 / n + 1, 64); else g.fillRect(0, (i * 64) / n, 64, 64 / n + 1); }
  }, repeat);
  const vgrad = (stops) => canvasTex(4, 64, (g, w, h) => {
    const gr = g.createLinearGradient(0, 0, 0, h);
    stops.forEach((c, i) => gr.addColorStop(i / (stops.length - 1), c));
    g.fillStyle = gr; g.fillRect(0, 0, w, h);
  });

  // ---------- reusable effects ----------
  // lightning: a few jagged bolts made once; strike() shows one for a moment and flashes the light
  function lightning(ctx, { color = '#d8c8ff', spots, height = 9, flash = 1.6, flashColor = '#e8e0ff' }) {
    const rng = DT.makeRng(91);
    const mat = glow(color, 1, { fog: false });
    const bolts = [0, 1, 2].map(() => {
      const path = new THREE.CurvePath();
      let p = new THREE.Vector3(0, height, 0);
      const parts = [];
      for (let i = 1; i <= 7; i++) {
        const q = new THREE.Vector3((rng() - 0.5) * 1.6, height * (1 - i / 7), (rng() - 0.5) * 1.6);
        path.add(new THREE.LineCurve3(p, q));
        if (i === 3) {
          const b = new THREE.CurvePath();
          b.add(new THREE.LineCurve3(q, q.clone().add(new THREE.Vector3(1.4, -2, 0.4))));
          parts.push(new THREE.TubeGeometry(b, 3, 0.04, 4));
        }
        p = q;
      }
      parts.push(new THREE.TubeGeometry(path, 24, 0.07, 4));
      const m = new THREE.Mesh(merge(parts), mat);
      m.visible = false;
      m.frustumCulled = false;
      ctx.g.add(m);
      return m;
    });
    const lit = ctx.light();
    lit.flashColor.set(flashColor);
    let on = null, t0 = 0;
    ctx.tick((dt, t) => {
      if (!on) return;
      const k = t - t0;
      on.visible = k < 0.32 && (k < 0.08 || k > 0.14);
      if (k > 0.35) { on.visible = false; on = null; }
    });
    return {
      strike(t) {
        if (on) on.visible = false;
        on = bolts[(rng() * bolts.length) | 0];
        const s = spots[(rng() * spots.length) | 0];
        on.position.set(s.x, s.y || 0, s.z);
        on.rotation.y = rng() * TAU;
        t0 = t;
        lit.flash = flash;
      },
    };
  }
  // confetti that falls over the board when fire() is called
  function confetti(ctx, { n = 220, colors = ['#ff4d6d', '#ffd23f', '#3ae374', '#3a86ff', '#c77dff', '#ffffff'] } = {}) {
    const rng = DT.makeRng(77);
    const geo = new THREE.PlaneGeometry(0.14, 0.22);
    const items = Array.from({ length: n }, (_, i) => ({ x: 0, y: -50, z: 0, color: colors[i % colors.length], vx: 0, vy: 0, vz: 0, spin: 0 }));
    const m = inst(ctx, geo, basic({ color: '#ffffff', side: THREE.DoubleSide }), items, { moving: true, shadow: false });
    m.visible = false;
    let left = 0;
    ctx.tick((dt) => {
      if (left <= 0) return;
      left -= dt;
      for (let i = 0; i < n; i++) {
        const it = items[i];
        it.vy = Math.max(-2.2, it.vy - 6 * dt);
        it.x += it.vx * dt; it.y += it.vy * dt; it.z += it.vz * dt;
        it.vx *= 0.985; it.vz *= 0.985;
        it.rx += it.spin * dt; it.rz += it.spin * 0.7 * dt;
        setInst(m, i, it);
      }
      m.instanceMatrix.needsUpdate = true;
      if (left <= 0) m.visible = false;
    });
    return {
      fire(from = null) {
        for (const it of items) {
          const o = from ? from[(rng() * from.length) | 0] : { x: (rng() - 0.5) * 16, z: -6 + rng() * 12 };
          Object.assign(it, { x: o.x + (rng() - 0.5) * 1.5, y: (o.y ?? 8) + rng() * 2, z: o.z + (rng() - 0.5) * 1.5, vx: (rng() - 0.5) * 6, vy: 2 + rng() * 5, vz: (rng() - 0.5) * 6, rx: rng() * 6, rz: rng() * 6, spin: 4 + rng() * 8 });
        }
        left = 5;
        m.visible = true;
      },
    };
  }
  // firework bursts: launch(x, z) pops a ball of colored sparks
  function fireworks(ctx, { per = 70, max = 4, colors = ['#ff5a7a', '#ffd84a', '#5affc8', '#6ab0ff', '#ff8af0'] } = {}) {
    const rng = DT.makeRng(55);
    const bursts = Array.from({ length: max }, () => {
      const pos = new Float32Array(per * 3), vel = new Float32Array(per * 3);
      const geo = new THREE.BufferGeometry();
      geo.setAttribute('position', new THREE.BufferAttribute(pos, 3));
      const mat = new THREE.PointsMaterial({ size: 0.32, map: softDot(), transparent: true, depthWrite: false, blending: THREE.AdditiveBlending, fog: false });
      const p = new THREE.Points(geo, mat);
      p.frustumCulled = false;
      p.visible = false;
      ctx.g.add(p);
      return { p, pos, vel, mat, t0: -9, o: new THREE.Vector3() };
    });
    let next = 0;
    ctx.tick((dt, t) => {
      for (const b of bursts) {
        const k = t - b.t0;
        if (k > 2.2) { b.p.visible = false; continue; }
        b.p.visible = true;
        for (let i = 0; i < per; i++) {
          b.pos[i * 3] = b.o.x + b.vel[i * 3] * k;
          b.pos[i * 3 + 1] = b.o.y + b.vel[i * 3 + 1] * k - 1.6 * k * k;
          b.pos[i * 3 + 2] = b.o.z + b.vel[i * 3 + 2] * k;
        }
        b.p.geometry.attributes.position.needsUpdate = true;
        b.mat.opacity = Math.max(0, 1 - k / 2.2);
      }
    });
    return {
      launch(t, x, y, z) {
        const b = bursts[next++ % max];
        b.t0 = t;
        b.o.set(x, y, z);
        b.mat.color.set(colors[(rng() * colors.length) | 0]);
        const sp = 2.2 + rng() * 1.2;
        for (let i = 0; i < per; i++) {
          const u = rng() * 2 - 1, a = rng() * TAU, s = Math.sqrt(1 - u * u);
          b.vel[i * 3] = Math.cos(a) * s * sp; b.vel[i * 3 + 1] = u * sp; b.vel[i * 3 + 2] = Math.sin(a) * s * sp;
        }
      },
    };
  }
  // a crowd of simple figures on rings; returns the items so a board can make them jump
  function crowdGeo() {
    return merge([part(new THREE.CylinderGeometry(0.13, 0.17, 0.42, 6), '#ffffff', { y: 0.21 }), part(new THREE.SphereGeometry(0.12, 6, 4), '#ffe0c8', { y: 0.53 })]);
  }
  // a hanging banner shape: top edge at y = 0
  function bannerGeo(w, h) { const g = new THREE.PlaneGeometry(w, h); g.translate(0, -h / 2, 0); return g; }

  // ---------- themes ----------
  return [
    // ===== The Dark Masters' Spiral Mountain =====
    {
      id: 'spiral-mountain', name: 'Spiral Mountain', rarity: 'legendary', price: 2200, icon: '🌀',
      desc: 'A shard of the Dark Masters\' mountain hangs in a violet void. Rocks spiral around it, faster in combat, with purple lightning.',
      sky: ['#12001e', '#3a0a4a', '#7a2a5a'], fog: ['#2a0a36', 30, 75], hemi: ['#c49aff', '#1a0820', 1.15], sun: ['#ff9ad0', 2.1],
      slab: '#160e1e', benchSlab: '#22162a', slot: '#3a2a4a', ped: '#55387a',
      tiles: { mine: '#3d63b8', enemy: '#ad3762', enemyPlan: '#5e2644', rough: 0.45, metal: 0.15 },
      look: { pedMetal: 0.5, pedRough: 0.3 },
      surface: () => ({ tile: tileTex('#e6e0ee', 'rgba(60,20,80,0.55)', (g) => { g.strokeStyle = 'rgba(120,60,160,0.35)'; g.lineWidth = 2; for (let i = 0; i < 3; i++) { g.beginPath(); g.arc(64, 64, 14 + i * 14, i, i + 4); g.stroke(); } }) }),
      props(ctx) {
        const top = speckle('#3e3046', ['#5a4566', '#2a2030', '#7a4a8a'], { seed: 101, n: 1600 });
        platform(ctx, { sides: 11, r: 11.9, r2: 12.4, h: 1.3, top, side: '#2c2234', rot: 0.2 });
        rockUnder(ctx, { r: 12.4, h: 11, seg: 11, color: '#271d2e', seed: 4, rot: 0.2, y: -1.32 });
        hexEdges(ctx, { mine: '#7fb0ff', enemy: '#ff5fa8', opacity: 0.6, additive: true });
        const rng = ctx.rng;
        // twisted spires on the rim
        const spire = merge([part(new THREE.ConeGeometry(0.55, 4.2, 5), '#3a2a48', { y: 2.1 }), part(new THREE.ConeGeometry(0.3, 2.2, 5), '#4a3058', { x: 0.45, y: 1.1, rz: -0.35 })]);
        inst(ctx, spire, std({ vertexColors: true, flatShading: true, roughness: 0.9 }), rimSpots(rng, 80, 9.8, 11.3, true).slice(0, 13).map((p) => ({ x: p.x, z: p.z, ry: rng() * 6, s: 0.6 + rng() * 0.7, rz: (rng() - 0.5) * 0.25 })));
        // the spiral of rocks: a helix around the island, turning
        const rocks = [];
        for (let i = 0; i < 90; i++) rocks.push({ k: i / 90, a0: (i / 90) * TAU * 3.5, rad: 12.4 + (i % 5) * 0.45, s: 0.35 + rng() * 0.9, sp: rng() * 6, phase: 0 });
        const sp = { v: 1 };
        anim(ctx, new THREE.DodecahedronGeometry(1, 0), std({ color: '#5a4468', flatShading: true, roughness: 0.9, emissive: '#2a0a3a', emissiveIntensity: 0.6 }), rocks, (r, i, t) => {
          const a = r.a0 + r.phase;
          r.x = Math.cos(a) * r.rad; r.z = Math.sin(a) * r.rad; r.y = -3 + r.k * 8;
          r.rx = t * 0.4 + r.sp; r.ry = t * 0.3 + r.sp;
        });
        ctx.tick((dt, t) => {
          sp.v = approach(sp.v, ctx.state.phase === 'combat' ? 3.2 : 1, 2, dt);
          for (const r of rocks) r.phase = (r.phase || 0) + dt * 0.07 * sp.v;
        });
        // the four Dark Masters' crystals, pulsing
        const cry = [{ c: '#4a8aff' }, { c: '#ff3a3a' }, { c: '#7aff6a' }, { c: '#c86aff' }].map((o, i) => { const a = Math.PI * (1.08 + i * 0.28); return { x: Math.cos(a) * 10.6, y: 1.5, z: Math.sin(a) * 10.6, color: o.c, ph: i * 1.6 }; });
        anim(ctx, new THREE.OctahedronGeometry(0.5, 0), basic({ color: '#ffffff' }), cry, (c, i, t) => { c.y = 1.6 + Math.sin(t * 1.4 + c.ph) * 0.25; c.ry = t * 1.2; c.sy = 1.6; });
        const halo = cry.map((c) => ({ ...c, s: 1.2 }));
        anim(ctx, new THREE.SphereGeometry(0.7, 12, 8), glow('#ffffff', 0.25), halo, (h, i, t) => { h.y = 1.6 + Math.sin(t * 1.4 + h.ph) * 0.25; h.s = 1.1 + 0.25 * Math.sin(t * 3 + h.ph); });
        // small islets drifting far below
        const islets = farSpots(rng, 10, 18, 30).map((p) => ({ x: p.x, y: -8 - rng() * 8, z: p.z, s: 1 + rng() * 2, ph: rng() * 6 }));
        anim(ctx, new THREE.ConeGeometry(1.2, 2.4, 6).rotateX(Math.PI), std({ color: '#33263c', flatShading: true }), islets, (o, i, t) => { o.y += Math.sin(t * 0.5 + o.ph) * 0.004; o.ry = t * 0.05 + o.ph; });
        particles(ctx, 140, { color: '#d58aff', size: 0.14, v: [0, 0.6, 0], wobble: 0.4, opacity: 0.8, additive: true, box: [-17, 17, -3, 9, -15, 11] });
        const bolt = lightning(ctx, { color: '#e0a8ff', spots: arc(7, 12.6, Math.PI * 0.95, Math.PI * 2.05), flash: 1.4, flashColor: '#d8b0ff' });
        let next = 1.5;
        ctx.tick((dt, t) => { if (ctx.state.phase === 'combat' && t > next) { bolt.strike(t); next = t + 1.2 + ctx.rng() * 2.5; } });
        ctx.on('combat', (t) => bolt.strike(t));
      },
    },

    // ===== Myotismon's castle =====
    {
      id: 'vampire-castle', name: "Myotismon's Castle", rarity: 'epic', price: 1000, icon: '🦇',
      desc: 'Flagstone courtyard on a cliff, a blood moon rising behind the walls. Bats swarm when the fight starts and the moon reddens each stage.',
      sky: ['#050108', '#1a0410', '#3a0810'], fog: ['#1a0610', 28, 62], hemi: ['#e8a0b0', '#20080c', 1.3], sun: ['#ffc0b0', 2.1],
      slab: '#141014', benchSlab: '#1e1416', slot: '#3a2420', ped: '#2a2a30',
      tiles: { mine: '#3a62b0', enemy: '#a8303e', enemyPlan: '#561c26', rough: 0.7 },
      look: { pedMetal: 0.6, pedRough: 0.4 },
      surface: () => ({
        tile: tileTex('#f2ecea', 'rgba(40,20,20,0.5)', (g) => { g.strokeStyle = 'rgba(60,40,40,0.22)'; g.lineWidth = 2; g.beginPath(); g.moveTo(20, 40); g.lineTo(60, 70); g.lineTo(80, 110); g.moveTo(60, 70); g.lineTo(108, 52); g.stroke(); }),
        slot: bricks('#4a2a22', '#1a0e0c', { bw: 64, bh: 32, repeat: 1, seed: 3 }),
      }),
      props(ctx) {
        const flag = bricks('#6a6470', '#2a242c', { bw: 40, bh: 40, seed: 202, vary: 0.2, repeat: 7 });
        platform(ctx, { sides: 8, r: 12.6, r2: 13, h: 1.4, top: flag, side: '#2a2428', rot: Math.PI / 8 });
        rockUnder(ctx, { r: 13, h: 16, seg: 8, color: '#1e181e', seed: 9, rot: Math.PI / 8 });
        hexEdges(ctx, { mine: '#9ab8ff', enemy: '#ff5a6a', opacity: 0.4 });
        // the blood moon behind the back wall, seen over the cliff edge
        const moonMat = basic({ color: '#ff8a6a', fog: false });
        // the top-left corner of the view looks past the cliff into the sky, so the moon sits there
        const moon = mesh(ctx, new THREE.CircleGeometry(4.2, 40), moonMat, { x: -17, y: -4.5, z: -20 });
        moon.lookAt(0, 16.5, 17.3);
        const halo = mesh(ctx, new THREE.CircleGeometry(7, 40), glow('#ff3a2a', 0.22, { fog: false }), { x: -17.05, y: -4.55, z: -20.1 });
        halo.lookAt(0, 16.5, 17.3);
        const red = { k: 0 };
        ctx.tick((dt, t) => {
          red.k = approach(red.k, Math.min(1, (ctx.state.stage - 1) / 6), 1, dt);
          moonMat.color.setRGB(1, lerp(0.42, 0.08, red.k), lerp(0.3, 0.06, red.k));
          halo.material.opacity = 0.18 + 0.12 * red.k + 0.03 * Math.sin(t * 1.3);
        });
        // castle wall with crenellations along the back, towers at the corners
        const wall = [];
        const segs = arc(11, 12.1, Math.PI * 1.42, Math.PI * 2.05);
        for (const s of segs) {
          wall.push(part(new THREE.BoxGeometry(2.6, 1.6, 0.8), '#3a3238', { x: s.x, y: 0.8, z: s.z, ry: -s.a - Math.PI / 2 }));
          for (const o of [-0.9, 0, 0.9]) {
            const ox = Math.cos(s.a + Math.PI / 2) * o, oz = Math.sin(s.a + Math.PI / 2) * o;
            wall.push(part(new THREE.BoxGeometry(0.5, 0.5, 0.8), '#433a40', { x: s.x + ox, y: 1.85, z: s.z + oz, ry: -s.a - Math.PI / 2 }));
          }
        }
        for (const s of [arc(1, 11.9, Math.PI * 1.4, Math.PI * 1.4)[0], arc(1, 11.9, Math.PI * 1.78, Math.PI * 1.78)[0], { x: 11.9, z: 1.2 }, { x: -11.4, z: -1.5 }]) {
          wall.push(part(new THREE.CylinderGeometry(1.2, 1.35, 5, 10), '#3e353c', { x: s.x, y: 2.5, z: s.z }));
          wall.push(part(new THREE.ConeGeometry(1.5, 2.6, 10), '#2a1a2e', { x: s.x, y: 6.3, z: s.z }));
          wall.push(part(new THREE.BoxGeometry(0.3, 0.6, 0.1), '#ffc070', { x: s.x - Math.sign(s.x) * 0.2, y: 3.2, z: s.z + 1.2 }));
        }
        mesh(ctx, merge(wall), std({ vertexColors: true, roughness: 0.95 }), { shadow: true, receive: true });
        // lit gothic windows: glow boxes on the wall
        inst(ctx, new THREE.BoxGeometry(0.35, 0.7, 0.1), basic({ color: '#ffb050' }), segs.filter((_, i) => i % 2).map((s) => ({ x: s.x * 0.965, y: 0.9, z: s.z * 0.965, ry: -s.a - Math.PI / 2 })), { shadow: false });
        // coffins and candelabras on the sides
        const coffin = new THREE.CylinderGeometry(0.5, 0.36, 0.3, 6);
        coffin.scale(0.75, 1, 2.2);
        inst(ctx, coffin, std({ color: '#3a1e1a', roughness: 0.6 }), [{ x: -11, y: 0.15, z: -2, ry: 0.4 }, { x: 11.2, y: 0.15, z: -3.2, ry: -0.3 }, { x: 10.6, y: 0.15, z: 1.8, ry: 0.2 }]);
        const cand = rimSpots(ctx.rng, 60, 10, 11.8).filter((p) => p.z > -8).slice(0, 6);
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.05, 0.12, 1.5, 6), '#8a7a50', { y: 0.75 }), part(new THREE.CylinderGeometry(0.4, 0.4, 0.05, 8), '#8a7a50', { y: 1.4 })]), std({ vertexColors: true, metalness: 0.6, roughness: 0.4 }), cand.map((p) => ({ x: p.x, z: p.z })));
        const flames = [];
        cand.forEach((p, i) => { for (const o of [-0.35, 0, 0.35]) flames.push({ x: p.x + o, y: 1.62, z: p.z, ph: i * 2 + o * 5 }); });
        anim(ctx, new THREE.ConeGeometry(0.06, 0.2, 6), basic({ color: '#ffc060' }), flames, (f, i, t) => { const k = 1 + 0.25 * Math.sin(t * 14 + f.ph); f.sy = k; f.sx = f.sz = 1 / k; });
        // bats: flapping by squashing the wings, circling; they swarm in after combat starts
        const wing = new THREE.Shape();
        wing.moveTo(0, 0.05); wing.lineTo(0.5, 0.18); wing.lineTo(0.4, 0.02); wing.lineTo(0.3, 0.06); wing.lineTo(0.2, -0.04); wing.lineTo(0, -0.06);
        wing.lineTo(-0.2, -0.04); wing.lineTo(-0.3, 0.06); wing.lineTo(-0.4, 0.02); wing.lineTo(-0.5, 0.18); wing.closePath();
        const batGeo = new THREE.ShapeGeometry(wing);
        batGeo.rotateX(-Math.PI / 2);
        const bats = Array.from({ length: 46 }, (_, i) => ({ a0: ctx.rng() * TAU, rad: 9 + ctx.rng() * 6, h: 2 + ctx.rng() * 4, sp: (0.35 + ctx.rng() * 0.4) * (i % 3 ? 1 : -1), ph: ctx.rng() * 6, s: 0.7 + ctx.rng() * 0.6 }));
        const swarm = { k: 0, until: -1 };
        ctx.on('combat', (t) => { swarm.until = t + 5; });
        ctx.tick((dt, t) => { swarm.k = approach(swarm.k, t < swarm.until ? 1 : 0, 1.5, dt); });
        anim(ctx, batGeo, basic({ color: '#0a0608', side: THREE.DoubleSide }), bats, (b, i, t) => {
          const a = b.a0 + t * b.sp * (1 + swarm.k * 1.8);
          const rad = lerp(b.rad, 7.5 + (i % 5) * 0.4, swarm.k);
          b.x = Math.cos(a) * rad; b.z = Math.sin(a) * rad * 0.85 - 1.5; b.y = lerp(b.h, 3 + Math.sin(i) * 0.8, swarm.k) + Math.sin(t * 2 + b.ph) * 0.3;
          b.ry = -a + (b.sp > 0 ? 0 : Math.PI); b.sx = b.s * (0.35 + 0.65 * Math.abs(Math.sin(t * 13 + b.ph))); b.sz = b.sy = b.s;
        });
        particles(ctx, 80, { color: '#a83040', size: 2.6, v: [0.3, 0, 0.1], wobble: 0.1, opacity: 0.06, box: [-18, 18, -0.5, 2, -15, 11] });
      },
    },

    // ===== Kowloon digital overlay =====
    {
      id: 'kowloon', name: 'Kowloon Overlay', rarity: 'epic', price: 1000, icon: '🏙️',
      desc: 'A rooftop in a neon city with a digital grid scanning over it. Neon signs flicker and the rain gets heavier when the fight starts.',
      sky: ['#05030f', '#1a0a30', '#3a1450'], fog: ['#1a1030', 24, 58], hemi: ['#9a8aff', '#1a1020', 1.05], sun: ['#ff9ae0', 1.5],
      slab: '#101016', benchSlab: '#1a1a24', slot: '#262634', ped: '#33334a',
      tiles: { mine: '#2f6fd0', enemy: '#c63a6c', enemyPlan: '#5e2244', rough: 0.35, metal: 0.25 },
      look: { pedMetal: 0.7, pedRough: 0.3, slotMetal: 0.3, slotRough: 0.4 },
      surface: () => ({
        tile: tileTex('#dcdce8', 'rgba(20,20,40,0.45)', (g) => { g.fillStyle = 'rgba(255,255,255,0.35)'; for (let i = 0; i < 6; i++) g.fillRect(28 + i * 12, 60, 6, 8); }),
        slot: stripes('#2a2a36', '#d0a020', 10, 1),
      }),
      props(ctx) {
        const roof = bricks('#4a4a56', '#2a2a32', { bw: 64, bh: 64, seed: 303, vary: 0.08, repeat: 6 });
        platform(ctx, { sides: 4, r: 15, r2: 15, h: 1.4, top: roof, side: '#24242e', rot: Math.PI / 4 });
        mesh(ctx, new THREE.CylinderGeometry(15, 15, 40, 4, 1, true).rotateY(Math.PI / 4), std({ color: '#1a1a24', roughness: 1 }), { y: -21.4 });
        hexEdges(ctx, { mine: '#36e0ff', enemy: '#ff3aa8', opacity: 0.65, additive: true });
        // the city below and around: one instanced block of buildings with a lit-window texture
        const win = canvasTex(32, 64, (g) => {
          const rng = DT.makeRng(31);
          g.fillStyle = '#0a0a12'; g.fillRect(0, 0, 32, 64);
          const lit = ['#ffd27a', '#7ae8ff', '#ff8ad8', '#ffefc0'];
          for (let y = 2; y < 64; y += 6) for (let x = 2; x < 32; x += 6) if (rng() < 0.45) { g.fillStyle = lit[(rng() * 4) | 0]; g.fillRect(x, y, 3, 3); }
        });
        win.repeat.set(1, 2);
        const b = [];
        const rng = DT.makeRng(32);
        for (let gx = -42; gx <= 42; gx += 5.5) for (let gz = -42; gz <= 14; gz += 5.5) {
          const x = gx + (rng() - 0.5) * 1.5, z = gz + (rng() - 0.5) * 1.5;
          if (Math.abs(x) < 13.6 && z > -13.6) continue;
          const near = Math.max(Math.abs(x) - 13.6, -13.6 - z) < 6;
          const top = near ? -3 + rng() * 8 : -12 + rng() * 10;
          const w = 3.4 + rng() * 1.8;
          b.push({ x, y: (top - 30) / 2, z, sx: w, sy: top + 30, sz: 3.4 + rng() * 1.8 });
        }
        inst(ctx, new THREE.BoxGeometry(1, 1, 1), std({ color: '#2a2a3a', map: win, emissive: '#ffffff', emissiveMap: win, emissiveIntensity: 1.1, roughness: 0.8 }), b, { shadow: false });
        // rooftop clutter: water tanks, AC units, antennas
        const rs = rimSpots(ctx.rng, 80, 9.8, 10.8).concat([{ x: -10.2, z: -9.5 }, { x: 10.2, z: -9.6 }, { x: 0, z: -10.5 }]);
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.8, 0.8, 1.6, 12), '#6a5a50', { y: 1.6 }), part(new THREE.ConeGeometry(0.85, 0.5, 12), '#5a4a40', { y: 2.65 }),
          ...[0, 1, 2, 3].map((k) => part(new THREE.CylinderGeometry(0.05, 0.05, 0.8, 4), '#3a3a40', { x: Math.cos(k * 1.57) * 0.6, y: 0.4, z: Math.sin(k * 1.57) * 0.6 }))]),
        std({ vertexColors: true }), rs.slice(0, 3).concat(rs.slice(-3, -1)).map((p) => ({ x: p.x, z: p.z })));
        inst(ctx, new THREE.BoxGeometry(1.1, 0.7, 0.8), std({ color: '#9aa0aa', metalness: 0.4, roughness: 0.5 }), rs.slice(3, 12).map((p) => ({ x: p.x, y: 0.35, z: p.z, ry: ctx.rng() * 3 })));
        inst(ctx, new THREE.CylinderGeometry(0.03, 0.05, 4, 4), std({ color: '#55555f' }), rs.slice(12, 18).map((p) => ({ x: p.x, y: 2, z: p.z })), { shadow: false });
        // neon signs on poles, flickering
        const neon = ['#ff3aa8', '#36e0ff', '#ffe03a', '#9a5aff', '#3aff9a'];
        const signs = rimSpots(ctx.rng, 80, 10.4, 11.4, true).slice(0, 9).map((p, i) => ({ x: p.x, y: 1.8 + (i % 3) * 0.5, z: p.z, ry: -p.a + Math.PI / 2, sx: 1, sy: 1.6 + (i % 2), color: neon[i % neon.length], ph: ctx.rng() * 9 }));
        const signTex = canvasTex(32, 96, (g) => { g.fillStyle = '#000'; g.fillRect(0, 0, 32, 96); g.strokeStyle = '#fff'; g.lineWidth = 3; g.strokeRect(3, 3, 26, 90); g.fillStyle = '#fff'; for (let y = 12; y < 90; y += 18) g.fillRect(9, y, 14, 9); });
        const sm = inst(ctx, new THREE.PlaneGeometry(0.9, 1), basic({ map: signTex, side: THREE.DoubleSide, blending: THREE.AdditiveBlending, transparent: true, depthWrite: false }), signs, { shadow: false });
        ctx.tick((dt, t) => {
          signs.forEach((s, i) => { const off = Math.sin(t * 23 + s.ph) > 0.93 && Math.sin(t * 1.7 + s.ph) > 0.4; sm.setColorAt(i, C(off ? '#222222' : s.color)); });
          sm.instanceColor.needsUpdate = true;
        });
        // the scanning overlay: a cyan band that sweeps up through the city
        const band = mesh(ctx, new THREE.CylinderGeometry(14.4, 14.4, 0.25, 4, 1, true).rotateY(Math.PI / 4), glow('#36e0ff', 0.7, { side: THREE.DoubleSide, fog: false }), { y: 0 });
        const hexTex = canvasTex(64, 64, (g) => { g.clearRect(0, 0, 64, 64); g.strokeStyle = 'rgba(60,220,255,0.9)'; g.lineWidth = 2; hexPath(g, 32, 32, 28); g.stroke(); }, 1);
        hexTex.repeat.set(24, 3);
        const lattice = mesh(ctx, new THREE.CylinderGeometry(14.5, 14.5, 6, 4, 1, true).rotateY(Math.PI / 4), basic({ map: hexTex, transparent: true, opacity: 0.25, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide, fog: false }), { y: 0 });
        ctx.tick((dt, t) => { const k = (t * 0.18) % 1; band.position.y = -12 + k * 18; lattice.position.y = band.position.y - 3; lattice.material.opacity = 0.28 * Math.sin(k * Math.PI); band.material.opacity = 0.75 * Math.sin(k * Math.PI); });
        // rain: heavier and closer fog in combat
        const rain = particles(ctx, 520, { color: '#a8c8ff', size: 0.07, v: [0.4, -14, 0.2], wobble: 0, opacity: 0.55, box: [-17, 17, -1, 14, -15, 11] });
        const fog = ctx.fog();
        const w = { k: 0 };
        ctx.tick((dt) => {
          w.k = approach(w.k, ctx.state.phase === 'combat' ? 1 : 0, 1.2, dt);
          rain.material.opacity = 0.35 + 0.4 * w.k;
          rain.material.size = 0.06 + 0.04 * w.k;
          fog.near = lerp(24, 15, w.k); fog.far = lerp(58, 44, w.k);
        });
      },
    },

    // ===== Inside the computer =====
    {
      id: 'circuit-core', name: 'Circuit Core', rarity: 'legendary', price: 2000, icon: '🖥️',
      desc: 'The Digital World from inside a computer: the board is a giant chip on a circuit board, data pulses run the traces and code scrolls on the walls.',
      sky: ['#000400', '#001a08', '#003a14'], fog: ['#001208', 28, 64], hemi: ['#9affc0', '#001004', 1.05], sun: ['#d8ffe8', 1.7],
      slab: '#0a0e0c', benchSlab: '#101814', slot: '#141a16', ped: '#c8ccd0',
      tiles: { mine: '#2c6ed6', enemy: '#c4405c', enemyPlan: '#5c2232', rough: 0.3, metal: 0.45 },
      look: { pedMetal: 0.8, pedRough: 0.25 },
      surface: () => ({
        tile: tileTex('#f2ecd8', 'rgba(140,100,20,0.7)', (g) => { g.fillStyle = 'rgba(160,120,30,0.45)'; for (const [x, y] of [[40, 40], [88, 40], [40, 88], [88, 88]]) { g.beginPath(); g.arc(x, y, 5, 0, TAU); g.fill(); } g.strokeStyle = 'rgba(160,120,30,0.35)'; g.lineWidth = 3; g.strokeRect(46, 46, 36, 36); }, 8),
        slot: canvasTex(64, 64, (g) => { g.fillStyle = '#1c2420'; g.fillRect(0, 0, 64, 64); g.fillStyle = '#d4a940'; for (let x = 4; x < 64; x += 6) g.fillRect(x, 50, 3, 10); g.fillStyle = '#0a0e0c'; g.fillRect(6, 10, 52, 30); }),
        ped: canvasTex(64, 64, (g) => { g.fillStyle = '#d4d8dc'; g.fillRect(0, 0, 64, 64); g.strokeStyle = '#8a9096'; g.lineWidth = 4; g.beginPath(); g.moveTo(32, 8); g.lineTo(32, 56); g.moveTo(8, 32); g.lineTo(56, 32); g.stroke(); }),
      }),
      props(ctx) {
        // the motherboard: green with copper traces and pads
        const pcb = canvasTex(256, 256, (g) => {
          const rng = DT.makeRng(404);
          g.fillStyle = '#0e4a26'; g.fillRect(0, 0, 256, 256);
          g.strokeStyle = '#3aa860'; g.lineWidth = 3;
          for (let i = 0; i < 26; i++) {
            let x = (rng() * 16 | 0) * 16, y = (rng() * 16 | 0) * 16;
            g.beginPath(); g.moveTo(x, y);
            for (let k = 0; k < 4; k++) { if (rng() < 0.5) x += (rng() < 0.5 ? -1 : 1) * 32; else y += (rng() < 0.5 ? -1 : 1) * 32; g.lineTo(x, y); }
            g.stroke();
            g.fillStyle = '#c8a040'; g.beginPath(); g.arc(x, y, 4, 0, TAU); g.fill();
          }
        }, 9);
        plain(ctx, std({ map: pcb, roughness: 0.6, metalness: 0.2 }), -1.4);
        // the big chip: black package with gold pins
        const chipTop = canvasTex(128, 128, (g) => { g.fillStyle = '#16181a'; g.fillRect(0, 0, 128, 128); g.strokeStyle = '#26292c'; g.lineWidth = 2; for (let i = 8; i < 128; i += 16) { g.beginPath(); g.moveTo(i, 0); g.lineTo(i, 128); g.moveTo(0, i); g.lineTo(128, i); g.stroke(); } }, 5);
        platform(ctx, { sides: 4, r: 14, r2: 14, h: 1.2, top: chipTop, side: '#121416', rot: Math.PI / 4 });
        const pins = [];
        const half = 14 * Math.SQRT1_2;
        for (let i = -13; i <= 13; i++) {
          const v = i * 0.72;
          pins.push({ x: v, y: -0.9, z: -half - 0.3, sz: 1 }, { x: -half - 0.3, y: -0.9, z: v, ry: Math.PI / 2 }, { x: half + 0.3, y: -0.9, z: v, ry: Math.PI / 2 }, { x: v, y: -0.9, z: half + 0.3 });
        }
        inst(ctx, new THREE.BoxGeometry(0.3, 0.12, 0.9), std({ color: '#e0b850', metalness: 0.9, roughness: 0.25 }), pins, { shadow: false });
        hexEdges(ctx, { mine: '#5affa0', enemy: '#ffd04a', opacity: 0.5, additive: true, inner: 0.88 });
        // capacitors and small chips on the motherboard, outside the big chip
        const caps = farSpots(ctx.rng, 30, 12, 20).filter((p) => Math.abs(p.x) > half + 1 || p.z < -half - 1).slice(0, 16).map((p) => ({ x: p.x, y: -1.4, z: p.z, sy: 1 + ctx.rng() * 1.8, s: 0.8 }));
        const capGeo = merge([part(new THREE.CylinderGeometry(0.7, 0.7, 2, 16), '#2a3a8a', { y: 1 }), part(new THREE.CylinderGeometry(0.71, 0.71, 0.08, 16), '#d0d4da', { y: 2 })]);
        inst(ctx, capGeo, std({ vertexColors: true, roughness: 0.35, metalness: 0.3 }), caps.map((c) => ({ ...c, s: undefined, sx: 0.8, sz: 0.8 })));
        inst(ctx, new THREE.BoxGeometry(2.6, 0.5, 1.8), std({ color: '#18191b', roughness: 0.5 }), farSpots(ctx.rng, 40, 13, 22).filter((p) => Math.abs(p.x) > half + 2 || p.z < -half - 2).slice(0, 10).map((p) => ({ x: p.x, y: -1.15, z: p.z, ry: ctx.rng() < 0.5 ? 0 : Math.PI / 2 })));
        // data pulses running along straight traces
        const N = 60;
        const pulses = Array.from({ length: N }, (_, i) => {
          const along = i % 2 === 0;
          const lane = (((i * 7919) % 23) - 11) * 1.6;
          return { along, lane, off: (i * 0.37) % 1, sp: 0.08 + ((i * 13) % 7) * 0.02 };
        });
        const pp = new Float32Array(N * 3);
        const pg = new THREE.BufferGeometry();
        pg.setAttribute('position', new THREE.BufferAttribute(pp, 3));
        const pts = new THREE.Points(pg, new THREE.PointsMaterial({ color: '#7affb8', size: 0.6, map: softDot(), transparent: true, depthWrite: false, blending: THREE.AdditiveBlending }));
        pts.frustumCulled = false;
        ctx.g.add(pts);
        const speed = { v: 1, ph: 0 };
        ctx.tick((dt, t) => {
          speed.v = approach(speed.v, ctx.state.phase === 'combat' ? 2.6 : 1, 2, dt);
          speed.ph += dt * speed.v;
          pulses.forEach((p, i) => {
            const k = ((p.off + speed.ph * p.sp) % 1) * 60 - 30;
            pp[i * 3] = p.along ? k : p.lane; pp[i * 3 + 1] = -1.3; pp[i * 3 + 2] = p.along ? p.lane - 6 : k;
            if (Math.abs(pp[i * 3]) < half + 0.6 && Math.abs(pp[i * 3 + 2]) < half + 0.6) pp[i * 3 + 1] = -60;
          });
          pg.attributes.position.needsUpdate = true;
        });
        // scrolling code on curved screens around the back and sides
        const code = canvasTex(256, 256, (g) => {
          const rng = DT.makeRng(405);
          g.fillStyle = '#000000'; g.fillRect(0, 0, 256, 256);
          g.font = 'bold 13px monospace';
          const words = ['0x3F', 'A9', 'JMP', 'MOV', '1011', 'DIGI', 'VPET', '0xFF', 'CALL', '00', 'EVO', 'XOR'];
          for (let y = 12; y < 256; y += 14) for (let x = 2; x < 256; x += 34) { g.fillStyle = rng() < 0.15 ? '#c8ffd8' : rng() < 0.5 ? '#3aff7a' : '#1a9a44'; g.fillText(words[(rng() * words.length) | 0], x, y); }
        });
        code.repeat.set(6, 1);
        const screenMat = basic({ map: code, transparent: true, opacity: 0.55, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide });
        mesh(ctx, new THREE.CylinderGeometry(13.6, 13.6, 7, 48, 1, true, Math.PI * 0.6, Math.PI * 1.3), screenMat, { y: 1.6 });
        const floorCode = code.clone();
        floorCode.repeat.set(10, 10);
        const fl = mesh(ctx, new THREE.RingGeometry(16, 40, 48).rotateX(-Math.PI / 2), basic({ map: floorCode, transparent: true, opacity: 0.18, depthWrite: false, blending: THREE.AdditiveBlending }), { y: -1.35 });
        const flash = { k: 0 };
        ctx.on('win', () => { flash.k = 1; ctx.light().flash = 1.2; });
        ctx.tick((dt, t) => {
          code.offset.y = (t * 0.12 * speed.v) % 1;
          floorCode.offset.y = (-t * 0.03) % 1;
          flash.k = Math.max(0, flash.k - dt * 0.8);
          screenMat.opacity = 0.5 + 0.5 * flash.k;
          fl.material.opacity = 0.16 + 0.4 * flash.k;
        });
        ctx.light().flashColor.set('#a8ffc8');
        particles(ctx, 200, { color: '#3aff7a', size: 0.12, v: [0, -3, 0], wobble: 0, opacity: 0.7, additive: true, box: [-17, 17, -1, 12, -15, 11] });
      },
    },

    // ===== Zhuqiaomon's flame temple =====
    {
      id: 'flame-temple', name: "Zhuqiaomon's Temple", rarity: 'legendary', price: 2400, icon: '🔥',
      desc: 'A Sovereign\'s temple on a lava sea. Braziers roar, flame geysers burst, and the whole temple flares when your Digimon digivolve.',
      sky: ['#1a0200', '#4a0a00', '#8a2a00'], fog: ['#3a0e04', 26, 62], hemi: ['#ffb080', '#2a0800', 1.25], sun: ['#ffc080', 2.4],
      slab: '#1a0c08', benchSlab: '#5a120c', slot: '#8a1a10', ped: '#d8a040',
      tiles: { mine: '#3a72d4', enemy: '#b23c28', enemyPlan: '#5a2216', rough: 0.6 },
      look: { pedMetal: 0.8, pedRough: 0.3, slotRough: 0.35 },
      surface: () => ({ tile: tileTex('#f6f0ec', 'rgba(50,20,10,0.5)', (g) => { g.strokeStyle = 'rgba(140,60,20,0.4)'; g.lineWidth = 2; g.beginPath(); g.moveTo(30, 90); g.lineTo(55, 60); g.lineTo(50, 30); g.moveTo(55, 60); g.lineTo(95, 70); g.stroke(); }) }),
      props(ctx) {
        // lava: bright unlit texture scrolling, brighter in later stages and in combat
        const lava = canvasTex(128, 128, (g) => {
          const rng = DT.makeRng(505);
          g.fillStyle = '#b42800'; g.fillRect(0, 0, 128, 128);
          for (let i = 0; i < 70; i++) { const gr = g.createRadialGradient(0, 0, 0, 0, 0, 1); gr.addColorStop(0, i % 3 ? '#ffb020' : '#fff080'); gr.addColorStop(1, 'rgba(255,90,0,0)'); g.setTransform(6 + rng() * 14, 0, 0, 4 + rng() * 10, rng() * 128, rng() * 128); g.fillStyle = gr; g.fillRect(-1, -1, 2, 2); }
          g.setTransform(1, 0, 0, 1, 0, 0);
          g.fillStyle = 'rgba(40,8,0,0.55)'; for (let i = 0; i < 26; i++) { g.beginPath(); g.ellipse(rng() * 128, rng() * 128, 4 + rng() * 9, 3 + rng() * 6, rng() * 3, 0, TAU); g.fill(); }
        }, 10);
        const lavaMat = basic({ map: lava, color: '#ffffff' });
        plain(ctx, lavaMat, -1.3);
        // two-tier octagonal temple base of dark basalt
        const basalt = bricks('#3a2a26', '#140a08', { bw: 48, bh: 24, seed: 506, repeat: 6 });
        platform(ctx, { sides: 8, r: 13.3, r2: 13.6, h: 0.9, top: basalt, side: '#2a1a16', rot: Math.PI / 8 });
        platform(ctx, { sides: 8, r: 14.8, r2: 15.6, h: 0.8, top: '#2a1c18', side: '#22140f', rot: Math.PI / 8, y: -0.9 });
        hexEdges(ctx, { mine: '#ffd27a', enemy: '#ff8a4a', opacity: 0.55, additive: true });
        // red lacquer pillars with gold caps, a gate on each side
        const pillar = merge([part(new THREE.CylinderGeometry(0.38, 0.42, 4.2, 12), '#b01a12', { y: 2.1 }), part(new THREE.CylinderGeometry(0.55, 0.55, 0.25, 12), '#e0b040', { y: 4.25 }), part(new THREE.CylinderGeometry(0.55, 0.6, 0.3, 12), '#e0b040', { y: 0.15 })]);
        const pil = arc(9, 12.6, Math.PI * 1.08, Math.PI * 1.92);
        inst(ctx, pillar, std({ vertexColors: true, roughness: 0.35, metalness: 0.15 }), pil.map((p) => ({ x: p.x, z: p.z })));
        const gate = merge([
          part(new THREE.CylinderGeometry(0.3, 0.34, 4, 10), '#c41e14', { z: -1.6, y: 2 }), part(new THREE.CylinderGeometry(0.3, 0.34, 4, 10), '#c41e14', { z: 1.6, y: 2 }),
          part(new THREE.BoxGeometry(0.5, 0.35, 4.8), '#c41e14', { y: 3.4 }), part(new THREE.BoxGeometry(0.6, 0.35, 5.8), '#1a0a08', { y: 4.1, rx: 0 }),
          part(new THREE.BoxGeometry(0.3, 0.6, 0.4), '#e0b040', { y: 3.75 }),
        ]);
        inst(ctx, gate, std({ vertexColors: true, roughness: 0.35 }), [{ x: -12.2, z: -1.5 }, { x: 12.2, z: -1.5 }]);
        // braziers with roaring flames
        const braz = [{ x: -10.8, z: -6.8 }, { x: 10.8, z: -6.8 }, { x: -11, z: 3.5 }, { x: 11, z: 3.5 }, { x: -5, z: -10.6 }, { x: 5, z: -10.6 }];
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.8, 0.35, 0.7, 12), '#d0a040', { y: 1.35 }), part(new THREE.CylinderGeometry(0.15, 0.3, 1.1, 8), '#8a6a2a', { y: 0.55 })]), std({ vertexColors: true, metalness: 0.7, roughness: 0.35 }), braz);
        const fl = [];
        braz.forEach((b, i) => { for (let k = 0; k < 3; k++) fl.push({ bx: b.x + (k - 1) * 0.25, bz: b.z + ((k * 7) % 3 - 1) * 0.2, x: b.x, y: 2.1, z: b.z, ph: i * 2.3 + k * 1.1, h: 1.2 - k * 0.2 }); });
        const fire = { k: 0 };
        anim(ctx, new THREE.ConeGeometry(0.35, 1.4, 8).translate(0, 0.7, 0), glow('#ff8a1a', 0.85), fl, (f, i, t) => { const k = (1 + 0.25 * Math.sin(t * 11 + f.ph) + 0.15 * Math.sin(t * 17 + f.ph * 2)) * (1 + fire.k * 0.8); f.x = f.bx; f.z = f.bz; f.y = 1.6; f.sy = k * f.h; f.sx = f.sz = 1 / Math.sqrt(k); f.ry = t * 2 + f.ph; });
        anim(ctx, new THREE.ConeGeometry(0.2, 0.9, 6).translate(0, 0.45, 0), glow('#fff2a0', 0.9), fl.filter((_, i) => i % 3 === 0), (f, i, t) => { f.x = f.bx; f.z = f.bz; f.y = 1.6; f.sy = (1 + 0.3 * Math.sin(t * 13 + f.ph)) * (1 + fire.k); });
        // flame geysers bursting out of the lava
        const gey = [{ x: -15.5, z: -8 }, { x: 15.5, z: -6 }, { x: -16, z: 2 }, { x: 16.3, z: 3 }, { x: -6, z: -15 }, { x: 7, z: -15.2 }].map((p, i) => ({ ...p, y: -1.3, t0: -9, ph: i * 1.7, sy: 0.01 }));
        anim(ctx, new THREE.CylinderGeometry(0.15, 0.7, 1, 10, 1, true).translate(0, 0.5, 0), glow('#ffb030', 0.8, { side: THREE.DoubleSide }), gey, (g, i, t) => {
          if (t - g.t0 > 3.4 && Math.sin(t * 0.9 + g.ph * 3) > 0.995) g.t0 = t;
          const k = t - g.t0;
          const h = k < 0.3 ? k / 0.3 : k < 1.6 ? 1 : Math.max(0, 1 - (k - 1.6) / 0.6);
          g.sy = 0.01 + h * 7; g.sx = g.sz = 0.6 + h * 0.6; g.ry = t * 3;
        });
        // embers rising
        const embers = particles(ctx, 260, { color: '#ffa040', size: 0.16, v: [0.1, 1.6, 0], wobble: 0.5, opacity: 0.9, additive: true, box: [-18, 18, -1, 10, -16, 11] });
        // digivolve flare: white-gold light burst, every flame and geyser at once; lava brightens by stage and in combat
        const lit = ctx.light();
        lit.flashColor.set('#fff0c0');
        const flare = { k: 0 };
        ctx.on('evolve', (t) => { flare.k = 1; lit.flash = 2.2; for (const g of gey) g.t0 = t + Math.random() * 0.3; });
        ctx.tick((dt, t) => {
          flare.k = Math.max(0, flare.k - dt * 0.6);
          fire.k = flare.k;
          lit.sun = 1 + flare.k * 1.4;
          lit.tint.set('#fff6e0'); lit.tintK = flare.k;
          embers.material.size = 0.16 + flare.k * 0.2;
          const heat = Math.min(1, (ctx.state.stage - 1) / 6) * 0.35 + (ctx.state.phase === 'combat' ? 0.2 : 0) + 0.65 + 0.06 * Math.sin(t * 2);
          lavaMat.color.setScalar(Math.min(1.3, heat + flare.k * 0.5));
          lava.offset.set((t * 0.01) % 1, (t * 0.006) % 1);
        });
      },
    },

    // ===== Underwater Gekomon castle =====
    {
      id: 'gekomon-castle', name: 'Gekomon Castle Depths', rarity: 'epic', price: 950, icon: '🐚',
      desc: 'A coral reef on the sea floor by ShogunGekomon\'s castle. Kelp sways, fish school around, bubbles rise and light ripples on the sand.',
      sky: ['#021428', '#0a3a5c', '#1a6a8a'], fog: ['#0b4466', 20, 54], hemi: ['#8ae8ff', '#0a2a3a', 1.3], sun: ['#bff4ff', 1.9],
      slab: '#2a4a5a', benchSlab: '#365a66', slot: '#c8b48a', ped: '#f2eaf6',
      tiles: { mine: '#2f7ad0', enemy: '#d0566a', enemyPlan: '#6a2a3a', rough: 0.4 },
      look: { pedMetal: 0.35, pedRough: 0.2 },
      surface: () => ({
        ped: canvasTex(64, 64, (g) => { const gr = g.createRadialGradient(32, 32, 2, 32, 32, 32); gr.addColorStop(0, '#ffffff'); gr.addColorStop(0.6, '#f0e4ff'); gr.addColorStop(1, '#c8e8f0'); g.fillStyle = gr; g.fillRect(0, 0, 64, 64); }),
      }),
      props(ctx) {
        const sand = speckle('#c8b080', ['#e0cca0', '#a89060', '#f0e0c0'], { seed: 606, repeat: 10 });
        plain(ctx, std({ map: sand, roughness: 1 }));
        platform(ctx, { sides: 40, r: 12.6, r2: 13.8, h: 1.4, top: speckle('#d4bc8c', ['#e8d4a8', '#b49c6c', '#f0b8c0'], { seed: 607 }), side: '#a87a7a' });
        hexEdges(ctx, { mine: '#9ae8ff', enemy: '#ffb8c8', opacity: 0.45 });
        // caustics: two scrolling layers of rippling light on the floor around the board
        const caus = canvasTex(128, 128, (g) => {
          const rng = DT.makeRng(608);
          g.clearRect(0, 0, 128, 128);
          g.strokeStyle = 'rgba(200,255,255,0.8)'; g.lineWidth = 2.2;
          for (let i = 0; i < 40; i++) { g.beginPath(); const x = rng() * 128, y = rng() * 128, r = 6 + rng() * 14; g.ellipse(x, y, r, r * (0.5 + rng() * 0.5), rng() * 3, 0, TAU); g.stroke(); }
        }, 8);
        const caus2 = caus.clone();
        caus2.repeat.set(6, 6);
        const cm = (tex) => basic({ map: tex, transparent: true, opacity: 0.22, depthWrite: false, blending: THREE.AdditiveBlending });
        mesh(ctx, new THREE.CircleGeometry(40, 40).rotateX(-Math.PI / 2), cm(caus), { y: -1.08 });
        mesh(ctx, new THREE.CircleGeometry(12.6, 40).rotateX(-Math.PI / 2), cm(caus2), { y: 0.0 });
        ctx.tick((dt, t) => { caus.offset.set(t * 0.012, t * 0.008); caus2.offset.set(-t * 0.01, t * 0.013); });
        // light shafts from above
        const shafts = arc(7, 12, Math.PI * 1.0, Math.PI * 2.0).map((p, i) => ({ x: p.x, y: 4, z: p.z, ry: -p.a, rz: 0.25, sx: 1.6, sy: 10, ph: i * 1.3 }));
        const shm = anim(ctx, new THREE.PlaneGeometry(1, 1), glow('#bff8ff', 0.08, { side: THREE.DoubleSide, fog: false }), shafts, (s, i, t) => { s.sx = 1.4 + 0.5 * Math.sin(t * 0.5 + s.ph); });
        shm.renderOrder = 2;
        // kelp: tall swaying ribbons
        const kelpGeo = new THREE.PlaneGeometry(0.35, 3.6, 1, 6);
        kelpGeo.translate(0, 1.8, 0);
        const kp = kelpGeo.attributes.position;
        for (let i = 0; i < kp.count; i++) kp.setX(i, kp.getX(i) + Math.sin(kp.getY(i) * 1.6) * 0.18);
        const kelp = rimSpots(ctx.rng, 120, 10, 12.4).slice(0, 34).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 3, s: 0.7 + ctx.rng() * 0.8, ph: ctx.rng() * 6 }));
        anim(ctx, kelpGeo, std({ color: '#3a8a3a', side: THREE.DoubleSide, roughness: 0.8 }), kelp, (k, i, t) => { k.rz = Math.sin(t * 0.9 + k.ph) * 0.12; k.rx = Math.cos(t * 0.7 + k.ph) * 0.08; });
        // coral: branches in reef colors
        const coral = merge([
          part(new THREE.CylinderGeometry(0.08, 0.14, 1.2, 6), '#ffffff', { y: 0.6 }), part(new THREE.CylinderGeometry(0.06, 0.1, 0.8, 6), '#ffffff', { x: 0.25, y: 1.0, rz: -0.6 }),
          part(new THREE.CylinderGeometry(0.06, 0.1, 0.9, 6), '#ffffff', { x: -0.25, y: 0.95, rz: 0.6 }), part(new THREE.CylinderGeometry(0.05, 0.08, 0.6, 6), '#ffffff', { z: 0.22, y: 1.1, rx: 0.6 }),
          part(new THREE.SphereGeometry(0.1, 6, 4), '#ffffff', { y: 1.22 }),
        ]);
        const cc = ['#ff7a8a', '#ffa04a', '#c87aff', '#ff5ac8', '#ffe07a'];
        inst(ctx, coral, std({ color: '#ffffff', roughness: 0.7 }), rimSpots(ctx.rng, 90, 9.8, 12.3).slice(0, 26).map((p, i) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.6 + ctx.rng() * 0.8, color: cc[i % cc.length] })));
        inst(ctx, new THREE.SphereGeometry(0.5, 10, 8), std({ color: '#ffffff', roughness: 0.9 }), rimSpots(ctx.rng, 90, 9.8, 12.3).slice(0, 14).map((p, i) => ({ x: p.x, y: 0.1, z: p.z, sy: 0.6, s: 0.5 + ctx.rng() * 0.6, color: ['#ff9a7a', '#a87aff', '#5ac8a8'][i % 3] })));
        // giant clams, opening and closing
        const clam = new THREE.SphereGeometry(0.7, 14, 6, 0, TAU, 0, Math.PI / 2);
        clam.scale(1, 0.45, 1);
        const clamsLo = [{ x: -10.6, z: -3.6 }, { x: 10.8, z: 0.8 }, { x: -9.6, z: -8.6 }];
        inst(ctx, clam, std({ color: '#d0a8c8', roughness: 0.5 }), clamsLo.map((c) => ({ ...c, rx: Math.PI, y: 0.3 })));
        anim(ctx, clam, std({ color: '#e0b8d8', roughness: 0.5 }), clamsLo.map((c, i) => ({ ...c, y: 0.3, ph: i * 2 })), (c, i, t) => { c.rx = -Math.max(0, Math.sin(t * 0.6 + c.ph)) * 0.7; });
        inst(ctx, new THREE.SphereGeometry(0.16, 10, 8), std({ color: '#ffffff', roughness: 0.1, emissive: '#c8e8ff', emissiveIntensity: 0.5 }), clamsLo.map((c) => ({ ...c, y: 0.38 })));
        // the castle: stacked tiers with sweeping roofs at the back right
        const castle = [];
        [[2.8, 1.4, 0], [2.2, 1.2, 1.9], [1.6, 1.1, 3.6]].forEach(([w, h, y]) => {
          castle.push(part(new THREE.BoxGeometry(w, h, w), '#e8e0d0', { y: y + h / 2 }));
          castle.push(part(new THREE.ConeGeometry(w * 0.95, 0.7, 4), '#2a6a7a', { y: y + h + 0.3, ry: Math.PI / 4 }));
        });
        castle.push(part(new THREE.ConeGeometry(0.5, 1.6, 10), '#f0c8a0', { y: 5.8 }));
        inst(ctx, merge(castle), std({ vertexColors: true, roughness: 0.7 }), [{ x: 9.8, z: -8.2, ry: 0.5 }, { x: -11.4, z: -6.2, ry: -0.4, s: 0.7 }]);
        // schools of fish circling; they scatter when combat starts
        const fishGeo = merge([part(new THREE.ConeGeometry(0.12, 0.5, 6), '#ffffff', { rz: -Math.PI / 2 }), part(new THREE.ConeGeometry(0.1, 0.18, 3), '#ffffff', { x: -0.3, rz: Math.PI / 2, sz: 0.3 })]);
        const fc = ['#ffb03a', '#3ac8ff', '#ff5a5a', '#ffe85a'];
        const fish = [];
        for (let s = 0; s < 4; s++) for (let k = 0; k < 14; k++) fish.push({ s, rad: 11 + s * 1.1 + ((k * 7) % 5) * 0.25, h: 1.4 + s * 0.7 + ((k * 3) % 4) * 0.18, off: k * 0.07, dir: s % 2 ? 1 : -1, color: fc[s], ph: k });
        const scat = { k: 0, until: -1 };
        ctx.on('combat', (t) => { scat.until = t + 2.5; });
        ctx.tick((dt, t) => { scat.k = approach(scat.k, t < scat.until ? 1 : 0, 3, dt); });
        anim(ctx, fishGeo, std({ color: '#ffffff', roughness: 0.4 }), fish, (f, i, t) => {
          const a = f.dir * t * (0.25 + scat.k * 0.6) + f.off + f.s * 1.6;
          const rad = f.rad + scat.k * (3 + (f.ph % 3));
          f.x = Math.cos(a) * rad; f.z = Math.sin(a) * rad * 0.9 - 1.5; f.y = f.h + Math.sin(t * 2 + f.ph) * 0.12 + scat.k * (f.ph % 2);
          f.ry = -a + (f.dir > 0 ? -Math.PI / 2 : Math.PI / 2) + Math.sin(t * 9 + f.ph) * 0.15;
        });
        particles(ctx, 160, { color: '#e0ffff', size: 0.16, v: [0, 1.4, 0], wobble: 0.25, opacity: 0.7, box: [-17, 17, -1, 10, -15, 11] });
      },
    },

    // ===== Freezeland =====
    {
      id: 'freezeland', name: 'Freezeland', rarity: 'rare', price: 450, icon: '❄️',
      desc: 'An ice shelf on a frozen sea at twilight under the aurora. Calm between rounds; a blizzard blows in when the fight starts.',
      sky: ['#0a1430', '#1a3a5a', '#4a7a9a'], fog: ['#24405e', 26, 62], hemi: ['#9ac4ff', '#1a2a40', 1.0], sun: ['#c8dcff', 1.2],
      slab: '#2a3e58', benchSlab: '#bcd0e4', slot: '#e2ecf6', ped: '#bfe3ff',
      tiles: { mine: '#2e74c4', enemy: '#c44a5a', enemyPlan: '#7a3a4a', rough: 0.15, metal: 0.1 },
      look: { pedRough: 0.1, pedMetal: 0.2 },
      surface: () => ({ tile: tileTex('#f4faff', 'rgba(150,190,230,0.9)', (g) => { g.strokeStyle = 'rgba(170,205,240,0.8)'; g.lineWidth = 1.5; g.beginPath(); g.moveTo(64, 64); g.lineTo(30, 30); g.moveTo(64, 64); g.lineTo(104, 50); g.moveTo(64, 64); g.lineTo(60, 112); g.moveTo(44, 44); g.lineTo(30, 60); g.stroke(); }, 5) }),
      props(ctx) {
        const ice = canvasTex(128, 128, (g) => { const rng = DT.makeRng(707); g.fillStyle = '#6a94bc'; g.fillRect(0, 0, 128, 128); g.strokeStyle = 'rgba(230,245,255,0.6)'; g.lineWidth = 1.2; for (let i = 0; i < 18; i++) { g.beginPath(); let x = rng() * 128, y = rng() * 128; g.moveTo(x, y); for (let k = 0; k < 4; k++) { x += (rng() - 0.5) * 50; y += (rng() - 0.5) * 50; g.lineTo(x, y); } g.stroke(); } }, 8);
        plain(ctx, std({ map: ice, roughness: 0.15, metalness: 0.2 }));
        platform(ctx, { sides: 15, r: 12.8, r2: 13.4, h: 1.4, top: speckle('#b4c6dc', ['#d0deee', '#9cb0cc', '#aabed6'], { seed: 708 }), side: '#8ab4dc', rot: 0.3 });
        hexEdges(ctx, { mine: '#ffffff', enemy: '#ffd8e0', opacity: 0.45 });
        // ice floes on the frozen sea
        inst(ctx, new THREE.CylinderGeometry(1, 1.1, 0.4, 7), std({ color: '#e8f4ff', roughness: 0.3, flatShading: true }), farSpots(ctx.rng, 22, 15, 30).map((p) => ({ x: p.x, y: -1.05, z: p.z, s: 1 + ctx.rng() * 2, sy: 1, ry: ctx.rng() * 3 })), { shadow: false });
        // ice spikes: clusters of glassy cones
        const spike = merge([0, 1, 2, 3].map((k) => part(new THREE.ConeGeometry(0.3 + (k % 2) * 0.15, 1.8 + k * 0.6, 5), '#ffffff', { x: Math.cos(k * 1.9) * 0.35, y: 0.8 + k * 0.3, z: Math.sin(k * 1.9) * 0.35, rz: (k - 1.5) * 0.18 })));
        inst(ctx, spike, std({ color: '#a8d8ff', roughness: 0.05, metalness: 0.1, transparent: true, opacity: 0.85, flatShading: true, emissive: '#2a5a8a', emissiveIntensity: 0.4 }), rimSpots(ctx.rng, 80, 9.8, 12.3).slice(0, 14).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.6 + ctx.rng() * 0.7 })));
        // Frigimon snowmen: body, head, hands and a face
        const snowman = merge([part(new THREE.SphereGeometry(0.75, 14, 10), '#ffffff', { y: 0.65 }), part(new THREE.SphereGeometry(0.5, 14, 10), '#ffffff', { y: 1.6 }),
          part(new THREE.SphereGeometry(0.22, 8, 6), '#ffffff', { x: 0.75, y: 1.0 }), part(new THREE.SphereGeometry(0.22, 8, 6), '#ffffff', { x: -0.75, y: 1.0 }),
          part(new THREE.SphereGeometry(0.06, 6, 4), '#222a40', { x: 0.17, y: 1.72, z: 0.44 }), part(new THREE.SphereGeometry(0.06, 6, 4), '#222a40', { x: -0.17, y: 1.72, z: 0.44 }),
          part(new THREE.SphereGeometry(0.1, 6, 4), '#2a4a8a', { y: 1.52, z: 0.48 })]);
        const men = [{ x: -10.8, z: -4.8 }, { x: 11, z: -1.5 }, { x: 4.5, z: -10.6 }, { x: -10.4, z: 2.2 }].map((p, i) => ({ ...p, ry: Math.atan2(-p.x, 6 - p.z), ph: i * 1.7, s: 0.9 }));
        anim(ctx, snowman, std({ vertexColors: true, roughness: 0.8 }), men, (m, i, t) => { m.rz = Math.sin(t * 1.3 + m.ph) * 0.06; m.y = Math.abs(Math.sin(t * 1.3 + m.ph)) * 0.08; }, { shadow: true });
        // igloos
        const igloo = merge([part(new THREE.SphereGeometry(1.4, 16, 8, 0, TAU, 0, Math.PI / 2), '#f4f8ff'), part(new THREE.CylinderGeometry(0.55, 0.55, 1, 10, 1, false, 0, Math.PI), '#e0eaf6', { z: 1.3, rx: Math.PI / 2, ry: Math.PI })]);
        inst(ctx, igloo, std({ vertexColors: true, roughness: 0.7 }), [{ x: -9.9, z: -8.2, ry: 0.8 }, { x: 10.6, z: -6.8, ry: -0.9 }]);
        // aurora curtains around the back and sides, rippling
        const rib = (r, a0, a1, y, h, color, ph) => {
          const geo = new THREE.PlaneGeometry(1, 1, 48, 1);
          const pos = geo.attributes.position;
          const base = [];
          for (let i = 0; i < pos.count; i++) base.push([pos.getX(i) + 0.5, pos.getY(i) + 0.5]);
          const tex = vgrad(['rgba(255,255,255,0)', 'rgba(255,255,255,0.9)', 'rgba(255,255,255,0.25)', 'rgba(255,255,255,0)']);
          const m = mesh(ctx, geo, basic({ color, map: tex, transparent: true, opacity: 0.6, depthWrite: false, blending: THREE.AdditiveBlending, side: THREE.DoubleSide, fog: false }));
          ctx.tick((dt, t) => {
            for (let i = 0; i < pos.count; i++) {
              const [u, v] = base[i];
              const a = a0 + (a1 - a0) * u;
              const rr = r + Math.sin(u * 9 + t * 0.7 + ph) * 0.9;
              pos.setXYZ(i, Math.cos(a) * rr, y + v * h + Math.sin(u * 5 + t * 0.4 + ph) * 0.5, Math.sin(a) * rr);
            }
            pos.needsUpdate = true;
          });
          return m;
        };
        const aur = [rib(11.6, Math.PI * 0.9, Math.PI * 2.1, -0.6, 5.5, '#4affa8', 0), rib(12.4, Math.PI * 0.85, Math.PI * 2.15, -0.2, 6, '#7a8aff', 2), rib(11.1, Math.PI * 1.05, Math.PI * 1.95, -0.8, 4, '#c86aff', 4)];
        // weather: light snow and bright aurora in planning; blizzard and fog in combat
        const snow = particles(ctx, 380, { color: '#ffffff', size: 0.13, v: [0.3, -1, 0.1], wobble: 0.3, opacity: 0.9, box: [-17, 17, 0, 13, -15, 12] });
        const fog = ctx.fog();
        const w = { k: 0 };
        const base = snow.geometry.attributes.position.array;
        ctx.tick((dt) => {
          w.k = approach(w.k, ctx.state.phase === 'combat' ? 1 : 0, 1, dt);
          for (let i = 0; i < base.length; i += 3) { base[i] += w.k * 9 * dt; if (base[i] > 17) base[i] -= 34; }
          snow.material.size = 0.13 + w.k * 0.05;
          aur.forEach((m, i) => { m.material.opacity = lerp(0.9, 0.15, w.k) * (0.85 + 0.15 * Math.sin(i + w.k)); });
          fog.near = lerp(26, 10, w.k); fog.far = lerp(62, 40, w.k);
          fog.color.set('#24405e').lerp(C('#8aa6c0'), w.k);
        });
      },
    },

    // ===== Numemon sewer (a joke board) =====
    {
      id: 'numemon-sewer', name: 'Numemon Sewer', rarity: 'common', price: 120, icon: '🐌',
      desc: 'A tiled walkway over green sludge, home of the Numemon. They bob around, pipes drip, a rubber duck floats by, and they jump for joy when you win.',
      sky: ['#0a0e06', '#1a2410', '#2a3a18'], fog: ['#1e2a14', 22, 52], hemi: ['#c8e8a0', '#1a1a0a', 1.2], sun: ['#e8f4c0', 1.6],
      slab: '#2a2a22', benchSlab: '#3a3a2c', slot: '#5a5040', ped: '#6a6a60',
      tiles: { mine: '#3a6aa8', enemy: '#a84a3a', enemyPlan: '#5a2a22', rough: 0.8 },
      look: { pedMetal: 0.5, pedRough: 0.6, slotMetal: 0.4 },
      surface: () => ({
        tile: tileTex('#e4e6d4', 'rgba(40,40,20,0.5)', (g) => { const rng = DT.makeRng(8); g.fillStyle = 'rgba(70,80,30,0.25)'; for (let i = 0; i < 26; i++) { g.beginPath(); g.arc(rng() * 128, rng() * 128, 1 + rng() * 5, 0, TAU); g.fill(); } }),
        slot: canvasTex(64, 64, (g) => { g.fillStyle = '#6a5a44'; g.fillRect(0, 0, 64, 64); g.fillStyle = '#1a1a14'; for (let x = 6; x < 64; x += 10) g.fillRect(x, 6, 5, 52); }),
        ped: canvasTex(64, 64, (g) => { g.fillStyle = '#7a7a70'; g.fillRect(0, 0, 64, 64); g.strokeStyle = '#4a4a44'; g.lineWidth = 3; for (let r = 8; r < 32; r += 7) { g.beginPath(); g.arc(32, 32, r, 0, TAU); g.stroke(); } }),
      }),
      props(ctx) {
        const sludge = sea(ctx, { color: '#5a8a2a', amp: 0.08, rough: 0.5, metal: 0.1, speed: 0.4, y: -1.0 });
        sludge.material.emissive = C('#1a3a08');
        const walk = bricks('#8a8a78', '#3a3a30', { bw: 32, bh: 32, seed: 808, vary: 0.15, repeat: 8 });
        platform(ctx, { sides: 4, r: 14.2, r2: 14.4, h: 1.2, top: walk, side: '#4a4a3a', rot: Math.PI / 4 });
        hexEdges(ctx, { mine: '#c8e8ff', enemy: '#ffd0c0', opacity: 0.35 });
        // sewer wall with tunnel mouths across the back, pipes along it
        const wallTex = bricks('#5a5240', '#2a261c', { bw: 32, bh: 16, seed: 809, repeat: 6 });
        mesh(ctx, new THREE.BoxGeometry(40, 8, 1), std({ map: wallTex, roughness: 1 }), { y: 2, z: -13.6, receive: true });
        inst(ctx, new THREE.CircleGeometry(2.2, 20, 0, Math.PI), basic({ color: '#050603' }), [{ x: -7, y: -1.2, z: -13.05 }, { x: 7, y: -1.2, z: -13.05 }]);
        inst(ctx, new THREE.CylinderGeometry(0.35, 0.35, 40, 10), std({ color: '#5a6a5a', metalness: 0.5, roughness: 0.5 }), [{ x: 0, y: 1.2, z: -12.9, rz: Math.PI / 2 }, { x: 0, y: 2.2, z: -12.9, rz: Math.PI / 2, s: 0.7 }]);
        // outflow pipes pouring sludge
        const pours = [{ x: -11.8, z: -6 }, { x: 11.8, z: -4 }, { x: 0, z: -12.6 }];
        inst(ctx, new THREE.CylinderGeometry(0.5, 0.5, 1.4, 12), std({ color: '#6a7060', metalness: 0.4 }), pours.map((p) => ({ x: p.x, y: 0.9, z: p.z, rx: p.z < -12 ? Math.PI / 2 : 0, rz: p.z < -12 ? 0 : Math.PI / 2 })));
        const pourTex = stripes('#7ab030', '#4a7a1a', 8, 1, false);
        pourTex.repeat.set(1, 3);
        inst(ctx, new THREE.CylinderGeometry(0.32, 0.4, 2.2, 10, 1, true), std({ map: pourTex, emissive: '#2a4a0a', roughness: 0.4 }), pours.map((p) => ({ x: p.x + Math.sign(p.x) * 0.8, y: -0.1, z: p.z + (p.z < -12 ? 0.8 : 0) })), { shadow: false });
        ctx.tick((dt, t) => { pourTex.offset.y = (t * 1.5) % 1; });
        // Numemon: green slugs with eye stalks, bobbing; they jump when you win
        const numemon = merge([
          part(new THREE.SphereGeometry(0.55, 12, 8), '#6ab84a', { y: 0.35, sx: 1, sy: 0.8, sz: 1.3 }), part(new THREE.SphereGeometry(0.38, 12, 8), '#6ab84a', { y: 0.7, z: 0.35 }),
          part(new THREE.CylinderGeometry(0.04, 0.05, 0.5, 5), '#6ab84a', { x: 0.14, y: 1.1, z: 0.38, rz: -0.2 }), part(new THREE.CylinderGeometry(0.04, 0.05, 0.5, 5), '#6ab84a', { x: -0.14, y: 1.1, z: 0.38, rz: 0.2 }),
          part(new THREE.SphereGeometry(0.1, 8, 6), '#ffffff', { x: 0.2, y: 1.38, z: 0.4 }), part(new THREE.SphereGeometry(0.1, 8, 6), '#ffffff', { x: -0.2, y: 1.38, z: 0.4 }),
          part(new THREE.SphereGeometry(0.045, 6, 4), '#111111', { x: 0.2, y: 1.39, z: 0.49 }), part(new THREE.SphereGeometry(0.045, 6, 4), '#111111', { x: -0.2, y: 1.39, z: 0.49 }),
          part(new THREE.TorusGeometry(0.12, 0.03, 4, 8, Math.PI), '#d04a6a', { y: 0.62, z: 0.7, rz: Math.PI }),
        ]);
        const nums = [{ x: -10.6, z: -7.5 }, { x: -11.4, z: -1.2 }, { x: 10.8, z: -8.5 }, { x: 11.2, z: 0.5 }, { x: -3.5, z: -10.6 }, { x: 4.2, z: -10.4 }, { x: 10.3, z: 4 }].map((p, i) => ({ ...p, cx: p.x, cz: p.z, ph: i * 1.3, ry: Math.atan2(-p.x, 4 - p.z) }));
        const joy = { until: -1 };
        ctx.on('win', (t) => { joy.until = t + 4; });
        anim(ctx, numemon, std({ vertexColors: true, roughness: 0.4 }), nums, (n, i, t) => {
          const jumping = t < joy.until;
          n.x = n.cx + Math.sin(t * 0.4 + n.ph) * 0.4;
          n.sy = 1 + Math.sin(t * 3 + n.ph) * 0.08; n.sx = n.sz = 1 / Math.sqrt(n.sy);
          n.y = jumping ? Math.abs(Math.sin(t * 6 + n.ph)) * 1.1 : 0;
        }, { shadow: true });
        // a rubber duck floating in the sludge, slowly circling
        const duck = merge([part(new THREE.SphereGeometry(0.5, 12, 8), '#ffd82a', { sy: 0.7, sz: 1.2 }), part(new THREE.SphereGeometry(0.32, 12, 8), '#ffd82a', { y: 0.45, z: 0.35 }), part(new THREE.ConeGeometry(0.12, 0.3, 6), '#ff8a1a', { y: 0.42, z: 0.72, rx: Math.PI / 2 }),
          part(new THREE.SphereGeometry(0.05, 6, 4), '#111111', { x: 0.14, y: 0.55, z: 0.6 }), part(new THREE.SphereGeometry(0.05, 6, 4), '#111111', { x: -0.14, y: 0.55, z: 0.6 })]);
        anim(ctx, duck, std({ vertexColors: true, roughness: 0.3 }), [{ s: 1.3 }], (d, i, t) => { const a = t * 0.06 + 3.6; d.x = Math.cos(a) * 14; d.z = Math.sin(a) * 9 - 2; d.y = -0.85 + Math.sin(t * 1.8) * 0.06; d.ry = -a; d.rz = Math.sin(t * 1.3) * 0.1; });
        // suspicious brown swirls, as Numemon do
        const swirl = merge([part(new THREE.TorusGeometry(0.28, 0.13, 6, 12), '#6a4224', { y: 0.12, rx: Math.PI / 2 }), part(new THREE.TorusGeometry(0.18, 0.1, 6, 12), '#6a4224', { y: 0.3, rx: Math.PI / 2 }), part(new THREE.ConeGeometry(0.12, 0.25, 8), '#6a4224', { y: 0.48 })]);
        inst(ctx, swirl, std({ vertexColors: true, roughness: 0.35 }), [{ x: -10.2, z: 3.2 }, { x: 9.8, z: -6.2 }, { x: -6, z: -10.2 }]);
        particles(ctx, 40, { color: '#1a1a10', size: 0.09, v: [0, 0, 0], wobble: 1.6, opacity: 0.9, box: [-13, 13, 0.3, 2.2, -12, 4] });
        particles(ctx, 60, { color: '#a8d878', size: 0.1, v: [0, -5, 0], wobble: 0, opacity: 0.6, box: [-14, 14, 0, 9, -14, -9] });
      },
    },

    // ===== Witchelny magic library =====
    {
      id: 'witchelny-library', name: 'Witchelny Library', rarity: 'epic', price: 950, icon: '📚',
      desc: 'A round hall of towering bookshelves. Books flap around, candles float and a magic circle turns under the board, flaring gold when your Digimon digivolve.',
      sky: ['#0a0604', '#1e120a', '#2e1c10'], fog: ['#24160e', 26, 60], hemi: ['#ffd8a8', '#2a1810', 1.25], sun: ['#ffe0b0', 2.0],
      slab: '#2a1a12', benchSlab: '#3a2416', slot: '#5a3a22', ped: '#c09040',
      tiles: { mine: '#3a64b8', enemy: '#a83a4a', enemyPlan: '#5a2430', rough: 0.45 },
      look: { pedMetal: 0.7, pedRough: 0.35 },
      surface: () => ({ tile: tileTex('#f0e8dc', 'rgba(110,70,30,0.6)', (g) => { g.strokeStyle = 'rgba(140,100,40,0.35)'; g.lineWidth = 2; g.beginPath(); g.arc(64, 64, 30, 0, TAU); g.stroke(); hexPath(g, 64, 64, 22); g.stroke(); }) }),
      props(ctx) {
        const parquet = canvasTex(128, 128, (g) => { const rng = DT.makeRng(909); for (let y = 0; y < 128; y += 16) for (let x = 0; x < 128; x += 64) { const o = (y / 16) % 2 ? 32 : 0; const v = 0.85 + rng() * 0.3; g.fillStyle = `rgb(${110 * v | 0},${70 * v | 0},${40 * v | 0})`; g.fillRect(x + o, y, 63, 15); g.fillRect(x + o - 64, y, 63, 15); } }, 10);
        plain(ctx, std({ map: parquet, roughness: 0.55 }), -1.1);
        platform(ctx, { sides: 48, r: 12.6, r2: 13, h: 1.4, top: std({ map: parquet.clone(), roughness: 0.5 }), side: '#3a2214' });
        hexEdges(ctx, { mine: '#ffe0a0', enemy: '#ffc0a0', opacity: 0.35 });
        // shelves around the hall: one merged unit of shelves and books, instanced along the wall
        const sh = [part(new THREE.BoxGeometry(3, 6, 0.8), '#4a2a16', { y: 3 })];
        const bc = ['#8a2a2a', '#2a4a8a', '#2a6a3a', '#7a5a2a', '#5a2a6a', '#c8a050', '#2a2a2a'];
        let seed = 1;
        const rr = () => { seed = (seed * 16807) % 2147483647; return seed / 2147483647; };
        for (let row = 0; row < 5; row++) for (let x = -1.35; x < 1.3;) { const w = 0.12 + rr() * 0.14, h = 0.7 + rr() * 0.3; sh.push(part(new THREE.BoxGeometry(w, h, 0.5), bc[(rr() * bc.length) | 0], { x: x + w / 2, y: 0.35 + row * 1.15 + h / 2, z: 0.2 })); x += w + 0.02; }
        for (let row = 0; row < 6; row++) sh.push(part(new THREE.BoxGeometry(3, 0.1, 0.85), '#5a3418', { y: 0.3 + row * 1.15, z: 0.05 }));
        const shelf = merge(sh);
        const walls = arc(15, 13.4, Math.PI * 0.82, Math.PI * 2.18).map((p) => ({ x: p.x, z: p.z, ry: -p.a - Math.PI / 2 }));
        inst(ctx, shelf, std({ vertexColors: true, roughness: 0.8 }), walls, { shadow: false });
        // the magic circle: two rune rings turning in opposite directions around the board
        const runes = canvasTex(256, 256, (g) => {
          g.clearRect(0, 0, 256, 256);
          g.strokeStyle = '#ffffff'; g.fillStyle = '#ffffff'; g.lineWidth = 3;
          for (const r of [124, 112, 70]) { g.beginPath(); g.arc(128, 128, r, 0, TAU); g.stroke(); }
          g.font = 'bold 14px serif';
          const glyphs = 'ᚠᚢᚦᚨᚱᚲᚷᚹᚺᚾᛁᛃᛇᛈᛉᛊᛏᛒᛖᛗᛚᛜᛞᛟ';
          for (let i = 0; i < 36; i++) { const a = (i / 36) * TAU; g.save(); g.translate(128 + Math.cos(a) * 118, 128 + Math.sin(a) * 118); g.rotate(a + Math.PI / 2); g.fillText(glyphs[i % glyphs.length], -5, 5); g.restore(); }
          g.beginPath(); for (let i = 0; i <= 6; i++) { const a = (i * 2 / 6) * TAU; const x = 128 + Math.cos(a) * 112, y = 128 + Math.sin(a) * 112; if (i) g.lineTo(x, y); else g.moveTo(x, y); } g.stroke();
          g.beginPath(); for (let i = 0; i <= 6; i++) { const a = ((i * 2 + 1) / 6) * TAU; const x = 128 + Math.cos(a) * 112, y = 128 + Math.sin(a) * 112; if (i) g.lineTo(x, y); else g.moveTo(x, y); } g.stroke();
        });
        const c1 = mesh(ctx, new THREE.PlaneGeometry(25, 25).rotateX(-Math.PI / 2), glow('#b07aff', 0.55, { map: runes }), { y: 0.004 });
        const c2 = mesh(ctx, new THREE.PlaneGeometry(31, 31).rotateX(-Math.PI / 2), glow('#7a5aff', 0.3, { map: runes }), { y: -1.09 });
        // floating books flapping in orbit, floating candles bobbing
        const page = new THREE.BoxGeometry(0.5, 0.04, 0.65);
        page.translate(0.25, 0, 0);
        const books = Array.from({ length: 20 }, (_, i) => ({ a0: (i / 20) * TAU, rad: 10.4 + (i % 3) * 0.9, h: 2 + (i % 4) * 0.6, sp: 0.12 + (i % 5) * 0.02, ph: i * 1.1, color: bc[i % 5] }));
        const pagesL = books.map((b) => ({ ...b, side: 1 })), pagesR = books.map((b) => ({ ...b, side: -1 }));
        const flapFn = (p, i, t) => {
          const a = p.a0 + t * p.sp;
          p.x = Math.cos(a) * p.rad; p.z = Math.sin(a) * p.rad * 0.85 - 1.5; p.y = p.h + Math.sin(t * 1.3 + p.ph) * 0.3;
          p.ry = -a + (p.side > 0 ? 0 : Math.PI); p.rx = 0; p.rz = p.side * (0.3 + 0.6 * Math.abs(Math.sin(t * 4 + p.ph)));
        };
        anim(ctx, page, std({ color: '#ffffff', roughness: 0.7 }), pagesL, flapFn);
        anim(ctx, page, std({ color: '#fff8e8', roughness: 0.7 }), pagesR, flapFn);
        const candles = rimSpots(ctx.rng, 80, 9.8, 12.2).slice(0, 12).map((p, i) => ({ x: p.x, z: p.z, by: 1.6 + (i % 3) * 0.5, ph: i * 1.4 }));
        anim(ctx, new THREE.CylinderGeometry(0.09, 0.09, 0.5, 8), std({ color: '#f4ecd8', roughness: 0.6 }), candles, (c, i, t) => { c.y = c.by + Math.sin(t * 1.1 + c.ph) * 0.2; });
        anim(ctx, new THREE.ConeGeometry(0.07, 0.2, 6), basic({ color: '#ffd070' }), candles.map((c) => ({ ...c })), (c, i, t) => { c.y = c.by + 0.36 + Math.sin(t * 1.1 + c.ph) * 0.2; c.sy = 1 + 0.25 * Math.sin(t * 15 + c.ph); });
        anim(ctx, new THREE.SphereGeometry(0.3, 8, 6), glow('#ffb040', 0.18), candles.map((c) => ({ ...c })), (c, i, t) => { c.y = c.by + 0.38 + Math.sin(t * 1.1 + c.ph) * 0.2; });
        const sparks = particles(ctx, 140, { color: '#d8b0ff', size: 0.12, v: [0, 0.4, 0], wobble: 0.6, opacity: 0.8, additive: true, box: [-15, 15, 0, 7, -14, 9] });
        // digivolve: the circle flares gold and spins up, the light warms
        const lit = ctx.light();
        lit.flashColor.set('#ffe8a0');
        const evo = { k: 0, spin: 0 };
        ctx.on('evolve', () => { evo.k = 1; lit.flash = 1.2; });
        const purple = C('#b07aff'), gold = C('#ffd27a');
        ctx.tick((dt, t) => {
          evo.k = Math.max(0, evo.k - dt * 0.45);
          evo.spin += dt * (0.05 + evo.k * 1.5);
          c1.rotation.y = evo.spin; c2.rotation.y = -evo.spin * 0.6;
          c1.material.color.copy(purple).lerp(gold, evo.k);
          c1.material.opacity = 0.5 + 0.1 * Math.sin(t * 2) + evo.k * 0.4;
          sparks.material.color.copy(C('#d8b0ff')).lerp(gold, evo.k);
          lit.tint.set('#ffd090'); lit.tintK = evo.k * 0.8;
        });
      },
    },

    // ===== Royal Knights' coliseum =====
    {
      id: 'knights-coliseum', name: 'Royal Knights Coliseum', rarity: 'legendary', price: 2600, icon: '🛡️',
      desc: 'A white marble arena at night with packed stands under floodlights. The crowd does the wave, jumps when the fight starts and goes wild when you win.',
      sky: ['#02040e', '#0a1430', '#1a2a50'], fog: ['#0a1020', 32, 72], hemi: ['#c8d8ff', '#2a2a3a', 1.1], sun: ['#ffffff', 2.3],
      slab: '#d8d0bf', benchSlab: '#b8ae98', slot: '#e2dccf', ped: '#e0b850',
      tiles: { mine: '#2f6ccf', enemy: '#c2363e', enemyPlan: '#6a2a30', rough: 0.25, metal: 0.05 },
      look: { pedMetal: 0.85, pedRough: 0.25, slotRough: 0.3 },
      surface: () => ({ tile: tileTex('#f0eee8', 'rgba(200,160,60,0.8)', (g) => { g.strokeStyle = 'rgba(150,150,170,0.3)'; g.lineWidth = 1.5; g.beginPath(); g.moveTo(10, 40); g.bezierCurveTo(50, 60, 70, 20, 120, 70); g.moveTo(30, 110); g.bezierCurveTo(60, 80, 90, 100, 110, 90); g.stroke(); }, 5) }),
      props(ctx) {
        const marble = canvasTex(128, 128, (g) => { const rng = DT.makeRng(1010); g.fillStyle = '#bdb6aa'; g.fillRect(0, 0, 128, 128); g.strokeStyle = 'rgba(120,120,140,0.35)'; for (let i = 0; i < 10; i++) { g.lineWidth = 0.5 + rng() * 1.5; g.beginPath(); g.moveTo(rng() * 128, 0); g.bezierCurveTo(rng() * 128, 40, rng() * 128, 90, rng() * 128, 128); g.stroke(); } }, 6);
        platform(ctx, { sides: 48, r: 11.9, r2: 11.9, h: 1.4, top: std({ map: marble, roughness: 0.45 }), side: '#d8d0c0' });
        hexEdges(ctx, { mine: '#ffd86a', enemy: '#ffd86a', opacity: 0.55, inner: 0.88 });
        // gold ring inlay around the arena
        mesh(ctx, new THREE.RingGeometry(11.35, 11.65, 64).rotateX(-Math.PI / 2), std({ color: '#e0b850', metalness: 0.8, roughness: 0.3 }), { y: 0.0 });
        // stands: stepped lathe of seats behind a wall
        const pts = [[11.9, -1.5], [11.9, 0.9]];
        for (let k = 0; k < 9; k++) pts.push([12.3 + k * 0.9, 0.9 + k * 0.6], [13.2 + k * 0.9, 0.9 + k * 0.6]);
        pts.push([20.6, 7.3], [20.6, -1.5]);
        mesh(ctx, new THREE.LatheGeometry(pts.map(([r, y]) => new THREE.Vector2(r, y)), 64), std({ color: '#3a3e4c', roughness: 0.9, side: THREE.DoubleSide }), { receive: true });
        mesh(ctx, new THREE.CylinderGeometry(11.95, 11.95, 0.25, 64, 1, true), std({ color: '#e0b850', metalness: 0.7, roughness: 0.3, side: THREE.DoubleSide }), { y: 0.95 });
        // the crowd
        const cols = ['#e84a4a', '#4a7ae8', '#f0d040', '#f4f4f4', '#7a4ae8', '#3ac87a', '#ff8a3a'];
        const crowd = [];
        const rng = DT.makeRng(1011);
        for (let k = 0; k < 9; k++) {
          const r = 12.65 + k * 0.9;
          const n = Math.floor((r * TAU) / 0.5);
          for (let i = 0; i < n; i++) {
            const a = (i / n) * TAU;
            const x = Math.cos(a) * r, z = Math.sin(a) * r;
            if (z > 9) continue;
            if (rng() < 0.12) continue;
            crowd.push({ x, y: 0.9 + k * 0.6, by: 0.9 + k * 0.6, z, a, ry: -a - Math.PI / 2, color: cols[(rng() * cols.length) | 0], ph: rng() * 6, s: 0.9 + rng() * 0.25 });
          }
        }
        const mood = { jump: -1, cheer: -1 };
        ctx.on('combat', (t) => { mood.jump = t + 3; });
        ctx.on('win', (t) => { mood.cheer = t + 6; });
        anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.8 }), crowd, (c, i, t) => {
          let h;
          if (t < mood.cheer) h = Math.abs(Math.sin(t * 9 + c.ph)) * 0.45;
          else if (t < mood.jump) h = Math.max(0, Math.sin(t * 7 + c.ph)) * 0.3;
          else { const w = Math.sin(c.a * 2 - t * 2.2); h = w > 0.85 ? (w - 0.85) * 3 : 0; }
          c.y = c.by + h;
        });
        // floodlight towers with beams
        const towers = [{ x: -14, z: -11 }, { x: 14, z: -11 }, { x: -17, z: 1 }, { x: 17, z: 1 }];
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.25, 0.35, 12, 8), '#5a5e6a', { y: 6 }), part(new THREE.BoxGeometry(2.4, 1.4, 0.4), '#2a2a30', { y: 12.4 })]), std({ vertexColors: true, metalness: 0.5 }), towers);
        const beamGeo = new THREE.ConeGeometry(2.6, 14, 18, 1, true);
        beamGeo.translate(0, -7, 0);
        towers.forEach((tw) => {
          const b = mesh(ctx, beamGeo, glow('#fff8e0', 0.07, { side: THREE.DoubleSide, fog: false }), { x: tw.x, y: 12.4, z: tw.z });
          b.quaternion.setFromUnitVectors(new THREE.Vector3(0, -1, 0), new THREE.Vector3(-tw.x * 0.7, -12.4, -tw.z * 0.7).normalize());
        });
        // the Royal Knights' banners on the wall
        const kc = ['#f4f4f4', '#e8b830', '#c8202a', '#2a5ad0', '#1a1a1a', '#8a2ab0', '#2ab06a', '#f08a2a', '#7ad0f0', '#b0b0c0', '#6a3a1a', '#f0f0a0', '#d04a8a'];
        const ban = arc(13, 11.97, Math.PI * 1.02, Math.PI * 1.98).map((p, i) => ({ x: p.x * 0.995, y: 0.85, z: p.z * 0.995, ry: -p.a - Math.PI / 2, color: kc[i], ph: i }));
        anim(ctx, bannerGeo(0.8, 1.5), std({ color: '#ffffff', side: THREE.DoubleSide, roughness: 0.8 }), ban, (b, i, t) => { b.rx = -0.05 + Math.sin(t * 1.4 + b.ph) * 0.06; });
        const conf = confetti(ctx, { n: 260 });
        ctx.on('win', () => conf.fire());
        // camera flashes in the stands
        const fl = [];
        for (let i = 0; i < 24; i++) fl.push(crowd[(i * 97) % crowd.length]);
        const flashPts = new Float32Array(fl.length * 3);
        const fg = new THREE.BufferGeometry();
        fg.setAttribute('position', new THREE.BufferAttribute(flashPts, 3));
        const fmat = new THREE.PointsMaterial({ color: '#ffffff', size: 0.6, map: softDot(), transparent: true, depthWrite: false, blending: THREE.AdditiveBlending });
        const fpts = new THREE.Points(fg, fmat);
        fpts.frustumCulled = false;
        ctx.g.add(fpts);
        ctx.tick((dt, t) => {
          fl.forEach((c, i) => { const on = Math.sin(t * 3.7 + i * 12.9) > 0.97 || (t < mood.cheer && Math.sin(t * 9 + i * 4.1) > 0.8); flashPts[i * 3] = c.x; flashPts[i * 3 + 1] = on ? c.y + 0.6 : -50; flashPts[i * 3 + 2] = c.z; });
          fg.attributes.position.needsUpdate = true;
        });
      },
    },

    // ===== Ultimate Battle Arena TV studio =====
    {
      id: 'battle-studio', name: 'Ultimate Battle Arena', rarity: 'epic', price: 1100, icon: '📺',
      desc: 'A TV studio stage with sweeping spotlights, LED walls and a studio audience. The lights turn red and blue for the fight and confetti drops when you win.',
      sky: '#000000', fog: ['#05030a', 30, 72], hemi: ['#a8a8ff', '#06060c', 0.85], sun: ['#ffffff', 1.9],
      slab: '#08080c', benchSlab: '#101018', slot: '#18181f', ped: '#c8ccd8',
      tiles: { mine: '#2a6ae0', enemy: '#e0304a', enemyPlan: '#6a1a2a', rough: 0.25, metal: 0.2 },
      look: { pedMetal: 0.9, pedRough: 0.15, slotRough: 0.2, slotMetal: 0.3 },
      surface: () => ({
        tile: tileTex('#e8eaf2', 'rgba(255,255,255,0.95)', (g) => { g.fillStyle = 'rgba(0,0,30,0.09)'; for (let y = 4; y < 128; y += 8) for (let x = 4; x < 128; x += 8) g.fillRect(x, y, 5, 5); }, 7),
        slot: canvasTex(64, 64, (g) => { g.fillStyle = '#121218'; g.fillRect(0, 0, 64, 64); g.strokeStyle = '#e8e8ff'; g.lineWidth = 3; g.strokeRect(4, 4, 56, 56); }),
      }),
      props(ctx) {
        plain(ctx, std({ color: '#0a0a10', roughness: 0.2, metalness: 0.4 }), -1.1);
        platform(ctx, { sides: 12, r: 12.6, r2: 13, h: 1.1, top: std({ color: '#121218', roughness: 0.18, metalness: 0.35 }), side: '#1a1a24', rot: Math.PI / 12 });
        // LED strip around the stage, cycling colors
        const strip = mesh(ctx, new THREE.TorusGeometry(12.8, 0.09, 6, 12), basic({ color: '#36e0ff' }), { y: -0.12, rx: Math.PI / 2, ry: 0 });
        strip.rotation.z = Math.PI / 12;
        const strip2 = mesh(ctx, new THREE.TorusGeometry(13.05, 0.07, 6, 12), basic({ color: '#ff3aa8' }), { y: -1.0, rx: Math.PI / 2 });
        strip2.rotation.z = Math.PI / 12;
        hexEdges(ctx, { mine: '#ffffff', enemy: '#ffffff', opacity: 0.55, additive: true, inner: 0.89 });
        // LED walls at the back corners with scrolling bars
        const led = canvasTex(128, 64, (g) => {
          const gr = g.createLinearGradient(0, 0, 128, 0);
          gr.addColorStop(0, '#ff2a5a'); gr.addColorStop(0.45, '#ff2a5a'); gr.addColorStop(0.55, '#2a6aff'); gr.addColorStop(1, '#2a6aff');
          g.fillStyle = gr; g.fillRect(0, 0, 128, 64);
          g.fillStyle = 'rgba(0,0,0,0.35)'; for (let y = 0; y < 64; y += 4) g.fillRect(0, y, 128, 1); for (let x = 0; x < 128; x += 4) g.fillRect(x, 0, 1, 64);
          g.fillStyle = '#ffffff'; g.font = 'bold 34px sans-serif'; g.fillText('VS', 44, 46);
        });
        const walls = [{ x: -11.8, z: -9.2, ry: 0.75 }, { x: 11.8, z: -9.2, ry: -0.75 }, { x: 0, z: -13.2, ry: 0 }];
        inst(ctx, new THREE.PlaneGeometry(7, 3.5), basic({ map: led }), walls.map((w) => ({ ...w, y: 1.8 })), { shadow: false });
        inst(ctx, new THREE.BoxGeometry(7.4, 3.9, 0.3), std({ color: '#141418' }), walls.map((w) => ({ ...w, y: 1.8, x: w.x - Math.sin(w.ry) * 0.2, z: w.z - Math.cos(w.ry) * 0.2 })));
        ctx.tick((dt, t) => { led.offset.x = Math.sin(t * 0.6) * 0.08; });
        // studio audience on risers at the sides, waving glowsticks
        const aud = [];
        for (const side of [-1, 1]) for (let row = 0; row < 4; row++) for (let i = 0; i < 12; i++) aud.push({ x: side * (14 + row * 1.0), y: -0.6 + row * 0.55, by: -0.6 + row * 0.55, z: -9 + i * 1.05, ry: side * -Math.PI / 2, color: '#1a1a24', ph: i * 1.7 + row });
        inst(ctx, new THREE.BoxGeometry(4.4, 1, 13.4), std({ color: '#16161e' }), [{ x: -15.4, y: -1.4, z: -3.2, sy: 2.2 }, { x: 15.4, y: -1.4, z: -3.2, sy: 2.2 }]);
        anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.9 }), aud, (c, i, t) => { c.y = c.by + Math.max(0, Math.sin(t * 5 + c.ph)) * 0.08; });
        const sticks = new Float32Array(aud.length * 3);
        const sg = new THREE.BufferGeometry();
        sg.setAttribute('position', new THREE.BufferAttribute(sticks, 3));
        const smat = new THREE.PointsMaterial({ color: '#7affd0', size: 0.3, map: softDot(), transparent: true, depthWrite: false, blending: THREE.AdditiveBlending });
        const sp = new THREE.Points(sg, smat);
        sp.frustumCulled = false;
        ctx.g.add(sp);
        // spotlights: beams from the rig above with pools of light on the floor
        const N = 6;
        const beamGeo = new THREE.CylinderGeometry(0.15, 1.6, 1, 16, 1, true);
        beamGeo.translate(0, -0.5, 0);
        const beams = [], pools = [];
        const poolTex = softDot();
        for (let i = 0; i < N; i++) {
          const b = mesh(ctx, beamGeo, glow('#ffffff', 0.06, { side: THREE.DoubleSide, fog: false }));
          const p = mesh(ctx, new THREE.CircleGeometry(1.8, 24).rotateX(-Math.PI / 2), glow('#ffffff', 0.25, { map: poolTex }), { y: 0.16 });
          p.renderOrder = 3;
          beams.push(b); pools.push(p);
        }
        const up = new THREE.Vector3(0, 1, 0), dir = new THREE.Vector3();
        const calm = C('#fff6e0'), red = C('#ff3a4a'), blue = C('#3a7aff'), stick = C('#7affd0');
        const st = { k: 0 };
        const conf = confetti(ctx);
        ctx.on('win', () => conf.fire());
        ctx.tick((dt, t) => {
          st.k = approach(st.k, ctx.state.phase === 'combat' ? 1 : 0, 2, dt);
          const s = 0.35 + st.k * 0.6;
          for (let i = 0; i < N; i++) {
            const ox = (i - (N - 1) / 2) * 3.4, oz = -12;
            const tx = Math.sin(t * s + i * 1.7) * 6.5, tz = Math.cos(t * s * 0.8 + i * 2.3) * 4.5 - 0.5;
            const top = new THREE.Vector3(ox, 14, oz);
            dir.set(tx - ox, 0.16 - 14, tz - oz);
            const len = dir.length();
            beams[i].position.copy(top);
            beams[i].scale.set(1, len, 1);
            beams[i].quaternion.setFromUnitVectors(up, dir.normalize().negate());
            pools[i].position.set(tx, 0.16, tz);
            const col = calm.clone().lerp(i % 2 ? red : blue, st.k);
            beams[i].material.color.copy(col); pools[i].material.color.copy(col);
          }
          strip.material.color.setHSL((t * 0.1) % 1, 0.9, 0.6);
          strip2.material.color.setHSL((t * 0.1 + 0.5) % 1, 0.9, 0.55);
          aud.forEach((c, i) => { sticks[i * 3] = c.x - Math.sign(c.x) * 0.3 + Math.sin(t * 4 + c.ph) * 0.2; sticks[i * 3 + 1] = c.y + 0.85; sticks[i * 3 + 2] = c.z; });
          sg.attributes.position.needsUpdate = true;
          smat.color.copy(stick).lerp(red, st.k);
        });
        // camera crane swinging on one side
        const crane = new THREE.Group();
        crane.position.set(11.6, 0, 3.2);
        const arm = merge([part(new THREE.BoxGeometry(5, 0.2, 0.2), '#3a3a44', { x: 1.8 }), part(new THREE.BoxGeometry(0.7, 0.5, 0.5), '#1a1a20', { x: -0.9 }), part(new THREE.BoxGeometry(0.6, 0.45, 0.4), '#202028', { x: 4.3, y: -0.3 }), part(new THREE.CylinderGeometry(0.14, 0.14, 0.2, 10), '#6a8aff', { x: 4.3, y: -0.3, z: 0.25, rx: Math.PI / 2 })]);
        const armM = new THREE.Mesh(arm, std({ vertexColors: true, metalness: 0.5 }));
        armM.position.y = 3;
        crane.add(new THREE.Mesh(new THREE.CylinderGeometry(0.15, 0.4, 3, 8), std({ color: '#2a2a32' })).translateY(1.5), armM);
        ctx.g.add(crane);
        ctx.tick((dt, t) => { crane.rotation.y = Math.PI * 0.75 + Math.sin(t * 0.3) * 0.6; armM.rotation.z = -0.2 + Math.sin(t * 0.45) * 0.15; });
      },
    },

    // ===== Trailmon station =====
    {
      id: 'trailmon-station', name: 'Trailmon Station', rarity: 'rare', price: 500, icon: '🚂',
      desc: 'A concrete station platform in the evening. Trailmon rumbles past on the tracks behind the board, and always pulls through when a fight starts.',
      sky: ['#3a5a8a', '#f0a070', '#ffd0a0'], fog: ['#e0b090', 32, 70], hemi: ['#ffe0c0', '#5a4a3a', 1.45], sun: ['#ffc890', 2.6],
      slab: '#5a5a5e', benchSlab: '#6a4a2a', slot: '#a8743a', ped: '#9aa0a8',
      tiles: { mine: '#2f6cb4', enemy: '#b8443a', enemyPlan: '#6a2e28', rough: 0.85 },
      look: { pedMetal: 0.6, pedRough: 0.4 },
      surface: () => ({
        tile: tileTex('#e6e4de', 'rgba(60,60,60,0.45)', (g) => { g.fillStyle = 'rgba(0,0,0,0.06)'; for (let i = 0; i < 4; i++) g.fillRect(20, 24 + i * 22, 88, 2); }),
        slot: stripes('#b07a40', '#7a5028', 6, 1, false),
      }),
      props(ctx) {
        // platform: concrete pavers with a yellow safety strip on the back edge
        const con = bricks('#a8a6a0', '#7a7874', { bw: 32, bh: 32, seed: 1212, vary: 0.06, repeat: 9 });
        const half = 11.8;
        mesh(ctx, new THREE.BoxGeometry(34, 1.4, half * 2 + 6), [std({ color: '#7a7874' }), std({ color: '#7a7874' }), std({ map: con, roughness: 0.95 }), std({ color: '#7a7874' }), std({ color: '#6a6864' }), std({ color: '#6a6864' })], { y: -0.72, z: 3, receive: true });
        mesh(ctx, new THREE.BoxGeometry(34, 0.02, 0.6), std({ color: '#f0c020', roughness: 0.7 }), { y: -0.01, z: -half + 0.7 });
        // track bed below the platform edge: ballast, sleepers and rails
        const TZ = -half - 2.4;
        mesh(ctx, new THREE.BoxGeometry(200, 0.6, 5), std({ map: speckle('#6a625a', ['#8a827a', '#4a443e'], { seed: 1213, repeat: 20 }), roughness: 1 }), { y: -1.7, z: TZ, receive: true });
        plain(ctx, std({ color: '#5a6a3a' }), -2.0);
        const sleepers = [];
        for (let x = -60; x <= 60; x += 1.1) sleepers.push({ x, y: -1.35, z: TZ });
        inst(ctx, new THREE.BoxGeometry(0.35, 0.15, 2.6), std({ color: '#4a3a2a' }), sleepers, { shadow: false });
        inst(ctx, new THREE.BoxGeometry(200, 0.15, 0.12), std({ color: '#9aa0a8', metalness: 0.8, roughness: 0.3 }), [{ y: -1.2, z: TZ - 0.75 }, { y: -1.2, z: TZ + 0.75 }], { shadow: false });
        // Trailmon: engine with a face lamp and carriages
        const car = merge([part(new THREE.BoxGeometry(3.6, 1.7, 2.3), '#ffffff', { y: 0.95 }), part(new THREE.CylinderGeometry(1.15, 1.15, 3.6, 12, 1, false, 0, Math.PI), '#ffffff', { y: 1.8, rz: Math.PI / 2, ry: 0 }),
          part(new THREE.BoxGeometry(3.2, 0.5, 2.36), '#f4e8c8', { y: 1.2 }), ...[-1.2, 1.2].flatMap((x) => [part(new THREE.CylinderGeometry(0.32, 0.32, 2.4, 10), '#2a2a2a', { x, y: 0.25, rx: Math.PI / 2 })])]);
        const engine = merge([part(new THREE.CylinderGeometry(1.1, 1.1, 3.2, 14), '#3a3a42', { y: 1.2, rz: Math.PI / 2 }), part(new THREE.BoxGeometry(1.4, 2.2, 2.2), '#2a2a30', { x: -1.6, y: 1.3 }),
          part(new THREE.CylinderGeometry(0.35, 0.45, 1, 10), '#2a2a30', { x: 0.9, y: 2.6 }), part(new THREE.CylinderGeometry(0.5, 0.5, 0.15, 14), '#ffe890', { x: 1.65, y: 1.4, rz: Math.PI / 2 }),
          ...[-1.2, 0, 1.2].map((x) => part(new THREE.CylinderGeometry(0.4, 0.4, 2.4, 10), '#1a1a1a', { x, y: 0.3, rx: Math.PI / 2 }))]);
        // the train runs every 16 s from t0, and pulls in when combat starts
        const train = { t0: -0.6, speed: 18, x: -80 };
        ctx.on('combat', (t) => { if (t - train.t0 > 7) train.t0 = t; });
        ctx.tick((dt, t) => { if (t - train.t0 > 16) train.t0 = t; train.x = -46 + (t - train.t0) * train.speed; });
        const cars = [0, 1, 2, 3, 4].map((i) => ({ i, y: -1.1, z: TZ, color: ['#c84a3a', '#3a7ac8', '#e0a830', '#3aa86a'][i % 4] }));
        anim(ctx, car, std({ vertexColors: true, roughness: 0.6 }), cars, (c, i) => { c.x = train.x - 4.2 - i * 3.9; }, { shadow: true });
        anim(ctx, engine, std({ vertexColors: true, roughness: 0.5, metalness: 0.3 }), [{ y: -1.1, z: TZ }], (e) => { e.x = train.x; }, { shadow: true });
        const steam = particles(ctx, 50, { color: '#ffffff', size: 1.1, v: [-1.5, 0.9, 0], wobble: 0.3, opacity: 0.0, box: [-3, 0.5, 0, 3, -0.6, 0.6] });
        ctx.tick(() => { steam.position.set(train.x + 0.9, 1.6, TZ); steam.material.opacity = Math.abs(train.x) < 40 ? 0.3 : 0; });
        // lamps, benches, a vending machine and a station sign along the platform
        const lamps = [{ x: -11, z: -9 }, { x: 11, z: -9 }, { x: -11, z: -2 }, { x: 11, z: -2 }, { x: -11, z: 5 }, { x: 11, z: 5 }];
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.08, 0.1, 3.6, 8), '#2a2e34', { y: 1.8 }), part(new THREE.BoxGeometry(0.9, 0.1, 0.3), '#2a2e34', { y: 3.6 })]), std({ vertexColors: true, metalness: 0.5 }), lamps);
        inst(ctx, new THREE.SphereGeometry(0.22, 10, 8), basic({ color: '#fff0c0' }), lamps.map((l) => ({ x: l.x, y: 3.42, z: l.z })), { shadow: false });
        inst(ctx, new THREE.SphereGeometry(0.7, 10, 8), glow('#ffd890', 0.18), lamps.map((l) => ({ x: l.x, y: 3.42, z: l.z })), { shadow: false });
        const bench = merge([part(new THREE.BoxGeometry(1.8, 0.1, 0.5), '#8a5a2a', { y: 0.5 }), part(new THREE.BoxGeometry(1.8, 0.5, 0.08), '#8a5a2a', { y: 0.8, z: -0.22 }), part(new THREE.BoxGeometry(0.08, 0.5, 0.45), '#2a2a2a', { x: -0.8, y: 0.25 }), part(new THREE.BoxGeometry(0.08, 0.5, 0.45), '#2a2a2a', { x: 0.8, y: 0.25 })]);
        inst(ctx, bench, std({ vertexColors: true }), [{ x: -11.2, z: -5.5, ry: Math.PI / 2 }, { x: 11.2, z: -5.5, ry: -Math.PI / 2 }, { x: -11.2, z: 1.5, ry: Math.PI / 2 }, { x: -4.5, z: -10.2 }, { x: 4.5, z: -10.2 }]);
        const vend = merge([part(new THREE.BoxGeometry(1.2, 2.2, 0.9), '#d8382a', { y: 1.1 }), part(new THREE.BoxGeometry(0.9, 1.2, 0.05), '#c8e8ff', { y: 1.4, z: 0.46 })]);
        inst(ctx, vend, std({ vertexColors: true, roughness: 0.4, emissive: '#2a1a10' }), [{ x: 11.3, z: 1.6, ry: -Math.PI / 2 }, { x: 11.3, z: 2.9, ry: -Math.PI / 2 }]);
        const signTex = canvasTex(256, 64, (g) => { g.fillStyle = '#f4f4f0'; g.fillRect(0, 0, 256, 64); g.fillStyle = '#2a6a3a'; g.fillRect(0, 48, 256, 16); g.fillStyle = '#1a1a1a'; g.font = 'bold 30px sans-serif'; g.textAlign = 'center'; g.fillText('FILE CITY', 128, 38); });
        mesh(ctx, new THREE.PlaneGeometry(4, 1), std({ map: signTex, roughness: 0.6, side: THREE.DoubleSide }), { x: 0, y: 2.6, z: -10.9 });
        inst(ctx, new THREE.CylinderGeometry(0.06, 0.06, 3, 6), std({ color: '#2a2e34' }), [{ x: -1.8, y: 1.5, z: -10.95 }, { x: 1.8, y: 1.5, z: -10.95 }]);
        // clock on a post that actually turns
        mesh(ctx, new THREE.CylinderGeometry(0.6, 0.6, 0.12, 20), std({ color: '#f4f4f0' }), { x: -6.5, y: 3, z: -10.5, rx: Math.PI / 2 });
        mesh(ctx, new THREE.CylinderGeometry(0.06, 0.06, 3, 6), std({ color: '#2a2e34' }), { x: -6.5, y: 1.5, z: -10.6 });
        const hand = mesh(ctx, new THREE.BoxGeometry(0.05, 0.5, 0.02).translate(0, 0.22, 0), basic({ color: '#111111' }), { x: -6.5, y: 3, z: -10.42 });
        ctx.tick((dt, t) => { hand.rotation.z = -t * 0.5; });
      },
    },

    // ===== Overdell graveyard =====
    {
      id: 'overdell-graveyard', name: 'Overdell Cemetery', rarity: 'rare', price: 450, icon: '🪦',
      desc: 'A misty graveyard at dusk with crooked tombstones and Bakemon drifting about. In combat the fog thickens and green lightning cracks.',
      sky: ['#0a0614', '#1e1430', '#3a2a40'], fog: ['#2a2238', 22, 56], hemi: ['#b0a0d8', '#141018', 1.1], sun: ['#b8ffd0', 1.5],
      slab: '#1a161e', benchSlab: '#2a2228', slot: '#4a3e44', ped: '#5a5a62',
      tiles: { mine: '#3462b0', enemy: '#a2384c', enemyPlan: '#52243a', rough: 0.8 },
      surface: () => ({ tile: tileTex('#dcdce0', 'rgba(30,30,40,0.5)', (g) => { g.fillStyle = 'rgba(60,80,60,0.25)'; g.beginPath(); g.arc(90, 40, 16, 0, TAU); g.fill(); g.beginPath(); g.arc(36, 96, 10, 0, TAU); g.fill(); }) }),
      props(ctx) {
        plain(ctx, std({ map: speckle('#2a2a24', ['#3a3a2e', '#1a1a16', '#4a4030'], { seed: 1313, repeat: 12 }), roughness: 1 }));
        platform(ctx, { sides: 13, r: 12.7, r2: 13.6, h: 1.4, top: speckle('#3e3a32', ['#4e4a3a', '#2e2a24', '#5a6a40'], { seed: 1314 }), side: '#2a2620', rot: 0.4 });
        hexEdges(ctx, { mine: '#b8ffd0', enemy: '#ffc8e0', opacity: 0.35 });
        // tombstones in three shapes, leaning
        const stoneMat = std({ color: '#8a8a90', roughness: 0.95, flatShading: true });
        const sp = rimSpots(ctx.rng, 160, 9.8, 12.4);
        const lean = (p) => ({ x: p.x, z: p.z, ry: -p.a + Math.PI / 2 + (ctx.rng() - 0.5) * 0.5, rx: (ctx.rng() - 0.5) * 0.25, rz: (ctx.rng() - 0.5) * 0.25, s: 0.8 + ctx.rng() * 0.5 });
        const round = merge([part(new THREE.BoxGeometry(0.8, 0.9, 0.2), '#ffffff', { y: 0.45 }), part(new THREE.CylinderGeometry(0.4, 0.4, 0.2, 12, 1, false, 0, Math.PI), '#ffffff', { y: 0.9, rx: Math.PI / 2, rz: Math.PI / 2, ry: Math.PI / 2 })]);
        inst(ctx, round, stoneMat, sp.slice(0, 18).map(lean));
        const cross = merge([part(new THREE.BoxGeometry(0.16, 1.3, 0.16), '#ffffff', { y: 0.65 }), part(new THREE.BoxGeometry(0.7, 0.16, 0.16), '#ffffff', { y: 0.95 })]);
        inst(ctx, cross, stoneMat, sp.slice(18, 30).map(lean));
        inst(ctx, new THREE.BoxGeometry(0.6, 0.5, 1.2), stoneMat, sp.slice(30, 36).map((p) => ({ ...lean(p), y: 0.25 })));
        // dead trees, iron fence along the back, a mausoleum
        const dead = merge([part(new THREE.CylinderGeometry(0.1, 0.22, 3, 5), '#2a2420', { y: 1.5, rz: 0.1 }), part(new THREE.CylinderGeometry(0.05, 0.09, 1.4, 4), '#2a2420', { x: 0.45, y: 2.3, rz: -0.8 }), part(new THREE.CylinderGeometry(0.04, 0.08, 1.2, 4), '#2a2420', { x: -0.4, y: 2, rz: 0.9 }), part(new THREE.CylinderGeometry(0.03, 0.05, 0.8, 4), '#2a2420', { x: 0.1, y: 3.2, rz: 0.4 })]);
        inst(ctx, dead, std({ vertexColors: true }), rimSpots(ctx.rng, 60, 10, 12.3, true).slice(0, 7).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6, s: 0.9 + ctx.rng() * 0.5 })));
        const fence = [];
        for (const p of arc(60, 13.2, Math.PI * 1.02, Math.PI * 1.98)) fence.push({ x: p.x, y: 0.6, z: p.z });
        inst(ctx, merge([part(new THREE.CylinderGeometry(0.035, 0.035, 1.2, 4), '#1a1a1e'), part(new THREE.ConeGeometry(0.07, 0.18, 4), '#1a1a1e', { y: 0.68 })]), std({ vertexColors: true, metalness: 0.6 }), fence, { shadow: false });
        mesh(ctx, new THREE.TorusGeometry(13.2, 0.04, 4, 64, Math.PI * 0.96), std({ color: '#1a1a1e', metalness: 0.6 }), { y: 1.0, rx: Math.PI / 2, rz: Math.PI * 1.02 });
        const maus = merge([part(new THREE.BoxGeometry(3, 2.4, 2.4), '#6a6a72', { y: 1.2 }), part(new THREE.ConeGeometry(2.3, 1.2, 4), '#4a4a52', { y: 3, ry: Math.PI / 4 }), part(new THREE.BoxGeometry(1, 1.6, 0.1), '#1a1a20', { y: 0.8, z: 1.21 }),
          part(new THREE.CylinderGeometry(0.14, 0.14, 2.4, 8), '#8a8a92', { x: -1.2, y: 1.2, z: 1.3 }), part(new THREE.CylinderGeometry(0.14, 0.14, 2.4, 8), '#8a8a92', { x: 1.2, y: 1.2, z: 1.3 })]);
        inst(ctx, maus, std({ vertexColors: true, roughness: 0.95 }), [{ x: -10.6, z: -7.2, ry: 0.85 }]);
        // jack-o'-lanterns and green flame lanterns
        const jacks = [{ x: 10.4, z: -2.5 }, { x: -10.7, z: 1.5 }, { x: 9.5, z: 3.2 }];
        inst(ctx, new THREE.SphereGeometry(0.4, 10, 8).scale(1, 0.8, 1), std({ color: '#e8701a', emissive: '#a03a00', emissiveIntensity: 0.8, roughness: 0.6 }), jacks.map((j) => ({ ...j, y: 0.3 })));
        // Bakemon: floating sheets with dark eyes
        const bake = merge([part(new THREE.SphereGeometry(0.5, 12, 8, 0, TAU, 0, Math.PI / 2), '#f4f4ff', { y: 0.9 }), part(new THREE.CylinderGeometry(0.5, 0.72, 0.9, 12, 1, true), '#f4f4ff', { y: 0.45 }),
          part(new THREE.SphereGeometry(0.08, 6, 4), '#1a1a2a', { x: 0.17, y: 1.0, z: 0.45 }), part(new THREE.SphereGeometry(0.08, 6, 4), '#1a1a2a', { x: -0.17, y: 1.0, z: 0.45 })]);
        const ghosts = Array.from({ length: 7 }, (_, i) => ({ a0: (i / 7) * TAU, rad: 10.6 + (i % 3) * 0.7, h: 1.2 + (i % 2) * 0.8, ph: i * 1.9, sp: 0.1 + (i % 3) * 0.03 }));
        const haste = { k: 0 };
        anim(ctx, bake, std({ vertexColors: true, transparent: true, opacity: 0.82, emissive: '#7a8a9a', emissiveIntensity: 0.6, side: THREE.DoubleSide }), ghosts, (g, i, t) => {
          const a = g.a0 + t * g.sp * (1 + haste.k * 2);
          g.x = Math.cos(a) * g.rad; g.z = Math.sin(a) * g.rad * 0.9 - 1; g.y = g.h + Math.sin(t * 1.6 + g.ph) * 0.35;
          g.ry = -a + Math.PI; g.rz = Math.sin(t * 2 + g.ph) * 0.1;
        });
        // ground mist: big soft sprites drifting low
        particles(ctx, 70, { color: '#a8b0c8', size: 4, v: [0.25, 0, 0.1], wobble: 0.1, opacity: 0.1, box: [-18, 18, -0.4, 0.8, -15, 11] });
        particles(ctx, 50, { color: '#9aff9a', size: 0.14, v: [0, 0.2, 0], wobble: 0.8, opacity: 0.9, additive: true, box: [-15, 15, 0.3, 3, -13, 8] });
        // combat weather: thick fog, green lightning
        const fog = ctx.fog();
        const bolt = lightning(ctx, { color: '#8aff9a', spots: arc(7, 12.6, Math.PI * 0.95, Math.PI * 2.05), flash: 1.5, flashColor: '#b8ffc8' });
        let next = 0;
        ctx.tick((dt, t) => {
          const k = ctx.state.phase === 'combat' ? 1 : 0;
          haste.k = approach(haste.k, k, 1, dt);
          fog.near = lerp(22, 9, haste.k); fog.far = lerp(56, 38, haste.k);
          if (k && t > next) { bolt.strike(t); next = t + 2 + ctx.rng() * 3; }
        });
      },
    },

    // ===== Server continent pyramid =====
    {
      id: 'server-pyramid', name: 'Server Pyramid', rarity: 'epic', price: 900, icon: '🔺',
      desc: 'The flat top of a great pyramid in the night desert. Glyph bands light up as the stages rise and the corner beams blaze when the fight starts.',
      sky: ['#020414', '#0a1636', '#1a2a50'], fog: ['#0e1830', 30, 70], hemi: ['#a8b8ff', '#2a2018', 1.15], sun: ['#c8d8ff', 2.0],
      slab: '#3a2e20', benchSlab: '#4a3a28', slot: '#b8945a', ped: '#e0b850',
      tiles: { mine: '#2c66b8', enemy: '#b8463a', enemyPlan: '#5e2a22', rough: 0.85 },
      look: { pedMetal: 0.8, pedRough: 0.3 },
      surface: () => ({ tile: tileTex('#f0e4cc', 'rgba(110,80,40,0.6)', (g) => { g.strokeStyle = 'rgba(120,90,50,0.2)'; g.lineWidth = 3; g.beginPath(); g.arc(64, 56, 12, 0, TAU); g.moveTo(64, 68); g.lineTo(64, 96); g.moveTo(50, 80); g.lineTo(78, 80); g.stroke(); }) }),
      props(ctx) {
        plain(ctx, std({ map: speckle('#5a4a3a', ['#6a5a48', '#4a3c2e'], { seed: 1414, repeat: 12 }), roughness: 1 }), -9);
        const dunes = farSpots(ctx.rng, 14, 30, 50).map((p) => ({ x: p.x, y: -9.2, z: p.z, sx: 8 + ctx.rng() * 6, sy: 2 + ctx.rng() * 2, sz: 5 + ctx.rng() * 3, ry: p.a }));
        inst(ctx, new THREE.SphereGeometry(1, 16, 8, 0, TAU, 0, Math.PI / 2), std({ color: '#5a4a38' }), dunes, { shadow: false });
        // stepped pyramid: square tiers going down
        const stone = bricks('#a8885a', '#6a5434', { bw: 64, bh: 24, seed: 1415, repeat: 4 });
        for (let k = 0; k < 6; k++) platform(ctx, { sides: 4, r: 14 + k * 2.2, r2: 14 + k * 2.2, h: 1.5, top: k ? std({ map: stone, roughness: 1 }) : std({ map: bricks('#b8986a', '#7a6444', { bw: 48, bh: 48, seed: 1416, repeat: 7 }), roughness: 1 }), side: std({ map: stone, roughness: 1 }), rot: Math.PI / 4, y: -k * 1.5 });
        hexEdges(ctx, { mine: '#8ae0ff', enemy: '#ffb07a', opacity: 0.3 });
        const half = 14 * Math.SQRT1_2;
        // obelisks with glowing tips at the corners
        const corners = [[-1, -1], [1, -1], [-1, 1], [1, 1]].map(([sx, sz]) => ({ x: sx * (half - 1.2), z: sz * (half - 1.2) })).filter((c) => c.z < 0 || Math.abs(c.x) > 9);
        inst(ctx, merge([part(new THREE.BoxGeometry(0.9, 5, 0.9), '#c8a870', { y: 2.5 }), part(new THREE.ConeGeometry(0.64, 0.9, 4), '#e0c060', { y: 5.45, ry: Math.PI / 4 })]), std({ vertexColors: true, roughness: 0.8 }), corners);
        // corner light beams, brighter when combat starts
        const beams = corners.map((c) => mesh(ctx, new THREE.CylinderGeometry(0.35, 0.35, 40, 10, 1, true), glow('#6ad8ff', 0.25, { side: THREE.DoubleSide, fog: false }), { x: c.x, y: 25, z: c.z }));
        const blaze = { k: 0, until: -1 };
        ctx.on('combat', (t) => { blaze.until = t + 2.5; });
        // sphinx statues on the sides
        const sphinx = merge([part(new THREE.BoxGeometry(1.2, 0.8, 2.8), '#b8986a', { y: 0.4 }), part(new THREE.BoxGeometry(1, 1, 1), '#c0a070', { y: 1.2, z: 1.0 }), part(new THREE.BoxGeometry(1.3, 0.9, 0.3), '#3a5aa8', { y: 1.15, z: 0.75 }), part(new THREE.BoxGeometry(0.4, 0.3, 1.2), '#b8986a', { x: 0.35, y: 0.15, z: 1.8 }), part(new THREE.BoxGeometry(0.4, 0.3, 1.2), '#b8986a', { x: -0.35, y: 0.15, z: 1.8 })]);
        inst(ctx, sphinx, std({ vertexColors: true, roughness: 0.9 }), [{ x: -9.4, z: -3.5, ry: Math.PI / 2 }, { x: 9.2, z: -3.5, ry: -Math.PI / 2 }, { x: -3.4, z: -8.6, ry: 0 }, { x: 3.4, z: -8.6, ry: 0 }]);
        // braziers with blue flames
        const bz = [{ x: -9.4, z: 1.2 }, { x: 9.2, z: 1.2 }, { x: -6.5, z: -8.7 }, { x: 6.5, z: -8.7 }];
        inst(ctx, new THREE.CylinderGeometry(0.5, 0.25, 0.9, 10), std({ color: '#c8a050', metalness: 0.7 }), bz.map((b) => ({ ...b, y: 0.45 })));
        anim(ctx, new THREE.ConeGeometry(0.32, 1.1, 8).translate(0, 0.55, 0), glow('#5ac8ff', 0.85), bz.map((b, i) => ({ ...b, y: 0.9, ph: i * 2 })), (f, i, t) => { f.sy = 1 + 0.25 * Math.sin(t * 12 + f.ph); f.ry = t; });
        // glyph bands on the terrace edges: one more band lights up each stage
        const glyph = canvasTex(256, 32, (g) => { const rng = DT.makeRng(1417); g.clearRect(0, 0, 256, 32); g.strokeStyle = '#ffffff'; g.lineWidth = 2.5; for (let x = 8; x < 256; x += 22) { const k = (rng() * 5) | 0; g.beginPath(); if (k === 0) g.arc(x + 7, 16, 7, 0, TAU); else if (k === 1) { g.moveTo(x, 26); g.lineTo(x + 7, 6); g.lineTo(x + 14, 26); } else if (k === 2) { g.moveTo(x + 7, 4); g.lineTo(x + 7, 28); g.moveTo(x, 12); g.lineTo(x + 14, 12); } else if (k === 3) { g.rect(x, 8, 14, 16); } else { g.moveTo(x, 16); g.quadraticCurveTo(x + 7, 0, x + 14, 16); g.quadraticCurveTo(x + 7, 32, x, 16); } g.stroke(); } });
        glyph.repeat.set(4, 1);
        const bands = [];
        for (let k = 0; k < 6; k++) {
          const r = (14 + k * 2.2) * 1.0005;
          const m = mesh(ctx, new THREE.CylinderGeometry(r, r, 0.6, 4, 1, true).rotateY(Math.PI / 4), glow('#5ad8ff', 0.0, { map: glyph, side: THREE.FrontSide }), { y: -0.75 - k * 1.5 });
          bands.push(m);
        }
        ctx.tick((dt, t) => {
          blaze.k = approach(blaze.k, t < blaze.until ? 1 : 0, 3, dt);
          beams.forEach((b, i) => { b.material.opacity = 0.18 + 0.05 * Math.sin(t * 2 + i) + blaze.k * 0.45; b.scale.x = b.scale.z = 1 + blaze.k * 1.5; });
          const lit = Math.min(6, ctx.state.stage);
          bands.forEach((b, k) => { b.material.opacity = approach(b.material.opacity, k < lit ? 0.75 + 0.2 * Math.sin(t * 2 - k) : 0.04, 2, dt); });
          glyph.offset.x = (t * 0.01) % 1;
        });
        particles(ctx, 120, { color: '#b8c8ff', size: 0.1, v: [1.2, 0, 0.2], wobble: 0.3, opacity: 0.5, box: [-18, 18, -1, 4, -15, 11] });
      },
    },

    // ===== File City, growing as you play =====
    {
      id: 'file-city', name: 'File City', rarity: 'rare', price: 600, icon: '🏘️',
      desc: 'Jijimon\'s village square. New buildings rise out of the ground each stage, from a few huts to a full town with a clock tower.',
      sky: ['#6ab8f0', '#bfe4ff', '#f0fff0'], fog: ['#cfe8f0', 34, 74], hemi: ['#ffffff', '#6a8a4a', 1.55], sun: ['#fff2d8', 2.8],
      slab: '#6a5038', benchSlab: '#7a5a3a', slot: '#a8845a', ped: '#c8b898',
      tiles: { mine: '#2f72c0', enemy: '#c2483e', enemyPlan: '#76322c', rough: 0.8 },
      surface: () => ({ tile: tileTex('#f4ecdc', 'rgba(110,80,50,0.5)', (g) => { g.strokeStyle = 'rgba(120,90,60,0.25)'; g.lineWidth = 2; for (let i = 0; i < 3; i++) { g.beginPath(); g.moveTo(24, 40 + i * 24); g.lineTo(104, 40 + i * 24); g.stroke(); } }) }),
      props(ctx) {
        // farmland around, cobbled plaza on top
        const farm = canvasTex(64, 64, (g) => { g.fillStyle = '#6aa040'; g.fillRect(0, 0, 64, 64); g.fillStyle = '#8ab850'; g.fillRect(0, 0, 32, 32); g.fillRect(32, 32, 32, 32); g.fillStyle = 'rgba(80,60,30,0.25)'; for (let y = 2; y < 64; y += 6) g.fillRect(0, y, 64, 2); }, 18);
        plain(ctx, std({ map: farm, roughness: 1 }));
        const cob = bricks('#b8a080', '#8a7454', { bw: 24, bh: 20, seed: 1515, vary: 0.15, repeat: 9 });
        platform(ctx, { sides: 48, r: 12.6, r2: 13.6, h: 1.4, top: cob, side: '#6a8a40' });
        // grass ring around the plaza edge
        mesh(ctx, new THREE.RingGeometry(11.6, 12.65, 48).rotateX(-Math.PI / 2), std({ map: speckle('#5a9a40', ['#7aba50', '#4a8a30'], { seed: 1516 }), roughness: 1 }), { y: -0.01 });
        // buildings with the stage they appear in; each rises from the ground with a bounce
        const hut = (body, roof, h = 1.4, r = 1.1) => merge([part(new THREE.CylinderGeometry(r, r * 1.05, h, 12), body, { y: h / 2 }), part(new THREE.ConeGeometry(r * 1.45, h * 0.9, 12), roof, { y: h + h * 0.45 }), part(new THREE.BoxGeometry(0.45, 0.7, 0.1), '#5a3a1a', { y: 0.35, z: r })]);
        const house = (body, roof, w = 1.6, h = 1.3) => merge([part(new THREE.BoxGeometry(w, h, w * 0.8), body, { y: h / 2 }), part(new THREE.ConeGeometry(w * 0.85, 0.9, 4), roof, { y: h + 0.45, ry: Math.PI / 4, sz: 0.8 }), part(new THREE.BoxGeometry(0.4, 0.7, 0.1), '#5a3a1a', { y: 0.35, z: w * 0.4 })]);
        const tower = merge([part(new THREE.BoxGeometry(1.4, 5, 1.4), '#e8dcc8', { y: 2.5 }), part(new THREE.ConeGeometry(1.2, 1.6, 4), '#a83a3a', { y: 5.8, ry: Math.PI / 4 }), part(new THREE.CylinderGeometry(0.5, 0.5, 0.08, 16), '#ffffff', { y: 4.2, z: 0.72, rx: Math.PI / 2 }), part(new THREE.BoxGeometry(0.05, 0.4, 0.02), '#1a1a1a', { y: 4.3, z: 0.77 })]);
        const well = merge([part(new THREE.CylinderGeometry(0.6, 0.65, 0.6, 12, 1, true), '#8a8a8a', { y: 0.3 }), part(new THREE.CylinderGeometry(0.55, 0.55, 0.05, 12), '#3a6aa8', { y: 0.45 }), part(new THREE.BoxGeometry(0.08, 1.2, 0.08), '#6a4a2a', { x: 0.55, y: 0.9 }), part(new THREE.BoxGeometry(0.08, 1.2, 0.08), '#6a4a2a', { x: -0.55, y: 0.9 }), part(new THREE.ConeGeometry(0.85, 0.5, 4), '#8a4a2a', { y: 1.7, ry: Math.PI / 4 })]);
        const shop = merge([part(new THREE.BoxGeometry(2.4, 1.6, 1.6), '#f0d8a0', { y: 0.8 }), part(new THREE.BoxGeometry(2.6, 0.12, 0.9), '#e84a4a', { y: 1.3, z: 1.1, rx: 0.3 }), part(new THREE.BoxGeometry(2.6, 0.5, 1.8), '#3a7ac8', { y: 1.85 })]);
        const jijimon = merge([part(new THREE.CylinderGeometry(2, 2.1, 2, 16), '#e8c890', { y: 1 }), part(new THREE.ConeGeometry(2.9, 2.6, 16), '#c8a040', { y: 3.3 }), part(new THREE.BoxGeometry(0.7, 1.1, 0.1), '#5a3a1a', { y: 0.55, z: 2.02 }), part(new THREE.CylinderGeometry(0.2, 0.2, 1, 8), '#7a7a7a', { x: 1, y: 4.2 })]);
        const B = [
          { geo: jijimon, x: -10.8, z: -6.8, ry: 0.7, st: 1 }, { geo: hut('#f0d8b0', '#b07a3a'), x: 10.6, z: -5.6, ry: -0.6, st: 1 }, { geo: hut('#e8e0c8', '#8a6a3a', 1.2, 0.9), x: -11, z: 2.2, ry: 1.4, st: 1 },
          { geo: well, x: 10.4, z: 1.4, st: 2 }, { geo: shop, x: 4.6, z: -10.6, ry: 0, st: 2 }, { geo: house('#f4f0e0', '#3a6aa8'), x: -4.6, z: -10.8, st: 3 },
          { geo: house('#f0e0c8', '#c84a3a', 1.4), x: 11.2, z: -1.6, ry: -Math.PI / 2, st: 3 }, { geo: hut('#d8e8c8', '#4a8a3a', 1.6, 1.2), x: -0.4, z: -11.6, st: 4 }, { geo: house('#ffe0e8', '#a84a8a'), x: -11.6, z: -2.4, ry: Math.PI / 2, st: 4 },
          { geo: tower, x: 8.4, z: -9.2, ry: -0.5, st: 5 }, { geo: house('#e8f0ff', '#2a8a8a', 1.8, 1.8), x: -8.2, z: -9.6, ry: 0.6, st: 6 }, { geo: shop, x: 11.8, z: 4.6, ry: -Math.PI / 2, st: 6 }, { geo: hut('#f8f0e0', '#d07030', 2, 1.4), x: 1.2, z: -14.2, st: 7 },
        ];
        const mat = std({ vertexColors: true, roughness: 0.8 });
        for (const b of B) {
          const m = mesh(ctx, b.geo, mat, { x: b.x, z: b.z, ry: b.ry || 0, shadow: true });
          b.m = m; b.k = 0; b.vel = 0;
          m.scale.setScalar(0.001);
        }
        // crops and fences, appear from stage 2 and 3
        const crops = [];
        for (let i = 0; i < 6; i++) for (let j = 0; j < 3; j++) crops.push({ x: -12.5 + j * 0.5, y: 0.2, z: 4 + i * 0.5, s: 0.4 });
        const cm = inst(ctx, new THREE.ConeGeometry(0.25, 0.6, 5), std({ color: '#e8c040' }), crops, { shadow: false });
        // chimney smoke from Jijimon's house
        const smoke = particles(ctx, 30, { color: '#e8e8e8', size: 0.8, v: [0.2, 0.7, 0], wobble: 0.2, opacity: 0.45, box: [-10.2, -9.2, 4.2, 8, -7.4, -6] });
        void smoke;
        let first = true;
        ctx.tick((dt, t) => {
          const stage = ctx.state.stage;
          for (const b of B) {
            const target = stage >= b.st ? 1 : 0;
            if (first) { b.k = target; b.vel = 0; }
            // spring toward the target scale for a little bounce when a building rises
            b.vel += ((target - b.k) * 60 - b.vel * 9) * dt;
            b.k = Math.max(0, b.k + b.vel * dt);
            const s = Math.max(0.001, b.k);
            b.m.scale.set(s, s, s);
            b.m.visible = s > 0.01;
          }
          cm.visible = stage >= 2;
          first = false;
        });
        // Jijimon's village flowers
        const fc = ['#ff6a8a', '#ffe04a', '#ffffff', '#b07aff'];
        inst(ctx, new THREE.SphereGeometry(0.12, 6, 4), std({ color: '#ffffff' }), rimSpots(ctx.rng, 160, 11.7, 12.5).slice(0, 70).map((p, i) => ({ x: p.x, y: 0.1, z: p.z, color: fc[i % 4] })), { shadow: false });
      },
    },

    // ===== A holiday Digital World =====
    {
      id: 'holiday-world', name: 'Holiday Digital World', rarity: 'rare', price: 500, icon: '🎄',
      desc: 'A snowy night square with a giant tree, presents, candy canes and gingerbread houses. The lights twinkle and burst gold when you win.',
      sky: ['#050a1e', '#14204a', '#2a2a5a'], fog: ['#1a2244', 26, 64], hemi: ['#c8d8ff', '#3a3040', 1.25], sun: ['#ffd8b0', 1.9],
      slab: '#3a2a2a', benchSlab: '#2a5a3a', slot: '#2a7a4a', ped: '#f4f4f4',
      tiles: { mine: '#2f6cc0', enemy: '#c43a3a', enemyPlan: '#6a2626', rough: 0.55 },
      surface: () => ({
        slot: canvasTex(64, 64, (g) => { g.fillStyle = '#2a8a4a'; g.fillRect(0, 0, 64, 64); g.fillStyle = '#d82a2a'; g.fillRect(27, 0, 10, 64); g.fillRect(0, 27, 64, 10); }),
        ped: canvasTex(64, 64, (g) => { g.fillStyle = '#ffffff'; g.fillRect(0, 0, 64, 64); g.strokeStyle = '#e02a3a'; g.lineWidth = 7; for (let i = -64; i < 128; i += 18) { g.beginPath(); g.moveTo(i, 0); g.lineTo(i + 64, 64); g.stroke(); } }),
      }),
      props(ctx) {
        plain(ctx, std({ map: speckle('#dfe6f4', ['#ffffff', '#c4d0e4'], { seed: 1616, repeat: 12, n: 500 }), roughness: 1 }));
        platform(ctx, { sides: 48, r: 12.6, r2: 13.6, h: 1.4, top: speckle('#eef2fa', ['#ffffff', '#d4dcec'], { seed: 1617 }), side: '#b8c4d8' });
        hexEdges(ctx, { mine: '#ffd86a', enemy: '#ffd86a', opacity: 0.4 });
        // the tree on the left side, with blinking bulbs
        const TX = -11.4, TZ = -5;
        const tree = merge([part(new THREE.CylinderGeometry(0.3, 0.4, 1.2, 8), '#5a3a1a', { y: 0.6 }), ...[0, 1, 2, 3].map((k) => part(new THREE.ConeGeometry(2.4 - k * 0.5, 2.2, 10), '#1f6a3a', { y: 1.8 + k * 1.3 })), part(new THREE.OctahedronGeometry(0.45, 0), '#ffd84a', { y: 7.3 })]);
        mesh(ctx, tree, std({ vertexColors: true, roughness: 0.8 }), { x: TX, z: TZ, shadow: true });
        const bulbs = [];
        for (let i = 0; i < 70; i++) { const h = 1.4 + (i / 70) * 5.4, r = 2.3 * (1 - (h - 1.2) / 6.6) + 0.15, a = i * 2.4; bulbs.push({ x: TX + Math.cos(a) * r, y: h, z: TZ + Math.sin(a) * r, ph: i * 1.7 }); }
        const bc = ['#ff3a3a', '#ffd83a', '#3aff6a', '#3a8aff', '#ff6ae0'].map(C);
        const gold = C('#ffe680');
        const bm = inst(ctx, new THREE.SphereGeometry(0.11, 6, 4), basic({ color: '#ffffff' }), bulbs, { shadow: false });
        const party = { until: -1 };
        ctx.on('win', (t) => { party.until = t + 5; });
        const tmp = new THREE.Color();
        ctx.tick((dt, t) => {
          const win = t < party.until;
          bulbs.forEach((b, i) => {
            const on = Math.sin(t * (win ? 9 : 2.2) + b.ph) > -0.2;
            tmp.copy(win ? gold : bc[i % bc.length]).multiplyScalar(on ? 1 : 0.15);
            bm.setColorAt(i, tmp);
          });
          bm.instanceColor.needsUpdate = true;
        });
        // presents piled around
        const pc = ['#d82a2a', '#2a8a4a', '#3a6ad8', '#f0c030', '#c84ad8'];
        const presents = [];
        rimSpots(ctx.rng, 120, 9.8, 12.2).slice(0, 18).forEach((p, i) => { const s = 0.4 + ctx.rng() * 0.4; presents.push({ x: p.x, y: s / 2, z: p.z, s, ry: ctx.rng() * 3, color: pc[i % pc.length] }); });
        inst(ctx, new THREE.BoxGeometry(1, 1, 1), std({ color: '#ffffff', roughness: 0.5 }), presents);
        inst(ctx, new THREE.BoxGeometry(1.02, 1.02, 0.2), std({ color: '#ffe680', metalness: 0.4, roughness: 0.3 }), presents.map((p) => ({ ...p, sx: p.s, sy: p.s, sz: p.s, color: undefined })), { shadow: false });
        // candy canes
        const cane = new THREE.TubeGeometry(new THREE.CatmullRomCurve3([new THREE.Vector3(0, 0, 0), new THREE.Vector3(0, 2, 0), new THREE.Vector3(0.25, 2.45, 0), new THREE.Vector3(0.55, 2.3, 0), new THREE.Vector3(0.6, 2, 0)]), 20, 0.1, 8);
        const caneTex = stripes('#ffffff', '#e02a3a', 10, 1);
        caneTex.repeat.set(4, 1);
        caneTex.rotation = 0.4;
        inst(ctx, cane, std({ map: caneTex, roughness: 0.35 }), rimSpots(ctx.rng, 80, 10, 12, true).slice(0, 8).map((p) => ({ x: p.x, z: p.z, ry: ctx.rng() * 6 })));
        // gingerbread houses
        const ginger = merge([part(new THREE.BoxGeometry(1.6, 1.1, 1.3), '#b8743a', { y: 0.55 }), part(new THREE.ConeGeometry(1.25, 0.9, 4), '#f4f4f4', { y: 1.5, ry: Math.PI / 4 }), part(new THREE.BoxGeometry(0.4, 0.6, 0.06), '#e02a3a', { y: 0.3, z: 0.66 }), part(new THREE.BoxGeometry(0.3, 0.3, 0.06), '#3aff8a', { x: 0.5, y: 0.75, z: 0.66 }), part(new THREE.BoxGeometry(0.3, 0.3, 0.06), '#3aff8a', { x: -0.5, y: 0.75, z: 0.66 })]);
        inst(ctx, ginger, std({ vertexColors: true, roughness: 0.7 }), [{ x: 10.6, z: -6.6, ry: -0.7 }, { x: 4.5, z: -10.8, ry: 0.1 }, { x: -4.2, z: -11, ry: -0.1 }, { x: 11, z: 0.6, ry: -1.4 }]);
        // string lights along the back rim
        const sl = arc(60, 12.2, Math.PI * 1.02, Math.PI * 1.98).map((p, i) => ({ x: p.x, y: 1.4 + Math.abs(Math.sin(i * 0.52)) * -0.5, z: p.z, ph: i }));
        const slm = inst(ctx, new THREE.SphereGeometry(0.09, 6, 4), basic({ color: '#ffffff' }), sl, { shadow: false });
        ctx.tick((dt, t) => { const win = t < party.until; sl.forEach((s, i) => { tmp.copy(win ? gold : bc[(i + ((t * 2) | 0)) % bc.length]); slm.setColorAt(i, tmp); }); slm.instanceColor.needsUpdate = true; });
        inst(ctx, new THREE.CylinderGeometry(0.05, 0.05, 1.6, 5), std({ color: '#3a2a2a' }), arc(7, 12.2, Math.PI * 1.02, Math.PI * 1.98).map((p) => ({ x: p.x, y: 0.8, z: p.z })), { shadow: false });
        const snow = particles(ctx, 340, { color: '#ffffff', size: 0.13, v: [0.1, -0.8, 0.05], wobble: 0.35, opacity: 0.9, box: [-17, 17, 0, 13, -15, 12] });
        const sparkle = particles(ctx, 80, { color: '#ffe680', size: 0.22, v: [0, 1.2, 0], wobble: 0.6, opacity: 0, additive: true, box: [-14, 14, 0, 6, -12, 6] });
        ctx.tick((dt, t) => { sparkle.material.opacity = t < party.until ? 0.9 : 0; snow.material.size = t < party.until ? 0.18 : 0.13; });
      },
    },

    // ===== Gargomon carnival =====
    {
      id: 'gargomon-carnival', name: 'Gargomon Carnival', rarity: 'epic', price: 1000, icon: '🎡',
      desc: 'A boardwalk carnival at dusk: a turning ferris wheel, a carousel, striped tents and balloons. Fireworks go up when you win.',
      sky: ['#1a0a3a', '#7a3a8a', '#ff9a5a'], fog: ['#5a2a5a', 30, 68], hemi: ['#ffc8e0', '#3a2030', 1.35], sun: ['#ffb070', 2.3],
      slab: '#3a2418', benchSlab: '#5a3a24', slot: '#8a5a34', ped: '#f4f4f4',
      tiles: { mine: '#2f6ac8', enemy: '#d0404a', enemyPlan: '#6e2430', rough: 0.6 },
      surface: () => ({
        ped: canvasTex(64, 64, (g) => { for (let i = 0; i < 8; i++) { g.fillStyle = i % 2 ? '#e02a3a' : '#ffffff'; g.beginPath(); g.moveTo(32, 32); g.arc(32, 32, 46, (i / 8) * TAU, ((i + 1) / 8) * TAU); g.fill(); } }),
        slot: stripes('#9a6a3a', '#7a5028', 8, 1, false),
      }),
      props(ctx) {
        const planks = canvasTex(128, 128, (g) => { const rng = DT.makeRng(1717); for (let y = 0; y < 128; y += 16) { const v = 0.8 + rng() * 0.3; g.fillStyle = `rgb(${150 * v | 0},${100 * v | 0},${60 * v | 0})`; g.fillRect(0, y, 128, 15); g.fillStyle = 'rgba(0,0,0,0.3)'; g.fillRect((rng() * 128) | 0, y, 2, 15); } }, 8);
        plain(ctx, std({ color: '#3a2a4a', roughness: 1 }), -1.1);
        platform(ctx, { sides: 48, r: 12.6, r2: 13, h: 1.4, top: planks, side: '#5a3a24' });
        hexEdges(ctx, { mine: '#ffe8a0', enemy: '#ffe8a0', opacity: 0.3 });
        // ferris wheel on the back right, turning, gondolas stay level
        const FW = { x: 11.2, y: 5.6, z: -10.6, r: 5 };
        const wheel = new THREE.Group();
        wheel.position.set(FW.x, FW.y, FW.z);
        wheel.rotation.y = -0.6;
        const rim = merge([part(new THREE.TorusGeometry(FW.r, 0.1, 6, 40), '#f4f4f4'), part(new THREE.TorusGeometry(FW.r * 0.6, 0.07, 6, 30), '#f4f4f4'), ...Array.from({ length: 12 }, (_, i) => part(new THREE.BoxGeometry(0.06, FW.r * 2, 0.06), '#e8e8e8', { rz: (i / 12) * Math.PI }))]);
        const rimM = new THREE.Mesh(rim, std({ vertexColors: true, metalness: 0.4, roughness: 0.4 }));
        wheel.add(rimM);
        ctx.g.add(wheel);
        inst(ctx, new THREE.BoxGeometry(0.2, 6.8, 0.2), std({ color: '#d8d8d8', metalness: 0.4 }), [-1, 1].map((s) => ({ x: FW.x + Math.cos(-0.6) * s * 1.6, y: 2.4, z: FW.z - Math.sin(-0.6) * s * 1.6, rz: s * 0.28, ry: -0.6 })));
        const gc = ['#ff4a6a', '#4ac8ff', '#ffd84a', '#7aff6a', '#c86aff', '#ff8a3a'];
        const gond = Array.from({ length: 12 }, (_, i) => ({ i, color: gc[i % gc.length] }));
        const cosA = Math.cos(-0.6), sinA = Math.sin(-0.6);
        const bulbs = [];
        for (let i = 0; i < 36; i++) bulbs.push({ i });
        const bl = inst(ctx, new THREE.SphereGeometry(0.1, 6, 4), basic({ color: '#fff0a0' }), bulbs, { moving: true, shadow: false });
        anim(ctx, merge([part(new THREE.CylinderGeometry(0.5, 0.42, 0.6, 8), '#ffffff', { y: -0.5 }), part(new THREE.ConeGeometry(0.55, 0.3, 8), '#ffffff', { y: 0.0 })]), std({ color: '#ffffff', roughness: 0.5 }), gond, (g, i, t) => {
          const a = t * 0.18 + (i / 12) * TAU;
          const lx = Math.cos(a) * FW.r, ly = Math.sin(a) * FW.r;
          g.x = FW.x + lx * cosA; g.y = FW.y + ly; g.z = FW.z - lx * sinA; g.ry = -0.6;
        });
        ctx.tick((dt, t) => {
          rimM.rotation.z = t * 0.18;
          bulbs.forEach((b, i) => { const a = t * 0.18 + (i / 36) * TAU; const lx = Math.cos(a) * FW.r, ly = Math.sin(a) * FW.r; setInst(bl, i, { x: FW.x + lx * cosA, y: FW.y + ly, z: FW.z - lx * sinA, s: Math.sin(t * 4 + i) > 0 ? 1 : 0.4 }); });
          bl.instanceMatrix.needsUpdate = true;
        });
        // carousel on the left: spinning platform, horses bobbing, striped roof
        const CX = -11.2, CZ = -4.2;
        const roofTex = stripes('#e02a3a', '#fff4e0', 16, 1);
        mesh(ctx, new THREE.CylinderGeometry(2.6, 2.6, 0.3, 24), std({ color: '#c8a050', metalness: 0.4 }), { x: CX, y: 0.15, z: CZ, receive: true });
        mesh(ctx, new THREE.ConeGeometry(3, 1.4, 16), std({ map: roofTex, roughness: 0.6 }), { x: CX, y: 3.7, z: CZ, shadow: true });
        mesh(ctx, new THREE.CylinderGeometry(0.3, 0.3, 3.2, 10), std({ color: '#ffd84a', metalness: 0.5 }), { x: CX, y: 1.7, z: CZ });
        const horse = merge([part(new THREE.BoxGeometry(0.3, 0.35, 0.8), '#ffffff', { y: 0 }), part(new THREE.BoxGeometry(0.22, 0.45, 0.25), '#ffffff', { y: 0.25, z: 0.45, rx: -0.4 }), part(new THREE.CylinderGeometry(0.03, 0.03, 3, 5), '#ffd84a', { y: 0.6 }),
          ...[[-0.1, -0.3], [0.1, -0.3], [-0.1, 0.3], [0.1, 0.3]].map(([x, z]) => part(new THREE.BoxGeometry(0.07, 0.35, 0.07), '#ffffff', { x, y: -0.3, z }))]);
        const hc = ['#ffffff', '#f0c8a0', '#c8e0ff', '#ffd0e0', '#e0ffc8', '#f0f0a0'];
        const horses = Array.from({ length: 6 }, (_, i) => ({ i, color: hc[i] }));
        anim(ctx, horse, std({ vertexColors: true, roughness: 0.4 }), horses, (h, i, t) => { const a = t * 0.6 + (i / 6) * TAU; h.x = CX + Math.cos(a) * 1.9; h.z = CZ + Math.sin(a) * 1.9; h.y = 1.1 + Math.sin(t * 2.4 + i * 1.3) * 0.25; h.ry = -a; });
        // striped tents
        const tentTex = stripes('#3a6ad8', '#fff4e0', 12, 1);
        const tent = merge([part(new THREE.CylinderGeometry(1.4, 1.4, 1.4, 12), '#ffffff', { y: 0.7 }), part(new THREE.ConeGeometry(1.7, 1.5, 12), '#ffffff', { y: 2.15 })]);
        inst(ctx, tent, std({ map: tentTex, roughness: 0.7 }), [{ x: 10.9, z: -2.6 }, { x: 3.8, z: -11 }, { x: -4.5, z: -10.9, s: 0.8 }, { x: 11.2, z: 2.8, s: 0.8 }]);
        // balloon bunches bobbing
        const bcol = ['#ff4a6a', '#ffd84a', '#4ac8ff', '#7aff6a', '#c86aff'];
        const balloons = [];
        [{ x: -9.8, z: 2.6 }, { x: 10.2, z: 5.4 }, { x: -2, z: -11.6 }, { x: 7, z: -10.2 }].forEach((p, k) => { for (let i = 0; i < 5; i++) balloons.push({ bx: p.x + Math.cos(i * 1.3) * 0.35, bz: p.z + Math.sin(i * 1.3) * 0.35, by: 2.6 + (i % 3) * 0.35, x: 0, y: 0, z: 0, sy: 1.2, color: bcol[(i + k) % 5], ph: i + k * 3 }); });
        anim(ctx, new THREE.SphereGeometry(0.3, 10, 8), std({ color: '#ffffff', roughness: 0.25 }), balloons, (b, i, t) => { b.x = b.bx + Math.sin(t * 0.8 + b.ph) * 0.12; b.z = b.bz; b.y = b.by + Math.sin(t * 1.2 + b.ph) * 0.12; });
        // string lights on poles across the back
        const poles = arc(6, 12.3, Math.PI * 1.1, Math.PI * 1.9);
        inst(ctx, new THREE.CylinderGeometry(0.06, 0.06, 2.6, 5), std({ color: '#3a2a2a' }), poles.map((p) => ({ x: p.x, y: 1.3, z: p.z })), { shadow: false });
        const lights = [];
        for (let i = 0; i < poles.length - 1; i++) for (let k = 0; k < 10; k++) { const u = k / 10; const a = poles[i], b = poles[i + 1]; lights.push({ x: lerp(a.x, b.x, u), y: 2.5 - Math.sin(u * Math.PI) * 0.7, z: lerp(a.z, b.z, u), color: bcol[(i * 10 + k) % 5] }); }
        inst(ctx, new THREE.SphereGeometry(0.08, 6, 4), basic({ color: '#ffffff' }), lights, { shadow: false });
        // fireworks: one now and then, a big show when you win
        const fw = fireworks(ctx, { max: 6 });
        let next = 3;
        const show = { left: 0, t: 0 };
        ctx.on('win', () => { show.left = 7; });
        ctx.tick((dt, t) => {
          if (t > next) { fw.launch(t, (ctx.rng() - 0.5) * 22, 3.5 + ctx.rng() * 2.5, -9 - ctx.rng() * 2); next = t + 4 + ctx.rng() * 4; }
          if (show.left > 0) { show.t -= dt; if (show.t <= 0) { show.left--; show.t = 0.35; fw.launch(t, (ctx.rng() - 0.5) * 20, 3 + ctx.rng() * 3, -6 - ctx.rng() * 5); } }
        });
        particles(ctx, 60, { color: '#ffe0a0', size: 0.1, v: [0, 0.3, 0], wobble: 0.6, opacity: 0.8, additive: true, box: [-14, 14, 0.5, 4, -12, 6] });
      },
    },
  ];
}
