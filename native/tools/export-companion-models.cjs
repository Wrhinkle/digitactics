// Export original companion/form selection and animation role metadata.
const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),read=name=>fs.readFileSync(path.join(root,'js',name),'utf8');
const items=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'))).legend;
const models=JSON.parse(fs.readFileSync(path.join(root,'native/content/models.json')));
const ctx={DT:{LINE:require('../../tools/headless.cjs').load({strict:true}).LINE},localStorage:{getItem:()=>null,setItem(){}},addEventListener(){}};
ctx.window=ctx;vm.createContext(ctx);vm.runInContext(read('cosmetics.js'),ctx);ctx.DT.Cosmetics.register('legend',items);vm.runInContext('(function(){'+read('legendevo.js')+'})();',ctx);
const text=read('legends.js');
const policy=vm.runInNewContext(text.slice(text.indexOf('const ROLE_FALLBACK ='),text.indexOf('// height cap per form'))+';({ROLE_FALLBACK,PROC,UNIT_CLIPS,UNIT_PROC})');
const bases={},forms={},reachable={};
for(const item of items){
 const data=item.data,source=data.file?'legend:'+item.id:data.model;
 if(!models.sources[source])throw Error('Missing companion source '+source);
 const unit=!data.file,staticModel=!models.assets[models.sources[source].asset].animations.length;
 bases[item.id]={source,height:data.scale||1,unit,clips:unit?policy.UNIT_CLIPS:data.clips||{},procedural:unit||staticModel?policy.UNIT_PROC:policy.PROC};
 reachable[item.id]=ctx.DT.LegendEvo.reach(item.id);
}
const heights={Fresh:1,'In-Training':1,Rookie:1.05,Champion:1.15,Ultimate:1.2};
for(const id of new Set(Object.values(reachable).flat())){
 if(!ctx.DT.LegendEvo.parse('@'+id).form)continue;
 const form=ctx.DT.LegendEvo.formInfo(id);let visual;
 if(form.legend)visual=bases[form.legend];
 else if(form.model){if(!models.sources[form.model])throw Error('Missing evolution model '+form.model);visual={source:form.model,unit:true,clips:policy.UNIT_CLIPS,procedural:policy.UNIT_PROC};}
 if(!visual)throw Error('Unresolved companion form '+id);
 forms[id]={...visual,height:heights[form.level]||1.2};
}
fs.writeFileSync(path.join(root,'native/content/companion-models.json'),JSON.stringify({schemaVersion:1,bases,forms,reachable,fallbacks:policy.ROLE_FALLBACK})+'\n');
vm.runInContext(text.slice(text.indexOf('function formOf(key)'),text.indexOf('// Template for a digivolved form')),ctx);
const cases=[];
for(const item of [...items,{id:'unknown'}])for(const key of [item.id,...[...Object.keys(forms),'unknown',item.id].map(f=>item.id+'@'+f)]){
 ctx.input=key;const form=vm.runInContext('formOf(input)',ctx),base=bases[item.id]||bases.tokomon;
 const expected=form?forms[form.id]:base;
 cases.push({key,source:expected.source,height:expected.height,unit:expected.unit});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/companion-models.json.gz'),require('zlib').gzipSync(JSON.stringify(cases)));
console.log(`Exported ${items.length} companions, ${Object.keys(forms).length} forms and original animation role mappings.`);
