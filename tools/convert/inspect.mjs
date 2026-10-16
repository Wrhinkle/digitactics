import { NodeIO } from '@gltf-transform/core';
const io = new NodeIO();
for (const f of process.argv.slice(2)) {
  const doc = await io.read(f);
  for (const m of doc.getRoot().listMeshes()) for (const p of m.listPrimitives()) {
    const uv = p.getAttribute('TEXCOORD_0'), col = p.getAttribute('COLOR_0');
    const r = (a) => { if (!a) return '-'; const lo = [], hi = []; const el = []; for (let i = 0; i < a.getCount(); i++) { a.getElement(i, el); el.forEach((v, k) => { lo[k] = Math.min(lo[k] ?? 1e9, v); hi[k] = Math.max(hi[k] ?? -1e9, v); }); } return lo.map((v, k) => `${v.toFixed(2)}..${hi[k].toFixed(2)}`).join(' '); };
    console.log(f.split(/[\/]/).pop(), 'uv', r(uv), 'color', r(col));
  }
}
