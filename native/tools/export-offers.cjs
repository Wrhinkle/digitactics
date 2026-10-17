const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,picks=[];
 const g=Object.create(DT.Game.prototype);
 const kinds=Object.fromEntries(Object.keys(DT.SET.mechanic.offerKinds).map(kind=>[kind,{options:(c,exclude)=>['custom_a','custom_b','custom_c'].filter(id=>!exclude.includes(id)),choose:(c,id)=>picks.push(`${kind}:${c.player.id}:${id}`)}]));
 if(set.id==='reckoning')for(const k of ['rec_bless','rec_radiant','rec_tome'])kinds[k]=DT.SET.mechanic.offerKinds[k];
 if(set.id==='coliseum')for(const k of ['col_core','col_form','col_gambit'])kinds[k]=DT.SET.mechanic.offerKinds[k];
 if(set.id==='dragonlands')kinds.drg_treasure=DT.SET.mechanic.offerKinds.drg_treasure;
 if(set.id==='autochess')for(const k of ['ac_loot','ac_relic'])kinds[k]=DT.SET.mechanic.offerKinds[k];
 if(set.id==='gizmos')kinds.upgrade=DT.SET.mechanic.offerKinds.upgrade;
 Object.assign(g,{R,mech:{},uid:0,rng:DT.makeRng(143),duo,phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]])),set:{...DT.SET,def:{...DT.SET.def,items:{...DT.SET.def.items,armory:null}},mechanic:{...DT.SET.mechanic,offerKinds:kinds}}});
 g.players=Array.from({length:8},(_,i)=>({id:i,isHuman:i===0,gold:R.startGold??3,freeRolls:0,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],augs:[],globals:[],mech:{},power:null,offer:null,boardUnits(){return Object.values(this.board);},allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of g.players)g.refreshShop(p);
 g.applyAugment=(p,id)=>{p.augs.push(id);picks.push(`augment:${p.id}:${id}`);g.combine(p);};
 g.applyPower=(p,id)=>{if(!DT.POWER[id])return;p.power={id,rank:1};picks.push(`power:${p.id}:${id}`);g.combine(p);};
 const snapshot=p=>structuredClone({offer:p.offer,items:p.items,globals:p.globals,augs:p.augs,power:p.power,loot:p.mech.loot??null,picks});
 const steps=[];
 for(let stage=1;stage<=12;stage++)for(let round=1;round<=R.roundsInStage(stage);round++)for(let index=0;index<8;index++){
  g.stage=stage;g.round=round;const p=g.players[index];p.mech.loot=DT.COMPONENT_IDS.slice(0,3);
  const at=g.offerAt(stage,round);g.makeOffer(p);const made=snapshot(p);
  const reroll=index%3===0?g.rerollOffer(p):null;const rolled=snapshot(p);
  const choice=index%3;const selected=g.chooseOffer(p,choice);
  const invalid=g.chooseOffer(p,-1);
  steps.push({stage,round,index,at,made,reroll,rolled,choice,selected,invalid,after:snapshot(p)});
 }
 traces.push({variant:`${set.id}/${pace}`,duo,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/offers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} offer traces.`);
