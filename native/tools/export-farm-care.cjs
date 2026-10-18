const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8')).legend;
const cosmetics=read('cosmetics.js'),evo=read('legendevo.js'),farmSource=read('farm.js'),time=1792260300000;
function load(wallet,legend,farm){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legend)storage['dt.legendevo.v1']=JSON.stringify(legend);if(farm)storage['dt.farm.v1']=JSON.stringify(farm);
 class Clock extends Date {constructor(...a){super(...(a.length?a:[time]));}static now(){return time;}}
 const ctx={DT:{},console,Date:Clock,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);ctx.DT.Cosmetics.register('legend',catalog);vm.runInContext('(function(){'+evo+'})();',ctx);vm.runInContext('(function(){'+farmSource+'})();',ctx);
 return {DT:ctx.DT,storage};
}
const operations=[...['meat','carrot','bigmeat','sirloin','cake','unknown'].map(option=>({action:'Feed',fn:'feed',option})),
 {action:'Clean',fn:'clean'},{action:'CleanPile',fn:'cleanPile',pile:1},...['bandage','medicine'].map(option=>({action:'Cure',fn:'cure',option})),
 ...[null,true,false].map(lights=>({action:'Lights',fn:'setLights',lights})),{action:'Praise',fn:'praise'},{action:'Scold',fn:'scold'},
 {action:'Play',fn:'play'},{action:'Play',fn:'play',option:'tsunomon'},{action:'Pet',fn:'petIt'}];
const cases=[];
for(let i=0;i<catalog.length;i++)for(let mode=0;mode<4;mode++){
 const id=catalog[i].id,wallet={bits:mode===3?0:1000,owned:{legend:catalog.map(x=>x.id)},equipped:{legend:id},history:[],future:true};
 const legend={v:1,legends:{[id]:{xp:mode===2?1999:mode===3?2000:0,form:id,path:[],dex:[id],nonce:0,care:{bond:0,battle:0,food:0}}},games:[],future:true};
 const initial=load(wallet,legend,null).DT.Farm._save();const farm=JSON.parse(JSON.stringify(initial));farm.future='keep';
 farm.meat.n=mode%2?0:3;farm.bandage.n=mode%2?0:2;farm.daily.bits=mode===2?59:0;farm.daily.data=mode===2?99:0;
 const pet=farm.pets[id];pet.hunger=[40,100,95,40][mode];pet.sick=mode>=2;pet.energy=mode===1?5:80;pet.flags.asleep=mode===1;
 pet.tantrum=mode===2?time+600000:0;pet.disc=mode===2?20:70;pet.lastPet=mode===1?time:0;pet.lastPraise=mode===1?time:0;
 pet.poop=mode===1?[]:[{t:time,x:0,z:0,n:false,k:1}];pet.pileN=1;pet.dirt=400;
 for(const op of operations){
  const ctx=load(wallet,legend,farm);let result;
  if(op.action==='Lights')result=ctx.DT.Farm.setLights(op.lights);
  else result=ctx.DT.Farm[op.fn](id,op.pile??op.option);
  cases.push({id,...op,time,wallet,legend,farm,result,farmAfter:JSON.parse(JSON.stringify(ctx.DT.Farm._save())),walletAfter:JSON.parse(ctx.storage['dt.cosmetics.v1']),legendAfter:JSON.parse(ctx.storage['dt.legendevo.v1']),events:ctx.DT.Farm.drainEvents()});
 }
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-care.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${cases.length} original farm care transactions.`);
