const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let battle=0;battle<10;battle++){
 DT.Sets.activate('fates',pace);const R=DT.HYPER,seed=8100+battle;
 const side=team=>Array.from({length:6},(_,i)=>({line:i<2?'fat_aruraumon':DT.SHOP_LINES[(battle+i+team*9)%DT.SHOP_LINES.length].id,stage:(battle+i+team)%3,c:i,r:4+i%2,uid:team*20+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:i<2?['fat_art_collector']:i===2?['fat_zzrot']:[]}));
 const a=side(0),b=side(1),cb=new DT.Combat({units:a},{units:b},{seed});let ticks=0;while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Unfinished battle');
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,mech:{},rng:DT.makeRng(143),phase:'combat',stage:1,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=Array.from({length:2},(_,id)=>({id,isHuman:true,gold:40,hp:30,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],augs:[],globals:[],mech:{},allUnits(){return [];}}));for(const p of players)g.refreshShop(p);
 const cases=[];for(let n=0;n<120;n++){
  const team=n%2,p=players[team],view=n%3===1,first=n%4===0,missing=n%17===0,initial=[0,1,57,58,60,72][Math.floor(n/2)%6];p.mech.blossom=initial;
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;
  const e={gold:0,team,pve:false,match:{combat:missing?null:own}};DT.SET.hooks.onRoundEnd(g.ctx(p),e);
  cases.push({team,view,first,missing,initial,gold:e.gold,blossom:p.mech.blossom,nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items}));
 traces.push({variant:`fates/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,units:cb.units.map(u=>({line:u.line.id,team:u.team,alive:u.alive,summoned:!!u.summoned,blossom:u.state.blossom||0,gold:u.state.gold||0})),cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/fates-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Fates settlement traces.`);
