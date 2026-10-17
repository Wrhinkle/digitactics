// Auto Chess (mode: autochess): Dota Auto Chess as it played in mid-2019 (the 2019-06-26 build: 60 pieces, Wizard
// class just added), with a few standalone Auto Chess ideas folded in (Strange Egg, Wizard rule, relic picks).
// 62 Digimon lines, each with one race and one class (three have two races), 13 races, 13 classes, Dota Auto Chess
// style items (13 drop parts, 33 two-part recipes, the Digi-Egg wildcard), 11 relics, creep rounds and the 2019
// economy (interest, streaks, XP, shop odds, pool sizes, damage per surviving unit).
// Behavior (traits, items, relics, creep loot, count rules, abilities) is in autochess-mech.js; art in autochess-art.js.
// Design notes and the research values chosen: docs/sets/autochess.md. Corpus: docs/inspo/autochess/.
(function (G) {
  const DT = G.DT;
  const L = DT.line;
  const pct = DT.pct;

  // ---------- races and classes ----------
  // kind 'race' or 'class'. Race and class counts are separate: a Fairy Assassin counts once for each.
  const traits = {
    // ---- races ----
    BEAST: { name: 'Beast', kind: 'race', color: '#d9893a', bp: [2, 4, 6], lv: [0.08, 0.16, 0.26], icon: 'BE', crestSymbol: 'star5',
      desc: 'All allies gain attack damage.', fmt: (v) => `+${pct(v)} AD to all allies` },
    DRAGON: { name: 'Dragon', kind: 'race', color: '#ff5a3a', bp: [3], lv: [0.7], icon: 'DR', crestSymbol: 'flame',
      desc: 'Dragons start combat with most of their mana.', fmt: (v) => `Dragons start with ${pct(v)} mana` },
    UNDEAD: { name: 'Undead', kind: 'race', color: '#8a7aa8', bp: [2, 4, 6], lv: [15, 22, 35], icon: 'UN', crestSymbol: 'skull',
      desc: 'All enemies lose armor.', fmt: (v) => `enemies -${v} armor` },
    DEMON: { name: 'Demon', kind: 'race', color: '#c0304a', bp: [1], lv: [0.4], icon: 'DE', crestSymbol: 'moon',
      desc: 'Works only while you field exactly one kind of Demon (two Demon Slayers lift this limit). Demon basic attacks deal extra true damage. An enemy Demon Slayer cancels it.',
      fmt: (v) => `+${pct(v)} of each attack as true damage`, notes: 'Two or more different Demons switch it off unless you also field two Demon Slayers.' },
    FAIRY: { name: 'Fairy', kind: 'race', color: '#7fe08a', bp: [3, 6, 9], lv: [0.28, 0.42, 0.56], icon: 'FA', crestSymbol: 'star6',
      desc: 'Fairies gain a chance to dodge basic attacks.', fmt: (v) => `Fairies +${pct(v)} dodge` },
    HUMAN: { name: 'Human', kind: 'race', color: '#f0d8a8', bp: [2, 4, 6], lv: [0.18, 0.25, 0.32], icon: 'HU', crestSymbol: 'shield',
      desc: 'Basic attacks from Humans can silence the target for 3 seconds.', fmt: (v) => `${pct(v)} chance to silence on attack` },
    MACHINE: { name: 'Machine', kind: 'race', color: '#9aa4b8', bp: [3, 6], lv: [25, 30], regen: [0.01, 0.012], icon: 'MA', crestSymbol: 'chip',
      desc: 'Nanobots: at 3, one random ally gains armor and regeneration. At 6, every ally does.',
      bpDesc: ['one random ally: +25 armor, 1% HP/s', 'all allies: +30 armor, 1.2% HP/s'] },
    DINO: { name: 'Dinosaur', kind: 'race', color: '#b8743a', bp: [2, 4], lv: [220, 260], icon: 'DI', crestSymbol: 'flame',
      desc: 'Dinosaurs gain max HP.', fmt: (v) => `Dinosaurs +${v} HP` },
    INSECT: { name: 'Insect', kind: 'race', color: '#a8c83a', bp: [2, 4], lv: [0.25, 0.25], all: [0, 0.03], icon: 'IN', crestSymbol: 'bug',
      desc: 'Insects gain attack speed. At 4, all allies gain attack speed too.',
      bpDesc: ['Insects +25% AS', 'Insects +25% AS, all allies +3% AS'] },
    ELEMENTAL: { name: 'Elemental', kind: 'race', color: '#ff9a4a', bp: [2, 4], lv: [0.3, 0.4], armor: [15, 15], icon: 'EL', crestSymbol: 'sun',
      desc: 'Elementals gain armor and magic resist. When a melee attacker hits them, they can turn it to stone (stunned 1.5 s). At 4, every ally can.',
      bpDesc: ['Elementals: +15 armor/MR, 30% to stone melee attackers', 'Elementals: +15 armor/MR; all allies: 40% to stone melee attackers'] },
    AQUAN: { name: 'Aquan', kind: 'race', color: '#3aa0ff', bp: [2, 4], lv: [22, 50], icon: 'AQ', crestSymbol: 'pearl',
      desc: 'All allies gain magic resist.', fmt: (v) => `all allies +${v} MR` },
    CYBORG: { name: 'Cyborg', kind: 'race', color: '#7ac8d8', bp: [1], lv: [0.1], icon: 'CY', crestSymbol: 'crosshair',
      desc: 'Cyborgs gain 1 attack range and attack damage.', fmt: (v) => `Cyborgs +1 range, +${pct(v)} AD` },
    OLYMPOS: { name: 'Olympos', kind: 'race', color: '#ffe07a', bp: [1, 2], lv: [2, 2.5], mana: [0, 0], icon: 'OL', crestSymbol: 'sun',
      desc: 'Works only while no other race is active. All allies gain more mana per attack.',
      bpDesc: ['all allies +2 mana per attack', 'all allies +2.5 mana per attack'],
      notes: 'Switched off whenever any other race reaches a breakpoint.' },
    // ---- classes ----
    WARRIOR: { name: 'Warrior', kind: 'class', color: '#c9a66b', bp: [3, 6, 9], lv: [20, 45, 80], icon: 'WA', crestSymbol: 'shield',
      desc: 'Warriors gain armor.', fmt: (v) => `Warriors +${v} armor` },
    MAGE: { name: 'Mage', kind: 'class', color: '#7a8cff', bp: [3, 6], lv: [0.14, 0.36], icon: 'MG', crestSymbol: 'star6',
      desc: 'Enemies take more magic damage from your team.', fmt: (v) => `+${pct(v)} magic damage` },
    HUNTER: { name: 'Hunter', kind: 'class', color: '#5fbf4a', bp: [3, 6], lv: [0.22, 0.5], icon: 'HN', crestSymbol: 'crosshair',
      desc: 'Hunters gain attack damage.', fmt: (v) => `Hunters +${pct(v)} AD` },
    ASSASSIN: { name: 'Assassin', kind: 'class', color: '#9a5aff', bp: [3, 6], lv: [0.07, 0.13], cd: [1.9, 2.3], icon: 'AS', crestSymbol: 'dagger',
      desc: 'Assassins leap to the back line and gain critical strikes.',
      bpDesc: ['Assassins: 7% crit for 190% damage', 'Assassins: 13% crit for 230% damage'] },
    KNIGHT: { name: 'Knight', kind: 'class', color: '#e0e6f0', bp: [2, 4, 6], lv: [1, 2, 3], icon: 'KN', crestSymbol: 'shield',
      desc: 'When hit, Knights can raise a divine shield: 30% chance per level, each one cuts damage taken by 18% for 3 seconds. Shields stack.',
      bpDesc: ['1 roll: 30% for an 18% damage shield', '2 rolls (shields stack)', '3 rolls (shields stack)'] },
    DRUID: { name: 'Druid', kind: 'class', color: '#3fbf7a', bp: [2, 4], lv: [0.03, 0.05], icon: 'DU', crestSymbol: 'star5',
      desc: 'Druids digivolve from 2 copies instead of 3. A Champion Druid fights at full strength only with 2 Druids, a final-form Druid only with 4; otherwise it fights as the lower form. Every 4 seconds each Druid heals the lowest-health ally.',
      fmt: (v) => `heal lowest ally ${pct(v)} max HP every 4s`, notes: 'The 2-copy digivolve itself always works, on the bench too; the trait decides how strong the result fights.' },
    MECH: { name: 'Mech', kind: 'class', color: '#ffb04a', bp: [2, 4], lv: [0.015, 0.035], icon: 'ME', crestSymbol: 'chip',
      desc: 'Mechs repair themselves during combat.', fmt: (v) => `Mechs regen ${pct(v)} HP/s` },
    SHAMAN: { name: 'Shaman', kind: 'class', color: '#3ad8c0', bp: [2], lv: [4], icon: 'SH', crestSymbol: 'snow',
      desc: 'At the start of combat, a random enemy is hexed (cannot act) for a few seconds.', fmt: (v) => `hex a random enemy for ${v}s` },
    WARLOCK: { name: 'Warlock', kind: 'class', color: '#b04ad8', bp: [3, 6], lv: [0.04, 0.09], icon: 'WL', crestSymbol: 'moon',
      desc: 'All allies heal for part of the damage they deal.', fmt: (v) => `all allies +${pct(v)} lifesteal` },
    DH: { name: 'Demon Slayer', kind: 'class', color: '#5a3a8a', bp: [1, 2], lv: [0.25, 0.4], as: [0.15, 0.25], icon: 'DS', crestSymbol: 'dagger',
      desc: 'Demon Slayers gain attack speed and deal more damage to Demons and cancel enemy Demon power. With 2, your Demons keep their power however many kinds you field.',
      bpDesc: ['cancel enemy Demons; Demon Slayers +15% AS, +25% damage to Demons', 'your Demon trait works with any number of Demons; Demon Slayers +25% AS, +40% damage to Demons'] },
    PRIEST: { name: 'Priest', kind: 'class', color: '#fff4c0', bp: [1, 2], lv: [0.2, 0.35], heal: [0.1, 0.25], icon: 'PR', crestSymbol: 'cross',
      desc: 'Your Tamer takes less damage when you lose a round, and your team heals more.',
      bpDesc: ['-20% Tamer damage, +10% healing', '-35% Tamer damage, +25% healing'] },
    WIZARD: { name: 'Wizard', kind: 'class', color: '#d0a0ff', bp: [2], lv: [35], icon: 'WZ', crestSymbol: 'star6',
      desc: 'Rule change: every other race and class that needs 4 or more units needs one fewer (for example Fairy 6 needs 5). Wizards also gain ability power.',
      fmt: (v) => `breakpoints of 4+ need 1 fewer; Wizards +${v} AP` },
  };

  // the trait panel and tooltips call fmt(v, i) on every trait: traits described by bpDesc get one that returns it
  for (const T of Object.values(traits)) if (!T.fmt) T.fmt = (v, i) => (T.bpDesc && T.bpDesc[i]) || String(v);

  // ---------- lines ----------
  // L(id, cost, traits, attr, forms, abilities, type, options). src: the Dota Auto Chess piece it adapts.
  const U = (o) => ({ unassigned: true, ...o });
  const R3M = ['Rookie', 'Champion', 'Mega'];
  const lines = [
    // ---- 1 cost ----
    L('ac_lopmon', 1, ['FAIRY', 'DH'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Ninja Fist', 'Bunny Blades'], 'execute', U({ role: 'Duelist', src: 'DAC Anti-Mage' })),
    L('ac_agumon', 1, ['DINO', 'WARRIOR'], 'Vaccine', ['Agumon', 'Greymon', 'MetalGreymon'], ['Pepper Breath', 'Mega Flame', 'Giga Destroyer'], 'shield', U({ role: 'Guardian', hpMul: 0.88, src: 'DAC Axe' })),
    L('ac_fanbeemon', 1, ['INSECT', 'KNIGHT'], 'Virus', ['FanBeemon', 'Waspmon', 'CannonBeemon'], ['Gear Stinger', 'Turbo Stinger', 'Nitro Stinger'], 'aoe', U({ role: 'Sniper', range: 3, src: 'DAC Batrider' })),
    L('ac_keramon', 1, ['MACHINE', 'ASSASSIN'], 'Virus', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', "Hell's Grenade"], 'nuke', U({ role: 'Assassin', src: 'DAC Bounty Hunter' })),
    L('ac_hagurumon', 1, ['MACHINE', 'MECH'], 'Vaccine', ['Hagurumon', 'Guardromon', 'Andromon'], ['Cog Crusher', 'Destruction Grenade', 'Gatling Missile'], 'slam', U({ role: 'Bruiser', hpMul: 0.92, src: 'DAC Clockwerk' })),
    L('ac_dracmon', 1, ['UNDEAD', 'HUNTER'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Eye of Nightmares', 'Sticker Blade', 'Thousand Arrows'], 'line', U({ role: 'Sniper', adMul: 1.1, src: 'DAC Drow Ranger' })),
    L('ac_kudamon', 1, ['BEAST', 'DRUID'], 'Vaccine', ['Kudamon', 'Reppamon', 'Chirinmon'], ['Bullet Whirlwind', 'Lion Claw', 'Knowledge of Swiftness'], 'heal', U({ role: 'Support', combine: 2, hpMul: 0.9, mana: 90, src: 'DAC Enchantress' })),
    L('ac_coronamon', 1, ['OLYMPOS', 'WARRIOR'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Corona Flame', 'Flame Dive', 'Phoebus Blow'], 'slam', U({ role: 'Guardian', levels: R3M, src: 'DAC Mars' })),
    L('ac_tentomon', 1, ['INSECT', 'SHAMAN'], 'Vaccine', ['Tentomon', 'Kabuterimon', 'MegaKabuterimon'], ['Super Shocker', 'Electro Shocker', 'Horn Buster'], 'stun', U({ role: 'Mystic', src: 'DAC Shadow Shaman' })),
    L('ac_kokuwamon', 1, ['MACHINE', 'MECH'], 'Data', ['Kokuwamon', 'Tankmon', 'MetalMamemon'], ['Electro Strike', 'Hyper Cannon', 'Energy Bomb'], 'barrage', U({ role: 'Sniper', src: 'DAC Tinker' })),
    L('ac_gotsumon', 1, ['ELEMENTAL', 'WARRIOR'], 'Data', ['Gotsumon', 'Golemon', 'Meteormon'], ['Rock Fist', 'Golem Punch', 'Big Bang Blow'], 'stun', U({ role: 'Bruiser', hpMul: 1.15, adMul: 1.1, src: 'DAC Tiny' })),
    L('ac_bearmon', 1, ['BEAST', 'WARRIOR'], 'Vaccine', ['Bearmon', 'Grizzlymon', 'GrapLeomon'], ['Bear Fist', 'Maul Attack', 'The King of Fists'], 'execute', U({ role: 'Striker', hpMul: 1.1, adMul: 1.1, src: 'DAC Tusk' })),
    L('ac_dracomon', 1, ['DRAGON', 'MAGE'], 'Vaccine', ['Dracomon', 'Coredramon (Blue)', 'Wingdramon'], ['Baby Breath', 'Blue Flare Breath', 'Blaze Sonic Breath'], 'nuke', U({ role: 'Mystic', src: 'DAC Winter Wyvern' })),
    L('ac_candlemon', 1, ['DEMON', 'WIZARD'], 'Virus', ['Candlemon', 'Wizardmon', 'Wisemon'], ['Bonfire', 'Thunder Cloud', 'Dialogue'], 'line', U({ role: 'Mystic', hpMul: 1.1, mana: 50, src: 'DAC Lion (2019-07)' })),
    // ---- 2 cost ----
    L('ac_hackmon', 2, ['DINO', 'HUNTER'], 'Data', ['Hackmon', 'BaoHuckmon', 'SaviorHuckmon'], ['Fifth Rush', 'Fif Cross', 'Meteor Flame'], 'line', U({ role: 'Striker', src: 'DAC Beastmaster' })),
    L('ac_goblimon', 2, ['DEMON', 'KNIGHT'], 'Virus', ['Goblimon', 'Musyamon', 'Zanbamon'], ['Goblin Strike', 'Kirisute Gomen', 'Vorpal Blade'], 'stun', U({ role: 'Bruiser', hpMul: 1.1, levels: R3M, src: 'DAC Chaos Knight' })),
    L('ac_salamon', 2, ['HUMAN', 'MAGE'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'aoe', U({ role: 'Mystic', src: 'DAC Crystal Maiden' })),
    L('ac_palmon', 2, ['FAIRY', 'DRUID'], 'Data', ['Palmon', 'Togemon', 'Lillymon'], ['Poison Ivy', 'Needle Spray', 'Flower Cannon'], 'summon', U({ role: 'Mystic', combine: 2, hpMul: 0.9, summon: 'ac_s_tanemon', summonN: [1, 1, 2], src: "DAC Nature's Prophet" })),
    L('ac_blackagumon', 2, ['DINO', 'WARRIOR'], 'Virus', ['BlackAgumon', 'BlackGreymon', 'BlackWarGreymon'], ['Spitfire', 'Great Antler', 'Dark Terra Force'], 'slam', U({ role: 'Duelist', levels: R3M, src: 'DAC Juggernaut' })),
    L('ac_tsukaimon', 2, ['UNDEAD', 'MAGE'], 'Virus', ['Tsukaimon', 'Bakemon', 'Phantomon'], ['Bad Message', "Hell's Hand", 'Soul Chopper'], 'rally', U({ role: 'Support', src: 'DAC Lich (Frost Shield)' })),
    L('ac_lalamon', 2, ['FAIRY', 'KNIGHT'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Nuts Gun', 'Sunshine Beam', 'Lila Shower'], 'chain', U({ role: 'Sniper', range: 3, hpMul: 1.05, src: 'DAC Luna' })),
    L('ac_hyokomon', 2, ['ELEMENTAL', 'ASSASSIN'], 'Data', ['Hyokomon', 'Meramon', 'SkullMeramon'], ['Downward Cleave', 'Burning Fist', 'Heavy Metal Fire'], 'dash', U({ role: 'Assassin', hpMul: 1.1, adMul: 1.1, src: 'DAC Morphling' })),
    L('ac_gumdramon', 2, ['DRAGON', 'FAIRY', 'MAGE'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Slayerdramon'], ['Fire Vortex', 'Spiral Shredder', 'Tenryu Slash'], 'line', U({ role: 'Mystic', levels: R3M, src: 'DAC Puck' })),
    L('ac_floramon', 2, ['DEMON', 'ASSASSIN'], 'Virus', ['Floramon', 'BlackGatomon', 'LadyDevimon'], ['Rain of Pollen', "Cat's Eye", 'Darkness Wave'], 'aoe', U({ role: 'Assassin', hpMul: 1.05, src: 'DAC Queen of Pain' })),
    L('ac_gomamon', 2, ['AQUAN', 'WARRIOR'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Zudomon'], ['Marching Fishes', 'Harpoon Torpedo', "Vulcan's Hammer"], 'slam', U({ role: 'Bruiser', src: 'DAC Slardar' })),
    L('ac_solarmon', 2, ['MACHINE', 'MECH'], 'Virus', ['Solarmon', 'Clockmon', 'Datamon'], ['Shiny Ring', 'Chrono Breaker', 'Digital Bomb'], 'drain', U({ role: 'Bruiser', radius: 1, hpMul: 0.88, src: 'DAC Timbersaw' })),
    L('ac_kunemon', 2, ['INSECT', 'WARLOCK'], 'Virus', ['Kunemon', 'Flymon', 'Arukenimon'], ['Electric Thread', 'Poison Powder', 'Spider Thread'], 'chain', U({ role: 'Mystic', src: 'DAC Witch Doctor' })),
    L('ac_labramon', 2, ['OLYMPOS', 'WIZARD'], 'Virus', ['Labramon', 'Dobermon', 'Merukimon'], ['Retrieve Bark', 'Grau Lärm', 'Mirage Image'], 'weaken', U({ role: 'Mystic', hpMul: 1.1, levels: R3M, src: 'DAC Rubick' })),
    L('ac_lunamon', 2, ['OLYMPOS', 'PRIEST'], 'Data', ['Lunamon', 'Lekismon', 'Dianamon'], ['Tearing Shot', 'Tear Arrow', 'Arrow of Artemis'], 'heal', U({ role: 'Support', mana: 85, levels: R3M, src: 'DAC Oracle (2019-08)' })),
    // ---- 3 cost ----
    L('ac_gizamon', 3, ['UNDEAD', 'KNIGHT'], 'Virus', ['Gizamon', 'Raremon', 'SkullGreymon'], ['Spiral Edge', 'Rotten Tackle', 'Dark Shot'], 'shield', U({ role: 'Guardian', hpMul: 0.92, src: 'DAC Abaddon' })),
    L('ac_wormmon', 3, ['INSECT', 'PRIEST'], 'Vaccine', ['Wormmon', 'Stingmon', 'JewelBeemon'], ['Sticky Net', 'Spiking Strike', 'Spike Buster'], 'heal', U({ role: 'Support', mana: 90, src: 'DAC Dazzle' })),
    L('ac_biyomon', 3, ['HUMAN', 'MAGE'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'nuke', U({ role: 'Mystic', src: 'DAC Lina' })),
    L('ac_gabumon', 3, ['HUMAN', 'BEAST', 'WARRIOR'], 'Vaccine', ['Gabumon', 'Garurumon', 'WereGarurumon'], ['Blue Blaster', 'Fox Fire', 'Wolf Claw'], 'transform', U({ role: 'Striker', hpMul: 1.05, tf: { ad: 0.35, as: 0.25 }, src: 'DAC Lycan' })),
    L('ac_patamon', 3, ['HUMAN', 'KNIGHT'], 'Vaccine', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Air Shot', "Heaven's Knuckle", "Heaven's Gate"], 'heal', U({ role: 'Guardian', mana: 110, src: 'DAC Omniknight' })),
    L('ac_falcomon', 3, ['FAIRY', 'ASSASSIN'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Scratch Smash', 'Thousand Beaks', 'Savage Emperor'], 'execute', U({ role: 'Assassin', adMul: 0.95, src: 'DAC Phantom Assassin' })),
    L('ac_snowgoblimon', 3, ['ELEMENTAL', 'MAGE'], 'Vaccine', ['SnowGoblimon', 'Hyogamon', 'IceLeomon'], ['Snow Goblin Volt', 'Throwing Ice', 'Blizzard Breath'], 'aoe', U({ role: 'Mystic', src: 'DAC Razor' })),
    L('ac_gazimon', 3, ['DEMON', 'WARLOCK'], 'Virus', ['Gazimon', 'Devimon', 'SkullSatamon'], ['Electric Stun Blast', 'Death Claw', 'Skull Hammer'], 'aoe', U({ role: 'Mystic', radius: 2, src: 'DAC Shadow Fiend' })),
    L('ac_commandramon', 3, ['CYBORG', 'HUNTER'], 'Virus', ['Commandramon', 'Sealsdramon', 'Darkdramon'], ['M16 Assassin', 'Death Behind', 'Gigastick Lance'], 'execute', U({ role: 'Sniper', range: 5, hpMul: 1.15, adMul: 1.05, levels: R3M, src: 'DAC Sniper' })),
    L('ac_monodramon', 3, ['DRAGON', 'ASSASSIN'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Crack Bite', 'Fang Strike', 'Desolation Claw'], 'weaken', U({ role: 'Assassin', adMul: 0.92, src: 'DAC Viper' })),
    L('ac_hawkmon', 3, ['FAIRY', 'HUNTER'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Top Gun'], 'line', U({ role: 'Sniper', adMul: 1.05, src: 'DAC Windranger' })),
    L('ac_mushroomon', 3, ['FAIRY', 'DRUID'], 'Virus', ['Mushroomon', 'Woodmon', 'Cherrymon'], ['Fungus Crusher', 'Branch Drain', 'Cherry Bomb'], 'drain', U({ role: 'Guardian', combine: 2, hpMul: 0.95, src: 'DAC Treant Protector' })),
    L('ac_guilmon', 3, ['DEMON', 'DH'], 'Virus', ['Guilmon', 'Growlmon', 'WarGrowlmon'], ['Fire Ball', 'Exhaust Flame', 'Double Edge'], 'transform', U({ role: 'Duelist', tf: { ad: 0.3, range: 2 }, src: 'DAC Terrorblade' })),
    L('ac_dorumon', 3, ['BEAST', 'WARLOCK'], 'Data', ['Dorumon', 'Dorugamon', 'DoruGreymon'], ['Dash Metal', 'Power Metal', 'Metal Meteor'], 'summon', U({ role: 'Mystic', summon: 'ac_s_vegiemon', summonN: [1, 1, 2], src: 'DAC Venomancer' })),
    // ---- 4 cost ----
    L('ac_tapirmon', 4, ['MACHINE', 'WARLOCK'], 'Virus', ['Tapirmon', 'Mekanorimon', 'Megadramon'], ['Nightmare Syndrome', 'Twinkle Beam', 'Ultimate Slicer'], 'transform', U({ role: 'Bruiser', tf: { ad: 0.25, as: 0.4 }, src: 'DAC Alchemist' })),
    L('ac_armadillomon', 4, ['DINO', 'SHAMAN'], 'Data', ['Armadillomon', 'Ankylomon', 'Triceramon'], ['Diamond Shell', 'Tail Hammer', 'Tri-Horn Attack'], 'aoe', U({ role: 'Mystic', radius: 2, src: 'DAC Disruptor' })),
    L('ac_impmon', 4, ['DEMON', 'WARRIOR'], 'Virus', ['Impmon', 'Beelzemon', 'Beelzemon Blast Mode'], ['Bada Boom', 'Darkness Claw', 'Death Slinger'], 'execute', U({ role: 'Bruiser', hpMul: 1.1, adMul: 1.25, levels: ['Rookie', 'Mega', 'Mega'], src: 'DAC Doom' })),
    L('ac_veemon', 4, ['HUMAN', 'DRAGON', 'KNIGHT'], 'Free', ['Veemon', 'ExVeemon', 'Paildramon'], ['Boom Boom Punch', 'Vee Laser', 'Death Parade Blaster'], 'transform', U({ role: 'Guardian', tf: { ad: 0.3, armor: 25, range: 2 }, src: 'DAC Dragon Knight' })),
    L('ac_shamanmon', 4, ['HUMAN', 'MAGE'], 'Vaccine', ['Shamanmon', 'Sorcermon', 'Asuramon'], ['Mad Twister', 'Crystal Barrage', 'Fire Fist of Shiva'], 'line', U({ role: 'Mystic', mana: 50, src: 'DAC Keeper of the Light' })),
    L('ac_kotemon', 4, ['HUMAN', 'WARRIOR'], 'Data', ['Kotemon', 'Gladimon', 'Knightmon'], ['Meteor Slash', 'Wheel Rush', 'Grand Smash'], 'slam', U({ role: 'Guardian', radius: 2, src: 'DAC Kunkka' })),
    L('ac_gaomon', 4, ['BEAST', 'DRUID'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Gaoga Hound', 'Winning Knuckle'], 'summon', U({ role: 'Bruiser', combine: 2, hpMul: 0.9, summon: 'ac_s_kumamon', summonN: [1, 1, 1], src: 'DAC Lone Druid' })),
    L('ac_betamon', 4, ['AQUAN', 'HUNTER'], 'Data', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Blast', 'Lightning Javelin'], 'barrage', U({ role: 'Sniper', hpMul: 1.15, src: 'DAC Medusa' })),
    L('ac_demidevimon', 4, ['UNDEAD', 'WARLOCK'], 'Virus', ['DemiDevimon', 'Soulmon', 'Myotismon'], ['Demi Darts', 'Necro Magic', 'Crimson Lightning'], 'drain', U({ role: 'Mystic', radius: 1, mana: 75, src: 'DAC Necrophos' })),
    L('ac_renamon', 4, ['FAIRY', 'ASSASSIN'], 'Data', ['Renamon', 'Kyubimon', 'Taomon'], ['Diamond Storm', 'Fox Tail Inferno', 'Talisman of Light'], 'dash', U({ role: 'Assassin', hpMul: 1.05, adMul: 1.05, src: 'DAC Templar Assassin' })),
    L('ac_dokunemon', 4, ['INSECT', 'WARRIOR'], 'Virus', ['DoKunemon', 'Kuwagamon', 'GranKuwagamon'], ['Worm Venom', 'Scissor Claw', 'Dimension Scissor'], 'transform', U({ role: 'Duelist', tf: { ad: 0.15, as: 0.35 }, levels: R3M, src: 'DAC Troll Warlord' })),
    L('ac_psychemon', 4, ['DEMON', 'WIZARD'], 'Virus', ['Psychemon', 'Witchmon', 'Astamon'], ['Colored Spark', 'Baluluna Gale', 'Maverick'], 'line', U({ role: 'Mystic', hpMul: 1.2, mana: 50, src: 'DAC Grimstroke' })),
    // ---- 5 cost ----
    L('ac_dexdorugamon', 5, ['UNDEAD', 'WARLOCK'], 'Virus', ['DexDorugamon', 'DexDorugreymon', 'DexDorugoramon'], ['Cannonball', 'Bloody Cave', 'Metal Impulse'], 'barrage', U({ role: 'Mystic', hpMul: 1.4, mana: 50, levels: ['Champion', 'Ultimate', 'Mega'], src: 'DAC Death Prophet' })),
    L('ac_vorvomon', 5, ['ELEMENTAL', 'WARLOCK'], 'Virus', ['Vorvomon', 'Lavorvomon', 'Volcanicdramon'], ['Great Flame', 'Great Flame', 'Volcanic Flare'], 'slam', U({ role: 'Bruiser', radius: 2, levels: R3M, src: 'DAC Enigma' })),
    L('ac_jazamon', 5, ['CYBORG', 'MECH'], 'Data', ['Jazamon', 'Jazardmon', 'Jazarichmon'], ['Laser Eye', 'Laser Eye', 'Messer Spiral'], 'barrage', U({ role: 'Sniper', hpMul: 1.25, adMul: 1.2, src: 'DAC Gyrocopter' })),
    L('ac_terriermon', 5, ['MACHINE', 'MECH'], 'Vaccine', ['Terriermon', 'Gargomon', 'MegaGargomon'], ['Terrier Tornado', 'Gargo Laser', 'Giant Missile'], 'aoe', U({ role: 'Sniper', radius: 2, levels: R3M, src: 'DAC Techies' })),
    L('ac_lucemon', 5, ['DEMON', 'WARRIOR'], 'Virus', ['Lucemon', 'Lucemon Chaos Mode', 'Lucemon Shadowlord Mode'], ['Grand Cross', 'Paradise Lost', 'Purgatorial Flame'], 'transform', U({ role: 'Striker', hpMul: 1.1, tf: { ad: 0.45, as: 0.15 }, levels: ['Rookie', 'Ultimate', 'Mega'], src: 'DAC Sven' })),
    L('ac_syakomon', 5, ['AQUAN', 'HUNTER'], 'Data', ['Syakomon', 'Whamon', 'Plesiomon'], ['Black Pearl Blast', 'Tidal Wave', 'Sorrow Blue'], 'slam', U({ role: 'Guardian', radius: 2, levels: R3M, src: 'DAC Tidehunter' })),
    L('ac_elecmon', 5, ['OLYMPOS', 'MAGE'], 'Vaccine', ['Elecmon', 'Aegiomon', 'Jupitermon'], ['Sparkling Thunder', 'Stun Beat Blow', 'Mabolt'], 'storm', U({ role: 'Mystic', hpMul: 1.2, levels: R3M, src: 'DAC Zeus' })),

    // ---- summons (not in the shop) ----
    L('ac_s_tanemon', 1, [], 'Data', ['Tanemon', 'Tanemon', 'Tanemon'], ['Bubble', 'Bubble', 'Bubble'], 'nuke', U({ role: 'Striker', shop: false, hpMul: 0.5, adMul: 0.6, levels: ['Summon', 'Summon', 'Summon'] })),
    L('ac_s_vegiemon', 2, [], 'Virus', ['Vegiemon', 'Vegiemon', 'Vegiemon'], ['Sweet Breath', 'Sweet Breath', 'Sweet Breath'], 'nuke', U({ role: 'Sniper', shop: false, range: 3, hpMul: 0.8, levels: ['Summon', 'Summon', 'Summon'] })),
    L('ac_s_kumamon', 3, [], 'Vaccine', ['Kumamon', 'Kumamon', 'Kumamon'], ['Blizzard Blaster', 'Blizzard Blaster', 'Blizzard Blaster'], 'stun', U({ role: 'Bruiser', shop: false, hpMul: 0.65, adMul: 0.75, levels: ['Summon', 'Summon', 'Summon'] })),

    // ---- creeps (wild rounds) ----
    L('ac_w_numemon', 1, [], 'Virus', ['Numemon', 'Numemon', 'Numemon'], ['Nume-Sludge', 'Nume-Sludge', 'Nume-Sludge'], 'nuke', U({ role: 'Striker', shop: false, hpMul: 0.55, adMul: 0.5, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_gekomon', 1, [], 'Virus', ['Gekomon', 'Gekomon', 'Gekomon'], ['Symphony Crusher', 'Symphony Crusher', 'Symphony Crusher'], 'nuke', U({ role: 'Sniper', shop: false, hpMul: 0.55, adMul: 0.5, range: 3, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_blackgarurmon', 2, [], 'Virus', ['BlackGarurmon', 'BlackGarurmon', 'BlackGarurmon'], ['Howling Blaster', 'Howling Blaster', 'Howling Blaster'], 'dash', U({ role: 'Duelist', shop: false, hpMul: 0.8, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_ogremon', 3, [], 'Virus', ['Ogremon', 'Ogremon', 'Ogremon'], ['Pummel Whack', 'Pummel Whack', 'Pummel Whack'], 'stun', U({ role: 'Bruiser', shop: false, boss: true, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_fugamon', 2, [], 'Virus', ['Fugamon', 'Fugamon', 'Fugamon'], ['Evil Hurricane', 'Evil Hurricane', 'Evil Hurricane'], 'aoe', U({ role: 'Striker', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_tyrannomon', 3, [], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Blaze Blast', 'Blaze Blast', 'Blaze Blast'], 'nuke', U({ role: 'Guardian', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_metaltyrannomon', 4, [], 'Virus', ['MetalTyrannomon', 'MetalTyrannomon', 'MetalTyrannomon'], ['Nuclear Laser', 'Nuclear Laser', 'Nuclear Laser'], 'line', U({ role: 'Bruiser', shop: false, boss: true, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_monochromon', 3, [], 'Data', ['Monochromon', 'Monochromon', 'Monochromon'], ['Volcanic Strike', 'Volcanic Strike', 'Volcanic Strike'], 'slam', U({ role: 'Guardian', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_mammothmon', 4, [], 'Vaccine', ['Mammothmon', 'Mammothmon', 'Mammothmon'], ['Tusk Crusher', 'Tusk Crusher', 'Tusk Crusher'], 'slam', U({ role: 'Bruiser', shop: false, boss: true, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_airdramon', 3, [], 'Vaccine', ['Airdramon', 'Airdramon', 'Airdramon'], ['Spinning Needle', 'Spinning Needle', 'Spinning Needle'], 'aoe', U({ role: 'Sniper', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_gigadramon', 4, [], 'Virus', ['Gigadramon', 'Gigadramon', 'Gigadramon'], ['Genocide Gear', 'Genocide Gear', 'Genocide Gear'], 'barrage', U({ role: 'Sniper', shop: false, boss: true, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_diaboromon', 5, [], 'Virus', ['Diaboromon', 'Diaboromon', 'Diaboromon'], ['Catastrophe Cannon', 'Catastrophe Cannon', 'Catastrophe Cannon'], 'line', U({ role: 'Bruiser', shop: false, boss: true, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_goldguardromon', 3, [], 'Free', ['Guardromon (Gold)', 'Guardromon (Gold)', 'Guardromon (Gold)'], ['Destruction Grenade', 'Destruction Grenade', 'Destruction Grenade'], 'nuke', U({ role: 'Guardian', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_machinedramon', 5, [], 'Virus', ['Machinedramon', 'Machinedramon', 'Machinedramon'], ['Infinity Cannon', 'Infinity Cannon', 'Infinity Cannon'], 'line', U({ role: 'Guardian', shop: false, boss: true, hpMul: 1.4, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_kimeramon', 4, [], 'Data', ['Kimeramon', 'Kimeramon', 'Kimeramon'], ['Heat Viper', 'Heat Viper', 'Heat Viper'], 'aoe', U({ role: 'Bruiser', shop: false, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_millenniummon', 5, [], 'Virus', ['Millenniummon', 'Millenniummon', 'Millenniummon'], ['Time Unlimited', 'Time Unlimited', 'Time Unlimited'], 'storm', U({ role: 'Bruiser', shop: false, boss: true, hpMul: 1.3, levels: ['Wild', 'Wild', 'Wild'] })),
    L('ac_w_apocalymon', 5, [], 'Virus', ['Apocalymon', 'Apocalymon', 'Apocalymon'], ['Total Annihilation', 'Total Annihilation', 'Total Annihilation'], 'storm', U({ role: 'Guardian', shop: false, boss: true, hpMul: 1.6, levels: ['Wild', 'Wild', 'Wild'] })),
  ];

  // ---------- creep rounds ----------
  // Dota Auto Chess creep rounds: 1, 2, 3, 10, 15, 20, 25, 30, 35, 40, 45, 50 (here x-5 from stage 2, 5 rounds a stage).
  const roundNo = (stage, round) => (stage - 1) * 5 + round;
  const pve = {
    wave(stage, round, game) {
      // Hyper Roll ends each later stage at x-4, with the same creep progression as Normal's x-5.
      // Without this mapping, every Hyper wild round after 1-3 fell through to the final boss wave.
      const n = roundNo(stage, game?.pace === 'hyper' && stage > 1 ? 5 : round);
      if (n === 1) return [['ac_w_numemon', 0, 2, 4], ['ac_w_numemon', 0, 4, 4]];
      if (n === 2) return [['ac_w_numemon', 0, 2, 4], ['ac_w_numemon', 0, 4, 4], ['ac_w_gekomon', 0, 3, 6]];
      if (n === 3) return [['ac_w_numemon', 0, 1, 4], ['ac_w_numemon', 0, 3, 4], ['ac_w_numemon', 0, 5, 4], ['ac_w_gekomon', 0, 3, 6]];
      if (n === 10) return [['ac_w_blackgarurmon', 0, 2, 4], ['ac_w_blackgarurmon', 0, 4, 4], ['ac_w_blackgarurmon', 0, 3, 5], ['ac_w_blackgarurmon', 1, 3, 6]];
      if (n === 15) return [['ac_w_fugamon', 1, 2, 4], ['ac_w_fugamon', 1, 4, 4], ['ac_w_ogremon', 1, 3, 4], ['ac_w_fugamon', 1, 3, 6]];
      if (n === 20) return [['ac_w_tyrannomon', 1, 1, 4], ['ac_w_tyrannomon', 1, 3, 4], ['ac_w_tyrannomon', 1, 5, 4], ['ac_w_metaltyrannomon', 1, 3, 6]];
      if (n === 25) return [['ac_w_monochromon', 1, 1, 4], ['ac_w_monochromon', 1, 3, 4], ['ac_w_monochromon', 1, 5, 4], ['ac_w_mammothmon', 1, 2, 5], ['ac_w_mammothmon', 1, 4, 5]];
      if (n === 30) return [['ac_w_airdramon', 1, 1, 5], ['ac_w_airdramon', 1, 5, 5], ['ac_w_gigadramon', 2, 3, 6], ['ac_w_airdramon', 1, 3, 5], ['ac_w_monochromon', 2, 3, 4]];
      if (n === 35) return [['ac_w_mammothmon', 2, 2, 4], ['ac_w_mammothmon', 2, 4, 4], ['ac_w_diaboromon', 1, 3, 6], ['ac_w_airdramon', 2, 1, 6], ['ac_w_airdramon', 2, 5, 6]];
      if (n === 40) return [['ac_w_goldguardromon', 2, 2, 4], ['ac_w_goldguardromon', 2, 4, 4], ['ac_w_machinedramon', 2, 3, 5], ['ac_w_gigadramon', 2, 3, 7]];
      if (n === 45) return [['ac_w_kimeramon', 2, 2, 4], ['ac_w_kimeramon', 2, 4, 4], ['ac_w_millenniummon', 2, 3, 6], ['ac_w_diaboromon', 2, 1, 6], ['ac_w_diaboromon', 2, 5, 6]];
      return [['ac_w_machinedramon', 2, 2, 4], ['ac_w_millenniummon', 2, 4, 4], ['ac_w_apocalymon', 2, 3, 6], ['ac_w_kimeramon', 2, 1, 5], ['ac_w_kimeramon', 2, 5, 5]];
    },
    // creep loot: win = pick 1 of 3 next round, loss = one random item (autochess-mech.js)
    reward: (c, won) => (DT.AutoChess ? DT.AutoChess.reward(c, won) : []),
  };

  // ---------- items ----------
  // Dota Auto Chess drops, adapted: 13 parts in 3 drop tiers that each work alone, and two-part recipes. Two parts on
  // one Digimon combine. Tier 4 drops are finished items. Passives use the engine library (FORMAT.md); items with
  // hooks get them in autochess-mech.js.
  const C = (name, tier, color, stats, desc, short) => ({ name, tier, color, stats, desc, short });
  const components = {
    ac_attack_chip: C('Attack Chip', 1, '#ff7a45', { ad: 0.12 }, '+12% AD. (Blades of Attack)', 'AT'),
    ac_defense_chip: C('Defense Chip', 1, '#c0c6d0', { armor: 18 }, '+18 armor. (Chainmail)', 'DF'),
    ac_speed_chip: C('Speed Chip', 1, '#ffd23f', { as: 0.12 }, '+12% attack speed. (Quarterstaff)', 'SP'),
    ac_recovery_disk: C('Recovery Disk', 1, '#5adf8a', {}, 'Regenerate 1.2% max HP per second. (Ring of Health)', 'RC'),
    ac_vampire_fang: C('Vampire Fang', 1, '#d83a5a', { omnivamp: 0.1 }, '+10% lifesteal on all damage. (Morbid Mask)', 'VF'),
    ac_resist_cape: C('Resist Cape', 1, '#3fd6b0', { mr: 18 }, '+18 magic resist. (Cloak)', 'RS'),
    ac_mp_chip: C('MP Chip', 1, '#4fb3ff', { mana: 15 }, '+15 starting mana. (Void Stone)', 'MP'),
    ac_hp_chip: C('HP Chip', 2, '#ff4d64', { hp: 200 }, '+200 max HP. (Vitality Booster)', 'HP'),
    ac_brain_chip: C('Brain Chip', 2, '#b26bff', { ap: 20 }, '+20 ability power. (Staff of Wizardry)', 'BR'),
    ac_power_claw: C('Power Claw', 2, '#ff9a3a', { ad: 0.2 }, '+20% AD. (Broadsword)', 'PC'),
    ac_hyper_chip: C('Hyper Chip', 3, '#ffe94a', { as: 0.3 }, '+30% attack speed. (Hyperstone)', 'HY'),
    ac_dark_edge: C('Dark Edge', 3, '#7a4aff', { ad: 0.32 }, '+32% AD. (Demon Edge)', 'DE'),
    ac_holy_ring: C('Holy Ring', 3, '#fff0a0', { hp: 220, mana: 20 }, '+220 max HP, +20 starting mana. (Ultimate Orb)', 'HR'),
  };
  const R = (a, b, id, name, desc, p, extra, tags, aiRank) => ({ a, b, id, name, desc, p: p || {}, extra: extra || {}, tags, aiRank: aiRank || 0 });
  const recipes = [
    R('ac_power_claw', 'ac_attack_chip', 'ac_crystal_claw', 'Crystal Claw', '+20% critical strike chance. (Crystalys)', null, { crit: 0.2 }, ['ad'], 2),
    R('ac_power_claw', 'ac_dark_edge', 'ac_grand_claw', 'Grand Claw', '+25% crit chance, critical strikes deal 60% more. (Daedalus)', { critDmg: 0.6 }, { crit: 0.25 }, ['ad'], 4),
    R('ac_attack_chip', 'ac_speed_chip', 'ac_thunder_disk', 'Thunder Disk', 'Every third attack sends lightning to 3 enemies for 70 magic damage. (Maelstrom)', { volt: 70 }, null, ['ad'], 1),
    R('ac_speed_chip', 'ac_hyper_chip', 'ac_thunder_hammer', 'Thunder Hammer', 'Every third attack sends lightning to 3 enemies for 140 magic damage. (Mjollnir)', { volt: 140 }, null, ['ad'], 4),
    R('ac_vampire_fang', 'ac_speed_chip', 'ac_berserk_mask', 'Berserk Mask', 'Each attack grants 5% attack speed, stacking. (Mask of Madness)', { rage: 0.05 }, null, ['ad'], 2),
    R('ac_recovery_disk', 'ac_dark_edge', 'ac_fury_axe', 'Fury Axe', 'Regenerates 1.2% HP/s. Attacks also hit a second enemy for 50%. (Battle Fury)', { split: 0.5, regen: 0.012 }, null, ['ad'], 3),
    R('ac_attack_chip', 'ac_attack_chip', 'ac_corrosion_blade', 'Corrosion Blade', 'Attacks cut the target armor by 35% for 3 s. (Desolator)', { shred: 0.35 }, null, ['ad'], 2),
    R('ac_hp_chip', 'ac_power_claw', 'ac_black_gear', 'Black Gear', 'Immune to stuns for the first 12 seconds. (Black King Bar)', { ccImmune: 12 }, null, ['ad', 'tank'], 2),
    R('ac_hp_chip', 'ac_hp_chip', 'ac_digicore', 'Digicore', '+300 more max HP. Regenerates 1% max HP per second. (Heart of Tarrasque)', { regen: 0.01 }, { hp: 300 }, ['tank'], 3),
    R('ac_defense_chip', 'ac_hyper_chip', 'ac_digizoid_armor', 'Chrome Digizoid Armor', 'At the start of combat, allies within 2 hexes gain 20 armor and 15% attack speed; enemies within 3 hexes lose 20 armor. (Assault Cuirass)', null, null, ['tank'], 3),
    R('ac_defense_chip', 'ac_power_claw', 'ac_reflect_mail', 'Reflect Mail', 'Returns 30% of physical damage taken to the attacker as magic damage. (Blade Mail)', null, null, ['tank'], 2),
    R('ac_dark_edge', 'ac_dark_edge', 'ac_divine_sword', 'Divine Sword', '+40% more AD. (Divine Rapier)', null, { ad: 0.4 }, ['ad'], 5),
    R('ac_mp_chip', 'ac_holy_ring', 'ac_refresh_disk', 'Refresh Disk', 'After the first cast each combat, mana refills at once. (Refresher Orb)', null, null, ['ap'], 4),
    R('ac_recovery_disk', 'ac_resist_cape', 'ac_data_hood', 'Data Hood', '+15 more magic resist. Regenerates 1% max HP per second. (Hood of Defiance)', { regen: 0.01 }, { mr: 15 }, ['tank'], 1),
    R('ac_hp_chip', 'ac_defense_chip', 'ac_brave_shield', 'Brave Shield', 'Takes 12% less damage. (Vanguard)', { dr: 0.12 }, null, ['tank'], 3),
    R('ac_brain_chip', 'ac_holy_ring', 'ac_numemon_wand', 'Numemon Wand', 'Every 7 s, turns the costliest enemy into a Numemon: it cannot act for 2.5 s. (Scythe of Vyse)', null, null, ['ap'], 4),
    R('ac_hyper_chip', 'ac_hyper_chip', 'ac_moon_chip', 'Moon Chip', '+35% more attack speed. (Moon Shard)', null, { as: 0.35 }, ['ad'], 3),
    R('ac_dark_edge', 'ac_speed_chip', 'ac_true_lance', 'True Strike Lance', 'Attacks deal 60 bonus magic damage. (Monkey King Bar)', null, null, ['ad'], 3),
    R('ac_brain_chip', 'ac_mp_chip', 'ac_hellfire_rod', 'Hellfire Rod', 'Every 6 s, deals 250 magic damage to a random enemy. (Dagon)', null, null, ['ap'], 3),
    R('ac_brain_chip', 'ac_brain_chip', 'ac_mystic_staff', 'Mystic Staff', '+40 more AP. Abilities can critically strike. (Mystic Staff)', { abilCrit: true }, { ap: 40, crit: 0.1 }, ['ap'], 4),
    R('ac_brain_chip', 'ac_resist_cape', 'ac_sage_orb', 'Sage Orb', 'Enemies within 2 hexes lose 25 armor and magic resist. (Kaya)', { veil: 25 }, null, ['ap'], 2),
    R('ac_recovery_disk', 'ac_mp_chip', 'ac_steady_disk', 'Steady Disk', 'Regenerates 1.2% HP/s. Attacks restore 5 more mana. (Perseverance)', { regen: 0.012, shojin: 5 }, null, ['ap'], 2),
    R('ac_vampire_fang', 'ac_dark_edge', 'ac_blood_ring', 'Blood Ring', '+20% more lifesteal. (Satanic)', { vamp: 0.2 }, null, ['ad'], 3),
    R('ac_attack_chip', 'ac_defense_chip', 'ac_stun_hammer', 'Stun Hammer', 'Attacks have a 15% chance to stun the target for 1.2 s. (Skull Basher)', null, null, ['ad', 'tank'], 1),
    R('ac_holy_ring', 'ac_holy_ring', 'ac_frost_ring', 'Frost Ring', 'Attacks slow the target attack speed by 25% for 2 s. (Eye of Skadi)', null, null, ['tank', 'ad'], 3),
    R('ac_defense_chip', 'ac_resist_cape', 'ac_medical_kit', 'Medical Kit', 'Every 5 s, heals allies within 2 hexes for 80 + 10% of missing HP. (Mekansm)', { redemption: true }, null, ['tank'], 2),
    R('ac_holy_ring', 'ac_dark_edge', 'ac_radiant_crest', 'Radiant Crest', 'Every 2 s, burns the nearest enemy within 2 hexes. (Radiance)', { sunfire: true }, null, ['ad', 'tank'], 3),
    R('ac_hp_chip', 'ac_speed_chip', 'ac_dragon_lance', 'Dragon Lance', 'Ranged holders gain 1 attack range. (Dragon Lance)', null, null, ['ad'], 1),
    R('ac_hp_chip', 'ac_mp_chip', 'ac_soul_disk', 'Soul Disk', 'More health and starting mana. (Soul Booster)', null, { mana: 10 }, ['ap', 'tank'], 1),
    R('ac_defense_chip', 'ac_defense_chip', 'ac_iron_shield', 'Iron Shield', 'Takes 10% less damage. (Poor Man\'s Shield)', { dr: 0.1 }, null, ['tank'], 1),
    R('ac_speed_chip', 'ac_speed_chip', 'ac_wind_feather', 'Wind Feather', '+20% dodge. (Butterfly)', null, { dodge: 0.2 }, ['ad'], 2),
    R('ac_vampire_fang', 'ac_defense_chip', 'ac_iron_helm', 'Iron Helm', 'Takes 6% less damage. (Helm of Iron Will)', { dr: 0.06 }, null, ['tank'], 1),
    R('ac_resist_cape', 'ac_resist_cape', 'ac_ward_cape', 'Ward Cape', 'At the start of combat, allies within 2 hexes gain 25 magic resist. (Pipe of Insight)', null, null, ['tank'], 1),
  ];

  // relics: pick 1 of 3 after the creep rounds 10, 20 and 30 (offered at 3-2, 5-2, 7-2); a new one replaces the old.
  const relic = (id, name, desc, color, color2) => ({ id, name, desc, cat: 'global', relic: true, noDrop: true, color, color2: color2 || '#3a2a10' });
  const list = [
    { id: 'ac_digiegg', name: 'Digi-Egg', consumable: true, color: '#fff3b0', color2: '#ff8a4a',
      desc: 'Wildcard. Drag onto a Rookie or Champion: it hatches into one more copy of that Digimon, which counts toward digivolving. Needs a copy left in the shared pool.' },
    relic('ac_r_treasure', 'Pirate Treasure', 'Gain 2 extra gold every round.', '#ffd84a'),
    relic('ac_r_forge', "Weaponsmith's Forge", 'Gain a random finished item now.', '#ff7a45'),
    relic('ac_r_vault', 'Great Vault', 'Interest cap rises from 5 to 8 gold.', '#ffcf4a'),
    relic('ac_r_charm', 'Light and Shadow Charm', 'Win and loss streak gold is doubled.', '#c9d1dc'),
    relic('ac_r_recruiter', 'Recruiter Badge', 'Your shop rolls as if you were one level higher.', '#7fd0ff'),
    relic('ac_r_grace', 'Grace of Nature', 'Your Digimon regenerate 1.2% max HP per second in combat.', '#5adf8a'),
    relic('ac_r_reaper', 'Sin Reaper', 'Your Digimon deal 10% more damage.', '#c0304a'),
    relic('ac_r_eggnest', 'Strange Egg Nest', 'Gain 2 Digi-Eggs now.', '#fff3b0'),
    relic('ac_r_card', 'Monopoly Card', 'One free reroll every round.', '#4fe08a'),
    relic('ac_r_key', 'Master Key', '+1 team size.', '#b0b8c8'),
    relic('ac_r_discount', 'Bargain Tag', 'Selling a Digimon pays 1 extra gold.', '#ffb04a'),
  ];

  // ---------- rules ----------
  // Dota Auto Chess, code of 2019-06 (see docs/sets/autochess.md for the values chosen where sources disagree).
  const offers = [{ kind: 'ac_loot', stage: 1, round: 2 }, { kind: 'ac_loot', stage: 1, round: 3 }, { kind: 'ac_loot', stage: 1, round: 4 }];
  for (let s = 3; s <= 20; s++) offers.push({ kind: 'ac_loot', stage: s, round: 1 });
  for (const s of [3, 5, 7]) offers.push({ kind: 'ac_relic', stage: s, round: 2 });
  // Hyper Roll pace: creeps at 1-1..1-3 and x-4, so loot picks at 1-2, 1-3 and x-1; relics at 3-2, 4-3, 5-3
  const hyperOffers = [{ kind: 'ac_loot', stage: 1, round: 2 }, { kind: 'ac_loot', stage: 1, round: 3 }];
  for (let s = 2; s <= 20; s++) hyperOffers.push({ kind: 'ac_loot', stage: s, round: 1 });
  hyperOffers.push({ kind: 'ac_relic', stage: 3, round: 2 }, { kind: 'ac_relic', stage: 4, round: 3 }, { kind: 'ac_relic', stage: 5, round: 3 });

  DT.Sets.register({
    id: 'autochess',
    name: 'Auto Chess',
    mode: 'autochess',
    order: 10,
    desc: 'Auto Chess: races and classes, 2-copy Druids, Wizards, creep loot, interest and XP.',
    colors: { main: '#e0a040', accent: '#7fe08a' },
    source: 'Dota Auto Chess (2019-06-26 build) plus standalone Auto Chess ideas. docs/inspo/autochess/',
    traits,
    lines,
    pve,
    items: { system: 'tft', components, recipes, list },
    augments: [],
    tamers: ['Davis', 'Ken', 'Yolei', 'Cody', 'TK', 'Kari', 'Willis'],
    rules: {
      startHp: 100,
      startGold: 1,
      planTime: 35,
      itemCap: 10,
      rerollCost: 2,
      shopSize: 5,
      winGold: 1,
      winGoldPvE: 1,
      poolSize: [0, 45, 30, 25, 15, 10],
      shopOdds: [null, [100, 0, 0, 0, 0], [70, 30, 0, 0, 0], [60, 35, 5, 0, 0], [50, 35, 15, 0, 0], [40, 35, 23, 2, 0],
        [33, 30, 30, 7, 0], [30, 30, 30, 10, 0], [24, 30, 30, 15, 1], [22, 30, 25, 20, 3], [19, 25, 25, 25, 6]],
      levelByStage: null,
      // XP to the next level (cumulative 1, 2, 4, 8, 16, 32, 56, 88, 128), +1 a round, 4 XP for 5 gold
      xp: { table: [0, 1, 1, 2, 4, 8, 16, 24, 32, 40], perRound: 1, buyCost: 5, buyXp: 4, startLevel: 1, maxLevel: 10 },
      incomeByStage: null,
      baseIncome: 5, // min(round, 5): rounds 1-4 pay less (onIncome in autochess-mech.js)
      interest: { per: 10, max: 5 },
      streak: [[3, 1], [5, 2], [7, 3]],
      // a lost round: a base amount by stage plus floor(1 + unit level / 3) per surviving enemy, unit level = cost + 2 x (stars - 1)
      lossDamage: (stage) => (stage <= 2 ? 0 : Math.min(stage - 2, 6)),
      unitDamage: (u) => Math.floor(1 + (u.line.tier + 2 * u.stage) / 3),
      pveLossDamage: () => 0,
      roundsInStage: () => 5,
      roundType: (stage, round) => (stage === 1 ? (round <= 3 ? 'pve' : 'pvp') : round === 5 ? 'pve' : 'pvp'),
      armoryStages: [],
      augmentStages: [],
      offers,
      copies: 3,
      maxStage: 2,
      // Hyper Roll pace (docs/sets/FORMAT.md, Pace): the engine's Auto Chess profile (20 HP, levels by stage, fixed
      // income, creeps at 1-1..1-3 and x-4) plus this set's loot and relic rounds for that structure.
      hyper: { offers: hyperOffers },
    },
    codexRules: (R, pace) => (pace === 'hyper' ? [
      { id: 'ac_rounds', title: 'Rounds', html: 'Hyper Roll: three creep rounds in stage 1, then four rounds a stage with creeps at x-4. Win a creep round: pick 1 of 3 items next round. Lose it: one random item. Relics: pick 1 of 3 at 3-2, 4-3 and 5-3; a new relic replaces the old one.' },
      { id: 'ac_combine', title: 'Digivolving', html: '3 copies digivolve (2 for Druids). The Digi-Egg counts as one copy of whatever you drop it on.' },
    ] : [
      { id: 'ac_econ', title: 'Economy', html: 'Income each round: 5 gold (1, 2, 3, 4 in rounds 1-4), plus 1 gold per 10 held (up to 5), plus streak gold (3-4 in a row: 1, 5-6: 2, 7+: 3, for wins or losses), plus 1 for a win. Rerolls cost 2. Buy XP: 4 XP for 5 gold; 1 XP each round. Max level 10.' },
      { id: 'ac_rounds', title: 'Rounds', html: 'Five rounds a stage. Creep rounds: 1, 2, 3 and every fifth round from 10 (2-5, 3-5, ...). Win a creep round: pick 1 of 3 items next round. Lose it: one random item. Relics: pick 1 of 3 at 3-2, 5-2 and 7-2; a new relic replaces the old one.' },
      { id: 'ac_damage', title: 'Damage', html: 'A lost round costs a base amount (0 through stage 2, then stage - 2, up to 6) plus floor(1 + level / 3) for every surviving enemy Digimon, where level = cost + 2 per digivolution. Creep rounds only deal the per-unit part. Start with 100 HP.' },
      { id: 'ac_combine', title: 'Digivolving', html: '3 copies digivolve (2 for Druids). The Digi-Egg counts as one copy of whatever you drop it on.' },
    ]),
  });
})(typeof window !== 'undefined' ? window : globalThis);
