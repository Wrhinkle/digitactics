const fs=require('node:fs'),zlib=require('node:zlib'),path=require('node:path'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);let n=0;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(91),phase:'planning',pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]])),hasHook:name=>name==='onShop'||name==='onShopRolled',fireGame(name,p,e){
  if(name==='onShop'){e.level+=n%3;e.size=n%12-2;if(n%2)e.odds=[10.5,20.5,30,25,14];}
  else if(name==='onShopRolled'){
   const line=DT.SHOP_LINES[n%DT.SHOP_LINES.length].id;
   if(g.pool[line]>0&&g.shopEligible(p,line)){if(p.shop[0])g.pool[p.shop[0]]++;g.takeFromPool(line);p.shop[0]=line;}
   p.shopMeta=p.shop.map((id,i)=>id?{cost:(n+i)%6,tag:'Special',unit:{stage:n%3,bonusTraits:{test:1},mods:{marker:n}}}:null);
  }return e;
 }});
 const p={gold:500,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],freeRolls:0,isHuman:false,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};
 const steps=[];for(n=0;n<80;n++){
  if(n%10===0)p.bench=Array(9).fill(null);
  p.freeRolls=n%3===0?1:0;
  let rolled=true;if(n%2)rolled=g.reroll(p);else g.refreshShop(p,n%4);
  const dice=set.id==='fates'&&n%9===0, diceLine=DT.SHOP_LINES[n%DT.SHOP_LINES.length].id;let diceResult=null;
  if(dice){p.items=['fat_dice'];diceResult=DT.ITEMS.fat_dice.use(g.ctx(p),{line:diceLine},0);}
  const shop=structuredClone(p.shop),meta=structuredClone(p.shopMeta),costs=shop.map((_,i)=>g.shopCost(p,i));
  const slot=n%p.shop.length,bought=g.buy(p,slot)!=null;
  steps.push({dice,diceLine,diceResult,rolled,shop,meta,costs,slot,bought,after:structuredClone({gold:p.gold,freeRolls:p.freeRolls,shop:p.shop,meta:p.shopMeta,bench:p.bench,pool:g.pool})});
 }
 traces.push({variant:`${set.id}/${pace}`,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/shop-rolls.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} shop roll traces.`);
