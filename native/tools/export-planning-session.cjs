const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const seed of [0,17,143]){
 const g=new DT.Game({setId:set.id,pace,seed,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});
 const snap=()=>structuredClone({stage:g.stage,round:g.round,type:g.roundType,phase:g.phase,pairs:(duo?(g.duoPairs||[]):g.pairs).map(m=>({a:m[0].id,b:m[1].id,ghost:!!m[2]})),teams:duo?g.teams.map(t=>({hp:t.hp})):[],pool:g.pool,players:g.players.map(p=>({gold:p.gold,hp:p.hp,level:p.level,xp:p.xp,streak:p.streak,freeRolls:p.freeRolls,teamBonus:p.teamBonus,power:p.power,augs:p.augs,items:p.items,globals:p.globals,shop:p.shop,board:p.board,bench:p.bench,portal:p.portal,cores:p.mech.cores||0,incoming:p.incoming,offer:p.offer||null,wisp:p.wisp,buffs:p.nextBuffs,goldStart:p.mech.goldStart||0,bank:p.mech.bank||0,hpFull:!!p.mech.hpFull})),carousel:g.carousel?{opts:g.carousel.opts,idx:g.carousel.idx,order:g.carousel.order.map(p=>p.id)}:null});
 const opening=snap(),steps=[];
 for(const p of g.players){const a=['duo_proud','duo_care','duo_interest'];if(set.id==='gizmos')a.splice(p.id%2?0:3,0,'giz_a_reserves');p.augs.push(...a);}
 for(let n=0;n<24;n++){
  const p=g.players[0],choice=p.offer?n%p.offer.options.length:null;if(choice!==null)g.chooseOffer(p,choice);
  g.finishCarousel();g.nextRound();steps.push({choice,after:snap()});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,seed,opening,steps,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/planning-session.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} composed planning sessions.`);

