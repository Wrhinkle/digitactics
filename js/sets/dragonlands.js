// Dragonlands: TFT Set 7 Dragonlands and Set 7.5 Uncharted Realms on Digimon lines.
// 60 shop lines (13/13/13/11/10), 10 of them Dragons that take 2 team slots, cost more and add +2 to their marked
// trait (they count as 3). 27 traits, 8 components + Dragon Egg, 36 recipes, 22 Crests, Golden Armor (Shimmerscale)
// gold items, Shining (Radiant) items, 4 consumables, 34 augments, a Treasure Dragon round, Hyper Roll rules.
// Behavior (trait, item, augment and mechanic hooks) is in dragonlands-mech.js; art in dragonlands-art.js.
// Design notes and balance results: docs/sets/dragonlands.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;

  // ---------- traits ----------
  // lv values are per breakpoint; extra arrays are read with c.val(key). Effects are in dragonlands-mech.js.
  const TRAITS = {
    // ---- origins ----
    STAR: { name: 'Stargazers', kind: 'origin', color: '#8f7bff', bp: [3, 5, 7], lv: [10, 25, 45], gold: [1, 2, 3], crestSymbol: 'star5',
      desc: 'Your whole team gains ability power. Every third combat with Stargazers active pays a Star Orb of gold, and your next shop shows a Stargazer.',
      fmt: (v, i) => `team +${v} AP, Star Orb ${TRAITS.STAR.gold[i]} gold` },
    XAB: { name: 'X-Antibody', kind: 'origin', color: '#e04a4a', bp: [1, 2, 3, 4, 5, 6], lv: [1, 1.2, 1.4, 1.6, 1.8, 2], crestSymbol: 'xros',
      desc: 'Each X-Antibody Digimon gives the whole team its own bonus; X-Antibody Digimon get double. Craniamon X line: +100 HP. Dynasmon X line: +8% attack speed. UlforceVeedramon X line: +8 armor and MR. Leopardmon X line: +8% attack damage. Kentaurosmon X line: +2 mana per attack. A Crest holder adds 3% omnivamp. The bonuses grow with more X-Antibody Digimon.',
      fmt: (v) => `${Math.round(v * 100)}% of each bonus` },
    STONE: { name: 'Ancient Stones', kind: 'origin', color: '#5fbf8a', bp: [3, 5, 7], lv: [1, 2, 3], heal: [0.008, 0.02, 0.035], as: [0.08, 0.2, 0.35], crestSymbol: 'shield',
      desc: 'Stone guardian statues rise next to your Digimon at the start of combat. Allies that start next to a statue heal a share of their max HP every 2 seconds and gain attack speed. Statues grow tougher each stage, and a destroyed statue deals a third of its HP as magic damage to adjacent enemies.',
      fmt: (v, i) => `${v} statue${v > 1 ? 's' : ''}, ${pct(TRAITS.STONE.heal[i])} HP/2s, +${pct(TRAITS.STONE.as[i])} AS` },
    MIRAGE: { name: 'Data Mirage', kind: 'origin', color: '#5ad6ff', bp: [2, 4, 6], lv: [1, 2, 3], crestSymbol: 'moon',
      dodge: [0.22, 0.32, 0.45], das: [0.25, 0.45, 0.65], war: [0.22, 0.4, 0.6], zap: [130, 240, 400], crit: [0.3, 0.45, 0.65],
      desc: 'Data Mirage Digimon gain one of four bonuses, picked at random each game: Dancer (dodge and attack speed), Warlord (attack damage and max HP), Overload (every 2 seconds, magic damage to adjacent enemies) or Executioner (crit chance, abilities can crit, and hits finish enemies below 10% HP).',
      fmt: (v, i) => { const T = TRAITS.MIRAGE; return `Dancer ${pct(T.dodge[i])} dodge +${pct(T.das[i])} AS / Warlord +${pct(T.war[i])} AD and HP / Overload ${T.zap[i]} / Executioner ${pct(T.crit[i])} crit`; } },
    RAGE: { name: 'Berserk', kind: 'origin', color: '#ff5a3a', bp: [3, 5, 7], lv: [0.2, 0.5, 0.9], vamp: [0.1, 0.2, 0.35], crestSymbol: 'flame',
      desc: 'Berserk Digimon gain 3 extra mana per basic attack. After casting they rage for 4 seconds: +25% attack speed plus the listed attack speed and omnivamp.',
      fmt: (v, i) => `rage: +${pct(v + 0.25)} AS, ${pct(TRAITS.RAGE.vamp[i])} omnivamp` },
    SLAYER: { name: 'Dragon Slayers', kind: 'origin', color: '#c9a66b', bp: [2, 4], lv: [0.2, 0.5], crestSymbol: 'dagger',
      desc: 'Dragon Slayers take 20% less damage from enemies with 1800 or more max HP. If you have no Dragon on your board, they also deal extra damage.',
      fmt: (v) => `-20% dmg from big foes, +${pct(v)} dmg without a Dragon` },
    GOLD: { name: 'Golden Armor', kind: 'origin', color: '#ffcf4a', bp: [3, 5, 7], lv: [1, 2, 4], armor: [20, 40, 60], crestSymbol: 'coin',
      desc: 'Golden Armor Digimon gain armor and magic resist. At the start of every round you get golden items for your Digimon. They grow with the gold you hold (counted at the start of the round, up to 15) and some pay out gold. Golden items vanish from the item bench and from benched Digimon each round and are handed out again. With 7, the Golden Crown can drop.',
      fmt: (v, i) => `+${TRAITS.GOLD.armor[i]} armor/MR, ${v} golden item${v > 1 ? 's' : ''}${i === 2 ? ', Golden Crown possible' : ''}` },
    THUNDER: { name: 'Thunder', kind: 'origin', color: '#ffe94a', bp: [2, 4, 6], lv: [0.08, 0.2, 0.35], as: [0.3, 0.6, 1.1], crestSymbol: 'chevrons',
      desc: 'After 8 seconds, lightning strikes every enemy: a 1 second stun and true damage equal to a share of its max HP. Then Thunder Digimon gain attack speed.',
      fmt: (v, i) => `${pct(v)} max HP true dmg, +${pct(TRAITS.THUNDER.as[i])} AS` },
    WHISPER: { name: 'Nightmare Whispers', kind: 'origin', color: '#9b4dff', bp: [2, 4, 6], lv: [0.012, 0.02, 0.035], crestSymbol: 'skull',
      desc: 'Damage from Nightmare Whispers shrinks enemies: 40% less armor and magic resist for 6 seconds. Each time they hit a shrunken enemy they gain attack damage and ability power for the rest of combat, up to 25 times.',
      fmt: (v) => `shrink -40% armor/MR, +${pct(v)} AD and +${Math.round(v * 100)} AP per hit` },
    LAGOON: { name: 'Lagoon', kind: 'origin', color: '#2f9bff', bp: [3, 5, 7], lv: [15, 35, 60], as: [0.12, 0.32, 0.6], gold: [1, 2, 3], crestSymbol: 'drop',
      desc: 'Lagoon Digimon gain ability power and attack speed. Every 20 abilities they cast (counted across the game) fill your Seastone: it pays gold, and every third payout also a component.',
      fmt: (v, i) => `+${v} AP, +${pct(TRAITS.LAGOON.as[i])} AS, Seastone ${TRAITS.LAGOON.gold[i]} gold` },
    // ---- classes ----
    ASSN: { name: 'Assassin', kind: 'class', color: '#ff4f7b', bp: [2, 4], lv: [0.25, 0.45], cd: [0.15, 0.4], amp: [0.08, 0.15], crestSymbol: 'dagger',
      desc: 'Assassins leap to the enemy backline when combat starts. Their abilities can critically strike, and they gain crit chance, crit damage and damage.',
      fmt: (v, i) => `+${pct(v)} crit, +${pct(TRAITS.ASSN.cd[i])} crit damage, +${pct(TRAITS.ASSN.amp[i])} damage` },
    BRUISER: { name: 'Bruiser', kind: 'class', color: '#d9773a', bp: [2, 4, 6], lv: [120, 250, 450], crestSymbol: 'heart',
      desc: 'Your whole team gains max HP. Bruisers gain double.', fmt: (v) => `team +${v} HP (x2 Bruisers)` },
    CANNON: { name: 'Cannoneer', kind: 'class', color: '#ff9a3a', bp: [2, 3, 4], lv: [1.2, 2.0, 2.8], flat: [60, 120, 200], crestSymbol: 'crosshair',
      desc: 'Every 4th basic attack of a Cannoneer fires a cannon shot that deals physical damage (a share of its attack damage plus a flat amount) to the target and adjacent enemies.',
      fmt: (v, i) => `cannon for ${Math.round(v * 100)}% AD + ${TRAITS.CANNON.flat[i]}` },
    RIDER: { name: 'Rider', kind: 'class', color: '#c08a5a', bp: [2, 3, 4], lv: [30, 50, 75], crestSymbol: 'shield',
      desc: 'Riders gain armor and magic resist, doubled for the first 4 seconds of combat.', fmt: (v) => `+${v} armor/MR (x2 for 4s)` },
    TAMER: { name: 'Dragon Tamer', kind: 'class', color: '#ff8fc8', bp: [2, 4, 6], lv: [350, 750, 1250], ap: [30, 55, 85], crestSymbol: 'up',
      desc: 'Your Dragon Tamer holding the most items (ties: higher cost) becomes the Hero: it gains max HP, ability power and the same share of attack damage. The bonus grows 10% for every star level (Champion or final form) among your Dragon Tamers.',
      fmt: (v, i) => `Hero +${v} HP, +${TRAITS.TAMER.ap[i]} AP` },
    EVOKER: { name: 'Evoker', kind: 'class', color: '#7fe0ff', bp: [2, 3, 4], lv: [1, 2, 3.5], crestSymbol: 'star6',
      desc: 'Evokers gain mana whenever any Digimon casts an ability.', fmt: (v) => `+${v} mana per cast` },
    GUARD: { name: 'Guardian', kind: 'class', color: '#c9c06b', bp: [2, 4, 6], lv: [0.2, 0.35, 0.55], crestSymbol: 'shield',
      desc: 'Once per combat at 50% HP, a Guardian shields itself and its closest ally for a share of its max HP. Shields stack.',
      fmt: (v) => `${pct(v)} max HP shield` },
    MAGE: { name: 'Mage', kind: 'class', color: '#d48bff', bp: [3, 5, 7], lv: [0.55, 0.7, 0.85], crestSymbol: 'star6',
      desc: 'Mages cast their ability twice. Their total ability power is changed to the listed share.',
      fmt: (v) => `cast twice at ${pct(v)} AP` },
    MYSTIC: { name: 'Mystic', kind: 'class', color: '#9cf6c0', bp: [2, 3, 4], lv: [12, 25, 45], crestSymbol: 'cross',
      desc: 'Your whole team gains magic resist.', fmt: (v) => `team +${v} MR` },
    MODE: { name: 'Mode Change', kind: 'class', color: '#ffb05a', bp: [2, 4], lv: [0.18, 0.35], crestSymbol: 'refresh',
      desc: 'The first time a Mode Change Digimon casts, it changes form and gains max HP (and heals that much).',
      fmt: (v) => `+${pct(v)} max HP on first cast` },
    SWIFT: { name: 'Swiftshot', kind: 'class', color: '#5ad1ff', bp: [2, 4], lv: [0.1, 0.2], crestSymbol: 'crosshair',
      desc: 'Swiftshots gain 1 range, and attack faster for each hex between them and their target.',
      fmt: (v) => `+1 range, +${pct(v)} AS per hex` },
    WARRIOR: { name: 'Warrior', kind: 'class', color: '#f4d35e', bp: [2, 4, 6], lv: [0.8, 1.75, 3.0], crestSymbol: 'sword',
      desc: 'After each basic attack a Warrior has a 50% chance to power up its next basic attack.', fmt: (v) => `+${pct(v)} on the powered attack` },
    DRAGON: { name: 'Dragon', kind: 'class', color: '#ff6a3d', bp: [1, 2, 3], lv: [550, 550, 550], armor: [0, 20, 20], amp: [0, 0, 0.1], crestSymbol: 'flame',
      desc: 'Dragons take 2 team slots, cost more and count as 3 Digimon for their marked trait. With 1 Dragon your Dragons gain max HP, with 2 also armor and magic resist, with 3 also 10% more damage. In Normal pace the bonus turns off with 3 or more Dragons.',
      bpDesc: ['Dragons +550 HP', 'Dragons +550 HP, +20 armor and MR', 'Dragons +550 HP, +20 armor and MR, 10% more damage (Hyper Roll only; Normal: no bonus)'],
      notes: 'No Crest exists for this trait. In Normal pace, from level 8 shops show fewer Dragons.' },
    // ---- unique ----
    STARCALL: { name: 'Wishing Star', kind: 'unique', color: '#fff0a0', bp: [1], lv: [1], crestSymbol: 'star5',
      desc: 'The first time Susanoomon\'s line casts in a PvP combat, your Tamer heals 1 HP (2 at its final form). If your Tamer is at full HP, the wish deletes a random enemy instead.',
      bpDesc: ['Heals your Tamer, or deletes an enemy at full HP'], notes: 'No Crest exists for this trait.' },
    COPYCAT: { name: 'Copycat', kind: 'unique', color: '#b0b8ff', bp: [1], lv: [1], crestSymbol: 'copy',
      desc: 'Millenniummon\'s line casts a random ability each time: a blast, an explosion, a stun, a barrage or a heal.',
      bpDesc: ['Random ability each cast'], notes: 'No Crest exists for this trait.' },
    CORE: { name: 'Digi-Core', kind: 'unique', color: '#ffd84a', bp: [1], lv: [1], crestSymbol: 'pearl',
      desc: 'Each cast of Kentaurosmon X\'s line drops a Digi-Core, and every ally alive after a PvP combat has a 2% chance to drop one. Each Digi-Core moves 1% of your shop odds to higher costs (up to 20).',
      bpDesc: ['Digi-Cores raise your shop odds'], notes: 'No Crest exists for this trait.' },
    MONO: { name: 'Monolith', kind: 'unique', color: '#a8a090', bp: [1], lv: [0.2], crestSymbol: 'shield',
      desc: 'Ebonwumon\'s line empowers 3 hexes on your side: the middle two of your front row\'s inner hexes and the center of your third row. Digimon that start combat there take 20% less damage.',
      bpDesc: ['Empowered hexes: 20% less damage'], notes: 'No Crest exists for this trait. Empowered hexes: front row columns 3 and 5, third row column 4 (counted from the left).' },
  };

  // ---------- lines ----------
  // L(id, tier, traits, attr, forms, abilities, type, opts). src: the corpus champion the line adapts.
  // Dragons: slots: 2, mark: the trait they add +2 to (they count as 3), price: shop cost.
  const L = (id, tier, traits, attr, forms, abil, type, opt = {}) => DT.line(id, tier, traits, attr, forms, abil, type, { unassigned: true, ...opt });
  const DR = (opt) => ({ slots: 2, levels: ['Rookie', 'Champion', 'Mega'], ...opt });
  const LINES = [
    // ---- 1-cost (13): the Set 7.5 1-costs ----
    L('drg_gazimon', 1, ['THUNDER', 'SWIFT'], 'Vaccine', ['Gazimon', 'Saberdramon', 'Varodurumon'], ['Electric Stun Blast', 'Black Saber', 'Purge Shine'], 'nuke', { role: 'Sniper', src: 'TFT7.5 Ezreal', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_gotsumon', 1, ['STONE', 'TAMER'], 'Data', ['Gotsumon', 'Icemon', 'Meteormon'], ['Rock Fist', 'Iceball Bomb', 'Galactic Flare'], 'aoe', { role: 'Mystic', src: 'TFT7.5 Karma' }),
    L('drg_patamon', 1, ['MIRAGE', 'GUARD'], 'Free', ['Patamon', 'Unimon', 'Shakkoumon'], ['Air Shot', 'Horn Blaster', 'Disaster Blaster'], 'shield', { role: 'Guardian', src: 'TFT7.5 Leona' }),
    L('drg_crabmon', 1, ['LAGOON', 'BRUISER'], 'Vaccine', ['Crabmon', 'Coelamon', 'Zudomon'], ['Scissors Execution', 'Fossil Bite', 'Cold Crusher'], 'shield', { role: 'Bruiser', src: 'TFT7.5 Malphite' }),
    L('drg_kamemon', 1, ['GOLD', 'GUARD'], 'Data', ['Kamemon', 'Guardromon (Gold)', 'Knightmon'], ['Shell Guard', 'Destruction Grenade', 'Grand Smash'], 'slam', { role: 'Guardian', src: 'TFT7.5 Nasus' }),
    L('drg_lunamon', 1, ['STAR', 'MODE'], 'Data', ['Lunamon', 'Sorcermon', 'Dianamon'], ['Luna Claw', 'Crystal Barrage', 'Crescent Harken'], 'dash', { role: 'Duelist', src: 'TFT7.5 Nidalee', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_elecmon', 1, ['XAB', 'RIDER'], 'Vaccine', ['Elecmon', 'Raptordramon', 'Craniamon X'], ['Sparkling Thunder', 'Crash Charge', 'Grand Gridr'], 'stun', { role: 'Bruiser', src: 'TFT7.5 Sejuani', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_impmon', 1, ['RAGE', 'CANNON'], 'Virus', ['Impmon', 'Sistermon Noir', 'Beelzemon X'], ['Infernal Funnel', 'Mickey Bullet', 'Gluttony Flare'], 'line', { role: 'Sniper', src: 'TFT7.5 Senna', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_gaomon', 1, ['RAGE', 'TAMER'], 'Vaccine', ['Gaomon', 'Golemon', 'BanchoLeomon'], ['Double Backhand', 'Golem Punch', 'Flashy Boss Punch'], 'drain', { role: 'Striker', src: 'TFT7.5 Sett', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_fanbeemon', 1, ['STAR', 'BRUISER'], 'Virus', ['FanBeemon', 'Hudiemon', 'TigerVespamon'], ['Gear Stinger', 'Infinity Dream', 'Mach Stinger V'], 'shield', { role: 'Bruiser', src: 'TFT7.5 Skarner', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_syakomon', 1, ['LAGOON', 'MAGE'], 'Vaccine', ['Syakomon', 'ShellNumemon', 'Neptunemon'], ['Black Pearl Blast', "Shell's Attack", 'Wave of Depth'], 'aoe', { role: 'Mystic', src: 'TFT7.5 Taliyah', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_dracmon', 1, ['STAR', 'MAGE'], 'Virus', ['Dracmon', 'IceDevimon', 'Merukimon'], ['Undead Fang', 'Frost Claw', 'Thousand Fists'], 'drain', { role: 'Mystic', src: 'TFT7.5 Vladimir', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_armadillomon', 1, ['STONE', 'WARRIOR'], 'Data', ['Armadillomon', 'Monochromon', 'Triceramon'], ['Scratch Beat', 'Volcanic Strike', 'Tri-Horn Attack'], 'stun', { role: 'Striker', src: 'TFT7.5 Wukong' }),

    // ---- 2-cost (13) ----
    L('drg_gabumon', 2, ['STONE', 'TAMER', 'SWIFT'], 'Data', ['Gabumon', 'Gururumon', 'AncientGarurumon'], ['Little Horn', 'Chaos Fire', 'Sharp Claymore'], 'barrage', { role: 'Sniper', src: 'TFT7 Ashe', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_gomamon', 2, ['SLAYER', 'GUARD'], 'Free', ['Gomamon', 'Ikkakumon', 'Vikemon'], ['Sharp Sider', 'Heat Top', 'Arctic Blizzard'], 'shield', { role: 'Guardian', src: 'TFT7.5 Braum', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_blackagumon', 2, ['STONE', 'MODE'], 'Vaccine', ['BlackAgumon', 'BlackGreymon', 'AncientGreymon'], ['Spitfire', 'Great Antler', 'Gaia Tornado'], 'slam', { role: 'Duelist', src: 'TFT7.5 Gnar', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_chuumon', 2, ['RAGE', 'GOLD', 'ASSN'], 'Virus', ['Chuumon', 'PlatinumSukamon', 'KingSukamon'], ['Cheese Bomb', 'Rare Metal Poop', 'King Stamp'], 'line', { role: 'Assassin', src: 'TFT7 Kayn' }),
    L('drg_betamon', 2, ['LAGOON', 'TAMER'], 'Data', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Beta Slugger', 'Ice Winder', 'Lightning Javelin'], 'barrage', { role: 'Sniper', src: "TFT7.5 Kai'Sa" }),
    L('drg_tsukaimon', 2, ['SLAYER', 'RIDER', 'MAGE'], 'Free', ['Tsukaimon', 'Piddomon', 'Valkyrimon'], ['Bad Message', 'Fire Phaser', 'Fenrir Sword'], 'aoe', { role: 'Bruiser', src: 'TFT7.5 Lillia', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_salamon', 2, ['STAR', 'MAGE'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'line', { role: 'Mystic', src: 'TFT7.5 Lux' }),
    L('drg_biyomon', 2, ['THUNDER', 'ASSN'], 'Vaccine', ['Biyomon', 'Airdramon', 'Ravemon'], ['Violin Attack', 'Spinning Needle', 'Spiral Raven Claw'], 'line', { role: 'Assassin', src: 'TFT7.5 Qiyana', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_monodramon', 2, ['RAGE', 'BRUISER', 'WARRIOR'], 'Virus', ['Monodramon', 'Cyclonemon', 'Gallantmon X'], ['Crack Bite', 'Hyper Heat', 'Shield of the Just'], 'rally', { role: 'Bruiser', src: 'TFT7 Shen', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_mushroomon', 2, ['XAB', 'SWIFT'], 'Data', ['Mushroomon', 'RedVegiemon', 'Dynasmon X'], ['Fungus Crusher', 'Hazard Breath', "Dragon's Roar"], 'aoe', { role: 'Sniper', src: 'TFT7.5 Twitch', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_kotemon', 2, ['MIRAGE', 'WARRIOR'], 'Data', ['Kotemon', 'Sistermon Ciel (Awakened)', 'Jesmon X'], ['Meteor Slash', 'Byakusai', 'Iron-Fist Judgment'], 'nuke', { role: 'Striker', src: 'TFT7.5 Yone', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_otamamon', 2, ['LAGOON', 'GUARD'], 'Virus', ['Otamamon', 'Gekomon', 'ShogunGekomon'], ['Wrapping Bubble', 'Tongue Attack', 'Musical Fist'], 'slam', { role: 'Guardian', src: 'TFT7.5 Zac' }),
    L('drg_aruraumon', 2, ['WHISPER', 'EVOKER'], 'Virus', ['Aruraumon', 'Vegiemon', 'Cherrymon'], ['Nemesis Ivy', 'Sharp Leaf', 'Cherry Bomb'], 'stun', { role: 'Mystic', src: 'TFT7.5 Zyra' }),

    // ---- 3-cost (13), one Dragon ----
    L('drg_renamon', 3, ['SLAYER', 'ASSN'], 'Data', ['Renamon', 'Kyubimon (Silver)', 'Kuzuhamon'], ['Kohenkyo', 'Dragon Wheel', 'Illuminator'], 'dash', { role: 'Assassin', src: 'TFT7.5 Diana', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_veemon', 3, ['THUNDER', 'TAMER'], 'Vaccine', ['Veemon', 'Flamedramon', 'WarGreymon X'], ['Hopping Kick', 'Flaming Fist', 'Terra Force Zero'], 'stun', { role: 'Striker', src: 'TFT7.5 Lee Sin', levels: ['Rookie', 'Armor', 'Mega'] }),
    L('drg_lalamon', 3, ['EVOKER', 'MYSTIC'], 'Data', ['Lalamon', 'Sunflowmon', 'Lotosmon'], ['Sing a Song', 'Sunshine Beam', 'Serpent Cure'], 'rally', { role: 'Support', src: 'TFT7 Lulu', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_snowgoblimon', 3, ['MIRAGE', 'RIDER'], 'Data', ['SnowGoblimon', 'MudFrigimon', 'SaberLeomon'], ['Snow Goblin Volt', 'Great Weight', 'Infinity Arrow'], 'execute', { role: 'Bruiser', src: 'TFT7.5 Nunu & Willump', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_blackgabumon', 3, ['SLAYER', 'BRUISER', 'WARRIOR'], 'Virus', ['BlackGabumon', 'BlackGarurmon', 'BlackMetalGarurumon'], ['Blue Blaster', 'Freeze Fang', 'Garuru Tomahawk'], 'drain', { role: 'Bruiser', src: 'TFT7.5 Olaf', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_hawkmon', 3, ['RAGE', 'GUARD', 'MYSTIC'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Glide Horn', 'Dual Sonic'], 'rally', { role: 'Guardian', src: 'TFT7.5 Rakan' }),
    L('drg_penguinmon', 3, ['LAGOON', 'EVOKER', 'MYSTIC'], 'Vaccine', ['Penguinmon', 'Whamon', 'MarineAngemon'], ['Ice Prism', 'Tidal Wave', 'Ocean Love'], 'heal', { role: 'Support', src: 'TFT7.5 Seraphine', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_psychemon', 3, ['WHISPER', 'MAGE', 'BRUISER'], 'Data', ['Psychemon', 'Meramon', 'SkullMeramon'], ['Colored Spark', 'Burning Fist', 'Heat Chain'], 'shield', { role: 'Bruiser', src: 'TFT7.5 Sylas' }),
    L('drg_kudamon', 3, ['STAR', 'SWIFT'], 'Data', ['Kudamon', 'Youkomon', 'Sakuyamon'], ['Bullet Whirlwind', 'Blazing Spheres', 'Izuna'], 'chain', { role: 'Sniper', src: 'TFT7.5 Varus', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_gumdramon', 3, ['GOLD', 'TAMER'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Magnamon X'], ['Jacked Hammer', 'Spin Caliber', 'Plasma Shoot'], 'chain', { role: 'Striker', src: 'TFT7.5 Volibear', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_kokuwamon', 3, ['LAGOON', 'CANNON'], 'Vaccine', ['Kokuwamon', 'Tankmon', 'HiAndromon'], ['Electro Strike', 'Sub Machine Gun', 'Atomic Ray'], 'barrage', { role: 'Sniper', src: 'TFT7.5 Zeri', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_toyagumon', 3, ['CANNON', 'SLAYER'], 'Vaccine', ['ToyAgumon', 'Guardromon', 'Andromon'], ['Plastic Blaze', 'Power Laser', 'Gatling Missile'], 'barrage', { role: 'Sniper', src: 'TFT7.5 Tristana' }),
    L('drg_dorumon', 3, ['XAB', 'DRAGON'], 'Vaccine', ['Dorumon', 'RedVeedramon', 'UlforceVeedramon X'], ['Dash Metal', 'V-Nova Blast', 'The Ray of Victory'], 'dash', DR({ role: 'Duelist', mark: 'XAB', price: 5, hpMul: 1.45, adMul: 1.3, src: 'TFT7.5 Zippy' })),

    // ---- 4-cost (11), five Dragons ----
    L('drg_dracomon', 4, ['MIRAGE', 'DRAGON'], 'Virus', ['Dracomon', 'Groundramon', 'Breakdramon'], ['Baby Breath', 'Megaton Hammer Crush', 'Destroyed Rush'], 'aoe', DR({ role: 'Mystic', mark: 'MIRAGE', price: 6, hpMul: 1.45, adMul: 1.2, radius: 2, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT7 Daeja' })),
    L('drg_tentomon', 4, ['THUNDER', 'CANNON'], 'Vaccine', ['Tentomon', 'CatchMamemon', 'Justimon'], ['Super Shocker', 'Magic Arm', 'Trinity Arm'], 'line', { role: 'Sniper', src: 'TFT7.5 Graves', levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('drg_guilmon', 4, ['RAGE', 'RIDER'], 'Virus', ['Guilmon', 'Gallantmon', 'Gallantmon Crimson Mode'], ['Rock Breaker', 'Lightning Joust', 'Invincible Sword'], 'line', { role: 'Bruiser', src: 'TFT7.5 Hecarim', levels: ['Rookie', 'Mega', 'Mega'] }),
    L('drg_coronamon', 4, ['GOLD', 'DRAGON', 'GUARD'], 'Vaccine', ['Coronamon', 'Coredramon (Green)', 'Goldramon'], ['Corona Flame', 'Green Flare Breath', 'God Flame'], 'rally', DR({ role: 'Guardian', mark: 'GOLD', price: 6, hpMul: 1.45, adMul: 1.2, src: 'TFT7 Idas' })),
    L('drg_hackmon', 4, ['XAB', 'MODE'], 'Data', ['Hackmon', 'SaviorHuckmon', 'Leopardmon X'], ['Fifth Rush', 'Meteor Flame', 'Black Aura Blast'], 'execute', { role: 'Duelist', src: 'TFT7.5 Jayce', levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('drg_gizamon', 4, ['LAGOON', 'ASSN'], 'Virus', ['Gizamon', 'Gesomon', 'Dragomon'], ['Spiral Edge', 'Devil Bashing', 'Thousand Whips'], 'dash', { role: 'Assassin', src: 'TFT7.5 Nilah' }),
    L('drg_goblimon', 4, ['WHISPER', 'WARRIOR'], 'Virus', ['Goblimon', 'Ogremon', 'Titamon'], ['Goblin Strike', 'Bone Cudgel', 'Phantom Blade'], 'line', { role: 'Striker', src: 'TFT7.5 Pantheon', levels: ['Rookie', 'Champion', 'Mega'] }),
    L('drg_ryudamon', 4, ['STONE', 'MYSTIC', 'DRAGON'], 'Vaccine', ['Ryudamon', 'Duramon', 'Ouryumon'], ['Katana Attack', 'Glass Slash', 'Eiseiryuoujin'], 'execute', DR({ role: 'Duelist', mark: 'STONE', price: 6, hpMul: 1.45, adMul: 1.25, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT7 Shi Oh Yu' })),
    L('drg_coredramon', 4, ['LAGOON', 'MAGE', 'DRAGON'], 'Data', ['Coredramon (Blue)', 'Megadramon', 'MetalSeadramon'], ['Blue Flare Breath', 'Big Mega Fire', 'River of Power'], 'aoe', DR({ role: 'Mystic', mark: 'LAGOON', price: 6, hpMul: 1.45, adMul: 1.2, radius: 2, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT7.5 Sohm' })),
    L('drg_dorugamon', 4, ['WHISPER', 'BRUISER', 'DRAGON'], 'Data', ['Dorugamon', 'DoruGreymon', 'Dorugoramon'], ['Power Metal', 'Metal Meteor', 'Brave Metal'], 'dash', DR({ role: 'Bruiser', mark: 'WHISPER', price: 6, hpMul: 1.4, adMul: 1.3, levels: ['Champion', 'Ultimate', 'Mega'], src: "TFT7 Sy'fen" })),
    L('drg_hyokomon', 4, ['RAGE', 'SWIFT'], 'Data', ['Hyokomon', 'HippoGryphonmon', 'Gryphonmon'], ['Downward Cleave', 'Heat Wave', 'Supersonic Voice'], 'barrage', { role: 'Sniper', src: 'TFT7.5 Xayah', levels: ['Rookie', 'Ultimate', 'Mega'] }),

    // ---- 5-cost (10), four Dragons ----
    L('drg_jazardmon', 5, ['THUNDER', 'DRAGON'], 'Data', ['Jazardmon', 'Jazarichmon', 'Metallicdramon'], ['Laser Eye', 'Messer Spiral', 'Laser Cannon'], 'barrage', DR({ role: 'Mystic', mark: 'THUNDER', price: 7, hpMul: 1.4, adMul: 1.2, shots: 6, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT7 Ao Shin' })),
    L('drg_growlmon', 5, ['STAR', 'EVOKER', 'DRAGON'], 'Vaccine', ['Growlmon (Yellow)', 'Wingdramon', 'Slayerdramon'], ['Plasma Blade', 'Blaze Sonic Breath', 'Tenryu Slash'], 'aoe', DR({ role: 'Mystic', mark: 'STAR', price: 7, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT7 Aurelion Sol' })),
    L('drg_baohuckmon', 5, ['XAB', 'MYSTIC', 'CORE'], 'Vaccine', ['BaoHuckmon', 'Kentaurosmon', 'Kentaurosmon X'], ['Fif Cross', 'Bifrost', 'Brander'], 'stun', { role: 'Support', src: 'TFT7 Bard', levels: ['Champion', 'Mega', 'Mega'] }),
    L('drg_bakemon', 5, ['WHISPER', 'ASSN'], 'Virus', ['Bakemon', 'Phantomon', 'GranDracmon'], ["Hell's Hand", 'Soul Chopper', 'Death Scream'], 'execute', { role: 'Assassin', src: 'TFT7 Pyke', levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('drg_greymon', 5, ['RAGE', 'MODE', 'DRAGON'], 'Virus', ['Greymon', 'BlackMetalGreymon', 'Volcanicdramon'], ['Great Horns Attack', 'Metal Slash', 'Volcanic Flare'], 'aoe', DR({ role: 'Duelist', mark: 'RAGE', price: 7, hpMul: 1.4, adMul: 1.25, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT7 Shyvana' })),
    L('drg_leomon', 5, ['STONE', 'STARCALL'], 'Vaccine', ['Leomon', 'GrapLeomon', 'Susanoomon'], ['Beast Sword', 'The King of Fists', 'Yakusa no Ikazuchi'], 'heal', { role: 'Support', hpMul: 1.25, src: 'TFT7 Soraka', levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('drg_zubamon', 5, ['MIRAGE', 'TAMER', 'WARRIOR'], 'Virus', ['Zubamon', 'Grademon', 'Samudramon'], ['Twenty Dive', 'Cross Blade', "Will-O'-Wisp Slash"], 'dash', { role: 'Striker', src: 'TFT7 Yasuo', levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('drg_clockmon', 5, ['GOLD', 'COPYCAT', 'MAGE'], 'Virus', ['Clockmon', 'Datamon', 'Millenniummon'], ['Chrono Breaker', 'Digital Bomb', 'Time Unlimited'], 'copy', { role: 'Mystic', hpMul: 1.15, src: 'TFT7 Zoe', levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('drg_ankylomon', 5, ['MONO', 'DRAGON'], 'Vaccine', ['Ankylomon', 'Lavogaritamon', 'Ebonwumon'], ['Megaton Press', 'Melding Blaze', 'Mugen'], 'slam', DR({ role: 'Guardian', mark: 'MONO', price: 7, hpMul: 1.6, adMul: 1.25, radius: 2, levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT7.5 Terra' })),
    L('drg_volcamon', 5, ['THUNDER', 'BRUISER'], 'Data', ['Volcamon', 'PileVolcamon', 'Vulcanusmon'], ['Big Bang Voice', 'Volcanic Driver', 'Vulcanus Hammer'], 'line', { role: 'Bruiser', src: 'TFT7 Ornn', levels: ['Ultimate', 'Mega', 'Mega'] }),

    // ---- wild Digimon (PvE only) ----
    L('drg_w_commandramon', 1, [], 'Data', ['Commandramon', 'Commandramon', 'Commandramon'], ['Rifle Burst', 'Rifle Burst', 'Rifle Burst'], 'nuke', { role: 'Striker', shop: false, adMul: 1.15 }),
    L('drg_w_sealsdramon', 2, [], 'Data', ['Sealsdramon', 'Sealsdramon', 'Sealsdramon'], ['Death Behind', 'Death Behind', 'Death Behind'], 'dash', { role: 'Assassin', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('drg_w_devidramon', 2, [], 'Virus', ['Devidramon', 'Devidramon', 'Devidramon'], ['Crimson Claw', 'Crimson Claw', 'Crimson Claw'], 'nuke', { role: 'Striker', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('drg_w_tyrannomon', 3, [], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Blaze Blast', 'Blaze Blast', 'Blaze Blast'], 'aoe', { role: 'Bruiser', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('drg_w_gigadramon', 4, [], 'Virus', ['Gigadramon', 'Gigadramon', 'Gigadramon'], ['Genocide Gear', 'Genocide Gear', 'Genocide Gear'], 'barrage', { role: 'Sniper', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('drg_w_metaltyrannomon', 4, [], 'Virus', ['MetalTyrannomon', 'MetalTyrannomon', 'MetalTyrannomon'], ['Giga Blaster', 'Giga Blaster', 'Giga Blaster'], 'slam', { role: 'Bruiser', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('drg_w_goldveedramon', 5, [], 'Vaccine', ['GoldVeedramon', 'GoldVeedramon', 'GoldVeedramon'], ['Gold V-Breath', 'Gold V-Breath', 'Gold V-Breath'], 'aoe', { role: 'Bruiser', shop: false, boss: true, hpMul: 1.4, levels: ['Armor', 'Armor', 'Armor'] }),
    L('drg_w_magnadramon', 5, [], 'Vaccine', ['Magnadramon', 'Magnadramon', 'Magnadramon'], ['Fire Tornado', 'Fire Tornado', 'Fire Tornado'], 'aoe', { role: 'Mystic', shop: false, boss: true, hpMul: 1.6, levels: ['Mega', 'Mega', 'Mega'] }),
    L('drg_w_megidramon', 5, [], 'Virus', ['Megidramon', 'Megidramon', 'Megidramon'], ['Megiddo Flame', 'Megiddo Flame', 'Megiddo Flame'], 'aoe', { role: 'Bruiser', shop: false, boss: true, hpMul: 1.5, adMul: 1.2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('drg_w_chaosdromon', 5, [], 'Virus', ['Chaosdromon', 'Chaosdromon', 'Chaosdromon'], ['Hyper Mugen Cannon', 'Hyper Mugen Cannon', 'Hyper Mugen Cannon'], 'slam', { role: 'Guardian', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('drg_w_blackwargreymon', 5, [], 'Virus', ['BlackWarGreymon', 'BlackWarGreymon', 'BlackWarGreymon'], ['Black Tornado', 'Black Tornado', 'Black Tornado'], 'dash', { role: 'Duelist', shop: false, boss: true, hpMul: 1.4, adMul: 1.3, levels: ['Mega', 'Mega', 'Mega'] }),
    L('drg_w_imperialdramon', 5, [], 'Free', ['Imperialdramon Fighter Mode', 'Imperialdramon Fighter Mode', 'Imperialdramon Fighter Mode'], ['Giga Death', 'Giga Death', 'Giga Death'], 'aoe', { role: 'Mystic', shop: false, boss: true, hpMul: 1.6, adMul: 1.3, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('drg_w_azulongmon', 5, [], 'Vaccine', ['Azulongmon', 'Azulongmon', 'Azulongmon'], ['Lightning Whip', 'Lightning Whip', 'Lightning Whip'], 'chain', { role: 'Mystic', shop: false, boss: true, hpMul: 1.8, adMul: 1.3, levels: ['Mega', 'Mega', 'Mega'] }),
    // summons (not in the shop, never counted for traits)
    L('drg_stone', 1, [], 'Free', ['Stone Guardian', 'Stone Guardian', 'Stone Guardian'], ['Stand', 'Stand', 'Stand'], 'shield', { role: 'Guardian', shop: false, mana: 999, levels: ['Statue', 'Statue', 'Statue'] }),
    L('drg_voidling', 2, [], 'Virus', ['Void Larva', 'Void Larva', 'Void Larva'], ['Void Bite', 'Void Bite', 'Void Bite'], 'nuke', { role: 'Bruiser', shop: false, levels: ['Summon', 'Summon', 'Summon'] }),
  ];
  for (const l of LINES) if (!l.src) l.src = 'wild';
  // Balance multipliers from the duel and full-game reports (docs/sets/dragonlands.md). Applied over the line options.
  const TUNE = {
    drg_elecmon: { hpMul: 0.85 }, drg_ryudamon: { adMul: 1.25, hpMul: 1.65 }, drg_hackmon: { adMul: 0.9 }, drg_penguinmon: { hpMul: 0.9 },
    drg_mushroomon: { adMul: 0.9 }, drg_gomamon: { hpMul: 0.92 }, drg_gizamon: { hpMul: 0.82, adMul: 0.75 },
    drg_dracmon: { hpMul: 1.5 }, drg_salamon: { hpMul: 1.4, adMul: 1.2, mana: 50 }, drg_dracomon: { hpMul: 1.65 }, drg_gotsumon: { hpMul: 1.35 },
    drg_syakomon: { hpMul: 1.35 }, drg_kotemon: { hpMul: 1.2, adMul: 1.2 }, drg_lunamon: { hpMul: 1.25 }, drg_renamon: { adMul: 0.95 },
    drg_toyagumon: { adMul: 1.05 }, drg_kokuwamon: { adMul: 1.05 }, drg_veemon: { hpMul: 1.1 }, drg_coronamon: { hpMul: 1.8, adMul: 1.3 },
    drg_coredramon: { hpMul: 1.7, adMul: 1.3 }, drg_dorugamon: { hpMul: 1.6, adMul: 1.4 }, drg_blackagumon: { hpMul: 1.35 }, drg_otamamon: { hpMul: 1.35 },
    drg_biyomon: { adMul: 1.2 }, drg_armadillomon: { hpMul: 1.15, adMul: 1.1 }, drg_impmon: { adMul: 1.1 }, drg_aruraumon: { hpMul: 1.3, mana: 50 },
    drg_kamemon: { hpMul: 1.15 }, drg_gaomon: { hpMul: 1.15 }, drg_gazimon: { adMul: 1.15 }, drg_betamon: { hpMul: 1.15 },
  };
  for (const l of LINES) if (TUNE[l.id]) Object.assign(l, TUNE[l.id]);

  // ---------- wild rounds ----------
  // [lineId, stage, col, row] in the enemy's own rows (4 = front). 1-2, 1-3, then x-4. 4-4 is the Treasure Dragon:
  // classic GoldVeedramon, Order Magnadramon or Chaos Megidramon (one version per game, rolled at the start).
  const TREASURE_BOSS = { classic: 'drg_w_goldveedramon', order: 'drg_w_magnadramon', chaos: 'drg_w_megidramon' };
  const pveWave = function (stage, round, game) {
    if (stage === 1) {
      if (round === 2) return [['drg_w_commandramon', 0, 2, 4], ['drg_w_commandramon', 0, 4, 4]];
      return [['drg_w_commandramon', 0, 2, 4], ['drg_w_commandramon', 0, 4, 4], ['drg_w_sealsdramon', 0, 3, 6]];
    }
    if (stage === 2) return [['drg_w_devidramon', 1, 2, 4], ['drg_w_devidramon', 1, 4, 4], ['drg_w_tyrannomon', 0, 3, 5], ['drg_w_commandramon', 0, 3, 7]];
    if (stage === 3) return [['drg_w_tyrannomon', 1, 2, 4], ['drg_w_tyrannomon', 1, 4, 4], ['drg_w_sealsdramon', 1, 0, 6], ['drg_w_gigadramon', 0, 3, 6], ['drg_w_devidramon', 1, 6, 5]];
    if (stage === 4) {
      const v = (game && game.mech && game.mech.treasure) || 'classic';
      return [[TREASURE_BOSS[v], 2, 3, 4], ['drg_w_devidramon', 1, 1, 5], ['drg_w_devidramon', 1, 5, 5], ['drg_w_gigadramon', 1, 3, 7]];
    }
    if (stage === 5) return [['drg_w_metaltyrannomon', 2, 2, 4], ['drg_w_metaltyrannomon', 2, 4, 4], ['drg_w_gigadramon', 1, 1, 6], ['drg_w_gigadramon', 1, 5, 6], ['drg_w_chaosdromon', 1, 3, 5], ['drg_w_sealsdramon', 2, 0, 7]];
    if (stage === 6) return [['drg_w_chaosdromon', 1, 3, 4], ['drg_w_metaltyrannomon', 2, 1, 4], ['drg_w_metaltyrannomon', 2, 5, 4], ['drg_w_blackwargreymon', 1, 3, 6], ['drg_w_gigadramon', 2, 1, 7], ['drg_w_gigadramon', 2, 5, 7]];
    if (stage === 7) return [['drg_w_chaosdromon', 2, 3, 4], ['drg_w_blackwargreymon', 1, 1, 5], ['drg_w_metaltyrannomon', 2, 5, 4], ['drg_w_imperialdramon', 2, 3, 6], ['drg_w_gigadramon', 2, 0, 7], ['drg_w_gigadramon', 2, 6, 7]];
    return [['drg_w_chaosdromon', 2, 3, 4], ['drg_w_blackwargreymon', 2, 2, 4], ['drg_w_megidramon', 1, 4, 5], ['drg_w_imperialdramon', 1, 2, 6], ['drg_w_azulongmon', 1, 4, 7], ['drg_w_gigadramon', 2, 6, 7]];
  };

  // ---------- items ----------
  const components = {
    sword: { name: 'Power Chip', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    bow: { name: 'Speed Chip', short: 'SC', color: '#ffd23f', stats: { as: 0.12 } },
    rod: { name: 'Data Rod', short: 'DR', color: '#b26bff', stats: { ap: 15 } },
    tear: { name: 'Mana Card', short: 'MC', color: '#4fb3ff', stats: { mana: 15 } },
    vest: { name: 'Chrome Plate', short: 'CP', color: '#c0c6d0', stats: { armor: 20 } },
    cloak: { name: 'Data Cloak', short: 'DC', color: '#5fe0c0', stats: { mr: 20 } },
    belt: { name: 'HP Disk', short: 'HD', color: '#ff5d73', stats: { hp: 150 } },
    glove: { name: 'Lucky Glove', short: 'LG', color: '#9be15d', stats: { crit: 0.15 } },
    egg: { name: 'Dragon Egg', short: 'DE', color: '#ffb46a', stats: {}, special: true,
      desc: 'Combine with a component to make a trait Crest, or with another Dragon Egg for a Tamer\'s Crown (+1 team size).' },
  };
  // [a, b, name, desc, passives, extra stats, { tags, aiRank, id }]. Item hooks (auras, banish, taunt...) are in the mech file.
  const R = (a, b, name, desc, p, extra, o) => [a, b, name, desc, p || null, extra || {}, o || {}];
  const recipes = [
    R('sword', 'sword', 'Dragon Fang', 'A lot of attack damage. Deals 8% more damage.', { amp: 0.08 }, { ad: 0.3 }, { id: 'drg_dragon_fang', tags: ['ad'], aiRank: 3 }),
    R('sword', 'bow', 'Titan Slayer', 'Deals 10% more damage, and 25% more to enemies with 1600 or more max HP.', { amp: 0.1, slayer: 0.25 }, {}, { id: 'drg_titan_slayer', tags: ['ad'], aiRank: 4 }),
    R('sword', 'rod', 'Data Gunblade', 'Heals for 25% of all damage dealt.', { vamp: 0.25 }, {}, { id: 'drg_gunblade', tags: ['ad', 'ap'], aiRank: 2 }),
    R('sword', 'tear', 'Mana Spear', 'Basic attacks restore 6 extra mana.', { shojin: 6 }, {}, { id: 'drg_mana_spear', tags: ['ap', 'ad'], aiRank: 3 }),
    R('sword', 'vest', 'Night Veil', 'Once per combat at 60% HP, become untouchable for 1 second, shake off stuns and burns, and gain 40% attack speed.', null, {}, { id: 'drg_night_veil', tags: ['ad'], aiRank: 1 }),
    R('sword', 'cloak', 'Blood Fang', 'Heals for 25% of damage dealt. Once below 40% HP, gain a 25% max HP shield.', { vamp: 0.25, lifeline: { at: 0.4, shield: 0.25 } }, {}, { id: 'drg_blood_fang', tags: ['ad'], aiRank: 3 }),
    R('sword', 'belt', 'Herald Banner', 'At the start of combat, the holder and allies next to it in the same row gain 30% attack speed.', null, {}, { id: 'drg_herald_banner', tags: ['tank'], aiRank: 0 }),
    R('sword', 'glove', 'Crit Edge', 'Abilities can critically strike. +45% crit chance and +10% crit damage.', { abilCrit: true, critDmg: 0.1 }, { crit: 0.45 }, { id: 'drg_crit_edge', tags: ['ad'], aiRank: 5 }),
    R('bow', 'bow', 'Rapid Cannon', 'Grants 1 range and a lot of attack speed.', null, { as: 0.3 }, { id: 'drg_rapid_cannon', tags: ['ad'], aiRank: 2 }),
    R('bow', 'rod', 'Rage Blade', 'Each basic attack grants 6% attack speed, stacking.', { rage: 0.06 }, {}, { id: 'drg_rage_blade', tags: ['ad', 'ap'], aiRank: 4 }),
    R('bow', 'tear', 'Volt Disk', 'Every 3rd basic attack sends lightning to 3 enemies for 80 magic damage.', { volt: 80 }, { as: 0.1 }, { id: 'drg_volt_disk', tags: ['ad'], aiRank: 1 }),
    R('bow', 'vest', 'Titan Will', 'Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks.', { titan: true }, {}, { id: 'drg_titan_will', tags: ['tank', 'ad'], aiRank: 2 }),
    R('bow', 'cloak', 'Twin Shot', 'Basic attacks also hit a second nearby enemy for 60% damage.', { split: 0.6 }, { ad: 0.1 }, { id: 'drg_twin_shot', tags: ['ad'], aiRank: 2 }),
    R('bow', 'belt', 'Lure Portal', 'At the start of combat, enemies within 4 hexes target the holder. When it is knocked out, a Void Larva rises in its place.', null, {}, { id: 'drg_lure_portal', tags: ['tank'], aiRank: 1 }),
    R('bow', 'glove', 'Armor Breaker', "Basic attacks cut the target's armor by 50% for 3s.", { shred: 0.5 }, {}, { id: 'drg_armor_breaker', tags: ['ad'], aiRank: 2 }),
    R('rod', 'rod', 'Ultima Rod', 'A lot of ability power.', null, { ap: 45 }, { id: 'drg_ultima_rod', tags: ['ap'], aiRank: 4 }),
    R('rod', 'tear', 'Seraph Rod', 'Gain 20 AP every 5 seconds.', { archangel: 20 }, { ap: 10 }, { id: 'drg_seraph_rod', tags: ['ap'], aiRank: 5 }),
    R('rod', 'vest', 'Solar Locket', 'At the start of combat, shields the holder and allies within 2 hexes in the same row (250/350/450 by form).', null, {}, { id: 'drg_solar_locket', tags: ['tank'], aiRank: 1 }),
    R('rod', 'cloak', 'Mana Zap', 'When an enemy within 2 hexes casts, deal 160 magic damage to it.', { zap: 160 }, {}, { id: 'drg_mana_zap', tags: ['tank'], aiRank: 1 }),
    R('rod', 'belt', 'Flame Tome', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, { ap: 20 }, { id: 'drg_flame_tome', tags: ['ap'], aiRank: 3 }),
    R('rod', 'glove', 'Jewel Gauntlet', 'Abilities can critically strike. +15% crit damage.', { abilCrit: true, critDmg: 0.15 }, { ap: 20 }, { id: 'drg_jewel_gauntlet', tags: ['ap'], aiRank: 5 }),
    R('tear', 'tear', 'Blue Core', 'Max mana -10. After casting, keep 20 mana.', { bluebuff: true }, { mana: 10 }, { id: 'drg_blue_core', tags: ['ap'], aiRank: 4 }),
    R('tear', 'vest', 'Frost Heart', 'Enemies within 2 hexes attack 25% slower.', null, {}, { id: 'drg_frost_heart', tags: ['tank'], aiRank: 2 }),
    R('tear', 'cloak', 'Chalice of Power', 'At the start of combat, the holder and allies next to it in the same row gain 30 AP.', null, {}, { id: 'drg_chalice', tags: ['ap'], aiRank: 0 }),
    R('tear', 'belt', 'Holy Ring', 'Every 5s, heals allies within 2 hexes for 80 + 10% of missing HP.', { redemption: true }, {}, { id: 'drg_holy_ring', tags: ['tank'], aiRank: 1 }),
    R('tear', 'glove', 'Justice Hand', 'Deals 12% more damage and heals for 12% of damage dealt. Each combat one of the two is doubled at random.', { amp: 0.12, vamp: 0.12 }, {}, { id: 'drg_justice_hand', tags: ['ap', 'ad'], aiRank: 3 }),
    R('vest', 'vest', 'Spike Plate', 'Lots of armor. When attacked, deals 60 magic damage to adjacent enemies (1.5s cooldown).', { thorns: 60 }, { armor: 30 }, { id: 'drg_spike_plate', tags: ['tank'], aiRank: 3 }),
    R('vest', 'cloak', 'Stone Hide', 'Gain 12 armor and MR for each enemy within 2 hexes.', { gargoyle: 12 }, {}, { id: 'drg_stone_hide', tags: ['tank'], aiRank: 4 }),
    R('vest', 'belt', 'Flame Core', 'Every 2s, burns the nearest enemy within 2 hexes.', { sunfire: true }, { hp: 100 }, { id: 'drg_flame_core', tags: ['tank'], aiRank: 4 }),
    R('vest', 'glove', 'Shroud of Stillness', 'At the start of combat, enemies in the 3 columns facing the holder need 35% more mana until they cast.', null, {}, { id: 'drg_shroud', tags: ['tank'], aiRank: 0 }),
    R('cloak', 'cloak', 'Dragon Claw', 'Lots of magic resist. Every 2s, heals 1.2% max HP for each enemy targeting the holder (20% more on Dragons).', null, { mr: 40 }, { id: 'drg_dragon_claw', tags: ['tank'], aiRank: 3 }),
    R('cloak', 'belt', 'Zephyr Gale', 'At the start of combat, the enemy standing opposite the holder is blown away and stunned for 4 seconds.', null, {}, { id: 'drg_zephyr', tags: ['tank'], aiRank: 1 }),
    R('cloak', 'glove', 'Quick Silver', 'Immune to stuns for the first 15 seconds. +20% attack speed.', { ccImmune: 15 }, { as: 0.2 }, { id: 'drg_quicksilver', tags: ['ad', 'tank'], aiRank: 1 }),
    R('belt', 'belt', 'Giga Belt', 'A lot of HP.', null, { hp: 500 }, { id: 'drg_giga_belt', tags: ['tank'], aiRank: 3 }),
    R('belt', 'glove', 'Banshee Veil', 'At the start of combat, the holder and allies next to it in the same row gain a shield (200/300/450 by form).', null, {}, { id: 'drg_banshee', tags: ['tank'], aiRank: 0 }),
    R('glove', 'glove', 'Thief Glove', 'Each combat, gains two random completed items.', { thief: true }, {}, { id: 'drg_thief_glove', tags: ['ad'], aiRank: 0 }),
  ];
  // Dragon Egg + component = Crest. TFT Set 7 Spatula recipes; the rest drop from loot, the Armory and augments.
  const emblems = { sword: 'GOLD', bow: 'RAGE', rod: 'TAMER', tear: 'MAGE', vest: 'RIDER', cloak: 'MIRAGE', belt: 'GUARD', glove: 'ASSN' };
  const dropEmblems = ['STAR', 'XAB', 'STONE', 'SLAYER', 'THUNDER', 'WHISPER', 'LAGOON', 'BRUISER', 'CANNON', 'EVOKER', 'MYSTIC', 'MODE', 'SWIFT', 'WARRIOR'];

  // Golden Armor items (Shimmerscale), handed out by the trait; Hyper Roll values (gold counts up to 15).
  const GI = (id, name, desc, stats, tags, extra) => ({ id, name, desc, cat: 'gold', noDrop: true, stats, tags, color: '#ffcf4a', color2: '#7a4a10', ...(extra || {}) });
  const goldItems = [
    GI('drg_g_axe', 'Treasure Axe', '+3% attack damage per gold you held at the start of the round (up to 15). Every 50 basic attacks (counted across combats) pay 4 gold and a component.', { ad: 0.1, as: 0.1 }, ['ad']),
    GI('drg_g_blade', "Gambler's Blade", '+3% attack speed per gold you held at the start of the round (up to 15). Basic attacks have a 10% chance to drop 1 gold (up to 3 per combat).', { ap: 20, as: 0.2 }, ['ad', 'ap']),
    GI('drg_g_staff', "Goldmancer's Staff", '+4 AP per gold you held at the start of the round (up to 15). Kills have an 80% chance to drop 2 gold (up to 6 per combat).', { ap: 20, mana: 20 }, ['ap']),
    GI('drg_g_mail', "Mogul's Mail", 'Taking damage grants 1 armor, 1 MR and 8 max HP, up to 40 stacks. At full stacks, gain 2 gold.', { hp: 250 }, ['tank']),
    GI('drg_g_gem', 'Big Gem', 'If the holder is alive after 15 seconds, your team deals 15% more damage plus 2% per gold you held at the start of the round (up to 15), and you gain 1 gold for every 2 allies alive.', { hp: 400 }, ['tank']),
    GI('drg_g_investor', 'Determined Investor', 'After the holder is knocked out 4 times (counted across combats), this turns into Diamond Hands and you gain a Champion Duplicator and 8 gold.', { hp: 150 }, ['tank']),
    GI('drg_g_diamond', 'Diamond Hands', 'At 66% and 33% HP, become untouchable for 1 second and gain 1 gold.', { ap: 20, hp: 200 }, ['tank'], { color2: '#7fe8ff' }),
    GI('drg_g_crown', 'Golden Crown', 'Every 4 seconds, the next basic attack deals bonus true damage equal to 20 times the gold you held at the start of the round (up to 15).', { hp: 100, ad: 0.15 }, ['ad']),
  ];
  // Shining items (TFT Radiant): from the Order Treasure Dragon, the Treasure Dragon's best bundles and Shining Relics.
  const SI = (id, name, desc, stats, p, tags, extra) => ({ id, name, desc, cat: 'radiant', noDrop: true, stats, p: p || {}, tags, color: '#fff3a0', color2: '#ff8fd8', ...(extra || {}) });
  const radiant = [
    SI('drg_r_fang', 'Shining Dragon Fang', 'A huge amount of attack damage. Deals 15% more damage.', { ad: 0.75 }, { amp: 0.15 }, ['ad']),
    SI('drg_r_gauntlet', 'Shining Jewel Gauntlet', 'Abilities can critically strike. +40% crit chance and +40% crit damage.', { ap: 60, crit: 0.4 }, { abilCrit: true, critDmg: 0.4 }, ['ap']),
    SI('drg_r_seraph', 'Shining Seraph Rod', 'Gain 30 AP every 4 seconds (counted every 5).', { ap: 50, mana: 15 }, { archangel: 30 }, ['ap']),
    SI('drg_r_spike', 'Shining Spike Plate', 'Huge armor. When attacked, deals 150 magic damage to adjacent enemies (1.5s cooldown).', { armor: 110 }, { thorns: 150 }, ['tank']),
    SI('drg_r_belt', 'Shining Giga Belt', 'A huge amount of HP. Regenerates 1% max HP per second.', { hp: 1100 }, { regen: 0.01 }, ['tank']),
    SI('drg_r_blood', 'Shining Blood Fang', 'Heals for 40% of damage dealt. Once below 40% HP, gain a 40% max HP shield.', { ad: 0.35, mr: 20 }, { vamp: 0.4, lifeline: { at: 0.4, shield: 0.4 } }, ['ad']),
    SI('drg_r_rage', 'Shining Rage Blade', 'Each basic attack grants 10% attack speed, stacking.', { ap: 15, as: 0.4 }, { rage: 0.1 }, ['ad', 'ap']),
    SI('drg_r_ring', 'Shining Holy Ring', 'Every 5s, heals allies within 2 hexes for 80 + 10% of missing HP. Takes 15% less damage.', { hp: 450, mana: 15 }, { redemption: true, dr: 0.15 }, ['tank']),
  ];
  const consumables = [
    { id: 'drg_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'drg_reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
    { id: 'drg_dup_lesser', consumable: true, name: 'Lesser Duplicator', short: 'LD', color: '#9fdcff', color2: '#3a6aa0', desc: 'Drag onto a Digimon of cost 3 or less to put a Rookie-form copy of it on your bench.' },
    { id: 'drg_dup', consumable: true, noDrop: true, name: 'Champion Duplicator', short: 'CD', color: '#ffd84a', color2: '#3a6aa0', desc: 'Drag onto any Digimon to put a Rookie-form copy of it on your bench.' },
  ];

  // ---------- augments ----------
  // tier 1 Silver, 2 Gold, 3 Prismatic. Rounds 2-1, 3-2 and 4-2, one reroll per game. Hooks are in the mech file.
  const A = (id, tier, name, desc, tags) => ({ id, tier, name, desc, tags });
  const augments = [
    A('drg_hoard', 1, 'Small Hoard', 'Gain 8 gold.', ['econ']),
    A('drg_feather', 1, 'Featherweights', 'Your 1 and 2-cost Digimon gain 25% attack speed.', ['combat']),
    A('drg_exiles', 1, 'Exiles', 'Your Digimon that start combat with no adjacent allies gain a 30% max HP shield.', ['combat']),
    A('drg_battlemage', 1, 'Battlemage', 'Your Digimon that start in the front two rows gain 20 armor and 20 AP.', ['combat']),
    A('drg_friends', 1, 'Best Friends', 'Your Digimon that start next to exactly one ally gain 15% attack speed and 20 armor.', ['combat']),
    A('drg_calcloss', 1, 'Calculated Loss', 'After losing a PvP combat, gain 2 gold and a free reroll next round.', ['econ']),
    A('drg_firstaid', 1, 'First Aid Kit', 'All healing on your team is 30% stronger.', ['combat']),
    A('drg_training', 1, 'Combat Training', 'Your team gains 8% attack damage, plus 0.5% more for every kill your Digimon score this game (up to 40 kills).', ['combat']),
    A('drg_duplicates', 1, 'Lesser Duplicates', 'Gain 2 Lesser Duplicators.', ['econ']),
    A('drg_scraps', 1, 'Scrap Heap', 'Gain a random component and 3 gold.', ['items']),
    A('drg_ascension', 2, 'Ascension', 'After 15 seconds of combat, your team deals 40% more damage.', ['combat']),
    A('drg_celestial', 2, 'Celestial Blessing', 'Your team heals for 18% of all damage dealt.', ['combat']),
    A('drg_implants', 2, 'Cybernetic Implants', 'Your Digimon holding an item gain 200 HP and 15% attack damage.', ['combat']),
    A('drg_shell', 2, 'Cybernetic Shell', 'Your Digimon holding an item gain 200 HP and 30 armor.', ['combat']),
    A('drg_uplink', 2, 'Cybernetic Uplink', 'Your Digimon holding an item gain 200 HP and 3 mana every second.', ['combat']),
    A('drg_grabbag', 2, 'Component Grab Bag', 'Gain 3 random components.', ['items']),
    A('drg_archives', 2, 'Ancient Archives', 'Gain a random Crest, preferring traits on your board.', ['items']),
    A('drg_imperialist', 2, 'Dragon Imperialist', 'Gain a Dorumon. If you have exactly one Dragon on your board, your Dragons finish enemies they hit below 18% HP.', ['combat']),
    A('drg_axiom', 2, 'Axiom Arc', 'Your Digimon gain 30 mana when they score a kill.', ['combat']),
    A('drg_bigfriend', 2, 'Big Friend', 'Your Digimon that start next to an ally with 1800 or more max HP take 15% less damage.', ['combat']),
    A('drg_double', 2, 'Double Trouble', 'If you have exactly 2 copies of a Digimon on your board, both gain 25% attack damage, 25 AP, 25 armor and 25 MR.', ['combat']),
    A('drg_electro', 2, 'Electrocharge', 'When your Digimon take a critical strike, they deal magic damage to adjacent enemies (1 second cooldown).', ['combat']),
    A('drg_treasure_hoard', 2, 'Treasure Hoard', 'Gain 4 gold. The Treasure Dragon offers 2 more bundles and 1 more reroll.', ['econ']),
    A('drg_built', 2, 'Built Different', 'Your Digimon with no active traits gain 250 HP plus 60 per stage, and 40% attack speed.', ['combat']),
    A('drg_crest_gold', 2, 'Golden Armor Crest', 'Gain a Golden Armor Crest and a Kamemon.', ['items']),
    A('drg_crest_rage', 2, 'Berserk Crest', 'Gain a Berserk Crest and an Impmon.', ['items']),
    A('drg_crest_tamer', 2, 'Dragon Tamer Crest', 'Gain a Dragon Tamer Crest and a Gotsumon.', ['items']),
    A('drg_crest_mage', 2, 'Mage Crest', 'Gain a Mage Crest, a Syakomon and a Tsukaimon.', ['items']),
    A('drg_jackpot', 3, 'Golden Hoard', 'Gain 30 gold.', ['econ']),
    A('drg_relics', 3, 'Shining Relics', 'Gain a random Shining item.', ['items']),
    A('drg_crown', 3, "Tamer's Crown", "Gain a Tamer's Crown (+1 team size while held on your board).", ['items']),
    A('drg_wings', 3, 'Wings of the Dragon', 'Your Dragons gain 40% max HP and deal 30% more damage.', ['combat']),
    A('drg_pact', 3, 'Dragon Pact', 'Gain a random 4-cost Dragon and permanently +1 team size.', ['econ']),
    A('drg_final', 3, 'Final Ascension', 'Your team deals 15% more damage, and 60% more after 15 seconds of combat.', ['combat']),
  ];

  DT.Sets.register({
    id: 'dragonlands',
    name: 'Dragonlands',
    mode: 'tft',
    order: 6,
    desc: 'Mega Dragons that take two team slots, a Treasure Dragon round, Golden Armor gold items and fixed augment rounds with one reroll. Hyper Roll rules.',
    colors: { main: '#ff6a3d', accent: '#ffcf4a' },
    source: 'TFT Set 7 Dragonlands and Set 7.5 Uncharted Realms. docs/inspo/tft/set-07-dragonlands.md, docs/inspo/tft/set-07-5-uncharted-realms.md, docs/sets/dragonlands.md',
    traits: TRAITS,
    lines: LINES,
    pve: { wave: pveWave }, // reward() is added in dragonlands-mech.js
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'egg',
      emblems,
      dropEmblems,
      dropEmblemSource: 'loot, the Armory, the Treasure Dragon and the Ancient Archives augment', // codex: where drop-only Crests come from
      // Crest ids 'drg_emblem_<trait>': other sets share some of this set's trait ids
      emblemIdPrefix: 'drg_',
      emblemName: (T) => `${T.name} Crest`,
      special: [{ id: 'drg_crown', name: "Tamer's Crown", desc: '+1 team size while equipped on a Digimon on your board.', p: { teamSize: 1 }, stats: {},
        from: ['egg', 'egg'], color: '#ffd84a', color2: '#ffb46a', short: 'TC' }],
      list: [...goldItems, ...radiant, ...consumables],
    },
    augments,
    // Hyper Roll economy (fixed income by stage, no interest or streaks, levels by stage). Set 7 round changes:
    // augments at 2-1, 3-2 and 4-2 (one reroll for the whole game, Gold the most common tier), the Treasure Dragon
    // at 4-4 with its pick at 5-1, Armory at 3-1, 4-1 and 6-1.
    rules: {
      startHp: 20,
      startGold: 3,
      rerollCost: 1,
      winGold: 2,
      poolSize: [0, 30, 25, 18, 10, 9],
      levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
      incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
      lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
      pveLossDamage: (stage) => (stage === 1 ? 1 : stage === 4 ? 0 : null),
      roundsInStage: (stage) => (stage === 1 ? 3 : 4),
      roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'pve' : 'pvp'),
      carouselTiers: (stage) => (stage <= 1 ? [1, 2] : [2, 3, 4]),
      armoryStages: [3, 4, 6],
      armoryRound: 1,
      augmentStages: [],
      augmentRerolls: 1,
      augmentTierOdds: [0.25, 0.85],
      offers: [
        { kind: 'augment', stage: 2, round: 1, rerolls: 1 },
        { kind: 'augment', stage: 3, round: 2, rerolls: 1 },
        { kind: 'augment', stage: 4, round: 2, rerolls: 1 },
        { kind: 'drg_treasure', stage: 5, round: 1, rerolls: 1 },
      ],
      copies: 3,
      // Normal pace (standard TFT rounds, docs/sets/FORMAT.md Pace): the same augment rounds and Treasure Dragon,
      // with the dragon on the 4-7 wild round and its pick at 5-1. Items come from carousels and wild rounds.
      normal: {
        augmentRounds: [],
        offers: [
          { kind: 'augment', stage: 2, round: 1, rerolls: 1 },
          { kind: 'augment', stage: 3, round: 2, rerolls: 1 },
          { kind: 'augment', stage: 4, round: 2, rerolls: 1 },
          { kind: 'drg_treasure', stage: 5, round: 1, rerolls: 1 },
        ],
        pveLossDamage: (stage) => (stage === 1 || stage === 4 ? 0 : null),
        // Normal reaches level 9-10, where 3-Dragon boards were too strong: the Dragon trait turns off with 3+
        // Dragons, and from level 8 each Dragon a shop rolls is swapped for a non-Dragon of its cost 60% of the time
        drgTraitMax: 2,
        drgShopCut: { level: 8, chance: 0.6 },
      },
    },
    // AI hints: Dragons are worth a little more to the AI (they replace two units), statues are never fielded.
    lineAi: (line) => (line.slots === 2 ? { value: 6 } : {}),
    codexRules: () => [
      { id: 'drg-dragons', title: 'Dragons', html: 'Dragons take 2 team slots and cost 5 (3-cost), 6 (4-cost) or 7 (5-cost) gold. Each Dragon counts as 3 Digimon for its marked trait (the first trait listed on it). The Dragon trait adds max HP, then armor and magic resist, then damage for 1, 2 and 3 Dragons. Selling a Dragon refunds what you paid. In Normal pace the Dragon trait gives nothing with 3 or more Dragons, and from level 8 a Dragon in your shop is often swapped for another Digimon of the same cost.' },
      { id: 'drg-treasure', title: 'Treasure Dragon', html: 'Round 4-4 is the Treasure Dragon. One version is rolled for the whole lobby: GoldVeedramon (classic), Magnadramon (Order: one bundle is always a Shining item) or Megidramon (Chaos: mystery bundles that can be very good or poor). At 5-1 pick 1 of 4 loot bundles (3 if you lost) with one free reroll.' },
      { id: 'drg-augments', title: 'Augments', html: 'Augments come at 2-1, 3-2 and 4-2. You have one augment reroll for the whole game. Gold augments are the most common tier.' },
      { id: 'drg-golden', title: 'Golden Armor', html: 'Golden Armor hands out golden items at the start of each round. They grow with the gold you hold (up to 15) and some pay out gold. Golden items on the item bench or on benched Digimon vanish each round and are handed out again.' },
    ],
    tamers: ['Takato', 'Rika', 'Henry', 'Ryo', 'Jeri', 'Kazu', 'Kenta'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
