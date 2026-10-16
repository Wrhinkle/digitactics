// K.O. Coliseum art: item, Crest, Artifact, consumable and augment portraits (SVG painters on the js/itemart.js kit),
// pictures for the Digi-Core path, form and Jewel Gambit offer cards ('pu:<id>'), a move spec for every ability and a
// basic attack for every line (js/moves.js format).
(function (G) {
  const DT = G.DT;

  // ---------- item portraits ----------
  const C = { col_sword: '#ff7a45', col_bow: '#ffd23f', col_rod: '#b26bff', col_tear: '#4fb3ff', col_vest: '#c0c6d0', col_cloak: '#3fd6b0', col_belt: '#ff4d64', col_glove: '#8fdc4e', col_egg: '#fff3b0', col_tag: '#ffb84a' };
  const items = {};
  const blade = (k, c) => [[0, '#ffffff'], [0.45, k.lt(c, 0.55)], [1, k.dk(c, 0.35)]];

  // small motif per component, so completed items show their recipe
  const MOTIF = {
    col_sword: (p, k) => k.M.miniBlade(p),
    col_bow: (p, k) => { p.at(-8, 0, 0.5, 0, () => k.M.wing(p, '#fff6c8')); p.at(8, 0, 0.5, 0, () => k.M.wing(p, '#fff6c8'), -1); p.at(0, 0, 0.55, 0, () => p.part(k.BOLT, [[0, '#ffffff'], [1, C.col_bow]], { ow: 4 })); },
    col_rod: (p, k) => { p.glow(0, 0, 18, C.col_rod, 0.7); k.M.crystal(p, C.col_rod, 34, 10); },
    col_tear: (p, k) => { k.M.card(p, C.col_tear, 30, 42); p.at(0, 2, 0.8, 0, () => p.part(k.DROP, [[0, '#ffffff'], [1, '#2a7fd0']], { ow: 4 })); },
    col_vest: (p, k) => p.at(0, 0, 0.55, 0, () => k.M.plate(p)),
    col_cloak: (p, k) => p.at(0, 0, 0.55, 0, () => k.M.hood(p, C.col_cloak)),
    col_belt: (p, k) => { k.M.disk(p, C.col_belt, 22); p.at(0, 1, 0.18, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); },
    col_glove: (p, k) => p.at(0, 0, 0.55, 0, () => { k.M.glove(p, C.col_glove, '#f2f5e6'); k.M.clover(p); }),
    col_egg: (p, k) => p.at(0, 0, 0.5, 0, () => k.M.egg(p, C.col_egg, [[-12, -8, 7, '#ff8a4a'], [12, 8, 8, '#ff8a4a'], [-4, 24, 6, '#5fb0ff']])),
    col_tag: (p, k) => p.at(0, 0, 0.55, 0, () => k.M.tag(p, '#ffe9a0')),
  };
  // components: bronze frame
  for (const id of Object.keys(MOTIF)) {
    items[id] = (p, k) => { k.frame(p, 'square', 'bronze', [C[id]], { w: 6 }); p.obj(() => p.at(64, 64, 1.75, 0, () => MOTIF[id](p, k))); };
  }
  // completed items: gold frame split in the two component colors, a main picture and a small recipe badge
  const done = (id, a, b, fn) => {
    items[id] = (p, k) => {
      k.frame(p, 'square', 'gold', [C[a], C[b]], { w: 8 });
      p.obj(() => { fn(p, k); p.at(100, 100, 0.55, 0, () => { p.part(k.circ(0, 0, 20), k.dk(C[b], 0.4), { ow: 4, hi: false }); MOTIF[b](p, k); }); });
    };
  };
  const swordPic = (p, k, col, o = {}) => p.at(60, 62, 1.05, 45, () => k.M.sword(p, { w: o.w || 16, tip: o.tip || 16, blade: blade(k, col), core: o.core, guard: o.guard || col, gw: 36, pommel: o.pommel || col }));
  done('col_giga_blade', 'col_sword', 'col_sword', (p, k) => swordPic(p, k, C.col_sword, { w: 20, core: '#ff7a1a' }));
  done('col_titan_slayer', 'col_sword', 'col_bow', (p, k) => { p.at(48, 70, 0.5, 0, () => k.M.wing(p, C.col_bow)); swordPic(p, k, C.col_sword, { w: 26, tip: 20, guard: C.col_bow }); });
  done('col_data_gunblade', 'col_sword', 'col_rod', (p, k) => { swordPic(p, k, '#ffd0b0', { guard: 'dark' }); p.gem(58, 66, 7, C.col_rod, 'hex', 4); });
  done('col_shojin_spear', 'col_sword', 'col_tear', (p, k) => p.at(62, 64, 1, 42, () => {
    p.tube('M0 50V-14', 8, C.col_sword);
    p.part(k.poly([[0, -52], [12, -30], [6, -16], [-6, -16], [-12, -30]]), [[0, '#ffffff'], [0.45, '#9fdcff'], [1, '#1f6fc0']], { hiA: 0.8 });
    p.gem(0, -12, 4, C.col_tear, 'diamond', 3);
  }));
  done('col_night_cloak', 'col_sword', 'col_vest', (p, k) => { p.at(58, 60, 1, 0, () => k.M.hood(p, '#2a2a44', '#b08cff')); p.at(76, 72, 0.6, 30, () => k.M.miniBlade(p)); });
  done('col_blood_fang', 'col_sword', 'col_cloak', (p, k) => { swordPic(p, k, '#ff4a5a', { core: '#ff1a3a', guard: '#5a1020' }); p.at(40, 40, 0.7, 0, () => p.part(k.DROP, '#ff2a40', { ow: 4 })); });
  done('col_sterak_gauntlet', 'col_sword', 'col_belt', (p, k) => p.at(60, 64, 0.95, 0, () => { k.M.fist(p, '#c06040', '#ffd8a0'); p.at(0, -6, 0.5, 0, () => k.M.miniBlade(p)); }));
  done('col_crit_edge', 'col_sword', 'col_glove', (p, k) => { swordPic(p, k, '#fff0c0', { core: '#ffd23f', guard: C.col_glove }); p.sparkle(36, 34, 10); p.sparkle(84, 86, 7); });
  done('col_red_fang', 'col_bow', 'col_bow', (p, k) => p.at(60, 62, 1.3, 0, () => { p.glow(0, 0, 30, '#ff4a1a', 0.7); p.part(k.FLAME, [[0, '#fff2a8'], [0.5, '#ff6a1a'], [1, '#a01a0a']], { ow: 5 }); p.at(-14, 4, 0.5, 0, () => k.M.wing(p, '#ffb070')); p.at(14, 4, 0.5, 0, () => k.M.wing(p, '#ffb070'), -1); }));
  done('col_rage_rod', 'col_bow', 'col_rod', (p, k) => p.at(60, 62, 1, 35, () => { p.part(k.rr(-5, -10, 10, 54, 4), 'dark'); p.glow(0, -28, 24, '#ff7a3a', 0.7); k.M.crystal(p, '#ff7a3a', 36, 10); p.at(0, -28, 0.6, 0, () => p.part(k.BOLT, '#fff2a8', { ow: 3 })); }));
  done('col_void_staff', 'col_bow', 'col_tear', (p, k) => p.at(60, 62, 1, 30, () => { p.part(k.rr(-4, -6, 8, 50, 3), 'dark'); p.glow(0, -24, 26, '#7a3aff', 0.8); p.part(k.circ(0, -24, 13), [[0, '#1a0a2a'], [1, '#5a2aaa']], { ow: 5 }); p.line(k.circ(0, -24, 8), '#c8a0ff', 2, 0.8); }));
  done('col_titan_will', 'col_bow', 'col_vest', (p, k) => p.at(60, 62, 0.95, 0, () => { k.M.plate(p); p.at(0, 4, 0.5, 0, () => p.part(k.star(0, 0, 20, 9, 5), '#ffd84a', { ow: 4 })); }));
  done('col_kraken_fang', 'col_bow', 'col_cloak', (p, k) => p.at(60, 62, 1, 0, () => { for (let i = -1; i <= 1; i++) p.tube(`M${i * 16} 34C${i * 16 - 10} 10 ${i * 16 + 12} -6 ${i * 10} -36`, 8, '#3fd6b0'); p.gem(0, 26, 8, '#ffd23f', 'round', 4); }));
  done('col_nashor_fang', 'col_bow', 'col_belt', (p, k) => p.at(60, 62, 1, 20, () => { p.part(k.poly([[0, -42], [10, -10], [6, 30], [-6, 30], [-10, -10]]), [[0, '#ffffff'], [1, '#d07aff']], { ow: 5 }); p.part(k.rr(-14, 26, 28, 10, 4), 'gold', { ow: 4 }); }));
  done('col_armor_breaker', 'col_bow', 'col_glove', (p, k) => p.at(60, 62, 1, 0, () => { k.M.plate(p); p.line('M-30 -30L30 30M-20 -36L36 20', '#ff3a3a', 6, 0.9); }));
  done('col_ultima_rod', 'col_rod', 'col_rod', (p, k) => p.at(62, 64, 1.05, 30, () => { p.part(k.rr(-5, -8, 10, 52, 4), 'dark'); p.glow(0, -30, 30, C.col_rod, 0.9); k.M.crystal(p, C.col_rod, 46, 13); p.sparkle(16, -48, 7); p.sparkle(-16, -14, 5); }));
  done('col_seraph_staff', 'col_rod', 'col_tear', (p, k) => p.at(62, 64, 1, 25, () => { p.part(k.rr(-4, -6, 8, 52, 3), 'gold', { ow: 4 }); p.at(-10, -30, 0.55, 0, () => k.M.wing(p, '#ffffff')); p.at(10, -30, 0.55, 0, () => k.M.wing(p, '#ffffff'), -1); p.gem(0, -30, 9, C.col_tear, 'diamond', 4); }));
  done('col_crown_guard', 'col_rod', 'col_vest', (p, k) => p.at(60, 64, 1, 0, () => { p.part('M-34 10L-28 -22L-14 -4L0 -30L14 -4L28 -22L34 10Z', 'gold'); p.part(k.rr(-34, 8, 68, 14, 4), 'gold'); p.gem(0, 15, 6, C.col_rod, 'round', 4); }));
  done('col_ion_spark', 'col_rod', 'col_cloak', (p, k) => p.at(60, 62, 1.2, 0, () => { p.glow(0, 0, 30, '#9fe8ff', 0.8); p.part(k.BOLT, [[0, '#ffffff'], [1, '#3fd6b0']], { ow: 5 }); p.line(k.circ(0, 0, 26), '#bfffff', 2, 0.6); }));
  done('col_flame_tome', 'col_rod', 'col_belt', (p, k) => p.at(60, 64, 1, 0, () => { p.part(k.rr(-30, -24, 60, 46, 6), '#8a2a3a'); p.part(k.rr(-26, -20, 52, 38, 4), '#f4e2c0', { ow: 3, hi: false }); p.at(0, -2, 0.9, 0, () => p.part(k.FLAME, [[0, '#fff2a8'], [1, '#ff4a1a']], { ow: 4 })); }));
  done('col_jewel_gauntlet', 'col_rod', 'col_glove', (p, k) => p.at(60, 64, 0.95, 0, () => { k.M.glove(p, '#a070e0', '#ffd84a'); p.gem(0, 6, 8, '#ff7ad8', 'diamond', 4); }));
  done('col_blue_card', 'col_tear', 'col_tear', (p, k) => { p.at(54, 60, 1, -12, () => k.M.card(p, '#2a6ac0', 46, 66)); p.at(66, 64, 1, 8, () => { k.M.card(p, C.col_tear, 48, 70); p.glow(0, 0, 22, '#bfe6ff', 0.8); p.at(0, 0, 1.1, 0, () => p.part(k.DROP, [[0, '#ffffff'], [1, '#2a7fd0']], { ow: 4 })); }); });
  done('col_guardian_vow', 'col_tear', 'col_vest', (p, k) => p.at(60, 62, 1, 0, () => { p.part('M0 -40L32 -28C32 6 20 26 0 40C-20 26 -32 6 -32 -28Z', 'silver'); p.at(0, 0, 0.9, 0, () => p.part(k.DROP, [[0, '#ffffff'], [1, '#2a7fd0']], { ow: 4 })); }));
  done('col_adaptive_helm', 'col_tear', 'col_cloak', (p, k) => p.at(60, 62, 1, 0, () => { p.part('M-30 10C-30 -22 -16 -36 0 -36C16 -36 30 -22 30 10L22 20H-22Z', [[0, '#bfffff'], [1, '#1a7a70']]); p.part(k.rr(-22, -4, 44, 10, 4), '#1a2a3a', { ow: 3, hi: false }); p.glow(0, 1, 14, '#4fb3ff', 0.8); }));
  done('col_spirit_visage', 'col_tear', 'col_belt', (p, k) => p.at(60, 62, 1, 0, () => { p.part(k.ell(0, 0, 28, 34), [[0, '#d0fff0'], [1, '#2a8a7a']]); p.add('<ellipse cx="-10" cy="-6" rx="5" ry="7" fill="#0a2a2a"/><ellipse cx="10" cy="-6" rx="5" ry="7" fill="#0a2a2a"/>'); p.at(0, 16, 0.2, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); }));
  done('col_justice_hand', 'col_tear', 'col_glove', (p, k) => p.at(60, 64, 0.95, 0, () => { k.M.glove(p, '#f4e6c0', '#4fb3ff'); p.at(0, 4, 0.7, 0, () => k.SYM.cross(p, '#ffd84a')); }));
  done('col_bramble_plate', 'col_vest', 'col_vest', (p, k) => p.at(60, 62, 1, 0, () => { k.M.plate(p); for (const [x, y, r] of [[-30, -14, -60], [30, -14, 60], [-24, 18, -110], [24, 18, 110], [0, -32, 0]]) p.at(x, y, 1, r, () => p.part(k.poly([[0, -12], [5, 2], [-5, 2]]), '#8a6a4a', { ow: 3 })); }));
  done('col_gargoyle_stone', 'col_vest', 'col_cloak', (p, k) => p.at(60, 62, 1, 0, () => { p.part('M-30 34L-34 -10L-18 -34L0 -24L18 -34L34 -10L30 34Z', 'stone'); p.add('<circle cx="-11" cy="-4" r="4" fill="#3fd6b0"/><circle cx="11" cy="-4" r="4" fill="#3fd6b0"/>'); }));
  done('col_sunfire_core', 'col_vest', 'col_belt', (p, k) => p.at(60, 62, 1.3, 0, () => k.SYM.sun(p, '#ff7a1a')));
  done('col_steadfast_heart', 'col_vest', 'col_glove', (p, k) => p.at(60, 62, 1, 0, () => { p.part('M0 -40L32 -28C32 6 20 26 0 40C-20 26 -32 6 -32 -28Z', 'steel'); k.M.heart(p, '#ff4d64', 0.5); }));
  done('col_dragon_scale', 'col_cloak', 'col_cloak', (p, k) => p.at(60, 62, 1, 0, () => { for (const [x, y] of [[-14, -14], [14, -14], [0, 8], [-24, 10], [24, 10], [0, 30]]) p.part(`M${x} ${y - 14}C${x + 14} ${y - 10} ${x + 12} ${y + 8} ${x} ${y + 14}C${x - 12} ${y + 8} ${x - 14} ${y - 10} ${x} ${y - 14}Z`, '#3fd6b0', { ow: 4 }); }));
  done('col_even_shroud', 'col_cloak', 'col_belt', (p, k) => p.at(60, 62, 1, 0, () => { k.M.hood(p, '#4a4a6a', '#ff6a8a'); p.line(k.circ(0, -18, 30), '#ff6a8a', 2.5, 0.5); }));
  done('col_quick_silver', 'col_cloak', 'col_glove', (p, k) => p.at(60, 62, 1, 0, () => { p.part(k.ell(0, 0, 30, 20), 'chrome'); p.part(k.ell(-6, -4, 10, 6), '#ffffff', { ow: 0, hi: false }); p.line('M-40 10L-52 10M-40 0L-56 0M-40 -10L-52 -10', '#e8f0ff', 3, 0.8); }));
  done('col_warmog_disk', 'col_belt', 'col_belt', (p, k) => p.at(60, 62, 1.2, 0, () => { k.M.disk(p, C.col_belt, 36); p.at(0, 1, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); p.at(20, -20, 0.4, 0, () => k.SYM.plus(p, '#7dff9a')); }));
  done('col_striker_flail', 'col_belt', 'col_glove', (p, k) => p.at(60, 62, 1, 0, () => { p.tube('M-30 34L-4 6', 6, '#6a4a2a'); p.tube('M-4 6C6 -6 10 -10 14 -14', 3, '#9aa3b2'); p.part(k.star(20, -20, 18, 9, 8), 'steel'); p.sparkle(34, -36, 6); }));
  done('col_thief_glove', 'col_glove', 'col_glove', (p, k) => p.at(60, 64, 0.95, 0, () => { k.M.glove(p, '#3a3a4a', '#9be15d'); p.at(0, 8, 0.4, 0, () => k.M.coin(p, 0, 0, 20)); }));

  // special team-size items
  items.col_golden_egg = (p, k) => { k.frame(p, 'square', 'gold', ['#ffd84a', '#fff3b0'], { w: 8 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 40, '#ffe58c', 0.7); k.M.egg(p, '#ffd84a', [[-12, -8, 7, '#fff7c8'], [12, 8, 8, '#c08a1c'], [-4, 24, 6, '#fff7c8']]); })); };
  items.col_golden_tag = (p, k) => { k.frame(p, 'square', 'gold', ['#ffb84a', '#ffe9a0'], { w: 8 }); p.obj(() => p.at(64, 66, 1.05, 0, () => { p.glow(0, 0, 40, '#ffe58c', 0.7); k.M.tag(p, '#ffd84a'); p.at(0, 0, 0.8, 0, () => p.part(k.star(0, 0, 14, 6, 5), '#fff7c8', { ow: 4 })); })); };
  items.col_digivice = (p, k) => { k.frame(p, 'square', 'gold', ['#fff3b0', '#ffb84a'], { w: 8 }); p.obj(() => p.at(64, 66, 0.95, 0, () => k.M.digivice(p, '#eef1f6', '#ff7a45', '#7dffb0'))); };

  // Crests: shield frame in the trait color with the trait's symbol
  const CREST = { BA: 'star5', CG: 'pearl', LU: 'flame', SF: 'sun', SG: 'star6', STR: 'chess', SC: 'bug', WR: 'moon', BAS: 'shield', DUE: 'sword', EDG: 'dagger', EXE: 'crosshair', HEV: 'shieldIron', JUG: 'shield', PRO: 'drop', SOR: 'star6', PRT: 'cross', SNI: 'crosshair' };
  const TCOL = { BA: '#ff8fb8', CG: '#5ff0e0', LU: '#ff6a3d', SF: '#ffcf4a', SG: '#b88cff', STR: '#9cf6c0', SC: '#7aff5a', WR: '#8a6aff', BAS: '#c9a66b', DUE: '#f4d35e', EDG: '#ff5a7a', EXE: '#d84a4a', HEV: '#d9773a', JUG: '#a07a5a', PRO: '#6ab4ff', SOR: '#d48bff', PRT: '#7fd0c0', SNI: '#5ad1ff' };
  for (const t of Object.keys(CREST)) items[`col_cr_${t.toLowerCase()}`] = (p, k) => k.crest(p, TCOL[t], CREST[t]);

  // Artifacts: dark frame with a jewel trim
  const artifact = (id, bg, fn) => { items[id] = (p, k) => { k.frame(p, 'square', 'dark', [bg], { w: 8, studs: [[18, 18], [110, 18], [18, 110], [110, 110]], studCol: '#ff8fd8' }); p.obj(() => fn(p, k)); }; };
  artifact('col_ar_doom', '#7a3aff', (p, k) => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 36, '#a04aff', 0.8); k.M.fist(p, '#3a2a4a', '#7a3aff'); p.at(0, -6, 0.6, 0, () => p.part(k.FLAME, [[0, '#e0c0ff'], [1, '#7a3aff']], { ow: 4 })); }));
  artifact('col_ar_collector', '#ffd84a', (p, k) => p.at(64, 64, 1, 45, () => { k.M.sword(p, { w: 16, blade: blade(k, '#ffd84a'), guard: 'gold', gw: 34, pommel: '#ffd84a' }); p.at(0, 20, 1, -45, () => k.M.coin(p, 18, 10, 12)); }));
  artifact('col_ar_lichbane', '#5ad1ff', (p, k) => p.at(64, 64, 1, 40, () => { k.M.sword(p, { w: 14, blade: blade(k, '#bfe8ff'), core: '#5ad1ff', guard: 'dark', gw: 30, pommel: '#5ad1ff' }); }));
  artifact('col_ar_hull', '#9ab0c8', (p, k) => p.at(64, 64, 1, 0, () => { p.part('M-34 -10L0 -36L34 -10L26 30H-26Z', 'steel'); p.part(k.rr(-20, -6, 40, 20, 5), '#2a3040', { ow: 3, hi: false }); p.glow(0, 4, 12, '#ff6a3a', 0.8); }));
  artifact('col_ar_static', '#ffe94a', (p, k) => p.at(64, 64, 1, 40, () => { p.part(k.poly([[0, -46], [8, -20], [4, 26], [-4, 26], [-8, -20]]), [[0, '#ffffff'], [1, '#ffe94a']], { ow: 5 }); p.at(0, -6, 0.9, -40, () => p.part(k.BOLT, '#fff6a0', { ow: 3 })); }));
  artifact('col_ar_hydra', '#5fe0a0', (p, k) => p.at(64, 66, 1, 0, () => { for (const r of [-30, 0, 30]) p.at(0, 0, 1, r, () => { p.tube('M0 30C0 10 4 -10 0 -24', 9, '#3fa070'); p.part(k.ell(0, -30, 10, 8), '#5fe0a0', { ow: 4 }); }); }));
  artifact('col_ar_zhonya', '#ffcf6a', (p, k) => p.at(64, 64, 1, 0, () => { p.part('M-22 -36H22L4 0L22 36H-22L-4 0Z', 'gold'); p.part('M-14 -28H14L2 -4H-2Z', '#ffe9a0', { ow: 2, hi: false }); p.part('M-14 28H14L0 10Z', '#ffe9a0', { ow: 2, hi: false }); }));
  artifact('col_ar_infinity', '#ff8fd8', (p, k) => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 40, '#ff8fd8', 0.7); p.tube('M-30 0C-30 -18 -10 -18 0 0C10 18 30 18 30 0C30 -18 10 -18 0 0C-10 18 -30 18 -30 0Z', 9, '#ff8fd8'); p.gem(0, 0, 7, '#7dffb0', 'diamond', 4); }));

  // consumables
  items.col_core = (p, k) => { k.frame(p, 'circle', 'chrome', ['#2a6a50'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 40, '#7dffb0', 0.8); p.part(k.ngon(0, 0, 30, 6), [[0, '#e0fff0'], [0.5, '#7dffb0'], [1, '#1a6a40']], { ow: 5 }); p.part(k.ngon(0, 0, 16, 6, 0), '#ffffff', { ow: 3, hiA: 0.9 }); p.sparkle(24, -26, 8); })); };
  items.col_extractor = (p, k) => { k.frame(p, 'circle', 'steel', ['#3a4a6a'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.part(k.ngon(0, 6, 22, 6), [[0, '#e0fff0'], [1, '#2a6a50']], { ow: 4 }); p.tube('M-26 -30L-8 -8M26 -30L8 -8', 7, '#9fd8ff'); p.part(k.rr(-34, -40, 20, 12, 4), 'chrome', { ow: 4 }); p.part(k.rr(14, -40, 20, 12, 4), 'chrome', { ow: 4 }); })); };
  items.col_remover = (p, k) => { k.frame(p, 'square', 'steel', ['#ff8fa3', '#8a8f9c'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.tube('M-22 30V-6A22 22 0 0 1 22 -6V30', 14, '#e0485a', { cap: 'butt' }); p.part(k.rr(-30, 24, 16, 14, 2), 'chrome', { ow: 4 }); p.part(k.rr(14, 24, 16, 14, 2), 'chrome', { ow: 4 }); })); };
  items.col_reforger = (p, k) => { k.frame(p, 'square', 'steel', ['#ffb347', '#5a6170'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.part(k.rr(-30, 10, 60, 18, 4), 'dark'); p.part(k.poly([[-22, 10], [-14, -4], [14, -4], [22, 10]]), 'steel', { ow: 4 }); p.at(10, -22, 1, -30, () => { p.part(k.rr(-4, -4, 8, 34, 2), '#6a4a2a', { ow: 4 }); p.part(k.rr(-14, -16, 28, 14, 3), 'steel', { ow: 4 }); }); p.sparkle(-18, -16, 7, '#ffd84a'); })); };

  // ---------- augments: hex frame by tier ----------
  const AUG_METAL = ['', 'silver', 'gold', 'prism'];
  const AUG_BG = ['', '#5a6a80', '#7a5a20', '#6a3a8a'];
  const TIER = {};
  const aug = (id, tier, fn) => { TIER[id] = tier; items[`aug:${id}`] = (p, k) => { k.frame(p, 'hex', AUG_METAL[tier], [AUG_BG[tier]], { w: 7 }); p.obj(() => p.at(64, 64, 0.95, 0, () => fn(p, k))); }; };
  const coins = (p, k, n) => { for (let i = 0; i < n; i++) k.M.coin(p, -16 + (i % 3) * 16, 14 - Math.floor(i / 3) * 14, 14); };
  aug('col_a_coins', 1, (p, k) => coins(p, k, 2));
  aug('col_a_band', 1, (p, k) => p.at(0, 4, 0.75, 0, () => { k.M.glove(p, '#3a3a4a', '#9be15d'); }));
  aug('col_a_cache', 1, (p, k) => { p.at(-14, 0, 0.42, -10, () => k.M.chip(p, C.col_sword)); p.at(16, 4, 0.42, 10, () => k.M.chip(p, C.col_rod)); });
  aug('col_a_rolls', 1, (p, k) => p.at(0, 0, 1.6, 0, () => k.SYM.refresh(p, '#7dd0ff')));
  aug('col_a_heart', 1, (p, k) => { k.M.heart(p, '#ff3b55', 0.8); p.at(22, -22, 0.5, 0, () => k.SYM.plus(p, '#7dff9a')); });
  aug('col_a_warmup', 1, (p, k) => { p.at(-10, 0, 0.9, 0, () => k.SYM.sword(p, '#ff7a45')); p.at(14, 4, 0.9, 0, () => k.SYM.star6(p, '#b26bff')); });
  aug('col_a_front', 1, (p, k) => { p.at(0, 0, 1.6, 0, () => k.SYM.shield(p, '#c0c6d0')); p.line('M-30 30H30', '#ffd84a', 5, 0.9); });
  aug('col_a_ringside', 1, (p, k) => { p.at(0, -4, 1.5, 0, () => k.SYM.crosshair(p, '#5ad1ff')); p.line('M-30 32H30', '#5ad1ff', 4, 0.8); });
  aug('col_a_snack', 1, (p, k) => { p.glow(0, 0, 28, '#7dffb0', 0.8); p.part(k.ngon(0, 0, 22, 6), [[0, '#e0fff0'], [1, '#1a6a40']], { ow: 4 }); });
  aug('col_a_second', 1, (p, k) => { p.at(0, 0, 1.4, 0, () => k.SYM.refresh(p, '#7dff9a')); p.at(0, 0, 0.6, 0, () => k.SYM.plus(p, '#7dff9a')); });
  aug('col_a_prize', 2, (p, k) => coins(p, k, 5));
  aug('col_a_trove', 2, (p, k) => { p.part(k.rr(-30, -6, 60, 34, 6), 'wood'); p.part(k.rr(-32, -18, 64, 16, 6), 'gold'); k.M.coin(p, 0, -24, 10); });
  aug('col_a_ascension', 2, (p, k) => { p.at(0, 0, 1.6, 0, () => k.SYM.up(p, '#ffd84a')); p.glow(0, -10, 30, '#fff3a0', 0.5); });
  aug('col_a_sponsor', 2, (p, k) => { coins(p, k, 2); p.at(18, -20, 0.6, 0, () => k.SYM.plus(p, '#7dff9a')); });
  aug('col_a_crest', 2, (p, k) => p.at(0, 2, 0.75, 0, () => k.M.tag(p, '#ff8fb8')));
  aug('col_a_tags', 2, (p, k) => { p.at(-14, 4, 0.5, -10, () => k.M.tag(p, '#ffe9a0')); p.at(16, 6, 0.4, 0, () => k.M.egg(p, '#fff3b0', [[-10, -6, 7, '#ff8a4a'], [10, 8, 8, '#5fb0ff']])); });
  aug('col_a_snacktime', 2, (p, k) => { p.at(-8, -4, 1, 0, () => p.part(k.ngon(0, 0, 20, 6), [[0, '#e0fff0'], [1, '#1a6a40']], { ow: 4 })); k.M.coin(p, 18, 18, 11); });
  aug('col_a_lotus', 2, (p, k) => { for (let i = 0; i < 6; i++) p.at(0, 0, 1, i * 60, () => p.part('M0 0C-10 -10 -8 -26 0 -32C8 -26 10 -10 0 0Z', '#ff8fd8', { ow: 3 })); p.gem(0, 0, 8, '#ffd84a', 'round', 3); });
  aug('col_a_unity', 2, (p, k) => { for (let i = 0; i < 3; i++) p.at(0, 0, 1, i * 120, () => p.at(0, -16, 0.6, 0, () => k.SYM.star5(p, ['#ff7a45', '#5ad1ff', '#7dff9a'][i]))); });
  aug('col_a_titan', 2, (p, k) => p.at(0, 0, 0.8, 0, () => k.M.fist(p, '#d9773a', '#ffd8a0')));
  aug('col_a_evolve', 2, (p, k) => { p.at(0, 0, 1.4, 0, () => k.SYM.chevrons(p, '#7dffb0')); });
  aug('col_a_artifact', 2, (p, k) => { p.glow(0, 0, 30, '#ff8fd8', 0.7); p.gem(0, 0, 18, '#ff8fd8', 'diamond', 5); });
  aug('col_a_jackpot', 3, (p, k) => { coins(p, k, 6); p.sparkle(26, -26, 8); });
  aug('col_a_armory', 3, (p, k) => { p.at(-10, 0, 0.6, 45, () => k.M.sword(p, { w: 14, blade: blade(k, '#ff7a45'), guard: 'gold', gw: 30 })); p.at(14, 0, 0.6, -45, () => k.M.sword(p, { w: 14, blade: blade(k, '#b26bff'), guard: 'gold', gw: 30 })); });
  aug('col_a_golden', 3, (p, k) => { p.glow(0, 0, 34, '#ffe58c', 0.8); k.M.egg(p, '#ffd84a', [[-10, -6, 7, '#fff7c8'], [10, 8, 8, '#c08a1c']], 0.75); });
  aug('col_a_mega', 3, (p, k) => { p.at(0, 4, 1.5, 0, () => k.SYM.chevrons(p, '#ffd84a')); p.sparkle(24, -26, 8); });
  aug('col_a_giant', 3, (p, k) => { k.M.heart(p, '#ff3b55', 0.9); p.at(0, 0, 0.5, 0, () => k.SYM.up(p, '#ffffff')); });
  aug('col_a_feast', 3, (p, k) => { for (const [x, y] of [[-14, 6], [14, 6], [0, -16]]) p.at(x, y, 1, 0, () => p.part(k.ngon(0, 0, 14, 6), [[0, '#e0fff0'], [1, '#1a6a40']], { ow: 4 })); });
  aug('col_a_arena', 3, (p, k) => { p.part('M-34 20L-26 -18H26L34 20Z', 'stone'); for (const x of [-18, -6, 6, 18]) p.part(k.rr(x - 3, -14, 6, 30, 2), '#e2e4df', { ow: 3, hi: false }); p.at(0, -26, 0.6, 0, () => k.SYM.star5(p, '#ffd84a')); });
  aug('col_a_finals', 3, (p, k) => { p.part('M-26 -30H26V-8C26 10 12 18 0 20C-12 18 -26 10 -26 -8Z', 'gold'); p.part(k.rr(-8, 18, 16, 10, 2), 'gold', { ow: 4 }); p.part(k.rr(-18, 26, 36, 8, 3), 'gold', { ow: 4 }); });

  // ---------- offer card pictures: Digi-Core paths, forms, Jewel Gambit ----------
  const PUCOL = { burst: '#ff6a3a', xanti: '#7dffb0', dark: '#8a3aff', warp: '#5ad1ff', armor: '#ffd84a', mode: '#9cf6c0', slide: '#b8fff0', spirit: '#ffcf4a', deathx: '#ff3a6a', biomerge: '#ff8fb8', mega: '#ff7a45', sage: '#b26bff', golem: '#c9a66b', speed: '#ffe94a', overwrite: '#ff4f7b', matrix: '#4fb3ff', overdrive: '#ff9a2a', atomic: '#ff5a1a', thorn: '#8a6a4a', glacier: '#9fe8ff', inferno: '#ff4a1a', hunter: '#d84a4a', guardian: '#c0c6d0', ascend: '#fff3a0' };
  const PUSYM = { burst: 'sun', xanti: 'xros', dark: 'moon', warp: 'chevrons', armor: 'shieldIron', mode: 'copy', slide: 'refresh', spirit: 'flame', deathx: 'dagger', biomerge: 'up', mega: 'sword', sage: 'star6', golem: 'shield', speed: 'chevrons', overwrite: 'crosshair', matrix: 'drop', overdrive: 'star5', atomic: 'sun', thorn: 'cross', glacier: 'snow', inferno: 'flame', hunter: 'crosshair', guardian: 'shield', ascend: 'up' };
  for (const id of Object.keys(PUCOL)) {
    items[`pu:${id}`] = (p, k) => {
      const c = PUCOL[id];
      k.frame(p, 'hex', 'chrome', [k.dk(c, 0.3)], { w: 7 });
      p.obj(() => p.at(64, 64, 1, 0, () => { p.glow(0, 0, 34, c, 0.6); p.part(k.ngon(0, 0, 30, 6), [[0, k.lt(c, 0.6)], [0.5, c], [1, k.dk(c, 0.5)]], { ow: 5, hiA: 0.7 }); p.at(0, 0, 1.1, 0, () => k.SYM[PUSYM[id]](p, '#ffffff')); }));
    };
  }
  const FORMPIC = { blitz: ['#ffe94a', 'chevrons'], critical: ['#d84a4a', 'crosshair'], trinity: ['#a07a5a', 'shield'], ballista: ['#c0c6d0', 'shieldIron'], dorulu: ['#5ad1ff', 'dagger'], sparrow: ['#ffd23f', 'up'] };
  for (const [id, [c, sym]] of Object.entries(FORMPIC)) {
    items[`pu:form_${id}`] = (p, k) => { k.frame(p, 'circle', 'gold', [k.dk(c, 0.3)], { w: 7 }); p.obj(() => p.at(64, 64, 1.5, 0, () => { p.glow(0, 0, 24, c, 0.7); k.SYM[sym](p, c); })); };
  }
  items['pu:gambit_cash'] = (p, k) => { k.frame(p, 'circle', 'gold', ['#3a6a60'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { coins(p, k, 3); p.at(18, -22, 0.9, 0, () => k.SYM.pearl(p, '#5ff0e0')); })); };
  items['pu:gambit_double'] = (p, k) => { k.frame(p, 'circle', 'prism', ['#3a3a6a'], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => { p.at(-12, 0, 1, 0, () => k.SYM.pearl(p, '#5ff0e0')); p.at(14, 4, 1, 0, () => k.SYM.pearl(p, '#ff8fd8')); p.at(0, 24, 0.55, 0, () => k.M.die(p)); })); };

  // ---------- move specs ----------
  const M = (elem, charge, impact, fx, o = {}) => ({ elem, charge, impact, release: 0.28, fx, ...o });
  const moves = {
    // 1-cost
    'Cog Crusher': M('metal', 'hands', 'metal', [['swarm', { cell: 'gear', count: 4, size: 0.4, speed: 9, spin: 10, wobble: 0.4 }]]),
    'Destruction Grenade': M('metal', 'hands', 'blast', [['missiles', { count: 3, size: 0.14, from: 'chest', curve: 1, arc: 1.2, dur: 0.5 }]], { shake: 0.08 }),
    'Spiral Sword': M('metal', 'hands', 'metal', [['slash', { count: 3, size: 1.4, spread: 0.6 }]], { colors: ['#ffb04a', '#e8f0ff'], shake: 0.12 }),
    'Lop-Ear Ripple': M('water', 'aura', 'splash', [['rings', { count: 3, size: 0.4, gap: 0.08, dur: 0.5 }]], { colors: ['#9fd8ff', '#ffffff'] }),
    'Tear Arrow': M('water', 'hands', 'splash', [['arrows', { count: 3, size: 0.6, cell: 'drop', gap: 0.06 }]], { colors: ['#4fb3ff', '#e0f4ff'] }),
    'Ice Archery': M('ice', 'hands', 'frost', [['arrows', { count: 5, size: 0.7, cell: 'shard', gap: 0.05 }]], { shake: 0.1 }),
    'Twenty Dive': M('wind', 'none', 'hit', [['dash', { ghosts: 4, slashes: 1, dur: 0.2 }]], { release: 0.12, colors: ['#9affc8', '#ffffff'] }),
    'Road Two Decade': M('holy', 'hands', 'holy', [['slash', { count: 2, size: 1.5, gap: 0.08 }]], { colors: ['#ffe9a0', '#ffffff'], shake: 0.1 }),
    'Glass Slash': M('holy', 'hands', 'holy', [['wave', { width: 1, count: 1, dur: 0.45 }], ['slash', { at: 0.3, count: 2, size: 1.6 }]], { colors: ['#bfe8ff', '#ffffff'], shake: 0.15 }),
    'Little Horn': M('ice', 'aura', 'none', [['glyph', { glyph: 'circle', size: 1.2, dur: 0.5, to: 'self', place: 'ground' }]], { colors: ['#7ab8ff', '#ffffff'] }),
    'Fox Fire': M('fire', 'mouth', 'blast', [['breath', { dur: 0.5, width: 0.4, cell: 'flame', size: 0.4, speed: 9 }], ['glyph', { glyph: 'circle', size: 1.2, to: 'self', place: 'ground' }]], { colors: ['#3a8bff', '#bfe8ff'] }),
    'Garuru Kick': M('beast', 'none', 'hit', [['streak', { width: 0.6, dur: 0.22, trail: 'wind' }], ['glyph', { glyph: 'tri', size: 1.4, to: 'self', place: 'ground' }]], { colors: ['#5ab0ff', '#e8f6ff'], shake: 0.1 }),
    'Licht Sieger': M('holy', 'hands', 'holy', [['beam', { width: 0.18, dur: 0.35, charge: 0.08 }]], { colors: ['#fff0a0', '#ffffff'] }),
    'Solar Laser': M('holy', 'mouth', 'holy', [['beam', { width: 0.3, dur: 0.45, charge: 0.12 }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.1 }),
    'Machinegun Destroyer': M('metal', 'back', 'blast', [['missiles', { count: 6, size: 0.14, from: 'back', curve: 1.2, arc: 1.4, dur: 0.6, gap: 0.04 }], ['volley', { count: 10, rate: 0.03, from: 'chest' }]], { shake: 0.18 }),
    'Bad Message': M('dark', 'mouth', 'dark', [['orb', { size: 0.22, speed: 12, trail: 'dark', head: 'wisp' }]], { colors: ['#a04aff', '#ffb0e0'] }),
    'Death Claw': M('dark', 'hands', 'dark', [['orb', { size: 0.28, speed: 12, trail: 'dark', head: 'skull' }]]),
    'Nail Bone': M('dark', 'hands', 'dark', [['orb', { size: 0.34, speed: 11, trail: 'dark', head: 'skull', impactSize: 1.3 }]], { colors: ['#c8c0b0', '#a04aff'] }),
    'Sparkling Thunder': M('thunder', 'sky', 'zap', [['bolt', { from: 'sky', count: 2, to: 'center', area: 2 }]]),
    'Stun Beat Blow': M('thunder', 'ground', 'zap', [['shock', { cell: 'bolt' }]], { shake: 0.12 }),
    'Fist of Ice': M('ice', 'ground', 'frost', [['shock', { cell: 'snow' }], ['erupt', { at: 0.15, kind: 'ice', to: 'center', area: true, count: 3 }]], { shake: 0.15 }),
    'Electro Strike': M('thunder', 'hands', 'zap', [['orb', { head: 'spark', trail: 'zap', size: 0.2, speed: 15 }]]),
    'Hyper Cannon': M('metal', 'chest', 'blast', [['orb', { size: 0.3, speed: 14, trail: 'smoke', impactSize: 1.2 }]], { charge: 'hands', shake: 0.08 }),
    'Digital Bomb': M('metal', 'hands', 'blast', [['missiles', { count: 2, size: 0.18, from: 'hands', curve: 0.6, arc: 1.4, dur: 0.6 }]], { colors: ['#7dffb0', '#e0fff0'], shake: 0.1 }),
    'Diamond Shell': M('metal', 'ground', 'metal', [['dome', { size: 1 }], ['shock', { at: 0.2, cell: 'shard' }]], { colors: ['#e8f0ff', '#9fd8ff'] }),
    'Tail Hammer': M('metal', 'ground', 'metal', [['shock', { hammer: true, cell: 'spark' }]], { shake: 0.15 }),
    'Disaster Blaster': M('holy', 'hands', 'holy', [['beam', { width: 0.3, dur: 0.45, charge: 0.1, to: 'center' }], ['shock', { at: 0.3, cell: 'star' }]], { shake: 0.2 }),
    'Rock Fist': M('metal', 'aura', 'metal', [['dome', { size: 1 }], ['shock', { at: 0.2, debris: true }]], { colors: ['#c8a070', '#fff0c0'] }),
    'Meteor Shower': M('holy', 'sky', 'blast', [['dome', { size: 1.1, orbit: 'star', count: 4 }], ['meteor', { at: 0.15, count: 3, size: 0.25, trail: 'holy' }]], { colors: ['#ffd84a', '#fff7c8'], shake: 0.12 }),
    'Haley Storm': M('holy', 'sky', 'blast', [['dome', { size: 1.2, orbit: 'star', count: 6 }], ['meteor', { at: 0.15, count: 5, size: 0.32, trail: 'holy' }]], { colors: ['#ffe94a', '#ffffff'], shake: 0.2 }),
    'Electric Shock': M('thunder', 'mouth', 'zap', [['bolt', { from: 'caster', width: 0.07 }]]),
    'Ice Blast': M('ice', 'mouth', 'frost', [['bolt', { from: 'caster', width: 0.1 }], ['spikes', { at: 0.1, kind: 'ice', count: 3 }]], { colors: ['#9fe8ff', '#ffffff'] }),
    'Lightning Javelin': M('thunder', 'mouth', 'zap', [['beam', { width: 0.2, dur: 0.4, charge: 0.1 }], ['bolt', { at: 0.1, from: 'caster', count: 2 }]], { shake: 0.12 }),
    'Black Pearl Blast': M('water', 'mouth', 'splash', [['orb', { size: 0.26, speed: 12, head: 'bubble', trail: 'bubble' }]], { colors: ['#3a2a5a', '#e0d0ff'] }),
    'Crystal Barrage': M('ice', 'hands', 'frost', [['volley', { count: 12, rate: 0.03, cell: 'shard', size: 0.4, spread: 0.4, from: 'hands', speed: 18 }]], { colors: ['#b0e0ff', '#ffffff'] }),
    'Galactic Flare': M('holy', 'sky', 'blast', [['meteor', { count: 2, size: 0.4, trail: 'holy' }]], { colors: ['#b88cff', '#fff0ff'], shake: 0.15 }),
    'Party Time': M('plant', 'aura', 'poison', [['swarm', { cell: 'note', count: 8, size: 0.3, speed: 6, wobble: 1 }]], { colors: ['#9aff6a', '#ffe0a0'] }),
    'Sludge': M('plant', 'mouth', 'poison', [['orb', { size: 0.3, speed: 10, arc: 0.5, trail: 'smoke' }], ['erupt', { at: 0.35, kind: 'thorn', to: 'center', area: true, count: 2, height: 0.8 }]], { colors: ['#8aa040', '#c0ff70'] }),
    'Dark Pupil': M('dark', 'aura', 'dark', [['vortex', { size: 1.2, dur: 0.6 }]], { shake: 0.1 }),
    // 2-cost
    'Goblin Strike': M('beast', 'aura', 'none', [['glyph', { glyph: 'x', size: 1.1, to: 'self', place: 'ground' }]], { colors: ['#6ad04a', '#ffffff'] }),
    'Pummel Whack': M('beast', 'aura', 'none', [['glyph', { glyph: 'x', size: 1.3, to: 'self', place: 'ground' }], ['heal', { to: 'self' }]], { colors: ['#c8a070', '#ffffff'] }),
    'Knuckle Beater': M('beast', 'none', 'hit', [['streak', { width: 0.5, dur: 0.2, trail: 'spark' }]], { release: 0.12, colors: ['#9a7aff', '#ffffff'] }),
    'Fang Strike': M('beast', 'none', 'hit', [['streak', { width: 0.6, dur: 0.22, trail: 'wind' }], ['slash', { at: 0.12, count: 2, claw: true, size: 1.3 }]], { release: 0.12, colors: ['#3ad06a', '#ffffff'] }),
    'Desolation Claw': M('dark', 'none', 'dark', [['dash', { ghosts: 5, slashes: 2, claw: true, dur: 0.22, size: 1.5 }]], { release: 0.12, colors: ['#5a3aff', '#ff5a7a'], shake: 0.15 }),
    'Sticky Net': M('plant', 'hands', 'none', [['dome', { size: 0.9 }]], { colors: ['#b0ff7a', '#ffffff'] }),
    'Infinity Dream': M('wind', 'sky', 'none', [['dome', { size: 1 }], ['swarm', { cell: 'feather', count: 8, size: 0.3, speed: 5, wobble: 1.2, tint: true }]], { colors: ['#ffb0e0', '#e0f0ff'] }),
    'Hell Masquerade': M('plant', 'sky', 'none', [['dome', { size: 1.1, orbit: 'hex', count: 5 }], ['rain', { cell: 'petal', count: 12, height: 4, size: 0.3, dur: 0.8 }]], { colors: ['#ffd23f', '#ffffff'] }),
    'Demi Darts': M('dark', 'none', 'poison', [['orb', { head: 'needle', size: 0.22, speed: 18, arc: 0.05 }]], { release: 0.12, colors: ['#c05aff', '#9aff6a'] }),
    'Frost Claw': M('ice', 'hands', 'frost', [['slash', { count: 3, claw: true, size: 1.3, spread: 0.5 }]], { colors: ['#9fe8ff', '#ffffff'] }),
    'Darkness Wave': M('dark', 'hands', 'dark', [['swarm', { cell: 'bat', count: 10, size: 0.4, speed: 9, wobble: 0.8, fan: true }]], { shake: 0.08 }),
    'Gear Stinger': M('metal', 'hands', 'hit', [['orb', { head: 'needle', size: 0.22, speed: 18 }]], { colors: ['#ffd23f', '#3a3a3a'] }),
    'Turbo Stinger': M('metal', 'hands', 'blast', [['missiles', { count: 2, size: 0.12, from: 'chest', curve: 0.8, arc: 0.8, dur: 0.4 }]], { colors: ['#ffd23f', '#ffffff'] }),
    'Sky Rocket': M('metal', 'back', 'blast', [['missiles', { count: 4, size: 0.16, from: 'back', curve: 1.2, arc: 1.6, dur: 0.6, smoke: true }]], { colors: ['#ffd23f', '#ff7a45'], shake: 0.12 }),
    'Scratch Smash': M('wind', 'none', 'hit', [['dash', { ghosts: 4, slashes: 1, claw: true, dur: 0.18 }]], { release: 0.1, colors: ['#c8a070', '#ffffff'] }),
    'Kunai Wing': M('wind', 'none', 'hit', [['dash', { ghosts: 5, slashes: 1, dur: 0.18 }], ['swarm', { at: 0.1, cell: 'feather', count: 6, size: 0.3, speed: 14 }]], { release: 0.1, colors: ['#e8d0a0', '#ffffff'] }),
    'Savage Emperor': M('fire', 'none', 'blast', [['dash', { ghosts: 6, slashes: 2, dur: 0.2, size: 1.4 }], ['erupt', { at: 0.2, kind: 'flame', height: 1.6 }]], { release: 0.1, colors: ['#ff6a1a', '#ffd84a'], shake: 0.15 }),
    'Shell Guard': M('metal', 'aura', 'none', [['dome', { size: 1 }]], { colors: ['#6ad0a0', '#e0fff0'] }),
    'Golem Punch': M('metal', 'aura', 'metal', [['dome', { size: 1.1 }], ['shock', { at: 0.2, debris: true }]], { colors: ['#a08060', '#f0e0c0'] }),
    'Guard Stick': M('holy', 'aura', 'none', [['dome', { size: 1.2, orbit: 'hex', count: 6 }], ['glyph', { glyph: 'circle', size: 1.4, place: 'ground', to: 'self' }]], { colors: ['#ffe9a0', '#ffffff'] }),
    'Air Shot': M('wind', 'mouth', 'wind', [['orb', { head: 'bubble', size: 0.26, speed: 12, trail: 'wind', impactSize: 1.3 }]], { colors: ['#bfe8ff', '#ffffff'] }),
    "Heaven's Knuckle": M('holy', 'hands', 'holy', [['pillar', { size: 1, height: 6, dur: 0.5 }]], { shake: 0.1 }),
    "Heaven's Gate": M('holy', 'sky', 'holy', [['portal', { size: 1.4, dur: 0.8 }], ['pillar', { at: 0.2, size: 1.2, height: 7 }]], { shake: 0.18 }),
    'Spiral Twister': M('plant', 'sky', 'none', [['heal', { cell: 'leaf' }]], { colors: ['#3fff7a', '#d0ffd8'] }),
    'Meteor Wing': M('fire', 'sky', 'none', [['heal', { cell: 'feather', color2: '#ffd84a' }], ['rain', { cell: 'feather', count: 8, height: 4, size: 0.3, dur: 0.6 }]], { colors: ['#ff8a4a', '#ffe0a0'] }),
    'Shadow Wing': M('fire', 'sky', 'none', [['heal', { cell: 'feather', color2: '#ffd84a', size: 1.2 }], ['tornado', { cell: 'feather', height: 3, dur: 0.6, size: 0.8, to: 'self' }]], { colors: ['#ff5a1a', '#ffe0a0'] }),
    'Blazing Ice': M('ice', 'aura', 'none', [['dome', { size: 1 }]], { colors: ['#9fe8ff', '#ffffff'] }),
    'Ninja Fist': M('beast', 'aura', 'none', [['dome', { size: 1.1, orbit: 'star', count: 4 }]], { colors: ['#e0b080', '#ffffff'] }),
    'Asipatravana': M('metal', 'aura', 'metal', [['dome', { size: 1.2 }], ['slash', { at: 0.2, count: 4, size: 1.2, spread: 0.8, to: 'self' }]], { colors: ['#ffd84a', '#ffffff'] }),
    'Rolling Upper': M('beast', 'none', 'hit', [['streak', { width: 0.5, dur: 0.2 }]], { release: 0.12, colors: ['#4a8aff', '#ffffff'] }),
    'Gaoga Hound': M('wind', 'none', 'wind', [['tornado', { height: 2.5, dur: 0.5, size: 0.8 }]], { colors: ['#4a8aff', '#e0f0ff'], shake: 0.08 }),
    'Winning Knuckle': M('beast', 'hands', 'hit', [['streak', { width: 0.7, dur: 0.22, trail: 'wind' }], ['shock', { at: 0.15 }]], { colors: ['#4a8aff', '#ffd84a'], shake: 0.15 }),
    'Feather Slash': M('wind', 'hands', 'hit', [['orb', { head: 'feather', size: 0.3, speed: 15, spin: 0 }]], { colors: ['#ff6a5a', '#fff0e0'] }),
    'Blast Laser': M('wind', 'mouth', 'wind', [['rings', { count: 3, size: 0.35, gap: 0.06, dur: 0.45 }]], { colors: ['#ffd23f', '#ffffff'] }),
    'Top Gun': M('wind', 'hands', 'wind', [['orb', { size: 0.34, speed: 14, trail: 'wind', impactSize: 1.4 }]], { colors: ['#b8fff0', '#ffffff'], shake: 0.1 }),
    'Kabutogaeshi': M('metal', 'hands', 'metal', [['slash', { count: 1, size: 1.3, vertical: true }]], { colors: ['#e8d0a0', '#ffffff'] }),
    'Metal Armor Blade': M('metal', 'hands', 'metal', [['wave', { width: 0.8, dur: 0.4 }]], { colors: ['#c0c6d0', '#ffffff'], shake: 0.1 }),
    'Legendary Dragon Blade': M('holy', 'hands', 'holy', [['wave', { width: 1.2, count: 2, dur: 0.45 }], ['slash', { at: 0.25, count: 2, size: 1.6 }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.18 }),
    // 3-cost
    'Fox Tail Inferno': M('fire', 'back', 'blast', [['orb', { count: 3, gap: 0.08, size: 0.24, speed: 12, trail: 'flame', side: 0.4 }]], { colors: ['#4a8aff', '#ffd0a0'] }),
    'Talisman of Light': M('holy', 'hands', 'holy', [['swarm', { cell: 'card', count: 6, size: 0.35, speed: 10, spin: 4 }], ['cross', { at: 0.35, size: 1 }]], { shake: 0.1 }),
    'Amethyst Mandala': M('holy', 'sky', 'holy', [['glyph', { glyph: 'circle', size: 1.6, dur: 0.6 }], ['pillar', { at: 0.2, size: 1.2, height: 7 }]], { colors: ['#c08cff', '#ffffff'], shake: 0.18 }),
    'Mickey Bullet': M('metal', 'hands', 'blast', [['volley', { count: 6, rate: 0.05, size: 0.3, guns: 2, from: 'hands', speed: 22 }]], { colors: ['#ff4a8a', '#ffffff'] }),
    'Bless Fire': M('fire', 'hands', 'blast', [['volley', { count: 10, rate: 0.04, size: 0.35, guns: 2, cell: 'flame', from: 'hands', speed: 20 }]], { colors: ['#ff6a3a', '#ffe0a0'], shake: 0.08 }),
    'Meteor Flame': M('fire', 'sky', 'blast', [['meteor', { count: 3, size: 0.32, trail: 'flame' }]], { shake: 0.15 }),
    'Scissor Claw': M('beast', 'hands', 'hit', [['slash', { count: 2, claw: true, size: 1.3, spread: 0.3 }]], { colors: ['#c03a2a', '#ffffff'] }),
    'Double Scissor Claw': M('beast', 'hands', 'hit', [['slash', { count: 4, claw: true, size: 1.4, spread: 0.5, gap: 0.05 }]], { colors: ['#c03a2a', '#ffd84a'], shake: 0.12 }),
    'Dimension Scissor': M('dark', 'hands', 'dark', [['portal', { size: 1.3, dur: 0.6 }], ['slash', { at: 0.3, count: 2, claw: true, size: 1.8 }]], { colors: ['#5a2a8a', '#ff5a5a'], shake: 0.2 }),
    'Harpoon Torpedo': M('water', 'ground', 'splash', [['missiles', { count: 1, size: 0.2, from: 'mouth', curve: 0.3, arc: 0.3, dur: 0.4, impactSize: 1.4 }]], { shake: 0.1 }),
    "Vulcan's Hammer": M('metal', 'sky', 'metal', [['shock', { hammer: true, cell: 'spark' }], ['erupt', { at: 0.2, kind: 'rock', to: 'center', area: true, count: 3, height: 1.2 }]], { colors: ['#ffb04a', '#fff2c0'], shake: 0.25 }),
    'Mjolnir': M('thunder', 'sky', 'zap', [['shock', { hammer: true, cell: 'bolt' }], ['bolt', { at: 0.1, from: 'sky', count: 3, to: 'center', area: 3, height: 8 }]], { shake: 0.3 }),
    'Sweet Breath': M('plant', 'mouth', 'poison', [['breath', { dur: 0.5, width: 0.4, cell: 'smoke', size: 0.4, speed: 7 }]], { colors: ['#c0ff6a', '#ffe0ff'] }),
    'Unidentified Flying Kiss': M('dark', 'mouth', 'poison', [['orb', { head: 'heart', size: 0.3, speed: 9, wobble: 0.6 }]], { colors: ['#ff6ad8', '#ffe0ff'] }),
    'Seed Blast': M('plant', 'ground', 'leaf', [['erupt', { kind: 'thorn', to: 'center', area: true, count: 3, height: 1 }]], { colors: ['#6aff7a', '#ffe0a0'] }),
    'Sunshine Beam': M('holy', 'sky', 'holy', [['pillar', { size: 1.6, height: 7, dur: 0.6, to: 'center' }]], { colors: ['#ffd23f', '#ffffff'], shake: 0.12 }),
    'Lila Shower': M('plant', 'sky', 'leaf', [['rain', { cell: 'petal', count: 20, height: 5, size: 0.35, dur: 0.8, to: 'center' }], ['shock', { at: 0.3, cell: 'petal' }]], { colors: ['#ff8ae0', '#fff0fa'], shake: 0.12 }),
    'Fire Breath': M('fire', 'mouth', 'blast', [['breath', { dur: 0.5, width: 0.4, cell: 'flame', size: 0.4, speed: 10 }]]),
    'Nuclear Laser': M('metal', 'mouth', 'blast', [['beam', { width: 0.3, dur: 0.5, charge: 0.12 }]], { colors: ['#7dff5a', '#ffffff'], shake: 0.12 }),
    'Night Roar': M('dark', 'mouth', 'dark', [['vortex', { size: 1.4, dur: 0.6 }], ['swarm', { cell: 'bat', count: 6, size: 0.35, speed: 6 }]], { shake: 0.08 }),
    'Ultimate Slicer': M('metal', 'hands', 'metal', [['slash', { count: 3, size: 1.6, spread: 0.7 }], ['shock', { at: 0.2 }]], { colors: ['#c0c6d0', '#ff5a5a'], shake: 0.15 }),
    'Giga Heat': M('fire', 'mouth', 'blast', [['breath', { dur: 0.7, width: 0.7, cell: 'flame', size: 0.5, speed: 10, rate: 300 }], ['erupt', { at: 0.3, kind: 'flame', to: 'center', area: true, count: 3 }]], { shake: 0.2 }),
    'Lion Claw': M('beast', 'hands', 'hit', [['slash', { count: 3, claw: true, size: 1.3 }]], { colors: ['#ffd27a', '#ffffff'] }),
    'Wind Cutter Sword': M('wind', 'hands', 'wind', [['wave', { width: 0.9, dur: 0.4 }], ['tornado', { at: 0.2, height: 2, size: 0.6 }]], { shake: 0.1 }),
    'Bifrost': M('holy', 'hands', 'holy', [['beam', { width: 0.35, dur: 0.55, charge: 0.15, spiral: true }]], { colors: ['#ff8fd8', '#9fe8ff'], shake: 0.18 }),
    'Sticker Blade': M('dark', 'hands', 'dark', [['slash', { count: 2, size: 1.3 }]], { colors: ['#a03a5a', '#ffffff'] }),
    'Thousand Arrows': M('dark', 'sky', 'dark', [['arrows', { count: 10, rain: true, size: 0.6, gap: 0.03 }]], { colors: ['#c03a5a', '#ffd0e0'], shake: 0.1 }),
    'Crystal Revolution': M('ice', 'hands', 'frost', [['spikes', { kind: 'ice', count: 6, size: 1.2, gap: 0.04 }], ['erupt', { at: 0.3, kind: 'ice', height: 1.8 }]], { colors: ['#c0a0ff', '#ffffff'], shake: 0.2 }),
    'Meteor Slash': M('wind', 'hands', 'hit', [['slash', { count: 2, size: 1.4 }]], { colors: ['#9fd8ff', '#ffffff'] }),
    'Crash Charge': M('beast', 'none', 'hit', [['streak', { width: 0.7, dur: 0.22, trail: 'wind' }], ['shock', { at: 0.15, to: 'center' }]], { release: 0.12, colors: ['#ffb04a', '#ffffff'], shake: 0.1 }),
    'Grade Slash': M('wind', 'hands', 'wind', [['wave', { width: 1.1, count: 2, dur: 0.45 }], ['slash', { at: 0.25, count: 3, size: 1.5, spread: 0.6 }]], { colors: ['#b8fff0', '#ffffff'], shake: 0.15 }),
    'Smiley Bomb': M('fire', 'hands', 'blast', [['orb', { size: 0.34, speed: 8, arc: 1.4, trail: 'smoke', impactSize: 1.5 }]], { shake: 0.12 }),
    'Energy Bomb': M('metal', 'hands', 'blast', [['sphere', { size: 0.5, grow: 0.3, from: 'hands', trail: 'spark', impactSize: 1.6 }]], { colors: ['#ffb04a', '#ffffff'], shake: 0.15 }),
    'Smile Warhead': M('fire', 'sky', 'blast', [['missiles', { count: 1, size: 0.3, from: 'sky', curve: 0.2, arc: 2, dur: 0.7, impactSize: 2.2 }], ['erupt', { at: 0.7, kind: 'flame', to: 'center', area: true, count: 4, height: 1.6 }]], { shake: 0.3 }),
    // 4-cost
    'Silent Bullet': M('ice', 'hands', 'frost', [['orb', { head: 'shard', size: 0.2, speed: 20, trail: 'ice' }]], { colors: ['#bfe8ff', '#ffffff'] }),
    'Piercing Wasp': M('ice', 'hands', 'frost', [['orb', { head: 'needle', size: 0.26, speed: 22, trail: 'ice' }]], { colors: ['#9fd8ff', '#ffffff'] }),
    'Arrow of Artemis': M('ice', 'hands', 'frost', [['arrows', { count: 3, size: 0.8, cell: 'shard', gap: 0.05 }]], { colors: ['#9fe8ff', '#fff0a0'], shake: 0.1 }),
    'Strike Bomber': M('fire', 'none', 'blast', [['streak', { width: 0.7, dur: 0.25, trail: 'flame' }], ['shock', { at: 0.15 }]], { colors: ['#6ad04a', '#ffe0a0'], shake: 0.15 }),
    'Megaton Hammer Crush': M('metal', 'sky', 'metal', [['shock', { hammer: true, cell: 'spark' }], ['erupt', { at: 0.2, kind: 'rock', count: 3, height: 1.2, to: 'center', area: true }]], { shake: 0.25 }),
    'Destroyed Rush': M('metal', 'ground', 'metal', [['shock', { debris: true, size: 1.4 }], ['erupt', { at: 0.15, kind: 'rock', count: 5, height: 1.6, to: 'center', area: true }]], { colors: ['#c08040', '#ffe0a0'], shake: 0.3 }),
    'Gargo Laser': M('metal', 'hands', 'blast', [['volley', { count: 10, rate: 0.03, guns: 2, from: 'hands', speed: 22 }]], { colors: ['#9affc8', '#ffffff'] }),
    'Miracle Missile': M('metal', 'back', 'blast', [['missiles', { count: 4, size: 0.14, from: 'back', curve: 1.3, arc: 1.4, dur: 0.6, smoke: true }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.12 }),
    'Giant Missile': M('metal', 'back', 'blast', [['missiles', { count: 2, size: 0.3, from: 'back', curve: 0.6, arc: 1.6, dur: 0.7, smoke: true, impactSize: 2 }]], { colors: ['#ffb04a', '#ffffff'], shake: 0.3 }),
    "Hell's Hand": M('dark', 'hands', 'dark', [['whip', { dur: 0.35, width: 0.14, sway: 0.6 }]], { colors: ['#6a3aaa', '#c0a0ff'] }),
    'Soul Chopper': M('dark', 'hands', 'dark', [['slash', { count: 2, size: 1.5, vertical: true }]], { colors: ['#5a2a8a', '#c0a0ff'], shake: 0.1 }),
    'Phantom Blade': M('dark', 'hands', 'dark', [['wave', { width: 1, dur: 0.45 }], ['slash', { at: 0.3, count: 2, size: 1.7 }]], { colors: ['#4a1a6a', '#ff5a7a'], shake: 0.18 }),
    'Crazy Giggle': M('dark', 'mouth', 'dark', [['orb', { size: 0.26, speed: 11, trail: 'data', impactSize: 1.2 }]], { colors: ['#7aff5a', '#e0ffe0'] }),
    'Data Crusher': M('dark', 'hands', 'dark', [['sphere', { size: 0.45, grow: 0.3, from: 'hands', trail: 'data', cell: 'square', impactSize: 1.6 }]], { colors: ['#7aff5a', '#a04aff'], shake: 0.15 }),
    "Hell's Grenade": M('dark', 'sky', 'blast', [['meteor', { count: 3, size: 0.34, trail: 'dark' }], ['vortex', { at: 0.4, size: 1.2, dur: 0.5 }]], { colors: ['#a04aff', '#7aff5a'], shake: 0.22 }),
    'Mega Burst': M('fire', 'aura', 'blast', [['dome', { size: 1.1 }], ['erupt', { at: 0.2, kind: 'flame', count: 4, height: 1.4, to: 'self', area: true }]], { shake: 0.12 }),
    'Trident Revolver': M('fire', 'aura', 'blast', [['dome', { size: 1.2 }], ['volley', { at: 0.15, count: 8, rate: 0.04, cell: 'flame', from: 'hands', speed: 20 }]], { colors: ['#ff6a1a', '#fff0a0'], shake: 0.15 }),
    'Glorious Burst': M('holy', 'sky', 'holy', [['dome', { size: 1.3, orbit: 'star', count: 6 }], ['pillar', { at: 0.15, size: 1.6, height: 8, to: 'self' }], ['shock', { at: 0.3, cell: 'star' }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.25 }),
    'Bear Fist': M('beast', 'aura', 'hit', [['dome', { size: 1 }], ['shock', { at: 0.2 }]], { colors: ['#c08a5a', '#fff0d0'] }),
    'Hearts Attack': M('holy', 'aura', 'holy', [['dome', { size: 1.2 }], ['swarm', { at: 0.1, cell: 'heart', count: 12, size: 0.4, speed: 7, fan: true, tint: true }]], { colors: ['#ff6a9a', '#ffe0f0'], shake: 0.15 }),
    'Thunder Cloud': M('thunder', 'sky', 'zap', [['bolt', { from: 'sky', count: 3, to: 'center', area: 3 }]], { shake: 0.1 }),
    'Eternal Nirvana': M('dark', 'hands', 'dark', [['portal', { size: 1.3, dur: 0.7 }], ['vortex', { at: 0.2, size: 1.3, dur: 0.6 }]], { colors: ['#7a5aff', '#e0d0ff'], shake: 0.15 }),
    'Pit Bomb': M('holy', 'hands', 'blast', [['orb', { size: 0.3, speed: 9, arc: 1.2, head: 'star', trail: 'holy', impactSize: 1.6 }]], { colors: ['#ffb0e0', '#ffffff'], shake: 0.15 }),
    'Bada Boom': M('fire', 'none', 'blast', [['dash', { ghosts: 4, slashes: 0, dur: 0.18 }], ['orb', { at: 0.1, size: 0.24, speed: 14, trail: 'flame' }]], { release: 0.1, colors: ['#ff6a1a', '#ffd84a'] }),
    'Double Impact': M('dark', 'none', 'blast', [['dash', { ghosts: 5, slashes: 0, dur: 0.18 }], ['volley', { at: 0.12, count: 8, rate: 0.04, guns: 2, from: 'hands', speed: 24 }]], { release: 0.1, colors: ['#a04aff', '#ffd84a'], shake: 0.12 }),
    'Corona Destroyer': M('dark', 'none', 'blast', [['dash', { ghosts: 6, slashes: 1, dur: 0.2, size: 1.4 }], ['beam', { at: 0.15, width: 0.35, dur: 0.45, charge: 0.1 }]], { release: 0.1, colors: ['#ff3a3a', '#ffd84a'], shake: 0.25 }),
    'Fist of the Beast King': M('fire', 'hands', 'blast', [['orb', { size: 0.4, speed: 12, trail: 'flame', impactSize: 1.4 }]], { colors: ['#ffb04a', '#fff0a0'], shake: 0.1 }),
    'Ice Fist': M('ice', 'none', 'frost', [['streak', { width: 0.8, dur: 0.22, trail: 'ice' }], ['erupt', { at: 0.15, kind: 'ice', height: 1.4 }]], { release: 0.12, shake: 0.18 }),
    'Flashy Boss Punch': M('fire', 'hands', 'blast', [['streak', { width: 0.9, dur: 0.24, trail: 'flame' }], ['shock', { at: 0.18, size: 1.4, debris: true }], ['erupt', { at: 0.22, kind: 'flame', count: 3, height: 1.4 }]], { colors: ['#ff5a1a', '#ffe94a'], shake: 0.3 }),
    'Puppy Howl': M('holy', 'mouth', 'holy', [['rings', { count: 3, size: 0.35, gap: 0.07, dur: 0.45 }]], { colors: ['#fff0c0', '#ffffff'] }),
    'Lightning Paw': M('thunder', 'hands', 'zap', [['orb', { head: 'paw', size: 0.26, speed: 15, trail: 'zap' }]], { colors: ['#ffe94a', '#ffffff'] }),
    'Celestial Arrow': M('holy', 'hands', 'holy', [['arrows', { count: 4, size: 0.8, gap: 0.05 }]], { colors: ['#ffd8f0', '#ffffff'], shake: 0.1 }),
    // 5-cost
    'Blast Wing': M('dark', 'none', 'dark', [['dash', { ghosts: 5, slashes: 0, dur: 0.18 }], ['swarm', { at: 0.1, cell: 'feather', count: 10, size: 0.35, speed: 16, fan: true }]], { release: 0.1, colors: ['#3a3a5a', '#ffd84a'], shake: 0.12 }),
    'Raven King Sword': M('holy', 'none', 'holy', [['dash', { ghosts: 6, slashes: 2, dur: 0.2, size: 1.5 }]], { release: 0.1, colors: ['#ffd84a', '#ffffff'], shake: 0.18 }),
    'Crimson Formation': M('fire', 'none', 'blast', [['dash', { ghosts: 8, slashes: 3, dur: 0.22, size: 1.7 }], ['erupt', { at: 0.25, kind: 'flame', count: 4, height: 2, to: 'center', area: true }]], { release: 0.1, colors: ['#ff2a3a', '#ffd84a'], shake: 0.3 }),
    'End Waltz': M('metal', 'ground', 'metal', [['shock', { size: 1.4, cell: 'spark' }], ['slash', { at: 0.1, count: 3, size: 1.4, to: 'center' }]], { colors: ['#c0c6d0', '#ffd84a'], shake: 0.2 }),
    'Breath of the Gods': M('wind', 'mouth', 'wind', [['tornado', { height: 4, dur: 0.7, size: 1.4, to: 'center' }]], { colors: ['#e0f0ff', '#ffd84a'], shake: 0.2 }),
    'Grand Gridr': M('holy', 'sky', 'holy', [['pillar', { size: 2, height: 8, dur: 0.7, to: 'center' }], ['shock', { at: 0.25, size: 1.6, cell: 'star' }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.3 }),
    'Puppet Pummel': M('plant', 'hands', 'hit', [['whip', { dur: 0.35, width: 0.12, count: 2, sway: 0.8 }]], { colors: ['#c08a5a', '#fff0d0'] }),
    'Flying Cross Cutter': M('metal', 'hands', 'metal', [['wheel', { size: 1, dur: 0.5, saw: true }]], { colors: ['#c0c6d0', '#ff5a5a'], shake: 0.12 }),
    'Wood Cross': M('plant', 'ground', 'leaf', [['spikes', { kind: 'thorn', count: 7, size: 1.3, gap: 0.04 }], ['cross', { at: 0.3, size: 1.2 }]], { colors: ['#8a5a2a', '#c0ff7a'], shake: 0.2 }),
    'Voltage Fist': M('thunder', 'none', 'zap', [['streak', { width: 0.6, dur: 0.2, trail: 'zap' }], ['bolt', { at: 0.12, from: 'caster', count: 2 }]], { release: 0.12, colors: ['#ffe94a', '#5ad1ff'], shake: 0.12 }),
    'Justice Kick': M('thunder', 'none', 'zap', [['streak', { width: 0.7, dur: 0.22, trail: 'zap' }], ['shock', { at: 0.15, cell: 'bolt' }]], { release: 0.12, colors: ['#ff4a4a', '#ffe94a'], shake: 0.18 }),
    'Trinity Arm': M('metal', 'ground', 'metal', [['dome', { size: 1.1 }], ['shock', { at: 0.25, size: 1.5, hammer: true, cell: 'spark' }]], { colors: ['#c0c6d0', '#ff4a4a'], shake: 0.25 }),
    'Sefirot Crystal': M('holy', 'sky', 'holy', [['swarm', { cell: 'shard', count: 10, size: 0.45, speed: 10, fan: true, glow: true }], ['cross', { at: 0.35, size: 1.2 }]], { colors: ['#9fe8ff', '#fff0a0'], shake: 0.12 }),
    "Eden's Javelin": M('holy', 'sky', 'holy', [['pillar', { size: 1.8, height: 8, dur: 0.6, to: 'center' }], ['arrows', { at: 0.1, count: 6, rain: true, size: 0.9 }]], { shake: 0.25 }),
    'Flame Hellscythe': M('fire', 'hands', 'blast', [['slash', { count: 3, size: 2, spread: 1 }], ['erupt', { at: 0.2, kind: 'flame', count: 5, height: 2, to: 'center', area: true }]], { colors: ['#ff3a1a', '#ffe94a'], shake: 0.3 }),
    'Trump Sword': M('dark', 'hands', 'dark', [['swarm', { cell: 'card', count: 4, size: 0.4, speed: 16, spin: 6 }]], { colors: ['#ff3a5a', '#ffffff'] }),
    "Clown's Trick": M('dark', 'hands', 'dark', [['swarm', { cell: 'card', count: 8, size: 0.4, speed: 14, spin: 8, fan: true }]], { colors: ['#a04aff', '#ffd84a'], shake: 0.08 }),
    'Ending Illusion': M('dark', 'sky', 'dark', [['portal', { size: 1.4, dur: 0.7 }], ['swarm', { at: 0.15, cell: 'card', count: 12, size: 0.45, speed: 14, spin: 8, fan: true }]], { colors: ['#ff3a5a', '#a04aff'], shake: 0.2 }),
    'Crimson Lightning': M('dark', 'hands', 'dark', [['whip', { dur: 0.4, width: 0.12, lightning: true }]], { colors: ['#ff2a4a', '#ffd0e0'] }),
    'Venom Infusion': M('dark', 'mouth', 'poison', [['beam', { width: 0.35, dur: 0.5, charge: 0.12 }]], { colors: ['#a04aff', '#7aff5a'], shake: 0.15 }),
    'Chaos Flame': M('dark', 'mouth', 'dark', [['breath', { dur: 0.7, width: 0.7, cell: 'flame', mix: 'skull', size: 0.55, speed: 11, rate: 320 }]], { colors: ['#7a2aff', '#ff3a5a'], shake: 0.25 }),
    'Lightning Blade': M('thunder', 'hands', 'zap', [['wave', { width: 1, dur: 0.4 }], ['bolt', { at: 0.2, from: 'caster', count: 2 }]], { colors: ['#9fe8ff', '#ffffff'], shake: 0.15 }),
    'Atomic Ray': M('thunder', 'chest', 'zap', [['beam', { width: 0.4, dur: 0.55, charge: 0.15, twin: true }]], { charge: 'hands', colors: ['#5ad1ff', '#ffffff'], shake: 0.25 }),
    'Thorn Whip': M('plant', 'hands', 'leaf', [['whip', { dur: 0.4, width: 0.12, count: 2, sway: 1, leafy: true }]], { colors: ['#ff5f9e', '#c0ff7a'] }),
    // summons
    'Giga Cannon': M('metal', 'chest', 'blast', [['shock', { size: 1.2, cell: 'spark' }], ['missiles', { at: 0.05, count: 3, size: 0.16, from: 'back', curve: 1, arc: 1, dur: 0.5 }]], { charge: 'hands', shake: 0.15 }),
    'Infinity Cannon': M('metal', 'chest', 'blast', [['shock', { size: 1.5, cell: 'spark' }], ['missiles', { at: 0.05, count: 6, size: 0.2, from: 'back', curve: 1.2, arc: 1.2, dur: 0.55, gap: 0.04 }]], { charge: 'hands', shake: 0.25 }),
    'Branch Drain': M('plant', 'aura', 'none', [['dome', { size: 1 }]], { colors: ['#8a5a2a', '#c0ff7a'] }),
    'Needle Spray': M('plant', 'aura', 'hit', [['volley', { count: 12, rate: 0.02, cell: 'needle', size: 0.5, spread: 0.5, from: 'chest', speed: 20 }]], { colors: ['#c8ff6a', '#fff6c0'] }),
    // wild Digimon
    'Electric Stun Blast': M('thunder', 'hands', 'zap', [['bolt', { from: 'caster', width: 0.06 }]]),
    'Fungus Crusher': M('plant', 'hands', 'poison', [['orb', { head: 'seed', size: 0.24, speed: 11, arc: 0.4, trail: 'leaf' }]], { colors: ['#a06aff', '#ffe0a0'] }),
    'Scissors Execution': M('water', 'hands', 'splash', [['slash', { count: 2, claw: true, size: 1.2 }]], { colors: ['#ff6a3a', '#ffffff'] }),
    'Snow Goblin Volt': M('ice', 'hands', 'frost', [['orb', { head: 'snow', size: 0.24, speed: 12, trail: 'ice' }]]),
    'Volcanic Strike': M('fire', 'ground', 'blast', [['shock', { debris: true }], ['erupt', { at: 0.15, kind: 'flame', height: 1.2 }]], { shake: 0.12 }),
    'Burning Fist': M('fire', 'hands', 'blast', [['orb', { size: 0.34, speed: 12, trail: 'flame', impactSize: 1.3 }]]),
    'Snow Punch': M('ice', 'hands', 'frost', [['streak', { width: 0.5, dur: 0.2, trail: 'ice' }], ['erupt', { at: 0.12, kind: 'ice', height: 1 }]], { release: 0.12 }),
    'Horn Blaster': M('holy', 'mouth', 'holy', [['beam', { width: 0.22, dur: 0.4, charge: 0.1 }]], { colors: ['#fff0a0', '#9fe8ff'] }),
    'Berserk Sword': M('metal', 'ground', 'metal', [['shock', { cell: 'spark' }], ['slash', { at: 0.1, count: 2, size: 1.4, to: 'center' }]], { shake: 0.15 }),
    'Tri-Horn Attack': M('beast', 'none', 'hit', [['streak', { width: 0.8, dur: 0.25, trail: 'wind' }]], { release: 0.12, colors: ['#c08a5a', '#ffffff'], shake: 0.12 }),
    'Dark Spirits': M('dark', 'hands', 'dark', [['swarm', { cell: 'skull', count: 6, size: 0.35, speed: 7, wobble: 0.8 }], ['vortex', { at: 0.3, size: 1, dur: 0.5 }]], { shake: 0.1 }),
    'Dark Shot': M('dark', 'chest', 'dark', [['orb', { size: 0.4, speed: 11, trail: 'dark', head: 'skull', impactSize: 1.5 }]], { charge: 'hands', shake: 0.15 }),
    'Digitalize of Soul': M('holy', 'ground', 'holy', [['glyph', { glyph: 'circle', size: 1.8, place: 'ground' }], ['shock', { at: 0.25, size: 1.6, cell: 'star' }]], { colors: ['#3a3a5a', '#ffd84a'], shake: 0.25 }),
    'Transcendent Sword': M('holy', 'hands', 'holy', [['slash', { count: 2, size: 1.9, vertical: true }], ['wave', { at: 0.15, width: 1.2, dur: 0.45 }]], { colors: ['#ff8a3a', '#ffffff'], shake: 0.25 }),
    'Omni Blade': M('holy', 'hands', 'holy', [['wave', { width: 1.3, count: 2, dur: 0.45 }]], { colors: ['#ffd84a', '#ffffff'], shake: 0.2 }),
    'Time Unlimited': M('dark', 'sky', 'dark', [['portal', { size: 1.6, dur: 0.8 }], ['meteor', { at: 0.2, count: 4, size: 0.35, trail: 'dark' }]], { colors: ['#7a2aff', '#ff3a5a'], shake: 0.3 }),
    'Darkness Zone': M('dark', 'aura', 'dark', [['vortex', { size: 2, dur: 0.8, to: 'center' }], ['drain', { at: 0.3 }]], { shake: 0.25 }),
    // model-swap forms
    'Evil Hurricane': M('dark', 'aura', 'none', [['tornado', { cell: 'smoke', height: 2.5, dur: 0.6, size: 0.9, to: 'self' }], ['heal', { to: 'self' }]], { colors: ['#6a3aaa', '#ffd0a0'] }),
    'Retriever Bark': M('holy', 'mouth', 'holy', [['rings', { count: 3, size: 0.4, gap: 0.07, dur: 0.45 }]], { colors: ['#ffe9c0', '#ffffff'] }),
    'Black Cannon': M('dark', 'mouth', 'dark', [['orb', { size: 0.34, speed: 13, trail: 'dark', impactSize: 1.3 }]], { shake: 0.1 }),
    'Black Cannon Burst': M('dark', 'mouth', 'dark', [['sphere', { size: 0.5, grow: 0.3, from: 'mouth', trail: 'dark', impactSize: 1.7 }], ['shock', { at: 0.4, to: 'center' }]], { shake: 0.2 }),
    'Spider Thread': M('dark', 'hands', 'poison', [['net', { size: 1 }], ['whip', { at: 0.1, dur: 0.35, width: 0.08, sway: 0.4 }]], { colors: ['#a04aff', '#e0e0e0'] }),
    'Hyper Mugen Cannon': M('metal', 'chest', 'blast', [['beam', { width: 0.4, dur: 0.55, charge: 0.15, twin: true }], ['missiles', { at: 0.1, count: 4, size: 0.16, from: 'back', curve: 1, arc: 1, dur: 0.5 }]], { charge: 'hands', colors: ['#ff5a3a', '#ffe0a0'], shake: 0.25 }),
    'Crescent Moon Kick': M('beast', 'aura', 'hit', [['dome', { size: 1.1 }], ['slash', { at: 0.2, count: 1, size: 1.6, vertical: true, to: 'self' }]], { colors: ['#e8d0a0', '#ffffff'], shake: 0.12 }),
    'Serpent Cure': M('plant', 'hands', 'leaf', [['whip', { dur: 0.45, width: 0.14, count: 2, sway: 1, leafy: true }], ['swarm', { at: 0.1, cell: 'petal', count: 8, size: 0.3, speed: 9, fan: true }]], { colors: ['#ff8ae0', '#c0ff7a'], shake: 0.1 }),
    'Serpent Ruin': M('plant', 'sky', 'leaf', [['rain', { cell: 'petal', count: 24, height: 5, size: 0.4, dur: 0.9, to: 'center' }], ['vortex', { at: 0.2, size: 1.4, dur: 0.6 }]], { colors: ['#ff5fae', '#ffe0f0'], shake: 0.2 }),
    'Thousand Fists': M('beast', 'aura', 'none', [['glyph', { glyph: 'circle', size: 1.4, to: 'self', place: 'ground' }]], { colors: ['#ffd84a', '#ffffff'] }),
    'Spiritual Enchantment': M('holy', 'aura', 'hit', [['glyph', { glyph: 'tri', size: 1.5, to: 'self', place: 'ground' }], ['shock', { at: 0.15, to: 'self' }]], { colors: ['#ffd84a', '#9fe8ff'], shake: 0.15 }),
    'Mirage Image': M('holy', 'sky', 'holy', [['glyph', { glyph: 'circle', size: 1.6, to: 'self', place: 'ground' }], ['bolt', { at: 0.1, from: 'sky', count: 2, to: 'self' }], ['dash', { at: 0.2, ghosts: 6, slashes: 1, dur: 0.2 }]], { colors: ['#ffe94a', '#ffffff'], shake: 0.2 }),
    'Heat Viper': M('fire', 'mouth', 'blast', [['beam', { width: 0.3, dur: 0.45, charge: 0.1 }]], { colors: ['#ff6a1a', '#ffe0a0'], shake: 0.15 }),
    'Giga Sea Destroyer': M('dark', 'chest', 'dark', [['sphere', { size: 0.6, grow: 0.4, from: 'hands', trail: 'dark', impactSize: 2 }], ['vortex', { at: 0.4, size: 1.5, dur: 0.6, to: 'center' }]], { charge: 'hands', colors: ['#3a2a8a', '#ff3a5a'], shake: 0.3 }),
  };

  // ---------- basic attacks ----------
  const shot = (elem, head, o = {}) => ({ kind: 'shot', elem, head, size: 0.22, ...o });
  const basic = {
    col_hagurumon: { kind: 'punch', elem: 'metal' },
    col_lunamon: shot('water', 'drop', { trail: 'bubble', colors: ['#9fd8ff', '#ffffff'] }),
    col_zubamon: { kind: 'slash', elem: 'holy', colors: ['#ffe9a0', '#ffffff'] },
    col_gabumon: shot('ice', 'flame', { trail: 'ice', colors: ['#3a8bff', '#bfe8ff'] }),
    col_lobomon: shot('holy', 'streak', { trail: 'holy', speed: 20 }),
    col_tsukaimon: shot('dark', 'wisp', { trail: 'dark' }),
    col_elecmon: { kind: 'tail', elem: 'thunder' },
    col_kokuwamon: shot('thunder', 'spark', { trail: 'zap', speed: 18 }),
    col_armadillomon: { kind: 'tail', elem: 'metal', colors: ['#e8c070', '#ffffff'] },
    col_gotsumon: { kind: 'punch', elem: 'metal', colors: ['#c8a070', '#fff0c0'] },
    col_betamon: shot('thunder', 'spark', { trail: 'zap' }),
    col_syakomon: shot('water', 'bubble', { trail: 'bubble', colors: ['#5a3a8a', '#e0d0ff'] }),
    col_numemon: { kind: 'bite', elem: 'plant', colors: ['#8aa040', '#c0ff70'] },
    col_goblimon: { kind: 'punch', elem: 'beast', colors: ['#6ad04a', '#ffffff'] },
    col_monodramon: { kind: 'claw', elem: 'beast', colors: ['#9a7aff', '#ffffff'] },
    col_wormmon: shot('plant', 'seed', { trail: 'leaf', arc: 0.3 }),
    col_demidevimon: shot('dark', 'needle', { speed: 20 }),
    col_fanbeemon: shot('metal', 'needle', { speed: 20, colors: ['#ffd23f', '#3a3a3a'] }),
    col_falcomon: { kind: 'slash', elem: 'wind' },
    col_kamemon: { kind: 'punch', elem: 'water', colors: ['#6ad0a0', '#e0fff0'] },
    col_patamon: shot('wind', 'bubble', { arc: 0.25, colors: ['#bfe8ff', '#ffffff'] }),
    col_biyomon: { kind: 'peck', elem: 'fire', colors: ['#ff7aa0', '#ffd0e0'] },
    col_lopmon: { kind: 'kick', elem: 'ice', colors: ['#c8a070', '#ffffff'] },
    col_gaomon: { kind: 'punch', elem: 'beast', colors: ['#4a8aff', '#ffffff'] },
    col_hawkmon: shot('wind', 'feather', { trail: 'wind', colors: ['#ff6a5a', '#fff0e0'] }),
    col_ryudamon: { kind: 'slash', elem: 'metal', colors: ['#e8d0a0', '#ffffff'] },
    col_kyubimon: shot('fire', 'flame', { trail: 'flame', colors: ['#4a8aff', '#ffd0a0'] }),
    col_sistermonnoir: shot('metal', 'streak', { speed: 24, colors: ['#ff4a8a', '#ffffff'] }),
    col_kuwagamon: { kind: 'claw', elem: 'beast', colors: ['#c03a2a', '#ffffff'] },
    col_ikkakumon: { kind: 'horn', elem: 'water' },
    col_labramon: { kind: 'bite', elem: 'holy', colors: ['#ffe9c0', '#ffffff'] },
    col_vegiemon: shot('plant', 'seed', { trail: 'smoke', arc: 0.4, colors: ['#c0ff6a', '#ffe0ff'] }),
    col_lalamon: { kind: 'tail', elem: 'plant', colors: ['#6aff7a', '#ffe0a0'] },
    col_tyrannomon: shot('fire', 'flame', { trail: 'flame' }),
    col_saberdramon: { kind: 'claw', elem: 'dark' },
    col_reppamon: { kind: 'claw', elem: 'wind', colors: ['#ffd27a', '#ffffff'] },
    col_sangloupmon: { kind: 'slash', elem: 'dark', colors: ['#a03a5a', '#ffffff'] },
    col_kotemon: { kind: 'slash', elem: 'wind', colors: ['#9fd8ff', '#ffffff'] },
    col_mamemon: shot('fire', 'ember', { arc: 0.5, trail: 'smoke' }),
    col_sistermonciel: shot('ice', 'shard', { trail: 'ice', speed: 22 }),
    col_coredramon: { kind: 'bite', elem: 'fire', colors: ['#6ad04a', '#ffe0a0'] },
    col_gargomon: shot('metal', 'streak', { speed: 24, colors: ['#9affc8', '#ffffff'] }),
    col_bakemon: { kind: 'claw', elem: 'dark', colors: ['#6a3aaa', '#c0a0ff'] },
    col_keramon: shot('dark', 'square', { trail: 'data', colors: ['#7aff5a', '#e0ffe0'] }),
    col_geogreymon: { kind: 'horn', elem: 'fire' },
    col_bearmon: { kind: 'punch', elem: 'beast', colors: ['#c08a5a', '#fff0d0'] },
    col_wizardmon: shot('thunder', 'spark', { trail: 'zap', colors: ['#ffe94a', '#7a5aff'] }),
    col_impmon: shot('fire', 'ember', { trail: 'flame', speed: 20 }),
    col_leomon: { kind: 'punch', elem: 'fire', colors: ['#ffb04a', '#fff0a0'] },
    col_salamon: shot('holy', 'paw', { arc: 0.3, spin: 0 }),
    col_ravemon: { kind: 'slash', elem: 'holy', colors: ['#ffd84a', '#ffffff'] },
    col_craniamon: { kind: 'slash', elem: 'metal', colors: ['#c0c6d0', '#ffd84a'] },
    col_puppetmon: shot('plant', 'blade', { spin: 8, colors: ['#c08a5a', '#ffffff'] }),
    col_justimon: { kind: 'punch', elem: 'thunder', colors: ['#ff4a4a', '#ffe94a'] },
    col_ophanimon: shot('holy', 'star', { trail: 'holy' }),
    col_piedmon: shot('dark', 'card', { spin: 10, colors: ['#ff3a5a', '#ffffff'] }),
    col_myotismon: shot('dark', 'bat', { trail: 'dark', colors: ['#ff2a4a', '#ffd0e0'] }),
    col_hiandromon: { kind: 'slash', elem: 'thunder', colors: ['#9fe8ff', '#ffffff'] },
    col_lotosmon: shot('plant', 'petal', { trail: 'petal', colors: ['#ff8ae0', '#ffffff'] }),
    col_merukimon: { kind: 'punch', elem: 'holy', colors: ['#ffd84a', '#ffffff'] },
    col_mech: { kind: 'punch', elem: 'metal' },
    col_root: { kind: 'tail', elem: 'plant', colors: ['#8a5a2a', '#c0ff7a'] },
    col_spine: shot('plant', 'needle', { speed: 20, colors: ['#c8ff6a', '#fff6c0'] }),
    col_w_gazimon: { kind: 'claw', elem: 'thunder' },
    col_w_mushroomon: shot('plant', 'seed', { arc: 0.4 }),
    col_w_crabmon: { kind: 'claw', elem: 'water', colors: ['#ff6a3a', '#ffffff'] },
    col_w_snowgoblimon: shot('ice', 'snow', { trail: 'ice' }),
    col_w_monochromon: { kind: 'horn', elem: 'fire' },
    col_w_meramon: { kind: 'punch', elem: 'fire' },
    col_w_hyogamon: { kind: 'punch', elem: 'ice' },
    col_w_unimon: shot('holy', 'star', { trail: 'holy' }),
    col_w_knightmon: { kind: 'slash', elem: 'metal' },
    col_w_triceramon: { kind: 'horn', elem: 'beast' },
    col_w_etemon: shot('dark', 'note', { arc: 0.3 }),
    col_w_skullgreymon: { kind: 'claw', elem: 'dark' },
    col_w_kimeramon: { kind: 'punch', elem: 'dark', colors: ['#9a5aff', '#ffd84a'] },
    col_w_darkdramon: { kind: 'punch', elem: 'dark', colors: ['#ff3a5a', '#7aff5a'] },
    col_w_alphamon: { kind: 'slash', elem: 'holy', colors: ['#3a3a5a', '#ffd84a'] },
    col_w_omnimon: { kind: 'slash', elem: 'holy', colors: ['#ff8a3a', '#ffffff'] },
    col_w_imperialdramon: { kind: 'slash', elem: 'holy' },
    col_w_millenniummon: shot('dark', 'wisp', { trail: 'dark' }),
    col_w_apocalymon: { kind: 'claw', elem: 'dark' },
  };

  DT.Sets.extend('coliseum', { art: { prerendered: false, items, moves, basic } });
})(typeof window !== 'undefined' ? window : globalThis);
