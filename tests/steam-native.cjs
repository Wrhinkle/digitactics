// Native test of desktop-steam/steam.js without Steam: builds tests/native/fake_steam_api.dll (MSVC x64 tools), then runs
// tests/native/harness-main.cjs in the shell's Electron, which drives steam.js's koffi paths (message receive, copy and
// release, send, session accept, persona and avatar, friends' lobbies) against the fake DLL. Electron matters: it forbids
// external buffers, and the old koffi.view() call aborted the process there on the first received message.
// Run: node tests/steam-native.cjs        (DT_FAKE_STEAM_DLL=<path> skips the build)
const { spawnSync } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');

const ROOT = path.join(__dirname, '..');
const work = path.join(os.tmpdir(), 'dt-steam-native');
fs.mkdirSync(work, { recursive: true });
let dll = process.env.DT_FAKE_STEAM_DLL;
if (!dll) {
  dll = path.join(work, 'fake_steam_api.dll');
  const src = path.join(__dirname, 'native', 'fake_steam_api.cpp');
  if (!fs.existsSync(dll) || fs.statSync(dll).mtimeMs < fs.statSync(src).mtimeMs) {
    const r = spawnSync('cmd.exe', ['/d', '/c', path.join(__dirname, 'native', 'build.cmd'), work], { windowsHide: true, encoding: 'utf8' });
    if (r.status !== 0) { console.error(`could not build the fake Steam DLL (exit ${r.status}):\n${r.stdout}${r.stderr}`); process.exit(r.status === 2 ? 0 : 1); }
  }
}
const out = path.join(work, 'report.json');
fs.rmSync(out, { force: true });
const electron = path.join(ROOT, 'desktop-steam', 'node_modules', 'electron', 'dist', 'electron.exe');
const r = spawnSync(electron, [path.join(__dirname, 'native', 'harness-main.cjs')], {
  windowsHide: true, timeout: 60000, encoding: 'utf8',
  env: { ...process.env, DT_NATIVE_DEBUG: '1', DT_NATIVE_OUT: out, DT_NATIVE_DLL: dll, ELECTRON_ENABLE_LOGGING: '' },
});
if (!fs.existsSync(out)) {
  console.error(`FAIL Electron exited (code ${r.status}) without a report: the native layer crashed`);
  console.error(String(r.stderr || '').split('\n').slice(0, 12).join('\n'));
  process.exit(1);
}
const rep = JSON.parse(fs.readFileSync(out, 'utf8'));
for (const x of rep.results) console.log(`${x.ok ? 'ok  ' : 'FAIL'} ${x.msg}`);
if (rep.stats) console.log(`fake Steam counters: ${JSON.stringify(rep.stats)}`);
const failed = rep.results.filter((x) => !x.ok);
if (r.status !== 0) failed.push({ msg: `Electron exit code ${r.status}` });
if (failed.length) {
  console.log(`\n${failed.length} FAILED; last log lines:\n${(rep.log || []).slice(-20).join('\n')}`);
  process.exit(1);
}
console.log(`\nall native checks passed (Electron exit ${r.status})`);
