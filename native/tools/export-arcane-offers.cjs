const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(const seed of [0,143]){
 DT.Sets.activate('arcane',pace);const kinds=['arcAnomaly','arcMarket','arcJunk'],R={...DT.HYPER,offers:kinds.map((kind,i)=>({kind,stage:1,round:i+1,rerolls:2}))},g=Object.create(DT.Game.prototype),rng=DT.makeRng(927),pick=a=>a[Math.floor(rng()*a.length)];
 Object.assign(g,{R,set:DT.SET,mech:{},uid:0,rng:DT.makeRng(seed),phase:'planning',stage:1,round:1,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);const steps=[];
 for(let n=0;n<300;n++){
  p.board={};p.bench=Array(9).fill(null);const setup=[];
  for(let i=0;i<n%10;i++){const u=g.newUnit(pick(DT.SHOP_LINES).id,i%3);u.items=Array.from({length:i%4},()=>pick(DT.CORE_ITEMS));u.mods={other:7,arcAnom:n%4===0?'x_power':null};u.noItems=i%7===0;const board=i<6,c=i%3+2,r=4+Math.floor(i/3);if(board)p.board[`${c},${r}`]=u;else p.bench[i-6]=u;setup.push({board,c,r,u:structuredClone(u)});}
  p.gold=n%8-2;p.items=n%7===0?Array(R.itemCap).fill(DT.CORE_ITEMS[0]):[];
  Object.assign(p.mech,{arcMarketsOpened:n%4,arcMarketPlus:n%2===0,arcSynLv:n%6,arcExtra:n%3,arcPendingAnom:false});
  const before={gold:p.gold,items:p.items.slice(),opened:p.mech.arcMarketsOpened,plus:p.mech.arcMarketPlus,lv:p.mech.arcSynLv,extra:p.mech.arcExtra};
  const offers=[];
  for(let i=0;i<3;i++){g.round=i+1;g.makeOffer(p);const made=structuredClone(p.offer),reroll=g.rerollOffer(p),rolled=structuredClone(p.offer),chosen=g.chooseOffer(p,n%3);offers.push({made,reroll,rolled,chosen,after:structuredClone({gold:p.gold,items:p.items,mech:p.mech,units:p.allUnits()})});}
  steps.push({setup,before,offers});
 }
 traces.push({variant:`arcane/${pace}`,human,seed,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/arcane-offers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Arcane offer traces.`);
