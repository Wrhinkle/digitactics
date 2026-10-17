const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('dragonlands',pace);const R=DT.HYPER,golden=Object.values(DT.ITEMS).filter(i=>i.cat==='gold').map(i=>i.id),ordinary=Object.keys(DT.ITEMS).find(id=>DT.ITEMS[id].cat!=='gold'),augs=Object.keys(DT.AUGMENT);
 for(let n=0;n<512;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:'planning',stage:1+n%8,round:1+Math.floor(n/8)%5,mech:{treasure:'classic'},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',true,g);g.players=[p];g.refreshShop(p);
  p.gold=n%35-5;p.hp=R.startHp+n%3-1;p.freeRolls=n%3;p.augs=['drg_calcloss','drg_treasure_hoard'].filter((_,i)=>n&(1<<i));p.augs.push(augs[n%augs.length]);p.augs=[...new Set(p.augs)];
  p.mech={bank:0,hpFull:false,calcRoll:n%2,augRolled:n%7===0?1:0,augOpts:n%5===0?null:n%3===0?[p.augs.at(-1)]:['drg_hoard']};
  p.items=Array(Math.floor(n/16)%4===0?30:Math.floor(n/16)%4===1?29:0).fill(ordinary).concat(golden.slice(0,n%golden.length));
  const added=[];
  for(let i=0;i<4;i++){const u=g.newUnit(DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length].id,0);u.items=[ordinary,...golden.slice(i,i+Math.floor(n/4)%4)];if(i===0)u.bonusTraits={GOLD:n%10};const board=i<2;if(board)p.board[`${i},4`]=u;else p.bench[i-2]=u;added.push({unit:structuredClone(u),board,col:i});}
  const before=structuredClone({gold:p.gold,hp:p.hp,rolls:p.freeRolls,augs:p.augs,mech:p.mech,items:p.items});g.makeOffer(p);
  const snapshots=[];for(let k=0;k<2;k++){g.fireGame('onRoundStart',p,{});snapshots.push(structuredClone({mech:p.mech,rolls:p.freeRolls,items:p.items,units:p.allUnits(),offer:p.offer}));}
  traces.push({variant:`dragonlands/${pace}`,seed:n+1,stage:g.stage,round:g.round,before,added,snapshots,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/dragon-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Dragonlands planning traces.`);
