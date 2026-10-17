const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const seed of [0,143,934])for(const concreteHooks of set.id==='coliseum'?[false,true]:[false]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',hasHook:()=>false,duo:false,maxTier:Math.max(...DT.SHOP_LINES.map(l=>l.tier)),pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:false,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},boardUnits(){return Object.values(this.board);},freeBenchSlot(){return this.bench.indexOf(null);},cap(){return this.level;}};g.refreshShop(p);if(concreteHooks)delete g.hasHook;
 const steps=[];
 for(let n=0;n<120;n++){
  g.stage=1+Math.floor(n/7)%9;g.round=1+n%7;p.gold=n%5===0?300:n%61;p.freeRolls=n%19;
  const before={gold:p.gold,freeRolls:p.freeRolls};g.aiLevel(p);g.aiBuyPass(p);let rolls=0;const keep=g.aiKeep();
  while((p.freeRolls>0||p.gold-R.rerollCost>=keep)&&rolls<15){g.reroll(p);g.aiBuyPass(p);rolls++;}
  while(p.bench.filter(Boolean).length>6)if(!g.aiSellWorst(p))break;
  steps.push({stage:g.stage,round:g.round,before,rolls,keep,after:structuredClone({gold:p.gold,level:p.level,xp:p.xp,freeRolls:p.freeRolls,bench:p.bench,shop:p.shop,pool:g.pool,items:p.items,cores:p.mech.cores||0})});
 }
 traces.push({variant:`${set.id}/${pace}`,seed,concreteHooks,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-shopping.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} AI shopping traces.`);
