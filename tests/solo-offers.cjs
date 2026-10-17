// Real solo UI choices, including all three card positions and pause/resume.
const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),fs=require('fs'),path=require('path'),assert=require('assert/strict');
const out=path.resolve(process.argv[3]);fs.mkdirSync(out,{recursive:true});
(async()=>{let browser;const server=spawn(process.execPath,['serve.cjs'],{cwd:path.join(__dirname,'..'),env:{...process.env,PORT:'8206'},windowsHide:true,stdio:'ignore'});
try{
 await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});
 const p=await browser.newPage({viewport:{width:1280,height:720}}),errors=[],results=[];p.on('pageerror',e=>errors.push(String(e)));
 await p.goto('http://localhost:8206');await p.waitForSelector('#boot',{state:'detached'});
 for(const set of ['reckoning','autochess'])for(const pace of ['hyper','normal']){
  await p.evaluate(({set,pace})=>{DT.UI.newGame(set,pace,'solo');__digi.ui.timerOn=false;}, {set,pace});await p.waitForSelector('#cload',{state:'detached'});
  for(const kind of set==='reckoning'?['armory','rec_bless','rec_radiant','rec_tome','augment']:['ac_loot','ac_relic'])for(let index=0;index<3;index++){
   const chosen=await p.evaluate(({kind,index})=>{
    const g=__digi.game,q=g.human;g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.over=false;
    q.items=[];q.augs=[];q.globals=[];q.orbs=[];q.board={};q.bench.fill(null);q.alive=true;
    if(kind==='ac_loot')q.mech.loot=['ac_attack_chip','ac_defense_chip','ac_speed_chip'];
    q.offer={kind,tier:1,options:g.offerOptions(q,{kind,tier:1}),rerolls:1};__digi.ui.peek=false;DT.UI.refresh();return q.offer.options[index];
   },{kind,index});assert.ok(chosen);
   if(index===1){await p.click('#gameMenuBtn');await p.click('[data-game-pause]');await p.click('[data-game-return]');await p.waitForFunction(()=>!DT.UI.paused());}
   await p.locator(`[data-offer="${index}"]`).click({delay:200});
   const state=await p.evaluate(()=>{const q=__digi.game.human;return {offer:q.offer,items:q.items,augs:q.augs,globals:q.globals};});
   const ok=!state.offer&&[...state.items,...state.augs,...state.globals].includes(chosen);
   results.push({set,pace,kind,index,ok});console.log(`${ok?'PASS':'FAIL'} ${set} ${pace} ${kind} card ${index+1}`);
   if(!ok){await p.screenshot({path:path.join(out,'failed.png')});fs.writeFileSync(path.join(out,'failed-state.json'),JSON.stringify(state));}
   assert.ok(ok,'Solo offer selection failed');
  }
 }
 assert.deepEqual(errors,[]);fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(results,null,2));
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});
