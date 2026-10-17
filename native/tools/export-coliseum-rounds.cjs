const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(const seed of [0,17,314]){
 DT.Sets.activate('coliseum',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'result',stage:1,round:1,roundType:R.roundType(1,1),over:false,playOut:true,duo:false,ensureSet(){},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=g.players=Array.from({length:2},(_,id)=>({id,isHuman:id===0?human:true,hp:R.startHp,alive:true,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],xp:0,streak:0,locked:false,freeRolls:0,board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:['col_a_arena'],power:null,offer:null,boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of players)g.refreshShop(p);
 const p=players[0],crew=DT.SHOP_LINES.filter(l=>l.traits.includes('CREW')).slice(0,2),setup=[{line:'col_piedmon',stage:0,board:true},...crew.map(l=>({line:l.id,stage:2,board:true})),{line:'col_justimon',stage:0,board:true},{line:'col_labramon',stage:1,board:false},{line:'col_justimon',stage:0,board:false}];
 let col=0;for(const s of setup){const u=g.newUnit(s.line,s.stage);if(s.board)p.board[`${col++},4`]=u;else p.bench[p.freeBenchSlot()]=u;}
 p.mech.cores=3;p.streak=-1;
 g.startCarousel=()=>{};g.planAI=()=>g.makePairs();
 const snapshot=()=>structuredClone(players.map(p=>({gold:p.gold,hp:p.hp,level:p.level,xp:p.xp,rolls:p.freeRolls,shop:p.shop,offer:p.offer,items:p.items,units:p.allUnits(),cores:p.mech.cores||0,formUid:p.mech.formUid??null,gems:p.mech.gem||0,gLv:p.mech.gLv||0,gCount:p.mech.gCount||0,dd:!!p.mech.dd,ready:!!p.mech.gReady,pirateCount:p.mech.rcN||0,pirateLast:p.mech.rcLast||null,pirateAP:p.mech.rcAP||0,buffs:p.nextBuffs||null})));
 const steps=[];
 for(let i=0;i<150;i++){
  p.hp=i%2?8:25;
  const gem=[0,24,45,80][i%4];p.mech.gem=gem;p.mech.gLv=i%4;p.mech.gReady=i%3!==0;
  p.items=Array(i%4===0?R.itemCap:i%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);
  const reset=i%7===0;if(reset)for(const u of p.allUnits())if(DT.Sets.def('coliseum').forms[u.line]){u.mods={pu:'warp'};u.bonusTraits={};}
  const buffs=i%9===0;if(buffs)p.nextBuffs={as:.1,mana:5};
  g.nextRound();const after=snapshot();
  const choice=i%3;let chosen=null;if(p.offer?.kind.startsWith('col_'))chosen=g.chooseOffer(p,choice);
  steps.push({i,reset,buffs,stage:g.stage,round:g.round,after,choice,chosen,final:snapshot()});
 }
 traces.push({variant:`coliseum/${pace}`,human,seed,setup,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/coliseum-rounds.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Coliseum round-start traces.`);
