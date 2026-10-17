const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib');
const root = path.resolve(__dirname, '../..');
async function main() {
  const {THREE, GLTFParser, GLTFMaterialsUnlitExtension} = await require('./three-loader.cjs')();
  const catalog = JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const traces = [];
  for (const [asset, entry] of Object.entries(catalog.assets)) {
    const bytes = zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',entry.file)));
    const length = bytes.readUInt32LE(12), json = JSON.parse(bytes.subarray(20,20+length));
    const binary = bytes.subarray(28+length);
    const parser = new GLTFParser(json,{manager:THREE.DefaultLoadingManager});
    parser.extensions.KHR_materials_unlit = new GLTFMaterialsUnlitExtension();
    parser.loadImageSource = async () => new THREE.Texture();
    parser.loadBuffer = async () => binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf = await new Promise((resolve,reject)=>parser.parse(resolve,reject));
    const selected = gltf.animations.find(a=>/idle|wait|stand/i.test(a.name))||gltf.animations.find(a=>/_bn01/i.test(a.name))||gltf.animations[0];
    const samples=[];
    for(const time of [0,.37]) {
      const mixer = new THREE.AnimationMixer(gltf.scene);
      if(selected){mixer.clipAction(selected).play();mixer.setTime(time);}
      gltf.scene.updateMatrixWorld(true);
      // Precise traversal uses the actual SkinnedMesh.getVertexPosition implementation.
      const bounds = new THREE.Box3().setFromObject(gltf.scene,true);
      const min=bounds.min.toArray(),max=bounds.max.toArray();
      if([...min,...max].every(Number.isFinite))samples.push({time,min,max});
      mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
    }
    const primitives=[];
    for(const [index,mesh] of (json.meshes||[]).entries()) {
      const object=await parser.getDependency('mesh',index);
      const list=object.isMesh?[object]:object.children;
      for(const o of list){
        const p=o.geometry.getAttribute('position'),w=o.geometry.getAttribute('skinWeight');
        const vertices=[];
        for(const i of new Set([0,Math.floor(p.count/2),p.count-1]))vertices.push({index:i,position:[p.getX(i),p.getY(i),p.getZ(i)],weights:w?[w.getX(i),w.getY(i),w.getZ(i),w.getW(i)]:null});
        const indices=o.geometry.index;
        primitives.push({mesh:index,count:p.count,indices:indices?indices.count:p.count,vertices});
      }
    }
    traces.push({asset,animation:selected?.name??null,samples,primitives});
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/meshes.json'),JSON.stringify(traces)+'\n');
  console.log(`Exported ${traces.length} original-loader mesh traces.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
