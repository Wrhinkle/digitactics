const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const root = path.resolve(__dirname, '../..'), source = fs.readFileSync(path.join(root, 'js/render3d.js'), 'utf8');
const context = { LEGEND_SPEED: 6, THREE: { MathUtils: { clamp: (x, a, b) => Math.max(a, Math.min(b, x)) } } };
vm.createContext(context);
vm.runInContext('this.renderer=new class {' + source.slice(source.indexOf('  moveLegend(x, y)'), source.indexOf('  legendPos()')) + '};', context);
let hop = 0;
const legend = { x: -7.6, z: 8.4, tx: null, tz: null, moving: false, view: { yaw: Math.PI, hasRun: false, setMoving() {}, update() {}, group: { position: { set(x, y, z) { hop = y; } } } } };
const renderer = context.renderer; renderer.time = 0; renderer.ensureLegend = () => legend; renderer.groundAt = (x, z) => ({ x, z }); renderer.spawnClickMark = () => {};
const rows = [];
for (let i = 0; i < 2000; i++) {
  const target = i % 11 === 0 ? { x: (i * 13 % 41) - 20, z: (i * 7 % 27) - 8 } : null;
  const delta = [0, 1 / 60, .05, .1, .25, 2][i % 6]; legend.view.hasRun = i % 3 === 0;
  if (target) renderer.moveLegend(target.x, target.z);
  renderer.time += delta; renderer.updateLegend(delta);
  rows.push({ target, delta, hasRun: legend.view.hasRun, expected: { x: legend.x, z: legend.z, tx: legend.tx, tz: legend.tz, moving: legend.moving, yaw: legend.view.yaw, hop } });
}
fs.writeFileSync(path.join(root, 'native/tests/fixtures/companion-travel.json.gz'), zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} original companion movement steps.`);
