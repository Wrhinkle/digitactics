// Bake the same system emoji used by the Windows browser UI; runtime is native.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 const source=fs.readFileSync(path.join(root,'js/farm.js'),'utf8');
 const body=source.slice(source.indexOf('function moodOf('),source.indexOf('\nfunction card('));
 const icons=[...body.matchAll(/icon: '([^']*)', text: '([^']+)'/g)].map(m=>({icon:m[1],text:m[2]}));
 if(icons.length!==10)throw Error('Mood source changed');
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  const png=await page.evaluate(icons=>{
   const c=document.createElement('canvas');c.width=64*icons.length;c.height=64;
   const g=c.getContext('2d');g.font='40px "Segoe UI Emoji"';g.textAlign='center';g.textBaseline='middle';
   icons.forEach((v,i)=>{g.fillStyle=v.icon==='♪'?'#e2558f':'#333';g.fillText(v.icon,i*64+32,32);});
   return c.toDataURL('image/png').split(',')[1];
  },icons);
  fs.writeFileSync(path.join(root,'native/content/farm-moods.png'),Buffer.from(png,'base64'));
 }finally{await browser.close();}
 const evaluate=new Function('p','lights','ideal',`const save={lights},SLEEPY_AT=20,FAT=1.4;const asleepNow=p=>!!p.flags.asleep,idealOf=()=>ideal;${body};return moodOf(p,'test');`);
 let seed=1531;const random=()=>{seed=seed*16807%2147483647;return seed/2147483647;};const rows=[];
 for(let i=0;i<10000;i++){
  const p={flags:{asleep:random()<.1},sick:random()<.1,tantrum:random()<.1?123:0,hunger:Math.floor(random()*101),energy:Math.floor(random()*101),poop:Array(Math.floor(random()*5)).fill({}),happy:Math.floor(random()*101),weight:Math.floor(random()*65)};
  const lights=random()<.5,ideal=[10,20,30,40][i%4];
  rows.push({p,lights,ideal,mood:evaluate(p,lights,ideal)});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-moods.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
 console.log('Exported 10 mood glyphs and 10000 original mood decisions.');
}
main().catch(e=>{console.error(e);process.exitCode=1;});
