const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper']){
 DT.Sets.activate('autochess',pace);const R=DT.HYPER;
 for(let n=0;n<1024;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,stage:1+n%10,round:1,mech:{}});
  const p={hp:n%31-5,augs:[],globals:[],mech:{}},team=n%2,pve=n%5===0&&team===0,won=n%3===0,missing=n%13===0;
  const units=Array.from({length:n%12},(_,i)=>({line:DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length],stage:(n+i)%3,team:i%2,alive:(n+i)%4!==0,summoned:(n+i)%7===0}));
  const counts={PRIEST:[0,1,2,3,4,8][Math.floor(n/2)%6]},before=p.hp,e={team,pve,won,gold:0,match:{combat:missing?null:{units}}};g.fireTraits('onRoundEnd',p,counts,e);
  traces.push({variant:`autochess/${pace}`,stage:g.stage,team,pve,won,missing,counts,before,units:units.map(u=>({...u,line:u.line.id})),after:p.hp});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/autochess-priest.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Auto Chess Priest cases.`);
