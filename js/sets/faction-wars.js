// Faction Wars: the original Digi-Tactics content (TFT Set 1 Faction Wars plus four Dota Auto Chess pieces).
// 60 shop lines, 27 wild Digimon, 29 traits (21 families, 8 roles), 9 components, 36 recipes, 19 Crests, the Golden
// Digi-Egg, 2 consumables, 27 augments and 15 Data Wisps, on Hyper Roll rules. Trait, augment, consumable and
// Data Wisp behavior is in faction-wars-mech.js. This file is the reference example for docs/sets/FORMAT.md.
(function (G) {
  const DT = G.DT;

  // ---------- traits ----------
  // lv values are indexed by active breakpoint (0 = first breakpoint). Codex fields: bpDesc, notes, icon.
  // Effects: hooks added in faction-wars-mech.js.
  const pct = DT.pct;
  const TRAITS = {
    // ---- families ----
    DR: { name: "Dragon's Roar", kind: 'family', color: '#ff6a3d', bp: [2, 4, 6], lv: [200, 450, 800],
      desc: "Dragon's Roar Digimon gain max HP.", fmt: (v) => `+${v} HP` },
    NSp: { name: 'Nature Spirits', kind: 'family', color: '#7ed957', bp: [2, 4, 6], lv: [0.3, 0.55, 0.9],
      desc: 'Nature Spirits gain attack speed.', fmt: (v) => `+${pct(v)} AS` },
    JT: { name: 'Jungle Troopers', kind: 'family', color: '#3fa34d', bp: [2, 4], lv: [0.025, 0.05],
      desc: 'Jungle Troopers regenerate max HP every second.', fmt: (v) => `${pct(v)} HP/s` },
    WG: { name: 'Wind Guardians', kind: 'family', color: '#9be7ff', bp: [2, 3], lv: [0.15, 0.28],
      desc: 'All allies gain a chance to dodge basic attacks.', fmt: (v) => `${pct(v)} dodge` },
    DS: { name: 'Deep Savers', kind: 'family', color: '#2f7bff', bp: [2, 4], lv: [0.35, 0.6],
      desc: 'Deep Savers start combat with a shield.', fmt: (v) => `${pct(v)} HP shield` },
    ME: { name: 'Metal Empire', kind: 'family', color: '#a0a8b8', bp: [2, 4], lv: [0.18, 0.32],
      desc: 'Metal Empire Digimon take less damage.', fmt: (v) => `-${pct(v)} dmg taken` },
    NSo: { name: 'Nightmare Soldiers', kind: 'family', color: '#9b4dff', bp: [2, 4], lv: [0.15, 0.25],
      desc: 'Nightmare Soldiers heal for a share of all damage they deal.', fmt: (v) => `${pct(v)} omnivamp` },
    VB: { name: 'Virus Busters', kind: 'family', color: '#ffe66d', bp: [2, 3], lv: [8, 20],
      desc: 'All allies start combat with bonus mana.', fmt: (v) => `+${v} mana` },
    DL: { name: 'Demon Lords', kind: 'family', color: '#d1003f', bp: [1, 2, 3], lv: [25, 50, 80],
      desc: 'Demon Lords gain ability power. With 2 they deal more damage, and with 3 their abilities also burn.',
      fmt: (v, i) => `+${v} AP${['', ', +15% dmg', ', +30% dmg, burn'][i]}` },
    RK: { name: 'Royal Knights', kind: 'family', color: '#f0d890', bp: [1, 2, 3], lv: [0.05, 0.07, 0.09],
      desc: 'Royal Knights gain damage and max HP for every active trait on your team, this one included.',
      fmt: (v) => `+${pct(v)} dmg & HP per trait`,
      notes: 'No Crest exists for this trait.' },
    DATS: { name: 'DATS', kind: 'family', color: '#ff9e2c', bp: [2, 3, 4], lv: [0.12, 0.25, 0.45], heal: [0.12, 0.2, 0.3],
      desc: 'DigiSoul Charge: the first time an ally is knocked out, or after 10 seconds, DATS Digimon heal and gain attack damage and ability power.',
      fmt: (v) => `DigiSoul: +${pct(v)} AD, +${Math.round(v * 100)} AP` },
    LW: { name: 'Legendary Warriors', kind: 'family', color: '#f2a93b', bp: [2], lv: [0.3], dr: [0.12],
      desc: 'Spirit Evolution (Agunimon and Lobomon): the first time a Legendary Warrior drops below 50% HP, it heals and fights harder for the rest of combat.',
      fmt: (v) => `+${pct(v)} heal & AS at 50% HP` },
    XF: { name: 'Xros Fusion', kind: 'family', color: '#ff4fa0', bp: [2, 3, 4], lv: [0.12, 0.14, 0.15],
      desc: "DigiXros: at the start of combat, each Xros Fusion Digimon adds a share of the other Xros Fusion Digimon's max HP and attack damage to its own.",
      fmt: (v) => `+${pct(v)} of others' HP & AD` },
    INS: { name: 'Insectoid', kind: 'family', color: '#b6d43a', bp: [2, 3, 4], lv: [30, 45, 60],
      desc: 'Every 2 seconds a swarm bites random enemies for magic damage. A bitten enemy misses its next basic attack.',
      fmt: (v, i) => `${i + 1} bite${i ? 's' : ''} for ${v} every 2s` },
    FUN: { name: 'Pranksters', kind: 'family', color: '#c9a227', bp: [2, 4], lv: [0.45, 0.6], gold: [1, 2],
      desc: 'Every second basic attack of a Prankster throws muck: the target deals less damage for 3 seconds. You also gain gold after every combat.',
      fmt: (v, i) => `muck -${pct(v)}, +${TRAITS.FUN.gold[i]} gold/round` },
    KN: { name: 'Knight Order', kind: 'family', color: '#8fb4d9', bp: [2, 3], lv: [0.12, 0.17],
      desc: 'Knights take less damage. A Knight standing next to another Knight takes that much less again.',
      fmt: (v) => `-${pct(v)} dmg, x2 beside a Knight` },
    FL: { name: 'Flame', kind: 'family', color: '#ff8a1f', bp: [2, 4], lv: [0.15, 0.3],
      desc: "Flame Digimon's abilities burn their targets for 1.5% max HP per second and cut their healing by a third for 5 seconds. Flame Digimon deal more damage to burning enemies.",
      fmt: (v) => `burn, +${pct(v)} vs burning` },
    FR: { name: 'Frost', kind: 'family', color: '#cfefff', bp: [2, 3], lv: [0.3, 0.5],
      desc: "Frost Digimon's basic attacks have a chance to freeze the target for 1 second.", fmt: (v) => `${pct(v)} freeze chance` },
    UND: { name: 'Undead', kind: 'family', color: '#8d9a7a', bp: [2, 3], lv: [0.05, 0.1],
      desc: "The first time an Undead Digimon would be knocked out, it stays at 1 HP and can't be hurt for 1 second, then heals.",
      fmt: (v) => `cheat death, heal ${pct(v)}` },
    HB: { name: 'Holy Beasts', kind: 'family', color: '#5fd6c8', bp: [2, 3], lv: [4, 10],
      desc: 'All allies gain armor, magic resist and ability power. Holy Beasts gain double.', fmt: (v) => `+${v} armor/MR/AP (x2 own)` },
    FA: { name: 'Fallen Angels', kind: 'family', color: '#b05cc8', bp: [2, 3, 4], lv: [6, 12, 20],
      desc: 'All enemies lose armor and magic resist.', fmt: (v) => `enemies -${v} armor/MR` },
    // ---- roles ----
    Guardian: { name: 'Guardian', kind: 'role', color: '#c9a66b', bp: [2, 4, 6], lv: [25, 50, 90],
      desc: 'Guardians gain armor and magic resist.', fmt: (v) => `+${v} armor/MR` },
    Striker: { name: 'Striker', kind: 'role', color: '#ff8f5a', bp: [2, 4, 6], lv: [0.25, 0.45, 0.75],
      desc: 'Strikers gain attack damage.', fmt: (v) => `+${Math.round(v * 100)}% AD` },
    Sniper: { name: 'Sniper', kind: 'role', color: '#5ad1ff', bp: [2, 4, 6], lv: [0.05, 0.09, 0.13],
      desc: 'Snipers deal more damage for each hex between them and their target.', fmt: (v) => `+${Math.round(v * 100)}%/hex` },
    Mystic: { name: 'Mystic', kind: 'role', color: '#d48bff', bp: [2, 4, 6], lv: [20, 40, 70],
      desc: 'Mystics gain ability power.', fmt: (v) => `+${v} AP` },
    Assassin: { name: 'Assassin', kind: 'role', color: '#ff4f7b', bp: [2, 4], lv: [0.25, 0.5],
      desc: 'Assassins always leap to the enemy backline when combat starts. The trait grants critical strike chance (160% damage).', fmt: (v) => `${pct(v)} crit` },
    Bruiser: { name: 'Bruiser', kind: 'role', color: '#d9773a', bp: [2, 4], lv: [0.2, 0.4],
      desc: 'Bruisers gain max HP.', fmt: (v) => `+${Math.round(v * 100)}% max HP` },
    Duelist: { name: 'Duelist', kind: 'role', color: '#f4d35e', bp: [2, 4, 6], lv: [0.06, 0.09, 0.13],
      desc: 'Each basic attack gives a Duelist attack speed, stacking up to 12 times.', fmt: (v) => `+${Math.round(v * 100)}% AS per attack` },
    Support: { name: 'Support', kind: 'role', color: '#9cf6c0', bp: [2, 4], lv: [0.1, 0.25], heal: [0.025, 0.05],
      desc: "Your team's healing is stronger. Every 4 seconds, each Support heals the lowest-health ally.",
      fmt: (v, i) => `+${pct(v)} healing, ${pct(TRAITS.Support.heal[i])} heal/4s` },
  };

  // Breakpoint text for the codex, built from the numbers above so tuning never leaves stale text.
  {
    const TT = TRAITS;
    const each = (id, f) => { TT[id].bpDesc = TT[id].lv.map((v, i) => f(v, i, TT[id])); };
    each('DL', (v, i) => [`+${v} AP`, `+${v} AP and 15% more damage`, `+${v} AP, 30% more damage, and abilities burn the targets`][i]);
    each('RK', (v) => `${pct(v)} more damage and ${pct(v)} max HP per active trait`);
    each('DATS', (v, i, T) => `Heal ${pct(T.heal[i])} max HP, +${pct(v)} AD and +${Math.round(v * 100)} AP`);
    each('LW', (v, i, T) => `Heals ${pct(v)} max HP, gains ${pct(v)} attack speed and takes ${pct(T.dr[i])} less damage`);
    each('XF', (v) => `Gain ${pct(v)} of the other Xros Fusion Digimon's combined max HP and attack damage`);
    each('INS', (v, i) => `${i + 1} bite${i ? 's' : ''} for ${v} magic damage every 2 seconds`);
    each('FUN', (v, i, T) => `Muck: the target deals ${pct(v)} less damage. +${T.gold[i]} gold after every combat`);
    each('KN', (v) => `${pct(v)} less damage, ${pct(v * 2)} next to another Knight`);
    each('UND', (v) => `Cheats death once, then heals ${pct(v)} max HP`);
    each('Support', (v, i, T) => `All healing on your team +${pct(v)}. Every 4s each Support heals the lowest-health ally for ${pct(T.heal[i])} of its max HP`);
  }


  // ---------- lines ----------
  const L = DT.line;

  const LINES = [
    // Tier 1 (12)
    L('agumon', 1, ['DR', 'Striker'], 'Vaccine', ['Agumon', 'Greymon', 'MetalGreymon'], ['Pepper Breath', 'Nova Blast', 'Giga Destroyer'], 'nuke'),
    L('gabumon', 1, ['NSp', 'FR', 'Striker'], 'Data', ['Gabumon', 'Garurumon', 'WereGarurumon'], ['Blue Blaster', 'Howling Blaster', 'Wolf Claw'], 'stun', { hpMul: 1.15, adMul: 1.15 }),
    L('tentomon', 1, ['JT', 'INS', 'Sniper'], 'Vaccine', ['Tentomon', 'Kabuterimon', 'MegaKabuterimon'], ['Super Shocker', 'Electro Shocker', 'Horn Buster'], 'aoe'),
    L('palmon', 1, ['JT', 'Mystic'], 'Data', ['Palmon', 'Togemon', 'Lillymon'], ['Poison Ivy', 'Needle Spray', 'Flower Cannon'], 'stun'),
    L('gomamon', 1, ['DS', 'FR', 'Guardian'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Zudomon'], ['Marching Fishes', 'Harpoon Torpedo', "Vulcan's Hammer"], 'slam'),
    L('biyomon', 1, ['WG', 'FL', 'Sniper'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Wing Blade'], 'barrage'),
    L('hagurumon', 1, ['ME', 'Guardian'], 'Virus', ['Hagurumon', 'Guardromon', 'Andromon'], ['Darkness Gear', 'Guardian Barrage', 'Gatling Attack'], 'shield'),
    L('demidevimon', 1, ['NSo', 'FA', 'Assassin'], 'Virus', ['DemiDevimon', 'Devimon', 'SkullSatamon'], ['Demi Dart', 'Death Claw', 'Nail Bone'], 'dash'),
    L('numemon', 1, ['FUN', 'Bruiser'], 'Virus', ['Numemon', 'BlackKingNumemon', 'PlatinumNumemon'], ['Nume-Sludge', 'Kingly Sludge', 'Platinum Sludge'], 'weaken', { levels: ['Champion', 'Ultimate', 'Ultimate'] }),
    L('otamamon', 1, ['FUN', 'Mystic'], 'Virus', ['Otamamon', 'Gekomon', 'ShogunGekomon'], ['Lullaby Bubble', 'Symphony Crusher', 'Musical Fist'], 'stun'),
    L('gotsumon', 1, ['FUN', 'Guardian'], 'Data', ['Gotsumon', 'Starmon', 'SuperStarmon'], ['Rock Fist', 'Meteor Squall', 'Cosmic Ray'], 'shield'),
    L('falcomon', 1, ['DATS', 'WG', 'Assassin'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Ninja Shuriken', 'Kunai Wing', 'Ether Burst'], 'dash', { adMul: 1.1, levels: ['Rookie', 'Champion', 'Mega'] }),
    // Tier 2 (13)
    L('guilmon', 2, ['DR', 'FL', 'Striker'], 'Virus', ['Guilmon', 'Growlmon', 'WarGrowlmon'], ['Pyro Sphere', 'Pyro Blaster', 'Atomic Blaster'], 'aoe'),
    L('renamon', 2, ['NSp', 'Assassin'], 'Data', ['Renamon', 'Kyubimon', 'Taomon'], ['Diamond Storm', 'Dragon Wheel', 'Talisman of Light'], 'dash', { adMul: 1.1 }),
    L('terriermon', 2, ['NSp', 'Sniper'], 'Vaccine', ['Terriermon', 'Gargomon', 'Rapidmon'], ['Bunny Blast', 'Gargo Laser', 'Tri-Beam'], 'barrage'),
    L('patamon', 2, ['VB', 'Mystic'], 'Data', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Boom Bubble', 'Hand of Fate', 'Gate of Destiny'], 'nuke'),
    L('salamon', 2, ['VB', 'Support'], 'Vaccine', ['Salamon', 'Gatomon', 'Angewomon'], ['Puppy Howl', 'Lightning Paw', "Heaven's Charm"], 'heal'),
    L('betamon', 2, ['DS', 'FR', 'Mystic'], 'Virus', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Blast', 'Thunder Javelin'], 'stun'),
    L('wormmon', 2, ['JT', 'INS', 'Assassin'], 'Free', ['Wormmon', 'Stingmon', 'Dinobeemon'], ['Sticky Net', 'Spiking Strike', 'Hell Masquerade'], 'dash'),
    L('lunamon', 2, ['DS', 'FR', 'Sniper'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Lunar Claw', 'Tear Arrow', 'Lunatic Dance'], 'line'),
    L('armadillomon', 2, ['XF', 'Guardian'], 'Free', ['Armadillomon', 'Ankylomon', 'Shakkoumon'], ['Diamond Shell', 'Tail Hammer', 'Justice Beam'], 'shield', { hpMul: 0.92 }),
    L('kudamon', 2, ['DATS', 'HB', 'Support'], 'Vaccine', ['Kudamon', 'Reppamon', 'Chirinmon'], ['Holy Shot', 'Kamaitachi', 'Holy Breath'], 'rally'),
    L('fanbeemon', 2, ['INS', 'Sniper'], 'Virus', ['FanBeemon', 'Waspmon', 'CannonBeemon'], ['Bee Shot', 'Turbo Stinger', 'Royal Smasher'], 'barrage'),
    L('mamemon', 2, ['FUN', 'ME', 'Duelist'], 'Data', ['Mamemon', 'MetalMamemon', 'PrinceMamemon'], ['Smiley Bomb', 'Energy Bomb', 'Smiley Warhead'], 'aoe', { levels: ['Ultimate', 'Ultimate', 'Mega'] }),
    L('dracmon', 2, ['UND', 'NSo', 'Assassin'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Zombie Claw', 'Snipe Steal', 'Fuerza Mortal'], 'execute'),
    // Tier 3 (13)
    L('elecmon', 3, ['NSp', 'Guardian'], 'Data', ['Elecmon', 'Leomon', 'Panjyamon'], ['Super Thunder Strike', 'Fist of the Beast King', 'Ice Fist'], 'slam'),
    L('hawkmon', 3, ['WG', 'XF', 'Striker'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Beak Pummel', 'Blast Rings', 'Static Force'], 'aoe'),
    L('keramon', 3, ['ME', 'Sniper'], 'Virus', ['Keramon', 'Kurisarimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', 'Spider Shooter'], 'barrage'),
    L('lopmon', 3, ['VB', 'HB', 'Guardian'], 'Data', ['Lopmon', 'Turuiemon', 'Andiramon'], ['Blazing Ice', 'Kongou Kick', 'Deva Spear'], 'shield', { hpMul: 0.92 }),
    L('gizamon', 3, ['DS', 'Striker'], 'Virus', ['Gizamon', 'Coelamon', 'Dragomon'], ['Spiral Saw', 'Ancient Fin', 'Forbidden Trident'], 'drain'),
    L('candlemon', 3, ['NSo', 'UND', 'Mystic'], 'Data', ['Meramon', 'Wizardmon', 'Wisemon'], ['Burning Fist', 'Thunder Ball', 'Pandora Dialogue'], 'aoe', { levels: ['Champion', 'Champion', 'Ultimate'] }),
    L('agumonsv', 3, ['DATS', 'DR', 'Striker'], 'Vaccine', ['GeoGreymon', 'RizeGreymon', 'ShineGreymon'], ['Mega Burst', 'Trident Revolver', 'Glorious Burst'], 'nuke', { levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('dracomon', 3, ['DR', 'KN', 'Bruiser'], 'Data', ['Dracomon', 'Coredramon', 'Wingdramon'], ['Baby Breath', 'Green Flare Breath', 'Sonic Blast'], 'aoe'),
    L('woodmon', 3, ['JT', 'Support'], 'Virus', ['Woodmon', 'Cherrymon', 'Puppetmon'], ['Branch Drill', 'Pit Pelter', 'Puppet Pummel'], 'weaken', { levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('monodramon', 3, ['DR', 'Assassin'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Beat Knuckle', 'Strike Fang', 'Desolation Claw'], 'execute'),
    L('jazamon', 3, ['NSp', 'Duelist'], 'Data', ['Jazamon', 'Jazardmon', 'Jazarichmon'], ['Jaza Claw', 'Jazard Fang', 'Jazarich Strike'], 'dash', { levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('zubamon', 3, ['KN', 'Duelist'], 'Vaccine', ['Zubamon', 'ZubaEagermon', 'Durandamon'], ['Zuba Saber', 'Eager Slash', 'Durandal'], 'line', { levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('okuwamon', 3, ['INS', 'Bruiser'], 'Virus', ['Kuwagamon', 'Okuwamon', 'GranKuwagamon'], ['Scissor Claw', 'Double Scissor Claw', 'Dimension Scissor'], 'execute', { levels: ['Champion', 'Ultimate', 'Mega'] }),
    // Tier 4 (12)
    L('veemon', 4, ['DR', 'XF', 'Striker'], 'Free', ['Veemon', 'ExVeemon', 'Paildramon'], ['Vee Headbutt', 'Vee Laser', 'Desperado Blaster'], 'stun'),
    L('dorumon', 4, ['DR', 'Mystic'], 'Data', ['Dorumon', 'Dorugamon', 'DoruGreymon'], ['Metal Cannon', 'Power Metal', 'Brave Metal'], 'nuke'),
    L('gaomon', 4, ['DATS', 'NSp', 'Duelist'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Dash Double Claw', 'Winning Knuckle'], 'drain'),
    L('lalamon', 4, ['DATS', 'JT', 'Support'], 'Data', ['Lalamon', 'Sunflowmon', 'Lilamon'], ['Seed Blast', 'Sunshine Beam', 'Lila Shower'], 'heal'),
    L('commandramon', 4, ['ME', 'Sniper'], 'Data', ['ToyAgumon', 'Tankmon', 'MetalTyrannomon'], ['Toy Flame', 'Hyper Cannon', 'Nuclear Laser'], 'barrage'),
    L('agunimon', 4, ['LW', 'FL', 'Bruiser'], 'Vaccine', ['Agunimon', 'BurningGreymon', 'EmperorGreymon'], ['Pyro Tornado', 'Wildfire Tsunami', 'Pyro Dragons'], 'slam', { levels: ['Human Spirit', 'Beast Spirit', 'Fused Spirit'] }),
    L('lobomon', 4, ['LW', 'NSp', 'Duelist'], 'Vaccine', ['Lobomon', 'KendoGarurumon', 'MagnaGarurumon'], ['Lobo Kendo', 'Solar Laser', 'Magna Missile'], 'line', { adMul: 1.1, hpMul: 1.1, levels: ['Human Spirit', 'Beast Spirit', 'Fused Spirit'] }),
    L('hackmon', 4, ['RK', 'Duelist'], 'Data', ['Hackmon', 'BaoHuckmon', 'Jesmon'], ['Fifth Crow', 'Cyclone Hunter', 'Schwertgeist'], 'dash', { levels: ['Rookie', 'Champion', 'Mega'] }),
    L('blackgatomon', 4, ['DL', 'FA', 'Mystic'], 'Virus', ['BlackGatomon', 'LadyDevimon', 'Lilithmon'], ["Cat's Eye", 'Darkness Wave', 'Phantom Pain'], 'drain', { levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('meicoomon', 4, ['FA', 'NSo', 'Bruiser'], 'Free', ['Meicoomon', 'Meicrackmon VM', 'Raguelmon'], ['Meicoo Crack', 'Berserk Thinking', 'Darkness Zone'], 'drain', { levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('vorvomon', 4, ['FL', 'Guardian'], 'Vaccine', ['Vorvomon', 'Lavorvomon', 'Lavogaritamon'], ['Vorvo Flame', 'Lava Burst', 'Volcanic Howl'], 'slam'),
    L('ryudamon', 4, ['KN', 'Duelist'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Battojutsu', 'Tetsuzan', 'Hisho Ryuuha'], 'execute'),
    // Tier 5 (10)
    L('myotismon', 5, ['NSo', 'UND', 'Mystic'], 'Virus', ['Myotismon', 'VenomMyotismon', 'GranDracmon'], ['Crimson Lightning', 'Venom Infuse', 'Eyes of the Gorgon'], 'drain', { radius: 1, levels: ['Ultimate', 'Mega', 'Mega'] }),
    L('imperialdramon', 5, ['DR', 'RK', 'Sniper'], 'Free', ['Flamedramon', 'Imperialdramon FM', 'Imperialdramon PM'], ['Fire Rocket', 'Giga Crusher', 'Omni Sword'], 'aoe', { radius: 2, levels: ['Armor', 'Mega', 'Mega'] }),
    L('lucemon', 5, ['DL', 'FA', 'Mystic'], 'Vaccine', ['Lucemon', 'Lucemon FM', 'Lucemon SM'], ['Grand Cross', 'Paradise Lost', 'Divine Atonement'], 'aoe', { radius: 2, levels: ['Rookie', 'Ultimate', 'Mega'] }),
    L('beelzemon', 5, ['DL', 'NSo', 'Sniper'], 'Virus', ['Impmon', 'Beelzemon', 'Beelzemon BM'], ['Bada Boom', 'Double Impact', 'Corona Destroyer'], 'barrage', { shots: 6, levels: ['Rookie', 'Mega', 'Mega'] }),
    L('machinedramon', 5, ['ME', 'Guardian'], 'Virus', ['Megadramon', 'Machinedramon', 'Chaosdramon'], ['Dark Side Attack', 'Giga Cannon', 'Hyper Mugen Cannon'], 'slam', { radius: 2, hpMul: 1.15, levels: ['Ultimate', 'Mega', 'Mega'] }),
    L('veedramon', 5, ['RK', 'DR', 'Duelist'], 'Vaccine', ['Veedramon', 'AeroVeedramon', 'UlforceVeedramon'], ['V-Breath Arrow', 'Magnum Crusher', 'Shining V Force'], 'line', { adMul: 1.2, hpMul: 1.15, levels: ['Champion', 'Ultimate', 'Mega'] }),
    L('omnimon', 5, ['RK', 'XF', 'Striker'], 'Vaccine', ['WarGreymon', 'Omnimon', 'Omnimon'], ['Great Tornado', 'Transcendent Sword', 'Supreme Cannon'], 'execute', { levels: ['Mega', 'Mega', 'Mega'] }),
    L('alphamon', 5, ['RK', 'Guardian'], 'Vaccine', ['Alphamon', 'Alphamon', 'Alphamon'], ['Digitalize of Soul', 'Alpha Inforce', 'Ouryuken'], 'slam', { radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('phoenixmon', 5, ['HB', 'FL', 'Mystic'], 'Vaccine', ['Phoenixmon', 'Phoenixmon', 'Phoenixmon'], ['Crimson Flame', 'Phoenix Wing', 'Starlight Explosion'], 'aoe', { radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('daemon', 5, ['DL', 'FL', 'Bruiser'], 'Virus', ['Daemon', 'Daemon', 'Daemon'], ['Evil Inferno', 'Flame Inferno', 'Chaos Flare'], 'aoe', { radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),

    // Wild Digimon for PvE rounds (not in the shop).
    L('goblimon', 1, ['NSo', 'Striker'], 'Virus', ['Goblimon', 'Goblimon', 'Goblimon'], ['Goblin Strike', 'Goblin Strike', 'Goblin Strike'], 'nuke', { shop: false, hpMul: 0.8 }),
    L('kuwagamon', 2, ['JT', 'Striker'], 'Virus', ['Kuwagamon', 'Kuwagamon', 'Kuwagamon'], ['Scissor Claw', 'Scissor Claw', 'Scissor Claw'], 'nuke', { shop: false }),
    L('ogremon', 2, ['NSo', 'Guardian'], 'Virus', ['Ogremon', 'Ogremon', 'Ogremon'], ['Pummel Whack', 'Pummel Whack', 'Pummel Whack'], 'slam', { shop: false }),
    L('tyrannomon', 3, ['DR', 'Striker'], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Blaze Blast', 'Blaze Blast', 'Blaze Blast'], 'aoe', { shop: false }),
    L('etemon', 3, ['NSo', 'Mystic'], 'Virus', ['Etemon', 'Etemon', 'Etemon'], ['Dark Network', 'Dark Network', 'Dark Network'], 'aoe', { shop: false }),
    L('skullgreymon', 4, ['NSo', 'Striker'], 'Virus', ['SkullGreymon', 'SkullGreymon', 'SkullGreymon'], ['Ground Zero', 'Ground Zero', 'Ground Zero'], 'aoe', { shop: false }),
    // Toilet Town round (stage 3) and helpers, on Digimon Links models.
    L('sukamon', 2, ['FUN', 'Bruiser'], 'Virus', ['Sukamon', 'Sukamon', 'Sukamon'], ['Poop Toss', 'Poop Toss', 'Poop Toss'], 'weaken', { shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('nanimon', 3, ['FUN', 'Striker'], 'Virus', ['Nanimon', 'Nanimon', 'Nanimon'], ['Party Time', 'Party Time', 'Party Time'], 'nuke', { shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('monzaemon', 4, ['FUN', 'Guardian'], 'Vaccine', ['Monzaemon', 'Monzaemon', 'Monzaemon'], ['Hearts Attack', 'Hearts Attack', 'Hearts Attack'], 'stun', { shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('frigimon', 2, ['FR', 'Guardian'], 'Vaccine', ['Frigimon', 'Frigimon', 'Frigimon'], ['Sub Zero Ice Punch', 'Sub Zero Ice Punch', 'Sub Zero Ice Punch'], 'stun', { shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('bakemon', 2, ['UND', 'Assassin'], 'Virus', ['Bakemon', 'Bakemon', 'Bakemon'], ['Dark Claw', 'Dark Claw', 'Dark Claw'], 'dash', { shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    // Bosses for late wild rounds.
    L('kimeramon', 5, ['NSo', 'Striker'], 'Data', ['Kimeramon', 'Kimeramon', 'Kimeramon'], ['Heat Viper', 'Heat Viper', 'Heat Viper'], 'aoe', { shop: false, boss: true, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('piedmon', 5, ['NSo', 'Assassin'], 'Virus', ['Piedmon', 'Piedmon', 'Piedmon'], ['Trump Sword', 'Trump Sword', 'Trump Sword'], 'dash', { shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
    L('metalseadramon', 5, ['DS', 'Sniper'], 'Data', ['MetalSeadramon', 'MetalSeadramon', 'MetalSeadramon'], ['River of Power', 'River of Power', 'River of Power'], 'line', { shop: false, boss: true, levels: ['Mega', 'Mega', 'Mega'] }),
    L('crusadermon', 5, ['RK', 'Assassin'], 'Virus', ['Crusadermon', 'Crusadermon', 'Crusadermon'], ['Spiral Masquerade', 'Spiral Masquerade', 'Spiral Masquerade'], 'dash', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('leopardmon', 5, ['RK', 'Duelist'], 'Data', ['Leopardmon', 'Leopardmon', 'Leopardmon'], ['Kaiser Nail', 'Kaiser Nail', 'Kaiser Nail'], 'execute', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('craniamon', 5, ['RK', 'Guardian'], 'Vaccine', ['Craniamon', 'Craniamon', 'Craniamon'], ['End Waltz', 'End Waltz', 'End Waltz'], 'slam', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('dynasmon', 5, ['RK', 'Bruiser'], 'Data', ['Dynasmon', 'Dynasmon', 'Dynasmon'], ['Breath of Wyvern', 'Breath of Wyvern', 'Breath of Wyvern'], 'aoe', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('magnamon', 5, ['RK', 'Guardian'], 'Free', ['Magnamon', 'Magnamon', 'Magnamon'], ['Extreme Jihad', 'Extreme Jihad', 'Extreme Jihad'], 'shield', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Armor', 'Armor', 'Armor'] }),
    L('diaboromon', 5, ['NSo', 'Assassin'], 'Virus', ['Diaboromon', 'Diaboromon', 'Diaboromon'], ['Catastrophe Cannon', 'Catastrophe Cannon', 'Catastrophe Cannon'], 'nuke', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('armageddemon', 5, ['NSo', 'Mystic'], 'Virus', ['Armageddemon', 'Armageddemon', 'Armageddemon'], ['Ultimate Flare', 'Ultimate Flare', 'Ultimate Flare'], 'aoe', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('seraphimon', 5, ['VB', 'Mystic'], 'Vaccine', ['Seraphimon', 'Seraphimon', 'Seraphimon'], ['Seven Heavens', 'Seven Heavens', 'Seven Heavens'], 'aoe', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('ophanimon', 5, ['VB', 'Support'], 'Vaccine', ['Ophanimon', 'Ophanimon', 'Ophanimon'], ["Eden's Javelin", "Eden's Javelin", "Eden's Javelin"], 'line', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('magnadramon', 5, ['HB', 'Mystic'], 'Vaccine', ['Magnadramon', 'Magnadramon', 'Magnadramon'], ['Fire Tornado', 'Fire Tornado', 'Fire Tornado'], 'aoe', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('leviamon', 5, ['DL', 'Bruiser'], 'Virus', ['Leviamon', 'Leviamon', 'Leviamon'], ['Rostrum', 'Rostrum', 'Rostrum'], 'drain', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, levels: ['Mega', 'Mega', 'Mega'] }),
    L('barbamon', 5, ['DL', 'Mystic'], 'Virus', ['Barbamon', 'Barbamon', 'Barbamon'], ['Pandemonium Lost', 'Pandemonium Lost', 'Pandemonium Lost'], 'aoe', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
    L('belphemon', 5, ['DL', 'Striker'], 'Virus', ['Belphemon RM', 'Belphemon RM', 'Belphemon RM'], ['Lampranthus', 'Lampranthus', 'Lampranthus'], 'slam', { shop: false, boss: true, hpMul: 1.5, adMul: 1.25, radius: 2, levels: ['Mega', 'Mega', 'Mega'] }),
  ];


  // Wild rounds: [lineId, stage, col, row] in the enemy's own coordinates (rows 4-7, 4 = front). Stage 1 has wild
  // rounds at 1-2 and 1-3; stage 3 is Toilet Town (Numemon, Sukamon and friends); later stages are boss rounds.
  const pveWave = function (stage, round) {
    if (stage === 1) {
      round -= 1; // 1-2 is the first wild round
      if (round === 1) return [['goblimon', 0, 2, 4], ['goblimon', 0, 4, 4]];
      if (round === 2) return [['goblimon', 0, 2, 4], ['goblimon', 0, 3, 4], ['goblimon', 0, 4, 4]];
      return [['goblimon', 0, 2, 4], ['goblimon', 0, 4, 4], ['kuwagamon', 0, 3, 5]];
    }
    if (stage === 2) return [['kuwagamon', 1, 2, 4], ['kuwagamon', 1, 4, 4], ['ogremon', 0, 3, 4], ['tyrannomon', 0, 3, 6]];
    if (stage === 3) return [['numemon', 1, 3, 4], ['sukamon', 1, 2, 4], ['sukamon', 1, 4, 4], ['bakemon', 1, 0, 6], ['nanimon', 1, 3, 6], ['frigimon', 0, 6, 5]];
    if (stage === 4) return [['ogremon', 2, 3, 4], ['tyrannomon', 1, 1, 4], ['tyrannomon', 1, 5, 4], ['kimeramon', 1, 3, 6], ['etemon', 1, 2, 7], ['etemon', 1, 4, 7]];
    // Dark Masters
    if (stage === 5) return [['ogremon', 2, 3, 4], ['tyrannomon', 2, 1, 4], ['tyrannomon', 2, 5, 4], ['machinedramon', 1, 3, 5], ['woodmon', 2, 2, 6], ['metalseadramon', 1, 4, 7], ['piedmon', 1, 6, 6]];
    // Royal Knights
    if (stage === 6) return [['craniamon', 1, 3, 4], ['dynasmon', 1, 2, 4], ['magnamon', 1, 4, 4], ['leopardmon', 1, 3, 5], ['crusadermon', 1, 0, 6], ['skullgreymon', 2, 5, 7]];
    // Diaboromon and Armageddemon
    if (stage === 7) return [['skullgreymon', 2, 2, 4], ['skullgreymon', 2, 4, 4], ['diaboromon', 1, 1, 6], ['diaboromon', 1, 5, 6], ['armageddemon', 1, 3, 6], ['etemon', 2, 2, 7], ['etemon', 2, 4, 7]];
    // the Celestial Digimon
    if (stage === 8) return [['monzaemon', 2, 3, 4], ['skullgreymon', 2, 2, 4], ['magnadramon', 1, 4, 5], ['seraphimon', 1, 2, 7], ['ophanimon', 1, 4, 7], ['phoenixmon', 1, 3, 6]];
    // Demon Lords
    return [['leviamon', 1, 3, 4], ['belphemon', 1, 2, 4], ['daemon', 1, 4, 4], ['barbamon', 1, 3, 7], ['lucemon', 2, 2, 6], ['beelzemon', 2, 4, 7], ['skullgreymon', 2, 5, 5]];
  };


  // ---------- items ----------
  // stat keys: ad (fraction of base AD), as (fraction), ap (flat), mana (flat start), armor, mr, hp (flat), crit (fraction)
  const components = {
    sword: { name: 'Power Chip', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    bow: { name: 'Speed Chip', short: 'SC', color: '#ffd23f', stats: { as: 0.12 } },
    rod: { name: 'Data Rod', short: 'DR', color: '#b26bff', stats: { ap: 15 } },
    tear: { name: 'Mana Card', short: 'MC', color: '#4fb3ff', stats: { mana: 15 } },
    vest: { name: 'Chrome Plate', short: 'CP', color: '#c0c6d0', stats: { armor: 20 } },
    cloak: { name: 'Data Cloak', short: 'DC', color: '#5fe0c0', stats: { mr: 20 } },
    belt: { name: 'HP Disk', short: 'HD', color: '#ff5d73', stats: { hp: 150 } },
    glove: { name: 'Lucky Glove', short: 'LG', color: '#9be15d', stats: { crit: 0.15 } },
    egg: { name: 'Digi-Egg', short: 'EG', color: '#fff3b0', stats: {}, special: true,
      desc: 'Combine with a component to make a trait Crest, or with another Digi-Egg for +1 team size.' },
  };

  // Completed items. p = passives read by combat.js (see FORMAT.md), extra = stats on top of the two components.
  const I = (a, b, name, desc, p = null, extra = {}) => ({ a, b, name, desc, p, extra });
  const recipes = [
    I('sword', 'sword', 'Omni Blade', 'Deals 12% more damage.', { amp: 0.12 }, { ad: 0.15 }),
    I('sword', 'bow', 'Giga Slayer', 'Deals 25% more damage to enemies with over 1600 max HP.', { slayer: 0.25 }),
    I('sword', 'rod', 'Data Gunblade', 'Heals for 22% of all damage dealt.', { vamp: 0.22 }),
    I('sword', 'tear', 'Mana Spear', 'Attacks restore 5 extra mana.', { shojin: 5 }),
    I('sword', 'vest', 'Night Edge', 'The first time HP drops below 60%, gain a 30% max HP shield and 20% attack speed.', { lifeline: { at: 0.6, shield: 0.3, as: 0.2 } }),
    I('sword', 'cloak', 'Blood Fang', 'Heals for 18% of damage dealt. Below 40% HP, gain a 25% max HP shield once.', { vamp: 0.18, lifeline: { at: 0.4, shield: 0.25 } }),
    I('sword', 'belt', 'Titan Gauntlet', 'The first time HP drops below 60%, gain a 25% max HP shield and 35% AD.', { lifeline: { at: 0.6, shield: 0.25, ad: 0.35 } }),
    I('sword', 'glove', 'Crit Edge', 'Abilities can critically strike. +10% crit damage.', { abilCrit: true, critDmg: 0.1 }),
    I('bow', 'bow', 'Rage Wing', 'Each attack grants 5% attack speed, stacking.', { rage: 0.05 }),
    I('bow', 'rod', 'Wild Rush', 'After casting, gain 50% attack speed for 5s.', { castAS: 0.5 }),
    I('bow', 'tear', 'Volt Disk', 'Every 3rd attack sends lightning to 3 enemies for 80 magic damage.', { volt: 80 }),
    I('bow', 'vest', 'Titan Will', 'Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks.', { titan: true }),
    I('bow', 'cloak', 'Twin Shot', 'Attacks also hit a second nearby enemy for 50% damage.', { split: 0.5 }),
    I('bow', 'belt', 'Kraken Fang', 'Each attack grants 3% AD, stacking.', { kraken: 0.03 }),
    I('bow', 'glove', 'Armor Breaker', 'Attacks cut the target\'s armor by 30% for 3s.', { shred: 0.3 }),
    I('rod', 'rod', 'Ultima Rod', 'A lot of ability power.', null, { ap: 40 }),
    I('rod', 'tear', 'Seraph Rod', 'Gain 15 AP every 5 seconds.', { archangel: 15 }),
    I('rod', 'vest', 'Holy Barrier', 'Start combat with a 25% max HP shield. After 8s, gain 30 AP.', { crown: true }),
    I('rod', 'cloak', 'Mana Zap', 'When an enemy within 2 hexes casts, deal 150 magic damage to it.', { zap: 150 }),
    I('rod', 'belt', 'Flame Tome', 'Damage burns the target for 1.5% max HP per second and cuts its healing by a third.', { burn: true }),
    I('rod', 'glove', 'Jewel Gauntlet', 'Abilities can critically strike. +15% crit damage.', { abilCrit: true, critDmg: 0.15 }),
    I('tear', 'tear', 'Mana Overflow', 'Max mana -10. After casting, keep 20 mana.', { bluebuff: true }),
    I('tear', 'vest', 'Guard Vow', 'Below 40% HP, gain a 25% max HP shield and 20 armor/MR once.', { lifeline: { at: 0.4, shield: 0.25, armor: 20 } }),
    I('tear', 'cloak', 'Adaptive Helm', 'Gain 10 mana every 3 seconds.', { manaRegen: 10 }),
    I('tear', 'belt', 'Holy Ring', 'Every 5s, heals allies within 2 hexes for 80 + 10% of missing HP.', { redemption: true }),
    I('tear', 'glove', 'Justice Hand', 'Deals 12% more damage and heals for 12% of damage dealt.', { amp: 0.12, vamp: 0.12 }),
    I('vest', 'vest', 'Spike Plate', 'When attacked, deals 40 magic damage to adjacent enemies (1.5s cooldown).', { thorns: 40 }),
    I('vest', 'cloak', 'Stone Hide', 'Gain 10 armor and MR for each enemy within 2 hexes.', { gargoyle: 10 }),
    I('vest', 'belt', 'Flame Core', 'Every 2s, burns the nearest enemy within 2 hexes.', { sunfire: true }),
    I('vest', 'glove', 'Steadfast Heart', 'Takes 12% less damage.', { dr: 0.12 }),
    I('cloak', 'cloak', 'Dragon Scale', 'Regenerates 1.2% max HP per second.', { regen: 0.012 }),
    I('cloak', 'belt', 'Shadow Veil', 'Enemies within 2 hexes lose 25 armor and MR.', { veil: 25 }),
    I('cloak', 'glove', 'Quick Silver', 'Immune to stuns for the first 15 seconds.', { ccImmune: 15 }),
    I('belt', 'belt', 'Giga Belt', 'A lot of HP.', null, { hp: 400 }),
    I('belt', 'glove', 'Guard Breaker', 'Deals 25% more damage to shielded enemies.', { guardbreak: 0.25 }),
    I('glove', 'glove', 'Thief Glove', 'Each combat, gains two random completed items.', { thief: true }),
  ];
  // Digi-Egg + component = Crest (emblem: adds a trait to the holder).
  const emblems = { sword: 'Striker', bow: 'Sniper', rod: 'Mystic', tear: 'VB', vest: 'Guardian', cloak: 'NSo', belt: 'DR', glove: 'Assassin' };
  // Crests with no recipe: found in the Armory, gold loot orbs and the Crest Wisp, like TFT's Tome of Traits emblems.
  const dropEmblems = ['DATS', 'LW', 'XF', 'INS', 'FUN', 'KN', 'FL', 'FR', 'UND', 'HB', 'FA'];

  // What the AI builds for each job, best first.
  const aiRoles = {
    ap: ['jewel_gauntlet', 'seraph_rod', 'ultima_rod', 'mana_overflow', 'data_gunblade', 'flame_tome', 'mana_spear', 'wild_rush', 'justice_hand', 'adaptive_helm'],
    ad: ['crit_edge', 'giga_slayer', 'rage_wing', 'omni_blade', 'blood_fang', 'kraken_fang', 'armor_breaker', 'twin_shot', 'titan_gauntlet', 'night_edge', 'volt_disk', 'guard_breaker', 'thief_glove'],
    tank: ['spike_plate', 'dragon_scale', 'stone_hide', 'flame_core', 'steadfast_heart', 'guard_vow', 'holy_barrier', 'giga_belt', 'shadow_veil', 'holy_ring', 'quick_silver', 'titan_will', 'mana_zap'],
  };

  // ---------- augments ----------
  // tier: 1 silver, 2 gold, 3 prismatic. Effects are hooks in faction-wars-mech.js; tags tell the AI what kind it is.
  const A = (id, tier, name, desc) => ({ id, tier, name, desc });
  const augments = [
    A('pocket', 1, 'Spare Change', 'Gain 8 gold.'),
    A('cache', 1, 'Item Cache', 'Gain 2 random components.'),
    A('rolls', 1, 'Rolling Start', 'Gain 1 free reroll every round.'),
    A('heart', 1, "Tamer's Heart", 'Gain 5 Tamer HP.'),
    A('training', 1, 'Combat Training', 'Your team gains 12% AD and 12 AP.'),
    A('thrill', 1, 'Thrill of the Hunt', 'When your Digimon score a kill, they heal 350 HP.'),
    A('secondWind', 1, 'Second Wind', 'After 10 seconds of combat, your team heals 35% of missing HP.'),
    A('featherweights', 1, 'Featherweights', 'Your 1 and 2-cost Digimon gain 25% attack speed.'),
    A('frontline', 1, 'Frontline Fortitude', 'Digimon in your front row gain 30 armor/MR and 200 HP.'),
    A('backline', 1, 'Backline Barrage', 'Digimon in your back two rows gain 25% attack speed.'),
    A('bank', 2, 'Digi-Bank', 'Gain 18 gold.'),
    A('trove', 2, 'Treasure Trove', 'Gain a random completed item and a component.'),
    A('egg', 2, 'Egg Hunt', 'Gain a Digi-Egg.'),
    A('rolls2', 2, 'Reroll Frenzy', 'Gain 2 free rerolls every round.'),
    A('income', 2, 'Steady Income', 'Gain 2 extra gold every round.'),
    A('evolve', 2, 'Instant Digivolve', 'Gain a random Champion-form (★★) Digimon of cost 2-3.'),
    A('blessing', 2, 'Celestial Blessing', 'Your team heals for 12% of damage dealt.'),
    A('lotus', 2, 'Jeweled Lotus', 'Your abilities can critically strike, and your team gains 15% crit.'),
    A('implants', 2, 'Cybernetic Implants', 'Your Digimon holding items gain 250 HP and 15% AD.'),
    A('unity', 2, 'Stand United', 'Your team gains 3% AD and 3 AP per active trait.'),
    A('overcharge', 2, 'Overcharged', 'Your team starts combat with 25 extra mana.'),
    A('jackpot', 3, 'Jackpot', 'Gain 35 gold.'),
    A('armory', 3, 'Big Armory', 'Gain 2 random completed items.'),
    A('goldenEgg', 3, 'Golden Egg', 'Gain a Golden Digi-Egg (+1 team size).'),
    A('megaEvolve', 3, 'Mega Digivolve', 'Gain a random Champion-form (★★) Digimon of cost 4.'),
    A('giant', 3, 'Giant Grab', 'Your team gains 25% max HP.'),
    A('crest', 3, 'Crest of Courage', 'Every family trait you have active counts one extra Digimon.'),
  ];

  // ---------- Data Wisps (Set 18 style shop visitors; the set mechanic, run by faction-wars-mech.js) ----------
  // cost is base + stage. kind is shown as a category label.
  // what kind of augment each one is, for the AI (augments without a tag count as combat)
  const AUG_TAGS = {
    econ: ['pocket', 'bank', 'jackpot', 'income', 'rolls', 'rolls2', 'evolve', 'megaEvolve'],
    items: ['cache', 'trove', 'armory', 'egg', 'goldenEgg', 'implants'],
  };
  for (const a of augments) a.tags = AUG_TAGS.econ.includes(a.id) ? ['econ'] : AUG_TAGS.items.includes(a.id) ? ['items'] : ['combat'];

  const W = (id, kind, base, name, desc, minStage = 2) => ({ id, kind, base, name, desc, minStage });
  const wisps = [
    W('gold', 'Gold', 0, 'Gold Wisp', 'Gain double this Wisp\'s cost in gold.'),
    W('comp', 'Item', 0, 'Item Wisp', 'Gain a random component.'),
    W('egg', 'Item', 2, 'Egg Wisp', 'Gain a Digi-Egg.', 3),
    W('copy', 'Champion', 0, 'Copy Wisp', 'Gain a copy of a random Rookie-form Digimon you own.'),
    W('evolve', 'Champion', 3, 'Evolve Wisp', 'Your cheapest Rookie-form Digimon on the board digivolves.', 3),
    W('level', 'Misc', 4, 'Leap Wisp', 'Permanently gain +1 team size.', 3),
    W('battle', 'Combat', 0, 'Battle Wisp', 'Next combat, your team gains 30% attack speed.'),
    W('iron', 'Combat', 0, 'Iron Wisp', 'Next combat, your team gains 35 armor and MR.'),
    W('mana', 'Combat', 0, 'Mana Wisp', 'Next combat, your team starts with 40 extra mana.'),
    W('heal', 'Misc', 0, 'Heal Wisp', 'Restore 3 Tamer HP.'),
    W('gamble', 'Risky', 0, 'Risky Wisp', '50% chance to gain triple this Wisp\'s cost in gold. Otherwise nothing.'),
    W('pact', 'Risky', 0, 'Blood Wisp', 'Lose 3 Tamer HP. Gain a random completed item.'),
    W('rolls', 'Shop', 0, 'Shop Wisp', 'Gain 4 free rerolls this round.'),
    W('crest', 'Item', 3, 'Crest Wisp', 'Gain a Crest for a family trait on your board.', 3),
    W('fortune', 'Shop', 1, 'Fortune Wisp', 'Your shop refreshes with odds two levels higher.'),
  ];

  DT.Sets.register({
    id: 'faction-wars',
    name: 'Faction Wars',
    mode: 'tft',
    order: 1,
    desc: 'The original roster: Digi-Destined partners, Tamers, Savers, Frontier and Xros Wars lines, with Royal Knights and Demon Lords at 5 cost. Hyper Roll rules and Data Wisps.',
    colors: { main: '#ffb02e', accent: '#4fb3ff' },
    source: 'TFT Set 1 Faction Wars (56 of 57 champions) and 4 Dota Auto Chess pieces. docs/sets/set-01-file-island.md, docs/inspo/tft/set-01-faction-wars.md',
    modelsById: true, // models/index.json keys are "<lineId>:<stage>"
    traits: TRAITS,
    lines: LINES,
    pve: pveWave,
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'egg',
      emblems,
      dropEmblems,
      dropEmblemSource: 'the Armory, gold loot orbs and the Crest Wisp', // codex: where drop-only Crests come from
      emblemName: (T) => `${T.name} Crest`,
      special: [
        { id: 'golden_egg', name: 'Golden Digi-Egg', desc: '+1 team size while equipped on a Digimon on your board.', p: { teamSize: 1 }, stats: {},
          from: ['egg', 'egg'], color: '#ffd84a', color2: '#fff3b0', short: 'GE' },
      ],
      // Consumables: used up when dragged onto a Digimon (their use() is in faction-wars-mech.js).
      list: [
        { id: 'remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
          desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
        { id: 'reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', stats: {}, p: {},
          desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
      ],
      aiRoles,
    },
    augments,
    wisps,
    // Hyper Roll. These equal js/sets.js DEFAULT_RULES and are listed so this example shows the main economy knobs.
    rules: {
      startHp: 20,
      startGold: 3,
      planTime: 30,
      itemCap: 10,
      rerollCost: 1,
      winGold: 2,
      shopSize: 5,
      // Copies of each Digimon in the shared pool by cost, matching current TFT (30/25/18/10/9).
      poolSize: [0, 30, 25, 18, 10, 9],
      shopOdds: [
        null,
        [100, 0, 0, 0, 0],
        [100, 0, 0, 0, 0],
        [75, 25, 0, 0, 0],
        [55, 30, 15, 0, 0],
        [45, 33, 20, 2, 0],
        [30, 40, 25, 5, 0],
        [19, 30, 40, 10, 1],
        [18, 25, 32, 22, 3],
        [15, 20, 25, 30, 10],
      ],
      levelByStage: [0, 3, 4, 5, 6, 7, 8, 9], // no XP: everyone levels up at the start of each stage
      incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9], // fixed income, no interest, no streaks
      lossDamage: (stage) => (stage <= 4 ? 2 : stage <= 7 ? 4 : 6),
      pveLossDamage: (stage) => (stage === 1 ? 1 : null),
      roundsInStage: (stage) => (stage === 1 ? 3 : 4),
      roundType: (stage, round) => (stage === 1 ? (round === 1 ? 'carousel' : 'pve') : round === 4 ? 'pve' : 'pvp'),
      armoryStages: [2, 3, 4, 5, 6],
      augmentStages: [2, 3, 4],
      copies: 3,
    },
    tamers: ['Tai', 'Matt', 'Sora', 'Izzy', 'Mimi', 'Joe', 'Kari'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
