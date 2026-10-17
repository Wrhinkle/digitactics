// Dorugamon's body is solid geometry. The imported blended material incorrectly
// treats its texture alpha as body transparency, and the renderer's cutout pass
// then removes most of it. Correct this one material without changing the texture,
// geometry, skeleton or animation data. Safe to rerun after importing this asset.
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const file=path.join(__dirname,'../../models/files/ef72384761d943c48e2890f125b13f0c.glb');
const input=fs.readFileSync(file);
assert.equal(input.readUInt32LE(0),0x46546c67);assert.equal(input.readUInt32LE(4),2);
assert.equal(input.readUInt32LE(16),0x4e4f534a);
const size=input.readUInt32LE(12),gltf=JSON.parse(input.subarray(20,20+size).toString());
const material=gltf.materials.find(m=>m.name==='chr396a01');assert.ok(material,'Expected Dorugamon body material');
material.alphaMode='OPAQUE';delete material.alphaCutoff;
const json=Buffer.from(JSON.stringify(gltf)),padded=Buffer.alloc(Math.ceil(json.length/4)*4,0x20);json.copy(padded);
const header=Buffer.from(input.subarray(0,20)),tail=input.subarray(20+size);
header.writeUInt32LE(20+padded.length+tail.length,8);header.writeUInt32LE(padded.length,12);
fs.writeFileSync(file,Buffer.concat([header,padded,tail]));
console.log('Corrected Dorugamon body material; binary mesh/texture/animation data unchanged');
