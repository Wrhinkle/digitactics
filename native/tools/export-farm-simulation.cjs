const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8')).legend;
const cosmetics=read('cosmetics.js'),evo=read('legendevo.js'),farmSource=read('farm.js');
function load(wallet,legend,farm,clock){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legend)storage['dt.legendevo.v1']=JSON.stringify(legend);if(farm)storage['dt.farm.v1']=JSON.stringify(farm);
 class Clock extends Date { constructor(...a){super(...(a.length?a:[clock.time]));}static now(){return clock.time;} }
 const ctx={DT:{},console,Date:Clock,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);ctx.DT.Cosmetics.register('legend',catalog);vm.runInContext('(function(){'+evo+'})();',ctx);vm.runInContext('(function(){'+farmSource+'})();',ctx);
 return ctx.DT;
}
const cases=[],start=1792260300000;
for(let n=0;n<catalog.length;n++){
 const clock={time:start},id=catalog[n].id,wallet={owned:{legend:catalog.map(x=>x.id)},equipped:{legend:id}},legend={v:1,legends:{},games:[]};
 const bootstrap=load(wallet,legend,null,clock);
 cases.push({wallet,legend:JSON.parse(JSON.stringify(legend)),input:null,time:start,expected:JSON.parse(JSON.stringify(bootstrap.Farm._save())),events:bootstrap.Farm.drainEvents()});
 const reachable=bootstrap.LegendEvo.reach(id);const form=reachable[n%reachable.length];
 legend.legends[id]={xp:2000,form,path:[],dex:[id,form],nonce:0,care:{bond:1,battle:2,food:3}};
 let input=JSON.parse(JSON.stringify(bootstrap.Farm._save()));input.future='keep';input.cap=n%2?12:6;input.lights=n%3!==0;
 input.residents.push(id,'unknown');input.seen=[];input.meat={n:n%3,t:start-60000*60};input.bandage={n:n%2,t:start-60000*90};
 for(const p of Object.values(input.pets)){
  p.hunger=n%4?80:0;p.happy=n%5?70:0;p.energy=[4,19,49,99][n%4];p.disc=n%3?65:0;p.weight=n%2?60:10;
  p.poopClock=n%2?179:0;p.starveMin=n%4?0:59;p.sadMin=n%5?0:119;p.sleepyMin=59;p.sick=n%7===0;p.sickMin=n%7===0?359:0;p.tantrum=n%11===0?start:0;p.future={keep:true};
 }
 // Keep a known pet in the bank; seen prevents automatic re-entry.
 if(n%2){input.residents=input.residents.filter(x=>x!==catalog[0].id);input.seen=[catalog[0].id];}
 const dt=load(wallet,legend,input,clock);
 for(const delta of [0,1,29,31,-10,180,360,720,2000]){
  clock.time+=delta*60000+ (delta===29?12345:0);if(delta!==0)dt.Farm.tick();
  const expected=JSON.parse(JSON.stringify(dt.Farm._save())),events=dt.Farm.drainEvents();
  cases.push({wallet,legend,input,time:clock.time,expected,events});input=expected;
 }
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-simulation.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${cases.length} original farm catch-up snapshots and event lists.`);
