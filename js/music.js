// Background music: two original tracks sequenced with WebAudio (planning and combat) plus a victory jingle,
// or the player's own audio files (kept in this browser's IndexedDB, never uploaded).
// audio.js forwards DT.Sfx.music(on) -> want(on) and DT.Sfx.combat(on) -> phase('combat' | 'planning').
//
// Notation: a line is space-separated tokens NOTE.LEN with LEN in 16th notes ("F#5.4" is a quarter note),
// "_.LEN" is a rest and "|" marks a bar line (checked, not required). Chords: one per bar separated by "|",
// two names in one bar split it in halves. Drums, bass and comp name a pattern below, or an array of
// pattern names per bar.
const DT = window.DT;
const E = () => DT.Sfx.engine;
const note = (n) => 440 * Math.pow(2, (n - 69) / 12);

// ---------- the tracks ----------
const TRACK_DEFS = {
  planning: {
    name: "Tamer's Morning",
    bpm: 152,
    lead: { osc: [['sawtooth', -7, 1], ['sawtooth', 7, 1], ['square', -1200, 0.35]], cut: 3200, vol: 0.1, q: 3 },
    counter: 'bell',
    delay: 3, // 16ths
    form: ['intro', 'verse', 'pre', 'chorus', 'bridge', 'chorus', 'intro'],
    loopFrom: 1,
    sections: {
      intro: {
        chords: 'G | A | F#m | Bm',
        lead: 'A4.2 D5.2 E5.2 F#5.4 E5.2 D5.2 E5.2 | F#5.3 E5.3 C#5.2 E5.8 | A4.2 D5.2 E5.2 F#5.4 A5.4 B5.2 | A5.3 F#5.3 E5.2 F#5.8',
        counter: 'arp8', drums: 'rock', bass: 'push', comp: 'push',
      },
      verse: {
        chords: 'D | A | Bm | G | D | A | G | A',
        lead: '_.2 F#4.2 F#4.2 A4.2 _.2 F#4.2 E4.2 D4.2 | E4.4 _.2 C#4.2 E4.2 F#4.2 E4.4 | _.2 F#4.2 F#4.2 B4.2 _.2 A4.2 F#4.2 D4.2 | E4.6 D4.2 B3.8 | '
          + '_.2 F#4.2 A4.2 D5.2 _.2 C#5.2 B4.2 A4.2 | C#5.4 B4.2 A4.2 E4.8 | _.2 D4.2 E4.2 G4.2 B4.4 A4.2 G4.2 | A4.12 _.4',
        leadShift: 12,
        counter: '_.48 | _.8 G5.2 F#5.2 D5.4 | _.48 | _.8 A5.2 G5.2 E5.4',
        drums: 'rock', bass: 'eighth', comp: 'stab8',
      },
      pre: {
        chords: 'Em | F#m | G | A',
        lead: 'B4.2 B4.2 A4.2 B4.2 _.2 E5.4 D5.2 | C#5.2 C#5.2 B4.2 C#5.2 _.2 F#5.4 E5.2 | D5.2 D5.2 C#5.2 D5.2 G5.4 F#5.2 E5.2 | E5.4 F#5.4 G5.4 A5.4',
        counter: 'arp8', drums: 'build', bass: 'eighth', comp: 'hold',
      },
      chorus: {
        chords: 'G | A | F#m | Bm | G | A | D | D',
        lead: 'B5.4 A5.2 F#5.2 _.2 D5.2 E5.2 F#5.2 | E5.6 C#5.2 E5.4 A5.4 | A5.4 F#5.2 E5.2 _.2 C#5.2 E5.2 F#5.2 | F#5.6 D5.2 B4.8 | '
          + 'B5.4 A5.2 F#5.2 _.2 D5.2 E5.2 G5.2 | A5.6 B5.2 C#6.4 A5.4 | D6.8 A5.4 F#5.4 | D5.12 _.4',
        counter: '_.8 G4.2 B4.2 D5.4 | _.8 A4.2 C#5.2 E5.4 | _.8 F#4.2 A4.2 C#5.4 | _.4 D5.2 C#5.2 B4.4 F#4.4 | '
          + '_.8 G4.2 B4.2 D5.4 | _.8 E5.2 F#5.2 G5.4 | F#5.4 E5.4 D5.4 A4.4 | F#4.8 A4.8',
        drums: 'four', bass: 'push', comp: 'push',
      },
      bridge: {
        chords: 'Bm | G | D | A | Bm | G | Em | A',
        lead: 'F#5.8 D5.4 B4.4 | D5.8 B4.4 G4.4 | A4.8 F#4.4 A4.4 | C#5.8 E5.8 | F#5.8 D5.4 F#5.4 | G5.8 F#5.4 D5.4 | E5.4 G5.4 B5.4 A5.4 | A5.12 _.4',
        counter: 'arp8', drums: 'half', bass: 'half', comp: 'pad',
      },
    },
  },
  combat: {
    name: 'Digital World Showdown',
    bpm: 172,
    lead: { osc: [['square', 0, 0.8], ['sawtooth', 10, 0.8], ['sawtooth', 1200, 0.22]], cut: 4200, vol: 0.085, q: 4 },
    counter: 'pluck',
    delay: 2,
    form: ['intro', 'A', 'B', 'chorus', 'break', 'A', 'chorus'],
    loopFrom: 1,
    sections: {
      intro: {
        chords: 'Em | Em | C | D',
        lead: 'E4.2 E4.1 E4.1 G4.2 E4.2 A4.2 E4.2 B4.2 A4.2 | E4.2 E4.1 E4.1 G4.2 E4.2 D5.2 B4.2 A4.2 G4.2 | E4.2 E4.1 E4.1 G4.2 E4.2 A4.2 E4.2 C5.2 B4.2 | B4.2 C5.2 D5.4 F#5.8',
        leadShift: 12,
        drums: ['hats', 'hats', 'drive', 'drive'], bass: 'drive', comp: 'stab8',
      },
      A: {
        chords: 'Em | C | D | B | Em | C | Am | B',
        lead: 'B4.3 B4.3 B4.2 G4.2 A4.2 B4.4 | C5.3 B4.3 A4.2 G4.4 E4.4 | A4.3 A4.3 A4.2 F#4.2 G4.2 A4.4 | B4.6 D#5.2 F#5.8 | '
          + 'E5.3 E5.3 E5.2 D5.2 B4.2 G4.4 | A4.3 G4.3 E4.2 G4.4 C5.4 | C5.3 B4.3 A4.2 E5.4 C5.4 | B4.8 A4.2 B4.2 D#5.4',
        counter: 'arp16', drums: 'drive', bass: 'drive', comp: 'stab8',
      },
      B: {
        chords: 'C | D | Em | Em | C | D | B | B',
        lead: 'G5.6 F#5.2 E5.8 | F#5.6 E5.2 D5.8 | E5.4 B4.4 G4.4 B4.4 | E5.16 | G5.6 A5.2 B5.8 | A5.6 G5.2 F#5.8 | F#5.4 D#5.4 B4.4 D#5.4 | F#5.16',
        drums: ['half', 'half', 'half', 'half', 'drive', 'drive', 'drive', 'drive'],
        bass: ['half', 'half', 'half', 'half', 'eighth', 'eighth', 'eighth', 'eighth'],
        comp: 'hold',
      },
      chorus: {
        chords: 'Em | C | G | D | Em | C | D | B',
        lead: 'E5.2 F#5.2 G5.4 B5.6 A5.2 | G5.3 E5.3 C5.2 E5.8 | D5.2 E5.2 G5.4 D6.6 B5.2 | A5.6 F#5.2 D5.8 | '
          + 'E5.2 F#5.2 G5.4 B5.6 A5.2 | G5.3 A5.3 B5.2 C6.8 | B5.3 A5.3 F#5.2 D5.4 F#5.4 | B5.8 A5.4 F#5.4',
        counter: '_.8 G5.6 F#5.2 | E5.6 C5.2 G4.8 | _.8 B5.6 G5.2 | F#5.6 D5.2 A4.8 | _.8 G5.6 F#5.2 | E5.4 F#5.4 A5.8 | G5.4 F#5.4 D5.8 | D#5.8 F#5.8',
        drums: 'blast', bass: 'gallop', comp: 'stab8',
      },
      break: {
        chords: 'Am | Em | C | B',
        lead: '_.32 | _.8 G5.4 A5.4 | B5.12 _.4',
        counter: 'arp16', drums: 'half', bass: 'half', comp: 'pad',
      },
    },
  },
};

// ---------- notation parsing ----------
const PC = { C: 0, D: 2, E: 4, F: 5, G: 7, A: 9, B: 11 };
function midiOf(s) {
  const m = /^([A-G])([#b]?)(\d)$/.exec(s);
  if (!m) throw new Error(`music: bad note ${s}`);
  return 12 * (+m[3] + 1) + PC[m[1]] + (m[2] === '#' ? 1 : m[2] === 'b' ? -1 : 0);
}
function parseLine(str, where) {
  const ev = [];
  let pos = 0;
  for (const tok of str.trim().split(/\s+/)) {
    if (tok === '|') { if (pos % 16) console.warn(`music: ${where} bar line at step ${pos}`); continue; }
    const [n, len] = tok.split('.');
    const l = +len;
    if (n !== '_') ev.push({ at: pos, n: midiOf(n), len: l });
    pos += l;
  }
  return { ev, len: pos };
}
const QUAL = { '': [0, 4, 7], m: [0, 3, 7], 7: [0, 4, 7, 10], m7: [0, 3, 7, 10], maj7: [0, 4, 7, 11], sus4: [0, 5, 7], sus2: [0, 2, 7], dim: [0, 3, 6], 5: [0, 7] };
function chordOf(s) {
  const m = /^([A-G])([#b]?)(.*)$/.exec(s);
  if (!m || !QUAL[m[3]]) throw new Error(`music: bad chord ${s}`);
  return { pc: (PC[m[1]] + (m[2] === '#' ? 1 : m[2] === 'b' ? -1 : 0) + 12) % 12, iv: QUAL[m[3]], name: s };
}
const pick = (v, b) => (Array.isArray(v) ? v[b % v.length] : v);
function bucket(line, b, shift = 0) {
  const out = new Array(16);
  if (!line) return out;
  for (const e of line.ev) {
    if (e.at < b * 16 || e.at >= b * 16 + 16) continue;
    const i = e.at - b * 16;
    (out[i] = out[i] || []).push({ n: e.n + shift, len: e.len });
  }
  return out;
}
// Flatten a track into bars: { s (section), first, last, chords, lead[16], counter[16], drums, bass, comp }
function compile(id, tr) {
  const secs = {};
  for (const [name, s] of Object.entries(tr.sections)) {
    const chordBars = s.chords.split('|').map((b) => b.trim().split(/\s+/).map(chordOf));
    const where = `${id}.${name}`;
    const lead = s.lead ? parseLine(s.lead, `${where}.lead`) : null;
    const isArp = /^arp/.test(s.counter || '');
    const counter = s.counter && !isArp ? parseLine(s.counter, `${where}.counter`) : null;
    if (lead && lead.len !== chordBars.length * 16) console.warn(`music: ${where} lead is ${lead.len} steps, expected ${chordBars.length * 16}`);
    if (counter && counter.len !== chordBars.length * 16) console.warn(`music: ${where} counter is ${counter.len} steps`);
    secs[name] = { ...s, name, chordBars, leadEv: lead, counterEv: counter, arp: isArp ? s.counter : null };
  }
  const bars = [];
  const loopBar = { at: 0 };
  tr.form.forEach((name, fi) => {
    const s = secs[name];
    if (fi === tr.loopFrom) loopBar.at = bars.length;
    for (let b = 0; b < s.chordBars.length; b++) {
      bars.push({
        s, first: b === 0, last: b === s.chordBars.length - 1, chords: s.chordBars[b],
        lead: bucket(s.leadEv, b, s.leadShift || 0), counter: bucket(s.counterEv, b),
        drums: pick(s.drums, b), bass: pick(s.bass, b), comp: pick(s.comp, b),
      });
    }
  });
  return { id, ...tr, bars, loopBar: loopBar.at, resume: 0, seconds: bars.length * 16 * (15 / tr.bpm) };
}
const TRACKS = {};
for (const [id, tr] of Object.entries(TRACK_DEFS)) TRACKS[id] = compile(id, tr);

// ---------- patterns ----------
// drums: k kick, s snare, h hats (c closed, o open)
const DRUMS = {
  rock: { k: 'x.......x.x.....', s: '....x.......x...', h: 'c.c.c.c.c.c.c.c.' },
  four: { k: 'x...x...x...x...', s: '....x.......x...', h: 'c.o.c.o.c.o.c.o.' },
  half: { k: 'x.........x.....', s: '........x.......', h: 'c...c...c...c...' },
  build: { k: 'x...x...x...x...', s: '....x.......x...', h: 'c.c.c.c.c.c.c.c.' },
  drive: { k: 'x.....x.x.....x.', s: '....x.......x...', h: 'cccccccccccccccc' },
  blast: { k: 'x.x.x.x.x.x.x.x.', s: '....x.......x...', h: 'c.c.c.c.c.c.c.c.' },
  hats: { k: 'x...............', s: '................', h: 'c.c.c.c.c.c.c.c.' },
};
// bass: step -> [semitones above the chord root, length in 16ths, accent 0..1] or null
const GALLOP = new Set([0, 2, 3, 4, 6, 7, 8, 10, 11, 12, 14, 15]);
const BASS = {
  eighth: (st) => (st % 2 ? null : [st === 14 ? 12 : 0, 2, st % 8 === 0 ? 1 : 0.5]),
  push: (st) => ({ 0: [0, 3, 1], 3: [0, 3, 0.6], 6: [0, 2, 0.6], 8: [0, 2, 1], 10: [0, 2, 0.6], 12: [7, 2, 0.6], 14: [12, 2, 0.8] })[st] || null,
  drive: (st) => [st === 6 || st === 14 ? 12 : 0, 1, st % 4 === 0 ? 1 : 0.4],
  gallop: (st) => (GALLOP.has(st) ? [0, st % 4 === 0 ? 2 : 1, st % 4 === 0 ? 1 : 0.5] : null),
  half: (st, split) => (st === 0 ? [0, 8, 1] : st === 8 ? [split ? 0 : 7, 8, 0.7] : null),
  walk: (st) => (st % 4 ? null : [[0, 7, 12, 7][st / 4], 4, 0.7]),
};
// comp: step -> [length in 16ths, accent] or null; 'hold' and 'pad' sound once per chord
const COMP = {
  stab8: (st) => (st % 2 ? null : [1.5, st % 8 === 0 ? 1 : 0.7]),
  push: (st) => ({ 0: [3, 1], 3: [3, 0.7], 6: [4, 0.8], 10: [2, 0.7], 12: [4, 0.8] })[st] || null,
};

// ---------- shared effects: a generated-impulse reverb and a feedback delay ----------
let fx = null;
function attach() {
  const { ac, musicBus } = E();
  if (fx || !ac || !musicBus) return;
  const len = Math.floor(ac.sampleRate * 2.2);
  const ir = ac.createBuffer(2, len, ac.sampleRate);
  for (let c = 0; c < 2; c++) {
    const d = ir.getChannelData(c);
    for (let i = 0; i < len; i++) d[i] = (Math.random() * 2 - 1) * Math.pow(1 - i / len, 3.2);
  }
  const rev = ac.createConvolver();
  rev.buffer = ir;
  const revIn = ac.createGain();
  const revOut = ac.createGain();
  revOut.gain.value = 0.32;
  revIn.connect(rev).connect(revOut).connect(musicBus);
  const delayIn = ac.createGain();
  const delay = ac.createDelay(1.5);
  delay.delayTime.value = 0.3;
  const fb = ac.createGain();
  fb.gain.value = 0.3;
  const damp = ac.createBiquadFilter();
  damp.type = 'lowpass';
  damp.frequency.value = 2600;
  const delayOut = ac.createGain();
  delayOut.gain.value = 0.45;
  delayIn.connect(delay);
  delay.connect(damp).connect(fb).connect(delay);
  delay.connect(delayOut);
  delayOut.connect(musicBus);
  delayOut.connect(revIn);
  // soft clipper for the rhythm guitar
  const curve = new Float32Array(1024);
  for (let i = 0; i < curve.length; i++) { const x = (i / (curve.length - 1)) * 2 - 1; curve[i] = (9 * x) / (1 + 8 * Math.abs(x)); }
  fx = { revIn, delayIn, delay, curve };
}

// ---------- instruments ----------
let ac = null; // set while scheduling
function gainNode(v, out) { const g = ac.createGain(); g.gain.value = v; if (out) g.connect(out); return g; }
function osc(type, f, t, end, det, out) {
  const o = ac.createOscillator();
  o.type = type;
  o.frequency.value = f;
  if (det) o.detune.value = det;
  o.connect(out);
  o.start(t);
  o.stop(end);
  return o;
}
function mnoise(t, { type = 'highpass', f = 7000, q = 0.7, dec = 0.05, vol = 0.1, out }) {
  const s = ac.createBufferSource();
  s.buffer = E().noiseBuf;
  const flt = ac.createBiquadFilter();
  flt.type = type;
  flt.frequency.value = f;
  flt.Q.value = q;
  const g = ac.createGain();
  g.gain.setValueAtTime(vol, t);
  g.gain.exponentialRampToValueAtTime(0.0001, t + dec);
  s.connect(flt).connect(g).connect(out);
  s.start(t, Math.random() * 0.8);
  s.stop(t + dec + 0.02);
}

function lead(p, t, n, d) {
  const P = p.tr.lead;
  const f = note(n);
  const end = t + d + 0.25;
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(P.vol, t + 0.012);
  g.gain.setTargetAtTime(P.vol * 0.7, t + 0.03, 0.1);
  g.gain.setTargetAtTime(0.0001, t + d, 0.045);
  g.connect(p.leadBus);
  const flt = ac.createBiquadFilter();
  flt.type = 'lowpass';
  flt.Q.value = P.q;
  flt.frequency.setValueAtTime(P.cut * 0.25, t);
  flt.frequency.linearRampToValueAtTime(P.cut, t + 0.025);
  flt.frequency.setTargetAtTime(P.cut * 0.5, t + 0.04, 0.18);
  flt.connect(g);
  const oscs = P.osc.map(([type, det, lvl]) => osc(type, f, t, end, det, lvl === 1 ? flt : gainNode(lvl, flt)));
  if (d > 0.3) {
    const lfo = ac.createOscillator();
    lfo.frequency.value = 5.6;
    const lg = ac.createGain();
    lg.gain.setValueAtTime(0, t);
    lg.gain.setValueAtTime(0, t + 0.15);
    lg.gain.linearRampToValueAtTime(14, t + 0.4);
    lfo.connect(lg);
    for (const o of oscs) lg.connect(o.detune);
    lfo.start(t);
    lfo.stop(end);
  }
}
function counter(p, t, n, d, lvl = 1) {
  const f = note(n);
  const g = ac.createGain();
  g.connect(p.counterBus);
  if (p.tr.counter === 'pluck') {
    const dec = Math.min(0.6, d + 0.1);
    g.gain.setValueAtTime(0.0001, t);
    g.gain.linearRampToValueAtTime(0.07 * lvl, t + 0.004);
    g.gain.exponentialRampToValueAtTime(0.0001, t + dec);
    const flt = ac.createBiquadFilter();
    flt.type = 'lowpass';
    flt.frequency.setValueAtTime(3200, t);
    flt.frequency.exponentialRampToValueAtTime(700, t + dec);
    flt.connect(g);
    osc('square', f, t, t + dec + 0.05, 0, flt);
    osc('triangle', f * 2, t, t + dec + 0.05, 4, flt);
    return;
  }
  const dec = Math.min(1.3, d + 0.3);
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(0.09 * lvl, t + 0.005);
  g.gain.exponentialRampToValueAtTime(0.0001, t + dec);
  osc('sine', f, t, t + dec + 0.05, 0, g);
  osc('sine', f * 2, t, t + dec + 0.05, 3, gainNode(0.3, g));
  osc('triangle', f * 3, t, t + 0.25, 0, gainNode(0.06, g));
}
function bass(p, t, n, d, acc) {
  const f = note(n);
  const end = t + d + 0.15;
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(0.2 * (0.75 + 0.25 * acc), t + 0.006);
  g.gain.setTargetAtTime(0.0001, t + d, 0.03);
  g.connect(p.bassBus);
  const flt = ac.createBiquadFilter();
  flt.type = 'lowpass';
  flt.Q.value = 5;
  flt.frequency.setValueAtTime(260 + 1100 * acc, t);
  flt.frequency.setTargetAtTime(260, t + 0.01, 0.07);
  flt.connect(g);
  osc('sawtooth', f, t, end, 0, flt);
  osc('sine', f, t, end, 0, gainNode(0.8, g));
}
function power(p, t, notes, d, acc) {
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(0.055 * acc, t + 0.005);
  g.gain.setTargetAtTime(0.0001, t + d, 0.025);
  g.connect(p.compIn);
  for (const n of notes) { osc('sawtooth', note(n), t, t + d + 0.2, -5, g); osc('sawtooth', note(n), t, t + d + 0.2, 5, g); }
}
function pad(p, t, notes, d) {
  const g = ac.createGain();
  g.gain.setValueAtTime(0.0001, t);
  g.gain.linearRampToValueAtTime(0.025, t + 0.3);
  g.gain.setTargetAtTime(0.0001, t + d, 0.25);
  g.connect(p.padBus);
  for (const n of notes) for (const det of [-9, 9]) osc('triangle', note(n), t, t + d + 1.2, det, g);
}
function kick(p, t, v) {
  const o = ac.createOscillator();
  o.frequency.setValueAtTime(150, t);
  o.frequency.exponentialRampToValueAtTime(42, t + 0.12);
  const g = ac.createGain();
  g.gain.setValueAtTime(0.75 * v, t);
  g.gain.exponentialRampToValueAtTime(0.0001, t + 0.32);
  o.connect(g).connect(p.drums);
  o.start(t);
  o.stop(t + 0.34);
  mnoise(t, { type: 'highpass', f: 3000, dec: 0.012, vol: 0.12 * v, out: p.drums });
  // sidechain-style pump on everything but the drums
  const dk = p.duck.gain;
  dk.setTargetAtTime(0.5, t, 0.005);
  dk.setTargetAtTime(1, t + 0.04, 0.09);
}
function snare(p, t, v) {
  mnoise(t, { type: 'bandpass', f: 1900, q: 0.7, dec: 0.17, vol: 0.32 * v, out: p.drums });
  mnoise(t, { type: 'highpass', f: 6000, dec: 0.08, vol: 0.1 * v, out: p.drums });
  const o = ac.createOscillator();
  o.type = 'triangle';
  o.frequency.setValueAtTime(205, t);
  o.frequency.exponentialRampToValueAtTime(140, t + 0.08);
  const g = ac.createGain();
  g.gain.setValueAtTime(0.2 * v, t);
  g.gain.exponentialRampToValueAtTime(0.0001, t + 0.1);
  o.connect(g).connect(p.drums);
  o.start(t);
  o.stop(t + 0.12);
}
function hat(p, t, open, v) { mnoise(t, { type: 'highpass', f: 7500, dec: open ? 0.2 : 0.035, vol: (open ? 0.05 : 0.06) * v, out: p.drums }); }
function crash(p, t) {
  mnoise(t, { type: 'highpass', f: 4500, dec: 1.6, vol: 0.11, out: p.drums });
  mnoise(t, { type: 'bandpass', f: 8500, q: 0.5, dec: 1.0, vol: 0.06, out: p.drums });
}
function tom(p, t, f) {
  const o = ac.createOscillator();
  o.frequency.setValueAtTime(f, t);
  o.frequency.exponentialRampToValueAtTime(f * 0.55, t + 0.2);
  const g = ac.createGain();
  g.gain.setValueAtTime(0.32, t);
  g.gain.exponentialRampToValueAtTime(0.0001, t + 0.24);
  o.connect(g).connect(p.drums);
  o.start(t);
  o.stop(t + 0.26);
}

// voicings
const wrap = (pc, lo) => lo + ((pc - lo) % 12 + 12) % 12; // the pitch class pc in [lo, lo + 12)
const bassRoot = (ch) => wrap(ch.pc, 33); // A1..G#2 (the sine doubles it, so it reads on small speakers)
const powerNotes = (ch) => { const r = wrap(ch.pc, 40); return [r, r + 7, r + 12]; };
const padNotes = (ch) => ch.iv.slice(0, 3).map((iv) => wrap(ch.pc + iv, 55));
const arpNotes = (ch) => { const ns = ch.iv.slice(0, 3).map((iv) => wrap(ch.pc + iv, 67)).sort((a, b) => a - b); return [...ns, ns[0] + 12]; };
const ARP = [0, 1, 2, 3, 2, 1];

// ---------- player: one running track with its own mix bus ----------
class Player {
  constructor(tr, bar, t, fadeIn) {
    this.tr = tr;
    this.bar = bar;
    this.step = 0;
    this.t = t;
    this.s16 = 15 / tr.bpm;
    this.arpK = 0;
    this.stopAt = Infinity;
    const { musicBus } = E();
    this.out = ac.createGain();
    this.out.gain.setValueAtTime(0.0001, t);
    this.out.gain.linearRampToValueAtTime(1, t + fadeIn);
    this.out.connect(musicBus);
    this.duck = gainNode(1, this.out);
    this.drums = gainNode(1, this.out);
    this.leadBus = gainNode(1, this.duck);
    this.leadBus.connect(gainNode(0.28, fx.delayIn));
    this.leadBus.connect(gainNode(0.22, fx.revIn));
    const cpan = ac.createStereoPanner();
    cpan.pan.value = 0.35;
    cpan.connect(this.duck);
    this.counterBus = gainNode(1, cpan);
    this.counterBus.connect(gainNode(0.35, fx.revIn));
    this.bassBus = gainNode(1, this.duck);
    const shaper = ac.createWaveShaper();
    shaper.curve = fx.curve;
    const tone = ac.createBiquadFilter();
    tone.type = 'lowpass';
    tone.frequency.value = 2400;
    const gpan = ac.createStereoPanner();
    gpan.pan.value = -0.3;
    this.compIn = gainNode(1, shaper);
    shaper.connect(tone).connect(gainNode(0.5, gpan));
    gpan.connect(this.duck);
    this.padBus = gainNode(1, this.duck);
    this.padBus.connect(gainNode(0.5, fx.revIn));
    fx.delay.delayTime.setTargetAtTime(this.s16 * tr.delay, t, 0.2);
  }
  // fade out and stop scheduling; the next visit starts at the following section
  fade(t, dur) {
    if (this.stopAt !== Infinity) return;
    const g = this.out.gain;
    g.cancelScheduledValues(t);
    g.setValueAtTime(g.value, t);
    g.linearRampToValueAtTime(0.0001, t + dur);
    this.stopAt = t + dur;
    const bars = this.tr.bars;
    let b = this.bar + 1;
    while (b < bars.length && !bars[b].first) b++;
    this.tr.resume = b >= bars.length ? this.tr.loopBar : b;
  }
  schedule(until) {
    while (this.t < until && this.t < this.stopAt) {
      this.play(this.t);
      this.t += this.s16;
      if (++this.step === 16) {
        this.step = 0;
        if (++this.bar >= this.tr.bars.length) this.bar = this.tr.loopBar;
      }
    }
  }
  play(t) {
    const bar = this.tr.bars[this.bar];
    const st = this.step;
    const s16 = this.s16;
    const split = bar.chords.length > 1;
    const ch = bar.chords[split && st >= 8 ? 1 : 0];
    const chordStart = st === 0 || (split && st === 8);
    const chordLeft = (split && st < 8 ? 8 : 16) - st;
    for (const e of bar.lead[st] || []) lead(this, t, e.n, e.len * s16 * 0.92);
    for (const e of bar.counter[st] || []) counter(this, t, e.n, e.len * s16);
    if (bar.s.arp) {
      const every = bar.s.arp === 'arp16' ? 1 : 2;
      if (st % every === 0) { const ns = arpNotes(ch); counter(this, t, ns[ARP[this.arpK++ % ARP.length]], every * s16, every === 1 ? 0.55 : 0.7); }
    }
    const bp = BASS[bar.bass];
    const bh = bp && bp(st, split);
    if (bh) bass(this, t, bassRoot(ch) + bh[0], bh[1] * s16 * 0.9, bh[2]);
    if (bar.comp === 'hold' && chordStart) power(this, t, powerNotes(ch), chordLeft * s16 * 0.95, 0.8);
    else if (bar.comp === 'pad' && chordStart) pad(this, t, padNotes(ch), chordLeft * s16);
    else if (COMP[bar.comp]) { const c = COMP[bar.comp](st); if (c) power(this, t, powerNotes(ch), c[0] * s16, c[1]); }
    const D = DRUMS[bar.drums];
    if (!D) return;
    if (bar.first && st === 0 && bar.s.crash !== false) crash(this, t);
    if (bar.last && st >= 8 && bar.s.fill !== false) {
      // fill into the next section: toms, then a snare run that gets louder
      if (st === 8) { kick(this, t, 1); tom(this, t, 220); } else if (st === 10 || st === 11) tom(this, t, st === 10 ? 170 : 150);
      else if (st >= 12) snare(this, t, 0.55 + (st - 12) * 0.15);
      return;
    }
    if (D.k[st] === 'x') kick(this, t, 1);
    if (D.s[st] === 'x') snare(this, t, 1);
    if (D.h[st] !== '.') hat(this, t, D.h[st] === 'o', st % 4 === 0 ? 1 : 0.65);
  }
}

// ---------- victory jingle (played by DT.Sfx.play('champion') on the effects bus) ----------
function jingle(t, out) {
  const e = E();
  const s = 0.1; // 16th at 150 BPM
  const mel = parseLine('A4.2 D5.2 F#5.2 A5.6 G5.2 A5.2 | B5.4 C#6.4 D6.8', 'jingle').ev;
  for (const m of mel) e.brass(t + m.at * s, m.n, m.at >= 24 ? 1.4 : m.len * s * 0.9, 0.22, out);
  const harm = [[0, [50, 54, 57]], [16, [55, 59, 62]], [24, [57, 61, 64]], [32, [50, 57, 62, 66]]];
  for (const [at, ns] of harm) for (const n of ns) e.tone(t + at * s, { type: 'triangle', f: note(n), a: 0.02, dec: at === 32 ? 1.6 : 0.75, vol: 0.05, out });
  for (const [at, n] of [[0, 38], [16, 43], [24, 45], [32, 38]]) e.tone(t + at * s, { type: 'sawtooth', f: note(n), dec: at === 32 ? 1.2 : 0.7, vol: 0.09, out });
  for (const at of [0, 8, 16, 24, 32]) e.tone(t + at * s, { f: 140, f2: 45, dec: 0.3, vol: 0.4, out });
  for (const at of [28, 29, 30, 31]) e.noise(t + at * s, { type: 'bandpass', f: 1900, q: 0.7, dec: 0.12, vol: 0.12 + (at - 28) * 0.05, out });
  e.noise(t + 32 * s, { type: 'highpass', f: 4500, dec: 1.8, vol: 0.16, out });
}

// ---------- the player's own files (IndexedDB) ----------
const idb = {
  p: null,
  open() {
    if (!this.p) {
      this.p = new Promise((res, rej) => {
        if (!window.indexedDB) return rej(new Error('IndexedDB is not available'));
        const r = indexedDB.open('dt-music', 1);
        r.onupgradeneeded = () => r.result.createObjectStore('tracks', { keyPath: 'id' });
        r.onsuccess = () => { const db = r.result; db.onversionchange = () => { db.close(); this.p = null; }; res(db); };
        r.onerror = () => rej(r.error);
      });
      this.p.catch(() => { this.p = null; });
    }
    return this.p;
  },
  async run(mode, fn) {
    const db = await this.open();
    return new Promise((res, rej) => {
      const tx = db.transaction('tracks', mode);
      const req = fn(tx.objectStore('tracks'));
      tx.oncomplete = () => res(req ? req.result : undefined);
      tx.onerror = () => rej(tx.error);
      tx.onabort = () => rej(tx.error);
    });
  },
  all() { return this.run('readonly', (s) => s.getAll()); },
  put(rec) { return this.run('readwrite', (s) => s.put(rec)); },
  del(id) { return this.run('readwrite', (s) => s.delete(id)); },
  clear() { return this.run('readwrite', (s) => s.clear()); },
};
const AUDIO_EXT = /\.(mp3|ogg|oga|m4a|aac|wav|flac|opus|webm)$/i;
const fits = (rec, ph) => rec.use === 'both' || rec.use === ph;

const own = {
  list: [],
  loaded: false,
  slots: [], // two { el, gain, url } for crossfades
  active: -1,
  rec: null,
  playing: false,
  bags: {},
  fails: 0,
  async load() {
    try { this.list = (await idb.all()).sort((a, b) => a.added - b.added); } catch (e) { console.warn('music: could not read saved files', e); this.list = []; }
    this.loaded = true;
    changed();
    if (cur.wanted) refresh();
  },
  ensureSlots() {
    if (this.slots.length) return;
    for (let i = 0; i < 2; i++) {
      const el = new Audio();
      el.preload = 'auto';
      const gain = ac.createGain();
      gain.gain.value = 0;
      ac.createMediaElementSource(el).connect(gain).connect(E().musicBus);
      const slot = { el, gain, url: null };
      el.addEventListener('ended', () => { if (this.slots[this.active] === slot && this.playing) this.next(0.2); });
      el.addEventListener('error', () => { if (this.slots[this.active] === slot && this.playing) this.failed(); });
      this.slots.push(slot);
    }
  },
  // shuffled bag per phase, so every file plays once before any repeats
  pick(ph) {
    const pool = this.list.filter((r) => fits(r, ph));
    const from = pool.length ? pool : this.list;
    if (!from.length) return null;
    let bag = (this.bags[ph] || []).filter((id) => from.some((r) => r.id === id));
    if (!bag.length) {
      bag = from.map((r) => r.id);
      for (let i = bag.length - 1; i > 0; i--) { const j = Math.floor(Math.random() * (i + 1)); [bag[i], bag[j]] = [bag[j], bag[i]]; }
      if (bag.length > 1 && this.rec && bag[0] === this.rec.id) bag.push(bag.shift());
    }
    const id = bag.shift();
    this.bags[ph] = bag;
    return from.find((r) => r.id === id);
  },
  playRec(rec, fade) {
    if (!rec) return;
    this.ensureSlots();
    const t = ac.currentTime;
    const old = this.slots[this.active];
    if (old) {
      old.gain.gain.cancelScheduledValues(t);
      old.gain.gain.setValueAtTime(old.gain.gain.value, t);
      old.gain.gain.linearRampToValueAtTime(0, t + fade);
      const o = old;
      setTimeout(() => { if (this.slots[this.active] !== o) { o.el.pause(); if (o.url) { URL.revokeObjectURL(o.url); o.url = null; o.el.removeAttribute('src'); } } }, fade * 1000 + 100);
    }
    this.active = (this.active + 1) % 2;
    const s = this.slots[this.active];
    if (s.url) URL.revokeObjectURL(s.url);
    s.url = URL.createObjectURL(rec.blob);
    s.el.src = s.url;
    s.gain.gain.cancelScheduledValues(t);
    s.gain.gain.setValueAtTime(0, t);
    s.gain.gain.linearRampToValueAtTime(1, t + fade);
    this.rec = rec;
    const pr = s.el.play();
    if (pr) pr.then(() => { this.fails = 0; }).catch((e) => { if (e && e.name !== 'AbortError') this.failed(); });
    changed();
  },
  failed() {
    console.warn('music: could not play', this.rec && this.rec.name);
    if (++this.fails >= Math.max(3, this.list.length)) { this.stop(0.1); this.error = 'Your files could not be played in this browser.'; changed(); return; }
    this.next(0.1);
  },
  start(ph) {
    if (this.playing || !this.list.length) return;
    this.playing = true;
    this.error = null;
    this.playRec(this.pick(ph), 1);
  },
  next(fade = 0.8) { if (this.playing) this.playRec(this.pick(cur.phase), fade); },
  phase(ph) {
    if (!this.playing || !this.rec || fits(this.rec, ph)) return;
    if (!this.list.some((r) => fits(r, ph))) return;
    this.playRec(this.pick(ph), 1.5);
  },
  stop(fade = 1) {
    if (!this.playing) return;
    this.playing = false;
    const s = this.slots[this.active];
    if (s) {
      const t = ac.currentTime;
      s.gain.gain.cancelScheduledValues(t);
      s.gain.gain.setValueAtTime(s.gain.gain.value, t);
      s.gain.gain.linearRampToValueAtTime(0, t + fade);
      setTimeout(() => { if (!this.playing) s.el.pause(); }, fade * 1000 + 100);
    }
    this.rec = null;
    changed();
  },
};

// ---------- control ----------
const cur = { wanted: false, phase: 'planning' };
let players = [];
const starts = []; // recent track starts, for tests
let timer = null;
const listeners = new Set();
function changed() { for (const fn of listeners) { try { fn(); } catch (e) { console.warn(e); } } }

const useOwn = () => DT.Sfx.settings.source === 'own' && own.list.length > 0;
function shouldPlay() {
  const S = DT.Sfx.settings;
  return cur.wanted && !S.muted && !S.musicOff && S.music > 0.001;
}
const live = () => players.find((p) => p.stopAt === Infinity);

function tick() {
  if (!ac || ac.state !== 'running') return;
  const now = ac.currentTime;
  for (const p of players) {
    if (p.t < now - 0.1) p.t = now + 0.05; // fell behind (tab was throttled): skip ahead instead of bursting notes
    p.schedule(now + 0.25);
  }
  const done = players.filter((p) => now > p.stopAt + 0.6);
  if (done.length) {
    for (const p of done) p.out.disconnect();
    players = players.filter((p) => !done.includes(p));
  }
  if (!players.length && !own.playing) { clearInterval(timer); timer = null; }
}
function startTimer() { if (!timer && !document.hidden) timer = setInterval(tick, 50); }

function startTrack(id, fade) {
  attach();
  const t = ac.currentTime + 0.06;
  for (const p of players) p.fade(t, fade);
  const tr = TRACKS[id];
  players.push(new Player(tr, tr.resume, t, fade));
  starts.push({ track: id, bar: tr.resume, fade, crossfade: players.length > 1 });
  if (starts.length > 20) starts.shift();
  startTimer();
  tick();
  changed();
}
function stopBuiltin(fade) {
  if (!ac) return;
  const t = ac.currentTime + 0.02;
  for (const p of players) p.fade(t, fade);
  changed();
}

function refresh() {
  const e = E();
  if (!shouldPlay()) { if (e.ac) { stopBuiltin(1); own.stop(1); } return; }
  e.ensure();
  ac = e.ac;
  if (!ac || ac.state !== 'running') return; // waits for a user gesture
  if (useOwn()) {
    if (players.some((p) => p.stopAt === Infinity)) stopBuiltin(1);
    own.start(cur.phase);
    startTimer();
  } else {
    own.stop(1);
    if (!live()) startTrack(cur.phase, 1.2);
  }
}
const kick0 = () => setTimeout(refresh, 0);
window.addEventListener('pointerdown', kick0, true);
window.addEventListener('keydown', kick0, true);
DT.Sfx.onChange((s, patch) => { if ('muted' in patch || 'musicOff' in patch || 'music' in patch || 'source' in patch) refresh(); });

DT.Music = {
  TRACKS,
  attach() { ac = E().ac; attach(); },
  visibility(hidden) {
    if (hidden) {
      clearInterval(timer);
      timer = null;
      const s = own.slots[own.active];
      if (s && own.playing) s.el.pause();
    } else {
      if (players.length || own.playing) startTimer();
      const s = own.slots[own.active];
      if (s && own.playing) s.el.play().catch(() => {});
    }
  },
  want(on) {
    if (on && !cur.wanted) for (const tr of Object.values(TRACKS)) tr.resume = 0; // a new game starts each track from the top
    cur.wanted = on;
    if (!on) cur.phase = 'planning';
    refresh();
  },
  phase(ph) {
    if (ph === cur.phase) return;
    cur.phase = ph;
    if (own.playing) own.phase(ph);
    else if (live() && ac) startTrack(ph, 1.5);
    changed();
  },
  jingle(t, out) { ac = E().ac; jingle(t, out); },
  get playing() { return !!live() || own.playing; },
  state() {
    const p = live();
    const bar = p && p.tr.bars[p.bar];
    return {
      wanted: cur.wanted, phase: cur.phase, playing: this.playing,
      source: own.playing ? 'own' : p ? 'builtin' : null,
      track: own.playing ? (own.rec && own.rec.name) : p ? p.tr.id : null,
      title: own.playing ? (own.rec && own.rec.name) : p ? p.tr.name : null,
      section: bar ? bar.s.name : null,
      players: players.length,
      starts: starts.slice(-4),
      error: own.error || null,
    };
  },
  onChange(fn) { listeners.add(fn); return () => listeners.delete(fn); },
  own: {
    get list() { return own.list.map(({ blob, ...r }) => r); },
    get loaded() { return own.loaded; },
    async add(files) {
      const added = [];
      const skipped = [];
      for (const f of files) {
        if (!/^audio\//.test(f.type) && !AUDIO_EXT.test(f.name)) { skipped.push(`${f.name} (not an audio file)`); continue; }
        const rec = { id: `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`, name: f.name.replace(/\.[^.]+$/, ''), size: f.size, type: f.type, use: 'both', added: Date.now(), blob: f };
        try { await idb.put(rec); own.list.push(rec); added.push(rec.name); } catch (e) {
          skipped.push(`${f.name} (${e && e.name === 'QuotaExceededError' ? 'browser storage is full' : 'could not be saved'})`);
        }
      }
      if (added.length && DT.Sfx.settings.source !== 'own') DT.Sfx.set({ source: 'own' });
      else refresh();
      changed();
      return { added, skipped };
    },
    async remove(id) {
      await idb.del(id).catch((e) => console.warn(e));
      const wasCur = own.rec && own.rec.id === id;
      own.list = own.list.filter((r) => r.id !== id);
      if (!own.list.length) { own.stop(0.5); DT.Sfx.set({ source: 'builtin' }); } else if (wasCur) own.next(0.5);
      changed();
    },
    async clear() {
      await idb.clear().catch((e) => console.warn(e));
      own.list = [];
      own.stop(0.5);
      DT.Sfx.set({ source: 'builtin' });
      changed();
    },
    async setUse(id, use) {
      const r = own.list.find((x) => x.id === id);
      if (!r) return;
      r.use = use;
      await idb.put(r).catch((e) => console.warn(e));
      if (own.playing) own.phase(cur.phase);
      changed();
    },
    next() { own.next(0.6); },
  },
};
own.load();
