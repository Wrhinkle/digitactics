const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(let n=0;n<256;n++){
 DT.Sets.activate('reckoning',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*53+19;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:'planning',stage:1+n%6,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p=new DT.Player(0,'Player',human,g);g.players=[p];g.refreshShop(p);const added=[];
 for(let j=0;j<n%10;j++){
  const u=g.newUnit(DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id,j%3);u.items=DT.CORE_ITEMS.slice(0,(n+j)%4);u.noItems=false;
  if(j<7)p.board[`${j},4`]=u;else p.bench[j-7]=u;added.push({unit:structuredClone(u),board:j<7,col:j});
 }
 const shadow=DT.SPECIAL_COMPONENTS.filter(k=>DT.COMPONENTS[k].shadow),stock=[...shadow,...DT.STAT_COMPONENTS,'rec_egg','rec_tome','rec_remover'];
 p.items=[];for(let j=0;j<n%31;j++)p.items.push(stock[(n*7+j)%stock.length]);
 if(n%4===0)p.items=['rec_egg',...shadow,...p.items].slice(0,R.itemCap);
 if(n%5===0)p.items=['rec_tome','rec_tome','rec_tome','rec_tome','rec_remover','rec_remover','rec_remover','rec_remover',...p.items].slice(0,R.itemCap);
 const before=p.items.slice();g.fireGame('onAiTurn',p,{});
 traces.push({variant:`reckoning/${pace}`,human,n,seed,phase:g.phase,stage:g.stage,added,drained:null,before,after:structuredClone({gold:p.gold,items:p.items,board:p.board,bench:p.bench,shop:p.shop,pool:g.pool}),nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/reckoning-ai.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Reckoning AI traces.`);
