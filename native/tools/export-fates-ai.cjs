const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(let n=0;n<128;n++){
 DT.Sets.activate('fates',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*53+19;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%3===0?'combat':'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:500,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,freeRolls:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.players=[p];g.refreshShop(p);
 if(R.xp)g.addXp(p,10000);else p.level=R.levelByStage[Math.min(10,R.levelByStage.length-1)];g.stage=2;g.round=2;
 const added=[];for(let j=0;j<(n%5===0?11:6);j++){
  const line=j<3?(n%3===0?DT.SHOP_LINES[n%DT.SHOP_LINES.length].id:'fat_lopmon'):DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id;
  const u=g.newUnit(line,j<3?n%2:j%3);u.items=j===0?DT.CORE_ITEMS.slice(0,n%4):[];if(n%11===0&&j===2){u.mods={partner:true,trait:DT.LINE[line].traits[0],bonus:'hp'};u.bonusTraits={[u.mods.trait]:1};}
  if(j<2)p.board[`${j},4`]=u;else p.bench[j-2]=u;added.push({unit:structuredClone(u),board:j<2,col:j});
 }
 p.items=n%4===0?['fat_dark']:n%4===1?[]:['fat_holy','fat_dark'];if(n%7!==0)p.items.push('fat_copy');if(n%5!==1)p.items.push('fat_dice');if(n%9===0)while(p.items.length<R.itemCap)p.items.push(DT.CORE_ITEMS[0]);p.mech.fallen=n%7===0?'holy':null;
 const drained=n%4===0?p.allUnits()[0].line:null;if(drained)g.pool[drained]=0;
 const before=structuredClone({items:p.items,fallen:p.mech.fallen});g.refreshShop(p);const beforeShop=structuredClone(p.shop);
 g.fireGame('onAiTurn',p,{});
 traces.push({variant:`fates/${pace}`,human,n,seed,phase:g.phase,added,drained,before,beforeShop,after:structuredClone({items:p.items,fallen:p.mech.fallen,seen:!!p.mech.partnerSeen,board:p.board,bench:p.bench,shop:p.shop,costs:p.shop.map((_,i)=>g.shopCost(p,i)),pool:g.pool}),nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/fates-ai.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Fates AI traces.`);
