// Preserve independent SVG animation periods without a giant common-cycle atlas.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const hash = bytes => crypto.createHash('sha256').update(bytes).digest('hex');
const TILE = 160, COLUMNS = 8, FRAME_MS = 50, MAX_LOOP_MS = 12000;

async function plan(page, art) {
  return page.evaluate(({art,maxLoop}) => {
    const sheet = document.getElementById('native-emote-sheet'); sheet.replaceChildren();
    const cell = document.createElement('div'); cell.style.cssText='width:160px;height:160px;padding:20px;box-sizing:border-box';
    cell.innerHTML=art; sheet.append(cell);
    const svg=cell.firstElementChild;
    if(svg.tagName.toLowerCase()!=='svg')throw Error('Emote must be source SVG');
    const gcd=(a,b)=>b?gcd(b,a%b):a, lcm=(a,b)=>a/gcd(a,b)*b;
    function period(element) {
      return element.getAnimations({subtree:true}).reduce((total,animation)=>{
        const t=animation.effect.getTiming();
        if(t.iterations!==Infinity||t.delay>0||!Number.isFinite(t.duration)||t.duration<=0)throw Error('Unsupported finite/delayed emote animation');
        const duration=t.duration*(t.direction.startsWith('alternate')?2:1);
        if(duration%50)throw Error('Animation period does not fit source 50ms frame cadence');
        return lcm(total,duration);
      },50);
    }
    const fullPeriod=period(svg),independentCost=[...svg.children].reduce((total,child)=>total+period(child),0);
    if(fullPeriod<=maxLoop&&fullPeriod<=independentCost*1.5)return [{art,loopMs:fullPeriod}];
    // Only independent top-level paint groups are separated. A nested transform,
    // opacity or filter stays with all its children and retains its fill-box origin.
    const layers=[];let pending=[];
    function add(children) {
      if(!children.length)return;
      const copy=svg.cloneNode(false);for(const child of children)copy.append(child.cloneNode(true));
      const duration=children.reduce((total,child)=>lcm(total,period(child)),50);
      if(duration>maxLoop)throw Error('Nested animation cycle exceeds bounded preview atlas');
      layers.push({art:copy.outerHTML,loopMs:duration});
    }
    for(const child of svg.children) {
      if(child.getAnimations({subtree:true}).length){add(pending);pending=[];add([child]);}
      else pending.push(child);
    }
    add(pending);return layers;
  },{art,maxLoop:MAX_LOOP_MS});
}

async function bake(page, art, times) {
  const rows=Math.ceil(times.length/COLUMNS),height=rows*TILE;
  if(height>8192)throw Error('Emote atlas exceeds conservative GPU texture limit');
  await page.evaluate(({art,times,height})=>{
    const sheet=document.getElementById('native-emote-sheet');sheet.replaceChildren();sheet.style.height=height+'px';
    for(const time of times){const cell=document.createElement('div');cell.style.cssText='width:160px;height:160px;padding:20px;box-sizing:border-box;overflow:hidden;background:transparent';cell.innerHTML=art;sheet.append(cell);for(const animation of cell.getAnimations({subtree:true})){animation.pause();animation.currentTime=time;}}
  },{art,times,height});
  return page.locator('#native-emote-sheet').screenshot({omitBackground:true,animations:'allow'});
}

async function exportPreview(page,out,item) {
  const layers=await plan(page,item.art), result=[];
  for(let index=0;index<layers.length;index++) {
    const layer=layers[index],frames=layer.loopMs/FRAME_MS;
    const bytes=await bake(page,layer.art,Array.from({length:frames},(_,i)=>i*FRAME_MS));
    const file=item.id+'.preview-'+index+'.png';fs.writeFileSync(path.join(out,file),bytes);
    result.push({file,sha256:hash(bytes),frames,frameMs:FRAME_MS,loopMs:layer.loopMs,columns:COLUMNS,rows:Math.ceil(frames/COLUMNS),data:bytes.toString('base64')});
  }
  // Compare independent native-style raster composition with whole source SVGs
  // before/at/after every cycle boundary, including the former 2.6-second reset.
  const times=[...new Set([0,50,2550,2600,2650,...layers.flatMap(x=>[Math.max(0,x.loopMs-50),x.loopMs,x.loopMs+50])])].sort((a,b)=>a-b);
  const reference=await bake(page,item.art,times);
  const comparison=await page.evaluate(async({layers,times,reference})=>{
    async function load(data){const image=new Image();image.src='data:image/png;base64,'+data;await image.decode();return image;}
    const original=await load(reference),images=await Promise.all(layers.map(x=>load(x.data)));
    const expected=document.createElement('canvas'),actual=document.createElement('canvas');expected.width=actual.width=1280;expected.height=actual.height=original.height;
    expected.getContext('2d').drawImage(original,0,0);const ctx=actual.getContext('2d');
    for(let sample=0;sample<times.length;sample++)for(let l=0;l<layers.length;l++){const layer=layers[l],frame=Math.floor(times[sample]%layer.loopMs/layer.frameMs);ctx.drawImage(images[l],frame%8*160,Math.floor(frame/8)*160,160,160,sample%8*160,Math.floor(sample/8)*160,160,160);}
    const a=expected.getContext('2d').getImageData(0,0,1280,original.height).data,b=ctx.getImageData(0,0,1280,original.height).data;
    let total=0,max=0,large=0;
    for(let i=0;i<a.length;i+=4)for(let c=0;c<4;c++){const d=Math.abs(c===3?a[i+c]-b[i+c]:(a[i+c]*a[i+3]-b[i+c]*b[i+3])/255);total+=d;max=Math.max(max,d);if(d>16)large++;}
    return {samples:times,meanAbsoluteError:total/a.length,maxAbsoluteError:max,largeErrorFraction:large/a.length};
  },{layers:result,times,reference:reference.toString('base64')});
  if(comparison.meanAbsoluteError>.5||comparison.largeErrorFraction>.005)throw Error(item.id+' preview source composition mismatch: '+JSON.stringify(comparison));
  return {layers:result.map(({data,...layer})=>layer),comparison};
}

async function exportNativeReferences(page,directory,items,sourceHash) {
  fs.mkdirSync(directory,{recursive:true});const pages=[];
  for(const ages of [[2550,2600],[5950,6000]])for(let start=0;start<items.length;start+=12){
    const cells=items.slice(start,start+12).flatMap(item=>ages.map(age=>({id:item.id,age,art:item.art})));
    await page.evaluate(cells=>{
      const sheet=document.getElementById('native-emote-sheet');sheet.replaceChildren();sheet.style.cssText='display:grid;grid-template-columns:repeat(6,160px);width:960px;height:640px;background:#142130;';
      for(const cell of cells){const div=document.createElement('div');div.style.cssText='width:160px;height:160px;padding:20px;box-sizing:border-box;overflow:hidden';div.innerHTML=cell.art;sheet.append(div);for(const animation of div.getAnimations({subtree:true})){animation.pause();animation.currentTime=cell.age;}}
    },cells);
    const file='page-'+pages.length+'.png',screenshot=await page.locator('#native-emote-sheet').screenshot({omitBackground:false,animations:'allow'});
    // DevTools optimizes opaque screenshots to RGB; the shipped decoder accepts
    // RGBA8. Canvas re-encodes identical pixels in that explicit source format.
    const encoded=await page.evaluate(async data=>{const image=new Image();image.src='data:image/png;base64,'+data;await image.decode();const canvas=document.createElement('canvas');canvas.width=image.width;canvas.height=image.height;canvas.getContext('2d').drawImage(image,0,0);return canvas.toDataURL('image/png').split(',')[1];},screenshot.toString('base64'));
    const bytes=Buffer.from(encoded,'base64');if(bytes[25]!==6)throw Error('Reference is not RGBA8');fs.writeFileSync(path.join(directory,file),bytes);
    pages.push({file,sha256:hash(bytes),cells:cells.map(({art,...cell})=>cell)});
  }
  fs.writeFileSync(path.join(directory,'manifest.json'),JSON.stringify({sourceHash,clipCount:items.length,width:960,height:640,columns:6,tile:160,pages},null,2)+'\n');
}

module.exports={exportPreview,exportNativeReferences};
