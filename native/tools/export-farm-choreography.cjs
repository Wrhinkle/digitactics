const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/farm-ui.js'),'utf8');
const cases=[];
for(const count of [1,3,12])for(const delta of [1/60,.05,.1])for(const seed of [1,17]){
 let randomSeed=seed;const math=Object.create(Math);math.random=()=>{randomSeed=randomSeed*16807%2147483647;return randomSeed/2147483647;};
 const events=[],S={agents:new Map(),t:0},ctx={Math:math,PI:Math.PI,R:5.4,SPOT:{bed:[-3.2,-2.3]},BED_SLOTS:[[0,.05],[-.62,.05],[.62,.05],[0,.75],[-.62,.75],[.62,.75]],WANDER:{x:0,z:.3,rx:2.7,rz:2.3},S,pop(){}};
 vm.createContext(ctx);vm.runInContext(source.slice(source.indexOf('function walkTo('),source.indexOf('// ---------- poop piles ----------')),ctx);
 const vector=()=>({values:[],set(...v){this.values=v;}});
 const initial=[];
 for(let i=0;i<count;i++){
  const id='pet'+i,hasRun=i%2===0,needs={asleep:false,sick:i%4===1,energy:i%4===2?12:80,happy:i%4===3?20:85,tantrum:i===5};
  const a={id,data:needs,mode:'idle',t:math.random()*2,busy:0,tilt:0,tx:0,tz:0,goingToBed:false,then:null,bedSlot:-1,root:{position:vector(),rotation:vector(),scale:vector()},shadow:{position:vector()},shadowSet:true};
  const angle=i*2.39996,r=.7*Math.sqrt((i+1)/12);a.x=Math.cos(angle)*2.7*r;a.z=.3+Math.sin(angle)*2.3*r;
  a.view={yaw:0,hasRun,ready:true,moving:false,setMoving(v){this.moving=v;events.push([id,'moving',String(v)]);},play(role){events.push([id,'play',role]);},update(){}};
  S.agents.set(id,a);initial.push({id,hasRun,needs:{...needs}});
 }
 const frames=[];
 for(let frame=0;frame<600;frame++){
  if(frame===100)for(const a of S.agents.values())a.data.asleep=true;
  if(frame===400)for(const a of S.agents.values())a.data.asleep=false;
  S.t+=delta;events.length=0;
  for(const a of S.agents.values())ctx.updateAgent(a,delta);
  frames.push({events:[...events],actors:[...S.agents.values()].map(a=>({id:a.id,x:a.x,z:a.z,targetX:a.tx,targetZ:a.tz,mode:a.mode,wait:a.t,busy:a.busy,goingToBed:a.goingToBed,bedSlot:a.bedSlot,tilt:a.tilt,height:a.root.position.values[1],breathe:a.root.scale.values[1],yaw:a.view.yaw,moving:a.view.moving}))});
 }
 cases.push({count,delta,seed,initial,frames});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-choreography.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${cases.length} original farm movement/sleep/social traces.`);
