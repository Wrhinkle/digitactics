// Restore pupils from the original imports before optimization removes unused UVs.
// node tools/convert/restore-eye-textures.cjs <rips/x> [source-dir=models/src]
// Then run optimize.mjs on the printed GLB filenames. Original textures retain their source credits.
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {NodeIO}=require('@gltf-transform/core');
const {ALL_EXTENSIONS}=require('@gltf-transform/extensions');
const rips=path.resolve(process.argv[2]),src=path.resolve(process.argv[3]||path.join(__dirname,'../../models/src'));
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):[path.join(d,e.name)]);
const pngs=walk(rips).filter(f=>/p01\.png$/i.test(f));
(async()=>{const io=new NodeIO().registerExtensions(ALL_EXTENSIONS);
for(const name of fs.readdirSync(src).filter(f=>f.endsWith('.glb'))){
 const file=path.join(src,name),doc=await io.read(file);let count=0;
 for(const m of doc.getRoot().listMaterials()){
  if(!/eye/i.test(m.getName()))continue;
  const id=m.getName().match(/chr\d+/i)?.[0];if(!id)continue;
  const png=pngs.find(f=>path.basename(f).toLowerCase()===id.toLowerCase()+'p01.png');if(!png)continue;
  const primitives=doc.getRoot().listMeshes().flatMap(mesh=>mesh.listPrimitives()).filter(p=>p.getMaterial()===m);
  assert.ok(primitives.every(p=>p.getAttribute('TEXCOORD_0')),name+': original eye UVs required');
  const t=doc.createTexture(id+'p01').setImage(fs.readFileSync(png)).setMimeType('image/png');
  m.setBaseColorTexture(t).setBaseColorFactor([1,1,1,1]).setAlphaMode('OPAQUE').setMetallicFactor(0).setRoughnessFactor(1);
  m.getBaseColorTextureInfo().setTexCoord(0);count++;
 }
 if(count){await io.write(file,doc);console.log(name+' '+count+' eye materials restored');}
}})().catch(e=>{console.error(e);process.exitCode=1;});
