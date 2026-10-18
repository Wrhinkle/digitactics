// Rig analysis is an asset-build step. The shipped client consumes immutable bindings and runs C# choreography/retargeting.
const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
async function main(){
 const {THREE,GLTFParser,GLTFMaterialsUnlitExtension}=await require('./three-loader.cjs')();
 const models=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
 const companions=JSON.parse(fs.readFileSync(path.join(root,'native/content/companion-models.json')));
 const motion=read('model-motion.js').replace("from 'three'",`from '${require('url').pathToFileURL(path.join(root,'vendor/three/build/three.module.js')).href}'`);
 const {modelClips}=await import('data:text/javascript;base64,'+Buffer.from(motion).toString('base64'));
 const ctx={THREE,modelClips,DT:{Cosmetics:{register(){}}},C:{equipped:()=> 'floss'}};vm.createContext(ctx);
 const mystery=read('mystery-content.js'),source=read('dances.js'),render=read('render3d.js');
 vm.runInContext(mystery.slice(0,mystery.indexOf('export const killThemes')).replace(/export /g,'')+';const mysteryDances=dances;const TAU=Math.PI*2,DEG=Math.PI/180;',ctx);
 vm.runInContext(source.slice(source.indexOf('const WORDS ='),source.indexOf('// ---------- LegendView driver')).replace(/export /g,'')+';this.Dancer=Dancer;this.applyRoot=applyRoot;',ctx);
 vm.runInContext(render.slice(render.indexOf('const IDLE_RE'),render.indexOf('// undefined = still loading')).replace('export function','function')+';this.prepareTemplate=prepareTemplate;',ctx);
 const bindings={},fixtures=[];
 const sources=[...new Set([...Object.values(companions.bases),...Object.values(companions.forms)].map(v=>v.source))];
 for(const key of sources){
  const source=models.sources[key],asset=models.assets[source.asset];
  const bytes=zlib.gunzipSync(fs.readFileSync(path.join(root,'native/artifacts/model-content',asset.file))),len=bytes.readUInt32LE(12),json=JSON.parse(bytes.subarray(20,20+len)),binary=bytes.subarray(28+len);
  const parser=new GLTFParser(json,{manager:THREE.DefaultLoadingManager});parser.loadImageSource=async()=>new THREE.Texture();parser.loadBuffer=async()=>binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength);
  parser.extensions.KHR_materials_unlit=new GLTFMaterialsUnlitExtension();
  const gltf=await new Promise((resolve,reject)=>parser.parse(resolve,reject));
  const nodes=await Promise.all(json.nodes.map((_,i)=>parser.getDependency('node',i)));
  const template=ctx.prepareTemplate(gltf,source.metadata.companion?null:source.metadata);
  const dancer=new ctx.Dancer(template.root);
  const binding={limbless:dancer.limbless,stubby:dancer.stubby,yaw:dancer.rig.yaw,bones:dancer.E.map(e=>{
   const bone=dancer.rig.bones[e.bi],node=nodes.indexOf(bone.bone);if(node<0)throw Error('Unmapped bone '+key+'/'+bone.name);
   return {node,kind:e.kind,parentEntry:e.pe,direction:bone.dir.toArray(),parentRotation:bone.P0.toArray(),restRotation:bone.q0.toArray(),side:e.side||0,share:e.share??1,index:e.n||0,limit:e.lim||0};
  })};
  if(!JSON.stringify(binding).includes('null'))bindings[key]=binding;else throw Error('Nonfinite dance binding '+key);
  const samples=[];
  for(const id of ['floss','robot','windmill','macarena','mystery_vault_dance_20'])for(const time of [.137,1.71,7.199,17.983]){
   // Each sample starts at the authored idle pose; this also exercises blend-from-idle.
   for(let i=0;i<dancer.bones.length;i++)dancer.bones[i].quaternion.copy(dancer.rig.bones[i].q0);
   const weight=time<1?.3:1;dancer.set(id);const rt=dancer.apply(time,weight);
   const obj=new THREE.Object3D();obj.position.set(.1,.2,-.3);obj.rotation.set(.17,.37,-.12);obj.scale.setScalar(1.2);ctx.applyRoot(obj,rt,1.2);obj.updateMatrix();delete rt.fq;
   samples.push({id,time,weight,root:rt,matrix:obj.matrix.toArray(),rotations:dancer.E.map(e=>dancer.bones[e.bi].quaternion.toArray())});
  }
  fixtures.push({source:key,samples});
  template.root.traverse(o=>{o.geometry?.dispose();for(const m of [].concat(o.material||[]))m.dispose();});
 }
 fs.writeFileSync(path.join(root,'native/content/dance-bindings.json'),JSON.stringify({version:1,bindings}));
 fs.writeFileSync(path.join(root,'native/tests/fixtures/dance-bindings.json.gz'),zlib.gzipSync(JSON.stringify(fixtures)));
 console.log(`Exported ${sources.length} actual companion rigs and ${fixtures.reduce((n,f)=>n+f.samples.length,0)} original dance samples.`);
}
main().catch(e=>{console.error(e);process.exitCode=1;});
