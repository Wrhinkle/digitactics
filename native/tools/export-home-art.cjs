// Original client paintings and set copy; the native runtime never executes JavaScript.
const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),out=path.join(root,'native/content/home');
fs.mkdirSync(out,{recursive:true});
fs.copyFileSync(path.join(root,'icons/favicon-32.png'),path.join(out,'client-mark.png'));
const DT=require('../../tools/headless.cjs').load({strict:true});
const client=fs.readFileSync(path.join(root,'js/client.js'),'utf8');
const match=client.match(/const SET_ART = (\{[\s\S]*?\n\});/);
if(!match)throw Error('Original client SET_ART mapping missing.');
const mapping=require('vm').runInNewContext('('+match[1]+')');
const sets={};
for(const entry of DT.Sets.pickable()){
 const set=DT.Sets.get(entry.id),art=mapping[entry.id]||{hero:'hero-digital-world',tag:'Set'};
 sets[entry.id]={name:set.name,description:set.desc||'',mechanic:set.mechanic?.name||'',mechanicDescription:set.mechanic?.desc||'',hero:art.hero,tag:art.tag};
}
const picker=fs.readFileSync(path.join(root,'js/setpicker.js'),'utf8');
const chaos=require('vm').runInNewContext('('+picker.match(/const CHAOS = (\{[^\n]+\});/)[1]+')');
sets.chaos={name:chaos.name,description:chaos.desc,mechanic:'',mechanicDescription:'',hero:mapping.chaos.hero,tag:mapping.chaos.tag};
fs.writeFileSync(path.join(out,'sets.json'),JSON.stringify(sets));
(async()=>{
 const browser=await chromium.launch({channel:'msedge',headless:true,args:['--disable-gpu']});
 try{
  const page=await browser.newPage();
  const names=[...new Set([...Object.values(sets).map(x=>x.hero),...Object.keys(sets).map(id=>'mode-'+id),'mode-solo','lobby-bg','hero-arena','hero-datastream-sky','hero-digivice-energy','events-bg','store-hero','hero-file-island','mode-faction-wars','mode-dragonlands','mode-autochess','mode-multiplayer'])];
  for(const name of names){
   const data=fs.readFileSync(path.join(root,'assets/client',name+'.webp')).toString('base64');
   const png=await page.evaluate(async data=>{const image=new Image();image.src='data:image/webp;base64,'+data;await image.decode();const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;canvas.getContext('2d').drawImage(image,0,0);return canvas.toDataURL('image/png').split(',')[1];},data);
   fs.writeFileSync(path.join(out,name+'.png'),Buffer.from(png,'base64'));
  }
  console.log(`Exported ${names.length} original home paintings and ${Object.keys(sets).length} set descriptions.`);
 }finally{await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
