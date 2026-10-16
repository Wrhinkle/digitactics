// Real UI checks for launch naming, credits and a playable renamed ruleset.
const {chromium}=require(process.argv[2]||'playwright');
const fs=require('fs'),path=require('path'),assert=require('assert/strict'),{spawn}=require('child_process');
const out=path.resolve(process.argv[3]);
(async()=>{
 fs.mkdirSync(out,{recursive:true});const server=spawn(process.execPath,['serve.cjs'],{cwd:path.join(__dirname,'..'),env:{...process.env,PORT:'8199'},windowsHide:true,stdio:'ignore'});let browser;
 const check=(v,s)=>{assert.ok(v,s);console.log('PASS '+s);};
 try{
  await new Promise(r=>setTimeout(r,600));browser=await chromium.launch({args:['--use-gl=angle','--use-angle=swiftshader']});
  const p=await browser.newPage({viewport:{width:1280,height:720}}),errors=[];p.on('pageerror',e=>errors.push(String(e)));
  await p.goto('http://localhost:8199');await p.waitForSelector('#boot',{state:'detached',timeout:60000});
  await p.evaluate(()=>DT.Client.go('play'));
  const options=await p.evaluate(()=>DT.SetPicker.options());
  check(options.find(x=>x.id==='underlords').name==='Undermons','Undermons is the display name without changing its saved identifier');
  check(options.find(x=>x.id==='underlords_ko').name==='Undermons Knockout','Knockout uses the new name');
  check(options.find(x=>x.id==='autochess').name==='Auto Chess','Auto Chess name is retained');
  for(const o of options){await p.evaluate(id=>DT.SetPicker.select(id),o.id);const text=await p.locator('.cpage.on').innerText();check(!/\b(?:Dota|Valve|Riot|Bandai|Namco|Drodo|Toei|Underlords)\b/i.test(text),'clean visible copy for '+o.name);}
  await p.evaluate(()=>DT.Client.credits());
  await p.waitForSelector('.cr-row');
  const rows=await p.locator('.cr-row').count();
  check(rows===await p.evaluate(()=>DT.AssetCredits.length)&&rows>400,'credits show every grouped bundled model source');
  const text=await p.locator('.credits').innerText();
  check(text.includes('Hallow')&&text.includes('Artzei')&&!text.includes('undefined'),'verified contributors are named without broken metadata');
  check(text.includes('Contributor not yet verified'),'unverified credits remain explicit');
  await p.screenshot({path:path.join(out,'credits-1280.png')});
  await p.setViewportSize({width:960,height:640});
  check(await p.locator('#cmodal .cm-box').evaluate(e=>{const r=e.getBoundingClientRect();return r.width>0&&r.left>=0&&r.right<=innerWidth&&r.top>=0&&r.bottom<=innerHeight;}),'credits fit compact desktop');
  await p.screenshot({path:path.join(out,'credits-960.png')});
  await p.click('#backBtn');await p.evaluate(()=>{DT.SetPicker.select('underlords');DT.Client.home();});await p.click('#startBtn');
  await p.waitForSelector('#cload',{state:'detached',timeout:30000});
  check(await p.evaluate(()=>DT.UI.game().set.name==='Undermons'),'renamed ruleset starts successfully');
  await p.click('#gameMenuBtn');await p.click('[data-game-pause]');
  check(await p.evaluate(()=>DT.UI.paused()),'solo pause remains available');
  await p.click('[data-game-leave]');await p.click('[data-game-confirm]');await p.waitForFunction(()=>!DT.UI.game());
  check(errors.length===0,'no browser exceptions: '+errors.join('; '));
 }finally{await browser?.close();server.kill();}
})().catch(e=>{console.error(e);process.exitCode=1;});
