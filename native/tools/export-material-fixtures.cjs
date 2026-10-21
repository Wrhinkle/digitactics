// Run the vendored loader and the game's material corrections without browser image I/O.
const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib'), vm = require('node:vm');
const root = path.resolve(__dirname, '../..');
const read = file => fs.readFileSync(path.join(root, file), 'utf8');
async function main() {
  const {THREE, GLTFParser, GLTFMaterialsUnlitExtension, GLTFMaterialsSpecularExtension, GLTFMaterialsEmissiveStrengthExtension} = await require('./three-loader.cjs')();
  const renderer = read('js/render3d.js');
  const pluginStart = renderer.indexOf('loader.register((parser) => (');
  const pluginEnd = renderer.indexOf('\nexport const lib', pluginStart);
  let plugin;
  vm.runInNewContext(renderer.slice(pluginStart, pluginEnd), {THREE, loader:{register: value => {plugin = value;}}});
  const correction = renderer.slice(renderer.indexOf('  scene.traverse((o) => {'), renderer.indexOf('  const actions = {}'));
  // Effect-card rule: the browser reads texture pixels through a canvas; here the
  // exported RGBA8 PNG is decoded directly and fed to the same decision code.
  const cards = {};
  vm.runInNewContext(renderer.slice(renderer.indexOf('const EFFECT_NAME'), renderer.indexOf('const cardStats')) +
    ';cards.stats=cardTextureStats;cards.mode=effectCardMode;', {cards});
  const pngPixels = data => {
    let width = 0, height = 0; const idat = [];
    for (let o = 8; o < data.length;) { const n = data.readUInt32BE(o), type = data.toString('latin1', o + 4, o + 8);
      if (type === 'IHDR') { width = data.readUInt32BE(o + 8); height = data.readUInt32BE(o + 12);
        if (data[o + 16] !== 8 || data[o + 17] !== 6 || data[o + 20] !== 0) throw Error('Expected exported RGBA8 PNG'); }
      if (type === 'IDAT') idat.push(data.subarray(o + 8, o + 8 + n)); o += 12 + n; }
    const raw = zlib.inflateSync(Buffer.concat(idat)), stride = width * 4, out = new Uint8Array(stride * height);
    for (let y = 0; y < height; y++) { const f = raw[y * (stride + 1)];
      for (let x = 0; x < stride; x++) { const v = raw[y * (stride + 1) + 1 + x], a = x >= 4 ? out[y * stride + x - 4] : 0, b = y ? out[(y - 1) * stride + x] : 0, c = x >= 4 && y ? out[(y - 1) * stride + x - 4] : 0;
        const p = a + b - c, pa = Math.abs(p - a), pb = Math.abs(p - b), pc = Math.abs(p - c);
        out[y * stride + x] = (v + (f === 0 ? 0 : f === 1 ? a : f === 2 ? b : f === 3 ? (a + b) >> 1 : pa <= pb && pa <= pc ? a : pb <= pc ? b : c)) & 255; } }
    return out;
  };
  const catalog = JSON.parse(read('native/content/models.json'));
  const traces = [];
  const filters = new Map([[THREE.NearestFilter,9728],[THREE.LinearFilter,9729],[THREE.NearestMipmapNearestFilter,9984],
    [THREE.LinearMipmapNearestFilter,9985],[THREE.NearestMipmapLinearFilter,9986],[THREE.LinearMipmapLinearFilter,9987]]);
  const wraps = new Map([[THREE.RepeatWrapping,10497],[THREE.ClampToEdgeWrapping,33071],[THREE.MirroredRepeatWrapping,33648]]);
  for (const [asset, entry] of Object.entries(catalog.assets)) {
    const bytes = zlib.gunzipSync(fs.readFileSync(path.join(root, 'native/artifacts/model-content', entry.file)));
    const json = JSON.parse(bytes.subarray(20,20+bytes.readUInt32LE(12))), binary = bytes.subarray(28+bytes.readUInt32LE(12)), imageStats = new Map();
    const statsFor = image => { if (!imageStats.has(image)) { const view = json.bufferViews[json.images[image].bufferView];
      imageStats.set(image, cards.stats(pngPixels(binary.subarray(view.byteOffset || 0, (view.byteOffset || 0) + view.byteLength)))); } return imageStats.get(image); };
    const parser = new GLTFParser(json, {manager:THREE.DefaultLoadingManager});
    parser.extensions.KHR_materials_unlit = new GLTFMaterialsUnlitExtension();
    parser.plugins.specGloss = plugin(parser);
    parser.plugins.specular = new GLTFMaterialsSpecularExtension(parser);
    parser.plugins.emission = new GLTFMaterialsEmissiveStrengthExtension(parser);
    parser.loadImageSource = async () => new THREE.Texture();
    const materials = [], vertexAlphaMaterials = [];
    const alphaMaterials = new Set((json.meshes || []).flatMap(m => m.primitives).filter(p =>
      p.attributes.COLOR_0 !== undefined && json.accessors[p.attributes.COLOR_0].type === 'VEC4').map(p => p.material));
    async function correctedMaterial(index, vertexAlpha) {
      const mesh = {isMesh:true, material:await parser.loadMaterial(index),geometry:{getAttribute:() => vertexAlpha ? {itemSize:4} : undefined}};
      const effectCard = m => { if (!m.map) return null; const image = json.textures[parser.associations.get(m.map).textures].source;
        return cards.mode([m.name, json.images[image].name], statsFor(image)); };
      vm.runInNewContext(correction, {scene:{traverse:fn => fn(mesh)},src:{},effectCard,THREE});
      const material = mesh.material;
      const binding = name => {
        const map = material[name]; if (!map) return null;
        return {texture:parser.associations.get(map).textures, texCoord:map.channel, srgb:map.colorSpace === THREE.SRGBColorSpace};
      };
      return {color:[material.color.r,material.color.g,material.color.b,material.opacity], unlit:!!material.isMeshBasicMaterial,
        doubleSided:material.side===THREE.DoubleSide,transparent:material.transparent,depthWrite:material.depthWrite,additive:material.blending===THREE.CustomBlending,
        alphaCutoff:material.alphaTest,roughness:material.roughness??1,specularIntensity:material.specularIntensity??1,normalScale:material.normalScale?.x??1,
        colorMap:binding('map'),roughnessMap:binding('roughnessMap'),normalMap:binding('normalMap'),specularMap:binding('specularIntensityMap'),
        emissive:material.emissive?.toArray()||[0,0,0],emissiveIntensity:material.emissiveIntensity??1,emissiveMap:binding('emissiveMap')};
    }
    for (let index = 0; index < (json.materials || []).length; index++) {
      // Capture the RGBA variant before a cached material's legacy correction can mutate it.
      if (alphaMaterials.has(index)) vertexAlphaMaterials.push({index,material:await correctedMaterial(index,true)});
      materials.push(await correctedMaterial(index,false));
    }
    const textures = [];
    for (let index = 0; index < (json.textures || []).length; index++) {
      const texture = await parser.getDependency('texture', index);
      textures.push({image:json.textures[index].source,magFilter:filters.get(texture.magFilter),minFilter:filters.get(texture.minFilter),
        wrapS:wraps.get(texture.wrapS),wrapT:wraps.get(texture.wrapT),mipmaps:texture.generateMipmaps});
    }
    traces.push({asset,materials,textures,...(vertexAlphaMaterials.length ? {vertexAlphaMaterials} : {})});
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/materials.json'),JSON.stringify(traces)+'\n');
  console.log(`Exported ${traces.reduce((n,t)=>n+t.materials.length,0)} original-loader materials.`);
}
main().catch(error => {console.error(error);process.exitCode=1;});
