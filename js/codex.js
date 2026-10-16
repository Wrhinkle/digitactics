// Codex: an in-game side panel that explains the traits in play, and a full-screen encyclopedia from the title screen.
// Everything is built from game data at runtime, so new traits, Digimon lines, items, augments and Wisps show up on their own.
//
// Trait fields read: name, kind, color, bp, lv, desc, fmt(value, index).
// Optional trait fields:
//   bpDesc: array of strings, one per breakpoint. Used instead of fmt() when present.
//   notes:  string or array of strings, shown under the description.
//   icon:   image URL, or 1-3 characters shown in the badge instead of initials.
// Any kind value works; the family/role filters are made from the kinds that exist.
//
// DT.Codex.toggle()              C key: the side panel in a game, the full codex on the title screen
// DT.Codex.openPanel(traitId?)   side panel, scrolled to a trait
// DT.Codex.openFull(tab?, q?, setId?)  tab: 'traits' | 'digimon' | 'items' | 'augments' | 'wisps' | 'rules'
// DT.Codex.browseSet(id)        the full codex shows another set (Faction Wars, Dragonlands, Auto Chess...). It reads the
//                                globals, so the set is activated only while rendering and the active set is restored
//                                right after. Not while a game runs: then the codex stays on the live game's set.
// DT.Codex.close()
import './codexstyle.js';

const DT = window.DT;
const KEY = 'c';
const UI = () => DT.UI || {};
const game = () => (UI().game ? UI().game() : null);
const esc = (s) => String(s == null ? '' : s).replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;');
const cap = (s) => String(s || '').charAt(0).toUpperCase() + String(s || '').slice(1);
const plural = (n, one, many) => `${n} ${n === 1 ? one : many || one + 's'}`;
const $ = (id) => document.getElementById(id);

// ---------- data helpers ----------
function trait(id) {
  const T = (DT.TRAITS && DT.TRAITS[id]) || {};
  return { id, T, name: T.name || id, color: T.color || '#8e98ad', kind: T.kind || 'trait', bp: Array.isArray(T.bp) ? T.bp : [] };
}
function bpText(T, i) {
  if (Array.isArray(T.bpDesc) && T.bpDesc[i] != null) return String(T.bpDesc[i]);
  const v = Array.isArray(T.lv) ? T.lv[i] : undefined;
  if (typeof T.fmt === 'function') { try { return tidy(String(T.fmt(v, i))); } catch (e) { /* bad fmt: fall through */ } }
  return v == null ? '' : tidy(String(v));
}
// float noise from fmt() like 0.55 * 100 = 55.00000000000001
const tidy = (s) => s.replace(/\d+\.\d{5,}/g, (x) => String(Number(Number(x).toFixed(3))));
const notesOf = (T) => (Array.isArray(T.notes) ? T.notes : T.notes ? [T.notes] : []).map(String);
const levelOf = (bp, n) => bp.reduce((lv, b) => (n >= b ? lv + 1 : lv), 0);
const allTraitIds = () => Object.keys(DT.TRAITS || {});
const linesWith = (id) => DT.LINES.filter((l) => (l.traits || []).includes(id));
const crestsFor = (id) => Object.values(DT.ITEMS || {}).filter((it) => it && it.emblem === id);
const tierColor = (t) => (DT.TIER_COLORS && DT.TIER_COLORS[t]) || '#9aa4b1';
const kinds = () => [...new Set(allTraitIds().map((id) => trait(id).kind))].sort((a, b) => kindOrder(a) - kindOrder(b) || a.localeCompare(b));
const kindOrder = (k) => (k === 'family' ? 0 : k === 'role' ? 1 : 2);
const sortTraits = (ids) => ids.slice().sort((a, b) => kindOrder(trait(a).kind) - kindOrder(trait(b).kind) || trait(a).name.localeCompare(trait(b).name));

function badge(id, cls = '') {
  const t = trait(id);
  const ic = t.T.icon;
  let inner;
  if (ic && /[/.]/.test(ic)) inner = `<img src="${esc(ic)}" alt="">`;
  else inner = esc(ic || (t.name.replace(/[^A-Z]/g, '').slice(0, 2) || t.name.slice(0, 2)));
  return `<span class="cx-hex ${cls}" style="--tc:${t.color}">${inner}</span>`;
}

function itemImg(id, cls = '') {
  let url = '';
  try { if (DT.ItemArt && typeof DT.ItemArt.url === 'function') url = DT.ItemArt.url(id, 64) || ''; } catch (e) { url = ''; }
  if (!url && DT.itemIcon) url = DT.itemIcon(id);
  const it = DT.ITEMS[id] || {};
  return url ? `<img class="cx-ii ${cls}" src="${esc(url)}" alt="" title="${esc(it.name || id)}">` : `<span class="cx-ii ${cls}">${esc(it.short || '?')}</span>`;
}

const portrait = (lineId, stage, cls = '') => (DT.Portraits ? DT.Portraits.html(lineId, stage, cls) : `<span class="pt ${cls}"><b>${esc(DT.LINE[lineId].forms[stage].slice(0, 2))}</b></span>`);

// What the viewer owns, for marks: line -> { board: best stage on board or -1, bench: best stage on bench or -1, n }
function ownedMap(p) {
  const m = new Map();
  if (!p) return m;
  const add = (u, where) => {
    const o = m.get(u.line) || { board: -1, bench: -1, n: 0, forms: [] };
    o[where] = Math.max(o[where], u.stage);
    o.n++;
    o.forms.push({ stage: u.stage, where });
    m.set(u.line, o);
  };
  for (const u of p.boardUnits()) add(u, 'board');
  for (const u of p.bench) if (u) add(u, 'bench');
  return m;
}

function gameCtx() {
  const g = game();
  if (!g || !g.human || !g.pool) return null;
  const p = g.human;
  return { g, p, counts: DT.countTraits(p.boardUnits(), p.augs), owned: ownedMap(p) };
}

// ---------- shared markup ----------
function unitChip(l, ctx, size = '') {
  const o = ctx && ctx.owned.get(l.id);
  const stage = o ? Math.max(o.board, o.bench) : 0;
  const mark = o ? (o.board >= 0 ? '<i class="cx-mark board" title="On your board"></i>' : '<i class="cx-mark own" title="On your bench"></i>') : '';
  const where = o ? (o.board >= 0 ? ', on your board' : ', on your bench') : '';
  return `<button class="cx-u ${size}${l.shop ? '' : ' wild'}${o ? ' owned' : ''}" data-cx-line="${esc(l.id)}" title="${esc(l.forms[stage])} (${l.shop ? `${l.tier} gold` : 'wild'}${where})">
    ${portrait(l.id, stage, `t${l.tier}`)}${mark}${stage ? `<i class="cx-stars">${'★'.repeat(stage + 1)}</i>` : ''}</button>`;
}

// One trait entry. ctx: in-game counts and ownership, or null for the out-of-game codex.
function traitHTML(id, ctx, opts = {}) {
  const t = trait(id);
  const T = t.T;
  const n = ctx ? ctx.counts[id] || 0 : 0;
  const lv = levelOf(t.bp, n);
  const next = t.bp.find((b) => b > n);
  const lines = linesWith(id).filter((l) => !ctx || l.shop || opts.wild);
  const shop = lines.filter((l) => l.shop).sort((a, b) => a.tier - b.tier || a.forms[0].localeCompare(b.forms[0]));
  const wild = lines.filter((l) => !l.shop);
  const crests = crestsFor(id);
  const count = ctx ? `<span class="cx-count${lv ? ' on' : ''}" title="Different Digimon on your board with this trait">${n}${next ? `<small>/${next}</small>` : ''}</span>` : '';
  return `<div class="cx-trait${lv ? ' active' : ''}${lv && lv >= t.bp.length ? ' max' : ''}" data-cx-tid="${esc(id)}" style="--tc:${t.color}">
    <div class="cx-th">${badge(id)}<div class="cx-tn"><b>${esc(t.name)}</b><span class="cx-kind">${esc(cap(t.kind))}</span></div>${count}</div>
    ${T.desc ? `<div class="cx-desc">${esc(T.desc)}</div>` : ''}
    ${notesOf(T).map((x) => `<div class="cx-note">${esc(x)}</div>`).join('')}
    <div class="cx-bps">${t.bp.map((b, i) => `<div class="cx-bp${ctx && i < lv ? ' on' : ''}${ctx && i === lv - 1 ? ' cur' : ''}"><b>${b}</b><span>${esc(bpText(T, i))}</span></div>`).join('')}</div>
    ${shop.length ? `<div class="cx-units">${shop.map((l) => unitChip(l, ctx)).join('')}</div>` : ''}
    ${wild.length ? `<div class="cx-sub">Wild</div><div class="cx-units">${wild.map((l) => unitChip(l, null)).join('')}</div>` : ''}
    ${crests.map((c) => `<div class="cx-crest">${itemImg(c.id, 'sm')}<span>${esc(c.name)}: ${c.from ? c.from.map((f) => esc((DT.ITEMS[f] || {}).name || f)).join(' + ') : `found in ${esc(dropSource())}`}</span></div>`).join('')}
  </div>`;
}

function statLine(s) {
  return `<span>AS <b>${s.as}</b></span><span>Range <b>${s.range}</b></span><span>Armor <b>${s.armor}</b></span><span>MR <b>${s.mr}</b></span><span>Mana <b>${s.mana}/${s.maxMana}</b></span>`;
}

function attrText(attr) {
  const beats = DT.ATTR_BEATS[attr];
  const by = Object.keys(DT.ATTR_BEATS).find((k) => DT.ATTR_BEATS[k] === attr);
  if (!beats) return 'No attribute advantage or weakness.';
  return `Deals 20% more damage to ${beats}${by ? `, takes 20% more from ${by}` : ''}.`;
}

function unitHTML(lineId, ctx) {
  const l = DT.LINE[lineId];
  if (!l) return '<div class="cx-empty">Unknown Digimon.</div>';
  const A = DT.ATTR[l.attr] || { color: '#c8c8c8' };
  const o = ctx && ctx.owned.get(l.id);
  const stages = [0, 1, 2].filter((s) => l.forms[s] != null);
  const s0 = DT.unitStats(l, 0);
  let own = '';
  if (o) {
    const parts = o.forms.map((f) => `${l.forms[f.stage]}${f.stage ? ' ' + '★'.repeat(f.stage + 1) : ''} (${f.where === 'board' ? 'board' : 'bench'})`);
    own = `<div class="cx-ownline">You have: ${esc(parts.join(', '))}</div>`;
  }
  const pool = ctx && l.shop && ctx.g.pool[l.id] != null ? `<span class="cx-pill">${ctx.g.pool[l.id]}/${DT.POOL_SIZE[l.tier]} left in the pool</span>` : '';
  return `<div class="cx-unit" style="--tc:${tierColor(l.tier)}">
    <div class="cx-uh">
      <div class="cx-uname"><b>${esc(l.forms[0])}</b> <span class="muted">line</span></div>
      <div class="cx-pills"><span class="cx-pill cost">${l.shop ? `${l.tier} gold` : `Wild (tier ${l.tier})`}</span>
        <span class="cx-pill" style="color:${A.color};border-color:${A.color}66" title="${esc(attrText(l.attr))}">${esc(l.attr)}</span>${pool}</div>
      <div class="cx-tchips">${(l.traits || []).map((id) => `<button class="cx-tchip" data-cx-goto="${esc(id)}" style="--tc:${trait(id).color}">${badge(id, 'xs')}${esc(trait(id).name)}</button>`).join('')}</div>
      <div class="cx-attr">${esc(l.attr)}: ${esc(attrText(l.attr))}</div>
      ${own}
    </div>
    <div class="cx-common">${statLine(s0)}</div>
    ${stages.map((s) => {
      const st = DT.unitStats(l, s);
      return `<div class="cx-form">
        ${portrait(l.id, s, `t${l.tier} big`)}
        <div class="cx-fbody">
          <div class="cx-fname"><b>${esc(l.forms[s])}</b> <span class="cx-stars">${'★'.repeat(s + 1)}</span> <span class="muted">${esc(DT.stageName(l, s))}</span></div>
          <div class="cx-fstats">HP <b>${st.maxHp}</b> · AD <b>${st.ad}</b></div>
          <div class="cx-abil"><b>${esc((l.abil || [])[s] || 'Ability')}</b> ${esc(DT.abilityText(l, s))}</div>
        </div></div>`;
    }).join('')}
    ${l.shop ? `<div class="cx-foot">Three of one form digivolve into the next. Sells for ${stages.map((s) => DT.sellValue({ line: l.id, stage: s })).join(' / ')} gold.</div>` : '<div class="cx-foot">Wild Digimon appear in PvE rounds only and never count for traits.</div>'}
  </div>`;
}

// ---------- rules (built from DT.HYPER and the other tables) ----------
// The active set can supply its own sections with def.codexRules(H) -> [{ id, title, html }]. Sets that are not on
// Hyper Roll rules (XP, interest) get the generic sections; Faction Wars gets the detailed Hyper Roll text.
function rulesSections() {
  const S = DT.SET;
  const pace = paceSection();
  const duos = { id: 'duos', title: 'Duos: teams of two', html: '<p>Choose Duos alongside your set and pace. Eight Tamers form four teams. Partners share health and finish together, from first to fourth, while keeping their own shops, gold, items and boards.</p><p>During planning, drag a bench Digimon or item onto the Partner Portal. Hyper Roll allows one send per round; Normal allows two. Sent copies can complete your partner’s digivolution. A full bench accepts a Digimon only if it can digivolve immediately.</p><p>Both partners fight together on a wide arena against the other team. Traits count across both boards and apply to your whole side. Your augments and items still belong to you. Eleven Duos augments reward teamwork. Wild Digimon rounds and carousels stay individual.</p><p>Click your partner’s panel to view their board. In multiplayer, adjacent lobby seats with the same team number are partners; move to an empty seat to change teams. When a team is eliminated, both partners see the final attacker’s ultimate and the winning team’s victory dance.</p>' };
  if (S && typeof S.def.codexRules === 'function') return [pace, duos, ...(S.def.codexRules(DT.HYPER, S.pace) || [])];
  if (S && (S.rules.xp || !S.rules.levelByStage || !S.rules.incomeByStage)) return [pace, duos, ...genericRules()];
  return [pace, duos, ...hyperRules()];
}

// The pace being shown (Hyper Roll or Normal): HP, damage, levels, income, rounds and timers from the active rules.
function paceSection() {
  const H = DT.HYPER || {};
  const S = DT.SET;
  const pace = (S && S.pace) || H.pace || 'hyper';
  const name = pace === 'normal' ? 'Normal' : 'Hyper Roll';
  const tname = { carousel: 'carousel', pve: 'wild', pvp: 'PvP' };
  const row = (st) => { const out = []; for (let r = 1; r <= H.roundsInStage(st); r++) out.push(`${st}-${r} ${tname[H.roundType(st, r)] || H.roundType(st, r)}`); return out.join(', '); };
  const offers = [];
  for (let st = 1; st <= 7; st++) for (let r = 1; r <= H.roundsInStage(st); r++) {
    const o = DT.Game && DT.Game.prototype.offerAt.call({ R: H }, st, r);
    if (o && (o.kind === 'augment' || o.kind === 'armory')) offers.push(`${o.kind === 'augment' ? 'augment' : 'Armory'} ${st}-${r}`);
  }
  const loss = [2, 3, 4, 5, 6, 7].map((st) => H.lossDamage(st));
  const unitDmg = H.unitDamage ? ' plus damage for each enemy Digimon still standing (1, 1 or 2 by form)' : '';
  const X = H.xp;
  const income = H.incomeByStage ? `Income by stage: ${H.incomeByStage.slice(1).join(' / ')}.`
    : typeof H.baseIncome === 'function' ? `Income: ${H.baseIncome(3, 1)} gold a round (${H.baseIncome(1, 2)} early in stage 1).` : `Income: ${H.baseIncome} gold a round.`;
  const T = H.combatTime || { overtime: 30, limit: 45 };
  const other = pace === 'normal' ? 'Hyper Roll' : 'Normal';
  return { id: 'pace', title: `Pace: ${name}`, html: `
    <p><b>${name}.</b> ${pace === 'normal' ? 'Standard rules: buy XP, save for interest, longer games.' : 'Short games: levels come with each stage, fixed income, cheap rerolls.'} Choose ${other} on the Play page or in the lobby for the other pace.</p>
    <p>${H.startHp} Tamer HP. A loss costs ${loss.join(' / ')} HP in stages 2-7${unitDmg}.</p>
    <p>${X ? `Levels: buy ${X.buyXp || 4} XP for ${X.buyCost || 4} gold (F), +${X.perRound != null ? X.perRound : 2} XP each round, up to level ${X.maxLevel || 9}.` : `Levels by stage: ${(H.levelByStage || []).slice(1).join(' / ')}. No XP.`}
      ${income} ${H.interest ? `Interest: 1 per ${H.interest.per} gold saved, up to ${H.interest.max}.` : 'No interest.'}
      ${H.streak ? `Win or loss streaks: ${H.streak.map(([n, g]) => `${g} gold at ${n}`).join(', ')}.` : 'No streak gold.'} A PvP win pays ${H.winGold}. Rerolls cost ${H.rerollCost}.</p>
    <p>Stage 1: ${esc(row(1))}. Stage 2: ${esc(row(2))}.${offers.length ? ` Picks: ${esc(offers.join(', '))}.` : ''}</p>
    <p>${H.planTime} s planning. Combat overtime starts at ${T.overtime} s; a fight still going at ${T.limit} s is a draw.</p>` };
}

// generic rules text from the active set's tables (XP, interest, streaks, offers)
function genericRules() {
  const H = DT.HYPER || {};
  const S = DT.SET;
  const tiers = [1, 2, 3, 4, 5].filter((t) => DT.POOL_SIZE[t]);
  const X = H.xp;
  const levels = X ? Array.from({ length: (X.maxLevel || 9) - (X.startLevel || 1) + 1 }, (_, i) => (X.startLevel || 1) + i) : [...new Set((H.levelByStage || []).filter(Boolean))];
  const offers = [];
  for (let st = 1; st <= 8; st++) for (let r = 1; r <= H.roundsInStage(st); r++) { const o = DT.Game && DT.Game.prototype.offerAt.call({ R: H }, st, r); if (o) offers.push(`${st}-${r} ${o.kind}`); }
  const mech = S && S.mechanic && S.mechanic.name ? `<p><b>${esc(S.mechanic.name)}.</b> ${esc(S.mechanic.desc || '')}</p>` : '';
  return [
    { id: 'set', title: S ? S.name : 'Set', html: `<p>${esc(S ? S.desc : '')}</p>${mech}` },
    { id: 'econ', title: 'Economy', html: `
      <p>8 Tamers start with ${H.startHp} HP and ${H.startGold} gold. ${H.incomeByStage ? 'Income is set by stage.' : `Base income is ${typeof H.baseIncome === 'function' ? H.baseIncome(3, 1) : H.baseIncome} gold a round.`}
      ${H.interest ? ` Interest: 1 gold per ${H.interest.per} saved, up to ${H.interest.max}.` : ' No interest.'}
      ${H.streak ? ` Win or loss streaks pay ${H.streak.map(([n, g]) => `${g} at ${n}`).join(', ')}.` : ''} A PvP win pays ${H.winGold}. Rerolls cost ${H.rerollCost}.</p>
      <p>${X ? `Buy ${X.buyXp || 4} XP for ${X.buyCost || 4} gold (F), and gain ${X.perRound != null ? X.perRound : 2} XP each round. Your level is your team size, up to ${X.maxLevel || 9}.` : 'Everyone levels up at the start of each stage.'}</p>` },
    { id: 'shop', title: 'Shop and pool', html: `
      <p>The shop shows ${DT.SHOP} Digimon. All 8 Tamers share one pool: ${tiers.map((t) => `<b style="color:${tierColor(t)}">${DT.POOL_SIZE[t]}</b> of each ${t}-cost`).join(', ')}.</p>
      <table class="cx-tbl odds"><tr><th>Level</th>${tiers.map((t) => `<th style="color:${tierColor(t)}">${t}g</th>`).join('')}</tr>
        ${levels.map((lv) => `<tr><td>${lv}</td>${tiers.map((t) => `<td>${(DT.SHOP_ODDS[Math.min(DT.SHOP_ODDS.length - 1, lv)] || [])[t - 1] || 0}%</td>`).join('')}</tr>`).join('')}</table>` },
    { id: 'evo', title: 'Digivolution', html: `<p>${H.copies || 3} copies of a form digivolve into the next form${DT.SHOP_LINES.some((l) => l.combine) ? ' (some Digimon need fewer)' : ''}. Items carry over.</p>` },
    { id: 'offers', title: 'Picks', html: `<p>${offers.length ? offers.map(esc).join(', ') : 'No picks.'}</p>` },
  ];
}

function hyperRules() {
  const H = DT.HYPER || {};
  const P0 = DT.Player ? new DT.Player(0, '', false) : { gold: 0, hp: H.startHp };
  const typeOf = DT.Game && DT.Game.prototype.typeOf;
  const tname = { carousel: 'Carousel', pve: 'Wild Digimon', pvp: 'PvP' };
  const roundRow = (s) => {
    const out = [];
    for (let r = 1; r <= H.roundsInStage(s); r++) {
      const ty = typeOf ? typeOf.call(null, s, r) : '';
      const tags = [];
      if (s >= 2 && r === 1 && H.armoryStages.includes(s)) tags.push('Armory');
      if (s >= 2 && r === 2 && H.augmentStages.includes(s)) tags.push('Augment');
      if (s >= 2 && r % 2 === 0) tags.push('Wisp');
      out.push(`<span class="cx-rd ${ty}"><b>${s}-${r}</b> ${tname[ty] || ty}${tags.length ? `<i>${tags.join(', ')}</i>` : ''}</span>`);
    }
    return out.join('');
  };
  const stages = [];
  for (let s = 1; s < Math.max(H.levelByStage.length, H.incomeByStage.length) + 1; s++) stages.push(s);
  const at = (arr, s) => arr[Math.min(s, arr.length - 1)];
  const lineA = DT.SHOP_LINES[0];
  const hpMul = lineA ? [1, 2].map((s) => (DT.unitStats(lineA, s).maxHp / DT.unitStats(lineA, 0).maxHp).toFixed(1)) : ['?', '?'];
  const abMul = lineA ? [1, 2].map((s) => (DT.abilityValue(lineA, s) / DT.abilityValue(lineA, 0)).toFixed(1)) : ['?', '?'];
  const tiers = [1, 2, 3, 4, 5].filter((t) => DT.POOL_SIZE[t]);
  const levels = [...new Set(H.levelByStage.filter(Boolean))];
  const attrs = Object.keys(DT.ATTR_BEATS);
  const nComp = DT.STAT_COMPONENTS.length;
  const nCore = DT.CORE_ITEMS.length;
  const nCrest = Object.values(DT.ITEMS).filter((it) => it.emblem).length;
  const augCount = (t) => DT.AUGMENTS.filter((a) => a.tier === t).length;
  const wispKinds = [...new Set(DT.WISPS.map((w) => w.kind))];
  const augRounds = H.augmentStages.map((s) => `${s}-2`).join(', ');
  const sellAt = (t, stage) => { const l = DT.SHOP_LINES.find((x) => x.tier === t); return l ? DT.sellValue({ line: l.id, stage }) : '?'; };

  return [
    { id: 'rounds', title: 'Rounds', html: `
      <p>8 Tamers start with ${P0.hp || H.startHp} HP each. The last one standing wins. Each round has a ${H.planTime} second planning phase (the timer can be turned off), then combat.</p>
      <div class="cx-rounds"><div><span class="cx-rl">Stage 1</span>${roundRow(1)}</div><div><span class="cx-rl">Stage 2+</span>${roundRow(2)}</div></div>
      <p>Armory (pick 1 of 3 items) at the start of stages ${H.armoryStages.join(', ')}. Augments at ${augRounds}. A Data Wisp sits in the shop on even rounds from stage 2.</p>` },
    { id: 'econ', title: 'Economy', html: `
      <p>You start with ${P0.gold} gold. Income is a fixed amount each round by stage. There is <b>no interest</b> and no streak gold. A PvP win pays ${H.winGold} gold. Rerolls cost ${H.rerollCost} gold.</p>
      <p>There is no XP to buy: everyone's level is set at the start of each stage. Your level is your team size. Leap Wisps and Golden Digi-Eggs add one more each.</p>
      <table class="cx-tbl"><tr><th>Stage</th>${stages.map((s) => `<th>${s}${s === stages.length ? '+' : ''}</th>`).join('')}</tr>
        <tr><td>Level</td>${stages.map((s) => `<td>${at(H.levelByStage, s)}</td>`).join('')}</tr>
        <tr><td>Income</td>${stages.map((s) => `<td>${at(H.incomeByStage, s)}</td>`).join('')}</tr>
        <tr><td>Loss</td>${stages.map((s) => `<td>${H.lossDamage(s)}</td>`).join('')}</tr></table>
      <p class="muted">Loss is the Tamer HP lost for a defeat. Losing a stage 1 wild round costs 1 HP. Surviving units add no damage.</p>` },
    { id: 'shop', title: 'Shop and pool', html: `
      <p>The shop shows ${DT.SHOP} Digimon plus a Wisp slot. All 8 Tamers share one pool: ${tiers.map((t) => `<b style="color:${tierColor(t)}">${DT.POOL_SIZE[t]}</b> of each ${t}-cost`).join(', ')}. Buying takes a copy out; selling or being eliminated puts it back.</p>
      <table class="cx-tbl odds"><tr><th>Level</th>${tiers.map((t) => `<th style="color:${tierColor(t)}">${t}g</th>`).join('')}</tr>
        ${levels.map((lv) => `<tr><td>${lv}</td>${tiers.map((t) => `<td>${(DT.SHOP_ODDS[Math.min(9, lv)] || [])[t - 1] || 0}%</td>`).join('')}</tr>`).join('')}</table>
      <p>Your bench holds ${DT.BENCH} Digimon.</p>` },
    { id: 'evo', title: 'Digivolution', html: `
      <p>Three copies of the same form digivolve into the next form: three Rookies make a Champion (★★), three Champions make an Ultimate (★★★). Copies on the bench and board both count. Items carry over (up to 3; extras go to your item bench).</p>
      <p>Each form has about ${hpMul[0]}x and ${hpMul[1]}x the HP and AD of the first, and its ability is ${abMul[0]}x and ${abMul[1]}x as strong.</p>
      <p>A Rookie sells for its cost. A ★★ of cost ${tiers.join('/')} sells for ${tiers.map((t) => sellAt(t, 1)).join('/')} gold, a ★★★ for ${tiers.map((t) => sellAt(t, 2)).join('/')}.</p>` },
    { id: 'traits', title: 'Traits', html: `
      <p>Every Digimon has a family and a role. A trait turns on when enough <b>different</b> Digimon with it are on your board; two copies of the same Digimon count once. Wild Digimon and the bench don't count.</p>
      <p>A Crest (Digi-Egg + a component) gives its holder one more trait.</p>` },
    { id: 'attr', title: 'Attributes', html: `
      <p class="cx-attrs">${attrs.map((a) => `<b style="color:${(DT.ATTR[a] || {}).color}">${esc(a)}</b> beats <b style="color:${(DT.ATTR[DT.ATTR_BEATS[a]] || {}).color}">${esc(DT.ATTR_BEATS[a])}</b>`).join(' · ')}</p>
      <p>The winning side deals 20% more damage. ${Object.keys(DT.ATTR).filter((a) => !DT.ATTR_BEATS[a]).map(esc).join(', ')} Digimon have no advantage or weakness.</p>` },
    { id: 'items', title: 'Items and recipes', html: `
      <p>${nComp} stat components plus the Digi-Egg. Drag a component onto a Digimon; a second component combines into one of ${nCore} completed items. Up to 3 items per Digimon. Items never combine on the item bench, which holds ${H.itemCap}.</p>
      <p>Digi-Egg + component = one of ${nCrest} Crests. Two Digi-Eggs make a Golden Digi-Egg (+1 team size). Selling a Digimon returns its items. Magnetic Remover and Reforger take items off or reroll them.</p>
      <p>Items come from the 1-1 carousel, the Armory (components through stage 3, completed items after) and loot orbs from wild rounds. Walk your Little Legend into an orb to open it.</p>` },
    { id: 'augs', title: 'Augments', html: `
      <p>Pick 1 of 3 at ${augRounds}, with one reroll. Each augment round rolls one tier for the whole lobby: Silver 35%, Gold 47%, Prismatic 18%.</p>
      <p>${[1, 2, 3].map((t) => `<b style="color:${DT.AUG_TIER_COLOR[t]}">${augCount(t)} ${DT.AUG_TIER_NAME[t]}</b>`).join(', ')} augments.</p>` },
    { id: 'wisps', title: 'Data Wisps', html: `
      <p>On even rounds from stage 2, a one-use Wisp sits in the sixth shop slot. It costs its base price plus the stage number. An unbought Wisp leaves at the end of the round.</p>
      <p>Categories: ${wispKinds.map(esc).join(', ')}.</p>` },
    { id: 'combat', title: 'Combat', html: `
      <p>Digimon attack on their own and gain 10 mana per attack, plus mana when hit. At full mana they cast their ability. Critical strikes deal 160% damage. After ${(H.combatTime || {}).overtime || 30} seconds, overtime adds 20% damage every second.</p>
      <p>Ability values in the codex assume 100 ability power.</p>` },
  ];
}

function rulesHTML(compact) {
  return rulesSections().map((s, i) => compact
    ? `<details class="cx-rule"${i === 0 ? ' open' : ''}><summary>${esc(s.title)}</summary>${s.html}</details>`
    : `<section class="cx-rule"><h3>${esc(s.title)}</h3>${s.html}</section>`).join('');
}

// =====================================================================
// In-game side panel
// =====================================================================
const pv = { open: false, scope: 'board', tab: 'traits', unit: null, focus: null, sig: '' };
let panel = null;

function buildPanel() {
  if (panel) return panel;
  panel = document.createElement('aside');
  panel.id = 'cxPanel';
  panel.className = 'hidden';
  panel.setAttribute('aria-label', 'Codex');
  panel.innerHTML = `<div class="cx-ph">
      <b class="cx-ptitle">Codex</b>
      <button class="cx-icon" data-full="1" title="Open the full codex">⤢</button>
      <button class="cx-icon" data-pclose="1" title="Close (${KEY.toUpperCase()})">✕</button>
    </div>
    <div class="cx-pbar"><div class="cx-seg cx-scope"><button data-scope="board">My board</button><button data-scope="pool" title="Every trait in this game's shop pool">All in game</button><button data-ptab="rules">Rules</button></div></div>
    <div class="cx-pback hidden"><button class="cx-back" data-back="1">← Traits</button></div>
    <div class="cx-pbody"></div>`;
  document.body.appendChild(panel);
  panel.addEventListener('click', onPanelClick);
  return panel;
}

function placePanel() {
  if (!panel || !pv.open) return;
  const tb = $('topbar'), sb = $('shopbar'), tr = $('traits');
  const top = tb ? tb.getBoundingClientRect().bottom : 48;
  const bottom = sb ? Math.max(0, innerHeight - sb.getBoundingClientRect().top) : 0;
  const r = tr ? tr.getBoundingClientRect() : null;
  panel.style.top = `${top}px`;
  panel.style.bottom = `${bottom}px`;
  panel.style.left = `${r && r.width ? r.left : 0}px`;
  panel.style.width = `${Math.max(208, r && r.width ? r.width : 260)}px`;
}

function panelTraitIds(ctx) {
  if (!ctx) return [];
  const board = Object.keys(ctx.counts).filter((id) => DT.TRAITS[id]);
  let ids = board;
  if (pv.scope === 'pool') {
    const set = new Set(board);
    for (const id of Object.keys(ctx.g.pool)) for (const t of (DT.LINE[id] && DT.LINE[id].traits) || []) if (DT.TRAITS[t]) set.add(t);
    ids = [...set];
  }
  const lv = (id) => levelOf(trait(id).bp, ctx.counts[id] || 0);
  return ids.sort((a, b) => lv(b) - lv(a) || (ctx.counts[b] || 0) - (ctx.counts[a] || 0)
    || kindOrder(trait(a).kind) - kindOrder(trait(b).kind) || trait(a).name.localeCompare(trait(b).name));
}

function panelSig(ctx) {
  if (!ctx) return 'none';
  const p = ctx.p;
  const u = (x) => (x ? `${x.line}.${x.stage}.${x.items.join('+')}` : '-');
  return [ctx.g.seed, pv.scope, pv.tab, pv.unit, p.augs.join(','), p.boardUnits().map(u).sort().join(','), p.bench.map(u).join(','),
    pv.unit && ctx.g.pool[pv.unit]].join('|');
}

function renderPanel(force) {
  if (!panel || !pv.open) return;
  const ctx = gameCtx();
  const sig = panelSig(ctx);
  if (!force && sig === pv.sig) return;
  pv.sig = sig;
  panel.querySelector('[data-ptab="rules"]').classList.toggle('on', pv.tab === 'rules');
  panel.querySelectorAll('[data-scope]').forEach((b) => b.classList.toggle('on', pv.tab === 'traits' && b.dataset.scope === pv.scope));
  panel.querySelector('.cx-pback').classList.toggle('hidden', !(pv.tab === 'traits' && pv.unit));
  const body = panel.querySelector('.cx-pbody');
  const top = body.scrollTop;
  if (pv.tab === 'rules') {
    body.innerHTML = `<div class="cx-rules compact">${rulesHTML(true)}</div>`;
  } else if (pv.unit) {
    body.innerHTML = unitHTML(pv.unit, ctx);
  } else {
    const ids = panelTraitIds(ctx);
    body.innerHTML = ids.length ? ids.map((id) => traitHTML(id, ctx)).join('')
      : `<div class="cx-empty">${ctx ? 'No traits on your board yet. Place Digimon to activate them, or switch to <b>All in game</b>.' : 'Start a game to see its traits.'}</div>`;
  }
  body.scrollTop = top;
  if (pv.focus) {
    const f = pv.focus;
    pv.focus = null;
    const el = body.querySelector(`[data-cx-tid="${CSS.escape(f)}"]`);
    if (el) { el.scrollIntoView({ block: 'start' }); el.classList.add('flash'); setTimeout(() => el.classList.remove('flash'), 1200); }
  }
}

function openPanel(traitId) {
  buildPanel();
  pv.open = true;
  panel.classList.remove('hidden');
  if (traitId) {
    pv.tab = 'traits';
    pv.unit = null;
    pv.focus = traitId;
    const ctx = gameCtx();
    if (ctx && !(ctx.counts[traitId] > 0)) pv.scope = 'pool';
  }
  placePanel();
  renderPanel(true);
  syncBtn();
}
function closePanel() {
  if (!panel || !pv.open) return;
  pv.open = false;
  panel.classList.add('hidden');
  syncBtn();
}

function onPanelClick(e) {
  const t = e.target.closest('button');
  if (!t) return;
  const d = t.dataset;
  if (d.pclose) return closePanel();
  if (d.full) return openFull(pv.unit ? 'digimon' : pv.tab === 'rules' ? 'rules' : 'traits');
  if (d.ptab) { pv.tab = d.ptab; pv.unit = null; panel.querySelector('.cx-pbody').scrollTop = 0; return renderPanel(true); }
  if (d.scope) { pv.scope = d.scope; pv.tab = 'traits'; pv.unit = null; return renderPanel(true); }
  if (d.back) { pv.unit = null; return renderPanel(true); }
  if (d.cxLine) { pv.unit = d.cxLine; panel.querySelector('.cx-pbody').scrollTop = 0; return renderPanel(true); }
  if (d.cxGoto) { pv.unit = null; pv.focus = d.cxGoto; const ctx = gameCtx(); if (ctx && !(ctx.counts[d.cxGoto] > 0)) pv.scope = 'pool'; return renderPanel(true); }
}

// =====================================================================
// Full-screen codex
// =====================================================================
const ALL_TABS = [['traits', 'Traits'], ['digimon', 'Digimon'], ['items', 'Items'], ['augments', 'Augments'], ['wisps', 'Data Wisps'], ['rules', 'Rules']];
// tabs with content in the active set (no Wisps tab in sets without Data Wisps)
const tabsNow = () => ALL_TABS.filter(([k]) => (k === 'wisps' ? (DT.WISPS || []).length : k === 'augments' ? (DT.AUGMENTS || []).length : true));
const TABS = ALL_TABS;
const fv = { open: false, tab: 'traits', q: '', kind: 'all', cost: 'all', modal: null };
let full = null;

function buildFull() {
  if (full) return full;
  full = document.createElement('div');
  full.id = 'cxFull';
  full.className = 'hidden';
  full.setAttribute('role', 'dialog');
  full.setAttribute('aria-label', 'Codex');
  full.innerHTML = `<div class="cx-win">
      <header class="cx-head"><h2>Codex</h2><nav class="cx-tabs"></nav>
        <input class="cx-search" type="search" placeholder="Search" aria-label="Search the codex" spellcheck="false">
        <button class="cx-close" title="Esc">Close</button></header>
      <nav class="cx-sets" role="tablist" aria-label="Set"></nav>
      <div class="cx-filters"></div>
      <div class="cx-body"></div>
    </div>
    <div class="cx-modal hidden"></div>`;
  document.body.appendChild(full);
  full.addEventListener('click', onFullClick);
  const inp = full.querySelector('.cx-search');
  inp.addEventListener('input', () => { fv.q = inp.value; renderFull(); });
  return full;
}

const norm = (s) => String(s || '').toLowerCase();
function matches(q, ...fields) {
  if (!q) return true;
  const hay = norm(fields.flat().join(' '));
  return norm(q).split(/\s+/).filter(Boolean).every((w) => hay.includes(w));
}
const traitHay = (id) => { const t = trait(id); return [t.name, id, t.kind, t.T.desc, notesOf(t.T), t.bp.map((_, i) => bpText(t.T, i)), linesWith(id).map((l) => l.forms)]; };
const lineHay = (l) => [l.id, l.forms, l.abil, l.attr, l.type, (l.traits || []).map((t) => trait(t).name), l.shop ? `${l.tier} gold cost` : 'wild'];
const itemHay = (it) => [it.name, it.id, it.desc, DT.statText ? DT.statText(it.stats || {}) : '', (it.from || []).map((f) => (DT.ITEMS[f] || {}).name), it.emblem ? trait(it.emblem).name : ''];

function tabCount(tab) {
  const q = fv.q;
  switch (tab) {
    case 'traits': return allTraitIds().filter((id) => matches(q, traitHay(id))).length;
    case 'digimon': return DT.LINES.filter((l) => matches(q, lineHay(l))).length;
    case 'items': return Object.values(DT.ITEMS).filter((it) => matches(q, itemHay(it))).length;
    case 'augments': return DT.AUGMENTS.filter((a) => matches(q, a.name, a.desc, DT.AUG_TIER_NAME[a.tier])).length;
    case 'wisps': return DT.WISPS.filter((w) => matches(q, w.name, w.desc, w.kind)).length;
    default: return '';
  }
}

// ---------- set switcher ----------
// The browsed set is activated only while the codex renders (it reads DT.TRAITS, DT.LINES, ...), then the active set is
// restored. A running game keeps its own set: no switching then.
const SET_ART = { 'faction-wars': 'mode-faction-wars', dragonlands: 'mode-dragonlands', autochess: 'mode-autochess' };
const browsable = () => (DT.Sets && DT.Sets.pickable ? DT.Sets.pickable() : []);
function gameRunning() {
  const g = game();
  return !!(g && !g.over) || !!(UI().inGame && UI().inGame());
}
function browseId() {
  if (!DT.Sets) return null;
  if (gameRunning()) return DT.SET_ID;
  const ids = browsable().map((o) => o.id);
  if (!fv.set || !ids.includes(fv.set)) fv.set = ids.includes(DT.SET_ID) ? DT.SET_ID : ids[0] || DT.SET_ID;
  return fv.set;
}
function withSet(fn) {
  const id = browseId();
  const prev = DT.Sets ? DT.Sets.activeId : null;
  if (!id || id === prev) return fn();
  DT.Sets.activate(id);
  try { return fn(); } finally { DT.Sets.activate(prev); }
}
function renderSets() {
  const nav = full.querySelector('.cx-sets');
  const opts = browsable();
  const live = gameRunning();
  const cur = browseId();
  nav.classList.toggle('hidden', opts.length < 2);
  nav.innerHTML = opts.map((o) => {
    let n = 0;
    try { n = Object.keys(DT.Sets.get(o.id).TRAITS || {}).length; } catch (e) { n = 0; }
    const lock = live && o.id !== cur;
    return `<button class="cx-set${o.id === cur ? ' on' : ''}" data-cx-set="${esc(o.id)}" role="tab" aria-selected="${o.id === cur}" ${lock ? 'disabled title="A game is running: the codex shows its set"' : `title="${esc(o.desc)}"`}
      style="--art:url('assets/client/${SET_ART[o.id] || 'mode-multiplayer'}.webp')" data-traits="${n}">${o.id === DT.SET_ID && live ? '<span class="cx-live">In game</span>' : ''}<b>${esc(o.name)}</b><small>${n} traits · ${esc(DT.SetPicker ? DT.SetPicker.modeName(o.mode) : o.mode)}</small></button>`;
  }).join('');
}
function browseSet(id) {
  if (!DT.Sets || !DT.Sets.has(id) || gameRunning()) return false;
  fv.set = id;
  fv.kind = 'all';
  fv.cost = 'all';
  if (full) { full.querySelector('.cx-body').scrollTop = 0; renderFull(); }
  return true;
}

function renderFull() {
  if (!full || !fv.open) return;
  renderSets();
  withSet(renderFullNow);
}
function renderFullNow() {
  if (!tabsNow().some(([k]) => k === fv.tab)) fv.tab = 'traits';
  const h2 = full.querySelector('.cx-head h2');
  if (h2) h2.textContent = DT.SET && DT.Sets && DT.Sets.ids().filter((id) => !DT.Sets.def(id).hidden).length > 1 ? `Codex · ${DT.SET.name}` : 'Codex';
  full.querySelector('.cx-tabs').innerHTML = tabsNow().map(([k, name]) => {
    const n = tabCount(k);
    return `<button data-tab="${k}" class="${k === fv.tab ? 'on' : ''}">${name}${n !== '' ? ` <span>${n}</span>` : ''}</button>`;
  }).join('');
  const filters = full.querySelector('.cx-filters');
  const body = full.querySelector('.cx-body');
  let fhtml = '';
  let html = '';
  const q = fv.q;
  if (fv.tab === 'traits') {
    const ks = kinds();
    const kname = (k) => (k === 'family' ? 'Families' : k === 'role' ? 'Roles' : cap(k));
    fhtml = `<div class="cx-seg">${[['all', 'All']].concat(ks.map((k) => [k, kname(k)]))
      .map(([k, n]) => `<button data-kind="${esc(k)}" class="${fv.kind === k ? 'on' : ''}">${esc(n)}</button>`).join('')}</div>`;
    const ids = sortTraits(allTraitIds()).filter((id) => (fv.kind === 'all' || trait(id).kind === fv.kind) && matches(q, traitHay(id)));
    html = ids.length ? `<div class="cx-tgrid">${ids.map((id) => traitHTML(id, null)).join('')}</div>` : emptyHTML();
  } else if (fv.tab === 'digimon') {
    const tiers = [...new Set(DT.SHOP_LINES.map((l) => l.tier))].sort((a, b) => a - b);
    fhtml = `<div class="cx-seg">${[['all', 'All']].concat(tiers.map((t) => [String(t), `${t} gold`])).concat(DT.LINES.some((l) => !l.shop) ? [['wild', 'Wild']] : [])
      .map(([k, n]) => `<button data-cost="${k}" class="${fv.cost === k ? 'on' : ''}"${/^\d+$/.test(k) ? ` style="--tc:${tierColor(Number(k))}"` : ''}>${n}</button>`).join('')}</div>`;
    const list = DT.LINES.filter((l) => matches(q, lineHay(l)) && (fv.cost === 'all' || (fv.cost === 'wild' ? !l.shop : l.shop && String(l.tier) === fv.cost)));
    const groups = tiers.map((t) => [`${t} gold`, tierColor(t), list.filter((l) => l.shop && l.tier === t)]).concat([['Wild Digimon (PvE only)', '#8e98ad', list.filter((l) => !l.shop)]]);
    html = list.length ? groups.filter((g) => g[2].length).map(([name, col, ls]) => `<section class="cx-group"><h3 style="color:${col}">${esc(name)} <span class="muted">${ls.length}</span></h3>
      <div class="cx-lgrid">${ls.map(lineCard).join('')}</div></section>`).join('') : emptyHTML();
  } else if (fv.tab === 'items') {
    html = itemsHTML(q);
  } else if (fv.tab === 'augments') {
    const groups = [1, 2, 3].concat([...new Set(DT.AUGMENTS.map((a) => a.tier))].filter((t) => ![1, 2, 3].includes(t)));
    const any = DT.AUGMENTS.filter((a) => matches(q, a.name, a.desc, DT.AUG_TIER_NAME[a.tier]));
    html = any.length ? groups.map((t) => {
      const ls = any.filter((a) => a.tier === t);
      if (!ls.length) return '';
      const col = DT.AUG_TIER_COLOR[t] || '#c9d1dc';
      return `<section class="cx-group"><h3 style="color:${col}">${esc(DT.AUG_TIER_NAME[t] || `Tier ${t}`)} <span class="muted">${ls.length}</span></h3>
        <div class="cx-cgrid">${ls.map((a) => `<div class="cx-card" data-cx-aug="${esc(a.id)}" style="--tc:${col}"><div class="cx-ch"><i class="cx-gem"></i><b>${esc(a.name)}</b></div><div class="cx-desc">${esc(a.desc)}</div></div>`).join('')}</div></section>`;
    }).join('') : emptyHTML();
  } else if (fv.tab === 'wisps') {
    const ws = DT.WISPS.filter((w) => matches(q, w.name, w.desc, w.kind));
    const ks = [...new Set(DT.WISPS.map((w) => w.kind))];
    html = `<p class="cx-intro">A one-use Wisp appears in the sixth shop slot on even rounds from stage 2. Its price is its base cost plus the stage number. Unbought Wisps leave at the end of the round.</p>` +
      (ws.length ? ks.map((k) => {
        const ls = ws.filter((w) => w.kind === k);
        if (!ls.length) return '';
        return `<section class="cx-group"><h3>${esc(k)} <span class="muted">${ls.length}</span></h3><div class="cx-cgrid">${ls.map((w) => `<div class="cx-card wisp" data-cx-wisp="${esc(w.id)}">
          <div class="cx-ch"><i class="cx-wisp"></i><b>${esc(w.name)}</b><span class="cx-cost">${w.base ? `${w.base} + stage` : 'stage'} gold</span></div>
          <div class="cx-desc">${esc(w.desc)}</div><div class="cx-meta">From stage ${w.minStage}. At stage ${Math.max(2, w.minStage)}: ${DT.wispCost(w, Math.max(2, w.minStage))} gold.</div></div>`).join('')}</div></section>`;
      }).join('') : emptyHTML());
  } else if (fv.tab === 'rules') {
    const secs = rulesSections().filter((s) => matches(q, s.title, s.html.replace(/<[^>]+>/g, ' ')));
    html = secs.length ? `<div class="cx-rules">${secs.map((s) => `<section class="cx-rule"><h3>${esc(s.title)}</h3>${s.html}</section>`).join('')}</div>` : emptyHTML();
  }
  filters.innerHTML = fhtml;
  filters.classList.toggle('hidden', !fhtml);
  body.innerHTML = html;
}

const emptyHTML = () => `<div class="cx-empty">Nothing matches "${esc(fv.q)}".</div>`;

function lineCard(l) {
  const stages = [0, 1, 2].filter((s) => l.forms[s] != null);
  const same = l.forms.every((f) => f === l.forms[0]);
  const s0 = DT.unitStats(l, 0);
  const A = DT.ATTR[l.attr] || { color: '#c8c8c8' };
  return `<div class="cx-line${l.shop ? '' : ' wild'}" data-cx-line="${esc(l.id)}" style="--tc:${tierColor(l.tier)}" role="button" tabindex="0">
    <div class="cx-lh"><b>${esc(l.forms[0])}</b><span class="cx-pill cost">${l.shop ? `${l.tier}g` : 'wild'}</span><span class="cx-pill" style="color:${A.color};border-color:${A.color}66">${esc(l.attr)}</span></div>
    <div class="cx-forms">${(same ? [0] : stages).map((s) => `<div class="cx-lf">${portrait(l.id, s, `t${l.tier}`)}<div><b>${esc(l.forms[s])}</b><span>${esc((l.abil || [])[s] || '')}</span></div></div>`).join('')}</div>
    <div class="cx-tchips">${(l.traits || []).map((id) => `<span class="cx-tchip" style="--tc:${trait(id).color}">${badge(id, 'xs')}${esc(trait(id).name)}</span>`).join('')}</div>
    <div class="cx-abil small">${esc(DT.abilityText(l, 0))}</div>
    <div class="cx-lstats"><span>HP <b>${stages.map((s) => DT.unitStats(l, s).maxHp).join('/')}</b></span><span>AD <b>${stages.map((s) => DT.unitStats(l, s).ad).join('/')}</b></span><span>AS <b>${s0.as}</b></span><span>Range <b>${s0.range}</b></span></div>
  </div>`;
}

// where the active set's drop-only Crests come from: items.dropEmblemSource, else the Armory (when this pace has one) and loot
function dropSource() {
  const src = DT.SET && DT.SET.def && DT.SET.def.items && DT.SET.def.items.dropEmblemSource;
  if (src) return src;
  const R = DT.HYPER || {};
  const armory = (R.armoryRounds && R.armoryRounds.length) || (R.armoryStages && R.armoryStages.length) || (R.offers || []).some((o) => o.kind === 'armory');
  return armory ? 'the Armory and loot' : 'loot';
}

function itemCard(it, extra = '') {
  const st = DT.statText && it.stats ? DT.statText(it.stats) : '';
  const pairs = it.from && !it.component ? (DT.recipesOf ? DT.recipesOf(it.id) : [it.from]) : [];
  const recipe = (r) => `<div class="cx-recipe">${r.map((f) => itemImg(f, 'xs')).join('<i>+</i>')}<span>${r.map((f) => esc((DT.ITEMS[f] || {}).name || f)).join(' + ')}</span></div>`;
  const rec = pairs.length ? pairs.map(recipe).join('') : it.drop ? `<div class="cx-recipe"><span>No recipe: found in ${esc(dropSource())}</span></div>` : '';
  return `<div class="cx-card item" data-cx-item="${esc(it.id)}" id="cx-item-${esc(it.id)}">
    <div class="cx-ch">${itemImg(it.id)}<div><b>${esc(it.name)}</b>${st && !it.component ? `<div class="cx-stat">${esc(st)}</div>` : ''}</div></div>
    <div class="cx-desc">${esc(it.desc || '')}</div>${rec}${extra}</div>`;
}

function itemsHTML(q) {
  const items = Object.values(DT.ITEMS);
  const comps = DT.COMPONENT_IDS.map((id) => DT.ITEMS[id]).filter((it) => it && matches(q, itemHay(it)));
  const core = DT.CORE_ITEMS.map((id) => DT.ITEMS[id]).filter((it) => it && matches(q, itemHay(it)));
  const crests = items.filter((it) => it.emblem && matches(q, itemHay(it)));
  const coreSet = new Set(DT.CORE_ITEMS);
  const special = items.filter((it) => !it.component && !it.emblem && !coreSet.has(it.id) && matches(q, itemHay(it)));
  if (!comps.length && !core.length && !crests.length && !special.length) return emptyHTML();
  const ids = DT.COMPONENT_IDS;
  const hit = new Set([...core, ...crests, ...special].map((it) => it.id));
  const grid = `<section class="cx-group"><h3>Recipe grid <span class="muted">two components make one item</span></h3>
    <div class="cx-gridwrap"><table class="cx-rgrid"><tr><th></th>${ids.map((c) => `<th>${itemImg(c, 'sm')}</th>`).join('')}</tr>
    ${ids.map((a) => `<tr><th>${itemImg(a, 'sm')}</th>${ids.map((b) => {
      const r = DT.combineItems(a, b);
      if (!r) return '<td></td>';
      const it = DT.ITEMS[r];
      return `<td class="${q && !hit.has(r) ? 'dim' : ''}"><button data-goitem="${esc(r)}" title="${esc(it.name)}: ${esc(DT.itemText ? DT.itemText(r) : it.desc)}">${itemImg(r, 'sm')}</button></td>`;
    }).join('')}</tr>`).join('')}</table></div></section>`;
  const sec = (title, sub, list) => (list.length ? `<section class="cx-group"><h3>${title} <span class="muted">${sub}</span></h3><div class="cx-cgrid">${list.map((it) => itemCard(it)).join('')}</div></section>` : '');
  return sec('Components', `${comps.length}`, comps) + grid + sec('Completed items', `${core.length}`, core)
    + sec('Crests', 'Digi-Egg + a component: the holder gains a trait', crests) + sec('Special', `${special.length}`, special);
}

function showModal(html) {
  const m = full.querySelector('.cx-modal');
  m.innerHTML = `<div class="cx-mbox"><button class="cx-mclose" data-mclose="1" title="Esc">✕</button>${html}</div>`;
  m.classList.remove('hidden');
  fv.modal = true;
}
function closeModal() {
  if (!full) return;
  const m = full.querySelector('.cx-modal');
  m.classList.add('hidden');
  m.innerHTML = '';
  fv.modal = null;
}

function onFullClick(e) {
  if (e.target.classList.contains('cx-modal')) return closeModal();
  const t = e.target.closest('button, [data-cx-line]');
  if (!t) return;
  const d = t.dataset;
  if (t.classList.contains('cx-close')) return closeFull();
  if (d.mclose) return closeModal();
  if (d.tab) { fv.tab = d.tab; full.querySelector('.cx-body').scrollTop = 0; return renderFull(); }
  if (d.kind) { fv.kind = d.kind; return renderFull(); }
  if (d.cost) { fv.cost = d.cost; return renderFull(); }
  if (d.cxSet) { if (DT.Sfx) DT.Sfx.play('pick'); return browseSet(d.cxSet); }
  if (d.cxLine) return showModal(withSet(() => unitHTML(d.cxLine, fv.ctx ? gameCtx() : null)));
  if (d.cxGoto) {
    closeModal();
    fv.tab = 'traits'; fv.kind = 'all'; fv.q = '';
    full.querySelector('.cx-search').value = '';
    renderFull();
    const el = full.querySelector(`[data-cx-tid="${CSS.escape(d.cxGoto)}"]`);
    if (el) { el.scrollIntoView({ block: 'center' }); el.classList.add('flash'); setTimeout(() => el.classList.remove('flash'), 1200); }
    return;
  }
  if (d.goitem) {
    const el = full.querySelector(`#cx-item-${CSS.escape(d.goitem)}`);
    if (el) { el.scrollIntoView({ block: 'center', behavior: 'smooth' }); el.classList.add('flash'); setTimeout(() => el.classList.remove('flash'), 1200); }
  }
}

function openFull(tab, q, setId) {
  buildFull();
  if (setId && DT.Sets && DT.Sets.has(setId) && !gameRunning()) fv.set = setId;
  else if (!fv.open) fv.set = null; // opens on the active set
  if (tab && TABS.some(([k]) => k === tab)) fv.tab = tab;
  if (q != null) { fv.q = String(q); full.querySelector('.cx-search').value = fv.q; }
  fv.ctx = !!(gameCtx() && $('overlay') && $('overlay').classList.contains('hidden'));
  fv.open = true;
  full.classList.remove('hidden');
  closeModal();
  full.querySelector('.cx-body').scrollTop = 0;
  renderFull();
  syncBtn();
}
function closeFull() {
  if (!full || !fv.open) return;
  fv.open = false;
  closeModal();
  full.classList.add('hidden');
  full.querySelector('.cx-body').innerHTML = '';
  syncBtn();
}

// =====================================================================
// wiring
// =====================================================================
const titleUp = () => { const o = $('overlay'); return !!(o && !o.classList.contains('hidden')); };
const busyElsewhere = () => (DT.Shop && DT.Shop.isOpen && DT.Shop.isOpen()) || ($('picker') && !$('picker').classList.contains('hidden'));

function toggle() {
  if (fv.open) return closeFull();
  if (titleUp() || !gameCtx()) return openFull();
  if (pv.open) closePanel(); else openPanel();
}

function close() { closeFull(); closePanel(); }

// a Codex button in the top bar, added at runtime so index.html stays as other code expects
function addTopButton() {
  if ($('codexBtn')) return;
  const bar = document.querySelector('#topbar .tbtns');
  if (!bar) return;
  const b = document.createElement('button');
  b.id = 'codexBtn';
  b.title = `Codex: traits, Digimon and rules (${KEY.toUpperCase()})`;
  b.textContent = 'Codex';
  const before = $('emoteBtn');
  bar.insertBefore(b, before && before.parentNode === bar ? before : bar.firstChild);
}
function syncBtn() {
  const b = $('codexBtn');
  if (b) b.classList.toggle('on', pv.open);
}

document.addEventListener('click', (e) => {
  if (!e.target.closest) return;
  const open = e.target.closest('[data-codex-open], #codexBtn');
  if (open) {
    e.preventDefault();
    if (open.id === 'codexBtn') return toggle();
    return openFull(open.getAttribute('data-codex-open') || undefined);
  }
  // clicking a trait in the HUD trait list opens the panel at that trait
  const tr = e.target.closest('#traitList [data-trait]');
  if (tr && DT.TRAITS[tr.dataset.trait]) {
    openPanel(tr.dataset.trait);
    const tip = $('tooltip'); // the panel now covers the row the tooltip points at
    if (tip) tip.classList.add('hidden');
  }
});

window.addEventListener('keydown', (e) => {
  if (e.key !== 'Escape') return;
  if (fv.open) {
    e.stopImmediatePropagation();
    e.preventDefault();
    const inp = full.querySelector('.cx-search');
    if (fv.modal) closeModal();
    else if (e.target === inp && inp.value) { inp.value = ''; fv.q = ''; renderFull(); } // first Escape clears the search
    else closeFull();
  } else if (pv.open && !busyElsewhere()) {
    if (pv.unit) { pv.unit = null; renderPanel(true); } else closePanel();
  }
}, true);

window.addEventListener('keydown', (e) => {
  if (e.key.toLowerCase() !== KEY || e.ctrlKey || e.metaKey || e.altKey || e.repeat) return;
  if (e.target && /input|textarea|select/i.test(e.target.tagName)) return;
  if (busyElsewhere()) return;
  e.preventDefault();
  toggle();
});

window.addEventListener('resize', placePanel);
setInterval(() => {
  if (!pv.open) return;
  // the panel belongs to a running game: hide it under the title / game-over screen
  placePanel();
  try { renderPanel(false); } catch (err) { /* game mid-swap */ }
}, 400);

addTopButton();
document.addEventListener('DOMContentLoaded', addTopButton);

DT.Codex = { toggle, openPanel, closePanel, openFull, closeFull, close, browseSet, browsing: () => (full && fv.open ? browseId() : null), isOpen: () => pv.open || fv.open, panelOpen: () => pv.open, fullOpen: () => fv.open,
  traitIds: allTraitIds, bpText: (id, i) => bpText(trait(id).T, i), key: KEY };
