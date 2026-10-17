const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let n=0;n<240;n++){
 DT.Sets.activate('dragonlands',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*41+71;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:false,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.players=[p];g.refreshShop(p);
 const stage=1+n%9,xp=n%16*25;if(R.xp)g.addXp(p,xp);else p.level=R.levelByStage[Math.min(stage,R.levelByStage.length-1)];p.gold=500;
 const added=[];for(let j=0;j<n%4;j++){const u=g.newUnit(DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id,2);p.bench[j]=u;added.push(structuredClone(u));}
 const snap=()=>structuredClone({gold:p.gold,star:!!p.mech.starShop,shop:p.shop,meta:p.shopMeta,bench:p.bench,pool:g.pool,rolls:p.freeRolls});
 const steps=[];for(let k=0;k<12;k++){
  p.mech.doots=[0,1,7.5,20,40][(n+k)%5];p.mech.starShop=k%3===0;
  const drained=[];if(k===0&&n%5===0)for(const l of DT.SHOP_LINES)if(n%10===0?l.traits.includes('STAR'):l.slots!==2){g.pool[l.id]=0;drained.push(l.id);}
  p.freeRolls=k%4===0?1:0;let rolled=true;if(k%2)rolled=g.reroll(p);else g.refreshShop(p,k%3);
  const afterRoll=snap(),slot=k%p.shop.length,bought=g.buy(p,slot)!==null;const after=snap();
  let sold=null;if(k%3===2){const u=p.bench.find(u=>u&&u.stage<2);if(u){sold=u.uid;g.sell(p,sold);}}
  steps.push({doots:p.mech.doots,star:k%3===0,drained,rolled,afterRoll,slot,bought,after,sold,final:snap()});
 }
 traces.push({variant:`dragonlands/${pace}`,n,seed,stage,xp,added,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/dragonlands-shops.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Dragonlands shop traces.`);
