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
    "0a574e7a153c87284e9b7102ad1254c223ddd6eda6c1af2e796312195a56a6a0",
    "e70de2ed208cd0d6c5712d85f4ff69c350b61f32364b062b2e49b4ea26c76cf6",
    "7fa8d61c52b38f6dd4faf094f1e9570e4524979e731d0cf566f21f978703a011",
    "8dbb7bfbec89683d3bfa0b32a5d0babad17b7dbd5dcad4686e9ffb165a9d37cd"
  ],
  "Keramon": [
    "1379fa745f34f8e6424505c46d125cba1f54f0d0018952e2e575be34772cd413",
    "66370931c92987ca46a5efa9cc6c1e2f48337831e84213292c80127d4d31fb8c",
    "51668f859b374c5bb4933961586af9bfe0d71078511a8bfb2da7486d2eae52e5",
    "2ea7184ea45e4523921502badfdd4d1bf453d87c113d54460fcc0710392cdc95"
  ],
  "Koromon": [
    "08f405894af8428dcad0f8c47d8eb6d2f514f6f600c44483672dad36bf67fe3c",
    "aafc6174d2c7ad0f929af00901b89636427d95c2dd17c030ecdda2967d17532e",
    "516f5030fbd068f78d9390e8e9a554ef159bb469e9353021e48ee03428e3f2a0",
    "d9eb98d11ec696635a5937100da24df084577cf76a3e0f192a784ba8c7de4ccf"
  ],
  "Kudamon": [
    "2cefb0e26603bd4465fed1d49a5453b2034954698781976b7d20d44fe174b7aa",
    "cdeaed295cc531643070771c1e11a717e8ac823c1ead55305e752d1e03f406d6",
    "5e2ca3b4bdecebe92bd55211da1dd9b93b08b57b268fe5624f27086d0eb1b035",
    "bcc816a56be12504d0e43797da829cf005045031f9cfe10509471dbf3c5fc521"
  ],
  "Kuramon": [
    "9abd52a4f33cd04d02b54335f589145140ae99b88f426e17401ebb3a0be02242",
    "555adf08c1ec36ae29e81aab3ae756a5c6e4273654a7b2c50f252676820dcfd8",
    "3875944fc7c7a7f17f6440fa82a6929c0b152f0ae197bdfc401690522edd72ed",
    "1c8237195f955a0175f5f5f4614bed169960c0a0b436925d679f20df480f61fd"
  ],
  "Lalamon": [
    "08f405894af8428dcad0f8c47d8eb6d2f514f6f600c44483672dad36bf67fe3c",
    "aafc6174d2c7ad0f929af00901b89636427d95c2dd17c030ecdda2967d17532e",
    "516f5030fbd068f78d9390e8e9a554ef159bb469e9353021e48ee03428e3f2a0",
    "d9eb98d11ec696635a5937100da24df084577cf76a3e0f192a784ba8c7de4ccf"
  ],
  "Lucemon": [
    "4f62fc7fd056bc5de350bc2a1ce6f66271e6266cc70ea90cdfa810fb10bbc7b8",
    "bfdf219050547ccfcbcf8b157f042f7651af4af35626d915f239c931d33e1237",
    "2daf9accfd2aa9bd034d203c8bc29af70f658c6001b46f52f7a477c60804d4af",
    "e83cbb7f39de79f78d6ff5fe83f0a67a1ef0c0b2a318301e30cc187d2bb58b1f"
  ],
  "Lunamon": [
    "4096f3d46226d99610696a192fd8516feca6f2e503e5f7836f1539c872943f37",
    "7a04307712252dc01e5284947f257a78b7e07475e8f917e349cfc56a4a409d8e",
    "875f7157e8560af49b83a28fe58dd75f852f05872b0d98ecd849151951a2ffcf",
    "d79a4e7db5d66ddaa6c5c4eecd428bb56c4f4124fcc4dd5913d4826cc913317f"
  ],
  "Mochimon": [
    "fad028271205ee4e696abe0aa9cd35893406ca2a7a89437f56708320272e115b",
    "75a586cf7c602e9421f750e206e5102d76f4acccbeade636015dc183f53ce1f1",
    "452e5346e845bda73d8bc5d15a747823f9cec4fc665d6a6ac2f69772424bf062",
    "aaa862b60f87ec0cd012e0c4b5617158d32378892b6a9cd1fc9d387f9eb415ea"
  ],
  "Monodramon": [
    "61aaff423f283f8b3f2e7a5c5cbba4d248f4eca7d493d07a82b852d63267b327",
    "00b1252b9b4c90688b8fd5b15195afe14d0caf51d48bbd8d15743a17b2ca9d94",
    "4d767cf97ef8828fd08cbea6295f120141fd5c82d80fba0d7542868dd5d31a26",
    "411e3504c266464e26e92469b4d2a700dc03ca9fa8da3cfbad1fa4b24d8e8039"
  ],
  "TerriermonAssistant": [
    "f4349356b9dcb8779e0d5ef0c10c9c6e7defa9c6b33c822fa4f0aaab5a8e3fc1",
    "ea46f39337a2a56887fa5e6da836b9aff461f1fa7fee9a9de9da0759096416bc",
    "9324ae25cee48540a5af8c1e19a3686ff4810ae8520cc2b68305d575dfd55212",
    "f7a0c601ef41dcba6bc409e7326d56282da6f3fedc042c5ee1fab33227c87ac2"
  ],
  "RapidmonArmor": [
    "824fb2c5382ce46fc0436962a878bcbdef83eb7a9e8995202bec5655eb4d4011",
    "a05a01c304a1c57327b84592a0de1e568b6b45785300616d66ef89136e05f9b2",
    "ea8f7fc74430f31778cd0d87dc26282964036731ca49a387cbdd6dda328316d3",
    "a331fcb95acf35be04aca68990d5e75e0a8a892446c1190d7a0bcbc1e71cb8c0"
  ],
  "OmnimonMerciful": [
    "0f15c5689bcc04884783bf26115411b500255af26182717296e9d2ccb730658c",
    "9048863a811c41c2f62a68f9548dd8b43fe91a77dcb50ee74b1d1f2d75460474",
    "a697c63120acf83bf5c71c7c22173ea1acfd1b86a7bf212dc53eff94aafdb1a8",
    "cb8994eb03132b2116fb102aa11471a6c0ea7d2e6cb12463e24754b7c7c90a8c"
  ]
};
main().catch(error=>{console.error(error);process.exitCode=1;});
