const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('dragonlands',pace);const R=DT.HYPER;
 for(const id of Object.keys(DT.AUGMENT))for(let n=0;n<40;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(n+1),phase:n%2?'combat':'planning',stage:3,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',Math.floor(n/2)%2===0,g);g.players=[p];g.refreshShop(p);p.gold=17;p.items=Array(n%3===0?R.itemCap:n%3===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);p.teamBonus=n%3;
  const targets=['drg_dorumon','drg_kamemon','drg_impmon','drg_gotsumon','drg_syakomon','drg_tsukaimon'],added=[];
  for(let i=0;i<2+n%10;i++){const line=i<3?targets[Math.floor(n/4)%targets.length]:DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length].id,u=g.newUnit(line,0);u.items=[DT.CORE_ITEMS[(n+i)%DT.CORE_ITEMS.length]];if(i===0)u.bonusTraits={GOLD:n%4};const board=i<2;if(board)p.board[`${i},4`]=u;else p.bench[i-2]=u;added.push({unit:structuredClone(u),board,col:i});}
  if(n%5===0)for(const l of DT.SHOP_LINES)if(l.slots===2||targets.includes(l.id))g.pool[l.id]=0;
  const before=structuredClone({items:p.items,pool:g.pool});let combines=0;g.combine=function(...args){combines++;return DT.Game.prototype.combine.apply(this,args);};
  g.applyAugment(p,id);
  traces.push({variant:`dragonlands/${pace}`,id,n,human:p.isHuman,phase:g.phase,added,before,combines,after:structuredClone({gold:p.gold,items:p.items,teamBonus:p.teamBonus,board:p.board,bench:p.bench,shop:p.shop,pool:g.pool}),nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/dragon-augments.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Dragonlands augment traces.`);
