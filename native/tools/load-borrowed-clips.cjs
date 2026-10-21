// Clips a clipless source borrows from its clipsFrom sibling (same skeleton,
// authored clips), prepared by the browser's own model-motion.js. Every
// exporter that prepares unit clips must pass these to modelClips/prepareTemplate
// so motion, dance and companion data agree with the browser.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
module.exports=async function(){
  const root=path.resolve(__dirname,'../..');
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {borrowedClips}=await require('./load-model-motion.cjs')();
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),cache=new Map();
  return async metadata=>{
    const key=metadata?.clipsFrom;
    if(!key)return null;
    if(cache.has(key))return cache.get(key);
    const source=catalog.sources[key];
    if(!source)throw Error(`Missing clipsFrom source: ${key}`);
    const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file)));
    const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
    if(!gltf.animations.length)throw Error(`clipsFrom source has no authored clips: ${key}`);
    const borrowed=borrowedClips(gltf,source.metadata);cache.set(key,borrowed);return borrowed;
  };
};
