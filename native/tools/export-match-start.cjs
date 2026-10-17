const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
class StartOnly extends DT.Game {
 startCarousel(){} makeOffer(){} planAI(){}
 fireGame(name,p,e){return name==='onRoundStart'?e:super.fireGame(name,p,e);}
}
const seeds=[...Array.from({length:128},(_,i)=>i*7919),0x7fffffff,0x80000000,0xfffffffe,0xffffffff];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duos of [false,true])for(const seed of seeds){
 const g=new StartOnly({setId:set.id,pace,seed,format:duos?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0||i===3})),teams:duos?[[0,7],[1,6],[2,5],[3,4]]:undefined});
 traces.push({variant:`${set.id}/${pace}`,seed,duos,mechanics:structuredClone(g.mech),players:g.players.map(p=>({id:p.id,name:p.name,human:p.isHuman,team:p.team??null,hp:p.hp,gold:p.gold,level:p.level,xp:p.xp,items:p.items,shop:p.shop,meta:p.shopMeta,bench:p.bench})),teams:duos?g.teams.map(t=>({id:t.id,hp:t.hp,members:t.members.map(p=>p.id)})):[],pool:g.pool,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-start.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
for(const key of ['galaxy','mutation','mirage','treasure','arcPortal'])console.log(key,[...new Set(traces.map(t=>t.mechanics[key]).filter(Boolean))]);
console.log(`Exported ${traces.length} native match-start references.`);
