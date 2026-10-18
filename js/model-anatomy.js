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
  'rip-cat_rd_beelzemon_main': {body:'chest',head:'head',family:'biped',arms:pair('arm_L','arm_R'),elbows:pair('forearm_L','forearm_R'),elbowAxis:'y',elbowSigns:[1,-1]},
  'rip-cat_rd_wargrowlmon_main': {body:'chest',head:'head',family:'biped',arms:pair('arm_L','arm_R'),elbows:pair('forearm_L','forearm_R'),elbowAxis:'y',elbowSigns:[1,-1]},
};

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
export function anatomyFallbackClips(scene,source,original){
  const map=anatomy[source?.uid];if(!map)return original;
  scene.updateMatrixWorld(true);
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
      for(const time of times){
        const u=time/clip.duration,g=gesture(clip.name,u),rotation=rest.clone();
        for(const turn of turns)rotation.multiply(new THREE.Quaternion().setFromAxisAngle(turn.axis,turn.amount(clip.name,u,g)));
        rotation.normalize().toArray(values,values.length);
      }
      return new THREE.QuaternionKeyframeTrack(bone.name+'.quaternion',times,values);
    });
    return new THREE.AnimationClip(clip.name,clip.duration,tracks);
  });
}

// These two OBJ conversions contain project-generated clips, not original game
// animation. The old victory canceled the lowered shoulders into a literal T,
// and its X-axis forearm tracks twisted along the bone instead of bending it.
// Keep the original root/tail/leg tracks and replace only reviewed upper-body
// channels with a lowered, bent-elbow neutral and the shared action builder.
export function generatedBipedClips(gltf,source,clips){
  if(!['rip-cat_rd_beelzemon_main','rip-cat_rd_wargrowlmon_main'].includes(source?.uid))return clips;
  if(gltf.scene.getObjectByName('DT_Rig')?.userData.digitacticsRig!==1)return clips;
  const idle=clips.find(c=>c.name==='idle');if(!idle)return clips;
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  function turn(name,direction,angle){
    const bone=gltf.scene.getObjectByName(name);if(!bone)return;
    const parent=bone.parent.getWorldQuaternion(new THREE.Quaternion()).invert();
    const world=bone.getWorldQuaternion(new THREE.Quaternion());
    bone.quaternion.copy(parent.multiply(new THREE.Quaternion().setFromAxisAngle(axis[direction],angle)).multiply(world));
    gltf.scene.updateMatrixWorld(true);
  }
  turn('arm_L','z',.3);turn('arm_R','z',-.3);
  turn('forearm_L','y',.55);turn('forearm_R','y',-.55);
  const result=anatomyFallbackClips(gltf.scene,source,clips).map(clip=>{
    const original=clips.find(c=>c.name===clip.name),bindings=new Set(clip.tracks.map(t=>t.name));
    return new THREE.AnimationClip(clip.name,clip.duration,[...original.tracks.filter(t=>!bindings.has(t.name)).map(t=>t.clone()),...clip.tracks]);
  });
  mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);return result;
}
