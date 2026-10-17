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
   const u={uid:i+1,line:pick(lines).id,stage:Math.floor(random()*3),items:Array.from({length:i%4},()=>pick(items)),noSell:n%11===0||i%13===0,bonusTraits:{[pick(Object.keys(DT.TRAITS))]:1+i%3}};
   if(board)p.board[`${i%7},${4+Math.floor(i/7)}`]=u;else p.bench[index]=u;
   setup.push({board,index,unit:structuredClone(u)});
  }
  const capacity=n%12, partnerCopies=n%4; p.gold=n%16; p.cap=()=>capacity;
  g.maxTier=Math.max(...DT.SHOP_LINES.map(l=>l.tier));g.duo=true;g.aiPartnerWants=()=>partnerCopies;
  const partner={alive:n%3!==0,boardUnits:()=>p.boardUnits().slice(0,n%8)};g.partnerOf=()=>partner;
  const counts=g.aiCounts(p,p.allUnits());
  const wants=lines.map(l=>({id:l.id,value:g.aiWants(p,l.id)}));
  let worst=null;g.sell=(_p,uid)=>{worst=uid;return true;};g.aiSellWorst(p);
  g.canPlace=(_p,_u,c,r)=>(c+r+n)%4!==0;
  const cells=lines.map(l=>({id:l.id,cell:g.freeCellFor(p,{line:l.id})}));

  cases.push({setup,capacity,partnerCopies,gold:p.gold,partnerAlive:partner.alive,partnerCount:n%8,counts,wants,worst,cells});
 }
 traces.push({variant:`${set.id}/${pace}`,cases});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/ai-decisions.json.gz'),zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} AI decision traces.`);
