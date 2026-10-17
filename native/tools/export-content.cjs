// Build-time migration adapter. The native runtime never executes JavaScript.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const variants = [];
const rolls = [];
for (const set of DT.Sets.pickable()) for (const pace of ['normal', 'hyper']) {
  DT.Sets.activate(set.id, pace);
  const rules = DT.HYPER;
  const lines = DT.SHOP_LINES.map(l => ({
    id: l.id, tier: l.tier, price: l.price || l.tier, copies: l.combine || rules.copies || 3,
    forms: l.forms, traits: l.traits, attribute: l.attr,
  }));
  const id = `${set.id}/${pace}`;
  variants.push({ id, name: set.name, pace, lines, poolSizes: DT.POOL_SIZE, odds: DT.SHOP_ODDS,
    rerollCost: rules.rerollCost, shopSize: rules.shopSize || 5 });
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
fs.writeFileSync(path.join(out, 'catalog.json'), JSON.stringify({ schemaVersion: 1, variants }, null, 2) + '\n');
fs.mkdirSync(path.join(__dirname, '../tests/fixtures'), { recursive: true });
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/parity.json'), JSON.stringify({ rng, rolls, grids }) + '\n');
console.log(`Exported ${variants.length} ruleset/pace catalogs and ${rolls.length} shop parity traces.`);
