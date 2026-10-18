const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
  DT.Sets.activate(set.id,pace);
  for(const treasure of (set.id==='dragonlands'?['classic','order','chaos']:['classic']))
    for(const stage of [...Array.from({length:12},(_,i)=>i+1),100,1000])for(let round=1;round<=7;round++){
      const game=Object.create(DT.Game.prototype);
      Object.assign(game,{stage,round,pace,R:DT.HYPER,mech:{treasure},roundType:'pve'});
      traces.push({variant:`${set.id}/${pace}`,stage,round,treasure,units:game.pveSide().units,time:game.combatInfo()});
    }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/pve-waves.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} PvE wave/timer cases.`);
