const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const source=fs.readFileSync(path.join(__dirname,'../../js/shop.js'),'utf8');
const trackSource=source.slice(source.indexOf('let tr = null;'),source.indexOf('setInterval(() => { try { track();'));
const DT=require('../../tools/headless.cjs').load({strict:true});const traces=[];
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duo of [false,true]){
 const g=new DT.Game({setId:set.id,pace,seed:143,format:duo?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0}))});g.playOut=true;
 const context={UI:()=>({}),Math};vm.createContext(context);vm.runInContext(trackSource+'\nthis.observe=g=>statsOf(track(g),g.human.placement);',context);
 const observations=[context.observe(g)];
 for(let n=0;n<100&&!g.over;n++){
  if(g.roundType!=='carousel'&&n%3!==0)g.aiTurn(g.players[0]);
  g.startCombat();observations.push(context.observe(g));g.finishCombat();observations.push(context.observe(g));observations.push(context.observe(g));
  if(!g.over)g.nextRound();
 }
 if(!g.over)throw Error('Game unfinished');traces.push({variant:`${set.id}/${pace}`,duo,observations});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/reward-tracker.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} original reward tracker traces.`);
