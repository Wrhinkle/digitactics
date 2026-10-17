// Exercise the shipping damage method with controlled effects/hooks; capture its observable results.
const fs = require('node:fs');
const path = require('node:path');
const { load } = require('../../tools/headless.cjs');
const DT = load({ strict: true });
const rng = DT.makeRng(19431);
const attrs = ['Vaccine', 'Virus', 'Data', 'Free'];
const fixtures = [];
for (let i = 0; i < 1200; i++) {
  const source = { attribute: attrs[i % 4], amplification: rng() * 0.8, weakened: i % 3 === 0,
    weakness: rng() * 0.6, bonusAgainstBurning: rng() * 0.5, perHexAmplification: i % 5 ? 0 : rng() * 0.1,
    giantSlayer: i % 4 ? 0 : 0.3, guardBreaker: i % 6 ? 0 : 0.4 };
  const target = { attribute: attrs[Math.floor(i / 4) % 4], maxHealth: i % 2 ? 1600 : 1599,
    armor: rng() * 400 - 100, magicResist: rng() * 300 - 100, shield: i % 3 ? rng() * 500 : 0,
    mana: 10, maxMana: i % 7 ? 80 : 0, reduction: rng() * 0.8, gargoyle: rng() * 40,
    veil: rng() * 60, shred: i % 2 ? 0 : 0.5, burning: i % 4 === 0,
    invulnerable: i % 37 === 0, alive: i % 41 !== 0 };
  const request = { source, target, kind: i % 3, amount: rng() * 600, distance: i % 7,
    overtime: 1 + rng() * 2, extraReduction: rng() * 0.3, incomingMultiplier: 0.5 + rng(),
    outgoingMultiplierOverride: i % 13 ? null : rng() * 2, isBurn: i % 9 === 0 };
  const src = { line: { attr: source.attribute }, c: 0, r: 0, amp: source.amplification,
    weakUntil: source.weakened ? 2 : 0, weak: source.weakness, vsBurningAmp: source.bonusAgainstBurning,
    perHexAmp: source.perHexAmplification, P: { slayer: source.giantSlayer, guardbreak: source.guardBreaker }, dealt: 0 };
  const tg = { line: { attr: target.attribute }, c: request.distance, r: 0, maxHp: target.maxHealth, hp: target.maxHealth,
    armor: target.armor, mr: target.magicResist, shield: target.shield, mana: target.mana, maxMana: target.maxMana,
    dr: target.reduction, garg: target.gargoyle, veilUntil: 2, veil: target.veil, shredUntil: 2, shred: target.shred,
    burn: target.burning ? { until: 2 } : null, invulnUntil: target.invulnerable ? 2 : 0,
    alive: target.alive, taken: 0 };
  const combat = Object.create(DT.Combat.prototype);
  Object.assign(combat, { t: 1, overtime: request.overtime,
    has: name => name === 'onBeforeDamageTaken' || (name === 'onBeforeDamage' && request.outgoingMultiplierOverride != null),
    fire: (name, unit, event) => {
      if (name === 'onBeforeDamage') event.mult = request.outgoingMultiplierOverride;
      if (name === 'onBeforeDamageTaken') { event.mult = request.incomingMultiplier; event.dr = request.extraReduction; }
    }, ev() {}, kill() {}, checkLifeline() {} });
  const amount = combat.damage(src, tg, request.amount, ['phys', 'magic', 'true'][request.kind], false, request.isBurn);
  fixtures.push({ request, expected: { amount, shieldAbsorbed: target.shield - tg.shield,
    healthDamage: target.maxHealth - tg.hp, manaAfter: tg.mana } });
}
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/damage.json'), JSON.stringify(fixtures) + '\n');
console.log(`Exported ${fixtures.length} reference damage traces.`);
