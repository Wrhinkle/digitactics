// Starts Digi-Tactics (Steam build) without a visible window, lets it load the game, check Steam (App ID 480) and,
// when Steam is running, create and leave a friends-only lobby. Prints the report it writes.
// Run: node smoke.cjs [path-to-Digi-Tactics.exe]   (default: ../dist/DigiTactics-Steam/Digi-Tactics.exe; "dev" runs electron .)
const { spawn, execFileSync } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');
const arg = process.argv[2] || path.join(__dirname, '..', 'dist', 'DigiTactics-Steam', 'Digi-Tactics.exe');
const report = path.join(os.tmpdir(), `dt-smoke-${Date.now()}.json`);
let steamRunning = false;
try { steamRunning = /steam\.exe/i.test(execFileSync('tasklist', ['/FI', 'IMAGENAME eq steam.exe', '/NH'], { windowsHide: true }).toString()); } catch (e) { /* no tasklist */ }
console.log('steam.exe running:', steamRunning);
const [cmd, args, cwd] = arg === 'dev' ? [require('electron'), ['.'], __dirname] : [arg, [], path.dirname(arg)];
const child = spawn(cmd, args, { cwd, env: { ...process.env, DT_SMOKE: report }, windowsHide: true, stdio: 'ignore' });
const t0 = Date.now();
const timer = setInterval(() => {
  if (fs.existsSync(report)) {
    clearInterval(timer);
    const r = JSON.parse(fs.readFileSync(report, 'utf8'));
    console.log(JSON.stringify(r, null, 2));
    fs.rmSync(report, { force: true });
    setTimeout(() => process.exit(r.error || (r.page && r.page.solo !== true) || !r.security || Object.values(r.security).some((v) => v !== true) ? 1 : 0), 500);
  } else if (Date.now() - t0 > 90000) {
    clearInterval(timer);
    console.error('no report after 90 s');
    child.kill();
    process.exit(1);
  }
}, 500);
