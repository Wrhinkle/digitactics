const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true});const traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const seed of [0,143,4294967295]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER;
 const g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(seed),duo,phase:'planning',roundType:'pvp',over:false,playOut:seed!==143,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:8},(_,i)=>({id:i,name:`Player ${i}`,isHuman:seed===0?i%3===0:i===0,hp:R.startHp,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],alive:true,placement:0,lastOpponent:null,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],augs:i===2?['duo_guard']:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));}}));
 const teams=seed===143?[[7,2],[5,0],[3,6],[1,4]]:seed===0?[[0,0],[1,2],[3,4],[5,6]]:null;
 if(duo)g.setupTeams(teams);
 for(const p of g.players)g.refreshShop(p);
 const line=DT.SHOP_LINES[0].id;
 for(const p of g.players){const u=g.newUnit(line,p.id%3);p.bench[0]=u;g.takeFromPool(line,DT.unitCopies(u));p.orbs=[{unit:line}];g.takeFromPool(line);p.items=[DT.CORE_ITEMS[0] || "test_inventory"];}
 const snapshot=()=>({over:g.over,pool:{...g.pool},players:g.players.map(p=>({hp:p.hp,alive:p.alive,placement:p.placement,lastOpponent:p.lastOpponent?.id??null,team:p.team??null,units:p.allUnits().map(u=>u.uid),shop:p.shop,orbs:p.orbs.length,items:p.items.length})),teams:duo?g.teams.map(t=>({hp:t.hp,alive:t.alive,placement:t.placement,lastOpponent:t.lastOpponent??null,members:t.members.map(p=>p.id)})):[]});
 const initial=structuredClone(snapshot()),steps=[];
 for(let i=0;i<70&&!g.over;i++){
  g.roundType=i%9===0?'pve':'pvp';g.makePairs();
  const pairs=(duo?g.duoPairs:g.pairs).map(([a,b,ghost])=>({a:a.id,b:b.id,ghost}));
  for(const[a,b,ghost]of duo?g.duoPairs:g.pairs){a.lastOpponent=duo?b.id:b;if(!ghost)b.lastOpponent=duo?a.id:a;}
  // Health-changing game hooks apply to individual players before Duos synchronization.
  const changes=g.players.map(p=>p.alive?(i%7===0?1:0)-(p.id%3===i%3?2:0):0);
  g.players.forEach((p,j)=>p.hp+=changes[j]);if(duo)g.syncTeams();
  const damage=duo?g.teams.map(t=>t.alive?2+(i+t.id)%5:0):[];
  const actual=duo?damage.map((d,j)=>g.damageTeam(g.teams[j],d)):[];
  g.eliminate();steps.push({type:g.roundType,pairs,changes,damage,actual,state:structuredClone(snapshot())});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,seed,teams,line,initial,steps});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-state.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} match state traces.`);
const losses=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER;
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,D:DT.Duos.rules(R)});
 const cases=[];
 for(const line of Object.values(DT.LINE))for(let stars=0;stars<3;stars++)for(const stage of [1,2,3,4,5,6,7,8,9,12,100,1000]){
  g.stage=stage;const units=[{line,stage:stars,team:1,alive:true,summoned:false},{line,stage:stars,team:1,alive:false,summoned:false},{line,stage:stars,team:1,alive:true,summoned:true},{line,stage:stars,team:0,alive:true,summoned:false}];
  cases.push({line:line.id,stars,stage,solo:g.lossFor({units},1,false),pve:g.lossFor({units},1,true),duo:g.duoLoss({units},1),negative:g.lossFor({units},-1,false)});
 }
 losses.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-loss.json.gz'),zlib.gzipSync(JSON.stringify(losses)));
console.log(`Exported ${losses.length} loss rule traces.`);
