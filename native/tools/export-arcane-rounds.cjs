const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const portal of ['rolls','golden','prismatic','gold'])for(let n=0;n<64;n++){
 DT.Sets.activate('arcane',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,pace,uid:0,rng:DT.makeRng(n+1),phase:'planning',stage:1,round:1,mech:{arcPortal:portal},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p=new DT.Player(0,'Player',n%2===0,g);g.players=[p];g.refreshShop(p);p.freeRolls=n%3;p.augs=n%3===0?['arc_a_rolls']:[];
 p.mech={arcMarkets:n%5,arcMarketsOpened:n%3,arcMarketPlus:n%2===0,arcSynLv:1+n%4,arcPendingAnom:n%3===0,arcJunkT:n%4,arcJunk:['flame','bots','rod','destruct','buster'].slice(0,n%6),arcSeen:n%7===0?Object.keys(DT.ArcaneInfo.ANOM):[]};
 if(n%4!==0){const u=g.newUnit('arc_chaosdromon',n%3);if(n%4===1)p.board['0,4']=u;else p.bench[0]=u;}
 const before=structuredClone({rolls:p.freeRolls,augs:p.augs,mech:p.mech,board:p.board,bench:p.bench});const steps=[];
 for(const [stage,round] of [[1,2],[2,1],[2,2],[3,1],[3,2],[4,1],[4,2],[4,3],[5,1],[5,2]]){
  g.stage=stage;g.round=round;g.makeOffer(p);g.fireGame('onRoundStart',p,{});const first=structuredClone({rolls:p.freeRolls,mech:p.mech,offer:p.offer});g.fireGame('onRoundStart',p,{});steps.push({stage,round,first,second:structuredClone({rolls:p.freeRolls,mech:p.mech,offer:p.offer})});
 }
 traces.push({variant:`arcane/${pace}`,portal,n,before,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/arcane-rounds.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Arcane round traces.`);
