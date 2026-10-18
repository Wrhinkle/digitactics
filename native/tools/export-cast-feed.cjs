// Execute the original DOM callback with a minimal DOM/timer host; no browser or golden refresh.
const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/ui.js'),'utf8');
const body=source.match(/renderer\.onCast = \(c\) => \{([\s\S]*?)\n  \};/)[1];
const colorSource=fs.readFileSync(path.join(root,'js/fx.js'),'utf8').match(/export const elemColor = ([\s\S]*?);/)[1];
const elemColor=vm.runInNewContext(colorSource);
const elements=['fire','ice','thunder','water','plant','wind','holy','dark','metal','beast','unknown'];
const colors=Object.fromEntries(elements.map(e=>[e,elemColor(e)])),scenarios=[];
for(const speed of [1,2,4]){
  const children=[],timers=[];
  const feed={children,appendChild:row=>children.push(row),get firstChild(){return children[0];}};
  const env={renderer:{},ui:{speed},$:()=>feed,P:{html:(line,stage)=>`<portrait>${line}/${stage}</portrait>`},
    esc:s=>s.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;'),
    setTimeout:(fn,time)=>timers.push({fn,time}),document:{createElement:()=>({style:{setProperty(key,value){this[key]=value;}},classList:{add(){}},remove(){const i=children.indexOf(this);if(i>=0)children.splice(i,1);}})}};
  vm.runInNewContext(`renderer.onCast=(c)=>{${body}\n};`,env);
  const casts=Array.from({length:7},(_,i)=>({line:'line'+i,stage:i%3,team:i%2,name:'Digimon '+i,ability:i===6?'Final <Blast> & Heal':'Ability '+i,element:elements[i]}));
  casts.forEach(c=>env.renderer.onCast({unit:{line:{id:c.line},stage:c.stage,name:c.name},name:c.ability,team:c.team,color:colors[c.element]}));
  const rows=children.map(row=>({className:row.className,portrait:row.innerHTML.match(/<portrait>(.*?)<\/portrait>/)[1],name:row.innerHTML.match(/class="cn">(.*?)<\/div>/)[1],ability:row.innerHTML.match(/class="ca">(.*?)<\/div>/)[1],color:row.style['--ec']}));
  scenarios.push({speed,casts,rows,life:timers[0].time/1000,removedAt:timers[1].time/1000});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/cast-feed.json'),JSON.stringify({colors,scenarios},null,2)+'\n');
console.log('Exported original five-row cast feed ordering, labels, colors and 1x/2x/4x expiry.');
