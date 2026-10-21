// CPU regression for the reviewed TS imports. Exercises the actual modelClips
// pipeline and skinned geometry; it neither regenerates content nor copies the
// animation implementation. Run: node native/tools/test-model-fallback-anatomy.cjs
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),roles=['idle','attack','special','win'];
const cases=[
  ['Impmon','081'],['Keramon','626'],['Koromon','322'],['Kudamon','053'],['Kuramon','629'],
  ['Lalamon','056'],['Lucemon','390'],['Lunamon','002'],['Mochimon','388'],['Monodramon','458'],
].map(([name,code])=>({name,key:`catalog:TS_${name}:chr${code}`,uid:`rip-cat_ts_${name.toLowerCase()}_chr${code}`})).concat([
  // Clipless imports whose generic victory raised both straight arms into a T.
  ['RapidmonArmor','giz:RapidmonArmor','rip-cs_rapidmon_armor'],
  ['OmnimonMerciful','unique:L_Omnimon_Merciful_Mode','rip-l_omnimon_merciful_mode'],
  // Ears and coat hide its arms: the generic clips rendered four identical frames.
  ['TerriermonAssistant','unique:TS_Terriermon_Assistant','rip-ts_terriermon_assistant'],
].map(([name,key,uid])=>({name,key,uid})));
// These three still failed the native action review despite nonzero quaternion
// tracks. Measure the imported shoulder/elbow/wrist landmarks: an axial twist
// can change a forearm quaternion without producing an actual elbow bend.
const actionLandmarks={
  'rip-cs_rapidmon_armor': {base:'J_center',arms:[['J_arm_l','J_elbow_l','J_weapon_l'],['J_arm_r','J_elbow_r','J_weapon_r']]},
  'rip-l_omnimon_merciful_mode': {base:'J_center',arms:[['J_arm_l','J_elbow_l','J_hand_l'],['J_arm_r','J_elbow_r','J_hand_r']]},
  'rip-ts_terriermon_assistant': {base:'J_center',arms:[['J_arm_l','J_elbow_l','J_hand_l'],['J_arm_r','J_elbow_r','J_hand_r']]},
  'rip-cat_ts_impmon_chr081': {base:'J_center',arms:[['J_arm_l','J_ancon_l','J_hand_l'],['J_arm_r','J_ancon_r','J_hand_r']]},
  'rip-cat_ts_lucemon_chr390': {base:'J_center',arms:[['J_shoulderL','J_elbowL','J_wristL'],['J_shoulderR','J_elbowR','J_wristR']]},
  'rip-cat_ts_monodramon_chr458': {base:'J_center',arms:[['J_arm_l','J_elbow_l','J_hand_l'],['J_arm_r','J_elbow_r','J_hand_r']]},
};
let checks=0;
const failures=[],check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
const sha=value=>crypto.createHash('sha256').update(value).digest('hex');
const bytes=a=>Buffer.from(a.buffer,a.byteOffset,a.byteLength);
const clipHash=c=>sha(JSON.stringify({name:c.name,duration:c.duration,tracks:c.tracks.map(t=>({name:t.name,type:t.ValueTypeName,interpolation:t.getInterpolation(),times:sha(bytes(t.times)),values:sha(bytes(t.values))}))}));
const round=n=>Number(n.toFixed(7));
const rms=(a,b,span,indices)=>{
  let sum=0,count=0;for(const i of indices||a.keys()){sum+=a[i].distanceToSquared(b[i]);count++;}
  return count?Math.sqrt(sum/count)/span:0;
};
function immutableState(gltf){
  const meshes=[];gltf.scene.traverse(o=>{if(!o.isMesh)return;
    const attributes=Object.entries(o.geometry.attributes).map(([name,a])=>[name,a.itemSize,a.normalized,sha(bytes(a.array))]);
    const morph=Object.entries(o.geometry.morphAttributes).map(([name,list])=>[name,list.map(a=>sha(bytes(a.array)))]);
    meshes.push({name:o.name,attributes,morph,index:o.geometry.index&&sha(bytes(o.geometry.index.array)),
      inverseBinds:o.isSkinnedMesh?o.skeleton.boneInverses.map(m=>m.toArray()):[],bind:o.bindMatrix?.toArray(),inverse:o.bindMatrixInverse?.toArray()});
  });return sha(JSON.stringify({meshes,clips:gltf.animations.map(clipHash)}));
}
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const rawSource=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8');
  const {modelClips}=await require('./load-model-motion.cjs')();
  async function load(key){
    const entry=catalog.sources[key];if(!entry)throw Error('Missing source '+key);
    const packed=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[entry.asset].file)));
    const length=packed.readUInt32LE(12),json=JSON.parse(packed.subarray(20,20+length)),binary=packed.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    return {gltf:await new Promise((resolve,reject)=>parser.parse(resolve,reject)),entry};
  }
  function prepare(gltf,clips,uid){
    // Match template preparation: the idle opening pose supplies unkeyed values.
    const template=new THREE.AnimationMixer(gltf.scene);template.clipAction(clips.find(c=>c.name==='idle')).play();template.update(0);
    const rest=[],meshes=[];gltf.scene.traverse(o=>{rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone()});if(o.isSkinnedMesh)meshes.push(o);});
    const mixer=new THREE.AnimationMixer(gltf.scene),bones=rest.filter(r=>r.o.isBone),weighted=new Map();
    const influences=[];let offset=0;const indices=[];
    for(const mesh of meshes){
      const geometry=mesh.geometry,j=geometry.attributes.skinIndex,w=geometry.attributes.skinWeight;
      for(let i=0;i<j.count;i++){
        const names=new Map();for(let k=0;k<4;k++){const weight=w.getComponent(i,k);if(weight<=0)continue;const bone=mesh.skeleton.bones[j.getComponent(i,k)];names.set(bone.name,(names.get(bone.name)||0)+weight);weighted.set(bone.name,(weighted.get(bone.name)||0)+weight);}
        influences.push(names);
      }
      const ix=geometry.index?Array.from(geometry.index.array):Array.from({length:j.count},(_,i)=>i);
      for(let i=0;i<ix.length;i+=3)indices.push([ix[i]+offset,ix[i+1]+offset,ix[i+2]+offset]);offset+=j.count;
    }
    const landmarks=actionLandmarks[uid];
    function anatomicalPose(){
      if(!landmarks)return null;
      const position=name=>{
        const bone=gltf.scene.getObjectByName(name);
        if(!bone?.isBone)throw Error(uid+': missing reviewed anatomical landmark '+name);
        return bone.getWorldPosition(new THREE.Vector3());
      };
      const arms=landmarks.arms.map(names=>{
        const [shoulder,elbow,wrist]=names.map(position);
        const upper=elbow.clone().sub(shoulder).normalize(),lower=wrist.clone().sub(elbow).normalize();
        return {shoulder,lower,elbowFlex:upper.angleTo(lower)};
      });
      // Build a torso frame from actual bone positions, not a rig's local axis
      // convention. Shoulder width captures turning; base-to-shoulders captures
      // leaning. This measurement remains independent of the clip generator.
      const up=arms[0].shoulder.clone().add(arms[1].shoulder).multiplyScalar(.5).sub(position(landmarks.base)).normalize();
      const across=arms[0].shoulder.clone().sub(arms[1].shoulder);
      across.addScaledVector(up,-across.dot(up)).normalize();
      const forward=new THREE.Vector3().crossVectors(across,up).normalize();
      const torso=new THREE.Quaternion().setFromRotationMatrix(new THREE.Matrix4().makeBasis(across,up,forward)).normalize();
      return {elbowFlex:arms.map(a=>a.elbowFlex),forwardReach:arms.map(a=>a.lower.dot(forward)),torso};
    }
    return {meshes,bones,influences,indices,weighted,
      sample(clip,time){
        mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}
        const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);
        for(const mesh of meshes)mesh.skeleton.update();
        const points=[];for(const mesh of meshes)for(let i=0;i<mesh.geometry.attributes.position.count;i++)points.push(mesh.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(mesh.matrixWorld));
        return {points,quaternions:new Map(bones.map(r=>[r.o.name,r.o.quaternion.clone().normalize()])),anatomical:anatomicalPose(),
          transformsExact:rest.every(r=>r.o.position.equals(r.p)&&r.o.scale.equals(r.s)),
          finiteMatrices:rest.every(r=>r.o.matrixWorld.elements.every(Number.isFinite))};
      },
      stop(){mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);template.stopAllAction();template.uncacheRoot(gltf.scene);},
    };
  }
  const results=[],controls=[];
  for(const config of cases){
    const {name,key,uid}=config,{gltf,entry}=await load(key);
    check(entry.metadata.uid===uid,key+': exact source UID',entry.metadata.uid);
    check(gltf.animations.length===0,key+': reviewed input still lacks authored clips',gltf.animations.map(c=>c.name));
    const immutableBefore=immutableState(gltf),restBefore=[];gltf.scene.traverse(o=>restBefore.push({o,p:o.position.clone(),s:o.scale.clone(),parent:o.parent}));
    const clips=modelClips(gltf,entry.metadata);
    check(immutableState(gltf)===immutableBefore,key+': source arrays and inverse binds unchanged after preparation');
    check(restBefore.every(r=>r.o.position.equals(r.p)&&r.o.scale.equals(r.s)&&r.o.parent===r.parent),key+': rest offsets, scales and hierarchy unchanged (arm rotations permitted)');
    check(roles.every(role=>clips.filter(c=>c.name===role).length===1),key+': exactly one clip for each required role');
    if(!roles.every(role=>clips.some(c=>c.name===role)))continue;
    for(const clip of clips){
      check(clip.duration>0&&Number.isFinite(clip.duration),key+': finite positive duration',clip.name);
      check(new Set(clip.tracks.map(t=>t.name)).size===clip.tracks.length,key+': no duplicate bindings',clip.name);
      check(clip.tracks.every(t=>t.name.endsWith('.quaternion')),key+': only rotation animation is generated',clip.name);
      check(clip.tracks.every(t=>Array.from(t.times).every(Number.isFinite)&&Array.from(t.values).every(Number.isFinite)),key+': finite animation keys',clip.name);
    }
    const rig=prepare(gltf,clips,uid),idle=clips.find(c=>c.name==='idle'),neutral=rig.sample(idle,0),box=new THREE.Box3().setFromPoints(neutral.points);
    const size=box.getSize(new THREE.Vector3()),span=Math.max(size.x,size.y,size.z),diameter=size.length();
    const regionNames={
      upper:/^J_ue_ago$/,lower:/^J_center$/,kuramonBody:/^J_(?:center|hip)$/,
      head:/^J_(?:head|atama|joint6)$/,ears:/^J_(?:ear|mimi)/,
      sprout:/^J_joint(?:17|7)$/,mochimonLower:/^J_ashi$/,
      spine:/^J_spine(?:\d+)?$/,tentacles:/^J_(?:paipu|shokushu)/,
    };
    const regions=Object.fromEntries(Object.entries(regionNames).map(([label,rx])=>[label,rig.influences.flatMap((weights,i)=>[...weights].reduce((n,[bone,w])=>n+(rx.test(bone)?w:0),0)>=.5?[i]:[])]));
    const edges=[],allEdges=[],seen=new Set();for(const ids of rig.indices)for(let i=0;i<3;i++){
      const a=Math.min(ids[i],ids[(i+1)%3]),b=Math.max(ids[i],ids[(i+1)%3]),id=a+':'+b;if(seen.has(id))continue;seen.add(id);
      const length=neutral.points[a].distanceTo(neutral.points[b]),edge={a,b,length};allEdges.push(edge);if(length>span*1e-5)edges.push(edge);
    }
    const roleResults=[],sampled=new Map();
    for(const role of roles){
      const clip=clips.find(c=>c.name===role),opening=rig.sample(clip,0),edgeMax=new Float64Array(edges.length),localMax=new Map();
      const stats={role,samples:65,peakRmsSpan:0,peakVertexSpan:0,maxBoundsRatio:0,maxEdgeStretch:0,maxEdgeGrowthSpan:0,edgeStretchP99:0,endpointSpan:0,startToIdleSpan:0,finite:true,transformsExact:true,regions:{}};
      if(opening.anatomical)stats.anatomical={elbowFlexMin:opening.anatomical.elbowFlex.slice(),elbowFlexMax:opening.anatomical.elbowFlex.slice(),forwardBendMax:[-1,-1],torsoSwing:0};
      for(let i=0;i<opening.points.length;i++)stats.startToIdleSpan=Math.max(stats.startToIdleSpan,opening.points[i].distanceTo(neutral.points[i])/span);
      const poses=[];
      for(let sample=0;sample<=64;sample++){
        const pose=rig.sample(clip,clip.duration*sample/64),points=pose.points;
        stats.finite&&=pose.finiteMatrices&&points.every(v=>[v.x,v.y,v.z].every(Number.isFinite));stats.transformsExact&&=pose.transformsExact;
        stats.peakRmsSpan=Math.max(stats.peakRmsSpan,rms(points,opening.points,span));
        stats.maxBoundsRatio=Math.max(stats.maxBoundsRatio,new THREE.Box3().setFromPoints(points).getSize(new THREE.Vector3()).length()/diameter);
        for(let i=0;i<points.length;i++){
          const movement=points[i].distanceTo(opening.points[i])/span;stats.peakVertexSpan=Math.max(stats.peakVertexSpan,movement);
          if(sample===64)stats.endpointSpan=Math.max(stats.endpointSpan,movement);
        }
        for(let i=0;i<edges.length;i++){const e=edges[i],ratio=points[e.a].distanceTo(points[e.b])/e.length;edgeMax[i]=Math.max(edgeMax[i],ratio);}
        for(const e of allEdges)stats.maxEdgeGrowthSpan=Math.max(stats.maxEdgeGrowthSpan,(points[e.a].distanceTo(points[e.b])-e.length)/span);
        for(const [bone,q] of pose.quaternions)localMax.set(bone,Math.max(localMax.get(bone)||0,opening.quaternions.get(bone).angleTo(q)));
        if(pose.anatomical){
          stats.anatomical.torsoSwing=Math.max(stats.anatomical.torsoSwing,opening.anatomical.torso.angleTo(pose.anatomical.torso));
          pose.anatomical.elbowFlex.forEach((angle,side)=>{
            stats.anatomical.elbowFlexMin[side]=Math.min(stats.anatomical.elbowFlexMin[side],angle);
            stats.anatomical.elbowFlexMax[side]=Math.max(stats.anatomical.elbowFlexMax[side],angle);
            if(angle>=.35)stats.anatomical.forwardBendMax[side]=Math.max(stats.anatomical.forwardBendMax[side],pose.anatomical.forwardReach[side]);
          });
        }
        for(const [label,ids] of Object.entries(regions))if(ids.length)stats.regions[label]=Math.max(stats.regions[label]||0,rms(points,opening.points,span,ids));
        if(sample===16||sample===32||sample===48)poses.push(points);
      }
      sampled.set(role,poses);
      let worst=0;for(let i=1;i<edgeMax.length;i++)if(edgeMax[i]>edgeMax[worst])worst=i;
      const edge=edges[worst];stats.worstEdge={restLengthSpan:edge.length/span,ratio:edgeMax[worst],vertices:[edge.a,edge.b],weights:[Object.fromEntries(rig.influences[edge.a]),Object.fromEntries(rig.influences[edge.b])]};
      // Ratios on nearly collapsed seam edges are unstable. Keep their raw
      // maximum in the report, enforce 2x on edges >=0.1% of model span, and
      // bound absolute growth on every edge so a small seam cannot form a spike.
      stats.ordinaryEdgeMaxStretch=1;
      for(let i=0;i<edges.length;i++){
        if(edges[i].length>=span*.001)stats.ordinaryEdgeMaxStretch=Math.max(stats.ordinaryEdgeMaxStretch,edgeMax[i]);
      }
      stats.edgesOver2=Array.from(edgeMax).filter(x=>x>2).length;
      edgeMax.sort();stats.maxEdgeStretch=edgeMax.at(-1)||1;stats.edgeStretchP99=edgeMax[Math.floor((edgeMax.length-1)*.99)]||1;
      stats.localDegrees=Object.fromEntries([...localMax].filter(([,angle])=>angle>1e-6).map(([bone,angle])=>[bone,round(angle*180/Math.PI)]));
      check(stats.finite,key+': finite sampled vertices and matrices',role);
      check(stats.transformsExact,key+': sampled local translations and scales stay exact',role);
      check(stats.maxBoundsRatio<=2,key+': no giant bounds growth',{role,ratio:stats.maxBoundsRatio});
      check(stats.ordinaryEdgeMaxStretch<=2,key+': ordinary connected edges do not stretch beyond 2x',{role,max:stats.ordinaryEdgeMaxStretch,allEdgesMax:stats.maxEdgeStretch,p99:stats.edgeStretchP99});
      check(stats.maxEdgeGrowthSpan<=.15,key+': small edges cannot form large spikes',{role,growth:stats.maxEdgeGrowthSpan});
      check(stats.endpointSpan<1e-5,key+': role has closed endpoints',{role,error:stats.endpointSpan});
      check(stats.startToIdleSpan<1e-5,key+': role starts at common idle pose',{role,error:stats.startToIdleSpan});
      check(stats.peakRmsSpan>(role==='idle'?.001:.005),key+': effective weighted motion',{role,rms:stats.peakRmsSpan});
      if(stats.anatomical&&(role==='attack'||role==='win')){
        // This is a minimum source-motion regression, not a visual acceptance
        // test. The leading attack arm or both celebrating arms must visibly
        // flex; a permanently bent but locally twisting arm cannot satisfy it.
        const sides=role==='attack'?[0]:[0,1],motion=stats.anatomical;
        check(sides.every(side=>motion.elbowFlexMax[side]>=.4&&motion.elbowFlexMax[side]-motion.elbowFlexMin[side]>=.15),key+': action has actual anatomical elbow flex',{role,...motion});
        check(motion.torsoSwing>=.18,key+': action has anatomical torso anticipation or celebration',{role,radians:motion.torsoSwing});
        if(name==='Monodramon')check(sides.every(side=>motion.forwardBendMax[side]>=.3),key+': bent forearms reach forward instead of recreating a lateral T silhouette',{role,forwardBendMax:motion.forwardBendMax});
      }
      const angle=bone=>localMax.get(bone)||0,any=names=>names.some(b=>angle(b)>.035),region=(label,minimum=.005)=>check((stats.regions[label]||0)>minimum,key+': '+label+' skin region moves',{role,rms:stats.regions[label]||0});
      if(role!=='idle'){
        if(name==='Kuramon')region('kuramonBody');
        if(name==='Koromon'){region('upper');region('lower');check(any(['J_mimiL','J_mimiR']),key+': ears articulate locally',role);}
        if(name==='Kudamon'){
          for(const [label,names] of [['proximal',['J_spine','J_spine1','J_spine2','J_spine3']],['middle',['J_spine4','J_spine5','J_spine6','J_spine7','J_spine8']],['distal',['J_spine9','J_spine10','J_spine11','J_spine12','J_spine13']]])
            check(any(names),key+': '+label+' spine bends relative to parent',role);
        }
        if(name==='Lalamon'){region('head');region('sprout');check(any(['J_joint6','J_joint17','J_joint7']),key+': head or sprout articulates locally',role);}
        if(name==='Lunamon'){region('ears');check(any(['J_earA01_l','J_earA01_r','J_earB01_l','J_earB01_r']),key+': ears articulate locally',role);}
        if(name==='Impmon')check(any(['J_ancon_l','J_ancon_r']),key+': elbows articulate beyond shoulder swing',role);
        if(name==='Keramon')check([...localMax].filter(([bone,a])=>/^J_(?:paipu|shokushu)/.test(bone)&&a>.035).length>=2,key+': multiple tentacle joints articulate',role);
        if(name==='Lucemon')check([...localMax].filter(([bone,a])=>/^J_wing/.test(bone)&&a>.035).length>=2,key+': multiple wing joints articulate',role);
        if(name==='Mochimon')region('mochimonLower');
        if(name==='Monodramon')check(any(['J_elbow_l','J_elbow_r','J_jaw','J_tail02','J_tail03','J_wing01_l','J_wing01_r']),key+': anatomy articulates beyond shoulder swing',role);
      }
      roleResults.push(stats);
    }
    const differences=[];for(let a=0;a<roles.length;a++)for(let b=a+1;b<roles.length;b++){
      const difference=Math.max(...sampled.get(roles[a]).map((p,i)=>rms(p,sampled.get(roles[b])[i],span)));
      check(difference>.0025,key+': roles have distinct skinned poses',{a:roles[a],b:roles[b],rms:difference});differences.push({a:roles[a],b:roles[b],rms:round(difference)});
    }
    rig.stop();check(immutableState(gltf)===immutableBefore,key+': source arrays and inverse binds unchanged after sampling');
    results.push({key,uid,asset:entry.asset,vertices:neutral.points.length,weightedJoints:rig.weighted.size,restSpan:span,roles:roleResults,differences});
    // Same source geometry with nonmatching UID must retain the frozen generic
    // clip hashes. This catches accidental global regex/amplitude changes.
    const control=await load(key),controlClips=modelClips(control.gltf,{...entry.metadata,uid:uid+'-unrelated-control'}),hashes=controlClips.map(clipHash);
    controls.push({key,hashes});
    check(JSON.stringify(hashes)===JSON.stringify(controlHashes[name]),key+': nonmatching UID retains existing generic fallback');
  }
  // An unrelated authored model must keep its original clips byte for byte.
  const authored=await load('agumon:0'),authoredBefore=immutableState(authored.gltf),authoredHashes=authored.gltf.animations.map(clipHash),authoredAfter=modelClips(authored.gltf,authored.entry.metadata);
  check(authoredHashes.length===4,'authored control has four original roles');
  check(JSON.stringify(authoredAfter.map(clipHash))===JSON.stringify(authoredHashes),'authored control retains original clip content');
  check(immutableState(authored.gltf)===authoredBefore,'authored control input arrays unchanged');
  const report={sourceSha256:sha(rawSource),anatomySha256:sha(fs.readFileSync(path.join(root,'js/model-anatomy.js'))),sampleFractions:65,checks,results,controls,failures};
  const output=path.join(root,'native/artifacts/model-fallback-anatomy-regression.json');fs.mkdirSync(path.dirname(output),{recursive:true});fs.writeFileSync(output,JSON.stringify(report,null,2)+'\n');
  console.log(`${cases.length} TS anatomy regressions: ${checks} checks, ${failures.length} failures; report ${path.relative(root,output)}`);
  for(const failure of failures)console.log('FAIL '+failure.label+' '+JSON.stringify(failure.detail??''));
  if(failures.length)process.exitCode=1;
}
// Frozen on model-motion.js SHA256
// 799ffce0405288d668646594908594e82971b6d0efac4d3efb0e3db3c34da113.
// Never update these as part of an exact-UID animation repair.
const controlHashes={
  "Impmon": [
    "6350ffcca653b41c9939daea5c34f8d80691fcdcfe8a538de077479424a2bd5b",
    "0a018d803065d6fe30c1e04953f611efaf7fc74d3016bda108ea16b587d39d3c",
    "8800702d45d00e2c469f97a68bf3215e879b86710ca0057cffb496afd858a7bc",
    "48ddd833b0e3084e20e0b22a41fb1467cacdc4198d5fb2df4d84abbe79cf9e37"
  ],
  "Keramon": [
    "1379fa745f34f8e6424505c46d125cba1f54f0d0018952e2e575be34772cd413",
    "34ebb534a9c6a10792df8f4793c85c600dba27bf64bc5207098d9a65ad439bbc",
    "9593f2c239f6be2f575a2da4adc747872b211b84773b4f779046e304435efd9c",
    "9747079677d6fba8f2a50ef91916693fec1e96b65bd3690a401a063e57321786"
  ],
  "Koromon": [
    "1001b2744cce882e40b612340cae5980d18cbff7e06d0e79f0ef38b410f6b5b3",
    "fbd0703e4fe6e82808ee81ecbd9c65101014778a19fadf9ee91bee341c678458",
    "5dd5146ae996de53ffc9237fcd7694ebe7030385f0d5857cd3b3c632ecb560bc",
    "f2086c34253f13a39184da5108569ea723b42848ea1883530da010b23435b0bf"
  ],
  "Kudamon": [
    "413cdcd65a5aaf5e76ab1113d8baa5e25edbeb190e787282a06d9b5eaf789f06",
    "19fc8f4d7d2596cac96b3ee08fa85bff8ff1c25bdc28df2ae5f5fb26b40d0534",
    "0e0d28b21da0112264b5cbe4c65d1647c3ad664b17780ca7793a22cc14ccdd82",
    "6961c0ee37d51f00249a90712402bc80cdd412bbbc471dd99c20bbcc174fb4e4"
  ],
  "Kuramon": [
    "9abd52a4f33cd04d02b54335f589145140ae99b88f426e17401ebb3a0be02242",
    "732ba240b0ced13f542e5f6e4894b9f0cddc8f111fa98545bc8108640b69d6a1",
    "439747a8f7f3bd3c098b7527ab1251b59494856a816c974dbe586b5077a12b66",
    "1c8237195f955a0175f5f5f4614bed169960c0a0b436925d679f20df480f61fd"
  ],
  "Lalamon": [
    "08f405894af8428dcad0f8c47d8eb6d2f514f6f600c44483672dad36bf67fe3c",
    "393c23c8ba342a7c601b64b4bb77ef7b054f6ef0e1928e7ee85b54a2953c74d0",
    "610cc6352afe67112a884e410bb6d4b9dab0d04a69d3fee66813949dee5f7668",
    "d9eb98d11ec696635a5937100da24df084577cf76a3e0f192a784ba8c7de4ccf"
  ],
  "Lucemon": [
    "0fe140fcb3dd595ece7e02615c554d41cfa281cade87312f85cc44af519f1fbf",
    "df9f038df138bd21578c2c5492bbdd70e5a44e270e7bf7b3de8056d02d320079",
    "171b0d9eeb4aaf1ef7ff82feaa5d5e4b19b76e08e09dec0b7fd456450f130516",
    "e299355d91decb02caadb7eae9f986566f6cb86fe7abc4a4f36b4bd7835a3f8d"
  ],
  "Lunamon": [
    "e80f3d8b7a217b652fa4ee96d485a7dc6a3a92c5ec566022ac2d2c87e3bcf64c",
    "efc41c1dcc3ddb869fd2474fa0f646f3965eec9ba87725a075d290bdf1afd329",
    "6b5e688fdb107302cbbb406e01ebc6f04437c90ca80777ba3e6a8b38bdb27b59",
    "b8ac9d25ceb5e842c1fe6e987a4cdc8110f80882dbd7a2ef0983c77daf246070"
  ],
  "Mochimon": [
    "fad028271205ee4e696abe0aa9cd35893406ca2a7a89437f56708320272e115b",
    "1b0663f3664d989308a6c52f38480c9a238d937dd156c1d3f233d086096999e5",
    "6845dce49eba7e277f8c2faaf711cdc603ee8318020841612a4400b35f2d8695",
    "ca3030b40f6192bcf815318920c464c05fc400df90676dd0b84157e2a2082962"
  ],
  "Monodramon": [
    "544bbed6d3d1e75cac7d486b27ad47186ebb7a02d867625a84b0769c3092decc",
    "73a3cba1f352f4da87d5bb90e9486978199367c57aac7ab641ae8748584d6930",
    "2f47e75cb064f52401adc42db3c5812f9cd859fb329938d884046cb6f98449db",
    "7c84de700049a83b07a6f84018ce0bf93188d75b511acf3b07e18a6f3325bf72"
  ],
  "RapidmonArmor": [
    "b931c160230ab4137869add23e35fbaea91492ee63cba34f969ff92443e09e60",
    "877966428559ded5101e12cc02ac77abcfcd9993139c967c41e09d95c701e7a1",
    "0a7662d89ec80698a45e66b97e26eaf28bed31eebe823f0718b372f766a139bc",
    "45895dab401741cc64ab989c29223f2cbfd2058d6c6c1ec48bf80d3f71c38c55"
  ],
  "OmnimonMerciful": [
    "f70d73c442ac841feaafdf78ce99e081690cd022e249170840a9fabae200e813",
    "44ae536bc184076b74e9585e0341ce6684aa94e0bd8e1d37494d07c2dd9cbbce",
    "d2fd8ef85b6d5db63d46f86905dc1b8b38faf71753e3183f85e9b3d5d037421c",
    "5d0b1ecbf27b80965b34922d31dcbe500833484b6f9b45ff1244ebfd3ed709e6"
  ],
  "TerriermonAssistant": [
    "d1b3544faa8636b033349757b61f9765db5e5a128d3adb3d34ab8af1cc0064d9",
    "b1dfe255e793edcc650f6da74cd467963c8ebc77716309575dce60471fd7840b",
    "4b9644ceac9bbbb7e59c9c6d025db61156ccc6b704e2677f8444f66cc0c67f68",
    "e88416208ae8b886c7e6e32c8bd46d8497a9418a3c4aa5a01777eb55974dc671"
  ]
};
main().catch(error=>{console.error(error);process.exitCode=1;});
