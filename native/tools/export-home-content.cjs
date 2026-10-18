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
const mapping=vm.runInNewContext('('+source.match(/const SET_ART = (\{[\s\S]*?\n\});/)[1]+')');
const allOptions=[...DT.Sets.pickable(),{id:'chaos',name:'Chaos'}];
const infoSource=source.slice(source.indexOf('function setInfo('),source.indexOf('function renderHomeParts('));
const slidesSource=source.slice(source.indexOf('function heroSlides('),source.indexOf('let heroTimer'));
function slides(count,featured){return vm.runInNewContext(infoSource+slidesSource+';heroSlides();',{
 DT:{Sets:DT.Sets,Shop:{featured:()=>featured?[{}]:[]}},setOptions:()=>allOptions.slice(0,count),setName:id=>id,
 setArt:id=>mapping[id]||{hero:'hero-digital-world',tag:'Set',renders:[]}}).map(({renders,...slide})=>slide);}
const full=slides(allOptions.length,true),fallback=slides(0,true);
content.hero={sets:full.filter(x=>x.act.startsWith('set:')),store:full.find(x=>x.id==='store'),multiplayer:full.find(x=>x.id==='mp'),farm:fallback.find(x=>x.id==='farm'),evolution:fallback.find(x=>x.id==='evo')};
const heroCases=[];for(let count=0;count<=allOptions.length;count++)for(const featured of [false,true])heroCases.push({count,featured,slides:slides(count,featured)});
// Execute the original timeout pair with a small DOM surface. Each tick is one
// browser event-loop opportunity, including blocked/deferred and late callbacks.
const timerSource=source.slice(source.indexOf('let heroTimer'),source.indexOf("$('homeHero').addEventListener"));
const timers=[];
for(let scenario=0;scenario<128;scenario++){
 let now=0,callback=null,due=null,rendered=0;
 const st={page:'home',module:null,result:null,htab:'overview',heroIdx:0},context={client:true,hover:false};
 const node={innerHTML:'',classList:{add(){},remove(){},toggle(){}},offsetWidth:1};
 const el={...node,querySelector:()=>node,querySelectorAll:()=>[],matches:()=>context.hover};
 const sandbox={st,$:()=>el,heroSlides:()=>full.map(slide=>({...slide,feat:false})),ART:x=>x,esc:x=>x,HERO_RENDERS:false,disposePreviews(){rendered++;},clientOn:()=>context.client,
  setTimeout(fn,ms){callback=fn;due=now+ms;return 1;},clearTimeout(){callback=null;due=null;},DT:{Shop:{featured:()=>[{kind:'legend',item:{id:'tokomon',name:'Tokomon',rarity:'common',price:0}}],priceOf:()=>0}},C:{owns:()=>true},RARITIES:['common'],KIND_ONE:{legend:'Little Legend'},bit:'',fillPreviews(){},Date};
 vm.createContext(sandbox);vm.runInContext(timerSource+';renderHero();',sandbox);
 const steps=[];
 for(let i=0;i<24;i++){
  now+=i%7===0?12000:i%3===0?7999:4000;
  const flags=(scenario*37+i*19)%128;
  st.page=flags&1?'home':'other';context.client=!!(flags&2);st.module=flags&4?'collection':null;st.result=flags&8?{}:null;st.htab=flags&16?'overview':'patch';context.hover=!!(flags&32);
  let selected=null;if((flags&64)&&i%4===0){selected=(scenario+i)%full.length;st.heroIdx=selected;vm.runInContext('renderHero();',sandbox);}
  else if(callback&&now>=due){const fn=callback;callback=null;due=null;fn();}
  steps.push({now:now/1000,home:st.page==='home',client:context.client,module:!!st.module,result:!!st.result,overview:st.htab==='overview',hovered:context.hover,selected,index:st.heroIdx,revision:rendered,due:due===null?null:due/1000});
 }
 timers.push(steps);
}
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
fs.writeFileSync(path.join(root,'native/tests/fixtures/home-content.json'),JSON.stringify({content,rotation:fixture,heroCases,timers}));
console.log(`Exported ${patch.length} Home highlights, ${fixture.length} countdowns, ${heroCases.length} carousel compositions and ${timers.length*24} original timeout ticks.`);
