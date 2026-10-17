// Texture-only forms omitted by a mesh-file inventory. Original geometry and authored clips are preserved.
const fs=require('fs'),path=require('path'),{execFileSync}=require('child_process');
const root=path.resolve(__dirname,'../..'),rips=path.join(__dirname,'rips/x'),inventory=require('./catalog-inventory.json').records,catalog=require('./catalog-imports.json');
const definitions=[
 ['MetalGreymon','chr327','BlackMetalGreymon'],['Leomon','chr409','IceLeomon'],['Coredramon_Green','chr218','Coredramon (Blue)'],
 ['Numemon','chr729','Geremon'],['Gatomon','chr043','BlackGatomon'],['Omnimon','chr757','Omnimon Zwart'],['WarGreymon','chr175','BlackWarGreymon'],
 ['MetalGarurumon','chr182','BlackMetalGarurumon'],['Sakuyamon','chr754','Kuzuhamon'],
 ['CS_Agumon','chr143','BlackAgumon'],['CS_Gabumon','chr713','BlackGabumon'],['CS_Devimon','chr730','IceDevimon'],['CS_Garurumon','chr714','BlackGarurumon'],
 ['CS_Gatomon','chr043','BlackGatomon'],['CS_Numemon','chr729','Geremon'],['CS_Omnimon','chr757','Omnimon Zwart'],
 ['L_Agumon','chr143','BlackAgumon'],['L_Agumon','chr143','BlackAgumon','Cyber Sleuth Textures'],['L_Gabumon','chr713','BlackGabumon'],
 ['L_Garurumon','chr714','BlackGarurumon'],['L_Greymon','chr712','BlackGreymon'],['L_Meramon','chr177','BlueMeramon'],['L_WereGarurumon','chr715','ShadowWereGarurumon'],
 ['L_Gallantmon','chr150','ChaosGallantmon'],['TS_Bearmon','chr924','Bearmon (Big)'],['TS_Bearmon','chr925','Bearmon (Little)'],['TS_Goburimon','chr577','Shamanmon'],
];
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):[path.join(d,e.name)]);
const run=(exe,args)=>execFileSync(exe,args,{windowsHide:true,stdio:'pipe',timeout:600000,maxBuffer:16e6});
const rows=definitions.map(([folder,code,form,textureFolder],i)=>{const source=inventory.find(r=>r.folder===folder),name='SKIN_'+folder+'_'+code+(textureFolder?'_cs':'');return {...source,name,form,newCode:code,textureFolder,file:'rip_'+name.toLowerCase()+'.glb'};});
const maps={_alias:{},_form:{},_input:{},_retexture:{}};
for(const r of rows){maps._alias[r.name]=r.folder;maps._form[r.name]=r.form;maps._input[r.name]=r.input;maps._retexture[r.name]={};
 const files=walk(path.join(rips,r.folder)).filter(f=>/\.png$/i.test(f)).sort((a,b)=>a.split(path.sep).length-b.split(path.sep).length);
 for(const f of files.filter(f=>path.basename(f).startsWith(r.code))){const target=path.basename(f).replace(r.code,r.newCode),p=files.find(p=>path.basename(p)===target&&(!r.textureFolder||p.includes(r.textureFolder)));
  if(p)maps._retexture[r.name][path.basename(f)]=path.relative(path.join(rips,r.folder),p).replaceAll('\\','/');}
 if(!Object.keys(maps._retexture[r.name]).length)throw Error('No alternate textures: '+r.name);
}
fs.writeFileSync(path.join(__dirname,'maps/skin-imports.json'),JSON.stringify(maps,null,2)+'\n');
fs.writeFileSync(path.join(__dirname,'skin-imports.json'),JSON.stringify(rows,null,2)+'\n');
if(!process.argv.includes('--register')){
 for(const r of rows){const out=path.join(root,'models/src',r.file);if(fs.existsSync(out))continue;
  const existing=catalog.find(x=>x.input===r.input),input=existing&&path.join(root,'models/src',existing.file);
  if(input&&fs.existsSync(input))fs.copyFileSync(input,out);else run(path.join(__dirname,'node_modules/fbx2gltf/bin/Windows_NT/FBX2GLTF.exe'),['-b','--pbr-metallic-roughness','-i',path.join(rips,r.input),'-o',out.replace(/\.glb$/,'')]);
  console.log('Imported '+r.name);
 }
 process.stdout.write(run(process.execPath,[path.join(__dirname,'patch-textures.cjs'),...rows.map(r=>r.name)]));
 process.stdout.write(run(process.execPath,[path.join(__dirname,'fix-upright.mjs'),...rows.map(r=>r.file)]));
 process.stdout.write(run(process.execPath,[path.join(__dirname,'optimize.mjs'),'--size','512','--meshopt','high',...rows.map(r=>r.file)]));
}
const index=JSON.parse(fs.readFileSync(path.join(root,'models/index.json'))),credits={};
const games={Links:'Digimon Links',CyberSleuth:'Digimon Story Cyber Sleuth: Complete Edition',TimeStranger:'Digimon Story Time Stranger'};
for(const r of rows){const key='skin:'+r.name;index[key]='models/files/'+r.file;if(!fs.existsSync(path.join(root,index[key])))throw Error('Missing '+index[key]);credits[key]={form:r.form,uid:'rip-'+r.name.toLowerCase(),rip:true,name:`${r.form} (${games[r.game]||r.game})`,url:r.source,author:'The Models Resource',license:'Game rip'};}
fs.writeFileSync(path.join(root,'models/index.json'),JSON.stringify(index,null,1)+'\n');const f=path.join(root,'js/models.js');let text=fs.readFileSync(f,'utf8').replace(/\n  \/\/ SKINS-START[\s\S]*?\/\/ SKINS-END\n/,'\n');text=text.replace('\n  // MODEL-OPTIONS',`\n  // SKINS-START (tools/convert/import-skins.cjs)\n  Object.assign(DT.MODEL_SOURCES, ${JSON.stringify(credits,null,2)});\n  // SKINS-END\n\n  // MODEL-OPTIONS`);fs.writeFileSync(f,text);console.log('Registered '+rows.length+' texture variants');
