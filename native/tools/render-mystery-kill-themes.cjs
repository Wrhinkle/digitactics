// Labeled Canvas contact sheets and caption-background contrast, not a full UI smoke.
// node native/tools/render-mystery-kill-themes.cjs [output-directory]
const fs=require('fs'),path=require('path');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
const output=path.resolve(process.argv[2]||path.join(root,'native/artifacts/secret-finales/visual'));
const source=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8');
(async()=>{
 fs.mkdirSync(output,{recursive:true});
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage({viewport:{width:1360,height:2200},deviceScaleFactor:1});
  const rows=[];
  for(const t of [.7,2.3]){
   const result=await page.evaluate(({source,t})=>{
    const themes=new Function(source.replace(/export /g,'')+'\nreturn killThemes;')();
    const canvas=document.createElement('canvas');canvas.width=1360;canvas.height=2200;const c=canvas.getContext('2d');
    c.fillStyle='#10141c';c.fillRect(0,0,1360,2200);const rows=[];
    for(let i=0;i<themes.length;i++){
     const theme=themes[i],tile=document.createElement('canvas');tile.width=340;tile.height=190;const p=tile.getContext('2d');theme.theme.draw(p,340,190,t);
     // The normal victory caption sits in the lower middle. Measure its background
     // before painting a white type swatch, so the letters cannot make the test pass.
     const pixels=p.getImageData(85,124,170,49).data;let worst=21,sum=0;
     for(let n=0;n<pixels.length;n+=4){const rgb=[pixels[n],pixels[n+1],pixels[n+2]].map(x=>x/255).map(x=>x<=.04045?x/12.92:((x+.055)/1.055)**2.4);const contrast=1.05/(.2126*rgb[0]+.7152*rgb[1]+.0722*rgb[2]+.05);worst=Math.min(worst,contrast);sum+=contrast;}
     rows.push({id:theme.id,t,worstWhiteCaptionContrast:worst,meanWhiteCaptionContrast:sum/(pixels.length/4)});
     const x=i%4*340,y=Math.floor(i/4)*220;
     c.fillStyle='#e3e7ef';c.font='16px sans-serif';c.textAlign='left';c.fillText(`${i+1}. ${theme.name}`,x+8,y+21);
     c.drawImage(tile,x,y+30);c.save();c.shadowColor='#000';c.shadowBlur=5;c.fillStyle='#fff';c.textAlign='center';c.font='bold 17px sans-serif';c.fillText('VICTORY',x+170,y+181);c.font='13px sans-serif';c.fillText('Your team wins',x+170,y+198);c.restore();
    }
    return {png:canvas.toDataURL('image/png').split(',')[1],rows};
   },{source,t});
   fs.writeFileSync(path.join(output,`secret-finales-${t}.png`),Buffer.from(result.png,'base64'));rows.push(...result.rows);
  }
  fs.writeFileSync(path.join(output,'caption-contrast.json'),JSON.stringify(rows,null,2)+'\n');
  const failures=rows.filter(row=>row.worstWhiteCaptionContrast<4.5);
  if(failures.length)throw Error('White caption background contrast below 4.5: '+JSON.stringify(failures));
  console.log(`Rendered all 40 secret finales at two times; minimum white-caption background contrast ${Math.min(...rows.map(r=>r.worstWhiteCaptionContrast)).toFixed(2)}:1. ${output}`);
 }finally{await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
