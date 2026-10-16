// Sound settings popover from the top-bar Sound button: volumes, mutes, and the music source
// (built-in soundtrack or the player's own files, which stay in this browser).
const DT = window.DT;
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
const S = () => DT.Sfx.settings;
const M = () => DT.Music;

const css = `
#sndPop { position: fixed; z-index: 80; width: 330px; max-width: calc(100vw - 16px); max-height: calc(100vh - 70px); overflow: auto; padding: 12px 14px;
  border-radius: 10px; background: var(--panel, #141a26); border: 1px solid var(--line2, #3a4762); box-shadow: 0 10px 30px rgba(0,0,0,0.6); font-size: 13px; }
#sndPop h4 { margin: 0 0 8px; font-size: 13px; display: flex; align-items: center; gap: 8px; }
#sndPop h4 span { flex: 1; }
#sndPop h5 { margin: 12px 0 6px; font-size: 11px; text-transform: uppercase; letter-spacing: 1px; color: var(--muted, #8e98ad); }
#sndPop .sp-row { display: grid; grid-template-columns: 100px 1fr 34px; align-items: center; gap: 8px; margin: 4px 0; }
#sndPop .sp-row input[type=range] { width: 100%; accent-color: var(--accent, #4fb3ff); }
#sndPop .sp-row output { text-align: right; color: var(--muted, #8e98ad); font-variant-numeric: tabular-nums; }
#sndPop label { cursor: pointer; display: flex; align-items: center; gap: 6px; }
#sndPop .sp-src { display: flex; gap: 14px; margin: 4px 0 6px; }
#sndPop .sp-now { color: var(--muted, #8e98ad); margin: 4px 0; min-height: 18px; }
#sndPop .sp-now b { color: var(--text, #e8ebf2); font-weight: 600; }
#sndPop .sp-note { color: var(--muted, #8e98ad); font-size: 12px; margin: 6px 0; }
#sndPop .sp-err { color: var(--bad, #ff5a6a); font-size: 12px; margin: 4px 0; }
#sndPop ul { list-style: none; margin: 6px 0; padding: 0; max-height: 170px; overflow: auto; }
#sndPop li { display: grid; grid-template-columns: 1fr auto auto; gap: 6px; align-items: center; padding: 3px 0; border-bottom: 1px solid var(--line, #2a3448); }
#sndPop li .nm { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
#sndPop li.cur .nm { color: var(--good, #5adf8a); }
#sndPop li select { font: inherit; font-size: 12px; color: var(--text, #e8ebf2); background: var(--panel2, #1c2333); border: 1px solid var(--line2, #3a4762); border-radius: 4px; }
#sndPop li button { padding: 1px 7px; }
#sndPop .sp-btns { display: flex; gap: 6px; flex-wrap: wrap; }
#sndPop .sp-btns button { padding: 4px 10px; }
`;
const style = document.createElement('style');
style.id = 'sp-style';
style.textContent = css;
document.head.appendChild(style);

let pop = null;
const isOpen = () => !!pop && !pop.classList.contains('hidden');
const pct = (v) => Math.round(v * 100);

function buttonLabel() {
  const b = $('soundBtn');
  if (!b) return;
  const m = DT.Sfx.mode;
  b.textContent = { all: 'Sound on', effects: 'Music off', off: 'Muted' }[m];
  b.classList.toggle('off', m === 'off');
}

function build() {
  pop = document.createElement('div');
  pop.id = 'sndPop';
  pop.className = 'hidden';
  pop.innerHTML = `
    <h4><span>Sound</span><label><input type="checkbox" id="spMute"> Mute all</label></h4>
    <div class="sp-row"><label><input type="checkbox" id="spMusicOn"> Music</label><input type="range" id="spMusic" min="0" max="100"><output id="spMusicV"></output></div>
    <div class="sp-row"><label>Effects</label><input type="range" id="spSfx" min="0" max="100"><output id="spSfxV"></output></div>
    <div class="sp-row"><label>Soundboard</label><input type="range" id="spBoard" min="0" max="100"><output id="spBoardV"></output></div>
    <label><input type="checkbox" id="spOthers"> Mute other players' soundboard</label>
    <h5>Music</h5>
    <div class="sp-src">
      <label><input type="radio" name="spSrc" value="builtin"> Built-in soundtrack</label>
      <label><input type="radio" name="spSrc" value="own"> My music files</label>
    </div>
    <div class="sp-now" id="spNow"></div>
    <div class="sp-note">Use your own music: pick mp3, ogg or m4a files you own. They are saved in this browser only and never uploaded. They replace the built-in tracks and play in shuffled order.</div>
    <div class="sp-btns">
      <button id="spAdd">Add files...</button>
      <button id="spNext">Next track</button>
      <button id="spClear">Remove all</button>
    </div>
    <input type="file" id="spFile" multiple accept="audio/*,.mp3,.ogg,.m4a" hidden>
    <div class="sp-err" id="spErr"></div>
    <ul id="spList"></ul>`;
  document.body.appendChild(pop);

  const slider = (id, key) => {
    const el = $(id);
    el.oninput = () => { DT.Sfx.set({ [key]: el.value / 100 }); $(`${id}V`).textContent = el.value; };
    el.onchange = () => { if (key === 'sfx') DT.Sfx.play('buy'); };
  };
  slider('spMusic', 'music');
  slider('spSfx', 'sfx');
  slider('spBoard', 'board');
  $('spBoard').onchange = () => DT.Sfx.clip('digivice', true);
  $('spMute').onchange = (ev) => DT.Sfx.set({ muted: ev.target.checked });
  $('spMusicOn').onchange = (ev) => DT.Sfx.set({ musicOff: !ev.target.checked });
  $('spOthers').onchange = (ev) => DT.Sfx.set({ muteOthers: ev.target.checked });
  for (const r of pop.querySelectorAll('input[name=spSrc]')) {
    r.onchange = () => {
      if (r.value === 'own' && !M().own.list.length) { $('spFile').click(); sync(); return; }
      DT.Sfx.set({ source: r.value });
    };
  }
  $('spAdd').onclick = () => $('spFile').click();
  $('spFile').onchange = async (ev) => {
    const files = [...ev.target.files];
    ev.target.value = '';
    if (!files.length) return;
    $('spErr').textContent = 'Saving...';
    const res = await M().own.add(files);
    $('spErr').textContent = res.skipped.length ? `Skipped: ${res.skipped.join(', ')}` : '';
    sync();
  };
  $('spNext').onclick = () => M().own.next();
  $('spClear').onclick = () => { if (confirm('Remove all of your music files from this browser?')) M().own.clear(); };
  pop.addEventListener('click', (ev) => {
    const rm = ev.target.closest('[data-rm]');
    if (rm) M().own.remove(rm.dataset.rm);
  });
  pop.addEventListener('change', (ev) => {
    const sel = ev.target.closest('select[data-use]');
    if (sel) M().own.setUse(sel.dataset.use, sel.value);
  });
  pop.addEventListener('pointerdown', (ev) => ev.stopPropagation());
}

function sync() {
  buttonLabel();
  if (!isOpen()) return;
  const s = S();
  $('spMute').checked = s.muted;
  $('spMusicOn').checked = !s.musicOff;
  for (const [id, key] of [['spMusic', 'music'], ['spSfx', 'sfx'], ['spBoard', 'board']]) {
    if (document.activeElement !== $(id)) $(id).value = pct(s[key]);
    $(`${id}V`).textContent = pct(s[key]);
  }
  $('spOthers').checked = s.muteOthers;
  const own = M().own.list;
  const src = s.source === 'own' && own.length ? 'own' : 'builtin';
  for (const r of pop.querySelectorAll('input[name=spSrc]')) r.checked = r.value === src;
  const st = M().state();
  const T = M().TRACKS;
  let now;
  if (s.muted || s.musicOff) now = 'Music is off.';
  else if (st.playing) now = `Now playing: <b>${esc(st.title || '')}</b>${st.source === 'builtin' ? ` (${st.phase === 'combat' ? 'combat' : 'planning'} theme)` : ''}`;
  else if (src === 'own') now = 'Your files play during a game.';
  else now = `Built-in: <b>${esc(T.planning.name)}</b> for planning, <b>${esc(T.combat.name)}</b> for combat.`;
  $('spNow').innerHTML = now;
  if (st.error) $('spErr').textContent = st.error;
  $('spNext').disabled = !(st.source === 'own' && own.length > 1);
  $('spClear').disabled = !own.length;
  $('spList').innerHTML = own.map((r) => `<li class="${st.source === 'own' && st.track === r.name ? 'cur' : ''}">
      <span class="nm" title="${esc(r.name)} (${(r.size / 1048576).toFixed(1)} MB)">${esc(r.name)}</span>
      <select data-use="${r.id}" title="When this file plays">
        ${[['both', 'Planning + combat'], ['planning', 'Planning'], ['combat', 'Combat']].map(([v, l]) => `<option value="${v}" ${r.use === v ? 'selected' : ''}>${l}</option>`).join('')}
      </select>
      <button data-rm="${r.id}" title="Remove from this browser">x</button></li>`).join('');
}

function place() {
  const b = $('soundBtn');
  if (!b || !pop) return;
  const r = b.getBoundingClientRect();
  const w = pop.offsetWidth || 330;
  pop.style.top = `${r.bottom + 6}px`;
  pop.style.left = `${Math.max(8, Math.min(window.innerWidth - w - 8, r.right - w))}px`;
}
function open() {
  if (!pop) build();
  pop.classList.remove('hidden');
  sync();
  place();
}
function close() { if (pop) pop.classList.add('hidden'); }
function toggle() { if (isOpen()) close(); else open(); }

window.addEventListener('pointerdown', (ev) => {
  if (!isOpen()) return;
  const b = $('soundBtn');
  if (b && b.contains(ev.target)) return;
  close();
});
window.addEventListener('keydown', (ev) => { if (ev.key === 'Escape' && isOpen()) close(); });
window.addEventListener('resize', () => { if (isOpen()) place(); });
DT.Sfx.onChange(sync);
DT.Music.onChange(sync);
buttonLabel();

DT.SoundUI = { open, close, toggle, isOpen, sync };
