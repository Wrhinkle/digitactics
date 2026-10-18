// CPU-only checks for identity, determinism, native vocabulary, and drawing budgets.
const fs=require('fs'),path=require('path'),vm=require('vm'),assert=require('assert/strict'),crypto=require('crypto');
const root=path.resolve(__dirname,'..');
const source=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8');
const context={};vm.createContext(context);vm.runInContext(source.replace(/export /g,'')+'\nthis.themes=killThemes;',context);
let checks=0,maxOperations=0,maxPaths=0;
function check(condition,label){checks++;assert.ok(condition,label);}
function trace(theme,w,h,t){
 const operations=[],saved=[];let alpha=1,paths=0;
 const c={};
 function emit(op,values){for(const value of values)check(Number.isFinite(value),`${theme.id} ${op}: finite geometry`);operations.push([op,...values]);}
 for(const op of ['fillRect','beginPath','closePath','moveTo','lineTo','ellipse','fill','stroke'])c[op]=(...values)=>{
  if(op==='fill'||op==='stroke'||op==='fillRect')paths++;
  if(op==='ellipse')check(values[2]>0&&values[3]>0,theme.id+': positive ellipse radii');
  if(op==='fillRect')check(values[2]>=0&&values[3]>=0,theme.id+': nonnegative rectangle');
  emit(op,values);
 };
 c.save=()=>{saved.push(alpha);emit('save',[]);};
 c.restore=()=>{check(saved.length>0,theme.id+': balanced restore');alpha=saved.pop();emit('restore',[]);};
 for(const prop of ['fillStyle','strokeStyle'])Object.defineProperty(c,prop,{set(value){
  check(typeof value==='string'||value?.gradient===true,theme.id+': supported paint');operations.push([prop,typeof value==='string'?value:JSON.parse(JSON.stringify(value))]);
 }});
 Object.defineProperty(c,'globalAlpha',{set(value){check(Number.isFinite(value)&&value>=0&&value<=1,theme.id+': valid opacity');alpha=value;emit('globalAlpha',[value]);}});
 Object.defineProperty(c,'lineWidth',{set(value){check(Number.isFinite(value)&&value>0,theme.id+': positive line width');emit('lineWidth',[value]);}});
 c.createLinearGradient=(...values)=>{values.forEach(value=>check(Number.isFinite(value),theme.id+': finite gradient'));return {gradient:true,values,stops:[],addColorStop(at,color){this.stops.push({at,color});}};};
 theme.theme.draw(c,w,h,t);
 check(saved.length===0&&alpha===1,theme.id+': caller state restored');
 check(operations.length<=600,`${theme.id}: <=600 canvas operations (${operations.length})`);
 check(paths>=4&&paths<=64,`${theme.id}: bounded drawing calls (${paths})`);
 const fills=operations.filter(op=>op[0]==='fillRect'&&op[1]===0&&op[2]===0&&op[3]===w&&op[4]===h);
 check(fills.length===2,theme.id+': only background and caption shade cover the full canvas');
 check(operations[0][0]==='save'&&operations.at(-1)[0]==='restore',theme.id+': isolated drawing state');
 maxOperations=Math.max(maxOperations,operations.length);maxPaths=Math.max(maxPaths,paths);
 return operations;
}
check(context.themes.length===40,'exactly forty secret finales');
const ids=new Set(),shapes=new Set(),descriptions=new Set();
for(const theme of context.themes){
 check(!ids.has(theme.id),'unique id '+theme.id);ids.add(theme.id);
 check(theme.secret===true&&theme.egg===false&&theme.price===0,'exclusive metadata '+theme.id);
 check(theme.theme.text==='#ffffff','white victory text '+theme.id);
 check(!descriptions.has(theme.desc)&&theme.desc.length>30,'distinct useful description '+theme.id);descriptions.add(theme.desc);
 const signature=trace(theme,340,190,.7);
 const geometry=signature.filter(op=>!['fillStyle','strokeStyle','globalAlpha'].includes(op[0]));
 const hash=crypto.createHash('sha256').update(JSON.stringify(geometry)).digest('hex');
 check(!shapes.has(hash),'geometry distinct without palette or alpha: '+theme.id);shapes.add(hash);
 check(JSON.stringify(signature)===JSON.stringify(trace(theme,340,190,.7)),'deterministic '+theme.id);
 check(JSON.stringify(signature)!==JSON.stringify(trace(theme,340,190,2.3)),'animated '+theme.id);
 for(const [w,h] of [[240,135],[340,190],[860,430],[1100,620]])for(const t of [0,.25,.55,1.6,2.7,3.5,6.9,120,3600])trace(theme,w,h,t);
}
for(const pool of ['daily','vault'])for(let i=1;i<=20;i++)check(ids.has(`mystery_${pool}_killscreen_${i}`),'preserved reward ID');
console.log(`Secret finales: ${checks.toLocaleString('en-US')} checks; 40 distinct animated compositions; maximum ${maxOperations} operations / ${maxPaths} drawing calls per frame.`);
