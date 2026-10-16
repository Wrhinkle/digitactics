// Elimination sounds: the 'elimsound' cosmetic. It plays when your kill screen starts, for everyone watching it.
// Sources: twelve synthesized stingers made here, plus every soundboard clip (js/soundboard.js). A soundboard clip
// counts as owned here whenever you own it on the soundboard (item.linked), so it is never sold twice.
// Playback goes through the soundboard bus: the soundboard volume, Mute all, and "Mute other players' soundboard"
// (for anyone else's elimination sound) all apply.
//
// DT.ElimSounds.play(id, { mine, delay, preview })  -> true when it played
// DT.ElimSounds.resolve(id)                          a known id, or the free default (older clients send none)
// DT.ElimSounds.stats                                { played, muted, preview }
const DT = window.DT;
const E = () => DT.Sfx.engine;
const n = (m) => E().note(m);

// ---------- stingers: fn(t, out), all about 2 s or less ----------
function hornBlast(t, out, f, dur, vol) {
  const { voice } = E();
  const horn = [[900, 2, 1], [2100, 3, 0.6], [3400, 4, 0.2]];
  voice(t, { f, f2: f * 0.97, dur, a: 0.012, vol, formants: horn, out });
  voice(t, { f: f * 0.944, f2: f * 0.92, dur, a: 0.012, vol: vol * 0.8, formants: horn, out });
}
function kick(t, out, vol = 0.45) {
  const { tone, noise } = E();
  tone(t, { f: 130, f2: 42, dec: 0.32, vol, out });
  noise(t, { type: 'lowpass', f: 900, dec: 0.05, vol: vol * 0.4, out });
}
function snareRoll(t, out, n0, step, vol = 0.12) {
  const { noise } = E();
  for (let i = 0; i < n0; i++) noise(t + i * step, { type: 'bandpass', f: 2600, q: 0.9, dec: 0.05, vol: vol * (0.5 + (0.5 * i) / n0), out });
}

const STINGERS = {
  // three horn blasts over a kick and a crash
  horn: { dur: 1.7, fn: (t, out) => {
    const { noise } = E();
    hornBlast(t, out, 466, 0.16, 0.2);
    hornBlast(t + 0.2, out, 466, 0.16, 0.2);
    hornBlast(t + 0.42, out, 554, 0.95, 0.22);
    kick(t, out); kick(t + 0.2, out); kick(t + 0.42, out, 0.55);
    noise(t + 0.42, { type: 'highpass', f: 5000, dec: 1.0, vol: 0.16, out });
  } },
  // rising arpeggio, a shimmer, and a held major chord
  chime: { dur: 1.9, fn: (t, out) => {
    const { tone, noise } = E();
    [60, 64, 67, 72, 76, 79, 84, 88].forEach((m, i) => tone(t + i * 0.055, { type: 'triangle', f: n(m), dec: 0.3, vol: 0.11, out }));
    noise(t + 0.2, { type: 'highpass', f: 6000, f2: 12000, a: 0.25, dec: 0.9, vol: 0.08, out });
    for (const m of [72, 76, 79, 84]) {
      tone(t + 0.5, { type: 'sine', f: n(m), a: 0.02, dec: 1.3, vol: 0.07, out, vib: [5, 8, 0.3] });
      tone(t + 0.5, { type: 'sine', f: n(m + 12) * 1.003, a: 0.02, dec: 0.9, vol: 0.025, out });
    }
  } },
  // a short square-wave victory tune with a drum hit
  jingle: { dur: 1.8, fn: (t, out) => {
    const { tone } = E();
    const tune = [[67, 0, 0.1], [67, 0.12, 0.1], [67, 0.24, 0.1], [72, 0.36, 0.5], [71, 0.88, 0.12], [72, 1.02, 0.7]];
    for (const [m, at, d] of tune) {
      tone(t + at, { type: 'square', f: n(m), a: 0.005, dec: d, vol: 0.07, out });
      tone(t + at, { type: 'triangle', f: n(m - 12), a: 0.005, dec: d, vol: 0.1, out });
    }
    for (const m of [60, 64, 67]) tone(t + 1.02, { type: 'triangle', f: n(m), a: 0.01, dec: 0.75, vol: 0.06, out });
    kick(t + 0.36, out, 0.35); kick(t + 1.02, out, 0.4);
  } },
  // a deletion zap: falling square sweep, bit-crunch bleeps, a low thud
  zap: { dur: 1.2, fn: (t, out) => {
    const { tone, noise } = E();
    tone(t, { type: 'square', f: 3200, f2: 90, a: 0.005, dec: 0.45, vol: 0.09, out });
    tone(t, { type: 'sawtooth', f: 1600, f2: 60, a: 0.005, dec: 0.5, vol: 0.06, out, detune: 12 });
    for (let i = 0; i < 10; i++) tone(t + 0.42 + i * 0.035, { type: 'square', f: 200 + ((i * 733) % 2400), dec: 0.02, vol: 0.05, out });
    noise(t + 0.45, { type: 'bandpass', f: 4000, f2: 400, q: 3, dec: 0.4, vol: 0.18, out });
    tone(t + 0.45, { f: 90, f2: 35, dec: 0.5, vol: 0.4, out });
  } },
  // a stadium: crowd swell, claps, two whistles
  cheer: { dur: 2.0, fn: (t, out) => {
    const { noise, voice, tone } = E();
    noise(t, { type: 'bandpass', f: 1000, q: 0.7, a: 0.3, dec: 1.6, vol: 0.34, out });
    noise(t + 0.1, { type: 'bandpass', f: 2600, q: 1.1, a: 0.3, dec: 1.4, vol: 0.12, out });
    const woo = [[330, 5, 1], [800, 6, 0.4], [2400, 8, 0.1]];
    for (let i = 0; i < 8; i++) {
      const f = 230 + ((i * 97) % 240);
      voice(t + 0.05 + i * 0.1, { f, f2: f * 1.45, dur: 0.7 + (i % 3) * 0.15, a: 0.1, vol: 0.05, formants: woo, vib: [6, 25], out });
    }
    for (let i = 0; i < 26; i++) noise(t + 0.15 + i * 0.065 + ((i * 37) % 10) / 400, { type: 'bandpass', f: 1300 + ((i * 211) % 1500), q: 1.4, dec: 0.025, vol: 0.18, out });
    tone(t + 0.3, { f: 2700, f2: 2900, a: 0.02, dec: 0.18, vol: 0.06, out, vib: [30, 60] });
    tone(t + 0.55, { f: 2900, f2: 2400, a: 0.02, dec: 0.4, vol: 0.06, out, vib: [30, 60] });
  } },
  // one big temple gong with a swell
  gong: { dur: 2.2, fn: (t, out) => {
    const { tone, noise } = E();
    noise(t, { type: 'bandpass', f: 300, f2: 1800, q: 1, a: 0.25, dec: 0.05, vol: 0.12, out });
    const h = t + 0.28;
    noise(h, { type: 'lowpass', f: 500, dec: 0.1, vol: 0.35, out });
    [[1, 0.22], [1.48, 0.13], [2.09, 0.11], [2.76, 0.08], [3.43, 0.06], [4.12, 0.05], [5.4, 0.03]].forEach(([r, v], i) =>
      tone(h, { f: 82 * r, a: 0.01 + i * 0.012, dec: 1.85 - i * 0.12, vol: v, detune: i % 2 ? 7 : -7, out, vib: [3 + i * 0.4, 6] }));
    noise(h, { type: 'bandpass', f: 1800, q: 4, a: 0.05, dec: 1.2, vol: 0.05, out });
  } },
  // a record scratch back and forth, then a stop
  scratch: { dur: 1.3, fn: (t, out) => {
    const { ac } = E();
    const strokes = [[0, 0.11, 1], [0.12, 0.09, -1], [0.24, 0.11, 1], [0.36, 0.08, -1], [0.5, 0.32, 1]];
    for (const [at, d, dir] of strokes) {
      const s = ac.createBufferSource();
      s.buffer = E().noiseBuf;
      s.playbackRate.setValueAtTime(dir > 0 ? 0.4 : 1.6, t + at);
      s.playbackRate.linearRampToValueAtTime(dir > 0 ? 1.8 : 0.3, t + at + d);
      const bp = ac.createBiquadFilter();
      bp.type = 'bandpass';
      bp.Q.value = 2.2;
      bp.frequency.setValueAtTime(dir > 0 ? 500 : 2600, t + at);
      bp.frequency.exponentialRampToValueAtTime(dir > 0 ? 2800 : 450, t + at + d);
      const g = ac.createGain();
      g.gain.setValueAtTime(0.0001, t + at);
      g.gain.linearRampToValueAtTime(0.5, t + at + 0.01);
      g.gain.exponentialRampToValueAtTime(0.0001, t + at + d);
      s.connect(bp).connect(g).connect(out);
      s.start(t + at, 0.1);
      s.stop(t + at + d + 0.05);
      // the voice on the record, pitched by the hand
      const o = ac.createOscillator();
      o.type = 'sawtooth';
      o.frequency.setValueAtTime(dir > 0 ? 110 : 420, t + at);
      o.frequency.exponentialRampToValueAtTime(dir > 0 ? 460 : 95, t + at + d);
      const og = ac.createGain();
      og.gain.setValueAtTime(0.0001, t + at);
      og.gain.linearRampToValueAtTime(0.07, t + at + 0.01);
      og.gain.exponentialRampToValueAtTime(0.0001, t + at + d);
      const lp = ac.createBiquadFilter();
      lp.type = 'lowpass';
      lp.frequency.value = 1400;
      o.connect(lp).connect(og).connect(out);
      o.start(t + at);
      o.stop(t + at + d + 0.05);
    }
    E().tone(t + 0.86, { f: 160, f2: 40, dec: 0.3, vol: 0.3, out });
  } },
  // a chiptune K.O.: falling arpeggio, a noise burst and a two-note ending
  ko8bit: { dur: 1.6, fn: (t, out) => {
    const { tone, noise } = E();
    [96, 91, 88, 84, 79, 76, 72, 67].forEach((m, i) => tone(t + i * 0.045, { type: 'square', f: n(m), dec: 0.05, vol: 0.06, out }));
    noise(t + 0.38, { type: 'lowpass', f: 3000, f2: 200, dec: 0.3, vol: 0.25, rate: 0.3, out });
    tone(t + 0.75, { type: 'square', f: n(72), dec: 0.14, vol: 0.07, out });
    tone(t + 0.92, { type: 'square', f: n(79), dec: 0.55, vol: 0.07, out });
    tone(t + 0.92, { type: 'triangle', f: n(55), dec: 0.55, vol: 0.14, out });
  } },
  // two orchestra stabs: brass, strings and timpani at once
  orch: { dur: 1.9, fn: (t, out) => {
    const { brass, tone, noise } = E();
    for (const [at, root, d] of [[0, 62, 0.28], [0.36, 65, 1.2]]) {
      for (const m of [root - 12, root, root + 4, root + 7, root + 12]) brass(t + at, m, d, 0.11, out);
      for (const m of [root + 16, root + 19]) tone(t + at, { type: 'sawtooth', f: n(m), a: 0.01, dec: d, vol: 0.025, out, vib: [6, 12, 0.1] });
      tone(t + at, { f: 80, f2: 48, dec: d * 0.9 + 0.2, vol: 0.45, out });
      noise(t + at, { type: 'bandpass', f: 1200, q: 0.8, dec: 0.12, vol: 0.2, out });
    }
  } },
  // a lightning crack and a long rolling rumble
  thunder: { dur: 2.2, fn: (t, out) => {
    const { noise, tone } = E();
    noise(t, { type: 'highpass', f: 2500, dec: 0.08, vol: 0.45, rate: 1.2, out });
    noise(t + 0.02, { type: 'bandpass', f: 1400, f2: 300, q: 0.8, dec: 0.4, vol: 0.4, out });
    for (let i = 0; i < 6; i++) noise(t + 0.15 + i * 0.22, { type: 'lowpass', f: 420 - i * 40, a: 0.06, dec: 0.5, vol: 0.32 - i * 0.04, rate: 0.35, out });
    tone(t + 0.05, { f: 55, f2: 28, a: 0.05, dec: 1.8, vol: 0.4, out });
  } },
  // a choir chord rising under bells
  choir: { dur: 2.2, fn: (t, out) => {
    const { voice, tone, noise } = E();
    const aah = [[800, 8, 1], [1150, 10, 0.5], [2900, 12, 0.2]];
    const ooh = [[400, 8, 1], [800, 10, 0.4], [2600, 12, 0.1]];
    for (const m of [55, 59, 62, 67]) voice(t, { f: n(m), f2: n(m + 5), dur: 1.0, a: 0.25, vol: 0.05, formants: ooh, vib: [5, 14], out });
    for (const m of [60, 64, 67, 72]) voice(t + 0.95, { f: n(m), dur: 1.2, a: 0.08, vol: 0.06, formants: aah, vib: [5, 16], out });
    [84, 88, 91, 96].forEach((m, i) => tone(t + 0.95 + i * 0.09, { f: n(m), a: 0.005, dec: 1.0, vol: 0.06, out }));
    noise(t + 0.9, { type: 'highpass', f: 7000, a: 0.3, dec: 1.1, vol: 0.06, out });
  } },
  // a riser, a gap, then a sub-bass drop
  drop: { dur: 2.2, fn: (t, out) => {
    const { tone, noise } = E();
    noise(t, { type: 'bandpass', f: 400, f2: 6000, q: 2, a: 0.75, dec: 0.05, vol: 0.18, out });
    tone(t, { type: 'sawtooth', f: 110, f2: 880, a: 0.75, dec: 0.05, vol: 0.05, out });
    snareRoll(t + 0.25, out, 10, 0.05, 0.14);
    const h = t + 0.9;
    tone(h, { f: 70, f2: 32, a: 0.005, dec: 1.2, vol: 0.6, out });
    tone(h, { type: 'sawtooth', f: 55, f2: 40, a: 0.005, dec: 0.9, vol: 0.06, out, vib: [8, 40] });
    noise(h, { type: 'lowpass', f: 1600, dec: 0.12, vol: 0.4, out });
    noise(h, { type: 'highpass', f: 6000, dec: 0.9, vol: 0.1, out });
  } },
};

// ---------- catalog ----------
const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };
const ITEMS = [
  ['chime', 'Digivolution Chime', 'common', '✨', 'A rising arpeggio that lands on a bright chord.', true],
  ['jingle', 'Victory Jingle', 'common', '🏆', 'A short win tune with a drum hit.', true],
  ['zap', 'Delete Zap', 'common', '⚡', 'A falling zap, broken bleeps and a thud.', true],
  ['horn', 'Air Horn Fanfare', 'common', '📯', 'Three horn blasts over kick drums and a crash.'],
  ['cheer', 'Stadium Cheer', 'common', '🏟️', 'A crowd swell, claps and whistles.'],
  ['gong', 'Temple Gong', 'rare', '🔔', 'A swell into one huge gong hit.'],
  ['scratch', 'Record Scratch', 'rare', '💿', 'Wicka-wicka, then the record stops.'],
  ['ko8bit', '8-Bit K.O.', 'rare', '👾', 'A chiptune knockout.'],
  ['orch', 'Orchestra Hit', 'epic', '🎻', 'Two full-orchestra stabs and timpani.'],
  ['thunder', 'Thunder Crash', 'epic', '🌩️', 'A lightning crack and a rolling rumble.'],
  ['choir', 'Holy Ascension', 'legendary', '😇', 'A choir rises into bells.'],
  ['drop', 'Bass Drop', 'legendary', '🔊', 'A riser, a snare roll and a sub-bass drop.'],
];
const clipId = (id) => `es_${id}`;
for (const [id, c] of Object.entries(STINGERS)) DT.Sfx.defineClip(clipId(id), c.fn, c.dur);

const stats = { played: 0, muted: 0, preview: 0 };
function previewButton(el, id, icon) {
  el.innerHTML = `<div class="es-wrap"><span class="es-ico">${icon || '♪'}</span><button type="button" class="sb-prev es-prev" title="Play" data-elim-play="${id}">▶ Play</button></div>`;
  el.querySelector('button').onclick = (ev) => { ev.stopPropagation(); ev.preventDefault(); DT.Sfx.engine.ensure(); play(id, { mine: true, preview: true }); };
}

const items = ITEMS.map(([id, name, rarity, icon, desc, free]) => ({
  id: `es_${id}`, name, rarity, icon, desc, price: free ? 0 : PRICE[rarity], ...(free ? { free: true } : {}),
  data: { clip: clipId(id), dur: STINGERS[id].dur, stinger: true },
  preview(el) { previewButton(el, `es_${id}`, icon); },
}));
// every soundboard clip works as an elimination sound too, owned with the soundboard clip
for (const s of DT.Cosmetics.list('sound')) {
  items.push({
    id: `sb_${s.id}`, name: s.name, rarity: s.rarity, icon: s.icon, desc: `${s.desc} Soundboard clip: yours when you own it on the soundboard.`,
    price: 0, egg: false, linked: ['sound', s.id], tag: 'Clip',
    data: { clip: (s.data && s.data.clip) || s.id, dur: (s.data && s.data.dur) || 1.5 },
    preview(el) { previewButton(el, `sb_${s.id}`, s.icon); },
  });
}
DT.Cosmetics.register('elimsound', items);

const DEFAULT = items[0].id;
function resolve(id) { return id && DT.Cosmetics.get('elimsound', id) ? id : DEFAULT; }

// mine: the local player's own sound (never muted by "mute others"); preview: the Collection / Store button
function play(id, opts = {}) {
  const it = DT.Cosmetics.get('elimsound', resolve(id));
  if (!it) return false;
  if (!opts.mine && !opts.preview && DT.Sfx.settings.muteOthers) { stats.muted++; return false; }
  const ok = DT.Sfx.clip(it.data.clip, !!opts.preview, opts.delay || 0);
  if (ok) {
    if (opts.preview) stats.preview++; else stats.played++;
    const k = `elim:${it.id}`;
    DT.Sfx.stats[k] = (DT.Sfx.stats[k] || 0) + 1;
    DT.Sfx.stats.elim = (DT.Sfx.stats.elim || 0) + 1;
  }
  return ok;
}

const css = `
.es-wrap { display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 6px; height: 100%; }
.es-ico { font-size: 30px; line-height: 1; filter: drop-shadow(0 4px 10px rgba(0,0,0,0.6)); }
.es-prev { padding: 4px 12px; font-size: 13px; }
.cprev-big .es-ico, .dts-prev.lg .es-ico { font-size: 96px; }
.cprev-big .es-prev, .dts-prev.lg .es-prev { padding: 10px 26px; font-size: 18px; }
`;
const st = document.createElement('style');
st.id = 'es-style';
st.textContent = css;
document.head.appendChild(st);

DT.ElimSounds = { play, resolve, stats, DEFAULT, stingers: Object.keys(STINGERS).map((k) => `es_${k}`) };
