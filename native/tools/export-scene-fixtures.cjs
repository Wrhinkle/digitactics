// Execute the original renderer methods with rendering/DOM work stubbed out.
// The reference math stays in render3d.js; no hand-copied positioning formula.
const fs = require('node:fs');
const path = require('node:path');
const zlib = require('node:zlib');
const vm = require('node:vm');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const source = fs.readFileSync(path.join(__dirname, '../../js/render3d.js'), 'utf8');
function method(name, next) {
  const start = source.indexOf(`  ${name}(`);
  const end = source.indexOf(next, start);
  if (start < 0 || end < 0) throw new Error(`Renderer source boundary missing: ${name}`);
  return source.slice(start, end).trim();
}
const cellWorld = source.match(/^const cellWorld = .*$/m)?.[0];
if (!cellWorld) throw new Error('Missing original cellWorld');
const methods = [method('combatCell', '\n  viewTeam('), method('viewTeam', '\n  ownerColor('),
  method('unitWorld', '\n  // ---------- view sync'), method('drawCombat', '\n  updateViews(')];
const renderer = vm.runInNewContext(`(() => { const COLS = 7, ROWS = 8, SQ3 = Math.sqrt(3); ${cellWorld}
  return { ${methods.join(',')} }; })()`);
const traces = [];
for (const set of DT.Sets.pickable()) for (const cols of [7, 14]) {
  const pace = cols === 7 ? 'hyper' : 'normal'; DT.Sets.activate(set.id, pace);
  const rng = DT.makeRng(82100 + traces.length);
  const make = () => Array.from({length: 8}, (_, i) => ({line: DT.SHOP_LINES[Math.floor(rng() * DT.SHOP_LINES.length)].id,
    stage: i % 3, c: i % cols, r: 4 + Math.floor(i / cols), items: []}));
  const a = make(), b = make(), seed = 66100 + traces.length;
  const cb = new DT.Combat({units: a, cols}, {units: b, cols}, {seed});
  const samples = []; let ticks = 0, eventCursor = 0;
  renderer.deathT = {}; renderer.cbSerial = 1;
  Object.assign(renderer, {setCombatWidth(c) { this.combatCols = c; }, tick() { return 1 / 30; },
    benchDesired() {}, dragDesired() {}, drawExtras() {}, drawDuoExtras() {}, highlight() {},
    updateFx() {}, updateViews() {}, render() {}, ult: {update() {}}, moves: {update() {}},
    otLabel: {classList: {add() {}, remove() {}}}, sync(out) { this.output = out; }});
  function capture() {
    while (eventCursor < cb.events.length) { const e = cb.events[eventCursor++]; if (e.type === 'death') renderer.deathT[e.at] = e.t; }
    for (const mirror of [false, true]) {
      renderer.mirror = mirror; const hover = ticks % cb.units.length;
      renderer.drawCombat({human: {}}, {hoverCombatId: hover}, cb);
      samples.push({ticks, mirror, hover, views: renderer.output.map(d => ({unitId: d.cu.id,
        position: {x: d.x, y: d.y, z: d.z}, yaw: d.yaw, team: d.team, dying: d.dying, hover: d.hover}))});
    }
  }
  capture();
  while (!cb.done && ticks < 2000) { cb.step(); ticks++; if (ticks % 5 === 0 || cb.done) capture(); }
  if (!cb.done) throw new Error('Unfinished scene battle');
  const convert = es => es.map(u => ({line: u.line, stage: u.stage, column: u.c, row: u.r, items: u.items}));
  traces.push({variant: `${set.id}/${pace}`, cols, seed, sideA: convert(a), sideB: convert(b), samples});
}
if (!traces.some(t => t.samples.some(s => s.views.some(v => v.position.y > 0)))) throw new Error('No moving scene coverage');
if (!traces.some(t => t.samples.some(s => s.views.some(v => v.dying > 0)))) throw new Error('No death fade coverage');
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/scenes.json.gz'), zlib.gzipSync(JSON.stringify(traces)));
console.log(`Exported ${traces.length} original-renderer scene traces.`);
