// Build-time migration adapter. The native runtime never executes JavaScript.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const variants = [];
const rolls = [];
const unitStats = [];
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const rules = DT.HYPER;
  const exportLine = l => {
    const role = DT.ROLES[DT.roleOf(l)] || DT.BASE_ROLES.Striker;
    return {
    tamer: l.tamer || null, swap: !!l.swap, envoy: l.envoy || null, experiment: l.exp || null, mark: l.mark || null, rivals: l.rivals || [], summon: l.summon || null, id: l.id, tier: l.tier, price: l.price || l.tier, copies: l.combine || rules.copies || 3,
    forms: l.forms, traits: l.traits, attribute: l.attr,
    slots: l.slots || 1,
    sellPremium: set.id === 'dragonlands' && l.slots === 2 ? Math.max(0, (l.price || l.tier) - l.tier) : 0,
    combat: {
      leap: !!(l.leap != null ? l.leap : role.leap), health: role.hp, attackDamage: role.ad, attackSpeed: l.as || role.as,
      range: l.range || role.range, armor: l.armor ?? role.armor, magicResist: l.mr ?? role.mr,
      maxMana: l.mana || role.mana, startMana: l.startMana ?? role.startMana,
      healthMultiplier: l.hpMul || 1, attackMultiplier: l.adMul || 1,
    },
    ability: { kind: l.type, baseValue: DT.abilityBase(l.type), radius: l.radius || 0, shots: l.shots || 4, names: l.abil },
  }; };
  const lines = DT.SHOP_LINES.map(exportLine);
  const extraLines = Object.values(DT.LINE).filter(l => !l.shop).map(exportLine);
  const id = `${set.id}/${pace}`;
  variants.push({ id, name: set.name, pace, lines, extraLines, partner: DT.Sets.def(set.id).partner || null, poolSizes: DT.POOL_SIZE, odds: DT.SHOP_ODDS,
    tamers: DT.Sets.def(set.id).ulTamers || {},
    forms: Object.fromEntries(Object.entries(DT.Sets.def(set.id).forms || {}).map(([line, forms]) => [line, forms.map(f => ({ id:f.id, name:f.name, description:f.desc, trait:f.trait || null }))])),
    dragonTraitLimits: Object.fromEntries(['normal','hyper','default'].map(p => [p, DT.Sets.rulesFor(set.id, p === 'default' ? undefined : p).drgTraitMax || 0])), pveMultipliers: rules.pveMul || [], rerollCost: rules.rerollCost, shopSize: rules.shopSize || 5,
    items: Object.entries(DT.ITEMS).map(([id, item]) => ({ id, tier: item.tier || 0, hat: !!item.hat, stats: item.stats || {}, passives: item.p || {}, hooks: Object.keys(item.hooks || {}), emblem: item.emblem || null, component: !!item.component, special: !!item.special, category: item.cat || null,
      ingredients: item.from || null, selfUpgrade: DT.combineItems(id, id),
      completedFrom: Object.entries(DT.RECIPE || {}).filter(([key, value]) => key.split('+').includes(id) && DT.ITEMS[value]?.cat === 'completed').map(([, value]) => value) })),
    coreItems: DT.CORE_ITEMS,
    traits: Object.entries(DT.TRAITS).map(([id, trait]) => ({ id, kind: trait.kind, duo: trait.duo ?? 'sum', breakpoints: trait.bp, values: trait.lv,
      parameters: Object.fromEntries(Object.entries(trait).filter(([key, value]) => !['bp', 'lv'].includes(key) && Array.isArray(value) && value.every(x => typeof x === 'number'))) })) });
  for (const line of Object.values(DT.LINE)) for (let stage = 0; stage < 3; stage++) {
    unitStats.push({ variant: id, line: line.id, stage, stats: DT.unitStats(line, stage),
      ability: DT.abilityValue(line, stage), sell: DT.sellValue({ line: line.id, stage, items: [] }) });
  }
  for (const seed of [0, 1, 143, 4294967295]) {
    const g = Object.create(DT.Game.prototype);
    g.rng = DT.makeRng(seed);
    g.pool = Object.fromEntries(lines.map(l => [l.id, DT.POOL_SIZE[l.tier]]));
    const maxed = lines.slice(0, 3).map(l => l.id);
    const player = { allUnits: () => maxed.map(line => ({ line, stage: 2 })) };
    const expected = Array.from({ length: 100 }, (_, i) => g.rollSlot(1 + i % (DT.SHOP_ODDS.length - 1), null, player));
    rolls.push({ variant: id, seed, maxed, expected });
  }
}
const rng = [0, 1, 143, 4294967295].map(seed => {
  const next = DT.makeRng(seed);
  return { seed, expected: Array.from({ length: 1000 }, () => next()) };
});
const grids = [7, 14].map(cols => {
  const rows = 8;
  const cells = Array.from({ length: cols * rows }, (_, i) => ({ c: i % cols, r: Math.floor(i / cols) }));
  const neighbors = cells.map(a => cells.filter(b => DT.hexDist(a, b) === 1));
  const next = DT.makeRng(414);
  const paths = Array.from({ length: 100 }, () => {
    const start = Math.floor(next() * cells.length);
    const enemies = [0, 1, 2].map(() => Math.floor(next() * cells.length)).filter(i => i !== start);
    const occupied = cells.map((_, i) => i === start || enemies.includes(i) || next() < 0.24);
    const range = 1 + Math.floor(next() * 3);
    const unit = { ...cells[start], team: 0, alive: true, range };
    const targets = enemies.map(i => ({ ...cells[i], team: 1, alive: true }));
    const combat = Object.create(DT.Combat.prototype);
    Object.assign(combat, { units: [unit, ...targets], CELLS: cells, NEIGH: neighbors,
      idx: (c, r) => r * cols + c, occ: occupied.map(x => x ? {} : null) });
    combat.moveTowardEnemy(unit);
    return { start, range, enemies, occupied, expected: unit.target ? { step: combat.idx(unit.c, unit.r), target: targets.indexOf(unit.target) } : null };
  });
  return { cols, rows, distances: cells.flatMap(a => cells.map(b => DT.hexDist(a, b))), paths };
});
const out = path.join(__dirname, '../content');
fs.mkdirSync(out, { recursive: true });
fs.writeFileSync(path.join(out, 'catalog.json'), JSON.stringify({ schemaVersion: 14, variants }, null, 2) + '\n');
fs.mkdirSync(path.join(__dirname, '../tests/fixtures'), { recursive: true });
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/parity.json'), JSON.stringify({ rng, rolls, grids }) + '\n');
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/unit-stats.json'), JSON.stringify(unitStats) + '\n');
console.log(`Exported ${variants.length} ruleset/pace catalogs and ${rolls.length} shop parity traces.`);
