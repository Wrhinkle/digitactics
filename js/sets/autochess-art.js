// Auto Chess art: item portraits (drop parts, finished items, the Digi-Egg, relics), a move look for every ability
// name in the set and a basic attack look for every line. Specs follow the format at the top of js/moves.js; painters
// use the js/itemart.js kit (FORMAT.md, "Art and move looks"). Nothing here runs until the renderer asks for it.
(function (G) {
  const DT = G.DT;

  // ---------- item portraits ----------
  // Drop parts: bronze frame (tier 1), silver (tier 2), gold (tier 3), one motif in the part color.
  // Finished items: gold frame split in the two part colors, a motif from the pair. Relics: hex prism frame + symbol.
  const TIER_METAL = { 1: 'bronze', 2: 'silver', 3: 'gold' };
  const PART_COLOR = {
    ac_attack_chip: '#ff7a45', ac_defense_chip: '#c0c6d0', ac_speed_chip: '#ffd23f', ac_recovery_disk: '#5adf8a', ac_vampire_fang: '#d83a5a',
    ac_resist_cape: '#3fd6b0', ac_mp_chip: '#4fb3ff', ac_hp_chip: '#ff4d64', ac_brain_chip: '#b26bff', ac_power_claw: '#ff9a3a',
    ac_hyper_chip: '#ffe94a', ac_dark_edge: '#7a4aff', ac_holy_ring: '#fff0a0',
  };
  const PART_TIER = { ac_attack_chip: 1, ac_defense_chip: 1, ac_speed_chip: 1, ac_recovery_disk: 1, ac_vampire_fang: 1, ac_resist_cape: 1, ac_mp_chip: 1,
    ac_hp_chip: 2, ac_brain_chip: 2, ac_power_claw: 2, ac_hyper_chip: 3, ac_dark_edge: 3, ac_holy_ring: 3 };
  const RECIPE_PARTS = {
    ac_crystal_claw: ['ac_power_claw', 'ac_attack_chip'], ac_grand_claw: ['ac_power_claw', 'ac_dark_edge'], ac_thunder_disk: ['ac_attack_chip', 'ac_speed_chip'],
    ac_thunder_hammer: ['ac_speed_chip', 'ac_hyper_chip'], ac_berserk_mask: ['ac_vampire_fang', 'ac_speed_chip'], ac_fury_axe: ['ac_recovery_disk', 'ac_dark_edge'],
    ac_corrosion_blade: ['ac_attack_chip', 'ac_attack_chip'], ac_black_gear: ['ac_hp_chip', 'ac_power_claw'], ac_digicore: ['ac_hp_chip', 'ac_hp_chip'],
    ac_digizoid_armor: ['ac_defense_chip', 'ac_hyper_chip'], ac_reflect_mail: ['ac_defense_chip', 'ac_power_claw'], ac_divine_sword: ['ac_dark_edge', 'ac_dark_edge'],
    ac_refresh_disk: ['ac_mp_chip', 'ac_holy_ring'], ac_data_hood: ['ac_recovery_disk', 'ac_resist_cape'], ac_brave_shield: ['ac_hp_chip', 'ac_defense_chip'],
    ac_numemon_wand: ['ac_brain_chip', 'ac_holy_ring'], ac_moon_chip: ['ac_hyper_chip', 'ac_hyper_chip'], ac_true_lance: ['ac_dark_edge', 'ac_speed_chip'],
    ac_hellfire_rod: ['ac_brain_chip', 'ac_mp_chip'], ac_mystic_staff: ['ac_brain_chip', 'ac_brain_chip'], ac_sage_orb: ['ac_brain_chip', 'ac_resist_cape'],
    ac_steady_disk: ['ac_recovery_disk', 'ac_mp_chip'], ac_blood_ring: ['ac_vampire_fang', 'ac_dark_edge'], ac_stun_hammer: ['ac_attack_chip', 'ac_defense_chip'],
    ac_frost_ring: ['ac_holy_ring', 'ac_holy_ring'], ac_medical_kit: ['ac_defense_chip', 'ac_resist_cape'], ac_radiant_crest: ['ac_holy_ring', 'ac_dark_edge'],
    ac_dragon_lance: ['ac_hp_chip', 'ac_speed_chip'], ac_soul_disk: ['ac_hp_chip', 'ac_mp_chip'], ac_iron_shield: ['ac_defense_chip', 'ac_defense_chip'],
    ac_wind_feather: ['ac_speed_chip', 'ac_speed_chip'], ac_iron_helm: ['ac_vampire_fang', 'ac_defense_chip'], ac_ward_cape: ['ac_resist_cape', 'ac_resist_cape'],
  };
  const bladeFill = (k, c) => [[0, '#ffffff'], [0.45, k.lt(c, 0.55)], [1, k.dk(c, 0.35)]];

  // motif painters, drawn around (0, 0) at about 90 px
  const MOTIF = {
    chip: (p, k, c) => k.M.chip(p, c, 60),
    sword: (p, k, c) => p.at(0, 0, 1, 45, () => k.M.sword(p, { w: 16, blade: bladeFill(k, c), guard: c, gw: 34, pommel: c })),
    bigsword: (p, k, c) => p.at(0, 0, 1.08, 45, () => k.M.sword(p, { w: 26, tip: 20, blade: bladeFill(k, c), guard: c, gw: 42, pommel: c })),
    claw: (p, k, c) => { for (const x of [-16, 0, 16]) p.at(x, 0, 1, 12, () => p.part(k.poly([[-5, 30], [5, 30], [3, -12], [0, -36], [-3, -12]]), bladeFill(k, c), { ow: 4 })); p.part(k.rr(-26, 22, 52, 14, 6), k.dk(c, 0.3), { ow: 4 }); },
    shield: (p, k, c) => { p.part('M0 -40L34 -28C34 6 22 28 0 40C-22 28 -34 6 -34 -28Z', c); p.line('M0 -32V32', k.dk(c, 0.5), 3, 0.6); p.gem(0, -4, 7, '#fff3b0', 'diamond', 4); },
    disk: (p, k, c) => { k.M.disk(p, c, 38); p.part(k.circ(0, 0, 9), '#ffffff', { ow: 3, hi: false }); },
    fang: (p, k, c) => { for (const x of [-12, 12]) p.part(k.poly([[x - 9, -26], [x + 9, -26], [x, 30]]), [[0, '#ffffff'], [1, k.lt(c, 0.4)]], { ow: 4 }); p.part(k.rr(-26, -34, 52, 12, 5), c, { ow: 4 }); },
    cape: (p, k, c) => k.M.hood(p, c),
    heart: (p, k, c) => k.M.heart(p, c, 0.85),
    orb: (p, k, c) => { p.glow(0, 0, 40, c, 0.7); p.gem(0, 0, 26, c, 'round', 5); },
    ring: (p, k, c) => { p.tube(k.circ(0, 4, 28), 12, c, { ow: 5 }); p.gem(0, -26, 9, '#ffffff', 'diamond', 4); },
    gem: (p, k, c) => { p.glow(0, 0, 36, c, 0.6); k.M.crystal(p, c, 64, 18); },
    bolt: (p, k, c) => { p.glow(0, 0, 34, c, 0.7); p.at(0, 0, 1.6, 0, () => p.part(k.BOLT, [[0, '#ffffff'], [1, c]], { ow: 3 })); },
    hammer: (p, k, c) => p.at(0, 0, 1, 30, () => { p.tube('M0 38V-10', 8, '#7a5a3a'); p.part(k.rr(-26, -34, 52, 26, 6), c); }),
    mask: (p, k, c) => { p.part('M-34 -16C-34 -34 34 -34 34 -16C34 14 16 34 0 34C-16 34 -34 14 -34 -16Z', c); p.part(k.ell(-13, -8, 8, 6), '#15161d', { ow: 0, hi: false }); p.part(k.ell(13, -8, 8, 6), '#15161d', { ow: 0, hi: false }); },
    axe: (p, k, c) => p.at(0, 0, 1, 25, () => { p.tube('M0 40V-34', 7, '#7a5a3a'); p.part('M2 -36C30 -36 38 -12 34 4C24 -2 12 -4 2 -2Z', bladeFill(k, c)); p.part('M-2 -36C-30 -36 -38 -12 -34 4C-24 -2 -12 -4 -2 -2Z', bladeFill(k, c)); }),
    gear: (p, k, c) => { p.part(k.star(0, 0, 38, 28, 10), c); p.part(k.circ(0, 0, 12), '#2a2c36', { ow: 4, hi: false }); },
    staff: (p, k, c) => p.at(0, 0, 1, 35, () => { p.part(k.rr(-4, -10, 8, 50, 3), 'dark'); p.glow(0, -26, 22, c, 0.8); p.at(0, -26, 1, 0, () => k.M.crystal(p, c, 34, 10)); }),
    feather: (p, k, c) => p.at(0, 0, 1, -30, () => k.M.wing(p, c, 1)),
    plate: (p, k) => k.M.plate(p),
    egg: (p, k, c) => k.M.egg(p, c, [[-12, -8, 7, '#ff8a4a'], [12, 6, 9, '#ff8a4a'], [-4, 24, 6, '#5fb0ff'], [16, -22, 5, '#5fb0ff']]),
    lance: (p, k, c) => p.at(0, 0, 1, 40, () => { p.tube('M0 44V-10', 7, k.dk(c, 0.2)); p.part(k.poly([[0, -46], [10, -12], [-10, -12]]), bladeFill(k, c)); }),
    flask: (p, k, c) => { p.part('M-8 -36H8V-14L26 22C30 30 24 38 16 38H-16C-24 38 -30 30 -26 22L-8 -14Z', 'glass', { ow: 5 }); p.part('M-20 18H20L24 26C26 32 22 34 16 34H-16C-22 34 -26 32 -24 26Z', c, { ow: 0 }); },
  };
  const PART_MOTIF = {
    ac_attack_chip: 'chip', ac_defense_chip: 'plate', ac_speed_chip: 'bolt', ac_recovery_disk: 'disk', ac_vampire_fang: 'fang', ac_resist_cape: 'cape',
    ac_mp_chip: 'gem', ac_hp_chip: 'heart', ac_brain_chip: 'staff', ac_power_claw: 'claw', ac_hyper_chip: 'gear', ac_dark_edge: 'sword', ac_holy_ring: 'ring',
  };
  const DONE_MOTIF = {
    ac_crystal_claw: 'claw', ac_grand_claw: 'claw', ac_thunder_disk: 'bolt', ac_thunder_hammer: 'hammer', ac_berserk_mask: 'mask', ac_fury_axe: 'axe',
    ac_corrosion_blade: 'sword', ac_black_gear: 'gear', ac_digicore: 'heart', ac_digizoid_armor: 'plate', ac_reflect_mail: 'shield', ac_divine_sword: 'bigsword',
    ac_refresh_disk: 'disk', ac_data_hood: 'cape', ac_brave_shield: 'shield', ac_numemon_wand: 'staff', ac_moon_chip: 'orb', ac_true_lance: 'lance',
    ac_hellfire_rod: 'staff', ac_mystic_staff: 'staff', ac_sage_orb: 'orb', ac_steady_disk: 'disk', ac_blood_ring: 'ring', ac_stun_hammer: 'hammer',
    ac_frost_ring: 'ring', ac_medical_kit: 'flask', ac_radiant_crest: 'orb', ac_dragon_lance: 'lance', ac_soul_disk: 'gem', ac_iron_shield: 'shield',
    ac_wind_feather: 'feather', ac_iron_helm: 'mask', ac_ward_cape: 'cape',
  };
  // accent symbol on finished items (small, bottom right), from the kit symbols
  const DONE_SYM = { ac_thunder_disk: 'chip', ac_moon_chip: 'moon', ac_sage_orb: 'star6', ac_radiant_crest: 'sun', ac_frost_ring: 'snow', ac_hellfire_rod: 'flame',
    ac_numemon_wand: 'sludge', ac_blood_ring: 'blood', ac_medical_kit: 'plus', ac_black_gear: 'skull', ac_divine_sword: 'star5', ac_grand_claw: 'star6' };

  const items = {};
  for (const [id, c] of Object.entries(PART_COLOR)) {
    items[id] = (p, k) => {
      k.frame(p, 'square', TIER_METAL[PART_TIER[id]], [c], { w: 6 });
      p.obj(() => p.at(64, 64, 1, 0, () => MOTIF[PART_MOTIF[id]](p, k, c)));
    };
  }
  for (const [id, [a, b]] of Object.entries(RECIPE_PARTS)) {
    items[id] = (p, k) => {
      const ca = PART_COLOR[a], cb = PART_COLOR[b];
      k.frame(p, 'square', 'gold', [ca, cb], { w: 8 });
      p.obj(() => {
        p.at(64, 62, 0.95, 0, () => MOTIF[DONE_MOTIF[id]](p, k, k.mix(ca, cb, 0.35)));
        const s = DONE_SYM[id];
        if (s && k.SYM[s]) p.at(96, 96, 0.8, 0, () => k.SYM[s](p, cb));
      });
    };
  }
  items.ac_digiegg = (p, k) => {
    k.frame(p, 'circle', 'prism', ['#fff3b0'], { w: 7 });
    p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 50, '#fff3b0', 0.6); MOTIF.egg(p, k, '#fff8d8'); p.sparkle(30, -34, 7); }));
  };
  const RELIC_ART = {
    ac_r_treasure: ['#ffd84a', 'coin'], ac_r_forge: ['#ff7a45', 'sword'], ac_r_vault: ['#ffcf4a', 'chevrons'], ac_r_charm: ['#c9d1dc', 'moon'],
    ac_r_recruiter: ['#7fd0ff', 'up'], ac_r_grace: ['#5adf8a', 'plus'], ac_r_reaper: ['#c0304a', 'skull'], ac_r_eggnest: ['#fff3b0', 'egg'],
    ac_r_card: ['#4fe08a', 'refresh'], ac_r_key: ['#b0b8c8', 'crestTag'], ac_r_discount: ['#ffb04a', 'die'],
  };
  for (const [id, [c, sym]] of Object.entries(RELIC_ART)) {
    items[id] = (p, k) => {
      k.frame(p, 'hex', 'prism', [c], { w: 7 });
      p.obj(() => p.at(64, 64, 1.5, 0, () => { p.glow(0, 0, 26, c, 0.6); (k.SYM[sym] || k.SYM.star6)(p, c); }));
    };
  }

  // ---------- move looks ----------
  // Builders: each returns a js/moves.js spec. size 1 small, 2 medium, 3 big (shake and scale).
  const SH = [0, 0.04, 0.1, 0.2];
  const B = {
    orb: (elem, n, trail = 'flame', o = {}) => ({ elem, charge: 'mouth', release: 0.22, impact: o.impact || 'blast', shake: SH[n], fx: [['orb', { size: 0.16 + 0.08 * n, speed: 12, arc: 0.2, trail, impactSize: 0.9 + 0.25 * n, ...(o.fx || {}) }]] }),
    breath: (elem, n, cell = 'flame', impact = 'blast') => ({ elem, charge: 'mouth', release: 0.25, impact, shake: SH[n], fx: [['breath', { dur: 0.3 + 0.12 * n, width: 0.2 + 0.12 * n, cell, size: 0.3 + 0.06 * n, speed: 10 }]] }),
    beam: (elem, n, impact = 'blast') => ({ elem, charge: 'hands', release: 0.28, impact, shake: SH[n], fx: [['beam', { width: 0.12 + 0.07 * n, dur: 0.35 + 0.05 * n, charge: 0.1 }]] }),
    bolt: (elem, n) => ({ elem, charge: 'sky', release: 0.22, impact: 'zap', shake: SH[n], fx: [['bolt', { from: n >= 2 ? 'sky' : 'caster', width: 0.06 + 0.02 * n, count: n >= 3 ? 2 : 1 }]] }),
    slash: (elem, n, claw = false, impact = 'hit') => ({ elem, charge: 'hands', release: 0.2, impact, shake: SH[n], fx: [['slash', { count: n + (claw ? 1 : 0), claw, size: 0.9 + 0.25 * n, spread: 0.4 }]] }),
    punch: (elem, n, impact = 'hit') => ({ elem, charge: 'hands', release: 0.2, impact, shake: SH[n] + 0.03, fx: [['streak', { width: 0.4 + 0.1 * n, dur: 0.2, trail: 'wind' }], ['slash', { at: 0.08, count: 1, size: 1 + 0.3 * n, spread: 0.2 }]] }),
    missiles: (elem, n, impact = 'blast') => ({ elem, charge: 'back', release: 0.25, impact, shake: SH[n], fx: [['missiles', { count: 1 + n, size: 0.12 + 0.03 * n, from: 'chest', curve: 1, arc: 1, dur: 0.6, impactSize: 1 + 0.2 * n }]] }),
    erupt: (elem, n, kind = 'rock') => ({ elem, charge: 'ground', release: 0.3, impact: elem === 'fire' ? 'blast' : 'hit', shake: SH[n] + 0.04, fx: [['shock', { cell: 'spark' }], ['erupt', { at: 0.15, kind, to: 'center', area: n >= 2, count: 2 + n, height: 0.8 + 0.3 * n }]] }),
    slam: (elem, n, impact = 'hit') => ({ elem, charge: 'sky', release: 0.3, impact, shake: SH[n] + 0.08, fx: [['shock', { hammer: n >= 2, cell: 'spark' }]] }),
    heal: (elem, n, cell = 'cross') => ({ elem, charge: 'aura', release: 0.25, impact: 'none', fx: [['heal', { cell, size: 0.8 + 0.15 * n }]] }),
    dome: (elem, n) => ({ elem, charge: 'aura', release: 0.2, impact: 'none', fx: [['dome', { size: 0.9 + 0.2 * n }]] }),
    arrows: (elem, n) => ({ elem, charge: 'hands', release: 0.25, impact: 'holy', shake: SH[n], fx: [['arrows', { count: 2 + 2 * n, size: 0.8 + 0.15 * n }]] }),
    swarm: (elem, n, cell, impact = 'hit') => ({ elem, charge: 'aura', release: 0.22, impact, shake: SH[n], fx: [['swarm', { cell, count: 6 + 4 * n, size: 0.25 + 0.05 * n, speed: 8, wobble: 0.8, spread: 0.5 }]] }),
    wave: (elem, n, impact = 'blast') => ({ elem, charge: 'hands', release: 0.25, impact, shake: SH[n], fx: [['wave', { width: 0.6 + 0.2 * n, count: n >= 2 ? 2 : 1, dur: 0.5 }]] }),
    tornado: (elem, n, cell = 'leaf') => ({ elem, charge: 'aura', release: 0.25, impact: 'wind', shake: SH[n], fx: [['tornado', { cell, height: 1.5 + 0.5 * n, dur: 0.8, size: 0.8 + 0.2 * n }]] }),
    volley: (elem, n, cell = 'spark') => ({ elem, charge: 'hands', release: 0.2, impact: 'hit', shake: SH[n], fx: [['volley', { count: 6 + 4 * n, rate: 0.03, cell, size: 0.35, spread: 0.4, from: 'chest', speed: 18 }]] }),
    dash: (elem, n, claw = true) => ({ elem, charge: 'none', release: 0.15, impact: 'hit', shake: SH[n], fx: [['dash', { ghosts: 3 + n, slashes: 1 + n, claw, dur: 0.3, size: 1 + 0.2 * n }]] }),
    pillar: (elem, n) => ({ elem, charge: 'sky', release: 0.3, impact: 'holy', shake: SH[n] + 0.03, fx: [['pillar', { size: 0.8 + 0.25 * n, height: 6, dur: 0.6 }]] }),
    whip: (elem, n, impact = 'poison') => ({ elem, charge: 'hands', release: 0.2, impact, shake: SH[n], fx: [['whip', { dur: 0.4, width: 0.08 + 0.02 * n, count: n, sway: 1.1 }]] }),
    meteor: (elem, n) => ({ elem, charge: 'sky', release: 0.3, impact: 'blast', shake: SH[n] + 0.06, fx: [['meteor', { size: 0.4 + 0.15 * n, count: n, height: 9, dur: 0.6, trail: 'flame' }]] }),
    sphere: (elem, n, cell = 'glow', impact = 'blast') => ({ elem, charge: 'hands', release: 0.25, impact, shake: SH[n] + 0.03, fx: [['sphere', { size: 0.3 + 0.12 * n, grow: 0.3, from: 'hands', trail: 'spark', cell, impactSize: 1 + 0.25 * n }]] }),
    spikes: (elem, n, kind = 'rock') => ({ elem, charge: 'ground', release: 0.25, impact: 'hit', shake: SH[n], fx: [['spikes', { count: 3 + n, kind, size: 0.6 + 0.15 * n }]] }),
    net: (elem, n) => ({ elem, charge: 'mouth', release: 0.2, impact: 'poison', shake: SH[n], fx: [['net', { size: 0.8 + 0.2 * n }]] }),
    drain: (elem, n) => ({ elem, charge: 'hands', release: 0.25, impact: 'dark', shake: SH[n], fx: [['beam', { width: 0.12, dur: 0.4, charge: 0.1 }], ['drain', { at: 0.2 }]] }),
    cross: (elem, n) => ({ elem, charge: 'sky', release: 0.3, impact: 'holy', shake: SH[n] + 0.04, fx: [['cross', { size: 0.9 + 0.25 * n, dur: 0.6 }]] }),
    wheel: (elem, n) => ({ elem, charge: 'hands', release: 0.22, impact: 'metal', shake: SH[n], fx: [['wheel', { size: 0.5 + 0.15 * n, cell: 'gear', dur: 0.5, saw: true }]] }),
    vortex: (elem, n) => ({ elem, charge: 'ground', release: 0.3, impact: 'dark', shake: SH[n] + 0.05, fx: [['vortex', { size: 1 + 0.3 * n, dur: 0.8 }]] }),
    rings: (elem, n) => ({ elem, charge: 'hands', release: 0.2, impact: 'holy', shake: SH[n], fx: [['rings', { count: 1 + n, size: 0.4 + 0.1 * n }]] }),
    // summon casts: a seal at the caster (no projectile)
    glyphy: (elem, n) => ({ elem, charge: 'aura', release: 0.25, impact: 'none', shake: SH[n], fx: [['glyph', { glyph: n >= 3 ? 'tri' : 'circle', size: 1 + 0.25 * n, dur: 0.6, place: 'ground' }], ['shock', { at: 0.3, cell: 'spark', debris: false, to: 'self' }]] }),
  };
  const T = (b, ...a) => B[b](...a);
  const moves = {
    // 1 cost
    'Blazing Ice': T('orb', 'ice', 1, 'ice', { impact: 'frost' }), 'Ninja Fist': T('punch', 'beast', 2), 'Bunny Blades': T('slash', 'metal', 3, false, 'metal'),
    'Pepper Breath': T('orb', 'fire', 1), 'Mega Flame': T('breath', 'fire', 2), 'Giga Destroyer': T('missiles', 'fire', 3),
    'Gear Stinger': T('orb', 'metal', 1, 'spark', { fx: { head: 'needle' } }), 'Turbo Stinger': T('missiles', 'metal', 2), 'Nitro Stinger': T('missiles', 'fire', 3),
    'Crazy Giggle': T('swarm', 'dark', 1, 'bat', 'dark'), 'Data Crusher': T('beam', 'dark', 2, 'dark'), "Hell's Grenade": T('missiles', 'dark', 3, 'dark'),
    'Cog Crusher': T('wheel', 'metal', 1), 'Destruction Grenade': T('missiles', 'metal', 2), 'Gatling Missile': T('missiles', 'metal', 3),
    'Eye of Nightmares': T('beam', 'dark', 1, 'dark'), 'Sticker Blade': T('slash', 'dark', 2, false, 'dark'), 'Thousand Arrows': T('arrows', 'dark', 3),
    'Bullet Whirlwind': T('heal', 'wind', 1, 'leaf'), 'Lion Claw': T('heal', 'holy', 2, 'cross'), 'Knowledge of Swiftness': T('heal', 'holy', 3, 'star'),
    'Corona Flame': T('orb', 'fire', 1), 'Flame Dive': T('slam', 'fire', 2, 'blast'), 'Phoebus Blow': T('erupt', 'fire', 3, 'flame'),
    'Super Shocker': T('bolt', 'thunder', 1), 'Electro Shocker': T('sphere', 'thunder', 2, 'bolt', 'zap'), 'Horn Buster': T('bolt', 'thunder', 3),
    'Electro Strike': T('orb', 'thunder', 1, 'zap', { impact: 'zap' }), 'Hyper Cannon': T('volley', 'metal', 2), 'Energy Bomb': T('sphere', 'metal', 3, 'glow'),
    'Rock Fist': T('punch', 'metal', 1), 'Golem Punch': T('punch', 'metal', 2), 'Big Bang Blow': T('erupt', 'fire', 3, 'rock'),
    'Bear Fist': T('punch', 'beast', 1), 'Maul Attack': T('slash', 'beast', 2, true), 'The King of Fists': T('punch', 'beast', 3),
    'Baby Breath': T('breath', 'fire', 1), 'Blue Flare Breath': T('breath', 'water', 2, 'flame', 'blast'), 'Blaze Sonic Breath': T('beam', 'wind', 3, 'wind'),
    'Bonfire': T('orb', 'fire', 1), 'Thunder Cloud': T('bolt', 'thunder', 2), 'Dialogue': T('vortex', 'dark', 3),
    // 2 cost
    'Fifth Rush': T('dash', 'metal', 1, false), 'Fif Cross': T('slash', 'metal', 2, false, 'metal'), 'Meteor Flame': T('meteor', 'fire', 3),
    'Goblin Strike': T('punch', 'beast', 1), 'Kirisute Gomen': T('slash', 'metal', 2, false, 'metal'), 'Vorpal Blade': T('slash', 'fire', 3, false, 'blast'),
    'Puppy Howl': T('wave', 'holy', 1, 'holy'), 'Lightning Paw': T('punch', 'thunder', 2, 'zap'), 'Celestial Arrow': T('arrows', 'holy', 3),
    'Poison Ivy': T('whip', 'plant', 1), 'Needle Spray': T('volley', 'plant', 2, 'needle'), 'Flower Cannon': T('beam', 'plant', 3, 'leaf'),
    'Spitfire': T('orb', 'fire', 1), 'Great Antler': T('punch', 'fire', 2), 'Dark Terra Force': T('sphere', 'dark', 3, 'glow', 'dark'),
    'Bad Message': T('dome', 'dark', 1), "Hell's Hand": T('dome', 'dark', 2), 'Soul Chopper': T('dome', 'dark', 3),
    'Nuts Gun': T('orb', 'plant', 1, 'leaf', { impact: 'leaf', fx: { head: 'seed' } }), 'Sunshine Beam': T('beam', 'holy', 2, 'holy'), 'Lila Shower': T('swarm', 'plant', 3, 'petal', 'leaf'),
    'Downward Cleave': T('dash', 'fire', 1, false), 'Burning Fist': T('dash', 'fire', 2, false), 'Heavy Metal Fire': T('dash', 'fire', 3, false),
    'Fire Vortex': T('tornado', 'fire', 1, 'flame'), 'Spiral Shredder': T('beam', 'wind', 2, 'wind'), 'Tenryu Slash': T('wave', 'holy', 3, 'holy'),
    'Rain of Pollen': T('swarm', 'plant', 1, 'petal', 'poison'), "Cat's Eye": T('glyphy', 'dark', 2), 'Darkness Wave': T('swarm', 'dark', 3, 'bat', 'dark'),
    'Marching Fishes': T('swarm', 'water', 1, 'fish', 'splash'), 'Harpoon Torpedo': T('missiles', 'water', 2, 'splash'), "Vulcan's Hammer": T('slam', 'metal', 3, 'metal'),
    'Shiny Ring': T('rings', 'holy', 1), 'Chrono Breaker': T('wheel', 'metal', 2), 'Digital Bomb': T('sphere', 'dark', 3, 'square', 'dark'),
    'Electric Thread': T('net', 'thunder', 1), 'Poison Powder': T('swarm', 'plant', 2, 'dot', 'poison'), 'Spider Thread': T('net', 'dark', 3),
    'Retrieve Bark': T('wave', 'beast', 1, 'hit'), 'Grau Lärm': T('wave', 'dark', 2, 'dark'), 'Mirage Image': T('sphere', 'holy', 3, 'star', 'holy'),
    'Tearing Shot': T('orb', 'water', 1, 'bubble', { impact: 'splash' }), 'Tear Arrow': T('arrows', 'water', 2), 'Arrow of Artemis': T('arrows', 'ice', 3),
    // 3 cost
    'Spiral Edge': T('dome', 'water', 1), 'Rotten Tackle': T('dome', 'dark', 2), 'Dark Shot': T('dome', 'dark', 3),
    'Sticky Net': T('heal', 'plant', 1, 'leaf'), 'Spiking Strike': T('heal', 'plant', 2, 'star'), 'Spike Buster': T('heal', 'holy', 3, 'cross'),
    'Spiral Twister': T('orb', 'plant', 1, 'leaf'), 'Meteor Wing': T('meteor', 'fire', 2), 'Shadow Wing': T('orb', 'fire', 3, 'flame', { fx: { wings: true } }),
    'Blue Blaster': T('dome', 'ice', 1), 'Fox Fire': T('dome', 'ice', 2), 'Wolf Claw': T('dome', 'beast', 3),
    'Air Shot': T('heal', 'wind', 1, 'feather'), "Heaven's Knuckle": T('heal', 'holy', 2, 'cross'), "Heaven's Gate": T('heal', 'holy', 3, 'star'),
    'Scratch Smash': T('slash', 'beast', 1, true), 'Thousand Beaks': T('slash', 'wind', 2, false, 'wind'), 'Savage Emperor': T('slash', 'fire', 3, false, 'blast'),
    'Snow Goblin Volt': T('bolt', 'ice', 1), 'Throwing Ice': T('orb', 'ice', 2, 'ice', { impact: 'frost' }), 'Blizzard Breath': T('breath', 'ice', 3, 'snow', 'frost'),
    'Electric Stun Blast': T('sphere', 'thunder', 1, 'bolt', 'zap'), 'Death Claw': T('slash', 'dark', 2, true, 'dark'), 'Skull Hammer': T('erupt', 'dark', 3, 'dark'),
    'M16 Assassin': T('orb', 'metal', 1, 'smoke', { fx: { speed: 26 } }), 'Death Behind': T('orb', 'dark', 2, 'dark', { fx: { speed: 26 } }), 'Gigastick Lance': T('beam', 'dark', 3, 'dark'),
    'Crack Bite': T('slash', 'beast', 1, true), 'Fang Strike': T('slash', 'fire', 2, true, 'poison'), 'Desolation Claw': T('slash', 'metal', 3, true, 'poison'),
    'Feather Slash': T('swarm', 'wind', 1, 'feather', 'wind'), 'Blast Laser': T('beam', 'holy', 2), 'Top Gun': T('beam', 'wind', 3, 'wind'),
    'Fungus Crusher': T('drain', 'plant', 1), 'Branch Drain': T('drain', 'plant', 2), 'Cherry Bomb': T('drain', 'plant', 3),
    'Fire Ball': T('dome', 'fire', 1), 'Exhaust Flame': T('dome', 'fire', 2), 'Double Edge': T('dome', 'metal', 3),
    'Dash Metal': T('glyphy', 'metal', 1), 'Power Metal': T('glyphy', 'metal', 2), 'Metal Meteor': T('glyphy', 'metal', 3),
    // 4 cost
    'Nightmare Syndrome': T('dome', 'dark', 1), 'Twinkle Beam': T('dome', 'metal', 2), 'Ultimate Slicer': T('dome', 'metal', 3),
    'Diamond Shell': T('erupt', 'metal', 1, 'rock'), 'Tail Hammer': T('erupt', 'metal', 2, 'rock'), 'Tri-Horn Attack': T('erupt', 'thunder', 3, 'rock'),
    'Bada Boom': T('orb', 'fire', 1), 'Darkness Claw': T('slash', 'dark', 2, true, 'dark'), 'Death Slinger': T('volley', 'dark', 3, 'ember'),
    'Boom Boom Punch': T('dome', 'beast', 1), 'Vee Laser': T('dome', 'holy', 2), 'Death Parade Blaster': T('dome', 'holy', 3),
    'Mad Twister': T('wave', 'wind', 1, 'wind'), 'Crystal Barrage': T('beam', 'ice', 2, 'frost'), 'Fire Fist of Shiva': T('wave', 'fire', 3, 'blast'),
    'Meteor Slash': T('slam', 'metal', 1, 'metal'), 'Wheel Rush': T('slam', 'metal', 2, 'metal'), 'Grand Smash': T('slam', 'metal', 3, 'metal'),
    'Rolling Upper': T('glyphy', 'beast', 1), 'Gaoga Hound': T('glyphy', 'beast', 2), 'Winning Knuckle': T('glyphy', 'metal', 3),
    'Electric Shock': T('orb', 'thunder', 1, 'zap', { impact: 'zap' }), 'Ice Blast': T('orb', 'ice', 2, 'ice', { impact: 'frost' }), 'Lightning Javelin': T('orb', 'thunder', 3, 'zap', { impact: 'zap' }),
    'Demi Darts': T('drain', 'dark', 1), 'Necro Magic': T('drain', 'dark', 2), 'Crimson Lightning': T('drain', 'dark', 3),
    'Diamond Storm': T('dash', 'holy', 1, false), 'Fox Tail Inferno': T('dash', 'fire', 2, false), 'Talisman of Light': T('dash', 'holy', 3, false),
    'Worm Venom': T('dome', 'plant', 1), 'Scissor Claw': T('dome', 'beast', 2), 'Dimension Scissor': T('dome', 'dark', 3),
    'Colored Spark': T('beam', 'holy', 1), 'Baluluna Gale': T('wave', 'wind', 2, 'wind'), 'Maverick': T('beam', 'dark', 3, 'dark'),
    // 5 cost
    'Cannonball': T('orb', 'metal', 1, 'smoke', { impact: 'metal' }), 'Bloody Cave': T('swarm', 'dark', 2, 'skull', 'dark'), 'Metal Impulse': T('missiles', 'dark', 3, 'dark'),
    'Great Flame': T('erupt', 'fire', 2, 'flame'), 'Volcanic Flare': T('erupt', 'fire', 3, 'flame'),
    'Laser Eye': T('beam', 'metal', 1), 'Messer Spiral': T('missiles', 'metal', 3),
    'Terrier Tornado': T('tornado', 'wind', 1, 'leaf'), 'Gargo Laser': T('volley', 'metal', 2, 'spark'), 'Giant Missile': T('missiles', 'fire', 3),
    'Grand Cross': T('dome', 'holy', 1), 'Paradise Lost': T('dome', 'dark', 2), 'Purgatorial Flame': T('dome', 'fire', 3),
    'Black Pearl Blast': T('slam', 'water', 1, 'splash'), 'Tidal Wave': T('slam', 'water', 2, 'splash'), 'Sorrow Blue': T('slam', 'water', 3, 'splash'),
    'Sparkling Thunder': T('bolt', 'thunder', 1), 'Stun Beat Blow': T('bolt', 'thunder', 2), 'Mabolt': T('bolt', 'thunder', 3),
    // summons and creeps
    'Bubble': T('orb', 'water', 1, 'bubble', { impact: 'splash' }), 'Sweet Breath': T('breath', 'plant', 1, 'petal', 'poison'), 'Blizzard Blaster': T('orb', 'ice', 2, 'ice', { impact: 'frost' }),
    'Nume-Sludge': T('orb', 'dark', 1, 'smoke', { impact: 'poison' }), 'Symphony Crusher': T('swarm', 'water', 1, 'note', 'splash'), 'Howling Blaster': T('breath', 'ice', 2, 'snow', 'frost'),
    'Pummel Whack': T('punch', 'beast', 2), 'Evil Hurricane': T('tornado', 'dark', 2, 'smoke'), 'Blaze Blast': T('breath', 'fire', 2), 'Nuclear Laser': T('beam', 'fire', 3),
    'Volcanic Strike': T('erupt', 'fire', 2, 'flame'), 'Tusk Crusher': T('slam', 'ice', 3, 'frost'), 'Spinning Needle': T('tornado', 'wind', 2, 'needle'),
    'Genocide Gear': T('missiles', 'metal', 3), 'Catastrophe Cannon': T('beam', 'dark', 3, 'dark'), 'Infinity Cannon': T('beam', 'metal', 3, 'metal'),
    'Heat Viper': T('beam', 'fire', 3), 'Time Unlimited': T('vortex', 'dark', 3), 'Total Annihilation': T('vortex', 'dark', 3),
  };
  // transform casts: the dome reads as "power up"; add a glyph under it
  for (const n of ['Blue Blaster', 'Fox Fire', 'Wolf Claw', 'Fire Ball', 'Exhaust Flame', 'Double Edge', 'Nightmare Syndrome', 'Twinkle Beam', 'Ultimate Slicer',
    'Boom Boom Punch', 'Vee Laser', 'Death Parade Blaster', 'Worm Venom', 'Scissor Claw', 'Dimension Scissor', 'Grand Cross', 'Paradise Lost', 'Purgatorial Flame']) {
    moves[n].fx.push(['glyph', { at: 0.1, glyph: 'circle', size: 1.3, dur: 0.6, place: 'ground' }]);
  }

  // ---------- basic attacks ----------
  const MELEE = { Guardian: 'punch', Bruiser: 'punch', Striker: 'claw', Duelist: 'slash', Assassin: 'slash' };
  const SHOT = {
    ac_fanbeemon: ['metal', 'needle'], ac_dracmon: ['dark', 'blade'], ac_kudamon: ['wind', 'leaf'], ac_tentomon: ['thunder', 'bolt'], ac_kokuwamon: ['thunder', 'spark'],
    ac_dracomon: ['ice', 'snow'], ac_candlemon: ['fire', 'flame'], ac_salamon: ['holy', 'star'], ac_palmon: ['plant', 'seed'], ac_tsukaimon: ['dark', 'bat'],
    ac_lalamon: ['plant', 'seed'], ac_gumdramon: ['fire', 'ember'], ac_kunemon: ['thunder', 'spark'], ac_labramon: ['beast', 'paw'], ac_lunamon: ['water', 'drop'],
    ac_wormmon: ['plant', 'leaf'], ac_biyomon: ['fire', 'flame'], ac_snowgoblimon: ['ice', 'snow'], ac_gazimon: ['dark', 'skull'], ac_commandramon: ['metal', 'streak'],
    ac_hawkmon: ['wind', 'feather'], ac_dorumon: ['metal', 'shard'], ac_armadillomon: ['metal', 'shard'], ac_shamanmon: ['ice', 'shard'], ac_betamon: ['thunder', 'bolt'],
    ac_demidevimon: ['dark', 'needle'], ac_psychemon: ['holy', 'star'], ac_dexdorugamon: ['dark', 'shard'], ac_jazamon: ['metal', 'streak'], ac_terriermon: ['metal', 'spark'],
    ac_elecmon: ['thunder', 'bolt'], ac_s_vegiemon: ['plant', 'seed'], ac_w_gekomon: ['water', 'note'], ac_w_airdramon: ['wind', 'needle'], ac_w_gigadramon: ['metal', 'streak'],
  };
  const MELEE_KIND = { ac_bearmon: 'punch', ac_gotsumon: 'punch', ac_agumon: 'bite', ac_blackagumon: 'claw', ac_hackmon: 'slash', ac_goblimon: 'punch', ac_gomamon: 'tail',
    ac_hyokomon: 'peck', ac_floramon: 'claw', ac_keramon: 'bite', ac_falcomon: 'peck', ac_monodramon: 'claw', ac_renamon: 'kick', ac_lopmon: 'kick', ac_gabumon: 'claw',
    ac_dokunemon: 'horn', ac_guilmon: 'claw', ac_impmon: 'claw', ac_veemon: 'punch', ac_kotemon: 'slash', ac_gaomon: 'punch', ac_lucemon: 'kick', ac_syakomon: 'tail',
    ac_patamon: 'punch', ac_mushroomon: 'punch', ac_gizamon: 'bite', ac_vorvomon: 'tail', ac_coronamon: 'punch', ac_tapirmon: 'horn', ac_hagurumon: 'punch', ac_solarmon: 'punch',
    ac_s_kumamon: 'punch', ac_s_tanemon: 'bite', ac_w_blackgarurmon: 'bite', ac_w_monochromon: 'horn', ac_w_mammothmon: 'horn', ac_w_tyrannomon: 'bite' };
  const basic = {};
  for (const l of (DT.Sets.def('autochess') || { lines: [] }).lines) {
    const ranged = (l.range || (DT.BASE_ROLES[l.role] || {}).range || 1) > 1;
    if (ranged) {
      const [elem, head] = SHOT[l.id] || ['metal', 'spark'];
      basic[l.id] = { kind: 'shot', elem, head, trail: elem === 'fire' ? 'flame' : elem === 'ice' ? 'ice' : elem === 'dark' ? 'dark' : elem === 'plant' ? 'leaf' : elem === 'thunder' ? 'zap' : elem === 'holy' ? 'holy' : 'spark', size: 0.2 };
    } else basic[l.id] = { kind: MELEE_KIND[l.id] || MELEE[l.role] || 'claw' };
  }

  DT.Sets.extend('autochess', { art: { items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
