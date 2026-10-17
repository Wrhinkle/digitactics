const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
const copiers={gizmos:'giz_copy',galaxies:'gal_copier',fates:'fat_copy',dragonlands:'drg_dup',arcane:'arc_copier',autochess:'ac_digiegg'};
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true])for(const route of ['direct','offer','loot','shop','portal','copy','carousel','settlement'])for(let n=0;n<16;n++){
 if(route==='portal'&&!duo||route==='copy'&&!copiers[set.id])continue;
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=31+n*71,human=route==='carousel'||n%2===0;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:route==='settlement'||n%7===0&&!['portal','carousel'].includes(route)?'combat':'planning',stage:2+n%4,round:1,roundType:'pvp',duo,D:DT.Duos.rules(R),mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.hasHook=(name,p)=>!['onShop','onShopRolled'].includes(name)&&DT.Game.prototype.hasHook.call(g,name,p);
 g.players=Array.from({length:8},(_,id)=>new DT.Player(id,`Player ${id}`,human,g));if(duo)g.setupTeams();g.players.forEach(p=>g.refreshShop(p));
 if(route==='carousel')g.startCarousel();
 const source=g.players[0],p=route==='portal'?g.partnerOf(source):route==='carousel'?g.carouselTurn():source;
 let line=set.id==='gizmos'?'giz_gekomon':set.id==='coliseum'?'col_agumon':DT.SHOP_LINES[n%DT.SHOP_LINES.length].id;
 if(route==='carousel')line=g.carousel.opts[0].line;
 if(!DT.LINE[line])line=DT.SHOP_LINES[0].id;
 const stage=['direct','offer','settlement'].includes(route)&&n%3===0?1:0,count=g.copiesOf(line),added=[];
 function add(who,j,s=stage){
  const u=g.newUnit(line,s);u.items=[DT.CORE_ITEMS[j%DT.CORE_ITEMS.length],DT.STAT_COMPONENTS[0]].filter(Boolean);
  u.mods=j===0?{other:7}:set.id==='arcane'?{arcAnom:'x_power',other:9}:set.id==='fates'?{partner:DT.LINE[line].traits[0],other:9}:set.id==='gizmos'?{glutHp:100*j,glutAp:5*j}:set.id==='coliseum'?{pu:j%2?'burst':'warp',form:'critical',other:9}:{other:9};
  if(set.id==='fates'&&j>0)u.bonusTraits={[DT.LINE[line].traits[0]]:1};
  const board=j===0&&n%4===0&&who===p,slot=board?'0,4':who.freeBenchSlot();if(board)who.board[slot]=u;else who.bench[slot]=u;
  added.push({player:who.id,board,slot,unit:structuredClone(u)});return u;
 }
 const amount=['direct','offer','settlement'].includes(route)?count:count-1;
 for(let j=0;j<amount;j++)add(p,j);
 if(stage===0&&n%3===1)for(let j=0;j<count-1;j++)add(p,amount+j,1);
 let send=null;if(route==='portal')send=add(source,amount,0).uid;
 for(const who of g.players){who.augs=['duo_proud'];who.gold=100;who.items=n%5===0?Array(R.itemCap).fill(DT.CORE_ITEMS[0]):[];}
 if(route==='copy')p.items.unshift(copiers[set.id]);
 const before=g.players.map(who=>({items:who.items.slice()}));
 if(route==='direct')g.combine(p);
 if(route==='settlement')for(const who of g.players)g.combine(who);
 if(route==='carousel')g.carouselPick(p,0);
 if(route==='offer'){p.offer={kind:'augment',options:['duo_proud'],rerolls:0};g.chooseOffer(p,0);}
 if(route==='loot'){g.takeFromPool(line);p.orbs.push({id:-1,kind:'blue',x:0,z:0,gold:0,items:[],unit:line});g.collectOrb(p,-1);}
 if(route==='shop'){if(p.shop[0])g.pool[p.shop[0]]++;g.takeFromPool(line);p.shop[0]=line;p.shopMeta=null;g.buy(p,0);}
 if(route==='portal')g.portal(source,{uid:send});
 if(route==='copy')g.useConsumable(p,0,p.allUnits()[0]);
 const snap=who=>({id:who.id,gold:who.gold,items:who.items,board:who.board,bench:who.bench,shop:who.shop,proudRound:who.duo?.proudRound??null,proudCount:who.duo?.proudN||0,cores:who.mech.cores||0});
 traces.push({variant:`${set.id}/${pace}`,duo,route,n,seed,human,phase:g.phase,stage:g.stage,line,send,added,before,after:structuredClone(g.players.map(snap)),pool:structuredClone(g.pool),nextId:g.newUnit(line,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/unit-lifecycle.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} unit lifecycle traces.`);
