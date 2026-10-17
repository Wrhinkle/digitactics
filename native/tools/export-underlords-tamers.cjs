const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of ['underlords','underlords_ko'])for(const pace of ['normal','hyper'])for(const human of [false,true]){
 DT.Sets.activate(set,pace);const R=DT.HYPER,powers=Object.keys(DT.POWER),tamers=Object.values(DT.LINE).filter(l=>l.tamer);
 for(const power of powers)for(let n=0;n<18;n++){
  const g=Object.create(DT.Game.prototype);Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(143),phase:n%2?'combat':'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
  const p=new DT.Player(0,'Player',human,g);g.players=[p];g.refreshShop(p);const added=[];
  const boardCount=[0,1,7,27,28,28][n%6],benchCount=[0,8,9][Math.floor(n/6)];
  for(let i=0;i<boardCount+benchCount;i++){
   const line=i===0&&n%5===0?tamers[n%tamers.length].id:DT.SHOP_LINES[(n+i)%DT.SHOP_LINES.length].id,u=g.newUnit(line,2);
   const board=i<boardCount,col=i%7,row=4+Math.floor(i/7);if(board)p.board[`${col},${row}`]=u;else p.bench[i-boardCount]=u;added.push({unit:structuredClone(u),board,col,row});
  }
  const snap=()=>structuredClone({power:p.power,board:p.board,bench:p.bench,shop:p.shop,pool:g.pool,capacity:p.cap()});
  g.applyPower(p,power);const picked=snap();g.applyPower(p,power);const repeated=snap(),ranks=[];
  for(const rank of [1,2,3,4,5,6,7,10]){p.power.rank=rank;g.fireGame('onRankUp',p,{rank});ranks.push({rank,after:snap()});}
  traces.push({variant:`${set}/${pace}`,human,power,n,added,picked,repeated,ranks,nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
 }
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/underlords-tamers.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Underlords Tamer traces.`);
