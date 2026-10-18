// Execute the unchanged render3d.js event cases and updateFx method. Geometry is
// exported from that file's GEO declaration, not reconstructed from native code.
const fs = require('node:fs'), path = require('node:path'), {pathToFileURL} = require('node:url');
const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'js/render3d.js'), 'utf8').replace(/\r\n/g,'\n');
const block = (text, marker) => {
  const start = text.indexOf(marker);
  if (start < 0) throw new Error(`Source marker missing: ${marker}`);
  const brace = text.indexOf('{', start);
  let depth = 1, end = brace + 1;
  for (; end < text.length && depth; end++) {
    if (text[end] === '{') depth++;
    if (text[end] === '}') depth--;
  }
  if (depth) throw new Error(`Unclosed source block: ${marker}`);
  return text.slice(brace + 1, end - 1);
};

(async () => {
  const THREE = await import(pathToFileURL(path.join(root, 'vendor/three/build/three.module.js')));
  const GEO = new Function('THREE', `return {${block(source, 'const GEO =')}};`)(THREE);
  const DT = require('../../tools/headless.cjs').load({strict:true});
  const geometry = {};
  for (const [name, key] of [['legacyBeam','beam'], ['legacyBurst','burst'], ['legacyShield','ball']]) {
    const g = GEO[key], vertices = [], p = g.attributes.position, n = g.attributes.normal, uv = g.attributes.uv;
    for (let i = 0; i < p.count; i++)
      vertices.push(p.getX(i),p.getY(i),p.getZ(i),n.getX(i),n.getY(i),n.getZ(i),uv.getX(i),1-uv.getY(i),0,0,0,0,0,0,0,0);
    geometry[name] = {vertices,indices:g.index ? Array.from(g.index.array) : Array.from({length:p.count},(_,i)=>i)};
  }
  const out = path.join(root,'native/content/move-effects');
  fs.mkdirSync(out,{recursive:true});
  fs.writeFileSync(path.join(out,'legacy-geometry.json'),JSON.stringify(geometry)+'\n');

  const casesStart = source.indexOf("        case 'beam': {", source.indexOf('  consume(cb)'));
  const casesEnd = source.indexOf('\n      }\n    }\n    // the cast', casesStart);
  if (casesStart < 0 || casesEnd < 0) throw new Error('Legacy source event cases not found.');
  const consume = new Function('THREE','DT','GEO','cellWorld','COLS','ROWS','sfx',
    `return function(e,from,to,cb) {switch(e.type) {${source.slice(casesStart,casesEnd)}}};`
  )(THREE,DT,GEO,(c,r)=>({x:c,z:r}),7,8,()=>{});
  const update = new Function(`return function(now) {${block(source,'  updateFx(now)')}};`)();
  const addMeshFx = new Function(`return function(mesh,t0,dur,update) {${block(source,'  addMeshFx(mesh, t0, dur, update)')}};`)();
  const vector = v => [v.x,v.y,v.z];
  const snapshot = host => host.fx.map(({mesh:m})=>({
    kind:m.geometry===GEO.beam?'legacyBeam':m.geometry===GEO.burst?'legacyBurst':'legacyShield',
    position:vector(m.position),scale:vector(m.scale),rotation:[m.rotation.x,m.rotation.y,m.rotation.z],order:m.rotation.order,
    color:[m.material.color.r,m.material.color.g,m.material.color.b],opacity:m.material.opacity,
    depthWrite:m.material.depthWrite,side:m.material.side,blending:m.material.blending,renderOrder:m.renderOrder
  }));
  const rows = [];
  const run = (name, events, times) => {
    const host = {fx:[],tiles:[],mirror:false,isUlt:()=>false,viewTeam:team=>team,unitWorld:u=>u.point,
      scene:{add(){},remove(){}},addMeshFx};
    for (const e of events) {
      const from = {point:{x:e.from?.[0]??0,y:e.from?.[1]??0,z:e.from?.[2]??0},line:{attr:e.attribute??'Free'}};
      const to = {point:{x:e.to?.[0]??0,y:e.to?.[1]??0,z:e.to?.[2]??0}};
      const event = {type:e.kind,t:e.time,at:0,c:e.center?.[0]??0,r:e.center?.[2]??0,radius:e.radius??0,team:e.team??0};
      const unit = {point:{x:e.center?.[0]??0,y:e.center?.[1]??0,z:e.center?.[2]??0}};
      consume.call(host,event,from,to,{units:[unit]});
    }
    const initial = snapshot(host), frames = [];
    for (const time of times) {update.call(host,time);frames.push({time,meshes:snapshot(host)});}
    rows.push({name,events,initial,frames});
  };
  for (const attribute of Object.keys(DT.ATTR))
    run('beam/'+attribute,[{kind:'beam',attribute,time:10,from:[-1.3,4,-2.7],to:[3.2,-1,1.8]}],[10,10,10.1,10.2,10.299,10.3,11]);
  run('beam/zero-length',[{kind:'beam',attribute:'Data',time:0,from:[2,0,-3],to:[2,2,-3]}],[0,.15,.299,.3]);
  run('beam/reverse',[{kind:'beam',attribute:'Virus',time:5,from:[3,0,4],to:[-2,0,-5]}],[5,5.04,5.2,5.31]);
  for (const team of [0,1,2])
    run('burst/team-'+team,[{kind:'burst',time:3,center:[-2,7,1.4],radius:team,team}],[3,3.15,3.3,3.449,3.45,4]);
  run('shield/default',[{kind:'shield',time:4,center:[1.5,9,-3.2]}],[4,4,4.14,4.35,4.699,4.7,5]);
  run('all/paused-and-jump',[
    {kind:'beam',attribute:'Vaccine',time:0,from:[0,0,0],to:[1,0,1]},
    {kind:'burst',time:0,center:[0,0,0],radius:3,team:0},
    {kind:'shield',time:0,center:[1,0,1]}],[0,.1,.1,.1,10]);
  run('all/rewind',[
    {kind:'beam',attribute:'Free',time:2,from:[0,0,0],to:[-1,0,2]},
    {kind:'burst',time:2,center:[1,0,2],radius:1,team:1},
    {kind:'shield',time:2,center:[1,0,2]}],[2.1,1.99,2.1]);
  run('all/future-event',[
    {kind:'beam',attribute:'Free',time:2,from:[0,0,0],to:[-1,0,2]},
    {kind:'burst',time:2,center:[1,0,2],radius:1,team:1},
    {kind:'shield',time:2,center:[1,0,2]}],[1,2]);

  const palette=Object.fromEntries(Object.entries(DT.ATTR).map(([name,{color}])=>[name,{hex:color,linear:new THREE.Color(color).toArray()}]));
  const geometrySummary=Object.fromEntries(Object.entries(geometry).map(([name,g])=>[name,{vertices:g.vertices.length/16,indices:g.indices.length}]));
  const fixture=path.join(root,'native/tests/fixtures/legacy-combat-effects.json');
  fs.writeFileSync(fixture,JSON.stringify({schema:1,palette,geometry:geometrySummary,cases:rows})+'\n');
  console.log(`Exported 3 original legacy geometries and ${rows.length} source timing cases.`);
})().catch(error=>{console.error(error);process.exitCode=1;});
