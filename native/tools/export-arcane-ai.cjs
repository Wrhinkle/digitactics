const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
for(const pace of ['normal','hyper'])for(const human of [false,true])for(let n=0;n<256;n++){
 DT.Sets.activate('arcane',pace);const R=DT.HYPER,g=Object.create(DT.Game.prototype),seed=n*53+19;
 Object.assign(g,{R,set:DT.SET,uid:0,rng:DT.makeRng(seed),phase:n%3===0?'combat':'planning',stage:1,round:1,mech:{},pool:Object.fromEntries(DT.SHOP_LINES.map(l=>[l.id,DT.POOL_SIZE[l.tier]]))});
 const p=new DT.Player(0,'Player',human,g);g.players=[p];g.refreshShop(p);const added=[],syn=DT.SHOP_LINES.filter(l=>l.traits.includes('SYN'));
 for(let j=0;j<(n%5===0?13:8);j++){
  const line=j<3?(n%3===0?DT.SHOP_LINES[n%DT.SHOP_LINES.length].id:syn[n%syn.length].id):DT.SHOP_LINES[(n+j)%DT.SHOP_LINES.length].id;
  const u=g.newUnit(line,j<3?n%2:j%3);u.items=j===0?DT.CORE_ITEMS.slice(0,n%4):[];if(j===3&&n%2===0)u.items=['arc_crest_syn'];if(j===1)u.mods={arcAnom:'x_power',other:7};if(j===2)u.bonusTraits={SYN:3};u.noItems=j===0&&n%7===0;
  if(j<4)p.board[`${j},4`]=u;else p.bench[j-4]=u;added.push({unit:structuredClone(u),board:j<4,col:j});
 }
 const contra=Object.values(DT.ITEMS).filter(i=>i.cat==='contraband').map(i=>i.id);p.items=[];for(let i=0;i<1+n%10;i++)p.items.push(i%3===0?'arc_copier':i%3===1?'arc_copier_small':contra[(n+i)%contra.length]);if(n%9===0)while(p.items.length<R.itemCap)p.items.push(DT.CORE_ITEMS[0]);
 const drained=n%4===0?p.allUnits()[0].line:null;if(drained)g.pool[drained]=0;const before=p.items.slice();
 g.fireGame('onAiTurn',p,{});
 traces.push({variant:`arcane/${pace}`,human,n,seed,phase:g.phase,added,drained,before,after:structuredClone({items:p.items,board:p.board,bench:p.bench,shop:p.shop,pool:g.pool}),nextId:g.newUnit(DT.SHOP_LINES[0].id,0).uid,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/arcane-ai.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} Arcane AI traces.`);
