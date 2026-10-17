const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
const supported=new Set(['remover','reforger','giz_remover','giz_reforger','giz_ockit','gal_remover','gal_reforger','fat_remover','fat_reforger','drg_remover','drg_reforger','arc_remover','arc_reforger','col_remover','col_reforger','rec_remover']);
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const seed of [0,781,94531]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,consumables=Object.keys(DT.ITEMS).filter(id=>supported.has(id));
 if(!consumables.length)continue;
 if(set.id==='gizmos')for(const it of Object.values(DT.ITEMS))if(!!it.radiant!==(it.cat==='radiant'))throw Error('Radiant category contract changed');
 const g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const line=DT.SHOP_LINES[0].id,u={uid:1,line,stage:0,items:[]};
 const p={id:0,isHuman:true,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{'0,4':u},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],allUnits(){return [u];}};g.refreshShop(p);
 const held=Object.keys(DT.ITEMS).filter(id=>!DT.ITEMS[id].consumable&&DT.ITEMS[id].cat!=='global'),cases=[];
 for(const id of consumables)for(let i=0;i<held.length*4;i++){
  u.items=Array.from({length:i%4},(_,j)=>held[(Math.floor(i/4)+j)%held.length]);
  p.items=Array.from({length:i%5===0?R.itemCap:i%7===0?R.itemCap-1:3},()=>held[0]);
  const index=i%p.items.length;p.items[index]=id;
  const before=structuredClone({held:u.items,items:p.items}),result=g.useConsumable(p,index,u);
  cases.push({before,index,result,after:structuredClone({held:u.items,items:p.items}),nextRandom:g.rng()});
 }
 traces.push({variant:`${set.id}/${pace}`,seed,line,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/consumables.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} consumable traces from original item handlers.`);
