const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
const supported=['giz_copy','gal_copier','fat_copy','drg_dup','drg_dup_lesser','arc_copier','arc_copier_small','ac_digiegg'];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,ids=supported.filter(id=>DT.ITEMS[id]);if(!ids.length)continue;
 if((R.maxStage??2)!==2)throw Error('Native copy stage limit requires migration');
 const lines=Object.values(DT.LINE),held=DT.CORE_ITEMS.slice(0,3),cases=[];
 for(const id of ids)for(let n=0;n<lines.length*18;n++){
  const target=lines[Math.floor(n/18)],scenario=n%9,stage=Math.floor(n/9)%2,locked=n%2===0,seed=n%3;
  const g=Object.create(DT.Game.prototype),events=[];
  Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:locked?'combat':'planning',hasHook:name=>name==='onCombine',fireGame(name,p,e){events.push({uid:e.unit.uid,from:e.from.map(u=>u.uid)});},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p={id:0,isHuman:true,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}};g.refreshShop(p);
  const setup=[];let board=0;
  const add=(line,st,onBoard=false)=>{const items=held.slice();setup.push({line,stage:st,board:onBoard,items});const u=g.newUnit(line,st);u.items=items;if(onBoard)p.board[`${board++},4`]=u;else p.bench[p.freeBenchSlot()]=u;return u;};
  const u=add(target.id,scenario===8?2:stage);
  const copyStage=id==='ac_digiegg'?u.stage:0;
  if([1,3,4,5].includes(scenario)){
   const need=g.copiesOf(target.id)-1-(u.stage===copyStage?1:0);
   for(let i=0;i<need;i++)add(target.id,copyStage,scenario===4);
  }
  if(scenario===5)for(let i=0;i<g.copiesOf(target.id)-1;i++)add(target.id,1);
  if([2,3,4,6].includes(scenario))while(p.freeBenchSlot()>=0)add(DT.SHOP_LINES[(p.freeBenchSlot()+7)%DT.SHOP_LINES.length].id,2);
  if(target.shop&&[6,7].includes(scenario))g.pool[target.id]=0;
  p.items=Array.from({length:n%4===0?R.itemCap:3},()=>held[0]);p.items[1]=id;
  const inventory=p.items.slice(),result=g.equipItem(p,1,u.uid);
  const entries=Object.entries(p.board).map(([spot,unit])=>({spot:`board:${spot}`,unit})).concat(p.bench.flatMap((unit,i)=>unit?[{spot:`bench:${i}`,unit}]:[]));
  cases.push({seed,setup,locked,emptyPool:target.shop&&[6,7].includes(scenario),inventory,result,after:{entries,items:p.items,pool:{...g.pool},shop:p.shop,events,nextId:g.newUnit(target.id,0).uid,nextRandom:g.rng()}});
 }
 traces.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/copy-consumables.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} copy consumable traces.`);
