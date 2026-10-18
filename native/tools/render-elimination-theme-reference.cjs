// Optional raster evidence for EliminationThemeSmoke: original browser Canvas at identical bounds/time.
// node native/tools/render-elimination-theme-reference.cjs [playwright-package] [reference.png] [native-page-zero.bmp]
// Passing native-page-zero.bmp compares all four native captures and rejects material pixel differences.
const fs=require('fs'),path=require('path');
const {chromium}=require(process.argv[2]||'../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/killscreens.js'),'utf8'),mystery=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8');
(async()=>{const browser=await chromium.launch({channel:'msedge',headless:true});try{
 const page=await browser.newPage({viewport:{width:1360,height:2470}});
 const png=await page.evaluate(({source,mystery})=>{
  const themes=new Function(mystery.replace(/export /g,'')+'\n'+source.slice(source.indexOf('const R ='),source.indexOf('// ---------- catalog ----------'))+'\nfor(const item of killThemes)THEMES[item.id]=item.theme;return THEMES;')();
  const cv=document.createElement('canvas');cv.width=1360;cv.height=2470;const c=cv.getContext('2d');c.fillStyle='#000';c.fillRect(0,0,cv.width,cv.height);
  let i=0;for(const theme of Object.values(themes)){c.save();c.translate(i%4*340,Math.floor(i/4)*190);c.beginPath();c.rect(0,0,340,190);c.clip();theme.draw(c,340,190,.7);c.restore();i++;}
  window.themeReference=cv;return cv.toDataURL('image/png').split(',')[1];
 },{source,mystery});
 const destination=process.argv[3]||path.join(root,'native/artifacts/elimination-themes-original.png');fs.writeFileSync(destination,Buffer.from(png,'base64'));console.log(destination);
 const native=process.argv[4];
 if(native){
  const rows=[];
  for(let pageIndex=0;pageIndex<4;pageIndex++){
   const file=pageIndex===0?native:native.replace(/\.[^.]+$/,`.page${pageIndex}.bmp`);
   const values=await page.evaluate(async({bytes,pageIndex})=>{
    const img=new Image();img.src='data:image/bmp;base64,'+bytes;await img.decode();
    if(img.width!==1360||img.height!==760)throw Error('Native comparison page dimensions changed.');
    const cv=document.createElement('canvas');cv.width=1360;cv.height=760;const c=cv.getContext('2d');c.drawImage(img,0,0);
    const reference=window.themeReference.getContext('2d'),out=[];
    for(let i=pageIndex*16;i<Math.min(51,(pageIndex+1)*16);i++){
     const a=reference.getImageData(i%4*340,Math.floor(i/4)*190,340,190).data,b=c.getImageData(i%4*340,Math.floor(i%16/4)*190,340,190).data;
     let total=0,large=0;for(let p=0;p<a.length;p+=4){const ds=[Math.abs(a[p]-b[p]),Math.abs(a[p+1]-b[p+1]),Math.abs(a[p+2]-b[p+2])];total+=ds[0]+ds[1]+ds[2];if(Math.max(...ds)>20)large++;}
     out.push({index:i,meanAbsoluteRgbError:total/(340*190*3),fractionPixelsOver20:large/(340*190)});
    }return out;
   },{bytes:fs.readFileSync(file).toString('base64'),pageIndex});rows.push(...values);
  }
  fs.writeFileSync(destination.replace(/\.[^.]+$/,'.comparison.json'),JSON.stringify(rows,null,2)+'\n');
  const failures=rows.filter(r=>r.meanAbsoluteRgbError>1.5||r.fractionPixelsOver20>.03);
  if(failures.length)throw Error('Native theme raster differs from original Canvas: '+JSON.stringify(failures));
  console.log(`Verified ${rows.length} native theme rasters against original Canvas; maximum RGB MAE ${Math.max(...rows.map(r=>r.meanAbsoluteRgbError)).toFixed(3)}/255.`);
 }
 }finally{await browser.close();}})().catch(error=>{console.error(error);process.exitCode=1;});
