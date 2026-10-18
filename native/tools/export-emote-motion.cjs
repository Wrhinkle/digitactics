const fs = require('fs'), path = require('path'), vm = require('vm'), zlib = require('zlib');
const root = path.resolve(__dirname, '../..'), source = fs.readFileSync(path.join(root, 'js/emotes.js'), 'utf8');
const context = { poses: new Map() }; vm.createContext(context);
vm.runInContext(source.slice(source.indexOf('const MOVES ='), source.indexOf('function playRole(')) + '\nthis.moves=Object.keys(MOVES);', context);
const rows = [];
for (const move of context.moves) for (const duration of [1600, 2600]) for (const elapsed of [-1, 0, ...Array.from({ length: 400 }, (_, i) => duration * i / 400), duration - .001, duration, duration + 1]) {
  context.poses.set(0, { move, t0: 100, dur: duration });
  const pose = context.poseNow(0, 100 + elapsed);
  if (pose?.rx && pose.pivot) { pose.y = (pose.y || 0) + pose.pivot - pose.pivot * Math.cos(pose.rx); pose.z = -pose.pivot * Math.sin(pose.rx); }
  rows.push({ move, duration, elapsed, pose });
}
fs.writeFileSync(path.join(root, 'native/tests/fixtures/emote-motion.json.gz'), zlib.gzipSync(JSON.stringify(rows)));
console.log(`Exported ${rows.length} original emote body poses.`);
