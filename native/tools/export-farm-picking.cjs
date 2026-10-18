const fs=require('fs'),path=require('path'),zlib=require('zlib');
async function main(){
 const {THREE}=await require('./three-loader.cjs')(); const traces=[];
 for(const [radius,height,segments] of [[.3,.5,8],[.5,1.3,10]]) for(const scale of [.01,.6,1,1.2]) for(const tilt of [0,1.15]){
  const root=new THREE.Group();root.position.set(-1.1,.2,.7);root.rotation.set(0,.37,tilt);root.scale.set(scale,scale*1.02,scale);
  const hit=new THREE.Mesh(new THREE.CylinderGeometry(radius,radius,height,segments),new THREE.MeshBasicMaterial());hit.position.y=height/2;root.add(hit);root.updateMatrixWorld(true);
  for(let x=-10;x<=10;x++)for(let y=-8;y<=8;y++){
   const origin=new THREE.Vector3(4,7,8),target=new THREE.Vector3(-1.1+x*.13,.2+y*.13,.7),direction=target.sub(origin).normalize();
   const ray=new THREE.Raycaster(origin,direction);const intersections=ray.intersectObject(hit,false);
   traces.push({radius,height,segments,matrix:root.matrixWorld.toArray(),origin:origin.toArray(),direction:direction.toArray(),distance:intersections[0]?.distance??null});
  }
 }
 fs.writeFileSync(path.resolve(__dirname,'../tests/fixtures/farm-picking.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
 console.log(`Exported ${traces.length} original farm cylinder raycasts.`);
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
