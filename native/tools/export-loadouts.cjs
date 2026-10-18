const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const LINE=require('../../tools/headless.cjs').load({strict:true}).LINE;
function load(wallet,legends){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet),'dt.legendevo.v1':JSON.stringify(legends)};
 const ctx={DT:{LINE},console,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(fs.readFileSync(path.join(root,'js/cosmetics.js'),'utf8'),ctx);
 for(const [kind,items] of Object.entries(catalog))ctx.DT.Cosmetics.register(kind,items);
 vm.runInContext('(function(){'+fs.readFileSync(path.join(root,'js/legendevo.js'),'utf8')+'})();',ctx);
 return ctx.DT;
}
const dt=load({},{}),ai=[],local=[],keys=[];
for(let seat=0;seat<8;seat++)for(let n=0;n<100;n++){
 const name=['Tamer','René','テイマー','🐉'][n%4]+n;
 ai.push({seat,name,expected:dt.Social.lookOf({id:seat,name,isHuman:false})});
}
for(const item of catalog.legend){
 for(const form of [...dt.LegendEvo.reach(item.id),'invalid-form']){
  const wallet={owned:{legend:[item.id]},equipped:{legend:item.id}},legends={v:1,legends:{[item.id]:{form,xp:3000}},games:[]};
  local.push({wallet,legends,expected:load(wallet,legends).Cosmetics.loadout()});
  keys.push({legend:item.id,legendForm:form,expected:dt.LegendEvo.look({legend:item.id,legendForm:form}).legend});
 }
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/loadouts.json.gz'),zlib.gzipSync(JSON.stringify({ai,local,keys})));
console.log(`Exported ${ai.length} AI loadouts, ${local.length} equipped loadouts and ${keys.length} appearance keys.`);
