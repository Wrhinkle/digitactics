// Actual game flow: repaired rigs load, then attacks and casts play their skeletal clips in combat.
const {chromium}=require(process.argv[2]||'playwright'),{spawn}=require('child_process');
const fs=require('fs'),path=require('path'),assert=require('assert/strict');
const out=path.resolve(process.argv[3]);fs.mkdirSync(out,{recursive:true});
(async()=>{let browser;const server=spawn(process.execPath,['serve.cjs'],{cwd:path.join(__dirname,'..'),env:{...process.env,PORT:'8212'},windowsHide:true,stdio:'ignore'});
try{await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});const p=await browser.newPage({viewport:{width:1440,height:900}}),errors=[];p.on('pageerror',e=>errors.push(String(e)));
await p.goto('http://localhost:8212');await p.waitForSelector('#boot',{state:'detached'});
await p.evaluate(()=>{DT.UI.newGame('autochess','hyper','solo');const g=__digi.game;g.stage=3;g.round=4;g.roundType='pve';g.isPvE=true;g.phase='planning';g.carousel=null;
for(const q of g.players){q.offer=null;q.board={};q.bench.fill(null);q.orbs=[];q.hp=100;}
for(const [i,id]of ['ac_blackagumon','ac_bearmon','ac_kokuwamon'].entries())g.human.board[`${i+2},4`]=g.newUnit(id,0);
__digi.ui.timerOn=false;DT.UI.refresh();});
await p.waitForSelector('#cload',{state:'detached'});await p.waitForFunction(()=>[...__digi.renderer.views.values()].filter(v=>/^ac_(blackagumon|bearmon|kokuwamon)$/.test(v.line.id)).every(v=>v.hasModel===true&&v.mixer&&v.clips.special));
await p.screenshot({path:path.join(out,'planning.png')});
await p.evaluate(()=>{window.played=[];const V=[...__digi.renderer.views.values()][0].constructor,play=V.prototype.play;V.prototype.play=function(role){const ok=play.call(this,role);if(ok)played.push({line:this.line.id,role});return ok;};});
await p.click('#readyBtn');await p.waitForFunction(()=>!!__digi.shown&&__digi.game.phase==='combat');
// Give the fixture enough mana to exercise cast playback immediately, through normal combat events.
await p.evaluate(()=>{for(const u of __digi.shown.units)if(u.team===0)u.mana=u.maxMana;});
await p.waitForFunction(()=>played.some(e=>e.role==='attack'&&/^ac_(blackagumon|bearmon|kokuwamon)$/.test(e.line))&&played.some(e=>e.role==='special'&&/^ac_(blackagumon|bearmon|kokuwamon)$/.test(e.line)),null,{timeout:20000});
await p.screenshot({path:path.join(out,'combat.png')});const played=await p.evaluate(()=>window.played);assert.deepEqual(errors,[]);fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({played,errors},null,2));console.log('PASS repaired models play skeletal attack and special clips through real combat events');
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});
