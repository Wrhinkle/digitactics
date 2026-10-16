// Into the Arcane art: item, Crest, augment, Anomaly and offer-card portraits (SVG painters on the js/itemart.js kit),
// move looks for every ability and a basic attack for every line (js/moves.js spec format). Painters only run in the
// browser; this file just registers them, so it also loads headless.
(function (G) {
  const DT = G.DT;

  // ---------- item portraits ----------
  const COMP = {
    arc_sword: '#ff7a45', arc_bow: '#ffd23f', arc_rod: '#b26bff', arc_tear: '#4fb3ff', arc_vest: '#c0c6d0',
    arc_cloak: '#3fd6b0', arc_belt: '#ff4d64', arc_glove: '#8fdc4e', arc_spat: '#ffe27a', arc_pan: '#9fdcff',
  };
  // component signatures, drawn at the origin about 80 px wide; completed items reuse two of them
  const MOTIF = {
    arc_sword: (p, k) => { k.M.chip(p, COMP.arc_sword, 56); p.glow(0, 0, 20, '#ffcf8a', 0.7); p.at(0, 0, 1.05, 45, () => k.M.miniBlade(p)); },
    arc_bow: (p, k) => {
      p.at(-14, 0, 0.75, 0, () => k.M.wing(p, '#fff6c8')); p.at(14, 0, 0.75, 0, () => k.M.wing(p, '#fff6c8'), -1);
      p.part(k.rr(-19, -19, 38, 38, 7), COMP.arc_bow);
      p.glow(0, 0, 15, '#fff3a0', 0.8);
      p.at(0, 0, 0.58, 0, () => p.part(k.BOLT, [[0, '#ffffff'], [1, COMP.arc_bow]], { ow: 5 }));
    },
    arc_rod: (p, k) => { p.glow(0, 0, 32, COMP.arc_rod, 0.7); k.M.crystal(p, COMP.arc_rod, 56, 15); p.sparkle(18, -22, 6); },
    arc_tear: (p, k) => {
      k.M.card(p, COMP.arc_tear, 46, 64);
      p.glow(0, 4, 22, '#bfe6ff', 0.8);
      p.at(0, 4, 1, 0, () => { p.part(k.DROP, [[0, '#ffffff'], [0.6, '#9fdcff'], [1, '#2a7fd0']], { ow: 4.5 }); p.gloss(-4, 3, 3, 7, 20, 0.95); });
    },
    arc_vest: (p, k) => p.at(0, -2, 0.92, 0, () => k.M.plate(p)),
    arc_cloak: (p, k) => p.at(0, 0, 0.9, 0, () => { k.M.hood(p, COMP.arc_cloak, '#bdfff0'); }),
    arc_belt: (p, k) => { k.M.disk(p, COMP.arc_belt, 38); p.at(0, 1, 0.28, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); },
    arc_glove: (p, k) => p.at(0, 0, 0.9, 0, () => { k.M.glove(p, COMP.arc_glove, '#f2f5e6'); p.at(1, 6, 1.05, 0, () => k.M.clover(p)); }),
    arc_spat: (p, k) => p.at(0, 0, 0.95, 0, () => { k.M.tag(p, '#ffe27a'); p.at(0, 0, 0.8, 0, () => p.part(k.star(0, 0, 14, 6, 5), '#ffcf4a', { ow: 4 })); }),
    arc_pan: (p, k) => p.at(0, 0, 0.9, 0, () => k.M.digivice(p, '#eef1f6', '#4fb3ff', '#7dffb0')),
  };
  const component = (id) => (p, k) => { k.frame(p, 'square', 'bronze', [COMP[id]], { w: 6 }); p.obj(() => p.at(64, 64, 1, 0, () => MOTIF[id](p, k))); };

  // completed items: gold frame split in the recipe colors, both component signatures, and an effect badge
  const BADGE = {
    arc_transcendent_sword: ['sword', '#ff9a5a'], arc_gram_lance: ['crosshair', '#ffd23f'], arc_durandal: ['blood', '#ff5a6a'], arc_mana_lance: ['drop', '#4fb3ff'],
    arc_shadow_mantle: ['moon', '#a08cff'], arc_crimson_fang: ['blood', '#ff3b4e'], arc_berserk_gauntlet: ['up', '#ff7a45'], arc_ultimate_edge: ['star5', '#ffe27a'],
    arc_burning_drive: ['flame', '#ff7a1a'], arc_rage_drive: ['chevrons', '#ffb04a'], arc_thunder_chip: ['dtector', '#ffe94a'], arc_titan_armor: ['shieldIron', '#d9dfe9'],
    arc_twin_arrows: ['copy', '#ffd23f'], arc_fang_charm: ['chevrons', '#b26bff'], arc_armor_piercer: ['dagger', '#c0c6d0'], arc_grand_grimoire: ['star6', '#d48bff'],
    arc_angel_staff: ['sun', '#fff0a0'], arc_crown_guard: ['shield', '#ffe27a'], arc_ionic_barrier: ['xros', '#7fdcff'], arc_hellfire_tome: ['flame', '#ff5a1a'],
    arc_crest_gauntlet: ['star5', '#e58bff'], arc_blue_card: ['drop', '#3a8bff'], arc_oath_plate: ['shield', '#9fdcff'], arc_mode_helm: ['refresh', '#3fd6b0'],
    arc_healing_ring: ['plus', '#7dff9a'], arc_justice_fist: ['badge', '#ffcf4a'], arc_spike_shell: ['star6', '#c0c6d0'], arc_stone_shell: ['shieldIron', '#a0a8b8'],
    arc_flame_mantle: ['sun', '#ff7a1a'], arc_iron_will: ['shield', '#8fdc4e'], arc_dragon_hide: ['plus', '#3fd6b0'], arc_shroud_veil: ['moon', '#5fd6c8'],
    arc_quick_chip: ['chevrons', '#9be15d'], arc_giga_heart: ['plus', '#ff4d64'], arc_guard_smasher: ['shieldIron', '#ff8f5a'], arc_thief_program: ['copy', '#8fdc4e'],
  };
  const badge = (p, k, sym, col) => {
    p.part(k.circ(101, 27, 17), [[0, k.lt(col, 0.4)], [1, k.dk(col, 0.55)]], { ow: 4 });
    p.at(101, 27, 0.62, 0, () => (k.SYM[sym] || k.SYM.star6)(p, k.lt(col, 0.25)));
  };
  const completed = (id) => (p, k) => {
    const it = DT.ITEMS[id];
    const [a, b] = it.from;
    k.frame(p, 'square', 'gold', [COMP[a], COMP[b]], { w: 8 });
    p.obj(() => {
      p.at(48, 56, 0.6, -10, () => MOTIF[a](p, k));
      p.at(80, 80, 0.6, 10, () => MOTIF[b](p, k));
      p.glow(64, 66, 20, '#ffffff', 0.35);
      const [sym, col] = BADGE[id] || ['star6', '#ffffff'];
      badge(p, k, sym, col);
    });
  };

  // Crests: faction Crests on a shield, class Crests on a hex with a Digivice ring
  const CREST_SYM = { ACA: 'star6', AUT: 'chip', BR: 'moon', CON: 'sword', ENF: 'badge', EXP: 'bug', FAM: 'plus', FIRE: 'sun', REB: 'flame', SCR: 'chip',
    AMB: 'dagger', ART: 'crosshair', BRU: 'shield', DOM: 'star5', PIT: 'blood', QS: 'chevrons', SEN: 'shieldIron', SNI: 'crosshair', SOR: 'star6', VIS: 'pearl', WAT: 'shield' };
  const CLASS = ['AMB', 'ART', 'BRU', 'DOM', 'PIT', 'QS', 'SEN', 'SNI', 'SOR', 'VIS', 'WAT'];
  const crest = (t) => (p, k) => {
    const T = DT.TRAITS[t] || { color: '#ffffff' };
    if (!CLASS.includes(t)) { k.crest(p, T.color, CREST_SYM[t]); if (t === 'FAM') p.obj(() => p.at(64, 64, 0.32, 0, () => k.M.heart(p, '#ff7a8a', 1, 8))); return; }
    k.frame(p, 'hex', 'silver', [T.color], { w: 7 });
    p.obj(() => p.at(64, 64, 1, 0, () => {
      p.tube(k.circ(0, 0, 34), 6, k.lt(T.color, 0.2), { ow: 4 });
      p.glow(0, 0, 28, T.color, 0.55);
      p.part(k.ngon(0, 0, 24, 6), [[0, k.lt(T.color, 0.5)], [1, k.dk(T.color, 0.5)]], { ow: 4 });
      p.at(0, 0, 0.95, 0, () => (k.SYM[CREST_SYM[t]] || k.SYM.star6)(p, '#ffffff'));
    }));
  };

  // painters keyed by a small shape description: frame, metal, background, then a symbol or a motif
  const iconOf = (shape, metal, bg, draw) => (p, k) => { k.frame(p, shape, metal, bg, { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => draw(p, k))); };
  const symIcon = (shape, metal, bg, sym, col, s = 1.7) => iconOf(shape, metal, [bg], (p, k) => { p.glow(0, 0, 36, col, 0.5); p.at(0, 0, s, 0, () => (k.SYM[sym] || k.SYM.star6)(p, col)); });

  const ITEMS = {};
  for (const id of Object.keys(COMP)) ITEMS[id] = component(id);
  for (const id of Object.keys(BADGE)) ITEMS[id] = completed(id);
  for (const t of Object.keys(CREST_SYM)) ITEMS[`arc_crest_${t.toLowerCase()}`] = crest(t);

  // Tamer items
  ITEMS.arc_tamer_crown = iconOf('square', 'gold', ['#ffd84a', '#fff3b0'], (p, k) => {
    p.glow(0, 4, 40, '#fff3a0', 0.6);
    p.part('M-34 18L-38 -18L-18 -2L0 -30L18 -2L38 -18L34 18Z', 'gold');
    p.part(k.rr(-36, 14, 72, 14, 5), 'gold', { ow: 5 });
    p.gem(0, -4, 7, '#ff4d64', 'diamond', 4); p.gem(-20, 8, 5, '#4fb3ff', 'round', 3.5); p.gem(20, 8, 5, '#7dff9a', 'round', 3.5);
  });
  ITEMS.arc_tamer_shield = iconOf('square', 'gold', ['#9fdcff', '#e8f4ff'], (p, k) => { p.at(0, 0, 2.3, 0, () => k.SYM.shield(p, '#d9ecff')); p.at(0, -2, 0.55, 0, () => k.M.digivice(p, '#eef1f6', '#4fb3ff', '#7dffb0')); });
  ITEMS.arc_tamer_cape = iconOf('square', 'gold', ['#ffe27a', '#9fdcff'], (p, k) => { p.at(0, 2, 0.95, 0, () => k.M.hood(p, '#c84a6a', '#ffe27a')); p.sparkle(26, -30, 7); });

  // artifacts: round prism frame
  const ART_SYM = {
    arc_regen_core: ['plus', '#5adf8a'], arc_doom_seal: ['skull', '#c98bff'], arc_gatling_arm: ['crosshair', '#ffb04a'], arc_bounty_chip: ['coin', '#ffd23f'],
    arc_lone_plate: ['shieldIron', '#d9dfe9'], arc_infinity_core: ['star6', '#ff8fd8'], arc_spell_blade: ['sword', '#c98bff'], arc_overkill_cannon: ['sun', '#ff5a3a'],
    arc_long_barrel: ['chevrons', '#ffe94a'], arc_scope_visor: ['crosshair', '#5ad1ff'], arc_ascension_talisman: ['up', '#fff0a0'], arc_time_stopper: ['refresh', '#7fdcff'],
  };
  for (const [id, [sym, col]] of Object.entries(ART_SYM)) {
    ITEMS[id] = iconOf('circle', 'prism', [col], (p, k) => {
      p.glow(0, 0, 40, col, 0.55);
      p.part(k.ngon(0, 0, 34, 8, -67.5), [[0, k.lt(col, 0.45)], [0.5, k.dk(col, 0.35)], [1, k.dk(col, 0.7)]], { ow: 5 });
      p.at(0, 0, 1.25, 0, () => (k.SYM[sym] || k.SYM.star6)(p, k.lt(col, 0.35)));
      p.sparkle(24, -26, 6);
    });
  }

  // contraband: dark hex with a chem vial; tier II gets a second vial and studs
  const CB = { tank: ['shield', '#7ed957'], blade: ['dagger', '#ff5d5d'], ripper: ['blood', '#ff9e5a'], hex: ['shieldIron', '#d9dfe9'],
    bloom: ['pearl', '#e05ad8'], toxins: ['skull', '#b6d43a'], virus: ['bug', '#9b4dff'], saber: ['dtector', '#ffe94a'] };
  const vial = (p, k, col, x, s) => p.at(x, 6, s, 0, () => {
    p.part('M-9 -30H9V-8L20 22C22 30 16 36 8 36H-8C-16 36 -22 30 -20 22L-9 -8Z', 'glass', { ow: 5 });
    p.part('M-15 10H15L19 22C21 29 16 33 8 33H-8C-16 33 -21 29 -19 22Z', [[0, k.lt(col, 0.5)], [1, k.dk(col, 0.4)]], { ow: 0, hi: false });
    p.part(k.rr(-11, -38, 22, 9, 3), '#5a3324', { ow: 4, hi: false });
    p.glow(0, 20, 14, col, 0.7);
  });
  for (const [base, [sym, col]] of Object.entries(CB)) {
    for (const tier of [1, 2]) {
      const id = `arc_k_${base}${tier === 2 ? '2' : ''}`;
      ITEMS[id] = (p, k) => {
        k.frame(p, 'hex', 'dark', ['#e05ad8'], { w: 7, studs: tier === 2 ? [[64, 9], [64, 119]] : null, studCol: col });
        p.obj(() => p.at(64, 64, 1, 0, () => {
          if (tier === 2) vial(p, k, col, -16, 0.85);
          vial(p, k, col, tier === 2 ? 14 : 0, tier === 2 ? 0.95 : 1.05);
          p.part(k.circ(26, -26, 15), [[0, k.lt(col, 0.3)], [1, k.dk(col, 0.6)]], { ow: 4 });
          p.at(26, -26, 0.55, 0, () => (k.SYM[sym] || k.SYM.star6)(p, '#ffffff'));
        }));
      };
    }
  }

  // consumables
  ITEMS.arc_remover = iconOf('square', 'steel', ['#ff8fa3', '#8a8f9c'], (p, k) => {
    p.tube('M-22 -24V6A22 22 0 0 0 22 6V-24', 14, '#ff5d73', { ow: 5, cap: 'butt' });
    for (const x of [-22, 22]) p.part(k.rr(x - 9, -34, 18, 12, 2), 'chrome', { ow: 4 });
    p.at(0, 30, 0.5, 0, () => k.SYM.up(p, '#ffffff'));
  });
  ITEMS.arc_reforger = iconOf('square', 'steel', ['#ffb347', '#5a6170'], (p, k) => { p.at(0, 0, 1.9, 0, () => k.SYM.refresh(p, '#ffcf8a')); p.sparkle(24, -26, 7); });
  ITEMS.arc_copier = iconOf('square', 'steel', ['#7dffb0', '#2a5a4a'], (p, k) => { p.at(0, 0, 1.8, 0, () => k.SYM.copy(p, '#7dffb0')); p.at(24, 24, 0.6, 0, () => k.SYM.plus(p, '#ffffff')); });
  ITEMS.arc_copier_small = iconOf('square', 'steel', ['#b8ffd8', '#2a5a4a'], (p, k) => { p.at(0, 4, 1.3, 0, () => k.SYM.copy(p, '#b8ffd8')); p.at(24, 24, 0.5, 0, () => k.SYM.plus(p, '#ffffff')); });

  // augments: hex frame by tier
  const AUG = {
    arc_a_coins: ['coin', '#ffd23f'], arc_a_cache: ['chip', '#ff9a45'], arc_a_rolls: ['refresh', '#9fdcff'], arc_a_heart: ['plus', '#ff5d73'],
    arc_a_training: ['sword', '#ff7a45'], arc_a_wind: ['plus', '#7dff9a'], arc_a_feather: ['chevrons', '#fff6c8'], arc_a_front: ['shield', '#d9dfe9'],
    arc_a_back: ['crosshair', '#5ad1ff'], arc_a_shimmer: ['drop', '#e05ad8'], arc_a_bank: ['coin', '#ffe27a'], arc_a_trove: ['chip', '#ffcf4a'],
    arc_a_tag: ['crestTag', '#ffe27a'], arc_a_device: ['dtector', '#9fdcff'], arc_a_income: ['up', '#ffd23f'], arc_a_evolve: ['up', '#c9a3ff'],
    arc_a_blessing: ['sun', '#fff0a0'], arc_a_lotus: ['star5', '#ff8fd8'], arc_a_overcharge: ['drop', '#4fb3ff'], arc_a_find: ['die', '#ffd23f'],
    arc_a_forge: ['star6', '#ff8fd8'], arc_a_unity: ['xros', '#7dffb0'], arc_a_jackpot: ['coin', '#fff3a0'], arc_a_armory: ['chip', '#ff8fd8'],
    arc_a_crown: ['star5', '#ffe27a'], arc_a_mega: ['up', '#ff8fd8'], arc_a_giant: ['shieldIron', '#ff5d73'], arc_a_mutant: ['bug', '#5adf8a'],
    arc_a_workshop: ['chess', '#ffb04a'], arc_a_market: ['skull', '#e05ad8'],
  };
  const AUG_FRAME = ['', 'silver', 'gold', 'prism'];
  const AUG_BG = ['', '#5a6b84', '#8a6420', '#6a3a9a'];
  for (const [id, [sym, col]] of Object.entries(AUG)) {
    ITEMS[`aug:${id}`] = (p, k) => {
      const tier = (DT.AUGMENT && DT.AUGMENT[id] && DT.AUGMENT[id].tier) || 1;
      k.frame(p, 'hex', AUG_FRAME[tier], [AUG_BG[tier]], { w: 7 });
      p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 34, col, 0.55); p.at(0, 0, 1.7, 0, () => (k.SYM[sym] || k.SYM.star6)(p, col)); }));
    };
  }

  // Anomalies (offer cards): X-Antibody chrome X, DNA gold helix, Digital Hazard dark warning sign
  const THEME = { x_: ['chrome', '#2a6bd8', '#9fdcff'], dna_: ['gold', '#2a8a4a', '#7dff9a'], dh_: ['dark', '#a0203a', '#ff5d73'], u_: ['silver', '#5a6b84', '#ffe27a'] };
  const ANOM_SYM = {
    x_power: 'sword', x_guard: 'shieldIron', x_mind: 'star6', x_bulwark: 'shield', x_berserk: 'blood', x_giant: 'up', x_velocity: 'chevrons', x_ascend: 'sun',
    dna_hunger: 'skull', dna_share: 'xros', dna_power: 'sword', dna_energy: 'star6', dna_mana: 'drop', dna_partners: 'copy', dna_familiar: 'egg', dna_shield: 'shield',
    dh_fireball: 'flame', dh_laser: 'crosshair', dh_slime: 'sludge', dh_cooker: 'sun', dh_finisher: 'skull', dh_cuts: 'dagger', dh_dragonsoul: 'flame', dh_lastchance: 'blood',
    dh_thorns: 'star6', u_crest: 'crestTag', u_dual: 'copy',
  };
  const anomaly = (id) => (p, k) => {
    const pre = Object.keys(THEME).find((x) => id.startsWith(x));
    const [metal, bg, col] = THEME[pre];
    k.frame(p, 'circle', metal, [bg], { w: 7 });
    p.obj(() => p.at(64, 64, 1, 0, () => {
      p.glow(0, 0, 44, col, 0.45);
      if (pre === 'x_') p.tube('M-30 -30L30 30M30 -30L-30 30', 12, k.lt(col, 0.2), { ow: 5 });
      else if (pre === 'dna_') {
        p.tube('M-14 -40C14 -26 14 -14 -14 0C-42 14 -14 26 14 40', 5, '#ffe27a', { ow: 4 });
        p.tube('M14 -40C-14 -26 -14 -14 14 0C42 14 14 26 -14 40', 5, col, { ow: 4 });
        for (const y of [-30, -10, 10, 30]) p.line(`M-10 ${y}H10`, '#ffffff', 3, 0.8);
      } else if (pre === 'dh_') {
        p.part(k.poly([[0, -40], [38, 30], [-38, 30]]), [[0, '#ffe94a'], [1, '#c08a1c']], { ow: 5 });
        p.part(k.poly([[0, -26], [26, 22], [-26, 22]]), '#2a0a10', { ow: 0, hi: false });
      } else p.at(0, 0, 0.9, 0, () => k.M.tag(p, '#ffe27a'));
      p.at(0, pre === 'dh_' ? 6 : 0, pre === 'u_' ? 0.9 : 1.15, 0, () => (k.SYM[ANOM_SYM[id]] || k.SYM.star6)(p, '#ffffff'));
    }));
  };
  for (const id of Object.keys(ANOM_SYM)) ITEMS[`arc_anom:${id}`] = anomaly(id);

  // offer cards: market pass and the Junk King workshop parts
  ITEMS.arc_pass = symIcon('hex', 'dark', '#e05ad8', 'plus', '#ff8fd8');
  const JUNK_SYM = { flame: ['flame', '#ff7a1a'], bots: ['plus', '#7dff9a'], rod: ['crosshair', '#c98bff'], destruct: ['skull', '#ff5d5d'], buster: ['shieldIron', '#ffe94a'], spare: ['chip', '#a0a8b8'] };
  for (const [id, [sym, col]] of Object.entries(JUNK_SYM)) ITEMS[`arc_junk:${id}`] = symIcon('square', 'steel', '#5a3a10', sym, col);

  // ---------- move looks ----------
  const IMPACT = { fire: 'blast', ice: 'frost', thunder: 'zap', water: 'splash', plant: 'leaf', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'metal', beast: 'hit' };
  const TRAIL = { fire: 'flame', ice: 'ice', thunder: 'zap', water: 'bubble', plant: 'leaf', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'spark', beast: 'smoke' };
  const HEAD = { fire: 'flame', ice: 'snow', thunder: 'bolt', water: 'drop', plant: 'leaf', wind: 'feather', holy: 'star', dark: 'skull', metal: 'gear', beast: 'paw' };
  const ERUPT = { fire: 'flame', ice: 'ice', water: 'water', dark: 'dark', plant: 'thorn', metal: 'metal', thunder: 'rock', wind: 'rock', holy: 'rock', beast: 'rock' };
  // element of each line (abilities and basic attacks)
  const ELEM = {
    arc_kokuwamon: 'thunder', arc_goblimon: 'beast', arc_impmon: 'fire', arc_zubamon: 'metal', arc_lalamon: 'plant', arc_commandramon: 'metal', arc_tsukaimon: 'dark',
    arc_chuumon: 'fire', arc_mushroomon: 'plant', arc_snowgoblimon: 'ice', arc_psychemon: 'dark', arc_gaomon: 'beast', arc_aruraumon: 'plant',
    arc_hyokomon: 'wind', arc_lopmon: 'ice', arc_coronamon: 'fire', arc_monodramon: 'beast', arc_kotemon: 'metal', arc_floramon: 'plant', arc_jazamon: 'beast',
    arc_terriermon: 'wind', arc_crabmon: 'water', arc_elecmon: 'thunder', arc_dracmon: 'dark', arc_kamemon: 'metal', arc_gotsumon: 'beast',
    arc_hagurumon: 'metal', arc_betamon: 'water', arc_shamanmon: 'ice', arc_gomamon: 'water', arc_keramon: 'dark', arc_armadillomon: 'metal', arc_syakomon: 'water',
    arc_penguinmon: 'ice', arc_numemon: 'dark', arc_gazimon: 'thunder', arc_saberdramon: 'dark', arc_renamon: 'holy', arc_tentomon: 'thunder',
    arc_ryudamon: 'metal', arc_hawkmon: 'wind', arc_tapirmon: 'fire', arc_kunemon: 'dark', arc_hackmon: 'fire', arc_gizamon: 'dark', arc_demidevimon: 'dark',
    arc_fanbeemon: 'thunder', arc_bearmon: 'beast', arc_lunamon: 'water', arc_biyomon: 'fire',
    arc_kentaurosmon: 'holy', arc_gallantmon: 'holy', arc_beelzemon: 'dark', arc_piedmon: 'dark', arc_diaboromon: 'dark', arc_craniamon: 'metal', arc_chaosdromon: 'metal',
    arc_blackwargreymon: 'dark', arc_millenniummon: 'dark', arc_witchmon: 'water',
    arc_w_toyagumon: 'fire', arc_w_otamamon: 'water', arc_w_kuwagamon: 'beast', arc_w_gekomon: 'water', arc_w_tyrannomon: 'fire', arc_w_shogun: 'water',
    arc_w_skullgreymon: 'dark', arc_w_megadramon: 'metal', arc_w_metaltyranno: 'metal', arc_w_machinedramon: 'metal', arc_w_megidramon: 'fire',
    arc_s_belphemon: 'dark', arc_s_shellnumemon: 'water', arc_s_garurumon: 'ice',
  };
  // colors for lines that should not use the plain element palette
  const COLORS = {
    arc_chuumon: ['#ffb04a', '#fff3a0'], arc_mushroomon: ['#b6d43a', '#ff6ad8'], arc_psychemon: ['#ff5ad8', '#7a4aff'], arc_floramon: ['#ff8ad8', '#fff0fa'],
    arc_lalamon: ['#ffd23f', '#9cff6a'], arc_numemon: ['#7ed957', '#4a2a10'], arc_kunemon: ['#c8ff4a', '#7a2aff'], arc_fanbeemon: ['#ffd23f', '#2a2a2a'],
    arc_lunamon: ['#bfa8ff', '#e8f6ff'], arc_gallantmon: ['#ffe8a0', '#ff4a4a'], arc_millenniummon: ['#a04aff', '#ffd23f'], arc_witchmon: ['#c98bff', '#7fdcff'],
    arc_kentaurosmon: ['#fff0a0', '#7fdcff'], arc_tsukaimon: ['#a04aff', '#ff9ad8'], arc_hagurumon: ['#7a5aa0', '#d0c0ff'],
  };
  const SHAKE = [0.05, 0.1, 0.18];
  const SCALE = [1, 1.3, 1.65];
  // ability-type templates; stage 0..2 scales size and count
  const TPL = {
    nuke: (e, s) => [['orb', { size: 0.22 * SCALE[s], speed: 12, arc: 0.2, trail: TRAIL[e], head: HEAD[e], impactSize: 1 + 0.25 * s }]],
    aoe: (e, s) => [['sphere', { size: 0.32 * SCALE[s], grow: 0.25, from: 'hands', trail: TRAIL[e], impactSize: 1.3 + 0.3 * s }], ['shock', { at: 0.55, to: 'center', debris: false }]],
    stun: (e, s) => [['beam', { width: 0.12 + 0.05 * s, dur: 0.35, charge: 0.1 }], ['glyph', { at: 0.2, glyph: 'circle', size: 0.8 + 0.2 * s, dur: 0.5, place: 'ground', to: 'targets' }]],
    slam: (e, s) => [['shock', { hammer: s >= 1, cell: 'spark', size: 1 + 0.3 * s }], ['erupt', { at: 0.15, kind: ERUPT[e], count: 2 + s, height: 1 + 0.3 * s, area: true, to: 'center' }]],
    shield: (e, s) => [['dome', { size: 1 + 0.15 * s, orbit: 'hex', count: 4 + s * 2, to: 'self' }]],
    heal: (e, s) => [['heal', { cell: 'cross', size: 0.8 + 0.2 * s, motes: 8 + 4 * s }]],
    rally: (e, s) => [['dome', { size: 0.9 + 0.1 * s, orbit: 'star', count: 4 + s, to: 'self' }], ['heal', { at: 0.2, cell: 'star', size: 0.7 + 0.15 * s }]],
    barrage: (e, s) => [['missiles', { count: 4 + s, size: 0.1 + 0.03 * s, from: 'chest', curve: 1.1, arc: 1, dur: 0.5, gap: 0.05, impactSize: 0.7 }]],
    drain: (e, s) => [['slash', { count: 2 + s, claw: true, size: 1 + 0.2 * s }], ['drain', { at: 0.2 }]],
    dash: (e, s) => [['dash', { ghosts: 4 + s, slashes: 1 + s, dur: 0.2, size: 1 + 0.25 * s }]],
    weaken: (e, s) => [['swarm', { cell: e === 'dark' ? 'bat' : 'bubble', count: 6 + 3 * s, size: 0.3, speed: 8, wobble: 0.8 }]],
    line: (e, s) => [['beam', { width: 0.18 + 0.08 * s, dur: 0.4, charge: 0.12, overshoot: true }]],
    execute: (e, s) => [['dash', { ghosts: 3, slashes: 0, dur: 0.16 }], ['slash', { at: 0.12, count: 1 + s, size: 1.4 + 0.3 * s, vertical: true }]],
    chain: (e, s) => [['bolt', { from: 'caster', count: 1 + s, width: 0.07 + 0.02 * s }]],
    arc_swap: (e, s) => [['dome', { size: 1 + 0.15 * s, to: 'self' }], ['wave', { at: 0.15, width: 0.8 + 0.2 * s, count: 1 + s, dur: 0.45 }]],
    arc_rotate: (e, s) => [['beam', { width: 0.2 + 0.05 * s, dur: 0.35, charge: 0.1 }], ['missiles', { at: 0.2, count: 3 + s, size: 0.14, from: 'hands', curve: 1, arc: 1.2, dur: 0.5 }], ['meteor', { at: 0.45, count: 1, size: 0.6 + 0.2 * s, to: 'center' }]],
    arc_gamble: (e, s) => [['breath', { dur: 0.45, width: 0.4 + 0.1 * s, cell: 'flame' }], ['slash', { at: 0.3, count: 2 + s, claw: true, size: 1.2 + 0.2 * s }]],
    arc_progress: (e, s) => [['missiles', { count: 5 + s, size: 0.1, from: 'hands', curve: 1.3, arc: 1.4, dur: 0.55, gap: 0.06, smoke: true }]],
  };
  // name keywords that override the type template's first step
  const KEY = [
    [/breath|flame dive|fire breath|hazard/i, (e, s) => ['breath', { dur: 0.4 + 0.1 * s, width: 0.35 + 0.1 * s, cell: e === 'ice' ? 'snow' : e === 'plant' ? 'leaf' : 'flame' }]],
    [/laser|beam|javelin|joust|cannon/i, (e, s) => ['beam', { width: 0.16 + 0.07 * s, dur: 0.4, charge: 0.12 }]],
    [/bomb|grenade|heat|meteor/i, (e, s) => ['meteor', { count: 1 + s, size: 0.4 + 0.15 * s, height: 6, dur: 0.5, trail: 'flame' }]],
    [/arrow|shot|bullet|gun|stinger/i, (e, s) => ['arrows', { count: 3 + 2 * s, size: 0.3 + 0.1 * s, cell: e === 'holy' ? 'star' : 'needle' }]],
    [/missile|rocket/i, (e, s) => ['missiles', { count: 3 + 2 * s, size: 0.12 + 0.03 * s, from: 'chest', curve: 1.2, arc: 1.1, dur: 0.5 }]],
    [/thunder|shock|volt|spark/i, (e, s) => ['bolt', { from: 'sky', count: 1 + s, width: 0.08 + 0.02 * s }]],
    [/slash|blade|sword|saber|katana|edge|claw|cuts?|cross/i, (e, s, n) => ['slash', { count: 1 + s, size: 1.2 + 0.25 * s, claw: /claw/i.test(n) }]],
    [/punch|fist|knuckle|rush|press/i, (e, s) => ['shock', { size: 0.8 + 0.25 * s, debris: true }]],
    [/pollen|petal|shower|wreath|dream/i, (e, s) => ['rain', { cell: 'petal', count: 10 + 5 * s, height: 3, size: 0.3 }]],
    [/bubble|wave|aqua|tidal|pearl/i, (e, s) => ['swarm', { cell: 'bubble', count: 8 + 4 * s, size: 0.35, speed: 7, wobble: 0.7 }]],
    [/talisman|card|koyosetsu/i, (e, s) => ['swarm', { cell: 'card', count: 5 + 2 * s, size: 0.35, speed: 10, spin: 6, fan: true }]],
    [/thread|cobweb|ivy|vine/i, (e, s) => ['whip', { dur: 0.4, width: 0.1 + 0.03 * s, count: 1 + s, sway: 1 }]],
  ];
  function spec(name, line, stage) {
    const e = ELEM[line.id] || 'beast';
    const fx = (TPL[line.type] || TPL.nuke)(e, stage).map((x) => [x[0], { ...x[1] }]);
    const kw = KEY.find(([re]) => re.test(name));
    // keyword steps replace the delivery of attack types; support types (shield, heal, rally) keep theirs and add it
    if (kw && !['shield', 'heal', 'rally', 'arc_swap'].includes(line.type)) fx[0] = kw[1](e, stage, name);
    const s = { elem: e, release: 0.25, impact: IMPACT[e], shake: SHAKE[stage], fx };
    if (COLORS[line.id]) s.colors = COLORS[line.id];
    const charge = { stun: 'hands', aoe: 'hands', slam: 'ground', shield: 'aura', heal: 'aura', rally: 'aura', barrage: 'aura', drain: 'hands', dash: 'none', execute: 'none',
      weaken: 'mouth', line: 'hands', chain: 'sky', nuke: 'mouth', arc_swap: 'aura', arc_rotate: 'hands', arc_gamble: 'mouth', arc_progress: 'hands' }[line.type];
    s.charge = charge || 'aura';
    return s;
  }
  // basic attacks
  const RANGED = new Set(['Sniper', 'Mystic', 'Support']);
  const MELEE = { Guardian: 'punch', Bruiser: 'claw', Striker: 'slash', Duelist: 'kick', Assassin: 'claw' };
  const MELEE_OVR = {
    arc_zubamon: 'slash', arc_kotemon: 'slash', arc_ryudamon: 'slash', arc_hackmon: 'slash', arc_gallantmon: 'horn', arc_craniamon: 'slash', arc_gaomon: 'punch',
    arc_bearmon: 'punch', arc_jazamon: 'bite', arc_monodramon: 'bite', arc_gotsumon: 'punch', arc_kunemon: 'sting', arc_saberdramon: 'peck', arc_biyomon: 'peck',
    arc_numemon: 'tail', arc_gomamon: 'tail', arc_blackwargreymon: 'claw', arc_crabmon: 'claw', arc_penguinmon: 'kick', arc_tapirmon: 'bite',
  };
  function basic(line) {
    const e = ELEM[line.id] || 'beast';
    const R = (DT.Sets && DT.BASE_ROLES && DT.BASE_ROLES[line.role]) || {};
    const ranged = RANGED.has(line.role) || (line.range || R.range || 1) > 1;
    if (ranged) return { kind: 'shot', elem: e, head: HEAD[e], trail: TRAIL[e], size: line.tier >= 4 ? 0.26 : 0.2 };
    return { kind: MELEE_OVR[line.id] || MELEE[line.role] || 'claw', elem: e };
  }

  // build specs from the registered set's lines (all forms, wild Digimon and summons)
  const moves = {};
  const basics = {};
  const def = DT.Sets.def('arcane');
  for (const line of (def && def.lines) || []) {
    line.abil.forEach((name, st) => { if (!moves[name]) moves[name] = spec(name, line, st); });
    basics[line.id] = basic(line);
  }
  // hand-tuned looks for signature moves
  Object.assign(moves, {
    'Lila Shower': { elem: 'plant', colors: ['#ff7ad8', '#fff0fa'], charge: 'hands', release: 0.3, impact: 'leaf', shake: 0.12,
      fx: [['glyph', { glyph: 'circle', size: 1.3, dur: 0.6 }], ['beam', { width: 0.24, dur: 0.45, charge: 0.1, color: '#ff8ae0', core: '#ffffff' }], ['swarm', { at: 0.05, cell: 'petal', count: 14, size: 0.25, speed: 12, wobble: 0.6 }]] },
    'Corona Destroyer': { elem: 'dark', colors: ['#a04aff', '#ff3a7a'], charge: 'hands', release: 0.3, impact: 'dark', shake: 0.25,
      fx: [['volley', { count: 14, rate: 0.03, size: 0.4, guns: 2, spread: 0.4, from: 'hands', speed: 22 }], ['meteor', { at: 0.4, count: 1, size: 0.9, to: 'center' }]] },
    'Infinity Bowling': { elem: 'metal', colors: ['#ffb04a', '#e8f0ff'], charge: 'ground', release: 0.3, impact: 'metal', shake: 0.3,
      fx: [['wheel', { size: 1.4, cell: 'gear', dur: 0.6, saw: true }], ['shock', { at: 0.45, hammer: true, cell: 'spark', size: 1.6 }]] },
    'Black Tornado': { elem: 'dark', colors: ['#5a2a8a', '#ff5a3a'], charge: 'ground', release: 0.25, impact: 'dark', shake: 0.25,
      fx: [['tornado', { cell: 'smoke', height: 3, dur: 0.7, size: 1.2 }], ['slash', { at: 0.3, count: 3, claw: true, size: 1.6 }]] },
    'Witchelny Storm': { elem: 'water', colors: ['#c98bff', '#7fdcff'], charge: 'hands', release: 0.3, impact: 'holy', shake: 0.15,
      fx: [['missiles', { count: 8, size: 0.12, from: 'hands', curve: 1.4, arc: 1.4, dur: 0.6, gap: 0.05, smoke: true }], ['cross', { at: 0.5, size: 1, dur: 0.5 }]] },
    'Moon Destroyer': { elem: 'dark', colors: ['#a04aff', '#ffd23f'], charge: 'sky', release: 0.3, impact: 'dark', shake: 0.3,
      fx: [['portal', { size: 1.4, dur: 0.6 }], ['bolt', { at: 0.3, from: 'sky', count: 3, width: 0.12, area: 2 }]] },
    'Ultimate Flare': { elem: 'dark', colors: ['#ff3a1a', '#ffd06a'], charge: 'mouth', release: 0.3, impact: 'blast', shake: 0.3,
      fx: [['beam', { width: 0.5, dur: 0.5, charge: 0.15, overshoot: true }], ['erupt', { at: 0.3, kind: 'flame', count: 4, height: 2, area: true, to: 'center' }]] },
    "Claiomh Solais": { elem: 'holy', colors: ['#fff6c0', '#7fdcff'], charge: 'sky', release: 0.3, impact: 'holy', shake: 0.3,
      fx: [['pillar', { size: 1.2, height: 8, dur: 0.6, to: 'center' }], ['shock', { at: 0.3, size: 1.6, cell: 'star' }]] },
    'Rivalry Shot': { elem: 'holy', colors: ['#fff0a0', '#7fdcff'], charge: 'hands', release: 0.25, impact: 'holy', shake: 0.2,
      fx: [['arrows', { count: 7, size: 0.5, cell: 'star' }], ['beam', { at: 0.1, width: 0.3, dur: 0.4, charge: 0.1, overshoot: true }]] },
    'Masks Square': { elem: 'dark', colors: ['#c05aff', '#ffd23f'], charge: 'hands', release: 0.3, impact: 'dark', shake: 0.2,
      fx: [['swarm', { cell: 'card', count: 12, size: 0.4, speed: 11, spin: 8, fan: true }], ['bolt', { at: 0.3, from: 'caster', count: 2, width: 0.1 }]] },
    'Lampranthus': { elem: 'dark', colors: ['#5a1a2a', '#ff3a3a'], charge: 'ground', release: 0.3, impact: 'dark', shake: 0.3,
      fx: [['shock', { hammer: true, cell: 'ember', size: 1.6 }], ['erupt', { at: 0.15, kind: 'dark', count: 4, height: 2, area: true, to: 'center' }]] },
  });

  DT.Sets.extend('arcane', { art: { prerendered: false, items: ITEMS, moves, basic: basics } });
})(typeof window !== 'undefined' ? window : globalThis);
