// Skeletal fallback for rigged imports that ship without usable animation clips.
// Authored clips take priority, with the documented source-specific repairs below.
// These motions preserve the original mesh and skin.
import * as THREE from 'three';
import { anatomyFallbackClips, generatedBipedClips, hasFallbackAnatomy } from './model-anatomy.js';
import { rigidPartClips } from './model-rigid-parts.js';

const V = () => new THREE.Vector3();
const Q = () => new THREE.Quaternion();
const X = new THREE.Vector3(1, 0, 0), Y = new THREE.Vector3(0, 1, 0), Z = new THREE.Vector3(0, 0, 1);
const armName = /(?:^|_)(?:arm\d*|ude|shoulder)(?:[lr])?(?:_|$)|upperarm/i;
const headName = /(?:^|_)(?:head|atama|face)(?:_?0?[12])?(?:_|$)/i;
const chestName = /(?:^|_)(?:chest|mune|spine0[12])(?:_|$)/i;
const tailName = /(?:^|_)(?:tail|sippo|shippo)(?:0?1)?(?:_|$)/i;
const valid = b => !/end|nub|twist|helper/i.test(b.name);

export function modelClips(gltf, src) {
  const rigid = rigidPartClips(gltf, src);
  if (rigid) return rigid;
  if(src?.uid==='rip-l_garurumon_greymon_statue')return statueClips(gltf.scene);
  const authored = livelyIdle(src,generatedBipedClips(gltf,src,blobBodyClips(gltf,src,heldWeaponOrigin(gltf,src,sampledRootAnchor(gltf,src,boardRootTravel(gltf,src,boardBodyScale(gltf,src))))))).filter(c=>!src?.fallbackRoles?.includes(c.name));
  if (authored.length && src?.anim !== false) {
    // This Seadramon export labels a single unchanging pose "idle". Build a
    // gentle skeletal loop from that assembled pose, retaining other actions.
    const staticIdle=src?.uid==='rip-cat_cs_seadramon_chr347';
    const missing = ['idle','attack','special','win'].filter(role => staticIdle&&role==='idle'||!authored.some(c => c.name === role));
    if (!missing.length) return authored;
    // Build missing actions from the authored neutral pose, retaining every original clip.
    const pose = new THREE.AnimationMixer(gltf.scene);
    pose.clipAction(authored.find(c => c.name === 'idle') || authored[0]).play();
    pose.update(0);
    let extra = fallbackClips(gltf.scene, false).filter(c => missing.includes(c.name));
    if(staticIdle)extra=serpentClips(gltf.scene,extra);
    pose.stopAllAction();pose.uncacheRoot(gltf.scene);
    return [...authored.filter(clip=>!staticIdle||clip.name!=='idle'), ...extra];
  }
  // Disabled Noesis reels contain unrelated actions and root travel. Their first frame
  // still provides the assembled rest pose, including armor and weapon transforms.
  if (authored.length) {
    const pose = new THREE.AnimationMixer(gltf.scene);
    pose.clipAction(authored[0]).play();
    pose.update(0);
  }
  // Reviewed anatomy maps keep the paired arm reference their weapon arms blend toward.
  const fallback=fallbackClips(gltf.scene,true,hasFallbackAnatomy(src));
  // This clipless Kotemon already parents the sword to its hand, but retains a
  // separate staging offset. Correct only its attachment origin, after fallback
  // preparation so the existing body/arm animation channels remain identical.
  if(src?.uid==='rip-ts_kotemon'&&!gltf.animations.length){
    const sword=gltf.scene.getObjectByName('J_bamboo_sword');
    if(sword?.parent?.name==='J_hand_r')sword.position.set(0,0,0);
  }
  return anatomyFallbackClips(gltf.scene,src,fallback);
}

// These authored idles breathe at a fifth of a normal loop (largest vertex
// movement 1.1-1.7% of body height), so boards show a frozen statue. Scale
// each rotation key's offset from its opening key: the authored rhythm, opening
// pose, loop endpoints, translations, scales and every other clip stay exact.
function livelyIdle(src,clips){
  const gain={'rip-durandamon':4,'rip-tankdramon':3}[src?.uid];
  if(!gain)return clips;
  return clips.map(clip=>{
    if(clip.name!=='idle')return clip;
    const corrected=clip.clone(),opening=new THREE.Quaternion(),key=new THREE.Quaternion();
    for(const track of corrected.tracks){
      if(!track.name.endsWith('.quaternion'))continue;
      opening.fromArray(track.values,0);
      const inverse=opening.clone().invert();
      for(let i=0;i<track.values.length;i+=4){
        key.fromArray(track.values,i);
        const delta=inverse.clone().multiply(key);if(delta.w<0)delta.set(-delta.x,-delta.y,-delta.z,-delta.w);
        const angle=2*Math.acos(Math.min(1,delta.w)),s=Math.sqrt(Math.max(0,1-delta.w*delta.w));
        if(s<1e-9)continue;
        const axis=new THREE.Vector3(delta.x/s,delta.y/s,delta.z/s);
        opening.clone().multiply(new THREE.Quaternion().setFromAxisAngle(axis,angle*gain)).normalize().toArray(track.values,i);
      }
    }
    return corrected;
  });
}

// Pabumon's disconnected body roots are animated independently in two imported
// clips, tearing ordinary body triangles into 14-24x spikes. Rock those roots
// together from the assembled idle shape. Mouth and floating-bubble tracks keep
// their original timing and values, and no mesh, skin weight or scale is changed.
function blobBodyClips(gltf,src,clips){
  if(src?.uid!=='rip-cat_cs_pabumon_chr387')return clips;
  const bodyName=/^J_(?:center|body[1-4]|leg[FLR][12]|tail[1-3])$/;
  const idle=clips.find(c=>c.name==='idle'),root=gltf.scene.getObjectByName('J_root');
  if(!idle||!root)return clips;
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  const body=[];gltf.scene.traverse(bone=>{if(bone.isBone&&bodyName.test(bone.name))body.push({bone,p:bone.position.clone(),q:bone.quaternion.clone()});});
  const center=body.find(b=>b.bone.name==='J_center');if(!center){mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);return clips;}
  const box=new THREE.Box3().setFromObject(gltf.scene),height=box.max.y-box.min.y;
  const rotation=root.getWorldQuaternion(Q()),inverse=root.matrixWorld.clone().invert();
  const bob=new THREE.Vector3(0,height*.04,0).applyMatrix4(inverse).sub(V().applyMatrix4(inverse));
  const result=clips.map(clip=>{
    if(clip.name!=='attack'&&clip.name!=='win')return clip;
    const times=Array.from({length:65},(_,i)=>clip.duration*i/64);
    const tracks=clip.tracks.filter(track=>{const dot=track.name.lastIndexOf('.');return !bodyName.test(track.name.slice(0,dot))||!['position','quaternion'].includes(track.name.slice(dot+1));}).map(track=>track.clone());
    for(const {bone,p,q} of body){
      const positions=[],rotations=[];
      for(const time of times){
        const u=time/clip.duration,pulse=Math.sin(Math.PI*u)**2;
        const angle=clip.name==='attack'?.15*pulse:.12*Math.sin(2*Math.PI*u);
        const rock=rotation.clone().invert().multiply(Q().setFromAxisAngle(clip.name==='attack'?X:Z,angle)).multiply(rotation);
        const position=p.clone(),quaternion=q.clone();
        if(bone.parent===root){position.sub(center.p).applyQuaternion(rock).add(center.p).addScaledVector(bob,pulse);quaternion.premultiply(rock);}
        position.toArray(positions,positions.length);quaternion.toArray(rotations,rotations.length);
      }
      tracks.push(new THREE.VectorKeyframeTrack(bone.name+'.position',times,positions),new THREE.QuaternionKeyframeTrack(bone.name+'.quaternion',times,rotations));
    }
    return new THREE.AnimationClip(clip.name,clip.duration,tracks);
  });
  mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);return result;
}

// The imported right gun is parented to the hand but retains an idle origin
// below the feet. Remove that constant attachment offset, retaining the authored
// recoil/throw deltas and rotations rather than hiding or locking the weapon.
function heldWeaponOrigin(gltf,src,clips){
  if(src?.uid!=='rip-cs_beelzemon')return clips;
  const name='J_gun_r.position',gun=gltf.scene.getObjectByName('J_gun_r');
  const track=clips.find(c=>c.name==='idle')?.tracks.find(t=>t.name===name);
  if(!track||gun?.parent?.name!=='J_hand_r')return clips;
  const offset=Array.from(track.createInterpolant().evaluate(0));
  return clips.map(clip=>{
    if(!clip.tracks.some(t=>t.name===name))return clip;
    const corrected=clip.clone(),out=corrected.tracks.find(t=>t.name===name);
    for(let i=0;i<out.values.length;i++)out.values[i]-=offset[i%3];
    return corrected;
  });
}

// This long serpent's spine uses J_chine names rather than humanoid spine/tail
// names. A small traveling bend makes the assembled body swim without changing
// its root, segment lengths or scale. Every role shares the same loop endpoints.
function serpentClips(scene,clips){
  const names=['J_chine_e','J_chine_f','J_chine_g','J_chine_h','J_chine_i','J_chine_j',
    'J_chine_a','J_chine_b','J_chine_c','J_chine','J_chine_k','J_chine_l','J_chine_m','J_chine_n'];
  const segments=names.map((name,i)=>({bone:scene.getObjectByName(name),phase:(i<6?i+1:5-i)*.75})).filter(s=>s.bone);
  return clips.map(clip=>{
    const corrected=clip.clone();
    for(const {bone,phase} of segments){
      const times=[],values=[],base=bone.quaternion.clone();
      for(let i=0;i<=32;i++){
        const u=i/32,pulse=Math.sin(u*Math.PI)**2,boost=clip.name==='idle'?0:clip.name==='attack'?.03:.06;
        const angle=(.12+boost*pulse)*Math.sin(u*Math.PI*2-phase),q=base.clone().multiply(Q().setFromAxisAngle(Y,angle));
        times.push(clip.duration*u);values.push(q.x,q.y,q.z,q.w);
      }
      corrected.tracks.push(new THREE.QuaternionKeyframeTrack(bone.name+'.quaternion',times,values));
    }
    return corrected;
  });
}

// Stone Guardian is an intentionally rigid statue, without a creature skeleton.
// Move its assembled root gently; never deform the stone pieces or change size.
function statueClips(scene){
  const root=scene.getObjectByName('ModelRoot');if(!root)return [];
  scene.updateMatrixWorld(true);
  const box=new THREE.Box3().setFromObject(scene),parentScale=root.parent.getWorldScale(V());
  const height=Math.max(.001,box.max.y-box.min.y)/Math.max(.001,Math.abs(parentScale.y));
  const base=root.position.clone(),rotation=root.quaternion.clone();
  return [['idle',2.8],['attack',.65],['special',1.2],['win',2]].map(([role,duration])=>{
    const times=[],positions=[],rotations=[];
    for(let i=0;i<=32;i++){
      const u=i/32,wave=Math.sin(u*Math.PI*2),pulse=Math.sin(u*Math.PI)**2;
      const y=role==='idle'?.006*(1-Math.cos(u*Math.PI*2)):role==='win'?.035*pulse:0;
      const angle=role==='idle'?.012*wave:role==='win'?.045*wave:(role==='special'?-.09:-.06)*pulse;
      const axis=role==='idle'||role==='win'?Z:X,q=rotation.clone().multiply(Q().setFromAxisAngle(axis,angle));
      times.push(duration*u);positions.push(base.x,base.y+height*y,base.z);rotations.push(q.x,q.y,q.z,q.w);
    }
    return new THREE.AnimationClip(role,duration,[new THREE.VectorKeyframeTrack(root.name+'.position',times,positions),new THREE.QuaternionKeyframeTrack(root.name+'.quaternion',times,rotations)]);
  });
}

// Some exports key an action's armature group (GRP_joint_03 and similar) at one
// constant scale that differs from idle's: Dorugamon 1.5x, Angemon 1.19x and
// Beelzemon 0.28x for the whole attack. That is a different export scale, not
// motion, so hold it at idle's value. Only constant tracks on a node that holds
// at least 80% of the skeleton's bones are held; animated squash, shown/hidden
// weapons and other partial-subtree scales stay.
function heldArmatureScale(gltf,clips){
  const idle=clips.find(c=>c.name==='idle');
  let total=0;gltf.scene.traverse(o=>{if(o.isBone)total++;});
  if(!total)return clips;
  return clips.map(clip=>{
    if(!['attack','special','win'].includes(clip.name))return clip;
    let corrected=null;
    clip.tracks.forEach((track,index)=>{
      if(!track.name.endsWith('.scale'))return;
      const node=gltf.scene.getObjectByName(track.name.slice(0,-6));
      if(!node)return;
      let bones=0;node.traverse(o=>{if(o.isBone)bones++;});
      if(bones<total*.8)return;
      const v=track.values,first=[v[0],v[1],v[2]];
      for(let i=3;i<v.length;i++)if(Math.abs(v[i]-first[i%3])>1e-6)return;
      const own=idle?.tracks.find(t=>t.name===track.name);
      const neutral=own?Array.from(own.createInterpolant().evaluate(0)):node.scale.toArray();
      if(first.every((x,i)=>Math.abs(x-neutral[i])<=.02*Math.max(Math.abs(neutral[i]),1e-6)))return;
      corrected??=clip.clone();
      const out=corrected.tracks[index];
      for(let i=0;i<out.values.length;i++)out.values[i]=neutral[i%3];
    });
    return corrected||clip;
  });
}

// These imported attacks contain whole-body enlargement (14x, 13.5x, 4.79x)
// or shrinkage (1/15x), rather than an independently animated attack effect.
// Links Syakomon also compounds spine/antenna scales into 11x height and 91x
// ordinary mesh edges. Keep those body scales at idle while retaining its shell
// hinge, mouth, teeth, joint rotations and root trajectory. Match exact asset UID.
function boardBodyScale(gltf,src) {
  const clips=heldArmatureScale(gltf,gltf.animations||[]);
  const bodies = {
    db64891d559b4d0cbcb5633d90779ee3: ['GRP_joint_02'],
    'rip-cat_cs_renamon_chr391': ['J_root'],
    '889b1324568743f3be36b32ac49c95da': ['GRP_joint_02'],
    bbeb1da28aca459da86b46cba6d57ca8: ['GRP_joint_03'],
    'rip-cat_l_syakomon_chr709': ['J_spine01','J_ear01_l','J_ear02_l','J_ear01_r','J_ear02_r'],
  }[src?.uid];
  if(!bodies)return clips;
  const idle=clips.find(c=>c.name==='idle'),neutral=new Map();
  for(const body of bodies){
    const name=body+'.scale',track=idle?.tracks.find(track=>track.name===name);
    const scale=track?Array.from(track.createInterpolant().evaluate(0)):gltf.scene.getObjectByName(body)?.scale.toArray();
    if(scale)neutral.set(name,scale);
  }
  return clips.map(clip=>{
    if(clip.name!=='attack'||!clip.tracks.some(track=>neutral.has(track.name)))return clip;
    const corrected=clip.clone();
    for(const track of corrected.tracks){
      const scale=neutral.get(track.name);if(!scale)continue;
      for(let i=0;i<track.values.length;i++)track.values[i]=scale[i%3];
    }
    return corrected;
  });
}

// These actions carry cinematic scene travel in a skeletal root. The combat
// simulation already owns the unit's hex, so keep that anchor in place. Retain
// the authored vertical trajectory at a board-sized amplitude, easing its final
// flight back to idle; limb and projectile/weapon animation remains untouched.
function boardRootTravel(gltf,src,clips){
  const policy={
    'rip-cs_gallantmon_crimson_mode':['J_root','special'], 'rip-hisyaryumon':['J_center','special'],
    'rip-cs_phantomon':['J_center','win'], 'rip-cs_icemon':['J_center','win'],
    'rip-cat_cs_dracomon_chr208':['J_center','win'], 'rip-dracomon':['J_center','win'],
    'rip-cs_raptordramon':['J_center','win'],
    'rip-crescemon':['J_center','special'], 'rip-cs_pilevolcamon':['J_center','special'],
    'rip-l_millenniummon':['J_center','special'], 'rip-l_omnimon_x':['J_center','special'],
    'rip-nc_jupitermon':['Bip001','special'],
    'rip-cs_skullsatamon':['J_center','win'], 'rip-cs_merukimon':['J_center','special','win'],
    bbeb1da28aca459da86b46cba6d57ca8:['J_root_04','attack'],
    'rip-cat_cs_betamon_chr389':['J_root','attack'], 'rip-cat_cs_alphamon_chr416':['J_center','special'],
    'rip-cat_magnagarurumon_chr678':['J_center','special','win'],
    'rip-cs_valkyrimon':['J_center','special'],
    'rip-durandamon':['J_center','special'], 'rip-kimeramon':['J_center','win'],
    'rip-cs_grademon':['J_center','special'], 'rip-cs_gryphonmon':['J_center','win'],
    'rip-l_gumdramon':['J_center','special'],
    'rip-stingmon':['J_center','special'], 'rip-dinobeemon':['J_center','special','win'],
    'rip-zubamon':['J_center','special'], 'rip-zubaeagermon':['J_center','special'],
    'rip-puppetmon':['J_center','special'],
    // Whole-body roots (100% of the weighted skin) that travel 2.5-19 idle
    // heights in these actions, so the creature leaves its hex and the frame.
    'rip-fanbeemon':['J_center','special','win'], 'rip-cs_gaomon':['J_root','special'],
    'rip-cs_sistermon_blanc_awakened':['J_center','special'], 'rip-cs_tyrantkabuterimon':['J_center','win'],
    'rip-cs_coelamon':['J_center','special'], 'rip-hackmon':['J_center','special'], 'rip-baohuckmon':['J_center','special'],
    '6ccfdb5317e14d39b899181ffef0860b':['J_root_03','attack'], 'rip-cs_aquilamon':['mass','special'],
    'rip-jazamon':['J_center','win'], 'rip-jazardmon':['J_center','special','win'], 'rip-jazarichmon':['J_center','special','win'],
    'rip-chirinmon':['J_center','special'], 'rip-kuwagamon':['J_center','special'],
    'rip-kendogarurumon':['J_center','special','win'], 'rip-magnagarurumon':['J_center','special','win'],
    'rip-lekismon':['J_center','special','win'], 'rip-princemamemon':['J_center','special'],
    'rip-meicoomon':['chr999J_center','special'], 'rip-meicrackmon_vicious_mode':['J_center','special'], 'rip-raguelmon':['J_center','special'],
    'rip-monodramon':['J_center','special'], 'rip-strikedramon':['J_center','special'],
    'rip-cs_alphamon_nx':['J_center','special'], 'rip-cs_alphamon_ouryuken':['J_center','special'],
    'rip-cs_arcadiamon_in_training':['J_center','special','win'], 'rip-cs_omnimon_nx':['J_center','special'],
    'rip-l_cresgarurumon':['J_center','special'], 'rip-l_minervamon':['J_center','special'], 'rip-l_omnimon_zwart_d':['J_center','special'],
    'rip-l_pandamon':['J_mass','special'], 'rip-vorvomon':['chr120J_center','win'],
    // The body root covers 97-98%; the thrown weapon / held lance is a sibling.
    'rip-ryudamon':['J_center','special'], 'rip-ginryumon':['J_center','special','win'],
    // Sweep range 0-312: whole-body roots that dash 2-15 idle heights away.
    'rip-cs_saviorhuckmon':['J_center','special'], 'rip-l_slayerdramon':['J_center','special'],
    'rip-cs_triceramon':['J_center','special'], 'rip-l_whamon':['J_center','attack','special'],
    'rip-armadillomon':['J_center','special'], 'rip-cat_cs_armadillomon_chr707':['J_center','special'],
    'rip-cat_cs_wanyamon_chr321':['J_center','special'], 'rip-cat_cs_zubamon_chr697':['J_center','special'],
    'rip-cat_cs_zubaeagermon_chr698':['J_center','special','win'], 'rip-cat_l_aquilamon_chr015':['mass','special'],
    'rip-cat_l_gaomon_chr055':['J_root','special'],
    'rip-cat_cs_ginryumon_chr191':['J_center','special','win'], 'rip-cat_cs_imperialdramon_paladin_mode_chr420':['J_center','special'],
  }[src?.uid];
  if(!policy)return clips;
  const [body,...roles]=policy,idle=clips.find(c=>c.name==='idle'),name=body+'.position',node=gltf.scene.getObjectByName(body);
  if(!idle||!node||!clips.some(clip=>roles.includes(clip.name)&&clip.tracks.some(t=>t.name===name)))return clips;
  const mixer=new THREE.AnimationMixer(gltf.scene);mixer.clipAction(idle).play();mixer.update(0);gltf.scene.updateMatrixWorld(true);
  const origin=node.getWorldPosition(V()),parent=node.parent.matrixWorld.clone(),inverse=parent.clone().invert();
  const box=new THREE.Box3().setFromObject(gltf.scene),height=Math.max(1e-9,box.max.y-box.min.y);
  // Puppetmon's mallet (Ryudamon's thrown blade, Ginryumon's lance) is a sibling
  // of the body subtree. Move their unanimated common parent so every authored
  // weapon/body relationship survives.
  const shared=['rip-puppetmon','rip-ryudamon','rip-ginryumon'].includes(src.uid)?node.parent:null;
  const sharedInverse=shared?new THREE.Matrix3().setFromMatrix4(shared.parent.matrixWorld.clone().invert()):null;
  const result=clips.map(action=>{
    const role=action.name,track=action.tracks.find(t=>t.name===name);
    if(!roles.includes(role)||!track)return action;
    // Some entrances contain only two root keys; some attack roots stop keying
    // before the other joints. Sample the complete settling envelope in both cases.
    const resample=!!shared||role==='win'||track.times[track.times.length-1]<action.duration;
    const times=resample?[...new Set([...track.times,...Array.from({length:65},(_,i)=>action.duration*i/64)])].sort((a,b)=>a-b):Array.from(track.times);
    const interpolant=track.createInterpolant(),displacements=times.map(time=>new THREE.Vector3().fromArray(interpolant.evaluate(time)).applyMatrix4(parent).y-origin.y);
    const up=Math.min(1,height*.35/Math.max(...displacements,1e-9)),down=Math.min(1,height*.08/Math.max(...displacements.map(y=>-y),1e-9));
    const corrected=action.clone(),out=shared?new THREE.VectorKeyframeTrack(shared.name+'.position',times,new Array(times.length*3).fill(0)):corrected.tracks.find(t=>t.name===name);
    if(shared)corrected.tracks.push(out);
    out.times=new Float32Array(times);out.values=new Float32Array(times.length*3);
    for(let i=0;i<displacements.length;i++){
      const u=times[i]/action.duration,end=Math.min(1,Math.max(0,(1-u)/.2)),start=role==='win'?Math.min(1,u/.15):1;
      const settle=end*end*(3-2*end)*start*start*(3-2*start);
      const y=displacements[i]*(displacements[i]<0?down:up)*settle;
      const desired=origin.clone().add(new THREE.Vector3(0,y,0));
      if(shared){
        const authored=new THREE.Vector3().fromArray(interpolant.evaluate(times[i])).applyMatrix4(parent);
        desired.sub(authored).applyMatrix3(sharedInverse).add(shared.position).toArray(out.values,i*3);
      }else desired.applyMatrix4(inverse).toArray(out.values,i*3);
    }
    return corrected;
  });
  mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
  return src.uid==='rip-zubaeagermon'?somersaultClearance(gltf,result):result;
}

// Turuiemon's special dash translates and turns J_root while its body root
// J_center travels inside that rotating frame (3.5 + 6 idle heights in all), so
// a single-track correction cannot hold the hex. Sample the complete pose and
// counter-translate J_root so J_center keeps its hex, with the same bounded,
// settling vertical envelope. Every rotation and the J_center track stay exact.
function sampledRootAnchor(gltf,src,clips){
  const policy={'rip-cs_turuiemon':['J_center','J_root','special']}[src?.uid];
  if(!policy)return clips;
  const [bodyName,moverName,...roles]=policy,scene=gltf.scene,body=scene.getObjectByName(bodyName),mover=scene.getObjectByName(moverName),idle=clips.find(c=>c.name==='idle');
  if(!body||!mover||!idle)return clips;
  const mixer=new THREE.AnimationMixer(scene);mixer.clipAction(idle).play();mixer.update(0);scene.updateMatrixWorld(true);
  const origin=body.getWorldPosition(V()),box=new THREE.Box3().setFromObject(scene),height=Math.max(1e-9,box.max.y-box.min.y);
  const toParent=new THREE.Matrix3().setFromMatrix4(mover.parent.matrixWorld.clone().invert());
  mixer.stopAllAction();
  const result=clips.map(clip=>{
    if(!roles.includes(clip.name))return clip;
    const times=Array.from({length:Math.ceil(clip.duration*240)+1},(_,i)=>Math.min(clip.duration,i/240));
    const action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
    const samples=times.map(time=>{mixer.setTime(time);scene.updateMatrixWorld(true);return {world:body.getWorldPosition(V()),local:mover.position.clone()};});
    mixer.stopAllAction();
    const lifts=samples.map(sample=>sample.world.y-origin.y);
    const up=Math.min(1,height*.35/Math.max(...lifts,1e-9)),down=Math.min(1,height*.08/Math.max(...lifts.map(y=>-y),1e-9)),values=[];
    samples.forEach((sample,i)=>{
      const end=Math.min(1,Math.max(0,(1-times[i]/clip.duration)/.2)),settle=end*end*(3-2*end);
      const desired=origin.clone().add(V().set(0,lifts[i]*(lifts[i]<0?down:up)*settle,0));
      sample.local.clone().add(desired.sub(sample.world).applyMatrix3(toParent)).toArray(values,values.length);
    });
    const corrected=clip.clone();corrected.tracks=corrected.tracks.filter(track=>track.name!==moverName+'.position');
    corrected.tracks.push(new THREE.VectorKeyframeTrack(moverName+'.position',times,values));
    return corrected;
  });
  mixer.uncacheRoot(scene);return result;
}

// Anchoring this exact special exposed a second issue: its preserved somersault
// can put the head through the board when the old cinematic jump is compressed.
// Keep the authored pose and XZ anchor; add only the minimum vertical clearance
// for the head/torso/feet. The separately weighted tail blade keeps its intended
// reach and does not lift the whole creature whenever its tip sweeps downward.
function somersaultClearance(gltf,clips){
  const scene=gltf.scene,idle=clips.find(c=>c.name==='idle'),clip=clips.find(c=>c.name==='special');
  const node=scene.getObjectByName('J_center'),track=clip?.tracks.find(t=>t.name==='J_center.position');
  if(!idle||!node||!track)return clips;
  const mixer=new THREE.AnimationMixer(scene);mixer.clipAction(idle).play();mixer.update(0);scene.updateMatrixWorld(true);
  const base=new THREE.Box3().setFromObject(scene,true),height=Math.max(1e-9,base.max.y-base.min.y),meshes=[];
  scene.traverse(mesh=>{
    if(!mesh.isSkinnedMesh)return;
    const indices=[],joints=mesh.geometry.attributes.skinIndex,weights=mesh.geometry.attributes.skinWeight;
    for(let i=0;i<joints.count;i++){
      let tail=0;for(let k=0;k<4;k++)if(/^J_tail/.test(mesh.skeleton.bones[joints.getComponent(i,k)].name))tail+=weights.getComponent(i,k);
      if(tail<.5)indices.push(i);
    }
    meshes.push({mesh,indices});
  });
  mixer.stopAllAction();
  const action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
  const times=[...new Set([...track.times,...Array.from({length:257},(_,i)=>clip.duration*i/256)])].sort((a,b)=>a-b),values=[];
  for(const time of times){
    mixer.setTime(time);scene.updateMatrixWorld(true);
    let floor=Infinity;
    for(const {mesh,indices} of meshes){mesh.skeleton.update();for(const index of indices)floor=Math.min(floor,mesh.getVertexPosition(index,V()).applyMatrix4(mesh.matrixWorld).y);}
    const margin=height*.01*Math.sin(Math.PI*time/clip.duration)**2;
    const lift=Math.max(0,base.min.y+margin-floor);
    const delta=new THREE.Vector3(0,lift,0).applyMatrix3(new THREE.Matrix3().setFromMatrix4(node.parent.matrixWorld.clone().invert()));
    node.position.clone().add(delta).toArray(values,values.length);
  }
  mixer.stopAllAction();mixer.uncacheRoot(scene);
  const corrected=clip.clone(),out=corrected.tracks.find(t=>t.name===track.name);
  out.times=new Float32Array(times);out.values=new Float32Array(values);
  return clips.map(c=>c===clip?corrected:c);
}

export function fallbackClips(scene, adjustRest = true, pairedArms = false) {
  const bones = new Set();
  scene.traverse(o => { if (o.isSkinnedMesh) for (const b of o.skeleton.bones) bones.add(b); });
  if (!bones.size) return [];
  scene.updateMatrixWorld(true);
  const box = new THREE.Box3().setFromObject(scene), height = box.max.y - box.min.y;
  const hasHead = [...bones].some(b => headName.test(b.name));
  const isHead = b => headName.test(b.name) || (!hasHead && /(?:^|_)(?:neck|kubi)(?:_|$)/i.test(b.name));
  const hasNamedBody = [...bones].some(b => chestName.test(b.name) || /(?:^|_)body$/i.test(b.name));
  const isChest = b => chestName.test(b.name) || /(?:^|_)body$/i.test(b.name) || (!hasNamedBody && /^J_center$/i.test(b.name));
  // Some imports leave a held weapon under the root instead of the hand. Preserve its rest
  // transform while parenting nearby grips to the hand, so posing an arm also carries its weapon.
  const hands = [...bones].filter(b => /(?:^|_)(?:hand\d*|te|wrist)(?:_|$)/i.test(b.name) && valid(b));
  const feet = [...bones].filter(b => /(?:^|_)(?:foot|ankle|kakato)(?:_|$)/i.test(b.name));
  const footY = feet.length ? Math.min(...feet.map(b => b.getWorldPosition(V()).y)) : box.min.y;
  for (const b of bones) {
    if (!adjustRest) break;
    if (!/bamboo_sword|(?:^|_)weapon(?:0?[12])?$/i.test(b.name)) continue;
    let parent = b.parent, held = false;
    while (parent) { if (hands.includes(parent)) held = true; parent = parent.parent; }
    if (held) continue;
    const pos = b.getWorldPosition(V());
    // Unanimated imports sometimes park a weapon below the feet until a clip puts it in a hand.
    const parked = pos.y < footY - height * .1;
    if (parked) {
      const side = /02$/.test(b.name) ? 'l' : 'r';
      const grip = hands.find(h => new RegExp(`_${side}(?:_|$)`, 'i').test(h.name));
      if (grip) { grip.attach(b); b.position.set(0, 0, 0); scene.updateMatrixWorld(true); continue; }
    }
    const knownGrip = /bamboo_sword/i.test(b.name) && hands.find(h => h.name === 'J_hand_r');
    const hand = knownGrip || hands.slice().sort((a, c) => a.getWorldPosition(V()).distanceTo(pos) - c.getWorldPosition(V()).distanceTo(pos))[0];
    if (hand && (knownGrip || hand.getWorldPosition(V()).distanceTo(pos) < height * .35)) {
      hand.attach(b);
      if (knownGrip) b.position.set(0, 0, 0);
    }
  }
  const arms = [];
  for (const b of bones) {
    if (!valid(b) || !(armName.test(b.name) || /^J_(?:wing|hand01)_[lr]$/i.test(b.name))) continue;
    const child = b.children.find(c => c.isBone && /elbow|hiji|ancon|forearm/i.test(c.name)) || b.children.find(c => c.isBone);
    if (!child) continue;
    const origin = b.getWorldPosition(V()), direction = child.getWorldPosition(V()).sub(origin).normalize();
    // Only lower outstretched upper arms. Forelegs and already-posed arms keep their orientation.
    if (adjustRest && armName.test(b.name) && Math.abs(direction.x) > .8 && Math.abs(direction.y) < .3 && origin.y > box.min.y + height * .25) {
      const target = new THREE.Vector3(Math.sign(direction.x) * .45, -.82, .3).normalize();
      const delta = Q().setFromUnitVectors(direction, target);
      const parent = b.parent.getWorldQuaternion(Q());
      b.quaternion.copy(parent.clone().invert().multiply(delta).multiply(b.getWorldQuaternion(Q())));
      scene.updateMatrixWorld(true);
    }
    // Victory raises each limb to about 55 degrees above horizontal in the side
    // plane. A fixed 1.6 rad lift left lowered arms near horizontal: a T-pose.
    const side = origin.x >= box.getCenter(V()).x ? 1 : -1;
    const current = child.getWorldPosition(V()).sub(b.getWorldPosition(V()));
    const elevation = Math.atan2(current.y, Math.abs(current.x));
    arms.push({ bone: b, side, limb: armName.test(b.name), raise: Number.isFinite(elevation) ? Math.min(2.4, Math.max(.25, .96 - elevation)) : 1.6 });
  }
  // Ears and wings are often the largest moving shapes on small or blob-like
  // rigs (Lopmon's ears, PlatinumNumemon's wings). Flap the root of each chain.
  const flapName = /(?:^|_)(?:ear|mimi|wing)/i, center = box.getCenter(V());
  const isFlap = b => flapName.test(b.name) && !flapName.test(b.parent?.name || '') && !arms.some(a => a.bone === b);
  const selected = [...bones].filter(b => valid(b) && (arms.some(a => a.bone === b) || isHead(b) || isChest(b) || tailName.test(b.name) || isFlap(b)));
  if (!selected.length) return [];
  scene.updateMatrixWorld(true);
  // Box3 otherwise reuses the bounds it cached before the arms were posed.
  scene.traverse(o => { if (o.isSkinnedMesh) o.computeBoundingBox(); });
  const rest = selected.map(b => ({
    bone: b, world: b.getWorldQuaternion(Q()), parent: b.parent.getWorldQuaternion(Q()).invert(),
    arm: arms.find(a => a.bone === b), head: isHead(b), chest: isChest(b), tail: tailName.test(b.name),
    flap: isFlap(b) && (b.getWorldPosition(V()).x >= center.x ? 1 : -1),
  })).filter(r=>[...r.world.toArray(),...r.parent.toArray()].every(Number.isFinite));
  const make = (role, duration) => {
    const count = 25, times = Array.from({ length: count }, (_, i) => duration * i / (count - 1));
    const tracks = rest.map(r => {
      const values = [];
      for (let i = 0; i < count; i++) {
        const u = i / (count - 1), wave = Math.sin(u * Math.PI * 2);
        // One-shots ease back into their neutral pose; victory and idle loop seamlessly.
        const pulse = Math.sin(Math.PI * u) ** 2;
        const delta = Q();
        const turn = (axis, angle) => delta.multiply(Q().setFromAxisAngle(axis, angle));
        // Upper arms strike asymmetrically: the -X arm leads, the other guards. Two
        // arms thrust to the same height read as a T-pose at mid-action. Victory
        // raises both by their measured elevation; wings keep their paired flap.
        const lead = !pairedArms && r.arm?.limb && r.arm.side < 0, guard = !pairedArms && r.arm?.limb && !lead;
        if (role === 'idle') {
          if (r.arm) turn(Z, r.arm.side * .035 * wave);
          if (r.head) turn(X, .025 * wave);
          if (r.chest) turn(X, .018 * wave);
          if (r.tail) turn(Y, .08 * wave);
          if (r.flap) turn(Z, r.flap * .06 * wave);
        } else if (lead && role !== 'win') {
          if (role === 'attack') { turn(X, -.95 * pulse); turn(Z, r.arm.side * .12 * pulse); }
          else turn(X, -1.7 * pulse);
        } else if (guard && role !== 'win') {
          if (role === 'attack') turn(X, .22 * pulse);
          else { turn(Z, r.arm.side * .5 * pulse); turn(X, -.45 * pulse); }
        } else if (role === 'attack' || role === 'special') {
          if (r.arm) turn(X, -(role === 'special' ? 1.25 : .8) * pulse);
          if (r.head) turn(X, .12 * pulse);
          // The torso leans into the action so arm-poor bodies still read as striking.
          if (r.chest) { turn(X, (role === 'special' ? .14 : .1) * pulse); turn(Y, (role === 'special' ? .15 : .1) * Math.sin(u * Math.PI * 2) * pulse); }
          if (r.tail) turn(Y, .25 * wave * pulse);
          if (r.flap) turn(Z, r.flap * (role === 'special' ? .3 : .2) * pulse);
        } else {
          if (r.arm) { turn(Z, r.arm.side * r.arm.raise * pulse); turn(X, .2 * wave); }
          if (r.head) turn(Y, .12 * wave);
          if (r.chest) turn(Z, .055 * wave);
          if (r.tail) turn(Y, .22 * wave);
          if (r.flap) turn(Z, r.flap * .25 * Math.sin(u * Math.PI * 4) * pulse);
        }
        const q = r.parent.clone().multiply(delta).multiply(r.world).normalize();
        values.push(q.x, q.y, q.z, q.w);
      }
      return new THREE.QuaternionKeyframeTrack(`${r.bone.name}.quaternion`, times, values);
    });
    return new THREE.AnimationClip(role, duration, tracks);
  };
  return [make('idle', 2.4), make('attack', .65), make('special', 1.2), make('win', 2)];
}
