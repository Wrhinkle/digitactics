// Template set: a small, hidden set that exercises the format end to end (10 lines, 4 traits, a TFT-style item
// system with tags, 3 augments, XP + interest + streak economy, a 2-copy line, a 2-slot line, a set mechanic).
// Copy this file (and _template-mech.js, _template-art.js) to start a new set; see docs/sets/FORMAT.md.
// Play it from the console with DT.UI.newGame('_template'), or: node tests/sim.js 5 --set _template
(function (G) {
  const DT = G.DT;
  const L = DT.line; // L(id, tier, traits, attr, forms, abilityNames, abilityType, options)

  const traits = {
    // origins (kind is free text; the codex groups traits by kind)
    EMBER: { name: 'Embers', kind: 'origin', color: '#ff7a3a', bp: [2, 4], lv: [0.15, 0.35],
      desc: 'Basic attacks from Embers burn their target. Embers deal more damage to burning enemies.',
      fmt: (v) => `burn on hit, +${DT.pct(v)} vs burning` },
    TIDE: { name: 'Tidecallers', kind: 'origin', color: '#3aa8ff', bp: [2, 3], lv: [0.2, 0.4],
      desc: 'Tidecallers start combat with a shield.', fmt: (v) => `${DT.pct(v)} HP shield` },
    // classes
    VAN: { name: 'Vanguard', kind: 'class', color: '#c9a66b', bp: [2, 4], lv: [20, 45],
      desc: 'Vanguards gain armor and magic resist.', fmt: (v) => `+${v} armor/MR` },
    ARC: { name: 'Arcanist', kind: 'class', color: '#c48bff', bp: [2, 4], lv: [20, 50],
      desc: 'All allies gain ability power. Arcanists gain double.', fmt: (v) => `+${v} AP (x2 own)` },
  };

  // role = stat template (DT.BASE_ROLES or def.roles); traits never set stats on their own.
  // Models: `model: 'agumon'` uses models/index.json keys agumon:0..2; no model (or unassigned: true) = placeholder.
  const lines = [
    L('t_ember1', 1, ['EMBER', 'VAN'], 'Vaccine', ['Cinderpup', 'Cinderhound', 'Cinderlord'], ['Spark Bite', 'Flame Fang', 'Inferno Fang'], 'nuke', { role: 'Striker', model: 'agumon' }),
    L('t_tide1', 1, ['TIDE', 'VAN'], 'Data', ['Puddlet', 'Wavelet', 'Tsunamon'], ['Splash', 'Tidal Slam', 'Maelstrom Slam'], 'slam', { role: 'Guardian', model: 'gomamon' }),
    L('t_arc1', 1, ['ARC'], 'Virus', ['Glimmer', 'Glowmancer', 'Starmancer'], ['Mote', 'Star Bolt', 'Nova Bolt'], 'nuke', { role: 'Mystic', unassigned: true }),
    L('t_ember2', 2, ['EMBER', 'ARC'], 'Virus', ['Ashling', 'Ashwitch', 'Ashqueen'], ['Ash Burst', 'Ember Storm', 'Firestorm'], 'aoe', { role: 'Mystic' }),
    L('t_tide2', 2, ['TIDE', 'ARC'], 'Vaccine', ['Rainling', 'Raincaller', 'Stormcaller'], ['Drizzle', 'Downpour', 'Deluge'], 'heal', { role: 'Support' }),
    L('t_van2', 2, ['VAN'], 'Data', ['Shieldkin', 'Shieldguard', 'Shieldlord'], ['Bash', 'Shield Wall', 'Bulwark'], 'shield', { role: 'Guardian', combine: 2 }), // 2 copies digivolve
    L('t_ember3', 3, ['EMBER'], 'Vaccine', ['Blazewing', 'Blazehawk', 'Blazephoenix'], ['Fire Dart', 'Flame Volley', 'Phoenix Rain'], 'barrage', { role: 'Sniper' }),
    L('t_tide3', 3, ['TIDE', 'VAN'], 'Data', ['Shellback', 'Shellguard', 'Shellfortress'], ['Shell Spin', 'Shell Crash', 'Fortress Crash'], 'stun', { role: 'Bruiser' }),
    L('t_arc4', 4, ['ARC', 'TIDE'], 'Free', ['Moonseer', 'Moonsage', 'Moonoracle'], ['Moonbeam', 'Lunar Lance', 'Eclipse'], 'line', { role: 'Mystic' }),
    L('t_dragon5', 5, ['EMBER', 'VAN'], 'Virus', ['Wyrmling', 'Wyrm', 'Elder Wyrm'], ['Wyrm Breath', 'Wyrm Breath', 'Elder Breath'], 'aoe', { role: 'Bruiser', slots: 2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }), // takes 2 team slots
    // wild units for PvE (not in the shop)
    L('t_slime', 1, ['TIDE'], 'Data', ['Slime', 'Slime', 'Slime'], ['Ooze', 'Ooze', 'Ooze'], 'nuke', { role: 'Striker', shop: false, hpMul: 0.8 }),
    L('t_golem', 3, ['VAN'], 'Data', ['Golem', 'Golem', 'Golem'], ['Rock Slam', 'Rock Slam', 'Rock Slam'], 'slam', { role: 'Guardian', shop: false, boss: true, hpMul: 1.5 }),
  ];

  // wild rounds: [lineId, stage, col, row] in the enemy's own rows 4-7 (4 = front)
  const pve = (stage, round) => {
    if (stage === 1) return round === 2 ? [['t_slime', 0, 2, 4], ['t_slime', 0, 4, 4]] : [['t_slime', 0, 2, 4], ['t_slime', 0, 3, 4], ['t_slime', 0, 4, 4]];
    const s = Math.min(2, Math.floor(stage / 3));
    return [['t_golem', s, 3, 4], ['t_slime', s, 1, 4], ['t_slime', s, 5, 4], ['t_slime', s, 3, 6]];
  };

  DT.Sets.register({
    id: '_template',
    name: 'Template',
    mode: 'tft',
    hidden: true, // not in the picker or Chaos
    nativePace: 'normal', // the flat rules below are a standard-style economy; Hyper Roll uses the engine defaults
    desc: 'A small test set that shows every part of the set format.',
    colors: { main: '#7fd0ff', accent: '#ff7a3a' },
    source: 'Made up for testing the set engine.',
    traits,
    lines,
    pve,
    items: {
      system: 'tft',
      components: {
        blade: { name: 'Blade', short: 'BL', color: '#ff7a45', stats: { ad: 0.15 } },
        orb: { name: 'Orb', short: 'OR', color: '#b26bff', stats: { ap: 15 } },
        plate: { name: 'Plate', short: 'PL', color: '#c0c6d0', stats: { armor: 20 } },
      },
      // [a, b, name, desc, passives, extraStats, { tags, aiRank, id, hooks }]. Tags drive the AI (ad / ap / tank).
      recipes: [
        ['blade', 'blade', 'Twin Blade', 'Deals 12% more damage.', { amp: 0.12 }, { ad: 0.1 }, { tags: ['ad'], aiRank: 2 }],
        ['blade', 'orb', 'Spellblade', 'Heals for 20% of all damage dealt.', { vamp: 0.2 }, {}, { tags: ['ad', 'ap'] }],
        ['blade', 'plate', 'Thorn Blade', 'When attacked, deals 40 magic damage to adjacent enemies.', { thorns: 40 }, {}, { tags: ['tank'] }],
        ['orb', 'orb', 'Grand Orb', 'A lot of ability power.', null, { ap: 35 }, { tags: ['ap'], aiRank: 2 }],
        ['orb', 'plate', 'Warded Orb', 'Start combat with a 25% max HP shield. After 8s, gain 30 AP.', { crown: true }, {}, { tags: ['ap', 'tank'] }],
        ['plate', 'plate', 'Tower Plate', 'Takes 15% less damage.', { dr: 0.15 }, {}, { tags: ['tank'], aiRank: 2 }],
      ],
      // a drop-only item with its own hook (onKill in _template-mech.js)
      list: [
        { id: 't_trophy', name: 'Hunter Trophy', cat: 'artifact', desc: 'Kills grant 20 AP for the rest of combat.', stats: { ad: 0.1 }, tags: ['ad', 'ap'], color: '#ffd84a', color2: '#7a4a10' },
      ],
    },
    augments: [
      { id: 't_cash', tier: 1, name: 'Pocket Change', desc: 'Gain 10 gold.', tags: ['econ'] },
      { id: 't_rally', tier: 2, name: 'Rally', desc: 'Your team gains 15% attack speed.', tags: ['combat'] },
      { id: 't_fortune', tier: 3, name: 'Fortune', desc: 'Gain 3 extra gold every round.', tags: ['econ'] },
    ],
    // TFT standard-style economy: XP, interest, streaks, damage from surviving units
    rules: {
      startHp: 100,
      startGold: 2,
      levelByStage: null,
      xp: { table: [0, 2, 2, 6, 10, 20, 36, 48, 72], perRound: 2, buyCost: 4, buyXp: 4, startLevel: 3, maxLevel: 9 },
      incomeByStage: null,
      baseIncome: 5,
      interest: { per: 10, max: 5 },
      streak: [[2, 1], [4, 2], [5, 3]],
      lossDamage: (stage) => [0, 0, 2, 5, 8, 10, 12, 17, 150][Math.min(stage, 8)],
      unitDamage: (u) => [1, 2, 3][u.stage],
      pveLossDamage: () => 0,
      poolSize: [0, 18, 15, 12, 10, 8],
      augmentStages: [2, 3, 4],
      armoryStages: [2, 4],
    },
  });
})(typeof window !== 'undefined' ? window : globalThis);
