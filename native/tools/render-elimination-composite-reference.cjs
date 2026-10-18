// Original Canvas backgrounds and actual browser CSS opacity transitions, compared
// with EliminationCompositeSmoke. This tests group compositing, not model/font art.
// node native/tools/render-elimination-composite-reference.cjs [playwright-package] [reference.png] [native.bmp]
const fs=require('fs'),path=require('path');
const {chromium}=require(process.argv[2]||'../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/killscreens.js'),'utf8');
(async()=>{const browser=await chromium.launch({channel:'msedge',headless:true});try{
 const page=await browser.newPage({viewport:{width:1360,height:570},deviceScaleFactor:1});
 await page.setContent('<style>html,body{margin:0;width:1360px;height:570px;overflow:hidden}</style><canvas id="checker" width="1360" height="570" style="position:absolute;left:0;top:0"></canvas>');
 const opacities=await page.evaluate(({source})=>{
  const themes=new Function(source.slice(source.indexOf('const R ='),source.indexOf('// ---------- catalog ----------'))+'\nreturn THEMES;')();
  const fade=Number(source.match(/const FADE = ([\d.]+);/)[1]);
  const css=source.match(/#ksLayer \{[^}]+\$\{FADE\}s;[^}]+\}/)[0].replace('${FADE}',fade);
  const style=document.createElement('style');style.textContent=css;document.head.appendChild(style);
  const board=document.querySelector('#checker').getContext('2d');
  for(let y=0;y<570;y+=10)for(let x=0;x<1360;x+=10){board.fillStyle=(x/10+y/10)%2===0?'rgb(38,52,68)':'rgb(120,140,160)';board.fillRect(x,y,10,10);}
  const opacities=[];
  for(let i=0;i<12;i++){
   const column=i%4,node=document.createElement('div');node.id='ksLayer';node.style.cssText=`left:${column*340}px;top:${Math.floor(i/4)*190}px;width:340px;height:190px;border:none;box-shadow:none;`;
   const cv=document.createElement('canvas');cv.width=340;cv.height=190;cv.style.display='block';node.appendChild(cv);document.body.appendChild(node);
   const c=cv.getContext('2d');themes[['gate','confetti','firewall'][Math.floor(i/4)]].draw(c,340,190,.7);
   c.fillStyle='rgba(40,150,230,'+(166/255)+')';c.fillRect(95,45,120,105);
   c.fillStyle='rgba(255,140,60,'+(191/255)+')';c.beginPath();c.arc(220,90,45,0,Math.PI*2);c.fill();
   c.fillStyle='rgba(0,0,0,'+(166/255)+')';c.fillRect(65,130,210,36);
   c.fillStyle='rgba(255,255,255,'+(230/255)+')';for(let letter=0;letter<10;letter++)c.fillRect(80+letter*18,141,11,14);
   const animation=node.animate([{opacity:column<2?0:1},{opacity:column<2?1:0}],{duration:fade*1000,easing:getComputedStyle(node).transitionTimingFunction,fill:'both'});animation.pause();animation.currentTime=(column%2+1)*100;
   opacities.push(Number(getComputedStyle(node).opacity));
  }
  return opacities;
 },{source});
 const destination=process.argv[3]||path.join(root,'native/artifacts/elimination-composite-original.png');
 const reference=await page.screenshot({path:destination});console.log(destination);
 if(process.argv[4]){
  const rows=await page.evaluate(async({reference,native,opacities})=>{
   async function pixels(bytes,mime){const image=new Image();image.src='data:'+mime+';base64,'+bytes;await image.decode();if(image.width!==1360||image.height!==570)throw Error('Composite raster dimensions changed.');const canvas=document.createElement('canvas');canvas.width=1360;canvas.height=570;const c=canvas.getContext('2d');c.drawImage(image,0,0);return c;}
   const a=await pixels(reference,'image/png'),b=await pixels(native,'image/bmp'),rows=[];
   for(let i=0;i<12;i++){
    const aa=a.getImageData(i%4*340,Math.floor(i/4)*190,340,190).data,bb=b.getImageData(i%4*340,Math.floor(i/4)*190,340,190).data;let total=0,large=0;
    for(let p=0;p<aa.length;p+=4){const ds=[Math.abs(aa[p]-bb[p]),Math.abs(aa[p+1]-bb[p+1]),Math.abs(aa[p+2]-bb[p+2])];total+=ds[0]+ds[1]+ds[2];if(Math.max(...ds)>20)large++;}
    rows.push({theme:['gate','confetti','firewall'][Math.floor(i/4)],direction:i%4<2?'in':'out',elapsed:(i%2+1)*.1,cssOpacity:opacities[i],meanAbsoluteRgbError:total/(340*190*3),fractionPixelsOver20:large/(340*190)});
   }return rows;
  },{reference:reference.toString('base64'),native:fs.readFileSync(process.argv[4]).toString('base64'),opacities});
  fs.writeFileSync(destination.replace(/\.[^.]+$/,'.comparison.json'),JSON.stringify(rows,null,2)+'\n');
  const failures=rows.filter(r=>r.meanAbsoluteRgbError>1.5||r.fractionPixelsOver20>.03);
  if(failures.length)throw Error('Native group opacity differs from browser CSS: '+JSON.stringify(failures));
  console.log(`Verified ${rows.length} native composites against actual CSS group opacity; maximum RGB MAE ${Math.max(...rows.map(r=>r.meanAbsoluteRgbError)).toFixed(3)}/255.`);
 }
 }finally{await browser.close();}})().catch(error=>{console.error(error);process.exitCode=1;});
