const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let battle=0;battle<12;battle++){
 DT.Sets.activate('dragonlands',pace);const R=DT.HYPER,seed=9100+battle;
 const special=['STARCALL','CORE','LAGOON','LAGOON','LAGOON','STAR'];
 const side=team=>special.map((trait,i)=>{const choices=DT.SHOP_LINES.filter(l=>l.traits.includes(trait));const l=choices[(battle+i+team)%choices.length];return {line:l.id,stage:(battle+i+team)%3,c:i,r:4+i%2,uid:team*20+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:[['drg_g_axe'],['drg_g_blade'],['drg_g_staff'],['drg_g_mail'],['drg_g_investor'],['drg_g_gem']][i]};});
 const a=side(0),b=side(1),cb=new DT.Combat({units:a,augs:['drg_training']},{units:b,augs:['drg_training']},{seed,info:{type:'pvp'}});let ticks=0;while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Unfinished battle');
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,mech:{},rng:DT.makeRng(143),phase:'combat',stage:1,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=[new DT.Player(0,'A',true,g),new DT.Player(1,'B',true,g)];g.players=players;for(const p of players)g.refreshShop(p);
 const cases=[];for(let n=0;n<240;n++){
  const team=n%2,p=players[team],missing=n%17===0,view=n%3===1,first=n%4===0,won=n%4<2,pve=n%5===0&&team===0;
  p.board={};p.bench=Array(9).fill(null);const added=[];for(let i=0;i<2;i++){const u=g.newUnit(DT.SHOP_LINES[i].id,0);u.items=n%7===0?[]:['drg_g_investor','drg_g_investor'];if(i===0&&n%3!==0)p.board['0,4']=u;else p.bench[p.freeBenchSlot()]=u;added.push({unit:structuredClone(u),board:i===0&&n%3!==0});}
  p.hp=R.startHp+n%5-3;p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);p.augs=['drg_training','drg_calcloss'].filter((_,i)=>n&(1<<i));
  p.mech={starN:n%6,starShop:!!(n%3),lagoon:n%65,lagoonPaid:n%7,doots:n%25,ctKills:n%45,calcRoll:!!(n%2),axe:n%110,investor:n%7};
  const counts={};for(const id of n%2?['CORE','LAGOON','STARCALL','STAR']:['STAR','LAGOON','STARCALL','CORE']){const t=DT.TRAITS[id];counts[id]=n%4===0?0:t.bp[Math.floor(n/4)%t.bp.length];}
  const before=structuredClone({hp:p.hp,items:p.items,augs:p.augs,mech:p.mech});
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;
  const gold=g.roundEnd(p,counts,{team,won,pve,match:{combat:missing?null:own}});
  cases.push({team,missing,view,first,won,pve,counts,before,added,gold,after:structuredClone({hp:p.hp,items:p.items,mech:p.mech,units:p.allUnits()}),nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items}));
 traces.push({variant:`dragonlands/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,rewards:cb.teamFx.map(f=>Object.fromEntries(['gold','axe','investor','lagoon','doots','wish','ctKills'].map(k=>[k,f[k]||0]))),cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/dragon-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Dragonlands settlement traces.`);

