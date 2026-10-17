const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),events=[];
 Object.assign(g,{R,set:DT.SET,mech:{},uid:0,rng:DT.makeRng(143),phase:'planning',duo,hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:8},(_,id)=>({id,name:'Player '+id,isHuman:id===0||id===3,alive:true,hp:R.startHp,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),items:[],globals:[],mech:{},augs:[],boardUnits(){return Object.values(this.board);},allUnits(){return this.boardUnits().concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));if(duo)g.setupTeams();g.players.forEach(p=>g.refreshShop(p));g.hasHook=()=>true;g.fireGame=(name,p,e)=>{events.push({name,player:p.id,uid:e.unit?.uid??null});return e;};
 const snap=()=>structuredClone({carousel:g.carousel?{opts:g.carousel.opts,order:g.carousel.order.map(p=>p.id),idx:g.carousel.idx}:null,pool:g.pool,players:g.players.map(p=>({gold:p.gold,items:p.items,shop:p.shop,board:p.board,bench:p.bench})),events});const steps=[];
 for(let n=0;n<50;n++){
  g.stage=n%9+1;g.phase=n%6===0?'combat':'planning';events.length=0;
  const health=g.players.map((p,i)=>p.hp=(n+i*3)%11);const drained={};if(n%7===0)for(const l of DT.SHOP_LINES)if(n%14===0||l.tier===1){drained[l.id]=g.pool[l.id];g.pool[l.id]=0;}
  g.startCarousel();const started=snap();const actions=[];
  while(g.carouselTurn()&&actions.length<n%3){
   const p=g.carouselTurn();const invalid=g.carouselPick(p,99);const options=g.carousel.opts.map((o,i)=>!o.by?i:-1).filter(i=>i>=0);if(!options.length)break;
   const wrong=g.carouselPick(g.players[(p.id+1)%8],options[0]);
   const slot=options[(n+p.id)%options.length],result=g.carouselPick(p,slot);g.carouselStep();actions.push({player:p.id,slot,invalid,wrong,result,state:snap()});
  }
  g.finishCarousel();const finished=snap();g.finishCarousel();
  steps.push({stage:g.stage,phase:g.phase,health,drained,started,actions,finished});for(const [id,count]of Object.entries(drained))g.pool[id]+=count;
 }
 traces.push({variant:`${set.id}/${pace}`,duo,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/carousels.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} carousel traces.`);
