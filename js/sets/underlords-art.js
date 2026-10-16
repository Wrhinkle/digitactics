// Underlords art: portraits for every item (tiered items, hats, global items) and Tamer power, move looks for every
// ability, and basic attack looks for every line. Specs follow the format at the top of js/moves.js; painters use the
// js/itemart.js kit. Shared by 'underlords' and 'underlords_ko'.
(function (G) {
  const DT = G.DT;
  const SETS = ['underlords', 'underlords_ko'];
  const DEF = DT.Sets.def('underlords');

  // ---------- move templates ----------
  const IMPACT = { fire: 'blast', ice: 'frost', thunder: 'zap', water: 'splash', plant: 'leaf', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'metal', beast: 'hit' };
  const TRAIL = { fire: 'flame', ice: 'ice', thunder: 'zap', water: 'bubble', plant: 'leaf', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'spark', beast: 'wind' };
  const ERUPT = { fire: 'flame', ice: 'ice', water: 'water', plant: 'thorn', dark: 'dark', metal: 'metal', holy: 'rock', thunder: 'rock', wind: 'rock', beast: 'rock' };
  const FX = {
    slash: (e) => ({ charge: 'hands', fx: [['slash', { count: 2, size: 1.2 }]] }),
    claw: (e) => ({ charge: 'hands', fx: [['slash', { count: 3, claw: true, size: 1.3, spread: 0.5 }]] }),
    dash: (e) => ({ charge: 'none', fx: [['dash', { ghosts: 4, slashes: 2, size: 1.2 }]] }),
    orb: (e) => ({ charge: 'mouth', fx: [['orb', { size: 0.3, speed: 11, arc: 0.25, trail: TRAIL[e] }]] }),
    shuriken: () => ({ charge: 'hands', fx: [['orb', { head: 'star', size: 0.28, speed: 15, count: 2, gap: 0.08, trail: 'spark' }]] }),
    burst: (e) => ({ charge: 'hands', fx: [['sphere', { size: 0.4, grow: 0.3, from: 'hands', trail: TRAIL[e] }], ['shock', { at: 0.45, to: 'center', debris: false }]] }),
    nova: (e) => ({ charge: 'aura', fx: [['shock', { to: 'self', size: 2 }], ['erupt', { at: 0.1, kind: ERUPT[e], to: 'self', area: true, count: 5, height: 1.2 }]] }),
    frost: () => ({ charge: 'hands', fx: [['beam', { width: 0.2, dur: 0.4, charge: 0.1 }], ['erupt', { at: 0.3, kind: 'ice', count: 3, height: 1 }]] }),
    heal: () => ({ charge: 'aura', fx: [['heal', { cell: 'leaf', motes: 10 }]] }),
    healholy: () => ({ charge: 'sky', fx: [['pillar', { size: 0.6, height: 4, dur: 0.5 }], ['heal', { at: 0.2, cell: 'cross', motes: 8 }]] }),
    shield: () => ({ charge: 'aura', fx: [['dome', { size: 1.2, count: 4 }]] }),
    buff: () => ({ charge: 'ground', fx: [['glyph', { glyph: 'circle', size: 1.3, dur: 0.6, place: 'ground' }], ['pillar', { at: 0.2, to: 'self', size: 0.6, height: 3 }]] }),
    arrow: () => ({ charge: 'hands', fx: [['arrows', { count: 1, size: 1.3 }]] }),
    rain: () => ({ charge: 'sky', fx: [['arrows', { count: 6, rain: true, size: 0.9, gap: 0.05 }]] }),
    line: (e) => ({ charge: 'hands', fx: [['beam', { width: 0.3, dur: 0.45, charge: 0.12 }]] }),
    wave: () => ({ charge: 'hands', fx: [['wave', { width: 1.2, count: 1, dur: 0.6 }]] }),
    breath: (e) => ({ charge: 'mouth', fx: [['breath', { dur: 0.5, width: 0.5, size: 0.4 }]] }),
    punch: () => ({ charge: 'hands', fx: [['streak', { width: 0.6, dur: 0.2 }], ['slash', { at: 0.1, count: 1, size: 1.5 }]] }),
    spin: () => ({ charge: 'none', fx: [['wheel', { size: 1.4, dur: 0.6, saw: true }]] }),
    chain: () => ({ charge: 'hands', fx: [['bolt', { from: 'caster', count: 2, width: 0.07 }]] }),
    bolt: () => ({ charge: 'sky', fx: [['bolt', { from: 'sky', count: 1, width: 0.1 }]] }),
    curse: (e) => ({ charge: 'hands', fx: [['glyph', { glyph: 'x', size: 1, dur: 0.5 }], ['orb', { at: 0.15, size: 0.26, speed: 9, trail: TRAIL[e] }]] }),
    summon: (e) => ({ charge: 'ground', fx: [['glyph', { glyph: 'circle', size: 1.4, dur: 0.7, place: 'ground' }], ['rain', { to: 'self', cell: e === 'plant' ? 'leaf' : 'spark', count: 10, height: 2 }]] }),
    needles: () => ({ charge: 'back', fx: [['volley', { count: 14, rate: 0.02, cell: 'needle', size: 0.5, spread: 0.6, from: 'back', speed: 18 }]] }),
    rock: () => ({ charge: 'ground', fx: [['spikes', { count: 5, kind: 'rock', size: 1 }]] }),
    poison: () => ({ charge: 'mouth', fx: [['orb', { size: 0.3, speed: 9, arc: 0.4, trail: 'bubble', impact: 'poison' }], ['erupt', { at: 0.4, kind: 'thorn', count: 3, height: 0.8 }]] }),
    bats: () => ({ charge: 'aura', fx: [['swarm', { cell: 'bat', count: 10, size: 0.35, speed: 7, wobble: 1 }]] }),
    vortex: () => ({ charge: 'hands', fx: [['vortex', { size: 1.6, dur: 0.8 }]] }),
    portal: () => ({ charge: 'none', fx: [['portal', { size: 1, dur: 0.6 }], ['shock', { at: 0.4, to: 'center', debris: false }]] }),
    meteor: () => ({ charge: 'sky', fx: [['meteor', { count: 2, size: 0.5, trail: 'flame' }]] }),
    tornado: () => ({ charge: 'aura', fx: [['tornado', { height: 3, dur: 0.7, size: 1 }]] }),
    hook: () => ({ charge: 'hands', fx: [['whip', { dur: 0.45, width: 0.12, count: 1, sway: 0.4 }]] }),
    drain: () => ({ charge: 'hands', fx: [['whip', { dur: 0.4, width: 0.1, leafy: true }], ['drain', { at: 0.3 }]] }),
    charge: (e) => ({ charge: 'none', fx: [['streak', { width: 0.8, dur: 0.3, trail: TRAIL[e] }]] }),
    slam: (e) => ({ charge: 'sky', fx: [['shock', { hammer: true }], ['erupt', { at: 0.15, kind: ERUPT[e], to: 'center', area: true, count: 4, height: 1.2 }]] }),
    gaze: () => ({ charge: 'aura', fx: [['glyph', { glyph: 'circle', size: 2, dur: 0.8, place: 'ground' }], ['erupt', { at: 0.5, kind: 'rock', count: 3, height: 1 }]] }),
    moon: () => ({ charge: 'hands', fx: [['orb', { head: 'ring', size: 0.32, speed: 12, trail: 'holy' }]] }),
    howl: () => ({ charge: 'mouth', fx: [['shock', { to: 'self', size: 1.5, debris: false }], ['swarm', { cell: 'paw', count: 8, size: 0.3, speed: 6 }]] }),
    bubble: () => ({ charge: 'mouth', fx: [['swarm', { cell: 'bubble', count: 8, size: 0.3, speed: 6 }]] }),
    missiles: () => ({ charge: 'back', fx: [['missiles', { count: 3, size: 0.16, from: 'back', curve: 1, arc: 1, dur: 0.6, smoke: true }]] }),
    volley: () => ({ charge: 'hands', fx: [['volley', { count: 10, rate: 0.03, size: 0.4, spread: 0.3, speed: 20 }]] }),
    rings: () => ({ charge: 'hands', fx: [['rings', { count: 3, size: 0.5, gap: 0.08 }]] }),
    cross: () => ({ charge: 'sky', fx: [['cross', { size: 1.2, orbs: 4, dur: 0.6 }]] }),
  };
  const SHAKE = [0.05, 0.1, 0.18];
  const spec = (name, elem, kind, stage, colors) => {
    const t = (FX[kind] || FX.orb)(elem);
    const s = { elem, release: 0.25, impact: IMPACT[elem], shake: SHAKE[stage], ...t };
    if (colors) s.colors = colors;
    return s;
  };

  // ---------- per line: [element, basic attack kind, ability looks (1 or 3), colors?] ----------
  const ST = {
    ul_antimage: ['dark', 'claw', ['claw', 'claw', 'dash']],
    ul_batrider: ['fire', 'shot', ['orb', 'burst', 'burst']],
    ul_bounty: ['thunder', 'slash', ['shuriken']],
    ul_crystal: ['ice', 'shot', ['frost']],
    ul_dazzle: ['plant', 'shot', ['heal', 'poison', 'heal']],
    ul_drow: ['ice', 'shot', ['arrow']],
    ul_enchantress: ['plant', 'shot', ['heal']],
    ul_lich: ['ice', 'shot', ['shield', 'frost', 'chain']],
    ul_magnus: ['beast', 'horn', ['buff']],
    ul_phantom: ['holy', 'slash', ['slash', 'dash', 'slash']],
    ul_shadowdemon: ['dark', 'shot', ['curse']],
    ul_slardar: ['water', 'slash', ['curse', 'curse', 'slam']],
    ul_snapfire: ['fire', 'shot', ['breath']],
    ul_tusk: ['water', 'punch', ['punch', 'punch', 'slam']],
    ul_vengeful: ['dark', 'shot', ['line', 'line', 'wave']],
    ul_venomancer: ['plant', 'shot', ['summon']],
    ul_bristleback: ['plant', 'sting', ['needles']],
    ul_chaosknight: ['dark', 'slash', ['bolt', 'orb', 'bolt']],
    ul_earthspirit: ['metal', 'punch', ['rock']],
    ul_juggernaut: ['metal', 'slash', ['spin']],
    ul_kunkka: ['water', 'slash', ['wave', 'wave', 'slam']],
    ul_legion: ['fire', 'slash', ['slash', 'buff', 'slash']],
    ul_luna: ['holy', 'shot', ['moon']],
    ul_meepo: ['metal', 'claw', ['dash', 'missiles', 'burst']],
    ul_prophet: ['plant', 'shot', ['summon']],
    ul_pudge: ['dark', 'punch', ['hook']],
    ul_queenofpain: ['dark', 'claw', ['nova']],
    ul_spiritbreaker: ['thunder', 'horn', ['charge']],
    ul_stormspirit: ['thunder', 'shot', ['line', 'chain', 'line']],
    ul_windranger: ['wind', 'shot', ['arrow', 'wave', 'tornado']],
    ul_abaddon: ['dark', 'slash', ['shield']],
    ul_alchemist: ['plant', 'punch', ['poison', 'slam', 'tornado']],
    ul_beastmaster: ['beast', 'slash', ['spin', 'wave', 'arrow']],
    ul_emberspirit: ['fire', 'punch', ['dash']],
    ul_lifestealer: ['dark', 'bite', ['buff']],
    ul_lycan: ['beast', 'bite', ['howl']],
    ul_omniknight: ['holy', 'horn', ['healholy', 'healholy', 'cross']],
    ul_puck: ['fire', 'shot', ['breath', 'line', 'line']],
    ul_shadowshaman: ['water', 'shot', ['summon', 'bubble', 'summon']],
    ul_slark: ['water', 'slash', ['dash']],
    ul_spectre: ['dark', 'claw', ['orb']],
    ul_terrorblade: ['dark', 'claw', ['orb', 'breath', 'breath'], ['#a04aff', '#ff3a3a']],
    ul_treant: ['plant', 'punch', ['drain']],
    ul_deathprophet: ['dark', 'shot', ['bats']],
    ul_doom: ['fire', 'slash', ['curse'], ['#ff3a1a', '#3a0a0a']],
    ul_lina: ['fire', 'shot', ['line', 'line', 'meteor']],
    ul_lonedruid: ['ice', 'punch', ['summon']],
    ul_mirana: ['holy', 'shot', ['arrow']],
    ul_pangolier: ['metal', 'tail', ['spin']],
    ul_rubick: ['thunder', 'shot', ['chain']],
    ul_sven: ['holy', 'slash', ['buff']],
    ul_templar: ['holy', 'claw', ['shield']],
    ul_tidehunter: ['water', 'tail', ['slam']],
    ul_viper: ['plant', 'shot', ['poison']],
    ul_voidspirit: ['dark', 'slash', ['portal']],
    ul_axe: ['fire', 'punch', ['spin', 'breath', 'breath']],
    ul_dragonknight: ['fire', 'slash', ['breath']],
    ul_faceless: ['metal', 'slash', ['vortex']],
    ul_keeper: ['holy', 'shot', ['line', 'bolt', 'line']],
    ul_medusa: ['water', 'shot', ['gaze']],
    ul_trollwarlord: ['ice', 'shot', ['volley']],
    ul_wraithking: ['dark', 'slash', ['orb'], ['#3aff9a', '#0a3a2a']],
    ul_tamer_anessa: ['dark', 'shot', ['summon']],
    ul_tamer_hob: ['fire', 'shot', ['meteor']],
    ul_tamer_jules: ['ice', 'punch', ['slam']],
    ul_tamer_enno: ['plant', 'shot', ['poison']],
    ul_s_archer: ['dark', 'shot', ['orb']],
    ul_s_golem: ['metal', 'punch', ['slam']],
    ul_s_wolf: ['ice', 'bite', ['claw']],
    ul_s_hatchling: ['fire', 'peck', ['orb']],
    ul_s_blackdragon: ['fire', 'bite', ['breath']],
    ul_s_ward: ['plant', 'shot', ['bubble']],
    ul_s_treant: ['plant', 'punch', ['bubble']],
    ul_s_bear: ['beast', 'claw', ['claw']],
    ul_s_serpent: ['water', 'shot', ['frost']],
    ul_s_bruiser: ['holy', 'punch', ['meteor']],
    ul_s_thunderhide: ['beast', 'horn', ['charge']],
    ul_w_melee: ['plant', 'punch', ['orb']],
    ul_w_ranged: ['water', 'shot', ['bubble']],
    ul_w_mega: ['plant', 'claw', ['slam']],
    ul_w_golem: ['metal', 'punch', ['slam']],
    ul_w_troll: ['beast', 'punch', ['punch']],
    ul_w_yearbeast: ['fire', 'bite', ['meteor']],
    ul_w_roshan: ['dark', 'punch', ['slam']],
  };
  const HEAD = { fire: 'flame', ice: 'shard', thunder: 'bolt', water: 'drop', plant: 'seed', wind: 'feather', holy: 'star', dark: 'skull', metal: 'gear', beast: 'paw' };
  const moves = {}, basic = {};
  for (const l of (DEF && DEF.lines) || []) {
    const st = ST[l.id];
    if (!st) continue;
    const [elem, bk, kinds, colors] = st;
    l.abil.forEach((name, i) => { if (!moves[name]) moves[name] = spec(name, elem, kinds[Math.min(i, kinds.length - 1)], i, colors); });
    basic[l.id] = bk === 'shot' ? { kind: 'shot', elem, head: HEAD[elem], trail: TRAIL[elem], size: 0.2 } : { kind: bk, elem };
  }

  // ---------- item portraits ----------
  // [shape, metal, motif, color, extra]; motifs draw around the origin within about 45 px
  const lt = (k, c, a) => k.lt(c, a), dk = (k, c, a) => k.dk(c, a);
  const MOTIF = {
    sym: (p, k, c, o) => p.at(0, 0, 2.1, 0, () => (k.SYM[o.sym] || k.SYM.star6)(p, c)),
    sword: (p, k, c) => p.at(0, 2, 0.95, 35, () => k.M.sword(p, { core: c, guard: 'gold' })),
    greatsword: (p, k, c) => p.at(0, 4, 1.05, 30, () => k.M.sword(p, { w: 20, tip: 20, core: c, guard: 'gold', pommel: c })),
    dagger: (p, k, c) => p.at(0, 0, 1.9, 0, () => k.SYM.dagger(p, c)),
    plate: (p, k) => k.M.plate(p),
    glove: (p, k, c) => p.at(0, 0, 0.95, 0, () => k.M.glove(p, c)),
    fist: (p, k, c) => k.M.fist(p, c),
    crystal: (p, k, c) => k.M.crystal(p, c, 72, 22),
    heart: (p, k, c) => k.M.heart(p, c, 0.9),
    hood: (p, k, c) => k.M.hood(p, c, '#9fe8ff'),
    disk: (p, k, c) => k.M.disk(p, c, 38),
    coin: (p, k) => { k.M.coin(p, -12, 8, 22); k.M.coin(p, 14, -6, 22, 20); },
    card: (p, k, c) => p.at(0, 0, 0.9, -8, () => k.M.card(p, c)),
    egg: (p, k, c) => p.at(0, 4, 0.82, 0, () => k.M.egg(p, c, [[-10, -6, 7, '#ffffff'], [12, 10, 8, '#ffffff']])),
    orb: (p, k, c) => { p.glow(0, 0, 44, c, 0.7); p.part(k.circ(0, 0, 26), [[0, '#ffffff'], [0.5, k.lt(c, 0.3)], [1, k.dk(c, 0.4)]]); p.gloss(-8, -9, 7, 10, -30, 0.9); },
    ring: (p, k, c) => { p.tube(k.circ(0, 0, 28), 11, c, { ow: 4 }); p.gem(0, -28, 9, k.lt(c, 0.4), 'diamond', 4); },
    boots: (p, k, c) => { p.part('M-26 -36H2V8L30 14C36 16 36 34 28 36H-26Z', c); p.line('M-26 22H32M-2 -30V8', k.dk(c, 0.5), 2.4, 0.6); p.part(k.rr(-30, 30, 64, 9, 4), k.dk(c, 0.4), { ow: 4, hi: false }); },
    mask: (p, k, c) => { p.part(k.ell(0, 0, 34, 28), c); p.part(k.ell(-13, -4, 8, 6), '#120a0a', { ow: 0, hi: false }); p.part(k.ell(13, -4, 8, 6), '#120a0a', { ow: 0, hi: false }); p.line('M-14 14C-6 20 6 20 14 14', '#120a0a', 3, 0.9); },
    staff: (p, k, c) => { p.tube('M-22 42L18 -24', 7, '#7a4a2a', { ow: 4 }); p.glow(22, -30, 22, c, 0.7); p.part(k.circ(22, -30, 13), [[0, '#ffffff'], [1, c]]); },
    book: (p, k, c) => { p.part(k.rr(-30, -36, 60, 72, 6), c); p.part(k.rr(-22, -36, 6, 72, 2), k.dk(c, 0.4), { ow: 0, hi: false }); p.at(6, 0, 1, 0, () => p.part(k.star(0, 0, 14, 6, 5), '#ffe680', { ow: 3 })); },
    hat: (p, k, c, o) => {
      p.part('M-44 18C-30 8 30 8 44 18C30 28 -30 28 -44 18Z', o.brim || k.dk(c, 0.35));
      p.part(o.tall ? 'M-22 14L-16 -36H16L22 14Z' : 'M-24 14C-24 -14 -14 -26 0 -26C14 -26 24 -14 24 14Z', c);
      p.part(k.rr(-24, 4, 48, 9, 3), o.band || '#ffe680', { ow: 3, hi: false });
      if (o.horns) { p.part('M-20 -8C-34 -16 -40 -34 -34 -44C-30 -30 -22 -24 -14 -20Z', '#f4e8d0', { ow: 3 }); p.part('M20 -8C34 -16 40 -34 34 -44C30 -30 22 -24 14 -20Z', '#f4e8d0', { ow: 3 }); }
      if (o.skull) p.at(0, -8, 0.5, 0, () => k.SYM.skull(p, '#f4f0e8'));
    },
    horn: (p, k, c) => { p.part('M-34 30C-24 -8 6 -30 36 -38C24 -20 6 2 -8 36Z', c); p.line('M-22 22C-12 0 6 -16 24 -26', k.dk(c, 0.4), 2.2, 0.6); },
    axe: (p, k, c) => { p.tube('M-20 40L16 -30', 7, '#6a3a1a', { ow: 4 }); p.part('M4 -40C30 -40 40 -20 34 -2L10 -18Z', c); p.part('M-2 -26C-24 -34 -34 -18 -30 -4L-6 -14Z', k.lt(c, 0.2)); },
    shieldsym: (p, k, c) => p.at(0, 0, 2.1, 0, () => k.SYM.shield(p, c)),
    wing: (p, k, c) => { p.at(30, 4, 0.9, 0, () => k.M.wing(p, c)); p.at(-30, 4, 0.9, 0, () => k.M.wing(p, k.lt(c, 0.2)), -1); },
    blade2: (p, k, c) => { p.at(-12, 0, 0.8, -25, () => k.M.sword(p, { core: c })); p.at(12, 0, 0.8, 25, () => k.M.sword(p, { core: c })); },
    tree: (p, k, c) => { p.part(k.rr(-7, 4, 14, 38, 4), '#7a4a2a'); p.part(k.circ(0, -14, 26), c); p.part(k.circ(-16, -2, 16), k.dk(c, 0.15)); p.part(k.circ(18, -4, 15), k.lt(c, 0.15)); p.part(k.circ(-4, -10, 7), '#ffcf4a', { ow: 3 }); },
    lightning: (p, k, c) => p.at(0, 0, 1.6, 0, () => p.part(k.BOLT, c, { ow: 3 })),
    clock: (p, k, c) => { p.part(k.circ(0, 0, 34), c); p.part(k.circ(0, 0, 27), '#fff8e0', { ow: 3, hi: false }); p.line('M0 0V-20M0 0L14 8', '#2a1a10', 4, 1); },
    flame: (p, k, c) => p.at(0, 4, 1.8, 0, () => k.SYM.flame(p, c)),
    snow: (p, k, c) => p.at(0, 0, 2, 0, () => k.SYM.snow(p, c)),
    moon: (p, k, c) => p.at(0, 0, 2, 0, () => k.SYM.moon(p, c)),
    skull: (p, k, c) => p.at(0, 0, 2, 0, () => k.SYM.skull(p, c)),
    drop: (p, k, c) => p.at(0, 0, 1.8, 0, () => k.SYM.drop(p, c)),
    star: (p, k, c) => p.at(0, 0, 2, 0, () => k.SYM.star5(p, c)),
    sun: (p, k, c) => p.at(0, 0, 1.8, 0, () => k.SYM.sun(p, c)),
    chip: (p, k, c) => p.at(0, 0, 0.75, 0, () => k.M.chip(p, c)),
    die: (p, k) => p.at(0, 0, 1.1, 0, () => k.M.die(p)),
    digivice: (p, k, c) => p.at(0, 0, 0.85, 0, () => k.M.digivice(p, '#eef1f6', c)),
    crown: (p, k, c) => { p.part('M-34 20L-38 -22L-18 -4L0 -30L18 -4L38 -22L34 20Z', c); p.part(k.rr(-36, 14, 72, 14, 5), k.dk(c, 0.3), { ow: 4, hi: false }); for (const x of [-20, 0, 20]) p.gem(x, 21, 5, '#ff4a6a', 'round', 3); },
  };
  const IT = {
    // tier 1
    ul_chainmail: ['square', 'steel', 'plate', '#a8b0bc'],
    ul_claymore: ['square', 'steel', 'greatsword', '#d0d4dc'],
    ul_gloves: ['square', 'bronze', 'glove', '#ffd23f'],
    ul_headdress: ['square', 'wood', 'crown', '#7fd06a'],
    ul_hood: ['square', 'silver', 'hood', '#3f8070'],
    ul_kaya: ['square', 'silver', 'crystal', '#b26bff'],
    ul_morbid: ['square', 'dark', 'mask', '#c03a3a'],
    ul_talisman: ['circle', 'silver', 'sym', '#9be7ff', { sym: 'pearl' }],
    ul_vitality: ['square', 'silver', 'heart', '#ff5d73'],
    ul_voidstone: ['circle', 'dark', 'orb', '#4fb3ff'],
    // tier 2
    ul_arcane: ['square', 'silver', 'boots', '#5fa0ff'],
    ul_armlet: ['circle', 'bronze', 'ring', '#d04a3a'],
    ul_blademail: ['shield', 'steel', 'plate', '#c0c6d0'],
    ul_blink: ['square', 'steel', 'dagger', '#7a8cff'],
    ul_desolator: ['square', 'dark', 'sword', '#a03050'],
    ul_lance: ['square', 'steel', 'staff', '#5ac0a0'],
    ul_halberd: ['square', 'gold', 'axe', '#ffcf4a'],
    ul_paladin: ['shield', 'gold', 'sword', '#f0e090'],
    ul_quelling: ['square', 'bronze', 'axe', '#b08a5a'],
    ul_stonecloak: ['square', 'stone', 'hood', '#8a9a8a'],
    ul_stonepike: ['square', 'stone', 'staff', '#7a8a9a'],
    ul_vanguard: ['shield', 'bronze', 'shieldsym', '#ff8a5a'],
    // tier 3
    ul_battlefury: ['square', 'gold', 'axe', '#ff7a45'],
    ul_craggy: ['square', 'stone', 'plate', '#8a7a6a'],
    ul_euls: ['square', 'silver', 'staff', '#bff0ff'],
    ul_madness: ['square', 'dark', 'mask', '#ff4a3a'],
    ul_necro: ['square', 'dark', 'book', '#6a3a9a'],
    ul_octarine: ['hex', 'prism', 'crystal', '#3ad0ff'],
    ul_pipe: ['square', 'wood', 'staff', '#5fe0c0'],
    ul_silveredge: ['square', 'silver', 'sword', '#d0e0ff'],
    ul_basher: ['square', 'bronze', 'fist', '#c09a6a'],
    ul_shako: ['square', 'wood', 'hat', '#c0a060', { tall: true, band: '#ff4a4a' }],
    ul_hat_antlers: ['shield', 'wood', 'hat', '#6ac05a', { horns: true, band: '#c08a3a' }],
    ul_hat_fedora: ['shield', 'dark', 'hat', '#3a3a44', { band: '#9a4a2a' }],
    ul_hat_pirate: ['shield', 'gold', 'hat', '#2a2a3a', { skull: true, band: '#ffd86a', tall: true }],
    ul_hat_circlet: ['shield', 'gold', 'crown', '#c0204a'],
    // tier 4
    ul_bkb: ['square', 'dark', 'greatsword', '#ffcf4a'],
    ul_butterfly: ['square', 'silver', 'wing', '#7ad0ff'],
    ul_dagon: ['circle', 'gold', 'staff', '#ff5a5a'],
    ul_diffusal: ['square', 'prism', 'blade2', '#9a6aff'],
    ul_skadi: ['circle', 'silver', 'snow', '#7fdcff'],
    ul_kaden: ['square', 'gold', 'sword', '#c0ff7a'],
    ul_maelstrom: ['square', 'steel', 'lightning', '#5ab0ff'],
    ul_mekansm: ['square', 'gold', 'chip', '#7dff9a'],
    ul_mkb: ['square', 'gold', 'staff', '#ffb02e'],
    ul_moonshard: ['circle', 'silver', 'moon', '#c8d8ff'],
    ul_refresher: ['circle', 'gold', 'orb', '#4fe0a0'],
    ul_vyse: ['square', 'prism', 'staff', '#5aa0ff'],
    ul_hat_ogre: ['shield', 'wood', 'hat', '#3f9a3a', { band: '#ffd86a' }],
    ul_hat_mantle: ['shield', 'dark', 'hood', '#8a4adf'],
    // tier 5
    ul_bloodthorn: ['square', 'dark', 'sword', '#c0203a'],
    ul_rapier: ['square', 'gold', 'greatsword', '#ffe680'],
    ul_tarrasque: ['square', 'gold', 'heart', '#ff3a5a'],
    ul_horn: ['circle', 'gold', 'horn', '#c8a060'],
    ul_radiance: ['circle', 'gold', 'sun', '#ffd84a'],
    ul_satanic: ['square', 'dark', 'skull', '#a01020'],
    ul_shiva: ['shield', 'silver', 'snow', '#9fe8ff'],
    ul_vesture: ['square', 'dark', 'hood', '#6a2a8a'],
    ul_vladmir: ['circle', 'bronze', 'crown', '#c04a4a'],
    // global items
    ul_g_riches: ['hex', 'gold', 'coin', '#ffd84a'],
    ul_g_silver: ['hex', 'silver', 'moon', '#c8d0e0'],
    ul_g_aegis: ['hex', 'gold', 'shieldsym', '#ffcf4a'],
    ul_g_stone: ['hex', 'stone', 'crystal', '#5fd0ff'],
    ul_g_vicious: ['hex', 'dark', 'skull', '#ff4a4a'],
    ul_g_ristul: ['hex', 'dark', 'drop', '#8a2a4a'],
    ul_g_assault: ['hex', 'wood', 'blade2', '#3f9a3a'],
    ul_g_chivalry: ['hex', 'steel', 'shieldsym', '#8fb4d9'],
    ul_g_recruiter: ['hex', 'silver', 'card', '#4fb3ff'],
    ul_g_smuggler: ['hex', 'bronze', 'die', '#a07a4a'],
    ul_g_roster: ['hex', 'gold', 'digivice', '#7dff9a'],
    ul_g_criminal: ['hex', 'prism', 'star', '#e58bff'],
  };
  const painter = ([shape, metal, motif, col, extra = {}]) => (p, k) => {
    k.frame(p, shape, metal, [col], { w: 8 });
    p.obj(() => p.at(64, 66, 1, 0, () => (MOTIF[motif] || MOTIF.orb)(p, k, col, extra)));
  };
  // Tamer powers: circle portrait with the Tamer's mark; the second build has a gold frame
  const POW = {
    ul_anessa_dark: ['circle', 'dark', 'sym', '#c0204a', { sym: 'moon' }],
    ul_anessa_heal: ['circle', 'gold', 'sym', '#7ff0c0', { sym: 'cross' }],
    ul_hob_fire: ['circle', 'dark', 'flame', '#ff6a1a'],
    ul_hob_crazy: ['circle', 'gold', 'sun', '#ffb02e'],
    ul_jules_brawl: ['circle', 'dark', 'fist', '#7fdcff'],
    ul_jules_bar: ['circle', 'gold', 'snow', '#bff0ff'],
    ul_enno_balm: ['circle', 'dark', 'drop', '#7dff9a'],
    ul_enno_rabid: ['circle', 'gold', 'sym', '#8adf3a', { sym: 'sludge' }],
  };
  const items = {};
  for (const [id, a] of Object.entries(IT)) items[id] = painter(a);
  for (const [id, a] of Object.entries(POW)) items[`aug:${id}`] = painter(a);

  for (const id of SETS) DT.Sets.extend(id, { art: { items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
