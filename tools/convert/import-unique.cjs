// Import reviewed distinct forms without regenerating or deleting existing model mappings.
// node tools/convert/import-unique.cjs [--register] (registration only after optimization / visual review)
const fs=require('fs'),path=require('path'),vm=require('vm'),{execFileSync}=require('child_process');
const here=__dirname,root=path.resolve(here,'../..'),rips=path.join(here,'rips/x'),src=path.join(root,'models/src');
const manifest=require('./unique-imports.json'),index=require('./rips/index.json');
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):[path.join(d,e.name)]);
const games={Links:'Digimon Links',CyberSleuth:'Digimon Story Cyber Sleuth: Complete Edition',TimeStranger:'Digimon Story Time Stranger',NextOrder:'Digimon World: Next Order'};
const run=(exe,args)=>execFileSync(exe,args,{windowsHide:true,timeout:600000,stdio:'pipe'});
if(process.argv.includes('--register')){
 const mi=JSON.parse(fs.readFileSync(path.join(root,'models/index.json'))),credits={};
 for(const r of manifest){const file=`models/files/rip_${r.folder.toLowerCase()}.glb`;if(!fs.existsSync(path.join(root,file)))throw Error('Missing optimized '+file);
  const m=index.find(x=>x.name.replaceAll(' ','_')===(r.source||r.folder));if(!m)throw Error('Missing source '+r.folder);
  const key='unique:'+r.folder;mi[key]=file;credits[key]={form:r.form,uid:'rip-'+r.folder.toLowerCase(),rip:true,name:`${r.form} (${games[m.game]||m.game})`,url:m.page,author:'The Models Resource',license:'Game rip',...(r.anim===false?{anim:false}:{})};
 }
 fs.writeFileSync(path.join(root,'models/index.json'),JSON.stringify(mi,null,1)+'\n');
 const f=path.join(root,'js/models.js');let s=fs.readFileSync(f,'utf8').replace(/\n  \/\/ UNIQUE-START[\s\S]*?\/\/ UNIQUE-END\n/,'\n');
 s=s.replace('\n  // MODEL-OPTIONS',`\n  // UNIQUE-START (tools/convert/import-unique.cjs)\n  Object.assign(DT.MODEL_SOURCES, ${JSON.stringify(credits,null,2)});\n  // UNIQUE-END\n\n  // MODEL-OPTIONS`);fs.writeFileSync(f,s);console.log('Registered '+manifest.length+' distinct forms');
}else{
 fs.mkdirSync(src,{recursive:true});
 for(const r of manifest){const out=path.join(src,'rip_'+r.folder.toLowerCase()+'.glb');if(fs.existsSync(out)){console.log('KEEP '+r.folder);continue;}
  const all=walk(path.join(rips,r.source||r.folder)).sort((a,b)=>a.split(path.sep).length-b.split(path.sep).length),fbx=all.filter(f=>/\.fbx$/i.test(f)&&!/splituv/i.test(f));
  const pick=r.pick?all.find(f=>new RegExp(r.pick,'i').test(f)):fbx.find(f=>/with anim/i.test(f))||fbx.find(f=>/[\\/]animations[\\/]/i.test(f))||fbx[0]||all.find(f=>/\.obj$/i.test(f))||all.find(f=>/\.dae$/i.test(f)&&!/splituv|fix/i.test(f));if(!pick)throw Error('No source for '+r.folder);
  if(/\.obj$/i.test(pick))run(process.execPath,[path.join(here,'node_modules/obj2gltf/bin/obj2gltf.js'),'-i',pick,'-o',out,'-b']);
  else run(path.join(here,'node_modules/fbx2gltf/bin/Windows_NT/FBX2glTF.exe'),['-b','--pbr-metallic-roughness','-i',pick,'-o',out.replace(/\.glb$/,'')]);
  console.log('CONVERTED '+r.folder+' <- '+path.relative(rips,pick));
 }
 process.stdout.write(run(process.execPath,[path.join(here,'patch-textures.cjs'),...manifest.map(r=>r.folder)]));
 process.stdout.write(run(process.execPath,[path.join(here,'fix-upright.mjs'),...manifest.map(r=>'rip_'+r.folder.toLowerCase()+'.glb')]));
}
