const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
(async()=>{
 const THREE=await import(require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
 const source=fs.readFileSync(path.join(root,'js/dances.js'),'utf8'), mystery=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8');
 const ctx={THREE,DT:{Cosmetics:{register(){}}},C:{equipped:()=> 'floss'}};vm.createContext(ctx);
 vm.runInContext(mystery.slice(0,mystery.indexOf('export const killThemes')).replace(/export /g,'')+';const mysteryDances=dances;const TAU=Math.PI*2,DEG=Math.PI/180;',ctx);
 vm.runInContext(source.slice(source.indexOf('const D ='),source.indexOf('// ---------- LegendView driver')).replace('export class Dancer','class Dancer')+';this.Dancer=Dancer;this.dances=DANCES;',ctx);
 const kinds=['hips','spine','neck','head','up','fore','thigh','shin','foot','wing','ear','tail','up','fore','thigh','shin','foot','wing','ear','tail'];
 const cases=[];
 for(let shape=0;shape<4;shape++){
  const binding={limbless:shape===3,stubby:shape>1,yaw:shape*.37,bones:kinds.map((kind,i)=>({node:i,kind,parentEntry:i?Math.floor((i-1)/2):-1,direction:new THREE.Vector3(Math.sin(i+1),Math.cos(i+1),.2).normalize().toArray(),parentRotation:new THREE.Quaternion().setFromEuler(new THREE.Euler(i*.013,i*.029,-i*.011)).toArray(),restRotation:new THREE.Quaternion().setFromEuler(new THREE.Euler(.02*i,0,0)).toArray(),side:i<12?1:-1,share:kind==='spine'?.5:kind==='tail'?.25:1,index:i%3,limit:shape===2&&kind==='up'?65*Math.PI/180:0}))};
  for(const dance of ctx.dances)for(let n=0;n<18;n++){
   const time=n===17?17.983:n*.137,weight=[0,.01,.3,.8,1][n%5],pose=dance.pose(time*dance.bpm/60);
   const d=Object.create(ctx.Dancer.prototype);d.dance=dance;d.limbless=binding.limbless;d.stubby=binding.stubby;d.fq=new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0,1,0),binding.yaw);d.fqi=d.fq.clone().invert();
   d.rig={bones:binding.bones.map(b=>({P0:new THREE.Quaternion().fromArray(b.parentRotation),q0:new THREE.Quaternion().fromArray(b.restRotation)}))};
   d.bones=binding.bones.map((b,i)=>({quaternion:new THREE.Quaternion().setFromEuler(new THREE.Euler(.1,i*.02,-.1))}));
   const initial=d.bones.map(b=>b.quaternion.toArray());
   d.E=binding.bones.map((b,i)=>({bi:i,kind:b.kind,pe:b.parentEntry,d0:new THREE.Vector3().fromArray(b.direction),side:b.side,share:b.share,n:b.index,lim:b.limit,R:new THREE.Quaternion(),A:new THREE.Quaternion(),local:new THREE.Quaternion()}));
   const rt=d.apply(time,weight);delete rt.fq;
   cases.push({shape,dance:dance.id,time,weight,pose,initial,expected:d.bones.map(b=>b.quaternion.toArray()),root:rt,...(n===0&&dance===ctx.dances[0]?{binding}: {})});
  }
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/dance-skeleton.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
 let json=Buffer.from(JSON.stringify({asset:{version:'2.0'},nodes:kinds.map((_,i)=>({name:'bone'+i}))}));json=Buffer.concat([json,Buffer.alloc((4-json.length%4)%4,32)]);
 const glb=Buffer.alloc(28+json.length);glb.writeUInt32LE(0x46546c67);glb.writeUInt32LE(2,4);glb.writeUInt32LE(glb.length,8);glb.writeUInt32LE(json.length,12);glb.writeUInt32LE(0x4e4f534a,16);json.copy(glb,20);glb.writeUInt32LE(0,20+json.length);glb.writeUInt32LE(0x004e4942,24+json.length);
 fs.writeFileSync(path.join(root,'native/tests/fixtures/dance-skeleton.glb.gz'),zlib.gzipSync(glb));
 console.log(`Exported ${cases.length} retargeted poses from ${ctx.dances.length} original dances.`);
})().catch(e=>{console.error(e);process.exitCode=1;});
