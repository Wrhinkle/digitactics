// Authored dance quality constraints: continuous phrases, distinct stories, and
// invariant unlock policy. Rig/mesh retargeting is checked by native parity tests.
const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert/strict');
const root=path.resolve(__dirname,'..'),source=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8'),ctx={};
vm.createContext(ctx);vm.runInContext(source.slice(0,source.indexOf('export const killThemes')).replace(/export /g,'')+';this.items=dances;this.library=danceLibrary;',ctx);
let checks=0;function check(ok,message){checks++;assert.ok(ok,message);}
const values=p=>Object.values(p).flatMap(v=>Array.isArray(v)?v:typeof v==='number'?[v]:Object.values(v));
const poses=ctx.items.map(d=>Array.from({length:64},(_,i)=>d.pose(i/8)));
check(ctx.items.length===40,'All forty unlocks remain present.');
const signatures=new Set();
ctx.items.forEach((dance,i)=>{
  const pool=i<20?'daily':'vault',index=i%20+1;
  check(dance.id===`mystery_${pool}_dance_${index}`,'Stable cosmetic IDs.');
  check(dance.secret&&dance.lootPool===pool&&!dance.egg&&dance.price===0,'Preserve loot exclusivity.');
  check(dance.bpm===84+(index-1)*3+(pool==='vault'?7:0),'Keep existing tempos.');
  const phrase=ctx.library.phrases[i];check(phrase.keys.length===8&&new Set(phrase.keys).size>=4,'A phrase has at least four readable gesture targets.');
  signatures.add(phrase.keys.join('|'));
  for(let b=0;b<=32;b++){
    const a=values(dance.pose(Math.max(0,b-1e-7))),z=values(dance.pose(b+1e-7));
    check(a.every((v,n)=>Math.abs(v-z[n])<.001),`${dance.id} has no discontinuity at beat ${b}.`);
  }
  for(let j=0;j<64;j++){
    const p=poses[i][j],q=values(dance.pose(j/8+8)),a=values(p);
    check(a.every(Number.isFinite),'Every pose channel is finite.');
    check(a.every((v,n)=>Math.abs(v-q[n])<1e-8),'Eight-beat loop closes exactly.');
    check(Math.hypot(p.rt.x,p.rt.y,p.rt.z)<.15&&Math.abs(p.rt.ry)<=.501,'Dance stays over its assigned spot.');
    check(p.preserveScale===1&&p.familyAdapt===1,'Secret phrases retain rigid size and anatomical adaptation.');
  }
});
check(signatures.size===40,'Every unlock has an independently authored gesture sequence.');
// Compare animation itself, independent of names/BPM/route metadata.
let nearest={distance:Infinity};
const gestureValues=p=>['aL','aR','fL','fR'].flatMap(k=>p[k]);
for(let i=0;i<40;i++)for(let j=i+1;j<40;j++){
  let square=0,n=0;for(let t=0;t<64;t++){const a=gestureValues(poses[i][t]),b=gestureValues(poses[j][t]);for(let k=0;k<a.length;k++){square+=(a[k]-b[k])**2;n++;}}
  const distance=Math.sqrt(square/n);check(distance>.08,`${ctx.items[i].id} and ${ctx.items[j].id} must differ visibly beyond tempo/route.`);
  if(distance<nearest.distance)nearest={a:ctx.items[i].id,b:ctx.items[j].id,distance};
}
console.log(`Secret dances: ${checks.toLocaleString()} continuity, bounds, identity, policy and differentiation checks passed.`);
console.log('Nearest arm-gesture RMS pair:',JSON.stringify(nearest));
