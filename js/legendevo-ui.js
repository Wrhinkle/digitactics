// Digivolve panel for Little Legends (data and rules: js/legendevo.js). Opened from the Locker, the title card and
// the egg hatch screen by any element with data-evo-open="<legend id>".
// DT.LegendEvo.open(id) / close() / isOpen()
// DT.LegendEvo.bannerHTML(id)   summary row at the top of the Locker's Little Legends tab
// DT.LegendEvo.cardHTML(id)     level / form line on a Locker card
import * as THREE from 'three';

const DT = window.DT;
const C = DT.Cosmetics;
const W = DT.Wallet;
const E = DT.LegendEvo;

const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const bit = '<i class="bit"></i>';
const sfx = (n) => { try { if (DT.Sfx) DT.Sfx.play(n); } catch (e) { /* audio blocked */ } };
const CARE_ICON = { bond: '♥', battle: '⚔', food: '🍖', wild: '?' };
const TIER_NAME = ['Baby', 'Rookie', 'Champion', 'Ultimate'];

// ---------- styles ----------
const css = `
#dtEvo { --gold2: #ffd36a; position: fixed; inset: 0; z-index: 60; display: flex; align-items: center; justify-content: center;
  background: radial-gradient(ellipse at 50% 30%, rgba(40, 80, 140, 0.55), rgba(4, 6, 10, 0.94) 70%); }
#dtEvo.hidden { display: none; }
.dte-win { position: relative; display: flex; flex-direction: column; width: min(1180px, calc(100vw - 20px)); height: min(740px, calc(100vh - 20px));
  background: var(--panel); border: 1px solid var(--line2); border-radius: 14px; box-shadow: 0 20px 60px rgba(0,0,0,0.7); overflow: hidden; }
.dte-head { display: flex; align-items: center; gap: 12px; padding: 10px 14px; background: linear-gradient(#182033, var(--panel)); border-bottom: 1px solid var(--line); }
.dte-head h2 { margin: 0; font-size: 19px; letter-spacing: 0.5px; white-space: nowrap; }
.dte-pick { flex: 1; display: flex; gap: 5px; overflow-x: auto; min-width: 0; padding: 2px; scrollbar-width: thin; }
.dte-chip { position: relative; flex: none; width: 46px; height: 46px; padding: 0; border-radius: 8px; overflow: hidden; }
.dte-chip .pt { width: 100%; height: 100%; border-radius: 0; background: transparent; border: 0; }
.dte-chip.on { border-color: var(--gold); box-shadow: 0 0 0 2px var(--gold); }
.dte-chip i { position: absolute; right: 2px; bottom: 1px; font-style: normal; font-size: 10px; font-weight: 800; color: #fff; text-shadow: 0 1px 2px #000; }
.dte-chip.ready::after { content: ''; position: absolute; left: 3px; top: 3px; width: 8px; height: 8px; border-radius: 50%; background: var(--gold2); box-shadow: 0 0 6px var(--gold2); }
.dte-wallet { font-size: 17px; font-weight: 800; padding: 4px 10px; border-radius: 8px; background: var(--bg); border: 1px solid var(--line2); white-space: nowrap; }
.dte-body { flex: 1; min-height: 0; display: grid; grid-template-columns: 330px 1fr; gap: 14px; padding: 12px 14px; }
.dte-left { display: flex; flex-direction: column; gap: 8px; min-height: 0; overflow-y: auto; }
.dte-right { display: flex; flex-direction: column; gap: 10px; min-width: 0; min-height: 0; }
.dte-stage { position: relative; height: 230px; flex: none; border-radius: 12px; overflow: hidden; border: 1px solid var(--line2);
  background: radial-gradient(circle at 50% 62%, #2a3b5c, #0c1220 75%); }
.dte-stage canvas { position: absolute; inset: 0; width: 100%; height: 100%; }
.dte-rays { position: absolute; left: 50%; top: 55%; width: 520px; height: 520px; margin: -260px 0 0 -260px; opacity: 0; pointer-events: none;
  background: repeating-conic-gradient(rgba(255,255,255,0.22) 0 6deg, transparent 6deg 18deg); border-radius: 50%;
  -webkit-mask: radial-gradient(circle, #000 10%, transparent 65%); mask: radial-gradient(circle, #000 10%, transparent 65%); transition: opacity 0.4s; }
.dte-stage.evolving .dte-rays { opacity: 1; animation: dteSpin 2.4s linear infinite; }
.dte-flash { position: absolute; inset: 0; background: #fff; opacity: 0; pointer-events: none; }
.dte-flash.on { animation: dteFlash 0.9s ease-out forwards; }
.dte-fname { position: absolute; left: 10px; top: 8px; right: 10px; display: flex; justify-content: space-between; align-items: baseline; pointer-events: none; }
.dte-fname b { font-size: 20px; text-shadow: 0 2px 6px #000; }
.dte-fname span { font-size: 12px; color: var(--muted); text-transform: uppercase; letter-spacing: 1px; }
.dte-said { position: absolute; left: 0; right: 0; bottom: 10px; text-align: center; font-weight: 800; font-size: 15px; color: #fff; text-shadow: 0 2px 6px #000, 0 0 16px var(--gold2);
  opacity: 0; transform: translateY(6px); transition: opacity 0.3s, transform 0.3s; pointer-events: none; padding: 0 8px; }
.dte-said.on { opacity: 1; transform: none; }
@keyframes dteSpin { to { transform: rotate(360deg); } }
@keyframes dteFlash { 0% { opacity: 0; } 18% { opacity: 1; } 45% { opacity: 1; } 100% { opacity: 0; } }
.dte-box { padding: 9px 11px; border-radius: 10px; background: var(--panel2); border: 1px solid var(--line2); }
.dte-row { display: flex; align-items: center; gap: 8px; }
.dte-lv { font-size: 20px; font-weight: 800; white-space: nowrap; }
.dte-lv small { font-size: 12px; color: var(--muted); font-weight: 600; }
.dte-bar { position: relative; flex: 1; height: 9px; border-radius: 5px; background: var(--bg); overflow: hidden; border: 1px solid var(--line); }
.dte-bar i { position: absolute; left: 0; top: 0; bottom: 0; border-radius: 5px; background: linear-gradient(90deg, #2f9bff, #8ff0ff); }
.dte-bar.care i { background: linear-gradient(90deg, #ff9a3c, #ffd36a); }
.dte-sub { font-size: 12px; color: var(--muted); margin-top: 4px; display: flex; justify-content: space-between; gap: 8px; }
.dte-sub b { color: var(--text); }
.dte-btn-sm { padding: 3px 9px; font-size: 12px; white-space: nowrap; }
.dte-care { display: grid; grid-template-columns: 22px 54px 1fr 40px auto; align-items: center; gap: 6px; font-size: 12px; padding: 2px 0; }
.dte-care .dte-ic { text-align: center; font-size: 14px; }
.dte-care .n { text-align: right; color: var(--muted); font-variant-numeric: tabular-nums; }
.dte-care .nm { font-weight: 700; }
.dte-acts { display: flex; gap: 8px; }
.dte-acts button { flex: 1; }
.dte-go { font-size: 16px; font-weight: 800; padding: 9px 10px; }
.dte-go:not([disabled]) { background: linear-gradient(135deg, #ffe7a8, #ffb347); color: #1d1406; border-color: #ffd36a; box-shadow: 0 0 14px rgba(255, 190, 80, 0.5); }
.dte-note { font-size: 11px; color: var(--muted); line-height: 1.35; }
.dte-odds h3, .dte-treebox h3 { margin: 0 0 6px; font-size: 13px; text-transform: uppercase; letter-spacing: 1px; display: flex; gap: 10px; align-items: baseline; flex-wrap: wrap; }
.dte-odds h3 .muted, .dte-treebox h3 .muted { text-transform: none; letter-spacing: 0; font-weight: 400; font-size: 12px; }
.dte-orow { display: grid; grid-template-columns: 34px 120px 74px 1fr 46px; align-items: center; gap: 8px; padding: 3px 0; font-size: 13px; }
.dte-orow .pt { width: 32px; height: 32px; }
.dte-orow .tag { font-size: 11px; padding: 1px 6px; border-radius: 9px; background: var(--bg); border: 1px solid var(--line2); color: var(--muted); white-space: nowrap; text-align: center; }
.dte-orow .tag.bond { color: #ff8fb0; border-color: #ff8fb055; } .dte-orow .tag.battle { color: #ff9a5a; border-color: #ff9a5a55; }
.dte-orow .tag.food { color: #ffd36a; border-color: #ffd36a55; } .dte-orow .tag.wild { color: #9fe07a; border-color: #9fe07a55; }
.dte-orow .pc { text-align: right; font-weight: 800; font-size: 15px; font-variant-numeric: tabular-nums; }
.dte-obar { height: 12px; border-radius: 6px; background: var(--bg); overflow: hidden; border: 1px solid var(--line); }
.dte-obar i { display: block; height: 100%; background: linear-gradient(90deg, #4fb3ff, #b8fbff); }
.dte-orow.unk .nm { color: var(--muted); }
.dte-treebox { flex: 1; min-height: 0; display: flex; flex-direction: column; }
.dte-tree { position: relative; flex: 1; min-height: 0; display: flex; flex-direction: column; justify-content: space-around; gap: 4px;
  border-radius: 10px; background: linear-gradient(180deg, rgba(79, 179, 255, 0.05), rgba(255, 207, 74, 0.04)); border: 1px solid var(--line); padding: 6px 8px 6px 62px; overflow: hidden; }
.dte-tree svg { position: absolute; inset: 0; width: 100%; height: 100%; pointer-events: none; }
.dte-tier { position: relative; display: flex; flex-wrap: wrap; justify-content: center; gap: 2px 4px; }
.dte-tier > span { position: absolute; left: -56px; top: 50%; transform: translateY(-50%); width: 52px; font-size: 10px; line-height: 1.1; text-transform: uppercase; letter-spacing: 0.5px; color: var(--muted); }
.dte-node { position: relative; z-index: 1; width: var(--nw, 66px); display: flex; flex-direction: column; align-items: center; gap: 2px; }
.dte-tree .dte-node .pt { width: var(--np, 54px); height: var(--np, 54px); }
.dte-tree.small .dte-node em { display: none; }
.dte-node .pt { width: 54px; height: 54px; border-radius: 50%; border: 2px solid var(--line2); background: radial-gradient(circle at 50% 40%, #33415f, #121826); }
.dte-node .pt img { object-fit: contain !important; }
.dte-node em { font-style: normal; font-size: 11px; line-height: 1.1; text-align: center; max-width: var(--nw, 66px); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dte-node.unk .pt img { filter: brightness(0) drop-shadow(0 0 1px rgba(120, 170, 255, 0.9)); }
.dte-node.unk .pt b { visibility: hidden; }
.dte-node.unk em { color: var(--muted); }
.dte-node.path .pt { border-color: #8fd0ff; }
.dte-node.next .pt { border-color: var(--gold2); border-style: dashed; }
.dte-node.cur .pt { border-color: var(--gold); box-shadow: 0 0 0 3px rgba(255, 207, 74, 0.35), 0 0 16px rgba(255, 207, 74, 0.6); }
.dte-node.cur em { color: var(--gold); font-weight: 800; }
.dte-node .pc { position: absolute; top: -4px; right: 2px; font-size: 10px; font-weight: 800; padding: 0 4px; border-radius: 7px; background: #1d1406; color: var(--gold2); border: 1px solid var(--gold2); }
/* Locker */
.dte-bsec { display: flex; align-items: center; gap: 14px; padding: 10px 12px; border-radius: 12px; background: linear-gradient(135deg, rgba(255, 207, 74, 0.09), rgba(79, 179, 255, 0.06));
  border: 1px solid rgba(255, 207, 74, 0.3); }
.dte-bsec .pt { width: 64px; height: 64px; border-radius: 10px; }
.dte-bsec .pt img { object-fit: contain !important; }
.dte-binfo { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 4px; }
.dte-binfo .t { font-weight: 800; font-size: 16px; }
.dte-binfo .t span { color: var(--muted); font-weight: 400; font-size: 13px; }
.dte-binfo .dte-bar { flex: none; width: min(320px, 100%); }
.dte-ready { color: var(--gold2); font-weight: 800; }
.dte-cardrow { display: flex; align-items: center; gap: 6px; font-size: 12px; margin-top: 2px; }
.dte-cardrow b { color: var(--accent); }
.dte-cardrow span { color: var(--muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.dte-cardbtn { width: 100%; margin-top: 4px; padding: 3px 8px; font-size: 12px; }
.dte-cardbtn.ready { border-color: var(--gold2); color: var(--gold2); box-shadow: 0 0 8px rgba(255, 207, 74, 0.35); }
@media (max-height: 800px) {
  .dte-stage { height: 196px; }
  .dte-orow { padding: 1px 0; }
}
`;
const tag = document.createElement('style');
tag.id = 'dt-legendevo-style';
tag.textContent = css;
document.head.appendChild(tag);

// ---------- markup helpers ----------
function formPic(formId, legendId, cls = '') {
  const f = E.formInfo(formId);
  if (!f) return '';
  return DT.Legends.html(f.legend ? f.legend : `${legendId}@${formId}`, cls);
}
const shown = (i, formId) => i.dex.includes(formId);

E.cardHTML = (id) => {
  const i = E.info(id);
  if (!i || !i.owned) return '';
  return `<div class="dte-cardrow"><b>Lv ${i.level}</b><span>${i.form !== id ? `Now ${esc(i.formInfo.name)} (${esc(i.formInfo.level)})` : i.next ? `Digivolves at Lv ${i.next}` : ''}</span>${i.canDigivolve ? '<span class="dte-ready">Ready!</span>' : ''}</div>
    <button class="dte-cardbtn${i.canDigivolve ? ' ready' : ''}" data-evo-open="${esc(id)}">Digivolve</button>`;
};

E.bannerHTML = (id) => {
  const i = E.info(id);
  if (!i) return '';
  const next = i.canDigivolve ? '<span class="dte-ready">Ready to digivolve!</span>'
    : i.next ? `Digivolves at level ${i.next}` : 'Final form reached';
  return `<section class="dte-bsec">${DT.Legends.html(i.key)}
    <div class="dte-binfo"><div class="t">${esc(i.formInfo.name)} <span>${i.form !== id ? `${esc(i.base.name)}'s ` : ''}${esc(i.formInfo.level)} · equipped</span></div>
      <div class="dte-row"><span class="dte-lv" style="font-size:14px">Lv ${i.level}</span><div class="dte-bar"><i style="width:${i.need ? (100 * i.data) / i.need : 100}%"></i></div>
        <span class="muted" style="font-size:12px">${i.need ? `${i.data}/${i.need} Digi-Data` : 'Max level'}</span></div>
      <div class="muted" style="font-size:12px">${next} · Dex ${i.dex.length}/${E.reach(id).length}</div></div>
    <div style="display:flex;flex-direction:column;gap:6px"><button class="primary" data-evo-open="${esc(id)}">Digivolve</button>${DT.FarmUI ? `<button data-farm-open="${esc(id)}" title="Care for your legends and train them">Digi-Farm</button>` : ''}</div></section>`;
};

// ---------- panel ----------
let root = null;
let cur = null; // legend id shown
let busy = false; // evolution animation running
const stage = { r: null, scene: null, cam: null, view: null, key: null, raf: 0, last: 0, white: null, extra: null };

function build() {
  if (root) return root;
  root = document.createElement('div');
  root.id = 'dtEvo';
  root.className = 'hidden';
  root.setAttribute('role', 'dialog');
  root.setAttribute('aria-label', 'Digivolve');
  root.innerHTML = `<div class="dte-win">
    <header class="dte-head"><h2>Digivolve</h2><div class="dte-pick"></div><div class="dte-wallet">${bit} <b class="dte-bits">0</b></div><button class="dte-close" title="Esc">Close</button></header>
    <div class="dte-body">
      <div class="dte-left">
        <div class="dte-stage"><div class="dte-rays"></div><canvas></canvas><div class="dte-flash"></div><div class="dte-fname"></div><div class="dte-said"></div></div>
        <div class="dte-stats"></div>
      </div>
      <div class="dte-right"><div class="dte-box dte-odds"></div><div class="dte-treebox"></div></div>
    </div></div>`;
  document.body.appendChild(root);
  root.addEventListener('click', onClick);
  return root;
}

function ownedLegends() { return C.list('legend').filter((x) => C.owns('legend', x.id)); }

function render() {
  if (!root || root.classList.contains('hidden')) return;
  const i = E.info(cur);
  if (!i) return close();
  root.querySelector('.dte-bits').textContent = W.bits;
  const eq = C.equipped('legend');
  // mid-animation the new form stays hidden until the flash
  if (busy) { for (const b of root.querySelectorAll('.dte-stats button, .dte-pick button')) b.disabled = true; return; }
  root.querySelector('.dte-pick').innerHTML = ownedLegends().map((x) => {
    const xi = E.info(x.id);
    return `<button class="dte-chip${x.id === cur ? ' on' : ''}${xi.canDigivolve ? ' ready' : ''}" data-evo-pick="${esc(x.id)}" title="${esc(x.name)}${x.id === eq ? ' (equipped)' : ''}: ${esc(xi.formInfo.name)}, level ${xi.level}">${DT.Legends.html(xi.key)}<i>${xi.level}</i></button>`;
  }).join('');
  root.querySelector('.dte-fname').innerHTML = `<b>${esc(i.formInfo.name)}</b><span>${esc(i.formInfo.level)}</span>`;
  setStageView(i.key);
  root.querySelector('.dte-stats').innerHTML = statsHTML(i, eq);
  root.querySelector('.dte-odds').innerHTML = oddsHTML(i);
  root.querySelector('.dte-treebox').innerHTML = treeHTML(i);
  fitTree();
  requestAnimationFrame(drawEdges);
}

function statsHTML(i, eq) {
  const care = ['bond', 'battle', 'food'].map((c) => {
    const n = i.care[c] || 0;
    const btn = c === 'food' ? (n >= i.careCap ? '<button class="dte-btn-sm" disabled>Full</button>'
      : `<button class="dte-btn-sm" data-evo-feed="1" ${!i.owned || W.bits < i.feedBits ? 'disabled' : ''}>Feed · ${bit}${i.feedBits}</button>`) : '<span></span>';
    return `<div class="dte-care" title="${esc(E.CARE[c].how)}"><span class="dte-ic">${CARE_ICON[c]}</span><span class="nm">${E.CARE[c].name}</span>
      <div class="dte-bar care"><i style="width:${(100 * Math.min(n, i.careCap)) / i.careCap}%"></i></div><span class="n">${Math.min(n, 99)}/${i.careCap}</span>${btn}</div>`;
  }).join('');
  const lvBtn = i.level >= i.maxLevel ? '<button class="dte-btn-sm" disabled>Max level</button>'
    : `<button class="dte-btn-sm" data-evo-level="1" ${!i.owned || W.bits < i.levelBits ? 'disabled' : ''} title="Buy the rest of this level">Level up · ${bit}${i.levelBits}</button>`;
  const nextTxt = i.canDigivolve ? '<span class="dte-ready">Ready to digivolve!</span>'
    : i.next ? `Digivolves at <b>level ${i.next}</b>` : '<b>Final form.</b> Revert to explore other branches.';
  return `<div class="dte-box">
      <div class="dte-row"><span class="dte-lv">Lv ${i.level} <small>/ ${i.maxLevel}</small></span><div class="dte-bar"><i style="width:${i.need ? (100 * i.data) / i.need : 100}%"></i></div>${lvBtn}</div>
      <div class="dte-sub"><span>${i.need ? `<b>${i.data}</b>/${i.need} Digi-Data` : 'Max level'}</span><span>${nextTxt}</span></div>
    </div>
    <div class="dte-box">${care}
      <div class="dte-note" style="margin-top:4px">Care raises the odds of matching branches. Bond: games played with it equipped. Battle: top 4 finishes. Food: feeding.</div>${i.careBonus && i.careBonus.notes && i.careBonus.notes.length ? `<div class="dte-note" style="margin-top:3px;color:#9fe07a" data-farm-note>${esc(i.careBonus.notes.join(' · '))}. Wild raises the Wild branch.</div>` : ''}</div>
    <div class="dte-acts">
      <button class="dte-go" data-evo-go="1" ${i.canDigivolve && !busy ? '' : 'disabled'}>Digivolve</button>
      <button data-evo-revert="1" ${i.canRevert && !busy && W.bits >= i.revertBits ? '' : 'disabled'} title="Back to the previous form. Keeps level, Digi-Data and care.">Revert · ${bit}${i.revertBits}</button>
    </div>
    <div class="dte-row" style="justify-content:space-between">
      <span class="dte-note">Duplicates from eggs add Digi-Data. Each roll is fixed for its level; a revert rolls again.</span>
      ${i.owned && eq !== i.id ? `<button class="dte-btn-sm" data-evo-equip="1">Equip</button>` : '<span class="dte-note" style="white-space:nowrap">Equipped</span>'}
    </div>`;
}

function oddsHTML(i) {
  if (!i.odds.length) return `<h3>Next digivolution <span class="muted">none: ${esc(i.formInfo.name)} is a final form here</span></h3>
    <div class="dte-note">Revert to go back a form, keep your level, and roll a different branch.</div>`;
  const lvName = (E.formInfo(i.odds[0].form) || {}).level || '';
  return `<h3>Next digivolution <span class="muted">${esc(lvName)} at level ${i.next}${i.canDigivolve ? ', ready now' : ` (you are level ${i.level})`}</span></h3>
    ${i.odds.map((o) => {
      const known = shown(i, o.form);
      const f = E.formInfo(o.form);
      return `<div class="dte-orow${known ? '' : ' unk'}" data-odds="${esc(o.form)}"><span class="dte-node${known ? '' : ' unk'}" style="width:auto">${formPic(o.form, i.id)}</span>
        <span class="nm"><b>${esc(f.name)}</b></span><span class="tag ${o.care}" title="${esc(E.CARE[o.care].how)}">${CARE_ICON[o.care]} ${E.CARE[o.care].name}</span>
        <div class="dte-obar"><i style="width:${o.pct}%"></i></div><span class="pc">${o.pct}%</span></div>`;
    }).join('')}`;
}

function treeHTML(i) {
  const t = E.tree(i.id);
  const next = new Set(i.odds.map((o) => o.form));
  const path = new Set(i.path);
  const total = t.tiers.reduce((n, x) => n + x.length, 0);
  const odds = Object.fromEntries(i.odds.map((o) => [o.form, o.pct]));
  return `<h3>${esc(i.base.name)}'s tree <span class="muted">Dex ${i.dex.length}/${total} discovered. Dark forms are not discovered yet.</span></h3>
    <div class="dte-tree"><svg></svg>${t.tiers.map((row, ti) => `<div class="dte-tier"><span>${esc(t.levels[ti] || TIER_NAME[ti] || '')}</span>${row.map((f) => {
      const known = shown(i, f);
      const fi = E.formInfo(f);
      const cls = [f === i.form ? 'cur' : '', path.has(f) ? 'path' : '', next.has(f) ? 'next' : '', known ? '' : 'unk'].filter(Boolean).join(' ');
      return `<div class="dte-node ${cls}" data-node="${esc(f)}" title="${known ? `${esc(fi.name)} (${esc(fi.level)})` : `Not discovered yet (${esc(fi.level)})`}">${formPic(f, i.id)}<em>${known ? esc(fi.name) : '???'}</em>${next.has(f) ? `<span class="pc">${odds[f]}%</span>` : ''}</div>`;
    }).join('')}</div>`).join('')}</div>`;
}

// Shrinks the nodes until every row (wrapped to the box width) fits the tree box; names hide below 46px.
function fitTree() {
  const box = root && root.querySelector('.dte-tree');
  if (!box || !box.clientWidth) return;
  const rows = [...box.querySelectorAll('.dte-tier')].map((r) => r.querySelectorAll('.dte-node').length);
  const W = box.clientWidth - 70, H = box.clientHeight - 14;
  let nw = 66;
  for (; nw > 30; nw -= 2) {
    const np = Math.round(nw * 0.82), label = nw >= 46 ? 15 : 0;
    const per = Math.max(1, Math.floor((W + 4) / (nw + 4)));
    const lines = rows.reduce((n, c) => n + Math.ceil(c / per), 0);
    if (lines * (np + 6 + label) + rows.length * 4 <= H) break;
  }
  box.style.setProperty('--nw', `${nw}px`);
  box.style.setProperty('--np', `${Math.round(nw * 0.82)}px`);
  box.classList.toggle('small', nw < 46);
}

function drawEdges() {
  const box = root && root.querySelector('.dte-tree');
  if (!box) return;
  const svg = box.querySelector('svg');
  const b = box.getBoundingClientRect();
  const i = E.info(cur);
  const t = E.tree(cur);
  const walked = new Set([...i.path, i.form]);
  const pos = {};
  for (const n of box.querySelectorAll('[data-node]')) {
    const r = n.querySelector('.pt').getBoundingClientRect();
    pos[n.dataset.node] = { x: r.left + r.width / 2 - b.left, top: r.top - b.top, bot: r.bottom - b.top };
  }
  const chain = [...i.path, i.form];
  const onPath = (a, c) => chain.some((x, k) => x === a && chain[k + 1] === c);
  svg.setAttribute('viewBox', `0 0 ${b.width} ${b.height}`);
  svg.innerHTML = t.edges.map(([a, c]) => {
    const p = pos[a], q = pos[c];
    if (!p || !q) return '';
    const hot = onPath(a, c), nxt = a === i.form;
    const my = (p.bot + q.top) / 2;
    const col = hot ? '#8fd0ff' : nxt ? '#ffd36a' : walked.has(a) ? 'rgba(160,190,240,0.4)' : 'rgba(140,160,200,0.22)';
    return `<path d="M${p.x},${p.bot} C${p.x},${my} ${q.x},${my} ${q.x},${q.top}" fill="none" stroke="${col}" stroke-width="${hot || nxt ? 2.5 : 1.5}"${nxt && !hot ? ' stroke-dasharray="5 4"' : ''}/>`;
  }).join('');
}

// ---------- 3D stage ----------
function ensureStage() {
  if (stage.r) return;
  const canvas = root.querySelector('.dte-stage canvas');
  const r = new THREE.WebGLRenderer({ canvas, alpha: true, antialias: true });
  r.setPixelRatio(Math.min(2, window.devicePixelRatio || 1));
  r.outputColorSpace = THREE.SRGBColorSpace;
  r.setClearColor(0x000000, 0);
  const scene = new THREE.Scene();
  scene.add(new THREE.HemisphereLight('#ffffff', '#3a4660', 2.2));
  const key = new THREE.DirectionalLight('#fff6e8', 2.4);
  key.position.set(1.5, 3, 4);
  scene.add(key);
  const rim = new THREE.DirectionalLight('#9fd0ff', 1.2);
  rim.position.set(-3, 2, -2);
  scene.add(rim);
  const floor = new THREE.Mesh(new THREE.CircleGeometry(0.75, 40), new THREE.MeshBasicMaterial({ color: '#000000', transparent: true, opacity: 0.3 }));
  floor.rotation.x = -Math.PI / 2;
  scene.add(floor);
  stage.floor = floor;
  const cam = new THREE.PerspectiveCamera(30, 1, 0.01, 50);
  cam.position.set(0, 1.0, 3.6);
  cam.lookAt(0, 0.6, 0);
  stage.r = r; stage.scene = scene; stage.cam = cam;
  stage.white = new THREE.MeshBasicMaterial({ color: '#ffffff' });
}

function makeView(key) {
  const v = DT.Legends.createView(key, { ring: false, yaw: 0.35, height: 1 });
  stage.scene.add(v.group);
  return v;
}
function setStageView(key) {
  ensureStage();
  if (stage.key === key && stage.view) return;
  if (stage.view) stage.view.dispose();
  stage.view = makeView(key);
  stage.key = key;
}

function loop(now) {
  stage.raf = requestAnimationFrame(loop);
  const dt = Math.min(0.05, (now - (stage.last || now)) / 1000);
  stage.last = now;
  const c = stage.r.domElement;
  const w = c.clientWidth, h = c.clientHeight;
  if (!w || !h) return;
  if (c.width !== Math.round(w * stage.r.getPixelRatio()) || c.height !== Math.round(h * stage.r.getPixelRatio())) {
    stage.r.setSize(w, h, false);
    stage.cam.aspect = w / h;
    stage.cam.updateProjectionMatrix();
  }
  if (stage.anim) stage.anim(dt);
  if (stage.view) stage.view.update(dt);
  if (stage.extra) stage.extra.update(dt);
  stage.r.render(stage.scene, stage.cam);
}

// White silhouette, morphing between the two forms faster and faster, a flash, then the new form in color.
function evolve(id) {
  const i = E.info(id);
  if (!i || !i.canDigivolve || busy) return;
  const res = E.digivolve(id);
  if (!res) return;
  busy = true;
  const st = root.querySelector('.dte-stage');
  const said = root.querySelector('.dte-said');
  const flash = root.querySelector('.dte-flash');
  const fromName = E.formInfo(res.from).name;
  const to = E.formInfo(res.to);
  render();
  said.classList.remove('on');
  st.classList.add('evolving');
  sfx('reroll');
  const oldV = stage.view;
  const newKey = E.key(id);
  const ready = Promise.race([DT.Legends.modelFor(newKey), new Promise((r) => setTimeout(r, 2500))]);
  ready.then(() => {
    const newV = makeView(newKey);
    newV.group.visible = false;
    stage.extra = newV;
    let t = 0;
    let flashed = false;
    stage.anim = (dt) => {
      t += dt;
      const spin = Math.min(1, t / 0.9);
      if (t > 0.5) { stage.scene.overrideMaterial = stage.white; stage.floor.visible = false; }
      if (t < 2.7) {
        // swap faster and faster: every 0.32 s at first, every 0.04 s at the end
        const k = Math.max(0, (t - 0.9) / 1.8);
        const period = 0.32 - 0.28 * k;
        const showNew = t > 0.9 && Math.floor((t - 0.9) / period) % 2 === 1;
        oldV.group.visible = !showNew;
        newV.group.visible = showNew;
        const s = 1 + 0.08 * Math.sin(t * 18) * spin;
        oldV.group.scale.setScalar(s);
        newV.group.scale.setScalar(s);
        oldV.group.rotation.y = newV.group.rotation.y = t * t * 3;
      } else if (!flashed) {
        flashed = true;
        flash.classList.remove('on'); void flash.offsetWidth; flash.classList.add('on');
        sfx('digivolve');
        setTimeout(() => {
          stage.scene.overrideMaterial = null;
          stage.floor.visible = true;
          oldV.dispose();
          newV.group.visible = true;
          newV.group.scale.setScalar(1);
          newV.group.rotation.y = 0;
          stage.view = newV;
          stage.key = newKey;
          stage.extra = null;
          stage.anim = null;
          st.classList.remove('evolving');
          root.querySelector('.dte-fname').innerHTML = `<b>${esc(to.name)}</b><span>${esc(to.level)}</span>`;
          said.textContent = `${fromName} digivolved into ${to.name}!`;
          said.classList.add('on');
          newV.play('happy');
          busy = false;
          render();
          setTimeout(() => said.classList.remove('on'), 3200);
        }, 260);
      }
    };
  });
}

function onClick(e) {
  const b = e.target.closest('button');
  if (!b) { if (e.target === root) close(); return; }
  const d = b.dataset;
  if (b.classList.contains('dte-close')) return close();
  if (busy) return;
  if (d.evoPick) { cur = d.evoPick; return render(); }
  if (d.evoGo) return evolve(cur);
  if (d.evoRevert) {
    const r = E.revert(cur);
    if (r) { sfx('sell'); flashSay(`Back to ${E.formInfo(r.to).name}.`); }
    return render();
  }
  if (d.evoLevel) { const r = E.buyLevel(cur); if (r) { sfx('levelup'); flashSay(`Level ${r.level}!`); } return render(); }
  if (d.evoFeed) { const r = E.feed(cur); if (r) { sfx('pick'); E.info(cur) && stage.view && stage.view.play('eat'); } return render(); }
  if (d.evoEquip) { C.equip('legend', cur); sfx('equip'); return render(); }
}
function flashSay(text) {
  const s = root.querySelector('.dte-said');
  s.textContent = text;
  s.classList.add('on');
  clearTimeout(flashSay.t);
  flashSay.t = setTimeout(() => s.classList.remove('on'), 1800);
}

function open(id) {
  build();
  cur = id && C.get('legend', String(id).split('@')[0]) ? String(id).split('@')[0] : C.equipped('legend');
  root.classList.remove('hidden');
  ensureStage();
  render();
  if (!stage.raf) { stage.last = 0; stage.raf = requestAnimationFrame(loop); }
}
function close() {
  if (!root || busy) return;
  root.classList.add('hidden');
  cancelAnimationFrame(stage.raf);
  stage.raf = 0;
  if (stage.view) { stage.view.dispose(); stage.view = null; stage.key = null; }
}
const isOpen = () => !!root && !root.classList.contains('hidden');

E.open = open;
E.close = close;
E.isOpen = isOpen;

document.addEventListener('click', (e) => {
  const b = e.target.closest && e.target.closest('[data-evo-open]');
  if (!b) return;
  e.preventDefault();
  open(b.getAttribute('data-evo-open'));
});
// while open, keys belong to the panel (Esc closes it; game keys are held back)
window.addEventListener('keydown', (e) => {
  if (!isOpen()) return;
  if (e.key === 'Escape') close();
  e.stopImmediatePropagation();
}, true);

let queued = false;
const later = () => { if (queued || !isOpen()) return; queued = true; requestAnimationFrame(() => { queued = false; render(); }); };
E.onChange(later);
W.onChange(later);
window.addEventListener('resize', () => { if (isOpen()) { fitTree(); requestAnimationFrame(drawEdges); } });
