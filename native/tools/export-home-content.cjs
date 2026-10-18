// Freeze the actual client patch copy and JavaScript local-midnight behavior.
const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/client.js'),'utf8');
const DT=require('../../tools/headless.cjs').load({strict:true});
const patchSource=source.match(/const PATCH = (\[[\s\S]*?\n\]);/);
if(!patchSource)throw Error('Original Home PATCH content missing.');
const patch=vm.runInNewContext('('+patchSource[1]+')',{setOptions:()=>[...DT.Sets.pickable(),{id:'chaos',name:'Chaos'}]})
 .map(([title,description])=>({title,description:typeof description==='function'?description():description}));
const eventFunction=source.slice(source.indexOf('function renderEvents()'),source.indexOf("overlay.addEventListener('click'",source.indexOf('function renderEvents()')));
const element={innerHTML:''};
vm.runInNewContext(eventFunction+';renderEvents();',{$:()=>element,DT:{Shop:{daily:()=>({claimed:false,bits:'{bits}'})}},ART:x=>x,hm:()=>'{countdown}',msToMidnight:()=>0,missionsHTML:()=>''});
const html=element.innerHTML;
const headings=[...html.matchAll(/<h1>([^<]+)<\/h1>|<div class="tile-h"><span>([^<]+)<\/span>/g)].map(m=>m[1]||m[2]);
const paragraphs=[...html.matchAll(/<p(?: class="tile-p")?>([^<]+)<\/p>/g)].map(m=>m[1]);
if(headings.length!==4||paragraphs.length!==3)throw Error('Home Events source structure changed.');
const content={patch,events:{title:headings[0],countdown:paragraphs[0],missions:headings[1],store:headings[2],storeDescription:paragraphs[1],farm:headings[3],farmDescription:paragraphs[2]}};
fs.writeFileSync(path.join(root,'native/content/home/content.json'),JSON.stringify(content));
const fixture=[];
for(const zone of ['UTC','America/Chicago','Pacific/Auckland','Asia/Kathmandu']){
 process.env.TZ=zone;
 for(const timestamp of ['2026-01-01T00:00:00Z','2026-03-08T05:30:00Z','2026-03-08T07:30:00Z','2026-03-08T08:30:00Z','2026-11-01T05:30:00Z','2026-11-01T06:30:00Z','2026-11-01T07:30:00Z','2026-12-31T23:59:59Z'])
 for(let i=0;i<24;i++){
  const now=new Date(new Date(timestamp).getTime()+i*3600000+17281),midnight=new Date(now);midnight.setHours(24,0,0,0);const ms=midnight-now;
  fixture.push({zone,now:now.toISOString(),milliseconds:ms,text:`${Math.floor(ms/3600000)}h ${Math.floor((ms%3600000)/60000)}m`});
 }
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/home-content.json'),JSON.stringify({content,rotation:fixture}));
console.log(`Exported ${patch.length} original Home highlights, Events copy and ${fixture.length} JavaScript rotation countdowns.`);
