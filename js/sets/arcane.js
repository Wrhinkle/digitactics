// Into the Arcane (TFT Set 13 Into the Arcane, docs/inspo/tft/set-13-into-the-arcane.md), on Digimon.
// 60 shop lines (13/13/13/11/10), 26 traits (15 factions, 11 classes), 10 components (8 stat plugins, the Crest Tag and
// the Digivice), 36 recipes, 21 Crests, 3 Tamer items, 12 artifacts, 16 Syndicate contraband items, 4 consumables,
// 30 augments, Anomalies (the set mechanic), opening portals, the Syndicate black market and the Junk King workshop.
// Behavior (traits, items, augments, Anomalies, markets) is in arcane-mech.js; art and move looks in arcane-art.js.
// Design notes and balance results: docs/sets/arcane.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;
  const L = DT.line;

  // ---------- traits ----------
  // lv values per breakpoint; extra arrays are read with c.val(key) in arcane-mech.js.
  const TRAITS = {
    // ---- factions ----
    ACA: { name: 'Witchelny Academy', kind: 'faction', color: '#7fc8ff', bp: [3, 4, 5], lv: [0.03, 0.04, 0.06], base: [0.12, 0.17, 0.24],
      desc: 'The Academy sponsors 3 items each game. Every copy of a sponsored item gives its holder bonus max HP and damage; Academy Digimon get double. Academy Digimon also gain HP and damage on their own, and you receive a sponsored item the first time you reach each breakpoint.',
      crestSymbol: 'star6' },
    AUT: { name: 'Clockwork', kind: 'faction', color: '#c9a66b', bp: [2, 4], lv: [180, 420], arm: [25, 55],
      desc: 'Clockwork Digimon gain armor and magic resist. Every 12 hits they deal, they release a blast at their target for magic damage plus 25% of the damage they dealt since the last blast.',
      crestSymbol: 'chip' },
    BR: { name: 'Dark Area', kind: 'faction', color: '#9b3dff', bp: [3, 4, 5], lv: [1, 1.15, 1.3],
      desc: 'Summon a chained Belphemon at the front of your board. It breaks free after Dark Area Digimon cast 5 times or when it drops below 65% HP. It grows with the star levels of your Dark Area Digimon. With 4, Dark Area Digimon deal 15% more damage. With 5, Belphemon heals to full and erupts when freed.',
      crestSymbol: 'moon' },
    SYN: { name: 'Kowloon Syndicate', kind: 'faction', color: '#e05ad8', bp: [3, 4, 5, 6], lv: [0.08, 0.12, 0.15, 0.2], hp: [0.06, 0.1, 0.14, 0.2],
      gain: [30, 30, 40, 50], streakGain: [60, 75, 95, 120], skipHp: [80, 140, 220, 320],
      desc: 'Syndicate Digimon gain max HP and heal for a share of the damage they deal. After each player combat you gain Shimmer (more on a losing streak of 3 or more). Every 100 Shimmer opens the black market: pick a contraband item only Syndicate Digimon can use, or pass to give Syndicate Digimon permanent max HP.',
      crestSymbol: 'drop' },
    CON: { name: 'Bagra Army', kind: 'faction', color: '#d1453a', bp: [2, 4, 6], lv: [0.15, 0.24, 0.34], mult: [1, 3, 6],
      desc: 'Bagra Army Digimon gain attack damage and ability power. Their takedowns earn Conquest; every 10 Conquest opens a war chest with loot, and each chest opened gives the Bagra Army 6% more attack damage and ability power for the rest of the game.',
      crestSymbol: 'sword' },
    EMI: { name: 'Sovereign Envoys', kind: 'faction', color: '#5fd6c8', bp: [1, 4], lv: [1, 1],
      desc: 'Only active with exactly 1 or 4 different Envoys. Terriermon: allies gain 5% attack speed per star. Syakomon: allies restore 3 more mana per attack. Ryudamon: allies gain 10 armor and magic resist. Hackmon: Hackmon and the allies beside it gain 12% of its max HP. With 4, you get every bonus and Envoys gain 250 HP and 20% more damage.',
      crestSymbol: 'cross' },
    ENF: { name: 'DigiPolice', kind: 'faction', color: '#4f8dff', bp: [2, 4, 6], lv: [0.12, 0.2, 0.32], sh: [0.12, 0.18, 0.25], n: [1, 2, 4],
      desc: 'DigiPolice start combat with a shield and deal more damage. The enemies with the highest max HP are WANTED; when a Wanted enemy is knocked out, DigiPolice gain 30% attack speed.',
      crestSymbol: 'badge' },
    EXP: { name: 'Chimera Lab', kind: 'faction', color: '#7ed957', bp: [3, 5], lv: [100, 350], labs: [2, 3],
      desc: 'Your strongest Chimera Lab Digimon (2, or 3 with 5) are upgraded in the Laboratory: each gains max HP and the Lab bonus of every Lab Digimon. Lab bonuses: Aruraumon, abilities deal 20% extra true damage; Crabmon, casting gives a shield and attack speed; Penguinmon, damage adds 3% of the target\'s max HP; Tapirmon, +200 max HP and growth on takedowns; FanBeemon, every 5th attack stings the nearest enemy for 6% of its max HP.',
      crestSymbol: 'bug' },
    FAM: { name: 'Found Family', kind: 'faction', color: '#ff9e5a', bp: [2, 3], lv: [0.25, 0.35],
      desc: 'Family members need less mana to cast and take 12% less damage. With 3, they also gain 20% attack speed.',
      crestSymbol: 'heart' },
    FIRE: { name: 'Firelight', kind: 'faction', color: '#ffd23f', bp: [2, 3], lv: [0.3, 0.45],
      desc: 'Every 6 seconds Firelights surge, healing a share of the damage they took since the last surge. With 3, each surge also gives 60% attack speed for 2 seconds.',
      crestSymbol: 'sun' },
    MODE: { name: 'Mode Change', kind: 'faction', color: '#ff7ab8', bp: [2, 3, 4], lv: [0.12, 0.2, 0.3], dr: [0.08, 0.14, 0.22],
      desc: 'Mode Change Digimon change form by row (always, even without the trait): in the front two rows they gain 10% max HP and their ability guards; in the back two rows they gain range 4 and their ability blasts. With the trait, front-row ones take less damage and back-row ones deal more damage.',
      crestSymbol: 'xros' },
    REB: { name: 'Digimon Rebels', kind: 'faction', color: '#ff5d5d', bp: [3, 5, 7], lv: [0.1, 0.22, 0.32], hp: [0.07, 0.12, 0.16],
      desc: 'Rebels gain max HP. When your team has lost 25% of its HP, a signal flare goes up: Rebels gain 60% attack speed for 5 seconds and more attack damage and ability power for the rest of combat. With 7, the flare also stuns all enemies for 2 seconds.',
      crestSymbol: 'flame' },
    SCR: { name: 'Junkyard', kind: 'faction', color: '#a0a8b8', bp: [2, 4, 6], lv: [20, 30, 40], base: [150, 280, 450], n: [1, 3, 99],
      desc: 'Junkyard Digimon start combat with a shield, plus more for every component your team holds (completed items count their two). Components held by Junkyard Digimon (1, 3, then all) act as full items in combat.',
      crestSymbol: 'chip' },
    HR: { name: 'High Roller', kind: 'faction', color: '#ffcf4a', bp: [1], lv: [1],
      desc: 'Each cast, BlackWarGreymon rolls a bonus: 8 rockets at random enemies, a big shield, a coin (1 gold after combat, up to 3), or a jackpot that hits its target again. It also takes 50% less damage for 1.5 seconds.',
      crestSymbol: 'die' },
    JUNK: { name: 'Junk King', kind: 'faction', color: '#ff8a1f', bp: [1], lv: [1],
      desc: 'Every 2 rounds while you own Chaosdromon, pick a permanent upgrade for its mech from the workshop. Owning all five gives a finishing bonus.',
      crestSymbol: 'chess' },
    // ---- classes ----
    AMB: { name: 'Ambusher', kind: 'class', color: '#ff4f7b', bp: [2, 3, 4, 5], lv: [0.25, 0.35, 0.45, 0.55], cd: [0.15, 0.25, 0.3, 0.35],
      desc: 'Ambushers\' abilities can critically strike, and they gain critical strike chance and damage. With 5, they also take 15% less damage.', crestSymbol: 'dagger' },
    ART: { name: 'Cannoneer', kind: 'class', color: '#ff8f5a', bp: [2, 4], lv: [0.22, 0.5],
      desc: 'Cannoneers gain attack damage. Every 4th attack fires a shell that hits the target and enemies next to it for 140% of their attack damage.', crestSymbol: 'crosshair' },
    BRU: { name: 'Bruiser', kind: 'class', color: '#d9773a', bp: [2, 4, 6], lv: [0.1, 0.22, 0.4],
      desc: 'Your team gains 80 max HP. Bruisers gain a share of their max HP on top.', crestSymbol: 'shield' },
    DOM: { name: 'Dominator', kind: 'class', color: '#c45cff', bp: [2, 4, 6], lv: [0.25, 0.45, 0.65], sh: [300, 480, 700],
      desc: 'Dominators start combat with a shield. When they cast, they gain ability power equal to a share of the mana spent, stacking.', crestSymbol: 'star6' },
    PIT: { name: 'Pit Fighter', kind: 'class', color: '#e0405a', bp: [2, 4, 6], lv: [0.04, 0.08, 0.14], heal: [0.1, 0.25, 0.4],
      desc: 'Pit Fighters heal for 10% of their damage and deal bonus true damage. Once per combat at 50% HP, they heal a share of their max HP over 2 seconds.', crestSymbol: 'blood' },
    QS: { name: 'Quickstriker', kind: 'class', color: '#9be15d', bp: [2, 3, 4], lv: [0.25, 0.35, 0.45], hi: [0.65, 0.85, 1.05],
      desc: 'Quickstrikers gain attack speed based on their target\'s missing HP. With 4, a takedown also gives them a 200 HP shield.', crestSymbol: 'chevrons' },
    SEN: { name: 'Sentinel', kind: 'class', color: '#d9dfe9', bp: [2, 4, 6], lv: [5, 10, 18],
      desc: 'Your team gains armor and magic resist. Sentinels gain triple.', crestSymbol: 'shieldIron' },
    SNI: { name: 'Sniper', kind: 'class', color: '#5ad1ff', bp: [2, 4], lv: [0.08, 0.22], hex: [0.05, 0.08],
      desc: 'Snipers deal more damage, and more again for each hex between them and their target.', crestSymbol: 'crosshair' },
    SOR: { name: 'Sorcerer', kind: 'class', color: '#d48bff', bp: [2, 4, 6], lv: [15, 35, 60],
      desc: 'Your team gains 8 ability power. Sorcerers gain more. With 6, their abilities make the target deal 20% less damage for 3 seconds.', crestSymbol: 'star6' },
    VIS: { name: 'Visionary', kind: 'class', color: '#9cf6c0', bp: [2, 4, 6], lv: [0.35, 0.65, 1.0],
      desc: 'Visionaries gain more mana from attacks and from damage taken. With 6, their ability damage heals the lowest-health ally for 16% of it.', crestSymbol: 'pearl' },
    WAT: { name: 'Watcher', kind: 'class', color: '#8fb4d9', bp: [2, 4, 6], lv: [0.1, 0.18, 0.28], hi: [0.2, 0.3, 0.4],
      desc: 'Watchers take less damage, more while above 50% HP.', crestSymbol: 'shield' },
  };

  // codex text per breakpoint, built from the numbers above
  {
    const T = TRAITS;
    const each = (id, f) => { T[id].bpDesc = T[id].lv.map((v, i) => f(v, i, T[id])); };
    each('ACA', (v, i, t) => `${pct(v)} HP and damage per sponsored copy (x2 on Academy). Academy +${pct(t.base[i])} HP and damage. +1 sponsored item`);
    each('AUT', (v, i, t) => `+${t.arm[i]} armor and MR. Blast: ${v} magic damage + 25% of damage dealt`);
    each('BR', (v, i) => [`Summon Belphemon (power x${v})`, `Belphemon power x${v}; Dark Area deals 15% more damage`, `Belphemon power x${v}; heals to full and erupts when freed`][i]);
    each('SYN', (v, i, t) => `+${pct(t.hp[i])} HP, ${pct(v)} omnivamp. ${t.gain[i]} Shimmer per combat (${t.streakGain[i]} on a 3+ loss streak). Pass: +${t.skipHp[i]} HP`);
    each('CON', (v, i, t) => `+${pct(v)} AD and AP, ${t.mult[i]}x Conquest`);
    each('EMI', (v, i) => (i === 0 ? 'Exactly 1 Envoy: gain its bonus' : 'All 4: every bonus; Envoys +250 HP and 20% damage'));
    each('ENF', (v, i, t) => `${pct(t.sh[i])} max HP shield, ${pct(v)} more damage, ${t.n[i]} Wanted`);
    each('EXP', (v, i, t) => `${t.labs[i]} Lab Digimon, +${v} HP each`);
    each('FAM', (v, i) => `${pct(v)} less max mana, 12% less damage taken${i ? ', +20% AS' : ''}`);
    each('FIRE', (v, i) => `Surge heals ${pct(v)} of damage taken${i ? ', +60% AS for 2s' : ''}`);
    each('MODE', (v, i, t) => `Front: ${pct(t.dr[i])} less damage. Back: ${pct(v)} more damage`);
    each('REB', (v, i, t) => `+${pct(t.hp[i])} HP. Flare: +${pct(v)} AD and AP${i === 2 ? ', stun all enemies 2s' : ''}`);
    each('SCR', (v, i, t) => `${t.base[i]} shield + ${v} per component. ${t.n[i] > 9 ? 'All' : t.n[i]} component${t.n[i] > 1 ? 's' : ''} act as full items`);
    each('HR', () => 'Roll a bonus on every cast');
    each('JUNK', () => 'Workshop upgrade every 2 rounds');
    each('AMB', (v, i, t) => `Abilities crit. +${pct(v)} crit, +${pct(t.cd[i])} crit damage${i === 3 ? ', 15% less damage taken' : ''}`);
    each('ART', (v) => `+${pct(v)} AD. Shell every 4th attack`);
    each('BRU', (v) => `Team +80 HP. Bruisers +${pct(v)} max HP`);
    each('DOM', (v, i, t) => `${t.sh[i]} shield. Cast: AP equal to ${pct(v)} of mana spent`);
    each('PIT', (v, i, t) => `10% omnivamp, +${pct(v)} true damage, heal ${pct(t.heal[i])} at 50% HP`);
    each('QS', (v, i, t) => `${pct(v)} to ${pct(t.hi[i])} attack speed${i === 2 ? ', 200 shield on takedown' : ''}`);
    each('SEN', (v) => `Team +${v} armor and MR, Sentinels +${v * 3}`);
    each('SNI', (v, i, t) => `${pct(v)} more damage, +${pct(t.hex[i])} per hex`);
    each('SOR', (v, i) => `Team +8 AP, Sorcerers +${v} AP${i === 2 ? '; abilities weaken 20%' : ''}`);
    each('VIS', (v, i) => `${pct(v)} more mana${i === 2 ? '; ability damage heals an ally' : ''}`);
    each('WAT', (v, i, t) => `${pct(v)} less damage, ${pct(t.hi[i])} above 50% HP`);
  }

  // ---------- lines ----------
  // src: the TFT Set 13 champion each line adapts. exp: Chimera Lab bonus, envoy: Sovereign Envoy bonus, swap: Mode Change.
  const U = { unassigned: true };
  const LINES = [
    // Tier 1 (13)
    L('arc_kokuwamon', 1, ['AUT', 'WAT'], 'Data', ['Kokuwamon', 'Guardromon', 'Andromon'], ['Electro Strike', 'Destruction Grenade', 'Spiral Sword'], 'slam', { ...U, role: 'Guardian', src: 'TFT13 Amumu' }),
    L('arc_goblimon', 1, ['CON', 'WAT'], 'Virus', ['Goblimon', 'Ogremon', 'Grademon'], ['Goblin Strike', 'Pummel Whack', 'Grade Slash'], 'drain', { ...U, role: 'Bruiser', radius: 1, src: 'TFT13 Darius' }),
    L('arc_impmon', 1, ['CON', 'PIT'], 'Virus', ['Impmon', 'Meramon', 'SkullMeramon'], ['Night of Fire', 'Burning Fist', 'Heat Chain'], 'barrage', { ...U, role: 'Sniper', shots: 3, src: 'TFT13 Draven' }),
    L('arc_zubamon', 1, ['REB', 'SEN'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Duramon'], ['Twenty Dive', 'Road to Decade', 'Glass Slash'], 'shield', { ...U, role: 'Guardian', src: 'TFT13 Irelia' }),
    L('arc_lalamon', 1, ['ACA', 'SOR'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Nuts Gun', 'Sunshine Beam', 'Lila Shower'], 'nuke', { ...U, role: 'Mystic', src: 'TFT13 Lux' }),
    L('arc_commandramon', 1, ['ENF', 'SNI'], 'Virus', ['Commandramon', 'Sealsdramon', 'Darkdramon'], ['M16 Assassin', 'Death Behind', 'Chaos Degradation'], 'barrage', { ...U, role: 'Sniper', shots: 5, range: 5, src: 'TFT13 Maddie' }),
    L('arc_tsukaimon', 1, ['BR', 'VIS'], 'Virus', ['Tsukaimon', 'Bakemon', 'Phantomon'], ['Bad Message', "Hell's Hand", 'Soul Chopper'], 'weaken', { ...U, role: 'Mystic', src: 'TFT13 Morgana' }),
    L('arc_chuumon', 1, ['FAM', 'SCR', 'AMB'], 'Virus', ['Chuumon', 'Mamemon', 'MetalMamemon'], ['Cheese Bomb', 'Smiley Bomb', 'Energy Bomb'], 'aoe', { ...U, role: 'Mystic', range: 4, levels: ['Rookie', 'Ultimate', 'Ultimate'], src: 'TFT13 Powder' }),
    L('arc_mushroomon', 1, ['SYN', 'SEN'], 'Virus', ['Mushroomon', 'RedVegiemon', 'Vademon'], ['Fungus Crusher', 'Hazard Breath', 'Unidentified Flying Kiss'], 'rally', { ...U, role: 'Guardian', hpMul: 1.1, src: 'TFT13 Singed' }),
    L('arc_snowgoblimon', 1, ['SCR', 'BRU'], 'Virus', ['SnowGoblimon', 'Hyogamon', 'IceLeomon'], ['Snow Goblin Volt', 'Icy Cudgel', 'Fist of Ice'], 'drain', { ...U, role: 'Bruiser', src: 'TFT13 Trundle' }),
    L('arc_psychemon', 1, ['REB', 'VIS'], 'Data', ['Psychemon', 'Gururumon', 'WereGarurumon'], ['Colored Spark', 'Chaos Fire', 'Wolf Claw'], 'aoe', { ...U, role: 'Mystic', mana: 50, hpMul: 1.1, src: 'TFT13 Vex' }),
    L('arc_gaomon', 1, ['FAM', 'PIT'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Gao Rush', 'Spiral Blow', 'Gaoga Tornado'], 'stun', { ...U, role: 'Duelist', src: 'TFT13 Violet' }),
    L('arc_aruraumon', 1, ['EXP', 'SOR'], 'Data', ['Aruraumon', 'Woodmon', 'Cherrymon'], ['Nemesis Ivy', 'Branch Drain', 'Cherry Bomb'], 'stun', { ...U, role: 'Mystic', mana: 50, exp: 'zyra', src: 'TFT13 Zyra' }),
    // Tier 2 (13)
    L('arc_hyokomon', 2, ['REB', 'QS'], 'Data', ['Hyokomon', 'Peckmon', 'Crowmon'], ['Downward Cleave', 'Kunai Wing', 'Savage Emperor'], 'dash', { ...U, role: 'Assassin', src: 'TFT13 Akali' }),
    L('arc_lopmon', 2, ['ENF', 'AMB'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Gauntlet Claw', 'Asipatravana'], 'execute', { ...U, role: 'Duelist', src: 'TFT13 Camille' }),
    L('arc_coronamon', 2, ['ACA', 'SEN'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Corona Flame', 'Flame Dive', 'Sol Blaster'], 'shield', { ...U, role: 'Guardian', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT13 Leona' }),
    L('arc_monodramon', 2, ['AUT', 'QS'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Crack Bite', 'Fang Strike', 'Cyber Nail'], 'dash', { ...U, role: 'Assassin', src: 'TFT13 Nocturne' }),
    L('arc_kotemon', 2, ['CON', 'SEN', 'VIS'], 'Data', ['Kotemon', 'Gladimon', 'Knightmon'], ['Meteor Slash', 'Gladius', 'Berserk Sword'], 'line', { ...U, role: 'Guardian', src: 'TFT13 Rell' }),
    L('arc_floramon', 2, ['SYN', 'VIS'], 'Data', ['Floramon', 'Hudiemon', 'Lillymon'], ['Rain of Pollen', 'Infinity Dream', 'Flower Wreath'], 'rally', { ...U, role: 'Support', mana: 55, hpMul: 1.1, src: 'TFT13 Renata Glasc' }),
    L('arc_jazamon', 2, ['REB', 'BRU', 'PIT'], 'Data', ['Jazamon', 'Jazardmon', 'Jazarichmon'], ['Jaza Claw', 'Laser Eye', 'Messer Spiral'], 'stun', { ...U, role: 'Bruiser', levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT13 Sett' }),
    L('arc_terriermon', 2, ['EMI', 'ART'], 'Vaccine', ['Terriermon', 'Gargomon', 'Rapidmon'], ['Blazing Fire', 'Gargo Laser', 'Miracle Missile'], 'nuke', { ...U, role: 'Sniper', envoy: 'tristana', src: 'TFT13 Tristana' }),
    L('arc_crabmon', 2, ['EXP', 'PIT', 'ART'], 'Data', ['Crabmon', 'Coelamon', 'Neptunemon'], ['Crab Meat Bomber', 'Fossil Bite', 'Pole Trident'], 'aoe', { ...U, role: 'Striker', range: 2, exp: 'urgot', src: 'TFT13 Urgot' }),
    L('arc_elecmon', 2, ['FAM', 'WAT'], 'Data', ['Elecmon', 'Leomon', 'SaberLeomon'], ['Sparkling Thunder', 'Beast Sword', 'Howling Crusher'], 'shield', { ...U, role: 'Guardian', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT13 Vander' }),
    L('arc_dracmon', 2, ['BR', 'WAT', 'SOR'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Undead Fang', 'Drain Slash', 'Thousand Arrows'], 'drain', { ...U, role: 'Bruiser', src: 'TFT13 Vladimir' }),
    L('arc_kamemon', 2, ['SCR', 'DOM'], 'Data', ['Kamemon', 'Tankmon', 'Gigadramon'], ['Pointer Arrow', 'Hyper Cannon', 'Giga Heat'], 'aoe', { ...U, role: 'Mystic', src: 'TFT13 Ziggs' }),
    L('arc_gotsumon', 2, ['ENF', 'BRU'], 'Data', ['Gotsumon', 'Golemon', 'Triceramon'], ['Rock Fist', 'Golem Punch', 'Tri-Horn Attack'], 'drain', { ...U, role: 'Bruiser', src: 'TFT13 Steb' }),
    // Tier 3 (13)
    L('arc_hagurumon', 3, ['AUT', 'DOM'], 'Virus', ['Hagurumon', 'Clockmon', 'Datamon'], ['Cog Crusher', 'Chrono Breaker', 'Digital Bomb'], 'shield', { ...U, role: 'Guardian', src: 'TFT13 Blitzcrank' }),
    L('arc_betamon', 3, ['BR', 'DOM'], 'Virus', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Winder', 'Lightning Javelin'], 'chain', { ...U, role: 'Mystic', src: 'TFT13 Cassiopeia' }),
    L('arc_shamanmon', 3, ['ACA', 'REB', 'ART'], 'Virus', ['Shamanmon', 'Sorcermon', 'Wisemon'], ['Shama Strike', 'Crystal Cloud', 'Pandora Dialogue'], 'aoe', { ...U, role: 'Sniper', adMul: 1.1, src: 'TFT13 Ezreal' }),
    L('arc_gomamon', 3, ['SCR', 'MODE', 'PIT'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Vikemon'], ['Big Wave Attack', 'Harpoon Torpedo', 'Mjolnir'], 'arc_swap', { ...U, role: 'Bruiser', swap: true, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT13 Gangplank' }),
    L('arc_keramon', 3, ['AUT', 'SNI'], 'Virus', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', "Hell's Grenade"], 'barrage', { ...U, role: 'Sniper', shots: 4, src: "TFT13 Kog'Maw" }),
    L('arc_armadillomon', 3, ['ENF', 'SEN'], 'Free', ['Armadillomon', 'Ankylomon', 'Shakkoumon'], ['Diamond Shell', 'Megaton Press', 'Disaster Blaster'], 'shield', { ...U, role: 'Guardian', src: 'TFT13 Loris' }),
    L('arc_syakomon', 3, ['EMI', 'SOR'], 'Virus', ['Syakomon', 'Whamon', 'MarineAngemon'], ['Black Pearl Blast', 'Tidal Wave', 'Ocean Love'], 'chain', { ...U, role: 'Mystic', envoy: 'nami', src: 'TFT13 Nami' }),
    L('arc_penguinmon', 3, ['EXP', 'BRU', 'VIS'], 'Vaccine', ['Penguinmon', 'Frigimon', 'Mammothmon'], ['Ice Prism', 'Sub-zero Ice Punch', 'Tusk Crusher'], 'slam', { ...U, role: 'Bruiser', radius: 2, exp: 'nunu', src: 'TFT13 Nunu & Willump' }),
    L('arc_numemon', 3, ['SYN', 'BRU'], 'Virus', ['Numemon', 'Raremon', 'BlackKingNumemon'], ['Party Time', 'Stinking Gas', 'Dark Pupil'], 'stun', { ...U, role: 'Bruiser', hpMul: 1.15, levels: ['Champion', 'Champion', 'Ultimate'], src: 'TFT13 Renni' }),
    L('arc_gazimon', 3, ['SYN', 'AMB'], 'Virus', ['Gazimon', 'Dobermon', 'Astamon'], ['Gazimon Claw', 'Grau Larm', 'Hellfire'], 'execute', { ...U, role: 'Assassin', adMul: 1.15, src: 'TFT13 Smeech' }),
    L('arc_saberdramon', 3, ['CON', 'MODE', 'SOR'], 'Vaccine', ['Saberdramon', 'Ravemon', 'Valkyrimon'], ['Black Saber', 'Blast Wing', 'Fenrir Sword'], 'arc_swap', { ...U, role: 'Bruiser', swap: true, levels: ['Champion', 'Mega', 'Mega'], src: 'TFT13 Swain' }),
    L('arc_renamon', 3, ['ENF', 'QS'], 'Data', ['Renamon', 'Kyubimon', 'Taomon'], ['Koyosetsu', 'Fox Tail Inferno', 'Talisman Spell'], 'stun', { ...U, role: 'Mystic', src: 'TFT13 Twisted Fate' }),
    L('arc_tentomon', 3, ['FIRE', 'SNI'], 'Vaccine', ['Tentomon', 'Kabuterimon', 'MegaKabuterimon'], ['Super Shocker', 'Electro Shocker', 'Horn Buster'], 'chain', { ...U, role: 'Sniper', adMul: 0.92, src: 'TFT13 Zeri' }),
    // Tier 4 (11)
    L('arc_ryudamon', 4, ['EMI', 'CON', 'QS'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Katana Attack', 'Metal Armor Blade', 'Legendary Dragon Blade'], 'execute', { ...U, role: 'Duelist', envoy: 'ambessa', src: 'TFT13 Ambessa' }),
    L('arc_hawkmon', 4, ['SCR', 'ART'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Top Gun'], 'barrage', { ...U, role: 'Sniper', shots: 5, adMul: 1.1, src: 'TFT13 Corki' }),
    L('arc_tapirmon', 4, ['EXP', 'DOM'], 'Vaccine', ['Tapirmon', 'Monochromon', 'Kimeramon'], ['Nightmare Syndrome', 'Volcanic Strike', 'Heat Viper'], 'drain', { ...U, role: 'Bruiser', radius: 1, hpMul: 0.92, exp: 'mundo', src: 'TFT13 Dr. Mundo' }),
    L('arc_kunemon', 4, ['BR', 'MODE', 'BRU'], 'Virus', ['Kunemon', 'DoKunemon', 'Arukenimon'], ['Electric Thread', 'Poison Cobweb', 'Spider Thread'], 'arc_swap', { ...U, role: 'Bruiser', swap: true, levels: ['Larva', 'Champion', 'Ultimate'], src: 'TFT13 Elise' }),
    L('arc_hackmon', 4, ['EMI', 'WAT'], 'Data', ['Hackmon', 'BaoHuckmon', 'SaviorHuckmon'], ['Fifth Rush', 'Fif Cross', 'Meteor Flame'], 'slam', { ...U, role: 'Bruiser', radius: 2, hpMul: 0.95, envoy: 'garen', src: 'TFT13 Garen' }),
    L('arc_gizamon', 4, ['REB', 'SEN'], 'Virus', ['Gizamon', 'Gesomon', 'Dragomon'], ['Spiral Edge', 'Deadly Shade', 'Forbidden Trident'], 'slam', { ...U, role: 'Guardian', radius: 2, src: 'TFT13 Illaoi' }),
    L('arc_demidevimon', 4, ['SYN', 'DOM'], 'Virus', ['DemiDevimon', 'Devimon', 'Myotismon'], ['Demi Darts', 'Deadly Nail', 'Night Raid'], 'aoe', { ...U, role: 'Mystic', mana: 55, src: 'TFT13 Silco' }),
    L('arc_fanbeemon', 4, ['EXP', 'SNI'], 'Virus', ['FanBeemon', 'Waspmon', 'CannonBeemon'], ['Gear Stinger', 'Turbo Stinger', 'Nitro Stinger'], 'barrage', { ...U, role: 'Sniper', shots: 5, exp: 'twitch', src: 'TFT13 Twitch' }),
    L('arc_bearmon', 4, ['ENF', 'PIT'], 'Vaccine', ['Bearmon', 'Grizzlymon', 'GrapLeomon'], ['Bear Fist', 'Maul Attack', 'The King of Fists'], 'stun', { ...U, role: 'Duelist', src: 'TFT13 Vi' }),
    L('arc_lunamon', 4, ['REB', 'SOR'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Tearing Shot', 'Tear Arrow', 'Lunatic Dance'], 'chain', { ...U, role: 'Mystic', src: 'TFT13 Zoe' }),
    L('arc_biyomon', 4, ['FIRE', 'WAT'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'slam', { ...U, role: 'Bruiser', radius: 2, src: 'TFT13 Scar' }),
    // Tier 5 (10)
    L('arc_kentaurosmon', 5, ['ENF', 'SNI'], 'Vaccine', ['Kentaurosmon', 'Kentaurosmon', 'Kentaurosmon X'], ['Bifrost', "Odin's Breath", 'Rivalry Shot'], 'line', { ...U, role: 'Sniper', range: 6, adMul: 1.15, hpMul: 1.1, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Caitlyn' }),
    L('arc_gallantmon', 5, ['ACA', 'MODE'], 'Virus', ['Gallantmon', 'Gallantmon', 'Gallantmon Crimson Mode'], ['Lightning Joust', 'Shield of the Just', 'Crimson Blast'], 'arc_swap', { ...U, role: 'Bruiser', swap: true, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Jayce' }),
    L('arc_beelzemon', 5, ['REB', 'AMB'], 'Virus', ['Beelzemon', 'Beelzemon X', 'Beelzemon Blast Mode'], ['Double Impact', 'Heartbreak Shot', 'Corona Destroyer'], 'arc_rotate', { ...U, role: 'Sniper', levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Jinx' }),
    L('arc_piedmon', 5, ['BR', 'SOR'], 'Virus', ['Piedmon', 'Piedmon', 'Piedmon'], ['Trump Sword', "Clown's Trick", 'Masks Square'], 'chain', { ...U, role: 'Mystic', levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 LeBlanc' }),
    L('arc_diaboromon', 5, ['AUT', 'VIS'], 'Virus', ['Diaboromon', 'Diaboromon', 'Armageddemon'], ['Catastrophe Cannon', 'System Failure', 'Ultimate Flare'], 'line', { ...U, role: 'Mystic', mana: 50, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Malzahar' }),
    L('arc_craniamon', 5, ['CON', 'DOM'], 'Vaccine', ['Craniamon', 'Craniamon', 'Craniamon X'], ["Waltz's End", 'Breath of the Gods', 'Claiomh Solais'], 'slam', { ...U, role: 'Bruiser', radius: 2, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Mordekaiser' }),
    L('arc_chaosdromon', 5, ['JUNK', 'SCR', 'SEN'], 'Virus', ['Chaosdromon', 'Chaosdromon', 'Breakdramon'], ['Hyper Infinity Cannon', 'Chaos Crusher', 'Infinity Bowling'], 'aoe', { ...U, role: 'Guardian', radius: 2, hpMul: 1.1, adMul: 1.1, levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Rumble' }),
    L('arc_blackwargreymon', 5, ['HR', 'SYN', 'PIT'], 'Virus', ['BlackMetalGreymon', 'BlackWarGreymon', 'BlackWarGreymon'], ['Terra Destroyer', 'Dark Terra Force', 'Black Tornado'], 'arc_gamble', { ...U, role: 'Duelist', levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT13 Sevika' }),
    L('arc_millenniummon', 5, ['FIRE', 'SCR', 'AMB'], 'Virus', ['Millenniummon', 'Millenniummon', 'Millenniummon'], ['Time Unlimited', 'Dimension Destroyer', 'Moon Destroyer'], 'chain', { ...U, role: 'Assassin', levels: ['Mega', 'Mega', 'Mega'], src: 'TFT13 Ekko (4-cost in TFT)' }),
    L('arc_witchmon', 5, ['ACA', 'VIS'], 'Virus', ['Witchmon', 'Witchmon', 'Witchmon'], ['Baluluna Gust', 'Aquary Pressure', 'Witchelny Storm'], 'arc_progress', { ...U, role: 'Mystic', levels: ['Ultimate', 'Ultimate', 'Ultimate'], src: 'TFT13 Heimerdinger (4-cost in TFT)' }),

    // Wild Digimon (PvE only; TFT13 monster camps re-skinned)
    L('arc_w_toyagumon', 1, [], 'Vaccine', ['ToyAgumon', 'ToyAgumon', 'ToyAgumon'], ['Toy Flame', 'Toy Flame', 'Toy Flame'], 'nuke', { ...U, role: 'Striker', shop: false, hpMul: 1.3, adMul: 1.15, levels: ['Rookie', 'Rookie', 'Rookie'] }),
    L('arc_w_otamamon', 1, [], 'Virus', ['Otamamon', 'Otamamon', 'Otamamon'], ['Lullaby Bubble', 'Lullaby Bubble', 'Lullaby Bubble'], 'nuke', { ...U, role: 'Mystic', shop: false, hpMul: 1.1 }),
    L('arc_w_kuwagamon', 2, [], 'Virus', ['Kuwagamon', 'Kuwagamon', 'Kuwagamon'], ['Scissor Claw', 'Scissor Claw', 'Scissor Claw'], 'nuke', { ...U, role: 'Striker', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('arc_w_gekomon', 2, [], 'Virus', ['Gekomon', 'Gekomon', 'Gekomon'], ['Symphony Crusher', 'Symphony Crusher', 'Symphony Crusher'], 'aoe', { ...U, role: 'Mystic', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('arc_w_tyrannomon', 3, [], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Fire Breath', 'Fire Breath', 'Fire Breath'], 'aoe', { ...U, role: 'Striker', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('arc_w_shogun', 4, [], 'Virus', ['ShogunGekomon', 'ShogunGekomon', 'ShogunGekomon'], ['Musical Fist', 'Musical Fist', 'Musical Fist'], 'slam', { ...U, role: 'Guardian', shop: false, boss: true, hpMul: 1.3, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('arc_w_skullgreymon', 4, [], 'Virus', ['SkullGreymon', 'SkullGreymon', 'SkullGreymon'], ['Dark Shot', 'Dark Shot', 'Dark Shot'], 'aoe', { ...U, role: 'Striker', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('arc_w_megadramon', 5, [], 'Virus', ['Megadramon', 'Megadramon', 'Megadramon'], ['Ultimate Slicer', 'Ultimate Slicer', 'Ultimate Slicer'], 'barrage', { ...U, role: 'Sniper', shop: false, boss: true, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('arc_w_metaltyranno', 5, [], 'Virus', ['MetalTyrannomon', 'MetalTyrannomon', 'MetalTyrannomon'], ['Nuclear Laser', 'Nuclear Laser', 'Nuclear Laser'], 'line', { ...U, role: 'Striker', shop: false, boss: true, hpMul: 1.2, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('arc_w_machinedramon', 5, [], 'Virus', ['Machinedramon', 'Machinedramon', 'Machinedramon'], ['Infinity Cannon', 'Infinity Cannon', 'Infinity Cannon'], 'aoe', { ...U, role: 'Guardian', shop: false, boss: true, hpMul: 1.5, adMul: 1.3, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('arc_w_megidramon', 5, [], 'Virus', ['Megidramon', 'Megidramon', 'Megidramon'], ['Megiddo Flame', 'Megiddo Flame', 'Megiddo Flame'], 'aoe', { ...U, role: 'Bruiser', shop: false, boss: true, hpMul: 1.6, adMul: 1.4, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    // Summons (Dark Area, Anomalies)
    L('arc_s_belphemon', 5, [], 'Virus', ['Belphemon', 'Belphemon', 'Belphemon'], ['Lampranthus', 'Lampranthus', 'Lampranthus'], 'slam', { ...U, role: 'Bruiser', shop: false, radius: 1, levels: ['Mega', 'Mega', 'Mega'] }),
    L('arc_s_shellnumemon', 2, [], 'Virus', ['ShellNumemon', 'ShellNumemon', 'ShellNumemon'], ['Shell Slam', 'Shell Slam', 'Shell Slam'], 'weaken', { ...U, role: 'Guardian', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('arc_s_garurumon', 2, [], 'Vaccine', ['Garurumon', 'Garurumon', 'Garurumon'], ['Fox Fire', 'Fox Fire', 'Fox Fire'], 'nuke', { ...U, role: 'Striker', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
  ];

  // ---------- wild rounds ----------
  // [lineId, stage, col, row] in the enemy's own coordinates (rows 4-7, 4 = front).
  const pveWave = (stage, round, game) => {
    // standard pace: three stage-1 wild rounds that start smaller, and the x-7 wild rounds come at the end of the stage,
    // so they use the next stage's wave
    if (game && game.pace === 'normal') {
      if (stage === 1) {
        if (round === 2) return [['arc_w_toyagumon', 0, 3, 4]];
        if (round === 3) return [['arc_w_toyagumon', 0, 2, 4], ['arc_w_toyagumon', 0, 4, 4]];
        return [['arc_w_toyagumon', 0, 2, 4], ['arc_w_toyagumon', 0, 4, 4], ['arc_w_otamamon', 0, 3, 6]];
      }
      if (stage >= 4) return pveWave(stage + 1, 4);
      return pveWave(stage, 4).concat([[stage === 2 ? 'arc_w_kuwagamon' : 'arc_w_tyrannomon', 0, 6, 6]]);
    }
    if (stage === 1) {
      if (round === 2) return [['arc_w_toyagumon', 0, 2, 4], ['arc_w_toyagumon', 0, 4, 4]];
      return [['arc_w_toyagumon', 0, 2, 4], ['arc_w_toyagumon', 0, 4, 4], ['arc_w_otamamon', 0, 3, 6]];
    }
    if (stage === 2) return [['arc_w_kuwagamon', 1, 2, 4], ['arc_w_kuwagamon', 1, 4, 4], ['arc_w_gekomon', 0, 3, 6], ['arc_w_otamamon', 1, 1, 6]];
    if (stage === 3) return [['arc_w_tyrannomon', 1, 3, 4], ['arc_w_kuwagamon', 1, 1, 4], ['arc_w_kuwagamon', 1, 5, 4], ['arc_w_gekomon', 1, 2, 6], ['arc_w_shogun', 1, 4, 6]];
    if (stage === 4) return [['arc_w_shogun', 1, 3, 4], ['arc_w_skullgreymon', 1, 2, 4], ['arc_w_tyrannomon', 2, 4, 4], ['arc_w_gekomon', 1, 1, 6], ['arc_w_gekomon', 1, 5, 6], ['arc_w_skullgreymon', 1, 3, 7]];
    if (stage === 5) return [['arc_w_metaltyranno', 1, 3, 4], ['arc_w_skullgreymon', 2, 2, 4], ['arc_w_shogun', 2, 4, 4], ['arc_w_megadramon', 1, 2, 7], ['arc_w_megadramon', 1, 4, 7], ['arc_w_gekomon', 2, 6, 6]];
    if (stage === 6) return [['arc_w_machinedramon', 2, 3, 4], ['arc_w_metaltyranno', 1, 2, 4], ['arc_w_metaltyranno', 1, 4, 4], ['arc_w_megadramon', 2, 1, 7], ['arc_w_megadramon', 1, 5, 7], ['arc_w_skullgreymon', 2, 3, 6], ['arc_w_shogun', 1, 3, 5]];
    return [['arc_w_megidramon', 2, 3, 4], ['arc_w_machinedramon', 1, 2, 5], ['arc_w_metaltyranno', 2, 4, 4], ['arc_w_megadramon', 2, 1, 7], ['arc_w_megadramon', 2, 5, 7], ['arc_w_shogun', 2, 3, 7]];
  };

  // Loot orbs: stage 1 components and a Digimon; later gold, components, completed items, Crests, consumables, and
  // from stage 5 a chance at an artifact (TFT13 dropped Ornn artifacts from late monster rounds).
  const pveReward = (c, won) => {
    const g = c.game;
    const out = [];
    if (c.stage === 1) {
      out.push({ kind: 'blue', items: [g.randomComponent()] });
      if (won) {
        out.push({ kind: 'blue', gold: c.round === 3 ? 2 : 1 });
        if (c.round === 3) {
          const lines = DT.SHOP_LINES.filter((l) => l.tier <= 2 && g.pool[l.id] > 0);
          if (lines.length) out.push({ kind: 'gold', unit: c.pick(lines).id });
        }
      }
      return out;
    }
    out.push({ kind: 'blue', gold: won ? 3 : 2 });
    if (won && c.stage >= 4) out.push({ kind: 'gold', items: [g.randomCompleted()] });
    if (won && c.stage >= 5 && c.rng() < 0.3) out.push({ kind: 'gold', items: [c.pick(ARTIFACT_IDS)] });
    else if (won && c.stage >= 4 && c.rng() < 0.2) out.push({ kind: 'gold', items: [g.randomCrest()] });
    else out.push({ kind: 'blue', items: [g.randomComponent(c.stage >= 3)] });
    if (won && c.stage >= 3 && c.rng() < 0.25) out.push({ kind: 'gold', items: [c.pick(['arc_remover', 'arc_reforger', 'arc_copier_small', 'arc_copier'])] });
    return out;
  };

  // ---------- items ----------
  const components = {
    arc_sword: { name: 'Attack Plugin', short: 'AP', color: '#ff7a45', stats: { ad: 0.15 } },
    arc_bow: { name: 'Speed Plugin', short: 'SP', color: '#ffd23f', stats: { as: 0.12 } },
    arc_rod: { name: 'Spirit Plugin', short: 'SR', color: '#b26bff', stats: { ap: 15 } },
    arc_tear: { name: 'Energy Plugin', short: 'EP', color: '#4fb3ff', stats: { mana: 15 } },
    arc_vest: { name: 'Digizoid Plate', short: 'DP', color: '#c0c6d0', stats: { armor: 20 } },
    arc_cloak: { name: 'Firewall Cloak', short: 'FC', color: '#3fd6b0', stats: { mr: 20 } },
    arc_belt: { name: 'Vital Plugin', short: 'VP', color: '#ff4d64', stats: { hp: 150 } },
    arc_glove: { name: 'Lucky Plugin', short: 'LP', color: '#8fdc4e', stats: { crit: 0.15 } },
    arc_spat: { name: 'Crest Tag', short: 'CT', color: '#ffe27a', stats: {}, special: true,
      desc: 'Combine with a component to make a faction Crest, or with another Crest Tag or a Digivice for +1 team size.' },
    arc_pan: { name: 'Digivice', short: 'DV', color: '#9fdcff', stats: {}, special: true,
      desc: 'Combine with a component to make a class Crest, or with another Digivice or a Crest Tag for +1 team size.' },
  };

  // [a, b, name, desc, passives, extra stats, { id, tags, aiRank }]
  const R = (a, b, id, name, desc, p, extra, tags) => [a, b, name, desc, p, extra || {}, { id, tags }];
  const recipes = [
    R('arc_sword', 'arc_sword', 'arc_transcendent_sword', 'Transcendent Sword', 'Deals 10% more damage.', { amp: 0.1 }, { ad: 0.25 }, ['ad']),
    R('arc_sword', 'arc_bow', 'arc_gram_lance', 'Gram Lance', 'Deals 10% more damage, and 25% more to enemies with over 1600 max HP.', { amp: 0.1, slayer: 0.25 }, {}, ['ad']),
    R('arc_sword', 'arc_rod', 'arc_durandal', 'Durandal', 'Heals for 20% of all damage dealt, and heals the lowest-health ally for 10% of it.', { vamp: 0.2 }, {}, ['ad', 'ap']),
    R('arc_sword', 'arc_tear', 'arc_mana_lance', 'Mana Lance', 'Attacks restore 5 extra mana.', { shojin: 5 }, {}, ['ap', 'ad']),
    R('arc_sword', 'arc_vest', 'arc_shadow_mantle', 'Shadow Mantle', 'The first time HP drops below 60%, become untargetable for 1 second, then gain a 20% max HP shield and 15% attack speed.', { lifeline: { at: 0.6, shield: 0.2, as: 0.15 } }, {}, ['ad']),
    R('arc_sword', 'arc_cloak', 'arc_crimson_fang', 'Crimson Fang', 'Heals for 20% of damage dealt. Below 40% HP, gain a 25% max HP shield once.', { vamp: 0.2, lifeline: { at: 0.4, shield: 0.25 } }, {}, ['ad']),
    R('arc_sword', 'arc_belt', 'arc_berserk_gauntlet', 'Berserk Gauntlet', 'The first time HP drops below 60%, gain a 25% max HP shield and 35% AD.', { lifeline: { at: 0.6, shield: 0.25, ad: 0.35 } }, {}, ['ad', 'tank']),
    R('arc_sword', 'arc_glove', 'arc_ultimate_edge', 'Ultimate Edge', 'Abilities can critically strike. +10% crit damage.', { abilCrit: true, critDmg: 0.1 }, { crit: 0.1 }, ['ad']),
    R('arc_bow', 'arc_bow', 'arc_burning_drive', 'Burning Drive', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, { as: 0.15 }, ['ad']),
    R('arc_bow', 'arc_rod', 'arc_rage_drive', 'Rage Drive', 'Each attack grants 5% attack speed, stacking.', { rage: 0.05 }, {}, ['ad', 'ap']),
    R('arc_bow', 'arc_tear', 'arc_thunder_chip', 'Thunder Chip', 'Every 3rd attack sends lightning to 3 enemies for 80 magic damage.', { volt: 80 }, {}, ['ap', 'ad']),
    R('arc_bow', 'arc_vest', 'arc_titan_armor', 'Titan Armor', 'Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks.', { titan: true }, {}, ['tank', 'ad']),
    R('arc_bow', 'arc_cloak', 'arc_twin_arrows', 'Twin Arrows', 'Attacks also hit a second nearby enemy for 50% damage.', { split: 0.5 }, {}, ['ad']),
    R('arc_bow', 'arc_belt', 'arc_fang_charm', 'Fang Charm', 'After casting, gain 60% attack speed for 5 seconds.', { castAS: 0.6 }, {}, ['ap']),
    R('arc_bow', 'arc_glove', 'arc_armor_piercer', 'Armor Piercer', 'Attacks cut the target\'s armor by 30% for 3 seconds.', { shred: 0.3 }, {}, ['ad']),
    R('arc_rod', 'arc_rod', 'arc_grand_grimoire', 'Grand Grimoire', 'A lot of ability power.', null, { ap: 40 }, ['ap']),
    R('arc_rod', 'arc_tear', 'arc_angel_staff', 'Angel Staff', 'Gain 18 AP every 5 seconds.', { archangel: 18 }, {}, ['ap']),
    R('arc_rod', 'arc_vest', 'arc_crown_guard', 'Crown Guard', 'Start combat with a 25% max HP shield. After 8 seconds, gain 30 AP.', { crown: true }, {}, ['ap', 'tank']),
    R('arc_rod', 'arc_cloak', 'arc_ionic_barrier', 'Ionic Barrier', 'When an enemy within 2 hexes casts, deal 150 magic damage to it.', { zap: 150 }, {}, ['tank']),
    R('arc_rod', 'arc_belt', 'arc_hellfire_tome', 'Hellfire Tome', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, { ap: 10 }, ['ap']),
    R('arc_rod', 'arc_glove', 'arc_crest_gauntlet', 'Crest Gauntlet', 'Abilities can critically strike. +15% crit damage.', { abilCrit: true, critDmg: 0.15 }, { crit: 0.1 }, ['ap']),
    R('arc_tear', 'arc_tear', 'arc_blue_card', 'Blue Card', 'Max mana -10. After casting, keep 20 mana. Deals 5% more damage.', { bluebuff: true, amp: 0.05 }, {}, ['ap']),
    R('arc_tear', 'arc_vest', 'arc_oath_plate', 'Oath Plate', 'Below 40% HP, gain a 25% max HP shield and 20 armor and MR once.', { lifeline: { at: 0.4, shield: 0.25, armor: 20 } }, {}, ['tank']),
    R('arc_tear', 'arc_cloak', 'arc_mode_helm', 'Mode Helm', 'Front two rows: +35 armor and MR. Back two rows: +15 AP and 10 mana every 3 seconds.', null, {}, ['ap', 'tank']),
    R('arc_tear', 'arc_belt', 'arc_healing_ring', 'Healing Ring', 'Every 5 seconds, heals allies within 2 hexes for 80 + 10% of missing HP.', { redemption: true }, {}, ['tank']),
    R('arc_tear', 'arc_glove', 'arc_justice_fist', 'Justice Fist', 'Deals 12% more damage and heals for 12% of damage dealt.', { amp: 0.12, vamp: 0.12 }, {}, ['ad', 'ap']),
    R('arc_vest', 'arc_vest', 'arc_spike_shell', 'Spike Shell', 'Takes 8% less damage. When attacked, deals 60 magic damage to adjacent enemies (1.5s cooldown).', { thorns: 60, dr: 0.08 }, {}, ['tank']),
    R('arc_vest', 'arc_cloak', 'arc_stone_shell', 'Stone Shell', 'Gain 10 armor and MR for each enemy within 2 hexes.', { gargoyle: 10 }, {}, ['tank']),
    R('arc_vest', 'arc_belt', 'arc_flame_mantle', 'Flame Mantle', 'Every 2 seconds, burns the nearest enemy within 2 hexes.', { sunfire: true }, { hp: 80 }, ['tank']),
    R('arc_vest', 'arc_glove', 'arc_iron_will', 'Iron Will', 'Takes 12% less damage.', { dr: 0.12 }, {}, ['tank']),
    R('arc_cloak', 'arc_cloak', 'arc_dragon_hide', 'Dragon Hide', 'Regenerates 1.2% max HP per second.', { regen: 0.012 }, { hp: 100 }, ['tank']),
    R('arc_cloak', 'arc_belt', 'arc_shroud_veil', 'Shroud Veil', 'Enemies within 2 hexes lose 25 armor and MR.', { veil: 25 }, {}, ['tank']),
    R('arc_cloak', 'arc_glove', 'arc_quick_chip', 'Quick Chip', 'Immune to stuns for the first 18 seconds.', { ccImmune: 18 }, { as: 0.1 }, ['ad', 'tank']),
    R('arc_belt', 'arc_belt', 'arc_giga_heart', 'Giga Heart', 'A lot of HP.', null, { hp: 450 }, ['tank']),
    R('arc_belt', 'arc_glove', 'arc_guard_smasher', 'Guard Smasher', 'Deals 25% more damage to shielded enemies.', { guardbreak: 0.25 }, { as: 0.1 }, ['ad']),
    R('arc_glove', 'arc_glove', 'arc_thief_program', 'Thief Program', 'Each combat, gains two random completed items.', { thief: true }, {}, ['ad']),
  ];

  // Crests (emblems). The Crest Tag makes faction Crests, the Digivice class Crests; some have no recipe (drop only).
  const CREST_FROM = {
    arc_spat: { arc_sword: 'CON', arc_vest: 'ENF', arc_rod: 'BR', arc_cloak: 'AUT', arc_belt: 'EXP', arc_tear: 'FAM', arc_bow: 'REB', arc_glove: 'FIRE' },
    arc_pan: { arc_sword: 'ART', arc_glove: 'AMB', arc_belt: 'BRU', arc_cloak: 'PIT', arc_bow: 'QS', arc_vest: 'SEN', arc_rod: 'SOR', arc_tear: 'VIS' },
  };
  const DROP_CRESTS = ['ACA', 'DOM', 'SCR', 'SNI', 'WAT'];
  const crests = [];
  for (const [sp, map] of Object.entries(CREST_FROM)) {
    for (const [c, t] of Object.entries(map)) {
      crests.push({ id: `arc_crest_${t.toLowerCase()}`, name: `${TRAITS[t].name} Crest`, cat: 'emblem', emblem: t, p: { emblem: t }, stats: { ...components[c].stats },
        from: [sp, c], color: TRAITS[t].color, color2: '#fff3b0', short: 'C' + TRAITS[t].name[0], desc: `The holder gains the ${TRAITS[t].name} trait.` });
    }
  }
  for (const t of DROP_CRESTS) {
    crests.push({ id: `arc_crest_${t.toLowerCase()}`, name: `${TRAITS[t].name} Crest`, cat: 'emblem', emblem: t, p: { emblem: t }, stats: { hp: 150 }, drop: true,
      color: TRAITS[t].color, color2: '#fff3b0', short: 'C' + TRAITS[t].name[0],
      desc: `The holder gains the ${TRAITS[t].name} trait.${t === 'SNI' ? ' +1 range.' : ''} Found in the Armory and in loot.` });
  }

  // Tamer items (TFT Tactician's Crown, Shield and Cape)
  const tamerItems = [
    { id: 'arc_tamer_crown', name: "Tamer's Crown", cat: 'special', from: ['arc_spat', 'arc_spat'], p: { teamSize: 1 }, stats: { hp: 150 }, color: '#ffe27a', color2: '#fff3b0', short: 'TC',
      desc: '+1 team size while held on your board. 10% chance to drop 1 gold when you win a combat.' },
    { id: 'arc_tamer_shield', name: "Tamer's Shield", cat: 'special', from: ['arc_pan', 'arc_pan'], p: { teamSize: 1 }, stats: { armor: 20, mr: 20 }, color: '#9fdcff', color2: '#e8f4ff', short: 'TS',
      desc: '+1 team size while held on your board. 10% chance to drop 1 gold when the holder is knocked out.' },
    { id: 'arc_tamer_cape', name: "Tamer's Cape", cat: 'special', from: ['arc_spat', 'arc_pan'], p: { teamSize: 1 }, stats: { as: 0.1 }, color: '#ffe27a', color2: '#9fdcff', short: 'TP',
      desc: '+1 team size while held on your board. 10% chance to drop 1 gold after each combat.' },
  ];

  // Artifacts (TFT Ornn artifacts): Armory from stage 5, wild round loot from stage 5, the Artifact Forge augment.
  const A = (id, name, desc, stats, p, tags, color, color2) => ({ id, name, desc, cat: 'artifact', stats, p: p || {}, tags, color, color2, noDrop: true });
  const artifacts = [
    A('arc_regen_core', 'Regen Core', 'Regenerates 2.5% max HP per second.', { hp: 400, armor: 20, mr: 20 }, { regen: 0.025 }, ['tank'], '#5adf8a', '#1f6a3a'),
    A('arc_doom_seal', 'Doom Seal', 'At the start of combat and every 13 seconds, blasts the current target for 25% of its max HP as magic damage.', { ap: 30, mana: 15 }, null, ['ap'], '#9b3dff', '#2a0a3a'),
    A('arc_gatling_arm', 'Gatling Arm', 'Doubles attack range (up to 8).', { ad: 0.2, as: 0.45 }, null, ['ad'], '#ffb04a', '#5a3a10'),
    A('arc_bounty_chip', 'Bounty Chip', 'Attacks and abilities knock out enemies below 12% HP. Each knockout has a 33% chance to drop 1 gold (up to 3 per combat).', { ad: 0.25, crit: 0.25 }, null, ['ad'], '#ffd23f', '#7a5a10'),
    A('arc_lone_plate', 'Lone Wolf Plate', 'If no ally is adjacent at the start of combat, gain 600 HP, 20% AD and 20 AP.', { armor: 35, mr: 35 }, null, ['tank', 'ad'], '#a0a8b8', '#3a404c'),
    A('arc_infinity_core', 'Infinity Core', 'A bit of everything.', { ad: 0.25, ap: 25, as: 0.2, armor: 25, mr: 25, hp: 250, mana: 15 }, null, ['ad', 'ap', 'tank'], '#ff8fd8', '#4a2a6a'),
    A('arc_spell_blade', 'Spell Blade', 'The first attack after each cast deals bonus magic damage that grows with the stage.', { ap: 30, as: 0.3 }, null, ['ap'], '#b26bff', '#3a1a5a'),
    A('arc_overkill_cannon', 'Overkill Cannon', 'Knockouts deal 100 magic damage plus 20% of the killing blow to the 3 enemies nearest the target.', { ad: 0.4, ap: 40 }, null, ['ad', 'ap'], '#ff5a3a', '#4a1a10'),
    A('arc_long_barrel', 'Long Barrel', '+1 range, and +1 more for each knockout (up to 8).', { as: 0.6 }, null, ['ad'], '#ffe94a', '#5a4a10'),
    A('arc_scope_visor', 'Scope Visor', '+2 range. Deals 8% more damage for each hex between the holder and its target.', { ad: 0.15, ap: 15, as: 0.15 }, null, ['ad', 'ap'], '#5ad1ff', '#103a5a'),
    A('arc_ascension_talisman', 'Ascension Talisman', 'After 20 seconds of combat, gain 80% max HP and deal 80% more damage.', { ad: 0.2, ap: 20, hp: 300 }, null, ['ad', 'ap', 'tank'], '#fff0a0', '#6a5a20'),
    A('arc_time_stopper', 'Time Stopper', 'Once per combat at 40% HP, become untargetable and invulnerable for 2.5 seconds.', { ap: 40, armor: 30, mr: 30 }, null, ['ap', 'tank'], '#7fdcff', '#1a3a5a'),
  ];

  // Contraband (TFT Chem-Baron black market): only works on Syndicate Digimon. Tier I from the first two markets, II later.
  const K = (id, name, desc, tier, tags, color) => ({ id, name, cat: 'contraband', tier, desc: `${desc} Only works on Kowloon Syndicate Digimon.`, stats: {}, p: {}, tags, noDrop: true, color, color2: '#2a0a2a' });
  const contraband = [
    K('arc_k_tank', 'Hazard Tank', '+15 armor and MR, +200 HP. At 50% HP, gain a 25% max HP shield, then a 10% shield every 8 seconds.', 1, ['tank'], '#7ed957'),
    K('arc_k_tank2', 'Hazard Tank II', '+25 armor and MR, +350 HP. At 50% HP, gain a 45% max HP shield, then a 12% shield every 8 seconds.', 2, ['tank'], '#7ed957'),
    K('arc_k_blade', 'Execution Blade', '+25% AD, +10% AS. Attacks and abilities knock out enemies below 10% HP. Knockouts give 10% damage and damage reduction for 5 seconds.', 1, ['ad'], '#ff5d5d'),
    K('arc_k_blade2', 'Execution Blade II', '+40% AD, +15% AS. Attacks and abilities knock out enemies below 15% HP. Knockouts give 15% damage and damage reduction for 5 seconds.', 2, ['ad'], '#ff5d5d'),
    K('arc_k_ripper', 'Feral Ripper', '+15% AD, +20% crit. Below 40% HP once: untargetable for 0.5 seconds, heal 25% max HP, gain 10% AD and 10% omnivamp.', 1, ['ad'], '#ff9e5a'),
    K('arc_k_ripper2', 'Feral Ripper II', '+25% AD, +35% crit. Below 40% HP once: untargetable for 0.5 seconds, heal 40% max HP, gain 20% AD and 15% omnivamp.', 2, ['ad'], '#ff9e5a'),
    K('arc_k_hex', 'Chrome Hexarmor', '+25 armor and MR, +100 HP. Gain 3 armor and MR when hit, up to 15 times. At full stacks, hits deal 50% of armor plus MR as magic damage to adjacent enemies (2s cooldown).', 1, ['tank'], '#d9dfe9'),
    K('arc_k_hex2', 'Chrome Hexarmor II', '+40 armor and MR, +175 HP. Gain 5 armor and MR when hit, up to 15 times. At full stacks, hits deal 75% of armor plus MR as magic damage to adjacent enemies (2s cooldown).', 2, ['tank'], '#d9dfe9'),
    K('arc_k_bloom', 'Shimmer Bloom', '+20 AP, +150 HP. Every 8 seconds, gain a 250 shield and give the same shield to the 2 lowest-health allies.', 1, ['ap', 'tank'], '#e05ad8'),
    K('arc_k_bloom2', 'Shimmer Bloom II', '+35 AP, +250 HP. Every 8 seconds, gain a 420 shield and give the same shield to the 2 lowest-health allies.', 2, ['ap', 'tank'], '#e05ad8'),
    K('arc_k_toxins', 'Toxic Payload', '+15 AP, +10 mana. Attacks restore 5 extra mana. After casting, fire 3 missiles at the target for 100 magic damage each.', 1, ['ap'], '#b6d43a'),
    K('arc_k_toxins2', 'Toxic Payload II', '+25 AP, +15 mana. Attacks restore 5 extra mana. After casting, fire 3 missiles at the target for 175 magic damage each.', 2, ['ap'], '#b6d43a'),
    K('arc_k_virus', 'Virulent Virus', '+25 AP, +15% AS. Ability damage deals 15% more as true damage.', 1, ['ap'], '#9b4dff'),
    K('arc_k_virus2', 'Virulent Virus II', '+40 AP, +25% AS. Ability damage deals 25% more as true damage.', 2, ['ap'], '#9b4dff'),
    K('arc_k_saber', 'Voltaic Saber', '+15% AD, +15 AP, +20% omnivamp. Every 6 hits, jolts the 2 lowest-health enemies for 150 physical damage.', 1, ['ad'], '#ffe94a'),
    K('arc_k_saber2', 'Voltaic Saber II', '+25% AD, +25 AP, +35% omnivamp. Every 6 hits, jolts the 2 lowest-health enemies for 250 physical damage.', 2, ['ad'], '#ffe94a'),
  ];

  const consumables = [
    { id: 'arc_remover', consumable: true, preview: 'remover', name: 'Item Remover', short: 'IR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'arc_reforger', consumable: true, preview: 'reforger', name: 'Item Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', stats: {}, p: {},
      desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
    { id: 'arc_copier', consumable: true, name: 'Data Copier', short: 'DC', color: '#7dffb0', color2: '#2a5a4a', stats: {}, p: {},
      desc: 'Drag onto a Digimon to put a Rookie-form copy of it on your bench.' },
    { id: 'arc_copier_small', consumable: true, name: 'Small Data Copier', short: 'SC', color: '#b8ffd8', color2: '#2a5a4a', stats: {}, p: {},
      desc: 'Drag onto a Digimon of cost 3 or less to put a Rookie-form copy of it on your bench.' },
  ];

  // What the AI builds for each job, best first.
  const aiRoles = {
    ap: ['arc_crest_gauntlet', 'arc_angel_staff', 'arc_grand_grimoire', 'arc_blue_card', 'arc_durandal', 'arc_hellfire_tome', 'arc_mana_lance', 'arc_fang_charm', 'arc_justice_fist', 'arc_mode_helm', 'arc_rage_drive',
      'arc_doom_seal', 'arc_spell_blade', 'arc_infinity_core', 'arc_time_stopper', 'arc_scope_visor', 'arc_overkill_cannon', 'arc_ascension_talisman'],
    ad: ['arc_ultimate_edge', 'arc_gram_lance', 'arc_rage_drive', 'arc_transcendent_sword', 'arc_crimson_fang', 'arc_burning_drive', 'arc_armor_piercer', 'arc_twin_arrows', 'arc_berserk_gauntlet', 'arc_shadow_mantle',
      'arc_thunder_chip', 'arc_guard_smasher', 'arc_quick_chip', 'arc_thief_program', 'arc_bounty_chip', 'arc_gatling_arm', 'arc_long_barrel', 'arc_infinity_core', 'arc_overkill_cannon', 'arc_scope_visor'],
    tank: ['arc_spike_shell', 'arc_dragon_hide', 'arc_stone_shell', 'arc_flame_mantle', 'arc_iron_will', 'arc_oath_plate', 'arc_crown_guard', 'arc_giga_heart', 'arc_shroud_veil', 'arc_healing_ring', 'arc_titan_armor',
      'arc_ionic_barrier', 'arc_regen_core', 'arc_lone_plate', 'arc_time_stopper', 'arc_infinity_core'],
  };

  // Armory: components through stage 3, completed items later; sometimes a consumable or a Crest; from stage 5 one artifact.
  const ARTIFACT_IDS = artifacts.map((a) => a.id);
  const armory = (c) => {
    const g = c.game;
    const out = [];
    for (let guard = 0; out.length < 3 && guard < 60; guard++) {
      const id = c.stage <= 3 ? g.randomComponent(true) : g.randomCompleted();
      if (id && !out.includes(id)) out.push(id);
    }
    if (c.stage >= 3 && c.rng() < 0.2) out[2] = c.pick(['arc_remover', 'arc_reforger', 'arc_copier_small']);
    if (c.stage >= 3 && c.rng() < 0.25) { const e = g.randomCrest(); if (e) out[1] = e; }
    if (c.stage >= 5) out[0] = c.pick(ARTIFACT_IDS);
    return out.filter(Boolean);
  };

  // ---------- augments ----------
  // tier 1 Silver, 2 Gold, 3 Prismatic. Hooks in arcane-mech.js. tags: econ / items / combat (AI).
  const AG = (id, tier, name, desc, tag) => ({ id, tier, name, desc, tags: [tag] });
  const augments = [
    AG('arc_a_coins', 1, 'Spare Data', 'Gain 8 gold.', 'econ'),
    AG('arc_a_cache', 1, 'Component Cache', 'Gain 2 random components.', 'items'),
    AG('arc_a_rolls', 1, 'Rolling Start', 'Gain 1 free reroll every round.', 'econ'),
    AG('arc_a_heart', 1, "Tamer's Heart", 'Gain 5 Tamer HP.', 'econ'),
    AG('arc_a_training', 1, 'Combat Training', 'Your team gains 12% AD and 12 AP.', 'combat'),
    AG('arc_a_wind', 1, 'Second Wind', 'After 10 seconds of combat, your team heals 35% of missing HP.', 'combat'),
    AG('arc_a_feather', 1, 'Featherweights', 'Your 1 and 2-cost Digimon gain 25% attack speed.', 'combat'),
    AG('arc_a_front', 1, 'Frontline Fortitude', 'Digimon in your front row gain 30 armor, 30 MR and 200 HP.', 'combat'),
    AG('arc_a_back', 1, 'Backline Barrage', 'Digimon in your back two rows gain 25% attack speed.', 'combat'),
    AG('arc_a_shimmer', 1, 'Syndicate Ties', 'Gain 60 Shimmer and a Mushroomon. Your Syndicate Digimon gain 150 HP.', 'econ'),
    AG('arc_a_bank', 2, 'Digi-Bank', 'Gain 18 gold.', 'econ'),
    AG('arc_a_trove', 2, 'Treasure Trove', 'Gain a random completed item and a component.', 'items'),
    AG('arc_a_tag', 2, 'Crest Hunter', 'Gain a Crest Tag.', 'items'),
    AG('arc_a_device', 2, 'Spare Digivice', 'Gain a Digivice.', 'items'),
    AG('arc_a_income', 2, 'Steady Income', 'Gain 2 extra gold every round.', 'econ'),
    AG('arc_a_evolve', 2, 'Instant Digivolve', 'Gain a random Champion-form (2-star) Digimon of cost 2 or 3.', 'econ'),
    AG('arc_a_blessing', 2, 'Celestial Blessing', 'Your team heals for 12% of damage dealt.', 'combat'),
    AG('arc_a_lotus', 2, 'Jeweled Lotus', 'Your abilities can critically strike, and your team gains 15% crit.', 'combat'),
    AG('arc_a_overcharge', 2, 'Overcharged', 'Your team starts combat with 25 extra mana.', 'combat'),
    AG('arc_a_find', 2, 'Golden Find', 'Gain 6 free rerolls now. Your Anomaly Digimon drops 1 gold for every 3 knockouts it scores.', 'econ'),
    AG('arc_a_forge', 2, 'Artifact Forge', 'Gain a random artifact.', 'items'),
    AG('arc_a_unity', 2, 'Stand United', 'Your team gains 3% AD and 3 AP per active trait.', 'combat'),
    AG('arc_a_jackpot', 3, 'Jackpot', 'Gain 35 gold.', 'econ'),
    AG('arc_a_armory', 3, 'Big Armory', 'Gain 2 random completed items.', 'items'),
    AG('arc_a_crown', 3, 'Crowned', "Gain a Tamer's Crown (+1 team size).", 'items'),
    AG('arc_a_mega', 3, 'Mega Digivolve', 'Gain a random Champion-form (2-star) Digimon of cost 4.', 'econ'),
    AG('arc_a_giant', 3, 'Giant Grab', 'Your team gains 25% max HP.', 'combat'),
    AG('arc_a_mutant', 3, 'Double Mutation', 'You get a second Anomaly pick, for a different Digimon, the round after your first. Gain 6 gold.', 'econ'),
    AG('arc_a_workshop', 3, 'Master Workshop', 'Gain 2 random artifacts.', 'items'),
    AG('arc_a_market', 3, 'Black Market Pass', 'Gain 120 Shimmer. Every black market offers 4 contraband items.', 'items'),
  ];

  // ---------- rules: Hyper Roll ----------
  const rules = {
    startHp: 20, startGold: 3, planTime: 30, itemCap: 10, rerollCost: 1, winGold: 2, shopSize: 5,
    poolSize: [0, 30, 25, 18, 10, 9],
    shopOdds: [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [75, 25, 0, 0, 0], [55, 30, 15, 0, 0], [45, 33, 20, 2, 0],
      [30, 40, 25, 5, 0], [19, 30, 40, 10, 1], [18, 25, 32, 22, 3], [15, 20, 25, 30, 10]],
    levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
    incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
    lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
    pveLossDamage: (stage) => (stage === 1 ? 1 : null),
    roundsInStage: (stage) => (stage === 1 ? 3 : 4),
    roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'pve' : 'pvp'),
    armoryStages: [2, 3, 4, 5, 6],
    augmentStages: [2, 3, 4],
    // Anomaly: once per game at 4-3, pick 1 of 3 mutations; each reroll costs 1 gold (up to 6)
    offers: [{ kind: 'arcAnomaly', stage: 4, round: 3, rerolls: 6 }],
    copies: 3,
    // standard TFT pace: engine Normal rules (100 HP, XP, interest, streaks, 2-gold rerolls, no Armory); the Anomaly at 4-6 as in TFT13
    normal: { offers: [{ kind: 'arcAnomaly', stage: 4, round: 6, rerolls: 6 }] },
  };

  DT.Sets.register({
    id: 'arcane',
    name: 'Into the Arcane',
    mode: 'tft',
    order: 7,
    desc: 'Kowloon gangs, the Bagra Army, DigiPolice and the Rebels. At 4-3 you mutate one Digimon with an Anomaly; Syndicate Digimon trade Shimmer at the black market. Hyper Roll rules.',
    colors: { main: '#e05ad8', accent: '#4fb3ff' },
    source: 'TFT Set 13 Into the Arcane. docs/inspo/tft/set-13-into-the-arcane.md, docs/sets/arcane.md',
    traits: TRAITS,
    lines: LINES,
    pve: { wave: pveWave, reward: pveReward },
    items: {
      system: 'tft',
      components,
      recipes,
      emblemName: (T) => `${T.name} Crest`,
      list: [...crests, ...tamerItems, ...artifacts, ...contraband, ...consumables],
      aiRoles,
      armory,
    },
    augments,
    rules,
    codexRules: (H, pace) => {
      const normal = pace === 'normal' || (H && H.xp);
      const I = DT.ArcaneInfo || { ANOM: {}, PORTALS: [], JUNK: {}, SHOWN: {} };
      const esc = (s) => String(s).replace(/[&<>"]/g, (ch) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' })[ch]);
      const portal = I.PORTALS.find((x) => x.id === I.SHOWN.portal);
      const spons = (I.SHOWN.sponsor || []).map((id) => (DT.ITEMS[id] ? DT.ITEMS[id].name : id));
      const byTheme = {};
      for (const a of Object.values(I.ANOM)) (byTheme[a.theme] = byTheme[a.theme] || []).push(a);
      const anoms = Object.entries(byTheme).map(([t, list]) => `<p><b>${esc(t)}</b></p><ul>${list.map((a) => `<li><b>${esc(a.name)}</b>: ${esc(a.desc)}</li>`).join('')}</ul>`).join('');
      return [
        { id: 'set', title: 'Into the Arcane', html: (normal ? `<p>${esc('TFT Set 13 Into the Arcane on Digimon, standard pace: 100 Tamer HP, buy XP (4 gold for 4 XP, +2 a round), 5 gold a round plus interest and streak gold, rerolls cost 2. A loss costs stage damage plus damage for each surviving enemy Digimon.')}</p>
          <p>${esc('Rounds: 1-1 carousel, 1-2 to 1-4 wild Digimon. From stage 2: x-1 to x-3 PvP, x-4 carousel, x-5 and x-6 PvP, x-7 wild Digimon (artifacts can drop from stage 5). Augments at 2-1, 3-2 and 4-2, the Anomaly at 4-6.')}</p>` : `<p>${esc('TFT Set 13 Into the Arcane on Digimon. Hyper Roll rules: 20 Tamer HP, everyone levels up at the start of each stage, fixed income (3/5/6/7/8/8/9 by stage), no interest, a PvP win pays 2 gold, rerolls cost 1.')}</p>
          <p>${esc('Rounds: 1-1 carousel, 1-2 and 1-3 wild Digimon. From stage 2: x-1 to x-3 PvP and x-4 wild Digimon. Armory at x-1 of stages 2-6 (an artifact in it from stage 5), augments at 2-2, 3-2 and 4-2, the Anomaly at 4-3. A loss costs 2 HP in stages 1-4, 4 HP in stages 5-7 and 6 HP after.')}</p>`) + `
          <p><b>This game's portal:</b> ${portal ? `${esc(portal.name)}: ${esc(portal.desc)}` : 'none yet'}</p>
          <p><b>Academy sponsored items:</b> ${spons.length ? esc(spons.join(', ')) : 'none yet'}</p>` },
        { id: 'anomaly', title: 'Anomalies', html: `<p>${esc('Once per game (4-3 in Hyper Roll, 4-6 in standard pace), pick 1 of 3 Anomalies. It goes to the Digimon on your center hex (column 4, second row from the front), or your strongest Digimon if that hex is empty. Rerolls cost 1 gold each (up to 6). The Anomaly stays on that Digimon when it digivolves and is lost if you sell it. A floating label shows it at the start of each combat.')}</p>${anoms}` },
        { id: 'market', title: 'Black Market and Workshop', html: `<p>${esc('Kowloon Syndicate: after each player combat with 3 or more Syndicate Digimon you gain Shimmer. Every 100 Shimmer opens the black market at the start of a round: pick one contraband item (tier II from the third market on) or pass to give Syndicate Digimon permanent max HP. Contraband only works on Syndicate Digimon.')}</p>
          <p>${esc('Junk King: every 2 rounds (3 in standard pace) while you own Chaosdromon, pick a mech upgrade: ' + Object.values(I.JUNK).map((j) => j.name).join(', ') + '.')}</p>` },
        { id: 'portals', title: 'Portals', html: `<p>${esc('One portal opens for the whole lobby at the start of each game:')}</p><ul>${I.PORTALS.map((x) => `<li><b>${esc(x.name)}</b>: ${esc(x.desc)}</li>`).join('')}</ul>` },
      ];
    },
    tamers: ['Nokia', 'Arata', 'Yuuko', 'Kyoko', 'Ryuji', 'Mirei', 'Kouki'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
