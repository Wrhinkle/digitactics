// Item icons, shared by the 3D item bench (textures) and the DOM (image URLs).
// The pictures are the illustrated portraits in js/itemart.js (pre-rendered to models/items/). The flat canvas
// glyphs below are only drawn when a portrait cannot load.
import { ItemArt } from './itemart.js';
const DT = window.DT;
const S = 128;

function glyph(g, kind) {
  g.save();
  g.translate(64, 64);
  g.fillStyle = '#ffffff';
  g.strokeStyle = 'rgba(16,19,26,0.9)';
  g.lineWidth = 7;
  g.lineJoin = 'round';
  g.lineCap = 'round';
  const both = () => { g.stroke(); g.fill(); };
  g.beginPath();
  switch (kind) {
    case 'sword':
      g.rotate(-Math.PI / 4);
      g.moveTo(-7, 20); g.lineTo(-7, -30); g.lineTo(0, -42); g.lineTo(7, -30); g.lineTo(7, 20); g.closePath(); both();
      g.beginPath(); g.rect(-20, 18, 40, 9); both();
      g.beginPath(); g.rect(-5, 27, 10, 16); both();
      break;
    case 'bow':
      g.lineWidth = 16; g.strokeStyle = 'rgba(16,19,26,0.9)';
      g.arc(-12, 0, 36, -1.2, 1.2); g.stroke();
      g.lineWidth = 8; g.strokeStyle = '#ffffff'; g.stroke();
      g.beginPath(); g.lineWidth = 4; g.moveTo(1, -33); g.lineTo(1, 33); g.stroke();
      g.beginPath(); g.moveTo(-30, 0); g.lineTo(34, 0); g.lineWidth = 9; g.strokeStyle = 'rgba(16,19,26,0.9)'; g.stroke();
      g.lineWidth = 4; g.strokeStyle = '#ffffff'; g.stroke();
      g.beginPath(); g.moveTo(42, 0); g.lineTo(28, -9); g.lineTo(28, 9); g.closePath(); g.strokeStyle = 'rgba(16,19,26,0.9)'; g.lineWidth = 5; both();
      break;
    case 'rod':
      g.rotate(Math.PI / 6);
      g.rect(-5, -14, 10, 56); both();
      g.beginPath(); g.arc(0, -27, 16, 0, Math.PI * 2); both();
      break;
    case 'tear':
      g.moveTo(0, -42); g.bezierCurveTo(20, -14, 30, 2, 30, 14); g.arc(0, 14, 30, 0, Math.PI); g.bezierCurveTo(-30, 2, -20, -14, 0, -42); both();
      break;
    case 'vest':
      g.moveTo(0, -38); g.lineTo(32, -26); g.lineTo(30, 6); g.quadraticCurveTo(24, 30, 0, 42); g.quadraticCurveTo(-24, 30, -30, 6); g.lineTo(-32, -26); g.closePath(); both();
      break;
    case 'cloak':
      g.moveTo(-14, -36); g.quadraticCurveTo(0, -46, 14, -36); g.lineTo(36, 36); g.quadraticCurveTo(0, 26, -36, 36); g.closePath(); both();
      break;
    case 'belt':
      g.moveTo(0, 38); g.bezierCurveTo(-46, 6, -36, -38, 0, -18); g.bezierCurveTo(36, -38, 46, 6, 0, 38); both();
      break;
    case 'glove':
      for (const [x, y] of [[0, -18], [18, 0], [0, 18], [-18, 0]]) { g.moveTo(x + 17, y); g.arc(x, y, 17, 0, Math.PI * 2); }
      g.stroke(); g.fill();
      g.beginPath(); g.moveTo(8, 24); g.quadraticCurveTo(16, 36, 28, 40); g.lineWidth = 6; g.strokeStyle = '#ffffff'; g.stroke();
      break;
    case 'egg':
      g.ellipse(0, 4, 28, 37, 0, 0, Math.PI * 2); both();
      g.fillStyle = 'rgba(16,19,26,0.45)';
      for (const [x, y, r] of [[-10, -10, 7], [11, 6, 8], [-6, 22, 5]]) { g.beginPath(); g.arc(x, y, r, 0, Math.PI * 2); g.fill(); }
      break;
  }
  g.restore();
}

function label(g, text, size) {
  g.font = `900 ${size}px system-ui, "Segoe UI", sans-serif`;
  g.textAlign = 'center';
  g.textBaseline = 'middle';
  g.lineWidth = 10;
  g.lineJoin = 'round';
  g.strokeStyle = 'rgba(16,19,26,0.92)';
  g.strokeText(text, 64, 68);
  g.fillStyle = '#ffffff';
  g.fillText(text, 64, 68);
}

function legacyCanvas(id, cv) {
  const it = DT.ITEMS[id];
  const g = cv.getContext('2d');
  const shape = () => {
    g.beginPath();
    if (it.component) g.arc(64, 64, 58, 0, Math.PI * 2);
    else if (it.emblem) { g.moveTo(64, 6); g.lineTo(118, 24); g.lineTo(112, 78); g.quadraticCurveTo(98, 108, 64, 122); g.quadraticCurveTo(30, 108, 16, 78); g.lineTo(10, 24); g.closePath(); }
    else g.roundRect(8, 8, 112, 112, 20);
  };
  g.save();
  shape();
  g.clip();
  g.fillStyle = it.color;
  g.fillRect(0, 0, S, S);
  if (!it.component) {
    g.fillStyle = it.color2;
    g.beginPath(); g.moveTo(S, 0); g.lineTo(S, S); g.lineTo(0, S); g.closePath(); g.fill();
    if (it.consumable) { g.fillStyle = 'rgba(0,0,0,0.25)'; for (let k = -S; k < S; k += 24) { g.beginPath(); g.moveTo(k, S); g.lineTo(k + 12, S); g.lineTo(k + 140, 0); g.lineTo(k + 128, 0); g.fill(); } }
  }
  const sh = g.createLinearGradient(0, 0, 0, S);
  sh.addColorStop(0, 'rgba(255,255,255,0.35)'); sh.addColorStop(0.5, 'rgba(255,255,255,0)'); sh.addColorStop(1, 'rgba(0,0,0,0.35)');
  g.fillStyle = sh;
  g.fillRect(0, 0, S, S);
  g.restore();
  g.lineWidth = it.component ? 7 : 10;
  g.strokeStyle = it.component ? 'rgba(255,255,255,0.85)' : it.consumable ? '#d8dde6' : '#ffd84a';
  shape();
  g.stroke();
  if (it.component) glyph(g, id);
  else if (id === 'golden_egg') glyph(g, 'egg');
  else if (it.emblem) label(g, DT.TRAITS[it.emblem].name.replace(/[^A-Z]/g, '').slice(0, 2) || it.short, 46);
  else label(g, it.short, it.short.length > 2 ? 40 : 50);
  return cv;
}

// Canvas with the item's portrait. It starts empty and is drawn once the picture loads; cv.ready resolves then
// (callers that upload it as a texture flag it for update).
const canvases = new Map();
export function itemCanvas(id) {
  if (canvases.has(id)) return canvases.get(id);
  const cv = document.createElement('canvas');
  cv.width = cv.height = S;
  cv.ready = ItemArt.paint(cv, id).then((ok) => { if (!ok && DT.ITEMS[id]) legacyCanvas(id, cv); return cv; });
  canvases.set(id, cv);
  return cv;
}

const urls = new Map();
export function itemIcon(id) {
  if (!DT.ITEMS[id]) return '';
  const u = ItemArt.url(id, 64);
  if (u) return u;
  if (!urls.has(id)) { const cv = document.createElement('canvas'); cv.width = cv.height = S; urls.set(id, legacyCanvas(id, cv).toDataURL('image/png')); }
  return urls.get(id);
}
DT.itemIcon = itemIcon;
