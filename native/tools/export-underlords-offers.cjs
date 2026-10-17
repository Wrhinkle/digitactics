const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['underlords','underlords_ko'])for(const pace of ['normal','hyper'])for(const human of [false,true])for(const kind of ['ul_tamer','ul_crew']){
 DT.Sets.activate(set,pace);const R={...DT.HYPER,offers:[{kind,stage:1,round:1}]},g=Object.create(DT.Game.prototype),picks=[];
 Object.assign(g,{R,set:DT.SET,mech:{},uid:0,rng:DT.makeRng(143),phase:'planning',stage:1,round:1,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:R.startGold??3,freeRolls:0,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:['unrelated_global'],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);
 g.applyAugment=(p,id)=>{p.augs.push(id);picks.push(id);g.combine(p);};
 g.applyPower=(p,id)=>{if(!DT.POWER[id])return;p.power={id,rank:1};picks.push(id);g.combine(p);};
 const steps=[];
 for(let n=0;n<180;n++){
  const line=DT.SHOP_LINES[n%DT.SHOP_LINES.length].id;const added=[];
  if(n%5===0){p.board={};p.bench=Array(9).fill(null);for(let i=0;i<4;i++){const u=g.newUnit(line,0);u.items=DT.CORE_ITEMS.slice(0,i%4);if(i<2)p.board[`${i},4`]=u;else p.bench[i-2]=u;added.push(structuredClone(u));}}
  if(n%7===0)p.power=null;
  g.phase=n%4===0?'combat':'planning';g.mech.gates=n%5===0?null:[n%3+1];
  const fill=n%7===0;if(fill)p.items=Array(R.itemCap).fill(DT.CORE_ITEMS[0]||Object.keys(DT.ITEMS)[0]);
  g.makeOffer(p);const made=structuredClone(p.offer);const reroll=n%2===0?g.rerollOffer(p):null,rolled=structuredClone(p.offer);
  const selected=g.chooseOffer(p,n%3);
  steps.push({added,phase:g.phase,gates:g.mech.gates,fill,made,reroll,rolled,choice:n%3,selected,after:structuredClone({items:p.items,globals:p.globals,augs:p.augs,freeRolls:p.freeRolls,power:p.power||null,picks,shop:p.shop,pool:g.pool,board:p.board,bench:p.bench})});
 }
 traces.push({variant:`${set}/${pace}`,human,kind,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/underlords-offers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Underlords offer traces.`);
