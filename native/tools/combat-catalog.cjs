// Audited wave functions depend only on stage, round, pace and Dragonlands treasure.
// All stage branches are exhausted by stage 10 (Underlords/Auto Chess round 45
// is the last exceptional wave). Later stages use their final boss wave.
module.exports = function combatCatalog(DT, pace) {
  const waves = {}, treasures = DT.SET.id === 'dragonlands' ? ['classic','order','chaos'] : ['classic'];
  for (const treasure of treasures) {
    const game = {pace, mech:{treasure}}, stages = [];
    for (let stage = 1; stage <= 10; stage++) {
      const rounds = [];
      for (let round = 1; round <= 7; round++) rounds.push(DT.pveWave(stage,round,game).map(([line,star,column,row]) => ({line,stage:star,column,row,uid:-1})));
      stages.push(rounds);
    }
    for (const stage of [11,20,100,1000]) for (let round=1; round<=7; round++) {
      const actual=DT.pveWave(stage,round,game).map(([line,star,column,row])=>({line,stage:star,column,row,uid:-1}));
      if(JSON.stringify(actual)!==JSON.stringify(stages[9][round-1])) throw Error('PvE tail changed: '+DT.SET.id);
    }
    waves[treasure]=stages;
  }
  const formations=[], indices=new Map();
  for(const treasure of treasures) waves[treasure]=waves[treasure].map(rounds=>rounds.map(wave=>{
    const key=JSON.stringify(wave);if(!indices.has(key)){indices.set(key,formations.length);formations.push(wave);}return indices.get(key);
  }));
  return {overtime:DT.HYPER.combatTime.overtime,limit:DT.HYPER.combatTime.limit,waves,formations};
};
