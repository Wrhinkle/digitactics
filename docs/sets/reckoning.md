# Reckoning

TFT Set 5 "Reckoning" (patch 11.14) and Set 5.5 "Dawn of Heroes" (patch 11.21) with Digimon. The Dark Spore stands in for the Black Mist: Spore components make Shadow items that trade a drawback for power, and the Radiant Blessing from 5.5 lifts Tamers who fall low.

Files: `js/sets/reckoning.js` (data), `js/sets/reckoning-mech.js` (hooks, custom abilities, the mechanic, loot and codex text), `js/sets/reckoning-art.js` (item, Crest and augment portraits, move and basic-attack looks), `js/sets/reckoning-models.js` (model keys). Corpus: `docs/inspo/tft/set-05-reckoning.md`, `docs/inspo/tft/set-05-5-dawn-of-heroes.md`.

## Overview

- 60 shop lines: 13 / 13 / 13 / 11 / 10 by cost. Each adapts one champion from Set 5 or 5.5 (`src` on the line). Every form in the set is a different Digimon, and every form uses a model already converted into `models/index.json` (`node tests/models.cjs reckoning`: 252 forms, 0 gaps). Five summon lines (Kimeramon, Monzaemon, Gumdramon, Garurumon, Voidlarva) and 19 wild Digimon lines.
- 27 traits: 15 origins and 12 classes, adapted from Set 5 and the 5.5 additions (Sentinel, Cannoneer, Inanimate). A few corpus traits were folded into abilities or dropped (see the last section).
- Items: 8 components, 8 Spore components, the Digi-Egg, 36 completed items, 36 Shadow items, 36 Radiant items, 24 Crests, Force of Nature, Magnetic Remover and Tome of Traits.
- 24 augments (8 Silver, 9 Gold, 7 Prismatic). Set 5 had no augments; these are Reckoning-flavored and several tie into the mechanic (Spore Cache, Shadow Trove, X-Antibody, Early Blessing, Force of Darkness, Tome Collection).
- Four custom ability types: `hook` (Thresh: pulls the farthest enemy and stuns it), `summon` (Ivern and Heimerdinger: call a partner, or heal it and make it cast again), `possess` (Viego: a target that falls within 3 seconds rises for your team and decays) and `respite` (Kindred: allies near it cannot drop below 1 HP for a few seconds).

## The mechanic: Shadow and Radiant

- **Spore components** have the same stats as the regular ones. A Spore component plus any component (either way round, or two Spore components) makes the Shadow version of that recipe. Each Shadow item lists its three pairs in its `recipes` field.
- **Shadow items** are stronger and carry a drawback written on the item (less max HP, HP cost per attack or cast, a slower team-mate, a weaker side effect). A drawback never knocks out its holder. The X-Antibody augment halves drawbacks. Spore components come from the Armory, wild Digimon loot (30%) and, in Hyper Roll, the 3-1 Shadow carousel (half its items); in Normal pace from every carousel after the first.
- **Digi-Egg + Spore component** makes the 8 Crests the Set 5 Shadow Spatula made (Dark Spore, Tricksters, Dragon Hunter, Witchelny, Cavalier, Phantom, Nightfall, Chimera). Digi-Egg + component makes the 8 regular Crests. 8 more Crests have no recipe and come from the Tome of Traits (5.5), the Armory and loot.
- **Radiant items** are every regular item, much stronger. They are never random drops. The Radiant Armory (Hyper Roll 4-1, Normal 3-6, as in 5.5) lets everyone pick one of 4. A pick upgrades a copy of the regular item you hold (board first), or goes to the item bench.
- **Radiant Blessing** (5.5 Divine Blessing): once per game, at the first round with no other pick after you drop to 8 HP (Hyper Roll) or 40 HP (Normal), you get gold and a pick of 3 Radiant upgrades with one reroll.
- **Dragon Kin eggs** (Draconic in Hyper Roll): after each PvP combat a Digi-Egg is added; it hatches into a loot orb after 1 to 3 rounds (bigger eggs, better loot; golden at 5 Dragon Kin).

## Economy and rounds

Reckoning's native pace is Hyper Roll, the mode Set 5 launched. Both paces share lines, traits, items and hooks.

- **Hyper Roll** (flat `rules`): 20 HP, levels by stage, fixed income, 1-gold rerolls, loss damage 2 / 4 / 6. Shop odds use the Set 5 (11.9) values at levels 7 and 8. Rounds: 1-1 carousel, 1-2 and 1-3 wild; then x-1 to x-3 PvP and x-4 wild, except 3-1, the Shadow carousel. Armory at 2-1, 3-3, 5-1 and 6-1; Radiant Armory at 4-1; augments at 2-2, 3-2, 4-2. AI games last 24.6 combat rounds and end at stage 6.85 on average.
- **Normal** (`rules.normal` over the engine's standard TFT rules): 100 HP, XP, interest and streaks, stage damage plus unit damage, carousels at x-4 with Spore components, wild Digimon at 1-2 to 1-4 and x-7. Armory at 2-5, 4-5 and 5-5; Radiant Armory at 3-6; augments at 2-1, 3-2, 4-2; Blessing at 40 HP. AI games last 39.5 combat rounds and end at stage 6.65 on average. Level 10 shop odds are added for this pace.
- **Wild Digimon** scale by stage through `rules.pveMul` (one table per pace): Koromon pests, the Sludge Pit, Dino Valley, Etemon's Dark Network, the Black Mist, the Dark Ocean, the Demon Lords, Millenniummon and Apocalymon, then Armageddemon.

## Dropped or folded corpus pieces

- Verdant (Set 5 only) was dropped; Caretaker became Clockmon's `summon` ability (Gumdramon is the baby dragon); Cruel (bought with Tamer HP) was dropped because the AI cannot weigh HP as currency. God-King became Royal Rival (Gallantmon and Megidramon), Eternal became Twin Souls (Sakuyamon with Garurumon), Inanimate became Hallowed Mist (Durandamon's line). Victorious was not used.

## Traits

| Trait | Kind | Adapts | Breakpoints | Effect and values |
|---|---|---|---|---|
| Dark Spore | origin | Forgotten | 2/4/6 | Dark Spore Digimon gain attack damage and ability power. Each Shadow item held by a Dark Spore Digimon makes the bonus 20% bigger, up to 4 items. (2) +15% AD and +15 AP, +20% per Shadow item; (4) +30% AD and +30 AP, +20% per Shadow item; (6) +55% AD and +55 AP, +20% per Shadow item |
| Tricksters | origin | Hellion | 3/5/7 | Tricksters gain attack speed. When a Trickster is knocked out, a copy one form lower jumps in with 55% HP. Copies do not copy again. (3) +10% AS, copies on knockout; (5) +25% AS, copies on knockout; (7) +55% AS, copies on knockout |
| Seraphic | origin | Redeemed | 3/5/7 | Seraphic Digimon gain armor, magic resist and ability power. When one is knocked out, its bonus is split among the Seraphic Digimon still standing. (3) +14 armor, MR and AP; (5) +28 armor, MR and AP; (7) +50 armor, MR and AP |
| Nightfall | origin | Nightbringer | 2/4/6 | The first time a Nightfall Digimon drops below 50% HP, it gains a shield for 8 seconds and deals more damage for the rest of combat. (2) 25% HP shield, +15% damage; (4) 45% HP shield, +25% damage; (6) 70% HP shield, +35% damage |
| Daybreak | origin | Dawnbringer | 2/4/6 | The first time a Daybreak Digimon drops below 50% HP, it quickly heals part of its max HP. Each time this happens, every Daybreak Digimon deals 8% more damage. (2) heal 20% HP, team Daybreak +8% damage; (4) heal 30% HP, team Daybreak +8% damage; (6) heal 50% HP, team Daybreak +8% damage |
| Chimera | origin | Abomination | 3/4/5 | When 3 of your Digimon have been knocked out, Kimeramon rises where the third fell. It copies one random item from up to 3 Chimera Digimon and grows with their forms. (3) Kimeramon: 1200 HP, 100 AD; (4) Kimeramon: 1800 HP, 150 AD; (5) Kimeramon: 2500 HP, 200 AD |
| Witchelny | origin | Coven | 3 | At the start of combat, the Digimon closest to the center of your Witchelny Digimon becomes the Coven Leader and gains ability power. Each time a Witchelny Digimon casts, the Leader gains 30% of that ability's mana cost. (3) Leader +80 AP, 30% mana share |
| Dragon Kin | origin | Draconic | 3/5 | Dragon Kin Digimon gain max HP. After each player combat you get a Digi-Egg that hatches into loot after 1 to 3 rounds; bigger eggs take longer and hold better loot. With 5, the eggs are golden and hold more. (3) +200 HP, egg after each PvP combat; (5) +400 HP, golden egg after each PvP combat |
| Dragon Hunter | origin | Dragonslayer | 2/4 | Dragon Hunters gain ability power. The first time a Dragon Hunter knocks out an enemy with 1500 or more max HP, your whole team gains ability power for the rest of combat. (2) +30 AP, team +20 AP after a big kill; (4) +70 AP, team +50 AP after a big kill |
| Chrome Digizoid | origin | Ironclad | 2/3 | Your whole team gains armor. (2) team +55 armor; (3) team +120 armor |
| Phantom | origin | Revenant | 2/3/4 | The first time a Phantom Digimon would be knocked out, it rises again with part of its max HP. After rising, it deals 20% and takes 30% more damage. (2) rise with 6% HP; (3) rise with 14% HP; (4) rise with 28% HP |
| Sentinel | origin | Sentinel (5.5) | 3/5 | At the start of combat, the Sentinel holding the most items gains a shield that grants attack speed. When the shield breaks or runs out (at most once per duration), it jumps to the ally with the lowest HP and renews, and its attack speed grows each jump (up to 5 times). (3) 120 shield, +10% AS per jump, 4s; (5) 350 shield, +20% AS per jump, 3s |
| Royal Rival | origin | God-King | 1 | Gallantmon and Megidramon are rivals. A Royal Rival deals 20% more damage to enemies with one of its rival traits, but only while you field exactly one of them. Gallantmon's rivals: Dark Spore, Nightfall, Witchelny, Tricksters, Dragon Hunter, Chimera, Phantom. Megidramon's rivals: Seraphic, Daybreak, Dragon Kin, Chrome Digizoid, Sentinel. (1) +20% damage vs rivals (exactly 1) |
| Twin Souls | origin | Eternal | 1 | Sakuyamon fights with a partner: at the start of combat, Garurumon appears next to it at the same form. Garurumon does not use a team slot. (1) Garurumon fights beside it |
| Hallowed Mist | origin | Inanimate (5.5) | 1 | At the start of combat, this Digimon and the allies next to it take 33% less damage for 8 seconds. (1) 33% less damage for 8s nearby |
| Assassin | class | Assassin | 2/4 | Assassins leap to the enemy backline when combat starts. Their abilities can critically strike, and they gain critical strike chance and damage. (2) +10% crit, +15% crit damage; (4) +25% crit, +35% crit damage |
| Brawler | class | Brawler | 2/4 | Brawlers gain max HP. (2) +250 HP; (4) +650 HP |
| Cavalier | class | Cavalier | 2/3/4 | Cavaliers take less damage. For the first 4 seconds of combat and for 3 seconds after each ability, the reduction is doubled. (2) -10% damage taken, doubled at times; (3) -14% damage taken, doubled at times; (4) -18% damage taken, doubled at times |
| Invoker | class | Invoker | 2/4 | All your Digimon gain extra mana from their basic attacks. (2) team +3 mana per attack; (4) team +6 mana per attack |
| Knight | class | Knight | 2/4/6 | All your Digimon block a flat amount of damage from every hit (burns are not blocked). (2) team blocks 15 per hit; (4) team blocks 30 per hit; (6) team blocks 50 per hit |
| Legionnaire | class | Legionnaire | 2/4/6 | Legionnaires gain attack speed. Their first basic attack after an ability heals them for 50% of its damage. (2) +20% AS; (4) +45% AS; (6) +80% AS |
| Mystic | class | Mystic | 2/3/4 | All your Digimon gain magic resist. (2) team +20 MR; (3) team +40 MR; (4) team +70 MR |
| Ranger | class | Ranger | 2/4 | Starting 4 seconds into combat, Rangers gain a burst of attack speed for 4 seconds, every 8 seconds. (2) +70% AS in bursts; (4) +150% AS in bursts |
| Renewer | class | Renewer | 2/4 | Renewers heal a share of their max HP every second. At full HP they gain that share of their mana instead. (2) 2.5% HP or mana per second; (4) 5% HP or mana per second |
| Skirmisher | class | Skirmisher | 3/6 | Skirmishers start combat with a shield and gain attack damage every second. (3) 25% HP shield, +3 AD per second; (6) 45% HP shield, +6 AD per second |
| Spellweaver | class | Spellweaver | 2/4 | Spellweavers gain ability power, and more each time any Digimon casts an ability, up to 10 times. (2) +30 AP, +4 per cast; (4) +65 AP, +7 per cast |
| Cannoneer | class | Cannoneer (5.5) | 2/4 | Every 5th basic attack of a Cannoneer also fires a cannon shot that deals a share of that attack's damage to the target and enemies next to it. (2) every 5th attack: 200% splash; (4) every 5th attack: 450% splash |

## Lines

| Line | Cost | Forms | Traits | Ability | Adapts |
|---|---|---|---|---|---|
| rec_zubamon | 1 | Zubamon / ZubaEagermon / Grademon | Seraphic, Legionnaire | drain | TFT5 Aatrox |
| rec_otamamon | 1 | Otamamon / Gekomon / ShogunGekomon | Daybreak, Brawler | slam | TFT5 Gragas |
| rec_keramon | 1 | Keramon / Chrysalimon / Infermon | Chimera, Legionnaire | line | TFT5 Kalista |
| rec_wormmon | 1 | Wormmon / Stingmon / Dinobeemon | Daybreak, Assassin | execute | TFT5 Kha'Zix |
| rec_chuumon | 1 | Chuumon / Sukamon / KingSukamon | Tricksters, Cavalier | dash | TFT5 Kled |
| rec_coronamon | 1 | Coronamon / Aegiomon / Duramon | Seraphic, Knight | stun | TFT5 Leona |
| rec_penguinmon | 1 | Penguinmon / Sorcermon / Witchmon | Witchelny, Renewer | weaken | TFT5 Lissandra |
| rec_gomamon | 1 | Gomamon / Ikkakumon / Vikemon | Sentinel, Skirmisher | drain | TFT5.5 Olaf |
| rec_kamemon | 1 | Kamemon / Golemon / Knightmon | Tricksters, Knight | shield | TFT5 Poppy |
| rec_sisnoir | 1 | Sistermon Noir / Astamon / SaviorHuckmon | Sentinel, Cannoneer | stun | TFT5.5 Senna |
| rec_demidevimon | 1 | DemiDevimon / Devimon / SkullSatamon | Dark Spore, Ranger | execute | TFT5 Vayne |
| rec_dracmon | 1 | Dracmon / Sangloupmon / Matadormon | Nightfall, Renewer | drain | TFT5 Vladimir |
| rec_toyagumon | 1 | ToyAgumon / Starmon / Mamemon | Tricksters, Spellweaver | aoe | TFT5 Ziggs |
| rec_blackagumon | 2 | BlackAgumon / Meramon / SkullMeramon | Chimera, Spellweaver | chain | TFT5 Brand |
| rec_blackgabumon | 2 | BlackGabumon / BlackGarurmon / BlackMetalGarurumon | Dark Spore, Cavalier | drain | TFT5 Hecarim |
| rec_falcomon | 2 | Falcomon / Peckmon / Crowmon | Tricksters, Skirmisher | line | TFT5 Kennen |
| rec_impmon | 2 | Impmon / Wizardmon / Wisemon | Witchelny, Assassin | stun | TFT5 LeBlanc |
| rec_hagurumon | 2 | Hagurumon / Guardromon / Andromon | Chrome Digizoid, Knight | slam | TFT5 Nautilus |
| rec_kumamon | 2 | Kumamon / Frigimon / IceLeomon | Nightfall, Cavalier, Brawler | stun | TFT5.5 Sejuani |
| rec_monodramon | 2 | Monodramon / Strikedramon / Cyberdramon | Dragon Kin, Brawler | aoe | TFT5 Sett |
| rec_lalamon | 2 | Lalamon / Sunflowmon / Lilamon | Daybreak, Renewer | heal | TFT5 Soraka |
| rec_patamon | 2 | Patamon / Angemon / MagnaAngemon | Seraphic, Invoker | aoe | TFT5 Syndra |
| rec_tsukaimon | 2 | Tsukaimon / Bakemon / Phantomon | Dark Spore, Knight | hook | TFT5 Thresh |
| rec_goblimon | 2 | Goblimon / Ogremon / Titamon | Dragon Hunter, Skirmisher | weaken | TFT5 Trundle |
| rec_lunamon | 2 | Lunamon / Lekismon / Crescemon | Seraphic, Ranger | barrage | TFT5 Varus |
| rec_terriermon | 2 | Terriermon / Gargomon / Rapidmon | Tricksters, Cannoneer | nuke | TFT5.5 Tristana |
| rec_dracomon | 3 | Dracomon / Coredramon (Blue) / Wingdramon | Dragon Kin, Ranger | stun | TFT5 Ashe |
| rec_elecmon | 3 | Elecmon / Leomon / GrapLeomon | Nightfall, Skirmisher | slam | TFT5 Lee Sin |
| rec_floramon | 3 | Floramon / Togemon / Piximon | Tricksters, Mystic | rally | TFT5 Lulu |
| rec_salamon | 3 | Salamon / Gatomon / Angewomon | Seraphic, Mystic | line | TFT5 Lux |
| rec_gazimon | 3 | Gazimon / Cyclonemon / Beelzemon | Dark Spore, Cannoneer | barrage | TFT5.5 Miss Fortune |
| rec_blackgatomon | 3 | BlackGatomon / LadyDevimon / Lilithmon | Nightfall, Witchelny, Mystic | aoe | TFT5 Morgana |
| rec_kudamon | 3 | Kudamon / Reppamon / Chirinmon | Daybreak, Skirmisher | dash | TFT5 Nidalee |
| rec_kuwagamon | 3 | Kuwagamon / Okuwamon / GranKuwagamon | Phantom, Assassin | drain | TFT5 Nocturne |
| rec_tapirmon | 3 | Tapirmon / Monochromon / Megadramon | Chimera, Brawler | execute | TFT5 Nunu & Willump |
| rec_lopmon | 3 | Lopmon / Turuiemon / Antylamon | Dragon Hunter, Skirmisher | aoe | TFT5 Pantheon |
| rec_biyomon | 3 | Biyomon / Birdramon / Garudamon | Sentinel, Renewer | heal | TFT5.5 Rakan |
| rec_ryudamon | 3 | Ryudamon / Ginryumon / Hisyaryumon | Nightfall, Legionnaire | nuke | TFT5 Yasuo |
| rec_palmon | 3 | Palmon / Vegiemon / Lillymon | Dragon Kin, Spellweaver | stun | TFT5 Zyra |
| rec_gaomon | 4 | Gaomon / Gaogamon / MachGaogamon | Nightfall, Dragon Hunter, Assassin | slam | TFT5 Diana |
| rec_dorumon | 4 | Dorumon / Dorugamon / DoruGreymon | Dark Spore, Legionnaire | nuke | TFT5 Draven |
| rec_raremon | 4 | Raremon / Soulmon / Raguelmon | Chimera, Phantom, Mystic | drain | TFT5.5 Fiddlesticks |
| rec_coredramon | 4 | Coredramon (Green) / Groundramon / Breakdramon | Dragon Kin, Sentinel, Knight | slam | TFT5.5 Galio |
| rec_woodmon | 4 | Woodmon / Cherrymon / Puppetmon | Phantom, Invoker, Renewer | summon | TFT5 Ivern |
| rec_hackmon | 4 | Hackmon / BaoHuckmon / Jesmon | Chrome Digizoid, Skirmisher | execute | TFT5 Jax |
| rec_hawkmon | 4 | Hawkmon / Aquilamon / Silphymon | Daybreak, Invoker | aoe | TFT5 Karma |
| rec_rapidarmor | 4 | Commandramon / MetalMamemon / MegaGargomon | Sentinel, Cannoneer | line | TFT5.5 Lucian |
| rec_blackgreymon | 4 | BlackGreymon / BlackMetalGreymon / BlackWarGreymon | Dragon Hunter, Legionnaire | shield | TFT5 Mordekaiser |
| rec_veemon | 4 | Veemon / Magnamon / Magnamon X | Seraphic, Chrome Digizoid, Cavalier | rally | TFT5 Rell |
| rec_unimon | 4 | Unimon / HippoGryphonmon / Ophanimon | Seraphic, Spellweaver | line | TFT5 Vel'Koz |
| rec_valkyrimon | 5 | Valkyrimon / Kentaurosmon / Kentaurosmon X | Sentinel, Ranger | execute | TFT5.5 Akshan |
| rec_growlmon | 5 | Growlmon / WarGrowlmon / Megidramon | Nightfall, Royal Rival, Knight | drain | TFT5 Darius |
| rec_guilmon | 5 | Guilmon / Gallantmon / Gallantmon Crimson Mode | Daybreak, Royal Rival, Knight | aoe | TFT5 Garen |
| rec_sisciel | 5 | Kotemon / Sistermon Ciel (Awakened) / Durandamon | Hallowed Mist, Mystic | aoe | TFT5.5 Gwen |
| rec_clockmon | 5 | Clockmon / Datamon / Machinedramon | Dragon Kin, Renewer | summon | TFT5 Heimerdinger |
| rec_lucemon | 5 | Lucemon / Lucemon Chaos Mode / Lucemon Shadowlord Mode | Seraphic, Legionnaire | chain | TFT5 Kayle |
| rec_renamon | 5 | Renamon / Taomon / Sakuyamon | Twin Souls, Ranger, Mystic | respite | TFT5 Kindred |
| rec_mushroomon | 5 | Mushroomon / Vademon / Piedmon | Tricksters, Invoker | aoe | TFT5 Teemo |
| rec_myotismon | 5 | Myotismon / VenomMyotismon / GranDracmon | Dark Spore, Skirmisher, Assassin | possess | TFT5 Viego |
| rec_bearmon | 5 | Bearmon / SaberLeomon / BanchoLeomon | Phantom, Brawler | slam | TFT5 Volibear |

## Items

Components: Power Blade (+15% AD), Speed Wing (+12% AS), Data Wand (+15 AP), Mana Drop (+15 mana), Chrome Vest (+20 armor), Data Mantle (+20 MR), Life Belt (+150 HP), Lucky Gloves (+10% crit, +8% dodge), each with a Spore twin of the same stats, and the Digi-Egg. Two Digi-Eggs make Force of Nature (+1 team size). Consumables: Magnetic Remover, Tome of Traits.

| Recipe | Regular | Shadow | Radiant |
|---|---|---|---|
| Power Blade + Power Blade | Victory Sword: Deals 8% more damage. Each knockout gives +12% attack damage for the rest of combat. | Ruin Sword: Deals 15% more damage. Each knockout gives +20% attack damage. Drawback: 15% less max HP. | Luminous Sword: Deals 20% more damage. Each knockout gives +20% attack damage. |
| Power Blade + Speed Wing | Titan Slayer: Deals 8% more damage, and 35% more to enemies with 1600 or more max HP. | Spectral Slayer: Deals 50% more damage to enemies with 1100 or more max HP. Drawback: no bonus against smaller enemies. | Demon Slayer: Deals 30% more damage, and 40% more on top to enemies with 1600 or more max HP. |
| Power Blade + Data Wand | Data Gunblade: Heals for 22% of all damage dealt. | Mercy Gunblade: Magic damage the holder deals heals the ally with the lowest HP for 50% of it. Drawback: the holder heals nothing from it. | Lifeblade: Heals for 40% of all damage dealt. |
| Power Blade + Mana Drop | Mana Lance: Basic attacks restore 6 extra mana. | Spectral Lance: Basic attacks restore 14 extra mana. Drawback: deals 10% less damage. | Lance of Light: Basic attacks restore 12 extra mana. |
| Power Blade + Chrome Vest | Guardian Feather: The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with 30% max HP. | Fallen Feather: The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with 70% max HP. Drawback: after that it has 50% less attack speed. | Archangel Feather: The first time the holder would be knocked out, it is held safe for 2 seconds, then returns with full HP. |
| Power Blade + Data Mantle | Blood Fang: Heals for 25% of damage dealt. Below 40% HP, gains a 25% max HP shield once. | Risk Fang: Heals for 40% of damage dealt. Each time it heals back above 90% HP, it gains 40% attack speed for 4 seconds. Drawback: starts combat at 80% HP. | Blessed Fang: Heals for 50% of damage dealt. Below 40% HP, gains a 40% max HP shield once. |
| Power Blade + Life Belt | War Banner: At the start of combat, the holder and allies next to it in the same row gain 25% attack speed. | Bleak Banner: At the start of combat, allies next to the holder in the same row lose 20% attack speed, and the holder gains 35% attack speed for each of them. Drawback: slows those allies. | Harmony Banner: At the start of combat, the holder and allies next to it in the same row gain 45% attack speed and 10% omnivamp. |
| Power Blade + Lucky Gloves | Crit Edge: Grants extra critical strike chance and 10% crit damage. | Sacrifice Edge: Grants extra critical strike chance and 50% crit damage. Drawback: each basic attack costs the holder 2% of its max HP. | Zenith Edge: Grants extra critical strike chance and 50% crit damage. Abilities can critically strike. |
| Speed Wing + Speed Wing | Rapid Cannon: +1 attack range and extra attack speed. | Rapid Deathcannon: +1 attack range. Gains 75% attack speed while no enemy is within 2 hexes. Drawback: no flat attack speed bonus. | Light Cannon: +2 attack range and a lot of attack speed. |
| Speed Wing + Data Wand | Rage Wing: Each basic attack grants 6% attack speed, stacking. | Blood Rage Wing: Each basic attack grants 10% attack speed, stacking. Drawback: each basic attack costs the holder 1.5% of its max HP. | Reckoning Wing: Each basic attack grants 10% attack speed, stacking. |
| Speed Wing + Mana Drop | Volt Disk: Every 3rd basic attack sends lightning to 3 enemies for 80 magic damage. | Volt Stiletto: Every 2nd basic attack sends lightning to 4 enemies for 80 magic damage. Drawback: 30% less attack damage. | Volt Favor: Every 3rd basic attack sends lightning to 3 enemies for 160 magic damage. |
| Speed Wing + Chrome Vest | Titan Will: Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times. | Titan Revenge: Stores 60% of the damage the holder takes. Every 2 seconds its next basic attack releases the store as true damage. Drawback: no stacking stats. | Titan Vow: Attacking or being attacked grants 2% attack damage and 2 ability power, up to 25 times. |
| Speed Wing + Data Mantle | Twin Shot: Basic attacks also hit a second nearby enemy for 55% damage. | Wild Twin Shot: Basic attacks also hit 2 more nearby enemies for 70% damage. Drawback: 10% less attack damage. | Twin Tempest: Basic attacks also hit a second nearby enemy for full damage. |
| Speed Wing + Life Belt | Void Portal: At the start of combat, enemies within 2 hexes target the holder. When the holder is knocked out, a Voidlarva rises in its place. | Unstable Portal: Like Void Portal, but the Voidlarva deals 250 true damage to enemies next to it when it falls. Drawback: 10% less max HP. | Void Invitation: At the start of combat, a Voidlarva rises next to the holder and enemies within 3 hexes target it. When the holder is knocked out, another rises. |
| Speed Wing + Lucky Gloves | Armor Breaker: Basic attacks cut the target's armor by 40% for 3 seconds. | Final Whisper: Critical strikes cut the target's armor and magic resist by 70% for 5 seconds. Drawback: the holder has 20% less armor and magic resist. | Eternal Whisper: Basic attacks cut the target's armor by 70% for 3 seconds. |
| Data Wand + Data Wand | Ultima Wand: A lot of ability power. | Caustic Wand: A huge amount of ability power. Drawback: 15% less max HP. | Ascended Wand: An enormous amount of ability power. |
| Data Wand + Mana Drop | Seraph Staff: Each cast grants ability power equal to 30% of the holder's max mana. | Undying Staff: Each cast grants max HP equal to 3.5 times the holder's max mana and heals that much. Drawback: no ability power gain. | Holy Staff: Each cast grants ability power equal to 60% of the holder's max mana. |
| Data Wand + Chrome Vest | Iron Locket: At the start of combat, the holder and allies within 2 hexes in the same row gain a 250 shield (350 Champion form, 450 final form). | Lunar Locket: At the start of combat, the holder and allies within 2 hexes in the same row gain 60 armor and magic resist. Drawback: they gain 5 less mana per basic attack. | Prime Locket: At the start of combat, the holder and allies within 3 hexes in the same row gain a 450 shield (600 Champion form, 750 final form) and 200 max HP. |
| Data Wand + Data Mantle | Mana Zap: When an enemy within 2 hexes casts, it takes 180 magic damage. | Dark Spark: When any other Digimon within 2 hexes casts, it is zapped: enemies take twice their max mana as magic damage, allies half their max mana. The holder gains 10 AP per zap. Drawback: zaps allies too. | Covalent Spark: When an enemy within 2 hexes casts, it takes 400 magic damage. Regenerates 1% max HP per second. |
| Data Wand + Life Belt | Flame Tome: Damage burns the target for 1.5% max HP per second and cuts its healing by a third. | Evil Tome: Magic damage curses the target: 6% of its max HP as true damage per second for 8 seconds, and less healing. Drawback: the holder's other magic damage is 20% lower. | Radiant Tome: Damage burns the target for 1.5% max HP per second and cuts its healing. Deals 20% more damage. |
| Data Wand + Lucky Gloves | Jewel Gauntlet: Abilities can critically strike. +30% crit damage. | Sacrifice Gauntlet: Abilities can critically strike. +50% crit damage, and +25% crit chance after the first cast. Drawback: each cast costs HP equal to half the max mana times the form (1, 2, 3). | Glamor Gauntlet: Abilities can critically strike. +90% crit damage. |
| Mana Drop + Mana Drop | Mana Overflow: Max mana -10. After casting, keeps 20 mana. | Dark Overflow: After casting, keeps 50 mana while below 60% HP. Drawback: only 20 mana above 60% HP, and max mana is not lowered. | Blue Blessing: Max mana -10. After casting, keeps 30 mana. |
| Mana Drop + Chrome Vest | Frost Heart: Enemies within 2 hexes have 25% less attack speed. | Frozen Dark Heart: Enemies within 2 hexes have 35% less attack speed. Drawback: allies next to the holder have 25% less attack speed too. | Frost Heart of Gold: Enemies within 2 hexes have 50% less attack speed. |
| Mana Drop + Data Mantle | Power Chalice: At the start of combat, the holder and allies next to it in the same row gain 25 ability power. | Malice Chalice: At the start of combat, allies within 2 hexes in the same row gain 45 ability power. Drawback: the holder loses 20 ability power. | Charity Chalice: At the start of combat, the holder and allies next to it in the same row gain 50 ability power and 10% omnivamp. |
| Mana Drop + Life Belt | Holy Ring: Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP. | Sacrifice Ring: Every 5 seconds, heals every other ally for 25% of its missing HP. Drawback: each time, the holder loses 5% of its current HP. | Radiant Ring: Every 5 seconds, heals allies within 2 hexes for 80 plus 10% of their missing HP. Regenerates 1% max HP per second. |
| Mana Drop + Lucky Gloves | Justice Hand: Deals 12% more damage and heals for 12% of damage dealt. | Vengeance Hand: Gains 30% attack damage, 30 ability power and 20% omnivamp. Drawback: these switch off after each knockout the holder scores and come back on the next one. | Fairness Fist: Deals 20% more damage and heals for 20% of damage dealt. |
| Chrome Vest + Chrome Vest | Spike Plate: When attacked, deals 60 magic damage to adjacent enemies (1.5 second cooldown). | Refracted Plate: A huge amount of armor. Drawback: no spikes. | Rosethorn Plate: When attacked, deals 200 magic damage to adjacent enemies (1.5 second cooldown). Regenerates 2% max HP per second. |
| Chrome Vest + Data Mantle | Stone Hide: Gains 12 armor and magic resist for each enemy within 2 hexes. | Undying Stone: Every 2 seconds, heals 2% max HP for each enemy within 2 hexes. Drawback: no armor or magic resist from nearby enemies. | Dvarapala Stone: Gains 20 armor and magic resist for each enemy within 2 hexes. Regenerates 2% max HP per second. |
| Chrome Vest + Life Belt | Flame Core: Every 2 seconds, burns the nearest enemy within 2 hexes. | Eclipse Core: Every 1.5 seconds, burns a random enemy within 3 hexes. Drawback: the holder burns itself for the first 5 seconds. | Sunlight Core: Every 2 seconds, burns the nearest enemy within 2 hexes, and every 1.5 seconds a random enemy within 4 hexes. Regenerates 0.5% max HP per second. |
| Chrome Vest + Lucky Gloves | Still Shroud: At the start of combat, enemies in the holder's column need 35% more mana for their first cast. | Dark Shroud: At the start of combat, enemies in the holder's column need 80% more mana for their first cast. Drawback: allies in that column need 40% more. | Reverent Shroud: At the start of combat, enemies in the holder's column and the columns next to it need 50% more mana for their first cast. Allies start with 15 more mana. |
| Data Mantle + Data Mantle | Dragon Scale: A lot of magic resist. | Refracted Scale: Takes 20% less magic damage, and critical strikes deal no extra damage to the holder. Drawback: no extra magic resist. | Dragon Will: A huge amount of magic resist. Regenerates 2% max HP per second. |
| Data Mantle + Life Belt | Wind Ward: At the start of combat, the enemy facing the holder across the board is stunned for 4 seconds, even if immune. | Turbulent Ward: At the start of combat, the enemy facing the holder across the board is stunned for 7 seconds, even if immune. Drawback: it then gains 30% attack damage and 30 ability power. | Mistral Ward: At the start of combat, the enemy facing the holder across the board is stunned for 6 seconds, even if immune, and your team gains 10% attack speed. |
| Data Mantle + Lucky Gloves | Quick Silver: Immune to stuns for the first 12 seconds. | Caustic Silver: Immune to stuns for the whole combat. Drawback: 12% less max HP. | Quickest Silver: Immune to stuns for the first 30 seconds. |
| Life Belt + Life Belt | Giga Belt: A lot of max HP. | Sacrifice Belt: A huge amount of max HP. Drawback: loses 1.5% max HP every second. | Pride Belt: An enormous amount of max HP. Regenerates 1% max HP per second. |
| Life Belt + Lucky Gloves | Trap Claw: Blocks the first magic hit from an enemy and stuns the attacker for 3 seconds. | Banshee Claw: The holder and allies next to it in the same row each block the first magic hit from an enemy. Drawback: no stun. | Banshee Silence: Every ally blocks the first magic hit from an enemy, and the holder stuns that attacker for 3 seconds. |
| Lucky Gloves + Lucky Gloves | Thief Gloves: Each combat, gains two random completed items. | Trickster Gloves: Each combat, gains two random completed items, plus 25% attack damage and 25 ability power. Drawback: 10% less max HP. | Rascal Gloves: Each combat, gains two random completed items. |

| Augment | Tier | Effect |
|---|---|---|
| Bit Pouch | Silver | Gain 8 gold. |
| Spore Cache | Silver | Gain 2 random Spore components. |
| Quick Scan | Silver | Gain 1 free reroll every round. |
| Tamer's Resolve | Silver | Gain 4 Tamer HP. |
| Training Disk | Silver | Your team gains 10% attack damage and 10 ability power. |
| Black Mist | Silver | When one of your Digimon is knocked out, enemies within 2 hexes of it take 150 magic damage. |
| Iron Front | Silver | Digimon in your front row gain 25 armor, 25 magic resist and 150 HP. |
| Sniper Nest | Silver | Digimon in your back two rows gain 20% attack speed. |
| Digi-Bank | Gold | Gain 16 gold. |
| Shadow Trove | Gold | Gain a random Shadow item and a component. |
| Egg Hunt | Gold | Gain a Digi-Egg. |
| Steady Data | Gold | Gain 2 extra gold every round. |
| X-Antibody | Gold | The drawbacks of your Shadow items are halved, and your Digimon holding a Shadow item gain 150 HP. |
| Celestial Blessing | Gold | Your team heals for 12% of damage dealt. |
| Overcharge | Gold | Your team starts combat with 25 extra mana. |
| Stand United | Gold | Your team gains 3% attack damage and 3 ability power for each active trait. |
| Ascension | Gold | After 12 seconds of combat, your team deals 35% more damage. |
| Jackpot | Prismatic | Gain 30 gold. |
| Early Blessing | Prismatic | Receive your Radiant Blessing at the start of the next free round instead of waiting until you drop low. |
| Big Armory | Prismatic | Gain 2 random completed items. |
| Force of Nature | Prismatic | Gain a Force of Nature (+1 team size). |
| Force of Darkness | Prismatic | Gain +2 team size. Drawback: every loss costs you twice the usual Tamer HP. |
| Giant Grab | Prismatic | Your team gains 25% max HP. |
| Tome Collection | Prismatic | Gain 2 Tomes of Traits and a Digi-Egg. |

| Crest | From |
|---|---|
| Skirmisher Crest | Digi-Egg + Power Blade |
| Legionnaire Crest | Digi-Egg + Speed Wing |
| Spellweaver Crest | Digi-Egg + Data Wand |
| Renewer Crest | Digi-Egg + Mana Drop |
| Chrome Digizoid Crest | Digi-Egg + Chrome Vest |
| Seraphic Crest | Digi-Egg + Data Mantle |
| Daybreak Crest | Digi-Egg + Life Belt |
| Assassin Crest | Digi-Egg + Lucky Gloves |
| Dark Spore Crest | Digi-Egg + Spore Blade |
| Tricksters Crest | Digi-Egg + Spore Wing |
| Dragon Hunter Crest | Digi-Egg + Spore Wand |
| Witchelny Crest | Digi-Egg + Spore Drop |
| Cavalier Crest | Digi-Egg + Spore Vest |
| Phantom Crest | Digi-Egg + Spore Mantle |
| Nightfall Crest | Digi-Egg + Spore Belt |
| Chimera Crest | Digi-Egg + Spore Gloves |
| Sentinel Crest | no recipe (Tome of Traits, Armory, loot) |
| Dragon Kin Crest | no recipe (Tome of Traits, Armory, loot) |
| Knight Crest | no recipe (Tome of Traits, Armory, loot) |
| Mystic Crest | no recipe (Tome of Traits, Armory, loot) |
| Ranger Crest | no recipe (Tome of Traits, Armory, loot) |
| Brawler Crest | no recipe (Tome of Traits, Armory, loot) |
| Invoker Crest | no recipe (Tome of Traits, Armory, loot) |
| Cannoneer Crest | no recipe (Tome of Traits, Armory, loot) |

## Balance results

Measured with `tools/balance.cjs` and two extra scripts (mirror-board item duels, and full games with and without the Blessing). Final numbers:

**Duels** (`--duel 5000 8 1`, pace-independent): every trait's win rate is between 47.9% (Cannoneer) and 56.6% (Phantom). Lowest and highest of the rest: Chrome Digizoid 48.3%, Royal Rival 49.5%, Hallowed Mist 50.4%; Twin Souls 56.4%, Ranger 56.4%, Dragon Hunter 56.0%. Lines range from 45.0% (Gallantmon line) to 56.4% (Sakuyamon line).

**Full games, Hyper Roll** (300 games): top-4 rate by cost, lowest to highest line: 1-cost 36-60%, 2-cost 39-53%, 3-cost 47-59%, 4-cost 60-79%, 5-cost 63-85% (5-costs appear on few boards). Games end at stage 6.85 after 24.3 combat rounds. Wild rounds won: stage 1 100%, 2 92%, 3 95%, 4 93%, 5 67%, 6 64%, 7 83% (82 fights).

**Full games, Normal** (300 games): 1-cost 31-51%, 2-cost 36-57%, 3-cost 48-58%, 4-cost 56-73%, 5-cost 48-73%. Games end at stage 6.6 after 39.4 combat rounds. Wild rounds won: stage 1 94%, 2 96%, 3 93%, 4 92%, 5 70%, 6 83%.

**Shadow and Radiant items** (mirror boards that differ in one item, 60 fights per variant): a Shadow item beats its regular item 55.3% of the time on average, a Radiant item 73.4%. Single Shadow items range from 47% to 69% and Radiant items from 57% to 85% (about 6 points of noise per item at this sample size).

**Radiant Blessing** (80 Hyper Roll games each way): with the Blessing, 49 of 80 winners had dropped to 8 HP at some point; without it, 36 of 80. Players who dropped to 8 HP average place 4.68 with it and 4.76 without. Games get 0.4 combat rounds longer. It is a real comeback tool but does not decide games.

**Tuning notes**: summoned partners (Monzaemon, Gumdramon, Garurumon) scale with the caster's cost and form and use 55-75% HP; they were the strongest thing in the set at full stats. Sentinel's shield renews at most once per duration and its attack speed sits on the current holder only; the first version gave permanent stacking attack speed and healed the team every 1.5 seconds. Barrage abilities on 5-costs spread damage too thin, so the Akshan and Lucian lines use focused abilities. Wild Digimon get HP and AD multipliers by stage (`rules.pveMul`, one table per pace).
