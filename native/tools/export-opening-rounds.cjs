const fs=require('node:fs'),path=require('node:path'),zlib=require('node:zlib'),DT=require('../../tools/headless.cjs').load({strict:true}),traces=[];
class Opening extends DT.Game {
 fireGame(name,p,e){if(name==='onRoundStart'){(this.trace??=[]).push(`round:${p.id}`);p.freeRolls++;if(p.id===0)p.hp++;return e;}return super.fireGame(name,p,e);}
 startCarousel(){(this.trace??=[]).push('carousel');super.startCarousel();}
 planAI(){(this.trace??=[]).push('ai');this.players[3].hp+=2;this.makePairs();if(this.duo)this.syncTeams();}
}
for(const set of DT.Sets.pickable())for(const pace of ['normal','hyper'])for(const duos of [false,true])for(const seed of [0,17,91,999,0x80000000,0xffffffff]){
 const g=new Opening({setId:set.id,pace,seed,format:duos?'duos':'solo',slots:Array.from({length:8},(_,i)=>({name:`Player ${i}`,human:i===0||i===3}))});
 traces.push({variant:`${set.id}/${pace}`,seed,duos,log:g.trace||[],phase:g.phase,type:g.roundType,players:g.players.map(p=>({gold:p.gold,hp:p.hp,rolls:p.freeRolls,shop:p.shop,offer:p.offer,items:p.items,bench:p.bench})),pool:g.pool,pairs:(duos?g.duoPairs||[]:g.pairs).map(([a,b,ghost])=>({a:a.id,b:b.id,ghost})),carousel:g.carousel?{options:g.carousel.opts,order:g.carousel.order.map(p=>p.id),index:g.carousel.idx}:null,nextRandom:g.rng()});
}
fs.writeFileSync(path.join(__dirname,'../tests/fixtures/opening-rounds.json.gz'),zlib.gzipSync(JSON.stringify(traces)));console.log(`Exported ${traces.length} opening-round references.`);
