// Engine-level data: board size, attributes, stat templates, stat and ability formulas.
// Set content (Digimon lines, traits, items, economy, wild rounds) lives in js/sets/<id>.js; js/sets.js points
// DT.LINES, DT.TRAITS, DT.ITEMS and the other content globals at the active set.
(function (G) {
  const DT = (G.DT = G.DT || {});

  DT.COLS = 7;
  DT.ROWS = 8;
  DT.BENCH = 9;
  DT.SHOP = 5;

  DT.TIER_COLORS = ['', '#9aa4b1', '#3fbf6a', '#3b8cff', '#b45cff', '#ffb02e'];
  DT.STAGE_COLORS = ['#d08a4a', '#d6dbe4', '#ffd84a'];

  DT.ATTR = {
    Vaccine: { color: '#4fb3ff', short: 'VAC' },
    Data: { color: '#4fe08a', short: 'DAT' },
    Virus: { color: '#c45cff', short: 'VIR' },
    Free: { color: '#c8c8c8', short: 'FRE' },
  };
  // attacker attribute -> attribute it deals +20% damage to
  DT.ATTR_BEATS = { Vaccine: 'Virus', Virus: 'Data', Data: 'Vaccine' };

  // Base stats per stat template ("role"), for a tier-1 unit at its first form. A set can override or add
  // templates in def.roles. Template flags: leap (jumps to the enemy backline at the start of combat),
  // ai: { place, tank, carry, dmg } (see js/sets.js ROLE_AI).
  DT.BASE_ROLES = {
    Guardian: { hp: 720, ad: 45, as: 0.6, range: 1, armor: 45, mr: 45, mana: 90, startMana: 30 },
    Striker: { hp: 620, ad: 60, as: 0.7, range: 1, armor: 30, mr: 30, mana: 70, startMana: 10 },
    Sniper: { hp: 480, ad: 55, as: 0.75, range: 4, armor: 20, mr: 20, mana: 70, startMana: 0 },
    Mystic: { hp: 480, ad: 40, as: 0.65, range: 3, armor: 20, mr: 25, mana: 60, startMana: 20 },
    Assassin: { hp: 520, ad: 65, as: 0.8, range: 1, armor: 25, mr: 25, mana: 60, startMana: 0, leap: true },
    Bruiser: { hp: 760, ad: 50, as: 0.6, range: 1, armor: 35, mr: 35, mana: 80, startMana: 20 },
    Duelist: { hp: 680, ad: 55, as: 0.75, range: 1, armor: 35, mr: 35, mana: 70, startMana: 0 },
    Support: { hp: 520, ad: 35, as: 0.65, range: 3, armor: 20, mr: 25, mana: 70, startMana: 30 },
  };
  DT.ROLES = DT.BASE_ROLES;

  DT.DEFAULT_TAMERS = ['Tai', 'Matt', 'Sora', 'Izzy', 'Mimi', 'Joe', 'Kari'];
  DT.TAMERS = DT.DEFAULT_TAMERS;

  DT.pct = (v) => `${+(v * 100).toFixed(1)}%`;

  // Line builder for set files: DT.line(id, tier, traits, attr, forms, abilityNames, abilityType, options)
  DT.line = (id, tier, traits, attr, forms, abil, type, opt = {}) => ({ id, tier, traits, attr, forms, abil, type, shop: true, ...opt });

  // Ability base values at tier 1, first form, 100 AP. Sets add types with def.abilities[type].base.
  DT.ABILITY_BASE = { nuke: 260, aoe: 160, stun: 165, slam: 120, shield: 300, heal: 170, barrage: 68, drain: 180, dash: 210,
    weaken: 190, rally: 70, line: 190, execute: 210, chain: 170 };

  // stat template of a line: line.role, else its first trait of kind 'role'
  DT.roleOf = (line) => line.role || line.traits.find((t) => DT.TRAITS[t] && DT.TRAITS[t].kind === 'role');
  DT.stageName = (line, stage) => (line.levels || ['Rookie', 'Champion', 'Ultimate'])[stage];

  DT.unitStats = function (line, stage) {
    const L = typeof line === 'string' ? DT.LINE[line] : line;
    const R = DT.ROLES[DT.roleOf(L)] || DT.BASE_ROLES.Striker;
    const tm = 1 + 0.18 * (L.tier - 1);
    const sm = [1, 1.8, 3.24][stage];
    return {
      maxHp: Math.round(R.hp * tm * sm * (L.hpMul || 1)),
      ad: Math.round(R.ad * tm * sm * (L.adMul || 1)),
      as: L.as || R.as,
      range: L.range || R.range,
      armor: L.armor != null ? L.armor : R.armor,
      mr: L.mr != null ? L.mr : R.mr,
      maxMana: L.mana || R.mana,
      mana: L.startMana != null ? L.startMana : R.startMana,
    };
  };

  DT.abilityBase = (type) => {
    const d = DT.abilityDef && DT.abilityDef(type);
    return d && d.base != null ? d.base : DT.ABILITY_BASE[type] || 0;
  };
  DT.abilityValue = function (line, stage) {
    return Math.round(DT.abilityBase(line.type) * [1, 1.7, 3][stage] * (1 + 0.25 * (line.tier - 1)));
  };
  DT.stunDuration = (stage) => [1.5, 1.75, 2][stage];
  DT.healTargets = (stage) => stage + 1;
  DT.barrageShots = (line, stage) => (line.shots || 4) + stage;
  DT.rallyTargets = (stage) => (stage >= 2 ? 3 : 2);
  DT.chainJumps = (stage) => stage + 2;

  DT.abilityText = function (line, stage, ap = 100) {
    const v = Math.round((DT.abilityValue(line, stage) * ap) / 100);
    const R = line.radius || 1;
    if (line.abilText) return typeof line.abilText === 'function' ? line.abilText(stage, v) : String(line.abilText);
    switch (line.type) {
      case 'nuke': return `Deals ${v} magic damage to the target.`;
      case 'aoe': return `Deals ${v} magic damage to the target and enemies within ${R} hex.`;
      case 'stun': return `Deals ${v} magic damage and stuns the target for ${DT.stunDuration(stage)}s.`;
      case 'slam': return `Slams the ground, dealing ${v} magic damage to enemies within ${R} hex and stunning them for 1s.`;
      case 'shield': return `Gains a ${v} shield and taunts nearby enemies.`;
      case 'heal': return `Heals the ${DT.healTargets(stage)} lowest-health allies for ${v}.`;
      case 'barrage': return `Fires ${DT.barrageShots(line, stage)} shots at random enemies, each dealing ${v} magic damage.`;
      case 'drain': return `Deals ${v} magic damage to the target${line.radius ? ' and adjacent enemies' : ''}, healing for 60% of damage dealt.`;
      case 'dash': return `Dashes to the lowest-health enemy and strikes for ${v} magic damage.`;
      case 'weaken': return `Deals ${v} magic damage and makes the target deal 30% less damage for 4s.`;
      case 'rally': return `Shields the ${DT.rallyTargets(stage)} lowest-health allies for ${v} and gives them 30% attack speed for 4s.`;
      case 'line': return `Fires through the target, dealing ${v} magic damage to every enemy in a line.`;
      case 'execute': return `Strikes the target for ${v} magic damage, doubled if it is below 50% HP.`;
      case 'chain': return `Lightning hits the target for ${v} magic damage, then jumps to ${DT.chainJumps(stage)} more enemies, losing 20% each jump.`;
    }
    const d = DT.abilityDef && DT.abilityDef(line.type);
    if (d && d.text) return d.text(line, stage, v);
    return '';
  };
})(typeof window !== 'undefined' ? window : globalThis);
