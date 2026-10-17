// Multiplayer Duos -> Leave match -> Solo, without reloading either client.
// Steam is replaced only at its native boundary; both clients run the full game.
// node tests/multiplayer-to-solo.cjs <playwright> <evidence-dir>
const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {MockSteamHub}=require('./steam-mock-hub.cjs');
const out=path.resolve(process.argv[3]),root=path.join(__dirname,'..');fs.mkdirSync(out,{recursive:true});
const checks=[],check=(ok,name)=>{checks.push({ok:!!ok,name});console.log((ok?'PASS ':'FAIL ')+name);};
(async()=>{const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8207'},windowsHide:true,stdio:'ignore'});let browser;
try{
 await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});
 const hub=new MockSteamHub(),pages=[],errors=[];
 for(const [steamId,name] of [['76561190000000001','Host'],['76561190000000002','Partner']]){
  const ctx=await browser.newContext({viewport:{width:1440,height:900}});await hub.install(ctx,{steamId,name});
  const p=await ctx.newPage();p.on('pageerror',e=>errors.push(String(e)));pages.push(p);await p.goto('http://localhost:8207');await p.waitForSelector('#boot',{state:'detached'});
 }
 const [a,b]=pages;await a.click('#mpBtn');await a.selectOption('#mpSet','reckoning');await a.selectOption('#mpPace','normal');await a.selectOption('#mpFormat','duos');await a.selectOption('#mpPlan','60');await a.click('#mpCreate');await a.waitForSelector('#mpInvite');
 const lobby=[...hub.lobbies.values()][0];hub.emit(hub.users.get('76561190000000002'),'joinRequested',{lobbyId:lobby.id,friendId:'76561190000000001'});
 await b.waitForSelector('.mpseat.me');await a.waitForFunction(()=>document.querySelectorAll('.mpseat.taken').length===2);await a.click('#mpStart');
 for(const p of pages)await p.waitForFunction(()=>!!window.__digi?.session);
 // Identical deterministic fixtures put both clients at each real offer type without waiting through a whole match.
 for(const kind of ['armory']){
  for(const p of pages)await p.evaluate(kind=>{
   const g=__digi.game;g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.phaseAt=Date.now();g.planMs=300000;g.votes=[];
   for(const q of g.players){q.offer=null;q.items=[];q.augs=[];q.board={};q.bench.fill(null);}
   for(const q of g.players.filter(q=>q.isHuman)){const o={kind,tier:1};q.offer={...o,options:g.offerOptions(q,o),rerolls:1};}
   DT.UI.refresh();
  },kind);
  for(const [n,p] of pages.entries()){
   const before=await p.evaluate(()=>({slot:__digi.game.human.id,id:__digi.game.human.offer.options[0]}));
   await p.locator('[data-offer="0"]').click();await p.waitForTimeout(650);
   const states=await Promise.all(pages.map(q=>q.evaluate(s=>{const p=__digi.game.players[s];return {offer:p.offer,items:p.items,augs:p.augs};},before.slot)));
   check(states.every(s=>!s.offer),kind+' selection accepted for '+(n?'partner':'host'));
   check(states.every(s=>(kind==='augment'?s.augs:s.items).includes(before.id)),kind+' reward synchronized on both clients');
   if(states.some(s=>s.offer))await p.screenshot({path:path.join(out,kind+'-rejected-'+n+'.png')});
  }
 }

 // The partner leaves first while the host continues; then the host follows.
 for(const [who,p] of [['partner',b],['host',a]]){
  await p.click('#gameMenuBtn');await p.click('[data-game-leave]');await p.click('[data-game-confirm]');
  await p.waitForFunction(()=>!document.querySelector('#gameMenu').open&&!__digi.session&&!DT.Net.status().room);
  check(await p.evaluate(()=>!DT.Social.net&&!document.body.classList.contains('mp')&&!DT.UI.paused()),who+' clears multiplayer state on exit');
  await p.locator('button[data-page="play"]').first().click();
  await p.click('[data-mode-pick="solo"]');await p.click('#setPickSolo [data-set="reckoning"]');
  await p.click('#setPickSolo [data-format="solo"]');await p.click('#playConfirm');await p.click('#playSoloBtn');
  await p.waitForFunction(()=>!!__digi.game&&!__digi.session);await p.waitForSelector('#cload',{state:'detached'});
  check(await p.evaluate(()=>!__digi.game.duo&&__digi.game.setId==='reckoning'),who+' starts fresh Reckoning solo through Play UI');
  for(const [index,kind] of ['armory','rec_bless','rec_radiant','rec_tome','augment'].entries()){
   const chosen=await p.evaluate(({kind,index})=>{
    const g=__digi.game,q=g.human;__digi.ui.timerOn=false;g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.over=false;
    q.items=[];q.augs=[];q.globals=[];q.orbs=[];q.board={};q.bench.fill(null);q.alive=true;
    q.offer={kind,tier:1,options:g.offerOptions(q,{kind,tier:1}),rerolls:1};__digi.ui.peek=false;DT.UI.refresh();return q.offer.options[index%3];
   },{kind,index});assert.ok(chosen);
   await p.locator('[data-offer="'+index%3+'"]').click({delay:200});
   check(await p.evaluate(id=>{const q=__digi.game.human;return !q.offer&&[...q.items,...q.augs,...q.globals].includes(id)&&!__digi.session;},chosen),who+' solo '+kind+' works after Duos');
  }
 }
 check(!errors.length,'no browser exceptions: '+errors.join('; '));fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(checks,null,2));assert.ok(checks.every(c=>c.ok),'Transition regression failed');
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});
