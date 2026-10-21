// Compile the original fallback/rest-pose preparation into native data, never runtime JS.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const vm=require('node:vm');
const root=path.resolve(__dirname,'../..'), content=path.join(root,'native/artifacts/model-content');
async function main(){
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const manifest=path.join(root,'native/content/models.json'), catalog=JSON.parse(fs.readFileSync(manifest));
  fs.mkdirSync(path.join(content,'motions'),{recursive:true});
  const fixtures=[],actionFixtures=[]; let generated=0,reparented=0;
  const renderer=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
  const methods=renderer.slice(renderer.indexOf('  play(role) {'),renderer.indexOf('  applyAlpha(force) {'));
  const operations=[{delta:.01},{role:'attack'},{delta:0},{delta:.04},{role:'attack'},{delta:.05},{role:'special'},
    {delta:.04},{role:'attack'},{delta:.12},{role:'win'},{delta:2.3},{role:'attack'},{end:true},{delta:.03},{delta:.15},
    {role:'attack'},{delta:.34},{role:'attack'},{delta:.9},{role:'missing'},{role:'special'},{delta:.04},{role:'special'},{delta:1.2},{delta:.2}];
  for(const [key,source] of Object.entries(catalog.sources)){
    const entry=catalog.assets[source.asset], bytes=zlib.gunzipSync(fs.readFileSync(path.join(content,entry.file)));
    const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});
    parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();
    parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
    const nodes=await Promise.all(json.nodes.map((_,i)=>parser.getDependency('node',i)));
    const ids=new Map(nodes.map((n,i)=>[n,i]));
    const before=nodes.map(n=>n.parent);
    const clips=source.metadata.companion?gltf.animations:modelClips(gltf,source.metadata);
    const transforms=nodes.map((n,i)=>{
      let parent=n.parent;while(parent&&!ids.has(parent))parent=parent.parent;
      if(n.parent!==before[i])reparented++;
      const result={parent:parent?ids.get(parent):-1,translation:n.position.toArray(),rotation:n.quaternion.toArray(),scale:n.scale.toArray()};
      if(![...result.translation,...result.rotation,...result.scale].every(Number.isFinite))throw Error(`Nonfinite prepared rest: ${key}/${i}`);
      return result;
    });
    const prepared=clips.map(clip=>{
      const authored=gltf.animations.indexOf(clip);if(authored>=0)return {authored};
      generated++;
      return {name:clip.name,duration:clip.duration,channels:clip.tracks.map(track=>{
        const binding=THREE.PropertyBinding.parseTrackName(track.name),node=THREE.PropertyBinding.findNode(gltf.scene,binding.nodeName);
        if(!ids.has(node))throw Error(`Unmapped fallback bone: ${key}/${track.name}`);
        return {node:ids.get(node),path:binding.propertyName,times:Array.from(track.times),values:Array.from(track.values)};
      })};
    });
    const payload=zlib.gzipSync(JSON.stringify({schemaVersion:1,nodes:transforms,clips:prepared}));
    const sha256=crypto.createHash('sha256').update(payload).digest('hex'),file=`motions/${sha256}.json.gz`;
    fs.writeFileSync(path.join(content,file),payload);source.motion={file,sha256,bytes:payload.length};
    const selected=new Set([0,nodes.length-1]);
    // These reviewed rigid-part clips articulate flat mesh nodes rather than
    // skin joints. Compare every part in native motion/action parity fixtures.
    if(['rip-dw1_agumon','rip-dw1_airdramon','rip-dw1_akatorimon','rip-dw1_brachiomon','rip-dw1_frigimon'].includes(source.metadata.uid))nodes.forEach((_,i)=>selected.add(i));
    for(const skin of json.skins||[]){selected.add(skin.joints[0]);selected.add(skin.joints[Math.floor(skin.joints.length/2)]);selected.add(skin.joints.at(-1));}
    nodes.forEach((n,i)=>{if(n.parent!==before[i])selected.add(i);});
    const samples=[];
    const capture=(clip,time)=>{gltf.scene.updateMatrixWorld(true);const world=[...selected].map(node=>({node,matrix:nodes[node].matrixWorld.toArray()})).filter(x=>x.matrix.every(Number.isFinite));samples.push({clip,time,world});};
    capture(-1,0);
    for(const [index,clip] of clips.entries()){
      // Original corrupt clips are tested by the existing native repair suite instead.
      if(clip.tracks.some(t=>!Array.from(t.values).every(Number.isFinite)))continue;
      const mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(clip);
      action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
      for(const fraction of [0,.37,.91,1]){const time=clip.duration*fraction;mixer.setTime(time);capture(index,time);}
      mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
    }
    fixtures.push({key,samples});
    const idle=clips.find(c=>/idle|wait|stand/i.test(c.name))||clips.find(c=>/_bn01/i.test(c.name))||clips[0];
    if(idle && clips.every(c=>c.tracks.every(t=>Array.from(t.values).every(Number.isFinite)))){
      const template=new THREE.AnimationMixer(gltf.scene);template.clipAction(idle).play();template.update(0);
      const view=vm.runInNewContext('new (class {'+methods+'})()',{THREE,ATTACK_MAX:.55,SPECIAL_MAX:1.1});
      view.mixer=new THREE.AnimationMixer(gltf.scene);view.idleAction=view.mixer.clipAction(idle);view.idleAction.time=idle.duration*.17;view.idleAction.play();
      view.clips=Object.fromEntries(['attack','special','win'].map(role=>[role,clips.find(c=>c.name===role)]));
      view.mixer.addEventListener('finished',e=>{if(e.action===view.oneShot)view.endOneShot();});
      const steps=[];
      for(const operation of operations){
        let accepted=null;
        if(operation.role)accepted=view.play(operation.role);
        else if(operation.end)view.endOneShot();
        else view.mixer.update(operation.delta);
        gltf.scene.updateMatrixWorld(true);
        steps.push({...operation,request:operation.role??null,accepted,role:view.oneShotRole??null,time:view.mixer.time,
          actions:view.mixer._actions.map(a=>({name:a.getClip().name,time:a.time,weight:a.getEffectiveWeight(),enabled:a.enabled,speed:a.timeScale})),
          locals:key.includes('CS_Goblimon')?nodes.map(n=>[...n.position.toArray(),...n.quaternion.toArray(),...n.scale.toArray()]):undefined,
          world:[...selected].map(node=>({node,matrix:nodes[node].matrixWorld.toArray()})).filter(x=>x.matrix.every(Number.isFinite))});
      }
      actionFixtures.push({key,idleOffset:idle.duration*.17,steps});
    }
  }
  fs.writeFileSync(manifest,JSON.stringify(catalog)+'\n');
  fs.writeFileSync(path.join(root,'native/tests/fixtures/motions.json.gz'),zlib.gzipSync(JSON.stringify(fixtures)));
  fs.writeFileSync(path.join(root,'native/tests/fixtures/actions.json.gz'),zlib.gzipSync(JSON.stringify(actionFixtures)));
  const directory=path.resolve(content,'motions'),keep=new Set(Object.values(catalog.sources).map(s=>path.basename(s.motion.file)));
  for(const name of fs.readdirSync(directory)){
    if(!/^[0-9a-f]{64}\.json\.gz$/.test(name)||keep.has(name))continue;
    const obsolete=path.resolve(directory,name);
    if(path.dirname(obsolete)!==directory)throw Error('Motion cleanup escaped its generated directory.');
    fs.unlinkSync(obsolete);
  }
  console.log(`Prepared ${fixtures.length} model sources, ${generated} fallback clips, ${reparented} reparented nodes.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
