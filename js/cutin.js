// Special cut-in and tracking shot for the arena renderer (policy and framing: js/cutin-core.js). Presentation
// only: it reads the shown fight and the caster's animated model; the simulation never sees it.
// Native counterpart: BattleView.Cinematic.cs, SpecialCutInView.cs and MatchScreen.SpecialCutIn.cs.
import * as THREE from 'three';
import { CUT, CutInDirector, pickHead, headFromBone, headEstimate } from './cutin-core.js';

const LAYER = 5; // the cut-in camera sees only the caster and its own lights
const STRIPS = 24, LINES = 30, BURST = 22;
const cutHash = (i) => { const x = Math.sin(i * 12.9898 + 78.233) * 43758.5453; return x - Math.floor(x); };
// t = amount of a, mixed in sRGB like the native HudMix
const cutMix = (a, b, t) => {
  const x = a.getRGB({}, THREE.SRGBColorSpace), y = b.getRGB({}, THREE.SRGBColorSpace);
  return new THREE.Color().setRGB(x.r * t + y.r * (1 - t), x.g * t + y.g * (1 - t), x.b * t + y.b * (1 - t), THREE.SRGBColorSpace);
};

export class CutIn {
  constructor(r3d) {
    this.r3d = r3d;
    this.director = new CutInDirector();
    this.camera = new THREE.PerspectiveCamera((CUT.headFov * 180) / Math.PI, 2, 0.02, 80);
    this.camera.layers.set(LAYER);
    const hemi = new THREE.HemisphereLight('#ffffff', '#334455', 2.2);
    const key = new THREE.DirectionalLight('#ffffff', 2.2);
    key.position.set(2, 4, 5);
    this.rim = new THREE.DirectionalLight('#ffffff', 3.4);
    this.rim.position.set(-3, 2, -4);
    for (const light of [hemi, key, this.rim]) { light.layers.set(LAYER); r3d.scene.add(light); }
    this.lights = [key, this.rim];
    this.target = new THREE.WebGLRenderTarget(16, 16, { samples: 4 });
    this.ortho = new THREE.OrthographicCamera(0, 1, 0, -1, -1, 1);
    this.overlay = new THREE.Scene();
    const root = new THREE.Group();
    root.scale.y = -1; // pixel coordinates, y down
    this.overlay.add(root);
    const layer = (tris, order) => {
      const g = new THREE.BufferGeometry();
      g.setAttribute('position', new THREE.BufferAttribute(new Float32Array(tris * 9), 3));
      g.setAttribute('color', new THREE.BufferAttribute(new Float32Array(tris * 12), 4));
      const m = new THREE.Mesh(g, new THREE.MeshBasicMaterial({ vertexColors: true, transparent: true, side: THREE.DoubleSide, depthTest: false, depthWrite: false }));
      m.frustumCulled = false; m.renderOrder = order; root.add(m);
      return { mesh: m, count: 0 };
    };
    this.back = layer(2 + STRIPS * 2 + LINES * 2 + 2 + BURST, 0);
    this.front = layer(16, 2);
    const pg = new THREE.BufferGeometry();
    pg.setAttribute('position', new THREE.BufferAttribute(new Float32Array(18), 3));
    pg.setAttribute('uv', new THREE.BufferAttribute(new Float32Array(12), 2));
    this.picture = new THREE.Mesh(pg, new THREE.MeshBasicMaterial({ map: this.target.texture, transparent: true, side: THREE.DoubleSide, depthTest: false, depthWrite: false }));
    this.picture.frustumCulled = false; this.picture.renderOrder = 1; root.add(this.picture);
    if (!document.getElementById('cutinStyle')) {
      const style = document.createElement('style');
      style.id = 'cutinStyle';
      style.textContent = `.cutin-text{position:absolute;inset:0;pointer-events:none;z-index:4;overflow:hidden}.cutin-text.hidden{display:none}
.cutin-text b,.cutin-text i{position:absolute;transform:translate(-50%,-50%);white-space:nowrap;font-family:var(--font-display);font-style:normal;text-shadow:3px 3px 0 rgba(0,0,0,.8)}
.cutin-text b{font-weight:700;color:#fff;text-transform:uppercase;letter-spacing:.06em}.cutin-text i{font-weight:500;letter-spacing:.04em}`;
      document.head.appendChild(style);
    }
    this.text = document.createElement('div');
    this.text.className = 'cutin-text hidden';
    this.text.innerHTML = '<b></b><i></i>';
    r3d.arena.appendChild(this.text);
    this.view = null; this.shot = false; this.frameData = null;
  }

  // A new fight on screen (render3d resetFx).
  reset() { this.end(); this.director = new CutInDirector(); }

  // render3d consume() 'cast': maybe start a cinematic for this caster.
  offer(u, v, ctx, color) {
    if (!v || v.hasModel !== true) return;
    const prefs = (window.DT.Client && window.DT.Client.cutIns && window.DT.Client.cutIns()) || { mode: 'mine', motion: true };
    const { game, ui } = ctx || {};
    const mine = !!game && (!ui || ui.viewing == null) && this.r3d.viewTeam(u.team) === 0 && (u.owner == null || !game.human || u.owner === game.human.id);
    const allowed = prefs.motion !== false && !document.hidden && !!game && game.phase === 'combat';
    const clip = v.clips && v.clips.special ? v.clips.special.duration : 0;
    if (this.director.offer(u.id, mine, prefs.mode, allowed, clip, this.r3d.speed || 1)) { this.end(); this.view = v; this.unit = u; this.color = color; }
  }

  // Before labels project: advance the timeline and blend the arena camera onto the caster.
  frame(dt) {
    const d = this.director, r = this.r3d;
    d.update(dt, r.speed || 1);
    if (d.unit == null || !this.view || !this.unit || !this.unit.alive || r.viewOf(d.unit) !== this.view) d.abort();
    if (d.unit == null) { this.end(); return; }
    const f = this.caster(this.view);
    if (!f) { d.abort(); return; }
    this.frameData = f;
    r.applyView();
    const shot = d.frame(f.min, f.max, f.facing, r.view.yaw, r.camera.aspect, (r.camera.fov * Math.PI) / 180, r.wide ? 18 : 13, [...r.views.values()].filter((o) => o !== this.view && o.d && o.d.cu).map((o) => o.group.position));
    if (!shot) return;
    const w = THREE.MathUtils.clamp(shot.weight, 0, 1);
    const base = r.camera.position.clone(), look = r.view.target.clone();
    r.camera.position.lerpVectors(base, new THREE.Vector3(shot.eye.x, shot.eye.y, shot.eye.z), w);
    r.camera.lookAt(look.lerp(new THREE.Vector3(shot.target.x, shot.target.y, shot.target.z), w));
    r.camera.updateMatrixWorld();
    this.shot = true;
    const panel = d.cutIn;
    r.labels.style.opacity = panel ? String(1 - 2 * panel.dim) : '';
  }

  // The caster's animated world bounds (skinned vertices sampled, so root motion, flight and scale count),
  // head point and facing.
  caster(v) {
    const obj = v.visual;
    if (!obj) return null;
    v.group.updateWorldMatrix(true, true);
    const box = new THREE.Box3(), p = new THREE.Vector3(), part = new THREE.Box3();
    if (!v.cutRig) {
      const nodes = [], objects = [];
      let vertices = 0;
      obj.traverse((o) => {
        let depth = 0;
        for (let q = o.parent; q && q !== obj; q = q.parent) depth++;
        nodes.push({ name: o.name, depth }); objects.push(o);
        if (o.isMesh && o.geometry.attributes.position) vertices += o.geometry.attributes.position.count;
      });
      const head = pickHead(nodes);
      v.cutRig = { head: head && { kind: head.kind, objects: head.indices.map((i) => objects[i]) }, stride: Math.max(1, Math.floor(vertices / 6000)) };
    }
    const stride = v.cutRig.stride;
    obj.traverse((o) => {
      if (!o.isMesh || !o.visible || !o.geometry.attributes.position) return;
      if (o.isSkinnedMesh) {
        const n = o.geometry.attributes.position.count;
        for (let i = 0; i < n; i += stride) { o.getVertexPosition(i, p); box.expandByPoint(p.applyMatrix4(o.matrixWorld)); }
      } else {
        if (!o.geometry.boundingBox) o.geometry.computeBoundingBox();
        box.union(part.copy(o.geometry.boundingBox).applyMatrix4(o.matrixWorld));
      }
    });
    if (box.isEmpty()) return null;
    const min = { x: box.min.x, y: box.min.y, z: box.min.z }, max = { x: box.max.x, y: box.max.y, z: box.max.z };
    const facing = v.body.rotation.y;
    const estimate = headEstimate(min, max, facing);
    let head = null;
    const rig = v.cutRig.head;
    if (rig) {
      const sum = new THREE.Vector3();
      for (const o of rig.objects) sum.add(o.getWorldPosition(p));
      sum.multiplyScalar(1 / rig.objects.length);
      const parent = rig.objects[0].parent && rig.objects[0].parent !== obj ? rig.objects[0].parent.getWorldPosition(new THREE.Vector3()) : null;
      head = headFromBone(rig.kind, { x: sum.x, y: sum.y, z: sum.z }, parent && { x: parent.x, y: parent.y, z: parent.z }, min, max, estimate.size);
    }
    return { min, max, facing, head: head || { x: estimate.x, y: estimate.y, z: estimate.z }, size: estimate.size, source: head ? rig.kind : 'bounds' };
  }

  // After the arena render: the caster's head view and the panel over the board.
  render() {
    const d = this.director, panel = d.cutIn, r = this.r3d;
    if (!panel || !this.frameData) { this.text.classList.add('hidden'); return; }
    const W = r.canvas.clientWidth, H = r.canvas.clientHeight, u = this.unit;
    const ally = r.viewTeam(u.team) === 0;
    const element = new THREE.Color(this.color || '#ffd6a8');
    const tier = new THREE.Color(window.DT.TIER_COLORS[u.line.tier] || '#9aa4b1');
    const deep = new THREE.Color('#0a1020'), white = new THREE.Color('#ffffff');
    const bh = THREE.MathUtils.clamp(H * 0.34, 130, 330), k = 0.45, dir = ally ? 1 : -1, half = W * 0.5 + bh;
    const angle = (-7 * Math.PI) / 180, ax = Math.cos(angle), ay = Math.sin(angle), cx = W * 0.5, cy = H * 0.42;
    const shift = panel.offset * (half + W * 0.5 + bh) * dir;
    const P = (uu, v) => [cx + ax * (uu + shift) - ay * v, cy + ay * (uu + shift) + ax * v];
    const upper = (uu) => P(uu + (k * bh) / 2, -bh / 2), lower = (uu) => P(uu - (k * bh) / 2, bh / 2);
    // geometry writers
    const begin = (L) => { L.count = 0; };
    const tri = (L, a, b, c, col, alpha) => {
      const pos = L.mesh.geometry.attributes.position.array, cl = L.mesh.geometry.attributes.color.array;
      for (const [x, y] of [a, b, c]) {
        const i = L.count++;
        pos.set([x, y, 0], i * 3); cl.set([col.r, col.g, col.b, alpha], i * 4);
      }
    };
    const quad = (L, a, b, c, d2, col, alpha) => { tri(L, a, b, c, col, alpha); tri(L, a, c, d2, col, alpha); };
    const finish = (L) => {
      const g = L.mesh.geometry;
      g.setDrawRange(0, L.count); g.attributes.position.needsUpdate = true; g.attributes.color.needsUpdate = true;
    };
    const B = this.back, F = this.front;
    begin(B); begin(F);
    quad(B, [0, 0], [W, 0], [W, H], [0, H], new THREE.Color(0, 0, 0), panel.dim);
    for (let i = 0; i < STRIPS; i++) {
      const u0 = -half + (2 * half * i) / STRIPS, u1 = -half + (2 * half * (i + 1)) / STRIPS + 0.5, toward = ally ? (i + 0.5) / STRIPS : 1 - (i + 0.5) / STRIPS;
      quad(B, upper(u0), upper(u1), lower(u1), lower(u0), cutMix(element, deep, 0.18 + 0.5 * toward * toward), 0.94);
    }
    for (let i = 0; i < LINES; i++) {
      const a = cutHash(i * 3 + 1), b = cutHash(i * 3 + 2), c = cutHash(i * 3 + 3);
      const v = (a - 0.5) * (bh - 14), length = W * (0.12 + 0.3 * b), thick = 1.2 + 2.2 * c, span = 2 * half + length;
      const travel = (c * span + panel.time * (1400 + 900 * a)) % span, uu = ally ? half - travel : -half + travel - length;
      quad(B, P(uu, v), P(uu + length, v), P(uu + length, v + thick), P(uu, v + thick), white, (40 + 90 * b) / 255);
    }
    const win = W * 0.46, mu0 = ally ? W * 0.02 : -win - W * 0.02, mu1 = mu0 + win;
    quad(B, upper(mu0), upper(mu1), lower(mu1), lower(mu0), cutMix(element, white, 0.72), 0.96);
    const focus = (mu0 + mu1) / 2;
    for (let i = 0; i < BURST; i++) {
      const ang = (Math.PI * 2 * i) / BURST + panel.time * 0.6, s = Math.sin(ang), cs = Math.cos(ang);
      const reach = Math.min(win * 0.75, (bh * 0.48) / Math.max(Math.abs(s), 0.05)), w = reach * 0.05;
      const tx = focus + cs * reach, ty = s * reach;
      tri(B, P(focus + cs * bh * 0.16, s * bh * 0.16), P(tx - s * w, ty + cs * w), P(tx + s * w, ty - cs * w), white, 0.6); // .38 in the native sRGB blend; three blends in linear light
    }
    // window sides, edges, rarity stripe
    const line = (L, a, b, width, col, alpha) => {
      const dx = b[0] - a[0], dy = b[1] - a[1], l = Math.hypot(dx, dy) || 1, nx = (-dy / l) * width / 2, ny = (dx / l) * width / 2;
      quad(L, [a[0] + nx, a[1] + ny], [b[0] + nx, b[1] + ny], [b[0] - nx, b[1] - ny], [a[0] - nx, a[1] - ny], col, alpha);
    };
    line(F, upper(mu0), lower(mu0), 4, white, 0.92); line(F, upper(mu1), lower(mu1), 4, white, 0.92);
    line(F, upper(-half), upper(half), 3, white, 0.94); line(F, lower(-half), lower(half), 3, white, 0.94);
    quad(F, P(-half + (k * bh) / 2, -bh / 2 + 6), P(half + (k * bh) / 2, -bh / 2 + 6), P(half + (k * bh) / 2, -bh / 2 + 11), P(-half + (k * bh) / 2, -bh / 2 + 11), tier, 0.95);
    finish(B); finish(F);
    // the live head view, mapped affinely onto the slanted window
    const t0 = mu0 - (k * bh) / 2, tw = win + k * bh;
    const corners = [[mu0 + (k * bh) / 2, -bh / 2], [mu1 + (k * bh) / 2, -bh / 2], [mu1 - (k * bh) / 2, bh / 2], [mu0 - (k * bh) / 2, bh / 2]];
    const pos = this.picture.geometry.attributes.position.array, uv = this.picture.geometry.attributes.uv.array;
    [0, 1, 2, 0, 2, 3].forEach((c, i) => {
      const [uu, v] = corners[c], [x, y] = P(uu, v);
      pos.set([x, y, 0], i * 3); uv.set([(uu - t0) / tw, 0.5 - v / bh], i * 2);
    });
    this.picture.geometry.attributes.position.needsUpdate = true; this.picture.geometry.attributes.uv.needsUpdate = true;
    // render the caster alone into the window texture
    const pr = r.r.getPixelRatio(), tw2 = Math.max(64, Math.ceil(tw * pr)), th2 = Math.max(64, Math.ceil(bh * pr));
    if (this.target.width !== tw2 || this.target.height !== th2) this.target.setSize(tw2, th2);
    const f = this.frameData, pose = d.headFrame(f.head, f.size, f.facing, ally);
    this.rim.color.copy(element);
    this.setLayer(true);
    if (pose) {
      this.camera.aspect = tw / bh; this.camera.updateProjectionMatrix();
      this.camera.position.set(pose.eye.x, pose.eye.y, pose.eye.z); this.camera.lookAt(pose.target.x, pose.target.y, pose.target.z);
      this.camera.updateMatrixWorld();
      const bg = r.scene.background, clear = r.r.getClearColor(new THREE.Color()), alpha = r.r.getClearAlpha();
      r.scene.background = null;
      r.r.setRenderTarget(this.target); r.r.setClearColor(0x000000, 0); r.r.clear();
      r.r.render(r.scene, this.camera);
      r.r.setRenderTarget(null); r.r.setClearColor(clear, alpha); r.scene.background = bg;
    }
    this.ortho.right = W; this.ortho.bottom = -H; this.ortho.updateProjectionMatrix();
    const auto = r.r.autoClear;
    r.r.autoClear = false; r.r.clearDepth(); r.r.render(this.overlay, this.ortho); r.r.autoClear = auto;
    // names on the text side (DOM, above the canvas)
    const textU = ally ? -W * 0.24 : W * 0.24, size = THREE.MathUtils.clamp(bh * 0.22, 22, 54);
    const [nx, ny] = P(textU, -bh * 0.12), [sx, sy] = P(textU, bh * 0.2);
    const el = this.text, name = el.firstChild, ability = el.lastChild;
    if (name.textContent !== u.line.forms[u.stage]) { name.textContent = u.line.forms[u.stage]; ability.textContent = u.line.abil[u.stage]; }
    name.style.cssText = `left:${nx}px;top:${ny}px;font-size:${size}px`;
    ability.style.cssText = `left:${sx}px;top:${sy}px;font-size:${size * 0.56}px;color:#${cutMix(element, new THREE.Color('#fff4d8'), 0.55).getHexString()}`;
    el.classList.remove('hidden');
  }

  setLayer(on) {
    if (!this.view || !this.view.visual || this.layered === on) return;
    this.layered = on;
    this.view.visual.traverse((o) => { if (o.isMesh) (on ? o.layers.enable(LAYER) : o.layers.disable(LAYER)); });
  }

  end() {
    this.setLayer(false);
    this.text.classList.add('hidden');
    if (this.shot) { this.shot = false; this.r3d.applyView(); }
    this.r3d.labels.style.opacity = '';
    this.view = null; this.unit = null; this.frameData = null; this.layered = false;
  }
}
