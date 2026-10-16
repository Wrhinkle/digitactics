// Item portraits: one illustrated SVG per item, augment and Data Wisp, drawn from shared helpers.
// tools/render-item-icons.cjs rasterizes them to models/items/*.webp (+ index.json); at runtime the game uses those
// files and falls back to the SVG (file://, missing file). Ids: item ids as in DT.ITEMS, 'aug:<id>', 'wisp:<id>'.
// Light comes from the top-left: every part gets a top-left gradient, an inner highlight, a rim light on the
// bottom-right edge and a dark outline. Completed items reuse the colors and motifs of their two components.
const DT = window.DT;
const SIZE = 128;
const INK = '#15161d';

// ---------- color ----------
const hx = (c) => { c = c.replace('#', ''); if (c.length === 3) c = c.split('').map((x) => x + x).join(''); return [0, 2, 4].map((i) => parseInt(c.slice(i, i + 2), 16)); };
const mix = (a, b, t) => { const A = hx(a), B = hx(b); return '#' + A.map((v, i) => Math.round(v + (B[i] - v) * t).toString(16).padStart(2, '0')).join(''); };
const lt = (c, t) => mix(c, '#ffffff', t);
const dk = (c, t) => mix(c, '#000000', t);

const COL = { sword: '#ff7a45', bow: '#ffd23f', rod: '#b26bff', tear: '#4fb3ff', vest: '#c0c6d0', cloak: '#3fd6b0', belt: '#ff4d64', glove: '#8fdc4e', egg: '#fff3b0' };
const METAL = {
  chrome: [[0, '#ffffff'], [0.28, '#d3dbe6'], [0.5, '#7f8999'], [0.64, '#e6ecf3'], [1, '#535b6a']],
  steel: [[0, '#eef2f7'], [0.5, '#98a2b1'], [1, '#4a5160']],
  gold: [[0, '#fff7c8'], [0.3, '#ffd54a'], [0.55, '#c08a1c'], [0.72, '#ffe58c'], [1, '#774a0a']],
  bronze: [[0, '#ffdcb5'], [0.33, '#d88c4c'], [0.6, '#874c20'], [0.76, '#e9a86c'], [1, '#55300f']],
  silver: [[0, '#ffffff'], [0.33, '#d9dfe9'], [0.6, '#8b95a5'], [0.76, '#f0f3f8'], [1, '#606978']],
  prism: [[0, '#fff2ff'], [0.2, '#ff8fd8'], [0.42, '#a08cff'], [0.62, '#6ff0ff'], [0.82, '#fff07a'], [1, '#ff7ab8']],
  dark: [[0, '#6a7183'], [0.5, '#2f3442'], [1, '#14161c']],
  wood: [[0, '#d9a066'], [0.5, '#9a5f2e'], [1, '#4f2c12']],
  stone: [[0, '#e2e4df'], [0.5, '#8e928c'], [1, '#3c403c']],
  glass: [[0, '#ffffff', 0.85], [0.5, '#cfeaff', 0.35], [1, '#6d9cc6', 0.6]],
};

// ---------- shapes (path strings) ----------
const n2 = (v) => +(+v).toFixed(2);
const circ = (cx, cy, r) => `M${n2(cx - r)} ${n2(cy)}a${r} ${r} 0 1 0 ${n2(2 * r)} 0a${r} ${r} 0 1 0 ${n2(-2 * r)} 0Z`;
const ell = (cx, cy, rx, ry) => `M${n2(cx - rx)} ${n2(cy)}a${rx} ${ry} 0 1 0 ${n2(2 * rx)} 0a${rx} ${ry} 0 1 0 ${n2(-2 * rx)} 0Z`;
const rr = (x, y, w, h, r) => { r = Math.min(r, w / 2, h / 2); return `M${n2(x + r)} ${n2(y)}h${n2(w - 2 * r)}a${r} ${r} 0 0 1 ${r} ${r}v${n2(h - 2 * r)}a${r} ${r} 0 0 1 ${-r} ${r}h${n2(2 * r - w)}a${r} ${r} 0 0 1 ${-r} ${-r}v${n2(2 * r - h)}a${r} ${r} 0 0 1 ${r} ${-r}Z`; };
const poly = (pts) => 'M' + pts.map((p) => `${n2(p[0])} ${n2(p[1])}`).join('L') + 'Z';
const star = (cx, cy, R, r, n = 5, rot = -90) => poly(Array.from({ length: n * 2 }, (_, i) => { const a = ((rot + (i * 180) / n) * Math.PI) / 180, q = i % 2 ? r : R; return [cx + q * Math.cos(a), cy + q * Math.sin(a)]; }));
const ngon = (cx, cy, R, n, rot = -90) => poly(Array.from({ length: n }, (_, i) => { const a = ((rot + (i * 360) / n) * Math.PI) / 180; return [cx + R * Math.cos(a), cy + R * Math.sin(a)]; }));
const HEART = 'M0 40C-30 18 -42 -2 -42 -16C-42 -32 -30 -40 -18 -40C-8 -40 -2 -34 0 -28C2 -34 8 -40 18 -40C30 -40 42 -32 42 -16C42 -2 30 18 0 40Z';
const EGG = 'M0 -44C24 -44 36 -6 36 12C36 32 20 44 0 44C-20 44 -36 32 -36 12C-36 -6 -24 -44 0 -44Z';
const FLAME = 'M0 -22C9 -11 16 -4 14 7C12 16 6 20 0 20C-6 20 -12 16 -14 7C-15 -2 -8 -6 -5 -15C-3 -8 2 -8 0 -22Z';
const DROP = 'M0 -20C8 -8 14 0 14 7A14 14 0 0 1 -14 7C-14 0 -8 -8 0 -20Z';
const BOLT = 'M5 -22L-12 3L-2 3L-7 22L12 -4L2 -4Z';

// ---------- picture builder ----------
class Pic {
  constructor(key) { this.k = key.replace(/[^a-z0-9]/gi, ''); this.defs = []; this.out = []; this.n = 0; this.cache = new Map(); this.st = [{ s: 1, r: 0, m: 1 }]; }
  uid(p) { return `${this.k}${p}${++this.n}`; }
  get cur() { return this.st[this.st.length - 1]; }
  add(s) { this.out.push(s); return this; }
  push(x, y, s = 1, r = 0, m = 1) {
    const c = this.cur;
    this.st.push({ s: c.s * s, r: c.r + r * c.m, m: c.m * m });
    this.add(`<g transform="translate(${n2(x)} ${n2(y)})${r ? ` rotate(${r})` : ''}${s !== 1 || m !== 1 ? ` scale(${n2(s * m)} ${n2(s)})` : ''}">`);
    return this;
  }
  pop() { this.st.pop(); return this.add('</g>'); }
  at(x, y, s, r, fn, m = 1) { this.push(x, y, s, r, m); fn(); return this.pop(); }
  // linear gradient that stays top-left -> bottom-right on screen even inside rotated/mirrored groups
  grad(stops) {
    const { r, m } = this.cur;
    const key = JSON.stringify([stops, r, m]);
    if (this.cache.has(key)) return this.cache.get(key);
    const id = this.uid('g');
    const t = [m < 0 ? 'translate(1 0) scale(-1 1)' : '', r ? `rotate(${n2(-r * m)} .5 .5)` : ''].filter(Boolean).join(' ');
    this.defs.push(`<linearGradient id="${id}" x1="0" y1="0" x2="1" y2="1"${t ? ` gradientTransform="${t}"` : ''}>${stops.map(([o, c, a]) => `<stop offset="${o}" stop-color="${c}"${a != null ? ` stop-opacity="${a}"` : ''}/>`).join('')}</linearGradient>`);
    const u = `url(#${id})`;
    this.cache.set(key, u);
    return u;
  }
  rad(stops, cx = 0.38, cy = 0.32, r = 0.8) {
    const id = this.uid('r');
    this.defs.push(`<radialGradient id="${id}" cx="${cx}" cy="${cy}" r="${r}">${stops.map(([o, c, a]) => `<stop offset="${o}" stop-color="${c}"${a != null ? ` stop-opacity="${a}"` : ''}/>`).join('')}</radialGradient>`);
    return `url(#${id})`;
  }
  ramp(c, k = 0.5) { return [[0, lt(c, k)], [0.45, c], [1, dk(c, k)]]; }
  paint(f) {
    if (Array.isArray(f)) return typeof f[0] === 'string' ? this.grad([[0, lt(f[0], 0.4)], [0.5, mix(f[0], f[1], 0.5)], [1, dk(f[1], 0.4)]]) : this.grad(f);
    if (METAL[f]) return this.grad(METAL[f]);
    if (f[0] === '#') return this.grad(this.ramp(f));
    return f;
  }
  // a shaded part: outline, gradient fill, inner top-left highlight, bottom-right rim light
  part(d, f, o = {}) {
    const { s, r, m } = this.cur;
    const ow = (o.ow ?? 6) / s;
    const fr = o.eo ? ' fill-rule="evenodd"' : '';
    let g = '';
    if (ow > 0) g += `<path d="${d}"${fr} fill="${INK}" stroke="${INK}" stroke-width="${n2(ow)}" stroke-linejoin="round"/>`;
    g += `<path d="${d}"${fr} fill="${this.paint(f)}"/>`;
    if (o.hi !== false) {
      const c = this.uid('c');
      this.defs.push(`<clipPath id="${c}"><path d="${d}"${o.eo ? ' clip-rule="evenodd"' : ''}/></clipPath>`);
      const a = (-r * Math.PI) / 180, off = (o.off ?? 2.6) / s;
      const dx = m * off * (Math.cos(a) - Math.sin(a)), dy = off * (Math.sin(a) + Math.cos(a));
      g += `<g clip-path="url(#${c})"><path d="${d}" fill="none" stroke="#fff" stroke-opacity="${o.hiA ?? 0.6}" stroke-width="${n2((o.hiW ?? 3.5) / s)}" transform="translate(${n2(dx)} ${n2(dy)})"/>`;
      g += `<path d="${d}" fill="none" stroke="${o.rim ?? '#e2f6ff'}" stroke-opacity="${o.rimA ?? 0.55}" stroke-width="${n2((o.rimW ?? 2.4) / s)}" transform="translate(${n2(-dx)} ${n2(-dy)})"/></g>`;
    }
    return this.add(g);
  }
  // detail line (no outline)
  line(d, col = INK, w = 3, a = 0.7) { return this.add(`<path d="${d}" fill="none" stroke="${col}" stroke-width="${n2(w / this.cur.s)}" stroke-opacity="${a}" stroke-linecap="round" stroke-linejoin="round"/>`); }
  // flat fill (no outline)
  flat(d, col, a = 1) { return this.add(`<path d="${d}" fill="${col}"${a !== 1 ? ` opacity="${a}"` : ''}/>`); }
  // rounded stroke with outline and a highlight, for shafts, ropes and bands
  tube(d, w, col, o = {}) {
    const { s } = this.cur;
    const ow = (o.ow ?? 6) / s;
    const L = (x, c, wd, a = 1) => `<path d="${d}" fill="none" stroke="${c}" stroke-width="${n2(wd)}" stroke-linecap="${o.cap ?? 'round'}" stroke-linejoin="round"${a !== 1 ? ` stroke-opacity="${a}"` : ''}${x ? ` transform="translate(${x} ${x})"` : ''}/>`;
    return this.add(L(0, INK, w + ow) + L(0, dk(col, 0.25), w) + L(n2(-w * 0.12), col, w * 0.7) + L(n2(-w * 0.22), lt(col, 0.55), w * 0.22, 0.85));
  }
  glow(cx, cy, r, col, a = 0.75) {
    const id = this.uid('w');
    this.defs.push(`<radialGradient id="${id}"><stop offset="0" stop-color="${col}" stop-opacity="${a}"/><stop offset=".45" stop-color="${col}" stop-opacity="${a * 0.45}"/><stop offset="1" stop-color="${col}" stop-opacity="0"/></radialGradient>`);
    return this.add(`<circle cx="${cx}" cy="${cy}" r="${r}" fill="url(#${id})"/>`);
  }
  gloss(cx, cy, rx, ry, rot = -35, a = 0.75) {
    const id = this.uid('s');
    this.defs.push(`<radialGradient id="${id}"><stop offset="0" stop-color="#fff" stop-opacity="${a}"/><stop offset="1" stop-color="#fff" stop-opacity="0"/></radialGradient>`);
    return this.add(`<ellipse cx="${cx}" cy="${cy}" rx="${rx}" ry="${ry}" transform="rotate(${rot} ${cx} ${cy})" fill="url(#${id})"/>`);
  }
  sparkle(cx, cy, r, col = '#ffffff') { this.add(`<path d="${star(cx, cy, r, r * 0.22, 4)}" fill="${col}" stroke="${INK}" stroke-opacity=".35" stroke-width="${n2(1 / this.cur.s)}"/>`); return this; }
  gem(cx, cy, r, col, shape = 'round', ow = 5) {
    const d = shape === 'diamond' ? poly([[cx, cy - r * 1.25], [cx + r, cy], [cx, cy + r * 1.25], [cx - r, cy]]) : shape === 'hex' ? ngon(cx, cy, r, 6) : circ(cx, cy, r);
    this.part(d, [[0, lt(col, 0.75)], [0.42, col], [1, dk(col, 0.6)]], { ow, hiA: 0.5 });
    if (shape === 'round') this.add(`<path d="${poly([[cx - r * 0.55, cy - r * 0.1], [cx - r * 0.1, cy - r * 0.6], [cx + r * 0.25, cy - r * 0.25], [cx - r * 0.2, cy + r * 0.2]])}" fill="#fff" opacity=".45"/>`);
    else this.add(`<path d="${poly([[cx, cy - r * (shape === 'hex' ? 0.9 : 1.1)], [cx - r * 0.55, cy - r * 0.05], [cx, cy]])}" fill="#fff" opacity=".55"/>`);
    return this.gloss(cx - r * 0.35, cy - r * 0.4, r * 0.32, r * 0.2, -35, 0.95);
  }
  clip(d, fn) {
    const c = this.uid('k');
    this.defs.push(`<clipPath id="${c}"><path d="${d}"/></clipPath>`);
    this.add(`<g clip-path="url(#${c})">`); fn(); return this.add('</g>');
  }
  // object layer: everything inside gets one soft drop shadow
  obj(fn) { this.add('<g filter="url(#ds)">'); fn(); return this.add('</g>'); }
  svg() {
    const ds = '<filter id="ds" x="-25%" y="-25%" width="150%" height="150%"><feDropShadow dx="1.6" dy="3" stdDeviation="1.8" flood-color="#000" flood-opacity=".6"/></filter>';
    return `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 ${SIZE} ${SIZE}" width="${SIZE}" height="${SIZE}"><defs>${ds}${this.defs.join('')}</defs>${this.out.join('')}</svg>`;
  }
}

// ---------- frames ----------
const FRAME = {
  square: rr(7, 7, 114, 114, 16),
  shield: 'M64 5L117 19L113 72Q103 105 64 123Q25 105 15 72L11 19Z',
  hex: ngon(64, 64, 61, 6, -90),
  circle: circ(64, 64, 58),
};
// bg: [color] or [colorA, colorB] (split diagonally, used for completed items so the recipe reads from the frame)
function frame(p, shape, metal, bg, o = {}) {
  const d = FRAME[shape];
  const w = o.w ?? 7;
  p.add(`<path d="${d}" fill="none" stroke="${INK}" stroke-width="${w + 6}" stroke-linejoin="round"/>`);
  const [a, b = a] = bg;
  const fill = bg.length > 1
    ? p.grad([[0, dk(a, 0.38)], [0.47, dk(a, 0.6)], [0.53, dk(b, 0.6)], [1, dk(b, 0.4)]])
    : p.rad([[0, dk(a, 0.25)], [0.55, dk(a, 0.6)], [1, dk(a, 0.82)]], 0.42, 0.38, 0.75);
  p.add(`<path d="${d}" fill="${fill}"/>`);
  p.clip(d, () => {
    // soft light behind the object, faint data grid, vignette
    p.add(`<circle cx="58" cy="54" r="56" fill="${p.rad([[0, '#fff', 0.22], [1, '#fff', 0]], 0.5, 0.5, 0.5)}"/>`);
    let grid = '';
    for (let k = 16; k < 128; k += 16) grid += `M${k} 0V128M0 ${k}H128`;
    p.add(`<path d="${grid}" stroke="#fff" stroke-opacity=".05" stroke-width="1"/>`);
    p.add(`<path d="${d}" fill="${p.rad([[0.55, '#000', 0], [1, '#000', 0.55]], 0.5, 0.5, 0.72)}"/>`);
  });
  p.add(`<path d="${d}" fill="none" stroke="${p.paint(metal)}" stroke-width="${w}" stroke-linejoin="round"/>`);
  // thin dark line inside the frame and a bright bevel on its top-left
  const k = 1 - (w + 1.5) / 60;
  p.add(`<path d="${d}" fill="none" stroke="${INK}" stroke-opacity=".75" stroke-width="${n2(1.6 / k)}" transform="translate(64 64) scale(${n2(k)}) translate(-64 -64)"/>`);
  p.add(`<path d="${d}" fill="none" stroke="#fff" stroke-opacity=".45" stroke-width="1.4" transform="translate(-0.8 -0.8)"/>`);
  if (o.studs) for (const [x, y] of o.studs) p.gem(x, y, 3.6, o.studCol || '#fff3b0', 'diamond', 3);
}

// ---------- reusable motifs (component signatures, so completed items read their recipe) ----------
const M = {
  chipPins(p, w, sides = 4, n = 4) {
    for (let s = 0; s < sides; s++) p.at(0, 0, 1, (s * 360) / sides, () => {
      for (let i = 0; i < n; i++) { const x = -w / 2 + (w / (n - 1)) * i * 0.8 + w * 0.1; p.part(rr(x - 3.5, -w / 2 - 11, 7, 14, 2), 'chrome', { ow: 4, hiW: 2 }); }
    });
  },
  chip(p, col, w = 64) { // square microchip with pins
    M.chipPins(p, w);
    p.part(rr(-w / 2, -w / 2, w, w, 9), col);
    p.part(rr(-w / 2 + 11, -w / 2 + 11, w - 22, w - 22, 6), [[0, dk(col, 0.7)], [1, dk(col, 0.45)]], { ow: 3, hi: false });
    p.add(`<circle cx="${-w / 2 + 6}" cy="${-w / 2 + 6}" r="2.6" fill="${dk(col, 0.6)}"/>`);
  },
  miniBlade(p, col = '#fff4e8') {
    p.part(poly([[0, -22], [6, -14], [6, 8], [-6, 8], [-6, -14]]), [[0, '#ffffff'], [0.5, col], [1, dk(col, 0.35)]], { ow: 4 });
    p.part(rr(-12, 7, 24, 6, 3), COL.sword, { ow: 4, hi: false });
    p.part(rr(-3, 13, 6, 9, 2), '#5a3324', { ow: 4, hi: false });
  },
  wing(p, col, s = 1) { // feathered wing pointing left from the origin
    p.push(0, 0, s);
    p.part('M0 -6C-14 -26 -40 -30 -54 -22C-46 -18 -42 -14 -40 -10C-50 -8 -56 -2 -56 4C-46 2 -40 3 -35 6C-42 10 -45 16 -43 22C-30 18 -14 12 0 8Z', col);
    p.line('M-8 -2C-20 -10 -32 -14 -44 -14M-8 2C-20 2 -30 2 -40 4M-8 6C-18 10 -26 13 -34 16', dk(col, 0.55), 2.4, 0.7);
    p.pop();
  },
  crystal(p, col, h = 40, w = 11) { // hexagonal crystal centered at the origin
    const t = -h / 2, b = h / 2;
    p.part(poly([[0, t], [w, t + h * 0.3], [w, b - h * 0.25], [0, b], [-w, b - h * 0.25], [-w, t + h * 0.3]]), [[0, lt(col, 0.7)], [0.4, col], [1, dk(col, 0.55)]], { hiA: 0.7 });
    p.flat(poly([[0, t], [-w, t + h * 0.3], [-w, b - h * 0.25], [0, b - h * 0.08], [0, t + h * 0.12]]), '#fff', 0.22);
    p.line(`M0 ${t + 4}V${b - 3}M${-w + 2} ${t + h * 0.3}L0 ${t + h * 0.42}L${w - 2} ${t + h * 0.3}`, '#fff', 1.6, 0.6);
  },
  card(p, col, w = 52, h = 74) {
    p.part(rr(-w / 2, -h / 2, w, h, 7), col);
    p.line(rr(-w / 2 + 6, -h / 2 + 6, w - 12, h - 12, 4), lt(col, 0.7), 2, 0.7);
    p.part(rr(-w / 2 + 6, -h / 2 + 6, w - 12, 9, 2), dk(col, 0.35), { ow: 0, hi: false });
  },
  plate(p) { // cuirass with pauldrons
    p.part('M-16 -34Q0 -24 16 -34L33 -28L30 -4C30 18 18 34 0 42C-18 34 -30 18 -30 -4L-33 -28Z', 'chrome');
    p.line('M0 -27V40M-27 8Q0 18 27 8M-23 22Q0 32 23 22', INK, 2.5, 0.55);
    for (const x of [-1, 1]) p.part(ell(x * 31, -27, 13, 9.5), 'chrome', { ow: 5 });
    for (const [x, y] of [[-20, -16], [20, -16]]) p.add(`<circle cx="${x}" cy="${y}" r="2.6" fill="#59616f"/>`);
  },
  hood(p, col, eyes) {
    const base = typeof col === 'string' ? col : '#16352f';
    p.part('M0 -44C18 -44 26 -30 26 -16L40 36C26 42 12 40 0 44C-12 40 -26 42 -40 36L-26 -16C-26 -30 -18 -44 0 -44Z', col);
    p.line('M-12 -6L-24 38M0 -4V42M12 -6L24 38', dk(base, 0.6), 2.4, 0.55);
    p.part('M0 -35C10 -35 16 -25 15 -14C10 -7 -10 -7 -15 -14C-16 -25 -10 -35 0 -35Z', [[0, '#05060a'], [1, dk(base, 0.75)]], { ow: 3, hi: false });
    if (eyes) { p.glow(0, -18, 16, eyes, 0.8); for (const x of [-6, 6]) p.add(`<ellipse cx="${x}" cy="-18" rx="3.4" ry="2.4" fill="${lt(eyes, 0.5)}"/>`); }
  },
  disk(p, col, r = 40) {
    p.part(circ(0, 0, r), col);
    p.line(circ(0, 0, r * 0.76), dk(col, 0.55), 1.6, 0.6);
    p.line(circ(0, 0, r * 0.6), dk(col, 0.55), 1.6, 0.5);
    p.flat(`M0 0L${-r * 0.97} ${-r * 0.2}A${r * 0.99} ${r * 0.99} 0 0 1 ${-r * 0.35} ${-r * 0.93}Z`, '#fff', 0.28);
    p.part(circ(0, 0, r * 0.36), 'chrome', { ow: 4 });
  },
  heart(p, col, s = 1, ow = 6) { p.push(0, 0, s); p.part(HEART, col, { ow }); p.gloss(-18, -22, 10, 6, -30, 0.8); p.pop(); },
  glove(p, col, cuff) { // open right hand, palm toward the viewer
    p.at(-24, 6, 1, -38, () => p.part(rr(-6, -24, 13, 30, 6.5), col));
    for (const [x, t] of [[-15, -34], [-3, -41], [9, -37], [20, -27]]) p.part(rr(x - 6, t, 12, 40, 6), col);
    p.part(rr(-24, -12, 49, 40, 15), col);
    p.part(rr(-25, 22, 51, 18, 6), cuff || dk(col, 0.25));
    p.line('M-21 31H22', INK, 2, 0.35);
  },
  clover(p, col = '#e8ffb8', s = 1) {
    p.push(0, 0, s);
    p.part('M0 2L3 14', col, { ow: 4, hi: false });
    p.tube('M0 2Q2 9 5 14', 3, dk(col, 0.3), { ow: 3 });
    for (let k = 0; k < 4; k++) p.at(0, 0, 1, k * 90 + 45, () => p.part('M0 0C-8 -4 -10 -12 -5 -14C-2 -15 0 -12 0 -10C0 -12 2 -15 5 -14C10 -12 8 -4 0 0Z', col, { ow: 3.5, hiW: 2 }));
    p.pop();
  },
  fist(p, col, cuff) {
    p.part(rr(-28, -14, 56, 40, 12), col);
    for (let i = 0; i < 4; i++) p.part(rr(-28 + i * 14, -30, 14, 24, 6), col, { ow: 5 });
    p.at(-8, 10, 1, -8, () => p.part(rr(-22, -7, 38, 15, 7.5), col, { ow: 5 }));
    p.part(rr(-24, 26, 48, 18, 5), cuff || dk(col, 0.3));
  },
  sword(p, o) { // vertical sword, tip at the top
    const W = o.w ?? 14, tip = o.tip ?? 16, top = o.top ?? -48;
    p.part(poly([[0, top], [W / 2, top + tip], [W / 2, 16], [-W / 2, 16], [-W / 2, top + tip]]), o.blade ?? 'steel', { hiA: 0.8 });
    if (o.core) { p.glow(0, (top + 16) / 2, 16, o.core, 0.35); p.part(poly([[0, top + 8], [2.6, top + tip + 2], [2.6, 12], [-2.6, 12], [-2.6, top + tip + 2]]), [[0, '#fff'], [1, o.core]], { ow: 0, hi: false }); }
    const gw = o.gw ?? 34;
    p.part(rr(-4.5, 22, 9, 17, 2), o.grip ?? '#4a2c22', { ow: 5, hi: false });
    p.line('M-4 26L4 29M-4 31L4 34M-4 36L4 39', '#000', 1.6, 0.4);
    p.part(rr(-gw / 2, 14, gw, 10, 5), o.guard ?? 'gold', { ow: 5 });
    if (o.pommel) p.gem(0, 44, 5.5, o.pommel, 'round', 4);
  },
  coin(p, x, y, r = 12, rot = 0) {
    p.at(x, y, 1, rot, () => {
      p.part(ell(0, 0, r, r), 'gold', { ow: 5 });
      p.line(circ(0, 0, r * 0.68), '#8a5a10', 1.8, 0.7);
      p.part(poly([[0, -r * 0.42], [r * 0.32, 0], [0, r * 0.42], [-r * 0.32, 0]]), '#fff1a8', { ow: 0, hi: false });
    });
  },
  egg(p, col, spots, s = 1) {
    p.push(0, 0, s);
    p.part(EGG, col);
    p.clip(EGG, () => { for (const [x, y, r, c] of spots) p.part(ell(x, y, r, r * 0.8), c, { ow: 3, hi: false }); });
    p.gloss(-14, -20, 10, 16, 20, 0.9);
    p.pop();
  },
  die(p, pips = [5, 3, 2]) { // isometric die
    const top = poly([[0, -34], [30, -18], [0, -2], [-30, -18]]), left = poly([[-30, -18], [0, -2], [0, 34], [-30, 18]]), right = poly([[30, -18], [0, -2], [0, 34], [30, 18]]);
    p.part(left, '#e9edf5', { hi: false, ow: 5 }); p.part(right, '#aeb6c6', { hi: false, ow: 5 }); p.part(top, '#ffffff', { hi: false, ow: 5 });
    const dot = (x, y, sx = 1) => p.add(`<ellipse cx="${n2(x)}" cy="${n2(y)}" rx="${3.6 * sx}" ry="3.2" fill="#c7253a"/>`);
    const face = (o, u, v, n, sx) => {
      const L = { 1: [[0.5, 0.5]], 2: [[0.25, 0.25], [0.75, 0.75]], 3: [[0.22, 0.22], [0.5, 0.5], [0.78, 0.78]], 5: [[0.24, 0.24], [0.76, 0.24], [0.5, 0.5], [0.24, 0.76], [0.76, 0.76]] }[n];
      for (const [a, b] of L) dot(o[0] + u[0] * a + v[0] * b, o[1] + u[1] * a + v[1] * b, sx);
    };
    face([-30, -18], [30, -16], [30, 16], pips[0], 1.2);
    face([-30, -18], [30, 16], [0, 36], pips[1], 0.75);
    face([0, -2], [30, -16], [0, 36], pips[2], 0.75);
  },
  digivice(p, body = '#eef1f6', trim = '#ff7a45', screen = '#7dffb0') {
    p.part('M-26 -30C-26 -40 -18 -44 0 -44C18 -44 26 -40 26 -30L30 20C30 36 18 44 0 44C-18 44 -30 36 -30 20Z', body);
    p.part(rr(-20, -32, 40, 32, 8), trim, { ow: 4 });
    p.part(rr(-14, -26, 28, 20, 4), [[0, lt(screen, 0.5)], [1, dk(screen, 0.4)]], { ow: 3, hiA: 0.8 });
    p.add(`<path d="M-8 -12L-3 -20L2 -14L7 -21" fill="none" stroke="${dk(screen, 0.7)}" stroke-width="2" stroke-linecap="round"/>`);
    p.gem(-12, 14, 5, trim, 'round', 3.5); p.gem(4, 22, 5, trim, 'round', 3.5); p.gem(14, 8, 4, trim, 'round', 3.5);
    p.tube('M-10 -44C-10 -54 10 -54 10 -44', 4, '#9aa3b2', { ow: 4 });
  },
  tag(p, inner = '#fff3b0') { // crest tag on a chain
    p.tube('M-8 -36C-8 -48 8 -48 8 -36', 4, '#e8c860', { ow: 4 });
    p.part(rr(-27, -36, 54, 72, 12), 'gold');
    p.part(rr(-19, -27, 38, 54, 8), [[0, '#ffffff'], [0.5, inner], [1, dk(inner, 0.35)]], { ow: 4, hiA: 0.9 });
    p.gloss(-8, -16, 8, 13, 20, 0.6);
  },
  wisp(p, col) {
    p.glow(0, 4, 44, col, 0.5);
    p.part('M0 -48C10 -34 32 -24 32 4C32 23 17 38 0 38C-17 38 -32 23 -32 4C-32 -14 -18 -18 -12 -32C-9 -23 -5 -21 -2 -19C-2 -29 -5 -38 0 -48Z', [[0, '#ffffff'], [0.35, lt(col, 0.55)], [0.75, col], [1, dk(col, 0.35)]], { hiA: 0.8 });
    p.add(`<path d="M0 -36C6 -26 18 -18 18 2C18 14 10 22 0 22C-10 22 -18 14 -18 2C-18 -10 -8 -14 -4 -22C-2 -16 0 -14 0 -36Z" fill="#fff" opacity=".35"/>`);
  },
};

// ---------- symbols (crest tags and Wisps) ----------
const SYM = {
  sword: (p, c) => { p.at(0, 0, 0.9, 0, () => M.miniBlade(p, lt(c, 0.6))); },
  crosshair: (p, c) => { p.tube(circ(0, 0, 13), 5, c, { ow: 4 }); p.tube('M0 -20V-7M0 7V20M-20 0H-7M7 0H20', 4.5, c, { ow: 4 }); p.part(circ(0, 0, 3.5), c, { ow: 3, hi: false }); },
  star6: (p, c) => p.part(star(0, 0, 19, 8, 6), c, { ow: 4 }),
  cross: (p, c) => { p.part('M-5 -20H5V-5H20V5H5V20H-5V5H-20V-5H-5Z', c, { ow: 4 }); p.line(circ(0, 0, 14), dk(c, 0.3), 2.5, 0.8); },
  shield: (p, c) => { p.part('M0 -20L17 -14C17 2 12 13 0 20C-12 13 -17 2 -17 -14Z', c, { ow: 4 }); p.line('M0 -15V15', dk(c, 0.5), 2.5, 0.6); },
  moon: (p, c) => p.part('M6 -20A20 20 0 1 0 18 10A15 15 0 1 1 6 -20Z', c, { ow: 4 }),
  flame: (p, c) => { p.part(FLAME, c, { ow: 4 }); p.at(0, 6, 0.45, 0, () => p.part(FLAME, '#fff2a8', { ow: 0, hi: false })); },
  dagger: (p, c) => p.at(0, 0, 1, 40, () => { p.part(poly([[0, -22], [5, -12], [4, 6], [-4, 6], [-5, -12]]), [[0, '#fff'], [1, c]], { ow: 4 }); p.part(rr(-10, 5, 20, 5, 2.5), c, { ow: 3.5, hi: false }); p.part(rr(-2.5, 10, 5, 9, 2), '#3a2a2a', { ow: 3.5, hi: false }); }),
  coin: (p) => M.coin(p, 0, 0, 14),
  chip: (p, c) => p.at(0, 0, 0.38, 0, () => M.chip(p, c)),
  egg: (p) => M.egg(p, COL.egg, [[-10, -6, 7, '#ff8a4a'], [12, 10, 8, '#ff8a4a'], [-4, 24, 6, '#5fb0ff']], 0.42),
  copy: (p, c) => { p.at(-5, -4, 0.38, -12, () => M.card(p, dk(c, 0.15))); p.at(6, 4, 0.38, 8, () => M.card(p, lt(c, 0.3))); },
  up: (p, c) => { p.part('M0 -21L17 -3H7V18H-7V-3H-17Z', c, { ow: 4 }); p.sparkle(13, -14, 6); },
  chevrons: (p, c) => { for (const y of [-7, 9]) p.part(`M-16 ${y + 6}L0 ${y - 10}L16 ${y + 6}L16 ${y + 13}L0 ${y - 3}L-16 ${y + 13}Z`, c, { ow: 4 }); },
  shieldIron: (p, c) => SYM.shield(p, c),
  drop: (p, c) => { p.part(DROP, c, { ow: 4 }); p.gloss(-4, 2, 3, 6, 20, 0.9); },
  plus: (p, c) => p.part('M-6 -19H6V-6H19V6H6V19H-6V6H-19V-6H-6Z', c, { ow: 4 }),
  die: (p) => p.at(0, 0, 0.55, 0, () => M.die(p)),
  blood: (p) => { p.part(DROP, '#ff3b4e', { ow: 4 }); p.gloss(-4, 2, 3, 6, 20, 0.9); },
  refresh: (p, c) => { p.tube('M-13 -6A14 14 0 0 1 11 -9', 5, c, { ow: 4, cap: 'butt' }); p.tube('M13 6A14 14 0 0 1 -11 9', 5, c, { ow: 4, cap: 'butt' }); p.part(poly([[8, -18], [18, -6], [4, -4]]), c, { ow: 3.5, hi: false }); p.part(poly([[-8, 18], [-18, 6], [-4, 4]]), c, { ow: 3.5, hi: false }); },
  star5: (p, c) => p.part(star(0, 0, 20, 9, 5), c, { ow: 4 }),
  crestTag: (p) => p.at(0, 2, 0.5, 0, () => { M.tag(p); p.at(0, 0, 0.9, 0, () => p.part(star(0, 0, 14, 6, 5), '#ffcf4a', { ow: 4 })); }),
  // drop-only family crests
  badge: (p, c) => { p.part('M0 -21L18 -14C18 4 12 14 0 21C-12 14 -18 4 -18 -14Z', c, { ow: 4 }); p.part('M-7 -10H1C9 -10 11 -4 11 0C11 4 9 10 1 10H-7Z', '#ffffff', { ow: 3, hi: false }); p.part('M-2 -5H1C5 -5 6 -2 6 0C6 2 5 5 1 5H-2Z', c, { ow: 0, hi: false }); },
  dtector: (p, c) => { p.part(ngon(0, 0, 20, 6, -90), dk(c, 0.35), { ow: 4 }); p.part(ngon(0, 0, 14, 6, -90), '#2a1a12', { ow: 0, hi: false }); p.at(0, 1, 0.62, 0, () => p.part(FLAME, [[0, '#fff6c0'], [1, c]], { ow: 3 })); },
  xros: (p, c) => { p.tube('M-15 -15L15 15M15 -15L-15 15', 8, c, { ow: 4 }); p.part(poly([[-21, -21], [-8, -19], [-19, -8]]), lt(c, 0.4), { ow: 3, hi: false }); p.part(poly([[21, 21], [8, 19], [19, 8]]), lt(c, 0.4), { ow: 3, hi: false }); p.part(circ(0, 0, 4.5), '#ffffff', { ow: 3, hi: false }); },
  bug: (p, c) => { p.line('M-9 -4L-19 -10M-10 3L-20 3M-9 10L-18 16M9 -4L19 -10M10 3L20 3M9 10L18 16', INK, 3.2, 1); p.line('M-4 -15L-9 -22M4 -15L9 -22', INK, 2.6, 1); p.part(ell(0, -12, 7, 5), dk(c, 0.45), { ow: 3.5, hi: false }); p.part(ell(0, 4, 12, 15), c, { ow: 4 }); p.line('M0 -10V18', dk(c, 0.55), 2.2, 0.9); p.gloss(-5, -1, 3, 6, 20, 0.8); },
  sludge: (p, c) => { p.part('M-20 16C-22 8 -14 5 -10 6C-13 -1 -7 -6 -2 -5C-4 -12 2 -18 6 -20C6 -14 12 -11 10 -5C16 -5 18 1 13 6C19 6 22 11 20 16Z', [[0, '#c9925a'], [0.5, '#8a5a2a'], [1, '#4a2c12']], { ow: 4 }); p.line('M-12 9C-4 11 6 11 13 9M-5 0C0 1 5 1 8 -1', '#3a2410', 2, 0.8); p.part(circ(-6, 11, 2.2), '#fff', { ow: 1.5, hi: false }); p.part(circ(4, 11, 2.2), '#fff', { ow: 1.5, hi: false }); },
  chess: (p, c) => { p.part('M-14 21H14V15H-14ZM-10 15L-7 -2H7L10 15ZM-11 -2H11V-8H-11ZM-9 -8L-12 -16L-5 -12L0 -18L5 -12L12 -16L9 -8Z', c, { ow: 4 }); p.tube('M0 -24V-17M-3 -21H3', 2.6, lt(c, 0.5), { ow: 2.5 }); p.line('M-6 4H6', dk(c, 0.5), 2, 0.8); },
  sun: (p, c) => { for (let i = 0; i < 8; i++) p.at(0, 0, 1, i * 45, () => p.part(poly([[0, -22], [5, -11], [-5, -11]]), c, { ow: 3, hi: false })); p.part(circ(0, 0, 11), [[0, '#fff6c0'], [0.5, lt(c, 0.3)], [1, c]], { ow: 4 }); p.at(0, 2, 0.4, 0, () => p.part(FLAME, '#fff2a8', { ow: 0, hi: false })); },
  snow: (p, c) => { const arm = 'M0 0V-20M0 -12L-6 -17M0 -12L6 -17M0 -5L-5 -9M0 -5L5 -9'; for (let i = 0; i < 6; i++) p.at(0, 0, 1, i * 60, () => p.tube(arm, 3.6, c, { ow: 3 })); p.part(ngon(0, 0, 5, 6), '#ffffff', { ow: 2.5, hi: false }); },
  skull: (p, c) => { p.part('M0 -20C12 -20 18 -11 18 -2C18 6 13 9 11 11V17H-11V11C-13 9 -18 6 -18 -2C-18 -11 -12 -20 0 -20Z', c, { ow: 4 }); p.part(ell(-7, 0, 5, 5.5), INK, { ow: 0, hi: false }); p.part(ell(7, 0, 5, 5.5), INK, { ow: 0, hi: false }); p.part(poly([[0, 5], [3, 10], [-3, 10]]), INK, { ow: 0, hi: false }); p.line('M-5 13V17M0 13V17M5 13V17', INK, 1.8, 0.9); },
  pearl: (p, c) => { for (let i = 0; i < 8; i++) p.at(0, 0, 1, i * 45 + 22.5, () => p.line('M0 -14V-22', lt(c, 0.5), 3, 0.95)); p.glow(0, 0, 18, c, 0.6); p.part(circ(0, 0, 12), [[0, '#ffffff'], [0.45, lt(c, 0.5)], [1, dk(c, 0.3)]], { ow: 4 }); p.gloss(-4, -4, 3, 5, -30, 0.95); p.line('M-6 6C-2 9 3 9 7 5', dk(c, 0.4), 1.8, 0.6); },
  brokenwing: (p, c) => p.at(13, 2, 0.62, 0, () => { M.wing(p, dk(c, 0.35)); p.line('M-30 -24L-22 -8L-34 2L-24 16', '#0f0a16', 5, 1); }),
};

// ---------- art per id ----------
const ART = {};
const C = COL;

// components (bronze frame, one dominant color)
const comp = (id, fn) => { ART[id] = (p) => { frame(p, 'square', 'bronze', [C[id]], { w: 6 }); p.obj(() => fn(p)); }; };
comp('sword', (p) => p.at(64, 64, 1, 0, () => { M.chip(p, C.sword, 62); p.glow(0, 0, 22, '#ffcf8a', 0.7); p.at(0, 0, 1.05, 45, () => M.miniBlade(p)); }));
comp('bow', (p) => p.at(64, 66, 1, 0, () => {
  p.at(-16, 0, 0.82, 0, () => M.wing(p, '#fff6c8')); p.at(16, 0, 0.82, 0, () => M.wing(p, '#fff6c8'), -1);
  M.chipPins(p, 40, 2, 3);
  p.part(rr(-22, -22, 44, 44, 8), C.bow);
  p.part(rr(-14, -14, 28, 28, 5), [[0, dk(C.bow, 0.72)], [1, dk(C.bow, 0.5)]], { ow: 3, hi: false });
  p.glow(0, 0, 16, '#fff3a0', 0.8);
  p.at(0, 0, 0.62, 0, () => p.part(BOLT, [[0, '#ffffff'], [1, '#ffd23f']], { ow: 5 }));
}));
comp('rod', (p) => p.at(66, 64, 1, 38, () => {
  p.part(rr(-5, -12, 10, 56, 4), 'dark');
  for (const y of [2, 22]) p.part(rr(-7.5, y, 15, 6, 2), C.rod, { ow: 4, hi: false });
  p.gem(0, 46, 6.5, C.rod, 'round', 4.5);
  p.part('M-12 -16C-14 -24 -10 -30 -6 -32L-4 -22L4 -22L6 -32C10 -30 14 -24 12 -16Z', 'dark', { ow: 5 });
  p.glow(0, -34, 26, C.rod, 0.75);
  p.at(0, -34, 1, 0, () => M.crystal(p, C.rod, 38, 11));
  p.sparkle(14, -46, 6);
}));
comp('tear', (p) => {
  p.at(56, 62, 1, -12, () => M.card(p, dk(C.tear, 0.35), 48, 68));
  p.at(68, 66, 1, 8, () => {
    M.card(p, C.tear, 50, 72);
    p.glow(0, 4, 24, '#bfe6ff', 0.8);
    p.at(0, 4, 1, 0, () => { p.part(DROP, [[0, '#ffffff'], [0.6, '#9fdcff'], [1, '#2a7fd0']], { ow: 4.5 }); p.gloss(-4, 3, 3, 7, 20, 0.95); });
    p.part(rr(-12, 24, 24, 6, 1.5), 'gold', { ow: 3.5, hi: false });
  });
});
comp('vest', (p) => p.at(64, 64, 1, 0, () => M.plate(p)));
comp('cloak', (p) => p.at(64, 64, 1, 0, () => {
  M.hood(p, C.cloak);
  p.line('M-30 30C-20 34 -10 33 0 36C10 33 20 34 30 30', '#d4fff2', 2.6, 0.9);
  p.line('M-18 10L-12 14L-6 10L0 14L6 10L12 14L18 10', '#bdfff0', 2, 0.6);
  p.gem(0, -6, 4.5, '#bdfff0', 'diamond', 3.5);
}));
comp('belt', (p) => p.at(64, 64, 1, 0, () => { M.disk(p, C.belt, 41); p.at(0, 1, 0.3, 0, () => p.part(HEART, '#ff3b55', { ow: 10 })); }));
comp('glove', (p) => p.at(64, 66, 1, 0, () => { M.glove(p, C.glove, '#f2f5e6'); p.at(1, 6, 1.05, 0, () => M.clover(p)); }));
comp('egg', (p) => p.at(64, 63, 1, 0, () => M.egg(p, C.egg, [[-14, -10, 8, '#ff8a4a'], [14, 4, 10, '#ff8a4a'], [-6, 26, 7, '#ff8a4a'], [18, -24, 5, '#5fb0ff'], [-20, 14, 5, '#5fb0ff']])));

// completed items (gold frame, background split in the two component colors)
const done = (id, fn) => { ART[id] = (p) => { const it = DT.ITEMS[id]; frame(p, 'square', 'gold', [C[it.from[0]], C[it.from[1]]], { w: 8 }); p.obj(() => fn(p)); }; };
const blade = (c) => [[0, '#ffffff'], [0.45, lt(c, 0.55)], [1, dk(c, 0.35)]];

done('omni_blade', (p) => p.at(64, 64, 1.08, 45, () => { M.sword(p, { w: 17, blade: blade(C.sword), core: '#ff7a1a', guard: C.sword, gw: 38, pommel: C.sword }); p.at(0, 19, 0.17, 0, () => M.chip(p, '#ffb070')); }));
done('giga_slayer', (p) => p.at(64, 64, 1.05, 45, () => {
  p.at(-14, 19, 0.5, 0, () => M.wing(p, C.bow)); p.at(14, 19, 0.5, 0, () => M.wing(p, C.bow), -1);
  M.sword(p, { w: 28, tip: 20, blade: blade(C.sword), guard: C.bow, gw: 40, pommel: C.bow });
  p.at(0, -16, 0.7, 0, () => p.part(BOLT, [[0, '#fff'], [1, C.bow]], { ow: 4 }));
}));
done('data_gunblade', (p) => p.at(62, 66, 1, 40, () => {
  M.sword(p, { w: 14, top: -50, blade: blade(C.sword), guard: 'dark', gw: 24 });
  p.at(-6, 30, 1, 25, () => p.part(rr(-5, -4, 11, 22, 3), '#4a2c22', { ow: 5 }));
  p.part(rr(-13, 10, 26, 16, 5), 'dark');
  p.part(circ(0, 18, 9), 'chrome', { ow: 4 });
  p.glow(0, 18, 14, C.rod, 0.9);
  p.gem(0, 18, 5, C.rod, 'hex', 3);
}));
done('mana_spear', (p) => p.at(64, 64, 1.05, 42, () => {
  p.tube('M0 50V-14', 8, C.sword);
  for (const y of [8, 30]) p.part(rr(-6, y, 12, 5, 2), 'gold', { ow: 4, hi: false });
  p.part(rr(-9, -20, 18, 8, 3), 'chrome', { ow: 4 });
  p.glow(0, -34, 22, '#9fdcff', 0.8);
  p.part(poly([[0, -56], [12, -34], [6, -20], [-6, -20], [-12, -34]]), [[0, '#ffffff'], [0.45, '#9fdcff'], [1, '#1f6fc0']], { hiA: 0.8 });
  p.line('M0 -52V-22', '#fff', 1.6, 0.7);
  p.gem(0, -16, 3.6, C.tear, 'diamond', 3);
}));
done('night_edge', (p) => p.at(64, 66, 1, 0, () => {
  p.glow(8, -10, 36, '#7a5cff', 0.5);
  p.part('M-30 -26A15 15 0 1 0 -12 -44A11 11 0 1 1 -30 -26Z', '#fff2b0', { ow: 4 });
  p.tube('M-14 26L-30 44', 8, '#3a2c3a');
  p.part('M-12 24C-22 -6 2 -34 40 -44C16 -26 4 -4 0 26Z', [[0, '#e6e8ff'], [0.35, '#8a8cc0'], [1, '#22223d']], { hiA: 0.8 });
  p.line('M-8 20C-14 -6 6 -28 34 -41', '#ff9a5a', 3.4, 1);
  p.part(ell(-8, 26, 15, 8), 'chrome', { ow: 5 });
}));
done('blood_fang', (p) => p.at(60, 68, 1.05, 18, () => {
  p.part('M-9 18C-12 -8 0 -32 24 -50C14 -26 10 -4 9 18Z', [[0, '#fffbea'], [0.45, '#ffc49a'], [1, '#e8462e']], { hiA: 0.85 });
  p.line('M-3 12C-4 -8 6 -28 18 -42', '#ff7a45', 2.2, 0.8);
  p.part('M22 -44C25 -40 26 -36 24 -33C22 -36 21 -40 22 -44Z', '#ff2e43', { ow: 3, hi: false });
  p.part(rr(-16, 16, 32, 9, 4), dk(C.sword, 0.25), { ow: 5 });
  p.part(rr(-7, 24, 14, 22, 4), C.cloak, { ow: 5 });
  p.line('M-6 28L6 32M-6 34L6 38M-6 40L6 44', dk(C.cloak, 0.6), 2, 0.7);
  p.part('M6 30C18 32 24 40 22 50C16 44 12 40 4 38Z', C.cloak, { ow: 4 });
  p.part(ell(0, 50, 4, 5), '#ff2e43', { ow: 3, hi: false });
}));
done('titan_gauntlet', (p) => p.at(64, 62, 1, 0, () => {
  M.fist(p, C.sword, dk(C.sword, 0.35));
  for (let i = 0; i < 4; i++) p.part(poly([[-24 + i * 14, -30], [-21 + i * 14, -40], [-18 + i * 14, -30]]), 'chrome', { ow: 3.5, hi: false });
  p.at(0, 35, 0.2, 0, () => p.part(HEART, C.belt, { ow: 18 }));
}));
done('crit_edge', (p) => p.at(64, 64, 1.05, 45, () => {
  M.sword(p, { w: 11, tip: 12, top: -52, blade: blade(C.sword), core: '#ffb070', guard: C.glove, gw: 26, grip: '#2f3b22' });
  p.at(0, 19, 0.5, 0, () => M.clover(p, '#f2ffd0'));
  p.at(0, -52, 1, -45, () => { p.glow(0, 0, 18, '#fff0a0', 0.9); p.add(`<path d="${star(0, 0, 15, 4, 4)}" fill="#fffbe0"/>`); });
}));
done('rage_wing', (p) => p.at(64, 64, 1, 0, () => {
  p.glow(0, 0, 40, C.bow, 0.35);
  const F = [[-10, 64, '#ffe680'], [16, 58, '#ffd23f'], [42, 50, '#ffb02e'], [68, 40, '#ff8a2a']];
  for (const [a, L, c] of F.slice().reverse()) p.at(-28, 32, 1, a, () => p.part(`M0 0C9 -10 11 ${-L * 0.6} 3 ${-L}C-6 ${-L * 0.6} -9 -12 0 0Z`, [[0, '#fffbe0'], [0.5, c], [1, dk(c, 0.45)]], { ow: 5 }));
  p.tube('M-30 32C-24 6 -6 -20 22 -32', 7, '#ffcf3a');
  p.at(10, 6, 0.6, 15, () => p.part(BOLT, [[0, '#fff'], [1, '#ff6a2a']], { ow: 5 }));
}));
done('wild_rush', (p) => p.at(64, 66, 1, 0, () => {
  p.line('M-44 -12H-26M-46 2H-30M-42 16H-28', '#fff3a0', 3.5, 0.8);
  for (const [x, r] of [[-15, -14], [0, 0], [15, 14]]) p.at(x, 12, 1, r, () => p.part('M-6 0C-6 -20 0 -40 13 -50C7 -32 6 -16 6 0Z', [[0, '#fbf4ff'], [0.5, '#c9a3ff'], [1, '#4c2384']], { ow: 5 }));
  p.part(rr(-26, 10, 52, 26, 9), C.bow);
  p.line('M-20 30H20', dk(C.bow, 0.5), 2, 0.5);
  p.gem(0, 22, 7, C.rod, 'hex', 4);
}));
done('volt_disk', (p) => p.at(64, 64, 1, 0, () => {
  M.disk(p, C.tear, 40);
  p.glow(0, 0, 30, '#fff3a0', 0.6);
  p.at(2, 0, 1.45, 10, () => p.part(BOLT, [[0, '#ffffff'], [0.5, C.bow], [1, '#e09a00']], { ow: 4.5 }));
}));
done('titan_will', (p) => p.at(64, 64, 1, 0, () => {
  p.part('M-27 -38L27 -38C32 -12 32 12 26 40L-26 40C-32 12 -32 -12 -27 -38Z', 'chrome');
  p.line('M-29 -26H29M-30 28H30', INK, 2.4, 0.55);
  for (const [x, y] of [[-22, -32], [22, -32], [-22, 34], [22, 34]]) p.add(`<circle cx="${x}" cy="${y}" r="2.4" fill="#59616f"/>`);
  p.glow(0, 0, 26, C.bow, 0.55);
  for (const y of [-14, 0, 14]) p.part(`M-15 ${y + 7}L0 ${y - 6}L15 ${y + 7}L15 ${y + 12}L0 ${y - 1}L-15 ${y + 12}Z`, [[0, '#fff8d0'], [1, '#f0a800']], { ow: 3.5, hi: false });
}));
done('twin_shot', (p) => p.at(64, 66, 1, 35, () => {
  p.part(rr(-6, -18, 12, 62, 4), C.cloak);
  p.tube('M-42 -2Q0 -34 42 -2', 8, C.bow);
  p.line('M-42 -2L0 14L42 -2', '#f6f1e0', 1.8, 0.95);
  for (const x of [-6, 6]) { p.tube(`M${x} 14V-40`, 3.5, '#e8e2cc', { ow: 4 }); p.part(poly([[x, -54], [x + 5, -40], [x - 5, -40]]), lt(C.cloak, 0.4), { ow: 3.5, hi: false }); }
  p.part(rr(-9, 22, 18, 9, 3), 'dark', { ow: 4 });
}));
done('kraken_fang', (p) => p.at(64, 64, 1.02, 32, () => {
  p.tube('M0 50V-14', 6, '#e0b030');
  p.part('M-20 -42C-22 -28 -16 -18 -6 -14L6 -14C16 -18 22 -28 20 -42L14 -30C12 -24 8 -22 4 -22L4 -52L0 -58L-4 -52L-4 -22C-8 -22 -12 -24 -14 -30Z', [[0, '#fff6c8'], [0.5, C.bow], [1, '#8a5a00']], { hiA: 0.7 });
  p.tube('M-2 46C14 40 14 30 0 26C-14 22 -14 12 0 8C12 4 12 -4 4 -8', 7, C.belt, { ow: 5 });
  for (const [x, y] of [[8, 38], [-8, 22], [7, 6]]) p.add(`<circle cx="${x}" cy="${y}" r="1.6" fill="#ffd0d6"/>`);
}));
done('armor_breaker', (p) => {
  p.at(96, 30, 1, 0, () => { for (const [x, y, r] of [[0, 0, 20], [-14, 12, -30], [10, 16, 60]]) p.at(x, y, 1, r, () => p.part(poly([[-6, -5], [6, -6], [3, 6]]), 'chrome', { ow: 3.5, hi: false })); });
  p.at(60, 68, 1.02, 45, () => {
    p.part(rr(-7, 26, 14, 22, 4), dk(C.glove, 0.4), { ow: 5 });
    p.part(rr(-17, 0, 34, 28, 7), C.glove);
    p.part(rr(-21, -8, 42, 10, 3), 'chrome', { ow: 5 });
    p.part(poly([[0, -54], [19, -8], [-19, -8]]), [[0, '#fff8d0'], [0.5, C.bow], [1, '#9a6800']]);
    p.line('M-14 -18L10 -26M-10 -30L6 -36M-6 -41L3 -45', '#6a4600', 2.6, 0.7);
  });
});
done('ultima_rod', (p) => p.at(64, 66, 1, 28, () => {
  p.tube('M0 50V-4', 7, '#5b3796');
  for (const y of [14, 32]) p.part(rr(-6, y, 12, 5, 2), 'gold', { ow: 4, hi: false });
  p.glow(0, -26, 34, C.rod, 0.85);
  p.part('M-26 -36C-30 -16 -16 -2 0 -2C16 -2 30 -16 26 -36C22 -22 12 -14 0 -14C-12 -14 -22 -22 -26 -36Z', [[0, '#f0e0ff'], [0.5, '#9a5ae0'], [1, '#3a1870']]);
  p.at(0, -28, 1, 0, () => M.crystal(p, '#c48aff', 36, 11));
  p.at(-22, -40, 0.35, 0, () => M.crystal(p, C.rod)); p.at(22, -40, 0.35, 0, () => M.crystal(p, C.rod));
  p.sparkle(-16, -54, 6); p.sparkle(20, -16, 4);
}));
done('seraph_rod', (p) => p.at(64, 66, 1, 12, () => {
  p.tube('M0 50V-6', 6.5, '#e6f2ff');
  p.part(rr(-5, 30, 10, 5, 2), C.rod, { ow: 4, hi: false });
  p.at(-8, -22, 0.62, 0, () => M.wing(p, '#dff2ff')); p.at(8, -22, 0.62, 0, () => M.wing(p, '#dff2ff'), -1);
  p.glow(0, -26, 22, C.rod, 0.8);
  p.at(0, -24, 0.85, 0, () => M.crystal(p, C.rod, 34, 10));
  p.add(`<ellipse cx="0" cy="-52" rx="14" ry="4.5" fill="none" stroke="${INK}" stroke-width="7"/><ellipse cx="0" cy="-52" rx="14" ry="4.5" fill="none" stroke="#9fdcff" stroke-width="3.5"/>`);
}));
done('holy_barrier', (p) => p.at(64, 70, 1, 0, () => {
  p.add(`<path d="M-48 18A48 48 0 0 1 48 18" fill="none" stroke="#e4ccff" stroke-opacity=".5" stroke-width="3"/>`);
  p.glow(0, -10, 40, C.rod, 0.4);
  p.part('M-34 8L-40 -28L-18 -8L0 -38L18 -8L40 -28L34 8Z', 'chrome');
  p.part(rr(-36, 4, 72, 18, 5), 'chrome');
  for (const [x, y] of [[-40, -28], [0, -38], [40, -28]]) p.gem(x, y, 4.5, C.rod, 'round', 3.5);
  for (const x of [-22, 22]) p.gem(x, 13, 3.6, C.rod, 'round', 3);
  p.gem(0, 6, 8, C.rod, 'diamond', 4.5);
}));
done('mana_zap', (p) => p.at(64, 70, 1, 0, () => {
  p.part(rr(-22, 26, 44, 14, 4), 'dark');
  p.part(poly([[-12, 27], [12, 27], [7, -12], [-7, -12]]), C.cloak);
  for (const [y, w] of [[16, 15], [4, 13], [-7, 11]]) p.part(ell(0, y, w, 3.6), 'chrome', { ow: 4, hi: false });
  p.glow(0, -24, 30, C.rod, 0.9);
  for (const d of ['M-8 -28L-20 -36L-26 -28L-40 -40', 'M8 -28L22 -40L26 -30L42 -42', 'M0 -36L-4 -48L4 -54L0 -62']) { p.line(d, INK, 7, 0.8); p.line(d, '#f2e2ff', 3, 1); }
  p.gem(0, -24, 11, C.rod, 'round', 5);
}));
done('flame_tome', (p) => p.at(62, 70, 1, 0, () => {
  p.at(-6, -30, 0.9, -12, () => p.part(FLAME, [[0, '#fff2a8'], [0.5, '#ff7a3a'], [1, '#d81e3a']], { ow: 4.5 }));
  p.at(12, -34, 1.1, 10, () => p.part(FLAME, [[0, '#fff2a8'], [0.5, '#ff5d4a'], [1, '#c41632']], { ow: 4.5 }));
  p.part(rr(-28, -26, 60, 60, 5), '#f6ecd4', { ow: 5 });
  p.part(rr(-32, -30, 56, 62, 6), C.rod);
  p.part(rr(-36, -30, 10, 62, 4), dk(C.rod, 0.35), { ow: 5 });
  for (const [x, y] of [[24, -30], [24, 32]]) p.part(poly([[x, y], [x - 10, y], [x, y + (y < 0 ? 10 : -10)]]), 'gold', { ow: 3.5, hi: false });
  p.part(circ(-2, 1, 16), 'gold', { ow: 4 });
  p.at(-2, 2, 0.62, 0, () => p.part(FLAME, [[0, '#fff2a8'], [0.5, C.belt], [1, '#9a1028']], { ow: 5 }));
}));
done('jewel_gauntlet', (p) => p.at(64, 64, 1, 0, () => {
  M.fist(p, C.glove, dk(C.glove, 0.35));
  for (let i = 0; i < 4; i++) p.gem(-21 + i * 14, -22, 4.2, C.rod, 'diamond', 3.5);
  p.gem(0, 35, 6.5, C.rod, 'hex', 4);
  p.sparkle(30, -36, 7); p.sparkle(-34, -30, 4);
}));
done('mana_overflow', (p) => p.at(64, 66, 1, 0, () => {
  const body = 'M-8 -40H8V-16C22 -12 32 0 32 14C32 32 18 44 0 44C-18 44 -32 32 -32 14C-32 0 -22 -12 -8 -16Z';
  p.part(body, 'glass', { hiA: 0.9 });
  p.clip(body, () => {
    p.part('M-40 4C-26 -2 -14 8 0 2C14 -4 26 6 40 0V50H-40Z', [[0, '#8fd6ff'], [0.5, C.tear], [1, '#1c4f9a']], { ow: 3, hi: false });
    p.add('<circle cx="-10" cy="22" r="3" fill="#fff" opacity=".6"/><circle cx="6" cy="32" r="2" fill="#fff" opacity=".6"/><circle cx="12" cy="14" r="2.4" fill="#fff" opacity=".5"/>');
  });
  p.gloss(-16, 10, 6, 14, 20, 0.7);
  p.part(rr(-11, -46, 22, 8, 3), C.tear, { ow: 4 });
  p.part('M-11 -44C-18 -40 -20 -30 -22 -18C-24 -10 -18 -8 -17 -16C-16 -26 -14 -36 -8 -40Z', C.tear, { ow: 3.5 });
  p.part('M10 -44C20 -40 24 -30 25 -22C26 -14 20 -14 20 -20C19 -28 16 -36 8 -40Z', C.tear, { ow: 3.5 });
  for (const [x, y, s] of [[-30, -32, 0.4], [32, -40, 0.32], [36, -10, 0.25]]) p.at(x, y, s, 0, () => p.part(DROP, lt(C.tear, 0.3), { ow: 10 }));
}));
done('guard_vow', (p) => p.at(64, 64, 1, 0, () => {
  p.part('M0 -44L34 -34C36 -4 26 24 0 44C-26 24 -36 -4 -34 -34Z', 'chrome');
  p.line('M0 -36L26 -28C27 -4 19 18 0 34C-19 18 -27 -4 -26 -28Z', INK, 2.2, 0.5);
  p.glow(0, -2, 24, '#9fdcff', 0.8);
  p.at(0, -1, 1.15, 0, () => { p.part(DROP, [[0, '#ffffff'], [0.5, C.tear], [1, '#1c5fae']], { ow: 4.5 }); p.gloss(-4, 3, 3, 7, 20, 0.95); });
}));
done('adaptive_helm', (p) => p.at(64, 66, 1, 0, () => {
  p.part(poly([[-4, -50], [4, -50], [6, -30], [-6, -30]]), C.cloak, { ow: 5 });
  p.part('M-32 12C-32 -22 -16 -40 0 -40C16 -40 32 -22 32 12L30 30C18 26 10 30 0 38C-10 30 -18 26 -30 30Z', C.tear);
  p.part('M-27 -4C-14 -10 14 -10 27 -4L25 10C12 5 -12 5 -25 10Z', [[0, '#e8fff8'], [0.4, C.cloak], [1, '#0d6e5a']], { ow: 4.5, hiA: 0.9 });
  p.line('M0 -38V-10', dk(C.tear, 0.55), 2.4, 0.6);
  p.gloss(-14, -24, 6, 10, 30, 0.6);
}));
done('holy_ring', (p) => p.at(64, 66, 1, 0, () => {
  p.glow(0, -14, 36, '#fff6c8', 0.55);
  p.part(ell(0, 16, 36, 24) + ell(0, 18, 25, 14), [[0, '#ffd7dc'], [0.5, C.belt], [1, '#7a1424']], { eo: true });
  p.part(poly([[-14, -6], [-10, -22], [10, -22], [14, -6]]), 'chrome', { ow: 4 });
  p.gem(0, -20, 14, C.tear, 'round', 5);
  p.sparkle(-28, -30, 7); p.sparkle(30, -18, 5);
}));
done('justice_hand', (p) => p.at(64, 70, 0.92, 0, () => {
  M.glove(p, C.glove, '#f2f5e6');
  p.glow(0, -54, 22, '#bfe6ff', 0.9);
  p.at(0, -54, 0.78, 0, () => { p.part(DROP, [[0, '#ffffff'], [0.5, C.tear], [1, '#1c5fae']], { ow: 5 }); p.gloss(-4, 3, 3, 7, 20, 0.95); });
  p.gem(0, 8, 6, C.tear, 'diamond', 4);
}));
done('spike_plate', (p) => p.at(64, 64, 1, 0, () => {
  p.part(star(0, 0, 50, 30, 8, -90), 'steel');
  p.part(circ(0, 0, 33), 'chrome');
  p.line(circ(0, 0, 24), INK, 2, 0.45);
  for (let k = 0; k < 8; k++) { const a = (k * Math.PI) / 4; p.add(`<circle cx="${n2(28 * Math.cos(a))}" cy="${n2(28 * Math.sin(a))}" r="2.2" fill="#59616f"/>`); }
  p.part(star(0, 0, 14, 7, 4, -45), 'chrome', { ow: 4 });
}));
done('stone_hide', (p) => p.at(64, 64, 1, 0, () => {
  const rock = poly([[-30, -40], [8, -44], [36, -26], [40, 8], [24, 38], [-12, 44], [-38, 22], [-42, -12]]);
  p.part(rock, 'stone');
  p.clip(rock, () => p.part(rr(-50, -6, 100, 13, 3), 'chrome', { ow: 4 }));
  for (const d of ['M-20 -30L-8 -16L-16 -6', 'M18 -30L10 -18L20 -10', 'M-10 12L0 24L-6 36', 'M24 14L14 26']) { p.line(d, '#063d31', 6, 0.9); p.line(d, '#7dfff0', 2.6, 1); }
  p.glow(0, 0, 40, C.cloak, 0.25);
}));
done('flame_core', (p) => p.at(64, 68, 1, 0, () => {
  for (const [x, s, r] of [[-22, 0.8, -25], [22, 0.8, 25], [0, 1.15, 0]]) p.at(x, -28, s, r, () => p.part(FLAME, [[0, '#fff2a8'], [0.5, '#ff6a3a'], [1, '#c41632']], { ow: 5 }));
  p.part(circ(0, 6, 31), 'chrome');
  p.line('M-31 6H-17M17 6H31M0 -25V-11M0 23V37', INK, 2.4, 0.55);
  p.part(circ(0, 6, 17), [[0, '#2a0a10'], [1, '#5a1420']], { ow: 4, hi: false });
  p.glow(0, 6, 18, '#ff8a3a', 0.9);
  p.at(0, 7, 0.68, 0, () => p.part(FLAME, [[0, '#fffbe0'], [0.5, '#ffb03a'], [1, C.belt]], { ow: 0, hi: false }));
}));
done('steadfast_heart', (p) => p.at(64, 64, 1, 0, () => {
  p.part(HEART, 'chrome');
  p.line('M0 -24V34', INK, 2, 0.4);
  for (const [x, y] of [[-28, -24], [28, -24], [-22, 6], [22, 6]]) p.add(`<circle cx="${x}" cy="${y}" r="2.4" fill="#59616f"/>`);
  p.glow(0, -2, 22, C.glove, 0.7);
  p.at(0, -2, 0.42, 0, () => p.part(HEART, [[0, '#f2ffd0'], [0.5, C.glove], [1, '#2f6a10']], { ow: 10 }));
  p.gloss(-4, -12, 4, 3, -30, 0.95);
}));
done('dragon_scale', (p) => p.at(64, 62, 1, 0, () => {
  const S = 'M0 -42C26 -42 38 -20 34 4C30 24 14 38 0 48C-14 38 -30 24 -34 4C-38 -20 -26 -42 0 -42Z';
  p.part(S, C.cloak);
  p.clip(S, () => { for (const [x, y] of [[-16, -16], [16, -16], [0, -2], [-16, 14], [16, 14], [0, 30]]) p.at(x, y, 0.36, 0, () => p.part(S, [[0, lt(C.cloak, 0.6)], [1, dk(C.cloak, 0.45)]], { ow: 6, hiA: 0.5 })); });
  p.line('M0 -40V44', '#d4fff2', 2, 0.5);
}));
done('shadow_veil', (p) => p.at(64, 64, 1, 0, () => {
  p.glow(0, 0, 48, C.belt, 0.35);
  M.hood(p, [[0, '#4a8a7a'], [0.5, '#16352f'], [1, '#05100d']], C.belt);
  p.line('M-36 34C-28 40 -24 30 -16 38C-8 46 -4 34 0 42C6 34 10 46 16 38C24 30 28 40 36 34', '#ff8a9a', 2, 0.7);
  p.at(0, -4, 0.14, 0, () => p.part(HEART, C.belt, { ow: 20 }));
}));
done('quick_silver', (p) => p.at(64, 68, 1, 0, () => {
  p.part(rr(-6, -46, 12, 10, 3), C.glove, { ow: 4.5 });
  p.at(26, -24, 1, 45, () => p.part(rr(-4, -6, 8, 10, 2), C.glove, { ow: 4 }));
  p.part(circ(0, 2, 36), 'silver');
  p.part(circ(0, 2, 27), [[0, '#f2fffb'], [0.5, '#a8f0de'], [1, '#2a9e86']], { ow: 4, hiA: 0.8 });
  p.add(`<path d="M0 2L0 -25A27 27 0 0 1 23.4 -11.5Z" fill="${C.cloak}" opacity=".55"/>`);
  for (let k = 0; k < 12; k++) { const a = (k * Math.PI) / 6; p.line(`M${n2(22 * Math.sin(a))} ${n2(2 - 22 * Math.cos(a))}L${n2(25 * Math.sin(a))} ${n2(2 - 25 * Math.cos(a))}`, INK, 2, 0.7); }
  p.line('M0 2L0 -18M0 2L12 8', INK, 3.2, 0.9);
  p.add('<circle cx="0" cy="2" r="3" fill="#15161d"/>');
}));
done('giga_belt', (p) => p.at(64, 64, 1, 0, () => {
  p.part('M-48 -10Q0 -26 48 -10L48 14Q0 -2 -48 14Z', '#c8323f');
  p.line('M-44 -6Q0 -20 44 -6M-44 9Q0 -5 44 9', '#ffb0b8', 1.6, 0.6);
  for (const x of [-38, -30, 30, 38]) p.add(`<circle cx="${x}" cy="${n2(1 - 10 * (1 - (x / 48) ** 2) + 2)}" r="2" fill="#4a0d14"/>`);
  p.part(rr(-22, -24, 44, 42, 10), 'gold');
  p.part(rr(-15, -17, 30, 28, 6), [[0, '#5a0d16'], [1, '#2a0509']], { ow: 3, hi: false });
  p.at(0, -3, 0.33, 0, () => p.part(HEART, C.belt, { ow: 12 }));
  p.gloss(-8, -10, 4, 3, -30, 0.9);
}));
done('guard_breaker', (p) => {
  p.at(30, 34, 1, 0, () => { for (const [x, y, r] of [[0, 0, 0], [12, -10, 40], [-6, 14, -50]]) p.at(x, y, 1, r, () => p.part(poly([[-7, -5], [7, -7], [2, 7]]), '#9fdcff', { ow: 3.5, hi: false })); });
  p.at(68, 66, 1.02, 38, () => {
    p.tube('M0 50V-16', 8, dk(C.glove, 0.35));
    for (const y of [20, 30, 40]) p.line(`M-4 ${y}L4 ${y + 4}`, '#000', 1.6, 0.4);
    p.part(rr(-30, -40, 60, 26, 6), C.belt);
    for (const x of [-30, 22]) p.part(rr(x, -42, 8, 30, 3), 'chrome', { ow: 4.5 });
    p.gem(-6, -27, 5, C.glove, 'diamond', 3.5);
  });
});
done('thief_glove', (p) => p.at(64, 68, 1, 0, () => {
  p.at(-12, -30, 0.3, -18, () => M.chip(p, C.sword));
  p.at(14, -32, 0.3, 14, () => M.card(p, C.tear));
  p.part('M-30 34C-40 10 -26 -10 -12 -14L-15 -22L15 -22L12 -14C26 -10 40 10 30 34C14 42 -14 42 -30 34Z', C.glove);
  p.tube('M-15 -16Q0 -10 15 -16', 5, '#e8c860', { ow: 4 });
  p.at(2, 14, 1.1, 0, () => M.clover(p, '#f2ffd0'));
  p.sparkle(30, -40, 7); p.sparkle(-32, -36, 5);
}));

// crests: shield frame in the trait color, crest tag with the trait symbol
const CREST_SYM = { Striker: 'sword', Sniper: 'crosshair', Mystic: 'star6', VB: 'cross', Guardian: 'shield', NSo: 'moon', DR: 'flame', Assassin: 'dagger',
  DATS: 'badge', LW: 'dtector', XF: 'xros', INS: 'bug', FUN: 'sludge', KN: 'chess', FL: 'sun', FR: 'snow', UND: 'skull', HB: 'pearl', FA: 'brokenwing' };
for (const [trait, sym] of Object.entries(CREST_SYM)) {
  const id = `emblem_${trait.toLowerCase()}`;
  ART[id] = (p) => {
    const c = DT.TRAITS[trait].color;
    frame(p, 'shield', [[0, lt(c, 0.7)], [0.35, c], [0.65, dk(c, 0.45)], [0.8, lt(c, 0.3)], [1, dk(c, 0.6)]], [c], { w: 8 });
    p.obj(() => p.at(64, 64, 1.16, 0, () => { M.tag(p); p.glow(0, 0, 22, c, 0.5); p.at(0, 0, 1.08, 0, () => SYM[sym](p, c)); }));
  };
}
ART.golden_egg = (p) => {
  frame(p, 'square', 'gold', ['#ffd84a', C.egg], { w: 8 });
  p.obj(() => p.at(64, 64, 1, 0, () => {
    p.glow(0, 0, 50, '#ffe680', 0.7);
    M.egg(p, [[0, '#fffbe0'], [0.35, '#ffd84a'], [0.7, '#c88a10'], [1, '#6a4206']], [[-14, -10, 8, '#fff2a0'], [14, 4, 10, '#fff2a0'], [-6, 26, 7, '#fff2a0']]);
    p.part('M-16 -40L-10 -52L-4 -44L0 -56L4 -44L10 -52L16 -40Z', 'gold', { ow: 4 });
    p.sparkle(30, -30, 8); p.sparkle(-32, 18, 5); p.sparkle(26, 32, 4);
  }));
};

// consumables: steel frame
ART.remover = (p) => {
  frame(p, 'square', 'steel', ['#ff8fa3', '#8a8f9c'], { w: 7 });
  p.obj(() => p.at(64, 70, 1, 0, () => {
    for (const r of [44, 54]) p.add(`<path d="M${-r * 0.7} ${-r * 0.55 - 6}A${r} ${r} 0 0 1 ${r * 0.7} ${-r * 0.55 - 6}" fill="none" stroke="#9fdcff" stroke-opacity=".7" stroke-width="2.5" stroke-dasharray="5 4" transform="translate(0 -4)"/>`);
    p.tube('M-20 -26V4A20 20 0 0 0 20 4V-26', 17, '#e5364f', { cap: 'butt' });
    for (const x of [-20, 20]) p.part(rr(x - 9.5, -42, 19, 14, 2), 'chrome', { ow: 5 });
    p.at(0, -52, 0.22, 20, () => M.chip(p, C.sword));
  }));
};
ART.reforger = (p) => {
  frame(p, 'square', 'steel', ['#ffb347', '#5a6170'], { w: 7 });
  p.obj(() => p.at(64, 68, 1, 0, () => {
    p.part('M-40 -2L28 -2C38 -2 44 -8 48 -12L44 2C38 10 28 12 20 12L14 24L24 36L-24 36L-14 24L-18 12C-28 10 -38 6 -40 -2Z', 'dark');
    p.part(rr(-40, -8, 70, 9, 3), 'steel', { ow: 5 });
    p.at(-6, -26, 1, -35, () => { p.tube('M0 0V-30', 6, '#a8652e'); p.part(rr(-15, -40, 30, 14, 3), 'chrome', { ow: 5 }); });
    p.glow(14, -14, 20, '#ffb347', 0.9);
    p.add(`<path d="${star(14, -14, 13, 4, 8)}" fill="#fff2b0"/>`);
    for (const [x, y] of [[30, -26], [34, -10], [24, -32]]) p.sparkle(x, y, 4, '#ffd27a');
  }));
};

// augments: hexagon frame, silver / gold / prismatic
const AUG_FRAME = ['', 'silver', 'gold', 'prism'];
const AUG_BG = ['', '#5a6b84', '#8a6420', '#6a3a9a'];
const aug = (id, fn, bg) => { ART[`aug:${id}`] = (p) => { const t = DT.AUGMENT[id].tier; frame(p, 'hex', AUG_FRAME[t], [bg || AUG_BG[t]], { w: 7 }); p.obj(() => p.at(64, 64, 0.92, 0, () => fn(p))); }; };
aug('pocket', (p) => { M.coin(p, -14, 14, 15, -10); M.coin(p, 14, 6, 15, 12); M.coin(p, -2, -16, 15, 0); p.sparkle(24, -24, 6); });
aug('cache', (p) => {
  p.part(rr(-36, -6, 72, 40, 6), 'wood');
  p.part('M-36 -4C-36 -26 -24 -32 0 -32C24 -32 36 -26 36 -4Z', 'wood');
  for (const x of [-22, 18]) p.part(rr(x, -32, 6, 66, 2), 'chrome', { ow: 4, hi: false });
  p.part(rr(-8, -8, 16, 16, 3), 'gold', { ow: 4 });
  p.add('<circle cx="0" cy="0" r="2.6" fill="#3a2410"/>');
});
aug('rolls', (p) => { M.die(p); p.add(`<path d="M30 -38A42 42 0 0 1 40 6" fill="none" stroke="${INK}" stroke-width="7"/><path d="M30 -38A42 42 0 0 1 40 6" fill="none" stroke="#bfe6ff" stroke-width="3"/>`); });
aug('heart', (p) => { M.heart(p, '#ff3b55', 0.95); p.at(24, 24, 0.55, 0, () => SYM.plus(p, '#7dff9a')); });
aug('training', (p) => p.at(0, 0, 1, -24, () => {
  p.tube('M-40 0H40', 7, '#c9d1dc');
  for (const x of [-1, 1]) { p.part(rr(x * 22 - 7, -24, 14, 48, 4), '#e05a4a'); p.part(rr(x * 35 - 5, -16, 10, 32, 3), '#3a7bd5'); }
}));
aug('thrill', (p) => {
  p.at(-4, -2, 1, 28, () => {
    p.part('M-14 -30C-6 -36 8 -36 14 -30C12 -4 6 20 -1 42C-5 20 -11 -4 -14 -30Z', [[0, '#fffbea'], [0.55, '#ffd0b0'], [1, '#d81e3a']], { hiA: 0.8 });
    p.tube('M-15 -32C-6 -38 8 -38 15 -32', 5, '#a8652e', { ow: 4 });
  });
  p.at(24, 22, 0.62, 0, () => SYM.plus(p, '#7dff9a'));
});
aug('secondWind', (p) => {
  for (const [d, c] of [['M-38 -10C-10 -24 30 -20 30 -2C30 10 14 12 10 2', '#d4fff6'], ['M-40 12C-14 0 24 6 24 20C24 30 10 30 8 22', '#9ff0e0']]) p.tube(d, 7, c);
  p.at(-6, -32, 0.6, 0, () => SYM.plus(p, '#7dff9a'));
});
aug('featherweights', (p) => p.at(4, 0, 1, 35, () => {
  p.part('M0 -46C18 -30 16 10 0 34C-16 10 -18 -30 0 -46Z', [[0, '#ffffff'], [0.5, '#dfeaff'], [1, '#7f93b8']]);
  p.line('M0 -40V44M0 -24L-10 -32M0 -10L-12 -18M0 4L-12 -4M0 -20L10 -28M0 -6L12 -14M0 8L12 0', '#6f80a0', 2, 0.7);
  p.line('M-30 30H-46M-26 40H-40', '#e8f0ff', 3, 0.8);
}));
aug('frontline', (p) => {
  p.part(rr(-28, -40, 56, 80, 10), [[0, '#e8f0ff'], [0.5, '#5f7fb8'], [1, '#1d2f55']]);
  p.part(rr(-20, -32, 40, 64, 6), 'chrome', { ow: 4 });
  for (const [x, y] of [[-14, -26], [14, -26], [-14, 26], [14, 26]]) p.add(`<circle cx="${x}" cy="${y}" r="2.6" fill="#59616f"/>`);
  p.part(rr(-4, -24, 8, 48, 3), '#5f7fb8', { ow: 3.5, hi: false });
});
aug('backline', (p) => { for (const [x, y] of [[-15, -15], [0, 0], [15, 15]]) p.at(x + 4, y + 4, 0.88, 45, () => { p.tube('M0 30V-18', 4, '#d9b27a', { ow: 4 }); p.part(poly([[0, -34], [7, -18], [-7, -18]]), 'steel', { ow: 4 }); p.part(poly([[0, 22], [6, 34], [0, 30], [-6, 34]]), '#ff8a5a', { ow: 3, hi: false }); }); });
aug('bank', (p) => { for (const [x, n] of [[-14, 4], [14, 6]]) for (let i = 0; i < n; i++) p.at(x, 30 - i * 9, 1, 0, () => { p.part(rr(-15, -4, 30, 10, 4), 'gold', { ow: 4, hi: false }); p.flat(ell(0, -3, 13, 3), '#fff1a8', 0.8); }); p.sparkle(26, -30, 7); });
aug('trove', (p) => {
  p.part('M-36 -8L-40 -38L36 -38L36 -8Z', 'wood');
  p.glow(0, -8, 36, '#ffe680', 0.9);
  for (const [x, y] of [[-16, -8], [14, -10], [-2, -18], [4, -4], [-24, -2], [24, -2]]) M.coin(p, x, y, 9);
  p.gem(-8, -6, 6, '#6ff0ff', 'diamond', 4);
  p.part(rr(-36, 0, 72, 34, 6), 'wood');
  for (const x of [-22, 18]) p.part(rr(x, 0, 6, 34, 2), 'gold', { ow: 4, hi: false });
});
aug('egg', (p) => {
  p.part(ell(0, 26, 38, 14), 'wood');
  p.line('M-32 22L-10 30M-20 18L4 32M8 18L30 28M-28 30L28 22', '#3a2410', 2, 0.6);
  p.at(0, -6, 0.75, 0, () => M.egg(p, C.egg, [[-14, -10, 8, '#ff8a4a'], [14, 4, 10, '#ff8a4a'], [-6, 26, 7, '#5fb0ff']]));
});
aug('rolls2', (p) => { p.at(-14, -8, 0.68, -8, () => M.die(p, [3, 2, 5])); p.at(16, 12, 0.68, 8, () => M.die(p)); });
aug('income', (p) => { M.coin(p, -6, 8, 26); p.part('M22 -42L40 -20H29V4H15V-20H4Z', [[0, '#d0ffd8'], [0.5, '#4fd86a'], [1, '#1d7a30']], { ow: 5 }); });
aug('evolve', (p) => { p.glow(0, 0, 50, '#7dffb0', 0.5); M.digivice(p); });
aug('blessing', (p) => {
  p.at(-6, 4, 0.8, 0, () => M.wing(p, '#fffbe8')); p.at(6, 4, 0.8, 0, () => M.wing(p, '#fffbe8'), -1);
  p.add(`<ellipse cx="0" cy="-30" rx="18" ry="6" fill="none" stroke="${INK}" stroke-width="8"/><ellipse cx="0" cy="-30" rx="18" ry="6" fill="none" stroke="#ffe680" stroke-width="4"/>`);
  p.at(0, 8, 0.45, 0, () => M.heart(p, '#ff6a80', 1, 10));
});
aug('lotus', (p) => {
  p.glow(0, 0, 44, '#ff9ae0', 0.5);
  for (const [r, s] of [[-70, 0.8], [70, 0.8], [-35, 0.95], [35, 0.95], [0, 1.05]]) p.at(0, 14, s, r, () => p.part('M0 0C-14 -14 -12 -32 0 -42C12 -32 14 -14 0 0Z', [[0, '#fff0fa'], [0.5, '#ff8fd0'], [1, '#9a2a7a']], { ow: 5 }));
  p.gem(0, 6, 9, '#6ff0ff', 'diamond', 4.5);
});
aug('implants', (p) => {
  for (const [d] of [['M-16 -12H-34V-30'], ['M16 -12H34V-30'], ['M-16 12H-34V30'], ['M16 12H34V30'], ['M0 -18V-40'], ['M0 18V40']]) { p.line(d, INK, 6, 0.9); p.line(d, '#6ff0ff', 2.5, 1); }
  for (const [x, y] of [[-34, -30], [34, -30], [-34, 30], [34, 30], [0, -40], [0, 40]]) p.part(circ(x, y, 4.5), '#6ff0ff', { ow: 3.5, hi: false });
  p.part(rr(-20, -20, 40, 40, 6), 'chrome');
  p.part(rr(-12, -12, 24, 24, 4), [[0, '#1d3a55'], [1, '#0a1622']], { ow: 3, hi: false });
  p.glow(0, 0, 14, '#6ff0ff', 0.9);
});
aug('unity', (p) => { for (const [x, y] of [[-15, 10], [15, 10], [0, -14]]) p.tube(circ(x, y, 15), 7, '#ffe27a'); });
aug('overcharge', (p) => p.at(0, 0, 1, 0, () => {
  p.part(rr(-8, -44, 16, 8, 2), 'chrome', { ow: 4 });
  p.part(rr(-22, -38, 44, 80, 8), 'dark');
  p.part(rr(-15, -30, 30, 64, 4), [[0, '#d8f4ff'], [0.5, C.tear], [1, '#103a7a']], { ow: 3, hiA: 0.7 });
  p.glow(0, 2, 26, '#fff3a0', 0.6);
  p.at(0, 2, 1.25, 0, () => p.part(BOLT, [[0, '#ffffff'], [1, C.bow]], { ow: 4.5 }));
}));
aug('jackpot', (p) => {
  p.glow(0, 0, 50, '#ffe680', 0.6);
  for (const [x, y, r] of [[-26, 28, 12], [24, 28, 12], [-10, 30, 13], [10, 26, 13], [-18, 12, 12], [16, 10, 12], [0, 14, 13], [-6, -4, 12], [10, -8, 12], [0, -24, 13]]) M.coin(p, x, y, r);
  p.sparkle(-28, -24, 8); p.sparkle(30, -14, 6);
});
aug('armory', (p) => { for (const [r, c] of [[-45, '#ffd54a'], [45, '#9fdcff']]) p.at(0, 4, 0.82, r, () => M.sword(p, { w: 13, blade: blade(c), guard: 'gold', gw: 30, pommel: c })); });
aug('goldenEgg', (p) => { p.glow(0, 0, 50, '#ffe680', 0.7); M.egg(p, [[0, '#fffbe0'], [0.35, '#ffd84a'], [0.7, '#c88a10'], [1, '#6a4206']], [[-14, -10, 8, '#fff2a0'], [14, 4, 10, '#fff2a0'], [-6, 26, 7, '#fff2a0']]); p.sparkle(30, -30, 8); });
aug('megaEvolve', (p) => {
  p.add(`<path d="${star(0, 0, 54, 24, 12)}" fill="${p.grad(METAL.prism)}" opacity=".85"/>`);
  p.at(0, 4, 0.82, 0, () => M.digivice(p, '#f6f2ff', '#a08cff', '#ffe27a'));
});
aug('giant', (p) => {
  p.part('M-22 6C-30 26 -16 44 0 44C16 44 30 26 22 6C16 -4 -16 -4 -22 6Z', '#8a5a3a');
  for (const [x, y, r] of [[-26, -22, -25], [0, -32, 0], [26, -22, 25]]) p.at(x, y, 1, r, () => p.part('M0 -16C8 -16 10 -4 8 8C6 14 -6 14 -8 8C-10 -4 -8 -16 0 -16Z', '#8a5a3a', { ow: 5 }));
  p.line('M-14 30Q0 36 14 30', '#2a170c', 2.5, 0.5);
});
aug('crest', (p) => p.at(0, 2, 1, 0, () => {
  M.tag(p);
  p.glow(0, 0, 24, '#ff8a2a', 0.7);
  p.part(circ(0, 0, 8), '#ff8a2a', { ow: 4 });
  for (let k = 0; k < 8; k++) p.at(0, 0, 1, k * 45, () => p.part(poly([[-4, -11], [4, -11], [0, -21]]), '#ff8a2a', { ow: 3.5, hi: false }));
}));

// Data Wisps: round frame in the category color, a spirit flame carrying a symbol
const WISP_COL = { Gold: '#ffd23f', Item: '#ff9a45', Champion: '#b98bff', Combat: '#ff5d5d', Shop: '#4fb3ff', Misc: '#6fe08a', Risky: '#e0405a' };
const WISP_SYM = { gold: 'coin', comp: 'chip', egg: 'egg', copy: 'copy', evolve: 'up', level: 'chevrons', battle: 'sword', iron: 'shieldIron', mana: 'drop', heal: 'plus', gamble: 'die', pact: 'blood', rolls: 'refresh', fortune: 'star5', crest: 'crestTag' };
const WISP_SYMCOL = { chip: COL.sword, copy: COL.tear, sword: '#ff7a45', shieldIron: '#d9dfe9', drop: COL.tear, plus: '#7dff9a', star5: '#ffe27a', up: '#c9a3ff', chevrons: '#7dff9a', refresh: '#9fdcff', badge: '#ffcf4a' };
for (const w of DT.WISPS) {
  ART[`wisp:${w.id}`] = (p) => {
    const c = WISP_COL[w.kind] || '#8a78ff';
    frame(p, 'circle', [[0, lt(c, 0.7)], [0.4, c], [0.7, dk(c, 0.5)], [0.85, lt(c, 0.3)], [1, dk(c, 0.6)]], [dk(c, 0.35)], { w: 6 });
    p.obj(() => p.at(64, 64, 0.95, 0, () => { M.wisp(p, c); const s = WISP_SYM[w.id]; p.at(0, 10, 1.25, 0, () => SYM[s](p, WISP_SYMCOL[s] || '#ffffff')); }));
  };
}

// ---------- per-set art ----------
// Sets add painters with DT.Sets.extend(id, { art: { items: { itemId | 'aug:<id>' | 'wisp:<id>': (p, kit) => {...} } } }).
// A set's own painter wins. The base art above is used for an id only while the active set's item, augment or Wisp
// of that id has the same name as the Faction Wars one (a shared item), so a reused id never shows the wrong picture.
// Emblems with no art get a crest in their trait color.
const KIT = { frame, M, SYM, COL, METAL, lt, dk, mix, circ, ell, rr, poly, star, ngon, HEART, EGG, FLAME, DROP, BOLT, SIZE,
  // crest: shield frame in color with a SYM symbol (names: Object.keys(kit.SYM))
  crest(p, color, sym = 'star6') {
    frame(p, 'shield', [[0, lt(color, 0.7)], [0.35, color], [0.65, dk(color, 0.45)], [0.8, lt(color, 0.3)], [1, dk(color, 0.6)]], [color], { w: 8 });
    p.obj(() => p.at(64, 64, 1.16, 0, () => { M.tag(p); p.glow(0, 0, 22, color, 0.5); p.at(0, 0, 1.08, 0, () => (SYM[sym] || SYM.star6)(p, color)); }));
  },
};
const sameAsBase = (id) => {
  const S = DT.SET;
  if (!S || !DT.Sets || S.id === DT.Sets.DEFAULT) return true;
  const B = DT.Sets.get(DT.Sets.DEFAULT);
  if (!B) return true;
  const name = (src, k) => (src && src[k] ? src[k].name : null);
  if (id.startsWith('aug:')) return name(S.AUGMENT, id.slice(4)) != null && name(S.AUGMENT, id.slice(4)) === name(B.AUGMENT, id.slice(4));
  if (id.startsWith('wisp:')) return name(S.WISP, id.slice(5)) != null && name(S.WISP, id.slice(5)) === name(B.WISP, id.slice(5));
  return name(S.ITEMS, id) != null && name(S.ITEMS, id) === name(B.ITEMS, id);
};
// { key, fn } for the active set, or null when there is no art
function resolve(id) {
  const S = DT.SET;
  const own = S && S.art && S.art.items && S.art.items[id];
  if (own) return { key: `${S.id}/${id}`, fn: (p) => own(p, KIT) };
  if (ART[id] && sameAsBase(id)) return { key: id, fn: ART[id] };
  const it = DT.ITEMS && DT.ITEMS[id];
  if (it && it.emblem && DT.TRAITS[it.emblem]) {
    const T = DT.TRAITS[it.emblem];
    return { key: `${S ? S.id : ''}/${id}`, fn: (p) => KIT.crest(p, T.color, T.crestSymbol || 'star6') };
  }
  return null;
}

// ---------- runtime ----------
const svgs = new Map();
function svg(id) {
  const r = resolve(id);
  if (!r) return '';
  if (!svgs.has(r.key)) { const p = new Pic(r.key); r.fn(p); svgs.set(r.key, p.svg()); }
  return svgs.get(r.key);
}
const svgUrls = new Map();
function svgUrl(id) {
  const r = resolve(id);
  if (!r) return '';
  if (!svgUrls.has(r.key)) svgUrls.set(r.key, 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg(id)));
  return svgUrls.get(r.key);
}

// pre-rendered files (tools/render-item-icons.cjs); missing on file:// or before the first render
const DIR = 'models/items/';
let index = null;
const ready = (typeof fetch === 'function' && location.protocol !== 'file:'
  ? fetch(DIR + 'index.json').then((r) => (r.ok ? r.json() : {})).catch(() => ({}))
  : Promise.resolve({})).then((j) => { index = j || {}; return index; });
// Pre-rendered files: models/items/index.json for the base art; a set's own art in models/items/sets/<setId>/index.json
// (tools/render-item-icons.cjs --set <id>), read only when the set's art says prerendered: true.
const setIdx = new Map();
function setIndex(sid) {
  if (setIdx.has(sid)) { const v = setIdx.get(sid); return v === 'loading' ? null : v; }
  const S = DT.Sets && DT.Sets.get(sid);
  if (!S || !S.art.prerendered || typeof fetch !== 'function' || location.protocol === 'file:') { setIdx.set(sid, {}); return {}; }
  setIdx.set(sid, 'loading');
  fetch(`${DIR}sets/${sid}/index.json`).then((r) => (r.ok ? r.json() : {})).catch(() => ({})).then((j) => setIdx.set(sid, j || {}));
  return null;
}
const fileUrl = (id) => {
  const r = resolve(id);
  if (!r) return '';
  const slash = r.key.indexOf('/');
  if (slash < 0) return index && index[r.key] ? DIR + index[r.key] : '';
  const sid = r.key.slice(0, slash);
  const si = setIndex(sid);
  return si && si[r.key] ? `${DIR}sets/${sid}/${si[r.key]}` : '';
};

// best image URL for an id at a display size (CSS px); files are 128 px, larger requests use the SVG
function url(id, size = 64) {
  if (!resolve(id)) return '';
  return (size <= SIZE && fileUrl(id)) || svgUrl(id);
}
function img(id, size = 64, cls = '') {
  const u = url(id, size);
  return u ? `<img class="itemart${cls ? ' ' + cls : ''}" src="${u}" width="${size}" height="${size}" alt="" draggable="false">` : '';
}
// draw the portrait into a canvas (for WebGL textures); resolves true when drawn, false if nothing could load
function paint(cv, id) {
  return ready.then(() => new Promise((res) => {
    const g = cv.getContext('2d');
    const load = (src, next) => {
      if (!src) { if (next) next(); else res(false); return; }
      const im = new Image();
      im.onload = () => { g.clearRect(0, 0, cv.width, cv.height); g.drawImage(im, 0, 0, cv.width, cv.height); res(true); };
      im.onerror = () => (next ? next() : res(false));
      im.src = src;
    };
    load(fileUrl(id), () => load(svgUrl(id)));
  }));
}
// ids with art: the base set's (default) or the active set's items, augments and Wisps
const ids = (all) => {
  if (all || !DT.SET || DT.SET.id === (DT.Sets && DT.Sets.DEFAULT)) return Object.keys(ART);
  const S = DT.SET;
  const keys = Object.keys(S.ITEMS).concat(S.AUGMENTS.map((a) => `aug:${a.id}`), S.WISPS.map((w) => `wisp:${w.id}`), Object.keys(S.art.items || {}));
  return [...new Set(keys)].filter((k) => resolve(k));
};
const has = (id) => !!resolve(id);

DT.ItemArt = { url, img, svg, svgUrl, paint, ready, ids, has, size: SIZE, kit: KIT, resolve };
export const ItemArt = DT.ItemArt;
