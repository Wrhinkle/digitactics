const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,log=[];
 const g=Object.create(DT.Game.prototype);
 const kinds=Object.fromEntries(Object.keys(DT.SET.mechanic.offerKinds).map(kind=>[kind,{options:()=>['custom_a','custom_b','custom_c']}]));
 if(set.id==='dragonlands')kinds.drg_treasure=DT.SET.mechanic.offerKinds.drg_treasure;
 if(set.id==='autochess')kinds.ac_loot=DT.SET.mechanic.offerKinds.ac_loot;
 Object.assign(g,{R,uid:0,rng:DT.makeRng(143),duo,phase:'result',stage:1,round:1,roundType:R.roundType(1,1),over:false,playOut:true,hasHook:()=>false,ensureSet(){},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]])),set:{...DT.SET,def:{...DT.SET.def,items:{...DT.SET.def.items,armory:null}},mechanic:{...DT.SET.mechanic,offerKinds:kinds}}});
 g.players=Array.from({length:8},(_,i)=>({id:i,isHuman:i===0,hp:R.startHp,alive:true,placement:0,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,streak:0,locked:i%2===0,freeRolls:3,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],augs:[],globals:[],mech:{},power:{id:'fixture_power',rank:1},offer:null,portal:4,incoming:[{kind:'fixture'}],boardUnits(){return Object.values(this.board);},allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 if(duo)g.setupTeams(null);
 for(const p of g.players)g.refreshShop(p);
 if(duo){g.teams[3].hp=0;for(const p of g.teams[3].members)p.hp=0;}else g.players[7].hp=0;
 g.eliminate();
 const snapshot=p=>({hp:p.hp,gold:p.gold,level:p.level,xp:p.xp,freeRolls:p.freeRolls,offer:p.offer,rank:p.power.rank,shop:p.shop});
 g.fireGame=(name,p,e)=>{
  if(name==='onIncome'){log.push({name,id:p.id,state:structuredClone(snapshot(p))});e.bonus+=p.id%3;}
  if(name==='onRankUp')log.push({name,id:p.id,state:structuredClone(snapshot(p))});
  if(name==='onRoundStart'){log.push({name,id:p.id,state:structuredClone(snapshot(p))});p.freeRolls+=1;if(p.id===0)p.hp+=1;}
  return e;
 };
 g.startCarousel=()=>log.push({name:'carousel',phase:g.phase});
 g.planAI=()=>{log.push({name:'ai',phase:g.phase});g.makePairs();};
 const steps=[];
 for(let i=0;i<70;i++){
  const start=log.length;g.nextRound();
  steps.push({stage:g.stage,round:g.round,type:g.roundType,phase:g.phase,log:log.slice(start),players:structuredClone(g.players.map(snapshot)),pairs:(duo?g.duoPairs||[]:g.pairs||[]).map(([a,b,ghost])=>({a:a.id,b:b.id,ghost}))});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/round-flow.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} round flow traces.`);
