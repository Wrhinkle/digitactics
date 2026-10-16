// Underlords: Dota Underlords (Season One / final patch 2020-11-19) as a Digimon set. Mode 'underlords'.
// 62 hero lines (the whole final hero pool), 26 alliances with Underlords breakpoints, tiered items with no recipes
// (pick 1 of 3 every 4 rounds), hats that add an alliance, global items, and four Tamers (the Underlords) picked at
// round 10. The Tamer stands on the board as a unit and ranks up every 5 rounds. Creep rounds at 1, 2, 3 and every
// 5th round. Behavior (alliances, hero kits, 3-star effects, items, Tamers) is in underlords-mech.js; art in
// underlords-art.js; design notes in docs/sets/underlords.md. Corpus: docs/inspo/underlords/.
// Round numbering: 5 rounds per stage, so round N is stage ceil(N/5), round ((N-1) % 5) + 1 (round 10 = 2-5).
// A second, shorter variant is registered at the end: Underlords Knockout (id 'underlords_ko').
(function (G) {
  const DT = G.DT;
  const L = DT.line;
  const pct = DT.pct;

  // ---------- alliances ----------
  // Values are tuned for this engine's stat scale (docs/sets/underlords.md lists the Underlords originals).
  const T = (name, color, bp, lv, desc, extra = {}) => ({ name, kind: 'alliance', color, bp, lv, desc, ...extra });
  const traits = {
    SHINOBI: T('Shinobi', '#e0405a', [3, 6], [0.12, 0.22],
      'Shinobi leap to the enemy back line. They gain a chance to land a critical hit; a critical hit wounds the target (healing on it is cut).',
      { critDmg: [2.2, 2.8], crestSymbol: 'dagger', src: 'Underlords Assassin', fmt: (v, i) => `${pct(v)} crit for ${[220, 280][i]}% damage` }),
    BEAST: T('Beast Kings', '#c9853a', [2, 4], [6, 12],
      'Beast Kings gain max HP, plus more for every knockout scored by your Beast Kings this game (multiplied by their star level).',
      { base: [200, 450], crestSymbol: 'flame', src: 'Underlords Brawny', fmt: (v, i) => `+${[200, 450][i]} HP, +${v} HP per knockout (x star level)` }),
    ONI: T('Oni', '#9a4a2a', [2, 4], [0.3, 0.5],
      'Oni attacks weaken the target: it deals less damage for 4 seconds. The first hit of the weakening deals bonus magic damage.',
      { dmg: [80, 120], crestSymbol: 'skull', src: 'Underlords Brute', fmt: (v, i) => `-${pct(v)} enemy damage, ${[80, 120][i]} on apply` }),
    LEGEND: T('Legend', '#ffd86a', [1], [0.03],
      'A Legend counts as a member of every alliance active on your team, and gains 3% damage and 8% max HP for each active alliance.',
      { hp: [0.08], crestSymbol: 'star5', src: 'Underlords Champion', fmt: () => 'all alliances, +3% damage and +8% HP per alliance' }),
    DEMON: T('Demon Lords', '#c0204a', [1], [0.5],
      'When a Demon Lord casts, your other Demon Lords are silenced for 2.5 seconds and the caster deals 50% more damage for each of your living Demon Lords for 4 seconds.',
      { crestSymbol: 'skull', src: 'Underlords Demon', fmt: () => '+50% damage per Demon Lord after casting' }),
    DRAGON: T("Dragon's Roar", '#ff6a2a', [2], [0.4],
      "Dragon's Roar Digimon unlock their draconic power: +15 ability power, their abilities burn, and they splash 40% of their damage onto enemies next to the target.",
      { crestSymbol: 'flame', src: 'Underlords Dragon', fmt: () => '+15 AP, abilities burn and splash 40%' }),
    UNDEAD: T('Undead', '#7f8a6a', [3, 6], [150, 150],
      'The first time each Undead Digimon falls, allies within 2 hexes heal and deal 6% more damage for the rest of combat. With 6, each Undead also rises again once with 40% HP.',
      { crestSymbol: 'skull', src: 'Underlords Fallen', fmt: (v, i) => `heal ${v}, +6% damage${i ? ', revive once' : ''}` }),
    HOLY: T('Holy Healers', '#7ff0c0', [2, 4], [0.15, 0.3],
      'All healing on your team is stronger.',
      { crestSymbol: 'cross', src: 'Underlords Healer', fmt: (v) => `+${pct(v)} healing` }),
    NIGHT: T('Nightmare Soldiers', '#8a4adf', [2, 4, 6], [18, 35, 55],
      'All enemies lose armor.',
      { crestSymbol: 'moon', src: 'Underlords Heartless', fmt: (v) => `enemies -${v} armor` }),
    PARTNER: T('Partners', '#4fb3ff', [2, 4, 6], [2, 4, 7],
      'Partner Digimon fight beside their humans: all allies generate mana every second.',
      { crestSymbol: 'star6', src: 'Underlords Human', fmt: (v) => `+${v} mana per second to all allies` }),
    HUNTER: T('Hunters', '#6ac05a', [3, 6], [0.35, 0.45],
      'Hunters have a chance to quickly attack again (twice more with 6).',
      { crestSymbol: 'crosshair', src: 'Underlords Hunter', fmt: (v, i) => `${pct(v)} chance of ${i + 1} extra attack${i ? 's' : ''}` }),
    KNIGHT: T('Knights', '#8fb4d9', [2, 4, 6], [0.15, 0.2, 0.25],
      'Knights take less damage, and that much less again while standing next to another Knight.',
      { crestSymbol: 'shield', src: 'Underlords Knight', fmt: (v) => `-${pct(v)} damage, x2 beside a Knight` }),
    WIZARD: T('Wizards', '#b86bff', [3, 6], [0.2, 0.4],
      'Enemies take more magic damage from your team.',
      { crestSymbol: 'star6', src: 'Underlords Mage', fmt: (v) => `+${pct(v)} magic damage taken by enemies` }),
    SAGE: T('Sage', '#d0a0ff', [1], [3],
      'Once per combat, when a Sage drops below 50% HP, it becomes invulnerable but cannot act for 3 seconds.',
      { crestSymbol: 'star6', src: 'Underlords Magus', fmt: () => '3 s invulnerable at 50% HP' }),
    TOXIC: T('Toxic', '#8adf3a', [3], [1],
      'Toxic attacks poison the target. Each poison stack deals 4 magic damage per second and slows attack speed by 4% (up to 10 stacks, 6 seconds).',
      { crestSymbol: 'sludge', src: 'Underlords Poisoner', fmt: () => 'attacks add a poison stack' }),
    ROGUE: T('Rogues', '#c9a227', [2, 4], [0.17, 0.32],
      'Rogues evade basic attacks. Each evaded attack gives the Rogue attack damage for the rest of combat.',
      { ad: [8, 16], crestSymbol: 'dagger', src: 'Underlords Rogue', fmt: (v, i) => `${pct(v)} evasion, +${[8, 16][i]} AD per evade` }),
    NATURE: T('Nature Spirits', '#4fc060', [2, 4, 6], [4, 6, 9],
      'Each landed attack gives attack damage for the rest of combat: Nature Spirits only, then also your summons (4), then every ally (6).',
      { crestSymbol: 'flame', src: 'Underlords Savage', fmt: (v, i) => `+${v} AD per attack (${['Nature Spirits', 'and summons', 'all allies'][i]})` }),
    DEEP: T('Deep Savers', '#2f8bff', [2, 4], [30, 55],
      'All allies gain magic resist.',
      { crestSymbol: 'pearl', src: 'Underlords Scaled', fmt: (v) => `+${v} MR to all allies` }),
    SHAMAN: T('Shamans', '#3fa080', [2, 4, 6], [1, 2, 3],
      'At the start of combat a beast joins your team: a wolf, then a wildwing (4), then a black dragon (6).',
      { crestSymbol: 'moon', src: 'Underlords Shaman', fmt: (v, i) => ['summon a wolf', 'summon a wildwing', 'summon a black dragon'][i] }),
    SPIRIT: T('Legendary Spirits', '#f2a93b', [3], [130],
      'When a Spirit casts it becomes charged. Once 3 Spirits are charged they strike together: magic damage and a short stun to every enemy near the middle of the three (more at higher stars).',
      { crestSymbol: 'dtector', src: 'Underlords Spirit', fmt: (v) => `Delta strike: ${v} x star per Spirit` }),
    SUMMONER: T('Summoners', '#5fd0ff', [2, 4], [0.2, 0.4],
      'Your summoned units deal more damage.',
      { crestSymbol: 'xros', src: 'Underlords Summoner', fmt: (v) => `summons +${pct(v)} damage` }),
    BLADE: T('Blademasters', '#c8d0e0', [3, 6], [0.1, 0.22],
      'Blademasters deal more damage for each enemy next to them.',
      { crestSymbol: 'sword', src: 'Underlords Swordsman', fmt: (v) => `+${pct(v)} damage per adjacent enemy` }),
    JUNGLE: T('Jungle Troopers', '#3f9a3a', [2, 4], [0.25, 0.5],
      'Jungle Troopers gain attack speed, and your other allies gain half as much.',
      { other: [0.08, 0.16], crestSymbol: 'bug', src: 'Underlords Troll', fmt: (v, i) => `+${pct(v)} AS (others +${pct([0.08, 0.16][i])})` }),
    WIND: T('Wind Guardians', '#9be7ff', [2, 4], [0.4, 0.5],
      'When an enemy casts, your Wind Guardians (every ally with 4) in range switch to it, and their next attack on it deals bonus damage.',
      { crestSymbol: 'crosshair', src: 'Underlords Vigilant', fmt: (v, i) => `${i ? 'all allies' : 'Wind Guardians'} focus casters, +${pct(v)} next hit` }),
    DARK: T('Dark Area', '#5a3a8a', [3], [0.01],
      "Every ally's attacks have a 25% chance to deal 1% of the target's max HP as true damage (once every 0.5 s per ally).",
      { crestSymbol: 'moon', src: 'Underlords Void', fmt: (v) => `25%: +${pct(v)} max HP true damage` }),
    METAL: T('Metal Empire', '#a0a8b8', [3, 6], [45, 100],
      'Metal Empire Digimon gain armor.',
      { crestSymbol: 'shield', src: 'Underlords Warrior', fmt: (v) => `+${v} armor` }),
  };
  for (const k in traits) traits[k].bpDesc = traits[k].lv.map((v, i) => traits[k].fmt(v, i));

  // ---------- stat templates for Tamers (they do not cast with mana; their abilities run on cooldowns) ----------
  const roles = {
    TamerSupport: { hp: 1100, ad: 0, as: 0.05, range: 4, armor: 20, mr: 20, mana: 0, startMana: 0, ai: { place: 'back', tank: 0, carry: false, dmg: 'ap' } },
    TamerRanged: { hp: 1300, ad: 70, as: 0.67, range: 3, armor: 25, mr: 20, mana: 0, startMana: 0, ai: { place: 'back', tank: 0, carry: false, dmg: 'ad' } },
    TamerTank: { hp: 1450, ad: 100, as: 0.5, range: 1, armor: 40, mr: 30, mana: 0, startMana: 0, ai: { place: 'front', tank: 0, carry: false, dmg: 'ad' } },
    TamerSkirmish: { hp: 1200, ad: 70, as: 1.2, range: 2, armor: 25, mr: 20, mana: 0, startMana: 0, ai: { place: 'back', tank: 0, carry: false, dmg: 'ad' } },
  };

  // ---------- heroes ----------
  // L(id, tier, alliances, attribute, forms, abilities, type, options). Every line is a placeholder (unassigned) for now.
  const H = (id, tier, tr, attr, forms, abil, type, src, opt = {}) => L(`ul_${id}`, tier, tr, attr, forms, abil, type, { unassigned: true, src: `Underlords ${src}`, ...opt });
  const SPIRIT_LV = ['Human Spirit', 'Beast Spirit', 'Fused Spirit'];
  const lines = [
    // ---- tier 1 (16) ----
    H('antimage', 1, ['ROGUE', 'HUNTER'], 'Virus', ['BlackGabumon', 'BlackGarurumon', 'BlackMetalGarurumon'], ['Little Horn', 'Howling Blaster', 'Cocytus Breath'], 'ul_passive', 'Anti-Mage', { role: 'Duelist', kit: 'manabreak', adMul: 0.9 }),
    H('batrider', 1, ['JUNGLE', 'KNIGHT'], 'Vaccine', ['Hawkmon', 'Saberdramon', 'Ravemon'], ['Beak Pummel', 'Mach Shadow', 'Blast Wing'], 'ul_napalm', 'Batrider', { role: 'Mystic', hpMul: 1.25 }),
    H('bounty', 1, ['ROGUE', 'SHINOBI'], 'Virus', ['Gazimon', 'Musyamon', 'Asuramon'], ['Electric Stun Blast', 'Musya Slash', 'Asura Fist'], 'ul_shuriken', 'Bounty Hunter', { role: 'Assassin' }),
    H('crystal', 1, ['PARTNER', 'WIZARD'], 'Vaccine', ['Penguinmon', 'Icemon', 'Merukimon'], ['Ice Prism', 'Iceball Bomb', 'Thousand Fists'], 'ul_frostbite', 'Crystal Maiden', { role: 'Mystic', range: 4 }),
    H('dazzle', 1, ['JUNGLE', 'HOLY', 'TOXIC'], 'Data', ['Aruraumon', 'RedVegiemon', 'Lotosmon'], ['Nemesis Ivy', 'Hazard Breath', 'Serpent Cure'], 'ul_shadowwave', 'Dazzle', { role: 'Support' }),
    H('drow', 1, ['NIGHT', 'WIND', 'HUNTER'], 'Vaccine', ['Hyokomon', 'Peckmon', 'Valkyrimon'], ['Downward Cleave', 'Kunai Wing', "Aurvandil's Arrow"], 'ul_passive', 'Drow Ranger', { role: 'Sniper', kit: 'aura', adMul: 1.15, adMul: 1.15, levels: ['Rookie', 'Armor', 'Mega'] }),
    H('enchantress', 1, ['SHAMAN', 'HOLY'], 'Data', ['Floramon', 'Kiwimon', 'Blossomon'], ['Rain of Pollen', 'Pummel Peck', 'Spiral Flower'], 'ul_attendants', 'Enchantress', { role: 'Support', range: 2 }),
    H('lich', 1, ['UNDEAD', 'WIZARD'], 'Vaccine', ['Gammamon', 'IceDevimon', 'Phantomon'], ['Gamma Chill', 'Zero Freeze', 'Soul Chopper'], 'ul_frostshield', 'Lich', { role: 'Mystic' }),
    H('magnus', 1, ['NATURE', 'SHAMAN'], 'Data', ['Kamemon', 'Monochromon', 'Mammothmon'], ['Shell Guard', 'Guarding Tusk', 'Tusk Strikes'], 'ul_empower', 'Magnus', { role: 'Bruiser', hpMul: 1.15 }),
    H('phantom', 1, ['SHINOBI', 'BLADE'], 'Data', ['Psychemon', 'Kyubimon (Silver)', 'Kuzuhamon'], ['Colored Spark', 'Dragon Wheel', 'Illuminator'], 'ul_passive', 'Phantom Assassin', { role: 'Assassin', kit: 'coup' }),
    H('shadowdemon', 1, ['NIGHT', 'DEMON'], 'Virus', ['Tsukaimon', 'Devidramon', 'SkullSatamon'], ['Bad Message', 'Crimson Claw', 'Skull Hammer'], 'ul_purge', 'Shadow Demon', { role: 'Mystic', hpMul: 1.4 }),
    H('slardar', 1, ['DEEP', 'METAL'], 'Virus', ['Syakomon', 'Coelamon', 'Dragomon'], ['Water Pressure', 'Fossil Bite', 'Forbidden Trident'], 'ul_haze', 'Slardar', { role: 'Bruiser' }),
    H('snapfire', 1, ['BEAST', 'DRAGON'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Slayerdramon'], ['Fire Vortex', 'Flog Shot', 'Tenryu Slash'], 'ul_scatter', 'Snapfire', { role: 'Striker', range: 2, hpMul: 1.15, adMul: 1.1 }),
    H('tusk', 1, ['NATURE', 'METAL'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Zudomon'], ['Sharp Sider', 'Harpoon Torpedo', "Vulcan's Hammer"], 'ul_punch', 'Tusk', { role: 'Bruiser', hpMul: 1.3 }),
    H('vengeful', 1, ['UNDEAD', 'NIGHT'], 'Vaccine', ['Tapirmon', 'Piddomon', 'Ophanimon'], ['Nightmare Syndrome', 'Fire Phaser', "Eden's Javelin"], 'ul_terror', 'Vengeful Spirit', { role: 'Mystic', range: 2, hpMul: 1.25 }),
    H('venomancer', 1, ['DEEP', 'SUMMONER', 'TOXIC'], 'Virus', ['Mushroomon', 'Vegiemon', 'Arukenimon'], ['Fungus Crusher', 'Sweet Breath', 'Spider Thread'], 'ul_summon', 'Venomancer', { role: 'Mystic', summon: 'ul_s_ward' }),
    // ---- tier 2 (14) ----
    H('bristleback', 2, ['BEAST', 'NATURE'], 'Virus', ['Kunemon', 'Flymon', 'TigerVespamon'], ['Electric Thread', 'Brown Stingers', 'Mach Stinger V'], 'ul_quill', 'Bristleback', { role: 'Bruiser', hpMul: 1.15, radius: 2 }),
    H('chaosknight', 2, ['DEMON', 'KNIGHT'], 'Virus', ['BlackAgumon', 'BlackGreymon', 'BlackMetalGreymon'], ['Spitfire', 'Great Antler', 'Dark Destroyer'], 'ul_chaosbolt', 'Chaos Knight', { role: 'Bruiser' }),
    H('earthspirit', 2, ['SPIRIT', 'METAL'], 'Free', ['Vorvomon', 'Lavorvomon', 'Lavogaritamon'], ['Vorvo Flame', 'Lava Burst', 'Volcanic Howl'], 'ul_grip', 'Earth Spirit', { role: 'Guardian' }),
    H('juggernaut', 2, ['BEAST', 'BLADE'], 'Data', ['Kotemon', 'Grademon', 'Zanbamon'], ['Meteor Slash', 'Grade Slash', 'Zanba Rend'], 'ul_bladefury', 'Juggernaut', { role: 'Duelist' }),
    H('kunkka', 2, ['PARTNER', 'METAL', 'BLADE'], 'Vaccine', ['Crabmon', 'ShellNumemon', 'Neptunemon'], ['Scissors Execution', 'Shell Rush', 'Gate of Atlantis'], 'ul_ghostship', 'Kunkka', { role: 'Striker', radius: 2 }),
    H('legion', 2, ['PARTNER', 'LEGEND'], 'Data', ['Zubamon', 'ZubaEagermon', 'Durandamon'], ['Zuba Saber', 'Eager Slash', 'Durandal'], 'ul_duel', 'Legion Commander', { role: 'Duelist', hpMul: 0.9 }),
    H('luna', 2, ['WIND', 'KNIGHT'], 'Vaccine', ['Terriermon', 'Sistermon Noir', 'Dianamon'], ['Bunny Blast', 'Mickey Bullet', 'Crescent Harken'], 'ul_passive', 'Luna', { role: 'Sniper', range: 3, kit: 'glaives' }),
    H('meepo', 2, ['ROGUE', 'SUMMONER'], 'Data', ['Kokuwamon', 'Tankmon', 'Datamon'], ['Electro Strike', 'Hyper Cannon', 'Digital Bomb'], 'ul_poof', 'Meepo', { role: 'Duelist' }),
    H('prophet', 2, ['SHAMAN', 'SUMMONER'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Seed Blast', 'Sunshine Beam', 'Lila Shower'], 'ul_summon', "Nature's Prophet", { role: 'Mystic', summon: 'ul_s_treant' }),
    H('pudge', 2, ['NIGHT', 'METAL'], 'Virus', ['Goblimon', 'Raremon', 'Boltmon'], ['Goblin Strike', 'Stinking Gas', 'Tomahawk Stinger'], 'ul_hook', 'Pudge', { role: 'Guardian', hpMul: 1.2 }),
    H('queenofpain', 2, ['DEMON', 'SHINOBI', 'TOXIC'], 'Virus', ['Impmon', 'LadyDevimon', 'Lilithmon'], ['Night of Fire', 'Darkness Wave', 'Phantom Pain'], 'ul_scream', 'Queen of Pain', { role: 'Assassin', radius: 2, hpMul: 1.15 }),
    H('spiritbreaker', 2, ['NATURE', 'ONI'], 'Data', ['Elecmon', 'Aegiomon', 'Kentaurosmon'], ['Body Attack', 'Thunder Charge', 'Bifrost'], 'ul_charge', 'Spirit Breaker', { role: 'Bruiser' }),
    H('stormspirit', 2, ['SPIRIT', 'WIZARD'], 'Free', ['Beetlemon', 'MetalKabuterimon', 'MegaKabuterimon'], ['Lightning Blitz', 'Bollo Thunder', 'Horn Buster'], 'line', 'Storm Spirit', { role: 'Mystic', hpMul: 1.15, mana: 50, levels: SPIRIT_LV }),
    H('windranger', 2, ['WIND', 'HUNTER'], 'Free', ['Kazemon', 'Zephyrmon', 'JetSilphymon'], ['Hurricane Wave', 'Gilgamesh Slicer', 'Jet Tornado'], 'line', 'Windranger', { role: 'Sniper', adMul: 1.1, mana: 60, levels: SPIRIT_LV }),
    // ---- tier 3 (13) ----
    H('abaddon', 3, ['UNDEAD', 'KNIGHT'], 'Data', ['Dracmon', 'Knightmon', 'Craniamon'], ['Undead Fang', 'Berserk Sword', "Waltz's End"], 'ul_aphotic', 'Abaddon', { role: 'Guardian' }),
    H('alchemist', 3, ['ONI', 'ROGUE', 'TOXIC'], 'Virus', ['Gotsumon', 'Ogremon', 'Fugamon'], ['Rock Fist', 'Bone Cudgel', 'Evil Hurricane'], 'ul_acid', 'Alchemist', { role: 'Bruiser', hpMul: 1.1, radius: 2 }),
    H('beastmaster', 3, ['BEAST', 'HUNTER', 'SHAMAN'], 'Vaccine', ['Gaomon', 'Leomon', 'SaberLeomon'], ['Rolling Upper', 'Beast Sword', 'Infinity Arrow'], 'ul_axes', 'Beastmaster', { role: 'Striker', adMul: 1.1, hpMul: 1.25 }),
    H('emberspirit', 3, ['SPIRIT', 'SHINOBI', 'BLADE'], 'Free', ['Agunimon', 'BurningGreymon', 'EmperorGreymon'], ['Pyro Darts', 'Crimson Fanger', 'Pyro Dragons'], 'ul_sleight', 'Ember Spirit', { role: 'Assassin', levels: SPIRIT_LV }),
    H('lifestealer', 3, ['NIGHT', 'ONI'], 'Virus', ['Keramon', 'Gururumon', 'Infermon'], ['Crazy Giggle', 'Chaos Fire', 'Spider Shooter'], 'ul_rage', 'Lifestealer', { role: 'Duelist', kit: 'feast', levels: ['Rookie', 'Champion', 'Ultimate'] }),
    H('lycan', 3, ['PARTNER', 'NATURE', 'SUMMONER'], 'Vaccine', ['Labramon', 'Dobermon', 'MetalGarurumon'], ['Retriever Rush', 'Grau Larm', 'Metal Wolf Claw'], 'ul_shapeshift', 'Lycan', { role: 'Striker' }),
    H('omniknight', 3, ['PARTNER', 'KNIGHT', 'HOLY'], 'Data', ['Patamon', 'Unimon', 'Seraphimon'], ['Air Shot', 'Holy Shot', 'Strike of the Seven Stars'], 'ul_purify', 'Omniknight', { role: 'Guardian' }),
    H('puck', 3, ['DRAGON', 'WIZARD'], 'Data', ['Dracomon', 'Coredramon (Blue)', 'Wingdramon'], ['Baby Breath', 'Blue Flare Breath', 'Sonic Blast'], 'line', 'Puck', { role: 'Mystic', hpMul: 1.3, mana: 45, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('shadowshaman', 3, ['JUNGLE', 'SUMMONER'], 'Virus', ['Shamanmon', 'Gekomon', 'ShogunGekomon'], ['Shama Hammer', 'Symphony Crusher', 'Musical Fist'], 'ul_wards', 'Shadow Shaman', { role: 'Mystic' }),
    H('slark', 3, ['DEEP', 'SHINOBI'], 'Virus', ['Gizamon', 'Gesomon', 'MegaSeadramon'], ['Spiral Edge', 'Coral Crusher', 'Thunder Javelin'], 'ul_passive', 'Slark', { role: 'Assassin', kit: 'essence', hpMul: 1.25, adMul: 1.1 }),
    H('spectre', 3, ['DARK', 'DEMON'], 'Virus', ['Soulmon', 'Bakemon', 'Piedmon'], ['Necro Magic', "Hell's Hand", 'Trump Sword'], 'ul_dagger', 'Spectre', { role: 'Duelist', hpMul: 1.15, adMul: 1.1 }),
    H('terrorblade', 3, ['DEMON', 'HUNTER', 'UNDEAD'], 'Virus', ['Dorumon', 'DexDorugamon', 'DexDorugoramon'], ['Metal Cannon', 'Dex Power Metal', 'Dex Brave Metal'], 'ul_metamorph', 'Terrorblade', { role: 'Striker', hpMul: 1.15, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('treant', 3, ['SHAMAN', 'HOLY'], 'Virus', ['Palmon', 'Woodmon', 'Cherrymon'], ['Poison Ivy', 'Branch Drain', 'Cherry Bomb'], 'ul_leech', 'Treant Protector', { role: 'Guardian' }),
    // ---- tier 4 (12) ----
    H('deathprophet', 4, ['UNDEAD', 'NIGHT'], 'Virus', ['Witchmon', 'Myotismon', 'GranDracmon'], ['Baluluna Gale', 'Crimson Lightning', 'Death Scream'], 'ul_exorcism', 'Death Prophet', { role: 'Mystic', hpMul: 1.4, levels: ['Champion', 'Ultimate', 'Mega'] }),
    H('doom', 4, ['DEMON', 'ONI'], 'Virus', ['Meramon', 'SkullMeramon', 'Belphemon Rage Mode'], ['Burning Fist', 'Heat Chain', 'Lampranthus'], 'ul_doom', 'Doom', { role: 'Bruiser', hpMul: 1.15, levels: ['In-Training', 'Ultimate', 'Mega'] }),
    H('lina', 4, ['PARTNER', 'WIZARD'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Corona Flame', 'Flame Dive', 'Sol Blaster'], 'ul_laguna', 'Lina', { role: 'Mystic', range: 4, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('lonedruid', 4, ['NATURE', 'SHAMAN', 'SUMMONER'], 'Free', ['Kumamon', 'Grizzlymon', 'BanchoLeomon'], ['Frozen Tundra', 'Maul Attack', 'Flashy Boss Punch'], 'ul_summon', 'Lone Druid', { role: 'Striker', range: 3, summon: 'ul_s_bear', levels: ['Rookie', 'Champion', 'Mega'] }),
    H('mirana', 4, ['WIND', 'HUNTER'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'ul_arrow', 'Mirana', { role: 'Sniper', hpMul: 1.15 }),
    H('pangolier', 4, ['NATURE', 'BLADE'], 'Free', ['Armadillomon', 'Ankylomon', 'Shakkoumon'], ['Diamond Shell', 'Armored Stampede', 'Justice Beam'], 'ul_shieldcrash', 'Pangolier', { role: 'Duelist', hpMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('rubick', 4, ['WIZARD', 'SAGE'], 'Virus', ['Kudamon', 'Sorcermon', 'Wisemon'], ['Kuda Strike', 'Crystal Cloud', 'Dialogue'], 'ul_fadebolt', 'Rubick', { role: 'Mystic', range: 2, hpMul: 1.2 }),
    H('sven', 4, ['ROGUE', 'KNIGHT', 'BLADE'], 'Data', ['Hackmon', 'SaviorHuckmon', 'Jesmon X'], ['Fifth Rush', 'Meteor Flame', 'Ultimate Seibaken'], 'ul_godstrength', 'Sven', { role: 'Striker', hpMul: 1.3, adMul: 1.1, levels: ['Rookie', 'Ultimate', 'Mega'] }),
    H('templar', 4, ['WIND', 'DARK', 'SHINOBI'], 'Data', ['Renamon', 'Youkomon', 'Sakuyamon'], ['Diamond Storm', 'Blazing Spheres', 'Amethyst Mandala'], 'ul_refraction', 'Templar Assassin', { role: 'Assassin', levels: ['Rookie', 'Champion', 'Mega'] }),
    H('tidehunter', 4, ['DEEP', 'METAL'], 'Vaccine', ['Betamon', 'Whamon', 'Plesiomon'], ['Electric Shock', 'Tidal Wave', 'Sorrow Blue'], 'ul_ravage', 'Tidehunter', { role: 'Guardian', hpMul: 1.2, mana: 90, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('viper', 4, ['DRAGON', 'TOXIC'], 'Virus', ['Monodramon', 'Airdramon', 'Gigadramon'], ['Crack Bite', 'Spinning Needle', 'Giga Heat'], 'ul_nethertoxin', 'Viper', { role: 'Sniper', range: 3, hpMul: 1.15 }),
    H('voidspirit', 4, ['DARK', 'SPIRIT'], 'Free', ['Lucemon', 'Lucemon FM', 'Lucemon SM'], ['Grand Cross', 'Paradise Lost', 'Divine Atonement'], 'ul_dissimilate', 'Void Spirit', { role: 'Duelist', levels: ['Rookie', 'Ultimate', 'Mega'] }),
    // ---- tier 5 (7) ----
    H('axe', 5, ['BEAST', 'ONI'], 'Data', ['ToyAgumon', 'Tyrannomon', 'MetalTyrannomon'], ['Block Punch', 'Fire Breath', 'Giga Blaster'], 'ul_passive', 'Axe', { role: 'Guardian', hpMul: 1.5, adMul: 0.8, kit: 'helix', levels: ['Rookie', 'Champion', 'Mega'] }),
    H('dragonknight', 5, ['PARTNER', 'DRAGON', 'KNIGHT'], 'Virus', ['Guilmon', 'Growlmon', 'Gallantmon'], ['Fire Ball', 'Exhaust Flame', 'Lightning Joust'], 'ul_breathefire', 'Dragon Knight', { role: 'Bruiser', levels: ['Rookie', 'Champion', 'Mega'] }),
    H('faceless', 5, ['DARK', 'SHINOBI'], 'Virus', ['Hagurumon', 'Clockmon', 'Chaosdromon'], ['Cog Crusher', 'Chrono Breaker', 'Chaos Cannon'], 'ul_chrono', 'Faceless Void', { role: 'Assassin', adMul: 1.15, hpMul: 1.25, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('keeper', 5, ['PARTNER', 'WIZARD'], 'Vaccine', ['Solarmon', 'Wizardmon', 'MagnaAngemon'], ['Shiny Ring', 'Thunder Cloud', 'Gate of Destiny'], 'ul_illuminate', 'Keeper of the Light', { role: 'Mystic', range: 4, levels: ['Rookie', 'Champion', 'Mega'] }),
    H('medusa', 5, ['DEEP', 'HUNTER'], 'Free', ['Lunamon', 'Lekismon', 'Crescemon'], ['Lunar Claw', 'Tear Arrow', 'Lunatic Dance'], 'ul_gaze', 'Medusa', { role: 'Sniper', range: 3, hpMul: 1.2 }),
    H('trollwarlord', 5, ['JUNGLE', 'METAL'], 'Virus', ['SnowGoblimon', 'Hyogamon', 'Vikemon'], ['Snow Goblin Volt', 'Throwing Ice', 'Viking Axe'], 'ul_passive', 'Troll Warlord', { role: 'Sniper', range: 3, adMul: 1.7, hpMul: 1.4, kit: 'fervor', levels: ['Rookie', 'Champion', 'Mega'] }),
    H('wraithking', 5, ['UNDEAD', 'BLADE'], 'Vaccine', ['Agumon', 'SkullGreymon', 'BlackWarGreymon'], ['Spirit Fire', 'Dark Shot', 'Terra Destroyer'], 'ul_wraithfire', 'Wraith King', { role: 'Bruiser', hpMul: 1.3, levels: ['Rookie', 'Ultimate', 'Mega'] }),

    // ---- Tamers (the Underlords): board units, never in the shop; the form follows the Tamer's rank ----
    H('tamer_anessa', 1, [], 'Data', ['Lopmon', 'Antylamon', 'Andiramon'], ['Blazing Ice', 'Mantra Chant', 'Deva Spear'], 'ul_passive', 'Anessix', { role: 'TamerSupport', shop: false, noSell: true, noItems: true, tamer: 'anessa', levels: ['Rank 1-2', 'Rank 3-4', 'Rank 5-6'], ai: { value: 900, carry: false, tank: 0 } }),
    H('tamer_hob', 1, [], 'Data', ['Candlemon', 'Volcamon', 'PileVolcamon'], ['Bonfire', 'Big Bang Voice', 'Volcanic Driver'], 'ul_passive', 'Hobgen', { role: 'TamerRanged', shop: false, noSell: true, noItems: true, tamer: 'hob', levels: ['Rank 1-2', 'Rank 3-4', 'Rank 5-6'], ai: { value: 900, carry: false, tank: 0 } }),
    H('tamer_jules', 1, [], 'Vaccine', ['Bearmon', 'Frigimon', 'Monzaemon'], ['Bear Fist', 'Sub-zero Ice Punch', 'Hearts Attack'], 'ul_passive', 'Jull', { role: 'TamerTank', shop: false, noSell: true, noItems: true, tamer: 'jules', levels: ['Rank 1-2', 'Rank 3-4', 'Rank 5-6'], ai: { value: 900, carry: false, tank: 0 } }),
    H('tamer_enno', 1, [], 'Virus', ['Chuumon', 'Sukamon', 'KingSukamon'], ['Cheese Bomb', 'Poop Blast', 'King Stamp'], 'ul_passive', 'Enno', { role: 'TamerSkirmish', shop: false, noSell: true, noItems: true, tamer: 'enno', levels: ['Rank 1-2', 'Rank 3-4', 'Rank 5-6'], ai: { value: 900, carry: false, tank: 0 } }),

    // ---- summons (made in combat by abilities, alliances, items and Tamers) ----
    H('s_archer', 2, [], 'Virus', ['DemiDevimon', 'DemiDevimon', 'DemiDevimon'], ['Demi Darts', 'Demi Darts', 'Demi Darts'], 'ul_passive', 'Anessix Demon Archer', { role: 'Sniper', shop: false, hpMul: 0.7 }),
    H('s_golem', 3, [], 'Virus', ['Golemon', 'Golemon', 'Golemon'], ['Golem Punch', 'Golem Punch', 'Golem Punch'], 'ul_passive', 'Anessix Demonic Golem', { role: 'Bruiser', shop: false, hpMul: 1.1 }),
    H('s_wolf', 1, [], 'Data', ['Gabumon', 'Garurumon', 'WereGarurumon'], ['Little Horn', 'Fox Fire', 'Wolf Claw'], 'ul_passive', 'Shaman wolf / Lycan wolf', { role: 'Striker', shop: false, hpMul: 0.6, adMul: 0.7 }),
    H('s_hatchling', 3, [], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'ul_passive', 'Shaman Wildwing Hatchling', { role: 'Sniper', shop: false, hpMul: 0.9 }),
    H('s_blackdragon', 5, [], 'Virus', ['Megadramon', 'Megadramon', 'Megadramon'], ['Big Mega Fire', 'Big Mega Fire', 'Big Mega Fire'], 'ul_passive', 'Shaman Black Dragon', { role: 'Bruiser', shop: false }),
    H('s_ward', 1, [], 'Virus', ['Kuramon', 'Kuramon', 'Kuramon'], ['Glare Eye', 'Glare Eye', 'Glare Eye'], 'ul_passive', 'Venomancer Plague Ward', { role: 'Sniper', shop: false, hpMul: 0.5, range: 3 }),
    H('s_treant', 1, [], 'Data', ['Tanemon', 'Tanemon', 'Tanemon'], ['Bubble Blow', 'Bubble Blow', 'Bubble Blow'], 'ul_passive', "Nature's Prophet Treant", { role: 'Bruiser', shop: false, hpMul: 0.8, adMul: 0.8 }),
    H('s_bear', 4, [], 'Vaccine', ['GrapLeomon', 'GrapLeomon', 'GrapLeomon'], ['Lion Uppercut', 'Lion Uppercut', 'Lion Uppercut'], 'ul_passive', 'Lone Druid Spirit Bear', { role: 'Bruiser', shop: false }),
    H('s_serpent', 1, [], 'Data', ['Seadramon', 'Seadramon', 'Seadramon'], ['Ice Blast', 'Ice Blast', 'Ice Blast'], 'ul_passive', 'Shadow Shaman Serpent Ward', { role: 'Sniper', shop: false, hpMul: 0.35, range: 2 }),
    H('s_bruiser', 3, [], 'Data', ['Starmon', 'Starmon', 'Starmon'], ['Meteor Shower', 'Meteor Shower', 'Meteor Shower'], 'ul_passive', 'Necronomicon Brill Bruiser', { role: 'Bruiser', shop: false, hpMul: 0.8 }),
    H('s_thunderhide', 5, [], 'Data', ['Triceramon', 'Triceramon', 'Triceramon'], ['Tri-Horn Attack', 'Tri-Horn Attack', 'Tri-Horn Attack'], 'ul_passive', 'Horn of the Alpha Thunderhide', { role: 'Bruiser', shop: false }),

    // ---- creeps (wild rounds) ----
    H('w_melee', 1, [], 'Virus', ['Numemon', 'Numemon', 'Numemon'], ['Poop Toss', 'Poop Toss', 'Poop Toss'], 'nuke', 'Melee Creep', { role: 'Striker', shop: false, hpMul: 0.75, adMul: 0.75 }),
    H('w_ranged', 1, [], 'Virus', ['Otamamon', 'Otamamon', 'Otamamon'], ['Lullaby Bubble', 'Lullaby Bubble', 'Lullaby Bubble'], 'nuke', 'Ranged Creep', { role: 'Sniper', shop: false, hpMul: 0.7, adMul: 0.7 }),
    H('w_mega', 2, [], 'Virus', ['Kuwagamon', 'Kuwagamon', 'Kuwagamon'], ['Scissor Claw', 'Scissor Claw', 'Scissor Claw'], 'slam', 'Mega Creep', { role: 'Bruiser', shop: false }),
    H('w_golem', 2, [], 'Data', ['MudFrigimon', 'MudFrigimon', 'MudFrigimon'], ['Great Weight', 'Great Weight', 'Great Weight'], 'slam', 'Golem (Mega-Warrior)', { role: 'Guardian', shop: false, boss: true }),
    H('w_troll', 3, [], 'Virus', ['Mojyamon', 'Mojyamon', 'Mojyamon'], ['Bone Boomerang', 'Bone Boomerang', 'Bone Boomerang'], 'nuke', 'Dark Troll (Mega-Troll)', { role: 'Duelist', shop: false, boss: true }),
    H('w_yearbeast', 5, [], 'Virus', ['Zhuqiaomon', 'Zhuqiaomon', 'Zhuqiaomon'], ['Crimson Blaze', 'Crimson Blaze', 'Crimson Blaze'], 'aoe', 'The Year Beast (Mega-Demon)', { role: 'Bruiser', shop: false, boss: true, hpMul: 1.6, adMul: 1.2, radius: 2 }),
    H('w_roshan', 5, [], 'Virus', ['Millenniummon', 'Millenniummon', 'Millenniummon'], ['Dimension Destroyer', 'Dimension Destroyer', 'Dimension Destroyer'], 'slam', 'Roshan', { role: 'Guardian', shop: false, boss: true, hpMul: 2.2, adMul: 1.6, radius: 2 }),
  ];

  // ---------- creep rounds ----------
  // Rounds 1, 2, 3 and every 5th round from 10 (Underlords beta loot rounds, also Classic mode). [line, star, col, row]
  const roundNo = (stage, round, per = 5) => (stage - 1) * per + round;
  const pveWave = (stage, round) => {
    const n = roundNo(stage, round);
    if (n === 1) return [['ul_w_melee', 0, 2, 4], ['ul_w_melee', 0, 4, 4]];
    if (n === 2) return [['ul_w_mega', 0, 3, 4], ['ul_w_ranged', 0, 2, 6], ['ul_w_ranged', 0, 4, 6]];
    if (n === 3) return [['ul_w_mega', 0, 2, 4], ['ul_w_mega', 0, 4, 4], ['ul_w_melee', 0, 1, 5], ['ul_w_melee', 0, 5, 5], ['ul_w_ranged', 0, 2, 6], ['ul_w_ranged', 0, 4, 6]];
    if (n === 10) return [['ul_w_golem', 1, 3, 4], ['ul_w_golem', 0, 2, 5], ['ul_w_golem', 0, 4, 5]];
    if (n === 15) return [['ul_s_wolf', 2, 3, 4], ['ul_s_wolf', 1, 1, 4], ['ul_s_wolf', 1, 5, 4], ['ul_s_wolf', 1, 2, 6], ['ul_s_wolf', 1, 4, 6]];
    if (n === 20) return [['ul_s_bear', 1, 2, 4], ['ul_s_bear', 1, 4, 4], ['ul_w_mega', 1, 3, 6]];
    if (n === 25) return [['ul_s_hatchling', 2, 3, 5], ['ul_s_hatchling', 1, 1, 6], ['ul_s_hatchling', 1, 5, 6], ['ul_w_mega', 2, 3, 4]];
    if (n === 30) return [['ul_s_thunderhide', 2, 3, 4], ['ul_s_thunderhide', 1, 1, 5], ['ul_s_thunderhide', 1, 5, 5]];
    if (n === 35) return [['ul_s_blackdragon', 2, 3, 4], ['ul_w_mega', 2, 1, 5], ['ul_w_mega', 2, 5, 5]];
    if (n === 40) return [['ul_w_troll', 2, 1, 4], ['ul_w_troll', 2, 3, 4], ['ul_w_troll', 2, 5, 4], ['ul_w_troll', 2, 2, 6], ['ul_w_troll', 2, 4, 6], ['ul_w_troll', 2, 3, 7]];
    if (n === 45) return [['ul_w_yearbeast', 2, 3, 5], ['ul_w_troll', 2, 2, 4], ['ul_w_troll', 2, 4, 4]];
    return [['ul_w_roshan', 2, 3, 5], ['ul_w_yearbeast', 2, 1, 4], ['ul_w_yearbeast', 2, 5, 4]];
  };
  // Creep loot: gold (the items come from the item rounds instead, as in Season One). Round 3 also drops a unit.
  const pveReward = (c, won) => {
    const n = roundNo(c.stage, c.round);
    if (!won) return [{ kind: 'blue', gold: n <= 3 ? 0 : 1 }];
    const out = [{ kind: 'blue', gold: n <= 3 ? 1 : 2 + Math.floor(n / 15) }];
    if (n === 3) {
      const lines = DT.SHOP_LINES.filter((l) => l.tier <= 2 && c.game.pool[l.id] > 0);
      if (lines.length) out.push({ kind: 'gold', unit: c.pick(lines).id });
    }
    return out;
  };

  // ---------- items: tiered, no recipes ----------
  // cat 'tiered' items go on a Digimon (up to 3); cat 'global' items affect the whole team. Hats add an alliance.
  // Behavior hooks are in underlords-mech.js; stats here are applied by the engine.
  const I = (id, tier, name, desc, o = {}) => ({ id: `ul_${id}`, tier, name, desc, cat: 'tiered', stats: {}, p: {}, ...o });
  const hat = (id, tier, name, trait) => I(id, tier, name, `The holder joins the ${traits[trait].name} alliance.`, { emblem: trait, p: { emblem: trait }, hat: true, tags: [], color: traits[trait].color, color2: '#3a2a1a' });
  const GL = (id, tier, name, desc, o = {}) => I(id, tier, name, desc, { cat: 'global', tags: ['global'], ...o });
  const items = [
    // tier 1
    I('chainmail', 1, 'Chainmail', '+20 armor.', { stats: { armor: 20 }, tags: ['tank'], color: '#a8b0bc' }),
    I('claymore', 1, 'Claymore', '+25% attack damage.', { stats: { ad: 0.25 }, tags: ['ad'], color: '#d0d4dc' }),
    I('gloves', 1, 'Gloves of Haste', '+20% attack speed.', { stats: { as: 0.2 }, tags: ['ad'], color: '#ffd23f' }),
    I('headdress', 1, 'Headdress', 'The holder and the allies left and right of it regenerate 20 HP per second.', { tags: ['tank'], color: '#7fd06a' }),
    I('hood', 1, 'Hood of Defiance', 'The holder and the allies left and right of it gain 25 magic resist.', { tags: ['tank'], color: '#5fb0a0' }),
    I('kaya', 1, 'Kaya', '+20 ability power.', { stats: { ap: 20 }, tags: ['ap'], color: '#b26bff' }),
    I('morbid', 1, 'Morbid Mask', 'Heals for 20% of damage dealt.', { stats: { omnivamp: 0.2 }, tags: ['ad'], color: '#c03a3a' }),
    I('talisman', 1, 'Talisman of Evasion', '20% chance to dodge basic attacks.', { stats: { dodge: 0.2 }, tags: ['tank'], color: '#9be7ff' }),
    I('vitality', 1, 'Vitality Booster', 'The holder and the allies left and right of it gain 200 max HP.', { tags: ['tank'], color: '#ff5d73' }),
    I('voidstone', 1, 'Void Stone', 'Gains 15 mana every 3 seconds.', { p: { manaRegen: 15 }, tags: ['ap'], color: '#4fb3ff' }),
    // tier 2
    I('arcane', 2, 'Arcane Boots', 'Every 10 seconds, if the holder has over half its mana, allies within 2 hexes gain 20 mana.', { tags: ['ap'], color: '#5fa0ff' }),
    I('armlet', 2, 'Armlet of Mordiggian', '+60% attack damage and +25 armor. The holder loses 3% of its max HP every second (it cannot fall below 1 HP from this).', { stats: { ad: 0.6, armor: 25 }, tags: ['ad'], color: '#d04a3a' }),
    I('blademail', 2, 'Blade Mail', '+15 armor. Returns 20% of damage taken to the attacker.', { stats: { armor: 15 }, tags: ['tank'], color: '#c0c6d0' }),
    I('blink', 2, 'Blink Dagger', 'At the start of combat and every 15 seconds, teleports next to the farthest enemy and gains 45 mana.', { tags: ['ap'], color: '#7a8cff' }),
    I('desolator', 2, 'Desolator', '+10% attack damage. Attacks cut the target\'s armor by 15 for 5 seconds.', { stats: { ad: 0.1 }, tags: ['ad'], color: '#a03050' }),
    I('lance', 2, 'Dragon Lance', 'Ranged holders only: +25% attack damage and 3 more range.', { stats: { ad: 0.25 }, tags: ['ad'], color: '#5ac0a0' }),
    I('halberd', 2, "Heaven's Halberd", '20% dodge chance. Every 10 seconds, an attack slows the target\'s attack speed by 60% for 3 seconds.', { stats: { dodge: 0.2 }, tags: ['tank'], color: '#ffcf4a' }),
    I('paladin', 2, 'Paladin Sword', '+20% attack damage. All healing on your team is 20% stronger.', { stats: { ad: 0.2 }, tags: ['ad'], color: '#f0e090' }),
    I('quelling', 2, 'Quelling Blade', '+20% attack damage. Deals double damage to summoned units.', { stats: { ad: 0.2 }, tags: ['ad'], color: '#b08a5a' }),
    I('stonecloak', 2, 'Stonehall Cloak', '+150 max HP. Each knockout gives 100 max HP and heals 100 for the rest of combat.', { stats: { hp: 150 }, tags: ['tank'], color: '#8a9a8a' }),
    I('stonepike', 2, 'Stonehall Pike', '+10% attack damage. Each knockout gives 10% attack damage for the rest of combat.', { stats: { ad: 0.1 }, tags: ['ad'], color: '#7a8a9a' }),
    I('vanguard', 2, 'Vanguard', '+350 max HP. 50% chance to block 40 damage from basic attacks.', { stats: { hp: 350 }, tags: ['tank'], color: '#ff8a5a' }),
    // tier 3
    I('battlefury', 3, 'Battle Fury', 'Melee holders only: +35% attack damage, and attacks also hit a second enemy for 50%.', { stats: { ad: 0.35 }, p: { split: 0.5 }, tags: ['ad'], color: '#ff7a45' }),
    I('craggy', 3, 'Craggy Coat', '+45 armor, but 25% less attack speed.', { stats: { armor: 45, as: -0.25 }, tags: ['tank'], color: '#8a7a6a' }),
    I('euls', 3, "Eul's Scepter", 'Gains 20 mana every 3 seconds. Every 10 seconds, the next enemy that attacks the holder is lifted into the air for 2.5 seconds.', { p: { manaRegen: 20 }, tags: ['ap'], color: '#bff0ff' }),
    I('madness', 3, 'Mask of Madness', '+50% attack speed and heals for 15% of damage dealt, but the holder cannot cast.', { stats: { as: 0.5, omnivamp: 0.15 }, tags: ['ad'], color: '#ff4a3a' }),
    I('necro', 3, 'Necronomicon', 'Each cast summons a Brill Bruiser (up to 2 at once).', { tags: ['ap'], color: '#6a3a9a' }),
    I('octarine', 3, 'Octarine Essence', 'Max mana -30%.', { tags: ['ap'], color: '#3ad0ff' }),
    I('pipe', 3, 'Pipe of Insight', '+40 magic resist. After 3 seconds, allies within 2 hexes gain a 250 shield.', { stats: { mr: 40 }, tags: ['tank'], color: '#5fe0c0' }),
    I('silveredge', 3, 'Silver Edge', '+25% attack speed and +300 max HP. Every 5 seconds, an attack makes the target deal 25% less damage for 3 seconds.', { stats: { as: 0.25, hp: 300 }, tags: ['ad'], color: '#d0e0ff' }),
    I('basher', 3, 'Skull Basher', '+20% attack damage. Attacks have a 25% chance to deal 100 bonus damage and stun for 1.5 seconds (2 second cooldown).', { stats: { ad: 0.2 }, tags: ['ad'], color: '#c09a6a' }),
    I('shako', 3, 'Witless Shako', '+1000 max HP, but the holder needs twice as much mana to cast.', { stats: { hp: 1000 }, tags: ['tank'], color: '#c0a060' }),
    hat('hat_antlers', 3, 'Crown of Antlers', 'HUNTER'),
    hat('hat_fedora', 3, "Leg Breaker's Fedora", 'ONI'),
    hat('hat_pirate', 3, 'Pirate Hat', 'BLADE'),
    hat('hat_circlet', 3, 'Ristul Circlet', 'DEMON'),
    // tier 4
    I('bkb', 4, 'Black King Bar', '+30% attack damage. Immune to stuns for the first 10 seconds.', { stats: { ad: 0.3 }, p: { ccImmune: 10 }, tags: ['ad', 'tank'], color: '#3a3a3a' }),
    I('butterfly', 4, 'Butterfly', '+25% attack damage, +30% attack speed and 30% dodge chance.', { stats: { ad: 0.25, as: 0.3, dodge: 0.3 }, tags: ['ad'], color: '#7ad0ff' }),
    I('dagon', 4, 'Dagon', 'Every 12 seconds, deals 600 magic damage to a random enemy at or below 50% HP.', { tags: ['ap'], color: '#ff5a5a' }),
    I('diffusal', 4, 'Diffusal Blade', 'Attacks burn 15 mana and deal 30 bonus damage.', { stats: { ad: 0.1 }, tags: ['ad'], color: '#9a6aff' }),
    I('skadi', 4, 'Eye of Skadi', '+550 max HP. Attacks slow the target\'s attack speed by 40% for 2.5 seconds.', { stats: { hp: 550 }, tags: ['ad', 'tank'], color: '#7fdcff' }),
    I('kaden', 4, "Kaden's Blade", 'Attacks deal half their normal damage, plus 6% of the target\'s max HP as true damage.', { tags: ['ad'], color: '#c0ff7a' }),
    I('maelstrom', 4, 'Maelstrom', 'Attacks have a 25% chance to send chain lightning to 4 enemies for 110 magic damage (more against summons).', { stats: { as: 0.1 }, tags: ['ad'], color: '#5ab0ff' }),
    I('mekansm', 4, 'Mekansm', 'The first time the holder drops below 50% HP, allies within 3 hexes heal 500.', { stats: { hp: 200 }, tags: ['tank'], color: '#7dff9a' }),
    I('mkb', 4, 'Monkey King Bar', '+50% attack damage and +15% critical strike chance.', { stats: { ad: 0.5, crit: 0.15 }, tags: ['ad'], color: '#ffb02e' }),
    I('moonshard', 4, 'Moon Shard', '+60% attack speed.', { stats: { as: 0.6 }, tags: ['ad'], color: '#c8d8ff' }),
    I('refresher', 4, 'Refresher Orb', 'After its first cast, the holder\'s mana is refilled once.', { stats: { ap: 10 }, tags: ['ap'], color: '#4fe0a0' }),
    I('vyse', 4, 'Scythe of Vyse', 'Gains 20 mana every 3 seconds. Every 15 seconds, the next enemy that damages the holder is hexed (stunned) for 2.5 seconds.', { p: { manaRegen: 20 }, tags: ['ap', 'tank'], color: '#5aa0ff' }),
    hat('hat_ogre', 4, 'Ogre Cap', 'JUNGLE'),
    hat('hat_mantle', 4, 'Mantle of the Flayed Twins', 'NIGHT'),
    // tier 5
    I('bloodthorn', 5, 'Bloodthorn', '+45% attack damage. Every 10 seconds, silences the enemy with the most mana for 4 seconds; attacks on silenced enemies deal 40% more damage.', { stats: { ad: 0.45 }, tags: ['ad'], color: '#c0203a' }),
    I('rapier', 5, 'Divine Rapier', '+150% attack damage. If you lose a round against a player while it is on your board, that player takes it.', { stats: { ad: 1.5 }, tags: ['ad'], color: '#ffe680' }),
    I('tarrasque', 5, 'Heart of Tarrasque', '+1200 max HP. Regenerates 2% of max HP per second.', { stats: { hp: 1200 }, p: { regen: 0.02 }, tags: ['tank'], color: '#ff3a5a' }),
    I('horn', 5, 'Horn of the Alpha', '2 seconds into combat, a Thunderhide joins your team.', { tags: ['tank'], color: '#c8a060' }),
    I('radiance', 5, 'Radiance', 'Every second, deals 50 magic damage to enemies within 2 hexes; each has a 17% chance to miss its next attack.', { stats: { hp: 300 }, tags: ['tank'], color: '#ffd84a' }),
    I('satanic', 5, 'Satanic', '+750 max HP. Below 50% HP, heals for 150% of damage dealt for 3 seconds (10 second cooldown).', { stats: { hp: 750, ad: 0.15 }, tags: ['ad', 'tank'], color: '#a01020' }),
    I('shiva', 5, "Shiva's Guard", '+30 armor. 2 seconds into combat, deals 250 magic damage to enemies within 3 hexes and slows their attack speed by 40% for 4 seconds.', { stats: { armor: 30 }, tags: ['tank'], color: '#9fe8ff' }),
    I('vesture', 5, 'Vesture of the Tyrant', 'Allies within 1 hex at the start of combat survive for 6 more seconds after a killing blow (once each).', { stats: { hp: 300 }, tags: ['tank'], color: '#6a2a8a' }),
    I('vladmir', 5, "Vladmir's Offering", 'Allies within 2 hexes at the start of combat deal 8% more damage, gain 15 armor and heal for 10% of damage dealt.', { stats: { hp: 200 }, tags: ['tank', 'ap'], color: '#c04a4a' }),
    // global items (beta era): they affect your whole team or your economy, not one Digimon
    GL('g_riches', 1, 'Embarrassment of Riches', 'Your item picks show 4 choices instead of 3.', { color: '#ffd84a' }),
    GL('g_silver', 2, 'Silver Lining', 'Each win stores a charge (up to 3). Each loss with a charge spends it for 2 gold.', { color: '#c8d0e0' }),
    GL('g_aegis', 2, 'Aegis of the Immortal', 'The first time a loss would knock you out, you take no damage instead. Then the Aegis breaks.', { color: '#ffcf4a' }),
    GL('g_stone', 2, 'Summoning Stone', 'Your summoned units gain 30% max HP and 30% attack speed.', { color: '#5fd0ff' }),
    GL('g_vicious', 3, 'Vicious Intent', 'When you beat a player, they lose 1 more HP for each of your surviving units.', { color: '#ff4a4a' }),
    GL('g_ristul', 3, 'Dawning of Ristul', 'In your combats, all healing on both teams is cut by 40%.', { color: '#8a2a4a' }),
    GL('g_assault', 3, 'Coordinated Assault', 'Your Jungle Troopers give 25% attack speed to allies next to them.', { color: '#3f9a3a' }),
    GL('g_chivalry', 3, 'Age of Chivalry', 'Your non-Knight units take 10% less damage for each allied Knight next to them.', { color: '#8fb4d9' }),
    GL('g_recruiter', 4, 'Recruiter', 'Gain 2 free rerolls every round.', { color: '#4fb3ff' }),
    GL('g_smuggler', 4, 'Smuggler', 'Your item picks come from one tier higher.', { color: '#a07a4a' }),
    GL('g_roster', 5, 'Expanded Roster', '+1 team size.', { color: '#7dff9a' }),
    GL('g_criminal', 5, 'A Higher Class of Criminal', 'Your shop rolls as if you were one level higher.', { color: '#e58bff' }),
  ];

  // ---------- Tamers (Underlords) ----------
  // Each Tamer is offered at round 10 with one of its two builds at random (kind 'power'). Stats per rank 1-6.
  const TAMERS = {
    anessa: { name: 'Anessa', title: 'Keeper of Secrets', line: 'ul_tamer_anessa', src: 'Anessix, Mistress of Secrets',
      hp: [1100, 1400, 1700, 2000, 2300, 2600], ad: [0, 0, 0, 0, 0, 0], armor: [15, 20, 20, 20, 25, 25], mr: 20,
      core: 'A Demon Archer fights beside her (it falls when she does). She does not attack.' },
    hob: { name: 'Hob', title: 'Fire Bug', line: 'ul_tamer_hob', src: 'Hobgen, Arson Aficionado',
      hp: [1300, 1650, 2000, 2350, 2700, 3050], ad: [70, 80, 90, 100, 110, 120], armor: [25, 25, 30, 30, 35, 35], mr: 20,
      core: 'His attacks set enemies on fire.' },
    jules: { name: 'Jules', title: 'Frost Brewer', line: 'ul_tamer_jules', src: 'Jull, Icewrack Bootlegger',
      hp: [1650, 2050, 2350, 2750, 3050, 3450], ad: [100, 108, 116, 124, 132, 140], armor: [40, 40, 55, 55, 70, 70], mr: 30,
      core: 'Gains 10 armor and magic resist for each unit next to him, friend or enemy.' },
    enno: { name: 'Enno', title: 'Scavenger King', line: 'ul_tamer_enno', src: 'Enno, Scavenger King',
      hp: [1200, 1450, 1650, 1900, 2100, 2350], ad: [70, 78, 86, 94, 102, 110], armor: [25, 25, 25, 25, 25, 25], mr: 20,
      core: 'Escape Artist: the first time he drops below 50% HP (and 25% from rank 4) he vanishes in smoke, shakes off stuns and burns, lands elsewhere and stuns enemies next to him.' },
  };
  const PW = (tamer, build, name, desc, tags) => {
    const Tm = TAMERS[tamer];
    return { id: `ul_${tamer}_${build}`, kind: 'power', tier: 2, tamer, build, name: `${Tm.name}: ${name}`, desc: `${Tm.core} ${desc} Ranks up every 5 rounds.`, tags };
  };
  const powers = [
    PW('anessa', 'dark', 'Dark Support', 'Every 8 s your team gains 40-55% attack damage for 6 s. Every 8 s she marks the nearest enemy (-30 armor, your team focuses it); if it falls while marked, it rises to fight for you.', ['combat']),
    PW('anessa', 'heal', 'Healing Support', 'Every 8 s she gives up 5% of her HP to heal your whole team at once and over 6 s. Every 15 s a Demonic Golem crashes down by the farthest enemy and stuns everything within 2 hexes.', ['combat']),
    PW('hob', 'fire', 'Fire Starter', 'Every 12 s a fireball splits 640-1560 magic damage among the target and enemies next to it. Every 13 s his buddies throw 8 bombs.', ['combat']),
    PW('hob', 'crazy', 'Fire Support', 'Every 12 s a fireball splits 420-1120 damage among the target and its neighbors and stuns them. Every 10 s he goes crazy: for 6 s he can hit anything on the board and your team gains 30-90% attack speed.', ['combat']),
    PW('jules', 'brawl', 'Aggressive Tank', 'Every 8 s he heals 10% of his HP, draws nearby enemies to him and, for 5 s, takes 50% less damage, attacks twice as fast and heals from his hits. Every 12 s barrels roll in: 4 allies gain attack speed and 4 enemies take 25% more damage for the rest of combat.', ['combat']),
    PW('jules', 'bar', 'Healing Tank', 'Every 10 s he takes 50% less damage for 5 s, and allies next to him heal for the damage he takes. Every 10 s Happy Hour: for 7 s your team takes half of each hit now and the rest over 10 s.', ['combat']),
    PW('enno', 'balm', "Healin' N' Stealin'", 'Every 3 s he throws poison darts at 3-5 enemies within 3 hexes, and each hit heals your most hurt allies a little. At the start of combat he steals the stats of the enemy\'s lowest-tier item for one of your Digimon.', ['combat']),
    PW('enno', 'rabid', 'Rabid Furball', 'Every 3 s poison darts hit 3-5 enemies within 3 hexes for damage per poison stack. Every 10 s All Out Attack: every enemy gains 3 poison stacks, takes damage and is stunned for 1.1 s.', ['combat']),
  ];

  // ---------- rules: Underlords Standard (final patch) ----------
  // Conflicts in the research and what was chosen: base income 5 (wiki; patch notes said 7, never confirmed live),
  // XP curve 1/1/3/5/10/20/25/35/50 (the cumulative 2019-10-24 curve), rank 6 at round 35 (rank every 5 rounds from
  // round 10), 1 gold per PvP win, interest 1 per 10 gold up to 3.
  const ITEM_ROUNDS = [1, 4, 8, 12, 16, 20, 24, 28, 32, 36, 40];
  const label = (n, per = 5) => ({ stage: Math.ceil(n / per), round: ((n - 1) % per) + 1 });
  const ITEM_ODDS = { 1: [100], 4: [20, 80], 8: [0, 100], 12: [0, 30, 70], 16: [0, 0, 100], 20: [0, 0, 20, 80], 24: [0, 0, 0, 70, 30],
    28: [0, 0, 0, 50, 50], 32: [0, 0, 0, 30, 70], 36: [0, 0, 0, 0, 100], 40: [0, 0, 0, 0, 100] };
  const mainTier = (odds) => odds.indexOf(Math.max(...odds)) + 1;
  const isCreep = (n) => n <= 3 || (n >= 10 && n % 5 === 0);
  const unitPower = (u) => (u.line.tier || 1) + 2 * u.stage;
  const rules = {
    startHp: 100,
    startGold: 5,
    planTime: 30,
    itemCap: 10,
    rerollCost: 2,
    winGold: 1,
    winGoldPvE: 0,
    shopSize: 5,
    poolSize: [0, 30, 20, 18, 12, 10],
    shopOdds: [null,
      [80, 20, 0, 0, 0], [70, 30, 0, 0, 0], [55, 35, 10, 0, 0], [45, 40, 15, 0, 0], [35, 40, 25, 0, 0],
      [25, 35, 35, 5, 0], [20, 30, 40, 10, 0], [18, 24, 35, 20, 3], [15, 21, 30, 28, 6], [12, 18, 28, 32, 10]],
    levelByStage: null,
    xp: { table: [0, 1, 1, 3, 5, 10, 20, 25, 35, 50], perRound: 1, buyCost: 5, buyXp: 5, startLevel: 3, maxLevel: 10 },
    incomeByStage: null,
    baseIncome: 5,
    interest: { per: 10, max: 3 },
    streak: [[3, 1], [5, 2], [7, 3], [8, 4]], // loss streaks stop at 2 gold (underlords-mech.js onIncome)
    // loser takes: round damage (+1 every 10 rounds) + per surviving enemy floor(1 + power / 3), power = tier + 2 x star;
    // a surviving Tamer deals 1 (2 from round 30); summons deal nothing.
    lossDamage: (stage) => Math.floor(((stage - 1) * 5 + 3) / 10),
    unitDamage: (u, stage) => (u.line.tamer ? (stage >= 7 ? 2 : 1) : Math.floor(1 + unitPower(u) / 3)),
    pveLossDamage: null,
    roundsInStage: () => 5,
    roundType: (stage, round) => (isCreep(roundNo(stage, round)) ? 'pve' : 'pvp'),
    armoryStages: [],
    augmentStages: [],
    offers: [
      ...ITEM_ROUNDS.map((n) => ({ kind: 'itemTier', ...label(n), tier: mainTier(ITEM_ODDS[n]), count: 3, rerolls: 1 })),
      { kind: 'ul_tamer', ...label(10), rerolls: 0 },
    ],
    powerRanks: [15, 20, 25, 30, 35].map((n) => { const l = label(n); return `${l.stage}-${l.round}`; }),
    copies: 3,
    maxStage: 2,
    // set data read by underlords-mech.js
    ulPerStage: 5,
    ulItemOdds: ITEM_ODDS,
    ulGoldCap: 100,
    ulLossStreakCap: 2,
    ulStreakReset: 8,
    // Pace overrides (rules.hyper / rules.normal, see docs/sets/FORMAT.md). Normal is the native Underlords economy
    // above. Hyper is Hyper Roll on the same rounds: level set by stage, fixed income, no interest or streaks, cheap
    // rerolls, 20 HP with flat loss damage.
    normal: {},
    hyper: {
      startHp: 20,
      startGold: 5,
      rerollCost: 1,
      levelByStage: [0, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10],
      xp: null,
      incomeByStage: [0, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10, 10],
      interest: null,
      streak: null,
      lossDamage: (stage) => (stage <= 2 ? 2 : stage <= 4 ? 3 : stage <= 6 ? 4 : 5),
      unitDamage: null,
      pveLossDamage: (stage) => (stage === 1 ? 1 : null),
    },
  };

  const codexRules = (R, pace) => (R && R.ulKnockout ? [
    { id: 'ul_ko', title: 'Knockout', html: '<p>4 lives: each lost round costs one. Two copies digivolve. You start at level 5 and level up every 2 rounds (XP cannot be bought). Pick a starter crew of 5 at 1-1 and a Tamer at 1-2; the Tamer ranks up every 2 rounds. An item pick every other round. Base gold starts at 6 and rises each round; no interest or streak gold. Each stage has 2 rounds.</p>' },
  ] : pace === 'hyper' ? [
    { id: 'ul_rounds', title: 'Rounds', html: '<p>Each stage has 5 rounds, so round 10 is 2-5. Rounds 1, 2, 3 and every 5th round from 10 are creep rounds; the rest are fights against players.</p>' },
    { id: 'ul_econ', title: 'Hyper Roll', html: '<p>20 HP. Your level is set by the stage (3 to 10). Fixed income of 5 to 10 gold by stage, no interest or streaks, 1 gold per player win. Rerolls cost 1. A loss costs a flat 2 to 5 HP by stage.</p>' },
    { id: 'ul_items', title: 'Items', html: '<p>Items have no recipes. On rounds 1, 4, 8, 12 ... 40 you pick 1 of 3 items from one tier (reroll once). Hats add an alliance. Global items affect your whole team or economy.</p>' },
    { id: 'ul_tamers', title: 'Tamers', html: '<p>At round 10 you pick a Tamer: Anessa, Hob, Jules or Enno, each offered with one of two builds. The Tamer stands on your board (it does not use a team slot), uses its abilities on cooldowns, cannot hold items, and ranks up every 5 rounds.</p>' },
  ] : [
    { id: 'ul_rounds', title: 'Rounds', html: '<p>Each stage has 5 rounds, so round 10 is 2-5 and round 30 is 6-5. Rounds 1, 2, 3 and every 5th round from 10 are creep rounds; the rest are fights against players.</p>' },
    { id: 'ul_econ', title: 'Economy', html: '<p>5 gold per round, 1 gold per player win, 1 interest per 10 gold (up to 3), streak gold for 3+ wins or losses (losses up to 2). Rerolls cost 2. 5 gold buys 5 XP; you also get 1 XP per round. You start at level 3 with 5 gold and 100 HP.</p>' },
    { id: 'ul_dmg', title: 'Damage', html: '<p>The loser takes 1 per 10 rounds played, plus for each surviving enemy Digimon 1 + (cost + 2 per star level) / 3, rounded down. A surviving Tamer deals 1 (2 from round 31). Summons deal nothing.</p>' },
    { id: 'ul_items', title: 'Items', html: '<p>Items have no recipes. On rounds 1, 4, 8, 12 ... 40 you pick 1 of 3 items from one tier (reroll once). The tier rises through the game. Hats add an alliance. Global items affect your whole team or economy.</p>' },
    { id: 'ul_tamers', title: 'Tamers', html: '<p>At round 10 you pick a Tamer: Anessa, Hob, Jules or Enno, each offered with one of two builds. The Tamer stands on your board (it does not use a team slot), uses its abilities on cooldowns, cannot hold items, and ranks up every 5 rounds (rank 6 at round 35).</p>' },
  ]);

  const base = {
    mode: 'underlords',
    colors: { main: '#c0392b', accent: '#ffcf4a' },
    source: 'Dota Underlords, Season One and the final patch (2020-11-19), with beta-era creep rounds and global items. docs/inspo/underlords/, docs/sets/underlords.md',
    roles,
    traits,
    lines,
    pve: { wave: pveWave, reward: pveReward },
    items: { system: 'tiered', list: items },
    augments: [],
    powers,
    rules,
    codexRules,
    tamers: ['Grek', "O'nia", 'Quiet', 'Turath', 'Wren', 'Carl', 'Ricky'],
    ulTamers: TAMERS,
  };

  DT.Sets.register({
    ...base,
    id: 'underlords',
    name: 'Undermons',
    order: 9,
    desc: 'Undermons: 26 alliances, tiered items with no recipes, creep rounds, and a Tamer on your board from round 10 who ranks up every 5 rounds.',
  });

  // ---------- Underlords Knockout ----------
  // Short mode: 4 lives, 2 copies combine, start at level 5 and level up every 2 rounds, a starter crew of 5, an item
  // every odd round, Tamer at round 2 ranking up every 2 rounds, no interest or streaks, base gold 6 rising (+1/+2).
  // Each stage has 2 rounds here (round 10 = 5-2).
  const KO_ODDS = (n) => (n <= 3 ? [100] : n <= 5 ? [0, 100] : n <= 9 ? [0, 0, 100] : n <= 13 ? [0, 0, 0, 100] : [0, 0, 0, 0, 100]);
  const koOdds = {};
  for (let n = 3; n <= 41; n += 2) koOdds[n] = KO_ODDS(n);
  const koLabel = (n) => label(n, 2);
  const koRules = {
    ...rules,
    startHp: 4,
    startGold: 6,
    winGold: 0,
    poolSize: [0, 30, 20, 18, 12, 10],
    shopOdds: [null, [40, 35, 25, 0, 0], [40, 35, 25, 0, 0], [40, 35, 25, 0, 0], [40, 35, 25, 0, 0], [40, 35, 25, 0, 0],
      [35, 30, 30, 5, 0], [25, 30, 35, 10, 0], [22, 27, 35, 15, 1], [22, 25, 30, 20, 3], [15, 21, 28, 30, 6]],
    xp: { table: [0, 2, 2, 2, 2, 1, 2, 2, 2, 2], perRound: 1, buyCost: 999, buyXp: 0, startLevel: 5, maxLevel: 10 },
    baseIncome: 6,
    interest: null,
    streak: null,
    lossDamage: () => 1,
    unitDamage: null,
    pveLossDamage: null,
    roundsInStage: () => 2,
    roundType: () => 'pvp',
    offers: [
      { kind: 'ul_crew', stage: 1, round: 1, rerolls: 1 },
      { kind: 'ul_tamer', stage: 1, round: 2, rerolls: 0 },
      ...Object.keys(koOdds).map(Number).map((n) => ({ kind: 'itemTier', ...koLabel(n), tier: mainTier(koOdds[n]), count: 3, rerolls: 1 })),
    ],
    powerRanks: [4, 6, 8, 10, 12].map((n) => { const l = koLabel(n); return `${l.stage}-${l.round}`; }),
    copies: 2,
    ulPerStage: 2,
    ulItemOdds: koOdds,
    ulKnockout: true,
  };
  // Knockout is already short (4 lives), so both paces play the same Knockout rules.
  koRules.normal = {};
  koRules.hyper = {};
  for (const k of DT.PACE_KEYS || []) if (k in koRules) koRules.hyper[k] = koRules[k];
  DT.Sets.register({
    ...base,
    id: 'underlords_ko',
    name: 'Undermons Knockout',
    order: 10,
    chaos: false,
    desc: 'Undermons Knockout: 4 lives, 2 copies digivolve, a starter crew, an item every other round and a Tamer from round 2. About 10 minutes.',
    // own line objects: the engine stamps each line with its set id
    lines: lines.map((l) => ({ ...l })),
    traits: Object.fromEntries(Object.entries(traits).map(([k, v]) => [k, { ...v }])),
    powers: powers.map((p) => ({ ...p })),
    rules: koRules,
  });
})(typeof window !== 'undefined' ? window : globalThis);
