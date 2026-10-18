// Original board assets. Dynamic objects and tick counts remain explicit for native animation integration.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 const source=fs.readFileSync(path.join(root,'js/boards.js'),'utf8'),render=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
 const rng=require('../../tools/headless.cjs').load({strict:true}).makeRng.toString();
 const layout=render.slice(render.indexOf('const SQ3'),render.indexOf('// placeholder color:')).replace('export const LAYOUT','const LAYOUT');
 const module="import * as THREE from '/vendor/three/build/three.module.js';import {mergeGeometries} from '/vendor/three/addons/utils/BufferGeometryUtils.js';import {extraThemes} from '/js/boards-extra.js';import {mysteryBoards} from '/js/mystery-boards.js';"
  +`const DT={BENCH:9,CELLS:Array.from({length:56},(_,i)=>({c:i%7,r:Math.floor(i/7)})),makeRng:${rng}},COLS=7,ROWS=8,BENCH=9;const LAYOUT=(()=>{`+layout+';return LAYOUT;})();'
  +source.slice(source.indexOf('const { cellWorld'),source.indexOf('// ---------- renderer integration ----------'))
  +'\nexport {THREE,THEMES,buildEnv,disposeEnv};';
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.route('http://boards.local/**',async route=>{
   const url=new URL(route.request().url());
   if(url.pathname==='/')return route.fulfill({contentType:'text/html',body:'<!doctype html><script type="importmap">{"imports":{"three":"/vendor/three/build/three.module.js"}}</script>'});
   if(url.pathname==='/boards.js')return route.fulfill({contentType:'text/javascript',body:module});
   const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));if(!file.startsWith(root+path.sep))return route.abort();
   return route.fulfill({contentType:'text/javascript',body:fs.readFileSync(file)});
  });
  await page.goto('http://boards.local/');
  const ids=await page.evaluate(async()=> (await import('/boards.js')).THEMES.map(t=>t.id));
  const folder=path.join(root,'native/content/boards');fs.mkdirSync(folder,{recursive:true});const manifest=[];
  for(const id of ids){
   const scene=await page.evaluate(async id=>{
    const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js');const spec=THEMES.find(t=>t.id===id),env=buildEnv(spec);
    env.signal({phase:'planning',stage:1,won:false,evo:0});env.tick(0,0);env.group.updateMatrixWorld(true);
    const textures=[],materials=[],geometries=[],objects=[],textureIds=new Map(),materialIds=new Map(),geometryIds=new Map();
    function texture(t){
     if(!t)return null;if(textureIds.has(t))return textureIds.get(t);
     const image=t.image,rgba=image.getContext('2d').getImageData(0,0,image.width,image.height).data;
     let binary='';for(const b of rgba)binary+=String.fromCharCode(b);
     const index=textures.length;textureIds.set(t,index);textures.push({width:image.width,height:image.height,rgba:btoa(binary),flipY:t.flipY,repeat:t.repeat.toArray(),offset:t.offset.toArray(),center:t.center.toArray(),rotation:t.rotation,wrapS:t.wrapS,wrapT:t.wrapT,colorSpace:t.colorSpace});return index;
    }
    function material(m){
     if(materialIds.has(m))return materialIds.get(m);const index=materials.length;materialIds.set(m,index);
     materials.push({color:[...m.color.toArray(),m.opacity],map:texture(m.map),unlit:!!m.isMeshBasicMaterial||!!m.isPointsMaterial||!!m.isSpriteMaterial||!!m.isLineBasicMaterial,transparent:m.transparent,depthWrite:m.depthWrite,side:m.side,fog:m.fog,flatShading:!!m.flatShading,alphaTest:m.alphaTest,
      emissive:m.emissive?.toArray()||[0,0,0],emissiveIntensity:m.emissiveIntensity||0,roughness:m.roughness??1,metalness:m.metalness??0,vertexColors:m.vertexColors,wireframe:!!m.wireframe,blending:m.blending,size:m.size??1,sizeAttenuation:m.sizeAttenuation??true});return index;
    }
    function geometry(g){
     if(geometryIds.has(g))return geometryIds.get(g);const index=geometries.length;geometryIds.set(g,index);
     const arrays={};for(const key of ['position','normal','uv','color']){const a=g.attributes[key];if(a)arrays[key]=Array.from(a.array);}
     geometries.push({...arrays,indices:g.index?Array.from(g.index.array):null,groups:g.groups});return index;
    }
    env.group.traverse(o=>{
     if(!o.isMesh&&!o.isPoints&&!o.isSprite&&!o.isLine)return;
     const g=o.isSprite?new THREE.PlaneGeometry(1,1):o.geometry;
     let visible=true;for(let parent=o;parent;parent=parent.parent)visible=visible&&parent.visible;
     const base={kind:o.isPoints?'points':o.isSprite?'sprite':o.isLineSegments?'lines':o.isLine?'lineStrip':'mesh',geometry:geometry(g),materials:[].concat(o.material).map(material),matrix:o.matrixWorld.toArray(),visible,castShadow:o.castShadow,receiveShadow:o.receiveShadow};
     if(o.isInstancedMesh){base.instances=[];const m=new THREE.Matrix4(),color=new THREE.Color();for(let i=0;i<o.count;i++){o.getMatrixAt(i,m);if(o.instanceColor)o.getColorAt(i,color);base.instances.push({matrix:m.toArray(),color:color.toArray()});}}
     objects.push(base);
    });
    const result={version:1,id,sky:Array.isArray(spec.sky)?spec.sky:[spec.sky],fog:spec.fog,hemi:spec.hemi,sun:spec.sun,tiles:spec.tiles,slot:spec.slot,ped:spec.ped,look:spec.look||{},surface:Object.fromEntries(Object.entries(env.surface).map(([k,v])=>[k,texture(v)])),tickCount:env.ticks.length,textures,materials,geometries,objects};
    disposeEnv(env);return result;
   },id);
   const file=id+'.json.gz',bytes=zlib.gzipSync(JSON.stringify(scene));fs.writeFileSync(path.join(folder,file),bytes);
   manifest.push({id,file,objects:scene.objects.length,instances:scene.objects.reduce((n,o)=>n+(o.instances?.length||1),0),geometries:scene.geometries.length,textures:scene.textures.length,ticks:scene.tickCount});
  }
  fs.writeFileSync(path.join(folder,'manifest.json'),JSON.stringify({version:1,themes:manifest}));
  const uvFixture=await page.evaluate(async()=>{
   const {THREE}=await import('/boards.js');
   return Array.from({length:120},(_,i)=>{
    const t=new THREE.Texture();t.repeat.set(.25+i%7,.5+i%5);t.offset.set((i%9-4)/7,(i%11-5)/9);t.center.set((i%4)/3,(i%6)/5);t.rotation=i*.17;t.updateMatrix();
    const uv=[(i%13-6)/4,(i%17-8)/5],expected=new THREE.Vector2(...uv).applyMatrix3(t.matrix).toArray();
    return {repeat:t.repeat.toArray(),offset:t.offset.toArray(),center:t.center.toArray(),rotation:t.rotation,uv,expected};
   });
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-uv.json'),JSON.stringify(uvFixture));
  console.log(`Exported ${manifest.length} board themes, ${manifest.reduce((n,t)=>n+t.instances,0)} scene instances; animation tick counts retained for native port.`);
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
