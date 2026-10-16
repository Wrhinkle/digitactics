# Galaxies

TFT Set 3 "Galaxies" and Set 3.5 "Return to the Stars" on Digimon. Corpus: `docs/inspo/tft/set-03-galaxies.md`, `docs/inspo/tft/set-03-5-return-to-the-stars.md`.

Files: `js/sets/galaxies.js` (data), `js/sets/galaxies-mech.js` (traits, ability types, items, augments, the Galaxy mechanic, codex rules), `js/sets/galaxies-art.js` (item, Crest, augment and Galaxy-card art, move and basic attack looks), `js/sets/galaxies-models.js` (a model for every form).

Counts: 60 shop lines (13/13/13/11/10 by cost), 13 wild Digimon and one summon, 26 traits (15 origins, 11 classes), 8 components plus the Digimental, 36 completed items, 8 crafted and 10 drop-only Crests, the Golden Digimental, 3 consumables, 21 augments and 15 Galaxies.

## The mechanic: Galaxies

Every game rolls one Galaxy. It is picked from the game seed with a hash (not the game rng), so every client gets the same one. Every Tamer sees it when the game starts, in a card above the 1-1 opening carousel (`mechanic.intro`), and the codex Rules tab lists all Galaxies with their odds. File Island and Big Bang are twice as likely as the others, as TFT 3.5 weighted Normal and Big Bang. The state is `game.mech.galaxy`; combat hooks read `c.info.mech.galaxy`. Headless tools can force one with `DT.GAL_FORCE = '<id>'`.

Left out because they need engine changes: Binary Star (two items per unit), Dwarf Planet (blocked rows) and Lilac Nebula (a 4-cost opening carousel; the 1-1 carousel is built before any set hook runs). Three new Digital World Galaxies (Dark Ocean, Data Stream, Server Continent) take their place.

## Pace

The native pace is Hyper Roll. The flat `rules` are the Hyper Roll rules; `rules.normal` restates the round-bound parts for the standard pace.

- **Hyper Roll**: 20 HP, levels by stage (3 to 9), income 3/5/6/7/8/8/9 by stage, small Set 3 streak gold (3 in a row: 1, 5: 2), 1-gold rerolls, the Set 3 pool (29/22/18/12/10) and Set 3 shop odds on the Hyper Roll level schedule. Damage keeps Set 3's shape, scaled for 20 HP: a stage base (1, 1, 2, 2, 3, 3, 4, 5 for stages 1-8) plus 1 per surviving enemy at 3-star form or of cost 5, from stage 4. Armory at x-1 of stages 2-6, augments at 2-2, 3-2 and 4-2. Wild rounds at 1-2, 1-3 and x-4.
- **Normal**: the engine's standard pace (100 HP, XP to level 10, interest, 7-round stages with an x-4 carousel and an x-7 wild round, augments at 2-1, 3-2 and 4-2), with the Set 3 (patch 10.8) streak table (2 in a row: 1, 4: 2, 5 or more: 3), Set 3-style damage (stage base 0/2/3/4/6/9/13 for stages 1-7, a little under Set 3's 0/3/4/5/10/15/20 so games reach 35 combats, plus 1 per surviving enemy) and the Galaxy card at 1-2. Stage 1 wild rounds send 1, 2 and 3 Goblimon (and a Kuwagamon at 1-4). Each x-7 wild round sends the next stage's Hyper Roll wave (stages 2 and 3 get one extra wild Digimon instead), because Normal boards are bigger.

Wild rounds: Goblimon on File Island, Kuwagamon and Ogremon, dinosaurs and SnowGoblimon, MetalSeadramon of the Dark Ocean (stage 5), Diaboromon with a Kunemon swarm (6), Armageddemon (7) and Azulongmon at the core of the Digital World (8 on). Rewards use the engine's default loot orbs.

## Notes on traits and items

Trait effects are hooks in `galaxies-mech.js`. So are the 53 ability types the engine does not have (snipe, riposte, smoke, sap, isolate, toss, plumage, bomb, bubble, orb, grab, dunk, frenzy, crater, lastbreath, chronobreak, polymorph, bullettime, lantern, reel, meteors and the rest); each recreates its corpus champion's spell.

- **DigiXros** (Mech-Pilot): at combat start the three highest-value DigiXros units fuse into the strongest of them (90% of their combined HP, 72% of their combined AD, +20 armor and MR). The other two are hidden; when the fusion falls they are ejected next to it at 60% HP.
- **Data Pirates** and the Bard line's meeps pay out after combat by reading that combat's team state in `onRoundEnd`.
- **Abyss** (Void) ignores 45% of armor and MR instead of dealing true damage; full true damage tested far above the 58% ceiling.
- **Bounty Hunters** (Mercenary): TFT's spell-upgrade shop slots need UI work, so each copy bought makes that Bounty Hunter permanently stronger instead (+6 AP and +3% AD, up to 9 copies), kept in `player.mech.bounty`.
- **Ark** (Starship): gains 30 mana a second, cannot be stunned and does not make basic attacks.
- **Data Copier** (Neeko's Help) takes its copy from the shared pool and fails when none is left (TFT 3.5 ignored the pool, which would break the pool count). The AI uses Data Copiers on its best Rookie-form unit.

## Traits

| Trait | Kind | Breakpoints | Effect | Corpus trait | Lines |
|---|---|---|---|---|---|
| DigiDestined | origin | 3/6 | When a DigiDestined Digimon casts its ability, the other DigiDestined share bonus mana. (3) 12 mana shared per cast; (6) 25 mana shared per cast. | Star Guardian | Gomamon, Patamon, Renamon, Lopmon, Tentomon, Salamon, Lilamon |
| Dark Area | origin | 2/4/6 | When a Dark Area Digimon is knocked out, the other Dark Area Digimon gain attack damage and ability power. (2) +14% AD and +14 AP per fallen ally; (4) +25% AD and +25 AP per fallen ally; (6) +38% AD and +38 AP per fallen ally. | Dark Star | BlackGabumon, Tsukaimon, Aruraumon, Dracmon, Impmon, Lucemon |
| DigiXros | origin | 3 | At the start of combat, three DigiXros Digimon fuse into one. The fusion has most of their combined health and attack damage. When it falls, the three are ejected and keep fighting. (3) fusion with 90% of combined HP and AD. | Mech-Pilot | Shoutmon, ToyAgumon, Gumdramon |
| Data Pirates | origin | 2/4 | When a Data Pirate knocks out an enemy in a PvP round, it has a chance to drop gold after combat. With 4, it can also drop a component. (2) 50% for 1 gold; (4) 50% for 1 gold, 30% for a component. | Space Pirate | Gazimon, Gizamon, Crabmon, Vikemon |
| Celestials | origin | 2/4 | All your Digimon heal for a share of the damage they deal. (2) 7% healing from damage; (4) 16% healing from damage. | Celestial | Hyokomon, Biyomon, Kudamon, Lunamon, MarineAngemon |
| Time Keepers | origin | 2/4/6 | All your Digimon gain 12% attack speed every few seconds, stacking. (2) +12% AS every 8s; (4) +12% AS every 4s; (6) +12% AS every 2s. | Chrono | Hawkmon, Solarmon, Kokuwamon, Agumon, Lobomon, Etemon, Piedmon |
| Cyborgs | origin | 3/6 | Cyborgs gain max health and attack damage. A Cyborg holding at least one item gains double. (3) +130 HP and +10% AD (x2 with an item); (6) +260 HP and +20% AD (x2 with an item). | Cybernetic | Kotemon, Commandramon, Dorumon, Gaomon, Hackmon, Kimeramon |
| Resistance | origin | 3/6 | At the start of combat, Resistance Digimon gain a shield and deal more damage for each other Resistance Digimon next to them. (3) 300 shield and +20% damage per adjacent ally; (6) 420 shield and +26% damage per adjacent ally. | Rebel | Armadillomon, Chuumon, Ryudamon, Monodramon, Elecmon, Terriermon, Paildramon |
| Abyss | origin | 3 | Abyss Digimon's attacks and abilities ignore part of the target's armor and magic resist. (3) damage ignores 45% of armor and MR. | Void | Keramon, Vegiemon, Leviamon |
| Valkyries | origin | 2 | Valkyries gain critical strike chance, and their attacks and abilities always critically strike enemies below half health. (2) +25% crit, always crit below 50% HP. | Valkyrie | Falcomon, Valkyrimon |
| Astro | origin | 3 | Astro Digimon's abilities cost less mana. (3) -15 max mana. | Astro | Gotsumon, Gammamon, BlackAgumon, Coronamon |
| Overload | origin | 2/4/6 | After dealing or taking 10 hits, an Overload Digimon blasts the nearest enemy for magic damage. Below half health, it heals itself instead. (2) 120 damage or healing every 10 hits; (4) 260 damage or healing every 10 hits; (6) 520 damage or healing every 10 hits. | Battlecast | Syakomon, Tapirmon, Betamon, Floramon, Hagurumon, Megadramon |
| Ark | origin | 1 | The Ark gains mana every second and cannot be stunned, but it does not make basic attacks. (1) +30 mana per second. | Starship | Paildramon |
| Sovereign | origin | 1 | Your team's basic attacks deal magic damage instead of physical. DigiDestined attacks deal true damage. (1) attacks deal magic damage (true for DigiDestined). | Paragon | Lilamon |
| Bounty Hunters | origin | 1 | Every copy of a Bounty Hunter you buy makes that Digimon stronger for the rest of the game: +6 ability power and +3% attack damage per copy, up to 9 copies. (1) +6 AP and +3% AD per copy bought. | Mercenary | Vikemon, Valkyrimon |
| Blade Masters | class | 3/6 | Blade Masters' basic attacks have a chance to strike two more times. (3) 30% chance for 2 extra strikes; (6) 55% chance for 2 extra strikes. | Blademaster | Kotemon, Hyokomon, Ryudamon, Elecmon, Hackmon, Lobomon |
| Gunners | class | 2/4 | Every fourth basic attack of a Gunner also fires extra shots at random enemies. (2) 4 extra shots; (4) 7 extra shots. | Blaster | Gazimon, Betamon, Commandramon, Agumon, Terriermon, Valkyrimon |
| Brawlers | class | 2/4 | Brawlers gain max health. (2) +200 HP; (4) +420 HP. | Brawler | Syakomon, Armadillomon, Kokuwamon, Gaomon, BlackAgumon, Leviamon |
| Bombers | class | 2 | Damage from a Bomber's ability stuns its targets, once per cast. (2) ability hits stun for 1.5s. | Demolitionist | Chuumon, ToyAgumon, Vikemon |
| Infiltrators | class | 2/4/6 | Infiltrators always leap to the enemy backline when combat starts. With the trait, they gain attack speed for 6 seconds at the start of combat, refreshed on each takedown. (2) +30% AS for 6s; (4) +60% AS for 6s; (6) +110% AS for 6s. | Infiltrator | Keramon, Tapirmon, Falcomon, Monodramon, Dracmon, Gumdramon, Kimeramon |
| Hackers | class | 2 | A Hacker's basic attacks raise the mana cost of the target's next ability. (2) +50% mana cost. | Mana-Reaver | Gizamon, Hackmon, Piedmon |
| Mystics | class | 2/4 | All your Digimon gain magic resist. (2) +35 MR; (4) +80 MR. | Mystic | Gammamon, Floramon, Aruraumon, Salamon, MarineAngemon |
| Protectors | class | 2/4 | Protectors gain a shield for 4 seconds when they cast their ability. It does not stack. (2) 15% max HP shield on cast; (4) 25% max HP shield on cast. | Protector | BlackGabumon, Biyomon, Kudamon, Lopmon, Megadramon |
| Snipers | class | 2/4 | Snipers deal more damage for each hex between them and their target. (2) +6% damage per hex; (4) +12% damage per hex. | Sniper | Hawkmon, Lunamon, Dorumon, Impmon, Coronamon |
| Sorcerers | class | 2/4/6 | All your Digimon gain ability power. (2) +18 AP; (4) +35 AP; (6) +60 AP. | Sorcerer | Solarmon, Patamon, Renamon, Shoutmon, Tentomon, Hagurumon, Vegiemon, Lucemon |
| Vanguards | class | 2/4 | Vanguards gain armor. (2) +60 armor; (4) +140 armor. | Vanguard | Gomamon, Tsukaimon, Gotsumon, Crabmon, Etemon |

## Lines

| Cost | Line (Rookie / Champion / final) | Traits | Attribute | Role | Abilities | Ability | Corpus |
|---|---|---|---|---|---|---|---|
| 1 | Hawkmon / Aquilamon / Silphymon | Time Keepers, Snipers | Free | Sniper | Feather Slash / Blast Laser / Top Gun | snipe: Shoots the farthest enemy for 370 magic damage. | TFT 3 Caitlyn |
| 1 | Kotemon / Raptordramon / Grademon | Cyborgs, Blade Masters | Data | Duelist | Meteor Slash / Crash Charge / Grade Slash | riposte: Blocks all damage for 1 second, then strikes the nearest enemy for 190 magic damage and stuns it for 1.5s. | TFT 3 Fiora |
| 1 | Gazimon / Gururumon / Astamon | Data Pirates, Gunners | Virus | Striker | Electric Stun Blast / Chaos Fire / Hellfire | smoke: Throws a smoke bomb at its target: 150 magic damage to it and enemies next to it, and their damage drops 50% for 3s. | TFT 3 Graves |
| 1 | Syakomon / Whamon / Dragomon | Overload, Brawlers | Virus | Bruiser | Black Pearl Blast / Tidal Wave / Thousand Whips | sap: Slams a tentacle in a line: 160 magic damage to each enemy hit, stealing 30% of their armor and magic resist for 6s. | TFT 3.5 Illaoi |
| 1 | BlackGabumon / BlackGarurumon / WereGarurumon | Dark Area, Protectors | Virus | Guardian | Little Horn / Freeze Fang / Garuru Kick | rally: Shields the 2 lowest-health allies for 70 and gives them 30% attack speed for 4s. | TFT 3 Jarvan IV |
| 1 | Keramon / Chrysalimon / Infermon | Abyss, Infiltrators | Free | Assassin | Crazy Giggle / Data Crusher / Hell's Grenade | isolate: Slashes the nearest enemy for 200 magic damage, or 500 if no ally stands next to it. | TFT 3 Kha'Zix |
| 1 | Armadillomon / Ankylomon / Shakkoumon | Resistance, Brawlers | Free | Bruiser | Diamond Shell / Megaton Press / Justice Beam | shield: Gains a 300 shield and taunts nearby enemies. | TFT 3 Malphite |
| 1 | Tapirmon / Bakemon / Phantomon | Overload, Infiltrators | Vaccine | Assassin | Nightmare Syndrome / Hell's Hand / Soul Chopper | stun: Deals 165 magic damage and stuns the target for 1.5s. | TFT 3.5 Nocturne |
| 1 | Gomamon / Ikkakumon / Zudomon | DigiDestined, Vanguards | Vaccine | Guardian | Marching Fishes / Harpoon Torpedo / Vulcan's Hammer | toss: Throws a buckler at the farthest enemy for 120 magic damage. It bounces back as a 192 shield. | TFT 3 Poppy |
| 1 | Solarmon / Clockmon / Wisemon | Time Keepers, Sorcerers | Vaccine | Mystic | Shiny Ring / Chrono Breaker / Pandora Dialogue | line: Fires through the target, dealing 190 magic damage to every enemy in a line. | TFT 3 Twisted Fate |
| 1 | Hyokomon / Piddomon / Angewomon | Celestials, Blade Masters | Vaccine | Sniper | Downward Cleave / Fire Phaser / Celestial Arrow | plumage: Gains 125% attack speed for 8s. | TFT 3 Xayah |
| 1 | Chuumon / Mamemon / MetalMamemon | Resistance, Bombers | Virus | Mystic | Cheese Bomb / Smiley Bomb / Energy Bomb | bomb: Throws a bomb at its target: 210 magic damage to it and enemies next to it. | TFT 3 Ziggs |
| 1 | Patamon / Angemon / MagnaAngemon | DigiDestined, Sorcerers | Data | Mystic | Boom Bubble / Hand of Fate / Gate of Destiny | bubble: Kicks a bubble at the healthiest enemy: 185 magic damage and a 2s stun. | TFT 3 Zoe |
| 2 | Renamon / Kyubimon / Taomon | DigiDestined, Sorcerers | Data | Mystic | Diamond Storm / Fox Tail Inferno / Talisman of Light | orb: Fires an orb through its target: 213 magic damage on the way out and 160 true damage on the way back. | TFT 3 Ahri |
| 2 | Shoutmon / Ballistamon / Shoutmon X3 | DigiXros, Sorcerers | Data | Mystic | Bellow Blaster / Hard Rock Soul / Flame Cannon | fireshield: Blasts its target and enemies next to it for 200 magic damage, then gains a 360 shield for 4s. | TFT 3 Annie |
| 2 | Kokuwamon / Guardromon / Andromon | Time Keepers, Brawlers | Data | Bruiser | Electro Strike / Destruction Grenade / Spiral Sword | grab: Pulls the farthest enemy next to it, dealing 275 magic damage and stunning it for 2s. Nearby allies attack that enemy. | TFT 3 Blitzcrank |
| 2 | Gizamon / Gesomon / Boltmon | Data Pirates, Hackers | Virus | Bruiser | Spiral Edge / Coral Crusher / Tomahawk Stinger | dunk: Dunks its target for 275 magic damage, doubled below 50% HP. A takedown lets it cast again at once. | TFT 3 Darius |
| 2 | Falcomon / Peckmon / Crowmon | Valkyries, Infiltrators | Vaccine | Assassin | Scratch Smash / Kunai Wing / Haguro | barrage: Fires 6 shots at random enemies, each dealing 85 magic damage. | TFT 3 Kai'Sa |
| 2 | Betamon / Seadramon / MegaSeadramon | Overload, Gunners | Virus | Sniper | Electric Shock / Ice Blast / Lightning Javelin | frenzy: For 4s: +6 range, 60% attack speed, and basic attacks deal 5% of the target's max HP as magic damage. | TFT 3.5 Kog'Maw |
| 2 | Commandramon / Sealsdramon / Darkdramon | Cyborgs, Gunners | Data | Sniper | M16 Assassin / Death Behind / Gungnir | dashshot: Fires twice at its target: a basic attack and a shot for 213 magic damage. | TFT 3 Lucian |
| 2 | Tsukaimon / Devimon / SkullSatamon | Dark Area, Vanguards | Virus | Guardian | Bad Message / Death Claw / Skull Hammer | dreadshield: Gains a 338 shield for 8s. While it holds, deals 42 magic damage per second to adjacent enemies. | TFT 3 Mordekaiser |
| 2 | Gotsumon / Golemon / Meteormon | Astro, Vanguards | Data | Guardian | Rock Fist / Golem Punch / Galactic Flare | crater: Erupts the ground under its target: 250 magic damage and a 2s stun. Adjacent enemies take half and a 1s stun. | TFT 3.5 Nautilus |
| 2 | Biyomon / Birdramon / Garudamon | Celestials, Protectors | Vaccine | Bruiser | Spiral Twister / Meteor Wing / Wing Blade | entrance: Dashes to the farthest enemy within 3 hexes, dealing 225 magic damage to it and enemies next to it and stunning them for 1.25s. | TFT 3 Rakan |
| 2 | Kudamon / Reppamon / Chirinmon | Celestials, Protectors | Vaccine | Duelist | Bullet Whirlwind / Lion Claw / Wind Cutter Sword | triple: Strikes its target three times. The third strike deals 200 more magic damage and stuns for 1.5s. | TFT 3 Xin Zhao |
| 2 | Ryudamon / Ginryumon / Hisyaryumon | Resistance, Blade Masters | Vaccine | Duelist | Katana Attack / Metal Armor Blade / Legendary Dragon Blade | lastbreath: Blinks to the farthest enemy within 3 hexes, stuns it for 1s and strikes it 3 times, plus 206 magic damage. | TFT 3 Yasuo |
| 2 | Monodramon / Strikedramon / Cyberdramon | Resistance, Infiltrators | Vaccine | Assassin | Crack Bite / Fang Strike / Desolation Claw | contempt: Strikes its target for 288 magic damage and steals 25% of its attack damage for the rest of combat. | TFT 3.5 Zed |
| 3 | Lunamon / Lekismon / Crescemon | Celestials, Snipers | Data | Sniper | Tearing Shot / Tear Arrow / Ice Archery | arrow: Fires an arrow at the farthest enemy that explodes for 240 magic damage, stunning enemies in the blast for 1.5s. | TFT 3 Ashe |
| 3 | Gammamon / Starmon / SuperStarmon | Astro, Mystics | Data | Support | Shooting Star / Meteor Shower / Haley Storm | meep: Hits its target for 398 magic damage and stuns it for 1s. Each cast leaves a meep that pays 1 gold after combat (up to 2). | TFT 3.5 Bard |
| 3 | Floramon / Sunflowmon / Lotosmon | Overload, Mystics | Data | Mystic | Rain of Pollen / Sunshine Beam / Serpent Ruin | poison: Poisons the nearest enemy that is not poisoned yet: 810 magic damage over 10s, and its healing is cut by a third. | TFT 3.5 Cassiopeia |
| 3 | Agumon / GeoGreymon / RizeGreymon | Time Keepers, Gunners | Vaccine | Sniper | Pepper Breath / Mega Burst / Trident Revolver | emp: Fires a pulse at a random enemy: 225 magic damage to it and enemies next to it, and their next ability costs 40% more mana. | TFT 3 Ezreal |
| 3 | Crabmon / Coelamon / Plesiomon | Data Pirates, Vanguards | Data | Guardian | Scissors Execution / Fossil Bite / Sorrow Blue | slam: Slams the ground, dealing 180 magic damage to enemies within 1 hex and stunning them for 1s. | TFT 3 Jayce |
| 3 | Aruraumon / Woodmon / Cherrymon | Dark Area, Mystics | Data | Support | Nemesis Ivy / Branch Drain / Illusion Mist | rally: Shields the 2 lowest-health allies for 105 and gives them 30% attack speed for 4s. | TFT 3 Karma |
| 3 | Elecmon / Leomon / SaberLeomon | Resistance, Blade Masters | Data | Duelist | Sparkling Thunder / Fist of the Beast King / Howling Crusher | chosen: For 5s: +30% attack speed, heals 6% of max HP per second, and basic attacks deal 105 more true damage. | TFT 3 Master Yi |
| 3 | Lopmon / Turuiemon / Antylamon | DigiDestined, Protectors | Data | Guardian | Blazing Ice / Ninja Fist / Asipatravana | slam: Slams the ground, dealing 180 magic damage to enemies within 1 hex and stunning them for 1s. | TFT 3 Neeko |
| 3 | ToyAgumon / Tankmon / MetalTyrannomon | DigiXros, Bombers | Vaccine | Bruiser | Plastic Blaze / Sub Machine Gun / Nuclear Laser | flame: Torches its target and enemies next to it for 225 magic damage and burns them (healing cut by a third). | TFT 3 Rumble |
| 3 | Dracmon / Sangloupmon / Matadormon | Dark Area, Infiltrators | Virus | Assassin | Eye of Nightmares / Sticker Blade / Thousand Arrows | backstab: Teleports behind its target and backstabs it: a critical strike for 200% attack damage plus 87 magic damage. | TFT 3 Shaco |
| 3 | Tentomon / Kabuterimon / MegaKabuterimon | DigiDestined, Sorcerers | Vaccine | Mystic | Super Shocker / Electro Shocker / Horn Buster | orbs: Fires 3 orbs at the healthiest enemy, each dealing 150 magic damage. | TFT 3 Syndra |
| 3 | Dorumon / Dorugamon / DoruGreymon | Cyborgs, Snipers | Data | Sniper | Dash Metal / Power Metal / Metal Meteor | tumble: Tumbles (50% dodge for 2s) and fires a bolt for 160% attack damage plus 105 true damage. | TFT 3.5 Vayne |
| 3 | Gaomon / Gaogamon / MachGaogamon | Cyborgs, Brawlers | Data | Bruiser | Rolling Upper / Dash Double Claw / Winning Knuckle | charge: Charges the farthest enemy: 102 magic damage to enemies in the way, then 255 magic damage and a 2s stun to the target. | TFT 3 Vi |
| 4 | Gumdramon / Arresterdramon / Slayerdramon | DigiXros, Infiltrators | Vaccine | Assassin | Ran-gum Break / Spin Caliber / Tenryu Slash | shark: Throws a lure at the farthest enemy. After 0.6s a shark bursts up: 263 magic damage to enemies there and a 1.5s stun. | TFT 3 Fizz |
| 4 | BlackAgumon / BlackGreymon / BlackMetalGreymon | Astro, Brawlers | Virus | Bruiser | Spitfire / Great Antler / Metal Slash | transform: The first cast digivolves it for the rest of combat: +322 HP, +40 AD, melee range. Every cast leaps at its target: 161 magic damage and a 1.5s stun to enemies next to it. | TFT 3.5 Gnar |
| 4 | Hackmon / BaoHuckmon / SaviorHuckmon | Cyborgs, Hackers, Blade Masters | Data | Duelist | Fifth Rush / Fif Cross / Meteor Flame | surge: Dashes through its target for 150% attack damage plus 210 magic damage. A takedown lets it dash again at the enemy with the most mana. | TFT 3 Irelia |
| 4 | Impmon / Beelzemon / Beelzemon BM | Dark Area, Snipers | Virus | Sniper | Bada Boom / Double Impact / Death Slinger | whisper: Every fourth shot is a whisper: 300% attack damage plus 140 magic damage. | TFT 3 Jhin |
| 4 | Terriermon / Gargomon / MegaGargomon | Resistance, Gunners | Vaccine | Sniper | Bunny Blast / Gargo Laser / Giant Missile | excited: Fires a rocket: 438 magic damage to its target and enemies next to it. After its first takedown in a combat it gains 50% attack speed. | TFT 3 Jinx |
| 4 | Lobomon / KendoGarurumon / MagnaGarurumon | Time Keepers, Blade Masters | Free | Duelist | Lobo Kendo / Solar Laser / Magna Missile | energyslash: Gains a 232 shield for 3s and slashes enemies next to it for 193 magic damage. Every third cast sends a wave through its target for 386. | TFT 3.5 Riven |
| 4 | Salamon / Gatomon / Ophanimon | DigiDestined, Mystics | Vaccine | Support | Puppy Howl / Lightning Paw / Sefirot Crystal | wish: Heals every ally for 263. | TFT 3 Soraka |
| 4 | Coronamon / Firamon / Apollomon | Astro, Snipers | Vaccine | Sniper | Petit Prominence / Fira Claw / Phoebus Blow | traps: Drops 3 traps near random enemies. After 1.2s each explodes for 193 magic damage to enemies within 1 hex and slows their attacks 30% for 3s. | TFT 3.5 Teemo |
| 4 | Hagurumon / Datamon / Gigadramon | Overload, Sorcerers | Virus | Mystic | Cog Crusher / Digital Bomb / Giga Heat | deathray: Burns a line through its target for 403 magic damage. One second later the line explodes for 322 more. | TFT 3.5 Viktor |
| 4 | Vegiemon / Vademon / Apocalymon | Abyss, Sorcerers | Virus | Mystic | Sweet Breath / Unidentified Flying Kiss / Darkness Zone | ray: Channels a ray through its target: three pulses of 219 magic damage to every enemy in the line. | TFT 3 Vel'Koz |
| 4 | Etemon / Etemon / Etemon | Time Keepers, Vanguards | Virus | Guardian | Dark Spirits / Banana Slip / Monkey Wrench | cyclone: Spins for 3s, dealing 420 magic damage over the spin to adjacent enemies. Each enemy is stunned for 1.5s the first time it is hit. | TFT 3 Wukong |
| 5 | Paildramon / Imperialdramon FM / Imperialdramon PM | Resistance, Ark | Free | Mystic | Mega Death / Positron Laser / Omni Blade | fleet: Launches 3 fighters at random enemies, each dealing 156 magic damage. | TFT 3 Aurelion Sol |
| 5 | Leviamon / Leviamon / Leviamon | Abyss, Brawlers | Virus | Bruiser | Biting Crush / Rostrum / Cauda | rupture: Ruptures a large area around its target: 240 magic damage and a 1.5s stun to enemies within 2 hexes. | TFT 3 Cho'Gath |
| 5 | Kimeramon / Millenniummon / Millenniummon | Cyborgs, Infiltrators | Virus | Assassin | Heat Viper / Time Unlimited / Dimension Destroyer | chronobreak: Stops time: every enemy is stunned for 1.25s and takes 50% attack damage plus 140 magic damage. It cannot be hurt for 0.6s. | TFT 3 Ekko |
| 5 | Vikemon / Vikemon / Vikemon | Data Pirates, Bounty Hunters, Bombers | Free | Striker | Viking Axe / Arctic Blizzard / Mjolnir | orbital: Calls an orbital strike on its target's area. After 1.5s, enemies within 2 hexes take 520 magic damage. | TFT 3 Gangplank |
| 5 | Lilamon / Sakuyamon / Sakuyamon | DigiDestined, Sovereign | Data | Support | Lila Shower / Izuna / Amethyst Mandala | gale: Sends a gale through its target: enemies in the line take 200 magic damage and are stunned for 1.5s. The 3 nearest allies gain 80% attack speed for 5s. | TFT 3.5 Janna |
| 5 | MarineAngemon / MarineAngemon / MarineAngemon | Celestials, Mystics | Vaccine | Support | Smiling Face / Ocean Love / Kahuna Waves | polymorph: Turns the 3 nearest enemies into harmless critters for 3s: they cannot act and take 10% more damage. | TFT 3 Lulu |
| 5 | Valkyrimon / Valkyrimon / Valkyrimon | Valkyries, Bounty Hunters, Gunners | Free | Sniper | Aurvandil's Arrow / Fenrir Sword / Laser Javelin | bullettime: Fires waves of arrows at its target's area: enemies within 2 hexes take 40% of their max HP as magic damage over 1.5s. | TFT 3 Miss Fortune |
| 5 | Piedmon / Piedmon / Piedmon | Time Keepers, Hackers | Virus | Bruiser | Trump Sword / Clown's Trick / Masks Square | lantern: Pulls the first fallen ally back into the fight with 40% HP and 40 mana (twice per combat). With no one to save, shields the weakest ally for 520. | TFT 3 Thresh |
| 5 | Megadramon / Machinedramon / Chaosdromon | Overload, Protectors | Virus | Bruiser | Ultimate Slicer / Infinity Cannon / Hyper Infinity Cannon | reel: Hooks the farthest enemy and reels it in, dealing 380 true damage. An enemy left below 25% HP is finished off; a takedown makes this ability cost 20 less mana. | TFT 3.5 Urgot |
| 5 | Lucemon / Lucemon Chaos Mode / Lucemon Shadowlord Mode | Dark Area, Sorcerers | Vaccine | Mystic | Grand Cross / Paradise Lost / Purgatorial Flame | meteors: Calls 4 meteors on random enemies, each dealing 200 magic damage. A meteor that knocks out its target stuns the enemies next to it for 1s. | TFT 3 Xerath |

## Items

| Item | Recipe | Effect |
|---|---|---|
| Data Reaper | Power Chip + Power Chip | +40% AD. Each takedown grants 20% attack damage for the rest of combat. |
| Giant Slayer | Power Chip + Speed Chip | +15% AD, +12% AS. Deals 10% more damage, and 40% more to enemies with 1600 or more max HP. |
| Data Gunblade | Power Chip + Data Rod | +15% AD, +15 AP. Heals for 25% of all damage dealt. |
| Mana Spear | Power Chip + Mana Card | +15% AD, +15 mana. Basic attacks restore 6 extra mana. |
| Rebirth Egg | Power Chip + Chrome Plate | +15% AD, +20 armor. The first time the holder would be knocked out, it stays at 1 HP and cannot be hurt for 1.5 seconds, then heals 40% of its max HP. |
| Blood Fang | Power Chip + Data Cloak | +15% AD, +20 MR. Heals for 30% of all damage dealt. |
| Battle Banner | Power Chip + HP Disk | +15% AD, +150 HP. At the start of combat, the holder and allies next to it in the same row gain 30% attack speed. |
| Crit Edge | Power Chip + Lucky Glove | +15% AD, +35% crit, +5% dodge. Gains 25% critical strike chance and 20% critical damage. |
| Long Shot | Speed Chip + Speed Chip | +34% AS. Gains 2 attack range. Attacks deal 8% more damage. |
| Rage Wing | Speed Chip + Data Rod | +12% AS, +15 AP. Each basic attack grants 5% attack speed, stacking. |
| Volt Disk | Speed Chip + Mana Card | +12% AS, +15 mana. Every third basic attack sends lightning to 3 enemies for 85 magic damage. |
| Titan Will | Speed Chip + Chrome Plate | +12% AS, +20 armor. Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks. |
| Twin Shot | Speed Chip + Data Cloak | +12% AS, +20 MR. Basic attacks also hit a second nearby enemy for 70% damage. |
| Data Portal | Speed Chip + HP Disk | +12% AS, +150 HP. When the holder is knocked out, a Mekanorimon rises in its place to keep fighting. |
| Armor Breaker | Speed Chip + Lucky Glove | +12% AS, +10% crit, +5% dodge. Basic attacks cut the target's armor by 40% for 3 seconds. |
| Ultima Rod | Data Rod + Data Rod | +40 AP. Increases the holder's total ability power by 30%. |
| Echo Rod | Data Rod + Mana Card | +15 AP, +15 mana. When the holder's ability hits, its target and up to 3 enemies near it take 160 more magic damage. |
| Holy Locket | Data Rod + Chrome Plate | +15 AP, +20 armor. At the start of combat, the holder and allies within 2 hexes in the same row gain a 260 shield. |
| Mana Zap | Data Rod + Data Cloak | +15 AP, +20 MR. When an enemy within 2 hexes casts its ability, it takes 180 magic damage. |
| Flame Tome | Data Rod + HP Disk | +15 AP, +200 HP. The holder's abilities burn their targets for 1.5% max HP per second and cut their healing by a third. |
| Jewel Gauntlet | Data Rod + Lucky Glove | +15 AP, +20% crit, +5% dodge. Abilities can critically strike. Gains 30% critical damage. |
| Mana Overflow | Mana Card + Mana Card | +30 mana. Max mana -10. After casting, keep 20 mana. |
| Frost Heart | Mana Card + Chrome Plate | +15 mana, +20 armor. Enemies within 2 hexes lose 30% attack speed. |
| Power Chalice | Mana Card + Data Cloak | +15 mana, +20 MR. At the start of combat, the holder and allies next to it in the same row gain 30 ability power. |
| Holy Ring | Mana Card + HP Disk | +15 mana, +150 HP. Every 5 seconds, heals allies within 2 hexes for 80 + 10% of their missing HP. |
| Justice Hand | Mana Card + Lucky Glove | +15 mana, +10% crit, +5% dodge. Deals 15% more damage and heals for 15% of damage dealt. |
| Spike Plate | Chrome Plate + Chrome Plate | +50 armor. Critical strikes against the holder deal normal damage. When attacked, deals 60 magic damage to adjacent enemies. |
| Sword Breaker | Chrome Plate + Data Cloak | +20 armor, +20 MR. Basic attacks have a 30% chance to disarm the target for 3 seconds: its attacks deal no damage. |
| Burning Core | Chrome Plate + HP Disk | +20 armor, +150 HP. Basic attacks burn the target for 1.5% max HP per second and cut its healing by a third. |
| Still Shroud | Chrome Plate + Lucky Glove | +20 armor, +10% crit, +5% dodge. At the start of combat, enemies in the holder's column and the columns beside it need 40% more mana for their first ability. |
| Dragon Scale | Data Cloak + Data Cloak | +50 MR. Takes 40% less magic damage. |
| Zephyr Gust | Data Cloak + HP Disk | +20 MR, +150 HP. At the start of combat, the enemy across from the holder is blown out of the fight for 4 seconds. |
| Quick Silver | Data Cloak + Lucky Glove | +20 MR, +10% crit, +5% dodge. Immune to stuns for the first 12 seconds of combat. |
| Giga Belt | HP Disk + HP Disk | +800 HP. A lot of health. |
| Trap Claw | HP Disk + Lucky Glove | +150 HP, +10% crit, +5% dodge. Blocks the first enemy ability that hits the holder and stuns its caster for 3 seconds. |
| Thief Glove | Lucky Glove + Lucky Glove | +20% crit, +10% dodge. Each combat, gains two random completed items. |
| Golden Digimental | Digimental + Digimental | +1 team size while held by a Digimon on your board. |
| Data Copier | consumable | Drag onto a Digimon to get a Rookie-form copy of it on your bench. |
| Magnetic Remover | consumable | Drag onto a Digimon to return all of its items to your item bench. |
| Reforger | consumable | Drag onto a Digimon to return its items to your bench as random new items of the same kind. |

## Augments

| Tier | Augment | Effect |
|---|---|---|
| Silver | Spare Change | Gain 8 gold. |
| Silver | Item Cache | Gain 2 random components. |
| Silver | Rolling Start | Gain 1 free reroll every round. |
| Silver | Combat Training | Your team gains 12% AD and 12 AP. |
| Silver | Front Line | Digimon in your front row gain 30 armor and MR and 200 HP. |
| Silver | Back Line | Digimon in your back two rows gain 25% attack speed. |
| Silver | Starlight | Your team heals 30% of its missing health after 10 seconds of combat. |
| Gold | Digi-Bank | Gain 18 gold. |
| Gold | Treasure Trove | Gain a random completed item and a component. |
| Gold | Spare Digimental | Gain a Digimental. |
| Gold | Steady Income | Gain 2 extra gold every round. |
| Gold | Celestial Blessing | Your team heals for 12% of the damage it deals. |
| Gold | Jewel Lotus | Your abilities can critically strike, and your team gains 15% crit. |
| Gold | Cyber Implants | Your Digimon holding items gain 250 HP and 15% AD. |
| Prismatic | Jackpot | Gain 35 gold. |
| Prismatic | Big Armory | Gain 2 random completed items. |
| Prismatic | Golden Digimental | Gain a Golden Digimental (+1 team size). |
| Prismatic | Giant Grab | Your team gains 25% max HP. |
| Prismatic | Star Power | Every origin trait you have active counts one extra Digimon. |
| Prismatic | Mega Digivolve | Gain a random Champion-form (2-star) Digimon of cost 4. |
| Prismatic | Overcharge | Your team starts combat with 30 extra mana and 20 AP. |

## Galaxies

| Galaxy | Rule | Corpus galaxy | Odds |
|---|---|---|---|
| File Island | No special rules this game. | Normal | 12% |
| Spiral Mountain | Every wild round drops an extra component. From stage 4 it drops a completed item instead. | Treasure Trove | 6% |
| Infinity Mountain | Every Tamer starts with the same two components. | Galactic Armory | 6% |
| Primary Village | At 1-2, the Digimon you took from the opening carousel digivolves straight to its Mega form (3-star). | Star Cluster | 6% |
| Mirror Sector | Every Tamer gets a Data Copier at 2-1 and at 3-1. Drag it onto a Digimon to get a Rookie copy of it. | The Neekoverse | 6% |
| Folder Continent | From 2-1, every team can field one more Digimon. | Medium Legends | 6% |
| Trade Sector | Every Tamer gets one free reroll every round. | Trade Sector | 6% |
| Superdense | At 3-1, every Tamer gets a Golden Digimental (+1 team size). | Superdense | 6% |
| Factorial Town | Selling a Digimon breaks its completed items back into their components. | Salvage World | 6% |
| Plunder Planet | Each enemy your team knocks out in a PvP round has a 30% chance to drop 1 gold. | Plunder Planet | 6% |
| Digimental Shrine | Every Tamer starts with a Digimental. | Manatee's Delight | 6% |
| Big Bang | One free reroll every round, an extra component from every wild round, and a Golden Digimental for every Tamer at 3-1. | Big Bang | 12% |
| Dark Ocean | All Digimon deal 15% more damage, and all healing is 30% weaker. | new | 6% |
| Data Stream | All Digimon start combat with 25 extra mana. | new | 6% |
| Server Continent | Shops show 6 Digimon instead of 5. | new | 6% |

## Models

Every form uses an existing `models/index.json` key (the Faction Wars, Dragonlands and Auto Chess conversions and the extras), assigned in `js/sets/galaxies-models.js`. `node tests/models.cjs galaxies` reports 0 gaps in 222 forms. Forms with no rip were swapped for a Digimon that has one, keeping the role: Octomon to Whamon, ShadowWereGarurumon to WereGarurumon, OmniShoutmon and Shoutmon X7 to Ballistamon and Shoutmon X3, MarineDevimon to Boltmon, Tankdramon to Darkdramon (Masters rip), ShootingStarmon to Gammamon, Kiwimon to Sunflowmon, Arresterdramon SM to Slayerdramon, BeoWolfmon to MagnaGarurumon, Ebemon to Apocalymon, MetalEtemon and KingEtemon to Etemon, Imperialdramon DM to Paildramon, ZeedMillenniummon to Millenniummon, Rosemon to Sakuyamon. Wild Kuramon became Kunemon, Apocalymon became Armageddemon, Yggdrasil became Azulongmon, and the Data Portal construct is Mekanorimon.

## Balance

Tools: `tools/balance.cjs --pace hyper|normal`. Duels have no economy, so they do not depend on the pace.

**Duels** (`--duel 5000 8 1`), win rate of boards with each trait active. Every trait is in 47.6-56.7%: Sorcerers 56.7, Gunners 55.9, Snipers 55.6, Bombers 54.8, Celestials 54.7, DigiDestined 54.5, Abyss 54.3, Mystics 54.0, Sovereign 53.6, Hackers 53.0, Blade Masters 52.8, Brawlers 52.6, Overload 52.5, Ark 52.4, Time Keepers 51.9, Cyborgs 51.9, Infiltrators 51.7, Protectors 51.5, Data Pirates 51.2, Valkyries 51.2, DigiXros 50.2, Bounty Hunters 49.6, Astro 49.0, Resistance 48.7, Dark Area 47.7, Vanguards 47.6.

**Hyper Roll, 300 games** (`--pace hyper`): 25.4 combat rounds a game (22-31), ending in stage 7 (210 games) or 8 (83). Wild rounds won by stage: 90, 93, 91, 91, 87, 66, 62%. Top-4 rate on the final board by cost: 1-cost 34-56% (small samples, mean 45), 2-cost 36-47% (41), 3-cost 48-57% (52), 4-cost 63-71% (67), 5-cost 78-92% (84). No line sits outside its cost band.

**Normal, 300 games** (`--pace normal`): 34.2 combat rounds a game (30-40), ending in stage 6 (101 games) or 7 (198). Wild rounds won by stage: 91, 84, 96, 86, 44, 46%. Top-4 by cost: 1-cost 19-59% (small samples, mean 40), 2-cost 38-51% (45), 3-cost 40-54% (49), 4-cost 56-62% (59), 5-cost 59-70% (65).

**Each Galaxy** (24 forced games each, both paces): no Galaxy moves the average game end by more than about a round. Hyper Roll: 7-3 (File Island) to 7-7 (Folder Continent and Superdense, where +1 team size makes fights longer). Normal: 6-4 to 6-7 (Primary Village; measured before the last Normal damage change, which lengthens every Galaxy alike). Primary Village (a 3-star carousel unit at 1-2) makes early wild rounds about 100% won; Dark Ocean lowers mid-game wild round wins to about 80%. The Galaxy is the same for all 8 Tamers, so none decides who wins.

## Engine notes

- The Galaxy is announced at 1-1 through `mechanic.intro`; there is no 1-2 pick any more.
- A hook after the opening carousel is built would allow Lilac Nebula (4-cost carousel).
