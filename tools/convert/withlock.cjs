// Runs a command while holding a repo-wide lock, so parallel agents don't overwrite shared files
// (models/index.json, js/models.js, models/portraits/index.json, rips/index.json, upright.json).
// Usage: node tools/convert/withlock.cjs <command> [args...]   (stale locks older than 20 min are broken)
const fs = require('fs');
const path = require('path');
const { spawnSync } = require('child_process');
const LOCK = path.join(__dirname, '.shared.lock');
const sleep = (ms) => Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
for (;;) {
  try { fs.mkdirSync(LOCK); break; } catch (e) {
    try { if (Date.now() - fs.statSync(LOCK).mtimeMs > 20 * 60e3) { fs.rmSync(LOCK, { recursive: true }); continue; } } catch (_) { /* gone */ }
    sleep(500);
  }
}
let code = 1;
try {
  const [cmd, ...args] = process.argv.slice(2);
  code = spawnSync(cmd, args, { stdio: 'inherit', windowsHide: true}).status;
} finally { fs.rmSync(LOCK, { recursive: true, force: true }); }
process.exit(code == null ? 1 : code);
