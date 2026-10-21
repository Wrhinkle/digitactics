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
  if (!spec || gltf.animations.length) return null;
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
