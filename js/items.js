// Item engine: builds a set's item table (TFT-style components and recipes, emblems, drop-only items, consumables
// and any custom items) and the helpers the rules and UI use. No DOM access. Item content lives in js/sets/<id>.js.
//
// Item fields (after DT.buildItems): id, name, desc, cat, stats, p (passives read by combat.js), hooks (combat/game
// hooks while held), from ([a, b] recipe or null), color, color2, short, tags (AI: 'ad' | 'ap' | 'tank' | 'carry' |
// 'util' ...), tier (tiered systems), emblem (trait id), component / consumable flags, use(game ctx, unit, idx).
// Categories: component, completed, emblem, radiant, artifact, support, shadow, consumable, global, special, or any
// set-specific name.
(function (G) {
  const DT = G.DT;

  // stat keys: ad (fraction of base AD), as (fraction), ap (flat), mana (flat start), armor, mr, hp (flat), crit (fraction),
  // plus anything a set adds to DT.STAT_LABEL (combat.js applies the known keys; others are for hooks to read)
  DT.STAT_LABEL = {
    ad: (v) => `+${Math.round(v * 100)}% AD`, as: (v) => `+${Math.round(v * 100)}% AS`, ap: (v) => `+${v} AP`,
    mana: (v) => `+${v} mana`, armor: (v) => `+${v} armor`, mr: (v) => `+${v} MR`, hp: (v) => `+${v} HP`, crit: (v) => `+${Math.round(v * 100)}% crit`,
    dodge: (v) => `+${Math.round(v * 100)}% dodge`, omnivamp: (v) => `+${Math.round(v * 100)}% omnivamp`,
  };
  DT.statText = (stats) => Object.entries(stats || {}).map(([k, v]) => (DT.STAT_LABEL[k] ? DT.STAT_LABEL[k](v) : `+${v} ${k}`)).join(', ');

  const recipeKey = (a, b) => [a, b].sort().join('+');
  const slug = (name) => name.toLowerCase().replace(/[^a-z]+/g, '_');
  const initials = (name) => name.split(' ').map((w) => w[0]).join('');

  // def: the set's `items` block (see docs/sets/FORMAT.md). traits: the set's TRAITS. patch: { itemId: fields } from
  // DT.Sets.extend. Returns the item globals for the set.
  DT.buildItems = function (def, traits, patch = {}, setId = '') {
    const ITEMS = {};
    const RECIPE = {};
    const COMPONENTS = def.components || {};
    const COMPONENT_IDS = Object.keys(COMPONENTS);
    const SPECIAL_COMPONENTS = COMPONENT_IDS.filter((c) => COMPONENTS[c].special);
    const STAT_COMPONENTS = COMPONENT_IDS.filter((c) => !COMPONENTS[c].special);
    const emblemName = def.emblemName || ((T) => `${T.name} Emblem`);
    const emblemColor2 = def.emblemColor2 || '#fff3b0';
    const emblemDesc = def.emblemDesc || ((T) => `The holder gains the ${T.name} trait.`);
    // emblem ids: '<emblemIdPrefix>emblem_<trait>' (prefix default '', so 'emblem_<trait>'); a set prefix keeps ids
    // unique when two sets share a trait id
    const emblemId = (trait) => `${def.emblemIdPrefix || ''}emblem_${trait.toLowerCase()}`;
    // extra recipes for one item: recipes: [[a, b], ...] (the first pair is the item's `from` unless it has one)
    const addRecipes = (id, list) => { for (const pair of list || []) if (Array.isArray(pair) && pair.length === 2) RECIPE[recipeKey(pair[0], pair[1])] = id; };
    const add = (it) => {
      if (ITEMS[it.id] && typeof console !== 'undefined') console.warn(`Digi-Tactics items (${setId}): duplicate item id ${it.id}`);
      ITEMS[it.id] = it;
      return it;
    };
    const stat = (...list) => { const out = {}; for (const s of list) for (const k in s || {}) out[k] = (out[k] || 0) + s[k]; return out; };

    // 1. completed items from two components
    const core = [];
    for (const raw of def.recipes || []) {
      const r = Array.isArray(raw) ? { a: raw[0], b: raw[1], name: raw[2], desc: raw[3], p: raw[4], extra: raw[5], ...(raw[6] || {}) } : raw;
      const id = r.id || slug(r.name);
      const A = COMPONENTS[r.a], B = COMPONENTS[r.b];
      if (!A || !B) { console.warn(`Digi-Tactics items (${setId}): recipe ${id} uses an unknown component`); continue; }
      add({ id, name: r.name, desc: r.desc, p: r.p || {}, stats: stat(A.stats, B.stats, r.extra || {}), from: [r.a, r.b], color: r.color || A.color, color2: r.color2 || B.color,
        short: r.short || initials(r.name), cat: r.cat || 'completed', ...(r.tags ? { tags: r.tags } : {}), ...(r.hooks ? { hooks: r.hooks } : {}), ...(r.aiRank != null ? { aiRank: r.aiRank } : {}) });
      RECIPE[recipeKey(r.a, r.b)] = id;
      if (r.recipes) { ITEMS[id].recipes = r.recipes; addRecipes(id, r.recipes); }
      if ((r.cat || 'completed') === 'completed' && !r.noDrop) core.push(id);
    }
    // 2. emblems: emblemFrom (a special component) + component = emblem of a trait
    const emblemFrom = def.emblemFrom || SPECIAL_COMPONENTS[0];
    for (const [c, trait] of Object.entries(def.emblems || {})) {
      const T = traits[trait];
      if (!T || !COMPONENTS[c]) { console.warn(`Digi-Tactics items (${setId}): bad emblem ${c} -> ${trait}`); continue; }
      const id = emblemId(trait);
      add({ id, name: emblemName(T), desc: emblemDesc(T), p: { emblem: trait }, stats: { ...COMPONENTS[c].stats }, from: [emblemFrom, c], color: T.color, color2: emblemColor2,
        short: 'C' + T.name[0], emblem: trait, cat: 'emblem' });
      RECIPE[recipeKey(emblemFrom, c)] = id;
    }
    // 3. emblems with no recipe (Armory, loot, mechanics)
    const DROP_EMBLEMS = (def.dropEmblems || []).slice();
    for (const trait of DROP_EMBLEMS) {
      const T = traits[trait];
      if (!T) { console.warn(`Digi-Tactics items (${setId}): drop emblem for unknown trait ${trait}`); continue; }
      const id = emblemId(trait);
      add({ id, name: emblemName(T), desc: emblemDesc(T), p: { emblem: trait }, stats: {}, from: null, drop: true, color: T.color, color2: emblemColor2,
        short: 'C' + T.name[0], emblem: trait, cat: 'emblem' });
    }
    const EMBLEM_IDS = Object.keys(ITEMS).filter((k) => ITEMS[k].emblem);
    // trait id -> its emblem's item id
    const EMBLEM_OF = {};
    for (const k of EMBLEM_IDS) EMBLEM_OF[ITEMS[k].emblem] = k;
    // 4. special recipes (e.g. two special components)
    for (const it of def.special || []) {
      add({ p: {}, stats: {}, cat: 'special', ...it });
      if (it.from) RECIPE[recipeKey(it.from[0], it.from[1])] = it.id;
    }
    // 5. components
    for (const c of COMPONENT_IDS) {
      const C = COMPONENTS[c];
      add({ id: c, component: true, ...C, desc: C.desc || DT.statText(C.stats), p: {}, cat: 'component' });
    }
    // 6. anything else: consumables, radiant, artifacts, support items, tiered items, globals...
    const listed = [];
    for (const it of def.list || def.extra || []) {
      const x = { p: {}, stats: {}, from: null, cat: it.consumable ? 'consumable' : 'completed', short: initials(it.name || it.id), color: '#9aa4b1', color2: '#5a6170', ...it };
      if (!x.from && Array.isArray(x.recipes) && x.recipes.length && x.cat !== 'component') x.from = x.recipes[0].slice();
      if (x.cat === 'consumable') x.consumable = true;
      if (x.cat === 'component') x.component = true;
      add(x);
      listed.push(x.id);
      if (x.from && x.cat !== 'component') RECIPE[recipeKey(x.from[0], x.from[1])] = x.id;
      if (x.recipes && x.cat !== 'component') addRecipes(x.id, x.recipes);
    }
    // patches from DT.Sets.extend (hooks, use, tags, ai...)
    for (const [id, fields] of Object.entries(patch)) {
      if (ITEMS[id]) Object.assign(ITEMS[id], fields);
      else console.warn(`Digi-Tactics items (${setId}): extend for unknown item ${id}`);
    }
    for (const item of Object.values(ITEMS)) if (item.p && item.p.thief)
      item.desc = `${item.desc} Occupies all 3 item slots; cannot be equipped with other items.`;
    // completed items that random drops hand out (no emblems, specials, consumables)
    const CORE_ITEMS = def.core ? def.core.slice() : core.concat(listed.filter((id) => ITEMS[id].cat === 'completed' && !ITEMS[id].noDrop));
    const CONSUMABLE_IDS = Object.keys(ITEMS).filter((k) => ITEMS[k].consumable && !ITEMS[k].noDrop);
    // What the AI builds for each job, best first: explicit lists, else items tagged ap / ad / tank, best aiRank first
    const ITEM_ROLE = {};
    for (const job of ['ap', 'ad', 'tank']) {
      if (def.aiRoles && def.aiRoles[job]) { ITEM_ROLE[job] = def.aiRoles[job].slice(); continue; }
      ITEM_ROLE[job] = Object.values(ITEMS)
        .filter((it) => !it.component && !it.consumable && !it.emblem && (it.tags || []).includes(job))
        .map((it, i) => ({ id: it.id, r: it.aiRank || 0, i }))
        .sort((a, b) => b.r - a.r || a.i - b.i)
        .map((x) => x.id);
    }
    return { ITEMS, RECIPE, COMPONENTS, COMPONENT_IDS, STAT_COMPONENTS, SPECIAL_COMPONENTS, CORE_ITEMS, EMBLEM_IDS, EMBLEM_OF, DROP_EMBLEMS, CONSUMABLE_IDS, ITEM_ROLE };
  };

  DT.combineItems = (a, b) => DT.RECIPE[recipeKey(a, b)] || null;
  // every component pair that makes item id: its `from` pair plus any `recipes` pairs, without duplicates
  DT.recipesOf = (id) => {
    const it = DT.ITEMS[id];
    if (!it || it.component) return [];
    const out = [], seen = new Set();
    for (const pair of [it.from].concat(it.recipes || [])) {
      if (!pair) continue;
      const k = recipeKey(pair[0], pair[1]);
      if (!seen.has(k)) { seen.add(k); out.push(pair); }
    }
    return out;
  };
  // the active set's emblem for a trait (null if it has none)
  DT.emblemOf = (trait) => (DT.EMBLEM_OF && DT.EMBLEM_OF[trait]) || null;

  // Consumable drag previews. An item's preview(unit, player, game) returns a string or { text, ok, detail } for the
  // card shown while it is dragged over a Digimon; preview: 'remover' | 'reforger' uses one of these. Without a
  // preview the card shows the item's own description.
  const plural = (n) => `${n} item${n === 1 ? '' : 's'}`;
  const benchCheck = (u, p, text) => {
    const n = u.items.length;
    if (!n) return { text: 'No items to remove', ok: false };
    if (p && p.items.length - 1 + n > ((p.game && p.game.R && p.game.R.itemCap) || (DT.HYPER && DT.HYPER.itemCap) || 10)) return { text: 'Not enough room on the item bench', ok: false };
    return { text: text(n), ok: true, detail: u.items.map((x) => (DT.ITEMS[x] ? DT.ITEMS[x].name : x)).join(', ') };
  };
  DT.CONSUMABLE_PREVIEW = {
    remover: (u, p) => benchCheck(u, p, (n) => `Returns ${plural(n)} to your bench`),
    reforger: (u, p) => benchCheck(u, p, (n) => `Rerolls ${plural(n)} into new items of the same kind`),
  };
  // { text, ok, detail } for dragging consumable id over unit u
  DT.consumablePreview = (id, u, p, game) => {
    const it = DT.ITEMS[id];
    if (!it) return { text: '', ok: false };
    const fn = typeof it.preview === 'function' ? it.preview : DT.CONSUMABLE_PREVIEW[it.preview];
    if (fn) {
      let r = null;
      try { r = fn(u, p, game); } catch (e) { r = null; }
      if (typeof r === 'string') return { text: r, ok: true };
      if (r && typeof r === 'object') return { text: String(r.text || ''), ok: r.ok !== false, detail: r.detail || '' };
    }
    const d = String(it.desc || '').replace(/^Drag onto (?:a|any) Digimon to /i, '');
    return { text: d ? d[0].toUpperCase() + d.slice(1) : `Uses ${it.name}`, ok: true };
  };
  DT.isComponent = (id) => !!(DT.ITEMS[id] && DT.ITEMS[id].component);
  DT.isConsumable = (id) => !!(DT.ITEMS[id] && DT.ITEMS[id].consumable);
  // Random-item gloves reserve the two slots that their combat rolls will fill.
  DT.itemSlots = (id) => DT.ITEMS[id] && DT.ITEMS[id].p && DT.ITEMS[id].p.thief ? 3 : 1;
  DT.itemSlotsUsed = (items) => items.reduce((sum, id) => sum + DT.itemSlots(id), 0);
  DT.itemsFit = (held, id) => DT.itemSlotsUsed(held) + DT.itemSlots(id) <= 3;
  DT.fitItems = (items, overflow = () => {}) => {
    const kept = [];
    for (const id of items) { if (DT.itemsFit(kept, id)) kept.push(id); else overflow(id); }
    return kept;
  };
  DT.itemText = (id) => {
    const it = DT.ITEMS[id];
    if (!it) return '';
    const st = DT.statText(it.stats);
    return it.component || it.consumable ? it.desc : `${st ? st + '. ' : ''}${it.desc}`;
  };

  DT.AUG_TIER_NAME = ['', 'Silver', 'Gold', 'Prismatic'];
  DT.AUG_TIER_COLOR = ['', '#c9d1dc', '#ffcf4a', '#e58bff'];
  DT.wispCost = (w, stage) => w.base + stage;
})(typeof window !== 'undefined' ? window : globalThis);
