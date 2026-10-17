const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('faction-wars',pace);const R=DT.HYPER;
 const cases=[...DT.WISPS.flatMap(w=>Array.from({length:64},(_,n)=>({wisp:w.id,n,augment:null}))),...Object.keys(DT.AUGMENT).flatMap(augment=>Array.from({length:16},(_,n)=>({wisp:null,n,augment})))];
 for(const sample of cases){
  const {n,augment}=sample,seed=n*43+17,g=Object.create(DT.Game.prototype);
  Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%7===0?'combat':'planning',stage:2+n%6,round:2,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',n%2===0,g);g.players=[p];g.refreshShop(p);const added=[];
  for(let j=0;j<n%14;j++){
   const u=g.newUnit(DT.SHOP_LINES[(n+Math.floor(j/3))%DT.SHOP_LINES.length].id,j%3===2?1:0);u.items=DT.CORE_ITEMS.slice(0,j%4);u.mods={other:j};
   if(j<4)p.board[`${j},4`]=u;else p.bench[j-4]=u;added.push({unit:structuredClone(u),board:j<4,col:j});
  }
  const drained=n%5===0?(p.allUnits()[0]?.line??null):null;if(drained)g.pool[drained]=0;
  p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);p.gold=n%4===0?0:n%4===1?5:100;p.hp=R.startHp-2;p.freeRolls=n%3;p.teamBonus=n%2;
  p.nextBuffs=n%3===0?{as:.2,armor:5,mana:7}:null;
  if(sample.wisp){const w=DT.WISP[sample.wisp];p.wisp={id:w.id,cost:DT.wispCost(w,g.stage)};}
  const snap=()=>structuredClone({gold:p.gold,hp:p.hp,rolls:p.freeRolls,teamBonus:p.teamBonus,buffs:p.nextBuffs,wisp:p.wisp,items:p.items,board:p.board,bench:p.bench,shop:p.shop,pool:g.pool});
  const before=snap();if(augment)g.applyAugment(p,augment);const picked=snap();
  const income=g.fireGame('onIncome',p,{base:5,interest:3,streak:2,augment:1,bonus:0});
  if(n%2===0)g.fireGame('onAiTurn',p,{});const ai=snap();
  const result=g.set.mechanic.actions.wisp(g.ctx(p),{}),acted=snap(),rounds=[];
  for(let k=0;k<6;k++){g.stage=1+k;g.round=1+(n+k)%2;g.fireGame('onRoundStart',p,{});rounds.push({stage:g.stage,round:g.round,after:snap()});}
  traces.push({variant:`faction-wars/${pace}`,n,seed,augment,human:p.isHuman,phase:g.phase,added,drained,before,picked,income,ai,result,acted,rounds,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/faction-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Faction Wars planning traces.`);



