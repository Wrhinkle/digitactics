// Reviewed original rigid pieces from DW1 rips. The virtual joints only
// generate tracks on existing nodes: no mesh, material, skin or hierarchy edits.
// Frigimon additionally seats its staged original pieces with exact translations.
import * as THREE from 'three';

const part = (name, parent, pivot, kind, side = 0, segment = 0) => ({ name, parent, pivot, kind, side, segment });
const mesh = (n, parent, pivot, kind, side = 0, segment = 0) => part(`Mesh_${n}`, parent === null ? null : `Mesh_${parent}`, pivot, kind, side, segment);
const ag = (name, parent, pivot, kind, side = 0) => part(`Agumon_${name}_${name}`, parent && `Agumon_${parent}_${parent}`, pivot, kind, side);
const br = (name, parent, pivot, kind, side = 0, segment = 0) => part(`Brachiomon_${name}`, parent && `Brachiomon_${parent}`, pivot, kind, side, segment);
const fr = (n, parent, pivot, kind, side = 0) => part(`TMD_object_#${n},_VRAM_page_#5`, parent === null ? null : `TMD_object_#${parent},_VRAM_page_#5`, pivot, kind, side);
const rigs = {
  'rip-dw1_frigimon': { family: 'frigimon', durations: { idle: 3, run: .9, attack: .75, special: 1.3, win: 2.4 },
    // Authored repair, not recovered game transforms. Each measured joint lies
    // inside both adjacent original cap volumes. Vertex/UV/material data stays
    // byte-identical; only staged piece translations change before animation.
    restOffsets: [[0,0,0],[0,-.48,0],[.65,-.10,.1],[1.15,-.08,.1],[1.75,-.05,.1],[-.45,-.15,.1],[-.95,-.13,.1],[-1.5,.05,.1],[0,.5,.1],[0,1,.1],[0,.5,.1],[0,1,.1]],
    triangles: [88,130,20,20,46,20,20,46,36,50,36,50],
    parts: [
      fr(0,null,[.55133,0,.1],'body'), fr(1,0,[.55133,1.64987,-.02058],'head'),
      fr(2,0,[-.83494,1.09074,-.13188],'arm',-1), fr(3,2,[-1.92171,1.04073,-.08063],'elbow',-1), fr(4,3,[-2.86642,1.04820,-.10063],'hand',-1),
      fr(5,0,[1.93637,1.08013,-.13188],'arm',1), fr(6,5,[3.03803,1.08013,-.08063],'elbow',1), fr(7,6,[3.98347,1.10176,-.10063],'hand',1),
      fr(8,0,[1.64061,-1.47254,-.14836],'leg',1), fr(9,8,[1.64061,-2.85404,-.14836],'foot',1),
      fr(10,0,[-.52010,-1.47254,-.14836],'leg',-1), fr(11,10,[-.59010,-2.83478,-.14836],'foot',-1),
    ] },
  'rip-dw1_agumon': { family: 'agumon', parts: [
    ag('Torso', null, [0, .86, 0], 'body'), ag('Head', 'Torso', [0, 1.80, .08], 'head'),
    ag('Left_Arm', 'Torso', [.27, 1.55, .04], 'arm', 1), ag('Right_Arm', 'Torso', [-.27, 1.55, .07], 'arm', -1),
    ag('Left_Leg', 'Torso', [.36, .85, 0], 'leg', 1), ag('Right_Leg', 'Torso', [-.33, .85, 0], 'leg', -1),
  ] },
  'rip-dw1_airdramon': { family: 'airdramon', parts: [
    mesh(2, null, [0, 69, -7], 'body'), mesh(0, 2, [0, 70, 0], 'head'), mesh(1, 0, [0, 64, -7], 'jaw'),
    mesh(3, 2, [-1.9, 69.6, -14.6], 'tail', 0, 0), mesh(4, 3, [-6.1, 65.5, -26.7], 'tail', 0, 1),
    mesh(5, 4, [-8.3, 55.5, -36.5], 'tail', 0, 2), mesh(6, 5, [-5.1, 43.9, -39.8], 'tail', 0, 3),
    mesh(7, 6, [2.3, 34.1, -37], 'tail', 0, 4), mesh(8, 7, [6.7, 25.7, -35.3], 'tail', 0, 5),
    mesh(9, 8, [4.5, 19.3, -40], 'tail', 0, 6),
    mesh(13, 2, [-.9, 76.3, -17.5], 'wing', 1, 0), mesh(14, 13, [16.5, 91.4, -17.7], 'wing', 1, 1),
    mesh(15, 14, [31.5, 98.3, -14], 'wing', 1, 2),
    mesh(10, 2, [-3.7, 76.3, -16.2], 'wing', -1, 0), mesh(11, 10, [-21.2, 91, -12.6], 'wing', -1, 1),
    mesh(12, 11, [-34.7, 98, -6], 'wing', -1, 2),
  ] },
  'rip-dw1_akatorimon': { family: 'akatorimon', parts: [
    mesh(0, null, [0, 60, 0], 'body'), mesh(1, 0, [0, 78, 22], 'neck'),
    mesh(2, 1, [0, 118, 24], 'head'), mesh(3, 2, [0, 115, 35], 'jaw'), mesh(10, 0, [0, 63, -24], 'tail'),
    mesh(7, 0, [22, 78, 12], 'wing', 1, 0), mesh(8, 7, [29, 89, 30], 'wing', 1, 1), mesh(9, 8, [32, 75, 10], 'wing', 1, 2),
    mesh(4, 0, [-21.6, 78, 12], 'wing', -1, 0), mesh(5, 4, [-28.6, 89, 30], 'wing', -1, 1), mesh(6, 5, [-31.6, 75, 10], 'wing', -1, 2),
    mesh(15, 0, [12.7, 47, 0], 'leg', 1, 0), mesh(16, 15, [12, 32, -11], 'leg', 1, 1),
    mesh(17, 16, [17, 12, -8], 'leg', 1, 2), mesh(18, 17, [22, 5, 9], 'leg', 1, 3),
    mesh(11, 0, [-12.3, 47, 0], 'leg', -1, 0), mesh(12, 11, [-11.6, 32, -11], 'leg', -1, 1),
    mesh(13, 12, [-16.6, 12, -8], 'leg', -1, 2), mesh(14, 13, [-21.6, 5, 9], 'leg', -1, 3),
  ] },
  'rip-dw1_brachiomon': { family: 'brachiomon', durations: { idle: 3.2, run: 1.2, attack: .9, special: 1.6, win: 2.8 }, parts: [
    br('Torso', null, [0, 0, 0], 'body'), br('NeckBase', 'Torso', [0, 2.65, 2.5], 'neck', 0, 0),
    br('Neck1', 'NeckBase', [0, 7.35, 2.3], 'neck', 0, 1), br('Neck2', 'Neck1', [0, 9.78, 2.2], 'neck', 0, 2),
    br('Neck3', 'Neck2', [0, 12.41, 2.25], 'neck', 0, 3), br('NeckTop', 'Neck3', [0, 14.68, 2.3], 'neck', 0, 4),
    br('Head', 'NeckTop', [0, 17.7, 2.8], 'head'), br('Jaw', 'Head', [0, 18.42, 2.5], 'jaw'),
    br('FrontLeftUpper', 'Torso', [-2.7, .2, 3.0], 'leg', -1, 0), br('FrontLeftFoot', 'FrontLeftUpper', [-2.9, -3.5, 2.9], 'foot', -1, 0),
    br('FrontRightUpper', 'Torso', [2.9, .2, 3.2], 'leg', 1, 0), br('FrontRightFoot', 'FrontRightUpper', [2.9, -3.5, 2.9], 'foot', 1, 0),
    br('BackLeftUpper', 'Torso', [-2.4, -.8, -3.6], 'leg', -1, 1), br('BackLeftFoot', 'BackLeftUpper', [-2.6, -3.9, -3.5], 'foot', -1, 1),
    br('BackRightUpper', 'Torso', [2.5, -.8, -3.6], 'leg', 1, 1), br('BackRightFoot', 'BackRightUpper', [2.6, -3.9, -3.5], 'foot', 1, 1),
    br('TailBase', 'Torso', [0, -.9, -5.2], 'tail', 0, 0), br('Tail1', 'TailBase', [0, -.91, -8.773], 'tail', 0, 1),
    br('TailTip', 'Tail1', [0, -.91, -11.913], 'tail', 0, 2),
  ] },
};

export const hasRigidPartMotion = src => Object.hasOwn(rigs, src?.uid);
// Exposed for geometry diagnostics; callers receive copies, never mutable policy.
export const rigidPartRig = uid => rigs[uid] && JSON.parse(JSON.stringify(rigs[uid]));
const durations = { idle: 2.8, run: .72, attack: .65, special: 1.2, win: 2.2 };
const pulse = (u, start, peak, end) => {
  const k = u < peak ? (u - start) / (peak - start) : (end - u) / (end - peak);
  const s = Math.max(0, Math.min(1, k)); return s * s * (3 - 2 * s);
};

function angles(family, p, role, u) {
  const cycle = u * Math.PI * 2, s = Math.sin(cycle), c = Math.cos(cycle), side = p.side;
  const hit = pulse(u, .15, .43, .80), wind = pulse(u, 0, .18, .42), big = pulse(u, .12, .52, .90);
  let x = 0, y = 0, z = 0;
  if (family === 'agumon') {
    if (p.kind === 'arm') z = -side * .65;
    if (role === 'idle') {
      if (p.kind === 'body') x = .012 * s;
      if (p.kind === 'head') { x = .035 * s; y = .025 * s; }
      if (p.kind === 'arm') z += side * .025 * s;
    } else if (role === 'run') {
      if (p.kind === 'leg') x = side * .38 * s;
      if (p.kind === 'arm') { x = -side * .30 * s; z += side * .055 * (1 - c); }
      if (p.kind === 'head') x = -.035 * (1 - c);
    } else if (role === 'attack') {
      if (p.kind === 'body') { x = .14 * hit - .06 * wind; y = -.13 * hit; }
      if (p.kind === 'head') x = -.11 * hit;
      if (p.kind === 'arm' && side === -1) { x = -.95 * hit + .25 * wind; z += side * .12 * hit; }
      if (p.kind === 'arm' && side === 1) x = .16 * hit;
    } else if (role === 'special') {
      if (p.kind === 'body') x = .12 * big - .08 * wind;
      if (p.kind === 'head') x = -.20 * big;
      if (p.kind === 'arm') { x = -1.05 * big + .18 * wind; z += side * .11 * big; }
    } else if (role === 'win') {
      if (p.kind === 'body') y = .12 * s;
      if (p.kind === 'head') { x = .10 * Math.sin(cycle * 2); z = .07 * s; }
      if (p.kind === 'arm') { x = -.43 * (1 - c) / 2 + side * .15 * s; z += side * .09 * (1 - c); }
      if (p.kind === 'leg') x = side * .10 * s;
    }
  } else if (family === 'airdramon') {
    const flap = role === 'idle' ? .10 * s : role === 'run' ? .38 * s : role === 'attack' ? -.24 * wind + .36 * hit : role === 'special' ? .46 * Math.sin(cycle * 2) * big : .30 * s;
    if (p.kind === 'wing') {
      // The complete wing moves at its root. Smaller distal flexes retain the
      // original three-panel membrane rather than pulling each panel apart.
      z = side * flap * (p.segment === 0 ? 1 : p.segment === 1 ? .24 : .16);
    }
    if (p.kind === 'tail') {
      const phase = p.segment * .48, wave = Math.sin(cycle - phase) - Math.sin(-phase);
      y = wave * (role === 'run' ? .055 : role === 'idle' ? .022 : .035);
      x = (Math.sin(cycle - phase * .5) - Math.sin(-phase * .5)) * .016;
    }
    if (p.kind === 'head') x = role === 'attack' ? .22 * hit - .09 * wind : role === 'special' ? -.18 * big : .035 * s;
    if (p.kind === 'jaw') x = role === 'attack' ? .40 * hit : role === 'special' ? .48 * big : role === 'win' ? .18 * (1 - c) : 0;
    if (p.kind === 'body') x = role === 'attack' ? .09 * hit : role === 'special' ? -.06 * big : .012 * s;
  } else if (family === 'frigimon') {
    if (p.kind === 'arm') z = -side * .65;
    if (p.kind === 'elbow') y = -side * .20;
    if (role === 'idle') {
      if (p.kind === 'head') { y = .045 * s; x = .025 * s; }
      if (p.kind === 'arm') z += side * .025 * s;
      if (p.kind === 'elbow') y -= side * .025 * s;
    } else if (role === 'run') {
      if (p.kind === 'leg') x = side * .30 * s;
      if (p.kind === 'foot') x = -side * .22 * s + .10 * Math.max(0, side * s) ** 2;
      if (p.kind === 'arm') x = -side * .25 * s;
      if (p.kind === 'head') x = .035 * (1 - c);
    } else if (role === 'attack') {
      if (p.kind === 'body') { x = .08 * hit - .035 * wind; y = .10 * hit; }
      if (p.kind === 'head') x = -.06 * hit;
      if (p.kind === 'arm' && side === -1) { x = -.95 * hit + .18 * wind; z += side * .12 * hit; }
      if (p.kind === 'elbow' && side === -1) y -= side * .35 * hit;
      if (p.kind === 'arm' && side === 1) x = .12 * hit;
    } else if (role === 'special') {
      if (p.kind === 'body') x = -.055 * wind + .09 * big;
      if (p.kind === 'head') x = -.12 * big;
      if (p.kind === 'arm') { x = -.85 * big; z += side * .20 * big; }
      if (p.kind === 'elbow') y -= side * .22 * big;
    } else if (role === 'win') {
      if (p.kind === 'body') y = .10 * s;
      if (p.kind === 'head') { x = .065 * Math.sin(2 * cycle); z = .05 * s; }
      if (p.kind === 'arm') { x = -.85 * (1 - c) / 2 + side * .13 * s; z += side * .10 * (1 - c); }
      if (p.kind === 'elbow') y -= side * .15 * (1 - c);
      if (p.kind === 'leg') x = side * .08 * s;
      if (p.kind === 'foot') x = -side * .065 * s;
    }
  } else if (family === 'brachiomon') {
    const gait = side * (p.segment === 0 ? 1 : -1) * s;
    if (p.kind === 'tail') y = (role === 'run' ? .065 : role === 'win' ? .095 : .035) * (Math.sin(cycle - p.segment * .45) - Math.sin(-p.segment * .45));
    if (role === 'idle') {
      if (p.kind === 'neck') x = .009 * s;
      if (p.kind === 'head') { x = .025 * s; y = .035 * s; }
    } else if (role === 'run' || role === 'win') {
      const strength = role === 'run' ? 1 : .40;
      if (p.kind === 'leg') x = -.25 * gait * strength;
      if (p.kind === 'foot') x = .20 * gait * strength + .10 * Math.max(0, gait) ** 2 * strength;
      if (p.kind === 'neck') { x = .018 * s; y = role === 'win' ? .035 * s : .008 * s; }
      if (p.kind === 'head') { x = -.025 * s; z = role === 'win' ? .08 * s : .025 * s; }
      if (p.kind === 'jaw' && role === 'win') x = .12 * (1 - c);
    } else if (role === 'attack') {
      if (p.kind === 'neck') x = (p.segment ? .065 : .035) * hit - .018 * wind;
      if (p.kind === 'head') x = .18 * hit - .07 * wind;
      if (p.kind === 'jaw') x = .26 * hit;
    } else if (role === 'special') {
      if (p.kind === 'neck') x = -.035 * big;
      if (p.kind === 'head') x = -.20 * big;
      if (p.kind === 'jaw') x = .45 * big;
      if (p.kind === 'leg') x = (p.segment ? -.055 : .055) * big;
      if (p.kind === 'foot') x = (p.segment ? .055 : -.055) * big;
    }
  } else {
    if (role === 'idle') {
      if (p.kind === 'head') { x = .035 * s; z = .025 * s; }
      if (p.kind === 'tail') z = .018 * s;
      if (p.kind === 'wing') z = side * .015 * s / (p.segment + 1);
    } else if (role === 'run' || role === 'win') {
      const strength = role === 'run' ? 1 : .46, gait = side * s;
      if (p.kind === 'leg') {
        const hip = -.38 * gait * strength, knee = .45 * Math.max(0, gait) ** 2 * strength;
        x = p.segment === 0 ? hip : p.segment === 1 ? knee : p.segment === 2 ? -hip - knee : 0;
      }
      if (p.kind === 'wing') z = side * (role === 'run' ? .12 : .28) * s / (p.segment + 1);
      if (p.kind === 'neck') x = -.045 * s;
      if (p.kind === 'head') z = (role === 'run' ? .025 : .10) * s;
      if (p.kind === 'tail') z = (role === 'run' ? .04 : .09) * s;
    } else if (role === 'attack') {
      if (p.kind === 'body') x = .08 * hit;
      if (p.kind === 'neck') x = .19 * hit - .09 * wind;
      if (p.kind === 'head') x = .48 * hit - .10 * wind;
      if (p.kind === 'jaw') x = .22 * hit;
      if (p.kind === 'wing') z = side * .23 * hit / (p.segment + 1);
      if (p.kind === 'tail') x = -.07 * hit;
    } else if (role === 'special') {
      if (p.kind === 'neck') x = -.16 * big;
      if (p.kind === 'head') x = -.28 * big;
      if (p.kind === 'jaw') x = .40 * big;
      if (p.kind === 'wing') z = side * .48 * big / (p.segment + 1);
      if (p.kind === 'tail') x = -.08 * big;
    }
  }
  return new THREE.Quaternion().setFromEuler(new THREE.Euler(x, y, z, 'XYZ'));
}

export function rigidPartClips(gltf, src) {
  const spec = rigs[src?.uid];
  if (gltf.animations.length) return null;
  if (!spec) return autoRigidClips(gltf, src);
  const scene = gltf.scene, prepared = new Map();
  if (spec.restOffsets && scene.userData.rigidAssemblyPrepared !== src.uid) {
    // Validate the whole exact source before mutating any node. Repeated
    // preparation of this scene must not accumulate staging corrections.
    const nodes = spec.parts.map(p => scene.getObjectByName(p.name));
    if (nodes.some((node, i) => !node?.isMesh || node.parent !== scene || node.isSkinnedMesh || node.geometry.index.count !== spec.triangles[i] * 3))
      throw new Error(`DW1 staged assembly changed: ${src.uid}`);
    nodes.forEach((node, i) => node.position.add(new THREE.Vector3(...spec.restOffsets[i])));
    scene.userData.rigidAssemblyPrepared = src.uid;
  }
  scene.updateMatrixWorld(true);
  for (const p of spec.parts) {
    const node = scene.getObjectByName(p.name);
    if (!node || node.parent !== scene || node.isSkinnedMesh) throw new Error(`DW1 rigid-part source changed: ${src.uid}/${p.name}`);
    const vertices = []; node.traverse(o => {
      if (!o.isMesh) return;
      for (let i = 0; i < o.geometry.attributes.position.count; i++) vertices.push(o.getVertexPosition(i, new THREE.Vector3()).applyMatrix4(o.matrixWorld));
    });
    prepared.set(p.name, { ...p, node, pivot: new THREE.Vector3(...p.pivot), restPosition: node.position.clone(), restRotation: node.quaternion.clone(), vertices });
  }
  const all = [...prepared.values()], floor = Math.min(...all.flatMap(p => p.vertices.map(v => v.y)));
  const height = Math.max(...all.flatMap(p => p.vertices.map(v => v.y))) - floor;
  return Object.entries(spec.durations || durations).map(([role, duration]) => {
    // Dense original-node tracks bound positional interpolation at virtual
    // joints. Runtime/native players use their existing linear+slerp sampler.
    const count = Math.ceil(duration * 120), times = [], positions = all.map(() => []), rotations = all.map(() => []);
    for (let frame = 0; frame <= count; frame++) {
      const u = frame / count, delta = new Map(), poses = [];
      times.push(u * duration);
      for (const p of all) {
        const rotation = angles(spec.family, p, role, u), local = new THREE.Matrix4().makeRotationFromQuaternion(rotation);
        const shift = p.pivot.clone().sub(p.pivot.clone().applyQuaternion(rotation)); local.setPosition(shift);
        const world = p.parent ? delta.get(p.parent).clone().multiply(local) : local;
        delta.set(p.name, world);
        const q = new THREE.Quaternion().setFromRotationMatrix(world).multiply(p.restRotation);
        poses.push({ position: p.restPosition.clone().applyMatrix4(world), rotation: q, world });
      }
      let minimum = Infinity;
      all.forEach((p, i) => { for (const v of p.vertices) minimum = Math.min(minimum, v.clone().applyMatrix4(poses[i].world).y); });
      const bounce = spec.family === 'agumon' && role === 'win' ? height * .025 * Math.sin(u * Math.PI * 2) ** 2 : 0;
      const lift = Math.max(0, floor - minimum) + bounce;
      poses.forEach((pose, i) => {
        pose.position.y += lift;
        positions[i].push(...pose.position.toArray()); rotations[i].push(...pose.rotation.toArray());
      });
    }
    const tracks = all.flatMap((p, i) => [new THREE.VectorKeyframeTrack(p.name + '.position', times, positions[i]), new THREE.QuaternionKeyframeTrack(p.name + '.quaternion', times, rotations[i])]);
    return new THREE.AnimationClip(role, duration, tracks);
  });
}

// Clipless, unskinned imports without a reviewed map above: the remaining DW1
// assemblies and single-mesh rips. Their original nodes stay the only animated
// objects (no mesh, material, skin or hierarchy edits). A part tree is grown
// from the torso through the nearest original contacts; each joint pivots at
// that contact. Outstretched arms are lowered as a constant clip offset, as in
// the reviewed Agumon/Frigimon maps. One-part rips move as a whole.
export function autoRigidPartRig(gltf) {
  const scene = gltf.scene;
  let skinned = false, meshes = 0;
  scene.traverse(o => { if (o.isSkinnedMesh || o.isBone) skinned = true; if (o.isMesh) meshes++; });
  if (skinned || !meshes) return null;
  scene.updateMatrixWorld(true);
  let container = scene;
  while (container.children.length === 1 && !container.children[0].isMesh && container.children[0].children.length) container = container.children[0];
  const nodes = container.children.filter(o => { let mesh = false; o.traverse(c => { if (c.isMesh) mesh = true; }); return mesh; });
  if (!nodes.length || nodes.some(n => !n.name) || new Set(nodes.map(n => n.name)).size !== nodes.length) return null;
  const parts = nodes.map(node => {
    const vertices = [], triangles = [];
    node.traverse(o => {
      if (!o.isMesh) return;
      const local = [];
      for (let i = 0; i < o.geometry.attributes.position.count; i++) local.push(o.getVertexPosition(i, new THREE.Vector3()).applyMatrix4(o.matrixWorld));
      const ids = o.geometry.index ? o.geometry.index.array : local.map((_, i) => i);
      for (let i = 0; i + 2 < ids.length; i += 3) triangles.push(new THREE.Triangle(local[ids[i]], local[ids[i + 1]], local[ids[i + 2]]));
      vertices.push(...local);
    });
    const step = Math.max(1, Math.floor(vertices.length / 200)), sample = vertices.filter((_, i) => i % step === 0);
    const box = new THREE.Box3().setFromPoints(vertices);
    return { node, name: node.name, vertices, triangles, sample, box, center: box.getCenter(new THREE.Vector3()) };
  });
  const all = new THREE.Box3(); parts.forEach(p => all.union(p.box));
  const size = all.getSize(new THREE.Vector3()), height = Math.max(size.y, 1e-6), floor = all.min.y;
  if (parts.length === 1) {
    const p = parts[0];
    return { family: 'auto', single: true, height, parts: [{ name: p.name, parent: null, pivot: [p.center.x, floor, p.center.z], kind: 'body', side: 0, segment: 0, restZ: 0, offset: [0, 0, 0] }] };
  }
  // Contact between two original parts: surface distance (vertex to triangle),
  // with the centre of the touching region as the joint pivot.
  const near = height * .06, band = height * .015, cache = new Map();
  const surface = (points, part, box) => {
    const triangles = part.triangles.filter(t => box.intersectsTriangle(t)), target = new THREE.Vector3();
    return points.map(v => {
      let best = Infinity, point = null;
      for (const t of triangles) { t.closestPointToPoint(v, target); const d = v.distanceTo(target); if (d < best) { best = d; point = target.clone(); } }
      return { d: best, v, point };
    });
  };
  const contact = (a, b) => {
    const key = a.name < b.name ? a.name + '|' + b.name : b.name + '|' + a.name;
    if (cache.has(key)) return cache.get(key);
    const boxGap = Math.max(0, ...['x', 'y', 'z'].map(k => Math.max(a.box.min[k] - b.box.max[k], b.box.min[k] - a.box.max[k])));
    let result = { gap: boxGap };
    if (boxGap <= near) {
      const region = a.box.clone().expandByScalar(near).intersect(b.box.clone().expandByScalar(near));
      const da = surface(a.sample, b, region), db = surface(b.sample, a, region), gap = Math.min(...da.map(x => x.d), ...db.map(x => x.d));
      const touching = [...da, ...db].filter(x => x.d <= gap + band).map(x => x.v);
      if (Number.isFinite(gap)) {
        // The closest pair, keyed by part, closes staged (exploded) assemblies.
        const ab = da.find(x => x.d === gap), ba = db.find(x => x.d === gap);
        const points = ab ? { [a.name]: ab.v, [b.name]: ab.point } : { [a.name]: ba.point, [b.name]: ba.v };
        result = { gap, points, pivot: touching.reduce((sum, v) => sum.add(v), new THREE.Vector3()).multiplyScalar(1 / touching.length) };
      }
    }
    cache.set(key, result); return result;
  };
  // Minimum spanning tree over contacts, rooted at its vertex-weighted median.
  const neighbours = new Map(parts.map(p => [p, []])), linked = [parts[0]], pending = parts.slice(1);
  while (pending.length) {
    let best = null;
    for (const p of pending) for (const q of linked) { const c = contact(p, q); if (!best || c.gap < best.c.gap - height * 1e-4) best = { p, q, c }; }
    const pivot = best.c.pivot || best.q.box.clampPoint(best.p.center, new THREE.Vector3());
    const link = { pivot, gap: best.c.gap, points: best.c.points };
    neighbours.get(best.p).push({ part: best.q, ...link }); neighbours.get(best.q).push({ part: best.p, ...link });
    linked.push(best.p); pending.splice(pending.indexOf(best.p), 1);
  }
  const cost = root => {
    let total = 0; const seen = new Set([root]), queue = [[root, 0]];
    while (queue.length) { const [p, d] = queue.shift(); total += d * p.vertices.length; for (const n of neighbours.get(p)) if (!seen.has(n.part)) { seen.add(n.part); queue.push([n.part, d + 1]); } }
    return total;
  };
  const torso = parts.reduce((a, b) => cost(b) < cost(a) ? b : a);
  const order = [torso], parentOf = new Map([[torso, null]]);
  for (let i = 0; i < order.length; i++) for (const n of neighbours.get(order[i])) if (!parentOf.has(n.part)) { parentOf.set(n.part, { part: order[i], pivot: n.pivot, gap: n.gap, points: n.points }); order.push(n.part); }
  const subtree = new Map();
  for (const p of [...order].reverse()) { const box = p.box.clone(); for (const q of order) if (parentOf.get(q)?.part === p) box.union(subtree.get(q)); subtree.set(p, box); }
  const tc = torso.center, wide = size.x * .12, total = parts.reduce((n, p) => n + p.vertices.length, 0), info = new Map([[torso, { kind: 'body', side: 0, segment: 0, restZ: 0 }]]);
  for (const p of order.slice(1)) {
    const { part: parent, pivot } = parentOf.get(p), up = info.get(parent), box = subtree.get(p), c = box.getCenter(new THREE.Vector3());
    const own = p.center.x - tc.x, side = Math.abs(own) > size.x * .04 ? Math.sign(own) : up.side;
    if (['head', 'arm', 'leg', 'tail', 'wing'].includes(up.kind) && (up.side === 0 || side === up.side)) { info.set(p, { kind: up.kind, side: up.side || side, segment: up.segment + 1, restZ: 0 }); continue; }
    const central = Math.abs(c.x - tc.x) < wide, spans = box.min.x < tc.x - wide && box.max.x > tc.x + wide;
    // Hips sit in the lower half of the torso (or below it); shoulders above.
    const members = order.filter(q => { for (let r = q; r; r = parentOf.get(r)?.part) if (r === p) return true; return false; });
    const low = pivot.y < torso.box.min.y + (torso.box.max.y - torso.box.min.y) * .5;
    const light = members.reduce((n, m) => n + m.vertices.length, 0) < total * .06;
    let kind = 'link';
    if (central && box.min.y > tc.y - height * .05 && c.y > pivot.y) kind = 'head';
    else if (box.min.y < floor + height * .15 && c.y < pivot.y && !spans && side !== 0 && low) kind = 'leg';
    else if (central && c.z < tc.z && box.max.z < pivot.z + height * .05 && !spans) kind = 'tail';
    else if (!spans && side !== 0 && light) kind = 'wing';
    else if (!spans && side !== 0 && Math.abs(c.x - tc.x) > wide && (!low || box.min.y > floor + height * .25)) kind = 'arm';
    else if (!spans && side !== 0 && box.min.y < floor + height * .15) kind = 'leg';
    let restZ = 0;
    if (kind === 'arm') {
      // Lower outstretched arms toward 50 degrees below horizontal.
      const far = members.flatMap(m => m.sample).reduce((a, b) => b.distanceToSquared(pivot) > a.distanceToSquared(pivot) ? b : a);
      // Long arms on short bodies stop where the hand would reach the floor.
      const d = far.clone().sub(pivot), elevation = Math.atan2(d.y, Math.abs(d.x)), reach = Math.hypot(d.x, d.y);
      for (let target = -.87; Math.abs(d.x) > d.length() * .6 && elevation > -.6 && target < Math.min(-.26, elevation); target += .05)
        if (pivot.y + reach * Math.sin(target) > floor + height * .08) { restZ = -Math.sign(d.x) * (elevation - target); break; }
    }
    info.set(p, { kind, side: ['arm', 'leg', 'wing'].includes(kind) ? side : 0, segment: 0, restZ });
  }
  // Staged DW1 assemblies (MudFrigimon, Monzaemon) leave limb pieces apart.
  // Like Frigimon's reviewed offsets, seat each separated limb piece on its
  // parent's nearest surface; the shift is carried by the clip tracks.
  const offset = new Map([[torso, new THREE.Vector3()]]), pivots = new Map([[torso, new THREE.Vector3(tc.x, floor, tc.z)]]);
  for (const p of order.slice(1)) {
    const { part: parent, pivot, gap, points } = parentOf.get(p), seat = new THREE.Vector3();
    if (gap > height * .015 && points && ['arm', 'leg', 'head'].includes(info.get(p).kind)) seat.subVectors(points[parent.name], points[p.name]);
    offset.set(p, offset.get(parent).clone().add(seat));
    pivots.set(p, pivot.clone().add(offset.get(parent)).addScaledVector(seat, .5));
  }
  return { family: 'auto', height, parts: order.map(p => ({ name: p.name, parent: parentOf.get(p)?.part.name ?? null,
    pivot: pivots.get(p).toArray(), offset: offset.get(p).toArray(), gap: p === torso ? 0 : parentOf.get(p).gap / height, ...info.get(p) })) };
}

function autoAngles(spec, p, role, u) {
  const cycle = u * Math.PI * 2, s = Math.sin(cycle), c = Math.cos(cycle), side = p.side, first = p.segment === 0;
  const hit = pulse(u, .15, .43, .80), wind = pulse(u, 0, .18, .42), big = pulse(u, .12, .52, .90), raise = (1 - c) / 2;
  let x = 0, y = 0, z = p.restZ;
  if (spec.single) {
    if (role === 'idle') { x = .025 * s; z = .03 * Math.sin(cycle + .7); }
    else if (role === 'run') { x = .08 * raise; z = .06 * s; }
    else if (role === 'attack') x = .3 * hit - .1 * wind;
    else if (role === 'special') { const k = Math.min(1, Math.max(0, (u - .12) / .66)); x = -.12 * wind; y = Math.PI * 2 * k * k * (3 - 2 * k); }
    else { z = .14 * s; y = .25 * Math.sin(cycle * 2) * raise; }
  } else if (p.kind === 'wing') {
    // Light lateral panels flap as a pair, distal panels flexing less.
    const flap = role === 'idle' ? .1 * s : role === 'run' ? .35 * s : role === 'attack' ? .3 * hit - .12 * wind : role === 'special' ? .45 * Math.sin(cycle * 2) * big : .3 * s;
    z = side * flap * (first ? 1 : .3);
  } else if (role === 'idle') {
    if (p.kind === 'body') x = .012 * s;
    if (p.kind === 'head') { x = .035 * s; y = .03 * Math.sin(cycle + .5); }
    if (p.kind === 'arm' && first) z += side * .03 * s;
    if (p.kind === 'tail') y = .05 * (Math.sin(cycle - p.segment * .5) - Math.sin(-p.segment * .5));
  } else if (role === 'run') {
    if (p.kind === 'leg') x = first ? side * .35 * s : -side * .12 * s;
    if (p.kind === 'arm' && first) x = -side * .28 * s;
    if (p.kind === 'head') x = -.03 * (1 - c);
    if (p.kind === 'tail') y = .08 * (Math.sin(cycle - p.segment * .5) - Math.sin(-p.segment * .5));
  } else if (role === 'attack') {
    // The -X arm leads; the other guards, so the strike never reads as a T.
    if (p.kind === 'body') { x = .12 * hit - .05 * wind; y = .1 * hit; }
    if (p.kind === 'head') x = -.09 * hit;
    if (p.kind === 'arm' && side < 0) x = first ? -1.0 * hit + .25 * wind : -.35 * hit;
    if (p.kind === 'arm' && side > 0 && first) x = .2 * hit;
    if (p.kind === 'tail') y = .18 * hit;
  } else if (role === 'special') {
    if (p.kind === 'body') x = .1 * big - .08 * wind;
    if (p.kind === 'head') x = -.18 * big;
    if (p.kind === 'arm' && side < 0) x = first ? -1.5 * big : -.3 * big;
    if (p.kind === 'arm' && side > 0 && first) { z += side * .6 * big; x = -.3 * big; }
    if (p.kind === 'tail') x = -.1 * big;
  } else {
    if (p.kind === 'body') y = .12 * s;
    if (p.kind === 'head') { x = .08 * Math.sin(cycle * 2); z = .06 * s; }
    if (p.kind === 'arm' && side < 0 && first) { z += side * 2.2 * raise; x = .15 * s; }
    if (p.kind === 'arm' && side > 0 && first) { z += side * .35 * raise; x = -.2 * raise; }
    if (p.kind === 'leg' && first) x = side * .08 * s;
    if (p.kind === 'tail') y = .12 * (Math.sin(cycle - p.segment * .5) - Math.sin(-p.segment * .5));
  }
  return new THREE.Quaternion().setFromEuler(new THREE.Euler(x, y, z, 'XYZ'));
}

function autoRigidClips(gltf) {
  const spec = autoRigidPartRig(gltf);
  if (!spec) return null;
  const scene = gltf.scene;
  scene.updateMatrixWorld(true);
  const all = spec.parts.map(p => {
    const node = scene.getObjectByName(p.name), vertices = [];
    node.traverse(o => {
      if (!o.isMesh) return;
      for (let i = 0; i < o.geometry.attributes.position.count; i++) vertices.push(o.getVertexPosition(i, new THREE.Vector3()).applyMatrix4(o.matrixWorld));
    });
    // The assembly container may carry an import transform (UprightFix), so
    // world-space joint deltas are converted back into the node's parent space.
    const space = node.parent.matrixWorld.clone();
    return { ...p, node, pivot: new THREE.Vector3(...p.pivot), seat: new THREE.Matrix4().makeTranslation(...p.offset), vertices, space, spaceInverse: space.clone().invert(), local: node.matrix.clone() };
  });
  const floor = Math.min(...all.flatMap(p => p.vertices.map(v => v.y)));
  return Object.entries(durations).map(([role, duration]) => {
    const count = Math.ceil(duration * 120), times = [], positions = all.map(() => []), rotations = all.map(() => []);
    for (let frame = 0; frame <= count; frame++) {
      const u = frame / count, delta = new Map(), poses = [];
      times.push(u * duration);
      for (const p of all) {
        const rotation = autoAngles(spec, p, role, u), local = new THREE.Matrix4().makeRotationFromQuaternion(rotation);
        local.setPosition(p.pivot.clone().sub(p.pivot.clone().applyQuaternion(rotation)));
        const world = p.parent !== null && delta.has(p.parent) ? delta.get(p.parent).clone().multiply(local) : local;
        delta.set(p.name, world); poses.push(world.clone().multiply(p.seat));
      }
      let minimum = Infinity;
      all.forEach((p, i) => { for (const v of p.vertices) minimum = Math.min(minimum, v.clone().applyMatrix4(poses[i]).y); });
      // Whole-body hops over a grounded lowest point (seated pieces may rise or drop).
      const wave = Math.sin(u * Math.PI * 2), bell = Math.sin(Math.PI * u) ** 2;
      const hop = spec.height * (spec.single
        ? role === 'idle' ? .02 * (1 - Math.cos(u * Math.PI * 4)) / 2 : role === 'special' ? .18 * bell : role === 'win' ? .1 * Math.abs(wave) : role === 'run' ? .06 * Math.abs(wave) : 0
        : role === 'special' ? .05 * bell : role === 'win' ? .03 * wave * wave : 0);
      const lift = new THREE.Matrix4().makeTranslation(0, floor - minimum + hop, 0);
      all.forEach((p, i) => {
        const matrix = p.spaceInverse.clone().multiply(lift.clone().multiply(poses[i])).multiply(p.space).multiply(p.local);
        const position = new THREE.Vector3(), quaternion = new THREE.Quaternion(), scale = new THREE.Vector3();
        matrix.decompose(position, quaternion, scale);
        positions[i].push(...position.toArray()); rotations[i].push(...quaternion.toArray());
      });
    }
    const tracks = all.flatMap((p, i) => [new THREE.VectorKeyframeTrack(p.name + '.position', times, positions[i]), new THREE.QuaternionKeyframeTrack(p.name + '.quaternion', times, rotations[i])]);
    return new THREE.AnimationClip(role, duration, tracks);
  });
}
