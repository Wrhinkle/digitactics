// Set picker: choose which ruleset (set) a solo game or a new lobby plays. Remembers the last choice in this
// browser. Mounted on the Play page's mode select as big art cards; net.js uses DT.SetPicker.options() for the lobby form.
// DT.SetPicker.selected()      chosen id ('faction-wars', another registered set, or 'chaos')
// DT.SetPicker.select(id)      choose (also shows that set in the codex while no game is running)
// DT.SetPicker.options()       [{ id, name, mode, desc, colors }] including Chaos
// DT.SetPicker.label(id)       display name, e.g. 'Chaos' or 'Faction Wars'
// DT.SetPicker.pace()          chosen pace for solo games and new lobbies: 'hyper' (Hyper Roll) or 'normal'
// DT.SetPicker.setPace(p)      choose (remembered; also switches the codex to that pace while no game is running)
// DT.SetPicker.paceOptions(chaos)  [{ id, name, desc }]; chaos: true adds 'chaos' (lobby host only)
// The pace toggle shows under the set cards on the mode select and in the solo party header.
const DT = window.DT;
const KEY = 'dt.set.v1';
const FORMAT_KEY = 'dt.format.v1';
function format() { return DT.Duos.validFormat(store.get(FORMAT_KEY)); }
function setFormat(f) { store.set(DT.Duos.validFormat(f), FORMAT_KEY); render(); if (DT.Client && DT.Client.refreshPlay) DT.Client.refreshPlay(); }
const formatHTML = () => `<div class="pace-pick" role="radiogroup" aria-label="Format"><span class="pp-lbl">Format</span>${['solo','duos'].map(f => `<button class="pp-btn${format() === f ? ' on' : ''}" role="radio" aria-checked="${format() === f}" data-format="${f}">${f === 'duos' ? 'Duos · 2v2' : 'Free for all'}</button>`).join('')}<small class="pp-desc">${format() === 'duos' ? 'Four teams of two. Share health, send Digimon through the portal, combine your traits in one team battle.' : 'Eight Tamers. Every board for itself.'}</small></div>`;
const PACE_KEY = 'dt.pace.v1';
const MODE = { tft: 'Classic', underlords: 'Undermons', autochess: 'Auto Chess' };
const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const store = {
  get(k = KEY) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(v, k = KEY) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
};

// ---------- pace: Hyper Roll or Normal ----------
const PACES = [
  { id: 'hyper', name: 'Hyper Roll', desc: '20 HP, levels by stage, fixed income, 1-gold rerolls. About 25 rounds.' },
  { id: 'normal', name: 'Normal', desc: '100 HP, buy XP, interest and streaks, carousels each stage. About 35-40 rounds.' },
];
const CHAOS_PACE = { id: 'chaos', name: 'Chaos pace', desc: 'The game seed picks Hyper Roll or Normal.' };
function paceOptions(chaos) { return chaos ? PACES.concat([CHAOS_PACE]) : PACES.slice(); }
function pace() { const p = store.get(PACE_KEY); return p === 'normal' || p === 'hyper' ? p : 'hyper'; }
function paceLabel(p) { return (paceOptions(true).find((o) => o.id === p) || PACES[0]).name; }
function setPace(p) {
  if (p !== 'hyper' && p !== 'normal') return;
  store.set(p, PACE_KEY);
  const id = selected();
  if (id !== 'chaos' && !gameRunning()) DT.Sets.activate(id, p);
  render();
  if (DT.Client && DT.Client.refreshPlay) DT.Client.refreshPlay();
}
const paceHTML = (where) => `<div class="pace-pick" role="radiogroup" aria-label="Pace" data-pace-where="${where}"><span class="pp-lbl">Pace</span>${PACES.map((o) => `<button type="button" class="pp-btn${o.id === pace() ? ' on' : ''}"
  role="radio" aria-checked="${o.id === pace()}" data-pace="${o.id}" title="${esc(o.desc)}">${esc(o.name)}</button>`).join('')}<small class="pp-desc">${esc((PACES.find((o) => o.id === pace()) || PACES[0]).desc)}</small></div>`;

const CHAOS = { id: 'chaos', name: 'Chaos', mode: 'mixed', desc: 'A random set every game, picked from the game seed.', colors: { main: '#e58bff' } };

function options() {
  const list = DT.Sets.pickable();
  return list.length > 1 ? list.concat([CHAOS]) : list;
}
function valid(id) { return options().some((o) => o.id === id); }
function selected() {
  const s = store.get();
  return s && valid(s) ? s : DT.Sets.DEFAULT;
}
// a game in progress keeps its own set active
function gameRunning() {
  const g = DT.UI && DT.UI.game && DT.UI.game();
  return !!(g && !g.over) || !!(DT.UI && DT.UI.inGame && DT.UI.inGame());
}
function select(id) {
  if (!valid(id)) return;
  store.set(id);
  if (id !== 'chaos' && !gameRunning()) DT.Sets.activate(id, pace());
  render();
  if (DT.Client && DT.Client.refreshPlay) DT.Client.refreshPlay();
}
const label = (id) => (id === 'chaos' ? 'Chaos' : DT.Sets.label(id));
const modeName = (m) => MODE[m] || (m === 'mixed' ? 'Any set' : m);

// ---------- Play page: big mode cards (League-style game mode select) ----------
// Each set has its own portrait, generated using the shared Astra art workflow.
const artOf = (id) => `assets/client/mode-${id}.webp`;
const css = document.createElement('style');
css.textContent = `
.setpick { display: flex; flex-direction: column; min-width: 0; }
.setpick-nav { display: flex; align-items: center; justify-content: space-between; gap: 12px; margin-bottom: 10px; }
.setpick-nav p { margin: 0; color: var(--c-text-2); font-size: var(--fs-sm); }
.setpick-controls { display: flex; align-items: center; gap: 12px; flex: none; }
.setpick-count { min-width: 54px; text-align: center; font-variant-numeric: tabular-nums; color: var(--lc-gold-1); }
#overlay .setpick-arrow { min-width: 44px; min-height: 40px; padding: 4px 12px; font-size: 24px; }
.setpick-row { display: flex; gap: 18px; overflow-x: auto; overscroll-behavior-x: contain; scroll-snap-type: x mandatory; padding: 4px 4px 16px; scrollbar-width: thin; scrollbar-color: var(--lc-gold-4) transparent; }
.set-card { position: relative; display: flex; flex-direction: column; flex: 0 0 clamp(250px, 27vw, 370px); min-width: 0; padding: 0 !important; text-align: left; white-space: normal; scroll-snap-align: center;
  overflow: hidden; cursor: pointer; border-radius: 6px !important; border: 1px solid var(--lc-gold-4) !important; background: #071320 !important; color: var(--c-text-1);
  transition: border-color var(--dur-2), box-shadow var(--dur-2) !important; }
.set-card .sc-art { display: block; width: 100%; height: clamp(250px, calc(100dvh - 410px), 555px); object-fit: contain; background: #071320; }
.set-card:hover { border-color: var(--lc-gold-2) !important; box-shadow: 0 0 18px #c8a46230 !important; }
.set-card.on { border-color: var(--lc-gold-1) !important; box-shadow: 0 0 0 2px var(--lc-gold-1) !important; }
.set-card .sc-txt { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 8px; padding: 18px; background: linear-gradient(150deg, #102438, #071320); }
.set-card .sc-tag { font: 700 var(--fs-xs) var(--font-display); letter-spacing: .14em; text-transform: uppercase; color: var(--sc, var(--c-accent)); }
.set-card b { font: 700 clamp(22px, 2vw, 30px) / 1.12 var(--font-display); letter-spacing: .02em; color: #f0e6d2; }
.set-card small { font-size: var(--fs-sm); line-height: 1.5; color: var(--c-text-2); overflow-wrap: anywhere; }
.set-card .sc-check { position: absolute; top: 12px; right: 12px; width: 28px; height: 28px; border-radius: 50%; border: 2px solid var(--lc-gold-1); background: #071320e8; opacity: 0; }
.set-card .sc-check::after { content: ""; position: absolute; left: 8px; top: 4px; width: 6px; height: 11px; border: solid var(--lc-gold-1); border-width: 0 2px 2px 0; transform: rotate(45deg); }
.set-card.on .sc-check { opacity: 1; }
.set-card:focus-visible { outline: 2px solid var(--c-accent); outline-offset: 3px; }
.setpick-options { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px 24px; padding: 4px 0 12px; }
@media (max-height: 850px) and (min-width: 701px) { .set-card { flex-basis: clamp(240px, 23vw, 300px); } }
@media (max-width: 700px) {
  .set-card { flex-basis: min(82vw, 320px); }
  .setpick-options { grid-template-columns: 1fr; gap: 0; }
  .setpick-nav p { max-width: 180px; }
}
/* pace toggle (mode select under the set cards, solo party header) */
.pace-pick { display: flex; align-items: center; flex-wrap: wrap; gap: 6px; margin-top: var(--sp-3); }
.pace-pick .pp-lbl { font: 700 var(--fs-xs) var(--font-display); letter-spacing: 0.14em; text-transform: uppercase; color: var(--c-text-2); margin-right: 4px; }
.pace-pick .pp-btn { padding: 6px 14px !important; border: 1px solid var(--lc-gold-4) !important; background: rgba(1, 10, 19, 0.7) !important; color: var(--c-text-2) !important;
  font: 700 var(--fs-sm) var(--font-display); letter-spacing: 0.06em; text-transform: uppercase; cursor: pointer; border-radius: 2px !important; }
.pace-pick .pp-btn:hover { border-color: var(--lc-gold-2) !important; color: var(--c-text-1) !important; }
.pace-pick .pp-btn.on { border-color: var(--lc-gold-1) !important; color: #f0e6d2 !important; box-shadow: 0 0 0 1px var(--lc-gold-1), 0 0 14px rgba(240, 210, 140, 0.25) !important; }
.pace-pick .pp-desc { flex-basis: 100%; color: var(--c-text-2); font-size: var(--fs-sm); }
.party-head .pace-pick { margin-top: 6px; }
/* lobby screens (js/net.js) */
.mp-setfield { margin-top: var(--sp-3); max-width: 240px; }
.mp-set { margin-top: 4px; }
.mp-set b { color: var(--c-text-1); }
`;
document.head.appendChild(css);

let mount = null;
function ensureMount() {
  if (mount && document.body.contains(mount)) return mount;
  const host = document.getElementById('playSets') || document.querySelector('#playSolo .play-solo-body');
  if (!host) return null;
  mount = document.createElement('div');
  mount.className = 'setpick';
  mount.id = 'setPickSolo';
  host.appendChild(mount);
  mount.addEventListener('click', (e) => {
    const fb = e.target.closest('[data-format]');
    if (fb) { setFormat(fb.dataset.format); return; }
    const pb = e.target.closest('[data-pace]');
    if (pb) { if (DT.Sfx) DT.Sfx.play('pick'); setPace(pb.dataset.pace); return; }
    const arrow = e.target.closest('[data-set-step]');
    if (arrow) { move(Number(arrow.dataset.setStep)); return; }
    const b = e.target.closest('[data-set]');
    if (!b) return;
    if (DT.Sfx) DT.Sfx.play('pick');
    select(b.dataset.set);
  });
  mount.addEventListener('keydown', (e) => {
    if (!e.target.closest('[data-set]') || !['ArrowLeft', 'ArrowRight', 'Home', 'End'].includes(e.key)) return;
    e.preventDefault();
    const opts = options();
    if (e.key === 'Home' || e.key === 'End') select(opts[e.key === 'Home' ? 0 : opts.length - 1].id);
    else move(e.key === 'ArrowRight' ? 1 : -1);
    mount.querySelector(`[data-set="${selected()}"]`).focus({ preventScroll: true });
  });
  // Re-center a remembered selection when the page becomes visible or the window changes size.
  new ResizeObserver(() => revealSelection(false)).observe(mount);
  return mount;
}

function move(step) {
  const opts = options();
  const i = opts.findIndex(o => o.id === selected());
  if (DT.Sfx) DT.Sfx.play('pick');
  select(opts[(i + step + opts.length) % opts.length].id);
}
function revealSelection(animate) {
  const row = mount && mount.querySelector('.setpick-row');
  const card = mount && mount.querySelector('.set-card.on');
  if (!row || !card || !row.clientWidth) return;
  const reduced = document.body.classList.contains('reduce-motion') || matchMedia('(prefers-reduced-motion: reduce)').matches;
  const left = row.scrollLeft + card.getBoundingClientRect().left - row.getBoundingClientRect().left - (row.clientWidth - card.offsetWidth) / 2;
  row.scrollTo({ left, behavior: animate && !reduced ? 'smooth' : 'instant' });
}
function render() {
  const m = ensureMount();
  if (!m) return;
  const sel = selected(), opts = options();
  const signature = opts.map(o => o.id).join('|');
  if (m.dataset.sets !== signature) {
    m.dataset.sets = signature;
    m.innerHTML = `<div class="setpick-nav"><p>Choose a set. Swipe or scroll to explore.</p><div class="setpick-controls"><button type="button" class="setpick-arrow" data-set-step="-1" aria-label="Previous set" aria-controls="setCarousel">&#8592;</button><span class="setpick-count" aria-live="polite" aria-atomic="true"></span><button type="button" class="setpick-arrow" data-set-step="1" aria-label="Next set" aria-controls="setCarousel">&#8594;</button></div></div>
      <div id="setCarousel" class="setpick-row" role="radiogroup" aria-label="Set">${opts.map(o => `<button type="button" class="set-card" role="radio" data-set="${esc(o.id)}" style="--sc:${esc((o.colors && o.colors.main) || '#ff8a3d')}" aria-labelledby="set-title-${esc(o.id)}" aria-describedby="set-desc-${esc(o.id)}"><img class="sc-art" src="${artOf(o.id)}" alt="" draggable="false"><span class="sc-check" aria-hidden="true"></span><span class="sc-txt"><span class="sc-tag">${esc(modeName(o.mode))}</span><b id="set-title-${esc(o.id)}">${esc(o.name)}</b><small id="set-desc-${esc(o.id)}">${esc(o.desc)}</small></span></button>`).join('')}</div><div class="setpick-options"></div>`;
  }
  for (const card of m.querySelectorAll('[data-set]')) {
    const on = card.dataset.set === sel;
    card.classList.toggle('on', on);
    card.setAttribute('aria-checked', String(on));
    card.tabIndex = on ? 0 : -1;
  }
  const counter = m.querySelector('.setpick-count');
  counter.textContent = `${opts.findIndex(o => o.id === sel) + 1} / ${opts.length}`;
  counter.setAttribute('aria-label', `${label(sel)}, set ${opts.findIndex(o => o.id === sel) + 1} of ${opts.length}`);
  const controls = m.querySelector('.setpick-options');
  const html = paceHTML('select') + formatHTML();
  if (controls.dataset.html !== html) {
    const focused = controls.contains(document.activeElement) ? document.activeElement : null;
    const selector = focused && (focused.dataset.pace ? `[data-pace="${focused.dataset.pace}"]` : `[data-format="${focused.dataset.format}"]`);
    controls.dataset.html = html; controls.innerHTML = html;
    if (selector) controls.querySelector(selector)?.focus({ preventScroll: true });
  }
  if (m.dataset.selection !== sel) {
    const animate = !!m.dataset.selection;
    m.dataset.selection = sel;
    requestAnimationFrame(() => revealSelection(animate));
  }
  renderPartyPace();
}

// the same toggle in the solo party header (index.html #playSolo .party-head)
let partyMount = null;
function renderPartyPace() {
  if (!partyMount || !document.body.contains(partyMount)) {
    const head = document.querySelector('#playSolo .party-head');
    if (!head) return;
    partyMount = document.createElement('div');
    partyMount.id = 'pacePickParty';
    (head.querySelector('.party-sub') || head).insertAdjacentElement('afterend', partyMount);
    partyMount.addEventListener('click', (e) => {
      const fb = e.target.closest('[data-format]');
    if (fb) { setFormat(fb.dataset.format); return; }
    const pb = e.target.closest('[data-pace]');
      if (pb) { if (DT.Sfx) DT.Sfx.play('pick'); setPace(pb.dataset.pace); }
    });
  }
  const html = paceHTML('party') + formatHTML();
  if (partyMount.dataset.h !== html) { partyMount.dataset.h = html; partyMount.innerHTML = html; }
}

render();
// show the remembered set in the codex once every module has loaded (no game is running yet)
setTimeout(() => { if (selected() !== 'chaos' && (selected() !== DT.SET_ID || DT.SET_PACE !== pace()) && !gameRunning()) DT.Sets.activate(selected(), pace()); }, 0);

DT.SetPicker = { format, setFormat, selected, select, options, label, modeName, render, pace, setPace, paceOptions, paceLabel };
