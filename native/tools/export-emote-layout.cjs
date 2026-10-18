const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/emotes.js'),'utf8');
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.setContent('<div id="outer"><div class="em-in"></div></div>');
  await page.addStyleTag({content:source.match(/st\.textContent = `([\s\S]*?)`;/)[1]});
  const rows=await page.evaluate(()=>{
   const outer=document.querySelector('#outer'),inner=outer.firstChild,rows=[];
   for(const row of [false,true])for(const age of [0,1,10,50,100,175,250,349,350,1000,2300,2301,2350,2400,2450,2500,2550,2599]){
    outer.className=(row?'em-row':'em-big')+(age>2300?' out':'');
    getComputedStyle(inner).transform;
    const a=inner.getAnimations()[0];a.pause();a.currentTime=age>2300?age-2300:age;
    const css=getComputedStyle(inner),m=new DOMMatrix(css.transform);
    rows.push({row,age,scale:m.a,x:m.e,y:m.f,opacity:+css.opacity});
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/emote-layout.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
  console.log(`Exported ${rows.length} original browser animation transforms.`);
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
