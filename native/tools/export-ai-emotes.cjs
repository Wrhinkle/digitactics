const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/emotes.js'),'utf8');
const items=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'))).emote;
const cases=[];
for(let n=0;n<600;n++){
 let inputs=(n+5)>>>0;const input=()=>{inputs=(Math.imul(inputs,1664525)+1013904223)>>>0;return inputs/4294967296;};
 let state=(n+1)>>>0,calls=0;const math=Object.create(Math);math.random=()=>{calls++;state=(Math.imul(state,1664525)+1013904223)>>>0;return state/4294967296;};
 let current,emitted=null,delay=0;const looks={};
 const ctx={Math:math,DT:{Social:{lookOf:p=>looks[p.id],emit:(_,seat,id)=>emitted={seat,id,delay}},Cosmetics:{get:(_,id)=>items.find(i=>i.id===id)}},game:()=>current,setTimeout:(fn,ms)=>{delay=ms;fn();}};vm.createContext(ctx);
 vm.runInContext('let aliveIds=new Set([0,1,2,3,4,5,6,7]);'+source.slice(source.indexOf('function pickFor('),source.indexOf('function watchGame(')),ctx);
 const rounds=[];
 for(let round=0;round<5;round++){
  const players=Array.from({length:8},(_,id)=>({id,name:'Tamer '+id,isHuman:input()<.3,alive:input()>.2}));
  for(const p of players)looks[p.id]={emote:Array.from({length:6},()=>input()<.15?null:items[Math.floor(input()*items.length)].id)};
  const results=Array.from({length:4},(_,i)=>({a:i*2,b:i*2+1,pve:input()<.2,ghost:input()<.2,winner:Math.floor(input()*3)-1}));
  const localSeat=n%9===8?-1:n%8;current={players,human:players[localSeat],results:results.map(r=>({match:{a:players[r.a],b:players[r.b],pve:r.pve,ghost:r.ghost},aWon:r.winner===0,bWon:r.winner===1}))};
  emitted=null;let before=calls;ctx.onResults(current);
  rounds.push({players,results,localSeat,looks:JSON.parse(JSON.stringify(looks)),expected:emitted,calls:calls-before});
 }
 cases.push({seed:n+1,rounds});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/ai-emotes.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log('Exported 3,000 original AI reaction rounds.');
