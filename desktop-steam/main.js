// Digi-Tactics, Steam desktop build (Electron). A standalone game window: solo works offline; multiplayer uses Steam
// (App ID 480, Spacewar) for lobbies, invites, names and relayed P2P messages, so the Steam client must be running.
// This file is part of the updatable payload (see boot.js): the game files are served from the payload's game folder
// over the app:// scheme, and updater.js fetches new payloads. No console window; the log is %APPDATA%\Digi-Tactics\main.log.
const { app, BrowserWindow, ipcMain, protocol, net, shell, Menu, crashReporter } = require('electron');
const path = require('path');
const fs = require('fs');
const { pathToFileURL } = require('url');
const steam = require('./steam');
const { createUpdater } = require('./updater');

// boot.js sets DTBoot; the fallback lets main.js run on its own (electron main.js) with updates off
const BOOT = global.DTBoot || { shell: 1, version: '0.0.0', userData: app.getPath('userData'), updatesOn: false, trial: false, require, cmpVer: () => 0, badList: () => [], dirs: {}, confirm() {}, relaunch() { app.relaunch(); app.exit(0); } };
const GAME_DIR = path.join(__dirname, 'game');
const ICON = path.join(GAME_DIR, 'icons', 'icon-512.png');
let logFile = null;
let logBytes = null;
function log(msg) {
  try {
    if (!logFile) logFile = path.join(app.getPath('userData'), 'main.log');
    if (logBytes == null) logBytes = fs.existsSync(logFile) ? fs.statSync(logFile).size : 0;
    if (logBytes > 5 * 1024 * 1024) {
      fs.copyFileSync(logFile, logFile + '.1');
      fs.writeFileSync(logFile, ''); logBytes = 0;
    }
    const line = `${new Date().toISOString()} ${String(msg).slice(0, 4000)}\n`;
    fs.appendFileSync(logFile, line); logBytes += Buffer.byteLength(line);
  } catch (e) { /* no log dir */ }
}
process.on('uncaughtException', (e) => log(`uncaught: ${e.stack || e}`));
process.on('unhandledRejection', (e) => log(`unhandled rejection: ${(e && e.stack) || e}`));
// Native crashes leave no JS trace: minidumps stay on this PC (never uploaded) in the folder logged here, and a clean
// exit always logs "exit", so a start line with no exit line before it means the previous run died.
try {
  crashReporter.start({ uploadToServer: false, compress: true });
  log(`crash dumps: ${app.getPath('crashDumps')}`);
} catch (e) { log(`crash reporter: ${e.message}`); }
process.on('exit', (code) => log(`exit ${code}`));
app.on('render-process-gone', (e, wc, d) => log(`renderer gone: ${d.reason} (exit ${d.exitCode})`));
app.on('child-process-gone', (e, d) => log(`child process gone: ${d.type}${d.serviceName ? ` ${d.serviceName}` : ''} ${d.reason} (exit ${d.exitCode})`));

// one window; a second launch (for example with +connect_lobby) hands its arguments to the first
if (!app.requestSingleInstanceLock()) { app.quit(); return; }

// "+connect_lobby <id>" on the command line: Steam's way of starting a game into a lobby
function lobbyArg(argv) {
  const i = argv.indexOf('+connect_lobby');
  return i >= 0 && steam.isId(argv[i + 1]) ? argv[i + 1] : null;
}
let launchLobby = lobbyArg(process.argv);
// smoke test (desktop-steam/smoke.cjs): no visible window; load the game, check Steam, create and leave a lobby, write a report, quit
const SMOKE = process.env.DT_SMOKE || null;
// updater test (tests/update.cjs): no visible window; writes JSON lines to this file, restarts as soon as an update is
// ready and quits once the check has nothing more to do
const UPDATE_TEST = process.env.DT_UPDATE_TEST || null;
const testNote = (o) => { if (UPDATE_TEST) try { fs.appendFileSync(UPDATE_TEST, `${JSON.stringify({ t: Date.now(), pid: process.pid, ...o })}
`); } catch (e) { /* ignore */ } };
testNote({ event: 'boot', version: BOOT.version, trial: BOOT.trial });
let win = null;
const emit = (type, payload) => { if (win && !win.isDestroyed()) win.webContents.send('dtsteam:event', type, payload); };
const updater = createUpdater({ boot: BOOT, log, emit: (type, st) => {
  testNote({ event: type, phase: st.phase, latest: st.latest, error: st.error });
  if (win && !win.isDestroyed()) win.webContents.send('dtupdate:event', type, st);
} });
function restartForUpdate() {
  log(`update: restarting to apply ${updater.state().latest || ''}`);
  try { steam.shutdown(); } catch (e) { /* ignore */ }
  BOOT.relaunch();
}

// Steam first: the overlay needs its GPU switches before the app is ready. Without Steam the game still runs (solo).
const steamInfo = steam.init({ emit, log });
if (steamInfo.ok) BOOT.require('steamworks.js').electronEnableSteamOverlay();
app.commandLine.appendSwitch('force_high_performance_gpu'); // laptops: use the discrete GPU for the 3D board

protocol.registerSchemesAsPrivileged([{ scheme: 'app', privileges: { standard: true, secure: true, supportFetchAPI: true, corsEnabled: true, stream: true } }]);

app.on('second-instance', (ev, argv) => {
  const id = lobbyArg(argv);
  if (id) emit('joinRequested', { lobbyId: id, friendId: '' });
  if (win) { if (win.isMinimized()) win.restore(); win.focus(); }
});

// ---- the DTSteam bridge (see js/net-steam.js for the contract) ----
const str = (v) => (typeof v === 'string' ? v : '');
const trusted = (ev) => !!(win && ev.sender === win.webContents && ev.senderFrame === win.webContents.mainFrame &&
  /^app:\/\/game\/index\.html(?:[?#]|$)/.test(ev.senderFrame.url));
const handlers = {
  init: () => steam.info(),
  persona: (id) => (steamInfo.ok ? steam.persona(str(id)) : { name: '', avatar: null }),
  createLobby: (type, max) => steam.createLobby(str(type), Number(max) || 8),
  joinLobby: (id) => steam.joinLobby(str(id)),
  leaveLobby: (id) => steam.leaveLobby(str(id)),
  blockPeer: (id) => steam.blockPeer(str(id)),
  lobbyMembers: (id) => steam.lobbyMembers(str(id)),
  lobbyOwner: (id) => steam.lobbyOwner(str(id)),
  setLobbyOwner: (id, owner) => steam.setLobbyOwner(str(id), str(owner)),
  setLobbyData: (id, data) => steam.setLobbyData(str(id), data),
  getLobbyData: (id) => steam.getLobbyData(str(id)),
  listLobbies: (filters) => steam.listLobbies(filters && typeof filters === 'object' ? filters : {}),
  friendLobbies: () => steam.friendLobbies(),
  inviteDialog: (id) => steam.inviteDialog(str(id)),
  send: (to, text, reliable) => steam.send(str(to), str(text), !!reliable),
  launchLobby: () => { const id = launchLobby; launchLobby = null; return id; },
};
for (const [name, fn] of Object.entries(handlers)) {
  ipcMain.handle(`dtsteam:${name}`, async (ev, ...args) => {
    if (!trusted(ev)) return null;
    if (name !== 'init' && name !== 'launchLobby' && !steamInfo.ok) return name === 'joinLobby' ? { ok: false, error: steam.NOT_RUNNING } : null;
    try { return await fn(...args); } catch (e) { log(`${name}: ${e.stack || e}`); return name === 'joinLobby' ? { ok: false, error: e.message } : null; }
  });
}
// ---- the DTUpdate bridge (preload.js): version, update state and Restart ----
ipcMain.handle('dtupdate:state', (ev) => (trusted(ev) ? { ...updater.state(), trial: BOOT.trial } : null));
ipcMain.handle('dtupdate:restart', (ev) => {
  if (!trusted(ev) || updater.state().phase !== 'ready') return false;
  setTimeout(restartForUpdate, 100);
  return true;
});
ipcMain.on('dtsteam:send', (ev, to, text, reliable) => { if (steamInfo.ok && trusted(ev)) steam.send(str(to), str(text), !!reliable); });
// page errors (preload.js forwards window errors and unhandled rejections), capped so a loop cannot flood the log
let pageErrors = 0;
ipcMain.on('dtlog:error', (ev, text) => {
  if (!trusted(ev) || ++pageErrors > 200) return;
  log(`page error: ${str(text).slice(0, 2000)}${pageErrors === 200 ? ' (further page errors not logged)' : ''}`);
});

function createWindow() {
  win = new BrowserWindow({
    title: 'Digi-Tactics',
    icon: ICON,
    width: 1500,
    height: 940,
    minWidth: 960,
    minHeight: 640,
    backgroundColor: '#0d1017',
    show: false,
    autoHideMenuBar: true,
    skipTaskbar: !!(SMOKE || UPDATE_TEST),
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      sandbox: true,
      nodeIntegration: false,
      backgroundThrottling: false, // the host keeps ordering moves while its window is minimised
    },
  });
  Menu.setApplicationMenu(null);
  win.webContents.session.setPermissionRequestHandler((_wc, _permission, cb) => cb(false));
  win.webContents.session.setPermissionCheckHandler(() => false);
  win.webContents.on('will-attach-webview', (e) => e.preventDefault());
  if (!SMOKE && !UPDATE_TEST) win.once('ready-to-show', () => win.show());
  if (SMOKE) win.webContents.once('did-finish-load', runSmoke);
  win.webContents.once('did-finish-load', afterLoad);
  win.on('page-title-updated', (e) => e.preventDefault()); // keep "Digi-Tactics"
  // links (model credits) open in the browser; the window itself never navigates away
  win.webContents.setWindowOpenHandler(({ url }) => { if (/^https?:\/\//.test(url)) shell.openExternal(url); return { action: 'deny' }; });
  win.webContents.on('will-navigate', (e) => e.preventDefault());
  win.webContents.on('will-frame-navigate', (e) => e.preventDefault());
  win.webContents.on('unresponsive', () => log('page unresponsive'));
  win.webContents.on('responsive', () => log('page responsive again'));
  // Electron 35+ passes the details on the event (level 'warning'|'error'); older versions as arguments (2 = warning)
  win.webContents.on('console-message', (e, level, message) => {
    const lv = e && typeof e.level === 'string' ? e.level : level >= 3 ? 'error' : level >= 2 ? 'warning' : 'info';
    const msg = e && typeof e.message === 'string' ? e.message : message;
    if (lv === 'warning' || lv === 'error') log(`page${lv === 'error' ? ' error' : ''}: ${String(msg).slice(0, 2000)}`);
  });
  win.on('closed', () => { win = null; });
  win.loadURL('app://game/index.html');
}

// Once the game page is up: tell boot.js this payload starts fine (ends a trial after an update), then look for updates.
async function afterLoad() {
  let ok = false;
  try {
    ok = await win.webContents.executeJavaScript(`new Promise((res) => {
      const t0 = Date.now();
      const wait = () => (window.DT && window.DT.UI) ? res(true) : Date.now() - t0 > 60000 ? res(false) : setTimeout(wait, 200);
      wait();
    })`);
  } catch (e) { log(`health check: ${e.message}`); }
  testNote({ event: 'start', version: BOOT.version, trial: BOOT.trial, healthy: ok, payloadDir: __dirname });
  if (!ok) { log(`payload ${BOOT.version}: the game page did not come up`); return; }
  BOOT.confirm();
  if (SMOKE) return;
  const st = await updater.check();
  if (UPDATE_TEST) {
    testNote({ event: 'done', phase: st.phase });
    if (st.phase === 'ready') setTimeout(restartForUpdate, 300);
    else setTimeout(() => { steam.shutdown(); app.quit(); }, 300);
  }
}

async function runSmoke() {
  const report = { steam: steam.info(), native: steam.selfTest(), appIdFile: fs.existsSync(path.join(path.dirname(process.execPath), 'steam_appid.txt')) };
  try {
    report.security = await win.webContents.executeJavaScript(`(async () => {
      const csp = (await fetch('index.html')).headers.get('content-security-policy') || '';
      const probe = document.createElement('script'); probe.textContent = 'window.__inlineSecurityProbe = true';
      document.head.append(probe); probe.remove();
      return { csp: csp.includes("script-src 'self'") && csp.includes("frame-src 'none'"), inlineBlocked: !window.__inlineSecurityProbe, noNode: typeof window.require === 'undefined' };
    })()`);
    report.page = await win.webContents.executeJavaScript(`new Promise((res) => {
      const t0 = Date.now();
      const wait = () => (window.DT && window.DT.UI && window.DT.Net) || Date.now() - t0 > 30000 ? res({ title: document.title, transport: window.DT && window.DT.Net && window.DT.Net.transport, bridge: !!window.DTSteam, solo: (() => { try { window.DT.UI.newGame(); return !!(window.__digi && window.__digi.game); } catch (e) { return String(e); } })() }) : setTimeout(wait, 200);
      wait();
    })`);
    if (report.steam.ok) {
      report.lobby = await win.webContents.executeJavaScript(`(async () => {
        const id = await window.DTSteam.createLobby('friends', 8);
        if (!id) return { created: false };
        const set = await window.DTSteam.setLobbyData(id, { dt_game: 'digitactics', dt_ver: 'smoke', name: 'smoke test' });
        const data = await window.DTSteam.getLobbyData(id);
        const members = await window.DTSteam.lobbyMembers(id);
        const owner = await window.DTSteam.lobbyOwner(id);
        const persona = await window.DTSteam.persona(owner);
        await window.DTSteam.leaveLobby(id);
        return { created: true, id, set, name: data.name, members, owner, personaName: persona.name, avatar: !!persona.avatar };
      })()`);
    }
  } catch (e) { report.error = String(e.stack || e); }
  fs.writeFileSync(SMOKE, JSON.stringify(report, null, 2));
  app.quit();
}

app.whenReady().then(() => {
  protocol.handle('app', async (req) => {
    const u = new URL(req.url);
    const rel = decodeURIComponent(u.pathname).replace(/^\/+/, '') || 'index.html';
    const file = path.normalize(path.join(GAME_DIR, rel));
    if (u.host !== 'game' || !file.startsWith(GAME_DIR + path.sep)) return new Response('forbidden', { status: 403 });
    const response = await net.fetch(pathToFileURL(file).toString());
    const headers = new Headers(response.headers);
    headers.set('Content-Security-Policy', "default-src 'self'; script-src 'self' 'wasm-unsafe-eval'; style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; font-src 'self' https://fonts.gstatic.com; img-src 'self' data: blob:; media-src 'self' blob:; connect-src 'self' data: blob:; worker-src 'self' blob:; frame-src 'none'; object-src 'none'; base-uri 'none'; form-action 'none'");
    return new Response(response.body, { status: response.status, headers });
  });
  log(`start ${BOOT.version} (shell ${BOOT.shell}${BOOT.trial ? ', first start after update' : ''}) steam=${steamInfo.ok ? steamInfo.steamId : steamInfo.error}${launchLobby ? ` lobby=${launchLobby}` : ''}`);
  createWindow();
});

app.on('window-all-closed', () => { log('window closed, quitting'); steam.shutdown(); app.quit(); });
