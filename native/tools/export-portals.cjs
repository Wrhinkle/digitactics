const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),random=DT.makeRng(817),pick=a=>a[Math.floor(random()*a.length)];
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(143),phase:'planning',duo:true,D:DT.Duos.rules(R),hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:8},(_,id)=>({id,isHuman:false,alive:true,gold:0,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],augs:[],portal:0,incoming:[],allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},boardUnits(){return Object.values(this.board);},freeBenchSlot(){return this.bench.indexOf(null);}}));g.setupTeams();g.players.forEach(p=>g.refreshShop(p));
 const p=g.players[0],q=g.partnerOf(p),lines=DT.SHOP_LINES.slice(0,5),items=Object.keys(DT.ITEMS),cases=[];
 for(let n=0;n<400;n++){
  const setup=[];for(const who of [p,q]){who.board={};who.bench=Array(9).fill(null);who.portal=0;who.incoming=[];who.augs=n%3===0?['duo_wide_portal']:[];who.items=Array.from({length:who===p?n%13:(n*3)%17},()=>pick(items));
   const units=[];for(let i=0;i<(who===p?n%10:9);i++){const u=g.newUnit(pick(lines).id,n%4===0?1:0);u.items=Array.from({length:i%4},()=>pick(items));u.noSell=n%17===0;who.bench[i]=u;units.push(structuredClone(u));}
   const board=[];for(let i=0;i<n%3&&i<units.length;i++){who.board[`${i},4`]=who.bench[i];who.bench[i]=null;board.push(i);}
   setup.push({id:who.id,units,board,items:who.items.slice(),augs:who.augs.slice()});}
  g.phase=n%11===0?'combat':n%13===0?'result':'planning';g.roundType=n%7===0?'carousel':'pvp';
  const events=[];g.hasHook=()=>true;g.fireGame=(name,who,e)=>{events.push({name,player:who.id,uid:e.unit?.uid??null});return e;};
  const what=n%3===0?{item:n%15}:n%3===1?{uid:(n%13===0?p.boardUnits()[0]:p.bench.find(Boolean))?.uid??-1}:{};
  const can=g.canPortal(p,what),sent=g.portal(p,what);g.aiPortal(p);
  const snap=who=>({id:who.id,portal:who.portal,items:who.items,bench:who.bench,board:who.board,shop:who.shop,incoming:who.incoming});
  cases.push({setup,phase:g.phase,roundType:g.roundType,what,can,result:typeof sent==='string'?sent:sent.kind,evolved:typeof sent==='string'?[]:sent.evolved.map(u=>u.uid),events,pool:structuredClone(g.pool),after:structuredClone([snap(p),snap(q)])});g.hasHook=()=>false;
 }
 traces.push({variant:`${set.id}/${pace}`,cases,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/portals.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} portal traces.`);
