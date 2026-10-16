// Reckoning: TFT Set 5 "Reckoning" and Set 5.5 "Dawn of Heroes" with Digimon. 60 shop lines (13/13/13/11/10),
// 27 traits (15 origins, 12 classes), 8 components plus 8 Shadow components (Dark Spore) and the Digi-Egg,
// 36 completed items, each with a Shadow variant (stronger, with a drawback) and a Radiant upgrade, 24 Crests,
// 24 augments. Native pace Hyper Roll (Shadow carousel at 3-1, Radiant Armory at 4-1, Radiant Blessing at 8 HP); the
// normal pace (rules.normal) adds the Armory, the Radiant Armory at 3-6 and the Blessing at 40 HP. Behavior (traits, items, augments, the mechanic) is in reckoning-mech.js.
// Corpus: docs/inspo/tft/set-05-reckoning.md and set-05-5-dawn-of-heroes.md. Design notes: docs/sets/reckoning.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;

  // ---------- traits ----------
  // lv[i] is the value at breakpoint i; extra per-breakpoint arrays are read with c.val(key).
  const TRAITS = {
    // ---- origins ----
    DSP: { name: 'Dark Spore', kind: 'origin', color: '#8a3cff', bp: [2, 4, 6], lv: [0.15, 0.3, 0.55], src: 'Forgotten',
      desc: 'Dark Spore Digimon gain attack damage and ability power. Each Shadow item held by a Dark Spore Digimon makes the bonus 20% bigger, up to 4 items.',
      fmt: (v) => `+${pct(v)} AD and +${Math.round(v * 100)} AP, +20% per Shadow item` },
    TRK: { name: 'Tricksters', kind: 'origin', color: '#ffb84a', bp: [3, 5, 7], lv: [0.1, 0.25, 0.55], src: 'Hellion',
      desc: 'Tricksters gain attack speed. When a Trickster is knocked out, a copy one form lower jumps in with 55% HP. Copies do not copy again.',
      fmt: (v) => `+${pct(v)} AS, copies on knockout` },
    SER: { name: 'Seraphic', kind: 'origin', color: '#fff0a0', bp: [3, 5, 7], lv: [14, 28, 50], src: 'Redeemed',
      desc: 'Seraphic Digimon gain armor, magic resist and ability power. When one is knocked out, its bonus is split among the Seraphic Digimon still standing.',
      fmt: (v) => `+${v} armor, MR and AP` },
    NGT: { name: 'Nightfall', kind: 'origin', color: '#5a4bd6', bp: [2, 4, 6], lv: [0.25, 0.45, 0.7], dmg: [0.15, 0.25, 0.35], src: 'Nightbringer',
      desc: 'The first time a Nightfall Digimon drops below 50% HP, it gains a shield for 8 seconds and deals more damage for the rest of combat.',
      fmt: (v, i) => `${pct(v)} HP shield, +${pct(TRAITS.NGT.dmg[i])} damage` },
    DAY: { name: 'Daybreak', kind: 'origin', color: '#ffcf4a', bp: [2, 4, 6], lv: [0.2, 0.3, 0.5], src: 'Dawnbringer',
      desc: 'The first time a Daybreak Digimon drops below 50% HP, it quickly heals part of its max HP. Each time this happens, every Daybreak Digimon deals 8% more damage.',
      fmt: (v) => `heal ${pct(v)} HP, team Daybreak +8% damage` },
    CHI: { name: 'Chimera', kind: 'origin', color: '#7fd04a', bp: [3, 4, 5], lv: [1200, 1800, 2500], ad: [100, 150, 200], src: 'Abomination',
      desc: 'When 3 of your Digimon have been knocked out, Kimeramon rises where the third fell. It copies one random item from up to 3 Chimera Digimon and grows with their forms.',
      fmt: (v, i) => `Kimeramon: ${v} HP, ${TRAITS.CHI.ad[i]} AD` },
    WIT: { name: 'Witchelny', kind: 'origin', color: '#c46bff', bp: [3], lv: [80], src: 'Coven',
      desc: 'At the start of combat, the Digimon closest to the center of your Witchelny Digimon becomes the Coven Leader and gains ability power. Each time a Witchelny Digimon casts, the Leader gains 30% of that ability\'s mana cost.',
      fmt: (v) => `Leader +${v} AP, 30% mana share` },
    DRK: { name: 'Dragon Kin', kind: 'origin', color: '#ff7a3a', bp: [3, 5], lv: [200, 400], egg: [1, 2], src: 'Draconic',
      desc: 'Dragon Kin Digimon gain max HP. After each player combat you get a Digi-Egg that hatches into loot after 1 to 3 rounds; bigger eggs take longer and hold better loot. With 5, the eggs are golden and hold more.',
      fmt: (v, i) => `+${v} HP, ${i ? 'golden ' : ''}egg after each PvP combat` },
    HNT: { name: 'Dragon Hunter', kind: 'origin', color: '#d9573a', bp: [2, 4], lv: [30, 70], team: [20, 50], src: 'Dragonslayer',
      desc: 'Dragon Hunters gain ability power. The first time a Dragon Hunter knocks out an enemy with 1500 or more max HP, your whole team gains ability power for the rest of combat.',
      fmt: (v, i) => `+${v} AP, team +${TRAITS.HNT.team[i]} AP after a big kill` },
    IRN: { name: 'Chrome Digizoid', kind: 'origin', color: '#b9c4d4', bp: [2, 3], lv: [55, 120], src: 'Ironclad',
      desc: 'Your whole team gains armor.', fmt: (v) => `team +${v} armor` },
    PHA: { name: 'Phantom', kind: 'origin', color: '#6fd6c8', bp: [2, 3, 4], lv: [0.06, 0.14, 0.28], src: 'Revenant',
      desc: 'The first time a Phantom Digimon would be knocked out, it rises again with part of its max HP. After rising, it deals 20% and takes 30% more damage.',
      fmt: (v) => `rise with ${pct(v)} HP` },
    SEN: { name: 'Sentinel', kind: 'origin', color: '#9fe6ff', bp: [3, 5], lv: [120, 350], as: [0.1, 0.2], dur: [4, 3], src: 'Sentinel (5.5)',
      desc: 'At the start of combat, the Sentinel holding the most items gains a shield that grants attack speed. When the shield breaks or runs out (at most once per duration), it jumps to the ally with the lowest HP and renews, and its attack speed grows each jump (up to 5 times).',
      fmt: (v, i) => `${v} shield, +${pct(TRAITS.SEN.as[i])} AS per jump, ${TRAITS.SEN.dur[i]}s` },
    RIV: { name: 'Royal Rival', kind: 'origin', color: '#e8d27a', bp: [1], lv: [0.2], src: 'God-King',
      desc: 'Gallantmon and Megidramon are rivals. A Royal Rival deals 20% more damage to enemies with one of its rival traits, but only while you field exactly one of them. Gallantmon\'s rivals: Dark Spore, Nightfall, Witchelny, Tricksters, Dragon Hunter, Chimera, Phantom. Megidramon\'s rivals: Seraphic, Daybreak, Dragon Kin, Chrome Digizoid, Sentinel.',
      fmt: (v) => `+${pct(v)} damage vs rivals (exactly 1)` },
    ETR: { name: 'Twin Souls', kind: 'origin', color: '#ffd6f0', bp: [1], lv: [1], src: 'Eternal',
      desc: 'Sakuyamon fights with a partner: at the start of combat, Garurumon appears next to it at the same form. Garurumon does not use a team slot.',
      fmt: () => 'Garurumon fights beside it' },
    INA: { name: 'Hallowed Mist', kind: 'origin', color: '#cfd8ff', bp: [1], lv: [0.33], src: 'Inanimate (5.5)',
      desc: 'At the start of combat, this Digimon and the allies next to it take 33% less damage for 8 seconds.',
      fmt: (v) => `${pct(v)} less damage for 8s nearby` },
    // ---- classes ----
    ASN: { name: 'Assassin', kind: 'class', color: '#ff4f7b', bp: [2, 4], lv: [0.1, 0.25], cd: [0.15, 0.35], src: 'Assassin',
      desc: 'Assassins leap to the enemy backline when combat starts. Their abilities can critically strike, and they gain critical strike chance and damage.',
      fmt: (v, i) => `+${pct(v)} crit, +${pct(TRAITS.ASN.cd[i])} crit damage` },
    BRW: { name: 'Brawler', kind: 'class', color: '#d9773a', bp: [2, 4], lv: [250, 650], src: 'Brawler',
      desc: 'Brawlers gain max HP.', fmt: (v) => `+${v} HP` },
    CAV: { name: 'Cavalier', kind: 'class', color: '#c9a66b', bp: [2, 3, 4], lv: [0.1, 0.14, 0.18], src: 'Cavalier',
      desc: 'Cavaliers take less damage. For the first 4 seconds of combat and for 3 seconds after each ability, the reduction is doubled.',
      fmt: (v) => `-${pct(v)} damage taken, doubled at times` },
    INV: { name: 'Invoker', kind: 'class', color: '#7fb6ff', bp: [2, 4], lv: [3, 6], src: 'Invoker',
      desc: 'All your Digimon gain extra mana from their basic attacks.', fmt: (v) => `team +${v} mana per attack` },
    KNT: { name: 'Knight', kind: 'class', color: '#a8b8d0', bp: [2, 4, 6], lv: [15, 30, 50], src: 'Knight',
      desc: 'All your Digimon block a flat amount of damage from every hit (burns are not blocked).', fmt: (v) => `team blocks ${v} per hit` },
    LEG: { name: 'Legionnaire', kind: 'class', color: '#ff8f5a', bp: [2, 4, 6], lv: [0.2, 0.45, 0.8], src: 'Legionnaire',
      desc: 'Legionnaires gain attack speed. Their first basic attack after an ability heals them for 50% of its damage.',
      fmt: (v) => `+${pct(v)} AS` },
    MYS: { name: 'Mystic', kind: 'class', color: '#d48bff', bp: [2, 3, 4], lv: [20, 40, 70], src: 'Mystic',
      desc: 'All your Digimon gain magic resist.', fmt: (v) => `team +${v} MR` },
    RNG: { name: 'Ranger', kind: 'class', color: '#5ad1ff', bp: [2, 4], lv: [0.7, 1.5], src: 'Ranger',
      desc: 'Starting 4 seconds into combat, Rangers gain a burst of attack speed for 4 seconds, every 8 seconds.',
      fmt: (v) => `+${pct(v)} AS in bursts` },
    REN: { name: 'Renewer', kind: 'class', color: '#9cf6c0', bp: [2, 4], lv: [0.025, 0.05], src: 'Renewer',
      desc: 'Renewers heal a share of their max HP every second. At full HP they gain that share of their mana instead.',
      fmt: (v) => `${pct(v)} HP or mana per second` },
    SKR: { name: 'Skirmisher', kind: 'class', color: '#f4d35e', bp: [3, 6], lv: [0.25, 0.45], ad: [3, 6], src: 'Skirmisher',
      desc: 'Skirmishers start combat with a shield and gain attack damage every second.',
      fmt: (v, i) => `${pct(v)} HP shield, +${TRAITS.SKR.ad[i]} AD per second` },
    SPW: { name: 'Spellweaver', kind: 'class', color: '#b26bff', bp: [2, 4], lv: [30, 65], per: [4, 7], src: 'Spellweaver',
      desc: 'Spellweavers gain ability power, and more each time any Digimon casts an ability, up to 10 times.',
      fmt: (v, i) => `+${v} AP, +${TRAITS.SPW.per[i]} per cast` },
    CAN: { name: 'Cannoneer', kind: 'class', color: '#ff6a4a', bp: [2, 4], lv: [2, 4.5], src: 'Cannoneer (5.5)',
      desc: 'Every 5th basic attack of a Cannoneer also fires a cannon shot that deals a share of that attack\'s damage to the target and enemies next to it.',
      fmt: (v) => `every 5th attack: ${Math.round(v * 100)}% splash` },
  };
  // codex text per breakpoint, built from the numbers so tuning never leaves stale text
  {
    const T = TRAITS;
    const each = (id, f) => { T[id].bpDesc = T[id].lv.map((v, i) => f(v, i, T[id])); };
    each('DSP', (v) => `+${pct(v)} AD and +${Math.round(v * 100)} AP for Dark Spore Digimon; each Shadow item they hold adds 20% to this (up to 4)`);
    each('TRK', (v) => `Tricksters gain ${pct(v)} attack speed; a knocked-out Trickster sends in a copy one form lower`);
    each('SER', (v) => `Seraphic Digimon gain ${v} armor, magic resist and ability power`);
    each('NGT', (v, i, t) => `Below 50% HP once: a ${pct(v)} max HP shield for 8s and ${pct(t.dmg[i])} more damage`);
    each('DAY', (v) => `Below 50% HP once: heal ${pct(v)} max HP over 1 second; all Daybreak Digimon gain 8% damage`);
    each('CHI', (v, i, t) => `After 3 knockouts, Kimeramon rises with ${v} HP and ${t.ad[i]} AD (more for higher Chimera forms)`);
    each('DRK', (v, i) => `Dragon Kin gain ${v} max HP. After each PvP combat, gain a ${i ? 'golden ' : ''}Digi-Egg that hatches into loot`);
    each('HNT', (v, i, t) => `Dragon Hunters gain ${v} AP; after their first knockout of a 1500+ HP enemy, your team gains ${t.team[i]} AP`);
    each('SEN', (v, i, t) => `A ${v} shield (more at higher forms) that lasts ${t.dur[i]}s, then jumps; its holder gains ${pct(t.as[i])} attack speed per jump so far (up to 5)`);
    each('ASN', (v, i, t) => `${pct(v)} crit chance and ${pct(t.cd[i])} crit damage; abilities can crit`);
    each('CAV', (v) => `Take ${pct(v)} less damage, ${pct(v * 2)} for the first 4s and for 3s after each ability`);
    each('KNT', (v) => `Every Digimon on your team takes ${v} less damage from each hit`);
    each('LEG', (v) => `${pct(v)} attack speed; the first attack after an ability heals for 50% of its damage`);
    each('RNG', (v) => `${pct(v)} attack speed for 4s at 4s, 12s, 20s...`);
    each('REN', (v) => `Heal ${pct(v)} max HP per second, or gain ${pct(v)} max mana per second at full HP`);
    each('SKR', (v, i, t) => `A ${pct(v)} max HP shield at the start, +${t.ad[i]} AD every second (more at higher forms)`);
    each('SPW', (v, i, t) => `+${v} AP, +${t.per[i]} AP each time any Digimon casts (up to 10 times)`);
    each('CAN', (v) => `Every 5th attack: ${Math.round(v * 100)}% of the attack's damage to the target and enemies next to it`);
  }

  // ---------- lines ----------
  // L(id, tier, traits, attr, forms, abilities, type, options). Models come from js/sets/reckoning-models.js
  // (DT.Sets.assignModels); without it a line shows the placeholder creature. src: the corpus champion the line adapts.
  const L = (id, tier, traits, attr, forms, abil, type, opt = {}) => DT.line(id, tier, traits, attr, forms, abil, type, { unassigned: true, ...opt });
  const GAL_RIVALS = ['DSP', 'NGT', 'WIT', 'TRK', 'HNT', 'CHI', 'PHA'];
  const MEG_RIVALS = ['SER', 'DAY', 'DRK', 'IRN', 'SEN'];
  const LINES = [
    // ---- 1-cost (13) ----
    L('rec_zubamon', 1, ['SER', 'LEG'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Grademon'], ['Twenty Dive', 'Road Two Decade', 'Grade Slash'], 'drain', { role: 'Duelist', src: 'TFT5 Aatrox' }),
    L('rec_otamamon', 1, ['DAY', 'BRW'], 'Virus', ['Otamamon', 'Gekomon', 'ShogunGekomon'], ['Lullaby Bubble', 'Symphony Crusher', 'Musical Fist'], 'slam', { role: 'Bruiser', src: 'TFT5 Gragas' }),
    L('rec_keramon', 1, ['CHI', 'LEG'], 'Virus', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', 'Spider Shooter'], 'line', { role: 'Sniper', hpMul: 1.2, adMul: 1.25, src: 'TFT5 Kalista' }),
    L('rec_wormmon', 1, ['DAY', 'ASN'], 'Free', ['Wormmon', 'Stingmon', 'Dinobeemon'], ['Sticky Net', 'Spiking Strike', 'Hell Masquerade'], 'execute', { role: 'Assassin', src: "TFT5 Kha'Zix" }),
    L('rec_chuumon', 1, ['TRK', 'CAV'], 'Virus', ['Chuumon', 'Sukamon', 'KingSukamon'], ['Cheese Bomb', 'Poop Toss', 'King Stamp'], 'dash', { role: 'Bruiser', src: 'TFT5 Kled' }),
    L('rec_coronamon', 1, ['SER', 'KNT'], 'Vaccine', ['Coronamon', 'Aegiomon', 'Duramon'], ['Corona Flame', 'Stun Beat Blow', 'Glass Slash'], 'stun', { role: 'Guardian', src: 'TFT5 Leona' }),
    L('rec_penguinmon', 1, ['WIT', 'REN'], 'Vaccine', ['Penguinmon', 'Sorcermon', 'Witchmon'], ['Ice Prism', 'Ice Illusion', 'Cold Flame'], 'weaken', { role: 'Support', hpMul: 1.1, src: 'TFT5 Lissandra' }),
    L('rec_gomamon', 1, ['SEN', 'SKR'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Vikemon'], ['Sharp Sider', 'Harpoon Torpedo', 'Mjolnir'], 'drain', { role: 'Striker', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT5.5 Olaf' }),
    L('rec_kamemon', 1, ['TRK', 'KNT'], 'Data', ['Kamemon', 'Golemon', 'Knightmon'], ['Shell Guard', 'Guardian Bomb', 'Grand Smash'], 'shield', { role: 'Guardian', src: 'TFT5 Poppy' }),
    L('rec_sisnoir', 1, ['SEN', 'CAN'], 'Virus', ['Sistermon Noir', 'Astamon', 'SaviorHuckmon'], ['Mickey Bullet', 'Bless Fire', 'Meteor Flame'], 'stun', { role: 'Sniper', levels: ['Rookie', 'Ultimate', 'Ultimate'], src: 'TFT5.5 Senna' }),
    L('rec_demidevimon', 1, ['DSP', 'RNG'], 'Virus', ['DemiDevimon', 'Devimon', 'SkullSatamon'], ['Demi Darts', 'Laser Wing', 'Nail Bone'], 'execute', { role: 'Sniper', src: 'TFT5 Vayne' }),
    L('rec_dracmon', 1, ['NGT', 'REN'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Undead Fang', 'Drain Slash', 'Thousand Arrows'], 'drain', { role: 'Mystic', range: 2, hpMul: 1.15, src: 'TFT5 Vladimir' }),
    L('rec_toyagumon', 1, ['TRK', 'SPW'], 'Vaccine', ['ToyAgumon', 'Starmon', 'Mamemon'], ['Plastic Blaze', 'Meteor Shower', 'Smiley Bomb'], 'aoe', { role: 'Mystic', src: 'TFT5 Ziggs' }),
    // ---- 2-cost (13) ----
    L('rec_blackagumon', 2, ['CHI', 'SPW'], 'Virus', ['BlackAgumon', 'Meramon', 'SkullMeramon'], ['Spitfire', 'Burning Fist', 'Heavy Metal Fire'], 'chain', { role: 'Mystic', src: 'TFT5 Brand' }),
    L('rec_blackgabumon', 2, ['DSP', 'CAV'], 'Virus', ['BlackGabumon', 'BlackGarurmon', 'BlackMetalGarurumon'], ['Little Horn', 'Freeze Fang', 'Full Moon Kick'], 'drain', { role: 'Bruiser', radius: 1, src: 'TFT5 Hecarim' }),
    L('rec_falcomon', 2, ['TRK', 'SKR'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Scratch Smash', 'Kunai Wing', 'Fire Jewels'], 'line', { role: 'Striker', range: 2, src: 'TFT5 Kennen' }),
    L('rec_impmon', 2, ['WIT', 'ASN'], 'Virus', ['Impmon', 'Wizardmon', 'Wisemon'], ['Bada Boom', 'Thunder Cloud', 'Pandora Dialogue'], 'stun', { role: 'Assassin', src: 'TFT5 LeBlanc' }),
    L('rec_hagurumon', 2, ['IRN', 'KNT'], 'Virus', ['Hagurumon', 'Guardromon', 'Andromon'], ['Cog Crusher', 'Destruction Grenade', 'Gatling Missile'], 'slam', { role: 'Guardian', src: 'TFT5 Nautilus' }),
    L('rec_kumamon', 2, ['NGT', 'CAV', 'BRW'], 'Vaccine', ['Kumamon', 'Frigimon', 'IceLeomon'], ['Snow Bomber', 'Sub-zero Ice Punch', 'Blizzard Breath'], 'stun', { role: 'Bruiser', src: 'TFT5.5 Sejuani' }),
    L('rec_monodramon', 2, ['DRK', 'BRW'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Knuckle Beater', 'Fang Strike', 'Desolation Claw'], 'aoe', { role: 'Bruiser', hpMul: 1.1, adMul: 1.1, src: 'TFT5 Sett' }),
    L('rec_lalamon', 2, ['DAY', 'REN'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Sing a Song', 'Sunshine Beam', 'Lila Shower'], 'heal', { role: 'Support', src: 'TFT5 Soraka' }),
    L('rec_patamon', 2, ['SER', 'INV'], 'Vaccine', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Air Shot', "Heaven's Knuckle", 'Gate of Destiny'], 'aoe', { role: 'Mystic', hpMul: 1.2, mana: 50, src: 'TFT5 Syndra' }),
    L('rec_tsukaimon', 2, ['DSP', 'KNT'], 'Virus', ['Tsukaimon', 'Bakemon', 'Phantomon'], ['Bad Message', "Hell's Hand", 'Soul Chopper'], 'hook', { role: 'Guardian', range: 2, src: 'TFT5 Thresh' }),
    L('rec_goblimon', 2, ['HNT', 'SKR'], 'Virus', ['Goblimon', 'Ogremon', 'Titamon'], ['Goblin Strike', 'Bone Cudgel', 'Phantom Blade'], 'weaken', { role: 'Striker', hpMul: 1.1, adMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT5 Trundle' }),
    L('rec_lunamon', 2, ['SER', 'RNG'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Lop-Ear Ripple', 'Tear Arrow', 'Ice Archery'], 'barrage', { role: 'Sniper', src: 'TFT5 Varus' }),
    L('rec_terriermon', 2, ['TRK', 'CAN'], 'Vaccine', ['Terriermon', 'Gargomon', 'Rapidmon'], ['Blazing Fire', 'Gargo Laser', 'Miracle Missile'], 'nuke', { role: 'Sniper', src: 'TFT5.5 Tristana' }),
    // ---- 3-cost (13) ----
    L('rec_dracomon', 3, ['DRK', 'RNG'], 'Vaccine', ['Dracomon', 'Coredramon (Blue)', 'Wingdramon'], ['Baby Breath', 'Blue Flare Breath', 'Blaze Sonic Breath'], 'stun', { role: 'Sniper', range: 5, hpMul: 1.1, adMul: 1.1, src: 'TFT5 Ashe' }),
    L('rec_elecmon', 3, ['NGT', 'SKR'], 'Data', ['Elecmon', 'Leomon', 'GrapLeomon'], ['Sparkling Thunder', 'Fist of the Beast King', 'Cyclonic Kick'], 'slam', { role: 'Striker', src: 'TFT5 Lee Sin' }),
    L('rec_floramon', 3, ['TRK', 'MYS'], 'Data', ['Floramon', 'Togemon', 'Piximon'], ['Rain of Pollen', 'Needle Spray', 'Pit Bomb'], 'rally', { role: 'Support', src: 'TFT5 Lulu' }),
    L('rec_salamon', 3, ['SER', 'MYS'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'line', { role: 'Mystic', src: 'TFT5 Lux' }),
    L('rec_gazimon', 3, ['DSP', 'CAN'], 'Virus', ['Gazimon', 'Cyclonemon', 'Beelzemon'], ['Electric Stun Blast', 'Hyper Heat', 'Double Impact'], 'barrage', { role: 'Sniper', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT5.5 Miss Fortune' }),
    L('rec_blackgatomon', 3, ['NGT', 'WIT', 'MYS'], 'Virus', ['BlackGatomon', 'LadyDevimon', 'Lilithmon'], ["Cat's Eye", 'Darkness Wave', 'Phantom Pain'], 'aoe', { role: 'Mystic', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Morgana' }),
    L('rec_kudamon', 3, ['DAY', 'SKR'], 'Vaccine', ['Kudamon', 'Reppamon', 'Chirinmon'], ['Bullet Whirlwind', 'Lion Claw', 'Wind Cutter Sword'], 'dash', { role: 'Duelist', src: 'TFT5 Nidalee' }),
    L('rec_kuwagamon', 3, ['PHA', 'ASN'], 'Virus', ['Kuwagamon', 'Okuwamon', 'GranKuwagamon'], ['Scissor Claw', 'Double Scissor Claw', 'Dimension Scissor'], 'drain', { role: 'Assassin', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Nocturne' }),
    L('rec_tapirmon', 3, ['CHI', 'BRW'], 'Vaccine', ['Tapirmon', 'Monochromon', 'Megadramon'], ['Nightmare Syndrome', 'Volcanic Strike', 'Ultimate Slicer'], 'execute', { role: 'Bruiser', src: 'TFT5 Nunu & Willump' }),
    L('rec_lopmon', 3, ['HNT', 'SKR'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Gauntlet Claw', 'Asipatravana'], 'aoe', { role: 'Striker', src: 'TFT5 Pantheon' }),
    L('rec_biyomon', 3, ['SEN', 'REN'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'heal', { role: 'Support', src: 'TFT5.5 Rakan' }),
    L('rec_ryudamon', 3, ['NGT', 'LEG'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Katana Attack', 'Metal Armor Blade', 'Legendary Dragon Blade'], 'nuke', { role: 'Duelist', src: 'TFT5 Yasuo' }),
    L('rec_palmon', 3, ['DRK', 'SPW'], 'Data', ['Palmon', 'Vegiemon', 'Lillymon'], ['Poison Ivy', 'Sharp Leaf', 'Flower Cannon'], 'stun', { role: 'Mystic', hpMul: 1.1, src: 'TFT5 Zyra' }),
    // ---- 4-cost (11) ----
    L('rec_gaomon', 4, ['NGT', 'HNT', 'ASN'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Gaoga Hound', 'Howling Cannon'], 'slam', { role: 'Assassin', hpMul: 1.15, levels: ['Rookie', 'Champion', 'Ultimate'], src: 'TFT5 Diana' }),
    L('rec_dorumon', 4, ['DSP', 'LEG'], 'Data', ['Dorumon', 'Dorugamon', 'DoruGreymon'], ['Dash Metal', 'Power Metal', 'Metal Meteor'], 'nuke', { role: 'Sniper', range: 3, as: 0.85, hpMul: 1.3, adMul: 1.55, src: 'TFT5 Draven' }),
    L('rec_raremon', 4, ['CHI', 'PHA', 'MYS'], 'Virus', ['Raremon', 'Soulmon', 'Raguelmon'], ['Stinking Gas', 'Necro Magic', 'Form Taranis'], 'drain', { role: 'Mystic', radius: 1, mana: 70, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5.5 Fiddlesticks' }),
    L('rec_coredramon', 4, ['DRK', 'SEN', 'KNT'], 'Virus', ['Coredramon (Green)', 'Groundramon', 'Breakdramon'], ['Green Flare Breath', 'Megaton Hammer Crush', 'Infinity Bowling'], 'slam', { role: 'Guardian', radius: 2, hpMul: 1.15, adMul: 1.1, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5.5 Galio' }),
    L('rec_woodmon', 4, ['PHA', 'INV', 'REN'], 'Virus', ['Woodmon', 'Cherrymon', 'Puppetmon'], ['Branch Drain', 'Illusion Mist', 'Puppet Pummel'], 'summon', { role: 'Support', summon: 'rec_daisy', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Ivern' }),
    L('rec_hackmon', 4, ['IRN', 'SKR'], 'Data', ['Hackmon', 'BaoHuckmon', 'Jesmon'], ['Fifth Rush', 'Fif Cross', 'Weltgeist'], 'execute', { role: 'Duelist', hpMul: 1.1, adMul: 1.1, mana: 50, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT5 Jax' }),
    L('rec_hawkmon', 4, ['DAY', 'INV'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Static Force'], 'aoe', { role: 'Mystic', hpMul: 1.25, mana: 40, src: 'TFT5 Karma' }),
    L('rec_rapidarmor', 4, ['SEN', 'CAN'], 'Vaccine', ['Commandramon', 'MetalMamemon', 'MegaGargomon'], ['M16 Assassin', 'Psycho Blaster', 'Giant Missile'], 'line', { role: 'Sniper', hpMul: 1.15, adMul: 1.35, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT5.5 Lucian' }),
    L('rec_blackgreymon', 4, ['HNT', 'LEG'], 'Virus', ['BlackGreymon', 'BlackMetalGreymon', 'BlackWarGreymon'], ['Mega Flame', 'Giga Destroyer', 'Terra Destroyer'], 'shield', { role: 'Bruiser', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Mordekaiser' }),
    L('rec_veemon', 4, ['SER', 'IRN', 'CAV'], 'Vaccine', ['Veemon', 'Magnamon', 'Magnamon X'], ['Vee Headbutt', 'Magna Explosion', 'Extreme Jihad'], 'rally', { role: 'Guardian', hpMul: 1.15, levels: ['Rookie', 'Armor', 'Mega'], src: 'TFT5 Rell' }),
    L('rec_unimon', 4, ['SER', 'SPW'], 'Vaccine', ['Unimon', 'HippoGryphonmon', 'Ophanimon'], ['Horn Blaster', 'Heat Wave', "EDEN's Javelin"], 'line', { role: 'Mystic', hpMul: 1.1, mana: 50, levels: ['Champion', 'Ultimate', 'Mega'], src: "TFT5 Vel'Koz" }),
    // ---- 5-cost (10) ----
    L('rec_valkyrimon', 5, ['SEN', 'RNG'], 'Vaccine', ['Valkyrimon', 'Kentaurosmon', 'Kentaurosmon X'], ["Aurvandil's Arrow", 'Bifrost', 'Brander'], 'execute', { role: 'Sniper', mana: 50, hpMul: 1.45, adMul: 1.55, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT5.5 Akshan' }),
    L('rec_growlmon', 5, ['NGT', 'RIV', 'KNT'], 'Virus', ['Growlmon', 'WarGrowlmon', 'Megidramon'], ['Exhaust Flame', 'Atomic Blaster', 'Megiddo Flame'], 'drain', { role: 'Bruiser', radius: 1, rivals: MEG_RIVALS, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Darius' }),
    L('rec_guilmon', 5, ['DAY', 'RIV', 'KNT'], 'Vaccine', ['Guilmon', 'Gallantmon', 'Gallantmon Crimson Mode'], ['Rock Breaker', 'Lightning Joust', 'Invincible Sword'], 'aoe', { role: 'Guardian', radius: 2, mana: 80, rivals: GAL_RIVALS, levels: ['Rookie', 'Mega', 'Mega'], src: 'TFT5 Garen' }),
    L('rec_sisciel', 5, ['INA', 'MYS'], 'Data', ['Kotemon', 'Sistermon Ciel (Awakened)', 'Durandamon'], ['Meteor Slash', 'Byakusai', 'Zwei Glanze'], 'aoe', { role: 'Duelist', range: 2, hpMul: 1.2, adMul: 1.1, ai: { dmg: 'ap' }, levels: ['Rookie', 'Rookie', 'Mega'], src: 'TFT5.5 Gwen' }),
    L('rec_clockmon', 5, ['DRK', 'REN'], 'Data', ['Clockmon', 'Datamon', 'Machinedramon'], ['Time Buster', 'Digital Bomb', 'Infinity Cannon'], 'summon', { role: 'Support', range: 4, hpMul: 1.3, mana: 50, summon: 'rec_gumdramon', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT5 Heimerdinger' }),
    L('rec_lucemon', 5, ['SER', 'LEG'], 'Vaccine', ['Lucemon', 'Lucemon Chaos Mode', 'Lucemon Shadowlord Mode'], ['Grand Cross', 'Paradise Lost', 'Divine Atonement'], 'chain', { role: 'Sniper', mana: 45, hpMul: 1.15, adMul: 1.25, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT5 Kayle' }),
    L('rec_renamon', 5, ['ETR', 'RNG', 'MYS'], 'Data', ['Renamon', 'Taomon', 'Sakuyamon'], ['Diamond Storm', 'Talisman of Light', 'Amethyst Mandala'], 'respite', { role: 'Mystic', mana: 75, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT5 Kindred' }),
    L('rec_mushroomon', 5, ['TRK', 'INV'], 'Virus', ['Mushroomon', 'Vademon', 'Piedmon'], ['Laughing Smasher', 'Unidentified Flying Kiss', 'Trump Sword'], 'aoe', { role: 'Mystic', radius: 2, range: 4, mana: 40, hpMul: 1.3, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT5 Teemo' }),
    L('rec_myotismon', 5, ['DSP', 'SKR', 'ASN'], 'Virus', ['Myotismon', 'VenomMyotismon', 'GranDracmon'], ['Crimson Lightning', 'Venom Infusion', 'Crystal Revolution'], 'possess', { role: 'Assassin', levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT5 Viego' }),
    L('rec_bearmon', 5, ['PHA', 'BRW'], 'Vaccine', ['Bearmon', 'SaberLeomon', 'BanchoLeomon'], ['Bear Fist', 'Howling Crusher', 'Flashy Boss Punch'], 'slam', { role: 'Bruiser', radius: 2, hpMul: 1.3, adMul: 1.2, levels: ['Rookie', 'Mega', 'Mega'], src: 'TFT5 Volibear' }),

    // ---- summons (never in the shop, never count for traits) ----
    L('rec_kimeramon', 3, [], 'Data', ['Kimeramon', 'Kimeramon', 'Kimeramon'], ['Heat Viper', 'Heat Viper', 'Heat Viper'], 'aoe', { role: 'Bruiser', shop: false, color: '#7fd04a', levels: ['Ultimate', 'Ultimate', 'Ultimate'], src: 'TFT5 The Monstrosity' }),
    L('rec_daisy', 2, [], 'Vaccine', ['Monzaemon', 'Monzaemon', 'Monzaemon'], ['Hearts Attack', 'Hearts Attack', 'Hearts Attack'], 'stun', { role: 'Guardian', shop: false, color: '#ffb0c8', levels: ['Ultimate', 'Ultimate', 'Ultimate'], src: 'TFT5 Daisy' }),
    L('rec_gumdramon', 2, [], 'Vaccine', ['Gumdramon', 'Gumdramon', 'Gumdramon'], ['Fire Vortex', 'Fire Vortex', 'Fire Vortex'], 'aoe', { role: 'Sniper', shop: false, color: '#ff8a3a', src: 'TFT5 Baby Dragon' }),
    L('rec_wolf', 4, [], 'Vaccine', ['Garurumon', 'Garurumon', 'Garurumon'], ['Fox Fire', 'Fox Fire', 'Fox Fire'], 'dash', { role: 'Duelist', shop: false, color: '#9fc8ff', levels: ['Champion', 'Champion', 'Champion'], src: 'TFT5 Wolf' }),
    L('rec_voidspawn', 1, [], 'Virus', ['Voidlarva', 'Voidlarva', 'Voidlarva'], ['Glare Eye', 'Glare Eye', 'Glare Eye'], 'shield', { role: 'Guardian', shop: false, color: '#9a6bd0', levels: ['In-Training', 'In-Training', 'In-Training'], src: "TFT5 Zz'Rot Voidspawn" }),

    // ---- wild Digimon for PvE rounds ----
    L('rec_w_pagumon', 1, ['DSP'], 'Virus', ['Koromon', 'Koromon', 'Koromon'], ['Poison Bubble', 'Poison Bubble', 'Poison Bubble'], 'nuke', { role: 'Striker', shop: false, hpMul: 0.75, levels: ['In-Training', 'In-Training', 'In-Training'] }),
    L('rec_w_numemon', 1, ['TRK'], 'Virus', ['Numemon', 'Numemon', 'Numemon'], ['Nume-Sludge', 'Nume-Sludge', 'Nume-Sludge'], 'weaken', { role: 'Bruiser', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('rec_w_geremon', 2, ['CHI'], 'Virus', ['ShellNumemon', 'ShellNumemon', 'ShellNumemon'], ['Hyper Smell', 'Hyper Smell', 'Hyper Smell'], 'weaken', { role: 'Mystic', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('rec_w_tyrannomon', 2, ['DRK'], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Fire Breath', 'Fire Breath', 'Fire Breath'], 'aoe', { role: 'Bruiser', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('rec_w_etemon', 3, ['WIT'], 'Virus', ['Etemon', 'Etemon', 'Etemon'], ['Dark Network', 'Dark Network', 'Dark Network'], 'aoe', { role: 'Mystic', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('rec_w_metaltyrannomon', 3, ['IRN'], 'Virus', ['MetalTyrannomon', 'MetalTyrannomon', 'MetalTyrannomon'], ['Nuclear Laser', 'Nuclear Laser', 'Nuclear Laser'], 'line', { role: 'Guardian', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('rec_w_icedevimon', 3, ['NGT'], 'Virus', ['IceDevimon', 'IceDevimon', 'IceDevimon'], ['Frost Claw', 'Frost Claw', 'Frost Claw'], 'stun', { role: 'Assassin', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('rec_w_skullgreymon', 4, ['DSP'], 'Virus', ['SkullGreymon', 'SkullGreymon', 'SkullGreymon'], ['Dark Shot', 'Dark Shot', 'Dark Shot'], 'aoe', { role: 'Striker', shop: false, boss: true, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('rec_w_kingetemon', 4, ['WIT'], 'Virus', ['Mammothmon', 'Mammothmon', 'Mammothmon'], ['King Monkick', 'King Monkick', 'King Monkick'], 'slam', { role: 'Bruiser', shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_gigadramon', 5, ['IRN'], 'Virus', ['Gigadramon', 'Gigadramon', 'Gigadramon'], ['Giga Heat', 'Giga Heat', 'Giga Heat'], 'aoe', { role: 'Striker', shop: false, boss: true, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('rec_w_rusttyranomon', 5, ['DRK'], 'Virus', ['Chaosdramon', 'Chaosdramon', 'Chaosdramon'], ['Rust Breath', 'Rust Breath', 'Rust Breath'], 'aoe', { role: 'Bruiser', shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_dragomon', 5, ['CHI'], 'Virus', ['Dragomon', 'Dragomon', 'Dragomon'], ['Forbidden Trident', 'Forbidden Trident', 'Forbidden Trident'], 'line', { role: 'Mystic', shop: false, boss: true, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('rec_w_daemon', 5, ['NGT'], 'Virus', ['Daemon', 'Daemon', 'Daemon'], ['Evil Inferno', 'Evil Inferno', 'Evil Inferno'], 'aoe', { role: 'Mystic', shop: false, boss: true, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_leviamon', 5, ['BRW'], 'Virus', ['Leviamon', 'Leviamon', 'Leviamon'], ['Rostrum', 'Rostrum', 'Rostrum'], 'drain', { role: 'Bruiser', shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_barbamon', 5, ['WIT'], 'Virus', ['Barbamon', 'Barbamon', 'Barbamon'], ['Death Lure', 'Death Lure', 'Death Lure'], 'aoe', { role: 'Mystic', shop: false, boss: true, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_belphemon', 5, ['PHA'], 'Virus', ['Belphemon Rage Mode', 'Belphemon Rage Mode', 'Belphemon Rage Mode'], ['Chains of Sloth', 'Chains of Sloth', 'Chains of Sloth'], 'slam', { role: 'Guardian', shop: false, boss: true, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_millenniummon', 5, ['CHI'], 'Virus', ['Millenniummon', 'Millenniummon', 'Millenniummon'], ['Dimension Destroyer', 'Dimension Destroyer', 'Dimension Destroyer'], 'aoe', { role: 'Bruiser', shop: false, boss: true, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_apocalymon', 5, ['DSP'], 'Free', ['Apocalymon', 'Apocalymon', 'Apocalymon'], ['Darkness Zone', 'Darkness Zone', 'Darkness Zone'], 'aoe', { role: 'Mystic', shop: false, boss: true, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('rec_w_armageddemon', 5, ['DSP'], 'Free', ['Armageddemon', 'Armageddemon', 'Armageddemon'], ['Black Rain', 'Black Rain', 'Black Rain'], 'barrage', { role: 'Sniper', shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
  ];

  // ---------- wild rounds ----------
  // [lineId, stage, col, row] in the enemy's rows 4-7 (4 = front). rules.pveMul scales their HP and AD by stage.
  const PVE = (stage, round) => {
    if (stage === 1) {
      if (round === 2) return [['rec_w_pagumon', 0, 2, 4], ['rec_w_pagumon', 0, 4, 4]];
      return [['rec_w_pagumon', 0, 2, 4], ['rec_w_pagumon', 0, 4, 4], ['rec_w_numemon', 0, 3, 5]];
    }
    // the sludge pit
    if (stage === 2) return [['rec_w_numemon', 1, 2, 4], ['rec_w_numemon', 1, 4, 4], ['rec_w_geremon', 0, 3, 6], ['rec_w_pagumon', 1, 1, 6]];
    // Dino Valley
    if (stage === 3) return [['rec_w_tyrannomon', 1, 2, 4], ['rec_w_tyrannomon', 1, 4, 4], ['rec_w_metaltyrannomon', 0, 3, 4], ['rec_w_geremon', 1, 2, 7], ['rec_w_etemon', 0, 4, 7]];
    // Etemon's Dark Network
    if (stage === 4) return [['rec_w_metaltyrannomon', 1, 3, 4], ['rec_w_tyrannomon', 1, 1, 4], ['rec_w_tyrannomon', 1, 5, 4], ['rec_w_kingetemon', 0, 3, 5], ['rec_w_etemon', 1, 2, 7], ['rec_w_etemon', 1, 4, 7]];
    // the Black Mist (dark dragons)
    if (stage === 5) return [['rec_w_metaltyrannomon', 2, 3, 4], ['rec_w_skullgreymon', 1, 2, 4], ['rec_w_skullgreymon', 1, 4, 4], ['rec_w_gigadramon', 1, 3, 5], ['rec_w_icedevimon', 2, 0, 6], ['rec_w_icedevimon', 2, 6, 6], ['rec_w_rusttyranomon', 1, 3, 7]];
    // the Dark Ocean
    if (stage === 6) return [['rec_w_metaltyrannomon', 2, 2, 4], ['rec_w_metaltyrannomon', 2, 4, 4], ['rec_w_dragomon', 1, 3, 5], ['rec_w_rusttyranomon', 1, 1, 5], ['rec_w_gigadramon', 1, 5, 5], ['rec_w_icedevimon', 2, 3, 7]];
    // Demon Lords
    if (stage === 7) return [['rec_w_belphemon', 1, 3, 4], ['rec_w_leviamon', 1, 1, 4], ['rec_w_daemon', 1, 5, 5], ['rec_w_barbamon', 1, 3, 7], ['rec_w_skullgreymon', 2, 2, 6], ['rec_w_skullgreymon', 2, 4, 6]];
    // Millenniummon and Apocalymon
    if (stage === 8) return [['rec_w_belphemon', 2, 3, 4], ['rec_w_leviamon', 2, 1, 4], ['rec_w_millenniummon', 1, 5, 4], ['rec_w_apocalymon', 1, 3, 7], ['rec_w_daemon', 2, 2, 6], ['rec_w_barbamon', 1, 4, 6]];
    // Armageddemon
    return [['rec_w_belphemon', 2, 3, 4], ['rec_w_millenniummon', 2, 1, 4], ['rec_w_leviamon', 2, 5, 4], ['rec_w_armageddemon', 2, 3, 7], ['rec_w_apocalymon', 2, 2, 6], ['rec_w_daemon', 2, 4, 6]];
  };

  // ---------- items ----------
  // 8 stat components, 8 Shadow components (same stats; any recipe with one makes the Shadow variant) and the Digi-Egg.
  const COMP = {
    blade: ['Power Blade', '#ff7a45', { ad: 0.15 }],
    bow: ['Speed Wing', '#ffd23f', { as: 0.12 }],
    rod: ['Data Wand', '#b26bff', { ap: 15 }],
    tear: ['Mana Drop', '#4fb3ff', { mana: 15 }],
    vest: ['Chrome Vest', '#c0c6d0', { armor: 20 }],
    cloak: ['Data Mantle', '#3fd6b0', { mr: 20 }],
    belt: ['Life Belt', '#ff4d64', { hp: 150 }],
    glove: ['Lucky Gloves', '#8fdc4e', { crit: 0.1, dodge: 0.08 }],
  };
  const SHADOW_NAME = { blade: 'Spore Blade', bow: 'Spore Wing', rod: 'Spore Wand', tear: 'Spore Drop', vest: 'Spore Vest', cloak: 'Spore Mantle', belt: 'Spore Belt', glove: 'Spore Gloves' };
  const components = {};
  for (const [k, [name, color, stats]] of Object.entries(COMP)) components[`rec_${k}`] = { name, short: name.split(' ').map((w) => w[0]).join(''), color, stats };
  for (const [k, [, color, stats]] of Object.entries(COMP)) {
    components[`rec_s${k}`] = { name: SHADOW_NAME[k], short: 'S' + k[0].toUpperCase(), color: '#5a2a8a', color2: color, stats, special: true, shadow: k,
      desc: `${DT.statText(stats)}. Dark Spore component: combined with any component it makes the Shadow version of the item.` };
  }
  components.rec_egg = { name: 'Digi-Egg', short: 'EG', color: '#fff3b0', stats: {}, special: true,
    desc: 'Combine with a component to make a Crest (Spore components make the rarer Crests), or with another Digi-Egg for +1 team size.' };

  // One row per completed item: [key, a, b, base, shadow, radiant]. Each variant: { n: name, d: desc, p: passives,
  // x: extra stats }. tags/aiRank for the AI. Hooks for items without a passive are in reckoning-mech.js by id.
  const ROWS = [
    ['victory_sword', 'blade', 'blade', ['ad'], 2,
      { n: 'Victory Sword', d: 'Deals 8% more damage. Each knockout gives +12% attack damage for the rest of combat.', p: { amp: 0.08 }, x: { ad: 0.1 } },
      { n: 'Ruin Sword', d: 'Deals 15% more damage. Each knockout gives +20% attack damage. Drawback: 15% less max HP.', p: { amp: 0.15 }, x: { ad: 0.15 } },
      { n: 'Luminous Sword', d: 'Deals 20% more damage. Each knockout gives +20% attack damage.', p: { amp: 0.2 }, x: { ad: 0.35 } }],
    ['titan_slayer', 'blade', 'bow', ['ad'], 2,
      { n: 'Titan Slayer', d: 'Deals 8% more damage, and 35% more to enemies with 1600 or more max HP.', p: { amp: 0.08, slayer: 0.35 } },
      { n: 'Spectral Slayer', d: 'Deals 50% more damage to enemies with 1100 or more max HP. Drawback: no bonus against smaller enemies.', p: {} },
      { n: 'Demon Slayer', d: 'Deals 30% more damage, and 40% more on top to enemies with 1600 or more max HP.', p: { amp: 0.3, slayer: 0.4 }, x: { as: 0.1 } }],
    ['gunblade', 'blade', 'rod', ['ad', 'ap'], 1,
      { n: 'Data Gunblade', d: 'Heals for 22% of all damage dealt.', p: { vamp: 0.22 } },
      { n: 'Mercy Gunblade', d: 'Magic damage the holder deals heals the ally with the lowest HP for 50% of it. Drawback: the holder heals nothing from it.', p: {} },
      { n: 'Lifeblade', d: 'Heals for 40% of all damage dealt.', p: { vamp: 0.4 }, x: { ap: 30 } }],
    ['mana_lance', 'blade', 'tear', ['ap', 'ad'], 1,
      { n: 'Mana Lance', d: 'Basic attacks restore 6 extra mana.', p: { shojin: 6 } },
      { n: 'Spectral Lance', d: 'Basic attacks restore 14 extra mana. Drawback: deals 10% less damage.', p: { shojin: 14, amp: -0.1 } },
      { n: 'Lance of Light', d: 'Basic attacks restore 12 extra mana.', p: { shojin: 12 }, x: { ap: 25 } }],
    ['guardian_feather', 'blade', 'vest', ['tank', 'ad'], 1,
      { n: 'Guardian Feather', d: 'The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with 30% max HP.', p: {} },
      { n: 'Fallen Feather', d: 'The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with 70% max HP. Drawback: after that it has 50% less attack speed.', p: {} },
      { n: 'Archangel Feather', d: 'The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with full HP.', p: {}, x: { as: 0.3 } }],
    ['blood_fang', 'blade', 'cloak', ['ad'], 1,
      { n: 'Blood Fang', d: 'Heals for 25% of damage dealt. Below 40% HP, gains a 25% max HP shield once.', p: { vamp: 0.25, lifeline: { at: 0.4, shield: 0.25 } } },
      { n: 'Risk Fang', d: 'Heals for 40% of damage dealt. Each time it heals back above 90% HP, it gains 40% attack speed for 4 seconds. Drawback: starts combat at 80% HP.', p: { vamp: 0.45 } },
      { n: 'Blessed Fang', d: 'Heals for 50% of damage dealt. Below 40% HP, gains a 40% max HP shield once.', p: { vamp: 0.5, lifeline: { at: 0.4, shield: 0.4 } }, x: { ad: 0.2 } }],
    ['war_banner', 'blade', 'belt', ['tank'], 0,
      { n: 'War Banner', d: 'At the start of combat, the holder and allies next to it in the same row gain 25% attack speed.', p: {} },
      { n: 'Bleak Banner', d: 'At the start of combat, allies next to the holder in the same row lose 20% attack speed, and the holder gains 35% attack speed for each of them. Drawback: slows those allies.', p: {} },
      { n: 'Harmony Banner', d: 'At the start of combat, the holder and allies next to it in the same row gain 45% attack speed and 10% omnivamp.', p: {} }],
    ['crit_edge', 'blade', 'glove', ['ad'], 3,
      { n: 'Crit Edge', d: 'Grants extra critical strike chance and 10% crit damage.', p: { critDmg: 0.1 }, x: { crit: 0.45 } },
      { n: 'Sacrifice Edge', d: 'Grants extra critical strike chance and 50% crit damage. Drawback: each basic attack costs the holder 2% of its max HP.', p: { critDmg: 0.5 }, x: { crit: 0.5 } },
      { n: 'Zenith Edge', d: 'Grants extra critical strike chance and 50% crit damage. Abilities can critically strike.', p: { critDmg: 0.5, abilCrit: true }, x: { crit: 0.6 } }],
    ['rapid_cannon', 'bow', 'bow', ['ad'], 1,
      { n: 'Rapid Cannon', d: '+1 attack range and extra attack speed.', p: {}, x: { as: 0.25 } },
      { n: 'Rapid Deathcannon', d: '+1 attack range. Gains 75% attack speed while no enemy is within 2 hexes. Drawback: no flat attack speed bonus.', p: {} },
      { n: 'Light Cannon', d: '+2 attack range and a lot of attack speed.', p: {}, x: { as: 0.8, ad: 0.15 } }],
    ['rage_wing', 'bow', 'rod', ['ad', 'ap'], 2,
      { n: 'Rage Wing', d: 'Each basic attack grants 6% attack speed, stacking.', p: { rage: 0.06 } },
      { n: 'Blood Rage Wing', d: 'Each basic attack grants 10% attack speed, stacking. Drawback: each basic attack costs the holder 1.5% of its max HP.', p: { rage: 0.1 } },
      { n: 'Reckoning Wing', d: 'Each basic attack grants 10% attack speed, stacking.', p: { rage: 0.1 }, x: { as: 0.3 } }],
    ['volt_disk', 'bow', 'tear', ['ad'], 0,
      { n: 'Volt Disk', d: 'Every 3rd basic attack sends lightning to 3 enemies for 80 magic damage.', p: { volt: 80 } },
      { n: 'Volt Stiletto', d: 'Every 2nd basic attack sends lightning to 4 enemies for 80 magic damage. Drawback: 30% less attack damage.', p: {} },
      { n: 'Volt Favor', d: 'Every 3rd basic attack sends lightning to 3 enemies for 160 magic damage.', p: { volt: 160 }, x: { as: 0.2 } }],
    ['titan_will', 'bow', 'vest', ['tank', 'ad'], 1,
      { n: 'Titan Will', d: 'Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times.', p: { titan: true } },
      { n: 'Titan Revenge', d: 'Stores 60% of the damage the holder takes. Every 2 seconds its next basic attack releases the store as true damage. Drawback: no stacking stats.', p: {}, x: { armor: 15 } },
      { n: 'Titan Vow', d: 'Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times.', p: { titan: true }, x: { armor: 30, mr: 30, ad: 0.2 } }],
    ['twin_shot', 'bow', 'cloak', ['ad'], 1,
      { n: 'Twin Shot', d: 'Basic attacks also hit a second nearby enemy for 55% damage.', p: { split: 0.55 } },
      { n: 'Wild Twin Shot', d: 'Basic attacks also hit 2 more nearby enemies for 70% damage. Drawback: 10% less attack damage.', p: {} },
      { n: 'Twin Tempest', d: 'Basic attacks also hit a second nearby enemy for full damage.', p: { split: 1 }, x: { as: 0.3 } }],
    ['void_portal', 'bow', 'belt', ['tank'], 0,
      { n: 'Void Portal', d: 'At the start of combat, enemies within 2 hexes target the holder. When the holder is knocked out, a Voidlarva rises in its place.', p: {} },
      { n: 'Unstable Portal', d: 'Like Void Portal, but the Voidlarva deals 250 true damage to enemies next to it when it falls. Drawback: 10% less max HP.', p: {} },
      { n: 'Void Invitation', d: 'At the start of combat, a Voidlarva rises next to the holder and enemies within 3 hexes target it. When the holder is knocked out, another rises.', p: {}, x: { hp: 200 } }],
    ['armor_breaker', 'bow', 'glove', ['ad'], 1,
      { n: 'Armor Breaker', d: 'Basic attacks cut the target\'s armor by 40% for 3 seconds.', p: { shred: 0.4 } },
      { n: 'Final Whisper', d: 'Critical strikes cut the target\'s armor and magic resist by 70% for 5 seconds. Drawback: the holder has 20% less armor and magic resist.', p: {}, x: { crit: 0.15 } },
      { n: 'Eternal Whisper', d: 'Basic attacks cut the target\'s armor by 70% for 3 seconds.', p: { shred: 0.7 }, x: { crit: 0.35 } }],
    ['ultima_wand', 'rod', 'rod', ['ap'], 2,
      { n: 'Ultima Wand', d: 'A lot of ability power.', p: {}, x: { ap: 40 } },
      { n: 'Caustic Wand', d: 'A huge amount of ability power. Drawback: 15% less max HP.', p: {}, x: { ap: 90 } },
      { n: 'Ascended Wand', d: 'An enormous amount of ability power.', p: {}, x: { ap: 125 } }],
    ['seraph_staff', 'rod', 'tear', ['ap'], 2,
      { n: 'Seraph Staff', d: 'Each cast grants ability power equal to 30% of the holder\'s max mana.', p: {} },
      { n: 'Undying Staff', d: 'Each cast grants max HP equal to 3.5 times the holder\'s max mana and heals that much. Drawback: no ability power gain.', p: {} },
      { n: 'Holy Staff', d: 'Each cast grants ability power equal to 60% of the holder\'s max mana.', p: {}, x: { mana: 15, ap: 30 } }],
    ['iron_locket', 'rod', 'vest', ['tank'], 0,
      { n: 'Iron Locket', d: 'At the start of combat, the holder and allies within 2 hexes in the same row gain a 250 shield (350 Champion form, 450 final form).', p: {} },
      { n: 'Lunar Locket', d: 'At the start of combat, the holder and allies within 2 hexes in the same row gain 60 armor and magic resist. Drawback: they gain 5 less mana per basic attack.', p: {} },
      { n: 'Prime Locket', d: 'At the start of combat, the holder and allies within 3 hexes in the same row gain a 450 shield (600 Champion form, 750 final form) and 200 max HP.', p: {} }],
    ['mana_zap', 'rod', 'cloak', ['tank'], 0,
      { n: 'Mana Zap', d: 'When an enemy within 2 hexes casts, it takes 180 magic damage.', p: { zap: 180 } },
      { n: 'Dark Spark', d: 'When any other Digimon within 2 hexes casts, it is zapped: enemies take twice their max mana as magic damage, allies half their max mana. The holder gains 10 AP per zap. Drawback: zaps allies too.', p: {} },
      { n: 'Covalent Spark', d: 'When an enemy within 2 hexes casts, it takes 400 magic damage. Regenerates 1% max HP per second.', p: { zap: 400, regen: 0.01 }, x: { mr: 40 } }],
    ['flame_tome', 'rod', 'belt', ['ap'], 1,
      { n: 'Flame Tome', d: 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', p: { burn: true } },
      { n: 'Evil Tome', d: 'Magic damage curses the target: 6% of its max HP as true damage per second for 8 seconds, and less healing. Drawback: the holder\'s other magic damage is 20% lower.', p: {}, x: { ap: 20 } },
      { n: 'Radiant Tome', d: 'Damage burns the target for 1.5% max HP per second and cuts its healing. Deals 20% more damage.', p: { burn: true, amp: 0.2 }, x: { ap: 30 } }],
    ['jewel_gauntlet', 'rod', 'glove', ['ap'], 3,
      { n: 'Jewel Gauntlet', d: 'Abilities can critically strike. +30% crit damage.', p: { abilCrit: true, critDmg: 0.3 } },
      { n: 'Sacrifice Gauntlet', d: 'Abilities can critically strike. +50% crit damage, and +25% crit chance after the first cast. Drawback: each cast costs HP equal to half the max mana times the form (1, 2, 3).', p: { abilCrit: true, critDmg: 0.5 } },
      { n: 'Glamor Gauntlet', d: 'Abilities can critically strike. +90% crit damage.', p: { abilCrit: true, critDmg: 0.9 }, x: { crit: 0.25, ap: 20 } }],
    ['mana_overflow', 'tear', 'tear', ['ap'], 2,
      { n: 'Mana Overflow', d: 'Max mana -10. After casting, keeps 20 mana.', p: { bluebuff: true } },
      { n: 'Dark Overflow', d: 'After casting, keeps 50 mana while below 60% HP. Drawback: only 20 mana above 60% HP, and max mana is not lowered.', p: {} },
      { n: 'Blue Blessing', d: 'Max mana -10. After casting, keeps 30 mana.', p: { bluebuff: true }, x: { ap: 20 } }],
    ['frost_heart', 'tear', 'vest', ['tank'], 0,
      { n: 'Frost Heart', d: 'Enemies within 2 hexes have 25% less attack speed.', p: {} },
      { n: 'Frozen Dark Heart', d: 'Enemies within 2 hexes have 35% less attack speed. Drawback: allies next to the holder have 25% less attack speed too.', p: {} },
      { n: 'Frost Heart of Gold', d: 'Enemies within 2 hexes have 50% less attack speed.', p: {}, x: { mana: 30, armor: 40 } }],
    ['power_chalice', 'tear', 'cloak', ['tank'], 0,
      { n: 'Power Chalice', d: 'At the start of combat, the holder and allies next to it in the same row gain 25 ability power.', p: {} },
      { n: 'Malice Chalice', d: 'At the start of combat, allies within 2 hexes in the same row gain 45 ability power. Drawback: the holder loses 20 ability power.', p: {} },
      { n: 'Charity Chalice', d: 'At the start of combat, the holder and allies next to it in the same row gain 50 ability power and 10% omnivamp.', p: {} }],
    ['holy_ring', 'tear', 'belt', ['tank'], 0,
      { n: 'Holy Ring', d: 'Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP.', p: { redemption: true } },
      { n: 'Sacrifice Ring', d: 'Every 5 seconds, heals every other ally for 25% of its missing HP. Drawback: each time, the holder loses 5% of its current HP.', p: {} },
      { n: 'Radiant Ring', d: 'Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP. Regenerates 1% max HP per second.', p: { redemption: true, regen: 0.01 }, x: { hp: 350 } }],
    ['justice_hand', 'tear', 'glove', ['ad', 'ap'], 1,
      { n: 'Justice Hand', d: 'Deals 12% more damage and heals for 12% of damage dealt.', p: { amp: 0.12, vamp: 0.12 } },
      { n: 'Vengeance Hand', d: 'Gains 30% attack damage, 30 ability power and 20% omnivamp. Drawback: these switch off after each knockout the holder scores and come back on the next one.', p: {} },
      { n: 'Fairness Fist', d: 'Deals 20% more damage and heals for 20% of damage dealt.', p: { amp: 0.2, vamp: 0.2 }, x: { ad: 0.15, ap: 15 } }],
    ['spike_plate', 'vest', 'vest', ['tank'], 2,
      { n: 'Spike Plate', d: 'When attacked, deals 60 magic damage to adjacent enemies (1.5 second cooldown).', p: { thorns: 60 }, x: { armor: 20 } },
      { n: 'Refracted Plate', d: 'A huge amount of armor. Drawback: no spikes.', p: {}, x: { armor: 120 } },
      { n: 'Rosethorn Plate', d: 'When attacked, deals 200 magic damage to adjacent enemies (1.5 second cooldown). Regenerates 2% max HP per second.', p: { thorns: 200, regen: 0.02 }, x: { armor: 20 } }],
    ['stone_hide', 'vest', 'cloak', ['tank'], 2,
      { n: 'Stone Hide', d: 'Gains 12 armor and magic resist for each enemy within 2 hexes.', p: { gargoyle: 12 } },
      { n: 'Undying Stone', d: 'Every 2 seconds, heals 2% max HP for each enemy within 2 hexes. Drawback: no armor or magic resist from nearby enemies.', p: {} },
      { n: 'Dvarapala Stone', d: 'Gains 20 armor and magic resist for each enemy within 2 hexes. Regenerates 2% max HP per second.', p: { gargoyle: 20, regen: 0.02 } }],
    ['flame_core', 'vest', 'belt', ['tank'], 1,
      { n: 'Flame Core', d: 'Every 2 seconds, burns the nearest enemy within 2 hexes.', p: { sunfire: true } },
      { n: 'Eclipse Core', d: 'Every 1.5 seconds, burns a random enemy within 3 hexes. Drawback: the holder burns itself for the first 5 seconds.', p: {}, x: { hp: 150 } },
      { n: 'Sunlight Core', d: 'Every 2 seconds, burns the nearest enemy within 2 hexes, and every 1.5 seconds a random enemy within 4 hexes. Regenerates 0.5% max HP per second.', p: { sunfire: true, regen: 0.005 } }],
    ['still_shroud', 'vest', 'glove', ['tank'], 0,
      { n: 'Still Shroud', d: 'At the start of combat, enemies in the holder\'s column need 35% more mana for their first cast.', p: {} },
      { n: 'Dark Shroud', d: 'At the start of combat, enemies in the holder\'s column need 80% more mana for their first cast. Drawback: allies in that column need 40% more.', p: {} },
      { n: 'Reverent Shroud', d: 'At the start of combat, enemies in the holder\'s column and the columns next to it need 50% more mana for their first cast. Allies start with 15 more mana.', p: {} }],
    ['dragon_scale', 'cloak', 'cloak', ['tank'], 1,
      { n: 'Dragon Scale', d: 'A lot of magic resist.', p: {}, x: { mr: 70 } },
      { n: 'Refracted Scale', d: 'Takes 20% less magic damage, and critical strikes deal no extra damage to the holder. Drawback: no extra magic resist.', p: {}, x: { hp: 150 } },
      { n: 'Dragon Will', d: 'A huge amount of magic resist. Regenerates 2% max HP per second.', p: { regen: 0.02 }, x: { mr: 200 } }],
    ['wind_ward', 'cloak', 'belt', ['tank'], 0,
      { n: 'Wind Ward', d: 'At the start of combat, the enemy facing the holder across the board is stunned for 4 seconds, even if immune.', p: {} },
      { n: 'Turbulent Ward', d: 'At the start of combat, the enemy facing the holder across the board is stunned for 7 seconds, even if immune. Drawback: it then gains 30% attack damage and 30 ability power.', p: {}, x: { hp: 150 } },
      { n: 'Mistral Ward', d: 'At the start of combat, the enemy facing the holder across the board is stunned for 6 seconds, even if immune, and your team gains 10% attack speed.', p: {} }],
    ['quick_silver', 'cloak', 'glove', ['ad', 'ap'], 0,
      { n: 'Quick Silver', d: 'Immune to stuns for the first 12 seconds.', p: { ccImmune: 12 }, x: { as: 0.1 } },
      { n: 'Caustic Silver', d: 'Immune to stuns for the whole combat. Drawback: 12% less max HP.', p: { ccImmune: 99 }, x: { as: 0.2 } },
      { n: 'Quickest Silver', d: 'Immune to stuns for the first 30 seconds.', p: { ccImmune: 30 }, x: { as: 0.45 } }],
    ['giga_belt', 'belt', 'belt', ['tank'], 2,
      { n: 'Giga Belt', d: 'A lot of max HP.', p: {}, x: { hp: 450 } },
      { n: 'Sacrifice Belt', d: 'A huge amount of max HP. Drawback: loses 1.5% max HP every second.', p: {}, x: { hp: 1100 } },
      { n: 'Pride Belt', d: 'An enormous amount of max HP. Regenerates 1% max HP per second.', p: { regen: 0.01 }, x: { hp: 1400 } }],
    ['trap_claw', 'belt', 'glove', ['tank'], 0,
      { n: 'Trap Claw', d: 'Blocks the first magic hit from an enemy and stuns the attacker for 3 seconds.', p: {} },
      { n: 'Banshee Claw', d: 'The holder and allies next to it in the same row each block the first magic hit from an enemy. Drawback: no stun.', p: {}, x: { hp: 150, mr: 20 } },
      { n: 'Banshee Silence', d: 'Every ally blocks the first magic hit from an enemy, and the holder stuns that attacker for 3 seconds.', p: {}, x: { hp: 150 } }],
    ['thief_gloves', 'glove', 'glove', ['ad'], 0,
      { n: 'Thief Gloves', d: 'Each combat, gains two random completed items.', p: { thief: true } },
      { n: 'Trickster Gloves', d: 'Each combat, gains two random completed items, plus 25% attack damage and 25 ability power. Drawback: 10% less max HP.', p: { thief: true }, x: { ad: 0.25, ap: 25 } },
      { n: 'Rascal Gloves', d: 'Each combat, gains two random completed items.', p: { thief: true }, x: { crit: 0.3, ad: 0.25, ap: 25, hp: 200 } }],
  ];

  const statSum = (...list) => { const o = {}; for (const s of list) for (const k in s || {}) o[k] = +((o[k] || 0) + s[k]).toFixed(3); return o; };
  const recipes = [];
  const list = [];
  const SHADOW_OF = {}; // base id -> shadow id
  const RADIANT_OF = {}; // base id -> radiant id
  const BASE_OF = {}; // shadow/radiant id -> base id
  for (const [key, a, b, tags, rank, base, sh, ra] of ROWS) {
    const id = `rec_${key}`;
    const sid = `rec_s_${key}`;
    const rid = `rec_r_${key}`;
    SHADOW_OF[id] = sid; RADIANT_OF[id] = rid; BASE_OF[sid] = id; BASE_OF[rid] = id;
    recipes.push({ id, a: `rec_${a}`, b: `rec_${b}`, name: base.n, desc: base.d, p: base.p, extra: base.x || {}, tags, aiRank: rank });
    const cs = statSum(COMP[a][2], COMP[b][2]);
    list.push({ id: sid, name: sh.n, desc: sh.d, cat: 'shadow', p: sh.p, stats: statSum(cs, sh.x), from: a === b ? [`rec_s${a}`, `rec_s${a}`] : [`rec_s${a}`, `rec_${b}`], tags, aiRank: rank,
      // every pair that makes the Shadow item: Spore + normal either way, or two Spore components
      recipes: [[`rec_s${a}`, `rec_${b}`], [`rec_${a}`, `rec_s${b}`], [`rec_s${a}`, `rec_s${b}`]],
      color: '#3a1460', color2: COMP[a][1], short: sh.n.split(' ').map((w) => w[0]).join('') });
    list.push({ id: rid, name: ra.n, desc: ra.d, cat: 'radiant', p: ra.p, stats: statSum(cs, ra.x), from: null, noDrop: true, tags, aiRank: rank + 5,
      color: '#ffe680', color2: COMP[a][1], short: ra.n.split(' ').map((w) => w[0]).join('') });
  }
  // consumables (use() in reckoning-mech.js)
  list.push(
    { id: 'rec_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'rec_tome', consumable: true, name: 'Tome of Traits', short: 'TT', color: '#c9a3ff', color2: '#4a2c6a', stats: {}, p: {},
      desc: 'Drag onto any Digimon to open a pick of 4 Crests, including Crests no recipe makes.' },
  );

  // Digi-Egg + component = Crest. Spore components make the Shadow Spatula Crests of Set 5.
  const emblems = {
    rec_blade: 'SKR', rec_bow: 'LEG', rec_rod: 'SPW', rec_tear: 'REN', rec_vest: 'IRN', rec_cloak: 'SER', rec_belt: 'DAY', rec_glove: 'ASN',
    rec_sblade: 'DSP', rec_sbow: 'TRK', rec_srod: 'HNT', rec_stear: 'WIT', rec_svest: 'CAV', rec_scloak: 'PHA', rec_sbelt: 'NGT', rec_sglove: 'CHI',
  };
  // Crests with no recipe (Tome of Traits, Armory, loot)
  const dropEmblems = ['SEN', 'DRK', 'KNT', 'MYS', 'RNG', 'BRW', 'INV', 'CAN'];

  // What the AI builds for each job, best first: Radiant, then the regular items, then Shadow items.
  const BASE_ORDER = {
    ap: ['jewel_gauntlet', 'seraph_staff', 'ultima_wand', 'mana_overflow', 'gunblade', 'flame_tome', 'mana_lance', 'rage_wing', 'justice_hand', 'quick_silver'],
    ad: ['crit_edge', 'titan_slayer', 'victory_sword', 'rage_wing', 'blood_fang', 'armor_breaker', 'twin_shot', 'rapid_cannon', 'justice_hand', 'gunblade', 'volt_disk', 'titan_will', 'guardian_feather', 'quick_silver', 'thief_gloves'],
    tank: ['spike_plate', 'stone_hide', 'dragon_scale', 'flame_core', 'giga_belt', 'guardian_feather', 'titan_will', 'iron_locket', 'holy_ring', 'frost_heart', 'trap_claw', 'void_portal', 'war_banner', 'power_chalice', 'wind_ward', 'mana_zap', 'still_shroud'],
  };
  const aiRoles = {};
  for (const job of ['ap', 'ad', 'tank']) {
    const keys = BASE_ORDER[job];
    aiRoles[job] = keys.map((k) => `rec_r_${k}`).concat(keys.map((k) => `rec_${k}`), keys.map((k) => `rec_s_${k}`));
  }

  // ---------- augments (effects in reckoning-mech.js) ----------
  const A = (id, tier, name, desc, tags) => ({ id, tier, name, desc, tags });
  const augments = [
    A('rec_a_coins', 1, 'Bit Pouch', 'Gain 8 gold.', ['econ']),
    A('rec_a_cache', 1, 'Spore Cache', 'Gain 2 random Spore components.', ['items']),
    A('rec_a_scan', 1, 'Quick Scan', 'Gain 1 free reroll every round.', ['econ']),
    A('rec_a_heart', 1, "Tamer's Resolve", 'Gain 4 Tamer HP.', ['combat']),
    A('rec_a_training', 1, 'Training Disk', 'Your team gains 10% attack damage and 10 ability power.', ['combat']),
    A('rec_a_mist', 1, 'Black Mist', 'When one of your Digimon is knocked out, enemies within 2 hexes of it take 150 magic damage.', ['combat']),
    A('rec_a_front', 1, 'Iron Front', 'Digimon in your front row gain 25 armor, 25 magic resist and 150 HP.', ['combat']),
    A('rec_a_nest', 1, 'Sniper Nest', 'Digimon in your back two rows gain 20% attack speed.', ['combat']),
    A('rec_a_bank', 2, 'Digi-Bank', 'Gain 16 gold.', ['econ']),
    A('rec_a_trove', 2, 'Shadow Trove', 'Gain a random Shadow item and a component.', ['items']),
    A('rec_a_egg', 2, 'Egg Hunt', 'Gain a Digi-Egg.', ['items']),
    A('rec_a_income', 2, 'Steady Data', 'Gain 2 extra gold every round.', ['econ']),
    A('rec_a_xanti', 2, 'X-Antibody', 'The drawbacks of your Shadow items are halved, and your Digimon holding a Shadow item gain 150 HP.', ['combat']),
    A('rec_a_vamp', 2, 'Celestial Blessing', 'Your team heals for 12% of damage dealt.', ['combat']),
    A('rec_a_mana', 2, 'Overcharge', 'Your team starts combat with 25 extra mana.', ['combat']),
    A('rec_a_unity', 2, 'Stand United', 'Your team gains 3% attack damage and 3 ability power for each active trait.', ['combat']),
    A('rec_a_ascend', 2, 'Ascension', 'After 12 seconds of combat, your team deals 35% more damage.', ['combat']),
    A('rec_a_jackpot', 3, 'Jackpot', 'Gain 30 gold.', ['econ']),
    A('rec_a_blessing', 3, 'Early Blessing', 'Receive your Radiant Blessing at the start of the next free round instead of waiting until you drop low.', ['items']),
    A('rec_a_armory', 3, 'Big Armory', 'Gain 2 random completed items.', ['items']),
    A('rec_a_nature', 3, 'Force of Nature', 'Gain a Force of Nature (+1 team size).', ['items']),
    A('rec_a_darkness', 3, 'Force of Darkness', 'Gain +2 team size. Drawback: every loss costs you twice the usual Tamer HP.', ['combat']),
    A('rec_a_giant', 3, 'Giant Grab', 'Your team gains 25% max HP.', ['combat']),
    A('rec_a_tome', 3, 'Tome Collection', 'Gain 2 Tomes of Traits and a Digi-Egg.', ['items']),
  ];

  // ---------- rules: Hyper Roll (Set 5 launched the mode), Set 5 odds at levels 7-8 ----------
  const rules = {
    startHp: 20,
    startGold: 3,
    planTime: 30,
    itemCap: 10,
    rerollCost: 1,
    winGold: 2,
    shopSize: 5,
    poolSize: [0, 30, 25, 18, 10, 9],
    shopOdds: [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [75, 25, 0, 0, 0], [55, 30, 15, 0, 0], [45, 33, 20, 2, 0],
      [30, 40, 25, 5, 0], [19, 35, 30, 15, 1], [15, 20, 35, 25, 5], [15, 20, 25, 30, 10], [5, 10, 20, 40, 25]],
    levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
    incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
    lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
    pveLossDamage: (stage) => (stage === 1 ? 1 : null),
    roundsInStage: (stage) => (stage === 1 ? 3 : 4),
    // 1-1 opening carousel, 3-1 Shadow carousel; x-4 wild Digimon
    roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : stage === 3 && round === 1 ? 'carousel' : round === 4 ? 'pve' : 'pvp'),
    carouselTiers: (stage) => (stage <= 1 ? [1, 2] : [2, 3, 4]),
    armoryStages: [],
    augmentStages: [2, 3, 4],
    augmentRound: 2,
    offers: [
      { kind: 'armory', stage: 2, round: 1 },
      { kind: 'armory', stage: 3, round: 3 },
      { kind: 'rec_radiant', stage: 4, round: 1, rerolls: 0 },
      { kind: 'armory', stage: 5, round: 1 },
      { kind: 'armory', stage: 6, round: 1 },
    ],
    carouselItems: (game, stage) => DT.Reckoning.carouselItem(game, stage),
    copies: 3,
    // Reckoning keys (read by reckoning-mech.js)
    blessAt: 8, // Radiant Blessing: once, at the first free round after dropping to this HP or lower
    blessGold: 2,
    shadowCarouselChance: 0.5, // chance each 3-1 carousel item is a Spore component
    shadowDropChance: 0.3, // chance a component from loot or the Armory is a Spore component
    pveMul: [1, 1, 1.15, 1.35, 1.45, 1.05, 1.75, 2.1, 2.4, 2.7], // wild Digimon HP and AD by stage
    // Normal pace (standard TFT, 100 HP): the engine's normal rules plus the Armory, the Radiant Armory at 3-6 (as in
    // Set 5.5) and the Blessing at 40 HP. Carousels at x-4 carry Spore components, as in Set 5.
    normal: {
      offers: [{ kind: 'armory', stage: 2, round: 5 }, { kind: 'rec_radiant', stage: 3, round: 6, rerolls: 0 }, { kind: 'armory', stage: 4, round: 5 }, { kind: 'armory', stage: 5, round: 5 }],
      blessAt: 40,
      blessGold: 4,
      pveMul: [1, 1, 1.35, 1.9, 2.2, 1.35, 1.9, 2.2, 2.5, 2.8],
    },
  };

  DT.Sets.register({
    id: 'reckoning',
    name: 'Reckoning',
    mode: 'tft',
    order: 5,
    desc: 'The Dark Spore spreads: Shadow items trade a drawback for power, and the Radiant Blessing lifts Tamers who fall low. Hyper Roll rules.',
    colors: { main: '#8a3cff', accent: '#ffe680' },
    source: 'TFT Set 5 Reckoning and Set 5.5 Dawn of Heroes. docs/inspo/tft/set-05-reckoning.md, docs/inspo/tft/set-05-5-dawn-of-heroes.md',
    traits: TRAITS,
    lines: LINES,
    // loot, Armory, carousel items and codex text live in reckoning-mech.js (DT.Reckoning)
    pve: { wave: PVE, reward: (c, won) => DT.Reckoning.pveReward(c, won) },
    codexRules: (H, pace) => DT.Reckoning.codexRules(H, pace),
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'rec_egg',
      emblems,
      dropEmblems,
      dropEmblemSource: 'the Tome of Traits, the Armory and loot', // codex: where drop-only Crests come from
      emblemName: (T) => `${T.name} Crest`,
      special: [
        { id: 'rec_golden_egg', name: 'Force of Nature', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {},
          from: ['rec_egg', 'rec_egg'], color: '#ffd84a', color2: '#fff3b0', short: 'FN' },
      ],
      list,
      aiRoles,
      armory: (c) => DT.Reckoning.armory(c),
      shadowOf: SHADOW_OF,
      radiantOf: RADIANT_OF,
      baseOf: BASE_OF,
    },
    augments,
    rules,
    tamers: ['Takato', 'Rika', 'Henry', 'Ryo', 'Kazu', 'Jeri', 'Suzie'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
