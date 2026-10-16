// Fates: TFT Set 4 Fates plus Set 4.5 Festival of Beasts, on Digimon partner lines. 60 shop lines (13/13/13/11/10),
// 28 traits, classic components and recipes with Digi-Egg crests, 10 Digi-Forge artifacts (Ornn's items), consumables,
// 30 augments, Hyper Roll economy and the Partner mechanic (TFT Chosen). Behavior is in fates-mech.js, art and move
// looks in fates-art.js, design notes and balance results in docs/sets/fates.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;

  // ---------- traits ----------
  // lv[i] is the value at breakpoint i. Extra per-breakpoint arrays are read with c.val(key) in fates-mech.js.
  const T = {
    // ---- origins ----
    OCEAN: { name: 'Dark Ocean', kind: 'origin', color: '#3b5bdb', bp: [3, 6, 9], lv: [0, 1, 2], hpMul: [0.5, 0.7, 1.1], crestSymbol: 'moon',
      desc: "The first time your team has lost half of its total HP, a Soulmon from the Dark Ocean rises next to the biggest group of enemies, slams them (stunned for 1.5s) and fights for you. It gains 8% HP and damage for each form stage of your Dark Ocean Digimon.",
      fmt: (v, i) => ['Soulmon (Rookie form)', 'Soulmon (Champion form)', 'Soulmon (final form)'][i] },
    CELES: { name: 'Celestial', kind: 'origin', color: '#ffe27a', bp: [2, 4, 6], lv: [3, 6, 10], amp: [0.35, 0.42, 0.5], crestSymbol: 'cross',
      desc: 'After 6 basic attacks, or the first time it drops below 50% HP, a Celestial Digimon shakes off stuns and ascends: it takes less damage and deals more damage for a few seconds.',
      fmt: (v, i) => `ascend ${v}s, -${pct(T.CELES.amp[i])} damage taken, +${pct(T.CELES.amp[i])} damage` },
    NIGHT: { name: 'Nightfall', kind: 'origin', color: '#8a5cff', bp: [2, 4], lv: [12, 15], own: [0, 35], crestSymbol: 'moon',
      desc: 'Creatures of the night power up every ability. All allies gain ability power; with 4, Nightfall Digimon gain more.',
      fmt: (v, i) => (i ? `all allies +${v} AP, Nightfall +${T.NIGHT.own[i]} more` : `all allies +${v} AP`) },
    FOREST: { name: 'Ancient Forest', kind: 'origin', color: '#4caf50', bp: [3, 6, 9], lv: [4, 9, 16], grow: [2, 3, 7], crestSymbol: 'star6',
      desc: 'Every 2 seconds, Ancient Forest Digimon grow: they gain armor, magic resist, attack damage and ability power, up to 5 times.',
      fmt: (v, i) => `+${v} armor/MR, +${T.FOREST.grow[i]} AD/AP per stack` },
    PILGRIM: { name: 'Pilgrims', kind: 'origin', color: '#7fd6ff', bp: [2, 4, 6], lv: [0.3, 0.55, 0.85], crestSymbol: 'star6',
      desc: 'Wandering monks and sages: Pilgrims gain more mana from attacks and from taking damage.',
      fmt: (v) => `+${pct(v)} mana gained` },
    RONIN: { name: 'Ronin', kind: 'origin', color: '#c9a66b', bp: [1, 2], lv: [0.5, 0.5], vamp: [0, 0.5], crestSymbol: 'sword',
      desc: 'A Ronin with no ally next to it when combat starts gains a shield. With 2, it also heals for half of the damage it deals.',
      fmt: (v, i) => (i ? `${pct(v)} HP shield alone, 50% omnivamp` : `${pct(v)} HP shield alone`) },
    TREASURE: { name: 'Treasure Hunters', kind: 'origin', color: '#ffc93a', bp: [3, 6], lv: [0.3, 0.4], crestSymbol: 'star5',
      desc: 'Treasure Hunters dodge some basic attacks. Each PvP loss with the trait active adds a lucky token. Your next PvP win cashes them in: gold, plus a component at 3 tokens and a completed item at 5. With 6, every PvP win also pays 3 gold.',
      fmt: (v, i) => `${pct(v)} dodge, tokens pay out on wins${i ? ', +3 gold per win' : ''}` },
    LUNAR: { name: 'Lunar', kind: 'origin', color: '#b7c6ff', bp: [3, 5], lv: [1, 2], crestSymbol: 'moon',
      desc: 'Under the full moon, the lowest-stage Lunar Digimon digivolves one stage for the combat (ties: most items). With 5, two of them do.',
      fmt: (v) => `${v} Lunar Digimon digivolve${v > 1 ? '' : 's'} for the combat` },
    NINJA: { name: 'Shinobi', kind: 'origin', color: '#5a5a7a', bp: [1, 4], lv: [0.35, 0.6], crestSymbol: 'dagger',
      desc: 'Only active with exactly 1 or exactly 4 different Shinobi. The Shinobi gain attack damage and ability power.',
      fmt: (v, i) => (i ? `all 4 gain +${pct(v)} AD and +${Math.round(v * 100)} AP` : `the one Shinobi gains +${pct(v)} AD and +${Math.round(v * 100)} AP`) },
    SPIRIT: { name: 'Onmyo Spirits', kind: 'origin', color: '#ff8fc8', bp: [2, 4], lv: [0.2, 0.42], crestSymbol: 'star5',
      desc: "The first time an Onmyo Spirit casts in a combat, all allies gain attack speed equal to a share of its max mana (a 70-mana spirit with 2 gives +24.5%).",
      fmt: (v) => `allies +${pct(v)} of the caster's max mana as attack speed` },
    DEVA: { name: 'Deva Generals', kind: 'origin', color: '#ff6a3d', bp: [3, 6, 9], lv: [220, 450, 700], ap: [20, 40, 70], crestSymbol: 'shield',
      desc: 'Generals of the Holy Beasts gain max HP and ability power. The bonus grows by 10% for each PvP fight you win with the trait active, up to 5 wins.',
      fmt: (v, i) => `+${v} HP and +${T.DEVA.ap[i]} AP, +10% per PvP win (max 5)` },
    // ---- unique (one 5-cost line each) ----
    SOVEREIGN: { name: 'Sovereign', kind: 'unique', color: '#ffd84a', bp: [1], lv: [0.4],
      desc: 'Imperialdramon deploys with two stone guards beside it. They do not move or attack, have 40% of its max HP, and crumble when it is knocked out.',
      fmt: () => 'two guards with 40% of its max HP' },
    BANCHO: { name: 'Bancho', kind: 'unique', color: '#e05a2a', bp: [1], lv: [0.15],
      desc: 'The first time the Bancho drops below 40% HP, it stops to do push-ups for up to 3 seconds and cannot be hurt. Each push-up heals 8% max HP and gives attack speed. Then it comes back pumped up and deals 20% more damage.',
      fmt: () => 'push-up break below 40% HP' },
    FALLEN: { name: 'Fallen Choice', kind: 'unique', color: '#9c6bd6', bp: [1], lv: [1],
      desc: 'After this line has fought 3 combats, you get two items: Holy Spirit and Dark Spirit. Using one on the line picks its path for the rest of the game (the other one vanishes). Holy: its damage heals it for 40% of damage dealt. Dark: it deals 40% more damage for the first 10 seconds.',
      fmt: () => 'choose a path after 3 combats' },
    FORGE: { name: 'Digi-Forge', kind: 'unique', color: '#ff8a2a', bp: [1], lv: [4],
      desc: 'After each combat it fights, the Digi-Forge line makes forge progress (1 per form stage). Every 4 points it forges a random artifact onto your item bench, up to 4 per game.',
      fmt: (v) => `an artifact every ${v} forge points` },
    STYLE: { name: 'Showtime', kind: 'unique', color: '#ff4fa0', bp: [1], lv: [0.1],
      desc: 'Every second basic attack, the Showtime Digimon shields itself for 10% max HP and fires a second shot. Every 2 double shots it casts its ability.',
      fmt: () => 'double shots and shields, casts on style' },
    // ---- classes ----
    ADEPT: { name: 'Sage', kind: 'class', color: '#a0e6d0', bp: [2, 3], lv: [1.2, 2.5], crestSymbol: 'star6',
      desc: 'Sages calm the start of every fight: all enemies lose 50% attack speed for a few seconds.',
      fmt: (v) => `enemies -50% attack speed for ${v}s` },
    ASSASSIN: { name: 'Assassin', kind: 'class', color: '#ff4f7b', bp: [2, 4], lv: [0.15, 0.3], critDmg: [0.3, 0.6], crestSymbol: 'dagger',
      desc: 'Assassins leap to the enemy backline when combat starts. They gain critical strike chance and damage, and their abilities can critically strike.',
      fmt: (v, i) => `+${pct(v)} crit, +${pct(T.ASSASSIN.critDmg[i])} crit damage` },
    BRAWLER: { name: 'Brawler', kind: 'class', color: '#d9773a', bp: [2, 4, 6], lv: [300, 650, 1000], crestSymbol: 'shield',
      desc: 'Brawlers gain max HP.', fmt: (v) => `+${v} max HP` },
    CHARMER: { name: 'Charmer', kind: 'class', color: '#ff9ad5', bp: [2, 4], lv: [0.55, 0.9], crestSymbol: 'star5',
      desc: "Enemies hit by a Charmer's magic damage lose attack damage for 8 seconds.", fmt: (v) => `-${pct(v)} AD for 8s` },
    DUELIST: { name: 'Duelist', kind: 'class', color: '#f4d35e', bp: [2, 4, 6], lv: [0.12, 0.18, 0.26], crestSymbol: 'sword',
      desc: 'Each basic attack gives a Duelist attack speed, stacking up to 8 times.', fmt: (v) => `+${pct(v)} attack speed per attack` },
    HUNTER: { name: 'Hunter', kind: 'class', color: '#9be15d', bp: [2, 3, 4], lv: [1.4, 1.7, 2], every: [3.5, 3, 2.5], crestSymbol: 'crosshair',
      desc: 'On a timer, every Hunter strikes the enemy with the lowest % HP for a share of its attack damage.',
      fmt: (v, i) => `every ${T.HUNTER.every[i]}s, a ${pct(v)} AD strike` },
    KEEPER: { name: 'Keeper', kind: 'class', color: '#8fb4d9', bp: [2, 4], lv: [140, 240], dur: [8, 10], crestSymbol: 'shield',
      desc: 'When combat starts, Keepers shield themselves and every ally next to them. The shield lasts a few seconds and is 50% stronger on Keepers.',
      fmt: (v, i) => `${v} shield for ${T.KEEPER.dur[i]}s` },
    MAGE: { name: 'Sorcerer', kind: 'class', color: '#c48bff', bp: [3, 5, 7], lv: [0.65, 0.85, 1.1], crestSymbol: 'star6',
      desc: 'Sorcerers cast their ability twice in a row, with changed ability power.', fmt: (v) => `cast twice at ${pct(v)} AP` },
    WARDER: { name: 'Warder', kind: 'class', color: '#5fd6c8', bp: [2, 4], lv: [14, 32], crestSymbol: 'shield',
      desc: 'Warders guard the team against abilities: all allies gain magic resist.', fmt: (v) => `all allies +${v} MR` },
    SHADE: { name: 'Shade', kind: 'class', color: '#6a4c93', bp: [2, 3], lv: [90, 220], crestSymbol: 'moon',
      desc: 'Shades leap to the enemy backline when combat starts. Every third basic attack they slip into the shadows (untouchable for 0.5s) and deal bonus magic damage.',
      fmt: (v) => `+${v} magic damage every 3rd attack` },
    SHARP: { name: 'Sharpshooter', kind: 'class', color: '#5ad1ff', bp: [2, 4, 6], lv: [1, 2, 3], frac: [0.5, 0.55, 0.6], crestSymbol: 'crosshair',
      desc: 'Sharpshooter basic attacks and abilities bounce to nearby enemies for part of the damage.',
      fmt: (v, i) => `${v} bounce${v > 1 ? 's' : ''} for ${pct(T.SHARP.frac[i])} damage` },
    VANGUARD: { name: 'Vanguard', kind: 'class', color: '#c0c6d0', bp: [2, 4, 6], lv: [60, 140, 260], crestSymbol: 'shield',
      desc: 'Vanguards gain armor.', fmt: (v) => `+${v} armor` },
  };

  // codex breakpoint text, built from the numbers above
  for (const id in T) T[id].bpDesc = T[id].lv.map((v, i) => T[id].fmt(v, i));
  T.NINJA.notes = 'With 2, 3 or 5+ different Shinobi the trait is off.';

  // ---------- lines ----------
  // L(id, tier, traits, attr, forms, abilities, type, options). src = the TFT champion the line adapts.
  const L = (id, tier, traits, attr, forms, abil, type, opt) => DT.line(id, tier, traits, attr, forms, abil, type, opt);
  const lines = [
    // 1-cost (13)
    L('fat_lunamon', 1, ['LUNAR', 'ASSASSIN'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Luna Claw', 'Moon Night Kick', 'Lunatic Dance'], 'dash', { role: 'Assassin', src: 'TFT4 Diana' }),
    L('fat_keramon', 1, ['OCEAN', 'KEEPER'], 'Virus', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', "Hell's Grenade"], 'drain', { role: 'Bruiser', range: 2, src: 'TFT4 Elise' }),
    L('fat_zubamon', 1, ['PILGRIM', 'DUELIST'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Durandamon'], ['Twenty Dive', 'Road to Decade', 'Zwei Glanze'], 'stun', { role: 'Duelist', src: 'TFT4 Fiora' }),
    L('fat_hagurumon', 1, ['DEVA', 'VANGUARD'], 'Vaccine', ['Hagurumon', 'Guardromon', 'Andromon'], ['Cog Crusher', 'Destruction Grenade', 'Spiral Sword'], 'slam', { role: 'Guardian', src: 'TFT4 Garen' }),
    L('fat_gotsumon', 1, ['LUNAR', 'CHARMER'], 'Data', ['Gotsumon', 'Icemon', 'IceDevimon'], ['Rock Fist', 'Iceball Bomb', 'Frost Claw'], 'aoe', { role: 'Mystic', src: 'TFT4 Lissandra' }),
    L('fat_mushroomon', 1, ['FOREST', 'BRAWLER'], 'Virus', ['Mushroomon', 'Woodmon', 'Cherrymon'], ['Laughing Smasher', 'Branch Drain', 'Cherry Bomb'], 'slam', { role: 'Bruiser', src: 'TFT4 Maokai' }),
    L('fat_otamamon', 1, ['PILGRIM', 'MAGE'], 'Virus', ['Otamamon', 'Gekomon', 'ShogunGekomon'], ['Lullaby Bubble', 'Symphony Crusher', 'Musical Fist'], 'stun', { role: 'Mystic', src: 'TFT4 Nami' }),
    L('fat_terriermon', 1, ['DEVA', 'SHARP'], 'Vaccine', ['Terriermon', 'Gargomon', 'Rapidmon'], ['Terrier Tornado', 'Gargo Laser', 'Rapid Fire'], 'nuke', { role: 'Sniper', src: 'TFT4 Nidalee' }),
    L('fat_numemon', 1, ['TREASURE', 'BRAWLER'], 'Virus', ['Numemon', 'Sukamon', 'PlatinumNumemon'], ['Poop Toss', 'Poop Blast', 'Platinum Burp'], 'shield', { role: 'Bruiser', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT4 Tahm Kench' }),
    L('fat_betamon', 1, ['OCEAN', 'MAGE'], 'Virus', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Winder', 'Lightning Javelin'], 'line', { role: 'Mystic', src: 'TFT4 Twisted Fate' }),
    L('fat_demidevimon', 1, ['NIGHT', 'SHARP'], 'Virus', ['DemiDevimon', 'Devimon', 'SkullSatamon'], ['Demi Darts', 'Death Claw', 'Nail Bone'], 'execute', { role: 'Sniper', src: 'TFT4 Vayne' }),
    L('fat_patamon', 1, ['CELES', 'VANGUARD'], 'Data', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Air Shot', "Heaven's Knuckle", "Heaven's Gate"], 'stun', { role: 'Guardian', src: 'TFT4 Wukong' }),
    L('fat_ryudamon', 1, ['RONIN', 'DUELIST'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Katana Attack', 'Metal Armor Blade', 'Legendary Dragon Blade'], 'line', { role: 'Duelist', ai: { place: 'flank' }, src: 'TFT4 Yasuo' }),
    // 2-cost (13)
    L('fat_coronamon', 2, ['TREASURE', 'MAGE'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Corona Flame', 'Fira Claw', 'Phoebus Blow'], 'aoe', { levels: ['Rookie', 'Champion', 'Mega'], role: 'Mystic', range: 2, hpMul: 1.1, src: 'TFT4 Annie' }),
    L('fat_gabumon', 2, ['LUNAR', 'HUNTER'], 'Data', ['Gabumon', 'Garurumon', 'WereGarurumon'], ['Blue Blaster', 'Fox Fire', 'Wolf Claw'], 'barrage', { role: 'Sniper', src: 'TFT4 Aphelios' }),
    L('fat_palmon', 2, ['FOREST', 'VANGUARD'], 'Data', ['Palmon', 'Togemon', 'Lillymon'], ['Poison Ivy', 'Needle Spray', 'Flower Cannon'], 'drain', { role: 'Guardian', radius: 1, src: 'TFT4 Hecarim' }),
    L('fat_hawkmon', 2, ['PILGRIM', 'WARDER'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Dual Sonic'], 'rally', { role: 'Support', src: 'TFT4 Janna' }),
    L('fat_kudamon', 2, ['DEVA', 'KEEPER'], 'Vaccine', ['Kudamon', 'Reppamon', 'Chirinmon'], ['Bullet Whirlwind', 'Rolling Tackle', 'Wind Cutter Sword'], 'stun', { role: 'Guardian', src: 'TFT4 Jarvan IV' }),
    L('fat_gaomon', 2, ['CELES', 'DUELIST'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Dash Double Claw', 'Winning Knuckle'], 'slam', { role: 'Duelist', src: 'TFT4 Jax' }),
    L('fat_lalamon', 2, ['FOREST', 'MAGE'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Seed Blast', 'Sunshine Beam', 'Lila Shower'], 'grow', { role: 'Support', src: 'TFT4 Lulu' }),
    L('fat_gizamon', 2, ['OCEAN', 'ASSASSIN'], 'Virus', ['Gizamon', 'Gesomon', 'Dragomon'], ['Spiral Edge', 'Devil Bashing', 'Forbidden Trident'], 'dash', { levels: ['Rookie', 'Champion', 'Mega'], role: 'Assassin', src: 'TFT4 Pyke' }),
    L('fat_dracmon', 2, ['LUNAR', 'BRAWLER'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Eye of Nightmares', 'Drain Slash', 'Thousand Arrows'], 'line', { role: 'Bruiser', src: 'TFT4 Sylas' }),
    L('fat_fanbeemon', 2, ['SPIRIT', 'SHARP'], 'Virus', ['FanBeemon', 'Waspmon', 'CannonBeemon'], ['Gear Stinger', 'Turbo Stinger', 'Nitro Stinger'], 'weaken', { role: 'Sniper', range: 3, src: 'TFT4 Teemo' }),
    L('fat_candlemon', 2, ['NIGHT', 'VANGUARD'], 'Data', ['Candlemon', 'Bakemon', 'Phantomon'], ['Bonfire', "Hell's Hand", 'Soul Chopper'], 'rally', { role: 'Guardian', range: 2, src: 'TFT4 Thresh' }),
    L('fat_agumon', 2, ['DEVA', 'BRAWLER'], 'Vaccine', ['Agumon', 'Greymon', 'MetalGreymon'], ['Pepper Breath', 'Mega Flame', 'Trident Arm'], 'aoe', { role: 'Bruiser', src: 'TFT4 Vi' }),
    L('fat_falcomon', 2, ['NINJA', 'SHADE'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Scratch Smash', 'Kunai Wing', 'Savage Emperor'], 'dash', { role: 'Assassin', src: 'TFT4 Zed' }),
    // 3-cost (13)
    L('fat_kotemon', 3, ['NINJA', 'ASSASSIN'], 'Data', ['Kotemon', 'Musyamon', 'Grademon'], ['Meteor Slash', 'Ninja Blade', 'Grade Slash'], 'nuke', { role: 'Assassin', src: 'TFT4 Akali' }),
    L('fat_blackgatomon', 3, ['OCEAN', 'SHADE'], 'Virus', ['BlackGatomon', 'LadyDevimon', 'Lilithmon'], ["Cat's Eye Hypnotism", 'Darkness Wave', 'Phantom Pain'], 'execute', { role: 'Assassin', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT4 Evelynn' }),
    L('fat_hackmon', 3, ['PILGRIM', 'CELES', 'ADEPT'], 'Data', ['Hackmon', 'BaoHuckmon', 'Jesmon'], ['Fifth Rush', 'Fif Cross', 'Judgment of the Blade'], 'line', { role: 'Duelist', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT4 Irelia' }),
    L('fat_commandramon', 3, ['TREASURE', 'SHARP'], 'Virus', ['Commandramon', 'Sealsdramon', 'Darkdramon'], ['M16 Assassin', 'Death Behind', 'Gigastick Lance'], 'burst', { levels: ['Rookie', 'Champion', 'Mega'], role: 'Sniper', src: 'TFT4 Jinx' }),
    L('fat_syakomon', 3, ['OCEAN', 'DUELIST'], 'Virus', ['Syakomon', 'Coelamon', 'MarineAngemon'], ['Black Pearl Blast', 'Fossil Bite', 'Ocean Love'], 'execute', { levels: ['Rookie', 'Champion', 'Mega'], role: 'Duelist', range: 3, hpMul: 0.9, src: 'TFT4 Kalista' }),
    L('fat_gumdramon', 3, ['DEVA', 'TREASURE', 'ASSASSIN'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Slayerdramon'], ['Fire Vortex', 'Spin Caliber', 'Tenryu Slash'], 'barrage', { levels: ['Rookie', 'Champion', 'Mega'], role: 'Assassin', src: 'TFT4 Katarina' }),
    L('fat_tentomon', 3, ['NINJA', 'KEEPER'], 'Vaccine', ['Tentomon', 'Kabuterimon', 'MegaKabuterimon'], ['Super Shocker', 'Electro Shocker', 'Horn Buster'], 'slam', { role: 'Bruiser', range: 2, radius: 2, src: 'TFT4 Kennen' }),
    L('fat_labramon', 3, ['SPIRIT', 'HUNTER'], 'Vaccine', ['Labramon', 'Dobermon', 'Astamon'], ['Cure Liqueur', 'Grau Larm', 'Maverick'], 'nuke', { role: 'Sniper', range: 3, src: 'TFT4 Kindred' }),
    L('fat_salamon', 3, ['CELES', 'CHARMER'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', 'Celestial Arrow'], 'line', { role: 'Mystic', range: 4, hpMul: 1.1, src: 'TFT4 Lux' }),
    L('fat_chuumon', 3, ['FOREST', 'BRAWLER'], 'Virus', ['Chuumon', 'Tyrannomon', 'MetalTyrannomon'], ['Cheese Bomb', 'Wild Buster', 'Nuclear Laser'], 'execute', { role: 'Bruiser', src: 'TFT4 Nunu & Willump' }),
    L('fat_aruraumon', 3, ['FOREST', 'MAGE'], 'Data', ['Aruraumon', 'Vegiemon', 'Vademon'], ['Nemesis Ivy', 'Sharp Leaf', 'Unidentified Flying Kiss'], 'blossom', { role: 'Mystic', src: 'TFT4 Veigar' }),
    L('fat_dracomon', 3, ['DEVA', 'DUELIST'], 'Data', ['Dracomon', 'Coredramon', 'Wingdramon'], ['Tail Smash', 'Blue Flare Breath', 'Blaze Sonic Breath'], 'slam', { role: 'Duelist', src: 'TFT4 Xin Zhao' }),
    L('fat_meicoomon', 3, ['SPIRIT', 'WARDER'], 'Data', ['Meicoomon', 'Meicrackmon VM', 'Raguelmon'], ['X Scratch', 'Berserk Sinking', 'Form Taranis'], 'heal', { role: 'Support', levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT4 Yuumi' }),
    // 4-cost (11)
    L('fat_blackagumon', 4, ['OCEAN', 'VANGUARD'], 'Virus', ['BlackAgumon', 'BlackGreymon', 'BlackWarGreymon'], ['Spitfire', 'Great Antler', 'Terra Destroyer'], 'slam', { role: 'Bruiser', hpMul: 1.15, adMul: 1.15, levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT4 Aatrox' }),
    L('fat_renamon', 4, ['SPIRIT', 'MAGE'], 'Data', ['Renamon', 'Kyubimon', 'Taomon'], ['Diamond Storm', 'Fox Tail Inferno', 'Talisman of Light'], 'aoe', { role: 'Mystic', radius: 2, src: 'TFT4 Ahri' }),
    L('fat_biyomon', 4, ['FOREST', 'HUNTER'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Shadow Wing'], 'barrage', { role: 'Sniper', shots: 5, src: 'TFT4 Ashe' }),
    L('fat_tapirmon', 4, ['NIGHT', 'WARDER'], 'Data', ['Tapirmon', 'Wizardmon', 'Wisemon'], ['Nightmare Syndrome', 'Thunder Cloud', 'Dialogue'], 'burst', { role: 'Mystic', range: 2, src: 'TFT4 Cassiopeia' }),
    L('fat_dorumon', 4, ['OCEAN', 'SHARP'], 'Data', ['Dorumon', 'Dorugamon', 'DoruGreymon'], ['Dash Metal', 'Power Metal', 'Metal Meteor'], 'nuke', { role: 'Sniper', adMul: 1.15, src: 'TFT4 Jhin' }),
    L('fat_piddomon', 4, ['PILGRIM', 'CHARMER'], 'Vaccine', ['Piddomon', 'Ophanimon', 'Lucemon FM'], ['Fire Phaser', "Eden's Javelin", 'Paradise Lost'], 'drain', { role: 'Mystic', radius: 1, levels: ['Champion', 'Mega', 'Mega'], src: 'TFT4 Morgana' }),
    L('fat_guilmon', 4, ['NIGHT', 'KEEPER'], 'Virus', ['Guilmon', 'Growlmon', 'Gallantmon'], ['Rock Breaker', 'Plasma Blade', 'Lightning Joust'], 'slam', { role: 'Duelist', levels: ['Rookie', 'Champion', 'Mega'], src: 'TFT4 Riven' }),
    L('fat_gomamon', 4, ['TREASURE', 'VANGUARD'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Zudomon'], ['Marching Fishes', 'Harpoon Torpedo', "Vulcan's Hammer"], 'burst', { role: 'Guardian', hpMul: 1.15, src: 'TFT4 Sejuani' }),
    L('fat_wormmon', 4, ['NINJA', 'ADEPT', 'WARDER'], 'Free', ['Wormmon', 'Stingmon', 'Dinobeemon'], ['Sticky Net', 'Spiking Strike', 'Hell Masquerade'], 'shield', { role: 'Guardian', src: 'TFT4 Shen' }),
    L('fat_jazamon', 4, ['PILGRIM', 'ASSASSIN'], 'Data', ['Jazamon', 'Jazardmon', 'Jazarichmon'], ['Jaza Claw', 'Laser Eye', 'Messer Spiral'], 'execute', { role: 'Assassin', levels: ['Rookie', 'Ultimate', 'Mega'], src: 'TFT4 Talon' }),
    L('fat_blackgabumon', 4, ['CELES', 'HUNTER', 'BRAWLER'], 'Virus', ['BlackGabumon', 'BlackGarurumon', 'BlackMetalGarurumon'], ['Little Horn', 'Howling Wolf', 'Garuru Tomahawk'], 'frenzy', { role: 'Bruiser', src: 'TFT4 Warwick' }),
    // 5-cost (10)
    L('fat_imperialdramon', 5, ['DEVA', 'KEEPER', 'SOVEREIGN'], 'Free', ['Paildramon', 'Imperialdramon FM', 'Imperialdramon PM'], ['Death Parade Blaster', 'Gigadeath', 'Omni Blade'], 'line', { role: 'Sniper', levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT4 Azir' }),
    L('fat_floramon', 5, ['FOREST', 'CHARMER'], 'Data', ['Floramon', 'Lotosmon', 'Lotosmon'], ['Rain of Pollen', 'Serpent Cure', 'Serpent Ruin'], 'line', { role: 'Sniper', hpMul: 1.15, adMul: 1.2, levels: ['Rookie', 'Mega', 'Mega'], src: 'TFT4 Ezreal' }),
    L('fat_lopmon', 5, ['FALLEN', 'SHADE'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Gauntlet Claw', 'Asipatravana'], 'dash', { role: 'Assassin', hpMul: 1.1, adMul: 1.15, levels: ['Rookie', 'Champion', 'Ultimate'], src: 'TFT4 Kayn' }),
    L('fat_veemon', 5, ['CELES', 'DUELIST'], 'Free', ['Veemon', 'Magnamon', 'Magnamon X'], ['Vee Headbutt', 'Magna Punch', 'Plasma Shoot'], 'stun', { role: 'Duelist', levels: ['Rookie', 'Armor', 'Mega'], src: 'TFT4 Lee Sin' }),
    L('fat_myotismon', 5, ['NIGHT', 'MAGE'], 'Virus', ['Myotismon', 'VenomMyotismon', 'GranDracmon'], ['Crimson Lightning', 'Venom Infusion', 'Crystal Revolution'], 'lullaby', { role: 'Mystic', range: 2, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT4 Lillia' }),
    L('fat_leomon', 5, ['BANCHO', 'BRAWLER'], 'Vaccine', ['Leomon', 'GrapLeomon', 'BanchoLeomon'], ['Fist of the Beast King', 'The King of Fists', 'Flashy Boss Punch'], 'showstopper', { role: 'Bruiser', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT4 Sett' }),
    L('fat_ouryumon', 5, ['RONIN', 'ADEPT'], 'Vaccine', ['Ouryumon', 'Zanbamon', 'Alphamon'], ['Eiseiryuoujin', 'Vorpal Blade', 'Blade of the Dragon King'], 'line', { role: 'Duelist', adMul: 1.1, ai: { place: 'flank' }, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT4 Yone' }),
    L('fat_clockmon', 5, ['OCEAN', 'WARDER'], 'Virus', ['Clockmon', 'Kimeramon', 'Millenniummon'], ['Chrono Breaker', 'Heat Viper', 'Time Unlimited'], 'rewind', { role: 'Support', levels: ['Champion', 'Ultimate', 'Mega'], src: 'TFT4 Zilean' }),
    L('fat_volcamon', 5, ['FOREST', 'VANGUARD', 'FORGE'], 'Data', ['Volcamon', 'PileVolcamon', 'Volcanicdramon'], ['Big Bang Voice', 'Volcanic Driver', 'Volcanic Flare'], 'slam', { role: 'Guardian', hpMul: 1.3, adMul: 1.2, radius: 2, levels: ['Ultimate', 'Mega', 'Mega'], src: 'TFT4.5 Ornn' }),
    L('fat_impmon', 5, ['STYLE', 'SHARP'], 'Virus', ['Impmon', 'Beelzemon', 'Beelzemon BM'], ['Bada Boom', 'Double Impact', 'Death Slinger'], 'barrage', { role: 'Sniper', range: 2, adMul: 1.25, hpMul: 1.1, shots: 6, mana: 1000, startMana: 0, levels: ['Rookie', 'Mega', 'Mega'], src: 'TFT4.5 Samira' }),

    // ---- wild Digimon (PvE only) and summons ----
    L('fat_goblimon', 1, [], 'Virus', ['Goblimon', 'Goblimon', 'Goblimon'], ['Goblin Strike', 'Goblin Strike', 'Goblin Strike'], 'nuke', { role: 'Striker', shop: false, hpMul: 0.8 }),
    L('fat_kuwagamon', 2, [], 'Virus', ['Kuwagamon', 'Kuwagamon', 'Kuwagamon'], ['Scissor Claw', 'Scissor Claw', 'Scissor Claw'], 'nuke', { role: 'Striker', shop: false }),
    L('fat_ogremon', 2, [], 'Virus', ['Ogremon', 'Ogremon', 'Ogremon'], ['Pummel Whack', 'Pummel Whack', 'Pummel Whack'], 'slam', { role: 'Guardian', shop: false }),
    L('fat_witchmon', 3, [], 'Data', ['Witchmon', 'Witchmon', 'Witchmon'], ['Baluluna Gale', 'Baluluna Gale', 'Baluluna Gale'], 'aoe', { role: 'Mystic', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_etemon', 3, [], 'Virus', ['Etemon', 'Etemon', 'Etemon'], ['Love Serenade', 'Love Serenade', 'Love Serenade'], 'stun', { role: 'Bruiser', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_asuramon', 4, [], 'Data', ['Asuramon', 'Asuramon', 'Asuramon'], ['Fire Fist of Shiva', 'Fire Fist of Shiva', 'Fire Fist of Shiva'], 'dash', { role: 'Assassin', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_andiramon', 4, [], 'Virus', ['Andiramon', 'Andiramon', 'Andiramon'], ['Deva Spear', 'Deva Spear', 'Deva Spear'], 'line', { role: 'Mystic', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_mammothmon', 4, [], 'Vaccine', ['Mammothmon', 'Mammothmon', 'Mammothmon'], ['Tusk Strikes', 'Tusk Strikes', 'Tusk Strikes'], 'slam', { role: 'Guardian', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_whamon', 3, [], 'Virus', ['Whamon', 'Whamon', 'Whamon'], ['Tidal Wave', 'Tidal Wave', 'Tidal Wave'], 'weaken', { role: 'Bruiser', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('fat_metalseadramon', 5, [], 'Virus', ['MetalSeadramon', 'MetalSeadramon', 'MetalSeadramon'], ['River of Power', 'River of Power', 'River of Power'], 'aoe', { role: 'Bruiser', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('fat_azulongmon', 5, [], 'Data', ['Azulongmon', 'Azulongmon', 'Azulongmon'], ['Lightning Whip', 'Lightning Whip', 'Lightning Whip'], 'chain', { role: 'Mystic', shop: false, boss: true, hpMul: 1.5, adMul: 1.2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('fat_zhuqiaomon', 5, [], 'Virus', ['Zhuqiaomon', 'Zhuqiaomon', 'Zhuqiaomon'], ['Phoenix Fire', 'Phoenix Fire', 'Phoenix Fire'], 'aoe', { role: 'Mystic', shop: false, boss: true, hpMul: 1.4, adMul: 1.2, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('fat_baihumon', 5, [], 'Data', ['Baihumon', 'Baihumon', 'Baihumon'], ['Kongou', 'Kongou', 'Kongou'], 'execute', { role: 'Duelist', shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('fat_ebonwumon', 5, [], 'Vaccine', ['Ebonwumon', 'Ebonwumon', 'Ebonwumon'], ['Mugen', 'Mugen', 'Mugen'], 'shield', { role: 'Guardian', shop: false, boss: true, hpMul: 1.7, levels: ['Mega', 'Mega', 'Mega'] }),
    L('fat_goldramon', 5, [], 'Free', ['Goldramon', 'Goldramon', 'Goldramon'], ['God Flame', 'God Flame', 'God Flame'], 'aoe', { role: 'Bruiser', shop: false, boss: true, hpMul: 2.2, adMul: 1.4, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    // summons: Dark Ocean shadow, Sovereign guards, Void Portal construct (never in waves or shops)
    L('fat_shadow', 4, [], 'Virus', ['Soulmon', 'Soulmon', 'Soulmon'], ['Necro Magic', 'Necro Magic', 'Necro Magic'], 'slam', { role: 'Bruiser', shop: false, hpMul: 1.3, levels: ['Rookie', 'Champion', 'Ultimate'] }),
    L('fat_statue', 1, [], 'Free', ['Golemon', 'Golemon', 'Golemon'], ['Guardian Bomb', 'Guardian Bomb', 'Guardian Bomb'], 'shield', { role: 'Guardian', shop: false, range: 0.01, mana: 99999, startMana: 0, as: 0.01 }),
    L('fat_construct', 2, [], 'Data', ['Tankmon', 'Tankmon', 'Tankmon'], ['Hyper Cannon', 'Hyper Cannon', 'Hyper Cannon'], 'slam', { role: 'Guardian', shop: false }),
  ];

  // Ability text for the set's own ability types (behavior in fates-mech.js)
  const ABIL_TEXT = {
    fat_lalamon: (s, v) => `Makes the lowest-health ally grow: it gains ${v} max HP and heals for as much. Enemies next to it are stunned for 1s.`,
    fat_aruraumon: (s, v) => `Blasts the lowest-health enemy for ${v} magic damage. A kill gives this Digimon 2 ability power for the rest of the game (up to 60).`,
    fat_commandramon: (s, v) => `Deals ${v} magic damage to the target and enemies next to it, stunning them for 1.5s.`,
    fat_tapirmon: (s, v) => `Deals ${v} magic damage to the target and enemies next to it, stunning them for 1.5s.`,
    fat_gomamon: (s, v) => `Deals ${v} magic damage to the target and enemies next to it, stunning them for 1.5s.`,
    fat_blackgabumon: (s, v) => `First cast: gains 60% attack speed and 25% omnivamp for the rest of combat. Later casts heal it for ${v}.`,
    fat_myotismon: (s, v) => `Puts the ${s + 2} enemies with the most HP to sleep (stunned for ${[1.5, 2, 2.5][s]}s) and deals ${v} magic damage to each.`,
    fat_leomon: (s, v) => `Slams the target for ${v} magic damage plus ${[6, 10, 20][s]}% of its max HP, and half of that to enemies next to it.`,
    fat_clockmon: (s, v) => `Places a time rune on the ${s + 1} lowest-health allies. When one would be knocked out, it comes back after 2s with ${v} HP and 30% attack speed.`,
  };
  for (const l of lines) if (ABIL_TEXT[l.id]) l.abilText = ABIL_TEXT[l.id];

  // Balance multipliers from the duel and full-game runs (docs/sets/fates.md)
  const TUNE = {
    fat_wormmon: { hpMul: 0.85, mana: 120 },
    fat_clockmon: { hpMul: 0.85 },
    fat_keramon: { hpMul: 0.9 },
    fat_leomon: { hpMul: 0.95 },
    fat_coronamon: { hpMul: 1.2, mana: 50 },
    fat_gumdramon: { adMul: 1.1 },
    fat_tapirmon: { hpMul: 1.15, mana: 50 },
    fat_veemon: { hpMul: 1.1, adMul: 1.15 },
    fat_gizamon: { adMul: 1.1 },
    fat_palmon: { hpMul: 0.92 },
    fat_gotsumon: { hpMul: 1.2 },
    fat_commandramon: { adMul: 1.1 },
    fat_lopmon: { hpMul: 1.3, adMul: 1.3 },
    fat_imperialdramon: { hpMul: 1.1, adMul: 1.3 },
    fat_biyomon: { adMul: 1.15 },
    fat_betamon: { hpMul: 1.1 },
    fat_aruraumon: { hpMul: 1.1 },
    fat_floramon: { adMul: 1.45, mana: 55 },
    fat_numemon: { hpMul: 0.9 },
    fat_patamon: { hpMul: 1.1 },
    fat_blackagumon: { hpMul: 1.25 },
  };
  for (const l of lines) if (TUNE[l.id]) Object.assign(l, TUNE[l.id]);

  // ---------- wild rounds (Festival of Beasts) ----------
  // [lineId, stage, col, row]: rows 4-7 in the enemy's own half, 4 = front
  const pveWave = (stage, round, game) => {
    const normal = !!(game && game.pace === 'normal');
    // Normal starts at level 1 and plays wild rounds at 1-2..1-4, then at x-7 when boards are stronger: from stage 4 one stage harder
    if (normal && stage === 1) return round === 2 ? [['fat_goblimon', 0, 3, 4]] : round === 3 ? [['fat_goblimon', 0, 2, 4], ['fat_goblimon', 0, 4, 4]] : [['fat_goblimon', 0, 2, 4], ['fat_goblimon', 0, 4, 4], ['fat_kuwagamon', 1, 3, 5]];
    // stages 2-3 x-7: slightly stronger than Hyper Roll's x-4
    if (normal && stage === 2) return [['fat_ogremon', 1, 3, 4], ['fat_kuwagamon', 1, 2, 4], ['fat_kuwagamon', 1, 4, 4], ['fat_goblimon', 1, 3, 6]];
    if (normal && stage === 3) return [['fat_etemon', 1, 3, 4], ['fat_ogremon', 2, 2, 4], ['fat_ogremon', 1, 4, 4], ['fat_witchmon', 1, 2, 6], ['fat_witchmon', 1, 4, 6]];
    if (normal && stage >= 4) stage += 1;
    if (stage === 1) return round === 2 ? [['fat_goblimon', 0, 2, 4], ['fat_goblimon', 0, 4, 4]] : [['fat_goblimon', 0, 2, 4], ['fat_goblimon', 0, 4, 4], ['fat_kuwagamon', 0, 3, 5]];
    if (stage === 2) return [['fat_ogremon', 0, 3, 4], ['fat_kuwagamon', 1, 2, 4], ['fat_kuwagamon', 1, 4, 4], ['fat_goblimon', 1, 3, 6]];
    // Lantern night: Witchmon and Etemon
    if (stage === 3) return [['fat_etemon', 1, 3, 4], ['fat_ogremon', 1, 2, 4], ['fat_ogremon', 1, 4, 4], ['fat_witchmon', 1, 2, 6], ['fat_witchmon', 1, 4, 6]];
    // the Deva Generals
    if (stage === 4) return [['fat_mammothmon', 1, 3, 4], ['fat_mammothmon', 1, 1, 4], ['fat_asuramon', 1, 5, 4], ['fat_andiramon', 1, 2, 6], ['fat_andiramon', 1, 4, 6], ['fat_etemon', 1, 3, 7]];
    // the Dark Ocean
    if (stage === 5) return [['fat_metalseadramon', 1, 3, 4], ['fat_whamon', 2, 1, 4], ['fat_whamon', 2, 5, 4], ['fat_mammothmon', 1, 3, 5], ['fat_andiramon', 1, 2, 7], ['fat_witchmon', 1, 4, 7]];
    // the Four Holy Beasts
    if (stage === 6) return [['fat_ebonwumon', 2, 3, 4], ['fat_baihumon', 1, 1, 4], ['fat_mammothmon', 2, 5, 4], ['fat_zhuqiaomon', 1, 2, 6], ['fat_azulongmon', 1, 4, 7], ['fat_asuramon', 2, 6, 6]];
    // Huanglongmon and the Holy Beasts
    const s = stage >= 8 ? 2 : 1;
    return [['fat_goldramon', 2, 3, 4], ['fat_ebonwumon', s, 2, 4], ['fat_baihumon', s, 4, 4], ['fat_zhuqiaomon', s, 2, 6], ['fat_azulongmon', s, 4, 6], ['fat_metalseadramon', 2, 3, 7]];
  };

  // Wild round loot: the base game's drops, plus rare artifacts in gold orbs from stage 4.
  const pveReward = (c, won) => {
    const out = [];
    if (c.stage === 1) {
      out.push({ kind: 'blue', items: [c.randomComponent()] });
      if (won) {
        out.push({ kind: 'blue', gold: c.round === 3 ? 2 : 1 });
        if (c.round === 3) {
          const ok = DT.SHOP_LINES.filter((l) => l.tier <= 2 && c.game.pool[l.id] > 0);
          if (ok.length) out.push({ kind: 'gold', unit: c.pick(ok).id });
        }
      }
      return out;
    }
    out.push({ kind: 'blue', gold: won ? 3 : 2 });
    if (won && c.stage >= 4) out.push({ kind: 'gold', items: [c.rng() < 0.12 ? c.pick(ARTIFACT_IDS) : c.randomCompleted()] });
    if (won && c.stage >= 4 && c.rng() < 0.2) out.push({ kind: 'gold', items: [c.randomEmblem()] });
    else out.push({ kind: 'blue', items: [c.randomComponent(c.stage >= 3)] });
    if (won && c.stage >= 3 && c.rng() < 0.25 && DT.CONSUMABLE_IDS.length) out.push({ kind: 'gold', items: [c.pick(DT.CONSUMABLE_IDS)] });
    return out;
  };

  // ---------- items ----------
  // Same stat sizes as the Hyper Roll base game so values read the same across sets.
  const components = {
    fat_sword: { name: 'Brave Blade', short: 'BB', color: '#ff7a45', stats: { ad: 0.15 } },
    fat_bow: { name: 'Swift Feather', short: 'SF', color: '#ffd23f', stats: { as: 0.12 } },
    fat_rod: { name: 'Spirit Staff', short: 'SS', color: '#b26bff', stats: { ap: 15 } },
    fat_tear: { name: 'Mana Lantern', short: 'ML', color: '#4fb3ff', stats: { mana: 15 } },
    fat_vest: { name: 'Chain Mail', short: 'CM', color: '#c0c6d0', stats: { armor: 20 } },
    fat_cloak: { name: 'Ward Cloak', short: 'WC', color: '#3fd6b0', stats: { mr: 20 } },
    fat_belt: { name: 'Giant Sash', short: 'GS', color: '#ff4d64', stats: { hp: 150 } },
    fat_glove: { name: 'Sparring Glove', short: 'SG', color: '#8fdc4e', stats: { crit: 0.1, dodge: 0.08 } },
    fat_egg: { name: 'Digi-Egg', short: 'EG', color: '#fff3b0', stats: {}, special: true,
      desc: 'Combine with a component to make a Crest, or with another Digi-Egg for +1 team size.' },
  };

  const R = (a, b, name, desc, p, extra, o) => [a, b, name, desc, p || null, extra || {}, o];
  const recipes = [
    R('fat_sword', 'fat_sword', 'Dragon Fang', 'Deals 8% more damage. Each kill gives 15% attack damage for the rest of combat.', { amp: 0.08 }, { ad: 0.1 }, { id: 'fat_deathblade', tags: ['ad'], aiRank: 3 }),
    R('fat_sword', 'fat_bow', 'Titan Slayer', 'Deals 6% more damage, and 30% more to enemies with over 1600 max HP.', { amp: 0.06, slayer: 0.3 }, {}, { id: 'fat_slayer', tags: ['ad'], aiRank: 3 }),
    R('fat_sword', 'fat_rod', 'Data Gunblade', 'Heals for 25% of all damage dealt.', { vamp: 0.25 }, {}, { id: 'fat_gunblade', tags: ['ad', 'ap'], aiRank: 2 }),
    R('fat_sword', 'fat_tear', 'Lantern Spear', 'Basic attacks restore 5 extra mana.', { shojin: 5 }, {}, { id: 'fat_shojin', tags: ['ap', 'ad'], aiRank: 2 }),
    R('fat_sword', 'fat_vest', 'Guardian Wing', 'The first time it would be knocked out, it stays at 1 HP, cannot be hurt for 2 seconds, then heals 400 HP.', null, {}, { id: 'fat_angel', tags: ['ad', 'tank'], aiRank: 2 }),
    R('fat_sword', 'fat_cloak', 'Blood Fang', 'Heals for 20% of damage dealt. Below 40% HP, gains a 25% max HP shield once.', { vamp: 0.2, lifeline: { at: 0.4, shield: 0.25 } }, {}, { id: 'fat_bloodthirster', tags: ['ad'], aiRank: 2 }),
    R('fat_sword', 'fat_belt', 'War Banner', 'When combat starts, it and the allies beside it in the same row gain 30% attack speed.', null, {}, { id: 'fat_zeke', tags: ['tank', 'util'] }),
    R('fat_sword', 'fat_glove', 'Infinity Edge', 'Gains 35% more critical strike chance and 10% crit damage.', { critDmg: 0.1 }, { crit: 0.35 }, { id: 'fat_ie', tags: ['ad'], aiRank: 3 }),
    R('fat_bow', 'fat_bow', 'Far Shot Bow', 'Gains 1 attack range and 12% more attack speed.', null, { as: 0.12 }, { id: 'fat_rfc', tags: ['ad'], aiRank: 1 }),
    R('fat_bow', 'fat_rod', 'Rage Wing', 'Each basic attack gives 6% attack speed, stacking.', { rage: 0.06 }, {}, { id: 'fat_rageblade', tags: ['ad', 'ap'], aiRank: 3 }),
    R('fat_bow', 'fat_tear', 'Volt Lantern', 'Every third basic attack sends lightning to 3 enemies for 80 magic damage.', { volt: 80 }, {}, { id: 'fat_shiv', tags: ['ad', 'ap'] }),
    R('fat_bow', 'fat_vest', 'Titan Will', 'Attacking or being attacked gives 2% attack damage and 2 ability power, up to 25 times.', { titan: true }, {}, { id: 'fat_titan', tags: ['tank', 'ad'] }),
    R('fat_bow', 'fat_cloak', 'Twin Shot', 'Basic attacks also hit a second nearby enemy for 60% damage.', { split: 0.6 }, {}, { id: 'fat_runaan', tags: ['ad'], aiRank: 2 }),
    R('fat_bow', 'fat_belt', 'Void Portal', 'When combat starts, nearby enemies attack it. When it is knocked out, a Tankmon rises in its place.', null, {}, { id: 'fat_zzrot', tags: ['tank'] }),
    R('fat_bow', 'fat_glove', 'Armor Breaker', "Basic attacks cut the target's armor by 40% for 3 seconds.", { shred: 0.4 }, {}, { id: 'fat_lw', tags: ['ad'], aiRank: 1 }),
    R('fat_rod', 'fat_rod', 'Ultima Staff', 'A lot of ability power.', null, { ap: 45 }, { id: 'fat_deathcap', tags: ['ap'], aiRank: 3 }),
    R('fat_rod', 'fat_tear', 'Echo Rod', 'Its ability deals 150 extra magic damage to the first target and up to 3 enemies near it.', null, {}, { id: 'fat_luden', tags: ['ap'], aiRank: 2 }),
    R('fat_rod', 'fat_vest', 'Iron Locket', 'When combat starts, it and allies within 2 hexes in the same row gain a shield of 250 (Rookie), 325 (Champion) or 450 (final form) for 8 seconds.', null, {}, { id: 'fat_locket', tags: ['tank', 'util'] }),
    R('fat_rod', 'fat_cloak', 'Mana Zap', 'When an enemy within 2 hexes casts, it takes 150 magic damage.', { zap: 150 }, {}, { id: 'fat_spark', tags: ['tank', 'ap'] }),
    R('fat_rod', 'fat_belt', 'Flame Tome', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }, {}, { id: 'fat_morello', tags: ['ap'], aiRank: 2 }),
    R('fat_rod', 'fat_glove', 'Jewel Gauntlet', 'Abilities can critically strike. +25% crit damage.', { abilCrit: true, critDmg: 0.25 }, {}, { id: 'fat_jg', tags: ['ap'], aiRank: 3 }),
    R('fat_tear', 'fat_tear', 'Blue Lantern', 'Max mana -10. After casting, keeps 20 mana.', { bluebuff: true }, {}, { id: 'fat_bluebuff', tags: ['ap'], aiRank: 3 }),
    R('fat_tear', 'fat_vest', 'Frost Heart', 'Enemies within 2 hexes lose 30% attack speed.', null, {}, { id: 'fat_frozenheart', tags: ['tank'], aiRank: 1 }),
    R('fat_tear', 'fat_cloak', 'Power Chalice', 'When combat starts, it and the allies beside it in the same row gain 30 ability power.', null, {}, { id: 'fat_chalice', tags: ['ap', 'util'] }),
    R('fat_tear', 'fat_belt', 'Holy Ring', 'Every 5 seconds, heals allies within 2 hexes for 80 HP plus 10% of their missing HP.', { redemption: true }, {}, { id: 'fat_redemption', tags: ['tank', 'util'] }),
    R('fat_tear', 'fat_glove', 'Justice Hand', 'Deals 12% more damage and heals for 12% of damage dealt.', { amp: 0.12, vamp: 0.12 }, {}, { id: 'fat_hoj', tags: ['ap', 'ad'], aiRank: 2 }),
    R('fat_vest', 'fat_vest', 'Thorn Mail', 'When hit by a basic attack, deals 60 magic damage to adjacent enemies (1.5s cooldown).', { thorns: 60 }, {}, { id: 'fat_bramble', tags: ['tank'], aiRank: 2 }),
    R('fat_vest', 'fat_cloak', 'Stone Hide', 'Gains 12 armor and magic resist for each enemy within 2 hexes.', { gargoyle: 12 }, {}, { id: 'fat_gargoyle', tags: ['tank'], aiRank: 3 }),
    R('fat_vest', 'fat_belt', 'Flame Core', 'Every 2 seconds, burns the nearest enemy within 2 hexes.', { sunfire: true }, {}, { id: 'fat_sunfire', tags: ['tank'], aiRank: 2 }),
    R('fat_vest', 'fat_glove', 'Stillness Shroud', 'When combat starts, enemies in its column lose a third of their max mana.', null, {}, { id: 'fat_shroud', tags: ['tank', 'util'] }),
    R('fat_cloak', 'fat_cloak', 'Dragon Scale', 'Takes 40% less magic damage.', null, {}, { id: 'fat_dclaw', tags: ['tank'], aiRank: 2 }),
    R('fat_cloak', 'fat_belt', 'Zephyr', 'When combat starts, the enemy across from it is stunned for 3 seconds.', null, {}, { id: 'fat_zephyr', tags: ['tank', 'util'] }),
    R('fat_cloak', 'fat_glove', 'Quick Silver', 'Immune to stuns for the first 12 seconds.', { ccImmune: 12 }, {}, { id: 'fat_qss', tags: ['ad', 'ap'] }),
    R('fat_belt', 'fat_belt', 'Giga Sash', 'A lot of HP.', null, { hp: 450 }, { id: 'fat_warmog', tags: ['tank'], aiRank: 2 }),
    R('fat_belt', 'fat_glove', 'Trap Claw', 'Blocks the first magic damage it would take and stuns the attacker for 3 seconds.', null, {}, { id: 'fat_trapclaw', tags: ['tank'], aiRank: 1 }),
    R('fat_glove', 'fat_glove', 'Thief Glove', 'Each combat, gains two random completed items.', { thief: true }, {}, { id: 'fat_thief', tags: ['ad', 'ap'] }),
  ];

  // Digi-Egg + component = Crest (Set 4 spatula items); more Crests drop from the Armory, loot and augments
  const emblems = { fat_sword: 'CELES', fat_bow: 'DUELIST', fat_rod: 'NIGHT', fat_tear: 'MAGE', fat_vest: 'VANGUARD', fat_cloak: 'FOREST', fat_belt: 'DEVA', fat_glove: 'ASSASSIN' };
  const dropEmblems = ['OCEAN', 'PILGRIM', 'TREASURE', 'LUNAR', 'NINJA', 'SPIRIT', 'BRAWLER', 'KEEPER', 'WARDER', 'SHARP', 'HUNTER', 'CHARMER'];

  // Digi-Forge artifacts (Set 4.5 Ornn items). Forged by the Digi-Forge line, rare in late wild-round loot, and from
  // two augments. noDrop keeps them out of the normal random item pool.
  const A = (id, name, desc, stats, p, tags) => ({ id, name, desc, cat: 'artifact', stats, p: p || {}, tags, noDrop: true, color: '#ff8a2a', color2: '#3a1a08' });
  const artifacts = [
    A('fat_art_anima', 'Ebonwumon Shell', 'Regenerates 3% max HP per second.', { mr: 20, hp: 150 }, { regen: 0.03 }, ['tank']),
    A('fat_art_defiance', 'Undying Armor', '60% of the damage it takes is spread over the next 5 seconds instead, and that delayed damage cannot knock it out.', { ad: 0.15, as: 0.12, armor: 20 }, null, ['tank', 'ad']),
    A('fat_art_winter', 'Glacial Horn', 'Enemies that hurt it lose 35% attack speed for 2 seconds. An enemy slowed 7 times is frozen for 2 seconds (once every 10 seconds).', { armor: 20, hp: 150 }, null, ['tank']),
    A('fat_art_collector', 'Golden Fang', 'Basic attacks knock out enemies below 10% HP. Each knockout this way pays 1 gold after combat.', { ad: 0.3, crit: 0.2 }, null, ['ad']),
    A('fat_art_infinity', 'Yggdrasil Core', 'A bit of every stat.', { ad: 0.3, ap: 30, as: 0.25, armor: 25, mr: 25, hp: 250, crit: 0.25 }, null, ['ad', 'ap', 'tank']),
    A('fat_art_manazane', 'Mana Zanbato', 'After its first cast each combat, it restores 100 mana over 4 seconds.', { ad: 0.15, as: 0.12, mana: 15 }, null, ['ap', 'ad']),
    A('fat_art_cleaver', 'Dark Cleaver', 'Its basic attacks cut armor by 40% and its magic damage cuts magic resist by 20 for 3 seconds.', { ad: 0.3, hp: 150 }, { shred: 0.4 }, ['ad']),
    A('fat_art_randuin', 'Holy Shield Sanctum', 'When combat starts, allies next to it gain 40 armor and magic resist.', { armor: 20, mr: 20, hp: 150 }, null, ['tank']),
    A('fat_art_fist', 'Rocket Fist', 'When combat starts, stuns the farthest enemy for 2 seconds.', { armor: 20, hp: 150, mana: 15 }, null, ['tank']),
    A('fat_art_zhonya', 'Chrono Paradox', 'The first hit that would drop it below 40% HP is blocked; it freezes in time, untouchable, for 2.5 seconds.', { ap: 15, armor: 20, mr: 20 }, null, ['ap', 'tank']),
  ];
  const ARTIFACT_IDS = artifacts.map((a) => a.id);

  const consumables = [
    { id: 'fat_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
    { id: 'fat_reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
    { id: 'fat_dice', consumable: true, name: 'Loaded Dice', short: 'LD', color: '#ff6a6a', color2: '#ffffff', desc: 'Drag onto a Digimon to reroll your shop for free with only Digimon that share a trait with it.' },
    { id: 'fat_copy', consumable: true, name: 'Copy Data', short: 'CD', color: '#7dffb0', color2: '#2a6a4a', desc: 'Drag onto a Digimon to get a Rookie-form copy of it on your bench.' },
    { id: 'fat_holy', consumable: true, noDrop: true, name: 'Holy Spirit', short: 'HS', color: '#fff0a0', color2: '#ffffff', desc: 'Use on the Fallen Choice line (Lopmon) to take the holy path: its damage heals it for 40% of damage dealt. Removes the Dark Spirit.' },
    { id: 'fat_dark', consumable: true, noDrop: true, name: 'Dark Spirit', short: 'DS', color: '#7a3aa8', color2: '#1a0a2a', desc: 'Use on the Fallen Choice line (Lopmon) to take the dark path: it deals 40% more damage for the first 10 seconds. Removes the Holy Spirit.' },
  ];

  // ---------- augments ----------
  const G1 = (id, name, desc, tags) => ({ id, tier: 1, name, desc, tags });
  const G2 = (id, name, desc, tags) => ({ id, tier: 2, name, desc, tags });
  const G3 = (id, name, desc, tags) => ({ id, tier: 3, name, desc, tags });
  const augments = [
    G1('fat_envelope', 'Red Envelope', 'Gain 8 gold.', ['econ']),
    G1('fat_stall', 'Festival Stall', 'Gain 2 random components.', ['items']),
    G1('fat_dice_aug', 'Lucky Dice', 'Gain 1 free reroll every round.', ['econ']),
    G1('fat_charm', 'Paper Charm', 'Gain 5 Tamer HP.', ['econ']),
    G1('fat_dojo', 'Dojo Training', 'Your team gains 12% attack damage and 12 ability power.', ['combat']),
    G1('fat_lanterns', 'Lantern Light', 'Your team starts combat with 20 extra mana.', ['combat']),
    G1('fat_wall', 'Shield Wall', 'Digimon in your front row gain 30 armor, 30 magic resist and 200 HP.', ['combat']),
    G1('fat_nest', "Archers' Nest", 'Digimon in your back two rows gain 25% attack speed.', ['combat']),
    G1('fat_bond', 'Partner Bond', 'Your Partner gains 300 HP and 25 ability power.', ['combat']),
    G1('fat_seeker', 'Destiny Seeker', 'Partners show up in your shop far more often. Gain 3 gold.', ['econ']),
    G2('fat_pouch', 'Lucky Pouch', 'Gain 18 gold.', ['econ']),
    G2('fat_chest', 'Treasure Chest', 'Gain a random completed item and a component.', ['items']),
    G2('fat_egghunt', 'Egg Hunt', 'Gain a Digi-Egg.', ['items']),
    G2('fat_offerings', 'Steady Offerings', 'Gain 2 extra gold every round.', ['econ']),
    G2('fat_shrine', 'Shrine Blessing', 'Your team heals for 12% of damage dealt.', ['combat']),
    G2('fat_united', 'Stand United', 'Your team gains 3% attack damage and 3 ability power for each active trait.', ['combat']),
    G2('fat_tome', 'Tome of Traits', 'Gain a random Crest.', ['items']),
    G2('fat_apprentice', 'Apprentice Smith', 'Gain a random artifact.', ['items']),
    G2('fat_digivolve', 'Instant Digivolve', 'Gain a random Champion-form Digimon of cost 2 or 3.', ['econ']),
    G2('fat_grace', 'Celestial Grace', 'The first time each of your Digimon drops below 50% HP, it takes 30% less damage for 4 seconds.', ['combat']),
    G2('fat_twin', 'Twin Destiny', 'The marked trait of your Partner counts one more time.', ['combat']),
    G3('fat_fortune', "Fortune's Favor", 'Gain 35 gold.', ['econ']),
    G3('fat_grandarmory', 'Grand Armory', 'Gain 2 random completed items.', ['items']),
    G3('fat_golden', 'Golden Egg', 'Gain a Golden Digi-Egg (+1 team size).', ['items']),
    G3('fat_master', 'Master Smith', 'Gain 2 random artifacts.', ['items']),
    G3('fat_giant', 'Giant Growth', 'Your team gains 25% max HP.', ['combat']),
    G3('fat_soulmates', 'Soul Partners', 'Your Partner gains 50% max HP and 50 ability power, and its marked trait counts one more time.', ['combat']),
    G3('fat_mega', 'Mega Digivolve', 'Gain a random Champion-form Digimon of cost 4.', ['econ']),
    G3('fat_festival', 'Festival of Beasts', 'Gain a Partner of cost 4 right away (if you have none) and 4 gold.', ['econ']),
    G3('fat_crowns', 'Lunar Crown', 'Your lowest-stage Digimon on the board digivolves one stage for every combat.', ['combat']),
  ];

  // ---------- Partner mechanic data (TFT Chosen) ----------
  // cost odds of the offered Partner by level, from TFT patch 10.19 (index = level)
  const partnerOdds = [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [40, 60, 0, 0, 0], [20, 50, 30, 0, 0], [10, 45, 45, 0, 0],
    [0, 30, 40, 30, 0], [0, 20, 40, 40, 0], [0, 0, 30, 40, 30]];

  DT.Sets.register({
    id: 'fates',
    name: 'Fates',
    mode: 'tft',
    order: 3,
    desc: 'Partner Digimon: some shop slots offer a Champion-form Partner whose marked trait counts double. One Partner at a time. Hyper Roll rules, Digi-Forge artifacts.',
    colors: { main: '#ff5d7a', accent: '#ffd84a' },
    source: 'TFT Set 4 Fates and Set 4.5 Festival of Beasts. docs/inspo/tft/set-04-fates.md, docs/inspo/tft/set-04-5-festival-of-beasts.md, docs/sets/fates.md',
    traits: T,
    lines,
    pve: { wave: pveWave, reward: pveReward },
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'fat_egg',
      emblems,
      dropEmblems,
      // Crest ids 'fat_emblem_<trait>': other sets share some of this set's trait ids
      emblemIdPrefix: 'fat_',
      emblemName: (t) => `${t.name} Crest`,
      special: [
        { id: 'fat_golden_egg', name: 'Golden Digi-Egg', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {},
          from: ['fat_egg', 'fat_egg'], color: '#ffd84a', color2: '#fff3b0', short: 'GE' },
      ],
      list: artifacts.concat(consumables),
      aiRoles: {
        ap: ['fat_jg', 'fat_bluebuff', 'fat_deathcap', 'fat_luden', 'fat_morello', 'fat_shojin', 'fat_gunblade', 'fat_rageblade', 'fat_hoj', 'fat_chalice', 'fat_shiv', 'fat_qss'],
        ad: ['fat_ie', 'fat_slayer', 'fat_deathblade', 'fat_rageblade', 'fat_bloodthirster', 'fat_runaan', 'fat_lw', 'fat_hoj', 'fat_gunblade', 'fat_angel', 'fat_rfc', 'fat_shiv', 'fat_thief', 'fat_qss'],
        tank: ['fat_gargoyle', 'fat_bramble', 'fat_warmog', 'fat_sunfire', 'fat_dclaw', 'fat_titan', 'fat_redemption', 'fat_locket', 'fat_zeke', 'fat_frozenheart', 'fat_zzrot', 'fat_trapclaw', 'fat_spark', 'fat_shroud', 'fat_zephyr'],
      },
    },
    augments,
    // Per-pace rules. hyper = the engine's Hyper Roll defaults (level by stage, fixed income, flat loss damage).
    // normal = the engine's standard TFT rules with Set 4's XP table (patch 10.19, max level 9), pool sizes and shop
    // odds. The Partner odds key off the level in both paces.
    rules: {
      hyper: {
        levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
        incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
        armoryStages: [2, 3, 4, 5, 6],
        augmentStages: [2, 3, 4],
      },
      normal: {
        poolSize: [0, 29, 22, 18, 12, 10],
        shopOdds: [null, [100, 0, 0, 0, 0], [100, 0, 0, 0, 0], [75, 25, 0, 0, 0], [55, 30, 15, 0, 0], [45, 30, 20, 5, 0],
          [30, 35, 25, 10, 0], [19, 35, 30, 15, 1], [15, 20, 35, 22, 8], [10, 15, 30, 30, 15]],
        xp: { table: [0, 2, 2, 6, 10, 20, 36, 56, 80], perRound: 2, buyCost: 4, buyXp: 4, startLevel: 1, maxLevel: 9 },
      },
    },
    // data the mechanic reads (fates-mech.js)
    partner: { odds: partnerOdds, chance: 0.33, seekerChance: 0.75, hp: 200, bonusHp: 300, bonusAp: 40, bonusAd: 0.2, bonusMana: 0.2, fromLevel: 4 },
    artifacts: ARTIFACT_IDS,
    lineAi: (l) => (l.role === 'Support' ? { carry: false } : {}),
    codexRules: () => [
      { id: 'partner', title: 'Partners', html: '<p>From level 4, a shop slot can offer a <b>Partner</b>: a Digimon already in its Champion form (like 3 copies) for 3 times its cost. One of its traits is marked and counts twice, and it gains 200 HP plus one bonus: 300 HP, 40 ability power, 20% attack damage, or 20% lower max mana.</p><p>You can own one Partner at a time. While you have one, no more are offered; sell it to see them again. A Partner keeps its bonus when it digivolves. You see at least one Partner by round 2-2.</p>' },
      { id: 'forge', title: 'Artifacts', html: '<p>Artifacts are strong items from the Digi-Forge (Volcamon line), rare gold loot from stage 4 wild rounds, and two augments. They never come from the normal random item pool.</p>' },
    ],
    tamers: ['Takato', 'Rika', 'Henry', 'Ryo', 'Jeri', 'Kazu', 'Kenta'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
