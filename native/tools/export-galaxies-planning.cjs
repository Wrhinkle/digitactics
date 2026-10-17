const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true]){
 DT.Sets.activate('galaxies',pace);const galaxies=DT.Sets.def('galaxies').galaxies.map(g=>g.id);
 for(const galaxy of galaxies)for(let n=0;n<32;n++){
 const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*31+11;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%3===0?'combat':'planning',stage:1,round:1,mech:{galaxy},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:500,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,freeRolls:2,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.players=[p];g.refreshShop(p);
 const bounty=DT.SHOP_LINES.find(l=>l.traits.includes('BH')).id; p.mech.bounty={[bounty]:n%11};
 const held=DT.CORE_ITEMS[n%DT.CORE_ITEMS.length],added=[];
 for(let j=0;j<(n%4===0?11:6);j++){
  const line=j<3?bounty:DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id,stage=j<3?0:j%3;
  const u=g.newUnit(line,stage);u.items=j===2?[held,held,Object.keys(DT.ITEMS).find(id=>DT.ITEMS[id].component)]:[];
  if(j<2)p.board[`${j},4`]=u;else p.bench[j-2]=u;added.push({unit:structuredClone(u),board:j<2,col:j});
 }
 p.items=n%5===0?Array(R.itemCap).fill(held):n%5===1?Array(R.itemCap-1).fill(held):Array(n%4).fill('gal_copier');
 const available=[0,1,2,8,20][n%5];g.pool[bounty]=available;
 const [stage,round]=[[1,1],[1,2],[2,1],[3,1],[4,1],[3,2]][n%6];g.stage=stage;g.round=round;
 const before=structuredClone({items:p.items,pool:g.pool});
 const snap=()=>structuredClone({gold:p.gold,rolls:p.freeRolls,items:p.items,bounty:p.mech.bounty,shop:p.shop,board:p.board,bench:p.bench,pool:g.pool});
 g.fireGame('onRoundStart',p,{});const size=g.fireGame('onTeamSize',p,{size:5}).size,afterRound=snap();
 const sold=p.bench[0].uid,sale=g.sell(p,sold),afterSale=snap();
 // The ordinary sale can return enough copies for a forced Bounty Hunter offer.
 let forced=false;if(g.pool[bounty]>0&&g.shopEligible(p,bounty)){if(p.shop[0])g.pool[p.shop[0]]++;g.takeFromPool(bounty);p.shop[0]=bounty;forced=true;}
 const bought=g.buy(p,0)!==null,afterBuy=snap();g.fireGame('onAiTurn',p,{});
 traces.push({variant:`galaxies/${pace}`,human,galaxy,n,seed,bounty,phase:g.phase,stage,round,added,before,size,afterRound,sold,sale,afterSale,forced,bought,afterBuy,final:snap(),nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/galaxies-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Galaxies planning references.`);
