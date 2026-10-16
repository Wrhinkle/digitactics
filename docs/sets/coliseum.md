# K.O. Coliseum (`coliseum`)

TFT Set 15 "K.O. Coliseum" (corpus: `docs/inspo/tft/set-15-ko-coliseum.md`) as a Digimon fighting tournament.

Files: `js/sets/coliseum.js` (data), `coliseum-mech.js` (hooks, ability types, mechanic), `coliseum-art.js` (item, augment and offer-card art, move and basic-attack specs), `coliseum-models.js` (model keys).

## Overview

- 60 shop lines: 13 / 13 / 13 / 11 / 10 by cost. The corpus has 61 shop champions (64 minus Lulu's three monsters). Naafiri was dropped; Akali and Volibear moved from 4 to 5 cost.
- 26 traits: 15 origins and 11 classes, one per corpus trait.
- Items: 8 stat components and 2 special components. The Digi-Egg plays TFT's Spatula and makes origin Crests; the Crest Tag plays the Frying Pan and makes class Crests. The set has 36 recipes, 18 Crests (Protector and Sniper drop only) and 3 team-size items (Golden Digi-Egg, Golden Tag and Digivice, the corpus Tactician items). It also has 8 Artifacts from boss rounds and the stage 5+ Armory, and 4 consumables: Digi-Core, Data Extractor, Magnetic Remover and Reforger.
- 30 augments: 10 Silver, 12 Gold and 8 Prismatic.
- Every line is a different Digimon, and every form uses a converted model (`node tests/models.cjs coliseum`: 0 gaps). Three summons share the line format: Machinedramon (Mega Mech), Woodmon and Togemon (Rose Queen plants). There are 19 wild lines for the tournament brackets.

## Set mechanic: Digi-Cores (Power Ups as branching digivolution)

- Every Tamer gets a Digi-Core at the start of 1-3 and 3-3 in Hyper Roll, and at 1-3 and 3-6 in Normal (the corpus Power Snax timing). If the item bench is full, the Digi-Core waits for space.
- Drag it onto a Digimon to pick 1 of 3 digivolution paths for it (offer kind `col_core`, one reroll). The choices depend on what the Digimon does:
  - attack carries are offered `ad` paths;
  - casters are offered `ap` paths;
  - front liners are offered `tank` paths;
  - `any` paths go to everyone.
- The path is stored in `unit.mods.pu`. It stays when the Digimon digivolves, and `onCombine` merges paths from the copies.
- A new Digi-Core replaces the old path. Selling the Digimon, or using a Data Extractor on it, gives the Digi-Core back. If two copies with paths combine, the extra Digi-Core comes back too.
- AI Tamers feed each Digi-Core to their best item holder that has no path yet, and pick among its three offers at random. Picking at random keeps the full-game path numbers unbiased.
- There are 24 paths. Each adapts a corpus Power Up (table below).

### Forms (Stance Master and Monster Tamer)

When you get Justimon (Lee Sin) or Labramon (Lulu), an offer of kind `col_form` asks you to pick its form. AI Tamers pick at once.

- Justimon's Blitz, Critical or Trinity Arm gives it the Duelist, Executioner or Juggernaut trait. The trait counts through `bonusTraits`, and Justimon becomes a member through the SM trait hook, which runs first.
- Each arm also changes its stats and its ability (`col_stance`).
- Labramon's partner changes its stats and ability (`col_monster`):
  - Ballistamon is a tank;
  - Dorulumon is a diver;
  - Sparrowmon is a ranged attacker.

### Jewel Gambit (Crystal Gambit)

- Kills and losses in player combats earn Gem Power. Each kill is worth 1, 1.5 or 2 by stage, and each loss 4, 6 or 8.
- Every 3 player combats, the `col_gambit` offer lets you choose:
  - **Cash out**: gold equal to half the Gem Power, plus a component at 25 Gem Power, a completed item at 45, or two completed items at 80;
  - **Double Down**: losses earn double, but a win halves the Gem Power and cashes it out at once.
- With 5 Jewel Gambit Digimon, every 8 kills also gives a component. With 7, cash-outs are worth 30% more.
- Jewel Gambit Digimon also gain a small amount of HP and damage at every level. Without it the trait was dead weight in combat (40% duel win rate).

### Other uniques

- **Pirate Captain** (Piedmon) draws a Bounty card each round it is fielded. Rewards are gold, rerolls, ability power for Piedmon, a team shield, components, or late in the game big gold and completed items.
- **The Champ** (Craniamon) earns fans equal to its star level for each player-combat win. On a loss, each fan blocks 1 damage, then the fans reset.
- **Rose Queen** (Lotosmon) brings two plants, four at three stars. Lotosmon's casts heal the plants, speed them up and regrow dead ones.
- **Mega Mech** summons Machinedramon in the front row. Its stats scale with the star levels of Mega Mech Digimon and the game stage, and Mega Mech Digimon heal it for 12% of the damage they deal.
- **Sensei** is active only with exactly 1 or 4 Sensei (`onCountTraits`).
- **Monster Tamer** and **Stance Master** are the forms above.

### Set ability types

| Type | Used by | What it does |
|---|---|---|
| `col_frenzy` | Gabumon (Gnar), Merukimon (Volibear) | Attack damage and attack speed buff shared with the 2 nearest allies, then a hit |
| `col_bulk` | Goblimon (Dr. Mundo) | Gains max HP and heals; the next attack deals 8% of max HP |
| `col_aegis` | Gotsumon (Rell), GeoGreymon (Leona), Bearmon (Poppy) | Shield, then damage within 2 hexes |
| `col_infect` | Vegiemon (Malzahar) | Damage over 6 seconds that moves to the next enemy when its victim falls |
| `col_stance` | Justimon | Depends on the arm |
| `col_monster` | Labramon | Depends on the partner |

The K'Sante line (Bakemon) has the corpus All Out passive. The first time it would fall, it comes back at 25% HP with attack speed and attack damage, then loses 5% HP every second.

## Economy and rounds

- **Hyper Roll** is the native pace, using the engine Hyper Roll rules:
  - 20 HP and flat loss damage of 2, 4 or 6;
  - levels set by the stage, and fixed income;
  - Armory at x-1 of stages 2-6, and augments at 2-2, 3-2 and 4-2;
  - wild rounds at 1-2, 1-3 and x-4.
- **Normal** pace uses the engine standard TFT rules: 100 HP, XP, interest, streaks, and wild rounds at 1-2 to 1-4 and x-7. Two set rules change:
  - Digi-Cores move to 1-3 and 3-6.
  - The wild brackets are the same, with some fighters one star higher, because the x-7 round comes at the end of a longer stage.
- **Wild rounds** are a tournament bracket. Stages 1-4 are headlined by Kimeramon and stages 5-6 by Darkdramon. Stage 7 brings Royal Knights and Imperialdramon Paladin Mode, and stage 8 brings Apocalymon and Millenniummon. Boss wins from stage 5 can drop Artifacts.

## Model swaps

Every form must use a converted model. Ten Digimon picked first had no converted model, so they were swapped for converted Digimon in the same role:

| Line | First pick | Swapped to |
|---|---|---|
| Shoutmon | Shoutmon / OmniShoutmon | Labramon / Dobermon. The Ballistamon and Dorulumon partner names stay. |
| Rosemon | Rosemon | Lotosmon |
| Gankoomon | Gankoomon | Merukimon |
| Arcadiamon (bosses) | Arcadiamon | Kimeramon and Darkdramon |
| Mundo line | Digitamamon | Fugamon |
| Malzahar line | Ebemon | Arukenimon |
| Senna line | RustTyranomon | Chaosdramon |
| Poppy line | Pandamon | Grizzlymon |
| Sett line | GrapLeomon | Panjyamon |
| Two Sistermon forms, Ravemon Burst Mode, Ophanimon Falldown Mode | as named | the base form, repeated |

## Traits

| Trait | Kind | Breakpoints | Effect per breakpoint | Lines |
|---|---|---|---|---|
| Digi Academy | origin | 3/5/7 | 3: 3 Potential: abilities deal 27% more damage, +150 max HP; 5: 5 Potential: abilities deal 45% more damage, +250 max HP; 7: 7 Potential: abilities deal 63% more damage, +350 max HP | Lunamon, Zubamon, Falcomon, Biyomon, Sistermon Noir, Ikkakumon, GeoGreymon, Salamon |
| Jewel Gambit | origin | 3/5/7 | 3: +80 HP, +4% damage. Earn Gem Power from kills and losses; 5: +150 HP, +9% damage. Also: every 8 kills, gain a component (the count then rolls 6-10); 7: +280 HP, +18% damage. Also: every 8 kills a component; cash-outs are worth 30% more | Syakomon, Wormmon, Gaomon, Saberdramon, Sistermon Ciel (Awakened), Lotosmon |
| Sensei | origin | 1/4 | 1: Exactly 1 Sensei: your whole team gains its lesson; 4: Exactly 4 Sensei: the Sensei gain every lesson and 10 ability power | Kamemon, Reppamon, Kotemon, Wizardmon |
| Mega Mech | origin | 3/5/7 | 3: Machinedramon joins the fight; 5: Blade Protocol 1: Machinedramon gains 15% attack damage and its attacks also hit a second enemy for 30%; 7: Blade Protocol 2: also 25% omnivamp and 20% less damage taken | Hagurumon, Kokuwamon, Monodramon, Tyrannomon, Coredramon (Green), Keramon, HiAndromon |
| Monster Tamer | origin | 1 | 1: Pick a partner for Labramon | Labramon |
| Pirate Captain | origin | 1 | 1: Draw a Bounty card every round; Street Crew deal 12% extra true damage | Piedmon |
| Rose Queen | origin | 1 | 1: Lotosmon brings 2 plants (4 at three stars) | Lotosmon |
| Spirit Fighters | origin | 2/4/6 | 2: +150 HP, +2.5% AD/AP per second, 12% true damage at 8 stacks; 4: +300 HP, +4% AD/AP per second, 18% true damage at 8 stacks; 6: +500 HP, +6% AD/AP per second, 25% true damage at 8 stacks | Lobomon, Patamon, Ryudamon, Sangloupmon, Impmon, Leomon, Puppetmon |
| Stance Master | origin | 1 | 1: Pick an arm for Justimon | Justimon |
| Star Wards | origin | 2/3/4/5/6/7/8 | 2: Each Star Ward bonus x1; 3: Each Star Ward bonus x1.05; 4: Each Star Ward bonus x1.1; 5: Each Star Ward bonus x1.12; 6: Each Star Ward bonus x1.14; 7: Each Star Ward bonus x1.16; 8: Each Star Ward bonus x1.2 | Gotsumon, Syakomon, Hawkmon, Kyubimon, Lalamon, Gargomon, Bearmon, Ophanimon |
| Supreme Virus | origin | 2/3/4 | 2: +10% damage; the Supreme deals +12% and finishes enemies below 10% HP; 3: +24% damage; the Supreme deals +32% and finishes enemies below 10% HP; 4: +30% damage; the Supreme deals +50% and finishes enemies below 10% HP; two Supremes | Elecmon, FanBeemon, Kuwagamon, Ravemon |
| The Champ | origin | 1 | 1: Wins bring fans; fans block damage on a loss | Craniamon |
| Street Crew | origin | 2/3/4/5 | 2: +4% max HP and attack speed; rockets every 400 damage; 3: +8% max HP and attack speed; rockets every 400 damage; 4: +10% max HP and attack speed; rockets every 400 damage; 5: +13% max HP and attack speed; rockets every 400 damage | Armadillomon, Betamon, Lopmon, Mamemon, Piedmon |
| Phantoms | origin | 2/4/6 | 2: The Shadow Realm deals 27% of Phantom damage every 4 seconds; 4: The Shadow Realm deals 33% of Phantom damage every 4 seconds; 6: The Shadow Realm deals 48% of Phantom damage every 4 seconds | Tsukaimon, Numemon, DemiDevimon, Vegiemon, Bakemon, Myotismon |
| Luchadors | origin | 2/4 | 2: +10% AD, heal 15%; 4: +35% AD, heal 35% | Gabumon, Goblimon, Craniamon, Merukimon |
| Bastion | class | 2/4/6 | 2: Bastions +10 armor and MR (double for 10 seconds); 4: Bastions +22 armor and MR (double for 10 seconds); 6: Bastions +40 armor and MR (double for 10 seconds); others +25 more | Zubamon, Gotsumon, Lopmon, Ryudamon, Saberdramon, GeoGreymon, Craniamon |
| Duelist | class | 2/4/6 | 2: +8% attack speed per attack (12 stacks); 4: +14% attack speed per attack (12 stacks); 6: +18% attack speed per attack (12 stacks); 12% less damage taken | Tsukaimon, Monodramon, FanBeemon, Reppamon, Sangloupmon, Sistermon Ciel (Awakened) |
| Swordmaster | class | 2/4/6 | 2: +22% AD, 10% omnivamp; 4: +35% AD, 12% omnivamp; 6: +60% AD, 15% omnivamp | Lopmon, Hawkmon, Sangloupmon, Kotemon, Impmon, HiAndromon, Merukimon |
| Executioner | class | 2/3/4/5 | 2: +35% crit, +15% crit damage; 3: +50% crit, +22% crit damage; 4: +55% crit, +25% crit damage; 5: +60% crit, +30% crit damage | Lobomon, Falcomon, Tyrannomon, Wizardmon, Ravemon |
| Heavyweight | class | 2/4/6 | 2: +12% max HP, AD +1.2% of max HP; 4: +25% max HP, AD +2% of max HP; 6: +40% max HP, AD +3% of max HP | Hagurumon, Numemon, Kamemon, Kuwagamon, Ikkakumon, Bearmon |
| Juggernaut | class | 2/4/6 | 2: -8% damage taken (-16% above 50% HP); 4: -10% damage taken (-18% above 50% HP); 6: -13% damage taken (-21% above 50% HP) | Hagurumon, Goblimon, Gaomon, Reppamon, Leomon |
| Prodigy | class | 2/3/4/5 | 2: Prodigies +5 mana per second; 3: Prodigies +7 mana per second; 4: Prodigies +9 mana per second; 5: Prodigies +11 mana per second; their ability damage heals an ally for 12% | Lunamon, Syakomon, Vegiemon, Salamon, Ophanimon |
| Protector | class | 2/4/6 | 2: 11% max HP shield; 4: 19% max HP shield; 6: 30% max HP shield | Elecmon, Armadillomon, Wormmon, Biyomon, Lalamon, Bakemon |
| Sniper | class | 2/3/4/5 | 2: +15% damage, +3% per hex; 3: +20% damage, +6% per hex; 4: +26% damage, +7% per hex; 5: +24% damage, +8% per hex | Gabumon, Betamon, DemiDevimon, Sistermon Noir, Gargomon, Myotismon |
| Sorcerer | class | 2/4/6 | 2: +18 AP, 8% max HP burst; 4: +38 AP, 10% max HP burst; 6: +65 AP, 12% max HP burst | Elecmon, Kokuwamon, Patamon, Kyubimon, Saberdramon, Keramon, Puppetmon |
| Strategist | class | 2/3/4/5 | 2: front 120 shield, back +3% damage; 3: front 160 shield, back +5% damage; 4: front 250 shield, back +8% damage; 5: front 340 shield, back +12% damage | Wormmon, Mamemon, Coredramon (Green), Wizardmon |

## Lines (adapted TFT Set 15 champions)

| Cost | Forms | Traits | Ability type | Abilities | Adapts (TFT15) |
|---|---|---|---|---|---|
| 1 | Hagurumon / Guardromon / Andromon | Mega Mech, Juggernaut, Heavyweight | execute | Cog Crusher / Destruction Grenade / Spiral Sword | Aatrox |
| 1 | Lunamon / Lekismon / Crescemon | Digi Academy, Prodigy | nuke | Lop-Ear Ripple / Tear Arrow / Ice Archery | Ezreal |
| 1 | Zubamon / ZubaEagermon / Duramon | Digi Academy, Bastion | drain | Twenty Dive / Road Two Decade / Glass Slash | Garen |
| 1 | Gabumon / Garurumon / WereGarurumon | Luchadors, Sniper | col_frenzy | Little Horn / Fox Fire / Garuru Kick | Gnar |
| 1 | Lobomon / KendoGarurumon / MagnaGarurumon | Spirit Fighters, Executioner | line | Licht Sieger / Solar Laser / Machinegun Destroyer | Kalista |
| 1 | Tsukaimon / Devimon / SkullSatamon | Phantoms, Duelist | barrage | Bad Message / Death Claw / Nail Bone | Kayle |
| 1 | Elecmon / Aegiomon / IceLeomon | Supreme Virus, Protector, Sorcerer | slam | Sparkling Thunder / Stun Beat Blow / Fist of Ice | Kennen |
| 1 | Kokuwamon / Tankmon / Datamon | Mega Mech, Sorcerer | barrage | Electro Strike / Hyper Cannon / Digital Bomb | Lucian |
| 1 | Armadillomon / Ankylomon / Shakkoumon | Street Crew, Protector | slam | Diamond Shell / Tail Hammer / Disaster Blaster | Malphite |
| 1 | Gotsumon / Starmon / SuperStarmon | Star Wards, Bastion | col_aegis | Rock Fist / Meteor Shower / Haley Storm | Rell |
| 1 | Betamon / Seadramon / MegaSeadramon | Street Crew, Sniper | chain | Electric Shock / Ice Blast / Lightning Javelin | Sivir |
| 1 | Syakomon / Sorcermon / Meteormon | Jewel Gambit, Star Wards, Prodigy | nuke | Black Pearl Blast / Crystal Barrage / Galactic Flare | Syndra |
| 1 | Numemon / Raremon / BlackKingNumemon | Phantoms, Heavyweight | drain | Party Time / Sludge / Dark Pupil | Zac |
| 2 | Goblimon / Ogremon / Fugamon | Luchadors, Juggernaut | col_bulk | Goblin Strike / Pummel Whack / Evil Hurricane | Dr. Mundo |
| 2 | Monodramon / Strikedramon / Cyberdramon | Mega Mech, Duelist | execute | Knuckle Beater / Fang Strike / Desolation Claw | Gangplank |
| 2 | Wormmon / Hudiemon / Dinobeemon | Jewel Gambit, Protector, Strategist | rally | Sticky Net / Infinity Dream / Hell Masquerade | Janna |
| 2 | DemiDevimon / IceDevimon / LadyDevimon | Phantoms, Sniper | nuke | Demi Darts / Frost Claw / Darkness Wave | Jhin |
| 2 | FanBeemon / Waspmon / CannonBeemon | Supreme Virus, Duelist | barrage | Gear Stinger / Turbo Stinger / Sky Rocket | Kai'Sa |
| 2 | Falcomon / Peckmon / Crowmon | Digi Academy, Executioner | dash | Scratch Smash / Kunai Wing / Savage Emperor | Katarina |
| 2 | Kamemon / Golemon / Jijimon | Sensei, Heavyweight | shield | Shell Guard / Golem Punch / Guard Stick | Kobuko |
| 2 | Patamon / Angemon / MagnaAngemon | Spirit Fighters, Sorcerer | aoe | Air Shot / Heaven's Knuckle / Heaven's Gate | Lux |
| 2 | Biyomon / Birdramon / Garudamon | Digi Academy, Protector | heal | Spiral Twister / Meteor Wing / Shadow Wing | Rakan |
| 2 | Lopmon / Turuiemon / Antylamon | Street Crew, Bastion, Swordmaster | shield | Blazing Ice / Ninja Fist / Asipatravana | Shen |
| 2 | Gaomon / Gaogamon / MachGaogamon | Jewel Gambit, Juggernaut | drain | Rolling Upper / Gaoga Hound / Winning Knuckle | Vi |
| 2 | Hawkmon / Aquilamon / Silphymon | Star Wards, Swordmaster | barrage | Feather Slash / Blast Laser / Top Gun | Xayah |
| 2 | Ryudamon / Ginryumon / Hisyaryumon | Spirit Fighters, Bastion | aoe | Kabutogaeshi / Metal Armor Blade / Legendary Dragon Blade | Xin Zhao |
| 3 | Kyubimon / Taomon / Sakuyamon | Star Wards, Sorcerer | execute | Fox Tail Inferno / Talisman of Light / Amethyst Mandala | Ahri |
| 3 | Sistermon Noir / Sistermon Noir / SaviorHuckmon | Digi Academy, Sniper | nuke | Mickey Bullet / Bless Fire / Meteor Flame | Caitlyn |
| 3 | Kuwagamon / Okuwamon / GranKuwagamon | Supreme Virus, Heavyweight | execute | Scissor Claw / Double Scissor Claw / Dimension Scissor | Darius |
| 3 | Ikkakumon / Zudomon / Vikemon | Digi Academy, Heavyweight | aoe | Harpoon Torpedo / Vulcan's Hammer / Mjolnir | Jayce |
| 3 | Labramon / Dobermon / Dobermon | Monster Tamer | col_monster | Retriever Bark / Black Cannon / Black Cannon Burst | Lulu (Monster Trainer) |
| 3 | Vegiemon / Vademon / Arukenimon | Phantoms, Prodigy | col_infect | Sweet Breath / Unidentified Flying Kiss / Spider Thread | Malzahar |
| 3 | Lalamon / Sunflowmon / Lilamon | Star Wards, Protector | slam | Seed Blast / Sunshine Beam / Lila Shower | Neeko |
| 3 | Tyrannomon / MetalTyrannomon / Chaosdramon | Mega Mech, Executioner | line | Fire Breath / Nuclear Laser / Hyper Mugen Cannon | Senna |
| 3 | Saberdramon / Megadramon / Gigadramon | Jewel Gambit, Bastion, Sorcerer | drain | Night Roar / Ultimate Slicer / Giga Heat | Swain |
| 3 | Reppamon / Chirinmon / Kentaurosmon | Sensei, Juggernaut, Duelist | drain | Lion Claw / Wind Cutter Sword / Bifrost | Udyr |
| 3 | Sangloupmon / Matadormon / GranDracmon | Spirit Fighters, Duelist, Swordmaster | stun | Sticker Blade / Thousand Arrows / Crystal Revolution | Viego |
| 3 | Kotemon / Raptordramon / Grademon | Sensei, Swordmaster | aoe | Meteor Slash / Crash Charge / Grade Slash | Yasuo |
| 3 | Mamemon / MetalMamemon / PrinceMamemon | Street Crew, Strategist | aoe | Smiley Bomb / Energy Bomb / Smile Warhead | Ziggs |
| 4 | Sistermon Ciel (Awakened) / Sistermon Ciel (Awakened) / Dianamon | Jewel Gambit, Duelist | barrage | Silent Bullet / Piercing Wasp / Arrow of Artemis | Ashe |
| 4 | Coredramon (Green) / Groundramon / Breakdramon | Mega Mech, Strategist | slam | Strike Bomber / Megaton Hammer Crush / Destroyed Rush | Jarvan IV |
| 4 | Gargomon / Rapidmon / MegaGargomon | Star Wards, Sniper | aoe | Gargo Laser / Miracle Missile / Giant Missile | Jinx |
| 4 | Bakemon / Phantomon / Titamon | Phantoms, Protector | stun | Hell's Hand / Soul Chopper / Phantom Blade | K'Sante |
| 4 | Keramon / Chrysalimon / Infermon | Mega Mech, Sorcerer | aoe | Crazy Giggle / Data Crusher / Hell's Grenade | Karma |
| 4 | GeoGreymon / RizeGreymon / ShineGreymon | Digi Academy, Bastion | col_aegis | Mega Burst / Trident Revolver / Glorious Burst | Leona |
| 4 | Bearmon / Grizzlymon / Monzaemon | Star Wards, Heavyweight | col_aegis | Bear Fist / Crescent Moon Kick / Hearts Attack | Poppy |
| 4 | Wizardmon / Wisemon / Piximon | Sensei, Executioner, Strategist | aoe | Thunder Cloud / Eternal Nirvana / Pit Bomb | Ryze |
| 4 | Impmon / Beelzemon / Beelzemon Blast Mode | Spirit Fighters, Swordmaster | dash | Bada Boom / Double Impact / Corona Destroyer | Samira |
| 4 | Leomon / Panjyamon / BanchoLeomon | Spirit Fighters, Juggernaut | drain | Fist of the Beast King / Ice Fist / Flashy Boss Punch | Sett |
| 4 | Salamon / Gatomon / Angewomon | Digi Academy, Prodigy | barrage | Puppy Howl / Lightning Paw / Celestial Arrow | Yuumi |
| 5 | Ravemon / Ravemon / Ravemon | Supreme Virus, Executioner | dash | Blast Wing / Raven King Sword / Crimson Formation | Akali |
| 5 | Craniamon / Craniamon / Craniamon X | The Champ, Luchadors, Bastion | slam | End Waltz / Breath of the Gods / Grand Gridr | Braum |
| 5 | Puppetmon / Puppetmon / Puppetmon | Spirit Fighters, Sorcerer | line | Puppet Pummel / Flying Cross Cutter / Wood Cross | Gwen |
| 5 | Justimon / Justimon / Justimon | Stance Master | col_stance | Voltage Fist / Justice Kick / Trinity Arm | Lee Sin |
| 5 | Ophanimon / Ophanimon / Ophanimon | Star Wards, Prodigy | aoe | Sefirot Crystal / Eden's Javelin / Flame Hellscythe | Seraphine |
| 5 | Piedmon / Piedmon / Piedmon | Pirate Captain, Street Crew | barrage | Trump Sword / Clown's Trick / Ending Illusion | Twisted Fate |
| 5 | Myotismon / VenomMyotismon / VenomMyotismon | Phantoms, Sniper | line | Crimson Lightning / Venom Infusion / Chaos Flame | Varus |
| 5 | HiAndromon / HiAndromon / HiAndromon | Mega Mech, Swordmaster | line | Lightning Blade / Atomic Ray / Atomic Ray | Yone |
| 5 | Lotosmon / Lotosmon / Lotosmon | Jewel Gambit, Rose Queen | aoe | Thorn Whip / Serpent Cure / Serpent Ruin | Zyra |
| 5 | Merukimon / Merukimon / Merukimon | Luchadors, Swordmaster | col_frenzy | Thousand Fists / Spiritual Enchantment / Mirage Image | Volibear |

## Digi-Core paths

| Path | Offered to | Effect | Adapts |
|---|---|---|---|
| Burst Mode | ad, ap | Deals 12% more damage, 25% more after its first takedown each combat. | Annihilation |
| X-Antibody | tank | When hit, gains 4 armor (physical) or 8 MR (magic), up to 90 in total. | Adaptive Skin |
| Dark Digivolution | ad | Gains 22% attack damage and 10% omnivamp, but loses 1% max HP every second. | All Out |
| Warp Digivolution | any | Gains 8% attack damage, 8 AP, 10 armor and MR, and deals 5% more damage. | 404 Power Up |
| Armor Digivolution | tank | Starts combat with a 25% max HP shield. While shielded, gains 30% attack speed. | Living Wall |
| Mode Change | tank, ap | At the start of combat, it and its nearest ally gain an 18% max HP shield. | Body Change |
| Slide Evolution | ad | When it switches targets, its next attack deals 120% bonus damage as magic damage. | Blink Attack |
| Spirit Evolution | any | The first time it drops below 40% HP, heals 25% max HP and gains 25% attack speed. | Not Done Yet |
| Death-X Evolution | ad | Leaps to the enemy backline at the start of combat and gains 30% crit chance and 10% crit damage. | Assassinate |
| Biomerge | ad, ap | Gains 3% attack damage and 3 AP every second, up to 30 times. | Hero's Arc |
| Mega Digivolution | ad | Gains 40% attack damage. | Max Attack |
| Sage Evolution | ap | Gains 25 AP. | Max Arcana |
| Golem Form | tank | Gains 30% max HP. | Max Vitality |
| Speed Form | ad | Gains 30% attack speed. | Max Speed |
| Overwrite | ad, ap | Gains 30% crit chance and 15% crit damage; its abilities can critically strike. | Critical Threat |
| Matrix Evolution | ap | Its max mana is 25% lower and it starts combat with 10 extra mana. | Efficient |
| Overdrive | any | Deals 12% more damage. After its first takedown or 10 seconds of combat, stuns enemies within 2 hexes for 1.5 seconds and deals 20% more damage for the rest of combat. | 2XKO |
| Atomic Core | tank | Stores 10% max HP at the start of combat and 2% more every second. When it falls, deals the stored amount as magic damage to enemies within 2 hexes. | Atomic |
| Thorn Armor | tank | When attacked, deals 50 + 5% of its max HP as magic damage to adjacent enemies (1 second cooldown). | Spiky Shell |
| Glacier Mode | ad | Gains 15% attack speed. Attacks slow the target's attack speed by 30% for 2 seconds. | Frost Touch |
| Inferno Mode | ap | Its abilities burn their targets, and it deals 15% more damage to burning enemies. | Inner Fire |
| Hunter Mode | ad, ap | Deals 8% more damage, and 30% more to enemies below 50% HP. | Killer Instinct |
| Guardian Mode | tank | Gains 30 armor and MR. Adjacent allies gain 10 armor and MR. | Resistant |
| Ascension | ad, ap | After 6 seconds of combat, deals 40% more damage. | Final Ascent |

## Items (recipes)

Components: Power Chip (+15% AD), Speed Chip (+12% AS), Data Rod (+15 AP), Mana Card (+15 starting mana), Chrome Plate (+20 armor), Data Cloak (+20 MR), HP Disk (+150 HP), Lucky Glove (+15% crit), plus the special Digi-Egg and Crest Tag. Artifacts: Doom Grasp, Gold Collector, Lich Bane, Hull Crusher, Static Shiv, Titanic Hydra, Zhonya Paradox, Infinity Force.

| Item | Recipe | Effect |
|---|---|---|
| Giga Blade | Power Chip + Power Chip | +55% AD. Deals 10% more damage. |
| Titan Slayer | Power Chip + Speed Chip | +15% AD, +12% AS, +10 AP. Deals 8% more damage, and 25% more to enemies with over 1600 max HP. |
| Data Gunblade | Power Chip + Data Rod | +15% AD, +15 AP. Heals for 18% of damage dealt. Also heals the lowest-health ally for 15% of damage dealt. |
| Shojin Spear | Power Chip + Mana Card | +15% AD, +15 mana. Attacks restore 5 extra mana. |
| Night Cloak | Power Chip + Chrome Plate | +15% AD, +20 armor. The first time HP drops below 60%, become untouchable for 1 second, shake off stuns and gain 25% attack speed. |
| Blood Fang | Power Chip + Data Cloak | +15% AD, +20 MR. Heals for 20% of damage dealt. Below 40% HP, gain a 25% max HP shield once. |
| Sterak Gauntlet | Power Chip + HP Disk | +15% AD, +250 HP. The first time HP drops below 60%, gain a 40% max HP shield and 30% attack damage. |
| Crit Edge | Power Chip + Lucky Glove | +25% AD, +30% crit. Abilities can critically strike. +10% crit damage. |
| Red Fang | Speed Chip + Speed Chip | +49% AS. Damage burns the target for 1.5% max HP per second and cuts its healing by a third. |
| Rage Rod | Speed Chip + Data Rod | +12% AS, +15 AP. Each attack grants 5% attack speed, stacking. |
| Void Staff | Speed Chip + Mana Card | +12% AS, +15 mana, +10 AP. Damage cuts the target's MR by 30% for 5 seconds. |
| Titan Will | Speed Chip + Chrome Plate | +12% AS, +20 armor. Attacking or being attacked grants 2% AD and 2 AP, up to 25 stacks. |
| Kraken Fang | Speed Chip + Data Cloak | +12% AS, +20 MR. Each attack grants 3.5% AD, stacking. |
| Nashor Fang | Speed Chip + HP Disk | +12% AS, +150 HP, +10 AP. Attacks restore 3 extra mana. |
| Armor Breaker | Speed Chip + Lucky Glove | +12% AS, +15% crit. Attacks cut the target's armor by 30% for 3 seconds. |
| Ultima Rod | Data Rod + Data Rod | +60 AP. Deals 8% more damage. |
| Seraph Staff | Data Rod + Mana Card | +15 AP, +15 mana. Gain 15 AP every 5 seconds. |
| Crown Guard | Data Rod + Chrome Plate | +15 AP, +20 armor. Start combat with a 25% max HP shield. After 8 seconds, gain 30 AP. |
| Ion Spark | Data Rod + Data Cloak | +15 AP, +20 MR. When an enemy within 2 hexes casts, deal 150 magic damage to it. Enemies within 2 hexes lose 15 armor and MR. |
| Flame Tome | Data Rod + HP Disk | +15 AP, +150 HP. Damage burns the target for 1.5% max HP per second and cuts its healing by a third. |
| Jewel Gauntlet | Data Rod + Lucky Glove | +15 AP, +25% crit. Abilities can critically strike. +15% crit damage. |
| Blue Card | Mana Card + Mana Card | +30 mana, +10 AP, +10% AD. Max mana -10. After casting, keep 20 mana. |
| Guardian Vow | Mana Card + Chrome Plate | +20 mana, +20 armor. Below 40% HP, gain a 25% max HP shield and 15 mana once. |
| Adaptive Helm | Mana Card + Data Cloak | +15 mana, +20 MR. Gain 10 mana every 3 seconds. In the front two rows, gain 25 armor and MR; in the back two rows, gain 15% AD and 15 AP. |
| Spirit Visage | Mana Card + HP Disk | +15 mana, +300 HP. Regenerates 1.5% max HP per second. |
| Justice Hand | Mana Card + Lucky Glove | +15 mana, +15% crit. Deals 12% more damage and heals for 12% of damage dealt. |
| Bramble Plate | Chrome Plate + Chrome Plate | +40 armor. Takes 6% less damage. When attacked, deals 80 magic damage to adjacent enemies (1.5s cooldown). |
| Gargoyle Stone | Chrome Plate + Data Cloak | +20 armor, +20 MR, +100 HP. Gain 10 armor and MR for each enemy within 2 hexes. |
| Sunfire Core | Chrome Plate + HP Disk | +20 armor, +250 HP. Every 2 seconds, burns the nearest enemy within 2 hexes. |
| Steadfast Heart | Chrome Plate + Lucky Glove | +20 armor, +15% crit. Takes 10% less damage, 18% less while above 50% HP. |
| Dragon Scale | Data Cloak + Data Cloak | +40 MR, +100 HP. Regenerates 1.2% max HP per second. |
| Even Shroud | Data Cloak + HP Disk | +20 MR, +150 HP. Enemies within 2 hexes lose 25 armor and MR. Gain 25 armor and MR for the first 15 seconds. |
| Quick Silver | Data Cloak + Lucky Glove | +20 MR, +15% crit. Immune to stuns for the first 18 seconds. Each attack grants 2% attack speed, stacking. |
| Warmog Disk | HP Disk + HP Disk | +700 HP. Gain 10% more max HP. |
| Striker Flail | HP Disk + Lucky Glove | +150 HP, +15% crit, +10% AS. Critical strikes make the holder deal 5% more damage for 5 seconds, stacking up to 4 times. |
| Thief Glove | Lucky Glove + Lucky Glove | +30% crit. Each combat, gains two random completed items. |

## Augments

| Augment | Tier | Effect |
|---|---|---|
| Pocket Change | Silver | Gain 8 gold. |
| Band of Thieves | Silver | Gain a Thief Glove. |
| Component Cache | Silver | Gain 2 random components. |
| Rolling Start | Silver | Gain 1 free reroll every round. |
| Cheering Crowd | Silver | Gain 5 Tamer HP. |
| Warm-Up | Silver | Your team gains 12% AD and 12 AP. |
| Front Row Seats | Silver | Digimon in your front row gain 30 armor and MR and 200 HP. |
| Ringside Snipers | Silver | Digimon in your back two rows gain 25% attack speed. |
| Extra Snack | Silver | Gain a Digi-Core and a random component. |
| Second Wind | Silver | After 10 seconds of combat, your team heals 35% of its missing HP. |
| Prize Money | Gold | Gain 18 gold. |
| Treasure Trove | Gold | Gain a random completed item and a component. |
| Ascension | Gold | After 15 seconds of combat, your team deals 40% more damage. |
| Sponsorship | Gold | Gain 2 extra gold every round. |
| Gym Crest | Gold | Gain a random Crest for a trait on your board. |
| Tag Team | Gold | Gain a Crest Tag and a Digi-Egg. |
| Snack Time | Gold | Gain a Digi-Core and 6 gold. |
| Jeweled Lotus | Gold | Your abilities can critically strike, and your team gains 15% crit. |
| Stand United | Gold | Your team gains 3% AD and 3 AP for every active trait. |
| Heavy Training | Gold | Your team gains 15% max HP. |
| Instant Digivolve | Gold | Gain a random two-star Digimon of cost 2 or 3. |
| Relic Hunter | Gold | Gain a random Artifact. |
| Grand Prize | Prismatic | Gain 35 gold. |
| Big Armory | Prismatic | Gain 2 random completed items. |
| Golden Egg | Prismatic | Gain a Golden Digi-Egg (+1 team size). |
| Mega Digivolve | Prismatic | Gain a random two-star 4-cost Digimon. |
| Giant Grab | Prismatic | Your team gains 25% max HP. |
| Digi-Core Feast | Prismatic | Gain 2 Digi-Cores and a random completed item. |
| Spirit of the Arena | Prismatic | Every active origin trait counts one extra Digimon. |
| Finals Night | Prismatic | Your team deals 12% more damage and takes 10% less damage. |


## Balance results (2026-10-15)

### Duels

`node tools/balance.cjs --duel 5000 8 1 --set coliseum`: random 7-unit boards, no items. Duels do not depend on the pace.

All 26 traits fall within 48.7-58.1%:

| Win rate | Traits |
|---|---|
| 58.1% | Mega Mech (n=403, the smallest sample) |
| 56-57% | Sniper 57.2, Sensei 56.5, Protector 56.4, Rose Queen 56.2, Star Wards 56.2, Phantoms 56.1 |
| 55% | Executioner 55.4, Bastion 55.4, Jewel Gambit 55.1 |
| 53-55% | Street Crew 54.5, Prodigy 54.1, Pirate Captain 54.1, Juggernaut 53.9, Luchadors 53.8, Monster Tamer 53.6, Digi Academy 53.3 |
| 50-53% | Sorcerer 52.9, Spirit Fighters 50.9, Duelist 50.6, Swordmaster 50.5, Heavyweight 50.4, Stance Master 50.4 |
| 48-50% | Supreme Virus 49.7, Strategist 49.2, The Champ 48.7 |

The duel's random boards depend on the worker count, because its RNG is seeded per worker chunk. Runs with 6 workers drew a different board set and landed within 47.5-58.6%.

The set has its own stat templates (`roles`): a sturdier back line and a slightly softer front line than the engine defaults. In the first runs, this set's many defensive traits made front-line Digimon win about 65% of duels and casters about 40%.

### Digi-Core paths

The path test, `scratchpad/col_pu.cjs` (6000 fights), puts the same board on both sides in mirrored cells. Only one Digimon's path differs: the main carry on even seeds and the tank on odd seeds. Each path's win rate is over all its pairings.

No path is dominant. All 24 fall within 40.7-58.4%:

| Win rate | Paths |
|---|---|
| 55-58% | Guardian Mode 58.4, Mode Change 56.7, Armor Digivolution 55.3, Hunter Mode 55.1 |
| 50-55% | Speed Form 54.3, Atomic Core 54.1, Dark Digivolution 53.0, Spirit Evolution 51.4, Inferno Mode 50.7, Matrix Evolution 50.4, Overwrite 50.0 |
| 46-50% | Glacier Mode 49.8, Overdrive 49.8, Golem Form 49.4, Burst Mode 49.2, Ascension 47.7, Biomerge 46.5, Death-X 46.3 |
| 40-45% | X-Antibody 44.7, Slide Evolution 44.7, Warp 44.3, Sage 43.9, Thorn Armor 43.7, Mega Digivolution 40.7 (n=199, the noisiest) |

In full games every path lands between about 35% and 70% top-4, but each path appears on only 10-100 final boards.

### Full games, Hyper Roll

`node tools/balance.cjs 300 8 --set coliseum --pace hyper`:

- **Length:** 24.7 combat rounds a game (median 25, range 20-29). 225 of 300 games end in stage 7, 24 in stage 6 and 51 in stage 8.
- **Wild rounds won by stage:** 98% in stage 1, then 93, 98, 94, 82, 81 and 47% in stage 7.
- **Lines by cost** (top-4 rate, lines with at least 30 boards):

  | Cost | Top-4 range | Lines |
  |---|---|---|
  | 4 | 70-82% | Coredramon at the top |
  | 5 | 68-77% | |
  | 3 | 53-59% | |
  | 2 | 37-49% | |
  | 1 | 31-41% | Betamon at 54% (n=41) |

- **No outliers beyond what cost explains.** Elecmon (1-cost, 31%) is the low end. The AI keeps three-trait 1-costs on late boards as trait fillers, so they sit on weak boards. Elecmon and Syakomon carry stat multipliers to offset this.
- **Traits** range from 49% to 63% top-4. Mega Mech shows 88% but on only 32 boards.

### Full games, Normal

`node tools/balance.cjs 300 8 --set coliseum --pace normal`:

- **Length:** 34.9 combat rounds a game (median 35, range 31-40). 236 games end in stage 7 and 60 in stage 6.
- **Wild rounds won by stage:** 95% in stage 1, then 89, 99, 89 and 87% in stage 5.
  - The 300-game run had stage 6 at 23%, because its lead fighter became a three-star Darkdramon.
  - After the stage 6+ upgrade was changed to the helpers only, a 120-game check gave stage 6 66% and stage 7 39% (n=18).
- **Lines by cost:**

  | Cost | Top-4 range |
  |---|---|
  | 4 | 59-68% |
  | 5 | 44-62% |
  | 3 | 44-59% |
  | 2 | 33-49% |
  | 1 | 22-51% |

- **Traits** range from 45% to 69% top-4. Monster Tamer shows 69% on 36 boards, and Jewel Gambit is lowest at 45%.

### Tests

| Test | Result |
|---|---|
| `node tests/sim.js 20 --set coliseum` | 24.8 combat rounds a game |
| `node tests/sim.js 20 --set coliseum --pace normal` | 35.3 combat rounds a game |
| `node tests/lockstep.js 3 --set coliseum` | Passed in both paces |
| `node tests/sets.cjs --set coliseum` | Passed |
| `node tests/moves.cjs <playwright> <out> soft --set coliseum` | Passed: 82 lines, no generated looks |
| `node tests/models.cjs coliseum` | 246 forms, 0 gaps |
| Solo browser game with `DT.UI.newGame('coliseum')` | Played to 3-2 with the Digi-Core offer shown; no page errors |

## Engine notes

- The unit tooltip shows a Digimon's Digi-Core path and its Stance Master / Monster Tamer form (`unitNote` in `coliseum-mech.js`).
- Dragging the Digi-Core or the Data Extractor over a Digimon shows the item's own description; the Remover and Reforger show the built-in item texts (`preview`).
- The AI buys a line whose `ai.value` is 6 or more even with no trait started, so Labramon (22) and Justimon (14) get bought.
