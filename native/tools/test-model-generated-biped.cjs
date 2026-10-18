// Regression for two generated RD biped rigs. Uses actual modelClips and skin;
// anatomical directions come from rig landmarks, never replacement clip math.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),roles=['idle','attack','special','win'];
const cases=[['RD_Beelzemon','rip-cat_rd_beelzemon_main'],['RD_WarGrowlmon','rip-cat_rd_wargrowlmon_main']].map(([name,uid])=>({key:`catalog:${name}:main`,uid}));
const failures=[];let checks=0;
const check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
const sha=b=>crypto.createHash('sha256').update(b).digest('hex'),bytes=a=>Buffer.from(a.buffer,a.byteOffset,a.byteLength);
const clipHash=c=>sha(JSON.stringify({name:c.name,duration:c.duration,tracks:c.tracks.map(t=>({name:t.name,type:t.ValueTypeName,interpolation:t.getInterpolation(),times:sha(bytes(t.times)),values:sha(bytes(t.values))}))}));
function immutable(gltf){
  const meshes=[];gltf.scene.traverse(o=>{if(o.isMesh)meshes.push({name:o.name,attributes:Object.entries(o.geometry.attributes).map(([k,a])=>[k,a.itemSize,a.normalized,sha(bytes(a.array))]),morph:Object.entries(o.geometry.morphAttributes).map(([k,v])=>[k,v.map(a=>sha(bytes(a.array)))]),indices:o.geometry.index&&sha(bytes(o.geometry.index.array)),inverseBinds:o.skeleton?.boneInverses.map(m=>m.toArray()),bind:o.bindMatrix?.toArray()});});
  return sha(JSON.stringify({meshes,clips:gltf.animations.map(clipHash)}));
}
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),presets=JSON.parse(fs.readFileSync(path.join(root,'tools/convert/catalog-rigs.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
  const load=async key=>{
    const entry=catalog.sources[key],packed=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[entry.asset].file)));
    const length=packed.readUInt32LE(12),json=JSON.parse(packed.subarray(20,20+length)),binary=packed.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    return {entry,gltf:await new Promise((resolve,reject)=>parser.parse(resolve,reject))};
  };
  function prepare(gltf,clips,spec){
    // As in prepareTemplate, leave assembled idle in place for the fresh mixer.
    const template=new THREE.AnimationMixer(gltf.scene);template.clipAction(clips.find(c=>c.name==='idle')).play();template.update(0);
    const rest=[],meshes=[];gltf.scene.traverse(o=>{rest.push({o,p:o.position.clone(),s:o.scale.clone(),q:o.quaternion.clone(),parent:o.parent});if(o.isSkinnedMesh)meshes.push(o);});
    const mixer=new THREE.AnimationMixer(gltf.scene),chest=gltf.scene.getObjectByName('chest'),rigRoot=gltf.scene.getObjectByName('DT_Rig');
    const armInfo=['L','R'].map(side=>{
      const arm=gltf.scene.getObjectByName('arm_'+side),forearm=gltf.scene.getObjectByName('forearm_'+side),entry=spec.bones.find(b=>b[0]==='forearm_'+side);
      // The generated forearm has no wrist child; its imported authoring endpoint
      // is an anatomical landmark in that bone's rest-local coordinates.
      return {side,arm,forearm,tip:new THREE.Vector3(...entry[2]).sub(new THREE.Vector3(...entry[1]))};
    });
    const indices=[],weights=[];let offset=0;
    for(const mesh of meshes){const g=mesh.geometry,j=g.attributes.skinIndex,w=g.attributes.skinWeight;
      for(let i=0;i<j.count;i++){let arm=0;for(let k=0;k<4;k++)if(/^(?:arm|forearm)_[LR]$/.test(mesh.skeleton.bones[j.getComponent(i,k)].name))arm+=w.getComponent(i,k);weights.push(arm);}
      const ix=g.index?Array.from(g.index.array):Array.from({length:j.count},(_,i)=>i);for(let i=0;i<ix.length;i+=3)indices.push(ix.slice(i,i+3).map(v=>v+offset));offset+=j.count;
    }
    return {indices,armVertices:weights.flatMap((w,i)=>w>=.5?[i]:[]),sample(clip,time){
      mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}
      const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);for(const m of meshes)m.skeleton.update();
      const points=[];for(const m of meshes)for(let i=0;i<m.geometry.attributes.position.count;i++)points.push(m.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(m.matrixWorld));
      const torso=chest.getWorldQuaternion(new THREE.Quaternion()).normalize(),torsoInverse=torso.clone().invert();
      const arms=armInfo.map(({side,arm,forearm,tip})=>{
        const shoulder=arm.getWorldPosition(new THREE.Vector3()),elbow=forearm.getWorldPosition(new THREE.Vector3()),wrist=tip.clone().applyMatrix4(forearm.matrixWorld);
        const upper=elbow.clone().sub(shoulder).normalize().applyQuaternion(torsoInverse),lower=wrist.sub(elbow).normalize().applyQuaternion(torsoInverse);
        return {side,upper:upper.toArray(),lower:lower.toArray(),elbowFlex:upper.angleTo(lower),shoulderEuler:new THREE.Euler().setFromQuaternion(arm.quaternion,'XYZ').toArray().slice(0,3)};
      });
      return {points,arms,torso,rootPosition:rigRoot.getWorldPosition(new THREE.Vector3()),
        offsetsAndScalesExact:rest.every(r=>r.o.scale.equals(r.s)&&r.o.parent===r.parent&&(r.o.name==='root'||r.o.name==='DT_Rig'||r.o.position.equals(r.p))),
        finite:rest.every(r=>r.o.matrixWorld.elements.every(Number.isFinite))&&points.every(p=>p.toArray().every(Number.isFinite))};
    }};
  }
  const rms=(a,b,span,indices)=>{let sum=0,count=0;for(const i of indices||a.keys()){sum+=a[i].distanceToSquared(b[i]);count++;}return Math.sqrt(sum/count)/span;};
  const tArm=a=>Math.abs(a.upper[1])<.18&&Math.abs(a.lower[1])<.18&&Math.abs(a.upper[0])>.85&&Math.abs(a.lower[0])>.85&&a.elbowFlex<.25;
  const area=(p,ids)=>new THREE.Vector3().subVectors(p[ids[1]],p[ids[0]]).cross(new THREE.Vector3().subVectors(p[ids[2]],p[ids[0]])).length()/2;
  const results=[];
  for(const config of cases){
    const {key,uid}=config,{gltf,entry}=await load(key),spec=presets[path.basename(entry.asset)];
    check(entry.metadata.uid===uid,key+': exact UID',entry.metadata.uid);
    check(spec?.type==='biped'&&gltf.scene.getObjectByName('DT_Rig')?.userData.digitacticsRig===1,key+': reviewed project-generated biped');
    const before=immutable(gltf),rest=[];gltf.scene.traverse(o=>rest.push({o,p:o.position.clone(),s:o.scale.clone(),parent:o.parent}));
    const raw=await load(key),rawRig=prepare(raw.gltf,raw.gltf.animations,spec),rawIdle=rawRig.sample(raw.gltf.animations.find(c=>c.name==='idle'),0),rawWin=rawRig.sample(raw.gltf.animations.find(c=>c.name==='win'),raw.gltf.animations.find(c=>c.name==='win').duration/2);
    const baseline={neutralArms:rawIdle.arms,winMidpointArms:rawWin.arms,midpointBilateralT:rawWin.arms.every(tArm),neutralShoulderZ:rawIdle.arms.map(a=>a.shoulderEuler[2]),midpointShoulderZ:rawWin.arms.map(a=>a.shoulderEuler[2])};
    check(baseline.midpointBilateralT,key+': frozen raw source reproduces the horizontal T silhouette',baseline);
    // Packed quaternion quantization shifts .65 by about2e-5 radians.
    check(baseline.neutralShoulderZ.every(z=>Math.abs(Math.abs(z)-.65)<1e-4)&&baseline.midpointShoulderZ.every(z=>Math.abs(z)<1e-5),key+': raw win cancels neutral shoulder lowering',baseline);
    const clips=modelClips(gltf,entry.metadata);
    check(immutable(gltf)===before,key+': source mesh, weights, inverse binds and authored clips immutable');
    check(rest.every(r=>r.o.position.equals(r.p)&&r.o.scale.equals(r.s)&&r.o.parent===r.parent),key+': imported bone offsets/scales/hierarchy unchanged');
    check(clips.length===4&&roles.every(role=>clips.filter(c=>c.name===role).length===1),key+': four required distinct roles');
    const retainedTrackEvidence=[];
    for(const clip of clips){
      check(clip.duration>0&&Number.isFinite(clip.duration),key+': valid duration',clip.name);
      check(new Set(clip.tracks.map(t=>t.name)).size===clip.tracks.length,key+': unique animation bindings',clip.name);
      // The reviewed repair owns only these anatomical rotation bindings. Root,
      // tail, leg and every position/scale track must retain their authored bytes.
      const authored=gltf.animations.find(c=>c.name===clip.name);
      const retained=(authored?.tracks||[]).filter(t=>!/^(?:chest|head|arm_[LR]|forearm_[LR])\.quaternion$/.test(t.name));
      const changed=retained.filter(raw=>{
        const current=clip.tracks.find(t=>t.name===raw.name);
        return !current||current.ValueTypeName!==raw.ValueTypeName||current.getInterpolation()!==raw.getInterpolation()
          ||current.times.constructor!==raw.times.constructor||current.values.constructor!==raw.values.constructor
          ||!bytes(current.times).equals(bytes(raw.times))||!bytes(current.values).equals(bytes(raw.values));
      }).map(t=>t.name);
      check(changed.length===0,key+': every track outside repaired anatomy rotations remains byte-identical',{role:clip.name,protectedTracks:retained.map(t=>t.name),changed});
      retainedTrackEvidence.push({role:clip.name,protectedTracks:retained.map(t=>t.name),changed});
    }
    const rig=prepare(gltf,clips,spec),idle=clips.find(c=>c.name==='idle'),neutral=rig.sample(idle,0),size=new THREE.Box3().setFromPoints(neutral.points).getSize(new THREE.Vector3()),span=Math.max(size.x,size.y,size.z),diameter=size.length();
    const edges=[],seen=new Set();for(const ids of rig.indices)for(let i=0;i<3;i++){const a=Math.min(ids[i],ids[(i+1)%3]),b=Math.max(ids[i],ids[(i+1)%3]),id=a+':'+b;if(seen.has(id))continue;seen.add(id);edges.push({a,b,length:neutral.points[a].distanceTo(neutral.points[b])});}
    const triangles=rig.indices.map(ids=>({ids,area:area(neutral.points,ids)})).filter(t=>t.area>span*span*1e-8),roleResults=[],posesByRole=new Map();
    check(neutral.arms.every(a=>a.upper[1]<-.25),key+': neutral upper arms are visibly lowered',neutral.arms);
    check(neutral.arms.every(a=>a.elbowFlex>=.15),key+': neutral elbows have a readable bend',neutral.arms);
    for(const role of roles){
      const clip=clips.find(c=>c.name===role),keys=[...new Set([0,clip.duration,...clip.tracks.flatMap(t=>Array.from(t.times)),...Array.from({length:65},(_,i)=>clip.duration*i/64)])].filter(t=>t<=clip.duration).sort((a,b)=>a-b);
      const times=[...new Set([...keys,...keys.slice(1).map((t,i)=>(t+keys[i])/2)])].sort((a,b)=>a-b),opening=rig.sample(clip,0),areaMax=new Float64Array(triangles.length),stats={role,samples:times.length,finite:true,offsetsAndScalesExact:true,peakRmsSpan:0,armRmsSpan:0,torsoSwing:0,upperArmSwing:[0,0],elbowFlexMax:[0,0],bilateralTFrames:0,tFractions:[],rootTravelSpan:0,maxBoundsRatio:0,ordinaryEdgeMaxStretch:1,edgeGrowthSpan:0,triangleAreaP99:0,startToIdleSpan:0,startArmsToIdleSpan:0,endpointSpan:0};
      for(let i=0;i<opening.points.length;i++)stats.startToIdleSpan=Math.max(stats.startToIdleSpan,opening.points[i].distanceTo(neutral.points[i])/span);
      for(const i of rig.armVertices)stats.startArmsToIdleSpan=Math.max(stats.startArmsToIdleSpan,opening.points[i].distanceTo(neutral.points[i])/span);
      for(const time of times){
        const pose=rig.sample(clip,time);stats.finite&&=pose.finite;stats.offsetsAndScalesExact&&=pose.offsetsAndScalesExact;
        stats.peakRmsSpan=Math.max(stats.peakRmsSpan,rms(pose.points,opening.points,span));stats.armRmsSpan=Math.max(stats.armRmsSpan,rms(pose.points,opening.points,span,rig.armVertices));stats.torsoSwing=Math.max(stats.torsoSwing,opening.torso.angleTo(pose.torso));
        pose.arms.forEach((a,i)=>{stats.elbowFlexMax[i]=Math.max(stats.elbowFlexMax[i],a.elbowFlex);stats.upperArmSwing[i]=Math.max(stats.upperArmSwing[i],new THREE.Vector3(...a.upper).angleTo(new THREE.Vector3(...opening.arms[i].upper)));});
        if(pose.arms.every(tArm)){stats.bilateralTFrames++;stats.tFractions.push(time/clip.duration);}
        stats.rootTravelSpan=Math.max(stats.rootTravelSpan,pose.rootPosition.distanceTo(opening.rootPosition)/span);stats.maxBoundsRatio=Math.max(stats.maxBoundsRatio,new THREE.Box3().setFromPoints(pose.points).getSize(new THREE.Vector3()).length()/diameter);
        for(const edge of edges){const length=pose.points[edge.a].distanceTo(pose.points[edge.b]);if(edge.length>=span*.001)stats.ordinaryEdgeMaxStretch=Math.max(stats.ordinaryEdgeMaxStretch,length/edge.length);stats.edgeGrowthSpan=Math.max(stats.edgeGrowthSpan,(length-edge.length)/span);}
        triangles.forEach((t,i)=>{areaMax[i]=Math.max(areaMax[i],area(pose.points,t.ids)/t.area);});
        if(time===clip.duration)for(let i=0;i<pose.points.length;i++)stats.endpointSpan=Math.max(stats.endpointSpan,pose.points[i].distanceTo(opening.points[i])/span);
      }
      areaMax.sort();stats.triangleAreaP99=areaMax[Math.floor((areaMax.length-1)*.99)];
      posesByRole.set(role,[.25,.5,.75].map(u=>rig.sample(clip,clip.duration*u).points));
      check(stats.finite,key+': finite skinned geometry',role);check(stats.offsetsAndScalesExact,key+': bone offsets/scales remain unchanged during motion',role);
      check(stats.rootTravelSpan<=.15&&stats.maxBoundsRatio<=1.5,key+': bounded root motion and overall silhouette',stats);
      check(stats.ordinaryEdgeMaxStretch<=2&&stats.edgeGrowthSpan<=.15&&stats.triangleAreaP99<=3,key+': connected skin remains bounded without spikes',stats);
      // WarGrowlmon's existing tail loop starts at a different phase per role;
      // require a shared arm stance without broadening this repair to its tail.
      check(stats.startArmsToIdleSpan<1e-4&&stats.endpointSpan<1e-4,key+': common neutral arm pose and closed endpoints',stats);
      check(stats.peakRmsSpan>(role==='idle'?.001:.015),key+': readable effective skin movement',stats);
      if(role!=='idle'){
        check(stats.armRmsSpan>.025&&stats.upperArmSwing.every(a=>a>.12),key+': actions change arm silhouette rather than only axial twist',stats);
        check(stats.elbowFlexMax.every(a=>a>.35),key+': both elbows bend during the action',stats);
        check(stats.torsoSwing>.08,key+': readable torso participation',stats);
      }
      if(role==='win')check(stats.bilateralTFrames===0,key+': celebration avoids bilateral horizontal straight-arm silhouette for entire cycle',stats);
      roleResults.push(stats);
    }
    const differences=[];for(const [a,b] of [['attack','special'],['attack','win'],['special','win']]){const value=Math.max(...posesByRole.get(a).map((p,i)=>rms(p,posesByRole.get(b)[i],span,rig.armVertices)));check(value>.01,key+': action silhouettes are distinct',{a,b,armRmsSpan:value});differences.push({a,b,armRmsSpan:value});}
    check(immutable(gltf)===before,key+': sampling preserves mesh, skin weights and source clips');
    const control=await load(key),authoredHashes=control.gltf.animations.map(clipHash),controlClips=modelClips(control.gltf,{...entry.metadata,uid:uid+'-unrelated-control'});
    check(JSON.stringify(controlClips.map(clipHash))===JSON.stringify(authoredHashes),key+': nonmatching UID retains authored generated clips byte-for-byte');
    results.push({key,uid,asset:entry.asset,baseline,neutralArms:neutral.arms,span,roles:roleResults,differences,retainedTrackEvidence});
  }
  check(results.some(result=>result.retainedTrackEvidence.some(role=>role.protectedTracks.length>0)),'reviewed assets exercise preservation of authored tracks outside repaired anatomy');
  const other=await load('agumon:0'),otherBefore=immutable(other.gltf),otherHashes=other.gltf.animations.map(clipHash),otherClips=modelClips(other.gltf,other.entry.metadata);
  check(JSON.stringify(otherClips.map(clipHash))===JSON.stringify(otherHashes),'unrelated authored model clips unchanged');check(immutable(other.gltf)===otherBefore,'unrelated authored model geometry unchanged');
  const report={sourceSha256:sha(fs.readFileSync(path.join(root,'js/model-motion.js'))),anatomySha256:sha(fs.readFileSync(path.join(root,'js/model-anatomy.js'))),rigBuilderSha256:sha(fs.readFileSync(path.join(root,'tools/convert/rig-static.mjs'))),checks,results,failures};
  const output=path.join(root,'native/artifacts/model-generated-biped-regression.json');fs.writeFileSync(output,JSON.stringify(report,null,2)+'\n');
  console.log(`${cases.length} RD generated-biped regressions: ${checks} checks, ${failures.length} failures; report ${path.relative(root,output)}`);
  for(const failure of failures)console.log('FAIL '+failure.label+' '+JSON.stringify(failure.detail??''));
  if(failures.length)process.exitCode=1;
}
main().catch(error=>{console.error(error);process.exitCode=1;});
