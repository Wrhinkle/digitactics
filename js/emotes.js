// Emotes and dances: 'emote' cosmetics, the emote wheel (T key or the Emote button), and stickers shown over a
// Tamer's Little Legend and next to their row in the Tamers list. Client-only, never part of game state.
// Every emote, yours included, is displayed through DT.Social.on('emote'), so solo and multiplayer share one path.
// Retro emotes (data.retro) also pop a small static Digimon World model up beside the legend for a moment.
import * as THREE from 'three';
import * as SkeletonUtils from 'three/addons/utils/SkeletonUtils.js';
import { loadTemplate } from './render3d.js';
import { stickers } from './mystery-content.js';

const DT = window.DT;
const OL = '#1b1424';
const RATE_N = 3; // emotes per RATE_MS per player
const RATE_MS = 6000;
const SHOW_MS = 2600;
const MUTE_KEY = 'dt.emotes.muteOthers';

// ---------- sticker art (original SVG) ----------
const svg = (inner) => `<svg class="em-svg" viewBox="0 0 120 120" xmlns="http://www.w3.org/2000/svg">${inner}</svg>`;
const g = (cls, inner, style = '') => `<g class="a ${cls}"${style ? ` style="${style}"` : ''}>${inner}</g>`;
const T = (s, y, size, fill, x = 60) => `<text x="${x}" y="${y}" text-anchor="middle" font-family="'Arial Black','Segoe UI Black',Impact,sans-serif" font-weight="900" font-size="${size}" fill="${fill}" stroke="${OL}" stroke-width="${(size / 5).toFixed(1)}" stroke-linejoin="round" paint-order="stroke">${s}</text>`;
const eye = (x, y, r = 7, iris = OL, dx = 1, dy = 1) => `<circle cx="${x}" cy="${y}" r="${r}" fill="#fff" stroke="${OL}" stroke-width="2.5"/><circle cx="${x + dx}" cy="${y + dy}" r="${(r * 0.55).toFixed(1)}" fill="${iris}"/><circle cx="${x + dx + 1.6}" cy="${y + dy - 1.6}" r="${(r * 0.18).toFixed(1)}" fill="#fff"/>`;
const sparkle = (x, y, s, c = '#fff') => `<path d="M${x} ${y - s}Q${x} ${y} ${x + s} ${y}Q${x} ${y} ${x} ${y + s}Q${x} ${y} ${x - s} ${y}Q${x} ${y} ${x} ${y - s}Z" fill="${c}"/>`;
const face = (fill, cy = 64, r = 36) => `<circle cx="60" cy="${cy}" r="${r}" fill="${fill}" stroke="${OL}" stroke-width="4"/>`;
const burst = (cx, cy, ro, ri, n, fill, extra = '') => {
  const pts = [];
  for (let i = 0; i < n * 2; i++) { const a = (Math.PI * i) / n - Math.PI / 2, r = i % 2 ? ri : ro; pts.push(`${(cx + r * Math.cos(a)).toFixed(1)},${(cy + r * Math.sin(a)).toFixed(1)}`); }
  return `<polygon points="${pts.join(' ')}" fill="${fill}" ${extra}/>`;
};
const tear = (x, y, c = '#5fc8ff') => `<path d="M${x} ${y} Q${x + 5} ${y + 8} ${x} ${y + 12} Q${x - 5} ${y + 8} ${x} ${y}Z" fill="${c}" stroke="${OL}" stroke-width="1.5"/>`;
const note = (x, y, c) => `<path d="M${x} ${y} v-14 l9 -3 v12" fill="none" stroke="${OL}" stroke-width="2.5"/><ellipse cx="${x - 3}" cy="${y}" rx="4" ry="3" fill="${c}" stroke="${OL}" stroke-width="2"/><ellipse cx="${x + 6}" cy="${y - 3}" rx="4" ry="3" fill="${c}" stroke="${OL}" stroke-width="2"/>`;
// round dancer blob used by the dance emotes; `hat` is an accessory drawn on top
const dancer = (fill, hat = '') => `<ellipse cx="46" cy="98" rx="9" ry="5" fill="${fill}" stroke="${OL}" stroke-width="3"/><ellipse cx="74" cy="98" rx="9" ry="5" fill="${fill}" stroke="${OL}" stroke-width="3"/>
  <ellipse cx="60" cy="70" rx="28" ry="26" fill="${fill}" stroke="${OL}" stroke-width="4"/>${hat}
  ${eye(50, 64, 6)}${eye(70, 64, 6)}<path d="M50 78 Q60 87 70 78" fill="#e0405a" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>
  <ellipse cx="42" cy="76" rx="4" ry="2.5" fill="#ff8aa8" opacity=".7"/><ellipse cx="78" cy="76" rx="4" ry="2.5" fill="#ff8aa8" opacity=".7"/>`;

const ART = {
  gg: () => g('a-bob', `<path d="M28 72 Q26 36 60 34 Q94 36 92 72 Q92 98 60 100 Q28 98 28 72Z" fill="#f7a23b" stroke="${OL}" stroke-width="4"/>
    <path d="M40 40 Q46 30 52 38" fill="#f7a23b" stroke="${OL}" stroke-width="3"/>
    ${eye(46, 60, 8, '#2f9b4a')}${eye(74, 60, 8, '#2f9b4a')}
    <circle cx="55" cy="72" r="1.6" fill="${OL}"/><circle cx="65" cy="72" r="1.6" fill="${OL}"/>
    <path d="M36 80 Q60 98 84 80 Q60 88 36 80Z" fill="#a8243a" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>
    <path d="M42 82 l3 5 l3 -4 M72 83 l3 4 l3 -5" fill="#fff" stroke="#fff" stroke-width="1.5"/>
    <circle cx="98" cy="88" r="9" fill="#f7a23b" stroke="${OL}" stroke-width="3"/><path d="M92 80 l-2 -7 l5 4 M98 79 l0 -8 l3 7 M104 81 l3 -6 l0 7" fill="#fff" stroke="${OL}" stroke-width="1.6" stroke-linejoin="round"/>`)
    + g('a-pulse', T('GG', 31, 34, '#ffe14d')),
  nice: () => g('a-spin-slow', burst(60, 60, 50, 34, 12, '#ffd23f', `stroke="${OL}" stroke-width="3" stroke-linejoin="round"`))
    + g('a-twinkle', sparkle(18, 20, 8) + sparkle(102, 98, 7)) + g('a-pulse', T('NICE!', 70, 30, '#fff')),
  cry: () => g('a-sob', `<ellipse cx="42" cy="100" rx="8" ry="5" fill="#f6c6d4" stroke="${OL}" stroke-width="3"/><ellipse cx="78" cy="100" rx="8" ry="5" fill="#f6c6d4" stroke="${OL}" stroke-width="3"/>
    <ellipse cx="44" cy="42" rx="7" ry="10" fill="#f6c6d4" stroke="${OL}" stroke-width="3"/><ellipse cx="76" cy="42" rx="7" ry="10" fill="#f6c6d4" stroke="${OL}" stroke-width="3"/>
    <path d="M22 78 Q22 42 60 42 Q98 42 98 78 Q98 102 60 102 Q22 102 22 78Z" fill="#fde9ef" stroke="${OL}" stroke-width="4"/>
    <path d="M38 62 l8 4 l-8 4 M82 62 l-8 4 l8 4" fill="none" stroke="${OL}" stroke-width="3.5" stroke-linecap="round" stroke-linejoin="round"/>
    <path d="M42 92 Q60 72 78 92 Q60 84 42 92Z" fill="#c2304a" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>`)
    + g('a-drip', tear(40, 72)) + g('a-drip d2', tear(80, 72)) + g('a-drip d3', tear(36, 78)),
  wave: () => `<g>${dancer('#7bd88f', `<path d="M42 48 Q38 34 48 38 M78 48 Q82 34 72 38" fill="none" stroke="${OL}" stroke-width="3"/>`)}</g>`
    + g('a-wave', `<ellipse cx="96" cy="50" rx="10" ry="11" fill="#7bd88f" stroke="${OL}" stroke-width="3"/><path d="M90 42 v-6 M96 40 v-7 M102 42 v-6" stroke="${OL}" stroke-width="2.5" stroke-linecap="round"/>`, 'transform-origin: 30% 100%')
    + g('a-pulse', T('Hi!', 26, 26, '#fff', 34)),
  think: () => face('#ffd54a') + `<path d="M40 50 l12 -3 M68 47 l12 3" stroke="${OL}" stroke-width="3.5" stroke-linecap="round"/>${eye(47, 60, 7, OL, -2, -3)}${eye(73, 60, 7, OL, -2, -3)}
    <path d="M50 84 L72 80" stroke="${OL}" stroke-width="3.5" stroke-linecap="round"/>
    <path d="M40 104 Q38 90 50 88 Q58 88 58 94 Q58 100 50 100" fill="#ffc23a" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>`
    + g('a-float', `<circle cx="96" cy="24" r="16" fill="#fff" stroke="${OL}" stroke-width="3"/><circle cx="84" cy="44" r="4" fill="#fff" stroke="${OL}" stroke-width="2"/>${T('?', 32, 22, '#4fb3ff', 96)}`),
  angry: () => g('a-shake', face('#ff6b4a') + `<path d="M36 46 l18 9 M84 46 l-18 9" stroke="${OL}" stroke-width="5" stroke-linecap="round"/>
    <circle cx="48" cy="62" r="4.5" fill="${OL}"/><circle cx="72" cy="62" r="4.5" fill="${OL}"/>
    <rect x="42" y="76" width="36" height="13" rx="4" fill="#fff" stroke="${OL}" stroke-width="3"/><path d="M51 76 v13 M60 76 v13 M69 76 v13 M42 82.5 h36" stroke="${OL}" stroke-width="2"/>
    <path d="M78 34 l6 6 M84 34 l-6 6 M81 31 v12" stroke="#c0102a" stroke-width="3" stroke-linecap="round"/>`)
    + g('a-puff', `<circle cx="16" cy="30" r="8" fill="#eee"/><circle cx="24" cy="22" r="6" fill="#eee"/>`) + g('a-puff d2', `<circle cx="104" cy="30" r="8" fill="#eee"/><circle cx="96" cy="22" r="6" fill="#eee"/>`),
  laugh: () => g('a-shake-s', face('#ffd54a') + `<path d="M38 58 l9 -8 l9 8 M64 58 l9 -8 l9 8" fill="none" stroke="${OL}" stroke-width="4" stroke-linecap="round" stroke-linejoin="round"/>
    <path d="M38 70 Q60 72 82 70 Q80 96 60 96 Q40 96 38 70Z" fill="#a8243a" stroke="${OL}" stroke-width="3.5" stroke-linejoin="round"/>
    <path d="M48 89 Q60 80 72 89 Q60 96 48 89Z" fill="#ff7a90"/>${tear(30, 56)}${tear(90, 56)}`)
    + g('a-bob', T('HA HA', 22, 20, '#fff')),
  heart: () => g('a-beat', `<path d="M60 102 C18 76 16 44 37 36 C50 31 58 38 60 47 C62 38 70 31 83 36 C104 44 102 76 60 102Z" fill="#ff4d7a" stroke="${OL}" stroke-width="4" stroke-linejoin="round"/>
    <ellipse cx="42" cy="50" rx="7" ry="4.5" fill="#fff" opacity=".7" transform="rotate(-35 42 50)"/>`)
    + g('a-twinkle', sparkle(16, 28, 7, '#ffd0de') + sparkle(104, 24, 6, '#ffd0de')) + g('a-twinkle d2', sparkle(100, 92, 7, '#fff')),
  crown: () => g('a-spin-slow', burst(60, 64, 56, 20, 10, '#ffe27a', 'opacity=".45"'))
    + g('a-bob', `<path d="M22 86 L26 44 L44 64 L60 32 L76 64 L94 44 L98 86 Z" fill="#ffcc33" stroke="${OL}" stroke-width="4" stroke-linejoin="round"/>
    <rect x="20" y="84" width="80" height="14" rx="4" fill="#f0a91e" stroke="${OL}" stroke-width="4"/>
    <circle cx="60" cy="91" r="5" fill="#e33b5a" stroke="${OL}" stroke-width="2"/><circle cx="38" cy="91" r="4" fill="#3fa9ff" stroke="${OL}" stroke-width="2"/><circle cx="82" cy="91" r="4" fill="#4fd17a" stroke="${OL}" stroke-width="2"/>
    <circle cx="26" cy="42" r="4" fill="#fff6c8" stroke="${OL}" stroke-width="2"/><circle cx="60" cy="30" r="4" fill="#fff6c8" stroke="${OL}" stroke-width="2"/><circle cx="94" cy="42" r="4" fill="#fff6c8" stroke="${OL}" stroke-width="2"/>`)
    + g('a-twinkle', sparkle(102, 74, 7) + sparkle(20, 100, 5)),
  deleted: () => g('a-glitch', `<rect x="8" y="30" width="104" height="62" rx="6" fill="#160b14" stroke="#ff3355" stroke-width="3"/>
    <rect x="8" y="42" width="104" height="2" fill="#ff3355" opacity=".18"/><rect x="8" y="58" width="104" height="2" fill="#ff3355" opacity=".18"/><rect x="8" y="74" width="104" height="2" fill="#ff3355" opacity=".18"/>
    ${T('DATA', 60, 24, '#ff3355')}${T('DELETED', 84, 17, '#fff')}`)
    + g('a-flicker', `<rect x="10" y="16" width="8" height="8" fill="#ff3355"/><rect x="100" y="98" width="9" height="9" fill="#ff3355"/><rect x="92" y="18" width="5" height="5" fill="#fff"/><rect x="20" y="100" width="6" height="6" fill="#fff"/>`),
  digivolve: () => g('a-flash', burst(60, 54, 56, 26, 14, '#9ff6ff', 'opacity=".7"'))
    + g('a-shake-s', `<path d="M52 24 h16 v-6 h-16Z" fill="#9aa3b4" stroke="${OL}" stroke-width="3"/>
    <rect x="34" y="22" width="52" height="66" rx="16" fill="#eef1f6" stroke="${OL}" stroke-width="4"/>
    <circle cx="60" cy="48" r="16" fill="#2a3448" stroke="${OL}" stroke-width="3"/>
    <circle cx="44" cy="76" r="4" fill="#4fb3ff" stroke="${OL}" stroke-width="2"/><circle cx="60" cy="78" r="4" fill="#ffcf4a" stroke="${OL}" stroke-width="2"/><circle cx="76" cy="76" r="4" fill="#ff5a6a" stroke="${OL}" stroke-width="2"/>`)
    + g('a-blink', `<circle cx="60" cy="48" r="11" fill="#8ff0ff"/>${sparkle(60, 48, 8, '#fff')}`)
    + T('DIGIVOLVE!', 110, 17, '#7ef9ff'),
  crest: () => `<path d="M60 4 L60 30" stroke="#9aa3b4" stroke-width="3"/>`
    + g('a-swing', `<rect x="32" y="30" width="56" height="70" rx="12" fill="#caa24c" stroke="${OL}" stroke-width="4"/>
    <rect x="40" y="38" width="40" height="54" rx="8" fill="#7a5a1c" stroke="${OL}" stroke-width="2.5"/>`
    + g('a-glow', `<circle cx="60" cy="65" r="10" fill="none" stroke="#fff36b" stroke-width="4"/>
    <path d="M60 44 l5 10 h-10Z M60 86 l5 -10 h-10Z M42 65 l10 5 v-10Z M78 65 l-10 5 v-10Z" fill="#fff36b"/>`), 'transform-origin: 50% 0%')
    + g('a-twinkle', sparkle(20, 46, 7, '#fff36b') + sparkle(100, 84, 6, '#fff36b')),
  meat: () => g('a-bob', `<g transform="rotate(-25 60 64)"><rect x="66" y="58" width="30" height="10" fill="#fff6e8" stroke="${OL}" stroke-width="3"/>
    <circle cx="98" cy="57" r="7" fill="#fff6e8" stroke="${OL}" stroke-width="3"/><circle cx="98" cy="69" r="7" fill="#fff6e8" stroke="${OL}" stroke-width="3"/>
    <ellipse cx="46" cy="63" rx="30" ry="24" fill="#b5532a" stroke="${OL}" stroke-width="4"/><ellipse cx="38" cy="54" rx="13" ry="7" fill="#d9773f"/>
    <circle cx="72" cy="54" r="5" fill="#141a26"/><circle cx="74" cy="64" r="5" fill="#141a26"/></g>`)
    + T('MEAT!', 108, 22, '#ffd27a'),
  sleepy: () => g('a-breathe', `<path d="M24 80 Q24 46 60 46 Q96 46 96 80 Q96 102 60 102 Q24 102 24 80Z" fill="#7ab8ff" stroke="${OL}" stroke-width="4"/>
    <path d="M40 72 q7 5 14 0 M66 72 q7 5 14 0" fill="none" stroke="${OL}" stroke-width="3.5" stroke-linecap="round"/>
    <ellipse cx="60" cy="88" rx="5" ry="4" fill="#25324a"/><path d="M78 80 q4 6 0 10" fill="none" stroke="#bfe3ff" stroke-width="2.5"/>`, 'transform-origin: 50% 100%')
    + g('a-z', T('Z', 40, 16, '#fff', 88)) + g('a-z d2', T('Z', 26, 20, '#fff', 98)) + g('a-z d3', T('Z', 12, 24, '#fff', 108)),
  shock: () => face('#ffe28a') + `<path d="M38 40 q9 -7 18 -2 M64 38 q9 -5 18 2" fill="none" stroke="${OL}" stroke-width="3.5" stroke-linecap="round"/>
    ${eye(47, 58, 10, OL, 0, 0)}${eye(73, 58, 10, OL, 0, 0)}<ellipse cx="60" cy="86" rx="8" ry="10" fill="#4a1420" stroke="${OL}" stroke-width="3"/>`
    + g('a-pop-loop', T('!?', 30, 32, '#ff5a6a', 100)),
  egg: () => g('a-wobble', `<path d="M60 18 C84 18 94 56 94 74 C94 94 78 104 60 104 C42 104 26 94 26 74 C26 56 36 18 60 18Z" fill="#fff6e0" stroke="${OL}" stroke-width="4"/>
    <ellipse cx="46" cy="48" rx="8" ry="10" fill="#6fc3ff"/><ellipse cx="74" cy="66" rx="10" ry="8" fill="#ff9a6a"/><ellipse cx="48" cy="86" rx="9" ry="6" fill="#7bd88f"/>`
    + g('a-crack', `<path d="M30 62 l10 6 l8 -8 l9 9 l8 -9 l9 8 l8 -7 l8 5" fill="none" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>`), 'transform-origin: 50% 100%')
    + T('HATCH!', 116, 17, '#ffe14d'),
  bits: () => `<ellipse cx="60" cy="96" rx="34" ry="9" fill="#e9a520" stroke="${OL}" stroke-width="3"/><ellipse cx="60" cy="88" rx="30" ry="8" fill="#ffcf4a" stroke="${OL}" stroke-width="3"/>
    <ellipse cx="60" cy="80" rx="24" ry="7" fill="#ffdb6e" stroke="${OL}" stroke-width="3"/><path d="M54 80 h12" stroke="#c88a12" stroke-width="2.5"/>`
    + g('a-fall', `<ellipse cx="40" cy="44" rx="8" ry="8" fill="#ffcf4a" stroke="${OL}" stroke-width="2.5"/>`) + g('a-fall d2', `<ellipse cx="78" cy="38" rx="8" ry="8" fill="#ffcf4a" stroke="${OL}" stroke-width="2.5"/>`)
    + g('a-fall d3', `<ellipse cx="60" cy="50" rx="7" ry="7" fill="#ffcf4a" stroke="${OL}" stroke-width="2.5"/>`) + g('a-pulse', T('+BITS', 26, 24, '#ffe14d')),
  hop: () => g('a-hop', dancer('#ffb3d1', `<ellipse cx="48" cy="38" rx="6" ry="15" fill="#ffb3d1" stroke="${OL}" stroke-width="3"/><ellipse cx="72" cy="38" rx="6" ry="15" fill="#ffb3d1" stroke="${OL}" stroke-width="3"/>`), 'transform-origin: 50% 100%')
    + g('a-note', note(18, 40, '#4fb3ff')) + g('a-note d2', note(98, 30, '#ffcf4a')),
  spin: () => g('a-spinx', dancer('#9fd4ff', `<path d="M60 26 l6 20 h-12Z" fill="#ffcf4a" stroke="${OL}" stroke-width="3" stroke-linejoin="round"/>`))
    + g('a-twinkle', sparkle(18, 30, 8) + sparkle(102, 50, 6)) + g('a-note d2', note(100, 26, '#e58bff')),
  wiggle: () => g('a-wiggle', dancer('#c6a3ff', `<path d="M86 84 q16 -4 12 -18" fill="none" stroke="${OL}" stroke-width="4" stroke-linecap="round"/>`), 'transform-origin: 50% 100%')
    + g('a-note', note(16, 34, '#5adf8a')) + g('a-note d3', note(100, 34, '#ff5a6a')),
  slide: () => g('a-slide', dancer('#ffd27a', `<rect x="40" y="57" width="40" height="11" rx="4" fill="${OL}"/><path d="M44 60 h8" stroke="#6ad0ff" stroke-width="2"/>`))
    + `<path d="M10 106 h100" stroke="#ffffff55" stroke-width="3" stroke-dasharray="6 6"/>` + g('a-note d2', note(18, 30, '#ffcf4a')),
  flip: () => g('a-flip', dancer('#ff8a5c', `<path d="M33 52 Q60 40 87 52" fill="none" stroke="#ff3355" stroke-width="7" stroke-linecap="round"/><path d="M86 52 l12 -4 M86 54 l10 6" stroke="#ff3355" stroke-width="4" stroke-linecap="round"/>`))
    + g('a-twinkle', sparkle(16, 20, 8, '#ffe14d') + sparkle(104, 18, 8, '#ffe14d')) + T('WIN!', 116, 18, '#ffe14d'),
  mydance: () => g('a-wiggle', dancer('#7fe0d0', `<path d="M38 58 L24 30 M82 58 L96 30" fill="none" stroke="${OL}" stroke-width="5" stroke-linecap="round"/>`), 'transform-origin: 50% 100%')
    + g('a-note', note(14, 50, '#ffcf4a')) + g('a-note d2', note(102, 52, '#ff5a6a')) + g('a-twinkle', sparkle(60, 14, 7, '#ffe14d')),
};

// Retro (Digimon World) stickers: a DW1-style menu box with a pixel icon and a label
const retroBox = (label, icon, color) => `<rect x="8" y="14" width="104" height="92" rx="6" fill="#16224a" stroke="${OL}" stroke-width="4"/>
    <rect x="13" y="19" width="94" height="82" rx="3" fill="none" stroke="${color}" stroke-width="3" stroke-dasharray="6 3"/>`
  + g('a-bob', icon, 'transform-origin: 50% 60%') + T(label, 98, 15, '#fff') + T('RETRO', 13, 12, color);
const RETRO_ART = {
  retro_hug: () => retroBox('HUG!', `<path d="M60 84 C30 66 28 44 43 38 C52 34 58 40 60 46 C62 40 68 34 77 38 C92 44 90 66 60 84Z" fill="#ff4d7a" stroke="${OL}" stroke-width="3.5"/>`, '#ff8fb0')
    + g('a-twinkle', sparkle(28, 34, 6, '#ffd0de') + sparkle(94, 40, 5, '#ffd0de')),
  retro_cane: () => retroBox('HO HO', `<path d="M66 82 L56 40 Q54 28 64 28 Q72 28 72 36" fill="none" stroke="#c8913f" stroke-width="7" stroke-linecap="round"/><path d="M66 82 L56 40 Q54 28 64 28 Q72 28 72 36" fill="none" stroke="${OL}" stroke-width="2" stroke-linecap="round"/>`, '#ffd27a'),
  retro_ufo: () => retroBox('BEAM', `<path d="M48 54 L36 86 L84 86 L72 54Z" fill="#9ff6ff" opacity=".55"/><ellipse cx="60" cy="50" rx="30" ry="9" fill="#b8c2d8" stroke="${OL}" stroke-width="3"/><path d="M46 46 Q60 26 74 46Z" fill="#7fe0d0" stroke="${OL}" stroke-width="3"/><circle cx="48" cy="51" r="2.5" fill="#ffcf4a"/><circle cx="60" cy="53" r="2.5" fill="#ffcf4a"/><circle cx="72" cy="51" r="2.5" fill="#ffcf4a"/>`, '#7fe0d0'),
  retro_shiver: () => retroBox('BRRR', `<g stroke="#bfe3ff" stroke-width="5" stroke-linecap="round"><path d="M60 30 V82 M38 43 L82 69 M38 69 L82 43"/></g><g stroke="${OL}" stroke-width="1.5" stroke-linecap="round"><path d="M60 30 V82 M38 43 L82 69 M38 69 L82 43"/></g>`, '#bfe3ff'),
  retro_flex: () => retroBox('FLEX', burst(60, 56, 28, 14, 8, '#ffcf4a', `stroke="${OL}" stroke-width="3" stroke-linejoin="round"`) + T('!', 66, 26, '#ff5a6a'), '#ffcf4a'),
  retro_float: () => retroBox('BOO', `<path d="M40 84 V52 Q40 30 60 30 Q80 30 80 52 V84 L73 78 L66 84 L60 78 L54 84 L47 78Z" fill="#eef1f6" stroke="${OL}" stroke-width="3.5" stroke-linejoin="round"/>${eye(52, 52, 5)}${eye(68, 52, 5)}`, '#c6a3ff'),
  retro_swoop: () => retroBox('SWOOP', `<path d="M24 70 Q60 20 96 50 Q70 46 60 64 Q50 48 24 70Z" fill="#ff6b4a" stroke="${OL}" stroke-width="3.5" stroke-linejoin="round"/>`, '#ff9a5a'),
  retro_salute: () => retroBox('SIR!', `<rect x="40" y="32" width="40" height="44" rx="8" fill="#c9d2e3" stroke="${OL}" stroke-width="3.5"/><rect x="44" y="44" width="32" height="10" rx="3" fill="#ff3355" stroke="${OL}" stroke-width="2"/><path d="M80 44 L96 30" stroke="#c9d2e3" stroke-width="7" stroke-linecap="round"/><path d="M80 44 L96 30" stroke="${OL}" stroke-width="2" stroke-linecap="round"/>`, '#9aa3b4'),
  retro_stomp: () => retroBox('STOMP', `<ellipse cx="60" cy="62" rx="20" ry="16" fill="#6fae5a" stroke="${OL}" stroke-width="3.5"/><circle cx="44" cy="40" r="6" fill="#6fae5a" stroke="${OL}" stroke-width="3"/><circle cx="60" cy="36" r="6" fill="#6fae5a" stroke="${OL}" stroke-width="3"/><circle cx="76" cy="40" r="6" fill="#6fae5a" stroke="${OL}" stroke-width="3"/>`, '#9fe07a'),
};
Object.assign(ART, RETRO_ART);

// role: Little Legend animation (DT.Legends.play); move: the procedural body move for dances; mood picks AI emotes;
// retro: the Digimon World model that pops up beside the legend ({ model, act, glyph })
const EMOTES = [
  { id: 'gg', name: 'GG', rarity: 'common', free: true, desc: 'A happy dino says good game.', data: { mood: 'any' } },
  { id: 'nice', name: 'Nice!', rarity: 'common', free: true, desc: 'A starburst of approval.', data: { mood: 'good' } },
  { id: 'cry', name: 'Big Cry', rarity: 'common', free: true, desc: 'A little blob cries its eyes out. Your Little Legend looks sad.', data: { mood: 'bad', role: 'sad' } },
  { id: 'wave', name: 'Hi!', rarity: 'common', free: true, desc: 'Say hello. Your Little Legend waves.', data: { mood: 'any', role: 'wave' } },
  { id: 'think', name: 'Hmm...', rarity: 'common', desc: 'Thinking it over.', data: { mood: 'bad' } },
  { id: 'angry', name: 'Fuming', rarity: 'common', desc: 'Steam out of both ears. Your Little Legend taunts.', data: { mood: 'bad', role: 'taunt' } },
  { id: 'laugh', name: 'Ha Ha', rarity: 'common', desc: 'Laughing to tears. Your Little Legend cheers.', data: { mood: 'good', role: 'happy' } },
  { id: 'heart', name: 'Heart', rarity: 'common', desc: 'A beating heart.', data: { mood: 'any' } },
  { id: 'sleepy', name: 'Zzz', rarity: 'common', desc: 'Is it my turn yet?', data: { mood: 'any' } },
  { id: 'shock', name: 'What?!', rarity: 'common', desc: 'Did not see that coming.', data: { mood: 'bad' } },
  { id: 'meat', name: 'Meat!', rarity: 'rare', desc: 'Snack time. Your Little Legend eats.', data: { mood: 'good', role: 'eat' } },
  { id: 'bits', name: '+Bits', rarity: 'rare', desc: 'Coins raining onto the pile.', data: { mood: 'good' } },
  { id: 'egg', name: 'Hatching', rarity: 'rare', desc: 'A Digi-Egg wobbles and cracks.', data: { mood: 'any' } },
  { id: 'crown', name: 'Crowned', rarity: 'rare', desc: 'Top of the lobby. Your Little Legend cheers.', data: { mood: 'good', role: 'happy' } },
  { id: 'hop', name: 'Hop Dance', rarity: 'rare', desc: 'Dance: your Little Legend bounces on the spot.', data: { mood: 'good', role: 'dance', move: 'hop' } },
  { id: 'wiggle', name: 'Wiggle Dance', rarity: 'rare', desc: 'Dance: your Little Legend wiggles side to side.', data: { mood: 'good', role: 'dance', move: 'wiggle' } },
  { id: 'deleted', name: 'Data Deleted', rarity: 'epic', desc: 'A glitching error screen. Your Little Legend taunts.', data: { mood: 'good', role: 'taunt' } },
  { id: 'digivolve', name: 'Digivolve!', rarity: 'epic', desc: 'The device on the screen flashes.', data: { mood: 'good' } },
  { id: 'crest', name: 'Crest Glow', rarity: 'epic', desc: 'A crest tag lights up. Your Little Legend cheers.', data: { mood: 'good', role: 'happy' } },
  { id: 'slide', name: 'Slide Dance', rarity: 'epic', desc: 'Dance: your Little Legend slides back and forth.', data: { mood: 'good', role: 'dance', move: 'slide' } },
  { id: 'spin', name: 'Spin Dance', rarity: 'epic', desc: 'Dance: your Little Legend spins twice.', data: { mood: 'good', role: 'dance', move: 'spin' } },
  { id: 'flip', name: 'Victory Flip', rarity: 'legendary', desc: 'Dance: your Little Legend jumps and flips.', data: { mood: 'good', role: 'dance', move: 'flip' } },
  { id: 'mydance', name: 'My Dance', rarity: 'common', free: true, desc: 'Your Little Legend does your equipped dance (pick it in the Locker).', data: { mood: 'good', role: 'dance' } },
  { id: 'retro_hug', name: 'Monzaemon Hug', rarity: 'epic', tag: 'Retro', desc: 'Retro: Monzaemon pops up, hugs your Little Legend and sends hearts.', data: { mood: 'good', role: 'happy', retro: { model: 'extra:DW1_Monzaemon', act: 'hug', glyph: { ch: '♥', color: '#ff4d7a' }, size: 0.85 } } },
  { id: 'retro_cane', name: "Jijimon's Cane", rarity: 'rare', tag: 'Retro', desc: 'Retro: Jijimon shows up and waves his cane.', data: { mood: 'any', role: 'wave', retro: { model: 'extra:DW1_Jijimon', act: 'cane', glyph: { ch: '!', color: '#ffd27a' } } } },
  { id: 'retro_ufo', name: 'Vademon UFO', rarity: 'epic', tag: 'Retro', desc: 'Retro: Vademon hovers over your Little Legend with a tractor beam.', data: { mood: 'bad', role: 'sad', retro: { model: 'extra:DW1_Vademon', act: 'ufo', size: 0.6 } } },
  { id: 'retro_shiver', name: 'Frigimon Shiver', rarity: 'rare', tag: 'Retro', desc: 'Retro: Frigimon pops up shivering in a snow flurry.', data: { mood: 'bad', retro: { model: 'extra:DW1_Frigimon', act: 'shiver', glyph: { ch: '❄', color: '#bfe3ff' } } } },
  { id: 'retro_flex', name: 'Mamemon Flex', rarity: 'rare', tag: 'Retro', desc: 'Retro: Mamemon flexes beside your Little Legend.', data: { mood: 'good', role: 'taunt', retro: { model: 'extra:DW1_Mamemon', act: 'flex', glyph: { ch: '★', color: '#ffcf4a' }, size: 0.55 } } },
  { id: 'retro_float', name: 'Bakemon Float', rarity: 'rare', tag: 'Retro', desc: 'Retro: Bakemon floats up and sways.', data: { mood: 'any', retro: { model: 'extra:DW1_Bakemon', act: 'float', glyph: { ch: '~', color: '#c6a3ff' } } } },
  { id: 'retro_swoop', name: 'Airdramon Swoop', rarity: 'epic', tag: 'Retro', desc: 'Retro: Airdramon swoops past your Little Legend.', data: { mood: 'good', role: 'happy', retro: { model: 'extra:DW1_Airdramon', act: 'swoop', size: 0.9 } } },
  { id: 'retro_salute', name: 'Andromon Salute', rarity: 'rare', tag: 'Retro', desc: 'Retro: Andromon steps in and salutes.', data: { mood: 'good', retro: { model: 'extra:DW1_Andromon', act: 'salute', glyph: { ch: '!', color: '#ff3355' }, size: 0.8 } } },
  { id: 'retro_stomp', name: 'Brachiomon Stomp', rarity: 'legendary', tag: 'Retro', desc: 'Retro: Brachiomon stomps the ground beside your Little Legend.', data: { mood: 'good', role: 'taunt', retro: { model: 'extra:DW1_Brachiomon', act: 'stomp', size: 0.85 } } },
];
const PRICE = { common: 150, rare: 400, epic: 900, legendary: 1800 };

for (const item of stickers) { ART[item.id] = item.art; EMOTES.push(item); }
export function stickerHTML(id) { return ART[id] ? svg(ART[id]()) : ''; }

// ---------- styles (kept here so the feature lives in one file) ----------
function injectCss() {
  if (document.getElementById('emoteCss')) return;
  const st = document.createElement('style');
  st.id = 'emoteCss';
  st.textContent = `
.em-svg { width: 100%; height: 100%; display: block; overflow: visible; }
.em-svg .a { transform-box: fill-box; transform-origin: center; }
.em-svg .d2 { animation-delay: -.4s !important; } .em-svg .d3 { animation-delay: -.8s !important; }
.a-bob { animation: emBob 1s ease-in-out infinite; } @keyframes emBob { 50% { transform: translateY(-5px); } }
.a-pulse { animation: emPulse .9s ease-in-out infinite; } @keyframes emPulse { 50% { transform: scale(1.12); } }
.a-spin-slow { animation: emSpin 6s linear infinite; } @keyframes emSpin { to { transform: rotate(360deg); } }
.a-twinkle { animation: emTwinkle 1.1s ease-in-out infinite; } @keyframes emTwinkle { 50% { opacity: .2; transform: scale(.6); } }
.a-sob { animation: emSob .35s ease-in-out infinite; } @keyframes emSob { 50% { transform: translateY(2px) scaleY(.97); } }
.a-drip { animation: emDrip 1.2s ease-in infinite; } @keyframes emDrip { 0% { transform: translateY(0); opacity: 1; } 100% { transform: translateY(30px); opacity: 0; } }
.a-wave { animation: emWave .5s ease-in-out infinite alternate; } @keyframes emWave { from { transform: rotate(-18deg); } to { transform: rotate(22deg); } }
.a-float { animation: emFloat 1.6s ease-in-out infinite; } @keyframes emFloat { 50% { transform: translate(-3px, -6px); } }
.a-shake { animation: emShake .25s linear infinite; } @keyframes emShake { 25% { transform: translate(-2px, 1px) rotate(-2deg); } 75% { transform: translate(2px, -1px) rotate(2deg); } }
.a-shake-s { animation: emShakeS .3s linear infinite; } @keyframes emShakeS { 25% { transform: rotate(-3deg); } 75% { transform: rotate(3deg); } }
.a-puff { animation: emPuff 1s ease-out infinite; } @keyframes emPuff { 0% { transform: translateY(6px) scale(.5); opacity: 0; } 40% { opacity: 1; } 100% { transform: translateY(-10px) scale(1.2); opacity: 0; } }
.a-beat { animation: emBeat .8s ease-in-out infinite; } @keyframes emBeat { 15% { transform: scale(1.14); } 30% { transform: scale(.98); } 45% { transform: scale(1.08); } }
.a-glitch { animation: emGlitch 1.4s steps(1) infinite; } @keyframes emGlitch { 0% { transform: none; } 10% { transform: translateX(-4px) skewX(8deg); } 14% { transform: translateX(3px); } 18% { transform: none; } 60% { transform: translateX(5px) skewX(-6deg); } 64% { transform: none; } }
.a-flicker { animation: emFlicker .5s steps(2) infinite; } @keyframes emFlicker { 50% { opacity: 0; } }
.a-flash { animation: emFlash .7s ease-in-out infinite; } @keyframes emFlash { 0%, 100% { opacity: .3; transform: scale(.85) rotate(0deg); } 50% { opacity: 1; transform: scale(1.1) rotate(10deg); } }
.a-blink { animation: emBlink .7s ease-in-out infinite; } @keyframes emBlink { 50% { opacity: .35; } }
.a-glow { animation: emGlow 1.2s ease-in-out infinite; } @keyframes emGlow { 0%, 100% { opacity: .45; filter: none; } 50% { opacity: 1; filter: drop-shadow(0 0 6px #fff36b); } }
.a-swing { animation: emSwing 2s ease-in-out infinite; } @keyframes emSwing { 0%, 100% { transform: rotate(-7deg); } 50% { transform: rotate(7deg); } }
.a-breathe { animation: emBreathe 2s ease-in-out infinite; } @keyframes emBreathe { 50% { transform: scaleY(.93) scaleX(1.04); } }
.a-z { animation: emZ 2s ease-out infinite; } @keyframes emZ { 0% { opacity: 0; transform: translate(-8px, 8px); } 30% { opacity: 1; } 100% { opacity: 0; transform: translate(4px, -6px); } }
.a-pop-loop { animation: emPopLoop 1s ease-out infinite; } @keyframes emPopLoop { 0% { transform: scale(.6); } 25% { transform: scale(1.25); } 50%, 100% { transform: scale(1); } }
.a-wobble { animation: emWobble 1.1s ease-in-out infinite; } @keyframes emWobble { 0%, 100% { transform: rotate(0); } 20% { transform: rotate(-9deg); } 40% { transform: rotate(8deg); } 60% { transform: rotate(-4deg); } 80% { transform: rotate(2deg); } }
.a-crack { animation: emCrack 2.2s ease-out infinite; } @keyframes emCrack { 0%, 30% { opacity: 0; } 45%, 100% { opacity: 1; } }
.a-fall { animation: emFall 1.3s ease-in infinite; } @keyframes emFall { 0% { transform: translateY(-30px); opacity: 0; } 20% { opacity: 1; } 80% { opacity: 1; } 100% { transform: translateY(26px); opacity: 0; } }
.a-hop { animation: emHop .5s ease-in-out infinite; } @keyframes emHop { 0%, 100% { transform: translateY(0) scale(1.08, .92); } 50% { transform: translateY(-16px) scale(.95, 1.05); } }
.a-spinx { animation: emSpinX .9s linear infinite; } @keyframes emSpinX { 0% { transform: scaleX(1); } 25% { transform: scaleX(.05); } 50% { transform: scaleX(-1); } 75% { transform: scaleX(.05); } 100% { transform: scaleX(1); } }
.a-wiggle { animation: emWiggle .45s ease-in-out infinite alternate; } @keyframes emWiggle { from { transform: rotate(-14deg); } to { transform: rotate(14deg); } }
.a-slide { animation: emSlide 1.4s ease-in-out infinite; } @keyframes emSlide { 0%, 100% { transform: translateX(-16px) skewX(10deg); } 50% { transform: translateX(16px) skewX(-10deg); } }
.a-flip { animation: emFlip 1.3s ease-in-out infinite; } @keyframes emFlip { 0%, 30% { transform: translateY(0) rotate(0); } 65% { transform: translateY(-22px) rotate(-180deg); } 100% { transform: translateY(0) rotate(-360deg); } }
.a-note { animation: emNote 1.6s ease-in-out infinite; } @keyframes emNote { 0% { transform: translateY(6px); opacity: 0; } 30% { opacity: 1; } 100% { transform: translateY(-14px) rotate(12deg); opacity: 0; } }
.em-prev { width: 100%; height: 100%; display: flex; align-items: center; justify-content: center; }
.em-prev > .em-svg { width: min(100%, 120px); height: auto; aspect-ratio: 1; }
#emoteLayer { position: absolute; inset: 0; pointer-events: none; overflow: hidden; z-index: 3; }
.em-big, .em-row { position: absolute; left: 0; top: 0; pointer-events: none; }
.em-big { width: 112px; height: 112px; }
.em-row { position: fixed; width: 50px; height: 50px; z-index: 25; }
.em-in { width: 100%; height: 100%; filter: drop-shadow(0 3px 3px rgba(0,0,0,.6)); animation: emIn .35s cubic-bezier(.2,1.6,.4,1) both; }
.em-row .em-in { animation-name: emInRow; }
.out > .em-in { animation: emOut .3s ease-in forwards; }
@keyframes emIn { from { transform: scale(.2) translateY(20px); opacity: 0; } to { transform: none; opacity: 1; } }
@keyframes emInRow { from { transform: translateX(30px) scale(.3); opacity: 0; } to { transform: none; opacity: 1; } }
@keyframes emOut { to { transform: scale(.6) translateY(-16px); opacity: 0; } }
#emoteWheel { position: fixed; left: 0; top: 0; width: 0; height: 0; z-index: 35; pointer-events: none; }
#emoteWheel.hidden { display: none; }
#emoteWheel .ering { position: absolute; left: -148px; top: -148px; width: 296px; height: 296px; border-radius: 50%; border: 54px solid rgba(12,16,26,.82); box-shadow: 0 6px 24px rgba(0,0,0,.5); animation: emIn .2s ease-out both; }
#emoteWheel .ew { position: absolute; width: 82px; height: 82px; margin: -41px 0 0 -41px; padding: 5px; border-radius: 50%; pointer-events: auto; background: var(--panel2); border: 2px solid var(--line2); transition: transform .1s; }
#emoteWheel .ew:hover, #emoteWheel .ew:focus-visible { border-color: var(--gold); background: var(--panel3); transform: scale(1.12); outline: none; }
#emoteWheel .ew.empty { border-style: dashed; color: var(--muted); font-size: 11px; white-space: normal; line-height: 1.1; }
#emoteWheel .ew kbd { position: absolute; left: -3px; top: -3px; min-width: 20px; padding: 1px 5px; border-radius: 9px; font: 800 11px system-ui, sans-serif; background: var(--gold); color: #1b1424; }
#emoteWheel .ec { position: absolute; left: -46px; top: -24px; width: 92px; height: 48px; display: flex; align-items: center; justify-content: center; text-align: center; font-weight: 700; font-size: 13px; color: var(--text); text-shadow: 0 1px 2px #000; }
#emoteWheel .emute { position: absolute; left: 0; top: 176px; transform: translateX(-50%); pointer-events: auto; font-size: 12px; padding: 4px 10px; }
#emoteWheel .emute.on { border-color: var(--bad); color: var(--bad); }
#emoteBtn.on { border-color: var(--gold); }
`;
  document.head.appendChild(st);
}

const rarityOrder = { common: 0, rare: 1, epic: 2, legendary: 3 };
for (const item of EMOTES) if (item.data?.retro) item.desc = `Digimon World 1 emote. ${item.desc} Shows a 3D Digimon beside your Little Legend when used in a match.`;
DT.Cosmetics.register('emote', EMOTES.map((e) => {
  const html = stickerHTML(e.id);
  return Object.assign({}, e, {
    price: e.free ? 0 : PRICE[e.rarity],
    icon: 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(html),
    preview(el) { injectCss(); el.innerHTML = `<div class="em-prev">${html}</div>`; },
  });
}).sort((a, b) => (b.free ? 1 : 0) - (a.free ? 1 : 0) || rarityOrder[a.rarity] - rarityOrder[b.rarity]));

// ---------- state ----------
const store = {
  get(k) { try { return localStorage.getItem(k); } catch (e) { return null; } },
  set(k, v) { try { localStorage.setItem(k, v); } catch (e) { /* private mode */ } },
};
let muteOthers = store.get(MUTE_KEY) === '1';
const sent = new Map(); // slot -> recent emote times
const active = new Map(); // slot -> { big, row, until, id }
const poses = new Map(); // slot -> procedural dance move on that Little Legend: { move, t0, dur }
let layer = null;

function allow(slot) {
  const now = performance.now();
  const list = (sent.get(slot) || []).filter((t) => now - t < RATE_MS);
  if (list.length >= RATE_N) { sent.set(slot, list); return false; }
  list.push(now);
  sent.set(slot, list);
  return true;
}

const game = () => (DT.UI && DT.UI.game ? DT.UI.game() : null);
const rend = () => (DT.UI && DT.UI.renderer ? DT.UI.renderer() : null);
const overlayUp = () => { const o = document.getElementById('overlay'); return !!o && !o.classList.contains('hidden') && getComputedStyle(o).display !== 'none'; };

// ---------- display ----------
function ensureLayer() {
  if (layer && layer.isConnected) return layer;
  const arena = document.getElementById('arena');
  if (!arena) return null;
  layer = document.createElement('div');
  layer.id = 'emoteLayer';
  arena.appendChild(layer);
  return layer;
}

function sticker(cls, id, parent) {
  const el = document.createElement('div');
  el.className = cls;
  el.innerHTML = `<div class="em-in">${stickerHTML(id)}</div>`;
  el.style.visibility = 'hidden';
  parent.appendChild(el);
  return el;
}

function clear(slot) {
  const a = active.get(slot);
  if (!a) return;
  a.big.remove();
  a.row.remove();
  active.delete(slot);
}

function show(slot, item) {
  injectCss();
  const L = ensureLayer();
  if (!L) return;
  clear(slot);
  active.set(slot, { id: item.id, until: performance.now() + SHOW_MS, big: sticker('em-big', item.id, L), row: sticker('em-row', item.id, document.body) });
  playRole(slot, item);
  if (item.data && item.data.retro) popIn(slot, item);
}

// ---------- retro emotes: a small Digimon World Digimon pops up beside the legend ----------
// item.data.retro = { model: models/index.json key, act, glyph?, size? }. The model loads on first use (the template
// stays cached by render3d), the clone shares its geometry and materials, and everything this adds is removed after
// POP_MS. Sprites and puffs own their materials, which are disposed.
const POP_MS = 2500;
const pops = new Map(); // slot -> { holder, obj, view, t0, act, fx: [], beam, nextFx }
const PUFF = new THREE.SphereGeometry(0.09, 10, 8);
const glyphTex = new Map();
function glyphTexture(ch, color) {
  const k = `${ch}|${color}`;
  if (glyphTex.has(k)) return glyphTex.get(k);
  const c = document.createElement('canvas');
  c.width = c.height = 64;
  const x = c.getContext('2d');
  x.font = 'bold 46px "Segoe UI Symbol", "Arial Black", sans-serif';
  x.textAlign = 'center';
  x.textBaseline = 'middle';
  x.lineWidth = 6;
  x.strokeStyle = OL;
  x.strokeText(ch, 32, 35);
  x.fillStyle = color;
  x.fillText(ch, 32, 35);
  const t = new THREE.CanvasTexture(c);
  t.colorSpace = THREE.SRGBColorSpace;
  glyphTex.set(k, t);
  return t;
}
function clearPop(slot) {
  const p = pops.get(slot);
  if (!p) return;
  pops.delete(slot);
  if (p.holder && p.holder.parent) p.holder.parent.remove(p.holder);
  for (const f of p.fx || []) { if (f.o.parent) f.o.parent.remove(f.o); f.o.material.dispose(); }
  if (p.beam) { p.beam.geometry.dispose(); p.beam.material.dispose(); }
}
function popIn(slot, item) {
  const R = item.data && item.data.retro;
  if (!R || !legendView(slot)) return;
  clearPop(slot);
  const p = { pending: true, t0: performance.now(), act: R.act, glyph: R.glyph, fx: [], nextFx: 0 };
  pops.set(slot, p);
  loadTemplate(R.model).then((t) => {
    const v = legendView(slot);
    if (pops.get(slot) !== p) return;
    if (!t || !v || !v.group.parent) { pops.delete(slot); return; }
    const obj = SkeletonUtils.clone(t.root);
    const src = DT.MODEL_SOURCES && DT.MODEL_SOURCES[R.model];
    obj.userData.rotY = src && src.rotY ? src.rotY : 0;
    obj.userData.s = Math.min(R.size || 0.7, 1.05 / Math.max(t.footprint || 1, 0.01));
    const holder = new THREE.Group();
    holder.add(obj);
    holder.scale.setScalar(0.001);
    v.group.parent.add(holder);
    Object.assign(p, { pending: false, holder, obj, view: v, t0: performance.now() });
    if (R.act === 'ufo') {
      const beam = new THREE.Mesh(new THREE.CylinderGeometry(0.12, 0.5, 1, 20, 1, true),
        new THREE.MeshBasicMaterial({ color: '#9ff6ff', transparent: true, opacity: 0, depthWrite: false, side: THREE.DoubleSide, blending: THREE.AdditiveBlending }));
      p.beam = beam;
      holder.parent.add(beam);
      p.fx.push({ o: beam, beam: true });
    }
  });
}
function addFx(p, kind, at, now) {
  let o;
  if (kind === 'puff') o = new THREE.Mesh(PUFF, new THREE.MeshBasicMaterial({ color: '#ffffff', transparent: true, opacity: 0.9, depthWrite: false }));
  else { o = new THREE.Sprite(new THREE.SpriteMaterial({ map: glyphTexture(kind.ch, kind.color), transparent: true, depthWrite: false })); o.scale.setScalar(0.32); }
  o.position.copy(at);
  p.holder.parent.add(o);
  const a = Math.random() * Math.PI * 2;
  p.fx.push({ o, t0: now, life: kind === 'puff' ? 0.5 : 1.1, vx: Math.cos(a) * (kind === 'puff' ? 0.9 : 0.15), vz: Math.sin(a) * (kind === 'puff' ? 0.9 : 0.15), vy: kind === 'puff' ? 0.4 : 0.7, puff: kind === 'puff' });
}
const _side = new THREE.Vector3();
const _at = new THREE.Vector3();
function updatePops(now) {
  const r = rend();
  for (const [slot, p] of [...pops]) {
    if (p.pending) { if (now - p.t0 > 4000) pops.delete(slot); continue; }
    const t = (now - p.t0) / 1000, dur = POP_MS / 1000;
    if (t >= dur + 0.5 || p.view.disposed || !p.view.group.parent || !r) { clearPop(slot); continue; }
    // beside the legend, on the camera's right
    const L = p.view.group.position;
    _side.set(1, 0, 0);
    if (r.camera) { _side.setFromMatrixColumn(r.camera.matrixWorld, 0); _side.y = 0; if (_side.lengthSq() < 1e-6) _side.set(1, 0, 0); _side.normalize(); }
    let off = 0.95, y = 0, yaw = 0, rz = 0, rx = 0, sx = 1, sy = 1, jitter = 0;
    const k = Math.max(0, t - 0.25);
    switch (p.act) {
      case 'hug': off = 0.95 - 0.5 * Math.min(1, k / 0.6); sy = 1 + 0.08 * Math.sin(k * 10); sx = 1 - 0.05 * Math.sin(k * 10); break;
      case 'cane': rz = Math.sin(k * 9) * 0.35; y = Math.abs(Math.sin(k * 9)) * 0.04; break;
      case 'ufo': y = 1.05 + Math.sin(k * 4) * 0.08; rz = Math.sin(k * 3) * 0.12; off = 0.35; break;
      case 'shiver': jitter = Math.sin(t * 70) * 0.035; sy = 1 + 0.03 * Math.sin(t * 40); break;
      case 'flex': { const b = Math.max(0, Math.sin(k * 5)); sx = 1 + 0.18 * b; sy = 1 - 0.06 * b; y = b * 0.05; break; }
      case 'float': y = 0.35 + Math.sin(k * 3) * 0.12; rz = Math.sin(k * 2.2) * 0.15; break;
      case 'swoop': { const q = Math.min(1, k / 1.8); off = 1.7 - 3.4 * q; y = 0.45 + 0.75 * Math.abs(2 * q - 1); yaw = Math.PI / 2; break; }
      case 'salute': rx = -0.25 * Math.min(1, k * 4, Math.max(0, (1.8 - k) * 4)); rz = 0.08 * Math.min(1, k * 4); break;
      case 'stomp': { const ph = (k * 1.6) % 1; y = Math.sin(ph * Math.PI) * 0.35; sy = ph < 0.08 ? 0.82 : 1; if (ph < p.lastPh) for (let i = 0; i < 4; i++) addFx(p, 'puff', _at.copy(L).addScaledVector(_side, off).setY(0.12), now); p.lastPh = ph; break; }
      default: break;
    }
    _at.copy(L).addScaledVector(_side, off + jitter);
    p.holder.position.set(_at.x, L.y + y, _at.z);
    // face the camera, turned a little toward the legend
    const cam = r.camera ? r.camera.position : null;
    const face = cam ? Math.atan2(cam.x - _at.x, cam.z - _at.z) : 0;
    const o = p.obj;
    o.rotation.set(rx, o.userData.rotY + yaw, rz);
    p.holder.rotation.y = face - (yaw ? 0 : 0.5 * Math.sign(off || 1));
    // pop in with an overshoot, poof out at the end
    const pin = Math.min(1, t / 0.25), pout = Math.max(0, Math.min(1, (t - dur + 0.3) / 0.3));
    const sc = (pin < 1 ? Math.sin(pin * Math.PI * 0.5) * (1 + 0.25 * Math.sin(pin * Math.PI)) : 1) * (1 - pout);
    o.scale.set(o.userData.s * sx, o.userData.s * sy, o.userData.s * sx);
    p.holder.scale.setScalar(Math.max(0.001, sc));
    if (pout > 0 && !p.poofed) { p.poofed = true; for (let i = 0; i < 9; i++) addFx(p, 'puff', _at.clone().setY(L.y + y + 0.35), now); }
    if (p.glyph && t > 0.3 && t < dur - 0.4 && now >= p.nextFx) { p.nextFx = now + 280; addFx(p, p.glyph, _at.clone().setY(L.y + y + 0.8), now); }
    // effects: rising glyphs, expanding puffs, the UFO beam
    for (let i = p.fx.length - 1; i >= 0; i--) {
      const f = p.fx[i];
      if (f.beam) {
        f.o.position.set(_at.x, L.y + (y + 0.0) / 2, _at.z);
        f.o.scale.set(1, Math.max(0.01, y), 1);
        f.o.material.opacity = 0.45 * Math.min(1, k * 3) * (1 - pout);
        continue;
      }
      const a = (now - f.t0) / 1000 / f.life;
      if (a >= 1) { if (f.o.parent) f.o.parent.remove(f.o); f.o.material.dispose(); p.fx.splice(i, 1); continue; }
      const d = 1 / 60;
      f.o.position.x += f.vx * d; f.o.position.z += f.vz * d; f.o.position.y += f.vy * d;
      f.o.material.opacity = (f.puff ? 0.9 : 1) * (1 - a);
      if (f.puff) f.o.scale.setScalar(1 + a * 1.6);
    }
  }
}


// Screen point above a Tamer's Little Legend, if it is on this screen (yours always; the opponent's in combat).
function legendScreen(slot) {
  const g = game(), r = rend();
  if (!g || !r) return null;
  if (DT.Legends && typeof DT.Legends.anchor === 'function') { try { return DT.Legends.anchor(slot); } catch (e) { return null; } }
  if (g.human && slot === g.human.id && r.legendPos) { const p = r.legendPos(); return r.project(p.x, 1.4, p.z); }
  return null;
}

function place(slot, a, now) {
  const r = rend();
  const fading = now > a.until - 300;
  a.big.classList.toggle('out', fading);
  a.row.classList.toggle('out', fading);
  // over the legend
  const s = legendScreen(slot);
  let onScreen = false;
  if (s && r && !overlayUp()) {
    const cw = r.canvas.clientWidth, ch = r.canvas.clientHeight;
    onScreen = !s.behind && s.x > -20 && s.x < cw + 20 && s.y > -40 && s.y < ch + 60;
    // keep the whole sticker inside the arena when the legend stands near the top or sides
    const x = Math.max(58, Math.min(cw - 58, s.x)), y = Math.max(118, Math.min(ch - 4, s.y));
    if (onScreen) a.big.style.transform = `translate(${x.toFixed(1)}px, ${y.toFixed(1)}px) translate(-50%, -100%)`;
  }
  a.big.style.visibility = onScreen ? 'visible' : 'hidden';
  // beside the row in the Tamers list
  const row = document.querySelector(`#players .pl[data-pid="${slot}"]`);
  const rb = row && row.getBoundingClientRect();
  if (rb && rb.height > 0 && !overlayUp()) {
    a.row.style.transform = `translate(${(rb.left - 54).toFixed(1)}px, ${(rb.top + rb.height / 2 - 25).toFixed(1)}px)`;
    a.row.style.visibility = 'visible';
  } else a.row.style.visibility = 'hidden';
}

// ---------- Little Legend animation ----------
// Procedural moves, applied to the legend's body only while the frame is drawn (see hookRenderer).
const MOVES = {
  hop: (t) => ({ y: Math.abs(Math.sin(t * 9)) * 0.5, sy: 1 + 0.1 * Math.cos(t * 18) }),
  spin: (t, k) => ({ ry: k * Math.PI * 4, y: Math.abs(Math.sin(t * 6)) * 0.15 }),
  wiggle: (t) => ({ rz: Math.sin(t * 14) * 0.35, x: Math.sin(t * 7) * 0.15 }),
  slide: (t) => ({ x: Math.sin(t * 3.6) * 0.8, ry: Math.cos(t * 3.6) * 1.1, y: Math.abs(Math.sin(t * 7.2)) * 0.06 }),
  flip: (t, k) => { const ph = (k * 2) % 1; return { y: Math.sin(ph * Math.PI) * 1.1, rx: -ph * Math.PI * 2, pivot: 0.7 }; },
  happy: (t) => ({ y: Math.abs(Math.sin(t * 8)) * 0.3 }),
  sad: (t, k) => ({ rx: 0.4 * Math.min(1, k * 5, (1 - k) * 5), rz: Math.sin(t * 3) * 0.06 }),
  taunt: (t) => ({ ry: Math.sin(t * 16) * 0.5, y: Math.abs(Math.sin(t * 8)) * 0.08 }),
  eat: (t) => ({ rx: (Math.sin(t * 14) * 0.5 + 0.5) * 0.4 }),
  wave: (t) => ({ rz: Math.sin(t * 6) * 0.22 }),
};
const SMOOTH = { hop: 1, wiggle: 1, slide: 1, happy: 1, taunt: 1, eat: 1, wave: 1 };

function poseNow(slot, now) {
  const pose = poses.get(slot);
  if (!pose) return null;
  const k = (now - pose.t0) / pose.dur;
  if (k >= 1 || k < 0) { poses.delete(slot); return null; }
  const o = MOVES[pose.move]((now - pose.t0) / 1000, k);
  if (SMOOTH[pose.move]) { const e = Math.min(1, k * 8, (1 - k) * 8); for (const key of Object.keys(o)) o[key] = key === 'sy' ? 1 + (o[key] - 1) * e : o[key] * e; }
  return o;
}

function playRole(slot, item) {
  const d = item.data || {};
  if (!d.role) return;
  let handled = false;
  // dance emotes play the Tamer's equipped dance on the legend's bones (js/dances.js)
  if (d.role === 'dance' && DT.Dances) { try { handled = DT.Dances.playFor(slot); } catch (e) { console.warn(e); } }
  if (!handled && DT.Legends && typeof DT.Legends.play === 'function') {
    try { handled = DT.Legends.play(slot, d.role, { move: d.move, emote: item.id }) !== false; } catch (e) { console.warn(e); }
  }
  // dances always get their own move on top, so each one looks different; other roles only when DT.Legends can't play
  if (d.move || !handled) {
    const move = d.move || d.role;
    if (MOVES[move] && legendView(slot)) poses.set(slot, { move, t0: performance.now(), dur: d.move ? 2600 : 1600 });
  }
}

// the 3D view of a Tamer's Little Legend: renderer.legend (yours) or renderer.oppLegend (combat opponent)
function legendView(slot) {
  const g = game(), r = rend();
  if (!g || !r) return null;
  if (g.human && slot === g.human.id) return r.legend ? r.legend.view : null;
  const O = r.oppLegend;
  return O && O.playerId === slot && O.view && O.view.group.visible ? O.view : null;
}

function hookRenderer(r) {
  if (!r || r.__emoteHook) return;
  r.__emoteHook = true;
  const orig = r.render.bind(r);
  r.render = function () {
    if (!poses.size) return orig();
    const now = performance.now();
    const undo = [];
    for (const slot of [...poses.keys()]) {
      const o = poseNow(slot, now);
      const v = o && legendView(slot);
      if (!v) continue;
      const b = v.body, p = b.position, q = b.rotation, s = b.scale;
      undo.push([b, p.x, p.y, p.z, q.x, q.y, q.z, s.y]);
      let y = o.y || 0, z = 0;
      if (o.rx && o.pivot) { y += o.pivot - o.pivot * Math.cos(o.rx); z -= o.pivot * Math.sin(o.rx); }
      p.set(p.x + (o.x || 0), p.y + y, p.z + z);
      q.set(q.x + (o.rx || 0), q.y + (o.ry || 0), q.z + (o.rz || 0));
      s.y *= o.sy || 1;
    }
    try { orig(); } finally {
      for (const [b, px, py, pz, qx, qy, qz, sy] of undo) { b.position.set(px, py, pz); b.rotation.set(qx, qy, qz); b.scale.y = sy; }
    }
  };
}

// ---------- receiving ----------
DT.Social.on('emote', (slot, id) => {
  const g = game();
  const item = DT.Cosmetics.get('emote', id);
  slot = Number(slot);
  if (!g || !item || !Number.isInteger(slot) || slot < 0 || slot > 7) return;
  const mine = g.human && slot === g.human.id;
  if (!mine && (muteOthers || !allow(slot))) return; // your own sends are rate-checked before sending
  show(slot, item);
});

// ---------- sending ----------
export function sendEmote(id) {
  const g = game();
  if (!g || !g.human || !id || !DT.Cosmetics.owns('emote', id)) return false;
  if (!allow(g.human.id)) { if (DT.UI.toast) DT.UI.toast('Too many emotes. Wait a few seconds.', 'bad'); return false; }
  DT.Social.send('emote', id, g.human.id);
  return true;
}

// ---------- the wheel ----------
let wheel = null;
function buildWheel() {
  injectCss();
  wheel = document.createElement('div');
  wheel.id = 'emoteWheel';
  wheel.className = 'hidden';
  document.body.appendChild(wheel);
  wheel.addEventListener('click', (ev) => {
    const b = ev.target.closest('button');
    if (!b) return;
    if (b.classList.contains('emute')) { setMuted(!muteOthers); fillWheel(); return; }
    if (b.dataset.id) { sendEmote(b.dataset.id); closeWheel(); }
  });
  wheel.addEventListener('mouseover', (ev) => {
    const b = ev.target.closest('.ew');
    const c = wheel.querySelector('.ec');
    if (c) c.textContent = b ? b.dataset.name || 'Empty slot' : 'Emotes';
  });
  wheel.addEventListener('contextmenu', (ev) => ev.preventDefault());
}

function fillWheel() {
  const ids = DT.Cosmetics.equipped('emote');
  const R = 121;
  wheel.innerHTML = '<div class="ering"></div><div class="ec">Emotes</div>' + ids.map((id, i) => {
    const a = -Math.PI / 2 + (i * Math.PI * 2) / ids.length;
    const pos = `left:${(Math.cos(a) * R).toFixed(1)}px;top:${(Math.sin(a) * R).toFixed(1)}px`;
    const it = id && DT.Cosmetics.get('emote', id);
    if (!it) return `<button class="ew empty" style="${pos}" title="Get emotes in the store"><kbd>${i + 1}</kbd>Empty</button>`;
    return `<button class="ew" style="${pos}" data-id="${it.id}" data-name="${it.name}" title="${it.name} (${i + 1})"><kbd>${i + 1}</kbd>${stickerHTML(it.id)}</button>`;
  }).join('') + `<button class="emute${muteOthers ? ' on' : ''}" title="Hide emotes from other Tamers">${muteOthers ? 'Others muted' : 'Mute others'}</button>`;
}

export function openWheel() {
  const g = game();
  if (!g || !g.human || overlayUp()) return false;
  if (!wheel) buildWheel();
  fillWheel();
  const arena = document.getElementById('arena').getBoundingClientRect();
  wheel.style.left = `${Math.round(arena.left + arena.width / 2)}px`;
  wheel.style.top = `${Math.round(arena.top + Math.max(180, arena.height * 0.45))}px`;
  wheel.classList.remove('hidden');
  const btn = document.getElementById('emoteBtn');
  if (btn) btn.classList.add('on');
  return true;
}
export function closeWheel() {
  if (wheel) wheel.classList.add('hidden');
  const btn = document.getElementById('emoteBtn');
  if (btn) btn.classList.remove('on');
}
const wheelOpen = () => !!wheel && !wheel.classList.contains('hidden');
export function setMuted(on) {
  muteOthers = !!on;
  store.set(MUTE_KEY, muteOthers ? '1' : '0');
  if (muteOthers) { const g = game(); for (const slot of [...active.keys()]) if (!g || !g.human || slot !== g.human.id) clear(slot); }
}

window.addEventListener('keydown', (ev) => {
  if (ev.target && /input|textarea|select/i.test(ev.target.tagName)) return;
  if (ev.ctrlKey || ev.metaKey || ev.altKey) return;
  const k = ev.key.toLowerCase();
  if (wheelOpen()) {
    if (k === 'escape' || k === 't') { closeWheel(); ev.preventDefault(); return; }
    if (/^[1-6]$/.test(k)) {
      const id = DT.Cosmetics.equipped('emote')[Number(k) - 1];
      if (id) { sendEmote(id); closeWheel(); }
      ev.preventDefault();
    }
    return;
  }
  if (k === 't' && !ev.repeat && openWheel()) ev.preventDefault();
});
// a press anywhere outside the wheel closes it; the press still goes through, so dragging and right-click walking work
window.addEventListener('pointerdown', (ev) => {
  if (!wheelOpen() || wheel.contains(ev.target)) return;
  const btn = document.getElementById('emoteBtn');
  if (btn && btn.contains(ev.target)) return;
  closeWheel();
}, true);

function bindButton() {
  const btn = document.getElementById('emoteBtn');
  if (!btn || btn.__emote) return;
  btn.__emote = true;
  btn.addEventListener('click', () => (wheelOpen() ? closeWheel() : openWheel()));
}

// ---------- AI Tamers emote now and then (local only) ----------
let watched = null;
let lastResults = null;
let aliveIds = new Set();
function pickFor(player, moods) {
  const look = DT.Social.lookOf(player);
  const ids = ((look && look.emote) || []).filter(Boolean);
  const ok = ids.filter((id) => { const it = DT.Cosmetics.get('emote', id); return it && moods.includes((it.data || {}).mood); });
  return ok.length ? ok[Math.floor(Math.random() * ok.length)] : null;
}
function onResults(g) {
  const isAi = (q) => q && !q.isHuman && q !== g.human;
  const cands = [];
  for (const r of g.results || []) {
    const m = r.match;
    if (!m || m.pve) continue;
    if (isAi(m.a)) cands.push([m.a, r.aWon ? 'win' : r.bWon ? 'loss' : null]);
    if (!m.ghost && isAi(m.b)) cands.push([m.b, r.bWon ? 'win' : r.aWon ? 'loss' : null]);
  }
  const out = [];
  for (const q of g.players) if (aliveIds.has(q.id) && !q.alive && isAi(q)) out.push(q);
  aliveIds = new Set(g.players.filter((q) => q.alive).map((q) => q.id));
  // at most one AI emote per round: an eliminated Tamer sometimes, else a rare win or loss reaction
  let pick = null;
  for (const q of out) if (Math.random() < 0.4) { pick = [q, pickFor(q, ['bad', 'any'])]; break; }
  if (!pick) {
    for (const [q, res] of cands.sort(() => Math.random() - 0.5)) {
      if (res === 'win' && Math.random() < 0.05) { pick = [q, pickFor(q, ['good', 'any'])]; break; }
      if (res === 'loss' && Math.random() < 0.035) { pick = [q, pickFor(q, ['bad', 'any'])]; break; }
    }
  }
  if (pick && pick[1]) {
    const [q, id] = pick;
    setTimeout(() => { if (game() === g) DT.Social.emit('emote', q.id, id); }, 500 + Math.random() * 1500);
  }
}
function watchGame(g) {
  if (g !== watched) {
    watched = g;
    lastResults = g ? g.results : null;
    aliveIds = new Set(g ? g.players.filter((q) => q.alive).map((q) => q.id) : []);
    for (const slot of [...active.keys()]) clear(slot);
    sent.clear();
    poses.clear();
    for (const slot of [...pops.keys()]) clearPop(slot);
    return;
  }
  if (g && g.results !== lastResults) {
    lastResults = g.results;
    if (g.results && g.results.length) onResults(g);
  }
}

// ---------- loop ----------
function frame() {
  const now = performance.now();
  const g = game();
  hookRenderer(rend());
  bindButton();
  watchGame(g);
  if (pops.size) updatePops(now);
  if (wheelOpen() && (!g || overlayUp())) closeWheel();
  for (const [slot, a] of [...active]) {
    if (now > a.until) clear(slot);
    else place(slot, a, now);
  }
  requestAnimationFrame(frame);
}
requestAnimationFrame(frame);

DT.Emotes = { pops: () => [...pops].map(([slot, p]) => ({ slot, act: p.act, ready: !p.pending, inScene: !!(p.holder && p.holder.parent) })), open: openWheel, close: closeWheel, send: sendEmote, sticker: stickerHTML, get muted() { return muteOthers; }, setMuted, isOpen: wheelOpen };
