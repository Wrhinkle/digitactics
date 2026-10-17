// Reference skeletal sampling uses the vendored Three.js AnimationMixer and Matrix4.
const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib');
const {pathToFileURL} = require('node:url');
const root = path.resolve(__dirname, '../..');
async function main() {
  const T = await import(pathToFileURL(path.join(root, 'vendor/three/build/three.module.js')));
  const catalog = JSON.parse(fs.readFileSync(path.join(root, 'native/content/models.json')));
  const types = {5120:Int8Array,5121:Uint8Array,5122:Int16Array,5123:Uint16Array,5125:Uint32Array,5126:Float32Array};
  const sizes = {SCALAR:1,VEC2:2,VEC3:3,VEC4:4,MAT4:16};
  const fixtures = []; let badClips = 0;
  for (const [asset, entry] of Object.entries(catalog.assets)) {
    const glb = zlib.gunzipSync(fs.readFileSync(path.join(root, 'native/artifacts/model-content', entry.file)));
    const size = glb.readUInt32LE(12), json = JSON.parse(glb.subarray(20,20+size)); const binary = glb.subarray(28+size);
    const cache = new Map();
    function accessor(index) {
      if (cache.has(index)) return cache.get(index);
      const a = json.accessors[index], v = json.bufferViews[a.bufferView], Type = types[a.componentType], components = sizes[a.type];
      const values = [];
      for (let i=0;i<a.count;i++) {
        const offset=(v.byteOffset||0)+(a.byteOffset||0)+i*(v.byteStride||components*Type.BYTES_PER_ELEMENT);
        const raw=binary.subarray(offset,offset+components*Type.BYTES_PER_ELEMENT);
        const attribute=new T.BufferAttribute(new Type(Uint8Array.from(raw).buffer),components,!!a.normalized);
        for(let c=0;c<components;c++)values.push(attribute.getComponent(0,c));
      }
      cache.set(index,values); return values;
    }
    const scene = new T.Group();
    const nodes = json.nodes.map(n => {
      const o = new T.Object3D();
      if(n.matrix)o.applyMatrix4(new T.Matrix4().fromArray(n.matrix));
      else {o.position.fromArray(n.translation||[0,0,0]);o.quaternion.fromArray(n.rotation||[0,0,0,1]);o.scale.fromArray(n.scale||[1,1,1]);}
      return o;
    });
    json.nodes.forEach((n,i)=>(n.children||[]).forEach(c=>nodes[i].add(nodes[c])));
    for(const n of nodes)if(!n.parent)scene.add(n);
    const selected = new Set([0,nodes.length-1]);
    const skinNodes=[];
    json.nodes.forEach((n,i)=>{if(n.skin!=null){skinNodes.push(i);selected.add(i);const j=json.skins[n.skin].joints;selected.add(j[0]);selected.add(j.at(-1));}});
    const samples=[];
    function capture(animation,time) {
      scene.updateMatrixWorld(true);
      const world=[...selected].map(i=>({node:i,matrix:nodes[i].matrixWorld.toArray()}));
      const skins=[];
      for(const mesh of skinNodes){const skin=json.skins[json.nodes[mesh].skin];const inverseMesh=nodes[mesh].matrixWorld.clone().invert();
        const matrices=skin.inverseBindMatrices!=null?accessor(skin.inverseBindMatrices):null;
        for(const i of new Set([0,Math.floor(skin.joints.length/2),skin.joints.length-1])){
          const bind=matrices?new T.Matrix4().fromArray(matrices,i*16):new T.Matrix4();
          if(!bind.elements.every(Number.isFinite))continue;
          skins.push({skin:json.nodes[mesh].skin,mesh,joint:i,matrix:inverseMesh.clone().multiply(nodes[skin.joints[i]].matrixWorld).multiply(bind).toArray()});
        }
      }
      samples.push({animation,time,world,skins});
    }
    capture(-1,0);
    for(const [index,a] of (json.animations||[]).entries()){
      let bad=false;
      const tracks=a.channels.map(ch=>{const s=a.samplers[ch.sampler], times=accessor(s.input),values=accessor(s.output);
        if(!values.every(Number.isFinite))bad=true;
        const property={translation:'position',rotation:'quaternion',scale:'scale'}[ch.target.path];
        const Track=ch.target.path==='rotation'?T.QuaternionKeyframeTrack:T.VectorKeyframeTrack;
        return new Track(nodes[ch.target.node].uuid+'.'+property,times,values,s.interpolation==='STEP'?T.InterpolateDiscrete:T.InterpolateLinear);
      });
      if(bad){badClips++;continue;}
      const clip=new T.AnimationClip(a.name,-1,tracks), mixer=new T.AnimationMixer(scene);
      const action=mixer.clipAction(clip);action.setLoop(T.LoopOnce,1);action.clampWhenFinished=true;action.play();
      for(const fraction of [0,.37,.91,1]){const time=clip.duration*fraction;mixer.setTime(time);capture(index,time);}
      mixer.stopAllAction();mixer.uncacheRoot(scene);
    }
    fixtures.push({asset,samples});
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/rigs.json.gz'),zlib.gzipSync(JSON.stringify(fixtures)));
  console.log(`Exported ${fixtures.length} rig traces; ${badClips} corrupt source clips excluded from reference transforms.`);
}
main().catch(e=>{console.error(e);process.exitCode=1;});
