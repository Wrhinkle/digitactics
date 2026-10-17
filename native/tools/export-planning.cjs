const fs = require('node:fs');
const path = require('node:path');
const {load} = require('../../tools/headless.cjs');
const DT = load({strict:true});
const traces = [];
for (const set of DT.Sets.pickable()) for (const pace of ['normal','hyper']) {
 DT.Sets.activate(set.id, pace);
 const R = DT.HYPER, g = Object.create(DT.Game.prototype), inventory = [];
 Object.assign(g, {R, uid:0, phase:'planning', hasHook:()=>false, clearMaxedShop(){}, giveItem(p,id){inventory.push(id);}});
 const p = {board:{},bench:Array(9).fill(null),isHuman:true,allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);},boardCount(){return Object.values(this.board).reduce((n,u)=>n+DT.slotsOf(u),0);},cap(){return 5+Object.values(this.board).reduce((n,u)=>n+u.items.reduce((a,id)=>a+(DT.ITEMS[id]?.p?.teamSize||0),0),0);}};
 const rng = DT.makeRng(456); const lines = DT.SHOP_LINES.slice(0,5).map(l=>l.id);
 const component=Object.keys(DT.ITEMS).find(id=>DT.isComponent(id));
 const completed=Object.keys(DT.ITEMS).find(id=>!DT.isComponent(id));
 const egg=Object.keys(DT.ITEMS).find(id=>DT.ITEMS[id].p?.teamSize);
 const snapshots=[];
 const nativeSpot=s=>({isBoard:s.type==='board',column:s.c||0,row:s.r||0,index:s.i||0});
 const snap=()=>({board:Object.entries(p.board).map(([k,u])=>({spot:nativeSpot({type:'board',c:+k.split(',')[0],r:+k.split(',')[1]}),unit:u})),bench:p.bench,inventory:[...inventory]});
 for(let i=0;i<700;i++) {
  g.phase=i%19<5?'combat':'planning'; let command;
  if(i%4<2){
   const line=lines[Math.floor(rng()*lines.length)];
   const free=p.freeBenchSlot();
   const can=free>=0||(g.boardLocked(p)?p.bench.filter(Boolean):p.allUnits()).filter(u=>u.line===line&&u.stage===0).length>=g.copiesOf(line)-1;
   const held=[component,completed,i%23===0?egg:component].filter(Boolean);
   command={kind:'add',line,items:held,accepted:can};
   if(can){const u=g.newUnit(line,0);u.items=held;if(free>=0){p.bench[free]=u;g.combine(p);}else g.combine(p,u);}
  }else if(i%4===2){
   const from=rng()<.65?{type:'bench',i:Math.floor(rng()*9)}:{type:'board',c:Math.floor(rng()*7),r:4+Math.floor(rng()*4)};
   const to=rng()<.8?{type:'board',c:Math.floor(rng()*7),r:4+Math.floor(rng()*4)}:{type:'bench',i:Math.floor(rng()*9)};
   const result=g.move(p,from,to);command={kind:'move',from:nativeSpot(from),to:nativeSpot(to),result:result===true?'Moved':result==='cap'?'Capacity':'Invalid'};
  }else{
   const units=p.allUnits();const u=units[Math.floor(rng()*units.length)];const found=u&&g.findUnit(p,u.uid);let accepted=false;
   if(found&&!(found.where==='board'&&g.boardLocked(p))){if(found.where==='board')delete p.board[found.k];else p.bench[found.i]=null;accepted=true;}
   command={kind:'remove',id:u?.uid||-1,accepted};
  }
  snapshots.push({locked:g.boardLocked(p),command,state:structuredClone(snap())});
 }
 traces.push({variant:`${set.id}/${pace}`,snapshots});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/planning.json.gz'),require('node:zlib').gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} planning traces.`);
const shops=[];
for(const set of DT.Sets.pickable()) for(const pace of ['normal','hyper']) {
 DT.Sets.activate(set.id,pace); const R=DT.HYPER;
 const g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(143),phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]])),giveItem(){}});
 const players=Array.from({length:2},()=>({gold:500,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),freeRolls:0,isHuman:true,allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of players)g.refreshShop(p);
 const snap=()=>({pool:{...g.pool},players:players.map(p=>({gold:p.gold,shop:[...p.shop],bench:structuredClone(p.bench),freeRolls:p.freeRolls}))});
 const initial=snap(),steps=[];const random=DT.makeRng(14);
 for(let i=0;i<400;i++){
  const index=i%2,p=players[index];let command,result;
  if(i%7===0){p.freeRolls=1;command={kind:'freeRoll'};result=g.reroll(p);}
  else if(i%5===0){command={kind:'reroll'};result=g.reroll(p);}
  else if(i%4===0){const u=p.bench.filter(Boolean)[0];command={kind:'sell',id:u?.uid||-1};result=g.sell(p,command.id);}
  else {command={kind:'buy',slot:Math.floor(random()*p.shop.length)};result=g.canBuy(p,command.slot)==='ok';g.buy(p,command.slot);}
  steps.push({index,command,result,state:snap()});
 }
 shops.push({variant:`${set.id}/${pace}`,initial,steps});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/planning-shops.json.gz'),require('node:zlib').gzipSync(JSON.stringify(shops)));
console.log(`Exported ${shops.length} shared shop traces.`);
