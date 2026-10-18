// Actual source mixer/mesh regression for reviewed clipless DW1 rigid imports.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto'),vm=require('node:vm');
const {pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/artifacts/dw1-rig-diagnosis');
const preview=process.argv.includes('--brachiomon-preview');
const keys=preview?['extra:DW1_Brachiomon']:['extra:DW1_Agumon','extra:DW1_Airdramon','extra:DW1_Akatorimon',...(process.argv.includes('--first-three')?[]:['extra:DW1_Brachiomon'])];
const originalHashes={
  'extra:DW1_Agumon':'fd89d75cd3a044cda275d09498237784628d5f9f4f18811fe09d6611ce42ca80',
  'extra:DW1_Airdramon':'8f56049bf128083aa0221d76803ceb76a4ee602a319e3e638b580854f19296ce',
  'extra:DW1_Akatorimon':'9e68d57bd02b2f54cd6216f128ac113fadbb6c0c23e9751224597ffa02c8d13c',
  'extra:DW1_Brachiomon':'c5de628e214f3e1a2d5cebf1e3d63bcd1b4b077819eb421cb7fd1311662d1670',
};
const sha=b=>crypto.createHash('sha256').update(b).digest('hex'),failures=[];let checks=0;
const check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
async function main(){
  fs.mkdirSync(out,{recursive:true});
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json'))),{THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const {modelClips}=await require('./load-model-motion.cjs')();
  const rigidSource=fs.readFileSync(path.join(root,'js/model-rigid-parts.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {rigidPartRig}=await import('data:text/javascript;base64,'+Buffer.from(rigidSource).toString('base64'));
  const render=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8'),ctx={THREE,modelClips};vm.createContext(ctx);
  vm.runInContext(render.slice(render.indexOf('const IDLE_RE'),render.indexOf('// undefined = still loading')).replace('export function','function')+';this.prepareTemplate=prepareTemplate;',ctx);
  const load=async key=>{
    const entry=catalog.sources[key],asset=catalog.assets[entry.asset];let bytes;
    if(preview){bytes=fs.readFileSync(path.join(out,'DW1_Brachiomon_Recovered.glb'));check(sha(bytes)===originalHashes[key],key+': pinned recovered candidate');}
    else{const packed=fs.readFileSync(path.join(root,'native/artifacts/model-content',asset.file));check(sha(packed)===asset.sha256,key+': content hash');check(sha(fs.readFileSync(path.join(root,entry.asset)))===asset.sourceSha256,key+': original asset hash');check(asset.sourceSha256===originalHashes[key],key+': pinned geometry/material asset');bytes=zlib.gunzipSync(packed);}
    const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    // CPU-only parser supplies dummy images. Source candidates retain WebP,
    // whereas exported native fixtures already contain PNG image sources.
    if(preview)for(const texture of json.textures||[])if(texture.source===undefined&&texture.extensions?.EXT_texture_webp)texture.source=texture.extensions.EXT_texture_webp.source;
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
    const nodes=await Promise.all(json.nodes.map((_,i)=>parser.getDependency('node',i)));gltf.scene.updateMatrixWorld(true);return {gltf,nodes,entry,json};
  };
  const immutable=gltf=>{const a=[];gltf.scene.traverse(o=>{if(o.isMesh)a.push({name:o.name,index:o.geometry.index&&Array.from(o.geometry.index.array),attributes:Object.fromEntries(Object.entries(o.geometry.attributes).map(([k,v])=>[k,{type:v.array.constructor.name,normalized:v.normalized,itemSize:v.itemSize,values:Array.from(v.array)}])),material:[].concat(o.material).map(m=>m.toJSON())});});return sha(JSON.stringify(a));};
  const results=[];
  for(const key of keys){
    const {gltf,nodes,entry,json}=await load(key),spec=rigidPartRig(entry.metadata.uid),initial=nodes.map(n=>({n,p:n.position.clone(),q:n.quaternion.clone(),s:n.scale.clone(),parent:n.parent,m:n.matrixWorld.clone()}));
    const identity=immutable(gltf),clips=modelClips(gltf,entry.metadata),height=new THREE.Box3().setFromObject(gltf.scene).getSize(new THREE.Vector3()).y;
    check(immutable(gltf)===identity,key+': source geometry/attributes/materials immutable');
    check(initial.every(r=>r.n.position.equals(r.p)&&r.n.quaternion.equals(r.q)&&r.n.scale.equals(r.s)&&r.n.parent===r.parent),key+': rest TRS/hierarchy unchanged');
    check(!json.skins?.length&&!json.animations?.length,key+': expected original clipless rigid source');
    check(clips.length===5&&['idle','run','attack','special','win'].every(r=>clips.filter(c=>c.name===r).length===1),key+': five distinct required roles');
    check(clips.every(c=>c.tracks.every(t=>nodes.some(n=>t.name===n.name+'.position'||t.name===n.name+'.quaternion'))),key+': only original-node rigid tracks');
    const parts=nodes.map((node,index)=>{
      const local=[],triangles=[];node.traverse(o=>{if(!o.isMesh)return;const offset=local.length;
        for(let i=0;i<o.geometry.attributes.position.count;i++)local.push(o.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(o.matrixWorld).applyMatrix4(node.matrixWorld.clone().invert()));
        const ids=o.geometry.index?Array.from(o.geometry.index.array):Array.from({length:o.geometry.attributes.position.count},(_,i)=>i);
        for(let i=0;i<ids.length;i+=3)triangles.push(ids.slice(i,i+3).map(n=>n+offset));
      });return {node,index,local,triangles,rest:local.map(p=>p.clone().applyMatrix4(node.matrixWorld))};
    });
    const byName=new Map(parts.map(p=>[p.node.name,p])),links=spec.parts.filter(p=>p.parent).map(p=>{
      const child=byName.get(p.name),parent=byName.get(p.parent),pivot=new THREE.Vector3(...p.pivot);
      const selected=part=>{
        const unique=[...new Map(part.rest.map((v,i)=>[v.toArray().map(x=>x.toFixed(5)).join(','),i])).values()];
        const vertices=unique.sort((a,b)=>part.rest[a].distanceToSquared(pivot)-part.rest[b].distanceToSquared(pivot)).slice(0,12);
        const faces=part.triangles.map(face=>({face,d:face.reduce((s,i)=>s+part.rest[i].distanceToSquared(pivot),0)/3})).sort((a,b)=>a.d-b.d).slice(0,16).map(p=>p.face);
        return {vertices,faces};
      };
      return {name:p.name,parent:p.parent,child,parentPart:parent,pivot,childAnchor:pivot.clone().applyMatrix4(child.node.matrixWorld.clone().invert()),parentAnchor:pivot.clone().applyMatrix4(parent.node.matrixWorld.clone().invert()),a:selected(child),b:selected(parent)};
    });
    const edges=parts.flatMap(part=>{const seen=new Set(),a=[];for(const face of part.triangles)for(let i=0;i<3;i++){const x=face[i],y=face[(i+1)%3],id=[Math.min(x,y),Math.max(x,y)].join(':');if(seen.has(id))continue;seen.add(id);const length=part.rest[x].distanceTo(part.rest[y]);if(length>height*1e-5)a.push({part:part.index,x,y,length});}return a;});
    const mixer=new THREE.AnimationMixer(gltf.scene);
    function sample(clip,time){
      mixer.stopAllAction();for(const r of initial){r.n.position.copy(r.p);r.n.quaternion.copy(r.q);r.n.scale.copy(r.s);}
      const action=mixer.clipAction(clip);action.reset().setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);
      return parts.map(p=>p.local.map(v=>v.clone().applyMatrix4(p.node.matrixWorld)));
    }
    const distance=(a,ai,b,bf)=>{let best=Infinity;const target=new THREE.Vector3(),triangle=new THREE.Triangle();for(const i of ai)for(const face of bf){triangle.set(...face.map(n=>b[n])).closestPointToPoint(a[i],target);best=Math.min(best,a[i].distanceTo(target));}return best;};
    const gap=(link,pose)=>Math.min(distance(pose[link.child.index],link.a.vertices,pose[link.parentPart.index],link.b.faces),distance(pose[link.parentPart.index],link.b.vertices,pose[link.child.index],link.a.faces));
    const idle=clips.find(c=>c.name==='idle'),neutral=sample(idle,0),floor=Math.min(...neutral.flat().map(p=>p.y));
    links.forEach(l=>l.baselineGap=gap(l,neutral));
    if(key.endsWith('Agumon'))for(const side of ['Left','Right']){
      const arm=byName.get(`Agumon_${side}_Arm_${side}_Arm`),pivot=new THREE.Vector3(...spec.parts.find(p=>p.name===arm.node.name).pivot),tip=neutral[arm.index].reduce((a,b)=>Math.abs(b.x)>Math.abs(a.x)?b:a);
      check((tip.y-pivot.y)/tip.distanceTo(pivot)<-.45,key+': '+side+' neutral arm lowered');
    }
    const roles=[],visual=[];
    for(const clip of clips){
      const opening=sample(clip,0),stats={role:clip.name,samples:0,maxAnchorErrorHeight:0,maxNearestContactGapGrowthHeight:0,maxEdgeError:0,minimumFloorHeight:Infinity,maxBoundsHeight:0,localArticulationRmsHeight:0,closureHeight:0,links:links.map(l=>({name:l.name,parent:l.parent,baselineGapHeight:l.baselineGap/height,maxGapHeight:0}))};
      const keyTimes=Array.from(clip.tracks[0].times),times=[...new Set([...keyTimes,...keyTimes.slice(1).map((t,i)=>(t+keyTimes[i])/2),clip.duration])].sort((a,b)=>a-b);
      const rootPart=byName.get(spec.parts.find(p=>p.parent===null).name),restRoot=initial[rootPart.index].m;
      const openingInverse=rootPart.node.matrixWorld.clone().multiply(restRoot.clone().invert()).invert();
      const openingInternal=opening.map(points=>points.map(v=>v.clone().applyMatrix4(openingInverse)));
      for(const [sampleIndex,time] of times.entries()){
        const pose=sample(clip,time);stats.samples++;
        check(pose.flat().every(p=>p.toArray().every(Number.isFinite)),key+'/'+clip.name+': finite mesh');
        const rootDelta=rootPart.node.matrixWorld.clone().multiply(restRoot.clone().invert()),inverse=rootDelta.clone().invert();let squared=0,count=0;
        for(let i=0;i<parts.length;i++)for(let j=0;j<pose[i].length;j++)if(i!==rootPart.index){squared+=pose[i][j].clone().applyMatrix4(inverse).distanceToSquared(openingInternal[i][j]);count++;}
        stats.localArticulationRmsHeight=Math.max(stats.localArticulationRmsHeight,Math.sqrt(squared/count)/height);
        links.forEach((link,i)=>{
          const a=link.childAnchor.clone().applyMatrix4(link.child.node.matrixWorld),b=link.parentAnchor.clone().applyMatrix4(link.parentPart.node.matrixWorld);
          stats.maxAnchorErrorHeight=Math.max(stats.maxAnchorErrorHeight,a.distanceTo(b)/height);
          if(sampleIndex%8===0||time===clip.duration){const current=gap(link,pose);stats.links[i].maxGapHeight=Math.max(stats.links[i].maxGapHeight,current/height);stats.maxNearestContactGapGrowthHeight=Math.max(stats.maxNearestContactGapGrowthHeight,(current-link.baselineGap)/height);}
        });
        for(const e of edges)stats.maxEdgeError=Math.max(stats.maxEdgeError,Math.abs(pose[e.part][e.x].distanceTo(pose[e.part][e.y])/e.length-1));
        const bounds=new THREE.Box3().setFromPoints(pose.flat());stats.minimumFloorHeight=Math.min(stats.minimumFloorHeight,(bounds.min.y-floor)/height);stats.maxBoundsHeight=Math.max(stats.maxBoundsHeight,bounds.getSize(new THREE.Vector3()).y/height);
        if(time===clip.duration)for(let i=0;i<parts.length;i++)for(let j=0;j<pose[i].length;j++)stats.closureHeight=Math.max(stats.closureHeight,pose[i][j].distanceTo(opening[i][j])/height);
      }
      check(stats.maxAnchorErrorHeight<.0003,key+'/'+clip.name+': attached virtual joints including interpolated frames',stats.maxAnchorErrorHeight);
      check(stats.maxNearestContactGapGrowthHeight<.018,key+'/'+clip.name+': nearest contact across original attachment regions',stats);
      check(stats.maxEdgeError<.00002,key+'/'+clip.name+': every mesh edge rigid',stats.maxEdgeError);
      check(stats.minimumFloorHeight>-.0005,key+'/'+clip.name+': no floor penetration',stats.minimumFloorHeight);
      check(stats.maxBoundsHeight<1.28,key+'/'+clip.name+': bounded pose',stats.maxBoundsHeight);
      check(stats.closureHeight<.00002,key+'/'+clip.name+': looping/one-shot endpoint returns to opening',stats.closureHeight);
      check(stats.localArticulationRmsHeight>(clip.name==='idle'?.001:.008),key+'/'+clip.name+': actual internal articulation',stats.localArticulationRmsHeight);
      roles.push(stats);
      for(const fraction of [0,.17,.34,.5,.67,.84,1]){const pose=sample(clip,clip.duration*fraction);visual.push({role:clip.name,fraction,parts:parts.map((p,i)=>({index:i,name:p.node.name,points:pose[i].map(v=>v.toArray()),triangles:p.triangles,bounds:{min:new THREE.Box3().setFromPoints(pose[i]).min.toArray(),max:new THREE.Box3().setFromPoints(pose[i]).max.toArray()}}))});}
    }
    const blends={maxAnchorErrorHeight:0,maxNearestContactGapGrowthHeight:0,worst:null},blendFrames=[];
    for(const from of clips)for(const to of clips)if(from!==to)for(const weight of [.25,.5,.75]){
      mixer.stopAllAction();for(const r of initial){r.n.position.copy(r.p);r.n.quaternion.copy(r.q);r.n.scale.copy(r.s);}
      const a=mixer.clipAction(from),b=mixer.clipAction(to);a.reset().setEffectiveWeight(1-weight).play();b.reset().setEffectiveWeight(weight).play();a.time=from.duration*.43;b.time=to.duration*.07;mixer.update(0);gltf.scene.updateMatrixWorld(true);
      const pose=parts.map(p=>p.local.map(v=>v.clone().applyMatrix4(p.node.matrixWorld)));
      let poseError=0;
      for(const link of links){const error=link.childAnchor.clone().applyMatrix4(link.child.node.matrixWorld).distanceTo(link.parentAnchor.clone().applyMatrix4(link.parentPart.node.matrixWorld))/height;
        poseError=Math.max(poseError,error);
        blends.maxAnchorErrorHeight=Math.max(blends.maxAnchorErrorHeight,error);const growth=(gap(link,pose)-link.baselineGap)/height;
        if(growth>blends.maxNearestContactGapGrowthHeight){blends.maxNearestContactGapGrowthHeight=growth;blends.worst={from:from.name,to:to.name,weight,link:link.name};}
      }
      blendFrames.push({role:from.name+'>'+to.name,fraction:weight,anchorErrorHeight:poseError,parts:parts.map((p,i)=>({index:i,name:p.node.name,points:pose[i].map(v=>v.toArray()),triangles:p.triangles,bounds:{min:new THREE.Box3().setFromPoints(pose[i]).min.toArray(),max:new THREE.Box3().setFromPoints(pose[i]).max.toArray()}}))});
      blendFrames.sort((a,b)=>b.anchorErrorHeight-a.anchorErrorHeight);blendFrames.splice(7);
    }
    check(blends.maxNearestContactGapGrowthHeight<.018,key+': interruption blend nearest attachment contact',blends);
    check(immutable(gltf)===identity,key+': source attributes/materials survive playback');
    const control=await load(key);check(modelClips(control.gltf,{...entry.metadata,uid:entry.metadata.uid+'-control'}).length===0,key+': nonmatching UID unchanged');
    const browser=await load(key),template=ctx.prepareTemplate(browser.gltf,entry.metadata);check(template.actions.run?.name==='run',key+': actual browser companion template exposes run');
    const context={key,sourceSha256:preview?originalHashes[key]:catalog.assets[entry.asset].sourceSha256,roles,blends};results.push(context);
    const visualName=preview?'DW1_Brachiomon_Recovered':key.split(':')[1];
    fs.writeFileSync(path.join(out,visualName+'.motion.json'),JSON.stringify({...context,frames:visual}));
    fs.writeFileSync(path.join(out,visualName+'.blends.json'),JSON.stringify({...context,frames:blendFrames}));
    console.log(JSON.stringify({key,roles:roles.map(({links,...r})=>r)}));
  }
  const report={checks,failures,metricDefinitions:{maxAnchorErrorHeight:'Distance between a shared virtual joint evaluated through the actual child and parent node transforms. Flat-node TRS interruption blending can move these anchors differently; the measured limitation is retained.',maxNearestContactGapGrowthHeight:'Increase in minimum distance among selected attachment-region vertices and opposite triangles. Bounds closest contact only, not every cap edge or seamless boundary coverage.'},results};fs.writeFileSync(path.join(out,preview?'brachiomon-motion-checks.json':'motion-checks.json'),JSON.stringify(report,null,2));console.log(JSON.stringify({checks,failures:failures.length}));
  if(failures.length){console.error(JSON.stringify(failures,null,2));process.exitCode=1;}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
