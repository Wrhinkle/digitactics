// Inventory every creature archive, including game-specific meshes and alternate forms.
// Do not deduplicate by species name: only identical source bytes / known bundled source identifiers.
const fs=require('fs'),path=require('path'),crypto=require('crypto');
const here=__dirname,root=path.resolve(here,'../..'),rips=path.join(here,'rips/x'),raw=require('./rips/index.json');
const credits=JSON.parse(fs.readFileSync(path.join(root,'docs/credits/model-assets.json'))).assets;
const walk=d=>fs.readdirSync(d,{withFileTypes:true}).flatMap(e=>e.isDirectory()?walk(path.join(d,e.name)):[path.join(d,e.name)]);
const sourceId=url=>String(url).match(/(?:asset|model)\/(\d+)/)?.[1]||url;
const glbIds=file=>{const b=fs.readFileSync(path.join(root,file)),j=JSON.parse(b.subarray(20,20+b.readUInt32LE(12)));return new Set((j.nodes||[]).concat(j.materials||[]).flatMap(n=>n.name?.match(/(?:chr|[gmdfu])\d+/ig)||[]).map(s=>s.toLowerCase()));};
const groups=new Map();for(const r of raw){if(r.section&&!/Digimon$/.test(r.section))continue;const id=sourceId(r.page);if(!groups.has(id))groups.set(id,[]);groups.get(id).push(r);}
// Some archive filenames use a different character code from their internal node names.
// A previously imported, byte-identical source is stronger evidence than that naming heuristic.
const imported=fs.existsSync(path.join(here,'catalog-imports.json'))?JSON.parse(fs.readFileSync(path.join(here,'catalog-imports.json'))):[];
const records=[],hashes=new Map(),fileCache=new Map();
for(const [id,rows] of groups){
 const available=rows.filter(r=>fs.existsSync(path.join(rips,r.name.replaceAll(' ','_'))));if(!available.length)continue;
 const base=available.find(r=>fs.existsSync(path.join(root,'models/files/rip_'+r.name.replaceAll(' ','_').toLowerCase()+'.glb')))||available[0];
 const folder=base.name.replaceAll(' ','_'),all=walk(path.join(rips,folder)).sort((a,b)=>a.split(path.sep).length-b.split(path.sep).length);
 const fbx=all.filter(f=>/\.fbx$/i.test(f)&&!/splituv/i.test(f));
 const candidates=fbx.length?fbx:all.filter(f=>/\.dae$/i.test(f)&&!/splituv|fix/i.test(f));
 if(!candidates.length)candidates.push(...all.filter(f=>/\.obj$/i.test(f)));
 const byCode=new Map();for(const f of candidates){const code=path.basename(f).match(/(?:chr|[gmdfu])\d+/i)?.[0]?.toLowerCase()||'main';if(!byCode.has(code))byCode.set(code,[]);byCode.get(code).push(f);}
 const bundled=credits.filter(c=>sourceId(c.url)===id),known=new Set();for(const c of bundled){if(!fileCache.has(c.file))fileCache.set(c.file,glbIds(c.file));for(const code of fileCache.get(c.file))known.add(code);}
 for(const [code,choices] of byCode){const pick=choices.find(f=>/with anim|animations[\\/]/i.test(f))||choices[0];
  const sha256=crypto.createHash('sha256').update(fs.readFileSync(pick)).digest('hex'),duplicate=hashes.get(sha256);if(!duplicate)hashes.set(sha256,folder+':'+code);
  const recorded=imported.filter(x=>x.sha256===sha256&&sourceId(x.source)===id&&bundled.some(c=>c.file==='models/files/'+x.file));
  const existing=(known.has(code)||(code==='main'&&bundled.length>0))?bundled.map(x=>x.file):recorded.map(x=>'models/files/'+x.file);
  records.push({folder,code,game:base.game,source:base.page,section:base.section||'Digimon',input:path.relative(rips,pick).replaceAll('\\','/'),sha256,duplicateOf:duplicate||null,bundled:existing,
    status:duplicate?'identical source':existing.length?'bundled':'needs import'});
 }
}
const counts={};for(const r of records){const c=counts[r.game]??={bundled:0,pending:0,duplicates:0};c[r.status==='bundled'?'bundled':r.status==='needs import'?'pending':'duplicates']++;}
fs.writeFileSync(path.join(here,'catalog-inventory.json'),JSON.stringify({counts,records},null,2)+'\n');console.log(JSON.stringify(counts,null,2));
