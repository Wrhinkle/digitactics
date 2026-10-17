const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const seed of [17,91]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',stage:1,round:1,mech:{galaxy:'server'},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const setup=(p,n)=>{p.augs=set.id==='gizmos'?(n%3===0?['giz_a_highend','giz_a_ticket']:n%3===1?['giz_a_ticket']:[]):[];p.globals=set.id==='autochess'?Array(n%3).fill('ac_r_recruiter'):(set.id==='underlords'||set.id==='underlords_ko')?Array(n%3).fill('ul_g_criminal'):[];p.mech.doots=n%5*5;p.mech.starShop=n%4===0;};
 const players=g.players=Array.from({length:2},(_,id)=>({id,isHuman:id===0,gold:500,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,freeRolls:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of players)setup(p,p.id+3);for(const p of players)g.refreshShop(p);
 const snap=()=>structuredClone({pool:g.pool,players:players.map(p=>({gold:p.gold,rolls:p.freeRolls,shop:p.shop,meta:p.shopMeta,seen:!!p.mech.partnerSeen,star:!!p.mech.starShop}))});
 const initial=snap(),steps=[];
 for(let n=0;n<160;n++){
  const p=players[n%2];setup(p,n);g.stage=1+Math.floor(n/20);g.round=1+n%R.roundsInStage(g.stage);g.mech.galaxy=n%4<2?'server':'island';
  if(R.xp)g.addXp(p,4);else p.level=R.levelByStage[Math.min(g.stage,R.levelByStage.length-1)];
  let added=null;if(n%17===0&&p.freeBenchSlot()>=0){const u=g.newUnit(DT.SHOP_LINES[n%DT.SHOP_LINES.length].id,2);p.bench[p.freeBenchSlot()]=u;added=structuredClone(u);}
  let result=true;if(n%3===0)g.refreshShop(p,n%4);else result=g.reroll(p);
  steps.push({stage:g.stage,round:g.round,added,result,after:snap()});
 }
 traces.push({variant:`${set.id}/${pace}`,seed,initial,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-shop-rolls.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} match shop dispatch traces.`);
