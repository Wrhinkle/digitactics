const fs=require('fs'),path=require('path'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
const source=fs.readFileSync(path.join(root,'js/net-guard.js'),'utf8');
const guard=new Function(source.replace(/export /g,'')+';return {parse};')();
const lobby={host:'123',name:'Room',hostName:'Tamer',status:'lobby',slots:{0:{uid:'123',name:'Host'},1:{uid:'456',name:'Guest'}},banned:['789'],planTime:30,setId:'base',round:'1-1',alive:Array(8).fill(true)};
const init={seed:4294967295,at:1000,planTime:30,setId:'base',slots:Array.from({length:8},(_,i)=>({name:'Tamer'+i,human:i<2})),pace:'hyper',format:'duos',teams:[[0,1],[2,3],[4,5],[6,7]]};
const rec={name:'Player',look:{legend:'tokomon',legendForm:'tokomon@patamon',board:'default',emote:['hello',null],sound:['clip'],dance:null,killscreen:'effect',elimsound:null},lobbySet:'base',lobbyPace:'chaos',lobbyFormat:'solo',at:1000};
const entry={k:'a',s:0,a:'buy',g:{slot:0},at:1000,d:'0123abcd'};
const bases=[{t:'snapreq',rid:1},{t:'seat',rid:1,i:7,name:'Guest'},{t:'seatres',rid:1,ok:true},{t:'unseat',i:1},{t:'pres',rec},{t:'presup',rec},{t:'presdel'},{t:'intent',s:0,q:1,round:1,a:'buy',g:{slot:0}},{t:'logreq',from:0},{t:'ping',t0:1},{t:'pong',t0:1,now:2},{t:'snap',rid:1,total:1,start:0,entries:[entry],lobby,presence:{123:rec},init,now:1000},{t:'lobby',lobby},{t:'presence',presence:{123:rec}},{t:'init',init},{t:'log',n:0,e:entry},{t:'logs',start:0,entries:[entry]},{t:'head',n:1},{t:'fx',v:{s:0,kind:'emote',id:'hello'}}].map(m=>({l:'999',...m}));
const actions={claim:{name:'Tamer'},release:{name:'Tamer'},ready:{},done:{},reroll:{},lock:{},buyXp:{},offerReroll:{},wisp:{},buy:{slot:15},sell:{uid:10000000},offer:{i:15},carousel:{i:31},orb:{id:10000000},move:{from:{type:'board',c:6,r:4},to:{type:'bench',i:8}},equip:{idx:511,uid:10000000},swapItems:{a:0,b:511},portal:{item:511}};
for(const [a,g] of Object.entries(actions)){
 bases.push({l:'999',t:'intent',s:7,q:1,round:10000,a,g});
 bases.push({l:'999',t:'log',n:0,e:{k:'a',s:7,a,g,at:1}});
}
for(const k of ['vote','start','end','next'])bases.push({l:'999',t:'log',n:0,e:{k,s:0,at:1}});
const values=[null,{},[],false,true,-1,0,1,7,8,16,32,511,512,32768,1.5,9007199254740991,9007199254740992,'','0','01','123','18446744073709551615','18446744073709551616','<tag>','a\0b','a'.repeat(257),'é','a@b.-_'];
const rows=[];const seen=new Set();
function add(wire){if(seen.has(wire))return;seen.add(wire);rows.push({wire,valid:!!guard.parse(wire)});}
function paths(v,p=[]){let result=[p];if(v&&typeof v==='object')for(const key of Object.keys(v))result.push(...paths(v[key],[...p,key]));return result;}
for(const base of bases){
 add(JSON.stringify(base));
 for(const p of paths(base)){
  for(const value of values){let v=structuredClone(base);if(!p.length)v=value;else{let dst=v;for(const k of p.slice(0,-1))dst=dst[k];dst[p.at(-1)]=value;}add(JSON.stringify(v));}
  if(p.length){const v=structuredClone(base);let dst=v;for(const k of p.slice(0,-1))dst=dst[k];delete dst[p.at(-1)];add(JSON.stringify(v));}
 }
}
for(const key of ['__proto__','prototype','constructor'])add(`{"l":"999","t":"presdel","ignored":{"${key}":1}}`);
for(const depth of [10,11,12,13]){let nested=0;for(let i=0;i<depth;i++)nested={v:nested};add(JSON.stringify({l:'999',t:'presdel',ignored:nested}));}
for(const count of [11995,11996,12000])add(JSON.stringify({l:'999',t:'presdel',ignored:Array(count).fill(0)}));
for(const raw of ['','null','[]','{"l":"999","t":"presdel",}','{"l":"999","t":"presdel","n":1e400}'])add(raw);
fs.writeFileSync(path.join(root,'native/tests/fixtures/network-protocol.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} original wire validation results (${rows.filter(r=>r.valid).length} accepted).`);
