const fs=require('node:fs'),path=require('node:path'),cp=require('node:child_process'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'..'),git=(...a)=>cp.execFileSync('git',a,{cwd:root,maxBuffer:128*1024*1024}),hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const excluded=new Set(['actions.json.gz','motions.json.gz','companion-playback.json.gz','dance-bindings.json.gz']);
const paths=new Set(git('diff','--name-only','--','native/tests/fixtures').toString().trim().split('\n').concat(git('ls-files','--others','--exclude-standard','native/tests/fixtures/combat').toString().trim().split('\n')).filter(Boolean));
const scripts=fs.readdirSync(path.join(root,'native/tools')).filter(f=>f.startsWith('export-')&&f.endsWith('.cjs')).map(f=>[f,fs.readFileSync(path.join(root,'native/tools',f),'utf8')]);
const fixtures=[];
for(const file of paths){if(excluded.has(path.basename(file)))continue;let before=null;try{before=hash(git('show','HEAD:'+file));}catch{}const filename=path.join(root,file),after=fs.existsSync(filename)?hash(fs.readFileSync(filename)):null;fixtures.push({file,exporters:file.includes('/combat/')?['native/tools/export-combat-fixtures.cjs']:scripts.filter(([,s])=>s.includes(path.basename(file))).map(([f])=>'native/tools/'+f),before,after});}
const sourceFiles=['js/items.js','js/game.js','js/combat.js','js/duos.js','js/sets/arcane-mech.js','js/sets/reckoning-mech.js'];
fs.writeFileSync(path.join(root,'native/artifacts/item-capacity-fixture-provenance.json'),JSON.stringify({reason:'Glove equipment capacity now reserves all three slots; regenerate original JavaScript behavioral reference traces.',baseline:git('rev-parse','HEAD').toString().trim(),sourceFiles:Object.fromEntries(sourceFiles.map(f=>[f,hash(fs.readFileSync(path.join(root,f)))])),fixtures},null,2)+'\n');
console.log('Recorded '+fixtures.length+' changed fixture paths with exporter and before/after SHA256.');
