// Source oracle for kill-screen attribution, honor ordering, layout and pop-in motion.
const fs=require('fs'),path=require('path');
const root=path.resolve(__dirname,'../..'),source=fs.readFileSync(path.join(root,'js/killscreens.js'),'utf8');
const DT=require('../../tools/headless.cjs').load({strict:true});
const resolver=Function('DT',source.slice(source.indexOf('function eliminations(game)'),source.indexOf('// ---------- overlay ----------'))+source.slice(source.indexOf('function teamOf(p)'),source.indexOf('// Model for one Digimon'))+'function next(){};return function(game){queue.length=0;onRoundEnd(game);return queue.slice();}')(DT);
const layout=Function('const LEGEND_SLOT={x:0,y:0,z:.3,s:.86};'+source.slice(source.indexOf('function lineupLayout('),source.indexOf('// Builds a lineup group.'))+'return lineupLayout;')();
const clamp01=x=>Math.max(0,Math.min(1,x)),easeOut=x=>1-Math.pow(1-clamp01(x),3);
const update=Function('L','easeOut','clamp01',source.slice(source.indexOf('  L.update ='),source.indexOf('  L.dispose ='))+'return L.update;');
const records=[];const lines=Object.keys(DT.LINE).slice(0,20);
function snapshot(p){if(!p)return null;return {id:p.id,name:p.name,alive:p.alive,placement:p.placement,team:p.team??null,board:p.boardUnits().map(u=>({line:u.line,stage:u.stage,uid:u.uid,owner:p.id,finalHit:false}))};}
function member(u){return u?{line:u.line,stage:u.stage,uid:u.uid??null,owner:u.owner??null,finalHit:!!u.finalHit}:null;}
function hit(h){return h?{id:h.id,uid:h.uid,line:h.line,stage:h.stage,owner:h.owner??null,victim:h.victim,time:h.t}:null;}
for(let seed=0;seed<512;seed++){
 const duo=!!(seed&1),pve=!!(seed&2),ghost=!!(seed&4);
 const players=Array.from({length:4},(_,id)=>{const board=Array.from({length:(seed+id)%4},(_,i)=>({line:lines[(seed+id+i)%lines.length],stage:(seed+i)%3,uid:id*10+i+1,items:[]}));board.sort((a,b)=>DT.unitValue(b)-DT.unitValue(a));return {id,name:`Tamer ${id}`,alive:!((seed>>(3+id))&1),placement:(seed+id)%8+1,team:duo?Math.floor(id/2):null,boardUnits:()=>board};});
 const a=players[0],a2=duo?players[1]:null,b=pve?null:players[2],b2=duo&&!pve?players[3]:null;
 const makeHit=(p,other)=>({id:p.id*10,uid:(seed&128)?999:p.id*10+1,line:(seed&128)?lines[19]:p.boardUnits()[0]?.line??lines[0],stage:(seed&128)?2:p.boardUnits()[0]?.stage??0,owner:(seed&256)?other?.id??p.id:p.id,victim:42,t:2.5});
 const ka=(seed%5)?makeHit(a,a2):null,kb=b&&seed%7?makeHit(b,b2):null;
 const m={a,a2,b,b2,pve,ghost,duo,combat:{lastKills:{0:ka,1:kb}}},r={match:m,dmgA:(seed%3)*2,dmgB:(seed%4)*2,teamDmgA:duo&&pve?3:0};
 const game={results:[r],human:a,duo};
 const result=resolver(game);
 records.push({rounds:[{a:snapshot(a),a2:snapshot(a2),b:snapshot(b),b2:snapshot(b2),pve,ghost,damageA:r.dmgA,damageB:r.dmgB,teamDamageA:r.teamDmgA,lastKillA:hit(ka),lastKillB:hit(kb)}],expected:result.map(e=>({victim:snapshot(e.victim),killer:snapshot(e.killer),ghost:e.ghost,victims:e.victims.filter(Boolean).map(snapshot),killers:e.killers.map(snapshot),finalHit:member(e.finalHit),team:e.team.map(member)}))});
}
const layouts=[];for(let n=0;n<=24;n++)for(const aspect of [.5,.79,1.29,1.3,16/9,2.4]){const l=layout(n,aspect);layouts.push({n,aspect,halfWidth:l.halfW,top:l.top,narrow:!!l.narrow,slots:l.slots.map(q=>({x:q.x,y:q.y,z:q.z,scale:q.s}))});}
const pops=[];for(let order=0;order<19;order++){const m={slot:{s:order? .8:.86},order,born:null,rig:{update(){}},holder:{scale:{setScalar(x){this.value=x}}}};const L={lastT:null,members:[m]};const advance=update(L,easeOut,clamp01);for(let i=0;i<121;i++){const t=i/60;advance(t);pops.push({order,time:t,born:m.born,scale:m.holder.scale.value,slotScale:m.slot.s});}}
fs.writeFileSync(path.join(root,'native/tests/fixtures/elimination-cinematics.json'),JSON.stringify({records,layouts,pops}));
console.log(`Exported ${records.length} elimination scenarios, ${layouts.length} layouts and ${pops.length} original lineup samples.`);
