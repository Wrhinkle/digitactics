const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const seed of [17,143]){
 const g=new DT.Game({setId:set.id,pace,seed,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});
 g.playOut=true;
 const snap=()=>structuredClone({over:g.over,stage:g.stage,round:g.round,type:g.roundType,phase:g.phase,pairs:(duo?(g.duoPairs||[]):g.pairs).map(m=>({a:m[0].id,b:m[1].id,ghost:!!m[2]})),teams:duo?g.teams.map(t=>({hp:t.hp})):[],pool:g.pool,players:g.players.map(p=>({name:p.name,human:p.isHuman,locked:p.locked,alive:p.alive,placement:p.placement,orbs:p.orbs,gold:p.gold,hp:p.hp,level:p.level,xp:p.xp,streak:p.streak,freeRolls:p.freeRolls,teamBonus:p.teamBonus,power:p.power,augs:p.augs,items:p.items,globals:p.globals,shop:p.shop,board:p.board,bench:p.bench,portal:p.portal,cores:p.mech.cores||0,incoming:p.incoming,offer:p.offer||null,wisp:p.wisp,buffs:p.nextBuffs,goldStart:p.mech.goldStart||0,bank:p.mech.bank||0,hpFull:!!p.mech.hpFull})),carousel:g.carousel?{opts:g.carousel.opts,idx:g.carousel.idx,order:g.carousel.order.map(p=>p.id)}:null});
 const opening=snap(),steps=[];
 g.players[0].gold+=100;g.players[0].items.push(...DT.STAT_COMPONENTS.slice(0,4));
 const unit=u=>({id:u.uid,line:u.line,stage:u.stage,items:u.items,bonusTraits:u.bonusTraits??null,mods:u.mods??null,noSell:!!u.noSell,noItems:!!u.noItems});
 function act(player,action,args={}){
  let result=g.applyAction(g.players[player],action,args);if(action==='buy'&&Array.isArray(result))result=result.map(unit);
  if(action==='portal'&&result&&typeof result==='object')result={kind:result.kind,evolved:result.evolved.map(unit)};
  steps.push({op:'action',player,action,args,result:structuredClone(result??null),after:snap()});
 }
 function phase(op,fn){fn();steps.push({op,after:snap()});}
 for(let n=0;n<12;n++){
  act(0,'claim',{name:'Native Player '+n+' long name'});
  if(n%4===0){act(0,'release',{name:'Bot'});act(0,'claim',{name:'Human'});}
  act(0,'offerReroll');act(0,'offer',{i:0});act(0,'carousel');
  for(let slot=0;slot<3;slot++)act(0,'buy',{slot});
  act(0,'reroll');act(0,'buy',{slot:1});act(0,'buyXp');
  for(let i=0;i<3;i++)act(0,'move',{from:{type:'bench',i},to:{type:'board',c:i+2,r:4}});
  let p=g.players[0],u=Object.values(p.board)[0];
  if(u){act(0,'equip',{idx:0,uid:u.uid});act(0,'equip',{idx:0,uid:u.uid});}
  act(0,'swapItems',{a:0,b:1});
  if(p.bench.some(Boolean))act(0,'portal',{uid:p.bench.find(Boolean).uid});else act(0,'portal',{item:0});
  act(0,'wisp');act(0,'lock');act(0,'unknown');
  for(const slot of [-1,0.5,'0',null,2147483648])act(0,'buy',{slot});
  act(0,'move',{from:{type:'bench',i:0},to:{type:'board',c:7,r:4}});act(0,'sell',{uid:-1});
  phase('start',()=>g.startCombat());
  act(0,'offer',{i:0});act(0,'offerReroll');act(0,'buyXp');
  u=Object.values(p.board)[0];if(u){act(0,'move',{from:{type:'board',c:Number(Object.keys(p.board)[0].split(',')[0]),r:Number(Object.keys(p.board)[0].split(',')[1])},to:{type:'bench',i:0}});act(0,'equip',{idx:0,uid:u.uid});act(0,'sell',{uid:u.uid});}
  act(0,'buy',{slot:2});act(0,'portal',{item:0});
  phase('finish',()=>g.finishCombat());
  if(p.orbs.length)act(0,'orb',{id:p.orbs[0].id});
  act(0,'offer',{i:0});act(0,'buyXp');act(0,'reroll');
  if(p.bench.some(Boolean))act(0,'sell',{uid:p.bench.find(Boolean).uid});
  const dead=g.players.find(p=>!p.alive);if(dead){act(dead.id,'buy',{slot:0});act(dead.id,'claim',{name:'Spectator'});act(dead.id,'release');}
  if(g.over)break;
  phase('advance',()=>g.nextRound());
 }
 traces.push({variant:`${set.id}/${pace}`,duo,seed,opening,steps,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-actions.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} player-action traces.`);

