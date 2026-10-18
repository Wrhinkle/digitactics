// Regression for authored cinematic whole-body scale keys in otherwise usable attacks.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
const {pathToFileURL}=require('node:url');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const text=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(text).toString('base64'));
  const key=process.argv[2]||'hagurumon:0',source=catalog.sources[key];
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
  const bodyName={'hagurumon:0':'GRP_joint_02','catalog:CS_Renamon:chr391':'J_root','biyomon:0':'GRP_joint_02','candlemon:1':'GRP_joint_03'}[key];
  assert.ok(bodyName,'No reviewed body-root fixture for '+key);let count=0;
  for(const [alias,entry] of Object.entries(catalog.sources).filter(([,entry])=>entry.asset===source.asset)){
    const clips=modelClips(gltf,entry.metadata),idle=clips.find(c=>c.name==='idle'),attack=clips.find(c=>c.name==='attack');
    const raw=gltf.animations.find(c=>c.name==='attack'),body=gltf.scene.getObjectByName(bodyName);assert.ok(body,alias+' body root missing');
    const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);const neutral=body.scale.clone();mixer.stopAllAction();
    const action=mixer.clipAction(attack);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
    for(let i=0;i<=32;i++){mixer.setTime(attack.duration*i/32);for(const axis of ['x','y','z'])assert.ok(Math.abs(body.scale[axis]/neutral[axis]-1)<1e-5,`${alias}: attack whole-body ${axis} scale is ${body.scale[axis]/neutral[axis]}x idle at ${attack.duration*i/32}s`);count++;}
    // The fix may only replace the offending body-scale channel. Preserve
    // authored attack duration, every rotation/position and attachment scale.
    assert.equal(attack.duration,raw.duration);assert.equal(attack.tracks.length,raw.tracks.length);
    for(const track of raw.tracks){if(track.name===bodyName+'.scale')continue;const retained=attack.tracks.find(t=>t.name===track.name);assert.deepEqual(Array.from(retained.times),Array.from(track.times));assert.deepEqual(Array.from(retained.values),Array.from(track.values));}
    assert.ok(attack.tracks.some(track=>track.name.endsWith('.quaternion')&&Array.from(track.values).some((value,i)=>i>=4&&Math.abs(value-track.values[i%4])>.01)),alias+' attack lost articulation');
    mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
  }
  console.log(`Body-scale regression passed: ${key}, ${count} cycle samples across every asset alias; authored articulation preserved.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
