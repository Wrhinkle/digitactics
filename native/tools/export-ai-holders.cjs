const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),random=DT.makeRng(934);
 Object.assign(g,{R,set:DT.SET,rng:DT.makeRng(143),phase:'planning',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p={id:0,isHuman:false,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));}};g.refreshShop(p);
 const lines=Object.values(DT.LINE).filter(l=>l.ai),items=Object.keys(DT.ITEMS),pick=a=>a[Math.floor(random()*a.length)],cases=[];
 for(let n=0;n<900;n++){
  p.board={};p.bench=Array(9).fill(null);const setup=[];
  for(let i=0;i<n%29+n%10;i++){
   const board=i<n%29,index=board?i:i-n%29;
   const u={uid:i+1,line:pick(lines).id,stage:Math.floor(random()*3),items:Array.from({length:i%4},()=>pick(items)),noItems:n%11===0||i%13===0,mods:{form:'fixture',pu:n%10===0||i%4===0?'warp':'',other:7}};
   if(board)p.board[`${i%7},${4+Math.floor(i/7)}`]=u;else p.bench[index]=u;
   setup.push({board,index,unit:structuredClone(u)});
  }
  p.items=set.id==='coliseum'?['col_bow',...Array(n%7).fill('col_core'),'col_vest']:[];
  const inventory=p.items.slice(),holders=g.aiHolders(p).map(h=>({uid:h.u.uid,kind:h.kind}));
  if(set.id==='coliseum')DT.SET.hooks.onAiTurn(g.ctx(p),{});
  cases.push({setup,inventory,holders,after:{items:p.items,units:p.allUnits()},nextRandom:g.rng()});
 }
 traces.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-holders.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} AI holder/core traces.`);
