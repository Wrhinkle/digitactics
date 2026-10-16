// End-to-end Duos client checks, using local data and a headless browser.
const path = require('path');
const { spawn } = require('child_process');
exports.run = async ({ PW, OUT, PORT, ok }) => {
  const { chromium } = require(PW);
  const server = spawn(process.execPath, [path.join(__dirname, '..', 'serve.cjs')], { env: { ...process.env, PORT }, windowsHide: true, stdio: 'ignore' });
  let browser;
  try {
    await new Promise(r => setTimeout(r, 700));
    browser = await chromium.launch({ args: ['--use-gl=angle', '--use-angle=swiftshader'] });
    const page = await browser.newPage({ viewport: { width: 1500, height: 920 } });
    const errors = [];
    page.on('pageerror', e => errors.push(String(e)));
    await page.goto(`http://localhost:${PORT}/`);
    await page.waitForFunction(() => window.DT?.SetPicker);
    await page.evaluate(() => DT.SetPicker.setFormat('duos'));
    await page.click('#startBtn');
    await page.waitForFunction(() => window.__digi?.game);
    await page.click('#timerBtn');
    ok(await page.evaluate(() => __digi.game.duo), 'Play selection starts a Duos game');
    await page.locator('.choice[data-carousel]:not(.taken)').first().click();
    await page.click('#carouselGo');
    await page.waitForTimeout(500);
    await page.locator('.card[data-slot]:not(.poor)').first().click();
    const bench = await page.evaluate(() => {
      const g=__digi.game, i=g.human.bench.findIndex(Boolean), r=__digi.renderer;
      return { i, uid:g.human.bench[i]?.uid, point:r.screenOfBench(i), partner:g.partnerOf(g.human).id };
    });
    const box = await page.locator('#board').boundingBox();
    const portal = await page.locator('#duoPortal').boundingBox();
    await page.mouse.move(box.x+bench.point.x,box.y+bench.point.y);
    await page.mouse.down();
    await page.mouse.move(portal.x+portal.width/2,portal.y+portal.height/2,{steps:12});
    await page.mouse.up();
    ok(await page.evaluate(uid => !__digi.game.human.allUnits().some(u=>u.uid===uid) && __digi.game.human.portal===1, bench.uid), 'Drag a bought bench Digimon through the portal');
    await page.click('#duoPartner');
    await page.waitForTimeout(600);
    ok(await page.evaluate(id => __digi.ui.viewing===id,bench.partner), 'Partner button opens the actual partner board');
    await page.click('#duoPartner');
    await page.waitForTimeout(600);
    await page.screenshot({path:path.join(OUT,'duos-planning.png')});
    // Advance actual game rounds through the controls, until the first combined PvP fight.
    for(let n=0;n<8;n++) {
      const s=await page.evaluate(()=>({phase:__digi.game.phase,pvp:__digi.game.roundType==='pvp',offer:!!__digi.game.human.offer}));
      if(s.pvp && s.phase==='planning') break;
      if(s.offer) await page.locator('.choice[data-offer]').first().click();
      if(s.phase==='planning') await page.click('#readyBtn');
      await page.waitForTimeout(200);
      await page.click('#skipBtn');
      await page.waitForFunction(()=>__digi.game.phase==='planning',{},{timeout:10000});
    }
    if (await page.evaluate(()=>!!__digi.game.human.offer)) {
      await page.locator('.choice[data-offer]').first().click({timeout:15000});
      await page.waitForFunction(()=>!__digi.game.human.offer);
    }
    await page.click('#readyBtn');
    await page.waitForFunction(()=>__digi.game.phase==='combat');
    await page.evaluate(()=>__digi.ui.speed=.05);
    await page.waitForTimeout(700);
    ok(await page.evaluate(()=>__digi.shown?.cols===14 && __digi.renderer.tiles.filter(t=>t.visible).length===112), 'Combined fight renders all 112 hexes in a 14-column arena');
    await page.screenshot({path:path.join(OUT,'duos-melee.png')});
    const ids=await page.evaluate(()=>__digi.game.alivePlayers().map(p=>p.id));
    for(const id of ids) {
      await page.evaluate(id=>DT.Scout.view(id),id);
      await page.waitForTimeout(450);
      ok(await page.evaluate(()=>!!__digi.shown && __digi.shown.cols===14),'Spectate team seat '+id);
    }
    for(const [width,height] of [[1280,720],[1366,768],[1500,920],[1920,1080]]) {
      await page.setViewportSize({width,height}); await page.waitForTimeout(250);
      ok(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth),'No page overflow at '+width+'x'+height);
    }
    ok(errors.length===0,'No browser errors: '+errors.join('; '));
    await page.close();
    // Exercise web lobby format persistence and the fallback for existing deployed database rules, locally.
    const { installFakeFirebase } = require('./fake-firebase.cjs');
    for (const rejectLobbyFormat of [false,true]) {
      const web = await browser.newPage({viewport:{width:1280,height:720}});
      const webErrors=[];web.on('pageerror',e=>webErrors.push(String(e)));
      await installFakeFirebase(web,{rejectLobbyFormat});
      await web.goto(`http://localhost:${PORT}/`,{waitUntil:'domcontentloaded',timeout:60000});
      await web.click('#mpBtn');
      await web.fill('#mpName','Local duo test');
      await web.selectOption('#mpFormat','duos');
      await web.click('#mpCreate');
      await web.waitForSelector('#mpStart');
      ok(await web.inputValue('#mpRoomFormat')==='duos','Web lobby keeps Duos'+(rejectLobbyFormat?' using presence fallback':''));
      await web.click('#mpStart');
      await web.waitForFunction(()=>window.__digi?.game?.duo);
      ok(await web.evaluate(()=>__digi.game.teams.length===4),'Web game init carries the format and team assignment');
      ok(webErrors.length===0,'Web lobby has no browser errors');
      await web.close();
    }
    return 0;
  } finally { if(browser) await browser.close(); server.kill(); }
};
