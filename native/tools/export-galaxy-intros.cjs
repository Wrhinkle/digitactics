// Preserve the original chosen-Galaxy announcement, including its set artwork.
const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/content/galaxy-intros');
const DT=require('../../tools/headless.cjs').load({strict:true});
DT.Sets.activate('galaxies','normal');
const intros=Object.fromEntries(DT.Sets.def('galaxies').galaxies.map(g=>{
  const intro=DT.mechanicIntro({set:DT.SET,mech:{galaxy:g.id}});
  if(!intro?.name||!intro.desc||!/^#[a-f\d]{6}$/i.test(intro.color))throw Error('Invalid source Galaxy intro '+g.id);
  return [g.id,{title:intro.title,name:intro.name,description:intro.desc,color:intro.color}];
}));
fs.mkdirSync(out,{recursive:true});
(async()=>{
  const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-gpu']});
  try{
    const page=await browser.newPage();
    await page.route('**/*',route=>{
      const url=new URL(route.request().url());
      if(url.hostname!=='galaxy.local')return route.abort();
      const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));
      if(!file.startsWith(root+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile())return route.fulfill({status:404,body:''});
      const types={'.html':'text/html','.js':'text/javascript','.json':'application/json','.webp':'image/webp','.png':'image/png','.css':'text/css'};
      return route.fulfill({contentType:types[path.extname(file)]||'application/octet-stream',body:fs.readFileSync(file)});
    });
    await require('../../tests/live-source-bundle.cjs')(page);
    await page.goto('http://galaxy.local/index.html');
    await page.waitForFunction(()=>window.DT?.ItemArt&&DT.Sets);
    const images=await page.evaluate(async ids=>{
      DT.Sets.activate('galaxies','normal');const result={};
      for(const id of ids){const canvas=document.createElement('canvas');canvas.width=canvas.height=128;if(!await DT.ItemArt.paint(canvas,'galaxy:'+id))throw Error('Missing Galaxy art '+id);result[id]=canvas.toDataURL('image/png').split(',')[1];}
      return result;
    },Object.keys(intros));
    for(const [id,png]of Object.entries(images))fs.writeFileSync(path.join(out,id+'.png'),Buffer.from(png,'base64'));
    fs.writeFileSync(path.join(out,'catalog.json'),JSON.stringify(intros,null,2)+'\n');
    console.log(`Exported ${Object.keys(intros).length} source Galaxy announcements and original portraits.`);
  }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
