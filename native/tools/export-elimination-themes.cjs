// Capture every original Canvas 2D operation; JavaScript is a build dependency only.
const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
const source=fs.readFileSync(path.join(root,'js/killscreens.js'),'utf8');
const mystery=fs.readFileSync(path.join(root,'js/mystery-content.js'),'utf8').replace(/export /g,'');
const context={};vm.createContext(context);
vm.runInContext(mystery+'\nthis.mystery=killThemes;',context);
vm.runInContext(source.slice(source.indexOf('const R ='),source.indexOf('// ---------- catalog ----------'))+'\nthis.themes=THEMES;',context);
for(const item of context.mystery)context.themes[item.id]=item.theme;
function color(s){if(s[0]==='#'){let h=s.slice(1);if(h.length===3)h=[...h].map(c=>c+c).join('');let n=parseInt(h,16);return [n>>16&255,n>>8&255,n&255,1];}let c=s.slice(s.indexOf('(')+1,-1).split(',').map(Number);return [c[0],c[1],c[2],c.length===4?Math.max(0,Math.min(1,c[3])):1];}
function paint(s){return typeof s==='string'?{kind:0,values:color(s),stops:[]}:JSON.parse(JSON.stringify(s));}
function canvas(ops){const c={};for(const op of ['fillRect','strokeRect','beginPath','closePath','moveTo','lineTo','arc','ellipse','fill','stroke','save','restore','translate','rotate'])c[op]=(...values)=>ops.push({operation:op,values});
  for(const prop of ['fillStyle','strokeStyle','globalAlpha','lineWidth','lineCap'])Object.defineProperty(c,prop,{set(value){ops.push(prop==='fillStyle'||prop==='strokeStyle'?{operation:prop,values:[],paint:paint(value)}:{operation:prop,values:[prop==='lineCap'?(value==='round'?1:0):value]});}});
  for(const [name,kind] of [['createLinearGradient',1],['createRadialGradient',2]])c[name]=(...values)=>({kind,values,stops:[],addColorStop(at,css){const [r,g,b,a]=color(css);this.stops.push({at,color:{r,g,b,a}});}});return c;}
const rows=[];
for(const [id,theme] of Object.entries(context.themes))for(const [w,h] of [[340,190],[860,430]])for(const t of [0,.25,.55,.7,1.6,2.7,3.5,6.9]){
  const operations=[];theme.draw(canvas(operations),w,h,t);rows.push({id,w,h,t,rim:color(theme.rim),text:color(theme.text),silhouette:!!theme.silhouette,operations});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/elimination-themes.json.gz'),zlib.gzipSync(JSON.stringify(rows),{level:9}));
console.log(`Exported ${rows.length} original theme traces (${Object.keys(context.themes).length} themes).`);
