// Cosmetic drops are local profile rewards, outside the deterministic combat simulation.
const DT = window.DT, C = DT.Cosmetics, KEY = 'dt.mysteryLoot.v1';
const fresh = () => ({ daily: 0, vault: 0, keys: 0, day: '', dayProgress: 0, games: {}, boxMisses: 0, keyMisses: 0, pending: null });
let save = fresh();
try { const s = JSON.parse(localStorage.getItem(KEY)); if (s && typeof s === 'object') save = { ...save, ...s }; } catch (_) { /* fresh profile */ }
for (const k of ['daily','vault','keys','boxMisses','keyMisses']) if (!Number.isSafeInteger(save[k]) || save[k] < 0) save[k] = 0;
if (save.dayProgress !== 1) save.dayProgress = 0;
if (!save.games || typeof save.games !== 'object' || Array.isArray(save.games)) save.games = {};
const listeners = new Set();
function persist(next) { try { localStorage.setItem(KEY, JSON.stringify(next)); save = next; return true; } catch (_) { DT.Client?.toast('Could not save loot. Free some storage and try again.', 'bad'); return false; } }
function emit() { C.notify('loot'); for (const fn of listeners) fn(); }
const roll = () => { const a = new Uint32Array(1); crypto.getRandomValues(a); return a[0] / 4294967296; };
function settle() {
  const p = save.pending;
  if (!p || !C.get(p.kind, p.id)) return;
  if (!C.grant(p.kind, p.id)) return; // idempotent, recovers an interrupted reveal on startup
  persist({ ...save, pending: null });
}
function roundEnd(g) {
  const me = g?.human;
  // Only the live end-of-round UI calls this. Replays/reconnect catch-up never award drops.
  if (!me || !g.results?.some(r => r.match && (r.match.a===me || r.match.a2===me || (!r.match.ghost && (r.match.b===me || r.match.b2===me))))) return [];
  const key = `${g.setId}:${g.duo?'duos':'solo'}:${g.seed}:${me.id}`, round = g.stage * 100 + g.round;
  if (!Number.isFinite(round) || round <= (save.games[key] || 0)) return [];
  const next = structuredClone(save), drops = [];
  next.games[key] = round;
  // Keep a generous bounded ledger. This is local progress, not a server-verified economy.
  const ids = Object.keys(next.games); for (const old of ids.slice(0, Math.max(0,ids.length-1000))) delete next.games[old];
  const date = new Date(), day = `${date.getFullYear()}-${String(date.getMonth()+1).padStart(2,'0')}-${String(date.getDate()).padStart(2,'0')}`;
  // Count active days, not elapsed calendar days. Breaks retain progress; old saves
  // start at zero and their last counted day prevents a same-day retroactive drop.
  if (day > next.day) {
    next.day = day;
    if (++next.dayProgress >= 2) { next.dayProgress = 0; next.daily++; drops.push('Daybreak Box'); }
  }
  // Stage 2 onward: independent 3% box / 2.5% key drops; doubled pity preserves the lower earning cadence.
  if (g.stage >= 2) {
    if (++next.boxMisses >= 32 || roll() < .03) { next.vault++; next.boxMisses=0; drops.push('Cipher Box'); }
    if (++next.keyMisses >= 40 || roll() < .025) { next.keys++; next.keyMisses=0; drops.push('Cipher Key'); }
  }
  if (!persist(next)) return [];
  emit();
  if (drops.length) DT.LootNotices?.earned({id:`${key}:${round}`,round:`${g.stage}-${g.round}`,drops});
  return drops;
}
function open(pool) {
  if (!['daily','vault'].includes(pool)) return null;
  settle();
  if (save.pending || save[pool] < 1 || (pool === 'vault' && save.keys < 1)) return null;
  const available = C.KINDS.flatMap(kind => C.list(kind).filter(it => it.lootPool === pool && !C.owns(kind,it.id)).map(it => ({kind,id:it.id})));
  // Never spend a box/key on a duplicate. Keep the box when the full pool is owned.
  if (!available.length) { DT.Client?.toast('You have discovered every reward in this box. Your box is saved.', 'info'); return null; }
  const reward = available[Math.floor(roll()*available.length)];
  const next = { ...save, [pool]:save[pool]-1, keys:save.keys-(pool==='vault'?1:0), pending:reward };
  if (!persist(next)) return null;
  settle(); emit(); return C.owns(reward.kind,reward.id) ? reward : null;
}
let dialog = null;
function reveal(reward) {
  if (!reward) return;
  if (dialog) dialog.close();
  const item=C.get(reward.kind,reward.id);if(!item)return;
  const el=document.createElement('dialog');let dispose=null;dialog=el;dialog.className='mystery-reveal';
  dialog.innerHTML='<div class="eyebrow">New discovery</div><div class="mystery-preview"></div><h2></h2><p></p><div class="mystery-actions"><button class="btn lc-cta" data-equip>Equip</button><button class="btn btn-secondary" data-close>Keep in collection</button></div>';
  dialog.querySelector('h2').textContent=item.name;dialog.querySelector('p').textContent=item.desc||'';
  document.body.appendChild(dialog);dialog.showModal();DT.Sfx?.play('pick');
  const preview=dialog.querySelector('.mystery-preview');
  if(item.preview)dispose=item.preview(preview);else preview.textContent=item.icon||'✦';
  el.querySelector('[data-close]').onclick=()=>el.close();
  el.querySelector('[data-equip]').onclick=()=>{C.equip(reward.kind,reward.id);DT.Client?.toast(`Equipped ${item.name}`,'good');el.close();};
  el.addEventListener('close',()=>{if(typeof dispose==='function')dispose();dispose=null;el.remove();if(dialog===el)dialog=null;},{once:true});
}
DT.MysteryLoot={ roundEnd,open,reveal, inventory:()=>({daily:save.daily,vault:save.vault,keys:save.keys,day:save.day,dayProgress:save.dayProgress}),onChange:fn=>{listeners.add(fn);return()=>listeners.delete(fn);} };
settle();
