const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  for(const [key,bodyName,role='special'] of [['drg:Gallantmon Crimson Mode','J_root'],['ryudamon:2','J_center'],
    ['ac:Phantomon','J_center','win'],['drg:Icemon','J_center','win'],['catalog:CS_Dracomon:chr208','J_center','win'],['dracomon:0','J_center','win'],['drg:Raptordramon','J_center','win'],
    ['lunamon:2','J_center'],['drg:PileVolcamon','J_center'],['ac:Millenniummon','J_center'],['unique:L_Omnimon_X','J_center'],
    ['ac:Jupitermon','Bip001'],['ac:SkullSatamon','J_center','win'],['ac:Merukimon','J_center','win'],
    ['candlemon:1','J_root_04','attack'],['catalog:CS_Betamon:chr389','J_root','attack'],['catalog:CS_Alphamon:chr416','J_center'],
    ['catalog:MagnaGarurumon:chr678','J_center','special'],['catalog:MagnaGarurumon:chr678','J_center','win']]){
    const source=catalog.sources[key],bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject)),clips=modelClips(gltf,source.metadata),idle=clips.find(c=>c.name==='idle'),clip=clips.find(c=>c.name===role),raw=gltf.animations.find(c=>c.name===role);
    const template=new THREE.AnimationMixer(gltf.scene);template.clipAction(idle).play();template.update(0);gltf.scene.updateMatrixWorld(true);
    const body=gltf.scene.getObjectByName(bodyName),origin=body.getWorldPosition(new THREE.Vector3()),box=new THREE.Box3().setFromObject(gltf.scene),height=box.max.y-box.min.y;
    const rawTrack=raw.tracks.find(t=>t.name===bodyName+'.position'),rawYs=[];
    for(let i=0;i<rawTrack.values.length;i+=3)rawYs.push(new THREE.Vector3().fromArray(rawTrack.values,i).applyMatrix4(body.parent.matrixWorld).y);
    // UnitView constructs its mixer from the already assembled idle template;
    // channels absent from the action keep those idle transforms (e.g. Betamon scale).
    const mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();let motion=0;
    for(let i=0;i<=64;i++){
      mixer.setTime(clip.duration*i/64);gltf.scene.updateMatrixWorld(true);const p=body.getWorldPosition(new THREE.Vector3()).sub(origin);
      assert.ok(Math.hypot(p.x,p.z)<height*1e-5,`${key} ${role} root leaves simulated hex by ${Math.hypot(p.x,p.z)/height} body heights at ${i/64}`);
      assert.ok(p.y<=height*.351&&p.y>=-height*.081,`${key} ${role} root leaves bounded jump envelope: ${p.y/height}`);motion=Math.max(motion,Math.abs(p.y));
      if(i===64||role==='win'&&i===0)assert.ok(p.length()<height*1e-5,key+' '+role+' must start and finish at its simulated position; drift='+p.length()/height);
    }
    if(Math.max(...rawYs)-Math.min(...rawYs)>height*.005)assert.ok(motion>height*.005,key+' must retain visible authored vertical movement');
    // Wizardmon also has the separately tested 1/15x body-scale repair.
    for(const track of raw.tracks){if(track.name===bodyName+'.position'||key==='candlemon:1'&&track.name==='GRP_joint_03.scale')continue;const retained=clip.tracks.find(t=>t.name===track.name);assert.deepEqual(Array.from(retained.times),Array.from(track.times));assert.deepEqual(Array.from(retained.values),Array.from(track.values));}
    console.log('Root-travel regression passed: '+key+'; 65 whole-cycle samples, vertical action retained, other authored tracks preserved.');
  }
}
main().catch(error=>{console.error(error);process.exitCode=1;});
