// Execute the original registry, SetPicker preferences and seeded host resolution.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),DT=require('../../tools/headless.cjs').load({strict:true});
const pickerSource=fs.readFileSync(path.join(root,'js/setpicker.js'),'utf8');
const setsSource=fs.readFileSync(path.join(root,'js/sets.js'),'utf8');
const setDefinitions=DT.Sets.ids().map(id=>{const d=DT.Sets.def(id);return {id,name:DT.Sets.label(id),description:d.desc||'',mode:d.mode||'tft',nativePace:DT.Sets.nativePace(id),hidden:!!d.hidden,chaosEligible:d.chaos!==false};});
const metadata={schemaVersion:1,defaultId:DT.Sets.DEFAULT,sets:setDefinitions,pickableIds:DT.Sets.pickable().map(s=>s.id)};
fs.writeFileSync(path.join(root,'native/content/set-selection.json'),JSON.stringify(metadata,null,2)+'\n');
const values=new Map(),context={window:{DT},localStorage:{getItem:key=>values.get(key)??null,setItem:(key,value)=>values.set(key,value)}};
vm.createContext(context);
vm.runInContext(pickerSource.slice(0,pickerSource.indexOf('// ---------- Play page'))+'\nfunction render(){};globalThis.picker={selected,pace,format,select,setPace,setFormat,options,paceOptions};',context);
const preferences=[];
for(const set of [null,'','missing','chaos',...metadata.pickableIds])for(const pace of [null,'','bad','chaos','hyper','normal'])for(const format of [null,'solo','duos','bad']){
 values.clear();values.set('dt.set.v1',set);values.set('dt.pace.v1',pace);values.set('dt.format.v1',format);
 preferences.push({set,pace,format,expectedSet:context.picker.selected(),expectedPace:context.picker.pace(),expectedFormat:context.picker.format()});
}
const seeds=[0,1,2,9,10,99,100,999,1000,999999999,1000000000,2147483647,2147483648,4294967294,4294967295];
const resolutions=[];
function resolve(set,pace,seed){const setId=DT.Sets.resolve(set,seed);return {seed,set,pace,setId,resolvedPace:DT.Sets.resolvePace(pace,seed)||DT.Sets.nativePace(setId),chaos:set==='chaos',chaosPace:pace==='chaos'};}
for(const seed of seeds)for(const set of [null,'','missing','chaos',...metadata.pickableIds])for(const pace of [null,'','bad','hyper','normal','chaos'])resolutions.push(resolve(set,pace,seed));
let random=0xb14e7c19;
for(let i=0;i<4096;i++){random=(Math.imul(random,1664525)+1013904223)>>>0;resolutions.push(resolve('chaos','chaos',random));}
const matches=[],slots=Array.from({length:8},(_,i)=>({name:'Tamer '+i,human:i<2}));
for(const seed of [0,1,42,4294967295])for(const set of ['chaos','underlords_ko'])for(const pace of ['hyper','normal','chaos'])for(const format of ['solo','duos']){
 const local={seed,setId:set,pace,format,slots,planTime:30,at:12345};
 const game=DT.Lock.create(local),resolution=resolve(set,pace,seed);
 const hostInit={seed,slots,format,teams:DT.Duos.TEAMS,planTime:30,at:12345,setId:DT.Sets.resolve(set,seed),chaos:set==='chaos',pace:DT.Sets.resolvePace(pace,seed)||'hyper',chaosPace:pace==='chaos'};
 matches.push({local,resolution,hostInit,localCheck:DT.Lock.check(game),networkCheck:DT.Lock.check(DT.Lock.create(hostInit))});
}
const poolCases=[];
const resolver=setsSource.slice(setsSource.indexOf('  function chaosPool()'),setsSource.indexOf('\n  const listeners'));
for(const definitions of [
 [{id:'default',chaos:false},{id:'hidden',hidden:true}],
 [{id:'z'},{id:'default',chaos:false},{id:'a'},{id:'hidden',hidden:true}],
 [{id:'default'}]
]){
 const c={defs:new Map(definitions.map(d=>[d.id,d])),DEFAULT:'default'};vm.createContext(c);vm.runInContext(resolver+';globalThis.pick=resolve;globalThis.pool=chaosPool();',c);
 poolCases.push({definitions,pool:c.pool,picks:seeds.map(seed=>({seed,id:c.pick('chaos',seed)}))});
}
const result={sourceSha256:crypto.createHash('sha256').update(setsSource+pickerSource).digest('hex'),defaultId:DT.Sets.DEFAULT,pool:DT.Sets.chaosPool(),options:context.picker.options().map(s=>s.id),localPaces:context.picker.paceOptions(false).map(p=>p.id),lobbyPaces:context.picker.paceOptions(true).map(p=>p.id),preferences,resolutions,matches,poolCases};
fs.writeFileSync(path.join(root,'native/tests/fixtures/set-selection.json'),JSON.stringify(result)+'\n');
console.log(`Set selection: ${resolutions.length} source resolutions, ${preferences.length} preferences, ${matches.length} Solo/Duos local/network starts and ${poolCases.length} eligibility cases.`);
