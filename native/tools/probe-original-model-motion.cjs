// Diagnostic: evaluate original Electron preparation/mixer and exact skinned bounds.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const {pathToFileURL}=require('node:url');
const vm=require('node:vm');
async function main(){
  const [key,role,timeText]=process.argv.slice(2),time=Number(timeText),runtime=process.argv.includes('--runtime');
  if(!key||!role||!Number.isFinite(time))throw Error('Usage: node native/tools/probe-original-model-motion.cjs sourceKey role time [--runtime]');
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),source=catalog.sources[key];
  if(!source)throw Error('Unknown source '+key);
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file)));
  const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
  parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
  const script=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(script).toString('base64'));
  const clips=source.metadata.companion?gltf.animations:modelClips(gltf,source.metadata),idle=clips.find(c=>/idle|wait|stand/i.test(c.name))||clips.find(c=>/_bn01/i.test(c.name))||clips[0];
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  const base=new THREE.Box3().setFromObject(gltf.scene),height=base.max.y-base.min.y,center=base.getCenter(new THREE.Vector3());
  const clip=clips.find(c=>c.name===role);if(!clip)throw Error('Missing role '+role);
  if(runtime){
    // UnitView clones an already-idle template before creating its action mixer.
    // Unkeyed channels therefore retain that assembled pose, not the GLB bind pose.
    const renderer=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
    const methods=renderer.slice(renderer.indexOf('  play(role) {'),renderer.indexOf('  applyAlpha(force) {'));
    const view=vm.runInNewContext('new (class {'+methods+'})()',{THREE,ATTACK_MAX:.55,SPECIAL_MAX:1.1});
    view.mixer=new THREE.AnimationMixer(gltf.scene);view.idleAction=view.mixer.clipAction(idle);view.idleAction.play();
    view.clips=Object.fromEntries(['attack','special','win'].map(name=>[name,clips.find(c=>c.name===name)]));
    view.mixer.addEventListener('finished',e=>{if(e.action===view.oneShot)view.endOneShot();});
    view.play(role);let elapsed=0;
    while(elapsed<time){const delta=Math.min(1/60,time-elapsed);view.mixer.update(delta);elapsed+=delta;}
  }else{
    mixer.stopAllAction();
    const action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);
  }
  gltf.scene.updateMatrixWorld(true);
  gltf.scene.traverse(o=>{if(o.isSkinnedMesh)o.skeleton.update();});
  const exact=new THREE.Box3().setFromObject(gltf.scene,true),normalize=p=>[(p.x-center.x)/height,(p.y-base.min.y)/height,(p.z-center.z)/height];
  const result={sourceKey:key,asset:source.asset,mode:runtime?'original-combat':'prepared-clip',role,time,duration:clip.duration,baselineHeight:height,min:normalize(exact.min),max:normalize(exact.max),normalizedHeight:(exact.max.y-exact.min.y)/height};
  const file=path.join(root,'native/artifacts/original-model-probe-'+key.replace(/[^a-zA-Z0-9]/g,'_')+(runtime?'-runtime':'')+'.json');fs.writeFileSync(file,JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
