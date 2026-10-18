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
  const catalog = JSON.parse(read('native/content/models.json'));
  const traces = [];
  const filters = new Map([[THREE.NearestFilter,9728],[THREE.LinearFilter,9729],[THREE.NearestMipmapNearestFilter,9984],
    [THREE.LinearMipmapNearestFilter,9985],[THREE.NearestMipmapLinearFilter,9986],[THREE.LinearMipmapLinearFilter,9987]]);
  const wraps = new Map([[THREE.RepeatWrapping,10497],[THREE.ClampToEdgeWrapping,33071],[THREE.MirroredRepeatWrapping,33648]]);
  for (const [asset, entry] of Object.entries(catalog.assets)) {
    const bytes = zlib.gunzipSync(fs.readFileSync(path.join(root, 'native/artifacts/model-content', entry.file)));
    const json = JSON.parse(bytes.subarray(20,20+bytes.readUInt32LE(12)));
    const parser = new GLTFParser(json, {manager:THREE.DefaultLoadingManager});
    parser.extensions.KHR_materials_unlit = new GLTFMaterialsUnlitExtension();
    parser.plugins.specGloss = plugin(parser);
    parser.plugins.specular = new GLTFMaterialsSpecularExtension(parser);
    parser.plugins.emission = new GLTFMaterialsEmissiveStrengthExtension(parser);
    parser.loadImageSource = async () => new THREE.Texture();
    const materials = [];
    for (let index = 0; index < (json.materials || []).length; index++) {
      const material = await parser.loadMaterial(index);
      vm.runInNewContext(correction, {scene:{traverse:fn => fn({isMesh:true,material})},src:{}});
      const binding = name => {
        const map = material[name]; if (!map) return null;
        return {texture:parser.associations.get(map).textures, texCoord:map.channel, srgb:map.colorSpace === THREE.SRGBColorSpace};
      };
      materials.push({color:[material.color.r,material.color.g,material.color.b,material.opacity], unlit:!!material.isMeshBasicMaterial,
        doubleSided:material.side===THREE.DoubleSide,transparent:material.transparent,depthWrite:material.depthWrite,
        alphaCutoff:material.alphaTest,roughness:material.roughness??1,specularIntensity:material.specularIntensity??1,
        colorMap:binding('map'),roughnessMap:binding('roughnessMap'),normalMap:binding('normalMap'),specularMap:binding('specularIntensityMap'),
        emissive:material.emissive?.toArray()||[0,0,0],emissiveIntensity:material.emissiveIntensity??1,emissiveMap:binding('emissiveMap')});
    }
    const textures = [];
    for (let index = 0; index < (json.textures || []).length; index++) {
      const texture = await parser.getDependency('texture', index);
      textures.push({image:json.textures[index].source,magFilter:filters.get(texture.magFilter),minFilter:filters.get(texture.minFilter),
        wrapS:wraps.get(texture.wrapS),wrapT:wraps.get(texture.wrapT),mipmaps:texture.generateMipmaps});
    }
    traces.push({asset,materials,textures});
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/materials.json'),JSON.stringify(traces)+'\n');
  console.log(`Exported ${traces.reduce((n,t)=>n+t.materials.length,0)} original-loader materials.`);
}
main().catch(error => {console.error(error);process.exitCode=1;});
