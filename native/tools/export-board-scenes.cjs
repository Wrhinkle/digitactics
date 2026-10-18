// Original board assets. Dynamic objects and tick counts remain explicit for native animation integration.
const fs=require('fs'),path=require('path'),zlib=require('zlib');
const {chromium}=require('../artifacts/audio-tools/node_modules/playwright');
const root=path.resolve(__dirname,'../..');
async function main(){
 const source=fs.readFileSync(path.join(root,'js/boards.js'),'utf8'),render=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
 const engine=require('../../tools/headless.cjs').load({strict:true}),rng=engine.makeRng.toString(),distance=engine.hexDist.toString();
 const layout=render.slice(render.indexOf('const SQ3'),render.indexOf('// placeholder color:')).replace('export const LAYOUT','const LAYOUT');
 const threeSource=fs.readFileSync(path.join(root,'vendor/three/build/three.module.js'),'utf8');
 const transparentSort=threeSource.slice(threeSource.indexOf('function reversePainterSortStable('),threeSource.indexOf('function WebGLRenderList()'));
 const module="import * as THREE from '/vendor/three/build/three.module.js';import {mergeGeometries} from '/vendor/three/addons/utils/BufferGeometryUtils.js';import {extraThemes} from '/js/boards-extra.js';import {mysteryBoards} from '/js/mystery-boards.js';"
  +`const DT={BENCH:9,CELLS:Array.from({length:56},(_,i)=>({c:i%7,r:Math.floor(i/7)})),makeRng:seed=>{const next=(${rng})(seed);const r=()=>{r.calls++;return next();};r.calls=0;return r;},hexDist:${distance}},COLS=7,ROWS=8,BENCH=9;const LAYOUT=(()=>{`+layout+';return LAYOUT;})();'
  +source.slice(source.indexOf('const { cellWorld'),source.indexOf('// ---------- renderer integration ----------'))
   .replace('spec.props(ctx);','spec.props(ctx); g.traverse(o=>{if(o.userData.nativeMotion?.fireworks)Object.assign(o.userData.nativeMotion.fireworks,{seed:spec.id.length*101+spec.id.charCodeAt(0),calls:ctx.rng.calls});if(o.userData.nativeMotion?.lightning)Object.assign(o.userData.nativeMotion.lightning,{delaySeed:spec.id.length*101+spec.id.charCodeAt(0),delayCalls:ctx.rng.calls});});')
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
  +'\nexport '+transparentSort+'\nexport {THREE,DT,THEMES,buildEnv,disposeEnv,applySurface,tileLook};'
  +'\nexport function surfaceGeometry(){'+['tileGeo','slotGeo','pedGeo'].map(name=>render.match(new RegExp('const '+name+' = [^;]+;'))[0]).join('\n')+'return {tile:tileGeo,slot:slotGeo,ped:pedGeo};}'
  +'\nexport const originalHighlight='+render.slice(render.indexOf('  highlight(ui, planning) {'),render.indexOf('  benchPos(i, partner')).trim().replace('highlight(','function(')+';'
  +'\nexport const originalDrawItems='+render.slice(render.indexOf('  drawItems(p, ui) {'),render.indexOf('  // which item token')).trim().replace('drawItems(','function(')+';'
  +'\nexport function originalBurst(e){'+render.slice(render.indexOf('const col = new THREE.Color(this.viewTeam(e.team)'),render.indexOf('const c = cellWorld(at.c, at.r);',render.indexOf('const col = new THREE.Color(this.viewTeam(e.team)')))+'}';
 const browser=await chromium.launch({channel:'msedge',headless:true});
 const extra=fs.readFileSync(path.join(root,'js/boards-extra.js'),'utf8').replace(/\r\n/g,'\n')
  .replace('const bursts = Array.from({ length: max }, () => {','const bursts = Array.from({ length: max }, (_,burstIndex) => {')
  .replace('return { p, pos, vel, mat, t0: -9, o: new THREE.Vector3() };','p.userData.nativeMotion={kind:"carnival-fireworks",fireworks:{index:burstIndex,count:per,max,colors}}; return { p, pos, vel, mat, t0: -9, o: new THREE.Vector3() };')
   .replace('wheel.add(rimM);','wheel.add(rimM); rimM.userData.nativeMotion={kind:"carnival-wheel"};')
  .replace('const cosA = Math.cos(-0.6), sinA = Math.sin(-0.6);','gond.nativeCarnival="carnival-gondolas"; const cosA = Math.cos(-0.6), sinA = Math.sin(-0.6);')
  .replace(/const bl = inst\(ctx, new THREE.SphereGeometry\(0.1, 6, 4\)[^\n]+;/,line=>line+' bl.userData.nativeMotion={kind:"carnival-bulbs",carnival:bulbs.map(p=>({...p}))};')
  .replace('anim(ctx, horse,','horses.nativeCarnival="carnival-horses"; anim(ctx, horse,')
  .replace('anim(ctx, new THREE.SphereGeometry(0.3, 10, 8),','balloons.nativeCarnival="carnival-balloons"; anim(ctx, new THREE.SphereGeometry(0.3, 10, 8),')
  .replace(/const bm = inst\(ctx, new THREE.SphereGeometry\(0.11, 6, 4\)[^\n]+;/,line=>line+' bm.userData.nativeMotion={kind:"holiday-tree",phases:bulbs.map(p=>p.ph)};')
  .replace(/const slm = inst\(ctx, new THREE.SphereGeometry\(0.09, 6, 4\)[^\n]+;/,line=>line+' slm.userData.nativeMotion={kind:"holiday-string",phases:sl.map(p=>p.ph)};')
  .replace('ctx.tick((dt, t) => { sparkle.material.opacity','snow.userData.nativeMotion.holidaySnow=true; sparkle.userData.nativeMotion.holidaySparkle=true; ctx.tick((dt, t) => { sparkle.material.opacity')
  .replace('b.m = m; b.k = 0; b.vel = 0;','m.userData.nativeMotion={kind:"city-building",building:{x:b.x,z:b.z,ry:b.ry||0,stage:b.st}}; b.m = m; b.k = 0; b.vel = 0;')
  .replace(/const cm = inst\(ctx, new THREE.ConeGeometry\(0.25, 0.6, 5\)[^\n]+;/,line=>line+' cm.userData.nativeMotion={kind:"city-crops"};')
  .replace('const blaze = { k: 0, until: -1 };','beams.forEach((b,i)=>b.userData.nativeMotion={kind:"pyramid-beams",speed:i}); const blaze = { k: 0, until: -1 };')
  .replace('bz.map((b, i) => ({ ...b, y: 0.9, ph: i * 2 })),','Object.assign(bz.map((b, i) => ({ ...b, y: 0.9, ph: i * 2 })),{nativePyramid:true}),')
  .replace('bands.push(m);','m.userData.nativeMotion={kind:"pyramid-bands",speed:k}; bands.push(m);')
  .replace('anim(ctx, bake,','ghosts.nativeGhosts=true; anim(ctx, bake,')
  .replace("anim(ctx, car, std({ vertexColors: true, roughness: 0.6 }), cars,", "cars.nativeTrain='train-cars'; anim(ctx, car, std({ vertexColors: true, roughness: 0.6 }), cars,")
  .replace("anim(ctx, engine, std({ vertexColors: true, roughness: 0.5, metalness: 0.3 }), [{ y: -1.1, z: TZ }],", "anim(ctx, engine, std({ vertexColors: true, roughness: 0.5, metalness: 0.3 }), Object.assign([{ y: -1.1, z: TZ }],{nativeTrain:'train-engine'}),")
  .replace('ctx.tick(() => { steam.position.set','steam.userData.nativeMotion.trainSteam=true; ctx.tick(() => { steam.position.set')
  .replace('ctx.tick((dt, t) => { hand.rotation.z = -t * 0.5; });','hand.userData.nativeMotion={kind:"train-clock"}; ctx.tick((dt, t) => { hand.rotation.z = -t * 0.5; });')
  .replace('strip2.rotation.z = Math.PI / 12;','strip2.rotation.z = Math.PI / 12; strip.userData.nativeMotion={kind:"studio-strip",speed:0}; strip2.userData.nativeMotion={kind:"studio-strip",speed:1};')
  .replace(/inst\(ctx, new THREE.PlaneGeometry\(7, 3.5\)[^\n]+;/,line=>'const nativeLed='+line+' nativeLed.userData.nativeMotion={kind:"studio-led"};')
  .replace("anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.9 }), aud,", "aud.nativeCrowd='studio-audience'; anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.9 }), aud,")
  .replace('const sp = new THREE.Points(sg, smat);','const sp = new THREE.Points(sg, smat); sp.userData.nativeMotion={kind:"studio-sticks",crowd:aud.map(p=>({...p}))};')
  .replace('beams.push(b); pools.push(p);','b.userData.nativeMotion={kind:"studio-beam",speed:i}; p.userData.nativeMotion={kind:"studio-pool",speed:i}; beams.push(b); pools.push(p);')
  .replace('const conf = confetti(ctx);','const conf = confetti(ctx); conf.nativeMesh.userData.nativeMotion={kind:"studio-confetti"};')
  .replace('ctx.g.add(crane);','ctx.g.add(crane); crane.children.forEach(o=>{o.updateMatrix();o.userData.nativeMotion={kind:"studio-crane",speed:o===armM?1:0,matrix:o.matrix.toArray()};});')
  .replace('fire(from = null) {','nativeMesh:m, fire(from = null) {')
  .replace('const conf = confetti(ctx, { n: 260 });','const conf = confetti(ctx, { n: 260 }); conf.nativeMesh.userData.nativeMotion={kind:"knights-confetti"};')
  .replace("anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.8 }), crowd,", "crowd.nativeCrowd='knights-crowd'; anim(ctx, crowdGeo(), std({ color: '#ffffff', roughness: 0.8 }), crowd,")
  .replace('anim(ctx, bannerGeo(0.8, 1.5),','ban.nativeCrowd="knights-banners"; anim(ctx, bannerGeo(0.8, 1.5),')
  .replace('const fpts = new THREE.Points(fg, fmat);','const fpts = new THREE.Points(fg, fmat); fpts.userData.nativeMotion={kind:"knights-flashes",crowd:fl.map(p=>({...p}))};')
  .replace('const flapFn = (p, i, t) => {','pagesL.nativeLibrary=pagesR.nativeLibrary="library-pages"; const flapFn = (p, i, t) => {')
  .replace('anim(ctx, new THREE.CylinderGeometry(0.09, 0.09, 0.5, 8),','candles.nativeLibrary="library-candles"; anim(ctx, new THREE.CylinderGeometry(0.09, 0.09, 0.5, 8),')
  .replace("basic({ color: '#ffd070' }), candles.map((c) => ({ ...c })),", "basic({ color: '#ffd070' }), Object.assign(candles.map((c) => ({ ...c })),{nativeLibrary:'library-flames'}),")
  .replace("glow('#ffb040', 0.18), candles.map((c) => ({ ...c })),", "glow('#ffb040', 0.18), Object.assign(candles.map((c) => ({ ...c })),{nativeLibrary:'library-halos'}),")
  .replace('const evo = { k: 0, spin: 0 };','c1.userData.nativeMotion={kind:"library-circle"}; c2.userData.nativeMotion={kind:"library-outer"}; sparks.userData.nativeMotion.librarySparks=true; const evo = { k: 0, spin: 0 };')
  .replace('anim(ctx, numemon,','nums.nativeSewer="sewer-numemon"; anim(ctx, numemon,')
  .replace('anim(ctx, duck, std({ vertexColors: true, roughness: 0.3 }), [{ s: 1.3 }],','anim(ctx, duck, std({ vertexColors: true, roughness: 0.3 }), Object.assign([{ s: 1.3 }],{nativeSewer:"sewer-duck"}),')
  .replace(/inst\(ctx, new THREE.CylinderGeometry\(0.32, 0.4, 2.2[^\n]+;/,line=>'const nativePour='+line+' nativePour.userData.nativeMotion={kind:"sewer-pour"};')
  .replace('anim(ctx, snowman,','men.nativeFreeze=true; anim(ctx, snowman,')
  .replace('const base = snow.geometry.attributes.position.array;', 'snow.userData.nativeMotion.freezeSnow=true; const base = snow.geometry.attributes.position.array;')
  .replace('          return m;\n        };\n        const aur', '          m.userData.nativeMotion={kind:"freeze-aurora",aurora:{r,a0,a1,y,h,ph,base}}; return m;\n        };\n        const aur')

  .replace(/mesh\(ctx, new THREE.CircleGeometry\(40, 40\)[^\n]+cm\(caus\)[^\n]+;/,line=>'const nativeCaustics='+line+' nativeCaustics.userData.nativeMotion={kind:"underwater-caustics"};')
  .replace(/mesh\(ctx, new THREE.CircleGeometry\(12.6, 40\)[^\n]+cm\(caus2\)[^\n]+;/,line=>'const nativeCaustics2='+line+' nativeCaustics2.userData.nativeMotion={kind:"underwater-caustics2"};')
  .replace('const shm = anim(ctx,','shafts.nativeUnderwater="underwater-shafts"; const shm = anim(ctx,')
  .replace('anim(ctx, kelpGeo,','kelp.nativeUnderwater="underwater-kelp"; anim(ctx, kelpGeo,')
  .replace('clamsLo.map((c, i) => ({ ...c, y: 0.3, ph: i * 2 })),','Object.assign(clamsLo.map((c, i) => ({ ...c, y: 0.3, ph: i * 2 })),{nativeUnderwater:"underwater-clams"}),')
  .replace('anim(ctx, fishGeo,','fish.nativeUnderwater="underwater-fish"; anim(ctx, fishGeo,')
  .replace('plain(ctx, lavaMat, -1.3);','const nativeLava=plain(ctx, lavaMat, -1.3); nativeLava.userData.nativeMotion={kind:"temple-lava"};')
  .replace('const fire = { k: 0 };','const fire = { k: 0 }; fl.nativeTemple="temple-outer";')
  .replace('fl.filter((_, i) => i % 3 === 0),','Object.assign(fl.filter((_, i) => i % 3 === 0),{nativeTemple:"temple-inner"}),')
  .replace('anim(ctx, new THREE.CylinderGeometry(0.15, 0.7, 1, 10, 1, true)', 'gey.nativeTemple="temple-geysers"; anim(ctx, new THREE.CylinderGeometry(0.15, 0.7, 1, 10, 1, true)')
  .replace(/const embers = particles\(ctx, 260,[^\n]+;/,line=>line+' embers.userData.nativeMotion.templeEmbers=true;')
  .replace('const speed = { v: 1, ph: 0 };','const speed = { v: 1, ph: 0 }; pts.userData.nativeMotion={kind:"circuit-pulses",pulses:pulses.map(p=>({...p}))};')
  .replace(/mesh\(ctx, new THREE.CylinderGeometry\(13.6, 13.6, 7[^\n]+;/,line=>'const nativeScreen='+line+' nativeScreen.userData.nativeMotion={kind:"circuit-screen"};')
  .replace(/const fl = mesh\(ctx, new THREE.RingGeometry\(16, 40, 48\)[^\n]+;/,line=>line+' fl.userData.nativeMotion={kind:"circuit-floor"};')
  .replace(/const sm = inst\(ctx, new THREE.PlaneGeometry\(0.9, 1\)[^\n]+;/,line=>line+' sm.userData.nativeMotion={kind:"kowloon-signs",signs:signs.map(s=>({phase:s.ph,color:C(s.color).toArray()}))};')
  .replace(/const band = mesh\(ctx, new THREE.CylinderGeometry\(14.4, 14.4[^\n]+;/,line=>line+' band.userData.nativeMotion={kind:"kowloon-band"};')
  .replace(/const lattice = mesh\(ctx, new THREE.CylinderGeometry\(14.5, 14.5[^\n]+;/,line=>line+' lattice.userData.nativeMotion={kind:"kowloon-lattice"};')
  .replace(/const rain = particles\(ctx, 520,[^\n]+;/,line=>line+' rain.userData.nativeMotion.kowloonRain=true;')
  .replace('moon.lookAt(0, 16.5, 17.3);','moon.lookAt(0, 16.5, 17.3); moon.userData.nativeMotion={kind:"castle-moon"};')
  .replace('halo.lookAt(0, 16.5, 17.3);','halo.lookAt(0, 16.5, 17.3); halo.userData.nativeMotion={kind:"castle-halo"};')
  .replace('anim(ctx, new THREE.ConeGeometry(0.06, 0.2, 6),','flames.nativeCastle="castle-flames"; anim(ctx, new THREE.ConeGeometry(0.06, 0.2, 6),')
  .replace('anim(ctx, batGeo,','bats.nativeCastle="castle-bats"; anim(ctx, batGeo,')
  .replace('const bolts = [0, 1, 2].map(() => {','const bolts = [0, 1, 2].map(boltIndex => {')
  .replace('const m = new THREE.Mesh(merge(parts), mat);','const m = new THREE.Mesh(merge(parts), mat); if(ctx.spec.id==="spiral-mountain"||ctx.spec.id==="overdell-graveyard") m.userData.nativeMotion={kind:ctx.spec.id==="spiral-mountain"?"spiral-lightning":"graveyard-lightning",lightning:{index:boltIndex,spots:spots.map(p=>[p.x,p.y||0,p.z]),flash,flashColor,...(ctx.spec.id==="overdell-graveyard"?{firstDelay:0,delayBase:2,delayRange:3,strikeOnCombat:false}:{})}};')
  .replace('const lit = ctx.light();','for(const b of bolts)if(b.userData.nativeMotion?.lightning)b.userData.nativeMotion.lightning.boltCalls=rng.calls; const lit = ctx.light();')
  .replace('const sp = { v: 1 };','const sp = { v: 1 }; rocks.nativeKind="spiral-rocks";')
  .replace('anim(ctx, new THREE.OctahedronGeometry(0.5, 0), basic({ color: \'#ffffff\' }), cry,','cry.nativeKind="spiral-crystals"; anim(ctx, new THREE.OctahedronGeometry(0.5, 0), basic({ color: \'#ffffff\' }), cry,')
  .replace('anim(ctx, new THREE.SphereGeometry(0.7, 12, 8), glow(\'#ffffff\', 0.25), halo,','halo.nativeKind="spiral-halos"; anim(ctx, new THREE.SphereGeometry(0.7, 12, 8), glow(\'#ffffff\', 0.25), halo,')
  .replace('anim(ctx, new THREE.ConeGeometry(1.2, 2.4, 6).rotateX(Math.PI)', 'islets.nativeKind="spiral-islets"; anim(ctx, new THREE.ConeGeometry(1.2, 2.4, 6).rotateX(Math.PI)')
  .replace('const m = inst(ctx, geo, mat, items, { moving: true, shadow: false, ...opts });','const m = inst(ctx, geo, mat, items, { moving: true, shadow: false, ...opts }); if(items.nativeCarnival)m.userData.nativeMotion={kind:items.nativeCarnival,carnival:items.map(p=>({...p}))}; if(items.nativePyramid)m.userData.nativeMotion={kind:"pyramid-flames",props:items.map(p=>({...p,phase:p.ph}))}; if(items.nativeGhosts)m.userData.nativeMotion={kind:"graveyard-ghosts",ghosts:items.map(p=>({...p}))}; if(items.nativeTrain)m.userData.nativeMotion={kind:items.nativeTrain}; if(items.nativeCrowd)m.userData.nativeMotion={kind:items.nativeCrowd,crowd:items.map(p=>({...p}))}; if(items.nativeLibrary)m.userData.nativeMotion={kind:items.nativeLibrary,library:items.map(p=>({...p}))}; if(items.nativeSewer)m.userData.nativeMotion={kind:items.nativeSewer,props:items.map(p=>({...p,phase:p.ph??0}))}; if(items.nativeFreeze)m.userData.nativeMotion={kind:"freeze-men",props:items.map(p=>({...p,phase:p.ph}))}; if(items.nativeKind) m.userData.nativeMotion={kind:items.nativeKind,spirals:items.map(p=>({...p}))}; if(items.nativeCastle) m.userData.nativeMotion={kind:items.nativeCastle,castle:items.map(p=>({...p}))}; if(items.nativeTemple)m.userData.nativeMotion={kind:items.nativeTemple,temple:items.map(p=>({...p}))}; if(items.nativeUnderwater)m.userData.nativeMotion={kind:items.nativeUnderwater,underwater:items.map(p=>({...p}))};');
 const mystery=fs.readFileSync(path.join(root,'js/mystery-boards.js'),'utf8')
  .replace("  const titles=", "  const tag=(o,kind,parent,speed=0)=>{o.updateMatrix();if(parent)parent.updateMatrix();o.userData.nativeMotion={kind,speed,matrix:o.matrix.toArray(),parent:parent?.matrix.toArray()};}; const titles=")
  .replace('center.add(monument);','center.add(monument);tag(monument,"mystery-monument",center,n);')
  .replace('center.add(cross);','center.add(cross);tag(cross,"mystery-center",center,n);')
  .replace('const satellites=inst(ctx,geometry,glow,props,{moving:true});','const satellites=inst(ctx,geometry,glow,props,{moving:true}); satellites.userData.nativeMotion={kind:"mystery-satellites",speed:n,props:props.map(p=>({...p}))};')
  .replace('ctx.tick((dt,t)=>{blades.rotation.z=', 'blades.children.forEach(o=>tag(o,"mystery-blades",blades));ctx.tick((dt,t)=>{blades.rotation.z=')
  .replace('ctx.tick((dt,t)=>{rings.rotation.y=', 'rings.children.forEach(o=>tag(o,"mystery-rings",rings));ctx.tick((dt,t)=>{rings.rotation.y=')
  .replace('ctx.tick((dt,t)=>{meteor.position.y=', 'tag(meteor,"mystery-meteor",null,x);ctx.tick((dt,t)=>{meteor.position.y=');
 try{
  const page=await browser.newPage();
  await page.route('http://boards.local/**',async route=>{
   const url=new URL(route.request().url());
   if(url.pathname==='/')return route.fulfill({contentType:'text/html',body:'<!doctype html><script type="importmap">{"imports":{"three":"/vendor/three/build/three.module.js"}}</script>'});
   if(url.pathname==='/boards.js')return route.fulfill({contentType:'text/javascript',body:module});
   if(url.pathname==='/js/mystery-boards.js')return route.fulfill({contentType:'text/javascript',body:mystery});
   if(url.pathname==='/js/boards-extra.js')return route.fulfill({contentType:'text/javascript',body:extra});
   const file=path.resolve(root,'.'+decodeURIComponent(url.pathname));if(!file.startsWith(root+path.sep))return route.abort();
   return route.fulfill({contentType:'text/javascript',body:fs.readFileSync(file)});
  });
  await page.goto('http://boards.local/');
  const ids=await page.evaluate(async()=> (await import('/boards.js')).THEMES.map(t=>t.id));
  const orderFixture=await page.evaluate(async()=>{
   const {THREE,reversePainterSortStable}=await import('/boards.js'),rows=[];
   for(let frame=0;frame<80;frame++){
    const camera=new THREE.PerspectiveCamera(50,1.3,.1,100);camera.position.set(Math.sin(frame*.3)*18,12,Math.cos(frame*.3)*18);camera.lookAt(0,0,0);camera.updateMatrixWorld();
    const view=new THREE.Matrix4().multiplyMatrices(camera.projectionMatrix,camera.matrixWorldInverse),items=[];
    for(let id=0;id<9;id++){
     const center=[id%3-1,(id%2)*.2,id%4-2],world=new THREE.Matrix4().compose(new THREE.Vector3((id%3-1)*4,0,(Math.floor(id/3)-1)*4),new THREE.Quaternion().setFromEuler(new THREE.Euler(.1,id*.4,.2)),new THREE.Vector3(1.8,1,1));
     const z=new THREE.Vector4(...center,1).applyMatrix4(world).applyMatrix4(view).z;
     items.push({id,groupOrder:0,renderOrder:frame%2===0?id%3:0,z,center,world:world.toArray()});
    }
    rows.push({camera:view.toArray(),items,expected:[...items].reverse().sort(reversePainterSortStable).map(i=>i.id)});
   }
   return rows;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-order.json'),JSON.stringify(orderFixture));
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
     materials.push({color:[...m.color.toArray(),m.opacity],map:texture(m.map),...(m.emissiveMap?{emissiveMap:texture(m.emissiveMap)}:{}),unlit:!!m.isMeshBasicMaterial||!!m.isPointsMaterial||!!m.isSpriteMaterial||!!m.isLineBasicMaterial,transparent:m.transparent,depthWrite:m.depthWrite,side:m.side,fog:m.fog,flatShading:!!m.flatShading,alphaTest:m.alphaTest,
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
     let sphere;if(o.boundingSphere!==undefined){if(o.boundingSphere===null)o.computeBoundingSphere();sphere=o.boundingSphere;}else{if(g.boundingSphere===null)g.computeBoundingSphere();sphere=g.boundingSphere;}
     let visible=true;for(let parent=o;parent;parent=parent.parent)visible=visible&&parent.visible;
     const base={kind:o.isPoints?'points':o.isSprite?'sprite':o.isLineSegments?'lines':o.isLine?'lineStrip':'mesh',geometry:geometry(g),materials:[].concat(o.material).map(material),matrix:o.matrixWorld.toArray(),visible,castShadow:o.castShadow,receiveShadow:o.receiveShadow,motion:o.userData.nativeMotion||null};
     if(o.isInstancedMesh){base.instances=[];const m=new THREE.Matrix4(),color=new THREE.Color();for(let i=0;i<o.count;i++){o.getMatrixAt(i,m);if(o.instanceColor)o.getColorAt(i,color);base.instances.push({matrix:m.toArray(),color:color.toArray()});}}
     Object.assign(base,{sortCenter:sphere.center.toArray(),sortId:o.id-env.group.id,renderOrder:o.renderOrder});objects.push(base);
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
      }else if(motion.kind==='island-crystals'||motion.kind==='beach-palms'||(motion.props&&!motion.kind.startsWith('sewer-')&&!motion.kind.startsWith('mystery-'))){
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
  const spiralFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js');
   const env=buildEnv(THEMES.find(s=>s.id==='spiral-mountain')),objects=[],frames=[];
   env.group.traverse(o=>{if(o.userData.nativeMotion?.spirals)objects.push(o);});
   env.signal({phase:'planning',stage:1,won:false,evo:0});env.tick(0,0);
   let time=0;
   for(let frame=0;frame<360;frame++){
    const delta=[0,1/60,.033,.1][frame%4],combat=Math.floor(frame/90)%2===1;time+=delta;
    env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);
    frames.push({delta,time,combat,objects:objects.map(o=>{
     const matrix=new THREE.Matrix4(),matrices=[];for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
     return {kind:o.userData.nativeMotion.kind,matrices};
    })});
   }
   const result={motions:objects.map(o=>o.userData.nativeMotion),frames};disposeEnv(env);return result;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-spiral.json.gz'),zlib.gzipSync(JSON.stringify(spiralFixture)));
  const lightningFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='spiral-mountain');
   function run(initialCombat){
   const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)},env=buildEnv(spec,{L}),bolts=[],frames=[];
   env.group.traverse(o=>{if(o.userData.nativeMotion?.lightning)bolts.push(o);});
   env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
   for(let frame=0;frame<1600;frame++){
    const delta=[1/60,.033,0,.1][frame%4],phase=['combat','result','planning','combat'][Math.floor(frame/200)%4],combat=phase==='combat';time+=delta;
    env.signal({phase,stage:1,won:false,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
    frames.push({time,delta,combat,flash:L.hemi.intensity-spec.hemi[2],bolts:bolts.map(o=>({visible:o.visible,matrix:o.matrixWorld.toArray()}))});
   }
   const result={initialCombat,config:bolts[0].userData.nativeMotion.lightning,frames};disposeEnv(env);return result;
   }
   return [run(false),run(true)];
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-lightning.json.gz'),zlib.gzipSync(JSON.stringify(lightningFixture)));
  const castleFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialCombat of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='vampire-castle')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('castle-'))objects.push(o);});
    env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<600;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=Math.floor(frame/100)%2===0,stage=1+Math.floor(frame/70);time+=delta;
     env.signal({phase:combat?'combat':'planning',stage,won:false,evo:0});env.tick(delta,time);
     frames.push({time,delta,combat,stage,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];for(let i=0;i<(o.count||0);i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {kind:o.userData.nativeMotion.kind,matrices,color:o.material.color.toArray(),opacity:o.material.opacity};
     })});
    }
    scenarios.push({initialCombat,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-castle.json.gz'),zlib.gzipSync(JSON.stringify(castleFixture)));
  const kowloonFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='kowloon');
   const scene={fog:new THREE.Fog(...spec.fog)},env=buildEnv(spec,{scene}),objects=[],frames=[];
   env.group.traverse(o=>{const m=o.userData.nativeMotion;if(m?.kind.startsWith('kowloon-')||m?.kowloonRain)objects.push(o);});
   env.signal({phase:'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
   for(let frame=0;frame<900;frame++){
    const delta=[0,1/60,.033,.1][frame%4],combat=Math.floor(frame/150)%2===1;time+=delta;
    env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);
    frames.push({time,delta,combat,fog:[scene.fog.near,scene.fog.far],objects:objects.map(o=>{
     const colors=[],color=new THREE.Color();if(o.instanceColor)for(let i=0;i<o.count;i++){o.getColorAt(i,color);colors.push(color.toArray());}
     return {kind:o.userData.nativeMotion.kind,y:o.position.y,opacity:o.material.opacity,size:o.material.size??null,colors};
    })});
   }
   const result={motions:objects.map(o=>o.userData.nativeMotion),frames};disposeEnv(env);return result;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-kowloon.json.gz'),zlib.gzipSync(JSON.stringify(kowloonFixture)));
  const circuitFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='circuit-core'),scenarios=[];
   for(const initialWon of [false,true]){
    const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)},env=buildEnv(spec,{L}),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('circuit-'))objects.push(o);});
    env.signal({phase:'planning',stage:1,won:initialWon,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%200<100,won=frame<90?initialWon:frame%200>=90;time+=delta;
     env.signal({phase:combat?'combat':'result',stage:1,won,evo:0});env.tick(delta,time);
     frames.push({time,delta,combat,won,flash:L.hemi.intensity-spec.hemi[2],objects:objects.map(o=>({kind:o.userData.nativeMotion.kind,positions:o.isPoints?Array.from(o.geometry.attributes.position.array):null,offset:o.material.map?.offset.toArray(),opacity:o.material.opacity}))});
    }
    scenarios.push({initialWon,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-circuit.json.gz'),zlib.gzipSync(JSON.stringify(circuitFixture)));
  const templeFixture=await page.evaluate(async()=>{
   const {THREE,DT,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='flame-temple'),scenarios=[],originalRandom=Math.random;
   try {for(const initialEvolution of [0,3]){
    Math.random=DT.makeRng(733);
    const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)},env=buildEnv(spec,{L}),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('temple-')||o.userData.nativeMotion?.templeEmbers)objects.push(o);});
    env.signal({phase:'planning',stage:1,won:false,evo:initialEvolution});env.tick(0,0);Math.random=DT.makeRng(733);let time=0;
    for(let frame=0;frame<600;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%200<100,stage=1+Math.floor(frame/100),evolution=initialEvolution+[0,1,0,2][Math.floor(frame/150)];time+=delta;
     env.signal({phase:combat?'combat':'planning',stage,won:false,evo:evolution});env.tick(delta,time);
     frames.push({time,delta,combat,stage,evolution,flash:L.hemi.intensity-spec.hemi[2],sun:L.sun.intensity,sunColor:L.sun.color.toArray(),objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {kind:o.userData.nativeMotion.kind,matrices,color:o.material.color.toArray(),offset:o.material.map?.offset.toArray(),size:o.material.size??null};
     })});
    }
    scenarios.push({initialEvolution,sun:spec.sun,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }}finally{Math.random=originalRandom;}
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-temple.json.gz'),zlib.gzipSync(JSON.stringify(templeFixture)));
  const underwaterFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialCombat of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='gekomon-castle')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('underwater-'))objects.push(o);});
    env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<500;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=Math.floor(frame/125)%2===0;time+=delta;
     env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);
     frames.push({time,delta,combat,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {kind:o.userData.nativeMotion.kind,matrices,offset:o.material.map?.offset.toArray()};
     })});
    }
    scenarios.push({initialCombat,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-underwater.json.gz'),zlib.gzipSync(JSON.stringify(underwaterFixture)));
  const mysteryFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const spec of THEMES.filter(s=>s.id.startsWith('mystery_'))){
    const env=buildEnv(spec),objects=[],frames=[];env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('mystery-'))objects.push(o);});
    env.tick(0,0);let time=0;
    for(let frame=0;frame<240;frame++){
     const delta=[0,1/60,.033,.1][frame%4];time+=delta;env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {matrix:o.matrixWorld.toArray(),matrices};
     })});
    }
    scenarios.push({id:spec.id,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-mystery.json.gz'),zlib.gzipSync(JSON.stringify(mysteryFixture)));
  const fireworkFixture=await page.evaluate(async()=>{
   const {THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialWon of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='gargomon-carnival')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.fireworks)objects.push(o);});
    env.signal({phase:'combat',stage:1,won:initialWon,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<1000;frame++){
     const delta=[0,1/60,.033,.1][frame%4],won=frame<150?initialWon:frame<400||frame>=650;time+=delta;
     env.signal({phase:'combat',stage:1,won,evo:0});env.tick(delta,time);
     frames.push({time,delta,won,bursts:objects.map(o=>({visible:o.visible,color:o.material.color.toArray(),opacity:o.material.opacity,positions:Array.from(o.geometry.attributes.position.array)}))});
    }
    scenarios.push({initialWon,config:objects[0].userData.nativeMotion.fireworks,frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-fireworks.json.gz'),zlib.gzipSync(JSON.stringify(fireworkFixture)));
  const carnivalFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),env=buildEnv(THEMES.find(s=>s.id==='gargomon-carnival')),objects=[],frames=[];
   env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('carnival-')&&!o.userData.nativeMotion.fireworks)objects.push(o);});
   for(const time of [0,.016,.37,1,2.7,10,31.25,120,1000]){
    env.tick(.016,time);env.group.updateMatrixWorld(true);
    frames.push({time,objects:objects.map(o=>{
     const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
     return {matrix:o.matrixWorld.toArray(),matrices};
    })});
   }
   const result={motions:objects.map(o=>o.userData.nativeMotion),frames};disposeEnv(env);return result;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-carnival.json.gz'),zlib.gzipSync(JSON.stringify(carnivalFixture)));
  const holidayFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialWon of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='holiday-world')),objects=[],frames=[];
    env.group.traverse(o=>{const m=o.userData.nativeMotion;if(m?.phases||m?.holidaySnow||m?.holidaySparkle)objects.push(o);});
    env.signal({phase:'combat',stage:1,won:initialWon,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],won=frame<100?initialWon:frame<350||frame>=600;time+=delta;
     env.signal({phase:'combat',stage:1,won,evo:0});env.tick(delta,time);
     frames.push({time,won,objects:objects.map(o=>{
      const color=new THREE.Color(),colors=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getColorAt(i,color);colors.push(color.toArray());}
      return {colors,opacity:o.material.opacity,size:o.material.size??0};
     })});
    }
    scenarios.push({initialWon,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-holiday.json.gz'),zlib.gzipSync(JSON.stringify(holidayFixture)));
  const cityFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialStage of [1,4,7]){
    const env=buildEnv(THEMES.find(s=>s.id==='file-city')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('city-'))objects.push(o);});
    env.signal({phase:'planning',stage:initialStage,won:false,evo:0});env.tick(0,0);env.group.updateMatrixWorld(true);let time=0;
    const initial=objects.map(o=>({visible:o.visible,matrix:o.matrixWorld.toArray()}));
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],stage=[initialStage,2,4,7,3,1,6,7][Math.floor(frame/100)];time+=delta;
     env.signal({phase:'planning',stage,won:false,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,stage,objects:objects.map(o=>({visible:o.visible,matrix:o.matrixWorld.toArray()}))});
    }
    scenarios.push({initialStage,initial,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-city.json.gz'),zlib.gzipSync(JSON.stringify(cityFixture)));
  const pyramidFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialCombat of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='server-pyramid')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('pyramid-'))objects.push(o);});
    env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%200<100,stage=[1,2,4,7,3,0,6,1][Math.floor(frame/100)];time+=delta;
     env.signal({phase:combat?'combat':'planning',stage,won:false,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,combat,stage,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {matrix:o.matrixWorld.toArray(),matrices,opacity:o.material.opacity,offset:o.material.map?.offset.toArray()};
     })});
    }
    scenarios.push({initialCombat,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-pyramid.json.gz'),zlib.gzipSync(JSON.stringify(pyramidFixture)));
  const graveyardFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='overdell-graveyard'),scenarios=[];
   for(const initialCombat of [false,true]){
    const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)},scene={fog:new THREE.Fog(...spec.fog)},env=buildEnv(spec,{L,scene}),bolts=[],frames=[];let ghosts;
    env.group.traverse(o=>{if(o.userData.nativeMotion?.lightning)bolts.push(o);if(o.userData.nativeMotion?.ghosts)ghosts=o;});
    env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<1600;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%400<200;time+=delta;
     env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
     const matrix=new THREE.Matrix4(),matrices=[];for(let i=0;i<ghosts.count;i++){ghosts.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
     frames.push({time,delta,combat,matrices,fog:[scene.fog.near,scene.fog.far],flash:L.hemi.intensity-spec.hemi[2],bolts:bolts.map(o=>({visible:o.visible,matrix:o.matrixWorld.toArray()}))});
    }
    scenarios.push({initialCombat,config:bolts[0].userData.nativeMotion.lightning,ghosts:ghosts.userData.nativeMotion.ghosts,frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-graveyard.json.gz'),zlib.gzipSync(JSON.stringify(graveyardFixture)));
  const trainFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialCombat of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='trailmon-station')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('train-')||o.userData.nativeMotion?.trainSteam)objects.push(o);});
    env.signal({phase:initialCombat?'combat':'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<1400;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame<200?initialCombat:Math.floor(frame/180)%2===0;time+=delta;
     env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,combat,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {kind:o.userData.nativeMotion.kind,matrix:o.matrixWorld.toArray(),matrices,opacity:o.material.opacity};
     })});
    }
    scenarios.push({initialCombat,frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-train.json.gz'),zlib.gzipSync(JSON.stringify(trainFixture)));
  const studioFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initial of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='battle-studio')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('studio-'))objects.push(o);});
    env.signal({phase:initial?'combat':'planning',stage:1,won:initial,evo:0});env.tick(0,0);let time=0;
    const motions=objects.map(o=>({motion:o.userData.nativeMotion,matrix:o.matrixWorld.toArray()}));
    for(let frame=0;frame<600;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%200<100,won=frame<100?initial:frame<300||frame>=450;time+=delta;
     env.signal({phase:combat?'combat':'planning',stage:1,won,evo:0});env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,combat,won,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i+=Math.max(1,Math.floor(o.count/24))){o.getMatrixAt(i,matrix);matrices.push({index:i,matrix:matrix.toArray()});}
      return {matrix:o.matrixWorld.toArray(),visible:o.visible,matrices,color:o.material.color.toArray(),offset:o.material.map?.offset.toArray(),positions:o.isPoints?Array.from(o.geometry.attributes.position.array):null};
     })});
    }
    scenarios.push({initial,motions,frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-studio.json.gz'),zlib.gzipSync(JSON.stringify(studioFixture)));
  const knightsFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initial of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='knights-coliseum')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('knights-'))objects.push(o);});
    env.signal({phase:initial?'combat':'planning',stage:1,won:initial,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],combat=frame%300<150,won=frame<200?initial:frame<430||frame>=620;time+=delta;
     env.signal({phase:combat?'combat':'planning',stage:1,won,evo:0});env.tick(delta,time);
     frames.push({time,delta,combat,won,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i+=Math.max(1,Math.floor(o.count/24))){o.getMatrixAt(i,matrix);matrices.push({index:i,matrix:matrix.toArray()});}
      return {kind:o.userData.nativeMotion.kind,visible:o.visible,matrices,positions:o.isPoints?Array.from(o.geometry.attributes.position.array):null};
     })});
    }
    scenarios.push({initial,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-knights.json.gz'),zlib.gzipSync(JSON.stringify(knightsFixture)));
  const libraryFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='witchelny-library'),scenarios=[];
   for(const initialEvolution of [0,3]){
    const L={hemi:new THREE.HemisphereLight(...spec.hemi),sun:new THREE.DirectionalLight(...spec.sun)},env=buildEnv(spec,{L}),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('library-')||o.userData.nativeMotion?.librarySparks)objects.push(o);});
    env.signal({phase:'planning',stage:1,won:false,evo:initialEvolution});env.tick(0,0);let time=0;
    for(let frame=0;frame<600;frame++){
     const delta=[0,1/60,.033,.1][frame%4],evolution=initialEvolution+[0,1,0,2][Math.floor(frame/150)];time+=delta;
     env.signal({phase:'planning',stage:1,won:false,evo:evolution});env.tick(delta,time);env.group.updateMatrixWorld(true);
     frames.push({time,delta,evolution,flash:L.hemi.intensity-spec.hemi[2],sunColor:L.sun.color.toArray(),objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {matrix:o.matrixWorld.toArray(),matrices,color:o.material.color.toArray(),opacity:o.material.opacity};
     })});
    }
    scenarios.push({initialEvolution,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-library.json.gz'),zlib.gzipSync(JSON.stringify(libraryFixture)));
  const sewerFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),scenarios=[];
   for(const initialWon of [false,true]){
    const env=buildEnv(THEMES.find(s=>s.id==='numemon-sewer')),objects=[],frames=[];
    env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('sewer-'))objects.push(o);});
    env.signal({phase:'combat',stage:1,won:initialWon,evo:0});env.tick(0,0);let time=0;
    for(let frame=0;frame<800;frame++){
     const delta=[0,1/60,.033,.1][frame%4],won=frame<200?initialWon:frame<420||frame>=600;time+=delta;
     env.signal({phase:'combat',stage:1,won,evo:0});env.tick(delta,time);
     frames.push({time,won,objects:objects.map(o=>{
      const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
      return {kind:o.userData.nativeMotion.kind,matrices,offset:o.material.map?.offset.toArray()};
     })});
    }
    scenarios.push({initialWon,motions:objects.map(o=>o.userData.nativeMotion),frames});disposeEnv(env);
   }
   return scenarios;
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-sewer.json.gz'),zlib.gzipSync(JSON.stringify(sewerFixture)));
  const freezeFixture=await page.evaluate(async()=>{
   const {THREE,THEMES,buildEnv,disposeEnv}=await import('/boards.js'),spec=THEMES.find(s=>s.id==='freezeland'),scene={fog:new THREE.Fog(...spec.fog)},env=buildEnv(spec,{scene}),objects=[],frames=[];
   env.group.traverse(o=>{if(o.userData.nativeMotion?.kind.startsWith('freeze-')||o.userData.nativeMotion?.freezeSnow)objects.push(o);});
   env.signal({phase:'planning',stage:1,won:false,evo:0});env.tick(0,0);let time=0;
   const initial=objects.map(o=>({motion:o.userData.nativeMotion,positions:Array.from(o.geometry.attributes.position.array)}));
   for(let frame=0;frame<600;frame++){
    const delta=[0,1/60,.033,.1][frame%4],combat=Math.floor(frame/100)%2===0;time+=delta;
    env.signal({phase:combat?'combat':'planning',stage:1,won:false,evo:0});env.tick(delta,time);
    frames.push({time,delta,combat,fog:{near:scene.fog.near,far:scene.fog.far,color:scene.fog.color.toArray()},objects:objects.map(o=>{
     const matrix=new THREE.Matrix4(),matrices=[];if(o.isInstancedMesh)for(let i=0;i<o.count;i++){o.getMatrixAt(i,matrix);matrices.push(matrix.toArray());}
     return {matrices,positions:Array.from(o.geometry.attributes.position.array),opacity:o.material.opacity,size:o.material.size??0};
    })});
   }
   disposeEnv(env);return {initial,frames};
  });
  fs.writeFileSync(path.join(root,'native/tests/fixtures/board-freeze.json.gz'),zlib.gzipSync(JSON.stringify(freezeFixture)));
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
