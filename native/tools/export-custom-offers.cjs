const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['arcane','reckoning','dragonlands'])for(const pace of ['normal','hyper'])for(const seed of [0,143,98765])for(const full of [false,true]){
 DT.Sets.activate(set,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(seed),phase:'planning',set:DT.SET,mech:{},hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:true,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],augs:[],globals:[],mech:{},boardUnits(){return Object.values(this.board);},allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};
 g.refreshShop(p);
 const boardLines=[DT.SHOP_LINES[0].id,(DT.SHOP_LINES.find(l=>l.mark)||DT.SHOP_LINES[2]).id];
 boardLines.forEach((line,i)=>{p.board[`${i},4`]=g.newUnit(line,0);g.takeFromPool(line);});
 if(full)for(let i=0;i<9;i++){p.bench[i]=g.newUnit(boardLines[0],2);g.takeFromPool(boardLines[0],DT.unitCopies(p.bench[i]));}
 const snap=()=>structuredClone({gold:p.gold,items:p.items,pool:g.pool,shop:p.shop,units:p.allUnits()});
 const steps=[];let previous=[];
 for(let i=0;i<120;i++){
  g.stage=1+i%10;g.round=1;g.phase=i%4===0?'combat':'planning';
  g.mech.treasure=['classic','order','chaos'][i%3];p.mech.treasureWon=i%4===0?undefined:i%4<3;
  p.augs=i%2?['drg_treasure_hoard']:[];
  const exclude=i%2?previous:[];
  const ctx=g.ctx(p),options=set==='dragonlands'?g.set.mechanic.offerKinds.drg_treasure.options(ctx,exclude):g.armoryOptions(p);
  const chosen=options[i%options.length]||null;
  if(chosen){if(set==='dragonlands')g.set.mechanic.offerKinds.drg_treasure.choose(ctx,chosen);else g.giveItem(p,chosen);}
  steps.push({stage:g.stage,locked:g.phase==='combat',version:g.mech.treasure,won:p.mech.treasureWon??null,hoard:!!p.augs.length,exclude,options,chosen,state:snap()});
  previous=options;
 }
 traces.push({variant:`${set}/${pace}`,seed,full,boardLines,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/custom-offers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} custom offer traces.`);
