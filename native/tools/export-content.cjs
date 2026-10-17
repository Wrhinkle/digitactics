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
  const standardIncome = '(stage, round) => (stage === 1 ? (round <= 3 ? 2 : 3) : stage === 2 && round === 1 ? 4 : 5)';
  if (typeof rules.baseIncome === 'function' && String(rules.baseIncome) !== standardIncome)
    throw new Error(`Unmapped income formula: ${set.id}/${pace}`);
  // Audited schedules repeat after stage one, except Reckoning's stage 3 carousel.
  // Refuse unknown source formulas rather than silently extrapolating sampled stages.
  const knownLengths = [
    '(stage) => (stage === 1 ? 4 : 7)', '(stage) => (stage === 1 ? 3 : 4)', '() => 5', '() => 2',
  ];
  const knownTypes = [
    "(stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'carousel' : round === 7 ? 'pve' : 'pvp')",
    "(stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'pve' : 'pvp')",
    "(stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : stage === 3 && round === 1 ? 'carousel' : round === 4 ? 'pve' : 'pvp')",
    "(stage, round) => (isCreep(roundNo(stage, round)) ? 'pve' : 'pvp')",
    "(stage, round) => (stage === 1 ? (round <= 3 ? 'pve' : 'pvp') : round === 5 ? 'pve' : 'pvp')",
    "(stage, round) => (stage === 1 || round === 4 ? 'pve' : 'pvp')",
    "() => 'pvp'",
  ];
  if (!knownLengths.includes(String(rules.roundsInStage)) || !knownTypes.includes(String(rules.roundType)))
    throw new Error(`Unmapped round schedule: ${set.id}/${pace}`);
  const roundTypes = stage => Array.from({ length: rules.roundsInStage(stage) }, (_, i) => rules.roundType(stage, i + 1));
  const schedule = { opening: roundTypes(1), repeating: roundTypes(2),
    overrides: String(rules.roundType) === knownTypes[2] ? { '3-1': 'carousel' } : {} };
  const stageLossSources = [
    '(stage) => TFT_STAGE_DAMAGE[Math.min(stage, TFT_STAGE_DAMAGE.length - 1)]',
    '(stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6)',
    '(stage) => [0, 0, 2, 3, 4, 6, 9, 13, 150][Math.min(stage, 8)]',
    '(stage) => [0, 1, 1, 2, 2, 3, 3, 4, 5][Math.min(stage, 8)]',
    '(stage) => Math.floor(((stage - 1) * 5 + 3) / 10)',
    '(stage) => (stage <= 2 ? 2 : stage <= 4 ? 3 : stage <= 6 ? 4 : 5)',
    '(stage) => (stage <= 2 ? 0 : Math.min(stage - 2, 6))', '() => 1',
  ];
  const unitLossSources = [ 'null', '(u) => [1, 1, 2][u.stage] || 1', '() => 1',
    '(u, stage) => (stage >= 4 && (u.stage >= 2 || u.line.tier >= 5) ? 1 : 0)',
    '(u, stage) => (u.line.tamer ? (stage >= 7 ? 2 : 1) : Math.floor(1 + unitPower(u) / 3))',
    '(u) => Math.floor(1 + (u.line.tier + 2 * u.stage) / 3)',
  ];
  const pveLossSources = [ 'null', '(stage) => (stage === 1 ? 0 : null)',
    '(stage) => (stage === 1 ? 1 : null)', '() => 1',
    '(stage) => (stage === 1 || stage === 4 ? 0 : null)',
    '(stage) => (stage === 1 ? 1 : stage === 4 ? 0 : null)', '() => 0',
  ];
  const lossIndex = stageLossSources.indexOf(String(rules.lossDamage));
  const unitLossIndex = unitLossSources.indexOf(String(rules.unitDamage));
  if (lossIndex < 0 || unitLossIndex < 0 || !pveLossSources.includes(String(rules.pveLossDamage)))
    throw new Error(`Unmapped match loss formula: ${set.id}/${pace}`);
  const loss = { stageDamage: lossIndex === 4 ? [] : Array.from({ length: 9 }, (_, stage) => rules.lossDamage(stage)),
    underlordsRoundDamage: lossIndex === 4,
    unitDamage: ['none','stars','one','galaxies','underlords','autochess'][unitLossIndex],
    pveDamage: rules.pveLossDamage ? rules.pveLossDamage(8) : null,
    pveOverrides: Object.fromEntries([1,4].filter(stage => rules.pveLossDamage && rules.pveLossDamage(stage) != null)
      .map(stage => [stage, rules.pveLossDamage(stage)])) };
  const economy = {
    startHp: rules.startHp, startGold: rules.startGold ?? 3,
    levelByStage: rules.levelByStage, incomeByStage: rules.incomeByStage,
    standardIncome: typeof rules.baseIncome === 'function',
    baseIncome: typeof rules.baseIncome === 'number' ? rules.baseIncome : 0,
    interest: rules.interest ? { per: rules.interest.per || 10, max: rules.interest.max || 5 } : null,
    streak: rules.streak || [], winGold: rules.winGold, winGoldPve: rules.winGoldPvE || 0,
    xp: rules.xp ? { table: DT.XP_NEXT, perRound: rules.xp.perRound ?? 2,
      buyCost: rules.xp.buyCost || 4, buyXp: rules.xp.buyXp || 4,
      startLevel: rules.xp.startLevel || 1, maxLevel: rules.xp.maxLevel || 9 } : null,
  };
  const exportLine = l => {
    const role = DT.ROLES[DT.roleOf(l)] || DT.BASE_ROLES.Striker;
    return {
    noSell: !!l.noSell, noItems: !!l.noItems, summonCount: l.summonN || null, transformation: l.tf || null, tamer: l.tamer || null, swap: !!l.swap, envoy: l.envoy || null, experiment: l.exp || null, mark: l.mark || null, rivals: l.rivals || [], summon: l.summon || null, id: l.id, tier: l.tier, price: l.price || l.tier, copies: l.combine || rules.copies || 3,
    forms: l.forms, traits: l.traits, attribute: l.attr, ai: { carry: !!l.ai?.carry, tank: l.ai?.tank || 0, damage: l.ai?.dmg || null, place: l.ai?.place || null, value: l.ai?.value || 0 },
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
  variants.push({ id, anomalyTags: set.id==='arcane'?Object.fromEntries(Object.entries(DT.ArcaneInfo.ANOM).map(([id,a])=>[id,a.tag])):{}, offerAi: Object.fromEntries([['augment',Object.values(DT.AUGMENT)],['power',Object.values(DT.POWER)]].flatMap(([kind, list])=>list.map(a=>[`${kind}:${a.id}`,{tier:a.tier||1,tags:a.tags||[],tamer:a.tamer||null}]))), economy, schedule, loss, recipes: DT.RECIPE, itemRoles: DT.ITEM_ROLE,
    coliseum: set.id === 'coliseum' ? { coreRounds: rules.coreRounds || [],
      powerUps: DT.Sets.def(set.id).powerUps.map(p => ({ id: p.id, tags: p.tags })),
    } : null,
    offers: { schedule: rules.offers || [], armoryStages: rules.armoryStages || [], armoryRound: rules.armoryRound || 1,
      armoryRounds: rules.armoryRounds, augmentStages: rules.augmentStages || [], augmentRound: rules.augmentRound || 2,
      augmentRounds: rules.augmentRounds, augmentRerolls: rules.augmentRerolls, augmentTierOdds: rules.augmentTierOdds,
      augments: DT.AUGMENTS.filter(a => !a.duo).map(a => ({ id: a.id, tier: a.tier })),
      duoAugments: DT.Duos.AUGMENTS.concat(DT.AUGMENTS.filter(a => a.duo && !DT.SHARED_AUGMENT[a.id])).map(a => ({ id: a.id, tier: a.tier })),
      powers: DT.POWERS.map(p => p.id), customArmory: !!DT.SET.def.items?.armory, powerRanks: rules.powerRanks || [] },
    loot: { itemCap: rules.itemCap, statComponents: DT.STAT_COMPONENTS, specialComponents: DT.SPECIAL_COMPONENTS || [],
      emblems: DT.EMBLEM_IDS, consumables: DT.CONSUMABLE_IDS, customPve: !!DT.SET.pveReward,
      mode: { policy: set.id, artifacts: Object.values(DT.ITEMS).filter(i => i.cat === 'artifact').map(i => i.id),
        components: DT.COMPONENT_IDS, shadowComponents: (DT.SPECIAL_COMPONENTS || []).filter(id => DT.COMPONENTS[id]?.shadow),
        radiantOf: DT.Sets.def(set.id).items?.radiantOf || {}, baseOf: DT.Sets.def(set.id).items?.baseOf || {},
        shadowOf: DT.Sets.def(set.id).items?.shadowOf || {}, shadowChance: rules.shadowDropChance || 0 } }, duo: DT.Duos.rules(rules), name: set.name, pace, lines, extraLines, partner: DT.Sets.def(set.id).partner || null, poolSizes: DT.POOL_SIZE, odds: DT.SHOP_ODDS,
    tamers: DT.Sets.def(set.id).ulTamers || {},
    forms: Object.fromEntries(Object.entries(DT.Sets.def(set.id).forms || {}).map(([line, forms]) => [line, forms.map(f => ({ id:f.id, name:f.name, description:f.desc, trait:f.trait || null }))])),
    dragonTraitLimits: Object.fromEntries(['normal','hyper','default'].map(p => [p, DT.Sets.rulesFor(set.id, p === 'default' ? undefined : p).drgTraitMax || 0])), pveMultipliers: rules.pveMul || [], rerollCost: rules.rerollCost, shopSize: rules.shopSize || 5,
    items: Object.entries(DT.ITEMS).map(([id, item]) => ({ id, tags: item.tags || [], aiRank: item.aiRank || 0, consumable: !!item.consumable, noDrop: !!item.noDrop, tier: item.tier || 0, hat: !!item.hat, stats: item.stats || {}, passives: item.p || {}, hooks: Object.keys(item.hooks || {}), emblem: item.emblem || null, component: !!item.component, special: !!item.special, category: item.cat || null,
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
fs.writeFileSync(path.join(out, 'catalog.json'), JSON.stringify({ schemaVersion: 29, variants }, null, 2) + '\n');
fs.mkdirSync(path.join(__dirname, '../tests/fixtures'), { recursive: true });
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/parity.json'), JSON.stringify({ rng, rolls, grids }) + '\n');
fs.writeFileSync(path.join(__dirname, '../tests/fixtures/unit-stats.json'), JSON.stringify(unitStats) + '\n');
console.log(`Exported ${variants.length} ruleset/pace catalogs and ${rolls.length} shop parity traces.`);
