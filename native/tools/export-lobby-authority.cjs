const fs=require('fs'),path=require('path'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/net-steam.js'),'utf8');
const seatBody=source.slice(source.indexOf('  function applySeat('),source.indexOf('  function freeSeatLocal('));
const successorBody=source.slice(source.indexOf('  function successor('),source.indexOf('  function becomeHost('));
const cleanLine=source.split('\n').find(s=>s.startsWith('const cleanRemote ='));
let seed=493;const random=()=>{seed=seed*16807%2147483647;return seed/2147483647;};const rows=[];
for(let run=0;run<30;run++){
 const R={members:['123','234','345','456','567','678','789','890'],lobby:{host:'123',hostName:'Host',name:'Room',status:run%2?'playing':'lobby',slots:{0:{uid:'123',name:'Host'}},alive:Array.from({length:8},()=>random()>.3),banned:run%3?[]:['890']}};
 const initial=structuredClone(R.lobby);
 const api=new Function('R','banned','commitLobby',cleanLine+seatBody+successorBody+'return {applySeat,successor};')(R,id=>R.lobby.banned.includes(id),()=>{});
 const steps=[];
 for(let i=0;i<90;i++){
  const uid=R.members[Math.floor(random()*8)],seat=Math.floor(random()*10)-1,name=['Guest','<A>&"\\\u0000','  very long tamer name here  ',''][i%4];
  const result=api.applySeat(uid,seat,name),members=R.members.filter(()=>random()>.3);const all=R.members;R.members=members;
  const successor=api.successor('123');R.members=all;
  steps.push({uid,seat,name,result,lobby:structuredClone(R.lobby),members,successor});
 }
 rows.push({initial,steps});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/lobby-authority.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
console.log('Exported 2700 original seat and successor decisions.');
