const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
async function main(){
 const THREE=await import(require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')));
 const source=fs.readFileSync(path.join(root,'js/legends.js'),'utf8');
 const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/content/companion-models.json')));
 const models=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
 const context={ROLE_FALLBACK:catalog.fallbacks};vm.createContext(context);
 vm.runInContext(source.slice(source.indexOf('const clipOf ='),source.indexOf('// ---------- view ----------'))+';this.policy={stepsFor,clipOf,FX};',context);
 const plans=[];
 for(const [kind,visuals] of Object.entries({bases:catalog.bases,forms:catalog.forms}))for(const [id,visual] of Object.entries(visuals)){
  const model=models.sources[visual.source],motion=JSON.parse(zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',model.motion.file))));
  const clips=motion.clips.map(c=>c.authored==null?c.name:models.assets[model.asset].animations[c.authored]);
  const t={clips:Object.fromEntries(clips.map(name=>[name,{name}])),item:{data:{clips:visual.clips}}};
  for(const role of ['idle','run','dance','happy','sad','taunt','eat','wave','hit','unknown'])for(const mode of ['still','moving','noMixer']){
   let steps=mode==='still'?context.policy.stepsFor(t,role):null;
   if(!steps)steps=[{clip:null,len:role==='dance'?2.6:role==='wave'?1.4:visual.procedural[role]==='slump'?1.6:.9,fx:visual.procedural[role]||'hop'}];
   const expected=steps.map(s=>({clip:s.clip==null?null:context.policy.clipOf(t,s.clip).name,loops:s.loops||1,speed:s.speed||1,length:s.len||.9,effect:s.fx||null}));
   plans.push({kind,id,clips,role,mode,expected});
  }
 }
 const effects=[];
 for(const effect of Object.keys(context.policy.FX))for(const k of [0,.01,.17,.35,.5,.73,.99,1])for(const loops of [1,2,4])for(const duration of [.9,1.4,2.6])for(const scale of [.6,1.2]){
  const v=new THREE.Object3D();v.userData={s:scale};v.rotation.y=.37;v.scale.setScalar(scale);
  context.policy.FX[effect](v,k,loops,duration);v.updateMatrix();
  effects.push({effect,k,loops,duration,scale,yaw:.37,position:v.position.toArray(),rotation:[v.rotation.x,v.rotation.y,v.rotation.z],size:v.scale.toArray(),matrix:v.matrix.toArray()});
 }
 fs.writeFileSync(path.join(root,'native/tests/fixtures/companion-motion.json.gz'),zlib.gzipSync(JSON.stringify({plans,effects})));
 console.log(`Exported ${plans.length} original companion plans and ${effects.length} procedural transforms.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
