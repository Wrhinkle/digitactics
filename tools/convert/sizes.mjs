// Byte breakdown of a GLB: images, animation data, geometry.
import { NodeIO } from '@gltf-transform/core';
import { ALL_EXTENSIONS } from '@gltf-transform/extensions';
import { MeshoptDecoder } from 'meshoptimizer';
await MeshoptDecoder.ready;
const io = new NodeIO().registerExtensions(ALL_EXTENSIONS).registerDependencies({ 'meshopt.decoder': MeshoptDecoder });
for (const f of process.argv.slice(2)) {
  const d = await io.read(f);
  const r = d.getRoot();
  const anim = new Set();
  let frames = 0;
  for (const a of r.listAnimations()) for (const s of a.listSamplers()) { anim.add(s.getInput()); anim.add(s.getOutput()); }
  let ab = 0, gb = 0;
  for (const acc of r.listAccessors()) { const b = acc.getArray().byteLength; if (anim.has(acc)) ab += b; else gb += b; }
  for (const a of r.listAnimations()) frames += a.listSamplers().length;
  const ib = r.listTextures().reduce((s, t) => s + t.getImage().byteLength, 0);
  console.log(f.split(/[\/]/).pop(), 'anims', r.listAnimations().length, 'channels', frames, 'animMB', (ab / 1e6).toFixed(2), 'geoMB', (gb / 1e6).toFixed(2), 'imgMB', (ib / 1e6).toFixed(2), 'nodes', r.listNodes().length);
}
