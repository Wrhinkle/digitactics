const fs=require('fs'),path=require('path'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/net-steam.js'),'utf8');
const body=source.slice(source.indexOf('  function drain()'),source.indexOf('  function requestSnapshot()'));
const rows=[];let seed=4321;const random=()=>{seed=seed*16807%2147483647;return seed/2147483647;};
for(let run=0;run<12;run++){
 const R={log:[],pending:new Map(),syncing:false,lastFill:0};let now=0,resets=0,requests=[];
 const api=new Function('R','Date','send','hostId','deliverAll','resetLog',body+'return {onLogEntry,drain};')(R,{now:()=>now},(_,m)=>requests.push(m.from),()=> '123',()=>{},()=>resets++);
 const steps=[];
 for(let i=0;i<180;i++){
  now+=Math.floor(random()*1200);resets=0;requests=[];
  const n=i%17===0?R.log.length+513:Math.max(0,R.log.length+Math.floor(random()*9)-4);
  const e={k:'vote',s:Math.floor(random()*8),at:now};
  if(i%11===0&&R.log[n])Object.assign(e,R.log[n]);
  api.onLogEntry(n,e);
  steps.push({n,e,now,log:structuredClone(R.log),pending:R.pending.size,reset:resets>0,request:requests[0]??null});
 }
 rows.push(steps);
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/replicated-log.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
console.log('Exported 2160 original ordered-log transitions.');
