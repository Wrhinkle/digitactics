const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let battle=0;battle<10;battle++){
 DT.Sets.activate('galaxies',pace);const R=DT.HYPER,seed=7100+battle;
 const pirates=DT.SHOP_LINES.filter(l=>l.traits.includes('PI')),pilots=DT.SHOP_LINES.filter(l=>l.traits.includes('XR')),meep=DT.SHOP_LINES.find(l=>l.type==='meep');
 const side=team=>{
  const chosen=battle%2===0?[...pirates.slice(0,2),meep,...pilots.slice(0,3)]:[...pirates,meep,DT.SHOP_LINES[(battle+team*3)%DT.SHOP_LINES.length]];
  return chosen.map((l,i)=>({line:l.id,stage:(battle+i+team)%3,c:i%7,r:4+Math.floor(i/7),uid:team*20+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:i===0&&battle%3===0?["gal_portal"]:[]}));
 };
 const a=side(0),b=side(1),cb=new DT.Combat({units:a},{units:b},{seed});let ticks=0;while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Unfinished battle');
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,mech:{},rng:DT.makeRng(143),phase:'combat',stage:1,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=Array.from({length:2},(_,id)=>({id,isHuman:true,gold:40,hp:30,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],augs:[],globals:[],mech:{},allUnits(){return [];}}));for(const p of players)g.refreshShop(p);
 const cases=[];for(let n=0;n<240;n++){
  const team=n%2,p=players[team],stage=[1,3,4,8][Math.floor(n/2)%4],galaxy=['normal','treasure','bigbang','plunder'][Math.floor(n/8)%4],pve=n%3===0&&team===0,view=n%3===1,first=n%4===0,missing=n%17===0;
  g.stage=stage;g.mech.galaxy=galaxy;const counts={PI:[0,2,4,6][Math.floor(n/2)%4]};p.gold=40;p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);const inventory=p.items.slice();
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;own.traitCounts=[counts,counts];
  const gold=g.roundEnd(p,counts,{team,won:cb.winner===team,pve,match:{combat:missing?null:own}});
  cases.push({team,stage,galaxy,pve,view,first,missing,counts,inventory,gold,items:p.items,nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items}));
 traces.push({variant:`galaxies/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,pirates:cb.teamFx.map(f=>f.pirate||0),meeps:cb.teamFx.map(f=>f.meep||0),units:cb.units.map(u=>({line:u.line.id,team:u.team,alive:u.alive,summoned:!!u.summoned,piloting:!!u.state.piloting})),cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/galaxies-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Galaxies settlement traces.`);
