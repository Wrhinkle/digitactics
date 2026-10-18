const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const seed of [17,143]){
 const g=new DT.Game({setId:set.id,pace,seed,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});
 g.playOut=true;
 const snap=()=>structuredClone({over:g.over,stage:g.stage,round:g.round,type:g.roundType,phase:g.phase,pairs:(duo?(g.duoPairs||[]):g.pairs).map(m=>({a:m[0].id,b:m[1].id,ghost:!!m[2]})),teams:duo?g.teams.map(t=>({hp:t.hp})):[],pool:g.pool,players:g.players.map(p=>({alive:p.alive,placement:p.placement,orbs:p.orbs,gold:p.gold,hp:p.hp,level:p.level,xp:p.xp,streak:p.streak,freeRolls:p.freeRolls,teamBonus:p.teamBonus,power:p.power,augs:p.augs,items:p.items,globals:p.globals,shop:p.shop,board:p.board,bench:p.bench,portal:p.portal,cores:p.mech.cores||0,incoming:p.incoming,offer:p.offer||null,wisp:p.wisp,buffs:p.nextBuffs,goldStart:p.mech.goldStart||0,bank:p.mech.bank||0,hpFull:!!p.mech.hpFull})),carousel:g.carousel?{opts:g.carousel.opts,idx:g.carousel.idx,order:g.carousel.order.map(p=>p.id)}:null});
 const opening=snap(),steps=[];
 for(let n=0;n<100;n++){
  if(g.roundType!=='carousel'&&(seed===17||n%3!==0))g.aiTurn(g.players[0]);
  g.startCombat(); const started=snap();
  g.finishCombat();
  const battles=g.matches.map(m=>({a:m.a.id,b:m.b?.id??null,pve:!!m.pve,ghost:!!m.ghost,ta:m.ta??null,tb:m.tb??null,seed:m.seed,winner:m.combat.winner,time:m.combat.t,counts:m.combat.traitCounts,
   units:m.combat.units.map(u=>({line:u.line.id,uid:u.uid,owner:u.owner??null,team:u.team,stage:u.stage,alive:u.alive,summoned:!!u.summoned,hp:u.hp}))}));
  const finished=snap();if(!g.over)g.nextRound();steps.push({started,battles,finished,after:snap()});if(g.over)break;
 }
 if(!g.over)throw Error('Match did not end: '+set.id);
 traces.push({variant:`${set.id}/${pace}`,duo,seed,opening,steps,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/full-match.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} complete match traces.`);

