const assert = require('node:assert/strict');
const DT = require('../tools/headless.cjs').load({ strict: true });
require('node:vm').runInNewContext(require('node:fs').readFileSync(require('node:path').join(__dirname, '../js/itemcards.js'), 'utf8'), {
  window: { DT }, document: { getElementById: id => id === 'itemcardsCss' ? {} : null },
});
let checks = 0, gloves = 0;
const check = (condition, message) => { checks++; assert.ok(condition, message); };
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) for (const format of ['solo', 'duos']) {
  const g = new DT.Game({ seed: 143, setId: set.id, pace, format, allAI: true });
  const p = g.players[0], line = DT.SHOP_LINES.find(l => !l.noItems && l.ai.carry).id;
  const ordinary = DT.CORE_ITEMS.find(id => !DT.ITEMS[id].p.thief);
  if (set.id === 'underlords') {
    const item = Object.values(DT.ITEMS).find(it => !it.consumable && it.cat !== 'global').id;
    const holder = g.newUnit(line, 0); holder.items = [item]; p.bench[0] = holder;
    const preview = DT.ItemCards.unitCard(item, holder, g, p);
    check(preview.includes("Full: can't hold another item") && !preview.includes('Full: 3 items'), 'Underlords preview preserves its one-item limit');
  }
  for (const glove of Object.values(DT.ITEMS).filter(it => it.p.thief)) {
    gloves++;
    const context = `${set.id}/${pace}/${format}/${glove.id}`;
    const reset = (held, inventory) => {
      p.board = {}; p.bench.fill(null); p.items = inventory.slice();
      const u = g.newUnit(line, 0); u.items = held.slice(); p.bench[0] = u;
      return u;
    };
    let u = reset([glove.id], [ordinary, glove.id]);
    check(DT.ItemCards.unitCard(ordinary, u, g, p).includes('Gloves occupy all 3 item slots'), context + ' drag preview explains blocked slots');
    check(g.equipItem(p, 0, u.uid) === 'full', context + ' rejects another item after glove');
    check(g.equipItem(p, 1, u.uid) === 'full', context + ' rejects second glove');
    check(u.items.length === 1 && p.items.length === 2, context + ' rejection preserves inventory');
    check(!g.giveFromBench(p, ordinary, u), context + ' AI cannot add to glove');
    u = reset([ordinary], [glove.id]);
    check(g.equipItem(p, 0, u.uid) === 'full', context + ' glove needs empty unit');
    u = reset([], [glove.id]);
    check(DT.ItemCards.unitCard(glove.id, u, g, p).includes('Items after: 3/3'), context + ' preview reserves three slots');
    check(g.equipItem(p, 0, u.uid) === glove.id, context + ' glove equips alone');
    u = reset([glove.id], [ordinary]);
    u.stage = 2; p.bench[0] = null;
    const free = g.newUnit(line, 0);
    p.board = { '0,4': u, '1,4': free }; g.stage = 5;
    g.aiItems(p);
    check(free.items.includes(ordinary) && u.items.length === 1, context + ' AI selects free holder instead of glove holder');
    if (set.id === 'arcane') {
      const contraband = Object.values(DT.ITEMS).find(it => it.cat === 'contraband').id;
      u = reset([glove.id], [contraband]);
      u.line = DT.SHOP_LINES.find(l => l.traits.includes('SYN')).id;
      g.fireGame('onAiTurn', p, {});
      check(u.items.length === 1 && p.items.includes(contraband), context + ' contraband AI respects glove');
    }
    if (glove.from) {
      u = reset([glove.from[0], ordinary], [glove.from[1]]);
      check(g.equipItem(p, 0, u.uid) === 'full', context + ' cannot craft glove beside item');
      u = reset([glove.from[0]], [glove.from[1]]);
      check(g.equipItem(p, 0, u.uid) === glove.id, context + ' can craft glove as only item');
    }
    u = reset([ordinary, ordinary], [ordinary, ordinary]);
    check(g.equipItem(p, 0, u.uid) === ordinary && u.items.length === 3, context + ' normal third item fits');
    check(g.equipItem(p, 0, u.uid) === 'full' && p.items.length === 1, context + ' normal fourth item rejected');
    const remover = Object.values(DT.ITEMS).find(it => it.consumable && it.id.endsWith('remover'));
    if (remover) {
      u = reset([glove.id], [remover.id]);
      check(g.equipItem(p, 0, u.uid) === remover.id && u.items.length === 0 && p.items.includes(glove.id), context + ' remover still returns glove');
    }
    const reforger = Object.values(DT.ITEMS).find(it => it.consumable && it.id.endsWith('reforger'));
    if (reforger) {
      u = reset([ordinary, ordinary, ordinary], [reforger.id]);
      check(g.equipItem(p, 0, u.uid) === reforger.id && u.items.length === 0 && p.items.length === 3, context + ' reforger rolls onto bench, never beside worn items');
    }
    if (set.id === 'gizmos' && glove.cat === 'completed') {
      u = reset([glove.id], ['giz_ockit', ordinary]);
      check(g.equipItem(p, 0, u.uid) === 'giz_ockit' && u.items[0] === 'giz_oc_thief_glove', context + ' overclock upgrades glove');
      check(g.equipItem(p, 0, u.uid) === 'full' && p.items.length === 1, context + ' upgraded glove still occupies three slots');
    }
    const radiant = set.id === 'reckoning' && DT.Sets.def(set.id).items.radiantOf[glove.id];
    if (radiant) {
      u = reset([glove.id], [ordinary]);
      g.set.mechanic.offerKinds.rec_bless.choose(g.ctx(p), radiant);
      check(u.items[0] === radiant && g.equipItem(p, 0, u.uid) === 'full', context + ' radiant glove retains full slot cost');
    }
    for (const firstGlove of [true, false]) for (const fullInventory of [false, true]) {
      const existing = fullInventory ? g.R.itemCap : 0;
      reset([], Array(existing).fill(ordinary));
      const copyCount = g.copiesOf(line);
      for (let i = 0; i < copyCount; i++) {
        const copy = g.newUnit(line, 0);
        copy.items = i === 0 ? [firstGlove ? glove.id : ordinary] : i === 1 ? [firstGlove ? ordinary : glove.id] : [];
        p.bench[i] = copy;
      }
      const evolved = g.combine(p)[0];
      check(evolved.items.length === 1 && evolved.items[0] === (firstGlove ? glove.id : ordinary), context + ' merge preserves first eligible item');
      check(p.items.length === existing + 1 && p.items[existing] === (firstGlove ? ordinary : glove.id), context + ' merge returns incompatible item even with full bench');
      if (fullInventory) {
        g.giveItem(p, ordinary);
        check(p.items.length === existing + 1, context + ' new drops still respect inventory capacity');
      }
    }
    const side = ids => ({ units: [{ line, stage: 0, c: 3, r: 4, items: ids }], augs: [], noTraits: true });
    const emblem = Object.values(DT.ITEMS).find(it => it.emblem && !DT.LINE[line].traits.includes(it.emblem));
    if (emblem) {
      const combat = new DT.Combat(side([glove.id, emblem.id]), side([]), { seed: 10 });
      check(!combat.units.find(unit => unit.team === 0).traits.includes(emblem.emblem), context + ' rejected legacy emblem has no trait effect');
    }
    for (const held of [[glove.id], [glove.id, ordinary, glove.id], [ordinary, glove.id, ordinary]]) {
      const original = held.slice();
      const combat = new DT.Combat(side(held), side([]), { seed: 10 });
      const items = combat.units.find(unit => unit.team === 0).items;
      check(items.length <= 3, context + ' combat never expands past three slots');
      if (held[0] === glove.id) check(items.length === 3 && items.filter(id => DT.ITEMS[id].p.thief).length === 1, context + ' glove grants exactly two non-gloves');
      check(held.length === original.length && held.every((id, i) => id === original[i]), context + ' combat projection preserves source inventory');
    }
  }
  if (set.id === 'reckoning') {
    p.board = { '0,4': { ...g.newUnit('rec_zubamon', 0), items: ['rec_thief_gloves'] } }; p.bench.fill(null);
    p.items = ['rec_sglove', 'rec_blade']; g.stage = 2;
    g.fireGame('onAiTurn', p, {});
    check(p.items.length === 2 && p.items[0] === 'rec_sglove', 'Reckoning AI does not craft early for full glove carrier');
  }
}
console.log(`Item capacity: ${checks} checks across ${gloves} glove/variant/format cases.`);
