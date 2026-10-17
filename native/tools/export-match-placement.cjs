const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const human of [false,true])for(let n=0;n<48;n++){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(143),phase:'planning',stage:1,round:1,mech:{galaxy:'bigger'},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p=new DT.Player(0,'Player',human,g);g.players=[p];g.refreshShop(p);const stage=1+n%9,xp=n%12*16;if(R.xp)g.addXp(p,xp);else p.level=R.levelByStage[Math.min(stage,R.levelByStage.length-1)];g.stage=stage;
 p.teamBonus=n%4;if(set.id==='gizmos'&&n%2)p.augs.push('giz_a_recruit');if(set.id.startsWith('underlords')){p.globals=Array(n%3).fill('ul_g_roster');p.power={id:n%3===0?'invalid':Object.keys(DT.POWER)[n%Object.keys(DT.POWER).length],rank:1};}if(set.id==='autochess')p.globals=Array(n%3).fill('ac_r_key');
 const eggs=Object.keys(DT.ITEMS).filter(id=>DT.ITEMS[id].p?.teamSize),tamers=Object.values(DT.LINE).filter(l=>l.tamer),added=[];
 for(let j=0;j<12;j++){
  const line=j===0&&tamers.length&&n%2===0?tamers[n%tamers.length].id:DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id,u=g.newUnit(line,j%3);
  if(eggs.length&&(j+n)%4===0)u.items=Array(1+n%2).fill(eggs[n%eggs.length]);if(j<3)p.board[`${j},4`]=u;else p.bench[j-3]=u;added.push({unit:structuredClone(u),board:j<3,col:j});
 }
 const native=s=>({isBoard:s.type==='board',column:s.c||0,row:s.r||0,index:s.i||0});
 const snap=()=>({cap:p.cap(),boardCount:p.boardCount(),board:structuredClone(p.board),bench:structuredClone(p.bench)}),initial=snap(),steps=[];
 for(let k=0;k<24;k++){
  g.phase=k%5===0?'combat':'planning';const from=k%3===0?{type:'board',c:k%7,r:4}: {type:'bench',i:(n+k)%9},to=k%4===0?{type:'bench',i:(n+k+3)%9}:{type:'board',c:(n+k)%7,r:4+Math.floor(k/7)%4};
  const result=g.move(p,from,to);steps.push({phase:g.phase,from:native(from),to:native(to),result:result===true?'Moved':result==='cap'?'Capacity':'Invalid',after:snap()});
 }
 traces.push({variant:`${set.id}/${pace}`,human,n,stage,xp,power:p.power,added,initial,steps});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/match-placement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} match placement traces.`);
