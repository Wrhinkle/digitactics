const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let battle=0;battle<12;battle++){
 DT.Sets.activate('arcane',pace);const R=DT.HYPER,seed=10100+battle;
 const side=team=>['HR','CON','CON','CON','ACA','SYN'].map((trait,i)=>{const choices=DT.SHOP_LINES.filter(l=>l.traits.includes(trait)),l=choices[(battle+i+team)%choices.length];return {line:l.id,stage:(battle+i+team)%3,c:i,r:4+i%2,uid:team*6+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:['arc_bounty_chip'],mods:{arcAnom:'x_power'}};});
 const a=side(0),b=side(1),cb=new DT.Combat({units:a},{units:b},{seed,info:{type:'pvp'}});let ticks=0;while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Unfinished battle');
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,pace,uid:0,mech:{},rng:DT.makeRng(143),phase:'combat',stage:3,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=[new DT.Player(0,'A',true,g),new DT.Player(1,'B',true,g)];g.players=players;for(const p of players)g.refreshShop(p);
 const cases=[];for(let n=0;n<240;n++){
  const team=n%2,p=players[team],view=n%3===1,first=n%4===0,missing=n%17===0,won=n%4<2,pve=n%5===0&&team===0;
  p.gold=40;p.streak=n%11-5;p.augs=n%3===0?[]:['arc_a_find'];p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);
  p.mech={arcAcaLv:n%5,arcSynLv:n%4,arcShimmer:n%150,arcMarkets:n%3,arcConquest:n%35,arcChests:n%8,arcFindK:n%8};g.mech.arcSponsor=n%7===0?[]:DT.CORE_ITEMS.slice(0,3);
  p.board={};p.bench=Array(9).fill(null);const added=[];for(let i=0;i<4;i++){const source=(team===0?a:b)[i],u={uid:source.uid+(n%11===0?100:0),line:source.line,stage:source.stage,items:n%3===0?['arc_tamer_cape','arc_tamer_crown','arc_tamer_shield']:['arc_tamer_shield','arc_tamer_cape','arc_tamer_crown']};if(i<3)p.board[`${i},4`]=u;else p.bench[0]=u;added.push({unit:u,board:i<3,col:i});}
  const counts={};for(const id of n%2?['SYN','CON','HR','ACA']:['ACA','HR','CON','SYN']){const t=DT.TRAITS[id];counts[id]=n%6===0?0:t.bp[Math.floor(n/6)%t.bp.length];}
  const before=structuredClone({gold:p.gold,streak:p.streak,items:p.items,augs:p.augs,mech:p.mech});
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;
  const gold=g.roundEnd(p,counts,{team,won,pve,match:{combat:missing?null:own}});
  cases.push({team,view,first,missing,won,pve,counts,added,sponsor:g.mech.arcSponsor,before,gold,after:structuredClone({gold:p.gold,items:p.items,mech:p.mech}),nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items,arcaneAnomaly:u.mods.arcAnom}));
 traces.push({variant:`arcane/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,rewards:[0,1].map(team=>{const mine=cb.units.filter(u=>u.team===team);return {conquestKills:mine.reduce((s,u)=>s+(u.state.arcConK||0),0),coins:mine.reduce((s,u)=>s+(u.state.coins||0),0),bountyGold:mine.reduce((s,u)=>s+(u.state.bountyGold||0),0),anomalyKills:mine.reduce((s,u)=>s+(u.state.anomK||0),0)}}),cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/arcane-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Arcane settlement traces.`);
