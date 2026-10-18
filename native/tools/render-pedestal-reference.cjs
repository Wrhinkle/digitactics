// Optional original-browser reference. No browser code is a native runtime dependency.
// Usage: node native/tools/render-pedestal-reference.cjs [output.png] [--pedestal-only]
// --check validates extraction without starting a browser or using the GPU.
const fs = require('node:fs'), path = require('node:path'), http = require('node:http');
const root = path.resolve(__dirname, '../..');
const args = process.argv.slice(2);
const pedestalOnly = args.includes('--pedestal-only');
const output = path.resolve(args.find(arg => !arg.startsWith('--')) || path.join(root,
  `native/artifacts/pedestal-stage-source${pedestalOnly ? '-empty' : '-tokomon'}.png`));
const source = fs.readFileSync(path.join(root, 'js/client.js'), 'utf8').replace(/\r\n/g, '\n');
const start = source.indexOf('class Stage {'), end = source.indexOf('\nlet stage = null;', start);
if (start < 0 || end < 0) throw new Error('Original Stage / hexTexture source markers are missing.');
const stageSource = source.slice(start, end);
// Keep constructor, model setup, camera fit, ring pulse, and Canvas2D drawing unchanged.
new Function('THREE', 'DT', 'st', 'save', 'sfx', stageSource + '\nreturn Stage;');
if (args.includes('--check')) {
  console.log(`Validated original Stage and hexTexture extraction (${stageSource.length} characters); no browser started.`);
  process.exit(0);
}
const html = `<!doctype html><html><head><meta charset="utf-8">
<style>html,body{margin:0;padding:0}#host{width:384px;height:240px}canvas{display:block}</style>
<script type="importmap">{"imports":{"three":"/vendor/three/build/three.module.js","three/addons/":"/vendor/three/addons/"}}</script>
</head><body class="client-on"><div id="host"></div><script type="module">
import * as THREE from 'three';
try {
  const catalogs = new Map();
  window.DT = {COLS:7,ROWS:8,BENCH:9,LINE:{},TRAITS:{},WISPS:[],Cosmetics:{
    register(kind,items){catalogs.set(kind,items);},
    get(kind,id){return (catalogs.get(kind)||[]).find(item=>item.id===id);},
    list(kind){return catalogs.get(kind)||[];}
  }};
  const DT = window.DT, st = {module:false,covered:false}, save = {prefs:{motion:true}}, sfx = ()=>{};
  // Stage.frame itself is invoked once at simulation time zero, without a live RAF loop.
  window.requestAnimationFrame = ()=>0;
  window.cancelAnimationFrame = ()=>{};
  ${stageSource}
  const stage = new Stage();
  if (!stage.ok) throw new Error('Original Stage could not initialize WebGL.');
  if (!${pedestalOnly}) {
    await import('/js/legends.js');
    if (!await DT.Legends.modelFor('tokomon')) throw new Error('Original Tokomon GLB did not load.');
    stage.setLegend('tokomon');
    // The original LegendView attaches the cached template in its promise callback.
    await Promise.resolve();
    if (!stage.view.ready || stage.view.placeholder) throw new Error('Original Tokomon LegendView is not ready.');
  }
  stage.host = document.getElementById('host');
  stage.host.appendChild(stage.canvas);
  stage.last = 0;
  stage.frame(0);
  const gl = stage.r.getContext(), pixels = new Uint8Array(384*240*4);
  gl.readPixels(0,0,384,240,gl.RGBA,gl.UNSIGNED_BYTE,pixels);
  let visible = 0;
  for(let i=3;i<pixels.length;i+=4) if(pixels[i]>0) visible++;
  if (visible < 100) throw new Error('Original Stage reference is blank: '+visible+' visible pixels.');
  window.reference = {
    image:stage.canvas.toDataURL('image/png'),
    metadata:{width:stage.canvas.width,height:stage.canvas.height,time:stage.t,legend:${pedestalOnly ? 'null' : "'tokomon'"},
      visiblePixels:visible,camera:{position:stage.cam.position.toArray(),quaternion:stage.cam.quaternion.toArray(),
        fov:stage.cam.fov,aspect:stage.cam.aspect,near:stage.cam.near,far:stage.cam.far},
      ringColor:stage.ringMat.color.toArray(),pedestalMeshes:stage.ped.children.length,
      legendHeight:stage.view?.height??null,legendScale:stage.view?.visual.scale.toArray()??null,
      calls:stage.r.info.render.calls,triangles:stage.r.info.render.triangles}
  };
  stage.stop();
  stage.view?.dispose();
  stage.r.dispose();
  stage.r.forceContextLoss();
} catch(error) {window.referenceError=error.stack||String(error);}
</script></body></html>`;

(async () => {
  const server = http.createServer((req,res) => {
    const name = decodeURIComponent((req.url || '/').split('?')[0]);
    if (name === '/') {res.setHeader('Content-Type','text/html');res.end(html);return;}
    if (!/^\/(vendor|js|models)\//.test(name)) {res.writeHead(404).end();return;}
    const file = path.resolve(root,'.'+name);
    if (!file.startsWith(root+path.sep)) {res.writeHead(403).end();return;}
    const types = {'.js':'text/javascript','.json':'application/json','.glb':'model/gltf-binary','.png':'image/png','.jpg':'image/jpeg','.webp':'image/webp','.wasm':'application/wasm'};
    try {res.setHeader('Content-Type',types[path.extname(file)]||'application/octet-stream');res.end(fs.readFileSync(file));}
    catch {res.writeHead(404).end();}
  });
  let browser;
  try {
    await new Promise((resolve,reject)=>{server.once('error',reject);server.listen(0,'127.0.0.1',resolve);});
    const {chromium} = require('../artifacts/audio-tools/node_modules/playwright');
    browser = await chromium.launch({channel:'msedge',headless:true});
    const page = await browser.newPage({viewport:{width:384,height:240},deviceScaleFactor:1});
    page.on('pageerror',error=>console.error(error));
    await page.goto('http://127.0.0.1:'+server.address().port);
    // The harness disables Stage's RAF loop, so Playwright must poll on a timer.
    await page.waitForFunction(()=>window.reference||window.referenceError,null,{timeout:60000,polling:100});
    const result = await page.evaluate(()=>({reference:window.reference,error:window.referenceError}));
    if (result.error) throw new Error(result.error);
    fs.mkdirSync(path.dirname(output),{recursive:true});
    fs.writeFileSync(output,Buffer.from(result.reference.image.split(',')[1],'base64'));
    const metadataPath = output.replace(/\.png$/i,'')+'.json';
    fs.writeFileSync(metadataPath,JSON.stringify(result.reference.metadata,null,2)+'\n');
    console.log(output);
    console.log(JSON.stringify(result.reference.metadata));
  } finally {
    if(browser) await browser.close();
    await new Promise(resolve=>server.close(resolve));
  }
})().catch(error=>{console.error(error);process.exitCode=1;});
