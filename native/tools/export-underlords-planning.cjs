const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['underlords','underlords_ko'])for(const pace of ['normal','hyper']){
 DT.Sets.activate(set,pace);const R=DT.HYPER;
 for(let n=0;n<512;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:'planning',stage:1+Math.floor(n/ R.ulPerStage)%12,round:1+n%R.ulPerStage,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',true,g);g.players=[p];g.refreshShop(p);
  p.globals=['ul_g_recruiter','ul_g_smuggler','ul_g_riches'].filter((_,i)=>n&(1<<i));if(n%11===0)p.globals.push('ul_g_recruiter');
  if(n%13===0)p.globals.push(...Object.values(DT.ITEMS).filter(i=>i.cat==='global').map(i=>i.id));
  p.gold=n%150;p.streak=n%23-11;p.freeRolls=n%4;p.mech.ulFree=!!(n%2);
  const before=structuredClone({gold:p.gold,streak:p.streak,globals:p.globals,rolls:p.freeRolls,free:p.mech.ulFree});
  const income=g.fireGame('onIncome',p,{base:9,interest:3,streak:5,augment:2,bonus:1});
  g.makeOffer(p);const snapshots=[];
  const snap=()=>structuredClone({gold:p.gold,streak:p.streak,rolls:p.freeRolls,free:p.mech.ulFree,offer:p.offer});
  g.fireGame('onRoundStart',p,{});snapshots.push(snap());g.fireGame('onRoundStart',p,{});snapshots.push(snap());
  if(p.offer&&p.offer.rerolls>0){g.rerollOffer(p);g.fireGame('onRoundStart',p,{});snapshots.push(snap());}
  const items=Object.keys(DT.ITEMS).slice(0,n%4),u=g.newUnit(DT.SHOP_LINES[0].id,1);u.items=items.slice();p.bench[0]=u;p.items=Array(n%3===0?30:n%3===1?29:0).fill(Object.keys(DT.ITEMS)[0]);const inventory=p.items.slice();g.fireGame('onCombine',p,{unit:u});
  traces.push({variant:`${set}/${pace}`,seed:n+1,stage:g.stage,round:g.round,before,income,snapshots,items,inventory,afterItems:u.items,afterInventory:p.items,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/underlords-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Underlords planning traces.`);
