// Execute the original client profile projections; no native-derived expectations.
const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/client.js'),'utf8');
function between(start,end){const a=source.indexOf(start),b=source.indexOf(end,a);if(a<0||b<0)throw Error('Profile source marker changed');return source.slice(a,b);}
const context={};vm.createContext(context);
vm.runInContext(between('const RANKS = [','// ---------- save ----------')+'\nlet save;\n'+between('function tamerRank()','// ---------- state ----------')+between('function stats()','// =====================================================================')+between('const cleanName =','// Steam builds')+between('const ordinal =','function compHTML')+between('function placementChart()','RENDER.profile ='),context);
const cases=[];
for(let i=0;i<140;i++){
 const games=i<4?i:10,average=[2.7,2.8,2.9,3.7,3.8,3.9,4.7,4.8,4.9][i%9],counts={fire:i%7,ice:(i+2)%7,water:(i+4)%7,wind:(i+1)%7};
 const save={profile:{avatar:i%2?'bukamon':null,banner:i%3?'profile-2':'unknown'},totals:{games,wins:Math.floor(games/3),top4:Math.floor(games/2),placeSum:Math.round(games*average),traits:counts,units:{patamon:4,agumon:9,gabumon:9,biyomon:1,tentomon:6,gomamon:8}},matches:Array.from({length:i%31},(_,j)=>({key:'m'+j,at:1760000000000-j*3600000,place:(j+i)%8+1,format:j%2?'duos':'solo',mode:j%3?'solo':'mp',set:'faction-wars',round:'3-2',level:6,traits:[],units:[]}))};
 context.input=save;vm.runInContext('save=input',context);
 // SVG bar titles and circle coordinates are emitted by the actual original functions.
 const result=vm.runInContext('({rank:tamerRank(),stats:stats(),placements:placementChart(),trend:trendChart()})',context);
 cases.push({saved:JSON.stringify(save),rank:result.rank,stats:result.stats,placements:[...result.placements.matchAll(/<title>[^:]+: (\d+) game/g)].map(m=>+m[1]),trend:[...result.trend.matchAll(/<g><title>Game (\d+): (\d+)(?:st|nd|rd|th) \((solo|multiplayer)\)<\/title>/g)].map(m=>({place:+m[2],mode:m[3]==='solo'?'solo':'mp'}))});
}
const names=['  A_B.c-d\' e!  ','\u00e9\u4e2dTamer\n','a'.repeat(50),'  ','100% Tamer','<new>"Name"'];
const nameCases=names.map(value=>{context.name=value;return {value,expected:vm.runInContext('cleanName(name)',context)};});
fs.writeFileSync(path.join(root,'native/tests/fixtures/profile-summary.json'),JSON.stringify({cases,names:nameCases}));
console.log(`Exported ${cases.length} original profile projections and ${names.length} name cases.`);
