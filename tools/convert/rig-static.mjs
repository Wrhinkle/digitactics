// Add project-authored skeletons and four gameplay clips to the legacy static roster.
// Inputs are immutable original GLBs; output can be previewed before replacing shipped assets.
// node tools/convert/rig-static.mjs <original-dir> <output-dir>
import fs from 'fs';
import path from 'path';
import { NodeIO } from '@gltf-transform/core';
import { ALL_EXTENSIONS } from '@gltf-transform/extensions';
import { MeshoptDecoder } from '../../vendor/three/addons/libs/meshopt_decoder.module.js';
import * as T from '../../vendor/three/build/three.module.js';

const input = path.resolve(process.argv[2]), output = path.resolve(process.argv[3]);
const presets = JSON.parse(fs.readFileSync(process.argv[4] || new URL('./static-rigs.json', import.meta.url)));
await MeshoptDecoder.ready;
const io = new NodeIO().registerExtensions(ALL_EXTENSIONS).registerDependencies({ 'meshopt.decoder': MeshoptDecoder });
fs.mkdirSync(output, { recursive: true });
const v = a => new T.Vector3(...a);
const distance = (p, a, b) => new T.Line3(v(a), v(b)).closestPointToPoint(p, true, new T.Vector3()).distanceTo(p);
function components(points, indices) {
  const parent = points.map((_, i) => i), find = i => parent[i] === i ? i : (parent[i] = find(parent[i]));
  const join = (a, b) => { parent[find(a)] = find(b); }, same = new Map();
  points.forEach((p, i) => { const k = p.toArray().map(x => Math.round(x * 1e5)).join(','); if (same.has(k)) join(i, same.get(k)); else same.set(k, i); });
  for (let i = 0; i < indices.length; i += 3) { join(indices[i], indices[i + 1]); join(indices[i], indices[i + 2]); }
  const groups = new Map(); points.forEach((p, i) => { const id = find(i); if (!groups.has(id)) groups.set(id, []); groups.get(id).push(i); });
  const result = [];
  for (const ids of groups.values()) { const box = new T.Box3().setFromPoints(ids.map(i => points[i])), center = box.getCenter(new T.Vector3()); for (const i of ids) result[i] = { box, center, count: ids.length }; }
  return result;
}

for (const [file, spec] of Object.entries(presets)) {
  const doc = await io.read(path.join(input, file)), root = doc.getRoot(), buffer = root.listBuffers()[0];
  if (root.listSkins().length || root.listAnimations().length) throw Error(`${file}: refusing to replace an existing rig`);
  const originals = root.listNodes().filter(n => n.getMesh()), box = new T.Box3();
  const parts = originals.map(n => {
    const world = new T.Matrix4().fromArray(n.getWorldMatrix()), normal = new T.Matrix3().getNormalMatrix(world);
    const prims = n.getMesh().listPrimitives().map(p => {
      const pos = p.getAttribute('POSITION'), points = Array.from({ length: pos.getCount() }, (_, i) => v(pos.getElement(i, [])).applyMatrix4(world));
      for (const point of points) box.expandByPoint(point);
      const normals = p.getAttribute('NORMAL');
      return { p, points, normals: normals && Array.from({ length: normals.getCount() }, (_, i) => v(normals.getElement(i, [])).applyMatrix3(normal).normalize()) };
    });
    return { name: n.getName(), mesh: n.getMesh(), prims };
  });
  const height = box.max.y - box.min.y;
  for (const part of parts) for (const prim of part.prims) for (const p of prim.points) { p.y -= box.min.y; p.divideScalar(height); }
  for (const n of root.listNodes()) n.dispose();
  const scene = root.listScenes()[0], rig = doc.createNode('DT_Rig'); scene.addChild(rig);
  const bones = [];
  const add = (name, at, end, parent, role, radius = .1) => {
    const node = doc.createNode(name), up = bones.find(b => b.name === parent);
    node.setTranslation(v(at).sub(up ? v(up.at) : new T.Vector3()).toArray());
    (up?.node || rig).addChild(node);
    const b = { name, at, end, node, parent, role, radius }; bones.push(b); return b;
  };
  add('root', [0, 0, 0], [0, .03, 0], null, 'root');
  for (const b of spec.bones) add(...b);
  const skin = doc.createSkin('DT_Skeleton').setSkeleton(bones[0].node);
  const inverse = [];
  for (const b of bones) { skin.addJoint(b.node); inverse.push(...new T.Matrix4().makeTranslation(...b.at).invert().elements); }
  skin.setInverseBindMatrices(doc.createAccessor().setType('MAT4').setArray(new Float32Array(inverse)).setBuffer(buffer));
  let vertices = 0;
  for (const part of parts) {
    const attachment = spec.attachments?.find(a => new RegExp(a.part, 'i').test(part.name));
    if (attachment) for (const prim of part.prims) for (const p of prim.points) {
      p.sub(v(attachment.grip)).applyEuler(new T.Euler(...attachment.rotation)).add(v(bones.find(b => b.name === attachment.bone).at));
    }
    const forced = attachment?.bone || spec.parts?.find(a => new RegExp(a.part, 'i').test(part.name))?.bone;
    for (const { p, points, normals } of part.prims) {
      const weights = [], joints = [];
      const islands = components(points, p.getIndices()?.getArray() || points.map((_, i) => i));
      for (const [vertex, point] of points.entries()) {
        const island = islands[vertex];
        const componentRule = spec.componentRules?.find(r => island.count >= (r.minCount || 0) && island.box.min.toArray().every((n, i) => n >= r.min[i] - .003) && island.box.max.toArray().every((n, i) => n <= r.max[i] + .003));
        let candidates = bones.filter(b => b.role !== 'root' && (!spec.defaultBones || spec.defaultBones.includes(b.name)));
        const sample = island.count <= 64 ? island.center : point;
        const region = spec.regions?.find(r => {
          const q = [r.absX ? Math.abs(sample.x) : sample.x, sample.y, sample.z];
          return q.every((n, i) => n >= r.min[i] && n <= r.max[i]);
        });
        if (region) candidates = candidates.filter(b => region.bones.includes(b.name));
        if (componentRule) candidates = bones.filter(b => componentRule.bones.includes(b.name));
        const finalRegion = spec.finalRegions?.find(r => point.toArray().every((n,i) => n >= r.min[i] && n <= r.max[i]));
        if (finalRegion) candidates = bones.filter(b => finalRegion.bones.includes(b.name));
        if (forced) candidates = bones.filter(b => b.name === forced);
        const ranked = candidates.map(b => ({ b, d: distance(sample, b.at, b.end) / b.radius })).sort((a, b) => a.d - b.d);
        if (!ranked.length) throw Error(`${file}: empty weight region`);
        const first = ranked[0];
        // Blend only adjacent joints. Opposite limbs and nearby wings must not pull each other's skin.
        const second = ranked.find(r => r !== first && (r.b.parent === first.b.name || first.b.parent === r.b.name));
        let w = second ? 1 / (1 + ((first.d + .05) / (second.d + .05)) ** 6) : 1;
        if (w > .98) w = 1;
        joints.push(bones.indexOf(first.b), second ? bones.indexOf(second.b) : 0, 0, 0);
        weights.push(w, 1 - w, 0, 0);
      }
      p.setAttribute('POSITION', doc.createAccessor().setType('VEC3').setArray(new Float32Array(points.flatMap(p => p.toArray()))).setBuffer(buffer));
      if (normals) p.setAttribute('NORMAL', doc.createAccessor().setType('VEC3').setArray(new Float32Array(normals.flatMap(n => attachment ? n.clone().applyEuler(new T.Euler(...attachment.rotation)).toArray() : n.toArray()))).setBuffer(buffer));
      p.setAttribute('JOINTS_0', doc.createAccessor().setType('VEC4').setArray(new Uint16Array(joints)).setBuffer(buffer));
      p.setAttribute('WEIGHTS_0', doc.createAccessor().setType('VEC4').setArray(new Float32Array(weights)).setBuffer(buffer));
      vertices += points.length;
    }
    rig.addChild(doc.createNode(part.name).setMesh(part.mesh).setSkin(skin));
  }
  for (const [role, duration] of [['idle', 3], ['attack', .7], ['special', 1.4], ['win', 2.4]]) {
    const anim = doc.createAnimation(role), times = Array.from({ length: 33 }, (_, i) => duration * i / 32);
    const time = doc.createAccessor().setType('SCALAR').setArray(new Float32Array(times)).setBuffer(buffer);
    for (const b of bones.filter(b => b.role !== 'root' && b.role !== 'fixed')) {
      const values = times.flatMap((t, i) => {
        const u = i / 32, wave = Math.sin(u * Math.PI * 2), pulse = Math.sin(u * Math.PI) ** 2;
        const side = Math.sign(b.at[0]) || 1;
        let x = 0, y = 0, z = 0;
        if (b.role === 'head') { y = (role === 'idle' ? .025 : role === 'win' ? .12 : .02) * wave; x = (role === 'attack' ? .16 : role === 'special' ? -.18 : .015) * pulse; }
        if (b.role === 'body') { x = (role === 'idle' ? .012 : .045) * wave; y = role === 'win' ? .05 * wave : 0; }
        if (b.role === 'arm') { x = role === 'idle' ? .015 * wave : -(role === 'special' ? .85 : .5) * pulse; z = role === 'win' ? side * .65 * pulse : 0; }
        if (b.role === 'forearm') x = role === 'idle' ? 0 : -.18 * pulse;
        if (b.role === 'rifle') x = (role === 'idle' ? .01 * wave : -(role === 'win' ? .5 : .06) * pulse);
        if (b.role === 'wing') { z = side * (role === 'idle' ? .045 : role === 'special' ? .35 : .2) * wave; }
        if (b.role === 'tail') y = (role === 'idle' ? .045 : .13) * Math.sin(u * Math.PI * 2 + bones.indexOf(b) * .4) * (role === 'idle' || role === 'win' ? 1 : pulse);
        if (b.role === 'segment') { x = (role === 'idle' ? .035 : role === 'attack' ? .14 : .09) * wave; z = role === 'win' ? .09 * wave : 0; }
        if (b.role === 'foreleg') x = role === 'idle' ? 0 : -.16 * pulse;
        if (b.role === 'turret') { y = (role === 'idle' ? .025 : .12) * wave; x = role === 'attack' ? -.035 * pulse : 0; }
        if (b.role === 'gun') x = (role === 'idle' ? .005 : -.055) * pulse;
        const rest = spec.restEuler?.[b.name] || [0,0,0];
        return new T.Quaternion().setFromEuler(new T.Euler(x+rest[0], y+rest[1], z+rest[2])).toArray();
      });
      const output = doc.createAccessor().setType('VEC4').setArray(new Float32Array(values)).setBuffer(buffer);
      const sampler = doc.createAnimationSampler().setInput(time).setOutput(output).setInterpolation('LINEAR');
      anim.addSampler(sampler).addChannel(doc.createAnimationChannel().setTargetNode(b.node).setTargetPath('rotation').setSampler(sampler));
    }
  }
  // Quantization/compression metadata describes the old accessors. Re-optimization is a separate final step.
  for (const ext of root.listExtensionsUsed()) if (['EXT_meshopt_compression', 'KHR_mesh_quantization'].includes(ext.extensionName)) ext.dispose();
  rig.setExtras({ digitacticsRig: 1, source: 'project-authored', body: spec.type });
  await io.write(path.join(output, file), doc);
  console.log(`${file}: ${bones.length} bones, ${vertices} weighted vertices, idle/attack/special/win`);
}
