# Into the Arcane (`arcane`)

TFT Set 13 "Into the Arcane" on Digimon. Corpus: `docs/inspo/tft/set-13-into-the-arcane.md`.

Files: `js/sets/arcane.js` (data), `arcane-mech.js` (hooks, Anomalies, black market, workshop, portals, ability types), `arcane-art.js` (portraits, move looks, basic attacks), `arcane-models.js` (model keys).

## Overview

- 60 shop lines, 13/13/13/11/10 by cost. TFT13 had 14/13/13/12/8; Steb moved to 2-cost, Zeri to 3, Scar to 4, Ekko and Heimerdinger to 5 to get the requested spread.
- 26 traits: 15 factions (including the one-unit High Roller and Junk King) and 11 classes. TFT13's Form Swapper is the faction Mode Change.
- 14 wild Digimon and summon lines (PvE camps, the Dark Area Belphemon, Anomaly summons).
- Every form uses a model that is already converted (`node tests/models.cjs arcane`: 222 forms, 0 gaps). Kunemon is the only Masters rip; the rest are Cyber Sleuth family rips reused from Faction Wars, Dragonlands and Auto Chess keys. Forms with no rip were swapped for a Digimon with one in the same role (Tankdramon to Darkdramon, Flaremon to Apollomon, Dinohumon to Gladimon, Pandamon to Mammothmon, and so on).
- Both paces work. Hyper Roll is the native pace (flat `rules`); standard pace uses the engine's Normal rules plus `rules.normal`, which moves the Anomaly to 4-6 as in TFT13.

## Set mechanic: Anomalies

Once per game (4-3 in Hyper Roll, 4-6 in standard pace) each Tamer picks 1 of 3 Anomalies. It goes to the Digimon on the center hex (column 4, second row); if that hex is empty, the strongest Digimon gets it. AI Tamers give it to their main carry. Rerolls cost 1 gold each (up to 6), and the offer cycles through the whole pool before repeating, as in TFT. The Anomaly is stored in the unit's `mods`, so it survives digivolving (the mechanic's `onCombine` copies it when the Anomaly unit was not the first of the three copies) and is lost when the unit is sold. A label shows it at the start of each combat, and the unit tooltip names it (`unitNote`).

27 Anomalies in three themes plus two utility picks:

| Theme | Anomalies |
|---|---|
| X-Antibody (raw mutation) | Power (55% more AD), Guard (60% more armor/MR), Mind (40% more AP), Bulwark, Berserk, Giant, Velocity, Ascension |
| DNA Digivolution (power from allies) | Devour, Share, Absorb Power, Absorb Energy, Nothing Wasted, Partners (two Garurumon), Spare Body (ShellNumemon on death), Guardian Link |
| Digital Hazard (dangerous code) | Fireball, Laser Eyes, Slime Time, Meltdown, Finisher, Thousand Cuts, Dragon Soul, Last Chance, Thornskin |
| Utility | Crest Copy, Dual Wielding |

The Double Mutation prismatic augment gives a second Anomaly the round after the first.

## Other signature systems

- **Kowloon Syndicate black market** (Chem-Baron). With 3+ Syndicate Digimon, each player combat gives Shimmer (more on a loss streak of 3+; standard pace uses about 55% of the Hyper Roll amounts, TFT13's own numbers). Every 100 Shimmer opens the black market at the start of a round: one of 3 contraband items (tier II from the third market) or Pass, which gives Syndicate Digimon permanent max HP. 16 contraband items (8 kinds x 2 tiers) only work on Syndicate Digimon. The trait also gives HP and omnivamp so it works without items.
- **Junk King workshop** (Rumble). Every 2 rounds (3 in standard pace, TFT13's own values) while you own Chaosdromon, pick a mech upgrade: Flamethrower, Repairing Microbots, Petricite Rod, Self Destruct Button, Tankbuster, or stacking Spare Parts. All five give a finishing bonus.
- **Opening portals** (TFT13 encounters). One of 10 portals is rolled for the lobby at the start: Gold Opener, Component Anvils, Crest Tag, Spare Digivice, Data Copier, Upgraded Partner, Golden Prelude, Prismatic Finale, Free Rolls, No Portal. The codex Rules tab shows the current one.
- **Witchelny Academy sponsors** 3 random completed items each game (shown in the codex and the trait notes).
- **Bagra Army war chests**: takedowns give Conquest; every 10 opens a chest (gold and a component, every 3rd a completed item) and adds 6% AD and AP to the trait.

## Traits

| Trait | TFT13 | Breakpoints | Effect |
|---|---|---|---|
| Witchelny Academy | Academy | 3/4/5 | HP and damage per sponsored item copy (x2 on Academy), Academy +12/17/24% HP and damage; a sponsored item at each new breakpoint |
| Clockwork | Automata | 2/4 | +25/55 armor and MR; every 12 hits, a blast for 180/420 + 25% of damage dealt |
| Dark Area | Black Rose | 3/4/5 | summon a chained Belphemon (freed after 5 casts or below 65% HP); 15% more damage at 4; heals and erupts when freed at 5 |
| Kowloon Syndicate | Chem-Baron | 3/4/5/6 | +6/10/14/20% HP, 8/12/15/20% omnivamp, Shimmer and the black market |
| Bagra Army | Conqueror | 2/4/6 | +15/24/34% AD and AP, 1x/3x/6x Conquest, war chests |
| Sovereign Envoys | Emissary | exactly 1 or 4 | the lone Envoy's bonus (Terriermon AS, Syakomon mana, Ryudamon armor, Hackmon HP); all 4: every bonus plus 250 HP and 20% damage |
| DigiPolice | Enforcer | 2/4/6 | 12/18/25% HP shield, 12/20/32% more damage; 1/2/4 Wanted enemies, a knockout gives 30% AS |
| Chimera Lab | Experiment | 3/5 | the 2/3 strongest get +100/350 HP and every Lab bonus |
| Found Family | Family | 2/3 | 25/35% less max mana, 12% less damage taken, +20% AS at 3 |
| Firelight | Firelight | 2/3 | every 6s heal 30/45% of damage taken since the last surge; AS burst at 3 |
| Mode Change | Form Swapper | 2/3/4 | front rows take 8/14/22% less damage, back rows deal 12/20/30% more; the row-based form is innate |
| Digimon Rebels | Rebel | 3/5/7 | +7/12/16% HP; at 25% team HP lost, a flare: 60% AS for 5s and +10/22/32% AD and AP; stun at 7 |
| Junkyard | Scrap | 2/4/6 | 150/280/450 shield + 20/30/40 per component held by the team; 1/3/all held components act as full items |
| High Roller | High Roller | 1 | a bonus roll on every BlackWarGreymon cast |
| Junk King | Junk King | 1 | the workshop |
| Ambusher | Ambusher | 2/3/4/5 | abilities crit, +25-55% crit, +15-35% crit damage |
| Cannoneer | Artillerist | 2/4 | +22/50% AD, a shell every 4th attack |
| Bruiser | Bruiser | 2/4/6 | team +80 HP, Bruisers +10/22/40% HP |
| Dominator | Dominator | 2/4/6 | 300/480/700 shield, AP from mana spent |
| Pit Fighter | Pit Fighter | 2/4/6 | 10% omnivamp, +4/8/14% true damage, a heal at 50% HP |
| Quickstriker | Quickstriker | 2/3/4 | 25-45% to 65-105% AS by the target's missing HP |
| Sentinel | Sentinel | 2/4/6 | team +5/10/18 armor and MR, Sentinels triple |
| Sniper | Sniper | 2/4 | 8/22% more damage, +5/8% per hex |
| Sorcerer | Sorcerer | 2/4/6 | team +8 AP, Sorcerers +15/35/60 AP; weaken at 6 |
| Visionary | Visionary | 2/4/6 | 35/65/100% more mana; ability damage heals at 6 |
| Watcher | Watcher | 2/4/6 | 10/18/28% less damage, 20/30/40% above 50% HP |

## Lines

| Cost | Line (forms) | Traits | TFT13 source |
|---|---|---|---|
| 1 | Kokuwamon, Guardromon, Andromon | Clockwork, Watcher | Amumu |
| 1 | Goblimon, Ogremon, Grademon | Bagra Army, Watcher | Darius |
| 1 | Impmon, Meramon, SkullMeramon | Bagra Army, Pit Fighter | Draven |
| 1 | Zubamon, ZubaEagermon, Duramon | Rebels, Sentinel | Irelia |
| 1 | Lalamon, Sunflowmon, Lilamon | Academy, Sorcerer | Lux |
| 1 | Commandramon, Sealsdramon, Darkdramon | DigiPolice, Sniper | Maddie |
| 1 | Tsukaimon, Bakemon, Phantomon | Dark Area, Visionary | Morgana |
| 1 | Chuumon, Mamemon, MetalMamemon | Family, Junkyard, Ambusher | Powder |
| 1 | Mushroomon, RedVegiemon, Vademon | Syndicate, Sentinel | Singed |
| 1 | SnowGoblimon, Hyogamon, IceLeomon | Junkyard, Bruiser | Trundle |
| 1 | Psychemon, Gururumon, WereGarurumon | Rebels, Visionary | Vex |
| 1 | Gaomon, Gaogamon, MachGaogamon | Family, Pit Fighter | Violet |
| 1 | Aruraumon, Woodmon, Cherrymon | Chimera Lab, Sorcerer | Zyra |
| 2 | Hyokomon, Peckmon, Crowmon | Rebels, Quickstriker | Akali |
| 2 | Lopmon, Turuiemon, Antylamon | DigiPolice, Ambusher | Camille |
| 2 | Coronamon, Firamon, Apollomon | Academy, Sentinel | Leona |
| 2 | Monodramon, Strikedramon, Cyberdramon | Clockwork, Quickstriker | Nocturne |
| 2 | Kotemon, Gladimon, Knightmon | Bagra Army, Sentinel, Visionary | Rell |
| 2 | Floramon, Hudiemon, Lillymon | Syndicate, Visionary | Renata Glasc |
| 2 | Jazamon, Jazardmon, Jazarichmon | Rebels, Bruiser, Pit Fighter | Sett |
| 2 | Terriermon, Gargomon, Rapidmon | Envoys, Cannoneer | Tristana |
| 2 | Crabmon, Coelamon, Neptunemon | Chimera Lab, Pit Fighter, Cannoneer | Urgot |
| 2 | Elecmon, Leomon, SaberLeomon | Family, Watcher | Vander |
| 2 | Dracmon, Sangloupmon, Matadormon | Dark Area, Watcher, Sorcerer | Vladimir |
| 2 | Kamemon, Tankmon, Gigadramon | Junkyard, Dominator | Ziggs |
| 2 | Gotsumon, Golemon, Triceramon | DigiPolice, Bruiser | Steb (1-cost in TFT) |
| 3 | Hagurumon, Clockmon, Datamon | Clockwork, Dominator | Blitzcrank |
| 3 | Betamon, Seadramon, MegaSeadramon | Dark Area, Dominator | Cassiopeia |
| 3 | Shamanmon, Sorcermon, Wisemon | Academy, Rebels, Cannoneer | Ezreal |
| 3 | Gomamon, Ikkakumon, Vikemon | Junkyard, Mode Change, Pit Fighter | Gangplank |
| 3 | Keramon, Chrysalimon, Infermon | Clockwork, Sniper | Kog'Maw |
| 3 | Armadillomon, Ankylomon, Shakkoumon | DigiPolice, Sentinel | Loris |
| 3 | Syakomon, Whamon, MarineAngemon | Envoys, Sorcerer | Nami |
| 3 | Penguinmon, Frigimon, Mammothmon | Chimera Lab, Bruiser, Visionary | Nunu & Willump |
| 3 | Numemon, Raremon, BlackKingNumemon | Syndicate, Bruiser | Renni |
| 3 | Gazimon, Dobermon, Astamon | Syndicate, Ambusher | Smeech |
| 3 | Saberdramon, Ravemon, Valkyrimon | Bagra Army, Mode Change, Sorcerer | Swain |
| 3 | Renamon, Kyubimon, Taomon | DigiPolice, Quickstriker | Twisted Fate |
| 3 | Tentomon, Kabuterimon, MegaKabuterimon | Firelight, Sniper | Zeri (2-cost in TFT) |
| 4 | Ryudamon, Ginryumon, Hisyaryumon | Envoys, Bagra Army, Quickstriker | Ambessa |
| 4 | Hawkmon, Aquilamon, Silphymon | Junkyard, Cannoneer | Corki |
| 4 | Tapirmon, Monochromon, Kimeramon | Chimera Lab, Dominator | Dr. Mundo |
| 4 | Kunemon, DoKunemon, Arukenimon | Dark Area, Mode Change, Bruiser | Elise |
| 4 | Hackmon, BaoHuckmon, SaviorHuckmon | Envoys, Watcher | Garen |
| 4 | Gizamon, Gesomon, Dragomon | Rebels, Sentinel | Illaoi |
| 4 | DemiDevimon, Devimon, Myotismon | Syndicate, Dominator | Silco |
| 4 | FanBeemon, Waspmon, CannonBeemon | Chimera Lab, Sniper | Twitch |
| 4 | Bearmon, Grizzlymon, GrapLeomon | DigiPolice, Pit Fighter | Vi |
| 4 | Lunamon, Lekismon, Crescemon | Rebels, Sorcerer | Zoe |
| 4 | Biyomon, Birdramon, Garudamon | Firelight, Watcher | Scar (3-cost in TFT) |
| 5 | Kentaurosmon, Kentaurosmon X | DigiPolice, Sniper | Caitlyn |
| 5 | Gallantmon, Gallantmon Crimson Mode | Academy, Mode Change | Jayce |
| 5 | Beelzemon, Beelzemon X, Beelzemon Blast Mode | Rebels, Ambusher | Jinx |
| 5 | Piedmon | Dark Area, Sorcerer | LeBlanc |
| 5 | Diaboromon, Armageddemon | Clockwork, Visionary | Malzahar |
| 5 | Craniamon, Craniamon X | Bagra Army, Dominator | Mordekaiser |
| 5 | Chaosdromon, Breakdramon | Junk King, Junkyard, Sentinel | Rumble |
| 5 | BlackMetalGreymon, BlackWarGreymon | High Roller, Syndicate, Pit Fighter | Sevika |
| 5 | Millenniummon | Firelight, Junkyard, Ambusher | Ekko (4-cost in TFT) |
| 5 | Witchmon | Academy, Visionary | Heimerdinger (4-cost in TFT) |

Set ability types: `arc_swap` (Mode Change: guard in front, blast from the back), `arc_rotate` (Jinx: line stun, three blasts, board-wide rocket), `arc_gamble` (Sevika: one of three moves, sometimes all), `arc_progress` (Heimerdinger: one more missile each cast).

## Items

- 10 components: 8 stat plugins plus the Crest Tag (TFT Spatula: faction Crests) and the Digivice (Frying Pan: class Crests).
- 36 completed items (every stat-component pair), mapped to TFT13's items on the engine passive library plus a few hooks (Durandal ally heal, Shadow Mantle untargetable, Mode Helm by row).
- 21 Crests: 16 with recipes, 5 drop-only (Academy, Dominator, Junkyard, Sniper, Watcher), as in TFT13.
- 3 Tamer items (Crown, Shield, Cape): +1 team size and a 10% gold chance.
- 12 artifacts (TFT Ornn artifacts): the Armory's first slot from stage 5, stage 5+ wild round loot, the Artifact Forge and Master Workshop augments.
- 16 contraband items (black market only, Syndicate only).
- 4 consumables: Item Remover, Item Reforger, Data Copier, Small Data Copier. AI Tamers use the copiers and equip contraband.
- 30 augments (10 Silver, 12 Gold, 8 Prismatic) with econ / items / combat AI tags, including set ones (Syndicate Ties, Golden Find, Double Mutation, Black Market Pass).

## Economy and rounds

- Hyper Roll (native): the engine's Hyper Roll rules (20 HP, levels by stage, fixed income, 1-gold rerolls, Armory at x-1 of stages 2-6, augments at 2-2/3-2/4-2) plus the Anomaly at 4-3.
- Standard pace: the engine's Normal rules (100 HP, XP, interest and streaks, 2-gold rerolls, no Armory, augments at 2-1/3-2/4-2) plus the Anomaly at 4-6. Normal plays three smaller stage-1 wild rounds; its x-7 wild rounds add a unit in stages 2-3 and use the next stage's wave from stage 4, since they come at the end of the stage.

## Balance

Tools: `node tools/balance.cjs --duel 5000 8 1 --set arcane`, `node tools/balance.cjs 300 8 --set arcane`, and for both paces and the Anomalies a script that plays full games with `new DT.Game({ pace })` and the AI picking Anomalies at random. Results:

**Duel (5000 fights, stage 1; combat only, so it holds for both paces).** Every trait is between 48.7% and 58.7%: Dark Area 58.7, High Roller 57.4, Envoys 57.3, Sorcerer 57.3, Watcher 56.5, Chimera Lab 56.5, Academy 56.3, Rebels 55.8, Clockwork 55.5, Sentinel 55.2, Firelight 55.1, Bagra Army 53.7, DigiPolice 53.5, Bruiser 53.3, Mode Change 53.2, Family 53.2, Quickstriker 53.0, Pit Fighter 52.9, Dominator 52.5, Syndicate 52.4, Cannoneer 52.4, Visionary 51.9, Ambusher 51.8, Junk King 50.5, Junkyard 50.2, Sniper 48.7. (Dark Area at 4-5 units is about 63% but appears on few boards; n=381 for the trait.)

**Hyper Roll, 300 full games.** Games end at 6-3 to 7-3 (some 8-1/8-2). Top-4 on the final board by cost: 4-costs 63-82%, 3-costs 52-69%, 2-costs 33-55%, 1-costs 25-59% (small samples), 5-costs 56-70% (rare, as in Faction Wars). No line sits outside its cost band; the highest are Tapirmon 82% and DemiDevimon 80% (4-cost). Traits on final boards: Firelight 82% (n=61) and Sniper 77% lead, Visionary 41% is lowest. Wild rounds won by stage: 95 / 90 / 93 / 90 / 84 / 55 / 65%.

**Standard pace, 300 full games.** Games end at 5-3 to 6-1 (the engine's Normal length; Faction Wars ends at the same rounds). Top-4 by cost: 4-costs 45-68%, 3-costs 53-68%, 2-costs 39-65%, 1-costs 17-41%. Traits: Firelight 77% (n=57), Clockwork 71%, Sniper 69% lead, Visionary 46% is lowest. Wild rounds won: 94 / 95 / 93 / 64 / 41%.

**Anomalies.** Hyper Roll, 400 games with the AI picking at random (n about 100-145 each): average place 4.21 (X-Antibody: Mind) to 5.03 (Thornskin), top-4 41-57%. Standard pace (AI picks): 2.95 (Dual Wielding) to 4.18 (X-Antibody: Ascension) among Tamers who got one. No Anomaly stands out beyond the sample noise (about ±0.25 average place).
