const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib');
const DT=require('../../tools/headless.cjs').load({strict:true});
const lootTraces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const full of [false,true])for(const seed of [0,143,98765]){
 DT.Sets.activate(set.id,pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype);
 Object.assign(g,{R,uid:0,rng:DT.makeRng(seed),pace,phase:'combat',hasHook:()=>false,pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 g.players=Array.from({length:2},(_,i)=>({id:i,isHuman:i===0,gold:R.startGold??3,level:R.xp?R.xp.startLevel||1:R.levelByStage[1],board:{},bench:Array(9).fill(null),shop:Array(R.shopSize||5).fill(null),orbs:[],items:[],mech:{},boardUnits(){return Object.values(this.board);},allUnits(){return Object.values(this.board).concat(this.bench.filter(Boolean));},freeBenchSlot(){return this.bench.indexOf(null);}}));
 for(const p of g.players)g.refreshShop(p);
 const line=DT.SHOP_LINES[0].id;
 for(const p of g.players){const u=g.newUnit(line,0);p.board['3,4']=u;g.takeFromPool(line);}
 if(full)for(const p of g.players)for(let j=0;j<9;j++){p.bench[j]=g.newUnit(line,2);g.takeFromPool(line,DT.unitCopies(p.bench[j]));}
 // Restore a known unit reserve to exercise both exhausted and available custom drops.
 g.pool[line]=3;
 const snapshot=()=>structuredClone({pool:g.pool,players:g.players.map(p=>({gold:p.gold,items:p.items,orbs:p.orbs,bench:p.bench,shop:p.shop,loot:p.mech.loot??null,treasureWon:p.mech.treasureWon??null}))});
 const steps=[];
 for(let i=0;i<120;i++){
  const index=i%2,p=g.players[index],stage=1+Math.floor(i/8)%10,round=1+i%R.roundsInStage(stage),won=i%3!==0;
  g.stage=stage;g.round=round;
  const specs=null;g.set=DT.SET;
  const reward=g.pveReward(p,won),before=snapshot();
  const collect=i%3===0;if(collect)g.collectAllOrbs(p);
  steps.push({index,stage,round,won,specs,reward,before,collect,after:snapshot()});
 }
 lootTraces.push({variant:`${set.id}/${pace}`,full,seed,line,steps,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/mode-loot.json.gz'),zlib.gzipSync(JSON.stringify(lootTraces)));
console.log(`Exported ${lootTraces.length} mode reward traces.`);
