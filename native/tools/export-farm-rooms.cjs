// Bake the original procedural room geometry and canvas textures; runtime remains native.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 let source=fs.readFileSync(path.join(root,'js/farm-ui.js'),'utf8');
 const lightBody=source.slice(source.indexOf('  const k = Math.min(1, dt * 3);'),source.indexOf('  for (const fn of S.ticks)'));
 const lightTick=new Function('S','LOOK','dt',lightBody);
 const lightState={hemi:{intensity:1.7},sun:{intensity:2.3},lamp:{intensity:0},lampBulb:{material:{}}},lightFrames=[];
 for(const theme of ['meadow','grid','sewer','snowy'])for(const night of [false,true,false])for(let i=0;i<90;i++){
  lightState.theme=theme;lightState.night=night;const delta=[1/60,.05,.1][i%3];lightTick(lightState,{[theme]:{deco:theme}},delta);
  lightFrames.push({theme,night,delta,hemisphere:lightState.hemi.intensity,sun:lightState.sun.intensity,lamp:lightState.lamp.intensity,bulb:lightState.lampBulb.material.emissiveIntensity});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-lighting.json.gz'),zlib.gzipSync(JSON.stringify(lightFrames)));
 for(const [needle,tag] of [
  ['S.ticks.push((dt, t) => { water.position',"water.userData.motion={kind:'water',phase:0};"],
  ['S.ticks.push((dt, t) => { p.rotation',"p.userData.motion={kind:'palm',phase:x};"],
  ['S.ticks.push((dt, t) => drips',"drips.forEach((d,i)=>d.userData.motion={kind:'drip',phase:i});"],
  ['S.ticks.push((dt, t) => crystals',"crystals.forEach((c,i)=>c.userData.motion={kind:'crystal',phase:i});"]
 ]){if(!source.includes(needle))throw Error('Missing room motion: '+needle);source=source.replace(needle,tag+needle);}
 const module="import * as THREE from '/vendor/three/build/three.module.js';const PI=Math.PI;const F={MEAT_MAX:3};const S={scene:new THREE.Scene(),world:null,ticks:[],meat:[]};function ensureScene(){}const $=()=>({style:{setProperty(){}}});"
  +source.slice(source.indexOf('const R ='),source.indexOf('// ---------- state ----------'))
  +source.slice(source.indexOf('function buildWorld('),source.indexOf('// ---------- legends in the room ----------'))
  +source.slice(source.indexOf('function pileMesh()'),source.indexOf('function syncPiles()'))
  +'\nexport {THREE,S,LOOK,buildWorld,foodMesh,pileMesh};';
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
  for(const theme of ['meadow','tropical','snowy','sewer','toy','grid','food-meat','food-carrot','food-bigmeat','food-sirloin','food-cake','pile']){
   const room=await page.evaluate(async theme=>{
    const {THREE,S,LOOK,buildWorld,foodMesh,pileMesh}=await import('/room.js');buildWorld(theme.startsWith('food-')||theme==='pile'?'meadow':theme);
    if(theme.startsWith('food-')){S.world=new THREE.Group();S.world.add(foodMesh(theme.slice(5)));S.meat=[];S.ticks=[];}
    if(theme==='pile'){S.world=pileMesh();S.world.remove(S.world.userData.hit);S.meat=[];S.ticks=[];}
    S.world.updateMatrixWorld(true);
    const materials=[],textures=[],meshes=[],points=[],materialIds=new Map(),textureIds=new Map(),animated=[];
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
     materials.push({color:[m.color.r,m.color.g,m.color.b,m.opacity],texture,unlit:!!m.isMeshBasicMaterial||!!m.isSpriteMaterial,transparent:m.transparent,depthWrite:m.depthWrite,emissive:m.emissive?.toArray()||[0,0,0],emissiveIntensity:m.emissiveIntensity||0,bulb:m===S.lampBulb.material,roughness:m.roughness??1,metalness:m.metalness??0,side:m.side,shadowSide:m.shadowSide});return id;
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
      let ancestor=o;while(ancestor&&!ancestor.userData.motion)ancestor=ancestor.parent;
      let motion=null;
      if(ancestor){
       motion={...ancestor.userData.motion,position:ancestor.position.toArray(),rotation:[ancestor.rotation.x,ancestor.rotation.y,ancestor.rotation.z],scale:ancestor.scale.toArray(),parent:ancestor.parent.matrixWorld.toArray(),relative:ancestor.matrixWorld.clone().invert().multiply(o.matrixWorld).toArray()};
       animated.push({index:meshes.length,object:o});
      }
      meshes.push({vertices,indices,matrix:o.matrixWorld.toArray(),material:material(m),sprite:!!o.isSprite,meat,motion,castShadow:o.castShadow,receiveShadow:o.receiveShadow});
     }
    });
    const snowChecks=[];
    if(points.length){
     let time=0;
     for(let frame=0;frame<700;frame++){
      const delta=[1/60,.05,.1][frame%3];time+=delta;
      for(const tick of S.ticks)tick(delta,time);
      const snapshots=[];S.world.traverse(o=>{if(o.isPoints)snapshots.push(Array.from(o.geometry.attributes.position.array));});
      snowChecks.push({delta,time,positions:snapshots});
     }
    }
    const motionChecks=[];
    for(const time of [0,.05,.5,1,3,17,123]){for(const tick of S.ticks)tick(.05,time);S.world.updateMatrixWorld(true);motionChecks.push({time,meshes:animated.map(a=>({index:a.index,matrix:a.object.matrixWorld.toArray()}))});}
    return {schemaVersion:2,theme,sky:(LOOK[theme]||LOOK.meadow).sky,materials,textures,meshes,points,motionChecks,snowChecks};
   },theme);
   if(room.snowChecks.length)fs.writeFileSync(path.join(root,'native/tests/fixtures/farm-snow.json.gz'),zlib.gzipSync(JSON.stringify({initial:room.points.map(p=>p.positions),frames:room.snowChecks})));
   delete room.snowChecks;
   fs.writeFileSync(path.join(output,theme+'.json.gz'),zlib.gzipSync(JSON.stringify(room)));
   console.log(`${theme}: ${room.meshes.length} original room meshes, ${room.textures.length} canvas textures.`);
  }
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e.message);process.exitCode=1;});
