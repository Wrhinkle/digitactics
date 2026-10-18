const fs = require('fs'), path = require('path');
const { chromium } = require('../artifacts/audio-tools/node_modules/playwright');
const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'js/legendevo-ui.js'), 'utf8');
const theme=fs.readFileSync(path.join(root,'js/theme.js'),'utf8');
const reducedStart=theme.indexOf('body.reduce-motion :is(#overlay'),reducedEnd=theme.indexOf('}',reducedStart);
if(reducedStart<0||reducedEnd<0)throw Error('Missing source reduced-motion CSS.');
const fitStart=source.indexOf('function fitTree() {'),fitEnd=source.indexOf('\nfunction drawEdges()',fitStart);
if(fitStart<0||fitEnd<0)throw Error('Missing source evolution tree fit.');
const fit=Function('width','height','rows',`const result={};const box={clientWidth:width,clientHeight:height,querySelectorAll:()=>rows.map(n=>({querySelectorAll:()=>Array(n)})),style:{setProperty(k,v){result[k]=parseInt(v)}},classList:{toggle(k,v){result.small=v}}};const root={querySelector:()=>box};${source.slice(fitStart,fitEnd)};fitTree();return {width,height,rows,nodeWidth:result['--nw'],portraitSize:result['--np'],showNames:!result.small};`);
const trees=JSON.parse(fs.readFileSync(path.join(root,'native/tests/fixtures/evolution-trees.json'),'utf8'));
const layouts=[];for(const tree of trees)for(const width of [240,360,560,700,808,1100])for(const height of [180,240,360,500,640])layouts.push(fit(width,height,tree.tiers.map(row=>row.length)));
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-tree-layout.json'),JSON.stringify(layouts));
function rule(selector) {
  const start = source.indexOf(selector + ' {'), end = source.indexOf('}', start);
  if (start < 0 || end < 0) throw Error('Missing evolution announcement CSS: ' + selector);
  return source.slice(start, end + 1);
}
(async () => {
  const browser = await chromium.launch({ channel: 'msedge', headless: true, args: ['--disable-gpu'] });
  try {
    const page = await browser.newPage();
    await page.setContent(`<style>body{--gold2:#ffd36a;font:14px/1.35 system-ui,"Segoe UI",sans-serif}${rule('.dte-said')}${rule('.dte-said.on')}</style><div style="position:relative;width:180px;height:180px"><div class="dte-said">Bukamon digivolved into Veemon!</div></div>`);
    const fixture = await page.evaluate(async () => {
      const label = document.querySelector('.dte-said');
      const style = getComputedStyle(label);
      const result = { style: { fontSize: style.fontSize, fontWeight: style.fontWeight, color: style.color, textShadow: style.textShadow, bottom: style.bottom, padding: style.padding, textAlign: style.textAlign, lineHeight: style.lineHeight, transitionDuration: style.transitionDuration }, samples: [], reversal: [] };
      void label.offsetHeight;
      for (const showing of [true, false]) {
        label.classList.toggle('on', showing);
        const animations = label.getAnimations();
        if (animations.length !== 2) throw Error('Expected independent opacity and transform transitions.');
        animations.forEach(animation => animation.pause());
        for (const ms of [0, 1, 25, 50, 75, 100, 150, 200, 250, 299, 300]) {
          animations.forEach(animation => { animation.currentTime = ms; });
          const current = getComputedStyle(label);
          result.samples.push({ showing, time: ms / 1000, opacity: Number(current.opacity), slide: current.transform === 'none' ? 0 : new DOMMatrix(current.transform).m42 });
        }
        animations.forEach(animation => animation.finish());
        void label.offsetHeight;
      }
      label.classList.add('on');
      label.getAnimations().forEach(animation => { animation.pause(); animation.currentTime = 100; });
      void getComputedStyle(label).opacity;
      label.classList.remove('on');
      const reversed = label.getAnimations();
      reversed.forEach(animation => animation.pause());
      for (const ms of [0, 25, 50, 75, 100, 150, 200]) {
        reversed.forEach(animation => { animation.currentTime = ms; });
        const current = getComputedStyle(label);
        result.reversal.push({ time: ms / 1000, opacity: Number(current.opacity), slide: current.transform === 'none' ? 0 : new DOMMatrix(current.transform).m42 });
      }
      return result;
    });
    await page.addStyleTag({content:theme.slice(reducedStart,reducedEnd+1)});
    fixture.reduced=await page.evaluate(()=>{
      const label=document.querySelector('.dte-said');label.getAnimations().forEach(animation=>animation.finish());
      label.parentElement.id='dtEvo';document.body.classList.add('reduce-motion');
      const result={duration:getComputedStyle(label).transitionDuration,samples:[]};
      for(const showing of [true,false]){
        label.classList.toggle('on',showing);const animations=label.getAnimations();
        if(animations.length!==2)throw Error('Reduced motion must retain both announcement transitions.');
        animations.forEach(animation=>animation.pause());
        for(const ms of [0,.25,.5,.75,1,2]){
          animations.forEach(animation=>animation.currentTime=ms);const current=getComputedStyle(label);
          result.samples.push({showing,time:ms/1000,opacity:Number(current.opacity),slide:current.transform==='none'?0:new DOMMatrix(current.transform).m42});
        }
        animations.forEach(animation=>animation.finish());void label.offsetHeight;
      }
      return result;
    });
    const completion = source.match(/setTimeout\(\(\) => said\.classList\.remove\('on'\), (\d+)\)/);
    const action = source.match(/flashSay\.t = setTimeout\(\(\) => s\.classList\.remove\('on'\), (\d+)\)/);
    if (!completion || !action) throw Error('Evolution announcement lifetime source changed.');
    fixture.completionSeconds = Number(completion[1]) / 1000;
    fixture.actionSeconds = Number(action[1]) / 1000;
    fs.writeFileSync(path.join(root, 'native/tests/fixtures/evolution-announcement.json'), JSON.stringify(fixture, null, 2) + '\n');
    console.log(`Exported ${fixture.samples.length + fixture.reversal.length} original browser announcement transition samples and ${fixture.reduced.samples.length} reduced-motion samples.`);
  } finally { await browser.close(); }
})().catch(error => { console.error(error); process.exitCode = 1; });
