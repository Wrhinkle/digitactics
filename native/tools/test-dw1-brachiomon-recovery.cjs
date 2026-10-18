// Independent preservation proof for the exact exploded-source repair.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {pathToFileURL}=require('node:url');
const {recover,originalSha,parts}=require('./recover-dw1-brachiomon.cjs');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/artifacts/dw1-rig-diagnosis');
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
let checks=0;const assert=(ok,message)=>{checks++;if(!ok)throw Error(message);};
async function inspect(bytes){
  let j,bin;for(let p=12;p<bytes.length;){const n=bytes.readUInt32LE(p),type=bytes.readUInt32LE(p+4),chunk=bytes.subarray(p+8,p+8+n);if(type===0x4e4f534a)j=JSON.parse(chunk);else if(type===0x004e4942)bin=chunk;p+=8+n;}
  const {MeshoptDecoder}=await import(pathToFileURL(path.join(root,'vendor/three/addons/libs/meshopt_decoder.module.js')));await MeshoptDecoder.ready;
  const views=j.bufferViews.map(v=>{const e=v.extensions?.EXT_meshopt_compression;if(!e)return bin.subarray(v.byteOffset||0,(v.byteOffset||0)+v.byteLength);const data=Buffer.alloc(e.count*e.byteStride);MeshoptDecoder.decodeGltfBuffer(data,e.count,e.byteStride,bin.subarray(e.byteOffset||0,(e.byteOffset||0)+e.byteLength),e.mode,e.filter);return data;});
  const sizes={5120:1,5121:1,5122:2,5123:2,5125:4,5126:4},components={SCALAR:1,VEC2:2,VEC3:3,VEC4:4};
  function tuple(id,i){const a=j.accessors[id],v=j.bufferViews[a.bufferView],size=sizes[a.componentType]*components[a.type],p=(a.byteOffset||0)+i*(v.byteStride||size);return views[a.bufferView].subarray(p,p+size).toString('hex');}
  const triangles=[];
  for(const mesh of j.meshes)for(const p of mesh.primitives){const a=j.accessors[p.indices];for(let i=0;i<a.count;i+=3){const vertices=[];for(let k=0;k<3;k++){const b=Buffer.from(tuple(p.indices,i+k),'hex'),index=a.componentType===5123?b.readUInt16LE():b.readUInt32LE();vertices.push(Object.entries(p.attributes).sort(([a],[b])=>a.localeCompare(b)).map(([key,id])=>key+':'+tuple(id,index)).join('/'));}triangles.push(p.material+'|'+vertices.join('|'));}}
  return {j,triangles:triangles.sort(),images:j.images.map(i=>views[i.bufferView]),tuple};
}
async function main(){
  const source=path.join(root,'models/files/rip_dw1_brachiomon.glb'),backup=path.join(root,'models/originals/rip_dw1_brachiomon.glb'),current=fs.readFileSync(source),original=sha(current)===originalSha?current:fs.readFileSync(backup);
  assert(sha(original)===originalSha,'Exact original source hash');
  const {candidate,report}=await recover(original),second=await recover(original);assert(candidate.equals(second.candidate),'Deterministic reconstruction');
  const before=await inspect(original),after=await inspect(candidate);
  assert(before.triangles.length===732&&after.triangles.length===732,'All 732 original triangles');
  before.triangles.forEach((triangle,i)=>assert(triangle===after.triangles[i],'Ordered triangle attribute tuples/winding preserved '+i));
  assert(JSON.stringify(before.j.materials)===JSON.stringify(after.j.materials),'Materials identical');
  assert(JSON.stringify(before.j.textures)===JSON.stringify(after.j.textures),'Textures identical');
  assert(JSON.stringify(before.j.samplers)===JSON.stringify(after.j.samplers),'Samplers identical');
  before.images.forEach((image,i)=>assert(image.equals(after.images[i]),'Embedded original texture bytes identical'));
  assert(after.j.nodes.length===19&&after.j.scenes[0].nodes.length===19,'All original components are independent scene roots');
  assert(!after.j.skins?.length&&!after.j.animations?.length,'Recovery adds no deformation or animation data');
  after.j.nodes.forEach((n,i)=>{
    assert(n.name==='Brachiomon_'+parts[i][0],'Named anatomical component');
    assert(JSON.stringify(n.scale)===JSON.stringify(before.j.nodes[0].scale),'Original scale retained');
    assert(JSON.stringify(n.rotation)===JSON.stringify(before.j.nodes[0].rotation),'Original rotation retained');
    assert(n.translation.every((v,a)=>Math.abs(v-before.j.nodes[0].translation[a]-parts[i][3][a])<1e-12),'Only declared rigid translation');
    const p=after.j.meshes[n.mesh].primitives[0],a=after.j.accessors[p.attributes.POSITION];
    assert(a.count===report.parts[i].sourceVertexIds.length&&a.count<before.j.accessors[before.j.meshes[0].primitives[0].attributes.POSITION].count,'Each part has compact local vertex subset');
  });
  // Wrong source must fail closed before any writes. This function only returns buffers.
  const changed=Buffer.from(original);changed[changed.length-1]^=1;let rejected=false;try{await recover(changed);}catch{rejected=true;}assert(rejected,'Reject unknown source bytes');
  const result={checks,failures:0,originalSha256:originalSha,recoveredSha256:sha(candidate),triangles:732,components:19,embeddedTextureSha256:before.images.map(sha),proof:'Complete multiset of ordered per-triangle position/normal/UV tuples, material assignments, original encoded image bytes, and declared-only node transforms.'};
  fs.writeFileSync(path.join(out,'brachiomon-recovery-checks.json'),JSON.stringify(result,null,2));console.log(JSON.stringify(result,null,2));
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
