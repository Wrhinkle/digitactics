// Decode the original client artwork to the native RGBA PNG format without repainting it.
const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
(async()=>{
 const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/content/postgame');fs.mkdirSync(out,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  for(const name of ['profile-1','profile-2','profile-3','profile-4']){
   const data=fs.readFileSync(path.join(root,'assets/client',name+'.webp')).toString('base64');
   const png=await page.evaluate(async data=>{const image=new Image();image.src='data:image/webp;base64,'+data;await image.decode();const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;canvas.getContext('2d').drawImage(image,0,0);return canvas.toDataURL('image/png').split(',')[1];},data);
   fs.writeFileSync(path.join(out,name+'.png'),Buffer.from(png,'base64'));
  }
  console.log('Exported four original profile banners.');
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
