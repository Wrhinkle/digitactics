// Export declarative source move specs and the original Canvas textures; runtime is native C#.
const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../..'),DT=require('../../tools/headless.cjs').load({strict:true});
new Function('window',fs.readFileSync(path.join(root,'js/moves.js'),'utf8').replace(/^export /gm,''))({DT});
const variants={};
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){
 DT.Sets.activate(set.id,pace);
 variants[`${set.id}/${pace}`]=Object.fromEntries(Object.values(DT.LINE).map(line=>[line.id,{type:line.type,radius:line.radius||0,
  moves:line.abil.map(name=>DT.Moves.specFor(name,line)),basic:[DT.Moves.basicFor(line,false),DT.Moves.basicFor(line,true)]}]));
}
const out=path.join(root,'native/content/move-effects');fs.mkdirSync(out,{recursive:true});
fs.writeFileSync(path.join(out,'specs.json'),JSON.stringify({schema:1,variants})+'\n');
(async()=>{
 const THREE=await import(require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
 const vfxSource=fs.readFileSync(path.join(root,'js/vfx.js'),'utf8');
 const geometrySource=vfxSource.slice(vfxSource.indexOf('const GEO ='),vfxSource.indexOf('// ---------- instanced quads'));
 const geos=new Function('THREE',geometrySource+';makeGeos();GEO.glow=new THREE.PlaneGeometry(1,1);return GEO;')(THREE),geometry={};
 for(const[name,g]of Object.entries(geos)){
  const vertices=[],p=g.attributes.position,n=g.attributes.normal,uv=g.attributes.uv;
  for(let i=0;i<p.count;i++)vertices.push(p.getX(i),p.getY(i),p.getZ(i),n?.getX(i)||0,n?.getY(i)||0,n?.getZ(i)||0,uv?.getX(i)||0,1-(uv?.getY(i)||0),0,0,0,0,0,0,0,0);
  geometry[name]={vertices,indices:g.index?Array.from(g.index.array):Array.from({length:p.count},(_,i)=>i)};
 }
 fs.writeFileSync(path.join(out,'geometry.json'),JSON.stringify(geometry)+'\n');
 const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  const source=fs.readFileSync(path.join(root,'js/vfx.js'),'utf8').split('// ---------- point sprites ----------')[0].replace(/^import .*;\r?\n/gm,'').replace(/^export /gm,'');
  const textures=await page.evaluate(source=>{
   const run=new Function('THREE',source+'\nmakeTextures();const result={atlas:atlasTexture().image.toDataURL()};for(const [name,texture] of Object.entries(TEX))result[name]=texture.image.toDataURL();return result;');
   return run({CanvasTexture:class{constructor(image){this.image=image;}},SRGBColorSpace:'srgb',LinearFilter:1006});
  },source);
  for(const[name,data]of Object.entries(textures))fs.writeFileSync(path.join(out,name+'.png'),Buffer.from(data.split(',')[1],'base64'));
  console.log(`Exported ${Object.keys(variants).length} source move variants and ${Object.keys(textures).length} original VFX textures.`);
 }finally{await browser.close();}
})().catch(error=>{console.error(error);process.exitCode=1;});
