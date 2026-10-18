// Exact-source skinned-geometry regression for the Links Syakomon attack.
// The captured .1925 s runtime peak is an authored .5833333 s key. Uniform
// sampling alone misses it, so include every authored key and interval midpoint.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),key='catalog:L_Syakomon:chr709',uid='rip-cat_l_syakomon_chr709';
const correctedNames=new Set(['J_spine01.scale','J_ear01_l.scale','J_ear02_l.scale','J_ear01_r.scale','J_ear02_r.scale']);
const failures=[];let checks=0;
function check(ok,label,detail){checks++;if(!ok)failures.push({label,detail});}
const bytes=a=>Buffer.from(a.buffer,a.byteOffset,a.byteLength);
const hash=t=>crypto.createHash('sha256').update(t.name).update(t.ValueTypeName).update(String(t.getInterpolation())).update(bytes(t.times)).update(bytes(t.values)).digest('hex');
const clipHash=c=>crypto.createHash('sha256').update(c.name).update(String(c.duration)).update(c.tracks.map(hash).join('|')).digest('hex');
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),entry=catalog.sources[key];
  if(entry?.metadata.uid!==uid)throw Error('Reviewed source fixture is missing.');
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const packed=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[entry.asset].file)));
  const length=packed.readUInt32LE(12),json=JSON.parse(packed.subarray(20,20+length)),binary=packed.subarray(28+length);
  async function load(){
    const parser=new GLTFParser(JSON.parse(JSON.stringify(json)),{manager:THREE.DefaultLoadingManager});
    parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();
    parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    return new Promise((resolve,reject)=>parser.parse(resolve,reject));
  }
  function prepare(gltf,clips){
    const idle=clips.find(c=>c.name==='idle'),template=new THREE.AnimationMixer(gltf.scene);
    template.clipAction(idle).play();template.update(0);gltf.scene.updateMatrixWorld(true);
    const rest=[],meshes=[];gltf.scene.traverse(o=>{rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone()});if(o.isSkinnedMesh)meshes.push(o);});
    const mixer=new THREE.AnimationMixer(gltf.scene);
    return {meshes,sample(clip,time){
      mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}
      const action=mixer.clipAction(clip);action.reset();action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);
      gltf.scene.updateMatrixWorld(true);for(const mesh of meshes)mesh.skeleton.update();
      return meshes.map(mesh=>Array.from({length:mesh.geometry.attributes.position.count},(_,i)=>mesh.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(mesh.matrixWorld).toArray()));
    }};
  }
  function bounds(points){const b=new THREE.Box3();for(const mesh of points)for(const p of mesh)b.expandByPoint(new THREE.Vector3(...p));return b;}
  const raw=await load(),reference=prepare(raw,raw.animations),idle=raw.animations.find(c=>c.name==='idle'),attack=raw.animations.find(c=>c.name==='attack');
  const baseline=reference.sample(idle,0),box=bounds(baseline),height=box.max.y-box.min.y;
  // Tooth growth is an authored bite effect and stays byte-identical. Bound only
  // the green body/antenna skin affected by the reviewed scale cascade, and
  // independently compare every other vertex with the original authored pose.
  const affected=reference.meshes.map(mesh=>Array.from({length:mesh.geometry.attributes.position.count},(_,i)=>{
    for(let j=0;j<4;j++)if(mesh.geometry.attributes.skinWeight.getComponent(i,j)>0)
      for(let bone=mesh.skeleton.bones[mesh.geometry.attributes.skinIndex.getComponent(i,j)];bone;bone=bone.parent)
        if(correctedNames.has(bone.name+'.scale'))return true;
    return false;
  }));
  const edges=reference.meshes.map((mesh,m)=>{
    const geometry=mesh.geometry,ids=geometry.index?Array.from(geometry.index.array):Array.from({length:geometry.attributes.position.count},(_,i)=>i),seen=new Set(),out=[];
    for(let i=0;i<ids.length;i+=3)for(const [a,b] of [[ids[i],ids[i+1]],[ids[i+1],ids[i+2]],[ids[i+2],ids[i]]]){
      const id=[Math.min(a,b),Math.max(a,b)].join(':');if(seen.has(id))continue;seen.add(id);
      const distance=Math.hypot(...baseline[m][a].map((v,k)=>v-baseline[m][b][k]));
      if(distance>height*1e-7&&(affected[m][a]||affected[m][b]))out.push({a,b,distance});
    }return out;
  });
  const keyTimes=[...new Set([0,attack.duration,...attack.tracks.flatMap(t=>Array.from(t.times)),...Array.from({length:129},(_,i)=>i*attack.duration/128)])].filter(t=>t<=attack.duration).sort((a,b)=>a-b);
  const times=[...new Set([...keyTimes,...keyTimes.slice(1).map((t,i)=>(t+keyTimes[i])/2)])].sort((a,b)=>a-b);
  const aliases=Object.entries(catalog.sources).filter(([,s])=>s.asset===entry.asset),results=[];
  const control=await load();check(modelClips(control,{...entry.metadata,uid:uid+'-control'}).map(clipHash).join('|')===raw.animations.map(clipHash).join('|'),'same rig with another UID retains every authored channel');
  let rawMaxHeight=0;
  for(const time of times){const b=bounds(reference.sample(attack,time));rawMaxHeight=Math.max(rawMaxHeight,(b.max.y-b.min.y)/height);}
  check(rawMaxHeight>8,'raw source reproduces the captured severe body enlargement',{rawMaxHeight});
  for(const [alias,source] of aliases){
    const gltf=await load(),before=gltf.animations.map(clipHash).join('|'),clips=modelClips(gltf,source.metadata),actual=prepare(gltf,clips),clip=clips.find(c=>c.name==='attack');
    check(gltf.animations.map(clipHash).join('|')===before,alias+': original clip arrays are not mutated');
    check(clips.length===raw.animations.length&&clip.duration===attack.duration,alias+': original roles and durations retained');
    check(clips.filter(c=>c.name!=='attack').every(c=>clipHash(c)===clipHash(raw.animations.find(r=>r.name===c.name))),alias+': idle, special and victory unchanged');
    check(attack.tracks.filter(t=>!correctedNames.has(t.name)).every(t=>hash(t)===hash(clip.tracks.find(c=>c.name===t.name))),alias+': shell hinge, mouth, tooth, root and all other channels byte-identical');
    check(clip.tracks.length===attack.tracks.length,alias+': no animation bindings added or removed');
    const stats={alias,samples:times.length,maxHeight:0,maxOrdinaryEdge:0,maxAbsoluteEdgeGrowth:0,unaffectedVertexDifference:0,peakTime:0,finite:true};
    for(const time of times){
      const points=actual.sample(clip,time),original=reference.sample(attack,time),b=bounds(points),ratio=(b.max.y-b.min.y)/height;
      if(ratio>stats.maxHeight){stats.maxHeight=ratio;stats.peakTime=time;}
      for(let m=0;m<points.length;m++){
        stats.finite&&=points[m].every(p=>p.every(Number.isFinite));
        for(let i=0;i<points[m].length;i++)if(!affected[m][i])
          stats.unaffectedVertexDifference=Math.max(stats.unaffectedVertexDifference,Math.hypot(...points[m][i].map((v,k)=>v-original[m][i][k]))/height);
        for(const edge of edges[m]){
          const length=Math.hypot(...points[m][edge.a].map((v,k)=>v-points[m][edge.b][k]));
          const growth=(length-edge.distance)/height;
          if(growth>stats.maxAbsoluteEdgeGrowth){
            stats.maxAbsoluteEdgeGrowth=growth;
            const mesh=actual.meshes[m],weights=i=>Array.from({length:4},(_,j)=>({bone:mesh.skeleton.bones[mesh.geometry.attributes.skinIndex.getComponent(i,j)].name,weight:mesh.geometry.attributes.skinWeight.getComponent(i,j)})).filter(w=>w.weight>0);
            stats.worstEdge={mesh:mesh.name,time,a:edge.a,b:edge.b,idleLength:edge.distance/height,currentLength:length/height,weights:[weights(edge.a),weights(edge.b)]};
          }
          if(edge.distance>=height*.01)stats.maxOrdinaryEdge=Math.max(stats.maxOrdinaryEdge,length/edge.distance);
        }
      }
    }
    check(stats.finite,alias+': complete skinned cycle finite');
    check(stats.maxHeight<2,alias+': opening clam stays within twice its closed idle height',stats);
    check(stats.maxAbsoluteEdgeGrowth<.15,alias+': no body/antenna edge stretches by more than 15 percent of idle body height',stats);
    check(stats.unaffectedVertexDifference<1e-7,alias+': shell, mouth and growing tooth vertices retain exact source motion',stats);
    results.push(stats);
  }
  const fileHash=file=>crypto.createHash('sha256').update(fs.readFileSync(path.join(root,file))).digest('hex');
  const report={sourceKey:key,uid,asset:entry.asset,codeHashes:Object.fromEntries(['native/content/models.json','js/model-motion.js','js/model-anatomy.js','native/tools/test-model-clam-scale.cjs'].map(file=>[file,fileHash(file)])),checks,rawMaxHeight,results,failures};
  fs.writeFileSync(path.join(root,'native/artifacts/model-clam-scale-regression.json'),JSON.stringify(report,null,2));
  console.log(JSON.stringify(report,null,2));if(failures.length)process.exitCode=1;
}
main().catch(error=>{console.error(error);process.exitCode=1;});
