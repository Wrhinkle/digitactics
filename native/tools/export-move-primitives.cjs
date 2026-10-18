// Reference fixtures execute the unchanged source move/timeline/particle code.
// Headless capture replaces GPU pools only; it records the commands sent to them.
const fs = require('node:fs'), path = require('node:path'), zlib = require('node:zlib');
const root = path.resolve(__dirname, '../..');
const cases = [];
const add = (primitive, options = {}, suffix = 'default', context = {}) => cases.push({name: `${primitive}/${suffix}`, primitive, options, context});
for (const primitive of ['orb','breath','beam','missiles','bolt','slash','dash','erupt','meteor','sphere','wave','arrows',
  'cross','vortex','portal','spikes','whip','swarm','shock','dome','heal','tornado','rings','volley','streak','wheel','net','pillar','rain','glyph','drain']) add(primitive);
add('orb', {count:3,from:'hands',side:.5,wobble:.3,arc:0,head:'leaf',headColor:'w',trail:'bubble',wings:true,muzzle:false,halo:false,size:.31}, 'winged-leaves');
add('breath', {cell:'leaf',mix:'spark',dur:.65,width:.8,rate:170,speed:7,color:'#78abef'}, 'mixed-alpha');
add('beam', {twin:true,from:'hands',charge:.25,dur:.65,overshoot:0,spiral:true,glyph:'x',width:.21}, 'twin-spiral');
add('missiles', {count:4,organic:true,smoke:false,curve:0,arc:0,size:.2}, 'organic-straight');
add('bolt', {area:3,count:3,height:9,width:.15}, 'area');
add('bolt', {from:'caster',launch:'back',dur:.4}, 'caster');
add('slash', {vertical:true,claw:true,count:4,spread:0}, 'vertical-claws');
add('dash', {ghosts:4,slashes:3,claw:true,spread:0}, 'from-leap', {from:{x:-3,z:-2}});
add('dash', {slashes:0,to:'self'}, 'no-slashes');
for (const kind of ['ice','rock','thorn','metal','water','dark']) add('erupt', {kind,to:'center',count:3,height:2.8}, kind);
add('meteor', {count:3,trail:'dark',size:.7}, 'shower');
add('sphere', {from:'mouth',grow:0,cell:'snow',trail:'ice'}, 'instant-mouth');
add('sphere', {from:'hands',grow:.22,core:'w',color:'#61a7ef'}, 'hands');
add('wave', {count:3,flat:true,width:1.7}, 'flat-multiple');
add('arrows', {rain:true,count:5,cell:'needle',size:.8}, 'rain');
add('arrows', {cell:'invalid',count:4,to:'center'}, 'unknown-cell');
add('cross', {orbs:true,size:1.6,to:'self'}, 'orbs');
add('vortex', {to:'targets',up:true,cell:'card',size:1.7}, 'cards');
add('portal', {size:1.9,dur:.7}, 'large');
add('spikes', {kind:'rock',count:4.5,gap:.06}, 'fractional-count');
add('whip', {count:2,lightning:true,from:'sky',width:.18,sway:1.3}, 'lightning');
add('whip', {leafy:false,count:2}, 'plain');
add('swarm', {cell:'card',fan:true,hold:.16,spin:5,count:6,arc:0,wobble:0,glow:false}, 'cards');
add('swarm', {cell:'leaf',tint:true,spread:1.2,spin:-3}, 'leaves');
add('shock', {hammer:true,to:'targets',size:2,cell:'shard',debris:false}, 'hammer');
add('dome', {orbit:'card',count:5}, 'orbit');
add('heal', {cell:'leaf',motes:false,color:'#fe8c44'}, 'leaves');
add('tornado', {to:'center',cell:'feather',height:4}, 'feathers');
add('rings', {from:'sky',count:4,size:.8}, 'sky');
add('volley', {guns:1,spread:0,count:7}, 'single-gun');
add('volley', {from:'back',cell:'bolt'}, 'back');
add('streak', {width:1.3,trail:'ice'}, 'ice');
add('wheel', {saw:true,cell:'spark',size:.9}, 'saw');
add('net', {size:1.3}, 'large');
add('pillar', {height:6,size:.8}, 'large');
add('rain', {to:'targets',cell:'leaf',count:51}, 'leaves');
add('glyph', {place:'ground',glyph:'cross'}, 'ground');
add('glyph', {glyph:'x'}, 'x');
add('orb', {to:'center'}, 'no-targets', {T:[]});
add('slash', {}, 'stun', {type:'stun',stunTime:.6});
add('sphere', {trail:'holy'}, 'drain', {type:'drain'});

(async () => {
  const create = await require('./move-reference.cjs')();
  const result = [];
  for (let caseIndex = 0; caseIndex < cases.length; caseIndex++) {
    const row = cases[caseIndex], seed = 84219 + caseIndex * 137;
    const {THREE,fx,snapshot,primitives,nextRandom,step} = create(seed);
    const c = {
      A:{x:-1.6,y:1.05,z:-.7},P:{x:-1.8,z:-1},C:{x:2.1,z:1.7},T:[{x:2.1,y:.8,z:1.7},{x:1.2,y:1.1,z:2.3}],
      h:1.65,R:1.8,S:1.3,stage:1,yaw:.84,ca:new THREE.Color('#e57630'),cb:new THREE.Color('#ffe9a4'),
      spec:{impact:'hit',shake:.2},shook:false,stunTime:1,...row.context
    };
    primitives[row.primitive](fx,c,row.options,.07);
    const initial = snapshot(), frames = [], samples = new Set([0,2,6,12,22,36,54,79,109,159]);
    for(let i=0;i<160;i++){
      const dt = i%17===0?.05:i%7===0?.027:1/60;
      step(dt);
      frames.push(samples.has(i)?{dt,snapshot:snapshot()}:{dt});
    }
    result.push({...row,seed,initial,frames,nextRandom:nextRandom()});
  }
  const destination = path.join(root,'native/tests/fixtures/move-primitives.json.gz');
  fs.writeFileSync(destination,zlib.gzipSync(JSON.stringify(result)));
  console.log(`Exported ${result.length} original move primitive cases to ${path.relative(root,destination)} (${fs.statSync(destination).size.toLocaleString()} bytes).`);
})().catch(error=>{console.error(error);process.exitCode=1;});
