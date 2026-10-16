// Reckoning art: item portraits (every component, Spore component, completed item, Shadow item, Radiant item,
// Crest, consumable and augment), move looks for every ability and a basic attack look for every line.
// Items share one motif per recipe in three treatments: regular (gold frame), Shadow (dark hex frame, Dark Spore
// motes, red glare) and Radiant (prism frame, halo and sparkles). Painters use the js/itemart.js kit (FORMAT.md).
(function (G) {
  const DT = G.DT;

  // ---------- item portraits ----------
  const CC = { blade: '#ff7a45', bow: '#ffd23f', rod: '#b26bff', tear: '#4fb3ff', vest: '#c0c6d0', cloak: '#3fd6b0', belt: '#ff4d64', glove: '#8fdc4e' };
  const SPORE = '#9a3cff';
  // tone a color for a treatment
  const toner = (k, tone) => (c) => (tone === 'shadow' ? k.mix(c, '#3a0e5a', 0.45) : tone === 'radiant' ? k.lt(c, 0.22) : c);

  // component motifs, drawn around the origin (about 90 px wide)
  const COMP_MOTIF = {
    blade: (p, k, t) => { k.M.chip(p, t(CC.blade), 60); p.glow(0, 0, 22, '#ffcf8a', 0.6); p.at(0, 0, 1.05, 45, () => k.M.miniBlade(p)); },
    bow: (p, k, t) => {
      p.at(-16, 0, 0.82, 0, () => k.M.wing(p, t('#fff6c8'))); p.at(16, 0, 0.82, 0, () => k.M.wing(p, t('#fff6c8')), -1);
      p.part(k.rr(-20, -20, 40, 40, 8), t(CC.bow));
      p.at(0, 0, 0.62, 0, () => p.part(k.BOLT, [[0, '#ffffff'], [1, t(CC.bow)]], { ow: 5 }));
    },
    rod: (p, k, t) => p.at(0, 4, 1, 35, () => {
      p.part(k.rr(-5, -10, 10, 54, 4), 'dark');
      p.gem(0, 46, 6, t(CC.rod), 'round', 4);
      p.glow(0, -30, 24, t(CC.rod), 0.7);
      p.at(0, -30, 1, 0, () => k.M.crystal(p, t(CC.rod), 36, 11));
    }),
    tear: (p, k, t) => {
      p.at(-6, 0, 1, -10, () => k.M.card(p, k.dk(t(CC.tear), 0.35), 46, 66));
      p.at(6, 2, 1, 8, () => { k.M.card(p, t(CC.tear), 48, 68); p.at(0, 4, 1, 0, () => p.part(k.DROP, [[0, '#ffffff'], [0.6, '#9fdcff'], [1, '#2a7fd0']], { ow: 4.5 })); });
    },
    vest: (p, k) => k.M.plate(p),
    cloak: (p, k, t) => { k.M.hood(p, t(CC.cloak)); p.gem(0, -6, 4.5, '#bdfff0', 'diamond', 3.5); },
    belt: (p, k, t) => { k.M.disk(p, t(CC.belt), 40); p.at(0, 1, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); },
    glove: (p, k, t) => { k.M.glove(p, t(CC.glove), '#f2f5e6'); p.at(1, 6, 1.05, 0, () => k.M.clover(p)); },
  };

  // completed item motifs: (p, k, A, B, t) with A/B the toned component colors
  const blade = (k, c) => [[0, '#ffffff'], [0.45, k.lt(c, 0.55)], [1, k.dk(c, 0.35)]];
  const MOTIF = {
    victory_sword: (p, k, A) => p.at(0, 0, 1.08, 45, () => k.M.sword(p, { w: 17, blade: blade(k, A), core: A, guard: A, gw: 38, pommel: A })),
    titan_slayer: (p, k, A, B) => p.at(0, 0, 1.05, 45, () => {
      p.at(-14, 19, 0.5, 0, () => k.M.wing(p, B)); p.at(14, 19, 0.5, 0, () => k.M.wing(p, B), -1);
      k.M.sword(p, { w: 28, tip: 20, blade: blade(k, A), guard: B, gw: 40, pommel: B });
    }),
    gunblade: (p, k, A, B) => p.at(-2, 2, 1, 40, () => {
      k.M.sword(p, { w: 14, top: -50, blade: blade(k, A), guard: 'dark', gw: 24 });
      p.part(k.rr(-13, 10, 26, 16, 5), 'dark'); p.glow(0, 18, 14, B, 0.9); p.gem(0, 18, 5, B, 'hex', 3);
    }),
    mana_lance: (p, k, A, B) => p.at(0, 0, 1.05, 42, () => {
      p.tube('M0 50V-14', 8, A);
      p.glow(0, -34, 22, B, 0.8);
      p.part(k.poly([[0, -56], [12, -34], [6, -20], [-6, -20], [-12, -34]]), [[0, '#ffffff'], [0.45, k.lt(B, 0.4)], [1, k.dk(B, 0.4)]], { hiA: 0.8 });
    }),
    guardian_feather: (p, k, A, B) => {
      p.at(-6, 4, 1, 0, () => p.part('M0 -20L17 -14C17 2 12 13 0 20C-12 13 -17 2 -17 -14Z', B, { ow: 5 }));
      p.at(30, -6, 1.1, -20, () => k.M.wing(p, k.lt(A, 0.4)));
      p.at(-6, 4, 0.5, 0, () => k.M.miniBlade(p));
    },
    blood_fang: (p, k, A, B) => p.at(-4, 4, 1.05, 18, () => {
      p.part('M-9 18C-12 -8 0 -32 24 -50C14 -26 10 -4 9 18Z', [[0, '#fffbea'], [0.45, k.lt(A, 0.3)], [1, k.dk(A, 0.2)]], { hiA: 0.85 });
      p.part(k.rr(-16, 16, 32, 9, 4), k.dk(A, 0.25), { ow: 5 });
      p.part(k.rr(-7, 24, 14, 20, 4), B, { ow: 5 });
      p.part(k.DROP, '#ff2e43', { ow: 3 });
    }),
    war_banner: (p, k, A, B) => {
      p.tube('M-26 46V-44', 6, '#8a6a3a');
      p.part('M-23 -42C-6 -48 8 -34 28 -40L28 4C8 10 -6 -4 -23 2Z', [[0, k.lt(A, 0.4)], [1, k.dk(A, 0.3)]]);
      p.at(2, -18, 0.55, 0, () => p.part(k.HEART, B, { ow: 6 }));
      p.gem(-26, -46, 5, '#ffd84a', 'round', 3);
    },
    crit_edge: (p, k, A, B) => p.at(0, 0, 1.05, 45, () => {
      k.M.sword(p, { w: 11, tip: 12, top: -52, blade: blade(k, A), core: k.lt(A, 0.3), guard: B, gw: 26, grip: '#2f3b22' });
      p.at(0, 19, 0.5, 0, () => k.M.clover(p, '#f2ffd0'));
      p.at(0, -52, 1, -45, () => { p.glow(0, 0, 18, '#fff0a0', 0.9); p.add(`<path d="${k.star(0, 0, 15, 4, 4)}" fill="#fffbe0"/>`); });
    }),
    rapid_cannon: (p, k, A) => p.at(0, 4, 1, -30, () => {
      p.at(-18, 14, 0.55, 0, () => k.M.wing(p, '#fff6c8')); p.at(18, 14, 0.55, 0, () => k.M.wing(p, '#fff6c8'), -1);
      p.part(k.rr(-10, -46, 20, 62, 6), [[0, k.lt(A, 0.5)], [1, k.dk(A, 0.4)]]);
      p.part(k.rr(-14, -50, 28, 10, 4), 'steel', { ow: 4 });
      p.part(k.circ(0, 22, 14), 'dark');
    }),
    rage_wing: (p, k, A, B) => {
      p.glow(0, 0, 40, A, 0.35);
      const F = [[-10, 64, k.lt(A, 0.4)], [16, 58, A], [42, 50, k.mix(A, B, 0.5)], [68, 40, B]];
      for (const [a, L, c] of F.slice().reverse()) p.at(-28, 32, 1, a, () => p.part(`M0 0C9 -10 11 ${-L * 0.6} 3 ${-L}C-6 ${-L * 0.6} -9 -12 0 0Z`, [[0, '#fffbe0'], [0.5, c], [1, k.dk(c, 0.45)]], { ow: 5 }));
      p.tube('M-30 32C-24 6 -6 -20 22 -32', 7, A);
    },
    volt_disk: (p, k, A, B) => { k.M.disk(p, B, 40); p.glow(0, 0, 30, '#fff3a0', 0.6); p.at(2, 0, 1.45, 10, () => p.part(k.BOLT, [[0, '#ffffff'], [0.5, A], [1, k.dk(A, 0.3)]], { ow: 4.5 })); },
    titan_will: (p, k, A) => {
      p.part('M-27 -38L27 -38C32 -12 32 12 26 40L-26 40C-32 12 -32 -12 -27 -38Z', 'chrome');
      p.glow(0, 0, 26, A, 0.55);
      for (const y of [-14, 0, 14]) p.part(`M-15 ${y + 7}L0 ${y - 6}L15 ${y + 7}L15 ${y + 12}L0 ${y - 1}L-15 ${y + 12}Z`, [[0, '#fff8d0'], [1, A]], { ow: 3.5, hi: false });
    },
    twin_shot: (p, k, A, B) => p.at(0, 2, 1, 35, () => {
      p.part(k.rr(-6, -18, 12, 62, 4), B);
      p.tube('M-42 -2Q0 -34 42 -2', 8, A);
      p.line('M-42 -2L0 14L42 -2', '#f6f1e0', 1.8, 0.95);
      for (const x of [-6, 6]) { p.tube(`M${x} 14V-40`, 3.5, '#e8e2cc', { ow: 4 }); p.part(k.poly([[x, -54], [x + 5, -40], [x - 5, -40]]), k.lt(B, 0.4), { ow: 3.5, hi: false }); }
    }),
    void_portal: (p, k, A, B) => {
      p.glow(0, 0, 46, '#8a3cff', 0.6);
      for (const [r, c] of [[40, k.dk(B, 0.4)], [30, '#5a1a9a'], [20, '#2a0a4a']]) p.part(k.circ(0, 0, r), c, { ow: 4 });
      p.part(k.ell(0, 0, 13, 8), '#ffe680', { ow: 3 });
      p.part(k.circ(0, 0, 5), '#15161d', { ow: 0, hi: false });
      p.line('M-36 -10C-20 -30 20 -30 36 -10M-30 18C-10 34 14 32 32 16', A, 3, 0.8);
    },
    armor_breaker: (p, k, A, B) => {
      k.M.plate(p);
      p.line('M-10 -20L2 -4L-6 6L8 24', '#15161d', 4, 0.9);
      p.at(18, -12, 0.9, 40, () => k.SYM.dagger(p, A));
      p.at(-20, 22, 0.5, 0, () => k.M.clover(p, k.lt(B, 0.5)));
    },
    ultima_wand: (p, k, A) => p.at(0, 6, 1.05, 30, () => {
      p.part(k.rr(-6, -8, 12, 56, 4), 'dark');
      p.glow(0, -30, 32, A, 0.8);
      p.at(-12, -26, 0.7, -20, () => k.M.crystal(p, A, 34, 10)); p.at(12, -26, 0.7, 20, () => k.M.crystal(p, A, 34, 10));
      p.at(0, -34, 1.1, 0, () => k.M.crystal(p, k.lt(A, 0.2), 40, 12));
    }),
    seraph_staff: (p, k, A, B) => p.at(0, 6, 1, 25, () => {
      p.part(k.rr(-5, -8, 10, 56, 4), 'gold');
      p.at(-12, -30, 0.55, 0, () => k.M.wing(p, '#ffffff')); p.at(12, -30, 0.55, 0, () => k.M.wing(p, '#ffffff'), -1);
      p.glow(0, -32, 22, B, 0.8); p.gem(0, -32, 9, A, 'round', 4);
    }),
    iron_locket: (p, k, A, B) => {
      p.tube('M-14 -40C-14 -54 14 -54 14 -40', 4, '#e8c860', { ow: 4 });
      p.part('M0 -40L30 -30C30 6 20 28 0 40C-20 28 -30 6 -30 -30Z', 'chrome');
      p.part('M0 -28L18 -22C18 2 12 16 0 26C-12 16 -18 2 -18 -22Z', [[0, k.lt(A, 0.4)], [1, k.dk(A, 0.3)]], { ow: 4 });
      p.gem(0, -2, 7, B, 'hex', 3.5);
    },
    mana_zap: (p, k, A, B) => {
      p.at(0, 4, 1, 0, () => k.M.crystal(p, A, 50, 15));
      for (const [x, r] of [[-30, -20], [30, 20]]) p.at(x, -6, 0.75, r, () => p.part(k.BOLT, [[0, '#ffffff'], [1, B]], { ow: 4 }));
      p.glow(0, 4, 30, A, 0.45);
    },
    flame_tome: (p, k, A, B) => {
      p.part(k.rr(-30, -26, 60, 56, 6), k.dk(A, 0.3));
      p.part(k.rr(-26, -22, 52, 48, 4), '#f6ecd6', { ow: 3 });
      p.line('M0 -22V26', '#8a6a3a', 2.5, 0.8);
      p.at(0, -2, 1.4, 0, () => p.part(k.FLAME, [[0, '#fff6c0'], [0.5, B], [1, k.dk(B, 0.3)]], { ow: 4 }));
    },
    jewel_gauntlet: (p, k, A, B) => { k.M.glove(p, B, '#f2f5e6'); p.glow(1, 2, 20, A, 0.7); p.gem(1, 4, 11, A, 'hex', 4); },
    mana_overflow: (p, k, A) => {
      p.at(-14, 8, 1.4, -10, () => p.part(k.DROP, [[0, '#ffffff'], [0.6, k.lt(A, 0.3)], [1, k.dk(A, 0.3)]], { ow: 4 }));
      p.at(16, -6, 1.7, 10, () => p.part(k.DROP, [[0, '#ffffff'], [0.6, A], [1, k.dk(A, 0.4)]], { ow: 4 }));
      p.sparkle(20, -38, 6); p.sparkle(-28, -20, 5);
    },
    frost_heart: (p, k, A, B) => { p.glow(0, 0, 36, '#bfeaff', 0.6); k.M.heart(p, k.mix(B, '#9fe0ff', 0.6), 0.9); p.at(0, -2, 0.9, 0, () => k.SYM.snow(p, '#ffffff')); },
    power_chalice: (p, k, A, B) => {
      p.part('M-26 -34H26C26 -8 16 4 0 6C-16 4 -26 -8 -26 -34Z', 'gold');
      p.part(k.rr(-5, 4, 10, 22, 3), 'gold', { ow: 4 });
      p.part(k.rr(-20, 24, 40, 10, 4), 'gold', { ow: 4 });
      p.part(k.ell(0, -32, 22, 6), [[0, k.lt(B, 0.5)], [1, k.dk(A, 0.2)]], { ow: 3 });
      p.glow(0, -36, 20, B, 0.6);
    },
    holy_ring: (p, k, A, B) => {
      p.glow(0, 0, 40, '#fff2b0', 0.5);
      p.tube(k.circ(0, 4, 30), 9, 'gold');
      p.part('M-5 -26H5V-14H17V-4H5V20H-5V-4H-17V-14H-5Z', [[0, '#ffffff'], [1, B]], { ow: 4 });
      p.gem(0, 34, 6, A, 'diamond', 3);
    },
    justice_hand: (p, k, A, B) => { k.M.fist(p, k.lt(B, 0.1), k.dk(B, 0.3)); p.at(0, -40, 0.9, 0, () => p.part(k.star(0, 0, 14, 6, 5), A, { ow: 4 })); },
    spike_plate: (p, k) => {
      for (const [x, y, r] of [[-34, -20, -60], [34, -20, 60], [-30, 16, -110], [30, 16, 110], [0, -42, 0]]) p.at(x, y, 1, r, () => p.part(k.poly([[0, -14], [6, 4], [-6, 4]]), 'steel', { ow: 4 }));
      k.M.plate(p);
    },
    stone_hide: (p, k, A, B) => {
      p.part(k.ngon(0, 0, 40, 6), 'stone');
      p.part(k.ngon(0, 0, 26, 6), [[0, k.lt(B, 0.3)], [1, k.dk(B, 0.5)]], { ow: 4 });
      p.line('M-30 -12L-12 -6L-6 10M20 -26L10 -8L24 6', '#2a2c2a', 2.5, 0.7);
      for (const x of [-9, 9]) p.part(k.ell(x, -2, 4, 3), '#fff2a8', { ow: 2, hi: false });
    },
    flame_core: (p, k, A, B) => {
      p.glow(0, 0, 44, '#ff8a2a', 0.55);
      p.part(k.circ(0, 4, 28), [[0, '#fff2a8'], [0.5, B], [1, k.dk(B, 0.4)]]);
      p.at(0, -16, 1.6, 0, () => p.part(k.FLAME, [[0, '#fff6c0'], [1, '#ff6a1a']], { ow: 4 }));
      p.tube(k.circ(0, 4, 34), 4, 'steel', { ow: 3 });
    },
    still_shroud: (p, k, A, B) => { k.M.hood(p, k.mix(A, B, 0.5)); p.line('M-9 -18Q-5 -15 -2 -18M2 -18Q5 -15 9 -18', '#cfe8ff', 2.2, 0.9); },
    dragon_scale: (p, k, A) => {
      for (let i = 0; i < 3; i++) for (let j = 0; j < 3; j++) {
        const x = -24 + j * 24 + (i % 2) * 12, y = -24 + i * 22;
        p.part(`M${x - 13} ${y}C${x - 13} ${y + 18} ${x + 13} ${y + 18} ${x + 13} ${y}C${x + 6} ${y + 6} ${x - 6} ${y + 6} ${x - 13} ${y}Z`, [[0, k.lt(A, 0.4)], [1, k.dk(A, 0.4)]], { ow: 4 });
      }
    },
    wind_ward: (p, k, A, B) => {
      p.line('M-40 -20C-10 -40 30 -30 20 -6C12 12 -16 4 -8 -10M-36 18C-6 4 38 10 30 30', k.lt(A, 0.5), 6, 0.95);
      p.line('M-40 -20C-10 -40 30 -30 20 -6C12 12 -16 4 -8 -10M-36 18C-6 4 38 10 30 30', B, 2.5, 0.9);
      p.at(26, -30, 0.6, 30, () => k.M.wing(p, '#ffffff'));
    },
    quick_silver: (p, k, A, B) => {
      p.at(-4, 6, 2, 0, () => p.part(k.DROP, 'silver', { ow: 3 }));
      p.at(24, -10, 0.7, 20, () => k.M.wing(p, k.lt(B, 0.3)));
      p.sparkle(-26, -30, 7);
    },
    giga_belt: (p, k, A) => { k.M.disk(p, A, 44); p.at(0, 1, 0.45, 0, () => p.part(k.HEART, '#ff3b55', { ow: 8 })); p.glow(0, 0, 20, '#ffb0c0', 0.4); },
    trap_claw: (p, k, A, B) => {
      p.part('M-36 -30C-30 0 -14 16 0 18C14 16 30 0 36 -30L24 -18L14 -30L4 -16L-4 -16L-14 -30L-24 -18Z', [[0, k.lt(B, 0.4)], [1, k.dk(B, 0.4)]]);
      p.part('M-30 34C-24 14 -10 6 0 6C10 6 24 14 30 34L20 26L10 34L0 24L-10 34L-20 26Z', [[0, k.lt(A, 0.3)], [1, k.dk(A, 0.4)]]);
    },
    thief_gloves: (p, k, A, B) => { p.at(-6, 4, 0.9, -10, () => k.M.glove(p, A, '#2a2c3a')); k.M.coin(p, 26, -24, 13, 15); k.M.coin(p, 30, 4, 10, -10); },
  };

  function framed(k, p, tone, A, B) {
    if (tone === 'shadow') k.frame(p, 'hex', 'dark', ['#24082e', k.dk(B, 0.2)], { w: 8 });
    else if (tone === 'radiant') k.frame(p, 'circle', 'prism', [k.lt(A, 0.35), '#fff6c8'], { w: 8 });
    else k.frame(p, 'square', 'gold', [A, B], { w: 8 });
  }
  function overlay(k, p, tone) {
    if (tone === 'shadow') {
      for (const [x, y, r] of [[30, 34, 6], [100, 28, 4.5], [96, 98, 7], [26, 96, 4], [64, 18, 3.5]]) { p.glow(x, y, r * 2.4, SPORE, 0.7); p.part(k.circ(x, y, r), [[0, '#e0b0ff'], [1, '#5a1a9a']], { ow: 2.5, hi: false }); }
      p.glow(64, 64, 14, '#ff2a4a', 0.35);
    } else if (tone === 'radiant') {
      for (const [x, y, r] of [[28, 30, 7], [102, 36, 6], [98, 100, 8], [30, 98, 5]]) p.sparkle(x, y, r, '#fffbe0');
    }
  }
  function backdrop(p, tone) {
    if (tone === 'shadow') p.glow(64, 64, 52, SPORE, 0.4);
    else if (tone === 'radiant') { p.glow(64, 64, 58, '#fff6c0', 0.75); p.glow(64, 64, 40, '#ffffff', 0.5); }
  }

  const art = {};
  for (const [key, draw] of Object.entries(COMP_MOTIF)) {
    art[`rec_${key}`] = (p, k) => { k.frame(p, 'square', 'bronze', [CC[key]], { w: 6 }); p.obj(() => p.at(64, 64, 1, 0, () => draw(p, k, toner(k, 'base')))); };
    art[`rec_s${key}`] = (p, k) => {
      k.frame(p, 'hex', 'dark', ['#24082e', k.dk(CC[key], 0.3)], { w: 6 });
      backdrop(p, 'shadow');
      p.obj(() => p.at(64, 64, 0.95, 0, () => draw(p, k, toner(k, 'shadow'))));
      overlay(k, p, 'shadow');
    };
  }
  art.rec_egg = (p, k) => { k.frame(p, 'square', 'bronze', ['#fff3b0'], { w: 6 }); p.obj(() => p.at(64, 63, 1, 0, () => k.M.egg(p, '#fff3b0', [[-14, -10, 8, '#ff8a4a'], [14, 4, 10, '#ff8a4a'], [-6, 26, 7, '#ff8a4a'], [18, -24, 5, '#5fb0ff']]))); };
  art.rec_golden_egg = (p, k) => { k.frame(p, 'square', 'gold', ['#ffd84a', '#fff3b0'], { w: 8 }); p.glow(64, 64, 50, '#fff0a0', 0.6); p.obj(() => p.at(64, 63, 1, 0, () => k.M.egg(p, '#ffd84a', [[-14, -10, 8, '#fff6c0'], [14, 4, 10, '#fff6c0'], [-6, 26, 7, '#ffb02e']]))); };
  art.rec_remover = (p, k) => {
    k.frame(p, 'square', 'steel', ['#ff8fa3', '#8a8f9c'], { w: 7 });
    p.obj(() => p.at(64, 66, 1, 0, () => {
      p.tube('M-24 30V-6A24 24 0 0 1 24 -6V30', 16, '#e2394f', { cap: 'butt' });
      for (const x of [-24, 24]) p.part(k.rr(x - 9, 24, 18, 14, 2), 'chrome', { ow: 4 });
      p.line('M-30 -36L-22 -26M0 -42V-30M30 -36L22 -26', '#9fdcff', 3, 0.9);
    }));
  };
  art.rec_tome = (p, k) => {
    k.frame(p, 'square', 'gold', ['#c9a3ff', '#4a2c6a'], { w: 7 });
    p.obj(() => p.at(64, 64, 1, -8, () => {
      p.part(k.rr(-30, -36, 60, 72, 6), '#5a2c8a');
      p.part(k.rr(-24, -30, 48, 60, 4), '#f6ecd6', { ow: 3 });
      p.at(0, 0, 0.8, 0, () => k.M.tag(p, '#fff3b0'));
      p.at(0, 0, 0.7, 0, () => p.part(k.star(0, 0, 14, 6, 5), '#ffcf4a', { ow: 4 }));
    }));
  };
  const ROWS = DT.Sets.def('reckoning').items.recipes;
  for (const r of ROWS) {
    const key = r.id.slice(4);
    const a = r.a.slice(4), b = r.b.slice(4);
    const draw = MOTIF[key];
    if (!draw) continue;
    for (const [id, tone] of [[r.id, 'base'], [`rec_s_${key}`, 'shadow'], [`rec_r_${key}`, 'radiant']]) {
      art[id] = (p, k) => {
        const t = toner(k, tone);
        const A = t(CC[a]), B = t(CC[b]);
        framed(k, p, tone, A, B);
        backdrop(p, tone);
        p.obj(() => p.at(64, 64, tone === 'base' ? 1 : 0.92, 0, () => draw(p, k, A, B, t)));
        overlay(k, p, tone);
      };
    }
  }
  // Crests: every emblem gets a shield crest with its own symbol
  const CREST_SYM = { DSP: 'skull', TRK: 'die', SER: 'cross', NGT: 'moon', DAY: 'sun', CHI: 'bug', WIT: 'star5', DRK: 'flame', HNT: 'dagger', IRN: 'shieldIron',
    PHA: 'brokenwing', SEN: 'star6', RIV: 'chess', ETR: 'copy', INA: 'refresh', ASN: 'blood', BRW: 'badge', CAV: 'up', INV: 'drop', KNT: 'shield',
    LEG: 'sword', MYS: 'pearl', RNG: 'crosshair', REN: 'plus', SKR: 'chevrons', SPW: 'chip', CAN: 'xros' };
  for (const [t, sym] of Object.entries(CREST_SYM)) {
    const T = DT.Sets.def('reckoning').traits[t];
    art[`emblem_${t.toLowerCase()}`] = (p, k) => k.crest(p, T.color, sym);
  }
  // augments: round badge, metal by tier, a symbol
  const AUG = {
    rec_a_coins: ['coin', '#ffd23f'], rec_a_cache: ['chip', SPORE], rec_a_scan: ['refresh', '#4fb3ff'], rec_a_heart: ['plus', '#ff5d73'],
    rec_a_training: ['sword', '#ff7a45'], rec_a_mist: ['skull', '#6a5aa0'], rec_a_front: ['shieldIron', '#c0c6d0'], rec_a_nest: ['crosshair', '#5ad1ff'],
    rec_a_bank: ['coin', '#ffcf4a'], rec_a_trove: ['crestTag', SPORE], rec_a_egg: ['egg', '#fff3b0'], rec_a_income: ['up', '#7dff9a'],
    rec_a_xanti: ['xros', '#ff4fa0'], rec_a_vamp: ['blood', '#ff3b4e'], rec_a_mana: ['drop', '#4fb3ff'], rec_a_unity: ['star6', '#f0d890'],
    rec_a_ascend: ['chevrons', '#fff0a0'], rec_a_jackpot: ['star5', '#ffd84a'], rec_a_blessing: ['sun', '#ffe680'], rec_a_armory: ['crestTag', '#ff9a45'],
    rec_a_nature: ['egg', '#ffd84a'], rec_a_darkness: ['moon', '#8a3cff'], rec_a_giant: ['badge', '#d9773a'], rec_a_tome: ['copy', '#c9a3ff'],
  };
  const TIER_METAL = ['', 'silver', 'gold', 'prism'];
  for (const a of DT.Sets.def('reckoning').augments) {
    const [sym, col] = AUG[a.id] || ['star6', '#ffffff'];
    art[`aug:${a.id}`] = (p, k) => {
      k.frame(p, 'circle', TIER_METAL[a.tier] || 'silver', [k.dk(col, 0.25)], { w: 7 });
      p.obj(() => p.at(64, 64, 2.1, 0, () => { p.glow(0, 0, 16, col, 0.5); (k.SYM[sym] || k.SYM.star6)(p, col); }));
    };
  }

  // ---------- move looks (js/moves.js spec format) ----------
  const S = (elem, charge, impact, fx, o = {}) => ({ elem, charge, impact, fx, ...o });
  const moves = {
    // 1-cost
    'Twenty Dive': S('holy', 'none', 'hit', [['dash', { ghosts: 3, slashes: 1 }]], { release: 0.12 }),
    'Road Two Decade': S('holy', 'hands', 'holy', [['slash', { count: 2, size: 1.2 }]], { shake: 0.08 }),
    'Grade Slash': S('holy', 'hands', 'holy', [['slash', { count: 3, size: 1.5, spread: 0.6 }], ['cross', { at: 0.15, size: 0.8 }]], { shake: 0.14 }),
    'Lullaby Bubble': S('water', 'mouth', 'splash', [['swarm', { cell: 'bubble', count: 8, size: 0.3, speed: 6 }]]),
    'Symphony Crusher': S('wind', 'ground', 'wind', [['swarm', { cell: 'note', count: 10, size: 0.35, speed: 7 }], ['shock', { at: 0.3, debris: false, cell: 'note' }]], { shake: 0.08 }),
    'Musical Fist': S('beast', 'ground', 'hit', [['shock', { hammer: true, cell: 'note', size: 1.4 }], ['swarm', { at: 0.1, cell: 'note', count: 12, size: 0.4, speed: 8, spread: 1 }]], { shake: 0.2 }),
    'Crazy Giggle': S('dark', 'mouth', 'dark', [['beam', { width: 0.14, dur: 0.35 }]]),
    'Data Crusher': S('dark', 'hands', 'dark', [['beam', { width: 0.2, dur: 0.4, glyph: 'x' }]], { shake: 0.06 }),
    'Spider Shooter': S('dark', 'back', 'dark', [['volley', { count: 10, rate: 0.03, cell: 'needle', size: 0.4, speed: 18 }]], { shake: 0.1 }),
    'Sticky Net': S('plant', 'mouth', 'poison', [['net', { size: 0.9 }]]),
    'Spiking Strike': S('plant', 'none', 'poison', [['dash', { ghosts: 3, slashes: 1, claw: true }]], { release: 0.12, shake: 0.06 }),
    'Hell Masquerade': S('dark', 'none', 'poison', [['dash', { ghosts: 5, slashes: 3 }], ['slash', { at: 0.2, count: 4, size: 1.2 }]], { release: 0.12, shake: 0.14 }),
    'Cheese Bomb': S('fire', 'hands', 'blast', [['orb', { size: 0.22, speed: 10, arc: 0.8, head: 'square', color: '#ffd84a' }]]),
    'Poop Toss': S('beast', 'hands', 'poison', [['orb', { size: 0.26, speed: 9, arc: 1, color: '#8a5a2a' }]], { colors: ['#8a5a2a', '#d9a066'] }),
    'King Stamp': S('beast', 'sky', 'hit', [['shock', { hammer: true, size: 1.5 }]], { shake: 0.22, colors: ['#ffd84a', '#8a5a2a'] }),
    'Corona Flame': S('fire', 'hands', 'blast', [['orb', { size: 0.26, speed: 12, trail: 'flame' }]]),
    'Stun Beat Blow': S('holy', 'hands', 'holy', [['streak', { width: 0.5, dur: 0.2, trail: 'holy' }], ['shock', { at: 0.15, size: 0.8 }]], { shake: 0.1 }),
    'Glass Slash': S('holy', 'hands', 'holy', [['wave', { width: 0.8, count: 1 }], ['slash', { at: 0.2, count: 2, size: 1.4 }]], { shake: 0.16, colors: ['#bfefff', '#ffffff'] }),
    'Ice Prism': S('ice', 'hands', 'frost', [['orb', { size: 0.22, head: 'shard', trail: 'ice', speed: 12 }]]),
    'Ice Illusion': S('ice', 'hands', 'frost', [['spikes', { kind: 'ice', count: 4 }]], { shake: 0.06 }),
    'Cold Flame': S('ice', 'hands', 'frost', [['breath', { dur: 0.5, width: 0.45, cell: 'flame', size: 0.4 }]], { colors: ['#4a8bff', '#d0f0ff'], shake: 0.1 }),
    'Sharp Sider': S('water', 'none', 'splash', [['streak', { width: 0.5, dur: 0.2, trail: 'bubble' }]], { release: 0.12 }),
    'Harpoon Torpedo': S('water', 'ground', 'splash', [['missiles', { count: 1, size: 0.2, curve: 0.3, arc: 0.3, dur: 0.4 }]], { shake: 0.1 }),
    Mjolnir: S('thunder', 'sky', 'zap', [['shock', { hammer: true, cell: 'bolt', size: 1.4 }], ['bolt', { at: 0.1, from: 'sky', count: 2 }]], { shake: 0.24 }),
    'Shell Guard': S('metal', 'aura', 'metal', [['dome', { size: 1 }]]),
    'Guardian Bomb': S('metal', 'aura', 'metal', [['dome', { size: 1.1 }], ['shock', { at: 0.2, debris: true }]], { shake: 0.08 }),
    'Grand Smash': S('metal', 'sky', 'metal', [['dome', { size: 1.3, orbit: 'blade', count: 3 }], ['shock', { at: 0.2, hammer: true }]], { shake: 0.16 }),
    'Mickey Bullet': S('holy', 'hands', 'holy', [['volley', { count: 4, rate: 0.06, size: 0.3, speed: 22, from: 'hands' }]]),
    'Bless Fire': S('fire', 'hands', 'blast', [['volley', { count: 7, rate: 0.05, size: 0.35, speed: 22, from: 'hands', cell: 'flame' }]], { shake: 0.06 }),
    'Meteor Flame': S('fire', 'sky', 'blast', [['meteor', { size: 0.6, count: 1 }]], { shake: 0.16 }),
    'Demi Darts': S('dark', 'hands', 'dark', [['orb', { size: 0.16, speed: 16, head: 'needle', trail: 'dark', count: 3, gap: 0.05 }]]),
    'Laser Wing': S('dark', 'back', 'dark', [['beam', { width: 0.18, dur: 0.35, twin: true }]], { shake: 0.06 }),
    'Nail Bone': S('dark', 'hands', 'dark', [['beam', { width: 0.26, dur: 0.45, from: 'hands' }], ['glyph', { glyph: 'x', size: 1 }]], { shake: 0.12 }),
    'Undead Fang': S('dark', 'mouth', 'dark', [['slash', { count: 2, claw: true, size: 0.9 }], ['drain', {}]]),
    'Drain Slash': S('dark', 'hands', 'dark', [['slash', { count: 2, size: 1.2 }], ['drain', { at: 0.2 }]], { shake: 0.06 }),
    'Thousand Arrows': S('dark', 'hands', 'dark', [['arrows', { count: 10, rain: true, size: 0.6 }], ['drain', { at: 0.35 }]], { shake: 0.1 }),
    'Plastic Blaze': S('fire', 'mouth', 'blast', [['orb', { size: 0.24, speed: 11, trail: 'flame' }]]),
    'Meteor Shower': S('fire', 'sky', 'blast', [['meteor', { size: 0.35, count: 3 }]], { shake: 0.1, colors: ['#ffd84a', '#fff6c0'] }),
    'Smiley Bomb': S('fire', 'hands', 'blast', [['orb', { size: 0.4, speed: 9, arc: 1, impactSize: 1.8 }]], { shake: 0.2 }),
    // 2-cost
    Spitfire: S('fire', 'mouth', 'blast', [['orb', { size: 0.22, speed: 13, trail: 'flame' }]]),
    'Burning Fist': S('fire', 'hands', 'blast', [['orb', { size: 0.3, speed: 12, trail: 'flame', head: 'flame' }]], { shake: 0.06 }),
    'Heavy Metal Fire': S('fire', 'hands', 'blast', [['whip', { dur: 0.4, width: 0.14, count: 2 }], ['erupt', { at: 0.2, kind: 'flame', count: 2 }]], { shake: 0.14 }),
    'Little Horn': S('ice', 'none', 'frost', [['streak', { width: 0.5, dur: 0.22, trail: 'ice' }]], { release: 0.12 }),
    'Freeze Fang': S('ice', 'mouth', 'frost', [['slash', { count: 2, claw: true }], ['erupt', { at: 0.1, kind: 'ice', count: 2 }]], { shake: 0.08 }),
    'Full Moon Kick': S('dark', 'none', 'dark', [['streak', { width: 0.8, dur: 0.25, trail: 'dark' }], ['shock', { at: 0.2, size: 1.1 }]], { shake: 0.16 }),
    'Scratch Smash': S('wind', 'none', 'hit', [['streak', { width: 0.4, dur: 0.18, trail: 'wind' }]], { release: 0.1 }),
    'Kunai Wing': S('wind', 'back', 'wind', [['volley', { count: 6, rate: 0.04, cell: 'blade', size: 0.35, speed: 20 }]], { shake: 0.06 }),
    'Fire Jewels': S('fire', 'back', 'blast', [['missiles', { count: 4, size: 0.16, curve: 1, arc: 1 }]], { shake: 0.12 }),
    'Bada Boom': S('fire', 'hands', 'blast', [['orb', { size: 0.22, speed: 12, trail: 'flame' }]]),
    'Thunder Cloud': S('thunder', 'sky', 'zap', [['bolt', { from: 'sky', count: 2 }]], { shake: 0.08 }),
    'Pandora Dialogue': S('dark', 'hands', 'dark', [['portal', { size: 1 }], ['beam', { at: 0.2, width: 0.22, dur: 0.4 }]], { shake: 0.12 }),
    'Cog Crusher': S('metal', 'hands', 'metal', [['shock', { cell: 'gear', size: 0.8 }]]),
    'Destruction Grenade': S('metal', 'back', 'metal', [['missiles', { count: 2, size: 0.16, arc: 1 }], ['shock', { at: 0.4, size: 1 }]], { shake: 0.12 }),
    'Gatling Missile': S('metal', 'back', 'metal', [['missiles', { count: 5, size: 0.17, curve: 1.2, arc: 0.8, smoke: true }], ['shock', { at: 0.45, size: 1.4 }]], { shake: 0.2 }),
    'Snow Bomber': S('ice', 'hands', 'frost', [['orb', { size: 0.26, speed: 10, arc: 0.6, head: 'snow', trail: 'ice' }]]),
    'Sub-zero Ice Punch': S('ice', 'hands', 'frost', [['streak', { width: 0.5, dur: 0.2, trail: 'ice' }], ['spikes', { at: 0.15, kind: 'ice', count: 3 }]], { shake: 0.1 }),
    'Blizzard Breath': S('ice', 'mouth', 'frost', [['breath', { dur: 0.6, width: 0.6, cell: 'snow', size: 0.42, rate: 300 }]], { shake: 0.14 }),
    'Knuckle Beater': S('beast', 'hands', 'hit', [['slash', { count: 2, size: 1 }]]),
    'Fang Strike': S('fire', 'hands', 'blast', [['streak', { width: 0.5, dur: 0.2, trail: 'flame' }], ['slash', { at: 0.12, count: 3, claw: true }]], { shake: 0.1 }),
    'Desolation Claw': S('dark', 'hands', 'dark', [['wave', { width: 1, count: 2 }], ['slash', { at: 0.15, count: 4, claw: true, size: 1.3 }]], { shake: 0.18 }),
    'Sing a Song': S('plant', 'aura', 'none', [['swarm', { cell: 'note', count: 8, size: 0.3, speed: 5, to: 'targets' }], ['heal', {}]]),
    'Sunshine Beam': S('holy', 'sky', 'none', [['pillar', { size: 0.8 }], ['heal', { at: 0.2 }]]),
    'Lila Shower': S('plant', 'sky', 'none', [['rain', { cell: 'petal', count: 20 }], ['heal', { at: 0.2, cell: 'petal' }]]),
    'Air Shot': S('wind', 'mouth', 'wind', [['orb', { size: 0.24, speed: 12, head: 'ring', trail: 'wind' }]]),
    "Heaven's Knuckle": S('holy', 'hands', 'holy', [['sphere', { size: 0.5, from: 'hands', trail: 'holy' }]], { shake: 0.1 }),
    'Gate of Destiny': S('holy', 'sky', 'holy', [['portal', { size: 1.4, dur: 0.9 }], ['glyph', { glyph: 'circle', size: 1.4 }]], { shake: 0.18 }),
    'Bad Message': S('dark', 'mouth', 'dark', [['whip', { dur: 0.35, width: 0.08 }]]),
    "Hell's Hand": S('dark', 'hands', 'dark', [['whip', { dur: 0.45, width: 0.12, count: 2, sway: 0.6 }]], { shake: 0.08 }),
    'Soul Chopper': S('dark', 'hands', 'dark', [['whip', { dur: 0.5, width: 0.16 }], ['slash', { at: 0.25, count: 2, size: 1.5, vertical: true }]], { shake: 0.16 }),
    'Goblin Strike': S('beast', 'hands', 'hit', [['slash', { count: 1, size: 1 }]]),
    'Bone Cudgel': S('beast', 'hands', 'hit', [['shock', { hammer: true, size: 0.9, cell: 'skull' }]], { shake: 0.1 }),
    'Phantom Blade': S('dark', 'hands', 'dark', [['wave', { width: 1.1, count: 1 }], ['slash', { at: 0.2, count: 2, size: 1.6 }]], { shake: 0.18 }),
    'Lop-Ear Ripple': S('water', 'hands', 'splash', [['rings', { count: 3, size: 0.4 }]]),
    'Tear Arrow': S('water', 'hands', 'splash', [['arrows', { count: 3, size: 0.6, cell: 'drop' }]], { shake: 0.06 }),
    'Ice Archery': S('ice', 'sky', 'frost', [['arrows', { count: 8, rain: true, size: 0.7, cell: 'shard' }]], { shake: 0.12 }),
    'Blazing Fire': S('fire', 'mouth', 'blast', [['orb', { size: 0.22, speed: 13, trail: 'flame' }]]),
    'Gargo Laser': S('metal', 'hands', 'metal', [['volley', { count: 10, rate: 0.03, size: 0.35, speed: 24, guns: 2 }]], { shake: 0.06 }),
    'Miracle Missile': S('metal', 'back', 'metal', [['missiles', { count: 6, size: 0.16, curve: 1.3, arc: 1, smoke: true }]], { shake: 0.16 }),
    // 3-cost
    'Baby Breath': S('fire', 'mouth', 'blast', [['breath', { dur: 0.35, width: 0.25, size: 0.3 }]]),
    'Blue Flare Breath': S('fire', 'mouth', 'blast', [['breath', { dur: 0.5, width: 0.4, size: 0.4 }]], { colors: ['#3a8bff', '#d0ecff'], shake: 0.08 }),
    'Blaze Sonic Breath': S('wind', 'mouth', 'wind', [['beam', { width: 0.36, dur: 0.5, charge: 0.12, spiral: true }]], { colors: ['#4ab0ff', '#ffffff'], shake: 0.16 }),
    'Sparkling Thunder': S('thunder', 'sky', 'zap', [['bolt', { from: 'caster', width: 0.07 }]]),
    'Fist of the Beast King': S('beast', 'hands', 'hit', [['sphere', { size: 0.6, from: 'hands', trail: 'flame', cell: 'paw' }]], { colors: ['#ffb04a', '#fff0c0'], shake: 0.12 }),
    'Cyclonic Kick': S('wind', 'ground', 'wind', [['tornado', { height: 2, size: 1.2 }], ['shock', { at: 0.2, size: 1.2 }]], { shake: 0.18 }),
    'Rain of Pollen': S('plant', 'sky', 'leaf', [['rain', { cell: 'petal', count: 14 }]]),
    'Needle Spray': S('plant', 'aura', 'hit', [['volley', { count: 14, rate: 0.02, cell: 'needle', size: 0.45, spread: 0.5, from: 'chest', speed: 20 }]], { colors: ['#c8ff6a', '#fff6c0'] }),
    'Pit Bomb': S('plant', 'hands', 'leaf', [['swarm', { cell: 'star', count: 10, size: 0.3, speed: 7 }], ['shock', { at: 0.3, debris: false, cell: 'star' }]], { shake: 0.1, colors: ['#ff9ad8', '#fff0fa'] }),
    'Puppy Howl': S('holy', 'mouth', 'holy', [['rings', { count: 2, size: 0.4 }]]),
    'Lightning Paw': S('thunder', 'hands', 'zap', [['beam', { width: 0.16, dur: 0.3 }]], { shake: 0.06 }),
    'Celestial Arrow': S('holy', 'hands', 'holy', [['beam', { width: 0.3, dur: 0.45, charge: 0.12, core: '#ffffff' }], ['arrows', { at: 0.1, count: 4, size: 0.7 }]], { shake: 0.14 }),
    'Electric Stun Blast': S('thunder', 'hands', 'zap', [['orb', { size: 0.2, speed: 14, head: 'bolt', trail: 'zap' }]]),
    'Hyper Heat': S('fire', 'back', 'blast', [['volley', { count: 8, rate: 0.04, size: 0.4, speed: 18, cell: 'flame' }]], { shake: 0.08 }),
    'Double Impact': S('dark', 'hands', 'dark', [['volley', { count: 12, rate: 0.03, size: 0.35, speed: 26, guns: 2 }]], { shake: 0.12 }),
    "Cat's Eye": S('dark', 'aura', 'dark', [['glyph', { glyph: 'circle', size: 0.9 }]]),
    'Darkness Wave': S('dark', 'hands', 'dark', [['swarm', { cell: 'bat', count: 12, size: 0.35, speed: 8 }], ['vortex', { at: 0.3, size: 1 }]], { shake: 0.1 }),
    'Phantom Pain': S('dark', 'hands', 'dark', [['vortex', { size: 1.4, dur: 0.8 }], ['erupt', { at: 0.2, kind: 'dark', to: 'center', area: true, count: 4 }]], { shake: 0.18 }),
    'Bullet Whirlwind': S('wind', 'none', 'wind', [['dash', { ghosts: 3, slashes: 1 }]], { release: 0.12 }),
    'Lion Claw': S('beast', 'none', 'hit', [['dash', { ghosts: 4, slashes: 2, claw: true }]], { release: 0.12, shake: 0.08 }),
    'Wind Cutter Sword': S('wind', 'none', 'wind', [['dash', { ghosts: 5, slashes: 3 }], ['tornado', { at: 0.2, size: 0.8 }]], { release: 0.12, shake: 0.14 }),
    'Scissor Claw': S('beast', 'hands', 'hit', [['slash', { count: 2, size: 1, claw: true }], ['drain', {}]]),
    'Double Scissor Claw': S('beast', 'hands', 'hit', [['slash', { count: 4, size: 1.2, claw: true }], ['drain', { at: 0.2 }]], { shake: 0.08 }),
    'Dimension Scissor': S('dark', 'hands', 'dark', [['portal', { size: 1.2 }], ['slash', { at: 0.2, count: 4, size: 1.6, claw: true }]], { shake: 0.18 }),
    'Nightmare Syndrome': S('dark', 'mouth', 'dark', [['orb', { size: 0.3, speed: 9, trail: 'dark', head: 'wisp' }]]),
    'Volcanic Strike': S('fire', 'mouth', 'blast', [['orb', { size: 0.4, speed: 10, trail: 'flame' }], ['erupt', { at: 0.35, kind: 'flame', count: 2 }]], { shake: 0.1 }),
    'Ultimate Slicer': S('metal', 'hands', 'metal', [['slash', { count: 3, size: 1.6, claw: true }], ['wave', { at: 0.1, width: 1 }]], { shake: 0.18 }),
    'Blazing Ice': S('ice', 'hands', 'frost', [['erupt', { kind: 'ice', to: 'center', area: true, count: 3 }]]),
    'Gauntlet Claw': S('beast', 'hands', 'hit', [['slash', { count: 3, size: 1.2, claw: true }], ['shock', { at: 0.15, size: 1 }]], { shake: 0.1 }),
    Asipatravana: S('holy', 'hands', 'holy', [['wave', { width: 1.2, count: 2 }], ['erupt', { at: 0.2, kind: 'metal', to: 'center', area: true, count: 4 }]], { shake: 0.18 }),
    'Spiral Twister': S('plant', 'mouth', 'blast', [['orb', { size: 0.22, speed: 11, arc: 0.3, trail: 'flame', side: 0.3, wobble: 0.5 }]], { colors: ['#3fff7a', '#d0ffd8'] }),
    'Meteor Wing': S('fire', 'sky', 'none', [['heal', { cell: 'feather' }], ['rain', { cell: 'feather', count: 10 }]]),
    'Shadow Wing': S('fire', 'sky', 'none', [['heal', { cell: 'feather' }], ['tornado', { cell: 'feather', height: 2 }]], { colors: ['#ff8a2a', '#fff0a0'] }),
    'Katana Attack': S('metal', 'hands', 'metal', [['slash', { count: 1, size: 1.2 }]]),
    'Metal Armor Blade': S('metal', 'hands', 'metal', [['slash', { count: 2, size: 1.4 }], ['wave', { at: 0.1, width: 0.6 }]], { shake: 0.08 }),
    'Legendary Dragon Blade': S('wind', 'hands', 'wind', [['wave', { width: 1.2, count: 3 }], ['tornado', { at: 0.2, size: 1 }]], { shake: 0.18 }),
    'Poison Ivy': S('plant', 'hands', 'poison', [['whip', { dur: 0.4, width: 0.1, count: 2, sway: 1.2 }]]),
    'Sharp Leaf': S('plant', 'hands', 'leaf', [['swarm', { cell: 'leaf', count: 10, size: 0.3, speed: 12 }]], { shake: 0.06 }),
    'Flower Cannon': S('plant', 'hands', 'leaf', [['glyph', { glyph: 'circle', size: 1.4, dur: 0.6 }], ['beam', { width: 0.24, dur: 0.45, charge: 0.1, color: '#ff8ae0', core: '#ffffff' }]], { colors: ['#ff7ad8', '#fff0fa'], shake: 0.12 }),
    // 4-cost
    'Rolling Upper': S('beast', 'hands', 'hit', [['shock', { size: 0.9 }]]),
    'Gaoga Hound': S('beast', 'ground', 'hit', [['vortex', { size: 1 }], ['shock', { at: 0.2, size: 1.1 }]], { shake: 0.1 }),
    'Howling Cannon': S('holy', 'sky', 'holy', [['sphere', { size: 0.9, from: 'above', trail: 'holy' }], ['shock', { at: 0.5, size: 1.6 }]], { colors: ['#cfe0ff', '#ffffff'], shake: 0.22 }),
    'Dash Metal': S('metal', 'mouth', 'metal', [['orb', { size: 0.24, speed: 14, head: 'gear', trail: 'smoke' }]]),
    'Power Metal': S('metal', 'mouth', 'metal', [['orb', { size: 0.36, speed: 12, head: 'gear', trail: 'smoke' }]], { shake: 0.1 }),
    'Metal Meteor': S('metal', 'sky', 'metal', [['meteor', { size: 0.8, count: 1, trail: 'smoke' }]], { shake: 0.24 }),
    'Stinking Gas': S('plant', 'mouth', 'poison', [['breath', { dur: 0.5, width: 0.5, cell: 'smoke' }]], { colors: ['#8ad04a', '#d0ff9a'] }),
    'Necro Magic': S('dark', 'hands', 'dark', [['swarm', { cell: 'bat', count: 10, size: 0.35, speed: 8 }], ['drain', { at: 0.3 }]], { colors: ['#ff8a2a', '#2a0a4a'], shake: 0.08 }),
    'Form Taranis': S('dark', 'sky', 'dark', [['swarm', { cell: 'card', count: 14, size: 0.4, speed: 9, spin: 3 }], ['vortex', { at: 0.2, size: 1.3 }]], { shake: 0.16 }),
    'Green Flare Breath': S('plant', 'mouth', 'blast', [['breath', { dur: 0.5, width: 0.45, cell: 'flame', size: 0.42 }], ['shock', { at: 0.3 }]], { colors: ['#4aff6a', '#e0ffd0'], shake: 0.1 }),
    'Megaton Hammer Crush': S('metal', 'sky', 'metal', [['shock', { hammer: true, size: 1.4 }], ['erupt', { at: 0.2, kind: 'rock', count: 4, area: true }]], { shake: 0.24 }),
    'Infinity Bowling': S('metal', 'ground', 'metal', [['wheel', { size: 1.2, saw: true }], ['shock', { at: 0.3, size: 1.8 }]], { shake: 0.3 }),
    'Branch Drain': S('plant', 'hands', 'leaf', [['whip', { dur: 0.4, width: 0.12, leafy: true }]]),
    'Illusion Mist': S('plant', 'aura', 'leaf', [['vortex', { size: 1, cell: 'petal' }]]),
    'Puppet Pummel': S('beast', 'hands', 'hit', [['portal', { size: 1 }], ['shock', { at: 0.2, size: 1.2 }]], { shake: 0.12 }),
    'Fifth Rush': S('beast', 'hands', 'hit', [['slash', { count: 2, size: 1 }]]),
    'Fif Cross': S('holy', 'hands', 'holy', [['slash', { count: 2, size: 1.3 }], ['cross', { at: 0.12, size: 0.8 }]], { shake: 0.1 }),
    Weltgeist: S('holy', 'hands', 'holy', [['wave', { width: 1.1, count: 3 }], ['slash', { at: 0.2, count: 4, size: 1.6 }]], { shake: 0.22 }),
    'Feather Slash': S('wind', 'hands', 'wind', [['swarm', { cell: 'feather', count: 8, size: 0.3, speed: 12 }]]),
    'Blast Laser': S('wind', 'mouth', 'wind', [['beam', { width: 0.24, dur: 0.4 }], ['shock', { at: 0.3 }]], { shake: 0.1 }),
    'Static Force': S('thunder', 'hands', 'zap', [['sphere', { size: 0.7, from: 'hands', trail: 'zap', cell: 'bolt' }], ['bolt', { at: 0.5, to: 'center', count: 2, area: 3 }]], { shake: 0.2 }),
    'M16 Assassin': S('metal', 'back', 'metal', [['missiles', { count: 3, size: 0.15, curve: 1, arc: 0.8 }]]),
    'Psycho Blaster': S('thunder', 'hands', 'zap', [['volley', { count: 8, rate: 0.04, size: 0.35, speed: 22, cell: 'bolt' }]], { shake: 0.08 }),
    'Giant Missile': S('metal', 'back', 'metal', [['missiles', { count: 2, size: 0.32, curve: 0.6, arc: 1.2, smoke: true }]], { shake: 0.24 }),
    'Mega Flame': S('fire', 'mouth', 'blast', [['orb', { size: 0.4, speed: 11, trail: 'flame' }]], { shake: 0.06 }),
    'Giga Destroyer': S('fire', 'back', 'blast', [['missiles', { count: 2, size: 0.18, organic: true, curve: 1.4, arc: 1.4 }]], { shake: 0.14 }),
    'Terra Destroyer': S('dark', 'sky', 'dark', [['sphere', { size: 1, from: 'above', trail: 'dark' }], ['shock', { at: 0.5, size: 1.8 }]], { shake: 0.26 }),
    'Vee Headbutt': S('beast', 'none', 'hit', [['streak', { width: 0.5, dur: 0.2, trail: 'wind' }]], { release: 0.12 }),
    'Magna Explosion': S('holy', 'aura', 'holy', [['dome', { size: 1.2 }], ['shock', { at: 0.2, size: 1.2 }]], { colors: ['#ffd84a', '#fff6c0'], shake: 0.12 }),
    'Extreme Jihad': S('holy', 'aura', 'holy', [['dome', { size: 1.5, orbit: 'star', count: 5 }], ['pillar', { at: 0.2, size: 1, to: 'self' }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.18 }),
    'Horn Blaster': S('holy', 'mouth', 'holy', [['beam', { width: 0.16, dur: 0.35 }]]),
    'Heat Wave': S('fire', 'mouth', 'blast', [['beam', { width: 0.3, dur: 0.5 }]], { shake: 0.1 }),
    "EDEN's Javelin": S('holy', 'sky', 'holy', [['beam', { width: 0.4, dur: 0.6, charge: 0.15, glyph: 'tri', core: '#ffffff' }]], { shake: 0.2 }),
    // 5-cost
    "Aurvandil's Arrow": S('holy', 'hands', 'holy', [['arrows', { count: 5, size: 0.8 }]], { shake: 0.08 }),
    Bifrost: S('holy', 'hands', 'holy', [['arrows', { count: 8, size: 0.9, cell: 'star' }], ['beam', { at: 0.1, width: 0.2, dur: 0.4 }]], { colors: ['#9fe0ff', '#fff0a0'], shake: 0.14 }),
    Brander: S('holy', 'sky', 'holy', [['arrows', { count: 12, rain: true, size: 1 }], ['pillar', { at: 0.3, size: 0.9 }]], { shake: 0.22 }),
    'Exhaust Flame': S('fire', 'mouth', 'blast', [['breath', { dur: 0.45, width: 0.4, size: 0.4 }]], { shake: 0.08 }),
    'Atomic Blaster': S('fire', 'back', 'blast', [['beam', { width: 0.34, dur: 0.5, twin: true }]], { shake: 0.16 }),
    'Megiddo Flame': S('fire', 'mouth', 'blast', [['breath', { dur: 0.7, width: 0.7, cell: 'flame', mix: 'skull', size: 0.5, rate: 340 }], ['erupt', { at: 0.3, kind: 'flame', area: true, count: 4 }]], { colors: ['#ff2a1a', '#ffb04a'], shake: 0.3 }),
    'Rock Breaker': S('fire', 'hands', 'blast', [['shock', { size: 1.2, debris: true }]], { shake: 0.1 }),
    'Lightning Joust': S('holy', 'hands', 'holy', [['bolt', { from: 'sky', count: 3, area: 3 }], ['cross', { at: 0.2, to: 'center', size: 1.2 }]], { shake: 0.2 }),
    'Invincible Sword': S('holy', 'sky', 'holy', [['pillar', { size: 1.4, height: 9, to: 'center' }], ['shock', { at: 0.3, size: 2 }]], { colors: ['#ff4a4a', '#fff0c0'], shake: 0.3 }),
    'Meteor Slash': S('holy', 'hands', 'holy', [['slash', { count: 2, size: 1 }]]),
    Byakusai: S('holy', 'hands', 'holy', [['slash', { count: 4, size: 1.3, spread: 0.7 }]], { shake: 0.1 }),
    'Zwei Glanze': S('holy', 'hands', 'holy', [['wheel', { size: 1.3 }], ['slash', { at: 0.15, count: 5, size: 1.6, spread: 0.8 }]], { shake: 0.2 }),
    'Time Buster': S('metal', 'hands', 'metal', [['glyph', { glyph: 'circle', size: 1.2 }], ['portal', { at: 0.2, size: 0.8, to: 'self' }]]),
    'Digital Bomb': S('metal', 'hands', 'metal', [['glyph', { glyph: 'x', size: 1.2 }], ['portal', { at: 0.2, size: 1, to: 'self' }]], { shake: 0.08 }),
    'Infinity Cannon': S('metal', 'back', 'metal', [['beam', { width: 0.4, dur: 0.6, twin: true }], ['portal', { at: 0.1, size: 1.2, to: 'self' }]], { shake: 0.24 }),
    'Grand Cross': S('holy', 'sky', 'holy', [['cross', { size: 1, orbs: 4 }]], { shake: 0.1 }),
    'Paradise Lost': S('dark', 'sky', 'dark', [['cross', { size: 1.2, orbs: 6 }], ['bolt', { at: 0.2, count: 2 }]], { shake: 0.16 }),
    'Divine Atonement': S('holy', 'sky', 'holy', [['cross', { size: 1.6, orbs: 8, dur: 0.9 }], ['pillar', { at: 0.3, size: 1.2 }]], { shake: 0.26 }),
    'Diamond Storm': S('holy', 'aura', 'none', [['swarm', { cell: 'shard', count: 12, size: 0.3, speed: 6, to: 'self' }], ['heal', { at: 0.2 }]]),
    'Talisman of Light': S('holy', 'aura', 'none', [['glyph', { glyph: 'circle', size: 1.6, place: 'ground' }], ['heal', { at: 0.2, cell: 'card' }]]),
    'Amethyst Mandala': S('holy', 'aura', 'none', [['glyph', { glyph: 'circle', size: 2, place: 'ground', dur: 0.9 }], ['dome', { at: 0.1, size: 1.6 }], ['heal', { at: 0.3, cell: 'star' }]], { colors: ['#c89aff', '#ffffff'], shake: 0.1 }),
    'Laughing Smasher': S('plant', 'hands', 'poison', [['orb', { size: 0.3, speed: 9, arc: 1, head: 'seed' }], ['erupt', { at: 0.4, kind: 'thorn', count: 2 }]], { colors: ['#c070ff', '#ffd0ff'] }),
    'Unidentified Flying Kiss': S('dark', 'sky', 'dark', [['swarm', { cell: 'heart', count: 10, size: 0.35, speed: 7 }], ['erupt', { at: 0.3, kind: 'dark', to: 'center', area: true, count: 3 }]], { colors: ['#ff6ad8', '#ffffff'], shake: 0.12 }),
    'Trump Sword': S('dark', 'sky', 'dark', [['swarm', { cell: 'blade', count: 14, size: 0.45, speed: 12 }], ['erupt', { at: 0.3, kind: 'dark', to: 'center', area: true, count: 5 }]], { shake: 0.22 }),
    'Crimson Lightning': S('dark', 'hands', 'dark', [['whip', { dur: 0.5, width: 0.12, lightning: true }]], { colors: ['#ff2a4a', '#ffb0c0'], shake: 0.1 }),
    'Venom Infusion': S('dark', 'mouth', 'poison', [['breath', { dur: 0.6, width: 0.5, cell: 'skull', size: 0.4 }]], { colors: ['#8a2aff', '#ff4a8a'], shake: 0.16 }),
    'Crystal Revolution': S('dark', 'sky', 'dark', [['spikes', { kind: 'ice', count: 6, size: 1.2 }], ['vortex', { at: 0.2, size: 1.4 }]], { colors: ['#ff2a4a', '#ffd0e0'], shake: 0.24 }),
    'Bear Fist': S('beast', 'hands', 'hit', [['shock', { size: 1 }]], { shake: 0.08 }),
    'Howling Crusher': S('thunder', 'sky', 'zap', [['shock', { size: 1.4, cell: 'bolt' }], ['bolt', { at: 0.1, from: 'sky', count: 3, area: 2 }]], { shake: 0.2 }),
    'Flashy Boss Punch': S('thunder', 'sky', 'zap', [['shock', { hammer: true, size: 1.8, cell: 'bolt' }], ['bolt', { at: 0.1, from: 'sky', count: 5, area: 3 }]], { colors: ['#ffe94a', '#ffffff'], shake: 0.3 }),
    // summons
    'Heat Viper': S('fire', 'mouth', 'blast', [['breath', { dur: 0.5, width: 0.5 }]], { shake: 0.12 }),
    'Hearts Attack': S('holy', 'back', 'holy', [['swarm', { cell: 'heart', count: 10, size: 0.4, speed: 8 }], ['shock', { at: 0.3, cell: 'heart' }]], { colors: ['#ff6aa8', '#ffe0f0'], shake: 0.1 }),
    'Fire Vortex': S('fire', 'mouth', 'blast', [['orb', { size: 0.3, speed: 10, trail: 'flame', count: 3, gap: 0.08 }]], { shake: 0.08 }),
    'Fox Fire': S('ice', 'none', 'frost', [['dash', { ghosts: 3, slashes: 2, claw: true }]], { colors: ['#4a9bff', '#d8f0ff'], release: 0.12 }),
    'Glare Eye': S('dark', 'aura', 'dark', [['dome', { size: 0.9 }]]),
    // wild Digimon
    'Poison Bubble': S('water', 'mouth', 'poison', [['orb', { size: 0.2, speed: 9, head: 'bubble', trail: 'bubble' }]], { colors: ['#9a5aff', '#e0c0ff'] }),
    'Nume-Sludge': S('plant', 'mouth', 'poison', [['orb', { size: 0.24, speed: 9, arc: 1 }]], { colors: ['#8ad04a', '#d0ff9a'] }),
    'Hyper Smell': S('plant', 'mouth', 'poison', [['breath', { dur: 0.4, width: 0.4, cell: 'smoke' }]], { colors: ['#9ac04a', '#e0ffb0'] }),
    'Fire Breath': S('fire', 'mouth', 'blast', [['breath', { dur: 0.45, width: 0.4 }]], { shake: 0.08 }),
    'Dark Network': S('dark', 'hands', 'dark', [['bolt', { count: 2, area: 2 }]], { shake: 0.08 }),
    'Nuclear Laser': S('metal', 'back', 'metal', [['beam', { width: 0.3, dur: 0.5 }]], { shake: 0.12 }),
    'Frost Claw': S('ice', 'hands', 'frost', [['slash', { count: 3, claw: true }]], { shake: 0.06 }),
    'Dark Shot': S('dark', 'back', 'dark', [['missiles', { count: 2, size: 0.25, organic: true, arc: 1.2 }]], { shake: 0.16 }),
    'King Monkick': S('beast', 'sky', 'hit', [['shock', { hammer: true, size: 1.4 }]], { shake: 0.2 }),
    'Giga Heat': S('fire', 'mouth', 'blast', [['beam', { width: 0.34, dur: 0.5 }], ['erupt', { at: 0.3, kind: 'flame', count: 3 }]], { shake: 0.18 }),
    'Rust Breath': S('metal', 'mouth', 'metal', [['breath', { dur: 0.6, width: 0.55, cell: 'smoke' }]], { colors: ['#c06a2a', '#ffd0a0'], shake: 0.16 }),
    'Forbidden Trident': S('water', 'hands', 'splash', [['beam', { width: 0.3, dur: 0.5, glyph: 'tri' }], ['erupt', { at: 0.3, kind: 'water', count: 3 }]], { shake: 0.18 }),
    'Evil Inferno': S('fire', 'sky', 'blast', [['meteor', { size: 0.6, count: 3 }]], { colors: ['#ff2a1a', '#ff9a4a'], shake: 0.22 }),
    Rostrum: S('water', 'mouth', 'splash', [['slash', { count: 3, claw: true, size: 1.4 }], ['drain', { at: 0.2 }]], { shake: 0.18 }),
    'Death Lure': S('dark', 'sky', 'dark', [['vortex', { size: 1.4 }], ['bolt', { at: 0.2, count: 3, area: 3 }]], { shake: 0.2 }),
    'Chains of Sloth': S('dark', 'ground', 'dark', [['whip', { dur: 0.5, width: 0.14, count: 3 }], ['shock', { at: 0.3, size: 1.6 }]], { shake: 0.22 }),
    'Dimension Destroyer': S('dark', 'sky', 'dark', [['portal', { size: 1.6 }], ['erupt', { at: 0.3, kind: 'dark', to: 'center', area: true, count: 5 }]], { shake: 0.26 }),
    'Darkness Zone': S('dark', 'aura', 'dark', [['vortex', { size: 1.8, dur: 0.9 }]], { shake: 0.2 }),
    'Black Rain': S('dark', 'sky', 'dark', [['rain', { cell: 'ember', count: 24 }], ['meteor', { at: 0.2, size: 0.4, count: 3 }]], { shake: 0.2 }),
  };

  // ---------- basic attacks ----------
  const shot = (elem, head, trail, size = 0.22) => ({ kind: 'shot', elem, head, trail, size });
  const basic = {
    rec_zubamon: { kind: 'slash', elem: 'holy' }, rec_otamamon: { kind: 'punch', elem: 'water' }, rec_keramon: shot('dark', 'dot', 'data'),
    rec_wormmon: { kind: 'sting', elem: 'plant' }, rec_chuumon: { kind: 'bite', elem: 'beast' }, rec_coronamon: { kind: 'punch', elem: 'fire' },
    rec_penguinmon: shot('ice', 'shard', 'ice'), rec_gomamon: { kind: 'claw', elem: 'water' }, rec_kamemon: { kind: 'punch', elem: 'metal' },
    rec_sisnoir: shot('holy', 'spark', 'holy', 0.18), rec_demidevimon: shot('dark', 'needle', 'dark', 0.18), rec_dracmon: shot('dark', 'bat', 'dark'),
    rec_toyagumon: shot('fire', 'square', 'flame'),
    rec_blackagumon: shot('fire', 'flame', 'flame'), rec_blackgabumon: { kind: 'horn', elem: 'ice' }, rec_falcomon: shot('wind', 'blade', 'wind', 0.2),
    rec_impmon: { kind: 'claw', elem: 'dark' }, rec_hagurumon: { kind: 'punch', elem: 'metal' }, rec_kumamon: { kind: 'punch', elem: 'ice' },
    rec_monodramon: { kind: 'punch', elem: 'beast' }, rec_lalamon: shot('plant', 'seed', 'leaf'), rec_patamon: shot('wind', 'ring', 'wind'),
    rec_tsukaimon: shot('dark', 'wisp', 'dark'), rec_goblimon: { kind: 'punch', elem: 'beast' }, rec_lunamon: shot('water', 'drop', 'bubble'),
    rec_terriermon: shot('metal', 'streak', null, 0.2),
    rec_dracomon: shot('fire', 'flame', 'flame'), rec_elecmon: { kind: 'kick', elem: 'thunder' }, rec_floramon: shot('plant', 'petal', 'petal'),
    rec_salamon: shot('holy', 'star', 'holy'), rec_gazimon: shot('metal', 'streak', null, 0.2), rec_blackgatomon: shot('dark', 'wisp', 'dark'),
    rec_kudamon: { kind: 'bite', elem: 'wind' }, rec_kuwagamon: { kind: 'claw', elem: 'beast' }, rec_tapirmon: { kind: 'bite', elem: 'beast' },
    rec_lopmon: { kind: 'kick', elem: 'ice' }, rec_biyomon: shot('fire', 'feather', 'flame'), rec_ryudamon: { kind: 'slash', elem: 'metal' },
    rec_palmon: shot('plant', 'leaf', 'leaf'),
    rec_gaomon: { kind: 'punch', elem: 'beast' }, rec_dorumon: shot('metal', 'gear', 'smoke', 0.26), rec_raremon: shot('plant', 'smoke', 'smoke'),
    rec_coredramon: { kind: 'tail', elem: 'plant' }, rec_woodmon: shot('plant', 'leaf', 'leaf'), rec_hackmon: { kind: 'slash', elem: 'holy' },
    rec_hawkmon: shot('wind', 'feather', 'wind'), rec_rapidarmor: shot('metal', 'streak', null, 0.2), rec_blackgreymon: { kind: 'claw', elem: 'fire' },
    rec_veemon: { kind: 'punch', elem: 'holy' }, rec_unimon: shot('holy', 'star', 'holy'),
    rec_valkyrimon: shot('holy', 'needle', 'holy', 0.26), rec_growlmon: { kind: 'claw', elem: 'fire' }, rec_guilmon: { kind: 'slash', elem: 'holy' },
    rec_sisciel: { kind: 'slash', elem: 'holy' }, rec_clockmon: shot('metal', 'gear', 'spark'), rec_lucemon: shot('holy', 'cross', 'holy'),
    rec_renamon: shot('holy', 'card', 'holy'), rec_mushroomon: shot('plant', 'seed', 'leaf'), rec_myotismon: { kind: 'claw', elem: 'dark' },
    rec_bearmon: { kind: 'punch', elem: 'thunder' },
    rec_kimeramon: { kind: 'claw', elem: 'fire' }, rec_daisy: { kind: 'punch', elem: 'holy' }, rec_gumdramon: shot('fire', 'flame', 'flame'),
    rec_wolf: { kind: 'bite', elem: 'ice' }, rec_voidspawn: { kind: 'bite', elem: 'dark' },
    rec_w_pagumon: { kind: 'bite', elem: 'dark' }, rec_w_numemon: { kind: 'punch', elem: 'plant' }, rec_w_geremon: shot('plant', 'smoke', 'smoke'),
    rec_w_tyrannomon: { kind: 'bite', elem: 'fire' }, rec_w_etemon: shot('dark', 'note', 'note'), rec_w_metaltyrannomon: { kind: 'claw', elem: 'metal' },
    rec_w_icedevimon: { kind: 'claw', elem: 'ice' }, rec_w_skullgreymon: { kind: 'claw', elem: 'dark' }, rec_w_kingetemon: { kind: 'kick', elem: 'beast' },
    rec_w_gigadramon: { kind: 'claw', elem: 'metal' }, rec_w_rusttyranomon: { kind: 'bite', elem: 'metal' }, rec_w_dragomon: shot('water', 'drop', 'bubble'),
    rec_w_daemon: shot('fire', 'ember', 'flame'), rec_w_leviamon: { kind: 'bite', elem: 'water' }, rec_w_barbamon: shot('dark', 'wisp', 'dark'),
    rec_w_belphemon: { kind: 'claw', elem: 'dark' }, rec_w_millenniummon: { kind: 'claw', elem: 'dark' }, rec_w_apocalymon: shot('dark', 'skull', 'dark'),
    rec_w_armageddemon: shot('dark', 'ember', 'dark', 0.3),
  };

  DT.Sets.extend('reckoning', { art: { prerendered: false, items: art, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
