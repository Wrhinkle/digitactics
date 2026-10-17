const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),rng=DT.makeRng(814);
 const items=Object.keys(DT.ITEMS),held=items.filter(id=>!DT.ITEMS[id].consumable&&DT.ITEMS[id].cat!=='global'),lines=Object.keys(DT.LINE);
 const pick=a=>a[Math.floor(rng()*a.length)];let deny=false;
 Object.assign(g,{R,set:DT.SET,hasHook:()=>true,phase:'planning',fireGame(name,p,e){if(set.id.startsWith('underlords')){if(e.unit.items.length>=1&&!e.combine){e.ok=false;e.reason='full';}}else if(deny){e.ok=false;e.reason='testVeto';}return e;},useConsumable(p,index,u){const id=p.items.splice(index,1)[0];return `used:${id}`;}});
 const cases=[];
 for(let i=0;i<1800;i++){
  const u={uid:1,line:pick(lines),stage:i%3,items:Array.from({length:i%4},()=>pick(held)),noItems:i%13===0};
  const p={id:0,isHuman:true,board:i%2===0?{'3,4':u}:{},bench:Array(9).fill(null),items:Array.from({length:3},()=>pick(items)),globals:[]};if(i%2)p.bench[0]=u;
  if(i%71===0)p.items[0]='missing_item';
  const before=structuredClone({unit:u,items:p.items,board:i%2===0});
  g.phase=i%3===0?'combat':'planning';deny=i%7===0;
  const index=i%19===0?-1:i%3;
  const result=g.equipItem(p,index,i%29===0?-1:1);
  const hold=g.canHold(p,u,p.items[0]||items[0]);
  const swapA=i%5-1,swapB=i%4;
  const swap=g.swapItems(p,swapA,swapB);
  cases.push({before,locked:g.phase==='combat',deny,index,uid:i%29===0?-1:1,result,hold,swapA,swapB,swap,after:{items:p.items,unitItems:u.items,globals:p.globals}});
 }
 const recipes=DT.COMPONENT_IDS.flatMap(a=>DT.COMPONENT_IDS.map(b=>({a,b,result:DT.combineItems(a,b)})));
 traces.push({variant:`${set.id}/${pace}`,recipes,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/planning-items.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} item command traces.`);
const radiant=[];
for(const pace of ['normal','hyper'])for(const seed of [0,143,98765]){
 DT.Sets.activate('reckoning',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const line=DT.SHOP_LINES[0].id,u=[1,2,3].map(uid=>({uid,line,stage:0,items:[]}));
 const p={id:0,isHuman:true,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{'0,4':u[0],'1,4':u[1]},bench:[u[2],...Array(8).fill(null)],shop:Array(R.shopSize||5).fill(null),items:[],boardUnits(){return Object.values(this.board);},allUnits(){return u;},mech:{},augs:[]};g.refreshShop(p);
 const base=Object.keys(DT.Sets.def('reckoning').items.radiantOf),steps=[];let previous=[];
 for(let i=0;i<180;i++){
  u.forEach((unit,j)=>unit.items=i%5===0?[]:[base[(i+j)%base.length],base[(i+2*j+1)%base.length]]);
  p.items=Array.from({length:i%4===0?R.itemCap:3},(_,j)=>base[(i+j+2)%base.length]);
  const before=structuredClone({items:p.items,held:u.map(x=>x.items)}),kind=['rec_bless','rec_radiant','rec_tome'][i%3],exclude=i%2?previous:[];
  const K=g.set.mechanic.offerKinds[kind],options=K.options(g.ctx(p),exclude),selected=options[i%options.length];K.choose(g.ctx(p),selected);
  steps.push({kind,exclude,before,options,selected,after:structuredClone({items:p.items,held:u.map(x=>x.items)})});previous=options;
 }
 radiant.push({variant:`reckoning/${pace}`,seed,line,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/radiant-offers.json.gz'),zlib.gzipSync(JSON.stringify(radiant)));
console.log(`Exported ${radiant.length} Radiant/Tome offer traces.`);
