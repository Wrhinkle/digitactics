const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
const {pathToFileURL}=require('node:url');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),key='ac:Beelzemon',source=catalog.sources[key];
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const script=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(script).toString('base64'));
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject)),clips=modelClips(gltf,source.metadata),idle=clips.find(c=>c.name==='idle');
  const gun=gltf.scene.getObjectByName('J_gun_r'),hand=gltf.scene.getObjectByName('J_hand_r');assert.equal(gun.parent,hand);
  const mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(idle);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  const bounds=new THREE.Box3().setFromObject(gltf.scene),height=bounds.max.y-bounds.min.y;
  for(let i=0;i<=32;i++){
    mixer.setTime(idle.duration*i/32);gltf.scene.updateMatrixWorld(true);
    const distance=gun.getWorldPosition(new THREE.Vector3()).distanceTo(hand.getWorldPosition(new THREE.Vector3()));
    assert.ok(distance<height*.025,`${key} idle gun grip is detached ${distance/height} body heights from its hand`);
  }
  for(const raw of gltf.animations){
    const corrected=clips.find(c=>c.name===raw.name);assert.equal(corrected.duration,raw.duration);
    for(const track of raw.tracks){
      const retained=corrected.tracks.find(t=>t.name===track.name);assert.deepEqual(Array.from(retained.times),Array.from(track.times));
      if(track.name!=='J_gun_r.position'){assert.deepEqual(Array.from(retained.values),Array.from(track.values));continue;}
      // An attachment-origin correction must retain deliberate throws and recoil.
      for(let i=3;i<track.values.length;i++)assert.ok(Math.abs((retained.values[i]-retained.values[i%3])-(track.values[i]-track.values[i%3]))<1e-7,'Gun trajectory changed beyond its constant grip offset');
    }
  }
  mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
  console.log('Weapon grip regression passed: Beelzemon full idle cycle attached; authored recoil, throws, rotations and other tracks preserved.');
}
main().catch(error=>{console.error(error);process.exitCode=1;});
