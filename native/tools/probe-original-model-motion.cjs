// Diagnostic: evaluate original Electron preparation/mixer and exact skinned bounds.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const vm=require('node:vm');
async function main(){
  const [key,role,timeText]=process.argv.slice(2),time=Number(timeText),runtime=process.argv.includes('--runtime'),roots=process.argv.includes('--root-channels');
  if(!key||!role||!Number.isFinite(time))throw Error('Usage: node native/tools/probe-original-model-motion.cjs sourceKey role time [--runtime] [--root-channels]');
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),source=catalog.sources[key];
  if(!source)throw Error('Unknown source '+key);
  const hash=file=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex');
  const inputHashes=Object.fromEntries(['native/content/models.json','js/model-motion.js','js/model-anatomy.js','js/render3d.js'].map(file=>[file,hash(file)]));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file)));
  const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
  parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
  const {modelClips}=await require('./load-model-motion.cjs')();
  const clips=source.metadata.companion?gltf.animations:modelClips(gltf,source.metadata),idle=clips.find(c=>/idle|wait|stand/i.test(c.name))||clips.find(c=>/_bn01/i.test(c.name))||clips[0];
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  const base=new THREE.Box3().setFromObject(gltf.scene),height=base.max.y-base.min.y,center=base.getCenter(new THREE.Vector3());
  const rest=new Map(),influence=new Map();let totalInfluence=0;
  if(roots)gltf.scene.traverse(node=>{
    rest.set(node,{local:node.position.toArray(),world:node.getWorldPosition(new THREE.Vector3())});
    if(!node.isSkinnedMesh)return;
    const indices=node.geometry.attributes.skinIndex,weights=node.geometry.attributes.skinWeight;
    for(let i=0;i<indices.count;i++)for(let j=0;j<4;j++){
      const weight=weights.getComponent(i,j);if(weight<=0)continue;
      const bone=node.skeleton.bones[indices.getComponent(i,j)];influence.set(bone,(influence.get(bone)||0)+weight);totalInfluence+=weight;
    }
  });
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
  const result={sourceKey:key,asset:source.asset,inputHashes,mode:runtime?'original-combat':'prepared-clip',role,time,duration:clip.duration,baselineHeight:height,min:normalize(exact.min),max:normalize(exact.max),normalizedHeight:(exact.max.y-exact.min.y)/height};
  if(roots)result.positionChannels=clip.tracks.filter(track=>track.name.endsWith('.position')).map(track=>{
    const binding=THREE.PropertyBinding.parseTrackName(track.name),node=THREE.PropertyBinding.findNode(gltf.scene,binding.nodeName),opening=rest.get(node);
    let subtree=0;node.traverse(child=>subtree+=influence.get(child)||0);
    const displacement=node.getWorldPosition(new THREE.Vector3()).sub(opening.world).multiplyScalar(1/height);
    return {name:node.name,weightedSubtreeFraction:subtree/totalInfluence,idleLocal:opening.local,currentLocal:node.position.toArray(),worldDisplacementIdleHeights:displacement.toArray()};
  });
  if(Object.entries(inputHashes).some(([file,before])=>hash(file)!==before))throw Error('Probe inputs changed while sampling; rerun against a frozen source/export.');
  const file=path.join(root,'native/artifacts/original-model-probe-'+key.replace(/[^a-zA-Z0-9]/g,'_')+(runtime?'-runtime':'')+'.json');fs.writeFileSync(file,JSON.stringify(result,null,2));console.log(JSON.stringify(result));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
