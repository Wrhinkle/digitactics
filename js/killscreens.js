// Kill screens: when a Tamer is knocked out, the Tamer whose board dealt the last damage gets a short cinematic.
// Client-only. Runs after the lockstep 'end' entry is applied; every client has the same results, so all agree on the killer.
// PvP loss: the killer is the opponent (for a ghost match, the ghost's real owner). PvE loss: no killer, a neutral sting.
// The killer's whole board team dances behind their Little Legend, and the killer's elimination sound plays.
// The victim and the killer see the big version over the arena; everyone else gets a small card in the corner.
// It never pauses anything: click it or press Esc to skip. Several eliminations in one round play one after another.
import * as THREE from 'three';
import { Moves } from './moveprims.js';
import { makePlaceholder, lib, UnitView } from './render3d.js';

const DT = window.DT;
const DUR_BIG = 4.0;
const DUR_KILLCAM = 3.5;
const DUR_SMALL = 2.8;
const DUR_STING = 2.0;
const FADE = 0.3;

// ---------- small helpers ----------
const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
// stable pseudo-random number in [0, 1) for particle i, channel k
const R = (i, k = 0) => { const x = Math.sin(i * 127.1 + k * 311.7) * 43758.5453; return x - Math.floor(x); };
const clamp01 = (x) => Math.max(0, Math.min(1, x));
const easeOut = (x) => 1 - Math.pow(1 - clamp01(x), 3);
const ordinal = (n) => `${n}${n % 100 >= 11 && n % 100 <= 13 ? 'th' : ['th', 'st', 'nd', 'rd'][n % 10] || 'th'}`;
function fillBg(c, w, h, a, b) {
  const g = c.createLinearGradient(0, 0, 0, h);
  g.addColorStop(0, a);
  g.addColorStop(1, b);
  c.fillStyle = g;
  c.fillRect(0, 0, w, h);
}
function glow(c, x, y, r, col, alpha = 1) {
  const g = c.createRadialGradient(x, y, 0, x, y, Math.max(1, r));
  g.addColorStop(0, col);
  g.addColorStop(1, 'rgba(0,0,0,0)');
  c.globalAlpha = alpha;
  c.fillStyle = g;
  c.fillRect(x - r, y - r, r * 2, r * 2);
  c.globalAlpha = 1;
}
function poly(c, x, y, r, n, rot) {
  c.beginPath();
  for (let i = 0; i <= n; i++) { const a = rot + (i / n) * Math.PI * 2; c[i ? 'lineTo' : 'moveTo'](x + Math.cos(a) * r, y + Math.sin(a) * r); }
}

// ---------- themes: draw(c, w, h, t) paints the background at t seconds; pure functions of t so previews match ----------
const THEMES = {
  basic: {
    rim: '#4fb3ff', text: '#bfe2ff',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#0b1830', '#04070d');
      for (let i = 0; i < 18; i++) {
        const y = R(i, 1) * h;
        const len = 60 + R(i, 5) * w * 0.25;
        const x = ((R(i, 2) * w + t * (180 + R(i, 3) * 420)) % (w + len)) - len;
        c.fillStyle = `rgba(80,170,255,${0.12 + 0.3 * R(i, 4)})`;
        c.fillRect(x, y, len, 1 + R(i, 6) * 3);
      }
      c.fillStyle = 'rgba(0,0,0,0.18)';
      for (let y = 0; y < h; y += 3) c.fillRect(0, y, w, 1);
    },
  },
  gate: {
    rim: '#8ff3ff', text: '#dffbff',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#06182a', '#020509');
      const cx = w / 2, cy = h * 0.45, m = Math.min(w, h);
      const open = easeOut(t / 0.9);
      glow(c, cx, cy, m * (0.25 + 0.45 * open), 'rgba(160,250,255,0.9)', 0.35 + 0.4 * open);
      c.lineWidth = Math.max(1.5, m * 0.008);
      for (let k = 0; k < 4; k++) {
        c.strokeStyle = `rgba(140,240,255,${0.75 - k * 0.15})`;
        poly(c, cx, cy, m * (0.16 + 0.09 * k) * (0.6 + 0.4 * open), 6, t * (k % 2 ? 0.7 : -0.5) + k);
        c.stroke();
      }
      // gate halves sliding apart
      const gap = open * m * 0.2;
      c.fillStyle = 'rgba(4,20,34,0.85)';
      c.fillRect(cx - gap - m * 0.12, cy - m * 0.24, m * 0.12, m * 0.48);
      c.fillRect(cx + gap, cy - m * 0.24, m * 0.12, m * 0.48);
      for (let i = 0; i < 70; i++) {
        const a = R(i, 1) * Math.PI * 2;
        const d = (1 - ((R(i, 2) + t * (0.3 + R(i, 3) * 0.4)) % 1)) * m * 0.8;
        c.fillStyle = `rgba(200,255,255,${0.3 + R(i, 4) * 0.6})`;
        const s = 2 + R(i, 5) * 4;
        c.fillRect(cx + Math.cos(a) * d - s / 2, cy + Math.sin(a) * d * 0.7 - s / 2, s, s);
      }
    },
  },
  courage: {
    rim: '#ffb347', text: '#ffe2b0',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#2a0d02', '#0d0300');
      const cx = w / 2, cy = h * 0.45, m = Math.max(w, h);
      const n = 26;
      for (let i = 0; i < n; i++) {
        const a = (i / n) * Math.PI * 2 + t * 0.25;
        c.fillStyle = i % 2 ? 'rgba(255,140,30,0.22)' : 'rgba(255,210,90,0.16)';
        c.beginPath();
        c.moveTo(cx, cy);
        c.arc(cx, cy, m, a, a + Math.PI / n);
        c.fill();
      }
      const pulse = 1 + 0.08 * Math.sin(t * 9);
      const r = Math.min(w, h) * 0.13 * easeOut(t / 0.5) * pulse;
      glow(c, cx, cy, r * 3.2, 'rgba(255,190,80,1)', 0.7);
      c.fillStyle = '#ffd46a';
      for (let i = 0; i < 8; i++) {
        const a = (i / 8) * Math.PI * 2 - t * 0.6;
        c.beginPath();
        c.moveTo(cx + Math.cos(a - 0.22) * r, cy + Math.sin(a - 0.22) * r);
        c.lineTo(cx + Math.cos(a) * r * 1.7, cy + Math.sin(a) * r * 1.7);
        c.lineTo(cx + Math.cos(a + 0.22) * r, cy + Math.sin(a + 0.22) * r);
        c.fill();
      }
      c.beginPath(); c.arc(cx, cy, r, 0, Math.PI * 2); c.fill();
      for (let i = 0; i < 60; i++) {
        const a = R(i, 1) * Math.PI * 2;
        const d = ((R(i, 2) + t * (0.4 + R(i, 3) * 0.5)) % 1) * Math.min(w, h) * 0.9;
        c.fillStyle = `rgba(255,${180 + R(i, 4) * 70 | 0},90,${1 - d / (Math.min(w, h) * 0.9)})`;
        c.fillRect(cx + Math.cos(a) * d, cy + Math.sin(a) * d, 3, 3);
      }
    },
  },
  glitch: {
    rim: '#ff4fd8', text: '#ffd0f6',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#14041e', '#040108');
      const step = Math.floor(t * 14);
      for (let i = 0; i < 46; i++) {
        const k = step * 64 + i;
        c.fillStyle = R(k, 1) < 0.5 ? `rgba(0,240,255,${0.15 + R(k, 2) * 0.35})` : `rgba(255,40,200,${0.15 + R(k, 2) * 0.35})`;
        c.fillRect(R(k, 3) * w, R(k, 4) * h, 10 + R(k, 5) * w * 0.3, 2 + R(k, 6) * h * 0.06);
      }
      // deletion bar sweeping down
      const y = ((t * 0.55) % 1) * h;
      c.fillStyle = 'rgba(255,255,255,0.12)';
      c.fillRect(0, y, w, h * 0.04);
      c.fillStyle = 'rgba(0,0,0,0.25)';
      for (let yy = 0; yy < h; yy += 2) c.fillRect(0, yy, w, 1);
    },
  },
  pillar: {
    rim: '#fff3b0', text: '#fffbe6',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#0c1230', '#03040c');
      const cx = w / 2, grow = easeOut(t / 0.6);
      const bw = w * 0.16 * grow;
      const g = c.createLinearGradient(cx - bw, 0, cx + bw, 0);
      g.addColorStop(0, 'rgba(255,240,170,0)');
      g.addColorStop(0.5, 'rgba(255,250,225,0.85)');
      g.addColorStop(1, 'rgba(255,240,170,0)');
      c.fillStyle = g;
      c.fillRect(cx - bw, 0, bw * 2, h);
      c.strokeStyle = 'rgba(255,240,180,0.6)';
      c.lineWidth = 2;
      for (let k = 0; k < 4; k++) {
        const p = (t * 0.45 + k / 4) % 1;
        c.globalAlpha = 1 - p;
        c.beginPath();
        c.ellipse(cx, h * (0.95 - p * 0.9), w * (0.08 + 0.12 * p), h * 0.03, 0, 0, Math.PI * 2);
        c.stroke();
      }
      c.globalAlpha = 1;
      for (let i = 0; i < 90; i++) {
        const x = cx + (R(i, 1) - 0.5) * w * 0.6 * (0.3 + R(i, 2));
        const y = h - ((R(i, 3) * h + t * (60 + R(i, 4) * 160)) % h);
        c.fillStyle = `rgba(255,245,200,${0.25 + R(i, 5) * 0.7})`;
        c.fillRect(x, y, 2, 2 + R(i, 6) * 4);
      }
    },
  },
  ocean: {
    rim: '#9aa8c8', text: '#d8def0',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#0e121c', '#05060a');
      glow(c, w * 0.75, h * 0.2, Math.min(w, h) * 0.22, 'rgba(210,215,230,0.55)');
      c.fillStyle = 'rgba(225,228,238,0.85)';
      c.beginPath(); c.arc(w * 0.75, h * 0.2, Math.min(w, h) * 0.06, 0, Math.PI * 2); c.fill();
      const cols = ['#1a2030', '#161b28', '#121622', '#0e111a', '#0a0c13', '#07080d'];
      for (let k = 0; k < cols.length; k++) {
        const base = h * (0.5 + k * 0.09);
        c.fillStyle = cols[k];
        c.beginPath();
        c.moveTo(0, h);
        for (let x = 0; x <= w; x += 8) {
          const y = base + Math.sin(x * (0.012 + k * 0.003) + t * (0.9 + k * 0.25) + k) * h * 0.025 + Math.sin(x * 0.031 - t * 1.7 + k * 2) * h * 0.01;
          c.lineTo(x, y);
        }
        c.lineTo(w, h);
        c.fill();
      }
      c.fillStyle = 'rgba(150,160,190,0.06)';
      c.fillRect(0, h * 0.45, w, h * 0.12);
    },
  },
  firewall: {
    rim: '#ff7a2a', text: '#ffd9b8',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#2a0705', '#090202');
      const cols = 12, rows = 7, bw = w / cols, bh = h / rows;
      const brk = 0.55, cx = w / 2, cy = h * 0.45;
      if (t > brk && t < brk + 0.3) { c.fillStyle = `rgba(255,200,120,${0.6 * (1 - (t - brk) / 0.3)})`; c.fillRect(0, 0, w, h); }
      for (let r = 0; r < rows; r++) for (let q = 0; q < cols; q++) {
        const x = q * bw + (r % 2 ? bw / 2 : 0) - bw / 4, y = r * bh;
        const k = r * cols + q;
        let dx = 0, dy = 0, rot = 0, a = 1;
        if (t > brk) {
          const e = t - brk;
          const vx = (x + bw / 2 - cx) * (1.2 + R(k, 1)), vy = (y + bh / 2 - cy) * (1.2 + R(k, 2));
          dx = vx * e; dy = vy * e + 300 * e * e; rot = (R(k, 3) - 0.5) * 6 * e; a = clamp01(1 - e / 1.6);
        }
        if (a <= 0) continue;
        c.save();
        c.globalAlpha = a;
        c.translate(x + bw / 2 + dx, y + bh / 2 + dy);
        c.rotate(rot);
        c.fillStyle = `rgba(${170 + R(k, 4) * 60 | 0},${40 + R(k, 5) * 40 | 0},10,0.85)`;
        c.fillRect(-bw / 2 + 2, -bh / 2 + 2, bw - 4, bh - 4);
        c.strokeStyle = `rgba(255,170,60,${t < brk ? 0.5 + 0.5 * Math.sin(t * 20 + k) : 0.8})`;
        c.lineWidth = 1.5;
        c.strokeRect(-bw / 2 + 2, -bh / 2 + 2, bw - 4, bh - 4);
        c.restore();
      }
      glow(c, cx, cy, Math.min(w, h) * 0.5 * easeOut((t - brk) / 0.5), 'rgba(255,140,40,0.8)', t > brk ? 0.6 : 0);
    },
  },
  stream: {
    rim: '#5affb0', text: '#d0ffe8',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#02100a', '#000403');
      const cx = w / 2, cy = h * 0.45, m = Math.max(w, h) * 0.5;
      c.lineCap = 'round';
      for (let i = 0; i < 150; i++) {
        const a = R(i, 1) * Math.PI * 2;
        const z = (R(i, 2) + t * (0.5 + R(i, 3) * 0.6)) % 1;
        const d0 = m * 0.05 / (1.05 - z), d1 = d0 * (1.08 + z * 0.3);
        c.strokeStyle = R(i, 4) < 0.7 ? `rgba(90,255,170,${z})` : `rgba(160,240,255,${z})`;
        c.lineWidth = 1 + z * 2.5;
        c.beginPath();
        c.moveTo(cx + Math.cos(a) * d0, cy + Math.sin(a) * d0);
        c.lineTo(cx + Math.cos(a) * d1, cy + Math.sin(a) * d1);
        c.stroke();
      }
      glow(c, cx, cy, Math.min(w, h) * 0.18, 'rgba(160,255,210,0.7)', 0.6);
    },
  },
  mega: {
    rim: '#ff3a3a', text: '#ffd0d0', silhouette: true,
    draw(c, w, h, t) {
      fillBg(c, w, h, '#3a0606', '#0a0101');
      // lightning flashes at fixed times
      for (const ft of [0.25, 1.6, 2.7]) {
        const e = t - ft;
        if (e < 0 || e > 0.25) continue;
        c.fillStyle = `rgba(255,220,220,${0.35 * (1 - e / 0.25)})`;
        c.fillRect(0, 0, w, h);
        c.strokeStyle = `rgba(255,240,240,${1 - e / 0.25})`;
        c.lineWidth = 2;
        c.beginPath();
        let x = w * (0.15 + R(ft * 10, 1) * 0.7), y = 0;
        c.moveTo(x, y);
        for (let s = 0; s < 8; s++) { x += (R(ft * 10 + s, 2) - 0.5) * w * 0.08; y += h * 0.07; c.lineTo(x, y); }
        c.stroke();
      }
      c.fillStyle = '#120202';
      c.fillRect(0, h * 0.82, w, h * 0.18);
      for (let i = 0; i < 40; i++) {
        const x = R(i, 1) * w, y = h * 0.82 - ((R(i, 2) * h * 0.6 + t * (20 + R(i, 3) * 50)) % (h * 0.6));
        c.fillStyle = `rgba(255,90,60,${0.2 + R(i, 4) * 0.5})`;
        c.fillRect(x, y, 2, 2);
      }
    },
  },
  confetti: {
    rim: '#ffd84a', text: '#fff2c0',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#1c0b34', '#07030f');
      const sway = Math.sin(t * 0.8) * w * 0.05;
      const g = c.createLinearGradient(0, 0, 0, h);
      g.addColorStop(0, 'rgba(255,240,180,0.45)');
      g.addColorStop(1, 'rgba(255,240,180,0)');
      c.fillStyle = g;
      c.beginPath();
      c.moveTo(w / 2 - w * 0.04 + sway, 0); c.lineTo(w / 2 + w * 0.04 + sway, 0);
      c.lineTo(w / 2 + w * 0.22, h); c.lineTo(w / 2 - w * 0.22, h);
      c.fill();
      const cols = ['#ffd84a', '#ff5a8a', '#4fb3ff', '#5adf8a', '#e58bff', '#ffffff'];
      for (let i = 0; i < 130; i++) {
        const x = R(i, 1) * w + Math.sin(t * 2 + i) * 18;
        const y = ((R(i, 2) * h * 1.2 + t * (70 + R(i, 3) * 120)) % (h * 1.2)) - h * 0.1;
        c.save();
        c.translate(x, y);
        c.rotate(t * (2 + R(i, 4) * 4) + i);
        c.fillStyle = cols[i % cols.length];
        c.fillRect(-4, -2, 8 * Math.abs(Math.cos(t * 3 + i)) + 1, 4);
        c.restore();
      }
    },
  },
  // PvE elimination: no killer, no cosmetic
  sting: {
    rim: '#ff5a6a', text: '#ffd0d6',
    draw(c, w, h, t) {
      fillBg(c, w, h, '#24070c', '#080204');
      c.fillStyle = `rgba(255,90,106,${0.25 * (1 - clamp01(t / 0.6))})`;
      c.fillRect(0, 0, w, h);
      c.fillStyle = 'rgba(255,90,106,0.5)';
      const x = easeOut(t / 0.4) * w;
      c.fillRect(0, h * 0.5 - 1, x, 2);
    },
  },
};

// ---------- catalog ----------
const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };
const VARIANTS = [
  ['ks_basic', 'basic', 'Data Delete', 'common', 'Scrolling data lines. The default kill screen.', true],
  ['ks_gate', 'gate', 'Digital Gate', 'rare', 'A hexagonal gate opens behind your Tamer.'],
  ['ks_stream', 'stream', 'Hyper Stream', 'rare', 'A warp tunnel of green data.'],
  ['ks_glitch', 'glitch', 'Deletion Glitch', 'rare', 'Corrupted color blocks and a sweeping delete bar.'],
  ['ks_courage', 'courage', 'Crest Burst', 'epic', 'An orange sunburst with spinning rays.'],
  ['ks_pillar', 'pillar', 'Light Pillar', 'epic', 'A column of light with rising sparks.'],
  ['ks_ocean', 'ocean', 'Dark Ocean', 'epic', 'Grey moonlight over slow dark waves.'],
  ['ks_firewall', 'firewall', 'Firewall Shatter', 'legendary', 'A wall of burning tiles breaks apart.'],
  ['ks_mega', 'mega', 'Mega Silhouette', 'legendary', 'A giant shadow of your Digimon under a red sky.'],
  ['ks_confetti', 'confetti', 'Champion Confetti', 'legendary', 'Spotlight and confetti.'],
];
const ICON = { basic: '💾', gate: '🌀', stream: '💨', glitch: '📺', courage: '☀️', pillar: '🔆', ocean: '🌊', firewall: '🧱', mega: '👹', confetti: '🎉' };

const items = VARIANTS.map(([id, theme, name, rarity, desc, free]) => ({
  id, name, rarity, desc, price: PRICE[rarity], icon: ICON[theme], ...(free ? { free: true } : {}),
  data: { theme },
  preview: (el) => preview(el, theme, id),
}));
DT.Cosmetics.register('killscreen', items);
const themeOf = (id) => { const it = DT.Cosmetics.get('killscreen', id); return (it && it.data && THEMES[it.data.theme]) ? it.data.theme : 'basic'; };

// ---------- store preview: the looping background on a plain 2D canvas ----------
// Small cards: your Little Legend doing your dance (the sample portrait until it loads). Big previews (the
// Collection's detail pane, the Store dialog): your whole team dancing behind it, the board you have in a running
// game or a sample team, and a play button that runs the screen's sting with your elimination sound.
function preview(el, theme, id) {
  const cv = document.createElement('canvas');
  cv.className = 'ksPreview';
  cv.style.cssText = 'width:100%;height:100%;display:block;border-radius:inherit';
  el.appendChild(cv);
  const c = cv.getContext('2d');
  const T = THEMES[theme] || THEMES.basic;
  let t0 = performance.now(), last = 0;
  let seen = false, btn = null;
  const playAll = () => {
    t0 = performance.now();
    if (!DT.Sfx) return;
    DT.Sfx.engine.ensure();
    DT.Sfx.play('killscreen', { variant: id || 'ks_basic', big: true });
    if (DT.ElimSounds) DT.ElimSounds.play(DT.Cosmetics.equipped('elimsound'), { mine: true, preview: true, delay: 0.3 });
  };
  const addButton = () => {
    if (getComputedStyle(el).position === 'static') el.style.position = 'relative';
    const es = DT.Cosmetics.get('elimsound', DT.Cosmetics.equipped('elimsound'));
    btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'ksPrevPlay';
    btn.title = 'Play the kill screen with your elimination sound';
    btn.innerHTML = `&#9654; <span>${es ? `${es.icon || '&#9834;'} ${esc(es.name)}` : 'Play'}</span>`;
    btn.style.cssText = 'position:absolute;right:8px;top:8px;z-index:2;padding:4px 10px;font-size:12px;border-radius:6px;background:rgba(0,0,0,0.55);color:#fff;border:1px solid rgba(255,255,255,0.3);cursor:pointer';
    btn.onclick = (ev) => { ev.stopPropagation(); ev.preventDefault(); playAll(); };
    el.appendChild(btn);
  };
  const tick = (now) => {
    if (!cv.isConnected) { if (seen || now - t0 > 5000) return; requestAnimationFrame(tick); return; }
    seen = true;
    const w = Math.max(80, el.clientWidth || 200), h = Math.max(48, el.clientHeight || 112);
    const big = w >= 240 && h >= 120;
    // a dancing team is drawn at about 30 frames a second, plenty for a preview
    if (big && now - last < 30) { requestAnimationFrame(tick); return; }
    last = now;
    if (cv.width !== w || cv.height !== h) { cv.width = w; cv.height = h; }
    const t = ((now - t0) / 1000) % 3.5;
    T.draw(c, w, h, t);
    if (big && !btn) addButton();
    let danced = big && drawLineup(c, w, h, now / 1000, T);
    cv.dataset.lineup = danced ? '1' : '0';
    if (!danced) {
      // your Little Legend doing your equipped dance (js/dances.js)
      const ds = h * 0.95 * (0.85 + 0.15 * easeOut(t / 0.4));
      danced = DT.Dances && DT.Dances.drawInto(c, w / 2 - ds / 2, h * 0.98 - ds, ds, ds, { t, size: 192 });
    }
    cv.dataset.dance = danced ? '1' : '0';
    const url = !danced && DT.Portraits && DT.Portraits.get('agumon', 1);
    if (url) {
      if (!cv._img || cv._img.src.indexOf(url) < 0) { cv._img = new Image(); cv._img.src = url; }
      if (cv._img.complete && cv._img.naturalWidth) {
        const s = h * 0.75 * (0.85 + 0.15 * easeOut(t / 0.4));
        if (T.silhouette) { c.globalAlpha = 0.7; c.filter = 'brightness(0)'; c.drawImage(cv._img, w / 2 - s, h * 0.95 - s * 2, s * 2, s * 2); c.filter = 'none'; c.globalAlpha = 1; }
        c.drawImage(cv._img, w / 2 - s / 2, h * 0.92 - s, s, s);
      }
    }
    requestAnimationFrame(tick);
  };
  requestAnimationFrame(tick);
}

// ---------- who killed whom ----------
// Eliminations from the round that just ended, in elimination order (worst placement first).
function eliminations(game) {
  const out = [];
  for (const r of game.results || []) {
    const m = r.match;
    const add = (victim, killer, dmg) => {
      if (!victim || victim.alive) return;
      const team = victim === m.a || victim === m.a2 ? 1 : 0;
      const hit = !m.pve && dmg && m.combat.lastKills && m.combat.lastKills[team];
      const owners = team === 0 ? [m.a,m.a2] : [m.b,m.b2];
      const credited = hit && owners.find(p => p && p.id === hit.owner);
      out.push({ victim, killer: m.pve || !dmg ? null : credited || killer, ghost: !!m.ghost,
        finalHit: hit ? { ...hit } : null, victims: m.duo ? (team === 1 ? [m.a,m.a2] : [m.b,m.b2]) : [victim],
        killers: !m.pve && dmg ? owners.filter(Boolean) : [] });
    };
    add(m.a, m.b, r.teamDmgA || r.dmgA);
    if (m.b && !m.ghost) add(m.b, m.a, r.dmgB);
  }
  return out.sort((x, y) => (y.victim.placement || 0) - (x.victim.placement || 0));
}

const shown = new WeakMap(); // game -> Set of victim ids already queued
const queue = [];
let cur = null;

// Called by ui.js after a live 'end' entry is applied.
function onRoundEnd(game) {
  if (!game) return;
  let set = shown.get(game);
  if (!set) shown.set(game, (set = new Set()));
  for (const e of eliminations(game)) {
    if (set.has(e.victim.id)) continue;
    set.add(e.victim.id);
    // the killer's board as it is now, before the next round changes it
    const team = e.killer ? (e.killers.length ? e.killers : [e.killer]).flatMap(p => teamOf(p)) : [];
    const hit = e.finalHit || (team.length ? { ...team[0] } : null);
    if (hit) {
      let i = team.findIndex(u => (hit.uid != null && u.uid === hit.uid) || (u.line === hit.line && u.stage === hit.stage && (hit.owner == null || u.owner === hit.owner)));
      if (i < 0) { team.unshift({ ...hit }); i = 0; }
      const [honor] = team.splice(i, 1); team.unshift({ ...honor, finalHit: true });
    }
    queue.push({ ...e, game, finalHit: hit, team });
  }
  if (!cur) next();
}

// ---------- overlay ----------
const CSS = `
#ksLayer { position: fixed; z-index: 1200; overflow: hidden; cursor: pointer; opacity: 0; transition: opacity ${FADE}s; border: 1px solid rgba(255,255,255,0.12); box-shadow: 0 14px 50px rgba(0,0,0,0.6); }
#ksLayer.on { opacity: 1; }
#ksLayer canvas { position: absolute; inset: 0; width: 100%; height: 100%; }
#ksLayer .ksText { position: absolute; left: 0; right: 0; bottom: 9%; text-align: center; pointer-events: none; text-shadow: 0 2px 8px #000, 0 0 2px #000; }
#ksLayer .ksMain { font-size: 30px; font-weight: 800; letter-spacing: 0.5px; color: #fff; transform: translateY(18px); opacity: 0; transition: transform 0.4s 0.25s, opacity 0.4s 0.25s; }
#ksLayer .ksMain b { color: var(--ks-accent); }
#ksLayer .ksSub { font-size: 15px; font-weight: 600; margin-top: 4px; color: var(--ks-text); opacity: 0; transition: opacity 0.4s 0.55s; }
#ksLayer.on .ksMain { transform: none; opacity: 1; }
#ksLayer.on .ksSub { opacity: 1; }
#ksLayer .ksTag { position: absolute; right: 10px; top: 8px; font-size: 11px; color: rgba(255,255,255,0.55); pointer-events: none; }
#ksLayer .ksBar { position: absolute; left: 0; right: 0; height: 0; background: #000; transition: height 0.35s; pointer-events: none; }
#ksLayer.on .ksBar { height: 7%; }
#ksLayer .ksBar.t { top: 0; } #ksLayer .ksBar.b { bottom: 0; }
#ksLayer.small { border-radius: 10px; }
#ksLayer.small .ksMain { font-size: 15px; }
#ksLayer.small .ksSub { font-size: 11px; margin-top: 1px; }
#ksLayer.small .ksBar, #ksLayer.small .ksTag { display: none; }
#ksLayer.small .ksText { bottom: 6%; }
@media (max-width: 700px) { #ksLayer .ksMain { font-size: 20px; } }
`;
const killcamCSS = `
#ksLayer.killcam .ksText { bottom:15%; transform:skew(-7deg); }
#ksLayer.killcam .ksMain { font:900 clamp(26px,5vw,76px)/.95 sans-serif; text-transform:uppercase; color:white; text-shadow:4px 3px #062a46,-3px -2px var(--ks-accent),0 0 30px var(--ks-accent); }
#ksLayer.killcam .ksSub { text-transform:uppercase; letter-spacing:.22em; font-size:14px; }
#ksLayer .ksHonor { position:absolute; top:12%; left:0; right:0; text-align:center; color:#ffe8a2; font-size:13px; font-weight:800; letter-spacing:.14em; text-transform:uppercase; text-shadow:0 2px 5px #000; pointer-events:none; }
#ksLayer.killcam .ksHonor { display:none; }
`;
let layer = null, bg = null, ctx2d = null, glCanvas = null;
function ensureLayer() {
  if (layer) return;
  const st = document.createElement('style');
  st.textContent = CSS + killcamCSS;
  document.head.appendChild(st);
  layer = document.createElement('div');
  layer.id = 'ksLayer';
  layer.className = 'hidden';
  layer.innerHTML = '<canvas class="ksBg"></canvas><canvas class="ksGl"></canvas><div class="ksBar t"></div><div class="ksBar b"></div><div class="ksTag"></div><div class="ksHonor"></div><div class="ksText"><div class="ksMain"></div><div class="ksSub"></div></div>';
  document.body.appendChild(layer);
  bg = layer.querySelector('.ksBg');
  ctx2d = bg.getContext('2d');
  glCanvas = layer.querySelector('.ksGl');
  layer.addEventListener('pointerdown', (ev) => { ev.stopPropagation(); skip(); });
  window.addEventListener('keydown', (ev) => { if (cur && ev.key === 'Escape') skip(); });
}

// position: the big version covers a band across the arena, the small one sits in its lower-left corner
function place(big) {
  const a = document.getElementById('arena');
  const r = a ? a.getBoundingClientRect() : { left: 0, top: 0, width: innerWidth, height: innerHeight };
  let w, h, x, y;
  if (big) { w = innerWidth; h = innerHeight; x = 0; y = 0; }
  else { w = Math.min(320, r.width * 0.6); h = Math.round(w * 0.56); x = r.left + 12; y = r.top + r.height - h - 12; }
  Object.assign(layer.style, { left: `${x}px`, top: `${y}px`, width: `${w}px`, height: `${h}px` });
  return { w: Math.round(w), h: Math.round(h) };
}

// ---------- 3D: one shared renderer for the killer's team ----------
let gl = null, scene = null, camera = null, rimLight = null, camMoves = null;
function lights(sc) {
  sc.add(new THREE.HemisphereLight(0xffffff, 0x334455, 2.2));
  const key = new THREE.DirectionalLight(0xffffff, 2.2);
  key.position.set(2, 4, 5);
  sc.add(key);
  const rim = new THREE.DirectionalLight(0xffffff, 3);
  rim.position.set(-3, 2, -4);
  sc.add(rim);
  return rim;
}
function ensureGL() {
  if (gl) return true;
  try {
    gl = new THREE.WebGLRenderer({ canvas: glCanvas, alpha: true, antialias: true });
  } catch (e) { console.warn('Kill screen: no WebGL', e); gl = null; return false; }
  gl.setClearColor(0x000000, 0);
  gl.outputColorSpace = THREE.SRGBColorSpace;
  scene = new THREE.Scene();
  camera = new THREE.PerspectiveCamera(30, 16 / 9, 0.1, 60);
  rimLight = lights(scene);
  camMoves = new Moves({ scene, camera, canvas: glCanvas, renderer: gl, shake: () => {} });
  return true;
}

// The killer's team at the moment of the elimination: every Digimon on their board, strongest first.
// { line, stage } only, so a later round changing the board doesn't change the kill screen.
function teamOf(p) {
  if (!p || !p.boardUnits) return [];
  const val = (u) => (DT.unitValue && DT.LINE[u.line] ? DT.unitValue(u) : u.stage || 0) + (u.items ? u.items.length * 0.5 : 0);
  return p.boardUnits().slice().sort((a, b) => val(b) - val(a)).map((u) => ({ line: u.line, stage: u.stage || 0, uid: u.uid, owner: p.id }));
}
// Model for one Digimon: its board form (DT.modelKey), or the placeholder creature when it has no model.
function strongestKey(p) {
  const units = p.boardUnits().length ? p.boardUnits() : p.allUnits();
  let best = null;
  for (const u of units) if (!best || DT.unitValue(u) + u.items.length * 0.5 > DT.unitValue(best) + best.items.length * 0.5) best = u;
  return best ? keyOf(best) : 'patamon:0';
}
function keyOf(u) {
  const line = DT.LINE[u.line];
  return (line && DT.modelKey ? DT.modelKey(line, u.stage) : null) || `${u.line}:${u.stage}`;
}
const darken = (obj) => {
  const mat = new THREE.MeshBasicMaterial({ color: 0x1a0303 });
  obj.traverse((o) => { if (o.isMesh) o.material = mat; });
  return mat;
};

// A rig is { group, update(dt, t), dispose() }.
async function legendRig(id, dark, dance) {
  const L = DT.Legends;
  if (!(await L.modelFor(id))) return null;
  const v = L.createView(id, { ring: false, yaw: 0, height: 1 });
  await new Promise((r) => setTimeout(r, 0)); // the view attaches its model on the resolved promise
  const mat = dark ? darken(v.group) : null;
  // the killer's equipped dance on the legend's own bones (js/dances.js), timed from the start of the kill screen
  const drv = dance && DT.Dances ? DT.Dances.attachView(v, dance) : null;
  if (!drv) v.play('dance');
  let own = 0;
  return {
    group: v.group, legend: true,
    update: (dt, t) => { own += dt; v.update(dt); if (drv) drv.update(t != null ? t : own); },
    dispose: () => { if (drv) drv.dispose(); v.dispose(); if (mat) mat.dispose(); },
  };
}
// One Digimon of the team doing the dance: rigged models move their bones, static rips (Time Stranger, Masters,
// DW1) and the placeholder creature get the Dancer's procedural body motion.
async function memberRig(u, dance) {
  const key = keyOf(u);
  let rig = null;
  if (DT.Dances) {
    try { rig = await DT.Dances.unitRig(key, dance); } catch (e) { console.warn('Kill screen: unit dance failed', key, e); }
  }
  if (rig) return rig;
  const line = DT.LINE[u.line] || DT.LINE.patamon;
  const obj = line ? makePlaceholder(line) : new THREE.Group();
  const mats = [];
  obj.traverse((o) => { if (o.isMesh) mats.push(o.material); });
  const dispose = () => { for (const m of mats) m.dispose(); };
  if (DT.Dances && DT.Dances.objRig) { const r = DT.Dances.objRig(obj, dance, { dispose }); r.key = key; return r; }
  const group = new THREE.Group();
  group.add(obj);
  return { group, key, procedural: true, update: () => {}, dispose };
}

// ---------- the lineup ----------
// A single shallow arc on a shared floor, including large Duos teams and narrow previews. The camera fits the
// width instead of stacking members on floating risers. The final-hit Digimon gets the first spot by the legend.
const LEGEND_SLOT = { x: 0, y: 0, z: 0.3, s: 0.86 };
function lineupLayout(n, aspect = 16 / 9) {
  if (!n) return { slots: [], halfW: 0.6, top: 1.0 };
  const narrow = aspect < 1.3;
  const s = n <= 2 ? 0.86 : 0.8;
  const gap = s * 1.22;
  const slots = [];
  const inner = LEGEND_SLOT.s * 0.6 + gap * 0.5; // clear of the legend
  for (let i = 0; i < n; i++) {
    const side = i % 2 ? -1 : 1, k = Math.floor(i / 2);
    // an odd count puts its last Digimon on the right
    slots.push({ x: side * (inner + k * gap), y: 0, z: -0.12 - Math.min(0.35, k * 0.04), s, row: 0 });
  }
  const halfW = Math.max(0.6, ...slots.map((q) => Math.abs(q.x) + q.s * 0.55));
  const top = Math.max(1.0, ...slots.map((q) => q.y + q.s * 1.05));
  return { slots, halfW, top, s, narrow };
}

// Builds a lineup group. Members are added as their models arrive (templates are shared with the board renderer,
// so units already on screen load at once) and pop in one after another, all on the same beat.
function buildLineup({ team, legend, dance, aspect }) {
  const lay = lineupLayout(team.length, aspect);
  const L = { group: new THREE.Group(), members: [], dead: false, lay, legendGroup: null, expected: team.length + (legend ? 1 : 0), failed: 0, lastT: null };
  // Soft contact shadows stay on the shared floor while the dance itself can bounce or jump.
  const shadowCanvas = document.createElement('canvas'); shadowCanvas.width = shadowCanvas.height = 64;
  const ctx = shadowCanvas.getContext('2d'), gradient = ctx.createRadialGradient(32,32,3,32,32,32);
  gradient.addColorStop(0,'rgba(0,0,0,0.5)'); gradient.addColorStop(1,'rgba(0,0,0,0)');
  ctx.fillStyle = gradient; ctx.fillRect(0,0,64,64);
  const shadowMap = new THREE.CanvasTexture(shadowCanvas), shadowGeo = new THREE.PlaneGeometry(1,1);
  const shadowMat = new THREE.MeshBasicMaterial({map:shadowMap,transparent:true,depthWrite:false});
  const add = (rig, slot, order) => {
    if (!rig) { L.failed++; return; }
    if (L.dead) { rig.dispose(); return; }
    const holder = new THREE.Group();
    holder.position.set(slot.x, slot.y || 0, slot.z);
    holder.scale.setScalar(0.001);
    holder.add(rig.group);
    L.group.add(holder);
    const shadow = new THREE.Mesh(shadowGeo,shadowMat);
    shadow.rotation.x = -Math.PI/2; shadow.position.set(slot.x,0.005,slot.z);
    shadow.scale.set(slot.s*1.35,slot.s*0.9,1); L.group.add(shadow);
    L.members.push({ rig, holder, slot, order, born: null });
    if (rig.legend) L.legendGroup = holder;
  };
  if (legend && DT.Legends && DT.Legends.createView) {
    legendRig(legend, false, dance).then((r) => add(r, LEGEND_SLOT, 0)).catch((e) => { L.failed++; console.warn('Kill screen: legend model failed', e); });
  }
  const honor = team.findIndex(u => u.finalHit);
  if (honor >= 0) {
    const q = lay.slots[honor];
    const ring = new THREE.Mesh(new THREE.RingGeometry(.32,.48,48), new THREE.MeshBasicMaterial({color:'#ffe6a0', side:THREE.DoubleSide, transparent:true, opacity:.75}));
    ring.rotation.x = -Math.PI/2; ring.position.set(q.x,.015,q.z); L.group.add(ring); L.honorRing = ring;
  }
  team.forEach((u, i) => {
    memberRig(u, dance).then((r) => add(r, lay.slots[i], i + 1)).catch((e) => { L.failed++; console.warn('Kill screen: unit failed', e); });
  });
  // t: seconds since the kill screen started; the dance pose is a function of t, so every member is on the same beat
  L.update = (t) => {
    const dt = L.lastT == null ? 0 : Math.min(0.1, Math.max(0, t - L.lastT));
    L.lastT = t;
    for (const m of L.members) {
      if (m.born == null) m.born = Math.max(t, 0.06 + m.order * 0.07);
      m.rig.update(dt, t);
      const e = t - m.born;
      const k = e <= 0 ? 0.001 : e < 0.4 ? easeOut(e / 0.4) * (1 + 0.14 * Math.sin(Math.PI * clamp01(e / 0.4))) : 1;
      m.holder.scale.setScalar(Math.max(0.001, k * m.slot.s));
    }
  };
  L.dispose = () => {
    L.dead = true;
    for (const m of L.members) { freeSkeletons(m.holder); try { m.rig.dispose(); } catch (e) { console.warn(e); } }
    L.members.length = 0;
    if (L.honorRing) { L.honorRing.geometry.dispose(); L.honorRing.material.dispose(); }
    shadowGeo.dispose(); shadowMat.dispose(); shadowMap.dispose();
    L.group.clear();
  };
  return L;
}

// Cloned skinned meshes get their own skeleton, and three.js gives each skeleton a bone texture on first render.
// Free those with the clone (a template's own skeleton just rebuilds its texture on its next render).
function freeSkeletons(obj) {
  const seen = new Set();
  obj.traverse((o) => { if (o.isSkinnedMesh && o.skeleton && !seen.has(o.skeleton)) { seen.add(o.skeleton); o.skeleton.dispose(); } });
}

// Camera that fits the whole lineup: width from the layout, height from the layout and the legend (flying legends
// rise during their dance; only the legend is measured, a few times a second).
const box = new THREE.Box3();
const tmp = new THREE.Box3();
function frameLineup(cam, L, c, aspect, t, dt) {
  if (L.legendGroup && t - c.at > 0.15) {
    c.at = t;
    L.legendGroup.updateMatrixWorld(true);
    box.makeEmpty();
    L.legendGroup.traverse((o) => {
      if (!o.isMesh) return;
      if (o.isSkinnedMesh) o.computeBoundingBox(); else if (!o.geometry.boundingBox) o.geometry.computeBoundingBox();
      const b = o.isSkinnedMesh ? o.boundingBox : o.geometry.boundingBox;
      if (b) box.union(tmp.copy(b).applyMatrix4(o.matrixWorld));
    });
    if (!box.isEmpty()) c.legendTop = Math.max(1, box.max.y);
  }
  const top = Math.max(L.lay.top, c.legendTop || 1.05) + 0.08;
  const bottom = -0.05;
  const tanH = Math.tan(THREE.MathUtils.degToRad(cam.fov / 2));
  const halfW = L.lay.halfW + 0.25;
  // distance to the front of the lineup that fits the width, and the height with room for the caption under it
  const dW = halfW / (tanH * aspect);
  const dH = ((top - bottom) * 0.66) / tanH;
  const wantD = Math.max(c.min, dW, dH) + LEGEND_SLOT.z;
  // a tall frame is limited by width, which leaves spare height: stand the team on the lower part of the frame
  const wantY = L.lay.narrow ? bottom + (wantD - LEGEND_SLOT.z) * tanH * 0.6 : bottom + (top - bottom) * 0.4;
  const k = c.d == null ? 1 : Math.min(1, dt * 4);
  c.d = c.d == null ? wantD : c.d + (wantD - c.d) * k;
  c.y = c.y == null ? wantY : c.y + (wantY - c.y) * k;
  // A near-level view keeps everyone's feet near the same screen baseline.
  cam.position.set(0, c.y + 0.2 + c.d * 0.025, c.d);
  cam.lookAt(0, c.y, -0.35);
}

// The silhouette theme's giant shadow: just the legend (or the strongest Digimon), not the whole team.
async function silRig(ev) {
  const look = DT.Social && DT.Social.lookOf(ev.killer);
  if (DT.Legends && DT.Legends.createView && look && look.legend) {
    try { const r = await legendRig(look.legend, true, ev.dance); if (r) return r; } catch (e) { console.warn(e); }
  }
  const key = strongestKey(ev.killer);
  const r = DT.Dances ? await DT.Dances.unitRig(key, ev.dance).catch(() => null) : null;
  if (!r) return null;
  const mat = darken(r.group);
  return { group: r.group, update: r.update, dispose: () => { r.dispose(); mat.dispose(); } };
}

// ---------- playback ----------
function next() {
  cur = null;
  while (queue.length) {
    const ev = queue.shift();
    if (DT.UI && DT.UI.game && DT.UI.game() !== ev.game) continue; // game ended or changed
    return show(ev);
  }
  if (layer) { layer.classList.remove('on'); layer.classList.add('hidden'); }
}

function show(ev) {
  ensureLayer();
  const me = ev.game.human;
  const big = ev.victim === me || ev.killer === me || (ev.victims || []).includes(me) || (ev.killers || []).includes(me);
  const look = ev.killer ? DT.Social.lookOf(ev.killer) || {} : {};
  const variant = ev.killer ? ev.variant || look.killscreen : null;
  const themeKey = ev.killer ? themeOf(variant) : 'sting';
  const T = THEMES[themeKey];
  const cinematic = !!(ev.killer && ev.finalHit && big);
  const dur = (!ev.killer ? DUR_STING : big ? DUR_BIG : DUR_SMALL) + (cinematic ? DUR_KILLCAM : 0);
  const name = (p) => (p === me ? 'You' : esc(p.name));
  const victimName = (ev.victims && ev.victims.length > 1) ? ev.victims.map(p => p === me ? 'you' : esc(p.name)).join(' & ') : ev.victim === me ? 'you' : esc(ev.victim.name);
  const main = ev.killer ? `<b>${name(ev.killer)}</b> eliminated ${victimName}` : `${name(ev.victim)} ${ev.victim === me ? 'were' : 'was'} eliminated`;
  const placeTxt = ev.victim.placement ? `${ev.game.duo ? `Team ${ev.victim.team + 1}` : ev.victim === me ? 'You' : ev.victim.name} placed ${ordinal(ev.victim.placement)}` : '';
  const sub = [placeTxt, ev.killer ? (ev.ghost ? 'by a ghost board' : '') : 'lost to Wild Digimon'].filter(Boolean).join(' · ');
  const it = variant && DT.Cosmetics.get('killscreen', variant);
  // the killer's dance and elimination sound from their loadout; older builds without them get the free defaults
  if (ev.killer && !ev.dance) ev.dance = look.dance && DT.Cosmetics.get('dance', look.dance) ? look.dance : DT.Cosmetics.equipped('dance');
  if (ev.killer && DT.ElimSounds) ev.elimsound = DT.ElimSounds.resolve(ev.elimsound || look.elimsound);
  if (ev.killer && !ev.team) ev.team = teamOf(ev.killer);

  layer.classList.remove('hidden', 'on');
  layer.classList.toggle('small', !big);
  layer.classList.toggle('killcam', cinematic);
  layer.querySelector('.ksHonor').textContent = ev.finalHit ? `FINAL HIT · ${DT.LINE[ev.finalHit.line]?.forms[ev.finalHit.stage] || ''}` : '';
  layer.style.setProperty('--ks-accent', T.rim);
  layer.style.setProperty('--ks-text', T.text);
  layer.querySelector('.ksMain').innerHTML = main;
  layer.querySelector('.ksSub').textContent = sub;
  const es = ev.elimsound && DT.Cosmetics.get('elimsound', ev.elimsound);
  layer.querySelector('.ksTag').textContent = [it ? it.name : '', es ? `${es.icon || '♪'} ${es.name}` : ''].filter(Boolean).join(' · ');
  const { w, h } = place(big);
  bg.width = w; bg.height = h;
  void layer.offsetWidth; // restart the CSS transitions
  layer.classList.add('on');

  const state = { ev, t: 0, last: performance.now(), dur, theme: T, big, w, h, lineup: null, sil: null, silRig: null, done: false,
    cinematic, phase: cinematic ? 'attack' : 'dance', caption: {main,sub},
    bpm: ev.dance && DT.Dances ? DT.Dances.bpm(ev.dance) : 0 };
  cur = state;
  if (!cinematic && DT.Sfx) DT.Sfx.play('killscreen', { variant: ev.killer ? variant || 'ks_basic' : 'eliminated', big });
  // the killer's elimination sound, just after the theme's sting; anyone else's counts as another player's soundboard
  if (!cinematic && ev.killer && ev.elimsound && DT.ElimSounds) state.elimPlayed = DT.ElimSounds.play(ev.elimsound, { mine: ev.killer === me, delay: 0.3 });

  glCanvas.style.display = ev.killer ? '' : 'none';
  if (ev.killer && ensureGL()) {
    const n = ev.team.length;
    // lower detail for a big team: fewer pixels per frame keeps 16 dancing models smooth
    gl.setPixelRatio(Math.min(n > 6 ? 1.25 : 2, window.devicePixelRatio || 1));
    gl.setSize(w, h, false);
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
    state.cam = { at: -1, min: big ? 3.2 : 3.0, d: null, y: null, legendTop: 1.05 };
    rimLight.color.set(T.rim);
    gl.clear();
    const team = n ? ev.team : [];
    state.lineup = buildLineup({ team: team.length || look.legend ? team : [{ line: 'patamon', stage: 0 }], legend: look.legend || null, dance: ev.dance, aspect: w / h });
    state.lineup.group.position.y = -0.05;
    scene.add(state.lineup.group);
    if (cinematic) {
      state.lineup.group.visible = false;
      const h = ev.finalHit, line = DT.LINE[h.line];
      if (line) {
        state.hero = new UnitView(scene, document.createElement('div'), 'killcam', line, h.stage, {height:2.5});
        state.hero.yaw = 0; state.hero.body.rotation.y = 0;
        state.hero.ring.visible = false;
        layer.querySelector('.ksMain').textContent = line.abil[h.stage] + '!!';
        layer.querySelector('.ksSub').textContent = line.forms[h.stage] + ' · Final attack';
      }
    }
    if (T.silhouette && !cinematic) {
      silRig(ev).then((sil) => {
        if (!sil) return;
        if (cur !== state) { sil.dispose(); return; }
        const sg = new THREE.Group();
        sg.add(sil.group);
        sg.position.set(0, -1.4, -7.5);
        sg.scale.setScalar(4.6);
        scene.add(sg);
        state.sil = sg;
        state.silRig = sil;
      }).catch((e) => console.warn('Kill screen silhouette', e));
    }
  }
  requestAnimationFrame((now) => tick(state, now));
}

function tick(s, now) {
  if (cur !== s || s.done) return;
  // play time advances by clamped frame steps, so a long stall (model load, shader compile) doesn't eat the screen
  const dt = Math.min(0.1, (now - s.last) / 1000);
  s.last = now;
  if (DT.UI?.paused()) { requestAnimationFrame((n) => tick(s, n)); return; }
  const t = (s.t += dt);
  if (DT.UI && DT.UI.game && DT.UI.game() !== s.ev.game) { finish(s, true); return; }
  try { s.theme.draw(ctx2d, s.w, s.h, t); } catch (e) { console.warn(e); }
  if (s.cinematic && t < DUR_KILLCAM) {
    drawKillcam(s, dt, t);
    requestAnimationFrame((n) => tick(s, n)); return;
  }
  if (s.phase === 'attack') {
    s.phase = 'dance'; layer.classList.remove('killcam');
    layer.querySelector('.ksMain').innerHTML = s.caption.main;
    layer.querySelector('.ksSub').textContent = s.caption.sub;
    if (s.hero) { s.hero.dispose(scene); s.hero = null; }
    camMoves && camMoves.clear();
    if (s.lineup) s.lineup.group.visible = true;
    if (DT.Sfx) DT.Sfx.play('killscreen', {variant: s.ev.variant || 'ks_basic', big:s.big});
    if (s.ev.elimsound && DT.ElimSounds) s.elimPlayed = DT.ElimSounds.play(s.ev.elimsound, {mine:s.ev.killer === s.ev.game.human, delay:.3});
  }
  const danceT = t - (s.cinematic ? DUR_KILLCAM : 0);
  if (gl && s.ev.killer && s.lineup) {
    s.lineup.update(danceT);
    if (s.silRig) s.silRig.update(dt, t);
    // the rim light pulses on the dance's beat
    if (s.bpm) rimLight.intensity = 3 + 2.5 * Math.exp(-(((t * s.bpm) / 60) % 1) * 5);
    s.lineup.group.rotation.y = Math.sin(t * 0.7) * 0.08;
    frameLineup(camera, s.lineup, s.cam, s.w / s.h, t, dt);
    if (s.sil) s.sil.rotation.y = Math.sin(t * 0.3) * 0.15;
    gl.render(scene, camera);
  }
  if (t >= s.dur) { finish(s, false); return; }
  if (t >= s.dur - FADE) layer.classList.remove('on');
  requestAnimationFrame((n) => tick(s, n));
}

function drawKillcam(s, dt, t) {
  const c = ctx2d, w = s.w, h = s.h;
  // Radial data streaks draw the eye from the caster into the impact.
  c.save(); c.strokeStyle = s.theme.rim; c.globalAlpha = .25 + .2*Math.sin(t*9);
  for (let i=0;i<48;i++) {
    const a=i*Math.PI/24, k=.2+((t*.7+i*.17)%1)*.8;
    c.beginPath(); c.moveTo(w/2+Math.cos(a)*w*k*.4,h*.45+Math.sin(a)*h*k*.4);
    c.lineTo(w/2+Math.cos(a)*w*k,h*.45+Math.sin(a)*h*k); c.stroke();
  }
  c.restore();
  if (gl && s.hero) {
    const v=s.hero;
    v.update(dt,t);
    camera.position.set(Math.sin(t*1.2)*.08,1.1,4.0-Math.min(t,2)*.15); camera.lookAt(0,.85,0);
    if (t>.55 && !s.fired) {
      s.fired=true; s.special=v.play('special');
      camMoves.demo(v.line.abil[v.stage], v.stage, {x:0,z:0}, [{x:0,z:3.1}], {line:v.line,view:v});
      if (DT.Sfx) DT.Sfx.play('cast',{stage:v.stage,elem:camMoves.elemOf(v.line.abil[v.stage],v.line)});
    }
    if (!s.special) { v.body.rotation.x = -.18*Math.sin(Math.min(1,t/.8)*Math.PI); v.body.position.z = t>.65 ? Math.sin(Math.min(1,(t-.65)/.7)*Math.PI)*.45 : 0; }
    camMoves.update(dt);
    if (t>1.15 && t<1.6) { camera.position.x+=Math.sin(t*90)*.05; camera.position.y+=Math.cos(t*71)*.04; }
    gl.render(scene,camera);
  }
  const flash = Math.max(0,1-Math.abs(t-1.35)/.12)*.75 + Math.max(0,(t-3.1)/.4);
  // Data shatter is kept behind the text so the move name remains readable.
  if (t>2.8) { c.save(); c.strokeStyle='#d7faff'; c.globalAlpha=Math.min(1,(t-2.8)*2); for(let i=0;i<12;i++){ const x=R(i)*w,y=R(i,2)*h; c.strokeRect(x,y,20+R(i,3)*130,10+R(i,4)*70); } c.restore(); }
  c.fillStyle=`rgba(235,251,255,${Math.min(1,flash)})`; c.fillRect(0,0,w,h);
}

function finish(s, dropAll) {
  if (s.done) return;
  s.done = true;
  if (s.hero) { s.hero.dispose(scene); s.hero = null; }
  camMoves && camMoves.clear();
  if (s.lineup) { scene.remove(s.lineup.group); s.lineup.dispose(); }
  if (s.silRig) { if (s.sil) freeSkeletons(s.sil); s.silRig.dispose(); }
  if (s.sil) scene.remove(s.sil);
  if (gl) gl.renderLists.dispose();
  if (dropAll) queue.length = 0;
  layer.classList.remove('on');
  next();
}

function skip() { if (cur) finish(cur, false); }

// Test and debug hook: play a kill screen without an elimination.
// opts: { killer, victim, variant?, dance?, elimsound?, team? ([{ line, stage }], defaults to the killer's board) }
function demo(opts) {
  const game = DT.UI.game();
  if (!game) return;
  const ev = { game, killer: opts.killer || null, victim: opts.victim, ghost: false, variant: opts.variant || null, dance: opts.dance || null,
    elimsound: opts.elimsound || null, team: opts.team || null, finalHit: opts.finalHit || null, victims: opts.victims || null, killers: opts.killers || null };
  queue.push(ev);
  if (!cur) next();
}

// ---------- Locker / Collection preview of the lineup: one offscreen renderer drawn into 2D canvases ----------
// A sample team when no game is running: nine Digimon of the active set with models, a spread of costs and forms.
function sampleTeam() {
  const g = DT.UI && DT.UI.game ? DT.UI.game() : null;
  if (g && g.human && g.human.boardUnits && g.human.boardUnits().length) return teamOf(g.human);
  const lines = (DT.LINES || []).filter((l) => l && l.forms && l.shop !== false);
  const withModel = lines.filter((l) => { const k = DT.modelKey && DT.modelKey(l, 0); return k && (!lib.index || !Object.keys(lib.index).length || lib.index[k]); });
  const pool = (withModel.length >= 9 ? withModel : lines).slice().sort((a, b) => b.tier - a.tier || (a.id < b.id ? -1 : 1));
  const out = [];
  const stages = [2, 2, 1, 1, 1, 1, 0, 0, 0];
  for (let i = 0; i < 9 && pool.length; i++) {
    const l = pool[Math.floor((i * pool.length) / 9)];
    if (!l || out.some((u) => u.line === l.id)) continue;
    out.push({ line: l.id, stage: Math.min(stages[i], (l.forms.length || 1) - 1) });
  }
  return out;
}
const myLegend = () => {
  const lo = DT.Cosmetics.loadout();
  const l = DT.LegendEvo && DT.LegendEvo.look ? DT.LegendEvo.look(lo) : lo;
  return (l && l.legend) || null;
};
let LP = null;
function lpEnsure() {
  if (LP) return LP;
  const canvas = document.createElement('canvas');
  let r;
  try { r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true, preserveDrawingBuffer: true }); } catch (e) { return null; }
  r.setPixelRatio(1);
  r.outputColorSpace = THREE.SRGBColorSpace;
  r.setClearColor(0x000000, 0);
  const sc = new THREE.Scene();
  const rim = lights(sc);
  LP = { canvas, r, scene: sc, rim, cam: new THREE.PerspectiveCamera(30, 16 / 9, 0.1, 60), lineup: null, sig: '', lastUse: 0, w: 0, h: 0, camState: null, timer: null };
  return LP;
}
// drop the preview lineup a moment after the last preview stops drawing it
function lpIdleCheck() {
  if (!LP) return;
  clearTimeout(LP.timer);
  LP.timer = setTimeout(() => {
    if (!LP || !LP.lineup) return;
    if (performance.now() - LP.lastUse < 1400) return lpIdleCheck();
    LP.scene.remove(LP.lineup.group);
    LP.lineup.dispose();
    LP.lineup = null;
    LP.sig = '';
    LP.r.renderLists.dispose();
  }, 1500);
}
function drawLineup(c, w, h, t, theme) {
  const p = lpEnsure();
  if (!p) return false;
  const team = sampleTeam();
  const legend = myLegend();
  const dance = DT.Cosmetics.equipped('dance');
  const sig = `${legend}|${dance}|${w / h < 0.8 ? 3 : w / h < 1.3 ? 4 : 'w'}|${team.map((u) => `${u.line}:${u.stage}`).join(',')}`;
  if (p.sig !== sig) {
    if (p.lineup) { p.scene.remove(p.lineup.group); p.lineup.dispose(); }
    p.lineup = buildLineup({ team, legend, dance, aspect: w / h });
    p.lineup.t0 = t;
    p.lineup.group.position.y = -0.05;
    p.scene.add(p.lineup.group);
    p.sig = sig;
    p.camState = { at: -1, min: 3.0, d: null, y: null, legendTop: 1.05 };
  }
  p.lastUse = performance.now();
  lpIdleCheck();
  const rw = Math.min(w, 720), rh = Math.round((rw * h) / w);
  if (p.w !== rw || p.h !== rh) { p.r.setSize(rw, rh, false); p.w = rw; p.h = rh; p.cam.aspect = rw / rh; p.cam.updateProjectionMatrix(); }
  const L = p.lineup;
  const lt = t - L.t0;
  const dt = L.lastT == null ? 0 : Math.max(0, lt - L.lastT);
  L.update(lt);
  L.group.rotation.y = Math.sin(lt * 0.7) * 0.08;
  p.rim.color.set(theme.rim);
  const bpm = DT.Dances ? DT.Dances.bpm(dance) : 0;
  if (bpm) p.rim.intensity = 3 + 2.5 * Math.exp(-(((lt * bpm) / 60) % 1) * 5);
  frameLineup(p.cam, L, p.camState, rw / rh, lt, dt);
  p.r.render(p.scene, p.cam);
  c.drawImage(p.canvas, 0, 0, w, h);
  return L.members.length > 0;
}

DT.KillScreens = {
  themes: THEMES,
  eliminations,
  onRoundEnd,
  skip,
  clear() { queue.length = 0; if (cur) finish(cur, true); },
  demo,
  teamOf,
  sampleTeam,
  layout: lineupLayout,
  get active() {
    if (!cur) return null;
    const L = cur.lineup;
    return { killer: cur.ev.killer && cur.ev.killer.name, victim: cur.ev.victim.name, big: cur.big, theme: Object.keys(THEMES).find((k) => THEMES[k] === cur.theme),
      phase: cur.phase, finalHit: cur.ev.finalHit || null, special: !!cur.special, fired: !!cur.fired, placement: cur.ev.victim.placement, dance: cur.ev.dance || null, elimsound: cur.ev.elimsound || null, elimPlayed: !!cur.elimPlayed,
      team: cur.ev.team ? cur.ev.team.length : 0, rigs: L ? L.members.length : 0, expected: L ? L.expected : 0, t: cur.t };
  },
  // test hook: the live lineup's members with a number that changes whenever any of them moves
  lineupInfo(preview) {
    const L = preview ? LP && LP.lineup : cur && cur.lineup;
    if (!L) return null;
    return {
      expected: L.expected, failed: L.failed,
      members: L.members.map((m) => {
        let sig = 0, bones = 0;
        m.rig.group.traverse((o) => {
          if (o.isBone) { bones++; sig += o.quaternion.x * 3 + o.quaternion.y * 5 + o.quaternion.z * 7; }
        });
        const obj = m.rig.obj || m.rig.group.children[0];
        if (obj) sig += obj.position.x * 11 + obj.position.y * 13 + obj.position.z * 17 + obj.quaternion.x * 19 + obj.quaternion.y * 23 + obj.quaternion.z * 29 + obj.scale.y * 31;
        return { key: m.rig.key || (m.rig.legend ? 'legend' : '?'), legend: !!m.rig.legend, procedural: !!m.rig.procedural, bones, sig, scale: m.holder.scale.x, x: m.slot.x, y: m.slot.y || 0, z: m.slot.z };
      }),
    };
  },
  // test hook: GPU objects held by the kill screen renderers
  memory() {
    const m = (r) => (r ? { geometries: r.info.memory.geometries, textures: r.info.memory.textures, programs: r.info.programs ? r.info.programs.length : 0 } : null);
    return { live: m(gl), preview: m(LP && LP.r), sceneChildren: scene ? scene.children.length : 0, previewChildren: LP ? LP.scene.children.length : 0 };
  },
  get queued() { return queue.length; },
};
