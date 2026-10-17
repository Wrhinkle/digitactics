const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true]){
 DT.Sets.activate('autochess',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(319),phase:'planning',stage:1,round:1,duo:false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:0,freeRolls:0,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.players=[p];g.refreshShop(p);
 const relics=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].relic),steps=[];
 for(let n=0;n<512;n++){
  p.board={};p.bench=Array(9).fill(null);p.items=Array(n%7).fill('ac_digiegg');p.globals=relics.filter((_,i)=>((n*37)%2048)&(1<<i));if(n%17===0)p.globals.push(...p.globals);
  p.gold=n%121-10;p.freeRolls=n%3;g.stage=1+n%8;g.round=1+n%R.roundsInStage(g.stage);g.phase=n%3===0?'combat':'planning';
  g.pool=Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]));
  const added=[];for(let k=0;k<3;k++){
   const l=DT.SHOP_LINES[(n*7+k)%DT.SHOP_LINES.length],stage=(n+k)%3,count=n%5===0?1:g.copiesOf(l.id)-1;
   for(let j=0;j<count;j++){const u=g.newUnit(l.id,stage);if(k===0)p.board[`${j},4`]=u;else p.bench[p.freeBenchSlot()]=u;added.push({unit:structuredClone(u),board:k===0,col:j});}
   if(n%4===0)g.pool[l.id]=0;
  }
  const before=structuredClone({gold:p.gold,freeRolls:p.freeRolls,items:p.items,globals:p.globals,pool:g.pool});
  const income=g.fireGame('onIncome',p,{base:9,interest:3,streak:n%4,augment:2,bonus:1});
  const shop=g.fireGame('onShop',p,{level:n%13}).level,size=g.fireGame('onTeamSize',p,{size:n%10}).size;
  g.fireGame('onRoundStart',p,{});g.fireGame('onSell',p,{});
  const target=DT.AutoChess.eggTarget(g,p)?.uid??null;
  g.fireGame('onAiTurn',p,{});
  steps.push({stage:g.stage,round:g.round,phase:g.phase,added,before,income,shop,size,target,after:structuredClone({gold:p.gold,freeRolls:p.freeRolls,items:p.items,shop:p.shop,board:p.board,bench:p.bench,pool:g.pool})});
 }
 traces.push({variant:`autochess/${pace}`,human,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/autochess-planning.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Auto Chess planning traces.`);
