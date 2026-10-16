// Exposes window.DTSteam to the game page (contract in js/net-steam.js). Only these calls reach the main process.
const { contextBridge, ipcRenderer } = require('electron');

// page errors with their stacks go to main.log (%APPDATA%\Digi-Tactics\main.log)
const report = (text) => { try { ipcRenderer.send('dtlog:error', String(text).slice(0, 2000)); } catch (e) { /* ignore */ } };
window.addEventListener('error', (ev) => {
  const err = ev.error;
  report(err && err.stack ? err.stack : `${ev.message} at ${ev.filename}:${ev.lineno}:${ev.colno}`);
});
window.addEventListener('unhandledrejection', (ev) => {
  const r = ev.reason;
  report(`unhandled rejection: ${r && r.stack ? r.stack : r}`);
});

const subs = {};
ipcRenderer.on('dtsteam:event', (ev, type, payload) => {
  for (const cb of (subs[type] || []).slice()) { try { cb(payload); } catch (e) { console.error(e); } }
});
const call = (name) => (...args) => ipcRenderer.invoke(`dtsteam:${name}`, ...args);

contextBridge.exposeInMainWorld('DTSteam', {
  init: call('init'),
  persona: call('persona'),
  createLobby: call('createLobby'),
  joinLobby: call('joinLobby'),
  leaveLobby: call('leaveLobby'),
  blockPeer: call('blockPeer'),
  lobbyMembers: call('lobbyMembers'),
  lobbyOwner: call('lobbyOwner'),
  setLobbyOwner: call('setLobbyOwner'),
  setLobbyData: call('setLobbyData'),
  getLobbyData: call('getLobbyData'),
  listLobbies: call('listLobbies'),
  friendLobbies: call('friendLobbies'),
  inviteDialog: call('inviteDialog'),
  launchLobby: call('launchLobby'),
  // fire and forget: game messages do not wait for a reply
  send: (to, text, reliable) => { ipcRenderer.send('dtsteam:send', String(to), String(text), !!reliable); return true; },
  on: (type, cb) => {
    if (typeof cb !== 'function') return () => {};
    (subs[type] = subs[type] || []).push(cb);
    return () => { subs[type] = (subs[type] || []).filter((x) => x !== cb); };
  },
});

// window.DTUpdate: the self-updater (updater.js). state() -> { phase, version, latest, notes, received, total, error, page }
// phase: off | checking | offline | current | downloading | extracting | ready | error | shell (needs a new full download)
// on(type, cb) with type = phase names plus 'available' and 'progress'; restart() applies a ready update (returns false otherwise)
const usubs = {};
ipcRenderer.on('dtupdate:event', (ev, type, st) => {
  for (const cb of (usubs[type] || []).concat(usubs['*'] || [])) { try { cb(st, type); } catch (e) { console.error(e); } }
});
contextBridge.exposeInMainWorld('DTUpdate', {
  state: () => ipcRenderer.invoke('dtupdate:state'),
  restart: () => ipcRenderer.invoke('dtupdate:restart'),
  on: (type, cb) => {
    if (typeof cb !== 'function') return () => {};
    (usubs[type] = usubs[type] || []).push(cb);
    return () => { usubs[type] = (usubs[type] || []).filter((x) => x !== cb); };
  },
});
