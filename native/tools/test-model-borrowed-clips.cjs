// Sources with metadata.clipsFrom borrow the authored clips of a rip with the
// same skeleton. For every such source: the skin joints exist in the sibling,
// their inverse binds agree, every role is present and finite, and sampled
// joint movement matches the sibling playing the same clip.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const root=path.resolve(__dirname,'../..'),failures=[];let checks=0;
const check=(ok,label,detail)=>{checks++;if(!ok)failures.push({label,detail});};
async function main(){
  const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')(),{modelClips}=await require('./load-model-motion.cjs')();
  const borrowed=await require('./load-borrowed-clips.cjs')();
  const parse=async key=>{const s=catalog.sources[key],bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[s.asset].file)));
    const length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const p=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});p.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
    p.loadImageSource=async()=>new THREE.Texture();p.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((res,rej)=>p.parse(res,rej));gltf.scene.updateMatrixWorld(true);return gltf;};
  const joints=gltf=>{const map=new Map();gltf.scene.traverse(o=>{if(o.isSkinnedMesh)o.skeleton.bones.forEach((b,i)=>{if(!map.has(b.name)&&!/dummy|^[A-Z]P\d+$/i.test(b.name))map.set(b.name,{bone:b,inverse:o.skeleton.boneInverses[i]});});});return map;};
  const results=[];
  for(const [key,source] of Object.entries(catalog.sources)){
    const from=source.metadata.clipsFrom;if(!from)continue;
    check(!!catalog.sources[from],key+': clipsFrom source exists',from);if(!catalog.sources[from])continue;
    check(!catalog.assets[source.asset].animations.length,key+': borrowing source has no authored clips of its own');
    const gltf=await parse(key),sibling=await parse(from),own=joints(gltf),other=joints(sibling);
    check(own.size>=3&&[...own.keys()].every(n=>other.has(n)),key+': every skin joint exists in '+from);
    const box=new THREE.Box3().setFromObject(gltf.scene),height=box.max.y-box.min.y||1;
    let bind=0;for(const [n,j] of own){const o=other.get(n);if(o)bind=Math.max(bind,...j.inverse.elements.map((v,i)=>Math.abs(v-o.inverse.elements[i])));}
    check(bind<.05,key+': inverse binds agree with '+from,bind);
    const clips=modelClips(gltf,source.metadata,await borrowed(source.metadata)),reference=modelClips(sibling,catalog.sources[from].metadata);
    check(['idle','attack','special','win'].every(r=>clips.some(c=>c.name===r)),key+': idle/attack/special/win present');
    check(clips.every(c=>c.tracks.every(t=>Array.from(t.values).every(Number.isFinite))),key+': finite tracks');
    // Each skeleton is normalized by its own joint extent (some rips use other units).
    const extent=(map,first)=>{const p0=map.get(first).bone.getWorldPosition(new THREE.Vector3());let e=0;for(const [,j] of map)e=Math.max(e,j.bone.getWorldPosition(new THREE.Vector3()).distanceTo(p0));return e||1;};
    let pose=0,extentA=1,extentB=1,worst=null;
    {const first=[...own.keys()][0];extentA=extent(own,first);extentB=extent(other,first);}
    for(const clip of clips){
      const ref=reference.find(c=>c.name===clip.name);if(!ref)continue;
      const a=new THREE.AnimationMixer(gltf.scene),b=new THREE.AnimationMixer(sibling.scene);
      a.clipAction(clip).play();b.clipAction(ref).play();
      const opening=new Map();
      for(const f of [0,.31,.67,.93]){
        a.setTime(clip.duration*f);b.setTime(ref.duration*f);gltf.scene.updateMatrixWorld(true);sibling.scene.updateMatrixWorld(true);
        // Joint positions relative to the skeleton's first joint: independent of import containers (UprightFix).
        const base=[...own.keys()][0],pa=own.get(base).bone.getWorldPosition(new THREE.Vector3()),pb=other.get(base).bone.getWorldPosition(new THREE.Vector3());
        const qa=own.get(base).bone.getWorldQuaternion(new THREE.Quaternion()).invert(),qb=other.get(base).bone.getWorldQuaternion(new THREE.Quaternion()).invert();
        for(const [n,j] of own){
          const va=j.bone.getWorldPosition(new THREE.Vector3()).sub(pa).applyQuaternion(qa),vb=other.get(n).bone.getWorldPosition(new THREE.Vector3()).sub(pb).applyQuaternion(qb);
          // Compare each joint's movement from the clip's opening frame; joint lengths may differ slightly.
          va.multiplyScalar(1/extentA);vb.multiplyScalar(1/extentB);
          if(f===0)opening.set(n,[va.clone(),vb.clone()]);
          const [oa,ob]=opening.get(n);
          if(Number.isFinite(va.x)&&Number.isFinite(vb.x)){const d=va.clone().sub(oa).distanceTo(vb.clone().sub(ob));if(d>pose){pose=d;worst={joint:n,clip:clip.name,f};}}
        }
      }
      a.stopAllAction();b.stopAllAction();
    }
    check(pose<.12,key+': borrowed clips reproduce the sibling movement (joint lengths may differ, e.g. Mochimon mouth .11)',{pose,worst});
    results.push({key,from,bind,pose,height});
  }
  check(results.length>=50,'clipsFrom sources covered',results.length);
  const output=path.join(root,'native/artifacts/model-borrowed-clips-regression.json');fs.mkdirSync(path.dirname(output),{recursive:true});
  fs.writeFileSync(output,JSON.stringify({checks,failures,results},null,2)+'\n');
  console.log(`${results.length} borrowed-clip sources: ${checks} checks, ${failures.length} failures; report ${path.relative(root,output)}`);
  for(const f of failures)console.log('FAIL '+f.label+' '+JSON.stringify(f.detail??''));
  if(failures.length)process.exitCode=1;
}
main().catch(error=>{console.error(error);process.exitCode=1;});
