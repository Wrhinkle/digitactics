// Dragonlands art: item, Crest and augment portraits (SVG painters on the js/itemart.js kit), a move look for every
// ability (js/moves.js spec format) and a basic attack look for every line. Painters run only in the browser; this
// file only defines functions and plain data, so it also loads headless.
(function (G) {
  const DT = G.DT;

  // ---------- small painting helpers (p = picture builder, k = kit) ----------
  const CC = { sword: '#ff7a45', bow: '#ffd23f', rod: '#b26bff', tear: '#4fb3ff', vest: '#c0c6d0', cloak: '#5fe0c0', belt: '#ff5d73', glove: '#9be15d', egg: '#ffb46a' };
  const blade = (k, c) => [[0, '#ffffff'], [0.45, k.lt(c, 0.55)], [1, k.dk(c, 0.35)]];
  const glowGem = (p, k, x, y, r, c, shape = 'round') => { p.glow(x, y, r * 2.4, c, 0.7); p.gem(x, y, r, c, shape, 4); };
  const flame = (p, k, x, y, s, c) => p.at(x, y, s, 0, () => p.part(k.FLAME, [[0, '#fff2a8'], [0.5, c], [1, k.dk(c, 0.4)]], { ow: 4.5 }));
  const bolt = (p, k, x, y, s, r, c) => p.at(x, y, s, r, () => p.part(k.BOLT, [[0, '#ffffff'], [0.5, c], [1, k.dk(c, 0.4)]], { ow: 4.5 }));
  const drop = (p, k, x, y, s, c) => p.at(x, y, s, 0, () => { p.part(k.DROP, [[0, '#ffffff'], [0.55, c], [1, k.dk(c, 0.45)]], { ow: 4.5 }); p.gloss(-4, 3, 3, 6, 20, 0.9); });
  const shieldShape = 'M0 -42L34 -32C36 -4 26 24 0 42C-26 24 -36 -4 -34 -32Z';
  const scale = (p, k, x, y, s, c) => p.at(x, y, s, 0, () => p.part('M0 -14C10 -14 14 -4 14 4C14 12 8 16 0 18C-8 16 -14 12 -14 4C-14 -4 -10 -14 0 -14Z', [[0, k.lt(c, 0.6)], [0.5, c], [1, k.dk(c, 0.5)]], { ow: 3.5 }));
  const claw = (p, k, x, y, s, r, c) => p.at(x, y, s, r, () => p.part('M-6 18C-8 0 -2 -18 12 -30C6 -12 6 4 8 18Z', [[0, '#fffbea'], [0.5, k.lt(c, 0.3)], [1, k.dk(c, 0.4)]], { ow: 4 }));
  const wing = (p, k, x, y, s, c, flip) => p.at(x, y, s, 0, () => k.M.wing(p, c), flip ? -1 : 1);
  const crystal = (p, k, x, y, s, c) => p.at(x, y, s, 0, () => k.M.crystal(p, c, 40, 11));
  const coins = (p, k, n = 3) => { const at = [[-16, 18, -10], [14, 12, 12], [-2, -8, 0], [20, -14, 8], [-22, -10, -6]]; for (let i = 0; i < n; i++) k.M.coin(p, at[i][0], at[i][1], 14, at[i][2]); };
  const ring = (p, k, r, c) => { p.add(`<circle cx="0" cy="0" r="${r}" fill="none" stroke="#15161d" stroke-width="8"/><circle cx="0" cy="0" r="${r}" fill="none" stroke="${c}" stroke-width="4"/>`); };

  // frames by item kind
  const comp = (id, fn) => (p, k) => { k.frame(p, 'square', 'bronze', [CC[id]], { w: 6 }); p.obj(() => p.at(64, 64, 1, 0, () => fn(p, k))); };
  const done = (a, b, fn) => (p, k) => { k.frame(p, 'square', 'gold', [CC[a], CC[b]], { w: 8 }); p.obj(() => p.at(64, 64, 1, 0, () => fn(p, k))); };
  const golden = (fn) => (p, k) => { k.frame(p, 'circle', 'gold', ['#ffcf4a', '#7a4a10'], { w: 8, studs: [[64, 8], [120, 64], [64, 120], [8, 64]] }); p.obj(() => p.at(64, 64, 1, 0, () => fn(p, k))); };
  const shining = (fn) => (p, k) => { k.frame(p, 'hex', 'prism', ['#ff8fd8', '#6ff0ff'], { w: 8 }); p.obj(() => p.at(64, 64, 0.95, 0, () => { p.glow(0, 0, 46, '#fff3a0', 0.4); fn(p, k); p.sparkle(34, -34, 7); p.sparkle(-36, 30, 5); })); };
  const tool = (c1, c2, fn) => (p, k) => { k.frame(p, 'square', 'steel', [c1, c2], { w: 7 }); p.obj(() => p.at(64, 64, 1, 0, () => fn(p, k))); };

  const items = {
    // ---- components ----
    sword: comp('sword', (p, k) => { k.M.chip(p, CC.sword, 62); p.glow(0, 0, 22, '#ffcf8a', 0.7); p.at(0, 0, 1.05, 45, () => k.M.miniBlade(p)); }),
    bow: comp('bow', (p, k) => { wing(p, k, -16, 2, 0.82, '#fff6c8'); wing(p, k, 16, 2, 0.82, '#fff6c8', true); p.part(k.rr(-22, -20, 44, 44, 8), CC.bow); bolt(p, k, 0, 2, 0.6, 0, '#ffd23f'); }),
    rod: comp('rod', (p, k) => p.at(2, 0, 1, 38, () => { p.part(k.rr(-5, -12, 10, 56, 4), 'dark'); p.gem(0, 46, 6.5, CC.rod, 'round', 4.5); p.glow(0, -34, 26, CC.rod, 0.75); crystal(p, k, 0, -34, 0.95, CC.rod); })),
    tear: comp('tear', (p, k) => { p.at(-8, -2, 1, -12, () => k.M.card(p, k.dk(CC.tear, 0.35), 48, 68)); p.at(4, 2, 1, 8, () => { k.M.card(p, CC.tear, 50, 72); drop(p, k, 0, 4, 1, '#9fdcff'); }); }),
    vest: comp('vest', (p, k) => { k.M.plate(p); scale(p, k, 0, 4, 0.9, '#ff9a5a'); }),
    cloak: comp('cloak', (p, k) => { k.M.hood(p, CC.cloak); p.gem(0, -6, 4.5, '#bdfff0', 'diamond', 3.5); }),
    belt: comp('belt', (p, k) => { k.M.disk(p, CC.belt, 41); p.at(0, 1, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 10 })); }),
    glove: comp('glove', (p, k) => { k.M.glove(p, CC.glove, '#f2f5e6'); p.at(1, 6, 1.05, 0, () => k.M.clover(p)); }),
    egg: comp('egg', (p, k) => { k.M.egg(p, '#ffd8a8', [[-14, -10, 8, '#ff6a3d'], [14, 4, 10, '#ff6a3d'], [-6, 26, 7, '#ff6a3d'], [18, -24, 5, '#ffcf4a']]); scale(p, k, 0, -30, 0.5, '#ff6a3d'); }),
    drg_crown: (p, k) => { k.frame(p, 'square', 'gold', [CC.egg, '#ffd84a'], { w: 8 }); p.obj(() => p.at(64, 70, 1, 0, () => { p.glow(0, -10, 40, '#ffd84a', 0.5); p.part('M-36 10L-42 -26L-18 -6L0 -38L18 -6L42 -26L36 10Z', 'gold'); p.part(k.rr(-38, 6, 76, 18, 5), 'gold'); for (const [x, y] of [[-42, -26], [0, -38], [42, -26]]) p.gem(x, y, 5, '#ff6a3d', 'round', 3.5); p.gem(0, 15, 7, '#ff6a3d', 'diamond', 4); })); },

    // ---- completed items ----
    drg_dragon_fang: done('sword', 'sword', (p, k) => { p.at(0, 0, 1.05, 45, () => k.M.sword(p, { w: 18, blade: blade(k, '#ff7a45'), core: '#ff5a1a', guard: '#ff7a45', gw: 40, pommel: '#ff3a1a' })); claw(p, k, -26, -6, 0.9, -30, '#ffe0c0'); }),
    drg_titan_slayer: done('sword', 'bow', (p, k) => { p.at(0, 2, 1.05, 45, () => k.M.sword(p, { w: 28, tip: 20, blade: blade(k, '#ff7a45'), guard: CC.bow, gw: 42, pommel: CC.bow })); bolt(p, k, -10, -14, 0.6, 20, CC.bow); }),
    drg_gunblade: done('sword', 'rod', (p, k) => { p.at(-2, 2, 1, 40, () => { k.M.sword(p, { w: 14, top: -50, blade: blade(k, '#ff7a45'), guard: 'dark', gw: 24 }); p.part(k.circ(0, 18, 9), 'chrome', { ow: 4 }); glowGem(p, k, 0, 18, 5, CC.rod, 'hex'); }); }),
    drg_mana_spear: done('sword', 'tear', (p, k) => p.at(0, 0, 1.05, 42, () => { p.tube('M0 50V-14', 8, CC.sword); p.part(k.rr(-9, -20, 18, 8, 3), 'chrome', { ow: 4 }); p.glow(0, -34, 22, '#9fdcff', 0.8); p.part(k.poly([[0, -56], [12, -34], [6, -20], [-6, -20], [-12, -34]]), [[0, '#ffffff'], [0.45, '#9fdcff'], [1, '#1f6fc0']]); })),
    drg_night_veil: done('sword', 'vest', (p, k) => { p.glow(8, -10, 36, '#7a5cff', 0.5); p.part('M-30 -26A15 15 0 1 0 -12 -44A11 11 0 1 1 -30 -26Z', '#fff2b0', { ow: 4 }); p.part('M-12 24C-22 -6 2 -34 40 -44C16 -26 4 -4 0 26Z', [[0, '#e6e8ff'], [0.35, '#8a8cc0'], [1, '#22223d']]); p.part(k.ell(-8, 26, 15, 8), 'chrome', { ow: 5 }); }),
    drg_blood_fang: done('sword', 'cloak', (p, k) => { claw(p, k, -12, 4, 1.3, 10, '#ff4a4a'); claw(p, k, 10, 0, 1.1, 22, '#ff8a6a'); drop(p, k, 22, 30, 0.7, '#ff2e43'); }),
    drg_herald_banner: done('sword', 'belt', (p, k) => { p.tube('M-26 46V-44', 6, '#8a5a2a'); p.part('M-24 -42H34L24 -24L34 -6H-24Z', [[0, '#ffe0c0'], [0.5, CC.sword], [1, k.dk(CC.belt, 0.3)]]); p.at(6, -24, 0.42, 0, () => k.SYM.flame(p, '#fff2a8')); p.gem(-26, -46, 5, CC.belt, 'round', 3.5); }),
    drg_crit_edge: done('sword', 'glove', (p, k) => { p.at(0, 0, 1.05, 45, () => k.M.sword(p, { w: 11, tip: 12, top: -52, blade: blade(k, '#ff7a45'), core: '#ffb070', guard: CC.glove, gw: 26, grip: '#2f3b22' })); p.at(-22, 22, 0.9, 0, () => k.M.clover(p, '#f2ffd0')); }),
    drg_rapid_cannon: done('bow', 'bow', (p, k) => p.at(0, 4, 1, -30, () => { p.part(k.rr(-40, -12, 64, 24, 8), 'steel'); p.part(k.rr(18, -16, 18, 32, 6), CC.bow); for (const y of [-4, 4]) p.line(`M-36 ${y}H16`, '#15161d', 2, 0.4); p.glow(42, 0, 16, '#fff3a0', 0.9); bolt(p, k, 44, 0, 0.45, 90, CC.bow); })),
    drg_rage_blade: done('bow', 'rod', (p, k) => { p.glow(0, 0, 40, CC.bow, 0.35); p.at(0, 0, 1.05, 30, () => k.M.sword(p, { w: 16, blade: [[0, '#fffbe0'], [0.5, '#ffb02e'], [1, '#c43a10']], guard: CC.rod, gw: 34, pommel: CC.rod, core: '#ff6a2a' })); flame(p, k, 18, -26, 0.9, '#ff6a2a'); }),
    drg_volt_disk: done('bow', 'tear', (p, k) => { k.M.disk(p, CC.tear, 40); p.glow(0, 0, 30, '#fff3a0', 0.6); bolt(p, k, 2, 0, 1.45, 10, CC.bow); }),
    drg_titan_will: done('bow', 'vest', (p, k) => { p.part('M-27 -38L27 -38C32 -12 32 12 26 40L-26 40C-32 12 -32 -12 -27 -38Z', 'chrome'); p.glow(0, 0, 26, CC.bow, 0.55); for (const y of [-14, 0, 14]) p.part(`M-15 ${y + 7}L0 ${y - 6}L15 ${y + 7}L15 ${y + 12}L0 ${y - 1}L-15 ${y + 12}Z`, [[0, '#fff8d0'], [1, '#f0a800']], { ow: 3.5, hi: false }); }),
    drg_twin_shot: done('bow', 'cloak', (p, k) => p.at(0, 2, 1, 35, () => { p.part(k.rr(-6, -18, 12, 62, 4), CC.cloak); p.tube('M-42 -2Q0 -34 42 -2', 8, CC.bow); for (const x of [-6, 6]) { p.tube(`M${x} 14V-40`, 3.5, '#e8e2cc', { ow: 4 }); p.part(k.poly([[x, -54], [x + 5, -40], [x - 5, -40]]), k.lt(CC.cloak, 0.4), { ow: 3.5, hi: false }); } })),
    drg_lure_portal: done('bow', 'belt', (p, k) => { p.glow(0, 0, 44, '#b04aff', 0.6); p.part(k.ell(0, 0, 34, 40), [[0, '#2a0a3a'], [0.6, '#6a1a9a'], [1, '#ff5d73']]); p.part(k.ell(0, 0, 20, 26), [[0, '#05060a'], [1, '#3a0a4a']], { ow: 3, hi: false }); for (const [x, y] of [[-7, -4], [7, -4]]) p.add(`<ellipse cx="${x}" cy="${y}" rx="3.4" ry="2.4" fill="#ffd84a"/>`); }),
    drg_armor_breaker: done('bow', 'glove', (p, k) => p.at(-4, 4, 1.02, 45, () => { p.part(k.rr(-7, 26, 14, 22, 4), k.dk(CC.glove, 0.4), { ow: 5 }); p.part(k.rr(-17, 0, 34, 28, 7), CC.glove); p.part(k.poly([[0, -54], [19, -8], [-19, -8]]), [[0, '#fff8d0'], [0.5, CC.bow], [1, '#9a6800']]); })),
    drg_ultima_rod: done('rod', 'rod', (p, k) => p.at(0, 2, 1, 28, () => { p.tube('M0 50V-4', 7, '#5b3796'); p.glow(0, -26, 34, CC.rod, 0.85); crystal(p, k, 0, -28, 1, '#c48aff'); crystal(p, k, -22, -40, 0.35, CC.rod); crystal(p, k, 22, -40, 0.35, CC.rod); })),
    drg_seraph_rod: done('rod', 'tear', (p, k) => p.at(0, 2, 1, 12, () => { p.tube('M0 50V-6', 6.5, '#e6f2ff'); wing(p, k, -8, -22, 0.62, '#dff2ff'); wing(p, k, 8, -22, 0.62, '#dff2ff', true); p.glow(0, -26, 22, CC.rod, 0.8); crystal(p, k, 0, -24, 0.85, CC.rod); })),
    drg_solar_locket: done('rod', 'vest', (p, k) => { p.tube('M-14 -44C-14 -56 14 -56 14 -44', 4, '#e8c860', { ow: 4 }); p.part(k.circ(0, 0, 36), 'gold'); p.at(0, 0, 0.9, 0, () => k.SYM.sun(p, '#ffb02e')); p.gem(0, 0, 6, CC.rod, 'round', 3.5); }),
    drg_mana_zap: done('rod', 'cloak', (p, k) => { p.part(k.rr(-22, 26, 44, 14, 4), 'dark'); p.part(k.poly([[-12, 27], [12, 27], [7, -12], [-7, -12]]), CC.cloak); p.glow(0, -24, 30, CC.rod, 0.9); for (const r of [-40, 0, 40]) bolt(p, k, 0, -30, 0.55, r, '#e2b8ff'); p.gem(0, -24, 11, CC.rod, 'round', 5); }),
    drg_flame_tome: done('rod', 'belt', (p, k) => { flame(p, k, -6, -30, 0.9, '#ff7a3a'); flame(p, k, 12, -34, 1.1, '#ff5d4a'); p.part(k.rr(-32, -24, 60, 58, 6), CC.rod); p.part(k.rr(-36, -24, 10, 58, 4), k.dk(CC.rod, 0.35), { ow: 5 }); p.part(k.circ(0, 5, 15), 'gold', { ow: 4 }); flame(p, k, 0, 6, 0.6, CC.belt); }),
    drg_jewel_gauntlet: done('rod', 'glove', (p, k) => { k.M.fist(p, CC.glove, k.dk(CC.glove, 0.35)); for (let i = 0; i < 4; i++) p.gem(-21 + i * 14, -22, 4.2, CC.rod, 'diamond', 3.5); p.gem(0, 35, 6.5, CC.rod, 'hex', 4); p.sparkle(30, -36, 7); }),
    drg_blue_core: done('tear', 'tear', (p, k) => { p.glow(0, 0, 44, '#4fb3ff', 0.7); p.part(k.ngon(0, 0, 34, 6), [[0, '#dff4ff'], [0.5, '#4fb3ff'], [1, '#14408a']]); p.part(k.ngon(0, 0, 20, 6), [[0, '#ffffff'], [1, '#7fd0ff']], { ow: 3.5 }); drop(p, k, 0, 2, 0.8, '#1f6fc0'); }),
    drg_frost_heart: done('tear', 'vest', (p, k) => { p.glow(0, 0, 42, '#bfeaff', 0.6); p.at(0, 0, 0.9, 0, () => k.M.heart(p, '#9fdcff', 1, 6)); p.at(0, 0, 0.9, 0, () => k.SYM.snow(p, '#ffffff')); }),
    drg_chalice: done('tear', 'cloak', (p, k) => { p.part('M-28 -36H28C28 -6 16 8 6 10V30H18V40H-18V30H-6V10C-16 8 -28 -6 -28 -36Z', 'gold'); p.part(k.ell(0, -36, 28, 7), [[0, '#9fdcff'], [1, '#1f6fc0']], { ow: 4 }); p.glow(0, -44, 22, '#b26bff', 0.8); p.sparkle(-8, -50, 6); p.sparkle(12, -54, 4); }),
    drg_holy_ring: done('tear', 'belt', (p, k) => { p.glow(0, 0, 44, '#fff0a0', 0.6); ring(p, k, 30, '#ffe58c'); p.at(0, 0, 0.8, 0, () => k.SYM.cross(p, '#ff8aa0')); }),
    drg_justice_hand: done('tear', 'glove', (p, k) => { k.M.glove(p, '#9fdcff', '#f2f5e6'); p.at(0, 4, 0.8, 0, () => k.SYM.crosshair(p, '#ffd84a')); }),
    drg_spike_plate: done('vest', 'vest', (p, k) => { for (let i = 0; i < 8; i++) p.at(0, 0, 1, i * 45, () => p.part(k.poly([[0, -50], [6, -32], [-6, -32]]), 'chrome', { ow: 3.5, hi: false })); p.part(shieldShape, 'chrome'); scale(p, k, 0, 0, 1.3, '#9aa3b2'); }),
    drg_stone_hide: done('vest', 'cloak', (p, k) => { p.part(shieldShape, 'stone'); for (const [x, y] of [[-12, -14], [12, -10], [0, 10], [-14, 18], [14, 20]]) scale(p, k, x, y, 0.55, '#8e928c'); p.gem(0, -2, 6, CC.cloak, 'hex', 3.5); }),
    drg_flame_core: done('vest', 'belt', (p, k) => { p.glow(0, 0, 44, '#ff6a2a', 0.6); p.part(k.circ(0, 0, 30), [[0, '#fff2a8'], [0.45, '#ff7a2a'], [1, '#a01a1a']]); flame(p, k, 0, 2, 1.4, '#ff5a1a'); }),
    drg_shroud: done('vest', 'glove', (p, k) => { k.M.hood(p, [[0, '#d0d6e8'], [1, '#3a4058']], '#9fdcff'); p.line('M-40 20L40 -10', '#cfeaff', 3, 0.7); }),
    drg_dragon_claw: done('cloak', 'cloak', (p, k) => { for (const [x, r] of [[-18, -18], [0, 0], [18, 18]]) claw(p, k, x, 8, 1.2, r, CC.cloak); scale(p, k, 0, 34, 0.7, '#ff6a3d'); }),
    drg_zephyr: done('cloak', 'belt', (p, k) => { for (let i = 0; i < 4; i++) p.at(0, 0, 1, i * 90, () => p.part('M0 -6C14 -12 26 -26 22 -44C16 -30 8 -22 0 -18Z', [[0, '#ffffff'], [0.5, '#b8fff0'], [1, '#2a8a7a']], { ow: 4 })); p.part(k.circ(0, 0, 9), '#ffffff', { ow: 4 }); }),
    drg_quicksilver: done('cloak', 'glove', (p, k) => { p.part(k.EGG, [[0, '#ffffff'], [0.45, '#d9dfe9'], [1, '#606978']]); p.line('M-20 -10C-6 -16 10 -4 22 -10M-24 8C-10 2 8 14 24 8', '#ffffff', 3, 0.8); p.gem(0, 22, 6, CC.glove, 'diamond', 3.5); }),
    drg_giga_belt: done('belt', 'belt', (p, k) => { p.part(k.rr(-46, -12, 92, 24, 8), CC.belt); p.part(k.rr(-16, -18, 32, 36, 6), 'gold'); p.at(0, 0, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 12 })); }),
    drg_banshee: done('belt', 'glove', (p, k) => { p.glow(0, 0, 44, '#e0d0ff', 0.6); p.part('M0 -40C22 -40 30 -20 30 0C30 22 18 40 0 40C-18 40 -30 22 -30 0C-30 -20 -22 -40 0 -40Z', 'glass'); k.M.hood(p, '#b8a0e0', '#ffffff'); }),
    drg_thief_glove: done('glove', 'glove', (p, k) => { k.M.glove(p, '#3a3a4a', '#9be15d'); p.at(-14, -6, 0.36, -20, () => k.M.chip(p, '#ff7a45')); p.at(14, -2, 0.36, 15, () => k.M.chip(p, '#4fb3ff')); }),

    // ---- golden items (Golden Armor) ----
    drg_g_axe: golden((p, k) => p.at(0, 0, 1, -30, () => { p.tube('M0 46V-36', 7, '#8a5a2a'); p.part('M0 -40C22 -44 34 -30 34 -12C22 -18 10 -16 0 -10Z', 'gold'); p.part('M0 -40C-22 -44 -34 -30 -34 -12C-22 -18 -10 -16 0 -10Z', 'gold'); k.M.coin(p, 0, 30, 10, 0); })),
    drg_g_blade: golden((p, k) => { p.at(0, 0, 1, 45, () => k.M.sword(p, { w: 14, blade: 'gold', guard: '#ff6a3d', gw: 30, pommel: '#ffd84a' })); p.at(-24, 22, 0.55, 0, () => k.M.die(p, [5, 2, 3])); }),
    drg_g_staff: golden((p, k) => p.at(0, 0, 1, 20, () => { p.tube('M0 50V-14', 7, '#c08a1c'); p.glow(0, -30, 26, '#ffd84a', 0.8); k.M.coin(p, 0, -30, 18, 0); p.sparkle(14, -48, 6); })),
    drg_g_mail: golden((p, k) => { p.part('M-16 -34Q0 -24 16 -34L33 -28L30 -4C30 18 18 34 0 42C-18 34 -30 18 -30 -4L-33 -28Z', 'gold'); for (const [x, y] of [[-10, -10], [10, -10], [0, 6], [-10, 22], [10, 22]]) scale(p, k, x, y, 0.5, '#ffd84a'); }),
    drg_g_gem: golden((p, k) => { p.glow(0, 0, 44, '#7fe8ff', 0.6); p.gem(0, 0, 30, '#5ad6ff', 'diamond', 6); p.sparkle(26, -30, 8); }),
    drg_g_investor: golden((p, k) => { p.part(k.rr(-30, -6, 60, 40, 6), 'wood'); p.part(k.rr(-30, -14, 60, 12, 4), 'gold'); coins(p, k, 3); }),
    drg_g_diamond: golden((p, k) => { k.M.glove(p, '#d9f6ff', '#ffd84a'); p.gem(0, 4, 12, '#7fe8ff', 'diamond', 4.5); }),
    drg_g_crown: golden((p, k) => p.at(0, 6, 1, 0, () => { p.glow(0, -10, 40, '#ffd84a', 0.6); p.part('M-36 10L-42 -26L-18 -6L0 -38L18 -6L42 -26L36 10Z', 'gold'); p.part(k.rr(-38, 6, 76, 18, 5), 'gold'); for (const [x, y] of [[-42, -26], [0, -38], [42, -26]]) p.gem(x, y, 5, '#ff3b55', 'round', 3.5); coins(p, k, 1); })),

    // ---- Shining items ----
    drg_r_fang: shining((p, k) => p.at(0, 0, 1.05, 45, () => k.M.sword(p, { w: 20, blade: 'prism', core: '#ffffff', guard: 'gold', gw: 42, pommel: '#ff8fd8' }))),
    drg_r_gauntlet: shining((p, k) => { k.M.fist(p, '#fff0ff', '#a08cff'); for (let i = 0; i < 4; i++) p.gem(-21 + i * 14, -22, 4.2, '#ff8fd8', 'diamond', 3.5); p.gem(0, 35, 6.5, '#6ff0ff', 'hex', 4); }),
    drg_r_seraph: shining((p, k) => p.at(0, 0, 1, 12, () => { p.tube('M0 50V-6', 6.5, '#ffffff'); wing(p, k, -8, -22, 0.7, '#fff0ff'); wing(p, k, 8, -22, 0.7, '#fff0ff', true); crystal(p, k, 0, -24, 0.9, '#ff8fd8'); })),
    drg_r_spike: shining((p, k) => { for (let i = 0; i < 8; i++) p.at(0, 0, 1, i * 45, () => p.part(k.poly([[0, -50], [6, -32], [-6, -32]]), 'prism', { ow: 3.5, hi: false })); p.part(shieldShape, 'prism'); }),
    drg_r_belt: shining((p, k) => { p.part(k.rr(-46, -12, 92, 24, 8), 'prism'); p.part(k.rr(-16, -18, 32, 36, 6), 'gold'); p.at(0, 0, 0.3, 0, () => p.part(k.HEART, '#ff3b55', { ow: 12 })); }),
    drg_r_blood: shining((p, k) => { claw(p, k, -12, 4, 1.3, 10, '#ff8fd8'); claw(p, k, 10, 0, 1.1, 22, '#6ff0ff'); drop(p, k, 22, 30, 0.7, '#ff2e43'); }),
    drg_r_rage: shining((p, k) => { p.at(0, 0, 1.05, 30, () => k.M.sword(p, { w: 16, blade: 'prism', guard: 'gold', gw: 34, core: '#ffffff' })); flame(p, k, 18, -26, 0.9, '#ff8fd8'); }),
    drg_r_ring: shining((p, k) => { ring(p, k, 30, '#fff3a0'); p.at(0, 0, 0.8, 0, () => k.SYM.cross(p, '#ff8fd8')); }),

    // ---- consumables ----
    drg_remover: tool('#ff8fa3', '#8a8f9c', (p, k) => p.at(0, 4, 1, 0, () => { p.part('M-30 -30H-12V10C-12 18 12 18 12 10V-30H30V10C30 32 -30 32 -30 10Z', [[0, '#ffb0c0'], [0.5, '#e84060'], [1, '#801830']]); for (const x of [-21, 21]) p.part(k.rr(x - 9.5, -42, 19, 14, 2), 'chrome', { ow: 5 }); })),
    drg_reforger: tool('#ffb347', '#5a6170', (p, k) => p.at(0, 4, 1, 0, () => { p.part('M-40 -2L28 -2C38 -2 44 -8 48 -12L44 2C38 10 28 12 20 12L14 24L24 36L-24 36L-14 24L-18 12C-28 10 -38 6 -40 -2Z', 'dark'); p.glow(14, -14, 20, '#ffb347', 0.9); p.add(`<path d="${k.star(14, -14, 13, 4, 8)}" fill="#fff2b0"/>`); })),
    drg_dup_lesser: tool('#9fdcff', '#3a6aa0', (p, k) => { p.at(-8, -4, 0.62, -10, () => k.M.digivice(p, '#eef1f6', '#5aa0ff', '#9fdcff')); p.at(14, 8, 0.62, 10, () => k.M.digivice(p, '#d8dde6', '#5aa0ff', '#9fdcff')); }),
    drg_dup: tool('#ffd84a', '#3a6aa0', (p, k) => { p.at(-8, -4, 0.66, -10, () => k.M.digivice(p, '#fff7d8', '#ffb02e', '#ffe58c')); p.at(14, 8, 0.66, 10, () => k.M.digivice(p, '#fff0c0', '#ffb02e', '#ffe58c')); p.sparkle(30, -30, 7); }),
  };

  // Crests: shield frame in the trait color with the trait's symbol
  const TRAIT_SYM = {
    gold: 'coin', rage: 'flame', tamer: 'up', mage: 'star6', rider: 'shield', mirage: 'moon', guard: 'shieldIron', assn: 'dagger',
    star: 'star5', xab: 'xros', stone: 'chess', slayer: 'sword', thunder: 'chevrons', whisper: 'skull', lagoon: 'drop',
    bruiser: 'plus', cannon: 'crosshair', evoker: 'pearl', mystic: 'cross', mode: 'refresh', swift: 'crosshair', warrior: 'sword',
  };
  const TRAIT_COL = {
    gold: '#ffcf4a', rage: '#ff5a3a', tamer: '#ff8fc8', mage: '#d48bff', rider: '#c08a5a', mirage: '#5ad6ff', guard: '#c9c06b', assn: '#ff4f7b',
    star: '#8f7bff', xab: '#e04a4a', stone: '#5fbf8a', slayer: '#c9a66b', thunder: '#ffe94a', whisper: '#9b4dff', lagoon: '#2f9bff',
    bruiser: '#d9773a', cannon: '#ff9a3a', evoker: '#7fe0ff', mystic: '#9cf6c0', mode: '#ffb05a', swift: '#5ad1ff', warrior: '#f4d35e',
  };
  for (const t of Object.keys(TRAIT_SYM)) items[`drg_emblem_${t}`] = (p, k) => k.crest(p, TRAIT_COL[t], TRAIT_SYM[t]);

  // Augments: hexagon frame, silver / gold / prismatic by tier, a symbol or small scene
  const AUG = { 1: ['silver', '#5a6b84'], 2: ['gold', '#8a6420'], 3: ['prism', '#6a3a9a'] };
  const aug = (tier, fn) => (p, k) => { const [m, bg] = AUG[tier]; k.frame(p, 'hex', m, [bg], { w: 7 }); p.obj(() => p.at(64, 64, 0.92, 0, () => fn(p, k))); };
  const sym = (tier, name, col, s = 1.6) => aug(tier, (p, k) => { p.glow(0, 0, 36, col, 0.5); p.at(0, 0, s, 0, () => k.SYM[name](p, col)); });
  const A = {
    drg_hoard: aug(1, (p, k) => coins(p, k, 4)),
    drg_feather: aug(1, (p, k) => { wing(p, k, -6, 0, 0.9, '#ffffff'); wing(p, k, 6, 0, 0.9, '#ffffff', true); }),
    drg_exiles: sym(1, 'moon', '#b0b8ff'),
    drg_battlemage: aug(1, (p, k) => { p.part(shieldShape, 'steel'); crystal(p, k, 0, 0, 0.7, '#b26bff'); }),
    drg_friends: aug(1, (p, k) => { p.at(-14, 0, 0.6, -12, () => k.M.heart(p, '#ff6a8a')); p.at(14, 0, 0.6, 12, () => k.M.heart(p, '#ff9ab0')); }),
    drg_calcloss: sym(1, 'refresh', '#9fdcff'),
    drg_firstaid: sym(1, 'plus', '#7dff9a'),
    drg_training: aug(1, (p, k) => { k.M.fist(p, '#ffb070', '#8a5a2a'); }),
    drg_duplicates: aug(1, (p, k) => { p.at(-10, 0, 0.55, -10, () => k.M.digivice(p, '#eef1f6', '#5aa0ff', '#9fdcff')); p.at(12, 4, 0.55, 10, () => k.M.digivice(p, '#eef1f6', '#5aa0ff', '#9fdcff')); }),
    drg_scraps: aug(1, (p, k) => { p.at(-14, 8, 0.45, -10, () => k.M.chip(p, '#ff7a45')); p.at(12, -6, 0.45, 12, () => k.M.chip(p, '#4fb3ff')); k.M.coin(p, 16, 22, 11, 0); }),
    drg_ascension: aug(2, (p, k) => { p.glow(0, -8, 40, '#fff0a0', 0.6); wing(p, k, -6, -4, 0.85, '#fff6c8'); wing(p, k, 6, -4, 0.85, '#fff6c8', true); k.SYM.up(p, '#ffd84a'); }),
    drg_celestial: sym(2, 'pearl', '#fff0a0'),
    drg_implants: aug(2, (p, k) => { k.M.chip(p, '#ff7a45', 50); p.at(0, 0, 0.5, 0, () => k.SYM.plus(p, '#ffffff')); }),
    drg_shell: aug(2, (p, k) => { k.M.chip(p, '#c0c6d0', 50); p.at(0, 0, 0.55, 0, () => k.SYM.shield(p, '#ffffff')); }),
    drg_uplink: aug(2, (p, k) => { k.M.chip(p, '#4fb3ff', 50); drop(p, k, 0, 0, 0.7, '#9fdcff'); }),
    drg_grabbag: aug(2, (p, k) => { p.part('M-30 -6C-30 -28 30 -28 30 -6L34 30C20 40 -20 40 -34 30Z', 'wood'); p.at(-10, -22, 0.3, -10, () => k.M.chip(p, '#ff7a45')); p.at(12, -24, 0.3, 10, () => k.M.chip(p, '#b26bff')); }),
    drg_archives: aug(2, (p, k) => { p.at(0, 4, 1, 0, () => k.M.card(p, '#8a5a2a', 56, 70)); p.at(0, 2, 0.5, 0, () => k.SYM.crestTag(p)); }),
    drg_imperialist: aug(2, (p, k) => { p.glow(0, 0, 40, '#ff6a3d', 0.6); scale(p, k, 0, 0, 2.2, '#ff6a3d'); flame(p, k, 0, 2, 0.9, '#ffcf4a'); }),
    drg_axiom: aug(2, (p, k) => { p.at(0, 0, 1, 0, () => k.SYM.dagger(p, '#9fdcff')); ring(p, k, 30, '#4fb3ff'); }),
    drg_bigfriend: aug(2, (p, k) => { p.at(-10, 4, 1.2, 0, () => k.SYM.shield(p, '#c0c6d0')); p.at(16, 14, 0.6, 0, () => k.M.heart(p, '#ff6a8a', 0.5)); }),
    drg_double: aug(2, (p, k) => { p.at(-10, 0, 0.9, 0, () => k.SYM.star5(p, '#ffd84a')); p.at(12, 6, 0.9, 0, () => k.SYM.star5(p, '#ffb02e')); }),
    drg_electro: aug(2, (p, k) => { bolt(p, k, -10, 0, 1.3, -10, '#ffe94a'); bolt(p, k, 14, 4, 1, 15, '#a8dcff'); }),
    drg_treasure_hoard: aug(2, (p, k) => { p.part(k.rr(-34, -4, 68, 38, 6), 'wood'); p.part('M-34 -2C-34 -24 -22 -30 0 -30C22 -30 34 -24 34 -2Z', 'gold'); p.glow(0, -12, 26, '#ffd84a', 0.8); scale(p, k, 0, 10, 0.8, '#ff6a3d'); }),
    drg_built: sym(2, 'chess', '#d0d6e8'),
    drg_crest_gold: aug(2, (p, k) => p.at(0, 0, 0.8, 0, () => { k.M.tag(p, '#fff3b0'); k.SYM.coin(p); })),
    drg_crest_rage: aug(2, (p, k) => p.at(0, 0, 0.8, 0, () => { k.M.tag(p, '#ffd0c0'); k.SYM.flame(p, '#ff5a3a'); })),
    drg_crest_tamer: aug(2, (p, k) => p.at(0, 0, 0.8, 0, () => { k.M.tag(p, '#ffe0f0'); k.SYM.up(p, '#ff8fc8'); })),
    drg_crest_mage: aug(2, (p, k) => p.at(0, 0, 0.8, 0, () => { k.M.tag(p, '#f0e0ff'); k.SYM.star6(p, '#d48bff'); })),
    drg_jackpot: aug(3, (p, k) => { p.glow(0, 0, 44, '#ffd84a', 0.6); coins(p, k, 5); }),
    drg_relics: aug(3, (p, k) => { p.glow(0, 0, 44, '#fff3a0', 0.6); p.gem(0, 0, 26, '#ff8fd8', 'hex', 6); p.sparkle(24, -26, 8); }),
    drg_crown: aug(3, (p, k) => p.at(0, 8, 0.9, 0, () => { p.part('M-36 10L-42 -26L-18 -6L0 -38L18 -6L42 -26L36 10Z', 'gold'); p.part(k.rr(-38, 6, 76, 18, 5), 'gold'); p.gem(0, 15, 7, '#ff6a3d', 'diamond', 4); })),
    drg_wings: aug(3, (p, k) => { p.glow(0, 0, 44, '#ff6a3d', 0.5); wing(p, k, -4, 0, 1, '#ff9a5a'); wing(p, k, 4, 0, 1, '#ff9a5a', true); scale(p, k, 0, 4, 1, '#ff6a3d'); }),
    drg_pact: aug(3, (p, k) => { p.glow(0, 0, 44, '#ff6a3d', 0.5); claw(p, k, -12, 2, 1.2, -16, '#ff6a3d'); claw(p, k, 12, 2, 1.2, 16, '#ffcf4a'); p.at(0, 30, 0.5, 0, () => k.SYM.plus(p, '#ffffff')); }),
    drg_final: aug(3, (p, k) => { p.glow(0, 0, 44, '#ff8fd8', 0.6); k.SYM.up(p, '#ff8fd8'); p.at(0, -24, 0.6, 0, () => k.SYM.up(p, '#ffffff')); }),
  };
  for (const id of Object.keys(A)) items[`aug:${id}`] = A[id];

  // ---------- move looks ----------
  // Each line: [elem, colors or null, look for Rookie, Champion, final]. Looks are built by LOOK[name](stage, colors).
  const S3 = (st, a, b, c) => [a, b, c][st];
  const LOOK = {
    orb: (st, o = {}) => ({ charge: 'mouth', impact: o.impact, shake: S3(st, 0.04, 0.08, 0.14), fx: [['orb', { size: S3(st, 0.22, 0.32, 0.44), speed: 12, arc: 0.2, trail: o.trail || 'flame', head: o.head, impactSize: S3(st, 1, 1.3, 1.6) }]] }),
    breath: (st, o = {}) => ({ charge: 'mouth', impact: o.impact, shake: S3(st, 0.06, 0.1, 0.18), fx: [['breath', { dur: S3(st, 0.35, 0.5, 0.7), width: S3(st, 0.3, 0.45, 0.65), cell: o.cell || 'flame', mix: o.mix, size: S3(st, 0.32, 0.42, 0.55), speed: 10, rate: 300 }]] }),
    beam: (st, o = {}) => ({ charge: 'hands', impact: o.impact, shake: S3(st, 0.05, 0.1, 0.18), fx: [['beam', { width: S3(st, 0.14, 0.22, 0.32), dur: S3(st, 0.3, 0.4, 0.5), charge: 0.1, glyph: o.glyph }]] }),
    bolt: (st) => ({ charge: 'sky', impact: 'zap', shake: S3(st, 0.05, 0.1, 0.16), fx: [['bolt', { from: st ? 'sky' : 'caster', count: S3(st, 1, 2, 3), width: 0.08 }]] }),
    slash: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'hit', shake: S3(st, 0.05, 0.1, 0.16), fx: [['slash', { count: S3(st, 2, 3, 4), size: S3(st, 1, 1.3, 1.7), claw: !!o.claw, spread: 0.5 }]] }),
    dash: (st, o = {}) => ({ charge: 'none', release: 0.12, impact: o.impact || 'hit', shake: S3(st, 0.06, 0.12, 0.2), fx: [['dash', { ghosts: S3(st, 3, 4, 6), slashes: S3(st, 2, 3, 4), claw: !!o.claw }]] }),
    erupt: (st, o = {}) => ({ charge: 'ground', impact: o.impact, shake: S3(st, 0.1, 0.16, 0.26), fx: [['erupt', { kind: o.kind || 'flame', to: 'center', area: true, count: S3(st, 3, 5, 7), height: S3(st, 1, 1.4, 1.9) }]] }),
    meteor: (st, o = {}) => ({ charge: 'sky', impact: o.impact || 'blast', shake: S3(st, 0.12, 0.2, 0.3), fx: [['meteor', { size: S3(st, 0.5, 0.75, 1.05), count: S3(st, 1, 2, 3), trail: o.trail || 'flame' }]] }),
    sphere: (st, o = {}) => ({ charge: 'hands', impact: o.impact, shake: S3(st, 0.08, 0.14, 0.22), fx: [['sphere', { size: S3(st, 0.4, 0.6, 0.85), grow: 0.3, from: 'hands', trail: o.trail || 'spark', cell: o.cell || 'glow', impactSize: S3(st, 1.2, 1.5, 1.9) }]] }),
    wave: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'hit', shake: S3(st, 0.06, 0.12, 0.2), fx: [['wave', { width: S3(st, 0.8, 1.1, 1.5), count: S3(st, 1, 2, 3), dur: 0.45 }]] }),
    arrows: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'holy', shake: S3(st, 0.05, 0.1, 0.16), fx: [['arrows', { count: S3(st, 3, 5, 8), size: S3(st, 0.6, 0.8, 1), cell: o.cell || 'streak', rain: st === 2 }]] }),
    cross: (st) => ({ charge: 'sky', impact: 'holy', shake: S3(st, 0.06, 0.12, 0.2), fx: [['cross', { size: S3(st, 1, 1.4, 1.9), orbs: S3(st, 2, 4, 6) }]] }),
    vortex: (st, o = {}) => ({ charge: 'ground', impact: o.impact || 'dark', shake: S3(st, 0.06, 0.12, 0.2), fx: [['vortex', { size: S3(st, 1, 1.4, 1.9), dur: 0.8, cell: o.cell || 'wisp', up: true }]] }),
    spikes: (st, o = {}) => ({ charge: 'ground', impact: o.impact || 'frost', shake: S3(st, 0.06, 0.12, 0.2), fx: [['spikes', { count: S3(st, 4, 6, 8), kind: o.kind || 'ice', size: S3(st, 0.6, 0.85, 1.1) }]] }),
    whip: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'poison', shake: S3(st, 0.04, 0.08, 0.14), fx: [['whip', { dur: 0.45, width: S3(st, 0.08, 0.11, 0.15), count: S3(st, 1, 2, 3), sway: 1.2, leafy: !!o.leafy, lightning: !!o.lightning }]] }),
    swarm: (st, o = {}) => ({ charge: 'aura', impact: o.impact || 'hit', shake: S3(st, 0.04, 0.08, 0.14), fx: [['swarm', { cell: o.cell || 'feather', count: S3(st, 8, 12, 18), size: S3(st, 0.3, 0.38, 0.48), speed: 9, wobble: 0.8, fan: true }]] }),
    shock: (st, o = {}) => ({ charge: 'ground', impact: o.impact || 'hit', shake: S3(st, 0.12, 0.2, 0.3), fx: [['shock', { size: S3(st, 1, 1.4, 1.9), hammer: !!o.hammer, debris: true, cell: o.cell }]] }),
    dome: (st, o = {}) => ({ charge: 'aura', impact: 'none', fx: [['dome', { size: S3(st, 1, 1.2, 1.4), orbit: o.cell, count: S3(st, 2, 4, 6), to: 'self' }]] }),
    heal: (st, o = {}) => ({ charge: 'sky', impact: 'none', fx: [['heal', { cell: o.cell || 'heart', size: S3(st, 0.8, 1, 1.25), motes: S3(st, 6, 10, 14) }]] }),
    tornado: (st, o = {}) => ({ charge: 'aura', impact: o.impact || 'wind', shake: S3(st, 0.06, 0.12, 0.2), fx: [['tornado', { cell: o.cell || 'feather', height: S3(st, 2, 2.6, 3.4), size: S3(st, 0.8, 1, 1.3) }]] }),
    rings: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'wind', shake: S3(st, 0.04, 0.08, 0.14), fx: [['rings', { count: S3(st, 2, 3, 5), size: S3(st, 0.5, 0.7, 0.9) }]] }),
    volley: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'metal', shake: S3(st, 0.04, 0.08, 0.14), fx: [['volley', { count: S3(st, 8, 14, 22), rate: 0.03, cell: o.cell || 'streak', size: 0.4, spread: 0.35, speed: 22 }]] }),
    missiles: (st, o = {}) => ({ charge: 'back', impact: o.impact || 'blast', shake: S3(st, 0.08, 0.16, 0.26), fx: [['missiles', { count: S3(st, 2, 4, 6), size: 0.16, curve: 1.2, arc: 1.2, dur: 0.6, smoke: true }]] }),
    streak: (st, o = {}) => ({ charge: 'none', release: 0.12, impact: o.impact || 'hit', shake: S3(st, 0.08, 0.14, 0.22), fx: [['streak', { width: S3(st, 0.5, 0.7, 0.9), dur: 0.25, trail: o.trail || 'wind' }], ['slash', { at: 0.1, count: S3(st, 1, 2, 3), size: 1.3 }]] }),
    wheel: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'hit', shake: S3(st, 0.06, 0.1, 0.16), fx: [['wheel', { size: S3(st, 0.6, 0.8, 1.05), cell: o.cell || 'blade', dur: 0.5, saw: !!o.saw }]] }),
    pillar: (st, o = {}) => ({ charge: 'sky', impact: o.impact || 'holy', shake: S3(st, 0.08, 0.14, 0.24), fx: [['pillar', { size: S3(st, 0.8, 1.1, 1.5), height: 6, dur: 0.6 }]] }),
    drainfx: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'dark', shake: S3(st, 0.04, 0.08, 0.12), fx: [['orb', { size: S3(st, 0.22, 0.3, 0.4), speed: 12, trail: o.trail || 'dark' }], ['drain', { at: 0.3 }]] }),
    glyphbeam: (st, o = {}) => ({ charge: 'hands', impact: o.impact || 'holy', shake: S3(st, 0.08, 0.14, 0.22), fx: [['glyph', { glyph: o.glyph || 'circle', size: S3(st, 1, 1.3, 1.6), dur: 0.6 }], ['beam', { width: S3(st, 0.18, 0.26, 0.36), dur: 0.45, charge: 0.12 }]] }),
    net: (st) => ({ charge: 'hands', impact: 'poison', fx: [['net', { size: S3(st, 0.8, 1, 1.3) }]] }),
    portal: (st) => ({ charge: 'aura', impact: 'dark', shake: S3(st, 0.06, 0.12, 0.2), fx: [['portal', { size: S3(st, 1, 1.3, 1.7), dur: 0.8 }]] }),
  };
  // look names: plain, or 'name:opt=value,opt=value'
  const IMPACT = { fire: 'blast', ice: 'frost', thunder: 'zap', water: 'splash', plant: 'poison', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'metal', beast: 'hit' };
  const TRAIL = { fire: 'flame', ice: 'ice', thunder: 'zap', water: 'bubble', plant: 'leaf', wind: 'wind', holy: 'holy', dark: 'dark', metal: 'smoke', beast: 'spark' };
  function make(elem, colors, look, st) {
    const [name, args] = look.split(':');
    const o = { impact: IMPACT[elem], trail: TRAIL[elem] };
    for (const kv of (args || '').split(',').filter(Boolean)) { const [k, v] = kv.split('='); o[k] = v === 'true' ? true : v; }
    const s = { elem, release: 0.28, ...LOOK[name](st, o) };
    if (!s.impact) s.impact = IMPACT[elem];
    if (colors) s.colors = colors;
    return s;
  }
  const MOVE_TABLE = {
    drg_gazimon: ['thunder', null, ['bolt', 'streak:trail=dark', 'pillar:impact=holy']],
    drg_gotsumon: ['metal', ['#b8a890', '#fff0d0'], ['shock:cell=shard', 'spikes:kind=ice', 'meteor:trail=smoke']],
    drg_patamon: ['holy', null, ['orb:trail=wind', 'beam', 'missiles:impact=holy']],
    drg_crabmon: ['water', null, ['slash:claw=true', 'orb:head=shard,trail=ice', 'shock:hammer=true,cell=snow']],
    drg_kamemon: ['metal', ['#ffd84a', '#fff6c8'], ['dome', 'missiles', 'shock:hammer=true']],
    drg_lunamon: ['water', ['#c8a8ff', '#ffffff'], ['dash:claw=true', 'arrows:cell=shard', 'arrows:cell=streak']],
    drg_elecmon: ['thunder', null, ['bolt', 'streak:trail=zap', 'shock:cell=spark']],
    drg_impmon: ['fire', ['#ff4a1a', '#2a0a1a'], ['orb', 'volley:cell=spark', 'beam:impact=dark']],
    drg_gaomon: ['beast', ['#4a8aff', '#ffffff'], ['slash', 'shock', 'streak:trail=flame']],
    drg_fanbeemon: ['plant', ['#ffd23f', '#3a2a10'], ['dome:cell=needle', 'swarm:cell=petal', 'dome:cell=needle']],
    drg_syakomon: ['water', ['#3a2a6a', '#e0d8ff'], ['orb:head=bubble,trail=bubble', 'shock:cell=bubble', 'erupt:kind=water']],
    drg_dracmon: ['dark', null, ['drainfx', 'drainfx:trail=ice,impact=frost', 'drainfx:impact=hit']],
    drg_armadillomon: ['beast', ['#ffb04a', '#fff0c0'], ['slash:claw=true', 'shock:cell=ember', 'streak']],
    drg_gabumon: ['ice', ['#3a8bff', '#bfe8ff'], ['orb:head=shard', 'volley:cell=flame,impact=blast', 'arrows:cell=shard,impact=frost']],
    drg_gomamon: ['water', null, ['dome:cell=fish', 'dome:cell=bubble', 'dome:cell=snow']],
    drg_blackagumon: ['fire', ['#ff6a1a', '#3a1a10'], ['shock', 'shock:hammer=true', 'tornado:cell=flame,impact=blast']],
    drg_chuumon: ['beast', ['#c8a060', '#fff0c0'], ['orb:head=dot,trail=smoke', 'wave', 'shock:hammer=true']],
    drg_betamon: ['thunder', ['#4ad0ff', '#ffffff'], ['orb:head=bubble', 'volley:cell=snow,impact=frost', 'volley:cell=bolt,impact=zap']],
    drg_tsukaimon: ['fire', ['#ff8a3a', '#fff0c0'], ['meteor:trail=smoke', 'meteor', 'erupt:kind=flame']],
    drg_salamon: ['holy', null, ['rings', 'beam:impact=zap', 'arrows:cell=streak']],
    drg_biyomon: ['wind', null, ['wave:impact=wind', 'volley:cell=needle,impact=hit', 'streak:trail=dark']],
    drg_monodramon: ['fire', null, ['dome:cell=ember', 'dome:cell=flame', 'dome:cell=cross']],
    drg_mushroomon: ['plant', ['#a04aff', '#d0ff8a'], ['meteor:trail=leaf,impact=poison', 'breath:cell=smoke', 'erupt:kind=thorn']],
    drg_kotemon: ['fire', ['#ff6a3a', '#ffffff'], ['slash', 'wave', 'glyphbeam:glyph=x,impact=hit']],
    drg_otamamon: ['water', ['#3aa0ff', '#d0ffd8'], ['shock:cell=bubble', 'shock:cell=note', 'shock:cell=note,hammer=true']],
    drg_aruraumon: ['plant', ['#a04aff', '#ff9ad8'], ['whip:leafy=true', 'swarm:cell=leaf,impact=poison', 'meteor:trail=petal,impact=poison']],
    drg_renamon: ['holy', ['#ffd84a', '#ffffff'], ['dash', 'wheel:cell=flame,impact=blast', 'dash:impact=holy']],
    drg_veemon: ['fire', ['#4a8aff', '#ff8a2a'], ['streak', 'streak:trail=flame,impact=blast', 'sphere:trail=flame,cell=flame,impact=blast']],
    drg_lalamon: ['plant', ['#ff9ad8', '#fff6c0'], ['dome:cell=note', 'dome:cell=petal', 'dome:cell=leaf']],
    drg_snowgoblimon: ['ice', null, ['bolt', 'shock:hammer=true,cell=snow', 'arrows:cell=shard,impact=hit']],
    drg_blackgabumon: ['ice', ['#5a6aff', '#d0e0ff'], ['drainfx:trail=ice,impact=frost', 'drainfx:trail=ice,impact=frost', 'drainfx:trail=ice,impact=frost']],
    drg_hawkmon: ['wind', null, ['dome:cell=feather', 'dome:cell=feather', 'dome:cell=ring']],
    drg_penguinmon: ['water', ['#5ac8ff', '#ffffff'], ['heal:cell=snow', 'heal:cell=drop', 'heal:cell=heart']],
    drg_psychemon: ['fire', ['#ff8ad8', '#ffe08a'], ['dome:cell=spark', 'dome:cell=flame', 'dome:cell=ember']],
    drg_kudamon: ['holy', ['#ffd84a', '#ffffff'], ['beam', 'beam:impact=blast', 'beam:glyph=tri']],
    drg_gumdramon: ['thunder', ['#a04aff', '#ffd84a'], ['beam', 'beam', 'beam:glyph=x']],
    drg_kokuwamon: ['thunder', ['#c0c6d0', '#ffe94a'], ['volley:cell=bolt,impact=zap', 'volley', 'volley:cell=streak']],
    drg_toyagumon: ['fire', ['#ff4a4a', '#ffd84a'], ['volley:cell=square,impact=blast', 'volley:cell=streak', 'missiles']],
    drg_dorumon: ['metal', ['#a04aff', '#ffffff'], ['dash', 'dash:impact=blast', 'dash:impact=holy']],
    drg_dracomon: ['fire', ['#ff8a2a', '#fff0c0'], ['breath', 'shock:hammer=true', 'meteor:trail=smoke']],
    drg_tentomon: ['thunder', null, ['bolt', 'beam:impact=metal', 'glyphbeam:glyph=tri,impact=zap']],
    drg_guilmon: ['fire', ['#ff3a2a', '#ffffff'], ['streak:trail=flame', 'beam:impact=holy', 'wave:impact=blast']],
    drg_coronamon: ['fire', ['#ffd84a', '#ff6a1a'], ['dome:cell=flame', 'dome:cell=flame', 'dome:cell=cross']],
    drg_hackmon: ['fire', ['#ff4a3a', '#ffffff'], ['streak:trail=flame', 'meteor', 'glyphbeam:glyph=x,impact=dark']],
    drg_gizamon: ['water', null, ['wheel:saw=true', 'dash:impact=dark', 'whip:impact=splash']],
    drg_goblimon: ['dark', ['#5a9a3a', '#ffd84a'], ['shock', 'wave:impact=hit', 'wave:impact=dark']],
    drg_ryudamon: ['metal', ['#d8e4f4', '#ffffff'], ['slash', 'slash:impact=frost', 'streak:trail=holy,impact=holy']],
    drg_coredramon: ['water', ['#3a8bff', '#e0f4ff'], ['breath:cell=flame', 'meteor:trail=flame', 'erupt:kind=water']],
    drg_dorugamon: ['metal', ['#a04aff', '#ffd84a'], ['streak:trail=smoke', 'meteor:trail=smoke,impact=metal', 'streak:trail=holy,impact=metal']],
    drg_hyokomon: ['fire', ['#ffb02e', '#ffffff'], ['volley:cell=feather,impact=hit', 'volley:cell=flame,impact=blast', 'volley:cell=ring,impact=wind']],
    drg_jazardmon: ['metal', ['#ff8a2a', '#e0e8f4'], ['volley:cell=streak', 'volley:cell=blade', 'volley:cell=bolt,impact=zap']],
    drg_growlmon: ['holy', ['#ffd84a', '#ffffff'], ['slash', 'breath:cell=glow', 'pillar']],
    drg_baohuckmon: ['holy', ['#ffffff', '#5ad6ff'], ['slash', 'glyphbeam:glyph=circle', 'cross']],
    drg_bakemon: ['dark', null, ['whip', 'slash:impact=dark', 'portal']],
    drg_greymon: ['fire', null, ['shock:hammer=true', 'slash:impact=metal', 'erupt:kind=flame']],
    drg_leomon: ['holy', ['#ffd84a', '#ffffff'], ['heal:cell=star', 'heal:cell=star', 'heal:cell=cross']],
    drg_zubamon: ['metal', ['#ffb04a', '#ffffff'], ['dash', 'dash', 'dash:impact=holy']],
    drg_clockmon: ['metal', ['#d8c8a0', '#a08cff'], ['glyphbeam:glyph=circle,impact=metal', 'missiles', 'portal']],
    drg_ankylomon: ['metal', ['#b8a890', '#ff8a2a'], ['shock:hammer=true', 'erupt:kind=flame', 'shock:cell=shard']],
    drg_volcamon: ['fire', ['#ff6a1a', '#ffd84a'], ['wave:impact=blast', 'streak:trail=flame,impact=blast', 'wave:impact=metal']],
    // wild
    drg_w_commandramon: ['metal', null, ['volley', 'volley', 'volley']],
    drg_w_sealsdramon: ['metal', null, ['dash', 'dash', 'dash']],
    drg_w_devidramon: ['dark', ['#c01a1a', '#ff8a2a'], ['slash:claw=true', 'slash:claw=true', 'slash:claw=true']],
    drg_w_tyrannomon: ['fire', null, ['breath', 'breath', 'breath']],
    drg_w_gigadramon: ['metal', null, ['missiles', 'missiles', 'missiles']],
    drg_w_metaltyrannomon: ['metal', null, ['shock:hammer=true', 'shock:hammer=true', 'shock:hammer=true']],
    drg_w_goldveedramon: ['holy', ['#ffd84a', '#ffffff'], ['breath:cell=glow', 'breath:cell=glow', 'breath:cell=glow']],
    drg_w_magnadramon: ['fire', null, ['tornado:cell=flame,impact=blast', 'tornado:cell=flame,impact=blast', 'tornado:cell=flame,impact=blast']],
    drg_w_megidramon: ['dark', ['#ff3a1a', '#3a0a0a'], ['erupt:kind=dark', 'erupt:kind=dark', 'erupt:kind=dark']],
    drg_w_chaosdromon: ['metal', ['#ff4a2a', '#e0e8f4'], ['shock:hammer=true', 'shock:hammer=true', 'shock:hammer=true']],
    drg_w_blackwargreymon: ['dark', null, ['streak:trail=dark,impact=dark', 'streak:trail=dark,impact=dark', 'streak:trail=dark,impact=dark']],
    drg_w_imperialdramon: ['holy', null, ['sphere:cell=glow,trail=holy', 'sphere:cell=glow,trail=holy', 'sphere:cell=glow,trail=holy']],
    drg_w_azulongmon: ['thunder', ['#5ad6ff', '#ffffff'], ['bolt', 'bolt', 'bolt']],
    drg_stone: ['metal', null, ['dome', 'dome', 'dome']],
    drg_voidling: ['dark', null, ['orb', 'orb', 'orb']],
  };

  // basic attacks: melee kind per line, or a shot for ranged lines
  const BASIC_TABLE = {
    drg_gazimon: { kind: 'shot', elem: 'thunder', head: 'bolt', trail: 'zap', size: 0.2 },
    drg_gotsumon: { kind: 'shot', elem: 'metal', head: 'shard', trail: 'smoke', size: 0.24, arc: 0.4 },
    drg_patamon: { kind: 'kick', elem: 'holy' },
    drg_crabmon: { kind: 'claw', elem: 'water' },
    drg_kamemon: { kind: 'punch', elem: 'metal', colors: ['#ffd84a', '#ffffff'] },
    drg_lunamon: { kind: 'claw', elem: 'water', colors: ['#c8a8ff', '#ffffff'] },
    drg_elecmon: { kind: 'tail', elem: 'thunder' },
    drg_impmon: { kind: 'shot', elem: 'fire', head: 'ember', trail: 'flame', size: 0.22 },
    drg_gaomon: { kind: 'punch', elem: 'beast', colors: ['#4a8aff', '#ffffff'] },
    drg_fanbeemon: { kind: 'sting', elem: 'plant', colors: ['#ffd23f', '#3a2a10'] },
    drg_syakomon: { kind: 'shot', elem: 'water', head: 'bubble', trail: 'bubble', size: 0.24 },
    drg_dracmon: { kind: 'shot', elem: 'dark', head: 'bat', trail: 'dark', size: 0.24 },
    drg_armadillomon: { kind: 'horn', elem: 'beast' },
    drg_gabumon: { kind: 'shot', elem: 'ice', head: 'shard', trail: 'ice', size: 0.22 },
    drg_gomamon: { kind: 'claw', elem: 'water' },
    drg_blackagumon: { kind: 'claw', elem: 'fire', colors: ['#ff6a1a', '#3a1a10'] },
    drg_chuumon: { kind: 'claw', elem: 'beast' },
    drg_betamon: { kind: 'shot', elem: 'thunder', head: 'spark', trail: 'zap', size: 0.22 },
    drg_tsukaimon: { kind: 'kick', elem: 'fire' },
    drg_salamon: { kind: 'shot', elem: 'holy', head: 'paw', trail: 'holy', size: 0.22 },
    drg_biyomon: { kind: 'peck', elem: 'wind' },
    drg_monodramon: { kind: 'bite', elem: 'fire' },
    drg_mushroomon: { kind: 'shot', elem: 'plant', head: 'seed', trail: 'leaf', size: 0.22, arc: 0.5 },
    drg_kotemon: { kind: 'slash', elem: 'fire', colors: ['#ff6a3a', '#ffffff'] },
    drg_otamamon: { kind: 'tail', elem: 'water' },
    drg_aruraumon: { kind: 'shot', elem: 'plant', head: 'petal', trail: 'petal', size: 0.22 },
    drg_renamon: { kind: 'kick', elem: 'holy', colors: ['#ffd84a', '#ffffff'] },
    drg_veemon: { kind: 'punch', elem: 'fire', colors: ['#4a8aff', '#ffffff'] },
    drg_lalamon: { kind: 'shot', elem: 'plant', head: 'note', trail: 'note', size: 0.22 },
    drg_snowgoblimon: { kind: 'punch', elem: 'ice' },
    drg_blackgabumon: { kind: 'claw', elem: 'ice', colors: ['#5a6aff', '#d0e0ff'] },
    drg_hawkmon: { kind: 'peck', elem: 'wind' },
    drg_penguinmon: { kind: 'shot', elem: 'ice', head: 'snow', trail: 'ice', size: 0.22 },
    drg_psychemon: { kind: 'punch', elem: 'fire', colors: ['#ff8ad8', '#ffe08a'] },
    drg_kudamon: { kind: 'shot', elem: 'holy', head: 'star', trail: 'holy', size: 0.22 },
    drg_gumdramon: { kind: 'tail', elem: 'thunder', colors: ['#a04aff', '#ffd84a'] },
    drg_kokuwamon: { kind: 'shot', elem: 'thunder', head: 'bolt', trail: 'zap', size: 0.2, speed: 20 },
    drg_toyagumon: { kind: 'shot', elem: 'fire', head: 'square', trail: 'spark', size: 0.22 },
    drg_dorumon: { kind: 'bite', elem: 'metal', colors: ['#a04aff', '#ffffff'] },
    drg_dracomon: { kind: 'shot', elem: 'fire', head: 'flame', trail: 'flame', size: 0.3, arc: 0.2 },
    drg_tentomon: { kind: 'shot', elem: 'thunder', head: 'spark', trail: 'zap', size: 0.22 },
    drg_guilmon: { kind: 'slash', elem: 'holy', colors: ['#ff3a2a', '#ffffff'] },
    drg_coronamon: { kind: 'claw', elem: 'fire', colors: ['#ffd84a', '#ff6a1a'] },
    drg_hackmon: { kind: 'claw', elem: 'fire', colors: ['#ff4a3a', '#ffffff'] },
    drg_gizamon: { kind: 'claw', elem: 'water' },
    drg_goblimon: { kind: 'punch', elem: 'dark', colors: ['#5a9a3a', '#ffd84a'] },
    drg_ryudamon: { kind: 'slash', elem: 'metal', colors: ['#d8e4f4', '#ffffff'] },
    drg_coredramon: { kind: 'shot', elem: 'water', head: 'flame', trail: 'bubble', size: 0.3, arc: 0.15 },
    drg_dorugamon: { kind: 'bite', elem: 'metal', colors: ['#a04aff', '#ffd84a'] },
    drg_hyokomon: { kind: 'shot', elem: 'wind', head: 'feather', trail: 'wind', size: 0.24 },
    drg_jazardmon: { kind: 'shot', elem: 'metal', head: 'streak', trail: 'smoke', size: 0.26, speed: 22 },
    drg_growlmon: { kind: 'shot', elem: 'holy', head: 'star', trail: 'holy', size: 0.3 },
    drg_baohuckmon: { kind: 'shot', elem: 'holy', head: 'cross', trail: 'holy', size: 0.24 },
    drg_bakemon: { kind: 'claw', elem: 'dark' },
    drg_greymon: { kind: 'bite', elem: 'fire' },
    drg_leomon: { kind: 'shot', elem: 'holy', head: 'star', trail: 'holy', size: 0.22 },
    drg_zubamon: { kind: 'slash', elem: 'metal', colors: ['#ffb04a', '#ffffff'] },
    drg_clockmon: { kind: 'shot', elem: 'metal', head: 'gear', trail: 'spark', size: 0.24, spin: 6 },
    drg_ankylomon: { kind: 'tail', elem: 'metal', colors: ['#b8a890', '#ff8a2a'] },
    drg_volcamon: { kind: 'punch', elem: 'fire', colors: ['#ff6a1a', '#ffd84a'] },
    drg_w_commandramon: { kind: 'shot', elem: 'metal', head: 'streak', trail: 'smoke', size: 0.18, speed: 22 },
    drg_w_sealsdramon: { kind: 'slash', elem: 'metal' },
    drg_w_devidramon: { kind: 'claw', elem: 'dark' },
    drg_w_tyrannomon: { kind: 'bite', elem: 'fire' },
    drg_w_gigadramon: { kind: 'shot', elem: 'metal', head: 'streak', trail: 'smoke', size: 0.26 },
    drg_w_metaltyrannomon: { kind: 'bite', elem: 'metal' },
    drg_w_goldveedramon: { kind: 'punch', elem: 'holy', colors: ['#ffd84a', '#ffffff'] },
    drg_w_magnadramon: { kind: 'shot', elem: 'fire', head: 'flame', trail: 'flame', size: 0.28 },
    drg_w_megidramon: { kind: 'claw', elem: 'dark', colors: ['#ff3a1a', '#3a0a0a'] },
    drg_w_chaosdromon: { kind: 'punch', elem: 'metal' },
    drg_w_blackwargreymon: { kind: 'claw', elem: 'dark' },
    drg_w_imperialdramon: { kind: 'shot', elem: 'holy', head: 'glow', trail: 'holy', size: 0.3 },
    drg_w_azulongmon: { kind: 'shot', elem: 'thunder', head: 'bolt', trail: 'zap', size: 0.26 },
    drg_stone: { kind: 'punch', elem: 'metal' },
    drg_voidling: { kind: 'bite', elem: 'dark' },
  };

  // ability names come from the registered lines, so the table can never drift from the data
  const moves = {};
  const def = DT.Sets.def('dragonlands');
  for (const l of (def && def.lines) || []) {
    const row = MOVE_TABLE[l.id];
    if (!row) continue;
    const [elem, colors, looks] = row;
    l.abil.forEach((name, st) => { if (!moves[name]) moves[name] = make(elem, colors, looks[st], st); });
  }

  DT.Sets.extend('dragonlands', { art: { prerendered: false, items, moves, basic: BASIC_TABLE } });
})(typeof window !== 'undefined' ? window : globalThis);
