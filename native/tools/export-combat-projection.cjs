const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
class StartOnly extends DT.Game {startCarousel(){} makeOffer(){} planAI(){} fireGame(name,p,e){return name==='onRoundStart'?e:super.fireGame(name,p,e);}}
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(let n=0;n<32;n++){
 const seed=n*31+17,g=new StartOnly({setId:set.id,pace,seed,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});
 g.stage=2+n%5;g.round=1;g.roundType=g.R.roundType(g.stage,g.round);
 const added=[];
 for(const p of g.players){
  p.augs=[...DT.AUGMENTS.slice((n+p.id)%4,(n+p.id)%4+2).map(a=>a.id),...(duo?['duo_crest_link','duo_sync']:[])];
  p.globals=Object.values(DT.ITEMS).filter(i=>i.cat==='global').slice(0,(n+p.id)%3).map(i=>i.id);
  if(DT.POWERS.length)p.power={id:DT.POWERS[(n+p.id)%DT.POWERS.length].id,rank:1+n%4};
  p.nextBuffs=n%2?{as:.1*p.id,armor:7*p.id,mana:3*p.id}:null;
  const k=n+p.id;
  p.mech={deva:k%6,blossom:k*2,goldStart:k*3,bank:k%16,ctKills:k%40,arcSynHp:k*5,arcChests:k,arcJunk:k%2?['bots','spare']:[],rcAP:k*2,ulDuel:k*3,ulBeastKills:k};
  if(k%3!==0){p.mech.arcSynAug=k%2===0;p.mech.hpFull=k%2!==0;}
  if(k%3===1)p.mech.fallen=k%2?'holy':'dark';
  if(k%4===1)p.mech.supreme='col_sup_eclipse';
  if(k%3!==0)p.mech.bounty={[DT.SHOP_LINES[k%DT.SHOP_LINES.length].id]:k%10};
  for(let j=0;j<k%7;j++){
   const line=DT.SHOP_LINES[(n+p.id+j)%DT.SHOP_LINES.length].id,u=g.newUnit(line,j%3);
   u.items=[DT.CORE_ITEMS[(k+j)%DT.CORE_ITEMS.length]].filter(Boolean);if(DT.EMBLEM_IDS.length&&j%2)u.items.push(DT.EMBLEM_IDS[(k+j)%DT.EMBLEM_IDS.length]);
   u.mods={partner:j%2===1,bonus:['hp','ad','ap','mana'][j%4],trait:DT.LINE[line].traits[0],glutHp:j*20,glutAp:j*2,arcAnom:j%2?'x_power':null,form:j%2?'critical':null,pu:j%3?'warp':null};u.bonusTraits={[DT.LINE[line].traits[0]]:j%2};
   p.board[`${j},${4+j%3}`]=u;added.push({player:p.id,col:j,row:4+j%3,unit:structuredClone(u)});
  }
 }
 const buffs=b=>b?{attackSpeed:b.as||0,armor:b.armor||0,mana:b.mana||0}:null;
 function effects(s){const m=s.mech||{};return {noTraits:false,counts:s.counts||null,augments:s.augs||[],parts:s.parts?s.parts.map(p=>{const e=effects(p);return {owner:p.owner,augments:e.augments,buffs:e.buffs,partnerTop:e.partnerTop,gizmos:e.gizmos,dragonlands:e.dragonlands,arcane:e.arcane,coliseum:e.coliseum,underlords:e.underlords,globals:e.globals,power:e.power};}):null,buffs:buffs(s.buffs),partnerTop:s.partnerTop||0,
  fates:set.id==='fates'?{deva:m.deva||0,fallen:m.fallen??null,blossom:m.blossom||0}:null,
  galaxies:set.id==='galaxies'?{bounty:m.bounty||{}}:null,gizmos:set.id==='gizmos'?{goldStart:m.goldStart||0}:null,
  dragonlands:set.id==='dragonlands'?{bank:m.bank||0,hpFull:!!m.hpFull,ctKills:m.ctKills||0}:null,
  arcane:set.id==='arcane'?{arcSynHp:m.arcSynHp||0,arcSynAug:!!m.arcSynAug,arcChests:m.arcChests||0,arcJunk:m.arcJunk||[]}:null,
  coliseum:set.id==='coliseum'?{rcAP:m.rcAP||0,supreme:m.supreme??null}:null,
  underlords:set.id.startsWith('underlords')?{ulDuel:m.ulDuel||0,ulBeastKills:m.ulBeastKills||0}:null,globals:s.globals||[],power:s.power||null};}
 function side(s){return {units:s.units.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner??null,items:u.items,bonusTraits:u.bonusTraits||null,partner:u.mods?.partner?{bonus:u.mods.bonus??null,trait:u.mods.trait??null}:null,gluttonHealth:u.mods?.glutHp||0,gluttonAbilityPower:u.mods?.glutAp||0,arcaneAnomaly:u.mods?.arcAnom??null,coliseum:u.mods?.form!=null||u.mods?.pu!=null?{form:u.mods.form??null,powerUp:u.mods.pu??null}:null})),effects:effects(s),columns:s.cols||7};}
 const a=side(duo?g.teamSide(g.teams[0]):g.sideOf(g.players[0])),b=side(duo?g.teamSide(g.teams[1]):g.sideOf(g.players[1]));
 traces.push({variant:`${set.id}/${pace}`,n,seed,duo,stage:g.stage,added,players:g.players.map(p=>({augs:p.augs,globals:p.globals,power:p.power,buffs:p.nextBuffs,mech:p.mech})),a,b,setup:{sideA:a.effects,sideB:b.effects,galaxy:g.mech.galaxy||null,mutation:g.mech.mutation||null,roundType:g.roundType,roundStage:g.stage,mirage:g.mech.mirage||null,pace,arcSponsor:g.mech.arcSponsor||[]},nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/combat-projection.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} combat projection traces.`);
