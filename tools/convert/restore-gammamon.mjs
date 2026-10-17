// Recover the original Gammamon FBX rig (the older importer selected its static OBJ).
// Input is the FBX2glTF output from the credited original archive; output is a preview GLB.
import { NodeIO } from '@gltf-transform/core';
import * as T from '../../vendor/three/build/three.module.js';
const io = new NodeIO(), doc = await io.read(process.argv[2]), root = doc.getRoot(), buffer = root.listBuffers()[0];
if (root.listSkins().length !== 3) throw Error('Expected the original body and two eyelid skins');
// The rip parks the closed eyelid meshes over the textured open eyes. Keep the original open eyes visible.
for (const n of root.listNodes()) if (/^Eyelid_[LR]$/.test(n.getName())) n.setMesh(null).setSkin(null);
const targets = root.listNodes().filter(n => /^Bip001 (?:Head|Spine1|[LR] UpperArm|[LR] Forearm|Tail[123]?)$/.test(n.getName()));
for (const [role, duration] of [['idle', 3], ['attack', .7], ['special', 1.4], ['win', 2.4]]) {
  const anim = doc.createAnimation(role), times = Array.from({ length: 33 }, (_, i) => duration * i / 32);
  const time = doc.createAccessor().setType('SCALAR').setArray(new Float32Array(times)).setBuffer(buffer);
  for (const node of targets) {
    const name = node.getName(), rest = new T.Quaternion().fromArray(node.getRotation());
    const world = new T.Quaternion(); new T.Matrix4().fromArray(node.getWorldMatrix()).decompose(new T.Vector3(), world, new T.Vector3());
    const parent = world.clone().multiply(rest.clone().invert()), inv = parent.clone().invert();
    const values = times.flatMap((t, i) => {
      const u = i / 32, wave = Math.sin(u * Math.PI * 2), pulse = Math.sin(u * Math.PI) ** 2;
      let x = 0, y = 0, z = 0;
      if (/Head$/.test(name)) { y = .045 * wave; x = role === 'idle' ? .02 * wave : .12 * pulse; }
      if (/Spine1$/.test(name)) x = .015 * wave;
      if (/UpperArm$/.test(name)) { x = role === 'idle' ? .025 * wave : -.65 * pulse; z = role === 'win' ? (name.includes(' L ') ? 1 : -1) * .65 * pulse : 0; }
      if (/Forearm$/.test(name)) x = role === 'idle' ? 0 : -.15 * pulse;
      if (/Tail/.test(name)) y = (role === 'idle' ? .04 : .08) * wave;
      const delta = new T.Quaternion().setFromEuler(new T.Euler(x, y, z));
      return inv.clone().multiply(delta).multiply(world).normalize().toArray();
    });
    const output = doc.createAccessor().setType('VEC4').setArray(new Float32Array(values)).setBuffer(buffer);
    const sampler = doc.createAnimationSampler().setInput(time).setOutput(output).setInterpolation('LINEAR');
    anim.addSampler(sampler).addChannel(doc.createAnimationChannel().setTargetNode(node).setTargetPath('rotation').setSampler(sampler));
  }
}
await io.write(process.argv[3], doc);
console.log('Restored original Gammamon skeleton, open eyes and four project-authored motion clips');
