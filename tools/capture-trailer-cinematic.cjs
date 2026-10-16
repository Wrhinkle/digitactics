// Records actual rendered game footage from isolated, staged showcase matches.
// No production saves, accounts or public lobbies are touched. Fixtures choose boards; the engine fights them.
// node tools/capture-trailer-cinematic.cjs <playwright-package> <output-dir>
const {chromium}=require(process.argv[2]||'playwright');
const {spawn}=require('child_process'),fs=require('fs'),path=require('path');
const out=path.resolve(process.argv[3]),root=path.join(__dirname,'..');
fs.mkdirSync(out,{recursive:true});
(async()=>{
 const server=spawn(process.execPath,['serve.cjs'],{cwd:root,env:{...process.env,PORT:'8198'},windowsHide:true,stdio:'ignore'});
 let browser;
 try{
  await new Promise(r=>setTimeout(r,600));
  browser=await chromium.launch({args:['--use-angle=d3d11']});
  const ctx=await browser.newContext({viewport:{width:1920,height:1080}});
  const page=await ctx.newPage(),clips=[],errors=[],actions=[];
  page.setDefaultTimeout(30000);
  let cinematic=false;
  page.on('pageerror',e=>errors.push(String(e)));
  const advance=ms=>page.clock.runFor(ms);
  const click=selector=>page.locator(selector).evaluate(el=>el.click());
  const record=async(name,seconds,onFrame=async()=>{})=>{
    const dir=path.join(out,name);fs.mkdirSync(dir,{recursive:true});
    const timings=[];
    for(let i=0;i<Math.round(seconds*30);i++){
      await onFrame(i);
      await advance(Math.floor((i+1)*1000/30)-Math.floor(i*1000/30));
      timings.push(await page.evaluate(()=>performance.now()));
      await (cinematic?page.locator('#board'):page).screenshot({path:path.join(dir,String(i).padStart(5,'0')+'.png')});
    }
    clips.push({name,frames:timings.length,fps:30,timings});console.log('Recorded',name,timings.length,'frames');fs.writeFileSync(path.join(out,'progress.json'),JSON.stringify({clips,actions,errors},null,2));
  };
  const hud=async on=>{
    cinematic=!on;
    await page.evaluate(on=>{
      document.getElementById('capture-layout')?.remove();
      if(!on){const s=document.createElement('style');s.id='capture-layout';s.textContent='#arena{position:fixed!important;inset:0!important;width:100vw!important;height:100vh!important;z-index:9999!important}#arena>:not(canvas){visibility:hidden!important}';document.head.appendChild(s);}
      __digi.renderer.fit();if(on)__digi.renderer.resetView();
    },on);
  };
  const camera=async(id,t)=>page.evaluate(({id,t})=>{
    const r=__digi.renderer,v=r.view,e=t*t*(3-2*t);
    if(id==='rookie'){v.target.set(-3.9,.8,.75);v.yaw=Math.PI+.45-.3*e;v.pitch=.4;v.zoom=.25-.025*e;}
    if(id==='evolve'){v.target.set(-3.9,.9,.75);v.yaw=Math.PI+.15-.25*e;v.pitch=.4;v.zoom=.26+.045*e;}
    if(id==='solo'){v.target.set(0,.65,-.6);v.yaw=.48-.34*e;v.pitch=.53+.05*e;v.zoom=.63-.07*e;}
    if(id==='duos'){v.target.set(-2+4*e,.65,-.5);v.yaw=-.28+.42*e;v.pitch=.54+.12*e;v.zoom=.62+.15*e;}
    r.applyView();
  },{id,t});
  const pointer=async(slot,pressed=false)=>page.evaluate(({slot,pressed})=>{
    let el=document.getElementById('capture-pointer');
    if(!el){el=document.createElement('div');el.id='capture-pointer';el.style.cssText='position:fixed;z-index:20000;pointer-events:none;width:28px;height:36px;filter:drop-shadow(0 2px 4px #061323)';el.innerHTML='<svg viewBox="0 0 28 36"><path d="M2 2L3 28L10 22L16 33L22 30L16 20L26 19Z" fill="white" stroke="#061323" stroke-width="2"/></svg>';document.body.appendChild(el);}
    const card=document.querySelector(`#shop .card[data-slot="${slot}"]`),box=card?.getBoundingClientRect();
    if(box){el.style.left=(box.left+box.width*.64)+'px';el.style.top=(box.top+box.height*.43)+'px';}
    el.style.transform=pressed?'scale(.82)':'scale(1)';
    el.style.filter=pressed?'drop-shadow(0 0 16px #51e6ff)':'drop-shadow(0 2px 4px #061323)';
    document.querySelectorAll('#shop .card').forEach(c=>c.style.outline='');
    if(pressed&&card)card.style.outline='3px solid #51e6ff';
  },{slot,pressed});
  const waitPhase=async phase=>{for(let i=0;i<600;i++){if(await page.evaluate(p=>DT.KillScreens.active?.phase===p,phase))return;await advance(50);await page.waitForTimeout(10);}throw Error('Missing cinematic phase '+phase);};
  await page.goto('http://localhost:8198');await page.waitForSelector('#boot',{state:'detached',timeout:60000});
  console.log('Loaded game');
  const epoch=Date.now();await page.clock.install({time:epoch});await page.clock.pauseAt(epoch+1000);
  console.log('Clock paused');
  const renderer=await page.evaluate(()=>{const gl=document.createElement('canvas').getContext('webgl2'),ext=gl.getExtension('WEBGL_debug_renderer_info');return gl.getParameter(ext.UNMASKED_RENDERER_WEBGL);});console.log(renderer);
  if(process.argv.includes('--farm-only')){
    await page.evaluate(()=>{DT.FarmUI.open('tokomon');Object.assign(DT.Farm._save().pets.tokomon,{hunger:45,happy:55,energy:90,sick:false,tantrum:0,nap:false});DT.FarmUI.select('tokomon');});
    await page.waitForTimeout(2500);await advance(1200);
    const before=await page.evaluate(()=>DT.Farm.card('tokomon'));
    await page.screenshot({path:path.join(out,'farm-probe.png')});
    if(process.argv.includes('--probe'))return;
    await record('farm',3.5,async i=>{
      if(i===10)await click('#dtFarm [data-fa="feed"]');
      if(i===24)await click('#dtFarm [data-food="meat"]');
      if(i===75)await click('#dtFarm [data-fa="praise"]');
    });
    const after=await page.evaluate(()=>DT.Farm.card('tokomon'));
    if(after.hunger<=before.hunger||after.happy<=before.happy)throw Error('Farm feed/praise did not increase care stats');
    fs.writeFileSync(path.join(out,'farm-capture.json'),JSON.stringify({clips,actions:[{action:'feed-and-praise',before,after}],errors,renderer},null,2));
    if(errors.length)throw Error(errors.join('\n'));
    return;
  }
  await page.evaluate(()=>{DT.SetPicker.select('faction-wars');DT.SetPicker.setPace('normal');DT.SetPicker.setFormat('solo');DT.Client.home();});
  await click('#startBtn');await advance(3000);await page.waitForSelector('#cload',{state:'detached',timeout:30000});
  await click('#timerBtn');await page.locator('.choice[data-carousel]:not(.taken)').first().evaluate(el=>el.click());await click('#carouselGo');
  const setup=async duos=>page.evaluate(duos=>{
    if(duos)DT.UI.newGame('faction-wars','normal','duos');
    const g=__digi.game;g.stage=4;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.isPvE=false;
    for(const p of g.players){p.board={};p.bench.fill(null);p.offer=null;p.hp=100;p.alive=true;p.level=8;}
    const rosters=[['agumon','gabumon','patamon','guilmon','renamon'],['veemon','terriermon','hawkmon','palmon','tentomon'],['beelzemon','myotismon','machinedramon','keramon','demidevimon'],['imperialdramon','gaomon','gomamon','candlemon','lalamon']];
    for(let n=0;n<4;n++){const p=g.players[n];rosters[n].filter(id=>DT.LINE[id]).forEach((id,i)=>p.board[`${1+i},${i<3?4:6}`]=g.newUnit(id,1));}
    g.pairs=[[g.players[0],g.players[2],false],[g.players[1],g.players[3],false],[g.players[4],g.players[5],false],[g.players[6],g.players[7],false]];
    if(duos)g.duoPairs=[[g.teams[0],g.teams[1],false],[g.teams[2],g.teams[3],false]];
    __digi.ui.timerOn=false;__digi.ui.speed=1;DT.UI.refresh();
  },duos);
  await setup(false);await page.waitForTimeout(4000);await advance(2500);
  await page.evaluate(()=>{
    const g=__digi.game,p=g.human;
    p.board['1,4']=g.newUnit('agumon',0);p.gold=30;
    p.shop=['agumon','agumon','gabumon','patamon','renamon'];p.shopMeta=null;
    DT.UI.refresh();
  });
  await page.waitForTimeout(1000);await advance(2000);
  const state=()=>page.evaluate(()=>({pace:__digi.game.pace,duo:!!__digi.game.duo,gold:__digi.game.human.gold,units:__digi.game.human.allUnits().map(u=>({line:u.line,stage:u.stage})),shop:__digi.game.human.shop.slice()}));
  const purchase=async slot=>{
    const before=await state();await click(`#shop .card[data-slot="${slot}"]`);const after=await state();
    if(after.gold>=before.gold||after.shop[slot]!==null)throw Error('Purchase did not execute');
    actions.push({action:'buy',slot,before,after});
  };
  await page.evaluate(()=>{__digi.ui.previewSide=null;});
  await hud(false);await camera('rookie',0);await advance(50);
  if(process.argv.includes('--probe')){
    await page.locator('#board').screenshot({path:path.join(out,'rookie-probe.png')});
    await page.evaluate(()=>{__digi.game.human.board['1,4']=__digi.game.newUnit('agumon',1);DT.UI.refresh();});
    await camera('evolve',.5);await advance(100);await page.waitForTimeout(1000);await advance(50);
    await page.locator('#board').screenshot({path:path.join(out,'evolve-probe.png')});return;
  }
  await record('rookie',1.8,i=>camera('rookie',i/53));
  await hud(true);await advance(50);
  await record('buy',3.6,async i=>{
    const slot=i<38?2:i<76?0:1;await pointer(slot,[15,50,91].some(n=>i>=n&&i<n+8));
    if(i===15)await purchase(2);if(i===50)await purchase(0);if(i===91)await purchase(1);
  });
  await page.evaluate(()=>document.getElementById('capture-pointer')?.remove());
  const evolved=await state();
  if(evolved.pace!=='normal'||evolved.duo||!evolved.units.some(u=>u.line==='agumon'&&u.stage===1)||evolved.units.some(u=>u.line==='agumon'&&u.stage===0))throw Error('Expected real three-copy Agumon evolution in Normal solo');
  await hud(false);await camera('evolve',0);await record('evolve',2.4,i=>camera('evolve',i/71));
  await hud(true);await click('#readyBtn');await advance(250);await record('solo-ui',2);
  await hud(false);await record('solo-orbit',3.2,i=>camera('solo',i/95));
  await hud(true);await setup(true);await page.waitForTimeout(4000);await advance(3000);await click('#readyBtn');await advance(250);await record('duos-ui',1.5);
  await hud(false);await record('duos-orbit',3,i=>camera('duos',i/89));
  actions.push({action:'duos-combat',state:await state()});
  await hud(true);
  // A real elimination supplies its final-hit record to the game's cinematic.
  await page.evaluate(()=>{
    DT.KillScreens.clear();DT.UI.newGame('faction-wars','hyper','solo');const g=__digi.game;
    g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.isPvE=false;
    for(const p of g.players){p.board={};p.bench.fill(null);p.offer=null;p.hp=20;p.alive=true;}
    ['agumon','gabumon','patamon','veemon','guilmon','renamon'].forEach((id,i)=>{g.players[0].board[`${i},4`]=g.newUnit(id,2);});
    g.players[1].board={'3,4':g.newUnit('demidevimon',0)};g.players[1].hp=1;
    g.pairs=[[g.players[0],g.players[1],false],[g.players[2],g.players[3],false],[g.players[4],g.players[5],false],[g.players[6],g.players[7],false]];
    __digi.ui.timerOn=false;DT.UI.refresh();
  });
  await page.waitForTimeout(4000);await advance(1000);await click('#readyBtn');await click('#skipBtn');
  await waitPhase('attack');await page.waitForTimeout(2000);
  await record('final-attack',2);await waitPhase('dance');await page.waitForTimeout(2000);await record('victory-dance',2);
  fs.writeFileSync(path.join(out,'capture.json'),JSON.stringify({clips,actions,errors,renderer,note:'Staged local gameplay, captured at fixed 30 fps simulation intervals; wall-clock capture cost cannot drop animation frames. No fake multiplayer session.'},null,2));
  console.log('RESULT: '+JSON.stringify({status:'succeeded',stage:'capture',artifacts:[path.join(out,'capture.json')]}));
  await ctx.close();
  if(errors.length)throw Error(errors.join('\n'));
 }finally{await browser?.close();server.kill();}
})().catch(e=>{console.error(e);process.exitCode=1;});
