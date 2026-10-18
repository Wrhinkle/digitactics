const fs=require('fs'),path=require('path'),crypto=require('crypto');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/content/evolution-portraits');
async function main(){
 const forms=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/evolution.json'),'utf8')).forms;
 const portraits=JSON.parse(fs.readFileSync(path.join(root,'models/portraits/index.json'),'utf8'));
 fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.route('**/*',route=>{
   const url=new URL(route.request().url()); if(url.hostname!=='portraits.local')return route.abort();
   const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));
   if(!file.startsWith(root+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile())return route.fulfill({status:404,body:''});
   const types={'.html':'text/html','.js':'text/javascript','.json':'application/json','.webp':'image/webp','.png':'image/png','.css':'text/css'};
   return route.fulfill({contentType:types[path.extname(file)]||'application/octet-stream',body:fs.readFileSync(file)});
  });
  await page.goto('http://portraits.local/index.html'); await page.waitForFunction(()=>window.DT?.Legends);
  const manifest={},cache=new Map();
  for(const [id,form] of Object.entries(forms)){
   const key=form.legend?'legend:'+form.legend:form.model;
   if(!cache.has(key)){
    let data;
    if(form.legend) data=await page.evaluate(async id=>{
     if(!await DT.Legends.modelFor(id))throw Error('Missing companion model '+id);
     return DT.Legends.thumb(id);
    },form.legend);
    else {
     const entry=portraits[form.model];if(!entry)throw Error('Missing form portrait '+form.model);
     data='data:image/webp;base64,'+fs.readFileSync(path.join(root,'models/portraits',entry.file)).toString('base64');
    }
    const png=await page.evaluate(async data=>{
     const image=new Image();image.src=data;await image.decode();
     const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;
     canvas.getContext('2d').drawImage(image,0,0);return canvas.toDataURL('image/png').split(',')[1];
    },data);
    const bytes=Buffer.from(png,'base64'),file=crypto.createHash('sha256').update(bytes).digest('hex')+'.png';
    fs.writeFileSync(path.join(out,file),bytes);cache.set(key,file);
   }
   manifest[id]=cache.get(key);
  }
  fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
  console.log(`Exported ${Object.keys(manifest).length} evolution portraits (${cache.size} original images).`);
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
