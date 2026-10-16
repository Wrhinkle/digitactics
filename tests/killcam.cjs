// Real combat -> elimination -> final attack -> victory dance, in an isolated local browser.
const path=require('path');
const fs=require('fs');
const {spawn}=require('child_process');
const {chromium}=require(process.argv[2]||'playwright');
const out=process.argv[3]||__dirname, port=Number(process.argv[4]||8197);
fs.mkdirSync(out,{recursive:true});
let failed=0;
const check=(v,s)=>{console.log((v?'ok  ':'FAIL ')+s); if(!v)failed++;};
(async()=>{
  const server=spawn(process.execPath,[path.join(__dirname,'..','serve.cjs')],{env:{...process.env,PORT:port},windowsHide:true,stdio:'ignore'});
  let browser;
  try{
    await new Promise(r=>setTimeout(r,600));
    browser=await chromium.launch({args:['--use-gl=angle','--use-angle=swiftshader']});
    const page=await browser.newPage({viewport:{width:1500,height:920}}),errors=[];
    page.on('pageerror',e=>errors.push(String(e)));
    await page.goto(`http://localhost:${port}/`);
    await page.waitForFunction(()=>window.DT?.KillScreens);
    await page.click('#startBtn');
    await page.click('#timerBtn');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    // Set up an isolated elimination round; combat, damage and cinematic all run through the application.
    await page.evaluate(()=>{
      const g=__digi.game; g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.isPvE=false;
      for(const p of g.players){p.board={};p.bench.fill(null);p.offer=null;p.hp=20;p.alive=true;}
      const strong=DT.SHOP_LINES.find(l=>l.tier===5),weak=DT.SHOP_LINES.find(l=>l.tier===1);
      g.human.board={'3,4':g.newUnit(strong.id,2),'2,4':g.newUnit(strong.id,2),'4,4':g.newUnit(strong.id,2)};
      g.players[1].board={'3,4':g.newUnit(weak.id,0)};g.players[1].hp=1;
      g.pairs=[[g.players[0],g.players[1],false],[g.players[2],g.players[3],false],[g.players[4],g.players[5],false],[g.players[6],g.players[7],false]];
      DT.UI.refresh();
    });
    await page.click('#readyBtn');
    await page.click('#skipBtn');
    await page.waitForFunction(()=>DT.KillScreens.active?.phase==='attack');
    const attribution=await page.evaluate(()=>{
      const m=__digi.game.results.find(r=>r.match.a===__digi.game.human).match, cb=m.combat;
      const h=cb.lastKills[0], death=cb.events.filter(e=>e.type==='death'&&e.by!=null&&cb.units[e.by]?.team===0).at(-1);
      return {match:h.id===death.by,unit:DT.LINE[h.line].forms[h.stage],move:DT.LINE[h.line].abil[h.stage]};
    });
    check(attribution.match,'Final-hit attribution matches the actual final death event');
    await page.waitForFunction(()=>DT.KillScreens.active?.fired);
    check((await page.locator('#ksLayer .ksMain').textContent()).includes(attribution.move),'Real ultimate name appears: '+attribution.move);
    await page.screenshot({path:path.join(out,'killcam-attack.png')});
    await page.waitForFunction(()=>DT.KillScreens.active?.phase==='dance',null,{timeout:30000});
    await page.waitForTimeout(450);
    const info=await page.evaluate(()=>({active:DT.KillScreens.active,lineup:DT.KillScreens.lineupInfo()}));
    check(info.active?.team===3,'Whole board is in the victory dance');
    check(info.lineup?.members.some(m=>!m.legend&&m.y===0&&Math.abs(m.x)<1.5),'Front-row Digimon dances beside the Little Legend');
    check((await page.locator('.ksHonor').textContent()).includes(attribution.unit),'Final-hit Digimon is named in the victory screen');
    await page.screenshot({path:path.join(out,'killcam-dance.png')});
    await page.keyboard.press('Escape');
    check(!await page.evaluate(()=>DT.KillScreens.active),'Escape dismisses the sequence');
    const baseline=[];
    // Repeated victim views also exercise disposal and a skip during the attack.
    for(let i=0;i<3;i++){
      await page.evaluate(()=>{
        const g=__digi.game,m=g.results[0].match,h=m.combat.lastKills[0];
        g.human=g.players[1];
        DT.KillScreens.demo({killer:g.players[0],victim:g.players[1],finalHit:h,team:DT.KillScreens.teamOf(g.players[0])});
      });
      await page.waitForFunction(()=>DT.KillScreens.active?.fired);
      check(await page.evaluate(()=>DT.KillScreens.active.big&&DT.KillScreens.active.phase==='attack'),'Eliminated player sees the full attack '+(i+1));
      if(i===0)await page.screenshot({path:path.join(out,'killcam-victim.png')});
      await page.locator('#ksLayer').click({position:{x:20,y:20}});
      await page.waitForTimeout(250);
      baseline.push(await page.evaluate(()=>DT.KillScreens.memory()));
    }
    check(baseline[2].sceneChildren===baseline[0].sceneChildren,'Scene object count remains stable after repeated killcams');
    check(baseline[2].live.geometries<=baseline[0].live.geometries+1&&baseline[2].live.textures<=baseline[0].live.textures+1,'GPU geometry and texture counts remain stable');
    await page.evaluate(()=>{
      DT.UI.newGame('faction-wars','hyper','duos');
      const g=__digi.game;g.stage=2;g.round=1;g.carousel=null;g.phase='planning';g.roundType='pvp';g.isPvE=false;
      for(const p of g.players){p.board={};p.bench.fill(null);p.offer=null;}
      const strong=DT.SHOP_LINES.find(l=>l.tier===5),weak=DT.SHOP_LINES.find(l=>l.tier===1);
      g.players[0].board={'3,4':g.newUnit(weak.id,0)};
      g.players[1].board={'3,4':g.newUnit(strong.id,2),'2,4':g.newUnit(strong.id,2),'4,4':g.newUnit(strong.id,2)};
      for(const p of g.teams[1].members){p.board={'3,4':g.newUnit(weak.id,0)};p.hp=1;}
      g.teams[1].hp=1;
      g.duoPairs=[[g.teams[0],g.teams[1],false],[g.teams[2],g.teams[3],false]];
      g.human=g.players[3];g.human.isHuman=true;
      DT.UI.refresh();
    });
    await page.click('#readyBtn');await page.click('#skipBtn');
    await page.waitForFunction(()=>DT.KillScreens.active?.phase==='attack');
    const duo=await page.evaluate(()=>{
      const g=__digi.game,cb=g.results.find(r=>r.match.a.id===0).match.combat;
      const death=cb.events.filter(e=>e.type==='death'&&e.by!=null&&cb.units[e.by]?.team===0).at(-1);
      const actual=death&&cb.units[death.by];
      return {a:DT.KillScreens.active,out:g.teams[1].members.every(p=>!p.alive),actual:actual&&{id:actual.id,owner:actual.owner,name:g.players[actual.owner]?.name}};
    });
    check(duo.out&&duo.a.big,'Second eliminated Duos partner gets the full team killcam');
    check(await page.locator('#ksLayer').isVisible(),'Eliminated partner can see the killcam over the results page');
    // Either partner can get the last hit, even with one much stronger board. Verify against the actual
    // death event instead of assuming player 1 won the random target/attack ordering.
    check(duo.actual&&duo.a.finalHit.id===duo.actual.id&&duo.a.finalHit.owner===duo.actual.owner&&duo.a.killer===duo.actual.name,'Duos credits the actual final-hit owner from the combat log');
    console.log('Duos final-hit evidence:', JSON.stringify({displayed:duo.a.finalHit,killer:duo.a.killer,actual:duo.actual}));
    check(duo.a.team===4,'Duos victory lineup includes both winning boards');
    await page.keyboard.press('Escape');
    // Render a crowded Duos victory at both aspect ratios; the full group must share a floor, not risers.
    for(const [label,width,height] of [['wide',1500,920],['narrow',720,1000]]){
      await page.setViewportSize({width,height});
      await page.evaluate(async()=>{
        const g=__digi.game,pool=DT.SHOP_LINES.slice(0,10);
        const team=Array.from({length:20},(_,i)=>({line:pool[i%pool.length].id,stage:i%2,owner:i<10?0:1,uid:1000+i,finalHit:i===0}));
        await DT.Dances.coverage(team.map(u=>'unit:'+DT.modelKey(DT.LINE[u.line],u.stage)));
        DT.KillScreens.demo({killer:g.players[0],victim:g.human,team});
      });
      await page.waitForFunction(()=>{
        const a=DT.KillScreens.active;
        if(!a||a.rigs!==a.expected||a.t<1.8)return false;
        // Hold this rendered pose while software GL captures the screenshot; the four-second celebration
        // can otherwise finish between the assertion and capture on a busy machine.
        const raf=window.requestAnimationFrame;let frozen;
        window.requestAnimationFrame=cb=>raf(t=>cb(frozen??(frozen=t)));
        window.__restoreKillcamFrame=()=>{window.requestAnimationFrame=raf;};
        return true;
      },null,{timeout:30000});
      const lineup=await page.evaluate(()=>DT.KillScreens.lineupInfo());
      check(lineup.members.filter(m=>!m.legend).length===20&&lineup.members.every(m=>m.y===0),`${label}: all 20 Duos Digimon and legend share ground level`);
      check(await page.evaluate(()=>!!document.elementFromPoint(innerWidth/2,innerHeight/2)?.closest('#ksLayer')),`${label}: grounded dance is visibly above the results screen`);
      await page.screenshot({path:path.join(out,`killcam-grounded-${label}.png`)});
      await page.evaluate(()=>window.__restoreKillcamFrame());
      await page.keyboard.press('Escape');
    }
    check(errors.length===0,'No browser exceptions: '+errors.join('; '));
  }finally{if(browser)await browser.close();server.kill();}
  process.exitCode=failed?1:0;
})().catch(e=>{console.error(e);process.exitCode=1;});
