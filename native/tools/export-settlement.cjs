const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true});const traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const scenario of ['pvp','ghost','pve'])for(const stage of [1,4,8]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,log=[];
 const g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(143),duo,stage,phase:'combat',roundType:scenario==='pve'?'pve':'pvp',over:false,playOut:true,hasHook:()=>false,ensureSet(){},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:8},(_,i)=>({id:i,name:`Player ${i}`,isHuman:i===0||i===3,hp:R.startHp,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],streak:0,alive:true,placement:0,lastOpponent:null,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],augs:i===2?['duo_guard']:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 if(duo)g.setupTeams(null);
 for(const p of g.players)g.refreshShop(p);
 if(scenario==='ghost'){if(duo){g.teams[3].hp=0;for(const p of g.teams[3].members)p.hp=0;}else g.players[7].hp=0;g.eliminate();}
 const line=DT.SHOP_LINES[0];
 for(const p of g.players.filter(p=>p.alive))for(let j=0;j<g.copiesOf(line.id);j++){
  const u=g.newUnit(line.id,0);if(j===0)p.board['3,4']=u;else p.bench[j-1]=u;g.takeFromPool(line.id);
 }
 const hookState=(kind,p,extra={})=>({kind,id:p.id,hp:p.hp,gold:p.gold,streak:p.streak,...extra});
 g.pveReward=(p,won)=>{log.push(hookState('reward',p,{won}));p.gold+=2;return{gold:2,unit:null,items:[],orbs:1};};
 g.roundEnd=(p,counts,info)=>{log.push(hookState('roundEnd',p,{won:info.won,pve:info.pve,team:info.team,owner:info.owner??null,units:info.match.combat.units.map(u=>u.uid),counts}));p.hp+=info.won?1:-1;const gold=(p.id%3)+1;p.gold+=gold;return gold;};
 const combine=DT.Game.prototype.combine;
 g.combine=function(p,extra){log.push(hookState('after',p));return combine.call(this,p,extra);};
 const knockOut=DT.Game.prototype.knockOut;
 g.knockOut=function(p,place){knockOut.call(this,p,place);log.push(hookState('eliminated',p,{placement:place}));};
 const configs=scenario==='pve'?g.alivePlayers().map(p=>({a:p.id,b:null,pve:true,ghost:false})):duo?
  (scenario==='ghost'?[{a:0,b:2,ta:0,tb:1},{a:4,b:0,ta:2,tb:0,ghost:true}]:[{a:0,b:2,ta:0,tb:1},{a:4,b:6,ta:2,tb:3}]):
  (scenario==='ghost'?[{a:0,b:1},{a:2,b:3},{a:4,b:5},{a:6,b:0,ghost:true}]:[{a:0,b:1},{a:2,b:3},{a:4,b:5},{a:6,b:7}]);
 const inputs=configs.map((m,index)=>{
  const units=Array.from({length:8},(_,j)=>({line:line.id,stage:j%3,team:j%2,alive:j!==6,summoned:j===7,uid:100+j,owner:j<2?null:(j%2===0?m.a:m.b)+(j>=4?1:0)}));
  return {...m,pve:!!m.pve,ghost:!!m.ghost,winner:(index+stage)%3-1,units,traitsA:{test:2},traitsB:{test:3}};
 });
 g.matches=inputs.map(m=>({...m,a:g.players[m.a],b:m.b==null?null:g.players[m.b],combat:{done:true,winner:m.winner,units:m.units.map(u=>({...u,line:DT.LINE[u.line]})),traitCounts:[m.traitsA,m.traitsB]}}));
 const results=g.finishCombat().map(r=>({damageA:r.dmgA,damageB:r.dmgB,teamDamageA:r.teamDmgA||0,bonusA:r.prankA||0,bonusB:r.prankB||0,bonusA2:r.prankA2||0,bonusB2:r.prankB2||0,reward:r.reward}));
 const state={over:g.over,pool:g.pool,players:g.players.map(p=>({hp:p.hp,gold:p.gold,streak:p.streak,alive:p.alive,placement:p.placement,lastOpponent:p.lastOpponent?.id??null,units:p.allUnits().map(u=>({uid:u.uid,line:u.line,stage:u.stage})),shop:p.shop})),teams:duo?g.teams.map(t=>({hp:t.hp,alive:t.alive,placement:t.placement,lastOpponent:t.lastOpponent??null})):[]};
 traces.push({variant:`${set.id}/${pace}`,duo,scenario,stage,line:line.id,inputs,results,log,state});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} settlement traces.`);
const lootTraces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const full of [false,true]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(143),phase:'combat',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:2},(_,i)=>({id:i,isHuman:i===0,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of g.players)g.refreshShop(p);
 const line=DT.SHOP_LINES[0].id;
 if(full)for(const p of g.players)for(let j=0;j<9;j++){p.bench[j]=g.newUnit(line,2);g.takeFromPool(line,DT.unitCopies(p.bench[j]));}
 // Restore a known unit reserve to exercise both exhausted and available custom drops.
 g.pool[line]=3;
 const snapshot=()=>structuredClone({pool:g.pool,players:g.players.map(p=>({gold:p.gold,items:p.items,orbs:p.orbs,bench:p.bench,shop:p.shop}))});
 const steps=[];
 for(let i=0;i<36;i++){
  const index=i%2,p=g.players[index],stage=1+i%6,round=stage===1?R.roundsInStage(1):1,won=i%3!==0;
  g.stage=stage;g.round=round;
  const specs=i%5===0?[{kind:'gold',gold:1,items:[null,DT.STAT_COMPONENTS[0]||null],unit:line},{kind:'blue',gold:2,items:[],unit:line}]:null;
  g.set=specs?{pveReward:()=>structuredClone(specs)}:{};
  const reward=g.pveReward(p,won),before=snapshot();
  const collect=i%3===0;if(collect)g.collectAllOrbs(p);
  steps.push({index,stage,round,won,specs,reward,before,collect,after:snapshot()});
 }
 lootTraces.push({variant:`${set.id}/${pace}`,full,line,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/loot.json.gz'),zlib.gzipSync(JSON.stringify(lootTraces)));
console.log(`Exported ${lootTraces.length} generic/custom loot traces.`);
