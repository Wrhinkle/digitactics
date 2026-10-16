// Galaxies art: item, Crest, augment and Galaxy-card portraits (SVG painters with the js/itemart.js kit), move looks
// for every ability that has no shared spec, and a basic attack look for every line. Visual only.
// Formats: docs/sets/FORMAT.md (art) and the header of js/moves.js (move and basic attack specs).
(function (G) {
  const DT = G.DT;

  // ---------- item portraits ----------
  const COL = { gal_sword: '#ff7a45', gal_bow: '#ffd23f', gal_rod: '#b26bff', gal_tear: '#4fb3ff', gal_vest: '#c0c6d0', gal_cloak: '#3fd6b0', gal_belt: '#ff4d64', gal_glove: '#8fdc4e', gal_mental: '#ffcf4a' };
  const items = {};
  // little stars in the frame corners: the Galaxies signature
  const stars = (p, k, col = '#fff6c8') => { for (const [x, y, r] of [[22, 24, 4.5], [104, 22, 3.5], [106, 100, 4], [24, 104, 3]]) p.sparkle(x, y, r, col); };
  const comp = (id, fn) => { items[id] = (p, k) => { k.frame(p, 'square', 'bronze', [COL[id]], { w: 6 }); stars(p, k); p.obj(() => fn(p, k)); }; };
  const done = (id, fn) => {
    items[id] = (p, k) => {
      const it = DT.ITEMS && DT.ITEMS[id];
      const [a, b] = it && it.from ? it.from : ['gal_rod', 'gal_rod'];
      k.frame(p, 'square', 'gold', [COL[a], COL[b]], { w: 8 });
      stars(p, k);
      p.obj(() => fn(p, k, COL[a], COL[b]));
    };
  };
  const blade = (k, c) => [[0, '#ffffff'], [0.45, k.lt(c, 0.55)], [1, k.dk(c, 0.35)]];
  // the Digimental: an egg with a crest horn
  function digimental(p, k, col, glow) {
    if (glow) p.glow(0, 0, 48, col, 0.55);
    p.part('M-10 -36L0 -58L10 -36Z', [[0, '#ffffff'], [0.5, col], [1, k.dk(col, 0.45)]], { ow: 5 });
    p.part(k.EGG, [[0, k.lt(col, 0.7)], [0.45, col], [1, k.dk(col, 0.5)]]);
    p.part(k.star(0, 6, 17, 7, 5), [[0, '#ffffff'], [1, k.lt(col, 0.3)]], { ow: 4 });
    p.gloss(-14, -18, 9, 15, 20, 0.85);
    for (const [x, y] of [[-24, 22], [24, 22]]) p.part(k.circ(x, y, 4), '#ff6a3d', { ow: 3, hi: false });
  }

  comp('gal_sword', (p, k) => p.at(64, 64, 1, 0, () => { k.M.chip(p, COL.gal_sword, 62); p.glow(0, 0, 22, '#ffcf8a', 0.7); p.at(0, 0, 1.05, 45, () => k.M.miniBlade(p)); }));
  comp('gal_bow', (p, k) => p.at(64, 66, 1, 0, () => {
    p.at(-16, 0, 0.82, 0, () => k.M.wing(p, '#fff6c8')); p.at(16, 0, 0.82, 0, () => k.M.wing(p, '#fff6c8'), -1);
    k.M.chip(p, COL.gal_bow, 44);
    p.glow(0, 0, 16, '#fff3a0', 0.8);
    p.at(0, 0, 0.62, 0, () => p.part(k.BOLT, [[0, '#ffffff'], [1, '#ffd23f']], { ow: 5 }));
  }));
  comp('gal_rod', (p, k) => p.at(66, 64, 1, 38, () => {
    p.part(k.rr(-5, -12, 10, 56, 4), 'dark');
    for (const y of [2, 22]) p.part(k.rr(-7.5, y, 15, 6, 2), COL.gal_rod, { ow: 4, hi: false });
    p.gem(0, 46, 6.5, COL.gal_rod, 'round', 4.5);
    p.glow(0, -30, 26, COL.gal_rod, 0.75);
    p.at(0, -30, 1, 0, () => k.M.crystal(p, COL.gal_rod, 38, 11));
  }));
  comp('gal_tear', (p, k) => {
    p.at(56, 62, 1, -12, () => k.M.card(p, k.dk(COL.gal_tear, 0.35), 48, 68));
    p.at(68, 66, 1, 8, () => { k.M.card(p, COL.gal_tear, 50, 72); p.glow(0, 4, 24, '#bfe6ff', 0.8); p.at(0, 4, 1, 0, () => p.part(k.DROP, [[0, '#ffffff'], [0.6, '#9fdcff'], [1, '#2a7fd0']], { ow: 4.5 })); });
  });
  comp('gal_vest', (p, k) => p.at(64, 64, 1, 0, () => k.M.plate(p)));
  comp('gal_cloak', (p, k) => p.at(64, 64, 1, 0, () => { k.M.hood(p, COL.gal_cloak); p.gem(0, -6, 4.5, '#bdfff0', 'diamond', 3.5); }));
  comp('gal_belt', (p, k) => p.at(64, 64, 1, 0, () => { k.M.disk(p, COL.gal_belt, 41); p.at(0, 1, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); }));
  comp('gal_glove', (p, k) => p.at(64, 66, 1, 0, () => { k.M.glove(p, COL.gal_glove, '#f2f5e6'); p.at(1, 6, 1.05, 0, () => k.M.clover(p)); }));
  comp('gal_mental', (p, k) => p.at(64, 70, 0.95, 0, () => digimental(p, k, COL.gal_mental, false)));

  // completed items
  done('gal_reaper', (p, k, a) => p.at(66, 66, 1, 0, () => {
    p.tube('M-26 44L18 -40', 7, '#4a3a5a');
    p.part('M14 -44C-8 -54 -36 -42 -46 -20C-30 -34 -10 -36 8 -28Z', blade(k, a), { hiA: 0.8 });
    p.at(-6, 18, 0.55, 0, () => k.SYM.skull(p, '#e8e2f2'));
  }));
  done('gal_slayer', (p, k, a, b) => p.at(64, 64, 1.05, 45, () => {
    k.M.sword(p, { w: 28, tip: 20, blade: blade(k, a), guard: b, gw: 40, pommel: b });
    p.at(0, -16, 0.7, 0, () => p.part(k.BOLT, [[0, '#fff'], [1, b]], { ow: 4 }));
  }));
  done('gal_gunblade', (p, k, a, b) => p.at(62, 66, 1, 40, () => {
    k.M.sword(p, { w: 14, top: -50, blade: blade(k, a), guard: 'dark', gw: 24 });
    p.part(k.rr(-13, 10, 26, 16, 5), 'dark');
    p.glow(0, 18, 14, b, 0.9);
    p.gem(0, 18, 6, b, 'hex', 3);
  }));
  done('gal_spear', (p, k, a, b) => p.at(64, 64, 1.05, 42, () => {
    p.tube('M0 50V-14', 8, a);
    p.part(k.rr(-9, -20, 18, 8, 3), 'chrome', { ow: 4 });
    p.glow(0, -34, 22, '#9fdcff', 0.8);
    p.part(k.poly([[0, -56], [12, -34], [6, -20], [-6, -20], [-12, -34]]), [[0, '#ffffff'], [0.45, b], [1, k.dk(b, 0.5)]], { hiA: 0.8 });
  }));
  done('gal_rebirth', (p, k) => p.at(64, 68, 1, 0, () => {
    p.at(-14, -6, 0.75, 0, () => k.M.wing(p, '#fff6e0')); p.at(14, -6, 0.75, 0, () => k.M.wing(p, '#fff6e0'), -1);
    p.glow(0, 0, 36, '#fff3b0', 0.6);
    k.M.egg(p, '#fff8e8', [[-12, -8, 7, '#ffcf4a'], [12, 8, 8, '#ffcf4a']], 0.72);
    p.line('M-16 -6L-6 2L-12 10L0 16L8 6L16 12', '#6a4a20', 2.6, 0.9);
  }));
  done('gal_bloodfang', (p, k, a, b) => p.at(60, 66, 1.05, 18, () => {
    p.part('M-9 18C-12 -8 0 -32 24 -50C14 -26 10 -4 9 18Z', blade(k, a), { hiA: 0.85 });
    p.part(k.rr(-16, 16, 32, 9, 4), k.dk(a, 0.25), { ow: 5 });
    p.part(k.rr(-7, 24, 14, 22, 4), b, { ow: 5 });
    p.part(k.ell(0, 50, 4, 5), '#ff2e43', { ow: 3, hi: false });
  }));
  done('gal_banner', (p, k, a, b) => p.at(60, 64, 1, 0, () => {
    p.tube('M-22 50V-46', 6, '#c9a35a');
    p.part('M-20 -44H34L24 -26L34 -8H-20Z', [[0, k.lt(a, 0.4)], [1, k.dk(a, 0.3)]]);
    p.at(6, -26, 0.6, 0, () => k.SYM.chevrons(p, '#fff6c8'));
    p.gem(-22, -50, 5, b, 'round', 3.5);
  }));
  done('gal_critedge', (p, k, a, b) => p.at(64, 64, 1.05, 45, () => {
    k.M.sword(p, { w: 11, tip: 12, top: -52, blade: blade(k, a), core: '#ffb070', guard: b, gw: 26, grip: '#2f3b22' });
    p.at(0, 19, 0.5, 0, () => k.M.clover(p, '#f2ffd0'));
    p.at(0, -52, 1, -45, () => { p.glow(0, 0, 18, '#fff0a0', 0.9); p.part(k.star(0, 0, 15, 4, 4), '#fffbe0', { ow: 2, hi: false }); });
  }));
  done('gal_longshot', (p, k, a) => p.at(64, 64, 1, 0, () => {
    p.tube('M-30 40Q-44 0 -30 -40', 6, '#c9a35a');
    p.line('M-30 -40L-30 40', '#f6f1e0', 1.8, 0.95);
    p.tube('M-30 0H38', 3.5, '#e8e2cc', { ow: 4 });
    p.part(k.poly([[48, 0], [34, -7], [34, 7]]), a, { ow: 3.5 });
    p.at(14, -22, 0.7, 0, () => k.SYM.crosshair(p, '#fff3a0'));
  }));
  done('gal_ragewing', (p, k, a, b) => p.at(64, 64, 1, 0, () => {
    p.glow(0, 0, 40, a, 0.35);
    for (const [r, c] of [[-10, '#ffe680'], [16, a], [42, '#ffb02e'], [68, '#ff8a2a']]) p.at(-28, 32, 1, r, () => p.part('M0 0C9 -10 11 -36 3 -60C-6 -36 -9 -12 0 0Z', [[0, '#fffbe0'], [0.5, c], [1, k.dk(c, 0.45)]], { ow: 5 }));
    p.at(10, 6, 0.6, 15, () => p.part(k.BOLT, [[0, '#fff'], [1, b]], { ow: 5 }));
  }));
  done('gal_voltdisk', (p, k, a, b) => p.at(64, 64, 1, 0, () => { k.M.disk(p, b, 40); p.glow(0, 0, 30, '#fff3a0', 0.6); p.at(2, 0, 1.45, 10, () => p.part(k.BOLT, [[0, '#ffffff'], [0.5, a], [1, '#e09a00']], { ow: 4.5 })); }));
  done('gal_titanwill', (p, k, a) => p.at(64, 64, 1, 0, () => { k.M.plate(p); p.glow(0, 4, 24, a, 0.5); p.at(0, 6, 0.8, 0, () => k.SYM.chevrons(p, '#fff6c8')); }));
  done('gal_twinshot', (p, k, a, b) => p.at(64, 66, 1, 35, () => {
    p.tube('M-42 -2Q0 -34 42 -2', 8, a);
    for (const x of [-7, 7]) { p.tube(`M${x} 18V-40`, 3.5, '#e8e2cc', { ow: 4 }); p.part(k.poly([[x, -54], [x + 5, -40], [x - 5, -40]]), b, { ow: 3.5, hi: false }); }
  }));
  done('gal_portal', (p, k, a, b) => p.at(64, 64, 1, 0, () => {
    p.glow(0, 0, 46, '#b07aff', 0.5);
    for (const [r, c] of [[40, b], [30, '#b07aff'], [20, a]]) p.tube(k.circ(0, 0, r), 6, c, { ow: 4 });
    p.part(k.circ(0, 0, 11), [[0, '#ffffff'], [1, '#6a3ad0']], { ow: 4 });
  }));
  done('gal_breaker', (p, k, a, b) => {
    p.at(96, 30, 1, 0, () => { for (const [x, y, r] of [[0, 0, 20], [-14, 12, -30], [10, 16, 60]]) p.at(x, y, 1, r, () => p.part(k.poly([[-6, -5], [6, -6], [3, 6]]), 'chrome', { ow: 3.5, hi: false })); });
    p.at(60, 68, 1.02, 45, () => {
      p.part(k.rr(-7, 26, 14, 22, 4), k.dk(b, 0.4), { ow: 5 });
      p.part(k.rr(-17, 0, 34, 28, 7), b);
      p.part(k.poly([[0, -54], [19, -8], [-19, -8]]), [[0, '#fff8d0'], [0.5, a], [1, '#9a6800']]);
    });
  });
  done('gal_ultima', (p, k, a) => p.at(64, 66, 1, 28, () => {
    p.tube('M0 50V-4', 7, '#5b3796');
    p.glow(0, -26, 34, a, 0.85);
    p.at(0, -28, 1, 0, () => k.M.crystal(p, '#c48aff', 36, 11));
    p.at(-22, -40, 0.35, 0, () => k.M.crystal(p, a)); p.at(22, -40, 0.35, 0, () => k.M.crystal(p, a));
    p.sparkle(-16, -54, 6); p.sparkle(20, -16, 4);
  }));
  done('gal_echo', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    for (const r of [44, 34]) p.add(`<path d="M${-r} -10A${r} ${r} 0 0 1 ${r} -10" fill="none" stroke="${b}" stroke-opacity=".7" stroke-width="4"/>`);
    p.tube('M0 50V-6', 6.5, '#e6f2ff');
    p.glow(0, -20, 22, a, 0.8);
    p.at(0, -20, 0.85, 0, () => k.M.crystal(p, a, 34, 10));
  }));
  done('gal_locket', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    p.tube('M-22 -40C-22 -60 22 -60 22 -40', 4, '#e8c860', { ow: 4 });
    p.part('M0 -40L30 -30C32 -4 22 20 0 40C-22 20 -32 -4 -30 -30Z', 'gold');
    p.glow(0, -2, 22, a, 0.7);
    p.gem(0, -2, 10, a, 'round', 4.5);
  }));
  done('gal_manazap', (p, k, a, b) => p.at(64, 70, 1, 0, () => {
    p.part(k.rr(-22, 26, 44, 14, 4), 'dark');
    p.part(k.poly([[-12, 27], [12, 27], [7, -12], [-7, -12]]), b);
    p.glow(0, -24, 30, a, 0.9);
    for (const d of ['M-8 -28L-20 -36L-26 -28L-40 -40', 'M8 -28L22 -40L26 -30L42 -42', 'M0 -36L-4 -48L4 -54L0 -62']) { p.line(d, '#15161d', 7, 0.8); p.line(d, '#f2e2ff', 3, 1); }
    p.gem(0, -24, 11, a, 'round', 5);
  }));
  done('gal_tome', (p, k, a, b) => p.at(62, 70, 1, 0, () => {
    p.at(4, -34, 1.1, 0, () => p.part(k.FLAME, [[0, '#fff2a8'], [0.5, '#ff5d4a'], [1, '#c41632']], { ow: 4.5 }));
    p.part(k.rr(-32, -30, 56, 62, 6), a);
    p.part(k.rr(-36, -30, 10, 62, 4), k.dk(a, 0.35), { ow: 5 });
    p.part(k.circ(-2, 1, 16), 'gold', { ow: 4 });
    p.at(-2, 2, 0.62, 0, () => p.part(k.FLAME, [[0, '#fff2a8'], [0.5, b], [1, '#9a1028']], { ow: 5 }));
  }));
  done('gal_jewel', (p, k, a, b) => p.at(64, 64, 1, 0, () => {
    k.M.fist(p, b, k.dk(b, 0.35));
    for (let i = 0; i < 4; i++) p.gem(-21 + i * 14, -22, 4.2, a, 'diamond', 3.5);
    p.gem(0, 35, 6.5, a, 'hex', 4);
    p.sparkle(30, -36, 7);
  }));
  done('gal_overflow', (p, k, a) => p.at(64, 66, 1, 0, () => {
    const body = 'M-8 -40H8V-16C22 -12 32 0 32 14C32 32 18 44 0 44C-18 44 -32 32 -32 14C-32 0 -22 -12 -8 -16Z';
    p.part(body, 'glass', { hiA: 0.9 });
    p.clip(body, () => p.part('M-40 4C-26 -2 -14 8 0 2C14 -4 26 6 40 0V50H-40Z', [[0, '#8fd6ff'], [0.5, a], [1, '#1c4f9a']], { ow: 3, hi: false }));
    p.part(k.rr(-11, -46, 22, 8, 3), a, { ow: 4 });
  }));
  done('gal_frostheart', (p, k, a, b) => p.at(64, 64, 1, 0, () => {
    p.glow(0, 0, 44, '#bfe6ff', 0.55);
    k.M.heart(p, [[0, '#ffffff'], [0.5, '#9fdcff'], [1, '#2a6fc0']], 0.95);
    p.at(0, -2, 0.9, 0, () => k.SYM.snow(p, '#ffffff'));
  }));
  done('gal_chalice', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    p.part('M-30 -40H30C30 -10 18 6 0 8C-18 6 -30 -10 -30 -40Z', 'gold');
    p.part(k.ell(0, -40, 30, 7), [[0, k.lt(a, 0.5)], [1, k.dk(a, 0.4)]], { ow: 4 });
    p.part(k.rr(-5, 6, 10, 26, 3), 'gold', { ow: 4 });
    p.part(k.ell(0, 36, 22, 7), 'gold', { ow: 4 });
    p.gem(0, -14, 7, b, 'diamond', 3.5);
  }));
  done('gal_holyring', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    p.glow(0, -10, 40, '#fff6c0', 0.6);
    p.add('<ellipse cx="0" cy="-34" rx="30" ry="9" fill="none" stroke="#15161d" stroke-width="10"/><ellipse cx="0" cy="-34" rx="30" ry="9" fill="none" stroke="#fff0a0" stroke-width="5"/>');
    p.at(0, 12, 1.5, 0, () => k.SYM.cross(p, b));
  }));
  done('gal_justice', (p, k, a, b) => p.at(64, 66, 1, 0, () => { k.M.glove(p, b, '#f2f5e6'); p.gem(0, 4, 9, a, 'diamond', 4); p.sparkle(26, -34, 6); }));
  done('gal_spikeplate', (p, k) => p.at(64, 64, 1, 0, () => {
    for (const [x, y, r] of [[-38, -10, -90], [38, -10, 90], [-30, 26, -125], [30, 26, 125], [0, 46, 180]]) p.at(x, y, 1, r, () => p.part(k.poly([[0, -14], [6, 4], [-6, 4]]), 'steel', { ow: 4, hi: false }));
    k.M.plate(p);
  }));
  done('gal_swordbreaker', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    p.part('M0 -44L34 -34C36 -4 26 24 0 44C-26 24 -36 -4 -34 -34Z', [[0, k.lt(b, 0.5)], [1, k.dk(b, 0.4)]]);
    p.at(0, -4, 0.6, 30, () => k.M.sword(p, { w: 12, blade: 'steel', guard: 'dark', gw: 22 }));
    p.line('M-14 -14L14 10', '#15161d', 5, 1);
  }));
  done('gal_core', (p, k, a, b) => p.at(64, 64, 1, 0, () => { k.M.plate(p); p.glow(0, 4, 22, '#ff6a2a', 0.85); p.at(0, 6, 0.9, 0, () => p.part(k.FLAME, [[0, '#fff2a8'], [0.5, '#ff7a3a'], [1, b]], { ow: 4.5 })); }));
  done('gal_shroud', (p, k) => p.at(64, 64, 1, 0, () => { k.M.hood(p, [[0, '#7a7f96'], [1, '#232634']], '#9fdcff'); p.add('<path d="M-46 30H46" stroke="#9fdcff" stroke-opacity=".6" stroke-width="3"/>'); }));
  done('gal_dragonscale', (p, k, a) => p.at(64, 64, 1, 0, () => {
    p.part('M0 -44L36 -26V14C36 30 18 42 0 46C-18 42 -36 30 -36 14V-26Z', [[0, k.lt(a, 0.5)], [1, k.dk(a, 0.5)]]);
    for (const [x, y] of [[-14, -18], [14, -18], [0, -2], [-14, 14], [14, 14], [0, 30]]) p.part(k.ngon(x, y, 10, 6), [[0, k.lt(a, 0.7)], [1, a]], { ow: 3, hiW: 2 });
  }));
  done('gal_zephyr', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    for (const [y, w] of [[-34, 40], [-18, 32], [-2, 24], [14, 16], [28, 9]]) p.tube(`M${-w} ${y}Q0 ${y + 8} ${w} ${y}`, 5, k.lt(a, 0.4), { ow: 4 });
    p.sparkle(26, -40, 6, b);
  }));
  done('gal_quicksilver', (p, k) => p.at(64, 66, 1, 0, () => {
    p.at(-12, -4, 0.6, 0, () => k.M.wing(p, '#e8f2ff')); p.at(12, -4, 0.6, 0, () => k.M.wing(p, '#e8f2ff'), -1);
    p.at(0, 8, 1.6, 0, () => p.part(k.DROP, 'chrome', { ow: 4 }));
  }));
  done('gal_gigabelt', (p, k, a) => p.at(64, 64, 1, 0, () => { k.M.disk(p, a, 46); p.at(0, 1, 0.42, 0, () => p.part(k.HEART, '#ff3b55', { ow: 9 })); }));
  done('gal_trapclaw', (p, k, a, b) => p.at(64, 66, 1, 0, () => {
    for (const s of [-1, 1]) p.at(0, 0, 1, 0, () => p.part(`M${-38 * s} ${s > 0 ? -6 : 6}A38 38 0 0 ${s > 0 ? 1 : 0} ${38 * s} ${s > 0 ? -6 : 6}`, 'steel', { ow: 5 }));
    for (let i = -3; i <= 3; i++) p.part(k.poly([[i * 10 - 4, -2], [i * 10 + 4, -2], [i * 10, -14]]), '#e6ecf3', { ow: 3, hi: false });
    p.gem(0, 22, 7, b, 'round', 4);
  }));
  done('gal_thief', (p, k, a) => p.at(64, 66, 1, 0, () => { k.M.glove(p, a, '#3a2a2a'); k.M.coin(p, -26, 30, 10, -20); k.M.coin(p, 28, 28, 12, 15); }));

  // Golden Digimental, consumables
  items.gal_golden = (p, k) => { k.frame(p, 'square', 'gold', ['#ffcf4a'], { w: 8 }); stars(p, k, '#ffffff'); p.obj(() => p.at(64, 70, 0.95, 0, () => digimental(p, k, '#ffd84a', true))); };
  items.gal_copier = (p, k) => { k.frame(p, 'circle', 'silver', ['#4fb3ff'], { w: 6 }); p.obj(() => p.at(64, 64, 2.4, 0, () => k.SYM.copy(p, '#4fb3ff'))); };
  items.gal_remover = (p, k) => {
    k.frame(p, 'circle', 'silver', ['#ff8fa3'], { w: 6 });
    p.obj(() => p.at(64, 66, 1, 0, () => {
      p.tube('M-24 30V-6A24 24 0 0 1 24 -6V30', 14, '#ff4d64', { cap: 'butt' });
      for (const x of [-24, 24]) p.part(k.rr(x - 9, 26, 18, 12, 2), 'chrome', { ow: 4 });
      p.line('M-36 -36L-26 -26M36 -36L26 -26M0 -48V-36', '#9fdcff', 3, 0.8);
    }));
  };
  items.gal_reforger = (p, k) => {
    k.frame(p, 'circle', 'silver', ['#ffb347'], { w: 6 });
    p.obj(() => p.at(64, 66, 1, 0, () => {
      p.part('M-36 0H36L28 14H-28Z', 'dark');
      p.part(k.rr(-12, 14, 24, 16, 3), 'dark', { ow: 4 });
      p.part(k.rr(-26, 28, 52, 10, 3), 'dark', { ow: 4 });
      p.at(0, -22, 1.1, 0, () => k.SYM.refresh(p, '#ffb347'));
    }));
  };

  // Crests: shield frame in the trait color with the trait symbol
  const CREST_TRAITS = ['BM', 'IN', 'OV', 'DD', 'RE', 'CE', 'PR', 'DA', 'CY', 'TK', 'PI', 'AS', 'SO', 'GU', 'BR', 'VG', 'MY', 'SN'];
  for (const t of CREST_TRAITS) {
    items[`emblem_${t.toLowerCase()}`] = (p, k) => {
      const T = DT.TRAITS && DT.TRAITS[t];
      k.crest(p, T ? T.color : '#ffcf4a', (T && T.crestSymbol) || 'star6');
    };
  }

  // Augments: round frame in the tier metal with a symbol
  const AUG = {
    cash: ['coin', '#ffd23f'], cache: ['chip', '#ff9a45'], rolls: ['refresh', '#9fdcff'], training: ['sword', '#ff7a45'], front: ['shieldIron', '#d9dfe9'],
    back: ['crosshair', '#5ad1ff'], starlight: ['star5', '#fff0a0'], bank: ['coin', '#ffd23f'], trove: ['crestTag', '#ffcf4a'], mental: ['egg', '#ffcf4a'],
    income: ['up', '#7dff9a'], blessing: ['cross', '#ffe680'], lotus: ['star6', '#e58bff'], implants: ['chip', '#9fb4c8'], jackpot: ['coin', '#ffd23f'],
    armory: ['badge', '#ffcf4a'], golden: ['egg', '#ffd84a'], giant: ['plus', '#ff5d73'], stars: ['star5', '#ff9ad5'], megaEvolve: ['chevrons', '#c9a3ff'], overcharge: ['drop', '#4fb3ff'],
  };
  const TIER_METAL = { 1: 'silver', 2: 'gold', 3: 'prism' };
  const TIER_BG = { 1: '#6a7a90', 2: '#a07a20', 3: '#8a4ab8' };
  const augDefs = () => ((DT.Sets.def('galaxies') || {}).augments || []);
  for (const [key, [sym, col]] of Object.entries(AUG)) {
    items[`aug:gal_a_${key}`] = (p, k) => {
      const a = augDefs().find((x) => x.id === `gal_a_${key}`) || { tier: 1 };
      k.frame(p, 'hex', TIER_METAL[a.tier], [TIER_BG[a.tier]], { w: 7 });
      stars(p, k);
      p.obj(() => p.at(64, 64, 2.1, 0, () => { p.glow(0, 0, 22, col, 0.5); (k.SYM[sym] || k.SYM.star6)(p, col); }));
    };
  }

  // Galaxy cards: a planet in the Galaxy color with its symbol (shown at game start and in the codex)
  const GAL_SYM = { normal: 'sun', treasure: 'crestTag', armory: 'chip', cluster: 'star6', mirror: 'copy', bigger: 'chevrons', trade: 'refresh', superdense: 'egg',
    salvage: 'chess', plunder: 'coin', shrine: 'egg', bigbang: 'star5', darkocean: 'pearl', datastream: 'drop', server: 'plus' };
  for (const [id, sym] of Object.entries(GAL_SYM)) {
    items[`galaxy:${id}`] = (p, k) => {
      const g = ((DT.Sets.def('galaxies') || {}).galaxies || []).find((x) => x.id === id) || { color: '#7a8cff' };
      const c = g.color;
      k.frame(p, 'circle', 'prism', ['#1a1440'], { w: 6 });
      for (const [x, y, r] of [[28, 36, 2.5], [96, 30, 3], [100, 92, 2], [34, 96, 3], [64, 18, 2]]) p.sparkle(x, y, r, '#ffffff');
      p.obj(() => p.at(64, 66, 1, 0, () => {
        p.glow(0, 0, 46, c, 0.45);
        p.add(`<ellipse cx="0" cy="4" rx="46" ry="11" transform="rotate(-14)" fill="none" stroke="#15161d" stroke-width="9"/>`);
        p.part(k.circ(0, 0, 30), [[0, k.lt(c, 0.6)], [0.5, c], [1, k.dk(c, 0.6)]]);
        p.add(`<path d="M-45 -6A46 11 -14 0 0 45 -16" transform="rotate(0)" fill="none" stroke="${k.lt(c, 0.5)}" stroke-width="4"/>`);
        p.at(0, 0, 0.95, 0, () => (k.SYM[sym] || k.SYM.star6)(p, '#ffffff'));
      }));
    };
  }

  // ---------- move looks ----------
  // m(elem, charge, impact, fx, extra): extra holds release, shake, colors
  const m = (elem, charge, impact, fx, extra = {}) => ({ elem, charge, impact, release: 0.25, fx, ...extra });
  const moves = {
    // Hawkmon line (snipe)
    'Feather Slash': m('wind', 'hands', 'wind', [['swarm', { cell: 'feather', count: 8, size: 0.32, speed: 13, wobble: 0.4 }]], { colors: ['#ff8a5a', '#fff2d0'] }),
    'Blast Laser': m('wind', 'mouth', 'blast', [['beam', { width: 0.16, dur: 0.3, charge: 0.1 }]], { colors: ['#ffdf6a', '#ffffff'], shake: 0.06 }),
    'Top Gun': m('wind', 'hands', 'blast', [['beam', { width: 0.12, dur: 0.25, charge: 0.25, core: '#ffffff' }], ['rings', { at: 0.05, count: 3, size: 0.5 }]], { colors: ['#9fe8ff', '#ffffff'], shake: 0.12, release: 0.4 }),
    // Kotemon line (riposte)
    'Meteor Slash': m('fire', 'hands', 'hit', [['dome', { size: 0.9 }], ['slash', { at: 0.9, count: 1, size: 1.2 }]], { colors: ['#ffcf4a', '#ffffff'] }),
    'Crash Charge': m('metal', 'none', 'hit', [['dome', { size: 1 }], ['streak', { at: 0.9, width: 0.5, dur: 0.2, trail: 'spark' }]], { shake: 0.08 }),
    'Grade Slash': m('holy', 'hands', 'holy', [['dome', { size: 1.1, orbit: 'blade', count: 2 }], ['slash', { at: 0.9, count: 2, size: 1.5, spread: 0.4 }]], { colors: ['#ffffff', '#ffe680'], shake: 0.12 }),
    // Gazimon line (smoke)
    'Electric Stun Blast': m('thunder', 'mouth', 'zap', [['orb', { size: 0.24, speed: 11, arc: 0.3, trail: 'zap' }], ['erupt', { at: 0.3, kind: 'dark', to: 'center', count: 3, height: 0.8 }]]),
    'Chaos Fire': m('dark', 'mouth', 'dark', [['breath', { dur: 0.4, width: 0.4, cell: 'flame', size: 0.36 }], ['vortex', { at: 0.3, size: 1.1, dur: 0.8, cell: 'smoke' }]], { colors: ['#8a4aff', '#ff6a3a'] }),
    'Hellfire': m('dark', 'hands', 'blast', [['volley', { count: 14, rate: 0.03, cell: 'ember', size: 0.4, spread: 0.3, from: 'hands', speed: 22 }], ['vortex', { at: 0.4, size: 1.3, dur: 0.9, cell: 'smoke' }]], { colors: ['#ff4a2a', '#ffd06a'], shake: 0.1 }),
    // Syakomon line (sap)
    'Black Pearl Blast': m('water', 'mouth', 'splash', [['orb', { size: 0.3, speed: 10, arc: 0.2, trail: 'bubble', head: 'bubble' }]], { colors: ['#3a2a6a', '#e8d8ff'] }),
    'Thousand Whips': m('dark', 'ground', 'dark', [['whip', { dur: 0.5, width: 0.2, count: 4, sway: 1.4, leafy: false }], ['spikes', { at: 0.2, count: 5, kind: 'dark', size: 0.6 }]], { colors: ['#4a3a8a', '#9fe8ff'], shake: 0.12 }),
    // BlackGabumon line (rally)
    'Little Horn': m('ice', 'aura', 'frost', [['dome', { size: 0.9 }], ['heal', { cell: 'snow', size: 0.5 }]], { colors: ['#4a5a8a', '#bfe8ff'] }),
    'Freeze Fang': m('ice', 'aura', 'frost', [['dome', { size: 1, orbit: 'snow', count: 4 }], ['heal', { cell: 'snow', size: 0.6 }]], { colors: ['#3a4a7a', '#dff4ff'] }),
    // Keramon line (isolate)
    "Hell's Grenade": m('dark', 'mouth', 'blast', [['orb', { size: 0.4, speed: 9, arc: 0.4, trail: 'dark', impactSize: 1.5 }]], { colors: ['#ff3a6a', '#ffd0e0'], shake: 0.15 }),
    // Armadillomon line
    'Megaton Press': m('metal', 'sky', 'metal', [['dome', { size: 1.2, orbit: 'hex', count: 6 }], ['shock', { at: 0.15, debris: true, cell: 'spark' }]], { colors: ['#c9a35a', '#fff2c0'], shake: 0.12 }),
    // Tapirmon line (stun)
    'Nightmare Syndrome': m('dark', 'mouth', 'dark', [['swarm', { cell: 'wisp', count: 8, size: 0.4, speed: 6, wobble: 1.2, tint: true }]], { colors: ['#8a5aff', '#ffb0e0'] }),
    "Hell's Hand": m('dark', 'hands', 'dark', [['erupt', { kind: 'dark', count: 1, height: 1.6 }], ['whip', { at: 0.1, dur: 0.3, width: 0.18, leafy: false }]], { colors: ['#6a3aa0', '#d8c0ff'] }),
    'Soul Chopper': m('dark', 'hands', 'dark', [['dash', { ghosts: 5, slashes: 1, dur: 0.2, size: 1.4 }], ['slash', { at: 0.2, count: 2, size: 1.6, vertical: true }]], { colors: ['#3a2a5a', '#c0a0ff'], shake: 0.12 }),
    // Solarmon line (line)
    'Shiny Ring': m('holy', 'hands', 'holy', [['rings', { count: 3, size: 0.4, gap: 0.08, dur: 0.4 }]], { colors: ['#ffd84a', '#ffffff'] }),
    'Chrono Breaker': m('metal', 'hands', 'metal', [['wheel', { size: 0.8, cell: 'gear', dur: 0.5, saw: true }], ['glyph', { at: 0.05, glyph: 'circle', size: 1.1, dur: 0.5 }]], { colors: ['#ffb04a', '#fff2c0'] }),
    'Pandora Dialogue': m('dark', 'hands', 'dark', [['glyph', { glyph: 'tri', size: 1.4, dur: 0.6 }], ['swarm', { at: 0.1, cell: 'card', count: 10, size: 0.36, speed: 13, fan: true, spread: 0.5 }]], { colors: ['#8a4aff', '#fff0a0'], shake: 0.08 }),
    // Hyokomon line (plumage)
    'Downward Cleave': m('wind', 'aura', 'hit', [['tornado', { cell: 'feather', height: 1.2, dur: 0.6, size: 0.6 }]], { colors: ['#ffd06a', '#ffffff'] }),
    'Fire Phaser': m('fire', 'aura', 'blast', [['tornado', { cell: 'flame', height: 1.4, dur: 0.7, size: 0.7 }], ['heal', { cell: 'feather', size: 0.5 }]], { colors: ['#ff6a2a', '#fff0a0'] }),
    'Celestial Arrow': m('holy', 'aura', 'holy', [['pillar', { size: 0.8, height: 4, dur: 0.6, to: 'self' }], ['tornado', { at: 0.1, cell: 'feather', height: 1.6, dur: 0.8, size: 0.7 }]], { colors: ['#fff0a0', '#ff9ad5'] }),
    // Chuumon line (bomb)
    'Cheese Bomb': m('fire', 'hands', 'blast', [['orb', { size: 0.26, speed: 9, arc: 0.8, trail: 'smoke', head: 'dot' }]], { colors: ['#ffd84a', '#fff6c0'], shake: 0.06 }),
    // Renamon line (orb)
    'Fox Tail Inferno': m('fire', 'back', 'blast', [['orb', { size: 0.28, speed: 9, arc: 0.5, count: 3, gap: 0.08, trail: 'flame', side: 0.4, wobble: 0.4 }]], { colors: ['#4fb3ff', '#e0f6ff'] }),
    // Shoutmon line (fireshield)
    'Bellow Blaster': m('fire', 'mouth', 'blast', [['breath', { dur: 0.4, width: 0.4, cell: 'note', mix: 'flame', size: 0.35 }], ['dome', { at: 0.35, size: 0.9 }]], { colors: ['#ff5a2a', '#ffe08a'] }),
    'Flame Cannon': m('fire', 'mouth', 'blast', [['sphere', { size: 0.5, grow: 0.3, from: 'mouth', trail: 'flame', cell: 'flame' }], ['dome', { at: 0.5, size: 1 }]], { colors: ['#ff4a1a', '#ffd06a'], shake: 0.1 }),
    'Hard Rock Soul': m('fire', 'hands', 'blast', [['swarm', { cell: 'note', count: 12, size: 0.4, speed: 9, fan: true, spread: 0.7 }], ['breath', { at: 0.1, dur: 0.5, width: 0.6, cell: 'flame', size: 0.45 }], ['dome', { at: 0.5, size: 1.2 }]], { colors: ['#ff3a1a', '#fff0a0'], shake: 0.15 }),
    // Kokuwamon line (grab)
    'Electro Strike': m('thunder', 'hands', 'zap', [['whip', { dur: 0.35, width: 0.12, lightning: true }]], { colors: ['#ffe94a', '#bfe8ff'] }),
    'Destruction Grenade': m('metal', 'hands', 'blast', [['whip', { dur: 0.4, width: 0.16, leafy: false }], ['missiles', { at: 0.2, count: 2, size: 0.14, from: 'chest', curve: 0.8, dur: 0.4 }]], { colors: ['#9aa3b2', '#ffd06a'], shake: 0.1 }),
    'Spiral Sword': m('metal', 'hands', 'hit', [['whip', { dur: 0.4, width: 0.18, leafy: false }], ['slash', { at: 0.25, count: 3, size: 1.3, spread: 0.6 }]], { colors: ['#c0c8d8', '#ffffff'], shake: 0.12 }),
    // Gizamon line (dunk)
    'Spiral Edge': m('water', 'none', 'hit', [['wheel', { size: 0.6, saw: true, dur: 0.3 }]], { colors: ['#4a9a8a', '#d0fff0'] }),
    // Falcomon line (barrage)
    'Scratch Smash': m('beast', 'hands', 'hit', [['swarm', { cell: 'blade', count: 6, size: 0.3, speed: 15, spin: 12 }]], { colors: ['#c8a070', '#ffffff'] }),
    'Haguro': m('dark', 'back', 'dark', [['swarm', { cell: 'feather', count: 16, size: 0.4, speed: 16, fan: true, spread: 0.6, color2: '#ff3a3a' }]], { colors: ['#2a2040', '#ff5a5a'], shake: 0.1 }),
    // Betamon line (frenzy)
    'Lightning Javelin': m('thunder', 'mouth', 'zap', [['bolt', { from: 'caster', width: 0.1, count: 2 }], ['dome', { at: 0.1, size: 1, orbit: 'bolt', count: 3 }]], { colors: ['#ffe94a', '#a8dcff'], shake: 0.1 }),
    // Tsukaimon line (dreadshield)
    'Bad Message': m('dark', 'aura', 'dark', [['dome', { size: 0.9, orbit: 'bat', count: 3 }]], { colors: ['#6a3aa0', '#ff8ad0'] }),
    'Skull Hammer': m('dark', 'aura', 'dark', [['dome', { size: 1.2, orbit: 'skull', count: 4 }], ['shock', { at: 0.2, hammer: true, cell: 'skull' }]], { colors: ['#3a2a4a', '#d0c0ff'], shake: 0.14 }),
    // Gotsumon line (crater)
    'Golem Punch': m('beast', 'ground', 'hit', [['erupt', { kind: 'rock', count: 3, height: 1.2 }], ['shock', { at: 0.1, debris: true }]], { colors: ['#a08a6a', '#ffe0a0'], shake: 0.12 }),
    'Galactic Flare': m('holy', 'sky', 'blast', [['meteor', { size: 0.6, count: 2, height: 6, dur: 0.6, trail: 'spark' }], ['erupt', { at: 0.6, kind: 'rock', count: 4, height: 1.4, area: true }]], { colors: ['#9a7aff', '#fff0a0'], shake: 0.2 }),
    // Kudamon line (triple)
    'Bullet Whirlwind': m('wind', 'none', 'wind', [['streak', { width: 0.4, dur: 0.2, trail: 'wind' }], ['slash', { at: 0.15, count: 3, size: 1, gap: 0.08 }]], { colors: ['#ffe8a0', '#ffffff'] }),
    'Lion Claw': m('beast', 'none', 'hit', [['dash', { ghosts: 4, slashes: 2, claw: true, dur: 0.2 }], ['slash', { at: 0.3, count: 3, claw: true, size: 1.2, gap: 0.08 }]], { colors: ['#ffd06a', '#ffffff'], shake: 0.08 }),
    'Wind Cutter Sword': m('wind', 'hands', 'wind', [['slash', { count: 3, size: 1.4, gap: 0.1, spread: 0.5 }], ['tornado', { at: 0.3, height: 1.2, dur: 0.5, size: 0.6 }]], { colors: ['#bfffe0', '#ffffff'], shake: 0.12 }),
    // Ryudamon line (lastbreath)
    'Katana Attack': m('metal', 'none', 'hit', [['dash', { ghosts: 4, slashes: 2, dur: 0.18 }]], { colors: ['#e0e6f0', '#ffffff'] }),
    'Metal Armor Blade': m('metal', 'none', 'metal', [['dash', { ghosts: 5, slashes: 3, dur: 0.2, size: 1.2 }], ['slash', { at: 0.25, count: 2, size: 1.4, vertical: true }]], { colors: ['#c0c8d8', '#ffffff'], shake: 0.1 }),
    'Legendary Dragon Blade': m('wind', 'none', 'wind', [['dash', { ghosts: 6, slashes: 4, dur: 0.22, size: 1.4 }], ['tornado', { at: 0.2, height: 1.6, dur: 0.6, size: 0.8 }], ['slash', { at: 0.45, count: 4, size: 1.6, gap: 0.06 }]], { colors: ['#9fe8ff', '#ffffff'], shake: 0.18 }),
    // Monodramon line (contempt)
    'Crack Bite': m('beast', 'none', 'hit', [['dash', { ghosts: 3, slashes: 1, claw: true, dur: 0.18 }]], { colors: ['#a070ff', '#ffffff'] }),
    'Fang Strike': m('beast', 'none', 'hit', [['dash', { ghosts: 4, slashes: 2, claw: true, dur: 0.2 }], ['drain', { at: 0.3 }]], { colors: ['#4a7aff', '#d0e0ff'], shake: 0.08 }),
    // Lunamon line (arrow)
    'Tearing Shot': m('water', 'mouth', 'splash', [['orb', { size: 0.22, speed: 12, arc: 0.2, trail: 'bubble' }]], { colors: ['#9fc0ff', '#ffffff'] }),
    'Ice Archery': m('ice', 'hands', 'frost', [['arrows', { count: 3, size: 0.5, cell: 'shard', gap: 0.06 }], ['erupt', { at: 0.35, kind: 'ice', to: 'center', count: 3, height: 1.2 }]], { colors: ['#8fd6ff', '#ffffff'], shake: 0.14 }),
    // ShootingStarmon line (meep)
    'Shooting Star': m('holy', 'sky', 'holy', [['meteor', { size: 0.25, count: 1, height: 5, dur: 0.4, trail: 'spark' }]], { colors: ['#ffd84a', '#ffffff'] }),
    'Meteor Shower': m('holy', 'sky', 'blast', [['meteor', { size: 0.3, count: 4, height: 6, dur: 0.5, trail: 'spark' }]], { colors: ['#ffcf4a', '#fff6c0'], shake: 0.08 }),
    'Haley Storm': m('holy', 'sky', 'holy', [['swarm', { cell: 'star', count: 14, size: 0.36, speed: 10, wobble: 0.6, spin: 6 }], ['meteor', { at: 0.2, size: 0.45, count: 3, height: 6, dur: 0.5, trail: 'spark' }]], { colors: ['#ffe060', '#ffffff'], shake: 0.12 }),
    // Floramon line (poison)
    'Rain of Pollen': m('plant', 'aura', 'poison', [['rain', { cell: 'petal', count: 14, height: 3, size: 0.25, dur: 0.6 }]], { colors: ['#ffb3e6', '#fff6c0'] }),
    'Serpent Ruin': m('plant', 'hands', 'poison', [['whip', { dur: 0.5, width: 0.16, count: 2, sway: 1.6 }], ['vortex', { at: 0.3, size: 0.9, dur: 0.7, cell: 'petal' }]], { colors: ['#c05aff', '#9aff6a'], shake: 0.08 }),
    // Crabmon line (slam)
    'Scissors Execution': m('water', 'ground', 'splash', [['slash', { count: 2, claw: true, size: 1.2 }], ['shock', { at: 0.15, debris: false, cell: 'bubble' }]], { colors: ['#ff7a5a', '#ffe0d0'] }),
    'Fossil Bite': m('beast', 'ground', 'hit', [['shock', { debris: true, cell: 'shard' }], ['erupt', { at: 0.1, kind: 'rock', count: 3, height: 1, area: true }]], { colors: ['#c8b090', '#fff2d0'], shake: 0.12 }),
    'Sorrow Blue': m('water', 'mouth', 'splash', [['shock', { debris: false, cell: 'drop' }], ['erupt', { at: 0.1, kind: 'water', count: 5, height: 1.6, area: true }]], { colors: ['#2f6bff', '#bff0ff'], shake: 0.16 }),
    // Aruraumon line (rally)
    'Nemesis Ivy': m('plant', 'aura', 'leaf', [['dome', { size: 0.9, orbit: 'leaf', count: 4 }]], { colors: ['#6a8a3a', '#d8ff9a'] }),
    'Branch Drain': m('plant', 'aura', 'leaf', [['dome', { size: 1, orbit: 'leaf', count: 5 }], ['heal', { cell: 'leaf', size: 0.5 }]], { colors: ['#5a7a2a', '#e0ffa0'] }),
    'Illusion Mist': m('dark', 'aura', 'poison', [['vortex', { size: 1.2, dur: 0.8, cell: 'smoke', up: true, to: 'self' }], ['dome', { at: 0.1, size: 1.1 }], ['heal', { at: 0.2, cell: 'petal', size: 0.6 }]], { colors: ['#c05aff', '#ffb3e6'] }),
    // Elecmon line (chosen)
    'Sparkling Thunder': m('thunder', 'aura', 'zap', [['dome', { size: 0.9, orbit: 'bolt', count: 3 }]], { colors: ['#ffe94a', '#ffffff'] }),
    'Howling Crusher': m('beast', 'aura', 'hit', [['tornado', { cell: 'spark', height: 1.6, dur: 0.7, size: 0.8 }], ['dome', { at: 0.1, size: 1.2 }]], { colors: ['#ffcf4a', '#ff7a3a'], shake: 0.1 }),
    // Lopmon line (slam)
    'Ninja Fist': m('beast', 'ground', 'hit', [['shock', { debris: true, cell: 'paw' }], ['slash', { at: 0.1, count: 3, size: 1.1, gap: 0.06 }]], { colors: ['#c8a070', '#fff2d0'], shake: 0.1 }),
    'Asipatravana': m('holy', 'ground', 'holy', [['spikes', { count: 6, kind: 'metal', size: 0.7 }], ['shock', { at: 0.1, cell: 'blade' }]], { colors: ['#ffe680', '#ffffff'], shake: 0.16 }),
    // ToyAgumon line (flame)
    'Plastic Blaze': m('fire', 'mouth', 'blast', [['breath', { dur: 0.4, width: 0.4, cell: 'square', mix: 'flame', size: 0.32 }]], { colors: ['#ff8a2a', '#ffe08a'] }),
    'Sub Machine Gun': m('metal', 'hands', 'blast', [['volley', { count: 14, rate: 0.03, size: 0.35, guns: 2, from: 'hands', speed: 24 }], ['breath', { at: 0.2, dur: 0.3, width: 0.4, cell: 'flame', size: 0.3 }]], { colors: ['#ffb04a', '#fff2c0'], shake: 0.08 }),
    'Nuclear Laser': m('fire', 'mouth', 'blast', [['beam', { width: 0.32, dur: 0.5, charge: 0.15, core: '#ffffff' }], ['erupt', { at: 0.3, kind: 'flame', to: 'center', count: 4, height: 1.4, area: true }]], { colors: ['#ff6a2a', '#ffe08a'], shake: 0.18 }),
    // Dracmon line (backstab)
    'Eye of Nightmares': m('dark', 'none', 'dark', [['dash', { ghosts: 4, slashes: 1, dur: 0.18 }], ['glyph', { at: 0.15, glyph: 'x', size: 0.8, dur: 0.4 }]], { colors: ['#ff3a5a', '#ffd0d8'] }),
    'Sticker Blade': m('dark', 'none', 'dark', [['dash', { ghosts: 5, slashes: 2, dur: 0.2, size: 1.2 }], ['drain', { at: 0.3 }]], { colors: ['#8a2a4a', '#ffb0c0'], shake: 0.08 }),
    'Thousand Arrows': m('dark', 'none', 'dark', [['dash', { ghosts: 6, slashes: 3, dur: 0.22, size: 1.3 }], ['arrows', { at: 0.2, count: 6, size: 0.4, cell: 'needle', gap: 0.03 }]], { colors: ['#c02a4a', '#ffe0e8'], shake: 0.12 }),
    // Dorumon line (tumble)
    'Dash Metal': m('metal', 'mouth', 'metal', [['orb', { size: 0.24, speed: 14, arc: 0.1, trail: 'spark', head: 'hex' }]], { colors: ['#c0c8d8', '#ffd06a'] }),
    'Metal Meteor': m('metal', 'sky', 'metal', [['meteor', { size: 0.6, count: 1, height: 6, dur: 0.5, trail: 'spark' }], ['shock', { at: 0.5, debris: true, cell: 'hex' }]], { colors: ['#a0a8b8', '#ffe08a'], shake: 0.16 }),
    // Gumdramon line (shark)
    'Ran-gum Break': m('beast', 'hands', 'hit', [['orb', { size: 0.22, speed: 10, arc: 0.6, trail: 'wind' }], ['erupt', { at: 0.6, kind: 'water', to: 'center', count: 2, height: 1.4 }]], { colors: ['#a070ff', '#ffe0a0'] }),
    'Spin Caliber': m('metal', 'hands', 'metal', [['wheel', { size: 0.8, saw: true, dur: 0.4 }], ['erupt', { at: 0.6, kind: 'water', to: 'center', count: 3, height: 1.6 }]], { colors: ['#4a7aff', '#fff0a0'], shake: 0.1 }),
    // BlackAgumon line (transform)
    'Spitfire': m('fire', 'mouth', 'blast', [['orb', { size: 0.24, speed: 12, arc: 0.2, trail: 'flame' }]], { colors: ['#ff6a2a', '#ffd06a'] }),
    'Great Antler': m('beast', 'none', 'hit', [['streak', { width: 0.6, dur: 0.22, trail: 'smoke' }], ['shock', { at: 0.2, debris: true }]], { colors: ['#4a5a8a', '#d0e0ff'], shake: 0.14 }),
    'Metal Slash': m('metal', 'none', 'metal', [['streak', { width: 0.7, dur: 0.22, trail: 'spark' }], ['slash', { at: 0.2, count: 3, claw: true, size: 1.6 }], ['shock', { at: 0.3, debris: true, cell: 'spark' }]], { colors: ['#6a7a9a', '#ffd06a'], shake: 0.2 }),
    // Hackmon line (surge)
    'Fifth Rush': m('fire', 'none', 'hit', [['dash', { ghosts: 4, slashes: 1, claw: true, dur: 0.18 }]], { colors: ['#ff5a3a', '#ffffff'] }),
    'Fif Cross': m('fire', 'none', 'hit', [['dash', { ghosts: 5, slashes: 2, dur: 0.2 }], ['glyph', { at: 0.2, glyph: 'x', size: 1, dur: 0.4 }]], { colors: ['#ff3a3a', '#ffe0a0'], shake: 0.08 }),
    'Meteor Flame': m('fire', 'none', 'blast', [['dash', { ghosts: 6, slashes: 2, dur: 0.22, size: 1.3 }], ['meteor', { at: 0.2, size: 0.4, count: 2, height: 5, dur: 0.4, trail: 'flame' }]], { colors: ['#ff4a1a', '#ffd06a'], shake: 0.14 }),
    // Impmon line (whisper)
    'Death Slinger': m('dark', 'hands', 'blast', [['beam', { width: 0.24, dur: 0.35, charge: 0.12, core: '#ffffff' }], ['missiles', { at: 0.1, count: 2, size: 0.12, from: 'hands', curve: 0.4, dur: 0.3 }]], { colors: ['#8a2aff', '#ff6a8a'], shake: 0.16 }),
    // Terriermon line (excited)
    'Giant Missile': m('metal', 'back', 'blast', [['missiles', { count: 2, size: 0.3, from: 'back', curve: 1.2, arc: 1.4, dur: 0.6, smoke: true, impactSize: 1.8 }]], { colors: ['#4a8a4a', '#ffe08a'], shake: 0.2 }),
    // Lobomon line (energyslash)
    // Salamon line (wish)
    'Sefirot Crystal': m('holy', 'sky', 'holy', [['pillar', { size: 1.1, height: 5, dur: 0.6, to: 'self' }], ['heal', { at: 0.1, cell: 'cross', color2: '#ffffff', size: 0.7 }]], { colors: ['#fff0a0', '#9fe8ff'] }),
    // Coronamon line (traps)
    'Petit Prominence': m('fire', 'hands', 'blast', [['orb', { size: 0.2, speed: 9, arc: 0.6, count: 3, gap: 0.08, trail: 'flame' }]], { colors: ['#ff8a2a', '#ffe08a'] }),
    'Fira Claw': m('fire', 'hands', 'blast', [['orb', { size: 0.24, speed: 9, arc: 0.7, count: 3, gap: 0.08, trail: 'flame' }], ['erupt', { at: 0.5, kind: 'flame', count: 2, height: 1 }]], { colors: ['#ff6a1a', '#fff0a0'] }),
    'Phoebus Blow': m('fire', 'sky', 'blast', [['meteor', { size: 0.35, count: 3, height: 5, dur: 0.5, trail: 'flame' }], ['erupt', { at: 0.5, kind: 'flame', count: 3, height: 1.4, area: true }]], { colors: ['#ff5a1a', '#fff6c0'], shake: 0.14 }),
    // Hagurumon line (deathray)
    'Cog Crusher': m('metal', 'hands', 'metal', [['wheel', { size: 0.5, cell: 'gear', dur: 0.4 }], ['beam', { at: 0.1, width: 0.1, dur: 0.3 }]], { colors: ['#9a7aff', '#e0d0ff'] }),
    'Digital Bomb': m('metal', 'hands', 'blast', [['beam', { width: 0.18, dur: 0.4, charge: 0.1, glyph: 'tri' }], ['erupt', { at: 0.3, kind: 'metal', to: 'center', count: 3, height: 1 }]], { colors: ['#7ad0ff', '#ffffff'], shake: 0.1 }),
    'Giga Heat': m('fire', 'mouth', 'blast', [['beam', { width: 0.34, dur: 0.6, charge: 0.15, core: '#fff0c0' }], ['erupt', { at: 0.4, kind: 'flame', to: 'center', count: 4, height: 1.6, area: true }]], { colors: ['#ff4a1a', '#ffd06a'], shake: 0.2 }),
    // Vegiemon line (ray)
    'Sweet Breath': m('plant', 'mouth', 'poison', [['breath', { dur: 0.6, width: 0.35, cell: 'smoke', size: 0.4 }]], { colors: ['#9aff6a', '#ffb3e6'] }),
    'Unidentified Flying Kiss': m('dark', 'mouth', 'dark', [['beam', { width: 0.2, dur: 0.9, charge: 0.1, spiral: true }], ['swarm', { at: 0.1, cell: 'heart', count: 6, size: 0.3, speed: 8 }]], { colors: ['#ff6ad0', '#ffe0f4'] }),
    // Etemon line (cyclone)
    'Dark Spirits': m('dark', 'aura', 'dark', [['tornado', { cell: 'note', height: 1.4, dur: 1.2, size: 0.8 }]], { colors: ['#6a3aa0', '#ffd06a'] }),
    'Banana Slip': m('metal', 'aura', 'metal', [['tornado', { cell: 'spark', height: 1.6, dur: 1.4, size: 0.9 }], ['wheel', { at: 0.1, size: 1, dur: 1.2 }]], { colors: ['#ffd84a', '#c0c8d8'], shake: 0.08 }),
    'Monkey Wrench': m('metal', 'aura', 'metal', [['tornado', { cell: 'gear', height: 2, dur: 1.6, size: 1.1 }], ['shock', { at: 0.2, debris: true, cell: 'spark' }]], { colors: ['#ffcf4a', '#ffffff'], shake: 0.12 }),
    // Imperialdramon (fleet)
    'Mega Death': m('dark', 'back', 'blast', [['missiles', { count: 5, size: 0.14, from: 'back', curve: 1.4, arc: 1.2, dur: 0.6, gap: 0.05 }]], { colors: ['#4a7aff', '#ffd06a'], shake: 0.12 }),
    'Omni Blade': m('holy', 'back', 'holy', [['missiles', { count: 7, size: 0.15, from: 'back', curve: 1.2, arc: 1.4, dur: 0.6, gap: 0.04 }], ['pillar', { at: 0.4, size: 0.8, height: 5, dur: 0.5, to: 'center' }]], { colors: ['#ffffff', '#ffe680'], shake: 0.16 }),
    // Leviamon (rupture)
    'Biting Crush': m('dark', 'ground', 'dark', [['shock', { debris: true, cell: 'shard' }], ['erupt', { at: 0.15, kind: 'dark', count: 5, height: 1.8, area: true }]], { colors: ['#8a2a2a', '#ffb0a0'], shake: 0.2 }),
    'Cauda': m('dark', 'ground', 'dark', [['shock', { hammer: true, debris: true, size: 1.4 }], ['erupt', { at: 0.15, kind: 'rock', count: 6, height: 2.2, area: true }]], { colors: ['#6a1a2a', '#ff8a6a'], shake: 0.3 }),
    // Millenniummon line (chronobreak)
    'Time Unlimited': m('dark', 'aura', 'dark', [['glyph', { glyph: 'circle', size: 2, dur: 0.8, place: 'ground' }], ['beam', { at: 0.1, width: 0.12, dur: 0.4 }]], { colors: ['#6a3aff', '#ffe0a0'], shake: 0.14 }),
    // Vikemon (orbital)
    'Viking Axe': m('ice', 'sky', 'frost', [['meteor', { size: 0.5, count: 1, height: 7, dur: 0.9, trail: 'ice' }], ['erupt', { at: 1.2, kind: 'ice', to: 'center', count: 5, height: 1.6, area: true }]], { colors: ['#8fd6ff', '#ffffff'], shake: 0.2 }),
    'Arctic Blizzard': m('ice', 'sky', 'frost', [['rain', { cell: 'snow', count: 30, height: 5, size: 0.3, dur: 1.4 }], ['erupt', { at: 1.2, kind: 'ice', to: 'center', count: 6, height: 2, area: true }]], { colors: ['#bfe8ff', '#ffffff'], shake: 0.22 }),
    'Mjolnir': m('thunder', 'sky', 'zap', [['bolt', { from: 'sky', count: 2, area: 3, height: 8 }], ['meteor', { at: 0.4, size: 0.7, count: 1, height: 8, dur: 0.9, trail: 'zap' }], ['shock', { at: 1.3, hammer: true, debris: true, size: 1.6, to: 'center' }]], { colors: ['#ffe94a', '#bfe8ff'], shake: 0.3 }),
    // Lilamon line (gale)
    // MarineAngemon (polymorph)
    'Smiling Face': m('water', 'aura', 'splash', [['swarm', { cell: 'heart', count: 6, size: 0.35, speed: 7, wobble: 0.8 }]], { colors: ['#ff8ad0', '#bff0ff'] }),
    'Ocean Love': m('water', 'aura', 'splash', [['swarm', { cell: 'bubble', count: 10, size: 0.4, speed: 6, wobble: 1.2 }], ['swarm', { at: 0.1, cell: 'heart', count: 8, size: 0.35, speed: 7 }]], { colors: ['#ff6ab0', '#bff0ff'] }),
    'Kahuna Waves': m('water', 'aura', 'splash', [['wave', { width: 1.2, count: 2, dur: 0.6, flat: true }], ['swarm', { at: 0.2, cell: 'heart', count: 12, size: 0.4, speed: 8 }]], { colors: ['#2f9bff', '#ffc0e8'], shake: 0.1 }),
    // Valkyrimon (bullettime)
    "Aurvandil's Arrow": m('holy', 'hands', 'holy', [['arrows', { count: 8, size: 0.45, cell: 'needle', gap: 0.05 }]], { colors: ['#ffe680', '#ffffff'] }),
    'Fenrir Sword': m('holy', 'hands', 'holy', [['wave', { width: 1, count: 3, dur: 0.5 }], ['arrows', { at: 0.2, count: 6, rain: true, size: 0.45 }]], { colors: ['#fff0a0', '#ff9ad5'], shake: 0.12 }),
    'Laser Javelin': m('holy', 'hands', 'holy', [['arrows', { count: 14, rain: true, size: 0.5, cell: 'streak', gap: 0.03 }], ['pillar', { at: 0.3, size: 1, height: 5, dur: 0.6, to: 'center' }]], { colors: ['#ffffff', '#9fe8ff'], shake: 0.16 }),
    // Piedmon (lantern)
    "Clown's Trick": m('dark', 'hands', 'dark', [['swarm', { cell: 'card', count: 8, size: 0.36, speed: 9, spin: 8 }], ['portal', { at: 0.2, size: 0.8, dur: 0.6 }]], { colors: ['#ff3a5a', '#fff0a0'] }),
    'Masks Square': m('dark', 'hands', 'holy', [['portal', { size: 1.1, dur: 0.8 }], ['swarm', { at: 0.1, cell: 'card', count: 14, size: 0.4, speed: 10, spin: 10, fan: true }]], { colors: ['#a03aff', '#ffe680'], shake: 0.1 }),
    // Machinedramon line (reel)
    'Ultimate Slicer': m('metal', 'hands', 'metal', [['whip', { dur: 0.4, width: 0.2, leafy: false }], ['slash', { at: 0.3, count: 2, claw: true, size: 1.4 }]], { colors: ['#8a9ab8', '#ffd06a'] }),
    'Infinity Cannon': m('metal', 'back', 'blast', [['whip', { dur: 0.35, width: 0.16, leafy: false }], ['beam', { at: 0.2, width: 0.3, dur: 0.4, twin: true, from: 'back' }]], { colors: ['#ffb04a', '#fff2c0'], shake: 0.2 }),
    'Hyper Infinity Cannon': m('metal', 'back', 'blast', [['whip', { dur: 0.35, width: 0.2, leafy: false }], ['beam', { at: 0.2, width: 0.4, dur: 0.5, twin: true, from: 'back', core: '#ffffff' }], ['erupt', { at: 0.4, kind: 'flame', to: 'center', count: 3, height: 1.6 }]], { colors: ['#ff7a2a', '#fff0c0'], shake: 0.26 }),
    // Lucemon line (meteors)
    'Purgatorial Flame': m('dark', 'sky', 'dark', [['meteor', { size: 0.5, count: 5, height: 7, dur: 0.6, trail: 'dark' }], ['erupt', { at: 0.6, kind: 'dark', count: 3, height: 1.4, area: true }]], { colors: ['#6a1aa0', '#ff5a3a'], shake: 0.22 }),
    // forms swapped in for model coverage
    'Tidal Wave': m('water', 'ground', 'splash', [['wave', { width: 1, count: 1, dur: 0.5, flat: true }], ['erupt', { at: 0.3, kind: 'water', count: 3, height: 1.2 }]], { colors: ['#2f9bff', '#bff0ff'], shake: 0.08 }),
    'Garuru Kick': m('beast', 'sky', 'hit', [['glyph', { glyph: 'circle', size: 1.4, dur: 0.6, place: 'ground' }], ['dome', { at: 0.1, size: 1.2 }], ['heal', { at: 0.2, cell: 'star', size: 0.6 }]], { colors: ['#5a7aff', '#fff6c8'] }),
    'Tomahawk Stinger': m('metal', 'hands', 'metal', [['wheel', { size: 0.7, dur: 0.4 }], ['slash', { at: 0.3, count: 1, size: 1.6, vertical: true }]], { colors: ['#a0a8b8', '#ff6a5a'], shake: 0.14 }),
    'Gungnir': m('dark', 'hands', 'blast', [['beam', { width: 0.26, dur: 0.4, charge: 0.12, core: '#ffffff' }], ['missiles', { at: 0.1, count: 2, size: 0.12, from: 'hands', curve: 0.4, dur: 0.3 }]], { colors: ['#4a3a8a', '#ffd06a'], shake: 0.12 }),
    'Tenryu Slash': m('wind', 'hands', 'wind', [['whip', { dur: 0.4, width: 0.18, leafy: false }], ['erupt', { at: 0.6, kind: 'water', to: 'center', count: 4, height: 2, area: true }]], { colors: ['#ff5a3a', '#ffe0a0'], shake: 0.16 }),
    'Izuna': m('holy', 'hands', 'leaf', [['swarm', { cell: 'petal', count: 12, size: 0.3, speed: 12, fan: true }], ['tornado', { at: 0.1, cell: 'petal', height: 1.4, dur: 0.6, size: 0.7, to: 'center' }]], { colors: ['#ffd84a', '#ffffff'] }),
    'Amethyst Mandala': m('holy', 'hands', 'holy', [['glyph', { glyph: 'circle', size: 1.8, dur: 0.8 }], ['tornado', { at: 0.1, cell: 'petal', height: 2, dur: 1, size: 1, to: 'center' }]], { colors: ['#c08aff', '#ffffff'], shake: 0.12 }),
    'Dimension Destroyer': m('dark', 'aura', 'dark', [['glyph', { glyph: 'circle', size: 2.4, dur: 1, place: 'ground' }], ['portal', { at: 0.1, size: 1.2, dur: 0.8 }], ['beam', { at: 0.2, width: 0.16, dur: 0.4, core: '#ffffff' }]], { colors: ['#4a1aa0', '#ffd06a'], shake: 0.2 }),
    'Electric Thread': m('thunder', 'mouth', 'zap', [['whip', { dur: 0.3, width: 0.08, lightning: true }]]),
    'Lightning Whip': m('thunder', 'sky', 'zap', [['bolt', { from: 'sky', count: 3, area: 3, height: 8 }], ['whip', { at: 0.2, dur: 0.5, width: 0.2, lightning: true }]], { colors: ['#7fd8ff', '#ffffff'], shake: 0.2 }),
    // wild Digimon and summons
    'Volcanic Strike': m('fire', 'mouth', 'blast', [['orb', { size: 0.3, speed: 10, arc: 0.3, trail: 'flame' }]]),
    'Snow Goblin Volt': m('ice', 'hands', 'frost', [['orb', { size: 0.24, speed: 11, arc: 0.3, trail: 'ice', head: 'snow' }]]),
    'Dark Shot': m('dark', 'mouth', 'dark', [['missiles', { count: 1, size: 0.25, from: 'back', curve: 0.6, arc: 0.8, dur: 0.5, smoke: true, impactSize: 1.6 }]], { shake: 0.14 }),
    'Tri-Horn Attack': m('beast', 'ground', 'hit', [['streak', { width: 0.7, dur: 0.22, trail: 'smoke' }], ['shock', { at: 0.2, debris: true }]], { shake: 0.14 }),
    'Construct Slam': m('metal', 'ground', 'metal', [['shock', { hammer: true, debris: true }]], { colors: ['#a0a8b8', '#ffd06a'], shake: 0.12 }),
  };

  // ---------- basic attacks ----------
  const shotB = (elem, head, trail, extra = {}) => ({ kind: 'shot', elem, head, trail, size: 0.22, ...extra });
  const basic = {
    gal_hawkmon: shotB('wind', 'feather', 'wind'),
    gal_kotemon: { kind: 'slash', elem: 'metal' },
    gal_gazimon: { kind: 'claw', elem: 'thunder' },
    gal_syakomon: { kind: 'tail', elem: 'water' },
    gal_blackgabumon: { kind: 'horn', elem: 'ice' },
    gal_keramon: { kind: 'bite', elem: 'dark' },
    gal_armadillomon: { kind: 'tail', elem: 'metal' },
    gal_tapirmon: { kind: 'claw', elem: 'dark' },
    gal_gomamon: { kind: 'claw', elem: 'water' },
    gal_solarmon: shotB('holy', 'ring', 'holy'),
    gal_hyokomon: shotB('wind', 'feather', 'wind', { colors: ['#ffd06a', '#ffffff'] }),
    gal_chuumon: shotB('fire', 'dot', 'smoke', { arc: 0.6 }),
    gal_patamon: shotB('wind', 'bubble', 'bubble'),
    gal_renamon: shotB('holy', 'shard', 'spark', { colors: ['#9fe8ff', '#ffffff'] }),
    gal_shoutmon: shotB('fire', 'note', 'flame'),
    gal_kokuwamon: { kind: 'punch', elem: 'thunder' },
    gal_gizamon: { kind: 'slash', elem: 'water' },
    gal_falcomon: shotB('wind', 'blade', 'wind', { spin: 12 }),
    gal_betamon: shotB('thunder', 'bolt', 'zap'),
    gal_commandramon: shotB('metal', 'streak', null, { size: 0.16, speed: 26 }),
    gal_tsukaimon: { kind: 'claw', elem: 'dark' },
    gal_gotsumon: { kind: 'punch', elem: 'beast' },
    gal_biyomon: shotB('fire', 'feather', 'flame'),
    gal_kudamon: { kind: 'bite', elem: 'holy' },
    gal_ryudamon: { kind: 'slash', elem: 'metal' },
    gal_monodramon: { kind: 'claw', elem: 'beast' },
    gal_lunamon: shotB('ice', 'shard', 'ice'),
    gal_starmon: shotB('holy', 'star', 'spark', { spin: 8 }),
    gal_floramon: shotB('plant', 'petal', 'petal'),
    gal_agumon: shotB('fire', 'flame', 'flame'),
    gal_crabmon: { kind: 'claw', elem: 'water' },
    gal_aruraumon: shotB('plant', 'leaf', 'leaf'),
    gal_elecmon: { kind: 'slash', elem: 'thunder' },
    gal_lopmon: { kind: 'kick', elem: 'beast' },
    gal_toyagumon: { kind: 'punch', elem: 'fire' },
    gal_dracmon: { kind: 'claw', elem: 'dark' },
    gal_tentomon: shotB('thunder', 'spark', 'zap'),
    gal_dorumon: shotB('metal', 'hex', 'spark'),
    gal_gaomon: { kind: 'punch', elem: 'beast' },
    gal_gumdramon: { kind: 'tail', elem: 'beast' },
    gal_blackagumon: shotB('fire', 'flame', 'flame', { colors: ['#4a5a8a', '#ffb06a'] }),
    gal_hackmon: { kind: 'claw', elem: 'fire' },
    gal_impmon: shotB('dark', 'streak', 'dark', { size: 0.18, speed: 26 }),
    gal_terriermon: shotB('metal', 'streak', 'smoke', { size: 0.16, speed: 24 }),
    gal_lobomon: { kind: 'slash', elem: 'holy' },
    gal_salamon: shotB('holy', 'paw', 'holy'),
    gal_coronamon: shotB('fire', 'ember', 'flame'),
    gal_hagurumon: shotB('metal', 'gear', 'spark', { spin: 10 }),
    gal_vegiemon: shotB('plant', 'seed', 'leaf'),
    gal_etemon: { kind: 'punch', elem: 'dark' },
    gal_imperialdramon: shotB('holy', 'streak', 'holy'),
    gal_leviamon: { kind: 'bite', elem: 'dark' },
    gal_kimeramon: { kind: 'claw', elem: 'fire' },
    gal_vikemon: { kind: 'slash', elem: 'ice' },
    gal_lilamon: shotB('plant', 'petal', 'petal'),
    gal_marineangemon: shotB('water', 'heart', 'bubble'),
    gal_valkyrimon: shotB('holy', 'needle', 'holy', { size: 0.2, speed: 24 }),
    gal_piedmon: shotB('dark', 'card', 'dark', { spin: 10 }),
    gal_machinedramon: { kind: 'claw', elem: 'metal' },
    gal_lucemon: shotB('holy', 'cross', 'holy'),
    gal_w_goblimon: { kind: 'punch', elem: 'beast' },
    gal_w_kuwagamon: { kind: 'claw', elem: 'beast' },
    gal_w_ogremon: { kind: 'punch', elem: 'beast' },
    gal_w_tyrannomon: { kind: 'bite', elem: 'fire' },
    gal_w_monochromon: { kind: 'horn', elem: 'beast' },
    gal_w_snowgoblimon: shotB('ice', 'snow', 'ice'),
    gal_w_skullgreymon: { kind: 'claw', elem: 'dark' },
    gal_w_triceramon: { kind: 'horn', elem: 'beast' },
    gal_w_kuramon: { kind: 'bite', elem: 'dark' },
    gal_w_metalseadramon: shotB('water', 'drop', 'bubble'),
    gal_w_diaboromon: { kind: 'claw', elem: 'dark' },
    gal_w_apocalymon: shotB('dark', 'skull', 'dark'),
    gal_w_yggdrasil: shotB('ice', 'shard', 'ice'),
    gal_construct: { kind: 'punch', elem: 'metal' },
  };

  DT.Sets.extend('galaxies', { art: { prerendered: false, items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
