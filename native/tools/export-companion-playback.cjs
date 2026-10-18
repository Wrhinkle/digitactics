const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
async function main(){
 const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
 const source=fs.readFileSync(path.join(root,'js/legends.js'),'utf8');
 const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/companion-models.json')));
 const models=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
 const motionSource=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8').replace("from 'three'",`from '${require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
 const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(motionSource).toString('base64'));
 const policy=source.slice(source.indexOf('const clipOf ='),source.indexOf('// ---------- view ----------'));
 const methods=source.slice(source.indexOf('  base() {'),source.indexOf('  dispose() {'));
 const fixture=[];
 for(const [kind,visuals] of Object.entries({bases:catalog.bases,forms:catalog.forms}))for(const [id,visual] of Object.entries(visuals)){
  const model=models.sources[visual.source],asset=models.assets[model.asset];
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',asset.file))),len=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+len)),binary=bytes.subarray(28+len);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
  const nodes=await Promise.all(json.nodes.map((_,i)=>parser.getDependency('node',i)));
  const clips=model.metadata.companion?gltf.animations:modelClips(gltf,model.metadata);
  const idle=clips.find(x=>x.name==='idle')||clips.find(x=>/idle|wait|stand/i.test(x.name))||clips.find(x=>/_bn01/i.test(x.name))||clips[0];
  // Template normalization starts from the authored idle pose.
  if(idle){const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);}
  const ctx={THREE,ROLE_FALLBACK:catalog.fallbacks,PROC:visual.procedural};vm.createContext(ctx);
  vm.runInContext(policy+';this.view=new (class{'+methods+'})();',ctx);const v=ctx.view;
  v.time=0;v.yaw=.3;v.body=new THREE.Object3D();v.body.rotation.y=.3;v.visual=new THREE.Object3D();v.visual.userData={rotY:.37,s:1.2};v.t={clips:Object.fromEntries(clips.map(c=>[c.name,c])),item:{data:{clips:visual.clips}},proc:visual.procedural};v.seq=null;v.moving=false;v.mixer=null;v.hasRun=false;
  v.visual.rotation.y=.37;v.visual.scale.setScalar(1.2);
  if(idle){v.mixer=new THREE.AnimationMixer(gltf.scene);v.idle=v.mixer.clipAction(idle);v.idle.play();const run=['run','walk','move'].map(n=>clips.find(c=>c.name===n)).find(Boolean);v.run=run?v.mixer.clipAction(run):null;v.hasRun=!!v.run;v.cur=v.idle;}
  const selected=new Set([0,nodes.length-1]);for(const skin of json.skins||[])for(const n of [skin.joints[0],skin.joints[Math.floor(skin.joints.length/2)],skin.joints.at(-1)])selected.add(n);
  const operations=[{delta:0},{delta:.03},{moving:true},{delta:.06},{play:'happy'},{delta:.3},{moving:false},{delta:.1}];
  for(const role of ['dance','sad','happy','taunt','eat','wave','hit','dance'])operations.push({play:role},...[0,.03,.09,.24,.8,1.7,2.4,3.5].map(delta=>({delta})));
  operations.push({play:'dance'},{delta:.04},{moving:true},{delta:.06},{moving:false},{yaw:7},{delta:.09},{delta:.4});
  operations.push({play:'happy'},{delta:.02},{play:'taunt'},{delta:.02},{play:'eat'},{delta:.02},{moving:true},{delta:.01},{moving:false},{delta:.06},{play:'dance'},{delta:.03},{play:'sad'},{delta:.02},{delta:.4});
  const steps=[];
  for(const op of operations){
   if(op.play)v.play(op.play);else if(op.moving!=null)v.setMoving(op.moving);else if(op.yaw!=null)v.yaw=op.yaw;else v.update(op.delta);
   gltf.scene.updateMatrixWorld(true);
   steps.push({...op,time:v.time,role:v.seq?.role||null,index:v.seq?.i??-1,moving:v.moving,bodyYaw:v.body.rotation.y,
    transform:[...v.visual.position.toArray(),v.visual.rotation.x,v.visual.rotation.y,v.visual.rotation.z,...v.visual.scale.toArray()],
    actions:(v.mixer? v.mixer._actions:[]).map(a=>({name:a.getClip().name,time:a.time,weight:a.getEffectiveWeight(),speed:a.timeScale,enabled:a.enabled,paused:a.paused})),
    world:[...selected].map(node=>({node,matrix:nodes[node].matrixWorld.toArray()})).filter(x=>x.matrix.every(Number.isFinite))});
  }
  fixture.push({kind,id,steps});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/companion-playback.json.gz'),zlib.gzipSync(JSON.stringify(fixture)));
 console.log(`Exported ${fixture.length} original companion playback traces.`);
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
