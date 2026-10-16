import { NodeIO } from '@gltf-transform/core';
const io = new NodeIO();
for (const f of process.argv.slice(2)) {
  const doc = await io.read(f);
  for (const t of doc.getRoot().listTextures()) { const im = t.getImage(); console.log(f.split(/[\/]/).pop(), t.getName(), t.getMimeType(), im ? im.length : 0, im ? Buffer.from(im.slice(0, 8)).toString('hex') : ''); }
}
