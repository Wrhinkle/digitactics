const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib');
const DT = require('../../tools/headless.cjs').load({strict:true}), traces = [];
for (const pace of ['normal','hyper']) {
  DT.Sets.activate('faction-wars', pace);
  for (let count = 0; count <= 16; count++) for (const won of [false,true]) for (const pve of [false,true]) {
    const g = Object.create(DT.Game.prototype);
    Object.assign(g, {R:DT.HYPER,set:DT.SET,stage:4,round:1,phase:'combat',mech:{},rng:DT.makeRng(143)});
    const p = new DT.Player(0,'Player',true,g); g.players = [p];
    const gold = g.roundEnd(p,{FUN:count},{won,pve,team:0,match:{combat:{units:[]}}});
    traces.push({variant:`faction-wars/${pace}`,count,won,pve,gold,nextRandom:g.rng()});
  }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/faction-settlement.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} Faction Wars settlement cases.`);
