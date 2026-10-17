const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['fates','reckoning','coliseum'])for(const pace of ['normal','hyper']){
 DT.Sets.activate(set,pace);const R=DT.HYPER,ids={fates:['fat_dice','fat_holy','fat_dark'],reckoning:['rec_tome'],coliseum:['col_extractor']}[set],cases=[];
 for(const id of ids)for(let n=0;n<720;n++){
  const seed=n%7,human=n%2===0,board=n%5===0,phase=['planning','combat','result'][Math.floor(n/2)%3];
  const line=id==='fat_dice'?DT.SHOP_LINES[n%DT.SHOP_LINES.length].id:id.startsWith('fat_')&&n%4!==0?'fat_lopmon':DT.SHOP_LINES[0].id;
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p={id:0,isHuman:human,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);
  const xp=n%500,roundStage=n%10+1;if(R.xp)g.addXp(p,xp);else p.level=R.levelByStage[Math.min(roundStage,R.levelByStage.length-1)];
  const u=g.newUnit(line,n%4===0?2:0);p.bench[0]=u;
  const mods=[null,{}, {pu:''},{pu:'test_power'}, {pu:'test_power',other:7}, {pu:false,other:'keep'}][n%6];if(mods)u.mods=structuredClone(mods);
  const preUse=id==='rec_tome'&&n%4===0;
  if(preUse){p.items=[id];g.equipItem(p,0,u.uid);}
  if(board){p.bench[0]=null;p.board['0,4']=u;}
  const fallen=n%7===0?'holy':null;if(fallen)p.mech.fallen=fallen;
  p.mech.cores=n%3;
  const poolMode=n%5;
  if(id==='fat_dice')for(const l of DT.SHOP_LINES)if(poolMode===0||poolMode===1&&l.tier!==5||poolMode===2&&l.tier!==1)g.pool[l.id]=0;
  p.items=Array.from({length:n%4===0?R.itemCap:4},()=>DT.CORE_ITEMS[0]);p.items[1]=id;
  if(id.startsWith('fat_')&&id!=='fat_dice'){p.items[2]='fat_holy';p.items[3]='fat_dark';}
  const inventory=p.items.slice();g.phase=phase;
  const result=g.equipItem(p,1,u.uid);
  cases.push({seed,human,board,phase,line,stage:u.stage,mods,preUse,fallen,cores:n%3,poolMode,xp,roundStage,inventory,result,after:{items:p.items,mods:u.mods||null,fallen:p.mech.fallen||null,cores:p.mech.cores,offer:p.offer||null,pool:{...g.pool},shop:p.shop,nextRandom:g.rng()}});
 }
 traces.push({variant:`${set}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/special-consumables.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} special-consumable traces.`);
