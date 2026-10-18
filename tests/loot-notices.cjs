const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),path=require('path'),fs=require('fs'),assert=require('assert/strict');
const root=path.join(__dirname,'..'),out=path.resolve(process.argv[3]||'native/artifacts/loot-notice-browser');
fs.mkdirSync(out,{recursive:true});
const checks=[];function check(ok,name){checks.push({ok:!!ok,name});console.log((ok?'PASS ':'FAIL ')+name);assert.ok(ok,name);}
(async()=>{
 const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8217'},windowsHide:true,stdio:'ignore'});let browser;
 try{
  browser=await chromium.launch({channel:'msedge',args:['--use-angle=d3d11']});
  const page=await browser.newPage({viewport:{width:1440,height:900}}),errors=[];page.on('pageerror',e=>{errors.push(String(e));console.error('Browser exception:',e);});
  await require('./live-source-bundle.cjs')(page);
  await page.addInitScript(()=>{if(!localStorage.getItem('dt.mysteryLoot.v1'))localStorage.setItem('dt.mysteryLoot.v1',JSON.stringify({day:'2000-01-01',dayProgress:1,boxMisses:31,keyMisses:39}));});
  await page.goto('http://localhost:8217/',{waitUntil:'domcontentloaded'});await page.waitForSelector('#boot',{state:'detached',timeout:60000});
  check(await page.locator('#lootAwardNotice').isHidden(),'startup does not announce saved inventory');
  const drops=await page.evaluate(()=>{const human={id:0};window.noticeRound={human,setId:'fates',seed:143,duo:false,stage:2,round:1,results:[{match:{a:human,b:{id:1}}}]};return DT.MysteryLoot.roundEnd(noticeRound);});
  check(JSON.stringify(drops)===JSON.stringify(['Daybreak Box','Cipher Box','Cipher Key']),'actual round transaction earns second-day box plus guaranteed box/key pity');
  await page.waitForSelector('#lootAwardNotice:not([hidden])');await page.waitForTimeout(300);
  const panel=page.locator('#lootAwardNotice');
  check(await panel.locator('.loot-award-name').allTextContents().then(a=>JSON.stringify(a)===JSON.stringify(['Daybreak Box ×1','Cipher Box ×1','Cipher Key ×1'])),'all three official earned names and counts are explicit');
  check(await panel.locator('.loot-award-source').allTextContents().then(a=>a[0].includes('2 active days')&&a[1].includes('2-1 complete')&&a[2].includes('2-1 complete')),'activity and round sources are visible');
  check(await panel.locator('svg').count()===3,'each earned kind has a chest or key icon');
  check(await panel.locator('[role=status][aria-live=polite][aria-atomic=true]').count()===1,'screen reader status is polite and atomic');
  check(await page.evaluate(()=>document.activeElement?.closest('#lootAwardNotice')===null),'reward appearance does not steal keyboard focus');
  for(const [name,width,height] of [['normal',1440,900],['minimum',1100,760],['short',1342,670],['mobile',540,800]]){
   await page.setViewportSize({width,height});await panel.hover();await page.waitForTimeout(100);
   const b=await panel.boundingBox();check(b.x>=0&&b.y>=0&&b.x+b.width<=width&&b.y+b.height<=height,'bounded '+name+' lower-third');
   check(await panel.locator('.loot-award-source').evaluateAll(nodes=>nodes.every(n=>parseFloat(getComputedStyle(n).fontSize)>=16&&n.scrollWidth<=n.clientWidth)),name+' body text remains at least16px without horizontal overflow');
   await page.screenshot({path:path.join(out,name+'.png')});
  }
  await page.setViewportSize({width:1100,height:760});await panel.hover();const progress=await panel.evaluate(p=>p.style.getPropertyValue('--remaining'));
  await page.waitForTimeout(10500);check(await panel.isVisible()&&await panel.evaluate(p=>p.style.getPropertyValue('--remaining'))===progress,'hover pauses ten-second timeout');
  check(await page.evaluate(()=>DT.MysteryLoot.roundEnd(noticeRound).length===0),'duplicate round settlement emits no second award');
  await panel.getByRole('button',{name:'Dismiss loot earned notification'}).click();await page.waitForTimeout(40);check(await panel.isHidden(),'Dismiss removes the only notice');
  // Direct presentation fixtures test grouping without granting inventory or exposing hidden cosmetics.
  await page.evaluate(()=>{DT.LootNotices.earned({id:'view1',round:'2-2',drops:['Cipher Box','Cipher Key','Unopened Secret Reward']});DT.LootNotices.earned({id:'view2',round:'2-3',drops:['Cipher Box','Cipher Key']});DT.LootNotices.earned({id:'view3',round:'2-4',drops:['Cipher Box','Cipher Key']});});
  await page.waitForTimeout(100);check(!(await panel.innerText()).includes('Unopened Secret'),'unknown secret contents cannot appear in notices');
  await panel.getByRole('button').click();await page.waitForTimeout(100);
  check((await panel.locator('.loot-award-name').allTextContents()).join('|')==='Cipher Box ×2|Cipher Key ×2','later rewards coalesce with conserved item counts');
  check((await panel.locator('.loot-award-source').allTextContents()).every(x=>x.includes('2 rounds, latest 2-4')),'coalesced sources identify number and latest round');
  await page.emulateMedia({reducedMotion:'reduce'});check(await panel.evaluate(p=>getComputedStyle(p).animationName)==='none','system reduced-motion preference suppresses entrance movement');
  await page.screenshot({path:path.join(out,'grouped-reduced.png')});
  await panel.getByRole('button').focus();await page.keyboard.press('Escape');await page.waitForTimeout(80);check(await panel.isHidden(),'focused Escape dismisses reward');
  const saved=await page.evaluate(()=>JSON.stringify(DT.MysteryLoot.inventory()));await page.reload();await page.waitForSelector('#boot',{state:'detached'});
  check(await panel.isHidden()&&await page.evaluate(()=>JSON.stringify(DT.MysteryLoot.inventory()))===saved,'actual reload preserves rewards without duplicate notification');
  check(await page.evaluate(()=>{const old=Storage.prototype.setItem,oldRoll=crypto.getRandomValues;Storage.prototype.setItem=function(){throw new Error('fixture write failure');};crypto.getRandomValues=a=>{a.fill(0);return a;};try{const h={id:0};return DT.MysteryLoot.roundEnd({human:h,setId:'fates',seed:144,stage:2,round:1,results:[{match:{a:h}}]}).length===0;}finally{Storage.prototype.setItem=old;crypto.getRandomValues=oldRoll;}}),'failed persistent write grants and announces nothing');
  await page.waitForTimeout(80);check(await panel.isHidden(),'failed write leaves notification hidden');
  check(!errors.length,'no browser exceptions');
 }finally{await browser?.close();server.kill();fs.writeFileSync(path.join(out,'result.json'),JSON.stringify({checks},null,2));}
})().catch(e=>{console.error(e);process.exitCode=1;});
