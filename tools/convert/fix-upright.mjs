// Bakes a corrective root rotation into GLB files so every Digimon stands upright and faces +z (the board's front).
// The rotation goes on one wrapper node named UprightFix that holds the scene's top-level nodes, so running this again
// just sets the same rotation (no stacking). Animations never target UprightFix.
//
// Rules, from upright.json next to this file ({ "<file>.glb": { "rot": [x, y, z] degrees, "note": ... } }):
//   - an explicit "rot" entry wins;
//   - otherwise Cyber Sleuth rips (rip_*.glb with an "<chr>_armature" node turned -90 degrees on X by the Blender FBX export, on top of a
//     Y-up skeleton) get +90 on X, which stands them back up.
// tests/upright.cjs checks the result; its reviewed exceptions also live in upright.json ("ok": [flags]).
//
// Run from this folder: node fix-upright.mjs [--dir ../../models/src] [--rips] [file.glb ...]
// convert-all.cjs runs it on every rip it converts. Then node optimize.mjs <same files> for models/files.
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';
import { NodeIO } from '@gltf-transform/core';
import { ALL_EXTENSIONS } from '@gltf-transform/extensions';

const HERE = path.dirname(fileURLToPath(import.meta.url));
// --rips: every file in --dir is one of our rips (models/src-legends uses plain ids, not rip_ names)
const ALL_RIPS = process.argv.includes('--rips');
const TABLE = JSON.parse(fs.readFileSync(path.join(HERE, 'upright.json'), 'utf8'));

function quat([x, y, z]) {
  // XYZ euler (degrees) -> quaternion, same order as three.js Euler 'XYZ'
  const [a, b, c] = [x, y, z].map((d) => (d * Math.PI) / 360);
  const [c1, c2, c3, s1, s2, s3] = [Math.cos(a), Math.cos(b), Math.cos(c), Math.sin(a), Math.sin(b), Math.sin(c)];
  return [s1 * c2 * c3 + c1 * s2 * s3, c1 * s2 * c3 - s1 * c2 * s3, c1 * c2 * s3 + s1 * s2 * c3, c1 * c2 * c3 - s1 * s2 * s3];
}

// rotation this file needs, or null
export function correctionFor(doc, file) {
  const e = TABLE[path.basename(file)];
  if (e && e.rot) return e.rot;
  // only our own rips: some Sketchfab uploads have the same armature node with the up-axis already compensated
  const csArmature = (/^rip_/.test(path.basename(file)) || ALL_RIPS) && doc.getRoot().listNodes().some((n) => {
    const r = n.getRotation();
    return /_armature$/.test(n.getName()) && Math.abs(r[0] + Math.SQRT1_2) < 0.01 && Math.abs(r[3] - Math.SQRT1_2) < 0.01;
  });
  return csArmature ? [90, 0, 0] : null;
}

// returns the applied rotation (degrees) or null
export function applyUpright(doc, file) {
  const rot = correctionFor(doc, file);
  const root = doc.getRoot();
  let fix = root.listNodes().find((n) => n.getName() === 'UprightFix');
  if (!rot && !fix) return null;
  for (const scene of root.listScenes()) {
    if (!fix) fix = doc.createNode('UprightFix');
    for (const child of scene.listChildren()) {
      if (child === fix) continue;
      scene.removeChild(child);
      fix.addChild(child);
    }
    if (!scene.listChildren().includes(fix)) scene.addChild(fix);
  }
  fix.setRotation(rot ? quat(rot) : [0, 0, 0, 1]);
  return rot || [0, 0, 0];
}

if (process.argv[1] && fileURLToPath(import.meta.url) === path.resolve(process.argv[1])) {
  const args = process.argv.slice(2);
  const di = args.indexOf('--dir');
  const dir = di >= 0 ? path.resolve(args[di + 1]) : path.join(HERE, '..', '..', 'models', 'src');
  const only = args.filter((a, i) => a.endsWith('.glb') && (di < 0 || i !== di + 1));
  const io = new NodeIO().registerExtensions(ALL_EXTENSIONS);
  const list = only.length ? only : fs.readdirSync(dir).filter((f) => f.endsWith('.glb'));
  let n = 0;
  for (const f of list) {
    const p = path.join(dir, path.basename(f));
    const doc = await io.read(p);
    const had = doc.getRoot().listNodes().some((x) => x.getName() === 'UprightFix');
    if (!correctionFor(doc, p) && !had) continue;
    const rot = applyUpright(doc, p);
    await io.write(p, doc);
    n++;
    console.log(`${path.basename(f).padEnd(44)} rot ${rot.join(',')}`);
  }
  console.log(`${n} files set`);
}
