// Bake the original procedural room geometry and canvas textures; runtime remains native.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 const source=fs.readFileSync(path.join(root,'js/farm-ui.js'),'utf8');
 const module="import * as THREE from '/vendor/three/build/three.module.js';const PI=Math.PI;const F={MEAT_MAX:3};const S={scene:new THREE.Scene(),world:null,ticks:[],meat:[]};function ensureScene(){}const $=()=>({style:{setProperty(){}}});"
  +source.slice(source.indexOf('const R ='),source.indexOf('// ---------- state ----------'))
  +source.slice(source.indexOf('function buildWorld('),source.indexOf('// ---------- legends in the room ----------'))
  +'\nexport {THREE,S,LOOK,buildWorld};';
 const browser=await chromium.launch({channel:'msedge',headless:true});
 try{
  const page=await browser.newPage();
  await page.route('http://farm.local/**',async route=>{
   const url=new URL(route.request().url());
   if(url.pathname==='/')return route.fulfill({contentType:'text/html',body:'<!doctype html>'});
   if(url.pathname==='/room.js')return route.fulfill({contentType:'text/javascript',body:module});
   const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));
   if(!file.startsWith(root+path.sep))return route.abort();
   return route.fulfill({contentType:'text/javascript',body:fs.readFileSync(file)});
  });
  await page.goto('http://farm.local/');
  const output=path.join(root,'native/content/farm-rooms');fs.mkdirSync(output,{recursive:true});
  for(const theme of ['meadow','tropical','snowy','sewer','toy','grid']){
   const room=await page.evaluate(async theme=>{
    const {THREE,S,LOOK,buildWorld}=await import('/room.js');buildWorld(theme);S.world.updateMatrixWorld(true);
    const materials=[],textures=[],meshes=[],points=[],materialIds=new Map(),textureIds=new Map();
    function material(m){
     if(materialIds.has(m))return materialIds.get(m);
     let texture=null;
     if(m.map){
      if(!textureIds.has(m.map)){
       const image=m.map.image,rgba=image.getContext('2d').getImageData(0,0,image.width,image.height).data;
       let data='';for(const byte of rgba)data+=String.fromCharCode(byte);
       textureIds.set(m.map,textures.length);textures.push({width:image.width,height:image.height,rgba:btoa(data),clamp:m.map.wrapS===THREE.ClampToEdgeWrapping});
      }texture=textureIds.get(m.map);
     }
     const id=materials.length;materialIds.set(m,id);
     materials.push({color:[m.color.r,m.color.g,m.color.b,m.opacity],texture,unlit:!!m.isMeshBasicMaterial||!!m.isSpriteMaterial,transparent:m.transparent,depthWrite:m.depthWrite});return id;
    }
    S.world.traverse(o=>{
     if(o.isPoints){points.push({positions:Array.from(o.geometry.attributes.position.array),matrix:o.matrixWorld.toArray()});return;}
     if(!o.isMesh&&!o.isSprite)return;
     const geometry=o.isSprite?new THREE.PlaneGeometry(1,1):o.geometry;
     const groups=Array.isArray(o.material)?geometry.groups:[{start:0,count:geometry.index?geometry.index.count:geometry.attributes.position.count,materialIndex:0}];
     for(const group of groups){
      const m=Array.isArray(o.material)?o.material[group.materialIndex]:o.material,p=geometry.attributes.position,n=geometry.attributes.normal,uv=geometry.attributes.uv,vertices=[];
      for(let i=0;i<p.count;i++){
       const u=(uv?.getX(i)||0)*(m.map?.repeat.x||1),v=(uv?.getY(i)||0)*(m.map?.repeat.y||1);
       vertices.push(p.getX(i),p.getY(i),p.getZ(i),n?.getX(i)||0,n?.getY(i)||0,n?.getZ(i)||0,u,m.map?.flipY?1-v:v,0,0,0,0,1,0,0,0);
      }
      const indices=Array.from({length:group.count},(_,i)=>geometry.index?geometry.index.getX(group.start+i):group.start+i);
      let meat=-1;for(let i=0;i<S.meat.length;i++){let parent=o;while(parent){if(parent===S.meat[i])meat=i;parent=parent.parent;}}
      meshes.push({vertices,indices,matrix:o.matrixWorld.toArray(),material:material(m),sprite:!!o.isSprite,meat});
     }
    });
    return {schemaVersion:1,theme,sky:LOOK[theme].sky,materials,textures,meshes,points};
   },theme);
   fs.writeFileSync(path.join(output,theme+'.json.gz'),zlib.gzipSync(JSON.stringify(room)));
   console.log(`${theme}: ${room.meshes.length} original room meshes, ${room.textures.length} canvas textures.`);
  }
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
