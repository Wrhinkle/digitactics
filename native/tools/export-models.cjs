// Build-time meshopt decoding. The native runtime reads standard embedded GLB data.
const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm');
const { pathToFileURL } = require('node:url');
const { createHash } = require('node:crypto');
const { gzipSync } = require('node:zlib');
const { load } = require('../../tools/headless.cjs');
const root = path.resolve(__dirname, '../..');
const output = path.join(root, 'native/artifacts/model-content/models');
const hash = b => createHash('sha256').update(b).digest('hex');
function readGlb(b) {
  if (b.readUInt32LE(0) !== 0x46546c67 || b.readUInt32LE(4) !== 2 || b.readUInt32LE(8) !== b.length) throw new Error('Invalid GLB');
  let json, bin;
  for (let offset = 12; offset < b.length;) {
    const size = b.readUInt32LE(offset), type = b.readUInt32LE(offset + 4);
    const chunk = b.subarray(offset + 8, offset + 8 + size);
    if (type === 0x4e4f534a) json = JSON.parse(chunk.toString());
    if (type === 0x004e4942) bin = chunk;
    offset += 8 + size;
  }
  if (!json || !bin) throw new Error('Missing GLB chunk');
  return {json, bin};
}
function writeGlb(json, bin) {
  const data = Buffer.from(JSON.stringify(json)); const j = Buffer.alloc((data.length + 3) & ~3, 32); data.copy(j);
  const b = Buffer.alloc((bin.length + 3) & ~3); bin.copy(b);
  const out = Buffer.alloc(28 + j.length + b.length);
  out.writeUInt32LE(0x46546c67); out.writeUInt32LE(2, 4); out.writeUInt32LE(out.length, 8);
  out.writeUInt32LE(j.length, 12); out.writeUInt32LE(0x4e4f534a, 16); j.copy(out, 20);
  out.writeUInt32LE(b.length, 20 + j.length); out.writeUInt32LE(0x004e4942, 24 + j.length); b.copy(out, 28 + j.length);
  return out;
}
async function main() {
  const {MeshoptDecoder} = await import(pathToFileURL(path.join(root, 'vendor/three/addons/libs/meshopt_decoder.module.js')));
  await MeshoptDecoder.ready;
  const {BufferAttribute} = await import(pathToFileURL(path.join(root, 'vendor/three/build/three.module.js')));
  const probes = [];
  const componentArrays = {5120: Int8Array, 5121: Uint8Array, 5122: Int16Array, 5123: Uint16Array, 5125: Uint32Array, 5126: Float32Array};
  const dimensions = {SCALAR: 1, VEC2: 2, VEC3: 3, VEC4: 4, MAT4: 16};
  const DT = load({strict: true}); vm.runInNewContext(fs.readFileSync(path.join(root, 'js/models.js'), 'utf8'), {DT});
  const index = JSON.parse(fs.readFileSync(path.join(root, 'models/index.json')));
  const legends = JSON.parse(fs.readFileSync(path.join(root, 'native/src/DigiTactics.Storage/cosmetics.json'))).legend;
  for (const item of legends) if (item.data?.file) index['legend:' + item.id] = item.data.file;
  fs.mkdirSync(output, {recursive: true});
  const assets = {}, sources = {}, variants = {};
  for (const file of [...new Set(Object.values(index))].sort()) {
    if (!file.startsWith('models/') || file.includes('..')) throw new Error(`Invalid model path: ${file}`);
    const original = fs.readFileSync(path.join(root, file)); const {json, bin} = readGlb(original);
    if (json.buffers.some(b => b.uri)) throw new Error(`External buffer: ${file}`);
    const chunks = []; let size = 0;
    for (const view of json.bufferViews || []) {
      const ext = view.extensions?.EXT_meshopt_compression; let decoded;
      if (ext) {
        if (ext.buffer !== 0) throw new Error(`External compressed buffer: ${file}`);
        decoded = Buffer.alloc(ext.count * ext.byteStride);
        MeshoptDecoder.decodeGltfBuffer(decoded, ext.count, ext.byteStride,
          bin.subarray(ext.byteOffset || 0, (ext.byteOffset || 0) + ext.byteLength), ext.mode, ext.filter);
        if (decoded.length !== view.byteLength) throw new Error(`Decoded length mismatch: ${file}`);
        delete view.extensions.EXT_meshopt_compression;
        if (!Object.keys(view.extensions).length) delete view.extensions;
      } else {
        if (view.buffer !== 0) throw new Error(`Missing decoded buffer: ${file}`);
        decoded = bin.subarray(view.byteOffset || 0, (view.byteOffset || 0) + view.byteLength);
        if (decoded.length !== view.byteLength) throw new Error(`Truncated buffer: ${file}`);
      }
      const padding = (4 - size % 4) % 4; if (padding) { chunks.push(Buffer.alloc(padding)); size += padding; }
      view.buffer = 0; view.byteOffset = size; chunks.push(decoded); size += decoded.length;
    }
    const data = Buffer.concat(chunks);
    const samples = [];
    for (const [i, accessor] of (json.accessors || []).entries()) {
      if (accessor.sparse || !dimensions[accessor.type]) throw new Error(`Accessor needs conversion: ${file}/${i}`);
      const Type = componentArrays[accessor.componentType], count = dimensions[accessor.type];
      const view = json.bufferViews[accessor.bufferView];
      for (const element of new Set([0, accessor.count - 1])) {
        const offset = view.byteOffset + (accessor.byteOffset || 0) + element * (view.byteStride || count * Type.BYTES_PER_ELEMENT);
        const raw = data.subarray(offset, offset + count * Type.BYTES_PER_ELEMENT);
        const values = new Type(Uint8Array.from(raw).buffer);
        const reference = new BufferAttribute(values, count, !!accessor.normalized);
        samples.push([i, element, ...Array.from({length: count}, (_, c) => reference.getComponent(0, c))]);
      }
    }
    probes.push({asset: file, samples});
    json.buffers = [{byteLength: size}];
    for (const key of ['extensionsUsed', 'extensionsRequired']) if (json[key]) json[key] = json[key].filter(e => e !== 'EXT_meshopt_compression');
    const converted = gzipSync(writeGlb(json, data)); const digest = hash(converted);
    const name = digest + '.glb.gz', destination = path.join(output, name);
    if (!fs.existsSync(destination) || hash(fs.readFileSync(destination)) !== digest) {
      fs.writeFileSync(destination + '.tmp', converted); fs.renameSync(destination + '.tmp', destination);
    }
    assets[file] = {file: 'models/' + name, sha256: digest, sourceSha256: hash(original), bytes: converted.length,
      nodes: (json.nodes || []).length, meshes: (json.meshes || []).length, skins: (json.skins || []).length,
      animations: (json.animations || []).map(a => a.name || ''), accessors: (json.accessors || []).length};
  }
  for (const [key, file] of Object.entries(index)) sources[key] = {asset: file, metadata: DT.MODEL_SOURCES[key] || {}};
  for (const item of legends) {
    if (item.data?.file) sources['legend:' + item.id].metadata = {...item.data, companion: true};
    else if (!sources[item.data?.model]) throw new Error('Missing companion model: ' + item.id);
  }
  for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
    DT.Sets.activate(set.id, pace);
    variants[`${set.id}/${pace}`] = Object.fromEntries(Object.values(DT.LINE).map(l => [l.id, [0,1,2].map(stage => DT.modelKey(l, stage))]));
  }
  fs.writeFileSync(path.join(root, 'native/content/models.json'), JSON.stringify({schemaVersion: 1, assets, sources, variants}) + '\n');
  fs.writeFileSync(path.join(root, 'native/tests/fixtures/models.json.gz'), gzipSync(JSON.stringify(probes, (_key, value) => typeof value === 'number' && !Number.isFinite(value) ? String(value) : value)));
  require('node:child_process').execFileSync('python', [path.join(__dirname, 'prepare-textures.py')], {stdio: 'inherit'});
  console.log(`Exported ${Object.keys(assets).length} native model assets and ${Object.keys(sources).length} mappings.`);
}
main().catch(e => {console.error(e); process.exitCode = 1;});
