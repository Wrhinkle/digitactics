// Shared headless capture for unchanged source move code; only GPU pools are replaced.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm');
const {pathToFileURL}=require('node:url');
const root=path.resolve(__dirname,'../..'),cache=new Map();
function source(name){if(!cache.has(name))cache.set(name,fs.readFileSync(path.join(root,name),'utf8').replace(/^import .*;\r?\n/gm,'').replace(/\bexport (?=(?:const|class|function)\b)/g,'').replace(/^export \{.*\};?\r?$/gm,''));return cache.get(name);}
module.exports=async function(){
  const THREE=await import(pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
  return function create(seed,DT={}){
    let randomState = seed;
    const math = Object.create(Math);
    math.random = () => {
      let t = randomState += 0x6D2B79F5;
      t = Math.imul(t ^ t >>> 15, t | 1);
      t ^= t + Math.imul(t ^ t >>> 7, t | 61);
      return ((t ^ t >>> 14) >>> 0) / 4294967296;
    };
    const context = {THREE, Math:math, window:{DT}, console};
    vm.createContext(context);
    vm.runInContext(source('js/vfx.js') + '\nglobalThis.reference={MoveFx,Sprites,CELL,CELL_NAMES};', context);
    vm.runInContext('{'+source('js/moves.js')+';globalThis.moveCatalog={specFor,basicFor,PALETTE,STAGE_SCALE};}', context);
    Object.assign(context,context.moveCatalog);
    vm.runInContext(source('js/moveprims.js') + '\nglobalThis.primitives=PRIMS;globalThis.SourceMoves=Moves;', context);
    const {MoveFx,Sprites,CELL_NAMES} = context.reference;
    const scene = new THREE.Group(), camera = new THREE.PerspectiveCamera(45,16/9,.1,100);
    camera.position.set(0,8,12); camera.lookAt(0,0,0); camera.updateMatrixWorld();
    const pieces = [], meshes = [], shakes = [];
    const fx = Object.create(MoveFx.prototype);
    Object.assign(fx, {now:0, tasks:[],taskFree:[],ribbons:[],pools:{},scene,cam:camera,
      host:{canvas:{height:900},shake:(amp,dur)=>shakes.push([amp,dur])},vp:new THREE.Matrix4(),paused:false});
    fx.add = new Sprites(scene,9000,true,null,8); fx.alpha = new Sprites(scene,2500,false,null,6);
    fx.batches = {};
    for (const kind of ['glow','shock','ring','crescent','claw','ball','pillar','spike_ice','spike_rock','spike_thorn','spike_dark','spike_metal']) {
      const max = kind === 'glow' ? 768 : kind === 'claw' || kind === 'pillar' ? 128 : 256;
      fx.batches[kind] = {
        n:0, begin(){this.n=0;}, end(){},
        push(x,y,z,sx,sy,sz,color,alpha,rotation,face){
          if(this.n>=max || alpha<=.002)return; this.n++;
          pieces.push({kind,values:[x,y,z,sx,sy,sz,color.r,color.g,color.b,alpha,rotation,face,0,0,0]});
        },
        pushEuler(x,y,z,sx,sy,sz,rx,ry,rz,color,alpha){
          if(this.n>=max)return;this.n++;
          pieces.push({kind,values:[x,y,z,sx,sy,sz,color.r,color.g,color.b,alpha,0,null,rx,ry,rz]});
        }
      };
    }
    fx.batchList = Object.values(fx.batches);
    fx.mesh = kind => {
      const pool = fx.pools[kind] || (fx.pools[kind] = []);
      let mesh = pool.pop();
      if (!mesh) {
        mesh = {kind,position:new THREE.Vector3(),scale:new THREE.Vector3(),rotation:new THREE.Euler(),
          material:{color:new THREE.Color(),emissive:new THREE.Color()},
          lookAt(x,y,z){this.lookTarget=[x,y,z];},rotateZ(a){this.rotation.z+=a;}};
        mesh.quaternion = {copy(){mesh.billboard=true;}};
        mesh.userData = {kind}; meshes.push(mesh);
      }
      mesh.visible=true;mesh.billboard=false;mesh.lookTarget=null;
      mesh.position.set(0,0,0);mesh.scale.set(1,1,1);mesh.rotation.set(0,0,0);mesh.rotation.order='XYZ';
      mesh.material.color.setRGB(1,1,1);mesh.material.emissive.setRGB(0,0,0);mesh.material.opacity=1;
      return mesh;
    };
    fx.ribbon = () => {
      let ribbon = fx.ribbons.find(x=>!x.busy);
      if(!ribbon){ribbon={pts:Array.from({length:24},()=>new THREE.Vector3()),color:new THREE.Color(),width:.1,mesh:{},build(cam,bright=1){this.brightness=bright;}};fx.ribbons.push(ribbon);}
      ribbon.busy=true;ribbon.mesh.visible=true;ribbon.count=0;ribbon.taper=1;
      return ribbon;
    };
    const particles = pool => Array.from({length:pool.n},(_,i)=>[
      ...pool.pos.slice(i*3,i*3+3),...pool.vel.slice(i*3,i*3+3),pool.life[i],pool.maxLife[i],pool.alpha0[i],pool.size0[i],
      pool.size[i],pool.col[i*4+3],pool.rot[i],pool.spin[i],pool.grav[i],pool.drag[i],pool.grow[i],
      CELL_NAMES[pool.cell[i]],...pool.col.slice(i*4,i*4+3),pool.align[i]!==0]);
    const snapshot = () => ({tasks:fx.tasks.length,add:particles(fx.add),alpha:particles(fx.alpha),pieces:[...pieces],shakes:[...shakes],
      meshes:meshes.filter(m=>m.visible).map(m=>({kind:m.kind,position:m.position.toArray(),scale:m.scale.toArray(),
        rotation:[m.rotation.x,m.rotation.y,m.rotation.z],order:m.rotation.order,color:m.material.color.toArray(),
        emissive:m.material.emissive.toArray(),opacity:m.material.opacity,lookTarget:m.lookTarget,billboard:m.billboard})),
      ribbons:fx.ribbons.filter(r=>r.busy).map(r=>({points:r.pts.slice(0,r.count).map(p=>p.toArray()),width:r.width,taper:r.taper,color:r.color.toArray(),brightness:r.brightness}))});
    return {THREE,fx,snapshot,primitives:context.primitives,SourceMoves:context.SourceMoves,nextRandom:math.random,step(dt){pieces.length=0;fx.update(dt);}};
  };
};
