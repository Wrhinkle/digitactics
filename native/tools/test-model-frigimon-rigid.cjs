// Exact clipless DW1 Frigimon: original geometry, joined caps, real articulation,
// floor, closure and interruption blends. No renderer/build/export side effects.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const {pathToFileURL}=require('node:url');
const R=path.resolve(__dirname,'../..'),D=path.join(R,'native/artifacts/dw1-frigimon-oct20');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex'),failures=[];let checks=0;
const check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
async function main(){
 const c=JSON.parse(fs.readFileSync(path.join(R,'native/content/models.json'))),s=c.sources['extra:DW1_Frigimon'],asset=c.assets[s.asset];
 const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
 const script=fs.readFileSync(path.join(R,'js/model-rigid-parts.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(R,'vendor/three/build/three.module.js')).href}'`);
 const {rigidPartRig}=await import('data:text/javascript;base64,'+Buffer.from(script).toString('base64'));
 const packed=fs.readFileSync(path.join(R,'native/artifacts/model-content',asset.file));
 check(sha(packed)===asset.sha256,'Pinned exported asset matches catalog');
 check(sha(fs.readFileSync(path.join(R,s.asset)))==='364413e2557346b5328a90af9c63b99b33e81a3d608c2fbb1e394302400f5d4d','Original 12-part source unchanged');
 const bytes=zlib.gunzipSync(packed),n=bytes.readUInt32LE(12),j=JSON.parse(bytes.subarray(20,20+n)),binary=bytes.subarray(28+n);
 const parse=async()=>{const p=new GLTFParser(j,{manager:THREE.DefaultLoadingManager});p.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();p.loadImageSource=async()=>new THREE.Texture();p.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);return new Promise((resolve,reject)=>p.parse(resolve,reject));};
 const gltf=await parse();gltf.scene.updateMatrixWorld(true);const nodes=[];gltf.scene.traverse(o=>{if(o.isMesh)nodes.push(o);});
 const immutable=()=>sha(JSON.stringify(nodes.map(o=>({index:Array.from(o.geometry.index.array),attributes:Object.fromEntries(Object.entries(o.geometry.attributes).map(([k,v])=>[k,{array:Array.from(v.array),normalized:v.normalized,itemSize:v.itemSize}])),material:{type:o.material.type,color:o.material.color.toArray(),opacity:o.material.opacity,transparent:o.material.transparent,alphaTest:o.material.alphaTest,side:o.material.side,map:!!o.material.map}}))));
 const before=immutable(),original=nodes.map(o=>({p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone(),parent:o.parent}));
 check(nodes.length===12&&!gltf.animations.length&&!j.skins?.length,'Expected exact rigid clipless assembly');
 const clips=modelClips(gltf,s.metadata),spec=rigidPartRig(s.metadata.uid);
 check(clips.length===5&&['idle','run','attack','special','win'].every(role=>clips.some(c=>c.name===role)),'Five required articulated roles');
 check(!!spec,'Exact UID anatomy policy exists');
 const report={sourceKey:'extra:DW1_Frigimon',scope:'Authored exact-source assembly and bounded rigid animation; visual quality pending native capture.',originalSha256:asset.sourceSha256,checks,failures,roles:[]};
 if(!spec||!clips.length){fs.mkdirSync(D,{recursive:true});fs.writeFileSync(path.join(D,'frigimon-rigid-checks.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));process.exitCode=1;return;}
 check(immutable()===before,'Every imported vertex/normal/UV/index/material unchanged');
 check(nodes.every((o,i)=>o.quaternion.equals(original[i].q)&&o.scale.equals(original[i].s)&&o.parent===original[i].parent),'Assembly changes only original node translations');
 const nodeByName=new Map(nodes.map((o,i)=>[o.name,i]));
 const parts=nodes.map(o=>({o,local:Array.from({length:o.geometry.attributes.position.count},(_,i)=>o.getVertexPosition(i,new THREE.Vector3())),faces:Array.from({length:o.geometry.index.count/3},(_,i)=>[0,1,2].map(k=>o.geometry.index.getX(i*3+k)))}));
 const rest=nodes.map(o=>({p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone(),m:o.matrixWorld.clone()}));gltf.scene.updateMatrixWorld(true);rest.forEach((r,i)=>r.m.copy(nodes[i].matrixWorld));
 const pose=()=>parts.map(p=>p.local.map(v=>v.clone().applyMatrix4(p.o.matrixWorld)));
 const assembled=pose(),box=new THREE.Box3().setFromPoints(assembled.flat()),height=box.max.y-box.min.y;
 const ray=new THREE.Ray(),dir=new THREE.Vector3(.411,.827,.383).normalize(),target=new THREE.Vector3(),tri=new THREE.Triangle();
 function interior(v,id,points){ray.set(v,dir);const ds=[];for(const face of parts[id].faces)if(ray.intersectTriangle(...face.map(i=>points[id][i]),false,target)){const d=v.distanceTo(target);if(!ds.some(x=>Math.abs(d-x)<1e-5))ds.push(d);}return ds.length%2===1;}
 function clearance(v,id,points){let d=Infinity;for(const face of parts[id].faces){tri.set(...face.map(i=>points[id][i])).closestPointToPoint(v,target);d=Math.min(d,v.distanceTo(target));}return d;}
 const links=spec.parts.filter(p=>p.parent).map(p=>{const child=nodeByName.get(p.name),parent=nodeByName.get(p.parent),pivot=new THREE.Vector3(...p.pivot);check(interior(pivot,child,assembled)&&interior(pivot,parent,assembled),'Shared joint is inside both original caps',p.name);return {name:p.name,child,parent,a:pivot.clone().applyMatrix4(rest[child].m.clone().invert()),b:pivot.clone().applyMatrix4(rest[parent].m.clone().invert()),depth:Math.min(clearance(pivot,child,assembled),clearance(pivot,parent,assembled))};});
 report.joints=links.map(p=>({name:p.name,sharedInteriorRadiusHeight:p.depth/height}));
 const mixer=new THREE.AnimationMixer(gltf.scene),reset=()=>{mixer.stopAllAction();nodes.forEach((o,i)=>{o.position.copy(rest[i].p);o.quaternion.copy(rest[i].q);o.scale.copy(rest[i].s);});};
 function sample(clip,time){reset();const a=mixer.clipAction(clip);a.reset().setLoop(THREE.LoopOnce,1);a.clampWhenFinished=true;a.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);return pose();}
 const neutral=sample(clips.find(c=>c.name==='idle'),0),floor=Math.min(...neutral.flat().map(v=>v.y));
 // Independent anatomy guard: both fists must rest below their shoulders.
 for(const [hand,shoulder] of [[4,2],[7,5]]){const center=neutral[hand].reduce((a,v)=>a.add(v),new THREE.Vector3()).divideScalar(neutral[hand].length),pivot=new THREE.Vector3(...spec.parts.find(p=>nodeByName.get(p.name)===shoulder).pivot);check(center.y<pivot.y-height*.1,'Neutral fist below shoulder',hand);}
 const edges=parts.map(p=>{const ids=new Set();return p.faces.flatMap(f=>f.map((v,i)=>[v,f[(i+1)%3]]).filter(e=>{const key=e.slice().sort((a,b)=>a-b).join(':');if(ids.has(key))return false;ids.add(key);return true;}));});
 const visual=[];
 for(const clip of clips){
  const opening=sample(clip,0),stats={role:clip.name,samples:0,maxAnchorErrorHeight:0,maxRigidEdgeRelativeError:0,minimumFloorHeight:Infinity,maxBoundsHeight:0,internalRmsHeight:0,footTravelHeight:[0,0],closureHeight:0};
  const root=nodeByName.get(spec.parts.find(p=>p.parent===null).name),openRoot=nodes[root].matrixWorld.clone(),openingRelative=opening.map(ps=>ps.map(v=>v.clone().applyMatrix4(openRoot.clone().invert())));
  for(let i=0;i<=128;i++){
   const ps=sample(clip,clip.duration*i/128),bounds=new THREE.Box3().setFromPoints(ps.flat());stats.samples++;
   check(ps.flat().every(v=>v.toArray().every(Number.isFinite)),'Finite skinned/rigid geometry '+clip.name);
   stats.minimumFloorHeight=Math.min(stats.minimumFloorHeight,(bounds.min.y-floor)/height);stats.maxBoundsHeight=Math.max(stats.maxBoundsHeight,bounds.getSize(new THREE.Vector3()).y/height);
   for(const l of links){const a=l.a.clone().applyMatrix4(nodes[l.child].matrixWorld),b=l.b.clone().applyMatrix4(nodes[l.parent].matrixWorld);stats.maxAnchorErrorHeight=Math.max(stats.maxAnchorErrorHeight,a.distanceTo(b)/height);}
   let sum=0,count=0;const inverse=nodes[root].matrixWorld.clone().invert();
   ps.forEach((vs,k)=>vs.forEach((v,n)=>{if(k===root)return;sum+=v.clone().applyMatrix4(inverse).distanceToSquared(openingRelative[k][n]);count++;}));stats.internalRmsHeight=Math.max(stats.internalRmsHeight,Math.sqrt(sum/count)/height);
   for(const [k,id] of [9,11].entries())stats.footTravelHeight[k]=Math.max(stats.footTravelHeight[k],...ps[id].map((v,n)=>v.clone().applyMatrix4(inverse).distanceTo(openingRelative[id][n])*rest[root].s.x/height));
   edges.forEach((es,k)=>es.forEach(([a,b])=>{const before=opening[k][a].distanceTo(opening[k][b]);if(before>height*1e-6)stats.maxRigidEdgeRelativeError=Math.max(stats.maxRigidEdgeRelativeError,Math.abs(ps[k][a].distanceTo(ps[k][b])/before-1));}));
   if(i===128)ps.forEach((vs,k)=>vs.forEach((v,n)=>stats.closureHeight=Math.max(stats.closureHeight,v.distanceTo(opening[k][n])/height)));
   if(i%32===0)visual.push({role:clip.name,fraction:i/128,parts:parts.map((p,k)=>({index:k,name:p.o.name,points:ps[k].map(v=>v.toArray()),triangles:p.faces,bounds:{min:new THREE.Box3().setFromPoints(ps[k]).min.toArray(),max:new THREE.Box3().setFromPoints(ps[k]).max.toArray()}}))});
  }
  check(stats.maxAnchorErrorHeight<.0005,clip.name+' interpolated joints remain connected',stats.maxAnchorErrorHeight);
  check(stats.maxRigidEdgeRelativeError<.00002,clip.name+' original pieces do not grow/deform',stats.maxRigidEdgeRelativeError);
  check(stats.minimumFloorHeight>-.0005,clip.name+' no floor penetration',stats.minimumFloorHeight);
  check(stats.maxBoundsHeight<1.3,clip.name+' bounded height',stats.maxBoundsHeight);
  check(stats.closureHeight<.00002,clip.name+' exact shared endpoint',stats.closureHeight);
  check(stats.internalRmsHeight>(clip.name==='idle'?.001:.012),clip.name+' actual internal articulation',stats.internalRmsHeight);
  if(clip.name==='run')check(stats.footTravelHeight.every(v=>v>.055),'Both feet articulate relative to torso',stats.footTravelHeight);
  report.roles.push(stats);
 }
 report.blends={samples:0,maxJointDistanceHeight:0,maxOverlapRadiusFraction:0};
 for(const from of clips)for(const to of clips)if(from!==to)for(const weight of [.25,.5,.75])for(const at of [0,.07,.17,.27,.43,.52,.67,.84,1])for(const bt of [0,.07,.17,.27,.43,.52,.67,.84,1]){
  reset();const a=mixer.clipAction(from),b=mixer.clipAction(to);a.reset().setEffectiveWeight(1-weight).play();b.reset().setEffectiveWeight(weight).play();a.time=from.duration*at;b.time=to.duration*bt;mixer.update(0);gltf.scene.updateMatrixWorld(true);report.blends.samples++;
  for(const l of links){const d=l.a.clone().applyMatrix4(nodes[l.child].matrixWorld).distanceTo(l.b.clone().applyMatrix4(nodes[l.parent].matrixWorld));report.blends.maxJointDistanceHeight=Math.max(report.blends.maxJointDistanceHeight,d/height);if(d/(2*l.depth)>report.blends.maxOverlapRadiusFraction){report.blends.maxOverlapRadiusFraction=d/(2*l.depth);report.blends.worst={from:from.name,to:to.name,weight,fromFraction:at,toFraction:bt,joint:l.name,distanceHeight:d/height};}}
 }
 // Each joint has an inscribed ball inside each rigid cap. The balls still
 // overlap when blended anchor separation is less than their combined radii.
 check(report.blends.maxOverlapRadiusFraction<1,'Interrupted blends preserve overlapping cap volumes',report.blends);
 check(immutable()===before,'Every source attribute/material survives all playback');
 const control=await parse();check(modelClips(control,{...s.metadata,uid:s.metadata.uid+'-control'}).length===0,'Other UIDs unchanged');
 report.checks=checks;report.failures=failures;report.geometrySha256=before;
 fs.mkdirSync(D,{recursive:true});fs.writeFileSync(path.join(D,'frigimon-rigid-checks.json'),JSON.stringify(report,null,2));fs.writeFileSync(path.join(D,'Frigimon_prepared.motion.json'),JSON.stringify({frames:visual}));
 console.log(JSON.stringify(report,null,2));if(failures.length)process.exitCode=1;
}
main().catch(e=>{console.error(e);process.exitCode=1;});
