// Multiplayer lobbies, seats, presence and host duties. The data goes through a transport:
//   js/net-firebase.js  the web version: Firebase Realtime Database, anonymous sign-in
//   js/net-steam.js     the Steam desktop build (window.DTSteam from desktop-steam/preload.js): Steam lobbies + Steam
//                       Networking Messages, with the host holding the authoritative lobby record and log
//
// The set (ruleset) a lobby plays: the host picks it (DT.SetPicker); it is stored as lobby.setId and, at the start,
// resolved ('chaos' -> a set from the seed) into init.setId, which every client builds the game from. Firebase rules
// that reject the setId field on lobbies fall back to the host's presence (lobbySet). Lobbies and games without a set
// play the default set (Faction Wars).
// The pace (Hyper Roll 'hyper' or standard 'normal', or 'chaos' = picked from the seed) works the same way: the host
// picks it, it is stored as lobby.pace (presence lobbyPace when the database refuses the field) and resolved into
// init.pace. Lobbies without a pace play Hyper Roll.
//
// Every seat starts as an AI Tamer. Taking a seat turns that Tamer into you (mid-game too, if it is still alive).
// The host orders intents into the log and runs DT.Lock.decide; if the host drops, the next seated player takes over.
// Seats whose player is offline for 20 s go back to the AI.
//
// Transport interface (both transports implement every member; ids are strings):
//   kind 'firebase'|'steam', uid (my player id after connect), personaName (account name or null), now() shared clock ms
//   connect()                                  sign in
//   lobbies:   watchLobbies(cb(rows)) -> off    open lobbies in display order; row = { id, ...lobby record, friend? }
//              createLobby(rec, setId, opts) -> { id, setStored }   opts.visibility 'public'|'friends' (Steam)
//              getLobby(id) -> record|null       (Steam: joins the Steam lobby and waits for the host's snapshot)
//              watchLobby(id, cb(record|null)) -> off,  updateLobby(id, patch),  removeLobby(id)
//              claimSeat(id, i, {uid, name}) -> bool (only if free),  freeSeat(id, i)
//   presence:  joinPresence(id, {name, look, lobbySet?}),  updatePresence(id, patch),  leavePresence(id),
//              watchPresence(id, cb({uid: rec})) -> off      (a player is online while their presence exists)
//   log:       writeInit(id, init),  readGame(id) -> { init, log: [{key, e}] }   (the host's full log)
//              watchLog(id, afterKey, cb(key, e), onReset()) -> off   onReset: the host's log replaced ours; reload
//              appendLog(id, seq, e)  host only, seq = index of the entry
//              sendIntent(id, {uid, s, a, g}),  watchIntents(id, cb(key, intent)) -> off (host),  dropIntent(id, key)
//   fx:        sendFx(id, {uid, s, kind, id, at}),  watchFx(id, cb(v)) -> off,  sweepFx(id, now)
//   handover:  claimHost(id, name)              make me the host when the old host is gone
//   invites:   inviteLink(id) -> url|null,  invite(id) or null (Steam overlay),  onJoinRequest(cb(id)) (Steam),
//              launchLobby() -> id|null (Steam +connect_lobby),  avatar(uid) -> url|null,  onChange (set by net.js),
//              leave(id)                        transport cleanup after leaving a room
import { firebaseTransport } from './net-firebase.js';
import { steamTransport } from './net-steam.js';
import * as Guard from './net-guard.js';

const DT = window.DT;
const NPC = ['Tai', 'Matt', 'Sora', 'Izzy', 'Mimi', 'Joe', 'Kari', 'T.K.'];
const HOST_GONE_MS = 6000;

const T = window.DTSteam ? steamTransport(window.DTSteam) : firebaseTransport();
const STEAM = T.kind === 'steam';

const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const store = {
  get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
};
const cleanName = (s) => String(s || '').replace(/[^\w .'\-]/g, '').trim().slice(0, 16);
let muted = new Set();
try { const saved = JSON.parse(store.get('dt.muted') || '[]'); if (Array.isArray(saved)) muted = new Set(saved.filter((v) => typeof v === 'string').slice(-256)); } catch { /* no saved preferences */ }
const mute = (uid) => {
  if (muted.has(uid)) muted.delete(uid); else { if (muted.size >= 256) muted.delete(muted.values().next().value); muted.add(uid); }
  store.set('dt.muted', JSON.stringify([...muted]));
};
const seatOf = (slots, i) => (slots && slots[i]) || null;

let connected = false;
async function connect() {
  if (connected) return;
  await T.connect();
  connected = true;
}

// ---------- screens ----------
const UI = () => DT.UI;
let screen = null; // cleanup for the current lobby screen listener

function setPanel(html) {
  UI().overlay().classList.remove('hidden');
  if (DT.Client) DT.Client.showLobby(); // the client shell hosts these screens on its Play page
  const p = UI().panel();
  p.innerHTML = html;
  p.classList.add('wide');
  return p;
}

function closeScreen() {
  if (screen) { screen(); screen = null; }
  UI().panel().classList.remove('wide');
}

// Steam builds use the Steam persona name; the web version uses the typed name
function myName() { return (T.personaName && steamName(T.personaName)) || cleanName(store.get('dt.name')) || ''; }
function steamName(s, max = 16) {
  const n = String(s || '').replace(/[\u0000-\u001f<>&"\\]/g, '').trim().slice(0, max);
  return n || `Tamer${String(T.uid || '').slice(-3)}`;
}
const goBack = () => { closeScreen(); if (DT.Client) DT.Client.lobbyBack(); else UI().showTitle(); };
const initials = (n) => esc(String(n || '?').replace(/[^\w]/g, '').slice(0, 2).toUpperCase() || '?');
// a seated player's picture: their Little Legend from presence when we have it, else their initials
function seatAvatar(look, name) {
  try {
    const l = look && DT.LegendEvo && DT.LegendEvo.look ? DT.LegendEvo.look(look) : look;
    if (l && l.legend && DT.Legends) return DT.Legends.html(l.legend, 'mp-av');
  } catch (e) { /* unknown legend */ }
  return `<span class="pt mp-av mp-ini"><b>${initials(name)}</b></span>`;
}
// Steam account picture next to a name (Steam builds only)
function accountPic(uid) {
  const url = uid && T.avatar(uid);
  return url && /^data:image\/png;base64,[\w+/=]+$/.test(url) ? `<img class="mp-acct" src="${url}" alt="" width="20" height="20" style="width:20px;height:20px;border-radius:4px;vertical-align:-4px;margin-right:6px">` : '';
}

const backend = STEAM ? 'Steam' : 'the server';

async function open(roomId) {
  closeScreen();
  if (DT.Client) DT.Client.showLobby(true); // asked for: bring the Play page's lobby view forward
  setPanel('<div class="mp-head"><div><div class="eyebrow">Multiplayer</div><h1 class="page-title">Lobbies</h1></div></div><div class="mp-wait"><span class="spinner"></span> Connecting...</div>');
  try { await connect(); } catch (err) {
    setPanel(`<div class="mp-head"><div><div class="eyebrow">Multiplayer</div><h1 class="page-title">Lobbies</h1></div></div>
      <div class="mp-empty">Could not connect: ${esc(err.message || err)}</div><div class="mpfoot"><button id="mpBack" class="btn btn-ghost">Back</button></div>`);
    document.getElementById('mpBack').onclick = goBack;
    return;
  }
  if (roomId) return enterRoom(roomId);
  showBrowser();
}

let lastLobbies = null; // the lobby list as last seen in the browser
function showBrowser() {
  closeScreen();
  const nameField = STEAM
    ? `<label class="field"><span>Your name</span><input id="mpPersona" readonly value="${esc(myName())}" title="Your Steam name"></label>`
    : `<label class="field"><span>Your name</span><input id="mpName" maxlength="16" value="${esc(myName())}" placeholder="Tamer name"></label>`;
  const visField = STEAM ? '<label class="field"><span>Who can join</span><select id="mpVis" title="Public lobbies show in everyone\'s list; friends-only lobbies show to your Steam friends"><option value="public">Public</option><option value="friends">Friends only</option></select></label>' : '';
  const p = setPanel(`
    <div class="mp-head"><div><div class="eyebrow">Multiplayer${STEAM ? ' · Steam' : ''}</div><h1 class="page-title">Lobbies</h1>
      <p class="sub">Every lobby has 8 Tamers. Seats you and your friends don't take are played by the AI.</p></div></div>
    <div class="mp-create box">
      ${nameField}
      <label class="field grow" style="min-width:220px"><span style="white-space:nowrap">Lobby name</span><input id="mpLobbyName" maxlength="32" placeholder="Lobby name"></label>
      <label class="field"><span>Set</span><select id="mpSet" title="Which set this lobby plays">${setOptions(DT.SetPicker ? DT.SetPicker.selected() : DT.Sets.DEFAULT)}</select></label>
      <label class="field"><span>Pace</span><select id="mpPace" title="Hyper Roll: short games, levels by stage. Normal: standard rules with XP, interest and streaks.">${paceOptions(DT.SetPicker && DT.SetPicker.pace ? DT.SetPicker.pace() : 'hyper')}</select></label>
      <label class="field"><span>Format</span><select id="mpFormat">${formatOptions(DT.SetPicker.format())}</select></label>
      <label class="field"><span>Rounds</span><select id="mpPlan" title="Planning time per round"><option value="30">30 s rounds</option><option value="45">45 s rounds</option><option value="60">60 s rounds</option></select></label>
      ${visField}
      <button id="mpCreate" class="btn btn-primary">Create lobby</button>
    </div>
    <h3 class="mph">Open lobbies <span class="muted" id="mpCount"></span></h3>
    <div id="mpList" class="mplist"><div class="mp-wait"><span class="spinner"></span> Loading...</div></div>
    <div class="mpfoot"><button id="mpBack" class="btn btn-ghost">Back</button></div>`);
  const nameEl = p.querySelector('#mpName');
  if (nameEl) nameEl.oninput = () => { store.set('dt.name', cleanName(nameEl.value)); if (DT.Client) DT.Client.refreshName(); };
  p.querySelector('#mpLobbyName').value = myName() ? `${myName()}'s lobby` : '';
  p.querySelector('#mpBack').onclick = goBack;
  p.querySelector('#mpSet').onchange = (ev) => { if (DT.SetPicker) DT.SetPicker.select(ev.target.value); };
  p.querySelector('#mpPace').onchange = (ev) => { if (DT.SetPicker && DT.SetPicker.setPace) DT.SetPicker.setPace(ev.target.value); };
  p.querySelector('#mpCreate').onclick = async () => {
    const name = needName();
    if (!name) return;
    const vis = p.querySelector('#mpVis');
    let id;
    try {
      id = await createLobby(name, p.querySelector('#mpLobbyName').value, Number(p.querySelector('#mpPlan').value), p.querySelector('#mpSet').value, vis ? vis.value : 'public', p.querySelector('#mpPace').value, p.querySelector('#mpFormat').value);
    } catch (err) { return UI().toast(`Could not create the lobby: ${err.message || err}`); }
    enterRoom(id);
  };
  let shown = null;
  const off = T.watchLobbies((fresh) => {
    lastLobbies = { at: Date.now(), rows: fresh }; // for the client's friends list (DT.Net.lobbies)
    const list = document.getElementById('mpList');
    if (!list) return;
    const count = document.getElementById('mpCount');
    if (count) count.textContent = fresh.length ? String(fresh.length) : '';
    const html = fresh.length ? fresh.map((l) => {
      const seats = Array.from({ length: 8 }, (_, i) => seatOf(l.slots, i));
      const seated = seats.filter(Boolean).length;
      const playing = l.status === 'playing';
      const pips = seats.map((s, i) => (s ? `<span class="mp-pip on" title="${esc(s.name)}">${initials(s.name)}</span>` : `<span class="mp-pip" title="AI: ${NPC[i]}"></span>`)).join('');
      const friend = l.friend ? `<span class="badge badge-you" title="A Steam friend is in this lobby">Friend · ${esc(l.friend)}</span>` : '';
      return `<div class="mplobby${playing ? ' live' : ''}${l.friend ? ' friend' : ''}">
        <div class="mpl-main"><div class="mpl-top"><b>${esc(l.name)}</b><span class="badge ${playing ? 'badge-live' : 'badge-open'}">${playing ? `In game · ${esc(l.round || '1-1')}` : 'Waiting'}</span>${friend}</div>
          <div class="muted">Host ${esc(l.hostName)} · ${seated} player${seated === 1 ? '' : 's'} · ${l.planTime || 30} s rounds${l.setId ? ` · ${esc(setLabel(l.setId))}` : ''} · ${esc(paceLabel(validPace(l.pace) ? l.pace : 'hyper'))}</div></div>
        <div class="mp-pips">${pips}</div>
        <button class="btn ${playing ? 'btn-secondary' : 'btn-primary'}" data-room="${esc(l.id)}">${playing ? 'View' : 'Join'}</button></div>`;
    }).join('') : `<div class="mp-empty">${STEAM ? 'No open lobbies right now. Create one and invite your Steam friends.' : 'No open lobbies right now. Create one and send your friends the link.'}</div>`;
    // the Steam list is polled: leave the page alone when nothing changed, so nothing moves under the cursor
    if (html === shown) return;
    shown = html;
    list.innerHTML = html;
  });
  p.querySelector('#mpList').onclick = (ev) => {
    const b = ev.target.closest('[data-room]');
    if (b && needName()) enterRoom(b.dataset.room);
  };
  screen = off;
}

function needName() {
  if (STEAM) return myName();
  const el = document.getElementById('mpName');
  const name = cleanName(el ? el.value : myName());
  if (!name) { UI().toast('Enter your name first'); if (el) el.focus(); return null; }
  store.set('dt.name', name);
  return name;
}

// ---------- sets ----------
const validSet = (id) => id === 'chaos' || DT.Sets.has(id);
const setLabel = (id) => (DT.SetPicker ? DT.SetPicker.label(id) : DT.Sets.label(id));
function setOptions(sel) {
  const opts = DT.SetPicker ? DT.SetPicker.options() : DT.Sets.pickable();
  return opts.map((o) => `<option value="${esc(o.id)}"${o.id === sel ? ' selected' : ''}>${esc(o.name)}${o.mode && o.mode !== 'tft' && o.mode !== 'mixed' ? ` (${esc(DT.SetPicker ? DT.SetPicker.modeName(o.mode) : o.mode)})` : ''}</option>`).join('');
}
const pendingSet = {}; // lobby id -> set id, when the database refused the setId field
// the room's set: the lobby record, else the host's presence, else the default
function lobbySet() {
  const l = room && room.lobby;
  if (!l) return DT.Sets.DEFAULT;
  const hp = room.presence[l.host];
  const id = l.setId || (hp && hp.lobbySet) || pendingSet[room.id];
  return id && validSet(id) ? id : DT.Sets.DEFAULT;
}
// host: store the room's set on the lobby; fall back to presence when the rules reject the field
async function saveLobbySet(id) {
  if (!room || !validSet(id)) return;
  try { await T.updateLobby(room.id, { setId: id, updated: T.now() }); } catch (e) {
    pendingSet[room.id] = id;
    await T.updatePresence(room.id, { lobbySet: id }).catch(() => {});
  }
}

// ---------- pace ----------
const validPace = (p) => p === 'hyper' || p === 'normal' || p === 'chaos';
const paceLabel = (p) => (DT.SetPicker && DT.SetPicker.paceLabel ? DT.SetPicker.paceLabel(p) : DT.Sets.paceLabel(p));
function paceOptions(sel) {
  const opts = DT.SetPicker && DT.SetPicker.paceOptions ? DT.SetPicker.paceOptions(true) : [{ id: 'hyper', name: 'Hyper Roll' }, { id: 'normal', name: 'Normal' }, { id: 'chaos', name: 'Chaos pace' }];
  return opts.map((o) => `<option value="${esc(o.id)}"${o.id === sel ? ' selected' : ''}>${esc(o.name)}</option>`).join('');
}
const pendingPace = {}; // lobby id -> pace, when the database refused the pace field
// the room's pace: the lobby record, else the host's presence, else Hyper Roll (lobbies from older builds)
function lobbyPace() {
  const l = room && room.lobby;
  if (!l) return 'hyper';
  const hp = room.presence[l.host];
  const p = l.pace || (hp && hp.lobbyPace) || pendingPace[room.id];
  return validPace(p) ? p : 'hyper';
}
// host: store the pace on the lobby record; fall back to presence when the rules reject the field
async function saveLobbyPace(id, pace, inRoom) {
  if (!validPace(pace)) return;
  try { await T.updateLobby(id, { pace, updated: T.now() }); delete pendingPace[id]; } catch (e) {
    pendingPace[id] = pace;
    if (inRoom) await T.updatePresence(id, { lobbyPace: pace }).catch(() => {});
  }
}

async function createLobby(name, lobbyName, planTime, setId, visibility, pace, format) {
  const now = T.now();
  const rec = {
    name: (STEAM ? steamName(lobbyName, 32) : cleanName(lobbyName).slice(0, 32)) || `${name}'s lobby`, host: T.uid, hostName: name, status: 'lobby',
    planTime: [30, 45, 60].includes(planTime) ? planTime : 30, created: now, updated: now, round: '1-1',
    slots: { 0: { uid: T.uid, name } },
  };
  if (STEAM && !String(lobbyName || '').trim()) rec.name = `${name}'s lobby`;
  const set = validSet(setId) ? setId : DT.Sets.DEFAULT;
  const { id, setStored } = await T.createLobby(rec, set, { visibility });
  if (!setStored) pendingSet[id] = set;
  await saveLobbyPace(id, validPace(pace) ? pace : 'hyper', false);
  await saveLobbyFormat(id, format, false);
  return id;
}

// Adjacent seats form pairs; moving seats selects a team. Old lobbies default to solo.
const pendingFormat = {};
const formatOptions = (sel) => ['solo', 'duos'].map(f => `<option value="${f}"${f === sel ? ' selected' : ''}>${f === 'duos' ? 'Duos · 2v2' : 'Free for all'}</option>`).join('');
function lobbyFormat() {
  const l = room && room.lobby;
  return DT.Duos.validFormat(l && (l.format || (room.presence[l.host] || {}).lobbyFormat || pendingFormat[room.id]));
}
async function saveLobbyFormat(id, format, inRoom) {
  format = DT.Duos.validFormat(format);
  try { await T.updateLobby(id, { format, updated: T.now() }); delete pendingFormat[id]; }
  catch (e) { pendingFormat[id] = format; if (inRoom) await T.updatePresence(id, { lobbyFormat: format }); }
}
// ---------- room ----------
let room = null;

async function enterRoom(id) {
  closeScreen();
  const name = myName() || `Tamer${T.uid.slice(0, 3)}`;
  if (STEAM) setPanel('<div class="mp-head"><div><div class="eyebrow">Multiplayer · Steam</div><h1 class="page-title">Joining lobby</h1></div></div><div class="mp-wait"><span class="spinner"></span> Joining...</div>');
  let lobby = null;
  try { lobby = await T.getLobby(id); } catch (e) { lobby = null; }
  if (!lobby) { UI().toast('That lobby is gone'); if (STEAM) T.leave(id); return showBrowser(); }
  room = { id, name, lobby, presence: {}, seenOffline: {}, game: null, host: null, timers: [] };
  await T.joinPresence(id, { ...(pendingFormat[id] ? { lobbyFormat: pendingFormat[id] } : {}), name, look: DT.Cosmetics.loadout(), ...(pendingSet[id] ? { lobbySet: pendingSet[id] } : {}), ...(pendingPace[id] ? { lobbyPace: pendingPace[id] } : {}) });
  // keep our loadout current for the other players
  const offLook = DT.Cosmetics.onChange(() => { if (room && room.id === id) T.updatePresence(id, { look: DT.Cosmetics.loadout() }).catch(() => {}); });
  room.offs = [
    offLook,
    T.watchLobby(id, (l) => { room && onLobby(l); }),
    T.watchPresence(id, (p) => { if (!room) return; room.presence = p || {}; if (!room.game && room.lobby) renderRoom(); }),
  ];
  room.timers.push(setInterval(roomTick, 1000));
  // sit down in the first free seat of a lobby that has not started (a returning player keeps their old seat)
  if (room.lobby.status === 'lobby' && mySlot() < 0) {
    const free = Array.from({ length: 8 }, (_, i) => i).find((i) => !seatOf(room.lobby.slots, i));
    if (free != null) takeSeat(free);
  }
  const link = T.inviteLink(id);
  if (link) history.replaceState(null, '', link);
}

function mySlot() {
  if (!room || !room.lobby) return -1;
  for (let i = 0; i < 8; i++) { const s = seatOf(room.lobby.slots, i); if (s && s.uid === T.uid) return i; }
  return -1;
}

function onLobby(l) {
  if (l?.banned?.includes(T.uid)) { UI().toast('You were removed from this lobby'); return leaveRoom(true); }
  if (!l) { UI().toast('The lobby was closed'); return leaveRoom(true); }
  room.lobby = l;
  const slot = mySlot();
  if (l.status === 'playing' && slot >= 0 && !room.game) return loadGame();
  if (room.game && room.session) {
    if (slot < 0) { UI().toast('You were moved out of your seat (offline too long)'); return UI().leaveNet(); }
    if (l.host === T.uid && !room.host) startHosting();
    return;
  }
  if (!room.game) renderRoom();
}

function renderRoom() {
  const l = room.lobby;
  const slot = mySlot();
  const playing = l.status === 'playing';
  const isHost = l.host === T.uid;
  const alive = l.alive || [];
  const seats = Array.from({ length: 8 }, (_, i) => {
    const s = seatOf(l.slots, i);
    const online = s && room.presence[s.uid];
    const canTake = !s && slot !== i && (!playing || alive[i] !== false);
    let body, av;
    if (s) {
      const pres = room.presence[s.uid];
      av = seatAvatar(pres && pres.look, s.name);
      const tags = `${s.uid === T.uid ? '<span class="badge badge-you">You</span>' : ''}${s.uid === l.host ? '<span class="badge badge-host">Host</span>' : ''}${online ? '' : '<span class="badge badge-off">Offline</span>'}`;
      body = `<b>${STEAM ? accountPic(s.uid) : ''}${esc(s.name)}</b><span class="mps-tags">${tags}</span>`;
    } else {
      av = '<span class="pt mp-av mp-ai"><b>AI</b></span>';
      body = `<span class="muted">AI: ${NPC[i]}${playing && alive[i] === false ? ' (out)' : ''}</span>`;
    }
    const btn = canTake ? `<button class="btn btn-sm btn-secondary" data-seat="${i}">${slot >= 0 ? 'Move here' : 'Take seat'}</button>` : s && s.uid === T.uid && !playing ? '<button class="btn btn-sm btn-ghost" data-unseat="1">Leave seat</button>' :
      s && s.uid !== T.uid ? `<button class="btn btn-sm btn-ghost" data-mute="${i}">${muted.has(s.uid) ? 'Unmute' : 'Mute'}</button>${isHost && T.ban ? `<button class="btn btn-sm btn-ghost" data-remove="${i}">Remove</button>` : ''}` : '';
    return `<div class="mpseat ${s ? 'taken' : ''} ${s && s.uid === T.uid ? 'me' : ''}"><span class="n">${lobbyFormat() === 'duos' ? `T${Math.floor(i / 2) + 1}` : i + 1}</span>${av}<div class="mps-body">${body}</div>${btn}</div>`;
  }).join('');
  const seated = Object.values(l.slots || {}).filter(Boolean).length;
  const link = T.inviteLink(room.id);
  const invite = link
    ? `<div class="mp-invite"><input id="mpLink" readonly value="${esc(link)}" aria-label="Invite link"><button id="mpCopy" class="btn btn-secondary">Copy invite link</button></div>`
    : `<div class="mp-invite"><button id="mpInvite" class="btn btn-secondary" title="Opens the Steam overlay's invite list">Invite Steam friends</button></div>`;
  const p = setPanel(`
    <div class="mp-head"><div><div class="eyebrow">${playing ? 'Game in progress' : 'Lobby'}</div><h1 class="page-title">${esc(l.name)}</h1>
      <p class="sub">${playing ? `Round ${esc(l.round)}. Take an AI seat that is still alive to join.` : `${seated}/8 players · ${l.planTime || 30} s rounds · empty seats are played by the AI.`}</p>
      ${!playing && isHost ? `<label class="field mp-setfield"><span>Set</span><select id="mpRoomSet">${setOptions(lobbySet())}</select></label>
        <label class="field mp-setfield"><span>Pace</span><select id="mpRoomPace">${paceOptions(lobbyPace())}</select></label><label class="field mp-setfield"><span>Format</span><select id="mpRoomFormat">${formatOptions(lobbyFormat())}</select></label>` : `<p class="sub mp-set">Set: <b>${esc(setLabel(lobbySet()))}</b> · Pace: <b>${esc(paceLabel(lobbyPace()))}</b></p>`}</div>
      ${invite}</div>
    <p class="sub">${lobbyFormat() === 'duos' ? 'Duos: seats with the same T number are partners. Move to an empty seat to choose your team.' : 'Free for all'}</p><div class="mpseats">${seats}</div>
    <div class="mpfoot">
      <button id="mpLeave" class="btn btn-ghost">Leave lobby</button>
      ${isHost && !playing ? '<button id="mpStart" class="btn btn-play btn-xl">Start game</button>' : !playing ? `<span class="mp-waithost"><span class="spinner"></span> Waiting for ${esc(l.hostName)} to start</span>` : ''}
    </div>`);
  const copy = p.querySelector('#mpCopy');
  if (copy) copy.onclick = () => {
    const el = p.querySelector('#mpLink');
    el.select();
    (navigator.clipboard ? navigator.clipboard.writeText(el.value) : Promise.reject()).then(() => UI().toast('Invite link copied', 'info'), () => document.execCommand('copy'));
  };
  const inv = p.querySelector('#mpInvite');
  if (inv) inv.onclick = () => { if (T.invite) T.invite(room.id); UI().toast('Pick friends in the Steam overlay (Shift+Tab if it does not open)', 'info'); };
  p.querySelector('#mpLeave').onclick = () => leaveRoom(false);
  p.querySelector('.mpseats').onclick = (ev) => {
    const mu = ev.target.closest('[data-mute]');
    if (mu) { const s = seatOf(room.lobby.slots, Number(mu.dataset.mute)); if (s) mute(s.uid); return renderRoom(); }
    const rm = ev.target.closest('[data-remove]');
    if (rm) { const s = seatOf(room.lobby.slots, Number(rm.dataset.remove)); if (s) T.ban(s.uid); return; }
    const t = ev.target.closest('[data-seat]');
    if (t) return takeSeat(Number(t.dataset.seat));
    if (ev.target.closest('[data-unseat]')) return T.freeSeat(room.id, slot);
  };
  const st = p.querySelector('#mpStart');
  if (st) st.onclick = startGame;
  const rs = p.querySelector('#mpRoomSet');
  if (rs) rs.onchange = () => { if (DT.SetPicker) DT.SetPicker.select(rs.value); saveLobbySet(rs.value); };
  const rf = p.querySelector('#mpRoomFormat');
  if (rf) rf.onchange = () => saveLobbyFormat(room.id, rf.value, true);
  const rp = p.querySelector('#mpRoomPace');
  if (rp) rp.onchange = () => { if (DT.SetPicker && DT.SetPicker.setPace && rp.value !== 'chaos') DT.SetPicker.setPace(rp.value); saveLobbyPace(room.id, rp.value, true); };
}

async function takeSeat(i) {
  const old = mySlot();
  const ok = await T.claimSeat(room.id, i, { uid: T.uid, name: room.name });
  if (!ok) return UI().toast('Someone took that seat');
  if (old >= 0 && old !== i) await T.freeSeat(room.id, old);
}

async function startGame() {
  const l = room.lobby;
  const slots = Array.from({ length: 8 }, (_, i) => {
    const s = seatOf(l.slots, i);
    return s ? { name: s.name, human: true } : { name: NPC[i], human: false };
  });
  const seed = (Math.random() * 1e9) | 0;
  const choice = lobbySet();
  // resolved here so every client plays the same set even if their builds list different sets for Chaos
  const paceChoice = lobbyPace();
  const init = { seed, slots, format: lobbyFormat(), teams: DT.Duos.TEAMS, planTime: l.planTime || 30, at: T.now(), setId: DT.Sets.resolve(choice, seed), chaos: choice === 'chaos',
    pace: DT.Sets.resolvePace(paceChoice, seed) || 'hyper', chaosPace: paceChoice === 'chaos' };
  await T.writeInit(room.id, init);
  await T.updateLobby(room.id, { status: 'playing', updated: T.now(), round: '1-1', alive: slots.map(() => true) });
}

// ---------- game ----------
async function loadGame() {
  room.game = 'loading';
  closeScreen();
  setPanel('<div class="mp-head"><div><div class="eyebrow">Multiplayer</div><h1 class="page-title">Joining game</h1></div></div><div class="mp-wait"><span class="spinner"></span> Loading the game log...</div>');
  const { init, log } = await T.readGame(room.id);
  if (!room || room.game !== 'loading') return;
  if (!init) { room.game = null; return renderRoom(); }
  const game = DT.Lock.create(init);
  let lastKey = null;
  let bad = null;
  for (const { key, e } of log) {
    if (e.d && !bad && DT.Lock.check(game) !== e.d) bad = key;
    DT.Lock.apply(game, e);
    lastKey = key;
  }
  // a fresh replay that still disagrees with the host means the rules ran differently here (a real bug, not a lost entry)
  if (bad) console.warn('Digi-Tactics: replay disagrees with the host at', bad, DT.Lock.digest(game));
  if (!room || room.game !== 'loading') return;
  room.game = game;
  const session = {
    game,
    slot: mySlot(),
    onEntry: null,
    now: () => T.now(),
    send: (a, g) => sendIntent(a, g),
    vote: () => sendIntent('ready'),
    done: () => sendIntent('done'),
    leave: () => leaveRoom(false),
  };
  room.session = session;
  // our own seat may not be human in the log yet (claimed mid-game): the host logs the claim shortly
  room.logOff = T.watchLog(room.id, lastKey, (key, e) => {
    if (!room || room.game !== game) return;
    // the host's state hash: if ours differs, rebuild from the log (fixes missed or misordered entries)
    if (e.d && !room.host && DT.Lock.check(game) !== e.d) return resync(key);
    const res = DT.Lock.apply(game, e);
    if (room.host) room.host.onApplied(e);
    if (session.onEntry) session.onEntry(e, res, true);
  }, () => { if (room && room.game === game) resync('reset', true); });
  // cosmetic effects (emotes, soundboard): outside the lockstep log, only recent ones, never replayed
  room.fxOff = T.watchFx(room.id, (v) => {
    if (!room || !v || v.uid === T.uid) return;
    if (muted.has(v.uid) || seatOf(room.lobby.slots, v.s)?.uid !== v.uid || !['sound', 'emote'].includes(v.kind)) return;
    room.fxLimits = room.fxLimits || new Map();
    if (!Guard.allow(room.fxLimits, v.uid, 0.5, 2)) return;
    if (!DT.Cosmetics.list(v.kind).some((c) => c.id === v.id)) return;
    DT.Social.emit(v.kind, v.s, v.id);
  });
  DT.Social.net = {
    lookOf: (slot) => { const s = room && seatOf(room.lobby.slots, slot); const p = s && room.presence[s.uid]; return (p && p.look) || null; },
    send: (kind, id) => { const s = mySlot(); if (s >= 0) T.sendFx(room.id, { uid: T.uid, s, kind, id: String(id), at: T.now() }); },
  };
  closeScreen();
  UI().startNetGame(session);
  if (room.lobby.host === T.uid) startHosting();
}

// Drop our copy of the game and replay the whole log. Once per minute at most, so a rules bug can't loop.
// forced: the transport replaced our log with the host's (after a host change), so reload regardless of the limit.
function resync(at, forced) {
  const now = Date.now();
  if (!forced) {
    if (room.lastResync && now - room.lastResync < 60000) return;
    room.lastResync = now;
    room.resyncs = (room.resyncs || 0) + 1;
    DT.Net.resyncs = (DT.Net.resyncs || 0) + 1;
  }
  console.warn('Digi-Tactics: out of sync with the host at', at, '- reloading the game');
  if (room.logOff) room.logOff();
  room.logOff = null;
  if (room.fxOff) room.fxOff();
  room.fxOff = null;
  room.game = null;
  room.session = null;
  UI().toast('Out of sync with the host, reloading the game', 'info');
  loadGame();
}

function sendIntent(a, g = {}) {
  if (!room || !room.session) return;
  const slot = mySlot();
  if (slot < 0) return;
  T.sendIntent(room.id, { uid: T.uid, s: slot, a, g: JSON.parse(JSON.stringify(g)), round: room.game.stage * 100 + room.game.round });
}

// ---------- host ----------
function startHosting() {
  const game = room.game;
  const id = room.id;
  const queue = [];
  const done = new Set();
  let waitingFor = -1; // seq of the entry we appended and are waiting to see applied
  let lastBeat = 0;
  const host = {
    onApplied(e) {
      if (e.k === 'start' || e.k === 'next') done.clear();
    },
  };
  room.host = host;
  const intentLimits = new Map();
  room.offs.push(T.watchIntents(id, (key, it) => {
    // Never let a network client submit host-only claim/release operations or fill an unbounded queue.
    const valid = it && Guard.seat(it.s) && seatOf(room?.lobby?.slots, it.s)?.uid === it.uid && Guard.action(it.a, it.g || {}) &&
      (!STEAM || it.round === game.stage * 100 + game.round) && queue.length < 64 && queue.filter((x) => x.uid === it.uid).length < 8 &&
      Guard.allow(intentLimits, it.uid, 4, 8);
    if (valid) queue.push({ key, ...it }); else T.dropIntent(id, key);
  }));

  const append = (e) => {
    e.at = T.now();
    if (e.k === 'next') e.d = DT.Lock.check(game);
    waitingFor = game.seq + 1;
    T.appendLog(id, game.seq, JSON.parse(JSON.stringify(e))).catch(() => { waitingFor = -1; });
  };

  const tick = () => {
    if (!room || room.host !== host || room.lobby.host !== T.uid) return;
    if (waitingFor >= 0 && game.seq < waitingFor) return; // previous entry not echoed yet
    waitingFor = -1;
    const l = room.lobby;
    const now = T.now();
    // seats -> human/AI
    for (let i = 0; i < 8; i++) {
      const s = seatOf(l.slots, i);
      const p = game.players[i];
      if (s && !p.isHuman && p.alive) return append({ k: 'a', s: i, a: 'claim', g: { name: s.name } });
      if (!s && p.isHuman) return append({ k: 'a', s: i, a: 'release', g: { name: NPC[i] } });
    }
    // seats offline too long go back to the AI
    for (let i = 0; i < 8; i++) {
      const s = seatOf(l.slots, i);
      if (!s || s.uid === T.uid) continue;
      if (room.presence[s.uid]) { delete room.seenOffline[s.uid]; continue; }
      room.seenOffline[s.uid] = room.seenOffline[s.uid] || now;
      if (now - room.seenOffline[s.uid] > DT.Lock.OFFLINE_RELEASE_MS) T.freeSeat(id, i);
    }
    // player intents, in arrival order
    // Phase deadlines take priority over input, so an always-full action queue cannot freeze a round.
    const online = new Set();
    for (let i = 0; i < 8; i++) { const s = seatOf(l.slots, i); if (s && (s.uid === T.uid || room.presence[s.uid])) online.add(i); }
    const decision = DT.Lock.decide(game, { now, online, done });
    if (decision) return append(decision);
    while (queue.length) {
      const it = queue.shift();
      T.dropIntent(id, it.key);
      const s = seatOf(l.slots, it.s);
      if (!s || s.uid !== it.uid || !game.players[it.s]) continue;
      if (STEAM && it.round !== game.stage * 100 + game.round) continue;
      if (it.a === 'done') { done.add(it.s); continue; }
      if (it.a === 'ready') return append({ k: 'vote', s: it.s });
      return append({ k: 'a', s: it.s, a: it.a, g: it.g || {} });
    }
    // old emote / soundboard entries
    if (now - (host.fxSweep || 0) > 20000) {
      host.fxSweep = now;
      T.sweepFx(id, now);
    }
    if (now - lastBeat > 15000 || l.round !== game.roundLabel || (game.over && l.status !== 'over')) {
      lastBeat = now;
      T.updateLobby(id, { updated: now, round: game.roundLabel, alive: game.players.map((p) => p.alive), status: game.over ? 'over' : 'playing' });
    }
  };
  room.timers.push(setInterval(tick, 200));
}

// Non-hosts: take over if the host has been gone for a while and we are the first seated player online.
function roomTick() {
  if (!room || !room.lobby || !room.session || room.host) return;
  const l = room.lobby;
  const now = T.now();
  if (room.presence[l.host]) { room.hostGoneAt = null; return; }
  room.hostGoneAt = room.hostGoneAt || now;
  if (now - room.hostGoneAt < HOST_GONE_MS) return;
  for (let i = 0; i < 8; i++) {
    const s = seatOf(l.slots, i);
    if (!s || !room.presence[s.uid]) continue;
    if (s.uid === T.uid) T.claimHost(room.id, room.name).catch(() => {});
    return;
  }
}

async function leaveRoom(closed) {
  if (!room) return;
  const rm = room;
  room = null;
  for (const off of rm.offs || []) off();
  if (rm.logOff) rm.logOff();
  if (rm.fxOff) rm.fxOff();
  DT.Social.net = null;
  for (const t of rm.timers) clearInterval(t);
  const slot = (() => { for (let i = 0; i < 8; i++) { const s = seatOf(rm.lobby && rm.lobby.slots, i); if (s && s.uid === T.uid) return i; } return -1; })();
  if (!closed) {
    try {
      const l = rm.lobby || {};
      const others = Object.entries(l.slots || {}).filter(([, s]) => s && s.uid !== T.uid);
      if (l.host === T.uid && l.status === 'lobby' && !others.length) await T.removeLobby(rm.id);
      else {
        if (slot >= 0) await T.freeSeat(rm.id, slot);
        if (l.host === T.uid && others.length) await T.updateLobby(rm.id, { host: others[0][1].uid, hostName: others[0][1].name });
      }
    } catch (e) { /* lobby already gone */ }
  }
  await T.leavePresence(rm.id).catch(() => {});
  await T.leave(rm.id);
  if (T.inviteLink(rm.id)) history.replaceState(null, '', location.pathname);
  if (UI().inGame()) { if (closed) { UI().leaveNet(); showBrowser(); } return; } // forced removal also exits the match screen
  showBrowser();
}

// for the client shell's lobby status chip
function status() {
  if (!room) return { connected, room: null };
  const l = room.lobby || {};
  // the seated players, for the client's party list
  const members = [];
  for (let i = 0; i < 8; i++) {
    const s = seatOf(l.slots, i);
    if (!s) continue;
    const pres = room.presence[s.uid];
    let legend = null;
    try { const lk = pres && pres.look && DT.LegendEvo && DT.LegendEvo.look ? DT.LegendEvo.look(pres.look) : pres && pres.look; legend = (lk && lk.legend) || null; } catch (e) { legend = null; }
    const pic = STEAM && T.avatar(s.uid);
    members.push({ seat: i, uid: s.uid, name: s.name, me: s.uid === T.uid, host: s.uid === l.host, online: s.uid === T.uid || !!pres, legend,
      pic: pic && /^data:image\/png;base64,[\w+/=]+$/.test(pic) ? pic : null });
  }
  return { connected, room: { id: room.id, name: l.name || '', status: l.status || 'lobby', seated: Object.values(l.slots || {}).filter(Boolean).length, inGame: !!room.session, members } };
}

// invite from outside the lobby screen (the client's friends list): the Steam overlay, or copy the invite link
function invite() {
  if (!room) return false;
  const link = T.inviteLink(room.id);
  if (link) {
    (navigator.clipboard ? navigator.clipboard.writeText(link) : Promise.reject()).then(() => UI().toast('Invite link copied', 'info'), () => UI().toast(`Invite link: ${link}`, 'info'));
    return true;
  }
  if (T.invite) T.invite(room.id);
  UI().toast('Pick friends in the Steam overlay (Shift+Tab if it does not open)', 'info');
  return true;
}
// open lobbies as last listed (rows as in the lobby browser), for the friends list; null before the browser has loaded
const lobbies = () => (lastLobbies && Date.now() - lastLobbies.at < 120000 ? lastLobbies.rows : null);

// Accessible from the match's Tamers header as well as the lobby; mute preferences stay on this device.
function players() {
  if (!room?.lobby) return;
  document.getElementById('mpSafety')?.remove();
  const dialog = document.createElement('dialog');
  dialog.id = 'mpSafety';
  dialog.className = 'mp-safety';
  const title = document.createElement('h2'); title.textContent = 'Players'; dialog.append(title);
  const help = document.createElement('p'); help.textContent = 'Mute hides a player’s emotes and soundboard. Remove blocks them from this lobby.'; dialog.append(help);
  const button = (label, fn, parent = dialog) => { const b = document.createElement('button'); b.className = 'btn btn-secondary'; b.textContent = label; b.onclick = fn; parent.append(b); return b; };
  for (const [slot, s] of Object.entries(room.lobby.slots || {})) {
    if (s.uid === T.uid) continue;
    const row = document.createElement('div'); row.className = 'mp-safety-row';
    const name = document.createElement('span'); name.textContent = s.name; row.append(name);
    button(muted.has(s.uid) ? 'Unmute' : 'Mute', () => { mute(s.uid); players(); }, row).dataset.mute = slot;
    if (T.ban && room.lobby.host === T.uid) button('Remove', async () => { await T.ban(s.uid); players(); }, row).dataset.remove = slot;
    dialog.append(row);
  }
  button('Close', () => dialog.close());
  dialog.addEventListener('close', () => dialog.remove());
  document.body.append(dialog); dialog.showModal();
}
DT.Net = { open, status, invite, lobbies, players, resyncs: 0, lobbySet: () => lobbySet(), transport: T.kind, backend, debug: () => (T.debug ? T.debug() : null) };

// a Steam avatar arrived: redraw the room
T.onChange = () => { if (room && !room.game && room.lobby && document.querySelector('.mpseats')) renderRoom(); };

if (STEAM) {
  // "Join Game" from the Steam friends list or an accepted invite while the game runs
  T.onJoinRequest(async (id) => {
    if (room && room.id === id) return;
    if (UI().inGame()) return UI().toast('Leave your current game first, then accept the invite again');
    if (room) await leaveRoom(false);
    open(id);
  });
  // started by Steam with +connect_lobby <id> (an invite accepted while the game was closed). Joined as soon as the
  // page's scripts have run, without waiting for every image: a returning player has 20 s before their seat goes to the AI.
  setTimeout(async () => {
    const id = await T.launchLobby().catch(() => null);
    if (id) open(id);
  }, 300);
} else {
  // Invite links: ?room=<id> opens that lobby straight away.
  const roomParam = new URLSearchParams(location.search).get('room');
  if (roomParam && /^[\w-]{10,40}$/.test(roomParam)) {
    window.addEventListener('load', () => setTimeout(() => open(roomParam), 50));
  }
}
