const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('reckoning',pace);const R=DT.HYPER;
 for(const id of Object.keys(DT.AUGMENT))for(let n=0;n<32;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',n%2===0,g);g.players=[p];g.refreshShop(p);p.gold=17;p.hp=R.blessAt+n%7-3;p.freeRolls=n%3;
  p.mech={blessed:n%5===0,blessEarly:n%7===0,eggs:Array.from({length:n%6},(_,i)=>({size:1+(n+i)%3,left:(n+i)%5,golden:i%2===0}))};
  p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);const added=[];
  for(let i=0;i<2;i++){const u=g.newUnit(DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length].id,i);u.items=DT.CORE_ITEMS.slice(i,i+2);if(i===0)p.board['0,4']=u;else p.bench[0]=u;added.push({unit:structuredClone(u),board:i===0});}
  const snap=()=>structuredClone({gold:p.gold,hp:p.hp,rolls:p.freeRolls,teamBonus:p.teamBonus,mech:p.mech,items:p.items,orbs:p.orbs,offer:p.offer});
  const before=snap();g.applyAugment(p,id);const picked=snap(),income=g.fireGame('onIncome',p,{base:5,interest:3,streak:2,augment:1,bonus:0}),steps=[];
  for(let k=0;k<4;k++){
   g.stage=2+k;g.round=1+(n+k)%2;g.roundType=R.roundType(g.stage,g.round);const won=(n+k)%3===0,pve=(n+k)%5===0,counts={DRK:[0,3,5,9][(n+k)%4]};
   g.roundEnd(p,counts,{team:0,won,pve,match:{combat:null}});g.makeOffer(p);g.fireGame('onRoundStart',p,{});
   steps.push({stage:g.stage,round:g.round,type:g.roundType,won,pve,counts,after:snap()});
  }
  traces.push({variant:`reckoning/${pace}`,id,n,human:p.isHuman,added,before,picked,income,steps,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/reckoning-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Reckoning planning traces.`);
