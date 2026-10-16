// Galaxies: TFT Set 3 "Galaxies" and Set 3.5 "Return to the Stars", on Digimon. 60 shop lines (13/13/13/11/10),
// 26 traits (15 origins, 11 classes), 8 components plus the Digimental, 36 recipes, 8 crafted and 10 drop-only
// Crests, the Golden Digimental, 3 consumables, 21 augments and 15 Galaxies. Behavior (traits, abilities, items,
// augments and the Galaxy mechanic) is in galaxies-mech.js; item art and move looks in galaxies-art.js.
// Design notes and balance results: docs/sets/galaxies.md. Corpus: docs/inspo/tft/set-03*.md.
(function (G) {
  const DT = G.DT;
  const pct = DT.pct;

  // ---------- traits ----------
  // bp = unit counts per breakpoint, lv = value per breakpoint. Extra per-breakpoint arrays are read with c.val(key).
  const TRAITS = {
    // ---- origins ----
    DD: { name: 'DigiDestined', kind: 'origin', color: '#ff9ad5', bp: [3, 6], lv: [12, 25], crestSymbol: 'star6',
      desc: 'When a DigiDestined Digimon casts its ability, the other DigiDestined share bonus mana.',
      fmt: (v) => `${v} mana shared per cast` },
    DA: { name: 'Dark Area', kind: 'origin', color: '#8a4dff', bp: [2, 4, 6], lv: [14, 25, 38], crestSymbol: 'skull',
      desc: 'When a Dark Area Digimon is knocked out, the other Dark Area Digimon gain attack damage and ability power.',
      fmt: (v) => `+${v}% AD and +${v} AP per fallen ally` },
    XR: { name: 'DigiXros', kind: 'origin', color: '#ff6a3d', bp: [3], lv: [0.9], crestSymbol: 'xros',
      desc: 'At the start of combat, three DigiXros Digimon fuse into one. The fusion has most of their combined health and attack damage. When it falls, the three are ejected and keep fighting.',
      fmt: (v) => `fusion with ${pct(v)} of combined HP and AD` },
    PI: { name: 'Data Pirates', kind: 'origin', color: '#ffb347', bp: [2, 4], lv: [0.5, 0.5], comp: [0, 0.3], crestSymbol: 'coin',
      desc: 'When a Data Pirate knocks out an enemy in a PvP round, it has a chance to drop gold after combat. With 4, it can also drop a component.',
      fmt: (v, i) => `${pct(v)} for 1 gold${i ? ', 30% for a component' : ''}` },
    CE: { name: 'Celestials', kind: 'origin', color: '#ffe680', bp: [2, 4], lv: [0.07, 0.16], crestSymbol: 'cross',
      desc: 'All your Digimon heal for a share of the damage they deal.',
      fmt: (v) => `${pct(v)} healing from damage` },
    TK: { name: 'Time Keepers', kind: 'origin', color: '#7fd8ff', bp: [2, 4, 6], lv: [8, 4, 2], crestSymbol: 'refresh',
      desc: 'All your Digimon gain 12% attack speed every few seconds, stacking.',
      fmt: (v) => `+12% AS every ${v}s` },
    CY: { name: 'Cyborgs', kind: 'origin', color: '#9fb4c8', bp: [3, 6], lv: [130, 260], ad: [0.1, 0.2], crestSymbol: 'chip',
      desc: 'Cyborgs gain max health and attack damage. A Cyborg holding at least one item gains double.',
      fmt: (v, i) => `+${v} HP and +${pct(TRAITS.CY.ad[i])} AD (x2 with an item)` },
    RE: { name: 'Resistance', kind: 'origin', color: '#ff5a5a', bp: [3, 6], lv: [300, 420], amp: [0.2, 0.26], crestSymbol: 'shield',
      desc: 'At the start of combat, Resistance Digimon gain a shield and deal more damage for each other Resistance Digimon next to them.',
      fmt: (v, i) => `${v} shield and +${pct(TRAITS.RE.amp[i])} damage per adjacent ally` },
    AB: { name: 'Abyss', kind: 'origin', color: '#5a3aa8', bp: [3], lv: [0.45], crestSymbol: 'pearl',
      desc: "Abyss Digimon's attacks and abilities ignore part of the target's armor and magic resist.",
      fmt: (v) => `damage ignores ${pct(v)} of armor and MR` },
    VA: { name: 'Valkyries', kind: 'origin', color: '#ff7ab8', bp: [2], lv: [0.5], crit: [0.25], crestSymbol: 'dagger',
      desc: "Valkyries gain critical strike chance, and their attacks and abilities always critically strike enemies below half health.",
      fmt: (v, i) => `+${pct(TRAITS.VA.crit[i])} crit, always crit below ${pct(v)} HP` },
    AS: { name: 'Astro', kind: 'origin', color: '#4f7dff', bp: [3], lv: [15], crestSymbol: 'moon',
      desc: "Astro Digimon's abilities cost less mana.",
      fmt: (v) => `-${v} max mana` },
    OV: { name: 'Overload', kind: 'origin', color: '#c8ff4a', bp: [2, 4, 6], lv: [120, 260, 520], crestSymbol: 'up',
      desc: 'After dealing or taking 10 hits, an Overload Digimon blasts the nearest enemy for magic damage. Below half health, it heals itself instead.',
      fmt: (v) => `${v} damage or healing every 10 hits` },
    ARK: { name: 'Ark', kind: 'origin', color: '#9ad8ff', bp: [1], lv: [30], crestSymbol: 'chevrons',
      desc: 'The Ark gains mana every second and cannot be stunned, but it does not make basic attacks.',
      fmt: (v) => `+${v} mana per second` },
    SV: { name: 'Sovereign', kind: 'origin', color: '#5fd6c8', bp: [1], lv: [1], crestSymbol: 'sun',
      desc: "Your team's basic attacks deal magic damage instead of physical. DigiDestined attacks deal true damage.",
      fmt: () => 'attacks deal magic damage (true for DigiDestined)' },
    BH: { name: 'Bounty Hunters', kind: 'origin', color: '#d9a15a', bp: [1], lv: [6], crestSymbol: 'coin',
      desc: 'Every copy of a Bounty Hunter you buy makes that Digimon stronger for the rest of the game: +6 ability power and +3% attack damage per copy, up to 9 copies.',
      fmt: (v) => `+${v} AP and +3% AD per copy bought` },
    // ---- classes ----
    BM: { name: 'Blade Masters', kind: 'class', color: '#e0e6f0', bp: [3, 6], lv: [0.3, 0.55], crestSymbol: 'sword',
      desc: "Blade Masters' basic attacks have a chance to strike two more times.",
      fmt: (v) => `${pct(v)} chance for 2 extra strikes` },
    GU: { name: 'Gunners', kind: 'class', color: '#ffcf4a', bp: [2, 4], lv: [4, 7], crestSymbol: 'crosshair',
      desc: 'Every fourth basic attack of a Gunner also fires extra shots at random enemies.',
      fmt: (v) => `${v} extra shots` },
    BR: { name: 'Brawlers', kind: 'class', color: '#d9773a', bp: [2, 4], lv: [200, 420], crestSymbol: 'shieldIron',
      desc: 'Brawlers gain max health.', fmt: (v) => `+${v} HP` },
    BO: { name: 'Bombers', kind: 'class', color: '#ff8a2a', bp: [2], lv: [1.5], crestSymbol: 'flame',
      desc: "Damage from a Bomber's ability stuns its targets, once per cast.",
      fmt: (v) => `ability hits stun for ${v}s` },
    IN: { name: 'Infiltrators', kind: 'class', color: '#c45cff', bp: [2, 4, 6], lv: [0.3, 0.6, 1.1], crestSymbol: 'dagger',
      desc: 'Infiltrators always leap to the enemy backline when combat starts. With the trait, they gain attack speed for 6 seconds at the start of combat, refreshed on each takedown.',
      fmt: (v) => `+${pct(v)} AS for 6s` },
    HA: { name: 'Hackers', kind: 'class', color: '#3fe0b0', bp: [2], lv: [0.5], crestSymbol: 'xros',
      desc: "A Hacker's basic attacks raise the mana cost of the target's next ability.",
      fmt: (v) => `+${pct(v)} mana cost` },
    MY: { name: 'Mystics', kind: 'class', color: '#d48bff', bp: [2, 4], lv: [35, 80], crestSymbol: 'drop',
      desc: 'All your Digimon gain magic resist.', fmt: (v) => `+${v} MR` },
    PR: { name: 'Protectors', kind: 'class', color: '#c9a66b', bp: [2, 4], lv: [0.15, 0.25], crestSymbol: 'shield',
      desc: 'Protectors gain a shield for 4 seconds when they cast their ability. It does not stack.',
      fmt: (v) => `${pct(v)} max HP shield on cast` },
    SN: { name: 'Snipers', kind: 'class', color: '#5ad1ff', bp: [2, 4], lv: [0.06, 0.12], crestSymbol: 'crosshair',
      desc: 'Snipers deal more damage for each hex between them and their target.',
      fmt: (v) => `+${pct(v)} damage per hex` },
    SO: { name: 'Sorcerers', kind: 'class', color: '#b26bff', bp: [2, 4, 6], lv: [18, 35, 60], crestSymbol: 'star5',
      desc: 'All your Digimon gain ability power.', fmt: (v) => `+${v} AP` },
    VG: { name: 'Vanguards', kind: 'class', color: '#a0a8b8', bp: [2, 4], lv: [60, 140], crestSymbol: 'shieldIron',
      desc: 'Vanguards gain armor.', fmt: (v) => `+${v} armor` },
  };
  // breakpoint text from the numbers, so tuning never leaves stale text
  for (const T of Object.values(TRAITS)) T.bpDesc = T.lv.map((v, i) => T.fmt(v, i));

  // ---------- lines ----------
  // L(id, tier, traits, attr, forms, abilities, abilityType, options). Models are assigned in galaxies-models.js. Each line
  // records the corpus champion it adapts in `src`. Ability types not built into the engine are in galaxies-mech.js.
  const L = (id, tier, traits, attr, forms, abil, type, opt) => DT.line(id, tier, traits, attr, forms, abil, type, { ...opt });
  const LV = { rcu: ['Rookie', 'Champion', 'Ultimate'], rcm: ['Rookie', 'Champion', 'Mega'], rum: ['Rookie', 'Ultimate', 'Mega'], umm: ['Ultimate', 'Mega', 'Mega'], mmm: ['Mega', 'Mega', 'Mega'] };
  const LINES = [
    // ---- 1 cost (13) ----
    L('gal_hawkmon', 1, ['TK', 'SN'], 'Free', ['Hawkmon', 'Aquilamon', 'Silphymon'], ['Feather Slash', 'Blast Laser', 'Top Gun'], 'snipe', { role: 'Sniper', mana: 90, adMul: 1.1, src: 'TFT3 Caitlyn' }),
    L('gal_kotemon', 1, ['CY', 'BM'], 'Data', ['Kotemon', 'Raptordramon', 'Grademon'], ['Meteor Slash', 'Crash Charge', 'Grade Slash'], 'riposte', { role: 'Duelist', src: 'TFT3 Fiora' }),
    L('gal_gazimon', 1, ['PI', 'GU'], 'Virus', ['Gazimon', 'Gururumon', 'Astamon'], ['Electric Stun Blast', 'Chaos Fire', 'Hellfire'], 'smoke', { role: 'Striker', startMana: 30, src: 'TFT3 Graves' }),
    L('gal_syakomon', 1, ['OV', 'BR'], 'Virus', ['Syakomon', 'Whamon', 'Dragomon'], ['Black Pearl Blast', 'Tidal Wave', 'Thousand Whips'], 'sap', { role: 'Bruiser', src: 'TFT3.5 Illaoi' }),
    L('gal_blackgabumon', 1, ['DA', 'PR'], 'Virus', ['BlackGabumon', 'BlackGarurumon', 'WereGarurumon'], ['Little Horn', 'Freeze Fang', 'Garuru Kick'], 'rally', { role: 'Guardian', src: 'TFT3 Jarvan IV' }),
    L('gal_keramon', 1, ['AB', 'IN'], 'Free', ['Keramon', 'Chrysalimon', 'Infermon'], ['Crazy Giggle', 'Data Crusher', "Hell's Grenade"], 'isolate', { role: 'Assassin', src: "TFT3 Kha'Zix" }),
    L('gal_armadillomon', 1, ['RE', 'BR'], 'Free', ['Armadillomon', 'Ankylomon', 'Shakkoumon'], ['Diamond Shell', 'Megaton Press', 'Justice Beam'], 'shield', { role: 'Bruiser', src: 'TFT3 Malphite' }),
    L('gal_tapirmon', 1, ['OV', 'IN'], 'Vaccine', ['Tapirmon', 'Bakemon', 'Phantomon'], ['Nightmare Syndrome', "Hell's Hand", 'Soul Chopper'], 'stun', { role: 'Assassin', hpMul: 1.1, adMul: 1.1, src: 'TFT3.5 Nocturne' }),
    L('gal_gomamon', 1, ['DD', 'VG'], 'Vaccine', ['Gomamon', 'Ikkakumon', 'Zudomon'], ['Marching Fishes', 'Harpoon Torpedo', "Vulcan's Hammer"], 'toss', { role: 'Guardian', src: 'TFT3 Poppy' }),
    L('gal_solarmon', 1, ['TK', 'SO'], 'Vaccine', ['Solarmon', 'Clockmon', 'Wisemon'], ['Shiny Ring', 'Chrono Breaker', 'Pandora Dialogue'], 'line', { role: 'Mystic', hpMul: 1.15, src: 'TFT3 Twisted Fate' }),
    L('gal_hyokomon', 1, ['CE', 'BM'], 'Vaccine', ['Hyokomon', 'Piddomon', 'Angewomon'], ['Downward Cleave', 'Fire Phaser', 'Celestial Arrow'], 'plumage', { role: 'Sniper', range: 3, src: 'TFT3 Xayah' }),
    L('gal_chuumon', 1, ['RE', 'BO'], 'Virus', ['Chuumon', 'Mamemon', 'MetalMamemon'], ['Cheese Bomb', 'Smiley Bomb', 'Energy Bomb'], 'bomb', { role: 'Mystic', levels: ['Rookie', 'Ultimate', 'Ultimate'], src: 'TFT3 Ziggs' }),
    L('gal_patamon', 1, ['DD', 'SO'], 'Data', ['Patamon', 'Angemon', 'MagnaAngemon'], ['Boom Bubble', 'Hand of Fate', 'Gate of Destiny'], 'bubble', { role: 'Mystic', src: 'TFT3 Zoe' }),
    // ---- 2 cost (13) ----
    L('gal_renamon', 2, ['DD', 'SO'], 'Data', ['Renamon', 'Kyubimon', 'Taomon'], ['Diamond Storm', 'Fox Tail Inferno', 'Talisman of Light'], 'orb', { role: 'Mystic', src: 'TFT3 Ahri' }),
    L('gal_shoutmon', 2, ['XR', 'SO'], 'Data', ['Shoutmon', 'Ballistamon', 'Shoutmon X3'], ['Bellow Blaster', 'Hard Rock Soul', 'Flame Cannon'], 'fireshield', { role: 'Mystic', range: 2, hpMul: 1.2, levels: ['Rookie', 'Champion', 'Ultimate'], src: 'TFT3 Annie' }),
    L('gal_kokuwamon', 2, ['TK', 'BR'], 'Data', ['Kokuwamon', 'Guardromon', 'Andromon'], ['Electro Strike', 'Destruction Grenade', 'Spiral Sword'], 'grab', { role: 'Bruiser', startMana: 40, src: 'TFT3 Blitzcrank' }),
    L('gal_gizamon', 2, ['PI', 'HA'], 'Virus', ['Gizamon', 'Gesomon', 'Boltmon'], ['Spiral Edge', 'Coral Crusher', 'Tomahawk Stinger'], 'dunk', { role: 'Bruiser', hpMul: 1.1, levels: LV.rcm, src: 'TFT3 Darius' }),
    L('gal_falcomon', 2, ['VA', 'IN'], 'Vaccine', ['Falcomon', 'Peckmon', 'Crowmon'], ['Scratch Smash', 'Kunai Wing', 'Haguro'], 'barrage', { role: 'Assassin', range: 2, shots: 6, adMul: 1.1, src: "TFT3 Kai'Sa" }),
    L('gal_betamon', 2, ['OV', 'GU'], 'Virus', ['Betamon', 'Seadramon', 'MegaSeadramon'], ['Electric Shock', 'Ice Blast', 'Lightning Javelin'], 'frenzy', { role: 'Sniper', mana: 40, src: "TFT3.5 Kog'Maw" }),
    L('gal_commandramon', 2, ['CY', 'GU'], 'Data', ['Commandramon', 'Sealsdramon', 'Darkdramon'], ['M16 Assassin', 'Death Behind', 'Gungnir'], 'dashshot', { role: 'Sniper', mana: 45, levels: LV.rcm, src: 'TFT3 Lucian' }),
    L('gal_tsukaimon', 2, ['DA', 'VG'], 'Virus', ['Tsukaimon', 'Devimon', 'SkullSatamon'], ['Bad Message', 'Death Claw', 'Skull Hammer'], 'dreadshield', { role: 'Guardian', src: 'TFT3 Mordekaiser' }),
    L('gal_gotsumon', 2, ['AS', 'VG'], 'Data', ['Gotsumon', 'Golemon', 'Meteormon'], ['Rock Fist', 'Golem Punch', 'Galactic Flare'], 'crater', { role: 'Guardian', src: 'TFT3.5 Nautilus' }),
    L('gal_biyomon', 2, ['CE', 'PR'], 'Vaccine', ['Biyomon', 'Birdramon', 'Garudamon'], ['Spiral Twister', 'Meteor Wing', 'Wing Blade'], 'entrance', { role: 'Bruiser', src: 'TFT3 Rakan' }),
    L('gal_kudamon', 2, ['CE', 'PR'], 'Vaccine', ['Kudamon', 'Reppamon', 'Chirinmon'], ['Bullet Whirlwind', 'Lion Claw', 'Wind Cutter Sword'], 'triple', { role: 'Duelist', src: 'TFT3 Xin Zhao' }),
    L('gal_ryudamon', 2, ['RE', 'BM'], 'Vaccine', ['Ryudamon', 'Ginryumon', 'Hisyaryumon'], ['Katana Attack', 'Metal Armor Blade', 'Legendary Dragon Blade'], 'lastbreath', { role: 'Duelist', src: 'TFT3 Yasuo' }),
    L('gal_monodramon', 2, ['RE', 'IN'], 'Vaccine', ['Monodramon', 'Strikedramon', 'Cyberdramon'], ['Crack Bite', 'Fang Strike', 'Desolation Claw'], 'contempt', { role: 'Assassin', mana: 60, src: 'TFT3.5 Zed' }),
    // ---- 3 cost (13) ----
    L('gal_lunamon', 3, ['CE', 'SN'], 'Data', ['Lunamon', 'Lekismon', 'Crescemon'], ['Tearing Shot', 'Tear Arrow', 'Ice Archery'], 'arrow', { role: 'Sniper', mana: 90, startMana: 30, src: 'TFT3 Ashe' }),
    L('gal_starmon', 3, ['AS', 'MY'], 'Data', ['Gammamon', 'Starmon', 'SuperStarmon'], ['Shooting Star', 'Meteor Shower', 'Haley Storm'], 'meep', { role: 'Support', src: 'TFT3.5 Bard' }),
    L('gal_floramon', 3, ['OV', 'MY'], 'Data', ['Floramon', 'Sunflowmon', 'Lotosmon'], ['Rain of Pollen', 'Sunshine Beam', 'Serpent Ruin'], 'poison', { role: 'Mystic', mana: 40, levels: LV.rcm, src: 'TFT3.5 Cassiopeia' }),
    L('gal_agumon', 3, ['TK', 'GU'], 'Vaccine', ['Agumon', 'GeoGreymon', 'RizeGreymon'], ['Pepper Breath', 'Mega Burst', 'Trident Revolver'], 'emp', { role: 'Sniper', range: 3, src: 'TFT3 Ezreal' }),
    L('gal_crabmon', 3, ['PI', 'VG'], 'Data', ['Crabmon', 'Coelamon', 'Plesiomon'], ['Scissors Execution', 'Fossil Bite', 'Sorrow Blue'], 'slam', { role: 'Guardian', hpMul: 1.2, levels: LV.rcm, src: 'TFT3 Jayce' }),
    L('gal_aruraumon', 3, ['DA', 'MY'], 'Data', ['Aruraumon', 'Woodmon', 'Cherrymon'], ['Nemesis Ivy', 'Branch Drain', 'Illusion Mist'], 'rally', { role: 'Support', src: 'TFT3 Karma' }),
    L('gal_elecmon', 3, ['RE', 'BM'], 'Data', ['Elecmon', 'Leomon', 'SaberLeomon'], ['Sparkling Thunder', 'Fist of the Beast King', 'Howling Crusher'], 'chosen', { role: 'Duelist', levels: LV.rcm, src: 'TFT3 Master Yi' }),
    L('gal_lopmon', 3, ['DD', 'PR'], 'Data', ['Lopmon', 'Turuiemon', 'Antylamon'], ['Blazing Ice', 'Ninja Fist', 'Asipatravana'], 'slam', { role: 'Guardian', src: 'TFT3 Neeko' }),
    L('gal_toyagumon', 3, ['XR', 'BO'], 'Vaccine', ['ToyAgumon', 'Tankmon', 'MetalTyrannomon'], ['Plastic Blaze', 'Sub Machine Gun', 'Nuclear Laser'], 'flame', { role: 'Bruiser', src: 'TFT3 Rumble' }),
    L('gal_dracmon', 3, ['DA', 'IN'], 'Virus', ['Dracmon', 'Sangloupmon', 'Matadormon'], ['Eye of Nightmares', 'Sticker Blade', 'Thousand Arrows'], 'backstab', { role: 'Assassin', src: 'TFT3 Shaco' }),
    L('gal_tentomon', 3, ['DD', 'SO'], 'Vaccine', ['Tentomon', 'Kabuterimon', 'MegaKabuterimon'], ['Super Shocker', 'Electro Shocker', 'Horn Buster'], 'orbs', { role: 'Mystic', src: 'TFT3 Syndra' }),
    L('gal_dorumon', 3, ['CY', 'SN'], 'Data', ['Dorumon', 'Dorugamon', 'DoruGreymon'], ['Dash Metal', 'Power Metal', 'Metal Meteor'], 'tumble', { role: 'Sniper', mana: 50, src: 'TFT3.5 Vayne' }),
    L('gal_gaomon', 3, ['CY', 'BR'], 'Data', ['Gaomon', 'Gaogamon', 'MachGaogamon'], ['Rolling Upper', 'Dash Double Claw', 'Winning Knuckle'], 'charge', { role: 'Bruiser', src: 'TFT3 Vi' }),
    // ---- 4 cost (11) ----
    L('gal_gumdramon', 4, ['XR', 'IN'], 'Vaccine', ['Gumdramon', 'Arresterdramon', 'Slayerdramon'], ['Ran-gum Break', 'Spin Caliber', 'Tenryu Slash'], 'shark', { role: 'Assassin', startMana: 30, levels: LV.rcm, src: 'TFT3 Fizz' }),
    L('gal_blackagumon', 4, ['AS', 'BR'], 'Virus', ['BlackAgumon', 'BlackGreymon', 'BlackMetalGreymon'], ['Spitfire', 'Great Antler', 'Metal Slash'], 'transform', { role: 'Bruiser', range: 2, src: 'TFT3.5 Gnar' }),
    L('gal_hackmon', 4, ['CY', 'HA', 'BM'], 'Data', ['Hackmon', 'BaoHuckmon', 'SaviorHuckmon'], ['Fifth Rush', 'Fif Cross', 'Meteor Flame'], 'surge', { role: 'Duelist', mana: 50, src: 'TFT3 Irelia' }),
    L('gal_impmon', 4, ['DA', 'SN'], 'Virus', ['Impmon', 'Beelzemon', 'Beelzemon BM'], ['Bada Boom', 'Double Impact', 'Death Slinger'], 'whisper', { role: 'Sniper', mana: 40, startMana: 0, levels: LV.rum, src: 'TFT3 Jhin' }),
    L('gal_terriermon', 4, ['RE', 'GU'], 'Vaccine', ['Terriermon', 'Gargomon', 'MegaGargomon'], ['Bunny Blast', 'Gargo Laser', 'Giant Missile'], 'excited', { role: 'Sniper', range: 3, mana: 80, levels: LV.rcm, src: 'TFT3 Jinx' }),
    L('gal_lobomon', 4, ['TK', 'BM'], 'Free', ['Lobomon', 'KendoGarurumon', 'MagnaGarurumon'], ['Lobo Kendo', 'Solar Laser', 'Magna Missile'], 'energyslash', { role: 'Duelist', mana: 50, levels: ['Human Spirit', 'Beast Spirit', 'Fused Spirit'], src: 'TFT3.5 Riven' }),
    L('gal_salamon', 4, ['DD', 'MY'], 'Vaccine', ['Salamon', 'Gatomon', 'Ophanimon'], ['Puppy Howl', 'Lightning Paw', 'Sefirot Crystal'], 'wish', { role: 'Support', mana: 90, levels: LV.rcm, src: 'TFT3 Soraka' }),
    L('gal_coronamon', 4, ['AS', 'SN'], 'Vaccine', ['Coronamon', 'Firamon', 'Apollomon'], ['Petit Prominence', 'Fira Claw', 'Phoebus Blow'], 'traps', { role: 'Sniper', levels: LV.rcm, src: 'TFT3.5 Teemo' }),
    L('gal_hagurumon', 4, ['OV', 'SO'], 'Virus', ['Hagurumon', 'Datamon', 'Gigadramon'], ['Cog Crusher', 'Digital Bomb', 'Giga Heat'], 'deathray', { role: 'Mystic', levels: ['Rookie', 'Ultimate', 'Ultimate'], src: 'TFT3.5 Viktor' }),
    L('gal_vegiemon', 4, ['AB', 'SO'], 'Virus', ['Vegiemon', 'Vademon', 'Apocalymon'], ['Sweet Breath', 'Unidentified Flying Kiss', 'Darkness Zone'], 'ray', { role: 'Mystic', levels: ['Champion', 'Ultimate', 'Mega'], src: "TFT3 Vel'Koz" }),
    L('gal_etemon', 4, ['TK', 'VG'], 'Virus', ['Etemon', 'Etemon', 'Etemon'], ['Dark Spirits', 'Banana Slip', 'Monkey Wrench'], 'cyclone', { role: 'Guardian', hpMul: 1.1, levels: ['Ultimate', 'Ultimate', 'Ultimate'], src: 'TFT3 Wukong' }),
    // ---- 5 cost (10) ----
    L('gal_imperialdramon', 5, ['RE', 'ARK'], 'Free', ['Paildramon', 'Imperialdramon FM', 'Imperialdramon PM'], ['Mega Death', 'Positron Laser', 'Omni Blade'], 'fleet', { role: 'Mystic', mana: 100, startMana: 20, hpMul: 1.6, levels: LV.umm, src: 'TFT3 Aurelion Sol' }),
    L('gal_leviamon', 5, ['AB', 'BR'], 'Virus', ['Leviamon', 'Leviamon', 'Leviamon'], ['Biting Crush', 'Rostrum', 'Cauda'], 'rupture', { role: 'Bruiser', mana: 110, startMana: 40, levels: LV.mmm, src: "TFT3 Cho'Gath" }),
    L('gal_kimeramon', 5, ['CY', 'IN'], 'Virus', ['Kimeramon', 'Millenniummon', 'Millenniummon'], ['Heat Viper', 'Time Unlimited', 'Dimension Destroyer'], 'chronobreak', { role: 'Assassin', mana: 110, startMana: 40, levels: LV.umm, src: 'TFT3 Ekko' }),
    L('gal_vikemon', 5, ['PI', 'BH', 'BO'], 'Free', ['Vikemon', 'Vikemon', 'Vikemon'], ['Viking Axe', 'Arctic Blizzard', 'Mjolnir'], 'orbital', { role: 'Striker', mana: 110, startMana: 40, hpMul: 1.2, adMul: 1.2, levels: LV.mmm, src: 'TFT3 Gangplank' }),
    L('gal_lilamon', 5, ['DD', 'SV'], 'Data', ['Lilamon', 'Sakuyamon', 'Sakuyamon'], ['Lila Shower', 'Izuna', 'Amethyst Mandala'], 'gale', { role: 'Support', range: 4, hpMul: 0.9, levels: LV.umm, src: 'TFT3.5 Janna' }),
    L('gal_marineangemon', 5, ['CE', 'MY'], 'Vaccine', ['MarineAngemon', 'MarineAngemon', 'MarineAngemon'], ['Smiling Face', 'Ocean Love', 'Kahuna Waves'], 'polymorph', { role: 'Support', mana: 100, startMana: 40, levels: LV.mmm, src: 'TFT3 Lulu' }),
    L('gal_valkyrimon', 5, ['VA', 'BH', 'GU'], 'Free', ['Valkyrimon', 'Valkyrimon', 'Valkyrimon'], ["Aurvandil's Arrow", 'Fenrir Sword', 'Laser Javelin'], 'bullettime', { role: 'Sniper', mana: 110, startMana: 40, hpMul: 1.15, levels: LV.mmm, src: 'TFT3 Miss Fortune' }),
    L('gal_piedmon', 5, ['TK', 'HA'], 'Virus', ['Piedmon', 'Piedmon', 'Piedmon'], ['Trump Sword', "Clown's Trick", 'Masks Square'], 'lantern', { role: 'Bruiser', range: 2, hpMul: 1.05, levels: LV.mmm, src: 'TFT3 Thresh' }),
    L('gal_machinedramon', 5, ['OV', 'PR'], 'Virus', ['Megadramon', 'Machinedramon', 'Chaosdromon'], ['Ultimate Slicer', 'Infinity Cannon', 'Hyper Infinity Cannon'], 'reel', { role: 'Bruiser', range: 2, levels: LV.umm, src: 'TFT3.5 Urgot' }),
    L('gal_lucemon', 5, ['DA', 'SO'], 'Vaccine', ['Lucemon', 'Lucemon Chaos Mode', 'Lucemon Shadowlord Mode'], ['Grand Cross', 'Paradise Lost', 'Purgatorial Flame'], 'meteors', { role: 'Mystic', range: 4, levels: LV.rum, src: 'TFT3 Xerath' }),

    // ---- wild Digimon (PvE only) and summons ----
    L('gal_w_goblimon', 1, ['BR'], 'Virus', ['Goblimon', 'Goblimon', 'Goblimon'], ['Goblin Strike', 'Goblin Strike', 'Goblin Strike'], 'nuke', { role: 'Striker', shop: false, hpMul: 0.95 }),
    L('gal_w_kuwagamon', 2, ['BR'], 'Virus', ['Kuwagamon', 'Kuwagamon', 'Kuwagamon'], ['Scissor Claw', 'Scissor Claw', 'Scissor Claw'], 'execute', { role: 'Striker', shop: false, hpMul: 0.85, levels: ['Champion', 'Champion', 'Champion'] }),
    L('gal_w_ogremon', 2, ['VG'], 'Virus', ['Ogremon', 'Ogremon', 'Ogremon'], ['Pummel Whack', 'Pummel Whack', 'Pummel Whack'], 'slam', { role: 'Guardian', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('gal_w_tyrannomon', 3, ['BR'], 'Data', ['Tyrannomon', 'Tyrannomon', 'Tyrannomon'], ['Blaze Blast', 'Blaze Blast', 'Blaze Blast'], 'aoe', { role: 'Bruiser', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('gal_w_monochromon', 3, ['VG'], 'Data', ['Monochromon', 'Monochromon', 'Monochromon'], ['Volcanic Strike', 'Volcanic Strike', 'Volcanic Strike'], 'stun', { role: 'Guardian', shop: false, levels: ['Champion', 'Champion', 'Champion'] }),
    L('gal_w_snowgoblimon', 2, ['BR'], 'Virus', ['SnowGoblimon', 'SnowGoblimon', 'SnowGoblimon'], ['Snow Goblin Volt', 'Snow Goblin Volt', 'Snow Goblin Volt'], 'stun', { role: 'Mystic', shop: false }),
    L('gal_w_skullgreymon', 4, ['BR'], 'Virus', ['SkullGreymon', 'SkullGreymon', 'SkullGreymon'], ['Dark Shot', 'Dark Shot', 'Dark Shot'], 'aoe', { role: 'Striker', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('gal_w_triceramon', 4, ['VG'], 'Data', ['Triceramon', 'Triceramon', 'Triceramon'], ['Tri-Horn Attack', 'Tri-Horn Attack', 'Tri-Horn Attack'], 'slam', { role: 'Guardian', shop: false, levels: ['Ultimate', 'Ultimate', 'Ultimate'] }),
    L('gal_w_kuramon', 1, ['IN'], 'Free', ['Kunemon', 'Kunemon', 'Kunemon'], ['Electric Thread', 'Electric Thread', 'Electric Thread'], 'nuke', { role: 'Assassin', shop: false, hpMul: 0.6, levels: ['Rookie', 'Rookie', 'Rookie'] }),
    L('gal_w_metalseadramon', 5, ['VG'], 'Data', ['MetalSeadramon', 'MetalSeadramon', 'MetalSeadramon'], ['River of Power', 'River of Power', 'River of Power'], 'line', { role: 'Sniper', shop: false, boss: true, hpMul: 1.7, levels: LV.mmm }),
    L('gal_w_diaboromon', 5, ['IN'], 'Free', ['Diaboromon', 'Diaboromon', 'Diaboromon'], ['Catastrophe Cannon', 'Catastrophe Cannon', 'Catastrophe Cannon'], 'nuke', { role: 'Assassin', shop: false, boss: true, hpMul: 1.5, adMul: 1.2, levels: LV.mmm }),
    L('gal_w_apocalymon', 5, ['SO'], 'Free', ['Armageddemon', 'Armageddemon', 'Armageddemon'], ['Ultimate Flare', 'Ultimate Flare', 'Ultimate Flare'], 'aoe', { role: 'Mystic', shop: false, boss: true, hpMul: 2.4, radius: 2, levels: LV.mmm }),
    L('gal_w_yggdrasil', 5, ['MY'], 'Free', ['Azulongmon', 'Azulongmon', 'Azulongmon'], ['Lightning Whip', 'Lightning Whip', 'Lightning Whip'], 'aoe', { role: 'Mystic', shop: false, boss: true, hpMul: 3, radius: 2, levels: LV.mmm }),
    // Data Portal construct (item summon)
    L('gal_construct', 1, ['VG'], 'Free', ['Mekanorimon', 'Mekanorimon', 'Mekanorimon'], ['Construct Slam', 'Construct Slam', 'Construct Slam'], 'slam', { role: 'Guardian', shop: false, levels: ['Construct', 'Construct', 'Construct'] }),
  ];

  // ---------- wild rounds ----------
  // [lineId, stage, col, row]; rows 4-7 in the wild side's own coordinates (4 = front). Stage 1: 1-2 and 1-3.
  // The later rounds visit the Digital World: File Island, the Dark Ocean, the Network, and Azulongmon at the core.
  const W = (id, st, c, r) => [`gal_w_${id}`, st, c, r];
  const pveWave = function (stage, round, game) {
    // Normal pace: 1-2 to 1-4 against 1-3 Digimon, and the x-7 wild round meets the next stage's wave (boards are bigger)
    if (game && game.pace === 'normal') {
      if (stage === 1) return round === 2 ? [W('goblimon', 0, 3, 4)] : round === 3 ? [W('goblimon', 0, 2, 4), W('goblimon', 0, 4, 4)] : [W('goblimon', 0, 2, 4), W('goblimon', 0, 3, 4), W('goblimon', 0, 4, 4), W('kuwagamon', 0, 3, 6)];
      if (stage === 2) return hyperWave(2, 4).concat([W('tyrannomon', 1, 1, 6)]);
      if (stage === 3) return hyperWave(3, 4).concat([W('snowgoblimon', 1, 3, 7)]);
      return hyperWave(stage + 1, 4);
    }
    return hyperWave(stage, round);
  };
  const hyperWave = function (stage, round) {
    if (stage === 1) return round === 2 ? [W('goblimon', 0, 2, 4), W('goblimon', 0, 4, 4)] : [W('goblimon', 0, 2, 4), W('goblimon', 0, 3, 4), W('goblimon', 0, 4, 4), W('kuwagamon', 0, 3, 6)];
    if (stage === 2) return [W('kuwagamon', 1, 2, 4), W('kuwagamon', 1, 4, 4), W('ogremon', 0, 3, 4), W('snowgoblimon', 1, 3, 6)];
    if (stage === 3) return [W('ogremon', 1, 3, 4), W('tyrannomon', 1, 2, 4), W('tyrannomon', 1, 4, 4), W('snowgoblimon', 1, 1, 6), W('snowgoblimon', 1, 5, 6), W('kuwagamon', 1, 3, 6)];
    if (stage === 4) return [W('monochromon', 1, 3, 4), W('tyrannomon', 1, 1, 4), W('tyrannomon', 1, 5, 4), W('skullgreymon', 1, 3, 6), W('snowgoblimon', 1, 2, 7), W('snowgoblimon', 1, 4, 7), W('kuwagamon', 2, 0, 5)];
    // Dark Ocean: MetalSeadramon
    if (stage === 5) return [W('triceramon', 1, 3, 4), W('monochromon', 2, 2, 4), W('monochromon', 2, 4, 4), W('metalseadramon', 1, 3, 7), W('skullgreymon', 2, 1, 6), W('skullgreymon', 1, 5, 6)];
    // the Network: Diaboromon and a Kunemon swarm
    if (stage === 6) return [W('diaboromon', 2, 3, 5), W('diaboromon', 1, 1, 6), W('triceramon', 2, 3, 4), W('kuramon', 2, 0, 4), W('kuramon', 2, 2, 4), W('kuramon', 2, 4, 4), W('kuramon', 2, 6, 4), W('skullgreymon', 2, 3, 7)];
    // Armageddemon
    if (stage === 7) return [W('apocalymon', 1, 3, 7), W('triceramon', 2, 2, 4), W('triceramon', 2, 4, 4), W('diaboromon', 2, 1, 5), W('diaboromon', 2, 5, 5), W('skullgreymon', 2, 3, 5), W('metalseadramon', 1, 0, 7)];
    // the core of the Digital World: Azulongmon
    return [W('yggdrasil', 1, 3, 7), W('apocalymon', 1, 1, 6), W('diaboromon', 2, 5, 6), W('triceramon', 2, 2, 4), W('triceramon', 2, 4, 4), W('metalseadramon', 2, 3, 5), W('skullgreymon', 2, 6, 4)];
  };


  // ---------- items ----------
  // Components: the TFT Set 3 eight plus the Digimental (TFT's Spatula). Sparring Gloves also gave dodge in Set 3.
  const components = {
    gal_sword: { name: 'Power Chip', short: 'PC', color: '#ff7a45', stats: { ad: 0.15 } },
    gal_bow: { name: 'Speed Chip', short: 'SC', color: '#ffd23f', stats: { as: 0.12 } },
    gal_rod: { name: 'Data Rod', short: 'DR', color: '#b26bff', stats: { ap: 15 } },
    gal_tear: { name: 'Mana Card', short: 'MC', color: '#4fb3ff', stats: { mana: 15 } },
    gal_vest: { name: 'Chrome Plate', short: 'CP', color: '#c0c6d0', stats: { armor: 20 } },
    gal_cloak: { name: 'Data Cloak', short: 'DC', color: '#3fd6b0', stats: { mr: 20 } },
    gal_belt: { name: 'HP Disk', short: 'HD', color: '#ff4d64', stats: { hp: 150 } },
    gal_glove: { name: 'Lucky Glove', short: 'LG', color: '#8fdc4e', stats: { crit: 0.1, dodge: 0.05 } },
    gal_mental: { name: 'Digimental', short: 'DM', color: '#ffcf4a', stats: {}, special: true,
      desc: 'Combine with a component to make a Crest, or with another Digimental for +1 team size.' },
  };

  // [a, b, name, desc, passives, extraStats, { id, tags, aiRank, ... }]. Hooks for items with no passive are in galaxies-mech.js.
  const R = (a, b, name, desc, p, extra, o) => [`gal_${a}`, `gal_${b}`, name, desc, p || null, extra || {}, { ...o, id: `gal_${o.id}` }];
  const recipes = [
    R('sword', 'sword', 'Data Reaper', 'Each takedown grants 20% attack damage for the rest of combat.', null, { ad: 0.1 }, { id: 'reaper', tags: ['ad'], aiRank: 3 }),
    R('sword', 'bow', 'Giant Slayer', 'Deals 10% more damage, and 40% more to enemies with 1600 or more max HP.', { amp: 0.1, slayer: 0.3 }, {}, { id: 'slayer', tags: ['ad'], aiRank: 4 }),
    R('sword', 'rod', 'Data Gunblade', 'Heals for 25% of all damage dealt.', { vamp: 0.25 }, {}, { id: 'gunblade', tags: ['ad', 'ap'], aiRank: 2 }),
    R('sword', 'tear', 'Mana Spear', 'Basic attacks restore 6 extra mana.', { shojin: 6 }, {}, { id: 'spear', tags: ['ap', 'ad'], aiRank: 3 }),
    R('sword', 'vest', 'Rebirth Egg', 'The first time the holder would be knocked out, it stays at 1 HP and cannot be hurt for 1.5 seconds, then heals 40% of its max HP.', null, {}, { id: 'rebirth', tags: ['tank', 'ad'], aiRank: 2 }),
    R('sword', 'cloak', 'Blood Fang', 'Heals for 30% of all damage dealt.', { vamp: 0.3 }, {}, { id: 'bloodfang', tags: ['ad'], aiRank: 2 }),
    R('sword', 'belt', 'Battle Banner', 'At the start of combat, the holder and allies next to it in the same row gain 30% attack speed.', null, {}, { id: 'banner', tags: ['tank'], aiRank: 0 }),
    R('sword', 'glove', 'Crit Edge', 'Gains 25% critical strike chance and 20% critical damage.', { critDmg: 0.2 }, { crit: 0.25 }, { id: 'critedge', tags: ['ad'], aiRank: 5 }),
    R('bow', 'bow', 'Long Shot', 'Gains 2 attack range. Attacks deal 8% more damage.', { amp: 0.08 }, { as: 0.1 }, { id: 'longshot', tags: ['ad'], aiRank: 1 }),
    R('bow', 'rod', 'Rage Wing', 'Each basic attack grants 5% attack speed, stacking.', { rage: 0.05 }, {}, { id: 'ragewing', tags: ['ad', 'ap'], aiRank: 4 }),
    R('bow', 'tear', 'Volt Disk', 'Every third basic attack sends lightning to 3 enemies for 85 magic damage.', { volt: 85 }, {}, { id: 'voltdisk', tags: ['ad'], aiRank: 1 }),
    R('bow', 'vest', 'Titan Will', 'Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks.', { titan: true }, {}, { id: 'titanwill', tags: ['tank', 'ad'], aiRank: 2 }),
    R('bow', 'cloak', 'Twin Shot', 'Basic attacks also hit a second nearby enemy for 70% damage.', { split: 0.7 }, {}, { id: 'twinshot', tags: ['ad'], aiRank: 2 }),
    R('bow', 'belt', 'Data Portal', 'When the holder is knocked out, a Mekanorimon rises in its place to keep fighting.', null, {}, { id: 'portal', tags: ['tank'], aiRank: 1 }),
    R('bow', 'glove', 'Armor Breaker', "Basic attacks cut the target's armor by 40% for 3 seconds.", { shred: 0.4 }, {}, { id: 'breaker', tags: ['ad'], aiRank: 3 }),
    R('rod', 'rod', 'Ultima Rod', "Increases the holder's total ability power by 30%.", null, { ap: 10 }, { id: 'ultima', tags: ['ap'], aiRank: 4 }),
    R('rod', 'tear', 'Echo Rod', "When the holder's ability hits, its target and up to 3 enemies near it take 160 more magic damage.", null, {}, { id: 'echo', tags: ['ap'], aiRank: 4 }),
    R('rod', 'vest', 'Holy Locket', 'At the start of combat, the holder and allies within 2 hexes in the same row gain a 260 shield.', null, {}, { id: 'locket', tags: ['tank'], aiRank: 1 }),
    R('rod', 'cloak', 'Mana Zap', 'When an enemy within 2 hexes casts its ability, it takes 180 magic damage.', { zap: 180 }, {}, { id: 'manazap', tags: ['tank'], aiRank: 1 }),
    R('rod', 'belt', 'Flame Tome', "The holder's abilities burn their targets for 1.5% max HP per second and cut their healing by a third.", null, { hp: 50 }, { id: 'tome', tags: ['ap'], aiRank: 2 }),
    R('rod', 'glove', 'Jewel Gauntlet', 'Abilities can critically strike. Gains 30% critical damage.', { abilCrit: true, critDmg: 0.3 }, { crit: 0.1 }, { id: 'jewel', tags: ['ap'], aiRank: 5 }),
    R('tear', 'tear', 'Mana Overflow', 'Max mana -10. After casting, keep 20 mana.', { bluebuff: true }, {}, { id: 'overflow', tags: ['ap'], aiRank: 3 }),
    R('tear', 'vest', 'Frost Heart', 'Enemies within 2 hexes lose 30% attack speed.', null, {}, { id: 'frostheart', tags: ['tank'], aiRank: 3 }),
    R('tear', 'cloak', 'Power Chalice', 'At the start of combat, the holder and allies next to it in the same row gain 30 ability power.', null, {}, { id: 'chalice', tags: ['ap', 'tank'], aiRank: 0 }),
    R('tear', 'belt', 'Holy Ring', 'Every 5 seconds, heals allies within 2 hexes for 80 + 10% of their missing HP.', { redemption: true }, {}, { id: 'holyring', tags: ['tank'], aiRank: 1 }),
    R('tear', 'glove', 'Justice Hand', 'Deals 15% more damage and heals for 15% of damage dealt.', { amp: 0.15, vamp: 0.15 }, {}, { id: 'justice', tags: ['ap', 'ad'], aiRank: 3 }),
    R('vest', 'vest', 'Spike Plate', 'Critical strikes against the holder deal normal damage. When attacked, deals 60 magic damage to adjacent enemies.', { thorns: 60 }, { armor: 10 }, { id: 'spikeplate', tags: ['tank'], aiRank: 4 }),
    R('vest', 'cloak', 'Sword Breaker', "Basic attacks have a 30% chance to disarm the target for 3 seconds: its attacks deal no damage.", null, {}, { id: 'swordbreaker', tags: ['tank'], aiRank: 3 }),
    R('vest', 'belt', 'Burning Core', "Basic attacks burn the target for 1.5% max HP per second and cut its healing by a third.", { burn: true }, {}, { id: 'core', tags: ['tank', 'ad'], aiRank: 2 }),
    R('vest', 'glove', 'Still Shroud', "At the start of combat, enemies in the holder's column and the columns beside it need 40% more mana for their first ability.", null, {}, { id: 'shroud', tags: ['tank'], aiRank: 2 }),
    R('cloak', 'cloak', 'Dragon Scale', 'Takes 40% less magic damage.', null, { mr: 10 }, { id: 'dragonscale', tags: ['tank'], aiRank: 3 }),
    R('cloak', 'belt', 'Zephyr Gust', 'At the start of combat, the enemy across from the holder is blown out of the fight for 4 seconds.', null, {}, { id: 'zephyr', tags: ['tank'], aiRank: 1 }),
    R('cloak', 'glove', 'Quick Silver', 'Immune to stuns for the first 12 seconds of combat.', { ccImmune: 12 }, {}, { id: 'quicksilver', tags: ['tank', 'ad'], aiRank: 1 }),
    R('belt', 'belt', 'Giga Belt', 'A lot of health.', null, { hp: 500 }, { id: 'gigabelt', tags: ['tank'], aiRank: 5 }),
    R('belt', 'glove', 'Trap Claw', 'Blocks the first enemy ability that hits the holder and stuns its caster for 3 seconds.', null, {}, { id: 'trapclaw', tags: ['tank'], aiRank: 2 }),
    R('glove', 'glove', 'Thief Glove', 'Each combat, gains two random completed items.', { thief: true }, {}, { id: 'thief', tags: ['ad'], aiRank: 0 }),
  ];
  // Digimental + component = Crest (TFT 3.5 Spatula items)
  const emblems = { gal_sword: 'BM', gal_bow: 'IN', gal_rod: 'OV', gal_tear: 'DD', gal_vest: 'RE', gal_cloak: 'CE', gal_belt: 'PR', gal_glove: 'DA' };
  // Crests with no recipe: Armory, gold loot orbs, the Crest augment
  const dropEmblems = ['CY', 'TK', 'PI', 'AS', 'SO', 'GU', 'BR', 'VG', 'MY', 'SN'];

  // ---------- augments ----------
  // TFT Set 3 had no augments (the Galaxy was its game-wide twist). These keep the Hyper Roll picks at 2-2, 3-2, 4-2.
  const A = (id, tier, name, desc, tag) => ({ id: `gal_a_${id}`, tier, name, desc, tags: [tag] });
  const augments = [
    A('cash', 1, 'Spare Change', 'Gain 8 gold.', 'econ'),
    A('cache', 1, 'Item Cache', 'Gain 2 random components.', 'items'),
    A('rolls', 1, 'Rolling Start', 'Gain 1 free reroll every round.', 'econ'),
    A('training', 1, 'Combat Training', 'Your team gains 12% AD and 12 AP.', 'combat'),
    A('front', 1, 'Front Line', 'Digimon in your front row gain 30 armor and MR and 200 HP.', 'combat'),
    A('back', 1, 'Back Line', 'Digimon in your back two rows gain 25% attack speed.', 'combat'),
    A('starlight', 1, 'Starlight', 'Your team heals 30% of its missing health after 10 seconds of combat.', 'combat'),
    A('bank', 2, 'Digi-Bank', 'Gain 18 gold.', 'econ'),
    A('trove', 2, 'Treasure Trove', 'Gain a random completed item and a component.', 'items'),
    A('mental', 2, 'Spare Digimental', 'Gain a Digimental.', 'items'),
    A('income', 2, 'Steady Income', 'Gain 2 extra gold every round.', 'econ'),
    A('blessing', 2, 'Celestial Blessing', 'Your team heals for 12% of the damage it deals.', 'combat'),
    A('lotus', 2, 'Jewel Lotus', 'Your abilities can critically strike, and your team gains 15% crit.', 'combat'),
    A('implants', 2, 'Cyber Implants', 'Your Digimon holding items gain 250 HP and 15% AD.', 'combat'),
    A('jackpot', 3, 'Jackpot', 'Gain 35 gold.', 'econ'),
    A('armory', 3, 'Big Armory', 'Gain 2 random completed items.', 'items'),
    A('golden', 3, 'Golden Digimental', 'Gain a Golden Digimental (+1 team size).', 'items'),
    A('giant', 3, 'Giant Grab', 'Your team gains 25% max HP.', 'combat'),
    A('stars', 3, 'Star Power', 'Every origin trait you have active counts one extra Digimon.', 'combat'),
    A('megaEvolve', 3, 'Mega Digivolve', 'Gain a random Champion-form (2-star) Digimon of cost 4.', 'econ'),
    A('overcharge', 3, 'Overcharge', 'Your team starts combat with 30 extra mana and 20 AP.', 'combat'),
  ];

  // ---------- Galaxies (the set mechanic; behavior in galaxies-mech.js) ----------
  // weight: relative odds. TFT 3.5 weighted Normal and Big Bang double.
  const GALAXIES = [
    { id: 'normal', name: 'File Island', weight: 2, color: '#7ed957', short: 'No special rules this game.', src: 'Normal' },
    { id: 'treasure', name: 'Spiral Mountain', weight: 1, color: '#ffb347', short: 'Every wild round drops an extra component. From stage 4 it drops a completed item instead.', src: 'Treasure Trove' },
    { id: 'armory', name: 'Infinity Mountain', weight: 1, color: '#c0c6d0', short: 'Every Tamer starts with the same two components.', src: 'Galactic Armory' },
    { id: 'cluster', name: 'Primary Village', weight: 1, color: '#ff9ad5', short: 'At 1-2, the Digimon you took from the opening carousel digivolves straight to its Mega form (3-star).', src: 'Star Cluster' },
    { id: 'mirror', name: 'Mirror Sector', weight: 1, color: '#4fb3ff', short: 'Every Tamer gets a Data Copier at 2-1 and at 3-1. Drag it onto a Digimon to get a Rookie copy of it.', src: 'The Neekoverse' },
    { id: 'bigger', name: 'Folder Continent', weight: 1, color: '#9ad8ff', short: 'From 2-1, every team can field one more Digimon.', src: 'Medium Legends' },
    { id: 'trade', name: 'Trade Sector', weight: 1, color: '#ffd23f', short: 'Every Tamer gets one free reroll every round.', src: 'Trade Sector' },
    { id: 'superdense', name: 'Superdense', weight: 1, color: '#ffcf4a', short: 'At 3-1, every Tamer gets a Golden Digimental (+1 team size).', src: 'Superdense' },
    { id: 'salvage', name: 'Factorial Town', weight: 1, color: '#a0a8b8', short: "Selling a Digimon breaks its completed items back into their components.", src: 'Salvage World' },
    { id: 'plunder', name: 'Plunder Planet', weight: 1, color: '#ffd84a', short: 'Each enemy your team knocks out in a PvP round has a 30% chance to drop 1 gold.', src: 'Plunder Planet' },
    { id: 'shrine', name: 'Digimental Shrine', weight: 1, color: '#ffcf4a', short: 'Every Tamer starts with a Digimental.', src: "Manatee's Delight" },
    { id: 'bigbang', name: 'Big Bang', weight: 2, color: '#e58bff', short: 'One free reroll every round, an extra component from every wild round, and a Golden Digimental for every Tamer at 3-1.', src: 'Big Bang' },
    { id: 'darkocean', name: 'Dark Ocean', weight: 1, color: '#5a3aa8', short: 'All Digimon deal 15% more damage, and all healing is 30% weaker.', src: 'new' },
    { id: 'datastream', name: 'Data Stream', weight: 1, color: '#3fe0b0', short: 'All Digimon start combat with 25 extra mana.', src: 'new' },
    { id: 'server', name: 'Server Continent', weight: 1, color: '#7fd8ff', short: 'Shops show 6 Digimon instead of 5.', src: 'new' },
  ];

  DT.Sets.register({
    id: 'galaxies',
    name: 'Galaxies',
    mode: 'tft',
    order: 3,
    desc: 'TFT Set 3 on Digimon: DigiDestined, Dark Area, DigiXros fusions and Data Pirates. Every game rolls one Galaxy, a rule that changes the game for every Tamer.',
    colors: { main: '#7a8cff', accent: '#ffcf4a' },
    source: 'TFT Set 3 Galaxies and Set 3.5 Return to the Stars. docs/inspo/tft/set-03-galaxies.md, docs/inspo/tft/set-03-5-return-to-the-stars.md, docs/sets/galaxies.md',
    traits: TRAITS,
    lines: LINES,
    pve: pveWave,
    galaxies: GALAXIES,
    items: {
      system: 'tft',
      components,
      recipes,
      emblemFrom: 'gal_mental',
      emblems,
      dropEmblems,
      dropEmblemSource: 'the Armory and gold loot orbs', // codex: where drop-only Crests come from
      emblemName: (T) => `${T.name} Crest`,
      special: [
        { id: 'gal_golden', name: 'Golden Digimental', desc: '+1 team size while held by a Digimon on your board.', p: { teamSize: 1 }, stats: {},
          from: ['gal_mental', 'gal_mental'], color: '#ffd84a', color2: '#fff3b0', short: 'GD' },
      ],
      list: [
        { id: 'gal_copier', consumable: true, name: 'Data Copier', short: 'CO', color: '#4fb3ff', color2: '#e6f6ff', stats: {}, p: {},
          desc: 'Drag onto a Digimon to get a Rookie-form copy of it on your bench.' },
        { id: 'gal_remover', consumable: true, preview: 'remover', name: 'Magnetic Remover', short: 'MR', color: '#ff8fa3', color2: '#8a8f9c', stats: {}, p: {},
          desc: 'Drag onto a Digimon to return all of its items to your item bench.' },
        { id: 'gal_reforger', consumable: true, preview: 'reforger', name: 'Reforger', short: 'RF', color: '#ffb347', color2: '#5a6170', stats: {}, p: {},
          desc: 'Drag onto a Digimon to return its items to your bench as random new items of the same kind.' },
      ],
    },
    augments,
    rules: {
      startHp: 20,
      startGold: 3,
      winGold: 2,
      // Set 3 pool and odds, on the Hyper Roll level schedule (level 3 in stage 1, then 4, 5 ... 9)
      poolSize: [0, 29, 22, 18, 12, 10],
      shopOdds: [
        null,
        [100, 0, 0, 0, 0],
        [100, 0, 0, 0, 0],
        [70, 30, 0, 0, 0],
        [55, 30, 15, 0, 0],
        [40, 35, 20, 5, 0],
        [25, 35, 30, 10, 0],
        [16, 30, 37, 16, 1],
        [13, 22, 33, 25, 7],
        [10, 15, 30, 30, 15],
        [5, 10, 20, 35, 30],
      ],
      levelByStage: [0, 3, 4, 5, 6, 7, 8, 9],
      incomeByStage: [0, 3, 5, 6, 7, 8, 8, 9],
      // Set 3 paid streak gold (also on wild rounds); Hyper Roll keeps it small
      streak: [[3, 1], [5, 2]],
      // Set 3 damage: a stage base plus 1 per surviving enemy Digimon, scaled down for 20 HP
      lossDamage: (stage) => [0, 1, 1, 2, 2, 3, 3, 4, 5][Math.min(stage, 8)],
      unitDamage: (u, stage) => (stage >= 4 && (u.stage >= 2 || u.line.tier >= 5) ? 1 : 0),
      pveLossDamage: () => 1,
      armoryStages: [2, 3, 4, 5, 6],
      augmentStages: [2, 3, 4],
      // the Galaxy is announced at game start (mechanic.intro in galaxies-mech.js)
      offers: [],
      // standard TFT pace: the engine defaults (100 HP, XP, interest, 7-round stages, stage + unit damage), with the
      // Set 3 (patch 10.8) streak table
      normal: {
        streak: [[2, 1], [4, 2], [5, 3]],
        // Set 3 damage: stage base 0/3/4/5/10/15/20 plus 1 per surviving enemy Digimon
        lossDamage: (stage) => [0, 0, 2, 3, 4, 6, 9, 13, 150][Math.min(stage, 8)],
        unitDamage: () => 1,
        offers: [],
      },
    },
    tamers: ['Davis', 'Ken', 'Yolei', 'Cody', 'Kari', 'T.K.', 'Mimi'],
  });
})(typeof window !== 'undefined' ? window : globalThis);
