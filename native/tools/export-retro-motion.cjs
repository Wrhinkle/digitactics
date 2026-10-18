const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
(async()=>{
 const THREE=await import(require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
 const source=fs.readFileSync(path.join(root,'js/emotes.js'),'utf8'),cases=[];
 const pops=new Map(),camera={position:new THREE.Vector3(3,16,18),matrixWorld:new THREE.Matrix4()};
 const ctx={THREE,pops,POP_MS:2500,rend:()=>({camera}),clearPop:s=>pops.delete(s),addFx(){}};vm.createContext(ctx);
 vm.runInContext(source.slice(source.indexOf('const _side ='),source.indexOf('// Screen point above')),ctx);
 const actions=['hug','cane','ufo','shiver','flex','float','swoop','salute','stomp'];
 for(const act of actions)for(let i=0;i<200;i++){
  const t=i===199?3:i*.015,angle=i*.13;camera.matrixWorld.makeRotationY(angle);
  const view={group:new THREE.Object3D()},holder=new THREE.Object3D(),obj=new THREE.Object3D();
  view.group.position.set(Math.sin(i)*3,.2,Math.cos(i)*3);new THREE.Group().add(view.group);obj.userData={s:.7,rotY:.37};
  const p={act,view,holder,obj,t0:0,fx:[],nextFx:0};pops.set(0,p);ctx.updatePops(t*1000);
  cases.push({act,t,companion:view.group.position.toArray(),camera:camera.position.toArray(),right:[Math.cos(angle),0,-Math.sin(angle)],expected:pops.has(0)?{model:[...obj.position.toArray(),obj.rotation.x,obj.rotation.y,obj.rotation.z,...obj.scale.toArray()],holder:[...holder.position.toArray(),holder.rotation.x,holder.rotation.y,holder.rotation.z,...holder.scale.toArray()]}:null});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/retro-motion.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
 console.log(`Exported ${cases.length} original retro model/holder poses.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
