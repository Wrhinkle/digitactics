const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['underlords','underlords_ko'])for(const pace of ['normal','hyper'])for(let battle=0;battle<6;battle++){
 DT.Sets.activate(set,pace);const R=DT.HYPER,seed=8100+battle;
 const side=team=>['ul_bounty','ul_legion','ul_axe','ul_beastmaster',battle%2?'ul_venomancer':'ul_bristleback',battle%2?'ul_lonedruid':'ul_snapfire'].map((line,i)=>({line,stage:(battle+i+team)%3,c:i,r:4+i%2,uid:team*20+i+1,owner:i%3===2?null:i%2===0?team:team+2,items:i===0?['ul_rapier']:[]}));
 const a=side(0),b=side(1),cb=new DT.Combat({units:a},{units:b},{seed});let ticks=0;while(!cb.done&&ticks<2000){cb.step();ticks++;}if(!cb.done)throw Error('Unfinished battle');
 const cases=[];for(let n=0;n<240;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,mech:{},rng:DT.makeRng(143),phase:'combat',stage:1+n%10,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const players=[new DT.Player(0,'A',true,g),new DT.Player(1,'B',true,g)];g.players=players;for(const q of players)g.refreshShop(q);
  const team=n%2,p=players[team],opp=players[1-team],view=n%3===1,first=n%4===0,missing=n%17===0,won=n%4<2,pve=n%5===0&&team===0,ghost=n%7===0,dead=n%19===0,absent=n%23===0&&team===0;
  p.mech={ulBeastKills:n%7*.25,ulDuel:n%4*6,ulSilver:n%5,ulFree:!!(n%3)};p.hp=[0,1,3,10,100][n%5];opp.hp=30;opp.alive=!dead;
  p.globals=['ul_g_silver','ul_g_aegis','ul_g_vicious'].filter((_,i)=>(Math.floor(n/2))&(1<<i));if(n%11===0)p.globals.push(...p.globals);
  for(let i=0;i<2;i++){const u=g.newUnit(DT.SHOP_LINES[i].id,0);u.items=['ul_rapier','ul_rapier'];if(n%6===0)p.bench[i]=u;else p.board[`${i},4`]=u;}
  opp.items=Array(n%3===0?30:n%3===1?29:0).fill('ul_rapier');
  const before=structuredClone({mech:p.mech,hp:p.hp,globals:p.globals,inventory:opp.items});
  const own=Object.create(cb);own.units=view?cb.units.filter(u=>u.team!==team||u.owner===p.id||(u.owner==null&&first)):cb.units;
  const match={combat:missing?null:own,ghost,a:absent&&team===1?null:players[0],b:absent&&team===0?null:players[1]};
  const e={gold:0,team,won,pve,match};g.fireGame('onRoundEnd',p,e);
  cases.push({n,team,view,first,missing,won,pve,ghost,dead,absent,stage:g.stage,before,gold:e.gold,after:{mech:p.mech,hp:p.hp,opponentHp:opp.hp,globals:p.globals,inventory:opp.items,items:p.allUnits().map(u=>u.items)},nextRandom:g.rng()});
 }
 const convert=arr=>arr.map(u=>({line:u.line,stage:u.stage,column:u.c,row:u.r,uid:u.uid,owner:u.owner,items:u.items}));
 traces.push({variant:`${set}/${pace}`,seed,sideA:convert(a),sideB:convert(b),ticks,winner:cb.winner,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/underlords-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Underlords settlement traces.`);

