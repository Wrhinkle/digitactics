const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('coliseum',pace);const R=DT.HYPER,lines=Object.values(DT.LINE),powers=DT.Sets.def('coliseum').powerUps,cases=[];
 for(let n=0;n<lines.length*36;n++){
  const line=lines[Math.floor(n/36)].id,seed=n%7,human=n%2===0,board=n%4===0,phase=['planning','combat','result'][Math.floor(n/4)%3];
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p={id:0,isHuman:human,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:['col_core','col_core'],globals:[],mech:{cores:n%3},augs:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);
  const u=g.newUnit(line,n%3),mods={other:7};if(n%2===0)mods.pu=powers[n%powers.length].id;u.mods=structuredClone(mods);
  if(board)p.board['0,4']=u;else p.bench[0]=u;
  const snap=()=>structuredClone({items:p.items,offer:p.offer||null,coreUid:p.mech.coreUid??null,cores:p.mech.cores,unit:g.findUnit(p,u.uid)?.u||null});
  const used=g.equipItem(p,0,u.uid),afterUse=snap(),busy=g.equipItem(p,0,u.uid),afterBusy=snap();
  const remove=n%7===0;if(remove){p.board={};p.bench=Array(9).fill(null);}
  const rolled=g.rerollOffer(p),afterRoll=snap(),rolledAgain=g.rerollOffer(p);
  const removeLate=n%13===0;if(removeLate){p.board={};p.bench=Array(9).fill(null);}
  const full=n%5===0;if(full)p.items=Array(R.itemCap).fill('col_core');
  const invalid=g.chooseOffer(p,-1),selected=g.chooseOffer(p,n%3),afterChoice=snap();
  const directInvalid=n%11===0;if(directInvalid)g.set.mechanic.offerKinds.col_core.choose(g.ctx(p),'missing_power');
  cases.push({seed,human,board,phase,line,stage:u.stage,mods,cores:n%3,used,afterUse,busy,afterBusy,remove,rolled,afterRoll,rolledAgain,removeLate,full,invalid,choice:n%3,selected,afterChoice,directInvalid,after:snap(),nextRandom:g.rng()});
 }
 traces.push({variant:`coliseum/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/coliseum-core.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Coliseum core traces.`);
