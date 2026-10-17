// Original event handlers and UnitView motion, with only GPU/audio/DOM effects stubbed.
const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),vm=require('node:vm');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/render3d.js'),'utf8');
const {load}=require('../../tools/headless.cjs');const DT=load({strict:true});
function section(start,end){const at=source.indexOf(start),to=source.indexOf(end,at);if(at<0||to<0)throw Error(`Missing renderer boundary ${start}`);return source.slice(at,to);}
async function main(){
  const {THREE}=await require('./three-loader.cjs')();
  const methods=[section('  combatCell(', '\n  viewTeam('),section('  viewTeam(', '\n  ownerColor('),section('  unitWorld(', '\n  // ---------- view sync'),section('  isUlt(', '\n  updateFx(')];
  const cases=[['dmg','heal'],['cast','leap'],['hit','shot'],['shot','beam']].map(([a,b])=>section(`        case '${a}':`,`        case '${b}':`)).join('\n');
  const prefix=section('  consume(cb) {',"        case 'dmg':");
  const consume=prefix+cases+'\n      }\n    }\n  }';
  const update=section('  update(dt, now) {','\n  dispose(scene)');
  const scale=section('      if (d.dying) {','\n      v.ringMat.color.set');
  const label=source.split('\n').find(l=>l.includes('if (v.castUntil && (!cb || now > v.castUntil))'));
  const cell=source.match(/^const cellWorld = .*$/m)[0];
  const context={THREE,sfx(){},elemColor(){return '#ffffff';},Math};
  const factory=vm.runInNewContext(`(()=>{const COLS=7,ROWS=8,SQ3=Math.sqrt(3);${cell};return ()=>new(class{${methods.join('\n')} ${consume}})();})()`,context);
  const updateUnit=vm.runInNewContext(`(new(class{${update}})).update`,context);
  const extras=vm.runInNewContext(`(v,d,cb,now)=>{${scale}\n${label}}`,context);
  const inputs=JSON.parse(zlib.gunzipSync(fs.readFileSync(path.join(root,'native/tests/fixtures/scenes.json.gz'))));
  const output=[];
  for(const trace of inputs)for(const mirror of [false,true]){
    const [set,pace]=trace.variant.split('/');DT.Sets.activate(set,pace);
    const entries=side=>side.map(u=>({line:u.line,stage:u.stage,c:u.column,r:u.row,items:u.items}));
    const cb=new DT.Combat({units:entries(trace.sideA),cols:trace.cols},{units:entries(trace.sideB),cols:trace.cols},{seed:trace.seed});
    const renderer=factory(),views=new Map(),calls=[];
    Object.assign(renderer,{mirror,combatCols:trace.cols,evIdx:0,celebrated:false,castCtx:null,leapFrom:null,
      setCombatWidth(){},viewOf(id){return views.get(id);},floatText(){},moves:{elemOf(){return 'neutral';},begin(){},target(){},basic(){}}});
    const samples=[];let ticks=0;
    for(const sample of trace.samples.filter(s=>s.mirror===mirror)){
      while(ticks<sample.ticks){cb.step();ticks++;}
      calls.length=0;renderer.consume(cb);
      const keep=new Set(),delta=ticks%3===0?.016:.09,frames=[];
      for(const d of sample.views){
        const u=cb.units[d.unitId];keep.add(u.id);let v=views.get(u.id);
        if(v&&(v.line!==u.line||v.stage!==u.stage)){calls.push(['remove',u.id]);views.delete(u.id);v=null;}
        if(!v){
          v={line:u.line,stage:u.stage,body:new THREE.Group(),group:new THREE.Group(),ring:new THREE.Group(),hasModel:true,flash:0,glow:null,lunge:null,
            materials:[{emissive:new THREE.Color(0),userData:{}}],elCast:{style:{setProperty(){}},classList:{add(){},remove(){}}},
            mixer:{update(dt){calls.push(['update',u.id,dt]);}},play(role){calls.push(['play',u.id,role]);return role!=='attack'||u.id%3!==0;}};
          v.body.rotation.y=d.yaw;views.set(u.id,v);calls.push(['create',u.id]);
        }
        v.yaw=d.yaw;v.group.position.set(d.position.x,d.position.y,d.position.z);
        updateUnit.call(v,delta,cb.t);extras(v,d,cb,cb.t);
        frames.push({unitId:u.id,position:v.group.position.toArray(),yaw:v.body.rotation.y,offset:v.body.position.toArray(),scale:v.group.scale.x,
          emission:v.materials[0].emissive.r,ring:v.ring.scale.x,castName:v.elCast.textContent??null,castVisible:!!v.castUntil});
      }
      for(const id of views.keys())if(!keep.has(id)){calls.push(['remove',id]);views.delete(id);}
      samples.push({ticks,delta,hover:sample.hover,frames,calls:calls.map(c=>[...c])});
    }
    output.push({...trace,samples,mirror});
  }
  fs.writeFileSync(path.join(root,'native/tests/fixtures/choreography.json.gz'),zlib.gzipSync(JSON.stringify(output)));
  console.log(`Exported ${output.length} original-renderer combat choreography traces.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
