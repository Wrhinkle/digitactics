const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/mystery-notices.js'),'utf8');
const actions=[
 {op:'earn',id:'match1:201',round:'2-1',drops:['Daybreak Box','Cipher Box','Cipher Key']},
 {op:'earn',id:'match1:201',round:'2-1',drops:['Daybreak Box','Cipher Box','Cipher Key']},
 {op:'advance',seconds:4},{op:'advance',seconds:100,paused:true},
 {op:'earn',id:'match1:202',round:'2-2',drops:['Cipher Box','Cipher Key']},
 {op:'earn',id:'match1:203',round:'2-3',drops:['Cipher Key','Cipher Key']},
 {op:'dismiss'},{op:'advance',seconds:9.5},{op:'advance',seconds:.5},
 {op:'earn',id:'secret',round:'3-1',drops:['Unopened secret cosmetic name']},
 {op:'earn',id:'match1:301',round:'3-1',drops:['Daybreak Box']},{op:'clear'},
 {op:'earn',id:'match2:201',round:'2-1',drops:['Cipher Key']},{op:'reload'},
 ...Array.from({length:60},(_,i)=>({op:'earn',id:'burst:'+i,round:'4-'+i,drops:i%2?['Cipher Box']:['Cipher Key']})),
 {op:'dismiss'},{op:'advance',seconds:10},{op:'dismiss'}
];
let context,queue;function reboot(){context={window:{DT:{}}};vm.createContext(context);vm.runInContext(source,context);queue=vm.runInContext('queue',context);}reboot();
const cases=actions.map(action=>{let result=null;switch(action.op){case'earn':result=queue.earned(action);break;case'advance':queue.advance(action.seconds,action.paused);break;case'dismiss':queue.dismiss();break;case'clear':queue.clear();break;case'reload':reboot();break;}
 return{action,result,expected:{current:queue.current,elapsed:queue.elapsed,pendingCount:queue.pendingCount}};});
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/loot-notices.json'),JSON.stringify(cases,null,2)+'\n');
console.log(`Exported ${cases.length} source loot-notification transitions: grouping, count conservation, duplicate suppression, pause, expiry, reset/reload and secret filtering.`);
