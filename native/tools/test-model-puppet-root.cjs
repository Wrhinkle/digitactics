// Exact Puppetmon regression: common translation must preserve the independently
// animated mallet and every string/body vertex relative to the creature.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict'),crypto=require('node:crypto'),vm=require('node:vm');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),source=catalog.sources['woodmon:2'];
  assert.equal(source.metadata.uid,'rip-puppetmon');
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
  async function load(){
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    return new Promise((resolve,reject)=>parser.parse(resolve,reject));
  }
  const raw=await load(),fixed=await load(),clips=modelClips(fixed,source.metadata),original=raw.animations.find(c=>c.name==='special'),corrected=clips.find(c=>c.name==='special');
  const failures=[],check=(ok,message)=>{if(!ok)failures.push(message);};
  for(const track of original.tracks){const retained=corrected.tracks.find(t=>t.name===track.name);assert.deepEqual(THREE.KeyframeTrack.toJSON(retained),THREE.KeyframeTrack.toJSON(track),'Original track unchanged: '+track.name);}
  const clipData=clip=>{const data=THREE.AnimationClip.toJSON(clip);delete data.uuid;return data;};
  for(const clip of raw.animations)if(clip.name!=='special')assert.deepEqual(clipData(clips.find(c=>c.name===clip.name)),clipData(clip),'Untouched '+clip.name);
  const body=fixed.scene.getObjectByName('J_center'),weapon=fixed.scene.getObjectByName('J_weapon'),anchor=fixed.scene.getObjectByName('J_root');
  assert.equal(body.parent,anchor);assert.equal(weapon.parent,anchor);
  assert.deepEqual(anchor.children.map(n=>n.name).sort(),['J_center','J_weapon']);
  assert.ok(raw.animations.every(c=>c.tracks.every(t=>!t.name.startsWith('J_root.'))),'Common parent must have no authored animation');
  const descendants=node=>{const set=new Set();node.traverse(n=>set.add(n));return set;},bodyBones=descendants(body),weaponBones=descendants(weapon);
  let total=0,bodyWeight=0,weaponWeight=0,stringBones=0;
  fixed.scene.traverse(mesh=>{
    if(mesh.isBone&&/^J_chen_/.test(mesh.name)){assert.ok(bodyBones.has(mesh),'Strings remain inside body subtree');stringBones++;}
    if(!mesh.isSkinnedMesh)return;
    const si=mesh.geometry.attributes.skinIndex,sw=mesh.geometry.attributes.skinWeight;
    for(let i=0;i<si.count;i++)for(let j=0;j<4;j++){const w=sw.getComponent(i,j),bone=mesh.skeleton.bones[si.getComponent(i,j)];total+=w;if(bodyBones.has(bone))bodyWeight+=w;if(weaponBones.has(bone))weaponWeight+=w;}
  });
  assert.ok(total>0&&stringBones>0&&Math.abs((bodyWeight+weaponWeight)/total-1)<1e-6,'Two exact subtrees cover complete weighted skin');
  assert.ok(bodyWeight/total>.90&&bodyWeight/total<.92&&weaponWeight/total>.08,'Retain original body/independent-mallet partition');
  function setup(gltf,available){
    const idle=available.find(c=>c.name==='idle'),template=new THREE.AnimationMixer(gltf.scene);template.clipAction(idle).play();template.update(0);gltf.scene.updateMatrixWorld(true);
    const box=new THREE.Box3().setFromObject(gltf.scene),origin=gltf.scene.getObjectByName('J_center').getWorldPosition(new THREE.Vector3()),mixer=new THREE.AnimationMixer(gltf.scene);
    const action=mixer.clipAction(available.find(c=>c.name==='special'));action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
    return {gltf,mixer,origin,height:box.max.y-box.min.y};
  }
  const before=setup(raw,raw.animations),after=setup(fixed,clips),height=after.height;
  function pose(state,t){state.mixer.setTime(t);state.gltf.scene.updateMatrixWorld(true);state.gltf.scene.traverse(n=>{if(n.isSkinnedMesh)n.skeleton.update();});return state.gltf.scene.getObjectByName('J_center').getWorldPosition(new THREE.Vector3());}
  function vertices(gltf){const values=[];gltf.scene.traverse(mesh=>{if(!mesh.isMesh)return;for(let i=0;i<mesh.geometry.attributes.position.count;i++)values.push(mesh.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(mesh.matrixWorld));});return values;}
  const frames=[];let maxRelativeError=0,maxRawRise=0,maxFixedRise=0,maxFixedDrop=0,maxHorizontal=0;
  for(let i=0;i<=64;i++){
    const t=original.duration*i/64,p=pose(before,t),q=pose(after,t),delta=q.clone().sub(p),travel=q.clone().sub(after.origin),a=vertices(raw),b=vertices(fixed);
    assert.equal(a.length,b.length);let error=0;
    for(let v=0;v<a.length;v++){assert.ok(b[v].toArray().every(Number.isFinite),'Finite skin');error=Math.max(error,b[v].clone().sub(a[v]).sub(delta).length()/height);}
    maxRelativeError=Math.max(maxRelativeError,error);maxRawRise=Math.max(maxRawRise,(p.y-before.origin.y)/height);maxFixedRise=Math.max(maxFixedRise,travel.y/height);maxFixedDrop=Math.max(maxFixedDrop,-travel.y/height);maxHorizontal=Math.max(maxHorizontal,Math.hypot(travel.x,travel.z)/height);
    check(error<1e-5,'Whole weighted shape/weapon relation changed at '+i+'/64: '+error);
    check(Math.hypot(travel.x,travel.z)<height*1e-5&&travel.y<=height*.351&&travel.y>=-height*.081,'Body escaped envelope at '+i+'/64');
    if(i===64)check(travel.length()<height*1e-5,'Ending anchor did not settle');
    frames.push({time:t,bodyTravelIdleHeights:travel.toArray().map(v=>v/height),maximumRelativeVertexError:error});
  }
  assert.ok(maxRawRise>1.9,'Original cinematic jump must reproduce');
  check(maxFixedRise>.005,'Retain authored vertical gesture');
  check(corrected.tracks.length===original.tracks.length+1&&corrected.tracks.some(t=>t.name==='J_root.position'),'Only added common-parent translation is permitted');
  // Exercise the actual source action controller, including compressed playback,
  // fade-in/out and restoration of a root absent from the idle clip.
  const runtimeGltf=await load(),runtimeClips=modelClips(runtimeGltf,source.metadata),runtimeIdle=runtimeClips.find(c=>c.name==='idle');
  const template=new THREE.AnimationMixer(runtimeGltf.scene);template.clipAction(runtimeIdle).play();template.update(0);runtimeGltf.scene.updateMatrixWorld(true);
  const renderSource=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8'),methods=renderSource.slice(renderSource.indexOf('  play(role) {'),renderSource.indexOf('  applyAlpha(force) {'));
  const view=vm.runInNewContext('new (class {'+methods+'})()',{THREE,ATTACK_MAX:.55,SPECIAL_MAX:1.1});
  view.mixer=new THREE.AnimationMixer(runtimeGltf.scene);view.idleAction=view.mixer.clipAction(runtimeIdle);view.idleAction.play();
  view.clips=Object.fromEntries(['attack','special','win'].map(role=>[role,runtimeClips.find(c=>c.name===role)]));
  view.mixer.addEventListener('finished',e=>{if(e.action===view.oneShot)view.endOneShot();});view.play('special');
  let runtimeSamples=0;for(let i=0;i<90;i++){
    view.mixer.update(1/60);runtimeGltf.scene.updateMatrixWorld(true);runtimeGltf.scene.traverse(n=>{if(n.isSkinnedMesh)n.skeleton.update();});
    assert.ok(vertices(runtimeGltf).every(p=>p.toArray().every(Number.isFinite)),'Finite production-controller blend');runtimeSamples++;
  }
  const runtimeRootResidual=runtimeGltf.scene.getObjectByName('J_root').position.length()/height;
  check(runtimeRootResidual<1e-8,'Common root must restore after one-shot fade');
  check(!view.oneShot,'Production controller must finish special');
  const report={sourceKey:'woodmon:2',scope:'Prepared special full cycle; exact all-vertex relative pose, body/string/mallet partition and untouched clips. Raster/production transition review remains separate.',
    sourceHashes:Object.fromEntries(['js/model-motion.js','native/tools/test-model-puppet-root.cjs'].map(p=>[p,crypto.createHash('sha256').update(fs.readFileSync(path.join(root,p))).digest('hex')])),
    samples:65,runtimeSamples,runtimeRootResidual,bodyFraction:bodyWeight/total,weaponFraction:weaponWeight/total,stringBones,maxRawRise,maxFixedRise,maxFixedDrop,maxHorizontal,maxRelativeError,failures,frames};
  const output=process.argv[2];if(output)fs.writeFileSync(output,JSON.stringify(report,null,2));
  console.log(JSON.stringify({...report,frames:undefined},null,2));assert.equal(failures.length,0,failures.join('\n'));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
