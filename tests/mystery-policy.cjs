// Independent earning/exclusivity contracts; execute the source implementation with local entropy.
const fs=require('fs'),vm=require('vm'),assert=require('assert'),path=require('path');
const root=path.join(__dirname,'..');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const code=fs.readFileSync(path.join(root,'js/mystery-loot.js'),'utf8'),cosmetics=fs.readFileSync(path.join(root,'js/cosmetics.js'),'utf8');
let checks=0;function check(value,label){checks++;assert.ok(value,label);}
function harness(initial={}){
 const storage={'dt.mysteryLoot.v1':JSON.stringify(initial)},NativeDate=Date;let day='2026-10-18',random=[],used=0;
 const context={structuredClone,Uint32Array,console,DT:{Client:{toast(){}}},localStorage:{getItem:k=>storage[k]??null,setItem:(k,v)=>storage[k]=v},
 Date:class extends NativeDate{constructor(...args){super(...(args.length?args:[day+'T12:00:00']));}},
 crypto:{getRandomValues(a){check(used<random.length,'entropy count');a[0]=random[used++];return a;}}};context.window=context;vm.createContext(context);vm.runInContext(cosmetics,context);
 for(const [kind,items]of Object.entries(catalog))context.DT.Cosmetics.register(kind,items);vm.runInContext(code,context);
 return {context,state:()=>JSON.parse(storage['dt.mysteryLoot.v1']),round(n,{date=day,stage=1,rolls=[]}={}){day=date;random=rolls;used=0;const human={id:0};const drops=context.DT.MysteryLoot.roundEnd({human,setId:'reckoning',duo:false,seed:1,stage,round:n,results:[{match:{a:human}}]});return [...drops];}};
}
let h=harness();check(h.round(1).length===0,'first active day advances without a chest');check(h.state().dayProgress===1,'first active day persists progress');
check(h.round(2).length===0&&h.state().dayProgress===1,'more same-day rounds do not advance daily progress');
check(h.round(3,{date:'2026-11-20'}).join()==='Daybreak Box'&&h.state().dayProgress===0,'nonconsecutive second active day earns exactly one chest');
check(h.round(3,{date:'2026-11-21'}).length===0&&h.state().day==='2026-11-20','duplicate settlement cannot advance local day');
check(h.round(4,{date:'2026-11-19'}).length===0&&h.state().dayProgress===0,'backward local date does not advance');
check(h.round(5,{date:'2026-11-21'}).length===0&&h.state().dayProgress===1,'third active day starts next pair');
h=harness({daily:7,vault:3,keys:2,day:'2026-10-18',boxMisses:15,keyMisses:19,pending:{kind:'legend',id:'unknown'},future:{keep:1}});
check(h.round(1).length===0&&h.state().dayProgress===0,'old same-day save does not receive retroactive chest');
check(h.round(2,{date:'2026-10-19'}).length===0&&h.state().dayProgress===1,'old save starts durable two-day cadence');
check(h.round(3,{date:'2026-10-25'}).join()==='Daybreak Box','migration does not require consecutive days');
check(h.state().daily===8&&h.state().vault===3&&h.state().keys===2&&h.state().pending.id==='unknown'&&h.state().future.keep===1,'migration preserves earned inventory, pending reward and unknown fields');
for(const [label,box,key,want]of [['below',128849018,107374182,'Cipher Box,Cipher Key'],['above',128849019,107374183,'']]){
 h=harness();check(h.round(1,{stage:2,rolls:[box,key]}).join()===want,label+' exact3%/2.5% boundary');
}
h=harness({boxMisses:30,keyMisses:38});check(h.round(1,{stage:2,rolls:[4294967295,4294967295]}).length===0,'not guaranteed one round before32/40');
check(h.round(2,{stage:2}).join()==='Cipher Box,Cipher Key','32/40 pity guarantees without consuming entropy');
h=harness({dayProgress:1,day:'2026-10-17',boxMisses:31,keyMisses:39});check(h.round(1).join()==='Daybreak Box'&&h.state().boxMisses===31&&h.state().keyMisses===39,'stage1 never advances Cipher pity');
const ids=new Set();for(const [kind,items]of Object.entries(catalog))for(const item of items.filter(x=>x.lootPool)){
 check(item.secret===true&&item.egg===false&&!item.free&&!item.linked,'mystery item remains exclusive: '+item.id);check(!ids.has(kind+'/'+item.id),'unique mystery ID');ids.add(kind+'/'+item.id);
 check(!h.context.DT.Cosmetics.visible(kind).some(x=>x.id===item.id),'undiscovered item hidden');h.context.DT.Cosmetics.grant(kind,item.id);
 check(h.context.DT.Cosmetics.visible(kind).some(x=>x.id===item.id),'owned discovery visible');
}
check(ids.size===250,'current two pools retain all250 owned-compatible IDs');
console.log('Mystery earning and secrecy: '+checks+' checks passed.');
