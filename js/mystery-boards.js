import { rewards } from './mystery-content.js';

// Scenery stays outside the playable slab; each arena shares its geometry and materials.
const THEMES = [
  ['Lantern Orchard','Warm lanterns sway between layered orchard trees.','#213e48','#507354','#314a42','#204653','#ffd481','#654636','#c39a60'],
  ['Lily Observatory','Water lilies surround a slowly turning brass armillary.','#233e58','#496c68','#2b4d59','#1d5265','#9de9de','#476b76','#c8aa72'],
  ['Honeycomb Haven','Amber hive cells shelter a quiet flight of bees.','#493f42','#8a774a','#68533d','#3e5361','#ffcb58','#775133','#e5ac48'],
  ['Cloudmill Landing','Timber windmills turn above soft cloud terraces.','#49677d','#879b9e','#597686','#668ca0','#e8f5ff','#57738b','#dbb486'],
  ['Petal Pavilion','A layered garden pavilion framed by drifting blossoms.','#44384f','#6c7962','#4c5553','#374d61','#f6b9d4','#744b58','#d9b280'],
  ['Prism Vault','A faceted portal illuminates angular crystal terraces.','#17152c','#36344f','#24263d','#171d32','#c5a0ff','#4d4669','#a7b6dd'],
  ['Orbital Foundry','Warm reactor cores hum between gantries and flywheels.','#152433','#344857','#202f40','#142436','#68e2f5','#4b5c6b','#d59655'],
  ['Crystal Relay','Five linked pylons carry a measured pulse of light.','#272039','#4a3a55','#2c2c44','#1d253c','#ff8fc4','#645475','#b4a5d5'],
  ['Eclipse Sanctuary','Split pillars gather around a luminous eclipse shrine.','#111c29','#334b4d','#22353e','#142b39','#a2efd5','#4f6b71','#d1c69a'],
  ['Meteor Garden','Veined meteorites drift above shallow crater beds.','#272238','#57505e','#36364d','#202940','#ffc87f','#6e6473','#b68d81'],
].map(([name,desc,sky,ground,rim,water,accent,dark,trim])=>({name,desc,sky,ground,rim,water,accent,dark,trim}));

function sceneKit(ctx,T,std,p) {
  const geometries=new Map(),materials=new Map(),roots=[];
  const geo=(name,...args)=>{const key=name+JSON.stringify(args);if(!geometries.has(key))geometries.set(key,new T[name](...args));return geometries.get(key);};
  const mat=(color,glow=0,metal=0,rough=.8)=>{const key=[color,glow,metal,rough].join(':');if(!materials.has(key))materials.set(key,std({color,emissive:color,emissiveIntensity:glow,metalness:metal,roughness:rough}));return materials.get(key);};
  const add=(geometry,material,x=0,y=0,z=0,o={})=>{
    const m=new T.Mesh(geometry,material);m.position.set(x,y,z);m.rotation.set(o.rx||0,o.ry||0,o.rz||0);
    m.scale.set(o.sx??o.s??1,o.sy??o.s??1,o.sz??o.s??1);m.castShadow=o.shadow!==false;m.receiveShadow=true;m.userData.mysteryScenery=true;(o.parent||ctx.g).add(m);if(!o.parent)roots.push(m);return m;
  };
  const group=(x,y,z)=>{const g=new T.Group();g.position.set(x,y,z);ctx.g.add(g);roots.push(g);return g;};
  const box=(w,h,d,m,x,y,z,o)=>add(geo('BoxGeometry',w,h,d),m,x,y,z,o);
  const sphere=(r,m,x,y,z,o)=>add(geo('SphereGeometry',r,12,8),m,x,y,z,o);
  const cylinder=(a,b,h,m,x,y,z,o={})=>add(geo('CylinderGeometry',a,b,h,o.segments||10),m,x,y,z,o);
  const ring=(r,t,m,x,y,z,o)=>add(geo('TorusGeometry',r,t,6,32),m,x,y,z,o);
  const beam=(from,to,width,m,o={})=>{
    const a=new T.Vector3(...from),b=new T.Vector3(...to),d=b.clone().sub(a),c=a.add(b).multiplyScalar(.5);
    const mesh=cylinder(width,width,d.length(),m,c.x,c.y,c.z,o);mesh.quaternion.setFromUnitVectors(new T.Vector3(0,1,0),d.normalize());return mesh;
  };
  // Explicit group-local metadata is exported and compared against browser ticks in the CPU suite.
  const animate=(g,{rotation=[0,0,0],bob=0,bobSpeed=1,phase=0,swing=0,swingSpeed=1}={})=>{
    g.updateMatrixWorld(true);const parent=g.matrixWorld.toArray(),inverse=g.matrixWorld.clone().invert();
    const kinetic={rotation,bob,bobSpeed,phase,swing,swingSpeed};
    g.traverse(o=>{if(o.isMesh)o.userData.nativeMotion={kind:'mystery-kinetic',matrix:inverse.clone().multiply(o.matrixWorld).toArray(),parent,kinetic};});
    const start=g.position.clone(),initial=g.quaternion.clone(),q=new T.Quaternion(),e=new T.Euler();
    ctx.tick((dt,t)=>{const angle=t+Math.sin(t*swingSpeed+phase)*swing;q.setFromEuler(e.set(rotation[0]*angle,rotation[1]*angle,rotation[2]*angle));g.quaternion.copy(initial).multiply(q);g.position.copy(start);g.position.y+=Math.sin(t*bobSpeed+phase)*bob;});
  };
  const stone=mat(p.dark),trim=mat(p.trim,0,.25,.6),glow=mat(p.accent,.35,.15,.45);
  const terrace=(x,z,r=1.7,h=.25)=>{cylinder(r,r+.18,h,stone,x,h/2,z,{segments:8});cylinder(r+.03,r+.03,.08,trim,x,h+.04,z,{segments:8});};
  const crystal=(x,y,z,h,m=glow,o={})=>add(geo('OctahedronGeometry',1),m,x,y,z,{sx:h*.32,sy:h,sz:h*.32,...o});
  const fitLandmark=(relocateCenter=true)=>{
    // Tall center landmarks sit at the open right shore, where the live camera can show their entire silhouette.
    // A parent translation preserves all authored local motion and keeps their footprints beyond the slab.
    const place=(selected,parent)=>{
      ctx.g.add(parent);parent.updateMatrixWorld(true);
      for(const o of selected){parent.add(o);o.traverse(child=>{const m=child.userData.nativeMotion;if(m?.kind==='mystery-kinetic'){
        m.parent=new T.Matrix4().fromArray(m.parent).premultiply(parent.matrixWorld).toArray();m.kinetic={...m.kinetic,bob:m.kinetic.bob*parent.scale.y};
      }});}
    };
    if(relocateCenter){const shore=new T.Group();shore.position.set(10.8,0,11);place(roots.filter(o=>Math.abs(o.position.x)<=5&&o.position.z<-7),shore);}
    // Keep distant crowns and spires inside the default Solo viewport, without changing their ground footprint.
    const back=new T.Group();back.scale.y=.72;place(roots.filter(o=>Math.abs(o.position.x)>5&&o.position.z<-7),back);
  };
  return {p,geo,mat,add,group,box,sphere,cylinder,ring,beam,animate,stone,trim,glow,terrace,crystal,fitLandmark};
}

function lanternOrchard(k) {
  const {mat,cylinder,box,sphere,ring,group,animate,terrace,stone,trim,glow}=k,leaf=mat('#527e63'),wood=mat('#654636');
  for(const [i,[x,z]] of [[-10.5,-3.5],[10.5,-3.5],[-8.4,-8.5],[8.4,-8.5]].entries()){
    terrace(x,z,1.45);cylinder(.18,.28,2.5,wood,x,1.35,z);
    cylinder(.2,1.28,1.1,leaf,x,2.4,z,{segments:7});cylinder(.05,.95,.85,leaf,x,3.15,z,{segments:7});sphere(.6,leaf,x,3.45,z,{sy:.5});
    for(const dx of [-.8,.8]){const g=group(x+dx,1.9,z+.15);cylinder(.25,.25,.6,glow,0,0,0,{parent:g,segments:8});
      cylinder(.3,.3,.08,trim,0,.34,0,{parent:g,segments:8});cylinder(.3,.3,.08,trim,0,-.34,0,{parent:g,segments:8});
      ring(.15,.025,stone,0,.48,0,{parent:g});box(.035,.3,.035,trim,0,-.52,0,{parent:g});animate(g,{rotation:[0,.06,0],bob:.055,bobSpeed:.8,phase:i+dx});}
  }
  for(const x of [-2.2,2.2]){cylinder(.2,.27,2.9,wood,x,1.45,-9.8);box(.5,.13,.65,trim,x,2.95,-9.8);}
  box(5.3,.22,.5,wood,0,2.65,-9.8);box(5.7,.14,.7,trim,0,2.95,-9.8);
  for(const x of [-1.4,0,1.4]){const g=group(x,2.1,-9.8);sphere(.28,glow,0,0,0,{parent:g,sy:1.2});cylinder(.025,.025,.3,trim,0,.42,0,{parent:g});animate(g,{bob:.04,bobSpeed:.7,phase:x});}
}

function lilyObservatory(k) {
  const {mat,cylinder,sphere,ring,group,animate,terrace,trim,glow}=k,water=mat('#246b78',.08,.35,.25),pad=mat('#4d927e'),petal=mat('#c9efdf');
  for(const [x,z] of [[-10.3,-3.8],[10.3,-3.8],[-8,-8.6],[8,-8.6]]){
    terrace(x,z,1.65,.2);cylinder(1.48,1.48,.08,water,x,.32,z,{segments:24});ring(1.57,.08,trim,x,.34,z,{rx:Math.PI/2});
    for(let j=0;j<3;j++){const a=j*2.1,px=x+Math.cos(a)*.72,pz=z+Math.sin(a)*.72;cylinder(.4,.4,.04,pad,px,.4,pz,{segments:12});
      for(let q=0;q<5;q++){const t=q*Math.PI*2/5;sphere(.22,petal,px+Math.cos(t)*.16,.48,pz+Math.sin(t)*.16,{sx:.65,sy:.35,sz:1.15,ry:-t});}sphere(.09,glow,px,.55,pz);}
  }
  terrace(0,-10,2,.4);cylinder(.45,.75,1.25,trim,0,1,-10);const g=group(0,2.65,-10);
  ring(1.2,.06,trim,0,0,0,{parent:g,rx:.7});ring(1.18,.06,trim,0,0,0,{parent:g,ry:1.1});ring(1.15,.045,glow,0,0,0,{parent:g,rx:Math.PI/2});sphere(.38,glow,0,0,0,{parent:g});sphere(.16,trim,1.2,0,0,{parent:g});animate(g,{rotation:[0,.12,0]});
}

function honeycombHaven(k) {
  const {mat,cylinder,sphere,group,animate,terrace,trim,glow}=k,wax=mat('#b48036'),inner=mat('#694722'),wing=mat('#e6e8c8',.05),black=mat('#3d3637');
  for(const [i,[x,z]] of [[-10.2,-4],[10.2,-4],[0,-9.9]].entries()){
    terrace(x,z,1.8);for(let row=0;row<2;row++)for(let col=0;col<3;col++){const px=x+(col-1)*.86+(row? .42:0),py=.9+row*.75;
      cylinder(.51,.51,.48,wax,px,py,z,{rx:Math.PI/2,segments:6});cylinder(.36,.36,.5,inner,px,py,z+.02,{rx:Math.PI/2,segments:6});cylinder(.28,.28,.04,glow,px,py-.05,z+.29,{rx:Math.PI/2,segments:6});}
    sphere(.75,glow,x,.36,z+.7,{sx:1.4,sy:.08});for(const dx of [-.5,.5])sphere(.1,glow,x+dx,.55,z+.28,{sy:2});
    const g=group(x-.9,2.6,z+.7);sphere(.23,trim,0,0,0,{parent:g,sx:1.5});cylinder(.235,.235,.11,black,0,0,0,{parent:g,rz:Math.PI/2});
    for(const dz of [-.2,.2])sphere(.21,wing,0,.18,dz,{parent:g,sx:1.4,sy:.12});animate(g,{rotation:[0,.23,0],bob:.17,bobSpeed:1.1,phase:i*2});
  }
}

function cloudmillLanding(k) {
  const {mat,cylinder,box,sphere,ring,group,animate,trim,stone}=k,cloud=mat('#d5e3eb'),wood=mat('#816d60'),sail=mat('#e7ddc4');
  for(const [i,[x,z,h]] of [[-10.8,-4.7,2.5],[10.8,-4.7,3.1],[0,-10.2,3.6]].entries()){
    for(let q=0;q<4;q++){const a=q*Math.PI/2;sphere(.85,cloud,x+Math.cos(a)*.7,.35,z+Math.sin(a)*.7,{sy:.45});}
    cylinder(.43,.72,h,wood,x,h/2+.4,z,{segments:8});cylinder(0,.7,.8,stone,x,h+.8,z,{segments:8});box(.28,.6,.06,trim,x,h*.6,z+.64);
    const g=group(x,h+.25,z+.75);sphere(.2,trim,0,0,0,{parent:g});
    for(let q=0;q<4;q++){const a=q*Math.PI/2;box(.12,2.7,.12,wood,0,0,0,{parent:g,rz:a});box(.48,.95,.08,sail,Math.sin(a)*.85,Math.cos(a)*.85,.05,{parent:g,rz:-a});}
    animate(g,{rotation:[0,0,.18+i*.025]});
  }
  for(const x of [-6.8,6.8]){cylinder(.9,1,.25,cloud,x,.2,-9.1);ring(.55,.055,trim,x,.36,-9.1,{rx:Math.PI/2});}
}

function petalPavilion(k) {
  const {mat,cylinder,box,sphere,group,animate,terrace,trim,glow}=k,wood=mat('#74525b'),blossom=mat('#dc9fbd'),roof=mat('#6b617c');
  for(const [i,[x,z]] of [[-10.6,-4.2],[10.6,-4.2],[-7.5,-9],[7.5,-9]].entries()){
    terrace(x,z,1.4);cylinder(.14,.24,2.2,wood,x,1.2,z);
    for(let j=0;j<3;j++){const a=j*2.1;sphere(.8,blossom,x+Math.cos(a)*.45,2.3+(j%2)*.25,z+Math.sin(a)*.4,{sy:.55});}
    const g=group(x,1.65,z+.3);for(let j=0;j<3;j++)sphere(.14,glow,Math.cos(j*2.1),j*.3,Math.sin(j*2.1)*.6,{parent:g,sx:1.3,sy:.2});animate(g,{rotation:[0,.1,0],bob:.18,bobSpeed:.5,phase:i});
  }
  terrace(0,-10,2.4,.35);for(const x of [-1.5,1.5])for(const z of [-10.8,-9.2]){cylinder(.13,.16,2.3,wood,x,1.55,z);box(.35,.14,.35,trim,x,2.7,z);}
  for(let j=0;j<3;j++){const w=4.2-j*.8,y=2.85+j*.35;box(w,.18,2.7-j*.45,roof,0,y,-10);for(const x of [-w/2,w/2])box(.5,.13,2.7-j*.45,trim,x,y+.1,-10,{rz:Math.sign(x)*.35});}
  const g=group(0,2.15,-10);sphere(.28,glow,0,0,0,{parent:g,sy:1.3});animate(g,{bob:.04,bobSpeed:.6});
}

function prismVault(k) {
  const {box,ring,group,animate,terrace,crystal,stone,trim,glow}=k;
  for(const [i,[x,z]] of [[-10.6,-4.4],[10.6,-4.4],[-7.6,-9],[7.6,-9]].entries()){
    terrace(x,z,1.4);crystal(x,1.65,z,1.4,glow,{ry:i*.7,rz:.12});crystal(x+.65,.95,z+.3,.75,trim,{rz:-.22});crystal(x-.55,.8,z-.35,.6,trim,{rz:.2});
    box(2.1,.07,.18,trim,x,.36,z+.6,{ry:.45});
  }
  terrace(0,-10,2.65,.3);for(const x of [-2.1,2.1]){box(.5,3.4,.65,stone,x,1.95,-10,{rz:-Math.sign(x)*.12});box(.13,2.8,.09,glow,x,1.9,-9.65);}
  box(4.6,.5,.65,trim,0,3.65,-10);box(4.6,.22,.8,trim,0,.5,-10);
  const g=group(0,2.15,-10);crystal(0,0,0,1,glow,{parent:g});ring(1.25,.035,trim,0,0,0,{parent:g,rx:.35});animate(g,{rotation:[0,.17,0],bob:.09,bobSpeed:.7});
  for(const x of [-1,0,1])box(.6,.08,.6,glow,x,.38,-8.2,{ry:Math.PI/4});
}

function orbitalFoundry(k) {
  const {mat,cylinder,box,sphere,ring,group,animate,terrace,stone,trim,glow}=k,heat=mat('#f5ad64',.45,.1,.45),steel=mat('#708396',0,.5,.5);
  for(const [i,x] of [-10.7,10.7].entries()){
    terrace(x,-4.5,1.75);box(1.45,1.6,1.35,stone,x,1.15,-4.5);box(1.65,.18,1.6,trim,x,2,-4.5);
    const g=group(x,2.3,-3.7);ring(.9,.12,steel,0,0,0,{parent:g});sphere(.27,heat,0,0,0,{parent:g});
    for(let j=0;j<8;j++){const a=j*Math.PI/4;box(.2,.35,.22,trim,Math.cos(a)*.95,Math.sin(a)*.95,0,{parent:g,rz:a-Math.PI/2});box(.08,1.7,.08,steel,0,0,0,{parent:g,rz:a});}
    animate(g,{rotation:[0,0,(i?1:-1)*.16]});
    for(const dx of [-1.05,1.05])box(.16,2.8,.16,steel,x+dx,1.7,-4.9);box(2.4,.18,.18,trim,x,3.1,-4.9);
  }
  terrace(0,-10,2.2,.4);cylinder(1.05,1.3,.5,steel,0,.75,-10);cylinder(.8,.8,2.1,stone,0,1.8,-10);
  for(let j=0;j<6;j++){const a=j*Math.PI/3;box(.13,1.7,.13,glow,Math.cos(a)*.85,1.8,-10+Math.sin(a)*.85);}
  const g=group(0,3.1,-10);sphere(.55,heat,0,0,0,{parent:g});ring(.95,.07,trim,0,0,0,{parent:g,rx:.65});animate(g,{rotation:[0,.13,0],bob:.06,bobSpeed:.6});
  for(const x of [-3,3]){box(.2,2.7,.25,steel,x,1.6,-10);box(1.5,.18,.25,trim,x-Math.sign(x)*.55,2.95,-10);}
}

function crystalRelay(k) {
  const {cylinder,sphere,ring,beam,group,animate,terrace,crystal,stone,trim,glow}=k,nodes=[[-10.8,-4],[-8,-8.6],[0,-10.4],[8,-8.6],[10.8,-4]];
  nodes.forEach(([x,z],i)=>{
    const h=i===2?.55:1;
    terrace(x,z,1.15);cylinder(.25,.45,1.5*h,stone,x,1.1*h,z,{segments:6});crystal(x,2.1*h,z,.85*h,glow,{ry:Math.PI/4});
    for(const dx of [-.65,.65])beam([x+dx,.4,z],[x+dx*.45,1.6*h,z],.07,trim);
    const g=group(x,2.1*h,z);ring(.78,.035,trim,0,0,0,{parent:g,rx:Math.PI/2});sphere(.12,glow,.78,0,0,{parent:g});animate(g,{rotation:[0,.35,0],bob:.05,bobSpeed:.8,phase:i});
    if(i){const [px,pz]=nodes[i-1];beam([px,.35,pz],[x,.35,z],.12,stone);beam([px,.48,pz],[x,.48,z],.025,glow);}
  });
}

function eclipseSanctuary(k) {
  const {mat,cylinder,box,sphere,ring,group,animate,terrace,trim,glow}=k,stone=mat('#506b71'),shadow=mat('#172b35');
  for(const [x,z] of [[-10.5,-4.6],[10.5,-4.6],[-7.2,-9],[7.2,-9]]){
    terrace(x,z,1.35);for(const dx of [-.55,.55]){box(.38,2.5,.6,stone,x+dx,1.6,z);box(.55,.12,.78,trim,x+dx,2.9,z);}
    sphere(.25,glow,x,2.2,z);ring(.95,.05,trim,x,.48,z,{rx:Math.PI/2});
  }
  terrace(0,-10.2,2.3,.45);cylinder(.55,.9,1.2,stone,0,1.1,-10.2);ring(1.45,.16,glow,0,2.8,-10.2);sphere(1.4,shadow,.4,2.98,-9.98,{sz:.15});
  const g=group(0,2.8,-10.2);for(let i=0;i<9;i++){const a=i*Math.PI*2/9;sphere(.085,trim,Math.cos(a)*1.78,Math.sin(a)*1.78,0,{parent:g});}animate(g,{rotation:[0,0,.07]});
}

function meteorGarden(k) {
  const {geo,mat,add,cylinder,sphere,ring,group,animate,terrace,stone,trim,glow}=k,rock=mat('#7a6879',0,.15,.8);
  for(const [i,[x,z]] of [[-10.5,-4.1],[10.5,-4.1],[-7.5,-9],[7.5,-9],[0,-10.5]].entries()){
    terrace(x,z,1.45,.16);cylinder(1.15,1.15,.05,stone,x,.22,z,{segments:16});ring(1.2,.15,trim,x,.3,z,{rx:Math.PI/2});
    const g=group(x,1.55,z);add(geo('IcosahedronGeometry',.72,0),rock,0,0,0,{parent:g,sx:1.15});ring(.64,.025,glow,0,0,0,{parent:g,rx:.6,ry:.4});ring(.67,.025,glow,0,0,0,{parent:g,ry:1.5});
    for(const dx of [-1,1])add(geo('IcosahedronGeometry',.18,0),rock,dx*.85,.25*dx,.4,{parent:g});animate(g,{rotation:[.025,.11,.02],bob:.1,bobSpeed:.55,phase:i});
    for(let j=0;j<4;j++){const a=j*Math.PI/2;sphere(.23,trim,x+Math.cos(a)*1.2,.35,z+Math.sin(a)*1.2,{sy:.6});}
  }
}

const BUILDERS=[lanternOrchard,lilyObservatory,honeycombHaven,cloudmillLanding,petalPavilion,prismVault,orbitalFoundry,crystalRelay,eclipseSanctuary,meteorGarden];

export function mysteryBoards({THREE:T,std,island,sea}) {
  return rewards('board',s=>{
    const index=s.p*5+s.i,p=THEMES[index];
    return {...p,icon:'◇',fog:[p.sky,35,75],hemi:['#dbe9f3',s.p?'#34344c':'#4b5153',1.35],sun:[s.p?'#e5e3ff':'#fff0d9',2],
      slab:s.p?'#29364a':'#425455',benchSlab:s.p?'#283342':'#394b52',slot:'#4c5e6b',ped:'#60727f',tiles:{mine:'#345b8b',enemy:'#85435c',enemyPlan:'#503347'},
      props(ctx){sea(ctx,{color:p.water,amp:.035,rough:.55,metal:.15,seg:30,speed:.55});island(ctx,p.ground,p.rim);const kit=sceneKit(ctx,T,std,p);BUILDERS[index](kit);kit.fitLandmark(index!==7);}
    };
  });
}
