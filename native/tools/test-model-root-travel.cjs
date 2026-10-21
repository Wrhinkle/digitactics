const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const requested=new Set(process.argv.slice(2)),visited=new Set();
  const terminalRoles={
    'rip-stingmon':['special'], 'rip-dinobeemon':['special','win'],
    'rip-zubamon':['special'], 'rip-zubaeagermon':['special'],
  };
  for(const [alias,canonical] of [['zubamon:2','extra:Durandamon'],['ac:Gumdramon','drg:Gumdramon'],...[0,1,2].map(stage=>['kimeramon:'+stage,'extra:Kimeramon'])]){
    assert.equal(catalog.sources[alias].asset,catalog.sources[canonical].asset,alias+' remains an exact asset alias');
    assert.equal(catalog.sources[alias].metadata.uid,catalog.sources[canonical].metadata.uid,alias+' retains the canonical preparation identity');
  }
  for(const [key,bodyName,role='special'] of [['drg:Gallantmon Crimson Mode','J_root'],['ryudamon:2','J_center'],
    ['ac:Phantomon','J_center','win'],['drg:Icemon','J_center','win'],['catalog:CS_Dracomon:chr208','J_center','win'],['dracomon:0','J_center','win'],['drg:Raptordramon','J_center','win'],
    ['lunamon:2','J_center'],['drg:PileVolcamon','J_center'],['ac:Millenniummon','J_center'],['unique:L_Omnimon_X','J_center'],
    ['ac:Jupitermon','Bip001'],['ac:SkullSatamon','J_center','win'],['ac:Merukimon','J_center','win'],
    ['candlemon:1','J_root_04','attack'],['catalog:CS_Betamon:chr389','J_root','attack'],['catalog:CS_Alphamon:chr416','J_center'],
    ['catalog:MagnaGarurumon:chr678','J_center','special'],['catalog:MagnaGarurumon:chr678','J_center','win'],
    ['drg:Valkyrimon','J_center','special'],
    ['extra:Durandamon','J_center','special'],['zubamon:2','J_center','special'],
    ['extra:Kimeramon','J_center','win'],['kimeramon:0','J_center','win'],['kimeramon:1','J_center','win'],['kimeramon:2','J_center','win'],
    ['drg:Grademon','J_center','special'],['drg:Gryphonmon','J_center','win'],['drg:Gumdramon','J_center','special'],['ac:Gumdramon','J_center','special'],
    ['wormmon:1','J_center','special'],['wormmon:2','J_center','special'],['wormmon:2','J_center','win'],
    ['zubamon:0','J_center','special'],['zubamon:1','J_center','special'],
    ['fanbeemon:0','J_center','special'],['fanbeemon:0','J_center','win'],['gaomon:0','J_root','special'],
    ['giz:SistermonBlancAwakened','J_center','special'],['giz:TyrantKabuterimon','J_center','win'],['gizamon:1','J_center','special'],
    ['hackmon:0','J_center','special'],['hackmon:1','J_center','special'],['hagurumon:2','J_root_03','attack'],['hawkmon:1','mass','special'],
    ['jazamon:0','J_center','win'],['jazamon:1','J_center','special'],['jazamon:1','J_center','win'],['jazamon:2','J_center','special'],['jazamon:2','J_center','win'],
    ['kudamon:2','J_center','special'],['kuwagamon:0','J_center','special'],['lobomon:1','J_center','special'],['lobomon:1','J_center','win'],
    ['lobomon:2','J_center','special'],['lobomon:2','J_center','win'],['lopmon:1','J_center','special'],['lunamon:1','J_center','special'],['lunamon:1','J_center','win'],
    ['mamemon:2','J_center','special'],['meicoomon:0','chr999J_center','special'],['meicoomon:1','J_center','special'],['meicoomon:2','J_center','special'],
    ['monodramon:0','J_center','special'],['monodramon:1','J_center','special'],['ryudamon:0','J_center','special'],['ryudamon:1','J_center','special'],['ryudamon:1','J_center','win'],
    ['unique:CS_Alphamon_NX','J_center','special'],['unique:CS_Alphamon_Ouryuken','J_center','special'],
    ['unique:CS_Arcadiamon_In_Training','J_center','special'],['unique:CS_Arcadiamon_In_Training','J_center','win'],['unique:CS_Omnimon_NX','J_center','special'],
    ['unique:L_CresGarurumon','J_center','special'],['unique:L_Minervamon','J_center','special'],['unique:L_Omnimon_Zwart_D','J_center','special'],
    ['unique:L_Pandamon','J_mass','special'],['vorvomon:0','chr120J_center','win']]){
    if(requested.size&&!requested.has(key))continue;
    visited.add(key);
    const source=catalog.sources[key],bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject)),clips=modelClips(gltf,source.metadata),idle=clips.find(c=>c.name==='idle'),clip=clips.find(c=>c.name===role),raw=gltf.animations.find(c=>c.name===role);
    const template=new THREE.AnimationMixer(gltf.scene);template.clipAction(idle).play();template.update(0);gltf.scene.updateMatrixWorld(true);
    const body=gltf.scene.getObjectByName(bodyName),origin=body.getWorldPosition(new THREE.Vector3()),box=new THREE.Box3().setFromObject(gltf.scene),height=box.max.y-box.min.y;
    if(key==='drg:Valkyrimon'){
      // Its bird is a separate animated actor, not a body-growth defect. The
      // corrected body root must not parent it; all its tracks are checked below.
      const bird=gltf.scene.getObjectByName('bird_center');assert.ok(bird,'Valkyrimon bird actor retained');
      for(let parent=bird;parent;parent=parent.parent)assert.notEqual(parent,body,'Valkyrimon bird trajectory remains independent of body-root correction');
    }
    const rawTrack=raw.tracks.find(t=>t.name===bodyName+'.position'),rawPositions=[];
    for(let i=0;i<rawTrack.values.length;i+=3)rawPositions.push(new THREE.Vector3().fromArray(rawTrack.values,i).applyMatrix4(body.parent.matrixWorld));
    const rawYs=rawPositions.map(p=>p.y);
    if(terminalRoles[source.metadata.uid]){
      // Independent binding evidence: this is the entire creature root, not a
      // thrown weapon/effect. Puppetmon's partial root is deliberately excluded.
      const descendants=new Set();body.traverse(node=>descendants.add(node));let total=0,covered=0;
      gltf.scene.traverse(mesh=>{
        if(!mesh.isSkinnedMesh)return;
        const indices=mesh.geometry.attributes.skinIndex,weights=mesh.geometry.attributes.skinWeight;
        for(let i=0;i<indices.count;i++)for(let j=0;j<4;j++){
          const weight=weights.getComponent(i,j);total+=weight;
          if(descendants.has(mesh.skeleton.bones[indices.getComponent(i,j)]))covered+=weight;
        }
      });
      assert.ok(total>0&&Math.abs(covered/total-1)<1e-6,key+' J_center must cover the complete weighted body');
      const excursion=Math.max(...rawPositions.map(p=>p.distanceTo(origin)))/height;
      assert.ok(excursion>1,key+' red source must retain the original whole-body departure');
      // Untargeted roles must keep the exact authored clip, including duration,
      // interpolation, key times and values; no generalized action suppression.
      for(const original of gltf.animations){
        if(terminalRoles[source.metadata.uid].includes(original.name))continue;
        const retained=clips.find(c=>c.name===original.name);
        assert.ok(retained,key+' retains '+original.name);
        assert.deepEqual(THREE.AnimationClip.toJSON(retained),THREE.AnimationClip.toJSON(original),key+' untouched role '+original.name);
      }
      console.log('Original complete-body departure: '+key+' '+role+' = '+excursion+' idle heights; weighted subtree='+covered/total);
    }
    if(['rip-durandamon','rip-kimeramon'].includes(source.metadata.uid)){
      const excursion=Math.max(...rawYs.map(y=>Math.abs(y-origin.y)))/height;
      assert.ok(excursion>1,key+' regression must retain the original greater-than-one-height body-root excursion');
      console.log('Original body-root excursion: '+key+' '+role+' = '+excursion+' idle heights.');
    }
    if(['rip-cs_grademon','rip-cs_gryphonmon','rip-l_gumdramon'].includes(source.metadata.uid)){
      const excursion=Math.max(...rawPositions.map(p=>Math.hypot(p.x-origin.x,p.z-origin.z)))/height;
      assert.ok(excursion>1,key+' regression must retain the original greater-than-one-height horizontal body travel');
      console.log('Original horizontal body-root travel: '+key+' '+role+' = '+excursion+' idle heights.');
    }
    // UnitView constructs its mixer from the already assembled idle template;
    // channels absent from the action keep those idle transforms (e.g. Betamon scale).
    const mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();let motion=0;
    for(let i=0;i<=64;i++){
      mixer.setTime(clip.duration*i/64);gltf.scene.updateMatrixWorld(true);const p=body.getWorldPosition(new THREE.Vector3()).sub(origin);
      if(terminalRoles[source.metadata.uid]){
        gltf.scene.traverse(node=>{if(node.isSkinnedMesh)node.skeleton.update();});
        const skinned=new THREE.Box3().setFromObject(gltf.scene,true);
        assert.ok([...skinned.min.toArray(),...skinned.max.toArray()].every(Number.isFinite)&&!skinned.isEmpty(),key+' finite complete skinned geometry at '+i/64);
      }
      assert.ok(Math.hypot(p.x,p.z)<height*1e-5,`${key} ${role} root leaves simulated hex by ${Math.hypot(p.x,p.z)/height} body heights at ${i/64}`);
      // ZubaEagermon's preserved somersault needs posed-body floor clearance;
      // the independent dense skin test checks that exception and its geometry.
      const ceiling=source.metadata.uid==='rip-zubaeagermon'&&role==='special'?1.2:.351;
      assert.ok(p.y<=height*ceiling&&p.y>=-height*.081,`${key} ${role} root leaves bounded jump envelope: ${p.y/height}`);motion=Math.max(motion,Math.abs(p.y));
      if(i===64||role==='win'&&i===0)assert.ok(p.length()<height*1e-5,key+' '+role+' must start and finish at its simulated position; drift='+p.length()/height);
    }
    if(Math.max(...rawYs)-Math.min(...rawYs)>height*.005)assert.ok(motion>height*.005,key+' must retain visible authored vertical movement');
    // Wizardmon also has the separately tested 1/15x body-scale repair.
    // Turuiemon's traveling J_root parent is held at idle in the anchored special.
    for(const track of raw.tracks){if(track.name===bodyName+'.position'||key==='candlemon:1'&&track.name==='GRP_joint_03.scale'||key==='lopmon:1'&&track.name==='J_root.position')continue;const retained=clip.tracks.find(t=>t.name===track.name);assert.deepEqual(Array.from(retained.times),Array.from(track.times));assert.deepEqual(Array.from(retained.values),Array.from(track.values));}
    console.log('Root-travel regression passed: '+key+'; 65 whole-cycle samples, vertical action retained, other authored tracks preserved.');
  }
  for(const key of requested)assert.ok(visited.has(key),'Unknown root-travel regression source: '+key);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
