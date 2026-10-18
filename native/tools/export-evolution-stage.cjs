const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/legendevo-ui.js'),'utf8');
function rule(selector){const start=source.indexOf(selector+' {'),end=source.indexOf('}',start);if(start<0||end<0)throw Error('Missing stage CSS');return source.slice(start,end+1);}
(async()=>{
 const out=path.join(root,'native/content/evolution-stage');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage({viewport:{width:520,height:520},deviceScaleFactor:1});
  async function save(selector,name){
   const bytes=await page.locator(selector).screenshot({omitBackground:true});
   const png=await page.evaluate(async data=>{
    const image=new Image();image.src='data:image/png;base64,'+data;await image.decode();
    const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;
    canvas.getContext('2d').drawImage(image,0,0);return canvas.toDataURL('image/png').split(',')[1];
   },bytes.toString('base64'));
   fs.writeFileSync(path.join(out,name+'.png'),Buffer.from(png,'base64'));
  }
  await page.setContent(`<style>html,body{margin:0;background:transparent}${rule('.dte-stage')}${rule('.dte-rays')} .dte-stage{width:384px;height:384px;border:0;border-radius:0}.dte-rays{left:0;top:0;margin:0;opacity:1;transition:none}</style><div class="dte-stage"></div>`);
  await save('.dte-stage','background');
  await page.setContent(`<style>html,body{margin:0;background:transparent}${rule('.dte-rays')}.dte-rays{left:0;top:0;margin:0;opacity:1;transition:none}</style><div class="dte-rays"></div>`);
  await save('.dte-rays','rays');
  console.log('Exported original stage gradient and masked transformation rays.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
