// Item preview cards for the tooltip: the item hover card with its recipe grid, the combine preview shown while
// dragging an item over another item, and the drop preview shown while dragging an item over a Digimon.
// Returns HTML strings; js/ui.js puts them in #tooltip. Also turns a long press / touch drag on an item pedestal
// into the same mouse drag. Styles are injected here so the packaged builds need no extra file.
const DT = window.DT;

const css = `
#tooltip.icard { width: 324px; padding: 0; background: none; border: none; box-shadow: none; }
.ic { --ic: #9aa6bb; position: relative; background: linear-gradient(180deg, #161d2c, #0e131d 70%); border: 2px solid var(--ic);
  border-radius: 10px; padding: 10px 12px 10px; box-shadow: 0 10px 30px rgba(0,0,0,0.6), 0 0 0 1px rgba(0,0,0,0.6), inset 0 0 24px rgba(0,0,0,0.35); font-size: 13px; }
.ic.no { --ic: #6a7386; }
.ic-eq { display: flex; align-items: center; justify-content: center; gap: 6px; margin: 0 0 8px; padding-bottom: 8px; border-bottom: 1px solid var(--line); }
.ic-eq img { width: 32px; height: 32px; }
.ic-eq img.r { width: 40px; height: 40px; filter: drop-shadow(0 0 6px var(--ic)); }
.ic-eq i { font-style: normal; font-weight: 900; color: var(--muted); font-size: 16px; }
.ic-eq .x { width: 40px; height: 40px; display: grid; place-items: center; border: 2px dashed #5a6374; border-radius: 8px; color: #8b93a3; font-size: 20px; }
.ic-hd { display: flex; gap: 10px; align-items: center; }
.ic-big { width: 58px; height: 58px; flex: none; border-radius: 10px; background: radial-gradient(circle, rgba(255,255,255,0.08), rgba(0,0,0,0) 70%); }
.ic-nm { font-size: 17px; font-weight: 800; line-height: 1.15; color: #fff; }
.ic-kind { font-size: 11px; text-transform: uppercase; letter-spacing: 0.06em; color: var(--ic); font-weight: 700; margin-top: 2px; }
.ic-st { margin-top: 7px; font-weight: 700; color: #ffe08a; font-size: 13px; }
.ic-st small { font-weight: 600; color: var(--muted); font-size: 11px; }
.ic-fx { margin-top: 5px; color: var(--text); line-height: 1.35; }
.ic-tr { margin-top: 6px; padding: 5px 8px; border-radius: 6px; background: rgba(255,255,255,0.04); border-left: 3px solid var(--tc, var(--ic)); font-size: 12px; color: var(--muted); }
.ic-tr b { color: var(--tc, var(--text)); }
.ic-note { margin-top: 7px; color: var(--muted); font-size: 12px; }
.ic-act { font-weight: 800; margin: 8px 0 6px; font-size: 14px; color: var(--good); }
.ic-act.bad { color: var(--bad); }
.ic-unit { display: flex; gap: 8px; align-items: center; padding-bottom: 7px; border-bottom: 1px solid var(--line); }
.ic-unit .pt { width: 40px; height: 40px; border-radius: 8px; border: 2px solid var(--tier, #556); flex: none; }
.ic-unit .un { flex: 1; min-width: 0; font-weight: 800; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
.ic-unit .un small { display: block; font-weight: 600; color: var(--muted); font-size: 11px; }
.ic-slots { display: flex; gap: 3px; flex: none; }
.ic-slots span { width: 22px; height: 22px; border-radius: 5px; border: 1px dashed #4a5468; display: grid; place-items: center; }
.ic-slots img { width: 22px; height: 22px; }
.ic-slots img.ghost { opacity: 0.6; }
.ic-slots span.hl { border: 1px solid var(--ic); box-shadow: 0 0 6px var(--ic); }
.ic-gh { margin: 9px 0 4px; font-size: 11px; text-transform: uppercase; letter-spacing: 0.06em; color: var(--muted); display: flex; justify-content: space-between; }
.ic-gh em { font-style: normal; color: var(--good); text-transform: none; letter-spacing: 0; }
.ic-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 3px; }
.ic-cell { display: flex; flex-direction: column; align-items: center; gap: 1px; padding: 4px 2px 3px; border-radius: 6px; background: rgba(255,255,255,0.03);
  border: 1px solid rgba(255,255,255,0.06); min-width: 0; }
.ic-cell .ii { display: flex; align-items: center; gap: 3px; }
.ic-cell .ii img { width: 16px; height: 16px; opacity: 0.85; }
.ic-cell .ii img.r { width: 26px; height: 26px; opacity: 1; }
.ic-cell .ii i { font-style: normal; font-size: 10px; color: var(--muted); }
.ic-cell b { font-size: 10.5px; line-height: 1.15; text-align: center; color: var(--text); font-weight: 600; max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.ic-cell.have { border-color: var(--good); background: rgba(90,223,138,0.1); }
.ic-cell.have b { color: var(--good); }
@media (max-height: 700px) { .ic-big { width: 46px; height: 46px; } .ic-cell .ii img.r { width: 22px; height: 22px; } }
`;
if (!document.getElementById('itemcardsCss')) {
  const st = document.createElement('style');
  st.id = 'itemcardsCss';
  st.textContent = css;
  document.head.appendChild(st);
}

const esc = (s) => String(s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/"/g, '&quot;');
const name = (id) => (DT.ITEMS[id] ? DT.ITEMS[id].name : id);

// Item picture: the item art module if it is loaded, otherwise the canvas icon from js/icons.js.
function icon(id, size = 64) {
  const A = DT.ItemArt;
  if (A && typeof A.url === 'function') {
    try { const u = A.url(id, size); if (u) return u; } catch (e) { /* fall back */ }
  }
  return DT.itemIcon ? DT.itemIcon(id) : '';
}
const img = (id, cls = '', size = 64) => (DT.ITEMS[id] ? `<img class="${cls}" src="${icon(id, size)}" alt="${esc(name(id))}" data-item="${id}">` : '');

// Border color by kind.
function tone(id) {
  const it = DT.ITEMS[id];
  if (!it) return '#6a7386';
  if (it.emblem) return DT.TRAITS[it.emblem].color;
  if (id === 'golden_egg') return '#ffd84a';
  if (it.consumable) return '#ff8fa3';
  if (it.component) return id === 'egg' ? '#fff3b0' : '#b7c0cf';
  return '#ffcf4a';
}
function kind(id) {
  const it = DT.ITEMS[id];
  if (it.consumable) return 'Consumable';
  if (it.emblem) return 'Crest';
  if (id === 'golden_egg') return 'Special item';
  if (it.component) return id === 'egg' ? 'Special component' : 'Component';
  return 'Completed item';
}

// Stats the recipe adds on top of its two components.
function bonus(id) {
  const it = DT.ITEMS[id];
  if (!it || !it.from || it.component || it.emblem) return {};
  const out = { ...it.stats };
  for (const c of it.from) for (const k in DT.COMPONENTS[c].stats) out[k] -= DT.COMPONENTS[c].stats[k];
  for (const k in out) if (Math.abs(out[k]) < 1e-9) delete out[k];
  return out;
}
function statLine(id) {
  const it = DT.ITEMS[id];
  const st = DT.statText(it.stats || {});
  if (!st) return '';
  const b = DT.statText(bonus(id));
  return `<div class="ic-st">${st}${b ? ` <small>(includes ${b} bonus)</small>` : ''}</div>`;
}
function effect(id) {
  const it = DT.ITEMS[id];
  if (it.component && id !== 'egg') return '';
  return `<div class="ic-fx">${esc(it.desc)}</div>`;
}
function traitBox(trait, extra = '') {
  const T = DT.TRAITS[trait];
  const bpText = (i) => (T.bpDesc && T.bpDesc[i] != null ? T.bpDesc[i] : T.fmt ? T.fmt(T.lv[i], i) : '');
  const bps = T.bp.map((b, i) => `(${b}) ${bpText(i)}`).join(' · ');
  return `<div class="ic-tr" style="--tc:${T.color}">Adds <b>${esc(T.name)}</b>${extra}<br>${esc(T.desc)}<br>${esc(bps)}</div>`;
}
// Big portrait, name, kind, stats, effect.
function body(id, traitExtra = '') {
  const it = DT.ITEMS[id];
  return `<div class="ic-hd">${img(id, 'ic-big', 128)}<div><div class="ic-nm">${esc(it.name)}</div><div class="ic-kind">${kind(id)}</div></div></div>
    ${statLine(id)}${effect(id)}${it.emblem ? traitBox(it.emblem, traitExtra) : ''}`;
}
const eq = (a, b, r) => `<div class="ic-eq">${img(a)}<i>+</i>${img(b)}<i>→</i>${r ? img(r, 'r', 96) : '<span class="x">✕</span>'}</div>`;
const card = (t, inner, cls = '') => `<div class="ic${cls ? ' ' + cls : ''}" style="--ic:${t}">${inner}</div>`;

// Component counts a player owns: bench pedestals plus components held on their Digimon.
function owned(p) {
  const n = {};
  if (!p) return n;
  for (const id of p.items) if (DT.isComponent(id)) n[id] = (n[id] || 0) + 1;
  for (const u of p.allUnits()) for (const id of u.items) if (DT.isComponent(id)) n[id] = (n[id] || 0) + 1;
  return n;
}

// TFT recipe row: the component with each of the 9 components. "have" = you own the other part (a second copy for a pair).
function recipeGrid(id, p) {
  const n = owned(p);
  const self = p ? 1 : 0; // the hovered item is one of the owned copies
  let ready = 0;
  const cells = DT.COMPONENT_IDS.map((c) => {
    const r = DT.combineItems(id, c);
    if (!r) return '';
    const have = (n[c] || 0) - (c === id ? self : 0) > 0 && (n[id] || 0) > 0;
    if (have) ready++;
    return `<div class="ic-cell${have ? ' have' : ''}" data-recipe="${r}" title="${esc(name(c))} → ${esc(name(r))}"><div class="ii">${img(c, '', 32)}<i>→</i>${img(r, 'r', 64)}</div><b>${esc(name(r))}</b></div>`;
  }).join('');
  return `<div class="ic-gh"><span>Recipes</span>${ready ? `<em>${ready} you can make now</em>` : ''}</div><div class="ic-grid">${cells}</div>`;
}

// Hover card for an item on the bench (also shown while holding it over empty ground).
function itemCard(id, p) {
  const it = DT.ITEMS[id];
  if (!it) return '';
  let inner = body(id);
  if (it.component) inner += recipeGrid(id, p);
  else if (it.from) inner += `<div class="ic-note">Made from ${DT.recipesOf(id).map((r) => r.map(name).join(' + ')).join(' or ')}</div>`;
  if (it.consumable) inner += '<div class="ic-note">Drag onto a Digimon to use it.</div>';
  return card(tone(id), inner);
}

// Dragging item a over bench item b. Items only combine on a Digimon; dropping here swaps the two pedestals.
function pairCard(a, b) {
  const A = DT.ITEMS[a], B = DT.ITEMS[b];
  if (!A || !B) return '';
  const swap = '<div class="ic-note">Items combine on a Digimon. Drop here to swap places.</div>';
  if (A.component && B.component) {
    const r = DT.combineItems(a, b);
    return card(tone(r), eq(a, b, r) + body(r) + swap);
  }
  let why;
  if (A.consumable || B.consumable) {
    const c = A.consumable ? A : B;
    why = `${esc(c.name)} is used on a Digimon. ${esc(c.desc.replace(/^Drag onto a Digimon to /, 'It will ').replace(/\.$/, ''))}.`;
  } else if (A.emblem && B.emblem) {
    why = `Crests don't combine. Each one gives its holder a trait, so put them on different Digimon.${traitBox(A.emblem)}${A.emblem !== B.emblem ? traitBox(B.emblem) : ''}`;
  } else if (a === 'golden_egg' || b === 'golden_egg') {
    why = `The Golden Digi-Egg doesn't combine. ${esc(DT.ITEMS.golden_egg.desc)}`;
  } else {
    why = 'Only two components combine. Completed items stay as they are.';
  }
  return card('#6a7386', eq(a, b, null) + `<div class="ic-act bad">Doesn't combine</div><div class="ic-fx">${why}</div>` +
    '<div class="ic-note">Drop here to swap places.</div>', 'no');
}

// Dragging item id over Digimon u (owned by p). Mirrors game.equipItem / useConsumable.
function unitCard(id, u, game, p) {
  const it = DT.ITEMS[id];
  if (!it || !u) return '';
  const L = DT.LINE[u.line];
  const f = game.findUnit(p, u.uid);
  let hl = -1; // slot to highlight
  let act, ok = true, inner = '', result = null;
  if (f && f.where === 'board' && game.boardLocked(p)) { act = 'The board is locked during combat'; ok = false; }
  else if (it.consumable) {
    // the item's own preview (or description): js/items.js DT.consumablePreview
    const pv = DT.consumablePreview(id, u, p, game);
    act = pv.text; ok = pv.ok;
    if (ok && pv.detail) inner = `<div class="ic-fx">${esc(pv.detail)}</div>`;
  } else if (it.emblem && DT.unitTraits(u).includes(it.emblem)) {
    act = `Already has ${DT.TRAITS[it.emblem].name}`; ok = false;
  } else if (DT.holdsItems && !DT.holdsItems(u)) {
    act = "Can't hold items"; ok = false;
  } else {
    const ci = it.component ? u.items.findIndex((x) => DT.isComponent(x) && DT.combineItems(x, id)) : -1;
    const veto = game.equipVeto ? game.equipVeto(p, u, id, ci >= 0 ? DT.combineItems(u.items[ci], id) : null) : null;
    if (veto) {
      const glove = DT.itemSlots(ci >= 0 ? DT.combineItems(u.items[ci], id) : id) === 3 || u.items.some(held => DT.itemSlots(held) === 3);
      act = veto === 'full' ? glove ? 'Gloves occupy all 3 item slots' : DT.itemSlotsUsed(u.items) >= 3 ? 'Full: 3 items' : "Full: can't hold another item" : "Can't hold this item"; ok = false;
    }
    else if (ci >= 0) {
      const held = u.items[ci];
      result = DT.combineItems(held, id);
      const R = DT.ITEMS[result];
      hl = ci;
      if (R.emblem && DT.unitTraits({ ...u, items: u.items.filter((_, k) => k !== ci) }).includes(R.emblem)) {
        act = `Would make ${R.name}, but it already has ${DT.TRAITS[R.emblem].name}`; ok = false;
      } else {
        act = `Combines with its ${name(held)} into ${R.name}`;
        inner = eq(held, id, result);
      }
    } else if (u.items.length >= 3) { act = 'Full: 3 items'; ok = false; }
    else { act = `Equips ${it.name}`; result = id; hl = u.items.length; }
  }
  if (ok && result) {
    const R = DT.ITEMS[result];
    let extra = '';
    if (R.emblem && f && f.where === 'board') {
      const before = DT.countTraits(p.boardUnits(), p.augs)[R.emblem] || 0;
      const items = hl >= 0 ? u.items.map((x, k) => (k === hl ? result : x)) : u.items.concat(result);
      const after = DT.countTraits(p.boardUnits().map((x) => (x === u ? { ...u, items } : x)), p.augs)[R.emblem] || 0;
      extra = `: ${before} → ${after} on your board`;
    }
    inner += body(result, extra);
    const slots = DT.itemSlotsUsed(hl >= 0 && hl < u.items.length ? u.items.map((held, i) => i === hl ? result : held) : u.items.concat(result));
    inner += `<div class="ic-note">Items after: ${slots}/3</div>`;
  }
  const slots = [0, 1, 2].map((k) => `<span class="${k === hl ? 'hl' : ''}">${u.items[k] ? img(u.items[k], '', 32) : k === hl && ok ? img(id, 'ghost', 32) : ''}</span>`).join('');
  const head = `<div class="ic-unit" style="--tier:${DT.TIER_COLORS[L.tier]}">${DT.Portraits ? DT.Portraits.html(L.id, u.stage) : ''}<div class="un">${esc(L.forms[u.stage])}<small>${f && f.where === 'bench' ? 'On your bench' : 'On your board'}</small></div><div class="ic-slots">${slots}</div></div>`;
  return card(ok ? tone(result || id) : '#ff5a6a', head + `<div class="ic-act${ok ? '' : ' bad'}">${esc(act)}</div>` + inner, ok ? '' : 'no');
}

DT.ItemCards = { icon, owned, recipeGrid, itemCard, pairCard, unitCard };

// Touch: a finger on an item pedestal drives the same mouse drag (holding shows the item card, as a long press would).
const board = document.getElementById('board');
if (board) {
  let touching = false;
  const send = (type, t, target) => target.dispatchEvent(new MouseEvent(type, { bubbles: true, cancelable: true, clientX: t.clientX, clientY: t.clientY, button: 0, buttons: type === 'mouseup' ? 0 : 1 }));
  board.addEventListener('touchstart', (ev) => {
    const d = window.__digi;
    if (ev.touches.length !== 1 || !d || !d.game || !d.renderer) return;
    const t = ev.touches[0];
    const r = board.getBoundingClientRect();
    if (d.renderer.itemSlotAt(t.clientX - r.left, t.clientY - r.top, d.game.human.items.length) < 0) return;
    ev.preventDefault();
    touching = true;
    send('mousedown', t, board);
    send('mousemove', t, board);
  }, { passive: false });
  board.addEventListener('touchmove', (ev) => {
    if (!touching) return;
    ev.preventDefault();
    const t = ev.touches[0];
    send('mousemove', t, document.elementFromPoint(t.clientX, t.clientY) || board);
  }, { passive: false });
  const end = (ev) => {
    if (!touching) return;
    touching = false;
    const t = ev.changedTouches[0];
    send('mouseup', t, document.elementFromPoint(t.clientX, t.clientY) || board);
  };
  board.addEventListener('touchend', end);
  board.addEventListener('touchcancel', end);
}
