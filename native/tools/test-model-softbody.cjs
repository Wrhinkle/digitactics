// Independent geometry regression: evaluates ACTUAL modelClips, never a copy of
// the repair. Run before/after the exact-UID fix to retain the red/green evidence.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),key='catalog:CS_Pabumon:chr387',uid='rip-cat_cs_pabumon_chr387';
const bodyNames=new Set(['J_center','J_body1','J_body2','J_body3','J_body4','J_legL1','J_legL2','J_legF1','J_legF2','J_legR1','J_legR2','J_tail1','J_tail2','J_tail3']);
const nodeName=track=>track.name.slice(0,track.name.lastIndexOf('.'));
const failures=[],check=(ok,label,detail)=>{if(!ok)failures.push({label,detail});};
const bytes=array=>Buffer.from(array.buffer,array.byteOffset,array.byteLength);
const trackHash=t=>crypto.createHash('sha256').update(t.name).update(t.ValueTypeName).update(String(t.getInterpolation())).update(bytes(t.times)).update(bytes(t.values)).digest('hex');
const clipHash=c=>crypto.createHash('sha256').update(c.name).update(String(c.duration)).update(c.tracks.map(trackHash).join('|')).digest('hex');
function triangle(points,a,b,c){
  const A=points[a],B=points[b],C=points[c],u=[B[0]-A[0],B[1]-A[1],B[2]-A[2]],v=[C[0]-A[0],C[1]-A[1],C[2]-A[2]];
  return {edges:[Math.hypot(...u),Math.hypot(B[0]-C[0],B[1]-C[1],B[2]-C[2]),Math.hypot(...v)],area:Math.hypot(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0])/2};
}
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),entry=catalog.sources[key];
  if(entry?.metadata.uid!==uid)throw Error('Exact source UID missing.');
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const packed=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[entry.asset].file)));
  const length=packed.readUInt32LE(12),json=JSON.parse(packed.subarray(20,20+length)),binary=packed.subarray(28+length);
  const load=async()=>{
    const parser=new GLTFParser(JSON.parse(JSON.stringify(json)),{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    return new Promise((resolve,reject)=>parser.parse(resolve,reject));
  };
  const prepare=(gltf,clips)=>{
    // Match prepareTemplate: leave assembled idle on the scene, then construct a
    // separate action mixer. Unkeyed channels inherit assembled idle, not bind.
    const idle=clips.find(c=>c.name==='idle'),template=new THREE.AnimationMixer(gltf.scene);template.clipAction(idle).play();template.update(0);
    const rest=[],meshes=[];gltf.scene.traverse(o=>{rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone()});if(o.isSkinnedMesh)meshes.push(o);});
    const mixer=new THREE.AnimationMixer(gltf.scene),rootBone=gltf.scene.getObjectByName('J_root');
    return {meshes,sample:(clip,time)=>{
      mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}
      const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);
      for(const mesh of meshes)mesh.skeleton.update();
      const inverse=rootBone.matrixWorld.clone().invert();
      const world=meshes.map(mesh=>Array.from({length:mesh.geometry.attributes.position.count},(_,i)=>mesh.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(mesh.matrixWorld).toArray()));
      return {world,local:world.map(points=>points.map(p=>new THREE.Vector3(...p).applyMatrix4(inverse).toArray())),rootScale:rootBone.getWorldScale(new THREE.Vector3()).y};
    }};
  };
  const raw=await load(),rawHashes=raw.animations.map(clipHash),reference=prepare(raw,raw.animations),idle=raw.animations.find(c=>c.name==='idle'),base=reference.sample(idle,0);
  const box=new THREE.Box3();for(const points of base.world)for(const p of points)box.expandByPoint(new THREE.Vector3(...p));
  const height=box.max.y-box.min.y,localHeight=height/base.rootScale;
  const meta=reference.meshes.map((mesh,m)=>{
    const geometry=mesh.geometry,indices=geometry.index?Array.from(geometry.index.array):Array.from({length:geometry.attributes.position.count},(_,i)=>i);
    const category=Array.from({length:geometry.attributes.position.count},(_,i)=>{
      const names=[];for(let j=0;j<4;j++)if(geometry.attributes.skinWeight.getComponent(i,j)>1e-5)names.push(mesh.skeleton.bones[geometry.attributes.skinIndex.getComponent(i,j)].name);
      return names.some(n=>/^J_float[1-4]$/.test(n))?'bubble':names.some(n=>/^J_mouth/.test(n))?'mouth':'body';
    });
    const triangles=[];for(let i=0;i<indices.length;i+=3){const ids=indices.slice(i,i+3),baseline=triangle(base.world[m],...ids);
      if(baseline.area<height*height*1e-12||Math.min(...baseline.edges)<height*1e-7)continue;
      const kind=ids.some(v=>category[v]==='bubble')?'bubble':ids.some(v=>category[v]==='mouth')?'mouth':'body';
      triangles.push({ids,baseline,kind});
    }return {category,triangles};
  });
  // Exact-UID scope guard on the same mesh: a neighboring/nonmatching UID must
  // retain every authored clip, independent of which algorithm repairs Pabumon.
  const control=await load(),controlClips=modelClips(control,{...entry.metadata,uid:uid+'-control'});
  check(JSON.stringify(controlClips.map(clipHash))===JSON.stringify(rawHashes),'exact UID scope: nonmatching metadata unchanged');
  // The historical capture's sample times are local evidence (native/artifacts is not in Git); without it,
  // the 121 uniform samples and every authored key time still run.
  const auditPath=path.join(root,'native/artifacts/model-visual-audit-v3/0206.json');
  const audit=fs.existsSync(auditPath)?JSON.parse(fs.readFileSync(auditPath)):(console.log('Soft-body: local audit 0206.json absent; using uniform and key-time samples only.'),{Samples:[]});
  const aliases=Object.entries(catalog.sources).filter(([,s])=>s.asset===entry.asset),results=[];
  for(const [alias,metadata] of aliases){
    const gltf=await load(),inputHashes=gltf.animations.map(clipHash),clips=modelClips(gltf,metadata.metadata),actual=prepare(gltf,clips);
    check(JSON.stringify(gltf.animations.map(clipHash))===JSON.stringify(inputHashes),alias+': source clips are not mutated');
    check(clips.length===raw.animations.length,alias+': clip membership unchanged');
    for(const rawClip of raw.animations){
      const clip=clips.find(c=>c.name===rawClip.name);
      check(!!clip,alias+': role exists',rawClip.name);if(!clip)continue;
      check(clip.duration===rawClip.duration,alias+': clip duration unchanged',clip.name);
      check(new Set(clip.tracks.map(t=>t.name)).size===clip.tracks.length,alias+': no duplicate animation bindings',clip.name);
      if(!['attack','win'].includes(clip.name)){check(clipHash(clip)===clipHash(rawClip),alias+': unrelated role unchanged',clip.name);continue;}
      const protectedTracks=rawClip.tracks.filter(t=>!bodyNames.has(nodeName(t))||t.name.endsWith('.scale'));
      check(protectedTracks.every(t=>{const other=clip.tracks.find(c=>c.name===t.name);return other&&trackHash(t)===trackHash(other);}),alias+': non-body and scale tracks byte-identical',clip.name);
      check(clip.tracks.filter(t=>t.name.endsWith('.scale')).map(trackHash).join('|')===rawClip.tracks.filter(t=>t.name.endsWith('.scale')).map(trackHash).join('|'),alias+': no scale animation added or removed',clip.name);
      const keys=[...new Set([0,clip.duration,...rawClip.tracks.flatMap(t=>Array.from(t.times)),...clip.tracks.flatMap(t=>Array.from(t.times)),...Array.from({length:121},(_,i)=>i*clip.duration/120),...audit.Samples.filter(s=>s.Role===clip.name).map(s=>s.Time)])].filter(t=>t<=clip.duration).sort((a,b)=>a-b);
      const times=[...new Set([...keys,...keys.slice(1).map((t,i)=>(t+keys[i])/2)])].sort((a,b)=>a-b);
      const stats={alias,role:clip.name,samples:times.length,bodyEdgeMin:Infinity,bodyEdgeMax:0,bodyAreaMin:Infinity,bodyAreaMax:0,mouthEdgeMax:0,mouthAreaMax:0,bubbleDifferenceMax:0,bodyRmsMax:0,endpointDifferenceMax:0,finite:true};
      for(const time of times){
        const pose=actual.sample(clip,time),original=reference.sample(rawClip,time);let sumSquares=0,bodyVertices=0;
        meta.forEach((mesh,m)=>{
          mesh.category.forEach((kind,i)=>{
            const p=pose.world[m][i];stats.finite&&=p.every(Number.isFinite);
            if(kind==='bubble')stats.bubbleDifferenceMax=Math.max(stats.bubbleDifferenceMax,Math.hypot(...p.map((v,k)=>v-original.world[m][i][k]))/height);
            if(kind==='body'){
              const delta=Math.hypot(...pose.local[m][i].map((v,k)=>v-base.local[m][i][k]))/localHeight;
              sumSquares+=delta*delta;bodyVertices++;
              if(time===0||time===clip.duration)stats.endpointDifferenceMax=Math.max(stats.endpointDifferenceMax,delta);
            }
          });
          for(const tri of mesh.triangles){if(tri.kind==='bubble')continue;const now=triangle(pose.world[m],...tri.ids),edges=now.edges.map((e,i)=>e/tri.baseline.edges[i]),area=now.area/tri.baseline.area;
            if(tri.kind==='body'){stats.bodyEdgeMin=Math.min(stats.bodyEdgeMin,...edges);stats.bodyEdgeMax=Math.max(stats.bodyEdgeMax,...edges);stats.bodyAreaMin=Math.min(stats.bodyAreaMin,area);stats.bodyAreaMax=Math.max(stats.bodyAreaMax,area);}
            else{stats.mouthEdgeMax=Math.max(stats.mouthEdgeMax,...edges);stats.mouthAreaMax=Math.max(stats.mouthAreaMax,area);}
          }
        });stats.bodyRmsMax=Math.max(stats.bodyRmsMax,Math.sqrt(sumSquares/bodyVertices));
      }
      check(stats.finite,alias+': finite skinned positions',clip.name);
      check(stats.bodyEdgeMin>=1/1.02&&stats.bodyEdgeMax<=1.02,alias+': body edges retain idle lengths',stats);
      check(stats.bodyAreaMin>=1/1.02&&stats.bodyAreaMax<=1.02,alias+': body triangles retain idle areas without collapse',stats);
      check(stats.mouthEdgeMax<=1.3&&stats.mouthAreaMax<=1.3,alias+': mouth deformation remains modest',stats);
      check(stats.bubbleDifferenceMax<1e-7,alias+': bubble vertices preserve original motion',stats);
      check(stats.bodyRmsMax>=.02,alias+': body action remains readable relative to root',stats);
      check(stats.endpointDifferenceMax<1e-4,alias+': body returns to idle at both endpoints in root space',stats);
      results.push(stats);
    }
  }
  const report={sourceKey:key,uid,asset:entry.asset,aliases:aliases.map(([k,s])=>({key:k,uid:s.metadata.uid})),results,failures};
  const output=path.join(root,'native/artifacts/model-softbody-regression.json');fs.writeFileSync(output,JSON.stringify(report,null,2));
  console.log(JSON.stringify(report,null,2));
  if(failures.length)process.exitCode=1;else console.log('Pabumon exact-UID soft-body geometry checks passed.');
}
main().catch(error=>{console.error(error);process.exitCode=1;});
