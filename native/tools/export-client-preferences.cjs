// Execute the unchanged client preference merge, menu sound gate and pedestal expression.
const fs=require('node:fs'),path=require('node:path'),vm=require('node:vm'),crypto=require('node:crypto');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/client.js'),'utf8');
const merge=source.slice(source.indexOf('let save ='),source.indexOf('const persist ='));
const gate=source.slice(source.indexOf('let lastSfx ='),source.indexOf('// ---------- Tamer level'));
const sway=source.match(/const sway = ([^;]+);/)[1];
const preferences=[];
for(const value of [undefined,null,false,true,0,1,'','false',[],{}])for(const key of ['motion','uiSfx','social']){
  const input={prefs:{[key]:value},profile:{banner:'profile-3'},future:{keep:7}};
  const json=JSON.stringify(input),context={KEY:'dt.client.v1',localStorage:{getItem:()=>json}};
  vm.createContext(context);vm.runInContext(merge+'; result={motion:!!save.prefs.motion,uiSfx:!!save.prefs.uiSfx,social:!!save.prefs.social}',context);
  preferences.push({json,expected:context.result});
}
const events=[],output=[];let now=0;
const context={save:{prefs:{uiSfx:true,motion:true}},performance:{now:()=>now},DT:{Sfx:{play:n=>output.push(n)}}};
vm.createContext(context);vm.runInContext(gate,context);
let state=0x9a73f05d;
for(let i=0;i<5000;i++){
  state=(Math.imul(state,1664525)+1013904223)>>>0;
  now += [0,1,69,70,71,300][state%6];
  const enabled=(state&8)!==0,quiet=(state&16)!==0,name=['tick','pick','place','levelup','equip','fight'][state%6];
  context.save.prefs.uiSfx=enabled;const before=output.length;context.name=name;context.quiet=quiet;vm.runInContext('sfx(name,quiet)',context);
  events.push({milliseconds:now,enabled,quiet,name,played:output.length>before});
}
const motion=[];
for(const enabled of [false,true])for(const seconds of [0,.001,.1,.3,1,2,7,60,600]){
  context.save.prefs.motion=enabled;context.t=seconds;
  motion.push({enabled,seconds,yaw:vm.runInContext('(function(){return '+sway+'}).call({t})',context)});
}
const result={sourceSha256:crypto.createHash('sha256').update(source).digest('hex'),preferences,events,motion};
const destination=path.join(root,'native/tests/fixtures/client-preferences.json');fs.writeFileSync(destination,JSON.stringify(result)+'\n');
console.log(`Client preferences: ${preferences.length} imported cases, ${events.length} source sound events, ${motion.length} pedestal samples.`);
