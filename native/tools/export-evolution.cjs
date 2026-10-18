const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),read=n=>fs.readFileSync(path.join(root,'js',n),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8')).legend;
const LINE=require('../../tools/headless.cjs').load({strict:true}).LINE;
const cosmetics=read('cosmetics.js'),evo=read('legendevo.js'),farmSource=read('farm.js');
function load(wallet,legend,farm){
 const storage={'dt.cosmetics.v1':JSON.stringify(wallet)};if(legend)storage['dt.legendevo.v1']=JSON.stringify(legend);if(farm)storage['dt.farm.v1']=JSON.stringify(farm);
 const ctx={DT:{LINE},console,setInterval:()=>0,addEventListener:()=>{},localStorage:{getItem:k=>storage[k]||null,setItem:(k,v)=>storage[k]=v}};ctx.window=ctx;vm.createContext(ctx);
 vm.runInContext(cosmetics,ctx);ctx.DT.Cosmetics.register('legend',catalog);vm.runInContext('(function(){'+evo+'})();',ctx);vm.runInContext('(function(){'+farmSource+'})();',ctx);
 return {E:ctx.DT.LegendEvo,storage};
}
const {E}=load({});
const ids=[...new Set([...Object.keys(E.FORMS),...catalog.map(x=>x.id)])];
const forms=Object.fromEntries(ids.map(id=>[id,E.formInfo(id)]));
const children=Object.fromEntries(ids.map(id=>[id,(E.EVO[catalog.find(x=>x.id===id)?.data?.evoRoot||id]||'').split(/\s+/).filter(Boolean).map(s=>{const [form,care]=s.split(':');return {form,care};}).filter(x=>E.FORMS[x.form]) ]));
fs.writeFileSync(path.join(root,'native/src/DigiTactics.Storage/evolution.json'),JSON.stringify({forms,children,dataFor:E.DATA_FOR,tierLevel:E.TIER_LEVEL,itLevel:E.IT_LEVEL}));
const cases=[],discoveries=[],cares=[];
for(const item of catalog)for(const form of [...E.reach(item.id),'invalid-form'])for(let mode=0;mode<4;mode++){
 const id=item.id, wallet={bits:mode===3?0:10000,owned:{legend:mode===2?[]:[id]},equipped:{},history:[],future:true};
 const legend={v:1,legends:{[id]:{xp:[0,350,2000,1990][mode],form,path:form===id?[]:[id],dex:[id],nonce:mode*143,care:{bond:mode*9,battle:mode*7,food:mode*11},future:'keep'}},games:[],future:'keep'};
 const farm=mode===0?null:{v:1,pets:{[id]:{weight:40,goodBond:mode*3.75,goodFood:mode*2.5,str:mode*44,spd:13,wis:26,sta:19,mistakes:mode,poopNeglect:mode,sick:mode===3,overfeed:mode*3}}};
 const context=load(wallet,legend,farm),info=context.E.info(id);
 const view={form:info.form,level:info.level,data:info.data,need:info.need,next:info.next,canDigivolve:info.canDigivolve,canRevert:info.canRevert,odds:info.odds,levelBits:info.levelBits,roll:context.E._roll(id)};
 discoveries.push({id,legend,current:info.form,path:info.path,dex:info.dex});
 cares.push({id,legend,farm,values:info.care,bonus:Object.fromEntries(['bond','battle','food','wild'].map(key=>[key,info.careBonus[key]]))});
 cases.push({id,wallet,legend,farm,view});
}
for(const item of catalog){
 const wallet={bits:10000,owned:{legend:[item.id]},equipped:{},history:[]},context=load(wallet,null,null),info=context.E.info(item.id);
 const view=Object.fromEntries(['form','level','data','need','next','canDigivolve','canRevert','odds','levelBits'].map(k=>[k,info[k]]));view.roll=context.E._roll(item.id);
 discoveries.push({id:item.id,legend:null,current:info.form,path:info.path,dex:info.dex});
 cares.push({id:item.id,legend:null,farm:null,values:info.care,bonus:Object.fromEntries(['bond','battle','food','wild'].map(key=>[key,info.careBonus[key]]))});
 cases.push({id:item.id,wallet,legend:null,farm:null,view});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution.json.gz'),zlib.gzipSync(JSON.stringify(cases)));
console.log(`Exported ${ids.length} evolution forms and ${cases.length} source views/rolls with farm care.`);
const actions=[];
for(const test of cases){
 const form=test.legend?.legends[test.id].form||test.id;
 if(form!==test.id && form!==E.reach(test.id)[1] && form!=='invalid-form')continue;
 for(const action of ['feed','buyLevel','digivolve','revert']){
  const context=load(test.wallet,test.legend,test.farm),result=context.E[action](test.id);
  actions.push({...test,action,result,legendAfter:JSON.parse(context.storage['dt.legendevo.v1']||'null'),walletAfter:JSON.parse(context.storage['dt.cosmetics.v1'])});
 }
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-actions.json.gz'),zlib.gzipSync(JSON.stringify(actions)));
console.log(`Exported ${actions.length} original evolution actions.`);

fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-trees.json'),JSON.stringify(catalog.map(item=>E.tree(item.id))));
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-discovery.json.gz'),zlib.gzipSync(JSON.stringify(discoveries)));
fs.writeFileSync(path.join(root,'native/tests/fixtures/evolution-care.json.gz'),zlib.gzipSync(JSON.stringify(cares)));
