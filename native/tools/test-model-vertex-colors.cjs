// Test the actual browser preparation correction, including shared RGB/RGBA materials.
const fs = require('node:fs'), path = require('node:path'), vm = require('node:vm'), assert = require('node:assert/strict');
async function main() {
  const {THREE} = await require('./three-loader.cjs')();
  const source = fs.readFileSync(path.resolve(__dirname,'../../js/render3d.js'),'utf8');
  const correction = source.slice(source.indexOf('  scene.traverse((o) => {'),source.indexOf('  const actions = {}'));
  let checks = 0;
  for (const order of [[0,3,4],[4,3,0],[3,4,4]]) {
    const shared = new THREE.MeshBasicMaterial({map:new THREE.Texture(),transparent:true,depthWrite:false,opacity:1});
    const meshes = order.map(size => ({isMesh:true,material:shared,geometry:{getAttribute:()=>size?{itemSize:size}:undefined}}));
    vm.runInNewContext(correction,{THREE,scene:{traverse:fn=>meshes.forEach(fn)},src:{}});
    for (let i=0;i<meshes.length;i++) {
      const m=meshes[i].material,alpha=order[i]===4;
      assert.equal(m.transparent,alpha);assert.equal(m.depthWrite,!alpha);assert.equal(m.alphaTest,alpha?0:.5);checks+=3;
      assert.equal(m.map,shared.map);checks++;
    }
  }
  for (const size of [0,3,4]) for (const mode of ['opaque','mask','blend']) {
    const alphaTest=mode==='mask'?.5:0;
    const material=new THREE.MeshBasicMaterial({map:new THREE.Texture(),transparent:mode==='blend',depthWrite:mode!=='blend',alphaTest,opacity:.7});
    const mesh={isMesh:true,material,geometry:{getAttribute:()=>size?{itemSize:size}:undefined}};
    vm.runInNewContext(correction,{THREE,scene:{traverse:fn=>fn(mesh)},src:{frontOnly:true}});
    assert.equal(mesh.material.opacity,.7);assert.equal(mesh.material.transparent,mode==='blend');assert.equal(mesh.material.alphaTest,alphaTest);assert.equal(mesh.material.side,THREE.FrontSide);checks+=4;
  }
  console.log(`Passed ${checks} source RGB/RGBA material correction and shared-material checks.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
