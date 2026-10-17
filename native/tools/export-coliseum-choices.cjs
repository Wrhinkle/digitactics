const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('coliseum',pace);const base=DT.HYPER,lines=Object.values(DT.LINE),cases=[];
 const amounts=[0,.5,1,24.5,25,44.5,45,79.5,80,100,25/1.3,45/1.3,80/1.3,250.75];
 for(const kind of ['col_form','col_gambit'])for(let n=0;n<lines.length*28;n++){
  const R={...base,offers:[{kind,stage:1,round:1,rerolls:1}]},seed=n%7,line=lines[Math.floor(n/28)].id;
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',stage:1,round:1,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p={id:0,isHuman:true,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);
  const u=g.newUnit(line,n%3);u.mods={pu:'warp',other:7};u.bonusTraits={fixture:2};p.bench[0]=u;
  const initial={formUid:n%7===0?999:u.uid,gem:amounts[n%amounts.length],gLv:n%5,gCount:n%4,dd:n%2===0,gReady:n%3===0};p.mech=structuredClone(initial);
  p.items=Array(n%3===0?R.itemCap:n%3===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);const inventory=p.items.slice();
  const snap=()=>structuredClone({gold:p.gold,items:p.items,offer:p.offer||null,mech:p.mech,unit:g.findUnit(p,u.uid)?.u||null});
  g.makeOffer(p);const made=snap(),rolled=g.rerollOffer(p),afterRoll=snap();
  const remove=n%11===0;if(remove)p.bench[0]=null;
  const choice=n%4-1,selected=g.chooseOffer(p,choice),afterChoice=snap();
  const direct=n%5===0?(kind==='col_form'?'missing_form':n%2?'double-invalid':'cash:999'):null;
  if(direct)g.set.mechanic.offerKinds[kind].choose(g.ctx(p),direct);
  cases.push({kind,seed,line,stage:u.stage,initial,inventory,made,rolled,afterRoll,remove,choice,selected,afterChoice,direct,after:snap(),nextRandom:g.rng()});
 }
 traces.push({variant:`coliseum/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/coliseum-choices.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Coliseum form/gambit traces.`);
