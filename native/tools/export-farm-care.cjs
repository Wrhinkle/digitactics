const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
const allCatalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8')),catalog=allCatalog.legend;
const cosmetics=read('cosmetics.js'),evo=read('legendevo.js'),farmSource=read('farm.js'),time=1792260300000;
function load(wallet,legend,farm){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legend)storage['dt.legendevo.v1']=JSON.stringify(legend);if(farm)storage['dt.farm.v1']=JSON.stringify(farm);
 class Clock extends Date {constructor(...a){super(...(a.length?a:[time]));}static now(){return time;}}
 const ctx={DT:{},console,Date:Clock,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);for(const [kind,items] of Object.entries(allCatalog))ctx.DT.Cosmetics.register(kind,items);vm.runInContext('(function(){'+evo+'})();',ctx);vm.runInContext('(function(){'+farmSource+'})();',ctx);
 return {DT:ctx.DT,storage};
}
const operations=[...['meat','carrot','bigmeat','sirloin','cake','unknown'].map(option=>({action:'Feed',fn:'feed',option})),
 {action:'Clean',fn:'clean'},{action:'CleanPile',fn:'cleanPile',pile:1},...['bandage','medicine'].map(option=>({action:'Cure',fn:'cure',option})),
 ...[null,true,false].map(lights=>({action:'Lights',fn:'setLights',lights})),{action:'Praise',fn:'praise'},{action:'Scold',fn:'scold'},
 {action:'Play',fn:'play'},{action:'Play',fn:'play',option:'tsunomon'},{action:'Pet',fn:'petIt'}];
operations.push({action:'MoveIn',fn:'moveIn',target:'bank'},{action:'MoveIn',fn:'moveIn',target:'unknown'},
 {action:'MoveOut',fn:'moveOut'},{action:'Swap',fn:'swap',target:'bank'},{action:'Expand',fn:'buyExpansion'},
 ...['meadow','tropical','snowy','sewer','toy','grid','unknown'].flatMap(theme=>[{action:'BuyTheme',fn:'buyTheme',theme},{action:'SetTheme',fn:'setTheme',theme}]));
async function main(){
const destination=path.join(root,'native/tests/fixtures/farm-care.json.gz');
const output=fs.createWriteStream(destination+'.tmp'),gzip=zlib.createGzip();
const completed=require('stream/promises').pipeline(gzip,output);
// Serialize each VM result immediately, so fixtures do not retain whole VM contexts.
const write=async text=>{if(!gzip.write(text))await require('events').once(gzip,'drain');};
let count=0;
await write('[');
operations.push(...['strength','speed','wisdom','stamina','unknown'].flatMap(option=>[-1,0,.59,.6,.7,.8,1,2].map(score=>({action:'Train',fn:'train',option,score}))));
for(let i=0;i<catalog.length;i++)for(let mode=0;mode<4;mode++){
 const id=catalog[i].id,wallet={bits:mode===3?0:1000,owned:{legend:catalog.map(x=>x.id),board:mode===2?allCatalog.board.map(x=>x.id):[]},equipped:{legend:id},history:[],future:true};
 const legend={v:1,legends:{[id]:{xp:mode===2?1999:mode===3?2000:0,form:id,path:[],dex:[id],nonce:0,care:{bond:0,battle:0,food:0}}},games:[],future:true};
 const initial=load(wallet,legend,null).DT.Farm._save();const farm=JSON.parse(JSON.stringify(initial));farm.future='keep';farm.cap=mode===2?12:mode===3?10:6;if(mode===1)farm.residents=[id];
 farm.meat.n=mode%2?0:3;farm.bandage.n=mode%2?0:2;farm.daily.bits=mode===2?59:0;farm.daily.data=mode===2?99:0;
 const pet=farm.pets[id];pet.hunger=[40,100,95,40][mode];pet.sick=mode>=2;pet.energy=mode===1?5:80;pet.flags.asleep=mode===1;
 pet.tantrum=mode===2?time+600000:0;pet.disc=mode===2?20:70;pet.lastPet=mode===1?time:0;pet.lastPraise=mode===1?time:0;
 pet.poop=mode===1?[]:[{t:time,x:0,z:0,n:false,k:1}];pet.pileN=1;pet.dirt=400;
 const bankId=catalog[(i+20)%catalog.length].id;if(mode===1)farm.pets[bankId]={...JSON.parse(JSON.stringify(pet)),t:time-86400000,hunger:12};
 for(const op of operations){
  const actionId=op.target==='bank'?bankId:op.target==='unknown'?'unknown':op.theme||id,option=op.action==='Swap'?id:op.option;
  const actionFarm=JSON.parse(JSON.stringify(farm));
  if(op.action==='Train'){
   const p=actionFarm.pets[id];
   if(mode>=2){p.sick=false;p.tantrum=0;for(const key of ['str','spd','wis','sta'])p[key]=299;}
   if(mode===1 && i%4!==0){p.flags.asleep=false;p.energy=i%4===1?14:80;p.hunger=i%4===2?9:40;p.tantrum=i%4===3?time+600000:0;p.disc=39;}
  }
  const ctx=load(wallet,legend,actionFarm);let result;
  if(op.action==='Lights')result=ctx.DT.Farm.setLights(op.lights);
  else if(op.action==='Train')result=ctx.DT.Farm.train(actionId,option,op.score);
  else result=ctx.DT.Farm[op.fn](actionId,op.pile??option);
  await write((count++?',':'')+JSON.stringify({...op,id:actionId,option,time,wallet,legend,farm:actionFarm,result,farmAfter:ctx.DT.Farm._save(),walletAfter:JSON.parse(ctx.storage['dt.cosmetics.v1']),legendAfter:JSON.parse(ctx.storage['dt.legendevo.v1']),events:ctx.DT.Farm.drainEvents()}));
 }
}
gzip.end(']');await completed;fs.renameSync(destination+'.tmp',destination);
console.log(`Exported ${count} original farm care transactions.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
