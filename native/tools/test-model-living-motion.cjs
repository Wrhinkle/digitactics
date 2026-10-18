const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),assert=require('node:assert/strict');
const {pathToFileURL}=require('node:url');
async function main(){
  const root=path.resolve(__dirname,'../..'),catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
  const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
  const text=fs.readFileSync(path.join(root,'js/model-motion.js'),'utf8').replace("from 'three'",`from '${pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
  const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(text).toString('base64'));
  for(const key of (process.argv[2]?[process.argv[2]]:['catalog:CS_Seadramon:chr347','drg:Stone Guardian'])){
    const source=catalog.sources[key],bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',catalog.assets[source.asset].file))),length=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+length)),binary=bytes.subarray(28+length);
    const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
    const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject)),clips=modelClips(gltf,source.metadata),idle=clips.find(c=>c.name==='idle');
    assert.ok(idle,key+' needs an idle');
    assert.ok(idle.tracks.some(track=>{const width=track.getValueSize();return Array.from(track.values).some((v,i)=>i>=width&&Math.abs(v-track.values[i%width])>1e-5);}),key+' idle remains a static pose');
    if(key==='catalog:CS_Seadramon:chr347'){
      const spine=[];gltf.scene.traverse(o=>{if(/^J_chine(?:_[a-n])?$/.test(o.name))spine.push(o);});
      assert.equal(spine.length,14,'Seadramon body chain must be fully identified');
      const rest=spine.map(o=>({o,q:o.quaternion.clone(),p:o.position.toArray(),s:o.scale.toArray()}));
      for(const role of ['idle','attack','special','win']){
        const clip=clips.find(c=>c.name===role),mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(clip);
        action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
        const seen=rest.map(()=>[]);
        for(let i=0;i<=32;i++){
          mixer.setTime(clip.duration*i/32);
          for(let j=0;j<rest.length;j++){
            const {o,q,p,s}=rest[j];assert.deepEqual(o.position.toArray(),p,'Swimming must not stretch body joints');assert.deepEqual(o.scale.toArray(),s,'Swimming must not grow the body');
            assert.ok(o.quaternion.angleTo(q)<.25,'Swimming joint exceeds gentle14degree envelope');seen[j].push(o.quaternion.clone());
          }
        }
        for(let j=0;j<rest.length;j++){
          assert.ok(seen[j].some(q=>q.angleTo(seen[j][0])>.04),key+' '+role+' leaves '+rest[j].o.name+' rigid');
          assert.ok(seen[j][0].toArray().every((v,k)=>Math.abs(v-seen[j][32].toArray()[k])<1e-6),'Swimming loop must return continuously to its opening body pose');
        }
        mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
      }
    }
    if(key==='drg:Stone Guardian'){
      const transform=gltf.scene.getObjectByName('ModelRoot');assert.ok(transform);
      const restScale=transform.scale.toArray(),restPosition=transform.position.clone(),restRotation=transform.quaternion.clone();
      const restBones=[];gltf.scene.traverse(o=>{if(o.isBone&&o!==transform)restBones.push([o,o.position.toArray(),o.quaternion.toArray(),o.scale.toArray()]);});
      for(const role of ['idle','attack','special','win']){
        const clip=clips.find(c=>c.name===role);assert.ok(clip,key+' needs '+role);
        const mixer=new THREE.AnimationMixer(gltf.scene),action=mixer.clipAction(clip);action.setLoop(THREE.LoopOnce,1);action.clampWhenFinished=true;action.play();
        for(let i=0;i<=32;i++){
          mixer.setTime(clip.duration*i/32);assert.deepEqual(transform.scale.toArray(),restScale,'Rigid statue scale must never animate');
          for(const [bone,p,q,s]of restBones){assert.deepEqual(bone.position.toArray(),p);assert.deepEqual(bone.quaternion.toArray(),q);assert.deepEqual(bone.scale.toArray(),s);}
          assert.ok(transform.quaternion.angleTo(restRotation)<.2,'Statue rotation exceeds subtle12degree envelope');
        }
        mixer.setTime(clip.duration);assert.ok(transform.position.distanceTo(restPosition)<1e-5,'Statue action must return to origin');mixer.stopAllAction();mixer.uncacheRoot(gltf.scene);
      }
    }
    console.log('Bounded living-motion regression passed: '+key);
  }
}
main().catch(error=>{console.error(error);process.exitCode=1;});
