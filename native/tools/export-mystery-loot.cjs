// Execute the original local reward modules; no copied probability/reward implementation.
const fs=require('fs'),path=require('path'),vm=require('vm');
const root=path.resolve(__dirname,'../..'),lootSource=fs.readFileSync(path.join(root,'js/mystery-loot.js'),'utf8');
const cosmeticSource=fs.readFileSync(path.join(root,'js/cosmetics.js'),'utf8');
const catalog=JSON.parse(fs.readFileSync(path.join(root,'native/src/DigiTactics.Storage/cosmetics.json'),'utf8'));
const LOOT='dt.mysteryLoot.v1',COSMETICS='dt.cosmetics.v1',MAX=4294967295,day='2026-10-18';
const copy=value=>JSON.parse(JSON.stringify(value));
function harness(initial={}){
  const storage={};
  if(initial.loot!==undefined&&initial.loot!==null)storage[LOOT]=typeof initial.loot==='string'?initial.loot:JSON.stringify(initial.loot);
  if(initial.cosmetics)storage[COSMETICS]=JSON.stringify(initial.cosmetics);
  let context,step={},randomUsed=0,lootWrites=0,cosmeticWrites=0;
  const reboot=()=>{
    const NativeDate=Date;
    const FixedDate=class extends NativeDate{constructor(...args){super(...(args.length?args:[...step.day.split('-').map(Number).map((n,i)=>i===1?n-1:n),12]));}};
    context={console,structuredClone,Uint32Array,Date:FixedDate,DT:{Client:{toast(){}}},
      crypto:{getRandomValues(array){if(randomUsed>=step.random.length)throw Error('Fixture exhausted random values');array[0]=step.random[randomUsed++];return array;}},
      localStorage:{getItem:key=>storage[key]??null,setItem(key,value){
        const nth=key===LOOT?++lootWrites:++cosmeticWrites;
        const failures=key===LOOT?step.failLootWrites:step.failCosmeticWrites;
        if(failures?.includes(nth))throw Error('Injected original persistence failure');storage[key]=value;
      }}};
    context.window=context;vm.createContext(context);vm.runInContext(cosmeticSource,context);
    for(const kind of context.DT.Cosmetics.KINDS)context.DT.Cosmetics.register(kind,catalog[kind]);
    vm.runInContext(lootSource,context);
  };
  return {run(input){
    step={day,random:[],...input};randomUsed=lootWrites=cosmeticWrites=0;
    let result=null;
    if(!context||step.op==='reload')reboot();
    if(step.op==='round'){
      const spec=step.game,me={id:spec.humanId},other={id:99},match={a:other,b:{id:98}};
      let participation=spec.participation;
      if(participation.startsWith('ghost-')){match.ghost=true;participation=participation.slice(6);}
      if(['a','a2','b','b2'].includes(participation))match[participation]=me;
      const game={...spec,human:participation==='no-human'?null:me,results:participation==='no-results'?[]:[{match}]};
      result=context.DT.MysteryLoot.roundEnd(game);
    }else if(step.op==='open')result=context.DT.MysteryLoot.open(step.pool);
    const expected={result:copy(result),state:copy(vm.runInContext('save',context)),inventory:copy(context.DT.MysteryLoot.inventory()),
      owned:Object.fromEntries(context.DT.Cosmetics.KINDS.map(kind=>[kind,context.DT.Cosmetics.list(kind).filter(it=>context.DT.Cosmetics.owns(kind,it.id)).map(it=>it.id)])),
      randomUsed,lootWrites,cosmeticWrites};
    return {...step,expected};
  }};
}
const sourceKinds=(()=>{const ctx={DT:{},localStorage:{getItem(){return null;}}};ctx.window=ctx;vm.createContext(ctx);vm.runInContext(cosmeticSource,ctx);return copy(ctx.DT.Cosmetics.KINDS);})();
const pools=Object.fromEntries(['daily','vault'].map(pool=>[pool,sourceKinds.flatMap(kind=>catalog[kind].filter(it=>it.lootPool===pool).map(it=>({kind,id:it.id})))]));
const cases=[],failureCases=[];
const game=(changes={})=>({setId:'reckoning',duo:false,seed:143,humanId:0,stage:1,round:1,participation:'a',...changes});
const round=(changes={},extra={})=>({op:'round',game:game(changes),...extra});
function add(name,initial,steps,failure=false){initial=copy(initial);const runtime=harness(initial);(failure?failureCases:cases).push({name,initial,steps:steps.map(step=>runtime.run(step))});}
add('fresh-no-round',{},[{op:'reload'}]);
add('malformed-save-recovers',{loot:'not-json'},[round()]);
add('invalid-counters-and-ledger-sanitize',{loot:{daily:-1,vault:1.5,keys:9007199254740992,boxMisses:'9',keyMisses:-1,games:[]}},[round()]);
for(const participation of ['a','a2','b','b2','ghost-a','ghost-a2','ghost-b','ghost-b2','none','no-results','no-human'])
  add('participation-'+participation,{},[round({participation})]);
add('daily-monotonic-and-round-ledger',{},[
  round(),round(),round({round:0}),round({round:2}),round({round:3},{day:'2026-10-19'}),
  round({round:4},{day:'2026-10-18'}),round({round:5},{day:'2026-10-20'}),{op:'reload',day:'2026-10-20'},round({round:5},{day:'2026-10-20'})]);
add('ledger-distinguishes-set-mode-seed-seat',{},[
  round(),round({setId:'origins'}),round({duo:true}),round({seed:144}),round({humanId:1}),round({humanId:1})]);
add('daily-cross-year',{loot:{day:'2026-12-31'}},[round({}, {day:'2027-01-01'}),round({round:2},{day:'2027-01-01'})]);
add('daily-future-clock-preserved',{loot:{daily:3,day:'2027-01-01',future:{keep:true}}},[round()]);
for(const [name,random] of [['both-drop',[0,0]],['only-box',[0,MAX]],['only-key',[MAX,0]],['neither',[MAX,MAX]],
  ['below-box-and-key-threshold',[257698037,214748364]],['above-box-and-key-threshold',[257698038,214748365]]])
  add('stage2-'+name,{},[round({stage:2},{random})]);
add('pity-both-without-random',{loot:{boxMisses:15,keyMisses:19}},[round({stage:2})]);
add('pity-box-skips-first-random',{loot:{boxMisses:15,keyMisses:3}},[round({stage:2},{random:[0]})]);
add('pity-key-skips-second-random',{loot:{boxMisses:3,keyMisses:19}},[round({stage:2},{random:[0]})]);
add('stage1-does-not-advance-pity',{loot:{boxMisses:15,keyMisses:19}},[round(),round({stage:2})]);
add('pity-through-21-misses',{},Array.from({length:21},(_,i)=>round({stage:2,round:i+1},{random:i===15||i===19?[MAX]:[MAX,MAX]})));
const games=Object.fromEntries(Array.from({length:1000},(_,i)=>[`old:solo:${i}:0`,101]));
add('bounded-ledger-evicts-oldest',{loot:{games}},[round(),round({setId:'old',seed:0}),round({setId:'old',seed:999})]);
for(const pool of ['daily','vault']){
  for(let i=0;i<pools[pool].length;i++)add(`select-${pool}-${i}`,{loot:{[pool]:1,keys:pool==='vault'?1:0}},[{op:'open',pool,random:[Math.ceil(i*4294967296/pools[pool].length)]}]);
  add(`select-${pool}-upper-endpoint`,{loot:{[pool]:1,keys:1}},[{op:'open',pool,random:[MAX]}]);
  add(`empty-${pool}`,{},[{op:'open',pool}]);
  const allOwned=Object.fromEntries(sourceKinds.map(kind=>[kind,pools[pool].filter(it=>it.kind===kind).map(it=>it.id)]));
  add(`full-${pool}-keeps-box`,{loot:{[pool]:2,keys:2},cosmetics:{owned:allOwned}},[{op:'open',pool}]);
  const remaining=pools[pool][Math.floor(pools[pool].length/2)];
  allOwned[remaining.kind]=allOwned[remaining.kind].filter(id=>id!==remaining.id);
  add(`last-${pool}-reward-then-exhaustion`,{loot:{[pool]:2,keys:2},cosmetics:{owned:allOwned}},[{op:'open',pool,random:[MAX]},{op:'open',pool},{op:'reload'}]);
  const first=pools[pool][0];
  add(`never-repeat-${pool}`,{loot:{[pool]:3,keys:3}},[{op:'open',pool,random:[0]},{op:'open',pool,random:[0]},{op:'open',pool,random:[0]}]);
  add(`pending-${pool}-startup-recovery`,{loot:{[pool]:0,keys:0,pending:first}},[{op:'reload'},{op:'reload'}]);
  add(`pending-${pool}-already-owned-recovery`,{loot:{pending:first},cosmetics:{owned:{[first.kind]:[first.id]}}},[{op:'reload'},{op:'reload'}]);
}
add('vault-needs-key',{loot:{vault:1,keys:0}},[{op:'open',pool:'vault'}]);
add('invalid-pool-preserves-inventory',{loot:{daily:1,vault:1,keys:1}},[{op:'open',pool:'unknown'}]);
add('unknown-pending-preserved',{loot:{daily:1,pending:{kind:'legend',id:'unknown'}}},[{op:'reload'},{op:'open',pool:'daily'}]);
add('failed-round-write-retry',{},[round({stage:2},{random:[0,0],failLootWrites:[1]}),round({stage:2},{random:[0,0]})],true);
add('failed-box-debit-retry',{loot:{daily:1}},[{op:'open',pool:'daily',random:[0],failLootWrites:[1]},{op:'open',pool:'daily',random:[0]}],true);
add('failed-cosmetic-grant-recovers',{loot:{daily:1}},[{op:'open',pool:'daily',random:[0],failCosmeticWrites:[1]},{op:'reload'}],true);
add('failed-pending-clear-recovers',{loot:{vault:1,keys:1}},[{op:'open',pool:'vault',random:[0],failLootWrites:[2]},{op:'reload'}],true);
add('startup-grant-failure-preserves-pending',{loot:{daily:1,pending:pools.daily[0]}},[{op:'reload',failCosmeticWrites:[1]},{op:'reload'}],true);
const result={sourceKinds,pools,cases,failureCases};
fs.writeFileSync(path.join(root,'native/tests/fixtures/mystery-loot.json'),JSON.stringify(result));
console.log(`Exported ${cases.length} mystery cases (${cases.reduce((n,c)=>n+c.steps.length,0)} steps), ${failureCases.length} persistence recovery cases, and ${pools.daily.length+pools.vault.length} source pool choices.`);
