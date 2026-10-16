// Soundboard: short synthesized clips registered as 'sound' cosmetics, a 6-pad panel (G key or the top-bar
// button, keys 1-6 while it is open), labels next to the Tamers list showing who played what, and a rate limit
// of 2 clips per 5 s per player. Clips go out through DT.Social.send('sound', id, slot) and play in DT.Social.on.
const DT = window.DT;
const E = () => DT.Sfx.engine;
const LIMIT = 2;
const WINDOW_MS = 5000;

// ---------- clips: fn(t, out), all under 2 s ----------
const n = (m) => E().note(m);
function wobble(t, out, { f, f2, rate, depth, dur, vol, type = 'sine' }) {
  // a pitch wobble that dies away (boing)
  const ac = E().ac;
  const o = ac.createOscillator();
  o.type = type;
  o.frequency.setValueAtTime(f, t);
  o.frequency.exponentialRampToValueAtTime(f2, t + dur);
  const lfo = ac.createOscillator();
  lfo.frequency.setValueAtTime(rate, t);
  lfo.frequency.linearRampToValueAtTime(rate * 0.6, t + dur);
  const lg = ac.createGain();
  lg.gain.setValueAtTime(depth, t);
  lg.gain.exponentialRampToValueAtTime(1, t + dur);
  lfo.connect(lg).connect(o.frequency);
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(vol, t + 0.01);
  g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
  o.connect(g).connect(out);
  o.start(t); lfo.start(t);
  o.stop(t + dur + 0.05); lfo.stop(t + dur + 0.05);
}
function sweep(t, out, { f0 = 200, f1 = 900, f2 = 200, dur = 1.8, vol = 0.12, notes = [33, 34] }) {
  // detuned saws through a low-pass that opens and closes (drone)
  const ac = E().ac;
  const flt = ac.createBiquadFilter();
  flt.type = 'lowpass';
  flt.Q.value = 8;
  flt.frequency.setValueAtTime(f0, t);
  flt.frequency.exponentialRampToValueAtTime(f1, t + dur * 0.55);
  flt.frequency.exponentialRampToValueAtTime(f2, t + dur);
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(vol, t + 0.35);
  g.gain.setValueAtTime(vol, t + dur - 0.4);
  g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
  flt.connect(g).connect(out);
  for (const m of notes) for (const det of [-8, 8]) {
    const o = ac.createOscillator();
    o.type = 'sawtooth';
    o.frequency.value = n(m);
    o.detune.value = det;
    o.connect(flt);
    o.start(t);
    o.stop(t + dur + 0.05);
  }
}

const CLIPS = {
  digivice: { dur: 0.7, fn: (t, out) => {
    const { tone } = E();
    [2093, 2637, 3136].forEach((f, i) => tone(t + i * 0.09, { type: 'square', f, dec: 0.05, vol: 0.06, out }));
    tone(t + 0.3, { type: 'square', f: 2637, dec: 0.36, vol: 0.05, out });
    tone(t + 0.3, { type: 'square', f: 2650, dec: 0.36, vol: 0.03, out });
  } },
  airhorn: { dur: 1.3, fn: (t, out) => {
    const { voice } = E();
    const horn = [[900, 2, 1], [2100, 3, 0.6], [3400, 4, 0.2]];
    for (const [at, d] of [[0, 0.2], [0.26, 0.2], [0.52, 0.75]]) {
      voice(t + at, { f: 466, f2: 452, dur: d, a: 0.015, vol: 0.2, formants: horn, out });
      voice(t + at, { f: 440, f2: 430, dur: d, a: 0.015, vol: 0.16, formants: horn, out });
    }
  } },
  boing: { dur: 0.7, fn: (t, out) => wobble(t, out, { f: 160, f2: 330, rate: 16, depth: 90, dur: 0.65, vol: 0.32 }) },
  cheer: { dur: 1.8, fn: (t, out) => {
    const { noise, voice } = E();
    noise(t, { type: 'bandpass', f: 1100, q: 0.8, a: 0.35, dec: 1.4, vol: 0.32, out });
    noise(t + 0.1, { type: 'bandpass', f: 2400, q: 1.2, a: 0.3, dec: 1.2, vol: 0.12, out });
    const woo = [[330, 5, 1], [800, 6, 0.4], [2400, 8, 0.1]];
    for (let i = 0; i < 6; i++) {
      const f = 240 + Math.random() * 220;
      voice(t + 0.05 + i * 0.12 + Math.random() * 0.1, { f, f2: f * 1.5, dur: 0.6 + Math.random() * 0.4, a: 0.1, vol: 0.06, formants: woo, vib: [6, 25], out });
    }
  } },
  fanfare: { dur: 1.7, fn: (t, out) => {
    const { brass, tone } = E();
    [67, 72, 76].forEach((m, i) => brass(t + i * 0.13, m, 0.16, 0.2, out));
    brass(t + 0.39, 79, 0.2, 0.2, out);
    brass(t + 0.6, 84, 1.0, 0.24, out);
    brass(t + 0.6, 76, 1.0, 0.14, out);
    for (let i = 0; i < 8; i++) tone(t + 0.25 + i * 0.045, { f: 98, f2: 70, dec: 0.12, vol: 0.18, out });
    tone(t + 0.6, { f: 110, f2: 45, dec: 0.6, vol: 0.4, out });
  } },
  sparkle: { dur: 1.4, fn: (t, out) => {
    const { tone, noise } = E();
    const scale = [84, 86, 88, 91, 93, 96, 98, 100];
    for (let i = 0; i < 14; i++) tone(t + i * 0.07 + Math.random() * 0.04, { f: n(scale[Math.floor(Math.random() * scale.length)]), dec: 0.35, vol: 0.06, out });
    noise(t, { type: 'highpass', f: 6000, f2: 11000, a: 0.3, dec: 0.9, vol: 0.08, out });
  } },
  sadtrombone: { dur: 1.75, fn: (t, out) => {
    const { voice } = E();
    const wah = [[500, 3, 1], [1100, 5, 0.5], [2500, 7, 0.15]];
    [[55, 0, 0.3], [54, 0.32, 0.3], [53, 0.64, 0.3]].forEach(([m, at, d]) => voice(t + at, { f: n(m), dur: d, a: 0.04, vol: 0.2, formants: wah, fshift: 1.6, out }));
    voice(t + 0.96, { f: n(52), f2: n(51.5), dur: 0.8, a: 0.05, vol: 0.2, formants: wah, fshift: 0.6, vib: [5, 45], out });
  } },
  slidewhistle: { dur: 1.0, fn: (t, out) => {
    const { tone, noise } = E();
    tone(t, { f: 600, f2: 2100, a: 0.03, dec: 0.45, vol: 0.14, glide: 'lin', out, vib: [7, 20] });
    tone(t + 0.5, { f: 2100, f2: 700, a: 0.01, dec: 0.42, vol: 0.13, glide: 'lin', out, vib: [7, 20] });
    noise(t, { type: 'bandpass', f: 1500, q: 2, a: 0.05, dec: 0.85, vol: 0.04, out });
  } },
  glitch: { dur: 0.8, fn: (t, out) => {
    const { tone, noise } = E();
    for (let i = 0; i < 18; i++) {
      const at = t + Math.random() * 0.7;
      if (Math.random() < 0.65) tone(at, { type: 'square', f: 150 + Math.random() * 3800, dec: 0.015 + Math.random() * 0.04, vol: 0.06, out });
      else noise(at, { type: 'bandpass', f: 500 + Math.random() * 6000, q: 6, dec: 0.03, vol: 0.2, rate: 0.2 + Math.random() * 2, out });
    }
  } },
  roar: { dur: 1.5, fn: (t, out) => {
    const { voice, tone } = E();
    const throat = [[600, 4, 1], [1000, 5, 0.7], [2400, 6, 0.3]];
    voice(t, { f: 115, f2: 58, dur: 1.4, a: 0.12, vol: 0.32, formants: throat, fshift: 0.7, vib: [32, 60], breath: 0.25, out });
    voice(t + 0.02, { f: 83, f2: 44, dur: 1.35, a: 0.15, vol: 0.2, formants: throat, fshift: 0.7, vib: [27, 80], out });
    tone(t, { f: 60, f2: 35, a: 0.1, dec: 1.2, vol: 0.3, out });
  } },
  rimshot: { dur: 1.2, fn: (t, out) => {
    const { tone, noise } = E();
    tone(t, { f: 220, f2: 130, dec: 0.14, vol: 0.35, out });
    noise(t, { type: 'bandpass', f: 1800, q: 1, dec: 0.08, vol: 0.2, out });
    tone(t + 0.16, { f: 160, f2: 95, dec: 0.16, vol: 0.35, out });
    tone(t + 0.4, { f: 140, f2: 45, dec: 0.25, vol: 0.4, out });
    noise(t + 0.4, { type: 'highpass', f: 5000, dec: 0.75, vol: 0.18, out });
  } },
  gong: { dur: 1.9, fn: (t, out) => {
    const { tone, noise } = E();
    noise(t, { type: 'lowpass', f: 400, dec: 0.08, vol: 0.3, out });
    [[1, 0.2], [1.48, 0.12], [2.09, 0.1], [2.76, 0.08], [3.43, 0.06], [4.12, 0.04]].forEach(([r, v], i) =>
      tone(t, { f: 98 * r, a: 0.02 + i * 0.01, dec: 1.8 - i * 0.12, vol: v, detune: (i % 2 ? 6 : -6), out }));
  } },
  applause: { dur: 1.7, fn: (t, out) => {
    const { noise } = E();
    for (let i = 0; i < 60; i++) {
      const k = i / 60;
      noise(t + k * 1.5 + Math.random() * 0.05, { type: 'bandpass', f: 1000 + Math.random() * 1800, q: 1.5, dec: 0.02 + Math.random() * 0.02, vol: (0.15 + Math.random() * 0.2) * Math.sin(Math.PI * Math.min(1, k * 1.4 + 0.15)), out });
    }
  } },
  drone: { dur: 1.9, fn: (t, out) => {
    sweep(t, out, { notes: [33, 34, 45], dur: 1.85, vol: 0.13 });
    E().tone(t, { f: n(21), a: 0.4, dec: 1.4, vol: 0.25, out });
  } },
  laser: { dur: 0.7, fn: (t, out) => {
    const { tone } = E();
    [0, 0.18, 0.36].forEach((at, i) => tone(t + at, { type: 'square', f: 2600 - i * 300, f2: 140, dec: 0.17, vol: 0.08 - i * 0.015, out }));
  } },
  jackpot: { dur: 1.6, fn: (t, out) => {
    const { tone } = E();
    [84, 88, 91, 96, 100].forEach((m, i) => tone(t + i * 0.06, { type: 'square', f: n(m), dec: 0.12, vol: 0.05, out }));
    for (let i = 0; i < 12; i++) {
      const at = t + 0.3 + i * 0.08 + Math.random() * 0.03;
      tone(at, { type: 'square', f: 1760 + Math.random() * 200, dec: 0.04, vol: 0.04, out });
      tone(at + 0.035, { f: 2637 + Math.random() * 300, dec: 0.22, vol: 0.08, out });
    }
  } },
  crow: { dur: 1.4, fn: (t, out) => {
    const { voice, tone } = E();
    tone(t, { f: n(28), a: 0.3, dec: 1.1, vol: 0.12, out });
    const beak = [[1100, 4, 1], [1800, 6, 0.6], [3000, 8, 0.3]];
    [[0, 0.3], [0.38, 0.26], [0.74, 0.4]].forEach(([at, d]) => voice(t + at, { f: 560, f2: 380, dur: d, a: 0.02, vol: 0.18, formants: beak, vib: [45, 70], breath: 0.12, out }));
  } },
  crest: { dur: 1.8, fn: (t, out) => {
    const { tone, voice, noise } = E();
    [72, 76, 79, 83, 86].forEach((m, i) => tone(t + i * 0.08, { f: n(m), a: 0.01, dec: 1.4, vol: 0.06, out }));
    const aah = [[800, 8, 1], [1150, 10, 0.5], [2900, 12, 0.2]];
    for (const m of [60, 64, 67]) voice(t + 0.2, { f: n(m), dur: 1.5, a: 0.4, vol: 0.06, formants: aah, vib: [5, 15], out });
    noise(t + 0.2, { type: 'highpass', f: 7000, a: 0.4, dec: 1.1, vol: 0.05, out });
  } },
};

// ---------- cosmetics ----------
const ITEMS = [
  ['digivice', 'Digivice Beep', 'common', '📟', 'Three quick beeps and a held tone.', true],
  ['airhorn', 'Air Horn', 'common', '📯', 'Honk, honk, hooonk.', true],
  ['boing', 'Boing', 'common', '🌀', 'A spring that wobbles to a stop.', true],
  ['cheer', 'Crowd Cheer', 'common', '🙌', 'A crowd roars its approval.', true],
  ['rimshot', 'Rimshot', 'common', '🥁', 'Ba-dum, tss.'],
  ['sadtrombone', 'Sad Trombone', 'common', '😢', 'Four notes going down.'],
  ['slidewhistle', 'Slide Whistle', 'common', '🎶', 'Up and back down.'],
  ['applause', 'Applause', 'common', '👏', 'A round of applause.'],
  ['laser', 'Laser', 'common', '⚡', 'Three shots, pew pew pew.'],
  ['fanfare', 'Digivolve Fanfare', 'rare', '🎺', 'A brass fanfare with a drum roll.'],
  ['sparkle', 'Evolution Sparkle', 'rare', '✨', 'A shower of high chimes.'],
  ['glitch', 'Data Glitch', 'rare', '👾', 'Broken bleeps and static.'],
  ['gong', 'Gong', 'rare', '🔔', 'One big gong hit.'],
  ['roar', 'Monster Roar', 'epic', '🦖', 'A deep growling roar.'],
  ['drone', 'Ominous Drone', 'epic', '🌑', 'Low, dark and getting closer.'],
  ['jackpot', 'Coin Jackpot', 'epic', '💰', 'A rising arpeggio and a pile of coins.'],
  ['crow', 'Dark Ocean Crow', 'legendary', '🌊', 'Three caws over a low sea.'],
  ['crest', 'Crest Glow', 'legendary', '🔆', 'Chimes and a soft choir.'],
];
const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };
for (const [id, c] of Object.entries(CLIPS)) DT.Sfx.defineClip(id, c.fn, c.dur);
DT.Cosmetics.register('sound', ITEMS.map(([id, name, rarity, icon, desc, free]) => ({
  id, name, rarity, icon, desc, price: free ? 0 : PRICE[rarity], free: !!free, data: { clip: id, dur: CLIPS[id].dur },
  preview(el) {
    el.innerHTML = `<button type="button" class="sb-prev" title="Play">▶ <span>${icon}</span></button>`;
    el.querySelector('button').onclick = (ev) => { ev.stopPropagation(); ev.preventDefault(); DT.Sfx.engine.ensure(); DT.Sfx.clip(id, true); };
  },
})));

// ---------- styles ----------
const css = `
#sbPanel { position: absolute; left: 50%; bottom: 14px; transform: translateX(-50%); z-index: 30; display: flex; flex-direction: column; gap: 6px;
  padding: 8px 10px 10px; border-radius: 10px; background: linear-gradient(#10261f, #0a1814); border: 1px solid #2f8f6a; box-shadow: 0 6px 24px rgba(0,0,0,0.55), 0 0 0 1px rgba(80,255,180,0.08) inset; }
#sbPanel .sb-h { display: flex; align-items: center; gap: 8px; font-size: 11px; font-weight: 800; letter-spacing: 1px; text-transform: uppercase; color: #7fe8b8; }
#sbPanel .sb-h span { flex: 1; }
#sbPanel .sb-h small { font-weight: 600; letter-spacing: 0; text-transform: none; color: #6aa58c; }
#sbPanel .sb-pads { display: grid; grid-template-columns: repeat(6, 66px); gap: 6px; }
#sbPanel .sb-pad { position: relative; height: 62px; padding: 4px 3px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 2px;
  border-radius: 7px; background: #13362b; border: 1px solid #2c6e55; color: #dff7ec; white-space: normal; overflow: hidden; }
#sbPanel .sb-pad:hover:not(:disabled) { background: #1b4a3a; border-color: #5adfa8; }
#sbPanel .sb-pad i { font-style: normal; font-size: 20px; line-height: 1; }
#sbPanel .sb-pad b { font-size: 10px; font-weight: 600; line-height: 1.1; text-align: center; max-height: 22px; overflow: hidden; }
#sbPanel .sb-pad kbd { position: absolute; top: 2px; left: 3px; padding: 0 3px; font-size: 9px; background: #0b1f18; border-color: #2c6e55; color: #7fe8b8; }
#sbPanel .sb-pad.empty { background: #0e211b; border-style: dashed; color: #6aa58c; }
#sbPanel .sb-pad.hit { animation: sb-hit 0.35s ease-out; }
#sbPanel.cool .sb-pad:not(.empty) { opacity: 0.5; }
#sbPanel .sb-cool { height: 3px; border-radius: 2px; background: #0b1f18; overflow: hidden; }
#sbPanel .sb-cool i { display: block; height: 100%; width: 0; background: #5adfa8; }
@keyframes sb-hit { from { box-shadow: 0 0 0 3px #5adfa8; } to { box-shadow: 0 0 0 0 transparent; } }
.sb-label { position: fixed; z-index: 60; pointer-events: none; transform: translate(-100%, -50%); padding: 3px 8px; border-radius: 6px; font-size: 12px; white-space: nowrap;
  background: #10261f; border: 1px solid #2f8f6a; color: #dff7ec; box-shadow: 0 2px 10px rgba(0,0,0,0.5); animation: sb-float 2.2s ease-out forwards; }
.sb-label b { color: #7fe8b8; }
@keyframes sb-float { 0% { opacity: 0; margin-left: 8px; } 10% { opacity: 1; margin-left: 0; } 75% { opacity: 1; } 100% { opacity: 0; margin-top: -10px; } }
.sb-prev { padding: 6px 12px; font-size: 14px; }
.sb-prev span { margin-left: 4px; }
#sbBtn.on { border-color: #5adfa8; background: #13362b; }
@media (max-width: 900px) { #sbPanel .sb-pads { grid-template-columns: repeat(3, 66px); } }
`;
const style = document.createElement('style');
style.id = 'sb-style';
style.textContent = css;
document.head.appendChild(style);

// ---------- panel ----------
const $ = (id) => document.getElementById(id);
const esc = (s) => String(s).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[c]);
const game = () => (DT.UI && DT.UI.game ? DT.UI.game() : null);
const stats = { sent: 0, played: 0, blocked: 0, droppedRemote: 0, mutedRemote: 0, labels: 0 };
const mine = []; // times of my recent clips
const theirs = new Map(); // slot -> times of their recent clips
let panel = null;
let coolTimer = null;

function recent(list, now) { while (list.length && now - list[0] > WINDOW_MS) list.shift(); return list; }
function cooldownLeft(now = Date.now()) { const l = recent(mine, now); return l.length < LIMIT ? 0 : WINDOW_MS - (now - l[0]); }

function build() {
  if (panel) return panel;
  panel = document.createElement('div');
  panel.id = 'sbPanel';
  panel.className = 'hidden';
  ($('arena') || document.body).appendChild(panel);
  panel.addEventListener('click', (ev) => {
    const pad = ev.target.closest('.sb-pad');
    if (!pad) return;
    if (pad.classList.contains('empty')) { if (DT.Shop) DT.Shop.open('sound'); return; }
    playSlot(+pad.dataset.slot);
  });
  panel.addEventListener('pointerdown', (ev) => ev.stopPropagation());
  panel.addEventListener('contextmenu', (ev) => ev.preventDefault());
  return panel;
}
function render() {
  if (!panel) return;
  const eq = DT.Cosmetics.equipped('sound');
  panel.innerHTML = `<div class="sb-h"><span>Soundboard</span><small>${LIMIT} per ${WINDOW_MS / 1000} s · G to close</small></div>
    <div class="sb-pads">${eq.map((id, i) => {
      const it = id && DT.Cosmetics.get('sound', id);
      if (!it) return `<button class="sb-pad empty" data-slot="${i}" title="Empty slot. Get clips in the Store."><kbd>${i + 1}</kbd><i>+</i><b>Empty</b></button>`;
      return `<button class="sb-pad" data-slot="${i}" title="${esc(it.name)} (${i + 1})"><kbd>${i + 1}</kbd><i>${it.icon || '♪'}</i><b>${esc(it.name)}</b></button>`;
    }).join('')}</div>
    <div class="sb-cool"><i></i></div>`;
  updateCool();
}
function updateCool() {
  if (!panel) return;
  const left = cooldownLeft();
  panel.classList.toggle('cool', left > 0);
  const bar = panel.querySelector('.sb-cool i');
  if (bar) bar.style.width = left > 0 ? `${(100 * left) / WINDOW_MS}%` : '0';
  clearTimeout(coolTimer);
  if (left > 0) coolTimer = setTimeout(updateCool, 100);
}
const isOpen = () => !!panel && !panel.classList.contains('hidden');
function open() {
  if (!game()) return false;
  build();
  render();
  if (DT.Emotes && DT.Emotes.isOpen && DT.Emotes.isOpen()) DT.Emotes.close();
  panel.classList.remove('hidden');
  const b = $('sbBtn');
  if (b) b.classList.add('on');
  return true;
}
function close() {
  if (panel) panel.classList.add('hidden');
  const b = $('sbBtn');
  if (b) b.classList.remove('on');
}
const toggle = () => (isOpen() ? close() : open());

function playSlot(i) {
  const id = DT.Cosmetics.equipped('sound')[i];
  const g = game();
  if (!id || !g || !g.human) return false;
  const now = Date.now();
  if (cooldownLeft(now) > 0) {
    stats.blocked++;
    DT.Sfx.play('error');
    updateCool();
    return false;
  }
  mine.push(now);
  stats.sent++;
  const pad = panel && panel.querySelector(`.sb-pad[data-slot="${i}"]`);
  if (pad) { pad.classList.remove('hit'); void pad.offsetWidth; pad.classList.add('hit'); }
  DT.Social.send('sound', id, g.human.id);
  updateCool();
  return true;
}

// a small label left of the player's row in the Tamers list
function showLabel(slot, id) {
  const g = game();
  const it = DT.Cosmetics.get('sound', id);
  const row = document.querySelector(`#players .pl[data-pid="${slot}"]`);
  if (!g || !it || !row) return;
  const p = g.players.find((q) => q.id === slot);
  const r = row.getBoundingClientRect();
  if (!r.width) return;
  const el = document.createElement('div');
  el.className = 'sb-label';
  el.innerHTML = `${it.icon || '♪'} <b>${esc(p === g.human ? 'You' : p ? p.name : 'Tamer')}</b> ${esc(it.name)}`;
  el.style.left = `${r.left - 6}px`;
  el.style.top = `${r.top + r.height / 2}px`;
  document.body.appendChild(el);
  stats.labels++;
  setTimeout(() => el.remove(), 2300);
}

DT.Social.on('sound', (slot, id) => {
  const g = game();
  if (!g || !DT.Cosmetics.get('sound', id)) return;
  const me = g.human && slot === g.human.id;
  if (!me) {
    if (DT.Sfx.settings.muteOthers) { stats.mutedRemote++; return; }
    const now = Date.now();
    const l = recent(theirs.get(slot) || [], now);
    if (l.length >= LIMIT) { stats.droppedRemote++; return; }
    l.push(now);
    theirs.set(slot, l);
  }
  if (DT.Sfx.clip(id)) stats.played++;
  showLabel(slot, id);
});
DT.Cosmetics.onChange((kind) => { if (kind === 'sound' && isOpen()) render(); });

// keys: G toggles, 1-6 play while open. Capture phase so the digits don't also reach the emote wheel.
window.addEventListener('keydown', (ev) => {
  if (ev.target && /input|textarea|select/i.test(ev.target.tagName)) return;
  if (ev.ctrlKey || ev.metaKey || ev.altKey) return;
  const k = ev.key.toLowerCase();
  if (isOpen()) {
    if (k === 'escape' || k === 't') { close(); return; }
    if (/^[1-6]$/.test(k)) { ev.preventDefault(); ev.stopImmediatePropagation(); if (!ev.repeat) playSlot(+k - 1); return; }
  }
  if (k === 'g' && !ev.repeat) { if (isOpen()) close(); else if (open()) ev.preventDefault(); }
}, true);
// a press outside the panel closes it, but the press still goes through
window.addEventListener('pointerdown', (ev) => {
  if (!isOpen()) return;
  const b = $('sbBtn');
  if (b && b.contains(ev.target)) return;
  close();
});

// top-bar button, next to Sound
(function addButton() {
  const snd = $('soundBtn');
  if (!snd || $('sbBtn')) return;
  const b = document.createElement('button');
  b.id = 'sbBtn';
  b.title = 'Soundboard (G)';
  b.textContent = 'Soundboard';
  b.onclick = () => toggle();
  snd.parentNode.insertBefore(b, snd);
})();

DT.Soundboard = { open, close, toggle, isOpen, play: playSlot, stats, cooldownLeft, clips: Object.keys(CLIPS), LIMIT, WINDOW_MS };
