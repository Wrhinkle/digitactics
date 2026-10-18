// CPU-only diagnostic for the exact Pabumon rip. Does not alter source or exports.
// Ratios compare each indexed triangle with the same triangle in authored idle(0).
// Default: weights 1/.2/.3/.4/.6/.7/.8 plus center-exclusion controls.
// --isolate-only compares translation-only, rotation-only and raw-center controls.
// --coherent-only measures a rigid world-axis rock/bob shared by body roots.
// --weights=1,.05,.1,.2 --no-center-test --output=native/artifacts/custom.json
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const root=path.resolve(__dirname,'../..'),sourceKey='catalog:CS_Pabumon:chr387';
const args=process.argv.slice(2),option=name=>args.find(a=>a.startsWith(name+'='))?.slice(name.length+1);
const weights=(option('--weights')||'1,.2,.3,.4,.6,.7,.8').split(',').map(Number);
if(weights.some(w=>!Number.isFinite(w)||w<0||w>1))throw Error('Weights must be between zero and one.');
const bodyBone=/^J_(?:center|body[1-4]|leg[FLR][12]|tail[1-3])$/;
const percentile=(values,p)=>values.length?values[Math.min(values.length-1,Math.floor((values.length-1)*p))]:0;
const dist=(a,b)=>Math.hypot(a[0]-b[0],a[1]-b[1],a[2]-b[2]);
function shape(p,a,b,c){
  const A=p[a],B=p[b],C=p[c],u=[B[0]-A[0],B[1]-A[1],B[2]-A[2]],v=[C[0]-A[0],C[1]-A[1],C[2]-A[2]];
  const edges=[dist(A,B),dist(B,C),dist(C,A)];
  const area=Math.hypot(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0])/2;
  return {edges,area,quality:4*Math.sqrt(3)*area/edges.reduce((s,e)=>s+e*e,0)};
}
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),source=catalog.sources[sourceKey];
  if(source?.metadata.uid!=='rip-cat_cs_pabumon_chr387')throw Error('Expected exact Pabumon UID.');
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file)));
  const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
  parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject)),clips=gltf.animations;
  const idle=clips.find(c=>c.name==='idle'),templateMixer=new THREE.AnimationMixer(gltf.scene),rest=[],meshes=[];
  // prepareTemplate leaves the assembled idle pose on the scene. A fresh production
  // mixer captures that pose for action channels that are not authored. Stopping
  // the preparation mixer would restore bind locals and change that inheritance.
  templateMixer.clipAction(idle).play();templateMixer.update(0);gltf.scene.updateMatrixWorld(true);
  gltf.scene.traverse(o=>{rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone()});if(o.isSkinnedMesh)meshes.push(o);});
  const mixer=new THREE.AnimationMixer(gltf.scene);
  const sample=(clip,time)=>{
    mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}
    const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);
    gltf.scene.updateMatrixWorld(true);for(const mesh of meshes)mesh.skeleton.update();
    return meshes.map(mesh=>Array.from({length:mesh.geometry.attributes.position.count},(_,i)=>mesh.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(mesh.matrixWorld).toArray()));
  };
  const baseline=sample(idle,0),idlePose=new Map(rest.map(r=>[r.o.name,{p:r.o.position.clone(),q:r.o.quaternion.clone(),s:r.o.scale.clone()}]));
  const bounds=new THREE.Box3();for(const points of baseline)for(const p of points)bounds.expandByPoint(new THREE.Vector3(...p));
  const height=bounds.max.y-bounds.min.y;
  const rootBone=rest.find(r=>r.o.name==='J_root').o,rootWorldRotation=rootBone.getWorldQuaternion(new THREE.Quaternion()),rootInverse=rootBone.matrixWorld.clone().invert();
  const bobLocal=new THREE.Vector3(0,height*.04,0).applyMatrix4(rootInverse).sub(new THREE.Vector3().applyMatrix4(rootInverse));
  const meta=meshes.map((mesh,m)=>{
    const g=mesh.geometry,ids=g.index?Array.from(g.index.array):Array.from({length:g.attributes.position.count},(_,i)=>i),influences=[];
    const categories=Array.from({length:g.attributes.position.count},(_,i)=>{
      const names=[];for(let j=0;j<4;j++)if(g.attributes.skinWeight.getComponent(i,j)>1e-5)names.push(mesh.skeleton.bones[g.attributes.skinIndex.getComponent(i,j)].name);
      influences.push(names);return names.some(n=>/^J_float[1-4]$/.test(n))?'bubble':names.some(n=>/mouth/i.test(n))?'mouth':'body';
    });
    const triangles=[];for(let i=0;i<ids.length;i+=3){const tri=ids.slice(i,i+3),ref=shape(baseline[m],...tri);if(ref.area<height*height*1e-12||Math.min(...ref.edges)<height*1e-7)continue;
      triangles.push({indices:tri,index:i/3,category:tri.some(v=>categories[v]==='bubble')?'bubble':tri.some(v=>categories[v]==='mouth')?'mouth':'body',ref});}
    return {name:mesh.name,categories,influences,triangles};
  });
  const measure=(positions,original)=>{
    const groups={};
    for(const category of ['body','mouth','bubble']){
      const edges=[],areas=[],quality=[],points=[],changes=[],originalChanges=[];let maxEdge={ratio:0},maxArea={ratio:0},refArea=0,stretchedArea=0,collapsedArea=0;
      meta.forEach((mesh,m)=>{
        mesh.categories.forEach((cat,i)=>{if(cat===category){points.push(positions[m][i]);changes.push(dist(positions[m][i],baseline[m][i])/height);if(original)originalChanges.push(dist(positions[m][i],original[m][i])/height);}});
        for(const tri of mesh.triangles){if(tri.category!==category)continue;const now=shape(positions[m],...tri.indices),ratio=now.area/tri.ref.area;
          areas.push(ratio);quality.push(now.quality/tri.ref.quality);refArea+=tri.ref.area;
          let stretched=false;for(let e=0;e<3;e++){const edge=now.edges[e]/tri.ref.edges[e];edges.push(edge);if(edge>2)stretched=true;if(edge>maxEdge.ratio)maxEdge={ratio:edge,mesh:m,triangle:tri.index,edge:e};}
          if(stretched)stretchedArea+=tri.ref.area;if(ratio<.25)collapsedArea+=tri.ref.area;
          if(ratio>maxArea.ratio)maxArea={ratio,mesh:m,triangle:tri.index};
        }
      });
      if(!points.length)continue;
      edges.sort((a,b)=>a-b);areas.sort((a,b)=>a-b);quality.sort((a,b)=>a-b);
      const min=[0,1,2].map(k=>Math.min(...points.map(p=>p[k]))/height),max=[0,1,2].map(k=>Math.max(...points.map(p=>p[k]))/height);
      const deltaVectors=[];meta.forEach((mesh,m)=>mesh.categories.forEach((cat,i)=>{if(cat===category)deltaVectors.push(positions[m][i].map((p,k)=>(p-baseline[m][i][k])/height));}));
      const centroidDelta=[0,1,2].map(k=>deltaVectors.reduce((sum,p)=>sum+p[k],0)/deltaVectors.length);
      const centeredRms=Math.sqrt(deltaVectors.reduce((sum,p)=>sum+p.reduce((s,v,k)=>s+(v-centroidDelta[k])**2,0),0)/deltaVectors.length);
      groups[category]={vertices:points.length,triangles:areas.length,edge:{min:edges[0],p01:percentile(edges,.01),p50:percentile(edges,.5),p95:percentile(edges,.95),p99:percentile(edges,.99),max:maxEdge},
        area:{min:areas[0],p01:percentile(areas,.01),p50:percentile(areas,.5),p95:percentile(areas,.95),p99:percentile(areas,.99),max:maxArea},
        qualityRatioP01:percentile(quality,.01),fractionAreaWithEdgeOver2:stretchedArea/refArea,fractionCollapsedArea:collapsedArea/refArea,
        normalizedSize:min.map((v,k)=>max[k]-v),centroidDisplacement:Math.hypot(...centroidDelta),centeredDisplacementRms:centeredRms,displacementRms:Math.sqrt(changes.reduce((s,v)=>s+v*v,0)/changes.length),displacementMax:Math.max(...changes),
        differenceFromOriginalMax:originalChanges.length?Math.max(...originalChanges):0};
    }return groups;
  };
  const blend=(clip,weight,includeCenter=true,mode='full')=>{
    if(weight===1)return clip;
    const out=clip.clone();
    for(const track of out.tracks){const dot=track.name.lastIndexOf('.'),name=track.name.slice(0,dot),prop=track.name.slice(dot+1);if(!bodyBone.test(name)||(!includeCenter&&name==='J_center')||!['position','quaternion'].includes(prop))continue;
      if(mode==='position'&&prop!=='position'||mode==='quaternion'&&prop!=='quaternion'||mode==='child-position'&&(prop!=='position'||name==='J_center')||mode==='keep-center-rotation'&&prop==='quaternion'&&name==='J_center')continue;
      const neutral=idlePose.get(name);if(!neutral)throw Error('Missing baseline bone '+name);
      if(prop==='position')for(let i=0;i<track.values.length;i+=3)new THREE.Vector3().fromArray(track.values,i).lerp(neutral.p,1-weight).toArray(track.values,i);
      else for(let i=0;i<track.values.length;i+=4)neutral.q.clone().slerp(new THREE.Quaternion().fromArray(track.values,i),weight).toArray(track.values,i);
    }return out;
  };
  const coherent=(clip,times)=>{
    const tracks=clip.tracks.filter(t=>{const dot=t.name.lastIndexOf('.');return !bodyBone.test(t.name.slice(0,dot))||!['position','quaternion','scale'].includes(t.name.slice(dot+1));}).map(t=>t.clone());
    const pivot=idlePose.get('J_center').p,axis=new THREE.Vector3(clip.name==='attack'?1:0,0,clip.name==='win'?1:0);
    for(const record of rest.filter(r=>r.o.isBone&&bodyBone.test(r.o.name))){
      const neutral=idlePose.get(record.o.name),positions=[],rotations=[],scales=[];
      for(const time of times){
        const u=Math.max(0,Math.min(1,time/clip.duration)),pulse=Math.sin(Math.PI*u)**2;
        const angle=clip.name==='attack'?.15*pulse:.12*Math.sin(2*Math.PI*u);
        const rotation=rootWorldRotation.clone().invert().multiply(new THREE.Quaternion().setFromAxisAngle(axis,angle)).multiply(rootWorldRotation);
        const p=neutral.p.clone(),q=neutral.q.clone();
        if(record.o.parent===rootBone){p.sub(pivot).applyQuaternion(rotation).add(pivot).addScaledVector(bobLocal,pulse);q.premultiply(rotation);}
        p.toArray(positions,positions.length);q.toArray(rotations,rotations.length);neutral.s.toArray(scales,scales.length);
      }
      tracks.push(new THREE.VectorKeyframeTrack(record.o.name+'.position',times,positions),new THREE.QuaternionKeyframeTrack(record.o.name+'.quaternion',times,rotations),new THREE.VectorKeyframeTrack(record.o.name+'.scale',times,scales));
    }
    return new THREE.AnimationClip(clip.name,clip.duration,tracks);
  };
  const summarize=frames=>{
    const out={};for(const cat of ['body','mouth','bubble']){
      const group=frames.filter(f=>f.groups[cat]);if(!group.length)continue;
      const peak=selector=>{const f=group.reduce((a,b)=>selector(a.groups[cat])>=selector(b.groups[cat])?a:b);return {value:selector(f.groups[cat]),time:f.time};};
      const trough=selector=>{const f=group.reduce((a,b)=>selector(a.groups[cat])<=selector(b.groups[cat])?a:b);return {value:selector(f.groups[cat]),time:f.time};};
      out[cat]={minEdge:trough(g=>g.edge.min),minArea:trough(g=>g.area.min),maxEdge:peak(g=>g.edge.max.ratio),maxArea:peak(g=>g.area.max.ratio),maxEdgeP99:peak(g=>g.edge.p99),maxAreaP99:peak(g=>g.area.p99),maxStretchedAreaFraction:peak(g=>g.fractionAreaWithEdgeOver2),maxCollapsedAreaFraction:peak(g=>g.fractionCollapsedArea),maxDisplacementRms:peak(g=>g.displacementRms),maxCenteredDisplacementRms:peak(g=>g.centeredDisplacementRms),maxCentroidDisplacement:peak(g=>g.centroidDisplacement),maxOriginalDifference:peak(g=>g.differenceFromOriginalMax),maxSizeX:peak(g=>g.normalizedSize[0]),maxSizeY:peak(g=>g.normalizedSize[1]),maxSizeZ:peak(g=>g.normalizedSize[2])};
    }return out;
  };
  const auditFile=path.join(root,'native/artifacts/model-visual-audit-v3/0206.json'),audit=JSON.parse(fs.readFileSync(auditFile));
  const report={sourceKey,uid:source.metadata.uid,asset:source.asset,definition:'retained authored weight: 0=idle(0), 1=unaltered authored; position lerp/quaternion slerp on exact body-bone tracks only',height,
    bones:rest.filter(r=>r.o.isBone).map(r=>({name:r.o.name,parent:r.o.parent?.name})),meshes:meta.map(m=>({name:m.name,vertices:m.categories.length,categories:m.categories.reduce((o,c)=>(o[c]=(o[c]||0)+1,o),{}),triangleCategories:m.triangles.reduce((o,t)=>(o[t.category]=(o[t.category]||0)+1,o),{})})),roles:{}};
  const idleFrames=Array.from({length:121},(_,i)=>{const time=i*idle.duration/120;return {time,groups:measure(sample(idle,time))};});
  report.idle={duration:idle.duration,summary:summarize(idleFrames)};
  for(const role of ['attack','win']){
    const clip=clips.find(c=>c.name===role),times=[...new Set([0,clip.duration,...clip.tracks.flatMap(t=>Array.from(t.times)),...Array.from({length:121},(_,i)=>i*clip.duration/120),...audit.Samples.filter(s=>s.Role===role).map(s=>s.Time)])].sort((a,b)=>a-b);
    const originals=times.map(time=>sample(clip,time)),candidates=[];
    for(const weight of args.includes('--isolate-only')||args.includes('--coherent-only')?[]:weights){
      const candidate=blend(clip,weight),frames=times.map((time,i)=>({time,groups:measure(weight===1?originals[i]:sample(candidate,time),originals[i])}));
      candidates.push({retainedWeight:weight,includeCenter:true,summary:summarize(frames),frames});mixer.uncacheClip(candidate);
    }
    // Parent motion also moves child effects: quantify whether excluding center is safer.
    for(const weight of args.includes('--no-center-test')||args.includes('--isolate-only')||args.includes('--coherent-only')?[]:[.2,.3,.4]){
      const candidate=blend(clip,weight,false),frames=times.map((time,i)=>({time,groups:measure(sample(candidate,time),originals[i])}));
      candidates.push({retainedWeight:weight,includeCenter:false,summary:summarize(frames),frames});mixer.uncacheClip(candidate);
    }
    if(args.includes('--isolate-only'))for(const [mode,weight] of [['position',0],['position',.2],['position',.3],['position',.4],['quaternion',.2],['quaternion',.3],['quaternion',.4],['child-position',0],['child-position',.2],['keep-center-rotation',.2]]){
      const candidate=blend(clip,weight,true,mode),frames=times.map((time,i)=>({time,groups:measure(sample(candidate,time),originals[i])}));
      candidates.push({mode,retainedWeight:weight,includeCenter:true,summary:summarize(frames),frames});mixer.uncacheClip(candidate);
    }
    if(args.includes('--coherent-only')){
      const keyTimes=times.filter(t=>t<=clip.duration),candidate=coherent(clip,keyTimes);
      // Midpoints exercise interpolated poses in addition to authored/common keys.
      const tested=[...new Set([...times,...keyTimes.slice(1).map((t,i)=>(t+keyTimes[i])/2)])].sort((a,b)=>a-b);
      const frames=tested.map(time=>({time,groups:measure(sample(candidate,time),sample(clip,time))}));
      const protectedTracks=clip.tracks.filter(t=>!bodyBone.test(t.name.slice(0,t.name.lastIndexOf('.'))));
      const unchangedProtectedTracks=protectedTracks.every(t=>{const other=candidate.tracks.find(c=>c.name===t.name);return other&&Buffer.from(t.times.buffer,t.times.byteOffset,t.times.byteLength).equals(Buffer.from(other.times.buffer,other.times.byteOffset,other.times.byteLength))&&Buffer.from(t.values.buffer,t.values.byteOffset,t.values.byteLength).equals(Buffer.from(other.values.buffer,other.values.byteOffset,other.values.byteLength));});
      candidates.push({mode:'coherent',parameters:{rockAxis:clip.name==='attack'?'worldX':'worldZ',rockRadians:clip.name==='attack'?.15:.12,bobHeightFraction:.04,bob:'sin(pi*u)^2',bodyRootNames:rest.filter(r=>bodyBone.test(r.o.name)&&r.o.parent===rootBone).map(r=>r.o.name)},samples:tested.length,protectedTrackCount:protectedTracks.length,unchangedProtectedTracks,summary:summarize(frames),frames});
      mixer.uncacheClip(candidate);
    }
    const bodyTracks=clip.tracks.filter(t=>bodyBone.test(t.name.slice(0,t.name.lastIndexOf('.')))).map(t=>({name:t.name,keys:t.times.length}));
    const worstDetails=positions=>{
      const stats=measure(positions),entry=stats.body.edge.max,mesh=meta[entry.mesh],triangle=mesh.triangles.find(t=>t.index===entry.triangle);
      return {...entry,referenceEdges:triangle.ref.edges.map(e=>e/height),referenceArea:triangle.ref.area/height**2,influences:triangle.indices.map(i=>mesh.influences[i])};
    };
    const originalWorstIndex=originals.reduce((best,p,i)=>measure(p).body.edge.max.ratio>measure(originals[best]).body.edge.max.ratio?i:best,0);
    report.roles[role]={duration:clip.duration,samples:times.length,bodyTracks,worstOriginalTriangle:worstDetails(originals[originalWorstIndex]),candidates};
  }
  const file=option('--output')?path.resolve(option('--output')):path.join(root,'native/artifacts/softbody-motion-probe.json');fs.writeFileSync(file,JSON.stringify(report));
  console.log(JSON.stringify({file,height:report.height,meshes:report.meshes,bones:report.bones,roles:Object.fromEntries(Object.entries(report.roles).map(([role,v])=>[role,{duration:v.duration,samples:v.samples,candidates:v.candidates.map(c=>({mode:c.mode||'full',retainedWeight:c.retainedWeight,includeCenter:c.includeCenter,summary:c.summary}))}]))},null,2));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
