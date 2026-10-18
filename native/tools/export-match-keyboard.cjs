// Execute the original capture/bubble handlers to freeze their input priority.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),uiSource=fs.readFileSync(path.join(root,'js/ui.js'),'utf8'),scoutSource=fs.readFileSync(path.join(root,'js/scout.js'),'utf8');
const menu=uiSource.slice(uiSource.indexOf("  window.addEventListener('keydown', e => {"),uiSource.indexOf('  // ---------- actions ----------'));
const ui=uiSource.slice(uiSource.lastIndexOf("  window.addEventListener('keydown', (ev) => {"),uiSource.indexOf('  function fit()'));
const scout=scoutSource.slice(scoutSource.indexOf('// Space and Esc return home'),scoutSource.indexOf('// ---------- per frame:'));
const cycleSource=scoutSource.slice(scoutSource.indexOf('function cycleOrder(g)'),scoutSource.indexOf('// Switch the view'));
const orderSource=uiSource.match(/const list = game.players.slice\(\).sort\(([^;]+)\);/)[1];
const order=Function('game',`return game.players.slice().sort(${orderSource});`);
const rows=[];
for(let seed=0;seed<32;seed++)for(const viewed of [0,1,5])for(const hovered of [null,91])for(const repeated of [false,true])for(const key of ['1','2','3','4','5','6','7','8','q','Q','e','E','d','f',' ','Escape','r','=','+','-','_']){
 const players=Array.from({length:8},(_,id)=>({id,alive:id===0||!!(seed&(1<<(id%5))),hp:100-(id*17+seed*3)%101,placement:id?((id+seed)%7+1):0,team:(seed&1)?Math.floor(id/2):null}));
 const game={players,human:players[0],duo:!!(seed&1),over:!!(seed&2),phase:seed&4?'combat':'planning'},state={viewing:viewed===0?null:viewed,hoverUid:hovered};
 const context={players:players.map(p=>({id:p.id,alive:p.alive,health:p.hp,placement:p.placement,team:p.team})),localSeat:0,viewed,duos:game.duo,over:game.over,planning:game.phase==='planning',hovered,textInput:!!(seed&8),modified:false,blocked:false,repeated};
 // The source game-menu capture handler deliberately opens from body focus.
 // Native text-entry protection is checked separately, including Escape.
 if(context.textInput&&key==='Escape')continue;
 const calls=[],handlers=[],window={addEventListener(_,fn,capture){handlers.push({fn,capture:!!capture});}};
 const emit=(kind,seat=null,unit=null)=>calls.push({kind,seat,unit});
 const view=id=>{id=id==null?0:id;if(id!==viewed)emit('scout',id);},home=()=>view(0);
 const document={querySelector:()=>false,body:{classList:{contains:()=>false}}},$=()=>({classList:{contains:()=>true}});
 const renderer={resetView:()=>emit('reset'),zoomAt:(_x,_y,k)=>emit(k<1?'zoomIn':'zoomOut')};
 Function('window','gameMenu','leavingGame','game','ui','DT','document','$','openGameMenu',menu)(window,{open:false},false,game,state,{},document,$,()=>emit('menu'));
 Function('window','game','ui','renderer','canvas','closePicker','reroll','buyXp','sellUid','ready',ui)(window,game,state,renderer,{clientWidth:100,clientHeight:100},()=>{},()=>emit('reroll'),()=>emit('buyXp'),id=>{if(viewed===0)emit('sell',null,id);},()=>emit('fight'));
 Function('window','game','ui','viewingId','trans','canScout','listOrder','home','document','view',`let following=false;${cycleSource}\n${scout}`)(window,()=>game,()=>state,()=>state.viewing,null,g=>!!g&&!g.over,order,home,document,view);
 const event={key,repeat:repeated,target:{tagName:context.textInput?'INPUT':'BODY'},ctrlKey:false,metaKey:false,altKey:false,defaultPrevented:false,stopped:false,preventDefault(){this.defaultPrevented=true;},stopImmediatePropagation(){this.stopped=true;}};
 for(const h of handlers.sort((a,b)=>Number(b.capture)-Number(a.capture))){h.fn(event);if(event.stopped)break;}
 if(calls.length>1)throw Error('Ambiguous original keyboard action '+JSON.stringify({key,context,calls}));
 rows.push({key,context,expected:calls[0]||null});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/match-keyboard.json.gz'),zlib.gzipSync(JSON.stringify(rows),{level:9}));
console.log(`Exported ${rows.length} original match keyboard projections.`);
