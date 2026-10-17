const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER;
 const picks=[...Object.keys(DT.AUGMENT).map(id=>({id,kind:set.id==='gizmos'?'upgrade':'augment'})),...Object.keys(DT.POWER).map(id=>({id,kind:'ul_tamer'}))];
 for(const {id,kind} of picks)for(let n=0;n<16;n++){
  const seed=31+n*71,human=n%2===0,g=Object.create(DT.Game.prototype);
  Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%3===0?'combat':'planning',stage:3,round:1,roundType:'pvp',duo,D:DT.Duos.rules(R),mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  g.hasHook=(name,p)=>!['onShop','onShopRolled'].includes(name)&&DT.Game.prototype.hasHook.call(g,name,p);
  g.players=Array.from({length:8},(_,i)=>new DT.Player(i,`Player ${i}`,human,g));if(duo)g.setupTeams();g.players.forEach(p=>g.refreshShop(p));
  const p=g.players[0],added=[];
  for(let j=0;j<n%14;j++){
   const line=set.id==='gizmos'&&j<3?'giz_gekomon':DT.SHOP_LINES[(n+Math.floor(j/3))%DT.SHOP_LINES.length].id,u=g.newUnit(line,0);
   u.items=[DT.CORE_ITEMS[j%DT.CORE_ITEMS.length],DT.STAT_COMPONENTS[0]].filter(Boolean);
   u.mods=j===0?{other:7}:set.id==='arcane'?{arcAnom:'x_power',other:9}:set.id==='fates'?{partner:DT.LINE[line].traits[0],other:9}:set.id==='gizmos'?{glutHp:100*j,glutAp:5*j}:set.id==='coliseum'?{pu:j%2?'burst':'warp',form:'critical',other:9}:{other:9};
   if(set.id==='fates'&&j>0)u.bonusTraits={[DT.LINE[line].traits[0]]:1};
   const board=j<4,slot=board?`${j},4`:p.freeBenchSlot();if(board)p.board[slot]=u;else p.bench[slot]=u;added.push({player:0,board,col:j,unit:structuredClone(u)});
  }
  for(const who of g.players){who.augs=who===p?Object.keys(DT.AUGMENT).filter(x=>x!==id).slice(0,n%3):['duo_proud'];who.gold=17;who.items=n%4===0?Array(R.itemCap).fill(DT.CORE_ITEMS[0]):n%4===1?Array(R.itemCap-1).fill(DT.CORE_ITEMS[0]):[];}
  if(n%5===0)for(const line of DT.SHOP_LINES)g.pool[line.id]=0;
  const before=g.players.map(who=>({items:who.items.slice(),augs:who.augs.slice()})),poolBefore={...g.pool};
  p.offer={kind,options:[id],rerolls:0};const result=g.chooseOffer(p,0);
  const snap=who=>({id:who.id,gold:who.gold,hp:who.hp,rolls:who.freeRolls,teamBonus:who.teamBonus,power:who.power,items:who.items,board:who.board,bench:who.bench,shop:who.shop,proudRound:who.duo?.proudRound??null,proudCount:who.duo?.proudN||0,cores:who.mech.cores||0,shimmer:who.mech.arcShimmer||0,markets:who.mech.arcMarkets||0,syn:!!who.mech.arcSynAug,marketPlus:!!who.mech.arcMarketPlus,extra:who.mech.arcExtra||0,pending:!!who.mech.arcPendingAnom,early:!!who.mech.blessEarly});
  traces.push({variant:`${set.id}/${pace}`,duo,kind,id,result,n,seed,human,phase:g.phase,added,before,poolBefore,after:structuredClone(g.players.map(snap)),pool:structuredClone(g.pool),nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/offer-effects.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} composed offer effect traces.`);
