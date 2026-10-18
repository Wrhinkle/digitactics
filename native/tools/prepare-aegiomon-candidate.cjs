// Audit-only reduction of the exact original chr183a candidate. Not an approved asset importer.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..');
const originalHash='6fa0d32b77d9b78111e07aa5cff620a87c57bee62ba4a9f2ea805419bcb02f53';
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
async function main(){
 const [input,output,special='bs03']=process.argv.slice(2);
 if(!input||!output||!['bs01','bs02','bs03'].includes(special))throw Error('Usage: node prepare-aegiomon-candidate.cjs <original.glb> <audit-output-directory> [bs01|bs02|bs03]');
 const out=path.resolve(output),audit=path.resolve(root,'native/artifacts');
 if(!out.startsWith(audit+path.sep))throw Error('Candidate output must remain under native/artifacts.');
 const raw=fs.readFileSync(input);if(sha(raw)!==originalHash)throw Error('Original source hash differs; re-review its identity before conversion.');
 const paths=[path.join(root,'native/artifacts/aegiomon-recovery'),path.join(root,'tools/convert')];
 const {NodeIO}=require(require.resolve('@gltf-transform/core',{paths}));
 const {prune,dedup,resample}=require(require.resolve('@gltf-transform/functions',{paths}));
 const length=raw.readUInt32LE(12),json=JSON.parse(raw.subarray(20,20+length)),bin=raw.subarray(28+length);
 const roles={idle:'bn01',run:'br01',attack:'ba01',special,win:'bv01'},provenance=[];
 json.animations=Object.entries(roles).map(([role,suffix])=>{
  const body=json.animations.find(a=>a.name===`chr183a_${suffix}`),face=json.animations.find(a=>a.name===`chrh183a_${suffix}`);
  if(!body)throw Error('Missing original body clip '+suffix);
  const clip=structuredClone(body);clip.name=role;
  if(face){const offset=clip.samplers.length;clip.samplers.push(...face.samplers);clip.channels.push(...face.channels.map(c=>({...c,sampler:c.sampler+offset})));}
  provenance.push({role,body:body.name,face:face?.name||'rest pose',channels:clip.channels.length});return clip;
 });
 const omitted=[];
 for(const node of json.nodes)if(node.mesh!==undefined&&/outline|ouline/i.test(json.meshes[node.mesh].name)){omitted.push(json.meshes[node.mesh].name);delete node.mesh;delete node.skin;}
 for(const material of json.materials){const p=material.pbrMetallicRoughness||={};delete p.metallicRoughnessTexture;p.metallicFactor=0;p.roughnessFactor=1;delete material.normalTexture;}
 const encoded=Buffer.from(JSON.stringify(json)),jp=(4-encoded.length%4)%4,bp=(4-bin.length%4)%4;
 const packed=Buffer.alloc(28+encoded.length+jp+bin.length+bp);packed.writeUInt32LE(0x46546c67,0);packed.writeUInt32LE(2,4);packed.writeUInt32LE(packed.length,8);
 packed.writeUInt32LE(encoded.length+jp,12);packed.writeUInt32LE(0x4e4f534a,16);encoded.copy(packed,20);packed.fill(32,20+encoded.length,20+encoded.length+jp);
 packed.writeUInt32LE(bin.length+bp,20+encoded.length+jp);packed.writeUInt32LE(0x004e4942,24+encoded.length+jp);bin.copy(packed,28+encoded.length+jp);
 const io=new NodeIO(),doc=await io.readBinary(packed);await doc.transform(prune(),dedup(),resample());const result=await io.writeBinary(doc);
 fs.mkdirSync(out,{recursive:true});fs.writeFileSync(path.join(out,'candidate.glb'),result);
 const report={status:'UNVERIFIED audit candidate; do not install',originalHash,candidateHash:sha(result),bytes:result.length,provenance,omitted,materialWarning:'Custom eye matcap and tint/transparency remain unconverted; standard PBR approximation is not visually approved.'};
 fs.writeFileSync(path.join(out,'candidate.json'),JSON.stringify(report,null,2));console.log(JSON.stringify(report,null,2));
}
main().catch(error=>{console.error(error);process.exitCode=1;});
