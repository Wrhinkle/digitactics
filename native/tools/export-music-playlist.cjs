const fs=require('fs'),path=require('path'),vm=require('vm'),zlib=require('zlib');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/music.js'),'utf8');
let seed=143,draws=[];function random(){seed=(Math.imul(seed,1664525)+1013904223)>>>0;const n=seed/4294967296;draws.push(n);return n;}
const context={console:{warn(){}},Math:Object.assign(Object.create(Math),{random}),cur:{phase:'planning'},changed(){},ac:{currentTime:0},setTimeout(){}};vm.createContext(context);
vm.runInContext(source.slice(source.indexOf('const fits ='),source.indexOf('// ---------- control ----------'))+';this.player=own;',context);
const player=context.player; let transition=null,id=0;
player.playRec=function(rec,fade){if(rec){this.rec=rec;transition={trackId:rec.id,fadeSeconds:fade};}};
const stop=player.stop;player.stop=function(fade=1){if(this.playing)transition={trackId:null,fadeSeconds:fade};return stop.call(this,fade);};
const rows=[];
for(let i=0;i<2500;i++){
 if(i%17===0){const count=[0,1,2,4,9][Math.floor(i/17)%5];player.list=Array.from({length:count},(_,n)=>({id:'track-'+id++,use:['both','planning','combat'][n%3]}));}
 if(i%13===0 && player.list.length)player.list[0].use=['combat','both','planning'][Math.floor(i/13)%3];
 let action=['start','next','phase','next','success','failure','failure','failure','failure','failure','failure','stop','start'][i%13];
 const phase=i%3===0?'combat':'planning';context.cur.phase=phase;draws=[];transition=null;
 // The source UI changes the current phase before invoking the music controller.
 if(action==='start')player.start(phase);else if(action==='phase')player.phase(phase);else if(action==='success')player.fails=0;else if(action==='failure')player.failed();else player[action]();
 rows.push({action,phase,tracks:player.list.map(t=>({...t})),random:draws,transition,current:player.rec?.id??null,playing:player.playing,failed:!!player.error,failures:player.fails});
}
fs.writeFileSync(path.join(root,'native/tests/fixtures/music-playlist.json.gz'),zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} original music playlist transitions.`);
