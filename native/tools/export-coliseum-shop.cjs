const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(const seed of [0,17]){
 DT.Sets.activate('coliseum',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:human,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:['col_a_arena'],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},boardUnits(){return Object.values(this.board);},freeBenchSlot(){return this.bench.indexOf(null);},boardCount(){return Object.values(this.board).reduce((n,u)=>n+DT.slotsOf(u),0);},cap(){return 100;}};g.refreshShop(p);
 p.gold+=50000;if(R.xp)g.addXp(p,10000);else p.level=R.levelByStage[Math.min(10,R.levelByStage.length-1)];
 const snap=()=>structuredClone({gold:p.gold,items:p.items,offer:p.offer||null,cores:p.mech.cores||0,formUid:p.mech.formUid??null,board:Object.entries(p.board).map(([spot,u])=>({spot,u})),bench:p.bench,pool:g.pool,shop:p.shop});
 const steps=[];
 for(let i=0;i<1200;i++){
  g.phase=i%17===0?'combat':i%19===0?'result':'planning';
  const entries=p.allUnits(),target=entries.length?entries[i%entries.length]:null;
  let edit=null;if(target&&i%3===0){edit={uid:target.uid,mods:i%4===0?{other:7}:{pu:['warp','burst','armor'][i%3],form:i%5===0?'critical':i%5===1?'ballista':'invalid_form',other:7},items:DT.CORE_ITEMS.slice(0,i%4),bonusTraits:{MEM:1}};Object.assign(target,structuredClone({mods:edit.mods,items:edit.items,bonusTraits:edit.bonusTraits}));}
  const fill=i%23===0;if(fill)p.items=Array(R.itemCap).fill(DT.CORE_ITEMS[0]);
  let kind,result,arg;
  if(i%6===0){kind='refresh';g.refreshShop(p);result=true;}
  else if(i%6===1||i%6===2){kind='buy';arg=(i+(i%6===2?1:0))%p.shop.length;result=g.buy(p,arg)!==null;}
  else if(i%6===3){kind='choose';arg=i%3;result=g.chooseOffer(p,arg);}
  else if(i%6===4){kind='move';const from=p.bench.findIndex(Boolean);arg={from,col:i%7,row:4+Math.floor(i/7)%4};result=from<0?'none':g.move(p,{type:'bench',i:from},{type:'board',c:arg.col,r:arg.row});}
  else{kind='sell';arg=target?.uid||-1;result=g.sell(p,arg);}
  steps.push({phase:g.phase,edit,fill,kind,arg,result,after:snap()});
 }
 traces.push({variant:`coliseum/${pace}`,human,seed,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/coliseum-shop.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Coliseum shop traces.`);
