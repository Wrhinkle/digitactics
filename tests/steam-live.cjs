// Live native check against the real Steam client with one account (Steam must already be running; this script never
// starts it). Runs tests/native/live-main.cjs in the shell's Electron: lobby create, lobby data, persona and avatar for
// yourself and a friend, empty receive, a message to yourself, friends' lobbies, lobby list. Prints the native trace.
// Run: node tests/steam-live.cjs
const { spawnSync, execFileSync } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');

let running = false;
try { running = /steam\.exe/i.test(execFileSync('tasklist', ['/FI', 'IMAGENAME eq steam.exe', '/NH'], { windowsHide: true }).toString()); } catch (e) { /* no tasklist */ }
if (!running) { console.log('Steam is not running: start it yourself, then rerun. Nothing tested.'); process.exit(0); }
const out = path.join(os.tmpdir(), `dt-steam-live-${Date.now()}.json`);
const electron = path.join(__dirname, '..', 'desktop-steam', 'node_modules', 'electron', 'dist', 'electron.exe');
const r = spawnSync(electron, [path.join(__dirname, 'native', 'live-main.cjs')], {
  windowsHide: true, timeout: 90000, encoding: 'utf8', cwd: path.join(__dirname, '..', 'desktop-steam'),
  env: { ...process.env, DT_LIVE_OUT: out, SteamAppId: '480' },
});
if (!fs.existsSync(out)) {
  console.error(`FAIL Electron exited (code ${r.status}) without a report: the native layer crashed`);
  console.error(String(r.stderr || '').split('\n').slice(0, 12).join('\n'));
  process.exit(1);
}
const rep = JSON.parse(fs.readFileSync(out, 'utf8'));
console.log(rep.log.join('\n'));
console.log('');
for (const x of rep.results) console.log(`${x.ok ? 'ok  ' : 'FAIL'} ${x.msg}`);
process.exit(rep.results.every((x) => x.ok) ? 0 : 1);
