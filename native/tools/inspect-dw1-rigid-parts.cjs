// Read-only geometry diagnostic for the first three clipless DW1 imports.
// Freezes content-addressed payloads; never rewrites source assets or catalogs.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/artifacts/dw1-rig-diagnosis');
const keys=process.argv.length>2?process.argv.slice(2):['extra:DW1_Agumon','extra:DW1_Airdramon','extra:DW1_Akatorimon'];
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
  fs.mkdirSync(out,{recursive:true});
  const catalogBytes=fs.readFileSync(path.join(root,'native/content/models.json')),catalog=JSON.parse(catalogBytes);
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const report={catalogSha256:sha(catalogBytes),purpose:'Frozen original geometry and current source animation-path diagnostic; no runtime mutation',sources:[]};
  for(const key of keys){
    const source=catalog.sources[key],asset=catalog.assets[source.asset],name=key.split(':')[1];
    const compressed=fs.readFileSync(path.join(root,'native/artifacts/model-content',asset.file)),packed=zlib.gunzipSync(compressed);
    const raw=fs.readFileSync(path.join(root,source.asset));
    if(sha(compressed)!==asset.sha256||sha(raw)!==asset.sourceSha256)throw new Error(key+': catalog/source payload mismatch');
    fs.writeFileSync(path.join(out,name+'.source.glb'),raw);fs.writeFileSync(path.join(out,name+'.decoded.glb'),packed);
    const length=packed.readUInt32LE(12),json=JSON.parse(packed.subarray(20,20+length)),binary=packed.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
    const nodes=await Promise.all(json.nodes.map((_,i)=>parser.getDependency('node',i)));gltf.scene.updateMatrixWorld(true);
    const parts=nodes.map((node,index)=>{
      const points=[],triangles=[];node.traverse(o=>{if(!o.isMesh)return;const g=o.geometry,offset=points.length;
        for(let i=0;i<g.attributes.position.count;i++)points.push(o.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(o.matrixWorld).toArray());
        const ids=g.index?Array.from(g.index.array):Array.from({length:g.attributes.position.count},(_,i)=>i);
        for(let i=0;i<ids.length;i+=3)triangles.push(ids.slice(i,i+3).map(v=>v+offset));
      });
      const vectors=points.map(p=>new THREE.Vector3(...p)),bounds=new THREE.Box3().setFromPoints(vectors);
      return {index,name:node.name,parent:nodes.indexOf(node.parent),position:node.position.toArray(),quaternion:node.quaternion.toArray(),scale:node.scale.toArray(),bounds:{min:bounds.min.toArray(),max:bounds.max.toArray()},points,triangles};
    });
    const bounds=new THREE.Box3().setFromPoints(parts.flatMap(p=>p.points.map(v=>new THREE.Vector3(...v)))),span=bounds.getSize(new THREE.Vector3()).length(),seams=[];
    for(let a=0;a<parts.length;a++)for(let b=a+1;b<parts.length;b++){
      let distance=Infinity,closest=[];
      for(const p of parts[a].points)for(const q of parts[b].points){const d=Math.hypot(...p.map((v,i)=>v-q[i]));
        if(d<distance-1e-5*span){distance=d;closest=[[p,q]];}else if(Math.abs(d-distance)<=1e-5*span)closest.push([p,q]);
      }
      const unique=[...new Map(closest.map(pair=>[JSON.stringify(pair),pair])).values()];
      const midpoint=[0,1,2].map(i=>unique.reduce((sum,pair)=>sum+(pair[0][i]+pair[1][i])/2,0)/unique.length);
      seams.push({a,b,distance,distanceFraction:distance/span,closestPairs:unique.length,midpoint});
    }
    const currentClips=modelClips(gltf,source.metadata).map(c=>({name:c.name,tracks:c.tracks.length,duration:c.duration}));
    const entry={key,source,asset,rawAnimations:json.animations?.length||0,skins:json.skins?.length||0,sceneRoots:json.scenes[json.scene||0].nodes,currentClips,bounds:{min:bounds.min.toArray(),max:bounds.max.toArray()},parts,seams:seams.sort((a,b)=>a.distance-b.distance)};
    fs.writeFileSync(path.join(out,name+'.json'),JSON.stringify(entry));
    report.sources.push({...entry,parts:parts.map(({points,triangles,...p})=>({...p,vertices:points.length,triangles:triangles.length}))});
    console.log(JSON.stringify({key,parts:parts.length,rawAnimations:entry.rawAnimations,skins:entry.skins,currentClips,bounds:entry.bounds}));
  }
  fs.writeFileSync(path.join(out,process.argv.length>2?'additional-report.json':'report.json'),JSON.stringify(report,null,2));
}
main().catch(e=>{console.error(e);process.exitCode=1;});
