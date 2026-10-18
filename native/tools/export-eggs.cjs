const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const shop=read('shop.js'), cosmetics=read('cosmetics.js'),evo=read('legendevo.js');
const setup=`const DT=window.DT,C=DT.Cosmetics,W=DT.Wallet;const closeModal=()=>null,sfx=()=>{},msg=()=>null;`+
 shop.slice(shop.indexOf('const KINDS'),shop.indexOf('// Daily missions'))+
 shop.slice(shop.indexOf('const EGGS'),shop.indexOf('// ---------- DOM'))+
 shop.slice(shop.indexOf('function hatch(eggId)'),shop.indexOf('  const dupText ='))+
 'return {kind,id:item.id,duplicate:dup,refund,evolution:evo};}\nthis.hatch=hatch;this.eggs=EGGS;this.odds=e=>eggOdds(eggPool(e));';
const cases=[], eggIds=['mixed','legend','emote','board','killscreen','dance','sound','elimsound'];let odds;
for(const egg of eggIds)for(const mode of ['new','duplicate','near-cap','max','poor'])for(let i=0;i<64;i++){
 const wallet={bits:mode==='poor'?1:1000,owned:Object.fromEntries(Object.entries(catalog).map(([k,items])=>[k,mode==='new'?[]:items.map(x=>x.id)])),equipped:{},history:[],future:'keep'};
 const legend=mode==='near-cap'||mode==='max'?{v:1,legends:Object.fromEntries(catalog.legend.map(it=>[it.id,{xp:mode==='max'?2000:1990,form:it.id,path:[],nonce:3,dex:[it.id],care:{bond:1,battle:2,food:3},future:'keep'}])),games:[],future:'keep'}:null;
 const rolls=[i/64,((i*37)%64)/64],storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legend)storage['dt.legendevo.v1']=JSON.stringify(legend);
 const math=Object.create(Math);let random=0;math.random=()=>rolls[random++];
 const ctx={DT:{},console,Math:math,Date:{now:()=>1234567},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);for(const [k,items]of Object.entries(catalog))ctx.DT.Cosmetics.register(k,items);
 vm.runInContext('(function(){'+evo+'})();',ctx);vm.runInContext(setup,ctx);
 if(!odds)odds=ctx.eggs.map(e=>({id:e.id,odds:ctx.odds(e)}));
 const award=ctx.hatch(egg);
 cases.push({egg,rolls,wallet,legend,award,walletAfter:JSON.parse(storage['dt.cosmetics.v1']),legendAfter:storage['dt.legendevo.v1']?JSON.parse(storage['dt.legendevo.v1']):null});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/eggs.json.gz'),zlib.gzipSync(JSON.stringify({odds,cases})));
console.log(`Exported ${cases.length} original egg transactions.`);
