// Run the original mini-game factories with a tiny DOM and a deterministic timer clock.
const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/farm-games.js'),'utf8');
function element(){
 const children=new Map(),classes=new Set();
 return {style:{},textContent:'',clientWidth:600,offsetWidth:600,
  classList:{add:x=>classes.add(x),remove:x=>classes.delete(x),toggle:(x,on)=>on?classes.add(x):classes.delete(x),contains:x=>classes.has(x)},
  insertAdjacentHTML(){},appendChild(){},remove(){},
  querySelector(key){if(!children.has(key))children.set(key,element());return children.get(key);},
  querySelectorAll(key){return Array.from({length:key==='.dg-pad'?4:2},(_,i)=>this.querySelector(key+i));}
 };
}
const cases=[];
for(const name of ['strength','speed','wisdom','stamina'])for(const delta of [1/60,.05,.007,.031])for(const style of ['idle','auto','spam','sparse','stutter']){
 let now=0,wallNow=0,next=0;const timers=new Map(),sounds=[];
 const ctx={DT:{Sfx:{play:n=>sounds.push(n)}},document:{createElement:element},
  setTimeout:(fn,ms)=>{const id=++next;timers.set(id,{at:wallNow+ms/1000,fn});return id;},clearTimeout:id=>timers.delete(id)};
 ctx.window=ctx;vm.createContext(ctx);vm.runInContext(source,ctx);
 const ui={stage:element(),left:element(),right:element(),feedback:'',remaining:1,say(s){this.feedback=s;},time(f){this.remaining=Math.max(0,Math.min(1,f));}};
 const game=ctx.DT.FarmGames.GAMES[name].make(ui),frames=[];let actions=[];
 const input=game.input.bind(game);game.input=x=>{actions.push(x??0);input(x);};
 for(let frame=0;frame<6000;frame++){
  now+=delta;
  const wallDelta=style==='stutter'&&frame%17===0?.2:delta;wallNow+=wallDelta;
  for(const [id,timer] of [...timers].sort((a,b)=>a[1].at-b[1].at||a[0]-b[0]))if(timer.at<=wallNow){timers.delete(id);timer.fn();}
  actions=[];
  const result=game.update(delta,now);
  if(result==null){
   if(style==='auto'||style==='stutter')game.auto(now);
   else if(style==='spam'||style==='sparse'&&frame%17===0)game.input(name==='wisdom'?frame%4:name==='speed'?frame%2:0);
  }
  const view={left:ui.left.textContent,right:ui.right.textContent,feedback:ui.feedback,timeLeft:ui.remaining,sounds:sounds.splice(0)};
  if(name==='strength')view.position=parseFloat(ui.stage.querySelector('.dg-sw-mark').style.left||'0')/100;
  if(name==='wisdom')view.litPad=ui.stage.querySelectorAll('.dg-pad').findIndex(x=>x.classList.contains('lit'));
  if(name==='stamina')view.meter=parseFloat(ui.stage.querySelector('.dg-meter .f').style.height)/100;
  frames.push({time:now,wallDelta,actions,score:result==null?null:Math.max(0,Math.min(1,result)),...view});
  if(result!=null)break;
 }
 if(frames.at(-1).score==null)throw Error('Training did not finish: '+name+'/'+style);
 const keys=Object.fromEntries([' ','Enter','Escape','f','F','ArrowUp','w','j','J','ArrowDown','s','1','2','3','4','5','x'].map(k=>[k,game.key(k)??null]));
 cases.push({name,delta,style,keys,frames});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/training-games.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${cases.length} original training runs / ${cases.reduce((n,c)=>n+c.frames.length,0)} frames.`);
