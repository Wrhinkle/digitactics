// Exact-source, reversible rigid-component recovery. No remeshing or UV changes.
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const {pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/artifacts/dw1-rig-diagnosis');
const file=path.join(root,'models/files/rip_dw1_brachiomon.glb'),backup=path.join(root,'models/originals/rip_dw1_brachiomon.glb');
const originalSha='dab4ffdfe1cb755921a8daaa1bc51caa97f70ef03b37cfdb826c49d0887cb031';
const sha=b=>crypto.createHash('sha256').update(b).digest('hex');
// Original connected pieces, identified independently by bounds and triangle count.
// Offsets seat the outer joint rings; the original rounded end caps overlap inside
// the adjoining piece, as they do in the other articulated DW1 source imports.
const parts=[
  ['Torso',78,[0,.14,-.30],[0,0,0]],
  ['NeckBase',42,[0,8.70,2.64],[0,-3.65,-.10]],
  ['Neck1',24,[0,14.54,2.49],[0,-5.50,-.20]],
  ['Neck2',24,[0,18.79,2.49],[0,-7.18,-.20]],
  ['Neck3',24,[0,22.82,2.39],[0,-8.88,-.10]],
  ['NeckTop',24,[0,27.16,2.97],[0,-10.83,-.15]],
  ['Head',86,[0,36.35,4.59],[0,-15.20,0]],
  ['Jaw',36,[0,31.60,4.47],[0,-13.77,.16]],
  ['FrontLeftUpper',38,[-3.70,-3.14,3.25],[.80,1.30,-.20]],
  ['FrontLeftFoot',48,[-4.18,-7.56,3.55],[.80,3.10,-.30]],
  ['FrontRightUpper',38,[3.88,-3.14,3.47],[-.80,1.30,-.20]],
  ['FrontRightFoot',48,[4.14,-7.56,3.55],[-.80,3.10,-.30]],
  ['BackLeftUpper',38,[-3.73,-3.51,-4.04],[1.10,.80,.35]],
  ['BackLeftFoot',48,[-4.08,-7.81,-3.89],[1.10,3.10,.65]],
  ['BackRightUpper',38,[3.80,-3.51,-4.01],[-1.10,.80,.35]],
  ['BackRightFoot',48,[4.04,-7.81,-3.89],[-1.10,3.10,.65]],
  ['TailBase',22,[0,-.99,-9.53],[0,.05,2.10]],
  ['Tail1',22,[0,-.98,-14.53],[0,.05,3.936]],
  ['TailTip',6,[0,-.98,-18.96],[0,.05,5.031]],
];
function readGlb(b){
  if(b.readUInt32LE(0)!==0x46546c67||b.readUInt32LE(4)!==2||b.readUInt32LE(8)!==b.length)throw Error('Invalid GLB');
  let json,bin;for(let i=12;i<b.length;){const n=b.readUInt32LE(i),kind=b.readUInt32LE(i+4),chunk=b.subarray(i+8,i+8+n);if(kind===0x4e4f534a)json=JSON.parse(chunk);if(kind===0x004e4942)bin=chunk;i+=8+n;}return {json,bin};
}
function writeGlb(json,bin){
  const s=Buffer.from(JSON.stringify(json)),j=Buffer.alloc((s.length+3)&~3,32);s.copy(j);
  const b=Buffer.alloc((bin.length+3)&~3);bin.copy(b);const result=Buffer.alloc(28+j.length+b.length);
  result.writeUInt32LE(0x46546c67);result.writeUInt32LE(2,4);result.writeUInt32LE(result.length,8);
  result.writeUInt32LE(j.length,12);result.writeUInt32LE(0x4e4f534a,16);j.copy(result,20);
  result.writeUInt32LE(b.length,20+j.length);result.writeUInt32LE(0x004e4942,24+j.length);b.copy(result,28+j.length);return result;
}
async function recover(original){
  if(sha(original)!==originalSha)throw Error('Unexpected original Brachiomon GLB');
  const {json,bin}=readGlb(original),{MeshoptDecoder}=await import(pathToFileURL(path.join(root,'vendor/three/addons/libs/meshopt_decoder.module.js')));await MeshoptDecoder.ready;
  const views=json.bufferViews.map(v=>{const e=v.extensions?.EXT_meshopt_compression;if(!e)return Buffer.from(bin.subarray(v.byteOffset||0,(v.byteOffset||0)+v.byteLength));const b=Buffer.alloc(e.count*e.byteStride);MeshoptDecoder.decodeGltfBuffer(b,e.count,e.byteStride,bin.subarray(e.byteOffset||0,(e.byteOffset||0)+e.byteLength),e.mode,e.filter);return b;});
  const types={5120:[1,'readInt8'],5121:[1,'readUInt8'],5122:[2,'readInt16LE'],5123:[2,'readUInt16LE'],5125:[4,'readUInt32LE'],5126:[4,'readFloatLE']},dims={SCALAR:1,VEC2:2,VEC3:3,VEC4:4};
  function tuple(index,vertex){const a=json.accessors[index],v=json.bufferViews[a.bufferView],width=types[a.componentType][0]*dims[a.type],start=(a.byteOffset||0)+vertex*(v.byteStride||width);return views[a.bufferView].subarray(start,start+width);}
  function values(index,vertex){const a=json.accessors[index],[width,reader]=types[a.componentType],b=tuple(index,vertex);return Array.from({length:dims[a.type]},(_,i)=>b[reader](i*width));}
  if(json.nodes.length!==1||json.meshes.length!==1||json.meshes[0].primitives.length!==1||json.animations?.length||json.skins?.length)throw Error('Source layout changed');
  const originalNode=json.nodes[0],primitive=json.meshes[0].primitives[0],position=json.accessors[primitive.attributes.POSITION];
  const points=Array.from({length:position.count},(_,i)=>values(primitive.attributes.POSITION,i).map((v,a)=>Math.max(-1,v/32767)*originalNode.scale[a]+originalNode.translation[a]));
  const indices=Array.from({length:json.accessors[primitive.indices].count},(_,i)=>values(primitive.indices,i)[0]);
  const parent=points.map((_,i)=>i),find=i=>{while(parent[i]!==i){parent[i]=parent[parent[i]];i=parent[i];}return i;},join=(a,b)=>parent[find(a)]=find(b),weld=new Map();
  points.forEach((_,i)=>{const key=tuple(primitive.attributes.POSITION,i).toString('hex');if(weld.has(key))join(i,weld.get(key));else weld.set(key,i);});
  for(let i=0;i<indices.length;i+=3){join(indices[i],indices[i+1]);join(indices[i+1],indices[i+2]);}
  const groups=new Map();for(let i=0;i<indices.length;i+=3){const k=find(indices[i]);if(!groups.has(k))groups.set(k,[]);groups.get(k).push(indices.slice(i,i+3));}
  if(groups.size!==19)throw Error('Expected 19 original components');
  const bounds=points=>({min:[0,1,2].map(a=>Math.min(...points.map(p=>p[a]))),max:[0,1,2].map(a=>Math.max(...points.map(p=>p[a])))});
  const components=[...groups.values()].map(triangles=>{const ids=[...new Set(triangles.flat())].sort((a,b)=>a-b),b=bounds(ids.map(i=>points[i]));return {triangles,ids,bounds:b,center:b.min.map((v,a)=>(v+b.max[a])/2)};});
  const result=JSON.parse(JSON.stringify(json));result.bufferViews=[];result.accessors=[];result.meshes=[];result.nodes=[];result.scenes=[{nodes:parts.map((_,i)=>i)}];result.scene=0;
  for(const k of ['extensionsUsed','extensionsRequired'])result[k]=(result[k]||[]).filter(e=>e!=='EXT_meshopt_compression');
  const chunks=[];let length=0;const addView=(b,extra={})=>{const pad=(4-length%4)%4;if(pad){chunks.push(Buffer.alloc(pad));length+=pad;}const index=result.bufferViews.length;result.bufferViews.push({buffer:0,byteOffset:length,byteLength:b.length,...extra});chunks.push(b);length+=b.length;return index;};
  result.images.forEach((image,i)=>{image.bufferView=addView(views[json.images[i].bufferView]);});
  const reportParts=[];
  for(const [index,[name,count,center,offset]] of parts.entries()){
    const candidates=components.filter(c=>c.triangles.length===count).map(c=>({c,d:Math.hypot(...center.map((v,a)=>v-c.center[a]))})).sort((a,b)=>a.d-b.d);
    if(!candidates.length||candidates[0].d>.06)throw Error('Component identity mismatch '+name);
    const c=candidates[0].c;components.splice(components.indexOf(c),1);const remap=new Map(c.ids.map((v,i)=>[v,i])),attributes={};
    for(const [semantic,accessor] of Object.entries(primitive.attributes)){
      const source=json.accessors[accessor],width=types[source.componentType][0]*dims[source.type],stride=(width+3)&~3,data=Buffer.alloc(c.ids.length*stride);
      c.ids.forEach((v,i)=>tuple(accessor,v).copy(data,i*stride));
      const a={...source,name:name+'_'+semantic,bufferView:addView(data,{target:34962,byteStride:stride}),byteOffset:0,count:c.ids.length};
      if(source.min)a.min=Array.from({length:dims[source.type]},(_,axis)=>Math.min(...c.ids.map(v=>values(accessor,v)[axis])));
      if(source.max)a.max=Array.from({length:dims[source.type]},(_,axis)=>Math.max(...c.ids.map(v=>values(accessor,v)[axis])));
      attributes[semantic]=result.accessors.length;result.accessors.push(a);
    }
    const data=Buffer.alloc(c.triangles.length*6);c.triangles.flat().forEach((v,i)=>data.writeUInt16LE(remap.get(v),i*2));
    const indexAccessor=result.accessors.length;result.accessors.push({type:'SCALAR',componentType:5123,count:c.triangles.length*3,bufferView:addView(data,{target:34963}),byteOffset:0});
    const nodeName='Brachiomon_'+name;result.meshes.push({name:nodeName,primitives:[{...primitive,attributes,indices:indexAccessor}]});
    result.nodes.push({...originalNode,name:nodeName,mesh:index,translation:originalNode.translation.map((v,a)=>v+offset[a])});
    const pts=c.ids.map(i=>points[i].map((v,a)=>v+offset[a]));reportParts.push({index,name:nodeName,offset,sourceVertexIds:c.ids,points:pts,triangles:c.triangles.map(t=>t.map(v=>remap.get(v))),bounds:bounds(pts)});
  }
  if(components.length)throw Error('Unassigned source component');
  result.buffers=[{byteLength:length}];const candidate=writeGlb(result,Buffer.concat(chunks));
  // Independent byte-level proof: all original attribute tuples and original
  // triangle winding appear once; the only geometry-space change is node TRS.
  const read=readGlb(candidate);let compared=0;
  for(const p of reportParts){const prim=read.json.meshes[p.index].primitives[0];
    for(const [semantic,accessor] of Object.entries(prim.attributes)){const a=read.json.accessors[accessor],v=read.json.bufferViews[a.bufferView],width=types[a.componentType][0]*dims[a.type];for(const [i,sourceIndex] of p.sourceVertexIds.entries()){const actual=read.bin.subarray(v.byteOffset+i*v.byteStride,v.byteOffset+i*v.byteStride+width);if(!actual.equals(tuple(primitive.attributes[semantic],sourceIndex)))throw Error('Changed attribute tuple');compared++;}}
  }
  const report={sourceUrl:'https://models.spriters-resource.com/playstation/digimonworld/asset/336247/',archiveSha256:'c352330480da0be3689fc2adee39b294518114deeecc5d6acbda4fb3e3688ba6',objSha256:'499eacafb11680aacca629498ed05540d6ed186a8c75ffd9a14a78f80732a21c',originalSha256:originalSha,recoveredSha256:sha(candidate),triangles:indices.length/3,attributeTuplesCompared:compared,policy:'Original 19 connected pieces, compacted without altering vertex attribute tuples or triangle order/winding; rigid translations reseat attachment regions. Assembly is a repair, not a claim of recovered original game transforms.',parts:reportParts};
  return {candidate,report};
}
async function main(){
  fs.mkdirSync(out,{recursive:true});const current=fs.readFileSync(file),original=sha(current)===originalSha?current:fs.readFileSync(backup),{candidate,report}=await recover(original);
  if(![originalSha,sha(candidate)].includes(sha(current)))throw Error('Refusing to overwrite unrecognized source');
  const action=process.argv[2]||'--preview';if(!['--preview','--apply','--restore'].includes(action))throw Error('Use --preview, --apply or --restore');
  fs.writeFileSync(path.join(out,'DW1_Brachiomon_Recovered.glb'),candidate);fs.writeFileSync(path.join(out,'DW1_Brachiomon_Recovered.json'),JSON.stringify(report));
  if(action==='--apply'){fs.mkdirSync(path.dirname(backup),{recursive:true});if(fs.existsSync(backup)&&sha(fs.readFileSync(backup))!==originalSha)throw Error('Backup changed');fs.writeFileSync(backup,original);fs.writeFileSync(file,candidate);}
  if(action==='--restore')fs.writeFileSync(file,original);
  console.log(JSON.stringify({action,...Object.fromEntries(Object.entries(report).filter(([k])=>k!=='parts'))},null,2));
}
if(require.main===module)main().catch(e=>{console.error(e);process.exitCode=1;});
module.exports={recover,originalSha,parts};
