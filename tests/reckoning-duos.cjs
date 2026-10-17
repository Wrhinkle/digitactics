// Real UI/transport regression for online offers and the combined-board item shelf.
// Steam is replaced only at its native boundary; both clients run the full game.
// node tests/reckoning-duos.cjs <playwright> <evidence-dir>
const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),fs=require('fs'),path=require('path'),assert=require('assert/strict');
const {MockSteamHub}=require('./steam-mock-hub.cjs');
const out=path.resolve(process.argv[3]),root=path.join(__dirname,'..');fs.mkdirSync(out,{recursive:true});
const checks=[],check=(ok,name)=>{checks.push({ok:!!ok,name});console.log((ok?'PASS ':'FAIL ')+name);};
(async()=>{const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8204'},windowsHide:true,stdio:'ignore'});let browser;
try{
 await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});
 const hub=new MockSteamHub(),pages=[],errors=[];
 for(const [steamId,name] of [['76561190000000001','Host'],['76561190000000002','Partner']]){
  const ctx=await browser.newContext({viewport:{width:1440,height:900}});await hub.install(ctx,{steamId,name});
  const p=await ctx.newPage();p.on('pageerror',e=>errors.push(String(e)));pages.push(p);await p.goto('http://localhost:8204');await p.waitForSelector('#boot',{state:'detached'});
 }
 const [a,b]=pages;await a.click('#mpBtn');await a.selectOption('#mpSet','reckoning');await a.selectOption('#mpPace','normal');await a.selectOption('#mpFormat','duos');await a.selectOption('#mpPlan','60');await a.click('#mpCreate');await a.waitForSelector('#mpInvite');
 const lobby=[...hub.lobbies.values()][0];hub.emit(hub.users.get('76561190000000002'),'joinRequested',{lobbyId:lobby.id,friendId:'76561190000000001'});
 await b.waitForSelector('.mpseat.me');await a.waitForFunction(()=>document.querySelectorAll('.mpseat.taken').length===2);await a.click('#mpStart');
 for(const p of pages)await p.waitForFunction(()=>!!window.__digi?.session);
 // Identical deterministic fixtures put both clients at each real offer type without waiting through a whole match.
 for(const kind of ['armory','rec_bless','rec_radiant','rec_tome','augment']){
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
 // Separate local match exercises actual planning -> combined combat -> planning UI transitions.
 const p=await browser.newPage({viewport:{width:1440,height:900}});p.on('pageerror',e=>errors.push(String(e)));await p.goto('http://localhost:8204');await p.waitForSelector('#boot',{state:'detached'});
 await p.evaluate(()=>{
  DT.UI.newGame('reckoning','normal','duos');const g=__digi.game;g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.isPvE=false;
  for(const q of g.players){q.offer=null;q.board={};q.bench.fill(null);q.hp=100;q.alive=true;q.board['3,4']=g.newUnit('rec_dorumon',1);}
  g.human.items=['sword','rod','vest'];g.duoPairs=[[g.teams[0],g.teams[1],false],[g.teams[2],g.teams[3],false]];__digi.ui.timerOn=false;DT.UI.refresh();
 });await p.waitForTimeout(2500);
 check(await p.evaluate(()=>{const v=[...__digi.renderer.views.values()].find(v=>v.line.id==='rec_dorumon'&&v.stage===1);return v?.hasModel===true&&v.materials.every(m=>!m.transparent&&m.opacity===1&&m.alphaTest===0);}), 'Dorugamon body renders opaque without the cutout that removed its fur');
 await p.screenshot({path:path.join(out,'dorugamon-planning.png')});
 check(await p.evaluate(()=>__digi.renderer.itemTokens.filter(x=>x.visible).length===3),'planning shows inventory');
 await p.click('#readyBtn');await p.waitForFunction(()=>__digi.shown?.cols===14);await p.evaluate(()=>__digi.ui.speed=.02);await p.waitForTimeout(150);
 check(await p.evaluate(()=>__digi.renderer.itemTokens.every(x=>!x.visible)&&__digi.renderer.itemPeds.every(x=>!x.visible)),'combined fight hides item tokens and pedestals');
 check(await p.evaluate(()=>{const r=__digi.renderer,p=r.screenOfItemSlot(0);return r.itemSlotAt(p.x,p.y,3)===-1;}),'hidden combat items cannot intercept board clicks');
 await p.screenshot({path:path.join(out,'duos-combat.png')});await p.click('#skipBtn');await p.waitForFunction(()=>__digi.game.phase==='planning',null,{timeout:15000});await p.waitForTimeout(150);
 check(await p.evaluate(()=>__digi.renderer.itemTokens.filter(x=>x.visible).length>=3&&__digi.renderer.itemPeds.every(x=>x.visible)),'inventory restored after combat');
 check(!errors.length,'no browser exceptions: '+errors.join('; '));fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(checks,null,2));assert.ok(checks.every(c=>c.ok),'Regression checks failed');
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});
