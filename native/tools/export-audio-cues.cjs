// Run the original renderer's complete event handler; only visuals are stubbed.
const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const root = path.resolve(__dirname, '../..');
const DT = require('../../tools/headless.cjs').load({ strict: true });
new Function('window', fs.readFileSync(path.join(root, 'js/moves.js'), 'utf8').replace(/^export /gm, ''))({ DT });
const source = fs.readFileSync(path.join(root, 'js/render3d.js'), 'utf8');
function section(start, end) {
  const a = source.indexOf(start), b = source.indexOf(end, a);
  if (a < 0 || b < 0) throw Error(`Renderer boundary missing: ${start}`);
  return source.slice(a, b);
}
(async () => {
  const { THREE } = await require('./three-loader.cjs')();
  let calls = [];
  const context = { DT, THREE, Math, COLS: 7, ROWS: 8, sfx(name, options = {}) {
    calls.push(name === 'cast' ? `cast_${options.elem}_${options.stage}` : name === 'hit' ? `hit_${options.big ? 'big' : 'normal'}` : name);
  }, elemColor() { return '#ffffff'; }, GEO: { beam: new THREE.PlaneGeometry(), burst: new THREE.PlaneGeometry(), ball: new THREE.SphereGeometry() } };
  const cell = source.match(/^const cellWorld = .*$/m)[0];
  const methods = [section('  consume(cb) {', '\n  updateFx('), section('  combatCell(', '\n  viewTeam('), section('  viewTeam(', '\n  ownerColor('), section('  unitWorld(', '\n  // ---------- view sync')];
  const factory = vm.runInNewContext(`(()=>{const SQ3=Math.sqrt(3);${cell};return ()=>new(class{${methods.join('\n')}})();})()`, context);
  const traces = JSON.parse(zlib.gunzipSync(fs.readFileSync(path.join(root, 'native/tests/fixtures/scenes.json.gz'))));
  const output = [];
  for (const trace of traces) {
    const [set, pace] = trace.variant.split('/'); DT.Sets.activate(set, pace);
    const entries = side => side.map(u => ({ line: u.line, stage: u.stage, c: u.column, r: u.row, items: u.items }));
    const combat = new DT.Combat({ units: entries(trace.sideA), cols: trace.cols }, { units: entries(trace.sideB), cols: trace.cols }, { seed: trace.seed });
    const renderer = factory(); Object.assign(renderer, { evIdx: 0, celebrated: false, tiles: [], deathT: {}, mirror: false, combatCols: trace.cols,
      setCombatWidth() {}, viewOf() { return null; }, floatText() {}, addMeshFx() {},
      moves: { elemOf(name, line) { return DT.Moves.specFor(name, line).elem; }, begin() {}, target() {}, basic() {}, area() {}, ally() {}, flush() {} } });
    const samples = []; let ticks = 0;
    for (const sample of trace.samples.filter(s => !s.mirror)) {
      while (ticks < sample.ticks) { combat.step(); ticks++; }
      calls = []; renderer.consume(combat); samples.push({ ticks, calls });
    }
    output.push({ variant: trace.variant, sideA: trace.sideA, sideB: trace.sideB, cols: trace.cols, seed: trace.seed, samples });
  }
  fs.writeFileSync(path.join(root, 'native/tests/fixtures/audio-cues.json.gz'), zlib.gzipSync(JSON.stringify(output)));
  console.log(`Exported ${output.length} original-renderer audio traces, ${output.reduce((n,t)=>n+t.samples.reduce((m,s)=>m+s.calls.length,0),0)} cues.`);
})().catch(error => { console.error(error); process.exitCode = 1; });


