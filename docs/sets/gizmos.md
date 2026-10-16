# Gizmos & Gadgets (`gizmos`)

TFT Set 6 Gizmos & Gadgets and Set 6.5 Neon Nights, adapted to Digimon. Corpus: `docs/inspo/tft/set-06-gizmos-and-gadgets.md`, `docs/inspo/tft/set-06-5-neon-nights.md`.

Files: `js/sets/gizmos.js` (data), `js/sets/gizmos-mech.js` (hooks, ability types, Hextech Gates), `js/sets/gizmos-art.js` (item, Chip and upgrade portraits, move and basic attack looks), `js/sets/gizmos-models.js` (model assignments), `tools/convert/maps/gizmos.json` (rips converted for this set).

## Overview

- 60 shop lines: 13 / 13 / 13 / 11 / 10 by cost. The roster follows the Set 6.5 champion list, with Vi and Jinx as the two Sisters (Set 6), Akali back from Set 6 and Veigar as a plain 5-cost.
- 27 traits: 12 origins, 3 uniques (Glutton, Mode Change, Mastermind) and 12 classes. Rival and Yordle-Lord are folded into Sisters and Petit.
- 8 wild lines (PvE) plus 2 summons (the Innovator machine and the Decoy Portal decoy).
- Items: 8 components plus the Blank Chip, 36 recipes, 36 Overclocked (radiant) versions, 8 crafted and 13 drop-only Chips (emblems), the Golden Chip, 4 consumables.
- 58 Digivice upgrades (augments): 22 Silver, 22 Gold, 14 Prismatic.
- Both paces. Native pace is Hyper Roll. Normal uses the engine's standard TFT rules with the same three upgrade rounds and no Armory.

## Mechanic: Digivice upgrades and Hextech Gates

Hextech Augments (Set 6) become Digivice upgrades: pick 1 of 3 at 2-1, 3-2 and 4-2, one reroll each, in both paces. They are a mechanic offer kind (`upgrade`) so the tier of each round can come from the gate pattern.

Hextech Gates (the 6.5 idea): when the game starts, one pattern of three tiers is rolled for the whole lobby (12 patterns such as Silver-Gold-Prismatic, Gold-Gold-Gold, Prismatic-Silver-Gold). The pattern is shown in the upgrade window. A Silver gate also gives every Tamer a random component and 4 gold, a Gold gate 2 gold, so a low roll is not a dead round. Clockwork and Chrome Digizoid scale with the number of upgrades you own, as the corpus traits did with the Hexcore.

Other set rules in the mechanic: the Pirates treasure chest (dice before each player combat, opened by a player win, luckier the longer it stays shut), Glutton feeding (selling a Digimon permanently feeds Gekomon), Colossus stun immunity, D-Brigade stuns breaking after 40% max HP lost, Spell Shield wards, and one Mutation rolled per game.

## Rounds and economy

| | Hyper Roll (native) | Normal |
|---|---|---|
| HP, levels, income | 20 HP, level by stage, fixed income, 2 starting gold (Set 6 Hyper Roll) | engine standard: 100 HP, XP, interest, streaks |
| Rounds | 1-1 carousel, 1-2/1-3 wild, x-1..x-3 PvP, x-4 wild | 1-1 carousel, 1-2..1-4 wild, x-4 carousel, x-7 wild |
| Upgrades | 2-1, 3-2, 4-2 | 2-1, 3-2, 4-2 |
| Armory | x-3 of stages 2-6 | none (carousels give items) |
| Wild rounds | Guardromon (Gold) minions, Nanimon's Junk Town (3-4), Gigadramon (4-4), Kurata's Belphemon (5-4, 6-4), Apocalymon (6-7), Yggdrasil 7D6 (8+) | the same waves; stage 1 starts with one minion, and x-7 adds one more Guardromon |

Hyper Roll was chosen as the native pace because the lobby is 8 players and the other finished sets use it. Loot: the Hyper Roll default drops plus an Overclock Kit from won wild rounds in stage 6 and later.

## Traits

| Trait | Kind | Breakpoints | Effect per breakpoint | Corpus trait |
|---|---|---|---|---|
| Masquerade (6) | origin | 3/5/7 | 3: +130 HP, +12 AP (VIP x1.5); 5: +300 HP, +30 AP (VIP x1.5); 7: +550 HP, +50 AP (VIP x1.5) | Debonair |
| D-Brigade (4) | origin | 2/4 | 2: stun the biggest enemy; 4: stun 2 enemies, +15% AS | Enforcer |
| Clockwork (4) | origin | 2/4 | 2: team +18% AS, +3% per upgrade; 4: team +42% AS, +5% per upgrade | Clockwork |
| Dark Masters (6) | origin | 3/5/7 | 3: 2 weakest Dark Masters: +40 armor/MR, 16% omnivamp; 5: all Dark Masters: +50 armor/MR, 20% omnivamp; 7: whole team: +75 armor/MR, 30% omnivamp | Syndicate |
| Junkyard (6) | origin | 2/4/6 | 2: 15 shield per component, 1 component upgraded, +15 armor/MR; 4: 25 shield per component, 2 components upgraded, +30 armor/MR; 6: 40 shield per component, 4 components upgraded, +50 armor/MR | Scrap |
| Pirates (5) | origin | 3/5 | 3: +18% damage, normal dice; 5: +28% damage, lucky dice | Mercenary |
| Chrome Digizoid (7) | origin | 2/4/6 | 2: 100 shield, +15 magic on hit; 4: 170 shield, +30 magic on hit; 6: 290 shield, +50 magic on hit | Hextech |
| Mutation (6) | origin | 3/5 | 3: mutation; 5: strong mutation | Mutant |
| Petit (7) | origin | 3/6 | 3: 20% dodge, a free Petit each round; 6: 35% dodge, a free Petit each round, -20% mana | Yordle |
| X-Antibody (7) | origin | 3/5/7 | 3: +15% AS, 1.5% HP/s for 8s; 5: +30% AS, 2.5% HP/s for 8s; 7: +50% AS, 4% HP/s for 8s | Chemtech |
| Sisters (2) | unique | 1/2 | 1: own bonus; 2: both bonuses, +15% damage | Sister / Rival |
| Idols (4) | origin | 1/2/3 | 1: spotlight: +15% damage; 2: +5 mana/s; 3: +20% omnivamp | Socialite |
| Glutton (1) | unique | 1 | 1: grows when you sell | Glutton |
| Mode Change (1) | unique | 1 | 1: melee in front, ranged in back | Transformer |
| Mastermind (1) | unique | 1 | 1: +30 mana to 2 allies | Mastermind |
| Arcanist (8) | class | 2/4/6 | 2: team +20 AP; 4: team +20 AP, Arcanists +40 more; 6: team +50 AP, Arcanists +50 more | Arcanist |
| Assassin (6) | class | 2/4/6 | 2: +15% crit, +25% crit damage; 4: +35% crit, +45% crit damage; 6: +55% crit, +70% crit damage | Assassin |
| Bodyguard (5) | class | 2/4 | 2: +30 armor, 80 shield and taunt; 4: +70 armor, 220 shield and taunt | Bodyguard |
| Bruiser (7) | class | 2/4/6 | 2: team +100 HP, Bruisers +200; 4: team +200 HP, Bruisers +400; 6: team +350 HP, Bruisers +700 | Bruiser |
| Challenger (6) | class | 2/4/6 | 2: +25% AS, doubled after a takedown; 4: +50% AS, doubled after a takedown; 6: +80% AS, doubled after a takedown | Challenger |
| Colossus (3) | class | 2/3 | 2: -15% damage taken; 3: -20% damage taken, +25% max HP | Colossus |
| Enchanter (4) | class | 2/3/4 | 2: team +10 MR, +6% healing, Enchanters +10 AP; 3: team +20 MR, +12% healing, Enchanters +20 AP; 4: team +30 MR, +18% healing, Enchanters +30 AP | Enchanter |
| Innovator (6) | class | 3/5/7 | 3: Tankmon; 5: Megadramon; 7: Metallicdramon | Innovator |
| Scholar (5) | class | 2/4 | 2: team +4 mana every 2s; 4: team +10 mana every 2s | Scholar |
| Sniper (5) | class | 2/4 | 2: +10% damage per hex; 4: +20% damage per hex | Sniper |
| Striker (5) | class | 2/4 | 2: +35% AD; 4: +80% AD | Striker |
| Twinshot (4) | class | 2/4 | 2: +5% AD, 40% double hit; 4: +30% AD, 70% double hit | Twinshot |

Notes:
- Junkyard (Scrap): a shield per component on the team, and up to 1/2/4 components held by Junkyard Digimon act as a random completed item made from them for that combat.
- Idols (Socialite): the spotlight goes to your Digimon with the most items (ties: cost, then star level), since the board has no marked hexes.
- Mutation (Mutant): Cybernetic, Voracious, Voidborne, Adrenal, Synaptic, Leeching or Metamorphosis, rolled once per game; the name shows above Mutation Digimon at the start of combat. Duel tests (no game) roll one per combat.
- Innovator: Tankmon at 3, Megadramon at 5, Metallicdramon at 7, stronger for every Innovator star level.
- Mode Change (Transformer): Justimon changes arms by row.

## Lines

`src` on every line names the corpus champion slot. All units use converted game models (see Models).

| Cost | Line (id) | Forms | Abilities (type) | Traits | Attr | Adapts |
|---|---|---|---|---|---|---|
| 1 | giz_meramon | Meramon / SkullMeramon / Volcanicdramon | Burning Fist / Heavy Metal Fire / Volcanic Flare (nuke) | Masquerade, Arcanist | Data | TFT6.5 Brand |
| 1 | giz_commandramon | Commandramon / Sealsdramon / Tankdramon | M16 Assassin / Death Behind / Striver Cannon (snipe) | D-Brigade, Sniper | Virus | TFT6 Caitlyn |
| 1 | giz_zubamon | Zubamon / ZubaEagermon / Duramon | Twenty Dive / Road T(w)o Decade / Glass Slash (slam) | Clockwork, Challenger | Vaccine | TFT6 Camille |
| 1 | giz_goblimon | Goblimon / Ogremon / SkullSatamon | Goblin Strike / Pummel Whack / Skull Hammer (drain) | Dark Masters, Bodyguard | Virus | TFT6 Darius |
| 1 | giz_toyagumon | ToyAgumon / Volcamon / PileVolcamon | Plastic Blaze / Big Bang Voice / Volcanic Driver (nuke) | Junkyard, Innovator | Vaccine | TFT6 Ezreal |
| 1 | giz_syakomon | Syakomon / Coelamon / Dragomon | Black Pearl Blast / Fossil Bite / Forbidden Trident (drain) | Pirates, Bruiser | Virus | TFT6 Illaoi |
| 1 | giz_kotemon | Kotemon / Gladimon / Knightmon | Hot Head / Wheel Rush / Grand Smash (slam) | Chrome Digizoid, Striker | Data | TFT6.5 Jarvan IV |
| 1 | giz_tsukaimon | Tsukaimon / Devimon / IceDevimon | Bad Message / Death Claw / Zero Freeze (weaken) | Mutation, Scholar | Virus | TFT6 Kassadin |
| 1 | giz_bakemon | Bakemon / Phantomon / Beelzemon | Hell's Hand / Soul Chopper / Darkness Claw (dash) | Chrome Digizoid, Assassin | Virus | TFT6.5 Nocturne |
| 1 | giz_kamemon | Kamemon / Ikkakumon / Zudomon | Shell Guard / Harpoon Torpedo / Vulcan's Hammer (shield) | Petit, Bodyguard | Data | TFT6 Poppy |
| 1 | giz_mushroomon | Mushroomon / RedVegiemon / Asuramon | Fungus Crusher / Hazard Breath / Fire Fist of Shiva (stun) | X-Antibody, Innovator | Virus | TFT6 Singed |
| 1 | giz_chuumon | Chuumon / Sukamon / KingSukamon | Cheese Bomb / Poop Blast / King Stamp (line) | X-Antibody, Assassin | Virus | TFT6 Twitch |
| 1 | giz_mamemon | Mamemon / MetalMamemon / PrinceMamemon | Smiley Bomb / Energy Bomb / Smile Warhead (aoe) | Junkyard, Petit, Arcanist | Data | TFT6 Ziggs |
| 2 | giz_seadramon | Seadramon / MegaSeadramon / MetalSeadramon | Ice Blast / Mega Ice Blast / River of Power (line) | Dark Masters, Sniper | Data | TFT6.5 Ashe |
| 2 | giz_kokuwamon | Kokuwamon / Guardromon / Andromon | Electro Strike / Power Laser / Spiral Sword (grab) | Junkyard, Bodyguard | Vaccine | TFT6 Blitzcrank |
| 2 | giz_terriermon | Terriermon / Gargomon / Rapidmon (Armor) | Bunny Blast / White Statue / Rapid Fire (barrage) | Petit, Twinshot | Vaccine | TFT6.5 Corki |
| 2 | giz_kazemon | Kazemon / Zephyrmon / Piximon | Tempest Twist / Hurricane Gale / Pit Bomb (heal) | Petit, Enchanter | Data | TFT6 Lulu |
| 2 | giz_kuwagamon | Kuwagamon / Okuwamon / GranKuwagamon | Scissor Claw / Double Scissor Claw / Dimension Scissor (execute) | Mutation, Striker, Bruiser | Virus | TFT6.5 Rek'Sai |
| 2 | giz_kumamon | Kumamon / Hyogamon / Mammothmon | Snow Bomber / Snow Punch / Tusk Crusher (stun) | Chrome Digizoid, D-Brigade, Bruiser | Free | TFT6.5 Sejuani |
| 2 | giz_falcomon | Falcomon / Crowmon / Ravemon | Scratch Smash / Savage Emperor / Blast Wing (drain) | Chrome Digizoid, Arcanist | Vaccine | TFT6 Swain |
| 2 | giz_sorcermon | Sorcermon / Wizardmon / Wisemon | Crystal Cloud / Thunder Cloud / Pandora Dialogue (barrage) | Masquerade, Scholar | Data | TFT6.5 Syndra |
| 2 | giz_dracmon | Dracmon / Sangloupmon / Matadormon | Undead Fang / Sticker Blade / Thousand Arrows (dash) | Masquerade, Assassin | Virus | TFT6 Talon |
| 2 | giz_blackgabumon | BlackGabumon / BlackGarurumon / BlackMetalGarurumon | Little Horn / Freeze Fang / Ice Wolf Claw (drain) | X-Antibody, Challenger | Virus | TFT6 Warwick |
| 2 | giz_solarmon | Solarmon / Clockmon / Jijimon | Soul Shocker / Chrono Breaker / Death Hang (stun) | Clockwork, Innovator | Data | TFT6 Zilean |
| 2 | giz_woodmon | Woodmon / Cherrymon / Puppetmon | Drain Buster / Cherry Bomb / Flying Cross Cutter (stun) | Dark Masters, Scholar | Virus | TFT6 Zyra |
| 2 | giz_hawkmon | Hawkmon / Aquilamon / Gryphonmon | Feather Slash / Blast Laser / Supersonic Voice (aoe) | Pirates, Challenger | Free | TFT6 Quinn |
| 3 | giz_airdramon | Airdramon / Kimeramon / Millenniummon | Spinning Needle / Heat Viper / Dimension Destroyer (execute) | Mutation, Bruiser, Colossus | Data | TFT6 Cho'Gath |
| 3 | giz_gumdramon | Gumdramon / Arresterdramon / Magnamon X | Ran-gum Break / Spin Caliber / Plasma Shoot (aoe) | Junkyard, Innovator, Assassin | Vaccine | TFT6.5 Ekko |
| 3 | giz_gizamon | Gizamon / Gesomon / MarineDevimon | Spiral Edge / Devil Bashing / Dark Deluge (nuke) | Pirates, Twinshot | Virus | TFT6 Gangplank |
| 3 | giz_bearmon | Bearmon / Grizzlymon / GrapLeomon | Bear Fist / Maul Attack / The King of Fists (slam) | Petit, Idols, Striker | Vaccine | TFT6.5 Gnar |
| 3 | giz_coronamon | Coronamon / Firamon / Apollomon | Corona Flame / Flame Dive / Sol Blaster (shield) | Masquerade, Bodyguard | Vaccine | TFT6 Leona |
| 3 | giz_starmon | Agumon / Starmon / SuperStarmon | Pepper Breath / Meteor Shower / Super Star Seeker (barrage) | Chrome Digizoid, Twinshot | Data | TFT6.5 Lucian |
| 3 | giz_vegiemon | Vegiemon / Vademon / Ebemon | Sweet Breath / Unidentified Flying Kiss / Crusher Bone (visions) | Mutation, Arcanist | Virus | TFT6 Malzahar |
| 3 | giz_penguinmon | Penguinmon / Whamon / Neptunemon | Ice Prism / Tidal Wave / Gate of Atlantis (barrage) | Pirates, Sniper | Vaccine | TFT6 Miss Fortune |
| 3 | giz_demidevimon | DemiDevimon / LadyDevimon / Mastemon | Demi Darts / Darkness Wave / Dark Despair (rally) | Dark Masters, Enchanter | Virus | TFT6.5 Morgana |
| 3 | giz_salamon | Salamon / Gatomon / Angewomon | Petty Punch / Cat's Eye / Celestial Arrow (heal) | Idols, Enchanter | Vaccine | TFT6.5 Senna |
| 3 | giz_blackagumon | BlackAgumon / BlackGreymon / WarGreymon X | Spitfire / Great Antler / Terra Force Zero (dash) | X-Antibody, Challenger | Virus | TFT6.5 Tryndamere |
| 3 | giz_impmon | Impmon / Pumpkinmon / Lilithmon | Night of Fire / Jack O' Lantern / Phantom Pain (aoe) | Petit, Arcanist | Virus | TFT6 Vex |
| 3 | giz_numemon | Numemon / Raremon / PlatinumNumemon | Party Time / Rotten Tackle / Platinum Burp (slam) | X-Antibody, Bruiser | Virus | TFT6 Zac |
| 4 | giz_youkomon | Youkomon / Kyubimon (Silver) / Kuzuhamon | Blazing Spheres / Dragon Wheel / Taizoukai Mandala (line) | Dark Masters, Arcanist | Data | TFT6.5 Ahri |
| 4 | giz_tyrannomon | Tyrannomon / MetalTyrannomon / RustTyranomon | Wild Buster / Nuclear Laser / Rust Breath (slam) | Chrome Digizoid, Colossus | Virus | TFT6.5 Alistar |
| 4 | giz_psychemon | Psychemon / Gururumon / Astamon | Colored Spark / Gururu Thrust / Mad Dog Fire (barrage) | Masquerade, Challenger | Data | TFT6.5 Draven |
| 4 | giz_shoutmon | Shoutmon / Shoutmon X3 / OmniShoutmon | Bellow Blaster / Hard Rock Soul / Victorize Banking (dash) | Junkyard, Striker | Data | TFT6.5 Irelia |
| 4 | giz_lekismon | Lekismon / Crescemon / Dianamon | Tear Arrow / Ice Archery / Arrow of Artemis (line) | Clockwork, Sniper | Data | TFT6 Jhin |
| 4 | giz_kunemon | Kunemon / Flymon / TyrantKabuterimon | Electric Thread / Poison Sting / Insect Cannon (execute) | Mutation, Assassin | Virus | TFT6.5 Kha'Zix |
| 4 | giz_hagurumon | Hagurumon / Mekanorimon / HiAndromon | Cog Crusher / Twinkle Beam / Atomic Ray (rally) | Clockwork, Enchanter | Vaccine | TFT6 Orianna |
| 4 | giz_floramon | Floramon / Kiwimon / Lotosmon | Rain of Pollen / Blast Shot / Serpent Ruin (weaken) | X-Antibody, Scholar | Data | TFT6.5 Renata Glasc |
| 4 | giz_lalamon | Lalamon / Lilamon / Rosemon | Sing a Song / Lila Shower / Forbidden Temptation (heal) | Idols, Innovator | Data | TFT6 Seraphine |
| 4 | giz_dorumon | Dorumon / Raptordramon / Grademon | Dash Metal / Ambush Crunch / Cross Blade (chain) | Chrome Digizoid, Striker | Vaccine | TFT6.5 Sivir |
| 4 | giz_sistermonb | Sistermon Blanc / Sistermon Blanc (Awakened) / Sistermon Blanc (Awakened) | Divine Pierce / Protect Wave / Divine Pierce (Awake) (line) | D-Brigade, Sisters, Bruiser | Vaccine | TFT6.5 Vi |
| 5 | giz_gotsumon | Gotsumon / Golemon / BanchoLeomon | Earth Tremor / Guardian Bomb / Flashy Boss Punch (slam) | Idols, Bodyguard, Colossus | Virus | TFT6 Galio |
| 5 | giz_justimon | Justimon: Blitz Arm / Justimon: Critical Arm / Justimon: Trinity Arm | Voltage Blitz / Thunder Blade / Trinity Arm (aoe) | D-Brigade, Innovator, Mode Change | Vaccine | TFT6 Jayce |
| 5 | giz_sistermonn | Sistermon Noir / Sistermon Noir (Awakened) / Sistermon Noir (Awakened) | Mickey Bullet / Bless Fire / Grand Sister Cruz (aoe) | Junkyard, Sisters, Twinshot | Virus | TFT6 Jinx |
| 5 | giz_keramon | Keramon / Infermon / Diaboromon | Crazy Giggle / Hell's Grenade / Catastrophe Cannon (barrage) | Mutation, Challenger | Virus | TFT6 Kai'Sa |
| 5 | giz_myotismon | Myotismon / VenomMyotismon / GranDracmon | Crimson Lightning / Venom Infusion / Crystal Revolution (drain) | Mastermind, Scholar | Virus | TFT6.5 Silco |
| 5 | giz_gekomon | Gekomon / ShogunGekomon / ShogunGekomon | Tongue Attack / Musical Fist / Tonosama Jump (devour) | Pirates, Bruiser, Glutton | Virus | TFT6 Tahm Kench |
| 5 | giz_datamon | Datamon / Machinedramon / Chaosdromon | Digital Bomb / Infinity Cannon / Hyper Infinity Cannon (line) | X-Antibody, Arcanist | Virus | TFT6 Viktor |
| 5 | giz_renamon | Renamon / Taomon / Sakuyamon | Kohenkyo / Talisman Spell / Izuna (barrage) | Masquerade, Sniper | Data | TFT6.5 Zeri |
| 5 | giz_lucemon | Lucemon / Lucemon Chaos Mode / Lucemon Shadowlord Mode | Galaxy Crash / Paradise Lost / Purgatorial Flame (nuke) | Petit, Arcanist | Vaccine | TFT6 Veigar |
| 5 | giz_arukenimon | Arukenimon / Piedmon / Boltboutamon | Spider Thread / Trump Sword / Pernicious Waltz (dash) | Dark Masters, Assassin | Virus | TFT6 Akali |

## Items

Components: Power Cell (AD), Turbo Gear (AS), Code Rod (AP), Mana Capacitor (mana), Steel Plate (armor), Static Cloak (MR), Battery Pack (HP), Grip Glove (crit and dodge), Blank Chip (special: Chips and the Golden Chip).

| Item | Recipe | Effect |
|---|---|---|
| Doom Blade | Power Cell + Power Cell | Grants a lot of attack damage. |
| Titan Breaker | Power Cell + Turbo Gear | Deals 10% more damage, and 35% more again to enemies with 1600 or more max HP. |
| Gun Blade | Power Cell + Code Rod | Heals for 22% of all damage dealt. Magic damage also heals the lowest-health ally for 25% of it. |
| Mana Spear | Power Cell + Mana Capacitor | Basic attacks restore 6 extra mana. |
| Backup Drive | Power Cell + Steel Plate | The first time the holder would be knocked out, it goes into stasis for 2 seconds instead, then returns with 400 HP. |
| Blood Fang | Power Cell + Static Cloak | Heals for 25% of damage dealt. Below 40% HP, gain a 25% max HP shield once. |
| War Banner | Power Cell + Battery Pack | At the start of combat, the holder and allies within 1 hex in the same row gain 30% attack speed. |
| Crit Edge | Power Cell + Grip Glove | Grants 45% critical strike chance and 10% critical strike damage. |
| Rail Cannon | Turbo Gear + Turbo Gear | Grants a lot of attack speed and 1 extra hex of attack range. |
| Rage Engine | Turbo Gear + Code Rod | Each basic attack grants 6% attack speed for the rest of combat, stacking. |
| Static Shiv | Turbo Gear + Mana Capacitor | Every third basic attack sends lightning to 3 enemies for 70 magic damage. |
| Titan Core | Turbo Gear + Steel Plate | Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times. |
| Twin Turbine | Turbo Gear + Static Cloak | Basic attacks also hit a second nearby enemy for 60% damage. |
| Decoy Portal | Turbo Gear + Battery Pack | At the start of combat, enemies within 4 hexes target the holder. When the holder is knocked out, a Chrysalimon decoy appears in its place. |
| Armor Piercer | Turbo Gear + Grip Glove | Basic attacks cut the target's armor by 40% for 3 seconds. |
| Mega Rod | Code Rod + Code Rod | Grants a lot of ability power. |
| Seraph Staff | Code Rod + Mana Capacitor | Gain 20 ability power every 5 seconds. |
| Guard Locket | Code Rod + Steel Plate | At the start of combat, the holder and allies within 2 hexes in the same row gain a shield of 300 (350 at Champion, 400 at Ultimate). |
| Ion Spark | Code Rod + Static Cloak | When an enemy within 2 hexes casts, it takes 160 magic damage. |
| Burn Codex | Code Rod + Battery Pack | All damage burns the target for 1.5% max HP per second and cuts its healing by a third. |
| Jewel Gauntlet | Code Rod + Grip Glove | Abilities can critically strike. +30% critical strike damage. |
| Mana Overflow | Mana Capacitor + Mana Capacitor | Max mana -10. After casting, keep 20 mana. |
| Frost Heart | Mana Capacitor + Steel Plate | Enemies within 2 hexes have 35% less attack speed. |
| Power Chalice | Mana Capacitor + Static Cloak | At the start of combat, the holder and allies within 1 hex in the same row gain 30 ability power. |
| Redemption Core | Mana Capacitor + Battery Pack | Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP. |
| Justice Glove | Mana Capacitor + Grip Glove | Deals 12% more damage and heals for 12% of damage dealt. |
| Thorn Mail | Steel Plate + Steel Plate | When hit by a basic attack, deals 70 magic damage to adjacent enemies (1.5 second cooldown). |
| Gargoyle Plate | Steel Plate + Static Cloak | Gain 15 armor and magic resist for each enemy within 2 hexes. |
| Solar Cape | Steel Plate + Battery Pack | Every 2 seconds, burns the nearest enemy within 2 hexes. |
| Static Shroud | Steel Plate + Grip Glove | At the start of combat, enemies in the holder's column and the columns next to it lose their mana and need 35% more for their first cast. |
| Dragon Scale | Static Cloak + Static Cloak | Grants a lot of magic resist and regenerates 1% max HP per second. |
| Wind Turbine | Static Cloak + Battery Pack | At the start of combat, blows the enemy across from the holder out of the fight for 5 seconds. It cannot act or be hurt. |
| Quick Silver | Static Cloak + Grip Glove | Immune to stuns for the first 15 seconds. |
| Giga Battery | Battery Pack + Battery Pack | Grants a lot of HP. |
| Spell Shield | Battery Pack + Grip Glove | At the start of combat, the holder and allies within 1 hex in the same row gain a ward that blocks the first 500 magic damage they take in one hit. |
| Thief Glove | Grip Glove + Grip Glove | Each combat, the holder gains two random completed items. |

- Overclocked items: every recipe has a radiant version with about 60% stronger passives and 1.5x stats. Never dropped at random; from the Overclock Kit (consumable), the Overclock Lab upgrade and won wild rounds from stage 6.
- Chips (emblems): Blank Chip + Power Cell = Striker, Turbo Gear = Challenger, Code Rod = Arcanist, Mana Capacitor = Chrome Digizoid, Steel Plate = Masquerade, Static Cloak = Dark Masters, Battery Pack = X-Antibody, Grip Glove = Assassin. Drop-only: Bodyguard, Bruiser, Clockwork, D-Brigade, Innovator, Junkyard, Pirates, Mutation, Petit, Scholar, Sniper, Enchanter, Twinshot.
- Consumables: Magnetic Remover, Reforger, Copy Chip (a Rookie-form copy of a 1-4 cost Digimon), Overclock Kit.
- AI tags: every item has `ad` / `ap` / `tank` tags and explicit `aiRoles` lists (Overclocked first); upgrades are tagged `econ` / `items` / `combat`, and trait upgrades score by the trait's count on the board.

## Digivice upgrades

| Tier | Upgrade | Effect |
|---|---|---|
| Silver | Spare Parts | Gain 10 gold. |
| Silver | Parts Bin | Gain 2 random components. |
| Silver | Featherweights | Your 1 and 2-cost Digimon gain 25% attack speed. |
| Silver | Celestial Blessing | Your team heals for 10% of all damage dealt. |
| Silver | Cybernetic Implants | Your Digimon holding an item gain 200 HP and 10% attack damage. |
| Silver | Thrill of the Hunt | When your Digimon score a kill, they heal 300 HP. |
| Silver | Knife's Edge | Your Digimon that start in the front two rows gain 15% attack damage. |
| Silver | Makeshift Armor | Your Digimon with no items gain 30 armor and magic resist. |
| Silver | Exiles | Your Digimon that start combat with no adjacent allies gain a 35% max HP shield. |
| Silver | Stand United | Your team gains 2% attack damage and 2 ability power for every active trait. |
| Silver | Ascension | After 15 seconds of combat, your team deals 40% more damage. |
| Silver | Calculated Loss | After you lose a combat, gain 3 gold and a free reroll next round. |
| Silver | Hyper Roll | After each combat, if you have less than 10 gold, gain 2 gold. |
| Silver | Weakspot | Your basic attacks cut the target's armor by 20% and cut its healing by a third for 5 seconds. |
| Silver | First Aid Kit | All healing and shields on your team are 30% stronger. |
| Silver | Scoped Weapons | Your ranged Digimon gain 1 hex of attack range. |
| Silver | Underdogs | While you have fewer Digimon alive than your opponent, your team heals 4% max HP every second (up to 150). |
| Silver | Trait Booster | Your largest origin trait counts one extra Digimon. |
| Silver | Runic Shield | Your Arcanists start combat with a shield equal to 3 times their ability power. |
| Silver | Chemical Overload | When your X-Antibody Digimon are knocked out, they explode for 25% of their max HP as magic damage to enemies within 2 hexes. |
| Silver | So Small | Your Petit Digimon gain 40% dodge chance and 10% attack speed. |
| Silver | Cutthroat | The first enemy each of your Assassins hits loses all its mana. |
| Gold | Digi-Bank | Gain 18 gold. |
| Gold | Spare Chip | Gain a Blank Chip and a random component. |
| Gold | Crest Kit | Gain a Chip (emblem) for an origin trait on your board. |
| Gold | Binary Airdrop | Your Digimon holding 2 or more items gain a random completed item for each combat. |
| Gold | Celestial Blessing II | Your team heals for 18% of all damage dealt. |
| Gold | Cybernetic Implants II | Your Digimon holding an item gain 300 HP and 15% attack damage. |
| Gold | Featherweights II | Your 1 and 2-cost Digimon gain 40% attack speed. |
| Gold | Knife's Edge II | Your Digimon that start in the front two rows gain 25% attack damage. |
| Gold | Makeshift Armor II | Your Digimon with no items gain 45 armor and magic resist. |
| Gold | Stand United II | Your team gains 3% attack damage and 3 ability power for every active trait. |
| Gold | Thrill of the Hunt II | When your Digimon score a kill, they heal 500 HP. |
| Gold | Trade Sector | Gain a free reroll every round. |
| Gold | Salvage Bin | Gain a random completed item. |
| Gold | Shrug It Off | Your Bruisers regenerate 2.5% of their max HP every second. |
| Gold | Spell Blade | After casting, your Arcanists' next basic attack deals bonus magic damage equal to 200% of their ability power. |
| Gold | Broken Stopwatch | 5 seconds into combat, every enemy freezes for 2 seconds. Your Clockwork Digimon gain 30% attack speed. |
| Gold | Smoke Bomb | The first time each of your Assassins drops below 60% HP, it vanishes for 1.5 seconds: it cannot be hurt and shakes off stuns. |
| Gold | Instant Injection | Your X-Antibody Digimon also trigger their X-Antibody bonus at the start of combat. |
| Gold | Gold Reserves | Your Pirates deal 15% more damage, plus 1% more for every gold you had at the start of the round (up to 45% in total). |
| Gold | Junkyard Haul | Gain a random component after every third combat that you fight with Junkyard active. |
| Gold | Titanic Force | Your Digimon with 1400 or more max HP gain attack damage equal to 3% of their max HP. |
| Gold | Ardent Censer | When one of your Digimon gains a shield, it gains 30% attack speed for 5 seconds (at most once every 3 seconds). |
| Prismatic | Jackpot | Gain 30 gold. |
| Prismatic | Item Grab Bag | Gain 2 random completed items. |
| Prismatic | New Recruit | Gain +1 team size. |
| Prismatic | Celestial Blessing III | Your team heals for 25% of all damage dealt. |
| Prismatic | Cybernetic Implants III | Your Digimon holding an item gain 400 HP and 20% attack damage. |
| Prismatic | Featherweights III | Your 1 and 2-cost Digimon gain 60% attack speed. |
| Prismatic | High End Shopping | Your shop rolls as if you were one level higher, and you gain a free reroll every round. |
| Prismatic | Golden Ticket | Gain 5 gold. Every time your shop rolls, there is a 50% chance you gain a free reroll. |
| Prismatic | Woodland Charm | At the start of combat, your Digimon with the most max HP makes a copy of itself with 1500 HP and no items. |
| Prismatic | Windfall | Gain 18 gold for each Digivice upgrade you own, this one included. |
| Prismatic | Overclock Lab | Gain a random Overclocked item. |
| Prismatic | Digivice Burst | Every trait with at least 2 Digimon on your board counts one extra. |
| Prismatic | Stand United III | Your team gains 5% attack damage and 5 ability power for every active trait. |
| Prismatic | Band of Thieves | Gain a Thief Glove and a random component. |

## Models

Every form uses a converted rip. Cyber Sleuth-family rips (Cyber Sleuth, Links, Time Stranger, Next Order) first; New Century, ReArise and Digimon Masters rips where no CS-family rip exists (Commandramon line, Gladimon, Asuramon, Kumamon line, Firamon, Apollomon, Astamon, Flymon, Kiwimon, Mekanorimon, Grizzlymon, Gesomon, Kunemon, Arukenimon). Forms without any rip were swapped for a Digimon in the same role: Octomon -> Coelamon, MetalPhantomon -> Beelzemon, Gwappamon/Shawujinmon -> Ikkakumon/Zudomon, BigMamemon -> PrinceMamemon, Tinkermon line -> Kazemon/Zephyrmon/Piximon, Korikakumon/AncientMegatheriummon -> Hyogamon/Mammothmon, Candlemon -> Sorcermon, ShadowWereGarurumon -> BlackMetalGarurumon, Chronomon -> Jijimon, Arresterdramon SM -> Magnamon X, Ranamon line -> Penguinmon/Whamon/Neptunemon, BlackGatomon -> DemiDevimon, BlackWarGreymon -> WarGreymon X, NoblePumpkinmon -> Lilithmon, Geremon line -> Numemon/Raremon/PlatinumNumemon, Doumon -> Kyubimon (Silver), BanchoGolemon -> BanchoLeomon, NeoMyotismon/MaloMyotismon -> VenomMyotismon/GranDracmon, TonosamaGekomon -> ShogunGekomon, Gizumon -> Guardromon (Gold), Garbagemon -> Nanimon, DemiMeramon line -> Meramon/SkullMeramon/Volcanicdramon. 18 rips were converted for this set (`giz:*` keys); the rest reuse existing keys. `node tests/models.cjs gizmos` reports 0 gaps.

## Balance

Commands: `node tools/balance.cjs --duel 5000 4 1 --set gizmos`, `node tools/balance.cjs 300 4 --set gizmos --pace hyper`, `node tools/balance.cjs 200 4 --set gizmos --pace normal`. Upgrade placements come from a scratch script that records each player's upgrades and final place (240 hyper games, 160 normal games).

Duel (5000 fights, no items, same cost mix on both sides): every trait between 48.7% and 57.2%. Highest: Mutation 57.2, Sniper 57.2, Pirates 57.0, Enchanter 56.8. Lowest: Challenger 48.7, Glutton 49.8 (Glutton grows from selling, which duels do not have), Petit 51.0, Junkyard 51.0 (it needs items).

Full games, Hyper Roll (300 games): 24.0 combat rounds a game (21-29), ending in stage 6 (17%), 7 (75%) or 8 (8%). Trait average placement 3.08-4.84 on the final board; Mode Change (Justimon, a 5-cost) and Innovator lead, Pirates and Bruiser trail. Lines track cost: 4- and 5-costs average 2.95-3.7, 3-costs 3.6-4.7, 2-costs 4.2-5.1. Wild rounds won: 1: 96%, 2: 94%, 3: 94%, 4: 76%, 5: 82%, 6: 77%, 7: 72%.

Full games, Normal (200 games): 35.1 combat rounds a game (31-40), ending in stage 6 (12%) or 7 (87%). Trait average placement 3.15-5.08 (Junkyard and Clockwork lead, D-Brigade trails). Wild rounds won: 1: 97%, 2: 91%, 3: 88%, 4: 93%, 5: 84%, 6: 84%, 7: 57%.

Digivice upgrades: average placement by upgrade 3.82-5.32 in Hyper Roll and 3.49-5.29 in Normal (about 45-150 picks each, so differences under about 0.4 are noise). No upgrade is a must-pick; the outliers that were fixed during tuning: Band of Thieves (2 Thief Gloves, 2.97) became a Thief Glove and a component; Digivice Burst (3.15 in Normal) now only boosts traits with 2+ Digimon; High End Shopping, Golden Ticket, Gold Reserves, Spare Chip, Ardent Censer, Windfall, Stand United III, So Small and Broken Stopwatch were buffed.

Main tuning steps: Enchanter, X-Antibody, D-Brigade, Bodyguard and Colossus were the first-pass outliers (64-72% duel win rate) and were cut; Mutation (29%) got a per-combat roll outside games and a Cybernetic bonus without items; Assassin, Sniper, Striker, Clockwork and Petit were raised; Junkyard was cut after full games showed it strong once items arrive.
