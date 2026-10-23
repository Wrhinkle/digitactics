// Execute js/cutin-core.js (special cut-in policy, timeline and camera framing) and record its outputs for
// native/tests/DigiTactics.Tests/SpecialCinematicChecks.cs.
const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), crypto = require('node:crypto');
const root = path.resolve(__dirname, '../..'), file = path.join(root, 'js/cutin-core.js');
const source = fs.readFileSync(file, 'utf8');
const context = {};
vm.createContext(context);
vm.runInContext(source.replace(/^export /gm, '') + '\nthis.api={blockedBy,CUT,cutInMode,cutInPace,spring,trackLength,cutInCurve,headScore,pickHead,headEstimate,headFromBone,trackAzimuth,trackPose,headPose,CutInDirector};', context);
const api = context.api;
const plain = (v) => JSON.parse(JSON.stringify(v));

const names = ['J_head', 'J_head_027', 'J_neck_026', 'J_haed', 'J_kao', 'atama', 'Bip01 Head', 'Bip01 HeadNub', 'head_end', 'Head_M', 'HeadTop_End',
  'CharacterFaceCenterTarget_013', 'HUDTarget_014', 'eye_L', 'eye_R', 'J_eyelid_L', 'J_kubiA', 'J_kubiB', 'neck', 'Neck1', 'hair_head',
  'head_mesh', 'TMD object #3, VRAM page #5', 'J_ashi', 'J_mune', 'RootNode', 'J_tail', 'headA', 'Kaonashi', 'J_kuchi', 'eyebrow_L', ''];
const scores = names.map((name) => ({ name, result: plain(api.headScore(name)) }));
const rigs = [
  [['RootNode', 0], ['J_root', 1], ['J_neck_026', 2], ['J_head_027', 3]],
  [['J_root', 0], ['J_kubiA', 1], ['J_kubiB', 2], ['J_chest', 1]],
  [['Geom', 0], ['J_root', 1], ['eye_L', 2], ['eye_R', 2], ['J_kuchi', 2]],
  [['Object_2', 0], ['CharacterFaceCenterTarget_013', 1], ['J_head', 2]],
  [['TMD object #0, VRAM page #5', 0], ['TMD object #1, VRAM page #5', 0]],
  [['neck', 1], ['neck_2', 3], ['neck_1', 2]],
].map((nodes) => nodes.map(([name, depth]) => ({ name, depth })));
const picks = rigs.map((nodes) => ({ nodes, result: plain(api.pickHead(nodes)) }));
const curves = [0, 0.03, 0.06, 0.12, 0.2, 0.3, 0.45, 0.5, 0.55, 0.6, 0.62, 0.7].map((t) => ({ t, ...api.cutInCurve(t) }));
const tracks = [0, 0.3, 0.62, 0.9, 1.1, 2, 5].map((clip) => ({ clip, length: api.trackLength(clip) }));
const boxes = [
  [{ x: -0.5, y: 0, z: -0.4 }, { x: 0.5, y: 1.3, z: 0.4 }, Math.PI],
  [{ x: 1, y: 0.3, z: -2 }, { x: 3.4, y: 1.1, z: -1.2 }, 0.7],
  [{ x: -6, y: 0, z: 3 }, { x: -3.5, y: 2.4, z: 5.6 }, -2.2],
  [{ x: -0.2, y: 0.8, z: 0 }, { x: 0.2, y: 1.2, z: 0.3 }, 0],
];
const poses = [];
for (const [min, max, facing] of boxes) for (const yaw of [0, 0.8, -2]) for (const [aspect, reach] of [[1.6, 13], [0.8, 18]]) {
  const center = { x: (min.x + max.x) / 2, z: (min.z + max.z) / 2 }, blockers = [{ x: center.x + 2 * Math.sin(facing + 1.25), z: center.z + 2 * Math.cos(facing + 1.25) }, { x: center.x - 3, z: center.z + 1 }];
  const az = api.trackAzimuth(facing, yaw, center, blockers);
  const estimate = api.headEstimate(min, max, facing);
  poses.push({ min, max, facing, yaw, aspect, reach, blockers, azimuth: az, plainAzimuth: api.trackAzimuth(facing, yaw), blocked: api.blockedBy(center, az, blockers), track: api.trackPose(min, max, az, aspect, (38 * Math.PI) / 180, reach), estimate,
    left: api.headPose(estimate, estimate.size, facing, true, 0.3), right: api.headPose(estimate, estimate.size, facing, false, 1),
    fromHead: api.headFromBone('head', { x: (min.x + max.x) / 2, y: max.y * 0.8, z: (min.z + max.z) / 2 }, null, min, max, estimate.size),
    fromNeck: api.headFromBone('neck', { x: (min.x + max.x) / 2, y: max.y * 0.7, z: min.z }, { x: (min.x + max.x) / 2, y: max.y * 0.5, z: min.z + 0.1 }, min, max, estimate.size),
    outside: api.headFromBone('face', { x: max.x + 5, y: max.y, z: max.z }, null, min, max, estimate.size) });
}
// Director scenario: offers, frame steps, camera samples and cooldowns.
const director = new api.CutInDirector(), steps = [];
let state = 0x5eed1234;
const rand = () => ((state = (Math.imul(state, 1664525) + 1013904223) >>> 0) / 4294967296);
const box = (t) => [{ x: Math.sin(t) - 0.6, y: 0.2 * Math.abs(Math.sin(t * 3)), z: -1 }, { x: Math.sin(t) + 0.6, y: 1.5 + 0.3 * Math.sin(t * 2), z: 0.2 }];
for (let i = 0; i < 900; i++) {
  const dt = [1 / 30, 1 / 60, 0.1, 0.016, 0.05][i % 5], speed = i > 700 && i < 760 ? 4 : i > 400 && i < 500 ? 2 : 1;
  const offers = [];
  if (rand() < 0.08) {
    const unit = Math.floor(rand() * 6), mine = rand() < 0.5, mode = ['all', 'mine', 'off', 'bogus'][Math.floor(rand() * 4)], allowed = rand() < 0.9, clip = rand() * 2.5;
    offers.push({ unit, mine, mode, allowed, clip, speed, claimed: director.offer(unit, mine, mode, allowed, clip, speed) });
  }
  if (i === 333 && director.unit != null) director.abort();
  director.update(dt, speed);
  const [min, max] = box(i * dt);
  const shot = director.frame(min, max, 0.4 + i * 0.001, 0.2, 1.7, (38 * Math.PI) / 180, 13, [{ x: 1.5, y: 0, z: 0.3 }, { x: -0.4, y: 0, z: -2 }]);
  const head = director.headFrame({ x: 0, y: 1.2, z: 0 }, 0.4, 0.4, i % 2 === 0);
  steps.push({ dt, speed, offers, unit: director.unit, weight: director.weight, cutIn: plain(director.cutIn), shot: plain(shot), head: plain(head) });
}
const result = { sourceSha256: crypto.createHash('sha256').update(source).digest('hex'), cut: plain(api.CUT), modes: ['all', 'mine', 'off', 'x', null].map((m) => ({ value: m, mode: api.cutInMode(m) })),
  paces: [1, 2, 3, 4, 8].map((s) => ({ speed: s, pace: api.cutInPace(s) })), scores, picks, curves, tracks, poses, steps };
fs.writeFileSync(path.join(root, 'native/tests/fixtures/special-cinematic.json'), JSON.stringify(result) + '\n');
console.log(`Special cinematic: ${scores.length} names, ${picks.length} rigs, ${poses.length} framing cases, ${steps.length} director steps.`);
