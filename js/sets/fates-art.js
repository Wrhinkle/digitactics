// Fates art: item, artifact, consumable, Crest and augment portraits (SVG painters on the js/itemart.js kit), a move look
// for every ability (js/moves.js spec format) and a basic attack look for every line. Visual only.
(function (G) {
  const DT = G.DT;

  // ---------- motifs: small pictures centered on (0, 0), about 90 px across ----------
  // p = picture builder, k = kit (frame, M, SYM, shapes, colors)
  const MO = {
    sword: (p, k, core = '#ff7a45', s = 1) => p.at(0, 4, s, 0, () => k.M.sword(p, { blade: 'steel', guard: 'gold', core, pommel: core })),
    feather: (p, k, col = '#ffd23f') => p.at(18, 4, 1, -35, () => k.M.wing(p, col, 0.9)),
    staff: (p, k, col = '#b26bff') => {
      p.tube('M-18 40L14 -24', 7, '#7a4a2a');
      p.at(16, -28, 1, 20, () => k.M.crystal(p, col, 34, 10));
      p.glow(16, -28, 22, col, 0.5);
    },
    lantern: (p, k, col = '#4fb3ff', glow = '#fff3b0') => {
      p.tube('M0 -46V-34', 4, '#8a5a2a', { ow: 4 });
      p.part(k.rr(-12, -36, 24, 8, 3), 'gold', { ow: 4 });
      p.part(k.ell(0, 0, 30, 34), col);
      p.glow(0, 0, 26, glow, 0.7);
      p.line('M-24 -14Q0 -20 24 -14M-28 0Q0 -6 28 0M-24 14Q0 8 24 14', k.dk(col, 0.5), 2, 0.6);
      p.part(k.rr(-12, 32, 24, 8, 3), 'gold', { ow: 4 });
      p.tube('M-6 40L-8 50M6 40L8 50', 3, '#ff4d64', { ow: 3 });
    },
    plate: (p, k) => k.M.plate(p),
    hood: (p, k, col = '#3fd6b0', eyes) => k.M.hood(p, col, eyes),
    sash: (p, k, col = '#ff4d64') => {
      p.part('M-44 -12Q0 -24 44 -12L44 12Q0 0 -44 12Z', col);
      p.line('M-40 -4Q0 -14 40 -4', k.lt(col, 0.4), 2.5, 0.7);
      p.part(k.rr(-14, -16, 28, 26, 6), 'gold');
      p.part(k.rr(-7, -9, 14, 12, 3), k.dk(col, 0.3), { ow: 3, hi: false });
      p.tube('M10 10L20 40M-6 12L-16 38', 7, k.dk(col, 0.15));
    },
    glove: (p, k, col = '#8fdc4e', cuff) => p.at(0, 2, 0.95, 0, () => k.M.glove(p, col, cuff)),
    egg: (p, k, col = '#fff3b0', s = 1) => k.M.egg(p, col, [[-10, -6, 7, '#ff8a4a'], [12, 10, 8, '#ff8a4a'], [-4, 24, 6, '#5fb0ff']], s),
    wings: (p, k, col = '#ffffff') => { p.at(-4, 0, 1, 0, () => k.M.wing(p, col, 0.8)); p.at(4, 0, 1, 0, () => k.M.wing(p, col, 0.8), -1); },
    drop: (p, k, col) => p.at(0, 0, 1.6, 0, () => p.part(k.DROP, col, { ow: 4 })),
    flame: (p, k, col = '#ff7a2a', s = 1.8) => p.at(0, 0, s, 0, () => { p.part(k.FLAME, col, { ow: 4 }); p.at(0, 6, 0.5, 0, () => p.part(k.FLAME, '#fff2a8', { ow: 0, hi: false })); }),
    bolt: (p, k, col = '#ffe94a', s = 1.7) => p.at(0, 0, s, 0, () => p.part(k.BOLT, col, { ow: 4 })),
    star: (p, k, col = '#ffd84a', s = 1) => p.part(k.star(0, 0, 36 * s, 15 * s, 5), col),
    heart: (p, k, col = '#ff3b55', s = 0.9) => k.M.heart(p, col, s),
    crystal: (p, k, col, h = 60) => k.M.crystal(p, col, h, 16),
    coin: (p, k, x = 0, y = 0, r = 14) => k.M.coin(p, x, y, r),
    card: (p, k, col) => k.M.card(p, col),
    fist: (p, k, col = '#d9773a') => k.M.fist(p, col),
    disk: (p, k, col) => k.M.disk(p, col, 38),
    shield: (p, k, col = '#c0c6d0') => {
      p.part('M0 -42L36 -30C36 6 24 30 0 44C-24 30 -36 6 -36 -30Z', col);
      p.line('M0 -34V36', k.dk(col, 0.5), 3, 0.5);
    },
    ring: (p, k, col = '#ffd84a') => { p.tube(k.circ(0, 0, 28), 9, col); p.gem(0, -28, 8, '#7dffb0', 'round', 4); },
    portal: (p, k, col = '#8a5cff') => { p.part(k.ell(0, 0, 38, 42), [[0, '#05060a'], [1, k.dk(col, 0.4)]]); for (const r of [30, 20, 10]) p.line(k.ell(0, 0, r, r * 1.1), k.lt(col, 0.3), 3, 0.7); p.glow(0, 0, 30, col, 0.5); },
    arrow: (p, k, col = '#ffd23f', rot = -45) => p.at(0, 0, 1, rot, () => { p.tube('M0 40V-28', 5, '#8a5a2a', { ow: 4 }); p.part(k.poly([[0, -46], [10, -26], [-10, -26]]), 'steel', { ow: 4 }); p.part(k.poly([[0, 30], [10, 44], [0, 38], [-10, 44]]), col, { ow: 3 }); }),
    bow: (p, k, col = '#ffd23f') => { p.tube('M-10 -44Q30 0 -10 44', 7, '#8a5a2a'); p.line('M-10 -44L-10 44', '#fff', 2, 0.8); MO.arrow(p, k, col, 0); },
    hat: (p, k, col = '#8a5cff') => { p.part(k.poly([[0, -48], [26, 18], [-26, 18]]), col); p.part(k.ell(0, 20, 44, 12), k.dk(col, 0.2)); p.sparkle(6, -14, 7, '#ffd84a'); p.sparkle(-8, 4, 5, '#ffd84a'); },
    book: (p, k, col = '#b26bff') => { p.part(k.rr(-34, -28, 68, 56, 6), col); p.line('M0 -26V26', k.dk(col, 0.5), 3, 0.7); p.part(k.rr(-28, -22, 24, 44, 3), '#fff3e0', { ow: 3, hi: false }); p.part(k.rr(4, -22, 24, 44, 3), '#fff3e0', { ow: 3, hi: false }); },
    cup: (p, k, col = '#ffd84a') => { p.part('M-28 -30H28C28 0 16 12 0 12C-16 12 -28 0 -28 -30Z', 'gold'); p.part(k.rr(-5, 10, 10, 20, 3), 'gold', { ow: 4 }); p.part(k.rr(-20, 28, 40, 10, 4), 'gold', { ow: 4 }); p.glow(0, -30, 20, col, 0.6); p.part(k.ell(0, -30, 26, 6), '#4fb3ff', { ow: 3 }); },
    banner: (p, k, col = '#ff7a45') => { p.tube('M-26 46V-44', 6, '#8a5a2a'); p.part('M-23 -40H34L22 -20L34 0H-23Z', col); p.part(k.star(4, -20, 10, 4, 5), '#fff3b0', { ow: 3, hi: false }); },
    clock: (p, k, col = '#7fd6ff') => { p.part(k.circ(0, 0, 38), 'gold'); p.part(k.circ(0, 0, 30), [[0, '#ffffff'], [1, k.lt(col, 0.3)]], { ow: 3 }); p.line('M0 0V-22M0 0L14 8', '#15161d', 4, 0.9); for (let i = 0; i < 12; i++) p.at(0, 0, 1, i * 30, () => p.line('M0 -26V-22', '#15161d', 2, 0.6)); },
    shell: (p, k, col = '#4caf50') => { p.part(k.ell(0, 0, 42, 34), col); for (const [x, y] of [[0, 0], [-20, -12], [20, -12], [-20, 12], [20, 12]]) p.part(k.ngon(x, y, 10, 6), k.dk(col, 0.3), { ow: 3, hi: false }); },
    fang: (p, k, col = '#ffffff') => p.part('M-14 -40C10 -40 22 -20 20 0C18 20 6 36 -6 46C0 26 -2 6 -14 -10Z', col),
    axe: (p, k, col = '#3a2a4a') => { p.tube('M-30 44L22 -36', 7, '#5a3a2a'); p.part('M8 -46C34 -44 46 -20 40 2C30 -8 18 -12 4 -10Z', [[0, '#c0a0ff'], [1, col]]); },
    claw: (p, k, col = '#3fd6b0') => { for (const x of [-16, 0, 16]) p.at(x, 0, 1, 15, () => p.part('M-4 -38C8 -30 10 -6 6 30L-2 34C0 0 -2 -20 -10 -34Z', col, { ow: 4 })); },
    magnet: (p, k) => { p.tube('M-22 30V-6A22 22 0 0 1 22 -6V30', 16, '#e84a5a', { cap: 'butt' }); p.part(k.rr(-30, 26, 16, 14, 2), 'steel', { ow: 4 }); p.part(k.rr(14, 26, 16, 14, 2), 'steel', { ow: 4 }); },
    hammer: (p, k) => { p.tube('M-26 40L14 -6', 7, '#7a4a2a'); p.at(18, -12, 1, -40, () => p.part(k.rr(-26, -12, 52, 24, 5), 'steel')); p.sparkle(-18, -26, 8, '#ffd84a'); },
    tornado: (p, k, col = '#b8fff0') => { for (let i = 0; i < 5; i++) p.tube(k.ell(i * 2 - 4, -30 + i * 15, 34 - i * 6, 6), 5, col, { ow: 3 }); },
    spikes: (p, k) => { k.M.plate(p); for (const [x, y, r] of [[-34, -10, -60], [34, -10, 60], [0, -40, 0], [-24, 24, -120], [24, 24, 120]]) p.at(x, y, 1, r, () => p.part(k.poly([[0, -14], [6, 0], [-6, 0]]), 'steel', { ow: 3 })); },
    eye: (p, k, col = '#ff4f7b') => { p.part(k.ell(0, 0, 40, 22), '#fff3e0'); p.part(k.circ(0, 0, 14), col, { ow: 4 }); p.part(k.circ(0, 0, 6), '#15161d', { ow: 0, hi: false }); },
    trap: (p, k) => { p.part('M-40 10L-30 -10L-20 6L-10 -14L0 6L10 -14L20 6L30 -10L40 10Z', 'steel'); p.part('M-40 14L-30 34L-20 18L-10 38L0 18L10 38L20 18L30 34L40 14Z', 'steel'); p.part(k.circ(0, 12, 6), '#ff4d64', { ow: 3 }); },
  };

  // ---------- frames ----------
  const COMP_COL = { fat_sword: '#ff7a45', fat_bow: '#ffd23f', fat_rod: '#b26bff', fat_tear: '#4fb3ff', fat_vest: '#c0c6d0', fat_cloak: '#3fd6b0', fat_belt: '#ff4d64', fat_glove: '#8fdc4e', fat_egg: '#fff3b0' };
  const component = (col, draw) => (p, k) => { k.frame(p, 'circle', 'steel', [col], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => draw(p, k))); };
  const completed = (a, b, draw, metal = 'gold') => (p, k) => { k.frame(p, 'square', metal, [COMP_COL[a], COMP_COL[b]], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => draw(p, k))); };
  const artifact = (col, draw) => (p, k) => { k.frame(p, 'hex', 'prism', [col], { w: 8 }); p.obj(() => p.at(64, 64, 0.95, 0, () => draw(p, k))); };
  const consumable = (col, draw) => (p, k) => { k.frame(p, 'circle', 'chrome', [col], { w: 6 }); p.obj(() => p.at(64, 64, 0.9, 0, () => draw(p, k))); };
  const AUG_METAL = ['', 'silver', 'gold', 'prism'];
  const augment = (tier, col, draw) => (p, k) => { k.frame(p, 'hex', AUG_METAL[tier], [col], { w: 8 }); p.obj(() => p.at(64, 66, 0.9, 0, () => draw(p, k))); };
  const sym = (name, col, s = 2.2) => (p, k) => p.at(0, 0, s, 0, () => k.SYM[name](p, col));
  const two = (a, b) => (p, k) => { a(p, k); b(p, k); };
  const small = (x, y, s, f) => (p, k) => p.at(x, y, s, 0, () => f(p, k));

  const items = {
    // components
    fat_sword: component('#ff7a45', (p, k) => MO.sword(p, k)),
    fat_bow: component('#ffd23f', (p, k) => MO.feather(p, k)),
    fat_rod: component('#b26bff', (p, k) => MO.staff(p, k)),
    fat_tear: component('#4fb3ff', (p, k) => MO.lantern(p, k)),
    fat_vest: component('#9aa4b1', (p, k) => MO.plate(p, k)),
    fat_cloak: component('#3fd6b0', (p, k) => MO.hood(p, k, '#16352f', '#5fffe0')),
    fat_belt: component('#ff4d64', (p, k) => MO.sash(p, k)),
    fat_glove: component('#8fdc4e', (p, k) => MO.glove(p, k)),
    fat_egg: component('#fff3b0', (p, k) => MO.egg(p, k)),
    fat_golden_egg: (p, k) => { k.frame(p, 'circle', 'gold', ['#ffd84a'], { w: 8 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 44, '#ffd84a', 0.6); MO.egg(p, k, '#ffd84a'); })); },
    // completed items
    fat_deathblade: completed('fat_sword', 'fat_sword', (p, k) => { MO.sword(p, k, '#ff3a2a', 1.05); p.at(22, -30, 0.5, 0, () => MO.fang(p, k)); }),
    fat_slayer: completed('fat_sword', 'fat_bow', (p, k) => { MO.sword(p, k, '#ffd23f'); p.at(26, 26, 0.9, 0, () => k.SYM.chevrons(p, '#ffd23f')); }),
    fat_gunblade: completed('fat_sword', 'fat_rod', (p, k) => { MO.sword(p, k, '#b26bff'); p.at(-22, -22, 0.45, 0, () => MO.crystal(p, k, '#b26bff')); }),
    fat_shojin: completed('fat_sword', 'fat_tear', (p, k) => { p.at(0, 0, 1, 30, () => { p.tube('M0 46V-24', 6, '#7a4a2a'); p.part(k.poly([[0, -50], [9, -24], [-9, -24]]), 'steel'); }); p.at(-22, 18, 0.45, 0, () => MO.lantern(p, k)); }),
    fat_angel: completed('fat_sword', 'fat_vest', (p, k) => { MO.wings(p, k, '#fff6e0'); p.at(0, 0, 0.7, 0, () => MO.sword(p, k, '#fff3b0')); }),
    fat_bloodthirster: completed('fat_sword', 'fat_cloak', (p, k) => { p.at(-6, 0, 0.85, -20, () => MO.sword(p, k, '#ff2a4a')); p.at(24, 24, 0.9, 0, () => MO.drop(p, k, '#ff2a4a')); }),
    fat_zeke: completed('fat_sword', 'fat_belt', (p, k) => MO.banner(p, k, '#ff7a45')),
    fat_ie: completed('fat_sword', 'fat_glove', (p, k) => { MO.sword(p, k, '#8fdc4e'); p.at(24, -26, 0.4, 0, () => MO.star(p, k, '#ffd84a')); }),
    fat_rfc: completed('fat_bow', 'fat_bow', (p, k) => MO.bow(p, k)),
    fat_rageblade: completed('fat_bow', 'fat_rod', (p, k) => { MO.feather(p, k, '#ffb02e'); p.at(-18, 18, 0.55, 0, () => MO.flame(p, k, '#ff5a1a')); }),
    fat_shiv: completed('fat_bow', 'fat_tear', (p, k) => { p.at(-8, 0, 0.8, 0, () => MO.lantern(p, k)); p.at(22, -10, 0.8, 0, () => MO.bolt(p, k)); }),
    fat_titan: completed('fat_bow', 'fat_vest', (p, k) => { MO.plate(p, k); p.at(24, -26, 0.8, 0, () => k.SYM.up(p, '#ffd23f')); }),
    fat_runaan: completed('fat_bow', 'fat_cloak', (p, k) => { MO.arrow(p, k, '#3fd6b0', -30); MO.arrow(p, k, '#ffd23f', 30); }),
    fat_zzrot: completed('fat_bow', 'fat_belt', (p, k) => MO.portal(p, k)),
    fat_lw: completed('fat_bow', 'fat_glove', (p, k) => { p.at(0, 0, 0.8, 0, () => MO.shield(p, k, '#9aa4b1')); p.line('M-30 -20L4 2L-6 10L30 34', '#15161d', 5, 0.9); MO.arrow(p, k, '#8fdc4e', -60); }),
    fat_deathcap: completed('fat_rod', 'fat_rod', (p, k) => MO.hat(p, k, '#8a5cff')),
    fat_luden: completed('fat_rod', 'fat_tear', (p, k) => { MO.staff(p, k, '#4fb3ff'); for (const r of [14, 24]) p.line(k.circ(16, -28, r), '#bff0ff', 2.5, 0.6); }),
    fat_locket: completed('fat_rod', 'fat_vest', (p, k) => { p.tube('M-20 -40Q0 -20 20 -40', 4, '#e8c860', { ow: 4 }); p.part(k.circ(0, 4, 30), 'gold'); p.gem(0, 4, 14, '#b26bff', 'hex', 4); }),
    fat_spark: completed('fat_rod', 'fat_cloak', (p, k) => { MO.crystal(p, k, '#7fdcff', 56); p.at(20, 6, 0.9, 0, () => MO.bolt(p, k, '#ffe94a', 1.2)); }),
    fat_morello: completed('fat_rod', 'fat_belt', (p, k) => { MO.book(p, k, '#7a2a6a'); p.at(0, -6, 0.7, 0, () => MO.flame(p, k, '#ff5a1a')); }),
    fat_jg: completed('fat_rod', 'fat_glove', (p, k) => { MO.glove(p, k, '#b26bff', '#ffd84a'); p.gem(0, -2, 10, '#ff8ae0', 'diamond', 4); }),
    fat_bluebuff: completed('fat_tear', 'fat_tear', (p, k) => MO.lantern(p, k, '#2f7bff', '#bfe8ff')),
    fat_frozenheart: completed('fat_tear', 'fat_vest', (p, k) => { MO.heart(p, k, '#7fdcff'); p.at(0, 0, 1, 0, () => k.SYM.snow(p, '#ffffff')); }),
    fat_chalice: completed('fat_tear', 'fat_cloak', (p, k) => MO.cup(p, k)),
    fat_redemption: completed('fat_tear', 'fat_belt', (p, k) => { MO.ring(p, k, '#ffd84a'); p.at(0, 2, 0.9, 0, () => k.SYM.plus(p, '#7dff9a')); }),
    fat_hoj: completed('fat_tear', 'fat_glove', (p, k) => { MO.fist(p, k, '#8fdc4e'); p.at(22, -30, 0.35, 0, () => MO.star(p, k, '#4fb3ff')); }),
    fat_bramble: completed('fat_vest', 'fat_vest', (p, k) => MO.spikes(p, k)),
    fat_gargoyle: completed('fat_vest', 'fat_cloak', (p, k) => MO.shield(p, k, '#8e928c')),
    fat_sunfire: completed('fat_vest', 'fat_belt', (p, k) => { p.part(k.circ(0, 0, 22), '#ff8a2a'); for (let i = 0; i < 8; i++) p.at(0, 0, 1, i * 45, () => p.at(0, -32, 0.6, 0, () => p.part(k.FLAME, '#ff5a1a', { ow: 3 }))); }),
    fat_shroud: completed('fat_vest', 'fat_glove', (p, k) => { MO.hood(p, k, '#2a2a4a', '#ff4f7b'); }),
    fat_dclaw: completed('fat_cloak', 'fat_cloak', (p, k) => { MO.shield(p, k, '#3fd6b0'); MO.claw(p, k, '#e8fff8'); }),
    fat_zephyr: completed('fat_cloak', 'fat_belt', (p, k) => MO.tornado(p, k, '#b8fff0')),
    fat_qss: completed('fat_cloak', 'fat_glove', (p, k) => { MO.hood(p, k, '#16352f'); p.sparkle(22, -30, 10, '#ffffff'); p.sparkle(-24, -14, 7, '#e8ffb8'); }),
    fat_warmog: completed('fat_belt', 'fat_belt', (p, k) => MO.heart(p, k, '#ff3b55', 1.05)),
    fat_trapclaw: completed('fat_belt', 'fat_glove', (p, k) => MO.trap(p, k)),
    fat_thief: completed('fat_glove', 'fat_glove', (p, k) => { MO.glove(p, k, '#5a5a7a', '#2a2a3a'); MO.coin(p, k, 18, 20, 13); MO.coin(p, k, -16, 26, 11); }),
    // artifacts
    fat_art_anima: artifact('#2f6b3a', (p, k) => MO.shell(p, k, '#5a8a3a')),
    fat_art_defiance: artifact('#4a2a2a', (p, k) => { MO.plate(p, k); p.at(0, 4, 0.6, 0, () => k.SYM.skull(p, '#ff4d64')); }),
    fat_art_winter: artifact('#2a6a9a', (p, k) => { p.at(0, 0, 1, 20, () => MO.fang(p, k, '#bfe8ff')); p.at(18, -18, 0.9, 0, () => k.SYM.snow(p, '#ffffff')); }),
    fat_art_collector: artifact('#7a5a10', (p, k) => { MO.fang(p, k, '#ffd84a'); MO.coin(p, k, 20, 24, 14); }),
    fat_art_infinity: artifact('#3a2a6a', (p, k) => { p.glow(0, 0, 46, '#ffffff', 0.6); p.gem(0, 0, 26, '#a08cff', 'hex', 5); p.sparkle(-26, -26, 9); p.sparkle(28, 22, 7); }),
    fat_art_manazane: artifact('#1a3a7a', (p, k) => { p.at(0, 0, 1.05, 30, () => k.M.sword(p, { blade: [[0, '#ffffff'], [1, '#4fb3ff']], guard: 'gold', core: '#4fb3ff', w: 18 })); }),
    fat_art_cleaver: artifact('#2a1a3a', (p, k) => MO.axe(p, k)),
    fat_art_randuin: artifact('#3a3a4a', (p, k) => { MO.shield(p, k, '#d9dfe9'); p.at(0, 0, 1.2, 0, () => k.SYM.cross(p, '#ffd84a')); }),
    fat_art_fist: artifact('#5a2a10', (p, k) => { p.at(10, 6, 0.85, 0, () => MO.fist(p, k, '#c0c6d0', '#ff7a2a')); p.at(-32, 28, 0.7, -60, () => p.part(k.FLAME, '#ff8a2a', { ow: 3 })); }),
    fat_art_zhonya: artifact('#5a4a10', (p, k) => MO.clock(p, k)),
    // consumables
    fat_remover: consumable('#ff8fa3', (p, k) => MO.magnet(p, k)),
    fat_reforger: consumable('#ffb347', (p, k) => MO.hammer(p, k)),
    fat_dice: consumable('#ff6a6a', (p, k) => p.at(0, 4, 0.95, 0, () => k.M.die(p))),
    fat_copy: consumable('#7dffb0', (p, k) => p.at(0, 0, 2.2, 0, () => k.SYM.copy(p, '#7dffb0'))),
    fat_holy: consumable('#fff0a0', (p, k) => { p.glow(0, 0, 40, '#fff6c0', 0.8); MO.flame(p, k, '#ffe680', 2); }),
    fat_dark: consumable('#7a3aa8', (p, k) => { p.glow(0, 0, 40, '#a04aff', 0.7); MO.flame(p, k, '#8a2aff', 2); }),
  };

  // Crests: the trait color and its symbol on a crest tag
  const CRESTS = ['CELES', 'DUELIST', 'NIGHT', 'MAGE', 'VANGUARD', 'FOREST', 'DEVA', 'ASSASSIN', 'OCEAN', 'PILGRIM', 'TREASURE', 'LUNAR', 'NINJA', 'SPIRIT', 'BRAWLER', 'KEEPER', 'WARDER', 'SHARP', 'HUNTER', 'CHARMER'];
  for (const t of CRESTS) {
    items[`fat_emblem_${t.toLowerCase()}`] = (p, k) => {
      const T = (DT.Sets.def('fates').traits || {})[t];
      k.crest(p, T ? T.color : '#ffffff', (T && T.crestSymbol) || 'star6');
    };
  }

  // augments: hex frame in the tier metal, one picture each
  const A = {
    fat_envelope: augment(1, '#c8102e', (p, k) => { p.part(k.rr(-26, -36, 52, 72, 6), '#e0283c'); p.part(k.circ(0, -4, 12), 'gold', { ow: 4 }); }),
    fat_stall: augment(1, '#8a5a2a', (p, k) => { small(-16, 4, 0.6, (q, kk) => MO.sword(q, kk))(p, k); small(18, 4, 0.6, (q, kk) => MO.lantern(q, kk))(p, k); }),
    fat_dice_aug: augment(1, '#ff6a6a', (p, k) => p.at(0, 0, 0.95, 0, () => k.M.die(p))),
    fat_charm: augment(1, '#fff3e0', (p, k) => { p.part(k.rr(-16, -40, 32, 80, 3), '#fff6e0'); p.line('M-6 -26H6M-8 -10H8M-6 6H6M-4 22H4', '#c8102e', 3.5, 0.9); }),
    fat_dojo: augment(1, '#d9773a', (p, k) => MO.fist(p, k, '#d9773a')),
    fat_lanterns: augment(1, '#2a4a9a', (p, k) => { small(-16, 4, 0.6, (q, kk) => MO.lantern(q, kk, '#ff4d64'))(p, k); small(18, -4, 0.6, (q, kk) => MO.lantern(q, kk))(p, k); }),
    fat_wall: augment(1, '#9aa4b1', (p, k) => MO.shield(p, k, '#c0c6d0')),
    fat_nest: augment(1, '#5a8a3a', (p, k) => MO.bow(p, k, '#9be15d')),
    fat_bond: augment(1, '#ff5d7a', (p, k) => { MO.heart(p, k, '#ff5d7a', 0.7); p.at(0, 0, 0.6, 0, () => k.M.digivice(p)); }),
    fat_seeker: augment(1, '#7fd6ff', (p, k) => { p.at(0, 0, 1, 0, () => k.M.digivice(p)); p.sparkle(26, -30, 9, '#ffd84a'); }),
    fat_pouch: augment(2, '#7a5a10', (p, k) => { p.part('M-28 -6C-28 -28 28 -28 28 -6C34 20 22 40 0 40C-22 40 -34 20 -28 -6Z', '#a07a3a'); MO.coin(p, k, 0, 10, 14); }),
    fat_chest: augment(2, '#8a5a2a', (p, k) => { p.part(k.rr(-38, -10, 76, 48, 6), [[0, '#d9a066'], [1, '#6a3a12']]); p.part(k.rr(-38, -30, 76, 22, 10), [[0, '#e9b076'], [1, '#7a4a1a']]); p.part(k.rr(-8, -14, 16, 18, 3), 'gold', { ow: 4 }); }),
    fat_egghunt: augment(2, '#fff3b0', (p, k) => MO.egg(p, k)),
    fat_offerings: augment(2, '#ffd84a', (p, k) => { MO.coin(p, k, -16, 10, 14); MO.coin(p, k, 14, 4, 14); MO.coin(p, k, 0, -18, 14); }),
    fat_shrine: augment(2, '#c8102e', (p, k) => { p.part(k.rr(-40, -34, 80, 10, 3), '#e0283c'); p.part(k.rr(-34, -22, 68, 8, 3), '#e0283c'); p.part(k.rr(-28, -16, 10, 56, 3), '#e0283c'); p.part(k.rr(18, -16, 10, 56, 3), '#e0283c'); }),
    fat_united: augment(2, '#ffcf4a', (p, k) => { for (const [x, c] of [[-20, '#ff7a45'], [0, '#4fb3ff'], [20, '#9be15d']]) p.at(x, 0, 0.5, 0, () => k.SYM.star5(p, c)); p.at(0, 0, 1.4, 0, () => k.SYM.chevrons(p, '#ffcf4a')); }),
    fat_tome: augment(2, '#b26bff', (p, k) => { MO.book(p, k, '#8a5cff'); p.at(0, 0, 0.6, 0, () => k.SYM.crestTag(p)); }),
    fat_apprentice: augment(2, '#ff8a2a', (p, k) => MO.hammer(p, k)),
    fat_digivolve: augment(2, '#4fb3ff', sym('up', '#4fb3ff')),
    fat_grace: augment(2, '#ffe27a', (p, k) => { MO.wings(p, k, '#fff6e0'); p.at(0, 0, 1.2, 0, () => k.SYM.cross(p, '#ffe27a')); }),
    fat_twin: augment(2, '#ff5d7a', (p, k) => { p.at(-12, 0, 0.6, 0, () => MO.heart(p, k, '#ff5d7a')); p.at(12, 0, 0.6, 0, () => MO.heart(p, k, '#ffd84a')); }),
    fat_fortune: augment(3, '#ffd84a', (p, k) => { for (const [x, y] of [[-20, 18], [0, 22], [20, 18], [-10, 2], [10, 2], [0, -14]]) MO.coin(p, k, x, y, 13); }),
    fat_grandarmory: augment(3, '#c0c6d0', (p, k) => { p.at(-14, 0, 0.75, -20, () => MO.sword(p, k)); p.at(16, 4, 0.6, 0, () => MO.shield(p, k)); }),
    fat_golden: augment(3, '#ffd84a', (p, k) => { p.glow(0, 0, 44, '#ffd84a', 0.6); MO.egg(p, k, '#ffd84a'); }),
    fat_master: augment(3, '#ff8a2a', (p, k) => { MO.hammer(p, k); p.at(-20, 22, 0.5, 0, () => MO.flame(p, k)); }),
    fat_giant: augment(3, '#ff4d64', (p, k) => { MO.heart(p, k, '#ff3b55', 1); p.at(24, -26, 0.7, 0, () => k.SYM.up(p, '#7dff9a')); }),
    fat_soulmates: augment(3, '#ff5d7a', (p, k) => { p.glow(0, 0, 40, '#ff8fc8', 0.6); MO.heart(p, k, '#ff5d7a', 1); p.at(0, 0, 0.5, 0, () => k.M.digivice(p)); }),
    fat_mega: augment(3, '#b45cff', (p, k) => { p.at(0, 0, 2, 0, () => k.SYM.up(p, '#b45cff')); p.sparkle(-26, -26, 9, '#ffd84a'); }),
    fat_festival: augment(3, '#ff5d7a', (p, k) => { small(-20, 6, 0.55, (q, kk) => MO.lantern(q, kk, '#ff4d64'))(p, k); small(20, 6, 0.55, (q, kk) => MO.lantern(q, kk, '#ffd84a'))(p, k); p.at(0, -6, 0.55, 0, () => k.M.digivice(p)); }),
    fat_crowns: augment(3, '#b7c6ff', (p, k) => { p.at(0, 0, 2.2, 0, () => k.SYM.moon(p, '#e8eeff')); p.part('M-18 -30L-10 -44L0 -32L10 -44L18 -30Z', 'gold', { ow: 3 }); }),
  };
  for (const id in A) items[`aug:${id}`] = A[id];

  // ---------- move looks ----------
  // a few spec makers; s = stage size factor (0, 1, 2)
  const orb = (elem, colors, o = {}) => ({ elem, colors, charge: o.charge || 'mouth', release: 0.24, impact: o.impact || 'blast', shake: o.shake,
    fx: [['orb', { size: o.size || 0.24, speed: o.speed || 12, arc: o.arc != null ? o.arc : 0.2, trail: o.trail, head: o.head, count: o.count, impactSize: o.isz }]] });
  const breath = (elem, colors, o = {}) => ({ elem, colors, charge: 'mouth', release: 0.26, impact: o.impact || 'blast', shake: o.shake,
    fx: [['breath', { dur: o.dur || 0.45, width: o.width || 0.35, cell: o.cell, mix: o.mix, size: o.size || 0.36, speed: 10 }]] });
  const beam = (elem, colors, o = {}) => ({ elem, colors, charge: o.charge || 'hands', release: 0.28, impact: o.impact || 'blast', shake: o.shake,
    fx: [['beam', { width: o.width || 0.24, dur: o.dur || 0.4, charge: 0.1, twin: o.twin, glyph: o.glyph }]].concat(o.extra || []) });
  const slash = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.2, impact: o.impact || 'hit', shake: o.shake,
    fx: [['slash', { count: o.count || 2, claw: o.claw, size: o.size || 1.2, spread: 0.4 }]].concat(o.extra || []) });
  const dash = (elem, colors, o = {}) => ({ elem, colors, charge: 'none', release: 0.12, impact: o.impact || 'hit', shake: o.shake,
    fx: [['dash', { ghosts: o.ghosts || 5, slashes: o.slashes != null ? o.slashes : 2, claw: o.claw, size: o.size || 1.2, dur: 0.2 }]].concat(o.extra || []) });
  const shock = (elem, colors, o = {}) => ({ elem, colors, charge: o.charge || 'sky', release: 0.3, impact: o.impact || 'hit', shake: o.shake || 0.12,
    fx: [['shock', { hammer: o.hammer, cell: o.cell, debris: o.debris }]].concat(o.extra || []) });
  const bolt = (elem, colors, o = {}) => ({ elem, colors, charge: 'sky', release: 0.25, impact: 'zap', shake: o.shake,
    fx: [['bolt', { from: o.from || 'sky', count: o.count || 1, width: o.width || 0.08, area: o.area }]] });
  const swarm = (elem, colors, cell, o = {}) => ({ elem, colors, charge: o.charge || 'aura', release: 0.22, impact: o.impact || 'hit', shake: o.shake,
    fx: [['swarm', { cell, count: o.count || 10, size: o.size || 0.3, speed: o.speed || 9, wobble: o.wobble || 0.6, spin: o.spin, fan: o.fan, tint: o.tint }]] });
  const missiles = (elem, colors, o = {}) => ({ elem, colors, charge: 'aura', release: 0.25, impact: o.impact || 'blast', shake: o.shake,
    fx: [['missiles', { count: o.count || 4, size: o.size || 0.12, from: o.from || 'chest', curve: 1, arc: 1.1, dur: 0.5, gap: 0.05 }]] });
  const volley = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.2, impact: o.impact || 'blast', shake: o.shake,
    fx: [['volley', { count: o.count || 10, rate: 0.03, cell: o.cell, size: o.size || 0.4, spread: 0.4, from: o.from || 'hands', speed: 20 }]] });
  const erupt = (elem, colors, kind, o = {}) => ({ elem, colors, charge: 'ground', release: 0.28, impact: o.impact || 'blast', shake: o.shake || 0.12,
    fx: [['erupt', { kind, count: o.count || 3, height: o.height || 1.6, area: o.area }]] });
  const meteor = (elem, colors, o = {}) => ({ elem, colors, charge: 'sky', release: 0.3, impact: o.impact || 'blast', shake: o.shake || 0.18,
    fx: [['meteor', { size: o.size || 0.5, count: o.count || 1, height: 8, trail: o.trail || 'flame' }]] });
  const dome = (elem, colors, o = {}) => ({ elem, colors, charge: 'aura', release: 0.2, impact: 'none',
    fx: [['dome', { size: o.size || 1, orbit: o.orbit, count: o.count || 4, to: 'self' }]].concat(o.extra || []) });
  const heal = (elem, colors, o = {}) => ({ elem, colors, charge: 'aura', release: 0.25, impact: 'none',
    fx: [['heal', { cell: o.cell || 'cross', color2: o.color2, size: o.size || 1, motes: o.motes || 10 }]] });
  const pillar = (elem, colors, o = {}) => ({ elem, colors, charge: 'sky', release: 0.3, impact: o.impact || 'holy', shake: o.shake,
    fx: [['pillar', { size: o.size || 1, height: 8, dur: 0.6 }]] });
  const wave = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.25, impact: o.impact || 'hit', shake: o.shake,
    fx: [['wave', { width: o.width || 1, count: o.count || 1, dur: 0.45 }]] });
  const whip = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.2, impact: o.impact || 'hit', shake: o.shake,
    fx: [['whip', { dur: 0.4, width: o.width || 0.12, count: o.count || 2, sway: 1, lightning: o.lightning, leafy: o.leafy }]] });
  const tornado = (elem, colors, o = {}) => ({ elem, colors, charge: 'aura', release: 0.25, impact: o.impact || 'wind', shake: o.shake,
    fx: [['tornado', { cell: o.cell, height: o.height || 3, dur: 0.8, size: o.size || 1 }]] });
  const arrows = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.25, impact: o.impact || 'holy', shake: o.shake,
    fx: [['arrows', { count: o.count || 3, rain: o.rain, size: o.size || 1, cell: o.cell }]] });
  const sphere = (elem, colors, o = {}) => ({ elem, colors, charge: 'hands', release: 0.3, impact: o.impact || 'blast', shake: o.shake || 0.12,
    fx: [['sphere', { size: o.size || 0.5, grow: 0.3, from: o.from || 'hands', trail: o.trail, cell: o.cell }]] });
  const drain = (elem, colors, o = {}) => ({ elem, colors, charge: 'aura', release: 0.25, impact: o.impact || 'dark', shake: o.shake,
    fx: [['beam', { width: 0.14, dur: 0.35, charge: 0.05 }], ['drain', { at: 0.2 }]] });
  const glyph = (elem, colors, g, extra) => ({ elem, colors, charge: 'aura', release: 0.25, impact: 'holy',
    fx: [['glyph', { glyph: g, size: 1.4, dur: 0.6, place: 'ground', to: 'self' }]].concat(extra || []) });
  const streak = (elem, colors, o = {}) => ({ elem, colors, charge: 'none', release: 0.15, impact: o.impact || 'hit', shake: o.shake,
    fx: [['streak', { width: o.width || 0.6, dur: 0.22, trail: o.trail }], ['slash', { at: 0.1, count: 2, claw: o.claw, size: 1.3 }]] });
  const vortex = (elem, colors, o = {}) => ({ elem, colors, charge: 'ground', release: 0.3, impact: o.impact || 'dark', shake: o.shake,
    fx: [['vortex', { size: o.size || 1.2, dur: 0.8, cell: o.cell, up: true }]] });
  const rain = (elem, colors, cell, o = {}) => ({ elem, colors, charge: 'sky', release: 0.25, impact: o.impact || 'leaf', shake: o.shake,
    fx: [['rain', { cell, count: o.count || 18, height: 5, size: o.size || 0.3, dur: 0.8 }]] });
  const big = (spec, shake = 0.2) => ({ ...spec, shake: Math.max(spec.shake || 0, shake) });

  const C = { fire: ['#ff5a1a', '#ffd36a'], ice: ['#7fdcff', '#ffffff'], dark: ['#a04aff', '#ff3a7a'], holy: ['#fff0a0', '#ffffff'] };
  const moves = {
    // 1-cost
    'Luna Claw': slash('ice', ['#b7c6ff', '#ffffff'], { claw: true, count: 3 }),
    'Moon Night Kick': dash('ice', ['#b7c6ff', '#ffffff'], { slashes: 1 }),
    'Lunatic Dance': big(dash('dark', ['#8a7aff', '#e8eeff'], { ghosts: 7, slashes: 3, size: 1.5 })),
    'Crazy Giggle': swarm('dark', ['#9aff6a', '#c05aff'], 'note', { count: 8, impact: 'poison' }),
    'Data Crusher': drain('dark', ['#c05aff', '#9aff6a']),
    "Hell's Grenade": big(missiles('dark', ['#c05aff', '#ff3a7a'], { count: 4, size: 0.18 })),
    'Twenty Dive': dash('metal', ['#ffffff', '#ffb04a'], { slashes: 1 }),
    'Road to Decade': streak('wind', ['#ffffff', '#ff7a45'], { trail: 'wind' }),
    'Zwei Glanze': big(slash('metal', ['#ffd84a', '#ffffff'], { count: 2, size: 1.7, impact: 'metal' })),
    'Cog Crusher': swarm('metal', ['#7a5aa0', '#d0c0ff'], 'gear', { count: 3, size: 0.45, spin: 10, impact: 'metal' }),
    'Destruction Grenade': missiles('metal', ['#ffb04a', '#e8f0ff'], { count: 3, size: 0.16 }),
    'Spiral Sword': big(shock('metal', ['#d8e4f4', '#7fdcff'], { charge: 'hands', cell: 'blade', impact: 'metal' })),
    'Rock Fist': orb('metal', ['#a0927a', '#e2d8c0'], { head: 'shard', size: 0.3, impact: 'hit', charge: 'hands' }),
    'Iceball Bomb': orb('ice', C.ice, { head: 'snow', size: 0.36, trail: 'ice', impact: 'frost', isz: 1.4 }),
    'Frost Claw': big(erupt('ice', C.ice, 'ice', { impact: 'frost', count: 4 })),
    'Laughing Smasher': shock('plant', ['#c0a060', '#ffe0a0'], { hammer: true, charge: 'hands' }),
    'Branch Drain': whip('plant', ['#7a5a2a', '#9aff6a'], { leafy: true, impact: 'leaf' }),
    'Cherry Bomb': big(erupt('plant', ['#ff3a6a', '#9aff6a'], 'thorn', { impact: 'leaf', count: 5 })),
    'Lullaby Bubble': swarm('water', ['#7ad0ff', '#ffffff'], 'bubble', { count: 8, size: 0.35, speed: 6, impact: 'splash' }),
    'Symphony Crusher': swarm('water', ['#7aff9a', '#ffffff'], 'note', { count: 14, size: 0.4, speed: 8, fan: true, impact: 'splash' }),
    'Musical Fist': big(shock('water', ['#ffd84a', '#ffffff'], { hammer: true, cell: 'note', impact: 'splash' })),
    'Terrier Tornado': tornado('wind', ['#9affc8', '#ffffff'], { size: 0.7, height: 2 }),
    'Gargo Laser': volley('metal', ['#9affc8', '#ffffff'], { count: 10, cell: 'spark' }),
    'Rapid Fire': big(missiles('metal', ['#9affc8', '#ffd84a'], { count: 6 })),
    'Poop Toss': orb('plant', ['#8a5a2a', '#c9925a'], { head: 'drop', size: 0.3, arc: 0.6, impact: 'poison' }),
    'Goldurian Rush': dome('metal', ['#ffd84a', '#fff3b0'], { orbit: 'star', count: 6 }),
    'Platinum Burp': big(breath('metal', ['#e8f0ff', '#c0c6d0'], { cell: 'smoke', width: 0.6, impact: 'poison' })),
    'Electric Shock': bolt('thunder', ['#ffe94a', '#a8dcff'], { from: 'caster' }),
    'Ice Winder': breath('ice', C.ice, { cell: 'snow', impact: 'frost' }),
    'Lightning Javelin': big(beam('thunder', ['#ffe94a', '#ffffff'], { width: 0.3, impact: 'zap', extra: [['bolt', { at: 0.2, to: 'center', count: 2, area: 2 }]] })),
    'Demi Darts': orb('dark', ['#c05aff', '#9aff6a'], { head: 'needle', size: 0.2, speed: 16, arc: 0.05, impact: 'poison', charge: 'hands' }),
    'Death Claw': slash('dark', ['#8a2aff', '#ff3a7a'], { claw: true, count: 2, impact: 'dark' }),
    'Nail Bone': big(beam('dark', ['#e8e0c0', '#8a2aff'], { width: 0.22, impact: 'dark' })),
    'Air Shot': orb('wind', ['#bfe8ff', '#ffffff'], { head: 'bubble', size: 0.26 }),
    "Heaven's Knuckle": pillar('holy', C.holy, { size: 0.8 }),
    "Heaven's Gate": big({ elem: 'holy', colors: C.holy, charge: 'sky', release: 0.3, impact: 'holy', fx: [['portal', { size: 1.4, dur: 0.8 }]] }),
    'Katana Attack': slash('metal', ['#ffffff', '#7fdcff'], { count: 1, size: 1.3 }),
    'Metal Armor Blade': wave('metal', ['#c0d8ff', '#ffffff'], { width: 0.8 }),
    'Legendary Dragon Blade': big(wave('holy', ['#ffd84a', '#ffffff'], { width: 1.3, count: 2 })),
    // 2-cost
    'Corona Flame': orb('fire', C.fire, { size: 0.3, trail: 'flame', head: 'flame' }),
    'Fira Claw': slash('fire', C.fire, { claw: true, count: 3, impact: 'blast' }),
    'Crimson Lion Dance': big(erupt('fire', C.fire, 'flame', { count: 5, area: true })),
    'Blue Blaster': breath('ice', ['#3a8bff', '#bfe8ff'], { cell: 'flame', impact: 'frost', width: 0.25 }),
    'Fox Fire': orb('ice', ['#3a8bff', '#bfe8ff'], { head: 'flame', size: 0.32, trail: 'flame', impact: 'frost', count: 2 }),
    'Wolf Claw': big(streak('beast', ['#5ab0ff', '#e8f6ff'], { claw: true, trail: 'wind' })),
    'Poison Ivy': whip('plant', ['#3fd04a', '#c6ff7a'], { impact: 'poison' }),
    'Needle Spray': volley('plant', ['#c8ff6a', '#fff6c0'], { count: 16, cell: 'needle', from: 'chest', impact: 'hit' }),
    'Flower Cannon': big(beam('plant', ['#ff7ad8', '#fff0fa'], { impact: 'leaf', extra: [['swarm', { at: 0.05, cell: 'petal', count: 12, size: 0.25, speed: 12 }]] })),
    'Feather Slash': swarm('wind', ['#ff9a5a', '#fff0d0'], 'feather', { count: 6, size: 0.35, speed: 12 }),
    'Blast Laser': beam('wind', ['#ff5a3a', '#ffe0a0'], { width: 0.2 }),
    'Dual Sonic': big({ elem: 'wind', colors: ['#ff8ad8', '#ffffff'], charge: 'hands', release: 0.3, impact: 'wind', fx: [['rings', { count: 4, size: 0.8, gap: 0.08, dur: 0.5 }]] }),
    'Bullet Whirlwind': tornado('wind', ['#fff0a0', '#ffffff'], { size: 0.6, height: 1.6 }),
    'Rolling Tackle': streak('holy', ['#ffe0a0', '#ffffff'], { trail: 'wind' }),
    'Wind Cutter Sword': big(slash('wind', ['#b8fff0', '#ffffff'], { count: 3, size: 1.6, impact: 'wind' })),
    'Rolling Upper': slash('beast', ['#5a8aff', '#ffffff'], { count: 1, size: 1.2 }),
    'Dash Double Claw': dash('beast', ['#5a8aff', '#ffffff'], { claw: true, slashes: 2 }),
    'Winning Knuckle': big(shock('beast', ['#5a8aff', '#ffffff'], { charge: 'hands', hammer: true })),
    'Seed Blast': orb('plant', ['#9aff6a', '#fff6c0'], { head: 'seed', size: 0.22, count: 3 }),
    'Sunshine Beam': beam('holy', ['#ffd84a', '#fff6c0'], { width: 0.28, impact: 'holy' }),
    'Lila Shower': big(rain('plant', ['#ff7ad8', '#fff0fa'], 'petal', { count: 24 })),
    'Spiral Edge': slash('water', ['#3aa0ff', '#e6f6ff'], { count: 2, impact: 'splash' }),
    'Devil Bashing': dash('water', ['#3a6aff', '#a04aff'], { slashes: 2, impact: 'splash' }),
    'Dark Deluge': big(erupt('water', ['#2a3a8a', '#a04aff'], 'water', { impact: 'dark', count: 4 })),
    'Eye of Nightmares': beam('dark', ['#ff3a3a', '#ffd0d0'], { width: 0.12, glyph: 'x', impact: 'dark' }),
    'Drain Slash': drain('dark', ['#8a2aff', '#ff3a7a']),
    'Thousand Arrows': big(arrows('dark', ['#ff3a7a', '#ffd0e0'], { count: 8, impact: 'dark' })),
    'Gear Stinger': orb('metal', ['#ffd23f', '#3a3a3a'], { head: 'needle', size: 0.22, speed: 16, arc: 0.05, impact: 'poison' }),
    'Turbo Stinger': streak('metal', ['#ffd23f', '#ffffff'], { trail: 'spark' }),
    'Nitro Stinger': big(missiles('metal', ['#ffd23f', '#ff7a2a'], { count: 4, size: 0.18 })),
    'Bonfire': orb('fire', C.fire, { head: 'flame', size: 0.3, trail: 'flame' }),
    "Hell's Hand": erupt('dark', C.dark, 'dark', { count: 2, impact: 'dark' }),
    'Soul Chopper': big(slash('dark', ['#8a5cff', '#e8e0ff'], { count: 2, size: 1.7, impact: 'dark' })),
    'Pepper Breath': orb('fire', C.fire, { size: 0.2, speed: 13, arc: 0.15, trail: 'flame' }),
    'Mega Flame': breath('fire', C.fire, { cell: 'flame', width: 0.4 }),
    'Trident Arm': big(missiles('metal', ['#ffb04a', '#e8f0ff'], { count: 3, size: 0.2, from: 'hands' })),
    'Scratch Smash': slash('wind', ['#c08a5a', '#ffffff'], { claw: true, count: 3 }),
    'Kunai Wing': swarm('wind', ['#d8e4f4', '#ffffff'], 'blade', { count: 6, size: 0.35, speed: 14, spin: 12 }),
    'Savage Emperor': big(dash('fire', ['#ff3a2a', '#ffd06a'], { ghosts: 7, slashes: 2, size: 1.5, impact: 'blast' })),
    // 3-cost
    'Meteor Slash': slash('fire', C.fire, { count: 2, impact: 'blast' }),
    'Lizard Dagger': dash('beast', ['#7aff9a', '#ffffff'], { slashes: 2 }),
    'Grade Slash': big(slash('metal', ['#ffffff', '#ffd84a'], { count: 3, size: 1.6, impact: 'metal' })),
    "Cat's Eye Hypnotism": beam('dark', ['#ffd84a', '#a04aff'], { width: 0.1, glyph: 'x', impact: 'dark' }),
    'Darkness Wave': swarm('dark', C.dark, 'bat', { count: 10, size: 0.35, speed: 10, impact: 'dark' }),
    'Phantom Pain': big(vortex('dark', C.dark, { size: 1.4 })),
    'Fifth Rush': dash('beast', ['#ff4a4a', '#ffffff'], { slashes: 2 }),
    'Fif Cross': slash('metal', ['#ffd0d0', '#ffffff'], { count: 2, size: 1.4, impact: 'metal' }),
    'Judgment of the Blade': big({ elem: 'metal', colors: ['#ff4a4a', '#ffffff'], charge: 'back', release: 0.3, impact: 'metal', shake: 0.2, fx: [['swarm', { cell: 'blade', count: 12, size: 0.5, speed: 14, spin: 10, fan: true }]] }),
    'M16 Assassin': volley('metal', ['#ffb04a', '#e8f0ff'], { count: 8, cell: 'spark' }),
    'Death Behind': dash('dark', ['#5a5a7a', '#ffffff'], { slashes: 1 }),
    'Striver Cannon': big({ elem: 'metal', colors: ['#ffb04a', '#ffffff'], charge: 'hands', release: 0.3, impact: 'blast', shake: 0.25, fx: [['orb', { size: 0.5, speed: 14, arc: 0.1, trail: 'smoke', impactSize: 1.8 }]] }),
    'Black Pearl Blast': orb('water', ['#2a2a4a', '#bfe8ff'], { head: 'bubble', size: 0.3, impact: 'splash' }),
    'Strike Fishing': orb('water', ['#3aa0ff', '#ffffff'], { head: 'needle', size: 0.3, speed: 16, arc: 0.05, impact: 'splash', charge: 'hands' }),
    'Needle Squall': big(volley('water', ['#bfe8ff', '#ffffff'], { count: 18, cell: 'needle', from: 'chest', impact: 'splash' })),
    'Fire Vortex': tornado('fire', C.fire, { cell: 'flame', size: 0.7, impact: 'blast' }),
    'Spin Caliber': { elem: 'metal', colors: ['#7fdcff', '#ffffff'], charge: 'hands', release: 0.2, impact: 'metal', fx: [['wheel', { size: 0.8, saw: true, dur: 0.5 }]] },
    'Spiral Shredder': big({ elem: 'metal', colors: ['#7fdcff', '#ffd84a'], charge: 'hands', release: 0.2, impact: 'metal', fx: [['wheel', { size: 1.2, saw: true, dur: 0.6 }], ['swarm', { at: 0.1, cell: 'blade', count: 8, size: 0.4, speed: 14, spin: 12 }]] }),
    'Super Shocker': bolt('thunder', ['#ffe94a', '#a8dcff'], { from: 'caster', width: 0.07 }),
    'Electro Shocker': sphere('thunder', ['#ffe94a', '#a8dcff'], { cell: 'bolt', trail: 'zap', impact: 'zap' }),
    'Horn Buster': big(bolt('thunder', ['#c8ff4a', '#ffffff'], { count: 3, area: 3, width: 0.12 })),
    'Cure Liqueur': orb('holy', ['#fff6c0', '#ffffff'], { head: 'drop', size: 0.26, impact: 'holy' }),
    'Grau Larm': breath('dark', ['#6a6a8a', '#d0d0ff'], { cell: 'smoke', impact: 'dark' }),
    'Inferno Gate': big({ elem: 'fire', colors: ['#ff3a1a', '#5a1a0a'], charge: 'ground', release: 0.3, impact: 'blast', shake: 0.2, fx: [['portal', { size: 1.3, dur: 0.8 }], ['erupt', { at: 0.3, kind: 'flame', count: 3 }]] }),
    'Puppy Howl': { elem: 'holy', colors: C.holy, charge: 'mouth', release: 0.2, impact: 'holy', fx: [['rings', { count: 3, size: 0.6, gap: 0.08, dur: 0.4 }]] },
    'Lightning Paw': slash('thunder', ['#ffe94a', '#ffffff'], { claw: true, count: 3, impact: 'zap' }),
    'Celestial Arrow': big(beam('holy', ['#ffd0f0', '#ffffff'], { width: 0.22, impact: 'holy', extra: [['arrows', { at: 0.1, count: 1, size: 1.4 }]] })),
    'Cheese Bomb': orb('fire', ['#ffd84a', '#fff3b0'], { head: 'square', size: 0.3, arc: 0.6 }),
    'Wild Buster': breath('fire', C.fire, { cell: 'flame', width: 0.5 }),
    'Nuclear Laser': big(beam('fire', ['#ff3a1a', '#ffe0a0'], { width: 0.36, impact: 'blast' })),
    'Nemesis Ivy': whip('plant', ['#5aff3a', '#2a5a1a'], { impact: 'poison' }),
    'Sharp Leaf': swarm('plant', ['#5aff3a', '#c6ff7a'], 'leaf', { count: 8, size: 0.35, speed: 12, impact: 'leaf' }),
    'Unidentified Flying Kiss': big(sphere('dark', ['#ff7ad8', '#a04aff'], { cell: 'heart', from: 'mouth', impact: 'dark' })),
    'Tail Smash': slash('beast', ['#7affc8', '#ffffff'], { count: 1, size: 1.3 }),
    'Blue Flare Breath': breath('fire', ['#3a8bff', '#bfe8ff'], { cell: 'flame', width: 0.45 }),
    'Blaze Sonic Breath': big(breath('fire', ['#ff8a2a', '#fff0a0'], { cell: 'flame', width: 0.6, dur: 0.6 })),
    'X Scratch': slash('beast', ['#ffd0a0', '#ffffff'], { claw: true, count: 2 }),
    'Berserk Sinking': dash('dark', ['#a04aff', '#ffd84a'], { slashes: 3 }),
    'Knowledge Stream': big(heal('holy', ['#ffffff', '#7fd6ff'], { cell: 'feather', color2: '#fff6c0', motes: 16 })),
    // 4-cost
    'Spitfire': orb('fire', ['#ff3a1a', '#5a1a3a'], { size: 0.24, trail: 'flame' }),
    'Great Antler': streak('fire', ['#ff3a1a', '#5a1a3a'], { trail: 'flame', impact: 'blast' }),
    'Terra Destroyer': big(sphere('dark', ['#ff5a1a', '#2a0a1a'], { from: 'above', size: 0.7, impact: 'dark' }), 0.3),
    'Diamond Storm': swarm('holy', ['#ffd27a', '#ffffff'], 'shard', { count: 12, size: 0.3, speed: 14, fan: true, impact: 'holy' }),
    'Fox Tail Inferno': orb('fire', ['#ffd27a', '#ff5a1a'], { head: 'flame', size: 0.3, count: 3, trail: 'flame' }),
    'Talisman of Light': big(glyph('holy', ['#fff0a0', '#ff5a5a'], 'tri', [['swarm', { at: 0.2, cell: 'card', count: 8, size: 0.4, speed: 10 }]])),
    'Spiral Twister': orb('plant', ['#3fff7a', '#d0ffd8'], { size: 0.22, trail: 'flame' }),
    'Meteor Wing': swarm('fire', C.fire, 'feather', { count: 8, size: 0.35, speed: 12, impact: 'blast' }),
    'Shadow Wing': big(arrows('fire', ['#ff7a2a', '#fff0a0'], { count: 6, rain: true, cell: 'feather', impact: 'blast' })),
    'Nightmare Syndrome': swarm('dark', ['#8a5cff', '#ff8ad8'], 'smoke', { count: 10, size: 0.45, speed: 6, impact: 'dark' }),
    'Thunder Cloud': bolt('thunder', ['#ffe94a', '#8a5cff'], { count: 2, area: 2 }),
    'Dialogue': big(glyph('dark', ['#8a5cff', '#7fd6ff'], 'circle', [['beam', { at: 0.2, width: 0.3, dur: 0.4 }]])),
    'Dash Metal': orb('metal', ['#c0c6d0', '#ffffff'], { head: 'square', size: 0.28, speed: 16, arc: 0.05, impact: 'metal' }),
    'Power Metal': orb('metal', ['#c0c6d0', '#7fdcff'], { head: 'square', size: 0.4, speed: 16, arc: 0.05, impact: 'metal', isz: 1.4 }),
    'Metal Meteor': big(meteor('metal', ['#c0c6d0', '#ffb04a'], { size: 0.7, trail: 'smoke', impact: 'metal' }), 0.3),
    "Eden's Javelin": beam('holy', ['#fff0a0', '#ff8ad8'], { width: 0.26, impact: 'holy' }),
    'Dark Despair': vortex('dark', C.dark, { size: 1.2 }),
    'Holy Desire': big(pillar('holy', ['#ffffff', '#a04aff'], { size: 1.4 })),
    'Rock Breaker': slash('fire', ['#ff3a2a', '#ffd06a'], { claw: true, count: 3 }),
    'Plasma Blade': dash('fire', ['#ff3a2a', '#ffd06a'], { slashes: 2, impact: 'blast' }),
    'Lightning Joust': big(streak('thunder', ['#ffe94a', '#ffffff'], { width: 0.8, trail: 'zap', impact: 'zap' })),
    'Marching Fishes': swarm('water', ['#3aa0ff', '#ffd27a'], 'fish', { count: 12, size: 0.42, speed: 5, wobble: 1.4, fan: true, impact: 'splash', tint: true }),
    'Harpoon Torpedo': missiles('water', ['#3aa0ff', '#e6f6ff'], { count: 1, size: 0.22, from: 'mouth', impact: 'splash' }),
    "Vulcan's Hammer": big(shock('metal', ['#ffb04a', '#fff2c0'], { hammer: true, cell: 'spark', impact: 'metal' }), 0.3),
    'Sticky Net': { elem: 'plant', colors: ['#e8ffe0', '#9aff6a'], charge: 'mouth', release: 0.2, impact: 'poison', fx: [['net', { size: 1 }]] },
    'Spiking Strike': dash('plant', ['#3a8a3a', '#e8ffe0'], { slashes: 1, impact: 'poison' }),
    'Hell Masquerade': big(dash('plant', ['#5aff3a', '#ffd84a'], { ghosts: 8, slashes: 3, impact: 'poison' })),
    'Jaza Claw': slash('beast', ['#ffd84a', '#ffffff'], { claw: true, count: 3 }),
    'Laser Eye': beam('beast', ['#ff3a3a', '#ffffff'], { width: 0.12, glyph: 'x' }),
    'Messer Spiral': big({ elem: 'beast', colors: ['#ffd84a', '#ffffff'], charge: 'none', release: 0.15, impact: 'hit', shake: 0.2, fx: [['dash', { ghosts: 6, slashes: 0, dur: 0.2 }], ['wheel', { at: 0.15, size: 1, saw: true, dur: 0.4 }]] }),
    'Little Horn': slash('beast', ['#5a5a7a', '#ffffff'], { count: 1 }),
    'Howling Wolf': { elem: 'beast', colors: ['#5a5a7a', '#d0d0ff'], charge: 'mouth', release: 0.25, impact: 'none', fx: [['rings', { count: 3, size: 0.8, gap: 0.1, dur: 0.5, to: 'self' }]] },
    'Full Moon Kick': big(dash('dark', ['#5a5a7a', '#e8eeff'], { ghosts: 6, slashes: 2 })),
    // 5-cost
    'Positron Laser': beam('holy', ['#7fdcff', '#ffffff'], { width: 0.32, impact: 'holy', charge: 'hands' }),
    'Gigadeath': big(sphere('holy', ['#ffd84a', '#ffffff'], { size: 0.8, from: 'above', impact: 'holy' }), 0.3),
    'Omni Blade': big(wave('holy', ['#7fdcff', '#ffffff'], { width: 1.6, count: 2 }), 0.3),
    'Rain of Pollen': rain('plant', ['#ffd84a', '#ff7ad8'], 'petal', { count: 16 }),
    'Pummel Peck': orb('plant', ['#7aff9a', '#ffffff'], { head: 'seed', size: 0.3, count: 3 }),
    'Forbidden Temptation': big(beam('plant', ['#ff3a6a', '#ffd0e0'], { width: 0.34, impact: 'leaf', extra: [['swarm', { at: 0.05, cell: 'petal', count: 16, size: 0.3, speed: 14 }]] }), 0.25),
    'Blazing Ice': orb('ice', C.ice, { head: 'snow', size: 0.3, trail: 'ice', impact: 'frost' }),
    'Gauntlet Claw': dash('beast', ['#c08aff', '#ffffff'], { claw: true, slashes: 2 }),
    "Heaven's Judgment": big(bolt('thunder', ['#ffffff', '#ffd84a'], { count: 4, area: 3, width: 0.14 }), 0.3),
    'Vee Headbutt': streak('holy', ['#3a8bff', '#ffffff'], { trail: 'wind' }),
    'Magna Punch': shock('holy', ['#ffd84a', '#ffffff'], { charge: 'hands', hammer: true, impact: 'holy' }),
    'Plasma Shoot': big(sphere('holy', ['#ffd84a', '#ffffff'], { size: 0.7, impact: 'holy', trail: 'holy' }), 0.3),
    'Crimson Lightning': whip('dark', ['#ff2a4a', '#ffd0d0'], { lightning: true, impact: 'dark' }),
    'Venom Infusion': big(breath('dark', ['#8a2aff', '#9aff6a'], { cell: 'smoke', width: 0.6, impact: 'poison' })),
    'Pandemonium Flame': big(erupt('dark', ['#8a2aff', '#ff5a1a'], 'dark', { count: 6, area: true, impact: 'dark' }), 0.3),
    'Fist of the Beast King': orb('fire', ['#ffd84a', '#ff7a2a'], { head: 'paw', size: 0.4, speed: 14, impact: 'blast', charge: 'hands' }),
    'The King of Fists': shock('beast', ['#ff7a2a', '#ffffff'], { charge: 'hands', hammer: true }),
    'Flashy Boss Punch': big(shock('fire', ['#ff5a1a', '#ffd84a'], { charge: 'hands', hammer: true, impact: 'blast' }), 0.3),
    'Eiseiryuoujin': slash('holy', ['#ffd84a', '#ffffff'], { count: 2, size: 1.5 }),
    'Blade of the Dragon King': wave('holy', ['#7fdcff', '#ffffff'], { width: 1.2 }),
    'Ultimate Ouryuken': big(wave('holy', ['#ffd84a', '#7fdcff'], { width: 1.7, count: 2 }), 0.3),
    'Heat Viper': beam('fire', C.fire, { width: 0.3, charge: 'mouth' }),
    'Time Unlimited': glyph('dark', ['#a04aff', '#7fd6ff'], 'circle', [['dome', { at: 0.1, size: 0.8, orbit: 'hex', count: 6 }]]),
    'Time Destroyer': big(glyph('dark', ['#ff3a7a', '#7fd6ff'], 'circle', [['dome', { at: 0.1, size: 1.1, orbit: 'hex', count: 8 }]]), 0.2),
    'Big Bang Voice': { elem: 'fire', colors: C.fire, charge: 'mouth', release: 0.25, impact: 'blast', shake: 0.15, fx: [['rings', { count: 4, size: 1, gap: 0.08, dur: 0.5 }]] },
    'Volcanic Driver': shock('fire', ['#ff5a1a', '#5a1a0a'], { hammer: true, impact: 'blast' }),
    'Volcanic Flare': big(erupt('fire', ['#ff3a1a', '#ffd36a'], 'flame', { count: 7, area: true, height: 2.2 }), 0.35),
    'Bada Boom': orb('fire', ['#ff3a1a', '#ffd36a'], { head: 'flame', size: 0.28, count: 2 }),
    'Double Impact': volley('dark', ['#a04aff', '#ffffff'], { count: 12, guns: 2, cell: 'spark' }),
    'Death Slinger': big(volley('dark', ['#ff3a7a', '#ffffff'], { count: 18, cell: 'spark', size: 0.5 }), 0.25),
    // swapped-in forms (all have models)
    'Poop Blast': orb('plant', ['#8a5a2a', '#c9925a'], { head: 'drop', size: 0.36, arc: 0.6, impact: 'poison', count: 2 }),
    'Phoebus Blow': big(shock('fire', ['#ffb02e', '#fff3b0'], { charge: 'hands', hammer: true, impact: 'blast', extra: [['erupt', { at: 0.2, kind: 'flame', count: 4, area: true }]] })),
    'Ninja Blade': slash('metal', ['#c0c6d0', '#ffffff'], { count: 2, size: 1.4, impact: 'metal' }),
    'Gigastick Lance': big(beam('dark', ['#5a5a7a', '#ff5a3a'], { width: 0.3, impact: 'dark' })),
    'Fossil Bite': slash('water', ['#a0927a', '#e6f6ff'], { claw: true, count: 2, impact: 'splash' }),
    'Ocean Love': big(swarm('water', ['#ff8ad8', '#bff0ff'], 'heart', { count: 14, size: 0.4, speed: 7, fan: true, impact: 'splash' })),
    'Tenryu Slash': big(slash('holy', ['#ffffff', '#7fdcff'], { count: 3, size: 1.8, impact: 'holy' })),
    'Maverick': big(volley('dark', ['#ff3a3a', '#ffd84a'], { count: 14, cell: 'spark', impact: 'dark' })),
    'Form Taranis': big(bolt('thunder', ['#a04aff', '#ffe94a'], { count: 3, area: 2 })),
    'Fire Phaser': orb('fire', C.fire, { head: 'feather', size: 0.28, trail: 'flame' }),
    'Paradise Lost': big(erupt('dark', C.dark, 'dark', { count: 5, area: true, impact: 'dark' }), 0.25),
    'Garuru Tomahawk': big(missiles('ice', ['#7fdcff', '#ffffff'], { count: 3, size: 0.2, from: 'chest', impact: 'frost' })),
    'Death Parade Blaster': volley('holy', ['#7fdcff', '#ffffff'], { count: 12, cell: 'glow', from: 'hands' }),
    'Serpent Cure': heal('plant', ['#ff8ad8', '#e8ffe0'], { cell: 'petal', motes: 12 }),
    'Serpent Ruin': big(whip('plant', ['#5aff3a', '#ff8ad8'], { count: 3, width: 0.16, impact: 'poison' })),
    'Asipatravana': big(slash('beast', ['#c08aff', '#ffffff'], { count: 4, size: 1.6, claw: true })),
    'Crystal Revolution': big(swarm('dark', ['#ff3a7a', '#e8e0ff'], 'shard', { count: 14, size: 0.4, speed: 12, fan: true, impact: 'dark' }), 0.25),
    'Vorpal Blade': slash('dark', ['#a04aff', '#ffffff'], { count: 2, size: 1.6, impact: 'dark' }),
    'Chrono Breaker': glyph('metal', ['#ffd84a', '#7fd6ff'], 'circle', [['rings', { at: 0.1, count: 2, size: 0.6, dur: 0.4 }]]),
    // wild Digimon and summons
    'Baluluna Gale': tornado('wind', ['#b8fff0', '#a04aff'], { size: 0.8 }),
    'Fire Fist of Shiva': shock('fire', C.fire, { charge: 'hands', hammer: true, impact: 'blast' }),
    'Deva Spear': streak('holy', ['#ffd27a', '#ffffff'], { trail: 'wind' }),
    'Tusk Strikes': streak('ice', C.ice, { trail: 'ice', impact: 'frost' }),
    'Tidal Wave': big(erupt('water', ['#2f9bff', '#bff0ff'], 'water', { count: 4, area: true, impact: 'splash' })),
    'River of Power': big(beam('water', ['#2f9bff', '#ffffff'], { width: 0.4, impact: 'splash', charge: 'mouth' })),
    'God Flame': big(breath('fire', ['#ffd84a', '#ffffff'], { cell: 'flame', width: 0.7, dur: 0.6 }), 0.3),
    'Necro Magic': swarm('dark', C.dark, 'skull', { count: 8, size: 0.4, impact: 'dark' }),
    'Goblin Strike': slash('beast', ['#7a9a3a', '#ffffff'], { count: 1 }),
    'Scissor Claw': slash('beast', ['#c03a2a', '#ffffff'], { claw: true, count: 2 }),
    'Pummel Whack': shock('beast', ['#a07a5a', '#ffffff'], { charge: 'hands', hammer: true }),
    'Trick or Treat': swarm('fire', ['#ff8a2a', '#5a2a8a'], 'bat', { count: 10, impact: 'dark' }),
    'Love Serenade': swarm('dark', ['#ff7ad8', '#ffffff'], 'note', { count: 12, fan: true, impact: 'dark' }),
    'Vimohana': dash('beast', ['#ffd84a', '#ffffff'], { claw: true, slashes: 3 }),
    'Krishna': beam('dark', ['#9aff6a', '#ffffff'], { width: 0.2, impact: 'poison' }),
    'Rodha': shock('metal', ['#c0c6d0', '#ffffff'], { hammer: true, impact: 'metal' }),
    'Spurting Ink': breath('dark', ['#1a1a2a', '#6a6a8a'], { cell: 'smoke', impact: 'dark' }),
    'Forbidden Trident': big(erupt('water', ['#2a3a8a', '#a04aff'], 'water', { count: 5, area: true, impact: 'dark' }), 0.3),
    'Lightning Whip': whip('thunder', ['#7fdcff', '#ffffff'], { lightning: true, impact: 'zap' }),
    'Phoenix Fire': big(meteor('fire', C.fire, { count: 3 })),
    'Kongou': dash('metal', ['#ffffff', '#c0c6d0'], { claw: true, slashes: 3, impact: 'metal' }),
    'Mugen': dome('plant', ['#5a8a3a', '#e8ffe0'], { size: 1.3, orbit: 'leaf', count: 6 }),
    'Ougon Kaiser': big(pillar('holy', ['#ffd84a', '#ffffff'], { size: 1.6 }), 0.35),
    'Thousand Whips': big(whip('dark', ['#2a3a8a', '#a04aff'], { count: 4, width: 0.18, impact: 'dark' })),
    'Guardian Bomb': dome('metal', ['#a0927a', '#e2d8c0'], { size: 0.9 }),
    'Hyper Cannon': orb('metal', ['#ffb04a', '#ffffff'], { size: 0.4, speed: 14, trail: 'smoke' }),
  };

  // basic attacks: melee kinds or a shot sprite
  const sh = (elem, head, o = {}) => ({ kind: 'shot', elem, head, trail: o.trail, size: o.size || 0.22, arc: o.arc != null ? o.arc : 0.15, speed: o.speed || 16, colors: o.colors });
  const ml = (kind, elem, colors) => ({ kind, elem, colors });
  const basic = {
    fat_lunamon: ml('claw', 'ice', ['#b7c6ff', '#ffffff']), fat_keramon: sh('dark', 'dot', { colors: ['#c05aff', '#9aff6a'] }), fat_zubamon: ml('slash', 'metal'),
    fat_hagurumon: ml('punch', 'metal'), fat_gotsumon: sh('ice', 'snow', { trail: 'ice' }), fat_mushroomon: ml('punch', 'plant'), fat_otamamon: sh('water', 'bubble', { arc: 0.3 }),
    fat_terriermon: sh('wind', 'ring', { colors: ['#9affc8', '#ffffff'] }), fat_numemon: ml('tail', 'plant', ['#8a5a2a', '#c9925a']), fat_betamon: sh('thunder', 'spark', { trail: 'zap' }),
    fat_demidevimon: sh('dark', 'needle', { speed: 20, arc: 0.05 }), fat_patamon: ml('punch', 'holy'), fat_ryudamon: ml('slash', 'metal', ['#ffffff', '#7fdcff']),
    fat_coronamon: sh('fire', 'flame', { trail: 'flame' }), fat_gabumon: sh('ice', 'flame', { colors: ['#3a8bff', '#bfe8ff'] }), fat_palmon: ml('claw', 'plant'),
    fat_hawkmon: sh('wind', 'feather', { arc: 0.3 }), fat_kudamon: ml('bite', 'holy'), fat_gaomon: ml('punch', 'beast', ['#5a8aff', '#ffffff']),
    fat_lalamon: sh('plant', 'seed', { arc: 0.35 }), fat_gizamon: ml('claw', 'water'), fat_dracmon: ml('claw', 'dark'), fat_fanbeemon: sh('metal', 'needle', { speed: 20, arc: 0.05 }),
    fat_candlemon: sh('fire', 'flame', { arc: 0.3 }), fat_agumon: ml('claw', 'fire'), fat_falcomon: ml('kick', 'wind'),
    fat_kotemon: ml('slash', 'fire'), fat_blackgatomon: ml('claw', 'dark'), fat_hackmon: ml('claw', 'beast', ['#ff4a4a', '#ffffff']), fat_commandramon: sh('metal', 'spark', { speed: 24, arc: 0.02 }),
    fat_syakomon: sh('water', 'needle', { speed: 20, arc: 0.05 }), fat_gumdramon: ml('tail', 'fire'), fat_tentomon: sh('thunder', 'spark', { trail: 'zap' }),
    fat_labramon: sh('holy', 'paw', { arc: 0.25 }), fat_salamon: sh('holy', 'star', { trail: 'holy' }), fat_chuumon: ml('bite', 'fire'), fat_aruraumon: sh('plant', 'leaf', { arc: 0.3 }),
    fat_dracomon: ml('tail', 'beast'), fat_meicoomon: sh('holy', 'heart', { arc: 0.3 }),
    fat_blackagumon: ml('claw', 'dark', ['#ff3a1a', '#5a1a3a']), fat_renamon: sh('holy', 'shard', { colors: ['#ffd27a', '#ffffff'] }), fat_biyomon: sh('fire', 'feather', { trail: 'flame', arc: 0.3 }),
    fat_tapirmon: sh('dark', 'wisp', { arc: 0.3 }), fat_dorumon: sh('metal', 'square', { speed: 22, arc: 0.05 }), fat_piddomon: sh('holy', 'cross', { trail: 'holy' }),
    fat_guilmon: ml('claw', 'fire', ['#ff3a2a', '#ffd06a']), fat_gomamon: ml('tail', 'water'), fat_wormmon: ml('sting', 'plant'), fat_jazamon: ml('claw', 'beast'),
    fat_blackgabumon: ml('bite', 'dark', ['#5a5a7a', '#d0d0ff']),
    fat_imperialdramon: sh('holy', 'glow', { trail: 'holy', size: 0.3, speed: 20 }), fat_floramon: sh('plant', 'petal', { trail: 'petal' }), fat_lopmon: ml('claw', 'ice'),
    fat_veemon: ml('punch', 'holy', ['#3a8bff', '#ffffff']), fat_myotismon: sh('dark', 'bat', { arc: 0.2 }), fat_leomon: ml('punch', 'beast', ['#ffd84a', '#ff7a2a']),
    fat_ouryumon: ml('slash', 'holy', ['#ffd84a', '#ffffff']), fat_clockmon: sh('fire', 'flame', { trail: 'flame' }), fat_volcamon: ml('punch', 'fire'),
    fat_impmon: sh('dark', 'spark', { speed: 24, arc: 0.02, colors: ['#a04aff', '#ffffff'] }),
    fat_goblimon: ml('punch', 'beast'), fat_kuwagamon: ml('claw', 'beast'), fat_ogremon: ml('punch', 'beast'), fat_witchmon: sh('fire', 'flame', { arc: 0.4 }),
    fat_etemon: ml('punch', 'dark'), fat_asuramon: ml('claw', 'beast'), fat_andiramon: ml('bite', 'plant'), fat_mammothmon: ml('horn', 'metal'), fat_whamon: ml('tail', 'water'),
    fat_metalseadramon: ml('slash', 'water', ['#2a3a8a', '#a04aff']), fat_azulongmon: sh('thunder', 'bolt', { trail: 'zap' }), fat_zhuqiaomon: sh('fire', 'feather', { trail: 'flame' }),
    fat_baihumon: ml('claw', 'metal'), fat_ebonwumon: ml('tail', 'plant'), fat_goldramon: sh('holy', 'glow', { trail: 'holy', size: 0.32 }),
    fat_shadow: ml('tail', 'dark'), fat_statue: ml('punch', 'metal'), fat_construct: sh('metal', 'square'),
  };

  DT.Sets.extend('fates', { art: { prerendered: false, items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
