const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),random=DT.makeRng(347),pick=a=>a[Math.floor(random()*a.length)];
 Object.assign(g,{R,set:DT.SET,rng:DT.makeRng(143),phase:'planning',mech:{},hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:false,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));}};g.refreshShop(p);delete g.hasHook;
 const lines=Object.values(DT.LINE).filter(l=>l.ai),items=Object.keys(DT.ITEMS),augs=Object.keys(DT.AUGMENT),powers=Object.keys(DT.POWER),cases=[];
 let selected;g.chooseOffer=(_p,i)=>{selected=i;};
 for(let n=0;n<900;n++){
  p.board={};p.bench=Array(9).fill(null);const setup=[];g.stage=n%9;
  for(let i=0;i<n%29+n%10;i++){
   const board=i<n%29,index=board?i:i-n%29;
   const u={uid:i+1,line:pick(lines).id,stage:Math.floor(random()*3),items:Array.from({length:i%4},()=>pick(items)),mods:{partner:n%4===0?{trait:'fixture'}:null},bonusTraits:{},noItems:i%13===0};
   if(board)p.board[`${i%7},${4+Math.floor(i/7)}`]=u;else p.bench[index]=u;setup.push({board,index,unit:structuredClone(u)});
  }
  p.hp=n%(R.startHp+1);p.streak=n%2?-1:1;p.items=Array.from({length:n%12},()=>pick(items));p.augs=Array.from({length:n%5},()=>pick(augs));
  const offers=[];for(const kind of ['armory','itemTier','augment','power']){
   const list=kind==='augment'?augs:kind==='power'?powers:items;
   const options=Array.from({length:n%7},()=>list.length?pick(list):'missing');p.offer={kind,options};g.aiOffer(p);offers.push({kind,options,selected,nextRandom:g.rng()});
  }
  cases.push({setup,stage:g.stage,hp:p.hp,won:n%2===0,items:p.items,augs:p.augs,offers});
 }
 traces.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-offers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} AI offer traces.`);
