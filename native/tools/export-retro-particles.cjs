const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/emotes.js'),'utf8');
(async()=>{
 const THREE=await import(require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
 const cases=[];
 for(const act of ['hug','cane','ufo','shiver','flex','float','swoop','salute','stomp']){
  let randomIndex=0;const math=Object.create(Math);math.random=()=>((randomIndex++*73+19)%997)/997;
  const pops=new Map(),camera={position:new THREE.Vector3(0,16,18),matrixWorld:new THREE.Matrix4()};
  const ctx={THREE,Math:math,pops,POP_MS:2500,PUFF:new THREE.SphereGeometry(.09,10,8),glyphTexture:()=>new THREE.Texture(),rend:()=>({camera}),clearPop:s=>pops.delete(s)};vm.createContext(ctx);
  vm.runInContext(source.slice(source.indexOf('function addFx('),source.indexOf('// Screen point above')),ctx);
  const view={group:new THREE.Object3D()},holder=new THREE.Object3D(),obj=new THREE.Object3D(),scene=new THREE.Group();scene.add(view.group,holder);obj.userData={s:.7,rotY:.37};
  const glyph=!['ufo','swoop','stomp'].includes(act),p={act,view,holder,obj,t0:0,fx:[],nextFx:0,glyph:glyph?{ch:'!',color:'#ffffff'}:null};pops.set(0,p);
  const steps=[];let now=0;
  for(let i=0;i<210;i++){
   now+=i%31===0?50:1000/60;view.group.position.set(Math.sin(i*.03),.2,Math.cos(i*.03));ctx.updatePops(now);
   steps.push({seconds:now/1000,companion:view.group.position.toArray(),particles:pops.has(0)?p.fx.map(f=>({puff:!!f.puff,position:f.o.position.toArray(),opacity:f.o.material.opacity,scale:f.o.scale.x})):[]});
  }
  cases.push({act,glyph,steps});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/retro-particles.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
 const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();await page.setContent('<body></body>');
  const clips=JSON.parse(fs.readFileSync(path.join(root,'native/content/emotes/manifest.json'))).clips,out=path.join(root,'native/content/retro');fs.mkdirSync(out,{recursive:true});
  for(const [id,clip] of Object.entries(clips))if(clip.data.retro?.glyph){
   const png=await page.evaluate(({code,glyph})=>new Function('glyph',`const OL='#1b1424',glyphTex=new Map(),THREE={CanvasTexture:class{constructor(c){this.canvas=c}},SRGBColorSpace:0};${code};return glyphTexture(glyph.ch,glyph.color).canvas.toDataURL('image/png');`)(glyph),{code:source.slice(source.indexOf('function glyphTexture('),source.indexOf('function clearPop(')),glyph:clip.data.retro.glyph});
   fs.writeFileSync(path.join(out,id+'.png'),Buffer.from(png.split(',')[1],'base64'));
  }
  function mesh(g){const p=g.attributes.position,n=g.attributes.normal,uv=g.attributes.uv,vertices=[];for(let i=0;i<p.count;i++)vertices.push(p.getX(i),p.getY(i),p.getZ(i),n.getX(i),n.getY(i),n.getZ(i),uv.getX(i),uv.getY(i),0,0,0,0,1,0,0,0);return {vertices,indices:Array.from(g.index.array)};}
  fs.writeFileSync(path.join(out,'geometry.json'),JSON.stringify({puff:mesh(new THREE.SphereGeometry(.09,10,8)),beam:mesh(new THREE.CylinderGeometry(.12,.5,1,20,1,true)),glyph:mesh(new THREE.PlaneGeometry(1,1))}));
 }finally{await browser.close();}
 console.log('Exported original retro particle traces, glyph canvases and geometry.');
})().catch(e=>{console.error(e);process.exitCode=1;});
