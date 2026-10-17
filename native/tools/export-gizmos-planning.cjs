const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('gizmos',pace);const R=DT.HYPER;
 for(let n=0;n<1024;n++){
  const round=R.offers[n%R.offers.length],g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:'planning',stage:round.stage,round:n%7===0?round.round+1:round.round,mech:n%9===0?{}:{gates:[1+n%3,1+Math.floor(n/3)%3,1+Math.floor(n/9)%3]},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  g.roundType=R.roundType(g.stage,g.round);
  const p=new DT.Player(0,'Player',true,g);g.players=[p];g.refreshShop(p);p.gold=n%17;p.freeRolls=n%3;
  let aug=['giz_a_calc','giz_a_hyper','giz_a_trade','giz_a_highend','giz_a_reserves','giz_a_junkyard'];if(n%2)aug.reverse();p.augs=aug.filter((_,i)=>n&(1<<i));
  p.mech={calcRoll:n%2,goldStart:0,junk:n%5,chest:n%7===0?null:{gold:n%8,shut:n%9,items:Array(n%6).fill(DT.CORE_ITEMS[0])}};
  p.items=Array([0,R.itemCap-1,R.itemCap][Math.floor(n/11)%3]).fill(DT.CORE_ITEMS[1]);
  const added=[];for(let i=0;i<2+n%10;i++){const u=g.newUnit(i<2?'giz_gekomon':DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length].id,i%3);u.mods={glutHp:(n+i)%70*20,glutAp:(n+i)%70,keep:'kept'};if(i===0)u.bonusTraits={PIR:n%8};const board=i<2;if(board)p.board[`${i},4`]=u;else p.bench[i-2]=u;added.push({unit:structuredClone(u),board,col:i});}
  if(n%13===0)for(const l of DT.SHOP_LINES)if(l.traits.includes('PET'))g.pool[l.id]=0;
  const before=structuredClone({gold:p.gold,rolls:p.freeRolls,augs:p.augs,mech:p.mech,items:p.items,pool:g.pool});
  g.fireGame('onRoundStart',p,{});const started=structuredClone({gold:p.gold,rolls:p.freeRolls,mech:p.mech,items:p.items});
  const counts={PET:n%4===0?0:8,JUNK:n%3===0?0:8},won=n%3!==0,pve=n%5===0,missing=n%17===0;
  let combines=0;g.combine=function(...args){combines++;return DT.Game.prototype.combine.apply(this,args);};
  const gold=g.roundEnd(p,counts,{team:0,won,pve,match:{combat:missing?null:{traitCounts:[counts,counts]}}});
  const sold=g.newUnit(DT.SHOP_LINES[(n*3)%DT.SHOP_LINES.length].id,n%3);g.fireGame('onSell',p,{unit:sold});
  const fed=structuredClone(p.allUnits().filter(u=>u.line==='giz_gekomon').map(u=>u.mods));
  const holder=p.allUnits().find(u=>u.line==='giz_gekomon'),sources=[{mods:{glutHp:n%100*15,glutAp:n%80}},{mods:{glutHp:250,glutAp:32}},{}];g.fireGame('onCombine',p,{unit:holder,from:sources});
  traces.push({variant:`gizmos/${pace}`,seed:n+1,stage:g.stage,round:g.round,type:g.roundType,gates:g.mech.gates||null,added,before,started,counts,won,pve,missing,gold,combines,sold,sources,fed,after:structuredClone({gold:p.gold,rolls:p.freeRolls,mech:p.mech,items:p.items,board:p.board,bench:p.bench,pool:g.pool}),nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/gizmos-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Gizmos planning traces.`);
