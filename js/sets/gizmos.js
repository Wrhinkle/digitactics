// Gizmos & Gadgets: TFT Set 6 Gizmos & Gadgets and Set 6.5 Neon Nights, adapted to Digimon.
// 60 shop lines (13/13/13/11/10), 27 traits (12 origins, 3 uniques, 12 classes), 8 components plus the Blank Chip,
// 36 recipes, 36 Overclocked (radiant) versions, 8 crafted and 13 drop-only Chips (emblems), the Golden Chip,
// 4 consumables and 58 Digivice upgrades (augments) in Silver / Gold / Prismatic tiers, picked at 2-1, 3-2 and 4-2.
// Hextech Gates (the 6.5 idea): the tiers of all three upgrade rounds are rolled for the whole lobby at the start of
// the game and shown on every upgrade card; Silver gates also pay out a component and gold.
// Hyper Roll rules (8 players). Behavior is in gizmos-mech.js, art and move looks in gizmos-art.js.
// Design notes and balance results: docs/sets/gizmos.md. Corpus: docs/inspo/tft/set-06*.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;
  const P = (v) => Math.round(v * 100);

  // ---------- traits ----------
  // lv[i] is the main value at breakpoint i; extra per-breakpoint arrays are read with c.val(key).
  const TRAITS = {
    // ---- origins ----
    MASQ: { name: 'Masquerade', kind: 'origin', color: '#e05ab8', bp: [3, 5, 7], lv: [130, 300, 550], ap: [12, 30, 50], icon: 'MQ', crestSymbol: 'chess',
      desc: 'Masquerade Digimon gain max HP and ability power. The one holding the most items is the VIP and gains 50% more.',
      fmt: (v, i) => `+${v} HP, +${TRAITS.MASQ.ap[i]} AP (VIP x1.5)` },
    DB: { name: 'D-Brigade', kind: 'origin', color: '#5a8fd8', bp: [2, 4], lv: [2, 2], as: [0, 0.15], second: [0, 1], icon: 'DB', crestSymbol: 'badge',
      desc: 'At the start of combat, D-Brigade stun the enemy with the most HP for 2 seconds. It breaks free early after losing 40% of its max HP. With 4, they also stun the enemy with the most attack damage for 1 second, and D-Brigade gain 15% attack speed.',
      fmt: (v, i) => (i ? 'stun 2 enemies, +15% AS' : 'stun the biggest enemy') },
    CLOCK: { name: 'Clockwork', kind: 'origin', color: '#d8b04a', bp: [2, 4], lv: [0.18, 0.42], per: [0.025, 0.05], icon: 'CW', crestSymbol: 'refresh',
      desc: 'Your whole team gains attack speed, plus more for each Digivice upgrade you own.',
      fmt: (v, i) => `team +${P(v)}% AS, +${P(TRAITS.CLOCK.per[i])}% per upgrade` },
    DM: { name: 'Dark Masters', kind: 'origin', color: '#7a3ad0', bp: [3, 5, 7], lv: [40, 50, 75], vamp: [0.16, 0.2, 0.3], icon: 'DM', crestSymbol: 'moon',
      desc: 'Shadows cloak some of your Digimon at the start of combat, giving armor, magic resist and omnivamp. With 3: the two Dark Masters with the lowest max HP. With 5: every Dark Master. With 7: your whole team, with stronger shadows.',
      fmt: (v, i) => `${['2 weakest Dark Masters', 'all Dark Masters', 'whole team'][i]}: +${v} armor/MR, ${P(TRAITS.DM.vamp[i])}% omnivamp` },
    JUNK: { name: 'Junkyard', kind: 'origin', color: '#b07a3a', bp: [2, 4, 6], lv: [15, 25, 40], conv: [1, 2, 4], armor: [15, 30, 50], icon: 'JY', crestSymbol: 'sludge',
      desc: 'Your team gains a shield for every component on your board (a completed item counts as 2). Up to 1, 2 or 4 components held by Junkyard Digimon also work as a random completed item made from them for that combat, and Junkyard Digimon gain armor and magic resist.',
      fmt: (v, i) => `${v} shield per component, ${TRAITS.JUNK.conv[i]} component${i ? 's' : ''} upgraded, +${TRAITS.JUNK.armor[i]} armor/MR` },
    PIR: { name: 'Pirates', kind: 'origin', color: '#2fb7a8', bp: [3, 5], lv: [0.18, 0.28], icon: 'PI', crestSymbol: 'coin',
      desc: 'Pirates deal more damage. Before each player combat, dice add loot to a treasure chest. Win a player combat to open it. The longer it stays shut, the luckier the dice. With 5 the dice are luckier.',
      fmt: (v, i) => `+${P(v)}% damage, ${i ? 'lucky' : 'normal'} dice` },
    CD: { name: 'Chrome Digizoid', kind: 'origin', color: '#4fd0ff', bp: [2, 4, 6], lv: [100, 170, 290], dmg: [15, 30, 50], icon: 'CD', crestSymbol: 'shieldIron',
      desc: 'At the start of combat and every 6 seconds, Chrome Digizoid Digimon gain a shield for 4 seconds. While shielded, their attacks deal bonus magic damage. Both are 15% stronger for each Digivice upgrade you own.',
      fmt: (v, i) => `${v} shield, +${TRAITS.CD.dmg[i]} magic on hit` },
    MUT: { name: 'Mutation', kind: 'origin', color: '#9ad43a', bp: [3, 5], lv: [1, 2], icon: 'MU', crestSymbol: 'bug',
      desc: 'Each game rolls one mutation for every Mutation Digimon in the lobby: Cybernetic (bonus HP and attack damage with an item), Voracious (stronger when allies fall), Voidborne (execute low-HP enemies), Adrenal (extra attacks), Synaptic (lower mana cost), Leeching (omnivamp for the team) or Metamorphosis (stats that grow during combat). The mutation is shown above them when combat starts.',
      fmt: (v, i) => (i ? 'strong mutation' : 'mutation') },
    PET: { name: 'Petit', kind: 'origin', color: '#ff9ac8', bp: [3, 6], lv: [0.2, 0.35], icon: 'PT', crestSymbol: 'egg',
      desc: 'Petit Digimon gain dodge chance. After every player combat, a random Petit Digimon joins your bench for free. With 6, Petit abilities also cost 20% less mana.',
      fmt: (v, i) => `${P(v)}% dodge, a free Petit each round${i ? ', -20% mana' : ''}` },
    XA: { name: 'X-Antibody', kind: 'origin', color: '#3fe07a', bp: [3, 5, 7], lv: [0.15, 0.3, 0.5], regen: [0.015, 0.025, 0.04], icon: 'XA', crestSymbol: 'drop',
      desc: 'The first time an X-Antibody Digimon drops below 75% HP, it gains attack speed, takes 15% less damage and regenerates max HP every second for 8 seconds.',
      fmt: (v, i) => `+${P(v)}% AS, ${pct(TRAITS.XA.regen[i])} HP/s for 8s` },
    SIS: { name: 'Sisters', kind: 'unique', color: '#f0f0f0', bp: [1, 2], lv: [0, 0.15], icon: 'SI', crestSymbol: 'cross',
      desc: 'Sistermon Blanc needs 20 less mana to cast. Sistermon Noir gains 40% attack speed for 3 seconds after a takedown. With both sisters, they also deal 15% more damage.',
      fmt: (v, i) => (i ? 'both bonuses, +15% damage' : 'own bonus') },
    IDOL: { name: 'Idols', kind: 'origin', color: '#ffd23f', bp: [1, 2, 3], lv: [0.15, 0.15, 0.15], mana: [0, 5, 5], vamp: [0, 0, 0.2], icon: 'ID', crestSymbol: 'star5',
      desc: 'At the start of combat, your Digimon holding the most items steps into the spotlight. It deals 15% more damage. With 2 Idols it also gains 5 mana every second, and with 3 it heals for 20% of the damage it deals.',
      fmt: (v, i) => ['spotlight: +15% damage', '+5 mana/s', '+20% omnivamp'][i] },
    // ---- uniques ----
    GLUT: { name: 'Glutton', kind: 'unique', color: '#5aa05a', bp: [1], lv: [1], icon: 'GL', crestSymbol: 'plus',
      desc: 'Every time you sell a Digimon while you own Gekomon, Gekomon permanently gains 20 max HP and 1 ability power per gold the sold Digimon was worth (up to 1200 HP and 60 AP).',
      fmt: () => 'grows when you sell' },
    TRANS: { name: 'Mode Change', kind: 'unique', color: '#ff6a3a', bp: [1], lv: [1], icon: 'MC', crestSymbol: 'up',
      desc: 'Justimon changes arms by position. In the front two rows it fights in melee with 40 bonus armor and magic resist, and its ability cuts nearby enemies\' armor and magic resist by 30%. In the back two rows it attacks from 4 hexes away, and its ability gives allies in its row 40% attack speed for 4 seconds.',
      fmt: () => 'melee in front, ranged in back' },
    MIND: { name: 'Mastermind', kind: 'unique', color: '#8a2a5a', bp: [1], lv: [30], icon: 'MM', crestSymbol: 'skull',
      desc: 'At the start of combat, Myotismon gives the two allies directly in front of it 30 mana.',
      fmt: (v) => `+${v} mana to 2 allies` },
    // ---- classes ----
    ARC: { name: 'Arcanist', kind: 'class', color: '#c48bff', bp: [2, 4, 6], lv: [20, 20, 50], own: [0, 40, 50], icon: 'AR', crestSymbol: 'star6',
      desc: 'Your team gains ability power. Arcanists gain extra.',
      fmt: (v, i) => `team +${v} AP${TRAITS.ARC.own[i] ? `, Arcanists +${TRAITS.ARC.own[i]} more` : ''}` },
    ASN: { name: 'Assassin', kind: 'class', color: '#ff4f7b', bp: [2, 4, 6], lv: [0.15, 0.35, 0.55], critDmg: [0.25, 0.45, 0.7], icon: 'AS', crestSymbol: 'dagger',
      desc: 'Assassins\' abilities can critically strike, and they gain critical strike chance and critical strike damage. Assassins with the Assassin stat template leap to the enemy backline when combat starts.',
      fmt: (v, i) => `+${P(v)}% crit, +${P(TRAITS.ASN.critDmg[i])}% crit damage` },
    BG: { name: 'Bodyguard', kind: 'class', color: '#c9a66b', bp: [2, 4], lv: [30, 70], shield: [80, 220], icon: 'BG', crestSymbol: 'shield',
      desc: 'Bodyguards gain armor. Shortly after combat starts, they gain a shield and taunt adjacent enemies.',
      fmt: (v, i) => `+${v} armor, ${TRAITS.BG.shield[i]} shield and taunt` },
    BRU: { name: 'Bruiser', kind: 'class', color: '#d9773a', bp: [2, 4, 6], lv: [100, 200, 350], icon: 'BR', crestSymbol: 'plus',
      desc: 'Your team gains max HP. Bruisers gain double.',
      fmt: (v) => `team +${v} HP, Bruisers +${v * 2}` },
    CHAL: { name: 'Challenger', kind: 'class', color: '#f4d35e', bp: [2, 4, 6], lv: [0.25, 0.5, 0.8], icon: 'CH', crestSymbol: 'sword',
      desc: 'Challengers gain attack speed. After a takedown, they double this bonus for 2.5 seconds.',
      fmt: (v) => `+${P(v)}% AS, doubled after a takedown` },
    COL: { name: 'Colossus', kind: 'class', color: '#8a8f9c', bp: [2, 3], lv: [0.15, 0.2], hp: [0, 0.25], icon: 'CO', crestSymbol: 'chevrons',
      desc: 'Colossi are huge: they take 2 team slots, have bonus HP and cannot be stunned. With 2, Colossi take 15% less damage. With 3, they take 20% less and gain 25% max HP.',
      fmt: (v, i) => `-${P(v)}% damage taken${i ? ', +25% max HP' : ''}`,
      notes: 'The bonus HP and stun immunity are always on, even with one Colossus.' },
    ENCH: { name: 'Enchanter', kind: 'class', color: '#9cf6c0', bp: [2, 3, 4], lv: [10, 20, 30], ap: [10, 20, 30], heal: [0.06, 0.12, 0.18], icon: 'EN', crestSymbol: 'pearl',
      desc: 'Your team gains magic resist and all healing on your team is stronger. Enchanters gain ability power.',
      fmt: (v, i) => `team +${v} MR, +${P(TRAITS.ENCH.heal[i])}% healing, Enchanters +${TRAITS.ENCH.ap[i]} AP` },
    INNO: { name: 'Innovator', kind: 'class', color: '#ff8a3a', bp: [3, 5, 7], lv: [0, 1, 2], icon: 'IN', crestSymbol: 'chip',
      desc: 'Innovators build a machine that joins the fight: Tankmon with 3, Megadramon with 5, Metallicdramon with 7. It gets more HP and attack damage for every star level of your Innovators.',
      fmt: (v) => ['Tankmon', 'Megadramon', 'Metallicdramon'][v] },
    SCH: { name: 'Scholar', kind: 'class', color: '#7ab0ff', bp: [2, 4], lv: [4, 10], icon: 'SC', crestSymbol: 'drop',
      desc: 'Your whole team gains mana every 2 seconds.',
      fmt: (v) => `team +${v} mana every 2s` },
    SNI: { name: 'Sniper', kind: 'class', color: '#5ad1ff', bp: [2, 4], lv: [0.1, 0.2], icon: 'SN', crestSymbol: 'crosshair',
      desc: 'Snipers deal more damage for each hex between them and their target.',
      fmt: (v) => `+${P(v)}% damage per hex` },
    STR: { name: 'Striker', kind: 'class', color: '#ff8f5a', bp: [2, 4], lv: [0.35, 0.8], icon: 'ST', crestSymbol: 'sword',
      desc: 'Strikers gain attack damage.',
      fmt: (v) => `+${P(v)}% AD` },
    TWIN: { name: 'Twinshot', kind: 'class', color: '#ffb02e', bp: [2, 4], lv: [0.4, 0.7], ad: [0.05, 0.3], icon: 'TW', crestSymbol: 'copy',
      desc: 'Twinshots gain attack damage, and each basic attack has a chance to hit twice.',
      fmt: (v, i) => `+${P(TRAITS.TWIN.ad[i])}% AD, ${P(v)}% double hit` },
  };
  // codex breakpoint text from the numbers above
  for (const [id, T] of Object.entries(TRAITS)) T.bpDesc = T.lv.map((v, i) => T.fmt(v, i));

  // ---------- lines ----------
  // L(id, tier, traits, attr, forms, abilities, type, opts). src: the corpus champion slot each line adapts.
  const L = (id, tier, traits, attr, forms, abil, type, opt = {}) => DT.line(id, tier, traits, attr, forms, abil, type, { unassigned: true, ...opt });
  const COLOSSUS = { slots: 2, hpMul: 1.55, adMul: 1.2 };
  const LINES = [
    // ---- 1 cost (13) ----
    L('giz_meramon', 1, ['MASQ', 'ARC'], 'Data', ['Meramon', 'SkullMeramon', 'Volcanicdramon'], ['Burning Fist', 'Heavy Metal Fire', 'Volcanic Flare'], 'nuke', { role: 'Mystic', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6.5 Brand' }),
    L('giz_commandramon', 1, ['DB', 'SNI'], 'Virus', ['Commandramon', 'Sealsdramon', 'Tankdramon'], ['M16 Assassin', 'Death Behind', 'Striver Cannon'], 'snipe', { role: 'Sniper', src: 'TFT6 Caitlyn' }),
    L('giz_zubamon', 1, ['CLOCK', 'CHAL'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Duramon'], ['Twenty Dive', 'Road T(w)o Decade', 'Glass Slash'], 'slam', { role: 'Duelist', src: 'TFT6 Camille' }),
    L('giz_goblimon', 1, ['DM', 'BG'], 'Virus', ['Goblimon', 'Ogremon', 'SkullSatamon'], ['Goblin Strike', 'Pummel Whack', 'Skull Hammer'], 'drain', { role: 'Guardian', radius: 1, hpMul: 0.9, src: 'TFT6 Darius' }),
    L('giz_toyagumon', 1, ['JUNK', 'INNO'], 'Vaccine', ['ToyAgumon', 'Volcamon', 'PileVolcamon'], ['Plastic Blaze', 'Big Bang Voice', 'Volcanic Driver'], 'nuke', { role: 'Sniper', src: 'TFT6 Ezreal' }),
    L('giz_syakomon', 1, ['PIR', 'BRU'], 'Virus', ['Syakomon', 'Coelamon', 'Dragomon'], ['Black Pearl Blast', 'Fossil Bite', 'Forbidden Trident'], 'drain', { role: 'Bruiser', hpMul: 1.1, src: 'TFT6 Illaoi' }),
    L('giz_kotemon', 1, ['CD', 'STR'], 'Data', ['Kotemon', 'Gladimon', 'Knightmon'], ['Hot Head', 'Wheel Rush', 'Grand Smash'], 'slam', { role: 'Striker', hpMul: 1.1, adMul: 1.2, src: 'TFT6.5 Jarvan IV' }),
    L('giz_tsukaimon', 1, ['MUT', 'SCH'], 'Virus', ['Tsukaimon', 'Devimon', 'IceDevimon'], ['Bad Message', 'Death Claw', 'Zero Freeze'], 'weaken', { role: 'Bruiser', src: 'TFT6 Kassadin' }),
    L('giz_bakemon', 1, ['CD', 'ASN'], 'Virus', ['Bakemon', 'Phantomon', 'Beelzemon'], ["Hell's Hand", 'Soul Chopper', 'Darkness Claw'], 'dash', { role: 'Assassin', levels: ['Champion', 'Ultimate', 'Mega'], adMul: 1.1, src: 'TFT6.5 Nocturne' }),
    L('giz_kamemon', 1, ['PET', 'BG'], 'Data', ['Kamemon', 'Ikkakumon', 'Zudomon'], ['Shell Guard', 'Harpoon Torpedo', "Vulcan's Hammer"], 'shield', { role: 'Guardian', src: 'TFT6 Poppy' }),
    L('giz_mushroomon', 1, ['XA', 'INNO'], 'Virus', ['Mushroomon', 'RedVegiemon', 'Asuramon'], ['Fungus Crusher', 'Hazard Breath', 'Fire Fist of Shiva'], 'stun', { role: 'Bruiser', hpMul: 0.9, src: 'TFT6 Singed' }),
    L('giz_chuumon', 1, ['XA', 'ASN'], 'Virus', ['Chuumon', 'Sukamon', 'KingSukamon'], ['Cheese Bomb', 'Poop Blast', 'King Stamp'], 'line', { role: 'Sniper', range: 3, src: 'TFT6 Twitch' }),
    L('giz_mamemon', 1, ['JUNK', 'PET', 'ARC'], 'Data', ['Mamemon', 'MetalMamemon', 'PrinceMamemon'], ['Smiley Bomb', 'Energy Bomb', 'Smile Warhead'], 'aoe', { role: 'Mystic', hpMul: 1.15, levels: ['Ultimate', 'Ultimate', 'Mega'], src: 'TFT6 Ziggs' }),
    // ---- 2 cost (13) ----
    L('giz_seadramon', 2, ['DM', 'SNI'], 'Data', ['Seadramon', 'MegaSeadramon', 'MetalSeadramon'], ['Ice Blast', 'Mega Ice Blast', 'River of Power'], 'line', { role: 'Sniper', hpMul: 1.1, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6.5 Ashe' }),
    L('giz_kokuwamon', 2, ['JUNK', 'BG'], 'Vaccine', ['Kokuwamon', 'Guardromon', 'Andromon'], ['Electro Strike', 'Power Laser', 'Spiral Sword'], 'grab', { role: 'Guardian', src: 'TFT6 Blitzcrank' }),
    L('giz_terriermon', 2, ['PET', 'TWIN'], 'Vaccine', ['Terriermon', 'Gargomon', 'Rapidmon (Armor)'], ['Bunny Blast', 'White Statue', 'Rapid Fire'], 'barrage', { role: 'Sniper', adMul: 1.15, hpMul: 1.1, src: 'TFT6.5 Corki' }),
    L('giz_kazemon', 2, ['PET', 'ENCH'], 'Data', ['Kazemon', 'Zephyrmon', 'Piximon'], ['Tempest Twist', 'Hurricane Gale', 'Pit Bomb'], 'heal', { role: 'Support', levels: ['Human Spirit', 'Beast Spirit', 'Ultimate'], src: 'TFT6 Lulu' }),
    L('giz_kuwagamon', 2, ['MUT', 'STR', 'BRU'], 'Virus', ['Kuwagamon', 'Okuwamon', 'GranKuwagamon'], ['Scissor Claw', 'Double Scissor Claw', 'Dimension Scissor'], 'execute', { role: 'Striker', adMul: 1.1, hpMul: 1.1, levels: ['Champion', 'Ultimate', 'Mega'], src: "TFT6.5 Rek'Sai" }),
    L('giz_kumamon', 2, ['CD', 'DB', 'BRU'], 'Free', ['Kumamon', 'Hyogamon', 'Mammothmon'], ['Snow Bomber', 'Snow Punch', 'Tusk Crusher'], 'stun', { role: 'Bruiser', hpMul: 0.92, levels: ['Human Spirit', 'Champion', 'Ultimate'], src: 'TFT6.5 Sejuani' }),
    L('giz_falcomon', 2, ['CD', 'ARC'], 'Vaccine', ['Falcomon', 'Crowmon', 'Ravemon'], ['Scratch Smash', 'Savage Emperor', 'Blast Wing'], 'drain', { role: 'Mystic', radius: 1, range: 2, src: 'TFT6 Swain' }),
    L('giz_sorcermon', 2, ['MASQ', 'SCH'], 'Data', ['Sorcermon', 'Wizardmon', 'Wisemon'], ['Crystal Cloud', 'Thunder Cloud', 'Pandora Dialogue'], 'barrage', { role: 'Mystic', shots: 5, levels: ['Champion', 'Champion', 'Ultimate'], src: 'TFT6.5 Syndra' }),
    L('giz_dracmon', 2, ['MASQ', 'ASN'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Undead Fang', 'Sticker Blade', 'Thousand Arrows'], 'dash', { role: 'Assassin', hpMul: 1.1, adMul: 1.1, src: 'TFT6 Talon' }),
    L('giz_blackgabumon', 2, ['XA', 'CHAL'], 'Virus', ['BlackGabumon', 'BlackGarurumon', 'BlackMetalGarurumon'], ['Little Horn', 'Freeze Fang', 'Ice Wolf Claw'], 'drain', { role: 'Duelist', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Warwick' }),
    L('giz_solarmon', 2, ['CLOCK', 'INNO'], 'Data', ['Solarmon', 'Clockmon', 'Jijimon'], ['Soul Shocker', 'Chrono Breaker', 'Death Hang'], 'stun', { role: 'Mystic', range: 4, hpMul: 1.15, mana: 55, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Zilean' }),
    L('giz_woodmon', 2, ['DM', 'SCH'], 'Virus', ['Woodmon', 'Cherrymon', 'Puppetmon'], ['Drain Buster', 'Cherry Bomb', 'Flying Cross Cutter'], 'stun', { role: 'Mystic', hpMul: 1.1, mana: 50, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6 Zyra' }),
    L('giz_hawkmon', 2, ['PIR', 'CHAL'], 'Free', ['Hawkmon', 'Aquilamon', 'Gryphonmon'], ['Feather Slash', 'Blast Laser', 'Supersonic Voice'], 'aoe', { role: 'Sniper', hpMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Quinn' }),
    // ---- 3 cost (13) ----
    L('giz_airdramon', 3, ['MUT', 'BRU', 'COL'], 'Data', ['Airdramon', 'Kimeramon', 'Millenniummon'], ['Spinning Needle', 'Heat Viper', 'Dimension Destroyer'], 'execute', { role: 'Bruiser', ...COLOSSUS, hpMul: 1.7, levels: ['Champion', 'Ultimate', 'Mega'], src: "TFT6 Cho'Gath" }),
    L('giz_gumdramon', 3, ['JUNK', 'INNO', 'ASN'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Magnamon X'], ['Ran-gum Break', 'Spin Caliber', 'Plasma Shoot'], 'aoe', { role: 'Assassin', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6.5 Ekko' }),
    L('giz_gizamon', 3, ['PIR', 'TWIN'], 'Virus', ['Gizamon', 'Gesomon', 'MarineDevimon'], ['Spiral Edge', 'Devil Bashing', 'Dark Deluge'], 'nuke', { role: 'Striker', src: 'TFT6 Gangplank' }),
    L('giz_bearmon', 3, ['PET', 'IDOL', 'STR'], 'Vaccine', ['Bearmon', 'Grizzlymon', 'GrapLeomon'], ['Bear Fist', 'Maul Attack', 'The King of Fists'], 'slam', { role: 'Striker', src: 'TFT6.5 Gnar' }),
    L('giz_coronamon', 3, ['MASQ', 'BG'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Corona Flame', 'Flame Dive', 'Sol Blaster'], 'shield', { role: 'Guardian', hpMul: 0.92, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Leona' }),
    L('giz_starmon', 3, ['CD', 'TWIN'], 'Data', ['Agumon', 'Starmon', 'SuperStarmon'], ['Pepper Breath', 'Meteor Shower', 'Super Star Seeker'], 'barrage', { role: 'Sniper', range: 3, src: 'TFT6.5 Lucian' }),
    L('giz_vegiemon', 3, ['MUT', 'ARC'], 'Virus', ['Vegiemon', 'Vademon', 'Ebemon'], ['Sweet Breath', 'Unidentified Flying Kiss', 'Crusher Bone'], 'visions', { role: 'Mystic', range: 4, hpMul: 1.1, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6 Malzahar' }),
    L('giz_penguinmon', 3, ['PIR', 'SNI'], 'Vaccine', ['Penguinmon', 'Whamon', 'Neptunemon'], ['Ice Prism', 'Tidal Wave', 'Gate of Atlantis'], 'barrage', { role: 'Sniper', range: 3, shots: 5, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Miss Fortune' }),
    L('giz_demidevimon', 3, ['DM', 'ENCH'], 'Virus', ['DemiDevimon', 'LadyDevimon', 'Mastemon'], ['Demi Darts', 'Darkness Wave', 'Dark Despair'], 'rally', { role: 'Support', levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT6.5 Morgana' }),
    L('giz_salamon', 3, ['IDOL', 'ENCH'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Petty Punch', "Cat's Eye", 'Celestial Arrow'], 'heal', { role: 'Support', range: 4, mana: 90, src: 'TFT6.5 Senna' }),
    L('giz_blackagumon', 3, ['XA', 'CHAL'], 'Virus', ['BlackAgumon', 'BlackGreymon', 'WarGreymon X'], ['Spitfire', 'Great Antler', 'Terra Force Zero'], 'dash', { role: 'Duelist', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6.5 Tryndamere' }),
    L('giz_impmon', 3, ['PET', 'ARC'], 'Virus', ['Impmon', 'Pumpkinmon', 'Lilithmon'], ['Night of Fire', "Jack O' Lantern", 'Phantom Pain'], 'aoe', { role: 'Mystic', levels: ['Rookie', 'Ultimate', 'Mega'], range: 2, hpMul: 1.25, src: 'TFT6 Vex' }),
    L('giz_numemon', 3, ['XA', 'BRU'], 'Virus', ['Numemon', 'Raremon', 'PlatinumNumemon'], ['Party Time', 'Rotten Tackle', 'Platinum Burp'], 'slam', { role: 'Bruiser', levels: ['Champion', 'Champion', 'Ultimate'], src: 'TFT6 Zac' }),
    // ---- 4 cost (11) ----
    L('giz_youkomon', 4, ['DM', 'ARC'], 'Data', ['Youkomon', 'Kyubimon (Silver)', 'Kuzuhamon'], ['Blazing Spheres', 'Dragon Wheel', 'Taizoukai Mandala'], 'line', { role: 'Mystic', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6.5 Ahri' }),
    L('giz_tyrannomon', 4, ['CD', 'COL'], 'Virus', ['Tyrannomon', 'MetalTyrannomon', 'RustTyranomon'], ['Wild Buster', 'Nuclear Laser', 'Rust Breath'], 'slam', { role: 'Guardian', ...COLOSSUS, radius: 2, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6.5 Alistar' }),
    L('giz_psychemon', 4, ['MASQ', 'CHAL'], 'Data', ['Psychemon', 'Gururumon', 'Astamon'], ['Colored Spark', 'Gururu Thrust', 'Mad Dog Fire'], 'barrage', { role: 'Sniper', adMul: 1.15, src: 'TFT6.5 Draven' }),
    L('giz_shoutmon', 4, ['JUNK', 'STR'], 'Data', ['Shoutmon', 'Shoutmon X3', 'OmniShoutmon'], ['Bellow Blaster', 'Hard Rock Soul', 'Victorize Banking'], 'dash', { role: 'Striker', hpMul: 1.15, adMul: 1.25, src: 'TFT6.5 Irelia' }),
    L('giz_lekismon', 4, ['CLOCK', 'SNI'], 'Data', ['Lekismon', 'Crescemon', 'Dianamon'], ['Tear Arrow', 'Ice Archery', 'Arrow of Artemis'], 'line', { role: 'Sniper', hpMul: 1.1, adMul: 1.1, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6 Jhin' }),
    L('giz_kunemon', 4, ['MUT', 'ASN'], 'Virus', ['Kunemon', 'Flymon', 'TyrantKabuterimon'], ['Electric Thread', 'Poison Sting', 'Insect Cannon'], 'execute', { role: 'Assassin', hpMul: 1.15, adMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'], src: "TFT6.5 Kha'Zix" }),
    L('giz_hagurumon', 4, ['CLOCK', 'ENCH'], 'Vaccine', ['Hagurumon', 'Mekanorimon', 'HiAndromon'], ['Cog Crusher', 'Twinkle Beam', 'Atomic Ray'], 'rally', { role: 'Support', hpMul: 0.9, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Orianna' }),
    L('giz_floramon', 4, ['XA', 'SCH'], 'Data', ['Floramon', 'Kiwimon', 'Lotosmon'], ['Rain of Pollen', 'Blast Shot', 'Serpent Ruin'], 'weaken', { role: 'Mystic', radius: 1, hpMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6.5 Renata Glasc' }),
    L('giz_lalamon', 4, ['IDOL', 'INNO'], 'Data', ['Lalamon', 'Lilamon', 'Rosemon'], ['Sing a Song', 'Lila Shower', 'Forbidden Temptation'], 'heal', { role: 'Support', mana: 85, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT6 Seraphine' }),
    L('giz_dorumon', 4, ['CD', 'STR'], 'Vaccine', ['Dorumon', 'Raptordramon', 'Grademon'], ['Dash Metal', 'Ambush Crunch', 'Cross Blade'], 'chain', { role: 'Striker', range: 3, src: 'TFT6.5 Sivir' }),
    L('giz_sistermonb', 4, ['DB', 'SIS', 'BRU'], 'Vaccine', ['Sistermon Blanc', 'Sistermon Blanc (Awakened)', 'Sistermon Blanc (Awakened)'], ['Divine Pierce', 'Protect Wave', 'Divine Pierce (Awake)'], 'line', { role: 'Bruiser', hpMul: 0.92, levels: ['Rookie', 'Champion', 'Champion'], src: 'TFT6.5 Vi' }),
    // ---- 5 cost (10) ----
    L('giz_gotsumon', 5, ['IDOL', 'BG', 'COL'], 'Virus', ['Gotsumon', 'Golemon', 'BanchoLeomon'], ['Earth Tremor', 'Guardian Bomb', 'Flashy Boss Punch'], 'slam', { role: 'Guardian', ...COLOSSUS, radius: 2, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT6 Galio' }),
    L('giz_justimon', 5, ['DB', 'INNO', 'TRANS'], 'Vaccine', ['Justimon: Blitz Arm', 'Justimon: Critical Arm', 'Justimon: Trinity Arm'], ['Voltage Blitz', 'Thunder Blade', 'Trinity Arm'], 'aoe', { role: 'Striker', hpMul: 1.1, adMul: 1.1, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT6 Jayce' }),
    L('giz_sistermonn', 5, ['JUNK', 'SIS', 'TWIN'], 'Virus', ['Sistermon Noir', 'Sistermon Noir (Awakened)', 'Sistermon Noir (Awakened)'], ['Mickey Bullet', 'Bless Fire', 'Grand Sister Cruz'], 'aoe', { role: 'Sniper', radius: 2, levels: ['Rookie', 'Champion', 'Champion'], src: 'TFT6 Jinx' }),
    L('giz_keramon', 5, ['MUT', 'CHAL'], 'Virus', ['Keramon', 'Infermon', 'Diaboromon'], ['Crazy Giggle', "Hell's Grenade", 'Catastrophe Cannon'], 'barrage', { role: 'Sniper', range: 3, shots: 6, hpMul: 1.25, mana: 60, levels: ['Rookie', 'Ultimate', 'Mega'], src: "TFT6 Kai'Sa" }),
    L('giz_myotismon', 5, ['MIND', 'SCH'], 'Virus', ['Myotismon', 'VenomMyotismon', 'GranDracmon'], ['Crimson Lightning', 'Venom Infusion', 'Crystal Revolution'], 'drain', { role: 'Mystic', radius: 1, hpMul: 0.85, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT6.5 Silco' }),
    L('giz_gekomon', 5, ['PIR', 'BRU', 'GLUT'], 'Virus', ['Gekomon', 'ShogunGekomon', 'ShogunGekomon'], ['Tongue Attack', 'Musical Fist', 'Tonosama Jump'], 'devour', { role: 'Bruiser', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT6 Tahm Kench' }),
    L('giz_datamon', 5, ['XA', 'ARC'], 'Virus', ['Datamon', 'Machinedramon', 'Chaosdromon'], ['Digital Bomb', 'Infinity Cannon', 'Hyper Infinity Cannon'], 'line', { role: 'Mystic', range: 4, hpMul: 1.2, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT6 Viktor' }),
    L('giz_renamon', 5, ['MASQ', 'SNI'], 'Data', ['Renamon', 'Taomon', 'Sakuyamon'], ['Kohenkyo', 'Talisman Spell', 'Izuna'], 'barrage', { role: 'Sniper', range: 5, shots: 5, hpMul: 1.25, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT6.5 Zeri' }),
    L('giz_lucemon', 5, ['PET', 'ARC'], 'Vaccine', ['Lucemon', 'Lucemon Chaos Mode', 'Lucemon Shadowlord Mode'], ['Galaxy Crash', 'Paradise Lost', 'Purgatorial Flame'], 'nuke', { role: 'Mystic', range: 4, hpMul: 1.2, mana: 50, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT6 Veigar' }),
    L('giz_arukenimon', 5, ['DM', 'ASN'], 'Virus', ['Arukenimon', 'Piedmon', 'Boltboutamon'], ['Spider Thread', 'Trump Sword', 'Pernicious Waltz'], 'dash', { role: 'Assassin', hpMul: 1.2, adMul: 1.15, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT6 Akali' }),

    // ---- summons (Innovator machine, Decoy Portal) ----
    L('giz_inno', 3, [], 'Free', ['Tankmon', 'Megadramon', 'Metallicdramon'], ['Sub Machine Gun', 'Ultimate Slicer', 'Laser Cannon'], 'aoe', { role: 'Bruiser', shop: false, levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('giz_decoy', 2, [], 'Free', ['Chrysalimon', 'Chrysalimon', 'Chrysalimon'], ['Data Crusher', 'Data Crusher', 'Data Crusher'], 'weaken', { role: 'Guardian', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    // ---- wild Digimon (PvE) ----
    L('giz_w_guardromon', 1, ['STR'], 'Free', ['Guardromon (Gold)', 'Guardromon (Gold)', 'Guardromon (Gold)'], ['Warning Beam', 'Homing Laser', 'Destruction Grenade'], 'nuke', { role: 'Striker', shop: false, levels: ['Rookie', 'Champion', 'Ultimate'] }),
    L('giz_w_nanimon', 2, ['BRU'], 'Virus', ['Nanimon', 'Nanimon', 'Nanimon'], ['Power Drive', 'Power Drive', 'Power Drive'], 'aoe', { role: 'Bruiser', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('giz_w_gigadramon', 5, ['BRU'], 'Virus', ['Gigadramon', 'Gigadramon', 'Gigadramon'], ['Giga Heat', 'Giga Heat', 'Giga Heat'], 'aoe', { role: 'Bruiser', shop: false, boss: true, hpMul: 1.4, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('giz_w_belphemon', 5, ['BRU'], 'Virus', ['Belphemon Rage Mode', 'Belphemon Rage Mode', 'Belphemon Rage Mode'], ['Lampranthus', 'Lampranthus', 'Lampranthus'], 'slam', { role: 'Bruiser', shop: false, boss: true, radius: 2, hpMul: 1.6, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('giz_w_apocalymon', 5, ['ARC'], 'Virus', ['Apocalymon', 'Apocalymon', 'Apocalymon'], ['Darkness Zone', 'Darkness Zone', 'Darkness Zone'], 'aoe', { role: 'Mystic', shop: false, boss: true, radius: 2, hpMul: 1.8, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('giz_w_ygg', 5, ['ARC'], 'Free', ['Yggdrasil 7D6', 'Yggdrasil 7D6', 'Yggdrasil 7D6'], ['Huge Crystal', 'Huge Crystal', 'Huge Crystal'], 'barrage', { role: 'Mystic', shop: false, boss: true, hpMul: 2, adMul: 1.25, shots: 6, levels: ['Super Ultimate', 'Super Ultimate', 'Super Ultimate'] }),
  ];

  // ---------- wild rounds ----------
  // [lineId, stage, col, row]; rows 4-7 in the wild side's own coordinates (4 = front).
  // Hyper Roll: 1-2, 1-3, then x-4. Normal: 1-2 to 1-4 against fewer units (players start at level 1), and the x-7
  // rounds add one more Guardromon, since boards are further along by then.
  const wave = (stage, round, game) => {
    const g = 'giz_w_guardromon', j = 'giz_w_nanimon';
    const normal = game && game.pace === 'normal';
    if (stage === 1 && normal) return round <= 2 ? [[g, 0, 3, 4]] : round === 3 ? [[g, 0, 2, 4], [g, 0, 4, 4]] : [[g, 0, 2, 4], [g, 0, 3, 4], [g, 0, 4, 4]];
    if (stage === 1) return round === 2 ? [[g, 0, 2, 4], [g, 0, 4, 4]] : [[g, 0, 2, 4], [g, 0, 3, 4], [g, 0, 4, 4]];
    if (normal && stage >= 2) return hyperWave(stage).concat([[g, Math.min(2, stage - 1), 6, 5]]);
    return hyperWave(stage);
  };
  const hyperWave = (stage) => {
    const g = 'giz_w_guardromon', j = 'giz_w_nanimon';
    if (stage === 2) return [[g, 1, 2, 4], [g, 1, 4, 4], [j, 0, 3, 4], [g, 1, 3, 6]];
    // Junk Town
    if (stage === 3) return [[j, 1, 2, 4], [j, 1, 4, 4], [g, 1, 3, 4], [g, 1, 1, 6], [g, 1, 5, 6], [j, 1, 3, 7]];
    // Gigadramon
    if (stage === 4) return [[j, 2, 3, 4], [g, 2, 1, 4], [g, 2, 5, 4], ['giz_w_gigadramon', 1, 3, 6], [g, 1, 2, 7], [g, 1, 4, 7]];
    // Belphemon Rage Mode, Kurata's weapon
    if (stage === 5) return [['giz_w_belphemon', 1, 3, 4], [j, 2, 1, 4], [j, 2, 5, 4], [g, 2, 2, 6], [g, 2, 4, 6], ['giz_w_gigadramon', 1, 3, 7]];
    if (stage === 6) return [[g, 2, 3, 6], ['giz_w_belphemon', 1, 2, 4], ['giz_w_gigadramon', 2, 4, 4], [j, 2, 3, 5], [g, 2, 1, 6], [g, 2, 5, 6], ['giz_w_apocalymon', 1, 3, 7]];
    if (stage === 7) return [['giz_w_belphemon', 2, 3, 4], ['giz_w_gigadramon', 2, 1, 5], ['giz_w_gigadramon', 2, 5, 5], ['giz_w_apocalymon', 1, 3, 6], [g, 2, 2, 7], [g, 2, 4, 7]];
    return [['giz_w_belphemon', 2, 2, 4], ['giz_w_belphemon', 2, 4, 4], ['giz_w_apocalymon', 2, 3, 6], ['giz_w_ygg', 1, 3, 7], ['giz_w_gigadramon', 2, 1, 6], ['giz_w_gigadramon', 2, 5, 6]];
  };

  // Loot: the Hyper Roll default drops, plus an Overclock Kit from won wild rounds in stage 6 and later.
  const reward = (c, won) => {
    const g = c.game, out = [];
    if (g.stage === 1) {
      out.push({ kind: 'blue', items: [c.randomComponent()] });
      if (won) {
        out.push({ kind: 'blue', gold: g.round === g.R.roundsInStage(1) ? 2 : 1 });
        if (g.round === g.R.roundsInStage(1)) {
          const lines = DT.SHOP_LINES.filter((l) => l.tier <= 2 && g.pool[l.id] > 0);
          if (lines.length) out.push({ kind: 'gold', unit: c.pick(lines).id });
        }
      }
      return out;
    }
    out.push({ kind: 'blue', gold: won ? 3 : 2 });
    if (won && g.stage >= 4) out.push({ kind: 'gold', items: [c.randomCompleted()] });
    if (won && g.stage >= 4 && c.rng() < 0.2) out.push({ kind: 'gold', items: [c.randomEmblem(false)] });
    else out.push({ kind: 'blue', items: [c.randomComponent(g.stage >= 3)] });
    if (won && g.stage >= 3 && c.rng() < 0.25) out.push({ kind: 'gold', items: [c.pick(['giz_remover', 'giz_reforger', 'giz_copy'])] });
    if (won && g.stage >= 6 && c.rng() < 0.4) out.push({ kind: 'gold', items: ['giz_ockit'] });
    return out;
  };
  const pve = { wave, reward };

  // ---------- items ----------
  const components = {
    giz_sword: { name: 'Power Cell', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    giz_bow: { name: 'Turbo Gear', short: 'TG', color: '#ffd23f', stats: { as: 0.12 } },
    giz_rod: { name: 'Code Rod', short: 'CR', color: '#b26bff', stats: { ap: 15 } },
    giz_tear: { name: 'Mana Capacitor', short: 'MC', color: '#4fb3ff', stats: { mana: 15 } },
    giz_vest: { name: 'Steel Plate', short: 'SP', color: '#c0c6d0', stats: { armor: 20 } },
    giz_cloak: { name: 'Static Cloak', short: 'SC', color: '#3fd6b0', stats: { mr: 20 } },
    giz_belt: { name: 'Battery Pack', short: 'BP', color: '#ff4d64', stats: { hp: 150 } },
    giz_glove: { name: 'Grip Glove', short: 'GG', color: '#8fdc4e', stats: { crit: 0.1, dodge: 0.08 } },
    giz_chip: { name: 'Blank Chip', short: 'BC', color: '#fff3b0', stats: {}, special: true,
      desc: 'Combine with a component to make a trait Chip, or with another Blank Chip to make a Golden Chip (+1 team size).' },
  };

  // [a, b, name, desc, passives, extraStats, { id, tags, aiRank }]
  const R = (a, b, id, name, desc, p, extra, tags, aiRank = 0) => [`giz_${a}`, `giz_${b}`, name, desc, p, extra || {}, { id: `giz_${id}`, tags, aiRank }];
  const recipes = [
    R('sword', 'sword', 'doom_blade', 'Doom Blade', 'Grants a lot of attack damage.', null, { ad: 0.45 }, ['ad'], 2),
    R('sword', 'bow', 'titan_breaker', 'Titan Breaker', 'Deals 10% more damage, and 35% more again to enemies with 1600 or more max HP.', { amp: 0.1, slayer: 0.35 }, {}, ['ad'], 3),
    R('sword', 'rod', 'gun_blade', 'Gun Blade', 'Heals for 22% of all damage dealt. Magic damage also heals the lowest-health ally for 25% of it.', { vamp: 0.22 }, {}, ['ad', 'ap'], 1),
    R('sword', 'tear', 'mana_spear', 'Mana Spear', 'Basic attacks restore 6 extra mana.', { shojin: 6 }, {}, ['ap'], 1),
    R('sword', 'vest', 'backup_drive', 'Backup Drive', 'The first time the holder would be knocked out, it goes into stasis for 2 seconds instead, then returns with 400 HP.', null, {}, ['tank', 'ad']),
    R('sword', 'cloak', 'blood_fang', 'Blood Fang', 'Heals for 25% of damage dealt. Below 40% HP, gain a 25% max HP shield once.', { vamp: 0.25, lifeline: { at: 0.4, shield: 0.25 } }, {}, ['ad'], 2),
    R('sword', 'belt', 'war_banner', 'War Banner', 'At the start of combat, the holder and allies within 1 hex in the same row gain 30% attack speed.', null, {}, ['tank', 'ad']),
    R('sword', 'glove', 'crit_edge', 'Crit Edge', 'Grants 45% critical strike chance and 10% critical strike damage.', { critDmg: 0.1 }, { crit: 0.45 }, ['ad'], 3),
    R('bow', 'bow', 'rail_cannon', 'Rail Cannon', 'Grants a lot of attack speed and 1 extra hex of attack range.', null, { as: 0.26 }, ['ad'], 1),
    R('bow', 'rod', 'rage_engine', 'Rage Engine', 'Each basic attack grants 6% attack speed for the rest of combat, stacking.', { rage: 0.06 }, {}, ['ad', 'ap'], 2),
    R('bow', 'tear', 'static_shiv', 'Static Shiv', 'Every third basic attack sends lightning to 3 enemies for 70 magic damage.', { volt: 70 }, { as: 0.1 }, ['ad', 'ap']),
    R('bow', 'vest', 'titan_core', 'Titan Core', 'Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times.', { titan: true }, {}, ['tank', 'ad']),
    R('bow', 'cloak', 'twin_turbine', 'Twin Turbine', 'Basic attacks also hit a second nearby enemy for 60% damage.', { split: 0.6 }, {}, ['ad'], 1),
    R('bow', 'belt', 'decoy_portal', 'Decoy Portal', 'At the start of combat, enemies within 4 hexes target the holder. When the holder is knocked out, a Chrysalimon decoy appears in its place.', null, {}, ['tank']),
    R('bow', 'glove', 'armor_piercer', 'Armor Piercer', 'Basic attacks cut the target\'s armor by 40% for 3 seconds.', { shred: 0.4 }, {}, ['ad']),
    R('rod', 'rod', 'mega_rod', 'Mega Rod', 'Grants a lot of ability power.', null, { ap: 45 }, ['ap'], 2),
    R('rod', 'tear', 'seraph_staff', 'Seraph Staff', 'Gain 20 ability power every 5 seconds.', { archangel: 20 }, {}, ['ap'], 3),
    R('rod', 'vest', 'guard_locket', 'Guard Locket', 'At the start of combat, the holder and allies within 2 hexes in the same row gain a shield of 300 (350 at Champion, 400 at Ultimate).', null, {}, ['tank']),
    R('rod', 'cloak', 'ion_spark', 'Ion Spark', 'When an enemy within 2 hexes casts, it takes 160 magic damage.', { zap: 160 }, {}, ['tank']),
    R('rod', 'belt', 'burn_codex', 'Burn Codex', 'All damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, {}, ['ap'], 1),
    R('rod', 'glove', 'jewel_gauntlet', 'Jewel Gauntlet', 'Abilities can critically strike. +30% critical strike damage.', { abilCrit: true, critDmg: 0.3 }, { ap: 10 }, ['ap'], 3),
    R('tear', 'tear', 'mana_overflow', 'Mana Overflow', 'Max mana -10. After casting, keep 20 mana.', { bluebuff: true }, { mana: 15 }, ['ap'], 2),
    R('tear', 'vest', 'frost_heart', 'Frost Heart', 'Enemies within 2 hexes have 35% less attack speed.', null, {}, ['tank']),
    R('tear', 'cloak', 'power_chalice', 'Power Chalice', 'At the start of combat, the holder and allies within 1 hex in the same row gain 30 ability power.', null, {}, ['ap', 'tank']),
    R('tear', 'belt', 'redemption_core', 'Redemption Core', 'Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP.', { redemption: true }, {}, ['tank']),
    R('tear', 'glove', 'justice_glove', 'Justice Glove', 'Deals 12% more damage and heals for 12% of damage dealt.', { amp: 0.12, vamp: 0.12 }, {}, ['ad', 'ap'], 1),
    R('vest', 'vest', 'thorn_mail', 'Thorn Mail', 'When hit by a basic attack, deals 70 magic damage to adjacent enemies (1.5 second cooldown).', { thorns: 70 }, { armor: 30 }, ['tank'], 2),
    R('vest', 'cloak', 'gargoyle_plate', 'Gargoyle Plate', 'Gain 15 armor and magic resist for each enemy within 2 hexes.', { gargoyle: 15 }, {}, ['tank'], 2),
    R('vest', 'belt', 'solar_cape', 'Solar Cape', 'Every 2 seconds, burns the nearest enemy within 2 hexes.', { sunfire: true }, {}, ['tank'], 1),
    R('vest', 'glove', 'static_shroud', 'Static Shroud', 'At the start of combat, enemies in the holder\'s column and the columns next to it lose their mana and need 35% more for their first cast.', null, {}, ['tank']),
    R('cloak', 'cloak', 'dragon_scale', 'Dragon Scale', 'Grants a lot of magic resist and regenerates 1% max HP per second.', { regen: 0.01 }, { mr: 50 }, ['tank'], 1),
    R('cloak', 'belt', 'wind_turbine', 'Wind Turbine', 'At the start of combat, blows the enemy across from the holder out of the fight for 5 seconds. It cannot act or be hurt.', null, {}, ['tank']),
    R('cloak', 'glove', 'quick_silver', 'Quick Silver', 'Immune to stuns for the first 15 seconds.', { ccImmune: 15 }, { as: 0.2 }, ['ad', 'tank']),
    R('belt', 'belt', 'giga_battery', 'Giga Battery', 'Grants a lot of HP.', null, { hp: 700 }, ['tank'], 1),
    R('belt', 'glove', 'spell_shield', 'Spell Shield', 'At the start of combat, the holder and allies within 1 hex in the same row gain a ward that blocks the first 500 magic damage they take in one hit.', null, {}, ['tank']),
    R('glove', 'glove', 'thief_glove', 'Thief Glove', 'Each combat, the holder gains two random completed items.', { thief: true }, {}, ['ad']),
  ];

  // Overclocked (radiant) versions: stronger numbers, never dropped at random. From Overclock Kits, Overclock Lab and late wild rounds.
  const OC_SCALE = 1.6;
  const compStats = (id) => components[id].stats;
  const sumStats = (...list) => { const o = {}; for (const s of list) for (const k in s || {}) o[k] = +(((o[k] || 0) + s[k]).toFixed(3)); return o; };
  const scaleP = (p) => {
    const o = {};
    for (const [k, v] of Object.entries(p || {})) {
      if (typeof v === 'number') o[k] = k === 'ccImmune' ? 30 : +(v * OC_SCALE).toFixed(3);
      else if (k === 'lifeline') o[k] = { ...v, shield: +(v.shield * OC_SCALE).toFixed(3) };
      else o[k] = v;
    }
    return o;
  };
  const overclocked = recipes.map(([a, b, name, desc, p, extra, meta]) => ({
    id: meta.id.replace('giz_', 'giz_oc_'), name: `Overclocked ${name}`, cat: 'radiant', noDrop: true, radiant: true, base: meta.id,
    desc: `${desc} Overclocked: about 60% stronger.`, p: scaleP(p),
    stats: Object.fromEntries(Object.entries(sumStats(compStats(a), compStats(b), extra)).map(([k, v]) => [k, +(v * 1.5).toFixed(3)])),
    tags: meta.tags, aiRank: (meta.aiRank || 0) + 5, color: components[a].color, color2: components[b].color, short: 'O' + name.split(' ').map((w) => w[0]).join(''),
  }));

  const consumables = [
    { id: 'giz_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'giz_reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
    { id: 'giz_copy', consumable: true, name: 'Copy Chip', short: 'CC', color: '#7dffb0', color2: '#2a6a4a', stats: {}, p: {},
      desc: 'Drag onto a Digimon of cost 1 to 4 to put a Rookie-form copy of it on your bench.' },
    { id: 'giz_ockit', consumable: true, name: 'Overclock Kit', short: 'OK', color: '#e58bff', color2: '#4fd0ff', stats: {}, p: {},
      desc: 'Drag onto a Digimon to overclock one of its completed items (the first one that is not overclocked yet).' },
  ];

  // Blank Chip + component = trait Chip (emblem). Chips with no recipe come from the Armory, gold orbs and upgrades.
  const emblems = { giz_sword: 'STR', giz_bow: 'CHAL', giz_rod: 'ARC', giz_tear: 'CD', giz_vest: 'MASQ', giz_cloak: 'DM', giz_belt: 'XA', giz_glove: 'ASN' };
  const dropEmblems = ['BG', 'BRU', 'CLOCK', 'DB', 'INNO', 'JUNK', 'PIR', 'MUT', 'PET', 'SCH', 'SNI', 'ENCH', 'TWIN'];

  const oc = (list) => list.flatMap((id) => [`giz_oc_${id}`, `giz_${id}`]);
  const aiRoles = {
    ap: oc(['jewel_gauntlet', 'seraph_staff', 'mega_rod', 'mana_overflow', 'gun_blade', 'burn_codex', 'mana_spear', 'rage_engine', 'justice_glove', 'static_shiv', 'power_chalice']),
    ad: oc(['crit_edge', 'titan_breaker', 'doom_blade', 'rage_engine', 'blood_fang', 'twin_turbine', 'armor_piercer', 'rail_cannon', 'justice_glove', 'static_shiv', 'quick_silver', 'gun_blade', 'thief_glove']),
    tank: oc(['thorn_mail', 'gargoyle_plate', 'solar_cape', 'dragon_scale', 'frost_heart', 'giga_battery', 'redemption_core', 'guard_locket', 'backup_drive', 'titan_core', 'wind_turbine', 'spell_shield', 'decoy_portal', 'static_shroud', 'war_banner', 'ion_spark']),
  };

  // ---------- Digivice upgrades (augments) ----------
  // tier 1 Silver, 2 Gold, 3 Prismatic. tags: econ (taken early by the AI), items, combat. Hooks in gizmos-mech.js.
  const A = (id, tier, name, desc, tag) => ({ id: `giz_a_${id}`, tier, name, desc, tags: [tag] });
  const augments = [
    // Silver
    A('spare', 1, 'Spare Parts', 'Gain 10 gold.', 'econ'),
    A('parts', 1, 'Parts Bin', 'Gain 2 random components.', 'items'),
    A('feather1', 1, 'Featherweights', 'Your 1 and 2-cost Digimon gain 25% attack speed.', 'combat'),
    A('bless1', 1, 'Celestial Blessing', 'Your team heals for 10% of all damage dealt.', 'combat'),
    A('implant1', 1, 'Cybernetic Implants', 'Your Digimon holding an item gain 200 HP and 10% attack damage.', 'combat'),
    A('thrill1', 1, 'Thrill of the Hunt', 'When your Digimon score a kill, they heal 300 HP.', 'combat'),
    A('knife1', 1, "Knife's Edge", 'Your Digimon that start in the front two rows gain 15% attack damage.', 'combat'),
    A('makeshift1', 1, 'Makeshift Armor', 'Your Digimon with no items gain 30 armor and magic resist.', 'combat'),
    A('exiles1', 1, 'Exiles', 'Your Digimon that start combat with no adjacent allies gain a 35% max HP shield.', 'combat'),
    A('united1', 1, 'Stand United', 'Your team gains 2% attack damage and 2 ability power for every active trait.', 'combat'),
    A('ascend', 1, 'Ascension', 'After 15 seconds of combat, your team deals 40% more damage.', 'combat'),
    A('calc', 1, 'Calculated Loss', 'After you lose a combat, gain 3 gold and a free reroll next round.', 'econ'),
    A('hyper', 1, 'Hyper Roll', 'After each combat, if you have less than 10 gold, gain 2 gold.', 'econ'),
    A('weak', 1, 'Weakspot', 'Your basic attacks cut the target\'s armor by 20% and cut its healing by a third for 5 seconds.', 'combat'),
    A('firstaid', 1, 'First Aid Kit', 'All healing and shields on your team are 30% stronger.', 'combat'),
    A('scoped', 1, 'Scoped Weapons', 'Your ranged Digimon gain 1 hex of attack range.', 'combat'),
    A('underdog', 1, 'Underdogs', 'While you have fewer Digimon alive than your opponent, your team heals 4% max HP every second (up to 150).', 'combat'),
    A('booster', 1, 'Trait Booster', 'Your largest origin trait counts one extra Digimon.', 'combat'),
    A('runic', 1, 'Runic Shield', 'Your Arcanists start combat with a shield equal to 3 times their ability power.', 'combat'),
    A('overload', 1, 'Chemical Overload', 'When your X-Antibody Digimon are knocked out, they explode for 25% of their max HP as magic damage to enemies within 2 hexes.', 'combat'),
    A('sosmall', 1, 'So Small', 'Your Petit Digimon gain 40% dodge chance and 10% attack speed.', 'combat'),
    A('cutthroat', 1, 'Cutthroat', 'The first enemy each of your Assassins hits loses all its mana.', 'combat'),
    // Gold
    A('bank', 2, 'Digi-Bank', 'Gain 18 gold.', 'econ'),
    A('chip', 2, 'Spare Chip', 'Gain a Blank Chip and a random component.', 'items'),
    A('crestkit', 2, 'Crest Kit', 'Gain a Chip (emblem) for an origin trait on your board.', 'items'),
    A('airdrop', 2, 'Binary Airdrop', 'Your Digimon holding 2 or more items gain a random completed item for each combat.', 'combat'),
    A('bless2', 2, 'Celestial Blessing II', 'Your team heals for 18% of all damage dealt.', 'combat'),
    A('implant2', 2, 'Cybernetic Implants II', 'Your Digimon holding an item gain 300 HP and 15% attack damage.', 'combat'),
    A('feather2', 2, 'Featherweights II', 'Your 1 and 2-cost Digimon gain 40% attack speed.', 'combat'),
    A('knife2', 2, "Knife's Edge II", 'Your Digimon that start in the front two rows gain 25% attack damage.', 'combat'),
    A('makeshift2', 2, 'Makeshift Armor II', 'Your Digimon with no items gain 45 armor and magic resist.', 'combat'),
    A('united2', 2, 'Stand United II', 'Your team gains 3% attack damage and 3 ability power for every active trait.', 'combat'),
    A('thrill2', 2, 'Thrill of the Hunt II', 'When your Digimon score a kill, they heal 500 HP.', 'combat'),
    A('trade', 2, 'Trade Sector', 'Gain a free reroll every round.', 'econ'),
    A('salvage', 2, 'Salvage Bin', 'Gain a random completed item.', 'items'),
    A('shrug', 2, 'Shrug It Off', 'Your Bruisers regenerate 2.5% of their max HP every second.', 'combat'),
    A('spellblade', 2, 'Spell Blade', 'After casting, your Arcanists\' next basic attack deals bonus magic damage equal to 200% of their ability power.', 'combat'),
    A('stopwatch', 2, 'Broken Stopwatch', '5 seconds into combat, every enemy freezes for 2 seconds. Your Clockwork Digimon gain 30% attack speed.', 'combat'),
    A('smoke', 2, 'Smoke Bomb', 'The first time each of your Assassins drops below 60% HP, it vanishes for 1.5 seconds: it cannot be hurt and shakes off stuns.', 'combat'),
    A('injection', 2, 'Instant Injection', 'Your X-Antibody Digimon also trigger their X-Antibody bonus at the start of combat.', 'combat'),
    A('reserves', 2, 'Gold Reserves', 'Your Pirates deal 15% more damage, plus 1% more for every gold you had at the start of the round (up to 45% in total).', 'combat'),
    A('junkyard', 2, 'Junkyard Haul', 'Gain a random component after every third combat that you fight with Junkyard active.', 'items'),
    A('titanic', 2, 'Titanic Force', 'Your Digimon with 1400 or more max HP gain attack damage equal to 3% of their max HP.', 'combat'),
    A('censer', 2, 'Ardent Censer', 'When one of your Digimon gains a shield, it gains 30% attack speed for 5 seconds (at most once every 3 seconds).', 'combat'),
    // Prismatic
    A('jackpot', 3, 'Jackpot', 'Gain 30 gold.', 'econ'),
    A('grabbag', 3, 'Item Grab Bag', 'Gain 2 random completed items.', 'items'),
    A('recruit', 3, 'New Recruit', 'Gain +1 team size.', 'combat'),
    A('bless3', 3, 'Celestial Blessing III', 'Your team heals for 25% of all damage dealt.', 'combat'),
    A('implant3', 3, 'Cybernetic Implants III', 'Your Digimon holding an item gain 400 HP and 20% attack damage.', 'combat'),
    A('feather3', 3, 'Featherweights III', 'Your 1 and 2-cost Digimon gain 60% attack speed.', 'combat'),
    A('highend', 3, 'High End Shopping', 'Your shop rolls as if you were one level higher, and you gain a free reroll every round.', 'econ'),
    A('ticket', 3, 'Golden Ticket', 'Gain 5 gold. Every time your shop rolls, there is a 50% chance you gain a free reroll.', 'econ'),
    A('woodland', 3, 'Woodland Charm', 'At the start of combat, your Digimon with the most max HP makes a copy of itself with 1500 HP and no items.', 'combat'),
    A('windfall', 3, 'Windfall', 'Gain 18 gold for each Digivice upgrade you own, this one included.', 'econ'),
    A('forge', 3, 'Overclock Lab', 'Gain a random Overclocked item.', 'items'),
    A('burst', 3, 'Digivice Burst', 'Every trait with at least 2 Digimon on your board counts one extra.', 'combat'),
    A('united3', 3, 'Stand United III', 'Your team gains 5% attack damage and 5 ability power for every active trait.', 'combat'),
    A('band', 3, 'Band of Thieves', 'Gain a Thief Glove and a random component.', 'items'),
  ];

  // Hextech Gates: patterns of upgrade tiers for 2-1, 3-2 and 4-2, rolled once per game for the whole lobby.
  const GATES = [[1, 2, 3], [2, 2, 2], [2, 2, 3], [1, 2, 2], [2, 3, 2], [3, 2, 2], [1, 1, 3], [2, 1, 3], [1, 3, 2], [2, 3, 3], [3, 1, 2], [1, 2, 1]];

  DT.Sets.register({
    id: 'gizmos',
    name: 'Gizmos & Gadgets',
    mode: 'tft',
    nativePace: 'hyper',
    order: 5,
    desc: 'Inventors, pirates and Dark Masters in a gadget city. Three Digivice upgrades per game in Silver, Gold and Prismatic, with the tiers shown in advance by Hextech Gates.',
    colors: { main: '#4fd0ff', accent: '#ff8a3a' },
    source: 'TFT Set 6 Gizmos & Gadgets and Set 6.5 Neon Nights. docs/inspo/tft/set-06-gizmos-and-gadgets.md, docs/inspo/tft/set-06-5-neon-nights.md',
    traits: TRAITS,
    lines: LINES,
    pve,
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'giz_chip',
      emblems,
      dropEmblems,
      dropEmblemSource: 'the Armory, gold orbs and Digivice upgrades', // codex: where drop-only Crests come from
      // Crest ids 'giz_emblem_<trait>': other sets share some of this set's trait ids
      emblemIdPrefix: 'giz_',
      emblemName: (T) => `${T.name} Chip`,
      emblemColor2: '#e8f6ff',
      special: [
        { id: 'giz_golden_chip', name: 'Golden Chip', desc: '+1 team size while equipped on a Digimon on your board.', p: { teamSize: 1 }, stats: {},
          from: ['giz_chip', 'giz_chip'], color: '#ffd84a', color2: '#fff3b0', short: 'GC' },
      ],
      list: overclocked.concat(consumables),
      aiRoles,
    },
    augments,
    // Hyper Roll for 8 players, with Set 6 Hyper Roll's 2 starting gold. Upgrades replace the default augment rounds.
    rules: {
      startHp: 20,
      startGold: 2,
      rerollCost: 1,
      winGold: 2,
      augmentStages: [],
      offers: [
        { kind: 'upgrade', stage: 2, round: 1, rerolls: 1, gate: 0 },
        { kind: 'upgrade', stage: 3, round: 2, rerolls: 1, gate: 1 },
        { kind: 'upgrade', stage: 4, round: 2, rerolls: 1, gate: 2 },
      ],
      armoryStages: [2, 3, 4, 5, 6],
      armoryRound: 3,
      gates: GATES,
      // standard TFT pace: engine defaults (100 HP, XP, interest, 7-round stages), with the same three upgrade rounds
      normal: {
        augmentStages: [],
        augmentRounds: [],
        armoryStages: [],
        offers: [
          { kind: 'upgrade', stage: 2, round: 1, rerolls: 1, gate: 0 },
          { kind: 'upgrade', stage: 3, round: 2, rerolls: 1, gate: 1 },
          { kind: 'upgrade', stage: 4, round: 2, rerolls: 1, gate: 2 },
        ],
      },
    },
    tamers: ['Ikuto', 'Thomas', 'Yoshino', 'Kurata', 'Marcus', 'Keenan', 'Nanami'],
    codexRules: (rules, pace) => [
      { id: 'giz-upgrades', title: 'Digivice upgrades and Hextech Gates',
        html: '<p>At 2-1, 3-2 and 4-2 every Tamer picks 1 of 3 Digivice upgrades (augments), with one reroll. When the game starts, a Hextech Gate pattern decides the tier (Silver, Gold or Prismatic) of all three rounds for the whole lobby. Every upgrade card shows the whole pattern.</p><p>A Silver gate also gives every Tamer a random component and 4 gold. A Gold gate gives 2 gold. Clockwork and Chrome Digizoid get stronger for every upgrade you own.</p>' },
      { id: 'giz-items', title: 'Chips and Overclocked items',
        html: '<p>A Blank Chip plus a component makes a trait Chip (emblem). Two Blank Chips make a Golden Chip (+1 team size). Overclocked items are stronger versions of completed items. They come from Overclock Kits, the Overclock Lab upgrade and wild rounds from stage 6.</p>' },
      { id: 'giz-rounds', title: 'Rounds',
        html: pace === 'normal'
          ? '<p>Standard pace: 100 Tamer HP, XP (4 XP for 4 gold), interest and streak gold, 2-gold rerolls. 1-1 carousel, 1-2 to 1-4 wild Digimon; then x-1 to x-3 player combat, x-4 carousel, x-5 and x-6 player combat, x-7 wild Digimon. No Armory. Digivice upgrades at 2-1, 3-2 and 4-2.</p>'
          : '<p>Hyper Roll: 20 Tamer HP, no XP (your level rises each stage), fixed income, 2 gold for a player win. 1-1 carousel, 1-2 and 1-3 wild Digimon; then x-1 to x-3 player combat and x-4 wild Digimon. An Armory (pick 1 of 3 items) opens at x-3 of stages 2 to 6.</p>' },
    ],
  });
})(typeof window !== 'undefined' ? window : globalThis);
