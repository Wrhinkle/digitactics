// Headless attachment regression and optional authored-pose prototype for the
// exact clipless Time Stranger Kamemon. --candidate does not modify source/assets.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');

async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),source=catalog.sources['drg:Kamemon'];
  assert.equal(source.metadata.uid,'rip-ts_kamemon');
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),n=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+n)),binary=bytes.subarray(28+n);
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));assert.equal(gltf.animations.length,0);
  const candidate=process.argv.includes('--candidate');
  const scene=gltf.scene,clips=modelClips(gltf,candidate?{...source.metadata,uid:'kamemon-carry-regression-baseline'}:source.metadata);scene.updateMatrixWorld(true);
  const H=new THREE.Box3().setFromObject(scene,true).getSize(new THREE.Vector3()).y;
  const pairs=[['J_obon','J_hand_l',1],['J_yunomi','J_hand_r',-1]],contacts=[];
  const V=()=>new THREE.Vector3();
  function vertices(){scene.updateMatrixWorld(true);const out=[];scene.traverse(mesh=>{if(!mesh.isSkinnedMesh)return;mesh.skeleton.update();const si=mesh.geometry.attributes.skinIndex,sw=mesh.geometry.attributes.skinWeight;for(let i=0;i<si.count;i++){const weights=[];for(let k=0;k<4;k++)if(sw.getComponent(i,k)>0)weights.push([mesh.skeleton.bones[si.getComponent(i,k)].name,sw.getComponent(i,k)]);out.push({mesh,index:i,weights,p:mesh.getVertexPosition(i,V()).applyMatrix4(mesh.matrixWorld)});}});return out;}
  const sourceVertices=vertices();
  for(const [propName,,] of pairs){const weighted=sourceVertices.filter(v=>v.weights.some(([name])=>name===propName));assert.equal(weighted.length,propName==='J_obon'?102:98);assert.ok(weighted.every(v=>v.weights.length===1&&v.weights[0][1]===1));}
  if(candidate){
    // Authored inference: tray in the left hand, handleless cup in the right.
    // Bend the existing forearms forward and slightly outward. No new bones or
    // scale changes; the prepared shoulders and all non-arm channels are retained.
    for(const [,handName,side] of pairs){const hand=scene.getObjectByName(handName),elbow=hand.parent,from=hand.getWorldPosition(V()).sub(elbow.getWorldPosition(V())).normalize(),to=new THREE.Vector3(side*.25,.1,1).normalize();const delta=new THREE.Quaternion().setFromUnitVectors(from,to),parent=elbow.parent.getWorldQuaternion(new THREE.Quaternion());elbow.quaternion.copy(parent.invert().multiply(delta).multiply(elbow.getWorldQuaternion(new THREE.Quaternion())));scene.updateMatrixWorld(true);}
    const prepared=vertices();
    for(const [propName,handName,side] of pairs){
      const hand=scene.getObjectByName(handName),elbow=hand.parent,prop=scene.getObjectByName(propName),handPosition=hand.getWorldPosition(V());
      // Ray through the distal palm, restricted to triangles whose weights all
      // belong to this forearm/hand. That surface follows the grip rigidly.
      const target=handPosition.clone().add(new THREE.Vector3(side*.012,0,.065).multiplyScalar(H)),ray=new THREE.Ray(target.clone().add(new THREE.Vector3(0,H,0)),new THREE.Vector3(0,-1,0));
      let contact=null,top=-Infinity;
      scene.traverse(mesh=>{if(!mesh.isSkinnedMesh)return;const si=mesh.geometry.attributes.skinIndex,sw=mesh.geometry.attributes.skinWeight,indices=mesh.geometry.index,position=mesh.geometry.attributes.position;const rigid=i=>{for(let k=0;k<4;k++)if(sw.getComponent(i,k)>0&&! [hand,elbow].includes(mesh.skeleton.bones[si.getComponent(i,k)]))return false;return true;};for(let i=0;i<(indices?.count||position.count);i+=3){const ids=[0,1,2].map(k=>indices?indices.getX(i+k):i+k);if(!ids.every(rigid))continue;const points=ids.map(index=>mesh.getVertexPosition(index,V()).applyMatrix4(mesh.matrixWorld)),hit=ray.intersectTriangle(...points,false,V());if(hit&&hit.y>top){top=hit.y;contact=hit.clone();}}});
      assert.ok(contact,`${handName}: distal palm support ray must hit its actual surface`);
      const supportLocal=hand.worldToLocal(contact.clone());
      // The tray is supported off-center, moving its broad back rim away from
      // the torso. The small cup rests directly on the right palm. Both retain
      // the upright source orientation; offsets are measured, not origin snaps.
      const center=contact.clone().add(propName==='J_obon'?new THREE.Vector3(.025,0,.06).multiplyScalar(H):V());
      const propLocalBottom=Math.min(...prepared.filter(v=>v.weights.some(([name])=>name===propName)).map(v=>v.p.y));
      center.y+=prop.getWorldPosition(V()).y-propLocalBottom;
      hand.attach(prop);prop.position.copy(hand.worldToLocal(center));scene.updateMatrixWorld(true);
      contacts.push({prop:propName,hand:handName,supportLocal:supportLocal.toArray(),propPosition:prop.position.toArray(),propQuaternion:prop.quaternion.toArray(),elbowQuaternion:elbow.quaternion.toArray(),supportWorldHeights:contact.toArray().map(v=>v/H)});
    }
    // A service carry is a restrained gesture: keep the generated shoulder
    // actions recognizable without raising the supported dishes into a T-pose.
    for(const clip of clips)for(const track of clip.tracks){if(!/^J_arm_[lr]\.quaternion$/.test(track.name))continue;const base=new THREE.Quaternion().fromArray(track.values),gain=clip.name==='idle'?1:clip.name==='win'?.16:.25;for(let i=0;i<track.values.length;i+=4)new THREE.Quaternion().fromArray(track.values,i).slerp(base,1-gain).toArray(track.values,i);}
  }
  for(const [propName,handName] of pairs)assert.equal(scene.getObjectByName(propName).parent,scene.getObjectByName(handName),`${propName} must follow its authored carry hand`);
  // These anchors were ray-intersected with the actual distal palm triangles,
  // then expressed in hand space. Verify both surfaces, rather than blessing a
  // parent relationship whose dish could still hover far away from the hand.
  vertices();const contactErrors=[];
  for(const [propName,handName,side] of pairs){
    const hand=scene.getObjectByName(handName),point=hand.localToWorld(new THREE.Vector3(side*.0015104227667474,.0004992617626851381,.000645883098626236));
    function surfaceDistance(allowed){let distance=Infinity;scene.traverse(mesh=>{if(!mesh.isSkinnedMesh)return;const si=mesh.geometry.attributes.skinIndex,sw=mesh.geometry.attributes.skinWeight,indices=mesh.geometry.index,position=mesh.geometry.attributes.position;const accepted=i=>{for(let k=0;k<4;k++)if(sw.getComponent(i,k)>0&&!allowed.includes(mesh.skeleton.bones[si.getComponent(i,k)].name))return false;return true;};for(let i=0;i<(indices?.count||position.count);i+=3){const ids=[0,1,2].map(k=>indices?indices.getX(i+k):i+k);if(!ids.every(accepted))continue;const triangle=new THREE.Triangle(...ids.map(index=>mesh.getVertexPosition(index,V()).applyMatrix4(mesh.matrixWorld)));distance=Math.min(distance,triangle.closestPointToPoint(point,V()).distanceTo(point)/H);}});return distance;}
    const handGap=surfaceDistance([handName,hand.parent.name]),propGap=surfaceDistance([propName]);
    assert.ok(handGap<1e-6,`${handName}: support anchor must lie on the actual palm surface`);assert.ok(propGap<.002,`${propName}: dish underside must touch the support surface (${propGap}H)`);
    contactErrors.push({prop:propName,handSurfaceGapHeights:handGap,propSurfaceGapHeights:propGap});
  }
  scene.updateMatrixWorld(true);const rest=[];scene.traverse(o=>rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone()}));
  const initial=vertices(),mixer=new THREE.AnimationMixer(scene),baseline=new Map(pairs.map(([name])=>{const bone=scene.getObjectByName(name);return [name,initial.filter(v=>v.weights.some(([n])=>n===name)).map(v=>bone.worldToLocal(v.p.clone()))];}));
  let samples=0,maxShapeError=0,maxLocalAttachmentError=0,maxScaleError=0;const roleMotion=[];
  for(const clip of clips){let moved=0;for(let i=0;i<=64;i++){mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(clip.duration*i/64);const now=vertices();
      for(const [propName,handName] of pairs){const prop=scene.getObjectByName(propName),hand=scene.getObjectByName(handName),saved=rest.find(r=>r.o===prop);maxLocalAttachmentError=Math.max(maxLocalAttachmentError,prop.position.distanceTo(saved.p)/H,...prop.quaternion.toArray().map((v,k)=>Math.abs(v-saved.q.toArray()[k])));const ps=now.filter(v=>v.weights.some(([name])=>name===propName));ps.forEach((v,index)=>{maxShapeError=Math.max(maxShapeError,prop.worldToLocal(v.p.clone()).distanceTo(baseline.get(propName)[index])/H);assert.ok(v.p.toArray().every(Number.isFinite));});const start=initial.filter(v=>v.weights.some(([name])=>name===propName));moved=Math.max(moved,...ps.map((v,index)=>v.p.distanceTo(start[index].p)/H));assert.ok(hand.parent.isBone);}
      for(const r of rest)maxScaleError=Math.max(maxScaleError,r.o.scale.distanceTo(r.s));samples++;
    }roleMotion.push({role:clip.name,maxPropTravelHeights:moved});}
  mixer.stopAllAction();mixer.uncacheRoot(scene);
  assert.ok(maxShapeError<1e-6,'Rigid props must not stretch');assert.ok(maxLocalAttachmentError<1e-6,'Prop contact frame must stay attached');assert.equal(maxScaleError,0);assert.ok(roleMotion.every(r=>r.maxPropTravelHeights>.001),'Every fallback role must carry the props');
  console.log(JSON.stringify({sourceKey:'drg:Kamemon',candidate,sourceHeight:H,contacts,contactErrors,samples,maxShapeError,maxLocalAttachmentError,maxScaleError,roleMotion,scope:'Authored carry inference; CPU support contact/rigid attachment/finite geometry only. Rendered contact and silhouette review remain required.'},null,2));
}
main().catch(error=>{console.error(error.name+': '+error.message);process.exitCode=1;});

