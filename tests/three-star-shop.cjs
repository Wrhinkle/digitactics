// Player-specific shop exclusions across every ruleset, pace and match format.
const assert = require('node:assert/strict');
const { load } = require('../tools/headless.cjs');
const DT = load({ strict: true });
let cases = 0;
for (const set of DT.Sets.pickable()) {
  for (const pace of ['normal', 'hyper']) for (const format of ['solo', 'duos']) {
    const g = new DT.Game({ seed: 143, setId: set.id, pace, format, allAI: true });
    const [p, q] = g.players;
    const line = DT.SHOP_LINES.find((l) => l.tier === 1).id;
    p.board = {}; p.bench.fill(null);
    p.bench[0] = g.newUnit(line, 1);
    assert(g.shopEligible(p, line), 'two-star units remain eligible');
    p.bench[0].stage = 2;
    assert(!g.shopEligible(p, line), 'three-star bench unit excluded');
    p.board['0,0'] = p.bench[0]; p.bench[0] = null;
    assert(!g.shopEligible(p, line), 'three-star board unit excluded');
    assert(g.shopEligible(q, line), 'opponent/partner still eligible');

    // Force the excluded line to be the only available pool entry. No retries or leaks.
    for (const id of Object.keys(g.pool)) g.pool[id] = id === line ? 20 : 0;
    p.shop.fill(null);
    for (let i = 0; i < 10; i++) {
      g.refreshShop(p);
      assert(p.shop.every((id) => id === null), `${set.id}: no special shop bypass`);
      assert.equal(g.pool[line], 20, 'excluded copies remain in the shared pool');
    }
    assert.equal(g.rollSlot(1, [100, 0, 0, 0, 0], q), line, 'other player can roll the same unit');
    g.pool[line]++;
    const unit = p.board['0,0'];
    assert(g.sell(p, unit.uid));
    assert(g.shopEligible(p, line), 'selling restores eligibility');
    assert.equal(g.rollSlot(1, [100, 0, 0, 0, 0], p), line);

    // An upgrade clears stale offers immediately, including locked shops and metadata.
    p.board = {}; p.bench.fill(null);
    for (let i = 0; i < g.copiesOf(line); i++) p.bench[i] = g.newUnit(line, 1);
    p.shop.fill(null); p.shop[0] = line; g.takeFromPool(line);
    p.shopMeta = [{ cost: 3, tag: 'Partner' }]; p.locked = true;
    const before = g.pool[line];
    assert(g.combine(p).some((u) => u.line === line && u.stage === 2));
    assert.equal(p.shop[0], null);
    assert.equal(p.shopMeta[0], null);
    assert.equal(g.pool[line], before + 1, 'stale offer returned exactly once');
    g.clearMaxedShop(p);
    assert.equal(g.pool[line], before + 1);
    cases++;
  }
}
console.log(`Passed three-star shop checks for ${cases} ruleset/pace/format combinations.`);
