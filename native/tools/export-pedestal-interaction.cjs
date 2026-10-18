// Reference gestures execute the exact Stage pointer listeners and yaw expression.
const fs = require('node:fs'), path = require('node:path');
const root = path.resolve(__dirname, '../..');
const source = fs.readFileSync(path.join(root, 'js/client.js'), 'utf8').replace(/\r\n/g, '\n');
const start = source.indexOf("    this.canvas.addEventListener('pointerdown'");
const end = source.indexOf('\n  }\n  setLegend(key)', start);
if (start < 0 || end < 0) throw new Error('Missing source Stage pointer listeners.');
const yaw = source.match(/const sway = save\.prefs\.motion[^;]+;\n\s*this\.spin\.rotation\.y = sway \+ this\.yawOff;/)?.[0];
if (!yaw) throw new Error('Missing source Stage yaw expression.');
const install = new Function('Math', 'sfx', source.slice(start, end));
const rotate = new Function('save', yaw);
const cases = [];
const down = x => ({type:'down',x}), move = x => ({type:'move',x}), up = {type:'up'};
const add = (name, actions, random = [.1], options = {}) => cases.push({name, actions, random, ...options});
add('role-quartiles', Array.from({length:8},()=>[down(12),up]).flat(), [0,.249999999,.25,.499999999,.5,.749999999,.75,.999999999]);
add('exact-positive-threshold', [down(100),move(103),up]);
add('exact-negative-threshold', [down(100),move(97),up], [.75]);
add('above-positive-threshold', [down(100),move(103.000001),up]);
add('above-negative-threshold', [down(100),move(96.999999),up]);
add('sticky-out-and-back', [down(100),move(110),move(102),move(100),up]);
add('reverse-drag', [down(100),move(120),move(70),up]);
add('accumulated-drags', [down(100),move(150),up,down(-20),move(-30),move(-40),up,down(800),move(803),up], [.5]);
add('unbounded-yaw', [down(0),move(1000),up,down(0),move(-3000),up]);
add('move-without-down', [move(99),up,up,down(10),up,move(-99),up], [.25]);
add('repeated-down-resets-threshold', [down(0),move(7),down(40),move(42),up], [.75]);
add('absent-view', [down(1),move(3),up,{type:'view',present:true},down(10),up], [.5], {hasView:false});
add('view-disappears-mid-gesture', [down(0),{type:'view',present:false},up,{type:'view',present:true},down(0),up], [.25]);
add('reduced-motion-still-interactive', [down(0),move(30),up,down(40),move(43),up,{type:'frame',time:3},{type:'frame',time:17}], [.75], {motion:false});
add('sway-and-offset', [{type:'frame',time:0},{type:'frame',time:1},down(50),move(75),{type:'frame',time:5},up,{type:'frame',time:12.5},{type:'frame',time:100,motion:false}]);

const rows = cases.map(row => {
  const handlers = new Map(), roles = [], sounds = [], captures = [];
  let randomCalls = 0, hasView = row.hasView ?? true, motion = row.motion ?? true;
  const math = Object.create(Math);
  math.random = () => {
    if (randomCalls >= row.random.length) throw new Error(`Unexpected RNG draw in ${row.name}`);
    return row.random[randomCalls++];
  };
  const view = {play(role){roles.push(role);}};
  const host = {drag:null,yawOff:0,t:0,spin:{rotation:{y:0}},view:hasView?view:null,
    canvas:{addEventListener(name,handler){handlers.set(name,handler);},setPointerCapture(id){captures.push(id);}}};
  install.call(host, math, (...args)=>sounds.push(args));
  const snapshot = () => {
    rotate.call(host,{prefs:{motion}});
    return {yawOffset:host.yawOff,dragging:!!host.drag,hasMoved:!!host.drag?.moved,
      yaw:host.spin.rotation.y,time:host.t,motion,hasView,randomCalls,roles:[...roles],sounds:sounds.map(s=>[...s])};
  };
  const initial = snapshot(), steps = [];
  for (const action of row.actions) {
    if (action.type === 'view') {hasView=action.present;host.view=hasView?view:null;}
    else if (action.type === 'frame') {host.t=action.time;if(action.motion!==undefined)motion=action.motion;}
    else handlers.get('pointer'+action.type)({clientX:action.x,pointerId:7});
    steps.push({action,expected:snapshot()});
  }
  return {name:row.name,random:row.random,initial,steps};
});
const destination = path.join(root,'native/tests/fixtures/pedestal-interaction.json');
fs.writeFileSync(destination,JSON.stringify({schema:1,cases:rows})+'\n');
console.log(`Exported ${rows.length} source pedestal gesture cases.`);
