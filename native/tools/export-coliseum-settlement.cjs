const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(let battle=0;battle<12;battle++){
 DT.Sets.activate('coliseum',pace);const R=DT.HYPER,seed=9000+battle;
 const sc=DT.SHOP_LINES.filter(l=>l.traits.includes('SC')),ordinary=DT.SHOP_LINES.filter(l=>!l.traits.includes('SC')&&l.id!=='col_craniamon'),summoner=DT.SHOP_LINES.find(l=>l.id==='col_lotosmon');
 const side=team=>Array.from({length:6},(_,i)=>({line:i<2&&battle%3!==0?sc[(i+battle)%sc.length].id:i===2&&battle%2===0?'col_craniamon':i===3&&summoner?summoner.id:ordinary[(i+battle+team*5)%ordinary.length].id,stage:(battle+i+team)%3,c:i,r:4+i%2,uid:team*10+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:[]}));
 const a=side(0),b=side(1),cb=new DT.Combat({units:a},{units:b},{seed});let ticks=0;
 while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Battle did not finish');
 const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,rng:DT.makeRng(143),phase:'combat',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const players=Array.from({length:2},(_,id)=>({id,isHuman:true,gold:40,hp:30,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],augs:[],globals:[],mech:{},allUnits(){return [];}}));for(const p of players)g.refreshShop(p);
 const cases=[];
 for(let n=0;n<240;n++){
  const team=n%2,p=players[team],stage=[1,2,3,4,8,20][Math.floor(n/2)%6],winner=n%3-1,pve=n%11===0&&team===0,view=n%3===0,first=n%4===0,missing=n%17===0;
  const counts={CG:[0,1,2,3,4,5,6,8][Math.floor(n/2)%8],CH:n%4<2?1:0};
  const initial={gem:n%101+.5,gLv:n%4,gCount:n%5,dd:Math.floor(n/2)%2===0,gReady:n%7===0,gKills:n%20,gTarget:[0,1,6,8,10][n%5],fans:n%12,supreme:'previous'};
  p.mech=structuredClone(initial);p.gold=40;p.hp=30;p.items=Array(n%4===0?R.itemCap:n%4===1?R.itemCap-1:0).fill(DT.CORE_ITEMS[0]);const inventory=p.items.slice();
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;own.traitCounts=[counts,counts];
  g.stage=stage;DT.SET.hooks.onRoundEnd(g.ctx(p),{team,won:winner===team,pve,match:{combat:missing?null:own}});
  cases.push({team,stage,winner,pve,view,first,missing,counts,initial,inventory,after:{mech:p.mech,gold:p.gold,hp:p.hp,items:p.items},nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items}));
 traces.push({variant:`coliseum/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,units:cb.units.map(u=>({line:u.line.id,stage:u.stage,team:u.team,alive:u.alive,summoned:!!u.summoned,owner:u.owner??null,dealt:u.dealt,traits:u.traits})),cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/coliseum-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Coliseum post-combat traces.`);
