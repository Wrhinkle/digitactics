// Original board assets. Dynamic objects and tick counts remain explicit for native animation integration.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 const source=fs.readFileSync(path.join(root,'js/boards.js'),'utf8'),render=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
 const engine=require('../../tools/headless.cjs').load({strict:true}),rng=engine.makeRng.toString(),distance=engine.hexDist.toString();
 const layout=render.slice(render.indexOf('const SQ3'),render.indexOf('// placeholder color:')).replace('export const LAYOUT','const LAYOUT');
 const module="import * as THREE from '/vendor/three/build/three.module.js';import {mergeGeometries} from '/vendor/three/addons/utils/BufferGeometryUtils.js';import {extraThemes} from '/js/boards-extra.js';import {mysteryBoards} from '/js/mystery-boards.js';"
  +`const DT={BENCH:9,CELLS:Array.from({length:56},(_,i)=>({c:i%7,r:Math.floor(i/7)})),makeRng:${rng},hexDist:${distance}},COLS=7,ROWS=8,BENCH=9;const LAYOUT=(()=>{`+layout+';return LAYOUT;})();'
  +source.slice(source.indexOf('const { cellWorld'),source.indexOf('// ---------- renderer integration ----------'))
   .replace('const base = Float32Array.from(pos.array);','const base = Float32Array.from(pos.array); m.userData.nativeMotion={kind:"sea",amp,speed};')
   .replace('const pts = new THREE.Points(geo, mat);','const pts = new THREE.Points(geo, mat); pts.userData.nativeMotion={kind:"particles",box,velocity:v,wobble,speeds:Array.from(spd)};')
   .replace(/const cm = inst\(ctx, new THREE.OctahedronGeometry\(0.45, 0\)[^\n]+;/,line=>line+' cm.userData.nativeMotion={kind:"island-crystals",items:crystals.map(c=>({...c}))};')
   .replace(/const foam = mesh\(ctx, new THREE.RingGeometry\(13.4[^\n]+;/,line=>line+' foam.updateMatrix(); foam.userData.nativeMotion={kind:"beach-surf",matrix:foam.matrix.toArray()};')
   .replace(/const pm = inst\(ctx, palmGeo\(\)[^\n]+;/,line=>line+' pm.userData.nativeMotion={kind:"beach-palms",palms:palms.map(p=>({...p}))};')
   .replace(/const em = inst\(ctx, eggGeo[^\n]+;/,line=>line+' em.userData.nativeMotion={kind:"village-eggs",props:eggs.map(p=>({...p}))};')
   .replace(/const bm = inst\(ctx, new THREE.BoxGeometry\(1, 1, 1\)[^\n]+;/,line=>line+' bm.userData.nativeMotion={kind:"village-blocks",props:blocks.map(p=>({...p}))};')
   .replace(/const tm = inst\(ctx, top,[^\n]+;/,line=>line+' tm.userData.nativeMotion={kind:"toy-tops",props:tops.map(p=>({...p}))};')
   .replace(/const trm = inst\(ctx, car,[^\n]+;/,line=>line+' trm.userData.nativeMotion={kind:"toy-train",props:cars.map(p=>({...p}))};')
   .replace(/const band = mesh\(ctx, new THREE.CylinderGeometry\(26, 26[^\n]+;/,line=>line+' band.userData.nativeMotion={kind:"desert-haze"};')
   .replace('ctx.g.add(stars);','ctx.g.add(stars); stars.userData.nativeMotion={kind:"mountain-stars"};')
   .replace('beam.add(b1, b2);','beam.add(b1, b2); beam.updateMatrix(); for(const child of [b1,b2]) { child.updateMatrix(); child.userData.nativeMotion={kind:"rotating-parent",speed:.7,matrix:child.matrix.toArray(),parent:beam.matrix.toArray()}; }')
   .replace('gears.push({ g, list });','gears.push({ g, list }); g.userData.nativeMotion={kind:"savanna-gears",props:list.map(p=>({...p}))};')
   .replace(/const pm = inst\(ctx, new THREE.IcosahedronGeometry\(1, 0\)[^\n]+;/,line=>line+' pm.userData.nativeMotion={kind:"grid-polys",props:polys.map(p=>({...p}))};')
   .replace(/const rm = inst\(ctx, ring,[^\n]+;/,line=>line+' rm.userData.nativeMotion={kind:"grid-rings"};')
   .replace('const floor = plain(ctx, new THREE.MeshBasicMaterial({ map: grid }));','const floor = plain(ctx, new THREE.MeshBasicMaterial({ map: grid })); floor.userData.nativeMotion={kind:"grid-floor"};')
   .replace(/const bm = inst\(ctx, bGeo,[^\n]+;/,line=>line+' bm.userData.nativeMotion={kind:"coliseum-banners",props:banners.map(p=>({...p}))};')
   .replace(/const fm = inst\(ctx, new THREE.ConeGeometry\(0.16, 0.45, 8\)[^\n]+;/,line=>line+' fm.userData.nativeMotion={kind:"coliseum-flames",props:flames.map(p=>({...p}))};')
  +'\nexport {THREE,THEMES,buildEnv,disposeEnv,applySurface,tileLook};'
  +'\nexport function surfaceGeometry(){'+['tileGeo','slotGeo','pedGeo'].map(name=>render.match(new RegExp('const '+name+' = [^;]+;'))[0]).join('\n')+'return {tile:tileGeo,slot:slotGeo,ped:pedGeo};}'
  +'\nexport const originalHighlight='+render.slice(render.indexOf('  highlight(ui, planning) {'),render.indexOf('  benchPos(i, partner')).trim().replace('highlight(','function(')+';'
  +'\nexport const originalDrawItems='+render.slice(render.indexOf('  drawItems(p, ui) {'),render.indexOf('  // which item token')).trim().replace('drawItems(','function(')+';'
  +'\nexport function originalBurst(e){'+render.slice(render.indexOf('const col = new THREE.Color(this.viewTeam(e.team)'),render.indexOf('const c = cellWorld(at.c, at.r);',render.indexOf('const col = new THREE.Color(this.viewTeam(e.team)')))+'}';
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
  const surfaces=await page.evaluate(async()=>{
   const {surfaceGeometry}=await import('/boards.js'),result={version:1};
   for(const [key,g] of Object.entries(surfaceGeometry())){
    const data={};for(const name of ['position','normal','uv'])data[name]=Array.from(g.attributes[name].array);
    result[key]={...data,indices:g.index?Array.from(g.index.array):null,groups:g.groups};g.dispose();
   }
   return result;
  });
  fs.writeFileSync(path.join(root,'native/content/board-surfaces.json'),JSON.stringify(surfaces));
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
     const base={kind:o.isPoints?'points':o.isSprite?'sprite':o.isLineSegments?'lines':o.isLine?'lineStrip':'mesh',geometry:geometry(g),materials:[].concat(o.material).map(material),matrix:o.matrixWorld.toArray(),visible,castShadow:o.castShadow,receiveShadow:o.receiveShadow,motion:o.userData.nativeMotion||null};
     if(o.isInstancedMesh){base.instances=[];const m=new THREE.Matrix4(),color=new THREE.Color();for(let i=0;i<o.count;i++){o.getMatrixAt(i,m);if(o.instanceColor)o.getColorAt(i,color);base.instances.push({matrix:m.toArray(),color:color.toArray()});}}
     objects.push(base);
    });
    const result={version:1,id,background:env.background.isTexture?texture(env.background):null,sky:Array.isArray(spec.sky)?spec.sky:[spec.sky],fog:spec.fog,hemi:spec.hemi,sun:spec.sun,tiles:spec.tiles,slot:spec.slot,ped:spec.ped,look:spec.look||{},surface:Object.fromEntries(Object.entries(env.surface).map(([k,v])=>[k,texture(v)])),tickCount:env.ticks.length,textures,materials,geometries,objects};
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
  const lightingFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),rows=[];
   for(const spec of THEMES){
    let controls;
    const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)};
    const scene={fog:new THREE.Fog(...spec.fog)};
    const env=buildEnv({...spec,props:ctx=>{controls=ctx;},surface:null},{L,scene});
    for(let i=0;i<12;i++){
     const input={hemi:.3+i*.11,sun:.1+i*.17,flash:i*.3,tintK:i/11,tint:'#469ac2',flashColor:'#f6a74d',dt:i%3===0?.4:1/60};
     Object.assign(controls.light(),input,{tint:new THREE.Color(input.tint),flashColor:new THREE.Color(input.flashColor)});
     env.tick(input.dt,i);
     rows.push({id:spec.id,input,sky:L.hemi.color.toArray(),ground:L.hemi.groundColor.toArray(),sun:L.sun.color.toArray(),hemiIntensity:L.hemi.intensity,sunIntensity:L.sun.intensity,flash:controls.light().flash});
    }
    disposeEnv(env);
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-lighting.json'),JSON.stringify(lightingFixture));
  const surfaceFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv,applySurface,tileLook}=await import('/boards.js');
   return THEMES.map(spec=>{
    const env=buildEnv(spec),look=tileLook(spec);
    const mine=new THREE.MeshStandardMaterial({color:look.mine,emissive:look.glowMine});
    const enemy=new THREE.MeshStandardMaterial({color:look.enemy,emissive:look.glowEnemy});
    const enemyPlanning=new THREE.MeshStandardMaterial({color:look.enemyPlan,emissive:look.glowEnemy});
    const slot=new THREE.MeshStandardMaterial(),ped=new THREE.MeshStandardMaterial();
    applySurface(spec,env,{tiles:[mine,enemy,enemyPlanning],slots:[slot],peds:[ped]});
    const materials=Object.entries({mine,enemy,enemyPlanning,slot,ped}).map(([kind,m])=>({kind,color:[...m.color.toArray(),m.opacity],emissive:m.emissive.toArray(),roughness:m.roughness,metalness:m.metalness,map:!!m.map}));
    for(const m of [mine,enemy,enemyPlanning,slot,ped])m.dispose();disposeEnv(env);return {id:spec.id,materials};
   });
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-surface-materials.json'),JSON.stringify(surfaceFixture));
  const feedbackFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,tileLook,originalHighlight,originalDrawItems}=await import('/boards.js'),rows=[];
   for(const spec of THEMES)for(let test=0;test<12;test++){
    const planning=test%2===0,wide=test>=10,time=7.3;
    const ui={drag:{moved:test%3!==0},hoverCell:{c:test%7,r:test%8},hoverBench:test%9,hoverItem:test%10,itemDrag:test%4===0?{over:(test+1)%10}:null};
    const tiles=Array.from({length:56},(_,i)=>({userData:{c:i%7,r:Math.floor(i/7),flash:7+(i%7)*.15,flashColor:new THREE.Color(i%2?'#6fd3ff':'#ff7a6a')},material:new THREE.MeshStandardMaterial()}));
    const slots=Array.from({length:9},()=>({material:new THREE.MeshStandardMaterial()}));
    const peds=Array.from({length:10},()=>({material:new THREE.MeshStandardMaterial(),visible:true}));
    const renderer={tiles,slots,tileLook:tileLook(spec),fxNow:time,wide,itemPeds:peds,itemTokens:Array.from({length:10},()=>({visible:false}))};
    originalHighlight.call(renderer,ui,planning);originalDrawItems.call(renderer,{items:[]},ui);
    rows.push({id:spec.id,test,planning,wide,time,ui,tiles:tiles.map(t=>t.material.emissive.toArray()),slots:slots.map(s=>s.material.emissive.toArray()),peds:peds.map(p=>({visible:p.visible,emissive:p.material.emissive.toArray()}))});
    for(const obj of [...tiles,...slots,...peds])obj.material.dispose();
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-feedback.json.gz'),zlib.gzipSync(JSON.stringify(feedbackFixture)));
  const bursts=await page.evaluate(async()=>{
   const {THREE,originalBurst}=await import('/boards.js'),rows=[];
   for(const columns of [7,14])for(const mirror of [false,true])for(let r=0;r<8;r++)for(let c=0;c<columns;c++)for(let radius=0;radius<4;radius++)for(let team=0;team<2;team++){
    const cells=[];for(let y=0;y<8;y++)for(let x=0;x<7;x++)cells.push({c:x,r:y});
    for(let y=0;y<8;y++)for(let x=7;x<columns;x++)cells.push({c:x,r:y});
    const tiles=cells.map(p=>({userData:{...p,flash:0,flashColor:new THREE.Color()}})),e={c,r,radius,team,t:3.25};
    originalBurst.call({tiles,mirror,combatCols:columns,viewTeam:t=>mirror?1-t:t},e);
    rows.push({columns,mirror,event:e,flashes:tiles.filter(t=>t.userData.flash>0).map(t=>({c:t.userData.c,r:t.userData.r,until:t.userData.flash,color:t.userData.flashColor.toArray()}))});
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-bursts.json.gz'),zlib.gzipSync(JSON.stringify(bursts)));
  const motionFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),rows=[];
   for(const spec of THEMES){
    const env=buildEnv(spec),moving=[];env.group.traverse(o=>{if(o.userData.nativeMotion)moving.push(o);});
    for(const time of [0,.016,.37,1,2.7,10,31.25,120]){
     env.tick(.016,time);env.group.updateMatrixWorld(true);
     for(const o of moving){
      const motion=o.userData.nativeMotion;
      if(motion.kind==='sea'){
       const a=o.geometry.attributes.position.array,vertices=[];
       for(let i=0;i<a.length/3;i+=13)vertices.push([a[i*3],a[i*3+2],a[i*3+1]]);
       rows.push({id:spec.id,time,motion,vertices});
      }else if(motion.kind==='island-crystals'||motion.kind==='beach-palms'||motion.props){
       const matrix=new THREE.Matrix4(),matrices=[];for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
       rows.push({id:spec.id,time,motion,matrices});
      }else if(motion.kind==='beach-surf'||motion.kind==='rotating-parent'){
       rows.push({id:spec.id,time,motion,matrix:o.matrixWorld.toArray(),opacity:o.material.opacity});
      }else if(motion.kind==='desert-haze'||motion.kind==='grid-floor'){
       const texture=o.material.map;texture.updateMatrix();const uv=o.geometry.attributes.uv,samples=[];
       for(let i=0;i<uv.count;i++) {const source=[uv.getX(i),uv.getY(i)];samples.push({source,expected:new THREE.Vector2(...source).applyMatrix3(texture.matrix).toArray()});}
       rows.push({id:spec.id,time,motion,samples,opacity:o.material.opacity});
      }else if(motion.kind==='mountain-stars'||motion.kind==='grid-rings'){
       rows.push({id:spec.id,time,motion,opacity:o.material.opacity});
      }
     }
    }
    disposeEnv(env);
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-motion.json.gz'),zlib.gzipSync(JSON.stringify(motionFixture)));
  const particleFixture=await page.evaluate(async()=>{
   const {THEMES,buildEnv,disposeEnv}=await import('/boards.js'),rows=[];
   for(const spec of THEMES){
    const env=buildEnv(spec),moving=[];env.group.traverse(o=>{if(o.userData.nativeMotion?.kind==='particles')moving.push(o);});
    if(!moving.length){disposeEnv(env);continue;}
    const objects=moving.map(o=>({motion:o.userData.nativeMotion,initial:Array.from(o.geometry.attributes.position.array),steps:[]}));
    let time=0;
    for(let frame=0;frame<2400;frame++){
     const dt=[1/60,.033,0,.1,.1,.1][frame%6];time+=dt;env.tick(dt,time);
     if(frame<12 || frame%120===119)for(let i=0;i<moving.length;i++)objects[i].steps.push({frame,time,positions:Array.from(moving[i].geometry.attributes.position.array)});
    }
    rows.push({id:spec.id,objects});disposeEnv(env);
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-particles.json.gz'),zlib.gzipSync(JSON.stringify(particleFixture)));
  console.log(`Exported ${manifest.length} board themes, ${manifest.reduce((n,t)=>n+t.instances,0)} scene instances; animation tick counts retained for native port.`);
 }finally{await browser.close();}
}
main().catch(e=>{console.error(e);process.exitCode=1;});
