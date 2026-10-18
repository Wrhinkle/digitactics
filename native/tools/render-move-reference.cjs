// Optional visual comparison artifact using the original Three renderer, never a runtime dependency.
const fs=require('fs'),path=require('path'),http=require('http');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..'),output=path.resolve(process.argv[2]||path.join(root,'native/artifacts/move-effects-source.png'));
const legacySource=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8').replace(/\r\n/g,'\n');
const block=marker=>{const start=legacySource.indexOf(marker);if(start<0)throw new Error('Missing source marker: '+marker);const brace=legacySource.indexOf('{',start);let depth=1,end=brace+1;for(;depth&&end<legacySource.length;end++){if(legacySource[end]==='{')depth++;if(legacySource[end]==='}')depth--;}return legacySource.slice(brace+1,end-1);};
const legacyStart=legacySource.indexOf("        case 'beam': {",legacySource.indexOf('  consume(cb)')),legacyEnd=legacySource.indexOf('\n      }\n    }\n    // the cast',legacyStart);
if(legacyStart<0||legacyEnd<0)throw new Error('Missing source equipment cases.');
const html=`<!doctype html><style>body{margin:0;background:black}canvas{display:block}</style><script type="importmap">{"imports":{"three":"/vendor/three/build/three.module.js"}}</script><script>window.DT={stunDuration:s=>[1.5,1.75,2][s]};</script><script type="module">
import * as THREE from '/vendor/three/build/three.module.js';import {MoveFx} from '/js/vfx.js';import {PRIMS} from '/js/moveprims.js';
const r=new THREE.WebGLRenderer({antialias:false,preserveDrawingBuffer:true});r.setSize(1200,1200);r.setClearColor(0,1);r.autoClear=false;document.body.append(r.domElement);r.clear();window.equipmentChecks=[];
const camera=new THREE.PerspectiveCamera(.72*180/Math.PI,1,.1,100);camera.position.set(5,5,8);camera.lookAt(0,.8,0);
for(const[name,prim]of Object.entries(PRIMS)){
const index=Object.keys(PRIMS).indexOf(name),scene=new THREE.Scene(),fx=new MoveFx({scene,camera,canvas:{height:200}});let seed=84219+index;Math.random=()=>{let t=seed+=0x6D2B79F5;t=Math.imul(t^(t>>>15),t|1);t^=t+Math.imul(t^(t>>>7),t|61);return((t^(t>>>14))>>>0)/4294967296;};
scene.add(new THREE.AmbientLight(0xffffff,.35*Math.PI));const sun=new THREE.DirectionalLight(0xffffff,.65*Math.PI);sun.position.set(2,4,3);scene.add(sun);
const c={A:{x:-1.6,y:1.05,z:-.7},P:{x:-1.8,z:-1},C:{x:2.1,z:1.7},T:[{x:2.1,y:.8,z:1.7},{x:1.2,y:1.1,z:2.3}],h:1.65,R:1.8,S:1.3,stage:1,yaw:.84,ca:new THREE.Color('#e57630'),cb:new THREE.Color('#ffe9a4'),spec:{impact:'hit',shake:0}};
prim(fx,c,{},0);for(let i=0;i<15;i++)fx.update(1/60);r.setViewport(index%6*200,(5-Math.floor(index/6))*200,200,200);r.setScissor(index%6*200,(5-Math.floor(index/6))*200,200,200);r.setScissorTest(true);r.clear();r.render(scene,camera);fx.clear();
}
const GEO={${block('const GEO =')}},DT={ATTR:{Vaccine:{color:'#4fb3ff'}},hexDist:()=>0},cellWorld=(c,r)=>({x:c,z:r}),COLS=7,ROWS=8,sfx=()=>{};
function consume(e,from,to,cb){switch(e.type){${legacySource.slice(legacyStart,legacyEnd)}}}
function updateFx(now){${block('  updateFx(now)')}}
function addMeshFx(mesh,t0,dur,update){${block('  addMeshFx(mesh, t0, dur, update)')}}
for(const[type,offset]of [['beam',0],['burst',1],['shield',2]]){
const index=31+offset,scene=new THREE.Scene(),host={scene,fx:[],mirror:false,tiles:[],unitWorld:u=>u,viewTeam:t=>t,isUlt:()=>false,addMeshFx};
consume.call(host,{type,t:0,c:0,r:0,radius:2,team:0,at:0},{x:-1.8,z:-1,line:{attr:'Vaccine'}},{x:2.1,z:1.7},{units:[{x:0,z:0}]});updateFx.call(host,.25);
r.setViewport(index%6*200,(5-Math.floor(index/6))*200,200,200);r.setScissor(index%6*200,(5-Math.floor(index/6))*200,200,200);r.clear();r.render(scene,camera);
const gl=r.getContext(),pixels=new Uint8Array(200*200*4);gl.readPixels(index%6*200,(5-Math.floor(index/6))*200,200,200,gl.RGBA,gl.UNSIGNED_BYTE,pixels);let visible=0;for(let p=0;p<pixels.length;p+=4)if(pixels[p]+pixels[p+1]+pixels[p+2]>12)visible++;window.equipmentChecks.push({type,visible,meshes:host.fx.map(f=>({position:f.mesh.position.toArray(),scale:f.mesh.scale.toArray(),opacity:f.mesh.material.opacity}))});
}
window.result=r.domElement.toDataURL('image/png');</script>`;
(async()=>{
 const server=http.createServer((req,res)=>{
  if(req.url==='/'){res.setHeader('Content-Type','text/html');res.end(html);return;}
  const name=decodeURIComponent(req.url.split('?')[0]);if(!/^\/(vendor|js)\//.test(name)){res.writeHead(404).end();return;}
  const file=path.resolve(root,'.'+name);if(!file.startsWith(root+path.sep)){res.writeHead(403).end();return;}
  try{res.setHeader('Content-Type','text/javascript');res.end(fs.readFileSync(file));}catch{res.writeHead(404).end();}
 });await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
 const browser=await chromium.launch({channel:'msedge',headless:true});try{const page=await browser.newPage();page.on('pageerror',e=>console.error(e));await page.goto('http://127.0.0.1:'+server.address().port);await page.waitForFunction(()=>window.result,{timeout:60000});const data=await page.evaluate(()=>({image:window.result,equipment:window.equipmentChecks}));for(const cue of data.equipment)if(cue.visible<4)throw new Error('Original equipment cue did not render: '+JSON.stringify(cue));fs.mkdirSync(path.dirname(output),{recursive:true});fs.writeFileSync(output,Buffer.from(data.image.split(',')[1],'base64'));console.log(output);console.log(JSON.stringify(data.equipment));}finally{await browser.close();server.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
