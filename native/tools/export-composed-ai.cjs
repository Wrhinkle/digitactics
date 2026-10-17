const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
class StartOnly extends DT.Game {
 startCarousel(){} makeOffer(){} planAI(){}
 fireGame(name,p,e){return name==='onRoundStart'?e:super.fireGame(name,p,e);}
}
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const seed of [0,17,143]){
 const g=new StartOnly({setId:set.id,pace,seed,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});
 g.makeOffer=DT.Game.prototype.makeOffer;const R=g.R={...g.R,offers:[{kind:'armory',stage:1,round:1}]},items=Object.keys(DT.ITEMS).filter(id=>!DT.ITEMS[id].consumable),consumables=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].consumable),steps=[];
 for(const p of g.players)p.augs=['duo_proud','duo_wide_portal',...(set.id==='gizmos'?['giz_a_highend','giz_a_ticket','giz_a_recruit']:[])];
 for(let n=0;n<18;n++){
  g.stage=1+Math.floor(n/4);g.round=1+n%4;g.roundType=n%7===0?'carousel':'pvp';g.phase=n%9===0?'combat':'planning';
  const kind=n===0&&DT.POWERS.length?'power':n%5===1?'augment':'armory';R.offers[0]={kind,stage:g.stage,round:g.round};
  const inputs=[];
  for(const p of g.players){
   const grant=[items[(n+p.id)%items.length],items[(n*7+p.id)%items.length],consumables[(n+p.id)%consumables.length]].filter(Boolean);
   p.gold+=n%4===0?30:10;p.freeRolls=n%3;p.portal=0;p.incoming=[];grant.forEach(id=>g.giveItem(p,id));
   if(DT.WISPS.length){const w=DT.WISPS[(n+p.id)%DT.WISPS.length];p.wisp={id:w.id,cost:DT.wispCost(w,g.stage)};}
   g.makeOffer(p);inputs.push(grant);
  }
  for(const p of g.players)if(p.alive&&!p.isHuman)g.aiTurn(p);
  steps.push({stage:g.stage,round:g.round,roundType:g.roundType,phase:g.phase,kind,inputs,after:structuredClone({pool:g.pool,players:g.players.map(p=>({gold:p.gold,hp:p.hp,level:p.level,xp:p.xp,freeRolls:p.freeRolls,teamBonus:p.teamBonus,power:p.power,augs:p.augs,items:p.items,globals:p.globals,shop:p.shop,board:p.board,bench:p.bench,portal:p.portal,cores:p.mech.cores||0,incoming:p.incoming,offer:p.offer||null,wisp:p.wisp,buffs:p.nextBuffs}))})});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,seed,steps,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/composed-ai.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} composed AI traces.`);
