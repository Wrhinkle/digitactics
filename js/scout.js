// Scouting and spectating: look at any Tamer's board at any time, like TFT.
// Purely a local view. Every client already holds every player's full state (lockstep), so nothing is sent and no
// game state changes. ui.viewing holds the viewed player's id (null = your own board).
//
// Planning: render3d.js drawPlanning draws that player's board, bench and item bench.
// Combat and result: their fight plays live. game.js only keeps a recorded fight for matches with a human; the rest
// are resolved with DT.simulate (no event timeline). So at combat start (ui.js beginCombatView -> snapshot) this
// module copies every match's inputs (sides, seed, combat info), and the first time a fight is watched it rebuilds it
// as a recorded DT.Combat from those copies. The sim is deterministic, so the replay plays the same fight the game
// resolved (checked against the recorded winner and length when it ends, see stats). Replays are stepped in fixed
// DT.TICK steps to the round's clock (ui.js: your own fight's time, else seconds since combat began times speed), so
// jumping in mid-fight fast-forwards to the same moment and then plays live. ui.js show() swaps the fight on screen.
// ui.js blocks drag, buying, selling, item drops and legend walking while ui.viewing is set.
//
// Enter: click a Tamer row, press 1-8 (Tamers list order), Q / E for the previous / next alive Tamer, the bar's arrows,
// or a row of the bar's fight list. Return: click yourself, Space, Esc, or the bar's Return button. Nothing snaps you
// back: in your planning phase the bar shows a prompt instead. After elimination (DT.Scout.follow) you follow the
// Tamer who knocked you out, or the top fight, and move on to the top fight when the Tamer you watch is knocked out.

const DT = window.DT;
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const FADE_OUT = 0.14; // seconds to cover
const FADE_IN = 0.22; // seconds to uncover, with the camera slide
const SLIDE = 3; // world units the camera slides in from
const LEGEND_SPOT = { x: -7.6, z: 8.4 }; // same as LEGEND_START in render3d.js
const OPP_SPOT = { x: 7.6, z: -6.6 }; // the opponent's corner (render3d.js updateOppLegend)
const FRESH_S = 0.25; // a replay started this close to the round's start shows its opening events
const MAX_STEPS = 10000; // same guard as DT.simulate

const css = `
#scoutBanner { position: absolute; top: 10px; left: 50%; transform: translateX(-50%); z-index: 7; max-width: calc(100% - 24px);
  padding: 8px 14px; border-radius: 10px; background: rgba(11, 14, 21, 0.9); border: 1px solid var(--accent);
  box-shadow: 0 6px 24px rgba(0,0,0,0.5); text-align: center; }
#scoutBanner .sch { display: flex; align-items: center; justify-content: center; gap: 10px; font-weight: 800; font-size: 15px; }
#scoutBanner .sch .pt { width: 32px; height: 32px; flex: none; }
#scoutBanner .sch b { color: var(--accent); }
#scoutBanner .sch b.op { color: var(--bad); }
#scoutBanner .sch button { padding: 3px 10px; font-size: 12px; font-weight: 700; }
#scoutBanner .sch button.spn { padding: 2px 9px; font-size: 15px; line-height: 1; }
#scoutBanner .scs { margin-top: 4px; font-size: 13px; color: var(--muted); }
#scoutBanner .scs b { color: var(--text); }
#scoutBanner .scs .w { color: var(--good); } #scoutBanner .scs .l { color: var(--bad); }
#scoutBanner .sct { margin-top: 5px; display: flex; flex-wrap: wrap; justify-content: center; gap: 4px; }
#scoutBanner .sct span { font-size: 11px; font-weight: 700; padding: 1px 6px; border-radius: 8px; border: 1px solid; }
#scoutBanner .spf { margin-top: 6px; display: flex; flex-wrap: wrap; justify-content: center; gap: 4px; }
#scoutBanner .spf div { display: flex; align-items: center; gap: 5px; font-size: 11px; padding: 2px 7px; border-radius: 7px;
  border: 1px solid rgba(255,255,255,0.12); background: rgba(255,255,255,0.04); cursor: pointer; white-space: nowrap; }
#scoutBanner .spf div:hover { border-color: var(--accent); }
#scoutBanner .spf div.on { border-color: var(--accent); background: rgba(79,179,255,0.16); }
#scoutBanner .spf div.end { opacity: 0.6; }
#scoutBanner .spf .u { font-weight: 800; color: var(--text); min-width: 9px; text-align: center; }
#scoutBanner .spf .h { color: var(--muted); }
#scoutBanner .spf .x { color: var(--good); font-weight: 800; }
#scoutBanner .spp { margin-top: 6px; font-size: 12px; color: var(--gold); }
#scoutFade { position: absolute; inset: 0; z-index: 2; pointer-events: none; opacity: 0; background: #05070c; }
.pl { cursor: pointer; }
.pl.scouting { border-color: var(--accent); box-shadow: 0 0 0 1px var(--accent) inset; }
.pl .tag.sc { background: var(--accent); color: #0b1a2c; }
#traitList .ph.scout { color: var(--accent); }
`;
const style = document.createElement('style');
style.textContent = css;
document.head.appendChild(style);

const arena = $('arena');
const banner = document.createElement('div');
banner.id = 'scoutBanner';
banner.className = 'hidden';
arena.appendChild(banner);
const fade = document.createElement('div');
fade.id = 'scoutFade';
arena.appendChild(fade);

const game = () => (DT.UI && DT.UI.game ? DT.UI.game() : null);
const ui = () => (DT.UI && DT.UI.ui ? DT.UI.ui() : null);
const renderer = () => (DT.UI && DT.UI.renderer ? DT.UI.renderer() : null);

let lastGame = null;
let trans = null; // { to, t, phase: 'out' | 'in', dir, off }
let legend = null; // { view, playerId, legendId } for the viewed Tamer
let oppLegend = null; // the same for their opponent while a fight is shown
let bannerSig = '';
let following = false; // eliminated: keep watching the action (see autoFollow)
const stats = { replays: 0, checked: 0, mismatches: 0 }; // replays built; finished replays compared with the game's result

function viewingId() {
  const u = ui();
  return u ? u.viewing : null;
}

// Any phase, as long as the game is running.
function canScout(g) {
  return !!g && !g.over;
}

// Players in the order the Tamers list shows them.
function listOrder(g) {
  const rows = [...document.querySelectorAll('#players .pl[data-pid]')].map((el) => g.players[Number(el.dataset.pid)]).filter(Boolean);
  return rows.length === g.players.length ? rows : g.players.slice();
}

// ---------- fights: copies of this round's inputs and local replays ----------
// round: { matches (game.matches this round), snaps: Map(match -> inputs), replays: Map(match -> Combat) }
let round = null;

// Deep copy of plain data; functions (if a set puts any in mods) are shared, they are not state.
function clone(v) {
  if (v === null || typeof v !== 'object') return v;
  if (Array.isArray(v)) return v.map(clone);
  const o = {};
  for (const k of Object.keys(v)) o[k] = clone(v[k]);
  return o;
}

// Called by ui.js when combat starts (before anything can change a player's state), and lazily after a join.
function snapshot(g) {
  if (!g || !g.matches || (round && round.matches === g.matches)) return;
  round = { matches: g.matches, snaps: new Map(), replays: new Map() };
  for (const m of g.matches) round.snaps.set(m, clone({ sideA: m.sideA, sideB: m.sideB, info: m.info }));
}

// p's match this round, with team = which combat team p is on (identity kept, unlike Game.matchOf)
function matchFor(g, p) {
  if (!g || !p || !p.alive || !g.matches) return null;
  for (const m of g.matches) {
    if (m.a === p || m.a2 === p) return { m, team: 0 };
    if ((m.b === p || m.b2 === p) && !m.ghost) return { m, team: 1 };
  }
  return null;
}

function replayOf(m) {
  let r = round.replays.get(m);
  if (r) return r;
  const s = round.snaps.get(m) || { sideA: clone(m.sideA), sideB: clone(m.sideB), info: clone(m.info) };
  r = new DT.Combat(clone(s.sideA), clone(s.sideB), { seed: m.seed, record: true, info: clone(s.info) });
  r.steps = 0;
  for (const u of r.units) { u.c0 = u.c; u.r0 = u.r; } // starting cells (tests read them; the sim never does)
  round.replays.set(m, r);
  stats.replays++;
  return r;
}

// Step a replay in fixed ticks up to time t (never past it), then check a finished one against the game's result.
function stepTo(r, m, t) {
  while (!r.done && r.t < t - 1e-9 && r.steps < MAX_STEPS) { r.step(DT.TICK); r.steps++; }
  if ((r.done || r.steps >= MAX_STEPS) && !r.checked && m.combat && m.combat.done) {
    r.checked = true;
    stats.checked++;
    const same = r.winner === m.combat.winner && Math.abs(r.t - m.combat.t) < 1e-6;
    if (!same) { stats.mismatches++; console.warn('spectate: replay differs from the recorded fight', { winner: [r.winner, m.combat.winner], t: [r.t, m.combat.t] }); }
  }
}

// The fight to show for the viewed Tamer at the round's clock, or null (planning, no fight, carousel).
// Every match's replay is kept at the clock while you spectate, for the bar's fight list.
function fight(clock) {
  const g = game();
  const id = viewingId();
  if (!g || id == null || !(g.phase === 'combat' || g.phase === 'result')) return null;
  if (!g.matches || !g.matches.length) return null;
  snapshot(g);
  let fresh = false;
  const mf = matchFor(g, g.players[id]);
  if (mf && !round.replays.has(mf.m)) fresh = clock < FRESH_S;
  for (const m of g.matches) stepTo(replayOf(m), m, clock);
  if (!mf) return null;
  return { cb: round.replays.get(mf.m), team: mf.team, fresh, match: mf.m };
}

// Tests and debugging: replay every fight of this round from the copied inputs to its end and compare with the
// game's resolved fight. Returns [{ a, b, winner, recorded, t, recordedT, same }] (recorded: null while unresolved).
function verify() {
  const g = game();
  if (!g || !g.matches) return [];
  snapshot(g);
  return g.matches.map((m) => {
    const s = round.snaps.get(m);
    const r = new DT.Combat(clone(s.sideA), clone(s.sideB), { seed: m.seed, record: true, info: clone(s.info) });
    let n = 0;
    while (!r.done && n++ < MAX_STEPS) r.step(DT.TICK);
    const rec = m.combat && m.combat.done ? m.combat : null;
    return { a: m.a.id, b: m.b ? m.b.id : null, winner: r.winner, recorded: rec ? rec.winner : null, t: r.t, recordedT: rec ? rec.t : null,
      same: !rec || (rec.winner === r.winner && Math.abs(rec.t - r.t) < 1e-6) };
  });
}

// ---------- moving between boards ----------
// Start a visit to player id (or return home when id is yours or null).
function view(id) {
  const g = game();
  const u = ui();
  if (!g || !u) return;
  if (id != null && (!g.players[id] || g.players[id] === g.human)) id = null;
  if (id != null && !canScout(g)) return;
  const cur = trans ? trans.to : u.viewing;
  if (id === cur) return;
  const order = listOrder(g);
  const from = order.indexOf(cur == null ? g.human : g.players[cur]);
  const to = order.indexOf(id == null ? g.human : g.players[id]);
  const dir = to >= from ? 1 : -1;
  if (trans && trans.phase === 'out') { trans.to = id; trans.dir = dir; return; }
  const covered = trans ? Number(fade.style.opacity) || 0 : 0;
  clearSlide();
  trans = { to: id, phase: 'out', t: covered * FADE_OUT, dir, off: 0 };
}

function home(immediate) {
  following = false;
  if (immediate) {
    clearSlide();
    trans = null;
    fade.style.opacity = '0';
    apply(null);
    return;
  }
  view(null);
}

// Alive Tamers in list order, yourself included while you are alive.
function cycleOrder(g) {
  return listOrder(g).filter((q) => q.alive || q === g.human);
}

function cycle(step) {
  const g = game();
  if (!canScout(g)) return;
  const order = cycleOrder(g);
  if (order.length < 2) return;
  const cur = trans ? trans.to : viewingId();
  const at = order.indexOf(cur == null ? g.human : g.players[cur]);
  const next = order[at < 0 ? (step > 0 ? 0 : order.length - 1) : (at + step + order.length) % order.length];
  view(next.id);
}

// Switch the view at full cover.
function apply(id) {
  const g = game();
  const u = ui();
  const r = renderer();
  if (!u) return;
  u.viewing = id;
  u.drag = null;
  u.itemDrag = null;
  u.hoverUid = null;
  u.hoverItem = -1;
  u.hoverCell = null;
  u.hoverCombatId = null;
  if (r && r.setBoard && DT.Social && g) {
    // the viewed Tamer's board theme, pinned (boardOverride) so the automatic choice does not swap it back mid-fight
    if (id != null) {
      const look = DT.Social.lookOf(g.players[id]);
      const board = (look && look.board) || (DT.Boards && DT.Boards.DEFAULT) || null;
      r.boardOverride = board;
      r.setBoard(board);
    } else {
      r.boardOverride = null;
      if (DT.Boards && DT.Boards.wanted) r.setBoard(DT.Boards.wanted(r));
    }
  }
  if (id == null) dropLegends();
  if (DT.UI.refresh) DT.UI.refresh();
  else render();
}

function clearSlide() {
  const r = renderer();
  if (trans && trans.off && r && r.view) { r.view.target.x -= trans.off; r.applyView(); }
  if (trans) trans.off = 0;
}

function setSlide(off) {
  const r = renderer();
  if (!r || !r.view) return;
  r.view.target.x += off - trans.off;
  trans.off = off;
  r.applyView();
}

function step(dt) {
  if (!trans) return;
  trans.t += dt;
  if (trans.phase === 'out') {
    const k = Math.min(1, trans.t / FADE_OUT);
    fade.style.opacity = String(k);
    if (k >= 1) {
      apply(trans.to);
      trans.phase = 'in';
      trans.t = 0;
      setSlide(trans.dir * SLIDE);
    }
    return;
  }
  const k = Math.min(1, trans.t / FADE_IN);
  const e = 1 - (1 - k) * (1 - k) * (1 - k);
  fade.style.opacity = String(1 - k);
  setSlide(trans.dir * SLIDE * (1 - e));
  if (k >= 1) { clearSlide(); trans = null; fade.style.opacity = '0'; }
}

// ---------- after elimination: who to watch ----------
// Total Tamer HP in a match: the "top fight" is the one between the healthiest Tamers.
function matchHp(m) {
  return Math.max(0, m.a.hp) + (m.b && !m.ghost ? Math.max(0, m.b.hp) : 0);
}

function topPlayer(g) {
  if (g.matches && g.matches.length && (g.phase === 'combat' || g.phase === 'result')) {
    const m = g.matches.slice().sort((x, y) => matchHp(y) - matchHp(x))[0];
    const a = m.a.alive ? m.a : m.b;
    if (a && a.alive && a !== g.human) return a;
  }
  return g.alivePlayers().filter((q) => q !== g.human).sort((x, y) => y.hp - x.hp)[0] || null;
}

// The Tamer who knocked you out: your last opponent (for a ghost match, the ghost's real owner).
function killerOf(g) {
  const me = g.human;
  for (const r of g.results || []) {
    const m = r.match;
    if (m.pve) continue;
    if ((m.a === me || m.a2 === me) && !r.aWon) return m.b;
    if ((m.b === me || m.b2 === me) && !m.ghost && !r.bWon) return m.a;
  }
  return null;
}

// Eliminated and chose to spectate: follow your killer if they are still in, else the top fight.
function follow() {
  const g = game();
  if (!canScout(g)) return;
  following = true;
  const k = killerOf(g);
  const q = k && k.alive ? k : topPlayer(g);
  if (q) view(q.id);
}

// While following, move on when the watched Tamer is knocked out (checked once their fight is over).
function autoFollow(g) {
  if (!following || g.human.alive || trans) return;
  const id = viewingId();
  const q = id != null ? g.players[id] : null;
  if (q && q.alive) return;
  const next = topPlayer(g);
  if (next) view(next.id);
}

// ---------- Little Legends of the viewed Tamer and their opponent ----------
function dropLegends() {
  const r = renderer();
  for (const L of [legend, oppLegend]) {
    if (!L) continue;
    if (r) r.scene.remove(L.view.group);
    L.view.dispose();
  }
  legend = null;
  oppLegend = null;
}

function syncLegend(cur, q, spot, color, yaw) {
  const r = renderer();
  const look = q && DT.Social ? DT.Social.lookOf(q) : null;
  const lid = look && look.legend;
  if (cur && (!q || cur.playerId !== q.id || cur.legendId !== lid)) {
    r.scene.remove(cur.view.group);
    cur.view.dispose();
    cur = null;
  }
  if (!q || !lid || !DT.Legends) return cur;
  if (!cur) {
    const v = DT.Legends.createView(lid, yaw == null ? { color } : { color, yaw });
    v.group.position.set(spot.x, 0, spot.z);
    r.scene.add(v.group);
    cur = { view: v, playerId: q.id, legendId: lid };
  }
  return cur;
}

// The viewed Tamer's opponent in the fight on screen (null for PvE, planning or no fight).
function shownOpponent(g) {
  const sh = DT.UI.shown ? DT.UI.shown() : null;
  const id = viewingId();
  if (!sh || !sh.combat || id == null || !round) return null;
  for (const [m, r] of round.replays) {
    if (r !== sh.combat || m.pve) continue;
    return m.a === g.players[id] || m.a2 === g.players[id] ? m.b : m.a;
  }
  return null;
}

// Called by render3d.js drawExtras every frame.
function sceneTick(g, u, dt) {
  const r = renderer();
  const id = u.viewing;
  const q = id != null ? g.players[id] : null;
  if (r.legend) r.legend.view.group.visible = !q;
  if (r.oppLegend) r.oppLegend.view.group.visible = !q;
  legend = syncLegend(legend, q, LEGEND_SPOT, '#4fb3ff');
  oppLegend = syncLegend(oppLegend, q ? shownOpponent(g) : null, OPP_SPOT, '#ff7a6a', 0);
  if (legend) legend.view.update(dt);
  if (oppLegend) oppLegend.view.update(dt);
}

// ---------- the spectate bar and panels ----------
function traitChips(q) {
  const counts = DT.countTraits(q.boardUnits(), q.augs);
  return Object.keys(counts).filter((t) => DT.traitLevel(t, counts[t]))
    .sort((a, b) => DT.traitLevel(b, counts[b]) - DT.traitLevel(a, counts[a]) || counts[b] - counts[a])
    .map((t) => `<span style="color:${DT.TRAITS[t].color};border-color:${DT.TRAITS[t].color}66">${counts[t]} ${esc(DT.TRAITS[t].name)}</span>`).join('');
}

function avatar(q) {
  const look = DT.Social ? DT.Social.lookOf(q) : null;
  return look && look.legend && DT.Legends ? DT.Legends.html(look.legend) : '';
}

const oppName = (m, team) => (m.pve ? 'Wild Digimon' : team === 0 ? `${m.b.name}${m.b2 ? ' + '+m.b2.name : ''}${m.ghost ? ' (ghost)' : ''}` : `${m.a.name}${m.a2 ? ' + '+m.a2.name : ''}`);
const left = (r, team) => (r ? r.units.filter((x) => x.alive && x.team === team).length : '?');

// One row per fight: name HP units-left vs units-left HP name. Click to watch side A.
function fightList(g, cur) {
  if (!round || !g.matches || !g.matches.length) return '';
  return g.matches.map((m) => {
    const r = round.replays.get(m);
    const done = r && r.done;
    const win = (team) => (done && r.winner === team ? '<span class="x">W</span>' : '');
    const bName = m.pve ? 'Wild' : `${m.b.name}${m.ghost ? ' (ghost)' : ''}`;
    const bHp = m.pve || m.ghost ? '' : `<span class="h">${Math.max(0, m.b.hp)}</span>`;
    return `<div data-pid="${m.a.id}" class="${cur === m ? 'on' : ''}${done ? ' end' : ''}" title="Watch ${esc(m.a.name)} vs ${esc(bName)}">
      ${win(0)}${esc(m.a.name)} <span class="h">${Math.max(0, m.a.hp)}</span> <span class="u">${left(r, 0)}</span>v<span class="u">${left(r, 1)}</span> ${bHp} ${esc(bName)}${win(1)}</div>`;
  }).join('');
}

function render() {
  const g = game();
  const id = viewingId();
  const q = g && id != null ? g.players[id] : null;
  for (const el of document.querySelectorAll('#players .pl[data-pid]')) {
    const on = !!q && Number(el.dataset.pid) === id;
    el.classList.toggle('scouting', on);
    const tag = el.querySelector('.tag.sc');
    if (on && !tag) el.querySelector('.nm').insertAdjacentHTML('beforeend', '<span class="tag sc">VIEWING</span>');
    else if (!on && tag) tag.remove();
  }
  if (!q) {
    banner.classList.add('hidden');
    bannerSig = '';
    return;
  }
  const fighting = g.phase === 'combat' || g.phase === 'result';
  const mf = fighting ? matchFor(g, q) : null;
  const r = mf && round ? round.replays.get(mf.m) : null;
  const nav = (d) => `<button class="spn" data-sp="${d}" title="${d < 0 ? 'Previous Tamer (Q)' : 'Next Tamer (E)'}">${d < 0 ? '&#8249;' : '&#8250;'}</button>`;
  const out = !g.human.alive;
  const btns = `<button id="scoutHome" title="Space or Esc">Return</button>${out ? '<button id="scoutLeave">Leave</button>' : ''}`;
  let head;
  let sub;
  if (mf) {
    const oppQ = mf.m.pve ? null : mf.team === 0 ? mf.m.b : mf.m.a;
    head = `${nav(-1)}${avatar(q)}<span>Watching <b>${esc(q.name)}</b> vs <b class="op">${esc(oppName(mf.m, mf.team))}</b></span>${nav(1)}${btns}`;
    const state = r && r.done ? (r.winner === mf.team ? '<span class="w">Won</span>' : r.winner === 1 - mf.team ? '<span class="l">Lost</span>' : 'Draw')
      : `<b>${left(r, mf.team)}</b> vs <b>${left(r, 1 - mf.team)}</b> units left`;
    sub = `<b>${Math.max(0, q.hp)}</b> HP${oppQ && !mf.m.ghost ? ` vs <b>${Math.max(0, oppQ.hp)}</b> HP` : ''} · ${state}`;
  } else {
    const streak = q.streak > 1 ? `<span class="w">W${q.streak} streak</span>` : q.streak < -1 ? `<span class="l">L${-q.streak} streak</span>` : 'No streak';
    const life = q.alive ? `<b>${Math.max(0, q.hp)}</b> HP` : `Placed <b>#${q.placement}</b>`;
    head = `${nav(-1)}${avatar(q)}<span>Viewing <b>${esc(q.name)}</b>'s board</span>${nav(1)}${btns}`;
    sub = `${life} · Level <b>${q.level}</b> · <b>${q.gold}</b> gold · ${streak}`;
  }
  const chips = mf ? '' : traitChips(q);
  const fights = fighting ? fightList(g, mf && mf.m) : '';
  let prompt = '';
  if (g.phase === 'planning' && g.human.alive && g.roundType !== 'carousel') {
    prompt = g.human.offer ? 'Your pick is waiting. Press Space to return to your board' : 'Your planning phase. Press Space to return to your board';
  }
  const html = `<div class="sch">${head}</div><div class="scs">${sub}</div>
    ${chips ? `<div class="sct">${chips}</div>` : ''}${fights ? `<div class="spf">${fights}</div>` : ''}${prompt ? `<div class="spp">${prompt}</div>` : ''}`;
  if (html !== bannerSig) { banner.innerHTML = html; bannerSig = html; }
  banner.classList.remove('hidden');
}

banner.addEventListener('click', (ev) => {
  const t = ev.target;
  if (t.id === 'scoutHome') return home();
  if (t.id === 'scoutLeave') return DT.UI.leaveSpectate && DT.UI.leaveSpectate();
  const nav = t.closest && t.closest('[data-sp]');
  if (nav) { following = false; return cycle(Number(nav.dataset.sp)); }
  const row = t.closest && t.closest('.spf [data-pid]');
  if (row) {
    following = false;
    const g = game();
    const id = Number(row.dataset.pid);
    if (g && g.players[id] === g.human) return home();
    view(id);
  }
});

$('players').addEventListener('click', (ev) => {
  const row = ev.target.closest && ev.target.closest('.pl[data-pid]');
  const g = game();
  if (!row || !g) return;
  following = false;
  const id = Number(row.dataset.pid);
  if (g.players[id] === g.human || id === viewingId()) return home();
  view(id);
});

// Space and Esc return home (the bar sits above #modal so its button stays reachable during picks). Capture phase, so Space does not also press Fight.
window.addEventListener('keydown', (ev) => {
  if (ev.target && /input|textarea|select/i.test(ev.target.tagName)) return;
  if (ev.ctrlKey || ev.metaKey || ev.altKey) return;
  if (viewingId() == null && !(trans && trans.to != null)) return;
  if (ev.key === ' ') { ev.preventDefault(); ev.stopImmediatePropagation(); if (!ev.repeat) home(); }
  else if (ev.key === 'Escape') home();
}, true);

// 1-8 and Q / E. Bubble phase: an open emote wheel takes the digits first (it calls preventDefault),
// and E stays "sell" while you hover one of your own Digimon.
window.addEventListener('keydown', (ev) => {
  if (ev.defaultPrevented || ev.repeat) return;
  if (ev.target && /input|textarea|select/i.test(ev.target.tagName)) return;
  if (ev.ctrlKey || ev.metaKey || ev.altKey) return;
  const g = game();
  const u = ui();
  if (!canScout(g) || !u) return;
  if (document.querySelector('#overlay:not(.hidden), #dtStore:not(.hidden)')) return;
  const k = ev.key.toLowerCase();
  if (/^[1-8]$/.test(k)) {
    const q = listOrder(g)[Number(k) - 1];
    if (q) { following = false; view(q.id); }
  } else if (k === 'q') { following = false; cycle(-1); }
  else if (k === 'e' && u.hoverUid == null) { following = false; cycle(1); }
});

// ---------- per frame: game changes, the fade, the bar ----------
let lastT = performance.now();
let barAt = 0;
let lastPhase = null;
function frame(now) {
  const dt = Math.min(0.1, (now - lastT) / 1000);
  lastT = now;
  const g = game();
  if (g !== lastGame) {
    lastGame = g;
    trans = null;
    following = false;
    round = null;
    fade.style.opacity = '0';
    dropLegends();
    const u = ui();
    if (u) u.viewing = null;
    const r = renderer();
    if (r) r.boardOverride = null;
    render();
  }
  if (g && (viewingId() != null || trans) && !canScout(g)) home(true);
  if (g && g.phase !== lastPhase) {
    lastPhase = g.phase;
    if (g.phase === 'planning') round = null; // the next combat start takes a new snapshot
    if (g.phase === 'planning' && viewingId() != null) render();
  }
  if (g && canScout(g)) {
    // move on after a fight's result is in (result phase), or at the next combat start
    if (g.phase === 'result' || g.phase === 'combat') autoFollow(g);
    if (viewingId() != null && now - barAt > 200) { barAt = now; render(); }
  }
  step(dt);
  requestAnimationFrame(frame);
}
requestAnimationFrame(frame);

const r0 = renderer();
if (r0) r0.scoutTick = sceneTick;

DT.Scout = { view, home, cycle, render, viewing: viewingId, snapshot, fight, follow, verify, stats, busy: () => !!trans, replays: () => (round ? round.replays : null) };
