const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const concrete of set.id==='coliseum'?[false,true]:[false]){
 DT.Sets.activate(set.id,pace);const R={...DT.HYPER,offers:[{kind:'armory',stage:1,round:1}]},g=Object.create(DT.Game.prototype),events=[];
 const realHas=g.hasHook,realFire=g.fireGame;
 Object.assign(g,{R,set:DT.SET,mech:{},uid:0,rng:DT.makeRng(143),phase:'planning',duo,maxTier:Math.max(...DT.SHOP_LINES.map(l=>l.tier)),pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.hasHook=function(name,p){return (concrete||name==='canEquip')&&realHas.call(this,name,p);};g.fireGame=function(name,p,e){if(name==='onAiTurn')events.push(p.id);if(concrete||name==='canEquip')return realFire.call(this,name,p,e);return e;};
 g.players=Array.from({length:8},(_,id)=>({id,isHuman:id===0,alive:true,hp:R.startHp,gold:R.startGold??3,freeRolls:0,teamBonus:0,portal:0,incoming:[],level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,streak:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);},benchCount(){return this.bench.filter(Boolean).length;},cap(){return this.level+this.boardUnits().reduce((s,u)=>s+u.items.reduce((s,id)=>s+(DT.ITEMS[id]?.p?.teamSize||0),0),0);}}));if(duo)g.setupTeams();g.players.forEach(p=>g.refreshShop(p));
 const items=Object.keys(DT.ITEMS).filter(id=>!DT.ITEMS[id].consumable),steps=[];
 for(let n=0;n<30;n++){
  g.stage=1+Math.floor(n/6);g.round=1+n%6;g.roundType=n%7===0?'carousel':'pvp';g.phase=n%9===0?'combat':'planning';R.offers[0]={kind:'armory',stage:g.stage,round:g.round};events.length=0;
  const inputs=[];for(const p of g.players){const grant=[items[(n+p.id)%items.length],items[(n*7+p.id)%items.length]];p.gold+=n%4===0?30:10;p.freeRolls=n%3;p.portal=0;p.incoming=[];grant.forEach(id=>g.giveItem(p,id));g.makeOffer(p);inputs.push(grant);}
  for(const p of g.players)if(p.alive&&!p.isHuman)g.aiTurn(p);
  steps.push({stage:g.stage,round:g.round,roundType:g.roundType,phase:g.phase,inputs,events:events.slice(),after:structuredClone({pool:g.pool,players:g.players.map(p=>({gold:p.gold,level:p.level,xp:p.xp,freeRolls:p.freeRolls,items:p.items,globals:p.globals,shop:p.shop,board:p.board,bench:p.bench,portal:p.portal,cores:p.mech.cores||0,incoming:p.incoming,offer:p.offer||null}))})});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,concrete,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-turns.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} AI turn traces.`);
