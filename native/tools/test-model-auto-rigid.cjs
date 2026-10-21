// Automatic rigid-part motion for every clipless, unskinned source without a
// reviewed map (remaining DW1 assemblies and single-mesh rips): five roles,
// rigid original pieces, attached joints, grounded, bounded, looping, lowered
// arms and real movement. Source geometry and materials stay immutable.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),crypto=require('node:crypto');
const {pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'../..'),sha=b=>crypto.createHash('sha256').update(b).digest('hex'),failures=[];let checks=0;
const check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
  const script=fs.readFileSync(path.join(root,'js/model-rigid-parts.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {autoRigidPartRig,hasRigidPartMotion}=await import('data:text/javascript;base64,'+Buffer.from(script).toString('base64'));
  const seen=new Set(),results=[];
  for(const [key,source] of Object.entries(catalog.sources)){
    const asset=catalog.assets[source.asset];
    if(source.metadata.companion||asset.animations.length||asset.skins||hasRigidPartMotion(source.metadata)||seen.has(source.asset))continue;
    seen.add(source.asset);
    const packed=fs.readFileSync(path.join(root,'native/artifacts/model-content',asset.file));check(sha(packed)===asset.sha256,key+': content hash');
    const bytes=zlib.gunzipSync(packed),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parse=async()=>{const p=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});p.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
      p.loadImageSource=async()=>new THREE.Texture();p.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);return new Promise((res,rej)=>p.parse(res,rej));};
    const gltf=await parse();gltf.scene.updateMatrixWorld(true);
    const meshes=[];gltf.scene.traverse(o=>{if(o.isMesh)meshes.push(o);});
    const immutable=()=>sha(JSON.stringify(meshes.map(o=>({index:o.geometry.index&&Array.from(o.geometry.index.array),position:Array.from(o.geometry.attributes.position.array),material:[].concat(o.material).map(m=>m.uuid)}))));
    const before=immutable(),rest=[];gltf.scene.traverse(o=>rest.push({o,p:o.position.clone(),q:o.quaternion.clone(),s:o.scale.clone(),parent:o.parent}));
    const spec=autoRigidPartRig(gltf),clips=modelClips(gltf,source.metadata);
    check(!!spec,key+': automatic rigid map');
    check(clips.length===5&&['idle','run','attack','special','win'].every(r=>clips.filter(c=>c.name===r).length===1),key+': five distinct roles');
    check(immutable()===before&&rest.every(r=>r.o.position.equals(r.p)&&r.o.quaternion.equals(r.q)&&r.o.scale.equals(r.s)&&r.o.parent===r.parent),key+': source geometry, materials, rest TRS and hierarchy unchanged');
    if(!spec||clips.length!==5)continue;
    const parts=spec.parts.map(p=>{const node=gltf.scene.getObjectByName(p.name),local=[];
      node.updateMatrixWorld(true);const inverse=node.matrixWorld.clone().invert();
      node.traverse(o=>{if(!o.isMesh)return;for(let i=0;i<o.geometry.attributes.position.count;i++)local.push(o.getVertexPosition(i,new THREE.Vector3()).applyMatrix4(o.matrixWorld).applyMatrix4(inverse));});
      return {...p,node,local};});
    const byName=new Map(parts.map(p=>[p.name,p])),mixer=new THREE.AnimationMixer(gltf.scene);
    const reset=()=>{mixer.stopAllAction();for(const r of rest){r.o.position.copy(r.p);r.o.quaternion.copy(r.q);r.o.scale.copy(r.s);}};
    const sample=(clip,time)=>{reset();const a=mixer.clipAction(clip);a.reset().setLoop(THREE.LoopOnce,1);a.clampWhenFinished=true;a.play();mixer.setTime(time);gltf.scene.updateMatrixWorld(true);
      return parts.map(p=>p.local.map(v=>v.clone().applyMatrix4(p.node.matrixWorld)));};
    const restPose=(reset(),gltf.scene.updateMatrixWorld(true),parts.map(p=>p.local.map(v=>v.clone().applyMatrix4(p.node.matrixWorld))));
    const restBox=new THREE.Box3().setFromPoints(restPose.flat()),height=restBox.max.y-restBox.min.y,floor=restBox.min.y;
    // Joint anchor: the seated pivot expressed on each piece's original (unseated) rest geometry.
    const idle=clips.find(c=>c.name==='idle');reset();gltf.scene.updateMatrixWorld(true);
    const anchors=parts.filter(p=>p.parent).map(p=>{const pivot=new THREE.Vector3(...p.pivot),parent=byName.get(p.parent);
      return {p,parent,child:pivot.clone().sub(new THREE.Vector3(...p.offset)).applyMatrix4(p.node.matrixWorld.clone().invert()),
        host:pivot.clone().sub(new THREE.Vector3(...parent.offset)).applyMatrix4(parent.node.matrixWorld.clone().invert())};});
    sample(idle,0);
    const seated=anchors.map(a=>a.child.clone().applyMatrix4(a.p.node.matrixWorld).distanceTo(a.host.clone().applyMatrix4(a.parent.node.matrixWorld)));
    const stats={key,parts:parts.length,height,roles:[]};
    for(const clip of clips){
      const s={role:clip.name,anchor:0,floor:Infinity,bounds:0,motion:0,closure:0},opening=sample(clip,0);
      for(let i=0;i<=48;i++){
        const t=clip.duration*i/48,pose=sample(clip,t);
        check(pose.flat().every(v=>v.toArray().every(Number.isFinite)),key+'/'+clip.name+': finite');
        anchors.forEach((a,k)=>{s.anchor=Math.max(s.anchor,Math.abs(a.child.clone().applyMatrix4(a.p.node.matrixWorld).distanceTo(a.host.clone().applyMatrix4(a.parent.node.matrixWorld))-seated[k])/height);});
        const box=new THREE.Box3().setFromPoints(pose.flat());s.floor=Math.min(s.floor,(box.min.y-floor)/height);s.bounds=Math.max(s.bounds,(box.max.y-box.min.y)/height);
        for(let p=0;p<pose.length;p++)for(let v=0;v<pose[p].length;v+=7)s.motion=Math.max(s.motion,pose[p][v].distanceTo(opening[p][v])/height);
        if(i===48)for(let p=0;p<pose.length;p++)for(let v=0;v<pose[p].length;v++)s.closure=Math.max(s.closure,pose[p][v].distanceTo(opening[p][v])/height);
      }
      check(s.anchor<.002,key+'/'+clip.name+': joints stay attached',s.anchor);
      check(s.floor>-.002&&s.floor<.25,key+'/'+clip.name+': grounded (no floor penetration, bounded hop)',s.floor);
      check(s.bounds<1.45,key+'/'+clip.name+': bounded pose height',s.bounds);
      check(s.motion>(clip.name==='idle'?.006:.02),key+'/'+clip.name+': visible movement',s.motion);
      check(s.closure<1e-4,key+'/'+clip.name+': loop/one-shot returns to its opening pose',s.closure);
      stats.roles.push(s);
    }
    // Lowered arms: every lowered arm chain points at least ~12 degrees below horizontal (front view) in the idle opening pose.
    const pose=sample(idle,0);
    for(const p of parts.filter(p=>p.kind==='arm'&&p.segment===0&&p.restZ)){
      const i=parts.indexOf(p),pivot=new THREE.Vector3(...p.pivot),members=parts.map((q,j)=>({q,j})).filter(({q})=>{for(let r=q;r;r=byName.get(r.parent))if(r===p)return true;return false;});
      const joint=anchors.find(a=>a.p===p).child.clone().applyMatrix4(p.node.matrixWorld);
      const far=members.flatMap(({j})=>pose[j]).reduce((a,b)=>b.distanceToSquared(joint)>a.distanceToSquared(joint)?b:a),d=far.clone().sub(joint);
      check(Math.atan2(d.y,Math.abs(d.x))<-.2,key+': '+p.name+' outstretched arm lowered',Math.atan2(d.y,Math.abs(d.x)));
      void i;
    }
    reset();check(immutable()===before,key+': geometry/materials unchanged after playback');
    results.push(stats);
  }
  check(results.length>=24,'Every clipless unskinned source without a reviewed map is covered',results.length);
  const output=path.join(root,'native/artifacts/model-auto-rigid-regression.json');fs.mkdirSync(path.dirname(output),{recursive:true});
  fs.writeFileSync(output,JSON.stringify({checks,failures,results},null,2)+'\n');
  console.log(`${results.length} automatic rigid sources: ${checks} checks, ${failures.length} failures; report ${path.relative(root,output)}`);
  for(const f of failures)console.log('FAIL '+f.label+' '+JSON.stringify(f.detail??''));
  if(failures.length)process.exitCode=1;
}
main().catch(error=>{console.error(error);process.exitCode=1;});
