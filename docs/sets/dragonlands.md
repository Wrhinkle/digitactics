# Dragonlands

Set id `dragonlands`. Adapts TFT Set 7 Dragonlands and Set 7.5 Uncharted Realms (corpus: `docs/inspo/tft/set-07-dragonlands.md`, `docs/inspo/tft/set-07-5-uncharted-realms.md`; earlier plan: `docs/sets/set-02-dragon-realm.md`).

Files: `js/sets/dragonlands.js` (data), `js/sets/dragonlands-mech.js` (hooks, Copycat ability, mechanic, wild-round rewards), `js/sets/dragonlands-art.js` (item, Crest and augment portraits, move looks, basic attacks).

## Overview

- 60 shop lines: 13 / 13 / 13 / 11 / 10 by cost. 10 of them are Dragons (one 3-cost, five 4-cost, four 5-cost).
- 27 traits: 10 origins, 12 classes, the Dragon trait and 4 one-unit traits.
- Items: 8 components plus the Dragon Egg, 36 recipes, 22 Crests (8 craftable, 14 drop-only), Tamer's Crown, 8 Golden Armor items, 8 Shining items, 4 consumables.
- 34 augments (10 Silver, 18 Gold, 6 Prismatic).
- 13 wild Digimon for PvE plus 2 summons (stone statues, Void Larva).
- Every line uses a different Digimon (180 distinct forms) and is `unassigned: true`, so the engine draws placeholders until models are assigned. 178 of the 180 forms are in `docs/sets/digimon-db.json` with a Cyber Sleuth-family source; Gesomon and Ebonwumon are in the database without one.

## Set mechanic

**Dragons** (Set 7 / 7.5 Dragon units). Lines with `slots: 2` take two team slots. Each Dragon has a `mark` trait (the first trait listed on it) and counts as 3 for that trait: the mechanic's `onCountTraits` adds 2 per Dragon line on the board, so a second copy of the same Dragon adds nothing. Dragons cost more than their tier, as in 7.5 (6/7/8 there): 5 gold for the 3-cost Dragon, 6 for 4-cost and 7 for 5-cost, set per shop slot with `shopMeta[slot].cost` in `onShopRolled`. Selling refunds the price (the set's `sellValue` hook, so the sell value shown in the UI matches). The Dragon trait uses the 7.5 breakpoints: 1 Dragon gives Dragons +550 HP, 2 add +20 armor and MR, 3 add 10% damage. Normal pace (which reaches level 9-10) has two extra rules in `rules.normal`: the Dragon trait gives nothing with 3 or more Dragons (`drgTraitMax: 2`), and from level 8 each Dragon a shop rolls is swapped for a non-Dragon of the same cost 60% of the time (`drgShopCut`). Dragons are not unique (7.5 rule).

**Treasure Dragon** (Set 7 Raptor replacement, 7.5 Order and Chaos versions). Round 4-4 is a Treasure Dragon wave. One version is rolled for the lobby at game start: classic GoldVeedramon (40%), Order Magnadramon (30%, one bundle is always a Shining item) or Chaos Megidramon (30%, half the bundles are mystery bundles: a Shining item plus a completed item, or 4 gold). Losing the wave costs no HP. At 5-1 each player gets a `drg_treasure` offer: 4 bundles (3 after a loss) with one free reroll. Bundle ids describe their own contents (`gold:10`, `comp:sword.bow.rod`, `item:drg_crit_edge.bow`, `emblem:drg_emblem_star`, `dup:5`, `unit:drg_bakemon`, `rad:drg_r_fang`, `chaos:N`), so they replay the same on every client. The Treasure Hoard augment adds 2 bundles and 1 reroll.

**Fixed augments with one reroll** (12.11 change). Augments come at 2-1, 3-2 and 4-2 (`rules.offers`). Gold is the most common tier (`augmentTierOdds [0.25, 0.85]`: 25% Silver, 60% Gold, 15% Prismatic). Each player has one augment reroll for the whole game: at each augment round `onRoundStart` checks whether the last augment picked was outside the first options shown (meaning the reroll was spent) and sets the offer's rerolls to 0 or 1.

**Golden Armor** (Shimmerscale). At round start the mechanic removes golden items from the item bench and from benched Digimon (the TFT "removed when benched" rule), then tops the board up to 1 / 2 / 4 golden items for 3 / 5 / 7 Golden Armor. The items use the Set 7 Hyper Roll versions (gold counted up to 15, read from `player.mech.bank` at round start). Gold they earn in combat is stored on the combat's team scratch object and paid in `onRoundEnd` from `e.match.combat.teamFx[e.team]`, so ghost fights never pay the wrong player.

## Economy and rounds

Hyper Roll length, as the brief asks for our 8-player games: 20 Tamer HP, levels set by stage (3 to 9), fixed income 3/5/6/7/8/8/9 by stage, no interest or streaks, 2 gold per PvP win, rerolls 1 gold, loss damage 2/4/6 HP. Pool 30/25/18/10/9 per cost.

Rounds: 1-1 carousel (1 and 2-costs), 1-2 and 1-3 PvE, then x-1 to x-3 PvP and x-4 PvE. Augments at 2-1, 3-2, 4-2. Armory at 3-1, 4-1 and 6-1. Treasure Dragon at 4-4 with its pick at 5-1. Set 7's count of at least 5 components from early PvE holds: 1-2, 1-3, 2-4 and 3-4 drop 5 components on wins, plus the carousel and the 3-1 and 4-1 Armory. From stage 5 a won PvE round drops a completed item and sometimes a Crest for a trait on your board (12.11: 5-6 PvE drops active emblems).

## Traits

Breakpoints and values as shipped. Dragons count 3 for their marked trait.

| Trait | Kind | Adapts | Breakpoints | Effect |
|---|---|---|---|---|
| Stargazers | origin | Astral | 3/5/7 | Team +10/25/45 AP. Every third combat with the trait active pays a Star Orb (1/2/3 gold) and puts a Stargazer in the next shop's first slot. |
| X-Antibody | origin | Guild | 1-6 | Each X-Antibody line gives the team its bonus (Craniamon X +100 HP, Dynasmon X +8% AS, UlforceVeedramon X +8 armor/MR, Leopardmon X +8% AD, Kentaurosmon X +2 mana per attack, Crest holder +3% omnivamp), x1.0 to x2.0 by count, doubled on members. |
| Ancient Stones | origin | Jade | 3/5/7 | 1/2/3 stone statues rise next to your Digimon (HP grows by stage). Allies starting next to one heal 0.8/2/3.5% max HP every 2s and gain 8/20/35% AS. A destroyed statue deals 33% of its HP to adjacent enemies; statues crumble when the last real ally falls. |
| Data Mirage | origin | Mirage | 2/4/6 | One bonus per game: Dancer (dodge and AS), Warlord (AD and HP), Overload (adjacent magic damage every 2s) or Executioner (crit, abilities crit, finish below 10%). |
| Berserk | origin | Ragewing | 3/5/7 | +3 mana per attack. After casting, rage 4s: +45/75/115% AS and 10/20/35% omnivamp. |
| Dragon Slayers | origin | Scalescorn | 2/4 | 20% less damage from enemies with 1800+ max HP; with no Dragon on your board, +20/50% damage. |
| Golden Armor | origin | Shimmerscale | 3/5/7 | +20/40/60 armor and MR; 1/2/4 golden items each round (Golden Crown possible at 7). |
| Thunder | origin | Tempest | 2/4/6 | At 8s lightning stuns every enemy 1s and deals 8/20/35% max HP true damage; Thunder units gain 30/60/110% AS. |
| Nightmare Whispers | origin | Whispers | 2/4/6 | Damage shrinks enemies (-40% armor/MR, 6s); hits on shrunken enemies stack AD and AP (25 stacks). |
| Lagoon | origin | Lagoon (7.5) | 3/5/7 | +15/35/60 AP and 12/32/60% AS. Every 20 Lagoon casts across the game pay 1/2/3 gold; every third payout adds a component. |
| Assassin | class | Assassin | 2/4 | Leap to the backline; abilities crit; +25/45% crit, +15/40% crit damage, +8/15% damage. |
| Bruiser | class | Bruiser | 2/4/6 | Team +120/250/450 HP, double on Bruisers. |
| Cannoneer | class | Cannoneer | 2/3/4 | Every 4th attack: cannon for 120/200/280% AD + 60/120/200 to the target and adjacent enemies. |
| Rider | class | Cavalier | 2/3/4 | +30/50/75 armor and MR, doubled for the first 4s. |
| Dragon Tamer | class | Dragonmancer | 2/4/6 | The Tamer holding the most items becomes the Hero: +350/750/1250 HP and +30/55/85 AP (same share of AD), +10% per star level among Tamers. |
| Evoker | class | Evoker | 2/3/4 | Mana whenever any unit casts. |
| Guardian | class | Guardian | 2/4/6 | Once at 50% HP, shield self and closest ally for 20/35/55% of max HP. |
| Mage | class | Mage | 3/5/7 | Cast twice, at a reduced share of total AP. |
| Mystic | class | Mystic | 2/3/4 | Team magic resist. |
| Mode Change | class | Shapeshifter | 2/4 | First cast: +18/35% max HP (and heal it). |
| Swiftshot | class | Swiftshot | 2/4 | +1 range; +10/20% AS per hex to the target. |
| Warrior | class | Warrior | 2/4/6 | 50% chance after an attack to power up the next attack by 80/175/300%. |
| Dragon | class | Dragon | 1/2/3 | See the mechanic. |
| Wishing Star | unique | Starcaller | 1 | First cast in PvP heals your Tamer 1 HP (2 at final form); at full HP it deletes a random enemy instead. |
| Copycat | unique | Spell Thief | 1 | Millenniummon's line casts a random ability each time. |
| Digi-Core | unique | Bard | 1 | Casts and surviving allies drop Digi-Cores; each moves 1% of shop odds to the top costs (cap 20). |
| Monolith | unique | Monolith (7.5) | 1 | Units starting on 3 fixed hexes take 20% less damage. |

Not adapted: Darkflight, Legend, Revel, Trainer, Prodigy (their sacrifice, summon and Snax systems need engine features or were replaced at mid-set), Spatula-only traits.

## Lines

`src` on each line names the corpus champion. Forms are Rookie / middle / final; Dragons marked with their counted trait.

| Cost | Line (forms) | Traits | Adapts |
|---|---|---|---|
| 1 | Gazimon / Saberdramon / Varodurumon | Thunder, Swiftshot | Ezreal |
| 1 | Gotsumon / Icemon / Meteormon | Ancient Stones, Dragon Tamer | Karma |
| 1 | Patamon / Unimon / Shakkoumon | Data Mirage, Guardian | Leona |
| 1 | Crabmon / Coelamon / Zudomon | Lagoon, Bruiser | Malphite |
| 1 | Kamemon / Guardromon (Gold) / Knightmon | Golden Armor, Guardian | Nasus |
| 1 | Lunamon / Sorcermon / Dianamon | Stargazers, Mode Change | Nidalee |
| 1 | Elecmon / Raptordramon / Craniamon X | X-Antibody, Rider | Sejuani |
| 1 | Impmon / Sistermon Noir / Beelzemon X | Berserk, Cannoneer | Senna |
| 1 | Gaomon / Golemon / BanchoLeomon | Berserk, Dragon Tamer | Sett |
| 1 | FanBeemon / Hudiemon / TigerVespamon | Stargazers, Bruiser | Skarner |
| 1 | Syakomon / ShellNumemon / Neptunemon | Lagoon, Mage | Taliyah |
| 1 | Dracmon / IceDevimon / Merukimon | Stargazers, Mage | Vladimir |
| 1 | Armadillomon / Monochromon / Triceramon | Ancient Stones, Warrior | Wukong |
| 2 | Gabumon / Gururumon / AncientGarurumon | Ancient Stones, Dragon Tamer, Swiftshot | Ashe (7) |
| 2 | Gomamon / Ikkakumon / Vikemon | Dragon Slayers, Guardian | Braum |
| 2 | BlackAgumon / BlackGreymon / AncientGreymon | Ancient Stones, Mode Change | Gnar |
| 2 | Chuumon / PlatinumSukamon / KingSukamon | Berserk, Golden Armor, Assassin | Kayn (7) |
| 2 | Betamon / Seadramon / MegaSeadramon | Lagoon, Dragon Tamer | Kai'Sa |
| 2 | Tsukaimon / Piddomon / Valkyrimon | Dragon Slayers, Rider, Mage | Lillia |
| 2 | Salamon / Gatomon / Angewomon | Stargazers, Mage | Lux |
| 2 | Biyomon / Airdramon / Ravemon | Thunder, Assassin | Qiyana |
| 2 | Monodramon / Cyclonemon / Gallantmon X | Berserk, Bruiser, Warrior | Shen (7) |
| 2 | Mushroomon / RedVegiemon / Dynasmon X | X-Antibody, Swiftshot | Twitch |
| 2 | Kotemon / Sistermon Ciel (Awakened) / Jesmon X | Data Mirage, Warrior | Yone |
| 2 | Otamamon / Gekomon / ShogunGekomon | Lagoon, Guardian | Zac |
| 2 | Aruraumon / Vegiemon / Cherrymon | Nightmare Whispers, Evoker | Zyra |
| 3 | Renamon / Kyubimon (Silver) / Kuzuhamon | Dragon Slayers, Assassin | Diana |
| 3 | Veemon / Flamedramon / WarGreymon X | Thunder, Dragon Tamer | Lee Sin |
| 3 | Lalamon / Sunflowmon / Lotosmon | Evoker, Mystic | Lulu (7) |
| 3 | SnowGoblimon / MudFrigimon / SaberLeomon | Data Mirage, Rider | Nunu & Willump |
| 3 | BlackGabumon / BlackGarurmon / BlackMetalGarurumon | Dragon Slayers, Bruiser, Warrior | Olaf |
| 3 | Hawkmon / Aquilamon / Silphymon | Berserk, Guardian, Mystic | Rakan |
| 3 | Penguinmon / Whamon / MarineAngemon | Lagoon, Evoker, Mystic | Seraphine |
| 3 | Psychemon / Meramon / SkullMeramon | Nightmare Whispers, Mage, Bruiser | Sylas |
| 3 | Kudamon / Youkomon / Sakuyamon | Stargazers, Swiftshot | Varus |
| 3 | Gumdramon / Arresterdramon / Magnamon X | Golden Armor, Dragon Tamer | Volibear |
| 3 | Kokuwamon / Tankmon / HiAndromon | Lagoon, Cannoneer | Zeri |
| 3 | ToyAgumon / Guardromon / Andromon | Cannoneer, Dragon Slayers | Tristana |
| 3 | **Dragon** Dorumon / RedVeedramon / UlforceVeedramon X | X-Antibody (x3), Dragon | Zippy (7.5) |
| 4 | **Dragon** Dracomon / Groundramon / Breakdramon | Data Mirage (x3), Dragon | Daeja |
| 4 | Tentomon / CatchMamemon / Justimon | Thunder, Cannoneer | Graves |
| 4 | Guilmon / Gallantmon / Gallantmon Crimson Mode | Berserk, Rider | Hecarim |
| 4 | **Dragon** Coronamon / Coredramon (Green) / Goldramon | Golden Armor (x3), Dragon, Guardian | Idas |
| 4 | Hackmon / SaviorHuckmon / Leopardmon X | X-Antibody, Mode Change | Jayce |
| 4 | Gizamon / Gesomon / Dragomon | Lagoon, Assassin | Nilah |
| 4 | Goblimon / Ogremon / Titamon | Nightmare Whispers, Warrior | Pantheon |
| 4 | **Dragon** Ryudamon / Duramon / Ouryumon | Ancient Stones (x3), Mystic, Dragon | Shi Oh Yu |
| 4 | **Dragon** Coredramon (Blue) / Megadramon / MetalSeadramon | Lagoon (x3), Mage, Dragon | Sohm (7.5) |
| 4 | **Dragon** Dorugamon / DoruGreymon / Dorugoramon | Nightmare Whispers (x3), Bruiser, Dragon | Sy'fen |
| 4 | Hyokomon / HippoGryphonmon / Gryphonmon | Berserk, Swiftshot | Xayah |
| 5 | **Dragon** Jazardmon / Jazarichmon / Metallicdramon | Thunder (x3), Dragon | Ao Shin |
| 5 | **Dragon** Growlmon (Yellow) / Wingdramon / Slayerdramon | Stargazers (x3), Evoker, Dragon | Aurelion Sol |
| 5 | BaoHuckmon / Kentaurosmon / Kentaurosmon X | X-Antibody, Mystic, Digi-Core | Bard |
| 5 | Bakemon / Phantomon / GranDracmon | Nightmare Whispers, Assassin | Pyke (7) |
| 5 | **Dragon** Greymon / BlackMetalGreymon / Volcanicdramon | Berserk (x3), Mode Change, Dragon | Shyvana |
| 5 | Leomon / GrapLeomon / Susanoomon | Ancient Stones, Wishing Star | Soraka |
| 5 | Zubamon / Grademon / Samudramon | Data Mirage, Dragon Tamer, Warrior | Yasuo |
| 5 | Clockmon / Datamon / Millenniummon | Golden Armor, Copycat, Mage | Zoe |
| 5 | **Dragon** Ankylomon / Lavogaritamon / Ebonwumon | Monolith, Dragon | Terra (7.5) |
| 5 | Volcamon / PileVolcamon / Vulcanusmon | Thunder, Bruiser | Ornn (7) |

Wild Digimon: Commandramon, Sealsdramon, Devidramon, Tyrannomon, Gigadramon, MetalTyrannomon, the three Treasure Dragons (GoldVeedramon, Magnadramon, Megidramon), Chaosdromon, BlackWarGreymon, Imperialdramon Fighter Mode and Azulongmon.

## Items

Components and recipes follow the TFT Set 7 table (Power Chip = B.F. Sword and so on). Most effects use the engine passive library; items that need hooks: Night Veil (Edge of Night), Herald Banner (Zeke's), Chalice of Power, Solar Locket, Banshee Veil, Rapid Cannon (+1 range), Justice Hand, Lure Portal (Zz'Rot: taunt, Void Larva on death), Frost Heart, Shroud of Stillness, Dragon Claw (regen per enemy targeting the holder, 20% more on Dragons) and Zephyr Gale.

- Crests: Dragon Egg + Power Chip / Speed Chip / Data Rod / Mana Card / Chrome Plate / Data Cloak / HP Disk / Lucky Glove = Golden Armor / Berserk / Dragon Tamer / Mage / Rider / Data Mirage / Guardian / Assassin (the Set 7 Spatula recipes). The other 14 origin and class Crests drop from loot, the Armory, the Treasure Dragon and Ancient Archives. Dragon and the one-unit traits have none. Two Dragon Eggs make a Tamer's Crown (+1 team size).
- Golden items: Treasure Axe, Gambler's Blade, Goldmancer's Staff, Mogul's Mail, Big Gem, Determined Investor (turns into Diamond Hands after 4 knockouts, with a Champion Duplicator and 8 gold), Golden Crown.
- Shining items (Radiant): Dragon Fang, Jewel Gauntlet, Seraph Rod, Spike Plate, Giga Belt, Blood Fang, Rage Blade, Holy Ring.
- Consumables: Magnetic Remover, Reforger, Lesser Duplicator (cost 3 or less), Champion Duplicator.
- AI tags: every completed, golden and Shining item has `tags` (`ad` / `ap` / `tank`) and recipes have `aiRank`. Dragons get `ai.value` 6 through `lineAi`.

## Augments

Silver: Small Hoard, Featherweights, Exiles, Battlemage, Best Friends, Calculated Loss, First Aid Kit, Combat Training, Lesser Duplicates, Scrap Heap. Gold: Ascension, Celestial Blessing, Cybernetic Implants / Shell / Uplink, Component Grab Bag, Ancient Archives, Dragon Imperialist, Axiom Arc, Big Friend, Double Trouble, Electrocharge, Treasure Hoard, Built Different, and Crest augments for Golden Armor, Berserk, Dragon Tamer and Mage (Crest plus a unit, like the Set 7 Crest augments). Prismatic: Golden Hoard, Shining Relics, Tamer's Crown, Wings of the Dragon, Dragon Pact, Final Ascension. All carry `econ` / `items` / `combat` tags for the AI.

## Art

`dragonlands-art.js` registers a painter for every item, Crest and augment (components, recipes, golden items on round gold frames, Shining items on prism hex frames, consumables on steel frames, Crests through `kit.crest` with each trait's color and symbol, augments on tiered hex frames). Every ability has a move spec, built from a per-line table of element plus one look per form (orb, breath, beam, bolt, slash, dash, erupt, meteor, volley, shock, dome, heal and others, scaled by form), and every line, wild unit and summon has a basic attack spec.

## Balance

Tuning knobs: trait values in `TRAITS`, per-line `hpMul` / `adMul` in the `TUNE` table at the end of the line list, Dragon prices, and wild-round strength. Final numbers (2026-10-15):

**Duel** (`node tools/balance.cjs --duel 5000 8 1 --set dragonlands`): every trait's win rate is within 46.4-58.2%.

| Trait | Duel win % | Trait | Duel win % |
|---|---|---|---|
| Mode Change | 58.2 | Data Mirage | 50.3 |
| Nightmare Whispers | 58.0 | Dragon Tamer | 50.3 |
| Ancient Stones | 57.7 | Swiftshot | 50.1 |
| Evoker | 56.8 | Rider | 49.7 |
| Mystic | 56.4 | Digi-Core | 49.6 |
| Bruiser | 54.8 | Lagoon | 49.6 |
| Guardian | 54.7 | Copycat | 49.3 |
| Berserk | 53.4 | Monolith | 49.1 |
| Dragon | 51.9 | Thunder | 48.9 |
| Stargazers | 51.5 | Dragon Slayers | 47.8 |
| Mage | 51.5 | Wishing Star | 47.0 |
| Golden Armor | 51.3 | Cannoneer | 46.4 |
| Warrior | 50.9 | | |
| X-Antibody | 50.7 | | |
| Assassin | 50.7 | | |

**Full games** (`node tools/balance.cjs 300 8 --set dragonlands`):

- Top-4 rate on the final board rises with cost, as expected: 1 and 2-costs mostly 33-57%, 3-costs 55-64%, 4-costs 49-82%, 5-costs 65-78%. Highest: Gizamon line (4) 82%, Tentomon line (4) 80%, Kentaurosmon X line (5) 78%. Lowest: Salamon line (2) 33%, Armadillomon line (1) 35%. 1-costs appear on few final boards (n about 30-50), so their rates are noisy.
- Traits on final boards: Digi-Core 78% (one 5-cost), Cannoneer 69% and Berserk 68% (they scale with items) at the top, Mage 35% at the bottom (it needs 3 Mages, mostly cheap lines).
- Wild rounds won: stage 1 97%, 2 91%, 3 89%, 4 (Treasure Dragon) 89%, 5 70%, 6 80%, 7 54%.

**Dragons and game length** (40 AI games, private stats script): Dragons are on 50% of final boards; those boards place top 4 55% of the time (average place 4.28), boards without a Dragon 45% (4.71). Boards with 2 or more Dragons (late-game boards) 69%. Games average 25 combats and end between 6-2 and 8-1 (most at 7-1 to 7-3); the first knockout comes at 4-2 to 5-3.

**Normal pace and 3-Dragon boards** (2026-10-16, `node tools/balance.cjs 400 8 --set dragonlands --pace normal`): before the Normal rules, boards with 3 or more Dragons placed top 4 74-80% of the time and won 28-33% (avg place 2.85), because Normal reaches level 9-10 where 3 Dragons fit beside 3-4 other Digimon. Mark counting (Dragons counting 3 for their trait) made no measurable difference, and one Dragon per shop or no 10% damage at 3 Dragons moved it only a few points. With `drgTraitMax: 2` and `drgShopCut` (level 8+, 60%): 3-Dragon boards top 4 54%, win 14%, avg place 4.50 (n=179 of 3200 boards); 2-Dragon boards 53%; the strongest other comps sit at 67-80% (X-Antibody 5, Golden Armor 5, the 5-costs). Games still last 36.4 combat rounds and end in stage 7. Hyper Roll keeps its rules; there 3-Dragon boards are rare (73 of 2400 final boards).

**Tests**: `tests/sim.js 20`, `tests/lockstep.js 3`, `tests/sets.cjs` (all with `--set dragonlands`) pass; `tests/moves.cjs <playwright> <out> soft --set dragonlands` passes with no generated looks (run in a temp copy on port 8175). A solo browser game (`DT.UI.newGame('dragonlands')`) played to 4-2 rendered the board, shop, traits, codex and combat with no page errors.

Notes:
- The duel builds boards without items, so item-scaling traits (Cannoneer, Assassin) sit low there and high in full games. Cannoneer and Assassin got flat parts (cannon flat damage, Assassin damage amp) to narrow that gap.
- AI Tamers never use the augment reroll; humans get one per game.
