// Shell entry point of the Steam desktop build. It stays fixed between updates, together with the Electron runtime,
// the Steam DLLs and node_modules (steamworks.js, koffi). Everything else (main.js, preload.js, steam.js, the updater
// and the whole game folder) is the "payload", which the self-updater replaces.
//
// Payload locations:
//   bundled:  resources/payload                 (shipped in the build; desktop-steam/payload when run from source)
//   updated:  %APPDATA%\Digi-Tactics\payload\app  (written by updater.js; app.old and staging sit next to it)
// On start, with the single-instance lock held:
//   1. trial.json left over = the last new payload never reported a healthy start: roll back (app.old -> app).
//   2. staging/ ready and newer: swap atomically (app -> app.old, staging -> app) and write trial.json.
//   3. run the newest payload this shell can run (minShell <= SHELL), else the bundled one.
// The payload calls DTBoot.confirm() once the game page is up; that deletes trial.json and app.old. A trial that
// throws while loading, or does not confirm within 90 s, is rolled back and the game restarts on the previous payload.
// Bump SHELL whenever this file, Electron, steamworks.js, koffi or the Steam DLLs change, and set "minShell" in
// version.json to match if the payload depends on it; older shells then show "download the new full build".
const { app } = require('electron');
const path = require('path');
const fs = require('fs');

const SHELL = 2;
if (process.env.DT_USERDATA) app.setPath('userData', path.resolve(process.env.DT_USERDATA));
const UD = app.getPath('userData');
const P = path.join(UD, 'payload');
const DIRS = { app: path.join(P, 'app'), old: path.join(P, 'app.old'), staging: path.join(P, 'staging'), trial: path.join(P, 'trial.json'), bad: path.join(P, 'bad.json') };
const BUNDLED = app.isPackaged ? path.join(process.resourcesPath, 'payload') : path.join(__dirname, 'payload');
// from source the updater stays off unless a test points DT_USERDATA somewhere
const UPDATES_ON = app.isPackaged || !!process.env.DT_USERDATA;
const TRIAL_MS = Number(process.env.DT_TRIAL_TIMEOUT_MS) || 90000;

function log(msg) {
  try { fs.mkdirSync(UD, { recursive: true }); fs.appendFileSync(path.join(UD, 'main.log'), `${new Date().toISOString()} ${msg}\n`); } catch (e) { /* no log dir */ }
}
const sleep = (ms) => Atomics.wait(new Int32Array(new SharedArrayBuffer(4)), 0, 0, ms);
const readJson = (f) => { try { return JSON.parse(fs.readFileSync(f, 'utf8')); } catch (e) { return null; } };
function readVer(dir) {
  const v = readJson(path.join(dir, 'version.json'));
  if (!v || typeof v.version !== 'string' || !fs.existsSync(path.join(dir, 'main.js'))) return null;
  return { version: v.version, minShell: Number(v.minShell) || 1, ...v };
}
function cmpVer(a, b) {
  const pa = String(a).split('.').map((x) => parseInt(x, 10) || 0);
  const pb = String(b).split('.').map((x) => parseInt(x, 10) || 0);
  for (let i = 0; i < Math.max(pa.length, pb.length); i++) { const d = (pa[i] || 0) - (pb[i] || 0); if (d) return d < 0 ? -1 : 1; }
  return 0;
}
// a renamed folder can stay locked for a moment while the previous process exits
function retry(fn, what) {
  for (let i = 0; ; i++) {
    try { return fn(); } catch (e) {
      if (i >= 40 || !['EPERM', 'EBUSY', 'EACCES', 'ENOTEMPTY'].includes(e.code)) { log(`boot: ${what} failed: ${e.message}`); throw e; }
      sleep(250);
    }
  }
}
const rm = (p) => { if (fs.existsSync(p)) retry(() => fs.rmSync(p, { recursive: true, force: true }), `remove ${p}`); };
const mv = (a, b) => retry(() => fs.renameSync(a, b), `rename ${a} -> ${b}`);
const badList = () => { const b = readJson(DIRS.bad); return Array.isArray(b && b.versions) ? b.versions : []; };
function markBad(version) {
  if (!version) return;
  const list = badList();
  if (!list.includes(version)) list.push(version);
  try { fs.writeFileSync(DIRS.bad, JSON.stringify({ versions: list.slice(-20) })); } catch (e) { /* ignore */ }
}

function rollback(reason) {
  const t = readJson(DIRS.trial) || {};
  log(`boot: rolling back ${t.version || '?'} (${reason})`);
  markBad(t.version);
  try {
    if (fs.existsSync(DIRS.app)) { const dead = path.join(P, 'app.bad'); rm(dead); mv(DIRS.app, dead); rm(dead); }
    if (fs.existsSync(DIRS.old)) mv(DIRS.old, DIRS.app);
  } catch (e) { log(`boot: rollback error ${e.message}`); }
  try { fs.rmSync(DIRS.trial, { force: true }); } catch (e) { /* ignore */ }
}

function relaunch() {
  try { app.releaseSingleInstanceLock(); } catch (e) { /* ignore */ }
  const opts = { args: process.defaultApp ? [app.getAppPath(), '--dt-relaunch'] : ['--dt-relaunch'] };
  if (process.env.PORTABLE_EXECUTABLE_FILE) opts.execPath = process.env.PORTABLE_EXECUTABLE_FILE; // single-file build: start the wrapper again
  app.relaunch(opts);
  app.exit(0);
}

// ---- single instance first: a second launch must never touch the payload folders ----
let locked = app.requestSingleInstanceLock();
if (!locked && process.argv.includes('--dt-relaunch')) {
  for (let i = 0; i < 40 && !locked; i++) { sleep(250); locked = app.requestSingleInstanceLock(); }
}
if (!locked) { app.quit(); return; }

let trial = false;
let dir = BUNDLED;
const bundled = readVer(BUNDLED);
if (UPDATES_ON) {
  try {
    fs.mkdirSync(P, { recursive: true });
    if (fs.existsSync(DIRS.trial)) rollback('the new version did not start');
    const cur = readVer(DIRS.app);
    const effective = cur && cur.minShell <= SHELL && (!bundled || cmpVer(cur.version, bundled.version) > 0) ? cur : bundled;
    const st = fs.existsSync(DIRS.staging) ? readVer(DIRS.staging) : null;
    if (st && st.minShell <= SHELL && !badList().includes(st.version) && (!effective || cmpVer(st.version, effective.version) > 0)) {
      log(`boot: applying ${st.version} (was ${effective ? effective.version : '?'})`);
      rm(DIRS.old);
      const hadApp = fs.existsSync(DIRS.app) && cur;
      if (fs.existsSync(DIRS.app)) mv(DIRS.app, DIRS.old);
      try { mv(DIRS.staging, DIRS.app); } catch (e) { if (hadApp) mv(DIRS.old, DIRS.app); throw e; }
      fs.writeFileSync(DIRS.trial, JSON.stringify({ version: st.version, from: effective ? effective.version : null, at: new Date().toISOString() }));
    } else if (fs.existsSync(DIRS.staging)) {
      rm(DIRS.staging);
    }
    if (!fs.existsSync(DIRS.trial) && fs.existsSync(DIRS.old)) rm(DIRS.old); // left from an update that started fine
    const now = readVer(DIRS.app);
    if (now && now.minShell <= SHELL && (!bundled || cmpVer(now.version, bundled.version) > 0)) {
      dir = DIRS.app;
      trial = fs.existsSync(DIRS.trial);
    } else {
      // a newer full build replaced the folder: the downloaded payload is out of date
      if (now) { log(`boot: bundled ${bundled && bundled.version} is newer than downloaded ${now.version}, using bundled`); rm(DIRS.app); rm(DIRS.old); }
      try { fs.rmSync(DIRS.trial, { force: true }); } catch (e) { /* ignore */ }
    }
  } catch (e) {
    log(`boot: payload check failed, using bundled: ${e.stack || e}`);
    dir = BUNDLED;
    trial = false;
  }
}

let confirmed = !trial;
let watchdog = null;
const ver = readVer(dir) || { version: '0.0.0', minShell: 1 };
global.DTBoot = {
  shell: SHELL,
  version: ver.version,
  info: ver,
  payloadDir: dir,
  bundledDir: BUNDLED,
  dirs: DIRS,
  userData: UD,
  updatesOn: UPDATES_ON,
  trial,
  require, // the shell's require: steamworks.js and koffi live in the shell's node_modules
  cmpVer,
  badList,
  log,
  relaunch,
  confirm() {
    if (confirmed) return;
    confirmed = true;
    if (watchdog) clearTimeout(watchdog);
    try { fs.rmSync(DIRS.trial, { force: true }); } catch (e) { /* ignore */ }
    log(`boot: ${ver.version} started fine`);
    fs.promises.rm(DIRS.old, { recursive: true, force: true }).catch((e) => log(`boot: could not remove app.old: ${e.message}`));
  },
};
if (trial) {
  log(`boot: trial start of ${ver.version}`);
  watchdog = setTimeout(() => { if (!confirmed) { rollback(`no healthy start within ${TRIAL_MS / 1000} s`); relaunch(); } }, TRIAL_MS);
}

try {
  require(path.join(dir, 'main.js'));
} catch (e) {
  log(`boot: payload ${ver.version} failed to load: ${e.stack || e}`);
  if (trial) { rollback('load error'); relaunch(); } else if (dir !== BUNDLED) { markBad(ver.version); rm(DIRS.app); relaunch(); } else throw e;
}
