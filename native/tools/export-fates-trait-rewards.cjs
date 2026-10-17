const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('fates',pace);const R=DT.HYPER,artifacts=DT.Sets.def('fates').artifacts;
 for(let n=0;n<1024;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:'combat',stage:3,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',true,g);g.players=[p];g.refreshShop(p);
  p.mech={luck:n%11,deva:Math.floor(n/3)%7,fallenFights:n%5,fallenGiven:n%7===0,fallen:[null,'holy','dark',null][Math.floor(n/7)%4],forge:n%31,forged:Math.floor(n/11)%6};
  p.items=Array([0,R.itemCap-3,R.itemCap-2,R.itemCap-1,R.itemCap][Math.floor(n/5)%5]).fill(DT.CORE_ITEMS[0]);if(n%9===0)p.items=artifacts.slice();
  const added=[];for(let i=0;i<2;i++){const u=g.newUnit(n%6===0?DT.SHOP_LINES[0].id:'fat_volcamon',(n+i)%3);const board=i===0?n%4!==0:n%8===0;if(board)p.board[`${i},4`]=u;else p.bench[p.freeBenchSlot()]=u;added.push({unit:u,board,col:i});}
  const order=n%2?['FORGE','FALLEN','DEVA','TREASURE']:['TREASURE','DEVA','FALLEN','FORGE'],counts={};for(let i=0;i<order.length;i++){const id=order[i],t=DT.TRAITS[id];counts[id]=(Math.floor(n/13)&(1<<i))?0:t.bp[Math.floor(n/17)%t.bp.length];}
  const before=structuredClone({items:p.items,mech:p.mech}),snapshots=[];
  for(let k=0;k<3;k++){const won=(n+k)%3!==0,pve=n%5===0,gold=g.roundEnd(p,counts,{team:0,won,pve,match:{combat:null}});snapshots.push(structuredClone({won,pve,gold,items:p.items,mech:p.mech}));}
  traces.push({variant:`fates/${pace}`,seed:n+1,counts,added,before,snapshots,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/fates-trait-rewards.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Fates trait reward traces.`);
