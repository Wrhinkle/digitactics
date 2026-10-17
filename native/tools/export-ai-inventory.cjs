const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),random=DT.makeRng(1847),pick=a=>a[Math.floor(random()*a.length)];
 Object.assign(g,{R,set:DT.SET,rng:DT.makeRng(143),phase:'planning',mech:{},hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:false,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));}};g.refreshShop(p);delete g.hasHook;
 const lines=Object.values(DT.LINE).filter(l=>l.ai),items=Object.keys(DT.ITEMS),held=items.filter(id=>!DT.ITEMS[id].consumable&&DT.ITEMS[id].cat!=='global'),cases=[];
 for(let n=0;n<1200;n++){
  p.board={};p.bench=Array(9).fill(null);p.globals=[];const setup=[];g.stage=n%7+1;
  for(let i=0;i<n%17+n%4;i++){
   const board=i<n%17,index=board?i:i-n%17;
   const u={uid:i+1,line:pick(lines).id,stage:Math.floor(random()*3),items:Array.from({length:i%4},()=>pick(held)),noItems:n%19===0||i%11===0};
   if(board)p.board[`${i%7},${4+Math.floor(i/7)}`]=u;else p.bench[index]=u;
   setup.push({board,index,unit:structuredClone(u)});
  }
  const specials=DT.SPECIAL_COMPONENTS||[],components=DT.STAT_COMPONENTS.length?DT.STAT_COMPONENTS:held;
  p.items=Array.from({length:n%(R.itemCap+1)},(_,i)=>n%4===0?pick(components):n%4===1?pick(items):n%4===2&&specials.length?i<4?specials[n%specials.length]:pick(components):pick(held));
  const inventory=p.items.slice(),recipeId=n%23===0?'missing_item':items[n%items.length],recipe=g.recipeFrom(p.items,recipeId);
  const hasEquipRules=g.hasHook('canEquip',p);g.aiItems(p);
  const after=structuredClone({items:p.items,globals:p.globals,units:p.allUnits()});
  const craftId=items[(n*7)%items.length],crafted=g.craft(p,craftId),afterCraft=p.items.slice();
  const giveId=p.items[0]||'missing_item',giveUid=p.allUnits()[0]?.uid??null,given=giveUid!==null?g.giveFromBench(p,giveId,p.allUnits()[0]):false;
  cases.push({stage:g.stage,setup,inventory,recipeId,recipe,hasEquipRules,after,craftId,crafted,afterCraft,giveId,giveUid,given,final:structuredClone({items:p.items,globals:p.globals,units:p.allUnits()}),nextRandom:g.rng()});
 }
 traces.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-inventory.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} AI inventory traces.`);
