// Cyber Sleuth and Time Stranger rips ship FBX files without texture links. Their DAE files name each material's
// diffuse texture, so this copies those links into the converted GLBs.
// Run from this folder after convert-all.cjs: node patch-textures.cjs [Folder ...], then node optimize.mjs
const fs = require('fs');
const path = require('path');
const { NodeIO } = require('@gltf-transform/core');
const sharp = require('sharp');

const HERE = __dirname;
// --legends: patch models/src-legends/<folder lowercased>.glb instead (Little Legends converted from plain rip folders)
const LEGENDS = process.argv.includes('--legends');
const OUT = path.join(HERE, '..', '..', 'models', LEGENDS ? 'src-legends' : 'src');
const files = (d) => fs.readdirSync(d, { withFileTypes: true }).flatMap((e) => (e.isDirectory() ? files(path.join(d, e.name)) : [path.join(d, e.name)]));

// maps/*.json "_alias" (a folder converted from another rip folder) and "_retexture" ({ from.png: to.png }), as in convert-all.cjs
const EXT = { _alias: {}, _retexture: {}, _input: {} };
for (const f of fs.existsSync(path.join(HERE, 'maps')) ? fs.readdirSync(path.join(HERE, 'maps')).filter((n) => n.endsWith('.json')) : []) {
  const j = JSON.parse(fs.readFileSync(path.join(HERE, 'maps', f), 'utf8'));
  for (const k of Object.keys(EXT)) Object.assign(EXT[k], j[k] || {});
}

// material name -> texture file, from a COLLADA file's effects
function daeMap(dae) {
  const x = fs.readFileSync(dae, 'utf8');
  const images = {};
  for (const m of x.matchAll(/<image id="([^"]+)"[^>]*>\s*<init_from>([^<]+)<\/init_from>/g)) images[m[1]] = m[2];
  const map = {};
  for (const m of x.matchAll(/<effect id="([^"]+)-effect">([\s\S]*?)<\/effect>/g)) {
    const t = m[2].match(/<texture texture="([^"]+)-sampler"/);
    if (!t) continue;
    // sampler -> surface -> image id (Blender names them <image>-sampler / <image>-surface)
    const img = images[t[1]] || images[t[1].replace(/_png$/, '.png')] || t[1].replace(/_png$/, '.png');
    map[m[1]] = img;
  }
  return map;
}

(async () => {
  const io = new NodeIO();
  let patched = 0;
  // node patch-textures.cjs [Folder ...]: only these rip folders (default: all)
  const only = process.argv.slice(2).filter((a) => !a.startsWith('--'));
  for (const folder of [...fs.readdirSync(path.join(HERE, 'rips', 'x')), ...Object.keys(EXT._alias)].filter((f) => !only.length || only.includes(f))) {
    const glb = path.join(OUT, `${LEGENDS ? '' : 'rip_'}${folder.toLowerCase()}.glb`);
    if (!fs.existsSync(glb)) continue;
    // shallowest first, so a rip's main textures win over alternate-color subfolders
    const preferred = EXT._input[folder] && path.dirname(path.join(HERE, 'rips', 'x', EXT._input[folder]));
    const distance = f => preferred ? path.relative(preferred, f).split(path.sep).length : f.split(path.sep).length;
    const all = files(path.join(HERE, 'rips', 'x', EXT._alias[folder] || folder)).sort((a, b) => distance(a) - distance(b));
    const daes = all.filter((f) => /\.dae$/i.test(f));
    // merge every DAE in the rip (some ship a fixed copy next to the original)
    const map = Object.assign({}, ...daes.filter((f) => !/splituv/i.test(f)).reverse().map(daeMap));
    const doc = await io.read(glb);
    const cache = {};
    let n = 0;
    // original game material names: base -> a01, spec/specA -> a01s, specB -> a02s
    const modelCode = EXT._input[folder] && path.basename(EXT._input[folder]).match(/(chr\d+|d\d+)/i)?.[1];
    const chr = modelCode || (all.map((f) => path.basename(f).match(/^(chr\d+|d\d+)a01\.png$/i)).find(Boolean) || [])[1];
    const colorPngs = all.filter((f) => /\.png$/i.test(f) && !/env|eff_|s(_a)?\.png$|mask|matcap|sss|eye/i.test(path.basename(f)));
    // Next Order rips: one material, base color in <id>_c00.png
    const sole = colorPngs.length === 1 ? colorPngs[0] : all.find((f) => /^[a-z]\d+_c00\.png$/i.test(path.basename(f))) || null;
    const byName = (name) => {
      if (!chr) return null;
      if (/base/i.test(name)) return `${chr}a01.png`;
      if (/specB/i.test(name)) return all.some((f) => path.basename(f) === `${chr}a02s.png`) ? `${chr}a02s.png` : `${chr}a01s.png`;
      if (/spec/i.test(name)) return `${chr}a01s.png`;
      return null;
    };
    // Links exports bake shading into vertex colors that three.js multiplies in (mostly black): drop them
    for (const mesh of doc.getRoot().listMeshes()) for (const prim of mesh.listPrimitives()) {
      const col = prim.getAttribute('COLOR_0');
      if (col) { prim.setAttribute('COLOR_0', null); n++; }
    }
    // Digimon New Century rips carry LOD copies (body_0_lod1, body_0_lod2) on top of the full mesh: remove them
    for (const node of doc.getRoot().listNodes()) {
      if (node.getMesh() && /_lod\d+$/i.test(node.getName())) { node.dispose(); n++; }
    }
    // outline shells (inverted hulls) cover the model in three.js: remove them
    for (const mesh of doc.getRoot().listMeshes()) {
      for (const prim of mesh.listPrimitives()) {
        const m = prim.getMaterial();
        if (m && /MTR_line|^eff_/i.test(m.getName())) { prim.dispose(); n++; }
      }
    }
    for (const mat of doc.getRoot().listMaterials()) {
      if (!mat.getEmissiveTexture() && mat.getEmissiveFactor().some(x=>x>0)) { mat.setEmissiveFactor([0,0,0]); n++; }
      const has = mat.getBaseColorTexture();
      // some Links FBX files carry a black diffuse factor next to a real texture (Ginryumon): use the texture's own color
      if (has && mat.getBaseColorFactor().slice(0, 3).every((v) => v < 0.05)) { mat.setBaseColorFactor([1, 1, 1, 1]); n++; }
      // FBX2glTF embeds a tiny placeholder PNG when it can't find the texture file; swap in the real one by name
      const stub = has && (!has.getImage() || has.getImage().length < 1024);
      const stubFile = stub ? `${has.getName().replace(/\.png$/i, '')}.png` : null;
      const base = (f) => f.split(/[\\/]/).pop();
      const swap = (f) => (f && (EXT._retexture[folder] || {})[base(f)]) || f;
      const find = (f) => f && (all.find(x=>x.replaceAll('\\','/').toLowerCase().endsWith('/'+f.replaceAll('\\','/').toLowerCase())) || all.find((x) => path.basename(x).toLowerCase() === base(f).toLowerCase()));
      // Time Stranger's e01 is only the eye-white mask; p01 carries the pupils and eyelids at UV0.
      const eyeChr = /eye/i.test(mat.getName()) && (mat.getName().match(/chr\d+/i) || [chr])[0];
      const eye = eyeChr ? `${eyeChr}p01.png` : null;
      // FBX uses original shader names, while companion DAE files rename them mat_000.
      // All shader passes need the diffuse atlas, not the white specular or cube-map mask.
      const shader = mat.getName().match(/^(chr\d+|d\d+)(base|spec|cube)(?:_?(\d+))?/i);
      const diffuse = shader && `${shader[1]}a${shader[2].toLowerCase()==='base' && shader[3] ? shader[3].padStart(2,'0') : '01'}.png`;
      const recolor = has && (EXT._retexture[folder]||{})[has.getName()];
      if (has && !stub && folder !== 'Gizamon' && !recolor && !(eye && find(swap(eye))) && !(diffuse && find(swap(diffuse)))) continue;
      // first candidate the rip has (a stub's own name, the DAE link, by material name, eyes); last resort for an untextured or
      // stub material: the rip's only color texture (Shoutmon, Meicoomon, Belphemon)
      const img = [recolor, eye, diffuse, stubFile, map[mat.getName()], byName(mat.getName())].map(swap).map(find).find(Boolean) || sole;
      if (!img) continue;
      if (!cache[img]) {
        // Eye atlases have transparent space around the pupils; the original shader supplies white beneath it.
        const bytes = eye && img === find(swap(eye)) ? await sharp(fs.readFileSync(img)).flatten({background:'#ffffff'}).png().toBuffer() : fs.readFileSync(img);
        cache[img] = doc.createTexture(path.basename(img)).setImage(bytes).setMimeType('image/png');
      }
      mat.setBaseColorTexture(cache[img]).setBaseColorFactor([1, 1, 1, 1]).setMetallicFactor(0).setRoughnessFactor(1);
      if (eye && img === find(swap(eye))) { mat.getBaseColorTextureInfo().setTexCoord(0); mat.setAlphaMode('OPAQUE'); }
      n++;
    }
    // drop textures no material uses any more
    for (const t of doc.getRoot().listTextures()) if (!t.listParents().some((p) => p !== doc.getRoot())) t.dispose();
    if (n) {
      await io.write(glb, doc);
      patched++;
      console.log(`${folder}: ${n} materials linked (${Object.values(map).filter((v, i, a) => a.indexOf(v) === i).join(', ')})`);
    }
  }
  console.log(`${patched} models patched`);
})();
