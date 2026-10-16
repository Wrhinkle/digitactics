// Sound effects, synthesized with WebAudio (no audio files to download).
// DT.Sfx.play(name, opts) from the UI and renderer. Music lives in music.js, soundboard clips in soundboard.js.
// Settings (volumes, mutes, music source) are saved per browser; DT.Sfx.settings / set(patch) / onChange(fn).
const DT = window.DT;
const store = {
  get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
};
const MODES = ['all', 'effects', 'off'];
const KEY = 'dt.audio.v1';
const settings = { music: 0.6, sfx: 0.8, board: 0.8, muted: false, musicOff: false, muteOthers: false, source: 'builtin' };
try {
  const s = JSON.parse(store.get(KEY));
  if (s && typeof s === 'object') Object.assign(settings, s);
  else {
    // older builds saved one mode string
    const old = store.get('dt.sound');
    if (old === 'off') settings.muted = true;
    if (old === 'effects') settings.musicOff = true;
  }
} catch (e) { /* corrupt */ }
const listeners = new Set();

let ac = null;
let master = null;
let sfxBus = null;
let musicBus = null; // music.js connects its tracks here
let musicDuck = null; // dips the music while a soundboard clip plays
let boardBus = null;
let noiseBuf = null;
const last = {}; // name -> time of the last play, for rate limits
const stats = {}; // name -> plays, for tests
let voices = 0;

const sfxGain = () => (settings.muted ? 0 : 0.7 * settings.sfx);
const musicGain = () => (settings.muted || settings.musicOff ? 0 : 0.42 * settings.music);
const boardGain = () => (settings.muted ? 0 : 0.75 * settings.board);

// Browsers only start audio after a user gesture.
function ensure() {
  if (ac) return ac.state === 'running';
  const AC = window.AudioContext || window.webkitAudioContext;
  if (!AC) return false;
  ac = new AC();
  master = ac.createGain();
  master.gain.value = 0.8;
  const comp = ac.createDynamicsCompressor();
  comp.threshold.value = -14;
  comp.ratio.value = 6;
  master.connect(comp).connect(ac.destination);
  sfxBus = ac.createGain();
  sfxBus.gain.value = sfxGain();
  sfxBus.connect(master);
  musicDuck = ac.createGain();
  musicDuck.connect(master);
  musicBus = ac.createGain();
  musicBus.gain.value = musicGain();
  musicBus.connect(musicDuck);
  boardBus = ac.createGain();
  boardBus.gain.value = boardGain();
  boardBus.connect(master);
  noiseBuf = ac.createBuffer(1, ac.sampleRate, ac.sampleRate);
  const d = noiseBuf.getChannelData(0);
  for (let i = 0; i < d.length; i++) d[i] = Math.random() * 2 - 1;
  if (DT.Music && DT.Music.attach) DT.Music.attach();
  return ac.state === 'running';
}
const unlock = () => { ensure(); if (ac && ac.state === 'suspended' && !document.hidden) ac.resume(); };
window.addEventListener('pointerdown', unlock, true);
window.addEventListener('keydown', unlock, true);
document.addEventListener('visibilitychange', () => {
  if (ac) { if (document.hidden) ac.suspend(); else ac.resume(); }
  if (DT.Music && DT.Music.visibility) DT.Music.visibility(document.hidden);
});

// ---------- building blocks ----------
// only game effects count toward MAX_VOICES; soundboard clips and the jingle don't hold back hits and casts
function count(node, out) {
  if (out !== sfxBus) return;
  voices++;
  node.onended = () => voices--;
}
function env(g, t, a, peak, dec) {
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(peak, t + a);
  g.gain.exponentialRampToValueAtTime(0.0001, t + a + dec);
}
// vib: [rate Hz, depth in cents, delay s] adds an LFO to the pitch
function tone(t, { type = 'sine', f = 440, f2 = null, a = 0.005, dec = 0.15, vol = 0.3, out = sfxBus, detune = 0, vib = null, glide = 'exp' }) {
  const o = ac.createOscillator();
  const g = ac.createGain();
  o.type = type;
  o.frequency.setValueAtTime(f, t);
  if (f2) {
    if (glide === 'lin') o.frequency.linearRampToValueAtTime(f2, t + a + dec);
    else o.frequency.exponentialRampToValueAtTime(f2, t + a + dec);
  }
  o.detune.value = detune;
  let lfo = null;
  if (vib) {
    lfo = ac.createOscillator();
    const lg = ac.createGain();
    lfo.frequency.value = vib[0];
    lg.gain.setValueAtTime(0, t);
    lg.gain.linearRampToValueAtTime(vib[1], t + (vib[2] || 0) + 0.05);
    lfo.connect(lg).connect(o.detune);
    lfo.start(t);
    lfo.stop(t + a + dec + 0.05);
  }
  env(g, t, a, vol, dec);
  o.connect(g).connect(out);
  o.start(t);
  o.stop(t + a + dec + 0.05);
  count(o, out);
  return o;
}
function noise(t, { a = 0.003, dec = 0.1, vol = 0.3, type = 'lowpass', f = 1200, f2 = null, q = 1, out = sfxBus, rate = null }) {
  const s = ac.createBufferSource();
  s.buffer = noiseBuf;
  s.playbackRate.value = rate || 0.8 + Math.random() * 0.4;
  const flt = ac.createBiquadFilter();
  flt.type = type;
  flt.frequency.setValueAtTime(f, t);
  if (f2) flt.frequency.exponentialRampToValueAtTime(f2, t + a + dec);
  flt.Q.value = q;
  const g = ac.createGain();
  env(g, t, a, vol, dec);
  s.connect(flt).connect(g).connect(out);
  s.start(t, Math.random() * 0.5);
  s.stop(t + a + dec + 0.05);
  count(s, out);
}
// A buzzy source through parallel band-pass formants: roars, caws, crowd "woo"s.
// formants: [[freq, q, gain], ...]; f/f2 is the pitch glide of the source.
function voice(t, { f = 120, f2 = null, dur = 0.6, a = 0.04, vol = 0.3, formants = [[700, 6, 1], [1200, 8, 0.6], [2600, 10, 0.3]], fshift = 1, type = 'sawtooth', vib = null, breath = 0, out = sfxBus }) {
  const o = ac.createOscillator();
  o.type = type;
  o.frequency.setValueAtTime(f, t);
  if (f2) o.frequency.exponentialRampToValueAtTime(f2, t + dur);
  if (vib) {
    const lfo = ac.createOscillator();
    const lg = ac.createGain();
    lfo.frequency.value = vib[0];
    lg.gain.value = vib[1];
    lfo.connect(lg).connect(o.detune);
    lfo.start(t);
    lfo.stop(t + dur + 0.1);
  }
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(vol, t + a);
  g.gain.setValueAtTime(vol, t + Math.max(a, dur * 0.6));
  g.gain.exponentialRampToValueAtTime(0.0001, t + dur);
  g.connect(out);
  for (const [ff, q, fg] of formants) {
    const bp = ac.createBiquadFilter();
    bp.type = 'bandpass';
    bp.frequency.setValueAtTime(ff, t);
    if (fshift !== 1) bp.frequency.exponentialRampToValueAtTime(ff * fshift, t + dur);
    bp.Q.value = q;
    const fgn = ac.createGain();
    fgn.gain.value = fg;
    o.connect(bp).connect(fgn).connect(g);
  }
  if (breath) noise(t, { type: 'bandpass', f: formants[0][0] * 1.5, q: 1.2, a, dec: dur, vol: breath, out });
  o.start(t);
  o.stop(t + dur + 0.05);
  count(o, out);
}
const note = (n) => 440 * Math.pow(2, (n - 69) / 12); // MIDI note -> Hz
const arp = (t, notes, step, o) => notes.forEach((n, i) => tone(t + i * step, { f: note(n), ...o }));
// brass-like stack: detuned saws through a low-pass that opens on the attack
function brass(t, n, dur, vol, out = sfxBus) {
  const flt = ac.createBiquadFilter();
  flt.type = 'lowpass';
  flt.Q.value = 2;
  flt.frequency.setValueAtTime(400, t);
  flt.frequency.linearRampToValueAtTime(3200, t + 0.06);
  flt.frequency.exponentialRampToValueAtTime(1400, t + dur);
  flt.connect(out);
  for (const det of [-9, 0, 9]) tone(t, { type: 'sawtooth', f: note(n), a: 0.03, dec: dur, vol: vol / 3, detune: det, out: flt, vib: [5.5, 10, 0.25] });
}

// element -> base MIDI note and waveform for ability casts
const ELEM = {
  fire: [52, 'sawtooth'], water: [60, 'sine'], ice: [76, 'triangle'], thunder: [64, 'square'], plant: [57, 'triangle'],
  wind: [67, 'sine'], holy: [72, 'triangle'], dark: [45, 'sawtooth'], metal: [62, 'square'], beast: [48, 'sawtooth'],
};

// soundboard clips (soundboard.js defines them); kill screens reuse a few
const CLIPS = {};
const clip = (id, t, out) => { if (CLIPS[id]) CLIPS[id].fn(t, out || sfxBus); };

// kill screen stings by theme (killscreens.js sends the cosmetic id as opts.variant)
const KILL = {
  basic: (t) => { arp(t, [84, 79, 76, 72, 67], 0.05, { type: 'square', dec: 0.08, vol: 0.07 }); noise(t + 0.2, { type: 'bandpass', f: 4000, f2: 300, q: 2, dec: 0.5, vol: 0.2 }); },
  gate: (t) => { noise(t, { type: 'bandpass', f: 200, f2: 3000, q: 3, a: 0.3, dec: 0.3, vol: 0.3 }); arp(t + 0.35, [62, 69, 74, 78], 0.0, { type: 'triangle', dec: 1.1, vol: 0.09 }); },
  stream: (t) => { tone(t, { type: 'sawtooth', f: 120, f2: 1800, a: 0.5, dec: 0.2, vol: 0.08 }); noise(t, { type: 'highpass', f: 800, f2: 6000, a: 0.5, dec: 0.3, vol: 0.18 }); tone(t + 0.6, { type: 'triangle', f: note(81), dec: 0.6, vol: 0.12 }); },
  glitch: (t) => clip('glitch', t),
  courage: (t) => { brass(t, 62, 0.35, 0.2); brass(t + 0.2, 66, 0.35, 0.2); brass(t + 0.4, 69, 0.9, 0.24); tone(t, { f: 90, f2: 40, dec: 0.5, vol: 0.4 }); },
  pillar: (t) => { for (const n of [60, 67, 72, 76, 79]) tone(t, { type: 'sine', f: note(n), f2: note(n + 12), a: 0.6, dec: 0.6, vol: 0.06 }); clip('sparkle', t + 0.5); },
  ocean: (t) => { tone(t, { type: 'sawtooth', f: note(33), a: 0.3, dec: 1.4, vol: 0.12 }); tone(t, { type: 'sawtooth', f: note(34), a: 0.3, dec: 1.4, vol: 0.08 }); clip('crow', t + 0.3); },
  firewall: (t) => { for (let i = 0; i < 10; i++) noise(t + Math.random() * 0.8, { type: 'bandpass', f: 1500 + Math.random() * 2500, q: 4, dec: 0.05, vol: 0.2 }); tone(t, { f: 70, f2: 30, dec: 0.9, vol: 0.5 }); noise(t, { f: 500, f2: 80, dec: 0.9, vol: 0.35 }); },
  mega: (t) => { tone(t, { f: 60, f2: 25, dec: 1.2, vol: 0.55 }); clip('roar', t + 0.05); },
  confetti: (t) => { arp(t, [67, 72, 76, 79, 84], 0.07, { type: 'square', dec: 0.25, vol: 0.07 }); for (let i = 0; i < 8; i++) noise(t + 0.3 + i * 0.07, { type: 'highpass', f: 3000, dec: 0.03, vol: 0.18 }); },
  sting: (t) => { brass(t, 50, 0.3, 0.18); brass(t + 0.25, 49, 0.9, 0.18); tone(t + 0.25, { f: 80, f2: 40, dec: 0.8, vol: 0.35 }); },
};
const killTheme = (variant) => {
  if (variant === 'eliminated') return 'sting';
  const it = DT.Cosmetics && DT.Cosmetics.get('killscreen', variant);
  const th = it && it.data && it.data.theme;
  return KILL[th] ? th : 'basic';
};

// ---------- sounds ----------
const SOUNDS = {
  buy: (t) => { tone(t, { type: 'triangle', f: 660, dec: 0.06, vol: 0.25 }); tone(t + 0.05, { type: 'triangle', f: 990, dec: 0.1, vol: 0.25 }); },
  sell: (t) => { tone(t, { type: 'triangle', f: 1320, dec: 0.08, vol: 0.2 }); tone(t + 0.06, { type: 'triangle', f: 1760, dec: 0.18, vol: 0.18 }); },
  reroll: (t) => { noise(t, { type: 'bandpass', f: 600, f2: 3200, q: 3, dec: 0.16, vol: 0.35 }); tone(t + 0.12, { type: 'triangle', f: 880, dec: 0.06, vol: 0.15 }); },
  place: (t) => tone(t, { f: 200, f2: 90, dec: 0.1, vol: 0.35 }),
  error: (t) => { tone(t, { type: 'square', f: 150, dec: 0.12, vol: 0.12 }); tone(t + 0.1, { type: 'square', f: 120, dec: 0.14, vol: 0.12 }); },
  equip: (t) => { tone(t, { type: 'triangle', f: 1480, dec: 0.25, vol: 0.18 }); tone(t, { f: 2960, dec: 0.12, vol: 0.08 }); },
  forge: (t) => { noise(t, { type: 'highpass', f: 2500, dec: 0.08, vol: 0.25 }); arp(t + 0.04, [79, 83, 86, 91], 0.05, { type: 'triangle', dec: 0.2, vol: 0.14 }); },
  pick: (t) => arp(t, [72, 76, 79], 0.06, { type: 'triangle', dec: 0.25, vol: 0.18 }),
  orb: (t) => arp(t, [84, 88, 91, 96], 0.04, { type: 'sine', dec: 0.18, vol: 0.14 }),
  digivolve: (t) => {
    arp(t, [60, 64, 67, 72, 76, 79, 84], 0.055, { type: 'triangle', dec: 0.3, vol: 0.16 });
    noise(t + 0.3, { type: 'highpass', f: 5000, dec: 0.5, vol: 0.12 });
  },
  levelup: (t) => arp(t, [60, 67, 72, 76, 79], 0.07, { type: 'square', dec: 0.22, vol: 0.08 }),
  round: (t) => { tone(t, { f: note(57), dec: 1.2, vol: 0.25 }); tone(t, { f: note(64), dec: 1.0, vol: 0.12 }); tone(t, { f: note(69) * 2.01, dec: 0.5, vol: 0.05 }); },
  fight: (t) => {
    tone(t, { f: 110, f2: 45, dec: 0.35, vol: 0.5 });
    noise(t, { f: 900, dec: 0.25, vol: 0.4 });
    noise(t + 0.08, { type: 'bandpass', f: 400, f2: 4000, q: 2, a: 0.25, dec: 0.15, vol: 0.18 });
  },
  tick: (t, o) => tone(t, { type: 'square', f: o && o.last ? 1320 : 990, dec: 0.04, vol: 0.08 }),
  win: (t) => arp(t, [67, 72, 76, 84], 0.09, { type: 'triangle', dec: 0.4, vol: 0.2 }),
  loss: (t) => arp(t, [64, 60, 57, 52], 0.12, { type: 'triangle', dec: 0.45, vol: 0.18 }),
  // the victory jingle from music.js when it is loaded
  champion: (t) => {
    if (DT.Music && DT.Music.jingle) return DT.Music.jingle(t, sfxBus);
    arp(t, [60, 64, 67, 72], 0.12, { type: 'square', dec: 0.3, vol: 0.1 });
    arp(t + 0.5, [72, 76, 79, 84], 0.0, { type: 'triangle', dec: 1.4, vol: 0.12 });
  },
  gameover: (t) => arp(t, [60, 56, 53, 48], 0.22, { type: 'triangle', dec: 0.7, vol: 0.18 }),
  // combat
  hit: (t, o) => {
    const big = o && o.big;
    noise(t, { f: big ? 2200 : 1400, f2: 300, dec: big ? 0.14 : 0.07, vol: big ? 0.4 : 0.22 });
    tone(t, { f: big ? 160 : 220 + Math.random() * 60, f2: 70, dec: 0.08, vol: big ? 0.35 : 0.2 });
  },
  shot: (t) => tone(t, { type: 'triangle', f: 900 + Math.random() * 200, f2: 260, dec: 0.12, vol: 0.12 }),
  cast: (t, o) => {
    const [base, type] = ELEM[(o && o.elem) || ''] || [60, 'triangle'];
    const st = (o && o.stage) || 0;
    noise(t, { type: 'bandpass', f: 300, f2: 3000, q: 1.5, a: 0.15, dec: 0.25, vol: 0.22 });
    tone(t + 0.05, { type, f: note(base + 12), f2: note(base + 24), a: 0.02, dec: 0.35, vol: 0.09 });
    tone(t + 0.05, { type: 'sine', f: note(base), dec: 0.5 + st * 0.2, vol: 0.18 });
    if (st >= 2) tone(t, { f: 70, f2: 35, dec: 0.6, vol: 0.4 });
  },
  death: (t) => { tone(t, { type: 'sawtooth', f: 300, f2: 60, dec: 0.35, vol: 0.12 }); noise(t, { f: 600, f2: 100, dec: 0.3, vol: 0.25 }); },
  heal: (t) => arp(t, [79, 86], 0.06, { type: 'sine', dec: 0.3, vol: 0.1 }),
  shield: (t) => { tone(t, { type: 'triangle', f: 520, f2: 780, dec: 0.25, vol: 0.12 }); tone(t, { f: 1040, dec: 0.3, vol: 0.05 }); },
  // cosmetics: kill screens, emotes, eggs, the store and Bits
  killscreen: (t, o) => {
    KILL[killTheme(o && o.variant)](t);
    if (o && o.big) tone(t, { f: 55, f2: 30, dec: 0.7, vol: 0.35 });
  },
  emote: (t) => { tone(t, { f: 520, f2: 1100, dec: 0.07, vol: 0.18 }); tone(t + 0.06, { type: 'triangle', f: 1320, dec: 0.12, vol: 0.12 }); },
  egg_wobble: (t) => {
    tone(t, { type: 'triangle', f: 310, f2: 250, dec: 0.07, vol: 0.22 });
    noise(t, { type: 'bandpass', f: 900, q: 4, dec: 0.04, vol: 0.18 });
    tone(t + 0.11, { type: 'triangle', f: 270, f2: 220, dec: 0.08, vol: 0.18 });
  },
  egg_crack: (t) => {
    for (let i = 0; i < 5; i++) noise(t + i * 0.03 + Math.random() * 0.02, { type: 'bandpass', f: 2200 + Math.random() * 2500, q: 5, dec: 0.025, vol: 0.3 });
    tone(t, { f: 190, f2: 80, dec: 0.12, vol: 0.25 });
  },
  egg_reveal: (t, o) => {
    const r = ['common', 'rare', 'epic', 'legendary'].indexOf((o && o.rarity) || 'common');
    const lvl = Math.max(0, r);
    noise(t, { type: 'highpass', f: 3000, f2: 9000, dec: 0.4 + lvl * 0.2, vol: 0.14 });
    arp(t, [72, 76, 79, 84, 88, 91].slice(0, 3 + lvl), 0.06, { type: 'triangle', dec: 0.35, vol: 0.13 });
    if (lvl >= 2) for (const n of [60, 67, 76]) tone(t + 0.25, { type: 'sine', f: note(n), a: 0.05, dec: 1.2, vol: 0.07 });
    if (lvl >= 3) { brass(t + 0.3, 72, 0.9, 0.12); clip('sparkle', t + 0.35); }
  },
  purchase: (t) => {
    noise(t, { type: 'bandpass', f: 1800, q: 2, dec: 0.05, vol: 0.25 });
    tone(t + 0.04, { type: 'square', f: 1568, dec: 0.05, vol: 0.07 });
    tone(t + 0.1, { type: 'sine', f: 2093, dec: 0.6, vol: 0.16 });
    tone(t + 0.1, { type: 'sine', f: 4186, dec: 0.3, vol: 0.05 });
  },
  coins: (t, o) => {
    const n = Math.max(2, Math.min(7, Math.round(((o && o.n) || 30) / 15)));
    for (let i = 0; i < n; i++) {
      const tt = t + i * 0.07 + Math.random() * 0.015;
      tone(tt, { type: 'square', f: 1760 + Math.random() * 120, dec: 0.04, vol: 0.05 });
      tone(tt + 0.04, { type: 'sine', f: 2637 + Math.random() * 160, dec: 0.18, vol: 0.09 });
    }
  },
};
// minimum seconds between two plays of the same sound, so a big fight doesn't turn into noise
const GAP = { hit: 0.05, shot: 0.05, heal: 0.15, death: 0.08, shield: 0.12, place: 0.04, tick: 0.3, cast: 0.06, egg_wobble: 0.08 };
const MAX_VOICES = 40;

function play(name, opts) {
  if (settings.muted || !SOUNDS[name]) return;
  if (!ensure() && !ac) return;
  if (ac.state !== 'running') return;
  const t = ac.currentTime;
  if (GAP[name] && last[name] && t - last[name] < GAP[name]) return;
  if (voices > MAX_VOICES && GAP[name]) return;
  last[name] = t;
  SOUNDS[name](t + 0.005, opts);
  stats[name] = (stats[name] || 0) + 1;
}

// Soundboard: play clip `id` on the soundboard bus and dip the music under it. delay: seconds from now.
function playClip(id, preview, delay = 0) {
  const c = CLIPS[id];
  if (!c || settings.muted) return false;
  if (!ensure() && !ac) return false;
  if (ac.state !== 'running') return false;
  const t = ac.currentTime + 0.01 + Math.max(0, delay || 0);
  c.fn(t, boardBus);
  const d = musicDuck.gain;
  d.cancelScheduledValues(t);
  d.setValueAtTime(d.value, t);
  d.linearRampToValueAtTime(0.45, t + 0.05);
  d.setValueAtTime(0.45, t + (c.dur || 1));
  d.linearRampToValueAtTime(1, t + (c.dur || 1) + 0.4);
  const k = preview ? 'clip_preview' : `clip:${id}`;
  stats[k] = (stats[k] || 0) + 1;
  return true;
}

function applyGains() {
  if (!ac) return;
  sfxBus.gain.setTargetAtTime(sfxGain(), ac.currentTime, 0.05);
  musicBus.gain.setTargetAtTime(musicGain(), ac.currentTime, 0.1);
  boardBus.gain.setTargetAtTime(boardGain(), ac.currentTime, 0.05);
}
function set(patch) {
  Object.assign(settings, patch);
  store.set(KEY, JSON.stringify(settings));
  applyGains();
  for (const fn of listeners) { try { fn(settings, patch); } catch (e) { console.warn(e); } }
}
const modeOf = () => (settings.muted ? 'off' : settings.musicOff ? 'effects' : 'all');

DT.Sfx = {
  play,
  stats,
  get running() { return !!ac && ac.state === 'running'; },
  get musicOn() { return !!(DT.Music && DT.Music.playing); },
  get mode() { return modeOf(); },
  get settings() { return settings; },
  set,
  onChange(fn) { listeners.add(fn); return () => listeners.delete(fn); },
  cycle() {
    const m = MODES[(MODES.indexOf(modeOf()) + 1) % MODES.length];
    set({ muted: m === 'off', musicOff: m === 'effects' });
    return m;
  },
  wantMusic: false,
  music(on) { this.wantMusic = on; if (DT.Music) DT.Music.want(on); },
  combat(on) { if (DT.Music) DT.Music.phase(on ? 'combat' : 'planning'); },
  // soundboard
  defineClip(id, fn, dur) { CLIPS[id] = { fn, dur }; },
  clip: playClip,
  hasClip: (id) => !!CLIPS[id],
  // building blocks for music.js and soundboard.js
  engine: {
    ensure,
    get ac() { return ac; },
    get musicBus() { return musicBus; },
    get sfxBus() { return sfxBus; },
    get boardBus() { return boardBus; },
    get noiseBuf() { return noiseBuf; },
    get voices() { return voices; },
    tone, noise, voice, brass, note, arp,
  },
};
