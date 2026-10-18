// Full-client flows: retro purchase/equip/play, natural round reward, reveal, persistence and secret previews.
const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),fs=require('fs'),path=require('path'),assert=require('assert/strict');
const out=path.resolve(process.argv[3]),root=path.join(__dirname,'..');fs.mkdirSync(out,{recursive:true});
const checks=[],check=(ok,name)=>{checks.push({ok:!!ok,name});console.log((ok?'PASS ':'FAIL ')+name);};
(async()=>{const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8215'},windowsHide:true,stdio:'ignore'});let browser;
try{
 await new Promise(r=>setTimeout(r,500));browser=await chromium.launch({args:['--use-angle=d3d11']});const page=await browser.newPage({viewport:{width:1440,height:900}}),errors=[];
 page.on('pageerror',e=>errors.push(String(e)));
 await page.addInitScript(()=>{if(!localStorage.getItem('dt.mysteryLoot.v1'))localStorage.setItem('dt.mysteryLoot.v1',JSON.stringify({daily:0,vault:0,keys:0,day:'',dayProgress:1,games:{},boxMisses:31,keyMisses:39,pending:null}));});
 await page.goto('http://localhost:8215/?bits=5000');await page.waitForSelector('#boot',{state:'detached'});
 const pools=await page.evaluate(()=>Object.fromEntries(['daily','vault'].map(pool=>[pool,Object.fromEntries(DT.Cosmetics.KINDS.map(k=>[k,DT.Cosmetics.list(k).filter(it=>it.lootPool===pool).length]))])));
 for(const [pool,kinds] of Object.entries(pools))check(Object.entries(kinds).every(([k,n])=>n===(k==='board'?5:20)),pool+' contains 20 per category and 5 boards');
 check(await page.evaluate(()=>DT.Cosmetics.KINDS.every(k=>DT.Cosmetics.visible(k).every(it=>!it.secret))), 'all undiscovered rewards hidden');
 await page.evaluate(()=>DT.Client.go('collection'));await page.click('[data-ctab="emote"]');await page.click('[data-retro-emotes]');
 check(await page.locator('.ccard').count()===9,'Digimon World 1 shortcut finds nine emotes');await page.screenshot({path:path.join(out,'retro-collection.png')});
 await page.evaluate(()=>DT.Shop.open('emote'));await page.click('[data-buy="retro_hug"]');await page.click('[data-confirm]');
 check(await page.evaluate(()=>DT.Cosmetics.owns('emote','retro_hug')),'purchase Monzaemon Hug from real store');
 await page.evaluate(()=>DT.Shop.close());await page.evaluate(()=>DT.Client.go('collection'));await page.click('[data-csel="retro_hug"]');await page.click('[data-cslot="0"]');
 check(await page.evaluate(()=>DT.Cosmetics.equipped('emote')[0]==='retro_hug'),'equip retro emote through Collection');
 await page.locator('button[data-page="play"]').first().click();await page.click('[data-mode-pick="solo"]');await page.click('#setPickSolo [data-set="reckoning"]');await page.click('#setPickSolo [data-format="solo"]');await page.click('#playConfirm');await page.click('#playSoloBtn');
 await page.waitForSelector('#cload',{state:'detached'});await page.evaluate(()=>{__digi.ui.timerOn=false;});
 const carousel=page.locator('.choice[data-carousel]:not(.taken)');if(await carousel.count()){await carousel.first().click();await page.click('#carouselGo');}
 await page.keyboard.press('t');await page.click('#emoteWheel [data-id="retro_hug"]');await page.waitForFunction(()=>DT.Emotes.pops().some(p=>p.ready&&p.inScene));
 check(true,'equipped Monzaemon Hug spawns 3D World model in match');await page.waitForTimeout(700);await page.screenshot({path:path.join(out,'retro-in-match.png')});
 await page.click('#readyBtn');await page.evaluate(()=>{__digi.ui.speed=16;__digi.renderer.speed=16;});
 await page.waitForFunction(()=>DT.MysteryLoot.inventory().daily===1,null,{timeout:60000});check(true,'natural completed round finishes seeded second active day and grants its box');
 check(await page.evaluate(()=>{const before=JSON.stringify(DT.MysteryLoot.inventory());DT.MysteryLoot.roundEnd(__digi.game);return before===JSON.stringify(DT.MysteryLoot.inventory());}),'same completed round cannot award twice');
 // Fixture the next eligible round, then fight it through the real combat/UI path.
 await page.evaluate(()=>{const g=__digi.game;g.stage=2;g.round=1;g.phase='planning';g.carousel=null;g.roundType='pvp';g.human.offer=null;__digi.ui.timerOn=false;DT.UI.refresh();});
 await page.click('#readyBtn');await page.waitForFunction(()=>DT.MysteryLoot.inventory().vault===1&&DT.MysteryLoot.inventory().keys===1,null,{timeout:60000});
 check(await page.evaluate(()=>DT.MysteryLoot.inventory().daily===1),'eligible combat drops a box and key at pity without a second daily box');
 await page.click('#gameMenuBtn');await page.click('[data-game-leave]');await page.click('[data-game-confirm]');await page.evaluate(()=>DT.Client.go('loot'));
 await page.click('[data-mystery-open="daily"]');await page.waitForSelector('.mystery-reveal[open]');
 const unlocked=await page.evaluate(()=>DT.Cosmetics.KINDS.flatMap(kind=>DT.Cosmetics.list(kind).filter(it=>it.secret&&DT.Cosmetics.owns(kind,it.id)).map(it=>({kind,id:it.id}))));
 check(unlocked.length===1,'opening daily box discovers exactly one reward');await page.screenshot({path:path.join(out,'first-reveal.png')});await page.click('.mystery-reveal [data-equip]');
 check(await page.evaluate(({kind,id})=>{const x=DT.Cosmetics.equipped(kind);return Array.isArray(x)?x.includes(id):x===id;},unlocked[0]),'discovered reward equips');
 await page.reload();await page.waitForSelector('#boot',{state:'detached'});check(await page.evaluate(({kind,id})=>DT.Cosmetics.owns(kind,id)&&DT.MysteryLoot.inventory().daily===0,unlocked[0]),'reward and consumed box persist across reload');
 // Fixture: one earned locked box, no key. UI must block opening; add an earned key and reload.
 await page.evaluate(()=>{const s=JSON.parse(localStorage.getItem('dt.mysteryLoot.v1'));s.vault=1;s.keys=0;localStorage.setItem('dt.mysteryLoot.v1',JSON.stringify(s));});await page.reload();await page.waitForSelector('#boot',{state:'detached'});await page.evaluate(()=>DT.Client.go('loot'));
 check(await page.locator('[data-mystery-open="vault"]').isDisabled(),'locked box requires a key');
 await page.evaluate(()=>{const s=JSON.parse(localStorage.getItem('dt.mysteryLoot.v1'));s.keys=1;localStorage.setItem('dt.mysteryLoot.v1',JSON.stringify(s));});await page.reload();await page.waitForSelector('#boot',{state:'detached'});await page.evaluate(()=>DT.Client.go('loot'));await page.click('[data-mystery-open="vault"]');
 check(await page.evaluate(()=>{const v=DT.MysteryLoot.inventory();return v.vault===0&&v.keys===0;}),'opening Cipher box consumes exactly one box and key');await page.click('.mystery-reveal [data-close]');
 // Render every new board and companion through production renderers, not placeholder cards.
 const assets=await page.evaluate(async()=>{const result=[];for(const it of DT.Cosmetics.list('board').filter(x=>x.secret)){try{result.push({id:it.id,ok:!!await DT.Boards.previewUrl(it.id)});}catch(e){result.push({id:it.id,ok:false,error:String(e)});}}
   for(const it of DT.Cosmetics.list('legend').filter(x=>x.secret)){try{const t=await DT.Legends.modelFor(it.id);result.push({id:it.id,ok:!!t?.root&&DT.LegendEvo.tree(it.id).edges.length>0});}catch(e){result.push({id:it.id,ok:false,error:String(e)});}}return result;});
 for(const a of assets)check(a.ok,'renderer loads '+a.id+(a.error||''));
 const boards=await page.evaluate(async()=>Promise.all(DT.Cosmetics.list('board').filter(x=>x.secret).map(async it=>({id:it.id,url:await DT.Boards.previewUrl(it.id)}))));
 for(const b of boards)if(b.url.startsWith('data:'))fs.writeFileSync(path.join(out,b.id+'.png'),Buffer.from(b.url.split(',')[1],'base64'));
 await page.setViewportSize({width:760,height:820});await page.evaluate(()=>DT.Client.go('loot'));await page.waitForTimeout(500);await page.screenshot({path:path.join(out,'loot-compact.png')});
 check(!errors.length,'no browser exceptions: '+errors.join('; '));fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({checks,pools,errors},null,2));assert.ok(checks.every(c=>c.ok),'Loot flow failed');
}finally{await browser?.close();server.kill();}})().catch(e=>{console.error(e);process.exitCode=1;});
