const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=name=>fs.readFileSync(path.join(root,'js',name),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const shop=read('shop.js'),client=read('client.js');
const setup='const DT=window.DT,C=DT.Cosmetics,W=DT.Wallet;'+
 shop.slice(shop.indexOf('const KINDS'),shop.indexOf('// Daily missions'))+
 shop.slice(shop.indexOf('const EGGS'),shop.indexOf('// ---------- DOM'))+
 'DT.Shop={eggs:()=>EGGS.map(e=>({...e,odds:eggOdds(eggPool(e))}))};'+
 client.slice(client.indexOf('function lootItems()'),client.indexOf('RENDER.loot ='))+
 shop.slice(shop.indexOf('function eggOnlyHTML(pool)'),shop.indexOf('  return `<div class="dts-eolist">'))+
 'return only.map(x=>({kind:x.kind,id:x.item.id,owned:C.owns(x.kind,x.item.id),percent:each(x.item.rarity||"common"),chance:each(x.item.rarity||"common").toFixed(each(x.item.rarity||"common")<1?2:1)+"%"}));}'+
 'this.inspect=()=>{const {eggs,legends}=lootItems();return {legends:legends.map(x=>({id:x.it.id,form:x.evo.form,level:x.evo.level,data:x.evo.data,ready:x.evo.canDigivolve})),data:legends.reduce((n,x)=>n+x.evo.data,0),ready:legends.filter(x=>x.evo.canDigivolve).length,eggs:eggs.map(e=>{const pool=eggPool(e);return {id:e.id,total:pool.length,owned:pool.filter(x=>C.owns(x.kind,x.item.id)).length,exclusives:eggOnlyHTML(pool)||[]};})};};';
const cases=[];
for(let mode=0;mode<7;mode++){
 const wallet={bits:1000,owned:Object.fromEntries(Object.entries(catalog).map(([kind,items])=>[kind,items.filter((x,i)=>mode>0&&(mode===1?i%2===0:mode===2?i%3===1:true)).map(x=>x.id)])),equipped:{},history:[]};
 const legends=mode<2?null:{v:1,legends:Object.fromEntries(catalog.legend.map((item,i)=>[item.id,{xp:mode===6?2000:[0,39,40,105,210,350,1990][(i+mode)%7],form:item.id,path:[],dex:[item.id],nonce:0,care:{bond:0,battle:0,food:0}}])),games:[]};
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legends)storage['dt.legendevo.v1']=JSON.stringify(legends);
 const context={console,DT:{},localStorage:{getItem:key=>storage[key]??null,setItem:(key,value)=>storage[key]=value}};context.window=context;vm.createContext(context);
 vm.runInContext(read('cosmetics.js'),context);for(const [kind,items]of Object.entries(catalog))context.DT.Cosmetics.register(kind,items);
 vm.runInContext('(function(){'+read('legendevo.js')+'})();',context);vm.runInContext(setup,context);
 cases.push({wallet,legends,farm:null,expected:context.inspect()});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/incubator.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${cases.length} original Incubator inventories, ready-first lists and per-item exclusive odds.`);
