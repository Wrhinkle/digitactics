// Skeletal fallback for rigged imports that ship without usable animation clips.
// Authored clips always take priority. These motions preserve the original mesh and skin.
import * as THREE from 'three';

const V = () => new THREE.Vector3();
const Q = () => new THREE.Quaternion();
const X = new THREE.Vector3(1, 0, 0), Y = new THREE.Vector3(0, 1, 0), Z = new THREE.Vector3(0, 0, 1);
const armName = /(?:^|_)(?:arm\d*|ude|shoulder)(?:_|$)|upperarm/i;
const headName = /(?:^|_)(?:head|atama)(?:_|$)/i;
const chestName = /(?:^|_)(?:chest|mune|spine02)(?:_|$)/i;
const tailName = /(?:^|_)(?:tail|sippo|shippo)(?:0?1)?(?:_|$)/i;
const valid = b => !/end|nub|twist|helper/i.test(b.name);

export function modelClips(gltf, src) {
  const authored = gltf.animations || [];
  if (authored.length && src?.anim !== false) return authored;
  // Disabled Noesis reels contain unrelated actions and root travel. Their first frame
  // still provides the assembled rest pose, including armor and weapon transforms.
  if (authored.length) {
    const pose = new THREE.AnimationMixer(gltf.scene);
    pose.clipAction(authored[0]).play();
    pose.update(0);
  }
  return fallbackClips(gltf.scene);
}

export function fallbackClips(scene) {
  const bones = new Set();
  scene.traverse(o => { if (o.isSkinnedMesh) for (const b of o.skeleton.bones) bones.add(b); });
  if (!bones.size) return [];
  scene.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(scene), height = box.max.y - box.min.y;
  const hasHead = [...bones].some(b => headName.test(b.name));
  const isHead = b => headName.test(b.name) || (!hasHead && /(?:^|_)(?:neck|kubi)(?:_|$)/i.test(b.name));
  const isChest = b => chestName.test(b.name) || /(?:^|_)body$/i.test(b.name);
  // Some imports leave a held weapon under the root instead of the hand. Preserve its rest
  // transform while parenting nearby grips to the hand, so posing an arm also carries its weapon.
  const hands = [...bones].filter(b => /(?:^|_)(?:hand\d*|te|wrist)(?:_|$)/i.test(b.name) && valid(b));
  const feet = [...bones].filter(b => /(?:^|_)(?:foot|ankle|kakato)(?:_|$)/i.test(b.name));
  const footY = feet.length ? Math.min(...feet.map(b => b.getWorldPosition(V()).y)) : box.min.y;
  for (const b of bones) {
    if (!/bamboo_sword|(?:^|_)weapon(?:0?[12])?$/i.test(b.name)) continue;
    let parent = b.parent, held = false;
    while (parent) { if (hands.includes(parent)) held = true; parent = parent.parent; }
    if (held) continue;
    const pos = b.getWorldPosition(V());
    // Unanimated imports sometimes park a weapon below the feet until a clip puts it in a hand.
    const parked = pos.y < footY - height * .1;
    if (parked) {
      const side = /02$/.test(b.name) ? 'l' : 'r';
      const grip = hands.find(h => new RegExp(`_${side}(?:_|$)`, 'i').test(h.name));
      if (grip) { grip.attach(b); b.position.set(0, 0, 0); scene.updateMatrixWorld(true); continue; }
    }
    const knownGrip = /bamboo_sword/i.test(b.name) && hands.find(h => h.name === 'J_hand_r');
    const hand = knownGrip || hands.slice().sort((a, c) => a.getWorldPosition(V()).distanceTo(pos) - c.getWorldPosition(V()).distanceTo(pos))[0];
    if (hand && (knownGrip || hand.getWorldPosition(V()).distanceTo(pos) < height * .35)) {
      hand.attach(b);
      if (knownGrip) b.position.set(0, 0, 0);
    }
  }
  const arms = [];
  for (const b of bones) {
    if (!valid(b) || !(armName.test(b.name) || /^J_(?:wing|hand01)_[lr]$/i.test(b.name))) continue;
    const child = b.children.find(c => c.isBone && /elbow|hiji|ancon|forearm/i.test(c.name)) || b.children.find(c => c.isBone);
    if (!child) continue;
    const origin = b.getWorldPosition(V()), direction = child.getWorldPosition(V()).sub(origin).normalize();
    // Only lower outstretched upper arms. Forelegs and already-posed arms keep their orientation.
    if (armName.test(b.name) && Math.abs(direction.x) > .8 && Math.abs(direction.y) < .3 && origin.y > box.min.y + height * .25) {
      const target = new THREE.Vector3(Math.sign(direction.x) * .45, -.82, .3).normalize();
      const delta = Q().setFromUnitVectors(direction, target);
      const parent = b.parent.getWorldQuaternion(Q());
      b.quaternion.copy(parent.clone().invert().multiply(delta).multiply(b.getWorldQuaternion(Q())));
      scene.updateMatrixWorld(true);
    }
    arms.push({ bone: b, side: origin.x >= box.getCenter(V()).x ? 1 : -1 });
  }
  const selected = [...bones].filter(b => valid(b) && (arms.some(a => a.bone === b) || isHead(b) || isChest(b) || tailName.test(b.name)));
  if (!selected.length) return [];
  scene.updateMatrixWorld(true);
  // Box3 otherwise reuses the bounds it cached before the arms were posed.
  scene.traverse(o => { if (o.isSkinnedMesh) o.computeBoundingBox(); });
  const rest = selected.map(b => ({
    bone: b, world: b.getWorldQuaternion(Q()), parent: b.parent.getWorldQuaternion(Q()).invert(),
    arm: arms.find(a => a.bone === b), head: isHead(b), chest: isChest(b), tail: tailName.test(b.name),
  }));
  const make = (role, duration) => {
    const count = 25, times = Array.from({ length: count }, (_, i) => duration * i / (count - 1));
    const tracks = rest.map(r => {
      const values = [];
      for (let i = 0; i < count; i++) {
        const u = i / (count - 1), wave = Math.sin(u * Math.PI * 2);
        // One-shots ease back into their neutral pose; victory and idle loop seamlessly.
        const pulse = Math.sin(Math.PI * u) ** 2;
        const delta = Q();
        const turn = (axis, angle) => delta.multiply(Q().setFromAxisAngle(axis, angle));
        if (role === 'idle') {
          if (r.arm) turn(Z, r.arm.side * .035 * wave);
          if (r.head) turn(X, .025 * wave);
          if (r.chest) turn(X, .018 * wave);
          if (r.tail) turn(Y, .08 * wave);
        } else if (role === 'attack' || role === 'special') {
          if (r.arm) turn(X, -(role === 'special' ? 1.25 : .8) * pulse);
          if (r.head) turn(X, .12 * pulse);
          if (r.chest) turn(Y, (role === 'special' ? .15 : .1) * Math.sin(u * Math.PI * 2) * pulse);
          if (r.tail) turn(Y, .25 * wave * pulse);
        } else {
          if (r.arm) { turn(Z, r.arm.side * 1.6 * pulse); turn(X, .2 * wave); }
          if (r.head) turn(Y, .12 * wave);
          if (r.chest) turn(Z, .055 * wave);
          if (r.tail) turn(Y, .22 * wave);
        }
        const q = r.parent.clone().multiply(delta).multiply(r.world).normalize();
        values.push(q.x, q.y, q.z, q.w);
      }
      return new THREE.QuaternionKeyframeTrack(`${r.bone.name}.quaternion`, times, values);
    });
    return new THREE.AnimationClip(role, duration, tracks);
  };
  return [make('idle', 2.4), make('attack', .65), make('special', 1.2), make('win', 2)];
}
