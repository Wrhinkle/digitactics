// Reviewed anatomy for imports with no usable authored actions. These maps name
// existing weighted bones; they never add a rig, alter weights, or change size.
import * as THREE from 'three';

const axis = {x:new THREE.Vector3(1,0,0),y:new THREE.Vector3(0,1,0),z:new THREE.Vector3(0,0,1)};
const chain = (names, amplitude=.08, phase=.7, direction='y') => ({names,amplitude,phase,direction});
const pair = (left,right) => [left,right];
// Several imports have abrupt shoulder/neck weight boundaries. Their measured
// safe ranges keep connected edges below twice their prepared length; the rest
// of the gesture comes from the torso, elbows, ears, and other mapped anatomy.
const anatomy = {
  'rip-ts_kotemon': {body:'J_spine01',chest:'J_spine02',head:'J_head',bodyLean:.2,headRange:.7,armRange:.55,idleGain:2,
    elbowAxis:'y',elbowSigns:[-1,1],elbowAttack:.55,elbowWin:.6,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),weaponArm:'J_arm_r',weaponElbow:'J_elbow_r',
    neutral:[['J_arm_l','z',-.3],['J_arm_r','z',.3],['J_elbow_l','y',-.65],['J_elbow_r','y',.65]],
    chains:[chain(['J_tail_01','J_tail_02','J_tail_03','J_tail_04','J_tail_05'],.035,.8)]},
  'rip-cat_ts_kuramon_chr629': {body:'J_center',secondary:'J_hip',family:'blob'},
  'rip-cat_ts_koromon_chr322': {body:'J_base',head:'J_ue_ago',headRange:.25,ears:pair('J_mimiL','J_mimiR'),family:'blob'},
  'rip-cat_ts_mochimon_chr388': {body:'J_center',head:'J_mune',headRange:.6,armRange:.35,jaw:'J_kuchi',arms:pair('J_ude_L','J_ude_R'),family:'blob'},
  'rip-cat_ts_kudamon_chr053': {body:'J_center',head:'J_head',jaw:'J_jaw',family:'serpent',
    chains:[chain(['J_spine',...Array.from({length:13},(_,i)=>`J_spine${i+1}`)],.065,.65)]},
  'rip-cat_ts_lalamon_chr056': {body:'J_center',head:'J_joint6',arms:pair('J_joint9','J_joint13'),family:'plant',
    chains:[chain(['J_joint17','J_joint7'],.12,.7,'z')]},
  'rip-cat_ts_lunamon_chr002': {body:'J_spine01',chest:'J_spine02',head:'J_head',headRange:.35,armRange:.4,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),
    chains:['A','B'].flatMap(row=>['l','r'].map(side=>chain([`J_ear${row}01_${side}`,`J_ear${row}02_${side}`],.13,.7,'x')))},
  'rip-cat_ts_impmon_chr081': {body:'J_spine2',bodyLean:.35,bodyWinTurn:.24,head:'J_head',jaw:'J_mouth',armRange:.15,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_ancon_l','J_ancon_r'),ears:pair('J_ear_l','J_ear_r'),
    chains:[chain(['J_tail','J_tail1','J_tail_end'],.1,.8)]},
  'rip-cat_ts_keramon_chr626': {body:'J_center',chest:'J_mune',head:'J_atama',jaw:'J_ago',family:'tentacle',
    arms:pair('J_ude_L','J_ude_R'),elbows:pair('J_hiji_L','J_hiji_R'),
    chains:[chain(['J_paipu01_L','J_paipu03_L','J_paipu05_L'],.1,.7,'x'),chain(['J_paipu01_R','J_paipu03_R','J_paipu05_R'],.1,.7,'x'),
      chain(['J_shokushu01','J_shokushu03'],.1,.8),chain(['J_shokushu01_L','J_shokushu02_L'],.1,.8),chain(['J_shokushu01_R','J_shokushu02_R'],.1,.8)]},
  'rip-cat_ts_lucemon_chr390': {body:'J_spine1',bodyLean:.3,chest:'J_spine2',head:'J_head',armRange:.3,elbowAttack:.5,elbowWin:.55,family:'biped',
    arms:pair('J_shoulderL','J_shoulderR'),elbows:pair('J_elbowL','J_elbowR'),
    chains:['L','R'].flatMap(side=>Array.from({length:4},(_,i)=>chain([1,2,3].map(j=>`J_wing${side}${i+1}_${j}`),.08,.6,'x')))},
  'rip-cat_ts_monodramon_chr458': {body:'J_spine01',bodyLean:.2,chest:'J_spine02',head:'J_head',headRange:.75,jaw:'J_jaw',armRange:.45,
    elbowAxis:'y',elbowSigns:[-1,1],elbowAttack:.55,elbowWin:.6,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),
    chains:[chain(['J_tail01','J_tail02','J_tail03','J_tail04'],.08,.8),...['l','r'].map(side=>chain([`J_wing01_${side}`,`J_wing02_${side}`],.1,.7,'x'))]},
  // Clipless imports whose shoulder-only generic victory swung both straight
  // arms up into a sideways T. Keep the readable generic swing on the gun/sword
  // arm (as Kotemon does), guard the other arm, and add torso/head/elbow motion.
  'rip-cs_rapidmon_armor': {body:'J_spine01',chest:'J_spine02',head:'J_head',bodyLean:.2,armRange:.6,elbowAttack:.55,elbowWin:.6,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),weaponArm:'J_arm_r',weaponBlend:.85,chains:[chain(['J_tail'],.08,.8)]},
  'rip-l_omnimon_merciful_mode': {body:'J_spine01',chest:'J_spine02',head:'J_head',bodyLean:.2,armRange:.6,elbowAttack:.55,elbowWin:.6,family:'biped',
    arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),weaponArm:'J_arm_r',
    chains:['l','r'].map(side=>chain(Array.from({length:4},(_,i)=>`J_wing0${i+1}_${side}`),.04,.6,'x'))},
  // Its long ears and lab coat hide the arms, so the generic shoulder-only
  // clips rendered four identical frames. Move the visible ears, head and torso.
  'rip-ts_terriermon_assistant': {body:'J_spine01',chest:'J_spine02',head:'J_neck',headRange:.6,jaw:'J_mouth',bodyLean:.3,armRange:.25,
    elbowAttack:.7,elbowWin:.7,family:'biped',arms:pair('J_arm_l','J_arm_r'),elbows:pair('J_elbow_l','J_elbow_r'),
    chains:['l','r'].map(side=>chain([1,2,3,4].map(i=>`J_ear0${i}_${side}`),.12,.6,'x'))},
  // Its Noesis reel is unrelated (anim:false). The visible creature is the
  // core sphere and its eight-link chain arms, which generic fallback never
  // reaches; rock the core and send traveling waves along each chain.
  'rip-cat_l_apocalymon_chr328': {body:'J_root_core',head:'J_head',family:'tentacle',
    chains:[['top','s_l'],['top','s_r'],['under','b_r'],['under','f'],['under','s_l']].map(([row,side])=>
      chain(Array.from({length:8},(_,i)=>`J_${row}chain0${i+1}${side==='f'?'_f':'_'+side}`),.07,.6))},
};
// Every project-generated biped rig (tools/convert/rig-static.mjs) shares these bone names.
const generatedBiped={body:'chest',head:'head',family:'biped',elbowAxis:'y',elbowSigns:[1,-1]};

const pulse = (u,start,end) => u<=start||u>=end?0:Math.sin(Math.PI*(u-start)/(end-start))**2;
function gesture(role,u){
  const envelope=Math.sin(Math.PI*u)**2,wave=Math.sin(2*Math.PI*u);
  if(role==='idle')return {lean:.22*wave,turn:.15*wave,lift:.12*(1-Math.cos(2*Math.PI*u)),wave,envelope};
  if(role==='attack')return {lean:-.3*pulse(u,0,.32)+pulse(u,.18,.76),turn:-.5*pulse(u,0,.4)+.6*pulse(u,.24,.8),lift:pulse(u,.15,.78),wave:wave*envelope,envelope};
  if(role==='special')return {lean:-.55*pulse(u,0,.55)+.8*pulse(u,.38,.92),turn:Math.sin(2*Math.PI*u)*envelope,lift:pulse(u,.12,.92),wave:wave*envelope,envelope};
  return {lean:.3*wave*envelope,turn:Math.sin(4*Math.PI*u)*envelope,lift:envelope,wave:Math.sin(4*Math.PI*u)*envelope,envelope};
}

// Called only after the normal import preparation has lowered outstretched arms.
// Each channel is relative to that prepared pose, with identical loop endpoints.
// Whole-body roots are explicit: name matching alone selected an unweighted leaf
// on Kuramon and missed Koromon's separately weighted upper-body branch.
export function anatomyFallbackClips(scene,source,original,map=anatomy[source?.uid]){
  if(!map)return original;
  scene.updateMatrixWorld(true);
  const unguarded=new Map((map.neutral||[]).map(([name])=>[name,scene.getObjectByName(name)?.quaternion.clone()]));
  // Kotemon's imported neutral is an A-pose with straight elbows. Its long
  // sleeves hide the wrists, so use measured shoulder/elbow/hand directions:
  // lower both upper arms and bend the forearms forward into a sword guard.
  // This map is reached only for its clipless fallback, never authored clips.
  for(const [name,direction,angle] of map.neutral||[]){
    const bone=scene.getObjectByName(name);if(!bone?.isBone)continue;
    const parent=bone.parent.getWorldQuaternion(new THREE.Quaternion()).invert();
    const world=bone.getWorldQuaternion(new THREE.Quaternion());
    bone.quaternion.copy(parent.multiply(new THREE.Quaternion().setFromAxisAngle(axis[direction],angle)).multiply(world));
    scene.updateMatrixWorld(true);
  }
  const channels=new Map();
  function add(name,direction,amount,{local=false}={}){
    const bone=name&&scene.getObjectByName(name);if(!bone?.isBone)return;
    let channel=channels.get(bone);
    if(!channel){channel={bone,rest:bone.quaternion.clone(),turns:[]};channels.set(bone,channel);}
    const vector=axis[direction].clone();
    if(!local)vector.applyQuaternion(bone.getWorldQuaternion(new THREE.Quaternion()).invert());
    channel.turns.push({axis:vector,amount});
  }
  const isBody=map.family==='blob'||map.family==='plant'||map.family==='tentacle';
  add(map.body,'x',(role,u,g)=>(role==='idle'?(isBody?.13:.1):(map.bodyLean??(isBody?.13:.1)))*g.lean);
  add(map.body,'z',(role,u,g)=>(isBody?.085:.04)*g.wave);
  add(map.body,'y',(role,u,g)=>(role==='win'?(map.bodyWinTurn??.14):.065)*g.turn);
  add(map.chest,'x',(role,u,g)=>.07*g.lean);
  add(map.chest,'y',(role,u,g)=>.09*g.turn);
  add(map.head,'x',(role,u,g)=>(map.headRange??1)*(role==='idle'?.045:.14)*g.lean);
  add(map.head,'y',(role,u,g)=>(map.headRange??1)*(role==='win'?.18:.075)*g.turn);
  add(map.secondary,'x',(role,u,g)=>-.045*g.lean);
  add(map.jaw,'x',(role,u,g)=>(role==='idle'?.015:role==='win'?.08:.12)*g.lift);
  for(const [side,name] of (map.ears||[]).entries())
    add(name,'z',(role,u,g)=>(side===0?1:-1)*(.06*g.wave+.16*g.lift));
  for(const [side,name] of (map.arms||[]).entries()){
    const sign=side===0?1:-1;
    // One arm leads the attack; the other remains a bent guard. Victory is an
    // asymmetric raised-hand gesture, never the former bilateral T-pose.
    add(name,'x',(role,u,g)=>-(map.armRange??1)*(role==='idle'?.08:role==='special'?.55:role==='win'?(side===0?.65:.24):(side===0?.6:.16))*g.lift);
    add(name,'z',(role,u,g)=>(map.armRange??1)*sign*(role==='win'?(side===0?.28:.09):.045)*g.wave);
  }
  // The axis follows the measured shoulder/elbow/wrist directions. Monodramon's
  // imported forearm points mostly along X, so X keys twist it instead of flexing.
  // Its mirrored Y hinges bring the hands forward; raising them sideways with Z
  // recreates an outstretched silhouette even when the elbow angle is nonzero.
  for(const [side,name] of (map.elbows||[]).entries())
    add(name,map.elbowAxis||'x',(role,u,g)=>(map.elbowSigns?.[side]??-1)*(role==='idle'?.05:role==='win'?(map.elbowWin??.42):side===0?(map.elbowAttack??.32):.2)*g.lift);
  for(const [chainIndex,path] of (map.chains||[]).entries())for(const [index,name] of path.names.entries()){
    add(name,path.direction,(role,u,g)=>{
      const phase=index*path.phase+chainIndex*.7;
      // Subtract the opening wave so all clips share the prepared neutral pose;
      // the idle loop stays smooth and actions recover without a pose snap.
      const traveling=Math.sin(2*Math.PI*u-phase)-Math.sin(-phase);
      const gain=role==='idle'?.5:role==='attack'?.8:role==='special'?1:1.2;
      return path.amplitude*gain*traveling*(role==='idle'?1:g.envelope);
    },{local:true});
  }
  if(!channels.size)return original;
  return original.map(clip=>{
    if(!['idle','attack','special','win'].includes(clip.name))return clip;
    const times=Array.from({length:65},(_,i)=>clip.duration*i/64);
    const tracks=[...channels.values()].map(({bone,rest,turns})=>{
      const values=[];
      const weapon=clip.name!=='idle'&&(bone.name===map.weaponArm||bone.name===map.weaponElbow);
      const sourceTrack=weapon&&clip.tracks.find(t=>t.name===bone.name+'.quaternion');
      const originalTurn=sourceTrack&&sourceTrack.createInterpolant();
      for(const time of times){
        const u=time/clip.duration,g=gesture(clip.name,u),rotation=rest.clone();
        if(clip.name==='idle'&&map.idleGain)for(const key of ['lean','turn','lift','wave'])g[key]*=map.idleGain;
        if(weapon){
          // Retain the already-reviewed clear sword lift/strike/celebration.
          // Blend only the weapon arm out of the new neutral; the other arm
          // stays guarded, and all clips meet at the same lowered endpoint.
          const target=originalTurn?new THREE.Quaternion().fromArray(originalTurn.evaluate(time)):unguarded.get(bone.name);
          if(target)rotation.slerp(target,g.envelope*(map.weaponBlend??1));
        }else for(const turn of turns)rotation.multiply(new THREE.Quaternion().setFromAxisAngle(turn.axis,turn.amount(clip.name,u,g)));
        rotation.normalize().toArray(values,values.length);
      }
      return new THREE.QuaternionKeyframeTrack(bone.name+'.quaternion',times,values);
    });
    return new THREE.AnimationClip(clip.name,clip.duration,tracks);
  });
}

// Project-generated biped rigs (static OBJ/mesh conversions) carry generated
// clips, not original game animation. Their idle barely moves the T/A-pose mesh,
// the old victory canceled the lowered shoulders into a literal T, and the
// X-axis forearm tracks twisted along the bone instead of bending it. Keep the
// original root/tail/leg tracks and replace only the upper-body channels with a
// lowered, bent-elbow neutral and the shared action builder. Upper arms are
// lowered to the reviewed RD neutral (0.95 rad below horizontal: the generated
// 0.65 rest plus 0.3). Arms already that low, or set low on the body (Gesomon's
// tentacles at hip height), keep their direction so limbs never reach the floor.
export function generatedBipedClips(gltf,source,clips){
  const rig=gltf.scene.getObjectByName('DT_Rig');
  if(rig?.userData.digitacticsRig!==1)return clips;
  if(rig.userData.body!=='biped')return generatedBodyClips(gltf,clips);
  if(!['chest','head','arm_L','arm_R','forearm_L','forearm_R'].every(name=>gltf.scene.getObjectByName(name)?.isBone))return clips;
  const idle=clips.find(c=>c.name==='idle');if(!idle)return clips;
  const rest=[];gltf.scene.traverse(node=>rest.push([node,node.position.clone(),node.quaternion.clone(),node.scale.clone()]));
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  // Catalog presets put arm_L on -X, the legacy static presets on +X. Order the
  // arms by side so lowering, elbow bends and the action builder agree.
  const sides=[['arm_L','forearm_L'],['arm_R','forearm_R']].map(([arm,elbow])=>{
    const shoulder=gltf.scene.getObjectByName(arm).getWorldPosition(new THREE.Vector3());
    return {arm,elbow,shoulder,direction:gltf.scene.getObjectByName(elbow).getWorldPosition(new THREE.Vector3()).sub(shoulder).normalize()};
  }).sort((a,b)=>a.direction.x-b.direction.x);
  // Arms already reaching forward (Ballistamon's short claws) are posed, and
  // their generated weights tear under the shared shoulder/elbow gestures.
  if(sides.some(side=>Math.abs(side.direction.z)>.5)){
    mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
    for(const [node,position,quaternion,scale] of rest){node.position.copy(position);node.quaternion.copy(quaternion);node.scale.copy(scale);}
    gltf.scene.updateMatrixWorld(true);return clips;
  }
  const box=new THREE.Box3().setFromObject(gltf.scene),height=Math.max(1e-9,box.max.y-box.min.y);
  function turn(name,direction,angle){
    const bone=gltf.scene.getObjectByName(name);if(!bone)return;
    const parent=bone.parent.getWorldQuaternion(new THREE.Quaternion()).invert();
    const world=bone.getWorldQuaternion(new THREE.Quaternion());
    bone.quaternion.copy(parent.multiply(new THREE.Quaternion().setFromAxisAngle(axis[direction],angle)).multiply(world));
    gltf.scene.updateMatrixWorld(true);
  }
  for(const [index,{arm,shoulder,direction}] of sides.entries()){
    const droop=Math.min(1.2,.95+Math.asin(Math.max(-1,Math.min(1,direction.y))));
    if(droop>0&&shoulder.y>box.min.y+height*.4)turn(arm,'z',(index===0?1:-1)*droop);
  }
  turn(sides[0].elbow,'y',.55);turn(sides[1].elbow,'y',-.55);
  const map={...generatedBiped,arms:pair(sides[0].arm,sides[1].arm),elbows:pair(sides[0].elbow,sides[1].elbow)};
  const result=anatomyFallbackClips(gltf.scene,source,clips,map).map(clip=>{
    const original=clips.find(c=>c.name===clip.name),bindings=new Set(clip.tracks.map(t=>t.name));
    return new THREE.AnimationClip(clip.name,clip.duration,[...original.tracks.filter(t=>!bindings.has(t.name)).map(t=>t.clone()),...clip.tracks]);
  });
  // The same whole-body root motion as the other generated plans adds weight
  // shifts that read at board scale, without touching the skin.
  mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);return generatedBodyClips(gltf,result);
}

// Every project-generated rig gets this root motion. The other body plans
// (quadruped, insect, ghost, baby, spider, mecha, tank) carry the same generator's clips at 0.01-0.16 rad, so
// every role reads as the frozen static mesh. Their generated weights are hard
// per-part islands, so larger joint angles open seams. Move the unweighted
// rig root instead: the whole creature bobs, leans into actions and hops in
// victory without any skin deformation. The generator's joint tracks, timing
// and loop endpoints are kept; only the smooth two-bone babies get a larger
// joint gain as well. Rig units are normalized to the creature's height.
function generatedBodyClips(gltf,clips){
  const root=gltf.scene.getObjectByName('root'),rig=gltf.scene.getObjectByName('DT_Rig');
  if(!root?.isBone||root.parent!==rig)return clips;
  const baby=rig.userData.body==='baby',base=root.quaternion.clone(),origin=root.position.clone();
  const rest=new Map();gltf.scene.traverse(node=>{if(node.isBone)rest.set(node.name,node.quaternion.clone());});
  const gain=track=>{
    const dot=track.name.lastIndexOf('.'),bind=rest.get(track.name.slice(0,dot));
    if(!baby||!bind||track.name.slice(dot+1)!=='quaternion')return track.clone();
    const inverse=bind.clone().invert(),values=new Float32Array(track.values.length),q=new THREE.Quaternion();
    for(let i=0;i<values.length;i+=4){
      const delta=inverse.clone().multiply(q.fromArray(track.values,i)).normalize();
      if(delta.w<0)delta.set(-delta.x,-delta.y,-delta.z,-delta.w);
      const length=Math.hypot(delta.x,delta.y,delta.z),angle=2*Math.atan2(length,delta.w);
      const scaled=length>1e-9?new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(delta.x/length,delta.y/length,delta.z/length),Math.min(.6,angle*2)):new THREE.Quaternion();
      bind.clone().multiply(scaled).toArray(values,i);
    }
    return new THREE.QuaternionKeyframeTrack(track.name,Array.from(track.times),Array.from(values));
  };
  return clips.map(clip=>{
    const role=clip.name;if(!['idle','attack','special','win'].includes(role))return clip;
    const times=Array.from({length:65},(_,i)=>clip.duration*i/64),positions=[],rotations=[];
    for(const time of times){
      const u=time/clip.duration,wave=Math.sin(2*Math.PI*u),pulse=Math.sin(Math.PI*u)**2;
      // Forward lean pivots on the floor; each hop at least clears that pivot.
      let lean=0,sway=0,turn=0,hop=0;
      if(role==='idle'){lean=.02*wave;sway=.015*Math.sin(4*Math.PI*u);hop=.012*(1-Math.cos(2*Math.PI*u))/2;}
      else if(role==='attack'){lean=.14*pulse;hop=.04*pulse;}
      else if(role==='special'){lean=.2*pulse;turn=.12*wave*pulse;hop=.07*pulse;}
      else{sway=.08*wave*pulse;hop=.09*Math.abs(Math.sin(2*Math.PI*u))*pulse;}
      origin.clone().add(new THREE.Vector3(0,hop,0)).toArray(positions,positions.length);
      base.clone().multiply(new THREE.Quaternion().setFromEuler(new THREE.Euler(lean,turn,sway,'YXZ'))).toArray(rotations,rotations.length);
    }
    const tracks=clip.tracks.filter(t=>!/^root\.(?:position|quaternion)$/.test(t.name)).map(gain);
    tracks.push(new THREE.VectorKeyframeTrack('root.position',times,positions),new THREE.QuaternionKeyframeTrack('root.quaternion',times,rotations));
    return new THREE.AnimationClip(role,clip.duration,tracks);
  });
}
