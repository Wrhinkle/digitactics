const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),client=fs.readFileSync(path.join(root,'js/client.js'),'utf8'),shop=fs.readFileSync(path.join(root,'js/shop.js'),'utf8');
const DT=require('../../tools/headless.cjs').load({strict:true}),colors={};
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper']){DT.Sets.activate(set.id,pace);colors[`${set.id}/${pace}`]=Object.fromEntries(Object.entries(DT.TRAITS).map(([id,t])=>[id,t.color]));}
fs.writeFileSync(path.join(root,'native/content/postgame/trait-colors.json'),JSON.stringify(colors));
function between(text,start,end){const a=text.indexOf(start),b=text.indexOf(end,a);if(a<0||b<0)throw Error('Missing original source '+start);return text.slice(a,b);}
const context=vm.createContext({Date,save:{totals:{},matches:[]}});
vm.runInContext(between(client,'const XP_PER =','function tamerRank()')+between(client,'function bestToday(m)','const setOptions =')+between(shop,'const MISSIONS =','// ---------- save ----------')+between(shop,'const hash =','const today =')+'\n'+between(client,'const placeLabel =','function compHTML('),context);
const levels=[];
for(let games=0;games<1000;games+=7)for(const [top4,wins] of [[0,0],[games,0],[games,games],[Math.floor(games/2),Math.floor(games/4)]]){
 context.save.totals={games,top4,wins}; levels.push({totals:context.save.totals,expected:vm.runInContext('tamerLevel()',context)});
}
const placements=[];for(const duos of [false,true])for(let place=1;place<=(duos?4:8);place++)placements.push({duos,place,expected:vm.runInContext(`placeLabel(${place},${duos})`,context)});
const day=new Date(2026,9,18,12),yesterday=new Date(2026,9,17,12),date='2026-10-18';
const stats=[{pvpWins:2,kills:0,maxStars:2,maxLevel:4,place:5},{pvpWins:1,kills:1,maxStars:1,maxLevel:6,place:3},{pvpWins:9,kills:9,maxStars:3,maxLevel:9,place:1}];
context.save.matches=stats.map((stats,i)=>({at:i===2?yesterday.getTime():day.getTime(),stats}));
const NativeDate=Date;context.Date=class extends NativeDate{constructor(...args){super(...(args.length?args:[day.getTime()]));}};
const missions=vm.runInContext(`MISSIONS.slice().sort((a,b)=>hash('${date}:'+a.id)-hash('${date}:'+b.id)).slice(0,2).map(m=>({id:m.id,name:m.name,bits:m.bits,goal:m.goal,current:bestToday(m)}))`,context);
const matches=context.save.matches,dailyCases=[];
for(let n=1;n<=31;n++){
 const at=new NativeDate(2026,9,n,12),yesterday=new NativeDate(2026,9,n-1,12),date=`2026-10-${String(n).padStart(2,'0')}`;
 context.Date=class extends NativeDate{constructor(...args){super(...(args.length?args:[at.getTime()]));}};
 context.save.matches=stats.map((stats,i)=>({at:i===2?yesterday.getTime():at.getTime(),stats}));
 const expected=vm.runInContext(`MISSIONS.slice().sort((a,b)=>hash('${date}:'+a.id)-hash('${date}:'+b.id)).slice(0,2).map(m=>({id:m.id,name:m.name,bits:m.bits,goal:m.goal,current:bestToday(m)}))`,context);
 dailyCases.push({day:date,matches:context.save.matches,missions:expected});
}
const countExpression=shop.match(/el\.textContent = (Math\.round\(n \* \(1 - Math\.pow\(1 - k, 3\)\)\));/)[1];
const counts=[];for(const n of [0,6,20,70,120,999])for(let i=0;i<=100;i++){context.n=n;context.k=i/100;counts.push({n,k:context.k,value:vm.runInContext(countExpression,context)});}
fs.writeFileSync(path.join(root,'native/tests/fixtures/postgame-progress.json'),JSON.stringify({levels,placements,day:date,matches,missions,dailyCases,counts}));
console.log(`Exported ${levels.length} original account XP samples, ${placements.length} placement labels and daily mission progress.`);
