const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/shop.js'),'utf8');
const initialShop={awarded:[],awards:{},lastDay:'',missions:{day:'',done:[]},future:'keep'};
const initialWallet={bits:491,owned:{legend:['future-id']},equipped:{},history:[],future:'keep'};
const storage={'dt.cosmetics.v1':JSON.stringify(initialWallet)};let day='2026-10-17',time=1700000000000;
const ctx={DT:{},console,Math,Date:{now:()=>time},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v},initialShop,day:()=>day};ctx.window=ctx;vm.createContext(ctx);
vm.runInContext(fs.readFileSync(path.join(root,'js/cosmetics.js'),'utf8'),ctx);
function section(a,b){return source.slice(source.indexOf(a),source.indexOf(b,source.indexOf(a)));}
vm.runInContext(`const DT=window.DT,W=DT.Wallet;let save=structuredClone(initialShop);const persist=()=>{};const today=()=>day();const UI=()=>({});function track(g){return g.stats;}`.replace('structuredClone(initialShop)','JSON.parse(JSON.stringify(initialShop))')+
 section('const PLACE_BITS','// ---------- save ----------')+source.match(/^const hash = .*$/m)[0]+section('function todaysMissions()', '// ---------- tracking')+source.match(/^const statsOf = .*$/m)[0]+section('function gameKey(', 'function showAward(')+'\nthis.calculate=award;this.shop=()=>JSON.parse(JSON.stringify(save));',ctx);
const cases=[];
for(let i=0;i<225;i++){
 day=`2026-10-${String(1+Math.floor(i/10)%28).padStart(2,'0')}`;time++;
 const duo=i%2===0,place=1+i%(duo?4:8),seed=i===12?11:i===25?1:i===224?0:i;
 const stats={pvpWins:i%9,kills:i%4,maxStars:1+i%3,maxLevel:3+i%8,place};
 const g={seed,human:{id:0,placement:place},duo,stats};const award=ctx.calculate(g,null);
 cases.push({key:`solo:${seed}:0`,duo,day,time,stats,award:award||null,shop:ctx.shop(),wallet:JSON.parse(storage['dt.cosmetics.v1'])});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/match-rewards.json.gz'),zlib.gzipSync(JSON.stringify({initialShop,initialWallet,cases})));console.log('Exported 225 original Bits/mission award updates.');
