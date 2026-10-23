// Special cut-in cinematic: who gets one, its timeline, and the camera framing. Presentation only: it reads
// combat state and never changes the fight, RNG or lockstep. js/cutin.js draws it; the native client runs a
// port (native/src/DigiTactics.Presentation/SpecialCinematic.cs), checked against
// native/tests/fixtures/special-cinematic.json (native/tools/export-special-cinematic.cjs).
//
// Phase A (cut-in, CUT.cutIn s): a slanted panel slides across the arena with the caster's head in a live
// side view, its name and the special's name. Phase B (tracking): the arena camera eases onto a closer
// three-quarter view that frames the caster's animated bounds every frame, for the special clip plus a
// follow-through, then eases back to the player's own camera. One cinematic at a time; a global and a
// per-unit cooldown keep frequent casters from repeating it. 2x playback runs the timeline 1.35x faster;
// 4x skips cinematics (the fight moves too fast for the board to be covered).

export const CUT = {
  cutIn: 0.62, slideIn: 0.12, slideOut: 0.12, dim: 0.35,
  cameraLead: 0.22, follow: 0.9, minTrack: 1.2, maxTrack: 2.5, returnLimit: 1.6,
  globalCooldown: 6, unitCooldown: 18,
  weightOmega: 6.5, frameOmega: 9,
  specialMax: 1.1, trackSide: 1.25, trackPitch: 0.34, headSide: Math.PI / 2 - 0.5, headPitch: 0.1, headFov: 30 * Math.PI / 180,
};
export const CUT_IN_MODES = ['all', 'mine', 'off'];
export const cutInMode = (value) => (CUT_IN_MODES.includes(value) ? value : 'mine');
// Playback speed to timeline rate; 0 skips the cinematic.
export const cutInPace = (speed) => (speed >= 4 ? 0 : speed >= 2 ? 1.35 : 1);

const cutClamp = (v, lo, hi) => Math.min(hi, Math.max(lo, v));
const cutEaseOut = (t) => 1 - (1 - t) ** 3;
const cutEaseIn = (t) => t ** 3;

// Exact critically damped step toward a fixed target over dt.
export function spring(x, v, target, omega, dt) {
  const y = x - target, j = v + omega * y, e = Math.exp(-omega * dt);
  return [target + (y + j * dt) * e, (v - omega * j * dt) * e];
}

// A special clip plays at most CUT.specialMax seconds (UnitView.play); the shot covers what is left after the
// cut-in plus a follow-through for its hits and effects.
export const trackLength = (clipSeconds) => cutClamp(Math.min(clipSeconds || 0, CUT.specialMax) - CUT.cutIn + CUT.follow, CUT.minTrack, CUT.maxTrack);

// Panel motion at cut-in time t: offset -1 (off-screen before) .. 0 (in place) .. 1 (off-screen after).
export function cutInCurve(t) {
  const p = cutEaseOut(cutClamp(t / CUT.slideIn, 0, 1));
  const q = t > CUT.cutIn - CUT.slideOut ? cutEaseIn(cutClamp((t - (CUT.cutIn - CUT.slideOut)) / CUT.slideOut, 0, 1)) : 0;
  const push = cutClamp((t - CUT.slideIn) / (CUT.cutIn - CUT.slideIn - CUT.slideOut), 0, 1);
  return { offset: p - 1 + q, push, dim: CUT.dim * cutClamp(t / 0.1, 0, 1) * (1 - q), time: t };
}

// Bone names that mark the head, best first. kind: face/eye anchors sit on the face, head joints sit at the
// base of the skull, neck joints are extended one segment.
export function headScore(name) {
  const n = String(name || '').toLowerCase();
  if (/mesh|geo|material|vram|tmd /.test(n)) return null;
  if (/face.?center/.test(n)) return { kind: 'face', score: 100 };
  const skip = /end|top|nub|tip|hair|target|hud|look|null/.test(n);
  if (!skip && /(^|[^a-z])(head|haed)([^a-z]|$)/.test(n)) return { kind: 'head', score: 90 };
  if (!skip && /head|haed/.test(n)) return { kind: 'head', score: 80 };
  if (!skip && /(^|[^a-z])(kao|atama)([^a-z]|$)/.test(n)) return { kind: 'head', score: 70 };
  if (/(^|[^a-z])eye/.test(n) && !/brow|lid|lash|light|target/.test(n)) return { kind: 'eye', score: 60 };
  if (!skip && /neck|kubi/.test(n)) return { kind: 'neck', score: 50 };
  return null;
}
// nodes: [{ name, depth }]. Returns { kind, indices } (eyes are averaged; necks prefer the deepest joint) or null.
export function pickHead(nodes) {
  let best = null;
  nodes.forEach((node, i) => {
    const s = headScore(node.name);
    if (!s) return;
    if (!best || s.score > best.score || (s.kind === 'neck' && s.score === best.score && node.depth > best.depth)) best = { ...s, depth: node.depth, index: i };
  });
  if (!best) return null;
  if (best.kind === 'eye') return { kind: 'eye', indices: nodes.map((n, i) => i).filter((i) => headScore(nodes[i].name)?.kind === 'eye') };
  return { kind: best.kind, indices: [best.index] };
}

// Fallback head (no usable bone): the top quarter of the animated bounds, toward the facing direction.
export function headEstimate(min, max, facing) {
  const ex = max.x - min.x, ey = max.y - min.y, ez = max.z - min.z;
  const fx = Math.sin(facing), fz = Math.cos(facing);
  const along = 0.5 * (Math.abs(fx) * ex + Math.abs(fz) * ez);
  const size = cutClamp(0.2 * Math.max(ey, Math.max(ex, ez) * 0.5), 0.18, 2);
  return { x: (min.x + max.x) / 2 + fx * along * 0.55, y: max.y - 0.6 * size, z: (min.z + max.z) / 2 + fz * along * 0.55, size };
}
// A bone point adjusted to the middle of the head, or null when it lies outside the body (unused helper nodes).
export function headFromBone(kind, point, parent, min, max, size) {
  let p = point;
  if (kind === 'head') p = { x: point.x, y: point.y + 0.35 * size, z: point.z };
  else if (kind === 'neck') {
    const d = parent ? { x: point.x - parent.x, y: point.y - parent.y, z: point.z - parent.z } : { x: 0, y: 1, z: 0 };
    const l = Math.hypot(d.x, d.y, d.z) || 1;
    p = { x: point.x + (d.x / l) * 0.8 * size, y: point.y + (d.y / l) * 0.8 * size, z: point.z + (d.z / l) * 0.8 * size };
  }
  const pad = (a, b) => 0.25 * (b - a);
  const inside = (v, a, b) => v >= a - pad(a, b) && v <= b + pad(a, b);
  if (!inside(p.x, min.x, max.x) || !inside(p.y, min.y, max.y) || !inside(p.z, min.z, max.z) || p.y < min.y + 0.2 * (max.y - min.y)) return null;
  return p;
}

// Units standing between the caster (center) and a camera on this azimuth, within the shot's near range.
export function blockedBy(center, azimuth, blockers) {
  const dx = Math.sin(azimuth), dz = Math.cos(azimuth);
  let n = 0;
  for (const b of blockers || []) {
    const x = b.x - center.x, z = b.z - center.z, t = x * dx + z * dz;
    if (t > 0.4 && t < 7 && Math.abs(x * dz - z * dx) < 1.1) n++;
  }
  return n;
}
// Tracking azimuth: mostly a side view, on the side nearer the player's camera, unless other units would
// stand in front of the caster on that side.
export function trackAzimuth(facing, cameraYaw, center, blockers) {
  const a = facing + CUT.trackSide, b = facing - CUT.trackSide;
  const score = (az) => Math.cos(az - cameraYaw) - 2 * (center ? blockedBy(center, az, blockers) : 0);
  return score(a) >= score(b) ? a : b;
}
// Camera that keeps the whole animated bounds in frame; it rises instead of leaving the island (reach).
export function trackPose(min, max, azimuth, aspect, fovY, reach) {
  const c = { x: (min.x + max.x) / 2, y: (min.y + max.y) / 2, z: (min.z + max.z) / 2 };
  const r = Math.max(0.5 * Math.hypot(max.x - min.x, max.y - min.y, max.z - min.z), 0.55);
  const half = Math.min(fovY / 2, Math.atan(Math.tan(fovY / 2) * aspect));
  const d = cutClamp((r * 1.3) / Math.sin(half), 3.2, 16);
  let pitch = CUT.trackPitch, eye;
  for (let i = 0; ; i++) {
    eye = { x: c.x + d * Math.cos(pitch) * Math.sin(azimuth), y: c.y + d * Math.sin(pitch), z: c.z + d * Math.cos(pitch) * Math.cos(azimuth) };
    if (Math.hypot(eye.x, eye.z) <= reach || pitch >= 1.1 || i >= 8) break;
    pitch += 0.1;
  }
  eye.y = Math.max(eye.y, 1.2);
  return { eye, target: c };
}
// Side view of the head; faceLeft puts the face toward the left of the panel. push 0..1 is the slow push-in.
export function headPose(head, size, facing, faceLeft, push) {
  const az = facing + (faceLeft ? CUT.headSide : -CUT.headSide);
  const fx = Math.sin(facing), fz = Math.cos(facing);
  const target = { x: head.x + fx * size * 0.12, y: head.y - 0.1 * size, z: head.z + fz * size * 0.12 };
  const d = ((1.2 * size) / Math.tan(CUT.headFov / 2)) * (1 - 0.12 * push);
  const eye = { x: target.x + d * Math.cos(CUT.headPitch) * Math.sin(az), y: target.y + d * Math.sin(CUT.headPitch), z: target.z + d * Math.cos(CUT.headPitch) * Math.cos(az) };
  return { eye, target };
}

const cutSpring3 = (s, goal, omega, dt) => {
  for (const k of ['x', 'y', 'z']) [s.p[k], s.v[k]] = spring(s.p[k], s.v[k], goal[k], omega, dt);
};
const cutState3 = (p) => ({ p: { ...p }, v: { x: 0, y: 0, z: 0 } });

// One director per shown fight. Wall-clock seconds in, camera blend and cut-in curve out.
export class CutInDirector {
  constructor() { this.clock = 0; this.active = null; this.lastEnd = -1e9; this.lastStart = new Map(); }

  // A special cast. mine: the local player's unit. allowed: false while hidden/minimised or reduced motion.
  offer(unit, mine, mode, allowed, clipSeconds, speed) {
    mode = cutInMode(mode);
    if (!allowed || mode === 'off' || (mode === 'mine' && !mine)) return false;
    const pace = cutInPace(speed);
    if (!pace || this.clock - (this.lastStart.get(unit) ?? -1e9) < CUT.unitCooldown) return false;
    const a = this.active;
    if (a && !(a.t === 0 && !a.mine && mine)) return false;
    if (!a && this.clock - this.lastEnd < CUT.globalCooldown) return false;
    this.active = { unit, mine, pace, t: 0, step: 0, track: trackLength(clipSeconds), weight: 0, velocity: 0, returning: false, aborted: false, returnT: 0, az: null, cam: null };
    this.lastStart.set(unit, this.clock);
    return true;
  }

  update(dt, speed) {
    this.clock += dt;
    const a = this.active;
    if (!a) return;
    if (!cutInPace(speed)) this.abort();
    a.step = dt * a.pace;
    a.t += a.step;
    if (!a.returning && a.t >= CUT.cutIn + a.track) a.returning = true;
    if (a.returning) a.returnT += a.step;
    const goal = !a.returning && a.t >= CUT.cameraLead ? 1 : 0;
    [a.weight, a.velocity] = spring(a.weight, a.velocity, goal, CUT.weightOmega, a.step);
    if (a.returning && ((a.weight < 0.004 && a.returnT > 0.2) || a.returnT >= CUT.returnLimit)) { this.active = null; this.lastEnd = this.clock; }
  }

  // The caster died, left the view or playback went to 4x: hide the panel and ease back.
  abort() { if (this.active) { this.active.returning = true; this.active.aborted = true; } }
  get unit() { return this.active ? this.active.unit : null; }
  get weight() { return this.active ? this.active.weight : 0; }
  get cutIn() { const a = this.active; return a && !a.aborted && a.t < CUT.cutIn ? cutInCurve(a.t) : null; }

  // Tracking camera for this frame from the caster's animated bounds; null when no cinematic runs.
  frame(min, max, facing, cameraYaw, aspect, fovY, reach, blockers) {
    const a = this.active;
    if (!a) return null;
    if (a.az == null) a.az = trackAzimuth(facing, cameraYaw, { x: (min.x + max.x) / 2, z: (min.z + max.z) / 2 }, blockers);
    const pose = trackPose(min, max, a.az, aspect, fovY, reach);
    if (!a.cam) a.cam = { eye: cutState3(pose.eye), target: cutState3(pose.target) };
    else { cutSpring3(a.cam.eye, pose.eye, CUT.frameOmega, a.step); cutSpring3(a.cam.target, pose.target, CUT.frameOmega, a.step); }
    return { eye: { ...a.cam.eye.p }, target: { ...a.cam.target.p }, weight: a.weight };
  }
  // Head camera for the cut-in panel this frame. It follows the head bone exactly: a leap or lunge in the
  // special would leave a smoothed close-up behind.
  headFrame(head, size, facing, faceLeft) {
    const curve = this.cutIn;
    return curve ? headPose(head, size, facing, faceLeft, curve.push) : null;
  }
}
