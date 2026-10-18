// Original client banner paintings, board previews and catalog icons for native cards.
// The browser is used only at build time; the shipped client loads these PNGs.
const fs=require('node:fs'),path=require('node:path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/content/collection');
async function main(){
 fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--use-angle=swiftshader-webgl','--enable-unsafe-swiftshader']});
 try{
  const page=await browser.newPage();
  await page.route('**/*',route=>{
   const url=new URL(route.request().url());if(url.hostname!=='collection.local')return route.abort();
   const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));
   if(!file.startsWith(root+path.sep)||!fs.existsSync(file)||!fs.statSync(file).isFile())return route.fulfill({status:404,body:''});
   const types={'.html':'text/html','.js':'text/javascript','.json':'application/json','.webp':'image/webp','.png':'image/png','.css':'text/css'};
   return route.fulfill({contentType:types[path.extname(file)]||'application/octet-stream',body:fs.readFileSync(file)});
  });
  await page.goto('http://collection.local/index.html');await page.waitForFunction(()=>window.DT?.Boards&&DT.Cosmetics.list('board').length>0);
  const png=async data=>page.evaluate(async data=>{const image=new Image();image.src=data;await image.decode();const c=document.createElement('canvas');c.width=image.width;c.height=image.height;c.getContext('2d').drawImage(image,0,0);return c.toDataURL('image/png').split(',')[1];},data);
  for(const name of ['coll-legends','coll-boards','coll-emotes','store-fx','store-hero']){
   const data='data:image/webp;base64,'+fs.readFileSync(path.join(root,'assets/client',name+'.webp')).toString('base64');
   fs.writeFileSync(path.join(out,name+'.png'),Buffer.from(await png(data),'base64'));
  }
  const manifest={};
  const items=await page.evaluate(()=>['board','dance','sound','elimsound'].flatMap(kind=>DT.Cosmetics.list(kind).map(item=>({kind,id:item.id,name:item.name,icon:item.icon||''}))));
  for(const item of items){
   let data;
   if(item.kind==='board')data=await page.evaluate(id=>DT.Boards.previewUrl(id),item.id);
   else data=await page.evaluate(async item=>{
    const c=document.createElement('canvas');c.width=160;c.height=120;const ctx=c.getContext('2d');
    if(item.icon&&/[/.]/.test(item.icon)&&/^(https?:|data:|\.|\/|[\w-]+\/)/.test(item.icon)){
     const image=new Image();image.src=item.icon;await image.decode();const s=Math.min(144/image.width,104/image.height);ctx.drawImage(image,(160-image.width*s)/2,(120-image.height*s)/2,image.width*s,image.height*s);
    }else{ctx.font='64px "Segoe UI Emoji", sans-serif';ctx.textAlign='center';ctx.textBaseline='middle';ctx.fillStyle='#c8aa6e';ctx.fillText(item.icon||item.name.split(/\s+/).map(w=>w[0]).join('').slice(0,3),80,60);}
    return c.toDataURL('image/png');
   },item);
   if(!data)throw Error('Missing original card artwork: '+item.kind+'/'+item.id);
   const file=item.kind+'-'+item.id;fs.writeFileSync(path.join(out,file+'.png'),Buffer.from(await png(data),'base64'));manifest[item.kind+'/'+item.id]=file;
  }
  fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2)+'\n');
  console.log(`Exported 5 original Collection/Store paintings and ${items.length} catalog card previews.`);
 }finally{await browser.close();}
}
main().catch(error=>{console.error(error);process.exitCode=1;});
