const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(let n=0;n<160;n++){
 DT.Sets.activate('fates',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*19+13;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%3===0?'combat':'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.players=[p];g.refreshShop(p);
 const stage=1+n%9,round=1+n%R.roundsInStage(stage),xp=n%12*16;
 g.stage=stage;g.round=round;if(R.xp)g.addXp(p,xp);else p.level=R.levelByStage[Math.min(stage,R.levelByStage.length-1)];p.gold=500;if(n%2)p.augs.push('fat_seeker');p.mech.partnerSeen=n%5===0;
 const snap=()=>structuredClone({gold:p.gold,seen:!!p.mech.partnerSeen,shop:p.shop,meta:p.shopMeta,bench:p.bench,pool:g.pool});
 const steps=[];for(let k=0;k<8;k++){
  g.refreshShop(p,k%3);const rolled=snap(),slot=p.shopMeta?.findIndex(m=>m?.tag==='Partner')??-1,added=[];let drain=null;
  if(slot>=0&&k===0){const line=p.shop[slot];if(n%4===1){drain=line;g.pool[line]=0;}
   if(n%4===3){for(let j=0;j<2;j++){const u=g.newUnit(line,1);p.bench[j]=u;added.push(structuredClone(u));}}
   if(n%4===2){for(let j=0;j<9;j++){const u=g.newUnit(j<2?line:DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id,j<2?0:2);p.bench[j]=u;added.push(structuredClone(u));}}
  }
  const bought=slot>=0?g.buy(p,slot)!==null:false,after=snap();let sold=null;
  if(k%3===2){const target=p.allUnits().find(u=>u.mods?.partner);if(target){sold=target.uid;g.sell(p,sold);}}
  steps.push({rolled,slot,added,drain,bought,after,sold,final:snap()});
 }
 traces.push({variant:`fates/${pace}`,human,n,seed,phase:g.phase,stage,round,xp,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/fates-partners.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Fates Partner traces.`);
