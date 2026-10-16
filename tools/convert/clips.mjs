// Lists each GLB's animation clips (name, duration), triangle count and texture sizes.
// Run from this folder: node clips.mjs <file.glb ...> [--json]
import path from 'path';
import { NodeIO } from '@gltf-transform/core';
import { ALL_EXTENSIONS } from '@gltf-transform/extensions';
import { MeshoptDecoder } from 'meshoptimizer';
import sharp from 'sharp';

await MeshoptDecoder.ready;
const io = new NodeIO().registerExtensions(ALL_EXTENSIONS).registerDependencies({ 'meshopt.decoder': MeshoptDecoder });
const json = process.argv.includes('--json');
const out = {};
for (const f of process.argv.slice(2).filter((a) => !a.startsWith('--'))) {
  const doc = await io.read(f);
  const root = doc.getRoot();
  const clips = root.listAnimations().map((a) => {
    let d = 0;
    for (const s of a.listSamplers()) { const i = s.getInput(); if (i) d = Math.max(d, i.getMax([])[0] || 0); }
    return { name: a.getName(), dur: +d.toFixed(2) };
  });
  let tris = 0;
  for (const m of root.listMeshes()) for (const p of m.listPrimitives()) {
    const idx = p.getIndices();
    const n = idx ? idx.getCount() : p.getAttribute('POSITION').getCount();
    if (p.getMode() === 4) tris += n / 3;
  }
  const tex = [];
  for (const t of root.listTextures()) {
    try { const m = await sharp(Buffer.from(t.getImage())).metadata(); tex.push(`${m.width}x${m.height}`); } catch { tex.push('?'); }
  }
  out[path.basename(f)] = { clips, tris: Math.round(tris), textures: tex };
  if (!json) console.log(`${path.basename(f).padEnd(36)} tris ${String(Math.round(tris)).padStart(6)} tex ${tex.join(',')}  clips(${clips.length}) ${clips.map((c) => c.name).join(' ')}`);
}
if (json) console.log(JSON.stringify(out, null, 1));
