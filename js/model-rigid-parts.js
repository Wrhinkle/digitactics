// Reviewed original rigid pieces from three DW1 rips. The virtual joints only
// generate tracks on existing nodes: no mesh, material, skin or hierarchy edits.
import * as THREE from 'three';

const part = (name, parent, pivot, kind, side = 0, segment = 0) => ({ name, parent, pivot, kind, side, segment });
const mesh = (n, parent, pivot, kind, side = 0, segment = 0) => part(`Mesh_${n}`, parent === null ? null : `Mesh_${parent}`, pivot, kind, side, segment);
const ag = (name, parent, pivot, kind, side = 0) => part(`Agumon_${name}_${name}`, parent && `Agumon_${parent}_${parent}`, pivot, kind, side);
const rigs = {
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
  return Object.entries(durations).map(([role, duration]) => {
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
