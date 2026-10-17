// Resumable import of the full audited creature catalog. Native converters stay windowless.
const fs=require('fs'),path=require('path'),{execFile}=require('child_process'),{promisify}=require('util');
const run=promisify(execFile),here=__dirname,root=path.resolve(here,'../..'),src=path.join(root,'models/src'),rips=path.join(here,'rips/x');
const inventory=require('./catalog-inventory.json'),unique=require('./unique-imports.json');
const glbIds=file=>{try{const b=fs.readFileSync(file),j=JSON.parse(b.subarray(20,20+b.readUInt32LE(12)));return new Set((j.nodes||[]).concat(j.materials||[]).flatMap(n=>n.name?.match(/(?:chr|[gmdfu])\d+/ig)||[]).map(s=>s.toLowerCase()));}catch(_){return new Set();}};
const reused=unique.map(u=>({...u,file:'rip_'+u.folder.toLowerCase()+'.glb',ids:glbIds(path.join(src,'rip_'+u.folder.toLowerCase()+'.glb'))}));
const rows=inventory.records.filter(r=>r.status==='needs import').map(r=>{
 const match=reused.find(u=>(u.source||u.folder)===r.folder&&(u.ids.has(r.code)||r.code==='main')&&!u.retexture);
 const name='CAT_'+r.folder+'_'+r.code;
 return {...r,name,file:match?.file||'rip_'+name.toLowerCase()+'.glb',reused:!!match,form:match?.form||r.folder.replace(/^(CS|TS|L|NC|RD|NO)_/,'').replaceAll('_',' ')};
});
fs.writeFileSync(path.join(here,'catalog-imports.json'),JSON.stringify(rows,null,2)+'\n');
const maps={_alias:{},_form:{},_input:{}};for(const r of rows.filter(r=>!r.reused)){maps._alias[r.name]=r.folder;maps._form[r.name]=r.form;maps._input[r.name]=r.input;}
fs.writeFileSync(path.join(here,'maps/catalog-imports.json'),JSON.stringify(maps,null,2)+'\n');
const report=[],pending=rows.filter(r=>!r.reused),reportFile=process.argv[2]||path.join(src,'catalog-import-report.json');
let cursor=0;
async function worker(){while(cursor<pending.length){const r=pending[cursor++],out=path.join(src,r.file);try{
 if(fs.existsSync(out)&&fs.statSync(out).size>20){report.push({name:r.name,status:'kept'});continue;}
 const input=path.join(rips,r.input),temp=out.replace(/\.glb$/,'.import');
 if(/\.obj$/i.test(input))await run(process.execPath,[path.join(here,'node_modules/obj2gltf/bin/obj2gltf.js'),'-i',input,'-o',temp+'.glb','-b'],{windowsHide:true,timeout:600000,maxBuffer:16e6});
 else await run(path.join(here,'node_modules/fbx2gltf/bin/Windows_NT/FBX2glTF.exe'),['-b','--pbr-metallic-roughness','-i',input,'-o',temp],{windowsHide:true,timeout:600000,maxBuffer:16e6});
 fs.renameSync(temp+'.glb',out);report.push({name:r.name,status:'converted'});console.log(`${report.length}/${pending.length} ${r.name}`);
 }catch(e){report.push({name:r.name,status:'failed',error:String(e.message).slice(0,400)});console.log('FAILED '+r.name+' '+String(e.message).slice(0,150));}
 fs.writeFileSync(reportFile,JSON.stringify(report,null,2));
}}
(async()=>{await Promise.all([worker(),worker(),worker()]);console.log('CONVERSION COMPLETE '+JSON.stringify({total:rows.length,reused:rows.filter(r=>r.reused).length,failed:report.filter(r=>r.status==='failed').length}));})().catch(e=>{console.error(e);process.exitCode=1;});
